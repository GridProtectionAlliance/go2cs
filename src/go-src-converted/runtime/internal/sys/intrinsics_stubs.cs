// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build 386
// +build 386

// package sys -- go2cs converted at 2022 March 06 22:08:18 UTC
// import "runtime/internal/sys" ==> using sys = go.runtime.@internal.sys_package
// Original source: C:\Program Files\Go\src\runtime\internal\sys\intrinsics_stubs.go


namespace go.runtime.@internal;

public static partial class sys_package {

public static nint Ctz64(ulong x);
public static nint Ctz32(uint x);
public static nint Ctz8(byte x);
public static ulong Bswap64(ulong x);
public static uint Bswap32(uint x);

} // end sys_package
