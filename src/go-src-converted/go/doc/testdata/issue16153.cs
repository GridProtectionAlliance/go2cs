// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package issue16153 -- go2cs converted at 2020 October 08 04:02:54 UTC
// import "go/doc.issue16153" ==> using issue16153 = go.go.doc.issue16153_package
// Original source: C:\Go\src\go\doc\testdata\issue16153.go

using static go.builtin;

namespace go {
namespace go
{
    public static partial class issue16153_package
    {
        // original test case
        private static readonly byte x1 = (byte)255L;
        public static readonly long Y1 = (long)256L;


        // variations
        private static readonly byte x2 = (byte)255L;
        public static readonly var Y2 = (var)0;


        public static readonly long X3 = (long)iota;
        public static readonly long Y3 = (long)1L;


        public static readonly long X4 = (long)iota;
        public static readonly var Y4 = (var)0;

    }
}}
