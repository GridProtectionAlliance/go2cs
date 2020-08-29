// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 08:53:53 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\fuse.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // fuse simplifies control flow by joining basic blocks.
        private static void fuse(ref Func f)
        {
            {
                var changed = true;

                while (changed)
                {
                    changed = false; 
                    // Fuse from end to beginning, to avoid quadratic behavior in fuseBlockPlain. See issue 13554.
                    for (var i = len(f.Blocks) - 1L; i >= 0L; i--)
                    {
                        var b = f.Blocks[i];
                        changed = fuseBlockIf(b) || changed;
                        changed = fuseBlockPlain(b) || changed;
                    }
                }
            }
        }

        // fuseBlockIf handles the following cases where s0 and s1 are empty blocks.
        //
        //   b        b        b      b
        //  / \      | \      / |    | |
        // s0  s1    |  s1   s0 |    | |
        //  \ /      | /      \ |    | |
        //   ss      ss        ss     ss
        //
        // If all Phi ops in ss have identical variables for slots corresponding to
        // s0, s1 and b then the branch can be dropped.
        // This optimization often comes up in switch statements with multiple
        // expressions in a case clause:
        //   switch n {
        //     case 1,2,3: return 4
        //   }
        // TODO: If ss doesn't contain any OpPhis, are s0 and s1 dead code anyway.
        private static bool fuseBlockIf(ref Block b)
        {
            if (b.Kind != BlockIf)
            {
                return false;
            }
            ref Block ss0 = default;            ref Block ss1 = default;

            var s0 = b.Succs[0L].b;
            var i0 = b.Succs[0L].i;
            if (s0.Kind != BlockPlain || len(s0.Preds) != 1L || len(s0.Values) != 0L)
            {
                s0 = b;
                ss0 = s0;
            }
            else
            {
                ss0 = s0.Succs[0L].b;
                i0 = s0.Succs[0L].i;
            }
            var s1 = b.Succs[1L].b;
            var i1 = b.Succs[1L].i;
            if (s1.Kind != BlockPlain || len(s1.Preds) != 1L || len(s1.Values) != 0L)
            {
                s1 = b;
                ss1 = s1;
            }
            else
            {
                ss1 = s1.Succs[0L].b;
                i1 = s1.Succs[0L].i;
            }
            if (ss0 != ss1)
            {
                return false;
            }
            var ss = ss0; 

            // s0 and s1 are equal with b if the corresponding block is missing
            // (2nd, 3rd and 4th case in the figure).

            foreach (var (_, v) in ss.Values)
            {
                if (v.Op == OpPhi && v.Uses > 0L && v.Args[i0] != v.Args[i1])
                {
                    return false;
                }
            } 

            // Now we have two of following b->ss, b->s0->ss and b->s1->ss,
            // with s0 and s1 empty if exist.
            // We can replace it with b->ss without if all OpPhis in ss
            // have identical predecessors (verified above).
            // No critical edge is introduced because b will have one successor.
            if (s0 != b && s1 != b)
            { 
                // Replace edge b->s0->ss with b->ss.
                // We need to keep a slot for Phis corresponding to b.
                b.Succs[0L] = new Edge(ss,i0);
                ss.Preds[i0] = new Edge(b,0);
                b.removeEdge(1L);
                s1.removeEdge(0L);
            }
            else if (s0 != b)
            {
                b.removeEdge(0L);
                s0.removeEdge(0L);
            }
            else if (s1 != b)
            {
                b.removeEdge(1L);
                s1.removeEdge(0L);
            }
            else
            {
                b.removeEdge(1L);
            }
            b.Kind = BlockPlain;
            b.SetControl(null); 

            // Trash the empty blocks s0 & s1.
            if (s0 != b)
            {
                s0.Kind = BlockInvalid;
                s0.Values = null;
                s0.Succs = null;
                s0.Preds = null;
            }
            if (s1 != b)
            {
                s1.Kind = BlockInvalid;
                s1.Values = null;
                s1.Succs = null;
                s1.Preds = null;
            }
            return true;
        }

        private static bool fuseBlockPlain(ref Block b)
        {
            if (b.Kind != BlockPlain)
            {
                return false;
            }
            var c = b.Succs[0L].b;
            if (len(c.Preds) != 1L)
            {
                return false;
            } 

            // move all of b's values to c.
            foreach (var (_, v) in b.Values)
            {
                v.Block = c;
            } 
            // Use whichever value slice is larger, in the hopes of avoiding growth.
            // However, take care to avoid c.Values pointing to b.valstorage.
            // See golang.org/issue/18602.
            if (cap(c.Values) >= cap(b.Values) || len(b.Values) <= len(b.valstorage))
            {
                c.Values = append(c.Values, b.Values);
            }
            else
            {
                c.Values = append(b.Values, c.Values);
            } 

            // replace b->c edge with preds(b) -> c
            c.predstorage[0L] = new Edge();
            if (len(b.Preds) > len(b.predstorage))
            {
                c.Preds = b.Preds;
            }
            else
            {
                c.Preds = append(c.predstorage[..0L], b.Preds);
            }
            foreach (var (i, e) in c.Preds)
            {
                var p = e.b;
                p.Succs[e.i] = new Edge(c,i);
            }
            var f = b.Func;
            if (f.Entry == b)
            {
                f.Entry = c;
            }
            f.invalidateCFG(); 

            // trash b, just in case
            b.Kind = BlockInvalid;
            b.Values = null;
            b.Preds = null;
            b.Succs = null;
            return true;
        }
    }
}}}}
