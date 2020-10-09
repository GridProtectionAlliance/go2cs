// +build windows

// package main -- go2cs converted at 2020 October 09 05:01:07 UTC
// Original source: C:\Go\src\runtime\testdata\testwinlibsignal\dummy.go

using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        //export Dummy
        public static long Dummy()
        {
            return 42L;
        }

        private static void Main()
        {
        }
    }
}
