// package a -- go2cs converted at 2020 October 08 04:42:06 UTC
// import "cmd/oldlink/internal/ld/testdata/issue25459/a" ==> using a = go.cmd.oldlink.@internal.ld.testdata.issue25459.a_package
// Original source: C:\Go\src\cmd\oldlink\internal\ld\testdata\issue25459\a\a.go

using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal {
namespace ld {
namespace testdata {
namespace issue25459
{
    public static partial class a_package
    {
        public static readonly var Always = (var)true;



        public static long Count = default;

        public delegate long FuncReturningInt();

        public static FuncReturningInt PointerToConstIf = default;

        public static long ConstIf()
        {
            if (Always)
            {
                return 1L;
            }

            array<long> imdead = new array<long>(4L);
            imdead[Count] = 1L;
            return imdead[0L];

        }

        public static long CallConstIf()
        {
            Count += 3L;
            return ConstIf();
        }

        public static void Another() => func((defer, _, __) =>
        {
            defer(() =>
            {
                PointerToConstIf = ConstIf;

                Count += 1L;
            }());

        });
    }
}}}}}}}
