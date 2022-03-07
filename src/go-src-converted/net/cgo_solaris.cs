// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build cgo && !netgo
// +build cgo,!netgo

// package net -- go2cs converted at 2022 March 06 22:15:14 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\cgo_solaris.go
/*
#cgo LDFLAGS: -lsocket -lnsl -lsendfile
#include <netdb.h>
*/
using C = go.C_package;

namespace go;

public static partial class net_package {

private static readonly var cgoAddrInfoFlags = C.AI_CANONNAME | C.AI_V4MAPPED | C.AI_ALL;


} // end net_package
