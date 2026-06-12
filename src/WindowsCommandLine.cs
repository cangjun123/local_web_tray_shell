using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace LocalWebTrayShell
{
    internal static class WindowsCommandLine
    {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine,
            out int pNumArgs);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr hMem);

        public static string[] Split(string commandLine)
        {
            IntPtr argv;
            string[] args;
            int argc;
            int index;

            if (string.IsNullOrWhiteSpace(commandLine))
            {
                return new string[0];
            }

            argv = CommandLineToArgvW(commandLine, out argc);

            if (argv == IntPtr.Zero)
            {
                throw new InvalidOperationException("Unable to parse command line.");
            }

            args = new string[argc];

            try
            {
                for (index = 0; index < argc; index++)
                {
                    IntPtr argPtr = Marshal.ReadIntPtr(argv, index * IntPtr.Size);
                    args[index] = Marshal.PtrToStringUni(argPtr);
                }
            }
            finally
            {
                LocalFree(argv);
            }

            return args;
        }

        public static string BuildArguments(string[] args, int startIndex)
        {
            List<string> parts = new List<string>();
            int index;

            if (args == null || args.Length <= startIndex)
            {
                return string.Empty;
            }

            for (index = startIndex; index < args.Length; index++)
            {
                parts.Add(Quote(args[index]));
            }

            return string.Join(" ", parts.ToArray());
        }

        public static string Quote(string value)
        {
            StringBuilder builder;
            int backslashCount;
            int index;
            char current;

            if (string.IsNullOrEmpty(value))
            {
                return "\"\"";
            }

            if (value.IndexOfAny(new[] { ' ', '\t', '"' }) < 0)
            {
                return value;
            }

            builder = new StringBuilder();
            builder.Append('"');
            backslashCount = 0;

            for (index = 0; index < value.Length; index++)
            {
                current = value[index];

                if (current == '\\')
                {
                    backslashCount++;
                    continue;
                }

                if (current == '"')
                {
                    builder.Append('\\', backslashCount * 2 + 1);
                    builder.Append('"');
                    backslashCount = 0;
                    continue;
                }

                if (backslashCount > 0)
                {
                    builder.Append('\\', backslashCount);
                    backslashCount = 0;
                }

                builder.Append(current);
            }

            if (backslashCount > 0)
            {
                builder.Append('\\', backslashCount * 2);
            }

            builder.Append('"');
            return builder.ToString();
        }
    }
}
