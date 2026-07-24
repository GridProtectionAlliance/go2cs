// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class abi_package {

public static partial void FuncPCTestFn();

public static uintptr FuncPCTestFnAddr; // address of FuncPCTestFn, directly retrieved from assembly

//go:noinline
public static uintptr FuncPCTest() {
    return FuncPCABI0(FuncPCTestFn);
}

} // end abi_package
