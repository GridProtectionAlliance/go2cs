// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package issue38068 -- go2cs converted at 2020 October 08 04:32:05 UTC
// import "cmd/compile/internal/gc/testdata.issue38068" ==> using issue38068 = go.cmd.compile.@internal.gc.testdata.issue38068_package
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\reproducible\issue38068.go

using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal {
namespace gc
{
    public static partial class issue38068_package
    {
        // A type with a couple of inlinable, non-pointer-receiver methods
        // that have params and local variables.
        public partial struct A
        {
            public @string s;
            public ptr<A> next;
            public ptr<A> prev;
        }

        // Inlinable, value-received method with locals and parms.
        public static @string @double(this A a, @string x, long y)
        {
            if (y == 191L)
            {
                a.s = "";
            }

            var q = a.s + "a";
            var r = a.s + "b";
            return q + r;

        }

        // Inlinable, value-received method with locals and parms.
        public static @string triple(this A a, @string x, long y)
        {
            var q = a.s;
            if (y == 998877L)
            {
                a.s = x;
            }

            var r = a.s + a.s;
            return q + r;

        }

        private partial struct methods
        {
            public Func<ptr<A>, @string, long, @string> m1;
            public Func<ptr<A>, @string, long, @string> m2;
        }

        // Now a function that makes references to the methods via pointers,
        // which should trigger the wrapper generation.
        public static void P(ptr<A> _addr_a, ptr<methods> _addr_ms) => func((defer, _, __) =>
        {
            ref A a = ref _addr_a.val;
            ref methods ms = ref _addr_ms.val;

            if (a != null)
            {
                defer(() =>
                {
                    println("done");
                }());

            }

            println(ms.m1(a, "a", 2L));
            println(ms.m2(a, "b", 3L));

        });

        public static void G(ptr<A> _addr_x, long n)
        {
            ref A x = ref _addr_x.val;

            if (n <= 0L)
            {
                println(n);
                return ;
            } 
            // Address-taken local of type A, which will insure that the
            // compiler's dtypesym() routine will create a method wrapper.
            ref A a = ref heap(out ptr<A> _addr_a);            ref A b = ref heap(out ptr<A> _addr_b);

            a.next = x;
            _addr_a.prev = _addr_b;
            a.prev = ref _addr_a.prev.val;
            _addr_x = _addr_a;
            x = ref _addr_x.val;
            G(_addr_x, n - 2L);

        }

        public static methods M = default;

        public static void F()
        {
            M.m1 = ptr<A>;
            M.m2 = ptr<A>;
            G(_addr_null, 100L);
        }
    }
}}}}}
