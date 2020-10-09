// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:00:42 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\checkptr.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("CheckPtrAlignmentNoPtr", CheckPtrAlignmentNoPtr);
            register("CheckPtrAlignmentPtr", CheckPtrAlignmentPtr);
            register("CheckPtrArithmetic", CheckPtrArithmetic);
            register("CheckPtrArithmetic2", CheckPtrArithmetic2);
            register("CheckPtrSize", CheckPtrSize);
            register("CheckPtrSmall", CheckPtrSmall);
        }

        public static void CheckPtrAlignmentNoPtr()
        {
            array<long> x = new array<long>(2L);
            var p = @unsafe.Pointer(_addr_x[0L]);
            sink2 = (int64.val)(@unsafe.Pointer(uintptr(p) + 1L));
        }

        public static void CheckPtrAlignmentPtr()
        {
            array<long> x = new array<long>(2L);
            var p = @unsafe.Pointer(_addr_x[0L]);
            sink2 = (int64.val)(@unsafe.Pointer(uintptr(p) + 1L));
        }

        public static void CheckPtrArithmetic()
        {
            ref long x = ref heap(out ptr<long> _addr_x);
            var i = uintptr(@unsafe.Pointer(_addr_x));
            sink2 = (int.val)(@unsafe.Pointer(i));
        }

        public static void CheckPtrArithmetic2()
        {
            array<long> x = new array<long>(2L);
            var p = @unsafe.Pointer(_addr_x[1L]);
            System.UIntPtr one = 1L;
            sink2 = @unsafe.Pointer(uintptr(p) & ~one);
        }

        public static void CheckPtrSize()
        {
            ptr<int64> p = @new<int64>();
            sink2 = p;
            sink2 = new ptr<ptr<array<long>>>(@unsafe.Pointer(p));
        }

        public static void CheckPtrSmall()
        {
            sink2 = @unsafe.Pointer(uintptr(1L));
        }
    }
}
