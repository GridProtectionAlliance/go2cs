// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package maphash provides hash functions on byte sequences.
// These hash functions are intended to be used to implement hash tables or
// other data structures that need to map arbitrary strings or byte
// sequences to a uniform distribution on unsigned 64-bit integers.
// Each different instance of a hash table or data structure should use its own Seed.
//
// The hash functions are not cryptographically secure.
// (See crypto/sha256 and crypto/sha512 for cryptographic use.)
//
// package maphash -- go2cs converted at 2022 March 06 22:14:59 UTC
// import "hash/maphash" ==> using maphash = go.hash.maphash_package
// Original source: C:\Program Files\Go\src\hash\maphash\maphash.go
using unsafeheader = go.@internal.unsafeheader_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go.hash;

public static partial class maphash_package {

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
public partial struct Seed {
    public ulong s;
}

// A Hash computes a seeded hash of a byte sequence.
//
// The zero Hash is a valid Hash ready to use.
// A zero Hash chooses a random seed for itself during
// the first call to a Reset, Write, Seed, or Sum64 method.
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
public partial struct Hash {
    public array<Action> _; // not comparable
    public Seed seed; // initial seed used for this hash
    public Seed state; // current hash of all flushed bytes
    public array<byte> buf; // unflushed byte buffer
    public nint n; // number of unflushed bytes
}

// bufSize is the size of the Hash write buffer.
// The buffer ensures that writes depend only on the sequence of bytes,
// not the sequence of WriteByte/Write/WriteString calls,
// by always calling rthash with a full buffer (except for the tail).
private static readonly nint bufSize = 128;

// initSeed seeds the hash if necessary.
// initSeed is called lazily before any operation that actually uses h.seed/h.state.
// Note that this does not include Write/WriteByte/WriteString in the case
// where they only add to h.buf. (If they write too much, they call h.flush,
// which does call h.initSeed.)


// initSeed seeds the hash if necessary.
// initSeed is called lazily before any operation that actually uses h.seed/h.state.
// Note that this does not include Write/WriteByte/WriteString in the case
// where they only add to h.buf. (If they write too much, they call h.flush,
// which does call h.initSeed.)
private static void initSeed(this ptr<Hash> _addr_h) {
    ref Hash h = ref _addr_h.val;

    if (h.seed.s == 0) {
        var seed = MakeSeed();
        h.seed = seed;
        h.state = seed;
    }
}

// WriteByte adds b to the sequence of bytes hashed by h.
// It never fails; the error result is for implementing io.ByteWriter.
private static error WriteByte(this ptr<Hash> _addr_h, byte b) {
    ref Hash h = ref _addr_h.val;

    if (h.n == len(h.buf)) {
        h.flush();
    }
    h.buf[h.n] = b;
    h.n++;
    return error.As(null!)!;

}

// Write adds b to the sequence of bytes hashed by h.
// It always writes all of b and never fails; the count and error result are for implementing io.Writer.
private static (nint, error) Write(this ptr<Hash> _addr_h, slice<byte> b) {
    nint _p0 = default;
    error _p0 = default!;
    ref Hash h = ref _addr_h.val;

    var size = len(b); 
    // Deal with bytes left over in h.buf.
    // h.n <= bufSize is always true.
    // Checking it is ~free and it lets the compiler eliminate a bounds check.
    if (h.n > 0 && h.n <= bufSize) {
        var k = copy(h.buf[(int)h.n..], b);
        h.n += k;
        if (h.n < bufSize) { 
            // Copied the entirety of b to h.buf.
            return (size, error.As(null!)!);

        }
        b = b[(int)k..];
        h.flush(); 
        // No need to set h.n = 0 here; it happens just before exit.
    }
    if (len(b) > bufSize) {
        h.initSeed();
        while (len(b) > bufSize) {
            h.state.s = rthash(_addr_b[0], bufSize, h.state.s);
            b = b[(int)bufSize..];
        }
    }
    copy(h.buf[..], b);
    h.n = len(b);
    return (size, error.As(null!)!);

}

// WriteString adds the bytes of s to the sequence of bytes hashed by h.
// It always writes all of s and never fails; the count and error result are for implementing io.StringWriter.
private static (nint, error) WriteString(this ptr<Hash> _addr_h, @string s) {
    nint _p0 = default;
    error _p0 = default!;
    ref Hash h = ref _addr_h.val;
 
    // WriteString mirrors Write. See Write for comments.
    var size = len(s);
    if (h.n > 0 && h.n <= bufSize) {
        var k = copy(h.buf[(int)h.n..], s);
        h.n += k;
        if (h.n < bufSize) {
            return (size, error.As(null!)!);
        }
        s = s[(int)k..];
        h.flush();

    }
    if (len(s) > bufSize) {
        h.initSeed();
        while (len(s) > bufSize) {
            var ptr = (byte.val)((unsafeheader.String.val)(@unsafe.Pointer(_addr_s)).Data);
            h.state.s = rthash(_addr_ptr, bufSize, h.state.s);
            s = s[(int)bufSize..];
        }
    }
    copy(h.buf[..], s);
    h.n = len(s);
    return (size, error.As(null!)!);

}

// Seed returns h's seed value.
private static Seed Seed(this ptr<Hash> _addr_h) {
    ref Hash h = ref _addr_h.val;

    h.initSeed();
    return h.seed;
}

// SetSeed sets h to use seed, which must have been returned by MakeSeed
// or by another Hash's Seed method.
// Two Hash objects with the same seed behave identically.
// Two Hash objects with different seeds will very likely behave differently.
// Any bytes added to h before this call will be discarded.
private static void SetSeed(this ptr<Hash> _addr_h, Seed seed) => func((_, panic, _) => {
    ref Hash h = ref _addr_h.val;

    if (seed.s == 0) {
        panic("maphash: use of uninitialized Seed");
    }
    h.seed = seed;
    h.state = seed;
    h.n = 0;

});

// Reset discards all bytes added to h.
// (The seed remains the same.)
private static void Reset(this ptr<Hash> _addr_h) {
    ref Hash h = ref _addr_h.val;

    h.initSeed();
    h.state = h.seed;
    h.n = 0;
}

// precondition: buffer is full.
private static void flush(this ptr<Hash> _addr_h) => func((_, panic, _) => {
    ref Hash h = ref _addr_h.val;

    if (h.n != len(h.buf)) {
        panic("maphash: flush of partially full buffer");
    }
    h.initSeed();
    h.state.s = rthash(_addr_h.buf[0], h.n, h.state.s);
    h.n = 0;

});

// Sum64 returns h's current 64-bit value, which depends on
// h's seed and the sequence of bytes added to h since the
// last call to Reset or SetSeed.
//
// All bits of the Sum64 result are close to uniformly and
// independently distributed, so it can be safely reduced
// by using bit masking, shifting, or modular arithmetic.
private static ulong Sum64(this ptr<Hash> _addr_h) {
    ref Hash h = ref _addr_h.val;

    h.initSeed();
    return rthash(_addr_h.buf[0], h.n, h.state.s);
}

// MakeSeed returns a new random seed.
public static Seed MakeSeed() {
    ulong s1 = default;    ulong s2 = default;

    while (true) {
        s1 = uint64(runtime_fastrand());
        s2 = uint64(runtime_fastrand()); 
        // We use seed 0 to indicate an uninitialized seed/hash,
        // so keep trying until we get a non-zero seed.
        if (s1 | s2 != 0) {
            break;
        }
    }
    return new Seed(s:s1<<32+s2);

}

//go:linkname runtime_fastrand runtime.fastrand
private static uint runtime_fastrand();

private static ulong rthash(ptr<byte> _addr_ptr, nint len, ulong seed) {
    ref byte ptr = ref _addr_ptr.val;

    if (len == 0) {>>MARKER:FUNCTION_runtime_fastrand_BLOCK_PREFIX<<
        return seed;
    }
    if (@unsafe.Sizeof(uintptr(0)) == 8) {
        return uint64(runtime_memhash(@unsafe.Pointer(ptr), uintptr(seed), uintptr(len)));
    }
    var lo = runtime_memhash(@unsafe.Pointer(ptr), uintptr(seed), uintptr(len));
    var hi = runtime_memhash(@unsafe.Pointer(ptr), uintptr(seed >> 32), uintptr(len));
    return uint64(hi) << 32 | uint64(lo);

}

//go:linkname runtime_memhash runtime.memhash
//go:noescape
private static System.UIntPtr runtime_memhash(unsafe.Pointer p, System.UIntPtr seed, System.UIntPtr s);

// Sum appends the hash's current 64-bit value to b.
// It exists for implementing hash.Hash.
// For direct calls, it is more efficient to use Sum64.
private static slice<byte> Sum(this ptr<Hash> _addr_h, slice<byte> b) {
    ref Hash h = ref _addr_h.val;

    var x = h.Sum64();
    return append(b, byte(x >> 0), byte(x >> 8), byte(x >> 16), byte(x >> 24), byte(x >> 32), byte(x >> 40), byte(x >> 48), byte(x >> 56));
}

// Size returns h's hash value size, 8 bytes.
private static nint Size(this ptr<Hash> _addr_h) {
    ref Hash h = ref _addr_h.val;

    return 8;
}

// BlockSize returns h's block size.
private static nint BlockSize(this ptr<Hash> _addr_h) {
    ref Hash h = ref _addr_h.val;

    return len(h.buf);
}

} // end maphash_package
