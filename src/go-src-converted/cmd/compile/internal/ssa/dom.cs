// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 13 06:01:11 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\dom.go
namespace go.cmd.compile.@internal;

using System;
public static partial class ssa_package {

// This file contains code to compute the dominator tree
// of a control-flow graph.

// postorder computes a postorder traversal ordering for the
// basic blocks in f. Unreachable blocks will not appear.
private static slice<ptr<Block>> postorder(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    return postorderWithNumbering(_addr_f, null);
}

private partial struct blockAndIndex {
    public ptr<Block> b;
    public nint index; // index is the number of successor edges of b that have already been explored.
}

// postorderWithNumbering provides a DFS postordering.
// This seems to make loop-finding more robust.
private static slice<ptr<Block>> postorderWithNumbering(ptr<Func> _addr_f, slice<int> ponums) {
    ref Func f = ref _addr_f.val;

    var seen = make_slice<bool>(f.NumBlocks()); 

    // result ordering
    var order = make_slice<ptr<Block>>(0, len(f.Blocks)); 

    // stack of blocks and next child to visit
    // A constant bound allows this to be stack-allocated. 32 is
    // enough to cover almost every postorderWithNumbering call.
    var s = make_slice<blockAndIndex>(0, 32);
    s = append(s, new blockAndIndex(b:f.Entry));
    seen[f.Entry.ID] = true;
    while (len(s) > 0) {
        var tos = len(s) - 1;
        var x = s[tos];
        var b = x.b;
        {
            var i = x.index;

            if (i < len(b.Succs)) {
                s[tos].index++;
                var bb = b.Succs[i].Block();
                if (!seen[bb.ID]) {
                    seen[bb.ID] = true;
                    s = append(s, new blockAndIndex(b:bb));
                }
                continue;
            }

        }
        s = s[..(int)tos];
        if (ponums != null) {
            ponums[b.ID] = int32(len(order));
        }
        order = append(order, b);
    }
    return order;
}

public delegate  slice<Edge> linkedBlocks(ptr<Block>);

private static readonly nint nscratchslices = 7;

// experimentally, functions with 512 or fewer blocks account
// for 75% of memory (size) allocation for dominator computation
// in make.bash.


// experimentally, functions with 512 or fewer blocks account
// for 75% of memory (size) allocation for dominator computation
// in make.bash.
private static readonly nint minscratchblocks = 512;



private static (slice<ID>, slice<ID>, slice<ID>, slice<ID>, slice<ID>, slice<ID>, slice<ID>) scratchBlocksForDom(this ptr<Cache> _addr_cache, nint maxBlockID) {
    slice<ID> a = default;
    slice<ID> b = default;
    slice<ID> c = default;
    slice<ID> d = default;
    slice<ID> e = default;
    slice<ID> f = default;
    slice<ID> g = default;
    ref Cache cache = ref _addr_cache.val;

    var tot = maxBlockID * nscratchslices;
    var scratch = cache.domblockstore;
    if (len(scratch) < tot) { 
        // req = min(1.5*tot, nscratchslices*minscratchblocks)
        // 50% padding allows for graph growth in later phases.
        var req = (tot * 3) >> 1;
        if (req < nscratchslices * minscratchblocks) {
            req = nscratchslices * minscratchblocks;
        }
        scratch = make_slice<ID>(req);
        cache.domblockstore = scratch;
    }
    else
 { 
        // Clear as much of scratch as we will (re)use
        scratch = scratch[(int)0..(int)tot];
        foreach (var (i) in scratch) {
            scratch[i] = 0;
        }
    }
    a = scratch[(int)0 * maxBlockID..(int)1 * maxBlockID];
    b = scratch[(int)1 * maxBlockID..(int)2 * maxBlockID];
    c = scratch[(int)2 * maxBlockID..(int)3 * maxBlockID];
    d = scratch[(int)3 * maxBlockID..(int)4 * maxBlockID];
    e = scratch[(int)4 * maxBlockID..(int)5 * maxBlockID];
    f = scratch[(int)5 * maxBlockID..(int)6 * maxBlockID];
    g = scratch[(int)6 * maxBlockID..(int)7 * maxBlockID];

    return ;
}

private static slice<ptr<Block>> dominators(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    Func<ptr<Block>, slice<Edge>> preds = b => b.Preds;
    Func<ptr<Block>, slice<Edge>> succs = b => b.Succs; 

    //TODO: benchmark and try to find criteria for swapping between
    // dominatorsSimple and dominatorsLT
    return f.dominatorsLTOrig(f.Entry, preds, succs);
}

// dominatorsLTOrig runs Lengauer-Tarjan to compute a dominator tree starting at
// entry and using predFn/succFn to find predecessors/successors to allow
// computing both dominator and post-dominator trees.
private static slice<ptr<Block>> dominatorsLTOrig(this ptr<Func> _addr_f, ptr<Block> _addr_entry, linkedBlocks predFn, linkedBlocks succFn) {
    ref Func f = ref _addr_f.val;
    ref Block entry = ref _addr_entry.val;
 
    // Adapted directly from the original TOPLAS article's "simple" algorithm

    var maxBlockID = entry.Func.NumBlocks();
    var (semi, vertex, label, parent, ancestor, bucketHead, bucketLink) = f.Cache.scratchBlocksForDom(maxBlockID); 

    // This version uses integers for most of the computation,
    // to make the work arrays smaller and pointer-free.
    // fromID translates from ID to *Block where that is needed.
    var fromID = make_slice<ptr<Block>>(maxBlockID);
    {
        var v__prev1 = v;

        foreach (var (_, __v) in f.Blocks) {
            v = __v;
            fromID[v.ID] = v;
        }
        v = v__prev1;
    }

    var idom = make_slice<ptr<Block>>(maxBlockID); 

    // Step 1. Carry out a depth first search of the problem graph. Number
    // the vertices from 1 to n as they are reached during the search.
    var n = f.dfsOrig(entry, succFn, semi, vertex, label, parent);

    {
        var i__prev1 = i;

        for (var i = n; i >= 2; i--) {
            var w = vertex[i]; 

            // step2 in TOPLAS paper
            foreach (var (_, e) in predFn(fromID[w])) {
                var v = e.b;
                if (semi[v.ID] == 0) { 
                    // skip unreachable predecessor
                    // not in original, but we're using existing pred instead of building one.
                    continue;
                }
                var u = evalOrig(v.ID, ancestor, semi, label);
                if (semi[u] < semi[w]) {
                    semi[w] = semi[u];
                }
            } 

            // add w to bucket[vertex[semi[w]]]
            // implement bucket as a linked list implemented
            // in a pair of arrays.
            var vsw = vertex[semi[w]];
            bucketLink[w] = bucketHead[vsw];
            bucketHead[vsw] = w;

            linkOrig(parent[w], w, ancestor); 

            // step3 in TOPLAS paper
            {
                var v__prev2 = v;

                v = bucketHead[parent[w]];

                while (v != 0) {
                    u = evalOrig(v, ancestor, semi, label);
                    if (semi[u] < semi[v]) {
                        idom[v] = fromID[u];
                    v = bucketLink[v];
                    }
                    else
 {
                        idom[v] = fromID[parent[w]];
                    }
                }


                v = v__prev2;
            }
        }

        i = i__prev1;
    } 
    // step 4 in toplas paper
    {
        var i__prev1 = i;

        for (i = ID(2); i <= n; i++) {
            w = vertex[i];
            if (idom[w].ID != vertex[semi[w]]) {
                idom[w] = idom[idom[w].ID];
            }
        }

        i = i__prev1;
    }

    return idom;
}

// dfs performs a depth first search over the blocks starting at entry block
// (in arbitrary order).  This is a de-recursed version of dfs from the
// original Tarjan-Lengauer TOPLAS article.  It's important to return the
// same values for parent as the original algorithm.
private static ID dfsOrig(this ptr<Func> _addr_f, ptr<Block> _addr_entry, linkedBlocks succFn, slice<ID> semi, slice<ID> vertex, slice<ID> label, slice<ID> parent) {
    ref Func f = ref _addr_f.val;
    ref Block entry = ref _addr_entry.val;

    var n = ID(0);
    var s = make_slice<ptr<Block>>(0, 256);
    s = append(s, entry);

    while (len(s) > 0) {
        var v = s[len(s) - 1];
        s = s[..(int)len(s) - 1]; 
        // recursing on v

        if (semi[v.ID] != 0) {
            continue; // already visited
        }
        n++;
        semi[v.ID] = n;
        vertex[n] = v.ID;
        label[v.ID] = v.ID; 
        // ancestor[v] already zero
        foreach (var (_, e) in succFn(v)) {
            var w = e.b; 
            // if it has a dfnum, we've already visited it
            if (semi[w.ID] == 0) { 
                // yes, w can be pushed multiple times.
                s = append(s, w);
                parent[w.ID] = v.ID; // keep overwriting this till it is visited.
            }
        }
    }
    return n;
}

// compressOrig is the "simple" compress function from LT paper
private static void compressOrig(ID v, slice<ID> ancestor, slice<ID> semi, slice<ID> label) {
    if (ancestor[ancestor[v]] != 0) {
        compressOrig(ancestor[v], ancestor, semi, label);
        if (semi[label[ancestor[v]]] < semi[label[v]]) {
            label[v] = label[ancestor[v]];
        }
        ancestor[v] = ancestor[ancestor[v]];
    }
}

// evalOrig is the "simple" eval function from LT paper
private static ID evalOrig(ID v, slice<ID> ancestor, slice<ID> semi, slice<ID> label) {
    if (ancestor[v] == 0) {
        return v;
    }
    compressOrig(v, ancestor, semi, label);
    return label[v];
}

private static void linkOrig(ID v, ID w, slice<ID> ancestor) {
    ancestor[w] = v;
}

// dominators computes the dominator tree for f. It returns a slice
// which maps block ID to the immediate dominator of that block.
// Unreachable blocks map to nil. The entry block maps to nil.
private static slice<ptr<Block>> dominatorsSimple(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;
 
    // A simple algorithm for now
    // Cooper, Harvey, Kennedy
    var idom = make_slice<ptr<Block>>(f.NumBlocks()); 

    // Compute postorder walk
    var post = f.postorder(); 

    // Make map from block id to order index (for intersect call)
    var postnum = make_slice<nint>(f.NumBlocks());
    {
        var i__prev1 = i;
        var b__prev1 = b;

        foreach (var (__i, __b) in post) {
            i = __i;
            b = __b;
            postnum[b.ID] = i;
        }
        i = i__prev1;
        b = b__prev1;
    }

    idom[f.Entry.ID] = f.Entry;
    if (postnum[f.Entry.ID] != len(post) - 1) {
        f.Fatalf("entry block %v not last in postorder", f.Entry);
    }
    while (true) {
        var changed = false;

        {
            var i__prev2 = i;

            for (var i = len(post) - 2; i >= 0; i--) {
                var b = post[i];
                ptr<Block> d;
                foreach (var (_, e) in b.Preds) {
                    var p = e.b;
                    if (idom[p.ID] == null) {
                        continue;
                    }
                    if (d == null) {
                        d = p;
                        continue;
                    }
                    d = intersect(d, _addr_p, postnum, idom);
                }
                if (d != idom[b.ID]) {
                    idom[b.ID] = d;
                    changed = true;
                }
            }


            i = i__prev2;
        }
        if (!changed) {
            break;
        }
    } 
    // Set idom of entry block to nil instead of itself.
    idom[f.Entry.ID] = null;
    return idom;
}

// intersect finds the closest dominator of both b and c.
// It requires a postorder numbering of all the blocks.
private static ptr<Block> intersect(ptr<Block> _addr_b, ptr<Block> _addr_c, slice<nint> postnum, slice<ptr<Block>> idom) {
    ref Block b = ref _addr_b.val;
    ref Block c = ref _addr_c.val;
 
    // TODO: This loop is O(n^2). It used to be used in nilcheck,
    // see BenchmarkNilCheckDeep*.
    while (b != c) {
        if (postnum[b.ID] < postnum[c.ID]) {
            b = idom[b.ID];
        }
        else
 {
            c = idom[c.ID];
        }
    }
    return _addr_b!;
}

} // end ssa_package
