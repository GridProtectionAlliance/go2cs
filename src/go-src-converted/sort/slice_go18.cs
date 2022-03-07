// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build go1.8 && !go1.13
// +build go1.8,!go1.13

// package sort -- go2cs converted at 2022 March 06 22:12:36 UTC
// import "sort" ==> using sort = go.sort_package
// Original source: C:\Program Files\Go\src\sort\slice_go18.go
using reflect = go.reflect_package;

namespace go;

public static partial class sort_package {

private static var reflectValueOf = reflect.ValueOf;
private static var reflectSwapper = reflect.Swapper;

} // end sort_package
