// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2022 March 06 22:31:02 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\arith_s390x.go
using cpu = go.@internal.cpu_package;

namespace go;

public static partial class math_package {

private static double expTrampolineSetup(double x);
private static double expAsm(double x);

private static double logTrampolineSetup(double x);
private static double logAsm(double x);

// Below here all functions are grouped in stubs.go for other
// architectures.

private static readonly var haveArchLog10 = true;



private static double archLog10(double x);
private static double log10TrampolineSetup(double x);
private static double log10Asm(double x);

private static readonly var haveArchCos = true;



private static double archCos(double x);
private static double cosTrampolineSetup(double x);
private static double cosAsm(double x);

private static readonly var haveArchCosh = true;



private static double archCosh(double x);
private static double coshTrampolineSetup(double x);
private static double coshAsm(double x);

private static readonly var haveArchSin = true;



private static double archSin(double x);
private static double sinTrampolineSetup(double x);
private static double sinAsm(double x);

private static readonly var haveArchSinh = true;



private static double archSinh(double x);
private static double sinhTrampolineSetup(double x);
private static double sinhAsm(double x);

private static readonly var haveArchTanh = true;



private static double archTanh(double x);
private static double tanhTrampolineSetup(double x);
private static double tanhAsm(double x);

private static readonly var haveArchLog1p = true;



private static double archLog1p(double x);
private static double log1pTrampolineSetup(double x);
private static double log1pAsm(double x);

private static readonly var haveArchAtanh = true;



private static double archAtanh(double x);
private static double atanhTrampolineSetup(double x);
private static double atanhAsm(double x);

private static readonly var haveArchAcos = true;



private static double archAcos(double x);
private static double acosTrampolineSetup(double x);
private static double acosAsm(double x);

private static readonly var haveArchAcosh = true;



private static double archAcosh(double x);
private static double acoshTrampolineSetup(double x);
private static double acoshAsm(double x);

private static readonly var haveArchAsin = true;



private static double archAsin(double x);
private static double asinTrampolineSetup(double x);
private static double asinAsm(double x);

private static readonly var haveArchAsinh = true;



private static double archAsinh(double x);
private static double asinhTrampolineSetup(double x);
private static double asinhAsm(double x);

private static readonly var haveArchErf = true;



private static double archErf(double x);
private static double erfTrampolineSetup(double x);
private static double erfAsm(double x);

private static readonly var haveArchErfc = true;



private static double archErfc(double x);
private static double erfcTrampolineSetup(double x);
private static double erfcAsm(double x);

private static readonly var haveArchAtan = true;



private static double archAtan(double x);
private static double atanTrampolineSetup(double x);
private static double atanAsm(double x);

private static readonly var haveArchAtan2 = true;



private static double archAtan2(double y, double x);
private static double atan2TrampolineSetup(double x, double y);
private static double atan2Asm(double x, double y);

private static readonly var haveArchCbrt = true;



private static double archCbrt(double x);
private static double cbrtTrampolineSetup(double x);
private static double cbrtAsm(double x);

private static readonly var haveArchTan = true;



private static double archTan(double x);
private static double tanTrampolineSetup(double x);
private static double tanAsm(double x);

private static readonly var haveArchExpm1 = true;



private static double archExpm1(double x);
private static double expm1TrampolineSetup(double x);
private static double expm1Asm(double x);

private static readonly var haveArchPow = true;



private static double archPow(double x, double y);
private static double powTrampolineSetup(double x, double y);
private static double powAsm(double x, double y);

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

private static readonly var haveArchLog2 = false;



private static double archLog2(double x) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchMod = false;



private static double archMod(double x, double y) => func((_, panic, _) => {
    panic("not implemented");
});

private static readonly var haveArchRemainder = false;



private static double archRemainder(double x, double y) => func((_, panic, _) => {
    panic("not implemented");
});

// hasVX reports whether the machine has the z/Architecture
// vector facility installed and enabled.
private static var hasVX = cpu.S390X.HasVX;

} // end math_package
