// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 08:54:10 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\nilcheck.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // nilcheckelim eliminates unnecessary nil checks.
        // runs on machine-independent code.
        private static void nilcheckelim(ref Func _f) => func(_f, (ref Func f, Defer defer, Panic _, Recover __) =>
        { 
            // A nil check is redundant if the same nil check was successful in a
            // dominating block. The efficacy of this pass depends heavily on the
            // efficacy of the cse pass.
            var sdom = f.sdom(); 

            // TODO: Eliminate more nil checks.
            // We can recursively remove any chain of fixed offset calculations,
            // i.e. struct fields and array elements, even with non-constant
            // indices: x is non-nil iff x.a.b[i].c is.

            private partial struct walkState // : long
            {
            }
            const walkState Work = iota; // process nil checks and traverse to dominees
            const var ClearPtr = 0; // forget the fact that ptr is nil

            private partial struct bp
            {
                public ptr<Block> block; // block, or nil in ClearPtr state
                public ptr<Value> ptr; // if non-nil, ptr that is to be cleared in ClearPtr state
                public walkState op;
            }

            var work = make_slice<bp>(0L, 256L);
            work = append(work, new bp(block:f.Entry)); 

            // map from value ID to bool indicating if value is known to be non-nil
            // in the current dominator path being walked. This slice is updated by
            // walkStates to maintain the known non-nil values.
            var nonNilValues = make_slice<bool>(f.NumValues()); 

            // make an initial pass identifying any non-nil values
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
                            // a value resulting from taking the address of a
                            // value, or a value constructed from an offset of a
                            // non-nil ptr (OpAddPtr) implies it is non-nil
                            if (v.Op == OpAddr || v.Op == OpAddPtr)
                            {
                                nonNilValues[v.ID] = true;
                            }
                        }
                        v = v__prev2;
                    }

                }
                b = b__prev1;
            }

            {
                var changed = true;

                while (changed)
                {
                    changed = false;
                    {
                        var b__prev2 = b;

                        foreach (var (_, __b) in f.Blocks)
                        {
                            b = __b;
                            {
                                var v__prev3 = v;

                                foreach (var (_, __v) in b.Values)
                                {
                                    v = __v; 
                                    // phis whose arguments are all non-nil
                                    // are non-nil
                                    if (v.Op == OpPhi)
                                    {
                                        var argsNonNil = true;
                                        foreach (var (_, a) in v.Args)
                                        {
                                            if (!nonNilValues[a.ID])
                                            {
                                                argsNonNil = false;
                                                break;
                                            }
                                        }                                        if (argsNonNil)
                                        {
                                            if (!nonNilValues[v.ID])
                                            {
                                                changed = true;
                                            }
                                            nonNilValues[v.ID] = true;
                                        }
                                    }
                                }
                                v = v__prev3;
                            }

                        }
                        b = b__prev2;
                    }

                }
            } 

            // allocate auxiliary date structures for computing store order
            var sset = f.newSparseSet(f.NumValues());
            defer(f.retSparseSet(sset));
            var storeNumber = make_slice<int>(f.NumValues()); 

            // perform a depth first walk of the dominee tree
            while (len(work) > 0L)
            {
                var node = work[len(work) - 1L];
                work = work[..len(work) - 1L];


                if (node.op == Work) 
                    var b = node.block; 

                    // First, see if we're dominated by an explicit nil check.
                    if (len(b.Preds) == 1L)
                    {
                        var p = b.Preds[0L].b;
                        if (p.Kind == BlockIf && p.Control.Op == OpIsNonNil && p.Succs[0L].b == b)
                        {
                            var ptr = p.Control.Args[0L];
                            if (!nonNilValues[ptr.ID])
                            {
                                nonNilValues[ptr.ID] = true;
                                work = append(work, new bp(op:ClearPtr,ptr:ptr));
                            }
                        }
                    }
                    b.Values = storeOrder(b.Values, sset, storeNumber); 

                    // Next, process values in the block.
                    long i = 0L;
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            b.Values[i] = v;
                            i++;

                            if (v.Op == OpIsNonNil) 
                                ptr = v.Args[0L];
                                if (nonNilValues[ptr.ID])
                                { 
                                    // This is a redundant explicit nil check.
                                    v.reset(OpConstBool);
                                    v.AuxInt = 1L; // true
                                }
                            else if (v.Op == OpNilCheck) 
                                ptr = v.Args[0L];
                                if (nonNilValues[ptr.ID])
                                { 
                                    // This is a redundant implicit nil check.
                                    // Logging in the style of the former compiler -- and omit line 1,
                                    // which is usually in generated code.
                                    if (f.fe.Debug_checknil() && v.Pos.Line() > 1L)
                                    {
                                        f.Warnl(v.Pos, "removed nil check");
                                    }
                                    v.reset(OpUnknown);
                                    f.freeValue(v);
                                    i--;
                                    continue;
                                }
                                nonNilValues[ptr.ID] = true;
                                work = append(work, new bp(op:ClearPtr,ptr:ptr));
                                                    }
                        v = v__prev2;
                    }

                    for (var j = i; j < len(b.Values); j++)
                    {
                        b.Values[j] = null;
                    }
                    b.Values = b.Values[..i]; 

                    // Add all dominated blocks to the work list.
                    {
                        var w = sdom[node.block.ID].child;

                        while (w != null)
                        {
                            work = append(work, new bp(op:Work,block:w));
                            w = sdom[w.ID].sibling;
                        }
                    }
                else if (node.op == ClearPtr) 
                    nonNilValues[node.ptr.ID] = false;
                    continue;
                            }
        });

        // All platforms are guaranteed to fault if we load/store to anything smaller than this address.
        //
        // This should agree with minLegalPointer in the runtime.
        private static readonly long minZeroPage = 4096L;

        // nilcheckelim2 eliminates unnecessary nil checks.
        // Runs after lowering and scheduling.


        // nilcheckelim2 eliminates unnecessary nil checks.
        // Runs after lowering and scheduling.
        private static void nilcheckelim2(ref Func _f) => func(_f, (ref Func f, Defer defer, Panic _, Recover __) =>
        {
            var unnecessary = f.newSparseSet(f.NumValues());
            defer(f.retSparseSet(unnecessary));
            foreach (var (_, b) in f.Blocks)
            { 
                // Walk the block backwards. Find instructions that will fault if their
                // input pointer is nil. Remove nil checks on those pointers, as the
                // faulting instruction effectively does the nil check for free.
                unnecessary.clear(); 
                // Optimization: keep track of removed nilcheck with smallest index
                var firstToRemove = len(b.Values);
                {
                    var i__prev2 = i;

                    for (var i = len(b.Values) - 1L; i >= 0L; i--)
                    {
                        var v = b.Values[i];
                        if (opcodeTable[v.Op].nilCheck && unnecessary.contains(v.Args[0L].ID))
                        {
                            if (f.fe.Debug_checknil() && v.Pos.Line() > 1L)
                            {
                                f.Warnl(v.Pos, "removed nil check");
                            }
                            v.reset(OpUnknown);
                            firstToRemove = i;
                            continue;
                        }
                        if (v.Type.IsMemory() || v.Type.IsTuple() && v.Type.FieldType(1L).IsMemory())
                        {
                            if (v.Op == OpVarDef || v.Op == OpVarKill || v.Op == OpVarLive)
                            { 
                                // These ops don't really change memory.
                                continue;
                            } 
                            // This op changes memory.  Any faulting instruction after v that
                            // we've recorded in the unnecessary map is now obsolete.
                            unnecessary.clear();
                        } 

                        // Find any pointers that this op is guaranteed to fault on if nil.
                        array<ref Value> ptrstore = new array<ref Value>(2L);
                        var ptrs = ptrstore[..0L];
                        if (opcodeTable[v.Op].faultOnNilArg0)
                        {
                            ptrs = append(ptrs, v.Args[0L]);
                        }
                        if (opcodeTable[v.Op].faultOnNilArg1)
                        {
                            ptrs = append(ptrs, v.Args[1L]);
                        }
                        foreach (var (_, ptr) in ptrs)
                        { 
                            // Check to make sure the offset is small.

                            if (opcodeTable[v.Op].auxType == auxSymOff) 
                                if (v.Aux != null || v.AuxInt < 0L || v.AuxInt >= minZeroPage)
                                {
                                    continue;
                                }
                            else if (opcodeTable[v.Op].auxType == auxSymValAndOff) 
                                var off = ValAndOff(v.AuxInt).Off();
                                if (v.Aux != null || off < 0L || off >= minZeroPage)
                                {
                                    continue;
                                }
                            else if (opcodeTable[v.Op].auxType == auxInt32)                             else if (opcodeTable[v.Op].auxType == auxInt64)                             else if (opcodeTable[v.Op].auxType == auxNone)                             else 
                                v.Fatalf("can't handle aux %s (type %d) yet\n", v.auxString(), int(opcodeTable[v.Op].auxType));
                            // This instruction is guaranteed to fault if ptr is nil.
                            // Any previous nil check op is unnecessary.
                            unnecessary.add(ptr.ID);
                        }
                    } 
                    // Remove values we've clobbered with OpUnknown.


                    i = i__prev2;
                } 
                // Remove values we've clobbered with OpUnknown.
                i = firstToRemove;
                {
                    var j__prev2 = j;

                    for (var j = i; j < len(b.Values); j++)
                    {
                        v = b.Values[j];
                        if (v.Op != OpUnknown)
                        {
                            b.Values[i] = v;
                            i++;
                        }
                    }


                    j = j__prev2;
                }
                {
                    var j__prev2 = j;

                    for (j = i; j < len(b.Values); j++)
                    {
                        b.Values[j] = null;
                    }


                    j = j__prev2;
                }
                b.Values = b.Values[..i]; 

                // TODO: if b.Kind == BlockPlain, start the analysis in the subsequent block to find
                // more unnecessary nil checks.  Would fix test/nilptr3_ssa.go:157.
            }
        });
    }
}}}}
