// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build cgo,!netgo
// +build android freebsd dragonfly openbsd

// package net -- go2cs converted at 2020 August 29 08:25:06 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\cgo_resold.go
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
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static (long, error) cgoNameinfoPTR(slice<byte> b, ref C.struct_sockaddr sa, C.socklen_t salen)
        {
            var (gerrno, err) = C.getnameinfo(sa, salen, (C.@char.Value)(@unsafe.Pointer(ref b[0L])), C.size_t(len(b)), null, 0L, C.NI_NAMEREQD);
            return (int(gerrno), err);
        }
    }
}
