// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build arm mips mipsle mips64 mips64le

// package runtime -- go2cs converted at 2020 August 29 08:21:34 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\unaligned2.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Note: These routines perform the read with an unspecified endianness.
        private static uint readUnaligned32(unsafe.Pointer p)
        {
            ref array<byte> q = new ptr<ref array<byte>>(p);
            return uint32(q[0L]) + uint32(q[1L]) << (int)(8L) + uint32(q[2L]) << (int)(16L) + uint32(q[3L]) << (int)(24L);
        }

        private static ulong readUnaligned64(unsafe.Pointer p)
        {
            ref array<byte> q = new ptr<ref array<byte>>(p);
            return uint64(q[0L]) + uint64(q[1L]) << (int)(8L) + uint64(q[2L]) << (int)(16L) + uint64(q[3L]) << (int)(24L) + uint64(q[4L]) << (int)(32L) + uint64(q[5L]) << (int)(40L) + uint64(q[6L]) << (int)(48L) + uint64(q[7L]) << (int)(56L);
        }
    }
}
