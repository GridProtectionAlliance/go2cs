// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package binary -- go2cs converted at 2022 March 13 05:28:39 UTC
// import "encoding/binary" ==> using binary = go.encoding.binary_package
// Original source: C:\Program Files\Go\src\encoding\binary\varint.go
namespace go.encoding;
// This file implements "varint" encoding of 64-bit integers.
// The encoding is:
// - unsigned integers are serialized 7 bits at a time, starting with the
//   least significant bits
// - the most significant bit (msb) in each output byte indicates if there
//   is a continuation byte (msb = 1)
// - signed integers are mapped to unsigned integers using "zig-zag"
//   encoding: Positive values x are written as 2*x + 0, negative values
//   are written as 2*(^x) + 1; that is, negative numbers are complemented
//   and whether to complement is encoded in bit 0.
//
// Design note:
// At most 10 bytes are needed for 64-bit values. The encoding could
// be more dense: a full 64-bit value needs an extra byte just to hold bit 63.
// Instead, the msb of the previous byte could be used to hold bit 63 since we
// know there can't be more than 64 bits. This is a trivial improvement and
// would reduce the maximum encoding length to 9 bytes. However, it breaks the
// invariant that the msb is always the "continuation bit" and thus makes the
// format incompatible with a varint encoding for larger numbers (say 128-bit).


using errors = errors_package;
using io = io_package;


// MaxVarintLenN is the maximum length of a varint-encoded N-bit integer.

public static partial class binary_package {

public static readonly nint MaxVarintLen16 = 3;
public static readonly nint MaxVarintLen32 = 5;
public static readonly nint MaxVarintLen64 = 10;

// PutUvarint encodes a uint64 into buf and returns the number of bytes written.
// If the buffer is too small, PutUvarint will panic.
public static nint PutUvarint(slice<byte> buf, ulong x) {
    nint i = 0;
    while (x >= 0x80) {
        buf[i] = byte(x) | 0x80;
        x>>=7;
        i++;
    }
    buf[i] = byte(x);
    return i + 1;
}

// Uvarint decodes a uint64 from buf and returns that value and the
// number of bytes read (> 0). If an error occurred, the value is 0
// and the number of bytes n is <= 0 meaning:
//
//     n == 0: buf too small
//     n  < 0: value larger than 64 bits (overflow)
//             and -n is the number of bytes read
//
public static (ulong, nint) Uvarint(slice<byte> buf) {
    ulong _p0 = default;
    nint _p0 = default;

    ulong x = default;
    nuint s = default;
    foreach (var (i, b) in buf) {
        if (i == MaxVarintLen64) { 
            // Catch byte reads past MaxVarintLen64.
            // See issue https://golang.org/issues/41185
            return (0, -(i + 1)); // overflow
        }
        if (b < 0x80) {
            if (i == MaxVarintLen64 - 1 && b > 1) {
                return (0, -(i + 1)); // overflow
            }
            return (x | uint64(b) << (int)(s), i + 1);
        }
        x |= uint64(b & 0x7f) << (int)(s);
        s += 7;
    }    return (0, 0);
}

// PutVarint encodes an int64 into buf and returns the number of bytes written.
// If the buffer is too small, PutVarint will panic.
public static nint PutVarint(slice<byte> buf, long x) {
    var ux = uint64(x) << 1;
    if (x < 0) {
        ux = ~ux;
    }
    return PutUvarint(buf, ux);
}

// Varint decodes an int64 from buf and returns that value and the
// number of bytes read (> 0). If an error occurred, the value is 0
// and the number of bytes n is <= 0 with the following meaning:
//
//     n == 0: buf too small
//     n  < 0: value larger than 64 bits (overflow)
//             and -n is the number of bytes read
//
public static (long, nint) Varint(slice<byte> buf) {
    long _p0 = default;
    nint _p0 = default;

    var (ux, n) = Uvarint(buf); // ok to continue in presence of error
    var x = int64(ux >> 1);
    if (ux & 1 != 0) {
        x = ~x;
    }
    return (x, n);
}

private static var overflow = errors.New("binary: varint overflows a 64-bit integer");

// ReadUvarint reads an encoded unsigned integer from r and returns it as a uint64.
public static (ulong, error) ReadUvarint(io.ByteReader r) {
    ulong _p0 = default;
    error _p0 = default!;

    ulong x = default;
    nuint s = default;
    for (nint i = 0; i < MaxVarintLen64; i++) {
        var (b, err) = r.ReadByte();
        if (err != null) {
            return (x, error.As(err)!);
        }
        if (b < 0x80) {
            if (i == MaxVarintLen64 - 1 && b > 1) {
                return (x, error.As(overflow)!);
            }
            return (x | uint64(b) << (int)(s), error.As(null!)!);
        }
        x |= uint64(b & 0x7f) << (int)(s);
        s += 7;
    }
    return (x, error.As(overflow)!);
}

// ReadVarint reads an encoded signed integer from r and returns it as an int64.
public static (long, error) ReadVarint(io.ByteReader r) {
    long _p0 = default;
    error _p0 = default!;

    var (ux, err) = ReadUvarint(r); // ok to continue in presence of error
    var x = int64(ux >> 1);
    if (ux & 1 != 0) {
        x = ~x;
    }
    return (x, error.As(err)!);
}

} // end binary_package
