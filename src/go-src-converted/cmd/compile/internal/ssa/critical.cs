// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 13 06:00:55 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\critical.go
namespace go.cmd.compile.@internal;

public static partial class ssa_package {

// critical splits critical edges (those that go from a block with
// more than one outedge to a block with more than one inedge).
// Regalloc wants a critical-edge-free CFG so it can implement phi values.
private static void critical(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;
 
    // maps from phi arg ID to the new block created for that argument
    var blocks = make_slice<ptr<Block>>(f.NumValues()); 
    // need to iterate over f.Blocks without range, as we might
    // need to split critical edges on newly constructed blocks
    for (nint j = 0; j < len(f.Blocks); j++) {
        var b = f.Blocks[j];
        if (len(b.Preds) <= 1) {
            continue;
        }
        ptr<Value> phi; 
        // determine if we've only got a single phi in this
        // block, this is easier to handle than the general
        // case of a block with multiple phi values.
        {
            var v__prev2 = v;

            foreach (var (_, __v) in b.Values) {
                v = __v;
                if (v.Op == OpPhi) {
                    if (phi != null) {
                        phi = null;
                        break;
                    }
                    phi = v;
                }
            }
            v = v__prev2;
        }

        if (phi != null) {
            {
                var v__prev2 = v;

                foreach (var (_, __v) in phi.Args) {
                    v = __v;
                    blocks[v.ID] = null;
                }
                v = v__prev2;
            }
        }
        {
            nint i = 0;

            while (i < len(b.Preds)) {
                var e = b.Preds[i];
                var p = e.b;
                var pi = e.i;
                if (p.Kind == BlockPlain) {
                    i++;
                    continue; // only single output block
                }
                ptr<Block> d; // new block used to remove critical edge
                var reusedBlock = false; // if true, then this is not the first use of this block
                if (phi != null) {
                    var argID = phi.Args[i].ID; 
                    // find or record the block that we used to split
                    // critical edges for this argument
                    d = blocks[argID];

                    if (d == null) { 
                        // splitting doesn't necessarily remove the critical edge,
                        // since we're iterating over len(f.Blocks) above, this forces
                        // the new blocks to be re-examined.
                        d = f.NewBlock(BlockPlain);
                        d.Pos = p.Pos;
                        blocks[argID] = d;
                        if (f.pass.debug > 0) {
                            f.Warnl(p.Pos, "split critical edge");
                        }
                    }
                    else
 {
                        reusedBlock = true;
                    }
                }
                else
 { 
                    // no existing block, so allocate a new block
                    // to place on the edge
                    d = f.NewBlock(BlockPlain);
                    d.Pos = p.Pos;
                    if (f.pass.debug > 0) {
                        f.Warnl(p.Pos, "split critical edge");
                    }
                }
                if (reusedBlock) { 
                    // Add p->d edge
                    p.Succs[pi] = new Edge(d,len(d.Preds));
                    d.Preds = append(d.Preds, new Edge(p,pi)); 

                    // Remove p as a predecessor from b.
                    b.removePred(i); 

                    // Update corresponding phi args
                    var n = len(b.Preds);
                    phi.Args[i].Uses--;
                    phi.Args[i] = phi.Args[n];
                    phi.Args[n] = null;
                    phi.Args = phi.Args[..(int)n]; 
                    // splitting occasionally leads to a phi having
                    // a single argument (occurs with -N)
                    if (n == 1) {
                        phi.Op = OpCopy;
                    }
                }
                else
 { 
                    // splice it in
                    p.Succs[pi] = new Edge(d,0);
                    b.Preds[i] = new Edge(d,0);
                    d.Preds = append(d.Preds, new Edge(p,pi));
                    d.Succs = append(d.Succs, new Edge(b,i));
                    i++;
                }
            }
        }
    }
}

} // end ssa_package
