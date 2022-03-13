// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
//go:build ppc64 || s390x || mips || mips64
// +build ppc64 s390x mips mips64

// package os -- go2cs converted at 2022 March 13 05:27:48 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\endian_big.go
namespace go;

public static partial class os_package {

private static readonly var isBigEndian = true;


} // end os_package
