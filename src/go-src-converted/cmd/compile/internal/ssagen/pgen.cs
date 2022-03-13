// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssagen -- go2cs converted at 2022 March 13 06:23:02 UTC
// import "cmd/compile/internal/ssagen" ==> using ssagen = go.cmd.compile.@internal.ssagen_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssagen\pgen.go
namespace go.cmd.compile.@internal;

using buildcfg = @internal.buildcfg_package;
using race = @internal.race_package;
using rand = math.rand_package;
using sort = sort_package;
using sync = sync_package;
using time = time_package;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using objw = cmd.compile.@internal.objw_package;
using ssa = cmd.compile.@internal.ssa_package;
using types = cmd.compile.@internal.types_package;
using obj = cmd.@internal.obj_package;
using objabi = cmd.@internal.objabi_package;
using src = cmd.@internal.src_package;


// cmpstackvarlt reports whether the stack variable a sorts before b.
//
// Sort the list of stack variables. Autos after anything else,
// within autos, unused after used, within used, things with
// pointers first, zeroed things first, and then decreasing size.
// Because autos are laid out in decreasing addresses
// on the stack, pointers first, zeroed things first and decreasing size
// really means, in memory, things with pointers needing zeroing at
// the top of the stack and increasing in size.
// Non-autos sort on offset.

using System;
public static partial class ssagen_package {

private static bool cmpstackvarlt(ptr<ir.Name> _addr_a, ptr<ir.Name> _addr_b) {
    ref ir.Name a = ref _addr_a.val;
    ref ir.Name b = ref _addr_b.val;

    if (needAlloc(_addr_a) != needAlloc(_addr_b)) {
        return needAlloc(_addr_b);
    }
    if (!needAlloc(_addr_a)) {
        return a.FrameOffset() < b.FrameOffset();
    }
    if (a.Used() != b.Used()) {
        return a.Used();
    }
    var ap = a.Type().HasPointers();
    var bp = b.Type().HasPointers();
    if (ap != bp) {
        return ap;
    }
    ap = a.Needzero();
    bp = b.Needzero();
    if (ap != bp) {
        return ap;
    }
    if (a.Type().Width != b.Type().Width) {
        return a.Type().Width > b.Type().Width;
    }
    return a.Sym().Name < b.Sym().Name;
}

// byStackvar implements sort.Interface for []*Node using cmpstackvarlt.
private partial struct byStackVar { // : slice<ptr<ir.Name>>
}

private static nint Len(this byStackVar s) {
    return len(s);
}
private static bool Less(this byStackVar s, nint i, nint j) {
    return cmpstackvarlt(_addr_s[i], _addr_s[j]);
}
private static void Swap(this byStackVar s, nint i, nint j) {
    (s[i], s[j]) = (s[j], s[i]);
}

// needAlloc reports whether n is within the current frame, for which we need to
// allocate space. In particular, it excludes arguments and results, which are in
// the callers frame.
private static bool needAlloc(ptr<ir.Name> _addr_n) {
    ref ir.Name n = ref _addr_n.val;

    return n.Class == ir.PAUTO || n.Class == ir.PPARAMOUT && n.IsOutputParamInRegisters();
}

private static void AllocFrame(this ptr<ssafn> _addr_s, ptr<ssa.Func> _addr_f) {
    ref ssafn s = ref _addr_s.val;
    ref ssa.Func f = ref _addr_f.val;

    s.stksize = 0;
    s.stkptrsize = 0;
    var fn = s.curfn; 

    // Mark the PAUTO's unused.
    foreach (var (_, ln) in fn.Dcl) {
        if (needAlloc(_addr_ln)) {
            ln.SetUsed(false);
        }
    }    foreach (var (_, l) in f.RegAlloc) {
        {
            ssa.LocalSlot (ls, ok) = l._<ssa.LocalSlot>();

            if (ok) {
                ls.N.SetUsed(true);
            }

        }
    }    foreach (var (_, b) in f.Blocks) {
        foreach (var (_, v) in b.Values) {
            {
                ptr<ir.Name> n__prev1 = n;

                ptr<ir.Name> (n, ok) = v.Aux._<ptr<ir.Name>>();

                if (ok) {

                    if (n.Class == ir.PPARAMOUT)
                    {
                        if (n.IsOutputParamInRegisters() && v.Op == ssa.OpVarDef) { 
                            // ignore VarDef, look for "real" uses.
                            // TODO: maybe do this for PAUTO as well?
                            continue;
                        }
                        fallthrough = true;
                    }
                    if (fallthrough || n.Class == ir.PPARAM || n.Class == ir.PAUTO)
                    {
                        n.SetUsed(true);
                        goto __switch_break0;
                    }

                    __switch_break0:;
                }

                n = n__prev1;

            }
        }
    }    sort.Sort(byStackVar(fn.Dcl)); 

    // Reassign stack offsets of the locals that are used.
    var lastHasPtr = false;
    {
        ptr<ir.Name> n__prev1 = n;

        foreach (var (__i, __n) in fn.Dcl) {
            i = __i;
            n = __n;
            if (n.Op() != ir.ONAME || n.Class != ir.PAUTO && !(n.Class == ir.PPARAMOUT && n.IsOutputParamInRegisters())) { 
                // i.e., stack assign if AUTO, or if PARAMOUT in registers (which has no predefined spill locations)
                continue;
            }
            if (!n.Used()) {
                fn.Dcl = fn.Dcl[..(int)i];
                break;
            }
            types.CalcSize(n.Type());
            var w = n.Type().Width;
            if (w >= types.MaxWidth || w < 0) {
                @base.Fatalf("bad width");
            }
            if (w == 0 && lastHasPtr) { 
                // Pad between a pointer-containing object and a zero-sized object.
                // This prevents a pointer to the zero-sized object from being interpreted
                // as a pointer to the pointer-containing object (and causing it
                // to be scanned when it shouldn't be). See issue 24993.
                w = 1;
            }
            s.stksize += w;
            s.stksize = types.Rnd(s.stksize, int64(n.Type().Align));
            if (n.Type().HasPointers()) {
                s.stkptrsize = s.stksize;
                lastHasPtr = true;
            }
            else
 {
                lastHasPtr = false;
            }
            n.SetFrameOffset(-s.stksize);
        }
        n = n__prev1;
    }

    s.stksize = types.Rnd(s.stksize, int64(types.RegSize));
    s.stkptrsize = types.Rnd(s.stkptrsize, int64(types.RegSize));
}

private static readonly nint maxStackSize = 1 << 30;

// Compile builds an SSA backend function,
// uses it to generate a plist,
// and flushes that plist to machine code.
// worker indicates which of the backend workers is doing the processing.


// Compile builds an SSA backend function,
// uses it to generate a plist,
// and flushes that plist to machine code.
// worker indicates which of the backend workers is doing the processing.
public static void Compile(ptr<ir.Func> _addr_fn, nint worker) => func((defer, _, _) => {
    ref ir.Func fn = ref _addr_fn.val;

    var f = buildssa(fn, worker); 
    // Note: check arg size to fix issue 25507.
    if (f.Frontend()._<ptr<ssafn>>().stksize >= maxStackSize || f.OwnAux.ArgWidth() >= maxStackSize) {
        largeStackFramesMu.Lock();
        largeStackFrames = append(largeStackFrames, new largeStack(locals:f.Frontend().(*ssafn).stksize,args:f.OwnAux.ArgWidth(),pos:fn.Pos()));
        largeStackFramesMu.Unlock();
        return ;
    }
    var pp = objw.NewProgs(fn, worker);
    defer(pp.Free());
    genssa(f, pp); 
    // Check frame size again.
    // The check above included only the space needed for local variables.
    // After genssa, the space needed includes local variables and the callee arg region.
    // We must do this check prior to calling pp.Flush.
    // If there are any oversized stack frames,
    // the assembler may emit inscrutable complaints about invalid instructions.
    if (pp.Text.To.Offset >= maxStackSize) {
        largeStackFramesMu.Lock();
        ptr<ssafn> locals = f.Frontend()._<ptr<ssafn>>().stksize;
        largeStackFrames = append(largeStackFrames, new largeStack(locals:locals,args:f.OwnAux.ArgWidth(),callee:pp.Text.To.Offset-locals,pos:fn.Pos()));
        largeStackFramesMu.Unlock();
        return ;
    }
    pp.Flush(); // assemble, fill in boilerplate, etc.
    // fieldtrack must be called after pp.Flush. See issue 20014.
    fieldtrack(_addr_pp.Text.From.Sym, fn.FieldTrack);
});

private static void init() {
    if (race.Enabled) {
        rand.Seed(time.Now().UnixNano());
    }
}

// StackOffset returns the stack location of a LocalSlot relative to the
// stack pointer, suitable for use in a DWARF location entry. This has nothing
// to do with its offset in the user variable.
public static int StackOffset(ssa.LocalSlot slot) {
    var n = slot.N;
    long off = default;

    if (n.Class == ir.PPARAM || n.Class == ir.PPARAMOUT)
    {
        if (!n.IsOutputParamInRegisters()) {
            off = n.FrameOffset() + @base.Ctxt.FixedFrameSize();
            break;
        }
        fallthrough = true; // PPARAMOUT in registers allocates like an AUTO
    }
    if (fallthrough || n.Class == ir.PAUTO)
    {
        off = n.FrameOffset();
        if (@base.Ctxt.FixedFrameSize() == 0) {
            off -= int64(types.PtrSize);
        }
        if (buildcfg.FramePointerEnabled) {
            off -= int64(types.PtrSize);
        }
        goto __switch_break1;
    }

    __switch_break1:;
    return int32(off + slot.Off);
}

// fieldtrack adds R_USEFIELD relocations to fnsym to record any
// struct fields that it used.
private static void fieldtrack(ptr<obj.LSym> _addr_fnsym, object tracked) {
    ref obj.LSym fnsym = ref _addr_fnsym.val;

    if (fnsym == null) {
        return ;
    }
    if (!buildcfg.Experiment.FieldTrack || len(tracked) == 0) {
        return ;
    }
    var trackSyms = make_slice<ptr<obj.LSym>>(0, len(tracked));
    {
        var sym__prev1 = sym;

        foreach (var (__sym) in tracked) {
            sym = __sym;
            trackSyms = append(trackSyms, sym);
        }
        sym = sym__prev1;
    }

    sort.Slice(trackSyms, (i, j) => trackSyms[i].Name < trackSyms[j].Name);
    {
        var sym__prev1 = sym;

        foreach (var (_, __sym) in trackSyms) {
            sym = __sym;
            var r = obj.Addrel(fnsym);
            r.Sym = sym;
            r.Type = objabi.R_USEFIELD;
        }
        sym = sym__prev1;
    }
}

// largeStack is info about a function whose stack frame is too large (rare).
private partial struct largeStack {
    public long locals;
    public long args;
    public long callee;
    public src.XPos pos;
}

private static sync.Mutex largeStackFramesMu = default;private static slice<largeStack> largeStackFrames = default;

public static void CheckLargeStacks() { 
    // Check whether any of the functions we have compiled have gigantic stack frames.
    sort.Slice(largeStackFrames, (i, j) => largeStackFrames[i].pos.Before(largeStackFrames[j].pos));
    foreach (var (_, large) in largeStackFrames) {
        if (large.callee != 0) {
            @base.ErrorfAt(large.pos, "stack frame too large (>1GB): %d MB locals + %d MB args + %d MB callee", large.locals >> 20, large.args >> 20, large.callee >> 20);
        }
        else
 {
            @base.ErrorfAt(large.pos, "stack frame too large (>1GB): %d MB locals + %d MB args", large.locals >> 20, large.args >> 20);
        }
    }
}

} // end ssagen_package
