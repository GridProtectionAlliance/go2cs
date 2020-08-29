// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// package ssa -- go2cs converted at 2020 August 29 08:53:42 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\debug.go
using obj = go.cmd.@internal.obj_package;
using fmt = go.fmt_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        public partial struct SlotID // : int
        {
        }

        // A FuncDebug contains all the debug information for the variables in a
        // function. Variables are identified by their LocalSlot, which may be the
        // result of decomposing a larger variable.
        public partial struct FuncDebug
        {
            public slice<ref LocalSlot> Slots; // VarSlots is the slots that represent part of user variables.
// Use this when iterating over all the slots to generate debug information.
            public slice<ref LocalSlot> VarSlots; // The blocks in the function, in program text order.
            public slice<ref BlockDebug> Blocks; // The registers of the current architecture, indexed by Register.num.
            public slice<Register> Registers;
        }

        private static @string BlockString(this ref FuncDebug f, ref BlockDebug b)
        {
            slice<@string> vars = default;

            foreach (var (slot) in f.VarSlots)
            {
                if (len(b.Variables[slot].Locations) == 0L)
                {
                    continue;
                }
                vars = append(vars, fmt.Sprintf("%v = %v", f.Slots[slot], b.Variables[slot]));
            }
            return fmt.Sprintf("{%v}", strings.Join(vars, ", "));
        }

        private static @string SlotLocsString(this ref FuncDebug f, SlotID id)
        {
            slice<@string> locs = default;
            foreach (var (_, block) in f.Blocks)
            {
                foreach (var (_, loc) in block.Variables[id].Locations)
                {
                    locs = append(locs, block.LocString(loc));
                }
            }
            return strings.Join(locs, " ");
        }

        public partial struct BlockDebug
        {
            public ptr<Block> Block; // The variables in this block, indexed by their SlotID.
            public slice<VarLocList> Variables;
        }

        private static @string LocString(this ref BlockDebug b, ref VarLoc loc)
        {
            var registers = b.Block.Func.Config.registers;

            slice<@string> storage = default;
            if (loc.OnStack)
            {
                storage = append(storage, "stack");
            }
            for (long reg = 0L; reg < 64L; reg++)
            {
                if (loc.Registers & (1L << (int)(uint8(reg))) == 0L)
                {
                    continue;
                }
                if (registers != null)
                {
                    storage = append(storage, registers[reg].String());
                }
                else
                {
                    storage = append(storage, fmt.Sprintf("reg%d", reg));
                }
            }

            if (len(storage) == 0L)
            {
                storage = append(storage, "!!!no storage!!!");
            }
            Func<ref Value, ref obj.Prog, long, @string> pos = (v, p, pc) =>
            {
                if (v == null)
                {
                    return "?";
                }
                var vStr = fmt.Sprintf("v%d", v.ID);
                if (v == BlockStart)
                {
                    vStr = fmt.Sprintf("b%dStart", b.Block.ID);
                }
                if (v == BlockEnd)
                {
                    vStr = fmt.Sprintf("b%dEnd", b.Block.ID);
                }
                if (p == null)
                {
                    return vStr;
                }
                return fmt.Sprintf("%s/%x", vStr, pc);
            }
;
            var start = pos(loc.Start, loc.StartProg, loc.StartPC);
            var end = pos(loc.End, loc.EndProg, loc.EndPC);
            return fmt.Sprintf("%v-%v@%s", start, end, strings.Join(storage, ","));

        }

        // append adds a location to the location list for slot.
        private static void append(this ref BlockDebug b, SlotID slot, ref VarLoc loc)
        {
            b.Variables[slot].append(loc);
        }

        // lastLoc returns the last VarLoc for slot, or nil if it has none.
        private static ref VarLoc lastLoc(this ref BlockDebug b, SlotID slot)
        {
            return b.Variables[slot].last();
        }

        // A VarLocList contains the locations for a variable, in program text order.
        // It will often have gaps.
        public partial struct VarLocList
        {
            public slice<ref VarLoc> Locations;
        }

        private static void append(this ref VarLocList l, ref VarLoc loc)
        {
            l.Locations = append(l.Locations, loc);
        }

        // last returns the last location in the list.
        private static ref VarLoc last(this ref VarLocList l)
        {
            if (l == null || len(l.Locations) == 0L)
            {
                return null;
            }
            return l.Locations[len(l.Locations) - 1L];
        }

        // A VarLoc describes a variable's location in a single contiguous range
        // of program text. It is generated from the SSA representation, but it
        // refers to the generated machine code, so the Values referenced are better
        // understood as PCs than actual Values, and the ranges can cross blocks.
        // The range is defined first by Values, which are then mapped to Progs
        // during genssa and finally to function PCs after assembly.
        // A variable can be on the stack and in any number of registers.
        public partial struct VarLoc
        {
            public ptr<Value> Start; // Exclusive -- the first SSA value after start that the range doesn't
// cover. A location with start == end is empty.
// The special sentinel value BlockEnd indicates that the variable survives
// to the end of the of the containing block, after all its Values and any
// control flow instructions added later.
            public ptr<Value> End; // The prog/PCs corresponding to Start and End above. These are for the
// convenience of later passes, since code generation isn't done when
// BuildFuncDebug runs.
// Control flow instructions don't correspond to a Value, so EndProg
// may point to a Prog in the next block if SurvivedBlock is true. For
// the last block, where there's no later Prog, it will be nil to indicate
// the end of the function.
            public ptr<obj.Prog> StartProg;
            public ptr<obj.Prog> EndProg;
            public long StartPC; // The registers this variable is available in. There can be more than
// one in various situations, e.g. it's being moved between registers.
            public long EndPC; // The registers this variable is available in. There can be more than
// one in various situations, e.g. it's being moved between registers.
            public RegisterSet Registers; // OnStack indicates that the variable is on the stack in the LocalSlot
// identified by StackLocation.
            public bool OnStack;
            public SlotID StackLocation;
        }

        public static Value BlockStart = ref new Value(ID:-10000,Op:OpInvalid,Aux:"BlockStart",);

        public static Value BlockEnd = ref new Value(ID:-20000,Op:OpInvalid,Aux:"BlockEnd",);

        // RegisterSet is a bitmap of registers, indexed by Register.num.
        public partial struct RegisterSet // : ulong
        {
        }

        // unexpected is used to indicate an inconsistency or bug in the debug info
        // generation process. These are not fixable by users. At time of writing,
        // changing this to a Fprintf(os.Stderr) and running make.bash generates
        // thousands of warnings.
        private static void unexpected(this ref debugState s, ref Value v, @string msg, params object[] args)
        {
            s.f.Logf("unexpected at " + fmt.Sprint(v.ID) + ":" + msg, args);
        }

        private static void logf(this ref debugState s, @string msg, params object[] args)
        {
            s.f.Logf(msg, args);
        }

        private partial struct debugState
        {
            public bool loggingEnabled;
            public slice<ref LocalSlot> slots;
            public slice<ref LocalSlot> varSlots;
            public ptr<Func> f;
            public ptr<Cache> cache;
            public long numRegisters; // working storage for BuildFuncDebug, reused between blocks.
            public slice<slice<SlotID>> registerContents;
        }

        // getHomeSlot returns the SlotID of the home slot for v, adding to s.slots
        // if necessary.
        private static SlotID getHomeSlot(this ref debugState s, ref Value v)
        {
            LocalSlot home = s.f.getHome(v.ID)._<LocalSlot>();
            foreach (var (id, slot) in s.slots)
            {
                if (slot == home.Value)
                {
                    return SlotID(id);
                }
            } 
            // This slot wasn't in the NamedValue table so it needs to be added.
            s.slots = append(s.slots, ref home);
            return SlotID(len(s.slots) - 1L);
        }

        private static @string BlockString(this ref debugState s, ref BlockDebug b)
        {
            FuncDebug f = ref new FuncDebug(Slots:s.slots,VarSlots:s.varSlots,Registers:s.f.Config.registers,);
            return f.BlockString(b);
        }

        // BuildFuncDebug returns debug information for f.
        // f must be fully processed, so that each Value is where it will be when
        // machine code is emitted.
        public static ref FuncDebug BuildFuncDebug(ref Func f, bool loggingEnabled)
        {
            if (f.RegAlloc == null)
            {
                f.Fatalf("BuildFuncDebug on func %v that has not been fully processed", f);
            }
            debugState state = ref new debugState(loggingEnabled:loggingEnabled,slots:make([]*LocalSlot,len(f.Names)),cache:f.Cache,f:f,numRegisters:len(f.Config.registers),registerContents:make([][]SlotID,len(f.Config.registers)),); 
            // TODO: consider storing this in Cache and reusing across functions.
            var valueNames = make_slice<slice<SlotID>>(f.NumValues());

            {
                var i__prev1 = i;
                var slot__prev1 = slot;

                foreach (var (__i, __slot) in f.Names)
                {
                    i = __i;
                    slot = __slot;
                    var slot = slot;
                    state.slots[i] = ref slot;

                    if (isSynthetic(ref slot))
                    {
                        continue;
                    }
                    foreach (var (_, value) in f.NamedValues[slot])
                    {
                        valueNames[value.ID] = append(valueNames[value.ID], SlotID(i));
                    }
                } 
                // state.varSlots is never changed, and state.slots is only appended to,
                // so aliasing is safe.

                i = i__prev1;
                slot = slot__prev1;
            }

            state.varSlots = state.slots;

            if (state.loggingEnabled)
            {
                slice<@string> names = default;
                {
                    var i__prev1 = i;

                    foreach (var (__i, __name) in f.Names)
                    {
                        i = __i;
                        name = __name;
                        names = append(names, fmt.Sprintf("%d = %s", i, name));
                    }

                    i = i__prev1;
                }

                state.logf("Name table: %v\n", strings.Join(names, ", "));
            } 

            // Build up block states, starting with the first block, then
            // processing blocks once their predecessors have been processed.

            // Location list entries for each block.
            var blockLocs = make_slice<ref BlockDebug>(f.NumBlocks()); 

            // Reverse postorder: visit a block after as many as possible of its
            // predecessors have been visited.
            var po = f.Postorder();
            {
                var i__prev1 = i;

                for (var i = len(po) - 1L; i >= 0L; i--)
                {
                    var b = po[i]; 

                    // Build the starting state for the block from the final
                    // state of its predecessors.
                    var locs = state.mergePredecessors(b, blockLocs);
                    if (state.loggingEnabled)
                    {
                        state.logf("Processing %v, initial locs %v, regs %v\n", b, state.BlockString(locs), state.registerContents);
                    } 
                    // Update locs/registers with the effects of each Value.
                    // The location list generated here needs to be slightly adjusted for use by gdb.
                    // These adjustments are applied in genssa.
                    foreach (var (_, v) in b.Values)
                    {
                        var slots = valueNames[v.ID]; 

                        // Loads and stores inherit the names of their sources.
                        ref Value source = default;

                        if (v.Op == OpStoreReg) 
                            source = v.Args[0L];
                        else if (v.Op == OpLoadReg) 
                            {
                                var a = v.Args[0L];


                                if (a.Op == OpArg) 
                                    source = a;
                                else if (a.Op == OpStoreReg) 
                                    source = a.Args[0L];
                                else 
                                    state.unexpected(v, "load with unexpected source op %v", a);

                            }
                                                if (source != null)
                        {
                            slots = append(slots, valueNames[source.ID]); 
                            // As of writing, the compiler never uses a load/store as a
                            // source of another load/store, so there's no reason this should
                            // ever be consulted. Update just in case, and so that when
                            // valueNames is cached, we can reuse the memory.
                            valueNames[v.ID] = slots;
                        }
                        if (len(slots) == 0L)
                        {
                            continue;
                        }
                        ref Register (reg, _) = f.getHome(v.ID)._<ref Register>();
                        state.processValue(locs, v, slots, reg);
                    } 

                    // The block is done; mark any live locations as ending with the block.
                    foreach (var (_, locList) in locs.Variables)
                    {
                        var last = locList.last();
                        if (last == null || last.End != null)
                        {
                            continue;
                        }
                        last.End = BlockEnd;
                    }
                    if (state.loggingEnabled)
                    {
                        f.Logf("Block done: locs %v, regs %v\n", state.BlockString(locs), state.registerContents);
                    }
                    blockLocs[b.ID] = locs;
                }


                i = i__prev1;
            }

            FuncDebug info = ref new FuncDebug(Slots:state.slots,VarSlots:state.varSlots,Registers:f.Config.registers,); 
            // Consumers want the information in textual order, not by block ID.
            {
                var b__prev1 = b;

                foreach (var (_, __b) in f.Blocks)
                {
                    b = __b;
                    info.Blocks = append(info.Blocks, blockLocs[b.ID]);
                }

                b = b__prev1;
            }

            if (state.loggingEnabled)
            {
                f.Logf("Final result:\n");
                {
                    var slot__prev1 = slot;

                    foreach (var (__slot) in info.VarSlots)
                    {
                        slot = __slot;
                        f.Logf("\t%v => %v\n", info.Slots[slot], info.SlotLocsString(SlotID(slot)));
                    }

                    slot = slot__prev1;
                }

            }
            return info;
        }

        // isSynthetic reports whether if slot represents a compiler-inserted variable,
        // e.g. an autotmp or an anonymous return value that needed a stack slot.
        private static bool isSynthetic(ref LocalSlot slot)
        {
            var c = slot.String()[0L];
            return c == '.' || c == '~';
        }

        // mergePredecessors takes the end state of each of b's predecessors and
        // intersects them to form the starting state for b.
        // The registers slice (the second return value) will be reused for each call to mergePredecessors.
        private static ref BlockDebug mergePredecessors(this ref debugState state, ref Block b, slice<ref BlockDebug> blockLocs)
        {
            var live = make_slice<VarLocList>(len(state.slots)); 

            // Filter out back branches.
            slice<ref Block> preds = default;
            foreach (var (_, pred) in b.Preds)
            {
                if (blockLocs[pred.b.ID] != null)
                {
                    preds = append(preds, pred.b);
                }
            }
            if (len(preds) > 0L)
            {
                var p = preds[0L];
                {
                    var slot__prev1 = slot;
                    var locList__prev1 = locList;

                    foreach (var (__slot, __locList) in blockLocs[p.ID].Variables)
                    {
                        slot = __slot;
                        locList = __locList;
                        var last = locList.last();
                        if (last == null || last.End != BlockEnd)
                        {
                            continue;
                        }
                        var loc = state.cache.NewVarLoc();
                        loc.Start = BlockStart;
                        loc.OnStack = last.OnStack;
                        loc.StackLocation = last.StackLocation;
                        loc.Registers = last.Registers;
                        live[slot].append(loc);
                    }

                    slot = slot__prev1;
                    locList = locList__prev1;
                }

            }
            if (state.loggingEnabled && len(b.Preds) > 1L)
            {
                state.logf("Starting merge with state from %v: %v\n", b.Preds[0L].b, state.BlockString(blockLocs[b.Preds[0L].b.ID]));
            }
            for (long i = 1L; i < len(preds); i++)
            {
                p = preds[i];
                if (state.loggingEnabled)
                {
                    state.logf("Merging in state from %v: %v &= %v\n", p, live, state.BlockString(blockLocs[p.ID]));
                }
                {
                    var slot__prev2 = slot;

                    foreach (var (__slot, __liveVar) in live)
                    {
                        slot = __slot;
                        liveVar = __liveVar;
                        var liveLoc = liveVar.last();
                        if (liveLoc == null)
                        {
                            continue;
                        }
                        var predLoc = blockLocs[p.ID].Variables[SlotID(slot)].last(); 
                        // Clear out slots missing/dead in p.
                        if (predLoc == null || predLoc.End != BlockEnd)
                        {
                            live[slot].Locations = null;
                            continue;
                        } 

                        // Unify storage locations.
                        if (!liveLoc.OnStack || !predLoc.OnStack || liveLoc.StackLocation != predLoc.StackLocation)
                        {
                            liveLoc.OnStack = false;
                            liveLoc.StackLocation = 0L;
                        }
                        liveLoc.Registers &= predLoc.Registers;
                    }

                    slot = slot__prev2;
                }

            } 

            // Create final result.
 

            // Create final result.
            BlockDebug locs = ref new BlockDebug(Variables:live);
            if (state.loggingEnabled)
            {
                locs.Block = b;
            }
            {
                var reg__prev1 = reg;

                foreach (var (__reg) in state.registerContents)
                {
                    reg = __reg;
                    state.registerContents[reg] = state.registerContents[reg][..0L];
                }

                reg = reg__prev1;
            }

            {
                var slot__prev1 = slot;
                var locList__prev1 = locList;

                foreach (var (__slot, __locList) in live)
                {
                    slot = __slot;
                    locList = __locList;
                    loc = locList.last();
                    if (loc == null)
                    {
                        continue;
                    }
                    {
                        var reg__prev2 = reg;

                        for (long reg = 0L; reg < state.numRegisters; reg++)
                        {
                            if (loc.Registers & (1L << (int)(uint8(reg))) != 0L)
                            {
                                state.registerContents[reg] = append(state.registerContents[reg], SlotID(slot));
                            }
                        }


                        reg = reg__prev2;
                    }
                }

                slot = slot__prev1;
                locList = locList__prev1;
            }

            return locs;
        }

        // processValue updates locs and state.registerContents to reflect v, a value with
        // the names in vSlots and homed in vReg.  "v" becomes visible after execution of
        // the instructions evaluating it.
        private static void processValue(this ref debugState state, ref BlockDebug locs, ref Value v, slice<SlotID> vSlots, ref Register vReg)
        {

            if (v.Op == OpRegKill) 
                if (state.loggingEnabled)
                {
                    var existingSlots = make_slice<bool>(len(state.slots));
                    {
                        var slot__prev1 = slot;

                        foreach (var (_, __slot) in state.registerContents[vReg.num])
                        {
                            slot = __slot;
                            existingSlots[slot] = true;
                        }

                        slot = slot__prev1;
                    }

                    {
                        var slot__prev1 = slot;

                        foreach (var (_, __slot) in vSlots)
                        {
                            slot = __slot;
                            if (existingSlots[slot])
                            {
                                existingSlots[slot] = false;
                            }
                            else
                            {
                                state.unexpected(v, "regkill of unassociated name %v\n", state.slots[slot]);
                            }
                        }

                        slot = slot__prev1;
                    }

                    {
                        var slot__prev1 = slot;

                        foreach (var (__slot, __live) in existingSlots)
                        {
                            slot = __slot;
                            live = __live;
                            if (live)
                            {
                                state.unexpected(v, "leftover register name: %v\n", state.slots[slot]);
                            }
                        }

                        slot = slot__prev1;
                    }

                }
                state.registerContents[vReg.num] = null;

                {
                    var slot__prev1 = slot;

                    foreach (var (_, __slot) in vSlots)
                    {
                        slot = __slot;
                        var last = locs.lastLoc(slot);
                        if (last == null)
                        {
                            state.unexpected(v, "regkill of already dead %s, %+v\n", vReg, state.slots[slot]);
                            continue;
                        }
                        if (state.loggingEnabled)
                        {
                            state.logf("at %v: %v regkilled out of %s\n", v.ID, state.slots[slot], vReg);
                        }
                        if (last.End != null)
                        {
                            state.unexpected(v, "regkill of dead slot, died at %v\n", last.End);
                        }
                        last.End = v;

                        var regs = last.Registers & ~(1L << (int)(uint8(vReg.num)));
                        if (!last.OnStack && regs == 0L)
                        {
                            continue;
                        }
                        var loc = state.cache.NewVarLoc();
                        loc.Start = v;
                        loc.OnStack = last.OnStack;
                        loc.StackLocation = last.StackLocation;
                        loc.Registers = regs;
                        locs.append(slot, loc);
                    }

                    slot = slot__prev1;
                }
            else if (v.Op == OpArg) 
                {
                    var slot__prev1 = slot;

                    foreach (var (_, __slot) in vSlots)
                    {
                        slot = __slot;
                        {
                            var last__prev1 = last;

                            last = locs.lastLoc(slot);

                            if (last != null)
                            {
                                state.unexpected(v, "Arg op on already-live slot %v", state.slots[slot]);
                                last.End = v;
                            }

                            last = last__prev1;

                        }
                        loc = state.cache.NewVarLoc();
                        loc.Start = v;
                        loc.OnStack = true;
                        loc.StackLocation = state.getHomeSlot(v);
                        locs.append(slot, loc);
                        if (state.loggingEnabled)
                        {
                            state.logf("at %v: arg %v now on stack in location %v\n", v.ID, state.slots[slot], state.slots[loc.StackLocation]);
                        }
                    }

                    slot = slot__prev1;
                }
            else if (v.Op == OpStoreReg) 
                {
                    var slot__prev1 = slot;

                    foreach (var (_, __slot) in vSlots)
                    {
                        slot = __slot;
                        last = locs.lastLoc(slot);
                        if (last == null)
                        {
                            state.unexpected(v, "spill of unnamed register %s\n", vReg);
                            break;
                        }
                        last.End = v;
                        loc = state.cache.NewVarLoc();
                        loc.Start = v;
                        loc.OnStack = true;
                        loc.StackLocation = state.getHomeSlot(v);
                        loc.Registers = last.Registers;
                        locs.append(slot, loc);
                        if (state.loggingEnabled)
                        {
                            state.logf("at %v: %v spilled to stack location %v\n", v.ID, state.slots[slot], state.slots[loc.StackLocation]);
                        }
                    }

                    slot = slot__prev1;
                }
            else if (vReg != null) 
                if (state.loggingEnabled)
                {
                    var newSlots = make_slice<bool>(len(state.slots));
                    {
                        var slot__prev1 = slot;

                        foreach (var (_, __slot) in vSlots)
                        {
                            slot = __slot;
                            newSlots[slot] = true;
                        }

                        slot = slot__prev1;
                    }

                    {
                        var slot__prev1 = slot;

                        foreach (var (_, __slot) in state.registerContents[vReg.num])
                        {
                            slot = __slot;
                            if (!newSlots[slot])
                            {
                                state.unexpected(v, "%v clobbered\n", state.slots[slot]);
                            }
                        }

                        slot = slot__prev1;
                    }

                }
                {
                    var slot__prev1 = slot;

                    foreach (var (_, __slot) in vSlots)
                    {
                        slot = __slot;
                        if (state.loggingEnabled)
                        {
                            state.logf("at %v: %v now in %s\n", v.ID, state.slots[slot], vReg);
                        }
                        last = locs.lastLoc(slot);
                        if (last != null && last.End == null)
                        {
                            last.End = v;
                        }
                        state.registerContents[vReg.num] = append(state.registerContents[vReg.num], slot);
                        loc = state.cache.NewVarLoc();
                        loc.Start = v;
                        if (last != null)
                        {
                            loc.OnStack = last.OnStack;
                            loc.StackLocation = last.StackLocation;
                            loc.Registers = last.Registers;
                        }
                        loc.Registers |= 1L << (int)(uint8(vReg.num));
                        locs.append(slot, loc);
                    }

                    slot = slot__prev1;
                }
            else 
                state.unexpected(v, "named value with no reg\n");
                    }
    }
}}}}
