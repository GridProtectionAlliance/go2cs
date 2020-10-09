// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Hashing algorithm inspired by
//   xxhash: https://code.google.com/p/xxhash/
// cityhash: https://code.google.com/p/cityhash/

// +build 386 arm mips mipsle

// package runtime -- go2cs converted at 2020 October 09 04:45:59 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\hash32.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
 
        // Constants for multiplication: four random odd 32-bit numbers.
        private static readonly long m1 = (long)3168982561L;
        private static readonly long m2 = (long)3339683297L;
        private static readonly long m3 = (long)832293441L;
        private static readonly long m4 = (long)2336365089L;


        private static System.UIntPtr memhashFallback(unsafe.Pointer p, System.UIntPtr seed, System.UIntPtr s)
        {
            var h = uint32(seed + s * hashkey[0L]);
tail:

            if (s == 0L)             else if (s < 4L) 
                h ^= uint32(new ptr<ptr<ptr<byte>>>(p));
                h ^= uint32(new ptr<ptr<ptr<byte>>>(add(p, s >> (int)(1L)))) << (int)(8L);
                h ^= uint32(new ptr<ptr<ptr<byte>>>(add(p, s - 1L))) << (int)(16L);
                h = rotl_15(h * m1) * m2;
            else if (s == 4L) 
                h ^= readUnaligned32(p);
                h = rotl_15(h * m1) * m2;
            else if (s <= 8L) 
                h ^= readUnaligned32(p);
                h = rotl_15(h * m1) * m2;
                h ^= readUnaligned32(add(p, s - 4L));
                h = rotl_15(h * m1) * m2;
            else if (s <= 16L) 
                h ^= readUnaligned32(p);
                h = rotl_15(h * m1) * m2;
                h ^= readUnaligned32(add(p, 4L));
                h = rotl_15(h * m1) * m2;
                h ^= readUnaligned32(add(p, s - 8L));
                h = rotl_15(h * m1) * m2;
                h ^= readUnaligned32(add(p, s - 4L));
                h = rotl_15(h * m1) * m2;
            else 
                var v1 = h;
                var v2 = uint32(seed * hashkey[1L]);
                var v3 = uint32(seed * hashkey[2L]);
                var v4 = uint32(seed * hashkey[3L]);
                while (s >= 16L)
                {
                    v1 ^= readUnaligned32(p);
                    v1 = rotl_15(v1 * m1) * m2;
                    p = add(p, 4L);
                    v2 ^= readUnaligned32(p);
                    v2 = rotl_15(v2 * m2) * m3;
                    p = add(p, 4L);
                    v3 ^= readUnaligned32(p);
                    v3 = rotl_15(v3 * m3) * m4;
                    p = add(p, 4L);
                    v4 ^= readUnaligned32(p);
                    v4 = rotl_15(v4 * m4) * m1;
                    p = add(p, 4L);
                    s -= 16L;
                }

                h = v1 ^ v2 ^ v3 ^ v4;
                goto tail;
                        h ^= h >> (int)(17L);
            h *= m3;
            h ^= h >> (int)(13L);
            h *= m4;
            h ^= h >> (int)(16L);
            return uintptr(h);

        }

        private static System.UIntPtr memhash32Fallback(unsafe.Pointer p, System.UIntPtr seed)
        {
            var h = uint32(seed + 4L * hashkey[0L]);
            h ^= readUnaligned32(p);
            h = rotl_15(h * m1) * m2;
            h ^= h >> (int)(17L);
            h *= m3;
            h ^= h >> (int)(13L);
            h *= m4;
            h ^= h >> (int)(16L);
            return uintptr(h);
        }

        private static System.UIntPtr memhash64Fallback(unsafe.Pointer p, System.UIntPtr seed)
        {
            var h = uint32(seed + 8L * hashkey[0L]);
            h ^= readUnaligned32(p);
            h = rotl_15(h * m1) * m2;
            h ^= readUnaligned32(add(p, 4L));
            h = rotl_15(h * m1) * m2;
            h ^= h >> (int)(17L);
            h *= m3;
            h ^= h >> (int)(13L);
            h *= m4;
            h ^= h >> (int)(16L);
            return uintptr(h);
        }

        // Note: in order to get the compiler to issue rotl instructions, we
        // need to constant fold the shift amount by hand.
        // TODO: convince the compiler to issue rotl instructions after inlining.
        private static uint rotl_15(uint x)
        {
            return (x << (int)(15L)) | (x >> (int)((32L - 15L)));
        }
    }
}
