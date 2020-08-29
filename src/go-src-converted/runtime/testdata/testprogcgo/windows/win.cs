// package windows -- go2cs converted at 2020 August 29 08:25:04 UTC
// import "runtime/testdata/testprogcgo/windows" ==> using windows = go.runtime.testdata.testprogcgo.windows_package
// Original source: C:\Go\src\runtime\testdata\testprogcgo\windows\win.go
/*
#cgo CFLAGS: -mnop-fun-dllimport

#include <windows.h>

DWORD agetthread() {
    return GetCurrentThreadId();
}
*/
using C = go.C_package;
using static go.builtin;

namespace go {
namespace runtime {
namespace testdata {
namespace testprogcgo
{
    public static partial class windows_package
    {
        public static uint GetThread()
        {
            return uint32(C.agetthread());
        }
    }
}}}}
