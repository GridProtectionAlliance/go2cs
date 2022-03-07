// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build netcgo
// +build netcgo

// package net -- go2cs converted at 2022 March 06 22:15:18 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\conf_netcgo.go
/*

// Fail if cgo isn't available.

*/
using C = go.C_package;

namespace go;

public static partial class net_package {

    // The build tag "netcgo" forces use of the cgo DNS resolver.
    // It is the opposite of "netgo".
private static void init() {
    netCgo = true;
}

} // end net_package
