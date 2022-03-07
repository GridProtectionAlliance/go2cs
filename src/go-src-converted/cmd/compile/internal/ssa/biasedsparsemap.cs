// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 22:49:18 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\biasedsparsemap.go
using src = go.cmd.@internal.src_package;
using math = go.math_package;

namespace go.cmd.compile.@internal;

public static partial class ssa_package {

    // A biasedSparseMap is a sparseMap for integers between J and K inclusive,
    // where J might be somewhat larger than zero (and K-J is probably much smaller than J).
    // (The motivating use case is the line numbers of statements for a single function.)
    // Not all features of a SparseMap are exported, and it is also easy to treat a
    // biasedSparseMap like a SparseSet.
private partial struct biasedSparseMap {
    public ptr<sparseMap> s;
    public nint first;
}

// newBiasedSparseMap returns a new biasedSparseMap for values between first and last, inclusive.
private static ptr<biasedSparseMap> newBiasedSparseMap(nint first, nint last) {
    if (first > last) {
        return addr(new biasedSparseMap(first:math.MaxInt32,s:nil));
    }
    return addr(new biasedSparseMap(first:first,s:newSparseMap(1+last-first)));

}

// cap returns one more than the largest key valid for s
private static nint cap(this ptr<biasedSparseMap> _addr_s) {
    ref biasedSparseMap s = ref _addr_s.val;

    if (s == null || s.s == null) {
        return 0;
    }
    return s.s.cap() + int(s.first);

}

// size returns the number of entries stored in s
private static nint size(this ptr<biasedSparseMap> _addr_s) {
    ref biasedSparseMap s = ref _addr_s.val;

    if (s == null || s.s == null) {
        return 0;
    }
    return s.s.size();

}

// contains reports whether x is a key in s
private static bool contains(this ptr<biasedSparseMap> _addr_s, nuint x) {
    ref biasedSparseMap s = ref _addr_s.val;

    if (s == null || s.s == null) {
        return false;
    }
    if (int(x) < s.first) {
        return false;
    }
    if (int(x) >= s.cap()) {
        return false;
    }
    return s.s.contains(ID(int(x) - s.first));

}

// get returns the value s maps for key x, or -1 if
// x is not mapped or is out of range for s.
private static int get(this ptr<biasedSparseMap> _addr_s, nuint x) {
    ref biasedSparseMap s = ref _addr_s.val;

    if (s == null || s.s == null) {
        return -1;
    }
    if (int(x) < s.first) {
        return -1;
    }
    if (int(x) >= s.cap()) {
        return -1;
    }
    return s.s.get(ID(int(x) - s.first));

}

// getEntry returns the i'th key and value stored in s,
// where 0 <= i < s.size()
private static (nuint, int) getEntry(this ptr<biasedSparseMap> _addr_s, nint i) {
    nuint x = default;
    int v = default;
    ref biasedSparseMap s = ref _addr_s.val;

    var e = s.s.contents()[i];
    x = uint(int(e.key) + s.first);
    v = e.val;
    return ;
}

// add inserts x->0 into s, provided that x is in the range of keys stored in s.
private static void add(this ptr<biasedSparseMap> _addr_s, nuint x) {
    ref biasedSparseMap s = ref _addr_s.val;

    if (int(x) < s.first || int(x) >= s.cap()) {
        return ;
    }
    s.s.set(ID(int(x) - s.first), 0, src.NoXPos);

}

// add inserts x->v into s, provided that x is in the range of keys stored in s.
private static void set(this ptr<biasedSparseMap> _addr_s, nuint x, int v) {
    ref biasedSparseMap s = ref _addr_s.val;

    if (int(x) < s.first || int(x) >= s.cap()) {
        return ;
    }
    s.s.set(ID(int(x) - s.first), v, src.NoXPos);

}

// remove removes key x from s.
private static void remove(this ptr<biasedSparseMap> _addr_s, nuint x) {
    ref biasedSparseMap s = ref _addr_s.val;

    if (int(x) < s.first || int(x) >= s.cap()) {
        return ;
    }
    s.s.remove(ID(int(x) - s.first));

}

private static void clear(this ptr<biasedSparseMap> _addr_s) {
    ref biasedSparseMap s = ref _addr_s.val;

    if (s.s != null) {
        s.s.clear();
    }
}

} // end ssa_package
