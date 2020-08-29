// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 09:30:11 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\addressed.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static @string output = default;

        private static void mypanic(@string s) => func((_, panic, __) =>
        {
            fmt.Printf(output);
            panic(s);
        });

        private static void assertEqual(long x, long y)
        {
            if (x != y)
            {
                mypanic("assertEqual failed");
            }
        }

        private static void Main()
        {
            var x = f1_ssa(2L, 3L);
            output += fmt.Sprintln("*x is", x.Value);
            output += fmt.Sprintln("Gratuitously use some stack");
            output += fmt.Sprintln("*x is", x.Value);
            assertEqual(x.Value, 9L);

            var w = f3a_ssa(6L);
            output += fmt.Sprintln("*w is", w.Value);
            output += fmt.Sprintln("Gratuitously use some stack");
            output += fmt.Sprintln("*w is", w.Value);
            assertEqual(w.Value, 6L);

            var y = f3b_ssa(12L);
            output += fmt.Sprintln("*y.(*int) is", y._<ref long>().Value);
            output += fmt.Sprintln("Gratuitously use some stack");
            output += fmt.Sprintln("*y.(*int) is", y._<ref long>().Value);
            assertEqual(y._<ref long>().Value, 12L);

            var z = f3c_ssa(8L);
            output += fmt.Sprintln("*z.(*int) is", z._<ref long>().Value);
            output += fmt.Sprintln("Gratuitously use some stack");
            output += fmt.Sprintln("*z.(*int) is", z._<ref long>().Value);
            assertEqual(z._<ref long>().Value, 8L);

            args();
            test_autos();
        }

        //go:noinline
        private static ref long f1_ssa(long x, long y)
        {
            x = x * y + y;
            return ref x;
        }

        //go:noinline
        private static ref long f3a_ssa(long x)
        {
            return ref x;
        }

        //go:noinline
        private static void f3b_ssa(long x)
        { // ./foo.go:15: internal error: f3b_ssa ~r1 (type interface {}) recorded as live on entry
            return ref x;
        }

        //go:noinline
        private static void f3c_ssa(long y)
        {
            var x = y;
            return ref x;
        }

        public partial struct V
        {
            public ptr<V> p;
            public long w;
            public long x;
        }

        private static void args()
        {
            V v = new V(p:nil,w:1,x:1);
            V a = new V(p:&v,w:2,x:2);
            V b = new V(p:&v,w:0,x:0);
            var i = v.args_ssa(a, b);
            output += fmt.Sprintln("i=", i);
            assertEqual(int(i), 2L);
        }

        //go:noinline
        public static long args_ssa(this V v, V a, V b)
        {
            if (v.w == 0L)
            {
                return v.x;
            }
            if (v.w == 1L)
            {
                return a.x;
            }
            if (v.w == 2L)
            {
                return b.x;
            }
            b.p.p = ref a; // v.p in caller = &a

            return -1L;
        }

        private static void test_autos()
        {
            test(11L);
            test(12L);
            test(13L);
            test(21L);
            test(22L);
            test(23L);
            test(31L);
            test(32L);
        }

        private static void test(long which)
        {
            output += fmt.Sprintln("test", which);
            V v1 = new V(w:30,x:3,p:nil);
            var (v2, v3) = v1.autos_ssa(which, 10L, 1L, 20L, 2L);
            if (which != v2.val())
            {
                output += fmt.Sprintln("Expected which=", which, "got v2.val()=", v2.val());
                mypanic("Failure of expected V value");
            }
            if (v2.p.val() != v3.val())
            {
                output += fmt.Sprintln("Expected v2.p.val()=", v2.p.val(), "got v3.val()=", v3.val());
                mypanic("Failure of expected V.p value");
            }
            if (which != v3.p.p.p.p.p.p.p.val())
            {
                output += fmt.Sprintln("Expected which=", which, "got v3.p.p.p.p.p.p.p.val()=", v3.p.p.p.p.p.p.p.val());
                mypanic("Failure of expected V.p value");
            }
        }

        public static long val(this V v)
        {
            return v.w + v.x;
        }

        // autos_ssa uses contents of v and parameters w1, w2, x1, x2
        // to initialize a bunch of locals, all of which have their
        // address taken to force heap allocation, and then based on
        // the value of which a pair of those locals are copied in
        // various ways to the two results y, and z, which are also
        // addressed. Which is expected to be one of 11-13, 21-23, 31, 32,
        // and y.val() should be equal to which and y.p.val() should
        // be equal to z.val().  Also, x(.p)**8 == x; that is, the
        // autos are all linked into a ring.
        //go:noinline
        public static (V, V) autos_ssa(this V v, long which, long w1, long x1, long w2, long x2) => func((_, panic, __) =>
        {
            fill_ssa(v.w, v.x, ref v, v.p); // gratuitous no-op to force addressing
            V a = default;            V b = default;            V c = default;            V d = default;            V e = default;            V f = default;            V g = default;            V h = default;

            fill_ssa(w1, x1, ref a, ref b);
            fill_ssa(w1, x2, ref b, ref c);
            fill_ssa(w1, v.x, ref c, ref d);
            fill_ssa(w2, x1, ref d, ref e);
            fill_ssa(w2, x2, ref e, ref f);
            fill_ssa(w2, v.x, ref f, ref g);
            fill_ssa(v.w, x1, ref g, ref h);
            fill_ssa(v.w, x2, ref h, ref a);
            switch (which)
            {
                case 11L: 
                    y = a;
                    z.getsI(ref b);
                    break;
                case 12L: 
                    y.gets(ref b);
                    z = c;
                    break;
                case 13L: 
                    y.gets(ref c);
                    z = d;
                    break;
                case 21L: 
                    y.getsI(ref d);
                    z.gets(ref e);
                    break;
                case 22L: 
                    y = e;
                    z = f;
                    break;
                case 23L: 
                    y.gets(ref f);
                    z.getsI(ref g);
                    break;
                case 31L: 
                    y = g;
                    z.gets(ref h);
                    break;
                case 32L: 
                    y.getsI(ref h);
                    z = a;
                    break;
                default: 

                    panic("");
                    break;
            }
            return;
        });

        // gets is an address-mentioning way of implementing
        // structure assignment.
        //go:noinline
        private static void gets(this ref V to, ref V from)
        {
            to.Value = from.Value;
        }

        // gets is an address-and-interface-mentioning way of
        // implementing structure assignment.
        //go:noinline
        private static void getsI(this ref V to, object from)
        {
            to.Value = from._<ref V>().Value;
        }

        // fill_ssa initializes r with V{w:w, x:x, p:p}
        //go:noinline
        private static void fill_ssa(long w, long x, ref V r, ref V p)
        {
            r.Value = new V(w:w,x:x,p:p);
        }
    }
}
