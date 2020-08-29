// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 09:24:26 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\trim.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // trim removes blocks with no code in them.
        // These blocks were inserted to remove critical edges.
        private static void trim(ref Func f)
        {
            long n = 0L;
            foreach (var (_, b) in f.Blocks)
            {
                if (!trimmableBlock(b))
                {
                    f.Blocks[n] = b;
                    n++;
                    continue;
                }
                var p = b.Preds[0L].b;
                var i = b.Preds[0L].i;
                var s = b.Succs[0L].b;
                var j = b.Succs[0L].i;
                var ns = len(s.Preds);
                p.Succs[i] = new Edge(s,j);
                s.Preds[j] = new Edge(p,i);

                foreach (var (_, e) in b.Preds[1L..])
                {
                    p = e.b;
                    i = e.i;
                    p.Succs[i] = new Edge(s,len(s.Preds));
                    s.Preds = append(s.Preds, new Edge(p,i));
                }                if (ns > 1L)
                {
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in s.Values)
                        {
                            v = __v;
                            if (v.Op == OpPhi)
                            {
                                mergePhi(v, j, b);
                            }
                        }
                        v = v__prev2;
                    }

                    long k = 0L;
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            if (v.Op == OpPhi)
                            {
                                if (v.Uses == 0L)
                                {
                                    v.resetArgs();
                                    continue;
                                }
                                var args = make_slice<ref Value>(len(v.Args));
                                copy(args, v.Args);
                                v.resetArgs();
                                {
                                    long x__prev3 = x;

                                    for (long x = 0L; x < j; x++)
                                    {
                                        v.AddArg(v);
                                    }

                                    x = x__prev3;
                                }
                                v.AddArg(args[0L]);
                                {
                                    long x__prev3 = x;

                                    for (x = j + 1L; x < ns; x++)
                                    {
                                        v.AddArg(v);
                                    }

                                    x = x__prev3;
                                }
                                foreach (var (_, a) in args[1L..])
                                {
                                    v.AddArg(a);
                                }
                            }
                            b.Values[k] = v;
                            k++;
                        }
                        v = v__prev2;
                    }

                    b.Values = b.Values[..k];
                }
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in b.Values)
                    {
                        v = __v;
                        v.Block = s;
                    }
                    v = v__prev2;
                }

                k = len(b.Values);
                var m = len(s.Values);
                {
                    var i__prev2 = i;

                    for (i = 0L; i < k; i++)
                    {
                        s.Values = append(s.Values, null);
                    }

                    i = i__prev2;
                }
                copy(s.Values[k..], s.Values[..m]);
                copy(s.Values, b.Values);
            }            if (n < len(f.Blocks))
            {
                f.invalidateCFG();
                var tail = f.Blocks[n..];
                {
                    var i__prev1 = i;

                    foreach (var (__i) in tail)
                    {
                        i = __i;
                        tail[i] = null;
                    }
                    i = i__prev1;
                }

                f.Blocks = f.Blocks[..n];
            }
        }

        // emptyBlock returns true if the block does not contain actual
        // instructions
        private static bool emptyBlock(ref Block b)
        {
            foreach (var (_, v) in b.Values)
            {
                if (v.Op != OpPhi)
                {
                    return false;
                }
            }
            return true;
        }

        // trimmableBlock returns true if the block can be trimmed from the CFG,
        // subject to the following criteria:
        //  - it should not be the first block
        //  - it should be BlockPlain
        //  - it should not loop back to itself
        //  - it either is the single predecessor of the successor block or
        //    contains no actual instructions
        private static bool trimmableBlock(ref Block b)
        {
            if (b.Kind != BlockPlain || b == b.Func.Entry)
            {
                return false;
            }
            var s = b.Succs[0L].b;
            return s != b && (len(s.Preds) == 1L || emptyBlock(b));
        }

        // mergePhi adjusts the number of `v`s arguments to account for merge
        // of `b`, which was `i`th predecessor of the `v`s block.
        private static void mergePhi(ref Value v, long i, ref Block b)
        {
            var u = v.Args[i];
            if (u.Block == b)
            {
                if (u.Op != OpPhi)
                {
                    b.Func.Fatalf("value %s is not a phi operation", u.LongString());
                } 
                // If the original block contained u = φ(u0, u1, ..., un) and
                // the current phi is
                //    v = φ(v0, v1, ..., u, ..., vk)
                // then the merged phi is
                //    v = φ(v0, v1, ..., u0, ..., vk, u1, ..., un)
                v.SetArg(i, u.Args[0L]);
                v.AddArgs(u.Args[1L..]);
            }
            else
            { 
                // If the original block contained u = φ(u0, u1, ..., un) and
                // the current phi is
                //    v = φ(v0, v1, ...,  vi, ..., vk)
                // i.e. it does not use a value from the predecessor block,
                // then the merged phi is
                //    v = φ(v0, v1, ..., vk, vi, vi, ...)
                for (long j = 1L; j < len(b.Preds); j++)
                {
                    v.AddArg(v.Args[i]);
                }

            }
        }
    }
}}}}
