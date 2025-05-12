// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package math provides basic constants and mathematical functions.
//
// This package does not guarantee bit-identical results across architectures.
namespace go;

partial class math_package {

// Mathematical constants.
public static readonly UntypedFloat E = /* 2.71828182845904523536028747135266249775724709369995957496696763 */ 2.71828;                                           // https://oeis.org/A001113

public static readonly UntypedFloat Pi = /* 3.14159265358979323846264338327950288419716939937510582097494459 */ 3.14159;                                           // https://oeis.org/A000796

public static readonly UntypedFloat Phi = /* 1.61803398874989484820458683436563811772030917980576286213544862 */ 1.61803;                                           // https://oeis.org/A001622

public static readonly UntypedFloat Sqrt2 = /* 1.41421356237309504880168872420969807856967187537694807317667974 */ 1.41421;                                           // https://oeis.org/A002193

public static readonly UntypedFloat SqrtE = /* 1.64872127070012814684865078781416357165377610071014801157507931 */ 1.64872;                                           // https://oeis.org/A019774

public static readonly UntypedFloat SqrtPi = /* 1.77245385090551602729816748334114518279754945612238712821380779 */ 1.77245;                                           // https://oeis.org/A002161

public static readonly UntypedFloat SqrtPhi = /* 1.27201964951406896425242246173749149171560804184009624861664038 */ 1.27202;                                           // https://oeis.org/A139339

public static readonly UntypedFloat Ln2 = /* 0.693147180559945309417232121458176568075500134360255254120680009 */ 0.693147;                                           // https://oeis.org/A002162

public static readonly UntypedFloat Log2E = /* 1 / Ln2 */ 1.4427;

public static readonly UntypedFloat Ln10 = /* 2.30258509299404568401799145468436420760110148862877297603332790 */ 2.30259;                                           // https://oeis.org/A002392

public static readonly UntypedFloat Log10E = /* 1 / Ln10 */ 0.434294;

// Floating-point limit values.
// Max is the largest finite value representable by the type.
// SmallestNonzero is the smallest positive, non-zero value representable by the type.
public static readonly UntypedFloat MaxFloat32 = /* 0x1p127 * (1 + (1 - 0x1p-23)) */ 3.40282e+38; // 3.40282346638528859811704183484516925440e+38

public static readonly UntypedFloat SmallestNonzeroFloat32 = /* 0x1p-126 * 0x1p-23 */ 1.4013e-45; // 1.401298464324817070923729583289916131280e-45

public static readonly UntypedFloat MaxFloat64 = /* 0x1p1023 * (1 + (1 - 0x1p-52)) */ 1.79769e+308; // 1.79769313486231570814527423731704356798070e+308

public static readonly UntypedFloat SmallestNonzeroFloat64 = /* 0x1p-1022 * 0x1p-52 */ 4.94066e-324; // 4.9406564584124654417656879286822137236505980e-324

// Integer limit values.
internal static readonly UntypedInt intSize = /* 32 << (^uint(0) >> 63) */ 64; // 32 or 64

public static readonly UntypedInt MaxInt = /* 1<<(intSize-1) - 1 */ 9223372036854775807; // MaxInt32 or MaxInt64 depending on intSize.

public static readonly GoUntyped MinInt = /* -1 << (intSize - 1) */ // MinInt32 or MinInt64 depending on intSize.
    GoUntyped.Parse("-9223372036854775808");

public static readonly UntypedInt MaxInt8 = /* 1<<7 - 1 */ 127; // 127

public static readonly GoUntyped MinInt8 = /* -1 << 7 */            // -128
    GoUntyped.Parse("-128");

public static readonly UntypedInt MaxInt16 = /* 1<<15 - 1 */ 32767; // 32767

public static readonly GoUntyped MinInt16 = /* -1 << 15 */           // -32768
    GoUntyped.Parse("-32768");

public static readonly UntypedInt MaxInt32 = /* 1<<31 - 1 */ 2147483647; // 2147483647

public static readonly GoUntyped MinInt32 = /* -1 << 31 */           // -2147483648
    GoUntyped.Parse("-2147483648");

public static readonly UntypedInt MaxInt64 = /* 1<<63 - 1 */ 9223372036854775807; // 9223372036854775807

public static readonly GoUntyped MinInt64 = /* -1 << 63 */           // -9223372036854775808
    GoUntyped.Parse("-9223372036854775808");

public static readonly UntypedInt MaxUint = /* 1<<intSize - 1 */ 18446744073709551615; // MaxUint32 or MaxUint64 depending on intSize.

public static readonly UntypedInt MaxUint8 = /* 1<<8 - 1 */ 255; // 255

public static readonly UntypedInt MaxUint16 = /* 1<<16 - 1 */ 65535; // 65535

public static readonly UntypedInt MaxUint32 = /* 1<<32 - 1 */ 4294967295; // 4294967295

public static readonly UntypedInt MaxUint64 = /* 1<<64 - 1 */ 18446744073709551615; // 18446744073709551615

} // end math_package
