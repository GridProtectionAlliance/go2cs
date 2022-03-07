// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build 386 || amd64 || s390x || arm || arm64 || ppc64 || ppc64le || mips || mipsle || wasm || mips64 || mips64le
// +build 386 amd64 s390x arm arm64 ppc64 ppc64le mips mipsle wasm mips64 mips64le

// package bytealg -- go2cs converted at 2022 March 06 22:30:04 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Program Files\Go\src\internal\bytealg\compare_native.go
using _@unsafe_ = go.@unsafe_package;

namespace go.@internal;

public static partial class bytealg_package {
 // For go:linkname

    //go:noescape
public static nint Compare(slice<byte> a, slice<byte> b);

// The declaration below generates ABI wrappers for functions
// implemented in assembly in this package but declared in another
// package.

//go:linkname abigen_runtime_cmpstring runtime.cmpstring
private static nint abigen_runtime_cmpstring(@string a, @string b);

} // end bytealg_package
