using System;

namespace go
{
    public static class runtime_package
    {
        // runtime GOOS proxy function
        public static string GOOS
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                    case PlatformID.WinCE:
                        return "windows";
                    case PlatformID.Unix:
                        return "linux";
                    case PlatformID.MacOSX:
                        return "darwin";
                    default:
                        return "undetermined";
                }
            }
        }
    }
}
