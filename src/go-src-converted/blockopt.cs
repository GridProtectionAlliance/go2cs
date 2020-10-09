// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 09 06:03:04 UTC
// import "golang.org/x/tools/go/ssa" ==> using ssa = go.golang.org.x.tools.go.ssa_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\blockopt.go
// Simple block optimizations to simplify the control flow graph.

// TODO(adonovan): opt: instead of creating several "unreachable" blocks
// per function in the Builder, reuse a single one (e.g. at Blocks[1])
// to reduce garbage.

using fmt = go.fmt_package;
using os = go.os_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class ssa_package
    {
        // If true, perform sanity checking and show progress at each
        // successive iteration of optimizeBlocks.  Very verbose.
        private static readonly var debugBlockOpt = false;

        // markReachable sets Index=-1 for all blocks reachable from b.


        // markReachable sets Index=-1 for all blocks reachable from b.
        private static void markReachable(ptr<BasicBlock> _addr_b)
        {
            ref BasicBlock b = ref _addr_b.val;

            b.Index = -1L;
            foreach (var (_, succ) in b.Succs)
            {
                if (succ.Index == 0L)
                {
                    markReachable(_addr_succ);
                }

            }

        }

        // deleteUnreachableBlocks marks all reachable blocks of f and
        // eliminates (nils) all others, including possibly cyclic subgraphs.
        //
        private static void deleteUnreachableBlocks(ptr<Function> _addr_f)
        {
            ref Function f = ref _addr_f.val;

            const long white = (long)0L;
            const long black = (long)-1L; 
            // We borrow b.Index temporarily as the mark bit.
 
            // We borrow b.Index temporarily as the mark bit.
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    b.Index = white;
                }

                b = b__prev1;
            }

            markReachable(_addr_f.Blocks[0L]);
            if (f.Recover != null)
            {
                markReachable(_addr_f.Recover);
            }

            {
                var b__prev1 = b;

                foreach (var (__i, __b) in f.Blocks)
                {
                    i = __i;
                    b = __b;
                    if (b.Index == white)
                    {
                        foreach (var (_, c) in b.Succs)
                        {
                            if (c.Index == black)
                            {
                                c.removePred(b); // delete white->black edge
                            }

                        }
                        if (debugBlockOpt)
                        {
                            fmt.Fprintln(os.Stderr, "unreachable", b);
                        }

                        f.Blocks[i] = null; // delete b
                    }

                }

                b = b__prev1;
            }

            f.removeNilBlocks();

        }

        // jumpThreading attempts to apply simple jump-threading to block b,
        // in which a->b->c become a->c if b is just a Jump.
        // The result is true if the optimization was applied.
        //
        private static bool jumpThreading(ptr<Function> _addr_f, ptr<BasicBlock> _addr_b)
        {
            ref Function f = ref _addr_f.val;
            ref BasicBlock b = ref _addr_b.val;

            if (b.Index == 0L)
            {
                return false; // don't apply to entry block
            }

            if (b.Instrs == null)
            {
                return false;
            }

            {
                ptr<Jump> (_, ok) = b.Instrs[0L]._<ptr<Jump>>();

                if (!ok)
                {
                    return false; // not just a jump
                }

            }

            var c = b.Succs[0L];
            if (c == b)
            {
                return false; // don't apply to degenerate jump-to-self.
            }

            if (c.hasPhi())
            {
                return false; // not sound without more effort
            }

            foreach (var (j, a) in b.Preds)
            {
                a.replaceSucc(b, c); 

                // If a now has two edges to c, replace its degenerate If by Jump.
                if (len(a.Succs) == 2L && a.Succs[0L] == c && a.Succs[1L] == c)
                {
                    ptr<Jump> jump = @new<Jump>();
                    jump.setBlock(a);
                    a.Instrs[len(a.Instrs) - 1L] = jump;
                    a.Succs = a.Succs[..1L];
                    c.removePred(b);
                }
                else
                {
                    if (j == 0L)
                    {
                        c.replacePred(b, a);
                    }
                    else
                    {
                        c.Preds = append(c.Preds, a);
                    }

                }

                if (debugBlockOpt)
                {
                    fmt.Fprintln(os.Stderr, "jumpThreading", a, b, c);
                }

            }
            f.Blocks[b.Index] = null; // delete b
            return true;

        }

        // fuseBlocks attempts to apply the block fusion optimization to block
        // a, in which a->b becomes ab if len(a.Succs)==len(b.Preds)==1.
        // The result is true if the optimization was applied.
        //
        private static bool fuseBlocks(ptr<Function> _addr_f, ptr<BasicBlock> _addr_a)
        {
            ref Function f = ref _addr_f.val;
            ref BasicBlock a = ref _addr_a.val;

            if (len(a.Succs) != 1L)
            {
                return false;
            }

            var b = a.Succs[0L];
            if (len(b.Preds) != 1L)
            {
                return false;
            } 

            // Degenerate &&/|| ops may result in a straight-line CFG
            // containing φ-nodes. (Ideally we'd replace such them with
            // their sole operand but that requires Referrers, built later.)
            if (b.hasPhi())
            {
                return false; // not sound without further effort
            } 

            // Eliminate jump at end of A, then copy all of B across.
            a.Instrs = append(a.Instrs[..len(a.Instrs) - 1L], b.Instrs);
            foreach (var (_, instr) in b.Instrs)
            {
                instr.setBlock(a);
            } 

            // A inherits B's successors
            a.Succs = append(a.succs2[..0L], b.Succs); 

            // Fix up Preds links of all successors of B.
            foreach (var (_, c) in b.Succs)
            {
                c.replacePred(b, a);
            }
            if (debugBlockOpt)
            {
                fmt.Fprintln(os.Stderr, "fuseBlocks", a, b);
            }

            f.Blocks[b.Index] = null; // delete b
            return true;

        }

        // optimizeBlocks() performs some simple block optimizations on a
        // completed function: dead block elimination, block fusion, jump
        // threading.
        //
        private static void optimizeBlocks(ptr<Function> _addr_f)
        {
            ref Function f = ref _addr_f.val;

            deleteUnreachableBlocks(_addr_f); 

            // Loop until no further progress.
            var changed = true;
            while (changed)
            {
                changed = false;

                if (debugBlockOpt)
                {
                    f.WriteTo(os.Stderr);
                    mustSanityCheck(f, null);
                }

                foreach (var (_, b) in f.Blocks)
                { 
                    // f.Blocks will temporarily contain nils to indicate
                    // deleted blocks; we remove them at the end.
                    if (b == null)
                    {
                        continue;
                    } 

                    // Fuse blocks.  b->c becomes bc.
                    if (fuseBlocks(_addr_f, _addr_b))
                    {
                        changed = true;
                    } 

                    // a->b->c becomes a->c if b contains only a Jump.
                    if (jumpThreading(_addr_f, _addr_b))
                    {
                        changed = true;
                        continue; // (b was disconnected)
                    }

                }

            }

            f.removeNilBlocks();

        }
    }
}}}}}
