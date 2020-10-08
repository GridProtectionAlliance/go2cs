// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package chacha20 implements the ChaCha20 and XChaCha20 encryption algorithms
// as specified in RFC 8439 and draft-irtf-cfrg-xchacha-01.
// package chacha20 -- go2cs converted at 2020 October 08 04:59:53 UTC
// import "vendor/golang.org/x/crypto/chacha20" ==> using chacha20 = go.vendor.golang.org.x.crypto.chacha20_package
// Original source: C:\Go\src\vendor\golang.org\x\crypto\chacha20\chacha_generic.go
using cipher = go.crypto.cipher_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using bits = go.math.bits_package;

using subtle = go.golang.org.x.crypto.@internal.subtle_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto
{
    public static partial class chacha20_package
    {
 
        // KeySize is the size of the key used by this cipher, in bytes.
        public static readonly long KeySize = (long)32L; 

        // NonceSize is the size of the nonce used with the standard variant of this
        // cipher, in bytes.
        //
        // Note that this is too short to be safely generated at random if the same
        // key is reused more than 2³² times.
        public static readonly long NonceSize = (long)12L; 

        // NonceSizeX is the size of the nonce used with the XChaCha20 variant of
        // this cipher, in bytes.
        public static readonly long NonceSizeX = (long)24L;


        // Cipher is a stateful instance of ChaCha20 or XChaCha20 using a particular key
        // and nonce. A *Cipher implements the cipher.Stream interface.
        public partial struct Cipher
        {
            public array<uint> key;
            public uint counter;
            public array<uint> nonce; // The last len bytes of buf are leftover key stream bytes from the previous
// XORKeyStream invocation. The size of buf depends on how many blocks are
// computed at a time by xorKeyStreamBlocks.
            public array<byte> buf;
            public long len; // overflow is set when the counter overflowed, no more blocks can be
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
        public static (ptr<Cipher>, error) NewUnauthenticatedCipher(slice<byte> key, slice<byte> nonce)
        {
            ptr<Cipher> _p0 = default!;
            error _p0 = default!;
 
            // This function is split into a wrapper so that the Cipher allocation will
            // be inlined, and depending on how the caller uses the return value, won't
            // escape to the heap.
            ptr<Cipher> c = addr(new Cipher());
            return _addr_newUnauthenticatedCipher(_addr_c, key, nonce)!;

        }

        private static (ptr<Cipher>, error) newUnauthenticatedCipher(ptr<Cipher> _addr_c, slice<byte> key, slice<byte> nonce)
        {
            ptr<Cipher> _p0 = default!;
            error _p0 = default!;
            ref Cipher c = ref _addr_c.val;

            if (len(key) != KeySize)
            {
                return (_addr_null!, error.As(errors.New("chacha20: wrong key size"))!);
            }

            if (len(nonce) == NonceSizeX)
            { 
                // XChaCha20 uses the ChaCha20 core to mix 16 bytes of the nonce into a
                // derived key, allowing it to operate on a nonce of 24 bytes. See
                // draft-irtf-cfrg-xchacha-01, Section 2.3.
                key, _ = HChaCha20(key, nonce[0L..16L]);
                var cNonce = make_slice<byte>(NonceSize);
                copy(cNonce[4L..12L], nonce[16L..24L]);
                nonce = cNonce;

            }
            else if (len(nonce) != NonceSize)
            {
                return (_addr_null!, error.As(errors.New("chacha20: wrong nonce size"))!);
            }

            key = key[..KeySize];
            nonce = nonce[..NonceSize]; // bounds check elimination hint
            c.key = new array<uint>(new uint[] { binary.LittleEndian.Uint32(key[0:4]), binary.LittleEndian.Uint32(key[4:8]), binary.LittleEndian.Uint32(key[8:12]), binary.LittleEndian.Uint32(key[12:16]), binary.LittleEndian.Uint32(key[16:20]), binary.LittleEndian.Uint32(key[20:24]), binary.LittleEndian.Uint32(key[24:28]), binary.LittleEndian.Uint32(key[28:32]) });
            c.nonce = new array<uint>(new uint[] { binary.LittleEndian.Uint32(nonce[0:4]), binary.LittleEndian.Uint32(nonce[4:8]), binary.LittleEndian.Uint32(nonce[8:12]) });
            return (_addr_c!, error.As(null!)!);

        }

        // The constant first 4 words of the ChaCha20 state.
        private static readonly uint j0 = (uint)0x61707865UL; // expa
        private static readonly uint j1 = (uint)0x3320646eUL; // nd 3
        private static readonly uint j2 = (uint)0x79622d32UL; // 2-by
        private static readonly uint j3 = (uint)0x6b206574UL; // te k

        private static readonly long blockSize = (long)64L;

        // quarterRound is the core of ChaCha20. It shuffles the bits of 4 state words.
        // It's executed 4 times for each of the 20 ChaCha20 rounds, operating on all 16
        // words each round, in columnar or diagonal groups of 4 at a time.


        // quarterRound is the core of ChaCha20. It shuffles the bits of 4 state words.
        // It's executed 4 times for each of the 20 ChaCha20 rounds, operating on all 16
        // words each round, in columnar or diagonal groups of 4 at a time.
        private static (uint, uint, uint, uint) quarterRound(uint a, uint b, uint c, uint d)
        {
            uint _p0 = default;
            uint _p0 = default;
            uint _p0 = default;
            uint _p0 = default;

            a += b;
            d ^= a;
            d = bits.RotateLeft32(d, 16L);
            c += d;
            b ^= c;
            b = bits.RotateLeft32(b, 12L);
            a += b;
            d ^= a;
            d = bits.RotateLeft32(d, 8L);
            c += d;
            b ^= c;
            b = bits.RotateLeft32(b, 7L);
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
        private static void SetCounter(this ptr<Cipher> _addr_s, uint counter) => func((_, panic, __) =>
        {
            ref Cipher s = ref _addr_s.val;
 
            // Internally, s may buffer multiple blocks, which complicates this
            // implementation slightly. When checking whether the counter has rolled
            // back, we must use both s.counter and s.len to determine how many blocks
            // we have already output.
            var outputCounter = s.counter - uint32(s.len) / blockSize;
            if (s.overflow || counter < outputCounter)
            {
                panic("chacha20: SetCounter attempted to rollback counter");
            } 

            // In the general case, we set the new counter value and reset s.len to 0,
            // causing the next call to XORKeyStream to refill the buffer. However, if
            // we're advancing within the existing buffer, we can save work by simply
            // setting s.len.
            if (counter < s.counter)
            {
                s.len = int(s.counter - counter) * blockSize;
            }
            else
            {
                s.counter = counter;
                s.len = 0L;
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
        private static void XORKeyStream(this ptr<Cipher> _addr_s, slice<byte> dst, slice<byte> src) => func((_, panic, __) =>
        {
            ref Cipher s = ref _addr_s.val;

            if (len(src) == 0L)
            {
                return ;
            }

            if (len(dst) < len(src))
            {
                panic("chacha20: output smaller than input");
            }

            dst = dst[..len(src)];
            if (subtle.InexactOverlap(dst, src))
            {
                panic("chacha20: invalid buffer overlap");
            } 

            // First, drain any remaining key stream from a previous XORKeyStream.
            if (s.len != 0L)
            {
                var keyStream = s.buf[bufSize - s.len..];
                if (len(src) < len(keyStream))
                {
                    keyStream = keyStream[..len(src)];
                }

                _ = src[len(keyStream) - 1L]; // bounds check elimination hint
                foreach (var (i, b) in keyStream)
                {
                    dst[i] = src[i] ^ b;
                }
                s.len -= len(keyStream);
                dst = dst[len(keyStream)..];
                src = src[len(keyStream)..];

            }

            if (len(src) == 0L)
            {
                return ;
            } 

            // If we'd need to let the counter overflow and keep generating output,
            // panic immediately. If instead we'd only reach the last block, remember
            // not to generate any more output after the buffer is drained.
            var numBlocks = (uint64(len(src)) + blockSize - 1L) / blockSize;
            if (s.overflow || uint64(s.counter) + numBlocks > 1L << (int)(32L))
            {
                panic("chacha20: counter overflow");
            }
            else if (uint64(s.counter) + numBlocks == 1L << (int)(32L))
            {
                s.overflow = true;
            } 

            // xorKeyStreamBlocks implementations expect input lengths that are a
            // multiple of bufSize. Platform-specific ones process multiple blocks at a
            // time, so have bufSizes that are a multiple of blockSize.
            var full = len(src) - len(src) % bufSize;
            if (full > 0L)
            {
                s.xorKeyStreamBlocks(dst[..full], src[..full]);
            }

            dst = dst[full..];
            src = src[full..]; 

            // If using a multi-block xorKeyStreamBlocks would overflow, use the generic
            // one that does one block at a time.
            const var blocksPerBuf = (var)bufSize / blockSize;

            if (uint64(s.counter) + blocksPerBuf > 1L << (int)(32L))
            {
                s.buf = new array<byte>(new byte[] {  });
                numBlocks = (len(src) + blockSize - 1L) / blockSize;
                var buf = s.buf[bufSize - numBlocks * blockSize..];
                copy(buf, src);
                s.xorKeyStreamBlocksGeneric(buf, buf);
                s.len = len(buf) - copy(dst, buf);
                return ;
            } 

            // If we have a partial (multi-)block, pad it for xorKeyStreamBlocks, and
            // keep the leftover keystream for the next XORKeyStream invocation.
            if (len(src) > 0L)
            {
                s.buf = new array<byte>(new byte[] {  });
                copy(s.buf[..], src);
                s.xorKeyStreamBlocks(s.buf[..], s.buf[..]);
                s.len = bufSize - copy(dst, s.buf[..]);
            }

        });

        private static void xorKeyStreamBlocksGeneric(this ptr<Cipher> _addr_s, slice<byte> dst, slice<byte> src) => func((_, panic, __) =>
        {
            ref Cipher s = ref _addr_s.val;

            if (len(dst) != len(src) || len(dst) % blockSize != 0L)
            {
                panic("chacha20: internal error: wrong dst and/or src length");
            } 

            // To generate each block of key stream, the initial cipher state
            // (represented below) is passed through 20 rounds of shuffling,
            // alternatively applying quarterRounds by columns (like 1, 5, 9, 13)
            // or by diagonals (like 1, 6, 11, 12).
            //
            //      0:cccccccc   1:cccccccc   2:cccccccc   3:cccccccc
            //      4:kkkkkkkk   5:kkkkkkkk   6:kkkkkkkk   7:kkkkkkkk
            //      8:kkkkkkkk   9:kkkkkkkk  10:kkkkkkkk  11:kkkkkkkk
            //     12:bbbbbbbb  13:nnnnnnnn  14:nnnnnnnn  15:nnnnnnnn
            //
            //            c=constant k=key b=blockcount n=nonce
            var c0 = j0;            var c1 = j1;            var c2 = j2;            var c3 = j3;
            var c4 = s.key[0L];            var c5 = s.key[1L];            var c6 = s.key[2L];            var c7 = s.key[3L];
            var c8 = s.key[4L];            var c9 = s.key[5L];            var c10 = s.key[6L];            var c11 = s.key[7L];
            var _ = s.counter;            var c13 = s.nonce[0L];            var c14 = s.nonce[1L];            var c15 = s.nonce[2L];
 

            // Three quarters of the first round don't depend on the counter, so we can
            // calculate them here, and reuse them for multiple blocks in the loop, and
            // for future XORKeyStream invocations.
            if (!s.precompDone)
            {
                s.p1, s.p5, s.p9, s.p13 = quarterRound(c1, c5, c9, c13);
                s.p2, s.p6, s.p10, s.p14 = quarterRound(c2, c6, c10, c14);
                s.p3, s.p7, s.p11, s.p15 = quarterRound(c3, c7, c11, c15);
                s.precompDone = true;
            } 

            // A condition of len(src) > 0 would be sufficient, but this also
            // acts as a bounds check elimination hint.
            while (len(src) >= 64L && len(dst) >= 64L)
            { 
                // The remainder of the first column round.
                var (fcr0, fcr4, fcr8, fcr12) = quarterRound(c0, c4, c8, s.counter); 

                // The second diagonal round.
                var (x0, x5, x10, x15) = quarterRound(fcr0, s.p5, s.p10, s.p15);
                var (x1, x6, x11, x12) = quarterRound(s.p1, s.p6, s.p11, fcr12);
                var (x2, x7, x8, x13) = quarterRound(s.p2, s.p7, fcr8, s.p13);
                var (x3, x4, x9, x14) = quarterRound(s.p3, fcr4, s.p9, s.p14); 

                // The remaining 18 rounds.
                for (long i = 0L; i < 9L; i++)
                { 
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
 

                // Add back the initial state to generate the key stream, then
                // XOR the key stream with the source and write out the result.
                addXor(dst[0L..4L], src[0L..4L], x0, c0);
                addXor(dst[4L..8L], src[4L..8L], x1, c1);
                addXor(dst[8L..12L], src[8L..12L], x2, c2);
                addXor(dst[12L..16L], src[12L..16L], x3, c3);
                addXor(dst[16L..20L], src[16L..20L], x4, c4);
                addXor(dst[20L..24L], src[20L..24L], x5, c5);
                addXor(dst[24L..28L], src[24L..28L], x6, c6);
                addXor(dst[28L..32L], src[28L..32L], x7, c7);
                addXor(dst[32L..36L], src[32L..36L], x8, c8);
                addXor(dst[36L..40L], src[36L..40L], x9, c9);
                addXor(dst[40L..44L], src[40L..44L], x10, c10);
                addXor(dst[44L..48L], src[44L..48L], x11, c11);
                addXor(dst[48L..52L], src[48L..52L], x12, s.counter);
                addXor(dst[52L..56L], src[52L..56L], x13, c13);
                addXor(dst[56L..60L], src[56L..60L], x14, c14);
                addXor(dst[60L..64L], src[60L..64L], x15, c15);

                s.counter += 1L;

                src = src[blockSize..];
                dst = dst[blockSize..];

            }


        });

        // HChaCha20 uses the ChaCha20 core to generate a derived key from a 32 bytes
        // key and a 16 bytes nonce. It returns an error if key or nonce have any other
        // length. It is used as part of the XChaCha20 construction.
        public static (slice<byte>, error) HChaCha20(slice<byte> key, slice<byte> nonce)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
 
            // This function is split into a wrapper so that the slice allocation will
            // be inlined, and depending on how the caller uses the return value, won't
            // escape to the heap.
            var @out = make_slice<byte>(32L);
            return hChaCha20(out, key, nonce);

        }

        private static (slice<byte>, error) hChaCha20(slice<byte> @out, slice<byte> key, slice<byte> nonce)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            if (len(key) != KeySize)
            {
                return (null, error.As(errors.New("chacha20: wrong HChaCha20 key size"))!);
            }

            if (len(nonce) != 16L)
            {
                return (null, error.As(errors.New("chacha20: wrong HChaCha20 nonce size"))!);
            }

            var x0 = j0;
            var x1 = j1;
            var x2 = j2;
            var x3 = j3;
            var x4 = binary.LittleEndian.Uint32(key[0L..4L]);
            var x5 = binary.LittleEndian.Uint32(key[4L..8L]);
            var x6 = binary.LittleEndian.Uint32(key[8L..12L]);
            var x7 = binary.LittleEndian.Uint32(key[12L..16L]);
            var x8 = binary.LittleEndian.Uint32(key[16L..20L]);
            var x9 = binary.LittleEndian.Uint32(key[20L..24L]);
            var x10 = binary.LittleEndian.Uint32(key[24L..28L]);
            var x11 = binary.LittleEndian.Uint32(key[28L..32L]);
            var x12 = binary.LittleEndian.Uint32(nonce[0L..4L]);
            var x13 = binary.LittleEndian.Uint32(nonce[4L..8L]);
            var x14 = binary.LittleEndian.Uint32(nonce[8L..12L]);
            var x15 = binary.LittleEndian.Uint32(nonce[12L..16L]);

            for (long i = 0L; i < 10L; i++)
            { 
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


            _ = out[31L]; // bounds check elimination hint
            binary.LittleEndian.PutUint32(out[0L..4L], x0);
            binary.LittleEndian.PutUint32(out[4L..8L], x1);
            binary.LittleEndian.PutUint32(out[8L..12L], x2);
            binary.LittleEndian.PutUint32(out[12L..16L], x3);
            binary.LittleEndian.PutUint32(out[16L..20L], x12);
            binary.LittleEndian.PutUint32(out[20L..24L], x13);
            binary.LittleEndian.PutUint32(out[24L..28L], x14);
            binary.LittleEndian.PutUint32(out[28L..32L], x15);
            return (out, error.As(null!)!);

        }
    }
}}}}}
