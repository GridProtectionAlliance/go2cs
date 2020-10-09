// package b -- go2cs converted at 2020 October 09 05:52:43 UTC
// import "cmd/oldlink/internal/ld/testdata/issue26237.b" ==> using b = go.cmd.oldlink.@internal.ld.testdata.issue26237.b_package
// Original source: C:\Go\src\cmd\oldlink\internal\ld\testdata\issue26237\b.dir\b.go

using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace oldlink {
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
