// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package liveness -- go2cs converted at 2022 March 13 06:23:56 UTC
// import "cmd/compile/internal/liveness" ==> using liveness = go.cmd.compile.@internal.liveness_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\liveness\bvset.go
namespace go.cmd.compile.@internal;

using bitvec = cmd.compile.@internal.bitvec_package;

public static partial class liveness_package {

// FNV-1 hash function constants.
private static readonly nint h0 = (nint)2166136261L;
private static readonly nint hp = 16777619;

// bvecSet is a set of bvecs, in initial insertion order.
private partial struct bvecSet {
    public slice<nint> index; // hash -> uniq index. -1 indicates empty slot.
    public slice<bitvec.BitVec> uniq; // unique bvecs, in insertion order
}

private static void grow(this ptr<bvecSet> _addr_m) {
    ref bvecSet m = ref _addr_m.val;
 
    // Allocate new index.
    var n = len(m.index) * 2;
    if (n == 0) {
        n = 32;
    }
    var newIndex = make_slice<nint>(n);
    {
        var i__prev1 = i;

        foreach (var (__i) in newIndex) {
            i = __i;
            newIndex[i] = -1;
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i, __bv) in m.uniq) {
            i = __i;
            bv = __bv;
            var h = hashbitmap(h0, bv) % uint32(len(newIndex));
            while (true) {
                var j = newIndex[h];
                if (j < 0) {
                    newIndex[h] = i;
                    break;
                }
                h++;
                if (h == uint32(len(newIndex))) {
                    h = 0;
                }
            }
        }
        i = i__prev1;
    }

    m.index = newIndex;
}

// add adds bv to the set and returns its index in m.extractUnique.
// The caller must not modify bv after this.
private static nint add(this ptr<bvecSet> _addr_m, bitvec.BitVec bv) {
    ref bvecSet m = ref _addr_m.val;

    if (len(m.uniq) * 4 >= len(m.index)) {
        m.grow();
    }
    var index = m.index;
    var h = hashbitmap(h0, bv) % uint32(len(index));
    while (true) {
        var j = index[h];
        if (j < 0) { 
            // New bvec.
            index[h] = len(m.uniq);
            m.uniq = append(m.uniq, bv);
            return len(m.uniq) - 1;
        }
        var jlive = m.uniq[j];
        if (bv.Eq(jlive)) { 
            // Existing bvec.
            return j;
        }
        h++;
        if (h == uint32(len(index))) {
            h = 0;
        }
    }
}

// extractUnique returns this slice of unique bit vectors in m, as
// indexed by the result of bvecSet.add.
private static slice<bitvec.BitVec> extractUnique(this ptr<bvecSet> _addr_m) {
    ref bvecSet m = ref _addr_m.val;

    return m.uniq;
}

private static uint hashbitmap(uint h, bitvec.BitVec bv) {
    var n = int((bv.N + 31) / 32);
    for (nint i = 0; i < n; i++) {
        var w = bv.B[i];
        h = (h * hp) ^ (w & 0xff);
        h = (h * hp) ^ ((w >> 8) & 0xff);
        h = (h * hp) ^ ((w >> 16) & 0xff);
        h = (h * hp) ^ ((w >> 24) & 0xff);
    }

    return h;
}

} // end liveness_package
