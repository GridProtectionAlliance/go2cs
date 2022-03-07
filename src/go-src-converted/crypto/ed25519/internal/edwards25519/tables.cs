// Copyright (c) 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package edwards25519 -- go2cs converted at 2022 March 06 22:17:36 UTC
// import "crypto/ed25519/internal/edwards25519" ==> using edwards25519 = go.crypto.ed25519.@internal.edwards25519_package
// Original source: C:\Program Files\Go\src\crypto\ed25519\internal\edwards25519\tables.go
using subtle = go.crypto.subtle_package;

namespace go.crypto.ed25519.@internal;

public static partial class edwards25519_package {

    // A dynamic lookup table for variable-base, constant-time scalar muls.
private partial struct projLookupTable {
    public array<projCached> points;
}

// A precomputed lookup table for fixed-base, constant-time scalar muls.
private partial struct affineLookupTable {
    public array<affineCached> points;
}

// A dynamic lookup table for variable-base, variable-time scalar muls.
private partial struct nafLookupTable5 {
    public array<projCached> points;
}

// A precomputed lookup table for fixed-base, variable-time scalar muls.
private partial struct nafLookupTable8 {
    public array<affineCached> points;
}

// Constructors.

// Builds a lookup table at runtime. Fast.
private static void FromP3(this ptr<projLookupTable> _addr_v, ptr<Point> _addr_q) {
    ref projLookupTable v = ref _addr_v.val;
    ref Point q = ref _addr_q.val;
 
    // Goal: v.points[i] = (i+1)*Q, i.e., Q, 2Q, ..., 8Q
    // This allows lookup of -8Q, ..., -Q, 0, Q, ..., 8Q
    v.points[0].FromP3(q);
    Point tmpP3 = new Point();
    projP1xP1 tmpP1xP1 = new projP1xP1();
    for (nint i = 0; i < 7; i++) { 
        // Compute (i+1)*Q as Q + i*Q and convert to a ProjCached
        // This is needlessly complicated because the API has explicit
        // recievers instead of creating stack objects and relying on RVO
        v.points[i + 1].FromP3(tmpP3.fromP1xP1(tmpP1xP1.Add(q, _addr_v.points[i])));

    }

}

// This is not optimised for speed; fixed-base tables should be precomputed.
private static void FromP3(this ptr<affineLookupTable> _addr_v, ptr<Point> _addr_q) {
    ref affineLookupTable v = ref _addr_v.val;
    ref Point q = ref _addr_q.val;
 
    // Goal: v.points[i] = (i+1)*Q, i.e., Q, 2Q, ..., 8Q
    // This allows lookup of -8Q, ..., -Q, 0, Q, ..., 8Q
    v.points[0].FromP3(q);
    Point tmpP3 = new Point();
    projP1xP1 tmpP1xP1 = new projP1xP1();
    for (nint i = 0; i < 7; i++) { 
        // Compute (i+1)*Q as Q + i*Q and convert to AffineCached
        v.points[i + 1].FromP3(tmpP3.fromP1xP1(tmpP1xP1.AddAffine(q, _addr_v.points[i])));

    }

}

// Builds a lookup table at runtime. Fast.
private static void FromP3(this ptr<nafLookupTable5> _addr_v, ptr<Point> _addr_q) {
    ref nafLookupTable5 v = ref _addr_v.val;
    ref Point q = ref _addr_q.val;
 
    // Goal: v.points[i] = (2*i+1)*Q, i.e., Q, 3Q, 5Q, ..., 15Q
    // This allows lookup of -15Q, ..., -3Q, -Q, 0, Q, 3Q, ..., 15Q
    v.points[0].FromP3(q);
    ref Point q2 = ref heap(new Point(), out ptr<Point> _addr_q2);
    q2.Add(q, q);
    Point tmpP3 = new Point();
    projP1xP1 tmpP1xP1 = new projP1xP1();
    for (nint i = 0; i < 7; i++) {
        v.points[i + 1].FromP3(tmpP3.fromP1xP1(tmpP1xP1.Add(_addr_q2, _addr_v.points[i])));
    }

}

// This is not optimised for speed; fixed-base tables should be precomputed.
private static void FromP3(this ptr<nafLookupTable8> _addr_v, ptr<Point> _addr_q) {
    ref nafLookupTable8 v = ref _addr_v.val;
    ref Point q = ref _addr_q.val;

    v.points[0].FromP3(q);
    ref Point q2 = ref heap(new Point(), out ptr<Point> _addr_q2);
    q2.Add(q, q);
    Point tmpP3 = new Point();
    projP1xP1 tmpP1xP1 = new projP1xP1();
    for (nint i = 0; i < 63; i++) {
        v.points[i + 1].FromP3(tmpP3.fromP1xP1(tmpP1xP1.AddAffine(_addr_q2, _addr_v.points[i])));
    }
}

// Selectors.

// Set dest to x*Q, where -8 <= x <= 8, in constant time.
private static void SelectInto(this ptr<projLookupTable> _addr_v, ptr<projCached> _addr_dest, sbyte x) {
    ref projLookupTable v = ref _addr_v.val;
    ref projCached dest = ref _addr_dest.val;
 
    // Compute xabs = |x|
    var xmask = x >> 7;
    var xabs = uint8((x + xmask) ^ xmask);

    dest.Zero();
    for (nint j = 1; j <= 8; j++) { 
        // Set dest = j*Q if |x| = j
        var cond = subtle.ConstantTimeByteEq(xabs, uint8(j));
        dest.Select(_addr_v.points[j - 1], dest, cond);

    } 
    // Now dest = |x|*Q, conditionally negate to get x*Q
    dest.CondNeg(int(xmask & 1));

}

// Set dest to x*Q, where -8 <= x <= 8, in constant time.
private static void SelectInto(this ptr<affineLookupTable> _addr_v, ptr<affineCached> _addr_dest, sbyte x) {
    ref affineLookupTable v = ref _addr_v.val;
    ref affineCached dest = ref _addr_dest.val;
 
    // Compute xabs = |x|
    var xmask = x >> 7;
    var xabs = uint8((x + xmask) ^ xmask);

    dest.Zero();
    for (nint j = 1; j <= 8; j++) { 
        // Set dest = j*Q if |x| = j
        var cond = subtle.ConstantTimeByteEq(xabs, uint8(j));
        dest.Select(_addr_v.points[j - 1], dest, cond);

    } 
    // Now dest = |x|*Q, conditionally negate to get x*Q
    dest.CondNeg(int(xmask & 1));

}

// Given odd x with 0 < x < 2^4, return x*Q (in variable time).
private static void SelectInto(this ptr<nafLookupTable5> _addr_v, ptr<projCached> _addr_dest, sbyte x) {
    ref nafLookupTable5 v = ref _addr_v.val;
    ref projCached dest = ref _addr_dest.val;

    dest = v.points[x / 2];
}

// Given odd x with 0 < x < 2^7, return x*Q (in variable time).
private static void SelectInto(this ptr<nafLookupTable8> _addr_v, ptr<affineCached> _addr_dest, sbyte x) {
    ref nafLookupTable8 v = ref _addr_v.val;
    ref affineCached dest = ref _addr_dest.val;

    dest = v.points[x / 2];
}

} // end edwards25519_package
