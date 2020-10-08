// package main -- go2cs converted at 2020 October 08 04:57:33 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\methprom.go

using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // Tests of method promotion logic.
        public partial struct A
        {
            public long magic;
        }

        public static void x(this A a) => func((_, panic, __) =>
        {
            if (a.magic != 1L)
            {
                panic(a.magic);
            }

        });
        private static ptr<A> y(this ptr<A> _addr_a)
        {
            ref A a = ref _addr_a.val;

            return _addr_a!;
        }

        public partial struct B
        {
            public long magic;
        }

        public static void p(this B b) => func((_, panic, __) =>
        {
            if (b.magic != 2L)
            {
                panic(b.magic);
            }

        });
        private static void q(this ptr<B> _addr_b) => func((_, panic, __) =>
        {
            ref B b = ref _addr_b.val;

            if (b != theC.B)
            {
                panic("oops");
            }

        });

        public partial interface I
        {
            void f();
        }

        private partial struct impl
        {
            public long magic;
        }

        private static void f(this impl i) => func((_, panic, __) =>
        {
            if (i.magic != 3L)
            {
                panic("oops");
            }

        });

        public partial struct C : I
        {
            public ref A A => ref A_val;
            public ref ptr<B> ptr<B> => ref ptr<B>_ptr;
            public I I;
        }

        private static void assert(bool cond) => func((_, panic, __) =>
        {
            if (!cond)
            {
                panic("failed");
            }

        });

        private static C theC = new C(A:A{1},B:&B{2},I:impl{3},);

        private static ptr<C> addr()
        {
            return _addr__addr_theC!;
        }

        private static C value()
        {
            return theC;
        }

        private static void Main() => func((_, panic, __) =>
        { 
            // address
            addr().x();
            if (addr().y() != _addr_theC.A)
            {
                panic("oops");
            }

            addr().p();
            addr().q();
            addr().f(); 

            // addressable value
            C c = value();
            c.x();
            if (c.y() != _addr_c.A)
            {
                panic("oops");
            }

            c.p();
            c.q();
            c.f(); 

            // non-addressable value
            value().x(); 
            // value().y() // not in method set
            value().p();
            value().q();
            value().f();

        });
    }
}
