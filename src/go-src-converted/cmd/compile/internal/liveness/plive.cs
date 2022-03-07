// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Garbage collector liveness bitmap generation.

// The command line flag -live causes this code to print debug information.
// The levels are:
//
//    -live (aka -live=1): print liveness lists as code warnings at safe points
//    -live=2: print an assembly listing with liveness annotations
//
// Each level includes the earlier output as well.

// package liveness -- go2cs converted at 2022 March 06 23:10:44 UTC
// import "cmd/compile/internal/liveness" ==> using liveness = go.cmd.compile.@internal.liveness_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\liveness\plive.go
using md5 = go.crypto.md5_package;
using sha1 = go.crypto.sha1_package;
using fmt = go.fmt_package;
using os = go.os_package;
using sort = go.sort_package;
using strings = go.strings_package;

using abi = go.cmd.compile.@internal.abi_package;
using @base = go.cmd.compile.@internal.@base_package;
using bitvec = go.cmd.compile.@internal.bitvec_package;
using ir = go.cmd.compile.@internal.ir_package;
using objw = go.cmd.compile.@internal.objw_package;
using reflectdata = go.cmd.compile.@internal.reflectdata_package;
using ssa = go.cmd.compile.@internal.ssa_package;
using typebits = go.cmd.compile.@internal.typebits_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class liveness_package {

    // OpVarDef is an annotation for the liveness analysis, marking a place
    // where a complete initialization (definition) of a variable begins.
    // Since the liveness analysis can see initialization of single-word
    // variables quite easy, OpVarDef is only needed for multi-word
    // variables satisfying isfat(n.Type). For simplicity though, buildssa
    // emits OpVarDef regardless of variable width.
    //
    // An 'OpVarDef x' annotation in the instruction stream tells the liveness
    // analysis to behave as though the variable x is being initialized at that
    // point in the instruction stream. The OpVarDef must appear before the
    // actual (multi-instruction) initialization, and it must also appear after
    // any uses of the previous value, if any. For example, if compiling:
    //
    //    x = x[1:]
    //
    // it is important to generate code like:
    //
    //    base, len, cap = pieces of x[1:]
    //    OpVarDef x
    //    x = {base, len, cap}
    //
    // If instead the generated code looked like:
    //
    //    OpVarDef x
    //    base, len, cap = pieces of x[1:]
    //    x = {base, len, cap}
    //
    // then the liveness analysis would decide the previous value of x was
    // unnecessary even though it is about to be used by the x[1:] computation.
    // Similarly, if the generated code looked like:
    //
    //    base, len, cap = pieces of x[1:]
    //    x = {base, len, cap}
    //    OpVarDef x
    //
    // then the liveness analysis will not preserve the new value of x, because
    // the OpVarDef appears to have "overwritten" it.
    //
    // OpVarDef is a bit of a kludge to work around the fact that the instruction
    // stream is working on single-word values but the liveness analysis
    // wants to work on individual variables, which might be multi-word
    // aggregates. It might make sense at some point to look into letting
    // the liveness analysis work on single-word values as well, although
    // there are complications around interface values, slices, and strings,
    // all of which cannot be treated as individual words.
    //
    // OpVarKill is the opposite of OpVarDef: it marks a value as no longer needed,
    // even if its address has been taken. That is, an OpVarKill annotation asserts
    // that its argument is certainly dead, for use when the liveness analysis
    // would not otherwise be able to deduce that fact.

    // TODO: get rid of OpVarKill here. It's useful for stack frame allocation
    // so the compiler can allocate two temps to the same location. Here it's now
    // useless, since the implementation of stack objects.

    // blockEffects summarizes the liveness effects on an SSA block.
private partial struct blockEffects {
    public bitvec.BitVec uevar;
    public bitvec.BitVec varkill; // Computed during Liveness.solve using control flow information:
//
//    livein: variables live at block entry
//    liveout: variables live at block exit
    public bitvec.BitVec livein;
    public bitvec.BitVec liveout;
}

// A collection of global state used by liveness analysis.
private partial struct liveness {
    public ptr<ir.Func> fn;
    public ptr<ssa.Func> f;
    public slice<ptr<ir.Name>> vars;
    public map<ptr<ir.Name>, int> idx;
    public long stkptrsize;
    public slice<blockEffects> be; // allUnsafe indicates that all points in this function are
// unsafe-points.
    public bool allUnsafe; // unsafePoints bit i is set if Value ID i is an unsafe-point
// (preemption is not allowed). Only valid if !allUnsafe.
    public bitvec.BitVec unsafePoints; // An array with a bit vector for each safe point in the
// current Block during liveness.epilogue. Indexed in Value
// order for that block. Additionally, for the entry block
// livevars[0] is the entry bitmap. liveness.compact moves
// these to stackMaps.
    public slice<bitvec.BitVec> livevars; // livenessMap maps from safe points (i.e., CALLs) to their
// liveness map indexes.
    public Map livenessMap;
    public bvecSet stackMapSet;
    public slice<bitvec.BitVec> stackMaps;
    public progeffectscache cache; // partLiveArgs includes input arguments (PPARAM) that may
// be partially live. That is, it is considered live because
// a part of it is used, but we may not initialize all parts.
    public map<ptr<ir.Name>, bool> partLiveArgs;
    public bool doClobber; // Whether to clobber dead stack slots in this function.
    public bool noClobberArgs; // Do not clobber function arguments
}

// Map maps from *ssa.Value to LivenessIndex.
public partial struct Map {
    public map<ssa.ID, objw.LivenessIndex> Vals; // The set of live, pointer-containing variables at the DeferReturn
// call (only set when open-coded defers are used).
    public objw.LivenessIndex DeferReturn;
}

private static void reset(this ptr<Map> _addr_m) {
    ref Map m = ref _addr_m.val;

    if (m.Vals == null) {
        m.Vals = make_map<ssa.ID, objw.LivenessIndex>();
    }
    else
 {
        foreach (var (k) in m.Vals) {
            delete(m.Vals, k);
        }
    }
    m.DeferReturn = objw.LivenessDontCare;

}

private static void set(this ptr<Map> _addr_m, ptr<ssa.Value> _addr_v, objw.LivenessIndex i) {
    ref Map m = ref _addr_m.val;
    ref ssa.Value v = ref _addr_v.val;

    m.Vals[v.ID] = i;
}

public static objw.LivenessIndex Get(this Map m, ptr<ssa.Value> _addr_v) {
    ref ssa.Value v = ref _addr_v.val;
 
    // If v isn't in the map, then it's a "don't care" and not an
    // unsafe-point.
    {
        var (idx, ok) = m.Vals[v.ID];

        if (ok) {
            return idx;
        }
    }

    return new objw.LivenessIndex(StackMapIndex:objw.StackMapDontCare,IsUnsafePoint:false);

}

private partial struct progeffectscache {
    public slice<int> retuevar;
    public slice<int> tailuevar;
    public bool initialized;
}

// shouldTrack reports whether the liveness analysis
// should track the variable n.
// We don't care about variables that have no pointers,
// nor do we care about non-local variables,
// nor do we care about empty structs (handled by the pointer check),
// nor do we care about the fake PAUTOHEAP variables.
private static bool shouldTrack(ptr<ir.Name> _addr_n) {
    ref ir.Name n = ref _addr_n.val;

    return (n.Class == ir.PAUTO && n.Esc() != ir.EscHeap || n.Class == ir.PPARAM || n.Class == ir.PPARAMOUT) && n.Type().HasPointers();
}

// getvariables returns the list of on-stack variables that we need to track
// and a map for looking up indices by *Node.
private static (slice<ptr<ir.Name>>, map<ptr<ir.Name>, int>) getvariables(ptr<ir.Func> _addr_fn) {
    slice<ptr<ir.Name>> _p0 = default;
    map<ptr<ir.Name>, int> _p0 = default;
    ref ir.Func fn = ref _addr_fn.val;

    slice<ptr<ir.Name>> vars = default;
    {
        var n__prev1 = n;

        foreach (var (_, __n) in fn.Dcl) {
            n = __n;
            if (shouldTrack(_addr_n)) {
                vars = append(vars, n);
            }
        }
        n = n__prev1;
    }

    var idx = make_map<ptr<ir.Name>, int>(len(vars));
    {
        var n__prev1 = n;

        foreach (var (__i, __n) in vars) {
            i = __i;
            n = __n;
            idx[n] = int32(i);
        }
        n = n__prev1;
    }

    return (vars, idx);

}

private static void initcache(this ptr<liveness> _addr_lv) {
    ref liveness lv = ref _addr_lv.val;

    if (lv.cache.initialized) {
        @base.Fatalf("liveness cache initialized twice");
        return ;
    }
    lv.cache.initialized = true;

    foreach (var (i, node) in lv.vars) {

        if (node.Class == ir.PPARAM) 
            // A return instruction with a p.to is a tail return, which brings
            // the stack pointer back up (if it ever went down) and then jumps
            // to a new function entirely. That form of instruction must read
            // all the parameters for correctness, and similarly it must not
            // read the out arguments - they won't be set until the new
            // function runs.
            lv.cache.tailuevar = append(lv.cache.tailuevar, int32(i));
        else if (node.Class == ir.PPARAMOUT) 
            // All results are live at every return point.
            // Note that this point is after escaping return values
            // are copied back to the stack using their PAUTOHEAP references.
            lv.cache.retuevar = append(lv.cache.retuevar, int32(i));
        
    }
}

// A liveEffect is a set of flags that describe an instruction's
// liveness effects on a variable.
//
// The possible flags are:
//    uevar - used by the instruction
//    varkill - killed by the instruction (set)
// A kill happens after the use (for an instruction that updates a value, for example).
private partial struct liveEffect { // : nint
}

private static readonly liveEffect uevar = 1 << (int)(iota);
private static readonly var varkill = 0;


// valueEffects returns the index of a variable in lv.vars and the
// liveness effects v has on that variable.
// If v does not affect any tracked variables, it returns -1, 0.
private static (int, liveEffect) valueEffects(this ptr<liveness> _addr_lv, ptr<ssa.Value> _addr_v) {
    int _p0 = default;
    liveEffect _p0 = default;
    ref liveness lv = ref _addr_lv.val;
    ref ssa.Value v = ref _addr_v.val;

    var (n, e) = affectedVar(_addr_v);
    if (e == 0 || n == null) { // cheapest checks first
        return (-1, 0);

    }

    if (v.Op == ssa.OpVarDef || v.Op == ssa.OpVarKill || v.Op == ssa.OpVarLive || v.Op == ssa.OpKeepAlive) 
        if (!n.Used()) {
            return (-1, 0);
        }
        if (n.Class == ir.PPARAM && !n.Addrtaken() && n.Type().Width > int64(types.PtrSize)) { 
        // Only aggregate-typed arguments that are not address-taken can be
        // partially live.
        lv.partLiveArgs[n] = true;

    }
    liveEffect effect = default; 
    // Read is a read, obviously.
    //
    // Addr is a read also, as any subsequent holder of the pointer must be able
    // to see all the values (including initialization) written so far.
    // This also prevents a variable from "coming back from the dead" and presenting
    // stale pointers to the garbage collector. See issue 28445.
    if (e & (ssa.SymRead | ssa.SymAddr) != 0) {
        effect |= uevar;
    }
    if (e & ssa.SymWrite != 0 && (!isfat(_addr_n.Type()) || v.Op == ssa.OpVarDef)) {
        effect |= varkill;
    }
    if (effect == 0) {
        return (-1, 0);
    }
    {
        var (pos, ok) = lv.idx[n];

        if (ok) {
            return (pos, effect);
        }
    }

    return (-1, 0);

}

// affectedVar returns the *ir.Name node affected by v
private static (ptr<ir.Name>, ssa.SymEffect) affectedVar(ptr<ssa.Value> _addr_v) {
    ptr<ir.Name> _p0 = default!;
    ssa.SymEffect _p0 = default;
    ref ssa.Value v = ref _addr_v.val;
 
    // Special cases.

    if (v.Op == ssa.OpLoadReg) 
        var (n, _) = ssa.AutoVar(v.Args[0]);
        return (_addr_n!, ssa.SymRead);
    else if (v.Op == ssa.OpStoreReg) 
        (n, _) = ssa.AutoVar(v);
        return (_addr_n!, ssa.SymWrite);
    else if (v.Op == ssa.OpArgIntReg) 
        // This forces the spill slot for the register to be live at function entry.
        // one of the following holds for a function F with pointer-valued register arg X:
        //  0. No GC (so an uninitialized spill slot is okay)
        //  1. GC at entry of F.  GC is precise, but the spills around morestack initialize X's spill slot
        //  2. Stack growth at entry of F.  Same as GC.
        //  3. GC occurs within F itself.  This has to be from preemption, and thus GC is conservative.
        //     a. X is in a register -- then X is seen, and the spill slot is also scanned conservatively.
        //     b. X is spilled -- the spill slot is initialized, and scanned conservatively
        //     c. X is not live -- the spill slot is scanned conservatively, and it may contain X from an earlier spill.
        //  4. GC within G, transitively called from F
        //    a. X is live at call site, therefore is spilled, to its spill slot (which is live because of subsequent LoadReg).
        //    b. X is not live at call site -- but neither is its spill slot.
        (n, _) = ssa.AutoVar(v);
        return (_addr_n!, ssa.SymRead);
    else if (v.Op == ssa.OpVarLive) 
        return (v.Aux._<ptr<ir.Name>>(), ssa.SymRead);
    else if (v.Op == ssa.OpVarDef || v.Op == ssa.OpVarKill) 
        return (v.Aux._<ptr<ir.Name>>(), ssa.SymWrite);
    else if (v.Op == ssa.OpKeepAlive) 
        (n, _) = ssa.AutoVar(v.Args[0]);
        return (_addr_n!, ssa.SymRead);
        var e = v.Op.SymEffect();
    if (e == 0) {
        return (_addr_null!, 0);
    }
    switch (v.Aux.type()) {
        case ptr<obj.LSym> a:
            return (_addr_null!, e);
            break;
        case ptr<ir.Name> a:
            return (_addr_a!, e);
            break;
        default:
        {
            var a = v.Aux.type();
            @base.Fatalf("weird aux: %s", v.LongString());
            return (_addr_null!, e);
            break;
        }
    }

}

private partial struct livenessFuncCache {
    public slice<blockEffects> be;
    public Map livenessMap;
}

// Constructs a new liveness structure used to hold the global state of the
// liveness computation. The cfg argument is a slice of *BasicBlocks and the
// vars argument is a slice of *Nodes.
private static ptr<liveness> newliveness(ptr<ir.Func> _addr_fn, ptr<ssa.Func> _addr_f, slice<ptr<ir.Name>> vars, map<ptr<ir.Name>, int> idx, long stkptrsize) {
    ref ir.Func fn = ref _addr_fn.val;
    ref ssa.Func f = ref _addr_f.val;

    ptr<liveness> lv = addr(new liveness(fn:fn,f:f,vars:vars,idx:idx,stkptrsize:stkptrsize,)); 

    // Significant sources of allocation are kept in the ssa.Cache
    // and reused. Surprisingly, the bit vectors themselves aren't
    // a major source of allocation, but the liveness maps are.
    {
        ptr<livenessFuncCache> (lc, _) = f.Cache.Liveness._<ptr<livenessFuncCache>>();

        if (lc == null) { 
            // Prep the cache so liveness can fill it later.
            f.Cache.Liveness = @new<livenessFuncCache>();

        }
        else
 {
            if (cap(lc.be) >= f.NumBlocks()) {
                lv.be = lc.be[..(int)f.NumBlocks()];
            }
            lv.livenessMap = new Map(Vals:lc.livenessMap.Vals,DeferReturn:objw.LivenessDontCare);
            lc.livenessMap.Vals = null;
        }
    }

    if (lv.be == null) {
        lv.be = make_slice<blockEffects>(f.NumBlocks());
    }
    var nblocks = int32(len(f.Blocks));
    var nvars = int32(len(vars));
    var bulk = bitvec.NewBulk(nvars, nblocks * 7);
    foreach (var (_, b) in f.Blocks) {
        var be = lv.blockEffects(b);

        be.uevar = bulk.Next();
        be.varkill = bulk.Next();
        be.livein = bulk.Next();
        be.liveout = bulk.Next();
    }    lv.livenessMap.reset();

    lv.markUnsafePoints();

    lv.partLiveArgs = make_map<ptr<ir.Name>, bool>();

    lv.enableClobber();

    return _addr_lv!;

}

private static ptr<blockEffects> blockEffects(this ptr<liveness> _addr_lv, ptr<ssa.Block> _addr_b) {
    ref liveness lv = ref _addr_lv.val;
    ref ssa.Block b = ref _addr_b.val;

    return _addr__addr_lv.be[b.ID]!;
}

// Generates live pointer value maps for arguments and local variables. The
// this argument and the in arguments are always assumed live. The vars
// argument is a slice of *Nodes.
private static void pointerMap(this ptr<liveness> _addr_lv, bitvec.BitVec liveout, slice<ptr<ir.Name>> vars, bitvec.BitVec args, bitvec.BitVec locals) {
    ref liveness lv = ref _addr_lv.val;

    for (var i = int32(0); >>MARKER:FOREXPRESSION_LEVEL_1<<; i++) {
        i = liveout.Next(i);
        if (i < 0) {
            break;
        }
        var node = vars[i];

        if (node.Class == ir.PPARAM || node.Class == ir.PPARAMOUT)
        {
            if (!node.IsOutputParamInRegisters()) {
                if (node.FrameOffset() < 0) {
                    lv.f.Fatalf("Node %v has frameoffset %d\n", node.Sym().Name, node.FrameOffset());
                }
                typebits.Set(node.Type(), node.FrameOffset(), args);
                break;
            }
            fallthrough = true; // PPARAMOUT in registers acts memory-allocates like an AUTO
        }
        if (fallthrough || node.Class == ir.PAUTO)
        {
            typebits.Set(node.Type(), node.FrameOffset() + lv.stkptrsize, locals);
            goto __switch_break0;
        }

        __switch_break0:;

    }

}

// IsUnsafe indicates that all points in this function are
// unsafe-points.
public static bool IsUnsafe(ptr<ssa.Func> _addr_f) {
    ref ssa.Func f = ref _addr_f.val;
 
    // The runtime assumes the only safe-points are function
    // prologues (because that's how it used to be). We could and
    // should improve that, but for now keep consider all points
    // in the runtime unsafe. obj will add prologues and their
    // safe-points.
    //
    // go:nosplit functions are similar. Since safe points used to
    // be coupled with stack checks, go:nosplit often actually
    // means "no safe points in this function".
    return @base.Flag.CompilingRuntime || f.NoSplit;

}

// markUnsafePoints finds unsafe points and computes lv.unsafePoints.
private static void markUnsafePoints(this ptr<liveness> _addr_lv) {
    ref liveness lv = ref _addr_lv.val;

    if (IsUnsafe(_addr_lv.f)) { 
        // No complex analysis necessary.
        lv.allUnsafe = true;
        return ;

    }
    lv.unsafePoints = bitvec.New(int32(lv.f.NumValues())); 

    // Mark architecture-specific unsafe points.
    {
        var b__prev1 = b;

        foreach (var (_, __b) in lv.f.Blocks) {
            b = __b;
            {
                var v__prev2 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    if (v.Op.UnsafePoint()) {
                        lv.unsafePoints.Set(int32(v.ID));
                    }
                }

                v = v__prev2;
            }
        }
        b = b__prev1;
    }

    foreach (var (_, wbBlock) in lv.f.WBLoads) {
        if (wbBlock.Kind == ssa.BlockPlain && len(wbBlock.Values) == 0) { 
            // The write barrier block was optimized away
            // but we haven't done dead block elimination.
            // (This can happen in -N mode.)
            continue;

        }
        if (len(wbBlock.Succs) != 2) {
            lv.f.Fatalf("expected branch at write barrier block %v", wbBlock);
        }
        var s0 = wbBlock.Succs[0].Block();
        var s1 = wbBlock.Succs[1].Block();
        if (s0 == s1) { 
            // There's no difference between write barrier on and off.
            // Thus there's no unsafe locations. See issue 26024.
            continue;

        }
        if (s0.Kind != ssa.BlockPlain || s1.Kind != ssa.BlockPlain) {
            lv.f.Fatalf("expected successors of write barrier block %v to be plain", wbBlock);
        }
        if (s0.Succs[0].Block() != s1.Succs[0].Block()) {
            lv.f.Fatalf("expected successors of write barrier block %v to converge", wbBlock);
        }
        ptr<ssa.Value> load;
        var v = wbBlock.Controls[0];
        while (true) {
            {
                ptr<obj.LSym> (sym, ok) = v.Aux._<ptr<obj.LSym>>();

                if (ok && sym == ir.Syms.WriteBarrier) {
                    load = v;
                    break;
                }

            }


            if (v.Op == ssa.Op386TESTL) 
                // 386 lowers Neq32 to (TESTL cond cond),
                if (v.Args[0] == v.Args[1]) {
                    v = v.Args[0];
                    continue;
                }
            else if (v.Op == ssa.Op386MOVLload || v.Op == ssa.OpARM64MOVWUload || v.Op == ssa.OpPPC64MOVWZload || v.Op == ssa.OpWasmI64Load32U) 
                // Args[0] is the address of the write
                // barrier control. Ignore Args[1],
                // which is the mem operand.
                // TODO: Just ignore mem operands?
                v = v.Args[0];
                continue;
            // Common case: just flow backwards.
            if (len(v.Args) != 1) {
                v.Fatalf("write barrier control value has more than one argument: %s", v.LongString());
            }

            v = v.Args[0];

        } 

        // Mark everything after the load unsafe.
        var found = false;
        {
            var v__prev2 = v;

            foreach (var (_, __v) in wbBlock.Values) {
                v = __v;
                found = found || v == load;
                if (found) {
                    lv.unsafePoints.Set(int32(v.ID));
                }
            } 

            // Mark the two successor blocks unsafe. These come
            // back together immediately after the direct write in
            // one successor and the last write barrier call in
            // the other, so there's no need to be more precise.

            v = v__prev2;
        }

        foreach (var (_, succ) in wbBlock.Succs) {
            {
                var v__prev3 = v;

                foreach (var (_, __v) in succ.Block().Values) {
                    v = __v;
                    lv.unsafePoints.Set(int32(v.ID));
                }

                v = v__prev3;
            }
        }
    }    bitvec.BitVec flooded = default;
    Action<ptr<ssa.Block>, nint> flood = default;
    flood = (b, vi) => {
        if (flooded.N == 0) {
            flooded = bitvec.New(int32(lv.f.NumBlocks()));
        }
        if (flooded.Get(int32(b.ID))) {
            return ;
        }
        {
            var i__prev1 = i;

            for (var i = vi - 1; i >= 0; i--) {
                v = b.Values[i];
                if (v.Op.IsCall()) { 
                    // Uintptrs must not contain live
                    // pointers across calls, so stop
                    // flooding.
                    return ;

                }

                lv.unsafePoints.Set(int32(v.ID));

            }


            i = i__prev1;
        }
        if (vi == len(b.Values)) { 
            // We marked all values in this block, so no
            // need to flood this block again.
            flooded.Set(int32(b.ID));

        }
        foreach (var (_, pred) in b.Preds) {
            flood(pred.Block(), len(pred.Block().Values));
        }
    };
    {
        var b__prev1 = b;

        foreach (var (_, __b) in lv.f.Blocks) {
            b = __b;
            {
                var i__prev2 = i;
                var v__prev2 = v;

                foreach (var (__i, __v) in b.Values) {
                    i = __i;
                    v = __v;
                    if (!(v.Op == ssa.OpConvert && v.Type.IsPtrShaped())) {
                        continue;
                    } 
                    // Flood the unsafe-ness of this backwards
                    // until we hit a call.
                    flood(b, i + 1);

                }

                i = i__prev2;
                v = v__prev2;
            }
        }
        b = b__prev1;
    }
}

// Returns true for instructions that must have a stack map.
//
// This does not necessarily mean the instruction is a safe-point. In
// particular, call Values can have a stack map in case the callee
// grows the stack, but not themselves be a safe-point.
private static bool hasStackMap(this ptr<liveness> _addr_lv, ptr<ssa.Value> _addr_v) {
    ref liveness lv = ref _addr_lv.val;
    ref ssa.Value v = ref _addr_v.val;

    if (!v.Op.IsCall()) {
        return false;
    }
    {
        ptr<ssa.AuxCall> (sym, ok) = v.Aux._<ptr<ssa.AuxCall>>();

        if (ok && (sym.Fn == ir.Syms.Typedmemclr || sym.Fn == ir.Syms.Typedmemmove)) {
            return false;
        }
    }

    return true;

}

// Initializes the sets for solving the live variables. Visits all the
// instructions in each basic block to summarizes the information at each basic
// block
private static void prologue(this ptr<liveness> _addr_lv) {
    ref liveness lv = ref _addr_lv.val;

    lv.initcache();

    foreach (var (_, b) in lv.f.Blocks) {
        var be = lv.blockEffects(b); 

        // Walk the block instructions backward and update the block
        // effects with the each prog effects.
        for (var j = len(b.Values) - 1; j >= 0; j--) {
            var (pos, e) = lv.valueEffects(b.Values[j]);
            if (e & varkill != 0) {
                be.varkill.Set(pos);
                be.uevar.Unset(pos);
            }
            if (e & uevar != 0) {
                be.uevar.Set(pos);
            }
        }

    }
}

// Solve the liveness dataflow equations.
private static void solve(this ptr<liveness> _addr_lv) {
    ref liveness lv = ref _addr_lv.val;
 
    // These temporary bitvectors exist to avoid successive allocations and
    // frees within the loop.
    var nvars = int32(len(lv.vars));
    var newlivein = bitvec.New(nvars);
    var newliveout = bitvec.New(nvars); 

    // Walk blocks in postorder ordering. This improves convergence.
    var po = lv.f.Postorder(); 

    // Iterate through the blocks in reverse round-robin fashion. A work
    // queue might be slightly faster. As is, the number of iterations is
    // so low that it hardly seems to be worth the complexity.

    {
        var change = true;

        while (change) {
            change = false;
            foreach (var (_, b) in po) {
                var be = lv.blockEffects(b);

                newliveout.Clear();

                if (b.Kind == ssa.BlockRet) 
                    {
                        var pos__prev3 = pos;

                        foreach (var (_, __pos) in lv.cache.retuevar) {
                            pos = __pos;
                            newliveout.Set(pos);
                        }

                        pos = pos__prev3;
                    }
                else if (b.Kind == ssa.BlockRetJmp) 
                    {
                        var pos__prev3 = pos;

                        foreach (var (_, __pos) in lv.cache.tailuevar) {
                            pos = __pos;
                            newliveout.Set(pos);
                        }

                        pos = pos__prev3;
                    }
                else if (b.Kind == ssa.BlockExit)                 else 
                    // A variable is live on output from this block
                    // if it is live on input to some successor.
                    //
                    // out[b] = \bigcup_{s \in succ[b]} in[s]
                    newliveout.Copy(lv.blockEffects(b.Succs[0].Block()).livein);
                    foreach (var (_, succ) in b.Succs[(int)1..]) {
                        newliveout.Or(newliveout, lv.blockEffects(succ.Block()).livein);
                    }
                                if (!be.liveout.Eq(newliveout)) {
                    change = true;
                    be.liveout.Copy(newliveout);
                } 

                // A variable is live on input to this block
                // if it is used by this block, or live on output from this block and
                // not set by the code in this block.
                //
                // in[b] = uevar[b] \cup (out[b] \setminus varkill[b])
                newlivein.AndNot(be.liveout, be.varkill);
                be.livein.Or(newlivein, be.uevar);

            }

        }
    }

}

// Visits all instructions in a basic block and computes a bit vector of live
// variables at each safe point locations.
private static void epilogue(this ptr<liveness> _addr_lv) {
    ref liveness lv = ref _addr_lv.val;

    var nvars = int32(len(lv.vars));
    var liveout = bitvec.New(nvars);
    var livedefer = bitvec.New(nvars); // always-live variables

    // If there is a defer (that could recover), then all output
    // parameters are live all the time.  In addition, any locals
    // that are pointers to heap-allocated output parameters are
    // also always live (post-deferreturn code needs these
    // pointers to copy values back to the stack).
    // TODO: if the output parameter is heap-allocated, then we
    // don't need to keep the stack copy live?
    if (lv.fn.HasDefer()) {
        {
            var i__prev1 = i;
            var n__prev1 = n;

            foreach (var (__i, __n) in lv.vars) {
                i = __i;
                n = __n;
                if (n.Class == ir.PPARAMOUT) {
                    if (n.IsOutputParamHeapAddr()) { 
                        // Just to be paranoid.  Heap addresses are PAUTOs.
                        @base.Fatalf("variable %v both output param and heap output param", n);

                    }

                    if (n.Heapaddr != null) { 
                        // If this variable moved to the heap, then
                        // its stack copy is not live.
                        continue;

                    } 
                    // Note: zeroing is handled by zeroResults in walk.go.
                    livedefer.Set(int32(i));

                }

                if (n.IsOutputParamHeapAddr()) { 
                    // This variable will be overwritten early in the function
                    // prologue (from the result of a mallocgc) but we need to
                    // zero it in case that malloc causes a stack scan.
                    n.SetNeedzero(true);
                    livedefer.Set(int32(i));

                }

                if (n.OpenDeferSlot()) { 
                    // Open-coded defer args slots must be live
                    // everywhere in a function, since a panic can
                    // occur (almost) anywhere. Because it is live
                    // everywhere, it must be zeroed on entry.
                    livedefer.Set(int32(i)); 
                    // It was already marked as Needzero when created.
                    if (!n.Needzero()) {
                        @base.Fatalf("all pointer-containing defer arg slots should have Needzero set");
                    }

                }

            }

            i = i__prev1;
            n = n__prev1;
        }
    }
    if (lv.f.Entry != lv.f.Blocks[0]) {
        lv.f.Fatalf("entry block must be first");
    }
 { 
        // Reserve an entry for function entry.
        var live = bitvec.New(nvars);
        lv.livevars = append(lv.livevars, live);

    }    foreach (var (_, b) in lv.f.Blocks) {
        var be = lv.blockEffects(b); 

        // Walk forward through the basic block instructions and
        // allocate liveness maps for those instructions that need them.
        {
            var v__prev2 = v;

            foreach (var (_, __v) in b.Values) {
                v = __v;
                if (!lv.hasStackMap(v)) {
                    continue;
                }
                live = bitvec.New(nvars);
                lv.livevars = append(lv.livevars, live);
            } 

            // walk backward, construct maps at each safe point

            v = v__prev2;
        }

        var index = int32(len(lv.livevars) - 1);

        liveout.Copy(be.liveout);
        {
            var i__prev2 = i;

            for (var i = len(b.Values) - 1; i >= 0; i--) {
                var v = b.Values[i];

                if (lv.hasStackMap(v)) { 
                    // Found an interesting instruction, record the
                    // corresponding liveness information.

                    live = _addr_lv.livevars[index];
                    live.Or(live.val, liveout);
                    live.Or(live.val, livedefer); // only for non-entry safe points
                    index--;

                } 

                // Update liveness information.
                var (pos, e) = lv.valueEffects(v);
                if (e & varkill != 0) {
                    liveout.Unset(pos);
                }

                if (e & uevar != 0) {
                    liveout.Set(pos);
                }

            }


            i = i__prev2;
        }

        if (b == lv.f.Entry) {
            if (index != 0) {
                @base.Fatalf("bad index for entry point: %v", index);
            } 

            // Check to make sure only input variables are live.
            {
                var i__prev2 = i;
                var n__prev2 = n;

                foreach (var (__i, __n) in lv.vars) {
                    i = __i;
                    n = __n;
                    if (!liveout.Get(int32(i))) {
                        continue;
                    }
                    if (n.Class == ir.PPARAM) {
                        continue; // ok
                    }

                    @base.FatalfAt(n.Pos(), "bad live variable at entry of %v: %L", lv.fn.Nname, n);

                } 

                // Record live variables.

                i = i__prev2;
                n = n__prev2;
            }

            live = _addr_lv.livevars[index];
            live.Or(live.val, liveout);

        }
        if (lv.doClobber) {
            lv.clobber(b);
        }
        lv.compact(b);

    }    if (lv.fn.OpenCodedDeferDisallowed()) {
        lv.livenessMap.DeferReturn = objw.LivenessDontCare;
    }
    else
 {
        lv.livenessMap.DeferReturn = new objw.LivenessIndex(StackMapIndex:lv.stackMapSet.add(livedefer),IsUnsafePoint:false,);
    }
    lv.stackMaps = lv.stackMapSet.extractUnique();
    lv.stackMapSet = new bvecSet(); 

    // Useful sanity check: on entry to the function,
    // the only things that can possibly be live are the
    // input parameters.
    {
        var n__prev1 = n;

        foreach (var (__j, __n) in lv.vars) {
            j = __j;
            n = __n;
            if (n.Class != ir.PPARAM && lv.stackMaps[0].Get(int32(j))) {
                lv.f.Fatalf("%v %L recorded as live on entry", lv.fn.Nname, n);
            }
        }
        n = n__prev1;
    }
}

// Compact coalesces identical bitmaps from lv.livevars into the sets
// lv.stackMapSet.
//
// Compact clears lv.livevars.
//
// There are actually two lists of bitmaps, one list for the local variables and one
// list for the function arguments. Both lists are indexed by the same PCDATA
// index, so the corresponding pairs must be considered together when
// merging duplicates. The argument bitmaps change much less often during
// function execution than the local variable bitmaps, so it is possible that
// we could introduce a separate PCDATA index for arguments vs locals and
// then compact the set of argument bitmaps separately from the set of
// local variable bitmaps. As of 2014-04-02, doing this to the godoc binary
// is actually a net loss: we save about 50k of argument bitmaps but the new
// PCDATA tables cost about 100k. So for now we keep using a single index for
// both bitmap lists.
private static void compact(this ptr<liveness> _addr_lv, ptr<ssa.Block> _addr_b) {
    ref liveness lv = ref _addr_lv.val;
    ref ssa.Block b = ref _addr_b.val;

    nint pos = 0;
    if (b == lv.f.Entry) { 
        // Handle entry stack map.
        lv.stackMapSet.add(lv.livevars[0]);
        pos++;

    }
    foreach (var (_, v) in b.Values) {
        var hasStackMap = lv.hasStackMap(v);
        var isUnsafePoint = lv.allUnsafe || v.Op != ssa.OpClobber && lv.unsafePoints.Get(int32(v.ID));
        objw.LivenessIndex idx = new objw.LivenessIndex(StackMapIndex:objw.StackMapDontCare,IsUnsafePoint:isUnsafePoint);
        if (hasStackMap) {
            idx.StackMapIndex = lv.stackMapSet.add(lv.livevars[pos]);
            pos++;
        }
        if (hasStackMap || isUnsafePoint) {
            lv.livenessMap.set(v, idx);
        }
    }    lv.livevars = lv.livevars[..(int)0];

}

private static void enableClobber(this ptr<liveness> _addr_lv) {
    ref liveness lv = ref _addr_lv.val;
 
    // The clobberdead experiment inserts code to clobber pointer slots in all
    // the dead variables (locals and args) at every synchronous safepoint.
    if (!@base.Flag.ClobberDead) {
        return ;
    }
    if (lv.fn.Pragma & ir.CgoUnsafeArgs != 0) { 
        // C or assembly code uses the exact frame layout. Don't clobber.
        return ;

    }
    if (len(lv.vars) > 10000 || len(lv.f.Blocks) > 10000) { 
        // Be careful to avoid doing too much work.
        // Bail if >10000 variables or >10000 blocks.
        // Otherwise, giant functions make this experiment generate too much code.
        return ;

    }
    if (lv.f.Name == "forkAndExecInChild") { 
        // forkAndExecInChild calls vfork on some platforms.
        // The code we add here clobbers parts of the stack in the child.
        // When the parent resumes, it is using the same stack frame. But the
        // child has clobbered stack variables that the parent needs. Boom!
        // In particular, the sys argument gets clobbered.
        return ;

    }
    if (lv.f.Name == "wbBufFlush" || ((lv.f.Name == "callReflect" || lv.f.Name == "callMethod") && lv.fn.ABIWrapper())) { 
        // runtime.wbBufFlush must not modify its arguments. See the comments
        // in runtime/mwbbuf.go:wbBufFlush.
        //
        // reflect.callReflect and reflect.callMethod are called from special
        // functions makeFuncStub and methodValueCall. The runtime expects
        // that it can find the first argument (ctxt) at 0(SP) in makeFuncStub
        // and methodValueCall's frame (see runtime/traceback.go:getArgInfo).
        // Normally callReflect and callMethod already do not modify the
        // argument, and keep it alive. But the compiler-generated ABI wrappers
        // don't do that. Special case the wrappers to not clobber its arguments.
        lv.noClobberArgs = true;

    }
    {
        var h = os.Getenv("GOCLOBBERDEADHASH");

        if (h != "") { 
            // Clobber only functions where the hash of the function name matches a pattern.
            // Useful for binary searching for a miscompiled function.
            @string hstr = "";
            foreach (var (_, b) in sha1.Sum((slice<byte>)lv.f.Name)) {
                hstr += fmt.Sprintf("%08b", b);
            }
            if (!strings.HasSuffix(hstr, h)) {
                return ;
            }

            fmt.Printf("\t\t\tCLOBBERDEAD %s\n", lv.f.Name);

        }
    }

    lv.doClobber = true;

}

// Inserts code to clobber pointer slots in all the dead variables (locals and args)
// at every synchronous safepoint in b.
private static void clobber(this ptr<liveness> _addr_lv, ptr<ssa.Block> _addr_b) {
    ref liveness lv = ref _addr_lv.val;
    ref ssa.Block b = ref _addr_b.val;
 
    // Copy block's values to a temporary.
    var oldSched = append(new slice<ptr<ssa.Value>>(new ptr<ssa.Value>[] {  }), b.Values);
    b.Values = b.Values[..(int)0];
    nint idx = 0; 

    // Clobber pointer slots in all dead variables at entry.
    if (b == lv.f.Entry) {
        while (len(oldSched) > 0 && len(oldSched[0].Args) == 0) { 
            // Skip argless ops. We need to skip at least
            // the lowered ClosurePtr op, because it
            // really wants to be first. This will also
            // skip ops like InitMem and SP, which are ok.
            b.Values = append(b.Values, oldSched[0]);
            oldSched = oldSched[(int)1..];

        }
        clobber(_addr_lv, _addr_b, lv.livevars[0]);
        idx++;

    }
    foreach (var (_, v) in oldSched) {
        if (!lv.hasStackMap(v)) {
            b.Values = append(b.Values, v);
            continue;
        }
        clobber(_addr_lv, _addr_b, lv.livevars[idx]);
        b.Values = append(b.Values, v);
        idx++;

    }
}

// clobber generates code to clobber pointer slots in all dead variables
// (those not marked in live). Clobbering instructions are added to the end
// of b.Values.
private static void clobber(ptr<liveness> _addr_lv, ptr<ssa.Block> _addr_b, bitvec.BitVec live) {
    ref liveness lv = ref _addr_lv.val;
    ref ssa.Block b = ref _addr_b.val;

    foreach (var (i, n) in lv.vars) {
        if (!live.Get(int32(i)) && !n.Addrtaken() && !n.OpenDeferSlot() && !n.IsOutputParamHeapAddr()) { 
            // Don't clobber stack objects (address-taken). They are
            // tracked dynamically.
            // Also don't clobber slots that are live for defers (see
            // the code setting livedefer in epilogue).
            if (lv.noClobberArgs && n.Class == ir.PPARAM) {
                continue;
            }

            clobberVar(_addr_b, _addr_n);

        }
    }
}

// clobberVar generates code to trash the pointers in v.
// Clobbering instructions are added to the end of b.Values.
private static void clobberVar(ptr<ssa.Block> _addr_b, ptr<ir.Name> _addr_v) {
    ref ssa.Block b = ref _addr_b.val;
    ref ir.Name v = ref _addr_v.val;

    clobberWalk(_addr_b, _addr_v, 0, _addr_v.Type());
}

// b = block to which we append instructions
// v = variable
// offset = offset of (sub-portion of) variable to clobber (in bytes)
// t = type of sub-portion of v.
private static void clobberWalk(ptr<ssa.Block> _addr_b, ptr<ir.Name> _addr_v, long offset, ptr<types.Type> _addr_t) {
    ref ssa.Block b = ref _addr_b.val;
    ref ir.Name v = ref _addr_v.val;
    ref types.Type t = ref _addr_t.val;

    if (!t.HasPointers()) {
        return ;
    }

    if (t.Kind() == types.TPTR || t.Kind() == types.TUNSAFEPTR || t.Kind() == types.TFUNC || t.Kind() == types.TCHAN || t.Kind() == types.TMAP) 
        clobberPtr(_addr_b, _addr_v, offset);
    else if (t.Kind() == types.TSTRING) 
        // struct { byte *str; int len; }
        clobberPtr(_addr_b, _addr_v, offset);
    else if (t.Kind() == types.TINTER) 
        // struct { Itab *tab; void *data; }
        // or, when isnilinter(t)==true:
        // struct { Type *type; void *data; }
        clobberPtr(_addr_b, _addr_v, offset);
        clobberPtr(_addr_b, _addr_v, offset + int64(types.PtrSize));
    else if (t.Kind() == types.TSLICE) 
        // struct { byte *array; int len; int cap; }
        clobberPtr(_addr_b, _addr_v, offset);
    else if (t.Kind() == types.TARRAY) 
        for (var i = int64(0); i < t.NumElem(); i++) {
            clobberWalk(_addr_b, _addr_v, offset + i * t.Elem().Size(), _addr_t.Elem());
        }
    else if (t.Kind() == types.TSTRUCT) 
        foreach (var (_, t1) in t.Fields().Slice()) {
            clobberWalk(_addr_b, _addr_v, offset + t1.Offset, _addr_t1.Type);
        }    else 
        @base.Fatalf("clobberWalk: unexpected type, %v", t);
    
}

// clobberPtr generates a clobber of the pointer at offset offset in v.
// The clobber instruction is added at the end of b.
private static void clobberPtr(ptr<ssa.Block> _addr_b, ptr<ir.Name> _addr_v, long offset) {
    ref ssa.Block b = ref _addr_b.val;
    ref ir.Name v = ref _addr_v.val;

    b.NewValue0IA(src.NoXPos, ssa.OpClobber, types.TypeVoid, offset, v);
}

private static void showlive(this ptr<liveness> _addr_lv, ptr<ssa.Value> _addr_v, bitvec.BitVec live) {
    ref liveness lv = ref _addr_lv.val;
    ref ssa.Value v = ref _addr_v.val;

    if (@base.Flag.Live == 0 || ir.FuncName(lv.fn) == "init" || strings.HasPrefix(ir.FuncName(lv.fn), ".")) {
        return ;
    }
    if (!(v == null || v.Op.IsCall())) { 
        // Historically we only printed this information at
        // calls. Keep doing so.
        return ;

    }
    if (live.IsEmpty()) {
        return ;
    }
    var pos = lv.fn.Nname.Pos();
    if (v != null) {
        pos = v.Pos;
    }
    @string s = "live at ";
    if (v == null) {
        s += fmt.Sprintf("entry to %s:", ir.FuncName(lv.fn));
    }    {
        ptr<ssa.AuxCall> (sym, ok) = v.Aux._<ptr<ssa.AuxCall>>();


        else if (ok && sym.Fn != null) {
            var fn = sym.Fn.Name;
            {
                var pos__prev3 = pos;

                pos = strings.Index(fn, ".");

                if (pos >= 0) {
                    fn = fn[(int)pos + 1..];
                }

                pos = pos__prev3;

            }

            s += fmt.Sprintf("call to %s:", fn);

        }
        else
 {
            s += "indirect call:";
        }
    }


    foreach (var (j, n) in lv.vars) {
        if (live.Get(int32(j))) {
            s += fmt.Sprintf(" %v", n);
        }
    }    @base.WarnfAt(pos, s);

}

private static bool printbvec(this ptr<liveness> _addr_lv, bool printed, @string name, bitvec.BitVec live) {
    ref liveness lv = ref _addr_lv.val;

    if (live.IsEmpty()) {
        return printed;
    }
    if (!printed) {
        fmt.Printf("\t");
    }
    else
 {
        fmt.Printf(" ");
    }
    fmt.Printf("%s=", name);

    @string comma = "";
    foreach (var (i, n) in lv.vars) {
        if (!live.Get(int32(i))) {
            continue;
        }
        fmt.Printf("%s%s", comma, n.Sym().Name);
        comma = ",";

    }    return true;

}

// printeffect is like printbvec, but for valueEffects.
private static bool printeffect(this ptr<liveness> _addr_lv, bool printed, @string name, int pos, bool x) {
    ref liveness lv = ref _addr_lv.val;

    if (!x) {
        return printed;
    }
    if (!printed) {
        fmt.Printf("\t");
    }
    else
 {
        fmt.Printf(" ");
    }
    fmt.Printf("%s=", name);
    if (x) {
        fmt.Printf("%s", lv.vars[pos].Sym().Name);
    }
    return true;

}

// Prints the computed liveness information and inputs, for debugging.
// This format synthesizes the information used during the multiple passes
// into a single presentation.
private static void printDebug(this ptr<liveness> _addr_lv) {
    ref liveness lv = ref _addr_lv.val;

    fmt.Printf("liveness: %s\n", ir.FuncName(lv.fn));

    foreach (var (i, b) in lv.f.Blocks) {
        if (i > 0) {
            fmt.Printf("\n");
        }
        fmt.Printf("bb#%d pred=", b.ID);
        {
            var j__prev2 = j;

            foreach (var (__j, __pred) in b.Preds) {
                j = __j;
                pred = __pred;
                if (j > 0) {
                    fmt.Printf(",");
                }
                fmt.Printf("%d", pred.Block().ID);
            }

            j = j__prev2;
        }

        fmt.Printf(" succ=");
        {
            var j__prev2 = j;

            foreach (var (__j, __succ) in b.Succs) {
                j = __j;
                succ = __succ;
                if (j > 0) {
                    fmt.Printf(",");
                }
                fmt.Printf("%d", succ.Block().ID);
            }

            j = j__prev2;
        }

        fmt.Printf("\n");

        var be = lv.blockEffects(b); 

        // initial settings
        var printed = false;
        printed = lv.printbvec(printed, "uevar", be.uevar);
        printed = lv.printbvec(printed, "livein", be.livein);
        if (printed) {
            fmt.Printf("\n");
        }
        if (b == lv.f.Entry) {
            var live = lv.stackMaps[0];
            fmt.Printf("(%s) function entry\n", @base.FmtPos(lv.fn.Nname.Pos()));
            fmt.Printf("\tlive=");
            printed = false;
            {
                var j__prev2 = j;
                var n__prev2 = n;

                foreach (var (__j, __n) in lv.vars) {
                    j = __j;
                    n = __n;
                    if (!live.Get(int32(j))) {
                        continue;
                    }
                    if (printed) {
                        fmt.Printf(",");
                    }
                    fmt.Printf("%v", n);
                    printed = true;
                }

                j = j__prev2;
                n = n__prev2;
            }

            fmt.Printf("\n");

        }
        foreach (var (_, v) in b.Values) {
            fmt.Printf("(%s) %v\n", @base.FmtPos(v.Pos), v.LongString());

            var pcdata = lv.livenessMap.Get(v);

            var (pos, effect) = lv.valueEffects(v);
            printed = false;
            printed = lv.printeffect(printed, "uevar", pos, effect & uevar != 0);
            printed = lv.printeffect(printed, "varkill", pos, effect & varkill != 0);
            if (printed) {
                fmt.Printf("\n");
            }
            if (pcdata.StackMapValid()) {
                fmt.Printf("\tlive=");
                printed = false;
                if (pcdata.StackMapValid()) {
                    live = lv.stackMaps[pcdata.StackMapIndex];
                    {
                        var j__prev3 = j;
                        var n__prev3 = n;

                        foreach (var (__j, __n) in lv.vars) {
                            j = __j;
                            n = __n;
                            if (!live.Get(int32(j))) {
                                continue;
                            }
                            if (printed) {
                                fmt.Printf(",");
                            }
                            fmt.Printf("%v", n);
                            printed = true;
                        }

                        j = j__prev3;
                        n = n__prev3;
                    }
                }

                fmt.Printf("\n");

            }

            if (pcdata.IsUnsafePoint) {
                fmt.Printf("\tunsafe-point\n");
            }

        }        fmt.Printf("end\n");
        printed = false;
        printed = lv.printbvec(printed, "varkill", be.varkill);
        printed = lv.printbvec(printed, "liveout", be.liveout);
        if (printed) {
            fmt.Printf("\n");
        }
    }    fmt.Printf("\n");

}

// Dumps a slice of bitmaps to a symbol as a sequence of uint32 values. The
// first word dumped is the total number of bitmaps. The second word is the
// length of the bitmaps. All bitmaps are assumed to be of equal length. The
// remaining bytes are the raw bitmaps.
private static (ptr<obj.LSym>, ptr<obj.LSym>) emit(this ptr<liveness> _addr_lv) {
    ptr<obj.LSym> argsSym = default!;
    ptr<obj.LSym> liveSym = default!;
    ref liveness lv = ref _addr_lv.val;
 
    // Size args bitmaps to be just large enough to hold the largest pointer.
    // First, find the largest Xoffset node we care about.
    // (Nodes without pointers aren't in lv.vars; see ShouldTrack.)
    ptr<ir.Name> maxArgNode;
    foreach (var (_, n) in lv.vars) {

        if (n.Class == ir.PPARAM || n.Class == ir.PPARAMOUT) 
            if (!n.IsOutputParamInRegisters()) {
                if (maxArgNode == null || n.FrameOffset() > maxArgNode.FrameOffset()) {
                    maxArgNode = n;
                }
            }
        
    }    long maxArgs = default;
    if (maxArgNode != null) {
        maxArgs = maxArgNode.FrameOffset() + types.PtrDataSize(maxArgNode.Type());
    }
    var maxLocals = lv.stkptrsize; 

    // Temporary symbols for encoding bitmaps.
    ref obj.LSym argsSymTmp = ref heap(out ptr<obj.LSym> _addr_argsSymTmp);    ref obj.LSym liveSymTmp = ref heap(out ptr<obj.LSym> _addr_liveSymTmp);



    var args = bitvec.New(int32(maxArgs / int64(types.PtrSize)));
    var aoff = objw.Uint32(_addr_argsSymTmp, 0, uint32(len(lv.stackMaps))); // number of bitmaps
    aoff = objw.Uint32(_addr_argsSymTmp, aoff, uint32(args.N)); // number of bits in each bitmap

    var locals = bitvec.New(int32(maxLocals / int64(types.PtrSize)));
    var loff = objw.Uint32(_addr_liveSymTmp, 0, uint32(len(lv.stackMaps))); // number of bitmaps
    loff = objw.Uint32(_addr_liveSymTmp, loff, uint32(locals.N)); // number of bits in each bitmap

    foreach (var (_, live) in lv.stackMaps) {
        args.Clear();
        locals.Clear();

        lv.pointerMap(live, lv.vars, args, locals);

        aoff = objw.BitVec(_addr_argsSymTmp, aoff, args);
        loff = objw.BitVec(_addr_liveSymTmp, loff, locals);
    }    Func<ptr<obj.LSym>, ptr<obj.LSym>> makeSym = tmpSym => {
        return _addr_@base.Ctxt.LookupInit(fmt.Sprintf("gclocals·%x", md5.Sum(tmpSym.P)), lsym => {
            lsym.P = tmpSym.P;
            lsym.Set(obj.AttrContentAddressable, true);
        })!;
    };
    return (_addr_makeSym(_addr_argsSymTmp)!, _addr_makeSym(_addr_liveSymTmp)!);

}

// Entry pointer for Compute analysis. Solves for the Compute of
// pointer variables in the function and emits a runtime data
// structure read by the garbage collector.
// Returns a map from GC safe points to their corresponding stack map index,
// and a map that contains all input parameters that may be partially live.
public static (Map, map<ptr<ir.Name>, bool>) Compute(ptr<ir.Func> _addr_curfn, ptr<ssa.Func> _addr_f, long stkptrsize, ptr<objw.Progs> _addr_pp) {
    Map _p0 = default;
    map<ptr<ir.Name>, bool> _p0 = default;
    ref ir.Func curfn = ref _addr_curfn.val;
    ref ssa.Func f = ref _addr_f.val;
    ref objw.Progs pp = ref _addr_pp.val;
 
    // Construct the global liveness state.
    var (vars, idx) = getvariables(_addr_curfn);
    var lv = newliveness(_addr_curfn, _addr_f, vars, idx, stkptrsize); 

    // Run the dataflow framework.
    lv.prologue();
    lv.solve();
    lv.epilogue();
    if (@base.Flag.Live > 0) {
        lv.showlive(null, lv.stackMaps[0]);
        foreach (var (_, b) in f.Blocks) {
            foreach (var (_, val) in b.Values) {
                {
                    var idx = lv.livenessMap.Get(val);

                    if (idx.StackMapValid()) {
                        lv.showlive(val, lv.stackMaps[idx.StackMapIndex]);
                    }

                }

            }

        }
    }
    if (@base.Flag.Live >= 2) {
        lv.printDebug();
    }
 {
        ptr<livenessFuncCache> cache = f.Cache.Liveness._<ptr<livenessFuncCache>>();
        if (cap(lv.be) < 2000) { // Threshold from ssa.Cache slices.
            foreach (var (i) in lv.be) {
                lv.be[i] = new blockEffects();
            }
            cache.be = lv.be;

        }
        if (len(lv.livenessMap.Vals) < 2000) {
            cache.livenessMap = lv.livenessMap;
        }
    }    var ls = curfn.LSym;
    var fninfo = ls.Func();
    fninfo.GCArgs, fninfo.GCLocals = lv.emit();

    var p = pp.Prog(obj.AFUNCDATA);
    p.From.SetConst(objabi.FUNCDATA_ArgsPointerMaps);
    p.To.Type = obj.TYPE_MEM;
    p.To.Name = obj.NAME_EXTERN;
    p.To.Sym = fninfo.GCArgs;

    p = pp.Prog(obj.AFUNCDATA);
    p.From.SetConst(objabi.FUNCDATA_LocalsPointerMaps);
    p.To.Type = obj.TYPE_MEM;
    p.To.Name = obj.NAME_EXTERN;
    p.To.Sym = fninfo.GCLocals;

    {
        var x = lv.emitStackObjects();

        if (x != null) {
            p = pp.Prog(obj.AFUNCDATA);
            p.From.SetConst(objabi.FUNCDATA_StackObjects);
            p.To.Type = obj.TYPE_MEM;
            p.To.Name = obj.NAME_EXTERN;
            p.To.Sym = x;
        }
    }


    return (lv.livenessMap, lv.partLiveArgs);

}

private static ptr<obj.LSym> emitStackObjects(this ptr<liveness> _addr_lv) {
    ref liveness lv = ref _addr_lv.val;

    slice<ptr<ir.Name>> vars = default;
    foreach (var (_, n) in lv.fn.Dcl) {
        if (shouldTrack(_addr_n) && n.Addrtaken() && n.Esc() != ir.EscHeap) {
            vars = append(vars, n);
        }
    }    if (len(vars) == 0) {
        return _addr_null!;
    }
    sort.Slice(vars, (i, j) => _addr_vars[i].FrameOffset() < vars[j].FrameOffset()!); 

    // Populate the stack object data.
    // Format must match runtime/stack.go:stackObjectRecord.
    var x = @base.Ctxt.Lookup(lv.fn.LSym.Name + ".stkobj");
    lv.fn.LSym.Func().StackObjects = x;
    nint off = 0;
    off = objw.Uintptr(x, off, uint64(len(vars)));
    {
        var v__prev1 = v;

        foreach (var (_, __v) in vars) {
            v = __v; 
            // Note: arguments and return values have non-negative Xoffset,
            // in which case the offset is relative to argp.
            // Locals have a negative Xoffset, in which case the offset is relative to varp.
            // We already limit the frame size, so the offset and the object size
            // should not be too big.
            var frameOffset = v.FrameOffset();
            if (frameOffset != int64(int32(frameOffset))) {
                @base.Fatalf("frame offset too big: %v %d", v, frameOffset);
            }

            off = objw.Uint32(x, off, uint32(frameOffset));

            var t = v.Type();
            var sz = t.Width;
            if (sz != int64(int32(sz))) {
                @base.Fatalf("stack object too big: %v of type %v, size %d", v, t, sz);
            }

            var (lsym, useGCProg, ptrdata) = reflectdata.GCSym(t);
            if (useGCProg) {
                ptrdata = -ptrdata;
            }

            off = objw.Uint32(x, off, uint32(sz));
            off = objw.Uint32(x, off, uint32(ptrdata));
            off = objw.SymPtr(x, off, lsym, 0);

        }
        v = v__prev1;
    }

    if (@base.Flag.Live != 0) {
        {
            var v__prev1 = v;

            foreach (var (_, __v) in vars) {
                v = __v;
                @base.WarnfAt(v.Pos(), "stack object %v %v", v, v.Type());
            }

            v = v__prev1;
        }
    }
    return _addr_x!;

}

// isfat reports whether a variable of type t needs multiple assignments to initialize.
// For example:
//
//     type T struct { x, y int }
//     x := T{x: 0, y: 1}
//
// Then we need:
//
//     var t T
//     t.x = 0
//     t.y = 1
//
// to fully initialize t.
private static bool isfat(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (t != null) {

        if (t.Kind() == types.TSLICE || t.Kind() == types.TSTRING || t.Kind() == types.TINTER) // maybe remove later
            return true;
        else if (t.Kind() == types.TARRAY) 
            // Array of 1 element, check if element is fat
            if (t.NumElem() == 1) {
                return isfat(_addr_t.Elem());
            }
            return true;
        else if (t.Kind() == types.TSTRUCT) 
            // Struct with 1 field, check if field is fat
            if (t.NumFields() == 1) {
                return isfat(_addr_t.Field(0).Type);
            }
            return true;
        
    }
    return false;

}

// WriteFuncMap writes the pointer bitmaps for bodyless function fn's
// inputs and outputs as the value of symbol <fn>.args_stackmap.
// If fn has outputs, two bitmaps are written, otherwise just one.
public static void WriteFuncMap(ptr<ir.Func> _addr_fn, ptr<abi.ABIParamResultInfo> _addr_abiInfo) {
    ref ir.Func fn = ref _addr_fn.val;
    ref abi.ABIParamResultInfo abiInfo = ref _addr_abiInfo.val;

    if (ir.FuncName(fn) == "_" || fn.Sym().Linkname != "") {
        return ;
    }
    var nptr = int(abiInfo.ArgWidth() / int64(types.PtrSize));
    var bv = bitvec.New(int32(nptr) * 2);

    {
        var p__prev1 = p;

        foreach (var (_, __p) in abiInfo.InParams()) {
            p = __p;
            typebits.Set(p.Type, p.FrameOffset(abiInfo), bv);
        }
        p = p__prev1;
    }

    nint nbitmap = 1;
    if (fn.Type().NumResults() > 0) {
        nbitmap = 2;
    }
    var lsym = @base.Ctxt.Lookup(fn.LSym.Name + ".args_stackmap");
    var off = objw.Uint32(lsym, 0, uint32(nbitmap));
    off = objw.Uint32(lsym, off, uint32(bv.N));
    off = objw.BitVec(lsym, off, bv);

    if (fn.Type().NumResults() > 0) {
        {
            var p__prev1 = p;

            foreach (var (_, __p) in abiInfo.OutParams()) {
                p = __p;
                if (len(p.Registers) == 0) {
                    typebits.Set(p.Type, p.FrameOffset(abiInfo), bv);
                }
            }

            p = p__prev1;
        }

        off = objw.BitVec(lsym, off, bv);

    }
    objw.Global(lsym, int32(off), obj.RODATA | obj.LOCAL);

}

} // end liveness_package
