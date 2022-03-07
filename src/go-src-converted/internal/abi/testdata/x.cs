// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package x -- go2cs converted at 2022 March 06 22:30:03 UTC
// import "internal/abi.x" ==> using x = go.@internal.abi.x_package
// Original source: C:\Program Files\Go\src\internal\abi\testdata\x.go
using abi = go.@internal.abi_package;
using System;


namespace go.@internal;

public static partial class x_package {

public static void Fn0(); // defined in assembly

public static void Fn1() {
}

public static Action FnExpr = default;

private static void test() {
    _ = abi.FuncPCABI0(Fn0); // line 16, no error
    _ = abi.FuncPCABIInternal(Fn0); // line 17, error
    _ = abi.FuncPCABI0(Fn1); // line 18, error
    _ = abi.FuncPCABIInternal(Fn1); // line 19, no error
    _ = abi.FuncPCABI0(FnExpr); // line 20, error
    _ = abi.FuncPCABIInternal(FnExpr); // line 21, no error
}

} // end x_package
