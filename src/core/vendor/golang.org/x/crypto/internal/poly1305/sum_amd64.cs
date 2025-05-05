// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build gc && !purego
namespace go.vendor.golang.org.x.crypto.@internal;

partial class poly1305_package {

//go:noescape
internal static partial void update(ж<macState> state, slice<byte> msg);

// mac is a wrapper for macGeneric that redirects calls that would have gone to
// updateGeneric to update.
//
// Its Write and Sum methods are otherwise identical to the macGeneric ones, but
// using function pointers would carry a major performance cost.
[GoType] partial struct mac {
    internal partial ref macGeneric macGeneric { get; }
}

[GoRecv] internal static (nint, error) Write(this ref mac h, slice<byte> p) {
    nint nn = len(p);
    if (h.offset > 0) {
        nint n = copy(h.buffer[(int)(h.offset)..], p);
        if (h.offset + n < TagSize) {
            h.offset += n;
            return (nn, default!);
        }
        p = p[(int)(n)..];
        h.offset = 0;
        update(Ꮡ(h.macState), h.buffer[..]);
    }
    {
        nint n = len(p) - (len(p) % TagSize); if (n > 0) {
            update(Ꮡ(h.macState), p[..(int)(n)]);
            p = p[(int)(n)..];
        }
    }
    if (len(p) > 0) {
        h.offset += copy(h.buffer[(int)(h.offset)..], p);
    }
    return (nn, default!);
}

[GoRecv] internal static void Sum(this ref mac h, ж<array<byte>> Ꮡout) {
    ref var @out = ref Ꮡout.val;

    ref var state = ref heap<macState>(out var Ꮡstate);
    state = h.macState;
    if (h.offset > 0) {
        update(Ꮡstate, h.buffer[..(int)(h.offset)]);
    }
    finalize(Ꮡout, Ꮡstate.of(macState.Ꮡh), Ꮡstate.of(macState.Ꮡs));
}

} // end poly1305_package
