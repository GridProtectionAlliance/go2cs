// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sha1 implements the SHA-1 hash algorithm as defined in RFC 3174.
//
// SHA-1 is cryptographically broken and should not be used for secure
// applications.
// package sha1 -- go2cs converted at 2020 October 08 03:36:41 UTC
// import "crypto/sha1" ==> using sha1 = go.crypto.sha1_package
// Original source: C:\Go\src\crypto\sha1\sha1.go
using crypto = go.crypto_package;
using binary = go.encoding.binary_package;
using errors = go.errors_package;
using hash = go.hash_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static partial class sha1_package
    {
        private static void init()
        {
            crypto.RegisterHash(crypto.SHA1, New);
        }

        // The size of a SHA-1 checksum in bytes.
        public static readonly long Size = (long)20L;

        // The blocksize of SHA-1 in bytes.


        // The blocksize of SHA-1 in bytes.
        public static readonly long BlockSize = (long)64L;



        private static readonly long chunk = (long)64L;
        private static readonly ulong init0 = (ulong)0x67452301UL;
        private static readonly ulong init1 = (ulong)0xEFCDAB89UL;
        private static readonly ulong init2 = (ulong)0x98BADCFEUL;
        private static readonly ulong init3 = (ulong)0x10325476UL;
        private static readonly ulong init4 = (ulong)0xC3D2E1F0UL;


        // digest represents the partial evaluation of a checksum.
        private partial struct digest
        {
            public array<uint> h;
            public array<byte> x;
            public long nx;
            public ulong len;
        }

        private static readonly @string magic = (@string)"sha\x01";
        private static readonly var marshaledSize = (var)len(magic) + 5L * 4L + chunk + 8L;


        private static (slice<byte>, error) MarshalBinary(this ptr<digest> _addr_d)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;
            ref digest d = ref _addr_d.val;

            var b = make_slice<byte>(0L, marshaledSize);
            b = append(b, magic);
            b = appendUint32(b, d.h[0L]);
            b = appendUint32(b, d.h[1L]);
            b = appendUint32(b, d.h[2L]);
            b = appendUint32(b, d.h[3L]);
            b = appendUint32(b, d.h[4L]);
            b = append(b, d.x[..d.nx]);
            b = b[..len(b) + len(d.x) - int(d.nx)]; // already zero
            b = appendUint64(b, d.len);
            return (b, error.As(null!)!);

        }

        private static error UnmarshalBinary(this ptr<digest> _addr_d, slice<byte> b)
        {
            ref digest d = ref _addr_d.val;

            if (len(b) < len(magic) || string(b[..len(magic)]) != magic)
            {
                return error.As(errors.New("crypto/sha1: invalid hash state identifier"))!;
            }

            if (len(b) != marshaledSize)
            {
                return error.As(errors.New("crypto/sha1: invalid hash state size"))!;
            }

            b = b[len(magic)..];
            b, d.h[0L] = consumeUint32(b);
            b, d.h[1L] = consumeUint32(b);
            b, d.h[2L] = consumeUint32(b);
            b, d.h[3L] = consumeUint32(b);
            b, d.h[4L] = consumeUint32(b);
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

            d.h[0L] = init0;
            d.h[1L] = init1;
            d.h[2L] = init2;
            d.h[3L] = init3;
            d.h[4L] = init4;
            d.nx = 0L;
            d.len = 0L;
        }

        // New returns a new hash.Hash computing the SHA1 checksum. The Hash also
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
            return append(in, hash[..]);

        }

        private static array<byte> checkSum(this ptr<digest> _addr_d) => func((_, panic, __) =>
        {
            ref digest d = ref _addr_d.val;

            var len = d.len; 
            // Padding.  Add a 1 bit and 0 bits until 56 bytes mod 64.
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

            return digest;

        });

        // ConstantTimeSum computes the same result of Sum() but in constant time
        private static slice<byte> ConstantTimeSum(this ptr<digest> _addr_d, slice<byte> @in)
        {
            ref digest d = ref _addr_d.val;

            var d0 = d.val;
            var hash = d0.constSum();
            return append(in, hash[..]);
        }

        private static array<byte> constSum(this ptr<digest> _addr_d)
        {
            ref digest d = ref _addr_d.val;

            array<byte> length = new array<byte>(8L);
            var l = d.len << (int)(3L);
            {
                var i__prev1 = i;

                for (var i = uint(0L); i < 8L; i++)
                {
                    length[i] = byte(l >> (int)((56L - 8L * i)));
                }


                i = i__prev1;
            }

            var nx = byte(d.nx);
            var t = nx - 56L; // if nx < 56 then the MSB of t is one
            var mask1b = byte(int8(t) >> (int)(7L)); // mask1b is 0xFF iff one block is enough

            var separator = byte(0x80UL); // gets reset to 0x00 once used
            {
                var i__prev1 = i;

                for (i = byte(0L); i < chunk; i++)
                {
                    var mask = byte(int8(i - nx) >> (int)(7L)); // 0x00 after the end of data

                    // if we reached the end of the data, replace with 0x80 or 0x00
                    d.x[i] = (~mask & separator) | (mask & d.x[i]); 

                    // zero the separator once used
                    separator &= mask;

                    if (i >= 56L)
                    { 
                        // we might have to write the length here if all fit in one block
                        d.x[i] |= mask1b & length[i - 56L];

                    }

                } 

                // compress, and only keep the digest if all fit in one block


                i = i__prev1;
            } 

            // compress, and only keep the digest if all fit in one block
            block(d, d.x[..]);

            array<byte> digest = new array<byte>(Size);
            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in d.h)
                {
                    i = __i;
                    s = __s;
                    digest[i * 4L] = mask1b & byte(s >> (int)(24L));
                    digest[i * 4L + 1L] = mask1b & byte(s >> (int)(16L));
                    digest[i * 4L + 2L] = mask1b & byte(s >> (int)(8L));
                    digest[i * 4L + 3L] = mask1b & byte(s);
                }

                i = i__prev1;
                s = s__prev1;
            }

            {
                var i__prev1 = i;

                for (i = byte(0L); i < chunk; i++)
                { 
                    // second block, it's always past the end of data, might start with 0x80
                    if (i < 56L)
                    {
                        d.x[i] = separator;
                        separator = 0L;
                    }
                    else
                    {
                        d.x[i] = length[i - 56L];
                    }

                } 

                // compress, and only keep the digest if we actually needed the second block


                i = i__prev1;
            } 

            // compress, and only keep the digest if we actually needed the second block
            block(d, d.x[..]);

            {
                var i__prev1 = i;
                var s__prev1 = s;

                foreach (var (__i, __s) in d.h)
                {
                    i = __i;
                    s = __s;
                    digest[i * 4L] |= ~mask1b & byte(s >> (int)(24L));
                    digest[i * 4L + 1L] |= ~mask1b & byte(s >> (int)(16L));
                    digest[i * 4L + 2L] |= ~mask1b & byte(s >> (int)(8L));
                    digest[i * 4L + 3L] |= ~mask1b & byte(s);
                }

                i = i__prev1;
                s = s__prev1;
            }

            return digest;

        }

        // Sum returns the SHA-1 checksum of the data.
        public static array<byte> Sum(slice<byte> data)
        {
            digest d = default;
            d.Reset();
            d.Write(data);
            return d.checkSum();
        }
    }
}}
