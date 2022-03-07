// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package dwarfgen -- go2cs converted at 2022 March 06 23:14:23 UTC
// import "cmd/compile/internal/dwarfgen" ==> using dwarfgen = go.cmd.compile.@internal.dwarfgen_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\dwarfgen\dwinl.go
using fmt = go.fmt_package;
using strings = go.strings_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using dwarf = go.cmd.@internal.dwarf_package;
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;

namespace go.cmd.compile.@internal;

public static partial class dwarfgen_package {

    // To identify variables by original source position.
private partial struct varPos {
    public @string DeclName;
    public @string DeclFile;
    public nuint DeclLine;
    public nuint DeclCol;
}

// This is the main entry point for collection of raw material to
// drive generation of DWARF "inlined subroutine" DIEs. See proposal
// 22080 for more details and background info.
private static dwarf.InlCalls assembleInlines(ptr<obj.LSym> _addr_fnsym, slice<ptr<dwarf.Var>> dwVars) {
    ref obj.LSym fnsym = ref _addr_fnsym.val;

    ref dwarf.InlCalls inlcalls = ref heap(out ptr<dwarf.InlCalls> _addr_inlcalls);

    if (@base.Debug.DwarfInl != 0) {
        @base.Ctxt.Logf("assembling DWARF inlined routine info for %v\n", fnsym.Name);
    }
    var imap = make_map<nint, nint>(); 

    // Walk progs to build up the InlCalls data structure
    src.XPos prevpos = default;
    {
        var p__prev1 = p;

        var p = fnsym.Func().Text;

        while (p != null) {
            if (p.Pos == prevpos) {
                continue;
            p = p.Link;
            }

            var ii = posInlIndex(p.Pos);
            if (ii >= 0) {
                insertInlCall(_addr_inlcalls, ii, imap);
            }

            prevpos = p.Pos;

        }

        p = p__prev1;
    } 

    // This is used to partition DWARF vars by inline index. Vars not
    // produced by the inliner will wind up in the vmap[0] entry.
    var vmap = make_map<int, slice<ptr<dwarf.Var>>>(); 

    // Now walk the dwarf vars and partition them based on whether they
    // were produced by the inliner (dwv.InlIndex > 0) or were original
    // vars/params from the function (dwv.InlIndex == 0).
    foreach (var (_, dwv) in dwVars) {
        vmap[dwv.InlIndex] = append(vmap[dwv.InlIndex], dwv); 

        // Zero index => var was not produced by an inline
        if (dwv.InlIndex == 0) {
            continue;
        }
        ii = int(dwv.InlIndex) - 1;
        var (idx, ok) = imap[ii];
        if (!ok) { 
            // We can occasionally encounter a var produced by the
            // inliner for which there is no remaining prog; add a new
            // entry to the call list in this scenario.
            idx = insertInlCall(_addr_inlcalls, ii, imap);

        }
        inlcalls.Calls[idx].InlVars = append(inlcalls.Calls[idx].InlVars, dwv);

    }    {
        var ii__prev1 = ii;

        foreach (var (__ii, __sl) in vmap) {
            ii = __ii;
            sl = __sl;
            map<varPos, nint> m = default;
            if (ii == 0) {
                if (!fnsym.WasInlined()) {
                    {
                        var v__prev2 = v;

                        foreach (var (__j, __v) in sl) {
                            j = __j;
                            v = __v;
                            v.ChildIndex = int32(j);
                        }

                        v = v__prev2;
                    }

                    continue;

                }
            else
                m = makePreinlineDclMap(_addr_fnsym);

            } {
                var ifnlsym = @base.Ctxt.InlTree.InlinedFunction(int(ii - 1));
                m = makePreinlineDclMap(_addr_ifnlsym);
            } 

            // Here we assign child indices to variables based on
            // pre-inlined decls, and set the "IsInAbstract" flag
            // appropriately. In addition: parameter and local variable
            // names are given "middle dot" version numbers as part of the
            // writing them out to export data (see issue 4326). If DWARF
            // inlined routine generation is turned on, we want to undo
            // this versioning, since DWARF variables in question will be
            // parented by the inlined routine and not the top-level
            // caller.
            var synthCount = len(m);
            {
                var v__prev2 = v;

                foreach (var (_, __v) in sl) {
                    v = __v;
                    var canonName = unversion(v.Name);
                    varPos vp = new varPos(DeclName:canonName,DeclFile:v.DeclFile,DeclLine:v.DeclLine,DeclCol:v.DeclCol,);
                    var synthesized = strings.HasPrefix(v.Name, "~r") || canonName == "_" || strings.HasPrefix(v.Name, "~b");
                    {
                        var idx__prev1 = idx;

                        var (idx, found) = m[vp];

                        if (found) {
                            v.ChildIndex = int32(idx);
                            v.IsInAbstract = !synthesized;
                            v.Name = canonName;
                        }
                        else
 { 
                            // Variable can't be found in the pre-inline dcl list.
                            // In the top-level case (ii=0) this can happen
                            // because a composite variable was split into pieces,
                            // and we're looking at a piece. We can also see
                            // return temps (~r%d) that were created during
                            // lowering, or unnamed params ("_").
                            v.ChildIndex = int32(synthCount);
                            synthCount++;

                        }

                        idx = idx__prev1;

                    }

                }

                v = v__prev2;
            }
        }
        ii = ii__prev1;
    }

    var start = int64(-1);
    nint curii = -1;
    ptr<obj.Prog> prevp;
    {
        var p__prev1 = p;

        p = fnsym.Func().Text;

        while (p != null) {
            if (prevp != null && p.Pos == prevp.Pos) {
                continue;
            (prevp, p) = (p, p.Link);
            }

            ii = posInlIndex(p.Pos);
            if (ii == curii) {
                continue;
            } 
            // Close out the current range
            if (start != -1) {
                addRange(inlcalls.Calls, start, p.Pc, curii, imap);
            } 
            // Begin new range
            start = p.Pc;
            curii = ii;

        }

        p = p__prev1;
    }
    if (start != -1) {
        addRange(inlcalls.Calls, start, fnsym.Size, curii, imap);
    }
    {
        var k__prev1 = k;
        var c__prev1 = c;

        foreach (var (__k, __c) in inlcalls.Calls) {
            k = __k;
            c = __c;
            if (c.Root) {
                unifyCallRanges(inlcalls, k);
            }
        }
        k = k__prev1;
        c = c__prev1;
    }

    if (@base.Debug.DwarfInl != 0) {
        dumpInlCalls(inlcalls);
        dumpInlVars(dwVars);
    }
    {
        var k__prev1 = k;
        var c__prev1 = c;

        foreach (var (__k, __c) in inlcalls.Calls) {
            k = __k;
            c = __c;
            if (c.Root) {
                checkInlCall(fnsym.Name, inlcalls, fnsym.Size, k, -1);
            }
        }
        k = k__prev1;
        c = c__prev1;
    }

    return inlcalls;

}

// Secondary hook for DWARF inlined subroutine generation. This is called
// late in the compilation when it is determined that we need an
// abstract function DIE for an inlined routine imported from a
// previously compiled package.
public static void AbstractFunc(ptr<obj.LSym> _addr_fn) {
    ref obj.LSym fn = ref _addr_fn.val;

    var ifn = @base.Ctxt.DwFixups.GetPrecursorFunc(fn);
    if (ifn == null) {
        @base.Ctxt.Diag("failed to locate precursor fn for %v", fn);
        return ;
    }
    _ = ifn._<ptr<ir.Func>>();
    if (@base.Debug.DwarfInl != 0) {
        @base.Ctxt.Logf("DwarfAbstractFunc(%v)\n", fn.Name);
    }
    @base.Ctxt.DwarfAbstractFunc(ifn, fn, @base.Ctxt.Pkgpath);

}

// Undo any versioning performed when a name was written
// out as part of export data.
private static @string unversion(@string name) {
    {
        var i = strings.Index(name, "·");

        if (i > 0) {
            name = name[..(int)i];
        }
    }

    return name;

}

// Given a function that was inlined as part of the compilation, dig
// up the pre-inlining DCL list for the function and create a map that
// supports lookup of pre-inline dcl index, based on variable
// position/name. NB: the recipe for computing variable pos/file/line
// needs to be kept in sync with the similar code in gc.createSimpleVars
// and related functions.
private static map<varPos, nint> makePreinlineDclMap(ptr<obj.LSym> _addr_fnsym) {
    ref obj.LSym fnsym = ref _addr_fnsym.val;

    var dcl = preInliningDcls(fnsym);
    var m = make_map<varPos, nint>();
    foreach (var (i, n) in dcl) {
        var pos = @base.Ctxt.InnermostPos(n.Pos());
        varPos vp = new varPos(DeclName:unversion(n.Sym().Name),DeclFile:pos.RelFilename(),DeclLine:pos.RelLine(),DeclCol:pos.Col(),);
        {
            var (_, found) = m[vp];

            if (found) { 
                // We can see collisions (variables with the same name/file/line/col) in obfuscated or machine-generated code -- see issue 44378 for an example. Skip duplicates in such cases, since it is unlikely that a human will be debugging such code.
                continue;

            }

        }

        m[vp] = i;

    }    return m;

}

private static nint insertInlCall(ptr<dwarf.InlCalls> _addr_dwcalls, nint inlIdx, map<nint, nint> imap) {
    ref dwarf.InlCalls dwcalls = ref _addr_dwcalls.val;

    var (callIdx, found) = imap[inlIdx];
    if (found) {
        return callIdx;
    }
    nint parCallIdx = -1;
    var parInlIdx = @base.Ctxt.InlTree.Parent(inlIdx);
    if (parInlIdx >= 0) {
        parCallIdx = insertInlCall(_addr_dwcalls, parInlIdx, imap);
    }
    var inlinedFn = @base.Ctxt.InlTree.InlinedFunction(inlIdx);
    var callXPos = @base.Ctxt.InlTree.CallPos(inlIdx);
    var absFnSym = @base.Ctxt.DwFixups.AbsFuncDwarfSym(inlinedFn);
    var pb = @base.Ctxt.PosTable.Pos(callXPos).Base();
    var callFileSym = @base.Ctxt.Lookup(pb.SymFilename());
    dwarf.InlCall ic = new dwarf.InlCall(InlIndex:inlIdx,CallFile:callFileSym,CallLine:uint32(callXPos.Line()),AbsFunSym:absFnSym,Root:parCallIdx==-1,);
    dwcalls.Calls = append(dwcalls.Calls, ic);
    callIdx = len(dwcalls.Calls) - 1;
    imap[inlIdx] = callIdx;

    if (parCallIdx != -1) { 
        // Add this inline to parent's child list
        dwcalls.Calls[parCallIdx].Children = append(dwcalls.Calls[parCallIdx].Children, callIdx);

    }
    return callIdx;

}

// Given a src.XPos, return its associated inlining index if it
// corresponds to something created as a result of an inline, or -1 if
// there is no inline info. Note that the index returned will refer to
// the deepest call in the inlined stack, e.g. if you have "A calls B
// calls C calls D" and all three callees are inlined (B, C, and D),
// the index for a node from the inlined body of D will refer to the
// call to D from C. Whew.
private static nint posInlIndex(src.XPos xpos) {
    var pos = @base.Ctxt.PosTable.Pos(xpos);
    {
        var b = pos.Base();

        if (b != null) {
            var ii = b.InliningIndex();
            if (ii >= 0) {
                return ii;
            }
        }
    }

    return -1;

}

private static void addRange(slice<dwarf.InlCall> calls, long start, long end, nint ii, map<nint, nint> imap) => func((_, panic, _) => {
    if (start == -1) {
        panic("bad range start");
    }
    if (end == -1) {
        panic("bad range end");
    }
    if (ii == -1) {
        return ;
    }
    if (start == end) {
        return ;
    }
    var (callIdx, found) = imap[ii];
    if (!found) {
        @base.Fatalf("can't find inlIndex %d in imap for prog at %d\n", ii, start);
    }
    var call = _addr_calls[callIdx];
    call.Ranges = append(call.Ranges, new dwarf.Range(Start:start,End:end));

});

private static void dumpInlCall(dwarf.InlCalls inlcalls, nint idx, nint ilevel) {
    for (nint i = 0; i < ilevel; i++) {
        @base.Ctxt.Logf("  ");
    }
    var ic = inlcalls.Calls[idx];
    var callee = @base.Ctxt.InlTree.InlinedFunction(ic.InlIndex);
    @base.Ctxt.Logf("  %d: II:%d (%s) V: (", idx, ic.InlIndex, callee.Name);
    foreach (var (_, f) in ic.InlVars) {
        @base.Ctxt.Logf(" %v", f.Name);
    }    @base.Ctxt.Logf(" ) C: (");
    {
        var k__prev1 = k;

        foreach (var (_, __k) in ic.Children) {
            k = __k;
            @base.Ctxt.Logf(" %v", k);
        }
        k = k__prev1;
    }

    @base.Ctxt.Logf(" ) R:");
    foreach (var (_, r) in ic.Ranges) {
        @base.Ctxt.Logf(" [%d,%d)", r.Start, r.End);
    }    @base.Ctxt.Logf("\n");
    {
        var k__prev1 = k;

        foreach (var (_, __k) in ic.Children) {
            k = __k;
            dumpInlCall(inlcalls, k, ilevel + 1);
        }
        k = k__prev1;
    }
}

private static void dumpInlCalls(dwarf.InlCalls inlcalls) {
    foreach (var (k, c) in inlcalls.Calls) {
        if (c.Root) {
            dumpInlCall(inlcalls, k, 0);
        }
    }
}

private static void dumpInlVars(slice<ptr<dwarf.Var>> dwvars) {
    foreach (var (i, dwv) in dwvars) {
        @string typ = "local";
        if (dwv.Abbrev == dwarf.DW_ABRV_PARAM_LOCLIST || dwv.Abbrev == dwarf.DW_ABRV_PARAM) {
            typ = "param";
        }
        nint ia = 0;
        if (dwv.IsInAbstract) {
            ia = 1;
        }
        @base.Ctxt.Logf("V%d: %s CI:%d II:%d IA:%d %s\n", i, dwv.Name, dwv.ChildIndex, dwv.InlIndex - 1, ia, typ);

    }
}

private static (bool, @string) rangesContains(slice<dwarf.Range> par, dwarf.Range rng) {
    bool _p0 = default;
    @string _p0 = default;

    {
        var r__prev1 = r;

        foreach (var (_, __r) in par) {
            r = __r;
            if (rng.Start >= r.Start && rng.End <= r.End) {
                return (true, "");
            }
        }
        r = r__prev1;
    }

    var msg = fmt.Sprintf("range [%d,%d) not contained in {", rng.Start, rng.End);
    {
        var r__prev1 = r;

        foreach (var (_, __r) in par) {
            r = __r;
            msg += fmt.Sprintf(" [%d,%d)", r.Start, r.End);
        }
        r = r__prev1;
    }

    msg += " }";
    return (false, msg);

}

private static (bool, @string) rangesContainsAll(slice<dwarf.Range> parent, slice<dwarf.Range> child) {
    bool _p0 = default;
    @string _p0 = default;

    foreach (var (_, r) in child) {
        var (c, m) = rangesContains(parent, r);
        if (!c) {
            return (false, m);
        }
    }    return (true, "");

}

// checkInlCall verifies that the PC ranges for inline info 'idx' are
// enclosed/contained within the ranges of its parent inline (or if
// this is a root/toplevel inline, checks that the ranges fall within
// the extent of the top level function). A panic is issued if a
// malformed range is found.
private static void checkInlCall(@string funcName, dwarf.InlCalls inlCalls, long funcSize, nint idx, nint parentIdx) {
    // Callee
    var ic = inlCalls.Calls[idx];
    var callee = @base.Ctxt.InlTree.InlinedFunction(ic.InlIndex).Name;
    var calleeRanges = ic.Ranges; 

    // Caller
    var caller = funcName;
    dwarf.Range parentRanges = new slice<dwarf.Range>(new dwarf.Range[] { dwarf.Range{Start:int64(0),End:funcSize} });
    if (parentIdx != -1) {
        var pic = inlCalls.Calls[parentIdx];
        caller = @base.Ctxt.InlTree.InlinedFunction(pic.InlIndex).Name;
        parentRanges = pic.Ranges;
    }
    var (c, m) = rangesContainsAll(parentRanges, calleeRanges);
    if (!c) {
        @base.Fatalf("** malformed inlined routine range in %s: caller %s callee %s II=%d %s\n", funcName, caller, callee, idx, m);
    }
    foreach (var (_, k) in ic.Children) {
        checkInlCall(funcName, inlCalls, funcSize, k, idx);
    }
}

// unifyCallRanges ensures that the ranges for a given inline
// transitively include all of the ranges for its child inlines.
private static void unifyCallRanges(dwarf.InlCalls inlcalls, nint idx) {
    var ic = _addr_inlcalls.Calls[idx];
    foreach (var (_, childIdx) in ic.Children) { 
        // First make sure child ranges are unified.
        unifyCallRanges(inlcalls, childIdx); 

        // Then merge child ranges into ranges for this inline.
        var cic = inlcalls.Calls[childIdx];
        ic.Ranges = dwarf.MergeRanges(ic.Ranges, cic.Ranges);

    }
}

} // end dwarfgen_package
