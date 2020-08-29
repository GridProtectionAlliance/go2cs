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
// package fnv -- go2cs converted at 2020 August 29 08:23:22 UTC
// import "hash/fnv" ==> using fnv = go.hash.fnv_package
// Original source: C:\Go\src\hash\fnv\fnv.go
using errors = go.errors_package;
using hash = go.hash_package;
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
        }        private static readonly long offset32 = 2166136261L;
        private static readonly ulong offset64 = 14695981039346656037UL;
        private static readonly ulong offset128Lower = 0x62b821756295c58dUL;
        private static readonly ulong offset128Higher = 0x6c62272e07bb0142UL;
        private static readonly long prime32 = 16777619L;
        private static readonly long prime64 = 1099511628211L;
        private static readonly ulong prime128Lower = 0x13bUL;
        private static readonly long prime128Shift = 24L;

        // New32 returns a new 32-bit FNV-1 hash.Hash.
        // Its Sum method will lay the value out in big-endian byte order.
        public static hash.Hash32 New32()
        {
            sum32 s = offset32;
            return ref s;
        }

        // New32a returns a new 32-bit FNV-1a hash.Hash.
        // Its Sum method will lay the value out in big-endian byte order.
        public static hash.Hash32 New32a()
        {
            sum32a s = offset32;
            return ref s;
        }

        // New64 returns a new 64-bit FNV-1 hash.Hash.
        // Its Sum method will lay the value out in big-endian byte order.
        public static hash.Hash64 New64()
        {
            sum64 s = offset64;
            return ref s;
        }

        // New64a returns a new 64-bit FNV-1a hash.Hash.
        // Its Sum method will lay the value out in big-endian byte order.
        public static hash.Hash64 New64a()
        {
            sum64a s = offset64;
            return ref s;
        }

        // New128 returns a new 128-bit FNV-1 hash.Hash.
        // Its Sum method will lay the value out in big-endian byte order.
        public static hash.Hash New128()
        {
            sum128 s = default;
            s[0L] = offset128Higher;
            s[1L] = offset128Lower;
            return ref s;
        }

        // New128a returns a new 128-bit FNV-1a hash.Hash.
        // Its Sum method will lay the value out in big-endian byte order.
        public static hash.Hash New128a()
        {
            sum128a s = default;
            s[0L] = offset128Higher;
            s[1L] = offset128Lower;
            return ref s;
        }

        private static void Reset(this ref sum32 s)
        {
            s.Value = offset32;

        }
        private static void Reset(this ref sum32a s)
        {
            s.Value = offset32;

        }
        private static void Reset(this ref sum64 s)
        {
            s.Value = offset64;

        }
        private static void Reset(this ref sum64a s)
        {
            s.Value = offset64;

        }
        private static void Reset(this ref sum128 s)
        {
            s[0L] = offset128Higher;

            s[1L] = offset128Lower;

        }
        private static void Reset(this ref sum128a s)
        {
            s[0L] = offset128Higher;

            s[1L] = offset128Lower;

        }

        private static uint Sum32(this ref sum32 s)
        {
            return uint32(s.Value);
        }
        private static uint Sum32(this ref sum32a s)
        {
            return uint32(s.Value);
        }
        private static ulong Sum64(this ref sum64 s)
        {
            return uint64(s.Value);
        }
        private static ulong Sum64(this ref sum64a s)
        {
            return uint64(s.Value);
        }

        private static (long, error) Write(this ref sum32 s, slice<byte> data)
        {
            var hash = s.Value;
            foreach (var (_, c) in data)
            {
                hash *= prime32;
                hash ^= sum32(c);
            }
            s.Value = hash;
            return (len(data), null);
        }

        private static (long, error) Write(this ref sum32a s, slice<byte> data)
        {
            var hash = s.Value;
            foreach (var (_, c) in data)
            {
                hash ^= sum32a(c);
                hash *= prime32;
            }
            s.Value = hash;
            return (len(data), null);
        }

        private static (long, error) Write(this ref sum64 s, slice<byte> data)
        {
            var hash = s.Value;
            foreach (var (_, c) in data)
            {
                hash *= prime64;
                hash ^= sum64(c);
            }
            s.Value = hash;
            return (len(data), null);
        }

        private static (long, error) Write(this ref sum64a s, slice<byte> data)
        {
            var hash = s.Value;
            foreach (var (_, c) in data)
            {
                hash ^= sum64a(c);
                hash *= prime64;
            }
            s.Value = hash;
            return (len(data), null);
        }

        private static (long, error) Write(this ref sum128 s, slice<byte> data)
        {
            foreach (var (_, c) in data)
            { 
                // Compute the multiplication in 4 parts to simplify carrying
                var s1l = (s[1L] & 0xffffffffUL) * prime128Lower;
                var s1h = (s[1L] >> (int)(32L)) * prime128Lower;
                var s0l = (s[0L] & 0xffffffffUL) * prime128Lower + (s[1L] & 0xffffffffUL) << (int)(prime128Shift);
                var s0h = (s[0L] >> (int)(32L)) * prime128Lower + (s[1L] >> (int)(32L)) << (int)(prime128Shift); 
                // Carries
                s1h += s1l >> (int)(32L);
                s0l += s1h >> (int)(32L);
                s0h += s0l >> (int)(32L); 
                // Update the values
                s[1L] = (s1l & 0xffffffffUL) + (s1h << (int)(32L));
                s[0L] = (s0l & 0xffffffffUL) + (s0h << (int)(32L));
                s[1L] ^= uint64(c);
            }
            return (len(data), null);
        }

        private static (long, error) Write(this ref sum128a s, slice<byte> data)
        {
            foreach (var (_, c) in data)
            {
                s[1L] ^= uint64(c); 
                // Compute the multiplication in 4 parts to simplify carrying
                var s1l = (s[1L] & 0xffffffffUL) * prime128Lower;
                var s1h = (s[1L] >> (int)(32L)) * prime128Lower;
                var s0l = (s[0L] & 0xffffffffUL) * prime128Lower + (s[1L] & 0xffffffffUL) << (int)(prime128Shift);
                var s0h = (s[0L] >> (int)(32L)) * prime128Lower + (s[1L] >> (int)(32L)) << (int)(prime128Shift); 
                // Carries
                s1h += s1l >> (int)(32L);
                s0l += s1h >> (int)(32L);
                s0h += s0l >> (int)(32L); 
                // Update the values
                s[1L] = (s1l & 0xffffffffUL) + (s1h << (int)(32L));
                s[0L] = (s0l & 0xffffffffUL) + (s0h << (int)(32L));
            }
            return (len(data), null);
        }

        private static long Size(this ref sum32 s)
        {
            return 4L;
        }
        private static long Size(this ref sum32a s)
        {
            return 4L;
        }
        private static long Size(this ref sum64 s)
        {
            return 8L;
        }
        private static long Size(this ref sum64a s)
        {
            return 8L;
        }
        private static long Size(this ref sum128 s)
        {
            return 16L;
        }
        private static long Size(this ref sum128a s)
        {
            return 16L;
        }

        private static long BlockSize(this ref sum32 s)
        {
            return 1L;
        }
        private static long BlockSize(this ref sum32a s)
        {
            return 1L;
        }
        private static long BlockSize(this ref sum64 s)
        {
            return 1L;
        }
        private static long BlockSize(this ref sum64a s)
        {
            return 1L;
        }
        private static long BlockSize(this ref sum128 s)
        {
            return 1L;
        }
        private static long BlockSize(this ref sum128a s)
        {
            return 1L;
        }

        private static slice<byte> Sum(this ref sum32 s, slice<byte> @in)
        {
            var v = uint32(s.Value);
            return append(in, byte(v >> (int)(24L)), byte(v >> (int)(16L)), byte(v >> (int)(8L)), byte(v));
        }

        private static slice<byte> Sum(this ref sum32a s, slice<byte> @in)
        {
            var v = uint32(s.Value);
            return append(in, byte(v >> (int)(24L)), byte(v >> (int)(16L)), byte(v >> (int)(8L)), byte(v));
        }

        private static slice<byte> Sum(this ref sum64 s, slice<byte> @in)
        {
            var v = uint64(s.Value);
            return append(in, byte(v >> (int)(56L)), byte(v >> (int)(48L)), byte(v >> (int)(40L)), byte(v >> (int)(32L)), byte(v >> (int)(24L)), byte(v >> (int)(16L)), byte(v >> (int)(8L)), byte(v));
        }

        private static slice<byte> Sum(this ref sum64a s, slice<byte> @in)
        {
            var v = uint64(s.Value);
            return append(in, byte(v >> (int)(56L)), byte(v >> (int)(48L)), byte(v >> (int)(40L)), byte(v >> (int)(32L)), byte(v >> (int)(24L)), byte(v >> (int)(16L)), byte(v >> (int)(8L)), byte(v));
        }

        private static slice<byte> Sum(this ref sum128 s, slice<byte> @in)
        {
            return append(in, byte(s[0L] >> (int)(56L)), byte(s[0L] >> (int)(48L)), byte(s[0L] >> (int)(40L)), byte(s[0L] >> (int)(32L)), byte(s[0L] >> (int)(24L)), byte(s[0L] >> (int)(16L)), byte(s[0L] >> (int)(8L)), byte(s[0L]), byte(s[1L] >> (int)(56L)), byte(s[1L] >> (int)(48L)), byte(s[1L] >> (int)(40L)), byte(s[1L] >> (int)(32L)), byte(s[1L] >> (int)(24L)), byte(s[1L] >> (int)(16L)), byte(s[1L] >> (int)(8L)), byte(s[1L]));
        }

        private static slice<byte> Sum(this ref sum128a s, slice<byte> @in)
        {
            return append(in, byte(s[0L] >> (int)(56L)), byte(s[0L] >> (int)(48L)), byte(s[0L] >> (int)(40L)), byte(s[0L] >> (int)(32L)), byte(s[0L] >> (int)(24L)), byte(s[0L] >> (int)(16L)), byte(s[0L] >> (int)(8L)), byte(s[0L]), byte(s[1L] >> (int)(56L)), byte(s[1L] >> (int)(48L)), byte(s[1L] >> (int)(40L)), byte(s[1L] >> (int)(32L)), byte(s[1L] >> (int)(24L)), byte(s[1L] >> (int)(16L)), byte(s[1L] >> (int)(8L)), byte(s[1L]));
        }

        private static readonly @string magic32 = "fnv\x01";
        private static readonly @string magic32a = "fnv\x02";
        private static readonly @string magic64 = "fnv\x03";
        private static readonly @string magic64a = "fnv\x04";
        private static readonly @string magic128 = "fnv\x05";
        private static readonly @string magic128a = "fnv\x06";
        private static readonly var marshaledSize32 = len(magic32) + 4L;
        private static readonly var marshaledSize64 = len(magic64) + 8L;
        private static readonly var marshaledSize128 = len(magic128) + 8L * 2L;

        private static (slice<byte>, error) MarshalBinary(this ref sum32 s)
        {
            var b = make_slice<byte>(0L, marshaledSize32);
            b = append(b, magic32);
            b = appendUint32(b, uint32(s.Value));
            return (b, null);
        }

        private static (slice<byte>, error) MarshalBinary(this ref sum32a s)
        {
            var b = make_slice<byte>(0L, marshaledSize32);
            b = append(b, magic32a);
            b = appendUint32(b, uint32(s.Value));
            return (b, null);
        }

        private static (slice<byte>, error) MarshalBinary(this ref sum64 s)
        {
            var b = make_slice<byte>(0L, marshaledSize64);
            b = append(b, magic64);
            b = appendUint64(b, uint64(s.Value));
            return (b, null);

        }

        private static (slice<byte>, error) MarshalBinary(this ref sum64a s)
        {
            var b = make_slice<byte>(0L, marshaledSize64);
            b = append(b, magic64a);
            b = appendUint64(b, uint64(s.Value));
            return (b, null);
        }

        private static (slice<byte>, error) MarshalBinary(this ref sum128 s)
        {
            var b = make_slice<byte>(0L, marshaledSize128);
            b = append(b, magic128);
            b = appendUint64(b, s[0L]);
            b = appendUint64(b, s[1L]);
            return (b, null);
        }

        private static (slice<byte>, error) MarshalBinary(this ref sum128a s)
        {
            var b = make_slice<byte>(0L, marshaledSize128);
            b = append(b, magic128a);
            b = appendUint64(b, s[0L]);
            b = appendUint64(b, s[1L]);
            return (b, null);
        }

        private static error UnmarshalBinary(this ref sum32 s, slice<byte> b)
        {
            if (len(b) < len(magic32) || string(b[..len(magic32)]) != magic32)
            {
                return error.As(errors.New("hash/fnv: invalid hash state identifier"));
            }
            if (len(b) != marshaledSize32)
            {
                return error.As(errors.New("hash/fnv: invalid hash state size"));
            }
            s.Value = sum32(readUint32(b[4L..]));
            return error.As(null);
        }

        private static error UnmarshalBinary(this ref sum32a s, slice<byte> b)
        {
            if (len(b) < len(magic32a) || string(b[..len(magic32a)]) != magic32a)
            {
                return error.As(errors.New("hash/fnv: invalid hash state identifier"));
            }
            if (len(b) != marshaledSize32)
            {
                return error.As(errors.New("hash/fnv: invalid hash state size"));
            }
            s.Value = sum32a(readUint32(b[4L..]));
            return error.As(null);
        }

        private static error UnmarshalBinary(this ref sum64 s, slice<byte> b)
        {
            if (len(b) < len(magic64) || string(b[..len(magic64)]) != magic64)
            {
                return error.As(errors.New("hash/fnv: invalid hash state identifier"));
            }
            if (len(b) != marshaledSize64)
            {
                return error.As(errors.New("hash/fnv: invalid hash state size"));
            }
            s.Value = sum64(readUint64(b[4L..]));
            return error.As(null);
        }

        private static error UnmarshalBinary(this ref sum64a s, slice<byte> b)
        {
            if (len(b) < len(magic64a) || string(b[..len(magic64a)]) != magic64a)
            {
                return error.As(errors.New("hash/fnv: invalid hash state identifier"));
            }
            if (len(b) != marshaledSize64)
            {
                return error.As(errors.New("hash/fnv: invalid hash state size"));
            }
            s.Value = sum64a(readUint64(b[4L..]));
            return error.As(null);
        }

        private static error UnmarshalBinary(this ref sum128 s, slice<byte> b)
        {
            if (len(b) < len(magic128) || string(b[..len(magic128)]) != magic128)
            {
                return error.As(errors.New("hash/fnv: invalid hash state identifier"));
            }
            if (len(b) != marshaledSize128)
            {
                return error.As(errors.New("hash/fnv: invalid hash state size"));
            }
            s[0L] = readUint64(b[4L..]);
            s[1L] = readUint64(b[12L..]);
            return error.As(null);
        }

        private static error UnmarshalBinary(this ref sum128a s, slice<byte> b)
        {
            if (len(b) < len(magic128a) || string(b[..len(magic128a)]) != magic128a)
            {
                return error.As(errors.New("hash/fnv: invalid hash state identifier"));
            }
            if (len(b) != marshaledSize128)
            {
                return error.As(errors.New("hash/fnv: invalid hash state size"));
            }
            s[0L] = readUint64(b[4L..]);
            s[1L] = readUint64(b[12L..]);
            return error.As(null);
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
