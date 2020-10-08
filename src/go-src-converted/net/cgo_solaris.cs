// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build cgo,!netgo

// package net -- go2cs converted at 2020 October 08 03:31:12 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\cgo_solaris.go
/*
#cgo LDFLAGS: -lsocket -lnsl -lsendfile
#include <netdb.h>
*/
using C = go.C_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static readonly var cgoAddrInfoFlags = (var)C.AI_CANONNAME | C.AI_V4MAPPED | C.AI_ALL;

    }
}
