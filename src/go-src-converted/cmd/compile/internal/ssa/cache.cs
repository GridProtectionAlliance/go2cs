// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 13 06:00:44 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\cache.go
namespace go.cmd.compile.@internal;

using obj = cmd.@internal.obj_package;
using sort = sort_package;


// A Cache holds reusable compiler state.
// It is intended to be re-used for multiple Func compilations.

using System;
public static partial class ssa_package {

public partial struct Cache {
    public array<Value> values;
    public array<Block> blocks;
    public array<Location> locs; // Reusable stackAllocState.
// See stackalloc.go's {new,put}StackAllocState.
    public ptr<stackAllocState> stackAllocState;
    public slice<ID> domblockstore; // scratch space for computing dominators
    public slice<ptr<sparseSet>> scrSparseSet; // scratch sparse sets to be re-used.
    public slice<ptr<sparseMap>> scrSparseMap; // scratch sparse maps to be re-used.
    public slice<ptr<poset>> scrPoset; // scratch poset to be reused
// deadcode contains reusable slices specifically for the deadcode pass.
// It gets special treatment because of the frequency with which it is run.
    public slice<valState> regallocValues;
    public slice<ptr<obj.Prog>> ValueToProgAfter;
    public debugState debugState;
}

private static void Reset(this ptr<Cache> _addr_c) {
    ref Cache c = ref _addr_c.val;

    var nv = sort.Search(len(c.values), i => c.values[i].ID == 0);
    var xv = c.values[..(int)nv];
    {
        var i__prev1 = i;

        foreach (var (__i) in xv) {
            i = __i;
            xv[i] = new Value();
        }
        i = i__prev1;
    }

    var nb = sort.Search(len(c.blocks), i => c.blocks[i].ID == 0);
    var xb = c.blocks[..(int)nb];
    {
        var i__prev1 = i;

        foreach (var (__i) in xb) {
            i = __i;
            xb[i] = new Block();
        }
        i = i__prev1;
    }

    var nl = sort.Search(len(c.locs), i => c.locs[i] == null);
    var xl = c.locs[..(int)nl];
    {
        var i__prev1 = i;

        foreach (var (__i) in xl) {
            i = __i;
            xl[i] = null;
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i) in c.regallocValues) {
            i = __i;
            c.regallocValues[i] = new valState();
        }
        i = i__prev1;
    }

    c.deadcode.liveOrderStmts = c.deadcode.liveOrderStmts[..(int)cap(c.deadcode.liveOrderStmts)];
    var no = sort.Search(len(c.deadcode.liveOrderStmts), i => c.deadcode.liveOrderStmts[i] == null);
    var xo = c.deadcode.liveOrderStmts[..(int)no];
    {
        var i__prev1 = i;

        foreach (var (__i) in xo) {
            i = __i;
            xo[i] = null;
        }
        i = i__prev1;
    }

    c.deadcode.q = c.deadcode.q[..(int)cap(c.deadcode.q)];
    var nq = sort.Search(len(c.deadcode.q), i => c.deadcode.q[i] == null);
    var xq = c.deadcode.q[..(int)nq];
    {
        var i__prev1 = i;

        foreach (var (__i) in xq) {
            i = __i;
            xq[i] = null;
        }
        i = i__prev1;
    }
}

} // end ssa_package
