// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This Go implementation is derived in part from the reference
// ANSI C implementation, which carries the following notice:
//
//	rijndael-alg-fst.c
//
//	@version 3.0 (December 2000)
//
//	Optimised ANSI C code for the Rijndael cipher (now AES)
//
//	@author Vincent Rijmen <vincent.rijmen@esat.kuleuven.ac.be>
//	@author Antoon Bosselaers <antoon.bosselaers@esat.kuleuven.ac.be>
//	@author Paulo Barreto <paulo.barreto@terra.com.br>
//
//	This code is hereby placed in the public domain.
//
//	THIS SOFTWARE IS PROVIDED BY THE AUTHORS ''AS IS'' AND ANY EXPRESS
//	OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
//	ARE DISCLAIMED.  IN NO EVENT SHALL THE AUTHORS OR CONTRIBUTORS BE
//	LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
//	CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
//	SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR
//	BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
//	WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
//	OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
//	EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// See FIPS 197 for specification, and see Daemen and Rijmen's Rijndael submission
// for implementation details.
//	https://csrc.nist.gov/csrc/media/publications/fips/197/final/documents/fips-197.pdf
//	https://csrc.nist.gov/archive/aes/rijndael/Rijndael-ammended.pdf
namespace go.crypto;

using byteorder = @internal.byteorder_package;
using @internal;

partial class aes_package {

// Encrypt one block from src into dst, using the expanded key xk.
internal static void encryptBlockGo(slice<uint32> xk, slice<byte> dst, slice<byte> src) {
    _ = src[15];
    // early bounds check
    var s0 = byteorder.BeUint32(src[0..4]);
    var s1 = byteorder.BeUint32(src[4..8]);
    var s2 = byteorder.BeUint32(src[8..12]);
    var s3 = byteorder.BeUint32(src[12..16]);
    // First round just XORs input with key.
    s0 ^= (uint32)(xk[0]);
    s1 ^= (uint32)(xk[1]);
    s2 ^= (uint32)(xk[2]);
    s3 ^= (uint32)(xk[3]);
    // Middle rounds shuffle using tables.
    // Number of rounds is set by length of expanded key.
    nint nr = len(xk) / 4 - 2;
    // - 2: one above, one more below
    nint k = 4;
    uint32 t0 = default!;
    uint32 t1 = default!;
    uint32 t2 = default!;
    uint32 t3 = default!;
    for (nint r = 0; r < nr; r++) {
        t0 = (uint32)((uint32)((uint32)((uint32)(xk[k + 0] ^ te0[((uint8)(s0 >> (int)(24)))]) ^ te1[((uint8)(s1 >> (int)(16)))]) ^ te2[((uint8)(s2 >> (int)(8)))]) ^ te3[((uint8)s3)]);
        t1 = (uint32)((uint32)((uint32)((uint32)(xk[k + 1] ^ te0[((uint8)(s1 >> (int)(24)))]) ^ te1[((uint8)(s2 >> (int)(16)))]) ^ te2[((uint8)(s3 >> (int)(8)))]) ^ te3[((uint8)s0)]);
        t2 = (uint32)((uint32)((uint32)((uint32)(xk[k + 2] ^ te0[((uint8)(s2 >> (int)(24)))]) ^ te1[((uint8)(s3 >> (int)(16)))]) ^ te2[((uint8)(s0 >> (int)(8)))]) ^ te3[((uint8)s1)]);
        t3 = (uint32)((uint32)((uint32)((uint32)(xk[k + 3] ^ te0[((uint8)(s3 >> (int)(24)))]) ^ te1[((uint8)(s0 >> (int)(16)))]) ^ te2[((uint8)(s1 >> (int)(8)))]) ^ te3[((uint8)s2)]);
        k += 4;
        (s0, s1, s2, s3) = (t0, t1, t2, t3);
    }
    // Last round uses s-box directly and XORs to produce output.
    s0 = (uint32)((uint32)((uint32)(((uint32)sbox0[t0 >> (int)(24)]) << (int)(24) | ((uint32)sbox0[(uint32)(t1 >> (int)(16) & 255)]) << (int)(16)) | ((uint32)sbox0[(uint32)(t2 >> (int)(8) & 255)]) << (int)(8)) | ((uint32)sbox0[(uint32)(t3 & 255)]));
    s1 = (uint32)((uint32)((uint32)(((uint32)sbox0[t1 >> (int)(24)]) << (int)(24) | ((uint32)sbox0[(uint32)(t2 >> (int)(16) & 255)]) << (int)(16)) | ((uint32)sbox0[(uint32)(t3 >> (int)(8) & 255)]) << (int)(8)) | ((uint32)sbox0[(uint32)(t0 & 255)]));
    s2 = (uint32)((uint32)((uint32)(((uint32)sbox0[t2 >> (int)(24)]) << (int)(24) | ((uint32)sbox0[(uint32)(t3 >> (int)(16) & 255)]) << (int)(16)) | ((uint32)sbox0[(uint32)(t0 >> (int)(8) & 255)]) << (int)(8)) | ((uint32)sbox0[(uint32)(t1 & 255)]));
    s3 = (uint32)((uint32)((uint32)(((uint32)sbox0[t3 >> (int)(24)]) << (int)(24) | ((uint32)sbox0[(uint32)(t0 >> (int)(16) & 255)]) << (int)(16)) | ((uint32)sbox0[(uint32)(t1 >> (int)(8) & 255)]) << (int)(8)) | ((uint32)sbox0[(uint32)(t2 & 255)]));
    s0 ^= (uint32)(xk[k + 0]);
    s1 ^= (uint32)(xk[k + 1]);
    s2 ^= (uint32)(xk[k + 2]);
    s3 ^= (uint32)(xk[k + 3]);
    _ = dst[15];
    // early bounds check
    byteorder.BePutUint32(dst[0..4], s0);
    byteorder.BePutUint32(dst[4..8], s1);
    byteorder.BePutUint32(dst[8..12], s2);
    byteorder.BePutUint32(dst[12..16], s3);
}

// Decrypt one block from src into dst, using the expanded key xk.
internal static void decryptBlockGo(slice<uint32> xk, slice<byte> dst, slice<byte> src) {
    _ = src[15];
    // early bounds check
    var s0 = byteorder.BeUint32(src[0..4]);
    var s1 = byteorder.BeUint32(src[4..8]);
    var s2 = byteorder.BeUint32(src[8..12]);
    var s3 = byteorder.BeUint32(src[12..16]);
    // First round just XORs input with key.
    s0 ^= (uint32)(xk[0]);
    s1 ^= (uint32)(xk[1]);
    s2 ^= (uint32)(xk[2]);
    s3 ^= (uint32)(xk[3]);
    // Middle rounds shuffle using tables.
    // Number of rounds is set by length of expanded key.
    nint nr = len(xk) / 4 - 2;
    // - 2: one above, one more below
    nint k = 4;
    uint32 t0 = default!;
    uint32 t1 = default!;
    uint32 t2 = default!;
    uint32 t3 = default!;
    for (nint r = 0; r < nr; r++) {
        t0 = (uint32)((uint32)((uint32)((uint32)(xk[k + 0] ^ td0[((uint8)(s0 >> (int)(24)))]) ^ td1[((uint8)(s3 >> (int)(16)))]) ^ td2[((uint8)(s2 >> (int)(8)))]) ^ td3[((uint8)s1)]);
        t1 = (uint32)((uint32)((uint32)((uint32)(xk[k + 1] ^ td0[((uint8)(s1 >> (int)(24)))]) ^ td1[((uint8)(s0 >> (int)(16)))]) ^ td2[((uint8)(s3 >> (int)(8)))]) ^ td3[((uint8)s2)]);
        t2 = (uint32)((uint32)((uint32)((uint32)(xk[k + 2] ^ td0[((uint8)(s2 >> (int)(24)))]) ^ td1[((uint8)(s1 >> (int)(16)))]) ^ td2[((uint8)(s0 >> (int)(8)))]) ^ td3[((uint8)s3)]);
        t3 = (uint32)((uint32)((uint32)((uint32)(xk[k + 3] ^ td0[((uint8)(s3 >> (int)(24)))]) ^ td1[((uint8)(s2 >> (int)(16)))]) ^ td2[((uint8)(s1 >> (int)(8)))]) ^ td3[((uint8)s0)]);
        k += 4;
        (s0, s1, s2, s3) = (t0, t1, t2, t3);
    }
    // Last round uses s-box directly and XORs to produce output.
    s0 = (uint32)((uint32)((uint32)(((uint32)sbox1[t0 >> (int)(24)]) << (int)(24) | ((uint32)sbox1[(uint32)(t3 >> (int)(16) & 255)]) << (int)(16)) | ((uint32)sbox1[(uint32)(t2 >> (int)(8) & 255)]) << (int)(8)) | ((uint32)sbox1[(uint32)(t1 & 255)]));
    s1 = (uint32)((uint32)((uint32)(((uint32)sbox1[t1 >> (int)(24)]) << (int)(24) | ((uint32)sbox1[(uint32)(t0 >> (int)(16) & 255)]) << (int)(16)) | ((uint32)sbox1[(uint32)(t3 >> (int)(8) & 255)]) << (int)(8)) | ((uint32)sbox1[(uint32)(t2 & 255)]));
    s2 = (uint32)((uint32)((uint32)(((uint32)sbox1[t2 >> (int)(24)]) << (int)(24) | ((uint32)sbox1[(uint32)(t1 >> (int)(16) & 255)]) << (int)(16)) | ((uint32)sbox1[(uint32)(t0 >> (int)(8) & 255)]) << (int)(8)) | ((uint32)sbox1[(uint32)(t3 & 255)]));
    s3 = (uint32)((uint32)((uint32)(((uint32)sbox1[t3 >> (int)(24)]) << (int)(24) | ((uint32)sbox1[(uint32)(t2 >> (int)(16) & 255)]) << (int)(16)) | ((uint32)sbox1[(uint32)(t1 >> (int)(8) & 255)]) << (int)(8)) | ((uint32)sbox1[(uint32)(t0 & 255)]));
    s0 ^= (uint32)(xk[k + 0]);
    s1 ^= (uint32)(xk[k + 1]);
    s2 ^= (uint32)(xk[k + 2]);
    s3 ^= (uint32)(xk[k + 3]);
    _ = dst[15];
    // early bounds check
    byteorder.BePutUint32(dst[0..4], s0);
    byteorder.BePutUint32(dst[4..8], s1);
    byteorder.BePutUint32(dst[8..12], s2);
    byteorder.BePutUint32(dst[12..16], s3);
}

// Apply sbox0 to each byte in w.
internal static uint32 subw(uint32 w) {
    return (uint32)((uint32)((uint32)(((uint32)sbox0[w >> (int)(24)]) << (int)(24) | ((uint32)sbox0[(uint32)(w >> (int)(16) & 255)]) << (int)(16)) | ((uint32)sbox0[(uint32)(w >> (int)(8) & 255)]) << (int)(8)) | ((uint32)sbox0[(uint32)(w & 255)]));
}

// Rotate
internal static uint32 rotw(uint32 w) {
    return (uint32)(w << (int)(8) | w >> (int)(24));
}

// Key expansion algorithm. See FIPS-197, Figure 11.
// Their rcon[i] is our powx[i-1] << 24.
internal static void expandKeyGo(slice<byte> key, slice<uint32> enc, slice<uint32> dec) {
    // Encryption key setup.
    nint i = default!;
    nint nk = len(key) / 4;
    for (i = 0; i < nk; i++) {
        enc[i] = byteorder.BeUint32(key[(int)(4 * i)..]);
    }
    for (; i < len(enc); i++) {
        var t = enc[i - 1];
        if (i % nk == 0){
            t = (uint32)(subw(rotw(t)) ^ (((uint32)powx[i / nk - 1]) << (int)(24)));
        } else 
        if (nk > 6 && i % nk == 4) {
            t = subw(t);
        }
        enc[i] = (uint32)(enc[i - nk] ^ t);
    }
    // Derive decryption key from encryption key.
    // Reverse the 4-word round key sets from enc to produce dec.
    // All sets but the first and last get the MixColumn transform applied.
    if (dec == default!) {
        return;
    }
    nint n = len(enc);
    for (nint iΔ1 = 0; iΔ1 < n; i += 4) {
        nint ei = n - iΔ1 - 4;
        for (nint j = 0; j < 4; j++) {
            var x = enc[ei + j];
            if (iΔ1 > 0 && iΔ1 + 4 < n) {
                x = (uint32)((uint32)((uint32)(td0[sbox0[x >> (int)(24)]] ^ td1[sbox0[(uint32)(x >> (int)(16) & 255)]]) ^ td2[sbox0[(uint32)(x >> (int)(8) & 255)]]) ^ td3[sbox0[(uint32)(x & 255)]]);
            }
            dec[i + j] = x;
        }
    }
}

} // end aes_package
