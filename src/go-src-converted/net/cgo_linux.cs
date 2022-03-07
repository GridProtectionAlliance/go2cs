// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !android && cgo && !netgo
// +build !android,cgo,!netgo

// package net -- go2cs converted at 2022 March 06 22:15:13 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\cgo_linux.go
/*
#include <netdb.h>
*/
using C = go.C_package;

namespace go;

public static partial class net_package {

    // NOTE(rsc): In theory there are approximately balanced
    // arguments for and against including AI_ADDRCONFIG
    // in the flags (it includes IPv4 results only on IPv4 systems,
    // and similarly for IPv6), but in practice setting it causes
    // getaddrinfo to return the wrong canonical name on Linux.
    // So definitely leave it out.
private static readonly var cgoAddrInfoFlags = C.AI_CANONNAME | C.AI_V4MAPPED | C.AI_ALL;


} // end net_package
