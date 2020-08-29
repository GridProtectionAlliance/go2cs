// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The package e is a go/doc test for embedded methods.
// package e -- go2cs converted at 2020 August 29 08:47:12 UTC
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
        private static void M(this ref T4 _p0)
        {
        }

        public partial struct T5
        {
            public ref T4 T4 => ref T4_val;
        }

        // ----------------------------------------------------------------------------
        // Recursive type declarations must not lead to endless recursion.

        public partial struct U1
        {
            public ref U1 U1 => ref U1_ptr;
        }

        // U1.M should appear as method of U1.
        private static void M(this ref U1 _p0)
        {
        }

        public partial struct U2
        {
            public ref U3 U3 => ref U3_ptr;
        }

        // U2.M should appear as method of U2 and as method of U3 only if AllMethods is set.
        private static void M(this ref U2 _p0)
        {
        }

        public partial struct U3
        {
            public ref U2 U2 => ref U2_ptr;
        }

        // U3.N should appear as method of U3 and as method of U2 only if AllMethods is set.
        private static void N(this ref U3 _p0)
        {
        }

        public partial struct U4
        {
            public ref u5 u5 => ref u5_ptr;
        }

        // U4.M should appear as method of U4.
        private static void M(this ref U4 _p0)
        {
        }

        private partial struct u5
        {
            public ref U4 U4 => ref U4_ptr;
        }

        // ----------------------------------------------------------------------------
        // A higher-level embedded type (and its methods) wins over the same type (and
        // its methods) embedded at a lower level.

        public partial struct V1
        {
            public ref V2 V2 => ref V2_ptr;
            public ref V5 V5 => ref V5_ptr;
        }

        public partial struct V2
        {
            public ref V3 V3 => ref V3_ptr;
        }

        public partial struct V3
        {
            public ref V4 V4 => ref V4_ptr;
        }

        public partial struct V4
        {
            public ref V5 V5 => ref V5_ptr;
        }

        public partial struct V5
        {
            public ref V6 V6 => ref V6_ptr;
        }

        public partial struct V6
        {
        }

        // V4.M should appear as method of V2 and V3 if AllMethods is set.
        private static void M(this ref V4 _p0)
        {
        }

        // V6.M should appear as method of V1 and V5 if AllMethods is set.
        private static void M(this ref V6 _p0)
        {
        }
    }
}}
