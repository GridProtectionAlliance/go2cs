// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 23:08:43 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\sparsemap.go
using src = go.cmd.@internal.src_package;

namespace go.cmd.compile.@internal;

public static partial class ssa_package {

    // from https://research.swtch.com/sparse
    // in turn, from Briggs and Torczon
private partial struct sparseEntry {
    public ID key;
    public int val;
    public src.XPos aux;
}

private partial struct sparseMap {
    public slice<sparseEntry> dense;
    public slice<int> sparse;
}

// newSparseMap returns a sparseMap that can map
// integers between 0 and n-1 to int32s.
private static ptr<sparseMap> newSparseMap(nint n) {
    return addr(new sparseMap(dense:nil,sparse:make([]int32,n)));
}

private static nint cap(this ptr<sparseMap> _addr_s) {
    ref sparseMap s = ref _addr_s.val;

    return len(s.sparse);
}

private static nint size(this ptr<sparseMap> _addr_s) {
    ref sparseMap s = ref _addr_s.val;

    return len(s.dense);
}

private static bool contains(this ptr<sparseMap> _addr_s, ID k) {
    ref sparseMap s = ref _addr_s.val;

    var i = s.sparse[k];
    return i < int32(len(s.dense)) && s.dense[i].key == k;
}

// get returns the value for key k, or -1 if k does
// not appear in the map.
private static int get(this ptr<sparseMap> _addr_s, ID k) {
    ref sparseMap s = ref _addr_s.val;

    var i = s.sparse[k];
    if (i < int32(len(s.dense)) && s.dense[i].key == k) {
        return s.dense[i].val;
    }
    return -1;

}

private static void set(this ptr<sparseMap> _addr_s, ID k, int v, src.XPos a) {
    ref sparseMap s = ref _addr_s.val;

    var i = s.sparse[k];
    if (i < int32(len(s.dense)) && s.dense[i].key == k) {
        s.dense[i].val = v;
        s.dense[i].aux = a;
        return ;
    }
    s.dense = append(s.dense, new sparseEntry(k,v,a));
    s.sparse[k] = int32(len(s.dense)) - 1;

}

// setBit sets the v'th bit of k's value, where 0 <= v < 32
private static void setBit(this ptr<sparseMap> _addr_s, ID k, nuint v) => func((_, panic, _) => {
    ref sparseMap s = ref _addr_s.val;

    if (v >= 32) {
        panic("bit index too large.");
    }
    var i = s.sparse[k];
    if (i < int32(len(s.dense)) && s.dense[i].key == k) {
        s.dense[i].val |= 1 << (int)(v);
        return ;
    }
    s.dense = append(s.dense, new sparseEntry(k,1<<v,src.NoXPos));
    s.sparse[k] = int32(len(s.dense)) - 1;

});

private static void remove(this ptr<sparseMap> _addr_s, ID k) {
    ref sparseMap s = ref _addr_s.val;

    var i = s.sparse[k];
    if (i < int32(len(s.dense)) && s.dense[i].key == k) {
        var y = s.dense[len(s.dense) - 1];
        s.dense[i] = y;
        s.sparse[y.key] = i;
        s.dense = s.dense[..(int)len(s.dense) - 1];
    }
}

private static void clear(this ptr<sparseMap> _addr_s) {
    ref sparseMap s = ref _addr_s.val;

    s.dense = s.dense[..(int)0];
}

private static slice<sparseEntry> contents(this ptr<sparseMap> _addr_s) {
    ref sparseMap s = ref _addr_s.val;

    return s.dense;
}

} // end ssa_package
