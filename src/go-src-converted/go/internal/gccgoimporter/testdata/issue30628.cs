// package issue30628 -- go2cs converted at 2022 March 06 23:32:49 UTC
// import "go/internal/gccgoimporter.issue30628" ==> using issue30628 = go.go.@internal.gccgoimporter.issue30628_package
// Original source: C:\Program Files\Go\src\go\internal\gccgoimporter\testdata\issue30628.go
using os = go.os_package;
using sync = go.sync_package;

namespace go.go.@internal;

public static partial class issue30628_package {

private static readonly var numR = int32(os.O_TRUNC + 5);



public partial struct Apple {
    public sync.RWMutex hey;
    public nint x;
}

} // end issue30628_package
