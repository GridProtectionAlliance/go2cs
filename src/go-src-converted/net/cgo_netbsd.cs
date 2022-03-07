// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build cgo && !netgo
// +build cgo,!netgo

// package net -- go2cs converted at 2022 March 06 22:15:13 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\cgo_netbsd.go
/*
#include <netdb.h>
*/
using C = go.C_package;

namespace go;

public static partial class net_package {

private static readonly var cgoAddrInfoFlags = C.AI_CANONNAME;


} // end net_package
