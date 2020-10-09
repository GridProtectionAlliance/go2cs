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

// package gc -- go2cs converted at 2020 October 09 05:42:21 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\plive.go
using ssa = go.cmd.compile.@internal.ssa_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using md5 = go.crypto.md5_package;
using fmt = go.fmt_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // go115ReduceLiveness disables register maps and only produces stack
        // maps at call sites.
        //
        // In Go 1.15, we changed debug call injection to use conservative
        // scanning instead of precise pointer maps, so these are no longer
        // necessary.
        //
        // Keep in sync with runtime/preempt.go:go115ReduceLiveness.
        private static readonly var go115ReduceLiveness = true;

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

        // BlockEffects summarizes the liveness effects on an SSA block.


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

        // BlockEffects summarizes the liveness effects on an SSA block.
        public partial struct BlockEffects
        {
            public varRegVec uevar;
            public varRegVec varkill; // Computed during Liveness.solve using control flow information:
//
//    livein: variables live at block entry
//    liveout: variables live at block exit
            public varRegVec livein;
            public varRegVec liveout;
        }

        // A collection of global state used by liveness analysis.
        public partial struct Liveness
        {
            public ptr<Node> fn;
            public ptr<ssa.Func> f;
            public slice<ptr<Node>> vars;
            public map<ptr<Node>, int> idx;
            public long stkptrsize;
            public slice<BlockEffects> be; // allUnsafe indicates that all points in this function are
// unsafe-points.
            public bool allUnsafe; // unsafePoints bit i is set if Value ID i is an unsafe-point
// (preemption is not allowed). Only valid if !allUnsafe.
            public bvec unsafePoints; // An array with a bit vector for each safe point in the
// current Block during Liveness.epilogue. Indexed in Value
// order for that block. Additionally, for the entry block
// livevars[0] is the entry bitmap. Liveness.compact moves
// these to stackMaps and regMaps.
            public slice<varRegVec> livevars; // livenessMap maps from safe points (i.e., CALLs) to their
// liveness map indexes.
            public LivenessMap livenessMap;
            public bvecSet stackMapSet;
            public slice<bvec> stackMaps;
            public map<liveRegMask, long> regMapSet;
            public slice<liveRegMask> regMaps;
            public progeffectscache cache; // These are only populated if open-coded defers are being used.
// List of vars/stack slots storing defer args
            public slice<openDeferVarInfo> openDeferVars; // Map from defer arg OpVarDef to the block where the OpVarDef occurs.
            public map<ptr<Node>, ptr<ssa.Block>> openDeferVardefToBlockMap; // Map of blocks that cannot reach a return or exit (panic)
            public map<ptr<ssa.Block>, bool> nonReturnBlocks;
        }

        private partial struct openDeferVarInfo
        {
            public ptr<Node> n; // Var/stack slot storing a defer arg
            public long varsIndex; // Index of variable in lv.vars
        }

        // LivenessMap maps from *ssa.Value to LivenessIndex.
        public partial struct LivenessMap
        {
            public map<ssa.ID, LivenessIndex> vals;
        }

        private static void reset(this ptr<LivenessMap> _addr_m)
        {
            ref LivenessMap m = ref _addr_m.val;

            if (m.vals == null)
            {
                m.vals = make_map<ssa.ID, LivenessIndex>();
            }
            else
            {
                foreach (var (k) in m.vals)
                {
                    delete(m.vals, k);
                }

            }

        }

        private static void set(this ptr<LivenessMap> _addr_m, ptr<ssa.Value> _addr_v, LivenessIndex i)
        {
            ref LivenessMap m = ref _addr_m.val;
            ref ssa.Value v = ref _addr_v.val;

            m.vals[v.ID] = i;
        }

        public static LivenessIndex Get(this LivenessMap m, ptr<ssa.Value> _addr_v)
        {
            ref ssa.Value v = ref _addr_v.val;

            if (!go115ReduceLiveness)
            { 
                // All safe-points are in the map, so if v isn't in
                // the map, it's an unsafe-point.
                {
                    var idx__prev2 = idx;

                    var (idx, ok) = m.vals[v.ID];

                    if (ok)
                    {
                        return idx;
                    }

                    idx = idx__prev2;

                }

                return LivenessInvalid;

            } 

            // If v isn't in the map, then it's a "don't care" and not an
            // unsafe-point.
            {
                var idx__prev1 = idx;

                (idx, ok) = m.vals[v.ID];

                if (ok)
                {
                    return idx;
                }

                idx = idx__prev1;

            }

            return new LivenessIndex(StackMapDontCare,StackMapDontCare,false);

        }

        // LivenessIndex stores the liveness map information for a Value.
        public partial struct LivenessIndex
        {
            public long stackMapIndex;
            public long regMapIndex; // only for !go115ReduceLiveness

// isUnsafePoint indicates that this is an unsafe-point.
//
// Note that it's possible for a call Value to have a stack
// map while also being an unsafe-point. This means it cannot
// be preempted at this instruction, but that a preemption or
// stack growth may happen in the called function.
            public bool isUnsafePoint;
        }

        // LivenessInvalid indicates an unsafe point with no stack map.
        public static LivenessIndex LivenessInvalid = new LivenessIndex(StackMapDontCare,StackMapDontCare,true); // only for !go115ReduceLiveness

        // StackMapDontCare indicates that the stack map index at a Value
        // doesn't matter.
        //
        // This is a sentinel value that should never be emitted to the PCDATA
        // stream. We use -1000 because that's obviously never a valid stack
        // index (but -1 is).
        public static readonly long StackMapDontCare = (long)-1000L;



        public static bool StackMapValid(this LivenessIndex idx)
        {
            return idx.stackMapIndex != StackMapDontCare;
        }

        public static bool RegMapValid(this LivenessIndex idx)
        {
            return idx.regMapIndex != StackMapDontCare;
        }

        private partial struct progeffectscache
        {
            public slice<int> retuevar;
            public slice<int> tailuevar;
            public bool initialized;
        }

        // varRegVec contains liveness bitmaps for variables and registers.
        private partial struct varRegVec
        {
            public bvec vars;
            public liveRegMask regs;
        }

        private static bool Eq(this ptr<varRegVec> _addr_v, varRegVec v2)
        {
            ref varRegVec v = ref _addr_v.val;

            return v.vars.Eq(v2.vars) && v.regs == v2.regs;
        }

        private static void Copy(this ptr<varRegVec> _addr_v, varRegVec v2)
        {
            ref varRegVec v = ref _addr_v.val;

            v.vars.Copy(v2.vars);
            v.regs = v2.regs;
        }

        private static void Clear(this ptr<varRegVec> _addr_v)
        {
            ref varRegVec v = ref _addr_v.val;

            v.vars.Clear();
            v.regs = 0L;
        }

        private static void Or(this ptr<varRegVec> _addr_v, varRegVec v1, varRegVec v2)
        {
            ref varRegVec v = ref _addr_v.val;

            v.vars.Or(v1.vars, v2.vars);
            v.regs = v1.regs | v2.regs;
        }

        private static void AndNot(this ptr<varRegVec> _addr_v, varRegVec v1, varRegVec v2)
        {
            ref varRegVec v = ref _addr_v.val;

            v.vars.AndNot(v1.vars, v2.vars);
            v.regs = v1.regs & ~v2.regs;
        }

        // livenessShouldTrack reports whether the liveness analysis
        // should track the variable n.
        // We don't care about variables that have no pointers,
        // nor do we care about non-local variables,
        // nor do we care about empty structs (handled by the pointer check),
        // nor do we care about the fake PAUTOHEAP variables.
        private static bool livenessShouldTrack(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.Op == ONAME && (n.Class() == PAUTO || n.Class() == PPARAM || n.Class() == PPARAMOUT) && types.Haspointers(n.Type);
        }

        // getvariables returns the list of on-stack variables that we need to track
        // and a map for looking up indices by *Node.
        private static (slice<ptr<Node>>, map<ptr<Node>, int>) getvariables(ptr<Node> _addr_fn)
        {
            slice<ptr<Node>> _p0 = default;
            map<ptr<Node>, int> _p0 = default;
            ref Node fn = ref _addr_fn.val;

            slice<ptr<Node>> vars = default;
            {
                var n__prev1 = n;

                foreach (var (_, __n) in fn.Func.Dcl)
                {
                    n = __n;
                    if (livenessShouldTrack(_addr_n))
                    {
                        vars = append(vars, n);
                    }

                }

                n = n__prev1;
            }

            var idx = make_map<ptr<Node>, int>(len(vars));
            {
                var n__prev1 = n;

                foreach (var (__i, __n) in vars)
                {
                    i = __i;
                    n = __n;
                    idx[n] = int32(i);
                }

                n = n__prev1;
            }

            return (vars, idx);

        }

        private static void initcache(this ptr<Liveness> _addr_lv)
        {
            ref Liveness lv = ref _addr_lv.val;

            if (lv.cache.initialized)
            {
                Fatalf("liveness cache initialized twice");
                return ;
            }

            lv.cache.initialized = true;

            foreach (var (i, node) in lv.vars)
            {

                if (node.Class() == PPARAM) 
                    // A return instruction with a p.to is a tail return, which brings
                    // the stack pointer back up (if it ever went down) and then jumps
                    // to a new function entirely. That form of instruction must read
                    // all the parameters for correctness, and similarly it must not
                    // read the out arguments - they won't be set until the new
                    // function runs.
                    lv.cache.tailuevar = append(lv.cache.tailuevar, int32(i));
                else if (node.Class() == PPARAMOUT) 
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
        private partial struct liveEffect // : long
        {
        }

        private static readonly liveEffect uevar = (liveEffect)1L << (int)(iota);
        private static readonly var varkill = 0;


        // valueEffects returns the index of a variable in lv.vars and the
        // liveness effects v has on that variable.
        // If v does not affect any tracked variables, it returns -1, 0.
        private static (int, liveEffect) valueEffects(this ptr<Liveness> _addr_lv, ptr<ssa.Value> _addr_v)
        {
            int _p0 = default;
            liveEffect _p0 = default;
            ref Liveness lv = ref _addr_lv.val;
            ref ssa.Value v = ref _addr_v.val;

            var (n, e) = affectedNode(_addr_v);
            if (e == 0L || n == null || n.Op != ONAME)
            { // cheapest checks first
                return (-1L, 0L);

            } 

            // AllocFrame has dropped unused variables from
            // lv.fn.Func.Dcl, but they might still be referenced by
            // OpVarFoo pseudo-ops. Ignore them to prevent "lost track of
            // variable" ICEs (issue 19632).

            if (v.Op == ssa.OpVarDef || v.Op == ssa.OpVarKill || v.Op == ssa.OpVarLive || v.Op == ssa.OpKeepAlive) 
                if (!n.Name.Used())
                {
                    return (-1L, 0L);
                }

                        liveEffect effect = default; 
            // Read is a read, obviously.
            //
            // Addr is a read also, as any subsequent holder of the pointer must be able
            // to see all the values (including initialization) written so far.
            // This also prevents a variable from "coming back from the dead" and presenting
            // stale pointers to the garbage collector. See issue 28445.
            if (e & (ssa.SymRead | ssa.SymAddr) != 0L)
            {
                effect |= uevar;
            }

            if (e & ssa.SymWrite != 0L && (!isfat(_addr_n.Type) || v.Op == ssa.OpVarDef))
            {
                effect |= varkill;
            }

            if (effect == 0L)
            {
                return (-1L, 0L);
            }

            {
                var (pos, ok) = lv.idx[n];

                if (ok)
                {
                    return (pos, effect);
                }

            }

            return (-1L, 0L);

        }

        // affectedNode returns the *Node affected by v
        private static (ptr<Node>, ssa.SymEffect) affectedNode(ptr<ssa.Value> _addr_v)
        {
            ptr<Node> _p0 = default!;
            ssa.SymEffect _p0 = default;
            ref ssa.Value v = ref _addr_v.val;
 
            // Special cases.

            if (v.Op == ssa.OpLoadReg) 
                var (n, _) = AutoVar(v.Args[0L]);
                return (_addr_n!, ssa.SymRead);
            else if (v.Op == ssa.OpStoreReg) 
                (n, _) = AutoVar(v);
                return (_addr_n!, ssa.SymWrite);
            else if (v.Op == ssa.OpVarLive) 
                return (v.Aux._<ptr<Node>>(), ssa.SymRead);
            else if (v.Op == ssa.OpVarDef || v.Op == ssa.OpVarKill) 
                return (v.Aux._<ptr<Node>>(), ssa.SymWrite);
            else if (v.Op == ssa.OpKeepAlive) 
                (n, _) = AutoVar(v.Args[0L]);
                return (_addr_n!, ssa.SymRead);
                        var e = v.Op.SymEffect();
            if (e == 0L)
            {
                return (_addr_null!, 0L);
            }

            switch (v.Aux.type())
            {
                case ptr<obj.LSym> a:
                    return (_addr_null!, e);
                    break;
                case ptr<Node> a:
                    return (_addr_a!, e);
                    break;
                default:
                {
                    var a = v.Aux.type();
                    Fatalf("weird aux: %s", v.LongString());
                    return (_addr_null!, e);
                    break;
                }
            }

        }

        // regEffects returns the registers affected by v.
        private static (liveRegMask, liveRegMask) regEffects(this ptr<Liveness> _addr_lv, ptr<ssa.Value> _addr_v)
        {
            liveRegMask uevar = default;
            liveRegMask kill = default;
            ref Liveness lv = ref _addr_lv.val;
            ref ssa.Value v = ref _addr_v.val;

            if (go115ReduceLiveness)
            {
                return (0L, 0L);
            }

            if (v.Op == ssa.OpPhi)
            { 
                // All phi node arguments must come from the same
                // register and the result must also go to that
                // register, so there's no overall effect.
                return (0L, 0L);

            }

            Func<liveRegMask, ptr<ssa.Value>, bool, liveRegMask> addLocs = (mask, v, ptrOnly) =>
            {
                if (int(v.ID) >= len(lv.f.RegAlloc))
                { 
                    // v has no allocated registers.
                    return mask;

                }

                ref var loc = ref heap(lv.f.RegAlloc[v.ID], out ptr<var> _addr_loc);
                if (loc == null)
                { 
                    // v has no allocated registers.
                    return mask;

                }

                if (v.Op == ssa.OpGetG)
                { 
                    // GetG represents the G register, which is a
                    // pointer, but not a valid GC register. The
                    // current G is always reachable, so it's okay
                    // to ignore this register.
                    return mask;

                } 

                // Collect registers and types from v's location.
                array<ptr<ssa.Register>> regs = new array<ptr<ssa.Register>>(2L);
                long nreg = 0L;
                switch (loc.type())
                {
                    case ssa.LocalSlot loc:
                        return mask;
                        break;
                    case ptr<ssa.Register> loc:
                        if (ptrOnly && !v.Type.HasHeapPointer())
                        {
                            return mask;
                        }

                        regs[0L] = loc;
                        nreg = 1L;
                        break;
                    case ssa.LocPair loc:
                        if (v.Type.Etype != types.TTUPLE)
                        {
                            v.Fatalf("location pair %s has non-tuple type %v", loc, v.Type);
                        }

                        foreach (var (i, loc1) in _addr_loc)
                        {
                            if (loc1 == null)
                            {
                                continue;
                            }

                            if (ptrOnly && !v.Type.FieldType(i).HasHeapPointer())
                            {
                                continue;
                            }

                            regs[nreg] = loc1._<ptr<ssa.Register>>();
                            nreg++;

                        }
                        break;
                    default:
                    {
                        var loc = loc.type();
                        v.Fatalf("weird RegAlloc location: %s (%T)", loc, loc);
                        break;
                    } 

                    // Add register locations to vars.
                } 

                // Add register locations to vars.
                foreach (var (_, reg) in regs[..nreg])
                {
                    if (reg.GCNum() == -1L)
                    {
                        if (ptrOnly)
                        {
                            v.Fatalf("pointer in non-pointer register %v", reg);
                        }
                        else
                        {
                            continue;
                        }

                    }

                    mask |= 1L << (int)(uint(reg.GCNum()));

                }
                return mask;

            } 

            // v clobbers all registers it writes to (whether or not the
            // write is pointer-typed).
; 

            // v clobbers all registers it writes to (whether or not the
            // write is pointer-typed).
            kill = addLocs(0L, v, false);
            foreach (var (_, arg) in v.Args)
            { 
                // v uses all registers is reads from, but we only
                // care about marking those containing pointers.
                uevar = addLocs(uevar, arg, true);

            }
            return (uevar, kill);

        }

        private partial struct liveRegMask // : uint
        {
        } // only if !go115ReduceLiveness

        private static @string niceString(this liveRegMask m, ptr<ssa.Config> _addr_config)
        {
            ref ssa.Config config = ref _addr_config.val;

            if (m == 0L)
            {
                return "<none>";
            }

            @string str = "";
            foreach (var (i, reg) in config.GCRegMap)
            {
                if (m & (1L << (int)(uint(i))) != 0L)
                {
                    if (str != "")
                    {
                        str += ",";
                    }

                    str += reg.String();

                }

            }
            return str;

        }

        private partial struct livenessFuncCache
        {
            public slice<BlockEffects> be;
            public LivenessMap livenessMap;
        }

        // Constructs a new liveness structure used to hold the global state of the
        // liveness computation. The cfg argument is a slice of *BasicBlocks and the
        // vars argument is a slice of *Nodes.
        private static ptr<Liveness> newliveness(ptr<Node> _addr_fn, ptr<ssa.Func> _addr_f, slice<ptr<Node>> vars, map<ptr<Node>, int> idx, long stkptrsize)
        {
            ref Node fn = ref _addr_fn.val;
            ref ssa.Func f = ref _addr_f.val;

            ptr<Liveness> lv = addr(new Liveness(fn:fn,f:f,vars:vars,idx:idx,stkptrsize:stkptrsize,regMapSet:make(map[liveRegMask]int),)); 

            // Significant sources of allocation are kept in the ssa.Cache
            // and reused. Surprisingly, the bit vectors themselves aren't
            // a major source of allocation, but the liveness maps are.
            {
                ptr<livenessFuncCache> (lc, _) = f.Cache.Liveness._<ptr<livenessFuncCache>>();

                if (lc == null)
                { 
                    // Prep the cache so liveness can fill it later.
                    f.Cache.Liveness = @new<livenessFuncCache>();

                }
                else
                {
                    if (cap(lc.be) >= f.NumBlocks())
                    {
                        lv.be = lc.be[..f.NumBlocks()];
                    }

                    lv.livenessMap = new LivenessMap(lc.livenessMap.vals);
                    lc.livenessMap.vals = null;

                }

            }

            if (lv.be == null)
            {
                lv.be = make_slice<BlockEffects>(f.NumBlocks());
            }

            var nblocks = int32(len(f.Blocks));
            var nvars = int32(len(vars));
            var bulk = bvbulkalloc(nvars, nblocks * 7L);
            foreach (var (_, b) in f.Blocks)
            {
                var be = lv.blockEffects(b);

                be.uevar = new varRegVec(vars:bulk.next());
                be.varkill = new varRegVec(vars:bulk.next());
                be.livein = new varRegVec(vars:bulk.next());
                be.liveout = new varRegVec(vars:bulk.next());
            }
            lv.livenessMap.reset();

            lv.markUnsafePoints();
            return _addr_lv!;

        }

        private static ptr<BlockEffects> blockEffects(this ptr<Liveness> _addr_lv, ptr<ssa.Block> _addr_b)
        {
            ref Liveness lv = ref _addr_lv.val;
            ref ssa.Block b = ref _addr_b.val;

            return _addr__addr_lv.be[b.ID]!;
        }

        // NOTE: The bitmap for a specific type t could be cached in t after
        // the first run and then simply copied into bv at the correct offset
        // on future calls with the same type t.
        private static void onebitwalktype1(ptr<types.Type> _addr_t, long off, bvec bv)
        {
            ref types.Type t = ref _addr_t.val;

            if (t.Align > 0L && off & int64(t.Align - 1L) != 0L)
            {
                Fatalf("onebitwalktype1: invalid initial alignment: type %v has alignment %d, but offset is %v", t, t.Align, off);
            }


            if (t.Etype == TINT8 || t.Etype == TUINT8 || t.Etype == TINT16 || t.Etype == TUINT16 || t.Etype == TINT32 || t.Etype == TUINT32 || t.Etype == TINT64 || t.Etype == TUINT64 || t.Etype == TINT || t.Etype == TUINT || t.Etype == TUINTPTR || t.Etype == TBOOL || t.Etype == TFLOAT32 || t.Etype == TFLOAT64 || t.Etype == TCOMPLEX64 || t.Etype == TCOMPLEX128)             else if (t.Etype == TPTR || t.Etype == TUNSAFEPTR || t.Etype == TFUNC || t.Etype == TCHAN || t.Etype == TMAP) 
                if (off & int64(Widthptr - 1L) != 0L)
                {
                    Fatalf("onebitwalktype1: invalid alignment, %v", t);
                }

                bv.Set(int32(off / int64(Widthptr))); // pointer
            else if (t.Etype == TSTRING) 
                // struct { byte *str; intgo len; }
                if (off & int64(Widthptr - 1L) != 0L)
                {
                    Fatalf("onebitwalktype1: invalid alignment, %v", t);
                }

                bv.Set(int32(off / int64(Widthptr))); //pointer in first slot
            else if (t.Etype == TINTER) 
                // struct { Itab *tab;    void *data; }
                // or, when isnilinter(t)==true:
                // struct { Type *type; void *data; }
                if (off & int64(Widthptr - 1L) != 0L)
                {
                    Fatalf("onebitwalktype1: invalid alignment, %v", t);
                } 
                // The first word of an interface is a pointer, but we don't
                // treat it as such.
                // 1. If it is a non-empty interface, the pointer points to an itab
                //    which is always in persistentalloc space.
                // 2. If it is an empty interface, the pointer points to a _type.
                //   a. If it is a compile-time-allocated type, it points into
                //      the read-only data section.
                //   b. If it is a reflect-allocated type, it points into the Go heap.
                //      Reflect is responsible for keeping a reference to
                //      the underlying type so it won't be GCd.
                // If we ever have a moving GC, we need to change this for 2b (as
                // well as scan itabs to update their itab._type fields).
                bv.Set(int32(off / int64(Widthptr) + 1L)); // pointer in second slot
            else if (t.Etype == TSLICE) 
                // struct { byte *array; uintgo len; uintgo cap; }
                if (off & int64(Widthptr - 1L) != 0L)
                {
                    Fatalf("onebitwalktype1: invalid TARRAY alignment, %v", t);
                }

                bv.Set(int32(off / int64(Widthptr))); // pointer in first slot (BitsPointer)
            else if (t.Etype == TARRAY) 
                var elt = t.Elem();
                if (elt.Width == 0L)
                { 
                    // Short-circuit for #20739.
                    break;

                }

                for (var i = int64(0L); i < t.NumElem(); i++)
                {
                    onebitwalktype1(_addr_elt, off, bv);
                    off += elt.Width;
                }

            else if (t.Etype == TSTRUCT) 
                foreach (var (_, f) in t.Fields().Slice())
                {
                    onebitwalktype1(_addr_f.Type, off + f.Offset, bv);
                }
            else 
                Fatalf("onebitwalktype1: unexpected type, %v", t);
            
        }

        // usedRegs returns the maximum width of the live register map.
        private static int usedRegs(this ptr<Liveness> _addr_lv)
        {
            ref Liveness lv = ref _addr_lv.val;

            liveRegMask any = default;
            foreach (var (_, live) in lv.regMaps)
            {
                any |= live;
            }
            var i = int32(0L);
            while (any != 0L)
            {
                any >>= 1L;
                i++;
            }

            return i;

        }

        // Generates live pointer value maps for arguments and local variables. The
        // this argument and the in arguments are always assumed live. The vars
        // argument is a slice of *Nodes.
        private static void pointerMap(this ptr<Liveness> _addr_lv, bvec liveout, slice<ptr<Node>> vars, bvec args, bvec locals)
        {
            ref Liveness lv = ref _addr_lv.val;

            for (var i = int32(0L); >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
            {
                i = liveout.Next(i);
                if (i < 0L)
                {
                    break;
                }

                var node = vars[i];

                if (node.Class() == PAUTO) 
                    onebitwalktype1(_addr_node.Type, node.Xoffset + lv.stkptrsize, locals);
                else if (node.Class() == PPARAM || node.Class() == PPARAMOUT) 
                    onebitwalktype1(_addr_node.Type, node.Xoffset, args);
                
            }


        }

        // allUnsafe indicates that all points in this function are
        // unsafe-points.
        private static bool allUnsafe(ptr<ssa.Func> _addr_f)
        {
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
            return compiling_runtime || f.NoSplit;

        }

        // markUnsafePoints finds unsafe points and computes lv.unsafePoints.
        private static void markUnsafePoints(this ptr<Liveness> _addr_lv)
        {
            ref Liveness lv = ref _addr_lv.val;

            if (allUnsafe(_addr_lv.f))
            { 
                // No complex analysis necessary.
                lv.allUnsafe = true;
                return ;

            }

            lv.unsafePoints = bvalloc(int32(lv.f.NumValues())); 

            // Mark architecture-specific unsafe points.
            {
                var b__prev1 = b;

                foreach (var (_, __b) in lv.f.Blocks)
                {
                    b = __b;
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            if (v.Op.UnsafePoint())
                            {
                                lv.unsafePoints.Set(int32(v.ID));
                            }

                        }

                        v = v__prev2;
                    }
                } 

                // Mark write barrier unsafe points.

                b = b__prev1;
            }

            foreach (var (_, wbBlock) in lv.f.WBLoads)
            {
                if (wbBlock.Kind == ssa.BlockPlain && len(wbBlock.Values) == 0L)
                { 
                    // The write barrier block was optimized away
                    // but we haven't done dead block elimination.
                    // (This can happen in -N mode.)
                    continue;

                } 
                // Check that we have the expected diamond shape.
                if (len(wbBlock.Succs) != 2L)
                {
                    lv.f.Fatalf("expected branch at write barrier block %v", wbBlock);
                }

                var s0 = wbBlock.Succs[0L].Block();
                var s1 = wbBlock.Succs[1L].Block();
                if (s0 == s1)
                { 
                    // There's no difference between write barrier on and off.
                    // Thus there's no unsafe locations. See issue 26024.
                    continue;

                }

                if (s0.Kind != ssa.BlockPlain || s1.Kind != ssa.BlockPlain)
                {
                    lv.f.Fatalf("expected successors of write barrier block %v to be plain", wbBlock);
                }

                if (s0.Succs[0L].Block() != s1.Succs[0L].Block())
                {
                    lv.f.Fatalf("expected successors of write barrier block %v to converge", wbBlock);
                } 

                // Flow backwards from the control value to find the
                // flag load. We don't know what lowered ops we're
                // looking for, but all current arches produce a
                // single op that does the memory load from the flag
                // address, so we look for that.
                ptr<ssa.Value> load;
                var v = wbBlock.Controls[0L];
                while (true)
                {
                    {
                        ptr<obj.LSym> (sym, ok) = v.Aux._<ptr<obj.LSym>>();

                        if (ok && sym == writeBarrier)
                        {
                            load = v;
                            break;
                        }

                    }


                    if (v.Op == ssa.Op386TESTL) 
                        // 386 lowers Neq32 to (TESTL cond cond),
                        if (v.Args[0L] == v.Args[1L])
                        {
                            v = v.Args[0L];
                            continue;
                        }

                    else if (v.Op == ssa.Op386MOVLload || v.Op == ssa.OpARM64MOVWUload || v.Op == ssa.OpPPC64MOVWZload || v.Op == ssa.OpWasmI64Load32U) 
                        // Args[0] is the address of the write
                        // barrier control. Ignore Args[1],
                        // which is the mem operand.
                        // TODO: Just ignore mem operands?
                        v = v.Args[0L];
                        continue;
                    // Common case: just flow backwards.
                    if (len(v.Args) != 1L)
                    {
                        v.Fatalf("write barrier control value has more than one argument: %s", v.LongString());
                    }

                    v = v.Args[0L];

                } 

                // Mark everything after the load unsafe.
 

                // Mark everything after the load unsafe.
                var found = false;
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in wbBlock.Values)
                    {
                        v = __v;
                        found = found || v == load;
                        if (found)
                        {
                            lv.unsafePoints.Set(int32(v.ID));
                        }

                    } 

                    // Mark the two successor blocks unsafe. These come
                    // back together immediately after the direct write in
                    // one successor and the last write barrier call in
                    // the other, so there's no need to be more precise.

                    v = v__prev2;
                }

                foreach (var (_, succ) in wbBlock.Succs)
                {
                    {
                        var v__prev3 = v;

                        foreach (var (_, __v) in succ.Block().Values)
                        {
                            v = __v;
                            lv.unsafePoints.Set(int32(v.ID));
                        }

                        v = v__prev3;
                    }
                }

            } 

            // Find uintptr -> unsafe.Pointer conversions and flood
            // unsafeness back to a call (which is always a safe point).
            //
            // Looking for the uintptr -> unsafe.Pointer conversion has a
            // few advantages over looking for unsafe.Pointer -> uintptr
            // conversions:
            //
            // 1. We avoid needlessly blocking safe-points for
            // unsafe.Pointer -> uintptr conversions that never go back to
            // a Pointer.
            //
            // 2. We don't have to detect calls to reflect.Value.Pointer,
            // reflect.Value.UnsafeAddr, and reflect.Value.InterfaceData,
            // which are implicit unsafe.Pointer -> uintptr conversions.
            // We can't even reliably detect this if there's an indirect
            // call to one of these methods.
            //
            // TODO: For trivial unsafe.Pointer arithmetic, it would be
            // nice to only flood as far as the unsafe.Pointer -> uintptr
            // conversion, but it's hard to know which argument of an Add
            // or Sub to follow.
            bvec flooded = default;
            Action<ptr<ssa.Block>, long> flood = default;
            flood = (b, vi) =>
            {
                if (flooded.n == 0L)
                {
                    flooded = bvalloc(int32(lv.f.NumBlocks()));
                }

                if (flooded.Get(int32(b.ID)))
                {
                    return ;
                }

                {
                    var i__prev1 = i;

                    for (var i = vi - 1L; i >= 0L; i--)
                    {
                        v = b.Values[i];
                        if (v.Op.IsCall())
                        { 
                            // Uintptrs must not contain live
                            // pointers across calls, so stop
                            // flooding.
                            return ;

                        }

                        lv.unsafePoints.Set(int32(v.ID));

                    }


                    i = i__prev1;
                }
                if (vi == len(b.Values))
                { 
                    // We marked all values in this block, so no
                    // need to flood this block again.
                    flooded.Set(int32(b.ID));

                }

                foreach (var (_, pred) in b.Preds)
                {
                    flood(pred.Block(), len(pred.Block().Values));
                }

            }
;
            {
                var b__prev1 = b;

                foreach (var (_, __b) in lv.f.Blocks)
                {
                    b = __b;
                    {
                        var i__prev2 = i;
                        var v__prev2 = v;

                        foreach (var (__i, __v) in b.Values)
                        {
                            i = __i;
                            v = __v;
                            if (!(v.Op == ssa.OpConvert && v.Type.IsPtrShaped()))
                            {
                                continue;
                            } 
                            // Flood the unsafe-ness of this backwards
                            // until we hit a call.
                            flood(b, i + 1L);

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
        private static bool hasStackMap(this ptr<Liveness> _addr_lv, ptr<ssa.Value> _addr_v)
        {
            ref Liveness lv = ref _addr_lv.val;
            ref ssa.Value v = ref _addr_v.val;
 
            // The runtime only has safe-points in function prologues, so
            // we only need stack maps at call sites. go:nosplit functions
            // are similar.
            if (go115ReduceLiveness || compiling_runtime || lv.f.NoSplit)
            {
                if (!v.Op.IsCall())
                {
                    return false;
                } 
                // typedmemclr and typedmemmove are write barriers and
                // deeply non-preemptible. They are unsafe points and
                // hence should not have liveness maps.
                {
                    ptr<obj.LSym> (sym, _) = v.Aux._<ptr<obj.LSym>>();

                    if (sym == typedmemclr || sym == typedmemmove)
                    {
                        return false;
                    }

                }

                return true;

            }


            if (v.Op == ssa.OpInitMem || v.Op == ssa.OpArg || v.Op == ssa.OpSP || v.Op == ssa.OpSB || v.Op == ssa.OpSelect0 || v.Op == ssa.OpSelect1 || v.Op == ssa.OpGetG || v.Op == ssa.OpVarDef || v.Op == ssa.OpVarLive || v.Op == ssa.OpKeepAlive || v.Op == ssa.OpPhi) 
                // These don't produce code (see genssa).
                return false;
                        return !lv.unsafePoints.Get(int32(v.ID));

        }

        // Initializes the sets for solving the live variables. Visits all the
        // instructions in each basic block to summarizes the information at each basic
        // block
        private static void prologue(this ptr<Liveness> _addr_lv)
        {
            ref Liveness lv = ref _addr_lv.val;

            lv.initcache();

            if (lv.fn.Func.HasDefer() && !lv.fn.Func.OpenCodedDeferDisallowed())
            {
                lv.openDeferVardefToBlockMap = make_map<ptr<Node>, ptr<ssa.Block>>();
                {
                    var n__prev1 = n;

                    foreach (var (__i, __n) in lv.vars)
                    {
                        i = __i;
                        n = __n;
                        if (n.Name.OpenDeferSlot())
                        {
                            lv.openDeferVars = append(lv.openDeferVars, new openDeferVarInfo(n:n,varsIndex:i));
                        }

                    } 

                    // Find any blocks that cannot reach a return or a BlockExit
                    // (panic) -- these must be because of an infinite loop.

                    n = n__prev1;
                }

                var reachesRet = make_map<ssa.ID, bool>();
                var blockList = make_slice<ptr<ssa.Block>>(0L, 256L);

                {
                    var b__prev1 = b;

                    foreach (var (_, __b) in lv.f.Blocks)
                    {
                        b = __b;
                        if (b.Kind == ssa.BlockRet || b.Kind == ssa.BlockRetJmp || b.Kind == ssa.BlockExit)
                        {
                            blockList = append(blockList, b);
                        }

                    }

                    b = b__prev1;
                }

                while (len(blockList) > 0L)
                {
                    var b = blockList[0L];
                    blockList = blockList[1L..];
                    if (reachesRet[b.ID])
                    {
                        continue;
                    }

                    reachesRet[b.ID] = true;
                    foreach (var (_, e) in b.Preds)
                    {
                        blockList = append(blockList, e.Block());
                    }

                }


                lv.nonReturnBlocks = make_map<ptr<ssa.Block>, bool>();
                {
                    var b__prev1 = b;

                    foreach (var (_, __b) in lv.f.Blocks)
                    {
                        b = __b;
                        if (!reachesRet[b.ID])
                        {
                            lv.nonReturnBlocks[b] = true; 
                            //fmt.Println("No reach ret", lv.f.Name, b.ID, b.Kind)
                        }

                    }

                    b = b__prev1;
                }
            }

            {
                var b__prev1 = b;

                foreach (var (_, __b) in lv.f.Blocks)
                {
                    b = __b;
                    var be = lv.blockEffects(b); 

                    // Walk the block instructions backward and update the block
                    // effects with the each prog effects.
                    for (var j = len(b.Values) - 1L; j >= 0L; j--)
                    {
                        if (b.Values[j].Op == ssa.OpVarDef)
                        {
                            ptr<Node> n = b.Values[j].Aux._<ptr<Node>>();
                            if (n.Name.OpenDeferSlot())
                            {
                                lv.openDeferVardefToBlockMap[n] = b;
                            }

                        }

                        var (pos, e) = lv.valueEffects(b.Values[j]);
                        var (regUevar, regKill) = lv.regEffects(b.Values[j]);
                        if (e & varkill != 0L)
                        {
                            be.varkill.vars.Set(pos);
                            be.uevar.vars.Unset(pos);
                        }

                        be.varkill.regs |= regKill;
                        be.uevar.regs &= regKill;
                        if (e & uevar != 0L)
                        {
                            be.uevar.vars.Set(pos);
                        }

                        be.uevar.regs |= regUevar;

                    }


                }

                b = b__prev1;
            }
        }

        // markDeferVarsLive marks each variable storing an open-coded defer arg as
        // specially live in block b if the variable definition dominates block b.
        private static void markDeferVarsLive(this ptr<Liveness> _addr_lv, ptr<ssa.Block> _addr_b, ptr<varRegVec> _addr_newliveout)
        {
            ref Liveness lv = ref _addr_lv.val;
            ref ssa.Block b = ref _addr_b.val;
            ref varRegVec newliveout = ref _addr_newliveout.val;
 
            // Only force computation of dominators if we have a block where we need
            // to specially mark defer args live.
            var sdom = lv.f.Sdom();
            foreach (var (_, info) in lv.openDeferVars)
            {
                var defB = lv.openDeferVardefToBlockMap[info.n];
                if (sdom.IsAncestorEq(defB, b))
                {
                    newliveout.vars.Set(int32(info.varsIndex));
                }

            }

        }

        // Solve the liveness dataflow equations.
        private static void solve(this ptr<Liveness> _addr_lv)
        {
            ref Liveness lv = ref _addr_lv.val;
 
            // These temporary bitvectors exist to avoid successive allocations and
            // frees within the loop.
            var nvars = int32(len(lv.vars));
            varRegVec newlivein = new varRegVec(vars:bvalloc(nvars));
            ref varRegVec newliveout = ref heap(new varRegVec(vars:bvalloc(nvars)), out ptr<varRegVec> _addr_newliveout); 

            // Walk blocks in postorder ordering. This improves convergence.
            var po = lv.f.Postorder(); 

            // Iterate through the blocks in reverse round-robin fashion. A work
            // queue might be slightly faster. As is, the number of iterations is
            // so low that it hardly seems to be worth the complexity.

            {
                var change = true;

                while (change)
                {
                    change = false;
                    foreach (var (_, b) in po)
                    {
                        var be = lv.blockEffects(b);

                        newliveout.Clear();

                        if (b.Kind == ssa.BlockRet) 
                            {
                                var pos__prev3 = pos;

                                foreach (var (_, __pos) in lv.cache.retuevar)
                                {
                                    pos = __pos;
                                    newliveout.vars.Set(pos);
                                }

                                pos = pos__prev3;
                            }
                        else if (b.Kind == ssa.BlockRetJmp) 
                            {
                                var pos__prev3 = pos;

                                foreach (var (_, __pos) in lv.cache.tailuevar)
                                {
                                    pos = __pos;
                                    newliveout.vars.Set(pos);
                                }

                                pos = pos__prev3;
                            }
                        else if (b.Kind == ssa.BlockExit)                         else 
                            // A variable is live on output from this block
                            // if it is live on input to some successor.
                            //
                            // out[b] = \bigcup_{s \in succ[b]} in[s]
                            newliveout.Copy(lv.blockEffects(b.Succs[0L].Block()).livein);
                            foreach (var (_, succ) in b.Succs[1L..])
                            {
                                newliveout.Or(newliveout, lv.blockEffects(succ.Block()).livein);
                            }
                                                if (lv.fn.Func.HasDefer() && !lv.fn.Func.OpenCodedDeferDisallowed() && (b.Kind == ssa.BlockExit || lv.nonReturnBlocks[b]))
                        { 
                            // Open-coded defer args slots must be live
                            // everywhere in a function, since a panic can
                            // occur (almost) anywhere. Force all appropriate
                            // defer arg slots to be live in BlockExit (panic)
                            // blocks and in blocks that do not reach a return
                            // (because of infinite loop).
                            //
                            // We are assuming that the defer exit code at
                            // BlockReturn/BlockReturnJmp accesses all of the
                            // defer args (with pointers), and so keeps them
                            // live. This analysis may have to be adjusted if
                            // that changes (because of optimizations).
                            lv.markDeferVarsLive(b, _addr_newliveout);

                        }

                        if (!be.liveout.Eq(newliveout))
                        {
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
        private static void epilogue(this ptr<Liveness> _addr_lv)
        {
            ref Liveness lv = ref _addr_lv.val;

            var nvars = int32(len(lv.vars));
            varRegVec liveout = new varRegVec(vars:bvalloc(nvars));
            var livedefer = bvalloc(nvars); // always-live variables

            // If there is a defer (that could recover), then all output
            // parameters are live all the time.  In addition, any locals
            // that are pointers to heap-allocated output parameters are
            // also always live (post-deferreturn code needs these
            // pointers to copy values back to the stack).
            // TODO: if the output parameter is heap-allocated, then we
            // don't need to keep the stack copy live?
            if (lv.fn.Func.HasDefer())
            {
                {
                    var i__prev1 = i;
                    var n__prev1 = n;

                    foreach (var (__i, __n) in lv.vars)
                    {
                        i = __i;
                        n = __n;
                        if (n.Class() == PPARAMOUT)
                        {
                            if (n.Name.IsOutputParamHeapAddr())
                            { 
                                // Just to be paranoid.  Heap addresses are PAUTOs.
                                Fatalf("variable %v both output param and heap output param", n);

                            }

                            if (n.Name.Param.Heapaddr != null)
                            { 
                                // If this variable moved to the heap, then
                                // its stack copy is not live.
                                continue;

                            } 
                            // Note: zeroing is handled by zeroResults in walk.go.
                            livedefer.Set(int32(i));

                        }

                        if (n.Name.IsOutputParamHeapAddr())
                        { 
                            // This variable will be overwritten early in the function
                            // prologue (from the result of a mallocgc) but we need to
                            // zero it in case that malloc causes a stack scan.
                            n.Name.SetNeedzero(true);
                            livedefer.Set(int32(i));

                        }

                    }

                    i = i__prev1;
                    n = n__prev1;
                }
            } 

            // We must analyze the entry block first. The runtime assumes
            // the function entry map is index 0. Conveniently, layout
            // already ensured that the entry block is first.
            if (lv.f.Entry != lv.f.Blocks[0L])
            {
                lv.f.Fatalf("entry block must be first");
            }

            { 
                // Reserve an entry for function entry.
                var live = bvalloc(nvars);
                lv.livevars = append(lv.livevars, new varRegVec(vars:live));

            }
            foreach (var (_, b) in lv.f.Blocks)
            {
                var be = lv.blockEffects(b);
                var firstBitmapIndex = len(lv.livevars); 

                // Walk forward through the basic block instructions and
                // allocate liveness maps for those instructions that need them.
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in b.Values)
                    {
                        v = __v;
                        if (!lv.hasStackMap(v))
                        {
                            continue;
                        }

                        live = bvalloc(nvars);
                        lv.livevars = append(lv.livevars, new varRegVec(vars:live));

                    } 

                    // walk backward, construct maps at each safe point

                    v = v__prev2;
                }

                var index = int32(len(lv.livevars) - 1L);

                liveout.Copy(be.liveout);
                {
                    var i__prev2 = i;

                    for (var i = len(b.Values) - 1L; i >= 0L; i--)
                    {
                        var v = b.Values[i];

                        if (lv.hasStackMap(v))
                        { 
                            // Found an interesting instruction, record the
                            // corresponding liveness information.

                            live = _addr_lv.livevars[index];
                            live.Or(live.val, liveout);
                            live.vars.Or(live.vars, livedefer); // only for non-entry safe points
                            index--;

                        } 

                        // Update liveness information.
                        var (pos, e) = lv.valueEffects(v);
                        var (regUevar, regKill) = lv.regEffects(v);
                        if (e & varkill != 0L)
                        {
                            liveout.vars.Unset(pos);
                        }

                        liveout.regs &= regKill;
                        if (e & uevar != 0L)
                        {
                            liveout.vars.Set(pos);
                        }

                        liveout.regs |= regUevar;

                    }


                    i = i__prev2;
                }

                if (b == lv.f.Entry)
                {
                    if (index != 0L)
                    {
                        Fatalf("bad index for entry point: %v", index);
                    } 

                    // Check to make sure only input variables are live.
                    {
                        var i__prev2 = i;
                        var n__prev2 = n;

                        foreach (var (__i, __n) in lv.vars)
                        {
                            i = __i;
                            n = __n;
                            if (!liveout.vars.Get(int32(i)))
                            {
                                continue;
                            }

                            if (n.Class() == PPARAM)
                            {
                                continue; // ok
                            }

                            Fatalf("bad live variable at entry of %v: %L", lv.fn.Func.Nname, n);

                        } 

                        // Record live variables.

                        i = i__prev2;
                        n = n__prev2;
                    }

                    live = _addr_lv.livevars[index];
                    live.Or(live.val, liveout);

                } 

                // Check that no registers are live across calls.
                // For closure calls, the CALLclosure is the last use
                // of the context register, so it's dead after the call.
                index = int32(firstBitmapIndex);
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in b.Values)
                    {
                        v = __v;
                        if (lv.hasStackMap(v))
                        {
                            live = lv.livevars[index];
                            if (v.Op.IsCall() && live.regs != 0L)
                            {
                                lv.printDebug();
                                v.Fatalf("%v register %s recorded as live at call", lv.fn.Func.Nname, live.regs.niceString(lv.f.Config));
                            }

                            index++;

                        }

                    } 

                    // The liveness maps for this block are now complete. Compact them.

                    v = v__prev2;
                }

                lv.compact(b);

            } 

            // Done compacting. Throw out the stack map set.
            lv.stackMaps = lv.stackMapSet.extractUniqe();
            lv.stackMapSet = new bvecSet(); 

            // Useful sanity check: on entry to the function,
            // the only things that can possibly be live are the
            // input parameters.
            {
                var n__prev1 = n;

                foreach (var (__j, __n) in lv.vars)
                {
                    j = __j;
                    n = __n;
                    if (n.Class() != PPARAM && lv.stackMaps[0L].Get(int32(j)))
                    {
                        lv.f.Fatalf("%v %L recorded as live on entry", lv.fn.Func.Nname, n);
                    }

                }

                n = n__prev1;
            }

            if (!go115ReduceLiveness)
            { 
                // Check that no registers are live at function entry.
                // The context register, if any, comes from a
                // LoweredGetClosurePtr operation first thing in the function,
                // so it doesn't appear live at entry.
                {
                    var regs = lv.regMaps[0L];

                    if (regs != 0L)
                    {
                        lv.printDebug();
                        lv.f.Fatalf("%v register %s recorded as live on entry", lv.fn.Func.Nname, regs.niceString(lv.f.Config));
                    }

                }

            }

        }

        // Compact coalesces identical bitmaps from lv.livevars into the sets
        // lv.stackMapSet and lv.regMaps.
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
        private static void compact(this ptr<Liveness> _addr_lv, ptr<ssa.Block> _addr_b)
        {
            ref Liveness lv = ref _addr_lv.val;
            ref ssa.Block b = ref _addr_b.val;

            Func<varRegVec, bool, LivenessIndex> add = (live, isUnsafePoint) =>
            { // only if !go115ReduceLiveness
                // Deduplicate the stack map.
                var stackIndex = lv.stackMapSet.add(live.vars); 
                // Deduplicate the register map.
                var (regIndex, ok) = lv.regMapSet[live.regs];
                if (!ok)
                {
                    regIndex = len(lv.regMapSet);
                    lv.regMapSet[live.regs] = regIndex;
                    lv.regMaps = append(lv.regMaps, live.regs);
                }

                return new LivenessIndex(stackIndex,regIndex,isUnsafePoint);

            }
;
            long pos = 0L;
            if (b == lv.f.Entry)
            { 
                // Handle entry stack map.
                if (!go115ReduceLiveness)
                {
                    add(lv.livevars[0L], false);
                }
                else
                {
                    lv.stackMapSet.add(lv.livevars[0L].vars);
                }

                pos++;

            }

            foreach (var (_, v) in b.Values)
            {
                if (go115ReduceLiveness)
                {
                    var hasStackMap = lv.hasStackMap(v);
                    var isUnsafePoint = lv.allUnsafe || lv.unsafePoints.Get(int32(v.ID));
                    LivenessIndex idx = new LivenessIndex(StackMapDontCare,StackMapDontCare,isUnsafePoint);
                    if (hasStackMap)
                    {
                        idx.stackMapIndex = lv.stackMapSet.add(lv.livevars[pos].vars);
                        pos++;
                    }

                    if (hasStackMap || isUnsafePoint)
                    {
                        lv.livenessMap.set(v, idx);
                    }

                }
                else if (lv.hasStackMap(v))
                {
                    isUnsafePoint = lv.allUnsafe || lv.unsafePoints.Get(int32(v.ID));
                    lv.livenessMap.set(v, add(lv.livevars[pos], isUnsafePoint));
                    pos++;
                }

            } 

            // Reset livevars.
            lv.livevars = lv.livevars[..0L];

        }

        private static void showlive(this ptr<Liveness> _addr_lv, ptr<ssa.Value> _addr_v, bvec live)
        {
            ref Liveness lv = ref _addr_lv.val;
            ref ssa.Value v = ref _addr_v.val;

            if (debuglive == 0L || lv.fn.funcname() == "init" || strings.HasPrefix(lv.fn.funcname(), "."))
            {
                return ;
            }

            if (!(v == null || v.Op.IsCall()))
            { 
                // Historically we only printed this information at
                // calls. Keep doing so.
                return ;

            }

            if (live.IsEmpty())
            {
                return ;
            }

            var pos = lv.fn.Func.Nname.Pos;
            if (v != null)
            {
                pos = v.Pos;
            }

            @string s = "live at ";
            if (v == null)
            {
                s += fmt.Sprintf("entry to %s:", lv.fn.funcname());
            }            {
                ptr<obj.LSym> (sym, ok) = v.Aux._<ptr<obj.LSym>>();


                else if (ok)
                {
                    var fn = sym.Name;
                    {
                        var pos__prev3 = pos;

                        pos = strings.Index(fn, ".");

                        if (pos >= 0L)
                        {
                            fn = fn[pos + 1L..];
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


            foreach (var (j, n) in lv.vars)
            {
                if (live.Get(int32(j)))
                {
                    s += fmt.Sprintf(" %v", n);
                }

            }
            Warnl(pos, s);

        }

        private static bool printbvec(this ptr<Liveness> _addr_lv, bool printed, @string name, varRegVec live)
        {
            ref Liveness lv = ref _addr_lv.val;

            if (live.vars.IsEmpty() && live.regs == 0L)
            {
                return printed;
            }

            if (!printed)
            {
                fmt.Printf("\t");
            }
            else
            {
                fmt.Printf(" ");
            }

            fmt.Printf("%s=", name);

            @string comma = "";
            foreach (var (i, n) in lv.vars)
            {
                if (!live.vars.Get(int32(i)))
                {
                    continue;
                }

                fmt.Printf("%s%s", comma, n.Sym.Name);
                comma = ",";

            }
            fmt.Printf("%s%s", comma, live.regs.niceString(lv.f.Config));
            return true;

        }

        // printeffect is like printbvec, but for valueEffects and regEffects.
        private static bool printeffect(this ptr<Liveness> _addr_lv, bool printed, @string name, int pos, bool x, liveRegMask regMask)
        {
            ref Liveness lv = ref _addr_lv.val;

            if (!x && regMask == 0L)
            {
                return printed;
            }

            if (!printed)
            {
                fmt.Printf("\t");
            }
            else
            {
                fmt.Printf(" ");
            }

            fmt.Printf("%s=", name);
            if (x)
            {
                fmt.Printf("%s", lv.vars[pos].Sym.Name);
            }

            foreach (var (j, reg) in lv.f.Config.GCRegMap)
            {
                if (regMask & (1L << (int)(uint(j))) != 0L)
                {
                    if (x)
                    {
                        fmt.Printf(",");
                    }

                    x = true;
                    fmt.Printf("%v", reg);

                }

            }
            return true;

        }

        // Prints the computed liveness information and inputs, for debugging.
        // This format synthesizes the information used during the multiple passes
        // into a single presentation.
        private static void printDebug(this ptr<Liveness> _addr_lv)
        {
            ref Liveness lv = ref _addr_lv.val;

            fmt.Printf("liveness: %s\n", lv.fn.funcname());

            foreach (var (i, b) in lv.f.Blocks)
            {
                if (i > 0L)
                {
                    fmt.Printf("\n");
                } 

                // bb#0 pred=1,2 succ=3,4
                fmt.Printf("bb#%d pred=", b.ID);
                {
                    var j__prev2 = j;

                    foreach (var (__j, __pred) in b.Preds)
                    {
                        j = __j;
                        pred = __pred;
                        if (j > 0L)
                        {
                            fmt.Printf(",");
                        }

                        fmt.Printf("%d", pred.Block().ID);

                    }

                    j = j__prev2;
                }

                fmt.Printf(" succ=");
                {
                    var j__prev2 = j;

                    foreach (var (__j, __succ) in b.Succs)
                    {
                        j = __j;
                        succ = __succ;
                        if (j > 0L)
                        {
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
                if (printed)
                {
                    fmt.Printf("\n");
                } 

                // program listing, with individual effects listed
                if (b == lv.f.Entry)
                {
                    var live = lv.stackMaps[0L];
                    fmt.Printf("(%s) function entry\n", linestr(lv.fn.Func.Nname.Pos));
                    fmt.Printf("\tlive=");
                    printed = false;
                    {
                        var j__prev2 = j;
                        var n__prev2 = n;

                        foreach (var (__j, __n) in lv.vars)
                        {
                            j = __j;
                            n = __n;
                            if (!live.Get(int32(j)))
                            {
                                continue;
                            }

                            if (printed)
                            {
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

                foreach (var (_, v) in b.Values)
                {
                    fmt.Printf("(%s) %v\n", linestr(v.Pos), v.LongString());

                    var pcdata = lv.livenessMap.Get(v);

                    var (pos, effect) = lv.valueEffects(v);
                    var (regUevar, regKill) = lv.regEffects(v);
                    printed = false;
                    printed = lv.printeffect(printed, "uevar", pos, effect & uevar != 0L, regUevar);
                    printed = lv.printeffect(printed, "varkill", pos, effect & varkill != 0L, regKill);
                    if (printed)
                    {
                        fmt.Printf("\n");
                    }

                    if (pcdata.StackMapValid() || pcdata.RegMapValid())
                    {
                        fmt.Printf("\tlive=");
                        printed = false;
                        if (pcdata.StackMapValid())
                        {
                            live = lv.stackMaps[pcdata.stackMapIndex];
                            {
                                var j__prev3 = j;
                                var n__prev3 = n;

                                foreach (var (__j, __n) in lv.vars)
                                {
                                    j = __j;
                                    n = __n;
                                    if (!live.Get(int32(j)))
                                    {
                                        continue;
                                    }

                                    if (printed)
                                    {
                                        fmt.Printf(",");
                                    }

                                    fmt.Printf("%v", n);
                                    printed = true;

                                }

                                j = j__prev3;
                                n = n__prev3;
                            }
                        }

                        if (pcdata.RegMapValid())
                        { // only if !go115ReduceLiveness
                            var regLive = lv.regMaps[pcdata.regMapIndex];
                            if (regLive != 0L)
                            {
                                if (printed)
                                {
                                    fmt.Printf(",");
                                }

                                fmt.Printf("%s", regLive.niceString(lv.f.Config));
                                printed = true;

                            }

                        }

                        fmt.Printf("\n");

                    }

                    if (pcdata.isUnsafePoint)
                    {
                        fmt.Printf("\tunsafe-point\n");
                    }

                } 

                // bb bitsets
                fmt.Printf("end\n");
                printed = false;
                printed = lv.printbvec(printed, "varkill", be.varkill);
                printed = lv.printbvec(printed, "liveout", be.liveout);
                if (printed)
                {
                    fmt.Printf("\n");
                }

            }
            fmt.Printf("\n");

        }

        // Dumps a slice of bitmaps to a symbol as a sequence of uint32 values. The
        // first word dumped is the total number of bitmaps. The second word is the
        // length of the bitmaps. All bitmaps are assumed to be of equal length. The
        // remaining bytes are the raw bitmaps.
        private static (ptr<obj.LSym>, ptr<obj.LSym>, ptr<obj.LSym>) emit(this ptr<Liveness> _addr_lv)
        {
            ptr<obj.LSym> argsSym = default!;
            ptr<obj.LSym> liveSym = default!;
            ptr<obj.LSym> regsSym = default!;
            ref Liveness lv = ref _addr_lv.val;
 
            // Size args bitmaps to be just large enough to hold the largest pointer.
            // First, find the largest Xoffset node we care about.
            // (Nodes without pointers aren't in lv.vars; see livenessShouldTrack.)
            ptr<Node> maxArgNode;
            foreach (var (_, n) in lv.vars)
            {

                if (n.Class() == PPARAM || n.Class() == PPARAMOUT) 
                    if (maxArgNode == null || n.Xoffset > maxArgNode.Xoffset)
                    {
                        maxArgNode = n;
                    }

                            } 
            // Next, find the offset of the largest pointer in the largest node.
            long maxArgs = default;
            if (maxArgNode != null)
            {
                maxArgs = maxArgNode.Xoffset + typeptrdata(maxArgNode.Type);
            } 

            // Size locals bitmaps to be stkptrsize sized.
            // We cannot shrink them to only hold the largest pointer,
            // because their size is used to calculate the beginning
            // of the local variables frame.
            // Further discussion in https://golang.org/cl/104175.
            // TODO: consider trimming leading zeros.
            // This would require shifting all bitmaps.
            var maxLocals = lv.stkptrsize; 

            // Temporary symbols for encoding bitmaps.
            ref obj.LSym argsSymTmp = ref heap(out ptr<obj.LSym> _addr_argsSymTmp);            ref obj.LSym liveSymTmp = ref heap(out ptr<obj.LSym> _addr_liveSymTmp);            ref obj.LSym regsSymTmp = ref heap(out ptr<obj.LSym> _addr_regsSymTmp);



            var args = bvalloc(int32(maxArgs / int64(Widthptr)));
            var aoff = duint32(_addr_argsSymTmp, 0L, uint32(len(lv.stackMaps))); // number of bitmaps
            aoff = duint32(_addr_argsSymTmp, aoff, uint32(args.n)); // number of bits in each bitmap

            var locals = bvalloc(int32(maxLocals / int64(Widthptr)));
            var loff = duint32(_addr_liveSymTmp, 0L, uint32(len(lv.stackMaps))); // number of bitmaps
            loff = duint32(_addr_liveSymTmp, loff, uint32(locals.n)); // number of bits in each bitmap

            {
                var live__prev1 = live;

                foreach (var (_, __live) in lv.stackMaps)
                {
                    live = __live;
                    args.Clear();
                    locals.Clear();

                    lv.pointerMap(live, lv.vars, args, locals);

                    aoff = dbvec(_addr_argsSymTmp, aoff, args);
                    loff = dbvec(_addr_liveSymTmp, loff, locals);
                }

                live = live__prev1;
            }

            if (!go115ReduceLiveness)
            {
                var regs = bvalloc(lv.usedRegs());
                var roff = duint32(_addr_regsSymTmp, 0L, uint32(len(lv.regMaps))); // number of bitmaps
                roff = duint32(_addr_regsSymTmp, roff, uint32(regs.n)); // number of bits in each bitmap
                if (regs.n > 32L)
                { 
                    // Our uint32 conversion below won't work.
                    Fatalf("GP registers overflow uint32");

                }

                if (regs.n > 0L)
                {
                    {
                        var live__prev1 = live;

                        foreach (var (_, __live) in lv.regMaps)
                        {
                            live = __live;
                            regs.Clear();
                            regs.b[0L] = uint32(live);
                            roff = dbvec(_addr_regsSymTmp, roff, regs);
                        }

                        live = live__prev1;
                    }
                }

            } 

            // Give these LSyms content-addressable names,
            // so that they can be de-duplicated.
            // This provides significant binary size savings.
            //
            // These symbols will be added to Ctxt.Data by addGCLocals
            // after parallel compilation is done.
            Func<ptr<obj.LSym>, ptr<obj.LSym>> makeSym = tmpSym =>
            {
                return _addr_Ctxt.LookupInit(fmt.Sprintf("gclocals·%x", md5.Sum(tmpSym.P)), lsym =>
                {
                    lsym.P = tmpSym.P;
                })!;

            }
;
            if (!go115ReduceLiveness)
            {
                return (_addr_makeSym(_addr_argsSymTmp)!, _addr_makeSym(_addr_liveSymTmp)!, _addr_makeSym(_addr_regsSymTmp)!);
            } 
            // TODO(go115ReduceLiveness): Remove regsSym result
            return (_addr_makeSym(_addr_argsSymTmp)!, _addr_makeSym(_addr_liveSymTmp)!, _addr_null!);

        }

        // Entry pointer for liveness analysis. Solves for the liveness of
        // pointer variables in the function and emits a runtime data
        // structure read by the garbage collector.
        // Returns a map from GC safe points to their corresponding stack map index.
        private static LivenessMap liveness(ptr<ssafn> _addr_e, ptr<ssa.Func> _addr_f, ptr<Progs> _addr_pp)
        {
            ref ssafn e = ref _addr_e.val;
            ref ssa.Func f = ref _addr_f.val;
            ref Progs pp = ref _addr_pp.val;
 
            // Construct the global liveness state.
            var (vars, idx) = getvariables(_addr_e.curfn);
            var lv = newliveness(_addr_e.curfn, _addr_f, vars, idx, e.stkptrsize); 

            // Run the dataflow framework.
            lv.prologue();
            lv.solve();
            lv.epilogue();
            if (debuglive > 0L)
            {
                lv.showlive(null, lv.stackMaps[0L]);
                foreach (var (_, b) in f.Blocks)
                {
                    foreach (var (_, val) in b.Values)
                    {
                        {
                            var idx = lv.livenessMap.Get(val);

                            if (idx.StackMapValid())
                            {
                                lv.showlive(val, lv.stackMaps[idx.stackMapIndex]);
                            }

                        }

                    }

                }

            }

            if (debuglive >= 2L)
            {
                lv.printDebug();
            } 

            // Update the function cache.
            {
                ptr<livenessFuncCache> cache = f.Cache.Liveness._<ptr<livenessFuncCache>>();
                if (cap(lv.be) < 2000L)
                { // Threshold from ssa.Cache slices.
                    foreach (var (i) in lv.be)
                    {
                        lv.be[i] = new BlockEffects();
                    }
                    cache.be = lv.be;

                }

                if (len(lv.livenessMap.vals) < 2000L)
                {
                    cache.livenessMap = lv.livenessMap;
                }

            } 

            // Emit the live pointer map data structures
            var ls = e.curfn.Func.lsym;
            ls.Func.GCArgs, ls.Func.GCLocals, ls.Func.GCRegs = lv.emit();

            var p = pp.Prog(obj.AFUNCDATA);
            Addrconst(_addr_p.From, objabi.FUNCDATA_ArgsPointerMaps);
            p.To.Type = obj.TYPE_MEM;
            p.To.Name = obj.NAME_EXTERN;
            p.To.Sym = ls.Func.GCArgs;

            p = pp.Prog(obj.AFUNCDATA);
            Addrconst(_addr_p.From, objabi.FUNCDATA_LocalsPointerMaps);
            p.To.Type = obj.TYPE_MEM;
            p.To.Name = obj.NAME_EXTERN;
            p.To.Sym = ls.Func.GCLocals;

            if (!go115ReduceLiveness)
            {
                p = pp.Prog(obj.AFUNCDATA);
                Addrconst(_addr_p.From, objabi.FUNCDATA_RegPointerMaps);
                p.To.Type = obj.TYPE_MEM;
                p.To.Name = obj.NAME_EXTERN;
                p.To.Sym = ls.Func.GCRegs;
            }

            return lv.livenessMap;

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
        private static bool isfat(ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            if (t != null)
            {

                if (t.Etype == TSLICE || t.Etype == TSTRING || t.Etype == TINTER) // maybe remove later
                    return true;
                else if (t.Etype == TARRAY) 
                    // Array of 1 element, check if element is fat
                    if (t.NumElem() == 1L)
                    {
                        return isfat(_addr_t.Elem());
                    }

                    return true;
                else if (t.Etype == TSTRUCT) 
                    // Struct with 1 field, check if field is fat
                    if (t.NumFields() == 1L)
                    {
                        return isfat(_addr_t.Field(0L).Type);
                    }

                    return true;
                
            }

            return false;

        }
    }
}}}}
