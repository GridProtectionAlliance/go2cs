// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build cgo && !netgo && (darwin || dragonfly || freebsd)
// +build cgo
// +build !netgo
// +build darwin dragonfly freebsd

// package net -- go2cs converted at 2022 March 06 22:15:13 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\cgo_bsd.go
/*
#include <netdb.h>
*/
using C = go.C_package;

namespace go;

public static partial class net_package {

private static readonly var cgoAddrInfoFlags = (C.AI_CANONNAME | C.AI_V4MAPPED | C.AI_ALL) & C.AI_MASK;


} // end net_package
