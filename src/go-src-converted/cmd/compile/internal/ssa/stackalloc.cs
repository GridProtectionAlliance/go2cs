// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// TODO: live at start of block instead?

// package ssa -- go2cs converted at 2022 March 06 23:08:47 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\stackalloc.go
using ir = go.cmd.compile.@internal.ir_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;

namespace go.cmd.compile.@internal;

public static partial class ssa_package {

private partial struct stackAllocState {
    public ptr<Func> f; // live is the output of stackalloc.
// live[b.id] = live values at the end of block b.
    public slice<slice<ID>> live; // The following slices are reused across multiple users
// of stackAllocState.
    public slice<stackValState> values;
    public slice<slice<ID>> interfere; // interfere[v.id] = values that interfere with v.
    public slice<LocalSlot> names;
    public slice<nint> slots;
    public slice<bool> used;
    public int nArgSlot; // Number of self-interferences
    public int nNotNeed; // Number of self-interferences
    public int nNamedSlot; // Number of self-interferences
    public int nReuse; // Number of self-interferences
    public int nAuto; // Number of self-interferences
    public int nSelfInterfere; // Number of self-interferences
}

private static ptr<stackAllocState> newStackAllocState(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    var s = f.Cache.stackAllocState;
    if (s == null) {
        return @new<stackAllocState>();
    }
    if (s.f != null) {
        f.fe.Fatalf(src.NoXPos, "newStackAllocState called without previous free");
    }
    return _addr_s!;

}

private static void putStackAllocState(ptr<stackAllocState> _addr_s) {
    ref stackAllocState s = ref _addr_s.val;

    {
        var i__prev1 = i;

        foreach (var (__i) in s.values) {
            i = __i;
            s.values[i] = new stackValState();
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i) in s.interfere) {
            i = __i;
            s.interfere[i] = null;
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i) in s.names) {
            i = __i;
            s.names[i] = new LocalSlot();
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i) in s.slots) {
            i = __i;
            s.slots[i] = 0;
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i) in s.used) {
            i = __i;
            s.used[i] = false;
        }
        i = i__prev1;
    }

    s.f.Cache.stackAllocState = s;
    s.f = null;
    s.live = null;
    (s.nArgSlot, s.nNotNeed, s.nNamedSlot, s.nReuse, s.nAuto, s.nSelfInterfere) = (0, 0, 0, 0, 0, 0);
}

private partial struct stackValState {
    public ptr<types.Type> typ;
    public ptr<Value> spill;
    public bool needSlot;
    public bool isArg;
}

// stackalloc allocates storage in the stack frame for
// all Values that did not get a register.
// Returns a map from block ID to the stack values live at the end of that block.
private static slice<slice<ID>> @stackalloc(ptr<Func> _addr_f, slice<slice<ID>> spillLive) => func((defer, _, _) => {
    ref Func f = ref _addr_f.val;

    if (f.pass.debug > stackDebug) {
        fmt.Println("before stackalloc");
        fmt.Println(f.String());
    }
    var s = newStackAllocState(_addr_f);
    s.init(f, spillLive);
    defer(putStackAllocState(_addr_s));

    s.@stackalloc();
    if (f.pass.stats > 0) {
        f.LogStat("stack_alloc_stats", s.nArgSlot, "arg_slots", s.nNotNeed, "slot_not_needed", s.nNamedSlot, "named_slots", s.nAuto, "auto_slots", s.nReuse, "reused_slots", s.nSelfInterfere, "self_interfering");
    }
    return s.live;

});

private static void init(this ptr<stackAllocState> _addr_s, ptr<Func> _addr_f, slice<slice<ID>> spillLive) {
    ref stackAllocState s = ref _addr_s.val;
    ref Func f = ref _addr_f.val;

    s.f = f; 

    // Initialize value information.
    {
        var n = f.NumValues();

        if (cap(s.values) >= n) {
            s.values = s.values[..(int)n];
        }
        else
 {
            s.values = make_slice<stackValState>(n);
        }
    }

    foreach (var (_, b) in f.Blocks) {
        foreach (var (_, v) in b.Values) {
            s.values[v.ID].typ = v.Type;
            s.values[v.ID].needSlot = !v.Type.IsMemory() && !v.Type.IsVoid() && !v.Type.IsFlags() && f.getHome(v.ID) == null && !v.rematerializeable() && !v.OnWasmStack;
            s.values[v.ID].isArg = hasAnyArgOp(_addr_v);
            if (f.pass.debug > stackDebug && s.values[v.ID].needSlot) {
                fmt.Printf("%s needs a stack slot\n", v);
            }
            if (v.Op == OpStoreReg) {
                s.values[v.Args[0].ID].spill = v;
            }
        }
    }    s.computeLive(spillLive); 

    // Build interference graph among values needing a slot.
    s.buildInterferenceGraph();

}

private static void @stackalloc(this ptr<stackAllocState> _addr_s) {
    ref stackAllocState s = ref _addr_s.val;

    var f = s.f; 

    // Build map from values to their names, if any.
    // A value may be associated with more than one name (e.g. after
    // the assignment i=j). This step picks one name per value arbitrarily.
    {
        var n__prev1 = n;

        var n = f.NumValues();

        if (cap(s.names) >= n) {
            s.names = s.names[..(int)n];
        }
        else
 {
            s.names = make_slice<LocalSlot>(n);
        }
        n = n__prev1;

    }

    var names = s.names;
    LocalSlot empty = new LocalSlot();
    {
        var name__prev1 = name;

        foreach (var (_, __name) in f.Names) {
            name = __name; 
            // Note: not "range f.NamedValues" above, because
            // that would be nondeterministic.
            {
                var v__prev2 = v;

                foreach (var (_, __v) in f.NamedValues[name.val]) {
                    v = __v;
                    if (v.Op == OpArgIntReg || v.Op == OpArgFloatReg) {
                        ptr<AuxNameOffset> aux = v.Aux._<ptr<AuxNameOffset>>(); 
                        // Never let an arg be bound to a differently named thing.
                        if (name.N != aux.Name || name.Off != aux.Offset) {
                            if (f.pass.debug > stackDebug) {
                                fmt.Printf("stackalloc register arg %s skipping name %s\n", v, name);
                            }
                            continue;
                        }

                    }
                    else if (name.N.Class == ir.PPARAM && v.Op != OpArg) { 
                        // PPARAM's only bind to OpArg
                        if (f.pass.debug > stackDebug) {
                            fmt.Printf("stackalloc PPARAM name %s skipping non-Arg %s\n", name, v);
                        }

                        continue;

                    }

                    if (names[v.ID] == empty) {
                        if (f.pass.debug > stackDebug) {
                            fmt.Printf("stackalloc value %s to name %s\n", v, name.val);
                        }
                        names[v.ID] = name.val;
                    }

                }

                v = v__prev2;
            }
        }
        name = name__prev1;
    }

    {
        var v__prev1 = v;

        foreach (var (_, __v) in f.Entry.Values) {
            v = __v;
            if (!hasAnyArgOp(_addr_v)) {
                continue;
            }
            if (v.Aux == null) {
                f.Fatalf("%s has nil Aux\n", v.LongString());
            }
            if (v.Op == OpArg) {
                LocalSlot loc = new LocalSlot(N:v.Aux.(*ir.Name),Type:v.Type,Off:v.AuxInt);
                if (f.pass.debug > stackDebug) {
                    fmt.Printf("stackalloc OpArg %s to %s\n", v, loc);
                }
                f.setHome(v, loc);
                continue;
            } 
            // You might think this below would be the right idea, but you would be wrong.
            // It almost works; as of 105a6e9518 - 2021-04-23,
            // GOSSAHASH=11011011001011111 == cmd/compile/internal/noder.(*noder).embedded
            // is compiled incorrectly.  I believe the cause is one of those SSA-to-registers
            // puzzles that the register allocator untangles; in the event that a register
            // parameter does not end up bound to a name, "fixing" it is a bad idea.
            //
            //if f.DebugTest {
            //    if v.Op == OpArgIntReg || v.Op == OpArgFloatReg {
            //        aux := v.Aux.(*AuxNameOffset)
            //        loc := LocalSlot{N: aux.Name, Type: v.Type, Off: aux.Offset}
            //        if f.pass.debug > stackDebug {
            //            fmt.Printf("stackalloc Op%s %s to %s\n", v.Op, v, loc)
            //        }
            //        names[v.ID] = loc
            //        continue
            //    }
            //}
        }
        v = v__prev1;
    }

    map locations = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<types.Type>, slice<LocalSlot>>{}; 

    // Each time we assign a stack slot to a value v, we remember
    // the slot we used via an index into locations[v.Type].
    var slots = s.slots;
    {
        var n__prev1 = n;

        n = f.NumValues();

        if (cap(slots) >= n) {
            slots = slots[..(int)n];
        }
        else
 {
            slots = make_slice<nint>(n);
            s.slots = slots;
        }
        n = n__prev1;

    }

    {
        var i__prev1 = i;

        foreach (var (__i) in slots) {
            i = __i;
            slots[i] = -1;
        }
        i = i__prev1;
    }

    slice<bool> used = default;
    {
        var n__prev1 = n;

        n = f.NumValues();

        if (cap(s.used) >= n) {
            used = s.used[..(int)n];
        }
        else
 {
            used = make_slice<bool>(n);
            s.used = used;
        }
        n = n__prev1;

    }

    foreach (var (_, b) in f.Blocks) {
        {
            var v__prev2 = v;

            foreach (var (_, __v) in b.Values) {
                v = __v;
                if (!s.values[v.ID].needSlot) {
                    s.nNotNeed++;
                    continue;
                }
                if (hasAnyArgOp(_addr_v)) {
                    s.nArgSlot++;
                    continue; // already picked
                } 

                // If this is a named value, try to use the name as
                // the spill location.
                LocalSlot name = default;
                if (v.Op == OpStoreReg) {
                    name = names[v.Args[0].ID];
                }
                else
 {
                    name = names[v.ID];
                }

                if (name.N != null && v.Type.Compare(name.Type) == types.CMPeq) {
                    foreach (var (_, id) in s.interfere[v.ID]) {
                        var h = f.getHome(id);
                        if (h != null && h._<LocalSlot>().N == name.N && h._<LocalSlot>().Off == name.Off) { 
                            // A variable can interfere with itself.
                            // It is rare, but it can happen.
                            s.nSelfInterfere++;
                            goto noname;

                        }

                    }
                    if (f.pass.debug > stackDebug) {
                        fmt.Printf("stackalloc %s to %s\n", v, name);
                    }

                    s.nNamedSlot++;
                    f.setHome(v, name);
                    continue;

                }

noname: 
                // Mark all positions in locs used by interfering values.
                var locs = locations[v.Type]; 
                // Mark all positions in locs used by interfering values.
                {
                    var i__prev3 = i;

                    for (nint i = 0; i < len(locs); i++) {
                        used[i] = false;
                    }


                    i = i__prev3;
                }
                foreach (var (_, xid) in s.interfere[v.ID]) {
                    var slot = slots[xid];
                    if (slot >= 0) {
                        used[slot] = true;
                    }
                } 
                // Find an unused stack slot.
                i = default;
                for (i = 0; i < len(locs); i++) {
                    if (!used[i]) {
                        s.nReuse++;
                        break;
                    }
                } 
                // If there is no unused stack slot, allocate a new one.
 
                // If there is no unused stack slot, allocate a new one.
                if (i == len(locs)) {
                    s.nAuto++;
                    locs = append(locs, new LocalSlot(N:f.fe.Auto(v.Pos,v.Type),Type:v.Type,Off:0));
                    locations[v.Type] = locs;
                } 
                // Use the stack variable at that index for v.
                loc = locs[i];
                if (f.pass.debug > stackDebug) {
                    fmt.Printf("stackalloc %s to %s\n", v, loc);
                }

                f.setHome(v, loc);
                slots[v.ID] = i;

            }

            v = v__prev2;
        }
    }
}

// computeLive computes a map from block ID to a list of
// stack-slot-needing value IDs live at the end of that block.
// TODO: this could be quadratic if lots of variables are live across lots of
// basic blocks. Figure out a way to make this function (or, more precisely, the user
// of this function) require only linear size & time.
private static void computeLive(this ptr<stackAllocState> _addr_s, slice<slice<ID>> spillLive) => func((defer, _, _) => {
    ref stackAllocState s = ref _addr_s.val;

    s.live = make_slice<slice<ID>>(s.f.NumBlocks());
    slice<ptr<Value>> phis = default;
    var live = s.f.newSparseSet(s.f.NumValues());
    defer(s.f.retSparseSet(live));
    var t = s.f.newSparseSet(s.f.NumValues());
    defer(s.f.retSparseSet(t)); 

    // Instead of iterating over f.Blocks, iterate over their postordering.
    // Liveness information flows backward, so starting at the end
    // increases the probability that we will stabilize quickly.
    var po = s.f.postorder();
    while (true) {
        var changed = false;
        {
            var b__prev2 = b;

            foreach (var (_, __b) in po) {
                b = __b; 
                // Start with known live values at the end of the block
                live.clear();
                live.addAll(s.live[b.ID]); 

                // Propagate backwards to the start of the block
                phis = phis[..(int)0];
                {
                    var i__prev3 = i;

                    for (var i = len(b.Values) - 1; i >= 0; i--) {
                        var v = b.Values[i];
                        live.remove(v.ID);
                        if (v.Op == OpPhi) { 
                            // Save phi for later.
                            // Note: its args might need a stack slot even though
                            // the phi itself doesn't. So don't use needSlot.
                            if (!v.Type.IsMemory() && !v.Type.IsVoid()) {
                                phis = append(phis, v);
                            }

                            continue;

                        }

                        {
                            var a__prev4 = a;

                            foreach (var (_, __a) in v.Args) {
                                a = __a;
                                if (s.values[a.ID].needSlot) {
                                    live.add(a.ID);
                                }
                            }

                            a = a__prev4;
                        }
                    } 

                    // for each predecessor of b, expand its list of live-at-end values
                    // invariant: s contains the values live at the start of b (excluding phi inputs)


                    i = i__prev3;
                } 

                // for each predecessor of b, expand its list of live-at-end values
                // invariant: s contains the values live at the start of b (excluding phi inputs)
                {
                    var i__prev3 = i;

                    foreach (var (__i, __e) in b.Preds) {
                        i = __i;
                        e = __e;
                        var p = e.b;
                        t.clear();
                        t.addAll(s.live[p.ID]);
                        t.addAll(live.contents());
                        t.addAll(spillLive[p.ID]);
                        {
                            var v__prev4 = v;

                            foreach (var (_, __v) in phis) {
                                v = __v;
                                var a = v.Args[i];
                                if (s.values[a.ID].needSlot) {
                                    t.add(a.ID);
                                }
                                {
                                    var spill = s.values[a.ID].spill;

                                    if (spill != null) { 
                                        //TODO: remove?  Subsumed by SpillUse?
                                        t.add(spill.ID);

                                    }

                                }

                            }

                            v = v__prev4;
                        }

                        if (t.size() == len(s.live[p.ID])) {
                            continue;
                        } 
                        // grow p's live set
                        s.live[p.ID] = append(s.live[p.ID][..(int)0], t.contents());
                        changed = true;

                    }

                    i = i__prev3;
                }
            }

            b = b__prev2;
        }

        if (!changed) {
            break;
        }
    }
    if (s.f.pass.debug > stackDebug) {
        {
            var b__prev1 = b;

            foreach (var (_, __b) in s.f.Blocks) {
                b = __b;
                fmt.Printf("stacklive %s %v\n", b, s.live[b.ID]);
            }

            b = b__prev1;
        }
    }
});

private static Location getHome(this ptr<Func> _addr_f, ID vid) {
    ref Func f = ref _addr_f.val;

    if (int(vid) >= len(f.RegAlloc)) {
        return null;
    }
    return f.RegAlloc[vid];

}

private static void setHome(this ptr<Func> _addr_f, ptr<Value> _addr_v, Location loc) {
    ref Func f = ref _addr_f.val;
    ref Value v = ref _addr_v.val;

    while (v.ID >= ID(len(f.RegAlloc))) {
        f.RegAlloc = append(f.RegAlloc, null);
    }
    f.RegAlloc[v.ID] = loc;
}

private static void buildInterferenceGraph(this ptr<stackAllocState> _addr_s) => func((defer, _, _) => {
    ref stackAllocState s = ref _addr_s.val;

    var f = s.f;
    {
        var n = f.NumValues();

        if (cap(s.interfere) >= n) {
            s.interfere = s.interfere[..(int)n];
        }
        else
 {
            s.interfere = make_slice<slice<ID>>(n);
        }
    }

    var live = f.newSparseSet(f.NumValues());
    defer(f.retSparseSet(live));
    foreach (var (_, b) in f.Blocks) { 
        // Propagate liveness backwards to the start of the block.
        // Two values interfere if one is defined while the other is live.
        live.clear();
        live.addAll(s.live[b.ID]);
        {
            var i__prev2 = i;

            for (var i = len(b.Values) - 1; i >= 0; i--) {
                var v = b.Values[i];
                if (s.values[v.ID].needSlot) {
                    live.remove(v.ID);
                    foreach (var (_, id) in live.contents()) { 
                        // Note: args can have different types and still interfere
                        // (with each other or with other values). See issue 23522.
                        if (s.values[v.ID].typ.Compare(s.values[id].typ) == types.CMPeq || hasAnyArgOp(_addr_v) || s.values[id].isArg) {
                            s.interfere[v.ID] = append(s.interfere[v.ID], id);
                            s.interfere[id] = append(s.interfere[id], v.ID);
                        }

                    }

                }

                foreach (var (_, a) in v.Args) {
                    if (s.values[a.ID].needSlot) {
                        live.add(a.ID);
                    }
                }
                if (hasAnyArgOp(_addr_v) && s.values[v.ID].needSlot) { 
                    // OpArg is an input argument which is pre-spilled.
                    // We add back v.ID here because we want this value
                    // to appear live even before this point. Being live
                    // all the way to the start of the entry block prevents other
                    // values from being allocated to the same slot and clobbering
                    // the input value before we have a chance to load it.

                    // TODO(register args) this is apparently not wrong for register args -- is it necessary?
                    live.add(v.ID);

                }

            }


            i = i__prev2;
        }

    }    if (f.pass.debug > stackDebug) {
        {
            var i__prev1 = i;

            foreach (var (__vid, __i) in s.interfere) {
                vid = __vid;
                i = __i;
                if (len(i) > 0) {
                    fmt.Printf("v%d interferes with", vid);
                    foreach (var (_, x) in i) {
                        fmt.Printf(" v%d", x);
                    }
                    fmt.Println();
                }
            }

            i = i__prev1;
        }
    }
});

private static bool hasAnyArgOp(ptr<Value> _addr_v) {
    ref Value v = ref _addr_v.val;

    return v.Op == OpArg || v.Op == OpArgIntReg || v.Op == OpArgFloatReg;
}

} // end ssa_package
