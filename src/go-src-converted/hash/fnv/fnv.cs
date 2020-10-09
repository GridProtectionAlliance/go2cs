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
// package fnv -- go2cs converted at 2020 October 09 04:50:10 UTC
// import "hash/fnv" ==> using fnv = go.hash.fnv_package
// Original source: C:\Go\src\hash\fnv\fnv.go
using errors = go.errors_package;
using hash = go.hash_package;
using bits = go.math.bits_package;
using static go.builtin;

namespace go {
namespace hash
{
    public static partial class fnv_package
    {
        private partial struct sum32 // : uint
        {
        }
        private partial struct sum32a // : uint
        {
        }
        private partial struct sum64 // : ulong
        {
        }
        private partial struct sum64a // : ulong
        {
        }
        private partial struct sum128 // : array<ulong>
        {
        }
        private partial struct sum128a // : array<ulong>
        {
        }
        private static readonly long offset32 = (long)2166136261L;
        private static readonly ulong offset64 = (ulong)14695981039346656037UL;
        private static readonly ulong offset128Lower = (ulong)0x62b821756295c58dUL;
        private static readonly ulong offset128Higher = (ulong)0x6c62272e07bb0142UL;
        private static readonly long prime32 = (long)16777619L;
        private static readonly long prime64 = (long)1099511628211L;
        private static readonly ulong prime128Lower = (ulong)0x13bUL;
        private static readonly long prime128Shift = (long)24L;


        // New32 returns a new 32-bit FNV-1 hash.Hash.
        // Its Sum method will lay the value out in big-endian byte order.
        public static hash.Hash32 New32()
        {
            ref sum32 s = ref heap(offset32, out ptr<sum32> _addr_s);
            return _addr_s;
        }

        // New32a returns a new 32-bit FNV-1a hash.Hash.
        // Its Sum method will lay the value out in big-endian byte order.
        public static hash.Hash32 New32a()
        {
            ref sum32a s = ref heap(offset32, out ptr<sum32a> _addr_s);
            return _addr_s;
        }

        // New64 returns a new 64-bit FNV-1 hash.Hash.
        // Its Sum method will lay the value out in big-endian byte order.
        public static hash.Hash64 New64()
        {
            ref sum64 s = ref heap(offset64, out ptr<sum64> _addr_s);
            return _addr_s;
        }

        // New64a returns a new 64-bit FNV-1a hash.Hash.
        // Its Sum method will lay the value out in big-endian byte order.
        public static hash.Hash64 New64a()
        {
            ref sum64a s = ref heap(offset64, out ptr<sum64a> _addr_s);
            return _addr_s;
        }

        // New128 returns a new 128-bit FNV-1 hash.Hash.
        // Its Sum method will lay the value out in big-endian byte order.
        public static hash.Hash New128()
        {
            ref sum128 s = ref heap(out ptr<sum128> _addr_s);
            s[0L] = offset128Higher;
            s[1L] = offset128Lower;
            return _addr_s;
        }

        // New128a returns a new 128-bit FNV-1a hash.Hash.
        // Its Sum method will lay the value out in big-endian byte order.
        public static hash.Hash New128a()
        {
            ref sum128a s = ref heap(out ptr<sum128a> _addr_s);
            s[0L] = offset128Higher;
            s[1L] = offset128Lower;
            return _addr_s;
        }

        private static void Reset(this ptr<sum32> _addr_s)
        {
            ref sum32 s = ref _addr_s.val;

            s.val = offset32;
        }
        private static void Reset(this ptr<sum32a> _addr_s)
        {
            ref sum32a s = ref _addr_s.val;

            s.val = offset32;
        }
        private static void Reset(this ptr<sum64> _addr_s)
        {
            ref sum64 s = ref _addr_s.val;

            s.val = offset64;
        }
        private static void Reset(this ptr<sum64a> _addr_s)
        {
            ref sum64a s = ref _addr_s.val;

            s.val = offset64;
        }
        private static void Reset(this ptr<sum128> _addr_s)
        {
            ref sum128 s = ref _addr_s.val;

            s[0L] = offset128Higher;

            s[1L] = offset128Lower;
        }
        private static void Reset(this ptr<sum128a> _addr_s)
        {
            ref sum128a s = ref _addr_s.val;

            s[0L] = offset128Higher;

            s[1L] = offset128Lower;
        }

        private static uint Sum32(this ptr<sum32> _addr_s)
        {
            ref sum32 s = ref _addr_s.val;

            return uint32(s.val);
        }
        private static uint Sum32(this ptr<sum32a> _addr_s)
        {
            ref sum32a s = ref _addr_s.val;

            return uint32(s.val);
        }
        private static ulong Sum64(this ptr<sum64> _addr_s)
        {
            ref sum64 s = ref _addr_s.val;

            return uint64(s.val);
        }
        private static ulong Sum64(this ptr<sum64a> _addr_s)
        {
            ref sum64a s = ref _addr_s.val;

            return uint64(s.val);
        }

        private static (long, error) Write(this ptr<sum32> _addr_s, slice<byte> data)
        {
            long _p0 = default;
            error _p0 = default!;
            ref sum32 s = ref _addr_s.val;

            var hash = s.val;
            foreach (var (_, c) in data)
            {
                hash *= prime32;
                hash ^= sum32(c);
            }
            s.val = hash;
            return (len(data), error.As(null!)!);

        }

        private static (long, error) Write(this ptr<sum32a> _addr_s, slice<byte> data)
        {
            long _p0 = default;
            error _p0 = default!;
            ref sum32a s = ref _addr_s.val;

            var hash = s.val;
            foreach (var (_, c) in data)
            {
                hash ^= sum32a(c);
                hash *= prime32;
            }
            s.val = hash;
            return (len(data), error.As(null!)!);

        }

        private static (long, error) Write(this ptr<sum64> _addr_s, slice<byte> data)
        {
            long _p0 = default;
            error _p0 = default!;
            ref sum64 s = ref _addr_s.val;

            var hash = s.val;
            foreach (var (_, c) in data)
            {
                hash *= prime64;
                hash ^= sum64(c);
            }
            s.val = hash;
            return (len(data), error.As(null!)!);

        }

        private static (long, error) Write(this ptr<sum64a> _addr_s, slice<byte> data)
        {
            long _p0 = default;
            error _p0 = default!;
            ref sum64a s = ref _addr_s.val;

            var hash = s.val;
            foreach (var (_, c) in data)
            {
                hash ^= sum64a(c);
                hash *= prime64;
            }
            s.val = hash;
            return (len(data), error.As(null!)!);

        }

        private static (long, error) Write(this ptr<sum128> _addr_s, slice<byte> data)
        {
            long _p0 = default;
            error _p0 = default!;
            ref sum128 s = ref _addr_s.val;

            foreach (var (_, c) in data)
            { 
                // Compute the multiplication
                var (s0, s1) = bits.Mul64(prime128Lower, s[1L]);
                s0 += s[1L] << (int)(prime128Shift) + prime128Lower * s[0L]; 
                // Update the values
                s[1L] = s1;
                s[0L] = s0;
                s[1L] ^= uint64(c);

            }
            return (len(data), error.As(null!)!);

        }

        private static (long, error) Write(this ptr<sum128a> _addr_s, slice<byte> data)
        {
            long _p0 = default;
            error _p0 = default!;
            ref sum128a s = ref _addr_s.val;

            foreach (var (_, c) in data)
            {
                s[1L] ^= uint64(c); 
                // Compute the multiplication
                var (s0, s1) = bits.Mul64(prime128Lower, s[1L]);
                s0 += s[1L] << (int)(prime128Shift) + prime128Lower * s[0L]; 
                // Update the values
                s[1L] = s1;
                s[0L] = s0;

            }
            return (len(data), error.As(null!)!);

        }

        private static long Size(this ptr<sum32> _addr_s)
        {
            ref sum32 s = ref _addr_s.val;

            return 4L;
        }
        private static long Size(this ptr<sum32a> _addr_s)
        {
            ref sum32a s = ref _addr_s.val;

            return 4L;
        }
        private static long Size(this ptr<sum64> _addr_s)
        {
            ref sum64 s = ref _addr_s.val;

            return 8L;
        }
        private static long Size(this ptr<sum64a> _addr_s)
        {
            ref sum64a s = ref _addr_s.val;

            return 8L;
        }
        private static long Size(this ptr<sum128> _addr_s)
        {
            ref sum128 s = ref _addr_s.val;

            return 16L;
        }
        private static long Size(this ptr<sum128a> _addr_s)
        {
            ref sum128a s = ref _addr_s.val;

            return 16L;
        }

        private static long BlockSize(this ptr<sum32> _addr_s)
        {
            ref sum32 s = ref _addr_s.val;

            return 1L;
        }
        private static long BlockSize(this ptr<sum32a> _addr_s)
        {
            ref sum32a s = ref _addr_s.val;

            return 1L;
        }
        private static long BlockSize(this ptr<sum64> _addr_s)
        {
            ref sum64 s = ref _addr_s.val;

            return 1L;
        }
        private static long BlockSize(this ptr<sum64a> _addr_s)
        {
            ref sum64a s = ref _addr_s.val;

            return 1L;
        }
        private static long BlockSize(this ptr<sum128> _addr_s)
        {
            ref sum128 s = ref _addr_s.val;

            return 1L;
        }
        private static long BlockSize(this ptr<sum128a> _addr_s)
        {
            ref sum128a s = ref _addr_s.val;

            return 1L;
        }

        private static slice<byte> Sum(this ptr<sum32> _addr_s, slice<byte> @in)
        {
            ref sum32 s = ref _addr_s.val;

            var v = uint32(s.val);
            return append(in, byte(v >> (int)(24L)), byte(v >> (int)(16L)), byte(v >> (int)(8L)), byte(v));
        }

        private static slice<byte> Sum(this ptr<sum32a> _addr_s, slice<byte> @in)
        {
            ref sum32a s = ref _addr_s.val;

            var v = uint32(s.val);
            return append(in, byte(v >> (int)(24L)), byte(v >> (int)(16L)), byte(v >> (int)(8L)), byte(v));
        }

        private static slice<byte> Sum(this ptr<sum64> _addr_s, slice<byte> @in)
        {
            ref sum64 s = ref _addr_s.val;

            var v = uint64(s.val);
            return append(in, byte(v >> (int)(56L)), byte(v >> (int)(48L)), byte(v >> (int)(40L)), byte(v >> (int)(32L)), byte(v >> (int)(24L)), byte(v >> (int)(16L)), byte(v >> (int)(8L)), byte(v));
        }

        private static slice<byte> Sum(this ptr<sum64a> _addr_s, slice<byte> @in)
        {
            ref sum64a s = ref _addr_s.val;

            var v = uint64(s.val);
            return append(in, byte(v >> (int)(56L)), byte(v >> (int)(48L)), byte(v >> (int)(40L)), byte(v >> (int)(32L)), byte(v >> (int)(24L)), byte(v >> (int)(16L)), byte(v >> (int)(8L)), byte(v));
        }

        private static slice<byte> Sum(this ptr<sum128> _addr_s, slice<byte> @in)
        {
            ref sum128 s = ref _addr_s.val;

            return append(in, byte(s[0L] >> (int)(56L)), byte(s[0L] >> (int)(48L)), byte(s[0L] >> (int)(40L)), byte(s[0L] >> (int)(32L)), byte(s[0L] >> (int)(24L)), byte(s[0L] >> (int)(16L)), byte(s[0L] >> (int)(8L)), byte(s[0L]), byte(s[1L] >> (int)(56L)), byte(s[1L] >> (int)(48L)), byte(s[1L] >> (int)(40L)), byte(s[1L] >> (int)(32L)), byte(s[1L] >> (int)(24L)), byte(s[1L] >> (int)(16L)), byte(s[1L] >> (int)(8L)), byte(s[1L]));
        }

        private static slice<byte> Sum(this ptr<sum128a> _addr_s, slice<byte> @in)
        {
            ref sum128a s = ref _addr_s.val;

            return append(in, byte(s[0L] >> (int)(56L)), byte(s[0L] >> (int)(48L)), byte(s[0L] >> (int)(40L)), byte(s[0L] >> (int)(32L)), byte(s[0L] >> (int)(24L)), byte(s[0L] >> (int)(16L)), byte(s[0L] >> (int)(8L)), byte(s[0L]), byte(s[1L] >> (int)(56L)), byte(s[1L] >> (int)(48L)), byte(s[1L] >> (int)(40L)), byte(s[1L] >> (int)(32L)), byte(s[1L] >> (int)(24L)), byte(s[1L] >> (int)(16L)), byte(s[1L] >> (int)(8L)), byte(s[1L]));
        }

        private static readonly @string magic32 = (@string)"fnv\x01";
        private static readonly @string magic32a = (@string)"fnv\x02";
        private static readonly @string magic64 = (@string)"fnv\x03";
        private static readonly @string magic64a = (@string)"fnv\x04";
        private static readonly @string magic128 = (@string)"fnv\x05";
        private static readonly @string magic128a = (@string)"fnv\x06";
        private static readonly var marshaledSize32 = len(magic32) + 4L;
        private static readonly var marshaledSize64 = len(magic64) + 8L;
        private static readonly var marshaledSize128 = len(magic128) + 8L * 2L;


        private static (slice<byte>, error) MarshalBinary(this ptr<sum32> _addr_s)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref sum32 s = ref _addr_s.val;

            var b = make_slice<byte>(0L, marshaledSize32);
            b = append(b, magic32);
            b = appendUint32(b, uint32(s.val));
            return (b, error.As(null!)!);
        }

        private static (slice<byte>, error) MarshalBinary(this ptr<sum32a> _addr_s)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref sum32a s = ref _addr_s.val;

            var b = make_slice<byte>(0L, marshaledSize32);
            b = append(b, magic32a);
            b = appendUint32(b, uint32(s.val));
            return (b, error.As(null!)!);
        }

        private static (slice<byte>, error) MarshalBinary(this ptr<sum64> _addr_s)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref sum64 s = ref _addr_s.val;

            var b = make_slice<byte>(0L, marshaledSize64);
            b = append(b, magic64);
            b = appendUint64(b, uint64(s.val));
            return (b, error.As(null!)!);
        }

        private static (slice<byte>, error) MarshalBinary(this ptr<sum64a> _addr_s)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref sum64a s = ref _addr_s.val;

            var b = make_slice<byte>(0L, marshaledSize64);
            b = append(b, magic64a);
            b = appendUint64(b, uint64(s.val));
            return (b, error.As(null!)!);
        }

        private static (slice<byte>, error) MarshalBinary(this ptr<sum128> _addr_s)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref sum128 s = ref _addr_s.val;

            var b = make_slice<byte>(0L, marshaledSize128);
            b = append(b, magic128);
            b = appendUint64(b, s[0L]);
            b = appendUint64(b, s[1L]);
            return (b, error.As(null!)!);
        }

        private static (slice<byte>, error) MarshalBinary(this ptr<sum128a> _addr_s)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref sum128a s = ref _addr_s.val;

            var b = make_slice<byte>(0L, marshaledSize128);
            b = append(b, magic128a);
            b = appendUint64(b, s[0L]);
            b = appendUint64(b, s[1L]);
            return (b, error.As(null!)!);
        }

        private static error UnmarshalBinary(this ptr<sum32> _addr_s, slice<byte> b)
        {
            ref sum32 s = ref _addr_s.val;

            if (len(b) < len(magic32) || string(b[..len(magic32)]) != magic32)
            {
                return error.As(errors.New("hash/fnv: invalid hash state identifier"))!;
            }

            if (len(b) != marshaledSize32)
            {
                return error.As(errors.New("hash/fnv: invalid hash state size"))!;
            }

            s.val = sum32(readUint32(b[4L..]));
            return error.As(null!)!;

        }

        private static error UnmarshalBinary(this ptr<sum32a> _addr_s, slice<byte> b)
        {
            ref sum32a s = ref _addr_s.val;

            if (len(b) < len(magic32a) || string(b[..len(magic32a)]) != magic32a)
            {
                return error.As(errors.New("hash/fnv: invalid hash state identifier"))!;
            }

            if (len(b) != marshaledSize32)
            {
                return error.As(errors.New("hash/fnv: invalid hash state size"))!;
            }

            s.val = sum32a(readUint32(b[4L..]));
            return error.As(null!)!;

        }

        private static error UnmarshalBinary(this ptr<sum64> _addr_s, slice<byte> b)
        {
            ref sum64 s = ref _addr_s.val;

            if (len(b) < len(magic64) || string(b[..len(magic64)]) != magic64)
            {
                return error.As(errors.New("hash/fnv: invalid hash state identifier"))!;
            }

            if (len(b) != marshaledSize64)
            {
                return error.As(errors.New("hash/fnv: invalid hash state size"))!;
            }

            s.val = sum64(readUint64(b[4L..]));
            return error.As(null!)!;

        }

        private static error UnmarshalBinary(this ptr<sum64a> _addr_s, slice<byte> b)
        {
            ref sum64a s = ref _addr_s.val;

            if (len(b) < len(magic64a) || string(b[..len(magic64a)]) != magic64a)
            {
                return error.As(errors.New("hash/fnv: invalid hash state identifier"))!;
            }

            if (len(b) != marshaledSize64)
            {
                return error.As(errors.New("hash/fnv: invalid hash state size"))!;
            }

            s.val = sum64a(readUint64(b[4L..]));
            return error.As(null!)!;

        }

        private static error UnmarshalBinary(this ptr<sum128> _addr_s, slice<byte> b)
        {
            ref sum128 s = ref _addr_s.val;

            if (len(b) < len(magic128) || string(b[..len(magic128)]) != magic128)
            {
                return error.As(errors.New("hash/fnv: invalid hash state identifier"))!;
            }

            if (len(b) != marshaledSize128)
            {
                return error.As(errors.New("hash/fnv: invalid hash state size"))!;
            }

            s[0L] = readUint64(b[4L..]);
            s[1L] = readUint64(b[12L..]);
            return error.As(null!)!;

        }

        private static error UnmarshalBinary(this ptr<sum128a> _addr_s, slice<byte> b)
        {
            ref sum128a s = ref _addr_s.val;

            if (len(b) < len(magic128a) || string(b[..len(magic128a)]) != magic128a)
            {
                return error.As(errors.New("hash/fnv: invalid hash state identifier"))!;
            }

            if (len(b) != marshaledSize128)
            {
                return error.As(errors.New("hash/fnv: invalid hash state size"))!;
            }

            s[0L] = readUint64(b[4L..]);
            s[1L] = readUint64(b[12L..]);
            return error.As(null!)!;

        }

        private static uint readUint32(slice<byte> b)
        {
            _ = b[3L];
            return uint32(b[3L]) | uint32(b[2L]) << (int)(8L) | uint32(b[1L]) << (int)(16L) | uint32(b[0L]) << (int)(24L);
        }

        private static slice<byte> appendUint32(slice<byte> b, uint x)
        {
            array<byte> a = new array<byte>(new byte[] { byte(x>>24), byte(x>>16), byte(x>>8), byte(x) });
            return append(b, a[..]);
        }

        private static slice<byte> appendUint64(slice<byte> b, ulong x)
        {
            array<byte> a = new array<byte>(new byte[] { byte(x>>56), byte(x>>48), byte(x>>40), byte(x>>32), byte(x>>24), byte(x>>16), byte(x>>8), byte(x) });
            return append(b, a[..]);
        }

        private static ulong readUint64(slice<byte> b)
        {
            _ = b[7L];
            return uint64(b[7L]) | uint64(b[6L]) << (int)(8L) | uint64(b[5L]) << (int)(16L) | uint64(b[4L]) << (int)(24L) | uint64(b[3L]) << (int)(32L) | uint64(b[2L]) << (int)(40L) | uint64(b[1L]) << (int)(48L) | uint64(b[0L]) << (int)(56L);
        }
    }
}}
