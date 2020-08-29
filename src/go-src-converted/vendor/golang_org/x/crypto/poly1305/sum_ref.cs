// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64,!arm gccgo appengine nacl

// package poly1305 -- go2cs converted at 2020 August 29 10:11:38 UTC
// import "vendor/golang_org/x/crypto/poly1305" ==> using poly1305 = go.vendor.golang_org.x.crypto.poly1305_package
// Original source: C:\Go\src\vendor\golang_org\x\crypto\poly1305\sum_ref.go
using binary = go.encoding.binary_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace crypto
{
    public static partial class poly1305_package
    {
        // Sum generates an authenticator for msg using a one-time key and puts the
        // 16-byte result into out. Authenticating two different messages with the same
        // key allows an attacker to forge messages at will.
        public static void Sum(ref array<byte> @out, slice<byte> msg, ref array<byte> key)
        {
            uint h0 = default;            uint h1 = default;            uint h2 = default;            uint h3 = default;            uint h4 = default; // the hash accumulators
            ulong r0 = default;            ulong r1 = default;            ulong r2 = default;            ulong r3 = default;            ulong r4 = default; // the r part of the key

            r0 = uint64(binary.LittleEndian.Uint32(key[0L..]) & 0x3ffffffUL);
            r1 = uint64((binary.LittleEndian.Uint32(key[3L..]) >> (int)(2L)) & 0x3ffff03UL);
            r2 = uint64((binary.LittleEndian.Uint32(key[6L..]) >> (int)(4L)) & 0x3ffc0ffUL);
            r3 = uint64((binary.LittleEndian.Uint32(key[9L..]) >> (int)(6L)) & 0x3f03fffUL);
            r4 = uint64((binary.LittleEndian.Uint32(key[12L..]) >> (int)(8L)) & 0x00fffffUL);

            var R1 = r1 * 5L;
            var R2 = r2 * 5L;
            var R3 = r3 * 5L;
            var R4 = r4 * 5L;

            while (len(msg) >= TagSize)
            { 
                // h += msg
                h0 += binary.LittleEndian.Uint32(msg[0L..]) & 0x3ffffffUL;
                h1 += (binary.LittleEndian.Uint32(msg[3L..]) >> (int)(2L)) & 0x3ffffffUL;
                h2 += (binary.LittleEndian.Uint32(msg[6L..]) >> (int)(4L)) & 0x3ffffffUL;
                h3 += (binary.LittleEndian.Uint32(msg[9L..]) >> (int)(6L)) & 0x3ffffffUL;
                h4 += (binary.LittleEndian.Uint32(msg[12L..]) >> (int)(8L)) | (1L << (int)(24L)); 

                // h *= r
                var d0 = (uint64(h0) * r0) + (uint64(h1) * R4) + (uint64(h2) * R3) + (uint64(h3) * R2) + (uint64(h4) * R1);
                var d1 = (d0 >> (int)(26L)) + (uint64(h0) * r1) + (uint64(h1) * r0) + (uint64(h2) * R4) + (uint64(h3) * R3) + (uint64(h4) * R2);
                var d2 = (d1 >> (int)(26L)) + (uint64(h0) * r2) + (uint64(h1) * r1) + (uint64(h2) * r0) + (uint64(h3) * R4) + (uint64(h4) * R3);
                var d3 = (d2 >> (int)(26L)) + (uint64(h0) * r3) + (uint64(h1) * r2) + (uint64(h2) * r1) + (uint64(h3) * r0) + (uint64(h4) * R4);
                var d4 = (d3 >> (int)(26L)) + (uint64(h0) * r4) + (uint64(h1) * r3) + (uint64(h2) * r2) + (uint64(h3) * r1) + (uint64(h4) * r0); 

                // h %= p
                h0 = uint32(d0) & 0x3ffffffUL;
                h1 = uint32(d1) & 0x3ffffffUL;
                h2 = uint32(d2) & 0x3ffffffUL;
                h3 = uint32(d3) & 0x3ffffffUL;
                h4 = uint32(d4) & 0x3ffffffUL;

                h0 += uint32(d4 >> (int)(26L)) * 5L;
                h1 += h0 >> (int)(26L);
                h0 = h0 & 0x3ffffffUL;

                msg = msg[TagSize..];
            }

            if (len(msg) > 0L)
            {
                array<byte> block = new array<byte>(TagSize);
                var off = copy(block[..], msg);
                block[off] = 0x01UL; 

                // h += msg
                h0 += binary.LittleEndian.Uint32(block[0L..]) & 0x3ffffffUL;
                h1 += (binary.LittleEndian.Uint32(block[3L..]) >> (int)(2L)) & 0x3ffffffUL;
                h2 += (binary.LittleEndian.Uint32(block[6L..]) >> (int)(4L)) & 0x3ffffffUL;
                h3 += (binary.LittleEndian.Uint32(block[9L..]) >> (int)(6L)) & 0x3ffffffUL;
                h4 += (binary.LittleEndian.Uint32(block[12L..]) >> (int)(8L)); 

                // h *= r
                d0 = (uint64(h0) * r0) + (uint64(h1) * R4) + (uint64(h2) * R3) + (uint64(h3) * R2) + (uint64(h4) * R1);
                d1 = (d0 >> (int)(26L)) + (uint64(h0) * r1) + (uint64(h1) * r0) + (uint64(h2) * R4) + (uint64(h3) * R3) + (uint64(h4) * R2);
                d2 = (d1 >> (int)(26L)) + (uint64(h0) * r2) + (uint64(h1) * r1) + (uint64(h2) * r0) + (uint64(h3) * R4) + (uint64(h4) * R3);
                d3 = (d2 >> (int)(26L)) + (uint64(h0) * r3) + (uint64(h1) * r2) + (uint64(h2) * r1) + (uint64(h3) * r0) + (uint64(h4) * R4);
                d4 = (d3 >> (int)(26L)) + (uint64(h0) * r4) + (uint64(h1) * r3) + (uint64(h2) * r2) + (uint64(h3) * r1) + (uint64(h4) * r0); 

                // h %= p
                h0 = uint32(d0) & 0x3ffffffUL;
                h1 = uint32(d1) & 0x3ffffffUL;
                h2 = uint32(d2) & 0x3ffffffUL;
                h3 = uint32(d3) & 0x3ffffffUL;
                h4 = uint32(d4) & 0x3ffffffUL;

                h0 += uint32(d4 >> (int)(26L)) * 5L;
                h1 += h0 >> (int)(26L);
                h0 = h0 & 0x3ffffffUL;
            }
            h2 += h1 >> (int)(26L);
            h1 &= 0x3ffffffUL;
            h3 += h2 >> (int)(26L);
            h2 &= 0x3ffffffUL;
            h4 += h3 >> (int)(26L);
            h3 &= 0x3ffffffUL;
            h0 += 5L * (h4 >> (int)(26L));
            h4 &= 0x3ffffffUL;
            h1 += h0 >> (int)(26L);
            h0 &= 0x3ffffffUL; 

            // h - p
            var t0 = h0 + 5L;
            var t1 = h1 + (t0 >> (int)(26L));
            var t2 = h2 + (t1 >> (int)(26L));
            var t3 = h3 + (t2 >> (int)(26L));
            var t4 = h4 + (t3 >> (int)(26L)) - (1L << (int)(26L));
            t0 &= 0x3ffffffUL;
            t1 &= 0x3ffffffUL;
            t2 &= 0x3ffffffUL;
            t3 &= 0x3ffffffUL; 

            // select h if h < p else h - p
            var t_mask = (t4 >> (int)(31L)) - 1L;
            var h_mask = ~t_mask;
            h0 = (h0 & h_mask) | (t0 & t_mask);
            h1 = (h1 & h_mask) | (t1 & t_mask);
            h2 = (h2 & h_mask) | (t2 & t_mask);
            h3 = (h3 & h_mask) | (t3 & t_mask);
            h4 = (h4 & h_mask) | (t4 & t_mask); 

            // h %= 2^128
            h0 |= h1 << (int)(26L);
            h1 = ((h1 >> (int)(6L)) | (h2 << (int)(20L)));
            h2 = ((h2 >> (int)(12L)) | (h3 << (int)(14L)));
            h3 = ((h3 >> (int)(18L)) | (h4 << (int)(8L))); 

            // s: the s part of the key
            // tag = (h + s) % (2^128)
            var t = uint64(h0) + uint64(binary.LittleEndian.Uint32(key[16L..]));
            h0 = uint32(t);
            t = uint64(h1) + uint64(binary.LittleEndian.Uint32(key[20L..])) + (t >> (int)(32L));
            h1 = uint32(t);
            t = uint64(h2) + uint64(binary.LittleEndian.Uint32(key[24L..])) + (t >> (int)(32L));
            h2 = uint32(t);
            t = uint64(h3) + uint64(binary.LittleEndian.Uint32(key[28L..])) + (t >> (int)(32L));
            h3 = uint32(t);

            binary.LittleEndian.PutUint32(out[0L..], h0);
            binary.LittleEndian.PutUint32(out[4L..], h1);
            binary.LittleEndian.PutUint32(out[8L..], h2);
            binary.LittleEndian.PutUint32(out[12L..], h3);
        }
    }
}}}}}
