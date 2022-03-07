// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build go1.13
// +build go1.13

// package sort -- go2cs converted at 2022 March 06 22:12:31 UTC
// import "sort" ==> using sort = go.sort_package
// Original source: C:\Program Files\Go\src\sort\slice_go113.go
using reflectlite = go.@internal.reflectlite_package;

namespace go;

public static partial class sort_package {

private static var reflectValueOf = reflectlite.ValueOf;
private static var reflectSwapper = reflectlite.Swapper;

} // end sort_package
