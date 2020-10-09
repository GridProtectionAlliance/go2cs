// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build go1.8,!go1.13

// package sort -- go2cs converted at 2020 October 09 04:49:19 UTC
// import "sort" ==> using sort = go.sort_package
// Original source: C:\Go\src\sort\slice_go18.go
using reflect = go.reflect_package;
using static go.builtin;

namespace go
{
    public static partial class sort_package
    {
        private static var reflectValueOf = reflect.ValueOf;
        private static var reflectSwapper = reflect.Swapper;
    }
}
