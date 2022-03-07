// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build cgo
// +build cgo

// package testenv -- go2cs converted at 2022 March 06 23:36:29 UTC
// import "internal/testenv" ==> using testenv = go.@internal.testenv_package
// Original source: C:\Program Files\Go\src\internal\testenv\testenv_cgo.go


namespace go.@internal;

public static partial class testenv_package {

private static void init() {
    haveCGO = true;
}

} // end testenv_package
