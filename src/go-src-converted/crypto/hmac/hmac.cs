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

    // ValidMAC reports whether messageMAC is a valid HMAC tag for message.
    func ValidMAC(message, messageMAC, key []byte) bool {
        mac := hmac.New(sha256.New, key)
        mac.Write(message)
        expectedMAC := mac.Sum(nil)
        return hmac.Equal(messageMAC, expectedMAC)
    }
*/
// package hmac -- go2cs converted at 2020 October 09 04:54:34 UTC
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
        // https://csrc.nist.gov/publications/fips/fips198-1/FIPS-198-1_final.pdf

        // key is zero padded to the block size of the hash function
        // ipad = 0x36 byte repeated for key length
        // opad = 0x5c byte repeated for key length
        // hmac = H([key ^ opad] H([key ^ ipad] text))

        // Marshalable is the combination of encoding.BinaryMarshaler and
        // encoding.BinaryUnmarshaler. Their method definitions are repeated here to
        // avoid a dependency on the encoding package.
        private partial interface marshalable
        {
            error MarshalBinary();
            error UnmarshalBinary(slice<byte> _p0);
        }

        private partial struct hmac
        {
            public slice<byte> opad;
            public slice<byte> ipad;
            public hash.Hash outer; // If marshaled is true, then opad and ipad do not contain a padded
// copy of the key, but rather the marshaled state of outer/inner after
// opad/ipad has been fed into it.
            public hash.Hash inner; // If marshaled is true, then opad and ipad do not contain a padded
// copy of the key, but rather the marshaled state of outer/inner after
// opad/ipad has been fed into it.
            public bool marshaled;
        }

        private static slice<byte> Sum(this ptr<hmac> _addr_h, slice<byte> @in) => func((_, panic, __) =>
        {
            ref hmac h = ref _addr_h.val;

            var origLen = len(in);
            in = h.inner.Sum(in);

            if (h.marshaled)
            {
                {
                    marshalable err = marshalable.As(h.outer._<marshalable>().UnmarshalBinary(h.opad))!;

                    if (err != null)
                    {
                        panic(err);
                    }

                }

            }
            else
            {
                h.outer.Reset();
                h.outer.Write(h.opad);
            }

            h.outer.Write(in[origLen..]);
            return h.outer.Sum(in[..origLen]);

        });

        private static (long, error) Write(this ptr<hmac> _addr_h, slice<byte> p)
        {
            long n = default;
            error err = default!;
            ref hmac h = ref _addr_h.val;

            return h.inner.Write(p);
        }

        private static long Size(this ptr<hmac> _addr_h)
        {
            ref hmac h = ref _addr_h.val;

            return h.outer.Size();
        }
        private static long BlockSize(this ptr<hmac> _addr_h)
        {
            ref hmac h = ref _addr_h.val;

            return h.inner.BlockSize();
        }

        private static void Reset(this ptr<hmac> _addr_h) => func((_, panic, __) =>
        {
            ref hmac h = ref _addr_h.val;

            if (h.marshaled)
            {
                {
                    marshalable err = marshalable.As(h.inner._<marshalable>().UnmarshalBinary(h.ipad))!;

                    if (err != null)
                    {
                        panic(err);
                    }

                }

                return ;

            }

            h.inner.Reset();
            h.inner.Write(h.ipad); 

            // If the underlying hash is marshalable, we can save some time by
            // saving a copy of the hash state now, and restoring it on future
            // calls to Reset and Sum instead of writing ipad/opad every time.
            //
            // If either hash is unmarshalable for whatever reason,
            // it's safe to bail out here.
            marshalable (marshalableInner, innerOK) = marshalable.As(h.inner._<marshalable>())!;
            if (!innerOK)
            {
                return ;
            }

            marshalable (marshalableOuter, outerOK) = marshalable.As(h.outer._<marshalable>())!;
            if (!outerOK)
            {
                return ;
            }

            var (imarshal, err) = marshalableInner.MarshalBinary();
            if (err != null)
            {
                return ;
            }

            h.outer.Reset();
            h.outer.Write(h.opad);
            var (omarshal, err) = marshalableOuter.MarshalBinary();
            if (err != null)
            {
                return ;
            } 

            // Marshaling succeeded; save the marshaled state for later
            h.ipad = imarshal;
            h.opad = omarshal;
            h.marshaled = true;

        });

        // New returns a new HMAC hash using the given hash.Hash type and key.
        // Note that unlike other hash implementations in the standard library,
        // the returned Hash does not implement encoding.BinaryMarshaler
        // or encoding.BinaryUnmarshaler.
        public static hash.Hash New(Func<hash.Hash> h, slice<byte> key)
        {
            ptr<hmac> hm = @new<hmac>();
            hm.outer = h();
            hm.inner = h();
            var blocksize = hm.inner.BlockSize();
            hm.ipad = make_slice<byte>(blocksize);
            hm.opad = make_slice<byte>(blocksize);
            if (len(key) > blocksize)
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
