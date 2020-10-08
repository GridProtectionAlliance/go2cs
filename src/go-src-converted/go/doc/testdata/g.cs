// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The package g is a go/doc test for mixed exported/unexported values.
// package g -- go2cs converted at 2020 October 08 04:02:53 UTC
// import "go/doc.g" ==> using g = go.go.doc.g_package
// Original source: C:\Go\src\go\doc\testdata\g.go

using static go.builtin;

namespace go {
namespace go
{
    public static partial class g_package
    {
        public static readonly var A = (var)iota;
        private static readonly var b = (var)iota;
        private static readonly var c = (var)0;
        public static readonly var D = (var)1;
        public static readonly var E = (var)2;
        private static readonly var f = (var)3;
        public static readonly var G = (var)4;
        public static readonly var H = (var)5;


        private static long c1 = 1L;        public static long C2 = 2L;        private static long c3 = 3L;
        public static long C4 = 4L;        private static long c5 = 5L;        public static long C6 = 6L;
        private static long c7 = 7L;        public static long C8 = 8L;        private static long c9 = 9L;
        private static long xx = 0L;        private static long yy = 0L;        private static long zz = 0L; // all unexported and hidden


    }
}}
