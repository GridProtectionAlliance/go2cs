// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:10:25 UTC
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
        private static void flagalloc(ptr<Func> _addr_f) => func((defer, panic, _) =>
        {
            ref Func f = ref _addr_f.val;
 
            // Compute the in-register flag value we want at the end of
            // each block. This is basically a best-effort live variable
            // analysis, so it can be much simpler than a full analysis.
            var end = make_slice<ptr<Value>>(f.NumBlocks());
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
                        ptr<Value> flag;
                        {
                            var c__prev3 = c;

                            foreach (var (_, __c) in b.ControlValues())
                            {
                                c = __c;
                                if (c.Type.IsFlags())
                                {
                                    if (flag != null)
                                    {
                                        panic("cannot have multiple controls using flags");
                                    }
                                    flag = c;

                                }
                            }
                            c = c__prev3;
                        }

                        if (flag == null)
                        {
                            flag = end[b.ID];
                        }
                        {
                            var j__prev3 = j;

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

                            j = j__prev3;
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
                    if (b.Kind == BlockDefer)
                    { 
                        // Defer blocks internally use/clobber the flags value.
                        end[b.ID] = null;
                        continue;

                    }
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.ControlValues())
                        {
                            v = __v;
                            if (v.Type.IsFlags() && end[b.ID] != v)
                            {
                                end[b.ID] = null;
                            }
                        }
                        v = v__prev2;
                    }
                }
                b = b__prev1;
            }

            map spill = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ID, bool>{};
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    flag = ;
                    if (len(b.Preds) > 0L)
                    {
                        flag = end[b.Preds[0L].b.ID];
                    }
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
                                    if (!a.Type.IsFlags())
                                    {
                                        continue;
                                    }
                                    if (a == flag)
                                    {
                                        continue;
                                    }
                                    spill[a.ID] = true;
                                    flag = a;

                                }
                                a = a__prev3;
                            }

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
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.ControlValues())
                        {
                            v = __v;
                            if (v != flag && v.Type.IsFlags())
                            {
                                spill[v.ID] = true;
                            }
                        }
                        v = v__prev2;
                    }

                    {
                        var v__prev1 = v;

                        v = end[b.ID];

                        if (v != null && v != flag)
                        {
                            spill[v.ID] = true;
                        }
                        v = v__prev1;

                    }

                }
                b = b__prev1;
            }

            slice<ptr<Value>> remove = default; // values that should be checked for possible removal
            slice<ptr<Value>> oldSched = default;
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    oldSched = append(oldSched[..0L], b.Values);
                    b.Values = b.Values[..0L]; 
                    // The current live flag value (the pre-flagalloc copy).
                    flag = ;
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
                            if (spill[v.ID] && v.MemoryArg() != null)
                            {
                                remove = append(remove, v);
                                if (!f.Config.splitLoad(v))
                                {
                                    f.Fatalf("can't split flag generator: %s", v.LongString());
                                }
                            }
                            {
                                var i__prev3 = i;
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
                                    var c = copyFlags(_addr_a, _addr_b); 
                                    // Update v.
                                    v.SetArg(i, c); 
                                    // Remember the most-recently computed flag value.
                                    flag = a;

                                }
                                i = i__prev3;
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
                        var i__prev2 = i;
                        var v__prev2 = v;

                        foreach (var (__i, __v) in b.ControlValues())
                        {
                            i = __i;
                            v = __v;
                            if (v != flag && v.Type.IsFlags())
                            { 
                                // Recalculate control value.
                                remove = append(remove, v);
                                c = copyFlags(_addr_v, _addr_b);
                                b.ReplaceControl(i, c);
                                flag = v;

                            }
                        }
                        i = i__prev2;
                        v = v__prev2;
                    }

                    {
                        var v__prev1 = v;

                        v = end[b.ID];

                        if (v != null && v != flag)
                        { 
                            // Need to reissue flag generator for use by
                            // subsequent blocks.
                            remove = append(remove, v);
                            copyFlags(_addr_v, _addr_b); 
                            // Note: this flag generator is not properly linked up
                            // with the flag users. This breaks the SSA representation.
                            // We could fix up the users with another pass, but for now
                            // we'll just leave it. (Regalloc has the same issue for
                            // standard regs, and it runs next.)
                            // For this reason, take care not to add this flag
                            // generator to the remove list.
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

            const var go115flagallocdeadcode = (var)true;

            if (!go115flagallocdeadcode)
            {
                return ;
            }
            {
                var i__prev1 = i;

                for (long i = 0L; i < len(remove); i++)
                {
                    v = remove[i];
                    if (v.Uses == 0L)
                    {
                        v.reset(OpInvalid);
                        continue;
                    }
                    var last = len(remove) - 1L;
                    remove[i] = remove[last];
                    remove[last] = null;
                    remove = remove[..last];
                    i--; // reprocess value at i
                }

                i = i__prev1;
            }

            if (len(remove) == 0L)
            {
                return ;
            }
            var removeBlocks = f.newSparseSet(f.NumBlocks());
            defer(f.retSparseSet(removeBlocks));
            {
                var v__prev1 = v;

                foreach (var (_, __v) in remove)
                {
                    v = __v;
                    removeBlocks.add(v.Block.ID);
                }
                v = v__prev1;
            }

            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    if (!removeBlocks.contains(b.ID))
                    {
                        continue;
                    }
                    i = 0L;
                    {
                        var j__prev2 = j;

                        for (j = 0L; j < len(b.Values); j++)
                        {
                            v = b.Values[j];
                            if (v.Op == OpInvalid)
                            {
                                continue;
                            }
                            b.Values[i] = v;
                            i++;

                        }

                        j = j__prev2;
                    }
                    b.truncateValues(i);

                }
                b = b__prev1;
            }
        });

        private static bool clobbersFlags(this ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

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
        private static ptr<Value> copyFlags(ptr<Value> _addr_v, ptr<Block> _addr_b)
        {
            ref Value v = ref _addr_v.val;
            ref Block b = ref _addr_b.val;

            var flagsArgs = make_map<long, ptr<Value>>();
            {
                var i__prev1 = i;
                var a__prev1 = a;

                foreach (var (__i, __a) in v.Args)
                {
                    i = __i;
                    a = __a;
                    if (a.Type.IsFlags() || a.Type.IsTuple())
                    {
                        flagsArgs[i] = copyFlags(_addr_a, _addr_b);
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

            return _addr_c!;

        }
    }
}}}}
