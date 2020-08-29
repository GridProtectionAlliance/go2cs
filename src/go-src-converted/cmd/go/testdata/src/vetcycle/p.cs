// package p -- go2cs converted at 2020 August 29 10:02:04 UTC
// import "cmd/go/testdata/src.p" ==> using p = go.cmd.go.testdata.src.p_package
// Original source: C:\Go\src\cmd\go\testdata\src\vetcycle\p.go

using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace testdata
{
    public static partial class p_package
    {
        private partial interface _
        {
            void m(B1 _p0);
        }
        public partial interface A1
        {
            void a(D1 _p0);
        }
        public partial interface B1 : A1
        {
        }
        public partial interface C1 : B1
        {
        }
        public partial interface D1 : C1
        {
        }        private static A1 _ = A1.As(C1(null));
    }
}}}}
