// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ir -- go2cs converted at 2022 March 13 06:00:17 UTC
// import "cmd/compile/internal/ir" ==> using ir = go.cmd.compile.@internal.ir_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ir\cfg.go
namespace go.cmd.compile.@internal;

public static partial class ir_package {

 
// maximum size variable which we will allocate on the stack.
// This limit is for explicit variable declarations like "var x T" or "x := ...".
// Note: the flag smallframes can update this value.
public static var MaxStackVarSize = int64(10 * 1024 * 1024);public static var MaxImplicitStackVarSize = int64(64 * 1024);public static var MaxSmallArraySize = int64(256);

} // end ir_package
