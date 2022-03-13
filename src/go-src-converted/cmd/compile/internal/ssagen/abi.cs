// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssagen -- go2cs converted at 2022 March 13 05:58:52 UTC
// import "cmd/compile/internal/ssagen" ==> using ssagen = go.cmd.compile.@internal.ssagen_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssagen\abi.go
namespace go.cmd.compile.@internal;

using fmt = fmt_package;
using buildcfg = @internal.buildcfg_package;
using ioutil = io.ioutil_package;
using log = log_package;
using os = os_package;
using strings = strings_package;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using staticdata = cmd.compile.@internal.staticdata_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;
using obj = cmd.@internal.obj_package;
using objabi = cmd.@internal.objabi_package;


// SymABIs records information provided by the assembler about symbol
// definition ABIs and reference ABIs.

using System;
public static partial class ssagen_package {

public partial struct SymABIs {
    public map<@string, obj.ABI> defs;
    public map<@string, obj.ABISet> refs;
    public @string localPrefix;
}

public static ptr<SymABIs> NewSymABIs(@string myimportpath) {
    @string localPrefix = default;
    if (myimportpath != "") {
        localPrefix = objabi.PathToPrefix(myimportpath) + ".";
    }
    return addr(new SymABIs(defs:make(map[string]obj.ABI),refs:make(map[string]obj.ABISet),localPrefix:localPrefix,));
}

// canonicalize returns the canonical name used for a linker symbol in
// s's maps. Symbols in this package may be written either as "".X or
// with the package's import path already in the symbol. This rewrites
// both to `"".`, which matches compiler-generated linker symbol names.
private static @string canonicalize(this ptr<SymABIs> _addr_s, @string linksym) {
    ref SymABIs s = ref _addr_s.val;
 
    // If the symbol is already prefixed with localPrefix,
    // rewrite it to start with "" so it matches the
    // compiler's internal symbol names.
    if (s.localPrefix != "" && strings.HasPrefix(linksym, s.localPrefix)) {
        return "\"\"." + linksym[(int)len(s.localPrefix)..];
    }
    return linksym;
}

// ReadSymABIs reads a symabis file that specifies definitions and
// references of text symbols by ABI.
//
// The symabis format is a set of lines, where each line is a sequence
// of whitespace-separated fields. The first field is a verb and is
// either "def" for defining a symbol ABI or "ref" for referencing a
// symbol using an ABI. For both "def" and "ref", the second field is
// the symbol name and the third field is the ABI name, as one of the
// named cmd/internal/obj.ABI constants.
private static void ReadSymABIs(this ptr<SymABIs> _addr_s, @string file) {
    ref SymABIs s = ref _addr_s.val;

    var (data, err) = ioutil.ReadFile(file);
    if (err != null) {
        log.Fatalf("-symabis: %v", err);
    }
    foreach (var (lineNum, line) in strings.Split(string(data), "\n")) {
        lineNum++; // 1-based
        line = strings.TrimSpace(line);
        if (line == "" || strings.HasPrefix(line, "#")) {
            continue;
        }
        var parts = strings.Fields(line);
        switch (parts[0]) {
            case "def": 
                // Parse line.

            case "ref": 
                // Parse line.
                           if (len(parts) != 3) {
                               log.Fatalf("%s:%d: invalid symabi: syntax is \"%s sym abi\"", file, lineNum, parts[0]);
                           }
                           var sym = parts[1];
                           var abistr = parts[2];
                           var (abi, valid) = obj.ParseABI(abistr);
                           if (!valid) {
                               log.Fatalf("%s:%d: invalid symabi: unknown abi \"%s\"", file, lineNum, abistr);
                           }
                           sym = s.canonicalize(sym); 

                           // Record for later.
                           if (parts[0] == "def") {
                               s.defs[sym] = abi;
                           }
                           else
                {
                               s.refs[sym] |= obj.ABISetOf(abi);
                           }
                break;
            default: 
                log.Fatalf("%s:%d: invalid symabi type \"%s\"", file, lineNum, parts[0]);
                break;
        }
    }
}

// GenABIWrappers applies ABI information to Funcs and generates ABI
// wrapper functions where necessary.
private static void GenABIWrappers(this ptr<SymABIs> _addr_s) {
    ref SymABIs s = ref _addr_s.val;
 
    // For cgo exported symbols, we tell the linker to export the
    // definition ABI to C. That also means that we don't want to
    // create ABI wrappers even if there's a linkname.
    //
    // TODO(austin): Maybe we want to create the ABI wrappers, but
    // ensure the linker exports the right ABI definition under
    // the unmangled name?
    var cgoExports = make_map<@string, slice<ptr<slice<@string>>>>();
    foreach (var (i, prag) in typecheck.Target.CgoPragmas) {
        switch (prag[0]) {
            case "cgo_export_static": 

            case "cgo_export_dynamic": 
                var symName = s.canonicalize(prag[1]);
                var pprag = _addr_typecheck.Target.CgoPragmas[i];
                cgoExports[symName] = append(cgoExports[symName], pprag);
                break;
        }
    }    {
        var fn__prev1 = fn;

        foreach (var (_, __fn) in typecheck.Target.Decls) {
            fn = __fn;
            if (fn.Op() != ir.ODCLFUNC) {
                continue;
            }
            ptr<ir.Func> fn = fn._<ptr<ir.Func>>();
            var nam = fn.Nname;
            if (ir.IsBlank(nam)) {
                continue;
            }
            var sym = nam.Sym();
            symName = default;
            if (sym.Linkname != "") {
                symName = s.canonicalize(sym.Linkname);
            }
            else
 { 
                // These names will already be canonical.
                symName = sym.Pkg.Prefix + "." + sym.Name;
            } 

            // Apply definitions.
            var (defABI, hasDefABI) = s.defs[symName];
            if (hasDefABI) {
                fn.ABI = defABI;
            }
            if (fn.Pragma & ir.CgoUnsafeArgs != 0) { 
                // CgoUnsafeArgs indicates the function (or its callee) uses
                // offsets to dispatch arguments, which currently using ABI0
                // frame layout. Pin it to ABI0.
                fn.ABI = obj.ABI0;
            } 

            // If cgo-exported, add the definition ABI to the cgo
            // pragmas.
            var cgoExport = cgoExports[symName];
            {
                var pprag__prev2 = pprag;

                foreach (var (_, __pprag) in cgoExport) {
                    pprag = __pprag; 
                    // The export pragmas have the form:
                    //
                    //   cgo_export_* <local> [<remote>]
                    //
                    // If <remote> is omitted, it's the same as
                    // <local>.
                    //
                    // Expand to
                    //
                    //   cgo_export_* <local> <remote> <ABI>
                    if (len(pprag.val) == 2) {
                        pprag.val = append(pprag.val, (pprag.val)[1]);
                    } 
                    // Add the ABI argument.
                    pprag.val = append(pprag.val, fn.ABI.String());
                } 

                // Apply references.

                pprag = pprag__prev2;
            }

            {
                var (abis, ok) = s.refs[symName];

                if (ok) {
                    fn.ABIRefs |= abis;
                } 
                // Assume all functions are referenced at least as
                // ABIInternal, since they may be referenced from
                // other packages.

            } 
            // Assume all functions are referenced at least as
            // ABIInternal, since they may be referenced from
            // other packages.
            fn.ABIRefs.Set(obj.ABIInternal, true); 

            // If a symbol is defined in this package (either in
            // Go or assembly) and given a linkname, it may be
            // referenced from another package, so make it
            // callable via any ABI. It's important that we know
            // it's defined in this package since other packages
            // may "pull" symbols using linkname and we don't want
            // to create duplicate ABI wrappers.
            //
            // However, if it's given a linkname for exporting to
            // C, then we don't make ABI wrappers because the cgo
            // tool wants the original definition.
            var hasBody = len(fn.Body) != 0;
            if (sym.Linkname != "" && (hasBody || hasDefABI) && len(cgoExport) == 0) {
                fn.ABIRefs |= obj.ABISetCallable;
            } 

            // Double check that cgo-exported symbols don't get
            // any wrappers.
            if (len(cgoExport) > 0 && fn.ABIRefs & ~obj.ABISetOf(fn.ABI) != 0) {
                @base.Fatalf("cgo exported function %s cannot have ABI wrappers", fn);
            }
            if (!buildcfg.Experiment.RegabiWrappers) { 
                // We'll generate ABI aliases instead of
                // wrappers once we have LSyms in InitLSym.
                continue;
            }
            forEachWrapperABI(_addr_fn, makeABIWrapper);
        }
        fn = fn__prev1;
    }
}

// InitLSym defines f's obj.LSym and initializes it based on the
// properties of f. This includes setting the symbol flags and ABI and
// creating and initializing related DWARF symbols.
//
// InitLSym must be called exactly once per function and must be
// called for both functions with bodies and functions without bodies.
// For body-less functions, we only create the LSym; for functions
// with bodies call a helper to setup up / populate the LSym.
public static void InitLSym(ptr<ir.Func> _addr_f, bool hasBody) {
    ref ir.Func f = ref _addr_f.val;

    if (f.LSym != null) {
        @base.FatalfAt(f.Pos(), "InitLSym called twice on %v", f);
    }
    {
        var nam = f.Nname;

        if (!ir.IsBlank(nam)) {
            f.LSym = nam.LinksymABI(f.ABI);
            if (f.Pragma & ir.Systemstack != 0) {
                f.LSym.Set(obj.AttrCFunc, true);
            }
            if (f.ABI == obj.ABIInternal || !buildcfg.Experiment.RegabiWrappers) { 
                // Function values can only point to
                // ABIInternal entry points. This will create
                // the funcsym for either the defining
                // function or its wrapper as appropriate.
                //
                // If we're using ABI aliases instead of
                // wrappers, we only InitLSym for the defining
                // ABI of a function, so we make the funcsym
                // when we see that.
                staticdata.NeedFuncSym(f);
            }
            if (!buildcfg.Experiment.RegabiWrappers) { 
                // Create ABI aliases instead of wrappers.
                forEachWrapperABI(_addr_f, makeABIAlias);
            }
        }
    }
    if (hasBody) {
        setupTextLSym(_addr_f, 0);
    }
}

private static void forEachWrapperABI(ptr<ir.Func> _addr_fn, Action<ptr<ir.Func>, obj.ABI> cb) {
    ref ir.Func fn = ref _addr_fn.val;

    var need = fn.ABIRefs & ~obj.ABISetOf(fn.ABI);
    if (need == 0) {
        return ;
    }
    for (var wrapperABI = obj.ABI(0); wrapperABI < obj.ABICount; wrapperABI++) {
        if (!need.Get(wrapperABI)) {
            continue;
        }
        cb(fn, wrapperABI);
    }
}

// makeABIAlias creates a new ABI alias so calls to f via wrapperABI
// will be resolved directly to f's ABI by the linker.
private static void makeABIAlias(ptr<ir.Func> _addr_f, obj.ABI wrapperABI) {
    ref ir.Func f = ref _addr_f.val;
 
    // These LSyms have the same name as the native function, so
    // we create them directly rather than looking them up.
    // The uniqueness of f.lsym ensures uniqueness of asym.
    ptr<obj.LSym> asym = addr(new obj.LSym(Name:f.LSym.Name,Type:objabi.SABIALIAS,R:[]obj.Reloc{{Sym:f.LSym}},));
    asym.SetABI(wrapperABI);
    asym.Set(obj.AttrDuplicateOK, true);
    @base.Ctxt.ABIAliases = append(@base.Ctxt.ABIAliases, asym);
}

// makeABIWrapper creates a new function that will be called with
// wrapperABI and calls "f" using f.ABI.
private static void makeABIWrapper(ptr<ir.Func> _addr_f, obj.ABI wrapperABI) => func((_, panic, _) => {
    ref ir.Func f = ref _addr_f.val;

    if (@base.Debug.ABIWrap != 0) {
        fmt.Fprintf(os.Stderr, "=-= %v to %v wrapper for %v\n", wrapperABI, f.ABI, f);
    }
    var savepos = @base.Pos;
    var savedclcontext = typecheck.DeclContext;
    var savedcurfn = ir.CurFunc;

    @base.Pos = @base.AutogeneratedPos;
    typecheck.DeclContext = ir.PEXTERN; 

    // At the moment we don't support wrapping a method, we'd need machinery
    // below to handle the receiver. Panic if we see this scenario.
    var ft = f.Nname.Type();
    if (ft.NumRecvs() != 0) {
        panic("makeABIWrapper support for wrapping methods not implemented");
    }
    ptr<ir.Field> noReceiver;
    var tfn = ir.NewFuncType(@base.Pos, noReceiver, typecheck.NewFuncParams(ft.Params(), true), typecheck.NewFuncParams(ft.Results(), false)); 

    // Reuse f's types.Sym to create a new ODCLFUNC/function.
    var fn = typecheck.DeclFunc(f.Nname.Sym(), tfn);
    fn.ABI = wrapperABI;

    fn.SetABIWrapper(true);
    fn.SetDupok(true); 

    // ABI0-to-ABIInternal wrappers will be mainly loading params from
    // stack into registers (and/or storing stack locations back to
    // registers after the wrapped call); in most cases they won't
    // need to allocate stack space, so it should be OK to mark them
    // as NOSPLIT in these cases. In addition, my assumption is that
    // functions written in assembly are NOSPLIT in most (but not all)
    // cases. In the case of an ABIInternal target that has too many
    // parameters to fit into registers, the wrapper would need to
    // allocate stack space, but this seems like an unlikely scenario.
    // Hence: mark these wrappers NOSPLIT.
    //
    // ABIInternal-to-ABI0 wrappers on the other hand will be taking
    // things in registers and pushing them onto the stack prior to
    // the ABI0 call, meaning that they will always need to allocate
    // stack space. If the compiler marks them as NOSPLIT this seems
    // as though it could lead to situations where the linker's
    // nosplit-overflow analysis would trigger a link failure. On the
    // other hand if they not tagged NOSPLIT then this could cause
    // problems when building the runtime (since there may be calls to
    // asm routine in cases where it's not safe to grow the stack). In
    // most cases the wrapper would be (in effect) inlined, but are
    // there (perhaps) indirect calls from the runtime that could run
    // into trouble here.
    // FIXME: at the moment all.bash does not pass when I leave out
    // NOSPLIT for these wrappers, so all are currently tagged with NOSPLIT.
    fn.Pragma |= ir.Nosplit; 

    // Generate call. Use tail call if no params and no returns,
    // but a regular call otherwise.
    //
    // Note: ideally we would be using a tail call in cases where
    // there are params but no returns for ABI0->ABIInternal wrappers,
    // provided that all params fit into registers (e.g. we don't have
    // to allocate any stack space). Doing this will require some
    // extra work in typecheck/walk/ssa, might want to add a new node
    // OTAILCALL or something to this effect.
    var tailcall = tfn.Type().NumResults() == 0 && tfn.Type().NumParams() == 0 && tfn.Type().NumRecvs() == 0;
    if (@base.Ctxt.Arch.Name == "ppc64le" && @base.Ctxt.Flag_dynlink) { 
        // cannot tailcall on PPC64 with dynamic linking, as we need
        // to restore R2 after call.
        tailcall = false;
    }
    if (@base.Ctxt.Arch.Name == "amd64" && wrapperABI == obj.ABIInternal) { 
        // cannot tailcall from ABIInternal to ABI0 on AMD64, as we need
        // to special registers (X15) when returning to ABIInternal.
        tailcall = false;
    }
    ir.Node tail = default;
    if (tailcall) {
        tail = ir.NewTailCallStmt(@base.Pos, f.Nname);
    }
    else
 {
        var call = ir.NewCallExpr(@base.Pos, ir.OCALL, f.Nname, null);
        call.Args = ir.ParamNames(tfn.Type());
        call.IsDDD = tfn.Type().IsVariadic();
        tail = call;
        if (tfn.Type().NumResults() > 0) {
            var n = ir.NewReturnStmt(@base.Pos, null);
            n.Results = new slice<ir.Node>(new ir.Node[] { call });
            tail = n;
        }
    }
    fn.Body.Append(tail);

    typecheck.FinishFuncBody();
    if (@base.Debug.DclStack != 0) {
        types.CheckDclstack();
    }
    typecheck.Func(fn);
    ir.CurFunc = fn;
    typecheck.Stmts(fn.Body);

    typecheck.Target.Decls = append(typecheck.Target.Decls, fn); 

    // Restore previous context.
    @base.Pos = savepos;
    typecheck.DeclContext = savedclcontext;
    ir.CurFunc = savedcurfn;
});

// setupTextLsym initializes the LSym for a with-body text symbol.
private static void setupTextLSym(ptr<ir.Func> _addr_f, nint flag) {
    ref ir.Func f = ref _addr_f.val;

    if (f.Dupok()) {
        flag |= obj.DUPOK;
    }
    if (f.Wrapper()) {
        flag |= obj.WRAPPER;
    }
    if (f.ABIWrapper()) {
        flag |= obj.ABIWRAPPER;
    }
    if (f.Needctxt()) {
        flag |= obj.NEEDCTXT;
    }
    if (f.Pragma & ir.Nosplit != 0) {
        flag |= obj.NOSPLIT;
    }
    if (f.ReflectMethod()) {
        flag |= obj.REFLECTMETHOD;
    }
    var fnname = f.Sym().Name;
    if (@base.Ctxt.Pkgpath == "runtime" && fnname == "reflectcall") {
        flag |= obj.WRAPPER;
    }
    else if (@base.Ctxt.Pkgpath == "reflect") {
        switch (fnname) {
            case "callReflect": 

            case "callMethod": 
                flag |= obj.WRAPPER;
                break;
        }
    }
    @base.Ctxt.InitTextSym(f.LSym, flag);
}

} // end ssagen_package
