using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LocalWebTrayShell
{
    internal static class EmbeddedDependencyBootstrapper
    {
        private const string CoreResourceName =
            "LocalWebTrayShell.Resources.Microsoft.Web.WebView2.Core.dll";
        private const string WinFormsResourceName =
            "LocalWebTrayShell.Resources.Microsoft.Web.WebView2.WinForms.dll";
        private const string LoaderResourceName =
            "LocalWebTrayShell.Resources.WebView2Loader.dll";
        private static readonly Dictionary<string, string> ManagedResourceMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Microsoft.Web.WebView2.Core", CoreResourceName },
                { "Microsoft.Web.WebView2.WinForms", WinFormsResourceName }
            };
        private static readonly string NativeDirectory = Path.Combine(
            Path.GetTempPath(),
            "SwitchShell",
            "webview2",
            "x64");
        private static bool initialized;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        public static bool Initialize(bool interactive)
        {
            if (initialized)
            {
                return true;
            }

            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += ResolveEmbeddedAssembly;
                Directory.CreateDirectory(NativeDirectory);
                ExtractEmbeddedFile(LoaderResourceName, Path.Combine(NativeDirectory, "WebView2Loader.dll"));
                SetDllDirectory(NativeDirectory);
                initialized = true;
                return true;
            }
            catch (Exception ex)
            {
                if (interactive)
                {
                    MessageBox.Show(
                        "Unable to prepare the embedded WebView2 runtime.\r\n\r\n" + ex.Message,
                        "Switch",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }

                return false;
            }
        }

        public static bool RunSelfTest()
        {
            try
            {
                if (!Initialize(false))
                {
                    return false;
                }

                Type.GetType(
                    "Microsoft.Web.WebView2.WinForms.WebView2, Microsoft.Web.WebView2.WinForms",
                    true);

                return File.Exists(Path.Combine(NativeDirectory, "WebView2Loader.dll"));
            }
            catch
            {
                return false;
            }
        }

        private static Assembly ResolveEmbeddedAssembly(object sender, ResolveEventArgs args)
        {
            AssemblyName requestedName = new AssemblyName(args.Name);
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            int index;

            for (index = 0; index < loadedAssemblies.Length; index++)
            {
                if (string.Equals(
                    loadedAssemblies[index].GetName().Name,
                    requestedName.Name,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return loadedAssemblies[index];
                }
            }

            if (!ManagedResourceMap.ContainsKey(requestedName.Name))
            {
                return null;
            }

            return LoadAssemblyFromResource(ManagedResourceMap[requestedName.Name]);
        }

        private static Assembly LoadAssemblyFromResource(string resourceName)
        {
            byte[] bytes;

            using (Stream stream = GetRequiredResourceStream(resourceName))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }

            return Assembly.Load(bytes);
        }

        private static void ExtractEmbeddedFile(string resourceName, string outputPath)
        {
            using (Stream stream = GetRequiredResourceStream(resourceName))
            using (FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }

        private static Stream GetRequiredResourceStream(string resourceName)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                throw new InvalidOperationException("Missing embedded resource: " + resourceName);
            }

            return stream;
        }
    }
}
