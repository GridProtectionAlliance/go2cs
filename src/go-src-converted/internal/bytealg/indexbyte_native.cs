// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build 386 || (amd64 && !plan9) || s390x || arm || arm64 || loong64 || ppc64 || ppc64le || mips || mipsle || mips64 || mips64le || riscv64 || wasm
namespace go.@internal;

partial class bytealg_package {

//go:noescape
public static partial nint IndexByte(slice<byte> b, byte c);

//go:noescape
public static partial nint IndexByteString(@string s, byte c);

} // end bytealg_package
