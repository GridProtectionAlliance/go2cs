// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package poly1305 implements Poly1305 one-time message authentication code as
// specified in https://cr.yp.to/mac/poly1305-20050329.pdf.
//
// Poly1305 is a fast, one-time authentication function. It is infeasible for an
// attacker to generate an authenticator for a message without the key. However, a
// key must only be used for a single message. Authenticating two different
// messages with the same key allows an attacker to forge authenticators for other
// messages with the same key.
//
// Poly1305 was originally coupled with AES in order to make Poly1305-AES. AES was
// used with a fixed key in order to generate one-time keys from an nonce.
// However, in this package AES isn't used and the one-time key is specified
// directly.
// package poly1305 -- go2cs converted at 2020 October 09 06:06:33 UTC
// import "vendor/golang.org/x/crypto/poly1305" ==> using poly1305 = go.vendor.golang.org.x.crypto.poly1305_package
// Original source: C:\Go\src\vendor\golang.org\x\crypto\poly1305\poly1305.go
// import "golang.org/x/crypto/poly1305"

using subtle = go.crypto.subtle_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto
{
    public static partial class poly1305_package
    {
        // TagSize is the size, in bytes, of a poly1305 authenticator.
        public static readonly long TagSize = (long)16L;

        // Sum generates an authenticator for msg using a one-time key and puts the
        // 16-byte result into out. Authenticating two different messages with the same
        // key allows an attacker to forge messages at will.


        // Sum generates an authenticator for msg using a one-time key and puts the
        // 16-byte result into out. Authenticating two different messages with the same
        // key allows an attacker to forge messages at will.
        public static void Sum(ptr<array<byte>> _addr_@out, slice<byte> m, ptr<array<byte>> _addr_key)
        {
            ref array<byte> @out = ref _addr_@out.val;
            ref array<byte> key = ref _addr_key.val;

            var h = New(_addr_key);
            h.Write(m);
            h.Sum(out[..0L]);
        }

        // Verify returns true if mac is a valid authenticator for m with the given key.
        public static bool Verify(ptr<array<byte>> _addr_mac, slice<byte> m, ptr<array<byte>> _addr_key)
        {
            ref array<byte> mac = ref _addr_mac.val;
            ref array<byte> key = ref _addr_key.val;

            ref array<byte> tmp = ref heap(new array<byte>(16L), out ptr<array<byte>> _addr_tmp);
            Sum(_addr_tmp, m, _addr_key);
            return subtle.ConstantTimeCompare(tmp[..], mac[..]) == 1L;
        }

        // New returns a new MAC computing an authentication
        // tag of all data written to it with the given key.
        // This allows writing the message progressively instead
        // of passing it as a single slice. Common users should use
        // the Sum function instead.
        //
        // The key must be unique for each message, as authenticating
        // two different messages with the same key allows an attacker
        // to forge messages at will.
        public static ptr<MAC> New(ptr<array<byte>> _addr_key)
        {
            ref array<byte> key = ref _addr_key.val;

            ptr<MAC> m = addr(new MAC());
            initialize(key, _addr_m.macState);
            return _addr_m!;
        }

        // MAC is an io.Writer computing an authentication tag
        // of the data written to it.
        //
        // MAC cannot be used like common hash.Hash implementations,
        // because using a poly1305 key twice breaks its security.
        // Therefore writing data to a running MAC after calling
        // Sum or Verify causes it to panic.
        public partial struct MAC
        {
            public ref mac mac => ref mac_val; // platform-dependent implementation

            public bool finalized;
        }

        // Size returns the number of bytes Sum will return.
        private static long Size(this ptr<MAC> _addr_h)
        {
            ref MAC h = ref _addr_h.val;

            return TagSize;
        }

        // Write adds more data to the running message authentication code.
        // It never returns an error.
        //
        // It must not be called after the first call of Sum or Verify.
        private static (long, error) Write(this ptr<MAC> _addr_h, slice<byte> p) => func((_, panic, __) =>
        {
            long n = default;
            error err = default!;
            ref MAC h = ref _addr_h.val;

            if (h.finalized)
            {
                panic("poly1305: write to MAC after Sum or Verify");
            }

            return h.mac.Write(p);

        });

        // Sum computes the authenticator of all data written to the
        // message authentication code.
        private static slice<byte> Sum(this ptr<MAC> _addr_h, slice<byte> b)
        {
            ref MAC h = ref _addr_h.val;

            ref array<byte> mac = ref heap(new array<byte>(TagSize), out ptr<array<byte>> _addr_mac);
            h.mac.Sum(_addr_mac);
            h.finalized = true;
            return append(b, mac[..]);
        }

        // Verify returns whether the authenticator of all data written to
        // the message authentication code matches the expected value.
        private static bool Verify(this ptr<MAC> _addr_h, slice<byte> expected)
        {
            ref MAC h = ref _addr_h.val;

            ref array<byte> mac = ref heap(new array<byte>(TagSize), out ptr<array<byte>> _addr_mac);
            h.mac.Sum(_addr_mac);
            h.finalized = true;
            return subtle.ConstantTimeCompare(expected, mac[..]) == 1L;
        }
    }
}}}}}
