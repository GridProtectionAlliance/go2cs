// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build solaris

// package lif -- go2cs converted at 2020 October 09 04:51:48 UTC
// import "golang.org/x/net/lif" ==> using lif = go.golang.org.x.net.lif_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\lif\binary.go

using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace net
{
    public static partial class lif_package
    {
        // This file contains duplicates of encoding/binary package.
        //
        // This package is supposed to be used by the net package of standard
        // library. Therefore the package set used in the package must be the
        // same as net package.
        private static binaryLittleEndian littleEndian = default;        private static binaryBigEndian bigEndian = default;

        private partial interface binaryByteOrder
        {
            ulong Uint16(slice<byte> _p0);
            ulong Uint32(slice<byte> _p0);
            ulong Uint64(slice<byte> _p0);
            ulong PutUint16(slice<byte> _p0, ushort _p0);
            ulong PutUint32(slice<byte> _p0, uint _p0);
            ulong PutUint64(slice<byte> _p0, ulong _p0);
        }

        private partial struct binaryLittleEndian
        {
        }

        private static ushort Uint16(this binaryLittleEndian _p0, slice<byte> b)
        {
            _ = b[1L]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint16(b[0L]) | uint16(b[1L]) << (int)(8L);

        }

        private static void PutUint16(this binaryLittleEndian _p0, slice<byte> b, ushort v)
        {
            _ = b[1L]; // early bounds check to guarantee safety of writes below
            b[0L] = byte(v);
            b[1L] = byte(v >> (int)(8L));

        }

        private static uint Uint32(this binaryLittleEndian _p0, slice<byte> b)
        {
            _ = b[3L]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint32(b[0L]) | uint32(b[1L]) << (int)(8L) | uint32(b[2L]) << (int)(16L) | uint32(b[3L]) << (int)(24L);

        }

        private static void PutUint32(this binaryLittleEndian _p0, slice<byte> b, uint v)
        {
            _ = b[3L]; // early bounds check to guarantee safety of writes below
            b[0L] = byte(v);
            b[1L] = byte(v >> (int)(8L));
            b[2L] = byte(v >> (int)(16L));
            b[3L] = byte(v >> (int)(24L));

        }

        private static ulong Uint64(this binaryLittleEndian _p0, slice<byte> b)
        {
            _ = b[7L]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint64(b[0L]) | uint64(b[1L]) << (int)(8L) | uint64(b[2L]) << (int)(16L) | uint64(b[3L]) << (int)(24L) | uint64(b[4L]) << (int)(32L) | uint64(b[5L]) << (int)(40L) | uint64(b[6L]) << (int)(48L) | uint64(b[7L]) << (int)(56L);

        }

        private static void PutUint64(this binaryLittleEndian _p0, slice<byte> b, ulong v)
        {
            _ = b[7L]; // early bounds check to guarantee safety of writes below
            b[0L] = byte(v);
            b[1L] = byte(v >> (int)(8L));
            b[2L] = byte(v >> (int)(16L));
            b[3L] = byte(v >> (int)(24L));
            b[4L] = byte(v >> (int)(32L));
            b[5L] = byte(v >> (int)(40L));
            b[6L] = byte(v >> (int)(48L));
            b[7L] = byte(v >> (int)(56L));

        }

        private partial struct binaryBigEndian
        {
        }

        private static ushort Uint16(this binaryBigEndian _p0, slice<byte> b)
        {
            _ = b[1L]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint16(b[1L]) | uint16(b[0L]) << (int)(8L);

        }

        private static void PutUint16(this binaryBigEndian _p0, slice<byte> b, ushort v)
        {
            _ = b[1L]; // early bounds check to guarantee safety of writes below
            b[0L] = byte(v >> (int)(8L));
            b[1L] = byte(v);

        }

        private static uint Uint32(this binaryBigEndian _p0, slice<byte> b)
        {
            _ = b[3L]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint32(b[3L]) | uint32(b[2L]) << (int)(8L) | uint32(b[1L]) << (int)(16L) | uint32(b[0L]) << (int)(24L);

        }

        private static void PutUint32(this binaryBigEndian _p0, slice<byte> b, uint v)
        {
            _ = b[3L]; // early bounds check to guarantee safety of writes below
            b[0L] = byte(v >> (int)(24L));
            b[1L] = byte(v >> (int)(16L));
            b[2L] = byte(v >> (int)(8L));
            b[3L] = byte(v);

        }

        private static ulong Uint64(this binaryBigEndian _p0, slice<byte> b)
        {
            _ = b[7L]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint64(b[7L]) | uint64(b[6L]) << (int)(8L) | uint64(b[5L]) << (int)(16L) | uint64(b[4L]) << (int)(24L) | uint64(b[3L]) << (int)(32L) | uint64(b[2L]) << (int)(40L) | uint64(b[1L]) << (int)(48L) | uint64(b[0L]) << (int)(56L);

        }

        private static void PutUint64(this binaryBigEndian _p0, slice<byte> b, ulong v)
        {
            _ = b[7L]; // early bounds check to guarantee safety of writes below
            b[0L] = byte(v >> (int)(56L));
            b[1L] = byte(v >> (int)(48L));
            b[2L] = byte(v >> (int)(40L));
            b[3L] = byte(v >> (int)(32L));
            b[4L] = byte(v >> (int)(24L));
            b[5L] = byte(v >> (int)(16L));
            b[6L] = byte(v >> (int)(8L));
            b[7L] = byte(v);

        }
    }
}}}}
