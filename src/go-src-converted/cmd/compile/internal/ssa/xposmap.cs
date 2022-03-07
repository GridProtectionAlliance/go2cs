// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 23:09:22 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\xposmap.go
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class ssa_package {

private partial struct lineRange {
    public uint first;
    public uint last;
}

// An xposmap is a map from fileindex and line of src.XPos to int32,
// implemented sparsely to save space (column and statement status are ignored).
// The sparse skeleton is constructed once, and then reused by ssa phases
// that (re)move values with statements attached.
private partial struct xposmap {
    public map<int, ptr<biasedSparseMap>> maps; // The next two fields provide a single-item cache for common case of repeated lines from same file.
    public int lastIndex; // -1 means no entry in cache
    public ptr<biasedSparseMap> lastMap; // map found at maps[lastIndex]
}

// newXposmap constructs an xposmap valid for inputs which have a file index in the keys of x,
// and line numbers in the range x[file index].
// The resulting xposmap will panic if a caller attempts to set or add an XPos not in that range.
private static ptr<xposmap> newXposmap(map<nint, lineRange> x) {
    var maps = make_map<int, ptr<biasedSparseMap>>();
    foreach (var (i, p) in x) {
        maps[int32(i)] = newBiasedSparseMap(int(p.first), int(p.last));
    }    return addr(new xposmap(maps:maps,lastIndex:-1)); // zero for the rest is okay
}

// clear removes data from the map but leaves the sparse skeleton.
private static void clear(this ptr<xposmap> _addr_m) {
    ref xposmap m = ref _addr_m.val;

    foreach (var (_, l) in m.maps) {
        if (l != null) {
            l.clear();
        }
    }    m.lastIndex = -1;
    m.lastMap = null;

}

// mapFor returns the line range map for a given file index.
private static ptr<biasedSparseMap> mapFor(this ptr<xposmap> _addr_m, int index) {
    ref xposmap m = ref _addr_m.val;

    if (index == m.lastIndex) {
        return _addr_m.lastMap!;
    }
    var mf = m.maps[index];
    m.lastIndex = index;
    m.lastMap = mf;
    return _addr_mf!;

}

// set inserts p->v into the map.
// If p does not fall within the set of fileindex->lineRange used to construct m, this will panic.
private static void set(this ptr<xposmap> _addr_m, src.XPos p, int v) => func((_, panic, _) => {
    ref xposmap m = ref _addr_m.val;

    var s = m.mapFor(p.FileIndex());
    if (s == null) {
        panic(fmt.Sprintf("xposmap.set(%d), file index not found in map\n", p.FileIndex()));
    }
    s.set(p.Line(), v);

});

// get returns the int32 associated with the file index and line of p.
private static int get(this ptr<xposmap> _addr_m, src.XPos p) {
    ref xposmap m = ref _addr_m.val;

    var s = m.mapFor(p.FileIndex());
    if (s == null) {
        return -1;
    }
    return s.get(p.Line());

}

// add adds p to m, treating m as a set instead of as a map.
// If p does not fall within the set of fileindex->lineRange used to construct m, this will panic.
// Use clear() in between set/map interpretations of m.
private static void add(this ptr<xposmap> _addr_m, src.XPos p) {
    ref xposmap m = ref _addr_m.val;

    m.set(p, 0);
}

// contains returns whether the file index and line of p are in m,
// treating m as a set instead of as a map.
private static bool contains(this ptr<xposmap> _addr_m, src.XPos p) {
    ref xposmap m = ref _addr_m.val;

    var s = m.mapFor(p.FileIndex());
    if (s == null) {
        return false;
    }
    return s.contains(p.Line());

}

// remove removes the file index and line for p from m,
// whether m is currently treated as a map or set.
private static void remove(this ptr<xposmap> _addr_m, src.XPos p) {
    ref xposmap m = ref _addr_m.val;

    var s = m.mapFor(p.FileIndex());
    if (s == null) {
        return ;
    }
    s.remove(p.Line());

}

// foreachEntry applies f to each (fileindex, line, value) triple in m.
private static void foreachEntry(this ptr<xposmap> _addr_m, Action<int, nuint, int> f) {
    ref xposmap m = ref _addr_m.val;

    foreach (var (j, mm) in m.maps) {
        var s = mm.size();
        for (nint i = 0; i < s; i++) {
            var (l, v) = mm.getEntry(i);
            f(j, l, v);
        }
    }
}

} // end ssa_package
