// Copyright (c) 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using subtle = crypto.subtle_package;
using crypto;

partial class edwards25519_package {

// A dynamic lookup table for variable-base, constant-time scalar muls.
[GoType] partial struct projLookupTable {
    internal array<projCached> points = new(8);
}

// A precomputed lookup table for fixed-base, constant-time scalar muls.
[GoType] partial struct affineLookupTable {
    internal array<affineCached> points = new(8);
}

// A dynamic lookup table for variable-base, variable-time scalar muls.
[GoType] partial struct nafLookupTable5 {
    internal array<projCached> points = new(8);
}

// A precomputed lookup table for fixed-base, variable-time scalar muls.
[GoType] partial struct nafLookupTable8 {
    internal array<affineCached> points = new(64);
}

// Constructors.

// Builds a lookup table at runtime. Fast.
[GoRecv] internal static void FromP3(this ref projLookupTable v, ж<Point> Ꮡq) {
    ref var q = ref Ꮡq.val;

    // Goal: v.points[i] = (i+1)*Q, i.e., Q, 2Q, ..., 8Q
    // This allows lookup of -8Q, ..., -Q, 0, Q, ..., 8Q
    v.points[0].FromP3(Ꮡq);
    var tmpP3 = new Point(nil);
    var tmpP1xP1 = new projP1xP1(nil);
    ref var i = ref heap<nint>(out var Ꮡi);
    for (i = 0; i < 7; i++) {
        // Compute (i+1)*Q as Q + i*Q and convert to a projCached
        // This is needlessly complicated because the API has explicit
        // receivers instead of creating stack objects and relying on RVO
        v.points[i + 1].FromP3(tmpP3.fromP1xP1(tmpP1xP1.Add(Ꮡq, Ꮡ(v.points[i]))));
    }
}

// This is not optimised for speed; fixed-base tables should be precomputed.
[GoRecv] internal static void FromP3(this ref affineLookupTable v, ж<Point> Ꮡq) {
    ref var q = ref Ꮡq.val;

    // Goal: v.points[i] = (i+1)*Q, i.e., Q, 2Q, ..., 8Q
    // This allows lookup of -8Q, ..., -Q, 0, Q, ..., 8Q
    v.points[0].FromP3(Ꮡq);
    var tmpP3 = new Point(nil);
    var tmpP1xP1 = new projP1xP1(nil);
    ref var i = ref heap<nint>(out var Ꮡi);
    for (i = 0; i < 7; i++) {
        // Compute (i+1)*Q as Q + i*Q and convert to affineCached
        v.points[i + 1].FromP3(tmpP3.fromP1xP1(tmpP1xP1.AddAffine(Ꮡq, Ꮡ(v.points[i]))));
    }
}

// Builds a lookup table at runtime. Fast.
[GoRecv] internal static void FromP3(this ref nafLookupTable5 v, ж<Point> Ꮡq) {
    ref var q = ref Ꮡq.val;

    // Goal: v.points[i] = (2*i+1)*Q, i.e., Q, 3Q, 5Q, ..., 15Q
    // This allows lookup of -15Q, ..., -3Q, -Q, 0, Q, 3Q, ..., 15Q
    v.points[0].FromP3(Ꮡq);
    ref var q2 = ref heap<Point>(out var Ꮡq2);
    q2 = new Point(nil);
    q2.Add(Ꮡq, Ꮡq);
    var tmpP3 = new Point(nil);
    var tmpP1xP1 = new projP1xP1(nil);
    ref var i = ref heap<nint>(out var Ꮡi);
    for (i = 0; i < 7; i++) {
        v.points[i + 1].FromP3(tmpP3.fromP1xP1(tmpP1xP1.Add(Ꮡq2, Ꮡ(v.points[i]))));
    }
}

// This is not optimised for speed; fixed-base tables should be precomputed.
[GoRecv] internal static void FromP3(this ref nafLookupTable8 v, ж<Point> Ꮡq) {
    ref var q = ref Ꮡq.val;

    v.points[0].FromP3(Ꮡq);
    ref var q2 = ref heap<Point>(out var Ꮡq2);
    q2 = new Point(nil);
    q2.Add(Ꮡq, Ꮡq);
    var tmpP3 = new Point(nil);
    var tmpP1xP1 = new projP1xP1(nil);
    ref var i = ref heap<nint>(out var Ꮡi);
    for (i = 0; i < 63; i++) {
        v.points[i + 1].FromP3(tmpP3.fromP1xP1(tmpP1xP1.AddAffine(Ꮡq2, Ꮡ(v.points[i]))));
    }
}

// Selectors.

// Set dest to x*Q, where -8 <= x <= 8, in constant time.
[GoRecv] internal static void SelectInto(this ref projLookupTable v, ж<projCached> Ꮡdest, int8 x) {
    ref var dest = ref Ꮡdest.val;

    // Compute xabs = |x|
    var xmask = x >> (int)(7);
    var xabs = ((uint8)((int8)((x + xmask) ^ xmask)));
    dest.Zero();
    ref var j = ref heap<nint>(out var Ꮡj);
    for (j = 1; j <= 8; j++) {
        // Set dest = j*Q if |x| = j
        nint cond = subtle.ConstantTimeByteEq(xabs, ((uint8)j));
        dest.Select(Ꮡ(v.points[j - 1]), Ꮡdest, cond);
    }
    // Now dest = |x|*Q, conditionally negate to get x*Q
    dest.CondNeg(((nint)((int8)(xmask & 1))));
}

// Set dest to x*Q, where -8 <= x <= 8, in constant time.
[GoRecv] internal static void SelectInto(this ref affineLookupTable v, ж<affineCached> Ꮡdest, int8 x) {
    ref var dest = ref Ꮡdest.val;

    // Compute xabs = |x|
    var xmask = x >> (int)(7);
    var xabs = ((uint8)((int8)((x + xmask) ^ xmask)));
    dest.Zero();
    ref var j = ref heap<nint>(out var Ꮡj);
    for (j = 1; j <= 8; j++) {
        // Set dest = j*Q if |x| = j
        nint cond = subtle.ConstantTimeByteEq(xabs, ((uint8)j));
        dest.Select(Ꮡ(v.points[j - 1]), Ꮡdest, cond);
    }
    // Now dest = |x|*Q, conditionally negate to get x*Q
    dest.CondNeg(((nint)((int8)(xmask & 1))));
}

// Given odd x with 0 < x < 2^4, return x*Q (in variable time).
[GoRecv] internal static void SelectInto(this ref nafLookupTable5 v, ж<projCached> Ꮡdest, int8 x) {
    ref var dest = ref Ꮡdest.val;

    dest = v.points[x / 2];
}

// Given odd x with 0 < x < 2^7, return x*Q (in variable time).
[GoRecv] internal static void SelectInto(this ref nafLookupTable8 v, ж<affineCached> Ꮡdest, int8 x) {
    ref var dest = ref Ꮡdest.val;

    dest = v.points[x / 2];
}

} // end edwards25519_package
