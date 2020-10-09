// Tests of bound method closures.

// package main -- go2cs converted at 2020 October 09 06:03:42 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\boundmeth.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void assert(bool b) => func((_, panic, __) =>
        {
            if (!b)
            {
                panic("oops");
            }
        });

        public partial struct I // : long
        {
        }

        public static long add(this I i, long x)
        {
            return int(i) + x;
        }

        private static void valueReceiver()
        {
            I three = 3L;
            assert(three.add(5L) == 8L);
            Func<long, long> add3 = three.add;
            assert(add3(5L) == 8L);
        }

        public partial struct S
        {
            public long x;
        }

        private static void incr(this ptr<S> _addr_s)
        {
            ref S s = ref _addr_s.val;

            s.x++;
        }

        private static long get(this ptr<S> _addr_s)
        {
            ref S s = ref _addr_s.val;

            return s.x;
        }

        private static void pointerReceiver()
        {
            ptr<S> ps = @new<S>();
            var incr = ps.incr;
            var get = ps.get;
            assert(get() == 0L);
            incr();
            incr();
            incr();
            assert(get() == 3L);
        }

        private static void addressibleValuePointerReceiver()
        {
            S s = default;
            var incr = s.incr;
            var get = s.get;
            assert(get() == 0L);
            incr();
            incr();
            incr();
            assert(get() == 3L);
        }

        public partial struct S2
        {
            public ref S S => ref S_val;
        }

        private static void promotedReceiver()
        {
            S2 s2 = default;
            var incr = s2.incr;
            var get = s2.get;
            assert(get() == 0L);
            incr();
            incr();
            incr();
            assert(get() == 3L);
        }

        private static void anonStruct()
        {
            var s = default;
            var incr = s.incr;
            var get = s.get;
            assert(get() == 0L);
            incr();
            incr();
            incr();
            assert(get() == 3L);
        }

        private static void typeCheck()
        {
            var i = default;
            i = ptr<S>;
            _ = i._<Action<ptr<S>>>(); // type assertion: receiver type prepended to params

            S s = default;
            i = s.incr;
            _ = i._<Action>(); // type assertion: receiver type disappears
        }

        private partial struct errString // : @string
        {
        }

        private static @string Error(this errString err)
        {
            return string(err);
        }

        // Regression test for a builder crash.
        private static Func<@string> regress1(error x)
        {
            return x.Error;
        }

        // Regression test for b/7269:
        // taking the value of an interface method performs a nil check.
        private static void nilInterfaceMethodValue() => func((defer, panic, _) =>
        {
            var err = errors.New("ok");
            var f = err.Error;
            {
                var got__prev1 = got;

                var got = f();

                if (got != "ok")
                {
                    panic(got);
                }

                got = got__prev1;

            }


            err = null;
            {
                var got__prev1 = got;

                got = f();

                if (got != "ok")
                {
                    panic(got);
                }

                got = got__prev1;

            }


            defer(() =>
            {
                var r = fmt.Sprint(recover()); 
                // runtime panic string varies across toolchains
                if (r != "interface conversion: interface is nil, not error" && r != "runtime error: invalid memory address or nil pointer dereference")
                {
                    panic("want runtime panic from nil interface method value, got " + r);
                }

            }());
            f = err.Error; // runtime panic: err is nil
            panic("unreachable");

        });

        private static void Main() => func((_, panic, __) =>
        {
            valueReceiver();
            pointerReceiver();
            addressibleValuePointerReceiver();
            promotedReceiver();
            anonStruct();
            typeCheck();

            {
                var e = regress1(errString("hi"))();

                if (e != "hi")
                {
                    panic(e);
                }

            }


            nilInterfaceMethodValue();

        });
    }
}
