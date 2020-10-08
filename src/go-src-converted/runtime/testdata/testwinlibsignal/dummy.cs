// +build windows

// package main -- go2cs converted at 2020 October 08 03:44:11 UTC
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
