// package issue30628 -- go2cs converted at 2020 October 08 04:56:22 UTC
// import "go/internal/gccgoimporter.issue30628" ==> using issue30628 = go.go.@internal.gccgoimporter.issue30628_package
// Original source: C:\Go\src\go\internal\gccgoimporter\testdata\issue30628.go
using os = go.os_package;
using sync = go.sync_package;
using static go.builtin;

namespace go {
namespace go {
namespace @internal
{
    public static partial class issue30628_package
    {
        private static readonly var numR = (var)int32(os.O_TRUNC + 5L);



        public partial struct Apple
        {
            public sync.RWMutex hey;
            public long x;
        }
    }
}}}
