// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Indexed package import.
// See iexport.go for the export data format.

// package typecheck -- go2cs converted at 2022 March 06 22:48:38 UTC
// import "cmd/compile/internal/typecheck" ==> using typecheck = go.cmd.compile.@internal.typecheck_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\typecheck\iimport.go
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using constant = go.go.constant_package;
using io = go.io_package;
using big = go.math.big_package;
using os = go.os_package;
using strings = go.strings_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using types = go.cmd.compile.@internal.types_package;
using bio = go.cmd.@internal.bio_package;
using goobj = go.cmd.@internal.goobj_package;
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class typecheck_package {

    // An iimporterAndOffset identifies an importer and an offset within
    // its data section.
private partial struct iimporterAndOffset {
    public ptr<iimporter> p;
    public ulong off;
}

 
// DeclImporter maps from imported identifiers to an importer
// and offset where that identifier's declaration can be read.
public static map DeclImporter = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<types.Sym>, iimporterAndOffset>{};private static map inlineImporter = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<types.Sym>, iimporterAndOffset>{};

// expandDecl returns immediately if n is already a Name node. Otherwise, n should
// be an Ident node, and expandDecl reads in the definition of the specified
// identifier from the appropriate package.
private static ir.Node expandDecl(ir.Node n) {
    {
        ptr<ir.Name> n__prev1 = n;

        ptr<ir.Name> (n, ok) = n._<ptr<ir.Name>>();

        if (ok) {
            return n;
        }
        n = n__prev1;

    }


    ptr<ir.Ident> id = n._<ptr<ir.Ident>>();
    {
        ptr<ir.Name> n__prev1 = n;

        var n = id.Sym().PkgDef();

        if (n != null) {
            return n._<ptr<ir.Name>>();
        }
        n = n__prev1;

    }


    var r = importReaderFor(_addr_id.Sym(), DeclImporter);
    if (r == null) { 
        // Can happen if user tries to reference an undeclared name.
        return n;

    }
    return r.doDecl(n.Sym());

}

// ImportBody reads in the dcls and body of an imported function (which should not
// yet have been read in).
public static void ImportBody(ptr<ir.Func> _addr_fn) {
    ref ir.Func fn = ref _addr_fn.val;

    if (fn.Inl.Body != null) {
        @base.Fatalf("%v already has inline body", fn);
    }
    var r = importReaderFor(_addr_fn.Nname.Sym(), inlineImporter);
    if (r == null) {
        @base.Fatalf("missing import reader for %v", fn);
    }
    if (inimport) {
        @base.Fatalf("recursive inimport");
    }
    inimport = true;
    r.doInline(fn);
    inimport = false;

}

private static ptr<importReader> importReaderFor(ptr<types.Sym> _addr_sym, map<ptr<types.Sym>, iimporterAndOffset> importers) {
    ref types.Sym sym = ref _addr_sym.val;

    var (x, ok) = importers[sym];
    if (!ok) {
        return _addr_null!;
    }
    return _addr_x.p.newReader(x.off, sym.Pkg)!;

}

private partial struct intReader {
    public ref ptr<bio.Reader> Reader> => ref Reader>_ptr;
    public ptr<types.Pkg> pkg;
}

private static long int64(this ptr<intReader> _addr_r) {
    ref intReader r = ref _addr_r.val;

    var (i, err) = binary.ReadVarint(r.Reader);
    if (err != null) {
        @base.Errorf("import %q: read error: %v", r.pkg.Path, err);
        @base.ErrorExit();
    }
    return i;

}

private static ulong uint64(this ptr<intReader> _addr_r) {
    ref intReader r = ref _addr_r.val;

    var (i, err) = binary.ReadUvarint(r.Reader);
    if (err != null) {
        @base.Errorf("import %q: read error: %v", r.pkg.Path, err);
        @base.ErrorExit();
    }
    return i;

}

public static goobj.FingerprintType ReadImports(ptr<types.Pkg> _addr_pkg, ptr<bio.Reader> _addr_@in) {
    goobj.FingerprintType fingerprint = default;
    ref types.Pkg pkg = ref _addr_pkg.val;
    ref bio.Reader @in = ref _addr_@in.val;

    ptr<intReader> ird = addr(new intReader(in,pkg));

    var version = ird.uint64();
    if (version != iexportVersion) {
        @base.Errorf("import %q: unknown export format version %d", pkg.Path, version);
        @base.ErrorExit();
    }
    var sLen = ird.uint64();
    var dLen = ird.uint64(); 

    // Map string (and data) section into memory as a single large
    // string. This reduces heap fragmentation and allows
    // returning individual substrings very efficiently.
    var (data, err) = mapFile(@in.File(), @in.Offset(), int64(sLen + dLen));
    if (err != null) {
        @base.Errorf("import %q: mapping input: %v", pkg.Path, err);
        @base.ErrorExit();
    }
    var stringData = data[..(int)sLen];
    var declData = data[(int)sLen..];

    @in.MustSeek(int64(sLen + dLen), os.SEEK_CUR);

    ptr<iimporter> p = addr(new iimporter(ipkg:pkg,pkgCache:map[uint64]*types.Pkg{},posBaseCache:map[uint64]*src.PosBase{},typCache:map[uint64]*types.Type{},stringData:stringData,declData:declData,));

    foreach (var (i, pt) in predeclared()) {
        p.typCache[uint64(i)] = pt;
    }    {
        var nPkgs__prev1 = nPkgs;

        for (var nPkgs = ird.uint64(); nPkgs > 0; nPkgs--) {
            var pkg = p.pkgAt(ird.uint64());
            var pkgName = p.stringAt(ird.uint64());
            var pkgHeight = int(ird.uint64());
            if (pkg.Name == "") {
                pkg.Name = pkgName;
                pkg.Height = pkgHeight;
                types.NumImport[pkgName]++; 

                // TODO(mdempsky): This belongs somewhere else.
                pkg.Lookup("_").Def;

                ir.BlankNode;

            }
            else
 {
                if (pkg.Name != pkgName) {
                    @base.Fatalf("conflicting package names %v and %v for path %q", pkg.Name, pkgName, pkg.Path);
                }
                if (pkg.Height != pkgHeight) {
                    @base.Fatalf("conflicting package heights %v and %v for path %q", pkg.Height, pkgHeight, pkg.Path);
                }
            }

            {
                var nSyms__prev2 = nSyms;

                for (var nSyms = ird.uint64(); nSyms > 0; nSyms--) {
                    var s = pkg.Lookup(p.stringAt(ird.uint64()));
                    var off = ird.uint64();

                    {
                        var (_, ok) = DeclImporter[s];

                        if (!ok) {
                            DeclImporter[s] = new iimporterAndOffset(p,off);
                        }

                    }

                }


                nSyms = nSyms__prev2;
            }

        }

        nPkgs = nPkgs__prev1;
    } 

    // Inline body index.
    {
        var nPkgs__prev1 = nPkgs;

        for (nPkgs = ird.uint64(); nPkgs > 0; nPkgs--) {
            pkg = p.pkgAt(ird.uint64());

            {
                var nSyms__prev2 = nSyms;

                for (nSyms = ird.uint64(); nSyms > 0; nSyms--) {
                    s = pkg.Lookup(p.stringAt(ird.uint64()));
                    off = ird.uint64();

                    {
                        (_, ok) = inlineImporter[s];

                        if (!ok) {
                            inlineImporter[s] = new iimporterAndOffset(p,off);
                        }

                    }

                }


                nSyms = nSyms__prev2;
            }

        }

        nPkgs = nPkgs__prev1;
    } 

    // Fingerprint.
    _, err = io.ReadFull(in, fingerprint[..]);
    if (err != null) {
        @base.Errorf("import %s: error reading fingerprint", pkg.Path);
        @base.ErrorExit();
    }
    return fingerprint;

}

private partial struct iimporter {
    public ptr<types.Pkg> ipkg;
    public map<ulong, ptr<types.Pkg>> pkgCache;
    public map<ulong, ptr<src.PosBase>> posBaseCache;
    public map<ulong, ptr<types.Type>> typCache;
    public @string stringData;
    public @string declData;
}

private static @string stringAt(this ptr<iimporter> _addr_p, ulong off) {
    ref iimporter p = ref _addr_p.val;

    array<byte> x = new array<byte>(binary.MaxVarintLen64);
    var n = copy(x[..], p.stringData[(int)off..]);

    var (slen, n) = binary.Uvarint(x[..(int)n]);
    if (n <= 0) {
        @base.Fatalf("varint failed");
    }
    var spos = off + uint64(n);
    return p.stringData[(int)spos..(int)spos + slen];

}

private static ptr<src.PosBase> posBaseAt(this ptr<iimporter> _addr_p, ulong off) {
    ref iimporter p = ref _addr_p.val;

    {
        var posBase__prev1 = posBase;

        var (posBase, ok) = p.posBaseCache[off];

        if (ok) {
            return _addr_posBase!;
        }
        posBase = posBase__prev1;

    }


    var file = p.stringAt(off);
    var posBase = src.NewFileBase(file, file);
    p.posBaseCache[off] = posBase;
    return _addr_posBase!;

}

private static ptr<types.Pkg> pkgAt(this ptr<iimporter> _addr_p, ulong off) {
    ref iimporter p = ref _addr_p.val;

    {
        var pkg__prev1 = pkg;

        var (pkg, ok) = p.pkgCache[off];

        if (ok) {
            return _addr_pkg!;
        }
        pkg = pkg__prev1;

    }


    var pkg = p.ipkg;
    {
        var pkgPath = p.stringAt(off);

        if (pkgPath != "") {
            pkg = types.NewPkg(pkgPath, "");
        }
    }

    p.pkgCache[off] = pkg;
    return _addr_pkg!;

}

// An importReader keeps state for reading an individual imported
// object (declaration or inline body).
private partial struct importReader {
    public ref strings.Reader Reader => ref Reader_val;
    public ptr<iimporter> p;
    public ptr<types.Pkg> currPkg;
    public ptr<src.PosBase> prevBase;
    public long prevLine;
    public long prevColumn; // curfn is the current function we're importing into.
    public ptr<ir.Func> curfn; // Slice of all dcls for function, including any interior closures
    public slice<ptr<ir.Name>> allDcls;
    public slice<ptr<ir.Name>> allClosureVars;
}

private static ptr<importReader> newReader(this ptr<iimporter> _addr_p, ulong off, ptr<types.Pkg> _addr_pkg) {
    ref iimporter p = ref _addr_p.val;
    ref types.Pkg pkg = ref _addr_pkg.val;

    ptr<importReader> r = addr(new importReader(p:p,currPkg:pkg,)); 
    // (*strings.Reader).Reset wasn't added until Go 1.7, and we
    // need to build with Go 1.4.
    r.Reader = new ptr<ptr<strings.NewReader>>(p.declData[(int)off..]);
    return _addr_r!;

}

private static @string @string(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    return r.p.stringAt(r.uint64());
}
private static ptr<src.PosBase> posBase(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    return _addr_r.p.posBaseAt(r.uint64())!;
}
private static ptr<types.Pkg> pkg(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    return _addr_r.p.pkgAt(r.uint64())!;
}

private static void setPkg(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    r.currPkg = r.pkg();
}

private static ptr<ir.Name> doDecl(this ptr<importReader> _addr_r, ptr<types.Sym> _addr_sym) => func((_, panic, _) => {
    ref importReader r = ref _addr_r.val;
    ref types.Sym sym = ref _addr_sym.val;

    var tag = r.@byte();
    var pos = r.pos();

    switch (tag) {
        case 'A': 
            var typ = r.typ();

            return _addr_importalias(r.p.ipkg, pos, sym, typ)!;
            break;
        case 'C': 
            typ = r.typ();
            var val = r.value(typ);

            var n = importconst(r.p.ipkg, pos, sym, typ, val);
            r.constExt(n);
            return _addr_n!;
            break;
        case 'F': 
            typ = r.signature(null);

            n = importfunc(r.p.ipkg, pos, sym, typ);
            r.funcExt(n);
            return _addr_n!;
            break;
        case 'T': 
            // Types can be recursive. We need to setup a stub
            // declaration before recursing.
            n = importtype(r.p.ipkg, pos, sym);
            var t = n.Type(); 

            // We also need to defer width calculations until
            // after the underlying type has been assigned.
            types.DeferCheckSize();
            var underlying = r.typ();
            t.SetUnderlying(underlying);
            types.ResumeCheckSize();

            if (underlying.IsInterface()) {
                r.typeExt(t);
                return _addr_n!;
            }
            var ms = make_slice<ptr<types.Field>>(r.uint64());
            foreach (var (i) in ms) {
                var mpos = r.pos();
                var msym = r.selector();
                var recv = r.param();
                var mtyp = r.signature(recv); 

                // MethodSym already marked m.Sym as a function.
                var m = ir.NewNameAt(mpos, ir.MethodSym(recv.Type, msym));
                m.Class = ir.PFUNC;
                m.SetType(mtyp);

                m.Func = ir.NewFunc(mpos);
                m.Func.Nname = m;

                var f = types.NewField(mpos, msym, mtyp);
                f.Nname = m;
                ms[i] = f;

            }        t.Methods().Set(ms);

            r.typeExt(t);
            {
                var m__prev1 = m;

                foreach (var (_, __m) in ms) {
                    m = __m;
                    r.methExt(m);
                }

                m = m__prev1;
            }

            return _addr_n!;

            break;
        case 'V': 
            typ = r.typ();

            n = importvar(r.p.ipkg, pos, sym, typ);
            r.varExt(n);
            return _addr_n!;
            break;
        default: 
            @base.Fatalf("unexpected tag: %v", tag);
            panic("unreachable");
            break;
    }

});

private static constant.Value value(this ptr<importReader> _addr_p, ptr<types.Type> _addr_typ) => func((_, panic, _) => {
    ref importReader p = ref _addr_p.val;
    ref types.Type typ = ref _addr_typ.val;


    if (constTypeOf(typ) == constant.Bool) 
        return constant.MakeBool(p.@bool());
    else if (constTypeOf(typ) == constant.String) 
        return constant.MakeString(p.@string());
    else if (constTypeOf(typ) == constant.Int) 
        ref big.Int i = ref heap(out ptr<big.Int> _addr_i);
        p.mpint(_addr_i, typ);
        return constant.Make(_addr_i);
    else if (constTypeOf(typ) == constant.Float) 
        return p.@float(typ);
    else if (constTypeOf(typ) == constant.Complex) 
        return makeComplex(p.@float(typ), p.@float(typ));
        @base.Fatalf("unexpected value type: %v", typ);
    panic("unreachable");

});

private static void mpint(this ptr<importReader> _addr_p, ptr<big.Int> _addr_x, ptr<types.Type> _addr_typ) {
    ref importReader p = ref _addr_p.val;
    ref big.Int x = ref _addr_x.val;
    ref types.Type typ = ref _addr_typ.val;

    var (signed, maxBytes) = intSize(typ);

    nint maxSmall = 256 - maxBytes;
    if (signed) {
        maxSmall = 256 - 2 * maxBytes;
    }
    if (maxBytes == 1) {
        maxSmall = 256;
    }
    var (n, _) = p.ReadByte();
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
        @base.Fatalf("weird decoding: %v, %v => %v", n, signed, v);
    }
    var b = make_slice<byte>(v);
    p.Read(b);
    x.SetBytes(b);
    if (signed && n & 1 != 0) {
        x.Neg(x);
    }
}

private static constant.Value @float(this ptr<importReader> _addr_p, ptr<types.Type> _addr_typ) {
    ref importReader p = ref _addr_p.val;
    ref types.Type typ = ref _addr_typ.val;

    ref big.Int mant = ref heap(out ptr<big.Int> _addr_mant);
    p.mpint(_addr_mant, typ);
    ref big.Float f = ref heap(out ptr<big.Float> _addr_f);
    f.SetInt(_addr_mant);
    if (f.Sign() != 0) {
        f.SetMantExp(_addr_f, int(p.int64()));
    }
    return constant.Make(_addr_f);

}

private static constant.Value mprat(this ptr<importReader> _addr_p, constant.Value orig) {
    ref importReader p = ref _addr_p.val;

    if (!p.@bool()) {
        return orig;
    }
    ref big.Rat rat = ref heap(out ptr<big.Rat> _addr_rat);
    rat.SetString(p.@string());
    return constant.Make(_addr_rat);

}

private static ptr<types.Sym> ident(this ptr<importReader> _addr_r, bool selector) {
    ref importReader r = ref _addr_r.val;

    var name = r.@string();
    if (name == "") {
        return _addr_null!;
    }
    var pkg = r.currPkg;
    if (selector && types.IsExported(name)) {
        pkg = types.LocalPkg;
    }
    return _addr_pkg.Lookup(name)!;

}

private static ptr<types.Sym> localIdent(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    return _addr_r.ident(false)!;
}
private static ptr<types.Sym> selector(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    return _addr_r.ident(true)!;
}

private static ptr<ir.Ident> qualifiedIdent(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var name = r.@string();
    var pkg = r.pkg();
    var sym = pkg.Lookup(name);
    return _addr_ir.NewIdent(src.NoXPos, sym)!;
}

private static src.XPos pos(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var delta = r.int64();
    r.prevColumn += delta >> 1;
    if (delta & 1 != 0) {
        delta = r.int64();
        r.prevLine += delta >> 1;
        if (delta & 1 != 0) {
            r.prevBase = r.posBase();
        }
    }
    if ((r.prevBase == null || r.prevBase.AbsFilename() == "") && r.prevLine == 0 && r.prevColumn == 0) { 
        // TODO(mdempsky): Remove once we reliably write
        // position information for all nodes.
        return src.NoXPos;

    }
    if (r.prevBase == null) {
        @base.Fatalf("missing posbase");
    }
    var pos = src.MakePos(r.prevBase, uint(r.prevLine), uint(r.prevColumn));
    return @base.Ctxt.PosTable.XPos(pos);

}

private static ptr<types.Type> typ(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    return _addr_r.p.typAt(r.uint64())!;
}

private static ptr<types.Type> exoticType(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;


    if (r.uint64() == exoticTypeNil) 
        return _addr_null!;
    else if (r.uint64() == exoticTypeTuple) 
        var funarg = types.Funarg(r.uint64());
        var n = r.uint64();
        var fs = make_slice<ptr<types.Field>>(n);
        foreach (var (i) in fs) {
            var pos = r.pos();
            ptr<types.Sym> sym;

            if (r.uint64() == exoticTypeSymNil) 
                sym = null;
            else if (r.uint64() == exoticTypeSymNoPkg) 
                sym = types.NoPkg.Lookup(r.@string());
            else if (r.uint64() == exoticTypeSymWithPkg) 
                var pkg = r.pkg();
                sym = pkg.Lookup(r.@string());
            else 
                @base.Fatalf("unknown symbol kind");
                        var typ = r.typ();
            var f = types.NewField(pos, sym, typ);
            fs[i] = f;

        }        var t = types.NewStruct(types.NoPkg, fs);
        t.StructType().Funarg = funarg;
        return _addr_t!;
    else if (r.uint64() == exoticTypeRecv) 
        ptr<types.Field> rcvr;
        if (r.@bool()) { // isFakeRecv
            rcvr = fakeRecvField();

        }
        else
 {
            rcvr = r.exoticParam();
        }
        return _addr_r.exoticSignature(rcvr)!;
    else if (r.uint64() == exoticTypeRegular) 
        return _addr_r.typ()!;
    else 
        @base.Fatalf("bad kind of call type");
        return _addr_null!;
    
}

private static ptr<types.Sym> exoticSelector(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var name = r.@string();
    if (name == "") {
        return _addr_null!;
    }
    var pkg = r.currPkg;
    if (types.IsExported(name)) {
        pkg = types.LocalPkg;
    }
    if (r.uint64() != 0) {
        pkg = r.pkg();
    }
    return _addr_pkg.Lookup(name)!;

}

private static ptr<types.Type> exoticSignature(this ptr<importReader> _addr_r, ptr<types.Field> _addr_recv) {
    ref importReader r = ref _addr_r.val;
    ref types.Field recv = ref _addr_recv.val;

    ptr<types.Pkg> pkg;
    if (r.@bool()) { // hasPkg
        pkg = r.pkg();

    }
    var @params = r.exoticParamList();
    var results = r.exoticParamList();
    return _addr_types.NewSignature(pkg, recv, null, params, results)!;

}

private static slice<ptr<types.Field>> exoticParamList(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var n = r.uint64();
    var fs = make_slice<ptr<types.Field>>(n);
    foreach (var (i) in fs) {
        fs[i] = r.exoticParam();
    }    return fs;
}

private static ptr<types.Field> exoticParam(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var pos = r.pos();
    var sym = r.exoticSym();
    var off = r.uint64();
    var typ = r.exoticType();
    var ddd = r.@bool();
    var f = types.NewField(pos, sym, typ);
    f.Offset = int64(off);
    if (sym != null) {
        f.Nname = ir.NewNameAt(pos, sym);
    }
    f.SetIsDDD(ddd);
    return _addr_f!;

}

private static ptr<types.Field> exoticField(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var pos = r.pos();
    var sym = r.exoticSym();
    var off = r.uint64();
    var typ = r.exoticType();
    var note = r.@string();
    var f = types.NewField(pos, sym, typ);
    f.Offset = int64(off);
    if (sym != null) {
        f.Nname = ir.NewNameAt(pos, sym);
    }
    f.Note = note;
    return _addr_f!;

}

private static ptr<types.Sym> exoticSym(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var name = r.@string();
    if (name == "") {
        return _addr_null!;
    }
    ptr<types.Pkg> pkg;
    if (types.IsExported(name)) {
        pkg = types.LocalPkg;
    }
    else
 {
        pkg = r.pkg();
    }
    return _addr_pkg.Lookup(name)!;

}

private static ptr<types.Type> typAt(this ptr<iimporter> _addr_p, ulong off) {
    ref iimporter p = ref _addr_p.val;

    var (t, ok) = p.typCache[off];
    if (!ok) {
        if (off < predeclReserved) {
            @base.Fatalf("predeclared type missing from cache: %d", off);
        }
        t = p.newReader(off - predeclReserved, null).typ1(); 
        // Ensure size is calculated for imported types. Since CL 283313, the compiler
        // does not compile the function immediately when it sees them. Instead, funtions
        // are pushed to compile queue, then draining from the queue for compiling.
        // During this process, the size calculation is disabled, so it is not safe for
        // calculating size during SSA generation anymore. See issue #44732.
        types.CheckSize(t);
        p.typCache[off] = t;

    }
    return _addr_t!;

}

private static ptr<types.Type> typ1(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    {
        var k = r.kind();


        if (k == definedType) 
            // We might be called from within doInline, in which
            // case Sym.Def can point to declared parameters
            // instead of the top-level types. Also, we don't
            // support inlining functions with local defined
            // types. Therefore, this must be a package-scope
            // type.
            var n = expandDecl(r.qualifiedIdent());
            if (n.Op() != ir.OTYPE) {
                @base.Fatalf("expected OTYPE, got %v: %v, %v", n.Op(), n.Sym(), n);
            }
            return _addr_n.Type()!;
        else if (k == pointerType) 
            return _addr_types.NewPtr(r.typ())!;
        else if (k == sliceType) 
            return _addr_types.NewSlice(r.typ())!;
        else if (k == arrayType) 
            n = r.uint64();
            return _addr_types.NewArray(r.typ(), int64(n))!;
        else if (k == chanType) 
            var dir = types.ChanDir(r.uint64());
            return _addr_types.NewChan(r.typ(), dir)!;
        else if (k == mapType) 
            return _addr_types.NewMap(r.typ(), r.typ())!;
        else if (k == signatureType) 
            r.setPkg();
            return _addr_r.signature(null)!;
        else if (k == structType) 
            r.setPkg();

            var fs = make_slice<ptr<types.Field>>(r.uint64());
            {
                var i__prev1 = i;

                foreach (var (__i) in fs) {
                    i = __i;
                    var pos = r.pos();
                    var sym = r.selector();
                    var typ = r.typ();
                    var emb = r.@bool();
                    var note = r.@string();

                    var f = types.NewField(pos, sym, typ);
                    if (emb) {
                        f.Embedded = 1;
                    }
                    f.Note = note;
                    fs[i] = f;
                }

                i = i__prev1;
            }

            return _addr_types.NewStruct(r.currPkg, fs)!;
        else if (k == interfaceType) 
            r.setPkg();

            var embeddeds = make_slice<ptr<types.Field>>(r.uint64());
            {
                var i__prev1 = i;

                foreach (var (__i) in embeddeds) {
                    i = __i;
                    pos = r.pos();
                    typ = r.typ();

                    embeddeds[i] = types.NewField(pos, null, typ);
                }

                i = i__prev1;
            }

            var methods = make_slice<ptr<types.Field>>(r.uint64());
            {
                var i__prev1 = i;

                foreach (var (__i) in methods) {
                    i = __i;
                    pos = r.pos();
                    sym = r.selector();
                    typ = r.signature(fakeRecvField());

                    methods[i] = types.NewField(pos, sym, typ);
                }

                i = i__prev1;
            }

            var t = types.NewInterface(r.currPkg, append(embeddeds, methods)); 

            // Ensure we expand the interface in the frontend (#25055).
            types.CheckSize(t);
            return _addr_t!;
        else 
            @base.Fatalf("unexpected kind tag in %q: %v", r.p.ipkg.Path, k);
            return _addr_null!;

    }

}

private static itag kind(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    return itag(r.uint64());
}

private static ptr<types.Type> signature(this ptr<importReader> _addr_r, ptr<types.Field> _addr_recv) {
    ref importReader r = ref _addr_r.val;
    ref types.Field recv = ref _addr_recv.val;

    var @params = r.paramList();
    var results = r.paramList();
    {
        var n = len(params);

        if (n > 0) {
            params[n - 1].SetIsDDD(r.@bool());
        }
    }

    return _addr_types.NewSignature(r.currPkg, recv, null, params, results)!;

}

private static slice<ptr<types.Field>> paramList(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var fs = make_slice<ptr<types.Field>>(r.uint64());
    foreach (var (i) in fs) {
        fs[i] = r.param();
    }    return fs;
}

private static ptr<types.Field> param(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    return _addr_types.NewField(r.pos(), r.localIdent(), r.typ())!;
}

private static bool @bool(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    return r.uint64() != 0;
}

private static long int64(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var (n, err) = binary.ReadVarint(r);
    if (err != null) {
        @base.Fatalf("readVarint: %v", err);
    }
    return n;

}

private static ulong uint64(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var (n, err) = binary.ReadUvarint(r);
    if (err != null) {
        @base.Fatalf("readVarint: %v", err);
    }
    return n;

}

private static byte @byte(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var (x, err) = r.ReadByte();
    if (err != null) {
        @base.Fatalf("declReader.ReadByte: %v", err);
    }
    return x;

}

// Compiler-specific extensions.

private static void constExt(this ptr<importReader> _addr_r, ptr<ir.Name> _addr_n) {
    ref importReader r = ref _addr_r.val;
    ref ir.Name n = ref _addr_n.val;


    if (n.Type() == types.UntypedFloat) 
        n.SetVal(r.mprat(n.Val()));
    else if (n.Type() == types.UntypedComplex) 
        var v = n.Val();
        var re = r.mprat(constant.Real(v));
        var im = r.mprat(constant.Imag(v));
        n.SetVal(makeComplex(re, im));
    
}

private static void varExt(this ptr<importReader> _addr_r, ptr<ir.Name> _addr_n) {
    ref importReader r = ref _addr_r.val;
    ref ir.Name n = ref _addr_n.val;

    r.linkname(n.Sym());
    r.symIdx(n.Sym());
}

private static void funcExt(this ptr<importReader> _addr_r, ptr<ir.Name> _addr_n) {
    ref importReader r = ref _addr_r.val;
    ref ir.Name n = ref _addr_n.val;

    r.linkname(n.Sym());
    r.symIdx(n.Sym());

    n.Func.ABI = obj.ABI(r.uint64());

    n.SetPragma(ir.PragmaFlag(r.uint64())); 

    // Escape analysis.
    foreach (var (_, fs) in _addr_types.RecvsParams) {
        foreach (var (_, f) in fs(n.Type()).FieldSlice()) {
            f.Note = r.@string();
        }
    }    {
        var u = r.uint64();

        if (u > 0) {
            n.Func.Inl = addr(new ir.Inline(Cost:int32(u-1),));
            n.Func.Endlineno = r.pos();
        }
    }

}

private static void methExt(this ptr<importReader> _addr_r, ptr<types.Field> _addr_m) {
    ref importReader r = ref _addr_r.val;
    ref types.Field m = ref _addr_m.val;

    if (r.@bool()) {
        m.SetNointerface(true);
    }
    r.funcExt(m.Nname._<ptr<ir.Name>>());

}

private static void linkname(this ptr<importReader> _addr_r, ptr<types.Sym> _addr_s) {
    ref importReader r = ref _addr_r.val;
    ref types.Sym s = ref _addr_s.val;

    s.Linkname = r.@string();
}

private static void symIdx(this ptr<importReader> _addr_r, ptr<types.Sym> _addr_s) {
    ref importReader r = ref _addr_r.val;
    ref types.Sym s = ref _addr_s.val;

    var lsym = s.Linksym();
    var idx = int32(r.int64());
    if (idx != -1) {
        if (s.Linkname != "") {
            @base.Fatalf("bad index for linknamed symbol: %v %d\n", lsym, idx);
        }
        lsym.SymIdx = idx;
        lsym.Set(obj.AttrIndexed, true);

    }
}

private static void typeExt(this ptr<importReader> _addr_r, ptr<types.Type> _addr_t) {
    ref importReader r = ref _addr_r.val;
    ref types.Type t = ref _addr_t.val;

    t.SetNotInHeap(r.@bool());
    var i = r.int64();
    var pi = r.int64();
    if (i != -1 && pi != -1) {
        typeSymIdx[t] = new array<long>(new long[] { i, pi });
    }
}

// Map imported type T to the index of type descriptor symbols of T and *T,
// so we can use index to reference the symbol.
private static var typeSymIdx = make_map<ptr<types.Type>, array<long>>();

public static long BaseTypeIndex(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    var tbase = t;
    if (t.IsPtr() && t.Sym() == null && t.Elem().Sym() != null) {
        tbase = t.Elem();
    }
    var (i, ok) = typeSymIdx[tbase];
    if (!ok) {
        return -1;
    }
    if (t != tbase) {
        return i[1];
    }
    return i[0];

}

private static void doInline(this ptr<importReader> _addr_r, ptr<ir.Func> _addr_fn) {
    ref importReader r = ref _addr_r.val;
    ref ir.Func fn = ref _addr_fn.val;

    if (len(fn.Inl.Body) != 0) {
        @base.Fatalf("%v already has inline body", fn);
    }
    r.funcBody(fn);

    importlist = append(importlist, fn);

    if (@base.Flag.E > 0 && @base.Flag.LowerM > 2) {
        if (@base.Flag.LowerM > 3) {
            fmt.Printf("inl body for %v %v: %+v\n", fn, fn.Type(), ir.Nodes(fn.Inl.Body));
        }
        else
 {
            fmt.Printf("inl body for %v %v: %v\n", fn, fn.Type(), ir.Nodes(fn.Inl.Body));
        }
    }
}

// ----------------------------------------------------------------------------
// Inlined function bodies

// Approach: Read nodes and use them to create/declare the same data structures
// as done originally by the (hidden) parser by closely following the parser's
// original code. In other words, "parsing" the import data (which happens to
// be encoded in binary rather textual form) is the best way at the moment to
// re-establish the syntax tree's invariants. At some future point we might be
// able to avoid this round-about way and create the rewritten nodes directly,
// possibly avoiding a lot of duplicate work (name resolution, type checking).
//
// Refined nodes (e.g., ODOTPTR as a refinement of OXDOT) are exported as their
// unrefined nodes (since this is what the importer uses). The respective case
// entries are unreachable in the importer.

private static void funcBody(this ptr<importReader> _addr_r, ptr<ir.Func> _addr_fn) {
    ref importReader r = ref _addr_r.val;
    ref ir.Func fn = ref _addr_fn.val;

    var outerfn = r.curfn;
    r.curfn = fn; 

    // Import local declarations.
    fn.Inl.Dcl = r.readFuncDcls(fn); 

    // Import function body.
    var body = r.stmtList();
    if (body == null) { 
        // Make sure empty body is not interpreted as
        // no inlineable body (see also parser.fnbody)
        // (not doing so can cause significant performance
        // degradation due to unnecessary calls to empty
        // functions).
        body = new slice<ir.Node>(new ir.Node[] {  });

    }
    if (go117ExportTypes) {
        ir.VisitList(body, n => {
            n.SetTypecheck(1);
        });
    }
    fn.Inl.Body = body;

    r.curfn = outerfn;

}

private static slice<ptr<ir.Name>> readNames(this ptr<importReader> _addr_r, ptr<ir.Func> _addr_fn) {
    ref importReader r = ref _addr_r.val;
    ref ir.Func fn = ref _addr_fn.val;

    var dcls = make_slice<ptr<ir.Name>>(r.int64());
    foreach (var (i) in dcls) {
        var n = ir.NewDeclNameAt(r.pos(), ir.ONAME, r.localIdent());
        n.Class = ir.PAUTO; // overwritten below for parameters/results
        n.Curfn = fn;
        n.SetType(r.typ());
        dcls[i] = n;

    }    r.allDcls = append(r.allDcls, dcls);
    return dcls;

}

private static slice<ptr<ir.Name>> readFuncDcls(this ptr<importReader> _addr_r, ptr<ir.Func> _addr_fn) {
    ref importReader r = ref _addr_r.val;
    ref ir.Func fn = ref _addr_fn.val;

    var dcls = r.readNames(fn); 

    // Fixup parameter classes and associate with their
    // signature's type fields.
    nint i = 0;
    Action<ptr<types.Field>, ir.Class> fix = (f, @class) => {
        if (class == ir.PPARAM && (f.Sym == null || f.Sym.Name == "_")) {
            return ;
        }
        var n = dcls[i];
        n.Class = class;
        f.Nname = n;
        i++;

    };

    var typ = fn.Type();
    {
        var recv = typ.Recv();

        if (recv != null) {
            fix(recv, ir.PPARAM);
        }
    }

    {
        var f__prev1 = f;

        foreach (var (_, __f) in typ.Params().FieldSlice()) {
            f = __f;
            fix(f, ir.PPARAM);
        }
        f = f__prev1;
    }

    {
        var f__prev1 = f;

        foreach (var (_, __f) in typ.Results().FieldSlice()) {
            f = __f;
            fix(f, ir.PPARAMOUT);
        }
        f = f__prev1;
    }

    return dcls;

}

private static ptr<ir.Name> localName(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var i = r.int64();
    if (i == -1) {
        return ir.BlankNode._<ptr<ir.Name>>();
    }
    if (i < 0) {
        return _addr_r.allClosureVars[-i - 2]!;
    }
    return _addr_r.allDcls[i]!;

}

private static slice<ir.Node> stmtList(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    slice<ir.Node> list = default;
    while (true) {
        var n = r.node();
        if (n == null) {
            break;
        }
        if (n.Op() == ir.OBLOCK) {
            n = n._<ptr<ir.BlockStmt>>();
            list = append(list, n.List);
        }
        else
 {
            list = append(list, n);
        }
    }
    return list;

}

private static slice<ptr<ir.CaseClause>> caseList(this ptr<importReader> _addr_r, ir.Node switchExpr) {
    ref importReader r = ref _addr_r.val;

    var namedTypeSwitch = isNamedTypeSwitch(switchExpr);

    var cases = make_slice<ptr<ir.CaseClause>>(r.uint64());
    foreach (var (i) in cases) {
        var cas = ir.NewCaseStmt(r.pos(), null, null);
        cas.List = r.stmtList();
        if (namedTypeSwitch) {
            cas.Var = r.localName();
            cas.Var.Defn = switchExpr;
        }
        cas.Body = r.stmtList();
        cases[i] = cas;

    }    return cases;

}

private static slice<ptr<ir.CommClause>> commList(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var cases = make_slice<ptr<ir.CommClause>>(r.uint64());
    foreach (var (i) in cases) {
        cases[i] = ir.NewCommStmt(r.pos(), r.node(), r.stmtList());
    }    return cases;
}

private static slice<ir.Node> exprList(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    slice<ir.Node> list = default;
    while (true) {
        var n = r.expr();
        if (n == null) {
            break;
        }
        list = append(list, n);

    }
    return list;

}

private static ir.Node expr(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var n = r.node();
    if (n != null && n.Op() == ir.OBLOCK) {
        n = n._<ptr<ir.BlockStmt>>();
        @base.Fatalf("unexpected block node: %v", n);
    }
    return n;

}

// TODO(gri) split into expr and stmt
private static ir.Node node(this ptr<importReader> _addr_r) => func((_, panic, _) => {
    ref importReader r = ref _addr_r.val;

    var op = r.op();

    // expressions
    // case OPAREN:
    //     unreachable - unpacked by exporter

    if (op == ir.ONIL) 
        var pos = r.pos();
        var typ = r.typ();

        var n = ir.NewNilExpr(pos);
        n.SetType(typ);
        return n;
    else if (op == ir.OLITERAL) 
        pos = r.pos();
        typ = r.typ();

        n = ir.NewBasicLit(pos, r.value(typ));
        n.SetType(typ);
        return n;
    else if (op == ir.ONONAME) 
        n = r.qualifiedIdent();
        if (go117ExportTypes) {
            var n2 = Resolve(n);
            typ = r.typ();
            if (n2.Type() == null) {
                n2.SetType(typ);
            }
            return n2;
        }
        return n;
    else if (op == ir.ONAME) 
        return r.localName(); 

        // case OPACK, ONONAME:
        //     unreachable - should have been resolved by typechecking
    else if (op == ir.OTYPE) 
        return ir.TypeNode(r.typ());
    else if (op == ir.OTYPESW) 
        pos = r.pos();
        ptr<ir.Ident> tag;
        {
            var s = r.localIdent();

            if (s != null) {
                tag = ir.NewIdent(pos, s);
            }

        }

        return ir.NewTypeSwitchGuard(pos, tag, r.expr()); 

        // case OTARRAY, OTMAP, OTCHAN, OTSTRUCT, OTINTER, OTFUNC:
        //      unreachable - should have been resolved by typechecking
    else if (op == ir.OCLOSURE) 
        //println("Importing CLOSURE")
        pos = r.pos();
        typ = r.signature(null); 

        // All the remaining code below is similar to (*noder).funcLit(), but
        // with Dcls and ClosureVars lists already set up
        var fn = ir.NewFunc(pos);
        fn.SetIsHiddenClosure(true);
        fn.Nname = ir.NewNameAt(pos, ir.BlankNode.Sym());
        fn.Nname.Func = fn;
        fn.Nname.Ntype = ir.TypeNode(typ);
        fn.Nname.Defn = fn;
        fn.Nname.SetType(typ);

        var cvars = make_slice<ptr<ir.Name>>(r.int64());
        foreach (var (i) in cvars) {
            cvars[i] = ir.CaptureName(r.pos(), fn, r.localName().Canonical());
            if (go117ExportTypes) {
                if (cvars[i].Type() != null || cvars[i].Defn == null) {
                    @base.Fatalf("bad import of closure variable");
                } 
                // Closure variable should have Defn set, which is its captured
                // variable, and it gets the same type as the captured variable.
                cvars[i].SetType(cvars[i].Defn.Type());

            }

        }        fn.ClosureVars = cvars;
        r.allClosureVars = append(r.allClosureVars, cvars);

        fn.Inl = addr(new ir.Inline()); 
        // Read in the Dcls and Body of the closure after temporarily
        // setting r.curfn to fn.
        r.funcBody(fn);
        fn.Dcl = fn.Inl.Dcl;
        fn.Body = fn.Inl.Body;
        if (len(fn.Body) == 0) { 
            // An empty closure must be represented as a single empty
            // block statement, else it will be dropped.
            fn.Body = new slice<ir.Node>(new ir.Node[] { ir.NewBlockStmt(src.NoXPos,nil) });

        }
        fn.Inl = null;

        ir.FinishCaptureNames(pos, r.curfn, fn);

        var clo = ir.NewClosureExpr(pos, fn);
        fn.OClosure = clo;
        if (go117ExportTypes) {
            clo.SetType(typ);
        }
        return clo;
    else if (op == ir.OSTRUCTLIT) 
        if (go117ExportTypes) {
            pos = r.pos();
            typ = r.typ();
            var list = r.fieldList();
            n = ir.NewCompLitExpr(pos, ir.OSTRUCTLIT, null, list);
            n.SetType(typ);
            return n;
        }
        return ir.NewCompLitExpr(r.pos(), ir.OCOMPLIT, ir.TypeNode(r.typ()), r.fieldList());
    else if (op == ir.OCOMPLIT) 
        return ir.NewCompLitExpr(r.pos(), ir.OCOMPLIT, ir.TypeNode(r.typ()), r.exprList());
    else if (op == ir.OARRAYLIT || op == ir.OSLICELIT || op == ir.OMAPLIT) 
        if (!go117ExportTypes) { 
            // unreachable - mapped to OCOMPLIT by exporter
            goto error;

        }
        pos = r.pos();
        typ = r.typ();
        list = r.exprList();
        n = ir.NewCompLitExpr(pos, op, ir.TypeNode(typ), list);
        n.SetType(typ);
        if (op == ir.OSLICELIT) {
            n.Len = int64(r.uint64());
        }
        return n;
    else if (op == ir.OKEY) 
        return ir.NewKeyExpr(r.pos(), r.expr(), r.expr()); 

        // case OSTRUCTKEY:
        //    unreachable - handled in case OSTRUCTLIT by elemList
    else if (op == ir.OXDOT) 
        // see parser.new_dotname
        if (go117ExportTypes) {
            @base.Fatalf("shouldn't encounter XDOT in new importer");
        }
        return ir.NewSelectorExpr(r.pos(), ir.OXDOT, r.expr(), r.exoticSelector());
    else if (op == ir.ODOT || op == ir.ODOTPTR || op == ir.ODOTINTER || op == ir.ODOTMETH || op == ir.OCALLPART || op == ir.OMETHEXPR) 
        if (!go117ExportTypes) { 
            // unreachable - mapped to case OXDOT by exporter
            goto error;

        }
        pos = r.pos();
        var expr = r.expr();
        var sel = r.exoticSelector();
        n = ir.NewSelectorExpr(pos, op, expr, sel);
        n.SetType(r.exoticType());

        if (op == ir.ODOT || op == ir.ODOTPTR || op == ir.ODOTINTER) 
            n.Selection = r.exoticField();
        else if (op == ir.OMETHEXPR) 
            n = typecheckMethodExpr(n)._<ptr<ir.SelectorExpr>>();
        else if (op == ir.ODOTMETH || op == ir.OCALLPART) 
            // These require a Lookup to link to the correct declaration.
            var rcvrType = expr.Type();
            typ = n.Type();
            n.Selection = Lookdot(n, rcvrType, 1);
            if (op == ir.OCALLPART) { 
                // Lookdot clobbers the opcode and type, undo that.
                n.SetOp(op);
                n.SetType(typ);

            }

                return n;
    else if (op == ir.ODOTTYPE || op == ir.ODOTTYPE2) 
        n = ir.NewTypeAssertExpr(r.pos(), r.expr(), null);
        n.SetType(r.typ());
        if (go117ExportTypes) {
            n.SetOp(op);
        }
        return n;
    else if (op == ir.OINDEX || op == ir.OINDEXMAP) 
        n = ir.NewIndexExpr(r.pos(), r.expr(), r.expr());
        if (go117ExportTypes) {
            n.SetOp(op);
            n.SetType(r.typ());
            if (op == ir.OINDEXMAP) {
                n.Assigned = r.@bool();
            }
        }
        return n;
    else if (op == ir.OSLICE || op == ir.OSLICESTR || op == ir.OSLICEARR || op == ir.OSLICE3 || op == ir.OSLICE3ARR) 
        pos = r.pos();
        var x = r.expr();
        var (low, high) = r.exprsOrNil();
        ir.Node max = default;
        if (op.IsSlice3()) {
            max = r.expr();
        }
        n = ir.NewSliceExpr(pos, op, x, low, high, max);
        if (go117ExportTypes) {
            n.SetType(r.typ());
        }
        return n;
    else if (op == ir.OCONV || op == ir.OCONVIFACE || op == ir.OCONVNOP || op == ir.OBYTES2STR || op == ir.ORUNES2STR || op == ir.OSTR2BYTES || op == ir.OSTR2RUNES || op == ir.ORUNESTR || op == ir.OSLICE2ARRPTR) 
        if (!go117ExportTypes && op != ir.OCONV) { 
            //     unreachable - mapped to OCONV case by exporter
            goto error;

        }
        return ir.NewConvExpr(r.pos(), op, r.typ(), r.expr());
    else if (op == ir.OCOPY || op == ir.OCOMPLEX || op == ir.OREAL || op == ir.OIMAG || op == ir.OAPPEND || op == ir.OCAP || op == ir.OCLOSE || op == ir.ODELETE || op == ir.OLEN || op == ir.OMAKE || op == ir.ONEW || op == ir.OPANIC || op == ir.ORECOVER || op == ir.OPRINT || op == ir.OPRINTN || op == ir.OUNSAFEADD || op == ir.OUNSAFESLICE) 
        if (go117ExportTypes) {

            if (op == ir.OCOPY || op == ir.OCOMPLEX || op == ir.OUNSAFEADD || op == ir.OUNSAFESLICE) 
                n = ir.NewBinaryExpr(r.pos(), op, r.expr(), r.expr());
                n.SetType(r.typ());
                return n;
            else if (op == ir.OREAL || op == ir.OIMAG || op == ir.OCAP || op == ir.OCLOSE || op == ir.OLEN || op == ir.ONEW || op == ir.OPANIC) 
                n = ir.NewUnaryExpr(r.pos(), op, r.expr());
                if (op != ir.OPANIC) {
                    n.SetType(r.typ());
                }
                return n;
            else if (op == ir.OAPPEND || op == ir.ODELETE || op == ir.ORECOVER || op == ir.OPRINT || op == ir.OPRINTN) 
                n = ir.NewCallExpr(r.pos(), op, null, r.exprList());
                if (op == ir.OAPPEND) {
                    n.IsDDD = r.@bool();
                }
                if (op == ir.OAPPEND || op == ir.ORECOVER) {
                    n.SetType(r.typ());
                }
                return n;
            // ir.OMAKE
            goto error;

        }
        n = builtinCall(r.pos(), op);
        n.Args = r.exprList();
        if (op == ir.OAPPEND) {
            n.IsDDD = r.@bool();
        }
        return n;
    else if (op == ir.OCALL || op == ir.OCALLFUNC || op == ir.OCALLMETH || op == ir.OCALLINTER || op == ir.OGETG) 
        pos = r.pos();
        var init = r.stmtList();
        n = ir.NewCallExpr(pos, ir.OCALL, r.expr(), r.exprList());
        if (go117ExportTypes) {
            n.SetOp(op);
        }
        n.PtrInit().val = init;
        n.IsDDD = r.@bool();
        if (go117ExportTypes) {
            n.SetType(r.exoticType());
            n.Use = ir.CallUse(r.uint64());
        }
        return n;
    else if (op == ir.OMAKEMAP || op == ir.OMAKECHAN || op == ir.OMAKESLICE) 
        if (go117ExportTypes) {
            pos = r.pos();
            typ = r.typ();
            list = r.exprList();
            ir.Node len_ = default;            ir.Node cap_ = default;

            if (len(list) > 0) {
                len_ = list[0];
            }
            if (len(list) > 1) {
                cap_ = list[1];
            }
            n = ir.NewMakeExpr(pos, op, len_, cap_);
            n.SetType(typ);
            return n;
        }
        n = builtinCall(r.pos(), ir.OMAKE);
        n.Args.Append(ir.TypeNode(r.typ()));
        n.Args.Append(r.exprList());
        return n; 

        // unary expressions
    else if (op == ir.OPLUS || op == ir.ONEG || op == ir.OBITNOT || op == ir.ONOT || op == ir.ORECV) 
        n = ir.NewUnaryExpr(r.pos(), op, r.expr());
        if (go117ExportTypes) {
            n.SetType(r.typ());
        }
        return n;
    else if (op == ir.OADDR || op == ir.OPTRLIT) 
        n = NodAddrAt(r.pos(), r.expr());
        if (go117ExportTypes) {
            n.SetOp(op);
            n.SetType(r.typ());
        }
        return n;
    else if (op == ir.ODEREF) 
        n = ir.NewStarExpr(r.pos(), r.expr());
        if (go117ExportTypes) {
            n.SetType(r.typ());
        }
        return n; 

        // binary expressions
    else if (op == ir.OADD || op == ir.OAND || op == ir.OANDNOT || op == ir.ODIV || op == ir.OEQ || op == ir.OGE || op == ir.OGT || op == ir.OLE || op == ir.OLT || op == ir.OLSH || op == ir.OMOD || op == ir.OMUL || op == ir.ONE || op == ir.OOR || op == ir.ORSH || op == ir.OSUB || op == ir.OXOR) 
        n = ir.NewBinaryExpr(r.pos(), op, r.expr(), r.expr());
        if (go117ExportTypes) {
            n.SetType(r.typ());
        }
        return n;
    else if (op == ir.OANDAND || op == ir.OOROR) 
        n = ir.NewLogicalExpr(r.pos(), op, r.expr(), r.expr());
        if (go117ExportTypes) {
            n.SetType(r.typ());
        }
        return n;
    else if (op == ir.OSEND) 
        return ir.NewSendStmt(r.pos(), r.expr(), r.expr());
    else if (op == ir.OADDSTR) 
        pos = r.pos();
        list = r.exprList();
        if (go117ExportTypes) {
            n = ir.NewAddStringExpr(pos, list);
            n.SetType(r.typ());
            return n;
        }
        x = list[0];
        foreach (var (_, y) in list[(int)1..]) {
            x = ir.NewBinaryExpr(pos, ir.OADD, x, y);
        }        return x; 

        // --------------------------------------------------------------------
        // statements
    else if (op == ir.ODCL) 
        ir.Nodes stmts = default;
        n = r.localName();
        stmts.Append(ir.NewDecl(n.Pos(), ir.ODCL, n));
        stmts.Append(ir.NewAssignStmt(n.Pos(), n, null));
        return ir.NewBlockStmt(n.Pos(), stmts); 

        // case OASWB:
        //     unreachable - never exported
    else if (op == ir.OAS) 
        return ir.NewAssignStmt(r.pos(), r.expr(), r.expr());
    else if (op == ir.OASOP) 
        n = ir.NewAssignOpStmt(r.pos(), r.op(), r.expr(), null);
        if (!r.@bool()) {
            n.Y = ir.NewInt(1);
            n.IncDec = true;
        }
        else
 {
            n.Y = r.expr();
        }
        return n;
    else if (op == ir.OAS2 || op == ir.OAS2DOTTYPE || op == ir.OAS2FUNC || op == ir.OAS2MAPR || op == ir.OAS2RECV) 
        if (!go117ExportTypes && op != ir.OAS2) { 
            // unreachable - mapped to case OAS2 by exporter
            goto error;

        }
        return ir.NewAssignListStmt(r.pos(), op, r.exprList(), r.exprList());
    else if (op == ir.ORETURN) 
        return ir.NewReturnStmt(r.pos(), r.exprList()); 

        // case ORETJMP:
        //     unreachable - generated by compiler for trampolin routines (not exported)
    else if (op == ir.OGO || op == ir.ODEFER) 
        return ir.NewGoDeferStmt(r.pos(), op, r.expr());
    else if (op == ir.OIF) 
        pos = r.pos();
        init = r.stmtList();
        n = ir.NewIfStmt(pos, r.expr(), r.stmtList(), r.stmtList());
        n.PtrInit().val = init;
        return n;
    else if (op == ir.OFOR) 
        pos = r.pos();
        init = r.stmtList();
        var (cond, post) = r.exprsOrNil();
        n = ir.NewForStmt(pos, null, cond, post, r.stmtList());
        n.PtrInit().val = init;
        return n;
    else if (op == ir.ORANGE) 
        pos = r.pos();
        var (k, v) = r.exprsOrNil();
        return ir.NewRangeStmt(pos, k, v, r.expr(), r.stmtList());
    else if (op == ir.OSELECT) 
        pos = r.pos();
        init = r.stmtList();
        n = ir.NewSelectStmt(pos, r.commList());
        n.PtrInit().val = init;
        return n;
    else if (op == ir.OSWITCH) 
        pos = r.pos();
        init = r.stmtList();
        var (x, _) = r.exprsOrNil();
        n = ir.NewSwitchStmt(pos, x, r.caseList(x));
        n.PtrInit().val = init;
        return n; 

        // case OCASE:
        //    handled by caseList
    else if (op == ir.OFALL) 
        return ir.NewBranchStmt(r.pos(), ir.OFALL, null); 

        // case OEMPTY:
        //     unreachable - not emitted by exporter
    else if (op == ir.OBREAK || op == ir.OCONTINUE || op == ir.OGOTO) 
        pos = r.pos();
        ptr<types.Sym> sym;
        {
            var label = r.@string();

            if (label != "") {
                sym = Lookup(label);
            }

        }

        return ir.NewBranchStmt(pos, op, sym);
    else if (op == ir.OLABEL) 
        return ir.NewLabelStmt(r.pos(), Lookup(r.@string()));
    else if (op == ir.OEND) 
        return null;
    else 
        @base.Fatalf("cannot import %v (%d) node\n" + "\t==> please file an issue and assign to gri@", op, int(op));
        panic("unreachable"); // satisfy compiler
    error:
    @base.Fatalf("cannot import %v (%d) node\n" + "\t==> please file an issue and assign to khr@", op, int(op));
    panic("unreachable"); // satisfy compiler
});

private static ir.Op op(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    if (debug && r.uint64() != magic) {
        @base.Fatalf("import stream has desynchronized");
    }
    return ir.Op(r.uint64());

}

private static slice<ir.Node> fieldList(this ptr<importReader> _addr_r) {
    ref importReader r = ref _addr_r.val;

    var list = make_slice<ir.Node>(r.uint64());
    foreach (var (i) in list) {
        var x = ir.NewStructKeyExpr(r.pos(), r.selector(), r.expr());
        if (go117ExportTypes) {
            x.Offset = int64(r.uint64());
        }
        list[i] = x;

    }    return list;

}

private static (ir.Node, ir.Node) exprsOrNil(this ptr<importReader> _addr_r) {
    ir.Node a = default;
    ir.Node b = default;
    ref importReader r = ref _addr_r.val;

    var ab = r.uint64();
    if (ab & 1 != 0) {
        a = r.expr();
    }
    if (ab & 2 != 0) {
        b = r.node();
    }
    return ;

}

private static ptr<ir.CallExpr> builtinCall(src.XPos pos, ir.Op op) {
    if (go117ExportTypes) { 
        // These should all be encoded as direct ops, not OCALL.
        @base.Fatalf("builtinCall should not be invoked when types are included in import/export");

    }
    return _addr_ir.NewCallExpr(pos, ir.OCALL, ir.NewIdent(@base.Pos, types.BuiltinPkg.Lookup(ir.OpNames[op])), null)!;

}

} // end typecheck_package
