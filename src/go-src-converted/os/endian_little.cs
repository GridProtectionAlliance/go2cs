// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
//go:build 386 || amd64 || arm || arm64 || ppc64le || mips64le || mipsle || riscv64 || wasm
// +build 386 amd64 arm arm64 ppc64le mips64le mipsle riscv64 wasm

// package os -- go2cs converted at 2022 March 13 05:27:48 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\endian_little.go
namespace go;

public static partial class os_package {

private static readonly var isBigEndian = false;


} // end os_package
