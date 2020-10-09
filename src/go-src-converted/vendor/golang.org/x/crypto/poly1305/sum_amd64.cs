// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !gccgo,!purego

// package poly1305 -- go2cs converted at 2020 October 09 06:06:33 UTC
// import "vendor/golang.org/x/crypto/poly1305" ==> using poly1305 = go.vendor.golang.org.x.crypto.poly1305_package
// Original source: C:\Go\src\vendor\golang.org\x\crypto\poly1305\sum_amd64.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto
{
    public static partial class poly1305_package
    {
        //go:noescape
        private static void update(ptr<macState> state, slice<byte> msg)
;

        // mac is a wrapper for macGeneric that redirects calls that would have gone to
        // updateGeneric to update.
        //
        // Its Write and Sum methods are otherwise identical to the macGeneric ones, but
        // using function pointers would carry a major performance cost.
        private partial struct mac
        {
            public ref macGeneric macGeneric => ref macGeneric_val;
        }

        private static (long, error) Write(this ptr<mac> _addr_h, slice<byte> p)
        {
            long _p0 = default;
            error _p0 = default!;
            ref mac h = ref _addr_h.val;

            var nn = len(p);
            if (h.offset > 0L)
            {>>MARKER:FUNCTION_update_BLOCK_PREFIX<<
                var n = copy(h.buffer[h.offset..], p);
                if (h.offset + n < TagSize)
                {
                    h.offset += n;
                    return (nn, error.As(null!)!);
                }

                p = p[n..];
                h.offset = 0L;
                update(_addr_h.macState, h.buffer[..]);

            }

            {
                var n__prev1 = n;

                n = len(p) - (len(p) % TagSize);

                if (n > 0L)
                {
                    update(_addr_h.macState, p[..n]);
                    p = p[n..];
                }

                n = n__prev1;

            }

            if (len(p) > 0L)
            {
                h.offset += copy(h.buffer[h.offset..], p);
            }

            return (nn, error.As(null!)!);

        }

        private static void Sum(this ptr<mac> _addr_h, ptr<array<byte>> _addr_@out)
        {
            ref mac h = ref _addr_h.val;
            ref array<byte> @out = ref _addr_@out.val;

            ref var state = ref heap(h.macState, out ptr<var> _addr_state);
            if (h.offset > 0L)
            {
                update(_addr_state, h.buffer[..h.offset]);
            }

            finalize(out, _addr_state.h, _addr_state.s);

        }
    }
}}}}}
