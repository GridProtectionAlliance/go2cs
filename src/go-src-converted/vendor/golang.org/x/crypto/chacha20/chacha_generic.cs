// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package chacha20 implements the ChaCha20 and XChaCha20 encryption algorithms
// as specified in RFC 8439 and draft-irtf-cfrg-xchacha-01.
// package chacha20 -- go2cs converted at 2022 March 06 23:36:32 UTC
// import "vendor/golang.org/x/crypto/chacha20" ==> using chacha20 = go.vendor.golang.org.x.crypto.chacha20_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\crypto\chacha20\chacha_generic.go
using cipher = go.crypto.cipher_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using bits = go.math.bits_package;

using subtle = go.golang.org.x.crypto.@internal.subtle_package;

namespace go.vendor.golang.org.x.crypto;

public static partial class chacha20_package {

 
// KeySize is the size of the key used by this cipher, in bytes.
public static readonly nint KeySize = 32; 

// NonceSize is the size of the nonce used with the standard variant of this
// cipher, in bytes.
//
// Note that this is too short to be safely generated at random if the same
// key is reused more than 2³² times.
public static readonly nint NonceSize = 12; 

// NonceSizeX is the size of the nonce used with the XChaCha20 variant of
// this cipher, in bytes.
public static readonly nint NonceSizeX = 24;


// Cipher is a stateful instance of ChaCha20 or XChaCha20 using a particular key
// and nonce. A *Cipher implements the cipher.Stream interface.
public partial struct Cipher {
    public array<uint> key;
    public uint counter;
    public array<uint> nonce; // The last len bytes of buf are leftover key stream bytes from the previous
// XORKeyStream invocation. The size of buf depends on how many blocks are
// computed at a time by xorKeyStreamBlocks.
    public array<byte> buf;
    public nint len; // overflow is set when the counter overflowed, no more blocks can be
// generated, and the next XORKeyStream call should panic.
    public bool overflow; // The counter-independent results of the first round are cached after they
// are computed the first time.
    public bool precompDone;
    public uint p1;
    public uint p5;
    public uint p9;
    public uint p13;
    public uint p2;
    public uint p6;
    public uint p10;
    public uint p14;
    public uint p3;
    public uint p7;
    public uint p11;
    public uint p15;
}

private static cipher.Stream _ = (Cipher.val)(null);

// NewUnauthenticatedCipher creates a new ChaCha20 stream cipher with the given
// 32 bytes key and a 12 or 24 bytes nonce. If a nonce of 24 bytes is provided,
// the XChaCha20 construction will be used. It returns an error if key or nonce
// have any other length.
//
// Note that ChaCha20, like all stream ciphers, is not authenticated and allows
// attackers to silently tamper with the plaintext. For this reason, it is more
// appropriate as a building block than as a standalone encryption mechanism.
// Instead, consider using package golang.org/x/crypto/chacha20poly1305.
public static (ptr<Cipher>, error) NewUnauthenticatedCipher(slice<byte> key, slice<byte> nonce) {
    ptr<Cipher> _p0 = default!;
    error _p0 = default!;
 
    // This function is split into a wrapper so that the Cipher allocation will
    // be inlined, and depending on how the caller uses the return value, won't
    // escape to the heap.
    ptr<Cipher> c = addr(new Cipher());
    return _addr_newUnauthenticatedCipher(c, key, nonce)!;

}

private static (ptr<Cipher>, error) newUnauthenticatedCipher(ptr<Cipher> _addr_c, slice<byte> key, slice<byte> nonce) {
    ptr<Cipher> _p0 = default!;
    error _p0 = default!;
    ref Cipher c = ref _addr_c.val;

    if (len(key) != KeySize) {
        return (_addr_null!, error.As(errors.New("chacha20: wrong key size"))!);
    }
    if (len(nonce) == NonceSizeX) { 
        // XChaCha20 uses the ChaCha20 core to mix 16 bytes of the nonce into a
        // derived key, allowing it to operate on a nonce of 24 bytes. See
        // draft-irtf-cfrg-xchacha-01, Section 2.3.
        key, _ = HChaCha20(key, nonce[(int)0..(int)16]);
        var cNonce = make_slice<byte>(NonceSize);
        copy(cNonce[(int)4..(int)12], nonce[(int)16..(int)24]);
        nonce = cNonce;

    }
    else if (len(nonce) != NonceSize) {
        return (_addr_null!, error.As(errors.New("chacha20: wrong nonce size"))!);
    }
    (key, nonce) = (key[..(int)KeySize], nonce[..(int)NonceSize]);    c.key = new array<uint>(new uint[] { binary.LittleEndian.Uint32(key[0:4]), binary.LittleEndian.Uint32(key[4:8]), binary.LittleEndian.Uint32(key[8:12]), binary.LittleEndian.Uint32(key[12:16]), binary.LittleEndian.Uint32(key[16:20]), binary.LittleEndian.Uint32(key[20:24]), binary.LittleEndian.Uint32(key[24:28]), binary.LittleEndian.Uint32(key[28:32]) });
    c.nonce = new array<uint>(new uint[] { binary.LittleEndian.Uint32(nonce[0:4]), binary.LittleEndian.Uint32(nonce[4:8]), binary.LittleEndian.Uint32(nonce[8:12]) });
    return (_addr_c!, error.As(null!)!);

}

// The constant first 4 words of the ChaCha20 state.
private static readonly uint j0 = 0x61707865; // expa
private static readonly uint j1 = 0x3320646e; // nd 3
private static readonly uint j2 = 0x79622d32; // 2-by
private static readonly uint j3 = 0x6b206574; // te k

private static readonly nint blockSize = 64;

// quarterRound is the core of ChaCha20. It shuffles the bits of 4 state words.
// It's executed 4 times for each of the 20 ChaCha20 rounds, operating on all 16
// words each round, in columnar or diagonal groups of 4 at a time.


// quarterRound is the core of ChaCha20. It shuffles the bits of 4 state words.
// It's executed 4 times for each of the 20 ChaCha20 rounds, operating on all 16
// words each round, in columnar or diagonal groups of 4 at a time.
private static (uint, uint, uint, uint) quarterRound(uint a, uint b, uint c, uint d) {
    uint _p0 = default;
    uint _p0 = default;
    uint _p0 = default;
    uint _p0 = default;

    a += b;
    d ^= a;
    d = bits.RotateLeft32(d, 16);
    c += d;
    b ^= c;
    b = bits.RotateLeft32(b, 12);
    a += b;
    d ^= a;
    d = bits.RotateLeft32(d, 8);
    c += d;
    b ^= c;
    b = bits.RotateLeft32(b, 7);
    return (a, b, c, d);
}

// SetCounter sets the Cipher counter. The next invocation of XORKeyStream will
// behave as if (64 * counter) bytes had been encrypted so far.
//
// To prevent accidental counter reuse, SetCounter panics if counter is less
// than the current value.
//
// Note that the execution time of XORKeyStream is not independent of the
// counter value.
private static void SetCounter(this ptr<Cipher> _addr_s, uint counter) => func((_, panic, _) => {
    ref Cipher s = ref _addr_s.val;
 
    // Internally, s may buffer multiple blocks, which complicates this
    // implementation slightly. When checking whether the counter has rolled
    // back, we must use both s.counter and s.len to determine how many blocks
    // we have already output.
    var outputCounter = s.counter - uint32(s.len) / blockSize;
    if (s.overflow || counter < outputCounter) {
        panic("chacha20: SetCounter attempted to rollback counter");
    }
    if (counter < s.counter) {
        s.len = int(s.counter - counter) * blockSize;
    }
    else
 {
        s.counter = counter;
        s.len = 0;
    }
});

// XORKeyStream XORs each byte in the given slice with a byte from the
// cipher's key stream. Dst and src must overlap entirely or not at all.
//
// If len(dst) < len(src), XORKeyStream will panic. It is acceptable
// to pass a dst bigger than src, and in that case, XORKeyStream will
// only update dst[:len(src)] and will not touch the rest of dst.
//
// Multiple calls to XORKeyStream behave as if the concatenation of
// the src buffers was passed in a single run. That is, Cipher
// maintains state and does not reset at each XORKeyStream call.
private static void XORKeyStream(this ptr<Cipher> _addr_s, slice<byte> dst, slice<byte> src) => func((_, panic, _) => {
    ref Cipher s = ref _addr_s.val;

    if (len(src) == 0) {
        return ;
    }
    if (len(dst) < len(src)) {
        panic("chacha20: output smaller than input");
    }
    dst = dst[..(int)len(src)];
    if (subtle.InexactOverlap(dst, src)) {
        panic("chacha20: invalid buffer overlap");
    }
    if (s.len != 0) {
        var keyStream = s.buf[(int)bufSize - s.len..];
        if (len(src) < len(keyStream)) {
            keyStream = keyStream[..(int)len(src)];
        }
        _ = src[len(keyStream) - 1]; // bounds check elimination hint
        foreach (var (i, b) in keyStream) {
            dst[i] = src[i] ^ b;
        }        s.len -= len(keyStream);
        (dst, src) = (dst[(int)len(keyStream)..], src[(int)len(keyStream)..]);
    }
    if (len(src) == 0) {
        return ;
    }
    var numBlocks = (uint64(len(src)) + blockSize - 1) / blockSize;
    if (s.overflow || uint64(s.counter) + numBlocks > 1 << 32) {
        panic("chacha20: counter overflow");
    }
    else if (uint64(s.counter) + numBlocks == 1 << 32) {
        s.overflow = true;
    }
    var full = len(src) - len(src) % bufSize;
    if (full > 0) {
        s.xorKeyStreamBlocks(dst[..(int)full], src[..(int)full]);
    }
    (dst, src) = (dst[(int)full..], src[(int)full..]);    const var blocksPerBuf = bufSize / blockSize;

    if (uint64(s.counter) + blocksPerBuf > 1 << 32) {
        s.buf = new array<byte>(new byte[] {  });
        numBlocks = (len(src) + blockSize - 1) / blockSize;
        var buf = s.buf[(int)bufSize - numBlocks * blockSize..];
        copy(buf, src);
        s.xorKeyStreamBlocksGeneric(buf, buf);
        s.len = len(buf) - copy(dst, buf);
        return ;
    }
    if (len(src) > 0) {
        s.buf = new array<byte>(new byte[] {  });
        copy(s.buf[..], src);
        s.xorKeyStreamBlocks(s.buf[..], s.buf[..]);
        s.len = bufSize - copy(dst, s.buf[..]);
    }
});

private static void xorKeyStreamBlocksGeneric(this ptr<Cipher> _addr_s, slice<byte> dst, slice<byte> src) => func((_, panic, _) => {
    ref Cipher s = ref _addr_s.val;

    if (len(dst) != len(src) || len(dst) % blockSize != 0) {
        panic("chacha20: internal error: wrong dst and/or src length");
    }
    var c0 = j0;    var c1 = j1;    var c2 = j2;    var c3 = j3;
    var c4 = s.key[0];    var c5 = s.key[1];    var c6 = s.key[2];    var c7 = s.key[3];
    var c8 = s.key[4];    var c9 = s.key[5];    var c10 = s.key[6];    var c11 = s.key[7];
    var _ = s.counter;    var c13 = s.nonce[0];    var c14 = s.nonce[1];    var c15 = s.nonce[2];
 

    // Three quarters of the first round don't depend on the counter, so we can
    // calculate them here, and reuse them for multiple blocks in the loop, and
    // for future XORKeyStream invocations.
    if (!s.precompDone) {
        s.p1, s.p5, s.p9, s.p13 = quarterRound(c1, c5, c9, c13);
        s.p2, s.p6, s.p10, s.p14 = quarterRound(c2, c6, c10, c14);
        s.p3, s.p7, s.p11, s.p15 = quarterRound(c3, c7, c11, c15);
        s.precompDone = true;
    }
    while (len(src) >= 64 && len(dst) >= 64) { 
        // The remainder of the first column round.
        var (fcr0, fcr4, fcr8, fcr12) = quarterRound(c0, c4, c8, s.counter); 

        // The second diagonal round.
        var (x0, x5, x10, x15) = quarterRound(fcr0, s.p5, s.p10, s.p15);
        var (x1, x6, x11, x12) = quarterRound(s.p1, s.p6, s.p11, fcr12);
        var (x2, x7, x8, x13) = quarterRound(s.p2, s.p7, fcr8, s.p13);
        var (x3, x4, x9, x14) = quarterRound(s.p3, fcr4, s.p9, s.p14); 

        // The remaining 18 rounds.
        for (nint i = 0; i < 9; i++) { 
            // Column round.
            x0, x4, x8, x12 = quarterRound(x0, x4, x8, x12);
            x1, x5, x9, x13 = quarterRound(x1, x5, x9, x13);
            x2, x6, x10, x14 = quarterRound(x2, x6, x10, x14);
            x3, x7, x11, x15 = quarterRound(x3, x7, x11, x15); 

            // Diagonal round.
            x0, x5, x10, x15 = quarterRound(x0, x5, x10, x15);
            x1, x6, x11, x12 = quarterRound(x1, x6, x11, x12);
            x2, x7, x8, x13 = quarterRound(x2, x7, x8, x13);
            x3, x4, x9, x14 = quarterRound(x3, x4, x9, x14);

        } 

        // Add back the initial state to generate the key stream, then
        // XOR the key stream with the source and write out the result.
        addXor(dst[(int)0..(int)4], src[(int)0..(int)4], x0, c0);
        addXor(dst[(int)4..(int)8], src[(int)4..(int)8], x1, c1);
        addXor(dst[(int)8..(int)12], src[(int)8..(int)12], x2, c2);
        addXor(dst[(int)12..(int)16], src[(int)12..(int)16], x3, c3);
        addXor(dst[(int)16..(int)20], src[(int)16..(int)20], x4, c4);
        addXor(dst[(int)20..(int)24], src[(int)20..(int)24], x5, c5);
        addXor(dst[(int)24..(int)28], src[(int)24..(int)28], x6, c6);
        addXor(dst[(int)28..(int)32], src[(int)28..(int)32], x7, c7);
        addXor(dst[(int)32..(int)36], src[(int)32..(int)36], x8, c8);
        addXor(dst[(int)36..(int)40], src[(int)36..(int)40], x9, c9);
        addXor(dst[(int)40..(int)44], src[(int)40..(int)44], x10, c10);
        addXor(dst[(int)44..(int)48], src[(int)44..(int)48], x11, c11);
        addXor(dst[(int)48..(int)52], src[(int)48..(int)52], x12, s.counter);
        addXor(dst[(int)52..(int)56], src[(int)52..(int)56], x13, c13);
        addXor(dst[(int)56..(int)60], src[(int)56..(int)60], x14, c14);
        addXor(dst[(int)60..(int)64], src[(int)60..(int)64], x15, c15);

        s.counter += 1;

        (src, dst) = (src[(int)blockSize..], dst[(int)blockSize..]);
    }

});

// HChaCha20 uses the ChaCha20 core to generate a derived key from a 32 bytes
// key and a 16 bytes nonce. It returns an error if key or nonce have any other
// length. It is used as part of the XChaCha20 construction.
public static (slice<byte>, error) HChaCha20(slice<byte> key, slice<byte> nonce) {
    slice<byte> _p0 = default;
    error _p0 = default!;
 
    // This function is split into a wrapper so that the slice allocation will
    // be inlined, and depending on how the caller uses the return value, won't
    // escape to the heap.
    var @out = make_slice<byte>(32);
    return hChaCha20(out, key, nonce);

}

private static (slice<byte>, error) hChaCha20(slice<byte> @out, slice<byte> key, slice<byte> nonce) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    if (len(key) != KeySize) {
        return (null, error.As(errors.New("chacha20: wrong HChaCha20 key size"))!);
    }
    if (len(nonce) != 16) {
        return (null, error.As(errors.New("chacha20: wrong HChaCha20 nonce size"))!);
    }
    var x0 = j0;
    var x1 = j1;
    var x2 = j2;
    var x3 = j3;
    var x4 = binary.LittleEndian.Uint32(key[(int)0..(int)4]);
    var x5 = binary.LittleEndian.Uint32(key[(int)4..(int)8]);
    var x6 = binary.LittleEndian.Uint32(key[(int)8..(int)12]);
    var x7 = binary.LittleEndian.Uint32(key[(int)12..(int)16]);
    var x8 = binary.LittleEndian.Uint32(key[(int)16..(int)20]);
    var x9 = binary.LittleEndian.Uint32(key[(int)20..(int)24]);
    var x10 = binary.LittleEndian.Uint32(key[(int)24..(int)28]);
    var x11 = binary.LittleEndian.Uint32(key[(int)28..(int)32]);
    var x12 = binary.LittleEndian.Uint32(nonce[(int)0..(int)4]);
    var x13 = binary.LittleEndian.Uint32(nonce[(int)4..(int)8]);
    var x14 = binary.LittleEndian.Uint32(nonce[(int)8..(int)12]);
    var x15 = binary.LittleEndian.Uint32(nonce[(int)12..(int)16]);

    for (nint i = 0; i < 10; i++) { 
        // Diagonal round.
        x0, x4, x8, x12 = quarterRound(x0, x4, x8, x12);
        x1, x5, x9, x13 = quarterRound(x1, x5, x9, x13);
        x2, x6, x10, x14 = quarterRound(x2, x6, x10, x14);
        x3, x7, x11, x15 = quarterRound(x3, x7, x11, x15); 

        // Column round.
        x0, x5, x10, x15 = quarterRound(x0, x5, x10, x15);
        x1, x6, x11, x12 = quarterRound(x1, x6, x11, x12);
        x2, x7, x8, x13 = quarterRound(x2, x7, x8, x13);
        x3, x4, x9, x14 = quarterRound(x3, x4, x9, x14);

    }

    _ = out[31]; // bounds check elimination hint
    binary.LittleEndian.PutUint32(out[(int)0..(int)4], x0);
    binary.LittleEndian.PutUint32(out[(int)4..(int)8], x1);
    binary.LittleEndian.PutUint32(out[(int)8..(int)12], x2);
    binary.LittleEndian.PutUint32(out[(int)12..(int)16], x3);
    binary.LittleEndian.PutUint32(out[(int)16..(int)20], x12);
    binary.LittleEndian.PutUint32(out[(int)20..(int)24], x13);
    binary.LittleEndian.PutUint32(out[(int)24..(int)28], x14);
    binary.LittleEndian.PutUint32(out[(int)28..(int)32], x15);
    return (out, error.As(null!)!);

}

} // end chacha20_package
