// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package noder -- go2cs converted at 2022 March 13 06:27:14 UTC
// import "cmd/compile/internal/noder" ==> using noder = go.cmd.compile.@internal.noder_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\noder\func.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using syntax = cmd.compile.@internal.syntax_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;
using src = cmd.@internal.src_package;

public static partial class noder_package {

private static void funcBody(this ptr<irgen> _addr_g, ptr<ir.Func> _addr_fn, ptr<syntax.Field> _addr_recv, ptr<syntax.FuncType> _addr_sig, ptr<syntax.BlockStmt> _addr_block) {
    ref irgen g = ref _addr_g.val;
    ref ir.Func fn = ref _addr_fn.val;
    ref syntax.Field recv = ref _addr_recv.val;
    ref syntax.FuncType sig = ref _addr_sig.val;
    ref syntax.BlockStmt block = ref _addr_block.val;

    typecheck.Func(fn); 

    // TODO(mdempsky): Remove uses of ir.CurFunc and
    // typecheck.DeclContext after we stop relying on typecheck
    // for desugaring.
    var outerfn = ir.CurFunc;
    var outerctxt = typecheck.DeclContext;
    ir.CurFunc = fn;

    var typ = fn.Type();
    {
        var param__prev1 = param;

        var param = typ.Recv();

        if (param != null) {
            g.defParam(param, recv, ir.PPARAM);
        }
        param = param__prev1;

    }
    {
        var i__prev1 = i;
        var param__prev1 = param;

        foreach (var (__i, __param) in typ.Params().FieldSlice()) {
            i = __i;
            param = __param;
            g.defParam(param, sig.ParamList[i], ir.PPARAM);
        }
        i = i__prev1;
        param = param__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i, __result) in typ.Results().FieldSlice()) {
            i = __i;
            result = __result;
            g.defParam(result, sig.ResultList[i], ir.PPARAMOUT);
        }
        i = i__prev1;
    }

    typ.Align = 0;
    types.CalcSize(typ);

    if (block != null) {
        typecheck.DeclContext = ir.PAUTO;

        fn.Body = g.stmts(block.List);
        if (fn.Body == null) {
            fn.Body = new slice<ir.Node>(new ir.Node[] { ir.NewBlockStmt(src.NoXPos,nil) });
        }
        fn.Endlineno = g.makeXPos(block.Rbrace);

        if (@base.Flag.Dwarf) {
            g.recordScopes(fn, sig);
        }
    }
    (ir.CurFunc, typecheck.DeclContext) = (outerfn, outerctxt);
}

private static void defParam(this ptr<irgen> _addr_g, ptr<types.Field> _addr_param, ptr<syntax.Field> _addr_decl, ir.Class @class) {
    ref irgen g = ref _addr_g.val;
    ref types.Field param = ref _addr_param.val;
    ref syntax.Field decl = ref _addr_decl.val;

    typecheck.DeclContext = class;

    ptr<ir.Name> name;
    if (decl.Name != null) {
        name, _ = g.def(decl.Name);
    }
    else if (class == ir.PPARAMOUT) {
        name = g.obj(g.info.Implicits[decl]);
    }
    if (name != null) {
        param.Nname = name;
        param.Sym = name.Sym(); // in case it was renamed
    }
}

} // end noder_package
