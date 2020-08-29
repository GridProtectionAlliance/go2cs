// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// TODO: live at start of block instead?

// package ssa -- go2cs converted at 2020 August 29 09:24:23 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\stackalloc.go
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        private partial struct stackAllocState
        {
            public ptr<Func> f; // live is the output of stackalloc.
// live[b.id] = live values at the end of block b.
            public slice<slice<ID>> live; // The following slices are reused across multiple users
// of stackAllocState.
            public slice<stackValState> values;
            public slice<slice<ID>> interfere; // interfere[v.id] = values that interfere with v.
            public slice<LocalSlot> names;
            public slice<long> slots;
            public slice<bool> used;
            public int nArgSlot; // Number of self-interferences
            public int nNotNeed; // Number of self-interferences
            public int nNamedSlot; // Number of self-interferences
            public int nReuse; // Number of self-interferences
            public int nAuto; // Number of self-interferences
            public int nSelfInterfere; // Number of self-interferences
        }

        private static ref stackAllocState newStackAllocState(ref Func f)
        {
            var s = f.Cache.stackAllocState;
            if (s == null)
            {
                return @new<stackAllocState>();
            }
            if (s.f != null)
            {
                f.fe.Fatalf(src.NoXPos, "newStackAllocState called without previous free");
            }
            return s;
        }

        private static void putStackAllocState(ref stackAllocState s)
        {
            {
                var i__prev1 = i;

                foreach (var (__i) in s.values)
                {
                    i = __i;
                    s.values[i] = new stackValState();
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in s.interfere)
                {
                    i = __i;
                    s.interfere[i] = null;
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in s.names)
                {
                    i = __i;
                    s.names[i] = new LocalSlot();
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in s.slots)
                {
                    i = __i;
                    s.slots[i] = 0L;
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in s.used)
                {
                    i = __i;
                    s.used[i] = false;
                }

                i = i__prev1;
            }

            s.f.Cache.stackAllocState = s;
            s.f = null;
            s.live = null;
            s.nArgSlot = 0L;
            s.nNotNeed = 0L;
            s.nNamedSlot = 0L;
            s.nReuse = 0L;
            s.nAuto = 0L;
            s.nSelfInterfere = 0L;
        }

        private partial struct stackValState
        {
            public ptr<types.Type> typ;
            public ptr<Value> spill;
            public bool needSlot;
            public bool isArg;
        }

        // stackalloc allocates storage in the stack frame for
        // all Values that did not get a register.
        // Returns a map from block ID to the stack values live at the end of that block.
        private static slice<slice<ID>> @stackalloc(ref Func _f, slice<slice<ID>> spillLive) => func(_f, (ref Func f, Defer defer, Panic _, Recover __) =>
        {
            if (f.pass.debug > stackDebug)
            {
                fmt.Println("before stackalloc");
                fmt.Println(f.String());
            }
            var s = newStackAllocState(f);
            s.init(f, spillLive);
            defer(putStackAllocState(s));

            s.@stackalloc();
            if (f.pass.stats > 0L)
            {
                f.LogStat("stack_alloc_stats", s.nArgSlot, "arg_slots", s.nNotNeed, "slot_not_needed", s.nNamedSlot, "named_slots", s.nAuto, "auto_slots", s.nReuse, "reused_slots", s.nSelfInterfere, "self_interfering");
            }
            return s.live;
        });

        private static void init(this ref stackAllocState s, ref Func f, slice<slice<ID>> spillLive)
        {
            s.f = f; 

            // Initialize value information.
            {
                var n = f.NumValues();

                if (cap(s.values) >= n)
                {
                    s.values = s.values[..n];
                }
                else
                {
                    s.values = make_slice<stackValState>(n);
                }

            }
            foreach (var (_, b) in f.Blocks)
            {
                foreach (var (_, v) in b.Values)
                {
                    s.values[v.ID].typ = v.Type;
                    s.values[v.ID].needSlot = !v.Type.IsMemory() && !v.Type.IsVoid() && !v.Type.IsFlags() && f.getHome(v.ID) == null && !v.rematerializeable();
                    s.values[v.ID].isArg = v.Op == OpArg;
                    if (f.pass.debug > stackDebug && s.values[v.ID].needSlot)
                    {
                        fmt.Printf("%s needs a stack slot\n", v);
                    }
                    if (v.Op == OpStoreReg)
                    {
                        s.values[v.Args[0L].ID].spill = v;
                    }
                }
            } 

            // Compute liveness info for values needing a slot.
            s.computeLive(spillLive); 

            // Build interference graph among values needing a slot.
            s.buildInterferenceGraph();
        }

        private static void @stackalloc(this ref stackAllocState s)
        {
            var f = s.f; 

            // Build map from values to their names, if any.
            // A value may be associated with more than one name (e.g. after
            // the assignment i=j). This step picks one name per value arbitrarily.
            {
                var n__prev1 = n;

                var n = f.NumValues();

                if (cap(s.names) >= n)
                {
                    s.names = s.names[..n];
                }
                else
                {
                    s.names = make_slice<LocalSlot>(n);
                }

                n = n__prev1;

            }
            var names = s.names;
            {
                var name__prev1 = name;

                foreach (var (_, __name) in f.Names)
                {
                    name = __name; 
                    // Note: not "range f.NamedValues" above, because
                    // that would be nondeterministic.
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in f.NamedValues[name])
                        {
                            v = __v;
                            names[v.ID] = name;
                        }

                        v = v__prev2;
                    }

                } 

                // Allocate args to their assigned locations.

                name = name__prev1;
            }

            {
                var v__prev1 = v;

                foreach (var (_, __v) in f.Entry.Values)
                {
                    v = __v;
                    if (v.Op != OpArg)
                    {
                        continue;
                    }
                    LocalSlot loc = new LocalSlot(N:v.Aux.(GCNode),Type:v.Type,Off:v.AuxInt);
                    if (f.pass.debug > stackDebug)
                    {
                        fmt.Printf("stackalloc %s to %s\n", v, loc);
                    }
                    f.setHome(v, loc);
                } 

                // For each type, we keep track of all the stack slots we
                // have allocated for that type.
                // TODO: share slots among equivalent types. We would need to
                // only share among types with the same GC signature. See the
                // type.Equal calls below for where this matters.

                v = v__prev1;
            }

            map locations = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ref types.Type, slice<LocalSlot>>{}; 

            // Each time we assign a stack slot to a value v, we remember
            // the slot we used via an index into locations[v.Type].
            var slots = s.slots;
            {
                var n__prev1 = n;

                n = f.NumValues();

                if (cap(slots) >= n)
                {
                    slots = slots[..n];
                }
                else
                {
                    slots = make_slice<long>(n);
                    s.slots = slots;
                }

                n = n__prev1;

            }
            {
                var i__prev1 = i;

                foreach (var (__i) in slots)
                {
                    i = __i;
                    slots[i] = -1L;
                } 

                // Pick a stack slot for each value needing one.

                i = i__prev1;
            }

            slice<bool> used = default;
            {
                var n__prev1 = n;

                n = f.NumValues();

                if (cap(s.used) >= n)
                {
                    used = s.used[..n];
                }
                else
                {
                    used = make_slice<bool>(n);
                    s.used = used;
                }

                n = n__prev1;

            }
            foreach (var (_, b) in f.Blocks)
            {
                {
                    var v__prev2 = v;

                    foreach (var (_, __v) in b.Values)
                    {
                        v = __v;
                        if (!s.values[v.ID].needSlot)
                        {
                            s.nNotNeed++;
                            continue;
                        }
                        if (v.Op == OpArg)
                        {
                            s.nArgSlot++;
                            continue; // already picked
                        } 

                        // If this is a named value, try to use the name as
                        // the spill location.
                        LocalSlot name = default;
                        if (v.Op == OpStoreReg)
                        {
                            name = names[v.Args[0L].ID];
                        }
                        else
                        {
                            name = names[v.ID];
                        }
                        if (name.N != null && v.Type.Compare(name.Type) == types.CMPeq)
                        {
                            foreach (var (_, id) in s.interfere[v.ID])
                            {
                                var h = f.getHome(id);
                                if (h != null && h._<LocalSlot>().N == name.N && h._<LocalSlot>().Off == name.Off)
                                { 
                                    // A variable can interfere with itself.
                                    // It is rare, but but it can happen.
                                    s.nSelfInterfere++;
                                    goto noname;
                                }
                            }
                            if (f.pass.debug > stackDebug)
                            {
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

                            for (long i = 0L; i < len(locs); i++)
                            {
                                used[i] = false;
                            }


                            i = i__prev3;
                        }
                        foreach (var (_, xid) in s.interfere[v.ID])
                        {
                            var slot = slots[xid];
                            if (slot >= 0L)
                            {
                                used[slot] = true;
                            }
                        } 
                        // Find an unused stack slot.
                        i = default;
                        for (i = 0L; i < len(locs); i++)
                        {
                            if (!used[i])
                            {
                                s.nReuse++;
                                break;
                            }
                        } 
                        // If there is no unused stack slot, allocate a new one.
 
                        // If there is no unused stack slot, allocate a new one.
                        if (i == len(locs))
                        {
                            s.nAuto++;
                            locs = append(locs, new LocalSlot(N:f.fe.Auto(v.Pos,v.Type),Type:v.Type,Off:0));
                            locations[v.Type] = locs;
                        } 
                        // Use the stack variable at that index for v.
                        loc = locs[i];
                        if (f.pass.debug > stackDebug)
                        {
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
        private static void computeLive(this ref stackAllocState _s, slice<slice<ID>> spillLive) => func(_s, (ref stackAllocState s, Defer defer, Panic _, Recover __) =>
        {
            s.live = make_slice<slice<ID>>(s.f.NumBlocks());
            slice<ref Value> phis = default;
            var live = s.f.newSparseSet(s.f.NumValues());
            defer(s.f.retSparseSet(live));
            var t = s.f.newSparseSet(s.f.NumValues());
            defer(s.f.retSparseSet(t)); 

            // Instead of iterating over f.Blocks, iterate over their postordering.
            // Liveness information flows backward, so starting at the end
            // increases the probability that we will stabilize quickly.
            var po = s.f.postorder();
            while (true)
            {
                var changed = false;
                {
                    var b__prev2 = b;

                    foreach (var (_, __b) in po)
                    {
                        b = __b; 
                        // Start with known live values at the end of the block
                        live.clear();
                        live.addAll(s.live[b.ID]); 

                        // Propagate backwards to the start of the block
                        phis = phis[..0L];
                        {
                            var i__prev3 = i;

                            for (var i = len(b.Values) - 1L; i >= 0L; i--)
                            {
                                var v = b.Values[i];
                                live.remove(v.ID);
                                if (v.Op == OpPhi)
                                { 
                                    // Save phi for later.
                                    // Note: its args might need a stack slot even though
                                    // the phi itself doesn't. So don't use needSlot.
                                    if (!v.Type.IsMemory() && !v.Type.IsVoid())
                                    {
                                        phis = append(phis, v);
                                    }
                                    continue;
                                }
                                {
                                    var a__prev4 = a;

                                    foreach (var (_, __a) in v.Args)
                                    {
                                        a = __a;
                                        if (s.values[a.ID].needSlot)
                                        {
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

                            foreach (var (__i, __e) in b.Preds)
                            {
                                i = __i;
                                e = __e;
                                var p = e.b;
                                t.clear();
                                t.addAll(s.live[p.ID]);
                                t.addAll(live.contents());
                                t.addAll(spillLive[p.ID]);
                                {
                                    var v__prev4 = v;

                                    foreach (var (_, __v) in phis)
                                    {
                                        v = __v;
                                        var a = v.Args[i];
                                        if (s.values[a.ID].needSlot)
                                        {
                                            t.add(a.ID);
                                        }
                                        {
                                            var spill = s.values[a.ID].spill;

                                            if (spill != null)
                                            { 
                                                //TODO: remove?  Subsumed by SpillUse?
                                                t.add(spill.ID);
                                            }

                                        }
                                    }

                                    v = v__prev4;
                                }

                                if (t.size() == len(s.live[p.ID]))
                                {
                                    continue;
                                } 
                                // grow p's live set
                                s.live[p.ID] = append(s.live[p.ID][..0L], t.contents());
                                changed = true;
                            }

                            i = i__prev3;
                        }

                    }

                    b = b__prev2;
                }

                if (!changed)
                {
                    break;
                }
            }

            if (s.f.pass.debug > stackDebug)
            {
                {
                    var b__prev1 = b;

                    foreach (var (_, __b) in s.f.Blocks)
                    {
                        b = __b;
                        fmt.Printf("stacklive %s %v\n", b, s.live[b.ID]);
                    }

                    b = b__prev1;
                }

            }
        });

        private static Location getHome(this ref Func f, ID vid)
        {
            if (int(vid) >= len(f.RegAlloc))
            {
                return null;
            }
            return f.RegAlloc[vid];
        }

        private static void setHome(this ref Func f, ref Value v, Location loc)
        {
            while (v.ID >= ID(len(f.RegAlloc)))
            {
                f.RegAlloc = append(f.RegAlloc, null);
            }

            f.RegAlloc[v.ID] = loc;
        }

        private static void buildInterferenceGraph(this ref stackAllocState _s) => func(_s, (ref stackAllocState s, Defer defer, Panic _, Recover __) =>
        {
            var f = s.f;
            {
                var n = f.NumValues();

                if (cap(s.interfere) >= n)
                {
                    s.interfere = s.interfere[..n];
                }
                else
                {
                    s.interfere = make_slice<slice<ID>>(n);
                }

            }
            var live = f.newSparseSet(f.NumValues());
            defer(f.retSparseSet(live));
            foreach (var (_, b) in f.Blocks)
            { 
                // Propagate liveness backwards to the start of the block.
                // Two values interfere if one is defined while the other is live.
                live.clear();
                live.addAll(s.live[b.ID]);
                {
                    var i__prev2 = i;

                    for (var i = len(b.Values) - 1L; i >= 0L; i--)
                    {
                        var v = b.Values[i];
                        if (s.values[v.ID].needSlot)
                        {
                            live.remove(v.ID);
                            foreach (var (_, id) in live.contents())
                            { 
                                // Note: args can have different types and still interfere
                                // (with each other or with other values). See issue 23522.
                                if (s.values[v.ID].typ.Compare(s.values[id].typ) == types.CMPeq || v.Op == OpArg || s.values[id].isArg)
                                {
                                    s.interfere[v.ID] = append(s.interfere[v.ID], id);
                                    s.interfere[id] = append(s.interfere[id], v.ID);
                                }
                            }
                        }
                        foreach (var (_, a) in v.Args)
                        {
                            if (s.values[a.ID].needSlot)
                            {
                                live.add(a.ID);
                            }
                        }
                        if (v.Op == OpArg && s.values[v.ID].needSlot)
                        { 
                            // OpArg is an input argument which is pre-spilled.
                            // We add back v.ID here because we want this value
                            // to appear live even before this point. Being live
                            // all the way to the start of the entry block prevents other
                            // values from being allocated to the same slot and clobbering
                            // the input value before we have a chance to load it.
                            live.add(v.ID);
                        }
                    }


                    i = i__prev2;
                }
            }
            if (f.pass.debug > stackDebug)
            {
                {
                    var i__prev1 = i;

                    foreach (var (__vid, __i) in s.interfere)
                    {
                        vid = __vid;
                        i = __i;
                        if (len(i) > 0L)
                        {
                            fmt.Printf("v%d interferes with", vid);
                            foreach (var (_, x) in i)
                            {
                                fmt.Printf(" v%d", x);
                            }
                            fmt.Println();
                        }
                    }

                    i = i__prev1;
                }

            }
        });
    }
}}}}
