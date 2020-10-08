// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build 386 arm mips mipsle

// package runtime -- go2cs converted at 2020 October 08 03:22:23 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\panic32.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Additional index/slice error paths for 32-bit platforms.
        // Used when the high word of a 64-bit index is not zero.

        // failures in the comparisons for s[x], 0 <= x < y (y == len(s))
        private static void goPanicExtendIndex(long hi, ulong lo, long y) => func((_, panic, __) =>
        {
            panicCheck1(getcallerpc(), "index out of range");
            panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:true,y:y,code:boundsIndex));
        });
        private static void goPanicExtendIndexU(ulong hi, ulong lo, long y) => func((_, panic, __) =>
        {
            panicCheck1(getcallerpc(), "index out of range");
            panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:false,y:y,code:boundsIndex));
        });

        // failures in the comparisons for s[:x], 0 <= x <= y (y == len(s) or cap(s))
        private static void goPanicExtendSliceAlen(long hi, ulong lo, long y) => func((_, panic, __) =>
        {
            panicCheck1(getcallerpc(), "slice bounds out of range");
            panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:true,y:y,code:boundsSliceAlen));
        });
        private static void goPanicExtendSliceAlenU(ulong hi, ulong lo, long y) => func((_, panic, __) =>
        {
            panicCheck1(getcallerpc(), "slice bounds out of range");
            panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:false,y:y,code:boundsSliceAlen));
        });
        private static void goPanicExtendSliceAcap(long hi, ulong lo, long y) => func((_, panic, __) =>
        {
            panicCheck1(getcallerpc(), "slice bounds out of range");
            panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:true,y:y,code:boundsSliceAcap));
        });
        private static void goPanicExtendSliceAcapU(ulong hi, ulong lo, long y) => func((_, panic, __) =>
        {
            panicCheck1(getcallerpc(), "slice bounds out of range");
            panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:false,y:y,code:boundsSliceAcap));
        });

        // failures in the comparisons for s[x:y], 0 <= x <= y
        private static void goPanicExtendSliceB(long hi, ulong lo, long y) => func((_, panic, __) =>
        {
            panicCheck1(getcallerpc(), "slice bounds out of range");
            panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:true,y:y,code:boundsSliceB));
        });
        private static void goPanicExtendSliceBU(ulong hi, ulong lo, long y) => func((_, panic, __) =>
        {
            panicCheck1(getcallerpc(), "slice bounds out of range");
            panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:false,y:y,code:boundsSliceB));
        });

        // failures in the comparisons for s[::x], 0 <= x <= y (y == len(s) or cap(s))
        private static void goPanicExtendSlice3Alen(long hi, ulong lo, long y) => func((_, panic, __) =>
        {
            panicCheck1(getcallerpc(), "slice bounds out of range");
            panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:true,y:y,code:boundsSlice3Alen));
        });
        private static void goPanicExtendSlice3AlenU(ulong hi, ulong lo, long y) => func((_, panic, __) =>
        {
            panicCheck1(getcallerpc(), "slice bounds out of range");
            panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:false,y:y,code:boundsSlice3Alen));
        });
        private static void goPanicExtendSlice3Acap(long hi, ulong lo, long y) => func((_, panic, __) =>
        {
            panicCheck1(getcallerpc(), "slice bounds out of range");
            panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:true,y:y,code:boundsSlice3Acap));
        });
        private static void goPanicExtendSlice3AcapU(ulong hi, ulong lo, long y) => func((_, panic, __) =>
        {
            panicCheck1(getcallerpc(), "slice bounds out of range");
            panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:false,y:y,code:boundsSlice3Acap));
        });

        // failures in the comparisons for s[:x:y], 0 <= x <= y
        private static void goPanicExtendSlice3B(long hi, ulong lo, long y) => func((_, panic, __) =>
        {
            panicCheck1(getcallerpc(), "slice bounds out of range");
            panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:true,y:y,code:boundsSlice3B));
        });
        private static void goPanicExtendSlice3BU(ulong hi, ulong lo, long y) => func((_, panic, __) =>
        {
            panicCheck1(getcallerpc(), "slice bounds out of range");
            panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:false,y:y,code:boundsSlice3B));
        });

        // failures in the comparisons for s[x:y:], 0 <= x <= y
        private static void goPanicExtendSlice3C(long hi, ulong lo, long y) => func((_, panic, __) =>
        {
            panicCheck1(getcallerpc(), "slice bounds out of range");
            panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:true,y:y,code:boundsSlice3C));
        });
        private static void goPanicExtendSlice3CU(ulong hi, ulong lo, long y) => func((_, panic, __) =>
        {
            panicCheck1(getcallerpc(), "slice bounds out of range");
            panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:false,y:y,code:boundsSlice3C));
        });

        // Implemented in assembly, as they take arguments in registers.
        // Declared here to mark them as ABIInternal.
        private static void panicExtendIndex(long hi, ulong lo, long y)
;
        private static void panicExtendIndexU(ulong hi, ulong lo, long y)
;
        private static void panicExtendSliceAlen(long hi, ulong lo, long y)
;
        private static void panicExtendSliceAlenU(ulong hi, ulong lo, long y)
;
        private static void panicExtendSliceAcap(long hi, ulong lo, long y)
;
        private static void panicExtendSliceAcapU(ulong hi, ulong lo, long y)
;
        private static void panicExtendSliceB(long hi, ulong lo, long y)
;
        private static void panicExtendSliceBU(ulong hi, ulong lo, long y)
;
        private static void panicExtendSlice3Alen(long hi, ulong lo, long y)
;
        private static void panicExtendSlice3AlenU(ulong hi, ulong lo, long y)
;
        private static void panicExtendSlice3Acap(long hi, ulong lo, long y)
;
        private static void panicExtendSlice3AcapU(ulong hi, ulong lo, long y)
;
        private static void panicExtendSlice3B(long hi, ulong lo, long y)
;
        private static void panicExtendSlice3BU(ulong hi, ulong lo, long y)
;
        private static void panicExtendSlice3C(long hi, ulong lo, long y)
;
        private static void panicExtendSlice3CU(ulong hi, ulong lo, long y)
;
    }
}
