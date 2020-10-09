// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build cgo,!netgo
// +build android linux solaris

// package net -- go2cs converted at 2020 October 09 04:50:26 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\cgo_socknew.go
/*
#include <sys/types.h>
#include <sys/socket.h>

#include <netinet/in.h>
*/
using C = go.C_package;/*
#include <sys/types.h>
#include <sys/socket.h>

#include <netinet/in.h>
*/


using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static ptr<C.struct_sockaddr> cgoSockaddrInet4(IP ip)
        {
            ref syscall.RawSockaddrInet4 sa = ref heap(new syscall.RawSockaddrInet4(Family:syscall.AF_INET), out ptr<syscall.RawSockaddrInet4> _addr_sa);
            copy(sa.Addr[..], ip);
            return _addr_(C.struct_sockaddr.val)(@unsafe.Pointer(_addr_sa))!;
        }

        private static ptr<C.struct_sockaddr> cgoSockaddrInet6(IP ip, long zone)
        {
            ref syscall.RawSockaddrInet6 sa = ref heap(new syscall.RawSockaddrInet6(Family:syscall.AF_INET6,Scope_id:uint32(zone)), out ptr<syscall.RawSockaddrInet6> _addr_sa);
            copy(sa.Addr[..], ip);
            return _addr_(C.struct_sockaddr.val)(@unsafe.Pointer(_addr_sa))!;
        }
    }
}
