// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !amd64 && !s390x
// +build !amd64,!s390x

// package math -- go2cs converted at 2022 March 06 22:31:09 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\log_stub.go


namespace go;

public static partial class math_package {

private static readonly var haveArchLog = false;



private static double archLog(double x) => func((_, panic, _) => {
    panic("not implemented");
});

} // end math_package
