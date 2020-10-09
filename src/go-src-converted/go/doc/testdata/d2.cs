// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test cases for sort order of declarations.

// package d -- go2cs converted at 2020 October 09 05:19:10 UTC
// import "go/doc.d" ==> using d = go.go.doc.d_package
// Original source: C:\Go\src\go\doc\testdata\d2.go

using static go.builtin;

namespace go {
namespace go
{
    public static partial class d_package
    {
        // C1 should be second.
        public static readonly long C1 = (long)1L;

        // C0 should be first.


        // C0 should be first.
        public static readonly long C0 = (long)0L;

        // V1 should be second.


        // V1 should be second.
        public static ulong V1 = default;

        // V0 should be first.
        public static System.UIntPtr V0 = default;

        // CAx constants should appear after CBx constants.
        public static readonly var CA2 = iota; // before CA1
        public static readonly var CA1 = 0; // before CA0
        public static readonly var CA0 = 1; // at end

        // VAx variables should appear after VBx variables.
        public static long VA2 = default;        public static long VA1 = default;        public static long VA0 = default;

        // T1 should be second.
        public partial struct T1
        {
        }

        // T0 should be first.
        public partial struct T0
        {
        }

        // F1 should be second.
        public static void F1()
        {
        }

        // F0 should be first.
        public static void F0()
        {
        }
    }
}}
