// package b -- go2cs converted at 2020 October 08 04:39:53 UTC
// import "cmd/link/internal/ld/testdata/issue26237.b" ==> using b = go.cmd.link.@internal.ld.testdata.issue26237.b_package
// Original source: C:\Go\src\cmd\link\internal\ld\testdata\issue26237\b.dir\b.go

using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace link {
namespace @internal {
namespace ld {
namespace testdata
{
    public static partial class b_package
    {
        private static long q = default;

        public static long Top(long x)
        {
            q += 1L;
            if (q != x)
            {
                return 3L;
            }

            return 4L;

        }

        public static long OOO(long x) => func((defer, _, __) =>
        {
            defer(() =>
            {
                q += x & 7L;
            }());
            return Top(x + 1L);

        });
    }
}}}}}}
