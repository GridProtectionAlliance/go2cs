// package main -- go2cs converted at 2020 October 09 06:03:46 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\fieldprom.go

using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // Tests of field promotion logic.
        public partial struct A
        {
            public long x;
            public ptr<long> y;
        }

        public partial struct B
        {
            public long p;
            public ptr<long> q;
        }

        public partial struct C
        {
            public ref A A => ref A_val;
            public ref ptr<B> ptr<B> => ref ptr<B>_ptr;
        }

        public partial struct D
        {
            public long a;
            public ref C C => ref C_val;
        }

        private static void assert(bool cond) => func((_, panic, __) =>
        {
            if (!cond)
            {
                panic("failed");
            }

        });

        private static void f1(C c)
        {
            assert(c.x == c.A.x);
            assert(c.y == c.A.y);
            assert(_addr_c.x == _addr_c.A.x);
            assert(_addr_c.y == _addr_c.A.y);

            assert(c.p == c.B.p);
            assert(c.q == c.B.q);
            assert(_addr_c.p == _addr_c.B.p);
            assert(_addr_c.q == _addr_c.B.q);

            c.x = 1L;
            c.y.val = 1L;
            c.p = 1L;
            c.q.val = 1L;
        }

        private static void f2(ptr<C> _addr_c)
        {
            ref C c = ref _addr_c.val;

            assert(c.x == c.A.x);
            assert(c.y == c.A.y);
            assert(_addr_c.x == _addr_c.A.x);
            assert(_addr_c.y == _addr_c.A.y);

            assert(c.p == c.B.p);
            assert(c.q == c.B.q);
            assert(_addr_c.p == _addr_c.B.p);
            assert(_addr_c.q == _addr_c.B.q);

            c.x = 1L;
            c.y.val = 1L;
            c.p = 1L;
            c.q.val = 1L;
        }

        private static void f3(D d)
        {
            assert(d.x == d.C.A.x);
            assert(d.y == d.C.A.y);
            assert(_addr_d.x == _addr_d.C.A.x);
            assert(_addr_d.y == _addr_d.C.A.y);

            assert(d.p == d.C.B.p);
            assert(d.q == d.C.B.q);
            assert(_addr_d.p == _addr_d.C.B.p);
            assert(_addr_d.q == _addr_d.C.B.q);

            d.x = 1L;
            d.y.val = 1L;
            d.p = 1L;
            d.q.val = 1L;
        }

        private static void f4(ptr<D> _addr_d)
        {
            ref D d = ref _addr_d.val;

            assert(d.x == d.C.A.x);
            assert(d.y == d.C.A.y);
            assert(_addr_d.x == _addr_d.C.A.x);
            assert(_addr_d.y == _addr_d.C.A.y);

            assert(d.p == d.C.B.p);
            assert(d.q == d.C.B.q);
            assert(_addr_d.p == _addr_d.C.B.p);
            assert(_addr_d.q == _addr_d.C.B.q);

            d.x = 1L;
            d.y.val = 1L;
            d.p = 1L;
            d.q.val = 1L;
        }

        private static void Main()
        {
            ref long y = ref heap(123L, out ptr<long> _addr_y);
            ref C c = ref heap(new C(A{x:42,y:&y},&B{p:42,q:&y},), out ptr<C> _addr_c);

            assert(_addr_c.x == _addr_c.A.x);

            f1(c);
            f2(_addr_c);

            ref D d = ref heap(new D(C:c), out ptr<D> _addr_d);
            f3(d);
            f4(_addr_d);
        }
    }
}
