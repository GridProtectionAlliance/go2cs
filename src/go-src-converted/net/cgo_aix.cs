// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build cgo && !netgo
// +build cgo,!netgo

// package net -- go2cs converted at 2022 March 06 22:15:13 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\cgo_aix.go
/*
#include <sys/types.h>
#include <sys/socket.h>

#include <netdb.h>
*/
using C = go.C_package;/*
#include <sys/types.h>
#include <sys/socket.h>

#include <netdb.h>
*/


using @unsafe = go.@unsafe_package;

namespace go;

public static partial class net_package {

private static readonly var cgoAddrInfoFlags = C.AI_CANONNAME;



private static (nint, error) cgoNameinfoPTR(slice<byte> b, ptr<C.struct_sockaddr> _addr_sa, C.socklen_t salen) {
    nint _p0 = default;
    error _p0 = default!;
    ref C.struct_sockaddr sa = ref _addr_sa.val;

    var (gerrno, err) = C.getnameinfo(sa, C.size_t(salen), (C.@char.val)(@unsafe.Pointer(_addr_b[0])), C.size_t(len(b)), null, 0, C.NI_NAMEREQD);
    return (int(gerrno), error.As(err)!);
}

} // end net_package
