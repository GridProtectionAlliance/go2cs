// Test of examples with divergent packages.

// Package buf ...
// package buf -- go2cs converted at 2020 August 29 10:10:41 UTC
// import "cmd/vet/testdata.buf" ==> using buf = go.cmd.vet.testdata.buf_package
// Original source: C:\Go\src\cmd\vet\testdata\divergent\buf.go

using static go.builtin;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class buf_package
    {
        // Buf is a ...
        public partial struct Buf // : slice<byte>
        {
        }

        // Append ...
        private static void Append(this ref Buf _p0, slice<byte> _p0)
        {
        }

        public static void Reset(this Buf _p0)
        {
        }

        public static long Len(this Buf _p0)
        {
            return 0L;
        }

        // DefaultBuf is a ...
        public static Buf DefaultBuf = default;
    }
}}}
