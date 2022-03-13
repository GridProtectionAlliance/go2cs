// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package dwarfgen -- go2cs converted at 2022 March 13 06:27:54 UTC
// import "cmd/compile/internal/dwarfgen" ==> using dwarfgen = go.cmd.compile.@internal.dwarfgen_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\dwarfgen\scope.go
namespace go.cmd.compile.@internal;

using sort = sort_package;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using dwarf = cmd.@internal.dwarf_package;
using obj = cmd.@internal.obj_package;
using src = cmd.@internal.src_package;


// See golang.org/issue/20390.

using System;
public static partial class dwarfgen_package {

private static bool xposBefore(src.XPos p, src.XPos q) {
    return @base.Ctxt.PosTable.Pos(p).Before(@base.Ctxt.PosTable.Pos(q));
}

private static ir.ScopeID findScope(slice<ir.Mark> marks, src.XPos pos) {
    var i = sort.Search(len(marks), i => xposBefore(pos, marks[i].Pos));
    if (i == 0) {
        return 0;
    }
    return marks[i - 1].Scope;
}

private static slice<dwarf.Scope> assembleScopes(ptr<obj.LSym> _addr_fnsym, ptr<ir.Func> _addr_fn, slice<ptr<dwarf.Var>> dwarfVars, slice<ir.ScopeID> varScopes) {
    ref obj.LSym fnsym = ref _addr_fnsym.val;
    ref ir.Func fn = ref _addr_fn.val;
 
    // Initialize the DWARF scope tree based on lexical scopes.
    var dwarfScopes = make_slice<dwarf.Scope>(1 + len(fn.Parents));
    foreach (var (i, parent) in fn.Parents) {
        dwarfScopes[i + 1].Parent = int32(parent);
    }    scopeVariables(dwarfVars, varScopes, dwarfScopes, fnsym.ABI() != obj.ABI0);
    if (fnsym.Func().Text != null) {
        scopePCs(_addr_fnsym, fn.Marks, dwarfScopes);
    }
    return compactScopes(dwarfScopes);
}

// scopeVariables assigns DWARF variable records to their scopes.
private static void scopeVariables(slice<ptr<dwarf.Var>> dwarfVars, slice<ir.ScopeID> varScopes, slice<dwarf.Scope> dwarfScopes, bool regabi) {
    if (regabi) {
        sort.Stable(new varsByScope(dwarfVars,varScopes));
    }
    else
 {
        sort.Stable(new varsByScopeAndOffset(dwarfVars,varScopes));
    }
    nint i0 = 0;
    foreach (var (i) in dwarfVars) {
        if (varScopes[i] == varScopes[i0]) {
            continue;
        }
        dwarfScopes[varScopes[i0]].Vars = dwarfVars[(int)i0..(int)i];
        i0 = i;
    }    if (i0 < len(dwarfVars)) {
        dwarfScopes[varScopes[i0]].Vars = dwarfVars[(int)i0..];
    }
}

// scopePCs assigns PC ranges to their scopes.
private static void scopePCs(ptr<obj.LSym> _addr_fnsym, slice<ir.Mark> marks, slice<dwarf.Scope> dwarfScopes) {
    ref obj.LSym fnsym = ref _addr_fnsym.val;
 
    // If there aren't any child scopes (in particular, when scope
    // tracking is disabled), we can skip a whole lot of work.
    if (len(marks) == 0) {
        return ;
    }
    var p0 = fnsym.Func().Text;
    var scope = findScope(marks, p0.Pos);
    {
        var p = p0;

        while (p != null) {
            if (p.Pos == p0.Pos) {
                continue;
            p = p.Link;
            }
            dwarfScopes[scope].AppendRange(new dwarf.Range(Start:p0.Pc,End:p.Pc));
            p0 = p;
            scope = findScope(marks, p0.Pos);
        }
    }
    if (p0.Pc < fnsym.Size) {
        dwarfScopes[scope].AppendRange(new dwarf.Range(Start:p0.Pc,End:fnsym.Size));
    }
}

private static slice<dwarf.Scope> compactScopes(slice<dwarf.Scope> dwarfScopes) { 
    // Reverse pass to propagate PC ranges to parent scopes.
    for (var i = len(dwarfScopes) - 1; i > 0; i--) {
        var s = _addr_dwarfScopes[i];
        dwarfScopes[s.Parent].UnifyRanges(s);
    }

    return dwarfScopes;
}

private partial struct varsByScopeAndOffset {
    public slice<ptr<dwarf.Var>> vars;
    public slice<ir.ScopeID> scopes;
}

private static nint Len(this varsByScopeAndOffset v) {
    return len(v.vars);
}

private static bool Less(this varsByScopeAndOffset v, nint i, nint j) {
    if (v.scopes[i] != v.scopes[j]) {
        return v.scopes[i] < v.scopes[j];
    }
    return v.vars[i].StackOffset < v.vars[j].StackOffset;
}

private static void Swap(this varsByScopeAndOffset v, nint i, nint j) {
    (v.vars[i], v.vars[j]) = (v.vars[j], v.vars[i]);    (v.scopes[i], v.scopes[j]) = (v.scopes[j], v.scopes[i]);
}

private partial struct varsByScope {
    public slice<ptr<dwarf.Var>> vars;
    public slice<ir.ScopeID> scopes;
}

private static nint Len(this varsByScope v) {
    return len(v.vars);
}

private static bool Less(this varsByScope v, nint i, nint j) {
    return v.scopes[i] < v.scopes[j];
}

private static void Swap(this varsByScope v, nint i, nint j) {
    (v.vars[i], v.vars[j]) = (v.vars[j], v.vars[i]);    (v.scopes[i], v.scopes[j]) = (v.scopes[j], v.scopes[i]);
}

} // end dwarfgen_package
