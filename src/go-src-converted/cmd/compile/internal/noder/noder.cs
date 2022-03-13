// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package noder -- go2cs converted at 2022 March 13 06:27:32 UTC
// import "cmd/compile/internal/noder" ==> using noder = go.cmd.compile.@internal.noder_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\noder\noder.go
namespace go.cmd.compile.@internal;

using fmt = fmt_package;
using constant = go.constant_package;
using token = go.token_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;

using @base = cmd.compile.@internal.@base_package;
using dwarfgen = cmd.compile.@internal.dwarfgen_package;
using ir = cmd.compile.@internal.ir_package;
using syntax = cmd.compile.@internal.syntax_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;
using objabi = cmd.@internal.objabi_package;
using src = cmd.@internal.src_package;
using System;
using System.Threading;

public static partial class noder_package {

public static void LoadPackage(slice<@string> filenames) => func((defer, _, _) => {
    @base.Timer.Start("fe", "parse");

    var mode = syntax.CheckBranches;
    if (@base.Flag.G != 0) {
        mode |= syntax.AllowGenerics;
    }
    var sem = make_channel<object>(runtime.GOMAXPROCS(0) + 10);

    var noders = make_slice<ptr<noder>>(len(filenames));
    {
        var i__prev1 = i;
        var filename__prev1 = filename;

        foreach (var (__i, __filename) in filenames) {
            i = __i;
            filename = __filename;
            ref noder p = ref heap(new noder(err:make(chansyntax.Error),trackScopes:base.Flag.Dwarf,), out ptr<noder> _addr_p);
            _addr_noders[i] = _addr_p;
            noders[i] = ref _addr_noders[i].val;

            var filename = filename;
            go_(() => () => {
                sem.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
                defer(() => {
                    sem.Receive();
                }());
                defer(close(p.err));
                var fbase = syntax.NewFileBase(filename);

                var (f, err) = os.Open(filename);
                if (err != null) {
                    p.error(new syntax.Error(Msg:err.Error()));
                    return ;
                }
                defer(f.Close());

                p.file, _ = syntax.Parse(fbase, f, p.error, p.pragma, mode); // errors are tracked via p.error
            }());
        }
        i = i__prev1;
        filename = filename__prev1;
    }

    nuint lines = default;
    {
        noder p__prev1 = p;

        foreach (var (_, __p) in noders) {
            p = __p;
            foreach (var (e) in p.err) {
                p.errorAt(e.Pos, "%s", e.Msg);
            }            if (p.file == null) {
                @base.ErrorExit();
            }
            lines += p.file.EOF.Line();
        }
        p = p__prev1;
    }

    @base.Timer.AddEvent(int64(lines), "lines");

    if (@base.Flag.G != 0) { 
        // Use types2 to type-check and possibly generate IR.
        check2(noders);
        return ;
    }
    {
        noder p__prev1 = p;

        foreach (var (_, __p) in noders) {
            p = __p;
            p.node();
            p.file = null; // release memory
        }
        p = p__prev1;
    }

    if (@base.SyntaxErrors() != 0) {
        @base.ErrorExit();
    }
    types.CheckDclstack();

    {
        noder p__prev1 = p;

        foreach (var (_, __p) in noders) {
            p = __p;
            p.processPragmas();
        }
        p = p__prev1;
    }

    types.LocalPkg.Height = myheight;
    typecheck.DeclareUniverse();
    typecheck.TypecheckAllowed = true; 

    // Process top-level declarations in phases.

    // Phase 1: const, type, and names and types of funcs.
    //   This will gather all the information about types
    //   and methods but doesn't depend on any of it.
    //
    //   We also defer type alias declarations until phase 2
    //   to avoid cycles like #18640.
    //   TODO(gri) Remove this again once we have a fix for #25838.

    // Don't use range--typecheck can add closures to Target.Decls.
    @base.Timer.Start("fe", "typecheck", "top1");
    {
        var i__prev1 = i;

        for (nint i = 0; i < len(typecheck.Target.Decls); i++) {
            var n = typecheck.Target.Decls[i];
            {
                var op__prev1 = op;

                var op = n.Op();

                if (op != ir.ODCL && op != ir.OAS && op != ir.OAS2 && (op != ir.ODCLTYPE || !n._<ptr<ir.Decl>>().X.Alias())) {
                    typecheck.Target.Decls[i] = typecheck.Stmt(n);
                }
                op = op__prev1;

            }
        }

        i = i__prev1;
    } 

    // Phase 2: Variable assignments.
    //   To check interface assignments, depends on phase 1.

    // Don't use range--typecheck can add closures to Target.Decls.
    @base.Timer.Start("fe", "typecheck", "top2");
    {
        var i__prev1 = i;

        for (i = 0; i < len(typecheck.Target.Decls); i++) {
            n = typecheck.Target.Decls[i];
            {
                var op__prev1 = op;

                op = n.Op();

                if (op == ir.ODCL || op == ir.OAS || op == ir.OAS2 || op == ir.ODCLTYPE && n._<ptr<ir.Decl>>().X.Alias()) {
                    typecheck.Target.Decls[i] = typecheck.Stmt(n);
                }
                op = op__prev1;

            }
        }

        i = i__prev1;
    } 

    // Phase 3: Type check function bodies.
    // Don't use range--typecheck can add closures to Target.Decls.
    @base.Timer.Start("fe", "typecheck", "func");
    long fcount = default;
    {
        var i__prev1 = i;

        for (i = 0; i < len(typecheck.Target.Decls); i++) {
            n = typecheck.Target.Decls[i];
            if (n.Op() == ir.ODCLFUNC) {
                if (@base.Flag.W > 1) {
                    var s = fmt.Sprintf("\nbefore typecheck %v", n);
                    ir.Dump(s, n);
                }
                typecheck.FuncBody(n._<ptr<ir.Func>>());
                if (@base.Flag.W > 1) {
                    s = fmt.Sprintf("\nafter typecheck %v", n);
                    ir.Dump(s, n);
                }
                fcount++;
            }
        }

        i = i__prev1;
    } 

    // Phase 4: Check external declarations.
    // TODO(mdempsky): This should be handled when type checking their
    // corresponding ODCL nodes.
    @base.Timer.Start("fe", "typecheck", "externdcls");
    {
        var i__prev1 = i;
        var n__prev1 = n;

        foreach (var (__i, __n) in typecheck.Target.Externs) {
            i = __i;
            n = __n;
            if (n.Op() == ir.ONAME) {
                typecheck.Target.Externs[i] = typecheck.Expr(typecheck.Target.Externs[i]);
            }
        }
        i = i__prev1;
        n = n__prev1;
    }

    typecheck.CheckMapKeys();
    CheckDotImports();
    @base.ExitIfErrors();
});

private static void errorAt(this ptr<noder> _addr_p, syntax.Pos pos, @string format, params object[] args) {
    args = args.Clone();
    ref noder p = ref _addr_p.val;

    @base.ErrorfAt(p.makeXPos(pos), format, args);
}

// TODO(gri) Can we eliminate fileh in favor of absFilename?
private static @string fileh(@string name) {
    return objabi.AbsFile("", name, @base.Flag.TrimPath);
}

private static @string absFilename(@string name) {
    return objabi.AbsFile(@base.Ctxt.Pathname, name, @base.Flag.TrimPath);
}

// noder transforms package syntax's AST into a Node tree.
private partial struct noder {
    public ref posMap posMap => ref posMap_val;
    public ptr<syntax.File> file;
    public slice<linkname> linknames;
    public slice<slice<@string>> pragcgobuf;
    public channel<syntax.Error> err;
    public bool importedUnsafe;
    public bool importedEmbed;
    public bool trackScopes;
    public ptr<funcState> funcState;
}

// funcState tracks all per-function state to make handling nested
// functions easier.
private partial struct funcState {
    public slice<nint> scopeVars;
    public dwarfgen.ScopeMarker marker;
    public syntax.Pos lastCloseScopePos;
}

private static void funcBody(this ptr<noder> _addr_p, ptr<ir.Func> _addr_fn, ptr<syntax.BlockStmt> _addr_block) {
    ref noder p = ref _addr_p.val;
    ref ir.Func fn = ref _addr_fn.val;
    ref syntax.BlockStmt block = ref _addr_block.val;

    var outerFuncState = p.funcState;
    p.funcState = @new<funcState>();
    typecheck.StartFuncBody(fn);

    if (block != null) {
        var body = p.stmts(block.List);
        if (body == null) {
            body = new slice<ir.Node>(new ir.Node[] { ir.NewBlockStmt(base.Pos,nil) });
        }
        fn.Body = body;

        @base.Pos = p.makeXPos(block.Rbrace);
        fn.Endlineno = @base.Pos;
    }
    typecheck.FinishFuncBody();
    p.funcState.marker.WriteTo(fn);
    p.funcState = outerFuncState;
}

private static void openScope(this ptr<noder> _addr_p, syntax.Pos pos) {
    ref noder p = ref _addr_p.val;

    var fs = p.funcState;
    types.Markdcl();

    if (p.trackScopes) {
        fs.scopeVars = append(fs.scopeVars, len(ir.CurFunc.Dcl));
        fs.marker.Push(p.makeXPos(pos));
    }
}

private static void closeScope(this ptr<noder> _addr_p, syntax.Pos pos) {
    ref noder p = ref _addr_p.val;

    var fs = p.funcState;
    fs.lastCloseScopePos = pos;
    types.Popdcl();

    if (p.trackScopes) {
        var scopeVars = fs.scopeVars[len(fs.scopeVars) - 1];
        fs.scopeVars = fs.scopeVars[..(int)len(fs.scopeVars) - 1];
        if (scopeVars == len(ir.CurFunc.Dcl)) { 
            // no variables were declared in this scope, so we can retract it.
            fs.marker.Unpush();
        }
        else
 {
            fs.marker.Pop(p.makeXPos(pos));
        }
    }
}

// closeAnotherScope is like closeScope, but it reuses the same mark
// position as the last closeScope call. This is useful for "for" and
// "if" statements, as their implicit blocks always end at the same
// position as an explicit block.
private static void closeAnotherScope(this ptr<noder> _addr_p) {
    ref noder p = ref _addr_p.val;

    p.closeScope(p.funcState.lastCloseScopePos);
}

// linkname records a //go:linkname directive.
private partial struct linkname {
    public syntax.Pos pos;
    public @string local;
    public @string remote;
}

private static void node(this ptr<noder> _addr_p) {
    ref noder p = ref _addr_p.val;

    p.importedUnsafe = false;
    p.importedEmbed = false;

    p.setlineno(p.file.PkgName);
    mkpackage(p.file.PkgName.Value);

    {
        ptr<pragmas> (pragma, ok) = p.file.Pragma._<ptr<pragmas>>();

        if (ok) {
            pragma.Flag &= ir.GoBuildPragma;
            p.checkUnused(pragma);
        }
    }

    typecheck.Target.Decls = append(typecheck.Target.Decls, p.decls(p.file.DeclList));

    @base.Pos = src.NoXPos;
    clearImports();
}

private static void processPragmas(this ptr<noder> _addr_p) {
    ref noder p = ref _addr_p.val;

    foreach (var (_, l) in p.linknames) {
        if (!p.importedUnsafe) {
            p.errorAt(l.pos, "//go:linkname only allowed in Go files that import \"unsafe\"");
            continue;
        }
        var n = ir.AsNode(typecheck.Lookup(l.local).Def);
        if (n == null || n.Op() != ir.ONAME) { 
            // TODO(mdempsky): Change to p.errorAt before Go 1.17 release.
            // base.WarnfAt(p.makeXPos(l.pos), "//go:linkname must refer to declared function or variable (will be an error in Go 1.17)")
            continue;
        }
        if (n.Sym().Linkname != "") {
            p.errorAt(l.pos, "duplicate //go:linkname for %s", l.local);
            continue;
        }
        n.Sym().Linkname = l.remote;
    }    typecheck.Target.CgoPragmas = append(typecheck.Target.CgoPragmas, p.pragcgobuf);
}

private static slice<ir.Node> decls(this ptr<noder> _addr_p, slice<syntax.Decl> decls) => func((_, panic, _) => {
    slice<ir.Node> l = default;
    ref noder p = ref _addr_p.val;

    ref constState cs = ref heap(out ptr<constState> _addr_cs);

    {
        var decl__prev1 = decl;

        foreach (var (_, __decl) in decls) {
            decl = __decl;
            p.setlineno(decl);
            switch (decl.type()) {
                case ptr<syntax.ImportDecl> decl:
                    p.importDecl(decl);
                    break;
                case ptr<syntax.VarDecl> decl:
                    l = append(l, p.varDecl(decl));
                    break;
                case ptr<syntax.ConstDecl> decl:
                    l = append(l, p.constDecl(decl, _addr_cs));
                    break;
                case ptr<syntax.TypeDecl> decl:
                    l = append(l, p.typeDecl(decl));
                    break;
                case ptr<syntax.FuncDecl> decl:
                    l = append(l, p.funcDecl(decl));
                    break;
                default:
                {
                    var decl = decl.type();
                    panic("unhandled Decl");
                    break;
                }
            }
        }
        decl = decl__prev1;
    }

    return ;
});

private static void importDecl(this ptr<noder> _addr_p, ptr<syntax.ImportDecl> _addr_imp) {
    ref noder p = ref _addr_p.val;
    ref syntax.ImportDecl imp = ref _addr_imp.val;

    if (imp.Path == null || imp.Path.Bad) {
        return ; // avoid follow-on errors if there was a syntax error
    }
    {
        ptr<pragmas> (pragma, ok) = imp.Pragma._<ptr<pragmas>>();

        if (ok) {
            p.checkUnused(pragma);
        }
    }

    var ipkg = importfile(imp);
    if (ipkg == null) {
        if (@base.Errors() == 0) {
            @base.Fatalf("phase error in import");
        }
        return ;
    }
    if (ipkg == ir.Pkgs.Unsafe) {
        p.importedUnsafe = true;
    }
    if (ipkg.Path == "embed") {
        p.importedEmbed = true;
    }
    ptr<types.Sym> my;
    if (imp.LocalPkgName != null) {
        my = p.name(imp.LocalPkgName);
    }
    else
 {
        my = typecheck.Lookup(ipkg.Name);
    }
    var pack = ir.NewPkgName(p.pos(imp), my, ipkg);

    switch (my.Name) {
        case ".": 
            importDot(pack);
            return ;
            break;
        case "init": 
            @base.ErrorfAt(pack.Pos(), "cannot import package as init - init must be a func");
            return ;
            break;
        case "_": 
            return ;
            break;
    }
    if (my.Def != null) {
        typecheck.Redeclared(pack.Pos(), my, "as imported package name");
    }
    my.Def = pack;
    my.Lastlineno = pack.Pos();
    my.Block = 1; // at top level
}

private static slice<ir.Node> varDecl(this ptr<noder> _addr_p, ptr<syntax.VarDecl> _addr_decl) {
    ref noder p = ref _addr_p.val;
    ref syntax.VarDecl decl = ref _addr_decl.val;

    var names = p.declNames(ir.ONAME, decl.NameList);
    var typ = p.typeExprOrNil(decl.Type);
    var exprs = p.exprList(decl.Values);

    {
        ptr<pragmas> (pragma, ok) = decl.Pragma._<ptr<pragmas>>();

        if (ok) {
            varEmbed(p.makeXPos, _addr_names[0], _addr_decl, pragma, p.importedEmbed);
            p.checkUnused(pragma);
        }
    }

    slice<ir.Node> init = default;
    p.setlineno(decl);

    if (len(names) > 1 && len(exprs) == 1) {
        var as2 = ir.NewAssignListStmt(@base.Pos, ir.OAS2, null, exprs);
        {
            var v__prev1 = v;

            foreach (var (_, __v) in names) {
                v = __v;
                as2.Lhs.Append(v);
                typecheck.Declare(v, typecheck.DeclContext);
                v.Ntype = typ;
                v.Defn = as2;
                if (ir.CurFunc != null) {
                    init = append(init, ir.NewDecl(@base.Pos, ir.ODCL, v));
                }
            }

            v = v__prev1;
        }

        return append(init, as2);
    }
    {
        var v__prev1 = v;

        foreach (var (__i, __v) in names) {
            i = __i;
            v = __v;
            ir.Node e = default;
            if (i < len(exprs)) {
                e = exprs[i];
            }
            typecheck.Declare(v, typecheck.DeclContext);
            v.Ntype = typ;

            if (ir.CurFunc != null) {
                init = append(init, ir.NewDecl(@base.Pos, ir.ODCL, v));
            }
            var @as = ir.NewAssignStmt(@base.Pos, v, e);
            init = append(init, as);
            if (e != null || ir.CurFunc == null) {
                v.Defn = as;
            }
        }
        v = v__prev1;
    }

    if (len(exprs) != 0 && len(names) != len(exprs)) {
        @base.Errorf("assignment mismatch: %d variables but %d values", len(names), len(exprs));
    }
    return init;
}

// constState tracks state between constant specifiers within a
// declaration group. This state is kept separate from noder so nested
// constant declarations are handled correctly (e.g., issue 15550).
private partial struct constState {
    public ptr<syntax.Group> group;
    public ir.Ntype typ;
    public slice<ir.Node> values;
    public long iota;
}

private static slice<ir.Node> constDecl(this ptr<noder> _addr_p, ptr<syntax.ConstDecl> _addr_decl, ptr<constState> _addr_cs) {
    ref noder p = ref _addr_p.val;
    ref syntax.ConstDecl decl = ref _addr_decl.val;
    ref constState cs = ref _addr_cs.val;

    if (decl.Group == null || decl.Group != cs.group) {
        cs = new constState(group:decl.Group,);
    }
    {
        ptr<pragmas> (pragma, ok) = decl.Pragma._<ptr<pragmas>>();

        if (ok) {
            p.checkUnused(pragma);
        }
    }

    var names = p.declNames(ir.OLITERAL, decl.NameList);
    var typ = p.typeExprOrNil(decl.Type);

    slice<ir.Node> values = default;
    if (decl.Values != null) {
        values = p.exprList(decl.Values);
        (cs.typ, cs.values) = (typ, values);
    }
    else
 {
        if (typ != null) {
            @base.Errorf("const declaration cannot have type without expression");
        }
        (typ, values) = (cs.typ, cs.values);
    }
    var nn = make_slice<ir.Node>(0, len(names));
    foreach (var (i, n) in names) {
        if (i >= len(values)) {
            @base.Errorf("missing value in const declaration");
            break;
        }
        var v = values[i];
        if (decl.Values == null) {
            v = ir.DeepCopy(n.Pos(), v);
        }
        typecheck.Declare(n, typecheck.DeclContext);

        n.Ntype = typ;
        n.Defn = v;
        n.SetIota(cs.iota);

        nn = append(nn, ir.NewDecl(p.pos(decl), ir.ODCLCONST, n));
    }    if (len(values) > len(names)) {
        @base.Errorf("extra expression in const declaration");
    }
    cs.iota++;

    return nn;
}

private static ir.Node typeDecl(this ptr<noder> _addr_p, ptr<syntax.TypeDecl> _addr_decl) {
    ref noder p = ref _addr_p.val;
    ref syntax.TypeDecl decl = ref _addr_decl.val;

    var n = p.declName(ir.OTYPE, decl.Name);
    typecheck.Declare(n, typecheck.DeclContext); 

    // decl.Type may be nil but in that case we got a syntax error during parsing
    var typ = p.typeExprOrNil(decl.Type);

    n.Ntype = typ;
    n.SetAlias(decl.Alias);
    {
        ptr<pragmas> (pragma, ok) = decl.Pragma._<ptr<pragmas>>();

        if (ok) {
            if (!decl.Alias) {
                n.SetPragma(pragma.Flag & typePragmas);
                pragma.Flag &= typePragmas;
            }
            p.checkUnused(pragma);
        }
    }

    var nod = ir.NewDecl(p.pos(decl), ir.ODCLTYPE, n);
    if (n.Alias() && !types.AllowsGoVersion(types.LocalPkg, 1, 9)) {
        @base.ErrorfAt(nod.Pos(), "type aliases only supported as of -lang=go1.9");
    }
    return nod;
}

private static slice<ptr<ir.Name>> declNames(this ptr<noder> _addr_p, ir.Op op, slice<ptr<syntax.Name>> names) {
    ref noder p = ref _addr_p.val;

    var nodes = make_slice<ptr<ir.Name>>(0, len(names));
    foreach (var (_, name) in names) {
        nodes = append(nodes, p.declName(op, name));
    }    return nodes;
}

private static ptr<ir.Name> declName(this ptr<noder> _addr_p, ir.Op op, ptr<syntax.Name> _addr_name) {
    ref noder p = ref _addr_p.val;
    ref syntax.Name name = ref _addr_name.val;

    return _addr_ir.NewDeclNameAt(p.pos(name), op, p.name(name))!;
}

private static ir.Node funcDecl(this ptr<noder> _addr_p, ptr<syntax.FuncDecl> _addr_fun) {
    ref noder p = ref _addr_p.val;
    ref syntax.FuncDecl fun = ref _addr_fun.val;

    var name = p.name(fun.Name);
    var t = p.signature(fun.Recv, fun.Type);
    var f = ir.NewFunc(p.pos(fun));

    if (fun.Recv == null) {
        if (name.Name == "init") {
            name = renameinit();
            if (len(t.Params) > 0 || len(t.Results) > 0) {
                @base.ErrorfAt(f.Pos(), "func init must have no arguments and no return values");
            }
            typecheck.Target.Inits = append(typecheck.Target.Inits, f);
        }
        if (types.LocalPkg.Name == "main" && name.Name == "main") {
            if (len(t.Params) > 0 || len(t.Results) > 0) {
                @base.ErrorfAt(f.Pos(), "func main must have no arguments and no return values");
            }
        }
    }
    else
 {
        f.Shortname = name;
        name = ir.BlankNode.Sym(); // filled in by tcFunc
    }
    f.Nname = ir.NewNameAt(p.pos(fun.Name), name);
    f.Nname.Func = f;
    f.Nname.Defn = f;
    f.Nname.Ntype = t;

    {
        ptr<pragmas> (pragma, ok) = fun.Pragma._<ptr<pragmas>>();

        if (ok) {
            f.Pragma = pragma.Flag & funcPragmas;
            if (pragma.Flag & ir.Systemstack != 0 && pragma.Flag & ir.Nosplit != 0) {
                @base.ErrorfAt(f.Pos(), "go:nosplit and go:systemstack cannot be combined");
            }
            pragma.Flag &= funcPragmas;
            p.checkUnused(pragma);
        }
    }

    if (fun.Recv == null) {
        typecheck.Declare(f.Nname, ir.PFUNC);
    }
    p.funcBody(f, fun.Body);

    if (fun.Body != null) {
        if (f.Pragma & ir.Noescape != 0) {
            @base.ErrorfAt(f.Pos(), "can only use //go:noescape with external func implementations");
        }
    }
    else
 {
        if (@base.Flag.Complete || strings.HasPrefix(ir.FuncName(f), "init.")) { 
            // Linknamed functions are allowed to have no body. Hopefully
            // the linkname target has a body. See issue 23311.
            var isLinknamed = false;
            foreach (var (_, n) in p.linknames) {
                if (ir.FuncName(f) == n.local) {
                    isLinknamed = true;
                    break;
                }
            }
            if (!isLinknamed) {
                @base.ErrorfAt(f.Pos(), "missing function body");
            }
        }
    }
    return f;
}

private static ptr<ir.FuncType> signature(this ptr<noder> _addr_p, ptr<syntax.Field> _addr_recv, ptr<syntax.FuncType> _addr_typ) {
    ref noder p = ref _addr_p.val;
    ref syntax.Field recv = ref _addr_recv.val;
    ref syntax.FuncType typ = ref _addr_typ.val;

    ptr<ir.Field> rcvr;
    if (recv != null) {
        rcvr = p.param(recv, false, false);
    }
    return _addr_ir.NewFuncType(p.pos(typ), rcvr, p.@params(typ.ParamList, true), p.@params(typ.ResultList, false))!;
}

private static slice<ptr<ir.Field>> @params(this ptr<noder> _addr_p, slice<ptr<syntax.Field>> @params, bool dddOk) {
    ref noder p = ref _addr_p.val;

    var nodes = make_slice<ptr<ir.Field>>(0, len(params));
    foreach (var (i, param) in params) {
        p.setlineno(param);
        nodes = append(nodes, p.param(param, dddOk, i + 1 == len(params)));
    }    return nodes;
}

private static ptr<ir.Field> param(this ptr<noder> _addr_p, ptr<syntax.Field> _addr_param, bool dddOk, bool final) {
    ref noder p = ref _addr_p.val;
    ref syntax.Field param = ref _addr_param.val;

    ptr<types.Sym> name;
    if (param.Name != null) {
        name = p.name(param.Name);
    }
    var typ = p.typeExpr(param.Type);
    var n = ir.NewField(p.pos(param), name, typ, null); 

    // rewrite ...T parameter
    {
        var typ__prev1 = typ;

        ptr<ir.SliceType> (typ, ok) = typ._<ptr<ir.SliceType>>();

        if (ok && typ.DDD) {
            if (!dddOk) { 
                // We mark these as syntax errors to get automatic elimination
                // of multiple such errors per line (see ErrorfAt in subr.go).
                @base.Errorf("syntax error: cannot use ... in receiver or result parameter list");
            }
            else if (!final) {
                if (param.Name == null) {
                    @base.Errorf("syntax error: cannot use ... with non-final parameter");
                }
                else
 {
                    p.errorAt(param.Name.Pos(), "syntax error: cannot use ... with non-final parameter %s", param.Name.Value);
                }
            }
            typ.DDD = false;
            n.IsDDD = true;
        }
        typ = typ__prev1;

    }

    return _addr_n!;
}

private static slice<ir.Node> exprList(this ptr<noder> _addr_p, syntax.Expr expr) {
    ref noder p = ref _addr_p.val;

    switch (expr.type()) {
        case 
            return null;
            break;
        case ptr<syntax.ListExpr> expr:
            return p.exprs(expr.ElemList);
            break;
        default:
        {
            var expr = expr.type();
            return new slice<ir.Node>(new ir.Node[] { p.expr(expr) });
            break;
        }
    }
}

private static slice<ir.Node> exprs(this ptr<noder> _addr_p, slice<syntax.Expr> exprs) {
    ref noder p = ref _addr_p.val;

    var nodes = make_slice<ir.Node>(0, len(exprs));
    foreach (var (_, expr) in exprs) {
        nodes = append(nodes, p.expr(expr));
    }    return nodes;
}

private static ir.Node expr(this ptr<noder> _addr_p, syntax.Expr expr) => func((_, panic, _) => {
    ref noder p = ref _addr_p.val;

    p.setlineno(expr);
    switch (expr.type()) {
        case ptr<syntax.BadExpr> expr:
            return null;
            break;
        case ptr<syntax.Name> expr:
            return p.mkname(expr);
            break;
        case ptr<syntax.BasicLit> expr:
            var n = ir.NewBasicLit(p.pos(expr), p.basicLit(expr));
            if (expr.Kind == syntax.RuneLit) {
                n.SetType(types.UntypedRune);
            }
            n.SetDiag(expr.Bad || n.Val().Kind() == constant.Unknown); // avoid follow-on errors if there was a syntax error
            return n;
            break;
        case ptr<syntax.CompositeLit> expr:
            n = ir.NewCompLitExpr(p.pos(expr), ir.OCOMPLIT, p.typeExpr(expr.Type), null);
            var l = p.exprs(expr.ElemList);
            {
                var i__prev1 = i;

                foreach (var (__i, __e) in l) {
                    i = __i;
                    e = __e;
                    l[i] = p.wrapname(expr.ElemList[i], e);
                }

                i = i__prev1;
            }

            n.List = l;
            @base.Pos = p.makeXPos(expr.Rbrace);
            return n;
            break;
        case ptr<syntax.KeyValueExpr> expr:
            return ir.NewKeyExpr(p.pos(expr.Key), p.expr(expr.Key), p.wrapname(expr.Value, p.expr(expr.Value)));
            break;
        case ptr<syntax.FuncLit> expr:
            return p.funcLit(expr);
            break;
        case ptr<syntax.ParenExpr> expr:
            return ir.NewParenExpr(p.pos(expr), p.expr(expr.X));
            break;
        case ptr<syntax.SelectorExpr> expr:
            var obj = p.expr(expr.X);
            if (obj.Op() == ir.OPACK) {
                ptr<ir.PkgName> pack = obj._<ptr<ir.PkgName>>();
                pack.Used = true;
                return importName(pack.Pkg.Lookup(expr.Sel.Value));
            }
            n = ir.NewSelectorExpr(@base.Pos, ir.OXDOT, obj, p.name(expr.Sel));
            n.SetPos(p.pos(expr)); // lineno may have been changed by p.expr(expr.X)
            return n;
            break;
        case ptr<syntax.IndexExpr> expr:
            return ir.NewIndexExpr(p.pos(expr), p.expr(expr.X), p.expr(expr.Index));
            break;
        case ptr<syntax.SliceExpr> expr:
            var op = ir.OSLICE;
            if (expr.Full) {
                op = ir.OSLICE3;
            }
            var x = p.expr(expr.X);
            array<ir.Node> index = new array<ir.Node>(3);
            {
                var i__prev1 = i;
                var n__prev1 = n;

                foreach (var (__i, __n) in _addr_expr.Index) {
                    i = __i;
                    n = __n;
                    if (n != null) {
                        index[i] = p.expr(n);
                    }
                }

                i = i__prev1;
                n = n__prev1;
            }

            return ir.NewSliceExpr(p.pos(expr), op, x, index[0], index[1], index[2]);
            break;
        case ptr<syntax.AssertExpr> expr:
            return ir.NewTypeAssertExpr(p.pos(expr), p.expr(expr.X), p.typeExpr(expr.Type));
            break;
        case ptr<syntax.Operation> expr:
            if (expr.Op == syntax.Add && expr.Y != null) {
                return p.sum(expr);
            }
            x = p.expr(expr.X);
            if (expr.Y == null) {
                var pos = p.pos(expr);
                op = p.unOp(expr.Op);

                if (op == ir.OADDR) 
                    return typecheck.NodAddrAt(pos, x);
                else if (op == ir.ODEREF) 
                    return ir.NewStarExpr(pos, x);
                                return ir.NewUnaryExpr(pos, op, x);
            }
            pos = p.pos(expr);
            op = p.binOp(expr.Op);
            var y = p.expr(expr.Y);

            if (op == ir.OANDAND || op == ir.OOROR) 
                return ir.NewLogicalExpr(pos, op, x, y);
                        return ir.NewBinaryExpr(pos, op, x, y);
            break;
        case ptr<syntax.CallExpr> expr:
            n = ir.NewCallExpr(p.pos(expr), ir.OCALL, p.expr(expr.Fun), p.exprs(expr.ArgList));
            n.IsDDD = expr.HasDots;
            return n;
            break;
        case ptr<syntax.ArrayType> expr:
            ir.Node len = default;
            if (expr.Len != null) {
                len = p.expr(expr.Len);
            }
            return ir.NewArrayType(p.pos(expr), len, p.typeExpr(expr.Elem));
            break;
        case ptr<syntax.SliceType> expr:
            return ir.NewSliceType(p.pos(expr), p.typeExpr(expr.Elem));
            break;
        case ptr<syntax.DotsType> expr:
            var t = ir.NewSliceType(p.pos(expr), p.typeExpr(expr.Elem));
            t.DDD = true;
            return t;
            break;
        case ptr<syntax.StructType> expr:
            return p.structType(expr);
            break;
        case ptr<syntax.InterfaceType> expr:
            return p.interfaceType(expr);
            break;
        case ptr<syntax.FuncType> expr:
            return p.signature(null, expr);
            break;
        case ptr<syntax.MapType> expr:
            return ir.NewMapType(p.pos(expr), p.typeExpr(expr.Key), p.typeExpr(expr.Value));
            break;
        case ptr<syntax.ChanType> expr:
            return ir.NewChanType(p.pos(expr), p.typeExpr(expr.Elem), p.chanDir(expr.Dir));
            break;
        case ptr<syntax.TypeSwitchGuard> expr:
            ptr<ir.Ident> tag;
            if (expr.Lhs != null) {
                tag = ir.NewIdent(p.pos(expr.Lhs), p.name(expr.Lhs));
                if (ir.IsBlank(tag)) {
                    @base.Errorf("invalid variable name %v in type switch", tag);
                }
            }
            return ir.NewTypeSwitchGuard(p.pos(expr), tag, p.expr(expr.X));
            break;
    }
    panic("unhandled Expr");
});

// sum efficiently handles very large summation expressions (such as
// in issue #16394). In particular, it avoids left recursion and
// collapses string literals.
private static ir.Node sum(this ptr<noder> _addr_p, syntax.Expr x) {
    ref noder p = ref _addr_p.val;
 
    // While we need to handle long sums with asymptotic
    // efficiency, the vast majority of sums are very small: ~95%
    // have only 2 or 3 operands, and ~99% of string literals are
    // never concatenated.

    var adds = make_slice<ptr<syntax.Operation>>(0, 2);
    while (true) {
        ptr<syntax.Operation> (add, ok) = x._<ptr<syntax.Operation>>();
        if (!ok || add.Op != syntax.Add || add.Y == null) {
            break;
        }
        adds = append(adds, add);
        x = add.X;
    } 

    // nstr is the current rightmost string literal in the
    // summation (if any), and chunks holds its accumulated
    // substrings.
    //
    // Consider the expression x + "a" + "b" + "c" + y. When we
    // reach the string literal "a", we assign nstr to point to
    // its corresponding Node and initialize chunks to {"a"}.
    // Visiting the subsequent string literals "b" and "c", we
    // simply append their values to chunks. Finally, when we
    // reach the non-constant operand y, we'll join chunks to form
    // "abc" and reassign the "a" string literal's value.
    //
    // N.B., we need to be careful about named string constants
    // (indicated by Sym != nil) because 1) we can't modify their
    // value, as doing so would affect other uses of the string
    // constant, and 2) they may have types, which we need to
    // handle correctly. For now, we avoid these problems by
    // treating named string constants the same as non-constant
    // operands.
    ir.Node nstr = default;
    var chunks = make_slice<@string>(0, 1);

    var n = p.expr(x);
    if (ir.IsConst(n, constant.String) && n.Sym() == null) {
        nstr = n;
        chunks = append(chunks, ir.StringVal(nstr));
    }
    for (var i = len(adds) - 1; i >= 0; i--) {
        var add = adds[i];

        var r = p.expr(add.Y);
        if (ir.IsConst(r, constant.String) && r.Sym() == null) {
            if (nstr != null) { 
                // Collapse r into nstr instead of adding to n.
                chunks = append(chunks, ir.StringVal(r));
                continue;
            }
            nstr = r;
            chunks = append(chunks, ir.StringVal(nstr));
        }
        else
 {
            if (len(chunks) > 1) {
                nstr.SetVal(constant.MakeString(strings.Join(chunks, "")));
            }
            nstr = null;
            chunks = chunks[..(int)0];
        }
        n = ir.NewBinaryExpr(p.pos(add), ir.OADD, n, r);
    }
    if (len(chunks) > 1) {
        nstr.SetVal(constant.MakeString(strings.Join(chunks, "")));
    }
    return n;
}

private static ir.Ntype typeExpr(this ptr<noder> _addr_p, syntax.Expr typ) {
    ref noder p = ref _addr_p.val;
 
    // TODO(mdempsky): Be stricter? typecheck should handle errors anyway.
    var n = p.expr(typ);
    if (n == null) {
        return null;
    }
    return n._<ir.Ntype>();
}

private static ir.Ntype typeExprOrNil(this ptr<noder> _addr_p, syntax.Expr typ) {
    ref noder p = ref _addr_p.val;

    if (typ != null) {
        return p.typeExpr(typ);
    }
    return null;
}

private static types.ChanDir chanDir(this ptr<noder> _addr_p, syntax.ChanDir dir) => func((_, panic, _) => {
    ref noder p = ref _addr_p.val;


    if (dir == 0) 
        return types.Cboth;
    else if (dir == syntax.SendOnly) 
        return types.Csend;
    else if (dir == syntax.RecvOnly) 
        return types.Crecv;
        panic("unhandled ChanDir");
});

private static ir.Node structType(this ptr<noder> _addr_p, ptr<syntax.StructType> _addr_expr) {
    ref noder p = ref _addr_p.val;
    ref syntax.StructType expr = ref _addr_expr.val;

    var l = make_slice<ptr<ir.Field>>(0, len(expr.FieldList));
    foreach (var (i, field) in expr.FieldList) {
        p.setlineno(field);
        ptr<ir.Field> n;
        if (field.Name == null) {
            n = p.embedded(field.Type);
        }
        else
 {
            n = ir.NewField(p.pos(field), p.name(field.Name), p.typeExpr(field.Type), null);
        }
        if (i < len(expr.TagList) && expr.TagList[i] != null) {
            n.Note = constant.StringVal(p.basicLit(expr.TagList[i]));
        }
        l = append(l, n);
    }    p.setlineno(expr);
    return ir.NewStructType(p.pos(expr), l);
}

private static ir.Node interfaceType(this ptr<noder> _addr_p, ptr<syntax.InterfaceType> _addr_expr) {
    ref noder p = ref _addr_p.val;
    ref syntax.InterfaceType expr = ref _addr_expr.val;

    var l = make_slice<ptr<ir.Field>>(0, len(expr.MethodList));
    foreach (var (_, method) in expr.MethodList) {
        p.setlineno(method);
        ptr<ir.Field> n;
        if (method.Name == null) {
            n = ir.NewField(p.pos(method), null, importName(p.packname(method.Type))._<ir.Ntype>(), null);
        }
        else
 {
            var mname = p.name(method.Name);
            if (mname.IsBlank()) {
                @base.Errorf("methods must have a unique non-blank name");
                continue;
            }
            ptr<ir.FuncType> sig = p.typeExpr(method.Type)._<ptr<ir.FuncType>>();
            sig.Recv = fakeRecv();
            n = ir.NewField(p.pos(method), mname, sig, null);
        }
        l = append(l, n);
    }    return ir.NewInterfaceType(p.pos(expr), l);
}

private static ptr<types.Sym> packname(this ptr<noder> _addr_p, syntax.Expr expr) => func((_, panic, _) => {
    ref noder p = ref _addr_p.val;

    switch (expr.type()) {
        case ptr<syntax.Name> expr:
            var name = p.name(expr);
            {
                var n = oldname(_addr_name);

                if (n.Name() != null && n.Name().PkgName != null) {
                    n.Name().PkgName.Used = true;
                }

            }
            return _addr_name!;
            break;
        case ptr<syntax.SelectorExpr> expr:
            name = p.name(expr.X._<ptr<syntax.Name>>());
            var def = ir.AsNode(name.Def);
            if (def == null) {
                @base.Errorf("undefined: %v", name);
                return _addr_name!;
            }
            ptr<types.Pkg> pkg;
            if (def.Op() != ir.OPACK) {
                @base.Errorf("%v is not a package", name);
                pkg = types.LocalPkg;
            }
            else
 {
                def = def._<ptr<ir.PkgName>>();
                def.Used = true;
                pkg = def.Pkg;
            }
            return _addr_pkg.Lookup(expr.Sel.Value)!;
            break;
    }
    panic(fmt.Sprintf("unexpected packname: %#v", expr));
});

private static ptr<ir.Field> embedded(this ptr<noder> _addr_p, syntax.Expr typ) => func((_, panic, _) => {
    ref noder p = ref _addr_p.val;

    ptr<syntax.Operation> (op, isStar) = typ._<ptr<syntax.Operation>>();
    if (isStar) {
        if (op.Op != syntax.Mul || op.Y != null) {
            panic("unexpected Operation");
        }
        typ = op.X;
    }
    var sym = p.packname(typ);
    var n = ir.NewField(p.pos(typ), typecheck.Lookup(sym.Name), importName(sym)._<ir.Ntype>(), null);
    n.Embedded = true;

    if (isStar) {
        n.Ntype = ir.NewStarExpr(p.pos(op), n.Ntype);
    }
    return _addr_n!;
});

private static slice<ir.Node> stmts(this ptr<noder> _addr_p, slice<syntax.Stmt> stmts) {
    ref noder p = ref _addr_p.val;

    return p.stmtsFall(stmts, false);
}

private static slice<ir.Node> stmtsFall(this ptr<noder> _addr_p, slice<syntax.Stmt> stmts, bool fallOK) {
    ref noder p = ref _addr_p.val;

    slice<ir.Node> nodes = default;
    foreach (var (i, stmt) in stmts) {
        var s = p.stmtFall(stmt, fallOK && i + 1 == len(stmts));
        if (s == null) {
        }
        else if (s.Op() == ir.OBLOCK && len(s._<ptr<ir.BlockStmt>>().List) > 0) { 
            // Inline non-empty block.
            // Empty blocks must be preserved for CheckReturn.
            nodes = append(nodes, s._<ptr<ir.BlockStmt>>().List);
        }
        else
 {
            nodes = append(nodes, s);
        }
    }    return nodes;
}

private static ir.Node stmt(this ptr<noder> _addr_p, syntax.Stmt stmt) {
    ref noder p = ref _addr_p.val;

    return p.stmtFall(stmt, false);
}

private static ir.Node stmtFall(this ptr<noder> _addr_p, syntax.Stmt stmt, bool fallOK) => func((_, panic, _) => {
    ref noder p = ref _addr_p.val;

    p.setlineno(stmt);
    switch (stmt.type()) {
        case ptr<syntax.EmptyStmt> stmt:
            return null;
            break;
        case ptr<syntax.LabeledStmt> stmt:
            return p.labeledStmt(stmt, fallOK);
            break;
        case ptr<syntax.BlockStmt> stmt:
            var l = p.blockStmt(stmt);
            if (len(l) == 0) { 
                // TODO(mdempsky): Line number?
                return ir.NewBlockStmt(@base.Pos, null);
            }
            return ir.NewBlockStmt(src.NoXPos, l);
            break;
        case ptr<syntax.ExprStmt> stmt:
            return p.wrapname(stmt, p.expr(stmt.X));
            break;
        case ptr<syntax.SendStmt> stmt:
            return ir.NewSendStmt(p.pos(stmt), p.expr(stmt.Chan), p.expr(stmt.Value));
            break;
        case ptr<syntax.DeclStmt> stmt:
            return ir.NewBlockStmt(src.NoXPos, p.decls(stmt.DeclList));
            break;
        case ptr<syntax.AssignStmt> stmt:
            if (stmt.Rhs == null) {
                var pos = p.pos(stmt);
                var n = ir.NewAssignOpStmt(pos, p.binOp(stmt.Op), p.expr(stmt.Lhs), ir.NewBasicLit(pos, one));
                n.IncDec = true;
                return n;
            }
            if (stmt.Op != 0 && stmt.Op != syntax.Def) {
                n = ir.NewAssignOpStmt(p.pos(stmt), p.binOp(stmt.Op), p.expr(stmt.Lhs), p.expr(stmt.Rhs));
                return n;
            }
            var rhs = p.exprList(stmt.Rhs);
            {
                ptr<syntax.ListExpr> (list, ok) = stmt.Lhs._<ptr<syntax.ListExpr>>();

                if (ok && len(list.ElemList) != 1 || len(rhs) != 1) {
                    n = ir.NewAssignListStmt(p.pos(stmt), ir.OAS2, null, null);
                    n.Def = stmt.Op == syntax.Def;
                    n.Lhs = p.assignList(stmt.Lhs, n, n.Def);
                    n.Rhs = rhs;
                    return n;
                }

            }

            n = ir.NewAssignStmt(p.pos(stmt), null, null);
            n.Def = stmt.Op == syntax.Def;
            n.X = p.assignList(stmt.Lhs, n, n.Def)[0];
            n.Y = rhs[0];
            return n;
            break;
        case ptr<syntax.BranchStmt> stmt:
            ir.Op op = default;

            if (stmt.Tok == syntax.Break) 
                op = ir.OBREAK;
            else if (stmt.Tok == syntax.Continue) 
                op = ir.OCONTINUE;
            else if (stmt.Tok == syntax.Fallthrough) 
                if (!fallOK) {
                    @base.Errorf("fallthrough statement out of place");
                }
                op = ir.OFALL;
            else if (stmt.Tok == syntax.Goto) 
                op = ir.OGOTO;
            else 
                panic("unhandled BranchStmt");
                        ptr<types.Sym> sym;
            if (stmt.Label != null) {
                sym = p.name(stmt.Label);
            }
            return ir.NewBranchStmt(p.pos(stmt), op, sym);
            break;
        case ptr<syntax.CallStmt> stmt:
            op = default;

            if (stmt.Tok == syntax.Defer) 
                op = ir.ODEFER;
            else if (stmt.Tok == syntax.Go) 
                op = ir.OGO;
            else 
                panic("unhandled CallStmt");
                        return ir.NewGoDeferStmt(p.pos(stmt), op, p.expr(stmt.Call));
            break;
        case ptr<syntax.ReturnStmt> stmt:
            n = ir.NewReturnStmt(p.pos(stmt), p.exprList(stmt.Results));
            if (len(n.Results) == 0 && ir.CurFunc != null) {
                foreach (var (_, ln) in ir.CurFunc.Dcl) {
                    if (ln.Class == ir.PPARAM) {
                        continue;
                    }
                    if (ln.Class != ir.PPARAMOUT) {
                        break;
                    }
                    if (ln.Sym().Def != ln) {
                        @base.Errorf("%s is shadowed during return", ln.Sym().Name);
                    }
                }
            }
            return n;
            break;
        case ptr<syntax.IfStmt> stmt:
            return p.ifStmt(stmt);
            break;
        case ptr<syntax.ForStmt> stmt:
            return p.forStmt(stmt);
            break;
        case ptr<syntax.SwitchStmt> stmt:
            return p.switchStmt(stmt);
            break;
        case ptr<syntax.SelectStmt> stmt:
            return p.selectStmt(stmt);
            break;
    }
    panic("unhandled Stmt");
});

private static slice<ir.Node> assignList(this ptr<noder> _addr_p, syntax.Expr expr, ir.InitNode defn, bool colas) {
    ref noder p = ref _addr_p.val;

    if (!colas) {
        return p.exprList(expr);
    }
    slice<syntax.Expr> exprs = default;
    {
        ptr<syntax.ListExpr> (list, ok) = expr._<ptr<syntax.ListExpr>>();

        if (ok) {
            exprs = list.ElemList;
        }
        else
 {
            exprs = new slice<syntax.Expr>(new syntax.Expr[] { expr });
        }
    }

    var res = make_slice<ir.Node>(len(exprs));
    var seen = make_map<ptr<types.Sym>, bool>(len(exprs));

    var newOrErr = false;
    foreach (var (i, expr) in exprs) {
        p.setlineno(expr);
        res[i] = ir.BlankNode;

        ptr<syntax.Name> (name, ok) = expr._<ptr<syntax.Name>>();
        if (!ok) {
            p.errorAt(expr.Pos(), "non-name %v on left side of :=", p.expr(expr));
            newOrErr = true;
            continue;
        }
        var sym = p.name(name);
        if (sym.IsBlank()) {
            continue;
        }
        if (seen[sym]) {
            p.errorAt(expr.Pos(), "%v repeated on left side of :=", sym);
            newOrErr = true;
            continue;
        }
        seen[sym] = true;

        if (sym.Block == types.Block) {
            res[i] = oldname(_addr_sym);
            continue;
        }
        newOrErr = true;
        var n = typecheck.NewName(sym);
        typecheck.Declare(n, typecheck.DeclContext);
        n.Defn = defn;
        defn.PtrInit().Append(ir.NewDecl(@base.Pos, ir.ODCL, n));
        res[i] = n;
    }    if (!newOrErr) {
        @base.ErrorfAt(defn.Pos(), "no new variables on left side of :=");
    }
    return res;
}

private static slice<ir.Node> blockStmt(this ptr<noder> _addr_p, ptr<syntax.BlockStmt> _addr_stmt) {
    ref noder p = ref _addr_p.val;
    ref syntax.BlockStmt stmt = ref _addr_stmt.val;

    p.openScope(stmt.Pos());
    var nodes = p.stmts(stmt.List);
    p.closeScope(stmt.Rbrace);
    return nodes;
}

private static ir.Node ifStmt(this ptr<noder> _addr_p, ptr<syntax.IfStmt> _addr_stmt) {
    ref noder p = ref _addr_p.val;
    ref syntax.IfStmt stmt = ref _addr_stmt.val;

    p.openScope(stmt.Pos());
    var init = p.stmt(stmt.Init);
    var n = ir.NewIfStmt(p.pos(stmt), p.expr(stmt.Cond), p.blockStmt(stmt.Then), null);
    if (init != null) {
        n.PtrInit().val = new slice<ir.Node>(new ir.Node[] { init });
    }
    if (stmt.Else != null) {
        var e = p.stmt(stmt.Else);
        if (e.Op() == ir.OBLOCK) {
            e = e._<ptr<ir.BlockStmt>>();
            n.Else = e.List;
        }
        else
 {
            n.Else = new slice<ir.Node>(new ir.Node[] { e });
        }
    }
    p.closeAnotherScope();
    return n;
}

private static ir.Node forStmt(this ptr<noder> _addr_p, ptr<syntax.ForStmt> _addr_stmt) => func((_, panic, _) => {
    ref noder p = ref _addr_p.val;
    ref syntax.ForStmt stmt = ref _addr_stmt.val;

    p.openScope(stmt.Pos());
    {
        ptr<syntax.RangeClause> (r, ok) = stmt.Init._<ptr<syntax.RangeClause>>();

        if (ok) {
            if (stmt.Cond != null || stmt.Post != null) {
                panic("unexpected RangeClause");
            }
            var n = ir.NewRangeStmt(p.pos(r), null, null, p.expr(r.X), null);
            if (r.Lhs != null) {
                n.Def = r.Def;
                var lhs = p.assignList(r.Lhs, n, n.Def);
                n.Key = lhs[0];
                if (len(lhs) > 1) {
                    n.Value = lhs[1];
                }
            }
            n.Body = p.blockStmt(stmt.Body);
            p.closeAnotherScope();
            return n;
        }
    }

    n = ir.NewForStmt(p.pos(stmt), p.stmt(stmt.Init), p.expr(stmt.Cond), p.stmt(stmt.Post), p.blockStmt(stmt.Body));
    p.closeAnotherScope();
    return n;
});

private static ir.Node switchStmt(this ptr<noder> _addr_p, ptr<syntax.SwitchStmt> _addr_stmt) {
    ref noder p = ref _addr_p.val;
    ref syntax.SwitchStmt stmt = ref _addr_stmt.val;

    p.openScope(stmt.Pos());

    var init = p.stmt(stmt.Init);
    var n = ir.NewSwitchStmt(p.pos(stmt), p.expr(stmt.Tag), null);
    if (init != null) {
        n.PtrInit().val = new slice<ir.Node>(new ir.Node[] { init });
    }
    ptr<ir.TypeSwitchGuard> tswitch;
    {
        var l = n.Tag;

        if (l != null && l.Op() == ir.OTYPESW) {
            tswitch = l._<ptr<ir.TypeSwitchGuard>>();
        }
    }
    n.Cases = p.caseClauses(stmt.Body, tswitch, stmt.Rbrace);

    p.closeScope(stmt.Rbrace);
    return n;
}

private static slice<ptr<ir.CaseClause>> caseClauses(this ptr<noder> _addr_p, slice<ptr<syntax.CaseClause>> clauses, ptr<ir.TypeSwitchGuard> _addr_tswitch, syntax.Pos rbrace) {
    ref noder p = ref _addr_p.val;
    ref ir.TypeSwitchGuard tswitch = ref _addr_tswitch.val;

    var nodes = make_slice<ptr<ir.CaseClause>>(0, len(clauses));
    foreach (var (i, clause) in clauses) {
        p.setlineno(clause);
        if (i > 0) {
            p.closeScope(clause.Pos());
        }
        p.openScope(clause.Pos());

        var n = ir.NewCaseStmt(p.pos(clause), p.exprList(clause.Cases), null);
        if (tswitch != null && tswitch.Tag != null) {
            var nn = typecheck.NewName(tswitch.Tag.Sym());
            typecheck.Declare(nn, typecheck.DeclContext);
            n.Var = nn; 
            // keep track of the instances for reporting unused
            nn.Defn = tswitch;
        }
        var body = clause.Body;
        while (len(body) > 0) {
            {
                ptr<syntax.EmptyStmt> (_, ok) = body[len(body) - 1]._<ptr<syntax.EmptyStmt>>();

                if (!ok) {
                    break;
                }

            }
            body = body[..(int)len(body) - 1];
        }

        n.Body = p.stmtsFall(body, true);
        {
            var l = len(n.Body);

            if (l > 0 && n.Body[l - 1].Op() == ir.OFALL) {
                if (tswitch != null) {
                    @base.Errorf("cannot fallthrough in type switch");
                }
                if (i + 1 == len(clauses)) {
                    @base.Errorf("cannot fallthrough final case in switch");
                }
            }

        }

        nodes = append(nodes, n);
    }    if (len(clauses) > 0) {
        p.closeScope(rbrace);
    }
    return nodes;
}

private static ir.Node selectStmt(this ptr<noder> _addr_p, ptr<syntax.SelectStmt> _addr_stmt) {
    ref noder p = ref _addr_p.val;
    ref syntax.SelectStmt stmt = ref _addr_stmt.val;

    return ir.NewSelectStmt(p.pos(stmt), p.commClauses(stmt.Body, stmt.Rbrace));
}

private static slice<ptr<ir.CommClause>> commClauses(this ptr<noder> _addr_p, slice<ptr<syntax.CommClause>> clauses, syntax.Pos rbrace) {
    ref noder p = ref _addr_p.val;

    var nodes = make_slice<ptr<ir.CommClause>>(len(clauses));
    foreach (var (i, clause) in clauses) {
        p.setlineno(clause);
        if (i > 0) {
            p.closeScope(clause.Pos());
        }
        p.openScope(clause.Pos());

        nodes[i] = ir.NewCommStmt(p.pos(clause), p.stmt(clause.Comm), p.stmts(clause.Body));
    }    if (len(clauses) > 0) {
        p.closeScope(rbrace);
    }
    return nodes;
}

private static ir.Node labeledStmt(this ptr<noder> _addr_p, ptr<syntax.LabeledStmt> _addr_label, bool fallOK) {
    ref noder p = ref _addr_p.val;
    ref syntax.LabeledStmt label = ref _addr_label.val;

    var sym = p.name(label.Label);
    var lhs = ir.NewLabelStmt(p.pos(label), sym);

    ir.Node ls = default;
    if (label.Stmt != null) { // TODO(mdempsky): Should always be present.
        ls = p.stmtFall(label.Stmt, fallOK); 
        // Attach label directly to control statement too.
        if (ls != null) {

            if (ls.Op() == ir.OFOR) 
                ls = ls._<ptr<ir.ForStmt>>();
                ls.Label = sym;
            else if (ls.Op() == ir.ORANGE) 
                ls = ls._<ptr<ir.RangeStmt>>();
                ls.Label = sym;
            else if (ls.Op() == ir.OSWITCH) 
                ls = ls._<ptr<ir.SwitchStmt>>();
                ls.Label = sym;
            else if (ls.Op() == ir.OSELECT) 
                ls = ls._<ptr<ir.SelectStmt>>();
                ls.Label = sym;
                    }
    }
    ir.Node l = new slice<ir.Node>(new ir.Node[] { lhs });
    if (ls != null) {
        if (ls.Op() == ir.OBLOCK) {
            ls = ls._<ptr<ir.BlockStmt>>();
            l = append(l, ls.List);
        }
        else
 {
            l = append(l, ls);
        }
    }
    return ir.NewBlockStmt(src.NoXPos, l);
}

private static array<ir.Op> unOps = new array<ir.Op>(InitKeyedValues<ir.Op>((syntax.Recv, ir.ORECV), (syntax.Mul, ir.ODEREF), (syntax.And, ir.OADDR), (syntax.Not, ir.ONOT), (syntax.Xor, ir.OBITNOT), (syntax.Add, ir.OPLUS), (syntax.Sub, ir.ONEG)));

private static ir.Op unOp(this ptr<noder> _addr_p, syntax.Operator op) => func((_, panic, _) => {
    ref noder p = ref _addr_p.val;

    if (uint64(op) >= uint64(len(unOps)) || unOps[op] == 0) {
        panic("invalid Operator");
    }
    return unOps[op];
});

private static array<ir.Op> binOps = new array<ir.Op>(InitKeyedValues<ir.Op>((syntax.OrOr, ir.OOROR), (syntax.AndAnd, ir.OANDAND), (syntax.Eql, ir.OEQ), (syntax.Neq, ir.ONE), (syntax.Lss, ir.OLT), (syntax.Leq, ir.OLE), (syntax.Gtr, ir.OGT), (syntax.Geq, ir.OGE), (syntax.Add, ir.OADD), (syntax.Sub, ir.OSUB), (syntax.Or, ir.OOR), (syntax.Xor, ir.OXOR), (syntax.Mul, ir.OMUL), (syntax.Div, ir.ODIV), (syntax.Rem, ir.OMOD), (syntax.And, ir.OAND), (syntax.AndNot, ir.OANDNOT), (syntax.Shl, ir.OLSH), (syntax.Shr, ir.ORSH)));

private static ir.Op binOp(this ptr<noder> _addr_p, syntax.Operator op) => func((_, panic, _) => {
    ref noder p = ref _addr_p.val;

    if (uint64(op) >= uint64(len(binOps)) || binOps[op] == 0) {
        panic("invalid Operator");
    }
    return binOps[op];
});

// checkLangCompat reports an error if the representation of a numeric
// literal is not compatible with the current language version.
private static void checkLangCompat(ptr<syntax.BasicLit> _addr_lit) {
    ref syntax.BasicLit lit = ref _addr_lit.val;

    var s = lit.Value;
    if (len(s) <= 2 || types.AllowsGoVersion(types.LocalPkg, 1, 13)) {
        return ;
    }
    if (strings.Contains(s, "_")) {
        @base.ErrorfVers("go1.13", "underscores in numeric literals");
        return ;
    }
    if (s[0] != '0') {
        return ;
    }
    var radix = s[1];
    if (radix == 'b' || radix == 'B') {
        @base.ErrorfVers("go1.13", "binary literals");
        return ;
    }
    if (radix == 'o' || radix == 'O') {
        @base.ErrorfVers("go1.13", "0o/0O-style octal literals");
        return ;
    }
    if (lit.Kind != syntax.IntLit && (radix == 'x' || radix == 'X')) {
        @base.ErrorfVers("go1.13", "hexadecimal floating-point literals");
    }
}

private static constant.Value basicLit(this ptr<noder> _addr_p, ptr<syntax.BasicLit> _addr_lit) {
    ref noder p = ref _addr_p.val;
    ref syntax.BasicLit lit = ref _addr_lit.val;
 
    // We don't use the errors of the conversion routines to determine
    // if a literal string is valid because the conversion routines may
    // accept a wider syntax than the language permits. Rely on lit.Bad
    // instead.
    if (lit.Bad) {
        return constant.MakeUnknown();
    }

    if (lit.Kind == syntax.IntLit || lit.Kind == syntax.FloatLit || lit.Kind == syntax.ImagLit) 
        checkLangCompat(_addr_lit); 
        // The max. mantissa precision for untyped numeric values
        // is 512 bits, or 4048 bits for each of the two integer
        // parts of a fraction for floating-point numbers that are
        // represented accurately in the go/constant package.
        // Constant literals that are longer than this many bits
        // are not meaningful; and excessively long constants may
        // consume a lot of space and time for a useless conversion.
        // Cap constant length with a generous upper limit that also
        // allows for separators between all digits.
        const nint limit = 10000;

        if (len(lit.Value) > limit) {
            p.errorAt(lit.Pos(), "excessively long constant: %s... (%d chars)", lit.Value[..(int)10], len(lit.Value));
            return constant.MakeUnknown();
        }
        var v = constant.MakeFromLiteral(lit.Value, tokenForLitKind[lit.Kind], 0);
    if (v.Kind() == constant.Unknown) { 
        // TODO(mdempsky): Better error message?
        p.errorAt(lit.Pos(), "malformed constant: %s", lit.Value);
    }
    return v;
}

private static array<token.Token> tokenForLitKind = new array<token.Token>(InitKeyedValues<token.Token>((syntax.IntLit, token.INT), (syntax.RuneLit, token.CHAR), (syntax.FloatLit, token.FLOAT), (syntax.ImagLit, token.IMAG), (syntax.StringLit, token.STRING)));

private static ptr<types.Sym> name(this ptr<noder> _addr_p, ptr<syntax.Name> _addr_name) {
    ref noder p = ref _addr_p.val;
    ref syntax.Name name = ref _addr_name.val;

    return _addr_typecheck.Lookup(name.Value)!;
}

private static ir.Node mkname(this ptr<noder> _addr_p, ptr<syntax.Name> _addr_name) {
    ref noder p = ref _addr_p.val;
    ref syntax.Name name = ref _addr_name.val;
 
    // TODO(mdempsky): Set line number?
    return mkname(_addr_p.name(name));
}

private static ir.Node wrapname(this ptr<noder> _addr_p, syntax.Node n, ir.Node x) {
    ref noder p = ref _addr_p.val;
 
    // These nodes do not carry line numbers.
    // Introduce a wrapper node to give them the correct line.

    if (x.Op() == ir.OTYPE || x.Op() == ir.OLITERAL)
    {
        if (x.Sym() == null) {
            break;
        }
        fallthrough = true;
    }
    if (fallthrough || x.Op() == ir.ONAME || x.Op() == ir.ONONAME || x.Op() == ir.OPACK)
    {
        var p = ir.NewParenExpr(p.pos(n), x);
        p.SetImplicit(true);
        return p;
        goto __switch_break0;
    }

    __switch_break0:;
    return x;
}

private static void setlineno(this ptr<noder> _addr_p, syntax.Node n) {
    ref noder p = ref _addr_p.val;

    if (n != null) {
        @base.Pos = p.pos(n);
    }
}

// error is called concurrently if files are parsed concurrently.
private static void error(this ptr<noder> _addr_p, error err) {
    ref noder p = ref _addr_p.val;

    p.err.Send(err._<syntax.Error>());
}

// pragmas that are allowed in the std lib, but don't have
// a syntax.Pragma value (see lex.go) associated with them.
private static map allowedStdPragmas = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"go:cgo_export_static":true,"go:cgo_export_dynamic":true,"go:cgo_import_static":true,"go:cgo_import_dynamic":true,"go:cgo_ldflag":true,"go:cgo_dynamic_linker":true,"go:embed":true,"go:generate":true,};

// *pragmas is the value stored in a syntax.pragmas during parsing.
private partial struct pragmas {
    public ir.PragmaFlag Flag; // collected bits
    public slice<pragmaPos> Pos; // position of each individual flag
    public slice<pragmaEmbed> Embeds;
}

private partial struct pragmaPos {
    public ir.PragmaFlag Flag;
    public syntax.Pos Pos;
}

private partial struct pragmaEmbed {
    public syntax.Pos Pos;
    public slice<@string> Patterns;
}

private static void checkUnused(this ptr<noder> _addr_p, ptr<pragmas> _addr_pragma) {
    ref noder p = ref _addr_p.val;
    ref pragmas pragma = ref _addr_pragma.val;

    foreach (var (_, pos) in pragma.Pos) {
        if (pos.Flag & pragma.Flag != 0) {
            p.errorAt(pos.Pos, "misplaced compiler directive");
        }
    }    if (len(pragma.Embeds) > 0) {
        foreach (var (_, e) in pragma.Embeds) {
            p.errorAt(e.Pos, "misplaced go:embed directive");
        }
    }
}

private static void checkUnusedDuringParse(this ptr<noder> _addr_p, ptr<pragmas> _addr_pragma) {
    ref noder p = ref _addr_p.val;
    ref pragmas pragma = ref _addr_pragma.val;

    foreach (var (_, pos) in pragma.Pos) {
        if (pos.Flag & pragma.Flag != 0) {
            p.error(new syntax.Error(Pos:pos.Pos,Msg:"misplaced compiler directive"));
        }
    }    if (len(pragma.Embeds) > 0) {
        foreach (var (_, e) in pragma.Embeds) {
            p.error(new syntax.Error(Pos:e.Pos,Msg:"misplaced go:embed directive"));
        }
    }
}

// pragma is called concurrently if files are parsed concurrently.
private static syntax.Pragma pragma(this ptr<noder> _addr_p, syntax.Pos pos, bool blankLine, @string text, syntax.Pragma old) => func((_, panic, _) => {
    ref noder p = ref _addr_p.val;

    ptr<pragmas> (pragma, _) = old._<ptr<pragmas>>();
    if (pragma == null) {
        pragma = @new<pragmas>();
    }
    if (text == "") { 
        // unused pragma; only called with old != nil.
        p.checkUnusedDuringParse(pragma);
        return null;
    }
    if (strings.HasPrefix(text, "line ")) { 
        // line directives are handled by syntax package
        panic("unreachable");
    }
    if (!blankLine) { 
        // directive must be on line by itself
        p.error(new syntax.Error(Pos:pos,Msg:"misplaced compiler directive"));
        return pragma;
    }

    if (strings.HasPrefix(text, "go:linkname "))
    {
        var f = strings.Fields(text);
        if (!(2 <= len(f) && len(f) <= 3)) {
            p.error(new syntax.Error(Pos:pos,Msg:"usage: //go:linkname localname [linkname]"));
            break;
        }
        @string target = default;
        if (len(f) == 3) {
            target = f[2];
        }
        else if (@base.Ctxt.Pkgpath != "") { 
            // Use the default object symbol name if the
            // user didn't provide one.
            target = objabi.PathToPrefix(@base.Ctxt.Pkgpath) + "." + f[1];
        }
        else
 {
            p.error(new syntax.Error(Pos:pos,Msg:"//go:linkname requires linkname argument or -p compiler flag"));
            break;
        }
        p.linknames = append(p.linknames, new linkname(pos,f[1],target));
        goto __switch_break1;
    }
    if (text == "go:embed" || strings.HasPrefix(text, "go:embed "))
    {
        var (args, err) = parseGoEmbed(text[(int)len("go:embed")..]);
        if (err != null) {
            p.error(new syntax.Error(Pos:pos,Msg:err.Error()));
        }
        if (len(args) == 0) {
            p.error(new syntax.Error(Pos:pos,Msg:"usage: //go:embed pattern..."));
            break;
        }
        pragma.Embeds = append(pragma.Embeds, new pragmaEmbed(pos,args));
        goto __switch_break1;
    }
    if (strings.HasPrefix(text, "go:cgo_import_dynamic ")) 
    {
        // This is permitted for general use because Solaris
        // code relies on it in golang.org/x/sys/unix and others.
        var fields = pragmaFields(text);
        if (len(fields) >= 4) {
            var lib = strings.Trim(fields[3], "\"");
            if (lib != "" && !safeArg(lib) && !isCgoGeneratedFile(pos)) {
                p.error(new syntax.Error(Pos:pos,Msg:fmt.Sprintf("invalid library name %q in cgo_import_dynamic directive",lib)));
            }
            p.pragcgo(pos, text);
            pragma.Flag |= pragmaFlag("go:cgo_import_dynamic");
            break;
        }
        fallthrough = true;
    }
    if (fallthrough || strings.HasPrefix(text, "go:cgo_")) 
    {
        // For security, we disallow //go:cgo_* directives other
        // than cgo_import_dynamic outside cgo-generated files.
        // Exception: they are allowed in the standard library, for runtime and syscall.
        if (!isCgoGeneratedFile(pos) && !@base.Flag.Std) {
            p.error(new syntax.Error(Pos:pos,Msg:fmt.Sprintf("//%s only allowed in cgo-generated code",text)));
        }
        p.pragcgo(pos, text);
    }
    // default: 
        var verb = text;
        {
            var i = strings.Index(text, " ");

            if (i >= 0) {
                verb = verb[..(int)i];
            }

        }
        var flag = pragmaFlag(verb);
        const var runtimePragmas = ir.Systemstack | ir.Nowritebarrier | ir.Nowritebarrierrec | ir.Yeswritebarrierrec;

        if (!@base.Flag.CompilingRuntime && flag & runtimePragmas != 0) {
            p.error(new syntax.Error(Pos:pos,Msg:fmt.Sprintf("//%s only allowed in runtime",verb)));
        }
        if (flag == 0 && !allowedStdPragmas[verb] && @base.Flag.Std) {
            p.error(new syntax.Error(Pos:pos,Msg:fmt.Sprintf("//%s is not allowed in the standard library",verb)));
        }
        pragma.Flag |= flag;
        pragma.Pos = append(pragma.Pos, new pragmaPos(flag,pos));

    __switch_break1:;

    return pragma;
});

// isCgoGeneratedFile reports whether pos is in a file
// generated by cgo, which is to say a file with name
// beginning with "_cgo_". Such files are allowed to
// contain cgo directives, and for security reasons
// (primarily misuse of linker flags), other files are not.
// See golang.org/issue/23672.
private static bool isCgoGeneratedFile(syntax.Pos pos) {
    return strings.HasPrefix(filepath.Base(filepath.Clean(fileh(pos.Base().Filename()))), "_cgo_");
}

// safeArg reports whether arg is a "safe" command-line argument,
// meaning that when it appears in a command-line, it probably
// doesn't have some special meaning other than its own name.
// This is copied from SafeArg in cmd/go/internal/load/pkg.go.
private static bool safeArg(@string name) {
    if (name == "") {
        return false;
    }
    var c = name[0];
    return '0' <= c && c <= '9' || 'A' <= c && c <= 'Z' || 'a' <= c && c <= 'z' || c == '.' || c == '_' || c == '/' || c >= utf8.RuneSelf;
}

private static ir.Node mkname(ptr<types.Sym> _addr_sym) {
    ref types.Sym sym = ref _addr_sym.val;

    var n = oldname(_addr_sym);
    if (n.Name() != null && n.Name().PkgName != null) {
        n.Name().PkgName.Used = true;
    }
    return n;
}

// parseGoEmbed parses the text following "//go:embed" to extract the glob patterns.
// It accepts unquoted space-separated patterns as well as double-quoted and back-quoted Go strings.
// go/build/read.go also processes these strings and contains similar logic.
private static (slice<@string>, error) parseGoEmbed(@string args) {
    slice<@string> _p0 = default;
    error _p0 = default!;

    slice<@string> list = default;
    args = strings.TrimSpace(args);

    while (args != "") {
        @string path = default;
Switch:

        switch (args[0]) {
            case '`': 
                i = strings.Index(args[(int)1..], "`");
                if (i < 0) {
                    return (null, error.As(fmt.Errorf("invalid quoted string in //go:embed: %s", args))!);
                }
                path = args[(int)1..(int)1 + i];
                args = args[(int)1 + i + 1..];
                break;
            case '"': 
                i = 1;
                while (i < len(args)) {
                    if (args[i] == '\\') {
                        i++;
                        continue;
                    i++;
                    }
                    if (args[i] == '"') {
                        var (q, err) = strconv.Unquote(args[..(int)i + 1]);
                        if (err != null) {
                            return (null, error.As(fmt.Errorf("invalid quoted string in //go:embed: %s", args[..(int)i + 1]))!);
                        }
                        path = q;
                        args = args[(int)i + 1..];
                        _breakSwitch = true;
                        break;
                    }
                }

                if (i >= len(args)) {
                    return (null, error.As(fmt.Errorf("invalid quoted string in //go:embed: %s", args))!);
                }
                break;
            default: 
                        var i = len(args);
                        foreach (var (j, c) in args) {
                            if (unicode.IsSpace(c)) {
                                i = j;
                                break;
                            }
                    args = strings.TrimSpace(args);
                        }
                        path = args[..(int)i];
                        args = args[(int)i..];
                break;
        }
        if (args != "") {
            var (r, _) = utf8.DecodeRuneInString(args);
            if (!unicode.IsSpace(r)) {
                return (null, error.As(fmt.Errorf("invalid quoted string in //go:embed: %s", args))!);
            }
        }
        list = append(list, path);
    }
    return (list, error.As(null!)!);
}

private static ptr<ir.Field> fakeRecv() {
    return _addr_ir.NewField(@base.Pos, null, null, types.FakeRecvType())!;
}

private static ir.Node funcLit(this ptr<noder> _addr_p, ptr<syntax.FuncLit> _addr_expr) {
    ref noder p = ref _addr_p.val;
    ref syntax.FuncLit expr = ref _addr_expr.val;

    var xtype = p.typeExpr(expr.Type);

    var fn = ir.NewFunc(p.pos(expr));
    fn.SetIsHiddenClosure(ir.CurFunc != null);

    fn.Nname = ir.NewNameAt(p.pos(expr), ir.BlankNode.Sym()); // filled in by tcClosure
    fn.Nname.Func = fn;
    fn.Nname.Ntype = xtype;
    fn.Nname.Defn = fn;

    var clo = ir.NewClosureExpr(p.pos(expr), fn);
    fn.OClosure = clo;

    p.funcBody(fn, expr.Body);

    ir.FinishCaptureNames(@base.Pos, ir.CurFunc, fn);

    return clo;
}

// A function named init is a special case.
// It is called by the initialization before main is run.
// To make it unique within a package and also uncallable,
// the name, normally "pkg.init", is altered to "pkg.init.0".
private static nint renameinitgen = default;

private static ptr<types.Sym> renameinit() {
    var s = typecheck.LookupNum("init.", renameinitgen);
    renameinitgen++;
    return _addr_s!;
}

// oldname returns the Node that declares symbol s in the current scope.
// If no such Node currently exists, an ONONAME Node is returned instead.
// Automatically creates a new closure variable if the referenced symbol was
// declared in a different (containing) function.
private static ir.Node oldname(ptr<types.Sym> _addr_s) {
    ref types.Sym s = ref _addr_s.val;

    if (s.Pkg != types.LocalPkg) {
        return ir.NewIdent(@base.Pos, s);
    }
    var n = ir.AsNode(s.Def);
    if (n == null) { 
        // Maybe a top-level declaration will come along later to
        // define s. resolve will check s.Def again once all input
        // source has been processed.
        return ir.NewIdent(@base.Pos, s);
    }
    {
        var n__prev1 = n;

        ptr<ir.Name> (n, ok) = n._<ptr<ir.Name>>();

        if (ok) { 
            // TODO(rsc): If there is an outer variable x and we
            // are parsing x := 5 inside the closure, until we get to
            // the := it looks like a reference to the outer x so we'll
            // make x a closure variable unnecessarily.
            return ir.CaptureName(@base.Pos, ir.CurFunc, n);
        }
        n = n__prev1;

    }

    return n;
}

private static src.XPos varEmbed(Func<syntax.Pos, src.XPos> makeXPos, ptr<ir.Name> _addr_name, ptr<syntax.VarDecl> _addr_decl, ptr<pragmas> _addr_pragma, bool haveEmbed) {
    ref ir.Name name = ref _addr_name.val;
    ref syntax.VarDecl decl = ref _addr_decl.val;
    ref pragmas pragma = ref _addr_pragma.val;

    if (pragma.Embeds == null) {
        return ;
    }
    var pragmaEmbeds = pragma.Embeds;
    pragma.Embeds = null;
    var pos = makeXPos(pragmaEmbeds[0].Pos);

    if (!haveEmbed) {
        @base.ErrorfAt(pos, "go:embed only allowed in Go files that import \"embed\"");
        return ;
    }
    if (len(decl.NameList) > 1) {
        @base.ErrorfAt(pos, "go:embed cannot apply to multiple vars");
        return ;
    }
    if (decl.Values != null) {
        @base.ErrorfAt(pos, "go:embed cannot apply to var with initializer");
        return ;
    }
    if (decl.Type == null) { 
        // Should not happen, since Values == nil now.
        @base.ErrorfAt(pos, "go:embed cannot apply to var without type");
        return ;
    }
    if (typecheck.DeclContext != ir.PEXTERN) {
        @base.ErrorfAt(pos, "go:embed cannot apply to var inside func");
        return ;
    }
    ref slice<ir.Embed> embeds = ref heap(out ptr<slice<ir.Embed>> _addr_embeds);
    foreach (var (_, e) in pragmaEmbeds) {
        embeds = append(embeds, new ir.Embed(Pos:makeXPos(e.Pos),Patterns:e.Patterns));
    }    typecheck.Target.Embeds = append(typecheck.Target.Embeds, name);
    _addr_name.Embed = _addr_embeds;
    name.Embed = ref _addr_name.Embed.val;
}

} // end noder_package
