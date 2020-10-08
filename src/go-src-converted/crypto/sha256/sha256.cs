// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sha256 implements the SHA224 and SHA256 hash algorithms as defined
// in FIPS 180-4.
// package sha256 -- go2cs converted at 2020 October 08 03:36:43 UTC
// import "crypto/sha256" ==> using sha256 = go.crypto.sha256_package
// Original source: C:\Go\src\crypto\sha256\sha256.go
using crypto = go.crypto_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using hash = go.hash_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class sha256_package
    {
        private static void init()
        {
            crypto.RegisterHash(crypto.SHA224, New224);
            crypto.RegisterHash(crypto.SHA256, New);
        }

        // The size of a SHA256 checksum in bytes.
        public static readonly long Size = (long)32L;

        // The size of a SHA224 checksum in bytes.


        // The size of a SHA224 checksum in bytes.
        public static readonly long Size224 = (long)28L;

        // The blocksize of SHA256 and SHA224 in bytes.


        // The blocksize of SHA256 and SHA224 in bytes.
        public static readonly long BlockSize = (long)64L;



        private static readonly long chunk = (long)64L;
        private static readonly ulong init0 = (ulong)0x6A09E667UL;
        private static readonly ulong init1 = (ulong)0xBB67AE85UL;
        private static readonly ulong init2 = (ulong)0x3C6EF372UL;
        private static readonly ulong init3 = (ulong)0xA54FF53AUL;
        private static readonly ulong init4 = (ulong)0x510E527FUL;
        private static readonly ulong init5 = (ulong)0x9B05688CUL;
        private static readonly ulong init6 = (ulong)0x1F83D9ABUL;
        private static readonly ulong init7 = (ulong)0x5BE0CD19UL;
        private static readonly ulong init0_224 = (ulong)0xC1059ED8UL;
        private static readonly ulong init1_224 = (ulong)0x367CD507UL;
        private static readonly ulong init2_224 = (ulong)0x3070DD17UL;
        private static readonly ulong init3_224 = (ulong)0xF70E5939UL;
        private static readonly ulong init4_224 = (ulong)0xFFC00B31UL;
        private static readonly ulong init5_224 = (ulong)0x68581511UL;
        private static readonly ulong init6_224 = (ulong)0x64F98FA7UL;
        private static readonly ulong init7_224 = (ulong)0xBEFA4FA4UL;


        // digest represents the partial evaluation of a checksum.
        private partial struct digest
        {
            public array<uint> h;
            public array<byte> x;
            public long nx;
            public ulong len;
            public bool is224; // mark if this digest is SHA-224
        }

        private static readonly @string magic224 = (@string)"sha\x02";
        private static readonly @string magic256 = (@string)"sha\x03";
        private static readonly var marshaledSize = (var)len(magic256) + 8L * 4L + chunk + 8L;


        private static (slice<byte>, error) MarshalBinary(this ptr<digest> _addr_d)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref digest d = ref _addr_d.val;

            var b = make_slice<byte>(0L, marshaledSize);
            if (d.is224)
            {
                b = append(b, magic224);
            }
            else
            {
                b = append(b, magic256);
            }

            b = appendUint32(b, d.h[0L]);
            b = appendUint32(b, d.h[1L]);
            b = appendUint32(b, d.h[2L]);
            b = appendUint32(b, d.h[3L]);
            b = appendUint32(b, d.h[4L]);
            b = appendUint32(b, d.h[5L]);
            b = appendUint32(b, d.h[6L]);
            b = appendUint32(b, d.h[7L]);
            b = append(b, d.x[..d.nx]);
            b = b[..len(b) + len(d.x) - int(d.nx)]; // already zero
            b = appendUint64(b, d.len);
            return (b, error.As(null!)!);

        }

        private static error UnmarshalBinary(this ptr<digest> _addr_d, slice<byte> b)
        {
            ref digest d = ref _addr_d.val;

            if (len(b) < len(magic224) || (d.is224 && string(b[..len(magic224)]) != magic224) || (!d.is224 && string(b[..len(magic256)]) != magic256))
            {
                return error.As(errors.New("crypto/sha256: invalid hash state identifier"))!;
            }

            if (len(b) != marshaledSize)
            {
                return error.As(errors.New("crypto/sha256: invalid hash state size"))!;
            }

            b = b[len(magic224)..];
            b, d.h[0L] = consumeUint32(b);
            b, d.h[1L] = consumeUint32(b);
            b, d.h[2L] = consumeUint32(b);
            b, d.h[3L] = consumeUint32(b);
            b, d.h[4L] = consumeUint32(b);
            b, d.h[5L] = consumeUint32(b);
            b, d.h[6L] = consumeUint32(b);
            b, d.h[7L] = consumeUint32(b);
            b = b[copy(d.x[..], b)..];
            b, d.len = consumeUint64(b);
            d.nx = int(d.len % chunk);
            return error.As(null!)!;

        }

        private static slice<byte> appendUint64(slice<byte> b, ulong x)
        {
            array<byte> a = new array<byte>(8L);
            binary.BigEndian.PutUint64(a[..], x);
            return append(b, a[..]);
        }

        private static slice<byte> appendUint32(slice<byte> b, uint x)
        {
            array<byte> a = new array<byte>(4L);
            binary.BigEndian.PutUint32(a[..], x);
            return append(b, a[..]);
        }

        private static (slice<byte>, ulong) consumeUint64(slice<byte> b)
        {
            slice<byte> _p0 = default;
            ulong _p0 = default;

            _ = b[7L];
            var x = uint64(b[7L]) | uint64(b[6L]) << (int)(8L) | uint64(b[5L]) << (int)(16L) | uint64(b[4L]) << (int)(24L) | uint64(b[3L]) << (int)(32L) | uint64(b[2L]) << (int)(40L) | uint64(b[1L]) << (int)(48L) | uint64(b[0L]) << (int)(56L);
            return (b[8L..], x);
        }

        private static (slice<byte>, uint) consumeUint32(slice<byte> b)
        {
            slice<byte> _p0 = default;
            uint _p0 = default;

            _ = b[3L];
            var x = uint32(b[3L]) | uint32(b[2L]) << (int)(8L) | uint32(b[1L]) << (int)(16L) | uint32(b[0L]) << (int)(24L);
            return (b[4L..], x);
        }

        private static void Reset(this ptr<digest> _addr_d)
        {
            ref digest d = ref _addr_d.val;

            if (!d.is224)
            {
                d.h[0L] = init0;
                d.h[1L] = init1;
                d.h[2L] = init2;
                d.h[3L] = init3;
                d.h[4L] = init4;
                d.h[5L] = init5;
                d.h[6L] = init6;
                d.h[7L] = init7;
            }
            else
            {
                d.h[0L] = init0_224;
                d.h[1L] = init1_224;
                d.h[2L] = init2_224;
                d.h[3L] = init3_224;
                d.h[4L] = init4_224;
                d.h[5L] = init5_224;
                d.h[6L] = init6_224;
                d.h[7L] = init7_224;
            }

            d.nx = 0L;
            d.len = 0L;

        }

        // New returns a new hash.Hash computing the SHA256 checksum. The Hash
        // also implements encoding.BinaryMarshaler and
        // encoding.BinaryUnmarshaler to marshal and unmarshal the internal
        // state of the hash.
        public static hash.Hash New()
        {
            ptr<digest> d = @new<digest>();
            d.Reset();
            return d;
        }

        // New224 returns a new hash.Hash computing the SHA224 checksum.
        public static hash.Hash New224()
        {
            ptr<digest> d = @new<digest>();
            d.is224 = true;
            d.Reset();
            return d;
        }

        private static long Size(this ptr<digest> _addr_d)
        {
            ref digest d = ref _addr_d.val;

            if (!d.is224)
            {
                return Size;
            }

            return Size224;

        }

        private static long BlockSize(this ptr<digest> _addr_d)
        {
            ref digest d = ref _addr_d.val;

            return BlockSize;
        }

        private static (long, error) Write(this ptr<digest> _addr_d, slice<byte> p)
        {
            long nn = default;
            error err = default!;
            ref digest d = ref _addr_d.val;

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

            return ;

        }

        private static slice<byte> Sum(this ptr<digest> _addr_d, slice<byte> @in)
        {
            ref digest d = ref _addr_d.val;
 
            // Make a copy of d so that caller can keep writing and summing.
            var d0 = d.val;
            var hash = d0.checkSum();
            if (d0.is224)
            {
                return append(in, hash[..Size224]);
            }

            return append(in, hash[..]);

        }

        private static array<byte> checkSum(this ptr<digest> _addr_d) => func((_, panic, __) =>
        {
            ref digest d = ref _addr_d.val;

            var len = d.len; 
            // Padding. Add a 1 bit and 0 bits until 56 bytes mod 64.
            array<byte> tmp = new array<byte>(64L);
            tmp[0L] = 0x80UL;
            if (len % 64L < 56L)
            {
                d.Write(tmp[0L..56L - len % 64L]);
            }
            else
            {
                d.Write(tmp[0L..64L + 56L - len % 64L]);
            } 

            // Length in bits.
            len <<= 3L;
            binary.BigEndian.PutUint64(tmp[..], len);
            d.Write(tmp[0L..8L]);

            if (d.nx != 0L)
            {
                panic("d.nx != 0");
            }

            array<byte> digest = new array<byte>(Size);

            binary.BigEndian.PutUint32(digest[0L..], d.h[0L]);
            binary.BigEndian.PutUint32(digest[4L..], d.h[1L]);
            binary.BigEndian.PutUint32(digest[8L..], d.h[2L]);
            binary.BigEndian.PutUint32(digest[12L..], d.h[3L]);
            binary.BigEndian.PutUint32(digest[16L..], d.h[4L]);
            binary.BigEndian.PutUint32(digest[20L..], d.h[5L]);
            binary.BigEndian.PutUint32(digest[24L..], d.h[6L]);
            if (!d.is224)
            {
                binary.BigEndian.PutUint32(digest[28L..], d.h[7L]);
            }

            return digest;

        });

        // Sum256 returns the SHA256 checksum of the data.
        public static array<byte> Sum256(slice<byte> data)
        {
            digest d = default;
            d.Reset();
            d.Write(data);
            return d.checkSum();
        }

        // Sum224 returns the SHA224 checksum of the data.
        public static array<byte> Sum224(slice<byte> data)
        {
            array<byte> sum224 = default;

            digest d = default;
            d.is224 = true;
            d.Reset();
            d.Write(data);
            var sum = d.checkSum();
            copy(sum224[..], sum[..Size224]);
            return ;
        }
    }
}}
