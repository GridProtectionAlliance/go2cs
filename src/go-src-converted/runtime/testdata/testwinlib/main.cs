// +build windows,cgo

// package main -- go2cs converted at 2020 October 09 05:01:07 UTC
// Original source: C:\Go\src\runtime\testdata\testwinlib\main.go
// #include <windows.h>
// typedef void(*callmeBackFunc)();
// static void bridgeCallback(callmeBackFunc callback) {
//    callback();
//}
using C = go.C_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // CallMeBack call backs C code.
        //export CallMeBack
        public static void CallMeBack(C.callmeBackFunc callback)
        {
            C.bridgeCallback(callback);
        }

        // Dummy is called by the C code before registering the exception/continue handlers simulating a debugger.
        // This makes sure that the Go runtime's lastcontinuehandler is reached before the C continue handler and thus,
        // validate that it does not crash the program before another handler could take an action.
        // The idea here is to reproduce what happens when you attach a debugger to a running program.
        // It also simulate the behavior of the .Net debugger, which register its exception/continue handlers lazily.
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
