// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build 386 || amd64 || s390x || arm || arm64 || ppc64 || ppc64le || mips || mipsle || mips64 || mips64le || riscv64 || wasm
// +build 386 amd64 s390x arm arm64 ppc64 ppc64le mips mipsle mips64 mips64le riscv64 wasm

// package bytealg -- go2cs converted at 2022 March 13 05:40:52 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Program Files\Go\src\internal\bytealg\indexbyte_native.go
namespace go.@internal;

public static partial class bytealg_package {

//go:noescape
public static nint IndexByte(slice<byte> b, byte c);

//go:noescape
public static nint IndexByteString(@string s, byte c);

} // end bytealg_package
