// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 09:24:13 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\shortcircuit.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // Shortcircuit finds situations where branch directions
        // are always correlated and rewrites the CFG to take
        // advantage of that fact.
        // This optimization is useful for compiling && and || expressions.
        private static void shortcircuit(ref Func f)
        { 
            // Step 1: Replace a phi arg with a constant if that arg
            // is the control value of a preceding If block.
            // b1:
            //    If a goto b2 else b3
            // b2: <- b1 ...
            //    x = phi(a, ...)
            //
            // We can replace the "a" in the phi with the constant true.
            ref Value ct = default;            ref Value cf = default;

            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            if (v.Op != OpPhi)
                            {
                                continue;
                            }
                            if (!v.Type.IsBoolean())
                            {
                                continue;
                            }
                            {
                                var i__prev3 = i;
                                var a__prev3 = a;

                                foreach (var (__i, __a) in v.Args)
                                {
                                    i = __i;
                                    a = __a;
                                    var e = b.Preds[i];
                                    var p = e.b;
                                    if (p.Kind != BlockIf)
                                    {
                                        continue;
                                    }
                                    if (p.Control != a)
                                    {
                                        continue;
                                    }
                                    if (e.i == 0L)
                                    {
                                        if (ct == null)
                                        {
                                            ct = f.ConstBool(f.Entry.Pos, f.Config.Types.Bool, true);
                                        }
                                        v.SetArg(i, ct);
                                    }
                                    else
                                    {
                                        if (cf == null)
                                        {
                                            cf = f.ConstBool(f.Entry.Pos, f.Config.Types.Bool, false);
                                        }
                                        v.SetArg(i, cf);
                                    }
                                }
                                i = i__prev3;
                                a = a__prev3;
                            }

                        }
                        v = v__prev2;
                    }

                }
                b = b__prev1;
            }

            var live = make_slice<bool>(f.NumValues());
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            {
                                var a__prev3 = a;

                                foreach (var (_, __a) in v.Args)
                                {
                                    a = __a;
                                    if (a.Block != v.Block)
                                    {
                                        live[a.ID] = true;
                                    }
                                }
                                a = a__prev3;
                            }

                        }
                        v = v__prev2;
                    }

                    if (b.Control != null && b.Control.Block != b)
                    {
                        live[b.Control.ID] = true;
                    }
                }
                b = b__prev1;
            }

            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    if (b.Kind != BlockIf)
                    {
                        continue;
                    }
                    if (len(b.Values) != 1L)
                    {
                        continue;
                    }
                    var v = b.Values[0L];
                    if (v.Op != OpPhi)
                    {
                        continue;
                    }
                    if (b.Control != v)
                    {
                        continue;
                    }
                    if (live[v.ID])
                    {
                        continue;
                    }
                    {
                        var i__prev2 = i;

                        for (long i = 0L; i < len(v.Args); i++)
                        {
                            var a = v.Args[i];
                            if (a.Op != OpConstBool)
                            {
                                continue;
                            }
                            var e1 = b.Preds[i];
                            p = e1.b;
                            var pi = e1.i; 

                            // The successor we always go to when coming in
                            // from that predecessor.
                            var e2 = b.Succs[1L - a.AuxInt];
                            var t = e2.b;
                            var ti = e2.i; 

                            // Remove b's incoming edge from p.
                            b.removePred(i);
                            var n = len(b.Preds);
                            v.Args[i].Uses--;
                            v.Args[i] = v.Args[n];
                            v.Args[n] = null;
                            v.Args = v.Args[..n]; 

                            // Redirect p's outgoing edge to t.
                            p.Succs[pi] = new Edge(t,len(t.Preds)); 

                            // Fix up t to have one more predecessor.
                            t.Preds = append(t.Preds, new Edge(p,pi));
                            foreach (var (_, w) in t.Values)
                            {
                                if (w.Op != OpPhi)
                                {
                                    continue;
                                }
                                w.AddArg(w.Args[ti]);
                            }                            if (len(b.Preds) == 1L)
                            {
                                v.Op = OpCopy; 
                                // No longer a phi, stop optimizing here.
                                break;
                            }
                            i--;
                        }

                        i = i__prev2;
                    }
                }
                b = b__prev1;
            }

        }
    }
}}}}
