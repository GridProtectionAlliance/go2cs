// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build cgo,!netgo

// package net -- go2cs converted at 2020 October 09 04:50:25 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\cgo_android.go
//#include <netdb.h>
using C = go.C_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static readonly var cgoAddrInfoFlags = C.AI_CANONNAME;

    }
}
