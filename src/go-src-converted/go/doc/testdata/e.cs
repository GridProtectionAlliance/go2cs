// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The package e is a go/doc test for embedded methods.
// package e -- go2cs converted at 2020 October 08 04:02:53 UTC
// import "go/doc.e" ==> using e = go.go.doc.e_package
// Original source: C:\Go\src\go\doc\testdata\e.go

using static go.builtin;

namespace go {
namespace go
{
    public static partial class e_package
    {
        // ----------------------------------------------------------------------------
        // Conflicting methods M must not show up.
        private partial struct t1
        {
        }

        // t1.M should not appear as method in a Tx type.
        private static void M(this t1 _p0)
        {
        }

        private partial struct t2
        {
        }

        // t2.M should not appear as method in a Tx type.
        private static void M(this t2 _p0)
        {
        }

        // T1 has no embedded (level 1) M method due to conflict.
        public partial struct T1
        {
            public ref t1 t1 => ref t1_val;
            public ref t2 t2 => ref t2_val;
        }

        // ----------------------------------------------------------------------------
        // Higher-level method M wins over lower-level method M.

        // T2 has only M as top-level method.
        public partial struct T2
        {
            public ref t1 t1 => ref t1_val;
        }

        // T2.M should appear as method of T2.
        public static void M(this T2 _p0)
        {
        }

        // ----------------------------------------------------------------------------
        // Higher-level method M wins over lower-level conflicting methods M.

        private partial struct t1e
        {
            public ref t1 t1 => ref t1_val;
        }

        private partial struct t2e
        {
            public ref t2 t2 => ref t2_val;
        }

        // T3 has only M as top-level method.
        public partial struct T3
        {
            public ref t1e t1e => ref t1e_val;
            public ref t2e t2e => ref t2e_val;
        }

        // T3.M should appear as method of T3.
        public static void M(this T3 _p0)
        {
        }

        // ----------------------------------------------------------------------------
        // Don't show conflicting methods M embedded via an exported and non-exported
        // type.

        // T1 has no embedded (level 1) M method due to conflict.
        public partial struct T4
        {
            public ref t2 t2 => ref t2_val;
            public ref T2 T2 => ref T2_val;
        }

        // ----------------------------------------------------------------------------
        // Don't show embedded methods of exported anonymous fields unless AllMethods
        // is set.

        public partial struct T4
        {
            public ref t2 t2 => ref t2_val;
            public ref T2 T2 => ref T2_val;
        }

        // T4.M should appear as method of T5 only if AllMethods is set.
        private static void M(this ptr<T4> _addr__p0)
        {
            ref T4 _p0 = ref _addr__p0.val;

        }

        public partial struct T5
        {
            public ref T4 T4 => ref T4_val;
        }

        // ----------------------------------------------------------------------------
        // Recursive type declarations must not lead to endless recursion.

        public partial struct U1
        {
            public ref ptr<U1> ptr<U1> => ref ptr<U1>_ptr;
        }

        // U1.M should appear as method of U1.
        private static void M(this ptr<U1> _addr__p0)
        {
            ref U1 _p0 = ref _addr__p0.val;

        }

        public partial struct U2
        {
            public ref ptr<U3> ptr<U3> => ref ptr<U3>_ptr;
        }

        // U2.M should appear as method of U2 and as method of U3 only if AllMethods is set.
        private static void M(this ptr<U2> _addr__p0)
        {
            ref U2 _p0 = ref _addr__p0.val;

        }

        public partial struct U3
        {
            public ref ptr<U2> ptr<U2> => ref ptr<U2>_ptr;
        }

        // U3.N should appear as method of U3 and as method of U2 only if AllMethods is set.
        private static void N(this ptr<U3> _addr__p0)
        {
            ref U3 _p0 = ref _addr__p0.val;

        }

        public partial struct U4
        {
            public ref ptr<u5> ptr<u5> => ref ptr<u5>_ptr;
        }

        // U4.M should appear as method of U4.
        private static void M(this ptr<U4> _addr__p0)
        {
            ref U4 _p0 = ref _addr__p0.val;

        }

        private partial struct u5
        {
            public ref ptr<U4> ptr<U4> => ref ptr<U4>_ptr;
        }

        // ----------------------------------------------------------------------------
        // A higher-level embedded type (and its methods) wins over the same type (and
        // its methods) embedded at a lower level.

        public partial struct V1
        {
            public ref ptr<V2> ptr<V2> => ref ptr<V2>_ptr;
            public ref ptr<V5> ptr<V5> => ref ptr<V5>_ptr;
        }

        public partial struct V2
        {
            public ref ptr<V3> ptr<V3> => ref ptr<V3>_ptr;
        }

        public partial struct V3
        {
            public ref ptr<V4> ptr<V4> => ref ptr<V4>_ptr;
        }

        public partial struct V4
        {
            public ref ptr<V5> ptr<V5> => ref ptr<V5>_ptr;
        }

        public partial struct V5
        {
            public ref ptr<V6> ptr<V6> => ref ptr<V6>_ptr;
        }

        public partial struct V6
        {
        }

        // V4.M should appear as method of V2 and V3 if AllMethods is set.
        private static void M(this ptr<V4> _addr__p0)
        {
            ref V4 _p0 = ref _addr__p0.val;

        }

        // V6.M should appear as method of V1 and V5 if AllMethods is set.
        private static void M(this ptr<V6> _addr__p0)
        {
            ref V6 _p0 = ref _addr__p0.val;

        }
    }
}}
