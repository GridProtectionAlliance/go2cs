// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Hashing algorithm inspired by
//   xxhash: https://code.google.com/p/xxhash/
// cityhash: https://code.google.com/p/cityhash/

// +build amd64 arm64 mips64 mips64le ppc64 ppc64le riscv64 s390x wasm

// package runtime -- go2cs converted at 2020 October 09 04:46:00 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\hash64.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
 
        // Constants for multiplication: four random odd 64-bit numbers.
        private static readonly ulong m1 = (ulong)16877499708836156737UL;
        private static readonly long m2 = (long)2820277070424839065L;
        private static readonly ulong m3 = (ulong)9497967016996688599UL;
        private static readonly ulong m4 = (ulong)15839092249703872147UL;


        private static System.UIntPtr memhashFallback(unsafe.Pointer p, System.UIntPtr seed, System.UIntPtr s)
        {
            var h = uint64(seed + s * hashkey[0L]);
tail:


            if (s == 0L)             else if (s < 4L) 
                h ^= uint64(new ptr<ptr<ptr<byte>>>(p));
                h ^= uint64(new ptr<ptr<ptr<byte>>>(add(p, s >> (int)(1L)))) << (int)(8L);
                h ^= uint64(new ptr<ptr<ptr<byte>>>(add(p, s - 1L))) << (int)(16L);
                h = rotl_31(h * m1) * m2;
            else if (s <= 8L) 
                h ^= uint64(readUnaligned32(p));
                h ^= uint64(readUnaligned32(add(p, s - 4L))) << (int)(32L);
                h = rotl_31(h * m1) * m2;
            else if (s <= 16L) 
                h ^= readUnaligned64(p);
                h = rotl_31(h * m1) * m2;
                h ^= readUnaligned64(add(p, s - 8L));
                h = rotl_31(h * m1) * m2;
            else if (s <= 32L) 
                h ^= readUnaligned64(p);
                h = rotl_31(h * m1) * m2;
                h ^= readUnaligned64(add(p, 8L));
                h = rotl_31(h * m1) * m2;
                h ^= readUnaligned64(add(p, s - 16L));
                h = rotl_31(h * m1) * m2;
                h ^= readUnaligned64(add(p, s - 8L));
                h = rotl_31(h * m1) * m2;
            else 
                var v1 = h;
                var v2 = uint64(seed * hashkey[1L]);
                var v3 = uint64(seed * hashkey[2L]);
                var v4 = uint64(seed * hashkey[3L]);
                while (s >= 32L)
                {
                    v1 ^= readUnaligned64(p);
                    v1 = rotl_31(v1 * m1) * m2;
                    p = add(p, 8L);
                    v2 ^= readUnaligned64(p);
                    v2 = rotl_31(v2 * m2) * m3;
                    p = add(p, 8L);
                    v3 ^= readUnaligned64(p);
                    v3 = rotl_31(v3 * m3) * m4;
                    p = add(p, 8L);
                    v4 ^= readUnaligned64(p);
                    v4 = rotl_31(v4 * m4) * m1;
                    p = add(p, 8L);
                    s -= 32L;
                }

                h = v1 ^ v2 ^ v3 ^ v4;
                goto tail;
                        h ^= h >> (int)(29L);
            h *= m3;
            h ^= h >> (int)(32L);
            return uintptr(h);

        }

        private static System.UIntPtr memhash32Fallback(unsafe.Pointer p, System.UIntPtr seed)
        {
            var h = uint64(seed + 4L * hashkey[0L]);
            var v = uint64(readUnaligned32(p));
            h ^= v;
            h ^= v << (int)(32L);
            h = rotl_31(h * m1) * m2;
            h ^= h >> (int)(29L);
            h *= m3;
            h ^= h >> (int)(32L);
            return uintptr(h);
        }

        private static System.UIntPtr memhash64Fallback(unsafe.Pointer p, System.UIntPtr seed)
        {
            var h = uint64(seed + 8L * hashkey[0L]);
            h ^= uint64(readUnaligned32(p)) | uint64(readUnaligned32(add(p, 4L))) << (int)(32L);
            h = rotl_31(h * m1) * m2;
            h ^= h >> (int)(29L);
            h *= m3;
            h ^= h >> (int)(32L);
            return uintptr(h);
        }

        // Note: in order to get the compiler to issue rotl instructions, we
        // need to constant fold the shift amount by hand.
        // TODO: convince the compiler to issue rotl instructions after inlining.
        private static ulong rotl_31(ulong x)
        {
            return (x << (int)(31L)) | (x >> (int)((64L - 31L)));
        }
    }
}
