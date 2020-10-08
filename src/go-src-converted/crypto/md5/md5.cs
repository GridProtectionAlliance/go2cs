// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate go run gen.go -output md5block.go

// Package md5 implements the MD5 hash algorithm as defined in RFC 1321.
//
// MD5 is cryptographically broken and should not be used for secure
// applications.
// package md5 -- go2cs converted at 2020 October 08 03:36:38 UTC
// import "crypto/md5" ==> using md5 = go.crypto.md5_package
// Original source: C:\Go\src\crypto\md5\md5.go
using crypto = go.crypto_package;
using binary = go.encoding.binary_package;
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
        public static readonly long Size = (long)16L;

        // The blocksize of MD5 in bytes.


        // The blocksize of MD5 in bytes.
        public static readonly long BlockSize = (long)64L;



        private static readonly ulong init0 = (ulong)0x67452301UL;
        private static readonly ulong init1 = (ulong)0xEFCDAB89UL;
        private static readonly ulong init2 = (ulong)0x98BADCFEUL;
        private static readonly ulong init3 = (ulong)0x10325476UL;


        // digest represents the partial evaluation of a checksum.
        private partial struct digest
        {
            public array<uint> s;
            public array<byte> x;
            public long nx;
            public ulong len;
        }

        private static void Reset(this ptr<digest> _addr_d)
        {
            ref digest d = ref _addr_d.val;

            d.s[0L] = init0;
            d.s[1L] = init1;
            d.s[2L] = init2;
            d.s[3L] = init3;
            d.nx = 0L;
            d.len = 0L;
        }

        private static readonly @string magic = (@string)"md5\x01";
        private static readonly var marshaledSize = (var)len(magic) + 4L * 4L + BlockSize + 8L;


        private static (slice<byte>, error) MarshalBinary(this ptr<digest> _addr_d)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref digest d = ref _addr_d.val;

            var b = make_slice<byte>(0L, marshaledSize);
            b = append(b, magic);
            b = appendUint32(b, d.s[0L]);
            b = appendUint32(b, d.s[1L]);
            b = appendUint32(b, d.s[2L]);
            b = appendUint32(b, d.s[3L]);
            b = append(b, d.x[..d.nx]);
            b = b[..len(b) + len(d.x) - d.nx]; // already zero
            b = appendUint64(b, d.len);
            return (b, error.As(null!)!);

        }

        private static error UnmarshalBinary(this ptr<digest> _addr_d, slice<byte> b)
        {
            ref digest d = ref _addr_d.val;

            if (len(b) < len(magic) || string(b[..len(magic)]) != magic)
            {
                return error.As(errors.New("crypto/md5: invalid hash state identifier"))!;
            }

            if (len(b) != marshaledSize)
            {
                return error.As(errors.New("crypto/md5: invalid hash state size"))!;
            }

            b = b[len(magic)..];
            b, d.s[0L] = consumeUint32(b);
            b, d.s[1L] = consumeUint32(b);
            b, d.s[2L] = consumeUint32(b);
            b, d.s[3L] = consumeUint32(b);
            b = b[copy(d.x[..], b)..];
            b, d.len = consumeUint64(b);
            d.nx = int(d.len % BlockSize);
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

            return (b[8L..], binary.BigEndian.Uint64(b[0L..8L]));
        }

        private static (slice<byte>, uint) consumeUint32(slice<byte> b)
        {
            slice<byte> _p0 = default;
            uint _p0 = default;

            return (b[4L..], binary.BigEndian.Uint32(b[0L..4L]));
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

        private static long Size(this ptr<digest> _addr_d)
        {
            ref digest d = ref _addr_d.val;

            return Size;
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
 
            // Note that we currently call block or blockGeneric
            // directly (guarded using haveAsm) because this allows
            // escape analysis to see that p and d don't escape.
            nn = len(p);
            d.len += uint64(nn);
            if (d.nx > 0L)
            {
                var n = copy(d.x[d.nx..], p);
                d.nx += n;
                if (d.nx == BlockSize)
                {
                    if (haveAsm)
                    {
                        block(d, d.x[..]);
                    }
                    else
                    {
                        blockGeneric(d, d.x[..]);
                    }

                    d.nx = 0L;

                }

                p = p[n..];

            }

            if (len(p) >= BlockSize)
            {
                n = len(p) & ~(BlockSize - 1L);
                if (haveAsm)
                {
                    block(d, p[..n]);
                }
                else
                {
                    blockGeneric(d, p[..n]);
                }

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
            return append(in, hash[..]);

        }

        private static array<byte> checkSum(this ptr<digest> _addr_d) => func((_, panic, __) =>
        {
            ref digest d = ref _addr_d.val;
 
            // Append 0x80 to the end of the message and then append zeros
            // until the length is a multiple of 56 bytes. Finally append
            // 8 bytes representing the message length in bits.
            //
            // 1 byte end marker :: 0-63 padding bytes :: 8 byte length
            array<byte> tmp = new array<byte>(new byte[] { 0x80 });
            long pad = (55L - d.len) % 64L; // calculate number of padding bytes
            binary.LittleEndian.PutUint64(tmp[1L + pad..], d.len << (int)(3L)); // append length in bits
            d.Write(tmp[..1L + pad + 8L]); 

            // The previous write ensures that a whole number of
            // blocks (i.e. a multiple of 64 bytes) have been hashed.
            if (d.nx != 0L)
            {
                panic("d.nx != 0");
            }

            array<byte> digest = new array<byte>(Size);
            binary.LittleEndian.PutUint32(digest[0L..], d.s[0L]);
            binary.LittleEndian.PutUint32(digest[4L..], d.s[1L]);
            binary.LittleEndian.PutUint32(digest[8L..], d.s[2L]);
            binary.LittleEndian.PutUint32(digest[12L..], d.s[3L]);
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
