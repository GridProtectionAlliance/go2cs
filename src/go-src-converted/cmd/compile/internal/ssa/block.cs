// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 08:53:12 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\block.go
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // Block represents a basic block in the control flow graph of a function.
        public partial struct Block
        {
            public ID ID; // Source position for block's control operation
            public src.XPos Pos; // The kind of block this is.
            public BlockKind Kind; // Likely direction for branches.
// If BranchLikely, Succs[0] is the most likely branch taken.
// If BranchUnlikely, Succs[1] is the most likely branch taken.
// Ignored if len(Succs) < 2.
// Fatal if not BranchUnknown and len(Succs) > 2.
            public BranchPrediction Likely; // After flagalloc, records whether flags are live at the end of the block.
            public bool FlagsLiveAtEnd; // Subsequent blocks, if any. The number and order depend on the block kind.
            public slice<Edge> Succs; // Inverse of successors.
// The order is significant to Phi nodes in the block.
// TODO: predecessors is a pain to maintain. Can we somehow order phi
// arguments by block id and have this field computed explicitly when needed?
            public slice<Edge> Preds; // A value that determines how the block is exited. Its value depends on the kind
// of the block. For instance, a BlockIf has a boolean control value and BlockExit
// has a memory control value.
            public ptr<Value> Control; // Auxiliary info for the block. Its value depends on the Kind.
            public slice<ref Value> Values; // The containing function
            public ptr<Func> Func; // Storage for Succs, Preds, and Values
            public array<Edge> succstorage;
            public array<Edge> predstorage;
            public array<ref Value> valstorage;
        }

        // Edge represents a CFG edge.
        // Example edges for b branching to either c or d.
        // (c and d have other predecessors.)
        //   b.Succs = [{c,3}, {d,1}]
        //   c.Preds = [?, ?, ?, {b,0}]
        //   d.Preds = [?, {b,1}, ?]
        // These indexes allow us to edit the CFG in constant time.
        // In addition, it informs phi ops in degenerate cases like:
        // b:
        //    if k then c else c
        // c:
        //    v = Phi(x, y)
        // Then the indexes tell you whether x is chosen from
        // the if or else branch from b.
        //   b.Succs = [{c,0},{c,1}]
        //   c.Preds = [{b,0},{b,1}]
        // means x is chosen if k is true.
        public partial struct Edge
        {
            public ptr<Block> b; // index of reverse edge.  Invariant:
//   e := x.Succs[idx]
//   e.b.Preds[e.i] = Edge{x,idx}
// and similarly for predecessors.
            public long i;
        }

        public static ref Block Block(this Edge e)
        {
            return e.b;
        }
        public static long Index(this Edge e)
        {
            return e.i;
        }

        //     kind           control    successors
        //   ------------------------------------------
        //     Exit        return mem                []
        //    Plain               nil            [next]
        //       If   a boolean Value      [then, else]
        //    Defer               mem  [nopanic, panic]  (control opcode should be OpStaticCall to runtime.deferproc)
        public partial struct BlockKind // : sbyte
        {
        }

        // short form print
        private static @string String(this ref Block b)
        {
            return fmt.Sprintf("b%d", b.ID);
        }

        // long form print
        private static @string LongString(this ref Block b)
        {
            var s = b.Kind.String();
            if (b.Aux != null)
            {
                s += fmt.Sprintf(" %s", b.Aux);
            }
            if (b.Control != null)
            {
                s += fmt.Sprintf(" %s", b.Control);
            }
            if (len(b.Succs) > 0L)
            {
                s += " ->";
                foreach (var (_, c) in b.Succs)
                {
                    s += " " + c.b.String();
                }
            }

            if (b.Likely == BranchUnlikely) 
                s += " (unlikely)";
            else if (b.Likely == BranchLikely) 
                s += " (likely)";
                        return s;
        }

        private static void SetControl(this ref Block b, ref Value v)
        {
            {
                var w = b.Control;

                if (w != null)
                {
                    w.Uses--;
                }

            }
            b.Control = v;
            if (v != null)
            {
                v.Uses++;
            }
        }

        // AddEdgeTo adds an edge from block b to block c. Used during building of the
        // SSA graph; do not use on an already-completed SSA graph.
        private static void AddEdgeTo(this ref Block b, ref Block c)
        {
            var i = len(b.Succs);
            var j = len(c.Preds);
            b.Succs = append(b.Succs, new Edge(c,j));
            c.Preds = append(c.Preds, new Edge(b,i));
            b.Func.invalidateCFG();
        }

        // removePred removes the ith input edge from b.
        // It is the responsibility of the caller to remove
        // the corresponding successor edge.
        private static void removePred(this ref Block b, long i)
        {
            var n = len(b.Preds) - 1L;
            if (i != n)
            {
                var e = b.Preds[n];
                b.Preds[i] = e; 
                // Update the other end of the edge we moved.
                e.b.Succs[e.i].i = i;
            }
            b.Preds[n] = new Edge();
            b.Preds = b.Preds[..n];
            b.Func.invalidateCFG();
        }

        // removeSucc removes the ith output edge from b.
        // It is the responsibility of the caller to remove
        // the corresponding predecessor edge.
        private static void removeSucc(this ref Block b, long i)
        {
            var n = len(b.Succs) - 1L;
            if (i != n)
            {
                var e = b.Succs[n];
                b.Succs[i] = e; 
                // Update the other end of the edge we moved.
                e.b.Preds[e.i].i = i;
            }
            b.Succs[n] = new Edge();
            b.Succs = b.Succs[..n];
            b.Func.invalidateCFG();
        }

        private static void swapSuccessors(this ref Block b)
        {
            if (len(b.Succs) != 2L)
            {
                b.Fatalf("swapSuccessors with len(Succs)=%d", len(b.Succs));
            }
            var e0 = b.Succs[0L];
            var e1 = b.Succs[1L];
            b.Succs[0L] = e1;
            b.Succs[1L] = e0;
            e0.b.Preds[e0.i].i = 1L;
            e1.b.Preds[e1.i].i = 0L;
            b.Likely *= -1L;
        }

        // LackingPos indicates whether b is a block whose position should be inherited
        // from its successors.  This is true if all the values within it have unreliable positions
        // and if it is "plain", meaning that there is no control flow that is also very likely
        // to correspond to a well-understood source position.
        private static bool LackingPos(this ref Block b)
        { 
            // Non-plain predecessors are If or Defer, which both (1) have two successors,
            // which might have different line numbers and (2) correspond to statements
            // in the source code that have positions, so this case ought not occur anyway.
            if (b.Kind != BlockPlain)
            {
                return false;
            }
            if (b.Pos != src.NoXPos)
            {
                return false;
            }
            foreach (var (_, v) in b.Values)
            {
                if (v.LackingPos())
                {
                    continue;
                }
                return false;
            }
            return true;
        }

        private static void Logf(this ref Block b, @string msg, params object[] args)
        {
            b.Func.Logf(msg, args);

        }
        private static bool Log(this ref Block b)
        {
            return b.Func.Log();
        }
        private static void Fatalf(this ref Block b, @string msg, params object[] args)
        {
            b.Func.Fatalf(msg, args);

        }

        public partial struct BranchPrediction // : sbyte
        {
        }

        public static readonly var BranchUnlikely = BranchPrediction(-1L);
        public static readonly var BranchUnknown = BranchPrediction(0L);
        public static readonly var BranchLikely = BranchPrediction(+1L);
    }
}}}}
