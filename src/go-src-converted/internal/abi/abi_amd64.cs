// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build goexperiment.regabireflect
// +build goexperiment.regabireflect

// package abi -- go2cs converted at 2022 March 06 22:30:03 UTC
// import "internal/abi" ==> using abi = go.@internal.abi_package
// Original source: C:\Program Files\Go\src\internal\abi\abi_amd64.go


namespace go.@internal;

public static partial class abi_package {

 
// See abi_generic.go.

// RAX, RBX, RCX, RDI, RSI, R8, R9, R10, R11.
public static readonly nint IntArgRegs = 9; 

// X0 -> X14.
public static readonly nint FloatArgRegs = 15; 

// We use SSE2 registers which support 64-bit float operations.
public static readonly nint EffectiveFloatRegSize = 8;


} // end abi_package
