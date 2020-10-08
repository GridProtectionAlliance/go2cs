// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package maphash provides hash functions on byte sequences.
// These hash functions are intended to be used to implement hash tables or
// other data structures that need to map arbitrary strings or byte
// sequences to a uniform distribution on unsigned 64-bit integers.
//
// The hash functions are collision-resistant but not cryptographically secure.
// (See crypto/sha256 and crypto/sha512 for cryptographic use.)
//
// The hash value of a given byte sequence is consistent within a
// single process, but will be different in different processes.
// package maphash -- go2cs converted at 2020 October 08 03:30:55 UTC
// import "hash/maphash" ==> using maphash = go.hash.maphash_package
// Original source: C:\Go\src\hash\maphash\maphash.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go {
namespace hash
{
    public static partial class maphash_package
    {
        // A Seed is a random value that selects the specific hash function
        // computed by a Hash. If two Hashes use the same Seeds, they
        // will compute the same hash values for any given input.
        // If two Hashes use different Seeds, they are very likely to compute
        // distinct hash values for any given input.
        //
        // A Seed must be initialized by calling MakeSeed.
        // The zero seed is uninitialized and not valid for use with Hash's SetSeed method.
        //
        // Each Seed value is local to a single process and cannot be serialized
        // or otherwise recreated in a different process.
        public partial struct Seed
        {
            public ulong s;
        }

        // A Hash computes a seeded hash of a byte sequence.
        //
        // The zero Hash is a valid Hash ready to use.
        // A zero Hash chooses a random seed for itself during
        // the first call to a Reset, Write, Seed, Sum64, or Seed method.
        // For control over the seed, use SetSeed.
        //
        // The computed hash values depend only on the initial seed and
        // the sequence of bytes provided to the Hash object, not on the way
        // in which the bytes are provided. For example, the three sequences
        //
        //     h.Write([]byte{'f','o','o'})
        //     h.WriteByte('f'); h.WriteByte('o'); h.WriteByte('o')
        //     h.WriteString("foo")
        //
        // all have the same effect.
        //
        // Hashes are intended to be collision-resistant, even for situations
        // where an adversary controls the byte sequences being hashed.
        //
        // A Hash is not safe for concurrent use by multiple goroutines, but a Seed is.
        // If multiple goroutines must compute the same seeded hash,
        // each can declare its own Hash and call SetSeed with a common Seed.
        public partial struct Hash
        {
            public array<Action> _; // not comparable
            public Seed seed; // initial seed used for this hash
            public Seed state; // current hash of all flushed bytes
            public array<byte> buf; // unflushed byte buffer
            public long n; // number of unflushed bytes
        }

        // initSeed seeds the hash if necessary.
        // initSeed is called lazily before any operation that actually uses h.seed/h.state.
        // Note that this does not include Write/WriteByte/WriteString in the case
        // where they only add to h.buf. (If they write too much, they call h.flush,
        // which does call h.initSeed.)
        private static void initSeed(this ptr<Hash> _addr_h)
        {
            ref Hash h = ref _addr_h.val;

            if (h.seed.s == 0L)
            {
                h.setSeed(MakeSeed());
            }

        }

        // WriteByte adds b to the sequence of bytes hashed by h.
        // It never fails; the error result is for implementing io.ByteWriter.
        private static error WriteByte(this ptr<Hash> _addr_h, byte b)
        {
            ref Hash h = ref _addr_h.val;

            if (h.n == len(h.buf))
            {
                h.flush();
            }

            h.buf[h.n] = b;
            h.n++;
            return error.As(null!)!;

        }

        // Write adds b to the sequence of bytes hashed by h.
        // It always writes all of b and never fails; the count and error result are for implementing io.Writer.
        private static (long, error) Write(this ptr<Hash> _addr_h, slice<byte> b)
        {
            long _p0 = default;
            error _p0 = default!;
            ref Hash h = ref _addr_h.val;

            var size = len(b);
            while (h.n + len(b) > len(h.buf))
            {
                var k = copy(h.buf[h.n..], b);
                h.n = len(h.buf);
                b = b[k..];
                h.flush();
            }

            h.n += copy(h.buf[h.n..], b);
            return (size, error.As(null!)!);

        }

        // WriteString adds the bytes of s to the sequence of bytes hashed by h.
        // It always writes all of s and never fails; the count and error result are for implementing io.StringWriter.
        private static (long, error) WriteString(this ptr<Hash> _addr_h, @string s)
        {
            long _p0 = default;
            error _p0 = default!;
            ref Hash h = ref _addr_h.val;

            var size = len(s);
            while (h.n + len(s) > len(h.buf))
            {
                var k = copy(h.buf[h.n..], s);
                h.n = len(h.buf);
                s = s[k..];
                h.flush();
            }

            h.n += copy(h.buf[h.n..], s);
            return (size, error.As(null!)!);

        }

        // Seed returns h's seed value.
        private static Seed Seed(this ptr<Hash> _addr_h)
        {
            ref Hash h = ref _addr_h.val;

            h.initSeed();
            return h.seed;
        }

        // SetSeed sets h to use seed, which must have been returned by MakeSeed
        // or by another Hash's Seed method.
        // Two Hash objects with the same seed behave identically.
        // Two Hash objects with different seeds will very likely behave differently.
        // Any bytes added to h before this call will be discarded.
        private static void SetSeed(this ptr<Hash> _addr_h, Seed seed)
        {
            ref Hash h = ref _addr_h.val;

            h.setSeed(seed);
            h.n = 0L;
        }

        // setSeed sets seed without discarding accumulated data.
        private static void setSeed(this ptr<Hash> _addr_h, Seed seed) => func((_, panic, __) =>
        {
            ref Hash h = ref _addr_h.val;

            if (seed.s == 0L)
            {
                panic("maphash: use of uninitialized Seed");
            }

            h.seed = seed;
            h.state = seed;

        });

        // Reset discards all bytes added to h.
        // (The seed remains the same.)
        private static void Reset(this ptr<Hash> _addr_h)
        {
            ref Hash h = ref _addr_h.val;

            h.initSeed();
            h.state = h.seed;
            h.n = 0L;
        }

        // precondition: buffer is full.
        private static void flush(this ptr<Hash> _addr_h) => func((_, panic, __) =>
        {
            ref Hash h = ref _addr_h.val;

            if (h.n != len(h.buf))
            {
                panic("maphash: flush of partially full buffer");
            }

            h.initSeed();
            h.state.s = rthash(h.buf[..], h.state.s);
            h.n = 0L;

        });

        // Sum64 returns h's current 64-bit value, which depends on
        // h's seed and the sequence of bytes added to h since the
        // last call to Reset or SetSeed.
        //
        // All bits of the Sum64 result are close to uniformly and
        // independently distributed, so it can be safely reduced
        // by using bit masking, shifting, or modular arithmetic.
        private static ulong Sum64(this ptr<Hash> _addr_h)
        {
            ref Hash h = ref _addr_h.val;

            h.initSeed();
            return rthash(h.buf[..h.n], h.state.s);
        }

        // MakeSeed returns a new random seed.
        public static Seed MakeSeed()
        {
            ulong s1 = default;            ulong s2 = default;

            while (true)
            {
                s1 = uint64(runtime_fastrand());
                s2 = uint64(runtime_fastrand()); 
                // We use seed 0 to indicate an uninitialized seed/hash,
                // so keep trying until we get a non-zero seed.
                if (s1 | s2 != 0L)
                {
                    break;
                }

            }

            return new Seed(s:s1<<32+s2);

        }

        //go:linkname runtime_fastrand runtime.fastrand
        private static uint runtime_fastrand()
;

        private static ulong rthash(slice<byte> b, ulong seed)
        {
            if (len(b) == 0L)
            {>>MARKER:FUNCTION_runtime_fastrand_BLOCK_PREFIX<<
                return seed;
            } 
            // The runtime hasher only works on uintptr. For 64-bit
            // architectures, we use the hasher directly. Otherwise,
            // we use two parallel hashers on the lower and upper 32 bits.
            if (@unsafe.Sizeof(uintptr(0L)) == 8L)
            {
                return uint64(runtime_memhash(@unsafe.Pointer(_addr_b[0L]), uintptr(seed), uintptr(len(b))));
            }

            var lo = runtime_memhash(@unsafe.Pointer(_addr_b[0L]), uintptr(seed), uintptr(len(b)));
            var hi = runtime_memhash(@unsafe.Pointer(_addr_b[0L]), uintptr(seed >> (int)(32L)), uintptr(len(b)));
            return uint64(hi) << (int)(32L) | uint64(lo);

        }

        //go:linkname runtime_memhash runtime.memhash
        //go:noescape
        private static System.UIntPtr runtime_memhash(unsafe.Pointer p, System.UIntPtr seed, System.UIntPtr s)
;

        // Sum appends the hash's current 64-bit value to b.
        // It exists for implementing hash.Hash.
        // For direct calls, it is more efficient to use Sum64.
        private static slice<byte> Sum(this ptr<Hash> _addr_h, slice<byte> b)
        {
            ref Hash h = ref _addr_h.val;

            var x = h.Sum64();
            return append(b, byte(x >> (int)(0L)), byte(x >> (int)(8L)), byte(x >> (int)(16L)), byte(x >> (int)(24L)), byte(x >> (int)(32L)), byte(x >> (int)(40L)), byte(x >> (int)(48L)), byte(x >> (int)(56L)));
        }

        // Size returns h's hash value size, 8 bytes.
        private static long Size(this ptr<Hash> _addr_h)
        {
            ref Hash h = ref _addr_h.val;

            return 8L;
        }

        // BlockSize returns h's block size.
        private static long BlockSize(this ptr<Hash> _addr_h)
        {
            ref Hash h = ref _addr_h.val;

            return len(h.buf);
        }
    }
}}
