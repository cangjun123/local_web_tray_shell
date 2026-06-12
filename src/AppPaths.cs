using System;
using System.IO;

namespace LocalWebTrayShell
{
    internal static class AppPaths
    {
        public static string LocalRootDirectory
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SwitchShell");
            }
        }

        public static string ConfigPath
        {
            get
            {
                return Path.Combine(LocalRootDirectory, "switch-config.json");
            }
        }

        public static string LegacySitesPath
        {
            get
            {
                return Path.Combine(LocalRootDirectory, "sites.json");
            }
        }

        public static string WebViewUserDataDirectory
        {
            get
            {
                return Path.Combine(LocalRootDirectory, "WebView2");
            }
        }
    }
}
