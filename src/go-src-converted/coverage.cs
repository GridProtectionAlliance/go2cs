// This interpreter test is designed to run very quickly yet provide
// some coverage of a broad selection of constructs.
//
// Validate this file with 'go run' after editing.
// TODO(adonovan): break this into small files organized by theme.

// package main -- go2cs converted at 2020 October 09 06:03:45 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\coverage.go
using fmt = go.fmt_package;
using reflect = go.reflect_package;
using strings = go.strings_package;
using static go.builtin;
using System.Threading;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void init() => func((_, panic, __) =>
        { 
            // Call of variadic function with (implicit) empty slice.
            {
                var x = fmt.Sprint();

                if (x != "")
                {
                    panic(x);
                }
            }

        });

        private partial interface empty
        {
        }

        public partial interface I
        {
            long f();
        }

        public partial struct T
        {
            public long z;
        }

        public static long f(this T t)
        {
            return t.z;
        }

        private static void use(object _p0)
        {
        }

        private static long counter = 2L;

        // Test initialization, including init blocks containing 'return'.
        // Assertion is in main.
        private static void init() => func((_, panic, __) =>
        {
            counter *= 3L;
            return ;
            counter *= 3L;
        });

        private static void init() => func((_, panic, __) =>
        {
            counter *= 5L;
            return ;
            counter *= 5L;
        });

        // Recursion.
        private static long fib(long x)
        {
            if (x < 2L)
            {
                return x;
            }

            return fib(x - 1L) + fib(x - 2L);

        }

        private static void fibgen(channel<long> ch)
        {
            for (long x = 0L; x < 10L; x++)
            {
                ch.Send(fib(x));
            }

            close(ch);

        }

        // Goroutines and channels.
        private static void init() => func((_, panic, __) =>
        {
            var ch = make_channel<long>();
            go_(() => fibgen(ch));
            slice<long> fibs = default;
            foreach (var (v) in ch)
            {
                fibs = append(fibs, v);
                if (len(fibs) == 10L)
                {
                    break;
                }

            }
            {
                var x = fmt.Sprint(fibs);

                if (x != "[0 1 1 2 3 5 8 13 21 34]")
                {
                    panic(x);
                }

            }

        });

        // Test of aliasing.
        private static void init() => func((_, panic, __) =>
        {
            public partial struct S
            {
                public @string a;
                public @string b;
            }

            @string s1 = new slice<@string>(new @string[] { "foo", "bar" });
            var s2 = s1; // creates an alias
            s2[0L] = "wiz";
            {
                var x = fmt.Sprint(s1, s2);

                if (x != "[wiz bar] [wiz bar]")
                {
                    panic(x);
                }

            }


            ptr<@string> pa1 = addr(new array<@string>(new @string[] { "foo", "bar" }));
            var pa2 = pa1; // creates an alias
            pa2[0L] = "wiz";
            {
                x = fmt.Sprint(pa1.val, pa2.val);

                if (x != "[wiz bar] [wiz bar]")
                {
                    panic(x);
                }

            }


            array<@string> a1 = new array<@string>(new @string[] { "foo", "bar" });
            var a2 = a1; // creates a copy
            a2[0L] = "wiz";
            {
                x = fmt.Sprint(a1, a2);

                if (x != "[foo bar] [wiz bar]")
                {
                    panic(x);
                }

            }


            S t1 = new S("foo","bar");
            var t2 = t1; // copy
            t2.a = "wiz";
            {
                x = fmt.Sprint(t1, t2);

                if (x != "{foo bar} {wiz bar}")
                {
                    panic(x);
                }

            }

        });

        private static void Main() => func((_, panic, __) =>
        {
            print(); // legal

            if (counter != 2L * 3L * 5L)
            {
                panic(counter);
            } 

            // Test builtins (e.g. complex) preserve named argument types.
            public partial struct N // : System.Numerics.Complex128
            {
            }
            N n = default;
            n = complex(1.0F, 2.0F);
            if (n != complex(1.0F, 2.0F))
            {
                panic(n);
            }

            {
                var x__prev1 = x;

                var x = reflect.TypeOf(n).String();

                if (x != "main.N")
                {
                    panic(x);
                }

                x = x__prev1;

            }

            if (real(n) != 1.0F || imag(n) != 2.0F)
            {
                panic(n);
            } 

            // Channel + select.
            var ch = make_channel<long>(1L);
            panic("couldn't send");
            if (ch != 1L.Receive())
            {
                panic("couldn't receive");
            } 
            // A "receive" select-case that doesn't declare its vars.  (regression test)
            long anint = 0L;
            var ok = false;
            _ = anint;
            _ = ok; 

            // Anon structs with methods.
            struct{T} anon = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{T}{T:T{z:1}};
            {
                var x__prev1 = x;

                x = anon.f();

                if (x != 1L)
                {
                    panic(x);
                }

                x = x__prev1;

            }

            I i = I.As(anon)!;
            {
                var x__prev1 = x;

                x = i.f();

                if (x != 1L)
                {
                    panic(x);
                } 
                // NB. precise output of reflect.Type.String is undefined.

                x = x__prev1;

            } 
            // NB. precise output of reflect.Type.String is undefined.
            {
                var x__prev1 = x;

                x = reflect.TypeOf(i).String();

                if (x != "struct { main.T }" && x != "struct{main.T}")
                {
                    panic(x);
                } 

                // fmt.

                x = x__prev1;

            } 

            // fmt.
            const @string message = (@string)"Hello, World!";

            if (fmt.Sprint("Hello", ", ", "World", "!") != message)
            {
                panic("oops");
            } 

            // Type assertion.
            public partial struct S
            {
                public @string a;
                public @string b;
            }
            empty e = empty.As(new S(f:42))!;
            switch (e.type())
            {
                case S v:
                    if (v.f != 42L)
                    {
                        panic(v.f);
                    }

                    break;
                default:
                {
                    var v = e.type();
                    panic(reflect.TypeOf(v));
                    break;
                }
            }
            {
                I i__prev1 = i;

                I (i, ok) = I.As(e._<I>())!;

                if (ok)
                {
                    panic(i);
                } 

                // Switch.

                i = i__prev1;

            } 

            // Switch.
            x = default;

            if (x == 1L)
            {
                panic(x);
                fallthrough = true;
            }
            if (fallthrough || x == 2L || x == 3L)
            {
                panic(x);
                goto __switch_break0;
            }
            // default: 
            __switch_break0:; 
            // empty switch

            // empty switch

            else             // empty switch

            if (false)             else 
            // string -> []rune conversion.
            use((slice<int>)"foo"); 

            // Calls of form x.f().
            public partial struct S2
            {
                public Func<long> f;
            }
            new S2(f:func()int{return1}).f(); // field is a func value
            new T().f(); // method call
            i.f()().f(); // anon interface method invocation

            // Map lookup.
            {
                var v__prev1 = v;

                map (v, ok) = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{}["foo5"];

                if (v != "" || ok)
                {
                    panic("oops");
                } 

                // Regression test: implicit address-taken struct literal
                // inside literal map element.

                v = v__prev1;

            } 

            // Regression test: implicit address-taken struct literal
            // inside literal map element.
        });

        private partial struct mybool // : bool
        {
        }

        private static void f(this mybool _p0)
        {
        }

        private static void init() => func((_, panic, __) =>
        {
            private partial struct mybool // : bool
            {
            }
            mybool b = default;
            var i = b || b; // result preserves types of operands
            _ = i._<mybool>();

            i = false && b; // result preserves type of "typed" operand
            _ = i._<mybool>();

            i = b || true; // result preserves type of "typed" operand
            _ = i._<mybool>();

        });

        private static void init() => func((_, panic, __) =>
        {
            long x = default;            long y = default;

            mybool b = x == y; // x==y is an untyped bool
            b.f();

        });

        // Simple closures.
        private static void init() => func((_, panic, __) =>
        {
            long b = 3L;
            Func<long, long> f = a =>
            {
                return a + b;
            }
;
            b++;
            {
                var x = f(1L);

                if (x != 5L)
                { // 1+4 == 5
                    panic(x);

                }

            }

            b++;
            {
                x = f(2L);

                if (x != 7L)
                { // 2+5 == 7
                    panic(x);

                }

            }

            {
                b = f(1L) < 16L || f(2L) < 17L;

                if (!b)
                {
                    panic("oops");
                }

            }

        });

        // Shifts.
        private static void init() => func((_, panic, __) =>
        {
            long i = 1L;
            ulong u = 1L << (int)(32L);
            {
                var x = i << (int)(uint32(u));

                if (x != 1L)
                {
                    panic(x);
                }

            }

            {
                x = i << (int)(uint64(u));

                if (x != 0L)
                {
                    panic(x);
                }

            }

        });

        // Implicit conversion of delete() key operand.
        private static void init() => func((_, panic, __) =>
        {
            public partial interface I
            {
                long f();
            }
            var m = make_map<I, bool>();
            m[1L] = true;
            m[I(2L)] = true;
            if (len(m) != 2L)
            {
                panic(m);
            }

            delete(m, I(1L));
            delete(m, 2L);
            if (len(m) != 0L)
            {
                panic(m);
            }

        });

        // An I->I conversion always succeeds.
        private static void init() => func((_, panic, __) =>
        {
            I x = default!;
            if (I(x) != I(null))
            {
                panic("I->I conversion failed");
            }

        });

        // An I->I type-assert fails iff the value is nil.
        private static void init() => func((_, panic, __) =>
        {
            defer(() =>
            {
                var r = fmt.Sprint(recover()); 
                // Exact error varies by toolchain.
                if (r != "runtime error: interface conversion: interface is nil, not main.I" && r != "interface conversion: interface is nil, not main.I")
                {
                    panic("I->I type assertion succeeded for nil value");
                }

            }());
            I x = default!;
            _ = x._<I>();

        });

        //////////////////////////////////////////////////////////////////////
        // Variadic bridge methods and interface thunks.

        public partial struct VT // : long
        {
        }

        private static long vcount = 0L;

        public static void f(this VT _p0, long x, params @string[] y) => func((_, panic, __) =>
        {
            y = y.Clone();

            vcount++;
            if (x != 1L)
            {
                panic(x);
            }

            if (len(y) != 2L || y[0L] != "foo" || y[1L] != "bar")
            {
                panic(y);
            }

        });

        public partial struct VS
        {
            public ref VT VT => ref VT_val;
        }

        public partial interface VI
        {
            void f(long x, params @string[] y);
        }

        private static void init() => func((_, panic, __) =>
        {
            @string foobar = new slice<@string>(new @string[] { "foo", "bar" });
            VS s = default;
            s.f(1L, "foo", "bar");
            s.f(1L, foobar);
            if (vcount != 2L)
            {
                panic("s.f not called twice");
            }

            var fn = VI.f;
            fn(s, 1L, "foo", "bar");
            fn(s, 1L, foobar);
            if (vcount != 4L)
            {
                panic("I.f not called twice");
            }

        });

        // Multiple labels on same statement.
        private static void multipleLabels() => func((_, panic, __) =>
        {
            slice<long> trace = default;
            long i = 0L;
one:
two:
            while (i < 3L)
            {
                trace = append(trace, i);
                switch (i)
                {
                    case 0L: 
                        _continuetwo = true;
                        break;
                        break;
                    case 1L: 
                        i++;
                        goto one;
                        break;
                    case 2L: 
                        _breaktwo = true;
                        break;
                        break;
                }
                i++;
            }
            {
                var x = fmt.Sprint(trace);

                if (x != "[0 1 2]")
                {
                    panic(x);
                }

            }

        });

        private static void init() => func((_, panic, __) =>
        {
            multipleLabels();
        });

        private static void init() => func((_, panic, __) =>
        { 
            // Struct equivalence ignores blank fields.
            private partial struct s
            {
                public long x;
                public long _;
                public long z;
            }
            s s1 = new s(x:1,z:3);
            s s2 = new s(x:1,z:3);
            if (s1 != s2)
            {
                panic("not equal");
            }

        });

        private static void init() => func((_, panic, __) =>
        { 
            // A slice var can be compared to const []T nil.
            @string i = new slice<@string>(new @string[] { "foo" });
            slice<@string> j = (slice<@string>)null;
            if (i._<slice<@string>>() == null)
            {
                panic("expected i non-nil");
            }

            if (j._<slice<@string>>() != null)
            {
                panic("expected j nil");
            } 
            // But two slices cannot be compared, even if one is nil.
            defer(() =>
            {
                var r = fmt.Sprint(recover());
                if (!(strings.Contains(r, "compar") && strings.Contains(r, "[]string")))
                {
                    panic("want panic from slice comparison, got " + r);
                }

            }());
            _ = i == j; // interface comparison recurses on types
        });

        private static void init() => func((_, panic, __) =>
        { 
            // Regression test for SSA renaming bug.
            slice<long> ints = default;
            foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_1<< in "foo")
            {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_1<<
                long x = default;
                x++;
                ints = append(ints, x);
            }
            if (fmt.Sprint(ints) != "[1 1 1]")
            {
                panic(ints);
            }

        });

        // Regression test for issue 6949:
        // []byte("foo") is not a constant since it allocates memory.
        private static void init() => func((_, panic, __) =>
        {
            @string r = default;
            foreach (var (i, b) in "ABC")
            {
                slice<byte> x = (slice<byte>)"abc";
                x[i] = byte(b);
                r += string(x);
            }
            if (r != "AbcaBcabC")
            {
                panic(r);
            }

        });

        // Test of 3-operand x[lo:hi:max] slice.
        private static void init() => func((_, panic, __) =>
        {
            long s = new slice<long>(new long[] { 0, 1, 2, 3 });
            Func<slice<long>, array<long>> lenCapLoHi = x => new array<long>(new long[] { len(x), cap(x), x[0], x[len(x)-1] });
            {
                var got = lenCapLoHi(s[1L..3L]);

                if (got != new array<long>(new long[] { 2, 3, 1, 2 }))
                {
                    panic(got);
                }

            }

            {
                got = lenCapLoHi(s.slice(1L, 3L, 3L));

                if (got != new array<long>(new long[] { 2, 2, 1, 2 }))
                {
                    panic(got);
                }

            }

            long max = 3L;
            if ("a"[0L] == 'a')
            {
                max = 2L; // max is non-constant, even in SSA form
            }

            {
                got = lenCapLoHi(s.slice(1L, 2L, max));

                if (got != new array<long>(new long[] { 1, 1, 1, 1 }))
                {
                    panic(got);
                }

            }

        });

        private static long one = 1L; // not a constant

        // Test makeslice.
        private static void init() => func((_, panic, __) =>
        {
            Action<slice<@string>, long, long> check = (s, wantLen, wantCap) =>
            {
                if (len(s) != wantLen)
                {
                    panic(len(s));
                }

                if (cap(s) != wantCap)
                {
                    panic(cap(s));
                }

            } 
            //                                       SSA form:
; 
            //                                       SSA form:
            check(make_slice<@string>(10L), 10L, 10L); // new([10]string)[:10]
            check(make_slice<@string>(one), 1L, 1L); // make([]string, one, one)
            check(make_slice<@string>(0L, 10L), 0L, 10L); // new([10]string)[:0]
            check(make_slice<@string>(0L, one), 0L, 1L); // make([]string, 0, one)
            check(make_slice<@string>(one, 10L), 1L, 10L); // new([10]string)[:one]
            check(make_slice<@string>(one, one), 1L, 1L); // make([]string, one, one)
        });

        // Test that a nice error is issued by indirection wrappers.
        private static void init() => func((_, panic, __) =>
        {
            ptr<T> ptr;
            I i = I.As(ptr)!;

            defer(() =>
            {
                var r = fmt.Sprint(recover()); 
                // Exact error varies by toolchain:
                if (r != "runtime error: value method (main.T).f called using nil *main.T pointer" && r != "value method (main.T).f called using nil *main.T pointer")
                {
                    panic("want panic from call with nil receiver, got " + r);
                }

            }());
            i.f();
            panic("unreachable");

        });

        // Regression test for a subtle bug in which copying values would causes
        // subcomponents of aggregate variables to change address, breaking
        // aliases.
        private static void init() => func((_, panic, __) =>
        {
            public partial struct T
            {
                public long z;
            }
            T x = default;
            var p = _addr_x.f;
            x = new T();
            p.val = 1L;
            if (x.f != 1L)
            {
                panic("lost store");
            }

            if (p != _addr_x.f)
            {
                panic("unstable address");
            }

        });
    }
}
