// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ChaCha20 implements the core ChaCha20 function as specified in https://tools.ietf.org/html/rfc7539#section-2.3.
// package chacha20 -- go2cs converted at 2020 August 29 10:11:15 UTC
// import "vendor/golang_org/x/crypto/chacha20poly1305/internal/chacha20" ==> using chacha20 = go.vendor.golang_org.x.crypto.chacha20poly1305.@internal.chacha20_package
// Original source: C:\Go\src\vendor\golang_org\x\crypto\chacha20poly1305\internal\chacha20\chacha_generic.go
using binary = go.encoding.binary_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace crypto {
namespace chacha20poly1305 {
namespace @internal
{
    public static partial class chacha20_package
    {
        private static readonly long rounds = 20L;

        // core applies the ChaCha20 core function to 16-byte input in, 32-byte key k,
        // and 16-byte constant c, and puts the result into 64-byte array out.


        // core applies the ChaCha20 core function to 16-byte input in, 32-byte key k,
        // and 16-byte constant c, and puts the result into 64-byte array out.
        private static void core(ref array<byte> @out, ref array<byte> @in, ref array<byte> k)
        {
            var j0 = uint32(0x61707865UL);
            var j1 = uint32(0x3320646eUL);
            var j2 = uint32(0x79622d32UL);
            var j3 = uint32(0x6b206574UL);
            var j4 = binary.LittleEndian.Uint32(k[0L..4L]);
            var j5 = binary.LittleEndian.Uint32(k[4L..8L]);
            var j6 = binary.LittleEndian.Uint32(k[8L..12L]);
            var j7 = binary.LittleEndian.Uint32(k[12L..16L]);
            var j8 = binary.LittleEndian.Uint32(k[16L..20L]);
            var j9 = binary.LittleEndian.Uint32(k[20L..24L]);
            var j10 = binary.LittleEndian.Uint32(k[24L..28L]);
            var j11 = binary.LittleEndian.Uint32(k[28L..32L]);
            var j12 = binary.LittleEndian.Uint32(in[0L..4L]);
            var j13 = binary.LittleEndian.Uint32(in[4L..8L]);
            var j14 = binary.LittleEndian.Uint32(in[8L..12L]);
            var j15 = binary.LittleEndian.Uint32(in[12L..16L]);

            var x0 = j0;
            var x1 = j1;
            var x2 = j2;
            var x3 = j3;
            var x4 = j4;
            var x5 = j5;
            var x6 = j6;
            var x7 = j7;
            var x8 = j8;
            var x9 = j9;
            var x10 = j10;
            var x11 = j11;
            var x12 = j12;
            var x13 = j13;
            var x14 = j14;
            var x15 = j15;

            {
                long i = 0L;

                while (i < rounds)
                {
                    x0 += x4;
                    x12 ^= x0;
                    x12 = (x12 << (int)(16L)) | (x12 >> (int)((16L)));
                    x8 += x12;
                    x4 ^= x8;
                    x4 = (x4 << (int)(12L)) | (x4 >> (int)((20L)));
                    x0 += x4;
                    x12 ^= x0;
                    x12 = (x12 << (int)(8L)) | (x12 >> (int)((24L)));
                    x8 += x12;
                    x4 ^= x8;
                    x4 = (x4 << (int)(7L)) | (x4 >> (int)((25L)));
                    x1 += x5;
                    x13 ^= x1;
                    x13 = (x13 << (int)(16L)) | (x13 >> (int)(16L));
                    x9 += x13;
                    x5 ^= x9;
                    x5 = (x5 << (int)(12L)) | (x5 >> (int)(20L));
                    x1 += x5;
                    x13 ^= x1;
                    x13 = (x13 << (int)(8L)) | (x13 >> (int)(24L));
                    x9 += x13;
                    x5 ^= x9;
                    x5 = (x5 << (int)(7L)) | (x5 >> (int)(25L));
                    x2 += x6;
                    x14 ^= x2;
                    x14 = (x14 << (int)(16L)) | (x14 >> (int)(16L));
                    x10 += x14;
                    x6 ^= x10;
                    x6 = (x6 << (int)(12L)) | (x6 >> (int)(20L));
                    x2 += x6;
                    x14 ^= x2;
                    x14 = (x14 << (int)(8L)) | (x14 >> (int)(24L));
                    x10 += x14;
                    x6 ^= x10;
                    x6 = (x6 << (int)(7L)) | (x6 >> (int)(25L));
                    x3 += x7;
                    x15 ^= x3;
                    x15 = (x15 << (int)(16L)) | (x15 >> (int)(16L));
                    x11 += x15;
                    x7 ^= x11;
                    x7 = (x7 << (int)(12L)) | (x7 >> (int)(20L));
                    x3 += x7;
                    x15 ^= x3;
                    x15 = (x15 << (int)(8L)) | (x15 >> (int)(24L));
                    x11 += x15;
                    x7 ^= x11;
                    x7 = (x7 << (int)(7L)) | (x7 >> (int)(25L));
                    x0 += x5;
                    x15 ^= x0;
                    x15 = (x15 << (int)(16L)) | (x15 >> (int)(16L));
                    x10 += x15;
                    x5 ^= x10;
                    x5 = (x5 << (int)(12L)) | (x5 >> (int)(20L));
                    x0 += x5;
                    x15 ^= x0;
                    x15 = (x15 << (int)(8L)) | (x15 >> (int)(24L));
                    x10 += x15;
                    x5 ^= x10;
                    x5 = (x5 << (int)(7L)) | (x5 >> (int)(25L));
                    x1 += x6;
                    x12 ^= x1;
                    x12 = (x12 << (int)(16L)) | (x12 >> (int)(16L));
                    x11 += x12;
                    x6 ^= x11;
                    x6 = (x6 << (int)(12L)) | (x6 >> (int)(20L));
                    x1 += x6;
                    x12 ^= x1;
                    x12 = (x12 << (int)(8L)) | (x12 >> (int)(24L));
                    x11 += x12;
                    x6 ^= x11;
                    x6 = (x6 << (int)(7L)) | (x6 >> (int)(25L));
                    x2 += x7;
                    x13 ^= x2;
                    x13 = (x13 << (int)(16L)) | (x13 >> (int)(16L));
                    x8 += x13;
                    x7 ^= x8;
                    x7 = (x7 << (int)(12L)) | (x7 >> (int)(20L));
                    x2 += x7;
                    x13 ^= x2;
                    x13 = (x13 << (int)(8L)) | (x13 >> (int)(24L));
                    x8 += x13;
                    x7 ^= x8;
                    x7 = (x7 << (int)(7L)) | (x7 >> (int)(25L));
                    x3 += x4;
                    x14 ^= x3;
                    x14 = (x14 << (int)(16L)) | (x14 >> (int)(16L));
                    x9 += x14;
                    x4 ^= x9;
                    x4 = (x4 << (int)(12L)) | (x4 >> (int)(20L));
                    x3 += x4;
                    x14 ^= x3;
                    x14 = (x14 << (int)(8L)) | (x14 >> (int)(24L));
                    x9 += x14;
                    x4 ^= x9;
                    x4 = (x4 << (int)(7L)) | (x4 >> (int)(25L));
                    i += 2L;
                }

            }

            x0 += j0;
            x1 += j1;
            x2 += j2;
            x3 += j3;
            x4 += j4;
            x5 += j5;
            x6 += j6;
            x7 += j7;
            x8 += j8;
            x9 += j9;
            x10 += j10;
            x11 += j11;
            x12 += j12;
            x13 += j13;
            x14 += j14;
            x15 += j15;

            binary.LittleEndian.PutUint32(out[0L..4L], x0);
            binary.LittleEndian.PutUint32(out[4L..8L], x1);
            binary.LittleEndian.PutUint32(out[8L..12L], x2);
            binary.LittleEndian.PutUint32(out[12L..16L], x3);
            binary.LittleEndian.PutUint32(out[16L..20L], x4);
            binary.LittleEndian.PutUint32(out[20L..24L], x5);
            binary.LittleEndian.PutUint32(out[24L..28L], x6);
            binary.LittleEndian.PutUint32(out[28L..32L], x7);
            binary.LittleEndian.PutUint32(out[32L..36L], x8);
            binary.LittleEndian.PutUint32(out[36L..40L], x9);
            binary.LittleEndian.PutUint32(out[40L..44L], x10);
            binary.LittleEndian.PutUint32(out[44L..48L], x11);
            binary.LittleEndian.PutUint32(out[48L..52L], x12);
            binary.LittleEndian.PutUint32(out[52L..56L], x13);
            binary.LittleEndian.PutUint32(out[56L..60L], x14);
            binary.LittleEndian.PutUint32(out[60L..64L], x15);
        }

        // XORKeyStream crypts bytes from in to out using the given key and counters.
        // In and out may be the same slice but otherwise should not overlap. Counter
        // contains the raw ChaCha20 counter bytes (i.e. block counter followed by
        // nonce).
        public static void XORKeyStream(slice<byte> @out, slice<byte> @in, ref array<byte> counter, ref array<byte> key)
        {
            array<byte> block = new array<byte>(64L);
            array<byte> counterCopy = new array<byte>(16L);
            copy(counterCopy[..], counter[..]);

            while (len(in) >= 64L)
            {
                core(ref block, ref counterCopy, key);
                {
                    var i__prev2 = i;

                    foreach (var (__i, __x) in block)
                    {
                        i = __i;
                        x = __x;
                        out[i] = in[i] ^ x;
                    }

                    i = i__prev2;
                }

                var u = uint32(1L);
                {
                    var i__prev2 = i;

                    for (long i = 0L; i < 4L; i++)
                    {
                        u += uint32(counterCopy[i]);
                        counterCopy[i] = byte(u);
                        u >>= 8L;
                    }


                    i = i__prev2;
                }
                in = in[64L..];
                out = out[64L..];
            }


            if (len(in) > 0L)
            {
                core(ref block, ref counterCopy, key);
                {
                    var i__prev1 = i;

                    foreach (var (__i, __v) in in)
                    {
                        i = __i;
                        v = __v;
                        out[i] = v ^ block[i];
                    }

                    i = i__prev1;
                }

            }
        }
    }
}}}}}}}
