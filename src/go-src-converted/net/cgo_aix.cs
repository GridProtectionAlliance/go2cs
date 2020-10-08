// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build cgo,!netgo

// package net -- go2cs converted at 2020 October 08 03:31:09 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\cgo_aix.go
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
        private static readonly var cgoAddrInfoFlags = (var)C.AI_CANONNAME;



        private static (long, error) cgoNameinfoPTR(slice<byte> b, ptr<C.struct_sockaddr> _addr_sa, C.socklen_t salen)
        {
            long _p0 = default;
            error _p0 = default!;
            ref C.struct_sockaddr sa = ref _addr_sa.val;

            var (gerrno, err) = C.getnameinfo(sa, C.size_t(salen), (C.@char.val)(@unsafe.Pointer(_addr_b[0L])), C.size_t(len(b)), null, 0L, C.NI_NAMEREQD);
            return (int(gerrno), error.As(err)!);
        }
    }
}
