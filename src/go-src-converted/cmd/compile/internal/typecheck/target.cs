// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate go run mkbuiltin.go

// package typecheck -- go2cs converted at 2022 March 13 06:00:05 UTC
// import "cmd/compile/internal/typecheck" ==> using typecheck = go.cmd.compile.@internal.typecheck_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\typecheck\target.go
namespace go.cmd.compile.@internal;

using ir = cmd.compile.@internal.ir_package;

public static partial class typecheck_package {

// Target is the package being compiled.
public static ptr<ir.Package> Target;

} // end typecheck_package
