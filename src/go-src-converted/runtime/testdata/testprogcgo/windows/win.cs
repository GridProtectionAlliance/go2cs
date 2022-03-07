// package windows -- go2cs converted at 2022 March 06 22:26:11 UTC
// import "runtime/testdata/testprogcgo/windows" ==> using windows = go.runtime.testdata.testprogcgo.windows_package
// Original source: C:\Program Files\Go\src\runtime\testdata\testprogcgo\windows\win.go
/*
#cgo amd64 386 CFLAGS: -mnop-fun-dllimport

#include <windows.h>

DWORD agetthread() {
    return GetCurrentThreadId();
}
*/
using C = go.C_package;

namespace go.runtime.testdata.testprogcgo;

public static partial class windows_package {

public static uint GetThread() {
    return uint32(C.agetthread());
}

} // end windows_package
