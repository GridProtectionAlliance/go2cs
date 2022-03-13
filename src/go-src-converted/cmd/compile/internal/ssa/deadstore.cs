// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 13 06:00:59 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\deadstore.go
namespace go.cmd.compile.@internal;

using ir = cmd.compile.@internal.ir_package;
using types = cmd.compile.@internal.types_package;
using src = cmd.@internal.src_package;


// dse does dead-store elimination on the Function.
// Dead stores are those which are unconditionally followed by
// another store to the same location, with no intervening load.
// This implementation only works within a basic block. TODO: use something more global.

using System;
public static partial class ssa_package {

private static void dse(ptr<Func> _addr_f) => func((defer, _, _) => {
    ref Func f = ref _addr_f.val;

    slice<ptr<Value>> stores = default;
    var loadUse = f.newSparseSet(f.NumValues());
    defer(f.retSparseSet(loadUse));
    var storeUse = f.newSparseSet(f.NumValues());
    defer(f.retSparseSet(storeUse));
    var shadowed = f.newSparseMap(f.NumValues());
    defer(f.retSparseMap(shadowed));
    foreach (var (_, b) in f.Blocks) { 
        // Find all the stores in this block. Categorize their uses:
        //  loadUse contains stores which are used by a subsequent load.
        //  storeUse contains stores which are used by a subsequent store.
        loadUse.clear();
        storeUse.clear();
        stores = stores[..(int)0];
        {
            var v__prev2 = v;

            foreach (var (_, __v) in b.Values) {
                v = __v;
                if (v.Op == OpPhi) { 
                    // Ignore phis - they will always be first and can't be eliminated
                    continue;
                }
                if (v.Type.IsMemory()) {
                    stores = append(stores, v);
                    {
                        var a__prev3 = a;

                        foreach (var (_, __a) in v.Args) {
                            a = __a;
                            if (a.Block == b && a.Type.IsMemory()) {
                                storeUse.add(a.ID);
                                if (v.Op != OpStore && v.Op != OpZero && v.Op != OpVarDef && v.Op != OpVarKill) { 
                                    // CALL, DUFFCOPY, etc. are both
                                    // reads and writes.
                                    loadUse.add(a.ID);
                                }
                            }
                        }
                else

                        a = a__prev3;
                    }
                } {
                    {
                        var a__prev3 = a;

                        foreach (var (_, __a) in v.Args) {
                            a = __a;
                            if (a.Block == b && a.Type.IsMemory()) {
                                loadUse.add(a.ID);
                            }
                        }
                        a = a__prev3;
                    }
                }
            }
            v = v__prev2;
        }

        if (len(stores) == 0) {
            continue;
        }
        ptr<Value> last;
        {
            var v__prev2 = v;

            foreach (var (_, __v) in stores) {
                v = __v;
                if (storeUse.contains(v.ID)) {
                    continue;
                }
                if (last != null) {
                    b.Fatalf("two final stores - simultaneous live stores %s %s", last.LongString(), v.LongString());
                }
                last = v;
            }
            v = v__prev2;
        }

        if (last == null) {
            b.Fatalf("no last store found - cycle?");
        }
        shadowed.clear();
        var v = last;

walkloop:
        if (loadUse.contains(v.ID)) { 
            // Someone might be reading this memory state.
            // Clear all shadowed addresses.
            shadowed.clear();
        }
        if (v.Op == OpStore || v.Op == OpZero) {
            long sz = default;
            if (v.Op == OpStore) {
                sz = v.Aux._<ptr<types.Type>>().Size();
            }
            else
 { // OpZero
                sz = v.AuxInt;
            }
            {
                var shadowedSize = int64(shadowed.get(v.Args[0].ID));

                if (shadowedSize != -1 && shadowedSize >= sz) { 
                    // Modify the store/zero into a copy of the memory state,
                    // effectively eliding the store operation.
                    if (v.Op == OpStore) { 
                        // store addr value mem
                        v.SetArgs1(v.Args[2]);
                    }
                    else
 { 
                        // zero addr mem
                        v.SetArgs1(v.Args[1]);
                    }
                    v.Aux = null;
                    v.AuxInt = 0;
                    v.Op = OpCopy;
                }
                else
 {
                    if (sz > 0x7fffffff) { // work around sparseMap's int32 value type
                        sz = 0x7fffffff;
                    }
                    shadowed.set(v.Args[0].ID, int32(sz), src.NoXPos);
                }
            }
        }
        if (v.Op == OpPhi) { 
            // At start of block.  Move on to next block.
            // The memory phi, if it exists, is always
            // the first logical store in the block.
            // (Even if it isn't the first in the current b.Values order.)
            continue;
        }
        {
            var a__prev2 = a;

            foreach (var (_, __a) in v.Args) {
                a = __a;
                if (a.Block == b && a.Type.IsMemory()) {
                    v = a;
                    goto walkloop;
                }
            }
            a = a__prev2;
        }
    }
});

// elimDeadAutosGeneric deletes autos that are never accessed. To achieve this
// we track the operations that the address of each auto reaches and if it only
// reaches stores then we delete all the stores. The other operations will then
// be eliminated by the dead code elimination pass.
private static void elimDeadAutosGeneric(ptr<Func> _addr_f) => func((_, panic, _) => {
    ref Func f = ref _addr_f.val;

    var addr = make_map<ptr<Value>, ptr<ir.Name>>(); // values that the address of the auto reaches
    var elim = make_map<ptr<Value>, ptr<ir.Name>>(); // values that could be eliminated if the auto is
    ir.NameSet used = default; // used autos that must be kept

    // visit the value and report whether any of the maps are updated
    Func<ptr<Value>, bool> visit = v => {
        var args = v.Args;

        if (v.Op == OpAddr || v.Op == OpLocalAddr) 
            // Propagate the address if it points to an auto.
            ptr<ir.Name> (n, ok) = v.Aux._<ptr<ir.Name>>();
            if (!ok || n.Class != ir.PAUTO) {
                return ;
            }
            if (addr[v] == null) {
                addr[v] = n;
                changed = true;
            }
            return ;
        else if (v.Op == OpVarDef || v.Op == OpVarKill) 
            // v should be eliminated if we eliminate the auto.
            (n, ok) = v.Aux._<ptr<ir.Name>>();
            if (!ok || n.Class != ir.PAUTO) {
                return ;
            }
            if (elim[v] == null) {
                elim[v] = n;
                changed = true;
            }
            return ;
        else if (v.Op == OpVarLive) 
            // Don't delete the auto if it needs to be kept alive.

            // We depend on this check to keep the autotmp stack slots
            // for open-coded defers from being removed (since they
            // may not be used by the inline code, but will be used by
            // panic processing).
            (n, ok) = v.Aux._<ptr<ir.Name>>();
            if (!ok || n.Class != ir.PAUTO) {
                return ;
            }
            if (!used.Has(n)) {
                used.Add(n);
                changed = true;
            }
            return ;
        else if (v.Op == OpStore || v.Op == OpMove || v.Op == OpZero) 
            // v should be eliminated if we eliminate the auto.
            (n, ok) = addr[args[0]];
            if (ok && elim[v] == null) {
                elim[v] = n;
                changed = true;
            } 
            // Other args might hold pointers to autos.
            args = args[(int)1..];
        // The code below assumes that we have handled all the ops
        // with sym effects already. Sanity check that here.
        // Ignore Args since they can't be autos.
        if (v.Op.SymEffect() != SymNone && v.Op != OpArg) {
            panic("unhandled op with sym effect");
        }
        if (v.Uses == 0 && v.Op != OpNilCheck && !v.Op.IsCall() && !v.Op.HasSideEffects() || len(args) == 0) { 
            // Nil check has no use, but we need to keep it.
            // Also keep calls and values that have side effects.
            return ;
        }
        if (v.Type.IsMemory() || v.Type.IsFlags() || v.Op == OpPhi || v.MemoryArg() != null) {
            {
                var a__prev1 = a;

                foreach (var (_, __a) in args) {
                    a = __a;
                    {
                        ptr<ir.Name> n__prev2 = n;

                        (n, ok) = addr[a];

                        if (ok) {
                            if (!used.Has(n)) {
                                used.Add(n);
                                changed = true;
                            }
                        }

                        n = n__prev2;

                    }
                }

                a = a__prev1;
            }

            return ;
        }
        ptr<ir.Name> node;
        {
            var a__prev1 = a;

            foreach (var (_, __a) in args) {
                a = __a;
                {
                    ptr<ir.Name> n__prev1 = n;

                    (n, ok) = addr[a];

                    if (ok && !used.Has(n)) {
                        if (node == null) {
                            node = n;
                        }
                        else if (node != n) { 
                            // Most of the time we only see one pointer
                            // reaching an op, but some ops can take
                            // multiple pointers (e.g. NeqPtr, Phi etc.).
                            // This is rare, so just propagate the first
                            // value to keep things simple.
                            used.Add(n);
                            changed = true;
                        }
                    }

                    n = n__prev1;

                }
            }

            a = a__prev1;
        }

        if (node == null) {
            return ;
        }
        if (addr[v] == null) { 
            // The address of an auto reaches this op.
            addr[v] = node;
            changed = true;
            return ;
        }
        if (addr[v] != node) { 
            // This doesn't happen in practice, but catch it just in case.
            used.Add(node);
            changed = true;
        }
        return ;
    };

    nint iterations = 0;
    while (true) {
        if (iterations == 4) { 
            // give up
            return ;
        }
        iterations++;
        var changed = false;
        foreach (var (_, b) in f.Blocks) {
            {
                var v__prev3 = v;

                foreach (var (_, __v) in b.Values) {
                    v = __v;
                    changed = visit(v) || changed;
                } 
                // keep the auto if its address reaches a control value

                v = v__prev3;
            }

            foreach (var (_, c) in b.ControlValues()) {
                {
                    ptr<ir.Name> n__prev1 = n;

                    (n, ok) = addr[c];

                    if (ok && !used.Has(n)) {
                        used.Add(n);
                        changed = true;
                    }

                    n = n__prev1;

                }
            }
        }        if (!changed) {
            break;
        }
    } 

    // Eliminate stores to unread autos.
    {
        var v__prev1 = v;
        ptr<ir.Name> n__prev1 = n;

        foreach (var (__v, __n) in elim) {
            v = __v;
            n = __n;
            if (used.Has(n)) {
                continue;
            } 
            // replace with OpCopy
            v.SetArgs1(v.MemoryArg());
            v.Aux = null;
            v.AuxInt = 0;
            v.Op = OpCopy;
        }
        v = v__prev1;
        n = n__prev1;
    }
});

// elimUnreadAutos deletes stores (and associated bookkeeping ops VarDef and VarKill)
// to autos that are never read from.
private static void elimUnreadAutos(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;
 
    // Loop over all ops that affect autos taking note of which
    // autos we need and also stores that we might be able to
    // eliminate.
    ir.NameSet seen = default;
    slice<ptr<Value>> stores = default;
    foreach (var (_, b) in f.Blocks) {
        foreach (var (_, v) in b.Values) {
            ptr<ir.Name> (n, ok) = v.Aux._<ptr<ir.Name>>();
            if (!ok) {
                continue;
            }
            if (n.Class != ir.PAUTO) {
                continue;
            }
            var effect = v.Op.SymEffect();

            if (effect == SymNone || effect == SymWrite) 
                // If we haven't seen the auto yet
                // then this might be a store we can
                // eliminate.
                if (!seen.Has(n)) {
                    stores = append(stores, v);
                }
            else 
                // Assume the auto is needed (loaded,
                // has its address taken, etc.).
                // Note we have to check the uses
                // because dead loads haven't been
                // eliminated yet.
                if (v.Uses > 0) {
                    seen.Add(n);
                }
                    }
    }    foreach (var (_, store) in stores) {
        ptr<ir.Name> (n, _) = store.Aux._<ptr<ir.Name>>();
        if (seen.Has(n)) {
            continue;
        }
        store.SetArgs1(store.MemoryArg());
        store.Aux = null;
        store.AuxInt = 0;
        store.Op = OpCopy;
    }
}

} // end ssa_package
