// package issue30628 -- go2cs converted at 2022 March 13 06:42:30 UTC
// import "go/internal/gccgoimporter.issue30628" ==> using issue30628 = go.go.@internal.gccgoimporter.issue30628_package
// Original source: C:\Program Files\Go\src\go\internal\gccgoimporter\testdata\issue30628.go
namespace go.go.@internal;

using os = os_package;
using sync = sync_package;

public static partial class issue30628_package {

private static readonly var numR = int32(os.O_TRUNC + 5);



public partial struct Apple {
    public sync.RWMutex hey;
    public nint x;
}

} // end issue30628_package
