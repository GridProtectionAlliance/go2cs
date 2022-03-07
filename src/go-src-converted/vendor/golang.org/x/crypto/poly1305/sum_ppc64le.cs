// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build gc && !purego
// +build gc,!purego

// package poly1305 -- go2cs converted at 2022 March 06 23:36:53 UTC
// import "vendor/golang.org/x/crypto/poly1305" ==> using poly1305 = go.vendor.golang.org.x.crypto.poly1305_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\crypto\poly1305\sum_ppc64le.go


namespace go.vendor.golang.org.x.crypto;

public static partial class poly1305_package {

    //go:noescape
private static void update(ptr<macState> state, slice<byte> msg);

// mac is a wrapper for macGeneric that redirects calls that would have gone to
// updateGeneric to update.
//
// Its Write and Sum methods are otherwise identical to the macGeneric ones, but
// using function pointers would carry a major performance cost.
private partial struct mac {
    public ref macGeneric macGeneric => ref macGeneric_val;
}

private static (nint, error) Write(this ptr<mac> _addr_h, slice<byte> p) {
    nint _p0 = default;
    error _p0 = default!;
    ref mac h = ref _addr_h.val;

    var nn = len(p);
    if (h.offset > 0) {>>MARKER:FUNCTION_update_BLOCK_PREFIX<<
        var n = copy(h.buffer[(int)h.offset..], p);
        if (h.offset + n < TagSize) {
            h.offset += n;
            return (nn, error.As(null!)!);
        }
        p = p[(int)n..];
        h.offset = 0;
        update(_addr_h.macState, h.buffer[..]);

    }
    {
        var n__prev1 = n;

        n = len(p) - (len(p) % TagSize);

        if (n > 0) {
            update(_addr_h.macState, p[..(int)n]);
            p = p[(int)n..];
        }
        n = n__prev1;

    }

    if (len(p) > 0) {
        h.offset += copy(h.buffer[(int)h.offset..], p);
    }
    return (nn, error.As(null!)!);

}

private static void Sum(this ptr<mac> _addr_h, ptr<array<byte>> _addr_@out) {
    ref mac h = ref _addr_h.val;
    ref array<byte> @out = ref _addr_@out.val;

    ref var state = ref heap(h.macState, out ptr<var> _addr_state);
    if (h.offset > 0) {
        update(_addr_state, h.buffer[..(int)h.offset]);
    }
    finalize(out, _addr_state.h, _addr_state.s);

}

} // end poly1305_package
