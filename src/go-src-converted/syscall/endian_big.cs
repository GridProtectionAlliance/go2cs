// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
//go:build ppc64 || s390x || mips || mips64
// +build ppc64 s390x mips mips64

// package syscall -- go2cs converted at 2022 March 06 22:26:25 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\endian_big.go


namespace go;

public static partial class syscall_package {

private static readonly var isBigEndian = true;


} // end syscall_package
