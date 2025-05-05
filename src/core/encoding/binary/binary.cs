// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package binary implements simple translation between numbers and byte
// sequences and encoding and decoding of varints.
//
// Numbers are translated by reading and writing fixed-size values.
// A fixed-size value is either a fixed-size arithmetic
// type (bool, int8, uint8, int16, float32, complex64, ...)
// or an array or struct containing only fixed-size values.
//
// The varint functions encode and decode single integer values using
// a variable-length encoding; smaller values require fewer bytes.
// For a specification, see
// https://developers.google.com/protocol-buffers/docs/encoding.
//
// This package favors simplicity over efficiency. Clients that require
// high-performance serialization, especially for large data structures,
// should look at more advanced solutions such as the [encoding/gob]
// package or [google.golang.org/protobuf] for protocol buffers.
namespace go.encoding;

using errors = errors_package;
using io = io_package;
using math = math_package;
using reflect = reflect_package;
using slices = slices_package;
using sync = sync_package;

partial class binary_package {

internal static error errBufferTooSmall = errors.New("buffer too small"u8);

// A ByteOrder specifies how to convert byte slices into
// 16-, 32-, or 64-bit unsigned integers.
//
// It is implemented by [LittleEndian], [BigEndian], and [NativeEndian].
[GoType] partial interface ByteOrder {
    uint16 Uint16(slice<byte> _);
    uint32 Uint32(slice<byte> _);
    uint64 Uint64(slice<byte> _);
    void PutUint16(slice<byte> _, uint16 _);
    void PutUint32(slice<byte> _, uint32 _);
    void PutUint64(slice<byte> _, uint64 _);
    @string String();
}

// AppendByteOrder specifies how to append 16-, 32-, or 64-bit unsigned integers
// into a byte slice.
//
// It is implemented by [LittleEndian], [BigEndian], and [NativeEndian].
[GoType] partial interface AppendByteOrder {
    slice<byte> AppendUint16(slice<byte> _, uint16 _);
    slice<byte> AppendUint32(slice<byte> _, uint32 _);
    slice<byte> AppendUint64(slice<byte> _, uint64 _);
    @string String();
}

// LittleEndian is the little-endian implementation of [ByteOrder] and [AppendByteOrder].
public static littleEndian LittleEndian;

// BigEndian is the big-endian implementation of [ByteOrder] and [AppendByteOrder].
public static bigEndian BigEndian;

[GoType] partial struct littleEndian {
}

internal static uint16 Uint16(this littleEndian _, slice<byte> b) {
    _ = b[1];
    // bounds check hint to compiler; see golang.org/issue/14808
    return (uint16)(((uint16)b[0]) | ((uint16)b[1]) << (int)(8));
}

internal static void PutUint16(this littleEndian _, slice<byte> b, uint16 v) {
    _ = b[1];
    // early bounds check to guarantee safety of writes below
    b[0] = ((byte)v);
    b[1] = ((byte)(v >> (int)(8)));
}

internal static slice<byte> AppendUint16(this littleEndian _, slice<byte> b, uint16 v) {
    return append(b,
        ((byte)v),
        ((byte)(v >> (int)(8))));
}

internal static uint32 Uint32(this littleEndian _, slice<byte> b) {
    _ = b[3];
    // bounds check hint to compiler; see golang.org/issue/14808
    return (uint32)((uint32)((uint32)(((uint32)b[0]) | ((uint32)b[1]) << (int)(8)) | ((uint32)b[2]) << (int)(16)) | ((uint32)b[3]) << (int)(24));
}

internal static void PutUint32(this littleEndian _, slice<byte> b, uint32 v) {
    _ = b[3];
    // early bounds check to guarantee safety of writes below
    b[0] = ((byte)v);
    b[1] = ((byte)(v >> (int)(8)));
    b[2] = ((byte)(v >> (int)(16)));
    b[3] = ((byte)(v >> (int)(24)));
}

internal static slice<byte> AppendUint32(this littleEndian _, slice<byte> b, uint32 v) {
    return append(b,
        ((byte)v),
        ((byte)(v >> (int)(8))),
        ((byte)(v >> (int)(16))),
        ((byte)(v >> (int)(24))));
}

internal static uint64 Uint64(this littleEndian _, slice<byte> b) {
    _ = b[7];
    // bounds check hint to compiler; see golang.org/issue/14808
    return (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)(((uint64)b[0]) | ((uint64)b[1]) << (int)(8)) | ((uint64)b[2]) << (int)(16)) | ((uint64)b[3]) << (int)(24)) | ((uint64)b[4]) << (int)(32)) | ((uint64)b[5]) << (int)(40)) | ((uint64)b[6]) << (int)(48)) | ((uint64)b[7]) << (int)(56));
}

internal static void PutUint64(this littleEndian _, slice<byte> b, uint64 v) {
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

internal static slice<byte> AppendUint64(this littleEndian _, slice<byte> b, uint64 v) {
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

internal static @string String(this littleEndian _) {
    return "LittleEndian"u8;
}

internal static @string GoString(this littleEndian _) {
    return "binary.LittleEndian"u8;
}

[GoType] partial struct bigEndian {
}

internal static uint16 Uint16(this bigEndian _, slice<byte> b) {
    _ = b[1];
    // bounds check hint to compiler; see golang.org/issue/14808
    return (uint16)(((uint16)b[1]) | ((uint16)b[0]) << (int)(8));
}

internal static void PutUint16(this bigEndian _, slice<byte> b, uint16 v) {
    _ = b[1];
    // early bounds check to guarantee safety of writes below
    b[0] = ((byte)(v >> (int)(8)));
    b[1] = ((byte)v);
}

internal static slice<byte> AppendUint16(this bigEndian _, slice<byte> b, uint16 v) {
    return append(b,
        ((byte)(v >> (int)(8))),
        ((byte)v));
}

internal static uint32 Uint32(this bigEndian _, slice<byte> b) {
    _ = b[3];
    // bounds check hint to compiler; see golang.org/issue/14808
    return (uint32)((uint32)((uint32)(((uint32)b[3]) | ((uint32)b[2]) << (int)(8)) | ((uint32)b[1]) << (int)(16)) | ((uint32)b[0]) << (int)(24));
}

internal static void PutUint32(this bigEndian _, slice<byte> b, uint32 v) {
    _ = b[3];
    // early bounds check to guarantee safety of writes below
    b[0] = ((byte)(v >> (int)(24)));
    b[1] = ((byte)(v >> (int)(16)));
    b[2] = ((byte)(v >> (int)(8)));
    b[3] = ((byte)v);
}

internal static slice<byte> AppendUint32(this bigEndian _, slice<byte> b, uint32 v) {
    return append(b,
        ((byte)(v >> (int)(24))),
        ((byte)(v >> (int)(16))),
        ((byte)(v >> (int)(8))),
        ((byte)v));
}

internal static uint64 Uint64(this bigEndian _, slice<byte> b) {
    _ = b[7];
    // bounds check hint to compiler; see golang.org/issue/14808
    return (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)(((uint64)b[7]) | ((uint64)b[6]) << (int)(8)) | ((uint64)b[5]) << (int)(16)) | ((uint64)b[4]) << (int)(24)) | ((uint64)b[3]) << (int)(32)) | ((uint64)b[2]) << (int)(40)) | ((uint64)b[1]) << (int)(48)) | ((uint64)b[0]) << (int)(56));
}

internal static void PutUint64(this bigEndian _, slice<byte> b, uint64 v) {
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

internal static slice<byte> AppendUint64(this bigEndian _, slice<byte> b, uint64 v) {
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

internal static @string String(this bigEndian _) {
    return "BigEndian"u8;
}

internal static @string GoString(this bigEndian _) {
    return "binary.BigEndian"u8;
}

internal static @string String(this nativeEndian _) {
    return "NativeEndian"u8;
}

internal static @string GoString(this nativeEndian _) {
    return "binary.NativeEndian"u8;
}

// Read reads structured binary data from r into data.
// Data must be a pointer to a fixed-size value or a slice
// of fixed-size values.
// Bytes read from r are decoded using the specified byte order
// and written to successive fields of the data.
// When decoding boolean values, a zero byte is decoded as false, and
// any other non-zero byte is decoded as true.
// When reading into structs, the field data for fields with
// blank (_) field names is skipped; i.e., blank field names
// may be used for padding.
// When reading into a struct, all non-blank fields must be exported
// or Read may panic.
//
// The error is [io.EOF] only if no bytes were read.
// If an [io.EOF] happens after reading some but not all the bytes,
// Read returns [io.ErrUnexpectedEOF].
public static error Read(io.Reader r, ByteOrder order, any data) {
    // Fast path for basic types and slices.
    {
        var (n, _) = intDataSize(data); if (n != 0) {
            var bs = new slice<byte>(n);
            {
                var (_, err) = io.ReadFull(r, bs); if (err != default!) {
                    return err;
                }
            }
            if (decodeFast(bs, order, data)) {
                return default!;
            }
        }
    }
    // Fallback to reflect-based decoding.
    var v = reflect.ValueOf(data);
    nint size = -1;
    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == reflect.ΔPointer) {
        v = v.Elem();
        size = dataSize(v);
    }
    else if (exprᴛ1 == reflect.ΔSlice) {
        size = dataSize(v);
    }

    if (size < 0) {
        return errors.New("binary.Read: invalid type "u8 + reflect.TypeOf(data).String());
    }
    var d = Ꮡ(new decoder(order: order, buf: new slice<byte>(size)));
    {
        var (_, err) = io.ReadFull(r, (~d).buf); if (err != default!) {
            return err;
        }
    }
    d.value(v);
    return default!;
}

// Decode decodes binary data from buf into data according to
// the given byte order.
// It returns an error if buf is too small, otherwise the number of
// bytes consumed from buf.
public static (nint, error) Decode(slice<byte> buf, ByteOrder order, any data) {
    {
        var (n, _) = intDataSize(data); if (n != 0) {
            if (len(buf) < n) {
                return (0, errBufferTooSmall);
            }
            if (decodeFast(buf, order, data)) {
                return (n, default!);
            }
        }
    }
    // Fallback to reflect-based decoding.
    var v = reflect.ValueOf(data);
    nint size = -1;
    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == reflect.ΔPointer) {
        v = v.Elem();
        size = dataSize(v);
    }
    else if (exprᴛ1 == reflect.ΔSlice) {
        size = dataSize(v);
    }

    if (size < 0) {
        return (0, errors.New("binary.Decode: invalid type "u8 + reflect.TypeOf(data).String()));
    }
    if (len(buf) < size) {
        return (0, errBufferTooSmall);
    }
    var d = Ꮡ(new decoder(order: order, buf: buf[..(int)(size)]));
    d.value(v);
    return (size, default!);
}

internal static bool decodeFast(slice<byte> bs, ByteOrder order, any data) {
    switch (data.type()) {
    case @bool.val data: {
        data = bs[0] != 0;
        break;
    }
    case int8.val data: {
        data = ((int8)bs[0]);
        break;
    }
    case uint8.val data: {
        data = bs[0];
        break;
    }
    case int16.val data: {
        data = ((int16)order.Uint16(bs));
        break;
    }
    case uint16.val data: {
        data = order.Uint16(bs);
        break;
    }
    case int32.val data: {
        data = ((int32)order.Uint32(bs));
        break;
    }
    case uint32.val data: {
        data = order.Uint32(bs);
        break;
    }
    case int64.val data: {
        data = ((int64)order.Uint64(bs));
        break;
    }
    case uint64.val data: {
        data = order.Uint64(bs);
        break;
    }
    case float32.val data: {
        data = math.Float32frombits(order.Uint32(bs));
        break;
    }
    case float64.val data: {
        data = math.Float64frombits(order.Uint64(bs));
        break;
    }
    case slice<bool> data: {
        foreach (var (i, x) in bs) {
            // Easier to loop over the input for 8-bit values.
            data[i] = x != 0;
        }
        break;
    }
    case slice<int8> data: {
        foreach (var (i, x) in bs) {
            data[i] = ((int8)x);
        }
        break;
    }
    case slice<uint8> data: {
        copy(data, bs);
        break;
    }
    case slice<int16> data: {
        foreach (var (i, _) in data) {
            data[i] = ((int16)order.Uint16(bs[(int)(2 * i)..]));
        }
        break;
    }
    case slice<uint16> data: {
        foreach (var (i, _) in data) {
            data[i] = order.Uint16(bs[(int)(2 * i)..]);
        }
        break;
    }
    case slice<int32> data: {
        foreach (var (i, _) in data) {
            data[i] = ((int32)order.Uint32(bs[(int)(4 * i)..]));
        }
        break;
    }
    case slice<uint32> data: {
        foreach (var (i, _) in data) {
            data[i] = order.Uint32(bs[(int)(4 * i)..]);
        }
        break;
    }
    case slice<int64> data: {
        foreach (var (i, _) in data) {
            data[i] = ((int64)order.Uint64(bs[(int)(8 * i)..]));
        }
        break;
    }
    case slice<uint64> data: {
        foreach (var (i, _) in data) {
            data[i] = order.Uint64(bs[(int)(8 * i)..]);
        }
        break;
    }
    case slice<float32> data: {
        foreach (var (i, _) in data) {
            data[i] = math.Float32frombits(order.Uint32(bs[(int)(4 * i)..]));
        }
        break;
    }
    case slice<float64> data: {
        foreach (var (i, _) in data) {
            data[i] = math.Float64frombits(order.Uint64(bs[(int)(8 * i)..]));
        }
        break;
    }
    default: {
        var data = data.type();
        return false;
    }}
    return true;
}

// Write writes the binary representation of data into w.
// Data must be a fixed-size value or a slice of fixed-size
// values, or a pointer to such data.
// Boolean values encode as one byte: 1 for true, and 0 for false.
// Bytes written to w are encoded using the specified byte order
// and read from successive fields of the data.
// When writing structs, zero values are written for fields
// with blank (_) field names.
public static error Write(io.Writer w, ByteOrder order, any data) {
    // Fast path for basic types and slices.
    {
        var (n, bs) = intDataSize(data); if (n != 0) {
            if (bs == default!) {
                bs = new slice<byte>(n);
                encodeFast(bs, order, data);
            }
            var (_, errΔ1) = w.Write(bs);
            return errΔ1;
        }
    }
    // Fallback to reflect-based encoding.
    var v = reflect.Indirect(reflect.ValueOf(data));
    nint size = dataSize(v);
    if (size < 0) {
        return errors.New("binary.Write: some values are not fixed-sized in type "u8 + reflect.TypeOf(data).String());
    }
    var buf = new slice<byte>(size);
    var e = Ꮡ(new encoder(order: order, buf: buf));
    e.value(v);
    var (_, err) = w.Write(buf);
    return err;
}

// Encode encodes the binary representation of data into buf according to
// the given byte order.
// It returns an error if buf is too small, otherwise the number of
// bytes written into buf.
public static (nint, error) Encode(slice<byte> buf, ByteOrder order, any data) {
    // Fast path for basic types and slices.
    {
        var (n, _) = intDataSize(data); if (n != 0) {
            if (len(buf) < n) {
                return (0, errBufferTooSmall);
            }
            encodeFast(buf, order, data);
            return (n, default!);
        }
    }
    // Fallback to reflect-based encoding.
    var v = reflect.Indirect(reflect.ValueOf(data));
    nint size = dataSize(v);
    if (size < 0) {
        return (0, errors.New("binary.Encode: some values are not fixed-sized in type "u8 + reflect.TypeOf(data).String()));
    }
    if (len(buf) < size) {
        return (0, errBufferTooSmall);
    }
    var e = Ꮡ(new encoder(order: order, buf: buf));
    e.value(v);
    return (size, default!);
}

// Append appends the binary representation of data to buf.
// buf may be nil, in which case a new buffer will be allocated.
// See [Write] on which data are acceptable.
// It returns the (possibily extended) buffer containing data or an error.
public static (slice<byte>, error) Append(slice<byte> buf, ByteOrder order, any data) {
    // Fast path for basic types and slices.
    {
        var (n, _) = intDataSize(data); if (n != 0) {
            (bufΔ1, posΔ1) = ensure(buf, n);
            encodeFast(posΔ1, order, data);
            return (bufΔ1, default!);
        }
    }
    // Fallback to reflect-based encoding.
    var v = reflect.Indirect(reflect.ValueOf(data));
    nint size = dataSize(v);
    if (size < 0) {
        return (default!, errors.New("binary.Append: some values are not fixed-sized in type "u8 + reflect.TypeOf(data).String()));
    }
    (buf, pos) = ensure(buf, size);
    var e = Ꮡ(new encoder(order: order, buf: pos));
    e.value(v);
    return (buf, default!);
}

internal static void encodeFast(slice<byte> bs, ByteOrder order, any data) {
    switch (data.type()) {
    case @bool.val v: {
        if (v.val){
            bs[0] = 1;
        } else {
            bs[0] = 0;
        }
        break;
    }
    case bool v: {
        if (v){
            bs[0] = 1;
        } else {
            bs[0] = 0;
        }
        break;
    }
    case slice<bool> v: {
        foreach (var (i, x) in v) {
            if (x){
                bs[i] = 1;
            } else {
                bs[i] = 0;
            }
        }
        break;
    }
    case int8.val v: {
        bs[0] = ((byte)(v.val));
        break;
    }
    case int8 v: {
        bs[0] = ((byte)v);
        break;
    }
    case slice<int8> v: {
        foreach (var (i, x) in v) {
            bs[i] = ((byte)x);
        }
        break;
    }
    case uint8.val v: {
        bs[0] = v.val;
        break;
    }
    case uint8 v: {
        bs[0] = v;
        break;
    }
    case slice<uint8> v: {
        copy(bs, v);
        break;
    }
    case int16.val v: {
        order.PutUint16(bs, ((uint16)(v.val)));
        break;
    }
    case int16 v: {
        order.PutUint16(bs, ((uint16)v));
        break;
    }
    case slice<int16> v: {
        foreach (var (i, x) in v) {
            order.PutUint16(bs[(int)(2 * i)..], ((uint16)x));
        }
        break;
    }
    case uint16.val v: {
        order.PutUint16(bs, v.val);
        break;
    }
    case uint16 v: {
        order.PutUint16(bs, v);
        break;
    }
    case slice<uint16> v: {
        foreach (var (i, x) in v) {
            order.PutUint16(bs[(int)(2 * i)..], x);
        }
        break;
    }
    case int32.val v: {
        order.PutUint32(bs, ((uint32)(v.val)));
        break;
    }
    case int32 v: {
        order.PutUint32(bs, ((uint32)v));
        break;
    }
    case slice<int32> v: {
        foreach (var (i, x) in v) {
            order.PutUint32(bs[(int)(4 * i)..], ((uint32)x));
        }
        break;
    }
    case uint32.val v: {
        order.PutUint32(bs, v.val);
        break;
    }
    case uint32 v: {
        order.PutUint32(bs, v);
        break;
    }
    case slice<uint32> v: {
        foreach (var (i, x) in v) {
            order.PutUint32(bs[(int)(4 * i)..], x);
        }
        break;
    }
    case int64.val v: {
        order.PutUint64(bs, ((uint64)(v.val)));
        break;
    }
    case int64 v: {
        order.PutUint64(bs, ((uint64)v));
        break;
    }
    case slice<int64> v: {
        foreach (var (i, x) in v) {
            order.PutUint64(bs[(int)(8 * i)..], ((uint64)x));
        }
        break;
    }
    case uint64.val v: {
        order.PutUint64(bs, v.val);
        break;
    }
    case uint64 v: {
        order.PutUint64(bs, v);
        break;
    }
    case slice<uint64> v: {
        foreach (var (i, x) in v) {
            order.PutUint64(bs[(int)(8 * i)..], x);
        }
        break;
    }
    case float32.val v: {
        order.PutUint32(bs, math.Float32bits(v.val));
        break;
    }
    case float32 v: {
        order.PutUint32(bs, math.Float32bits(v));
        break;
    }
    case slice<float32> v: {
        foreach (var (i, x) in v) {
            order.PutUint32(bs[(int)(4 * i)..], math.Float32bits(x));
        }
        break;
    }
    case float64.val v: {
        order.PutUint64(bs, math.Float64bits(v.val));
        break;
    }
    case float64 v: {
        order.PutUint64(bs, math.Float64bits(v));
        break;
    }
    case slice<float64> v: {
        foreach (var (i, x) in v) {
            order.PutUint64(bs[(int)(8 * i)..], math.Float64bits(x));
        }
        break;
    }}
}

// Size returns how many bytes [Write] would generate to encode the value v, which
// must be a fixed-size value or a slice of fixed-size values, or a pointer to such data.
// If v is neither of these, Size returns -1.
public static nint Size(any v) {
    switch (v.type()) {
    case bool data: {
        return 1;
    }
    case int8 data: {
        return 1;
    }
    case uint8 data: {
        return 1;
    }
    case @bool.val data: {
        if (data == nil) {
            return -1;
        }
        return 1;
    }
    case int8.val data: {
        if (data == nil) {
            return -1;
        }
        return 1;
    }
    case uint8.val data: {
        if (data == nil) {
            return -1;
        }
        return 1;
    }
    case slice<bool> data: {
        return len(data);
    }
    case slice<int8> data: {
        return len(data);
    }
    case slice<uint8> data: {
        return len(data);
    }
    case int16 data: {
        return 2;
    }
    case uint16 data: {
        return 2;
    }
    case int16.val data: {
        if (data == nil) {
            return -1;
        }
        return 2;
    }
    case uint16.val data: {
        if (data == nil) {
            return -1;
        }
        return 2;
    }
    case slice<int16> data: {
        return 2 * len(data);
    }
    case slice<uint16> data: {
        return 2 * len(data);
    }
    case int32 data: {
        return 4;
    }
    case uint32 data: {
        return 4;
    }
    case int32.val data: {
        if (data == nil) {
            return -1;
        }
        return 4;
    }
    case uint32.val data: {
        if (data == nil) {
            return -1;
        }
        return 4;
    }
    case slice<int32> data: {
        return 4 * len(data);
    }
    case slice<uint32> data: {
        return 4 * len(data);
    }
    case int64 data: {
        return 8;
    }
    case uint64 data: {
        return 8;
    }
    case int64.val data: {
        if (data == nil) {
            return -1;
        }
        return 8;
    }
    case uint64.val data: {
        if (data == nil) {
            return -1;
        }
        return 8;
    }
    case slice<int64> data: {
        return 8 * len(data);
    }
    case slice<uint64> data: {
        return 8 * len(data);
    }
    case float32 data: {
        return 4;
    }
    case float32.val data: {
        if (data == nil) {
            return -1;
        }
        return 4;
    }
    case float64 data: {
        return 8;
    }
    case float64.val data: {
        if (data == nil) {
            return -1;
        }
        return 8;
    }
    case slice<float32> data: {
        return 4 * len(data);
    }
    case slice<float64> data: {
        return 8 * len(data);
    }}
    return dataSize(reflect.Indirect(reflect.ValueOf(v)));
}

internal static sync.Map structSize; // map[reflect.Type]int

// dataSize returns the number of bytes the actual data represented by v occupies in memory.
// For compound structures, it sums the sizes of the elements. Thus, for instance, for a slice
// it returns the length of the slice times the element size and does not count the memory
// occupied by the header. If the type of v is not acceptable, dataSize returns -1.
internal static nint dataSize(reflectꓸValue v) {
    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == reflect.ΔSlice || exprᴛ1 == reflect.Array) {
        var t = v.Type().Elem();
        {
            var (size, ok) = structSize.Load(t); if (ok) {
                return size._<nint>() * v.Len();
            }
        }
        nint size = @sizeof(t);
        if (size >= 0) {
            if (t.Kind() == reflect.Struct) {
                structSize.Store(t, size);
            }
            return size * v.Len();
        }
    }
    if (exprᴛ1 == reflect.Struct) {
        var t = v.Type();
        {
            var (size, ok) = structSize.Load(t); if (ok) {
                return size._<nint>();
            }
        }
        nint size = @sizeof(t);
        structSize.Store(t, size);
        return size;
    }
    { /* default: */
        if (v.IsValid()) {
            return @sizeof(v.Type());
        }
    }

    return -1;
}

// sizeof returns the size >= 0 of variables for the given type or -1 if the type is not acceptable.
internal static nint @sizeof(reflectꓸType t) {
    var exprᴛ1 = t.Kind();
    if (exprᴛ1 == reflect.Array) {
        {
            nint s = @sizeof(t.Elem()); if (s >= 0) {
                return s * t.Len();
            }
        }
    }
    if (exprᴛ1 == reflect.Struct) {
        nint sum = 0;
        for (nint i = 0;nint n = t.NumField(); i < n; i++) {
            nint s = @sizeof(t.Field(i).Type);
            if (s < 0) {
                return -1;
            }
            sum += s;
        }
        return sum;
    }
    if (exprᴛ1 == reflect.ΔBool || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64 || exprᴛ1 == reflect.Float32 || exprᴛ1 == reflect.Float64 || exprᴛ1 == reflect.Complex64 || exprᴛ1 == reflect.Complex128) {
        return ((nint)t.Size());
    }

    return -1;
}

[GoType] partial struct coder {
    internal ByteOrder order;
    internal slice<byte> buf;
    internal nint offset;
}

[GoType("struct{order encoding.binary.ByteOrder; buf <>byte; offset int}")] partial struct decoder;

[GoType("struct{order encoding.binary.ByteOrder; buf <>byte; offset int}")] partial struct encoder;

[GoRecv] internal static bool @bool(this ref decoder d) {
    var x = d.buf[d.offset];
    d.offset++;
    return x != 0;
}

[GoRecv] internal static void @bool(this ref encoder e, bool x) {
    if (x){
        e.buf[e.offset] = 1;
    } else {
        e.buf[e.offset] = 0;
    }
    e.offset++;
}

[GoRecv] internal static uint8 uint8(this ref decoder d) {
    var x = d.buf[d.offset];
    d.offset++;
    return x;
}

[GoRecv] internal static void uint8(this ref encoder e, uint8 x) {
    e.buf[e.offset] = x;
    e.offset++;
}

[GoRecv] internal static uint16 uint16(this ref decoder d) {
    var x = d.order.Uint16(d.buf[(int)(d.offset)..(int)(d.offset + 2)]);
    d.offset += 2;
    return x;
}

[GoRecv] internal static void uint16(this ref encoder e, uint16 x) {
    e.order.PutUint16(e.buf[(int)(e.offset)..(int)(e.offset + 2)], x);
    e.offset += 2;
}

[GoRecv] internal static uint32 uint32(this ref decoder d) {
    var x = d.order.Uint32(d.buf[(int)(d.offset)..(int)(d.offset + 4)]);
    d.offset += 4;
    return x;
}

[GoRecv] internal static void uint32(this ref encoder e, uint32 x) {
    e.order.PutUint32(e.buf[(int)(e.offset)..(int)(e.offset + 4)], x);
    e.offset += 4;
}

[GoRecv] internal static uint64 uint64(this ref decoder d) {
    var x = d.order.Uint64(d.buf[(int)(d.offset)..(int)(d.offset + 8)]);
    d.offset += 8;
    return x;
}

[GoRecv] internal static void uint64(this ref encoder e, uint64 x) {
    e.order.PutUint64(e.buf[(int)(e.offset)..(int)(e.offset + 8)], x);
    e.offset += 8;
}

[GoRecv] internal static int8 int8(this ref decoder d) {
    return ((int8)d.uint8());
}

[GoRecv] internal static void int8(this ref encoder e, int8 x) {
    e.uint8(((uint8)x));
}

[GoRecv] internal static int16 int16(this ref decoder d) {
    return ((int16)d.uint16());
}

[GoRecv] internal static void int16(this ref encoder e, int16 x) {
    e.uint16(((uint16)x));
}

[GoRecv] internal static int32 int32(this ref decoder d) {
    return ((int32)d.uint32());
}

[GoRecv] internal static void int32(this ref encoder e, int32 x) {
    e.uint32(((uint32)x));
}

[GoRecv] internal static int64 int64(this ref decoder d) {
    return ((int64)d.uint64());
}

[GoRecv] internal static void int64(this ref encoder e, int64 x) {
    e.uint64(((uint64)x));
}

[GoRecv] internal static void value(this ref decoder d, reflectꓸValue v) {
    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == reflect.Array) {
        nint l = v.Len();
        for (nint i = 0; i < l; i++) {
            d.value(v.Index(i));
        }
    }
    else if (exprᴛ1 == reflect.Struct) {
        var t = v.Type();
        nint l = v.NumField();
        for (nint i = 0; i < l; i++) {
            // Note: Calling v.CanSet() below is an optimization.
            // It would be sufficient to check the field name,
            // but creating the StructField info for each field is
            // costly (run "go test -bench=ReadStruct" and compare
            // results when making changes to this code).
            {
                var vΔ2 = v.Field(i); if (vΔ2.CanSet() || t.Field(i).Name != "_"u8){
                    d.value(vΔ2);
                } else {
                    d.skip(vΔ2);
                }
            }
        }
    }
    else if (exprᴛ1 == reflect.ΔSlice) {
        nint l = v.Len();
        for (nint i = 0; i < l; i++) {
            d.value(v.Index(i));
        }
    }
    else if (exprᴛ1 == reflect.ΔBool) {
        v.SetBool(d.@bool());
    }
    else if (exprᴛ1 == reflect.Int8) {
        v.SetInt(((int64)d.int8()));
    }
    else if (exprᴛ1 == reflect.Int16) {
        v.SetInt(((int64)d.int16()));
    }
    else if (exprᴛ1 == reflect.Int32) {
        v.SetInt(((int64)d.int32()));
    }
    else if (exprᴛ1 == reflect.Int64) {
        v.SetInt(d.int64());
    }
    else if (exprᴛ1 == reflect.Uint8) {
        v.SetUint(((uint64)d.uint8()));
    }
    else if (exprᴛ1 == reflect.Uint16) {
        v.SetUint(((uint64)d.uint16()));
    }
    else if (exprᴛ1 == reflect.Uint32) {
        v.SetUint(((uint64)d.uint32()));
    }
    else if (exprᴛ1 == reflect.Uint64) {
        v.SetUint(d.uint64());
    }
    else if (exprᴛ1 == reflect.Float32) {
        v.SetFloat(((float64)math.Float32frombits(d.uint32())));
    }
    else if (exprᴛ1 == reflect.Float64) {
        v.SetFloat(math.Float64frombits(d.uint64()));
    }
    else if (exprᴛ1 == reflect.Complex64) {
        v.SetComplex(complex(
            ((float64)math.Float32frombits(d.uint32())),
            ((float64)math.Float32frombits(d.uint32()))));
    }
    else if (exprᴛ1 == reflect.Complex128) {
        v.SetComplex(complex(
            math.Float64frombits(d.uint64()),
            math.Float64frombits(d.uint64())));
    }

}

[GoRecv] internal static void value(this ref encoder e, reflectꓸValue v) {
    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == reflect.Array) {
        nint l = v.Len();
        for (nint i = 0; i < l; i++) {
            e.value(v.Index(i));
        }
    }
    else if (exprᴛ1 == reflect.Struct) {
        var t = v.Type();
        nint l = v.NumField();
        for (nint i = 0; i < l; i++) {
            // see comment for corresponding code in decoder.value()
            {
                var vΔ2 = v.Field(i); if (vΔ2.CanSet() || t.Field(i).Name != "_"u8){
                    e.value(vΔ2);
                } else {
                    e.skip(vΔ2);
                }
            }
        }
    }
    else if (exprᴛ1 == reflect.ΔSlice) {
        nint l = v.Len();
        for (nint i = 0; i < l; i++) {
            e.value(v.Index(i));
        }
    }
    else if (exprᴛ1 == reflect.ΔBool) {
        e.@bool(v.Bool());
    }
    else if (exprᴛ1 == reflect.Int8) {
        e.int8(((int8)v.Int()));
    }
    else if (exprᴛ1 == reflect.Int16) {
        e.int16(((int16)v.Int()));
    }
    else if (exprᴛ1 == reflect.Int32) {
        e.int32(((int32)v.Int()));
    }
    else if (exprᴛ1 == reflect.Int64) {
        e.int64(v.Int());
    }
    else if (exprᴛ1 == reflect.Uint8) {
        e.uint8(((uint8)v.Uint()));
    }
    else if (exprᴛ1 == reflect.Uint16) {
        e.uint16(((uint16)v.Uint()));
    }
    else if (exprᴛ1 == reflect.Uint32) {
        e.uint32(((uint32)v.Uint()));
    }
    else if (exprᴛ1 == reflect.Uint64) {
        e.uint64(v.Uint());
    }
    else if (exprᴛ1 == reflect.Float32) {
        e.uint32(math.Float32bits(((float32)v.Float())));
    }
    else if (exprᴛ1 == reflect.Float64) {
        e.uint64(math.Float64bits(v.Float()));
    }
    else if (exprᴛ1 == reflect.Complex64) {
        var x = v.Complex();
        e.uint32(math.Float32bits(((float32)real(x))));
        e.uint32(math.Float32bits(((float32)imag(x))));
    }
    else if (exprᴛ1 == reflect.Complex128) {
        var x = v.Complex();
        e.uint64(math.Float64bits(real(x)));
        e.uint64(math.Float64bits(imag(x)));
    }

}

[GoRecv] internal static void skip(this ref decoder d, reflectꓸValue v) {
    d.offset += dataSize(v);
}

[GoRecv] internal static void skip(this ref encoder e, reflectꓸValue v) {
    nint n = dataSize(v);
    clear(e.buf[(int)(e.offset)..(int)(e.offset + n)]);
    e.offset += n;
}

// intDataSize returns the size of the data required to represent the data when encoded,
// and optionally a byte slice containing the encoded data if no conversion is necessary.
// It returns zero, nil if the type cannot be implemented by the fast path in Read or Write.
internal static (nint, slice<byte>) intDataSize(any data) {
    switch (data.type()) {
    case bool data: {
        return (1, default!);
    }
    case int8 data: {
        return (1, default!);
    }
    case uint8 data: {
        return (1, default!);
    }
    case @bool.val data: {
        return (1, default!);
    }
    case int8.val data: {
        return (1, default!);
    }
    case uint8.val data: {
        return (1, default!);
    }
    case slice<bool> data: {
        return (len(data), default!);
    }
    case slice<int8> data: {
        return (len(data), default!);
    }
    case slice<uint8> data: {
        return (len(data), data);
    }
    case int16 data: {
        return (2, default!);
    }
    case uint16 data: {
        return (2, default!);
    }
    case int16.val data: {
        return (2, default!);
    }
    case uint16.val data: {
        return (2, default!);
    }
    case slice<int16> data: {
        return (2 * len(data), default!);
    }
    case slice<uint16> data: {
        return (2 * len(data), default!);
    }
    case int32 data: {
        return (4, default!);
    }
    case uint32 data: {
        return (4, default!);
    }
    case int32.val data: {
        return (4, default!);
    }
    case uint32.val data: {
        return (4, default!);
    }
    case slice<int32> data: {
        return (4 * len(data), default!);
    }
    case slice<uint32> data: {
        return (4 * len(data), default!);
    }
    case int64 data: {
        return (8, default!);
    }
    case uint64 data: {
        return (8, default!);
    }
    case int64.val data: {
        return (8, default!);
    }
    case uint64.val data: {
        return (8, default!);
    }
    case slice<int64> data: {
        return (8 * len(data), default!);
    }
    case slice<uint64> data: {
        return (8 * len(data), default!);
    }
    case float32 data: {
        return (4, default!);
    }
    case float32.val data: {
        return (4, default!);
    }
    case float64 data: {
        return (8, default!);
    }
    case float64.val data: {
        return (8, default!);
    }
    case slice<float32> data: {
        return (4 * len(data), default!);
    }
    case slice<float64> data: {
        return (8 * len(data), default!);
    }}
    return (0, default!);
}

// ensure grows buf to length len(buf) + n and returns the grown buffer
// and a slice starting at the original length of buf (that is, buf2[len(buf):]).
internal static (slice<byte> buf2, slice<byte> pos) ensure(slice<byte> buf, nint n) {
    slice<byte> buf2 = default!;
    slice<byte> pos = default!;

    nint l = len(buf);
    buf = slices.Grow(buf, n)[..(int)(l + n)];
    return (buf, buf[(int)(l)..]);
}

} // end binary_package
