// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 08:53:38 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\deadstore.go
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // dse does dead-store elimination on the Function.
        // Dead stores are those which are unconditionally followed by
        // another store to the same location, with no intervening load.
        // This implementation only works within a basic block. TODO: use something more global.
        private static void dse(ref Func _f) => func(_f, (ref Func f, Defer defer, Panic _, Recover __) =>
        {
            slice<ref Value> stores = default;
            var loadUse = f.newSparseSet(f.NumValues());
            defer(f.retSparseSet(loadUse));
            var storeUse = f.newSparseSet(f.NumValues());
            defer(f.retSparseSet(storeUse));
            var shadowed = newSparseMap(f.NumValues()); // TODO: cache
            foreach (var (_, b) in f.Blocks)
            { 
                // Find all the stores in this block. Categorize their uses:
                //  loadUse contains stores which are used by a subsequent load.
                //  storeUse contains stores which are used by a subsequent store.
                loadUse.clear();
                storeUse.clear();
                stores = stores[..0L];
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in b.Values)
                    {
                        v = __v;
                        if (v.Op == OpPhi)
                        { 
                            // Ignore phis - they will always be first and can't be eliminated
                            continue;
                        }
                        if (v.Type.IsMemory())
                        {
                            stores = append(stores, v);
                            {
                                var a__prev3 = a;

                                foreach (var (_, __a) in v.Args)
                                {
                                    a = __a;
                                    if (a.Block == b && a.Type.IsMemory())
                                    {
                                        storeUse.add(a.ID);
                                        if (v.Op != OpStore && v.Op != OpZero && v.Op != OpVarDef && v.Op != OpVarKill)
                                        { 
                                            // CALL, DUFFCOPY, etc. are both
                                            // reads and writes.
                                            loadUse.add(a.ID);
                                        }
                                    }
                                }
                        else

                                a = a__prev3;
                            }

                        }                        {
                            {
                                var a__prev3 = a;

                                foreach (var (_, __a) in v.Args)
                                {
                                    a = __a;
                                    if (a.Block == b && a.Type.IsMemory())
                                    {
                                        loadUse.add(a.ID);
                                    }
                                }
                                a = a__prev3;
                            }

                        }
                    }
                    v = v__prev2;
                }

                if (len(stores) == 0L)
                {
                    continue;
                }
                ref Value last = default;
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in stores)
                    {
                        v = __v;
                        if (storeUse.contains(v.ID))
                        {
                            continue;
                        }
                        if (last != null)
                        {
                            b.Fatalf("two final stores - simultaneous live stores %s %s", last.LongString(), v.LongString());
                        }
                        last = v;
                    }
                    v = v__prev2;
                }

                if (last == null)
                {
                    b.Fatalf("no last store found - cycle?");
                }
                shadowed.clear();
                var v = last;

walkloop:
                if (loadUse.contains(v.ID))
                { 
                    // Someone might be reading this memory state.
                    // Clear all shadowed addresses.
                    shadowed.clear();
                }
                if (v.Op == OpStore || v.Op == OpZero)
                {
                    long sz = default;
                    if (v.Op == OpStore)
                    {
                        sz = v.Aux._<ref types.Type>().Size();
                    }
                    else
                    { // OpZero
                        sz = v.AuxInt;
                    }
                    {
                        var shadowedSize = int64(shadowed.get(v.Args[0L].ID));

                        if (shadowedSize != -1L && shadowedSize >= sz)
                        { 
                            // Modify store into a copy
                            if (v.Op == OpStore)
                            { 
                                // store addr value mem
                                v.SetArgs1(v.Args[2L]);
                            }
                            else
                            { 
                                // zero addr mem
                                var typesz = v.Args[0L].Type.ElemType().Size();
                                if (sz != typesz)
                                {
                                    f.Fatalf("mismatched zero/store sizes: %d and %d [%s]", sz, typesz, v.LongString());
                                }
                                v.SetArgs1(v.Args[1L]);
                            }
                            v.Aux = null;
                            v.AuxInt = 0L;
                            v.Op = OpCopy;
                        }
                        else
                        {
                            if (sz > 0x7fffffffUL)
                            { // work around sparseMap's int32 value type
                                sz = 0x7fffffffUL;
                            }
                            shadowed.set(v.Args[0L].ID, int32(sz), src.NoXPos);
                        }
                    }
                }
                if (v.Op == OpPhi)
                { 
                    // At start of block.  Move on to next block.
                    // The memory phi, if it exists, is always
                    // the first logical store in the block.
                    // (Even if it isn't the first in the current b.Values order.)
                    continue;
                }
                {
                    var a__prev2 = a;

                    foreach (var (_, __a) in v.Args)
                    {
                        a = __a;
                        if (a.Block == b && a.Type.IsMemory())
                        {
                            v = a;
                            goto walkloop;
                        }
                    }
                    a = a__prev2;
                }

            }
        });

        // elimUnreadAutos deletes stores (and associated bookkeeping ops VarDef and VarKill)
        // to autos that are never read from.
        private static void elimUnreadAutos(ref Func f)
        { 
            // Loop over all ops that affect autos taking note of which
            // autos we need and also stores that we might be able to
            // eliminate.
            var seen = make_map<GCNode, bool>();
            slice<ref Value> stores = default;
            foreach (var (_, b) in f.Blocks)
            {
                foreach (var (_, v) in b.Values)
                {
                    GCNode (n, ok) = v.Aux._<GCNode>();
                    if (!ok)
                    {
                        continue;
                    }
                    if (n.StorageClass() != ClassAuto)
                    {
                        continue;
                    }
                    var effect = v.Op.SymEffect();

                    if (effect == SymNone || effect == SymWrite) 
                        // If we haven't seen the auto yet
                        // then this might be a store we can
                        // eliminate.
                        if (!seen[n])
                        {
                            stores = append(stores, v);
                        }
                    else 
                        // Assume the auto is needed (loaded,
                        // has its address taken, etc.).
                        // Note we have to check the uses
                        // because dead loads haven't been
                        // eliminated yet.
                        if (v.Uses > 0L)
                        {
                            seen[n] = true;
                        }
                                    }
            } 

            // Eliminate stores to unread autos.
            foreach (var (_, store) in stores)
            {
                GCNode (n, _) = store.Aux._<GCNode>();
                if (seen[n])
                {
                    continue;
                } 

                // replace store with OpCopy
                store.SetArgs1(store.MemoryArg());
                store.Aux = null;
                store.AuxInt = 0L;
                store.Op = OpCopy;
            }
        }
    }
}}}}
