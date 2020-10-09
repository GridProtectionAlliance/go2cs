// package main -- go2cs converted at 2020 October 09 06:03:43 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\complit.go
// Tests of composite literals.

using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // Map literals.
        // TODO(adonovan): we can no longer print maps
        // until the interpreter supports (reflect.Value).MapRange.
        private static void _() => func((_, panic, __) =>
        {
            public partial struct M // : map<long, long>
            {
            }
            ptr<M> m1 = new slice<ptr<M>>(new ptr<M>[] { {1:1}, &M{2:2} });
            @string want = "map[1:1] map[2:2]";
            {
                var got__prev1 = got;

                var got = fmt.Sprint(m1[0L].val, m1[1L].val);

                if (got != want)
                {
                    panic(got);
                }
                got = got__prev1;

            }

            M m2 = new slice<M>(new M[] { {1:1}, M{2:2} });
            {
                var got__prev1 = got;

                got = fmt.Sprint(m2[0L], m2[1L]);

                if (got != want)
                {
                    panic(got);
                }
                got = got__prev1;

            }

        });

        // Nonliteral keys in composite literal.
        private static void init() => func((_, panic, __) =>
        {
            const long zero = 1L;

            long v = new slice<long>(InitKeyedValues<long>((1+zero, 42)));
            {
                var x = fmt.Sprint(v);

                if (x != "[0 0 42]")
                {
                    panic(x);
                }

            }

        });

        // Test for in-place initialization.
        private static void init() => func((_, panic, __) =>
        { 
            // struct
            public partial struct S
            {
                public long a;
                public long b;
            }
            S s = new S(1,2);
            s = new S(b:3);
            if (s.a != 0L)
            {
                panic("s.a != 0");
            }

            if (s.b != 3L)
            {
                panic("s.b != 3");
            }

            s = new S();
            if (s.a != 0L)
            {
                panic("s.a != 0");
            }

            if (s.b != 0L)
            {
                panic("s.b != 0");
            } 

            // array
            public partial struct A // : array<long>
            {
            }
            A a = new A(2,4,6,8);
            a = new A(1:6,2:4);
            if (a[0L] != 0L)
            {
                panic("a[0] != 0");
            }

            if (a[1L] != 6L)
            {
                panic("a[1] != 6");
            }

            if (a[2L] != 4L)
            {
                panic("a[2] != 4");
            }

            if (a[3L] != 0L)
            {
                panic("a[3] != 0");
            }

            a = new A();
            if (a[0L] != 0L)
            {
                panic("a[0] != 0");
            }

            if (a[1L] != 0L)
            {
                panic("a[1] != 0");
            }

            if (a[2L] != 0L)
            {
                panic("a[2] != 0");
            }

            if (a[3L] != 0L)
            {
                panic("a[3] != 0");
            }

        });

        // Regression test for https://golang.org/issue/10127:
        // composite literal clobbers destination before reading from it.
        private static void init() => func((_, panic, __) =>
        { 
            // map
            {
                public partial struct M // : map<@string, long>
                {
                }
                M m = new M("x":1,"y":2);
                m = new M("x":m["y"],"y":m["x"]);
                if (m["x"] != 2L || m["y"] != 1L)
                {
                    panic(fmt.Sprint(m));
                }

                M n = new M("x":3);
                m = new M("x":n["x"]);
                n = new M("x":m["x"]); // parallel assignment
                {
                    var got = fmt.Sprint(m["x"], n["x"]);

                    if (got != "3 2")
                    {
                        panic(got);
                    }

                }

            } 

            // struct
            {
                public partial struct T
                {
                    public long x;
                    public long y;
                    public long z;
                }
                T t = new T(x:1,y:2,z:3);

                t = new T(x:t.y,y:t.z,z:t.x); // all fields
                {
                    got = fmt.Sprint(t);

                    if (got != "{2 3 1}")
                    {
                        panic(got);
                    }

                }


                t = new T(x:t.y,y:t.z+3); // not all fields
                {
                    got = fmt.Sprint(t);

                    if (got != "{3 4 0}")
                    {
                        panic(got);
                    }

                }


                T u = new T(x:5,y:6,z:7);
                t = new T(x:u.x);
                u = new T(x:t.x); // parallel assignment
                {
                    got = fmt.Sprint(t, u);

                    if (got != "{5 0 0} {3 0 0}")
                    {
                        panic(got);
                    }

                }

            } 

            // array
            {
                array<long> a = new array<long>(InitKeyedValues<long>(3, (0, 1), (1, 2), (2, 3)));

                a = new array<long>(InitKeyedValues<long>(3, (0, a[1]), (1, a[2]), (2, a[0]))); //  all elements
                {
                    got = fmt.Sprint(a);

                    if (got != "[2 3 1]")
                    {
                        panic(got);
                    }

                }


                a = new array<long>(InitKeyedValues<long>(3, (0, a[1]), (1, a[2]+3))); //  not all elements
                {
                    got = fmt.Sprint(a);

                    if (got != "[3 4 0]")
                    {
                        panic(got);
                    }

                }


                array<long> b = new array<long>(InitKeyedValues<long>(3, (0, 5), (1, 6), (2, 7)));
                a = new array<long>(InitKeyedValues<long>(3, (0, b[0])));
                b = new array<long>(InitKeyedValues<long>(3, (0, a[0]))); // parallel assignment
                {
                    got = fmt.Sprint(a, b);

                    if (got != "[5 0 0] [3 0 0]")
                    {
                        panic(got);
                    }

                }

            } 

            // slice
            {
                long s = new slice<long>(InitKeyedValues<long>((0, 1), (1, 2), (2, 3)));

                s = new slice<long>(InitKeyedValues<long>((0, s[1]), (1, s[2]), (2, s[0]))); //  all elements
                {
                    got = fmt.Sprint(s);

                    if (got != "[2 3 1]")
                    {
                        panic(got);
                    }

                }


                s = new slice<long>(InitKeyedValues<long>((0, s[1]), (1, s[2]+3))); //  not all elements
                {
                    got = fmt.Sprint(s);

                    if (got != "[3 4]")
                    {
                        panic(got);
                    }

                }


                t = new slice<long>(InitKeyedValues<long>((0, 5), (1, 6), (2, 7)));
                s = new slice<long>(InitKeyedValues<long>((0, t[0])));
                t = new slice<long>(InitKeyedValues<long>((0, s[0]))); // parallel assignment
                {
                    got = fmt.Sprint(s, t);

                    if (got != "[5] [3]")
                    {
                        panic(got);
                    }

                }

            }

        });

        // Regression test for https://golang.org/issue/13341:
        // within a map literal, if a key expression is a composite literal,
        // Go 1.5 allows its type to be omitted.  An & operation may be implied.
        private static void init() => func((_, panic, __) =>
        {
            public partial struct S
            {
                public long a;
                public long b;
            } 
            // same as map[*S]bool{&S{x: 1}: true}
            map m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<S>, bool>{{x:1}:true};
            foreach (var (s) in m)
            {
                if (s.x != 1L)
                {
                    panic(s); // wrong key
                }

                return ;

            }
            panic("map is empty");

        });

        private static void Main()
        {
        }
    }
}
