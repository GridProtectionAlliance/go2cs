// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build 386 || arm || mips || mipsle
// +build 386 arm mips mipsle

// package runtime -- go2cs converted at 2022 March 13 05:26:14 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\panic32.go
namespace go;

public static partial class runtime_package {

// Additional index/slice error paths for 32-bit platforms.
// Used when the high word of a 64-bit index is not zero.

// failures in the comparisons for s[x], 0 <= x < y (y == len(s))
private static void goPanicExtendIndex(nint hi, nuint lo, nint y) => func((_, panic, _) => {
    panicCheck1(getcallerpc(), "index out of range");
    panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:true,y:y,code:boundsIndex));
});
private static void goPanicExtendIndexU(nuint hi, nuint lo, nint y) => func((_, panic, _) => {
    panicCheck1(getcallerpc(), "index out of range");
    panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:false,y:y,code:boundsIndex));
});

// failures in the comparisons for s[:x], 0 <= x <= y (y == len(s) or cap(s))
private static void goPanicExtendSliceAlen(nint hi, nuint lo, nint y) => func((_, panic, _) => {
    panicCheck1(getcallerpc(), "slice bounds out of range");
    panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:true,y:y,code:boundsSliceAlen));
});
private static void goPanicExtendSliceAlenU(nuint hi, nuint lo, nint y) => func((_, panic, _) => {
    panicCheck1(getcallerpc(), "slice bounds out of range");
    panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:false,y:y,code:boundsSliceAlen));
});
private static void goPanicExtendSliceAcap(nint hi, nuint lo, nint y) => func((_, panic, _) => {
    panicCheck1(getcallerpc(), "slice bounds out of range");
    panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:true,y:y,code:boundsSliceAcap));
});
private static void goPanicExtendSliceAcapU(nuint hi, nuint lo, nint y) => func((_, panic, _) => {
    panicCheck1(getcallerpc(), "slice bounds out of range");
    panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:false,y:y,code:boundsSliceAcap));
});

// failures in the comparisons for s[x:y], 0 <= x <= y
private static void goPanicExtendSliceB(nint hi, nuint lo, nint y) => func((_, panic, _) => {
    panicCheck1(getcallerpc(), "slice bounds out of range");
    panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:true,y:y,code:boundsSliceB));
});
private static void goPanicExtendSliceBU(nuint hi, nuint lo, nint y) => func((_, panic, _) => {
    panicCheck1(getcallerpc(), "slice bounds out of range");
    panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:false,y:y,code:boundsSliceB));
});

// failures in the comparisons for s[::x], 0 <= x <= y (y == len(s) or cap(s))
private static void goPanicExtendSlice3Alen(nint hi, nuint lo, nint y) => func((_, panic, _) => {
    panicCheck1(getcallerpc(), "slice bounds out of range");
    panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:true,y:y,code:boundsSlice3Alen));
});
private static void goPanicExtendSlice3AlenU(nuint hi, nuint lo, nint y) => func((_, panic, _) => {
    panicCheck1(getcallerpc(), "slice bounds out of range");
    panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:false,y:y,code:boundsSlice3Alen));
});
private static void goPanicExtendSlice3Acap(nint hi, nuint lo, nint y) => func((_, panic, _) => {
    panicCheck1(getcallerpc(), "slice bounds out of range");
    panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:true,y:y,code:boundsSlice3Acap));
});
private static void goPanicExtendSlice3AcapU(nuint hi, nuint lo, nint y) => func((_, panic, _) => {
    panicCheck1(getcallerpc(), "slice bounds out of range");
    panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:false,y:y,code:boundsSlice3Acap));
});

// failures in the comparisons for s[:x:y], 0 <= x <= y
private static void goPanicExtendSlice3B(nint hi, nuint lo, nint y) => func((_, panic, _) => {
    panicCheck1(getcallerpc(), "slice bounds out of range");
    panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:true,y:y,code:boundsSlice3B));
});
private static void goPanicExtendSlice3BU(nuint hi, nuint lo, nint y) => func((_, panic, _) => {
    panicCheck1(getcallerpc(), "slice bounds out of range");
    panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:false,y:y,code:boundsSlice3B));
});

// failures in the comparisons for s[x:y:], 0 <= x <= y
private static void goPanicExtendSlice3C(nint hi, nuint lo, nint y) => func((_, panic, _) => {
    panicCheck1(getcallerpc(), "slice bounds out of range");
    panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:true,y:y,code:boundsSlice3C));
});
private static void goPanicExtendSlice3CU(nuint hi, nuint lo, nint y) => func((_, panic, _) => {
    panicCheck1(getcallerpc(), "slice bounds out of range");
    panic(new boundsError(x:int64(hi)<<32+int64(lo),signed:false,y:y,code:boundsSlice3C));
});

// Implemented in assembly, as they take arguments in registers.
// Declared here to mark them as ABIInternal.
private static void panicExtendIndex(nint hi, nuint lo, nint y);
private static void panicExtendIndexU(nuint hi, nuint lo, nint y);
private static void panicExtendSliceAlen(nint hi, nuint lo, nint y);
private static void panicExtendSliceAlenU(nuint hi, nuint lo, nint y);
private static void panicExtendSliceAcap(nint hi, nuint lo, nint y);
private static void panicExtendSliceAcapU(nuint hi, nuint lo, nint y);
private static void panicExtendSliceB(nint hi, nuint lo, nint y);
private static void panicExtendSliceBU(nuint hi, nuint lo, nint y);
private static void panicExtendSlice3Alen(nint hi, nuint lo, nint y);
private static void panicExtendSlice3AlenU(nuint hi, nuint lo, nint y);
private static void panicExtendSlice3Acap(nint hi, nuint lo, nint y);
private static void panicExtendSlice3AcapU(nuint hi, nuint lo, nint y);
private static void panicExtendSlice3B(nint hi, nuint lo, nint y);
private static void panicExtendSlice3BU(nuint hi, nuint lo, nint y);
private static void panicExtendSlice3C(nint hi, nuint lo, nint y);
private static void panicExtendSlice3CU(nuint hi, nuint lo, nint y);

} // end runtime_package
