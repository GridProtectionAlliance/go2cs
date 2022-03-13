// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package noder -- go2cs converted at 2022 March 13 06:25:38 UTC
// import "cmd/compile/internal/noder" ==> using noder = go.cmd.compile.@internal.noder_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\noder\decl.go
namespace go.cmd.compile.@internal;

using constant = go.constant_package;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using syntax = cmd.compile.@internal.syntax_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;
using types2 = cmd.compile.@internal.types2_package;


// TODO(mdempsky): Skip blank declarations? Probably only safe
// for declarations without pragmas.

public static partial class noder_package {

private static slice<ir.Node> decls(this ptr<irgen> _addr_g, slice<syntax.Decl> decls) {
    ref irgen g = ref _addr_g.val;

    ref ir.Nodes res = ref heap(out ptr<ir.Nodes> _addr_res);
    {
        var decl__prev1 = decl;

        foreach (var (_, __decl) in decls) {
            decl = __decl;
            switch (decl.type()) {
                case ptr<syntax.ConstDecl> decl:
                    g.constDecl(_addr_res, decl);
                    break;
                case ptr<syntax.FuncDecl> decl:
                    g.funcDecl(_addr_res, decl);
                    break;
                case ptr<syntax.TypeDecl> decl:
                    if (ir.CurFunc == null) {
                        continue; // already handled in irgen.generate
                    }
                    g.typeDecl(_addr_res, decl);
                    break;
                case ptr<syntax.VarDecl> decl:
                    g.varDecl(_addr_res, decl);
                    break;
                default:
                {
                    var decl = decl.type();
                    g.unhandled("declaration", decl);
                    break;
                }
            }
        }
        decl = decl__prev1;
    }

    return res;
}

private static void importDecl(this ptr<irgen> _addr_g, ptr<noder> _addr_p, ptr<syntax.ImportDecl> _addr_decl) {
    ref irgen g = ref _addr_g.val;
    ref noder p = ref _addr_p.val;
    ref syntax.ImportDecl decl = ref _addr_decl.val;
 
    // TODO(mdempsky): Merge with gcimports so we don't have to import
    // packages twice.

    g.pragmaFlags(decl.Pragma, 0);

    var ipkg = importfile(decl);
    if (ipkg == ir.Pkgs.Unsafe) {
        p.importedUnsafe = true;
    }
    if (ipkg.Path == "embed") {
        p.importedEmbed = true;
    }
}

private static void constDecl(this ptr<irgen> _addr_g, ptr<ir.Nodes> _addr_@out, ptr<syntax.ConstDecl> _addr_decl) {
    ref irgen g = ref _addr_g.val;
    ref ir.Nodes @out = ref _addr_@out.val;
    ref syntax.ConstDecl decl = ref _addr_decl.val;

    g.pragmaFlags(decl.Pragma, 0);

    {
        var name__prev1 = name;

        foreach (var (_, __name) in decl.NameList) {
            name = __name;
            var (name, obj) = g.def(name); 

            // For untyped numeric constants, make sure the value
            // representation matches what the rest of the
            // compiler (really just iexport) expects.
            // TODO(mdempsky): Revisit after #43891 is resolved.
            ptr<types2.Const> val = obj._<ptr<types2.Const>>().Val();

            if (name.Type() == types.UntypedInt || name.Type() == types.UntypedRune) 
                val = constant.ToInt(val);
            else if (name.Type() == types.UntypedFloat) 
                val = constant.ToFloat(val);
            else if (name.Type() == types.UntypedComplex) 
                val = constant.ToComplex(val);
                        name.SetVal(val);

            @out.Append(ir.NewDecl(g.pos(decl), ir.ODCLCONST, name));
        }
        name = name__prev1;
    }
}

private static void funcDecl(this ptr<irgen> _addr_g, ptr<ir.Nodes> _addr_@out, ptr<syntax.FuncDecl> _addr_decl) {
    ref irgen g = ref _addr_g.val;
    ref ir.Nodes @out = ref _addr_@out.val;
    ref syntax.FuncDecl decl = ref _addr_decl.val;

    var fn = ir.NewFunc(g.pos(decl));
    fn.Nname, _ = g.def(decl.Name);
    fn.Nname.Func = fn;
    fn.Nname.Defn = fn;

    fn.Pragma = g.pragmaFlags(decl.Pragma, funcPragmas);
    if (fn.Pragma & ir.Systemstack != 0 && fn.Pragma & ir.Nosplit != 0) {
        @base.ErrorfAt(fn.Pos(), "go:nosplit and go:systemstack cannot be combined");
    }
    if (decl.Name.Value == "init" && decl.Recv == null) {
        g.target.Inits = append(g.target.Inits, fn);
    }
    g.funcBody(fn, decl.Recv, decl.Type, decl.Body);

    @out.Append(fn);
}

private static void typeDecl(this ptr<irgen> _addr_g, ptr<ir.Nodes> _addr_@out, ptr<syntax.TypeDecl> _addr_decl) {
    ref irgen g = ref _addr_g.val;
    ref ir.Nodes @out = ref _addr_@out.val;
    ref syntax.TypeDecl decl = ref _addr_decl.val;

    if (decl.Alias) {
        var (name, _) = g.def(decl.Name);
        g.pragmaFlags(decl.Pragma, 0); 

        // TODO(mdempsky): This matches how typecheckdef marks aliases for
        // export, but this won't generalize to exporting function-scoped
        // type aliases. We should maybe just use n.Alias() instead.
        if (ir.CurFunc == null) {
            name.Sym().Def = ir.TypeNode(name.Type());
        }
        @out.Append(ir.NewDecl(g.pos(decl), ir.ODCLTYPE, name));
        return ;
    }
    types.DeferCheckSize();

    var (name, obj) = g.def(decl.Name);
    var ntyp = name.Type();
    var otyp = obj.Type();
    if (ir.CurFunc != null) {
        typecheck.TypeGen++;
        ntyp.Vargen = typecheck.TypeGen;
    }
    var pragmas = g.pragmaFlags(decl.Pragma, typePragmas);
    name.SetPragma(pragmas); // TODO(mdempsky): Is this still needed?

    if (pragmas & ir.NotInHeap != 0) {
        ntyp.SetNotInHeap(true);
    }
    ntyp.SetUnderlying(g.typeExpr(decl.Type));
    if (len(decl.TParamList) > 0) { 
        // Set HasTParam if there are any tparams, even if no tparams are
        // used in the type itself (e.g., if it is an empty struct, or no
        // fields in the struct use the tparam).
        ntyp.SetHasTParam(true);
    }
    types.ResumeCheckSize();

    {
        var otyp__prev1 = otyp;

        ptr<types2.Named> (otyp, ok) = otyp._<ptr<types2.Named>>();

        if (ok && otyp.NumMethods() != 0) {
            var methods = make_slice<ptr<types.Field>>(otyp.NumMethods());
            foreach (var (i) in methods) {
                var m = otyp.Method(i);
                var meth = g.obj(m);
                methods[i] = types.NewField(meth.Pos(), g.selector(m), meth.Type());
                methods[i].Nname = meth;
            }
            ntyp.Methods().Set(methods);
        }
        otyp = otyp__prev1;

    }

    @out.Append(ir.NewDecl(g.pos(decl), ir.ODCLTYPE, name));
}

private static void varDecl(this ptr<irgen> _addr_g, ptr<ir.Nodes> _addr_@out, ptr<syntax.VarDecl> _addr_decl) {
    ref irgen g = ref _addr_g.val;
    ref ir.Nodes @out = ref _addr_@out.val;
    ref syntax.VarDecl decl = ref _addr_decl.val;

    var pos = g.pos(decl);
    var names = make_slice<ptr<ir.Name>>(len(decl.NameList));
    {
        var i__prev1 = i;
        var name__prev1 = name;

        foreach (var (__i, __name) in decl.NameList) {
            i = __i;
            name = __name;
            names[i], _ = g.def(name);
        }
        i = i__prev1;
        name = name__prev1;
    }

    var values = g.exprList(decl.Values);

    if (decl.Pragma != null) {
        ptr<pragmas> pragma = decl.Pragma._<ptr<pragmas>>(); 
        // TODO(mdempsky): Plumb noder.importedEmbed through to here.
        varEmbed(g.makeXPos, names[0], decl, pragma, true);
        g.reportUnused(pragma);
    }
    ptr<ir.AssignListStmt> as2;
    if (len(values) != 0 && len(names) != len(values)) {
        as2 = ir.NewAssignListStmt(pos, ir.OAS2, make_slice<ir.Node>(len(names)), values);
    }
    {
        var i__prev1 = i;
        var name__prev1 = name;

        foreach (var (__i, __name) in names) {
            i = __i;
            name = __name;
            if (ir.CurFunc != null) {
                @out.Append(ir.NewDecl(pos, ir.ODCL, name));
            }
            if (as2 != null) {
                as2.Lhs[i] = name;
                name.Defn = as2;
            }
            else
 {
                var @as = ir.NewAssignStmt(pos, name, null);
                if (len(values) != 0) {
                    @as.Y = values[i];
                    name.Defn = as;
                }
                else if (ir.CurFunc == null) {
                    name.Defn = as;
                }
                ir.Node lhs = new slice<ir.Node>(new ir.Node[] { as.X });
                ir.Node rhs = new slice<ir.Node>(new ir.Node[] {  });
                if (@as.Y != null) {
                    rhs = new slice<ir.Node>(new ir.Node[] { as.Y });
                }
                transformAssign(as, lhs, rhs);
                @as.X = lhs[0];
                if (@as.Y != null) {
                    @as.Y = rhs[0];
                }
                @as.SetTypecheck(1);
                @out.Append(as);
            }
        }
        i = i__prev1;
        name = name__prev1;
    }

    if (as2 != null) {
        transformAssign(as2, as2.Lhs, as2.Rhs);
        as2.SetTypecheck(1);
        @out.Append(as2);
    }
}

// pragmaFlags returns any specified pragma flags included in allowed,
// and reports errors about any other, unexpected pragmas.
private static ir.PragmaFlag pragmaFlags(this ptr<irgen> _addr_g, syntax.Pragma pragma, ir.PragmaFlag allowed) {
    ref irgen g = ref _addr_g.val;

    if (pragma == null) {
        return 0;
    }
    ptr<pragmas> p = pragma._<ptr<pragmas>>();
    var present = p.Flag & allowed;
    p.Flag &= allowed;
    g.reportUnused(p);
    return present;
}

// reportUnused reports errors about any unused pragmas.
private static void reportUnused(this ptr<irgen> _addr_g, ptr<pragmas> _addr_pragma) {
    ref irgen g = ref _addr_g.val;
    ref pragmas pragma = ref _addr_pragma.val;

    foreach (var (_, pos) in pragma.Pos) {
        if (pos.Flag & pragma.Flag != 0) {
            @base.ErrorfAt(g.makeXPos(pos.Pos), "misplaced compiler directive");
        }
    }    if (len(pragma.Embeds) > 0) {
        foreach (var (_, e) in pragma.Embeds) {
            @base.ErrorfAt(g.makeXPos(e.Pos), "misplaced go:embed directive");
        }
    }
}

} // end noder_package
