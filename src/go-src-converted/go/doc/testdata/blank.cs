// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package blank is a go/doc test for the handling of _.
// See issue 5397.
// package blank -- go2cs converted at 2020 October 09 05:19:10 UTC
// import "go/doc.blank" ==> using blank = go.go.doc.blank_package
// Original source: C:\Go\src\go\doc\testdata\blank.go
using os = go.os_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class blank_package
    {
        public partial struct T // : long
        {
        }

        // T constants counting from a blank constant.
        private static readonly T _ = (T)iota;
        public static readonly var T1 = 0;
        public static readonly var T2 = 1;


        // T constants counting from unexported constants.
        private static readonly T tweedledee = (T)iota;
        private static readonly var tweedledum = 0;
        public static readonly var C1 = 1;
        public static readonly var C2 = 2;
        private static readonly var alice = 3;
        public static readonly var C3 = 4;
        private static readonly long redQueen = (long)iota;
        public static readonly var C4 = 5;


        // Constants with a single type that is not propagated.
        private static readonly os.FileMode zero = (os.FileMode)0L;
        public static readonly long Default = (long)0644L;
        public static readonly long Useless = (long)0312L;
        public static readonly long WideOpen = (long)0777L;


        // Constants with an imported type that is propagated.
        private static readonly os.FileMode zero = (os.FileMode)0L;
        public static readonly var M1 = 0;
        public static readonly var M2 = 1;
        public static readonly var M3 = 2;


        // Package constants.
        private static readonly long _ = (long)iota;
        public static readonly var I1 = 0;
        public static readonly var I2 = 1;


        // Unexported constants counting from blank iota.
        // See issue 9615.
        private static readonly var _ = iota;
        private static readonly var one = iota + 1L;


        // Blanks not in doc output:

        // S has a padding field.
        public partial struct S
        {
            public uint H;
            public byte _;
            public byte A;
        }

        private static void _()
        {
        }

        private partial struct _ // : T
        {
        }

        private static var _ = T(55L);
    }
}}
