// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// DO NOT EDIT.
// Generate with: go run gen.go -full -output md5block.go

// package md5 -- go2cs converted at 2020 August 29 08:30:47 UTC
// import "crypto/md5" ==> using md5 = go.crypto.md5_package
// Original source: C:\Go\src\crypto\md5\md5block.go
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace crypto
{
    public static unsafe partial class md5_package
    {
        private static readonly var x86 = runtime.GOARCH == "amd64" || runtime.GOARCH == "386";



        private static bool littleEndian = default;

        private static void init()
        {
            var x = uint32(0x04030201UL);
            array<byte> y = new array<byte>(new byte[] { 0x1, 0x2, 0x3, 0x4 });
            littleEndian = @unsafe.Pointer(ref x).Value == y;
        }

        private static void blockGeneric(ref digest dig, slice<byte> p)
        {
            var a = dig.s[0L];
            var b = dig.s[1L];
            var c = dig.s[2L];
            var d = dig.s[3L];
            ref array<uint> X = default;
            array<uint> xbuf = new array<uint>(16L);
            while (len(p) >= chunk)
            {
                var aa = a;
                var bb = b;
                var cc = c;
                var dd = d; 

                // This is a constant condition - it is not evaluated on each iteration.
                if (x86)
                { 
                    // MD5 was designed so that x86 processors can just iterate
                    // over the block data directly as uint32s, and we generate
                    // less code and run 1.3x faster if we take advantage of that.
                    // My apologies.
                    X = new ptr<ref array<uint>>(@unsafe.Pointer(ref p[0L]));
                }
                else if (littleEndian && uintptr(@unsafe.Pointer(ref p[0L])) & (@unsafe.Alignof(uint32(0L)) - 1L) == 0L)
                {
                    X = new ptr<ref array<uint>>(@unsafe.Pointer(ref p[0L]));
                }
                else
                {
                    X = ref xbuf;
                    long j = 0L;
                    for (long i = 0L; i < 16L; i++)
                    {
                        X[i & 15L] = uint32(p[j]) | uint32(p[j + 1L]) << (int)(8L) | uint32(p[j + 2L]) << (int)(16L) | uint32(p[j + 3L]) << (int)(24L);
                        j += 4L;
                    }

                } 

                // Round 1.
                a += (((c ^ d) & b) ^ d) + X[0L] + 3614090360L;
                a = a << (int)(7L) | a >> (int)((32L - 7L)) + b;

                d += (((b ^ c) & a) ^ c) + X[1L] + 3905402710L;
                d = d << (int)(12L) | d >> (int)((32L - 12L)) + a;

                c += (((a ^ b) & d) ^ b) + X[2L] + 606105819L;
                c = c << (int)(17L) | c >> (int)((32L - 17L)) + d;

                b += (((d ^ a) & c) ^ a) + X[3L] + 3250441966L;
                b = b << (int)(22L) | b >> (int)((32L - 22L)) + c;

                a += (((c ^ d) & b) ^ d) + X[4L] + 4118548399L;
                a = a << (int)(7L) | a >> (int)((32L - 7L)) + b;

                d += (((b ^ c) & a) ^ c) + X[5L] + 1200080426L;
                d = d << (int)(12L) | d >> (int)((32L - 12L)) + a;

                c += (((a ^ b) & d) ^ b) + X[6L] + 2821735955L;
                c = c << (int)(17L) | c >> (int)((32L - 17L)) + d;

                b += (((d ^ a) & c) ^ a) + X[7L] + 4249261313L;
                b = b << (int)(22L) | b >> (int)((32L - 22L)) + c;

                a += (((c ^ d) & b) ^ d) + X[8L] + 1770035416L;
                a = a << (int)(7L) | a >> (int)((32L - 7L)) + b;

                d += (((b ^ c) & a) ^ c) + X[9L] + 2336552879L;
                d = d << (int)(12L) | d >> (int)((32L - 12L)) + a;

                c += (((a ^ b) & d) ^ b) + X[10L] + 4294925233L;
                c = c << (int)(17L) | c >> (int)((32L - 17L)) + d;

                b += (((d ^ a) & c) ^ a) + X[11L] + 2304563134L;
                b = b << (int)(22L) | b >> (int)((32L - 22L)) + c;

                a += (((c ^ d) & b) ^ d) + X[12L] + 1804603682L;
                a = a << (int)(7L) | a >> (int)((32L - 7L)) + b;

                d += (((b ^ c) & a) ^ c) + X[13L] + 4254626195L;
                d = d << (int)(12L) | d >> (int)((32L - 12L)) + a;

                c += (((a ^ b) & d) ^ b) + X[14L] + 2792965006L;
                c = c << (int)(17L) | c >> (int)((32L - 17L)) + d;

                b += (((d ^ a) & c) ^ a) + X[15L] + 1236535329L;
                b = b << (int)(22L) | b >> (int)((32L - 22L)) + c; 

                // Round 2.

                a += (((b ^ c) & d) ^ c) + X[(1L + 5L * 0L) & 15L] + 4129170786L;
                a = a << (int)(5L) | a >> (int)((32L - 5L)) + b;

                d += (((a ^ b) & c) ^ b) + X[(1L + 5L * 1L) & 15L] + 3225465664L;
                d = d << (int)(9L) | d >> (int)((32L - 9L)) + a;

                c += (((d ^ a) & b) ^ a) + X[(1L + 5L * 2L) & 15L] + 643717713L;
                c = c << (int)(14L) | c >> (int)((32L - 14L)) + d;

                b += (((c ^ d) & a) ^ d) + X[(1L + 5L * 3L) & 15L] + 3921069994L;
                b = b << (int)(20L) | b >> (int)((32L - 20L)) + c;

                a += (((b ^ c) & d) ^ c) + X[(1L + 5L * 4L) & 15L] + 3593408605L;
                a = a << (int)(5L) | a >> (int)((32L - 5L)) + b;

                d += (((a ^ b) & c) ^ b) + X[(1L + 5L * 5L) & 15L] + 38016083L;
                d = d << (int)(9L) | d >> (int)((32L - 9L)) + a;

                c += (((d ^ a) & b) ^ a) + X[(1L + 5L * 6L) & 15L] + 3634488961L;
                c = c << (int)(14L) | c >> (int)((32L - 14L)) + d;

                b += (((c ^ d) & a) ^ d) + X[(1L + 5L * 7L) & 15L] + 3889429448L;
                b = b << (int)(20L) | b >> (int)((32L - 20L)) + c;

                a += (((b ^ c) & d) ^ c) + X[(1L + 5L * 8L) & 15L] + 568446438L;
                a = a << (int)(5L) | a >> (int)((32L - 5L)) + b;

                d += (((a ^ b) & c) ^ b) + X[(1L + 5L * 9L) & 15L] + 3275163606L;
                d = d << (int)(9L) | d >> (int)((32L - 9L)) + a;

                c += (((d ^ a) & b) ^ a) + X[(1L + 5L * 10L) & 15L] + 4107603335L;
                c = c << (int)(14L) | c >> (int)((32L - 14L)) + d;

                b += (((c ^ d) & a) ^ d) + X[(1L + 5L * 11L) & 15L] + 1163531501L;
                b = b << (int)(20L) | b >> (int)((32L - 20L)) + c;

                a += (((b ^ c) & d) ^ c) + X[(1L + 5L * 12L) & 15L] + 2850285829L;
                a = a << (int)(5L) | a >> (int)((32L - 5L)) + b;

                d += (((a ^ b) & c) ^ b) + X[(1L + 5L * 13L) & 15L] + 4243563512L;
                d = d << (int)(9L) | d >> (int)((32L - 9L)) + a;

                c += (((d ^ a) & b) ^ a) + X[(1L + 5L * 14L) & 15L] + 1735328473L;
                c = c << (int)(14L) | c >> (int)((32L - 14L)) + d;

                b += (((c ^ d) & a) ^ d) + X[(1L + 5L * 15L) & 15L] + 2368359562L;
                b = b << (int)(20L) | b >> (int)((32L - 20L)) + c; 

                // Round 3.

                a += (b ^ c ^ d) + X[(5L + 3L * 0L) & 15L] + 4294588738L;
                a = a << (int)(4L) | a >> (int)((32L - 4L)) + b;

                d += (a ^ b ^ c) + X[(5L + 3L * 1L) & 15L] + 2272392833L;
                d = d << (int)(11L) | d >> (int)((32L - 11L)) + a;

                c += (d ^ a ^ b) + X[(5L + 3L * 2L) & 15L] + 1839030562L;
                c = c << (int)(16L) | c >> (int)((32L - 16L)) + d;

                b += (c ^ d ^ a) + X[(5L + 3L * 3L) & 15L] + 4259657740L;
                b = b << (int)(23L) | b >> (int)((32L - 23L)) + c;

                a += (b ^ c ^ d) + X[(5L + 3L * 4L) & 15L] + 2763975236L;
                a = a << (int)(4L) | a >> (int)((32L - 4L)) + b;

                d += (a ^ b ^ c) + X[(5L + 3L * 5L) & 15L] + 1272893353L;
                d = d << (int)(11L) | d >> (int)((32L - 11L)) + a;

                c += (d ^ a ^ b) + X[(5L + 3L * 6L) & 15L] + 4139469664L;
                c = c << (int)(16L) | c >> (int)((32L - 16L)) + d;

                b += (c ^ d ^ a) + X[(5L + 3L * 7L) & 15L] + 3200236656L;
                b = b << (int)(23L) | b >> (int)((32L - 23L)) + c;

                a += (b ^ c ^ d) + X[(5L + 3L * 8L) & 15L] + 681279174L;
                a = a << (int)(4L) | a >> (int)((32L - 4L)) + b;

                d += (a ^ b ^ c) + X[(5L + 3L * 9L) & 15L] + 3936430074L;
                d = d << (int)(11L) | d >> (int)((32L - 11L)) + a;

                c += (d ^ a ^ b) + X[(5L + 3L * 10L) & 15L] + 3572445317L;
                c = c << (int)(16L) | c >> (int)((32L - 16L)) + d;

                b += (c ^ d ^ a) + X[(5L + 3L * 11L) & 15L] + 76029189L;
                b = b << (int)(23L) | b >> (int)((32L - 23L)) + c;

                a += (b ^ c ^ d) + X[(5L + 3L * 12L) & 15L] + 3654602809L;
                a = a << (int)(4L) | a >> (int)((32L - 4L)) + b;

                d += (a ^ b ^ c) + X[(5L + 3L * 13L) & 15L] + 3873151461L;
                d = d << (int)(11L) | d >> (int)((32L - 11L)) + a;

                c += (d ^ a ^ b) + X[(5L + 3L * 14L) & 15L] + 530742520L;
                c = c << (int)(16L) | c >> (int)((32L - 16L)) + d;

                b += (c ^ d ^ a) + X[(5L + 3L * 15L) & 15L] + 3299628645L;
                b = b << (int)(23L) | b >> (int)((32L - 23L)) + c; 

                // Round 4.

                a += (c ^ (b | ~d)) + X[(7L * 0L) & 15L] + 4096336452L;
                a = a << (int)(6L) | a >> (int)((32L - 6L)) + b;

                d += (b ^ (a | ~c)) + X[(7L * 1L) & 15L] + 1126891415L;
                d = d << (int)(10L) | d >> (int)((32L - 10L)) + a;

                c += (a ^ (d | ~b)) + X[(7L * 2L) & 15L] + 2878612391L;
                c = c << (int)(15L) | c >> (int)((32L - 15L)) + d;

                b += (d ^ (c | ~a)) + X[(7L * 3L) & 15L] + 4237533241L;
                b = b << (int)(21L) | b >> (int)((32L - 21L)) + c;

                a += (c ^ (b | ~d)) + X[(7L * 4L) & 15L] + 1700485571L;
                a = a << (int)(6L) | a >> (int)((32L - 6L)) + b;

                d += (b ^ (a | ~c)) + X[(7L * 5L) & 15L] + 2399980690L;
                d = d << (int)(10L) | d >> (int)((32L - 10L)) + a;

                c += (a ^ (d | ~b)) + X[(7L * 6L) & 15L] + 4293915773L;
                c = c << (int)(15L) | c >> (int)((32L - 15L)) + d;

                b += (d ^ (c | ~a)) + X[(7L * 7L) & 15L] + 2240044497L;
                b = b << (int)(21L) | b >> (int)((32L - 21L)) + c;

                a += (c ^ (b | ~d)) + X[(7L * 8L) & 15L] + 1873313359L;
                a = a << (int)(6L) | a >> (int)((32L - 6L)) + b;

                d += (b ^ (a | ~c)) + X[(7L * 9L) & 15L] + 4264355552L;
                d = d << (int)(10L) | d >> (int)((32L - 10L)) + a;

                c += (a ^ (d | ~b)) + X[(7L * 10L) & 15L] + 2734768916L;
                c = c << (int)(15L) | c >> (int)((32L - 15L)) + d;

                b += (d ^ (c | ~a)) + X[(7L * 11L) & 15L] + 1309151649L;
                b = b << (int)(21L) | b >> (int)((32L - 21L)) + c;

                a += (c ^ (b | ~d)) + X[(7L * 12L) & 15L] + 4149444226L;
                a = a << (int)(6L) | a >> (int)((32L - 6L)) + b;

                d += (b ^ (a | ~c)) + X[(7L * 13L) & 15L] + 3174756917L;
                d = d << (int)(10L) | d >> (int)((32L - 10L)) + a;

                c += (a ^ (d | ~b)) + X[(7L * 14L) & 15L] + 718787259L;
                c = c << (int)(15L) | c >> (int)((32L - 15L)) + d;

                b += (d ^ (c | ~a)) + X[(7L * 15L) & 15L] + 3951481745L;
                b = b << (int)(21L) | b >> (int)((32L - 21L)) + c;

                a += aa;
                b += bb;
                c += cc;
                d += dd;

                p = p[chunk..];
            }


            dig.s[0L] = a;
            dig.s[1L] = b;
            dig.s[2L] = c;
            dig.s[3L] = d;
        }
    }
}}
