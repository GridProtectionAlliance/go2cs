// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sha512 implements the SHA-384, SHA-512, SHA-512/224, and SHA-512/256
// hash algorithms as defined in FIPS 180-4.
//
// All the hash.Hash implementations returned by this package also
// implement encoding.BinaryMarshaler and encoding.BinaryUnmarshaler to
// marshal and unmarshal the internal state of the hash.
// package sha512 -- go2cs converted at 2020 August 29 08:29:41 UTC
// import "crypto/sha512" ==> using sha512 = go.crypto.sha512_package
// Original source: C:\Go\src\crypto\sha512\sha512.go
using crypto = go.crypto_package;
using errors = go.errors_package;
using hash = go.hash_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class sha512_package
    {
        private static void init()
        {
            crypto.RegisterHash(crypto.SHA384, New384);
            crypto.RegisterHash(crypto.SHA512, New);
            crypto.RegisterHash(crypto.SHA512_224, New512_224);
            crypto.RegisterHash(crypto.SHA512_256, New512_256);
        }

 
        // Size is the size, in bytes, of a SHA-512 checksum.
        public static readonly long Size = 64L; 

        // Size224 is the size, in bytes, of a SHA-512/224 checksum.
        public static readonly long Size224 = 28L; 

        // Size256 is the size, in bytes, of a SHA-512/256 checksum.
        public static readonly long Size256 = 32L; 

        // Size384 is the size, in bytes, of a SHA-384 checksum.
        public static readonly long Size384 = 48L; 

        // BlockSize is the block size, in bytes, of the SHA-512/224,
        // SHA-512/256, SHA-384 and SHA-512 hash functions.
        public static readonly long BlockSize = 128L;

        private static readonly long chunk = 128L;
        private static readonly ulong init0 = 0x6a09e667f3bcc908UL;
        private static readonly ulong init1 = 0xbb67ae8584caa73bUL;
        private static readonly ulong init2 = 0x3c6ef372fe94f82bUL;
        private static readonly ulong init3 = 0xa54ff53a5f1d36f1UL;
        private static readonly ulong init4 = 0x510e527fade682d1UL;
        private static readonly ulong init5 = 0x9b05688c2b3e6c1fUL;
        private static readonly ulong init6 = 0x1f83d9abfb41bd6bUL;
        private static readonly ulong init7 = 0x5be0cd19137e2179UL;
        private static readonly ulong init0_224 = 0x8c3d37c819544da2UL;
        private static readonly ulong init1_224 = 0x73e1996689dcd4d6UL;
        private static readonly ulong init2_224 = 0x1dfab7ae32ff9c82UL;
        private static readonly ulong init3_224 = 0x679dd514582f9fcfUL;
        private static readonly ulong init4_224 = 0x0f6d2b697bd44da8UL;
        private static readonly ulong init5_224 = 0x77e36f7304c48942UL;
        private static readonly ulong init6_224 = 0x3f9d85a86a1d36c8UL;
        private static readonly ulong init7_224 = 0x1112e6ad91d692a1UL;
        private static readonly ulong init0_256 = 0x22312194fc2bf72cUL;
        private static readonly ulong init1_256 = 0x9f555fa3c84c64c2UL;
        private static readonly ulong init2_256 = 0x2393b86b6f53b151UL;
        private static readonly ulong init3_256 = 0x963877195940eabdUL;
        private static readonly ulong init4_256 = 0x96283ee2a88effe3UL;
        private static readonly ulong init5_256 = 0xbe5e1e2553863992UL;
        private static readonly ulong init6_256 = 0x2b0199fc2c85b8aaUL;
        private static readonly ulong init7_256 = 0x0eb72ddc81c52ca2UL;
        private static readonly ulong init0_384 = 0xcbbb9d5dc1059ed8UL;
        private static readonly ulong init1_384 = 0x629a292a367cd507UL;
        private static readonly ulong init2_384 = 0x9159015a3070dd17UL;
        private static readonly ulong init3_384 = 0x152fecd8f70e5939UL;
        private static readonly ulong init4_384 = 0x67332667ffc00b31UL;
        private static readonly ulong init5_384 = 0x8eb44a8768581511UL;
        private static readonly ulong init6_384 = 0xdb0c2e0d64f98fa7UL;
        private static readonly ulong init7_384 = 0x47b5481dbefa4fa4UL;

        // digest represents the partial evaluation of a checksum.
        private partial struct digest
        {
            public array<ulong> h;
            public array<byte> x;
            public long nx;
            public ulong len;
            public crypto.Hash function;
        }

        private static void Reset(this ref digest d)
        {

            if (d.function == crypto.SHA384) 
                d.h[0L] = init0_384;
                d.h[1L] = init1_384;
                d.h[2L] = init2_384;
                d.h[3L] = init3_384;
                d.h[4L] = init4_384;
                d.h[5L] = init5_384;
                d.h[6L] = init6_384;
                d.h[7L] = init7_384;
            else if (d.function == crypto.SHA512_224) 
                d.h[0L] = init0_224;
                d.h[1L] = init1_224;
                d.h[2L] = init2_224;
                d.h[3L] = init3_224;
                d.h[4L] = init4_224;
                d.h[5L] = init5_224;
                d.h[6L] = init6_224;
                d.h[7L] = init7_224;
            else if (d.function == crypto.SHA512_256) 
                d.h[0L] = init0_256;
                d.h[1L] = init1_256;
                d.h[2L] = init2_256;
                d.h[3L] = init3_256;
                d.h[4L] = init4_256;
                d.h[5L] = init5_256;
                d.h[6L] = init6_256;
                d.h[7L] = init7_256;
            else 
                d.h[0L] = init0;
                d.h[1L] = init1;
                d.h[2L] = init2;
                d.h[3L] = init3;
                d.h[4L] = init4;
                d.h[5L] = init5;
                d.h[6L] = init6;
                d.h[7L] = init7;
                        d.nx = 0L;
            d.len = 0L;
        }

        private static readonly @string magic384 = "sha\x04";
        private static readonly @string magic512_224 = "sha\x05";
        private static readonly @string magic512_256 = "sha\x06";
        private static readonly @string magic512 = "sha\x07";
        private static readonly var marshaledSize = len(magic512) + 8L * 8L + chunk + 8L;

        private static (slice<byte>, error) MarshalBinary(this ref digest d)
        {
            var b = make_slice<byte>(0L, marshaledSize);

            if (d.function == crypto.SHA384) 
                b = append(b, magic384);
            else if (d.function == crypto.SHA512_224) 
                b = append(b, magic512_224);
            else if (d.function == crypto.SHA512_256) 
                b = append(b, magic512_256);
            else if (d.function == crypto.SHA512) 
                b = append(b, magic512);
            else 
                return (null, errors.New("crypto/sha512: invalid hash function"));
                        b = appendUint64(b, d.h[0L]);
            b = appendUint64(b, d.h[1L]);
            b = appendUint64(b, d.h[2L]);
            b = appendUint64(b, d.h[3L]);
            b = appendUint64(b, d.h[4L]);
            b = appendUint64(b, d.h[5L]);
            b = appendUint64(b, d.h[6L]);
            b = appendUint64(b, d.h[7L]);
            b = append(b, d.x[..d.nx]);
            b = b[..len(b) + len(d.x) - int(d.nx)]; // already zero
            b = appendUint64(b, d.len);
            return (b, null);
        }

        private static error UnmarshalBinary(this ref digest d, slice<byte> b)
        {
            if (len(b) < len(magic512))
            {
                return error.As(errors.New("crypto/sha512: invalid hash state identifier"));
            }

            if (d.function == crypto.SHA384 && string(b[..len(magic384)]) == magic384)             else if (d.function == crypto.SHA512_224 && string(b[..len(magic512_224)]) == magic512_224)             else if (d.function == crypto.SHA512_256 && string(b[..len(magic512_256)]) == magic512_256)             else if (d.function == crypto.SHA512 && string(b[..len(magic512)]) == magic512)             else 
                return error.As(errors.New("crypto/sha512: invalid hash state identifier"));
                        if (len(b) != marshaledSize)
            {
                return error.As(errors.New("crypto/sha512: invalid hash state size"));
            }
            b = b[len(magic512)..];
            b, d.h[0L] = consumeUint64(b);
            b, d.h[1L] = consumeUint64(b);
            b, d.h[2L] = consumeUint64(b);
            b, d.h[3L] = consumeUint64(b);
            b, d.h[4L] = consumeUint64(b);
            b, d.h[5L] = consumeUint64(b);
            b, d.h[6L] = consumeUint64(b);
            b, d.h[7L] = consumeUint64(b);
            b = b[copy(d.x[..], b)..];
            b, d.len = consumeUint64(b);
            d.nx = int(d.len) % chunk;
            return error.As(null);
        }

        private static slice<byte> appendUint64(slice<byte> b, ulong x)
        {
            array<byte> a = new array<byte>(new byte[] { byte(x>>56), byte(x>>48), byte(x>>40), byte(x>>32), byte(x>>24), byte(x>>16), byte(x>>8), byte(x) });
            return append(b, a[..]);
        }

        private static (slice<byte>, ulong) consumeUint64(slice<byte> b)
        {
            _ = b[7L];
            var x = uint64(b[7L]) | uint64(b[6L]) << (int)(8L) | uint64(b[5L]) << (int)(16L) | uint64(b[4L]) << (int)(24L) | uint64(b[3L]) << (int)(32L) | uint64(b[2L]) << (int)(40L) | uint64(b[1L]) << (int)(48L) | uint64(b[0L]) << (int)(56L);
            return (b[8L..], x);
        }

        // New returns a new hash.Hash computing the SHA-512 checksum.
        public static hash.Hash New()
        {
            digest d = ref new digest(function:crypto.SHA512);
            d.Reset();
            return d;
        }

        // New512_224 returns a new hash.Hash computing the SHA-512/224 checksum.
        public static hash.Hash New512_224()
        {
            digest d = ref new digest(function:crypto.SHA512_224);
            d.Reset();
            return d;
        }

        // New512_256 returns a new hash.Hash computing the SHA-512/256 checksum.
        public static hash.Hash New512_256()
        {
            digest d = ref new digest(function:crypto.SHA512_256);
            d.Reset();
            return d;
        }

        // New384 returns a new hash.Hash computing the SHA-384 checksum.
        public static hash.Hash New384()
        {
            digest d = ref new digest(function:crypto.SHA384);
            d.Reset();
            return d;
        }

        private static long Size(this ref digest d)
        {

            if (d.function == crypto.SHA512_224) 
                return Size224;
            else if (d.function == crypto.SHA512_256) 
                return Size256;
            else if (d.function == crypto.SHA384) 
                return Size384;
            else 
                return Size;
                    }

        private static long BlockSize(this ref digest d)
        {
            return BlockSize;
        }

        private static (long, error) Write(this ref digest d, slice<byte> p)
        {
            nn = len(p);
            d.len += uint64(nn);
            if (d.nx > 0L)
            {
                var n = copy(d.x[d.nx..], p);
                d.nx += n;
                if (d.nx == chunk)
                {
                    block(d, d.x[..]);
                    d.nx = 0L;
                }
                p = p[n..];
            }
            if (len(p) >= chunk)
            {
                n = len(p) & ~(chunk - 1L);
                block(d, p[..n]);
                p = p[n..];
            }
            if (len(p) > 0L)
            {
                d.nx = copy(d.x[..], p);
            }
            return;
        }

        private static slice<byte> Sum(this ref digest d0, slice<byte> @in)
        { 
            // Make a copy of d0 so that caller can keep writing and summing.
            ptr<digest> d = @new<digest>();
            d.Value = d0.Value;
            var hash = d.checkSum();

            if (d.function == crypto.SHA384) 
                return append(in, hash[..Size384]);
            else if (d.function == crypto.SHA512_224) 
                return append(in, hash[..Size224]);
            else if (d.function == crypto.SHA512_256) 
                return append(in, hash[..Size256]);
            else 
                return append(in, hash[..]);
                    }

        private static array<byte> checkSum(this ref digest _d) => func(_d, (ref digest d, Defer _, Panic panic, Recover __) =>
        { 
            // Padding. Add a 1 bit and 0 bits until 112 bytes mod 128.
            var len = d.len;
            array<byte> tmp = new array<byte>(128L);
            tmp[0L] = 0x80UL;
            if (len % 128L < 112L)
            {
                d.Write(tmp[0L..112L - len % 128L]);
            }
            else
            {
                d.Write(tmp[0L..128L + 112L - len % 128L]);
            } 

            // Length in bits.
            len <<= 3L;
            {
                var i__prev1 = i;

                for (var i = uint(0L); i < 16L; i++)
                {
                    tmp[i] = byte(len >> (int)((120L - 8L * i)));
                }


                i = i__prev1;
            }
            d.Write(tmp[0L..16L]);

            if (d.nx != 0L)
            {
                panic("d.nx != 0");
            }
            var h = d.h[..];
            if (d.function == crypto.SHA384)
            {
                h = d.h[..6L];
            }
            array<byte> digest = new array<byte>(Size);
            {
                var i__prev1 = i;

                foreach (var (__i, __s) in h)
                {
                    i = __i;
                    s = __s;
                    digest[i * 8L] = byte(s >> (int)(56L));
                    digest[i * 8L + 1L] = byte(s >> (int)(48L));
                    digest[i * 8L + 2L] = byte(s >> (int)(40L));
                    digest[i * 8L + 3L] = byte(s >> (int)(32L));
                    digest[i * 8L + 4L] = byte(s >> (int)(24L));
                    digest[i * 8L + 5L] = byte(s >> (int)(16L));
                    digest[i * 8L + 6L] = byte(s >> (int)(8L));
                    digest[i * 8L + 7L] = byte(s);
                }

                i = i__prev1;
            }

            return digest;
        });

        // Sum512 returns the SHA512 checksum of the data.
        public static array<byte> Sum512(slice<byte> data)
        {
            digest d = new digest(function:crypto.SHA512);
            d.Reset();
            d.Write(data);
            return d.checkSum();
        }

        // Sum384 returns the SHA384 checksum of the data.
        public static array<byte> Sum384(slice<byte> data)
        {
            digest d = new digest(function:crypto.SHA384);
            d.Reset();
            d.Write(data);
            var sum = d.checkSum();
            copy(sum384[..], sum[..Size384]);
            return;
        }

        // Sum512_224 returns the Sum512/224 checksum of the data.
        public static array<byte> Sum512_224(slice<byte> data)
        {
            digest d = new digest(function:crypto.SHA512_224);
            d.Reset();
            d.Write(data);
            var sum = d.checkSum();
            copy(sum224[..], sum[..Size224]);
            return;
        }

        // Sum512_256 returns the Sum512/256 checksum of the data.
        public static array<byte> Sum512_256(slice<byte> data)
        {
            digest d = new digest(function:crypto.SHA512_256);
            d.Reset();
            d.Write(data);
            var sum = d.checkSum();
            copy(sum256[..], sum[..Size256]);
            return;
        }
    }
}}
