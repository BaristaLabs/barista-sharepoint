namespace Barista.TuesPechkin
{
    using System;
    using System.Runtime.InteropServices;

    internal static class WinApiHelper
    {
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string filename);

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool SetDllDirectory(string lpPathName);
    }
}