// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package typecheck -- go2cs converted at 2022 March 06 22:48:08 UTC
// import "cmd/compile/internal/typecheck" ==> using typecheck = go.cmd.compile.@internal.typecheck_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\typecheck\export.go
using constant = go.go.constant_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;

namespace go.cmd.compile.@internal;

public static partial class typecheck_package {

    // importalias declares symbol s as an imported type alias with type t.
    // ipkg is the package being imported
private static ptr<ir.Name> importalias(ptr<types.Pkg> _addr_ipkg, src.XPos pos, ptr<types.Sym> _addr_s, ptr<types.Type> _addr_t) {
    ref types.Pkg ipkg = ref _addr_ipkg.val;
    ref types.Sym s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_importobj(_addr_ipkg, pos, _addr_s, ir.OTYPE, ir.PEXTERN, _addr_t)!;
}

// importconst declares symbol s as an imported constant with type t and value val.
// ipkg is the package being imported
private static ptr<ir.Name> importconst(ptr<types.Pkg> _addr_ipkg, src.XPos pos, ptr<types.Sym> _addr_s, ptr<types.Type> _addr_t, constant.Value val) {
    ref types.Pkg ipkg = ref _addr_ipkg.val;
    ref types.Sym s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    var n = importobj(_addr_ipkg, pos, _addr_s, ir.OLITERAL, ir.PEXTERN, _addr_t);
    n.SetVal(val);
    return _addr_n!;
}

// importfunc declares symbol s as an imported function with type t.
// ipkg is the package being imported
private static ptr<ir.Name> importfunc(ptr<types.Pkg> _addr_ipkg, src.XPos pos, ptr<types.Sym> _addr_s, ptr<types.Type> _addr_t) {
    ref types.Pkg ipkg = ref _addr_ipkg.val;
    ref types.Sym s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    var n = importobj(_addr_ipkg, pos, _addr_s, ir.ONAME, ir.PFUNC, _addr_t);
    n.Func = ir.NewFunc(pos);
    n.Func.Nname = n;
    return _addr_n!;
}

// importobj declares symbol s as an imported object representable by op.
// ipkg is the package being imported
private static ptr<ir.Name> importobj(ptr<types.Pkg> _addr_ipkg, src.XPos pos, ptr<types.Sym> _addr_s, ir.Op op, ir.Class ctxt, ptr<types.Type> _addr_t) {
    ref types.Pkg ipkg = ref _addr_ipkg.val;
    ref types.Sym s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    var n = importsym(_addr_ipkg, pos, _addr_s, op, ctxt);
    n.SetType(t);
    if (ctxt == ir.PFUNC) {
        n.Sym().SetFunc(true);
    }
    return _addr_n!;

}

private static ptr<ir.Name> importsym(ptr<types.Pkg> _addr_ipkg, src.XPos pos, ptr<types.Sym> _addr_s, ir.Op op, ir.Class ctxt) {
    ref types.Pkg ipkg = ref _addr_ipkg.val;
    ref types.Sym s = ref _addr_s.val;

    {
        var n__prev1 = n;

        var n = s.PkgDef();

        if (n != null) {
            @base.Fatalf("importsym of symbol that already exists: %v", n);
        }
        n = n__prev1;

    }


    n = ir.NewDeclNameAt(pos, op, s);
    n.Class = ctxt; // TODO(mdempsky): Move this into NewDeclNameAt too?
    s.SetPkgDef(n);
    return _addr_n!;

}

// importtype returns the named type declared by symbol s.
// If no such type has been declared yet, a forward declaration is returned.
// ipkg is the package being imported
private static ptr<ir.Name> importtype(ptr<types.Pkg> _addr_ipkg, src.XPos pos, ptr<types.Sym> _addr_s) {
    ref types.Pkg ipkg = ref _addr_ipkg.val;
    ref types.Sym s = ref _addr_s.val;

    var n = importsym(_addr_ipkg, pos, _addr_s, ir.OTYPE, ir.PEXTERN);
    n.SetType(types.NewNamed(n));
    return _addr_n!;
}

// importvar declares symbol s as an imported variable with type t.
// ipkg is the package being imported
private static ptr<ir.Name> importvar(ptr<types.Pkg> _addr_ipkg, src.XPos pos, ptr<types.Sym> _addr_s, ptr<types.Type> _addr_t) {
    ref types.Pkg ipkg = ref _addr_ipkg.val;
    ref types.Sym s = ref _addr_s.val;
    ref types.Type t = ref _addr_t.val;

    return _addr_importobj(_addr_ipkg, pos, _addr_s, ir.ONAME, ir.PEXTERN, _addr_t)!;
}

} // end typecheck_package
