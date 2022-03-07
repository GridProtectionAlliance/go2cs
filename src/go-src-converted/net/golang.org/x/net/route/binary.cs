// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// package route -- go2cs converted at 2022 March 06 22:15:55 UTC
// import "golang.org/x/net/route" ==> using route = go.golang.org.x.net.route_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\route\binary.go


namespace go.golang.org.x.net;

public static partial class route_package {

    // This file contains duplicates of encoding/binary package.
    //
    // This package is supposed to be used by the net package of standard
    // library. Therefore the package set used in the package must be the
    // same as net package.
private static binaryLittleEndian littleEndian = default;private static binaryBigEndian bigEndian = default;

private partial interface binaryByteOrder {
    ulong Uint16(slice<byte> _p0);
    ulong Uint32(slice<byte> _p0);
    ulong PutUint16(slice<byte> _p0, ushort _p0);
    ulong PutUint32(slice<byte> _p0, uint _p0);
    ulong Uint64(slice<byte> _p0);
}

private partial struct binaryLittleEndian {
}

private static ushort Uint16(this binaryLittleEndian _p0, slice<byte> b) {
    _ = b[1]; // bounds check hint to compiler; see golang.org/issue/14808
    return uint16(b[0]) | uint16(b[1]) << 8;

}

private static void PutUint16(this binaryLittleEndian _p0, slice<byte> b, ushort v) {
    _ = b[1]; // early bounds check to guarantee safety of writes below
    b[0] = byte(v);
    b[1] = byte(v >> 8);

}

private static uint Uint32(this binaryLittleEndian _p0, slice<byte> b) {
    _ = b[3]; // bounds check hint to compiler; see golang.org/issue/14808
    return uint32(b[0]) | uint32(b[1]) << 8 | uint32(b[2]) << 16 | uint32(b[3]) << 24;

}

private static void PutUint32(this binaryLittleEndian _p0, slice<byte> b, uint v) {
    _ = b[3]; // early bounds check to guarantee safety of writes below
    b[0] = byte(v);
    b[1] = byte(v >> 8);
    b[2] = byte(v >> 16);
    b[3] = byte(v >> 24);

}

private static ulong Uint64(this binaryLittleEndian _p0, slice<byte> b) {
    _ = b[7]; // bounds check hint to compiler; see golang.org/issue/14808
    return uint64(b[0]) | uint64(b[1]) << 8 | uint64(b[2]) << 16 | uint64(b[3]) << 24 | uint64(b[4]) << 32 | uint64(b[5]) << 40 | uint64(b[6]) << 48 | uint64(b[7]) << 56;

}

private partial struct binaryBigEndian {
}

private static ushort Uint16(this binaryBigEndian _p0, slice<byte> b) {
    _ = b[1]; // bounds check hint to compiler; see golang.org/issue/14808
    return uint16(b[1]) | uint16(b[0]) << 8;

}

private static void PutUint16(this binaryBigEndian _p0, slice<byte> b, ushort v) {
    _ = b[1]; // early bounds check to guarantee safety of writes below
    b[0] = byte(v >> 8);
    b[1] = byte(v);

}

private static uint Uint32(this binaryBigEndian _p0, slice<byte> b) {
    _ = b[3]; // bounds check hint to compiler; see golang.org/issue/14808
    return uint32(b[3]) | uint32(b[2]) << 8 | uint32(b[1]) << 16 | uint32(b[0]) << 24;

}

private static void PutUint32(this binaryBigEndian _p0, slice<byte> b, uint v) {
    _ = b[3]; // early bounds check to guarantee safety of writes below
    b[0] = byte(v >> 24);
    b[1] = byte(v >> 16);
    b[2] = byte(v >> 8);
    b[3] = byte(v);

}

private static ulong Uint64(this binaryBigEndian _p0, slice<byte> b) {
    _ = b[7]; // bounds check hint to compiler; see golang.org/issue/14808
    return uint64(b[7]) | uint64(b[6]) << 8 | uint64(b[5]) << 16 | uint64(b[4]) << 24 | uint64(b[3]) << 32 | uint64(b[2]) << 40 | uint64(b[1]) << 48 | uint64(b[0]) << 56;

}

} // end route_package
