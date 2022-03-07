// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package noder -- go2cs converted at 2022 March 06 23:14:21 UTC
// import "cmd/compile/internal/noder" ==> using noder = go.cmd.compile.@internal.noder_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\noder\types.go
using bytes = go.bytes_package;
using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using typecheck = go.cmd.compile.@internal.typecheck_package;
using types = go.cmd.compile.@internal.types_package;
using types2 = go.cmd.compile.@internal.types2_package;
using src = go.cmd.@internal.src_package;
using strings = go.strings_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class noder_package {

private static ptr<types.Pkg> pkg(this ptr<irgen> _addr_g, ptr<types2.Package> _addr_pkg) {
    ref irgen g = ref _addr_g.val;
    ref types2.Package pkg = ref _addr_pkg.val;


    if (pkg == null) 
        return _addr_types.BuiltinPkg!;
    else if (pkg == g.self) 
        return _addr_types.LocalPkg!;
    else if (pkg == types2.Unsafe) 
        return _addr_ir.Pkgs.Unsafe!;
        return _addr_types.NewPkg(pkg.Path(), pkg.Name())!;

}

// typ converts a types2.Type to a types.Type, including caching of previously
// translated types.
private static ptr<types.Type> typ(this ptr<irgen> _addr_g, types2.Type typ) {
    ref irgen g = ref _addr_g.val;

    var res = g.typ1(typ); 

    // Calculate the size for all concrete types seen by the frontend. The old
    // typechecker calls CheckSize() a lot, and we want to eliminate calling
    // it eventually, so we should do it here instead. We only call it for
    // top-level types (i.e. we do it here rather in typ1), to make sure that
    // recursive types have been fully constructed before we call CheckSize.
    if (res != null && !res.IsUntyped() && !res.IsFuncArgStruct() && !res.HasTParam()) {
        types.CheckSize(res);
    }
    return _addr_res!;

}

// typ1 is like typ, but doesn't call CheckSize, since it may have only
// constructed part of a recursive type. Should not be called from outside this
// file (g.typ is the "external" entry point).
private static ptr<types.Type> typ1(this ptr<irgen> _addr_g, types2.Type typ) {
    ref irgen g = ref _addr_g.val;
 
    // Cache type2-to-type mappings. Important so that each defined generic
    // type (instantiated or not) has a single types.Type representation.
    // Also saves a lot of computation and memory by avoiding re-translating
    // types2 types repeatedly.
    var (res, ok) = g.typs[typ];
    if (!ok) {
        res = g.typ0(typ);
        g.typs[typ] = res;
    }
    return _addr_res!;

}

// instTypeName2 creates a name for an instantiated type, base on the type args
// (given as types2 types).
private static @string instTypeName2(@string name, slice<types2.Type> targs) {
    var b = bytes.NewBufferString(name);
    b.WriteByte('[');
    foreach (var (i, targ) in targs) {
        if (i > 0) {
            b.WriteByte(',');
        }
        var tname = types2.TypeString(targ, _p0 => "");
        if (strings.Index(tname, ", ") >= 0) { 
            // types2.TypeString puts spaces after a comma in a type
            // list, but we don't want spaces in our actual type names
            // and method/function names derived from them.
            tname = strings.Replace(tname, ", ", ",", -1);

        }
        b.WriteString(tname);

    }    b.WriteByte(']');
    return b.String();

}

// typ0 converts a types2.Type to a types.Type, but doesn't do the caching check
// at the top level.
private static ptr<types.Type> typ0(this ptr<irgen> _addr_g, types2.Type typ) => func((_, panic, _) => {
    ref irgen g = ref _addr_g.val;

    switch (typ.type()) {
        case ptr<types2.Basic> typ:
            return _addr_g.basic(typ)!;
            break;
        case ptr<types2.Named> typ:
            if (typ.TParams() != null) { 
                // typ is an instantiation of a defined (named) generic type.
                // This instantiation should also be a defined (named) type.
                // types2 gives us the substituted type in t.Underlying()
                // The substituted type may or may not still have type
                // params. We might, for example, be substituting one type
                // param for another type param.

                if (typ.TArgs() == null) {
                    @base.Fatalf("In typ0, Targs should be set if TParams is set");
                } 

                // When converted to types.Type, typ must have a name,
                // based on the names of the type arguments. We need a
                // name to deal with recursive generic types (and it also
                // looks better when printing types).
                var instName = instTypeName2(typ.Obj().Name(), typ.TArgs());
                var s = g.pkg(typ.Obj().Pkg()).Lookup(instName);
                if (s.Def != null) { 
                    // We have already encountered this instantiation,
                    // so use the type we previously created, since there
                    // must be exactly one instance of a defined type.
                    return _addr_s.Def.Type()!;

                } 

                // Create a forwarding type first and put it in the g.typs
                // map, in order to deal with recursive generic types.
                // Fully set up the extra ntyp information (Def, RParams,
                // which may set HasTParam) before translating the
                // underlying type itself, so we handle recursion
                // correctly, including via method signatures.
                var ntyp = newIncompleteNamedType(g.pos(typ.Obj().Pos()), s);
                g.typs[typ] = ntyp; 

                // If ntyp still has type params, then we must be
                // referencing something like 'value[T2]', as when
                // specifying the generic receiver of a method,
                // where value was defined as "type value[T any]
                // ...". Save the type args, which will now be the
                // new type  of the current type.
                //
                // If ntyp does not have type params, we are saving the
                // concrete types used to instantiate this type. We'll use
                // these when instantiating the methods of the
                // instantiated type.
                var rparams = make_slice<ptr<types.Type>>(len(typ.TArgs()));
                {
                    var i__prev1 = i;

                    foreach (var (__i, __targ) in typ.TArgs()) {
                        i = __i;
                        targ = __targ;
                        rparams[i] = g.typ1(targ);
                    }

                    i = i__prev1;
                }

                ntyp.SetRParams(rparams); 
                //fmt.Printf("Saw new type %v %v\n", instName, ntyp.HasTParam())

                ntyp.SetUnderlying(g.typ1(typ.Underlying()));
                g.fillinMethods(typ, ntyp);
                return _addr_ntyp!;

            }

            var obj = g.obj(typ.Obj());
            if (obj.Op() != ir.OTYPE) {
                @base.FatalfAt(obj.Pos(), "expected type: %L", obj);
            }

            return _addr_obj.Type()!;
            break;
        case ptr<types2.Array> typ:
            return _addr_types.NewArray(g.typ1(typ.Elem()), typ.Len())!;
            break;
        case ptr<types2.Chan> typ:
            return _addr_types.NewChan(g.typ1(typ.Elem()), dirs[typ.Dir()])!;
            break;
        case ptr<types2.Map> typ:
            return _addr_types.NewMap(g.typ1(typ.Key()), g.typ1(typ.Elem()))!;
            break;
        case ptr<types2.Pointer> typ:
            return _addr_types.NewPtr(g.typ1(typ.Elem()))!;
            break;
        case ptr<types2.Signature> typ:
            return _addr_g.signature(null, typ)!;
            break;
        case ptr<types2.Slice> typ:
            return _addr_types.NewSlice(g.typ1(typ.Elem()))!;
            break;
        case ptr<types2.Struct> typ:
            var fields = make_slice<ptr<types.Field>>(typ.NumFields());
            {
                var i__prev1 = i;

                foreach (var (__i) in fields) {
                    i = __i;
                    var v = typ.Field(i);
                    var f = types.NewField(g.pos(v), g.selector(v), g.typ1(v.Type()));
                    f.Note = typ.Tag(i);
                    if (v.Embedded()) {
                        f.Embedded = 1;
                    }
                    fields[i] = f;
                }

                i = i__prev1;
            }

            return _addr_types.NewStruct(g.tpkg(typ), fields)!;
            break;
        case ptr<types2.Interface> typ:
            var embeddeds = make_slice<ptr<types.Field>>(typ.NumEmbeddeds());
            nint j = 0;
            {
                var i__prev1 = i;

                foreach (var (__i) in embeddeds) {
                    i = __i; 
                    // TODO(mdempsky): Get embedding position.
                    var e = typ.EmbeddedType(i);
                    {
                        var t__prev1 = t;

                        var t = types2.AsInterface(e);

                        if (t != null && t.IsComparable()) { 
                            // Ignore predefined type 'comparable', since it
                            // doesn't resolve and it doesn't have any
                            // relevant methods.
                            continue;

                        }

                        t = t__prev1;

                    }

                    embeddeds[j] = types.NewField(src.NoXPos, null, g.typ1(e));
                    j++;

                }

                i = i__prev1;
            }

            embeddeds = embeddeds[..(int)j];

            var methods = make_slice<ptr<types.Field>>(typ.NumExplicitMethods());
            {
                var i__prev1 = i;

                foreach (var (__i) in methods) {
                    i = __i;
                    var m = typ.ExplicitMethod(i);
                    var mtyp = g.signature(typecheck.FakeRecv(), m.Type()._<ptr<types2.Signature>>());
                    methods[i] = types.NewField(g.pos(m), g.selector(m), mtyp);
                }

                i = i__prev1;
            }

            return _addr_types.NewInterface(g.tpkg(typ), append(embeddeds, methods))!;
            break;
        case ptr<types2.TypeParam> typ:
            var tp = types.NewTypeParam(g.tpkg(typ)); 
            // Save the name of the type parameter in the sym of the type.
            // Include the types2 subscript in the sym name
            var sym = g.pkg(typ.Obj().Pkg()).Lookup(types2.TypeString(typ, _p0 => _addr_""!));
            tp.SetSym(sym); 
            // Set g.typs[typ] in case the bound methods reference typ.
            g.typs[typ] = tp; 

            // TODO(danscales): we don't currently need to use the bounds
            // anywhere, so eventually we can probably remove.
            var bound = g.typ1(typ.Bound());
            tp.Methods().val = bound.Methods().val;
            return _addr_tp!;
            break;
        case ptr<types2.Tuple> typ:
            if (typ == null) {
                return _addr_(types.Type.val)(null)!;
            }
            fields = make_slice<ptr<types.Field>>(typ.Len());
            {
                var i__prev1 = i;

                foreach (var (__i) in fields) {
                    i = __i;
                    fields[i] = g.param(typ.At(i));
                }

                i = i__prev1;
            }

            t = types.NewStruct(types.LocalPkg, fields);
            t.StructType().Funarg = types.FunargResults;
            return _addr_t!;
            break;
        default:
        {
            var typ = typ.type();
            @base.FatalfAt(src.NoXPos, "unhandled type: %v (%T)", typ, typ);
            panic("unreachable");
            break;
        }
    }

});

// fillinMethods fills in the method name nodes and types for a defined type. This
// is needed for later typechecking when looking up methods of instantiated types,
// and for actually generating the methods for instantiated types.
private static void fillinMethods(this ptr<irgen> _addr_g, ptr<types2.Named> _addr_typ, ptr<types.Type> _addr_ntyp) {
    ref irgen g = ref _addr_g.val;
    ref types2.Named typ = ref _addr_typ.val;
    ref types.Type ntyp = ref _addr_ntyp.val;

    if (typ.NumMethods() != 0) {
        var targs = make_slice<ir.Node>(len(typ.TArgs()));
        {
            var i__prev1 = i;

            foreach (var (__i, __targ) in typ.TArgs()) {
                i = __i;
                targ = __targ;
                targs[i] = ir.TypeNode(g.typ1(targ));
            }

            i = i__prev1;
        }

        var methods = make_slice<ptr<types.Field>>(typ.NumMethods());
        {
            var i__prev1 = i;

            foreach (var (__i) in methods) {
                i = __i;
                var m = typ.Method(i);
                var meth = g.obj(m);
                var recvType = types2.AsSignature(m.Type()).Recv().Type();
                var ptr = types2.AsPointer(recvType);
                if (ptr != null) {
                    recvType = ptr.Elem();
                }
                if (recvType != types2.Type(typ)) { 
                    // Unfortunately, meth is the type of the method of the
                    // generic type, so we have to do a substitution to get
                    // the name/type of the method of the instantiated type,
                    // using m.Type().RParams() and typ.TArgs()
                    var inst2 = instTypeName2("", typ.TArgs());
                    var name = meth.Sym().Name;
                    var i1 = strings.Index(name, "[");
                    var i2 = strings.Index(name[(int)i1..], "]");
                    assert(i1 >= 0 && i2 >= 0); 
                    // Generate the name of the instantiated method.
                    name = name[(int)0..(int)i1] + inst2 + name[(int)i1 + i2 + 1..];
                    var newsym = meth.Sym().Pkg.Lookup(name);
                    ptr<ir.Name> meth2;
                    if (newsym.Def != null) {
                        meth2 = newsym.Def._<ptr<ir.Name>>();
                    }
                    else
 {
                        meth2 = ir.NewNameAt(meth.Pos(), newsym);
                        var rparams = types2.AsSignature(m.Type()).RParams();
                        var tparams = make_slice<ptr<types.Field>>(len(rparams));
                        {
                            var i__prev2 = i;

                            foreach (var (__i, __rparam) in rparams) {
                                i = __i;
                                rparam = __rparam;
                                tparams[i] = types.NewField(src.NoXPos, null, g.typ1(rparam.Type()));
                            }

                            i = i__prev2;
                        }

                        assert(len(tparams) == len(targs));
                        ptr<subster> subst = addr(new subster(g:g,tparams:tparams,targs:targs,)); 
                        // Do the substitution of the type
                        meth2.SetType(subst.typ(meth.Type()));
                        newsym.Def = meth2;

                    }

                    meth = meth2;

                }

                methods[i] = types.NewField(meth.Pos(), g.selector(m), meth.Type());
                methods[i].Nname = meth;

            }

            i = i__prev1;
        }

        ntyp.Methods().Set(methods);
        if (!ntyp.HasTParam()) { 
            // Generate all the methods for a new fully-instantiated type.
            g.instTypeList = append(g.instTypeList, ntyp);

        }
    }
}

private static ptr<types.Type> signature(this ptr<irgen> _addr_g, ptr<types.Field> _addr_recv, ptr<types2.Signature> _addr_sig) {
    ref irgen g = ref _addr_g.val;
    ref types.Field recv = ref _addr_recv.val;
    ref types2.Signature sig = ref _addr_sig.val;

    var tparams2 = sig.TParams();
    var tparams = make_slice<ptr<types.Field>>(len(tparams2));
    {
        var i__prev1 = i;

        foreach (var (__i) in tparams) {
            i = __i;
            var tp = tparams2[i];
            tparams[i] = types.NewField(g.pos(tp), g.sym(tp), g.typ1(tp.Type()));
        }
        i = i__prev1;
    }

    Func<ptr<types2.Tuple>, slice<ptr<types.Field>>> @do = typ => {
        var fields = make_slice<ptr<types.Field>>(typ.Len());
        {
            var i__prev1 = i;

            foreach (var (__i) in fields) {
                i = __i;
                fields[i] = g.param(typ.At(i));
            }

            i = i__prev1;
        }

        return _addr_fields!;

    };
    var @params = do(sig.Params());
    var results = do(sig.Results());
    if (sig.Variadic()) {
        params[len(params) - 1].SetIsDDD(true);
    }
    return _addr_types.NewSignature(g.tpkg(sig), recv, tparams, params, results)!;

}

private static ptr<types.Field> param(this ptr<irgen> _addr_g, ptr<types2.Var> _addr_v) {
    ref irgen g = ref _addr_g.val;
    ref types2.Var v = ref _addr_v.val;

    return _addr_types.NewField(g.pos(v), g.sym(v), g.typ1(v.Type()))!;
}

private static ptr<types.Sym> sym(this ptr<irgen> _addr_g, types2.Object obj) {
    ref irgen g = ref _addr_g.val;

    {
        var name = obj.Name();

        if (name != "") {
            return _addr_g.pkg(obj.Pkg()).Lookup(obj.Name())!;
        }
    }

    return _addr_null!;

}

private static ptr<types.Sym> selector(this ptr<irgen> _addr_g, types2.Object obj) {
    ref irgen g = ref _addr_g.val;

    var pkg = g.pkg(obj.Pkg());
    var name = obj.Name();
    if (types.IsExported(name)) {
        pkg = types.LocalPkg;
    }
    return _addr_pkg.Lookup(name)!;

}

// tpkg returns the package that a function, interface, or struct type
// expression appeared in.
//
// Caveat: For the degenerate types "func()", "interface{}", and
// "struct{}", tpkg always returns LocalPkg. However, we only need the
// package information so that go/types can report it via its API, and
// the reason we fail to return the original package for these
// particular types is because go/types does *not* report it for
// them. So in practice this limitation is probably moot.
private static ptr<types.Pkg> tpkg(this ptr<irgen> _addr_g, types2.Type typ) {
    ref irgen g = ref _addr_g.val;

    Func<types2.Object> anyObj = () => {
        switch (typ.type()) {
            case ptr<types2.Signature> typ:
                {
                    var recv = typ.Recv();

                    if (recv != null) {
                        return _addr_recv!;
                    }

                }

                {
                    var @params = typ.Params();

                    if (@params.Len() > 0) {
                        return _addr_@params.At(0)!;
                    }

                }

                {
                    var results = typ.Results();

                    if (results.Len() > 0) {
                        return _addr_results.At(0)!;
                    }

                }

                break;
            case ptr<types2.Struct> typ:
                if (typ.NumFields() > 0) {
                    return _addr_typ.Field(0)!;
                }
                break;
            case ptr<types2.Interface> typ:
                if (typ.NumExplicitMethods() > 0) {
                    return _addr_typ.ExplicitMethod(0)!;
                }
                break;
        }
        return _addr_null!;

    };

    {
        var obj = anyObj();

        if (obj != null) {
            return _addr_g.pkg(obj.Pkg())!;
        }
    }

    return _addr_types.LocalPkg!;

}

private static ptr<types.Type> basic(this ptr<irgen> _addr_g, ptr<types2.Basic> _addr_typ) {
    ref irgen g = ref _addr_g.val;
    ref types2.Basic typ = ref _addr_typ.val;

    switch (typ.Name()) {
        case "byte": 
            return _addr_types.ByteType!;
            break;
        case "rune": 
            return _addr_types.RuneType!;
            break;
    }
    return _addr_basics[typ.Kind()].val!;

}

private static array<ptr<ptr<types.Type>>> basics = new array<ptr<ptr<types.Type>>>(InitKeyedValues<ptr<ptr<types.Type>>>((types2.Invalid, new(*types.Type)), (types2.Bool, &types.Types[types.TBOOL]), (types2.Int, &types.Types[types.TINT]), (types2.Int8, &types.Types[types.TINT8]), (types2.Int16, &types.Types[types.TINT16]), (types2.Int32, &types.Types[types.TINT32]), (types2.Int64, &types.Types[types.TINT64]), (types2.Uint, &types.Types[types.TUINT]), (types2.Uint8, &types.Types[types.TUINT8]), (types2.Uint16, &types.Types[types.TUINT16]), (types2.Uint32, &types.Types[types.TUINT32]), (types2.Uint64, &types.Types[types.TUINT64]), (types2.Uintptr, &types.Types[types.TUINTPTR]), (types2.Float32, &types.Types[types.TFLOAT32]), (types2.Float64, &types.Types[types.TFLOAT64]), (types2.Complex64, &types.Types[types.TCOMPLEX64]), (types2.Complex128, &types.Types[types.TCOMPLEX128]), (types2.String, &types.Types[types.TSTRING]), (types2.UnsafePointer, &types.Types[types.TUNSAFEPTR]), (types2.UntypedBool, &types.UntypedBool), (types2.UntypedInt, &types.UntypedInt), (types2.UntypedRune, &types.UntypedRune), (types2.UntypedFloat, &types.UntypedFloat), (types2.UntypedComplex, &types.UntypedComplex), (types2.UntypedString, &types.UntypedString), (types2.UntypedNil, &types.Types[types.TNIL])));

private static array<types.ChanDir> dirs = new array<types.ChanDir>(InitKeyedValues<types.ChanDir>((types2.SendRecv, types.Cboth), (types2.SendOnly, types.Csend), (types2.RecvOnly, types.Crecv)));

} // end noder_package
