// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package byteorder provides functions for decoding and encoding
// little and big endian integer types from/to byte slices.
namespace go.@internal;

partial class byteorder_package {

public static uint16 LeUint16(slice<byte> b) {
    _ = b[1];
    // bounds check hint to compiler; see golang.org/issue/14808
    return (uint16)(((uint16)b[0]) | ((uint16)b[1]) << (int)(8));
}

public static void LePutUint16(slice<byte> b, uint16 v) {
    _ = b[1];
    // early bounds check to guarantee safety of writes below
    b[0] = ((byte)v);
    b[1] = ((byte)(v >> (int)(8)));
}

public static slice<byte> LeAppendUint16(slice<byte> b, uint16 v) {
    return append(b,
        ((byte)v),
        ((byte)(v >> (int)(8))));
}

public static uint32 LeUint32(slice<byte> b) {
    _ = b[3];
    // bounds check hint to compiler; see golang.org/issue/14808
    return (uint32)((uint32)((uint32)(((uint32)b[0]) | ((uint32)b[1]) << (int)(8)) | ((uint32)b[2]) << (int)(16)) | ((uint32)b[3]) << (int)(24));
}

public static void LePutUint32(slice<byte> b, uint32 v) {
    _ = b[3];
    // early bounds check to guarantee safety of writes below
    b[0] = ((byte)v);
    b[1] = ((byte)(v >> (int)(8)));
    b[2] = ((byte)(v >> (int)(16)));
    b[3] = ((byte)(v >> (int)(24)));
}

public static slice<byte> LeAppendUint32(slice<byte> b, uint32 v) {
    return append(b,
        ((byte)v),
        ((byte)(v >> (int)(8))),
        ((byte)(v >> (int)(16))),
        ((byte)(v >> (int)(24))));
}

public static uint64 LeUint64(slice<byte> b) {
    _ = b[7];
    // bounds check hint to compiler; see golang.org/issue/14808
    return (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)(((uint64)b[0]) | ((uint64)b[1]) << (int)(8)) | ((uint64)b[2]) << (int)(16)) | ((uint64)b[3]) << (int)(24)) | ((uint64)b[4]) << (int)(32)) | ((uint64)b[5]) << (int)(40)) | ((uint64)b[6]) << (int)(48)) | ((uint64)b[7]) << (int)(56));
}

public static void LePutUint64(slice<byte> b, uint64 v) {
    _ = b[7];
    // early bounds check to guarantee safety of writes below
    b[0] = ((byte)v);
    b[1] = ((byte)(v >> (int)(8)));
    b[2] = ((byte)(v >> (int)(16)));
    b[3] = ((byte)(v >> (int)(24)));
    b[4] = ((byte)(v >> (int)(32)));
    b[5] = ((byte)(v >> (int)(40)));
    b[6] = ((byte)(v >> (int)(48)));
    b[7] = ((byte)(v >> (int)(56)));
}

public static slice<byte> LeAppendUint64(slice<byte> b, uint64 v) {
    return append(b,
        ((byte)v),
        ((byte)(v >> (int)(8))),
        ((byte)(v >> (int)(16))),
        ((byte)(v >> (int)(24))),
        ((byte)(v >> (int)(32))),
        ((byte)(v >> (int)(40))),
        ((byte)(v >> (int)(48))),
        ((byte)(v >> (int)(56))));
}

public static uint16 BeUint16(slice<byte> b) {
    _ = b[1];
    // bounds check hint to compiler; see golang.org/issue/14808
    return (uint16)(((uint16)b[1]) | ((uint16)b[0]) << (int)(8));
}

public static void BePutUint16(slice<byte> b, uint16 v) {
    _ = b[1];
    // early bounds check to guarantee safety of writes below
    b[0] = ((byte)(v >> (int)(8)));
    b[1] = ((byte)v);
}

public static slice<byte> BeAppendUint16(slice<byte> b, uint16 v) {
    return append(b,
        ((byte)(v >> (int)(8))),
        ((byte)v));
}

public static uint32 BeUint32(slice<byte> b) {
    _ = b[3];
    // bounds check hint to compiler; see golang.org/issue/14808
    return (uint32)((uint32)((uint32)(((uint32)b[3]) | ((uint32)b[2]) << (int)(8)) | ((uint32)b[1]) << (int)(16)) | ((uint32)b[0]) << (int)(24));
}

public static void BePutUint32(slice<byte> b, uint32 v) {
    _ = b[3];
    // early bounds check to guarantee safety of writes below
    b[0] = ((byte)(v >> (int)(24)));
    b[1] = ((byte)(v >> (int)(16)));
    b[2] = ((byte)(v >> (int)(8)));
    b[3] = ((byte)v);
}

public static slice<byte> BeAppendUint32(slice<byte> b, uint32 v) {
    return append(b,
        ((byte)(v >> (int)(24))),
        ((byte)(v >> (int)(16))),
        ((byte)(v >> (int)(8))),
        ((byte)v));
}

public static uint64 BeUint64(slice<byte> b) {
    _ = b[7];
    // bounds check hint to compiler; see golang.org/issue/14808
    return (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)(((uint64)b[7]) | ((uint64)b[6]) << (int)(8)) | ((uint64)b[5]) << (int)(16)) | ((uint64)b[4]) << (int)(24)) | ((uint64)b[3]) << (int)(32)) | ((uint64)b[2]) << (int)(40)) | ((uint64)b[1]) << (int)(48)) | ((uint64)b[0]) << (int)(56));
}

public static void BePutUint64(slice<byte> b, uint64 v) {
    _ = b[7];
    // early bounds check to guarantee safety of writes below
    b[0] = ((byte)(v >> (int)(56)));
    b[1] = ((byte)(v >> (int)(48)));
    b[2] = ((byte)(v >> (int)(40)));
    b[3] = ((byte)(v >> (int)(32)));
    b[4] = ((byte)(v >> (int)(24)));
    b[5] = ((byte)(v >> (int)(16)));
    b[6] = ((byte)(v >> (int)(8)));
    b[7] = ((byte)v);
}

public static slice<byte> BeAppendUint64(slice<byte> b, uint64 v) {
    return append(b,
        ((byte)(v >> (int)(56))),
        ((byte)(v >> (int)(48))),
        ((byte)(v >> (int)(40))),
        ((byte)(v >> (int)(32))),
        ((byte)(v >> (int)(24))),
        ((byte)(v >> (int)(16))),
        ((byte)(v >> (int)(8))),
        ((byte)v));
}

} // end byteorder_package
