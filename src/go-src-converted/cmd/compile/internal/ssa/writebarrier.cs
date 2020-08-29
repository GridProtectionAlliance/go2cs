// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 09:24:31 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\writebarrier.go
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // needwb returns whether we need write barrier for store op v.
        // v must be Store/Move/Zero.
        private static bool needwb(ref Value v)
        {
            ref types.Type (t, ok) = v.Aux._<ref types.Type>();
            if (!ok)
            {
                v.Fatalf("store aux is not a type: %s", v.LongString());
            }
            if (!t.HasHeapPointer())
            {
                return false;
            }
            if (IsStackAddr(v.Args[0L]))
            {
                return false; // write on stack doesn't need write barrier
            }
            return true;
        }

        // writebarrier pass inserts write barriers for store ops (Store, Move, Zero)
        // when necessary (the condition above). It rewrites store ops to branches
        // and runtime calls, like
        //
        // if writeBarrier.enabled {
        //   writebarrierptr(ptr, val)
        // } else {
        //   *ptr = val
        // }
        //
        // A sequence of WB stores for many pointer fields of a single type will
        // be emitted together, with a single branch.
        private static void writebarrier(ref Func _f) => func(_f, (ref Func f, Defer defer, Panic _, Recover __) =>
        {
            if (!f.fe.UseWriteBarrier())
            {
                return;
            }
            ref Value sb = default;            ref Value sp = default;            ref Value wbaddr = default;            ref Value const0 = default;

            ref obj.LSym writebarrierptr = default;            ref obj.LSym typedmemmove = default;            ref obj.LSym typedmemclr = default;            ref obj.LSym gcWriteBarrier = default;

            slice<ref Value> stores = default;            slice<ref Value> after = default;

            ref sparseSet sset = default;
            slice<int> storeNumber = default;

            foreach (var (_, b) in f.Blocks)
            { // range loop is safe since the blocks we added contain no stores to expand
                // first, identify all the stores that need to insert a write barrier.
                // mark them with WB ops temporarily. record presence of WB ops.
                long nWBops = 0L; // count of temporarily created WB ops remaining to be rewritten in the current block
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in b.Values)
                    {
                        v = __v;

                        if (v.Op == OpStore || v.Op == OpMove || v.Op == OpZero) 
                            if (needwb(v))
                            {

                                if (v.Op == OpStore) 
                                    v.Op = OpStoreWB;
                                else if (v.Op == OpMove) 
                                    v.Op = OpMoveWB;
                                else if (v.Op == OpZero) 
                                    v.Op = OpZeroWB;
                                                                nWBops++;
                            }
                                            }

                    v = v__prev2;
                }

                if (nWBops == 0L)
                {
                    continue;
                }
                if (wbaddr == null)
                { 
                    // lazily initialize global values for write barrier test and calls
                    // find SB and SP values in entry block
                    var initpos = f.Entry.Pos;
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in f.Entry.Values)
                        {
                            v = __v;
                            if (v.Op == OpSB)
                            {
                                sb = v;
                            }
                            if (v.Op == OpSP)
                            {
                                sp = v;
                            }
                            if (sb != null && sp != null)
                            {
                                break;
                            }
                        }

                        v = v__prev2;
                    }

                    if (sb == null)
                    {
                        sb = f.Entry.NewValue0(initpos, OpSB, f.Config.Types.Uintptr);
                    }
                    if (sp == null)
                    {
                        sp = f.Entry.NewValue0(initpos, OpSP, f.Config.Types.Uintptr);
                    }
                    var wbsym = f.fe.Syslook("writeBarrier");
                    wbaddr = f.Entry.NewValue1A(initpos, OpAddr, f.Config.Types.UInt32Ptr, wbsym, sb);
                    writebarrierptr = f.fe.Syslook("writebarrierptr");
                    if (!f.fe.Debug_eagerwb())
                    {
                        gcWriteBarrier = f.fe.Syslook("gcWriteBarrier");
                    }
                    typedmemmove = f.fe.Syslook("typedmemmove");
                    typedmemclr = f.fe.Syslook("typedmemclr");
                    const0 = f.ConstInt32(initpos, f.Config.Types.UInt32, 0L); 

                    // allocate auxiliary data structures for computing store order
                    sset = f.newSparseSet(f.NumValues());
                    defer(f.retSparseSet(sset));
                    storeNumber = make_slice<int>(f.NumValues());
                } 

                // order values in store order
                b.Values = storeOrder(b.Values, sset, storeNumber);

again:
                ref Value last = default;
                long start = default;                long end = default;

                var values = b.Values;
FindSeq:
                for (var i = len(values) - 1L; i >= 0L; i--)
                {
                    var w = values[i];

                    if (w.Op == OpStoreWB || w.Op == OpMoveWB || w.Op == OpZeroWB) 
                        start = i;
                        if (last == null)
                        {
                            last = w;
                            end = i + 1L;
                        }
                    else if (w.Op == OpVarDef || w.Op == OpVarLive || w.Op == OpVarKill) 
                        continue;
                    else 
                        if (last == null)
                        {
                            continue;
                        }
                        _breakFindSeq = true;
                        break;
                                    }
                stores = append(stores[..0L], b.Values[start..end]); // copy to avoid aliasing
                after = append(after[..0L], b.Values[end..]);
                b.Values = b.Values[..start]; 

                // find the memory before the WB stores
                var mem = stores[0L].MemoryArg();
                var pos = stores[0L].Pos;
                var bThen = f.NewBlock(BlockPlain);
                var bElse = f.NewBlock(BlockPlain);
                var bEnd = f.NewBlock(b.Kind);
                bThen.Pos = pos;
                bElse.Pos = pos;
                bEnd.Pos = b.Pos;
                b.Pos = pos; 

                // set up control flow for end block
                bEnd.SetControl(b.Control);
                bEnd.Likely = b.Likely;
                foreach (var (_, e) in b.Succs)
                {
                    bEnd.Succs = append(bEnd.Succs, e);
                    e.b.Preds[e.i].b = bEnd;
                } 

                // set up control flow for write barrier test
                // load word, test word, avoiding partial register write from load byte.
                var cfgtypes = ref f.Config.Types;
                var flag = b.NewValue2(pos, OpLoad, cfgtypes.UInt32, wbaddr, mem);
                flag = b.NewValue2(pos, OpNeq32, cfgtypes.Bool, flag, const0);
                b.Kind = BlockIf;
                b.SetControl(flag);
                b.Likely = BranchUnlikely;
                b.Succs = b.Succs[..0L];
                b.AddEdgeTo(bThen);
                b.AddEdgeTo(bElse); 
                // TODO: For OpStoreWB and the buffered write barrier,
                // we could move the write out of the write barrier,
                // which would lead to fewer branches. We could do
                // something similar to OpZeroWB, since the runtime
                // could provide just the barrier half and then we
                // could unconditionally do an OpZero (which could
                // also generate better zeroing code). OpMoveWB is
                // trickier and would require changing how
                // cgoCheckMemmove works.
                bThen.AddEdgeTo(bEnd);
                bElse.AddEdgeTo(bEnd); 

                // for each write barrier store, append write barrier version to bThen
                // and simple store version to bElse
                var memThen = mem;
                var memElse = mem;
                {
                    var w__prev2 = w;

                    foreach (var (_, __w) in stores)
                    {
                        w = __w;
                        var ptr = w.Args[0L];
                        pos = w.Pos;

                        ref obj.LSym fn = default;
                        ref obj.LSym typ = default;
                        ref Value val = default;

                        if (w.Op == OpStoreWB) 
                            fn = writebarrierptr;
                            val = w.Args[1L];
                            nWBops--;
                        else if (w.Op == OpMoveWB) 
                            fn = typedmemmove;
                            val = w.Args[1L];
                            typ = w.Aux._<ref types.Type>().Symbol();
                            nWBops--;
                        else if (w.Op == OpZeroWB) 
                            fn = typedmemclr;
                            typ = w.Aux._<ref types.Type>().Symbol();
                            nWBops--;
                        else if (w.Op == OpVarDef || w.Op == OpVarLive || w.Op == OpVarKill)                         // then block: emit write barrier call

                        if (w.Op == OpStoreWB || w.Op == OpMoveWB || w.Op == OpZeroWB) 
                            var @volatile = w.Op == OpMoveWB && isVolatile(val);
                            if (w.Op == OpStoreWB && !f.fe.Debug_eagerwb())
                            {
                                memThen = bThen.NewValue3A(pos, OpWB, types.TypeMem, gcWriteBarrier, ptr, val, memThen);
                            }
                            else
                            {
                                memThen = wbcall(pos, bThen, fn, typ, ptr, val, memThen, sp, sb, volatile);
                            }
                        else if (w.Op == OpVarDef || w.Op == OpVarLive || w.Op == OpVarKill) 
                            memThen = bThen.NewValue1A(pos, w.Op, types.TypeMem, w.Aux, memThen);
                        // else block: normal store

                        if (w.Op == OpStoreWB) 
                            memElse = bElse.NewValue3A(pos, OpStore, types.TypeMem, w.Aux, ptr, val, memElse);
                        else if (w.Op == OpMoveWB) 
                            memElse = bElse.NewValue3I(pos, OpMove, types.TypeMem, w.AuxInt, ptr, val, memElse);
                            memElse.Aux = w.Aux;
                        else if (w.Op == OpZeroWB) 
                            memElse = bElse.NewValue2I(pos, OpZero, types.TypeMem, w.AuxInt, ptr, memElse);
                            memElse.Aux = w.Aux;
                        else if (w.Op == OpVarDef || w.Op == OpVarLive || w.Op == OpVarKill) 
                            memElse = bElse.NewValue1A(pos, w.Op, types.TypeMem, w.Aux, memElse);
                                                if (fn != null)
                        { 
                            // Note that we set up a writebarrier function call.
                            f.fe.SetWBPos(pos);
                        }
                    } 

                    // merge memory
                    // Splice memory Phi into the last memory of the original sequence,
                    // which may be used in subsequent blocks. Other memories in the
                    // sequence must be dead after this block since there can be only
                    // one memory live.

                    w = w__prev2;
                }

                bEnd.Values = append(bEnd.Values, last);
                last.Block = bEnd;
                last.reset(OpPhi);
                last.Type = types.TypeMem;
                last.AddArg(memThen);
                last.AddArg(memElse);
                {
                    var w__prev2 = w;

                    foreach (var (_, __w) in stores)
                    {
                        w = __w;
                        if (w != last)
                        {
                            w.resetArgs();
                        }
                    }

                    w = w__prev2;
                }

                {
                    var w__prev2 = w;

                    foreach (var (_, __w) in stores)
                    {
                        w = __w;
                        if (w != last)
                        {
                            f.freeValue(w);
                        }
                    } 

                    // put values after the store sequence into the end block

                    w = w__prev2;
                }

                bEnd.Values = append(bEnd.Values, after);
                {
                    var w__prev2 = w;

                    foreach (var (_, __w) in after)
                    {
                        w = __w;
                        w.Block = bEnd;
                    } 

                    // if we have more stores in this block, do this block again

                    w = w__prev2;
                }

                if (nWBops > 0L)
                {
                    goto again;
                }
            }
        });

        // wbcall emits write barrier runtime call in b, returns memory.
        // if valIsVolatile, it moves val into temp space before making the call.
        private static ref Value wbcall(src.XPos pos, ref Block b, ref obj.LSym fn, ref obj.LSym typ, ref Value ptr, ref Value val, ref Value mem, ref Value sp, ref Value sb, bool valIsVolatile)
        {
            var config = b.Func.Config;

            GCNode tmp = default;
            if (valIsVolatile)
            { 
                // Copy to temp location if the source is volatile (will be clobbered by
                // a function call). Marshaling the args to typedmemmove might clobber the
                // value we're trying to move.
                var t = val.Type.ElemType();
                tmp = b.Func.fe.Auto(val.Pos, t);
                mem = b.NewValue1A(pos, OpVarDef, types.TypeMem, tmp, mem);
                var tmpaddr = b.NewValue1A(pos, OpAddr, t.PtrTo(), tmp, sp);
                var siz = t.Size();
                mem = b.NewValue3I(pos, OpMove, types.TypeMem, siz, tmpaddr, val, mem);
                mem.Aux = t;
                val = tmpaddr;
            } 

            // put arguments on stack
            var off = config.ctxt.FixedFrameSize();

            if (typ != null)
            { // for typedmemmove
                var taddr = b.NewValue1A(pos, OpAddr, b.Func.Config.Types.Uintptr, typ, sb);
                off = round(off, taddr.Type.Alignment());
                var arg = b.NewValue1I(pos, OpOffPtr, taddr.Type.PtrTo(), off, sp);
                mem = b.NewValue3A(pos, OpStore, types.TypeMem, ptr.Type, arg, taddr, mem);
                off += taddr.Type.Size();
            }
            off = round(off, ptr.Type.Alignment());
            arg = b.NewValue1I(pos, OpOffPtr, ptr.Type.PtrTo(), off, sp);
            mem = b.NewValue3A(pos, OpStore, types.TypeMem, ptr.Type, arg, ptr, mem);
            off += ptr.Type.Size();

            if (val != null)
            {
                off = round(off, val.Type.Alignment());
                arg = b.NewValue1I(pos, OpOffPtr, val.Type.PtrTo(), off, sp);
                mem = b.NewValue3A(pos, OpStore, types.TypeMem, val.Type, arg, val, mem);
                off += val.Type.Size();
            }
            off = round(off, config.PtrSize); 

            // issue call
            mem = b.NewValue1A(pos, OpStaticCall, types.TypeMem, fn, mem);
            mem.AuxInt = off - config.ctxt.FixedFrameSize();

            if (valIsVolatile)
            {
                mem = b.NewValue1A(pos, OpVarKill, types.TypeMem, tmp, mem); // mark temp dead
            }
            return mem;
        }

        // round to a multiple of r, r is a power of 2
        private static long round(long o, long r)
        {
            return (o + r - 1L) & ~(r - 1L);
        }

        // IsStackAddr returns whether v is known to be an address of a stack slot
        public static bool IsStackAddr(ref Value v)
        {
            while (v.Op == OpOffPtr || v.Op == OpAddPtr || v.Op == OpPtrIndex || v.Op == OpCopy)
            {
                v = v.Args[0L];
            }


            if (v.Op == OpSP) 
                return true;
            else if (v.Op == OpAddr) 
                return v.Args[0L].Op == OpSP;
                        return false;
        }

        // isVolatile returns whether v is a pointer to argument region on stack which
        // will be clobbered by a function call.
        private static bool isVolatile(ref Value v)
        {
            while (v.Op == OpOffPtr || v.Op == OpAddPtr || v.Op == OpPtrIndex || v.Op == OpCopy)
            {
                v = v.Args[0L];
            }

            return v.Op == OpSP;
        }
    }
}}}}
