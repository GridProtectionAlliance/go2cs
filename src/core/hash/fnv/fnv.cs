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
namespace go.hash;

using errors = errors_package;
using hash = hash_package;
using byteorder = @internal.byteorder_package;
using bits = math.bits_package;
using @internal;
using math;

partial class fnv_package {

[GoType("num:uint32")] partial struct sum32;

[GoType("num:uint32")] partial struct sum32a;

[GoType("num:uint64")] partial struct sum64;

[GoType("num:uint64")] partial struct sum64a;

[GoType("[2]uint64")] partial struct sum128;

[GoType("[2]uint64")] partial struct sum128a;

internal static readonly UntypedInt offset32 = 2166136261;
internal static readonly GoUntyped offset64 = /* 14695981039346656037 */
    GoUntyped.Parse("14695981039346656037");
internal static readonly UntypedInt offset128Lower = /* 0x62b821756295c58d */ 7113472399480571277;
internal static readonly UntypedInt offset128Higher = /* 0x6c62272e07bb0142 */ 7809847782465536322;
internal static readonly UntypedInt prime32 = 16777619;
internal static readonly UntypedInt prime64 = 1099511628211;
internal static readonly UntypedInt prime128Lower = /* 0x13b */ 315;
internal static readonly UntypedInt prime128Shift = 24;

// New32 returns a new 32-bit FNV-1 [hash.Hash].
// Its Sum method will lay the value out in big-endian byte order.
public static hash.Hash32 New32() {
    ref var s = ref heap(new sum32(), out var Ꮡs);
    s = offset32;
    return ~Ꮡs;
}

// New32a returns a new 32-bit FNV-1a [hash.Hash].
// Its Sum method will lay the value out in big-endian byte order.
public static hash.Hash32 New32a() {
    ref var s = ref heap(new sum32a(), out var Ꮡs);
    s = offset32;
    return ~Ꮡs;
}

// New64 returns a new 64-bit FNV-1 [hash.Hash].
// Its Sum method will lay the value out in big-endian byte order.
public static hash.Hash64 New64() {
    ref var s = ref heap(new sum64(), out var Ꮡs);
    s = offset64;
    return ~Ꮡs;
}

// New64a returns a new 64-bit FNV-1a [hash.Hash].
// Its Sum method will lay the value out in big-endian byte order.
public static hash.Hash64 New64a() {
    ref var s = ref heap(new sum64a(), out var Ꮡs);
    s = offset64;
    return ~Ꮡs;
}

// New128 returns a new 128-bit FNV-1 [hash.Hash].
// Its Sum method will lay the value out in big-endian byte order.
public static hash.Hash New128() {
    ref var s = ref heap(new sum128(), out var Ꮡs);
    s[0] = offset128Higher;
    s[1] = offset128Lower;
    return ~Ꮡs;
}

// New128a returns a new 128-bit FNV-1a [hash.Hash].
// Its Sum method will lay the value out in big-endian byte order.
public static hash.Hash New128a() {
    ref var s = ref heap(new sum128a(), out var Ꮡs);
    s[0] = offset128Higher;
    s[1] = offset128Lower;
    return ~Ꮡs;
}

[GoRecv] internal static void Reset(this ref sum32 s) {
    s = offset32;
}

[GoRecv] internal static void Reset(this ref sum32a s) {
    s = offset32;
}

[GoRecv] internal static void Reset(this ref sum64 s) {
    s = offset64;
}

[GoRecv] internal static void Reset(this ref sum64a s) {
    s = offset64;
}

[GoRecv] internal static void Reset(this ref sum128 s) {
    s[0] = offset128Higher;
    s[1] = offset128Lower;
}

[GoRecv] internal static void Reset(this ref sum128a s) {
    s[0] = offset128Higher;
    s[1] = offset128Lower;
}

[GoRecv] internal static uint32 Sum32(this ref sum32 s) {
    return ((uint32)(s));
}

[GoRecv] internal static uint32 Sum32(this ref sum32a s) {
    return ((uint32)(s));
}

[GoRecv] internal static uint64 Sum64(this ref sum64 s) {
    return ((uint64)(s));
}

[GoRecv] internal static uint64 Sum64(this ref sum64a s) {
    return ((uint64)(s));
}

[GoRecv] internal static (nint, error) Write(this ref sum32 s, slice<byte> data) {
    var hash = s;
    foreach (var (_, c) in data) {
        hash *= prime32;
        hash ^= (sum32)(((sum32)c));
    }
    s = hash;
    return (len(data), default!);
}

[GoRecv] internal static (nint, error) Write(this ref sum32a s, slice<byte> data) {
    var hash = s;
    foreach (var (_, c) in data) {
        hash ^= (sum32a)(((sum32a)c));
        hash *= prime32;
    }
    s = hash;
    return (len(data), default!);
}

[GoRecv] internal static (nint, error) Write(this ref sum64 s, slice<byte> data) {
    var hash = s;
    foreach (var (_, c) in data) {
        hash *= prime64;
        hash ^= (sum64)(((sum64)c));
    }
    s = hash;
    return (len(data), default!);
}

[GoRecv] internal static (nint, error) Write(this ref sum64a s, slice<byte> data) {
    var hash = s;
    foreach (var (_, c) in data) {
        hash ^= (sum64a)(((sum64a)c));
        hash *= prime64;
    }
    s = hash;
    return (len(data), default!);
}

[GoRecv] internal static (nint, error) Write(this ref sum128 s, slice<byte> data) {
    foreach (var (_, c) in data) {
        // Compute the multiplication
        var (s0, s1) = bits.Mul64(prime128Lower, s[1]);
        s0 += s[1] << (int)(prime128Shift) + prime128Lower * s[0];
        // Update the values
        s[1] = s1;
        s[0] = s0;
        s[1] ^= (uint64)(((uint64)c));
    }
    return (len(data), default!);
}

[GoRecv] internal static (nint, error) Write(this ref sum128a s, slice<byte> data) {
    foreach (var (_, c) in data) {
        s[1] ^= (uint64)(((uint64)c));
        // Compute the multiplication
        var (s0, s1) = bits.Mul64(prime128Lower, s[1]);
        s0 += s[1] << (int)(prime128Shift) + prime128Lower * s[0];
        // Update the values
        s[1] = s1;
        s[0] = s0;
    }
    return (len(data), default!);
}

[GoRecv] internal static nint Size(this ref sum32 s) {
    return 4;
}

[GoRecv] internal static nint Size(this ref sum32a s) {
    return 4;
}

[GoRecv] internal static nint Size(this ref sum64 s) {
    return 8;
}

[GoRecv] internal static nint Size(this ref sum64a s) {
    return 8;
}

[GoRecv] internal static nint Size(this ref sum128 s) {
    return 16;
}

[GoRecv] internal static nint Size(this ref sum128a s) {
    return 16;
}

[GoRecv] internal static nint BlockSize(this ref sum32 s) {
    return 1;
}

[GoRecv] internal static nint BlockSize(this ref sum32a s) {
    return 1;
}

[GoRecv] internal static nint BlockSize(this ref sum64 s) {
    return 1;
}

[GoRecv] internal static nint BlockSize(this ref sum64a s) {
    return 1;
}

[GoRecv] internal static nint BlockSize(this ref sum128 s) {
    return 1;
}

[GoRecv] internal static nint BlockSize(this ref sum128a s) {
    return 1;
}

[GoRecv] internal static slice<byte> Sum(this ref sum32 s, slice<byte> @in) {
    var v = ((uint32)(s));
    return byteorder.BeAppendUint32(@in, v);
}

[GoRecv] internal static slice<byte> Sum(this ref sum32a s, slice<byte> @in) {
    var v = ((uint32)(s));
    return byteorder.BeAppendUint32(@in, v);
}

[GoRecv] internal static slice<byte> Sum(this ref sum64 s, slice<byte> @in) {
    var v = ((uint64)(s));
    return byteorder.BeAppendUint64(@in, v);
}

[GoRecv] internal static slice<byte> Sum(this ref sum64a s, slice<byte> @in) {
    var v = ((uint64)(s));
    return byteorder.BeAppendUint64(@in, v);
}

[GoRecv] internal static slice<byte> Sum(this ref sum128 s, slice<byte> @in) {
    var ret = byteorder.BeAppendUint64(@in, s[0]);
    return byteorder.BeAppendUint64(ret, s[1]);
}

[GoRecv] internal static slice<byte> Sum(this ref sum128a s, slice<byte> @in) {
    var ret = byteorder.BeAppendUint64(@in, s[0]);
    return byteorder.BeAppendUint64(ret, s[1]);
}

internal static readonly @string magic32 = "fnv\x01"u8;
internal static readonly @string magic32a = "fnv\x02"u8;
internal static readonly @string magic64 = "fnv\x03"u8;
internal static readonly @string magic64a = "fnv\x04"u8;
internal static readonly @string magic128 = "fnv\x05"u8;
internal static readonly @string magic128a = "fnv\x06"u8;
internal const nint marshaledSize32 = /* len(magic32) + 4 */ 8;
internal const nint marshaledSize64 = /* len(magic64) + 8 */ 12;
internal const nint marshaledSize128 = /* len(magic128) + 8*2 */ 20;

[GoRecv] internal static (slice<byte>, error) MarshalBinary(this ref sum32 s) {
    var b = new slice<byte>(0, marshaledSize32);
    b = append(b, magic32.ꓸꓸꓸ);
    b = byteorder.BeAppendUint32(b, ((uint32)(s)));
    return (b, default!);
}

[GoRecv] internal static (slice<byte>, error) MarshalBinary(this ref sum32a s) {
    var b = new slice<byte>(0, marshaledSize32);
    b = append(b, magic32a.ꓸꓸꓸ);
    b = byteorder.BeAppendUint32(b, ((uint32)(s)));
    return (b, default!);
}

[GoRecv] internal static (slice<byte>, error) MarshalBinary(this ref sum64 s) {
    var b = new slice<byte>(0, marshaledSize64);
    b = append(b, magic64.ꓸꓸꓸ);
    b = byteorder.BeAppendUint64(b, ((uint64)(s)));
    return (b, default!);
}

[GoRecv] internal static (slice<byte>, error) MarshalBinary(this ref sum64a s) {
    var b = new slice<byte>(0, marshaledSize64);
    b = append(b, magic64a.ꓸꓸꓸ);
    b = byteorder.BeAppendUint64(b, ((uint64)(s)));
    return (b, default!);
}

[GoRecv] internal static (slice<byte>, error) MarshalBinary(this ref sum128 s) {
    var b = new slice<byte>(0, marshaledSize128);
    b = append(b, magic128.ꓸꓸꓸ);
    b = byteorder.BeAppendUint64(b, s[0]);
    b = byteorder.BeAppendUint64(b, s[1]);
    return (b, default!);
}

[GoRecv] internal static (slice<byte>, error) MarshalBinary(this ref sum128a s) {
    var b = new slice<byte>(0, marshaledSize128);
    b = append(b, magic128a.ꓸꓸꓸ);
    b = byteorder.BeAppendUint64(b, s[0]);
    b = byteorder.BeAppendUint64(b, s[1]);
    return (b, default!);
}

[GoRecv] internal static error UnmarshalBinary(this ref sum32 s, slice<byte> b) {
    if (len(b) < len(magic32) || ((@string)(b[..(int)(len(magic32))])) != magic32) {
        return errors.New("hash/fnv: invalid hash state identifier"u8);
    }
    if (len(b) != marshaledSize32) {
        return errors.New("hash/fnv: invalid hash state size"u8);
    }
    s = ((sum32)byteorder.BeUint32(b[4..]));
    return default!;
}

[GoRecv] internal static error UnmarshalBinary(this ref sum32a s, slice<byte> b) {
    if (len(b) < len(magic32a) || ((@string)(b[..(int)(len(magic32a))])) != magic32a) {
        return errors.New("hash/fnv: invalid hash state identifier"u8);
    }
    if (len(b) != marshaledSize32) {
        return errors.New("hash/fnv: invalid hash state size"u8);
    }
    s = ((sum32a)byteorder.BeUint32(b[4..]));
    return default!;
}

[GoRecv] internal static error UnmarshalBinary(this ref sum64 s, slice<byte> b) {
    if (len(b) < len(magic64) || ((@string)(b[..(int)(len(magic64))])) != magic64) {
        return errors.New("hash/fnv: invalid hash state identifier"u8);
    }
    if (len(b) != marshaledSize64) {
        return errors.New("hash/fnv: invalid hash state size"u8);
    }
    s = ((sum64)byteorder.BeUint64(b[4..]));
    return default!;
}

[GoRecv] internal static error UnmarshalBinary(this ref sum64a s, slice<byte> b) {
    if (len(b) < len(magic64a) || ((@string)(b[..(int)(len(magic64a))])) != magic64a) {
        return errors.New("hash/fnv: invalid hash state identifier"u8);
    }
    if (len(b) != marshaledSize64) {
        return errors.New("hash/fnv: invalid hash state size"u8);
    }
    s = ((sum64a)byteorder.BeUint64(b[4..]));
    return default!;
}

[GoRecv] internal static error UnmarshalBinary(this ref sum128 s, slice<byte> b) {
    if (len(b) < len(magic128) || ((@string)(b[..(int)(len(magic128))])) != magic128) {
        return errors.New("hash/fnv: invalid hash state identifier"u8);
    }
    if (len(b) != marshaledSize128) {
        return errors.New("hash/fnv: invalid hash state size"u8);
    }
    s[0] = byteorder.BeUint64(b[4..]);
    s[1] = byteorder.BeUint64(b[12..]);
    return default!;
}

[GoRecv] internal static error UnmarshalBinary(this ref sum128a s, slice<byte> b) {
    if (len(b) < len(magic128a) || ((@string)(b[..(int)(len(magic128a))])) != magic128a) {
        return errors.New("hash/fnv: invalid hash state identifier"u8);
    }
    if (len(b) != marshaledSize128) {
        return errors.New("hash/fnv: invalid hash state size"u8);
    }
    s[0] = byteorder.BeUint64(b[4..]);
    s[1] = byteorder.BeUint64(b[12..]);
    return default!;
}

} // end fnv_package
