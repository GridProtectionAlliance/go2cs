// package main -- go2cs converted at 2020 October 08 04:57:32 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\ifaceconv.go

using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // Tests of interface conversions and type assertions.
        public partial interface I0
        {
        }
        public partial interface I1
        {
            void f();
        }
        public partial interface I2
        {
            void f();
            void g();
        }

        public partial struct C0
        {
        }
        public partial struct C1
        {
        }

        public static void f(this C1 _p0)
        {
        }

        public partial struct C2
        {
        }

        public static void f(this C2 _p0)
        {
        }
        public static void g(this C2 _p0)
        {
        }

        private static void Main() => func((_, panic, __) =>
        {
            I0 i0 = default!;
            I1 i1 = default!;
            I2 i2 = default!; 

            // Nil always causes a type assertion to fail, even to the
            // same type.
            {
                I0 (_, ok) = I0.As(i0._<I0>())!;

                if (ok)
                {
                    panic("nil i0.(I0) succeeded");
                }

            }

            {
                (_, ok) = I1.As(i1._<I1>())!;

                if (ok)
                {
                    panic("nil i1.(I1) succeeded");
                }

            }

            {
                (_, ok) = I2.As(i2._<I2>())!;

                if (ok)
                {
                    panic("nil i2.(I2) succeeded");
                } 

                // Conversions can't fail, even with nil.

            } 

            // Conversions can't fail, even with nil.
            _ = I0(i0);

            _ = I0(i1);
            _ = I1(i1);

            _ = I0(i2);
            _ = I1(i2);
            _ = I2(i2); 

            // Non-nil type assertions pass or fail based on the concrete type.
            i1 = I1.As(new C1())!;
            {
                (_, ok) = I0.As(i1._<I0>())!;

                if (!ok)
                {
                    panic("C1 i1.(I0) failed");
                }

            }

            {
                (_, ok) = I1.As(i1._<I1>())!;

                if (!ok)
                {
                    panic("C1 i1.(I1) failed");
                }

            }

            {
                (_, ok) = I2.As(i1._<I2>())!;

                if (ok)
                {
                    panic("C1 i1.(I2) succeeded");
                }

            }


            i1 = I1.As(new C2())!;
            {
                (_, ok) = I0.As(i1._<I0>())!;

                if (!ok)
                {
                    panic("C2 i1.(I0) failed");
                }

            }

            {
                (_, ok) = I1.As(i1._<I1>())!;

                if (!ok)
                {
                    panic("C2 i1.(I1) failed");
                }

            }

            {
                (_, ok) = I2.As(i1._<I2>())!;

                if (!ok)
                {
                    panic("C2 i1.(I2) failed");
                } 

                // Conversions can't fail.

            } 

            // Conversions can't fail.
            i1 = I1.As(new C1())!;
            if (I0(i1) == null)
            {
                panic("C1 I0(i1) was nil");
            }

            if (I1(i1) == null)
            {
                panic("C1 I1(i1) was nil");
            }

        });
    }
}
