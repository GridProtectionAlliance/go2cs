// Derived from Inferno utils/6c/txt.c
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/6c/txt.c
//
//    Copyright © 1994-1999 Lucent Technologies Inc.  All rights reserved.
//    Portions Copyright © 1995-1997 C H Forsyth (forsyth@terzarima.net)
//    Portions Copyright © 1997-1999 Vita Nuova Limited
//    Portions Copyright © 2000-2007 Vita Nuova Holdings Limited (www.vitanuova.com)
//    Portions Copyright © 2004,2006 Bruce Ellis
//    Portions Copyright © 2005-2007 C H Forsyth (forsyth@terzarima.net)
//    Revisions Copyright © 2000-2007 Lucent Technologies Inc. and others
//    Portions Copyright © 2009 The Go Authors. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

// package objw -- go2cs converted at 2022 March 06 22:47:46 UTC
// import "cmd/compile/internal/objw" ==> using objw = go.cmd.compile.@internal.objw_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\objw\prog.go
using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;

namespace go.cmd.compile.@internal;

public static partial class objw_package {

private static ptr<var> sharedProgArray = @new<[10000]obj.Prog>(); // *T instead of T to work around issue 19839

// NewProgs returns a new Progs for fn.
// worker indicates which of the backend workers will use the Progs.
public static ptr<Progs> NewProgs(ptr<ir.Func> _addr_fn, nint worker) {
    ref ir.Func fn = ref _addr_fn.val;

    ptr<Progs> pp = @new<Progs>();
    if (@base.Ctxt.CanReuseProgs()) {
        var sz = len(sharedProgArray) / @base.Flag.LowerC;
        pp.Cache = sharedProgArray[(int)sz * worker..(int)sz * (worker + 1)];
    }
    pp.CurFunc = fn; 

    // prime the pump
    pp.Next = pp.NewProg();
    pp.Clear(pp.Next);

    pp.Pos = fn.Pos();
    pp.SetText(fn); 
    // PCDATA tables implicitly start with index -1.
    pp.PrevLive = new LivenessIndex(-1,false);
    pp.NextLive = pp.PrevLive;
    return _addr_pp!;

}

// Progs accumulates Progs for a function and converts them into machine code.
public partial struct Progs {
    public ptr<obj.Prog> Text; // ATEXT Prog for this function
    public ptr<obj.Prog> Next; // next Prog
    public long PC; // virtual PC; count of Progs
    public src.XPos Pos; // position to use for new Progs
    public ptr<ir.Func> CurFunc; // fn these Progs are for
    public slice<obj.Prog> Cache; // local progcache
    public nint CacheIndex; // first free element of progcache

    public LivenessIndex NextLive; // liveness index for the next Prog
    public LivenessIndex PrevLive; // last emitted liveness index
}

// LivenessIndex stores the liveness map information for a Value.
public partial struct LivenessIndex {
    public nint StackMapIndex; // IsUnsafePoint indicates that this is an unsafe-point.
//
// Note that it's possible for a call Value to have a stack
// map while also being an unsafe-point. This means it cannot
// be preempted at this instruction, but that a preemption or
// stack growth may happen in the called function.
    public bool IsUnsafePoint;
}

// StackMapDontCare indicates that the stack map index at a Value
// doesn't matter.
//
// This is a sentinel value that should never be emitted to the PCDATA
// stream. We use -1000 because that's obviously never a valid stack
// index (but -1 is).
public static readonly nint StackMapDontCare = -1000;

// LivenessDontCare indicates that the liveness information doesn't
// matter. Currently it is used in deferreturn liveness when we don't
// actually need it. It should never be emitted to the PCDATA stream.


// LivenessDontCare indicates that the liveness information doesn't
// matter. Currently it is used in deferreturn liveness when we don't
// actually need it. It should never be emitted to the PCDATA stream.
public static LivenessIndex LivenessDontCare = new LivenessIndex(StackMapDontCare,true);

public static bool StackMapValid(this LivenessIndex idx) {
    return idx.StackMapIndex != StackMapDontCare;
}

private static ptr<obj.Prog> NewProg(this ptr<Progs> _addr_pp) {
    ref Progs pp = ref _addr_pp.val;

    ptr<obj.Prog> p;
    if (pp.CacheIndex < len(pp.Cache)) {
        p = _addr_pp.Cache[pp.CacheIndex];
        pp.CacheIndex++;
    }
    else
 {
        p = @new<obj.Prog>();
    }
    p.Ctxt = @base.Ctxt;
    return _addr_p!;

}

// Flush converts from pp to machine code.
private static void Flush(this ptr<Progs> _addr_pp) {
    ref Progs pp = ref _addr_pp.val;

    ptr<obj.Plist> plist = addr(new obj.Plist(Firstpc:pp.Text,Curfn:pp.CurFunc));
    obj.Flushplist(@base.Ctxt, plist, pp.NewProg, @base.Ctxt.Pkgpath);
}

// Free clears pp and any associated resources.
private static void Free(this ptr<Progs> _addr_pp) {
    ref Progs pp = ref _addr_pp.val;

    if (@base.Ctxt.CanReuseProgs()) { 
        // Clear progs to enable GC and avoid abuse.
        var s = pp.Cache[..(int)pp.CacheIndex];
        foreach (var (i) in s) {
            s[i] = new obj.Prog();
        }
    }
    pp.val = new Progs();

}

// Prog adds a Prog with instruction As to pp.
private static ptr<obj.Prog> Prog(this ptr<Progs> _addr_pp, obj.As @as) {
    ref Progs pp = ref _addr_pp.val;

    if (pp.NextLive.StackMapValid() && pp.NextLive.StackMapIndex != pp.PrevLive.StackMapIndex) { 
        // Emit stack map index change.
        var idx = pp.NextLive.StackMapIndex;
        pp.PrevLive.StackMapIndex = idx;
        var p = pp.Prog(obj.APCDATA);
        p.From.SetConst(objabi.PCDATA_StackMapIndex);
        p.To.SetConst(int64(idx));

    }
    if (pp.NextLive.IsUnsafePoint != pp.PrevLive.IsUnsafePoint) { 
        // Emit unsafe-point marker.
        pp.PrevLive.IsUnsafePoint = pp.NextLive.IsUnsafePoint;
        p = pp.Prog(obj.APCDATA);
        p.From.SetConst(objabi.PCDATA_UnsafePoint);
        if (pp.NextLive.IsUnsafePoint) {
            p.To.SetConst(objabi.PCDATA_UnsafePointUnsafe);
        }
        else
 {
            p.To.SetConst(objabi.PCDATA_UnsafePointSafe);
        }
    }
    p = pp.Next;
    pp.Next = pp.NewProg();
    pp.Clear(pp.Next);
    p.Link = pp.Next;

    if (!pp.Pos.IsKnown() && @base.Flag.K != 0) {
        @base.Warn("prog: unknown position (line 0)");
    }
    p.As = as;
    p.Pos = pp.Pos;
    if (pp.Pos.IsStmt() == src.PosIsStmt) { 
        // Clear IsStmt for later Progs at this pos provided that as can be marked as a stmt
        if (LosesStmtMark(as)) {
            return _addr_p!;
        }
        pp.Pos = pp.Pos.WithNotStmt();

    }
    return _addr_p!;

}

private static void Clear(this ptr<Progs> _addr_pp, ptr<obj.Prog> _addr_p) {
    ref Progs pp = ref _addr_pp.val;
    ref obj.Prog p = ref _addr_p.val;

    obj.Nopout(p);
    p.As = obj.AEND;
    p.Pc = pp.PC;
    pp.PC++;
}

private static ptr<obj.Prog> Append(this ptr<Progs> _addr_pp, ptr<obj.Prog> _addr_p, obj.As @as, obj.AddrType ftype, short freg, long foffset, obj.AddrType ttype, short treg, long toffset) {
    ref Progs pp = ref _addr_pp.val;
    ref obj.Prog p = ref _addr_p.val;

    var q = pp.NewProg();
    pp.Clear(q);
    q.As = as;
    q.Pos = p.Pos;
    q.From.Type = ftype;
    q.From.Reg = freg;
    q.From.Offset = foffset;
    q.To.Type = ttype;
    q.To.Reg = treg;
    q.To.Offset = toffset;
    q.Link = p.Link;
    p.Link = q;
    return _addr_q!;
}

private static void SetText(this ptr<Progs> _addr_pp, ptr<ir.Func> _addr_fn) {
    ref Progs pp = ref _addr_pp.val;
    ref ir.Func fn = ref _addr_fn.val;

    if (pp.Text != null) {
        @base.Fatalf("Progs.SetText called twice");
    }
    var ptxt = pp.Prog(obj.ATEXT);
    pp.Text = ptxt;

    fn.LSym.Func().Text = ptxt;
    ptxt.From.Type = obj.TYPE_MEM;
    ptxt.From.Name = obj.NAME_EXTERN;
    ptxt.From.Sym = fn.LSym;

}

// LosesStmtMark reports whether a prog with op as loses its statement mark on the way to DWARF.
// The attributes from some opcodes are lost in translation.
// TODO: this is an artifact of how funcpctab combines information for instructions at a single PC.
// Should try to fix it there.
public static bool LosesStmtMark(obj.As @as) { 
    // is_stmt does not work for these; it DOES for ANOP even though that generates no code.
    return as == obj.APCDATA || as == obj.AFUNCDATA;

}

} // end objw_package
