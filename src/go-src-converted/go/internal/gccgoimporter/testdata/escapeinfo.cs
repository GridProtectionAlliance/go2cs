// Test case for escape info in export data. To compile and extract .gox file:
// gccgo -fgo-optimize-allocs -c escapeinfo.go
// objcopy -j .go_export escapeinfo.o escapeinfo.gox

// package escapeinfo -- go2cs converted at 2020 October 09 06:02:54 UTC
// import "go/internal/gccgoimporter.escapeinfo" ==> using escapeinfo = go.go.@internal.gccgoimporter.escapeinfo_package
// Original source: C:\Go\src\go\internal\gccgoimporter\testdata\escapeinfo.go

using static go.builtin;

namespace go {
namespace go {
namespace @internal
{
    public static partial class escapeinfo_package
    {
        public partial struct T
        {
            public slice<byte> data;
        }

        public static ptr<T> NewT(slice<byte> data)
        {
            return addr(new T(data));
        }

        private static void Read(this ptr<T> _addr__p0, slice<byte> p)
        {
            ref T _p0 = ref _addr__p0.val;

        }
    }
}}}
