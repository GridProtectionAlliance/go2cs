// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package math -- go2cs converted at 2020 August 29 08:44:43 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Go\src\math\arith_s390x.go

using static go.builtin;

namespace go
{
    public static partial class math_package
    {
        private static double log10TrampolineSetup(double x)
;
        private static double log10Asm(double x)
;

        private static double cosTrampolineSetup(double x)
;
        private static double cosAsm(double x)
;

        private static double coshTrampolineSetup(double x)
;
        private static double coshAsm(double x)
;

        private static double sinTrampolineSetup(double x)
;
        private static double sinAsm(double x)
;

        private static double sinhTrampolineSetup(double x)
;
        private static double sinhAsm(double x)
;

        private static double tanhTrampolineSetup(double x)
;
        private static double tanhAsm(double x)
;

        private static double log1pTrampolineSetup(double x)
;
        private static double log1pAsm(double x)
;

        private static double atanhTrampolineSetup(double x)
;
        private static double atanhAsm(double x)
;

        private static double acosTrampolineSetup(double x)
;
        private static double acosAsm(double x)
;

        private static double acoshTrampolineSetup(double x)
;
        private static double acoshAsm(double x)
;

        private static double asinTrampolineSetup(double x)
;
        private static double asinAsm(double x)
;

        private static double asinhTrampolineSetup(double x)
;
        private static double asinhAsm(double x)
;

        private static double erfTrampolineSetup(double x)
;
        private static double erfAsm(double x)
;

        private static double erfcTrampolineSetup(double x)
;
        private static double erfcAsm(double x)
;

        private static double atanTrampolineSetup(double x)
;
        private static double atanAsm(double x)
;

        private static double atan2TrampolineSetup(double x, double y)
;
        private static double atan2Asm(double x, double y)
;

        private static double cbrtTrampolineSetup(double x)
;
        private static double cbrtAsm(double x)
;

        private static double logTrampolineSetup(double x)
;
        private static double logAsm(double x)
;

        private static double tanTrampolineSetup(double x)
;
        private static double tanAsm(double x)
;

        private static double expTrampolineSetup(double x)
;
        private static double expAsm(double x)
;

        private static double expm1TrampolineSetup(double x)
;
        private static double expm1Asm(double x)
;

        private static double powTrampolineSetup(double x, double y)
;
        private static double powAsm(double x, double y)
;

        // hasVectorFacility reports whether the machine has the z/Architecture
        // vector facility installed and enabled.
        private static bool hasVectorFacility()
;

        private static var hasVX = hasVectorFacility();
    }
}
