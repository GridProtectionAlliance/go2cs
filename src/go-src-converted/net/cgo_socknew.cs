// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build cgo,!netgo
// +build android linux solaris

// package net -- go2cs converted at 2020 August 29 08:25:06 UTC
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
        private static ref C.struct_sockaddr cgoSockaddrInet4(IP ip)
        {
            syscall.RawSockaddrInet4 sa = new syscall.RawSockaddrInet4(Family:syscall.AF_INET);
            copy(sa.Addr[..], ip);
            return (C.struct_sockaddr.Value)(@unsafe.Pointer(ref sa));
        }

        private static ref C.struct_sockaddr cgoSockaddrInet6(IP ip, long zone)
        {
            syscall.RawSockaddrInet6 sa = new syscall.RawSockaddrInet6(Family:syscall.AF_INET6,Scope_id:uint32(zone));
            copy(sa.Addr[..], ip);
            return (C.struct_sockaddr.Value)(@unsafe.Pointer(ref sa));
        }
    }
}
