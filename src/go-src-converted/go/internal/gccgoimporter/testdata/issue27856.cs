// package lib -- go2cs converted at 2020 October 09 06:02:54 UTC
// import "go/internal/gccgoimporter.lib" ==> using lib = go.go.@internal.gccgoimporter.lib_package
// Original source: C:\Go\src\go\internal\gccgoimporter\testdata\issue27856.go

using static go.builtin;

namespace go {
namespace go {
namespace @internal
{
    public static partial class lib_package
    {
        public partial struct M
        {
            public E E;
        }
        public partial struct F
        {
            public ptr<M> _;
        }
        public partial struct E // : F
        {
        }
    }
}}}
