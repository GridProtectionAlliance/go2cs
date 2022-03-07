// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package typecheck -- go2cs converted at 2022 March 06 22:48:56 UTC
// import "cmd/compile/internal/typecheck" ==> using typecheck = go.cmd.compile.@internal.typecheck_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\typecheck\universe.go
using constant = go.go.constant_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class typecheck_package {

private static array<slice<bool>> okfor = new array<slice<bool>>(ir.OEND);private static array<bool> iscmp = new array<bool>(ir.OEND);

private static array<bool> okforeq = new array<bool>(types.NTYPE);private static array<bool> okforadd = new array<bool>(types.NTYPE);private static array<bool> okforand = new array<bool>(types.NTYPE);private static array<bool> okfornone = new array<bool>(types.NTYPE);private static array<bool> okforbool = new array<bool>(types.NTYPE);private static array<bool> okforcap = new array<bool>(types.NTYPE);private static array<bool> okforlen = new array<bool>(types.NTYPE);private static array<bool> okforarith = new array<bool>(types.NTYPE);









// InitUniverse initializes the universe block.
public static void InitUniverse() {
    if (types.PtrSize == 0) {
        @base.Fatalf("typeinit before betypeinit");
    }
    types.SlicePtrOffset = 0;
    types.SliceLenOffset = types.Rnd(types.SlicePtrOffset + int64(types.PtrSize), int64(types.PtrSize));
    types.SliceCapOffset = types.Rnd(types.SliceLenOffset + int64(types.PtrSize), int64(types.PtrSize));
    types.SliceSize = types.Rnd(types.SliceCapOffset + int64(types.PtrSize), int64(types.PtrSize)); 

    // string is same as slice wo the cap
    types.StringSize = types.Rnd(types.SliceLenOffset + int64(types.PtrSize), int64(types.PtrSize));

    {
        var et__prev1 = et;

        for (var et = types.Kind(0); et < types.NTYPE; et++) {
            types.SimType[et] = et;
        }

        et = et__prev1;
    }

    types.Types[types.TANY] = types.New(types.TANY);
    types.Types[types.TINTER] = types.NewInterface(types.LocalPkg, null);

    Func<types.Kind, ptr<types.Pkg>, @string, ptr<types.Type>> defBasic = (kind, pkg, name) => {
        var sym = pkg.Lookup(name);
        var n = ir.NewDeclNameAt(src.NoXPos, ir.OTYPE, sym);
        var t = types.NewBasic(kind, n);
        n.SetType(t);
        sym.Def = n;
        if (kind != types.TANY) {
            types.CalcSize(t);
        }
        return t;

    };

    {
        var s__prev1 = s;

        foreach (var (_, __s) in _addr_basicTypes) {
            s = __s;
            types.Types[s.etype] = defBasic(s.etype, types.BuiltinPkg, s.name);
        }
        s = s__prev1;
    }

    {
        var s__prev1 = s;

        foreach (var (_, __s) in _addr_typedefs) {
            s = __s;
            var sameas = s.sameas32;
            if (types.PtrSize == 8) {
                sameas = s.sameas64;
            }
            types.SimType[s.etype] = sameas;

            types.Types[s.etype] = defBasic(s.etype, types.BuiltinPkg, s.name);
        }
        s = s__prev1;
    }

    types.ByteType = defBasic(types.TUINT8, types.BuiltinPkg, "byte");
    types.RuneType = defBasic(types.TINT32, types.BuiltinPkg, "rune"); 

    // error type
    var s = types.BuiltinPkg.Lookup("error");
    n = ir.NewDeclNameAt(src.NoXPos, ir.OTYPE, s);
    types.ErrorType = types.NewNamed(n);
    types.ErrorType.SetUnderlying(makeErrorInterface());
    n.SetType(types.ErrorType);
    s.Def = n;
    types.CalcSize(types.ErrorType);

    types.Types[types.TUNSAFEPTR] = defBasic(types.TUNSAFEPTR, ir.Pkgs.Unsafe, "Pointer"); 

    // simple aliases
    types.SimType[types.TMAP] = types.TPTR;
    types.SimType[types.TCHAN] = types.TPTR;
    types.SimType[types.TFUNC] = types.TPTR;
    types.SimType[types.TUNSAFEPTR] = types.TPTR;

    {
        var s__prev1 = s;

        foreach (var (_, __s) in _addr_builtinFuncs) {
            s = __s;
            var s2 = types.BuiltinPkg.Lookup(s.name);
            var def = NewName(s2);
            def.BuiltinOp = s.op;
            s2.Def = def;
        }
        s = s__prev1;
    }

    {
        var s__prev1 = s;

        foreach (var (_, __s) in _addr_unsafeFuncs) {
            s = __s;
            s2 = ir.Pkgs.Unsafe.Lookup(s.name);
            def = NewName(s2);
            def.BuiltinOp = s.op;
            s2.Def = def;
        }
        s = s__prev1;
    }

    s = types.BuiltinPkg.Lookup("true");
    s.Def = ir.NewConstAt(src.NoXPos, s, types.UntypedBool, constant.MakeBool(true));

    s = types.BuiltinPkg.Lookup("false");
    s.Def = ir.NewConstAt(src.NoXPos, s, types.UntypedBool, constant.MakeBool(false));

    s = Lookup("_");
    types.BlankSym = s;
    s.Block = -100;
    s.Def = NewName(s);
    types.Types[types.TBLANK] = types.New(types.TBLANK);
    ir.AsNode(s.Def).SetType(types.Types[types.TBLANK]);
    ir.BlankNode = ir.AsNode(s.Def);
    ir.BlankNode.SetTypecheck(1);

    s = types.BuiltinPkg.Lookup("_");
    s.Block = -100;
    s.Def = NewName(s);
    types.Types[types.TBLANK] = types.New(types.TBLANK);
    ir.AsNode(s.Def).SetType(types.Types[types.TBLANK]);

    types.Types[types.TNIL] = types.New(types.TNIL);
    s = types.BuiltinPkg.Lookup("nil");
    var nnil = NodNil();
    nnil._<ptr<ir.NilExpr>>().SetSym(s);
    s.Def = nnil;

    s = types.BuiltinPkg.Lookup("iota");
    s.Def = ir.NewIota(@base.Pos, s);

    {
        var et__prev1 = et;

        for (et = types.TINT8; et <= types.TUINT64; et++) {
            types.IsInt[et] = true;
        }

        et = et__prev1;
    }
    types.IsInt[types.TINT] = true;
    types.IsInt[types.TUINT] = true;
    types.IsInt[types.TUINTPTR] = true;

    types.IsFloat[types.TFLOAT32] = true;
    types.IsFloat[types.TFLOAT64] = true;

    types.IsComplex[types.TCOMPLEX64] = true;
    types.IsComplex[types.TCOMPLEX128] = true; 

    // initialize okfor
    {
        var et__prev1 = et;

        for (et = types.Kind(0); et < types.NTYPE; et++) {
            if (types.IsInt[et] || et == types.TIDEAL) {
                okforeq[et] = true;
                types.IsOrdered[et] = true;
                okforarith[et] = true;
                okforadd[et] = true;
                okforand[et] = true;
                ir.OKForConst[et] = true;
                types.IsSimple[et] = true;
            }
            if (types.IsFloat[et]) {
                okforeq[et] = true;
                types.IsOrdered[et] = true;
                okforadd[et] = true;
                okforarith[et] = true;
                ir.OKForConst[et] = true;
                types.IsSimple[et] = true;
            }
            if (types.IsComplex[et]) {
                okforeq[et] = true;
                okforadd[et] = true;
                okforarith[et] = true;
                ir.OKForConst[et] = true;
                types.IsSimple[et] = true;
            }
        }

        et = et__prev1;
    }

    types.IsSimple[types.TBOOL] = true;

    okforadd[types.TSTRING] = true;

    okforbool[types.TBOOL] = true;

    okforcap[types.TARRAY] = true;
    okforcap[types.TCHAN] = true;
    okforcap[types.TSLICE] = true;

    ir.OKForConst[types.TBOOL] = true;
    ir.OKForConst[types.TSTRING] = true;

    okforlen[types.TARRAY] = true;
    okforlen[types.TCHAN] = true;
    okforlen[types.TMAP] = true;
    okforlen[types.TSLICE] = true;
    okforlen[types.TSTRING] = true;

    okforeq[types.TPTR] = true;
    okforeq[types.TUNSAFEPTR] = true;
    okforeq[types.TINTER] = true;
    okforeq[types.TCHAN] = true;
    okforeq[types.TSTRING] = true;
    okforeq[types.TBOOL] = true;
    okforeq[types.TMAP] = true; // nil only; refined in typecheck
    okforeq[types.TFUNC] = true; // nil only; refined in typecheck
    okforeq[types.TSLICE] = true; // nil only; refined in typecheck
    okforeq[types.TARRAY] = true; // only if element type is comparable; refined in typecheck
    okforeq[types.TSTRUCT] = true; // only if all struct fields are comparable; refined in typecheck

    types.IsOrdered[types.TSTRING] = true;

    foreach (var (i) in okfor) {
        okfor[i] = okfornone[..];
    }    okfor[ir.OADD] = okforadd[..];
    okfor[ir.OAND] = okforand[..];
    okfor[ir.OANDAND] = okforbool[..];
    okfor[ir.OANDNOT] = okforand[..];
    okfor[ir.ODIV] = okforarith[..];
    okfor[ir.OEQ] = okforeq[..];
    okfor[ir.OGE] = types.IsOrdered[..];
    okfor[ir.OGT] = types.IsOrdered[..];
    okfor[ir.OLE] = types.IsOrdered[..];
    okfor[ir.OLT] = types.IsOrdered[..];
    okfor[ir.OMOD] = okforand[..];
    okfor[ir.OMUL] = okforarith[..];
    okfor[ir.ONE] = okforeq[..];
    okfor[ir.OOR] = okforand[..];
    okfor[ir.OOROR] = okforbool[..];
    okfor[ir.OSUB] = okforarith[..];
    okfor[ir.OXOR] = okforand[..];
    okfor[ir.OLSH] = okforand[..];
    okfor[ir.ORSH] = okforand[..]; 

    // unary
    okfor[ir.OBITNOT] = okforand[..];
    okfor[ir.ONEG] = okforarith[..];
    okfor[ir.ONOT] = okforbool[..];
    okfor[ir.OPLUS] = okforarith[..]; 

    // special
    okfor[ir.OCAP] = okforcap[..];
    okfor[ir.OLEN] = okforlen[..]; 

    // comparison
    iscmp[ir.OLT] = true;
    iscmp[ir.OGT] = true;
    iscmp[ir.OGE] = true;
    iscmp[ir.OLE] = true;
    iscmp[ir.OEQ] = true;
    iscmp[ir.ONE] = true;

}

private static ptr<types.Type> makeErrorInterface() {
    var sig = types.NewSignature(types.NoPkg, fakeRecvField(), null, null, new slice<ptr<types.Field>>(new ptr<types.Field>[] { types.NewField(src.NoXPos,nil,types.Types[types.TSTRING]) }));
    var method = types.NewField(src.NoXPos, Lookup("Error"), sig);
    return _addr_types.NewInterface(types.NoPkg, new slice<ptr<types.Field>>(new ptr<types.Field>[] { method }))!;
}

// DeclareUniverse makes the universe block visible within the current package.
public static void DeclareUniverse() { 
    // Operationally, this is similar to a dot import of builtinpkg, except
    // that we silently skip symbols that are already declared in the
    // package block rather than emitting a redeclared symbol error.

    foreach (var (_, s) in types.BuiltinPkg.Syms) {
        if (s.Def == null) {
            continue;
        }
        var s1 = Lookup(s.Name);
        if (s1.Def != null) {
            continue;
        }
        s1.Def = s.Def;
        s1.Block = s.Block;

    }
}

} // end typecheck_package
