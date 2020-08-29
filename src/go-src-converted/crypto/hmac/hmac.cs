// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package hmac implements the Keyed-Hash Message Authentication Code (HMAC) as
defined in U.S. Federal Information Processing Standards Publication 198.
An HMAC is a cryptographic hash that uses a key to sign a message.
The receiver verifies the hash by recomputing it using the same key.

Receivers should be careful to use Equal to compare MACs in order to avoid
timing side-channels:

    // CheckMAC reports whether messageMAC is a valid HMAC tag for message.
    func CheckMAC(message, messageMAC, key []byte) bool {
        mac := hmac.New(sha256.New, key)
        mac.Write(message)
        expectedMAC := mac.Sum(nil)
        return hmac.Equal(messageMAC, expectedMAC)
    }
*/
// package hmac -- go2cs converted at 2020 August 29 08:28:34 UTC
// import "crypto/hmac" ==> using hmac = go.crypto.hmac_package
// Original source: C:\Go\src\crypto\hmac\hmac.go
using subtle = go.crypto.subtle_package;
using hash = go.hash_package;
using static go.builtin;
using System;

namespace go {
namespace crypto
{
    public static partial class hmac_package
    {
        // FIPS 198-1:
        // http://csrc.nist.gov/publications/fips/fips198-1/FIPS-198-1_final.pdf

        // key is zero padded to the block size of the hash function
        // ipad = 0x36 byte repeated for key length
        // opad = 0x5c byte repeated for key length
        // hmac = H([key ^ opad] H([key ^ ipad] text))
        private partial struct hmac
        {
            public long size;
            public long blocksize;
            public slice<byte> opad;
            public slice<byte> ipad;
            public hash.Hash outer;
            public hash.Hash inner;
        }

        private static slice<byte> Sum(this ref hmac h, slice<byte> @in)
        {
            var origLen = len(in);
            in = h.inner.Sum(in);
            h.outer.Reset();
            h.outer.Write(h.opad);
            h.outer.Write(in[origLen..]);
            return h.outer.Sum(in[..origLen]);
        }

        private static (long, error) Write(this ref hmac h, slice<byte> p)
        {
            return h.inner.Write(p);
        }

        private static long Size(this ref hmac h)
        {
            return h.size;
        }

        private static long BlockSize(this ref hmac h)
        {
            return h.blocksize;
        }

        private static void Reset(this ref hmac h)
        {
            h.inner.Reset();
            h.inner.Write(h.ipad);
        }

        // New returns a new HMAC hash using the given hash.Hash type and key.
        // Note that unlike other hash implementations in the standard library,
        // the returned Hash does not implement encoding.BinaryMarshaler
        // or encoding.BinaryUnmarshaler.
        public static hash.Hash New(Func<hash.Hash> h, slice<byte> key)
        {
            ptr<hmac> hm = @new<hmac>();
            hm.outer = h();
            hm.inner = h();
            hm.size = hm.inner.Size();
            hm.blocksize = hm.inner.BlockSize();
            hm.ipad = make_slice<byte>(hm.blocksize);
            hm.opad = make_slice<byte>(hm.blocksize);
            if (len(key) > hm.blocksize)
            { 
                // If key is too big, hash it.
                hm.outer.Write(key);
                key = hm.outer.Sum(null);
            }
            copy(hm.ipad, key);
            copy(hm.opad, key);
            {
                var i__prev1 = i;

                foreach (var (__i) in hm.ipad)
                {
                    i = __i;
                    hm.ipad[i] ^= 0x36UL;
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in hm.opad)
                {
                    i = __i;
                    hm.opad[i] ^= 0x5cUL;
                }

                i = i__prev1;
            }

            hm.inner.Write(hm.ipad);
            return hm;
        }

        // Equal compares two MACs for equality without leaking timing information.
        public static bool Equal(slice<byte> mac1, slice<byte> mac2)
        { 
            // We don't have to be constant time if the lengths of the MACs are
            // different as that suggests that a completely different hash function
            // was used.
            return subtle.ConstantTimeCompare(mac1, mac2) == 1L;
        }
    }
}}
