using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace LocalWebTrayShell
{
    internal static class WindowsStartupManager
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string RunValueName = "SwitchShell";

        public static bool IsEnabled()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
            {
                string value = key == null ? null : key.GetValue(RunValueName) as string;
                return string.Equals(value, GetCommand(), StringComparison.OrdinalIgnoreCase);
            }
        }

        public static void SetEnabled(bool enabled)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKeyPath))
            {
                if (enabled)
                {
                    key.SetValue(RunValueName, GetCommand(), RegistryValueKind.String);
                }
                else if (key.GetValue(RunValueName) != null)
                {
                    key.DeleteValue(RunValueName, false);
                }
            }
        }

        public static string GetCommand()
        {
            return "\"" + Application.ExecutablePath + "\"";
        }
    }
}
