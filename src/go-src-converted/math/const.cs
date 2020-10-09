// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package math provides basic constants and mathematical functions.
//
// This package does not guarantee bit-identical results across architectures.
// package math -- go2cs converted at 2020 October 09 05:07:39 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\const.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        // Mathematical constants.
        public static readonly float E = (float)2.71828182845904523536028747135266249775724709369995957496696763F; // https://oeis.org/A001113
        public static readonly float Pi = (float)3.14159265358979323846264338327950288419716939937510582097494459F; // https://oeis.org/A000796
        public static readonly float Phi = (float)1.61803398874989484820458683436563811772030917980576286213544862F; // https://oeis.org/A001622

        public static readonly float Sqrt2 = (float)1.41421356237309504880168872420969807856967187537694807317667974F; // https://oeis.org/A002193
        public static readonly float SqrtE = (float)1.64872127070012814684865078781416357165377610071014801157507931F; // https://oeis.org/A019774
        public static readonly float SqrtPi = (float)1.77245385090551602729816748334114518279754945612238712821380779F; // https://oeis.org/A002161
        public static readonly float SqrtPhi = (float)1.27201964951406896425242246173749149171560804184009624861664038F; // https://oeis.org/A139339

        public static readonly float Ln2 = (float)0.693147180559945309417232121458176568075500134360255254120680009F; // https://oeis.org/A002162
        public static readonly long Log2E = (long)1L / Ln2;
        public static readonly float Ln10 = (float)2.30258509299404568401799145468436420760110148862877297603332790F; // https://oeis.org/A002392
        public static readonly long Log10E = (long)1L / Ln10;


        // Floating-point limit values.
        // Max is the largest finite value representable by the type.
        // SmallestNonzero is the smallest positive, non-zero value representable by the type.
        public static readonly float MaxFloat32 = (float)3.40282346638528859811704183484516925440e+38F; // 2**127 * (2**24 - 1) / 2**23
        public static readonly float SmallestNonzeroFloat32 = (float)1.401298464324817070923729583289916131280e-45F; // 1 / 2**(127 - 1 + 23)

        public static readonly float MaxFloat64 = (float)1.797693134862315708145274237317043567981e+308F; // 2**1023 * (2**53 - 1) / 2**52
        public static readonly float SmallestNonzeroFloat64 = (float)4.940656458412465441765687928682213723651e-324F; // 1 / 2**(1023 - 1 + 52)

        // Integer limit values.
        public static readonly long MaxInt8 = (long)1L << (int)(7L) - 1L;
        public static readonly long MinInt8 = (long)-1L << (int)(7L);
        public static readonly long MaxInt16 = (long)1L << (int)(15L) - 1L;
        public static readonly long MinInt16 = (long)-1L << (int)(15L);
        public static readonly long MaxInt32 = (long)1L << (int)(31L) - 1L;
        public static readonly long MinInt32 = (long)-1L << (int)(31L);
        public static readonly long MaxInt64 = (long)1L << (int)(63L) - 1L;
        public static readonly long MinInt64 = (long)-1L << (int)(63L);
        public static readonly long MaxUint8 = (long)1L << (int)(8L) - 1L;
        public static readonly long MaxUint16 = (long)1L << (int)(16L) - 1L;
        public static readonly long MaxUint32 = (long)1L << (int)(32L) - 1L;
        public static readonly long MaxUint64 = (long)1L << (int)(64L) - 1L;

    }
}
