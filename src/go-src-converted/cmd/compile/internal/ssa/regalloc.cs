// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Register allocation.
//
// We use a version of a linear scan register allocator. We treat the
// whole function as a single long basic block and run through
// it using a greedy register allocator. Then all merge edges
// (those targeting a block with len(Preds)>1) are processed to
// shuffle data into the place that the target of the edge expects.
//
// The greedy allocator moves values into registers just before they
// are used, spills registers only when necessary, and spills the
// value whose next use is farthest in the future.
//
// The register allocator requires that a block is not scheduled until
// at least one of its predecessors have been scheduled. The most recent
// such predecessor provides the starting register state for a block.
//
// It also requires that there are no critical edges (critical =
// comes from a block with >1 successor and goes to a block with >1
// predecessor).  This makes it easy to add fixup code on merge edges -
// the source of a merge edge has only one successor, so we can add
// fixup code to the end of that block.

// Spilling
//
// During the normal course of the allocator, we might throw a still-live
// value out of all registers. When that value is subsequently used, we must
// load it from a slot on the stack. We must also issue an instruction to
// initialize that stack location with a copy of v.
//
// pre-regalloc:
//   (1) v = Op ...
//   (2) x = Op ...
//   (3) ... = Op v ...
//
// post-regalloc:
//   (1) v = Op ...    : AX // computes v, store result in AX
//       s = StoreReg v     // spill v to a stack slot
//   (2) x = Op ...    : AX // some other op uses AX
//       c = LoadReg s : CX // restore v from stack slot
//   (3) ... = Op c ...     // use the restored value
//
// Allocation occurs normally until we reach (3) and we realize we have
// a use of v and it isn't in any register. At that point, we allocate
// a spill (a StoreReg) for v. We can't determine the correct place for
// the spill at this point, so we allocate the spill as blockless initially.
// The restore is then generated to load v back into a register so it can
// be used. Subsequent uses of v will use the restored value c instead.
//
// What remains is the question of where to schedule the spill.
// During allocation, we keep track of the dominator of all restores of v.
// The spill of v must dominate that block. The spill must also be issued at
// a point where v is still in a register.
//
// To find the right place, start at b, the block which dominates all restores.
//  - If b is v.Block, then issue the spill right after v.
//    It is known to be in a register at that point, and dominates any restores.
//  - Otherwise, if v is in a register at the start of b,
//    put the spill of v at the start of b.
//  - Otherwise, set b = immediate dominator of b, and repeat.
//
// Phi values are special, as always. We define two kinds of phis, those
// where the merge happens in a register (a "register" phi) and those where
// the merge happens in a stack location (a "stack" phi).
//
// A register phi must have the phi and all of its inputs allocated to the
// same register. Register phis are spilled similarly to regular ops.
//
// A stack phi must have the phi and all of its inputs allocated to the same
// stack location. Stack phis start out life already spilled - each phi
// input must be a store (using StoreReg) at the end of the corresponding
// predecessor block.
//     b1: y = ... : AX        b2: z = ... : BX
//         y2 = StoreReg y         z2 = StoreReg z
//         goto b3                 goto b3
//     b3: x = phi(y2, z2)
// The stack allocator knows that StoreReg args of stack-allocated phis
// must be allocated to the same stack slot as the phi that uses them.
// x is now a spilled value and a restore must appear before its first use.

// TODO

// Use an affinity graph to mark two values which should use the
// same register. This affinity graph will be used to prefer certain
// registers for allocation. This affinity helps eliminate moves that
// are required for phi implementations and helps generate allocations
// for 2-register architectures.

// Note: regalloc generates a not-quite-SSA output. If we have:
//
//             b1: x = ... : AX
//                 x2 = StoreReg x
//                 ... AX gets reused for something else ...
//                 if ... goto b3 else b4
//
//   b3: x3 = LoadReg x2 : BX       b4: x4 = LoadReg x2 : CX
//       ... use x3 ...                 ... use x4 ...
//
//             b2: ... use x3 ...
//
// If b3 is the primary predecessor of b2, then we use x3 in b2 and
// add a x4:CX->BX copy at the end of b4.
// But the definition of x3 doesn't dominate b2.  We should really
// insert a dummy phi at the start of b2 (x5=phi(x3,x4):BX) to keep
// SSA form. For now, we ignore this problem as remaining in strict
// SSA form isn't needed after regalloc. We'll just leave the use
// of x3 not dominated by the definition of x3, and the CX->BX copy
// will have no use (so don't run deadcode after regalloc!).
// TODO: maybe we should introduce these extra phis?

// package ssa -- go2cs converted at 2020 October 09 05:25:44 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\regalloc.go
using types = go.cmd.compile.@internal.types_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using sys = go.cmd.@internal.sys_package;
using fmt = go.fmt_package;
using bits = go.math.bits_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        private static readonly var moveSpills = iota;
        private static readonly var logSpills = 0;
        private static readonly var regDebug = 1;
        private static readonly var stackDebug = 2;


        // distance is a measure of how far into the future values are used.
        // distance is measured in units of instructions.
        private static readonly long likelyDistance = (long)1L;
        private static readonly long normalDistance = (long)10L;
        private static readonly long unlikelyDistance = (long)100L;


        // regalloc performs register allocation on f. It sets f.RegAlloc
        // to the resulting allocation.
        private static void regalloc(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            regAllocState s = default;
            s.init(f);
            s.regalloc(f);
        }

        private partial struct register // : byte
        {
        }

        private static readonly register noRegister = (register)255L;

        // A regMask encodes a set of machine registers.
        // TODO: regMask -> regSet?


        // A regMask encodes a set of machine registers.
        // TODO: regMask -> regSet?
        private partial struct regMask // : ulong
        {
        }

        private static @string String(this regMask m)
        {
            @string s = "";
            for (var r = register(0L); m != 0L; r++)
            {
                if (m >> (int)(r) & 1L == 0L)
                {
                    continue;
                }

                m &= regMask(1L) << (int)(r);
                if (s != "")
                {
                    s += " ";
                }

                s += fmt.Sprintf("r%d", r);

            }

            return s;

        }

        private static @string RegMaskString(this ptr<regAllocState> _addr_s, regMask m)
        {
            ref regAllocState s = ref _addr_s.val;

            @string str = "";
            for (var r = register(0L); m != 0L; r++)
            {
                if (m >> (int)(r) & 1L == 0L)
                {
                    continue;
                }

                m &= regMask(1L) << (int)(r);
                if (str != "")
                {
                    str += " ";
                }

                str += s.registers[r].String();

            }

            return str;

        }

        // countRegs returns the number of set bits in the register mask.
        private static long countRegs(regMask r)
        {
            return bits.OnesCount64(uint64(r));
        }

        // pickReg picks an arbitrary register from the register mask.
        private static register pickReg(regMask r) => func((_, panic, __) =>
        {
            if (r == 0L)
            {
                panic("can't pick a register from an empty set");
            } 
            // pick the lowest one
            return register(bits.TrailingZeros64(uint64(r)));

        });

        private partial struct use
        {
            public int dist; // distance from start of the block to a use of a value
            public src.XPos pos; // source position of the use
            public ptr<use> next; // linked list of uses of a value in nondecreasing dist order
        }

        // A valState records the register allocation state for a (pre-regalloc) value.
        private partial struct valState
        {
            public regMask regs; // the set of registers holding a Value (usually just one)
            public ptr<use> uses; // list of uses in this block
            public ptr<Value> spill; // spilled copy of the Value (if any)
            public int restoreMin; // minimum of all restores' blocks' sdom.entry
            public int restoreMax; // maximum of all restores' blocks' sdom.exit
            public bool needReg; // cached value of !v.Type.IsMemory() && !v.Type.IsVoid() && !.v.Type.IsFlags()
            public bool rematerializeable; // cached value of v.rematerializeable()
        }

        private partial struct regState
        {
            public ptr<Value> v; // Original (preregalloc) Value stored in this register.
            public ptr<Value> c; // A Value equal to v which is currently in a register.  Might be v or a copy of it.
// If a register is unused, v==c==nil
        }

        private partial struct regAllocState
        {
            public ptr<Func> f;
            public SparseTree sdom;
            public slice<Register> registers;
            public register numRegs;
            public register SPReg;
            public register SBReg;
            public register GReg;
            public regMask allocatable; // for each block, its primary predecessor.
// A predecessor of b is primary if it is the closest
// predecessor that appears before b in the layout order.
// We record the index in the Preds list where the primary predecessor sits.
            public slice<int> primary; // live values at the end of each block.  live[b.ID] is a list of value IDs
// which are live at the end of b, together with a count of how many instructions
// forward to the next use.
            public slice<slice<liveInfo>> live; // desired register assignments at the end of each block.
// Note that this is a static map computed before allocation occurs. Dynamic
// register desires (from partially completed allocations) will trump
// this information.
            public slice<desiredState> desired; // current state of each (preregalloc) Value
            public slice<valState> values; // ID of SP, SB values
            public ID sp; // For each Value, map from its value ID back to the
// preregalloc Value it was derived from.
            public ID sb; // For each Value, map from its value ID back to the
// preregalloc Value it was derived from.
            public slice<ptr<Value>> orig; // current state of each register
            public slice<regState> regs; // registers that contain values which can't be kicked out
            public regMask nospill; // mask of registers currently in use
            public regMask used; // mask of registers used in the current instruction
            public regMask tmpused; // current block we're working on
            public ptr<Block> curBlock; // cache of use records
            public ptr<use> freeUseRecords; // endRegs[blockid] is the register state at the end of each block.
// encoded as a set of endReg records.
            public slice<slice<endReg>> endRegs; // startRegs[blockid] is the register state at the start of merge blocks.
// saved state does not include the state of phi ops in the block.
            public slice<slice<startReg>> startRegs; // spillLive[blockid] is the set of live spills at the end of each block
            public slice<slice<ID>> spillLive; // a set of copies we generated to move things around, and
// whether it is used in shuffle. Unused copies will be deleted.
            public map<ptr<Value>, bool> copies;
            public ptr<loopnest> loopnest; // choose a good order in which to visit blocks for allocation purposes.
            public slice<ptr<Block>> visitOrder;
        }

        private partial struct endReg
        {
            public register r;
            public ptr<Value> v; // pre-regalloc value held in this register (TODO: can we use ID here?)
            public ptr<Value> c; // cached version of the value
        }

        private partial struct startReg
        {
            public register r;
            public ptr<Value> v; // pre-regalloc value needed in this register
            public ptr<Value> c; // cached version of the value
            public src.XPos pos; // source position of use of this register
        }

        // freeReg frees up register r. Any current user of r is kicked out.
        private static void freeReg(this ptr<regAllocState> _addr_s, register r)
        {
            ref regAllocState s = ref _addr_s.val;

            var v = s.regs[r].v;
            if (v == null)
            {
                s.f.Fatalf("tried to free an already free register %d\n", r);
            } 

            // Mark r as unused.
            if (s.f.pass.debug > regDebug)
            {
                fmt.Printf("freeReg %s (dump %s/%s)\n", _addr_s.registers[r], v, s.regs[r].c);
            }

            s.regs[r] = new regState();
            s.values[v.ID].regs &= regMask(1L) << (int)(r);
            s.used &= regMask(1L) << (int)(r);

        }

        // freeRegs frees up all registers listed in m.
        private static void freeRegs(this ptr<regAllocState> _addr_s, regMask m)
        {
            ref regAllocState s = ref _addr_s.val;

            while (m & s.used != 0L)
            {
                s.freeReg(pickReg(m & s.used));
            }


        }

        // setOrig records that c's original value is the same as
        // v's original value.
        private static void setOrig(this ptr<regAllocState> _addr_s, ptr<Value> _addr_c, ptr<Value> _addr_v)
        {
            ref regAllocState s = ref _addr_s.val;
            ref Value c = ref _addr_c.val;
            ref Value v = ref _addr_v.val;

            while (int(c.ID) >= len(s.orig))
            {
                s.orig = append(s.orig, null);
            }

            if (s.orig[c.ID] != null)
            {
                s.f.Fatalf("orig value set twice %s %s", c, v);
            }

            s.orig[c.ID] = s.orig[v.ID];

        }

        // assignReg assigns register r to hold c, a copy of v.
        // r must be unused.
        private static void assignReg(this ptr<regAllocState> _addr_s, register r, ptr<Value> _addr_v, ptr<Value> _addr_c)
        {
            ref regAllocState s = ref _addr_s.val;
            ref Value v = ref _addr_v.val;
            ref Value c = ref _addr_c.val;

            if (s.f.pass.debug > regDebug)
            {
                fmt.Printf("assignReg %s %s/%s\n", _addr_s.registers[r], v, c);
            }

            if (s.regs[r].v != null)
            {
                s.f.Fatalf("tried to assign register %d to %s/%s but it is already used by %s", r, v, c, s.regs[r].v);
            } 

            // Update state.
            s.regs[r] = new regState(v,c);
            s.values[v.ID].regs |= regMask(1L) << (int)(r);
            s.used |= regMask(1L) << (int)(r);
            s.f.setHome(c, _addr_s.registers[r]);

        }

        // allocReg chooses a register from the set of registers in mask.
        // If there is no unused register, a Value will be kicked out of
        // a register to make room.
        private static register allocReg(this ptr<regAllocState> _addr_s, regMask mask, ptr<Value> _addr_v)
        {
            ref regAllocState s = ref _addr_s.val;
            ref Value v = ref _addr_v.val;

            if (v.OnWasmStack)
            {
                return noRegister;
            }

            mask &= s.allocatable;
            mask &= s.nospill;
            if (mask == 0L)
            {
                s.f.Fatalf("no register available for %s", v.LongString());
            } 

            // Pick an unused register if one is available.
            if (mask & ~s.used != 0L)
            {
                return pickReg(mask & ~s.used);
            } 

            // Pick a value to spill. Spill the value with the
            // farthest-in-the-future use.
            // TODO: Prefer registers with already spilled Values?
            // TODO: Modify preference using affinity graph.
            // TODO: if a single value is in multiple registers, spill one of them
            // before spilling a value in just a single register.

            // Find a register to spill. We spill the register containing the value
            // whose next use is as far in the future as possible.
            // https://en.wikipedia.org/wiki/Page_replacement_algorithm#The_theoretically_optimal_page_replacement_algorithm
            register r = default;
            var maxuse = int32(-1L);
            for (var t = register(0L); t < s.numRegs; t++)
            {
                if (mask >> (int)(t) & 1L == 0L)
                {
                    continue;
                }

                var v = s.regs[t].v;
                {
                    var n = s.values[v.ID].uses.dist;

                    if (n > maxuse)
                    { 
                        // v's next use is farther in the future than any value
                        // we've seen so far. A new best spill candidate.
                        r = t;
                        maxuse = n;

                    }

                }

            }

            if (maxuse == -1L)
            {
                s.f.Fatalf("couldn't find register to spill");
            }

            if (s.f.Config.ctxt.Arch.Arch == sys.ArchWasm)
            { 
                // TODO(neelance): In theory this should never happen, because all wasm registers are equal.
                // So if there is still a free register, the allocation should have picked that one in the first place instead of
                // trying to kick some other value out. In practice, this case does happen and it breaks the stack optimization.
                s.freeReg(r);
                return r;

            } 

            // Try to move it around before kicking out, if there is a free register.
            // We generate a Copy and record it. It will be deleted if never used.
            var v2 = s.regs[r].v;
            var m = s.compatRegs(v2.Type) & ~s.used & ~s.tmpused & ~(regMask(1L) << (int)(r));
            if (m != 0L && !s.values[v2.ID].rematerializeable && countRegs(s.values[v2.ID].regs) == 1L)
            {
                var r2 = pickReg(m);
                var c = s.curBlock.NewValue1(v2.Pos, OpCopy, v2.Type, s.regs[r].c);
                s.copies[c] = false;
                if (s.f.pass.debug > regDebug)
                {
                    fmt.Printf("copy %s to %s : %s\n", v2, c, _addr_s.registers[r2]);
                }

                s.setOrig(c, v2);
                s.assignReg(r2, v2, c);

            }

            s.freeReg(r);
            return r;

        }

        // makeSpill returns a Value which represents the spilled value of v.
        // b is the block in which the spill is used.
        private static ptr<Value> makeSpill(this ptr<regAllocState> _addr_s, ptr<Value> _addr_v, ptr<Block> _addr_b)
        {
            ref regAllocState s = ref _addr_s.val;
            ref Value v = ref _addr_v.val;
            ref Block b = ref _addr_b.val;

            var vi = _addr_s.values[v.ID];
            if (vi.spill != null)
            { 
                // Final block not known - keep track of subtree where restores reside.
                vi.restoreMin = min32(vi.restoreMin, s.sdom[b.ID].entry);
                vi.restoreMax = max32(vi.restoreMax, s.sdom[b.ID].exit);
                return _addr_vi.spill!;

            } 
            // Make a spill for v. We don't know where we want
            // to put it yet, so we leave it blockless for now.
            var spill = s.f.newValueNoBlock(OpStoreReg, v.Type, v.Pos); 
            // We also don't know what the spill's arg will be.
            // Leave it argless for now.
            s.setOrig(spill, v);
            vi.spill = spill;
            vi.restoreMin = s.sdom[b.ID].entry;
            vi.restoreMax = s.sdom[b.ID].exit;
            return _addr_spill!;

        }

        // allocValToReg allocates v to a register selected from regMask and
        // returns the register copy of v. Any previous user is kicked out and spilled
        // (if necessary). Load code is added at the current pc. If nospill is set the
        // allocated register is marked nospill so the assignment cannot be
        // undone until the caller allows it by clearing nospill. Returns a
        // *Value which is either v or a copy of v allocated to the chosen register.
        private static ptr<Value> allocValToReg(this ptr<regAllocState> _addr_s, ptr<Value> _addr_v, regMask mask, bool nospill, src.XPos pos) => func((_, panic, __) =>
        {
            ref regAllocState s = ref _addr_s.val;
            ref Value v = ref _addr_v.val;

            if (s.f.Config.ctxt.Arch.Arch == sys.ArchWasm && v.rematerializeable())
            {
                var c = v.copyIntoWithXPos(s.curBlock, pos);
                c.OnWasmStack = true;
                s.setOrig(c, v);
                return _addr_c!;
            }

            if (v.OnWasmStack)
            {
                return _addr_v!;
            }

            var vi = _addr_s.values[v.ID];
            pos = pos.WithNotStmt(); 
            // Check if v is already in a requested register.
            if (mask & vi.regs != 0L)
            {
                var r = pickReg(mask & vi.regs);
                if (s.regs[r].v != v || s.regs[r].c == null)
                {
                    panic("bad register state");
                }

                if (nospill)
                {
                    s.nospill |= regMask(1L) << (int)(r);
                }

                return _addr_s.regs[r].c!;

            }

            r = default; 
            // If nospill is set, the value is used immediately, so it can live on the WebAssembly stack.
            var onWasmStack = nospill && s.f.Config.ctxt.Arch.Arch == sys.ArchWasm;
            if (!onWasmStack)
            { 
                // Allocate a register.
                r = s.allocReg(mask, v);

            } 

            // Allocate v to the new register.
            c = ;
            if (vi.regs != 0L)
            { 
                // Copy from a register that v is already in.
                var r2 = pickReg(vi.regs);
                if (s.regs[r2].v != v)
                {
                    panic("bad register state");
                }

                c = s.curBlock.NewValue1(pos, OpCopy, v.Type, s.regs[r2].c);

            }
            else if (v.rematerializeable())
            { 
                // Rematerialize instead of loading from the spill location.
                c = v.copyIntoWithXPos(s.curBlock, pos);

            }
            else
            { 
                // Load v from its spill location.
                var spill = s.makeSpill(v, s.curBlock);
                if (s.f.pass.debug > logSpills)
                {
                    s.f.Warnl(vi.spill.Pos, "load spill for %v from %v", v, spill);
                }

                c = s.curBlock.NewValue1(pos, OpLoadReg, v.Type, spill);

            }

            s.setOrig(c, v);

            if (onWasmStack)
            {
                c.OnWasmStack = true;
                return _addr_c!;
            }

            s.assignReg(r, v, c);
            if (c.Op == OpLoadReg && s.isGReg(r))
            {
                s.f.Fatalf("allocValToReg.OpLoadReg targeting g: " + c.LongString());
            }

            if (nospill)
            {
                s.nospill |= regMask(1L) << (int)(r);
            }

            return _addr_c!;

        });

        // isLeaf reports whether f performs any calls.
        private static bool isLeaf(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            foreach (var (_, b) in f.Blocks)
            {
                foreach (var (_, v) in b.Values)
                {
                    if (opcodeTable[v.Op].call)
                    {
                        return false;
                    }

                }

            }
            return true;

        }

        private static void init(this ptr<regAllocState> _addr_s, ptr<Func> _addr_f) => func((defer, _, __) =>
        {
            ref regAllocState s = ref _addr_s.val;
            ref Func f = ref _addr_f.val;

            s.f = f;
            s.f.RegAlloc = s.f.Cache.locs[..0L];
            s.registers = f.Config.registers;
            {
                var nr = len(s.registers);

                if (nr == 0L || nr > int(noRegister) || nr > int(@unsafe.Sizeof(regMask(0L)) * 8L))
                {
                    s.f.Fatalf("bad number of registers: %d", nr);
                }
                else
                {
                    s.numRegs = register(nr);
                } 
                // Locate SP, SB, and g registers.

            } 
            // Locate SP, SB, and g registers.
            s.SPReg = noRegister;
            s.SBReg = noRegister;
            s.GReg = noRegister;
            for (var r = register(0L); r < s.numRegs; r++)
            {
                switch (s.registers[r].String())
                {
                    case "SP": 
                        s.SPReg = r;
                        break;
                    case "SB": 
                        s.SBReg = r;
                        break;
                    case "g": 
                        s.GReg = r;
                        break;
                }

            } 
            // Make sure we found all required registers.
 
            // Make sure we found all required registers.

            if (noRegister == s.SPReg) 
                s.f.Fatalf("no SP register found");
            else if (noRegister == s.SBReg) 
                s.f.Fatalf("no SB register found");
            else if (noRegister == s.GReg) 
                if (f.Config.hasGReg)
                {
                    s.f.Fatalf("no g register found");
                }

            // Figure out which registers we're allowed to use.
            s.allocatable = s.f.Config.gpRegMask | s.f.Config.fpRegMask | s.f.Config.specialRegMask;
            s.allocatable &= 1L << (int)(s.SPReg);
            s.allocatable &= 1L << (int)(s.SBReg);
            if (s.f.Config.hasGReg)
            {
                s.allocatable &= 1L << (int)(s.GReg);
            }

            if (s.f.Config.ctxt.Framepointer_enabled && s.f.Config.FPReg >= 0L)
            {
                s.allocatable &= 1L << (int)(uint(s.f.Config.FPReg));
            }

            if (s.f.Config.LinkReg != -1L)
            {
                if (isLeaf(_addr_f))
                { 
                    // Leaf functions don't save/restore the link register.
                    s.allocatable &= 1L << (int)(uint(s.f.Config.LinkReg));

                }

                if (s.f.Config.arch == "arm" && objabi.GOARM == 5L)
                { 
                    // On ARMv5 we insert softfloat calls at each FP instruction.
                    // This clobbers LR almost everywhere. Disable allocating LR
                    // on ARMv5.
                    s.allocatable &= 1L << (int)(uint(s.f.Config.LinkReg));

                }

            }

            if (s.f.Config.ctxt.Flag_dynlink)
            {
                switch (s.f.Config.arch)
                {
                    case "amd64": 
                        s.allocatable &= 1L << (int)(15L); // R15
                        break;
                    case "arm": 
                        s.allocatable &= 1L << (int)(9L); // R9
                        break;
                    case "ppc64le": 
                        break;
                    case "arm64": 
                        break;
                    case "386": 
                        break;
                    case "s390x": 
                        s.allocatable &= 1L << (int)(11L); // R11
                        break;
                    default: 
                        s.f.fe.Fatalf(src.NoXPos, "arch %s not implemented", s.f.Config.arch);
                        break;
                }

            }

            if (s.f.Config.use387)
            {
                s.allocatable &= 1L << (int)(15L); // X7 disallowed (one 387 register is used as scratch space during SSE->387 generation in ../x86/387.go)
            } 

            // Linear scan register allocation can be influenced by the order in which blocks appear.
            // Decouple the register allocation order from the generated block order.
            // This also creates an opportunity for experiments to find a better order.
            s.visitOrder = layoutRegallocOrder(f); 

            // Compute block order. This array allows us to distinguish forward edges
            // from backward edges and compute how far they go.
            var blockOrder = make_slice<int>(f.NumBlocks());
            {
                var i__prev1 = i;
                var b__prev1 = b;

                foreach (var (__i, __b) in s.visitOrder)
                {
                    i = __i;
                    b = __b;
                    blockOrder[b.ID] = int32(i);
                }

                i = i__prev1;
                b = b__prev1;
            }

            s.regs = make_slice<regState>(s.numRegs);
            var nv = f.NumValues();
            if (cap(s.f.Cache.regallocValues) >= nv)
            {
                s.f.Cache.regallocValues = s.f.Cache.regallocValues[..nv];
            }
            else
            {
                s.f.Cache.regallocValues = make_slice<valState>(nv);
            }

            s.values = s.f.Cache.regallocValues;
            s.orig = make_slice<ptr<Value>>(nv);
            s.copies = make_map<ptr<Value>, bool>();
            {
                var b__prev1 = b;

                foreach (var (_, __b) in s.visitOrder)
                {
                    b = __b;
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            if (!v.Type.IsMemory() && !v.Type.IsVoid() && !v.Type.IsFlags() && !v.Type.IsTuple())
                            {
                                s.values[v.ID].needReg = true;
                                s.values[v.ID].rematerializeable = v.rematerializeable();
                                s.orig[v.ID] = v;
                            } 
                            // Note: needReg is false for values returning Tuple types.
                            // Instead, we mark the corresponding Selects as needReg.
                        }

                        v = v__prev2;
                    }
                }

                b = b__prev1;
            }

            s.computeLive(); 

            // Compute primary predecessors.
            s.primary = make_slice<int>(f.NumBlocks());
            {
                var b__prev1 = b;

                foreach (var (_, __b) in s.visitOrder)
                {
                    b = __b;
                    long best = -1L;
                    {
                        var i__prev2 = i;

                        foreach (var (__i, __e) in b.Preds)
                        {
                            i = __i;
                            e = __e;
                            var p = e.b;
                            if (blockOrder[p.ID] >= blockOrder[b.ID])
                            {
                                continue; // backward edge
                            }

                            if (best == -1L || blockOrder[p.ID] > blockOrder[b.Preds[best].b.ID])
                            {
                                best = i;
                            }

                        }

                        i = i__prev2;
                    }

                    s.primary[b.ID] = int32(best);

                }

                b = b__prev1;
            }

            s.endRegs = make_slice<slice<endReg>>(f.NumBlocks());
            s.startRegs = make_slice<slice<startReg>>(f.NumBlocks());
            s.spillLive = make_slice<slice<ID>>(f.NumBlocks());
            s.sdom = f.Sdom(); 

            // wasm: Mark instructions that can be optimized to have their values only on the WebAssembly stack.
            if (f.Config.ctxt.Arch.Arch == sys.ArchWasm)
            {
                var canLiveOnStack = f.newSparseSet(f.NumValues());
                defer(f.retSparseSet(canLiveOnStack));
                {
                    var b__prev1 = b;

                    foreach (var (_, __b) in f.Blocks)
                    {
                        b = __b; 
                        // New block. Clear candidate set.
                        canLiveOnStack.clear();
                        foreach (var (_, c) in b.ControlValues())
                        {
                            if (c.Uses == 1L && !opcodeTable[c.Op].generic)
                            {
                                canLiveOnStack.add(c.ID);
                            }

                        } 
                        // Walking backwards.
                        {
                            var i__prev2 = i;

                            for (var i = len(b.Values) - 1L; i >= 0L; i--)
                            {
                                var v = b.Values[i];
                                if (canLiveOnStack.contains(v.ID))
                                {
                                    v.OnWasmStack = true;
                                }
                                else
                                { 
                                    // Value can not live on stack. Values are not allowed to be reordered, so clear candidate set.
                                    canLiveOnStack.clear();

                                }

                                foreach (var (_, arg) in v.Args)
                                { 
                                    // Value can live on the stack if:
                                    // - it is only used once
                                    // - it is used in the same basic block
                                    // - it is not a "mem" value
                                    // - it is a WebAssembly op
                                    if (arg.Uses == 1L && arg.Block == v.Block && !arg.Type.IsMemory() && !opcodeTable[arg.Op].generic)
                                    {
                                        canLiveOnStack.add(arg.ID);
                                    }

                                }

                            }


                            i = i__prev2;
                        }

                    }

                    b = b__prev1;
                }
            }

        });

        // Adds a use record for id at distance dist from the start of the block.
        // All calls to addUse must happen with nonincreasing dist.
        private static void addUse(this ptr<regAllocState> _addr_s, ID id, int dist, src.XPos pos)
        {
            ref regAllocState s = ref _addr_s.val;

            var r = s.freeUseRecords;
            if (r != null)
            {
                s.freeUseRecords = r.next;
            }
            else
            {
                r = addr(new use());
            }

            r.dist = dist;
            r.pos = pos;
            r.next = s.values[id].uses;
            s.values[id].uses = r;
            if (r.next != null && dist > r.next.dist)
            {
                s.f.Fatalf("uses added in wrong order");
            }

        }

        // advanceUses advances the uses of v's args from the state before v to the state after v.
        // Any values which have no more uses are deallocated from registers.
        private static void advanceUses(this ptr<regAllocState> _addr_s, ptr<Value> _addr_v)
        {
            ref regAllocState s = ref _addr_s.val;
            ref Value v = ref _addr_v.val;

            foreach (var (_, a) in v.Args)
            {
                if (!s.values[a.ID].needReg)
                {
                    continue;
                }

                var ai = _addr_s.values[a.ID];
                var r = ai.uses;
                ai.uses = r.next;
                if (r.next == null)
                { 
                    // Value is dead, free all registers that hold it.
                    s.freeRegs(ai.regs);

                }

                r.next = s.freeUseRecords;
                s.freeUseRecords = r;

            }

        }

        // liveAfterCurrentInstruction reports whether v is live after
        // the current instruction is completed.  v must be used by the
        // current instruction.
        private static bool liveAfterCurrentInstruction(this ptr<regAllocState> _addr_s, ptr<Value> _addr_v)
        {
            ref regAllocState s = ref _addr_s.val;
            ref Value v = ref _addr_v.val;

            var u = s.values[v.ID].uses;
            var d = u.dist;
            while (u != null && u.dist == d)
            {
                u = u.next;
            }

            return u != null && u.dist > d;

        }

        // Sets the state of the registers to that encoded in regs.
        private static void setState(this ptr<regAllocState> _addr_s, slice<endReg> regs)
        {
            ref regAllocState s = ref _addr_s.val;

            s.freeRegs(s.used);
            foreach (var (_, x) in regs)
            {
                s.assignReg(x.r, x.v, x.c);
            }

        }

        // compatRegs returns the set of registers which can store a type t.
        private static regMask compatRegs(this ptr<regAllocState> _addr_s, ptr<types.Type> _addr_t)
        {
            ref regAllocState s = ref _addr_s.val;
            ref types.Type t = ref _addr_t.val;

            regMask m = default;
            if (t.IsTuple() || t.IsFlags())
            {
                return 0L;
            }

            if (t.IsFloat() || t == types.TypeInt128)
            {
                if (t.Etype == types.TFLOAT32 && s.f.Config.fp32RegMask != 0L)
                {
                    m = s.f.Config.fp32RegMask;
                }
                else if (t.Etype == types.TFLOAT64 && s.f.Config.fp64RegMask != 0L)
                {
                    m = s.f.Config.fp64RegMask;
                }
                else
                {
                    m = s.f.Config.fpRegMask;
                }

            }
            else
            {
                m = s.f.Config.gpRegMask;
            }

            return m & s.allocatable;

        }

        // regspec returns the regInfo for operation op.
        private static regInfo regspec(this ptr<regAllocState> _addr_s, Op op)
        {
            ref regAllocState s = ref _addr_s.val;

            if (op == OpConvert)
            { 
                // OpConvert is a generic op, so it doesn't have a
                // register set in the static table. It can use any
                // allocatable integer register.
                var m = s.allocatable & s.f.Config.gpRegMask;
                return new regInfo(inputs:[]inputInfo{{regs:m}},outputs:[]outputInfo{{regs:m}});

            }

            return opcodeTable[op].reg;

        }

        private static bool isGReg(this ptr<regAllocState> _addr_s, register r)
        {
            ref regAllocState s = ref _addr_s.val;

            return s.f.Config.hasGReg && s.GReg == r;
        }

        private static void regalloc(this ptr<regAllocState> _addr_s, ptr<Func> _addr_f) => func((defer, _, __) =>
        {
            ref regAllocState s = ref _addr_s.val;
            ref Func f = ref _addr_f.val;

            var regValLiveSet = f.newSparseSet(f.NumValues()); // set of values that may be live in register
            defer(f.retSparseSet(regValLiveSet));
            slice<ptr<Value>> oldSched = default;
            slice<ptr<Value>> phis = default;
            slice<register> phiRegs = default;
            slice<ptr<Value>> args = default; 

            // Data structure used for computing desired registers.
            ref desiredState desired = ref heap(out ptr<desiredState> _addr_desired); 

            // Desired registers for inputs & outputs for each instruction in the block.
            private partial struct dentry
            {
                public array<register> @out; // desired output registers
                public array<array<register>> @in; // desired input registers (for inputs 0,1, and 2)
            }
            slice<dentry> dinfo = default;

            if (f.Entry != f.Blocks[0L])
            {
                f.Fatalf("entry block must be first");
            }

            {
                var b__prev1 = b;

                foreach (var (_, __b) in s.visitOrder)
                {
                    b = __b;
                    if (s.f.pass.debug > regDebug)
                    {
                        fmt.Printf("Begin processing block %v\n", b);
                    }

                    s.curBlock = b; 

                    // Initialize regValLiveSet and uses fields for this block.
                    // Walk backwards through the block doing liveness analysis.
                    regValLiveSet.clear();
                    {
                        var e__prev2 = e;

                        foreach (var (_, __e) in s.live[b.ID])
                        {
                            e = __e;
                            s.addUse(e.ID, int32(len(b.Values)) + e.dist, e.pos); // pseudo-uses from beyond end of block
                            regValLiveSet.add(e.ID);

                        }

                        e = e__prev2;
                    }

                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.ControlValues())
                        {
                            v = __v;
                            if (s.values[v.ID].needReg)
                            {
                                s.addUse(v.ID, int32(len(b.Values)), b.Pos); // pseudo-use by control values
                                regValLiveSet.add(v.ID);

                            }

                        }

                        v = v__prev2;
                    }

                    {
                        var i__prev2 = i;

                        for (var i = len(b.Values) - 1L; i >= 0L; i--)
                        {
                            var v = b.Values[i];
                            regValLiveSet.remove(v.ID);
                            if (v.Op == OpPhi)
                            { 
                                // Remove v from the live set, but don't add
                                // any inputs. This is the state the len(b.Preds)>1
                                // case below desires; it wants to process phis specially.
                                continue;

                            }

                            if (opcodeTable[v.Op].call)
                            { 
                                // Function call clobbers all the registers but SP and SB.
                                regValLiveSet.clear();
                                if (s.sp != 0L && s.values[s.sp].uses != null)
                                {
                                    regValLiveSet.add(s.sp);
                                }

                                if (s.sb != 0L && s.values[s.sb].uses != null)
                                {
                                    regValLiveSet.add(s.sb);
                                }

                            }

                            {
                                var a__prev3 = a;

                                foreach (var (_, __a) in v.Args)
                                {
                                    a = __a;
                                    if (!s.values[a.ID].needReg)
                                    {
                                        continue;
                                    }

                                    s.addUse(a.ID, int32(i), v.Pos);
                                    regValLiveSet.add(a.ID);

                                }

                                a = a__prev3;
                            }
                        }


                        i = i__prev2;
                    }
                    if (s.f.pass.debug > regDebug)
                    {
                        fmt.Printf("use distances for %s\n", b);
                        {
                            var i__prev2 = i;

                            foreach (var (__i) in s.values)
                            {
                                i = __i;
                                var vi = _addr_s.values[i];
                                var u = vi.uses;
                                if (u == null)
                                {
                                    continue;
                                }

                                fmt.Printf("  v%d:", i);
                                while (u != null)
                                {
                                    fmt.Printf(" %d", u.dist);
                                    u = u.next;
                                }

                                fmt.Println();

                            }

                            i = i__prev2;
                        }
                    } 

                    // Make a copy of the block schedule so we can generate a new one in place.
                    // We make a separate copy for phis and regular values.
                    long nphi = 0L;
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            if (v.Op != OpPhi)
                            {
                                break;
                            }

                            nphi++;

                        }

                        v = v__prev2;
                    }

                    phis = append(phis[..0L], b.Values[..nphi]);
                    oldSched = append(oldSched[..0L], b.Values[nphi..]);
                    b.Values = b.Values[..0L]; 

                    // Initialize start state of block.
                    if (b == f.Entry)
                    { 
                        // Regalloc state is empty to start.
                        if (nphi > 0L)
                        {
                            f.Fatalf("phis in entry block");
                        }

                    }
                    else if (len(b.Preds) == 1L)
                    { 
                        // Start regalloc state with the end state of the previous block.
                        s.setState(s.endRegs[b.Preds[0L].b.ID]);
                        if (nphi > 0L)
                        {
                            f.Fatalf("phis in single-predecessor block");
                        } 
                        // Drop any values which are no longer live.
                        // This may happen because at the end of p, a value may be
                        // live but only used by some other successor of p.
                        {
                            var r__prev2 = r;

                            for (var r = register(0L); r < s.numRegs; r++)
                            {
                                v = s.regs[r].v;
                                if (v != null && !regValLiveSet.contains(v.ID))
                                {
                                    s.freeReg(r);
                                }

                            }
                    else


                            r = r__prev2;
                        }

                    }                    { 
                        // This is the complicated case. We have more than one predecessor,
                        // which means we may have Phi ops.

                        // Start with the final register state of the primary predecessor
                        var idx = s.primary[b.ID];
                        if (idx < 0L)
                        {
                            f.Fatalf("block with no primary predecessor %s", b);
                        }

                        var p = b.Preds[idx].b;
                        s.setState(s.endRegs[p.ID]);

                        if (s.f.pass.debug > regDebug)
                        {
                            fmt.Printf("starting merge block %s with end state of %s:\n", b, p);
                            {
                                var x__prev2 = x;

                                foreach (var (_, __x) in s.endRegs[p.ID])
                                {
                                    x = __x;
                                    fmt.Printf("  %s: orig:%s cache:%s\n", _addr_s.registers[x.r], x.v, x.c);
                                }

                                x = x__prev2;
                            }
                        } 

                        // Decide on registers for phi ops. Use the registers determined
                        // by the primary predecessor if we can.
                        // TODO: pick best of (already processed) predecessors?
                        // Majority vote? Deepest nesting level?
                        phiRegs = phiRegs[..0L];
                        regMask phiUsed = default;

                        {
                            var v__prev2 = v;

                            foreach (var (_, __v) in phis)
                            {
                                v = __v;
                                if (!s.values[v.ID].needReg)
                                {
                                    phiRegs = append(phiRegs, noRegister);
                                    continue;
                                }

                                var a = v.Args[idx]; 
                                // Some instructions target not-allocatable registers.
                                // They're not suitable for further (phi-function) allocation.
                                var m = s.values[a.ID].regs & ~phiUsed & s.allocatable;
                                if (m != 0L)
                                {
                                    r = pickReg(m);
                                    phiUsed |= regMask(1L) << (int)(r);
                                    phiRegs = append(phiRegs, r);
                                }
                                else
                                {
                                    phiRegs = append(phiRegs, noRegister);
                                }

                            } 

                            // Second pass - deallocate all in-register phi inputs.

                            v = v__prev2;
                        }

                        {
                            var i__prev2 = i;
                            var v__prev2 = v;

                            foreach (var (__i, __v) in phis)
                            {
                                i = __i;
                                v = __v;
                                if (!s.values[v.ID].needReg)
                                {
                                    continue;
                                }

                                a = v.Args[idx];
                                r = phiRegs[i];
                                if (r == noRegister)
                                {
                                    continue;
                                }

                                if (regValLiveSet.contains(a.ID))
                                { 
                                    // Input value is still live (it is used by something other than Phi).
                                    // Try to move it around before kicking out, if there is a free register.
                                    // We generate a Copy in the predecessor block and record it. It will be
                                    // deleted later if never used.
                                    //
                                    // Pick a free register. At this point some registers used in the predecessor
                                    // block may have been deallocated. Those are the ones used for Phis. Exclude
                                    // them (and they are not going to be helpful anyway).
                                    m = s.compatRegs(a.Type) & ~s.used & ~phiUsed;
                                    if (m != 0L && !s.values[a.ID].rematerializeable && countRegs(s.values[a.ID].regs) == 1L)
                                    {
                                        var r2 = pickReg(m);
                                        var c = p.NewValue1(a.Pos, OpCopy, a.Type, s.regs[r].c);
                                        s.copies[c] = false;
                                        if (s.f.pass.debug > regDebug)
                                        {
                                            fmt.Printf("copy %s to %s : %s\n", a, c, _addr_s.registers[r2]);
                                        }

                                        s.setOrig(c, a);
                                        s.assignReg(r2, a, c);
                                        s.endRegs[p.ID] = append(s.endRegs[p.ID], new endReg(r2,a,c));

                                    }

                                }

                                s.freeReg(r);

                            } 

                            // Copy phi ops into new schedule.

                            i = i__prev2;
                            v = v__prev2;
                        }

                        b.Values = append(b.Values, phis); 

                        // Third pass - pick registers for phis whose inputs
                        // were not in a register.
                        {
                            var i__prev2 = i;
                            var v__prev2 = v;

                            foreach (var (__i, __v) in phis)
                            {
                                i = __i;
                                v = __v;
                                if (!s.values[v.ID].needReg)
                                {
                                    continue;
                                }

                                if (phiRegs[i] != noRegister)
                                {
                                    continue;
                                }

                                if (s.f.Config.use387 && v.Type.IsFloat())
                                {
                                    continue; // 387 can't handle floats in registers between blocks
                                }

                                m = s.compatRegs(v.Type) & ~phiUsed & ~s.used;
                                if (m != 0L)
                                {
                                    r = pickReg(m);
                                    phiRegs[i] = r;
                                    phiUsed |= regMask(1L) << (int)(r);
                                }

                            } 

                            // Set registers for phis. Add phi spill code.

                            i = i__prev2;
                            v = v__prev2;
                        }

                        {
                            var i__prev2 = i;
                            var v__prev2 = v;

                            foreach (var (__i, __v) in phis)
                            {
                                i = __i;
                                v = __v;
                                if (!s.values[v.ID].needReg)
                                {
                                    continue;
                                }

                                r = phiRegs[i];
                                if (r == noRegister)
                                { 
                                    // stack-based phi
                                    // Spills will be inserted in all the predecessors below.
                                    s.values[v.ID].spill = v; // v starts life spilled
                                    continue;

                                } 
                                // register-based phi
                                s.assignReg(r, v, v);

                            } 

                            // Deallocate any values which are no longer live. Phis are excluded.

                            i = i__prev2;
                            v = v__prev2;
                        }

                        {
                            var r__prev2 = r;

                            for (r = register(0L); r < s.numRegs; r++)
                            {
                                if (phiUsed >> (int)(r) & 1L != 0L)
                                {
                                    continue;
                                }

                                v = s.regs[r].v;
                                if (v != null && !regValLiveSet.contains(v.ID))
                                {
                                    s.freeReg(r);
                                }

                            } 

                            // Save the starting state for use by merge edges.
                            // We append to a stack allocated variable that we'll
                            // later copy into s.startRegs in one fell swoop, to save
                            // on allocations.


                            r = r__prev2;
                        } 

                        // Save the starting state for use by merge edges.
                        // We append to a stack allocated variable that we'll
                        // later copy into s.startRegs in one fell swoop, to save
                        // on allocations.
                        var regList = make_slice<startReg>(0L, 32L);
                        {
                            var r__prev2 = r;

                            for (r = register(0L); r < s.numRegs; r++)
                            {
                                v = s.regs[r].v;
                                if (v == null)
                                {
                                    continue;
                                }

                                if (phiUsed >> (int)(r) & 1L != 0L)
                                { 
                                    // Skip registers that phis used, we'll handle those
                                    // specially during merge edge processing.
                                    continue;

                                }

                                regList = append(regList, new startReg(r,v,s.regs[r].c,s.values[v.ID].uses.pos));

                            }


                            r = r__prev2;
                        }
                        s.startRegs[b.ID] = make_slice<startReg>(len(regList));
                        copy(s.startRegs[b.ID], regList);

                        if (s.f.pass.debug > regDebug)
                        {
                            fmt.Printf("after phis\n");
                            {
                                var x__prev2 = x;

                                foreach (var (_, __x) in s.startRegs[b.ID])
                                {
                                    x = __x;
                                    fmt.Printf("  %s: v%d\n", _addr_s.registers[x.r], x.v.ID);
                                }

                                x = x__prev2;
                            }
                        }

                    } 

                    // Allocate space to record the desired registers for each value.
                    {
                        var l__prev1 = l;

                        var l = len(oldSched);

                        if (cap(dinfo) < l)
                        {
                            dinfo = make_slice<dentry>(l);
                        }
                        else
                        {
                            dinfo = dinfo[..l];
                            {
                                var i__prev2 = i;

                                foreach (var (__i) in dinfo)
                                {
                                    i = __i;
                                    dinfo[i] = new dentry();
                                }

                                i = i__prev2;
                            }
                        } 

                        // Load static desired register info at the end of the block.

                        l = l__prev1;

                    } 

                    // Load static desired register info at the end of the block.
                    desired.copy(_addr_s.desired[b.ID]); 

                    // Check actual assigned registers at the start of the next block(s).
                    // Dynamically assigned registers will trump the static
                    // desired registers computed during liveness analysis.
                    // Note that we do this phase after startRegs is set above, so that
                    // we get the right behavior for a block which branches to itself.
                    {
                        var e__prev2 = e;

                        foreach (var (_, __e) in b.Succs)
                        {
                            e = __e;
                            var succ = e.b; 
                            // TODO: prioritize likely successor?
                            {
                                var x__prev3 = x;

                                foreach (var (_, __x) in s.startRegs[succ.ID])
                                {
                                    x = __x;
                                    desired.add(x.v.ID, x.r);
                                } 
                                // Process phi ops in succ.

                                x = x__prev3;
                            }

                            var pidx = e.i;
                            {
                                var v__prev3 = v;

                                foreach (var (_, __v) in succ.Values)
                                {
                                    v = __v;
                                    if (v.Op != OpPhi)
                                    {
                                        break;
                                    }

                                    if (!s.values[v.ID].needReg)
                                    {
                                        continue;
                                    }

                                    ptr<Register> (rp, ok) = s.f.getHome(v.ID)._<ptr<Register>>();
                                    if (!ok)
                                    {
                                        continue;
                                    }

                                    desired.add(v.Args[pidx].ID, register(rp.num));

                                }

                                v = v__prev3;
                            }
                        } 
                        // Walk values backwards computing desired register info.
                        // See computeLive for more comments.

                        e = e__prev2;
                    }

                    {
                        var i__prev2 = i;

                        for (i = len(oldSched) - 1L; i >= 0L; i--)
                        {
                            v = oldSched[i];
                            var prefs = desired.remove(v.ID);
                            var regspec = s.regspec(v.Op);
                            desired.clobber(regspec.clobbers);
                            {
                                var j__prev3 = j;

                                foreach (var (_, __j) in regspec.inputs)
                                {
                                    j = __j;
                                    if (countRegs(j.regs) != 1L)
                                    {
                                        continue;
                                    }

                                    desired.clobber(j.regs);
                                    desired.add(v.Args[j.idx].ID, pickReg(j.regs));

                                }

                                j = j__prev3;
                            }

                            if (opcodeTable[v.Op].resultInArg0)
                            {
                                if (opcodeTable[v.Op].commutative)
                                {
                                    desired.addList(v.Args[1L].ID, prefs);
                                }

                                desired.addList(v.Args[0L].ID, prefs);

                            } 
                            // Save desired registers for this value.
                            dinfo[i].@out = prefs;
                            {
                                var j__prev3 = j;
                                var a__prev3 = a;

                                foreach (var (__j, __a) in v.Args)
                                {
                                    j = __j;
                                    a = __a;
                                    if (j >= len(dinfo[i].@in))
                                    {
                                        break;
                                    }

                                    dinfo[i].@in[j] = desired.get(a.ID);

                                }

                                j = j__prev3;
                                a = a__prev3;
                            }
                        } 

                        // Process all the non-phi values.


                        i = i__prev2;
                    } 

                    // Process all the non-phi values.
                    {
                        var idx__prev2 = idx;
                        var v__prev2 = v;

                        foreach (var (__idx, __v) in oldSched)
                        {
                            idx = __idx;
                            v = __v;
                            if (s.f.pass.debug > regDebug)
                            {
                                fmt.Printf("  processing %s\n", v.LongString());
                            }

                            regspec = s.regspec(v.Op);
                            if (v.Op == OpPhi)
                            {
                                f.Fatalf("phi %s not at start of block", v);
                            }

                            if (v.Op == OpSP)
                            {
                                s.assignReg(s.SPReg, v, v);
                                b.Values = append(b.Values, v);
                                s.advanceUses(v);
                                s.sp = v.ID;
                                continue;
                            }

                            if (v.Op == OpSB)
                            {
                                s.assignReg(s.SBReg, v, v);
                                b.Values = append(b.Values, v);
                                s.advanceUses(v);
                                s.sb = v.ID;
                                continue;
                            }

                            if (v.Op == OpSelect0 || v.Op == OpSelect1)
                            {
                                if (s.values[v.ID].needReg)
                                {
                                    i = 0L;
                                    if (v.Op == OpSelect1)
                                    {
                                        i = 1L;
                                    }

                                    s.assignReg(register(s.f.getHome(v.Args[0L].ID)._<LocPair>()[i]._<ptr<Register>>().num), v, v);

                                }

                                b.Values = append(b.Values, v);
                                s.advanceUses(v);
                                goto issueSpill;

                            }

                            if (v.Op == OpGetG && s.f.Config.hasGReg)
                            { 
                                // use hardware g register
                                if (s.regs[s.GReg].v != null)
                                {
                                    s.freeReg(s.GReg); // kick out the old value
                                }

                                s.assignReg(s.GReg, v, v);
                                b.Values = append(b.Values, v);
                                s.advanceUses(v);
                                goto issueSpill;

                            }

                            if (v.Op == OpArg)
                            { 
                                // Args are "pre-spilled" values. We don't allocate
                                // any register here. We just set up the spill pointer to
                                // point at itself and any later user will restore it to use it.
                                s.values[v.ID].spill = v;
                                b.Values = append(b.Values, v);
                                s.advanceUses(v);
                                continue;

                            }

                            if (v.Op == OpKeepAlive)
                            { 
                                // Make sure the argument to v is still live here.
                                s.advanceUses(v);
                                a = v.Args[0L];
                                vi = _addr_s.values[a.ID];
                                if (vi.regs == 0L && !vi.rematerializeable)
                                { 
                                    // Use the spill location.
                                    // This forces later liveness analysis to make the
                                    // value live at this point.
                                    v.SetArg(0L, s.makeSpill(a, b));

                                }                                {
                                    GCNode (_, ok) = a.Aux._<GCNode>();


                                    else if (ok && vi.rematerializeable)
                                    { 
                                        // Rematerializeable value with a gc.Node. This is the address of
                                        // a stack object (e.g. an LEAQ). Keep the object live.
                                        // Change it to VarLive, which is what plive expects for locals.
                                        v.Op = OpVarLive;
                                        v.SetArgs1(v.Args[1L]);
                                        v.Aux = a.Aux;

                                    }
                                    else
                                    { 
                                        // In-register and rematerializeable values are already live.
                                        // These are typically rematerializeable constants like nil,
                                        // or values of a variable that were modified since the last call.
                                        v.Op = OpCopy;
                                        v.SetArgs1(v.Args[1L]);

                                    }

                                }

                                b.Values = append(b.Values, v);
                                continue;

                            }

                            if (len(regspec.inputs) == 0L && len(regspec.outputs) == 0L)
                            { 
                                // No register allocation required (or none specified yet)
                                s.freeRegs(regspec.clobbers);
                                b.Values = append(b.Values, v);
                                s.advanceUses(v);
                                continue;

                            }

                            if (s.values[v.ID].rematerializeable)
                            { 
                                // Value is rematerializeable, don't issue it here.
                                // It will get issued just before each use (see
                                // allocValueToReg).
                                {
                                    var a__prev3 = a;

                                    foreach (var (_, __a) in v.Args)
                                    {
                                        a = __a;
                                        a.Uses--;
                                    }

                                    a = a__prev3;
                                }

                                s.advanceUses(v);
                                continue;

                            }

                            if (s.f.pass.debug > regDebug)
                            {
                                fmt.Printf("value %s\n", v.LongString());
                                fmt.Printf("  out:");
                                {
                                    var r__prev3 = r;

                                    foreach (var (_, __r) in dinfo[idx].@out)
                                    {
                                        r = __r;
                                        if (r != noRegister)
                                        {
                                            fmt.Printf(" %s", _addr_s.registers[r]);
                                        }

                                    }

                                    r = r__prev3;
                                }

                                fmt.Println();
                                {
                                    var i__prev3 = i;

                                    for (i = 0L; i < len(v.Args) && i < 3L; i++)
                                    {
                                        fmt.Printf("  in%d:", i);
                                        {
                                            var r__prev4 = r;

                                            foreach (var (_, __r) in dinfo[idx].@in[i])
                                            {
                                                r = __r;
                                                if (r != noRegister)
                                                {
                                                    fmt.Printf(" %s", _addr_s.registers[r]);
                                                }

                                            }

                                            r = r__prev4;
                                        }

                                        fmt.Println();

                                    }


                                    i = i__prev3;
                                }

                            } 

                            // Move arguments to registers. Process in an ordering defined
                            // by the register specification (most constrained first).
                            args = append(args[..0L], v.Args);
                            {
                                var i__prev3 = i;

                                foreach (var (_, __i) in regspec.inputs)
                                {
                                    i = __i;
                                    var mask = i.regs;
                                    if (mask & s.values[args[i.idx].ID].regs == 0L)
                                    { 
                                        // Need a new register for the input.
                                        mask &= s.allocatable;
                                        mask &= s.nospill; 
                                        // Used desired register if available.
                                        if (i.idx < 3L)
                                        {
                                            {
                                                var r__prev4 = r;

                                                foreach (var (_, __r) in dinfo[idx].@in[i.idx])
                                                {
                                                    r = __r;
                                                    if (r != noRegister && (mask & ~s.used) >> (int)(r) & 1L != 0L)
                                                    { 
                                                        // Desired register is allowed and unused.
                                                        mask = regMask(1L) << (int)(r);
                                                        break;

                                                    }

                                                }

                                                r = r__prev4;
                                            }
                                        } 
                                        // Avoid registers we're saving for other values.
                                        if (mask & ~desired.avoid != 0L)
                                        {
                                            mask &= desired.avoid;
                                        }

                                    }

                                    args[i.idx] = s.allocValToReg(args[i.idx], mask, true, v.Pos);

                                } 

                                // If the output clobbers the input register, make sure we have
                                // at least two copies of the input register so we don't
                                // have to reload the value from the spill location.

                                i = i__prev3;
                            }

                            if (opcodeTable[v.Op].resultInArg0)
                            {
                                m = default;
                                if (!s.liveAfterCurrentInstruction(v.Args[0L]))
                                { 
                                    // arg0 is dead.  We can clobber its register.
                                    goto ok;

                                }

                                if (opcodeTable[v.Op].commutative && !s.liveAfterCurrentInstruction(v.Args[1L]))
                                {
                                    args[0L] = args[1L];
                                    args[1L] = args[0L];
                                    goto ok;

                                }

                                if (s.values[v.Args[0L].ID].rematerializeable)
                                { 
                                    // We can rematerialize the input, don't worry about clobbering it.
                                    goto ok;

                                }

                                if (opcodeTable[v.Op].commutative && s.values[v.Args[1L].ID].rematerializeable)
                                {
                                    args[0L] = args[1L];
                                    args[1L] = args[0L];
                                    goto ok;

                                }

                                if (countRegs(s.values[v.Args[0L].ID].regs) >= 2L)
                                { 
                                    // we have at least 2 copies of arg0.  We can afford to clobber one.
                                    goto ok;

                                }

                                if (opcodeTable[v.Op].commutative && countRegs(s.values[v.Args[1L].ID].regs) >= 2L)
                                {
                                    args[0L] = args[1L];
                                    args[1L] = args[0L];
                                    goto ok;

                                } 

                                // We can't overwrite arg0 (or arg1, if commutative).  So we
                                // need to make a copy of an input so we have a register we can modify.

                                // Possible new registers to copy into.
                                m = s.compatRegs(v.Args[0L].Type) & ~s.used;
                                if (m == 0L)
                                { 
                                    // No free registers.  In this case we'll just clobber
                                    // an input and future uses of that input must use a restore.
                                    // TODO(khr): We should really do this like allocReg does it,
                                    // spilling the value with the most distant next use.
                                    goto ok;

                                } 

                                // Try to move an input to the desired output.
                                {
                                    var r__prev3 = r;

                                    foreach (var (_, __r) in dinfo[idx].@out)
                                    {
                                        r = __r;
                                        if (r != noRegister && m >> (int)(r) & 1L != 0L)
                                        {
                                            m = regMask(1L) << (int)(r);
                                            args[0L] = s.allocValToReg(v.Args[0L], m, true, v.Pos); 
                                            // Note: we update args[0] so the instruction will
                                            // use the register copy we just made.
                                            goto ok;

                                        }

                                    } 
                                    // Try to copy input to its desired location & use its old
                                    // location as the result register.

                                    r = r__prev3;
                                }

                                {
                                    var r__prev3 = r;

                                    foreach (var (_, __r) in dinfo[idx].@in[0L])
                                    {
                                        r = __r;
                                        if (r != noRegister && m >> (int)(r) & 1L != 0L)
                                        {
                                            m = regMask(1L) << (int)(r);
                                            c = s.allocValToReg(v.Args[0L], m, true, v.Pos);
                                            s.copies[c] = false; 
                                            // Note: no update to args[0] so the instruction will
                                            // use the original copy.
                                            goto ok;

                                        }

                                    }

                                    r = r__prev3;
                                }

                                if (opcodeTable[v.Op].commutative)
                                {
                                    {
                                        var r__prev3 = r;

                                        foreach (var (_, __r) in dinfo[idx].@in[1L])
                                        {
                                            r = __r;
                                            if (r != noRegister && m >> (int)(r) & 1L != 0L)
                                            {
                                                m = regMask(1L) << (int)(r);
                                                c = s.allocValToReg(v.Args[1L], m, true, v.Pos);
                                                s.copies[c] = false;
                                                args[0L] = args[1L];
                                                args[1L] = args[0L];
                                                goto ok;

                                            }

                                        }

                                        r = r__prev3;
                                    }
                                } 
                                // Avoid future fixed uses if we can.
                                if (m & ~desired.avoid != 0L)
                                {
                                    m &= desired.avoid;
                                } 
                                // Save input 0 to a new register so we can clobber it.
                                c = s.allocValToReg(v.Args[0L], m, true, v.Pos);
                                s.copies[c] = false;

                            }

ok: 

                            // Dump any registers which will be clobbered
                            if (!opcodeTable[v.Op].resultNotInArgs)
                            {
                                s.tmpused = s.nospill;
                                s.nospill = 0L;
                                s.advanceUses(v); // frees any registers holding args that are no longer live
                            } 

                            // Dump any registers which will be clobbered
                            s.freeRegs(regspec.clobbers);
                            s.tmpused |= regspec.clobbers; 

                            // Pick registers for outputs.
                            {
                                array<register> outRegs = new array<register>(new register[] { noRegister, noRegister });
                                regMask used = default;
                                foreach (var (_, out) in regspec.outputs)
                                {
                                    mask = @out.regs & s.allocatable & ~used;
                                    if (mask == 0L)
                                    {
                                        continue;
                                    }

                                    if (opcodeTable[v.Op].resultInArg0 && @out.idx == 0L)
                                    {
                                        if (!opcodeTable[v.Op].commutative)
                                        { 
                                            // Output must use the same register as input 0.
                                            r = register(s.f.getHome(args[0L].ID)._<ptr<Register>>().num);
                                            mask = regMask(1L) << (int)(r);

                                        }
                                        else
                                        { 
                                            // Output must use the same register as input 0 or 1.
                                            var r0 = register(s.f.getHome(args[0L].ID)._<ptr<Register>>().num);
                                            var r1 = register(s.f.getHome(args[1L].ID)._<ptr<Register>>().num); 
                                            // Check r0 and r1 for desired output register.
                                            var found = false;
                                            {
                                                var r__prev4 = r;

                                                foreach (var (_, __r) in dinfo[idx].@out)
                                                {
                                                    r = __r;
                                                    if ((r == r0 || r == r1) && (mask & ~s.used) >> (int)(r) & 1L != 0L)
                                                    {
                                                        mask = regMask(1L) << (int)(r);
                                                        found = true;
                                                        if (r == r1)
                                                        {
                                                            args[0L] = args[1L];
                                                            args[1L] = args[0L];

                                                        }

                                                        break;

                                                    }

                                                }

                                                r = r__prev4;
                                            }

                                            if (!found)
                                            { 
                                                // Neither are desired, pick r0.
                                                mask = regMask(1L) << (int)(r0);

                                            }

                                        }

                                    }

                                    {
                                        var r__prev4 = r;

                                        foreach (var (_, __r) in dinfo[idx].@out)
                                        {
                                            r = __r;
                                            if (r != noRegister && (mask & ~s.used) >> (int)(r) & 1L != 0L)
                                            { 
                                                // Desired register is allowed and unused.
                                                mask = regMask(1L) << (int)(r);
                                                break;

                                            }

                                        } 
                                        // Avoid registers we're saving for other values.

                                        r = r__prev4;
                                    }

                                    if (mask & ~desired.avoid & ~s.nospill != 0L)
                                    {
                                        mask &= desired.avoid;
                                    }

                                    r = s.allocReg(mask, v);
                                    outRegs[@out.idx] = r;
                                    used |= regMask(1L) << (int)(r);
                                    s.tmpused |= regMask(1L) << (int)(r);

                                } 
                                // Record register choices
                                if (v.Type.IsTuple())
                                {
                                    LocPair outLocs = default;
                                    {
                                        var r__prev2 = r;

                                        r = outRegs[0L];

                                        if (r != noRegister)
                                        {
                                            outLocs[0L] = _addr_s.registers[r];
                                        }

                                        r = r__prev2;

                                    }

                                    {
                                        var r__prev2 = r;

                                        r = outRegs[1L];

                                        if (r != noRegister)
                                        {
                                            outLocs[1L] = _addr_s.registers[r];
                                        }

                                        r = r__prev2;

                                    }

                                    s.f.setHome(v, outLocs); 
                                    // Note that subsequent SelectX instructions will do the assignReg calls.
                                }
                                else
                                {
                                    {
                                        var r__prev2 = r;

                                        r = outRegs[0L];

                                        if (r != noRegister)
                                        {
                                            s.assignReg(r, v, v);
                                        }

                                        r = r__prev2;

                                    }

                                }

                            } 

                            // deallocate dead args, if we have not done so
                            if (opcodeTable[v.Op].resultNotInArgs)
                            {
                                s.nospill = 0L;
                                s.advanceUses(v); // frees any registers holding args that are no longer live
                            }

                            s.tmpused = 0L; 

                            // Issue the Value itself.
                            {
                                var i__prev3 = i;
                                var a__prev3 = a;

                                foreach (var (__i, __a) in args)
                                {
                                    i = __i;
                                    a = __a;
                                    v.SetArg(i, a); // use register version of arguments
                                }

                                i = i__prev3;
                                a = a__prev3;
                            }

                            b.Values = append(b.Values, v);

issueSpill: 

                            // Load control values into registers.
                            var controls = append(make_slice<ptr<Value>>(0L, 2L), b.ControlValues()); 

                            // Load control values into registers.
                            {
                                var i__prev3 = i;
                                var v__prev3 = v;

                                foreach (var (__i, __v) in b.ControlValues())
                                {
                                    i = __i;
                                    v = __v;
                                    if (!s.values[v.ID].needReg)
                                    {
                                        continue;
                                    }

                                    if (s.f.pass.debug > regDebug)
                                    {
                                        fmt.Printf("  processing control %s\n", v.LongString());
                                    } 
                                    // We assume that a control input can be passed in any
                                    // type-compatible register. If this turns out not to be true,
                                    // we'll need to introduce a regspec for a block's control value.
                                    b.ReplaceControl(i, s.allocValToReg(v, s.compatRegs(v.Type), false, b.Pos));

                                } 

                                // Reduce the uses of the control values once registers have been loaded.
                                // This loop is equivalent to the advanceUses method.

                                i = i__prev3;
                                v = v__prev3;
                            }

                            {
                                var v__prev3 = v;

                                foreach (var (_, __v) in controls)
                                {
                                    v = __v;
                                    vi = _addr_s.values[v.ID];
                                    if (!vi.needReg)
                                    {
                                        continue;
                                    } 
                                    // Remove this use from the uses list.
                                    u = vi.uses;
                                    vi.uses = u.next;
                                    if (u.next == null)
                                    {
                                        s.freeRegs(vi.regs); // value is dead
                                    }

                                    u.next = s.freeUseRecords;
                                    s.freeUseRecords = u;

                                } 

                                // Spill any values that can't live across basic block boundaries.

                                v = v__prev3;
                            }

                            if (s.f.Config.use387)
                            {
                                s.freeRegs(s.f.Config.fpRegMask);
                            } 

                            // If we are approaching a merge point and we are the primary
                            // predecessor of it, find live values that we use soon after
                            // the merge point and promote them to registers now.
                            if (len(b.Succs) == 1L)
                            {
                                if (s.f.Config.hasGReg && s.regs[s.GReg].v != null)
                                {
                                    s.freeReg(s.GReg); // Spill value in G register before any merge.
                                } 
                                // For this to be worthwhile, the loop must have no calls in it.
                                var top = b.Succs[0L].b;
                                var loop = s.loopnest.b2l[top.ID];
                                if (loop == null || loop.header != top || loop.containsUnavoidableCall)
                                {
                                    goto badloop;
                                } 

                                // TODO: sort by distance, pick the closest ones?
                                {
                                    var live__prev3 = live;

                                    foreach (var (_, __live) in s.live[b.ID])
                                    {
                                        live = __live;
                                        if (live.dist >= unlikelyDistance)
                                        { 
                                            // Don't preload anything live after the loop.
                                            continue;

                                        }

                                        var vid = live.ID;
                                        vi = _addr_s.values[vid];
                                        if (vi.regs != 0L)
                                        {
                                            continue;
                                        }

                                        if (vi.rematerializeable)
                                        {
                                            continue;
                                        }

                                        v = s.orig[vid];
                                        if (s.f.Config.use387 && v.Type.IsFloat())
                                        {
                                            continue; // 387 can't handle floats in registers between blocks
                                        }

                                        m = s.compatRegs(v.Type) & ~s.used;
                                        if (m & ~desired.avoid != 0L)
                                        {
                                            m &= desired.avoid;
                                        }

                                        if (m != 0L)
                                        {
                                            s.allocValToReg(v, m, false, b.Pos);
                                        }

                                    }

                                    live = live__prev3;
                                }
                            }

badloop: 

                            // Save end-of-block register state.
                            // First count how many, this cuts allocations in half.
                            long k = 0L;
                            {
                                var r__prev3 = r;

                                for (r = register(0L); r < s.numRegs; r++)
                                {
                                    v = s.regs[r].v;
                                    if (v == null)
                                    {
                                        continue;
                                    }

                                    k++;

                                }


                                r = r__prev3;
                            }
                            regList = make_slice<endReg>(0L, k);
                            {
                                var r__prev3 = r;

                                for (r = register(0L); r < s.numRegs; r++)
                                {
                                    v = s.regs[r].v;
                                    if (v == null)
                                    {
                                        continue;
                                    }

                                    regList = append(regList, new endReg(r,v,s.regs[r].c));

                                }


                                r = r__prev3;
                            }
                            s.endRegs[b.ID] = regList;

                            if (checkEnabled)
                            {
                                regValLiveSet.clear();
                                {
                                    var x__prev3 = x;

                                    foreach (var (_, __x) in s.live[b.ID])
                                    {
                                        x = __x;
                                        regValLiveSet.add(x.ID);
                                    }

                                    x = x__prev3;
                                }

                                {
                                    var r__prev3 = r;

                                    for (r = register(0L); r < s.numRegs; r++)
                                    {
                                        v = s.regs[r].v;
                                        if (v == null)
                                        {
                                            continue;
                                        }

                                        if (!regValLiveSet.contains(v.ID))
                                        {
                                            s.f.Fatalf("val %s is in reg but not live at end of %s", v, b);
                                        }

                                    }


                                    r = r__prev3;
                                }

                            } 

                            // If a value is live at the end of the block and
                            // isn't in a register, generate a use for the spill location.
                            // We need to remember this information so that
                            // the liveness analysis in stackalloc is correct.
                            {
                                var e__prev3 = e;

                                foreach (var (_, __e) in s.live[b.ID])
                                {
                                    e = __e;
                                    vi = _addr_s.values[e.ID];
                                    if (vi.regs != 0L)
                                    { 
                                        // in a register, we'll use that source for the merge.
                                        continue;

                                    }

                                    if (vi.rematerializeable)
                                    { 
                                        // we'll rematerialize during the merge.
                                        continue;

                                    } 
                                    //fmt.Printf("live-at-end spill for %s at %s\n", s.orig[e.ID], b)
                                    var spill = s.makeSpill(s.orig[e.ID], b);
                                    s.spillLive[b.ID] = append(s.spillLive[b.ID], spill.ID);

                                } 

                                // Clear any final uses.
                                // All that is left should be the pseudo-uses added for values which
                                // are live at the end of b.

                                e = e__prev3;
                            }

                            {
                                var e__prev3 = e;

                                foreach (var (_, __e) in s.live[b.ID])
                                {
                                    e = __e;
                                    u = s.values[e.ID].uses;
                                    if (u == null)
                                    {
                                        f.Fatalf("live at end, no uses v%d", e.ID);
                                    }

                                    if (u.next != null)
                                    {
                                        f.Fatalf("live at end, too many uses v%d", e.ID);
                                    }

                                    s.values[e.ID].uses = null;
                                    u.next = s.freeUseRecords;
                                    s.freeUseRecords = u;

                                }

                                e = e__prev3;
                            }
                        } 

                        // Decide where the spills we generated will go.

                        idx = idx__prev2;
                        v = v__prev2;
                    }

                    s.placeSpills(); 

                    // Anything that didn't get a register gets a stack location here.
                    // (StoreReg, stack-based phis, inputs, ...)
                    var stacklive = stackalloc(s.f, s.spillLive); 

                    // Fix up all merge edges.
                    s.shuffle(stacklive); 

                    // Erase any copies we never used.
                    // Also, an unused copy might be the only use of another copy,
                    // so continue erasing until we reach a fixed point.
                    while (true)
                    {
                        var progress = false;
                        {
                            var c__prev3 = c;
                            regMask used__prev3 = used;

                            foreach (var (__c, __used) in s.copies)
                            {
                                c = __c;
                                used = __used;
                                if (!used && c.Uses == 0L)
                                {
                                    if (s.f.pass.debug > regDebug)
                                    {
                                        fmt.Printf("delete copied value %s\n", c.LongString());
                                    }

                                    c.RemoveArg(0L);
                                    f.freeValue(c);
                                    delete(s.copies, c);
                                    progress = true;

                                }

                            }

                            c = c__prev3;
                            used = used__prev3;
                        }

                        if (!progress)
                        {
                            break;
                        }

                    }


                    {
                        var b__prev2 = b;

                        foreach (var (_, __b) in s.visitOrder)
                        {
                            b = __b;
                            i = 0L;
                            {
                                var v__prev3 = v;

                                foreach (var (_, __v) in b.Values)
                                {
                                    v = __v;
                                    if (v.Op == OpInvalid)
                                    {
                                        continue;
                                    }

                                    b.Values[i] = v;
                                    i++;

                                }

                                v = v__prev3;
                            }

                            b.Values = b.Values[..i];

                        }

                        b = b__prev2;
                    }
                }

                b = b__prev1;
            }

            (s * regAllocState);

            placeSpills();

            {
                var f = s.f; 

                // Precompute some useful info.
                phiRegs = make_slice<regMask>(f.NumBlocks());
                {
                    var b__prev1 = b;

                    foreach (var (_, __b) in s.visitOrder)
                    {
                        b = __b;
                        m = default;
                        {
                            var v__prev2 = v;

                            foreach (var (_, __v) in b.Values)
                            {
                                v = __v;
                                if (v.Op != OpPhi)
                                {
                                    break;
                                }

                                {
                                    var r__prev1 = r;

                                    ptr<Register> (r, ok) = f.getHome(v.ID)._<ptr<Register>>();

                                    if (ok)
                                    {
                                        m |= regMask(1L) << (int)(uint(r.num));
                                    }

                                    r = r__prev1;

                                }

                            }

                            v = v__prev2;
                        }

                        phiRegs[b.ID] = m;

                    } 

                    // Start maps block IDs to the list of spills
                    // that go at the start of the block (but after any phis).

                    b = b__prev1;
                }

                map start = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ID, slice<ptr<Value>>>{}; 
                // After maps value IDs to the list of spills
                // that go immediately after that value ID.
                map after = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ID, slice<ptr<Value>>>{};

                {
                    var i__prev1 = i;

                    foreach (var (__i) in s.values)
                    {
                        i = __i;
                        vi = s.values[i];
                        spill = vi.spill;
                        if (spill == null)
                        {
                            continue;
                        }

                        if (spill.Block != null)
                        { 
                            // Some spills are already fully set up,
                            // like OpArgs and stack-based phis.
                            continue;

                        }

                        v = s.orig[i]; 

                        // Walk down the dominator tree looking for a good place to
                        // put the spill of v.  At the start "best" is the best place
                        // we have found so far.
                        // TODO: find a way to make this O(1) without arbitrary cutoffs.
                        var best = v.Block;
                        var bestArg = v;
                        short bestDepth = default;
                        {
                            var l__prev1 = l;

                            l = s.loopnest.b2l[best.ID];

                            if (l != null)
                            {
                                bestDepth = l.depth;
                            }

                            l = l__prev1;

                        }

                        var b = best;
                        const long maxSpillSearch = (long)100L;

                        {
                            var i__prev2 = i;

                            for (i = 0L; i < maxSpillSearch; i++)
                            { 
                                // Find the child of b in the dominator tree which
                                // dominates all restores.
                                p = b;
                                b = null;
                                {
                                    var c__prev3 = c;

                                    c = s.sdom.Child(p);

                                    while (c != null && i < maxSpillSearch)
                                    {
                                        if (s.sdom[c.ID].entry <= vi.restoreMin && s.sdom[c.ID].exit >= vi.restoreMax)
                                        { 
                                            // c also dominates all restores.  Walk down into c.
                                            b = c;
                                            break;
                                        c = s.sdom.Sibling(c);
                                    i = i + 1L;
                                        }

                                    }


                                    c = c__prev3;
                                }
                                if (b == null)
                                { 
                                    // Ran out of blocks which dominate all restores.
                                    break;

                                }

                                short depth = default;
                                {
                                    var l__prev1 = l;

                                    l = s.loopnest.b2l[b.ID];

                                    if (l != null)
                                    {
                                        depth = l.depth;
                                    }

                                    l = l__prev1;

                                }

                                if (depth > bestDepth)
                                { 
                                    // Don't push the spill into a deeper loop.
                                    continue;

                                } 

                                // If v is in a register at the start of b, we can
                                // place the spill here (after the phis).
                                if (len(b.Preds) == 1L)
                                {
                                    {
                                        var e__prev3 = e;

                                        foreach (var (_, __e) in s.endRegs[b.Preds[0L].b.ID])
                                        {
                                            e = __e;
                                            if (e.v == v)
                                            { 
                                                // Found a better spot for the spill.
                                                best = b;
                                                bestArg = e.c;
                                                bestDepth = depth;
                                                break;

                                            }

                                        }
                                else

                                        e = e__prev3;
                                    }
                                }                                {
                                    {
                                        var e__prev3 = e;

                                        foreach (var (_, __e) in s.startRegs[b.ID])
                                        {
                                            e = __e;
                                            if (e.v == v)
                                            { 
                                                // Found a better spot for the spill.
                                                best = b;
                                                bestArg = e.c;
                                                bestDepth = depth;
                                                break;

                                            }

                                        }

                                        e = e__prev3;
                                    }
                                }

                            } 

                            // Put the spill in the best block we found.


                            i = i__prev2;
                        } 

                        // Put the spill in the best block we found.
                        spill.Block = best;
                        spill.AddArg(bestArg);
                        if (best == v.Block && v.Op != OpPhi)
                        { 
                            // Place immediately after v.
                            after[v.ID] = append(after[v.ID], spill);

                        }
                        else
                        { 
                            // Place at the start of best block.
                            start[best.ID] = append(start[best.ID], spill);

                        }

                    } 

                    // Insert spill instructions into the block schedules.

                    i = i__prev1;
                }

                oldSched = default;
                {
                    var b__prev1 = b;

                    foreach (var (_, __b) in s.visitOrder)
                    {
                        b = __b;
                        nphi = 0L;
                        {
                            var v__prev2 = v;

                            foreach (var (_, __v) in b.Values)
                            {
                                v = __v;
                                if (v.Op != OpPhi)
                                {
                                    break;
                                }

                                nphi++;

                            }

                            v = v__prev2;
                        }

                        oldSched = append(oldSched[..0L], b.Values[nphi..]);
                        b.Values = b.Values[..nphi];
                        b.Values = append(b.Values, start[b.ID]);
                        {
                            var v__prev2 = v;

                            foreach (var (_, __v) in oldSched)
                            {
                                v = __v;
                                b.Values = append(b.Values, v);
                                b.Values = append(b.Values, after[v.ID]);
                            }

                            v = v__prev2;
                        }
                    }

                    b = b__prev1;
                }
            }

            // shuffle fixes up all the merge edges (those going into blocks of indegree > 1).
            (s * regAllocState);

            stacklive;

            {
                edgeState e = default;
                e.s = s;
                e.cache = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ID, slice<ptr<Value>>>{};
                e.contents = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<Location, contentRecord>{};
                if (s.f.pass.debug > regDebug)
                {
                    fmt.Printf("shuffle %s\n", s.f.Name);
                    fmt.Println(s.f.String());
                }

                {
                    var b__prev1 = b;

                    foreach (var (_, __b) in s.visitOrder)
                    {
                        b = __b;
                        if (len(b.Preds) <= 1L)
                        {
                            continue;
                        }

                        e.b = b;
                        {
                            var i__prev2 = i;

                            foreach (var (__i, __edge) in b.Preds)
                            {
                                i = __i;
                                edge = __edge;
                                p = edge.b;
                                e.p = p;
                                e.setup(i, s.endRegs[p.ID], s.startRegs[b.ID], stacklive[p.ID]);
                                e.process();
                            }

                            i = i__prev2;
                        }
                    }

                    b = b__prev1;
                }

                if (s.f.pass.debug > regDebug)
                {
                    fmt.Printf("post shuffle %s\n", s.f.Name);
                    fmt.Println(s.f.String());
                }

            }
            private partial struct edgeState
            {
                public ptr<regAllocState> s;
                public ptr<Block> p; // edge goes from p->b.

// for each pre-regalloc value, a list of equivalent cached values
                public ptr<Block> b; // edge goes from p->b.

// for each pre-regalloc value, a list of equivalent cached values
                public map<ID, slice<ptr<Value>>> cache;
                public slice<ID> cachedVals; // (superset of) keys of the above map, for deterministic iteration

// map from location to the value it contains
                public map<Location, contentRecord> contents; // desired destination locations
                public slice<dstRecord> destinations;
                public slice<dstRecord> extra;
                public regMask usedRegs; // registers currently holding something
                public regMask uniqueRegs; // registers holding the only copy of a value
                public regMask finalRegs; // registers holding final target
                public regMask rematerializeableRegs; // registers that hold rematerializeable values
            }

            private partial struct contentRecord
            {
                public ID vid; // pre-regalloc value
                public ptr<Value> c; // cached value
                public bool final; // this is a satisfied destination
                public src.XPos pos; // source position of use of the value
            }

            private partial struct dstRecord
            {
                public Location loc; // register or stack slot
                public ID vid; // pre-regalloc value it should contain
                public ptr<ptr<Value>> splice; // place to store reference to the generating instruction
                public src.XPos pos; // source position of use of this location
            }

            // setup initializes the edge state for shuffling.
            (e * edgeState);

            idx;

            int <missing '='> srcReg;

            endReg <missing '='> dstReg;

            startReg <missing '='> stacklive;

            {
                if (e.s.f.pass.debug > regDebug)
                {
                    fmt.Printf("edge %s->%s\n", e.p, e.b);
                } 

                // Clear state.
                {
                    var vid__prev1 = vid;

                    foreach (var (_, __vid) in e.cachedVals)
                    {
                        vid = __vid;
                        delete(e.cache, vid);
                    }

                    vid = vid__prev1;
                }

                e.cachedVals = e.cachedVals[..0L];
                {
                    long k__prev1 = k;

                    foreach (var (__k) in e.contents)
                    {
                        k = __k;
                        delete(e.contents, k);
                    }

                    k = k__prev1;
                }

                e.usedRegs = 0L;
                e.uniqueRegs = 0L;
                e.finalRegs = 0L;
                e.rematerializeableRegs = 0L; 

                // Live registers can be sources.
                {
                    var x__prev1 = x;

                    foreach (var (_, __x) in srcReg)
                    {
                        x = __x;
                        e.set(_addr_e.s.registers[x.r], x.v.ID, x.c, false, src.NoXPos); // don't care the position of the source
                    } 
                    // So can all of the spill locations.

                    x = x__prev1;
                }

                foreach (var (_, spillID) in stacklive)
                {
                    v = e.s.orig[spillID];
                    spill = e.s.values[v.ID].spill;
                    if (!e.s.sdom.IsAncestorEq(spill.Block, e.p))
                    { 
                        // Spills were placed that only dominate the uses found
                        // during the first regalloc pass. The edge fixup code
                        // can't use a spill location if the spill doesn't dominate
                        // the edge.
                        // We are guaranteed that if the spill doesn't dominate this edge,
                        // then the value is available in a register (because we called
                        // makeSpill for every value not in a register at the start
                        // of an edge).
                        continue;

                    }

                    e.set(e.s.f.getHome(spillID), v.ID, spill, false, src.NoXPos); // don't care the position of the source
                } 

                // Figure out all the destinations we need.
                var dsts = e.destinations[..0L];
                {
                    var x__prev1 = x;

                    foreach (var (_, __x) in dstReg)
                    {
                        x = __x;
                        dsts = append(dsts, new dstRecord(&e.s.registers[x.r],x.v.ID,nil,x.pos));
                    } 
                    // Phis need their args to end up in a specific location.

                    x = x__prev1;
                }

                {
                    var v__prev1 = v;

                    foreach (var (_, __v) in e.b.Values)
                    {
                        v = __v;
                        if (v.Op != OpPhi)
                        {
                            break;
                        }

                        var loc = e.s.f.getHome(v.ID);
                        if (loc == null)
                        {
                            continue;
                        }

                        dsts = append(dsts, new dstRecord(loc,v.Args[idx].ID,&v.Args[idx],v.Pos));

                    }

                    v = v__prev1;
                }

                e.destinations = dsts;

                if (e.s.f.pass.debug > regDebug)
                {
                    {
                        var vid__prev1 = vid;

                        foreach (var (_, __vid) in e.cachedVals)
                        {
                            vid = __vid;
                            a = e.cache[vid];
                            {
                                var c__prev2 = c;

                                foreach (var (_, __c) in a)
                                {
                                    c = __c;
                                    fmt.Printf("src %s: v%d cache=%s\n", e.s.f.getHome(c.ID), vid, c);
                                }

                                c = c__prev2;
                            }
                        }

                        vid = vid__prev1;
                    }

                    {
                        var d__prev1 = d;

                        foreach (var (_, __d) in e.destinations)
                        {
                            d = __d;
                            fmt.Printf("dst %s: v%d\n", d.loc, d.vid);
                        }

                        d = d__prev1;
                    }
                }

            }

            // process generates code to move all the values to the right destination locations.
            (e * edgeState);

            process();

            {
                dsts = e.destinations; 

                // Process the destinations until they are all satisfied.
                while (len(dsts) > 0L)
                {
                    i = 0L;
                    {
                        var d__prev2 = d;

                        foreach (var (_, __d) in dsts)
                        {
                            d = __d;
                            if (!e.processDest(d.loc, d.vid, d.splice, d.pos))
                            { 
                                // Failed - save for next iteration.
                                dsts[i] = d;
                                i++;

                            }

                        }

                        d = d__prev2;
                    }

                    if (i < len(dsts))
                    { 
                        // Made some progress. Go around again.
                        dsts = dsts[..i]; 

                        // Append any extras destinations we generated.
                        dsts = append(dsts, e.extra);
                        e.extra = e.extra[..0L];
                        continue;

                    } 

                    // We made no progress. That means that any
                    // remaining unsatisfied moves are in simple cycles.
                    // For example, A -> B -> C -> D -> A.
                    //   A ----> B
                    //   ^       |
                    //   |       |
                    //   |       v
                    //   D <---- C

                    // To break the cycle, we pick an unused register, say R,
                    // and put a copy of B there.
                    //   A ----> B
                    //   ^       |
                    //   |       |
                    //   |       v
                    //   D <---- C <---- R=copyofB
                    // When we resume the outer loop, the A->B move can now proceed,
                    // and eventually the whole cycle completes.

                    // Copy any cycle location to a temp register. This duplicates
                    // one of the cycle entries, allowing the just duplicated value
                    // to be overwritten and the cycle to proceed.
                    var d = dsts[0L];
                    loc = d.loc;
                    vid = e.contents[loc].vid;
                    c = e.contents[loc].c;
                    r = e.findRegFor(c.Type);
                    if (e.s.f.pass.debug > regDebug)
                    {
                        fmt.Printf("breaking cycle with v%d in %s:%s\n", vid, loc, c);
                    }

                    e.erase(r);
                    var pos = d.pos.WithNotStmt();
                    {
                        ptr<Register> (_, isReg) = loc._<ptr<Register>>();

                        if (isReg)
                        {
                            c = e.p.NewValue1(pos, OpCopy, c.Type, c);
                        }
                        else
                        {
                            c = e.p.NewValue1(pos, OpLoadReg, c.Type, c);
                        }

                    }

                    e.set(r, vid, c, false, pos);
                    if (c.Op == OpLoadReg && e.s.isGReg(register(r._<ptr<Register>>().num)))
                    {
                        e.s.f.Fatalf("process.OpLoadReg targeting g: " + c.LongString());
                    }

                }


            }

            // processDest generates code to put value vid into location loc. Returns true
            // if progress was made.
            (e * edgeState);

            loc;

            vid;

            ID, splice * Value.val <missing '='> pos;

            bool;

            {
                pos = pos.WithNotStmt();
                var occupant = e.contents[loc];
                if (occupant.vid == vid)
                { 
                    // Value is already in the correct place.
                    e.contents[loc] = new contentRecord(vid,occupant.c,true,pos);
                    if (splice != null)
                    {
                        ptr<splice>--;
                        splice.val = occupant.c;
                        occupant.c.Uses++;
                    } 
                    // Note: if splice==nil then c will appear dead. This is
                    // non-SSA formed code, so be careful after this pass not to run
                    // deadcode elimination.
                    {
                        (_, ok) = e.s.copies[occupant.c];

                        if (ok)
                        { 
                            // The copy at occupant.c was used to avoid spill.
                            e.s.copies[occupant.c] = true;

                        }

                    }

                    return true;

                } 

                // Check if we're allowed to clobber the destination location.
                if (len(e.cache[occupant.vid]) == 1L && !e.s.values[occupant.vid].rematerializeable)
                { 
                    // We can't overwrite the last copy
                    // of a value that needs to survive.
                    return false;

                } 

                // Copy from a source of v, register preferred.
                v = e.s.orig[vid];
                c = ;
                Location src = default;
                if (e.s.f.pass.debug > regDebug)
                {
                    fmt.Printf("moving v%d to %s\n", vid, loc);
                    fmt.Printf("sources of v%d:", vid);
                }

                foreach (var (_, w) in e.cache[vid])
                {
                    var h = e.s.f.getHome(w.ID);
                    if (e.s.f.pass.debug > regDebug)
                    {
                        fmt.Printf(" %s:%s", h, w);
                    }

                    ptr<Register> (_, isreg) = h._<ptr<Register>>();
                    if (src == null || isreg)
                    {
                        c = w;
                        src = h;
                    }

                }
                if (e.s.f.pass.debug > regDebug)
                {
                    if (src != null)
                    {
                        fmt.Printf(" [use %s]\n", src);
                    }
                    else
                    {
                        fmt.Printf(" [no source]\n");
                    }

                }

                ptr<Register> (_, dstReg) = loc._<ptr<Register>>(); 

                // Pre-clobber destination. This avoids the
                // following situation:
                //   - v is currently held in R0 and stacktmp0.
                //   - We want to copy stacktmp1 to stacktmp0.
                //   - We choose R0 as the temporary register.
                // During the copy, both R0 and stacktmp0 are
                // clobbered, losing both copies of v. Oops!
                // Erasing the destination early means R0 will not
                // be chosen as the temp register, as it will then
                // be the last copy of v.
                e.erase(loc);
                ptr<Value> x;
                if (c == null || e.s.values[vid].rematerializeable)
                {
                    if (!e.s.values[vid].rematerializeable)
                    {
                        e.s.f.Fatalf("can't find source for %s->%s: %s\n", e.p, e.b, v.LongString());
                    }

                    if (dstReg)
                    {
                        x = v.copyInto(e.p);
                    }
                    else
                    { 
                        // Rematerialize into stack slot. Need a free
                        // register to accomplish this.
                        r = e.findRegFor(v.Type);
                        e.erase(r);
                        x = v.copyIntoWithXPos(e.p, pos);
                        e.set(r, vid, x, false, pos); 
                        // Make sure we spill with the size of the slot, not the
                        // size of x (which might be wider due to our dropping
                        // of narrowing conversions).
                        x = e.p.NewValue1(pos, OpStoreReg, loc._<LocalSlot>().Type, x);

                    }

                }
                else
                { 
                    // Emit move from src to dst.
                    ptr<Register> (_, srcReg) = src._<ptr<Register>>();
                    if (srcReg)
                    {
                        if (dstReg)
                        {
                            x = e.p.NewValue1(pos, OpCopy, c.Type, c);
                        }
                        else
                        {
                            x = e.p.NewValue1(pos, OpStoreReg, loc._<LocalSlot>().Type, c);
                        }

                    }
                    else
                    {
                        if (dstReg)
                        {
                            x = e.p.NewValue1(pos, OpLoadReg, c.Type, c);
                        }
                        else
                        { 
                            // mem->mem. Use temp register.
                            r = e.findRegFor(c.Type);
                            e.erase(r);
                            var t = e.p.NewValue1(pos, OpLoadReg, c.Type, c);
                            e.set(r, vid, t, false, pos);
                            x = e.p.NewValue1(pos, OpStoreReg, loc._<LocalSlot>().Type, t);

                        }

                    }

                }

                e.set(loc, vid, x, true, pos);
                if (x.Op == OpLoadReg && e.s.isGReg(register(loc._<ptr<Register>>().num)))
                {
                    e.s.f.Fatalf("processDest.OpLoadReg targeting g: " + x.LongString());
                }

                if (splice != null)
                {
                    ptr<splice>--;
                    splice.val = x;
                    x.Uses++;
                }

                return true;

            }

            // set changes the contents of location loc to hold the given value and its cached representative.
            (e * edgeState);

            loc;

            vid;

            ID, c * Value <missing '='> final;

            pos;

            {
                e.s.f.setHome(c, loc);
                e.contents[loc] = new contentRecord(vid,c,final,pos);
                a = e.cache[vid];
                if (len(a) == 0L)
                {
                    e.cachedVals = append(e.cachedVals, vid);
                }

                a = append(a, c);
                e.cache[vid] = a;
                {
                    var r__prev1 = r;

                    (r, ok) = loc._<ptr<Register>>();

                    if (ok)
                    {
                        if (e.usedRegs & (regMask(1L) << (int)(uint(r.num))) != 0L)
                        {
                            e.s.f.Fatalf("%v is already set (v%d/%v)", r, vid, c);
                        }

                        e.usedRegs |= regMask(1L) << (int)(uint(r.num));
                        if (final)
                        {
                            e.finalRegs |= regMask(1L) << (int)(uint(r.num));
                        }

                        if (len(a) == 1L)
                        {
                            e.uniqueRegs |= regMask(1L) << (int)(uint(r.num));
                        }

                        if (len(a) == 2L)
                        {
                            {
                                var t__prev3 = t;

                                ptr<Register> (t, ok) = e.s.f.getHome(a[0L].ID)._<ptr<Register>>();

                                if (ok)
                                {
                                    e.uniqueRegs &= regMask(1L) << (int)(uint(t.num));
                                }

                                t = t__prev3;

                            }

                        }

                        if (e.s.values[vid].rematerializeable)
                        {
                            e.rematerializeableRegs |= regMask(1L) << (int)(uint(r.num));
                        }

                    }

                    r = r__prev1;

                }

                if (e.s.f.pass.debug > regDebug)
                {
                    fmt.Printf("%s\n", c.LongString());
                    fmt.Printf("v%d now available in %s:%s\n", vid, loc, c);
                }

            }

            // erase removes any user of loc.
            (e * edgeState);

            loc;

            {
                var cr = e.contents[loc];
                if (cr.c == null)
                {
                    return ;
                }

                vid = cr.vid;

                if (cr.final)
                { 
                    // Add a destination to move this value back into place.
                    // Make sure it gets added to the tail of the destination queue
                    // so we make progress on other moves first.
                    e.extra = append(e.extra, new dstRecord(loc,cr.vid,nil,cr.pos));

                } 

                // Remove c from the list of cached values.
                a = e.cache[vid];
                {
                    var i__prev1 = i;
                    var c__prev1 = c;

                    foreach (var (__i, __c) in a)
                    {
                        i = __i;
                        c = __c;
                        if (e.s.f.getHome(c.ID) == loc)
                        {
                            if (e.s.f.pass.debug > regDebug)
                            {
                                fmt.Printf("v%d no longer available in %s:%s\n", vid, loc, c);
                            }

                            a[i] = a[len(a) - 1L];
                            a = a[..len(a) - 1L];
                            break;

                        }

                    }

                    i = i__prev1;
                    c = c__prev1;
                }

                e.cache[vid] = a; 

                // Update register masks.
                {
                    var r__prev1 = r;

                    (r, ok) = loc._<ptr<Register>>();

                    if (ok)
                    {
                        e.usedRegs &= regMask(1L) << (int)(uint(r.num));
                        if (cr.final)
                        {
                            e.finalRegs &= regMask(1L) << (int)(uint(r.num));
                        }

                        e.rematerializeableRegs &= regMask(1L) << (int)(uint(r.num));

                    }

                    r = r__prev1;

                }

                if (len(a) == 1L)
                {
                    {
                        var r__prev2 = r;

                        (r, ok) = e.s.f.getHome(a[0L].ID)._<ptr<Register>>();

                        if (ok)
                        {
                            e.uniqueRegs |= regMask(1L) << (int)(uint(r.num));
                        }

                        r = r__prev2;

                    }

                }

            }

            // findRegFor finds a register we can use to make a temp copy of type typ.
            (Func<ptr<edgeState>, findRegFor>)typ * types.Type;

            Location;

            { 
                // Which registers are possibilities.
                var types = _addr_e.s.f.Config.Types;
                m = e.s.compatRegs(typ); 

                // Pick a register. In priority order:
                // 1) an unused register
                // 2) a non-unique register not holding a final value
                // 3) a non-unique register
                // 4) a register holding a rematerializeable value
                x = m & ~e.usedRegs;
                if (x != 0L)
                {
                    return _addr_e.s.registers[pickReg(x)];
                }

                x = m & ~e.uniqueRegs & ~e.finalRegs;
                if (x != 0L)
                {
                    return _addr_e.s.registers[pickReg(x)];
                }

                x = m & ~e.uniqueRegs;
                if (x != 0L)
                {
                    return _addr_e.s.registers[pickReg(x)];
                }

                x = m & e.rematerializeableRegs;
                if (x != 0L)
                {
                    return _addr_e.s.registers[pickReg(x)];
                } 

                // No register is available.
                // Pick a register to spill.
                {
                    var vid__prev1 = vid;

                    foreach (var (_, __vid) in e.cachedVals)
                    {
                        vid = __vid;
                        a = e.cache[vid];
                        {
                            var c__prev2 = c;

                            foreach (var (_, __c) in a)
                            {
                                c = __c;
                                {
                                    var r__prev1 = r;

                                    (r, ok) = e.s.f.getHome(c.ID)._<ptr<Register>>();

                                    if (ok && m >> (int)(uint(r.num)) & 1L != 0L)
                                    {
                                        if (!c.rematerializeable())
                                        {
                                            x = e.p.NewValue1(c.Pos, OpStoreReg, c.Type, c); 
                                            // Allocate a temp location to spill a register to.
                                            // The type of the slot is immaterial - it will not be live across
                                            // any safepoint. Just use a type big enough to hold any register.
                                            t = new LocalSlot(N:e.s.f.fe.Auto(c.Pos,types.Int64),Type:types.Int64); 
                                            // TODO: reuse these slots. They'll need to be erased first.
                                            e.set(t, vid, x, false, c.Pos);
                                            if (e.s.f.pass.debug > regDebug)
                                            {
                                                fmt.Printf("  SPILL %s->%s %s\n", r, t, x.LongString());
                                            }

                                        } 
                                        // r will now be overwritten by the caller. At some point
                                        // later, the newly saved value will be moved back to its
                                        // final destination in processDest.
                                        return r;

                                    }

                                    r = r__prev1;

                                }

                            }

                            c = c__prev2;
                        }
                    }

                    vid = vid__prev1;
                }

                fmt.Printf("m:%d unique:%d final:%d rematerializable:%d\n", m, e.uniqueRegs, e.finalRegs, e.rematerializeableRegs);
                {
                    var vid__prev1 = vid;

                    foreach (var (_, __vid) in e.cachedVals)
                    {
                        vid = __vid;
                        a = e.cache[vid];
                        {
                            var c__prev2 = c;

                            foreach (var (_, __c) in a)
                            {
                                c = __c;
                                fmt.Printf("v%d: %s %s\n", vid, c, e.s.f.getHome(c.ID));
                            }

                            c = c__prev2;
                        }
                    }

                    vid = vid__prev1;
                }

                e.s.f.Fatalf("can't find empty register on edge %s->%s", e.p, e.b);
                return null;

            }

            // rematerializeable reports whether the register allocator should recompute
            // a value instead of spilling/restoring it.
            (v * Value);

            rematerializeable();

            bool;

            {
                if (!opcodeTable[v.Op].rematerializeable)
                {
                    return false;
                }

                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in v.Args)
                    {
                        a = __a; 
                        // SP and SB (generated by OpSP and OpSB) are always available.
                        if (a.Op != OpSP && a.Op != OpSB)
                        {
                            return false;
                        }

                    }

                    a = a__prev1;
                }

                return true;

            }
            private partial struct liveInfo
            {
                public ID ID; // ID of value
                public int dist; // # of instructions before next use
                public src.XPos pos; // source position of next use
            }

            // computeLive computes a map from block ID to a list of value IDs live at the end
            // of that block. Together with the value ID is a count of how many instructions
            // to the next use of that value. The resulting map is stored in s.live.
            // computeLive also computes the desired register information at the end of each block.
            // This desired register information is stored in s.desired.
            // TODO: this could be quadratic if lots of variables are live across lots of
            // basic blocks. Figure out a way to make this function (or, more precisely, the user
            // of this function) require only linear size & time.
            (s * regAllocState);

            computeLive();

            {
                f = s.f;
                s.live = make_slice<slice<liveInfo>>(f.NumBlocks());
                s.desired = make_slice<desiredState>(f.NumBlocks());
                phis = default;

                var live = f.newSparseMap(f.NumValues());
                defer(f.retSparseMap(live));
                t = f.newSparseMap(f.NumValues());
                defer(f.retSparseMap(t)); 

                // Keep track of which value we want in each register.
                desired = default; 

                // Instead of iterating over f.Blocks, iterate over their postordering.
                // Liveness information flows backward, so starting at the end
                // increases the probability that we will stabilize quickly.
                // TODO: Do a better job yet. Here's one possibility:
                // Calculate the dominator tree and locate all strongly connected components.
                // If a value is live in one block of an SCC, it is live in all.
                // Walk the dominator tree from end to beginning, just once, treating SCC
                // components as single blocks, duplicated calculated liveness information
                // out to all of them.
                var po = f.postorder();
                s.loopnest = f.loopnest();
                s.loopnest.calculateDepths();
                while (true)
                {
                    var changed = false;

                    {
                        var b__prev2 = b;

                        foreach (var (_, __b) in po)
                        {
                            b = __b; 
                            // Start with known live values at the end of the block.
                            // Add len(b.Values) to adjust from end-of-block distance
                            // to beginning-of-block distance.
                            live.clear();
                            {
                                var e__prev3 = e;

                                foreach (var (_, __e) in s.live[b.ID])
                                {
                                    e = __e;
                                    live.set(e.ID, e.dist + int32(len(b.Values)), e.pos);
                                } 

                                // Mark control values as live

                                e = e__prev3;
                            }

                            {
                                var c__prev3 = c;

                                foreach (var (_, __c) in b.ControlValues())
                                {
                                    c = __c;
                                    if (s.values[c.ID].needReg)
                                    {
                                        live.set(c.ID, int32(len(b.Values)), b.Pos);
                                    }

                                } 

                                // Propagate backwards to the start of the block
                                // Assumes Values have been scheduled.

                                c = c__prev3;
                            }

                            phis = phis[..0L];
                            {
                                var i__prev3 = i;

                                for (i = len(b.Values) - 1L; i >= 0L; i--)
                                {
                                    v = b.Values[i];
                                    live.remove(v.ID);
                                    if (v.Op == OpPhi)
                                    { 
                                        // save phi ops for later
                                        phis = append(phis, v);
                                        continue;

                                    }

                                    if (opcodeTable[v.Op].call)
                                    {
                                        c = live.contents();
                                        {
                                            var i__prev4 = i;

                                            foreach (var (__i) in c)
                                            {
                                                i = __i;
                                                c[i].val += unlikelyDistance;
                                            }

                                            i = i__prev4;
                                        }
                                    }

                                    {
                                        var a__prev4 = a;

                                        foreach (var (_, __a) in v.Args)
                                        {
                                            a = __a;
                                            if (s.values[a.ID].needReg)
                                            {
                                                live.set(a.ID, int32(i), v.Pos);
                                            }

                                        }

                                        a = a__prev4;
                                    }
                                } 
                                // Propagate desired registers backwards.


                                i = i__prev3;
                            } 
                            // Propagate desired registers backwards.
                            desired.copy(_addr_s.desired[b.ID]);
                            {
                                var i__prev3 = i;

                                for (i = len(b.Values) - 1L; i >= 0L; i--)
                                {
                                    v = b.Values[i];
                                    prefs = desired.remove(v.ID);
                                    if (v.Op == OpPhi)
                                    { 
                                        // TODO: if v is a phi, save desired register for phi inputs.
                                        // For now, we just drop it and don't propagate
                                        // desired registers back though phi nodes.
                                        continue;

                                    }

                                    regspec = s.regspec(v.Op); 
                                    // Cancel desired registers if they get clobbered.
                                    desired.clobber(regspec.clobbers); 
                                    // Update desired registers if there are any fixed register inputs.
                                    {
                                        var j__prev4 = j;

                                        foreach (var (_, __j) in regspec.inputs)
                                        {
                                            j = __j;
                                            if (countRegs(j.regs) != 1L)
                                            {
                                                continue;
                                            }

                                            desired.clobber(j.regs);
                                            desired.add(v.Args[j.idx].ID, pickReg(j.regs));

                                        } 
                                        // Set desired register of input 0 if this is a 2-operand instruction.

                                        j = j__prev4;
                                    }

                                    if (opcodeTable[v.Op].resultInArg0)
                                    {
                                        if (opcodeTable[v.Op].commutative)
                                        {
                                            desired.addList(v.Args[1L].ID, prefs);
                                        }

                                        desired.addList(v.Args[0L].ID, prefs);

                                    }

                                } 

                                // For each predecessor of b, expand its list of live-at-end values.
                                // invariant: live contains the values live at the start of b (excluding phi inputs)


                                i = i__prev3;
                            } 

                            // For each predecessor of b, expand its list of live-at-end values.
                            // invariant: live contains the values live at the start of b (excluding phi inputs)
                            {
                                var i__prev3 = i;
                                var e__prev3 = e;

                                foreach (var (__i, __e) in b.Preds)
                                {
                                    i = __i;
                                    e = __e;
                                    p = e.b; 
                                    // Compute additional distance for the edge.
                                    // Note: delta must be at least 1 to distinguish the control
                                    // value use from the first user in a successor block.
                                    var delta = int32(normalDistance);
                                    if (len(p.Succs) == 2L)
                                    {
                                        if (p.Succs[0L].b == b && p.Likely == BranchLikely || p.Succs[1L].b == b && p.Likely == BranchUnlikely)
                                        {
                                            delta = likelyDistance;
                                        }

                                        if (p.Succs[0L].b == b && p.Likely == BranchUnlikely || p.Succs[1L].b == b && p.Likely == BranchLikely)
                                        {
                                            delta = unlikelyDistance;
                                        }

                                    } 

                                    // Update any desired registers at the end of p.
                                    s.desired[p.ID].merge(_addr_desired); 

                                    // Start t off with the previously known live values at the end of p.
                                    t.clear();
                                    {
                                        var e__prev4 = e;

                                        foreach (var (_, __e) in s.live[p.ID])
                                        {
                                            e = __e;
                                            t.set(e.ID, e.dist, e.pos);
                                        }

                                        e = e__prev4;
                                    }

                                    var update = false; 

                                    // Add new live values from scanning this block.
                                    {
                                        var e__prev4 = e;

                                        foreach (var (_, __e) in live.contents())
                                        {
                                            e = __e;
                                            d = e.val + delta;
                                            if (!t.contains(e.key) || d < t.get(e.key))
                                            {
                                                update = true;
                                                t.set(e.key, d, e.aux);
                                            }

                                        } 
                                        // Also add the correct arg from the saved phi values.
                                        // All phis are at distance delta (we consider them
                                        // simultaneously happening at the start of the block).

                                        e = e__prev4;
                                    }

                                    {
                                        var v__prev4 = v;

                                        foreach (var (_, __v) in phis)
                                        {
                                            v = __v;
                                            var id = v.Args[i].ID;
                                            if (s.values[id].needReg && (!t.contains(id) || delta < t.get(id)))
                                            {
                                                update = true;
                                                t.set(id, delta, v.Pos);
                                            }

                                        }

                                        v = v__prev4;
                                    }

                                    if (!update)
                                    {
                                        continue;
                                    } 
                                    // The live set has changed, update it.
                                    l = s.live[p.ID][..0L];
                                    if (cap(l) < t.size())
                                    {
                                        l = make_slice<liveInfo>(0L, t.size());
                                    }

                                    {
                                        var e__prev4 = e;

                                        foreach (var (_, __e) in t.contents())
                                        {
                                            e = __e;
                                            l = append(l, new liveInfo(e.key,e.val,e.aux));
                                        }

                                        e = e__prev4;
                                    }

                                    s.live[p.ID] = l;
                                    changed = true;

                                }

                                i = i__prev3;
                                e = e__prev3;
                            }
                        }

                        b = b__prev2;
                    }

                    if (!changed)
                    {
                        break;
                    }

                }

                if (f.pass.debug > regDebug)
                {
                    fmt.Println("live values at end of each block");
                    {
                        var b__prev1 = b;

                        foreach (var (_, __b) in f.Blocks)
                        {
                            b = __b;
                            fmt.Printf("  %s:", b);
                            {
                                var x__prev2 = x;

                                foreach (var (_, __x) in s.live[b.ID])
                                {
                                    x = __x;
                                    fmt.Printf(" v%d", x.ID);
                                    {
                                        var e__prev3 = e;

                                        foreach (var (_, __e) in s.desired[b.ID].entries)
                                        {
                                            e = __e;
                                            if (e.ID != x.ID)
                                            {
                                                continue;
                                            }

                                            fmt.Printf("[");
                                            var first = true;
                                            {
                                                var r__prev4 = r;

                                                foreach (var (_, __r) in e.regs)
                                                {
                                                    r = __r;
                                                    if (r == noRegister)
                                                    {
                                                        continue;
                                                    }

                                                    if (!first)
                                                    {
                                                        fmt.Printf(",");
                                                    }

                                                    fmt.Print(_addr_s.registers[r]);
                                                    first = false;

                                                }

                                                r = r__prev4;
                                            }

                                            fmt.Printf("]");

                                        }

                                        e = e__prev3;
                                    }
                                }

                                x = x__prev2;
                            }

                            {
                                var avoid = s.desired[b.ID].avoid;

                                if (avoid != 0L)
                                {
                                    fmt.Printf(" avoid=%v", s.RegMaskString(avoid));
                                }

                            }

                            fmt.Println();

                        }

                        b = b__prev1;
                    }
                }

            }

            // A desiredState represents desired register assignments.
            private partial struct desiredState
            {
                public slice<desiredStateEntry> entries; // Registers that other values want to be in.  This value will
// contain at least the union of the regs fields of entries, but
// may contain additional entries for values that were once in
// this data structure but are no longer.
                public regMask avoid;
            }
            private partial struct desiredStateEntry
            {
                public ID ID; // Registers it would like to be in, in priority order.
// Unused slots are filled with noRegister.
                public array<register> regs;
            }

            (d * desiredState);

            clear();

            {
                d.entries = d.entries[..0L];
                d.avoid = 0L;
            }

            // get returns a list of desired registers for value vid.
            (d * desiredState);

            vid;

            register;

            {
                {
                    var e__prev1 = e;

                    foreach (var (_, __e) in d.entries)
                    {
                        e = __e;
                        if (e.ID == vid)
                        {
                            return e.regs;
                        }

                    }

                    e = e__prev1;
                }

                return new array<register>(new register[] { noRegister, noRegister, noRegister, noRegister });

            }

            // add records that we'd like value vid to be in register r.
            (d * desiredState);

            vid;

            r;

            {
                d.avoid |= regMask(1L) << (int)(r);
                {
                    var i__prev1 = i;

                    foreach (var (__i) in d.entries)
                    {
                        i = __i;
                        e = _addr_d.entries[i];
                        if (e.ID != vid)
                        {
                            continue;
                        }

                        if (e.regs[0L] == r)
                        { 
                            // Already known and highest priority
                            return ;

                        }

                        {
                            var j__prev2 = j;

                            for (long j = 1L; j < len(e.regs); j++)
                            {
                                if (e.regs[j] == r)
                                { 
                                    // Move from lower priority to top priority
                                    copy(e.regs[1L..], e.regs[..j]);
                                    e.regs[0L] = r;
                                    return ;

                                }

                            }


                            j = j__prev2;
                        }
                        copy(e.regs[1L..], e.regs[..]);
                        e.regs[0L] = r;
                        return ;

                    }

                    i = i__prev1;
                }

                d.entries = append(d.entries, new desiredStateEntry(vid,[4]register{r,noRegister,noRegister,noRegister}));

            }
            (d * desiredState);

            vid;

            ID <missing '='> regs[4L];

            { 
                // regs is in priority order, so iterate in reverse order.
                {
                    var i__prev1 = i;

                    for (i = len(regs) - 1L; i >= 0L; i--)
                    {
                        r = regs[i];
                        if (r != noRegister)
                        {
                            d.add(vid, r);
                        }

                    }


                    i = i__prev1;
                }

            }

            // clobber erases any desired registers in the set m.
            (d * desiredState);

            m;

            {
                {
                    var i__prev1 = i;

                    i = 0L;

                    while (i < len(d.entries))
                    {
                        e = _addr_d.entries[i];
                        j = 0L;
                        {
                            var r__prev2 = r;

                            foreach (var (_, __r) in e.regs)
                            {
                                r = __r;
                                if (r != noRegister && m >> (int)(r) & 1L == 0L)
                                {
                                    e.regs[j] = r;
                                    j++;
                                }

                            }

                            r = r__prev2;
                        }

                        if (j == 0L)
                        { 
                            // No more desired registers for this value.
                            d.entries[i] = d.entries[len(d.entries) - 1L];
                            d.entries = d.entries[..len(d.entries) - 1L];
                            continue;

                        }

                        while (j < len(e.regs))
                        {
                            e.regs[j] = noRegister;
                            j++;
                        }

                        i++;

                    }


                    i = i__prev1;
                }
                d.avoid &= m;

            }

            // copy copies a desired state from another desiredState x.
            (Func<ptr<desiredState>, copy>)x * desiredState;

            {
                d.entries = append(d.entries[..0L], x.entries);
                d.avoid = x.avoid;
            }

            // remove removes the desired registers for vid and returns them.
            (d * desiredState);

            vid;

            register;

            {
                {
                    var i__prev1 = i;

                    foreach (var (__i) in d.entries)
                    {
                        i = __i;
                        if (d.entries[i].ID == vid)
                        {
                            var regs = d.entries[i].regs;
                            d.entries[i] = d.entries[len(d.entries) - 1L];
                            d.entries = d.entries[..len(d.entries) - 1L];
                            return regs;
                        }

                    }

                    i = i__prev1;
                }

                return new array<register>(new register[] { noRegister, noRegister, noRegister, noRegister });

            }

            // merge merges another desired state x into d.
            (Func<ptr<desiredState>, merge>)x * desiredState;

            {
                d.avoid |= x.avoid; 
                // There should only be a few desired registers, so
                // linear insert is ok.
                {
                    var e__prev1 = e;

                    foreach (var (_, __e) in x.entries)
                    {
                        e = __e;
                        d.addList(e.ID, e.regs);
                    }

                    e = e__prev1;
                }
            }
            y;

            int32;

            {
                if (x < y)
                {
                    return x;
                }

                return y;

            }
            y;

            int32;

            {
                if (x > y)
                {
                    return x;
                }

                return y;

            }

        });
    }
}}}}
