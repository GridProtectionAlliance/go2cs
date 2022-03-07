// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package noder -- go2cs converted at 2022 March 06 23:14:07 UTC
// import "cmd/compile/internal/noder" ==> using noder = go.cmd.compile.@internal.noder_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\noder\object.go
using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using syntax = go.cmd.compile.@internal.syntax_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using types2 = go.cmd.compile.@internal.types2_package;
using src = go.cmd.@internal.src_package;

namespace go.cmd.compile.@internal;

public static partial class noder_package {

private static (ptr<ir.Name>, types2.Object) def(this ptr<irgen> _addr_g, ptr<syntax.Name> _addr_name) {
    ptr<ir.Name> _p0 = default!;
    types2.Object _p0 = default;
    ref irgen g = ref _addr_g.val;
    ref syntax.Name name = ref _addr_name.val;

    var (obj, ok) = g.info.Defs[name];
    if (!ok) {
        @base.FatalfAt(g.pos(name), "unknown name %v", name);
    }
    return (_addr_g.obj(obj)!, obj);

}

// use returns the Name node associated with the use of name. The returned node
// will have the correct type and be marked as typechecked.
private static ptr<ir.Name> use(this ptr<irgen> _addr_g, ptr<syntax.Name> _addr_name) {
    ref irgen g = ref _addr_g.val;
    ref syntax.Name name = ref _addr_name.val;

    var (obj2, ok) = g.info.Uses[name];
    if (!ok) {
        @base.FatalfAt(g.pos(name), "unknown name %v", name);
    }
    var obj = ir.CaptureName(g.pos(obj2), ir.CurFunc, g.obj(obj2));
    if (obj.Defn != null && obj.Defn.Op() == ir.ONAME) { 
        // If CaptureName created a closure variable, then transfer the
        // type of the captured name to the new closure variable.
        obj.SetTypecheck(1);
        obj.SetType(obj.Defn.Type());

    }
    return _addr_obj!;

}

// obj returns the Name that represents the given object. If no such Name exists
// yet, it will be implicitly created. The returned node will have the correct
// type and be marked as typechecked.
//
// For objects declared at function scope, ir.CurFunc must already be
// set to the respective function when the Name is created.
private static ptr<ir.Name> obj(this ptr<irgen> _addr_g, types2.Object obj) {
    ref irgen g = ref _addr_g.val;
 
    // For imported objects, we use iimport directly instead of mapping
    // the types2 representation.
    if (obj.Pkg() != g.self) {
        var sym = g.sym(obj);
        if (sym.Def != null) {
            return sym.Def._<ptr<ir.Name>>();
        }
        var n = typecheck.Resolve(ir.NewIdent(src.NoXPos, sym));
        {
            var n__prev2 = n;

            ptr<ir.Name> (n, ok) = n._<ptr<ir.Name>>();

            if (ok) {
                n.SetTypecheck(1);
                return _addr_n!;
            }

            n = n__prev2;

        }

        @base.FatalfAt(g.pos(obj), "failed to resolve %v", obj);

    }
    {
        var name__prev1 = name;

        var (name, ok) = g.objs[obj];

        if (ok) {
            return _addr_name!; // previously mapped
        }
        name = name__prev1;

    }


    ptr<ir.Name> name;
    var pos = g.pos(obj);

    var @class = typecheck.DeclContext;
    if (obj.Parent() == g.self.Scope()) {
        class = ir.PEXTERN; // forward reference to package-block declaration
    }
    switch (obj.type()) {
        case ptr<types2.Const> obj:
            name = g.objCommon(pos, ir.OLITERAL, g.sym(obj), class, g.typ(obj.Type()));
            break;
        case ptr<types2.Func> obj:
            ptr<types2.Signature> sig = obj.Type()._<ptr<types2.Signature>>();
            sym = ;
            ptr<types.Type> typ;
            {
                var recv = sig.Recv();

                if (recv == null) {
                    if (obj.Name() == "init") {
                        sym = renameinit();
                    }
                    else
 {
                        sym = g.sym(obj);
                    }

                    typ = g.typ(sig);

                }
                else
 {
                    sym = g.selector(obj);
                    if (!sym.IsBlank()) {
                        sym = ir.MethodSym(g.typ(recv.Type()), sym);
                    }
                    typ = g.signature(g.param(recv), sig);
                }

            }

            name = g.objCommon(pos, ir.ONAME, sym, ir.PFUNC, typ);
            break;
        case ptr<types2.TypeName> obj:
            if (obj.IsAlias()) {
                name = g.objCommon(pos, ir.OTYPE, g.sym(obj), class, g.typ(obj.Type()));
            }
            else
 {
                name = ir.NewDeclNameAt(pos, ir.OTYPE, g.sym(obj));
                g.objFinish(name, class, types.NewNamed(name));
            }

            break;
        case ptr<types2.Var> obj:
            sym = ;
            if (class == ir.PPARAMOUT) { 
                // Backend needs names for result parameters,
                // even if they're anonymous or blank.
                switch (obj.Name()) {
                    case "": 
                        sym = typecheck.LookupNum("~r", len(ir.CurFunc.Dcl)); // 'r' for "result"
                        break;
                    case "_": 
                        sym = typecheck.LookupNum("~b", len(ir.CurFunc.Dcl)); // 'b' for "blank"
                        break;
                }

            }

            if (sym == null) {
                sym = g.sym(obj);
            }

            name = g.objCommon(pos, ir.ONAME, sym, class, g.typ(obj.Type()));
            break;
        default:
        {
            var obj = obj.type();
            g.unhandled("object", obj);
            break;
        }

    }

    g.objs[obj] = name;
    name.SetTypecheck(1);
    return _addr_name!;

}

private static ptr<ir.Name> objCommon(this ptr<irgen> _addr_g, src.XPos pos, ir.Op op, ptr<types.Sym> _addr_sym, ir.Class @class, ptr<types.Type> _addr_typ) {
    ref irgen g = ref _addr_g.val;
    ref types.Sym sym = ref _addr_sym.val;
    ref types.Type typ = ref _addr_typ.val;

    var name = ir.NewDeclNameAt(pos, op, sym);
    g.objFinish(name, class, typ);
    return _addr_name!;
}

private static void objFinish(this ptr<irgen> _addr_g, ptr<ir.Name> _addr_name, ir.Class @class, ptr<types.Type> _addr_typ) {
    ref irgen g = ref _addr_g.val;
    ref ir.Name name = ref _addr_name.val;
    ref types.Type typ = ref _addr_typ.val;

    var sym = name.Sym();

    name.SetType(typ);
    name.Class = class;
    if (name.Class == ir.PFUNC) {
        sym.SetFunc(true);
    }
    name.SetTypecheck(1);
    name.SetWalkdef(1);

    if (ir.IsBlank(name)) {
        return ;
    }

    if (class == ir.PEXTERN)
    {
        g.target.Externs = append(g.target.Externs, name);
        fallthrough = true;
    }
    if (fallthrough || class == ir.PFUNC)
    {
        sym.Def = name;
        if (name.Class == ir.PFUNC && name.Type().Recv() != null) {
            break; // methods are exported with their receiver type
        }
        if (types.IsExported(sym.Name)) {
            if (name.Class == ir.PFUNC && name.Type().NumTParams() > 0) {
                @base.FatalfAt(name.Pos(), "Cannot export a generic function (yet): %v", name);
            }
            typecheck.Export(name);
        }
        if (@base.Flag.AsmHdr != "" && !name.Sym().Asm()) {
            name.Sym().SetAsm(true);
            g.target.Asms = append(g.target.Asms, name);
        }
        goto __switch_break0;
    }
    // default: 
        // Function-scoped declaration.
        name.Curfn = ir.CurFunc;
        if (name.Op() == ir.ONAME) {
            ir.CurFunc.Dcl = append(ir.CurFunc.Dcl, name);
        }

    __switch_break0:;

}

} // end noder_package
