// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate go run gen.go -full -output md5block.go

// Package md5 implements the MD5 hash algorithm as defined in RFC 1321.
//
// MD5 is cryptographically broken and should not be used for secure
// applications.
// package md5 -- go2cs converted at 2020 August 29 08:30:45 UTC
// import "crypto/md5" ==> using md5 = go.crypto.md5_package
// Original source: C:\Go\src\crypto\md5\md5.go
using crypto = go.crypto_package;
using errors = go.errors_package;
using hash = go.hash_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class md5_package
    {
        private static void init()
        {
            crypto.RegisterHash(crypto.MD5, New);
        }

        // The size of an MD5 checksum in bytes.
        public static readonly long Size = 16L;

        // The blocksize of MD5 in bytes.


        // The blocksize of MD5 in bytes.
        public static readonly long BlockSize = 64L;



        private static readonly long chunk = 64L;
        private static readonly ulong init0 = 0x67452301UL;
        private static readonly ulong init1 = 0xEFCDAB89UL;
        private static readonly ulong init2 = 0x98BADCFEUL;
        private static readonly ulong init3 = 0x10325476UL;

        // digest represents the partial evaluation of a checksum.
        private partial struct digest
        {
            public array<uint> s;
            public array<byte> x;
            public long nx;
            public ulong len;
        }

        private static void Reset(this ref digest d)
        {
            d.s[0L] = init0;
            d.s[1L] = init1;
            d.s[2L] = init2;
            d.s[3L] = init3;
            d.nx = 0L;
            d.len = 0L;
        }

        private static readonly @string magic = "md5\x01";
        private static readonly var marshaledSize = len(magic) + 4L * 4L + chunk + 8L;

        private static (slice<byte>, error) MarshalBinary(this ref digest d)
        {
            var b = make_slice<byte>(0L, marshaledSize);
            b = append(b, magic);
            b = appendUint32(b, d.s[0L]);
            b = appendUint32(b, d.s[1L]);
            b = appendUint32(b, d.s[2L]);
            b = appendUint32(b, d.s[3L]);
            b = append(b, d.x[..d.nx]);
            b = b[..len(b) + len(d.x) - int(d.nx)]; // already zero
            b = appendUint64(b, d.len);
            return (b, null);
        }

        private static error UnmarshalBinary(this ref digest d, slice<byte> b)
        {
            if (len(b) < len(magic) || string(b[..len(magic)]) != magic)
            {
                return error.As(errors.New("crypto/md5: invalid hash state identifier"));
            }
            if (len(b) != marshaledSize)
            {
                return error.As(errors.New("crypto/md5: invalid hash state size"));
            }
            b = b[len(magic)..];
            b, d.s[0L] = consumeUint32(b);
            b, d.s[1L] = consumeUint32(b);
            b, d.s[2L] = consumeUint32(b);
            b, d.s[3L] = consumeUint32(b);
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

        private static slice<byte> appendUint32(slice<byte> b, uint x)
        {
            array<byte> a = new array<byte>(new byte[] { byte(x>>24), byte(x>>16), byte(x>>8), byte(x) });
            return append(b, a[..]);
        }

        private static (slice<byte>, ulong) consumeUint64(slice<byte> b)
        {
            _ = b[7L];
            var x = uint64(b[7L]) | uint64(b[6L]) << (int)(8L) | uint64(b[5L]) << (int)(16L) | uint64(b[4L]) << (int)(24L) | uint64(b[3L]) << (int)(32L) | uint64(b[2L]) << (int)(40L) | uint64(b[1L]) << (int)(48L) | uint64(b[0L]) << (int)(56L);
            return (b[8L..], x);
        }

        private static (slice<byte>, uint) consumeUint32(slice<byte> b)
        {
            _ = b[3L];
            var x = uint32(b[3L]) | uint32(b[2L]) << (int)(8L) | uint32(b[1L]) << (int)(16L) | uint32(b[0L]) << (int)(24L);
            return (b[4L..], x);
        }

        // New returns a new hash.Hash computing the MD5 checksum. The Hash also
        // implements encoding.BinaryMarshaler and encoding.BinaryUnmarshaler to
        // marshal and unmarshal the internal state of the hash.
        public static hash.Hash New()
        {
            ptr<digest> d = @new<digest>();
            d.Reset();
            return d;
        }

        private static long Size(this ref digest d)
        {
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
            var d = d0.Value;
            var hash = d.checkSum();
            return append(in, hash[..]);
        }

        private static array<byte> checkSum(this ref digest _d) => func(_d, (ref digest d, Defer _, Panic panic, Recover __) =>
        { 
            // Padding. Add a 1 bit and 0 bits until 56 bytes mod 64.
            var len = d.len;
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
            {
                var i__prev1 = i;

                for (var i = uint(0L); i < 8L; i++)
                {
                    tmp[i] = byte(len >> (int)((8L * i)));
                }


                i = i__prev1;
            }
            d.Write(tmp[0L..8L]);

            if (d.nx != 0L)
            {
                panic("d.nx != 0");
            }
            array<byte> digest = new array<byte>(Size);
            {
                var i__prev1 = i;

                foreach (var (__i, __s) in d.s)
                {
                    i = __i;
                    s = __s;
                    digest[i * 4L] = byte(s);
                    digest[i * 4L + 1L] = byte(s >> (int)(8L));
                    digest[i * 4L + 2L] = byte(s >> (int)(16L));
                    digest[i * 4L + 3L] = byte(s >> (int)(24L));
                }

                i = i__prev1;
            }

            return digest;
        });

        // Sum returns the MD5 checksum of the data.
        public static array<byte> Sum(slice<byte> data)
        {
            digest d = default;
            d.Reset();
            d.Write(data);
            return d.checkSum();
        }
    }
}}
