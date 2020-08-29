// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test cases for sort order of declarations.

// package d -- go2cs converted at 2020 August 29 08:47:12 UTC
// import "go/doc.d" ==> using d = go.go.doc.d_package
// Original source: C:\Go\src\go\doc\testdata\d1.go

using static go.builtin;

namespace go {
namespace go
{
    public static partial class d_package
    {
        // C2 should be third.
        public static readonly long C2 = 2L;

        // V2 should be third.


        // V2 should be third.
        public static long V2 = default;

        // CBx constants should appear before CAx constants.
        public static readonly var CB2 = iota; // before CB1
        public static readonly var CB1 = 0; // before CB0
        public static readonly var CB0 = 1; // at end

        // VBx variables should appear before VAx variables.
        public static long VB2 = default;        public static long VB1 = default;        public static long VB0 = default;

 
        // Single const declarations inside ()'s are considered ungrouped
        // and show up in sorted order.
        public static readonly long Cungrouped = 0L;

 
        // Single var declarations inside ()'s are considered ungrouped
        // and show up in sorted order.
        public static long Vungrouped = 0L;

        // T2 should be third.
        public partial struct T2
        {
        }

        // Grouped types are sorted nevertheless.
 
        // TG2 should be third.
        public partial struct TG2
        {
        } 

        // TG1 should be second.
        public partial struct TG1
        {
        } 

        // TG0 should be first.
        public partial struct TG0
        {
        }        public static void F2()
        {
        }
    }
}}
