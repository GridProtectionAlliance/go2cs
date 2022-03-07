// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package dwarfgen -- go2cs converted at 2022 March 06 23:12:15 UTC
// import "cmd/compile/internal/dwarfgen" ==> using dwarfgen = go.cmd.compile.@internal.dwarfgen_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\dwarfgen\dwarf.go
using bytes = go.bytes_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using sort = go.sort_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using reflectdata = go.cmd.compile.@internal.reflectdata_package;
using ssa = go.cmd.compile.@internal.ssa_package;
using ssagen = go.cmd.compile.@internal.ssagen_package;
using types = go.cmd.compile.@internal.types_package;
using dwarf = go.cmd.@internal.dwarf_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class dwarfgen_package {

public static (slice<dwarf.Scope>, dwarf.InlCalls) Info(ptr<obj.LSym> _addr_fnsym, ptr<obj.LSym> _addr_infosym, object curfn) {
    slice<dwarf.Scope> _p0 = default;
    dwarf.InlCalls _p0 = default;
    ref obj.LSym fnsym = ref _addr_fnsym.val;
    ref obj.LSym infosym = ref _addr_infosym.val;

    ptr<ir.Func> fn = curfn._<ptr<ir.Func>>();

    if (fn.Nname != null) {
        var expect = fn.Linksym();
        if (fnsym.ABI() == obj.ABI0) {
            expect = fn.LinksymABI(obj.ABI0);
        }
        if (fnsym != expect) {
            @base.Fatalf("unexpected fnsym: %v != %v", fnsym, expect);
        }
    }
    var isODCLFUNC = infosym.Name == "";

    slice<ptr<ir.Name>> apdecls = default; 
    // Populate decls for fn.
    if (isODCLFUNC) {
        foreach (var (_, n) in fn.Dcl) {
            if (n.Op() != ir.ONAME) { // might be OTYPE or OLITERAL
                continue;

            }

            if (n.Class == ir.PAUTO) 
                if (!n.Used()) { 
                    // Text == nil -> generating abstract function
                    if (fnsym.Func().Text != null) {
                        @base.Fatalf("debuginfo unused node (AllocFrame should truncate fn.Func.Dcl)");
                    }
                    continue;

                }
            else if (n.Class == ir.PPARAM || n.Class == ir.PPARAMOUT)             else 
                continue;
                        apdecls = append(apdecls, n);
            fnsym.Func().RecordAutoType(reflectdata.TypeLinksym(n.Type()));

        }
    }
    var (decls, dwarfVars) = createDwarfVars(_addr_fnsym, isODCLFUNC, fn, apdecls); 

    // For each type referenced by the functions auto vars but not
    // already referenced by a dwarf var, attach an R_USETYPE relocation to
    // the function symbol to insure that the type included in DWARF
    // processing during linking.
    ptr<obj.LSym> typesyms = new slice<ptr<obj.LSym>>(new ptr<obj.LSym>[] {  });
    foreach (var (t, _) in fnsym.Func().Autot) {
        typesyms = append(typesyms, t);
    }    sort.Sort(obj.BySymName(typesyms));
    foreach (var (_, sym) in typesyms) {
        var r = obj.Addrel(infosym);
        r.Sym = sym;
        r.Type = objabi.R_USETYPE;
    }    fnsym.Func().Autot = null;

    slice<ir.ScopeID> varScopes = default;
    foreach (var (_, decl) in decls) {
        var pos = declPos(_addr_decl);
        varScopes = append(varScopes, findScope(fn.Marks, pos));
    }    var scopes = assembleScopes(fnsym, fn, dwarfVars, varScopes);
    dwarf.InlCalls inlcalls = default;
    if (@base.Flag.GenDwarfInl > 0) {
        inlcalls = assembleInlines(fnsym, dwarfVars);
    }
    return (scopes, inlcalls);

}

private static src.XPos declPos(ptr<ir.Name> _addr_decl) {
    ref ir.Name decl = ref _addr_decl.val;

    return decl.Canonical().Pos();
}

// createDwarfVars process fn, returning a list of DWARF variables and the
// Nodes they represent.
private static (slice<ptr<ir.Name>>, slice<ptr<dwarf.Var>>) createDwarfVars(ptr<obj.LSym> _addr_fnsym, bool complexOK, ptr<ir.Func> _addr_fn, slice<ptr<ir.Name>> apDecls) {
    slice<ptr<ir.Name>> _p0 = default;
    slice<ptr<dwarf.Var>> _p0 = default;
    ref obj.LSym fnsym = ref _addr_fnsym.val;
    ref ir.Func fn = ref _addr_fn.val;
 
    // Collect a raw list of DWARF vars.
    slice<ptr<dwarf.Var>> vars = default;
    slice<ptr<ir.Name>> decls = default;
    ir.NameSet selected = default;

    if (@base.Ctxt.Flag_locationlists && @base.Ctxt.Flag_optimize && fn.DebugInfo != null && complexOK) {
        decls, vars, selected = createComplexVars(_addr_fnsym, _addr_fn);
    }
    else if (fn.ABI == obj.ABIInternal && @base.Flag.N != 0 && complexOK) {
        decls, vars, selected = createABIVars(_addr_fnsym, _addr_fn, apDecls);
    }
    else
 {
        decls, vars, selected = createSimpleVars(_addr_fnsym, apDecls);
    }
    var dcl = apDecls;
    if (fnsym.WasInlined()) {
        dcl = preInliningDcls(_addr_fnsym);
    }
    foreach (var (_, n) in dcl) {
        if (selected.Has(n)) {
            continue;
        }
        var c = n.Sym().Name[0];
        if (c == '.' || n.Type().IsUntyped()) {
            continue;
        }
        if (n.Class == ir.PPARAM && !ssagen.TypeOK(n.Type())) { 
            // SSA-able args get location lists, and may move in and
            // out of registers, so those are handled elsewhere.
            // Autos and named output params seem to get handled
            // with VARDEF, which creates location lists.
            // Args not of SSA-able type are treated here; they
            // are homed on the stack in a single place for the
            // entire call.
            vars = append(vars, createSimpleVar(_addr_fnsym, _addr_n));
            decls = append(decls, n);
            continue;

        }
        var typename = dwarf.InfoPrefix + types.TypeSymName(n.Type());
        decls = append(decls, n);
        var abbrev = dwarf.DW_ABRV_AUTO_LOCLIST;
        var isReturnValue = (n.Class == ir.PPARAMOUT);
        if (n.Class == ir.PPARAM || n.Class == ir.PPARAMOUT) {
            abbrev = dwarf.DW_ABRV_PARAM_LOCLIST;
        }
        if (n.Esc() == ir.EscHeap) { 
            // The variable in question has been promoted to the heap.
            // Its address is in n.Heapaddr.
            // TODO(thanm): generate a better location expression
        }
        nint inlIndex = 0;
        if (@base.Flag.GenDwarfInl > 1) {
            if (n.InlFormal() || n.InlLocal()) {
                inlIndex = posInlIndex(n.Pos()) + 1;
                if (n.InlFormal()) {
                    abbrev = dwarf.DW_ABRV_PARAM_LOCLIST;
                }
            }
        }
        var declpos = @base.Ctxt.InnermostPos(n.Pos());
        vars = append(vars, addr(new dwarf.Var(Name:n.Sym().Name,IsReturnValue:isReturnValue,Abbrev:abbrev,StackOffset:int32(n.FrameOffset()),Type:base.Ctxt.Lookup(typename),DeclFile:declpos.RelFilename(),DeclLine:declpos.RelLine(),DeclCol:declpos.Col(),InlIndex:int32(inlIndex),ChildIndex:-1,))); 
        // Record go type of to insure that it gets emitted by the linker.
        fnsym.Func().RecordAutoType(reflectdata.TypeLinksym(n.Type()));

    }    sortDeclsAndVars(_addr_fn, decls, vars);

    return (decls, vars);

}

// sortDeclsAndVars sorts the decl and dwarf var lists according to
// parameter declaration order, so as to insure that when a subprogram
// DIE is emitted, its parameter children appear in declaration order.
// Prior to the advent of the register ABI, sorting by frame offset
// would achieve this; with the register we now need to go back to the
// original function signature.
private static void sortDeclsAndVars(ptr<ir.Func> _addr_fn, slice<ptr<ir.Name>> decls, slice<ptr<dwarf.Var>> vars) {
    ref ir.Func fn = ref _addr_fn.val;

    var paramOrder = make_map<ptr<ir.Name>, nint>();
    nint idx = 1;
    foreach (var (_, selfn) in types.RecvsParamsResults) {
        var fsl = selfn(fn.Type()).FieldSlice();
        foreach (var (_, f) in fsl) {
            {
                ptr<ir.Name> (n, ok) = f.Nname._<ptr<ir.Name>>();

                if (ok) {
                    paramOrder[n] = idx;
                    idx++;
                }

            }

        }
    }    sort.Stable(new varsAndDecls(decls,vars,paramOrder));

}

private partial struct varsAndDecls {
    public slice<ptr<ir.Name>> decls;
    public slice<ptr<dwarf.Var>> vars;
    public map<ptr<ir.Name>, nint> paramOrder;
}

private static nint Len(this varsAndDecls v) {
    return len(v.decls);
}

private static bool Less(this varsAndDecls v, nint i, nint j) {
    Func<ptr<ir.Name>, ptr<ir.Name>, bool> nameLT = (ni, nj) => {
        var (oi, foundi) = v.paramOrder[ni];
        var (oj, foundj) = v.paramOrder[nj];
        if (foundi) {
            if (foundj) {
                return oi < oj;
            }
            else
 {
                return true;
            }

        }
        return false;

    };
    return nameLT(v.decls[i], v.decls[j]);

}

private static void Swap(this varsAndDecls v, nint i, nint j) {
    (v.vars[i], v.vars[j]) = (v.vars[j], v.vars[i]);    (v.decls[i], v.decls[j]) = (v.decls[j], v.decls[i]);
}

// Given a function that was inlined at some point during the
// compilation, return a sorted list of nodes corresponding to the
// autos/locals in that function prior to inlining. If this is a
// function that is not local to the package being compiled, then the
// names of the variables may have been "versioned" to avoid conflicts
// with local vars; disregard this versioning when sorting.
private static slice<ptr<ir.Name>> preInliningDcls(ptr<obj.LSym> _addr_fnsym) {
    ref obj.LSym fnsym = ref _addr_fnsym.val;

    ptr<ir.Func> fn = @base.Ctxt.DwFixups.GetPrecursorFunc(fnsym)._<ptr<ir.Func>>();
    slice<ptr<ir.Name>> rdcl = default;
    foreach (var (_, n) in fn.Inl.Dcl) {
        var c = n.Sym().Name[0]; 
        // Avoid reporting "_" parameters, since if there are more than
        // one, it can result in a collision later on, as in #23179.
        if (unversion(n.Sym().Name) == "_" || c == '.' || n.Type().IsUntyped()) {
            continue;
        }
        rdcl = append(rdcl, n);

    }    return rdcl;

}

// createSimpleVars creates a DWARF entry for every variable declared in the
// function, claiming that they are permanently on the stack.
private static (slice<ptr<ir.Name>>, slice<ptr<dwarf.Var>>, ir.NameSet) createSimpleVars(ptr<obj.LSym> _addr_fnsym, slice<ptr<ir.Name>> apDecls) {
    slice<ptr<ir.Name>> _p0 = default;
    slice<ptr<dwarf.Var>> _p0 = default;
    ir.NameSet _p0 = default;
    ref obj.LSym fnsym = ref _addr_fnsym.val;

    slice<ptr<dwarf.Var>> vars = default;
    slice<ptr<ir.Name>> decls = default;
    ir.NameSet selected = default;
    foreach (var (_, n) in apDecls) {
        if (ir.IsAutoTmp(n)) {
            continue;
        }
        decls = append(decls, n);
        vars = append(vars, createSimpleVar(_addr_fnsym, _addr_n));
        selected.Add(n);

    }    return (decls, vars, selected);

}

private static ptr<dwarf.Var> createSimpleVar(ptr<obj.LSym> _addr_fnsym, ptr<ir.Name> _addr_n) {
    ref obj.LSym fnsym = ref _addr_fnsym.val;
    ref ir.Name n = ref _addr_n.val;

    nint abbrev = default;
    long offs = default;

    Func<long> localAutoOffset = () => {
        offs = n.FrameOffset();
        if (@base.Ctxt.FixedFrameSize() == 0) {
            offs -= int64(types.PtrSize);
        }
        if (buildcfg.FramePointerEnabled) {
            offs -= int64(types.PtrSize);
        }
        return _addr_offs!;

    };


    if (n.Class == ir.PAUTO) 
        offs = localAutoOffset();
        abbrev = dwarf.DW_ABRV_AUTO;
    else if (n.Class == ir.PPARAM || n.Class == ir.PPARAMOUT) 
        abbrev = dwarf.DW_ABRV_PARAM;
        if (n.IsOutputParamInRegisters()) {
            offs = localAutoOffset();
        }
        else
 {
            offs = n.FrameOffset() + @base.Ctxt.FixedFrameSize();
        }
    else 
        @base.Fatalf("createSimpleVar unexpected class %v for node %v", n.Class, n);
        var typename = dwarf.InfoPrefix + types.TypeSymName(n.Type());
    delete(fnsym.Func().Autot, reflectdata.TypeLinksym(n.Type()));
    nint inlIndex = 0;
    if (@base.Flag.GenDwarfInl > 1) {
        if (n.InlFormal() || n.InlLocal()) {
            inlIndex = posInlIndex(n.Pos()) + 1;
            if (n.InlFormal()) {
                abbrev = dwarf.DW_ABRV_PARAM;
            }
        }
    }
    var declpos = @base.Ctxt.InnermostPos(declPos(_addr_n));
    return addr(new dwarf.Var(Name:n.Sym().Name,IsReturnValue:n.Class==ir.PPARAMOUT,IsInlFormal:n.InlFormal(),Abbrev:abbrev,StackOffset:int32(offs),Type:base.Ctxt.Lookup(typename),DeclFile:declpos.RelFilename(),DeclLine:declpos.RelLine(),DeclCol:declpos.Col(),InlIndex:int32(inlIndex),ChildIndex:-1,));

}

// createABIVars creates DWARF variables for functions in which the
// register ABI is enabled but optimization is turned off. It uses a
// hybrid approach in which register-resident input params are
// captured with location lists, and all other vars use the "simple"
// strategy.
private static (slice<ptr<ir.Name>>, slice<ptr<dwarf.Var>>, ir.NameSet) createABIVars(ptr<obj.LSym> _addr_fnsym, ptr<ir.Func> _addr_fn, slice<ptr<ir.Name>> apDecls) {
    slice<ptr<ir.Name>> _p0 = default;
    slice<ptr<dwarf.Var>> _p0 = default;
    ir.NameSet _p0 = default;
    ref obj.LSym fnsym = ref _addr_fnsym.val;
    ref ir.Func fn = ref _addr_fn.val;

    // Invoke createComplexVars to generate dwarf vars for input parameters
    // that are register-allocated according to the ABI rules.
    var (decls, vars, selected) = createComplexVars(_addr_fnsym, _addr_fn); 

    // Now fill in the remainder of the variables: input parameters
    // that are not register-resident, output parameters, and local
    // variables.
    foreach (var (_, n) in apDecls) {
        if (ir.IsAutoTmp(n)) {
            continue;
        }
        {
            var (_, ok) = selected[n];

            if (ok) { 
                // already handled
                continue;

            }

        }


        decls = append(decls, n);
        vars = append(vars, createSimpleVar(_addr_fnsym, _addr_n));
        selected.Add(n);

    }    return (decls, vars, selected);

}

// createComplexVars creates recomposed DWARF vars with location lists,
// suitable for describing optimized code.
private static (slice<ptr<ir.Name>>, slice<ptr<dwarf.Var>>, ir.NameSet) createComplexVars(ptr<obj.LSym> _addr_fnsym, ptr<ir.Func> _addr_fn) {
    slice<ptr<ir.Name>> _p0 = default;
    slice<ptr<dwarf.Var>> _p0 = default;
    ir.NameSet _p0 = default;
    ref obj.LSym fnsym = ref _addr_fnsym.val;
    ref ir.Func fn = ref _addr_fn.val;

    ptr<ssa.FuncDebug> debugInfo = fn.DebugInfo._<ptr<ssa.FuncDebug>>(); 

    // Produce a DWARF variable entry for each user variable.
    slice<ptr<ir.Name>> decls = default;
    slice<ptr<dwarf.Var>> vars = default;
    ir.NameSet ssaVars = default;

    {
        var dvar__prev1 = dvar;

        foreach (var (__varID, __dvar) in debugInfo.Vars) {
            varID = __varID;
            dvar = __dvar;
            var n = dvar;
            ssaVars.Add(n);
            foreach (var (_, slot) in debugInfo.VarSlots[varID]) {
                ssaVars.Add(debugInfo.Slots[slot].N);
            }
            {
                var dvar__prev1 = dvar;

                var dvar = createComplexVar(_addr_fnsym, _addr_fn, ssa.VarID(varID));

                if (dvar != null) {
                    decls = append(decls, n);
                    vars = append(vars, dvar);
                }

                dvar = dvar__prev1;

            }

        }
        dvar = dvar__prev1;
    }

    return (decls, vars, ssaVars);

}

// createComplexVar builds a single DWARF variable entry and location list.
private static ptr<dwarf.Var> createComplexVar(ptr<obj.LSym> _addr_fnsym, ptr<ir.Func> _addr_fn, ssa.VarID varID) {
    ref obj.LSym fnsym = ref _addr_fnsym.val;
    ref ir.Func fn = ref _addr_fn.val;

    ptr<ssa.FuncDebug> debug = fn.DebugInfo._<ptr<ssa.FuncDebug>>();
    var n = debug.Vars[varID];

    nint abbrev = default;

    if (n.Class == ir.PAUTO) 
        abbrev = dwarf.DW_ABRV_AUTO_LOCLIST;
    else if (n.Class == ir.PPARAM || n.Class == ir.PPARAMOUT) 
        abbrev = dwarf.DW_ABRV_PARAM_LOCLIST;
    else 
        return _addr_null!;
        var gotype = reflectdata.TypeLinksym(n.Type());
    delete(fnsym.Func().Autot, gotype);
    var typename = dwarf.InfoPrefix + gotype.Name[(int)len("type.")..];
    nint inlIndex = 0;
    if (@base.Flag.GenDwarfInl > 1) {
        if (n.InlFormal() || n.InlLocal()) {
            inlIndex = posInlIndex(n.Pos()) + 1;
            if (n.InlFormal()) {
                abbrev = dwarf.DW_ABRV_PARAM_LOCLIST;
            }
        }
    }
    var declpos = @base.Ctxt.InnermostPos(n.Pos());
    ptr<dwarf.Var> dvar = addr(new dwarf.Var(Name:n.Sym().Name,IsReturnValue:n.Class==ir.PPARAMOUT,IsInlFormal:n.InlFormal(),Abbrev:abbrev,Type:base.Ctxt.Lookup(typename),StackOffset:ssagen.StackOffset(debug.Slots[debug.VarSlots[varID][0]]),DeclFile:declpos.RelFilename(),DeclLine:declpos.RelLine(),DeclCol:declpos.Col(),InlIndex:int32(inlIndex),ChildIndex:-1,));
    var list = debug.LocationLists[varID];
    if (len(list) != 0) {
        dvar.PutLocationList = (listSym, startPC) => {
            debug.PutLocationList(list, @base.Ctxt, listSym._<ptr<obj.LSym>>(), startPC._<ptr<obj.LSym>>());
        };
    }
    return _addr_dvar!;

}

// RecordFlags records the specified command-line flags to be placed
// in the DWARF info.
public static void RecordFlags(params @string[] flags) {
    flags = flags.Clone();

    if (@base.Ctxt.Pkgpath == "") { 
        // We can't record the flags if we don't know what the
        // package name is.
        return ;

    }
    public partial interface BoolFlag {
        bool IsBoolFlag();
    }
    public partial interface CountFlag {
        bool IsCountFlag();
    }
    ref bytes.Buffer cmd = ref heap(out ptr<bytes.Buffer> _addr_cmd);
    foreach (var (_, name) in flags) {
        var f = flag.Lookup(name);
        if (f == null) {
            continue;
        }
        flag.Getter getter = f.Value._<flag.Getter>();
        if (getter.String() == f.DefValue) { 
            // Flag has default value, so omit it.
            continue;

        }
        {
            BoolFlag (bf, ok) = BoolFlag.As(f.Value._<BoolFlag>())!;

            if (ok && bf.IsBoolFlag()) {
                bool (val, ok) = getter.Get()._<bool>();
                if (ok && val) {
                    fmt.Fprintf(_addr_cmd, " -%s", f.Name);
                    continue;
                }
            }

        }

        {
            CountFlag (cf, ok) = CountFlag.As(f.Value._<CountFlag>())!;

            if (ok && cf.IsCountFlag()) {
                (val, ok) = getter.Get()._<nint>();
                if (ok && val == 1) {
                    fmt.Fprintf(_addr_cmd, " -%s", f.Name);
                    continue;
                }
            }

        }

        fmt.Fprintf(_addr_cmd, " -%s=%v", f.Name, getter.Get());

    }    if (buildcfg.Experiment.RegabiArgs) {
        cmd.Write((slice<byte>)" regabi");
    }
    if (cmd.Len() == 0) {
        return ;
    }
    var s = @base.Ctxt.Lookup(dwarf.CUInfoPrefix + "producer." + @base.Ctxt.Pkgpath);
    s.Type = objabi.SDWARFCUINFO; 
    // Sometimes (for example when building tests) we can link
    // together two package main archives. So allow dups.
    s.Set(obj.AttrDuplicateOK, true);
    @base.Ctxt.Data = append(@base.Ctxt.Data, s);
    s.P = cmd.Bytes()[(int)1..];

}

// RecordPackageName records the name of the package being
// compiled, so that the linker can save it in the compile unit's DIE.
public static void RecordPackageName() {
    var s = @base.Ctxt.Lookup(dwarf.CUInfoPrefix + "packagename." + @base.Ctxt.Pkgpath);
    s.Type = objabi.SDWARFCUINFO; 
    // Sometimes (for example when building tests) we can link
    // together two package main archives. So allow dups.
    s.Set(obj.AttrDuplicateOK, true);
    @base.Ctxt.Data = append(@base.Ctxt.Data, s);
    s.P = (slice<byte>)types.LocalPkg.Name;

}

} // end dwarfgen_package
