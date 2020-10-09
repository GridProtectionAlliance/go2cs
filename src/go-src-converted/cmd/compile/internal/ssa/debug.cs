// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// package ssa -- go2cs converted at 2020 October 09 05:24:36 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\debug.go
using dwarf = go.cmd.@internal.dwarf_package;
using obj = go.cmd.@internal.obj_package;
using hex = go.encoding.hex_package;
using fmt = go.fmt_package;
using bits = go.math.bits_package;
using sort = go.sort_package;
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
        public partial struct VarID // : int
        {
        }

        // A FuncDebug contains all the debug information for the variables in a
        // function. Variables are identified by their LocalSlot, which may be the
        // result of decomposing a larger variable.
        public partial struct FuncDebug
        {
            public slice<LocalSlot> Slots; // The user variables, indexed by VarID.
            public slice<GCNode> Vars; // The slots that make up each variable, indexed by VarID.
            public slice<slice<SlotID>> VarSlots; // The location list data, indexed by VarID. Must be processed by PutLocationList.
            public slice<slice<byte>> LocationLists; // Filled in by the user. Translates Block and Value ID to PC.
            public Func<ID, ID, long> GetPC;
        }

        public partial struct BlockDebug
        {
            public bool relevant; // State at the end of the block if it's fully processed. Immutable once initialized.
            public slice<liveSlot> endState;
        }

        // A liveSlot is a slot that's live in loc at entry/exit of a block.
        private partial struct liveSlot
        {
            public RegisterSet Registers;
            public ref StackOffset StackOffset => ref StackOffset_val;
            public SlotID slot;
        }

        private static bool absent(this liveSlot loc)
        {
            return loc.Registers == 0L && !loc.onStack();
        }

        // StackOffset encodes whether a value is on the stack and if so, where. It is
        // a 31-bit integer followed by a presence flag at the low-order bit.
        public partial struct StackOffset // : int
        {
        }

        public static bool onStack(this StackOffset s)
        {
            return s != 0L;
        }

        public static int stackOffsetValue(this StackOffset s)
        {
            return int32(s) >> (int)(1L);
        }

        // stateAtPC is the current state of all variables at some point.
        private partial struct stateAtPC
        {
            public slice<VarLoc> slots; // The slots present in each register, indexed by register number.
            public slice<slice<SlotID>> registers;
        }

        // reset fills state with the live variables from live.
        private static void reset(this ptr<stateAtPC> _addr_state, slice<liveSlot> live)
        {
            ref stateAtPC state = ref _addr_state.val;

            var slots = state.slots;
            var registers = state.registers;
            {
                var i__prev1 = i;

                foreach (var (__i) in slots)
                {
                    i = __i;
                    slots[i] = new VarLoc();
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in registers)
                {
                    i = __i;
                    registers[i] = registers[i][..0L];
                }

                i = i__prev1;
            }

            foreach (var (_, live) in live)
            {
                slots[live.slot] = new VarLoc(live.Registers,live.StackOffset);
                if (live.Registers == 0L)
                {
                    continue;
                }

                var mask = uint64(live.Registers);
                while (true)
                {
                    if (mask == 0L)
                    {
                        break;
                    }

                    var reg = uint8(bits.TrailingZeros64(mask));
                    mask &= 1L << (int)(reg);

                    registers[reg] = append(registers[reg], live.slot);

                }


            }
            state.slots = slots;
            state.registers = registers;

        }

        private static @string LocString(this ptr<debugState> _addr_s, VarLoc loc)
        {
            ref debugState s = ref _addr_s.val;

            if (loc.absent())
            {
                return "<nil>";
            }

            slice<@string> storage = default;
            if (loc.onStack())
            {
                storage = append(storage, "stack");
            }

            var mask = uint64(loc.Registers);
            while (true)
            {
                if (mask == 0L)
                {
                    break;
                }

                var reg = uint8(bits.TrailingZeros64(mask));
                mask &= 1L << (int)(reg);

                storage = append(storage, s.registers[reg].String());

            }

            return strings.Join(storage, ",");

        }

        // A VarLoc describes the storage for part of a user variable.
        public partial struct VarLoc
        {
            public RegisterSet Registers;
            public ref StackOffset StackOffset => ref StackOffset_val;
        }

        public static bool absent(this VarLoc loc)
        {
            return loc.Registers == 0L && !loc.onStack();
        }

        public static ptr<Value> BlockStart = addr(new Value(ID:-10000,Op:OpInvalid,Aux:"BlockStart",));

        public static ptr<Value> BlockEnd = addr(new Value(ID:-20000,Op:OpInvalid,Aux:"BlockEnd",));

        // RegisterSet is a bitmap of registers, indexed by Register.num.
        public partial struct RegisterSet // : ulong
        {
        }

        // logf prints debug-specific logging to stdout (always stdout) if the current
        // function is tagged by GOSSAFUNC (for ssa output directed either to stdout or html).
        private static void logf(this ptr<debugState> _addr_s, @string msg, params object[] args)
        {
            args = args.Clone();
            ref debugState s = ref _addr_s.val;

            if (s.f.PrintOrHtmlSSA)
            {
                fmt.Printf(msg, args);
            }

        }

        private partial struct debugState
        {
            public slice<LocalSlot> slots;
            public slice<GCNode> vars;
            public slice<slice<SlotID>> varSlots;
            public slice<slice<byte>> lists; // The user variable that each slot rolls up to, indexed by SlotID.
            public slice<VarID> slotVars;
            public ptr<Func> f;
            public bool loggingEnabled;
            public slice<Register> registers;
            public Func<LocalSlot, int> stackOffset;
            public ptr<obj.Link> ctxt; // The names (slots) associated with each value, indexed by Value ID.
            public slice<slice<SlotID>> valueNames; // The current state of whatever analysis is running.
            public stateAtPC currentState;
            public slice<long> liveCount;
            public ptr<sparseSet> changedVars; // The pending location list entry for each user variable, indexed by VarID.
            public slice<pendingEntry> pendingEntries;
            public map<GCNode, slice<SlotID>> varParts;
            public slice<BlockDebug> blockDebug;
            public slice<VarLoc> pendingSlotLocs;
            public slice<liveSlot> liveSlots;
            public long liveSlotSliceBegin;
            public sort.Interface partsByVarOffset;
        }

        private static void initializeCache(this ptr<debugState> _addr_state, ptr<Func> _addr_f, long numVars, long numSlots)
        {
            ref debugState state = ref _addr_state.val;
            ref Func f = ref _addr_f.val;
 
            // One blockDebug per block. Initialized in allocBlock.
            if (cap(state.blockDebug) < f.NumBlocks())
            {
                state.blockDebug = make_slice<BlockDebug>(f.NumBlocks());
            }
            else
            { 
                // This local variable, and the ones like it below, enable compiler
                // optimizations. Don't inline them.
                var b = state.blockDebug[..f.NumBlocks()];
                {
                    var i__prev1 = i;

                    foreach (var (__i) in b)
                    {
                        i = __i;
                        b[i] = new BlockDebug();
                    }

                    i = i__prev1;
                }
            } 

            // A list of slots per Value. Reuse the previous child slices.
            if (cap(state.valueNames) < f.NumValues())
            {
                var old = state.valueNames;
                state.valueNames = make_slice<slice<SlotID>>(f.NumValues());
                copy(state.valueNames, old);
            }

            var vn = state.valueNames[..f.NumValues()];
            {
                var i__prev1 = i;

                foreach (var (__i) in vn)
                {
                    i = __i;
                    vn[i] = vn[i][..0L];
                } 

                // Slot and register contents for currentState. Cleared by reset().

                i = i__prev1;
            }

            if (cap(state.currentState.slots) < numSlots)
            {
                state.currentState.slots = make_slice<VarLoc>(numSlots);
            }
            else
            {
                state.currentState.slots = state.currentState.slots[..numSlots];
            }

            if (cap(state.currentState.registers) < len(state.registers))
            {
                state.currentState.registers = make_slice<slice<SlotID>>(len(state.registers));
            }
            else
            {
                state.currentState.registers = state.currentState.registers[..len(state.registers)];
            } 

            // Used many times by mergePredecessors.
            if (cap(state.liveCount) < numSlots)
            {
                state.liveCount = make_slice<long>(numSlots);
            }
            else
            {
                state.liveCount = state.liveCount[..numSlots];
            } 

            // A relatively small slice, but used many times as the return from processValue.
            state.changedVars = newSparseSet(numVars); 

            // A pending entry per user variable, with space to track each of its pieces.
            long numPieces = 0L;
            {
                var i__prev1 = i;

                foreach (var (__i) in state.varSlots)
                {
                    i = __i;
                    numPieces += len(state.varSlots[i]);
                }

                i = i__prev1;
            }

            if (cap(state.pendingSlotLocs) < numPieces)
            {
                state.pendingSlotLocs = make_slice<VarLoc>(numPieces);
            }
            else
            {
                var psl = state.pendingSlotLocs[..numPieces];
                {
                    var i__prev1 = i;

                    foreach (var (__i) in psl)
                    {
                        i = __i;
                        psl[i] = new VarLoc();
                    }

                    i = i__prev1;
                }
            }

            if (cap(state.pendingEntries) < numVars)
            {
                state.pendingEntries = make_slice<pendingEntry>(numVars);
            }

            var pe = state.pendingEntries[..numVars];
            long freePieceIdx = 0L;
            foreach (var (varID, slots) in state.varSlots)
            {
                pe[varID] = new pendingEntry(pieces:state.pendingSlotLocs[freePieceIdx:freePieceIdx+len(slots)],);
                freePieceIdx += len(slots);
            }
            state.pendingEntries = pe;

            if (cap(state.lists) < numVars)
            {
                state.lists = make_slice<slice<byte>>(numVars);
            }
            else
            {
                state.lists = state.lists[..numVars];
                {
                    var i__prev1 = i;

                    foreach (var (__i) in state.lists)
                    {
                        i = __i;
                        state.lists[i] = null;
                    }

                    i = i__prev1;
                }
            }

            state.liveSlots = state.liveSlots[..0L];
            state.liveSlotSliceBegin = 0L;

        }

        private static ptr<BlockDebug> allocBlock(this ptr<debugState> _addr_state, ptr<Block> _addr_b)
        {
            ref debugState state = ref _addr_state.val;
            ref Block b = ref _addr_b.val;

            return _addr__addr_state.blockDebug[b.ID]!;
        }

        private static void appendLiveSlot(this ptr<debugState> _addr_state, liveSlot ls)
        {
            ref debugState state = ref _addr_state.val;

            state.liveSlots = append(state.liveSlots, ls);
        }

        private static slice<liveSlot> getLiveSlotSlice(this ptr<debugState> _addr_state)
        {
            ref debugState state = ref _addr_state.val;

            var s = state.liveSlots[state.liveSlotSliceBegin..];
            state.liveSlotSliceBegin = len(state.liveSlots);
            return s;
        }

        private static @string blockEndStateString(this ptr<debugState> _addr_s, ptr<BlockDebug> _addr_b)
        {
            ref debugState s = ref _addr_s.val;
            ref BlockDebug b = ref _addr_b.val;

            stateAtPC endState = new stateAtPC(slots:make([]VarLoc,len(s.slots)),registers:make([][]SlotID,len(s.registers)));
            endState.reset(b.endState);
            return s.stateString(endState);
        }

        private static @string stateString(this ptr<debugState> _addr_s, stateAtPC state)
        {
            ref debugState s = ref _addr_s.val;

            slice<@string> strs = default;
            foreach (var (slotID, loc) in state.slots)
            {
                if (!loc.absent())
                {
                    strs = append(strs, fmt.Sprintf("\t%v = %v\n", s.slots[slotID], s.LocString(loc)));
                }

            }
            strs = append(strs, "\n");
            foreach (var (reg, slots) in state.registers)
            {
                if (len(slots) != 0L)
                {
                    slice<@string> slotStrs = default;
                    foreach (var (_, slot) in slots)
                    {
                        slotStrs = append(slotStrs, s.slots[slot].String());
                    }
                    strs = append(strs, fmt.Sprintf("\t%v = %v\n", _addr_s.registers[reg], slotStrs));

                }

            }
            if (len(strs) == 1L)
            {
                return "(no vars)\n";
            }

            return strings.Join(strs, "");

        }

        // BuildFuncDebug returns debug information for f.
        // f must be fully processed, so that each Value is where it will be when
        // machine code is emitted.
        public static ptr<FuncDebug> BuildFuncDebug(ptr<obj.Link> _addr_ctxt, ptr<Func> _addr_f, bool loggingEnabled, Func<LocalSlot, int> stackOffset)
        {
            ref obj.Link ctxt = ref _addr_ctxt.val;
            ref Func f = ref _addr_f.val;

            if (f.RegAlloc == null)
            {
                f.Fatalf("BuildFuncDebug on func %v that has not been fully processed", f);
            }

            var state = _addr_f.Cache.debugState;
            state.loggingEnabled = loggingEnabled;
            state.f = f;
            state.registers = f.Config.registers;
            state.stackOffset = stackOffset;
            state.ctxt = ctxt;

            if (state.loggingEnabled)
            {
                state.logf("Generating location lists for function %q\n", f.Name);
            }

            if (state.varParts == null)
            {
                state.varParts = make_map<GCNode, slice<SlotID>>();
            }
            else
            {
                {
                    var n__prev1 = n;

                    foreach (var (__n) in state.varParts)
                    {
                        n = __n;
                        delete(state.varParts, n);
                    }

                    n = n__prev1;
                }
            } 

            // Recompose any decomposed variables, and establish the canonical
            // IDs for each var and slot by filling out state.vars and state.slots.
            state.slots = state.slots[..0L];
            state.vars = state.vars[..0L];
            {
                var i__prev1 = i;
                var slot__prev1 = slot;

                foreach (var (__i, __slot) in f.Names)
                {
                    i = __i;
                    slot = __slot;
                    state.slots = append(state.slots, slot);
                    if (slot.N.IsSynthetic())
                    {
                        continue;
                    }

                    var topSlot = _addr_slot;
                    while (topSlot.SplitOf != null)
                    {
                        topSlot = topSlot.SplitOf;
                    }

                    {
                        var (_, ok) = state.varParts[topSlot.N];

                        if (!ok)
                        {
                            state.vars = append(state.vars, topSlot.N);
                        }

                    }

                    state.varParts[topSlot.N] = append(state.varParts[topSlot.N], SlotID(i));

                } 

                // Recreate the LocalSlot for each stack-only variable.
                // This would probably be better as an output from stackframe.

                i = i__prev1;
                slot = slot__prev1;
            }

            foreach (var (_, b) in f.Blocks)
            {
                foreach (var (_, v) in b.Values)
                {
                    if (v.Op == OpVarDef || v.Op == OpVarKill)
                    {
                        GCNode n = v.Aux._<GCNode>();
                        if (n.IsSynthetic())
                        {
                            continue;
                        }

                        {
                            (_, ok) = state.varParts[n];

                            if (!ok)
                            {
                                ref LocalSlot slot = ref heap(new LocalSlot(N:n,Type:v.Type,Off:0), out ptr<LocalSlot> _addr_slot);
                                state.slots = append(state.slots, slot);
                                state.varParts[n] = new slice<SlotID>(new SlotID[] { SlotID(len(state.slots)-1) });
                                state.vars = append(state.vars, n);
                            }

                        }

                    }

                }

            } 

            // Fill in the var<->slot mappings.
            if (cap(state.varSlots) < len(state.vars))
            {
                state.varSlots = make_slice<slice<SlotID>>(len(state.vars));
            }
            else
            {
                state.varSlots = state.varSlots[..len(state.vars)];
                {
                    var i__prev1 = i;

                    foreach (var (__i) in state.varSlots)
                    {
                        i = __i;
                        state.varSlots[i] = state.varSlots[i][..0L];
                    }

                    i = i__prev1;
                }
            }

            if (cap(state.slotVars) < len(state.slots))
            {
                state.slotVars = make_slice<VarID>(len(state.slots));
            }
            else
            {
                state.slotVars = state.slotVars[..len(state.slots)];
            }

            if (state.partsByVarOffset == null)
            {
                state.partsByVarOffset = addr(new partsByVarOffset());
            }

            {
                var n__prev1 = n;

                foreach (var (__varID, __n) in state.vars)
                {
                    varID = __varID;
                    n = __n;
                    var parts = state.varParts[n];
                    state.varSlots[varID] = parts;
                    foreach (var (_, slotID) in parts)
                    {
                        state.slotVars[slotID] = VarID(varID);
                    }
                    state.partsByVarOffset._<ptr<partsByVarOffset>>().val = new partsByVarOffset(parts,state.slots);
                    sort.Sort(state.partsByVarOffset);

                }

                n = n__prev1;
            }

            state.initializeCache(f, len(state.varParts), len(state.slots));

            {
                var i__prev1 = i;
                var slot__prev1 = slot;

                foreach (var (__i, __slot) in f.Names)
                {
                    i = __i;
                    slot = __slot;
                    if (slot.N.IsSynthetic())
                    {
                        continue;
                    }

                    foreach (var (_, value) in f.NamedValues[slot])
                    {
                        state.valueNames[value.ID] = append(state.valueNames[value.ID], SlotID(i));
                    }

                }

                i = i__prev1;
                slot = slot__prev1;
            }

            var blockLocs = state.liveness();
            state.buildLocationLists(blockLocs);

            return addr(new FuncDebug(Slots:state.slots,VarSlots:state.varSlots,Vars:state.vars,LocationLists:state.lists,));

        }

        // liveness walks the function in control flow order, calculating the start
        // and end state of each block.
        private static slice<ptr<BlockDebug>> liveness(this ptr<debugState> _addr_state)
        {
            ref debugState state = ref _addr_state.val;

            var blockLocs = make_slice<ptr<BlockDebug>>(state.f.NumBlocks()); 

            // Reverse postorder: visit a block after as many as possible of its
            // predecessors have been visited.
            var po = state.f.Postorder();
            for (var i = len(po) - 1L; i >= 0L; i--)
            {
                var b = po[i]; 

                // Build the starting state for the block from the final
                // state of its predecessors.
                var (startState, startValid) = state.mergePredecessors(b, blockLocs, null);
                var changed = false;
                if (state.loggingEnabled)
                {
                    state.logf("Processing %v, initial state:\n%v", b, state.stateString(state.currentState));
                } 

                // Update locs/registers with the effects of each Value.
                foreach (var (_, v) in b.Values)
                {
                    var slots = state.valueNames[v.ID]; 

                    // Loads and stores inherit the names of their sources.
                    ptr<Value> source;

                    if (v.Op == OpStoreReg) 
                        source = v.Args[0L];
                    else if (v.Op == OpLoadReg) 
                        {
                            var a = v.Args[0L];


                            if (a.Op == OpArg || a.Op == OpPhi) 
                                source = a;
                            else if (a.Op == OpStoreReg) 
                                source = a.Args[0L];
                            else 
                                if (state.loggingEnabled)
                                {
                                    state.logf("at %v: load with unexpected source op: %v (%v)\n", v, a.Op, a);
                                }


                        }
                    // Update valueNames with the source so that later steps
                    // don't need special handling.
                    if (source != null)
                    {
                        slots = append(slots, state.valueNames[source.ID]);
                        state.valueNames[v.ID] = slots;
                    }

                    ptr<Register> (reg, _) = state.f.getHome(v.ID)._<ptr<Register>>();
                    var c = state.processValue(v, slots, reg);
                    changed = changed || c;

                }
                if (state.loggingEnabled)
                {
                    state.f.Logf("Block %v done, locs:\n%v", b, state.stateString(state.currentState));
                }

                var locs = state.allocBlock(b);
                locs.relevant = changed;
                if (!changed && startValid)
                {
                    locs.endState = startState;
                }
                else
                {
                    foreach (var (slotID, slotLoc) in state.currentState.slots)
                    {
                        if (slotLoc.absent())
                        {
                            continue;
                        }

                        state.appendLiveSlot(new liveSlot(slot:SlotID(slotID),Registers:slotLoc.Registers,StackOffset:slotLoc.StackOffset));

                    }
                    locs.endState = state.getLiveSlotSlice();

                }

                blockLocs[b.ID] = locs;

            }

            return blockLocs;

        }

        // mergePredecessors takes the end state of each of b's predecessors and
        // intersects them to form the starting state for b. It puts that state in
        // blockLocs, and fills state.currentState with it. If convenient, it returns
        // a reused []liveSlot, true that represents the starting state.
        // If previousBlock is non-nil, it registers changes vs. that block's end
        // state in state.changedVars. Note that previousBlock will often not be a
        // predecessor.
        private static (slice<liveSlot>, bool) mergePredecessors(this ptr<debugState> _addr_state, ptr<Block> _addr_b, slice<ptr<BlockDebug>> blockLocs, ptr<Block> _addr_previousBlock)
        {
            slice<liveSlot> _p0 = default;
            bool _p0 = default;
            ref debugState state = ref _addr_state.val;
            ref Block b = ref _addr_b.val;
            ref Block previousBlock = ref _addr_previousBlock.val;
 
            // Filter out back branches.
            array<ptr<Block>> predsBuf = new array<ptr<Block>>(10L);
            var preds = predsBuf[..0L];
            {
                var pred__prev1 = pred;

                foreach (var (_, __pred) in b.Preds)
                {
                    pred = __pred;
                    if (blockLocs[pred.b.ID] != null)
                    {
                        preds = append(preds, pred.b);
                    }

                }

                pred = pred__prev1;
            }

            if (state.loggingEnabled)
            { 
                // The logf below would cause preds to be heap-allocated if
                // it were passed directly.
                var preds2 = make_slice<ptr<Block>>(len(preds));
                copy(preds2, preds);
                state.logf("Merging %v into %v\n", preds2, b);

            } 

            // TODO all the calls to this are overkill; only need to do this for slots that are not present in the merge.
            Action<slice<liveSlot>> markChangedVars = slots =>
            {
                foreach (var (_, live) in slots)
                {
                    state.changedVars.add(ID(state.slotVars[live.slot]));
                }

            }
;

            if (len(preds) == 0L)
            {
                if (previousBlock != null)
                { 
                    // Mark everything in previous block as changed because it is not a predecessor.
                    markChangedVars(blockLocs[previousBlock.ID].endState);

                }

                state.currentState.reset(null);
                return (null, true);

            }

            var p0 = blockLocs[preds[0L].ID].endState;
            if (len(preds) == 1L)
            {
                if (previousBlock != null && preds[0L].ID != previousBlock.ID)
                { 
                    // Mark everything in previous block as changed because it is not a predecessor.
                    markChangedVars(blockLocs[previousBlock.ID].endState);

                }

                state.currentState.reset(p0);
                return (p0, true);

            }

            var baseID = preds[0L].ID;
            var baseState = p0; 

            // If previous block is not a predecessor, its location information changes at boundary with this block.
            var previousBlockIsNotPredecessor = previousBlock != null; // If it's nil, no info to change.

            if (previousBlock != null)
            { 
                // Try to use previousBlock as the base state
                // if possible.
                {
                    var pred__prev1 = pred;

                    foreach (var (_, __pred) in preds[1L..])
                    {
                        pred = __pred;
                        if (pred.ID == previousBlock.ID)
                        {
                            baseID = pred.ID;
                            baseState = blockLocs[pred.ID].endState;
                            previousBlockIsNotPredecessor = false;
                            break;
                        }

                    }

                    pred = pred__prev1;
                }
            }

            if (state.loggingEnabled)
            {
                state.logf("Starting %v with state from b%v:\n%v", b, baseID, state.blockEndStateString(blockLocs[baseID]));
            }

            var slotLocs = state.currentState.slots;
            {
                var predSlot__prev1 = predSlot;

                foreach (var (_, __predSlot) in baseState)
                {
                    predSlot = __predSlot;
                    slotLocs[predSlot.slot] = new VarLoc(predSlot.Registers,predSlot.StackOffset);
                    state.liveCount[predSlot.slot] = 1L;
                }

                predSlot = predSlot__prev1;
            }

            {
                var pred__prev1 = pred;

                foreach (var (_, __pred) in preds)
                {
                    pred = __pred;
                    if (pred.ID == baseID)
                    {
                        continue;
                    }

                    if (state.loggingEnabled)
                    {
                        state.logf("Merging in state from %v:\n%v", pred, state.blockEndStateString(blockLocs[pred.ID]));
                    }

                    {
                        var predSlot__prev2 = predSlot;

                        foreach (var (_, __predSlot) in blockLocs[pred.ID].endState)
                        {
                            predSlot = __predSlot;
                            state.liveCount[predSlot.slot]++;
                            var liveLoc = slotLocs[predSlot.slot];
                            if (!liveLoc.onStack() || !predSlot.onStack() || liveLoc.StackOffset != predSlot.StackOffset)
                            {
                                liveLoc.StackOffset = 0L;
                            }

                            liveLoc.Registers &= predSlot.Registers;
                            slotLocs[predSlot.slot] = liveLoc;

                        }

                        predSlot = predSlot__prev2;
                    }
                } 

                // Check if the final state is the same as the first predecessor's
                // final state, and reuse it if so. In principle it could match any,
                // but it's probably not worth checking more than the first.

                pred = pred__prev1;
            }

            var unchanged = true;
            {
                var predSlot__prev1 = predSlot;

                foreach (var (_, __predSlot) in baseState)
                {
                    predSlot = __predSlot;
                    if (state.liveCount[predSlot.slot] != len(preds) || slotLocs[predSlot.slot].Registers != predSlot.Registers || slotLocs[predSlot.slot].StackOffset != predSlot.StackOffset)
                    {
                        unchanged = false;
                        break;
                    }

                }

                predSlot = predSlot__prev1;
            }

            if (unchanged)
            {
                if (state.loggingEnabled)
                {
                    state.logf("After merge, %v matches b%v exactly.\n", b, baseID);
                }

                if (previousBlockIsNotPredecessor)
                { 
                    // Mark everything in previous block as changed because it is not a predecessor.
                    markChangedVars(blockLocs[previousBlock.ID].endState);

                }

                state.currentState.reset(baseState);
                return (baseState, true);

            }

            {
                var reg__prev1 = reg;

                foreach (var (__reg) in state.currentState.registers)
                {
                    reg = __reg;
                    state.currentState.registers[reg] = state.currentState.registers[reg][..0L];
                } 

                // A slot is live if it was seen in all predecessors, and they all had
                // some storage in common.

                reg = reg__prev1;
            }

            {
                var predSlot__prev1 = predSlot;

                foreach (var (_, __predSlot) in baseState)
                {
                    predSlot = __predSlot;
                    var slotLoc = slotLocs[predSlot.slot];

                    if (state.liveCount[predSlot.slot] != len(preds))
                    { 
                        // Seen in only some predecessors. Clear it out.
                        slotLocs[predSlot.slot] = new VarLoc();
                        continue;

                    } 

                    // Present in all predecessors.
                    var mask = uint64(slotLoc.Registers);
                    while (true)
                    {
                        if (mask == 0L)
                        {
                            break;
                        }

                        var reg = uint8(bits.TrailingZeros64(mask));
                        mask &= 1L << (int)(reg);
                        state.currentState.registers[reg] = append(state.currentState.registers[reg], predSlot.slot);

                    }


                }

                predSlot = predSlot__prev1;
            }

            if (previousBlockIsNotPredecessor)
            { 
                // Mark everything in previous block as changed because it is not a predecessor.
                markChangedVars(blockLocs[previousBlock.ID].endState);


            }

            return (null, false);

        }

        // processValue updates locs and state.registerContents to reflect v, a value with
        // the names in vSlots and homed in vReg.  "v" becomes visible after execution of
        // the instructions evaluating it. It returns which VarIDs were modified by the
        // Value's execution.
        private static bool processValue(this ptr<debugState> _addr_state, ptr<Value> _addr_v, slice<SlotID> vSlots, ptr<Register> _addr_vReg)
        {
            ref debugState state = ref _addr_state.val;
            ref Value v = ref _addr_v.val;
            ref Register vReg = ref _addr_vReg.val;

            var locs = state.currentState;
            var changed = false;
            Action<SlotID, VarLoc> setSlot = (slot, loc) =>
            {
                changed = true;
                state.changedVars.add(ID(state.slotVars[slot]));
                state.currentState.slots[slot] = loc;
            } 

            // Handle any register clobbering. Call operations, for example,
            // clobber all registers even though they don't explicitly write to
            // them.
; 

            // Handle any register clobbering. Call operations, for example,
            // clobber all registers even though they don't explicitly write to
            // them.
            var clobbers = uint64(opcodeTable[v.Op].reg.clobbers);
            while (true)
            {
                if (clobbers == 0L)
                {
                    break;
                }

                var reg = uint8(bits.TrailingZeros64(clobbers));
                clobbers &= 1L << (int)(reg);

                {
                    var slot__prev2 = slot;

                    foreach (var (_, __slot) in locs.registers[reg])
                    {
                        slot = __slot;
                        if (state.loggingEnabled)
                        {
                            state.logf("at %v: %v clobbered out of %v\n", v, state.slots[slot], _addr_state.registers[reg]);
                        }

                        var last = locs.slots[slot];
                        if (last.absent())
                        {
                            state.f.Fatalf("at %v: slot %v in register %v with no location entry", v, state.slots[slot], _addr_state.registers[reg]);
                            continue;
                        }

                        var regs = last.Registers & ~(1L << (int)(reg));
                        setSlot(slot, new VarLoc(regs,last.StackOffset));

                    }

                    slot = slot__prev2;
                }

                locs.registers[reg] = locs.registers[reg][..0L];

            }



            if (v.Op == OpVarDef || v.Op == OpVarKill) 
                GCNode n = v.Aux._<GCNode>();
                if (n.IsSynthetic())
                {
                    break;
                }

                var slotID = state.varParts[n][0L];
                StackOffset stackOffset = default;
                if (v.Op == OpVarDef)
                {
                    stackOffset = StackOffset(state.stackOffset(state.slots[slotID]) << (int)(1L) | 1L);
                }

                setSlot(slotID, new VarLoc(0,stackOffset));
                if (state.loggingEnabled)
                {
                    if (v.Op == OpVarDef)
                    {
                        state.logf("at %v: stack-only var %v now live\n", v, state.slots[slotID]);
                    }
                    else
                    {
                        state.logf("at %v: stack-only var %v now dead\n", v, state.slots[slotID]);
                    }

                }

            else if (v.Op == OpArg) 
                LocalSlot home = state.f.getHome(v.ID)._<LocalSlot>();
                stackOffset = state.stackOffset(home) << (int)(1L) | 1L;
                {
                    var slot__prev1 = slot;

                    foreach (var (_, __slot) in vSlots)
                    {
                        slot = __slot;
                        if (state.loggingEnabled)
                        {
                            state.logf("at %v: arg %v now on stack in location %v\n", v, state.slots[slot], home);
                            {
                                var last__prev2 = last;

                                last = locs.slots[slot];

                                if (!last.absent())
                                {
                                    state.logf("at %v: unexpected arg op on already-live slot %v\n", v, state.slots[slot]);
                                }

                                last = last__prev2;

                            }

                        }

                        setSlot(slot, new VarLoc(0,StackOffset(stackOffset)));

                    }

                    slot = slot__prev1;
                }
            else if (v.Op == OpStoreReg) 
                home = state.f.getHome(v.ID)._<LocalSlot>();
                stackOffset = state.stackOffset(home) << (int)(1L) | 1L;
                {
                    var slot__prev1 = slot;

                    foreach (var (_, __slot) in vSlots)
                    {
                        slot = __slot;
                        last = locs.slots[slot];
                        if (last.absent())
                        {
                            if (state.loggingEnabled)
                            {
                                state.logf("at %v: unexpected spill of unnamed register %s\n", v, vReg);
                            }

                            break;

                        }

                        setSlot(slot, new VarLoc(last.Registers,StackOffset(stackOffset)));
                        if (state.loggingEnabled)
                        {
                            state.logf("at %v: %v spilled to stack location %v\n", v, state.slots[slot], home);
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

                        foreach (var (_, __slot) in locs.registers[vReg.num])
                        {
                            slot = __slot;
                            if (!newSlots[slot])
                            {
                                state.logf("at %v: overwrote %v in register %v\n", v, state.slots[slot], vReg);
                            }

                        }

                        slot = slot__prev1;
                    }
                }

                {
                    var slot__prev1 = slot;

                    foreach (var (_, __slot) in locs.registers[vReg.num])
                    {
                        slot = __slot;
                        last = locs.slots[slot];
                        setSlot(slot, new VarLoc(last.Registers&^(1<<uint8(vReg.num)),last.StackOffset));
                    }

                    slot = slot__prev1;
                }

                locs.registers[vReg.num] = locs.registers[vReg.num][..0L];
                locs.registers[vReg.num] = append(locs.registers[vReg.num], vSlots);
                {
                    var slot__prev1 = slot;

                    foreach (var (_, __slot) in vSlots)
                    {
                        slot = __slot;
                        if (state.loggingEnabled)
                        {
                            state.logf("at %v: %v now in %s\n", v, state.slots[slot], vReg);
                        }

                        last = locs.slots[slot];
                        setSlot(slot, new VarLoc(1<<uint8(vReg.num)|last.Registers,last.StackOffset));

                    }

                    slot = slot__prev1;
                }
                        return changed;

        }

        // varOffset returns the offset of slot within the user variable it was
        // decomposed from. This has nothing to do with its stack offset.
        private static long varOffset(LocalSlot slot)
        {
            var offset = slot.Off;
            var s = _addr_slot;
            while (s.SplitOf != null)
            {
                offset += s.SplitOffset;
                s = s.SplitOf;
            }

            return offset;

        }

        private partial struct partsByVarOffset
        {
            public slice<SlotID> slotIDs;
            public slice<LocalSlot> slots;
        }

        private static long Len(this partsByVarOffset a)
        {
            return len(a.slotIDs);
        }
        private static bool Less(this partsByVarOffset a, long i, long j)
        {
            return varOffset(a.slots[a.slotIDs[i]]) < varOffset(a.slots[a.slotIDs[j]]);
        }
        private static void Swap(this partsByVarOffset a, long i, long j)
        {
            a.slotIDs[i] = a.slotIDs[j];
            a.slotIDs[j] = a.slotIDs[i];
        }

        // A pendingEntry represents the beginning of a location list entry, missing
        // only its end coordinate.
        private partial struct pendingEntry
        {
            public bool present;
            public ID startBlock; // The location of each piece of the variable, in the same order as the
// SlotIDs in varParts.
            public ID startValue; // The location of each piece of the variable, in the same order as the
// SlotIDs in varParts.
            public slice<VarLoc> pieces;
        }

        private static void clear(this ptr<pendingEntry> _addr_e)
        {
            ref pendingEntry e = ref _addr_e.val;

            e.present = false;
            e.startBlock = 0L;
            e.startValue = 0L;
            foreach (var (i) in e.pieces)
            {
                e.pieces[i] = new VarLoc();
            }

        }

        // canMerge reports whether the location description for new is the same as
        // pending.
        private static bool canMerge(VarLoc pending, VarLoc @new)
        {
            if (pending.absent() && @new.absent())
            {
                return true;
            }

            if (pending.absent() || @new.absent())
            {
                return false;
            }

            if (pending.onStack())
            {
                return pending.StackOffset == @new.StackOffset;
            }

            if (pending.Registers != 0L && @new.Registers != 0L)
            {
                return firstReg(pending.Registers) == firstReg(@new.Registers);
            }

            return false;

        }

        // firstReg returns the first register in set that is present.
        private static byte firstReg(RegisterSet set)
        {
            if (set == 0L)
            { 
                // This is wrong, but there seem to be some situations where we
                // produce locations with no storage.
                return 0L;

            }

            return uint8(bits.TrailingZeros64(uint64(set)));

        }

        // buildLocationLists builds location lists for all the user variables in
        // state.f, using the information about block state in blockLocs.
        // The returned location lists are not fully complete. They are in terms of
        // SSA values rather than PCs, and have no base address/end entries. They will
        // be finished by PutLocationList.
        private static void buildLocationLists(this ptr<debugState> _addr_state, slice<ptr<BlockDebug>> blockLocs)
        {
            ref debugState state = ref _addr_state.val;
 
            // Run through the function in program text order, building up location
            // lists as we go. The heavy lifting has mostly already been done.

            ptr<Block> prevBlock;
            foreach (var (_, b) in state.f.Blocks)
            {
                state.mergePredecessors(b, blockLocs, prevBlock);

                if (!blockLocs[b.ID].relevant)
                { 
                    // Handle any differences among predecessor blocks and previous block (perhaps not a predecessor)
                    {
                        var varID__prev2 = varID;

                        foreach (var (_, __varID) in state.changedVars.contents())
                        {
                            varID = __varID;
                            state.updateVar(VarID(varID), b, BlockStart);
                        }

                        varID = varID__prev2;
                    }

                    continue;

                }

                var zeroWidthPending = false;
                long apcChangedSize = 0L; // size of changedVars for leading Args, Phi, ClosurePtr
                // expect to see values in pattern (apc)* (zerowidth|real)*
                foreach (var (_, v) in b.Values)
                {
                    var slots = state.valueNames[v.ID];
                    ptr<Register> (reg, _) = state.f.getHome(v.ID)._<ptr<Register>>();
                    var changed = state.processValue(v, slots, reg); // changed == added to state.changedVars

                    if (opcodeTable[v.Op].zeroWidth)
                    {
                        if (changed)
                        {
                            if (v.Op == OpArg || v.Op == OpPhi || v.Op.isLoweredGetClosurePtr())
                            { 
                                // These ranges begin at true beginning of block, not after first instruction
                                if (zeroWidthPending)
                                {
                                    b.Func.Fatalf("Unexpected op mixed with OpArg/OpPhi/OpLoweredGetClosurePtr at beginning of block %s in %s\n%s", b, b.Func.Name, b.Func);
                                }

                                apcChangedSize = len(state.changedVars.contents());
                                continue;

                            } 
                            // Other zero-width ops must wait on a "real" op.
                            zeroWidthPending = true;

                        }

                        continue;

                    }

                    if (!changed && !zeroWidthPending)
                    {
                        continue;
                    } 
                    // Not zero-width; i.e., a "real" instruction.
                    zeroWidthPending = false;
                    {
                        var i__prev3 = i;
                        var varID__prev3 = varID;

                        foreach (var (__i, __varID) in state.changedVars.contents())
                        {
                            i = __i;
                            varID = __varID;
                            if (i < apcChangedSize)
                            { // buffered true start-of-block changes
                                state.updateVar(VarID(varID), v.Block, BlockStart);

                            }
                            else
                            {
                                state.updateVar(VarID(varID), v.Block, v);
                            }

                        }

                        i = i__prev3;
                        varID = varID__prev3;
                    }

                    state.changedVars.clear();
                    apcChangedSize = 0L;

                }
                {
                    var i__prev2 = i;
                    var varID__prev2 = varID;

                    foreach (var (__i, __varID) in state.changedVars.contents())
                    {
                        i = __i;
                        varID = __varID;
                        if (i < apcChangedSize)
                        { // buffered true start-of-block changes
                            state.updateVar(VarID(varID), b, BlockStart);

                        }
                        else
                        {
                            state.updateVar(VarID(varID), b, BlockEnd);
                        }

                    }

                    i = i__prev2;
                    varID = varID__prev2;
                }

                prevBlock = b;

            }
            if (state.loggingEnabled)
            {
                state.logf("location lists:\n");
            } 

            // Flush any leftover entries live at the end of the last block.
            {
                var varID__prev1 = varID;

                foreach (var (__varID) in state.lists)
                {
                    varID = __varID;
                    state.writePendingEntry(VarID(varID), state.f.Blocks[len(state.f.Blocks) - 1L].ID, BlockEnd.ID);
                    var list = state.lists[varID];
                    if (state.loggingEnabled)
                    {
                        if (len(list) == 0L)
                        {
                            state.logf("\t%v : empty list\n", state.vars[varID]);
                        }
                        else
                        {
                            state.logf("\t%v : %q\n", state.vars[varID], hex.EncodeToString(state.lists[varID]));
                        }

                    }

                }

                varID = varID__prev1;
            }
        }

        // updateVar updates the pending location list entry for varID to
        // reflect the new locations in curLoc, beginning at v in block b.
        // v may be one of the special values indicating block start or end.
        private static void updateVar(this ptr<debugState> _addr_state, VarID varID, ptr<Block> _addr_b, ptr<Value> _addr_v)
        {
            ref debugState state = ref _addr_state.val;
            ref Block b = ref _addr_b.val;
            ref Value v = ref _addr_v.val;

            var curLoc = state.currentState.slots; 
            // Assemble the location list entry with whatever's live.
            var empty = true;
            {
                var slotID__prev1 = slotID;

                foreach (var (_, __slotID) in state.varSlots[varID])
                {
                    slotID = __slotID;
                    if (!curLoc[slotID].absent())
                    {
                        empty = false;
                        break;
                    }

                }

                slotID = slotID__prev1;
            }

            var pending = _addr_state.pendingEntries[varID];
            if (empty)
            {
                state.writePendingEntry(varID, b.ID, v.ID);
                pending.clear();
                return ;
            } 

            // Extend the previous entry if possible.
            if (pending.present)
            {
                var merge = true;
                {
                    var i__prev1 = i;
                    var slotID__prev1 = slotID;

                    foreach (var (__i, __slotID) in state.varSlots[varID])
                    {
                        i = __i;
                        slotID = __slotID;
                        if (!canMerge(pending.pieces[i], curLoc[slotID]))
                        {
                            merge = false;
                            break;
                        }

                    }

                    i = i__prev1;
                    slotID = slotID__prev1;
                }

                if (merge)
                {
                    return ;
                }

            }

            state.writePendingEntry(varID, b.ID, v.ID);
            pending.present = true;
            pending.startBlock = b.ID;
            pending.startValue = v.ID;
            {
                var i__prev1 = i;

                foreach (var (__i, __slot) in state.varSlots[varID])
                {
                    i = __i;
                    slot = __slot;
                    pending.pieces[i] = curLoc[slot];
                }

                i = i__prev1;
            }
        }

        // writePendingEntry writes out the pending entry for varID, if any,
        // terminated at endBlock/Value.
        private static void writePendingEntry(this ptr<debugState> _addr_state, VarID varID, ID endBlock, ID endValue)
        {
            ref debugState state = ref _addr_state.val;

            var pending = state.pendingEntries[varID];
            if (!pending.present)
            {
                return ;
            } 

            // Pack the start/end coordinates into the start/end addresses
            // of the entry, for decoding by PutLocationList.
            var (start, startOK) = encodeValue(_addr_state.ctxt, pending.startBlock, pending.startValue);
            var (end, endOK) = encodeValue(_addr_state.ctxt, endBlock, endValue);
            if (!startOK || !endOK)
            { 
                // If someone writes a function that uses >65K values,
                // they get incomplete debug info on 32-bit platforms.
                return ;

            }

            if (start == end)
            {
                if (state.loggingEnabled)
                { 
                    // Printf not logf so not gated by GOSSAFUNC; this should fire very rarely.
                    fmt.Printf("Skipping empty location list for %v in %s\n", state.vars[varID], state.f.Name);

                }

                return ;

            }

            var list = state.lists[varID];
            list = appendPtr(_addr_state.ctxt, list, start);
            list = appendPtr(_addr_state.ctxt, list, end); 
            // Where to write the length of the location description once
            // we know how big it is.
            var sizeIdx = len(list);
            list = list[..len(list) + 2L];

            if (state.loggingEnabled)
            {
                slice<@string> partStrs = default;
                {
                    var i__prev1 = i;
                    var slot__prev1 = slot;

                    foreach (var (__i, __slot) in state.varSlots[varID])
                    {
                        i = __i;
                        slot = __slot;
                        partStrs = append(partStrs, fmt.Sprintf("%v@%v", state.slots[slot], state.LocString(pending.pieces[i])));
                    }

                    i = i__prev1;
                    slot = slot__prev1;
                }

                state.logf("Add entry for %v: \tb%vv%v-b%vv%v = \t%v\n", state.vars[varID], pending.startBlock, pending.startValue, endBlock, endValue, strings.Join(partStrs, " "));

            }

            {
                var i__prev1 = i;

                foreach (var (__i, __slotID) in state.varSlots[varID])
                {
                    i = __i;
                    slotID = __slotID;
                    var loc = pending.pieces[i];
                    var slot = state.slots[slotID];

                    if (!loc.absent())
                    {
                        if (loc.onStack())
                        {
                            if (loc.stackOffsetValue() == 0L)
                            {
                                list = append(list, dwarf.DW_OP_call_frame_cfa);
                            }
                            else
                            {
                                list = append(list, dwarf.DW_OP_fbreg);
                                list = dwarf.AppendSleb128(list, int64(loc.stackOffsetValue()));
                            }

                        }
                        else
                        {
                            var regnum = state.ctxt.Arch.DWARFRegisters[state.registers[firstReg(loc.Registers)].ObjNum()];
                            if (regnum < 32L)
                            {
                                list = append(list, dwarf.DW_OP_reg0 + byte(regnum));
                            }
                            else
                            {
                                list = append(list, dwarf.DW_OP_regx);
                                list = dwarf.AppendUleb128(list, uint64(regnum));
                            }

                        }

                    }

                    if (len(state.varSlots[varID]) > 1L)
                    {
                        list = append(list, dwarf.DW_OP_piece);
                        list = dwarf.AppendUleb128(list, uint64(slot.Type.Size()));
                    }

                }

                i = i__prev1;
            }

            state.ctxt.Arch.ByteOrder.PutUint16(list[sizeIdx..], uint16(len(list) - sizeIdx - 2L));
            state.lists[varID] = list;

        }

        // PutLocationList adds list (a location list in its intermediate representation) to listSym.
        private static void PutLocationList(this ptr<FuncDebug> _addr_debugInfo, slice<byte> list, ptr<obj.Link> _addr_ctxt, ptr<obj.LSym> _addr_listSym, ptr<obj.LSym> _addr_startPC)
        {
            ref FuncDebug debugInfo = ref _addr_debugInfo.val;
            ref obj.Link ctxt = ref _addr_ctxt.val;
            ref obj.LSym listSym = ref _addr_listSym.val;
            ref obj.LSym startPC = ref _addr_startPC.val;

            var getPC = debugInfo.GetPC;

            if (ctxt.UseBASEntries)
            {
                listSym.WriteInt(ctxt, listSym.Size, ctxt.Arch.PtrSize, ~0L);
                listSym.WriteAddr(ctxt, listSym.Size, ctxt.Arch.PtrSize, startPC, 0L);
            } 

            // Re-read list, translating its address from block/value ID to PC.
            {
                long i = 0L;

                while (i < len(list))
                {
                    var begin = getPC(decodeValue(_addr_ctxt, readPtr(_addr_ctxt, list[i..])));
                    var end = getPC(decodeValue(_addr_ctxt, readPtr(_addr_ctxt, list[i + ctxt.Arch.PtrSize..]))); 

                    // Horrible hack. If a range contains only zero-width
                    // instructions, e.g. an Arg, and it's at the beginning of the
                    // function, this would be indistinguishable from an
                    // end entry. Fudge it.
                    if (begin == 0L && end == 0L)
                    {
                        end = 1L;
                    }

                    if (ctxt.UseBASEntries)
                    {
                        listSym.WriteInt(ctxt, listSym.Size, ctxt.Arch.PtrSize, int64(begin));
                        listSym.WriteInt(ctxt, listSym.Size, ctxt.Arch.PtrSize, int64(end));
                    }
                    else
                    {
                        listSym.WriteCURelativeAddr(ctxt, listSym.Size, startPC, int64(begin));
                        listSym.WriteCURelativeAddr(ctxt, listSym.Size, startPC, int64(end));
                    }

                    i += 2L * ctxt.Arch.PtrSize;
                    long datalen = 2L + int(ctxt.Arch.ByteOrder.Uint16(list[i..]));
                    listSym.WriteBytes(ctxt, listSym.Size, list[i..i + datalen]); // copy datalen and location encoding
                    i += datalen;

                } 

                // Location list contents, now with real PCs.
                // End entry.

            } 

            // Location list contents, now with real PCs.
            // End entry.
            listSym.WriteInt(ctxt, listSym.Size, ctxt.Arch.PtrSize, 0L);
            listSym.WriteInt(ctxt, listSym.Size, ctxt.Arch.PtrSize, 0L);

        }

        // Pack a value and block ID into an address-sized uint, returning ~0 if they
        // don't fit.
        private static (ulong, bool) encodeValue(ptr<obj.Link> _addr_ctxt, ID b, ID v) => func((_, panic, __) =>
        {
            ulong _p0 = default;
            bool _p0 = default;
            ref obj.Link ctxt = ref _addr_ctxt.val;

            if (ctxt.Arch.PtrSize == 8L)
            {
                var result = uint64(b) << (int)(32L) | uint64(uint32(v)); 
                //ctxt.Logf("b %#x (%d) v %#x (%d) -> %#x\n", b, b, v, v, result)
                return (result, true);

            }

            if (ctxt.Arch.PtrSize != 4L)
            {
                panic("unexpected pointer size");
            }

            if (ID(int16(b)) != b || ID(int16(v)) != v)
            {
                return (0L, false);
            }

            return (uint64(b) << (int)(16L) | uint64(uint16(v)), true);

        });

        // Unpack a value and block ID encoded by encodeValue.
        private static (ID, ID) decodeValue(ptr<obj.Link> _addr_ctxt, ulong word) => func((_, panic, __) =>
        {
            ID _p0 = default;
            ID _p0 = default;
            ref obj.Link ctxt = ref _addr_ctxt.val;

            if (ctxt.Arch.PtrSize == 8L)
            {
                var b = ID(word >> (int)(32L));
                var v = ID(word); 
                //ctxt.Logf("%#x -> b %#x (%d) v %#x (%d)\n", word, b, b, v, v)
                return (b, v);

            }

            if (ctxt.Arch.PtrSize != 4L)
            {
                panic("unexpected pointer size");
            }

            return (ID(word >> (int)(16L)), ID(int16(word)));

        });

        // Append a pointer-sized uint to buf.
        private static slice<byte> appendPtr(ptr<obj.Link> _addr_ctxt, slice<byte> buf, ulong word)
        {
            ref obj.Link ctxt = ref _addr_ctxt.val;

            if (cap(buf) < len(buf) + 20L)
            {
                var b = make_slice<byte>(len(buf), 20L + cap(buf) * 2L);
                copy(b, buf);
                buf = b;
            }

            var writeAt = len(buf);
            buf = buf[0L..len(buf) + ctxt.Arch.PtrSize];
            writePtr(_addr_ctxt, buf[writeAt..], word);
            return buf;

        }

        // Write a pointer-sized uint to the beginning of buf.
        private static void writePtr(ptr<obj.Link> _addr_ctxt, slice<byte> buf, ulong word) => func((_, panic, __) =>
        {
            ref obj.Link ctxt = ref _addr_ctxt.val;

            switch (ctxt.Arch.PtrSize)
            {
                case 4L: 
                    ctxt.Arch.ByteOrder.PutUint32(buf, uint32(word));
                    break;
                case 8L: 
                    ctxt.Arch.ByteOrder.PutUint64(buf, word);
                    break;
                default: 
                    panic("unexpected pointer size");
                    break;
            }


        });

        // Read a pointer-sized uint from the beginning of buf.
        private static ulong readPtr(ptr<obj.Link> _addr_ctxt, slice<byte> buf) => func((_, panic, __) =>
        {
            ref obj.Link ctxt = ref _addr_ctxt.val;

            switch (ctxt.Arch.PtrSize)
            {
                case 4L: 
                    return uint64(ctxt.Arch.ByteOrder.Uint32(buf));
                    break;
                case 8L: 
                    return ctxt.Arch.ByteOrder.Uint64(buf);
                    break;
                default: 
                    panic("unexpected pointer size");
                    break;
            }


        });
    }
}}}}
