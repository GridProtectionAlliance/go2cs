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
//    https://csrc.nist.gov/csrc/media/publications/fips/197/final/documents/fips-197.pdf
//    https://csrc.nist.gov/archive/aes/rijndael/Rijndael-ammended.pdf

// package aes -- go2cs converted at 2022 March 13 05:32:26 UTC
// import "crypto/aes" ==> using aes = go.crypto.aes_package
// Original source: C:\Program Files\Go\src\crypto\aes\block.go
namespace go.crypto;

using binary = encoding.binary_package;


// Encrypt one block from src into dst, using the expanded key xk.

public static partial class aes_package {

private static void encryptBlockGo(slice<uint> xk, slice<byte> dst, slice<byte> src) {
    _ = src[15]; // early bounds check
    var s0 = binary.BigEndian.Uint32(src[(int)0..(int)4]);
    var s1 = binary.BigEndian.Uint32(src[(int)4..(int)8]);
    var s2 = binary.BigEndian.Uint32(src[(int)8..(int)12]);
    var s3 = binary.BigEndian.Uint32(src[(int)12..(int)16]); 

    // First round just XORs input with key.
    s0 ^= xk[0];
    s1 ^= xk[1];
    s2 ^= xk[2];
    s3 ^= xk[3]; 

    // Middle rounds shuffle using tables.
    // Number of rounds is set by length of expanded key.
    var nr = len(xk) / 4 - 2; // - 2: one above, one more below
    nint k = 4;
    uint t0 = default;    uint t1 = default;    uint t2 = default;    uint t3 = default;

    for (nint r = 0; r < nr; r++) {
        t0 = xk[k + 0] ^ te0[uint8(s0 >> 24)] ^ te1[uint8(s1 >> 16)] ^ te2[uint8(s2 >> 8)] ^ te3[uint8(s3)];
        t1 = xk[k + 1] ^ te0[uint8(s1 >> 24)] ^ te1[uint8(s2 >> 16)] ^ te2[uint8(s3 >> 8)] ^ te3[uint8(s0)];
        t2 = xk[k + 2] ^ te0[uint8(s2 >> 24)] ^ te1[uint8(s3 >> 16)] ^ te2[uint8(s0 >> 8)] ^ te3[uint8(s1)];
        t3 = xk[k + 3] ^ te0[uint8(s3 >> 24)] ^ te1[uint8(s0 >> 16)] ^ te2[uint8(s1 >> 8)] ^ te3[uint8(s2)];
        k += 4;
        (s0, s1, s2, s3) = (t0, t1, t2, t3);
    } 

    // Last round uses s-box directly and XORs to produce output.
    s0 = uint32(sbox0[t0 >> 24]) << 24 | uint32(sbox0[t1 >> 16 & 0xff]) << 16 | uint32(sbox0[t2 >> 8 & 0xff]) << 8 | uint32(sbox0[t3 & 0xff]);
    s1 = uint32(sbox0[t1 >> 24]) << 24 | uint32(sbox0[t2 >> 16 & 0xff]) << 16 | uint32(sbox0[t3 >> 8 & 0xff]) << 8 | uint32(sbox0[t0 & 0xff]);
    s2 = uint32(sbox0[t2 >> 24]) << 24 | uint32(sbox0[t3 >> 16 & 0xff]) << 16 | uint32(sbox0[t0 >> 8 & 0xff]) << 8 | uint32(sbox0[t1 & 0xff]);
    s3 = uint32(sbox0[t3 >> 24]) << 24 | uint32(sbox0[t0 >> 16 & 0xff]) << 16 | uint32(sbox0[t1 >> 8 & 0xff]) << 8 | uint32(sbox0[t2 & 0xff]);

    s0 ^= xk[k + 0];
    s1 ^= xk[k + 1];
    s2 ^= xk[k + 2];
    s3 ^= xk[k + 3];

    _ = dst[15]; // early bounds check
    binary.BigEndian.PutUint32(dst[(int)0..(int)4], s0);
    binary.BigEndian.PutUint32(dst[(int)4..(int)8], s1);
    binary.BigEndian.PutUint32(dst[(int)8..(int)12], s2);
    binary.BigEndian.PutUint32(dst[(int)12..(int)16], s3);
}

// Decrypt one block from src into dst, using the expanded key xk.
private static void decryptBlockGo(slice<uint> xk, slice<byte> dst, slice<byte> src) {
    _ = src[15]; // early bounds check
    var s0 = binary.BigEndian.Uint32(src[(int)0..(int)4]);
    var s1 = binary.BigEndian.Uint32(src[(int)4..(int)8]);
    var s2 = binary.BigEndian.Uint32(src[(int)8..(int)12]);
    var s3 = binary.BigEndian.Uint32(src[(int)12..(int)16]); 

    // First round just XORs input with key.
    s0 ^= xk[0];
    s1 ^= xk[1];
    s2 ^= xk[2];
    s3 ^= xk[3]; 

    // Middle rounds shuffle using tables.
    // Number of rounds is set by length of expanded key.
    var nr = len(xk) / 4 - 2; // - 2: one above, one more below
    nint k = 4;
    uint t0 = default;    uint t1 = default;    uint t2 = default;    uint t3 = default;

    for (nint r = 0; r < nr; r++) {
        t0 = xk[k + 0] ^ td0[uint8(s0 >> 24)] ^ td1[uint8(s3 >> 16)] ^ td2[uint8(s2 >> 8)] ^ td3[uint8(s1)];
        t1 = xk[k + 1] ^ td0[uint8(s1 >> 24)] ^ td1[uint8(s0 >> 16)] ^ td2[uint8(s3 >> 8)] ^ td3[uint8(s2)];
        t2 = xk[k + 2] ^ td0[uint8(s2 >> 24)] ^ td1[uint8(s1 >> 16)] ^ td2[uint8(s0 >> 8)] ^ td3[uint8(s3)];
        t3 = xk[k + 3] ^ td0[uint8(s3 >> 24)] ^ td1[uint8(s2 >> 16)] ^ td2[uint8(s1 >> 8)] ^ td3[uint8(s0)];
        k += 4;
        (s0, s1, s2, s3) = (t0, t1, t2, t3);
    } 

    // Last round uses s-box directly and XORs to produce output.
    s0 = uint32(sbox1[t0 >> 24]) << 24 | uint32(sbox1[t3 >> 16 & 0xff]) << 16 | uint32(sbox1[t2 >> 8 & 0xff]) << 8 | uint32(sbox1[t1 & 0xff]);
    s1 = uint32(sbox1[t1 >> 24]) << 24 | uint32(sbox1[t0 >> 16 & 0xff]) << 16 | uint32(sbox1[t3 >> 8 & 0xff]) << 8 | uint32(sbox1[t2 & 0xff]);
    s2 = uint32(sbox1[t2 >> 24]) << 24 | uint32(sbox1[t1 >> 16 & 0xff]) << 16 | uint32(sbox1[t0 >> 8 & 0xff]) << 8 | uint32(sbox1[t3 & 0xff]);
    s3 = uint32(sbox1[t3 >> 24]) << 24 | uint32(sbox1[t2 >> 16 & 0xff]) << 16 | uint32(sbox1[t1 >> 8 & 0xff]) << 8 | uint32(sbox1[t0 & 0xff]);

    s0 ^= xk[k + 0];
    s1 ^= xk[k + 1];
    s2 ^= xk[k + 2];
    s3 ^= xk[k + 3];

    _ = dst[15]; // early bounds check
    binary.BigEndian.PutUint32(dst[(int)0..(int)4], s0);
    binary.BigEndian.PutUint32(dst[(int)4..(int)8], s1);
    binary.BigEndian.PutUint32(dst[(int)8..(int)12], s2);
    binary.BigEndian.PutUint32(dst[(int)12..(int)16], s3);
}

// Apply sbox0 to each byte in w.
private static uint subw(uint w) {
    return uint32(sbox0[w >> 24]) << 24 | uint32(sbox0[w >> 16 & 0xff]) << 16 | uint32(sbox0[w >> 8 & 0xff]) << 8 | uint32(sbox0[w & 0xff]);
}

// Rotate
private static uint rotw(uint w) {
    return w << 8 | w >> 24;
}

// Key expansion algorithm. See FIPS-197, Figure 11.
// Their rcon[i] is our powx[i-1] << 24.
private static void expandKeyGo(slice<byte> key, slice<uint> enc, slice<uint> dec) { 
    // Encryption key setup.
    nint i = default;
    var nk = len(key) / 4;
    for (i = 0; i < nk; i++) {
        enc[i] = binary.BigEndian.Uint32(key[(int)4 * i..]);
    }
    while (i < len(enc)) {
        var t = enc[i - 1];
        if (i % nk == 0) {
            t = subw(rotw(t)) ^ (uint32(powx[i / nk - 1]) << 24);
        i++;
        }
        else if (nk > 6 && i % nk == 4) {
            t = subw(t);
        }
        enc[i] = enc[i - nk] ^ t;
    } 

    // Derive decryption key from encryption key.
    // Reverse the 4-word round key sets from enc to produce dec.
    // All sets but the first and last get the MixColumn transform applied.
    if (dec == null) {
        return ;
    }
    var n = len(enc);
    {
        nint i__prev1 = i;

        i = 0;

        while (i < n) {
            var ei = n - i - 4;
            for (nint j = 0; j < 4; j++) {
                var x = enc[ei + j];
                if (i > 0 && i + 4 < n) {
                    x = td0[sbox0[x >> 24]] ^ td1[sbox0[x >> 16 & 0xff]] ^ td2[sbox0[x >> 8 & 0xff]] ^ td3[sbox0[x & 0xff]];
                }
                dec[i + j] = x;
            i += 4;
            }
        }

        i = i__prev1;
    }
}

} // end aes_package
