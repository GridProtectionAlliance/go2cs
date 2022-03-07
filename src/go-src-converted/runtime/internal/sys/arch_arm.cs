// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sys -- go2cs converted at 2022 March 06 22:08:17 UTC
// import "runtime/internal/sys" ==> using sys = go.runtime.@internal.sys_package
// Original source: C:\Program Files\Go\src\runtime\internal\sys\arch_arm.go


namespace go.runtime.@internal;

public static partial class sys_package {

private static readonly var _ArchFamily = ARM;
private static readonly nint _DefaultPhysPageSize = 65536;
private static readonly nint _PCQuantum = 4;
private static readonly nint _MinFrameSize = 4;
private static readonly var _StackAlign = PtrSize;


} // end sys_package
