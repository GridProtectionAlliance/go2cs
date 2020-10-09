// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Indexed package import.
// See iexport.go for the export data format.

// package gc -- go2cs converted at 2020 October 09 05:41:41 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\iimport.go
using types = go.cmd.compile.@internal.types_package;
using bio = go.cmd.@internal.bio_package;
using goobj2 = go.cmd.@internal.goobj2_package;
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using io = go.io_package;
using big = go.math.big_package;
using os = go.os_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // An iimporterAndOffset identifies an importer and an offset within
        // its data section.
        private partial struct iimporterAndOffset
        {
            public ptr<iimporter> p;
            public ulong off;
        }

 
        // declImporter maps from imported identifiers to an importer
        // and offset where that identifier's declaration can be read.
        private static map declImporter = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<types.Sym>, iimporterAndOffset>{};        private static map inlineImporter = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<types.Sym>, iimporterAndOffset>{};

        private static void expandDecl(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Op != ONONAME)
            {
                return ;
            }

            var r = importReaderFor(_addr_n, declImporter);
            if (r == null)
            { 
                // Can happen if user tries to reference an undeclared name.
                return ;

            }

            r.doDecl(n);

        }

        private static void expandInline(ptr<Node> _addr_fn)
        {
            ref Node fn = ref _addr_fn.val;

            if (fn.Func.Inl.Body != null)
            {
                return ;
            }

            var r = importReaderFor(_addr_fn, inlineImporter);
            if (r == null)
            {
                Fatalf("missing import reader for %v", fn);
            }

            r.doInline(fn);

        }

        private static ptr<importReader> importReaderFor(ptr<Node> _addr_n, map<ptr<types.Sym>, iimporterAndOffset> importers)
        {
            ref Node n = ref _addr_n.val;

            var (x, ok) = importers[n.Sym];
            if (!ok)
            {
                return _addr_null!;
            }

            return _addr_x.p.newReader(x.off, n.Sym.Pkg)!;

        }

        private partial struct intReader
        {
            public ref ptr<bio.Reader> Reader> => ref Reader>_ptr;
            public ptr<types.Pkg> pkg;
        }

        private static long int64(this ptr<intReader> _addr_r)
        {
            ref intReader r = ref _addr_r.val;

            var (i, err) = binary.ReadVarint(r.Reader);
            if (err != null)
            {
                yyerror("import %q: read error: %v", r.pkg.Path, err);
                errorexit();
            }

            return i;

        }

        private static ulong uint64(this ptr<intReader> _addr_r)
        {
            ref intReader r = ref _addr_r.val;

            var (i, err) = binary.ReadUvarint(r.Reader);
            if (err != null)
            {
                yyerror("import %q: read error: %v", r.pkg.Path, err);
                errorexit();
            }

            return i;

        }

        private static goobj2.FingerprintType iimport(ptr<types.Pkg> _addr_pkg, ptr<bio.Reader> _addr_@in)
        {
            goobj2.FingerprintType fingerprint = default;
            ref types.Pkg pkg = ref _addr_pkg.val;
            ref bio.Reader @in = ref _addr_@in.val;

            ptr<intReader> ir = addr(new intReader(in,pkg));

            var version = ir.uint64();
            if (version != iexportVersion)
            {
                yyerror("import %q: unknown export format version %d", pkg.Path, version);
                errorexit();
            }

            var sLen = ir.uint64();
            var dLen = ir.uint64(); 

            // Map string (and data) section into memory as a single large
            // string. This reduces heap fragmentation and allows
            // returning individual substrings very efficiently.
            var (data, err) = mapFile(@in.File(), @in.Offset(), int64(sLen + dLen));
            if (err != null)
            {
                yyerror("import %q: mapping input: %v", pkg.Path, err);
                errorexit();
            }

            var stringData = data[..sLen];
            var declData = data[sLen..];

            @in.MustSeek(int64(sLen + dLen), os.SEEK_CUR);

            ptr<iimporter> p = addr(new iimporter(ipkg:pkg,pkgCache:map[uint64]*types.Pkg{},posBaseCache:map[uint64]*src.PosBase{},typCache:map[uint64]*types.Type{},stringData:stringData,declData:declData,));

            foreach (var (i, pt) in predeclared())
            {
                p.typCache[uint64(i)] = pt;
            } 

            // Declaration index.
            {
                var nPkgs__prev1 = nPkgs;

                for (var nPkgs = ir.uint64(); nPkgs > 0L; nPkgs--)
                {
                    var pkg = p.pkgAt(ir.uint64());
                    var pkgName = p.stringAt(ir.uint64());
                    var pkgHeight = int(ir.uint64());
                    if (pkg.Name == "")
                    {
                        pkg.Name = pkgName;
                        pkg.Height = pkgHeight;
                        numImport[pkgName]++; 

                        // TODO(mdempsky): This belongs somewhere else.
                        pkg.Lookup("_").Def;

                        asTypesNode(nblank);

                    }
                    else
                    {
                        if (pkg.Name != pkgName)
                        {
                            Fatalf("conflicting package names %v and %v for path %q", pkg.Name, pkgName, pkg.Path);
                        }

                        if (pkg.Height != pkgHeight)
                        {
                            Fatalf("conflicting package heights %v and %v for path %q", pkg.Height, pkgHeight, pkg.Path);
                        }

                    }

                    {
                        var nSyms__prev2 = nSyms;

                        for (var nSyms = ir.uint64(); nSyms > 0L; nSyms--)
                        {
                            var s = pkg.Lookup(p.stringAt(ir.uint64()));
                            var off = ir.uint64();

                            {
                                var (_, ok) = declImporter[s];

                                if (ok)
                                {
                                    continue;
                                }

                            }

                            declImporter[s] = new iimporterAndOffset(p,off); 

                            // Create stub declaration. If used, this will
                            // be overwritten by expandDecl.
                            if (s.Def != null)
                            {
                                Fatalf("unexpected definition for %v: %v", s, asNode(s.Def));
                            }

                            s.Def = asTypesNode(npos(src.NoXPos, dclname(s)));

                        }


                        nSyms = nSyms__prev2;
                    }

                } 

                // Inline body index.


                nPkgs = nPkgs__prev1;
            } 

            // Inline body index.
            {
                var nPkgs__prev1 = nPkgs;

                for (nPkgs = ir.uint64(); nPkgs > 0L; nPkgs--)
                {
                    pkg = p.pkgAt(ir.uint64());

                    {
                        var nSyms__prev2 = nSyms;

                        for (nSyms = ir.uint64(); nSyms > 0L; nSyms--)
                        {
                            s = pkg.Lookup(p.stringAt(ir.uint64()));
                            off = ir.uint64();

                            {
                                (_, ok) = inlineImporter[s];

                                if (ok)
                                {
                                    continue;
                                }

                            }

                            inlineImporter[s] = new iimporterAndOffset(p,off);

                        }


                        nSyms = nSyms__prev2;
                    }

                } 

                // Fingerprint


                nPkgs = nPkgs__prev1;
            } 

            // Fingerprint
            var (n, err) = io.ReadFull(in, fingerprint[..]);
            if (err != null || n != len(fingerprint))
            {
                yyerror("import %s: error reading fingerprint", pkg.Path);
                errorexit();
            }

            return fingerprint;

        }

        private partial struct iimporter
        {
            public ptr<types.Pkg> ipkg;
            public map<ulong, ptr<types.Pkg>> pkgCache;
            public map<ulong, ptr<src.PosBase>> posBaseCache;
            public map<ulong, ptr<types.Type>> typCache;
            public @string stringData;
            public @string declData;
        }

        private static @string stringAt(this ptr<iimporter> _addr_p, ulong off)
        {
            ref iimporter p = ref _addr_p.val;

            array<byte> x = new array<byte>(binary.MaxVarintLen64);
            var n = copy(x[..], p.stringData[off..]);

            var (slen, n) = binary.Uvarint(x[..n]);
            if (n <= 0L)
            {
                Fatalf("varint failed");
            }

            var spos = off + uint64(n);
            return p.stringData[spos..spos + slen];

        }

        private static ptr<src.PosBase> posBaseAt(this ptr<iimporter> _addr_p, ulong off)
        {
            ref iimporter p = ref _addr_p.val;

            {
                var posBase__prev1 = posBase;

                var (posBase, ok) = p.posBaseCache[off];

                if (ok)
                {
                    return _addr_posBase!;
                }

                posBase = posBase__prev1;

            }


            var file = p.stringAt(off);
            var posBase = src.NewFileBase(file, file);
            p.posBaseCache[off] = posBase;
            return _addr_posBase!;

        }

        private static ptr<types.Pkg> pkgAt(this ptr<iimporter> _addr_p, ulong off)
        {
            ref iimporter p = ref _addr_p.val;

            {
                var pkg__prev1 = pkg;

                var (pkg, ok) = p.pkgCache[off];

                if (ok)
                {
                    return _addr_pkg!;
                }

                pkg = pkg__prev1;

            }


            var pkg = p.ipkg;
            {
                var pkgPath = p.stringAt(off);

                if (pkgPath != "")
                {
                    pkg = types.NewPkg(pkgPath, "");
                }

            }

            p.pkgCache[off] = pkg;
            return _addr_pkg!;

        }

        // An importReader keeps state for reading an individual imported
        // object (declaration or inline body).
        private partial struct importReader
        {
            public ref strings.Reader Reader => ref Reader_val;
            public ptr<iimporter> p;
            public ptr<types.Pkg> currPkg;
            public ptr<src.PosBase> prevBase;
            public long prevLine;
            public long prevColumn;
        }

        private static ptr<importReader> newReader(this ptr<iimporter> _addr_p, ulong off, ptr<types.Pkg> _addr_pkg)
        {
            ref iimporter p = ref _addr_p.val;
            ref types.Pkg pkg = ref _addr_pkg.val;

            ptr<importReader> r = addr(new importReader(p:p,currPkg:pkg,)); 
            // (*strings.Reader).Reset wasn't added until Go 1.7, and we
            // need to build with Go 1.4.
            r.Reader = new ptr<ptr<strings.NewReader>>(p.declData[off..]);
            return _addr_r!;

        }

        private static @string @string(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            return r.p.stringAt(r.uint64());
        }
        private static ptr<src.PosBase> posBase(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            return _addr_r.p.posBaseAt(r.uint64())!;
        }
        private static ptr<types.Pkg> pkg(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            return _addr_r.p.pkgAt(r.uint64())!;
        }

        private static void setPkg(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            r.currPkg = r.pkg();
        }

        private static void doDecl(this ptr<importReader> _addr_r, ptr<Node> _addr_n)
        {
            ref importReader r = ref _addr_r.val;
            ref Node n = ref _addr_n.val;

            if (n.Op != ONONAME)
            {
                Fatalf("doDecl: unexpected Op for %v: %v", n.Sym, n.Op);
            }

            var tag = r.@byte();
            var pos = r.pos();

            switch (tag)
            {
                case 'A': 
                    var typ = r.typ();

                    importalias(r.p.ipkg, pos, n.Sym, typ);
                    break;
                case 'C': 
                    var (typ, val) = r.value();

                    importconst(r.p.ipkg, pos, n.Sym, typ, val);
                    break;
                case 'F': 
                    typ = r.signature(null);

                    importfunc(r.p.ipkg, pos, n.Sym, typ);
                    r.funcExt(n);
                    break;
                case 'T': 
                    // Types can be recursive. We need to setup a stub
                    // declaration before recursing.
                    var t = importtype(r.p.ipkg, pos, n.Sym); 

                    // We also need to defer width calculations until
                    // after the underlying type has been assigned.
                    defercheckwidth();
                    var underlying = r.typ();
                    setUnderlying(t, underlying);
                    resumecheckwidth();

                    if (underlying.IsInterface())
                    {
                        break;
                    }

                    var ms = make_slice<ptr<types.Field>>(r.uint64());
                    foreach (var (i) in ms)
                    {
                        var mpos = r.pos();
                        var msym = r.ident();
                        var recv = r.param();
                        var mtyp = r.signature(recv);

                        var f = types.NewField();
                        f.Pos = mpos;
                        f.Sym = msym;
                        f.Type = mtyp;
                        ms[i] = f;

                        var m = newfuncnamel(mpos, methodSym(recv.Type, msym));
                        m.Type = mtyp;
                        m.SetClass(PFUNC); 
                        // methodSym already marked m.Sym as a function.

                        // (comment from parser.go)
                        // inl.C's inlnode in on a dotmeth node expects to find the inlineable body as
                        // (dotmeth's type).Nname.Inl, and dotmeth's type has been pulled
                        // out by typecheck's lookdot as this $$.ttype. So by providing
                        // this back link here we avoid special casing there.
                        mtyp.SetNname(asTypesNode(m));

                    }
                    t.Methods().Set(ms);

                    {
                        var m__prev1 = m;

                        foreach (var (_, __m) in ms)
                        {
                            m = __m;
                            r.methExt(m);
                        }

                        m = m__prev1;
                    }
                    break;
                case 'V': 
                    typ = r.typ();

                    importvar(r.p.ipkg, pos, n.Sym, typ);
                    r.varExt(n);
                    break;
                default: 
                    Fatalf("unexpected tag: %v", tag);
                    break;
            }

        }

        private static (ptr<types.Type>, Val) value(this ptr<importReader> _addr_p)
        {
            ptr<types.Type> typ = default!;
            Val v = default;
            ref importReader p = ref _addr_p.val;

            typ = p.typ();


            if (constTypeOf(typ) == CTNIL) 
                v.U = addr(new NilVal());
            else if (constTypeOf(typ) == CTBOOL) 
                v.U = p.@bool();
            else if (constTypeOf(typ) == CTSTR) 
                v.U = p.@string();
            else if (constTypeOf(typ) == CTINT) 
                ptr<object> x = @new<Mpint>();
                x.Rune = typ == types.Idealrune;
                p.mpint(_addr_x.Val, typ);
                v.U = x;
            else if (constTypeOf(typ) == CTFLT) 
                x = newMpflt();
                p.@float(x, typ);
                v.U = x;
            else if (constTypeOf(typ) == CTCPLX) 
                x = newMpcmplx();
                p.@float(_addr_x.Real, typ);
                p.@float(_addr_x.Imag, typ);
                v.U = x;
                        return ;

        }

        private static void mpint(this ptr<importReader> _addr_p, ptr<big.Int> _addr_x, ptr<types.Type> _addr_typ)
        {
            ref importReader p = ref _addr_p.val;
            ref big.Int x = ref _addr_x.val;
            ref types.Type typ = ref _addr_typ.val;

            var (signed, maxBytes) = intSize(typ);

            long maxSmall = 256L - maxBytes;
            if (signed)
            {
                maxSmall = 256L - 2L * maxBytes;
            }

            if (maxBytes == 1L)
            {
                maxSmall = 256L;
            }

            var (n, _) = p.ReadByte();
            if (uint(n) < maxSmall)
            {
                var v = int64(n);
                if (signed)
                {
                    v >>= 1L;
                    if (n & 1L != 0L)
                    {
                        v = ~v;
                    }

                }

                x.SetInt64(v);
                return ;

            }

            v = -n;
            if (signed)
            {
                v = -(n & ~1L) >> (int)(1L);
            }

            if (v < 1L || uint(v) > maxBytes)
            {
                Fatalf("weird decoding: %v, %v => %v", n, signed, v);
            }

            var b = make_slice<byte>(v);
            p.Read(b);
            x.SetBytes(b);
            if (signed && n & 1L != 0L)
            {
                x.Neg(x);
            }

        }

        private static void @float(this ptr<importReader> _addr_p, ptr<Mpflt> _addr_x, ptr<types.Type> _addr_typ)
        {
            ref importReader p = ref _addr_p.val;
            ref Mpflt x = ref _addr_x.val;
            ref types.Type typ = ref _addr_typ.val;

            ref big.Int mant = ref heap(out ptr<big.Int> _addr_mant);
            p.mpint(_addr_mant, typ);
            var m = x.Val.SetInt(_addr_mant);
            if (m.Sign() == 0L)
            {
                return ;
            }

            m.SetMantExp(m, int(p.int64()));

        }

        private static ptr<types.Sym> ident(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            var name = r.@string();
            if (name == "")
            {
                return _addr_null!;
            }

            var pkg = r.currPkg;
            if (types.IsExported(name))
            {
                pkg = localpkg;
            }

            return _addr_pkg.Lookup(name)!;

        }

        private static ptr<types.Sym> qualifiedIdent(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            var name = r.@string();
            var pkg = r.pkg();
            return _addr_pkg.Lookup(name)!;
        }

        private static src.XPos pos(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            var delta = r.int64();
            r.prevColumn += delta >> (int)(1L);
            if (delta & 1L != 0L)
            {
                delta = r.int64();
                r.prevLine += delta >> (int)(1L);
                if (delta & 1L != 0L)
                {
                    r.prevBase = r.posBase();
                }

            }

            if ((r.prevBase == null || r.prevBase.AbsFilename() == "") && r.prevLine == 0L && r.prevColumn == 0L)
            { 
                // TODO(mdempsky): Remove once we reliably write
                // position information for all nodes.
                return src.NoXPos;

            }

            if (r.prevBase == null)
            {
                Fatalf("missing posbase");
            }

            var pos = src.MakePos(r.prevBase, uint(r.prevLine), uint(r.prevColumn));
            return Ctxt.PosTable.XPos(pos);

        }

        private static ptr<types.Type> typ(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            return _addr_r.p.typAt(r.uint64())!;
        }

        private static ptr<types.Type> typAt(this ptr<iimporter> _addr_p, ulong off)
        {
            ref iimporter p = ref _addr_p.val;

            var (t, ok) = p.typCache[off];
            if (!ok)
            {
                if (off < predeclReserved)
                {
                    Fatalf("predeclared type missing from cache: %d", off);
                }

                t = p.newReader(off - predeclReserved, null).typ1();
                p.typCache[off] = t;

            }

            return _addr_t!;

        }

        private static ptr<types.Type> typ1(this ptr<importReader> _addr_r)
        {
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
                    var n = asNode(r.qualifiedIdent().PkgDef());
                    if (n.Op == ONONAME)
                    {
                        expandDecl(_addr_n);
                    }

                    if (n.Op != OTYPE)
                    {
                        Fatalf("expected OTYPE, got %v: %v, %v", n.Op, n.Sym, n);
                    }

                    return _addr_n.Type!;
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

                        foreach (var (__i) in fs)
                        {
                            i = __i;
                            var pos = r.pos();
                            var sym = r.ident();
                            var typ = r.typ();
                            var emb = r.@bool();
                            var note = r.@string();

                            var f = types.NewField();
                            f.Pos = pos;
                            f.Sym = sym;
                            f.Type = typ;
                            if (emb)
                            {
                                f.Embedded = 1L;
                            }

                            f.Note = note;
                            fs[i] = f;

                        }

                        i = i__prev1;
                    }

                    var t = types.New(TSTRUCT);
                    t.SetPkg(r.currPkg);
                    t.SetFields(fs);
                    return _addr_t!;
                else if (k == interfaceType) 
                    r.setPkg();

                    var embeddeds = make_slice<ptr<types.Field>>(r.uint64());
                    {
                        var i__prev1 = i;

                        foreach (var (__i) in embeddeds)
                        {
                            i = __i;
                            pos = r.pos();
                            typ = r.typ();

                            f = types.NewField();
                            f.Pos = pos;
                            f.Type = typ;
                            embeddeds[i] = f;
                        }

                        i = i__prev1;
                    }

                    var methods = make_slice<ptr<types.Field>>(r.uint64());
                    {
                        var i__prev1 = i;

                        foreach (var (__i) in methods)
                        {
                            i = __i;
                            pos = r.pos();
                            sym = r.ident();
                            typ = r.signature(fakeRecvField());

                            f = types.NewField();
                            f.Pos = pos;
                            f.Sym = sym;
                            f.Type = typ;
                            methods[i] = f;
                        }

                        i = i__prev1;
                    }

                    t = types.New(TINTER);
                    t.SetPkg(r.currPkg);
                    t.SetInterface(append(embeddeds, methods)); 

                    // Ensure we expand the interface in the frontend (#25055).
                    checkwidth(t);

                    return _addr_t!;
                else 
                    Fatalf("unexpected kind tag in %q: %v", r.p.ipkg.Path, k);
                    return _addr_null!;

            }

        }

        private static itag kind(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            return itag(r.uint64());
        }

        private static ptr<types.Type> signature(this ptr<importReader> _addr_r, ptr<types.Field> _addr_recv)
        {
            ref importReader r = ref _addr_r.val;
            ref types.Field recv = ref _addr_recv.val;

            var @params = r.paramList();
            var results = r.paramList();
            {
                var n = len(params);

                if (n > 0L)
                {
                    params[n - 1L].SetIsDDD(r.@bool());
                }

            }

            var t = functypefield(recv, params, results);
            t.SetPkg(r.currPkg);
            return _addr_t!;

        }

        private static slice<ptr<types.Field>> paramList(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            var fs = make_slice<ptr<types.Field>>(r.uint64());
            foreach (var (i) in fs)
            {
                fs[i] = r.param();
            }
            return fs;

        }

        private static ptr<types.Field> param(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            var f = types.NewField();
            f.Pos = r.pos();
            f.Sym = r.ident();
            f.Type = r.typ();
            return _addr_f!;
        }

        private static bool @bool(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            return r.uint64() != 0L;
        }

        private static long int64(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            var (n, err) = binary.ReadVarint(r);
            if (err != null)
            {
                Fatalf("readVarint: %v", err);
            }

            return n;

        }

        private static ulong uint64(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            var (n, err) = binary.ReadUvarint(r);
            if (err != null)
            {
                Fatalf("readVarint: %v", err);
            }

            return n;

        }

        private static byte @byte(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            var (x, err) = r.ReadByte();
            if (err != null)
            {
                Fatalf("declReader.ReadByte: %v", err);
            }

            return x;

        }

        // Compiler-specific extensions.

        private static void varExt(this ptr<importReader> _addr_r, ptr<Node> _addr_n)
        {
            ref importReader r = ref _addr_r.val;
            ref Node n = ref _addr_n.val;

            r.linkname(n.Sym);
            r.symIdx(n.Sym);
        }

        private static void funcExt(this ptr<importReader> _addr_r, ptr<Node> _addr_n)
        {
            ref importReader r = ref _addr_r.val;
            ref Node n = ref _addr_n.val;

            r.linkname(n.Sym);
            r.symIdx(n.Sym); 

            // Escape analysis.
            foreach (var (_, fs) in _addr_types.RecvsParams)
            {
                foreach (var (_, f) in fs(n.Type).FieldSlice())
                {
                    f.Note = r.@string();
                }

            } 

            // Inline body.
            {
                var u = r.uint64();

                if (u > 0L)
                {
                    n.Func.Inl = addr(new Inline(Cost:int32(u-1),));
                    n.Func.Endlineno = r.pos();
                }

            }

        }

        private static void methExt(this ptr<importReader> _addr_r, ptr<types.Field> _addr_m)
        {
            ref importReader r = ref _addr_r.val;
            ref types.Field m = ref _addr_m.val;

            if (r.@bool())
            {
                m.SetNointerface(true);
            }

            r.funcExt(asNode(m.Type.Nname()));

        }

        private static void linkname(this ptr<importReader> _addr_r, ptr<types.Sym> _addr_s)
        {
            ref importReader r = ref _addr_r.val;
            ref types.Sym s = ref _addr_s.val;

            s.Linkname = r.@string();
        }

        private static void symIdx(this ptr<importReader> _addr_r, ptr<types.Sym> _addr_s)
        {
            ref importReader r = ref _addr_r.val;
            ref types.Sym s = ref _addr_s.val;

            if (Ctxt.Flag_go115newobj)
            {
                var lsym = s.Linksym();
                var idx = int32(r.int64());
                if (idx != -1L)
                {
                    if (s.Linkname != "")
                    {
                        Fatalf("bad index for linknamed symbol: %v %d\n", lsym, idx);
                    }

                    lsym.SymIdx = idx;
                    lsym.Set(obj.AttrIndexed, true);

                }

            }

        }

        private static void doInline(this ptr<importReader> _addr_r, ptr<Node> _addr_n)
        {
            ref importReader r = ref _addr_r.val;
            ref Node n = ref _addr_n.val;

            if (len(n.Func.Inl.Body) != 0L)
            {
                Fatalf("%v already has inline body", n);
            }

            funchdr(n);
            var body = r.stmtList();
            funcbody();
            if (body == null)
            { 
                //
                // Make sure empty body is not interpreted as
                // no inlineable body (see also parser.fnbody)
                // (not doing so can cause significant performance
                // degradation due to unnecessary calls to empty
                // functions).
                body = new slice<ptr<Node>>(new ptr<Node>[] {  });

            }

            n.Func.Inl.Body = body;

            importlist = append(importlist, n);

            if (Debug['E'] > 0L && Debug['m'] > 2L)
            {
                if (Debug['m'] > 3L)
                {
                    fmt.Printf("inl body for %v %#v: %+v\n", n, n.Type, asNodes(n.Func.Inl.Body));
                }
                else
                {
                    fmt.Printf("inl body for %v %#v: %v\n", n, n.Type, asNodes(n.Func.Inl.Body));
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

        private static slice<ptr<Node>> stmtList(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            slice<ptr<Node>> list = default;
            while (true)
            {
                var n = r.node();
                if (n == null)
                {
                    break;
                } 
                // OBLOCK nodes may be created when importing ODCL nodes - unpack them
                if (n.Op == OBLOCK)
                {
                    list = append(list, n.List.Slice());
                }
                else
                {
                    list = append(list, n);
                }

            }

            return list;

        }

        private static slice<ptr<Node>> exprList(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            slice<ptr<Node>> list = default;
            while (true)
            {
                var n = r.expr();
                if (n == null)
                {
                    break;
                }

                list = append(list, n);

            }

            return list;

        }

        private static ptr<Node> expr(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            var n = r.node();
            if (n != null && n.Op == OBLOCK)
            {
                Fatalf("unexpected block node: %v", n);
            }

            return _addr_n!;

        }

        // TODO(gri) split into expr and stmt
        private static ptr<Node> node(this ptr<importReader> _addr_r) => func((_, panic, __) =>
        {
            ref importReader r = ref _addr_r.val;

            {
                var op = r.op();


                // expressions
                // case OPAREN:
                //     unreachable - unpacked by exporter

                if (op == OLITERAL) 
                    var pos = r.pos();
                    var (typ, val) = r.value();

                    var n = npos(pos, nodlit(val));
                    n.Type = typ;
                    return _addr_n!;
                else if (op == ONONAME) 
                    return _addr_mkname(r.qualifiedIdent())!;
                else if (op == ONAME) 
                    return _addr_mkname(r.ident())!; 

                    // case OPACK, ONONAME:
                    //     unreachable - should have been resolved by typechecking
                else if (op == OTYPE) 
                    return _addr_typenod(r.typ())!; 

                    // case OTARRAY, OTMAP, OTCHAN, OTSTRUCT, OTINTER, OTFUNC:
                    //      unreachable - should have been resolved by typechecking

                    // case OCLOSURE:
                    //    unimplemented

                    // case OPTRLIT:
                    //    unreachable - mapped to case OADDR below by exporter
                else if (op == OSTRUCTLIT) 
                    // TODO(mdempsky): Export position information for OSTRUCTKEY nodes.
                    var savedlineno = lineno;
                    lineno = r.pos();
                    n = nodl(lineno, OCOMPLIT, null, typenod(r.typ()));
                    n.List.Set(r.elemList()); // special handling of field names
                    lineno = savedlineno;
                    return _addr_n!; 

                    // case OARRAYLIT, OSLICELIT, OMAPLIT:
                    //     unreachable - mapped to case OCOMPLIT below by exporter
                else if (op == OCOMPLIT) 
                    n = nodl(r.pos(), OCOMPLIT, null, typenod(r.typ()));
                    n.List.Set(r.exprList());
                    return _addr_n!;
                else if (op == OKEY) 
                    pos = r.pos();
                    var (left, right) = r.exprsOrNil();
                    return _addr_nodl(pos, OKEY, left, right)!; 

                    // case OSTRUCTKEY:
                    //    unreachable - handled in case OSTRUCTLIT by elemList

                    // case OCALLPART:
                    //    unimplemented

                    // case OXDOT, ODOT, ODOTPTR, ODOTINTER, ODOTMETH:
                    //     unreachable - mapped to case OXDOT below by exporter
                else if (op == OXDOT) 
                    // see parser.new_dotname
                    return _addr_npos(r.pos(), nodSym(OXDOT, r.expr(), r.ident()))!; 

                    // case ODOTTYPE, ODOTTYPE2:
                    //     unreachable - mapped to case ODOTTYPE below by exporter
                else if (op == ODOTTYPE) 
                    n = nodl(r.pos(), ODOTTYPE, r.expr(), null);
                    n.Type = r.typ();
                    return _addr_n!; 

                    // case OINDEX, OINDEXMAP, OSLICE, OSLICESTR, OSLICEARR, OSLICE3, OSLICE3ARR:
                    //     unreachable - mapped to cases below by exporter
                else if (op == OINDEX) 
                    return _addr_nodl(r.pos(), op, r.expr(), r.expr())!;
                else if (op == OSLICE || op == OSLICE3) 
                    n = nodl(r.pos(), op, r.expr(), null);
                    var (low, high) = r.exprsOrNil();
                    ptr<Node> max;
                    if (n.Op.IsSlice3())
                    {
                        max = r.expr();
                    }

                    n.SetSliceBounds(low, high, max);
                    return _addr_n!; 

                    // case OCONV, OCONVIFACE, OCONVNOP, OBYTES2STR, ORUNES2STR, OSTR2BYTES, OSTR2RUNES, ORUNESTR:
                    //     unreachable - mapped to OCONV case below by exporter
                else if (op == OCONV) 
                    n = nodl(r.pos(), OCONV, r.expr(), null);
                    n.Type = r.typ();
                    return _addr_n!;
                else if (op == OCOPY || op == OCOMPLEX || op == OREAL || op == OIMAG || op == OAPPEND || op == OCAP || op == OCLOSE || op == ODELETE || op == OLEN || op == OMAKE || op == ONEW || op == OPANIC || op == ORECOVER || op == OPRINT || op == OPRINTN) 
                    n = npos(r.pos(), builtinCall(op));
                    n.List.Set(r.exprList());
                    if (op == OAPPEND)
                    {
                        n.SetIsDDD(r.@bool());
                    }

                    return _addr_n!; 

                    // case OCALL, OCALLFUNC, OCALLMETH, OCALLINTER, OGETG:
                    //     unreachable - mapped to OCALL case below by exporter
                else if (op == OCALL) 
                    n = nodl(r.pos(), OCALL, null, null);
                    n.Ninit.Set(r.stmtList());
                    n.Left = r.expr();
                    n.List.Set(r.exprList());
                    n.SetIsDDD(r.@bool());
                    return _addr_n!;
                else if (op == OMAKEMAP || op == OMAKECHAN || op == OMAKESLICE) 
                    n = npos(r.pos(), builtinCall(OMAKE));
                    n.List.Append(typenod(r.typ()));
                    n.List.Append(r.exprList());
                    return _addr_n!; 

                    // unary expressions
                else if (op == OPLUS || op == ONEG || op == OADDR || op == OBITNOT || op == ODEREF || op == ONOT || op == ORECV) 
                    return _addr_nodl(r.pos(), op, r.expr(), null)!; 

                    // binary expressions
                else if (op == OADD || op == OAND || op == OANDAND || op == OANDNOT || op == ODIV || op == OEQ || op == OGE || op == OGT || op == OLE || op == OLT || op == OLSH || op == OMOD || op == OMUL || op == ONE || op == OOR || op == OOROR || op == ORSH || op == OSEND || op == OSUB || op == OXOR) 
                    return _addr_nodl(r.pos(), op, r.expr(), r.expr())!;
                else if (op == OADDSTR) 
                    pos = r.pos();
                    var list = r.exprList();
                    var x = npos(pos, list[0L]);
                    foreach (var (_, y) in list[1L..])
                    {
                        x = nodl(pos, OADD, x, y);
                    }
                    return _addr_x!; 

                    // --------------------------------------------------------------------
                    // statements
                else if (op == ODCL) 
                    pos = r.pos();
                    var lhs = npos(pos, dclname(r.ident()));
                    var typ = typenod(r.typ());
                    return _addr_npos(pos, liststmt(variter(new slice<ptr<Node>>(new ptr<Node>[] { lhs }), typ, null)))!; // TODO(gri) avoid list creation

                    // case ODCLFIELD:
                    //    unimplemented

                    // case OAS, OASWB:
                    //     unreachable - mapped to OAS case below by exporter
                else if (op == OAS) 
                    return _addr_nodl(r.pos(), OAS, r.expr(), r.expr())!;
                else if (op == OASOP) 
                    n = nodl(r.pos(), OASOP, null, null);
                    n.SetSubOp(r.op());
                    n.Left = r.expr();
                    if (!r.@bool())
                    {
                        n.Right = nodintconst(1L);
                        n.SetImplicit(true);
                    }
                    else
                    {
                        n.Right = r.expr();
                    }

                    return _addr_n!; 

                    // case OAS2DOTTYPE, OAS2FUNC, OAS2MAPR, OAS2RECV:
                    //     unreachable - mapped to OAS2 case below by exporter
                else if (op == OAS2) 
                    n = nodl(r.pos(), OAS2, null, null);
                    n.List.Set(r.exprList());
                    n.Rlist.Set(r.exprList());
                    return _addr_n!;
                else if (op == ORETURN) 
                    n = nodl(r.pos(), ORETURN, null, null);
                    n.List.Set(r.exprList());
                    return _addr_n!; 

                    // case ORETJMP:
                    //     unreachable - generated by compiler for trampolin routines (not exported)
                else if (op == OGO || op == ODEFER) 
                    return _addr_nodl(r.pos(), op, r.expr(), null)!;
                else if (op == OIF) 
                    n = nodl(r.pos(), OIF, null, null);
                    n.Ninit.Set(r.stmtList());
                    n.Left = r.expr();
                    n.Nbody.Set(r.stmtList());
                    n.Rlist.Set(r.stmtList());
                    return _addr_n!;
                else if (op == OFOR) 
                    n = nodl(r.pos(), OFOR, null, null);
                    n.Ninit.Set(r.stmtList());
                    n.Left, n.Right = r.exprsOrNil();
                    n.Nbody.Set(r.stmtList());
                    return _addr_n!;
                else if (op == ORANGE) 
                    n = nodl(r.pos(), ORANGE, null, null);
                    n.List.Set(r.stmtList());
                    n.Right = r.expr();
                    n.Nbody.Set(r.stmtList());
                    return _addr_n!;
                else if (op == OSELECT || op == OSWITCH) 
                    n = nodl(r.pos(), op, null, null);
                    n.Ninit.Set(r.stmtList());
                    n.Left, _ = r.exprsOrNil();
                    n.List.Set(r.stmtList());
                    return _addr_n!;
                else if (op == OCASE) 
                    n = nodl(r.pos(), OCASE, null, null);
                    n.List.Set(r.exprList()); 
                    // TODO(gri) eventually we must declare variables for type switch
                    // statements (type switch statements are not yet exported)
                    n.Nbody.Set(r.stmtList());
                    return _addr_n!;
                else if (op == OFALL) 
                    n = nodl(r.pos(), OFALL, null, null);
                    return _addr_n!;
                else if (op == OBREAK || op == OCONTINUE) 
                    pos = r.pos();
                    var (left, _) = r.exprsOrNil();
                    if (left != null)
                    {
                        left = newname(left.Sym);
                    }

                    return _addr_nodl(pos, op, left, null)!; 

                    // case OEMPTY:
                    //     unreachable - not emitted by exporter
                else if (op == OGOTO || op == OLABEL) 
                    n = nodl(r.pos(), op, null, null);
                    n.Sym = lookup(r.@string());
                    return _addr_n!;
                else if (op == OEND) 
                    return _addr_null!;
                else 
                    Fatalf("cannot import %v (%d) node\n" + "\t==> please file an issue and assign to gri@", op, int(op));
                    panic("unreachable"); // satisfy compiler

            }

        });

        private static Op op(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            return Op(r.uint64());
        }

        private static slice<ptr<Node>> elemList(this ptr<importReader> _addr_r)
        {
            ref importReader r = ref _addr_r.val;

            var c = r.uint64();
            var list = make_slice<ptr<Node>>(c);
            foreach (var (i) in list)
            {
                var s = r.ident();
                list[i] = nodSym(OSTRUCTKEY, r.expr(), s);
            }
            return list;

        }

        private static (ptr<Node>, ptr<Node>) exprsOrNil(this ptr<importReader> _addr_r)
        {
            ptr<Node> a = default!;
            ptr<Node> b = default!;
            ref importReader r = ref _addr_r.val;

            var ab = r.uint64();
            if (ab & 1L != 0L)
            {
                a = r.expr();
            }

            if (ab & 2L != 0L)
            {
                b = r.node();
            }

            return ;

        }
    }
}}}}
