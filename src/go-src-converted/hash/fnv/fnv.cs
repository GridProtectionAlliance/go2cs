// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fnv implements FNV-1 and FNV-1a, non-cryptographic hash functions
// created by Glenn Fowler, Landon Curt Noll, and Phong Vo.
// See
// https://en.wikipedia.org/wiki/Fowler-Noll-Vo_hash_function.
//
// All the hash.Hash implementations returned by this package also
// implement encoding.BinaryMarshaler and encoding.BinaryUnmarshaler to
// marshal and unmarshal the internal state of the hash.

// package fnv -- go2cs converted at 2022 March 13 05:29:01 UTC
// import "hash/fnv" ==> using fnv = go.hash.fnv_package
// Original source: C:\Program Files\Go\src\hash\fnv\fnv.go
namespace go.hash;

using errors = errors_package;
using hash = hash_package;
using bits = math.bits_package;

public static partial class fnv_package {

private partial struct sum32 { // : uint
}
private partial struct sum32a { // : uint
}
private partial struct sum64 { // : ulong
}
private partial struct sum64a { // : ulong
}
private partial struct sum128 { // : array<ulong>
}
private partial struct sum128a { // : array<ulong>
}private static readonly nint offset32 = (nint)2166136261L;
private static readonly nuint offset64 = (nuint)14695981039346656037UL;
private static readonly nuint offset128Lower = 0x62b821756295c58d;
private static readonly nuint offset128Higher = 0x6c62272e07bb0142;
private static readonly nint prime32 = 16777619;
private static readonly nint prime64 = (nint)1099511628211L;
private static readonly nuint prime128Lower = 0x13b;
private static readonly nint prime128Shift = 24;

// New32 returns a new 32-bit FNV-1 hash.Hash.
// Its Sum method will lay the value out in big-endian byte order.
public static hash.Hash32 New32() {
    ref sum32 s = ref heap(offset32, out ptr<sum32> _addr_s);
    return _addr_s;
}

// New32a returns a new 32-bit FNV-1a hash.Hash.
// Its Sum method will lay the value out in big-endian byte order.
public static hash.Hash32 New32a() {
    ref sum32a s = ref heap(offset32, out ptr<sum32a> _addr_s);
    return _addr_s;
}

// New64 returns a new 64-bit FNV-1 hash.Hash.
// Its Sum method will lay the value out in big-endian byte order.
public static hash.Hash64 New64() {
    ref sum64 s = ref heap(offset64, out ptr<sum64> _addr_s);
    return _addr_s;
}

// New64a returns a new 64-bit FNV-1a hash.Hash.
// Its Sum method will lay the value out in big-endian byte order.
public static hash.Hash64 New64a() {
    ref sum64a s = ref heap(offset64, out ptr<sum64a> _addr_s);
    return _addr_s;
}

// New128 returns a new 128-bit FNV-1 hash.Hash.
// Its Sum method will lay the value out in big-endian byte order.
public static hash.Hash New128() {
    ref sum128 s = ref heap(out ptr<sum128> _addr_s);
    s[0] = offset128Higher;
    s[1] = offset128Lower;
    return _addr_s;
}

// New128a returns a new 128-bit FNV-1a hash.Hash.
// Its Sum method will lay the value out in big-endian byte order.
public static hash.Hash New128a() {
    ref sum128a s = ref heap(out ptr<sum128a> _addr_s);
    s[0] = offset128Higher;
    s[1] = offset128Lower;
    return _addr_s;
}

private static void Reset(this ptr<sum32> _addr_s) {
    ref sum32 s = ref _addr_s.val;

    s.val = offset32;
}
private static void Reset(this ptr<sum32a> _addr_s) {
    ref sum32a s = ref _addr_s.val;

    s.val = offset32;
}
private static void Reset(this ptr<sum64> _addr_s) {
    ref sum64 s = ref _addr_s.val;

    s.val = offset64;
}
private static void Reset(this ptr<sum64a> _addr_s) {
    ref sum64a s = ref _addr_s.val;

    s.val = offset64;
}
private static void Reset(this ptr<sum128> _addr_s) {
    ref sum128 s = ref _addr_s.val;

    s[0] = offset128Higher;

    s[1] = offset128Lower;
}
private static void Reset(this ptr<sum128a> _addr_s) {
    ref sum128a s = ref _addr_s.val;

    s[0] = offset128Higher;

    s[1] = offset128Lower;
}

private static uint Sum32(this ptr<sum32> _addr_s) {
    ref sum32 s = ref _addr_s.val;

    return uint32(s.val);
}
private static uint Sum32(this ptr<sum32a> _addr_s) {
    ref sum32a s = ref _addr_s.val;

    return uint32(s.val);
}
private static ulong Sum64(this ptr<sum64> _addr_s) {
    ref sum64 s = ref _addr_s.val;

    return uint64(s.val);
}
private static ulong Sum64(this ptr<sum64a> _addr_s) {
    ref sum64a s = ref _addr_s.val;

    return uint64(s.val);
}

private static (nint, error) Write(this ptr<sum32> _addr_s, slice<byte> data) {
    nint _p0 = default;
    error _p0 = default!;
    ref sum32 s = ref _addr_s.val;

    var hash = s.val;
    foreach (var (_, c) in data) {
        hash *= prime32;
        hash ^= sum32(c);
    }    s.val = hash;
    return (len(data), error.As(null!)!);
}

private static (nint, error) Write(this ptr<sum32a> _addr_s, slice<byte> data) {
    nint _p0 = default;
    error _p0 = default!;
    ref sum32a s = ref _addr_s.val;

    var hash = s.val;
    foreach (var (_, c) in data) {
        hash ^= sum32a(c);
        hash *= prime32;
    }    s.val = hash;
    return (len(data), error.As(null!)!);
}

private static (nint, error) Write(this ptr<sum64> _addr_s, slice<byte> data) {
    nint _p0 = default;
    error _p0 = default!;
    ref sum64 s = ref _addr_s.val;

    var hash = s.val;
    foreach (var (_, c) in data) {
        hash *= prime64;
        hash ^= sum64(c);
    }    s.val = hash;
    return (len(data), error.As(null!)!);
}

private static (nint, error) Write(this ptr<sum64a> _addr_s, slice<byte> data) {
    nint _p0 = default;
    error _p0 = default!;
    ref sum64a s = ref _addr_s.val;

    var hash = s.val;
    foreach (var (_, c) in data) {
        hash ^= sum64a(c);
        hash *= prime64;
    }    s.val = hash;
    return (len(data), error.As(null!)!);
}

private static (nint, error) Write(this ptr<sum128> _addr_s, slice<byte> data) {
    nint _p0 = default;
    error _p0 = default!;
    ref sum128 s = ref _addr_s.val;

    foreach (var (_, c) in data) { 
        // Compute the multiplication
        var (s0, s1) = bits.Mul64(prime128Lower, s[1]);
        s0 += s[1] << (int)(prime128Shift) + prime128Lower * s[0]; 
        // Update the values
        s[1] = s1;
        s[0] = s0;
        s[1] ^= uint64(c);
    }    return (len(data), error.As(null!)!);
}

private static (nint, error) Write(this ptr<sum128a> _addr_s, slice<byte> data) {
    nint _p0 = default;
    error _p0 = default!;
    ref sum128a s = ref _addr_s.val;

    foreach (var (_, c) in data) {
        s[1] ^= uint64(c); 
        // Compute the multiplication
        var (s0, s1) = bits.Mul64(prime128Lower, s[1]);
        s0 += s[1] << (int)(prime128Shift) + prime128Lower * s[0]; 
        // Update the values
        s[1] = s1;
        s[0] = s0;
    }    return (len(data), error.As(null!)!);
}

private static nint Size(this ptr<sum32> _addr_s) {
    ref sum32 s = ref _addr_s.val;

    return 4;
}
private static nint Size(this ptr<sum32a> _addr_s) {
    ref sum32a s = ref _addr_s.val;

    return 4;
}
private static nint Size(this ptr<sum64> _addr_s) {
    ref sum64 s = ref _addr_s.val;

    return 8;
}
private static nint Size(this ptr<sum64a> _addr_s) {
    ref sum64a s = ref _addr_s.val;

    return 8;
}
private static nint Size(this ptr<sum128> _addr_s) {
    ref sum128 s = ref _addr_s.val;

    return 16;
}
private static nint Size(this ptr<sum128a> _addr_s) {
    ref sum128a s = ref _addr_s.val;

    return 16;
}

private static nint BlockSize(this ptr<sum32> _addr_s) {
    ref sum32 s = ref _addr_s.val;

    return 1;
}
private static nint BlockSize(this ptr<sum32a> _addr_s) {
    ref sum32a s = ref _addr_s.val;

    return 1;
}
private static nint BlockSize(this ptr<sum64> _addr_s) {
    ref sum64 s = ref _addr_s.val;

    return 1;
}
private static nint BlockSize(this ptr<sum64a> _addr_s) {
    ref sum64a s = ref _addr_s.val;

    return 1;
}
private static nint BlockSize(this ptr<sum128> _addr_s) {
    ref sum128 s = ref _addr_s.val;

    return 1;
}
private static nint BlockSize(this ptr<sum128a> _addr_s) {
    ref sum128a s = ref _addr_s.val;

    return 1;
}

private static slice<byte> Sum(this ptr<sum32> _addr_s, slice<byte> @in) {
    ref sum32 s = ref _addr_s.val;

    var v = uint32(s.val);
    return append(in, byte(v >> 24), byte(v >> 16), byte(v >> 8), byte(v));
}

private static slice<byte> Sum(this ptr<sum32a> _addr_s, slice<byte> @in) {
    ref sum32a s = ref _addr_s.val;

    var v = uint32(s.val);
    return append(in, byte(v >> 24), byte(v >> 16), byte(v >> 8), byte(v));
}

private static slice<byte> Sum(this ptr<sum64> _addr_s, slice<byte> @in) {
    ref sum64 s = ref _addr_s.val;

    var v = uint64(s.val);
    return append(in, byte(v >> 56), byte(v >> 48), byte(v >> 40), byte(v >> 32), byte(v >> 24), byte(v >> 16), byte(v >> 8), byte(v));
}

private static slice<byte> Sum(this ptr<sum64a> _addr_s, slice<byte> @in) {
    ref sum64a s = ref _addr_s.val;

    var v = uint64(s.val);
    return append(in, byte(v >> 56), byte(v >> 48), byte(v >> 40), byte(v >> 32), byte(v >> 24), byte(v >> 16), byte(v >> 8), byte(v));
}

private static slice<byte> Sum(this ptr<sum128> _addr_s, slice<byte> @in) {
    ref sum128 s = ref _addr_s.val;

    return append(in, byte(s[0] >> 56), byte(s[0] >> 48), byte(s[0] >> 40), byte(s[0] >> 32), byte(s[0] >> 24), byte(s[0] >> 16), byte(s[0] >> 8), byte(s[0]), byte(s[1] >> 56), byte(s[1] >> 48), byte(s[1] >> 40), byte(s[1] >> 32), byte(s[1] >> 24), byte(s[1] >> 16), byte(s[1] >> 8), byte(s[1]));
}

private static slice<byte> Sum(this ptr<sum128a> _addr_s, slice<byte> @in) {
    ref sum128a s = ref _addr_s.val;

    return append(in, byte(s[0] >> 56), byte(s[0] >> 48), byte(s[0] >> 40), byte(s[0] >> 32), byte(s[0] >> 24), byte(s[0] >> 16), byte(s[0] >> 8), byte(s[0]), byte(s[1] >> 56), byte(s[1] >> 48), byte(s[1] >> 40), byte(s[1] >> 32), byte(s[1] >> 24), byte(s[1] >> 16), byte(s[1] >> 8), byte(s[1]));
}

private static readonly @string magic32 = "fnv\x01";
private static readonly @string magic32a = "fnv\x02";
private static readonly @string magic64 = "fnv\x03";
private static readonly @string magic64a = "fnv\x04";
private static readonly @string magic128 = "fnv\x05";
private static readonly @string magic128a = "fnv\x06";
private static readonly var marshaledSize32 = len(magic32) + 4;
private static readonly var marshaledSize64 = len(magic64) + 8;
private static readonly var marshaledSize128 = len(magic128) + 8 * 2;

private static (slice<byte>, error) MarshalBinary(this ptr<sum32> _addr_s) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref sum32 s = ref _addr_s.val;

    var b = make_slice<byte>(0, marshaledSize32);
    b = append(b, magic32);
    b = appendUint32(b, uint32(s.val));
    return (b, error.As(null!)!);
}

private static (slice<byte>, error) MarshalBinary(this ptr<sum32a> _addr_s) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref sum32a s = ref _addr_s.val;

    var b = make_slice<byte>(0, marshaledSize32);
    b = append(b, magic32a);
    b = appendUint32(b, uint32(s.val));
    return (b, error.As(null!)!);
}

private static (slice<byte>, error) MarshalBinary(this ptr<sum64> _addr_s) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref sum64 s = ref _addr_s.val;

    var b = make_slice<byte>(0, marshaledSize64);
    b = append(b, magic64);
    b = appendUint64(b, uint64(s.val));
    return (b, error.As(null!)!);
}

private static (slice<byte>, error) MarshalBinary(this ptr<sum64a> _addr_s) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref sum64a s = ref _addr_s.val;

    var b = make_slice<byte>(0, marshaledSize64);
    b = append(b, magic64a);
    b = appendUint64(b, uint64(s.val));
    return (b, error.As(null!)!);
}

private static (slice<byte>, error) MarshalBinary(this ptr<sum128> _addr_s) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref sum128 s = ref _addr_s.val;

    var b = make_slice<byte>(0, marshaledSize128);
    b = append(b, magic128);
    b = appendUint64(b, s[0]);
    b = appendUint64(b, s[1]);
    return (b, error.As(null!)!);
}

private static (slice<byte>, error) MarshalBinary(this ptr<sum128a> _addr_s) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref sum128a s = ref _addr_s.val;

    var b = make_slice<byte>(0, marshaledSize128);
    b = append(b, magic128a);
    b = appendUint64(b, s[0]);
    b = appendUint64(b, s[1]);
    return (b, error.As(null!)!);
}

private static error UnmarshalBinary(this ptr<sum32> _addr_s, slice<byte> b) {
    ref sum32 s = ref _addr_s.val;

    if (len(b) < len(magic32) || string(b[..(int)len(magic32)]) != magic32) {
        return error.As(errors.New("hash/fnv: invalid hash state identifier"))!;
    }
    if (len(b) != marshaledSize32) {
        return error.As(errors.New("hash/fnv: invalid hash state size"))!;
    }
    s.val = sum32(readUint32(b[(int)4..]));
    return error.As(null!)!;
}

private static error UnmarshalBinary(this ptr<sum32a> _addr_s, slice<byte> b) {
    ref sum32a s = ref _addr_s.val;

    if (len(b) < len(magic32a) || string(b[..(int)len(magic32a)]) != magic32a) {
        return error.As(errors.New("hash/fnv: invalid hash state identifier"))!;
    }
    if (len(b) != marshaledSize32) {
        return error.As(errors.New("hash/fnv: invalid hash state size"))!;
    }
    s.val = sum32a(readUint32(b[(int)4..]));
    return error.As(null!)!;
}

private static error UnmarshalBinary(this ptr<sum64> _addr_s, slice<byte> b) {
    ref sum64 s = ref _addr_s.val;

    if (len(b) < len(magic64) || string(b[..(int)len(magic64)]) != magic64) {
        return error.As(errors.New("hash/fnv: invalid hash state identifier"))!;
    }
    if (len(b) != marshaledSize64) {
        return error.As(errors.New("hash/fnv: invalid hash state size"))!;
    }
    s.val = sum64(readUint64(b[(int)4..]));
    return error.As(null!)!;
}

private static error UnmarshalBinary(this ptr<sum64a> _addr_s, slice<byte> b) {
    ref sum64a s = ref _addr_s.val;

    if (len(b) < len(magic64a) || string(b[..(int)len(magic64a)]) != magic64a) {
        return error.As(errors.New("hash/fnv: invalid hash state identifier"))!;
    }
    if (len(b) != marshaledSize64) {
        return error.As(errors.New("hash/fnv: invalid hash state size"))!;
    }
    s.val = sum64a(readUint64(b[(int)4..]));
    return error.As(null!)!;
}

private static error UnmarshalBinary(this ptr<sum128> _addr_s, slice<byte> b) {
    ref sum128 s = ref _addr_s.val;

    if (len(b) < len(magic128) || string(b[..(int)len(magic128)]) != magic128) {
        return error.As(errors.New("hash/fnv: invalid hash state identifier"))!;
    }
    if (len(b) != marshaledSize128) {
        return error.As(errors.New("hash/fnv: invalid hash state size"))!;
    }
    s[0] = readUint64(b[(int)4..]);
    s[1] = readUint64(b[(int)12..]);
    return error.As(null!)!;
}

private static error UnmarshalBinary(this ptr<sum128a> _addr_s, slice<byte> b) {
    ref sum128a s = ref _addr_s.val;

    if (len(b) < len(magic128a) || string(b[..(int)len(magic128a)]) != magic128a) {
        return error.As(errors.New("hash/fnv: invalid hash state identifier"))!;
    }
    if (len(b) != marshaledSize128) {
        return error.As(errors.New("hash/fnv: invalid hash state size"))!;
    }
    s[0] = readUint64(b[(int)4..]);
    s[1] = readUint64(b[(int)12..]);
    return error.As(null!)!;
}

private static uint readUint32(slice<byte> b) {
    _ = b[3];
    return uint32(b[3]) | uint32(b[2]) << 8 | uint32(b[1]) << 16 | uint32(b[0]) << 24;
}

private static slice<byte> appendUint32(slice<byte> b, uint x) {
    array<byte> a = new array<byte>(new byte[] { byte(x>>24), byte(x>>16), byte(x>>8), byte(x) });
    return append(b, a[..]);
}

private static slice<byte> appendUint64(slice<byte> b, ulong x) {
    array<byte> a = new array<byte>(new byte[] { byte(x>>56), byte(x>>48), byte(x>>40), byte(x>>32), byte(x>>24), byte(x>>16), byte(x>>8), byte(x) });
    return append(b, a[..]);
}

private static ulong readUint64(slice<byte> b) {
    _ = b[7];
    return uint64(b[7]) | uint64(b[6]) << 8 | uint64(b[5]) << 16 | uint64(b[4]) << 24 | uint64(b[3]) << 32 | uint64(b[2]) << 40 | uint64(b[1]) << 48 | uint64(b[0]) << 56;
}

} // end fnv_package
