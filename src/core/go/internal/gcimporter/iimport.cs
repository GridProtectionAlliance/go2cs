// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Indexed package import.
// See cmd/compile/internal/gc/iexport.go for the export data format.
namespace go.go.@internal;

using bufio = bufio_package;
using bytes = bytes_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using constant = go.constant_package;
using token = go.token_package;
using types = go.types_package;
using saferio = @internal.saferio_package;
using io = io_package;
using math = math_package;
using big = math.big_package;
using slices = slices_package;
using strings = strings_package;
using @internal;
using encoding;
using go;
using math;

partial class gcimporter_package {

[GoType] partial struct intReader {
    public partial ref ж<bufio_package.Reader> Reader { get; }
    internal @string path;
}

[GoRecv] internal static int64 int64(this ref intReader r) {
    var (i, err) = binary.ReadVarint(~r.Reader);
    if (err != default!) {
        errorf("import %q: read varint error: %v"u8, r.path, err);
    }
    return i;
}

[GoRecv] internal static uint64 uint64(this ref intReader r) {
    var (i, err) = binary.ReadUvarint(~r.Reader);
    if (err != default!) {
        errorf("import %q: read varint error: %v"u8, r.path, err);
    }
    return i;
}

// Keep this in sync with constants in iexport.go.
internal static readonly UntypedInt iexportVersionGo1_11 = 0;

internal static readonly UntypedInt iexportVersionPosCol = 1;

internal static readonly UntypedInt iexportVersionGenerics = 2;

internal static readonly UntypedInt iexportVersionGo1_18 = 2;

internal static readonly UntypedInt iexportVersionCurrent = 2;

[GoType] partial struct Δident {
    internal ж<go.types_package.Package> pkg;
    internal @string name;
}

internal static readonly UntypedInt predeclReserved = 32;

[GoType("num:uint64")] partial struct itag;

internal static readonly itag definedType = /* iota */ 0;
internal static readonly itag pointerType = 1;
internal static readonly itag sliceType = 2;
internal static readonly itag arrayType = 3;
internal static readonly itag chanType = 4;
internal static readonly itag mapType = 5;
internal static readonly itag signatureType = 6;
internal static readonly itag ΔstructType = 7;
internal static readonly itag ΔinterfaceType = 8;
internal static readonly itag typeParamType = 9;
internal static readonly itag instanceType = 10;
internal static readonly itag ΔunionType = 11;

// iImportData imports a package from the serialized package data
// and returns the number of bytes consumed and a reference to the package.
// If the export data version is not recognized or the format is otherwise
// compromised, an error is returned.
internal static (ж<types.Package> pkg, error err) iImportData(ж<token.FileSet> Ꮡfset, types.Package imports, ж<bufio.Reader> ᏑdataReader, @string path) => func((defer, recover) => {
    ж<types.Package> pkg = default!;
    error err = default!;

    ref var fset = ref Ꮡfset.val;
    ref var dataReader = ref ᏑdataReader.val;
    static readonly UntypedInt currentVersion = /* iexportVersionCurrent */ 2;
    var version = ((int64)(-1));
    var errʗ1 = err;
    defer(() => {
        {
            var e = recover(); if (e != default!) {
                if (version > currentVersion){
                    errʗ1 = fmt.Errorf("cannot import %q (%v), export data is newer version - update tool"u8, path, e);
                } else {
                    errʗ1 = fmt.Errorf("cannot import %q (%v), possibly version skew - reinstall package"u8, path, e);
                }
            }
        }
    });
    var r = Ꮡ(new intReader(ᏑdataReader, path));
    version = ((int64)r.uint64());
    switch (version) {
    case iexportVersionGo1_18 or iexportVersionPosCol or iexportVersionGo1_11: {
        break;
    }
    default: {
        errorf("unknown iexport format version %d"u8, version);
        break;
    }}

    var sLen = r.uint64();
    var dLen = r.uint64();
    if (sLen > math.MaxUint64 - dLen) {
        errorf("lengths out of range (%d, %d)"u8, sLen, dLen);
    }
    (data, err) = saferio.ReadData(~r, sLen + dLen);
    if (err != default!) {
        errorf("cannot read %d bytes of stringData and declData: %s"u8, sLen + dLen, err);
    }
    var stringData = data[..(int)(sLen)];
    var declData = data[(int)(sLen)..];
    ref var p = ref heap<iimporter>(out var Ꮡp);
    p = new iimporter(
        exportVersion: version,
        ipath: path,
        version: ((nint)version),
        stringData: stringData,
        stringCache: new map<uint64, @string>(),
        pkgCache: new types.Package(),
        declData: declData,
        pkgIndex: new types.Package>map<@string>uint64(),
        typCache: new typesꓸType(), // Separate map for typeparams, keyed by their package and unique
 // name (name with subscript).

        tparamIndex: new types.TypeParam(),
        fake: new fakeFileSet(
            fset: fset,
            files: new map<@string, ж<fileInfo>>()
        )
    );
    var pʗ1 = p;
    defer(pʗ1.fake.setLines);
    // set lines for files in fset
    foreach (var (i, pt) in predeclared) {
        p.typCache[((uint64)i)] = pt;
    }
    // Special handling for "any", whose representation may be changed by the
    // gotypesalias GODEBUG variable.
    p.typCache[((uint64)len(predeclared))] = types.Universe.Lookup("any"u8).Type();
    var pkgList = new slice<types.Package>(r.uint64());
    foreach (var (i, _) in pkgList) {
        var pkgPathOff = r.uint64();
        @string pkgPath = p.stringAt(pkgPathOff);
        @string pkgName = p.stringAt(r.uint64());
        _ = r.uint64();
        // package height; unused by go/types
        if (pkgPath == ""u8) {
            pkgPath = path;
        }
        var pkgΔ1 = imports[pkgPath];
        if (pkgΔ1 == nil){
            pkgΔ1 = types.NewPackage(pkgPath, pkgName);
            imports[pkgPath] = pkgΔ1;
        } else 
        if (pkgΔ1.Name() != pkgName) {
            errorf("conflicting names %s and %s for package %q"u8, pkgΔ1.Name(), pkgName, path);
        }
        p.pkgCache[pkgPathOff] = pkgΔ1;
        var nameIndex = new map<@string, uint64>();
        for (var nSyms = r.uint64(); nSyms > 0; nSyms--) {
            @string name = p.stringAt(r.uint64());
            nameIndex[name] = r.uint64();
        }
        p.pkgIndex[pkg] = nameIndex;
        pkgList[i] = pkgΔ1;
    }
    var localpkg = pkgList[0];
    var names = new slice<@string>(0, len(p.pkgIndex[localpkg]));
    foreach (var (name, _) in p.pkgIndex[localpkg]) {
        names = append(names, name);
    }
    slices.Sort(names);
    foreach (var (_, name) in names) {
        p.doDecl(localpkg, name);
    }
    // SetConstraint can't be called if the constraint type is not yet complete.
    // When type params are created in the 'P' case of (*importReader).obj(),
    // the associated constraint type may not be complete due to recursion.
    // Therefore, we defer calling SetConstraint there, and call it here instead
    // after all types are complete.
    foreach (var (_, d) in p.later) {
        d.t.SetConstraint(d.constraint);
    }
    foreach (var (_, typ) in p.interfaceList) {
        typ.Complete();
    }
    // record all referenced packages as imports
    var list = append((slice<types.Package>)(default!), pkgList[1..].ꓸꓸꓸ);
    slices.SortFunc(list, 
    (ж<types.Package> a, ж<types.Package> b) => strings.Compare(a.Path(), b.Path()));
    localpkg.SetImports(list);
    // package was imported completely and without errors
    localpkg.MarkComplete();
    return (localpkg, default!);
});

[GoType] partial struct setConstraintArgs {
    internal ж<go.types_package.TypeParam> t;
    internal go.types_package.ΔType constraint;
}

[GoType] partial struct iimporter {
    internal int64 exportVersion;
    internal @string ipath;
    internal nint version;
    internal slice<byte> stringData;
    internal map<uint64, @string> stringCache;
    internal types.Package pkgCache;
    internal slice<byte> declData;
    internal types.Package>map<@string>uint64 pkgIndex;
    internal typesꓸType typCache;
    internal types.TypeParam tparamIndex;
    internal fakeFileSet fake;
    internal types.Interface interfaceList;
    // Arguments for calls to SetConstraint that are deferred due to recursive types
    internal slice<setConstraintArgs> later;
}

[GoRecv] internal static void doDecl(this ref iimporter p, ж<types.Package> Ꮡpkg, @string name) {
    ref var pkg = ref Ꮡpkg.val;

    // See if we've already imported this declaration.
    {
        var obj = pkg.Scope().Lookup(name); if (obj != default!) {
            return;
        }
    }
    var (off, ok) = p.pkgIndex[pkg][name];
    if (!ok) {
        errorf("%v.%v not in index"u8, pkg, name);
    }
    var r = Ꮡ(new importReader(p: p, currPkg: pkg));
    (~r).declReader.Reset(p.declData[(int)(off)..]);
    r.obj(name);
}

[GoRecv] internal static @string stringAt(this ref iimporter p, uint64 off) {
    {
        @string sΔ1 = p.stringCache[off];
        var ok = p.stringCache[off]; if (ok) {
            return sΔ1;
        }
    }
    var (slen, n) = binary.Uvarint(p.stringData[(int)(off)..]);
    if (n <= 0) {
        errorf("varint failed"u8);
    }
    var spos = off + ((uint64)n);
    @string s = ((@string)(p.stringData[(int)(spos)..(int)(spos + slen)]));
    p.stringCache[off] = s;
    return s;
}

[GoRecv] internal static ж<types.Package> pkgAt(this ref iimporter p, uint64 off) {
    {
        var pkg = p.pkgCache[off];
        var ok = p.pkgCache[off]; if (ok) {
            return pkg;
        }
    }
    @string path = p.stringAt(off);
    errorf("missing package %q in %q"u8, path, p.ipath);
    return default!;
}

[GoRecv] internal static typesꓸType typAt(this ref iimporter p, uint64 off, ж<types.Named> Ꮡbase) {
    ref var @base = ref Ꮡbase.val;

    {
        var tΔ1 = p.typCache[off];
        var ok = p.typCache[off]; if (ok && canReuse(Ꮡbase, tΔ1)) {
            return tΔ1;
        }
    }
    if (off < predeclReserved) {
        errorf("predeclared type missing from cache: %v"u8, off);
    }
    var r = Ꮡ(new importReader(p: p));
    (~r).declReader.Reset(p.declData[(int)(off - predeclReserved)..]);
    var t = r.doType(Ꮡbase);
    if (canReuse(Ꮡbase, t)) {
        p.typCache[off] = t;
    }
    return t;
}

// canReuse reports whether the type rhs on the RHS of the declaration for def
// may be re-used.
//
// Specifically, if def is non-nil and rhs is an interface type with methods, it
// may not be re-used because we have a convention of setting the receiver type
// for interface methods to def.
internal static bool canReuse(ж<types.Named> Ꮡdef, typesꓸType rhs) {
    ref var def = ref Ꮡdef.val;

    if (def == nil) {
        return true;
    }
    var (iface, _) = rhs._<ж<types.Interface>>(ᐧ);
    if (iface == nil) {
        return true;
    }
    // Don't use iface.Empty() here as iface may not be complete.
    return iface.NumEmbeddeds() == 0 && iface.NumExplicitMethods() == 0;
}

[GoType] partial struct importReader {
    internal ж<iimporter> p;
    internal bytes_package.Reader declReader;
    internal ж<go.types_package.Package> currPkg;
    internal @string prevFile;
    internal int64 prevLine;
    internal int64 prevColumn;
}

[GoRecv] internal static void obj(this ref importReader r, @string name) {
    var tag = r.@byte();
    tokenꓸPos pos = r.pos();
    switch (tag) {
    case (rune)'A': {
        var typ = r.typ();
        r.declare(~types.NewTypeName(pos, r.currPkg, name, typ));
        break;
    }
    case (rune)'C': {
        (typ, val) = r.value();
        r.declare(~types.NewConst(pos, r.currPkg, name, typ, val));
        break;
    }
    case (rune)'F' or (rune)'G': {
        slice<types.TypeParam> tparams = default!;
        if (tag == (rune)'G') {
            tparams = r.tparamList();
        }
        var sig = r.signature(nil, default!, tparams);
        r.declare(~types.NewFunc(pos, r.currPkg, name, sig));
        break;
    }
    case (rune)'T' or (rune)'U': {
        var obj = types.NewTypeName(pos, // Types can be recursive. We need to setup a stub
 // declaration before recurring.
 r.currPkg, name, default!);
        var named = types.NewNamed(obj, default!, default!);
        r.declare(~obj);
        if (tag == (rune)'U') {
            // Declare obj before calling r.tparamList, so the new type name is recognized
            // if used in the constraint of one of its own typeparams (see #48280).
            var tparams = r.tparamList();
            named.SetTypeParams(tparams);
        }
        var underlying = r.p.typAt(r.uint64(), named).Underlying();
        named.SetUnderlying(underlying);
        if (!isInterface(underlying)) {
            for (var n = r.uint64(); n > 0; n--) {
                tokenꓸPos mpos = r.pos();
                @string mname = r.ident();
                var recv = r.param();
                // If the receiver has any targs, set those as the
                // rparams of the method (since those are the
                // typeparams being used in the method sig/body).
                var targs = baseType(recv.Type()).TypeArgs();
                slice<types.TypeParam> rparams = default!;
                if (targs.Len() > 0) {
                    rparams = new slice<types.TypeParam>(targs.Len());
                    foreach (var (i, _) in rparams) {
                        (rparams[i], _) = targs.At(i)._<ж<types.TypeParam>>(ᐧ);
                    }
                }
                var msig = r.signature(recv, rparams, default!);
                named.AddMethod(types.NewFunc(mpos, r.currPkg, mname, msig));
            }
        }
        break;
    }
    case (rune)'P': {
        if (r.p.exportVersion < iexportVersionGenerics) {
            // We need to "declare" a typeparam in order to have a name that
            // can be referenced recursively (if needed) in the type param's
            // bound.
            errorf("unexpected type param type"u8);
        }
        @string name0 = tparamName(name);
        var tn = types.NewTypeName(pos, // Remove the "path" from the type param name that makes it unique,
 // and revert any unique name used for blank typeparams.
 r.currPkg, name0, default!);
        var t = types.NewTypeParam(tn, default!);
        var id = new Δident( // To handle recursive references to the typeparam within its
 // bound, save the partial type in tparamIndex before reading the bounds.
r.currPkg, name);
        r.p.tparamIndex[id] = t;
        bool @implicit = default!;
        if (r.p.exportVersion >= iexportVersionGo1_18) {
            @implicit = r.@bool();
        }
        var constraint = r.typ();
        if (@implicit) {
            var (iface, _) = constraint._<ж<types.Interface>>(ᐧ);
            if (iface == nil) {
                errorf("non-interface constraint marked implicit"u8);
            }
            iface.MarkImplicit();
        }
        r.p.later = append(r.p.later, // The constraint type may not be complete, if we
 // are in the middle of a type recursion involving type
 // constraints. So, we defer SetConstraint until we have
 // completely set up all types in ImportData.
 new setConstraintArgs(t: t, constraint: constraint));
        break;
    }
    case (rune)'V': {
        var typ = r.typ();
        r.declare(~types.NewVar(pos, r.currPkg, name, typ));
        break;
    }
    default: {
        errorf("unexpected tag: %v"u8, tag);
        break;
    }}

}

[GoRecv] internal static void declare(this ref importReader r, types.Object obj) {
    obj.Pkg().Scope().Insert(obj);
}

[GoRecv] internal static (typesꓸType typ, constant.Value val) value(this ref importReader r) {
    typesꓸType typ = default!;
    constant.Value val = default!;

    typ = r.typ();
    if (r.p.exportVersion >= iexportVersionGo1_18) {
        // TODO: add support for using the kind
        _ = ((constantꓸKind)r.int64());
    }
    {
        var b = typ.Underlying()._<ж<types.Basic>>();
        var exprᴛ1 = (types.BasicInfo)(b.Info() & types.IsConstType);
        if (exprᴛ1 == types.IsBoolean) {
            val = constant.MakeBool(r.@bool());
        }
        else if (exprᴛ1 == types.IsString) {
            val = constant.MakeString(r.@string());
        }
        else if (exprᴛ1 == types.IsInteger) {
            ref var x = ref heap(new math.big_package.ΔInt(), out var Ꮡx);
            r.mpint(Ꮡx, b);
            val = constant.Make(Ꮡx);
        }
        else if (exprᴛ1 == types.IsFloat) {
            val = r.mpfloat(b);
        }
        else if (exprᴛ1 == types.IsComplex) {
            var re = r.mpfloat(b);
            var im = r.mpfloat(b);
            val = constant.BinaryOp(re, token.ADD, constant.MakeImag(im));
        }
        else { /* default: */
            errorf("unexpected type %v"u8, typ);
            throw panic("unreachable");
        }
    }

    // panics
    return (typ, val);
}

internal static (bool signed, nuint maxBytes) intSize(ж<types.Basic> Ꮡb) {
    bool signed = default!;
    nuint maxBytes = default!;

    ref var b = ref Ꮡb.val;
    if (((types.BasicInfo)(b.Info() & types.IsUntyped)) != 0) {
        return (true, 64);
    }
    var exprᴛ1 = b.Kind();
    if (exprᴛ1 == types.Float32 || exprᴛ1 == types.Complex64) {
        return (true, 3);
    }
    if (exprᴛ1 == types.Float64 || exprᴛ1 == types.Complex128) {
        return (true, 7);
    }

    signed = ((types.BasicInfo)(b.Info() & types.IsUnsigned)) == 0;
    var exprᴛ2 = b.Kind();
    if (exprᴛ2 == types.Int8 || exprᴛ2 == types.Uint8) {
        maxBytes = 1;
    }
    else if (exprᴛ2 == types.Int16 || exprᴛ2 == types.Uint16) {
        maxBytes = 2;
    }
    else if (exprᴛ2 == types.Int32 || exprᴛ2 == types.Uint32) {
        maxBytes = 4;
    }
    else { /* default: */
        maxBytes = 8;
    }

    return (signed, maxBytes);
}

[GoRecv] internal static void mpint(this ref importReader r, ж<bigꓸInt> Ꮡx, ж<types.Basic> Ꮡtyp) {
    ref var x = ref Ꮡx.val;
    ref var typ = ref Ꮡtyp.val;

    var (signed, maxBytes) = intSize(Ꮡtyp);
    nuint maxSmall = 256 - maxBytes;
    if (signed) {
        maxSmall = 256 - 2 * maxBytes;
    }
    if (maxBytes == 1) {
        maxSmall = 256;
    }
    var (n, _) = r.declReader.ReadByte();
    if (((nuint)n) < maxSmall) {
        var vΔ1 = ((int64)n);
        if (signed) {
             >>= (UntypedInt)(1);
            if ((byte)(n & 1) != 0) {
                 = ^vΔ1;
            }
        }
        x.SetInt64(vΔ1);
        return;
    }
    var v = -n;
    if (signed) {
        v = -((byte)(n & ~1)) >> (int)(1);
    }
    if (v < 1 || ((nuint)v) > maxBytes) {
        errorf("weird decoding: %v, %v => %v"u8, n, signed, v);
    }
    var b = new slice<byte>(v);
    io.ReadFull(r.declReader, b);
    x.SetBytes(b);
    if (signed && (byte)(n & 1) != 0) {
        x.Neg(Ꮡx);
    }
}

[GoRecv] internal static constant.Value mpfloat(this ref importReader r, ж<types.Basic> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.val;

    ref var mant = ref heap(new math.big_package.ΔInt(), out var Ꮡmant);
    r.mpint(Ꮡmant, Ꮡtyp);
    ref var f = ref heap(new math.big_package.Float(), out var Ꮡf);
    f.SetInt(Ꮡmant);
    if (f.Sign() != 0) {
        f.SetMantExp(Ꮡf, ((nint)r.int64()));
    }
    return constant.Make(Ꮡf);
}

[GoRecv] internal static @string ident(this ref importReader r) {
    return r.@string();
}

[GoRecv] internal static (ж<types.Package>, @string) qualifiedIdent(this ref importReader r) {
    @string name = r.@string();
    var pkg = r.pkg();
    return (pkg, name);
}

[GoRecv] internal static tokenꓸPos pos(this ref importReader r) {
    if (r.p.version >= 1){
        r.posv1();
    } else {
        r.posv0();
    }
    if (r.prevFile == ""u8 && r.prevLine == 0 && r.prevColumn == 0) {
        return token.NoPos;
    }
    return r.p.fake.pos(r.prevFile, ((nint)r.prevLine), ((nint)r.prevColumn));
}

[GoRecv] internal static void posv0(this ref importReader r) {
    var delta = r.int64();
    if (delta != deltaNewFile){
        r.prevLine += delta;
    } else 
    {
        var l = r.int64(); if (l == -1){
            r.prevLine += deltaNewFile;
        } else {
            r.prevFile = r.@string();
            r.prevLine = l;
        }
    }
}

[GoRecv] internal static void posv1(this ref importReader r) {
    var delta = r.int64();
    r.prevColumn += delta >> (int)(1);
    if ((int64)(delta & 1) != 0) {
        delta = r.int64();
        r.prevLine += delta >> (int)(1);
        if ((int64)(delta & 1) != 0) {
            r.prevFile = r.@string();
        }
    }
}

[GoRecv] internal static typesꓸType typ(this ref importReader r) {
    return r.p.typAt(r.uint64(), nil);
}

internal static bool isInterface(typesꓸType t) {
    var (_, ok) = t._<ж<types.Interface>>(ᐧ);
    return ok;
}

[GoRecv] internal static ж<types.Package> pkg(this ref importReader r) {
    return r.p.pkgAt(r.uint64());
}

[GoRecv] internal static @string @string(this ref importReader r) {
    return r.p.stringAt(r.uint64());
}

[GoRecv] internal static typesꓸType doType(this ref importReader r, ж<types.Named> Ꮡbase) {
    ref var @base = ref Ꮡbase.val;

    {
        var k = r.kind();
        var exprᴛ1 = k;
        { /* default: */
            errorf("unexpected kind tag in %q: %v"u8, r.p.ipath, k);
            return default!;
        }
        if (exprᴛ1 == definedType) {
            var (pkg, name) = r.qualifiedIdent();
            r.p.doDecl(pkg, name);
            return pkg.Scope().Lookup(name)._<ж<types.TypeName>>().Type();
        }
        if (exprᴛ1 == pointerType) {
            return ~types.NewPointer(r.typ());
        }
        if (exprᴛ1 == sliceType) {
            return ~types.NewSlice(r.typ());
        }
        if (exprᴛ1 == arrayType) {
            var n = r.uint64();
            return ~types.NewArray(r.typ(), ((int64)n));
        }
        if (exprᴛ1 == chanType) {
            types.ChanDir dir = chanDir(((nint)r.uint64()));
            return ~types.NewChan(dir, r.typ());
        }
        if (exprᴛ1 == mapType) {
            return ~types.NewMap(r.typ(), r.typ());
        }
        if (exprᴛ1 == signatureType) {
            r.currPkg = r.pkg();
            return ~r.signature(nil, default!, default!);
        }
        if (exprᴛ1 == ΔstructType) {
            r.currPkg = r.pkg();
            var fields = new slice<types.Var>(r.uint64());
            var tags = new slice<@string>(len(fields));
            foreach (var (i, _) in fields) {
                tokenꓸPos fpos = r.pos();
                @string fname = r.ident();
                var ftyp = r.typ();
                var emb = r.@bool();
                @string tag = r.@string();
                fields[i] = types.NewField(fpos, r.currPkg, fname, ftyp, emb);
                tags[i] = tag;
            }
            return ~types.NewStruct(fields, tags);
        }
        if (exprᴛ1 == ΔinterfaceType) {
            r.currPkg = r.pkg();
            var embeddeds = new slice<typesꓸType>(r.uint64());
            foreach (var (i, _) in embeddeds) {
                _ = r.pos();
                embeddeds[i] = r.typ();
            }
            var methods = new slice<types.Func>(r.uint64());
            foreach (var (i, _) in methods) {
                tokenꓸPos mpos = r.pos();
                @string mname = r.ident();
                // TODO(mdempsky): Matches bimport.go, but I
                // don't agree with this.
                ж<types.Var> recv = default!;
                if (@base != nil) {
                    recv = types.NewVar(token.NoPos, r.currPkg, ""u8, ~@base);
                }
                var msig = r.signature(recv, default!, default!);
                methods[i] = types.NewFunc(mpos, r.currPkg, mname, msig);
            }
            var typ = types.NewInterfaceType(methods, embeddeds);
            r.p.interfaceList = append(r.p.interfaceList, typ);
            return ~typ;
        }
        if (exprᴛ1 == typeParamType) {
            if (r.p.exportVersion < iexportVersionGenerics) {
                errorf("unexpected type param type"u8);
            }
            var (pkg, name) = r.qualifiedIdent();
            var id = new Δident(pkg, name);
            {
                var t = r.p.tparamIndex[id];
                var ok = r.p.tparamIndex[id]; if (ok) {
                    // We're already in the process of importing this typeparam.
                    return ~t;
                }
            }
            r.p.doDecl(pkg, // Otherwise, import the definition of the typeparam now.
 name);
            return ~r.p.tparamIndex[id];
        }
        if (exprᴛ1 == instanceType) {
            if (r.p.exportVersion < iexportVersionGenerics) {
                errorf("unexpected instantiation type"u8);
            }
            _ = r.pos();
            var len = r.uint64();
            var targs = new slice<typesꓸType>(len);
            foreach (var (i, _) in targs) {
                // pos does not matter for instances: they are positioned on the original
                // type.
                targs[i] = r.typ();
            }
            var baseType = r.typ();
            (t, _) = types.Instantiate(nil, // The imported instantiated type doesn't include any methods, so
 // we must always use the methods of the base (orig) type.
 // TODO provide a non-nil *Context
 baseType, targs, false);
            return t;
        }
        if (exprᴛ1 == ΔunionType) {
            if (r.p.exportVersion < iexportVersionGenerics) {
                errorf("unexpected instantiation type"u8);
            }
            var terms = new slice<typesꓸTerm>(r.uint64());
            foreach (var (i, _) in terms) {
                terms[i] = types.NewTerm(r.@bool(), r.typ());
            }
            return ~types.NewUnion(terms);
        }
    }

}

[GoRecv] internal static itag kind(this ref importReader r) {
    return ((itag)r.uint64());
}

[GoRecv] internal static ж<typesꓸSignature> signature(this ref importReader r, ж<types.Var> Ꮡrecv, slice<types.TypeParam> rparams, slice<types.TypeParam> tparams) {
    ref var recv = ref Ꮡrecv.val;

    var @params = r.paramList();
    var results = r.paramList();
    var variadic = @params.Len() > 0 && r.@bool();
    return types.NewSignatureType(Ꮡrecv, rparams, tparams, @params, results, variadic);
}

[GoRecv] internal static slice<types.TypeParam> tparamList(this ref importReader r) {
    var n = r.uint64();
    if (n == 0) {
        return default!;
    }
    var xs = new slice<types.TypeParam>(n);
    foreach (var (i, _) in xs) {
        (xs[i], _) = r.typ()._<ж<types.TypeParam>>(ᐧ);
    }
    return xs;
}

[GoRecv] internal static ж<types.Tuple> paramList(this ref importReader r) {
    var xs = new slice<types.Var>(r.uint64());
    foreach (var (i, _) in xs) {
        xs[i] = r.param();
    }
    return types.NewTuple(Ꮡxs.ꓸꓸꓸ);
}

[GoRecv] internal static ж<types.Var> param(this ref importReader r) {
    tokenꓸPos pos = r.pos();
    @string name = r.ident();
    var typ = r.typ();
    return types.NewParam(pos, r.currPkg, name, typ);
}

[GoRecv] internal static bool @bool(this ref importReader r) {
    return r.uint64() != 0;
}

[GoRecv] internal static int64 int64(this ref importReader r) {
    var (n, err) = binary.ReadVarint(r.declReader);
    if (err != default!) {
        errorf("readVarint: %v"u8, err);
    }
    return n;
}

[GoRecv] internal static uint64 uint64(this ref importReader r) {
    var (n, err) = binary.ReadUvarint(r.declReader);
    if (err != default!) {
        errorf("readUvarint: %v"u8, err);
    }
    return n;
}

[GoRecv] internal static byte @byte(this ref importReader r) {
    var (x, err) = r.declReader.ReadByte();
    if (err != default!) {
        errorf("declReader.ReadByte: %v"u8, err);
    }
    return x;
}

internal static ж<types.Named> baseType(typesꓸType typ) {
    // pointer receivers are never types.Named types
    {
        var (p, _) = typ._<ж<types.Pointer>>(ᐧ); if (p != nil) {
            typ = p.Elem();
        }
    }
    // receiver base types are always (possibly generic) types.Named types
    var (n, _) = typ._<ж<types.Named>>(ᐧ);
    return n;
}

internal static readonly @string blankMarker = "$"u8;

// tparamName returns the real name of a type parameter, after stripping its
// qualifying prefix and reverting blank-name encoding. See tparamExportName
// for details.
internal static @string tparamName(@string exportName) {
    // Remove the "path" from the type param name that makes it unique.
    nint ix = strings.LastIndex(exportName, "."u8);
    if (ix < 0) {
        errorf("malformed type parameter export name %s: missing prefix"u8, exportName);
    }
    @string name = exportName[(int)(ix + 1)..];
    if (strings.HasPrefix(name, blankMarker)) {
        return "_"u8;
    }
    return name;
}

} // end gcimporter_package
