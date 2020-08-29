// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This Go implementation is derived in part from the reference
// ANSI C implementation, which carries the following notice:
//
//    rijndael-alg-fst.c
//
//    @version 3.0 (December 2000)
//
//    Optimised ANSI C code for the Rijndael cipher (now AES)
//
//    @author Vincent Rijmen <vincent.rijmen@esat.kuleuven.ac.be>
//    @author Antoon Bosselaers <antoon.bosselaers@esat.kuleuven.ac.be>
//    @author Paulo Barreto <paulo.barreto@terra.com.br>
//
//    This code is hereby placed in the public domain.
//
//    THIS SOFTWARE IS PROVIDED BY THE AUTHORS ''AS IS'' AND ANY EXPRESS
//    OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//    WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
//    ARE DISCLAIMED.  IN NO EVENT SHALL THE AUTHORS OR CONTRIBUTORS BE
//    LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
//    CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
//    SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
//    BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
//    WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
//    OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
//    EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// See FIPS 197 for specification, and see Daemen and Rijmen's Rijndael submission
// for implementation details.
//    http://www.csrc.nist.gov/publications/fips/fips197/fips-197.pdf
//    http://csrc.nist.gov/archive/aes/rijndael/Rijndael-ammended.pdf

// package aes -- go2cs converted at 2020 August 29 08:28:44 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Go\src\crypto\aes\block.go

using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class aes_package
    {
        // Encrypt one block from src into dst, using the expanded key xk.
        private static void encryptBlockGo(slice<uint> xk, slice<byte> dst, slice<byte> src)
        {
            uint s0 = default;            uint s1 = default;            uint s2 = default;            uint s3 = default;            uint t0 = default;            uint t1 = default;            uint t2 = default;            uint t3 = default;



            s0 = uint32(src[0L]) << (int)(24L) | uint32(src[1L]) << (int)(16L) | uint32(src[2L]) << (int)(8L) | uint32(src[3L]);
            s1 = uint32(src[4L]) << (int)(24L) | uint32(src[5L]) << (int)(16L) | uint32(src[6L]) << (int)(8L) | uint32(src[7L]);
            s2 = uint32(src[8L]) << (int)(24L) | uint32(src[9L]) << (int)(16L) | uint32(src[10L]) << (int)(8L) | uint32(src[11L]);
            s3 = uint32(src[12L]) << (int)(24L) | uint32(src[13L]) << (int)(16L) | uint32(src[14L]) << (int)(8L) | uint32(src[15L]); 

            // First round just XORs input with key.
            s0 ^= xk[0L];
            s1 ^= xk[1L];
            s2 ^= xk[2L];
            s3 ^= xk[3L]; 

            // Middle rounds shuffle using tables.
            // Number of rounds is set by length of expanded key.
            var nr = len(xk) / 4L - 2L; // - 2: one above, one more below
            long k = 4L;
            for (long r = 0L; r < nr; r++)
            {
                t0 = xk[k + 0L] ^ te0[uint8(s0 >> (int)(24L))] ^ te1[uint8(s1 >> (int)(16L))] ^ te2[uint8(s2 >> (int)(8L))] ^ te3[uint8(s3)];
                t1 = xk[k + 1L] ^ te0[uint8(s1 >> (int)(24L))] ^ te1[uint8(s2 >> (int)(16L))] ^ te2[uint8(s3 >> (int)(8L))] ^ te3[uint8(s0)];
                t2 = xk[k + 2L] ^ te0[uint8(s2 >> (int)(24L))] ^ te1[uint8(s3 >> (int)(16L))] ^ te2[uint8(s0 >> (int)(8L))] ^ te3[uint8(s1)];
                t3 = xk[k + 3L] ^ te0[uint8(s3 >> (int)(24L))] ^ te1[uint8(s0 >> (int)(16L))] ^ te2[uint8(s1 >> (int)(8L))] ^ te3[uint8(s2)];
                k += 4L;
                s0 = t0;
                s1 = t1;
                s2 = t2;
                s3 = t3;
            } 

            // Last round uses s-box directly and XORs to produce output.
            s0 = uint32(sbox0[t0 >> (int)(24L)]) << (int)(24L) | uint32(sbox0[t1 >> (int)(16L) & 0xffUL]) << (int)(16L) | uint32(sbox0[t2 >> (int)(8L) & 0xffUL]) << (int)(8L) | uint32(sbox0[t3 & 0xffUL]);
            s1 = uint32(sbox0[t1 >> (int)(24L)]) << (int)(24L) | uint32(sbox0[t2 >> (int)(16L) & 0xffUL]) << (int)(16L) | uint32(sbox0[t3 >> (int)(8L) & 0xffUL]) << (int)(8L) | uint32(sbox0[t0 & 0xffUL]);
            s2 = uint32(sbox0[t2 >> (int)(24L)]) << (int)(24L) | uint32(sbox0[t3 >> (int)(16L) & 0xffUL]) << (int)(16L) | uint32(sbox0[t0 >> (int)(8L) & 0xffUL]) << (int)(8L) | uint32(sbox0[t1 & 0xffUL]);
            s3 = uint32(sbox0[t3 >> (int)(24L)]) << (int)(24L) | uint32(sbox0[t0 >> (int)(16L) & 0xffUL]) << (int)(16L) | uint32(sbox0[t1 >> (int)(8L) & 0xffUL]) << (int)(8L) | uint32(sbox0[t2 & 0xffUL]);

            s0 ^= xk[k + 0L];
            s1 ^= xk[k + 1L];
            s2 ^= xk[k + 2L];
            s3 ^= xk[k + 3L];

            dst[0L] = byte(s0 >> (int)(24L));
            dst[1L] = byte(s0 >> (int)(16L));
            dst[2L] = byte(s0 >> (int)(8L));
            dst[3L] = byte(s0);
            dst[4L] = byte(s1 >> (int)(24L));
            dst[5L] = byte(s1 >> (int)(16L));
            dst[6L] = byte(s1 >> (int)(8L));
            dst[7L] = byte(s1);
            dst[8L] = byte(s2 >> (int)(24L));
            dst[9L] = byte(s2 >> (int)(16L));
            dst[10L] = byte(s2 >> (int)(8L));
            dst[11L] = byte(s2);
            dst[12L] = byte(s3 >> (int)(24L));
            dst[13L] = byte(s3 >> (int)(16L));
            dst[14L] = byte(s3 >> (int)(8L));
            dst[15L] = byte(s3);
        }

        // Decrypt one block from src into dst, using the expanded key xk.
        private static void decryptBlockGo(slice<uint> xk, slice<byte> dst, slice<byte> src)
        {
            uint s0 = default;            uint s1 = default;            uint s2 = default;            uint s3 = default;            uint t0 = default;            uint t1 = default;            uint t2 = default;            uint t3 = default;



            s0 = uint32(src[0L]) << (int)(24L) | uint32(src[1L]) << (int)(16L) | uint32(src[2L]) << (int)(8L) | uint32(src[3L]);
            s1 = uint32(src[4L]) << (int)(24L) | uint32(src[5L]) << (int)(16L) | uint32(src[6L]) << (int)(8L) | uint32(src[7L]);
            s2 = uint32(src[8L]) << (int)(24L) | uint32(src[9L]) << (int)(16L) | uint32(src[10L]) << (int)(8L) | uint32(src[11L]);
            s3 = uint32(src[12L]) << (int)(24L) | uint32(src[13L]) << (int)(16L) | uint32(src[14L]) << (int)(8L) | uint32(src[15L]); 

            // First round just XORs input with key.
            s0 ^= xk[0L];
            s1 ^= xk[1L];
            s2 ^= xk[2L];
            s3 ^= xk[3L]; 

            // Middle rounds shuffle using tables.
            // Number of rounds is set by length of expanded key.
            var nr = len(xk) / 4L - 2L; // - 2: one above, one more below
            long k = 4L;
            for (long r = 0L; r < nr; r++)
            {
                t0 = xk[k + 0L] ^ td0[uint8(s0 >> (int)(24L))] ^ td1[uint8(s3 >> (int)(16L))] ^ td2[uint8(s2 >> (int)(8L))] ^ td3[uint8(s1)];
                t1 = xk[k + 1L] ^ td0[uint8(s1 >> (int)(24L))] ^ td1[uint8(s0 >> (int)(16L))] ^ td2[uint8(s3 >> (int)(8L))] ^ td3[uint8(s2)];
                t2 = xk[k + 2L] ^ td0[uint8(s2 >> (int)(24L))] ^ td1[uint8(s1 >> (int)(16L))] ^ td2[uint8(s0 >> (int)(8L))] ^ td3[uint8(s3)];
                t3 = xk[k + 3L] ^ td0[uint8(s3 >> (int)(24L))] ^ td1[uint8(s2 >> (int)(16L))] ^ td2[uint8(s1 >> (int)(8L))] ^ td3[uint8(s0)];
                k += 4L;
                s0 = t0;
                s1 = t1;
                s2 = t2;
                s3 = t3;
            } 

            // Last round uses s-box directly and XORs to produce output.
 

            // Last round uses s-box directly and XORs to produce output.
            s0 = uint32(sbox1[t0 >> (int)(24L)]) << (int)(24L) | uint32(sbox1[t3 >> (int)(16L) & 0xffUL]) << (int)(16L) | uint32(sbox1[t2 >> (int)(8L) & 0xffUL]) << (int)(8L) | uint32(sbox1[t1 & 0xffUL]);
            s1 = uint32(sbox1[t1 >> (int)(24L)]) << (int)(24L) | uint32(sbox1[t0 >> (int)(16L) & 0xffUL]) << (int)(16L) | uint32(sbox1[t3 >> (int)(8L) & 0xffUL]) << (int)(8L) | uint32(sbox1[t2 & 0xffUL]);
            s2 = uint32(sbox1[t2 >> (int)(24L)]) << (int)(24L) | uint32(sbox1[t1 >> (int)(16L) & 0xffUL]) << (int)(16L) | uint32(sbox1[t0 >> (int)(8L) & 0xffUL]) << (int)(8L) | uint32(sbox1[t3 & 0xffUL]);
            s3 = uint32(sbox1[t3 >> (int)(24L)]) << (int)(24L) | uint32(sbox1[t2 >> (int)(16L) & 0xffUL]) << (int)(16L) | uint32(sbox1[t1 >> (int)(8L) & 0xffUL]) << (int)(8L) | uint32(sbox1[t0 & 0xffUL]);

            s0 ^= xk[k + 0L];
            s1 ^= xk[k + 1L];
            s2 ^= xk[k + 2L];
            s3 ^= xk[k + 3L];

            dst[0L] = byte(s0 >> (int)(24L));
            dst[1L] = byte(s0 >> (int)(16L));
            dst[2L] = byte(s0 >> (int)(8L));
            dst[3L] = byte(s0);
            dst[4L] = byte(s1 >> (int)(24L));
            dst[5L] = byte(s1 >> (int)(16L));
            dst[6L] = byte(s1 >> (int)(8L));
            dst[7L] = byte(s1);
            dst[8L] = byte(s2 >> (int)(24L));
            dst[9L] = byte(s2 >> (int)(16L));
            dst[10L] = byte(s2 >> (int)(8L));
            dst[11L] = byte(s2);
            dst[12L] = byte(s3 >> (int)(24L));
            dst[13L] = byte(s3 >> (int)(16L));
            dst[14L] = byte(s3 >> (int)(8L));
            dst[15L] = byte(s3);
        }

        // Apply sbox0 to each byte in w.
        private static uint subw(uint w)
        {
            return uint32(sbox0[w >> (int)(24L)]) << (int)(24L) | uint32(sbox0[w >> (int)(16L) & 0xffUL]) << (int)(16L) | uint32(sbox0[w >> (int)(8L) & 0xffUL]) << (int)(8L) | uint32(sbox0[w & 0xffUL]);
        }

        // Rotate
        private static uint rotw(uint w)
        {
            return w << (int)(8L) | w >> (int)(24L);
        }

        // Key expansion algorithm. See FIPS-197, Figure 11.
        // Their rcon[i] is our powx[i-1] << 24.
        private static void expandKeyGo(slice<byte> key, slice<uint> enc, slice<uint> dec)
        { 
            // Encryption key setup.
            long i = default;
            var nk = len(key) / 4L;
            for (i = 0L; i < nk; i++)
            {
                enc[i] = uint32(key[4L * i]) << (int)(24L) | uint32(key[4L * i + 1L]) << (int)(16L) | uint32(key[4L * i + 2L]) << (int)(8L) | uint32(key[4L * i + 3L]);
            }

            while (i < len(enc))
            {
                var t = enc[i - 1L];
                if (i % nk == 0L)
                {
                    t = subw(rotw(t)) ^ (uint32(powx[i / nk - 1L]) << (int)(24L));
                i++;
                }
                else if (nk > 6L && i % nk == 4L)
                {
                    t = subw(t);
                }
                enc[i] = enc[i - nk] ^ t;
            } 

            // Derive decryption key from encryption key.
            // Reverse the 4-word round key sets from enc to produce dec.
            // All sets but the first and last get the MixColumn transform applied.
 

            // Derive decryption key from encryption key.
            // Reverse the 4-word round key sets from enc to produce dec.
            // All sets but the first and last get the MixColumn transform applied.
            if (dec == null)
            {
                return;
            }
            var n = len(enc);
            {
                long i__prev1 = i;

                i = 0L;

                while (i < n)
                {
                    var ei = n - i - 4L;
                    for (long j = 0L; j < 4L; j++)
                    {
                        var x = enc[ei + j];
                        if (i > 0L && i + 4L < n)
                        {
                            x = td0[sbox0[x >> (int)(24L)]] ^ td1[sbox0[x >> (int)(16L) & 0xffUL]] ^ td2[sbox0[x >> (int)(8L) & 0xffUL]] ^ td3[sbox0[x & 0xffUL]];
                        }
                        dec[i + j] = x;
                    i += 4L;
                    }

                }


                i = i__prev1;
            }
        }
    }
}}
