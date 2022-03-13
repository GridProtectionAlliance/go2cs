// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package typeparams provides functions to work with type parameter data
// stored in the AST, while these AST changes are guarded by a build
// constraint.

// package typeparams -- go2cs converted at 2022 March 13 05:52:49 UTC
// import "go/internal/typeparams" ==> using typeparams = go.go.@internal.typeparams_package
// Original source: C:\Program Files\Go\src\go\internal\typeparams\common.go
namespace go.go.@internal;

public static partial class typeparams_package {

// DisallowParsing is the numeric value of a parsing mode that disallows type
// parameters. This only matters if the typeparams experiment is active, and
// may be used for running tests that disallow generics.
public static readonly nint DisallowParsing = 1 << 30;


} // end typeparams_package
