// UNREVIEWED
// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Indexed package import.
// See cmd/compile/internal/typecheck/iexport.go for the export data format.

// package importer -- go2cs converted at 2022 March 13 06:27:21 UTC
// import "cmd/compile/internal/importer" ==> using importer = go.cmd.compile.@internal.importer_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\importer\iimport.go
namespace go.cmd.compile.@internal;

using bytes = bytes_package;
using syntax = cmd.compile.@internal.syntax_package;
using types2 = cmd.compile.@internal.types2_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using constant = go.constant_package;
using token = go.token_package;
using io = io_package;
using big = math.big_package;
using sort = sort_package;
using System;

public static partial class importer_package {

private partial struct intReader {
    public ref ptr<bytes.Reader> Reader> => ref Reader>_ptr;
    public @string path;
}

private static long int64(this ptr<intReader> _addr_r) {
    ref intReader r = ref _addr_r.val;

    var (i, err) = binary.ReadVarint(r.Reader);
    if (err != null) {
        errorf("import %q: read varint error: %v", r.path, err);
    }
    return i;
}

private static ulong uint64(this ptr<intReader> _addr_r) {
    ref intReader r = ref _addr_r.val;

    var (i, err) = binary.ReadUvarint(r.Reader);
    if (err != null) {
        errorf("import %q: read varint error: %v", r.path, err);
    }
    return i;
}

private static readonly nint predeclReserved = 32;



private partial struct itag { // : ulong
}

 
// Types
private static readonly itag definedType = iota;
private static readonly var pointerType = 0;
private static readonly var sliceType = 1;
private static readonly var arrayType = 2;
private static readonly var chanType = 3;
private static readonly var mapType = 4;
private static readonly var signatureType = 5;
private static readonly var structType = 6;
private static readonly var interfaceType = 7;

private static readonly nint io_SeekCurrent = 1; // io.SeekCurrent (not defined in Go 1.4)

// iImportData imports a package from the serialized package data
// and returns the number of bytes consumed and a reference to the package.
// If the export data version is not recognized or the format is otherwise
// compromised, an error is returned.
 // io.SeekCurrent (not defined in Go 1.4)

// iImportData imports a package from the serialized package data
// and returns the number of bytes consumed and a reference to the package.
// If the export data version is not recognized or the format is otherwise
// compromised, an error is returned.
private static (nint, ptr<types2.Package>, error) iImportData(map<@string, ptr<types2.Package>> imports, slice<byte> data, @string path) => func((defer, _, _) => {
    nint _ = default;
    ptr<types2.Package> pkg = default!;
    error err = default!;

    const nint currentVersion = 1;

    var version = int64(-1);
    defer(() => {
        {
            var e = recover();

            if (e != null) {
                if (version > currentVersion) {
                    err = fmt.Errorf("cannot import %q (%v), export data is newer version - update tool", path, e);
                }
                else
 {
                    err = fmt.Errorf("cannot import %q (%v), possibly version skew - reinstall package", path, e);
                }
            }

        }
    }());

    ptr<intReader> r = addr(new intReader(bytes.NewReader(data),path));

    version = int64(r.uint64());

    if (version == currentVersion || version == 0)     else 
        errorf("unknown iexport format version %d", version);
        var sLen = int64(r.uint64());
    var dLen = int64(r.uint64());

    var (whence, _) = r.Seek(0, io_SeekCurrent);
    var stringData = data[(int)whence..(int)whence + sLen];
    var declData = data[(int)whence + sLen..(int)whence + sLen + dLen];
    r.Seek(sLen + dLen, io_SeekCurrent);

    iimporter p = new iimporter(ipath:path,version:int(version),stringData:stringData,stringCache:make(map[uint64]string),pkgCache:make(map[uint64]*types2.Package),declData:declData,pkgIndex:make(map[*types2.Package]map[string]uint64),typCache:make(map[uint64]types2.Type),);

    {
        var i__prev1 = i;

        foreach (var (__i, __pt) in predeclared) {
            i = __i;
            pt = __pt;
            p.typCache[uint64(i)] = pt;
        }
        i = i__prev1;
    }

    var pkgList = make_slice<ptr<types2.Package>>(r.uint64());
    {
        var i__prev1 = i;

        foreach (var (__i) in pkgList) {
            i = __i;
            var pkgPathOff = r.uint64();
            var pkgPath = p.stringAt(pkgPathOff);
            var pkgName = p.stringAt(r.uint64());
            _ = r.uint64(); // package height; unused by go/types

            if (pkgPath == "") {
                pkgPath = path;
            }
            var pkg = imports[pkgPath];
            if (pkg == null) {
                pkg = types2.NewPackage(pkgPath, pkgName);
                imports[pkgPath] = pkg;
            }
            else if (pkg.Name() != pkgName) {
                errorf("conflicting names %s and %s for package %q", pkg.Name(), pkgName, path);
            }
            p.pkgCache[pkgPathOff] = pkg;

            var nameIndex = make_map<@string, ulong>();
            for (var nSyms = r.uint64(); nSyms > 0; nSyms--) {
                var name = p.stringAt(r.uint64());
                nameIndex[name] = r.uint64();
            }


            p.pkgIndex[pkg] = nameIndex;
            pkgList[i] = pkg;
        }
        i = i__prev1;
    }

    var localpkg = pkgList[0];

    var names = make_slice<@string>(0, len(p.pkgIndex[localpkg]));
    {
        var name__prev1 = name;

        foreach (var (__name) in p.pkgIndex[localpkg]) {
            name = __name;
            names = append(names, name);
        }
        name = name__prev1;
    }

    sort.Strings(names);
    {
        var name__prev1 = name;

        foreach (var (_, __name) in names) {
            name = __name;
            p.doDecl(localpkg, name);
        }
        name = name__prev1;
    }

    foreach (var (_, typ) in p.interfaceList) {
        typ.Complete();
    }    var list = append((slice<ptr<types2.Package>>)null, pkgList[(int)1..]);
    sort.Sort(byPath(list));
    localpkg.SetImports(list); 

    // package was imported completely and without errors
    localpkg.MarkComplete();

    var (consumed, _) = r.Seek(0, io_SeekCurrent);
    return (int(consumed), _addr_localpkg!, error.As(null!)!);
});

private partial struct iimporter {
    public @string ipath;
    public nint version;
    public slice<byte> stringData;
    public map<ulong, @string> stringCache;
    public map<ulong, ptr<types2.Package>> pkgCache;
    public slice<byte> declData;
    public map<ptr<types2.Package>, map<@string, ulong>> pkgIndex;
    public map<ulong, types2.Type> typCache;
    public slice<ptr<types2.Interface>> interfaceList;
}

private static void doDecl(this ptr<iimporter> _addr_p, ptr<types2.Package> _addr_pkg, @string name) {
    ref iimporter p = ref _addr_p.val;
    ref types2.Package pkg = ref _addr_pkg.val;
 
    // See if we've already imported this declaration.
    {
        var obj = pkg.Scope().Lookup(name);

        if (obj != null) {
            return ;
        }
    }

    var (off, ok) = p.pkgIndex[pkg][name];
    if (!ok) {
        errorf("%v.%v not in index", pkg, name);
    }
    ptr<importReader> r = addr(new importReader(p:p,currPkg:pkg)); 
    // Reader.Reset is not available in Go 1.4.
    // Use bytes.NewReader for now.
    // r.declReader.Reset(p.declData[off:])
    r.declReader = new ptr<ptr<bytes.NewReader>>(p.declData[(int)off..]);

    r.obj(name);
}

private static @string stringAt(this ptr<iimporter> _addr_p, ulong off) {
    ref iimporter p = ref _addr_p.val;

    {
        var s__prev1 = s;

        var (s, ok) = p.stringCache[off];

        if (ok) {
            return s;
        }
        s = s__prev1;

    }

    var (slen, n) = binary.Uvarint(p.stringData[(int)off..]);
    if (n <= 0) {
        errorf("varint failed");
    }
    var spos = off + uint64(n);
    var s = string(p.stringData[(int)spos..(int)spos + slen]);
    p.stringCache[off] = s;
    return s;
}

private static ptr<types2.Package> pkgAt(this ptr<iimporter> _addr_p, ulong off) {
    ref iimporter p = ref _addr_p.val;

    {
        var (pkg, ok) = p.pkgCache[off];

        if (ok) {
            return _addr_pkg!;
        }
    }
    var path = p.stringAt(off);
    errorf("missing package %q in %q", path, p.ipath);
    return _addr_null!;
}

private static types2.Type typAt(this ptr<iimporter> _addr_p, ulong off, ptr<types2.Named> _addr_@base) {
    ref iimporter p = ref _addr_p.val;
    ref types2.Named @base = ref _addr_@base.val;

    {
        var t__prev1 = t;

        var (t, ok) = p.typCache[off];

        if (ok && (base == null || !isInterface(t))) {
            return t;
        }
        t = t__prev1;

    }

    if (off < predeclReserved) {
        errorf("predeclared type missing from cache: %v", off);
    }
    ptr<importReader> r = addr(new importReader(p:p)); 
    // Reader.Reset is not available in Go 1.4.
    // Use bytes.NewReader for now.
    // r.declReader.Reset(p.declData[off-predeclReserved:])
    r.declReader = new ptr<ptr<bytes.NewReader>>(p.declData[(int)off - predeclReserved..]);
    var t = r.doType(base);

    if (base == null || !isInterface(t)) {
        p.typCache[off] = t;
    }
    return t;
}

private partial struct importReader {
    public ptr<iimporter> p;
    public bytes.Reader declReader;
    public ptr<types2.Package> currPkg;
    public @string prevFile;
    public long prevLine;
    public long prevColumn;
}

private static void obj(this ptr<importReader> _addr_r, @string name) {
    ref importReader r = ref _addr_r.val;

    var tag = r.@byte();
    var pos = r.pos();

    switch (tag) {
        case 'A': 
            var typ = r.typ();

            r.declare(types2.NewTypeName(pos, r.currPkg, name, typ));
            break;
        case 'C': 
            var (typ, val) = r.value();

            r.declare(types2.NewConst(pos, r.currPkg, name, typ, val));
            break;
        case 'F': 
            var sig = r.signature(null);

            r.declare(types2.NewFunc(pos, r.currPkg, name, sig));
            break;
        case 'T': 
            // Types can be recursive. We need to setup a stub
            // declaration before recursing.
            var obj = types2.NewTypeName(pos, r.currPkg, name, null);
            var named = types2.NewNamed(obj, null, null);
            r.declare(obj);

            var underlying = r.p.typAt(r.uint64(), named).Underlying();
            named.SetUnderlying(underlying);

            if (!isInterface(underlying)) {
                for (var n = r.uint64(); n > 0; n--) {
                    var mpos = r.pos();
                    var mname = r.ident();
                    var recv = r.param();
                    var msig = r.signature(recv);

                    named.AddMethod(types2.NewFunc(mpos, r.currPkg, mname, msig));
                }
            }
            break;
        case 'V': 
            typ = r.typ();

            r.declare(types2.NewVar(pos, r.currPkg, name, typ));
            break;
        default: 
            errorf("unexpected tag: %v", tag);
            break;
    }
}

private static void declare(this ptr<importReader> _addr_r, types2.Object obj) {
    ref importReader r = ref _addr_r.val;

    obj.Pkg().Scope().Insert(obj);
}

private static (types2.Type, constant.Value) value(this ptr<importReader> _addr_r) => func((_, panic, _) => {
    types2.Type typ = default;
    constant.Value val = default;
    ref importReader r = ref _addr_r.val;

    typ = r.typ();

    {
        ptr<types2.Basic> b = typ.Underlying()._<ptr<types2.Basic>>();


        if (b.Info() & types2.IsConstType == types2.IsBoolean) 
            val = constant.MakeBool(r.@bool());
        else if (b.Info() & types2.IsConstType == types2.IsString) 
            val = constant.MakeString(r.@string());
        else if (b.Info() & types2.IsConstType == types2.IsInteger) 
            ref big.Int x = ref heap(out ptr<big.Int> _addr_x);
            r.mpint(_addr_x, b);
            val = constant.Make(_addr_x);
        else if (b.Info() & types2.IsConstType == types2.IsFloat) 
            val = r.mpfloat(b);
        else if (b.Info() & types2.IsConstType == types2.IsComplex) 
            var re = r.mpfloat(b);
            var im = r.mpfloat(b);
            val = constant.BinaryOp(re, token.ADD, constant.MakeImag(im));
        else 
            errorf("unexpected type %v", typ); // panics
            panic("unreachable");

    }

    return ;
});

private static (bool, nuint) intSize(ptr<types2.Basic> _addr_b) {
    bool signed = default;
    nuint maxBytes = default;
    ref types2.Basic b = ref _addr_b.val;

    if ((b.Info() & types2.IsUntyped) != 0) {
        return (true, 64);
    }

    if (b.Kind() == types2.Float32 || b.Kind() == types2.Complex64) 
        return (true, 3);
    else if (b.Kind() == types2.Float64 || b.Kind() == types2.Complex128) 
        return (true, 7);
        signed = (b.Info() & types2.IsUnsigned) == 0;

    if (b.Kind() == types2.Int8 || b.Kind() == types2.Uint8) 
        maxBytes = 1;
    else if (b.Kind() == types2.Int16 || b.Kind() == types2.Uint16) 
        maxBytes = 2;
    else if (b.Kind() == types2.Int32 || b.Kind() == types2.Uint32) 
        maxBytes = 4;
    else 
        maxBytes = 8;
        return ;
}

private static void mpint(this ptr<importReader> _addr_r, ptr<big.Int> _addr_x, ptr<types2.Basic> _addr_typ) {
    ref importReader r = ref _addr_r.val;
    ref big.Int x = ref _addr_x.val;
    ref types2.Basic typ = ref _addr_typ.val;

    var (signed, maxBytes) = intSize(_addr_typ);

    nint maxSmall = 256 - maxBytes;
    if (signed) {
        maxSmall = 256 - 2 * maxBytes;
    }
    if (maxBytes == 1) {
        maxSmall = 256;
    }
    var (n, _) = r.declReader.ReadByte();
    if (uint(n) < maxSmall) {
        var v = int64(n);
        if (signed) {
            v>>=1;
            if (n & 1 != 0) {
                v = ~v;
            }
        }
        x.SetInt64(v);
        return ;
    }
    v = -n;
    if (signed) {
        v = -(n & ~1) >> 1;
    }
    if (v < 1 || uint(v) > maxBytes) {
        errorf("weird decoding: %v, %v => %v", n, signed, v);
    }
    var b = make_slice<byte>(v);
    io.ReadFull(_addr_r.declReader, b);
    x.SetBytes(b);
    if (signed && n & 1 != 0) {
        x.Neg(x);
    }
}

private static constant.Value mpfloat(this ptr<importReader> _addr_r, ptr<types2.Basic> _addr_typ) {
    ref importReader r = ref _addr_r.val;
    ref types2.Basic typ = ref _addr_typ.val;

    ref big.Int mant = ref heap(out ptr<big.Int> _addr_mant);
    r.mpint(_addr_mant, typ);
    ref big.Float f = ref heap(out ptr<big.Float> _addr_f);
    f.SetInt(_addr_mant);
    if (f.Sign() != 0) {
        f.SetMantExp(_addr_f, int(r.int64()));
    }
    return constant.Make(_addr_f);
}

private static @string ident(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    return r.@string();
}

private static (ptr<types2.Package>, @string) qualifiedIdent(this ptr<importReader> _addr_r) {
    ptr<types2.Package> _p0 = default!;
    @string _p0 = default;
    ref importReader r = ref _addr_r.val;

    var name = r.@string();
    var pkg = r.pkg();
    return (_addr_pkg!, name);
}

private static syntax.Pos pos(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    if (r.p.version >= 1) {
        r.posv1();
    }
    else
 {
        r.posv0();
    }
    if (r.prevFile == "" && r.prevLine == 0 && r.prevColumn == 0) {
        return new syntax.Pos();
    }
    return new syntax.Pos();
}

private static void posv0(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var delta = r.int64();
    if (delta != deltaNewFile) {
        r.prevLine += delta;
    }    {
        var l = r.int64();


        else if (l == -1) {
            r.prevLine += deltaNewFile;
        }
        else
 {
            r.prevFile = r.@string();
            r.prevLine = l;
        }
    }
}

private static void posv1(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var delta = r.int64();
    r.prevColumn += delta >> 1;
    if (delta & 1 != 0) {
        delta = r.int64();
        r.prevLine += delta >> 1;
        if (delta & 1 != 0) {
            r.prevFile = r.@string();
        }
    }
}

private static types2.Type typ(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    return r.p.typAt(r.uint64(), null);
}

private static bool isInterface(types2.Type t) {
    ptr<types2.Interface> (_, ok) = t._<ptr<types2.Interface>>();
    return ok;
}

private static ptr<types2.Package> pkg(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    return _addr_r.p.pkgAt(r.uint64())!;
}
private static @string @string(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    return r.p.stringAt(r.uint64());
}

private static types2.Type doType(this ptr<importReader> _addr_r, ptr<types2.Named> _addr_@base) {
    ref importReader r = ref _addr_r.val;
    ref types2.Named @base = ref _addr_@base.val;

    {
        var k = r.kind();


        if (k == definedType) 
            var (pkg, name) = r.qualifiedIdent();
            r.p.doDecl(pkg, name);
            return pkg.Scope().Lookup(name)._<ptr<types2.TypeName>>().Type();
        else if (k == pointerType) 
            return types2.NewPointer(r.typ());
        else if (k == sliceType) 
            return types2.NewSlice(r.typ());
        else if (k == arrayType) 
            var n = r.uint64();
            return types2.NewArray(r.typ(), int64(n));
        else if (k == chanType) 
            var dir = chanDir(int(r.uint64()));
            return types2.NewChan(dir, r.typ());
        else if (k == mapType) 
            return types2.NewMap(r.typ(), r.typ());
        else if (k == signatureType) 
            r.currPkg = r.pkg();
            return r.signature(null);
        else if (k == structType) 
            r.currPkg = r.pkg();

            var fields = make_slice<ptr<types2.Var>>(r.uint64());
            var tags = make_slice<@string>(len(fields));
            {
                var i__prev1 = i;

                foreach (var (__i) in fields) {
                    i = __i;
                    var fpos = r.pos();
                    var fname = r.ident();
                    var ftyp = r.typ();
                    var emb = r.@bool();
                    var tag = r.@string();

                    fields[i] = types2.NewField(fpos, r.currPkg, fname, ftyp, emb);
                    tags[i] = tag;
                }

                i = i__prev1;
            }

            return types2.NewStruct(fields, tags);
        else if (k == interfaceType) 
            r.currPkg = r.pkg();

            var embeddeds = make_slice<types2.Type>(r.uint64());
            {
                var i__prev1 = i;

                foreach (var (__i) in embeddeds) {
                    i = __i;
                    _ = r.pos();
                    embeddeds[i] = r.typ();
                }

                i = i__prev1;
            }

            var methods = make_slice<ptr<types2.Func>>(r.uint64());
            {
                var i__prev1 = i;

                foreach (var (__i) in methods) {
                    i = __i;
                    var mpos = r.pos();
                    var mname = r.ident(); 

                    // TODO(mdempsky): Matches bimport.go, but I
                    // don't agree with this.
                    ptr<types2.Var> recv;
                    if (base != null) {
                        recv = types2.NewVar(new syntax.Pos(), r.currPkg, "", base);
                    }
                    var msig = r.signature(recv);
                    methods[i] = types2.NewFunc(mpos, r.currPkg, mname, msig);
                }

                i = i__prev1;
            }

            var typ = types2.NewInterfaceType(methods, embeddeds);
            r.p.interfaceList = append(r.p.interfaceList, typ);
            return typ;
        else 
            errorf("unexpected kind tag in %q: %v", r.p.ipath, k);
            return null;

    }
}

private static itag kind(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    return itag(r.uint64());
}

private static ptr<types2.Signature> signature(this ptr<importReader> _addr_r, ptr<types2.Var> _addr_recv) {
    ref importReader r = ref _addr_r.val;
    ref types2.Var recv = ref _addr_recv.val;

    var @params = r.paramList();
    var results = r.paramList();
    var variadic = @params.Len() > 0 && r.@bool();
    return _addr_types2.NewSignature(recv, params, results, variadic)!;
}

private static ptr<types2.Tuple> paramList(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var xs = make_slice<ptr<types2.Var>>(r.uint64());
    foreach (var (i) in xs) {
        xs[i] = r.param();
    }    return _addr_types2.NewTuple(xs)!;
}

private static ptr<types2.Var> param(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var pos = r.pos();
    var name = r.ident();
    var typ = r.typ();
    return _addr_types2.NewParam(pos, r.currPkg, name, typ)!;
}

private static bool @bool(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    return r.uint64() != 0;
}

private static long int64(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var (n, err) = binary.ReadVarint(_addr_r.declReader);
    if (err != null) {
        errorf("readVarint: %v", err);
    }
    return n;
}

private static ulong uint64(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var (n, err) = binary.ReadUvarint(_addr_r.declReader);
    if (err != null) {
        errorf("readUvarint: %v", err);
    }
    return n;
}

private static byte @byte(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var (x, err) = r.declReader.ReadByte();
    if (err != null) {
        errorf("declReader.ReadByte: %v", err);
    }
    return x;
}

} // end importer_package
