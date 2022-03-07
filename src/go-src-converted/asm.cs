// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains declarations to test the assembly in test_asm.s.

// package a -- go2cs converted at 2022 March 06 23:32:53 UTC
// import "golang.org/x/tools/go/analysis/passes/asmdecl/testdata/src/a" ==> using a = go.golang.org.x.tools.go.analysis.passes.asmdecl.testdata.src.a_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\asmdecl\testdata\src\a\asm.go

using System;


namespace go.golang.org.x.tools.go.analysis.passes.asmdecl.testdata.src;

public static partial class a_package {

public partial struct S {
    public int i;
    public bool b;
    public @string s;
}

private static void arg1(sbyte x, byte y);
private static void arg2(short x, ushort y);
private static void arg4(int x, uint y);
private static void arg8(long x, ulong y);
private static void argint(nint x, nuint y);
private static void argptr(ptr<byte> x, ptr<byte> y, channel<nint> c, map<nint, nint> m, Action f);
private static void argstring(@string x, @string y);
private static void argslice(slice<@string> x, slice<@string> y);
private static void argiface(object x, object y);
private static void argcomplex(complex64 x, System.Numerics.Complex128 y);
private static void argstruct(S x, object y);
private static void argarray(array<S> x);
private static nint returnint();
private static byte returnbyte(nint x);
private static (nint, short, @string, byte) returnnamed(byte x);
private static nint returnintmissing();
private static nint leaf(nint x, nint y);

private static void noprof(nint x);
private static void dupok(nint x);
private static void nosplit(nint x);
private static void rodata(nint x);
private static void noptr(nint x);
private static void wrapper(nint x);

private static uint f15271();
private static void f17584(float x, complex64 y);
private static void f29318(array<array<ulong>> x);

private static void noframe1(int x);
private static void noframe2(int x);

private static void fvariadic(nint _p0, params nint _p0);

} // end a_package
