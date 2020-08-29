// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 08:53:48 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\flagalloc.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // flagalloc allocates the flag register among all the flag-generating
        // instructions. Flag values are recomputed if they need to be
        // spilled/restored.
        private static void flagalloc(ref Func f)
        { 
            // Compute the in-register flag value we want at the end of
            // each block. This is basically a best-effort live variable
            // analysis, so it can be much simpler than a full analysis.
            var end = make_slice<ref Value>(f.NumBlocks());
            var po = f.postorder();
            for (long n = 0L; n < 2L; n++)
            {
                {
                    var b__prev2 = b;

                    foreach (var (_, __b) in po)
                    {
                        b = __b; 
                        // Walk values backwards to figure out what flag
                        // value we want in the flag register at the start
                        // of the block.
                        var flag = end[b.ID];
                        if (b.Control != null && b.Control.Type.IsFlags())
                        {
                            flag = b.Control;
                        }
                        for (var j = len(b.Values) - 1L; j >= 0L; j--)
                        {
                            var v = b.Values[j];
                            if (v == flag)
                            {
                                flag = null;
                            }
                            if (v.clobbersFlags())
                            {
                                flag = null;
                            }
                            {
                                var a__prev4 = a;

                                foreach (var (_, __a) in v.Args)
                                {
                                    a = __a;
                                    if (a.Type.IsFlags())
                                    {
                                        flag = a;
                                    }
                                }
                                a = a__prev4;
                            }

                        }
                        if (flag != null)
                        {
                            {
                                var e__prev3 = e;

                                foreach (var (_, __e) in b.Preds)
                                {
                                    e = __e;
                                    var p = e.b;
                                    end[p.ID] = flag;
                                }
                                e = e__prev3;
                            }

                        }
                    }
                    b = b__prev2;
                }

            } 

            // For blocks which have a flags control value, that's the only value
            // we can leave in the flags register at the end of the block. (There
            // is no place to put a flag regeneration instruction.)
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    v = b.Control;
                    if (v != null && v.Type.IsFlags() && end[b.ID] != v)
                    {
                        end[b.ID] = null;
                    }
                    if (b.Kind == BlockDefer)
                    { 
                        // Defer blocks internally use/clobber the flags value.
                        end[b.ID] = null;
                    }
                }
                b = b__prev1;
            }

            slice<ref Value> oldSched = default;
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    oldSched = append(oldSched[..0L], b.Values);
                    b.Values = b.Values[..0L]; 
                    // The current live flag value the pre-flagalloc copy).
                    flag = default;
                    if (len(b.Preds) > 0L)
                    {
                        flag = end[b.Preds[0L].b.ID]; 
                        // Note: the following condition depends on the lack of critical edges.
                        {
                            var e__prev2 = e;

                            foreach (var (_, __e) in b.Preds[1L..])
                            {
                                e = __e;
                                p = e.b;
                                if (end[p.ID] != flag)
                                {
                                    f.Fatalf("live flag in %s's predecessors not consistent", b);
                                }
                            }
                            e = e__prev2;
                        }

                    }
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in oldSched)
                        {
                            v = __v;
                            if (v.Op == OpPhi && v.Type.IsFlags())
                            {
                                f.Fatalf("phi of flags not supported: %s", v.LongString());
                            }
                            {
                                var a__prev3 = a;

                                foreach (var (__i, __a) in v.Args)
                                {
                                    i = __i;
                                    a = __a;
                                    if (!a.Type.IsFlags())
                                    {
                                        continue;
                                    }
                                    if (a == flag)
                                    {
                                        continue;
                                    }
                                    var c = copyFlags(a, b); 
                                    // Update v.
                                    v.SetArg(i, c); 
                                    // Remember the most-recently computed flag value.
                                    flag = a;
                                }
                                a = a__prev3;
                            }

                            b.Values = append(b.Values, v);
                            if (v.clobbersFlags())
                            {
                                flag = null;
                            }
                            if (v.Type.IsFlags())
                            {
                                flag = v;
                            }
                        }
                        v = v__prev2;
                    }

                    {
                        var v__prev1 = v;

                        v = b.Control;

                        if (v != null && v != flag && v.Type.IsFlags())
                        { 
                            // Recalculate control value.
                            c = v.copyInto(b);
                            b.SetControl(c);
                            flag = v;
                        }
                        v = v__prev1;

                    }
                    {
                        var v__prev1 = v;

                        v = end[b.ID];

                        if (v != null && v != flag)
                        { 
                            // Need to reissue flag generator for use by
                            // subsequent blocks.
                            copyFlags(v, b); 
                            // Note: this flag generator is not properly linked up
                            // with the flag users. This breaks the SSA representation.
                            // We could fix up the users with another pass, but for now
                            // we'll just leave it.  (Regalloc has the same issue for
                            // standard regs, and it runs next.)
                        }
                        v = v__prev1;

                    }
                }
                b = b__prev1;
            }

            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    b.FlagsLiveAtEnd = end[b.ID] != null;
                }
                b = b__prev1;
            }

        }

        private static bool clobbersFlags(this ref Value v)
        {
            if (opcodeTable[v.Op].clobberFlags)
            {
                return true;
            }
            if (v.Type.IsTuple() && (v.Type.FieldType(0L).IsFlags() || v.Type.FieldType(1L).IsFlags()))
            { 
                // This case handles the possibility where a flag value is generated but never used.
                // In that case, there's no corresponding Select to overwrite the flags value,
                // so we must consider flags clobbered by the tuple-generating instruction.
                return true;
            }
            return false;
        }

        // copyFlags copies v (flag generator) into b, returns the copy.
        // If v's arg is also flags, copy recursively.
        private static ref Value copyFlags(ref Value v, ref Block b)
        {
            var flagsArgs = make_map<long, ref Value>();
            {
                var i__prev1 = i;
                var a__prev1 = a;

                foreach (var (__i, __a) in v.Args)
                {
                    i = __i;
                    a = __a;
                    if (a.Type.IsFlags() || a.Type.IsTuple())
                    {
                        flagsArgs[i] = copyFlags(a, b);
                    }
                }

                i = i__prev1;
                a = a__prev1;
            }

            var c = v.copyInto(b);
            {
                var i__prev1 = i;
                var a__prev1 = a;

                foreach (var (__i, __a) in flagsArgs)
                {
                    i = __i;
                    a = __a;
                    c.SetArg(i, a);
                }

                i = i__prev1;
                a = a__prev1;
            }

            return c;
        }
    }
}}}}
