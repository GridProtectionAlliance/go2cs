// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !s390x
// +build !s390x

// This is a large group of functions that most architectures don't
// implement in assembly.

// package math -- go2cs converted at 2022 March 13 05:42:05 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\stubs.go
namespace go;

public static partial class math_package {

private static readonly var haveArchAcos = false;



private static double archAcos(double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchAcosh = false;



private static double archAcosh(double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchAsin = false;



private static double archAsin(double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchAsinh = false;



private static double archAsinh(double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchAtan = false;



private static double archAtan(double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchAtan2 = false;



private static double archAtan2(double y, double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchAtanh = false;



private static double archAtanh(double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchCbrt = false;



private static double archCbrt(double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchCos = false;



private static double archCos(double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchCosh = false;



private static double archCosh(double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchErf = false;



private static double archErf(double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchErfc = false;



private static double archErfc(double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchExpm1 = false;



private static double archExpm1(double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchFrexp = false;



private static (double, nint) archFrexp(double x) => func((_, panic, _) => {
    double _p0 = default;
    nint _p0 = default;

    panic("not implemented");
});

private static readonly var haveArchLdexp = false;



private static double archLdexp(double frac, nint exp) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchLog10 = false;



private static double archLog10(double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchLog2 = false;



private static double archLog2(double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchLog1p = false;



private static double archLog1p(double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchMod = false;



private static double archMod(double x, double y) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchPow = false;



private static double archPow(double x, double y) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchRemainder = false;



private static double archRemainder(double x, double y) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchSin = false;



private static double archSin(double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchSinh = false;



private static double archSinh(double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchTan = false;



private static double archTan(double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchTanh = false;



private static double archTanh(double x) => func((_, panic, _) => {
    panic("not implemented");
});

} // end math_package
