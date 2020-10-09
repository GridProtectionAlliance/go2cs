// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 09 05:24:43 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\fuse.go
using src = go.cmd.@internal.src_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // fuseEarly runs fuse(f, fuseTypePlain|fuseTypeIntInRange).
        private static void fuseEarly(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            fuse(_addr_f, fuseTypePlain | fuseTypeIntInRange);
        }

        // fuseLate runs fuse(f, fuseTypePlain|fuseTypeIf).
        private static void fuseLate(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            fuse(_addr_f, fuseTypePlain | fuseTypeIf);
        }

        private partial struct fuseType // : byte
        {
        }

        private static readonly fuseType fuseTypePlain = (fuseType)1L << (int)(iota);
        private static readonly var fuseTypeIf = 0;
        private static readonly var fuseTypeIntInRange = 1;
        private static readonly var fuseTypeShortCircuit = 2;


        // fuse simplifies control flow by joining basic blocks.
        private static void fuse(ptr<Func> _addr_f, fuseType typ)
        {
            ref Func f = ref _addr_f.val;

            {
                var changed = true;

                while (changed)
                {
                    changed = false; 
                    // Fuse from end to beginning, to avoid quadratic behavior in fuseBlockPlain. See issue 13554.
                    for (var i = len(f.Blocks) - 1L; i >= 0L; i--)
                    {
                        var b = f.Blocks[i];
                        if (typ & fuseTypeIf != 0L)
                        {
                            changed = fuseBlockIf(_addr_b) || changed;
                        }

                        if (typ & fuseTypeIntInRange != 0L)
                        {
                            changed = fuseIntegerComparisons(b) || changed;
                        }

                        if (typ & fuseTypePlain != 0L)
                        {
                            changed = fuseBlockPlain(_addr_b) || changed;
                        }

                        if (typ & fuseTypeShortCircuit != 0L)
                        {
                            changed = shortcircuitBlock(b) || changed;
                        }

                    }

                    if (changed)
                    {
                        f.invalidateCFG();
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
        private static bool fuseBlockIf(ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;

            if (b.Kind != BlockIf)
            {
                return false;
            }

            ptr<Block> ss0;            ptr<Block> ss1;

            var s0 = b.Succs[0L].b;
            var i0 = b.Succs[0L].i;
            if (s0.Kind != BlockPlain || len(s0.Preds) != 1L || !isEmpty(_addr_s0))
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
            if (s1.Kind != BlockPlain || len(s1.Preds) != 1L || !isEmpty(_addr_s1))
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

            {
                var v__prev1 = v;

                foreach (var (_, __v) in ss.Values)
                {
                    v = __v;
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

                v = v__prev1;
            }

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
            b.Likely = BranchUnknown;
            b.ResetControls(); 

            // Trash the empty blocks s0 and s1.
            ref array<ptr<Block>> blocks = ref heap(new array<ptr<Block>>(new ptr<Block>[] { s0, s1 }), out ptr<array<ptr<Block>>> _addr_blocks);
            foreach (var (_, s) in _addr_blocks)
            {
                if (s == b)
                {
                    continue;
                } 
                // Move any (dead) values in s0 or s1 to b,
                // where they will be eliminated by the next deadcode pass.
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in s.Values)
                    {
                        v = __v;
                        v.Block = b;
                    }

                    v = v__prev2;
                }

                b.Values = append(b.Values, s.Values); 
                // Clear s.
                s.Kind = BlockInvalid;
                s.Values = null;
                s.Succs = null;
                s.Preds = null;

            }
            return true;

        }

        // isEmpty reports whether b contains any live values.
        // There may be false positives.
        private static bool isEmpty(ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;

            foreach (var (_, v) in b.Values)
            {
                if (v.Uses > 0L || v.Op.IsCall() || v.Op.HasSideEffects() || v.Type.IsVoid())
                {
                    return false;
                }

            }
            return true;

        }

        private static bool fuseBlockPlain(ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;

            if (b.Kind != BlockPlain)
            {
                return false;
            }

            var c = b.Succs[0L].b;
            if (len(c.Preds) != 1L)
            {
                return false;
            } 

            // If a block happened to end in a statement marker,
            // try to preserve it.
            if (b.Pos.IsStmt() == src.PosIsStmt)
            {
                var l = b.Pos.Line();
                {
                    var v__prev1 = v;

                    foreach (var (_, __v) in c.Values)
                    {
                        v = __v;
                        if (v.Pos.IsStmt() == src.PosNotStmt)
                        {
                            continue;
                        }

                        if (l == v.Pos.Line())
                        {
                            v.Pos = v.Pos.WithIsStmt();
                            l = 0L;
                            break;
                        }

                    }

                    v = v__prev1;
                }

                if (l != 0L && c.Pos.Line() == l)
                {
                    c.Pos = c.Pos.WithIsStmt();
                }

            } 

            // move all of b's values to c.
            {
                var v__prev1 = v;

                foreach (var (_, __v) in b.Values)
                {
                    v = __v;
                    v.Block = c;
                } 
                // Use whichever value slice is larger, in the hopes of avoiding growth.
                // However, take care to avoid c.Values pointing to b.valstorage.
                // See golang.org/issue/18602.
                // It's important to keep the elements in the same order; maintenance of
                // debugging information depends on the order of *Values in Blocks.
                // This can also cause changes in the order (which may affect other
                // optimizations and possibly compiler output) for 32-vs-64 bit compilation
                // platforms (word size affects allocation bucket size affects slice capacity).

                v = v__prev1;
            }

            if (cap(c.Values) >= cap(b.Values) || len(b.Values) <= len(b.valstorage))
            {
                var bl = len(b.Values);
                var cl = len(c.Values);
                slice<ptr<Value>> t = default; // construct t = b.Values followed-by c.Values, but with attention to allocation.
                if (cap(c.Values) < bl + cl)
                { 
                    // reallocate
                    t = make_slice<ptr<Value>>(bl + cl);

                }
                else
                { 
                    // in place.
                    t = c.Values[0L..bl + cl];

                }

                copy(t[bl..], c.Values); // possibly in-place
                c.Values = t;
                copy(c.Values, b.Values);

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

            // trash b, just in case
            b.Kind = BlockInvalid;
            b.Values = null;
            b.Preds = null;
            b.Succs = null;
            return true;

        }
    }
}}}}
