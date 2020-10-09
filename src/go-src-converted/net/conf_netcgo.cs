// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build netcgo

// package net -- go2cs converted at 2020 October 09 04:50:29 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\conf_netcgo.go
/*

// Fail if cgo isn't available.

*/
using C = go.C_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // The build tag "netcgo" forces use of the cgo DNS resolver.
        // It is the opposite of "netgo".
        private static void init()
        {
            netCgo = true;
        }
    }
}
