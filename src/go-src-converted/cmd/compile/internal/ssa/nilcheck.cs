// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 09 05:24:54 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\nilcheck.go
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
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
        private static void nilcheckelim(ptr<Func> _addr_f) => func((defer, _, __) =>
        {
            ref Func f = ref _addr_f.val;
 
            // A nil check is redundant if the same nil check was successful in a
            // dominating block. The efficacy of this pass depends heavily on the
            // efficacy of the cse pass.
            var sdom = f.Sdom(); 

            // TODO: Eliminate more nil checks.
            // We can recursively remove any chain of fixed offset calculations,
            // i.e. struct fields and array elements, even with non-constant
            // indices: x is non-nil iff x.a.b[i].c is.

            private partial struct walkState // : long
            {
            }
            const walkState Work = (walkState)iota; // process nil checks and traverse to dominees
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
                            // We also assume unsafe pointer arithmetic generates non-nil pointers. See #27180.
                            // We assume that SlicePtr is non-nil because we do a bounds check
                            // before the slice access (and all cap>0 slices have a non-nil ptr). See #30366.
                            if (v.Op == OpAddr || v.Op == OpLocalAddr || v.Op == OpAddPtr || v.Op == OpOffPtr || v.Op == OpAdd32 || v.Op == OpAdd64 || v.Op == OpSub32 || v.Op == OpSub64 || v.Op == OpSlicePtr)
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
                        if (p.Kind == BlockIf && p.Controls[0L].Op == OpIsNonNil && p.Succs[0L].b == b)
                        {
                            {
                                var ptr__prev3 = ptr;

                                var ptr = p.Controls[0L].Args[0L];

                                if (!nonNilValues[ptr.ID])
                                {
                                    nonNilValues[ptr.ID] = true;
                                    work = append(work, new bp(op:ClearPtr,ptr:ptr));
                                }
                                ptr = ptr__prev3;

                            }

                        }
                    }
                    b.Values = storeOrder(b.Values, sset, storeNumber);

                    var pendingLines = f.cachedLineStarts; // Holds statement boundaries that need to be moved to a new value/block
                    pendingLines.clear(); 

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
                            {
                                ptr = v.Args[0L];
                                if (nonNilValues[ptr.ID])
                                {
                                    if (v.Pos.IsStmt() == src.PosIsStmt)
                                    { // Boolean true is a terrible statement boundary.
                                        pendingLines.add(v.Pos);
                                        v.Pos = v.Pos.WithNotStmt();

                                    }
                                    v.reset(OpConstBool);
                                    v.AuxInt = 1L; // true
                                }
                                goto __switch_break0;
                            }
                            if (v.Op == OpNilCheck)
                            {
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
                                    if (v.Pos.IsStmt() == src.PosIsStmt)
                                    { // About to lose a statement boundary
                                        pendingLines.add(v.Pos);

                                    }
                                    v.reset(OpUnknown);
                                    f.freeValue(v);
                                    i--;
                                    continue;

                                }
                                nonNilValues[ptr.ID] = true;
                                work = append(work, new bp(op:ClearPtr,ptr:ptr));
                            }
                            // default: 
                                if (v.Pos.IsStmt() != src.PosNotStmt && !isPoorStatementOp(v.Op) && pendingLines.contains(v.Pos))
                                {
                                    v.Pos = v.Pos.WithIsStmt();
                                    pendingLines.remove(v.Pos);
                                }

                            __switch_break0:;

                        }
                        v = v__prev2;
                    }

                    for (long j = 0L; j < i; j++)
                    { // is this an ordering problem?
                        var v = b.Values[j];
                        if (v.Pos.IsStmt() != src.PosNotStmt && !isPoorStatementOp(v.Op) && pendingLines.contains(v.Pos))
                        {
                            v.Pos = v.Pos.WithIsStmt();
                            pendingLines.remove(v.Pos);
                        }
                    }
                    if (pendingLines.contains(b.Pos))
                    {
                        b.Pos = b.Pos.WithIsStmt();
                        pendingLines.remove(b.Pos);
                    }
                    b.truncateValues(i); 

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
        private static readonly long minZeroPage = (long)4096L;

        // faultOnLoad is true if a load to an address below minZeroPage will trigger a SIGSEGV.


        // faultOnLoad is true if a load to an address below minZeroPage will trigger a SIGSEGV.
        private static var faultOnLoad = objabi.GOOS != "aix";

        // nilcheckelim2 eliminates unnecessary nil checks.
        // Runs after lowering and scheduling.
        private static void nilcheckelim2(ptr<Func> _addr_f) => func((defer, _, __) =>
        {
            ref Func f = ref _addr_f.val;

            var unnecessary = f.newSparseMap(f.NumValues()); // map from pointer that will be dereferenced to index of dereferencing value in b.Values[]
            defer(f.retSparseMap(unnecessary));

            var pendingLines = f.cachedLineStarts; // Holds statement boundaries that need to be moved to a new value/block

            foreach (var (_, b) in f.Blocks)
            { 
                // Walk the block backwards. Find instructions that will fault if their
                // input pointer is nil. Remove nil checks on those pointers, as the
                // faulting instruction effectively does the nil check for free.
                unnecessary.clear();
                pendingLines.clear(); 
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
                            // For bug 33724, policy is that we might choose to bump an existing position
                            // off the faulting load/store in favor of the one from the nil check.

                            // Iteration order means that first nilcheck in the chain wins, others
                            // are bumped into the ordinary statement preservation algorithm.
                            var u = b.Values[unnecessary.get(v.Args[0L].ID)];
                            if (!u.Pos.SameFileAndLine(v.Pos))
                            {
                                if (u.Pos.IsStmt() == src.PosIsStmt)
                                {
                                    pendingLines.add(u.Pos);
                                }

                                u.Pos = v.Pos;

                            }
                            else if (v.Pos.IsStmt() == src.PosIsStmt)
                            {
                                pendingLines.add(v.Pos);
                            }

                            v.reset(OpUnknown);
                            firstToRemove = i;
                            continue;

                        }

                        if (v.Type.IsMemory() || v.Type.IsTuple() && v.Type.FieldType(1L).IsMemory())
                        {
                            if (v.Op == OpVarKill || v.Op == OpVarLive || (v.Op == OpVarDef && !v.Aux._<GCNode>().Typ().HasHeapPointer()))
                            { 
                                // These ops don't really change memory.
                                continue; 
                                // Note: OpVarDef requires that the defined variable not have pointers.
                                // We need to make sure that there's no possible faulting
                                // instruction between a VarDef and that variable being
                                // fully initialized. If there was, then anything scanning
                                // the stack during the handling of that fault will see
                                // a live but uninitialized pointer variable on the stack.
                                //
                                // If we have:
                                //
                                //   NilCheck p
                                //   VarDef x
                                //   x = *p
                                //
                                // We can't rewrite that to
                                //
                                //   VarDef x
                                //   NilCheck p
                                //   x = *p
                                //
                                // Particularly, even though *p faults on p==nil, we still
                                // have to do the explicit nil check before the VarDef.
                                // See issue #32288.
                            } 
                            // This op changes memory.  Any faulting instruction after v that
                            // we've recorded in the unnecessary map is now obsolete.
                            unnecessary.clear();

                        } 

                        // Find any pointers that this op is guaranteed to fault on if nil.
                        array<ptr<Value>> ptrstore = new array<ptr<Value>>(2L);
                        var ptrs = ptrstore[..0L];
                        if (opcodeTable[v.Op].faultOnNilArg0 && (faultOnLoad || v.Type.IsMemory()))
                        { 
                            // On AIX, only writing will fault.
                            ptrs = append(ptrs, v.Args[0L]);

                        }

                        if (opcodeTable[v.Op].faultOnNilArg1 && (faultOnLoad || (v.Type.IsMemory() && v.Op != OpPPC64LoweredMove)))
                        { 
                            // On AIX, only writing will fault.
                            // LoweredMove is a special case because it's considered as a "mem" as it stores on arg0 but arg1 is accessed as a load and should be checked.
                            ptrs = append(ptrs, v.Args[1L]);

                        }

                        foreach (var (_, ptr) in ptrs)
                        { 
                            // Check to make sure the offset is small.

                            if (opcodeTable[v.Op].auxType == auxSym) 
                                if (v.Aux != null)
                                {
                                    continue;
                                }

                            else if (opcodeTable[v.Op].auxType == auxSymOff) 
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
                            unnecessary.set(ptr.ID, int32(i), src.NoXPos);

                        }

                    } 
                    // Remove values we've clobbered with OpUnknown.


                    i = i__prev2;
                } 
                // Remove values we've clobbered with OpUnknown.
                i = firstToRemove;
                for (var j = i; j < len(b.Values); j++)
                {
                    v = b.Values[j];
                    if (v.Op != OpUnknown)
                    {
                        if (!notStmtBoundary(v.Op) && pendingLines.contains(v.Pos))
                        { // Late in compilation, so any remaining NotStmt values are probably okay now.
                            v.Pos = v.Pos.WithIsStmt();
                            pendingLines.remove(v.Pos);

                        }

                        b.Values[i] = v;
                        i++;

                    }

                }


                if (pendingLines.contains(b.Pos))
                {
                    b.Pos = b.Pos.WithIsStmt();
                }

                b.truncateValues(i); 

                // TODO: if b.Kind == BlockPlain, start the analysis in the subsequent block to find
                // more unnecessary nil checks.  Would fix test/nilptr3.go:159.
            }

        });
    }
}}}}
