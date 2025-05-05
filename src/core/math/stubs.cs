// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !s390x
// This is a large group of functions that most architectures don't
// implement in assembly.
namespace go;

partial class math_package {

internal const bool haveArchAcos = false;

internal static float64 archAcos(float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchAcosh = false;

internal static float64 archAcosh(float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchAsin = false;

internal static float64 archAsin(float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchAsinh = false;

internal static float64 archAsinh(float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchAtan = false;

internal static float64 archAtan(float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchAtan2 = false;

internal static float64 archAtan2(float64 y, float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchAtanh = false;

internal static float64 archAtanh(float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchCbrt = false;

internal static float64 archCbrt(float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchCos = false;

internal static float64 archCos(float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchCosh = false;

internal static float64 archCosh(float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchErf = false;

internal static float64 archErf(float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchErfc = false;

internal static float64 archErfc(float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchExpm1 = false;

internal static float64 archExpm1(float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchFrexp = false;

internal static (float64, nint) archFrexp(float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchLdexp = false;

internal static float64 archLdexp(float64 frac, nint exp) {
    throw panic("not implemented");
}

internal const bool haveArchLog10 = false;

internal static float64 archLog10(float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchLog2 = false;

internal static float64 archLog2(float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchLog1p = false;

internal static float64 archLog1p(float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchMod = false;

internal static float64 archMod(float64 x, float64 y) {
    throw panic("not implemented");
}

internal const bool haveArchPow = false;

internal static float64 archPow(float64 x, float64 y) {
    throw panic("not implemented");
}

internal const bool haveArchRemainder = false;

internal static float64 archRemainder(float64 x, float64 y) {
    throw panic("not implemented");
}

internal const bool haveArchSin = false;

internal static float64 archSin(float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchSinh = false;

internal static float64 archSinh(float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchTan = false;

internal static float64 archTan(float64 x) {
    throw panic("not implemented");
}

internal const bool haveArchTanh = false;

internal static float64 archTanh(float64 x) {
    throw panic("not implemented");
}

} // end math_package
