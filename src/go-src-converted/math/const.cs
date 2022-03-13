// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package math provides basic constants and mathematical functions.
//
// This package does not guarantee bit-identical results across architectures.

// package math -- go2cs converted at 2022 March 13 05:41:55 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\const.go
namespace go;

public static partial class math_package {

// Mathematical constants.
public static readonly float E = 2.71828182845904523536028747135266249775724709369995957496696763F; // https://oeis.org/A001113
public static readonly float Pi = 3.14159265358979323846264338327950288419716939937510582097494459F; // https://oeis.org/A000796
public static readonly float Phi = 1.61803398874989484820458683436563811772030917980576286213544862F; // https://oeis.org/A001622

public static readonly float Sqrt2 = 1.41421356237309504880168872420969807856967187537694807317667974F; // https://oeis.org/A002193
public static readonly float SqrtE = 1.64872127070012814684865078781416357165377610071014801157507931F; // https://oeis.org/A019774
public static readonly float SqrtPi = 1.77245385090551602729816748334114518279754945612238712821380779F; // https://oeis.org/A002161
public static readonly float SqrtPhi = 1.27201964951406896425242246173749149171560804184009624861664038F; // https://oeis.org/A139339

public static readonly float Ln2 = 0.693147180559945309417232121458176568075500134360255254120680009F; // https://oeis.org/A002162
public static readonly nint Log2E = 1 / Ln2;
public static readonly float Ln10 = 2.30258509299404568401799145468436420760110148862877297603332790F; // https://oeis.org/A002392
public static readonly nint Log10E = 1 / Ln10;

// Floating-point limit values.
// Max is the largest finite value representable by the type.
// SmallestNonzero is the smallest positive, non-zero value representable by the type.
public static readonly nuint MaxFloat32 = 0x1;private static readonly var p127 = 0;private static readonly nint intSize = 32 << (int)((~uint(0) >> 63)); // 32 or 64

public static readonly nint MaxInt = 1 << (int)((intSize - 1)) - 1;
public static readonly nint MinInt = -1 << (int)((intSize - 1));
public static readonly nint MaxInt8 = 1 << 7 - 1;
public static readonly nint MinInt8 = -1 << 7;
public static readonly nint MaxInt16 = 1 << 15 - 1;
public static readonly nint MinInt16 = -1 << 15;
public static readonly nint MaxInt32 = 1 << 31 - 1;
public static readonly nint MinInt32 = -1 << 31;
public static readonly nint MaxInt64 = 1 << 63 - 1;
public static readonly nint MinInt64 = -1 << 63;
public static readonly nint MaxUint = 1 << (int)(intSize) - 1;
public static readonly nint MaxUint8 = 1 << 8 - 1;
public static readonly nint MaxUint16 = 1 << 16 - 1;
public static readonly nint MaxUint32 = 1 << 32 - 1;
public static readonly nint MaxUint64 = 1 << 64 - 1;

} // end math_package
