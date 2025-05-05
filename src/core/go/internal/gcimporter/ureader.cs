// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.@internal;

using token = go.token_package;
using types = go.types_package;
using godebug = @internal.godebug_package;
using pkgbits = @internal.pkgbits_package;
using slices = slices_package;
using strings = strings_package;
using @internal;
using go;

partial class gcimporter_package {

// A pkgReader holds the shared state for reading a unified IR package
// description.
[GoType] partial struct pkgReader {
    public partial ref @internal.pkgbits_package.PkgDecoder PkgDecoder { get; }
    internal fakeFileSet fake;
    internal ж<go.types_package.Context> ctxt;
    internal types.Package imports; // previously imported packages, indexed by path
    // lazily initialized arrays corresponding to the unified IR
    // PosBase, Pkg, and Type sections, respectively.
    internal slice<@string> posBases; // position bases (i.e., file names)
    internal types.Package pkgs;
    internal typesꓸType typs;
    // laterFns holds functions that need to be invoked at the end of
    // import reading.
    internal slice<Action> laterFns;
    // ifaces holds a list of constructed Interfaces, which need to have
    // Complete called after importing is done.
    internal types.Interface ifaces;
}

// later adds a function to be invoked at the end of import reading.
[GoRecv] internal static void later(this ref pkgReader pr, Action fn) {
    pr.laterFns = append(pr.laterFns, fn);
}

// readUnifiedPackage reads a package description from the given
// unified IR export data decoder.
internal static ж<types.Package> readUnifiedPackage(ж<token.FileSet> Ꮡfset, ж<types.Context> Ꮡctxt, types.Package imports, pkgbits.PkgDecoder input) => func((defer, _) => {
    ref var fset = ref Ꮡfset.val;
    ref var ctxt = ref Ꮡctxt.val;

    ref var pr = ref heap<pkgReader>(out var Ꮡpr);
    pr = new pkgReader(
        PkgDecoder: input,
        fake: new fakeFileSet(
            fset: fset,
            files: new map<@string, ж<fileInfo>>()
        ),
        ctxt: ctxt,
        imports: imports,
        posBases: new slice<@string>(input.NumElems(pkgbits.RelocPosBase)),
        pkgs: new slice<types.Package>(input.NumElems(pkgbits.RelocPkg)),
        typs: new slice<typesꓸType>(input.NumElems(pkgbits.RelocType))
    );
    var prʗ1 = pr;
    defer(prʗ1.fake.setLines);
    var r = pr.newReader(pkgbits.RelocMeta, pkgbits.PublicRootIdx, pkgbits.SyncPublic);
    var pkg = r.pkg();
    r.Bool();
    // TODO(mdempsky): Remove; was "has init"
    for (nint i = 0;nint n = r.Len(); i < n; i++) {
        // As if r.obj(), but avoiding the Scope.Lookup call,
        // to avoid eager loading of imports.
        r.Sync(pkgbits.SyncObject);
        assert(!r.Bool());
        (~r).p.objIdx(r.Reloc(pkgbits.RelocObj));
        assert(r.Len() == 0);
    }
    r.Sync(pkgbits.SyncEOF);
    foreach (var (_, fn) in pr.laterFns) {
        fn();
    }
    foreach (var (_, iface) in pr.ifaces) {
        iface.Complete();
    }
    // Imports() of pkg are all of the transitive packages that were loaded.
    slice<types.Package> imps = default!;
    foreach (var (_, imp) in pr.pkgs) {
        if (imp != nil && imp != pkg) {
            imps = append(imps, imp);
        }
    }
    slices.SortFunc(imps, 
    (ж<types.Package> a, ж<types.Package> b) => strings.Compare(a.Path(), b.Path()));
    pkg.SetImports(imps);
    pkg.MarkComplete();
    return pkg;
});

// A reader holds the state for reading a single unified IR element
// within a package.
[GoType] partial struct reader {
    public partial ref @internal.pkgbits_package.Decoder Decoder { get; }
    internal ж<pkgReader> p;
    internal ж<readerDict> dict;
}

// A readerDict holds the state for type parameters that parameterize
// the current unified IR element.
[GoType] partial struct readerDict {
    // bounds is a slice of typeInfos corresponding to the underlying
    // bounds of the element's type parameters.
    internal slice<typeInfo> bounds;
    // tparams is a slice of the constructed TypeParams for the element.
    internal types.TypeParam tparams;
    // derived is a slice of types derived from tparams, which may be
    // instantiated while reading the current element.
    internal slice<derivedInfo> derived;
    internal typesꓸType derivedTypes; // lazily instantiated from derived
}

[GoRecv] internal static ж<reader> newReader(this ref pkgReader pr, pkgbits.RelocKind k, pkgbits.Index idx, pkgbits.SyncMarker marker) {
    return Ꮡ(new reader(
        Decoder: pr.NewDecoder(k, idx, marker),
        p: pr
    ));
}

[GoRecv] internal static ж<reader> tempReader(this ref pkgReader pr, pkgbits.RelocKind k, pkgbits.Index idx, pkgbits.SyncMarker marker) {
    return Ꮡ(new reader(
        Decoder: pr.TempDecoder(k, idx, marker),
        p: pr
    ));
}

[GoRecv] internal static void retireReader(this ref pkgReader pr, ж<reader> Ꮡr) {
    ref var r = ref Ꮡr.val;

    pr.RetireDecoder(Ꮡ(r.Decoder));
}

// @@@ Positions
[GoRecv] internal static tokenꓸPos pos(this ref reader r) {
    r.Sync(pkgbits.SyncPos);
    if (!r.Bool()) {
        return token.NoPos;
    }
    // TODO(mdempsky): Delta encoding.
    @string posBase = r.posBase();
    nuint line = r.Uint();
    nuint col = r.Uint();
    return r.p.fake.pos(posBase, ((nint)line), ((nint)col));
}

[GoRecv] internal static @string posBase(this ref reader r) {
    return r.p.posBaseIdx(r.Reloc(pkgbits.RelocPosBase));
}

[GoRecv] internal static @string posBaseIdx(this ref pkgReader pr, pkgbits.Index idx) {
    {
        @string bΔ1 = pr.posBases[idx]; if (bΔ1 != ""u8) {
            return bΔ1;
        }
    }
    @string filename = default!;
    {
        var r = pr.tempReader(pkgbits.RelocPosBase, idx, pkgbits.SyncPosBase);
        // Within types2, position bases have a lot more details (e.g.,
        // keeping track of where //line directives appeared exactly).
        //
        // For go/types, we just track the file name.
        filename = r.String();
        if (r.Bool()){
        } else {
            // file base
            // Was: "b = token.NewTrimmedFileBase(filename, true)"
            // line base
            tokenꓸPos pos = r.pos();
            nuint line = r.Uint();
            nuint col = r.Uint();
            // Was: "b = token.NewLineBase(pos, filename, true, line, col)"
            var _ = pos;
            var _ = line;
            var _ = col;
        }
        pr.retireReader(r);
    }
    @string b = filename;
    pr.posBases[idx] = b;
    return b;
}

// @@@ Packages
[GoRecv] internal static ж<types.Package> pkg(this ref reader r) {
    r.Sync(pkgbits.SyncPkg);
    return r.p.pkgIdx(r.Reloc(pkgbits.RelocPkg));
}

[GoRecv] internal static ж<types.Package> pkgIdx(this ref pkgReader pr, pkgbits.Index idx) {
    // TODO(mdempsky): Consider using some non-nil pointer to indicate
    // the universe scope, so we don't need to keep re-reading it.
    {
        var pkgΔ1 = pr.pkgs[idx]; if (pkgΔ1 != nil) {
            return pkgΔ1;
        }
    }
    var pkg = pr.newReader(pkgbits.RelocPkg, idx, pkgbits.SyncPkgDef).doPkg();
    pr.pkgs[idx] = pkg;
    return pkg;
}

[GoRecv] internal static ж<types.Package> doPkg(this ref reader r) {
    @string path = r.String();
    var exprᴛ1 = path;
    if (exprᴛ1 == ""u8) {
        path = r.p.PkgPath();
    }
    else if (exprᴛ1 == "builtin"u8) {
        return default!;
    }
    if (exprᴛ1 == "unsafe"u8) {
        return types.Unsafe;
    }

    // universe
    {
        var pkgΔ1 = r.p.imports[path]; if (pkgΔ1 != nil) {
            return pkgΔ1;
        }
    }
    @string name = r.String();
    var pkg = types.NewPackage(path, name);
    r.p.imports[path] = pkg;
    return pkg;
}

// @@@ Types
[GoRecv] internal static typesꓸType typ(this ref reader r) {
    return r.p.typIdx(r.typInfo(), r.dict);
}

[GoRecv] internal static typeInfo typInfo(this ref reader r) {
    r.Sync(pkgbits.SyncType);
    if (r.Bool()) {
        return new typeInfo(idx: ((pkgbits.Index)r.Len()), derived: true);
    }
    return new typeInfo(idx: r.Reloc(pkgbits.RelocType), derived: false);
}

[GoRecv] internal static typesꓸType typIdx(this ref pkgReader pr, typeInfo info, ж<readerDict> Ꮡdict) {
    ref var dict = ref Ꮡdict.val;

    var idx = info.idx;
    ж<typesꓸType> where = default!;
    if (info.derived){
        where = Ꮡ(dict.derivedTypes, idx);
        idx = dict.derived[idx].idx;
    } else {
        where = Ꮡ(pr.typs[idx]);
    }
    {
        var typΔ1 = where.val; if (typΔ1 != default!) {
            return typΔ1;
        }
    }
    typesꓸType typ = default!;
    {
        var r = pr.tempReader(pkgbits.RelocType, idx, pkgbits.SyncTypeIdx);
        r.val.dict = dict;
        typ = r.doTyp();
        assert(typ != default!);
        pr.retireReader(r);
    }
    // See comment in pkgReader.typIdx explaining how this happens.
    {
        var prev = where.val; if (prev != default!) {
            return prev;
        }
    }
    where.val = typ;
    return typ;
}

[GoRecv] internal static typesꓸType /*res*/ doTyp(this ref reader r) {
    typesꓸType res = default!;

    {
        pkgbits.CodeType tag = ((pkgbits.CodeType)r.Code(pkgbits.SyncType));
        var exprᴛ1 = tag;
        { /* default: */
            errorf("unhandled type tag: %v"u8, tag);
            throw panic("unreachable");
        }
        else if (exprᴛ1 == pkgbits.TypeBasic) {
            return ~types.Typ[r.Len()];
        }
        if (exprᴛ1 == pkgbits.TypeNamed) {
            (obj, targs) = r.obj();
            var name = obj._<ж<types.TypeName>>();
            if (len(targs) != 0) {
                (t, _) = types.Instantiate(r.p.ctxt, name.Type(), targs, false);
                return t;
            }
            return name.Type();
        }
        if (exprᴛ1 == pkgbits.TypeTypeParam) {
            return ~r.dict.tparams[r.Len()];
        }
        if (exprᴛ1 == pkgbits.TypeArray) {
            var len = ((int64)r.Uint64());
            return ~types.NewArray(r.typ(), len);
        }
        if (exprᴛ1 == pkgbits.TypeChan) {
            types.ChanDir dir = ((types.ChanDir)r.Len());
            return ~types.NewChan(dir, r.typ());
        }
        if (exprᴛ1 == pkgbits.TypeMap) {
            return ~types.NewMap(r.typ(), r.typ());
        }
        if (exprᴛ1 == pkgbits.TypePointer) {
            return ~types.NewPointer(r.typ());
        }
        if (exprᴛ1 == pkgbits.TypeSignature) {
            return ~r.signature(nil, default!, default!);
        }
        if (exprᴛ1 == pkgbits.TypeSlice) {
            return ~types.NewSlice(r.typ());
        }
        if (exprᴛ1 == pkgbits.TypeStruct) {
            return ~r.structType();
        }
        if (exprᴛ1 == pkgbits.TypeInterface) {
            return ~r.interfaceType();
        }
        if (exprᴛ1 == pkgbits.TypeUnion) {
            return ~r.unionType();
        }
    }

}

[GoRecv] internal static ж<types.Struct> structType(this ref reader r) {
    var fields = new slice<types.Var>(r.Len());
    slice<@string> tags = default!;
    foreach (var (i, _) in fields) {
        tokenꓸPos pos = r.pos();
        var (pkg, name) = r.selector();
        var ftyp = r.typ();
        @string tag = r.String();
        var embedded = r.Bool();
        fields[i] = types.NewField(pos, pkg, name, ftyp, embedded);
        if (tag != ""u8) {
            while (len(tags) < i) {
                tags = append(tags, ""u8);
            }
            tags = append(tags, tag);
        }
    }
    return types.NewStruct(fields, tags);
}

[GoRecv] internal static ж<types.Union> unionType(this ref reader r) {
    var terms = new slice<typesꓸTerm>(r.Len());
    foreach (var (i, _) in terms) {
        terms[i] = types.NewTerm(r.Bool(), r.typ());
    }
    return types.NewUnion(terms);
}

[GoRecv] internal static ж<types.Interface> interfaceType(this ref reader r) {
    var methods = new slice<types.Func>(r.Len());
    var embeddeds = new slice<typesꓸType>(r.Len());
    var @implicit = len(methods) == 0 && len(embeddeds) == 1 && r.Bool();
    foreach (var (i, _) in methods) {
        tokenꓸPos pos = r.pos();
        var (pkg, name) = r.selector();
        var mtyp = r.signature(nil, default!, default!);
        methods[i] = types.NewFunc(pos, pkg, name, mtyp);
    }
    foreach (var (i, _) in embeddeds) {
        embeddeds[i] = r.typ();
    }
    var iface = types.NewInterfaceType(methods, embeddeds);
    if (@implicit) {
        iface.MarkImplicit();
    }
    // We need to call iface.Complete(), but if there are any embedded
    // defined types, then we may not have set their underlying
    // interface type yet. So we need to defer calling Complete until
    // after we've called SetUnderlying everywhere.
    //
    // TODO(mdempsky): After CL 424876 lands, it should be safe to call
    // iface.Complete() immediately.
    r.p.ifaces = append(r.p.ifaces, iface);
    return iface;
}

[GoRecv] internal static ж<typesꓸSignature> signature(this ref reader r, ж<types.Var> Ꮡrecv, slice<types.TypeParam> rtparams, slice<types.TypeParam> tparams) {
    ref var recv = ref Ꮡrecv.val;

    r.Sync(pkgbits.SyncSignature);
    var @params = r.@params();
    var results = r.@params();
    var variadic = r.Bool();
    return types.NewSignatureType(Ꮡrecv, rtparams, tparams, @params, results, variadic);
}

[GoRecv] internal static ж<types.Tuple> @params(this ref reader r) {
    r.Sync(pkgbits.SyncParams);
    var @params = new slice<types.Var>(r.Len());
    foreach (var (i, _) in @params) {
        @params[i] = r.param();
    }
    return types.NewTuple(Ꮡparams.ꓸꓸꓸ);
}

[GoRecv] internal static ж<types.Var> param(this ref reader r) {
    r.Sync(pkgbits.SyncParam);
    tokenꓸPos pos = r.pos();
    var (pkg, name) = r.localIdent();
    var typ = r.typ();
    return types.NewParam(pos, pkg, name, typ);
}

// @@@ Objects
[GoRecv] internal static (types.Object, slice<typesꓸType>) obj(this ref reader r) {
    r.Sync(pkgbits.SyncObject);
    assert(!r.Bool());
    var (pkg, name) = r.p.objIdx(r.Reloc(pkgbits.RelocObj));
    var obj = pkgScope(pkg).Lookup(name);
    var targs = new slice<typesꓸType>(r.Len());
    foreach (var (i, _) in targs) {
        targs[i] = r.typ();
    }
    return (obj, targs);
}

[GoRecv] internal static (ж<types.Package>, @string) objIdx(this ref pkgReader pr, pkgbits.Index idx) {
    ж<types.Package> objPkg = default!;
    @string objName = default!;
    pkgbits.CodeObj tag = default!;
    {
        var rname = pr.tempReader(pkgbits.RelocName, idx, pkgbits.SyncObject1);
        (objPkg, objName) = rname.qualifiedIdent();
        assert(objName != ""u8);
        tag = ((pkgbits.CodeObj)rname.Code(pkgbits.SyncCodeObj));
        pr.retireReader(rname);
    }
    if (tag == pkgbits.ObjStub) {
        assert(objPkg == nil || objPkg == types.Unsafe);
        return (objPkg, objName);
    }
    // Ignore local types promoted to global scope (#55110).
    {
        var (_, suffix) = splitVargenSuffix(objName); if (suffix != ""u8) {
            return (objPkg, objName);
        }
    }
    if (objPkg.Scope().Lookup(objName) == default!) {
        var dict = pr.objDictIdx(idx);
        var r = pr.newReader(pkgbits.RelocObj, idx, pkgbits.SyncObject1);
        r.val.dict = dict;
        var declare = 
        var objPkgʗ1 = objPkg;
        (types.Object obj) => {
            objPkgʗ1.Scope().Insert(obj);
        };
        var exprᴛ1 = tag;
        { /* default: */
            throw panic("weird");
        }
        else if (exprᴛ1 == pkgbits.ObjAlias) {
            tokenꓸPos pos = r.pos();
            var typ = r.typ();
            declare(~newAliasTypeName(pos, objPkg, objName, typ));
        }
        else if (exprᴛ1 == pkgbits.ObjConst) {
            tokenꓸPos pos = r.pos();
            var typ = r.typ();
            var val = r.Value();
            declare(~types.NewConst(pos, objPkg, objName, typ, val));
        }
        else if (exprᴛ1 == pkgbits.ObjFunc) {
            tokenꓸPos pos = r.pos();
            var tparams = r.typeParamNames();
            var sig = r.signature(nil, default!, tparams);
            declare(~types.NewFunc(pos, objPkg, objName, sig));
        }
        else if (exprᴛ1 == pkgbits.ObjType) {
            tokenꓸPos pos = r.pos();
            var obj = types.NewTypeName(pos, objPkg, objName, default!);
            var named = types.NewNamed(obj, default!, default!);
            declare(~obj);
            named.SetTypeParams(r.typeParamNames());
            var underlying = r.typ().Underlying();
            {
                var (iface, ok) = underlying._<ж<types.Interface>>(ᐧ); if (ok && iface.NumExplicitMethods() != 0) {
                    // If the underlying type is an interface, we need to
                    // duplicate its methods so we can replace the receiver
                    // parameter's type (#49906).
                    var methods = new slice<types.Func>(iface.NumExplicitMethods());
                    foreach (var (i, _) in methods) {
                        var fn = iface.ExplicitMethod(i);
                        var sig = fn.Type()._<ж<typesꓸSignature>>();
                        var recv = types.NewVar(fn.Pos(), fn.Pkg(), ""u8, ~named);
                        methods[i] = types.NewFunc(fn.Pos(), fn.Pkg(), fn.Name(), types.NewSignature(recv, sig.Params(), sig.Results(), sig.Variadic()));
                    }
                    var embeds = new slice<typesꓸType>(iface.NumEmbeddeds());
                    foreach (var (i, _) in embeds) {
                        embeds[i] = iface.EmbeddedType(i);
                    }
                    var newIface = types.NewInterfaceType(methods, embeds);
                    (~r).p.val.ifaces = append((~(~r).p).ifaces, newIface);
                    underlying = ~newIface;
                }
            }
            named.SetUnderlying(underlying);
            for (nint i = 0;nint n = r.Len(); i < n; i++) {
                named.AddMethod(r.method());
            }
        }
        else if (exprᴛ1 == pkgbits.ObjVar) {
            tokenꓸPos pos = r.pos();
            var typ = r.typ();
            declare(~types.NewVar(pos, objPkg, objName, typ));
        }

    }
    return (objPkg, objName);
}

[GoRecv] internal static ж<readerDict> objDictIdx(this ref pkgReader pr, pkgbits.Index idx) {
    ref var dict = ref heap(new readerDict(), out var Ꮡdict);
    {
        var r = pr.tempReader(pkgbits.RelocObjDict, idx, pkgbits.SyncObject1);
        {
            nint implicits = r.Len(); if (implicits != 0) {
                errorf("unexpected object with %v implicit type parameter(s)"u8, implicits);
            }
        }
        dict.bounds = new slice<typeInfo>(r.Len());
        foreach (var (i, _) in dict.bounds) {
            dict.bounds[i] = r.typInfo();
        }
        dict.derived = new slice<derivedInfo>(r.Len());
        dict.derivedTypes = new slice<typesꓸType>(len(dict.derived));
        foreach (var (i, _) in dict.derived) {
            dict.derived[i] = new derivedInfo(r.Reloc(pkgbits.RelocType), r.Bool());
        }
        pr.retireReader(r);
    }
    // function references follow, but reader doesn't need those
    return Ꮡdict;
}

[GoRecv] internal static slice<types.TypeParam> typeParamNames(this ref reader r) {
    r.Sync(pkgbits.SyncTypeParamNames);
    // Note: This code assumes it only processes objects without
    // implement type parameters. This is currently fine, because
    // reader is only used to read in exported declarations, which are
    // always package scoped.
    if (len(r.dict.bounds) == 0) {
        return default!;
    }
    // Careful: Type parameter lists may have cycles. To allow for this,
    // we construct the type parameter list in two passes: first we
    // create all the TypeNames and TypeParams, then we construct and
    // set the bound type.
    r.dict.tparams = new slice<types.TypeParam>(len(r.dict.bounds));
    foreach (var (i, _) in r.dict.bounds) {
        tokenꓸPos pos = r.pos();
        var (pkg, name) = r.localIdent();
        var tname = types.NewTypeName(pos, pkg, name, default!);
        r.dict.tparams[i] = types.NewTypeParam(tname, default!);
    }
    var typs = new slice<typesꓸType>(len(r.dict.bounds));
    foreach (var (i, bound) in r.dict.bounds) {
        typs[i] = r.p.typIdx(bound, r.dict);
    }
    // TODO(mdempsky): This is subtle, elaborate further.
    //
    // We have to save tparams outside of the closure, because
    // typeParamNames() can be called multiple times with the same
    // dictionary instance.
    //
    // Also, this needs to happen later to make sure SetUnderlying has
    // been called.
    //
    // TODO(mdempsky): Is it safe to have a single "later" slice or do
    // we need to have multiple passes? See comments on CL 386002 and
    // go.dev/issue/52104.
    var tparams = r.dict.tparams;
    r.p.later(
    var tparamsʗ2 = tparams;
    var typsʗ2 = typs;
    () => {
        foreach (var (i, typ) in typsʗ2) {
            tparamsʗ2[i].SetConstraint(typ);
        }
    });
    return r.dict.tparams;
}

[GoRecv] internal static ж<types.Func> method(this ref reader r) {
    r.Sync(pkgbits.SyncMethod);
    tokenꓸPos pos = r.pos();
    var (pkg, name) = r.selector();
    var rparams = r.typeParamNames();
    var sig = r.signature(r.param(), rparams, default!);
    _ = r.pos();
    // TODO(mdempsky): Remove; this is a hacker for linker.go.
    return types.NewFunc(pos, pkg, name, sig);
}

[GoRecv] internal static (ж<types.Package>, @string) qualifiedIdent(this ref reader r) {
    return r.ident(pkgbits.SyncSym);
}

[GoRecv] internal static (ж<types.Package>, @string) localIdent(this ref reader r) {
    return r.ident(pkgbits.SyncLocalIdent);
}

[GoRecv] internal static (ж<types.Package>, @string) selector(this ref reader r) {
    return r.ident(pkgbits.SyncSelector);
}

[GoRecv] internal static (ж<types.Package>, @string) ident(this ref reader r, pkgbits.SyncMarker marker) {
    r.Sync(marker);
    return (r.pkg(), r.String());
}

// pkgScope returns pkg.Scope().
// If pkg is nil, it returns types.Universe instead.
//
// TODO(mdempsky): Remove after x/tools can depend on Go 1.19.
internal static ж<typesꓸScope> pkgScope(ж<types.Package> Ꮡpkg) {
    ref var pkg = ref Ꮡpkg.val;

    if (pkg != nil) {
        return pkg.Scope();
    }
    return types.Universe;
}

// newAliasTypeName returns a new TypeName, with a materialized *types.Alias if supported.
internal static ж<types.TypeName> newAliasTypeName(tokenꓸPos pos, ж<types.Package> Ꮡpkg, @string name, typesꓸType rhs) {
    ref var pkg = ref Ꮡpkg.val;

    // When GODEBUG=gotypesalias=1 or unset, the Type() of the return value is a
    // *types.Alias. Copied from x/tools/internal/aliases.NewAlias.
    var exprᴛ1 = godebug.New("gotypesalias"u8).Value();
    if (exprᴛ1 == ""u8 || exprᴛ1 == "1"u8) {
        var tname = types.NewTypeName(pos, Ꮡpkg, name, default!);
        _ = types.NewAlias(tname, rhs);
        return tname;
    }

    // form TypeName -> Alias cycle
    return types.NewTypeName(pos, Ꮡpkg, name, rhs);
}

} // end gcimporter_package
