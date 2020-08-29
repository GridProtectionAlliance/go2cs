// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Binary package import.
// See bexport.go for the export data format and how
// to make a format change.

// package gc -- go2cs converted at 2020 August 29 09:25:50 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\bimport.go
using bufio = go.bufio_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using big = go.math.big_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // The overall structure of Import is symmetric to Export: For each
        // export method in bexport.go there is a matching and symmetric method
        // in bimport.go. Changing the export format requires making symmetric
        // changes to bimport.go and bexport.go.
        private partial struct importer
        {
            public ptr<bufio.Reader> @in;
            public ptr<types.Pkg> imp; // imported package
            public slice<byte> buf; // reused for reading strings
            public long version; // export format version

// object lists, in order of deserialization
            public slice<@string> strList;
            public slice<@string> pathList;
            public slice<ref types.Pkg> pkgList;
            public slice<ref types.Type> typList;
            public slice<ref Node> funcList; // nil entry means already declared
            public bool trackAllTypes; // for delayed type verification
            public slice<object> cmpList; // position encoding
            public bool posInfoFormat;
            public @string prevFile;
            public long prevLine;
            public ptr<src.PosBase> posBase; // debugging support
            public bool debugFormat;
            public long read; // bytes read
        }

        // Import populates imp from the serialized package data read from in.
        public static void Import(ref types.Pkg _imp, ref bufio.Reader _@in) => func(_imp, _@in, (ref types.Pkg imp, ref bufio.Reader @in, Defer defer, Panic _, Recover __) =>
        {
            inimport = true;
            defer(() =>
            {
                inimport = false;

            }());

            importer p = new importer(in:in,imp:imp,version:-1,strList:[]string{""},pathList:[]string{""},); 

            // read version info
            @string versionstr = default;
            {
                var b = p.rawByte();

                if (b == 'c' || b == 'd')
                { 
                    // Go1.7 encoding; first byte encodes low-level
                    // encoding format (compact vs debug).
                    // For backward-compatibility only (avoid problems with
                    // old installed packages). Newly compiled packages use
                    // the extensible format string.
                    // TODO(gri) Remove this support eventually; after Go1.8.
                    if (b == 'd')
                    {
                        p.debugFormat = true;
                    }
                    p.trackAllTypes = p.rawByte() == 'a';
                    p.posInfoFormat = p.@bool();
                    versionstr = p.@string();
                    if (versionstr == "v1")
                    {
                        p.version = 0L;
                    }
                }
                else
                { 
                    // Go1.8 extensible encoding
                    // read version string and extract version number (ignore anything after the version number)
                    versionstr = p.rawStringln(b);
                    {
                        var s = strings.SplitN(versionstr, " ", 3L);

                        if (len(s) >= 2L && s[0L] == "version")
                        {
                            {
                                var (v, err) = strconv.Atoi(s[1L]);

                                if (err == null && v > 0L)
                                {
                                    p.version = v;
                                }

                            }
                        }

                    }
                } 

                // read version specific flags - extend as necessary

            } 

            // read version specific flags - extend as necessary
            switch (p.version)
            { 
            // case 6:
            //     ...
            //    fallthrough
                case 5L: 

                case 4L: 

                case 3L: 

                case 2L: 

                case 1L: 
                    p.debugFormat = p.rawStringln(p.rawByte()) == "debug";
                    p.trackAllTypes = p.@bool();
                    p.posInfoFormat = p.@bool();
                    break;
                case 0L: 
                    break;
                default: 
                    p.formatErrorf("unknown export format version %d (%q)", p.version, versionstr);
                    break;
            } 

            // --- generic export data ---

            // populate typList with predeclared "known" types
            p.typList = append(p.typList, predeclared()); 

            // read package data
            p.pkg(); 

            // defer some type-checking until all types are read in completely
            var tcok = typecheckok;
            typecheckok = true;
            defercheckwidth(); 

            // read objects

            // phase 1
            long objcount = 0L;
            while (true)
            {
                var tag = p.tagOrIndex();
                if (tag == endTag)
                {
                    break;
                }
                p.obj(tag);
                objcount++;
            } 

            // self-verification
 

            // self-verification
            {
                var count__prev1 = count;

                var count = p.@int();

                if (count != objcount)
                {
                    p.formatErrorf("got %d objects; want %d", objcount, count);
                } 

                // --- compiler-specific export data ---

                // read compiler-specific flags

                // phase 2

                count = count__prev1;

            } 

            // --- compiler-specific export data ---

            // read compiler-specific flags

            // phase 2
            objcount = 0L;
            while (true)
            {
                tag = p.tagOrIndex();
                if (tag == endTag)
                {
                    break;
                }
                p.obj(tag);
                objcount++;
            } 

            // self-verification
 

            // self-verification
            {
                var count__prev1 = count;

                count = p.@int();

                if (count != objcount)
                {
                    p.formatErrorf("got %d objects; want %d", objcount, count);
                } 

                // read inlineable functions bodies

                count = count__prev1;

            } 

            // read inlineable functions bodies
            if (dclcontext != PEXTERN)
            {
                p.formatErrorf("unexpected context %d", dclcontext);
            }
            objcount = 0L;
            {
                long i0 = -1L;

                while (>>MARKER:FOREXPRESSION_LEVEL_1<<)
                {
                    var i = p.@int(); // index of function with inlineable body
                    if (i < 0L)
                    {
                        break;
                    } 

                    // don't process the same function twice
                    if (i <= i0)
                    {
                        p.formatErrorf("index not increasing: %d <= %d", i, i0);
                    }
                    i0 = i;

                    if (funcdepth != 0L)
                    {
                        p.formatErrorf("unexpected Funcdepth %d", funcdepth);
                    } 

                    // Note: In the original code, funchdr and funcbody are called for
                    // all functions (that were not yet imported). Now, we are calling
                    // them only for functions with inlineable bodies. funchdr does
                    // parameter renaming which doesn't matter if we don't have a body.
                    var inlCost = p.@int();
                    {
                        var f = p.funcList[i];

                        if (f != null && f.Func.Inl.Len() == 0L)
                        { 
                            // function not yet imported - read body and set it
                            funchdr(f);
                            var body = p.stmtList();
                            if (body == null)
                            { 
                                // Make sure empty body is not interpreted as
                                // no inlineable body (see also parser.fnbody)
                                // (not doing so can cause significant performance
                                // degradation due to unnecessary calls to empty
                                // functions).
                                body = new slice<ref Node>(new ref Node[] { nod(OEMPTY,nil,nil) });
                            }
                            f.Func.Inl.Set(body);
                            f.Func.InlCost = int32(inlCost);
                            if (Debug['E'] > 0L && Debug['m'] > 2L && f.Func.Inl.Len() != 0L)
                            {
                                if (Debug['m'] > 3L)
                                {
                                    fmt.Printf("inl body for %v: %+v\n", f, f.Func.Inl);
                                }
                                else
                                {
                                    fmt.Printf("inl body for %v: %v\n", f, f.Func.Inl);
                                }
                            }
                            funcbody();
                        }
                        else
                        { 
                            // function already imported - read body but discard declarations
                            dclcontext = PDISCARD; // throw away any declarations
                            p.stmtList();
                            dclcontext = PEXTERN;
                        }

                    }

                    objcount++;
                } 

                // self-verification

            } 

            // self-verification
            {
                var count__prev1 = count;

                count = p.@int();

                if (count != objcount)
                {
                    p.formatErrorf("got %d functions; want %d", objcount, count);
                }

                count = count__prev1;

            }

            if (dclcontext != PEXTERN)
            {
                p.formatErrorf("unexpected context %d", dclcontext);
            }
            p.verifyTypes(); 

            // --- end of export data ---

            typecheckok = tcok;
            resumecheckwidth();

            if (debug_dclstack != 0L)
            {
                testdclstack();
            }
        });

        private static void formatErrorf(this ref importer p, @string format, params object[] args)
        {
            if (debugFormat)
            {
                Fatalf(format, args);
            }
            yyerror("cannot import %q due to version skew - reinstall package (%s)", p.imp.Path, fmt.Sprintf(format, args));
            errorexit();
        }

        private static void verifyTypes(this ref importer p)
        {
            foreach (var (_, pair) in p.cmpList)
            {
                var pt = pair.pt;
                var t = pair.t;
                if (!eqtype(pt.Orig, t))
                {
                    p.formatErrorf("inconsistent definition for type %v during import\n\t%L (in %q)\n\t%L (in %q)", pt.Sym, pt, pt.Sym.Importdef.Path, t, p.imp.Path);
                }
            }
        }

        // numImport tracks how often a package with a given name is imported.
        // It is used to provide a better error message (by using the package
        // path to disambiguate) if a package that appears multiple times with
        // the same name appears in an error message.
        private static var numImport = make_map<@string, long>();

        private static ref types.Pkg pkg(this ref importer p)
        { 
            // if the package was seen before, i is its index (>= 0)
            var i = p.tagOrIndex();
            if (i >= 0L)
            {
                return p.pkgList[i];
            } 

            // otherwise, i is the package tag (< 0)
            if (i != packageTag)
            {
                p.formatErrorf("expected package tag, found tag = %d", i);
            } 

            // read package data
            var name = p.@string();
            @string path = default;
            if (p.version >= 5L)
            {
                path = p.path();
            }
            else
            {
                path = p.@string();
            } 

            // we should never see an empty package name
            if (name == "")
            {
                p.formatErrorf("empty package name for path %q", path);
            } 

            // we should never see a bad import path
            if (isbadimport(path, true))
            {
                p.formatErrorf("bad package path %q for package %s", path, name);
            } 

            // an empty path denotes the package we are currently importing;
            // it must be the first package we see
            if ((path == "") != (len(p.pkgList) == 0L))
            {
                p.formatErrorf("package path %q for pkg index %d", path, len(p.pkgList));
            } 

            // add package to pkgList
            var pkg = p.imp;
            if (path != "")
            {
                pkg = types.NewPkg(path, "");
            }
            if (pkg.Name == "")
            {
                pkg.Name = name;
                numImport[name]++;
            }
            else if (pkg.Name != name)
            {
                yyerror("conflicting package names %s and %s for path %q", pkg.Name, name, path);
            }
            if (myimportpath != "" && path == myimportpath)
            {
                yyerror("import %q: package depends on %q (import cycle)", p.imp.Path, path);
                errorexit();
            }
            p.pkgList = append(p.pkgList, pkg);

            return pkg;
        }

        private static ref types.Type idealType(ref types.Type typ)
        {
            if (typ.IsUntyped())
            { 
                // canonicalize ideal types
                typ = types.Types[TIDEAL];
            }
            return typ;
        }

        private static void obj(this ref importer p, long tag)
        {

            if (tag == constTag) 
                var pos = p.pos();
                var sym = p.qualifiedName();
                var typ = p.typ();
                var val = p.value(typ);
                importconst(p.imp, sym, idealType(typ), npos(pos, nodlit(val)));
            else if (tag == aliasTag) 
                pos = p.pos();
                sym = p.qualifiedName();
                typ = p.typ();
                importalias(pos, p.imp, sym, typ);
            else if (tag == typeTag) 
                p.typ();
            else if (tag == varTag) 
                pos = p.pos();
                sym = p.qualifiedName();
                typ = p.typ();
                importvar(pos, p.imp, sym, typ);
            else if (tag == funcTag) 
                pos = p.pos();
                sym = p.qualifiedName();
                var @params = p.paramList();
                var result = p.paramList();

                var sig = functypefield(null, params, result);
                importsym(p.imp, sym, ONAME);
                {
                    var old = asNode(sym.Def);

                    if (old != null && old.Op == ONAME)
                    { 
                        // function was imported before (via another import)
                        if (!eqtype(sig, old.Type))
                        {
                            p.formatErrorf("inconsistent definition for func %v during import\n\t%v\n\t%v", sym, old.Type, sig);
                        }
                        var n = asNode(old.Type.Nname());
                        p.funcList = append(p.funcList, n);
                        break;
                    }

                }

                n = newfuncnamel(pos, sym);
                n.Type = sig; 
                // TODO(mdempsky): Stop clobbering n.Pos in declare.
                var savedlineno = lineno;
                lineno = pos;
                declare(n, PFUNC);
                lineno = savedlineno;
                p.funcList = append(p.funcList, n);
                importlist = append(importlist, n);

                sig.SetNname(asTypesNode(n));

                if (Debug['E'] > 0L)
                {
                    fmt.Printf("import [%q] func %v \n", p.imp.Path, n);
                }
            else 
                p.formatErrorf("unexpected object (tag = %d)", tag);
                    }

        private static src.XPos pos(this ref importer p)
        {
            if (!p.posInfoFormat)
            {
                return src.NoXPos;
            }
            var file = p.prevFile;
            var line = p.prevLine;
            var delta = p.@int();
            line += delta;
            if (p.version >= 5L)
            {
                if (delta == deltaNewFile)
                {
                    {
                        var n__prev3 = n;

                        var n = p.@int();

                        if (n >= 0L)
                        { 
                            // file changed
                            file = p.path();
                            line = n;
                        }

                        n = n__prev3;

                    }
                }
            }
            else
            {
                if (delta == 0L)
                {
                    {
                        var n__prev3 = n;

                        n = p.@int();

                        if (n >= 0L)
                        { 
                            // file changed
                            file = p.prevFile[..n] + p.@string();
                            line = p.@int();
                        }

                        n = n__prev3;

                    }
                }
            }
            if (file != p.prevFile)
            {
                p.prevFile = file;
                p.posBase = src.NewFileBase(file, file);
            }
            p.prevLine = line;

            var pos = src.MakePos(p.posBase, uint(line), 0L);
            var xpos = Ctxt.PosTable.XPos(pos);
            return xpos;
        }

        private static @string path(this ref importer p)
        { 
            // if the path was seen before, i is its index (>= 0)
            // (the empty string is at index 0)
            var i = p.@int();
            if (i >= 0L)
            {
                return p.pathList[i];
            } 
            // otherwise, i is the negative path length (< 0)
            var a = make_slice<@string>(-i);
            foreach (var (n) in a)
            {
                a[n] = p.@string();
            }
            var s = strings.Join(a, "/");
            p.pathList = append(p.pathList, s);
            return s;
        }

        private static ref types.Type newtyp(this ref importer p, types.EType etype)
        {
            var t = types.New(etype);
            if (p.trackAllTypes)
            {
                p.typList = append(p.typList, t);
            }
            return t;
        }

        // importtype declares that pt, an imported named type, has underlying type t.
        private static void importtype(this ref importer p, ref types.Type pt, ref types.Type t)
        {
            if (pt.Etype == TFORW)
            {
                copytype(asNode(pt.Nod), t);
                pt.Sym.Importdef = p.imp;
                pt.Sym.Lastlineno = lineno;
                declare(asNode(pt.Nod), PEXTERN);
                checkwidth(pt);
            }
            else
            { 
                // pt.Orig and t must be identical.
                if (p.trackAllTypes)
                { 
                    // If we track all types, t may not be fully set up yet.
                    // Collect the types and verify identity later.
                    p.cmpList = append(p.cmpList, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{pt,t*types.Type}{pt,t});
                }
                else if (!eqtype(pt.Orig, t))
                {
                    yyerror("inconsistent definition for type %v during import\n\t%L (in %q)\n\t%L (in %q)", pt.Sym, pt, pt.Sym.Importdef.Path, t, p.imp.Path);
                }
            }
            if (Debug['E'] != 0L)
            {
                fmt.Printf("import type %v %L\n", pt, t);
            }
        }

        private static ref types.Type typ(this ref importer p)
        { 
            // if the type was seen before, i is its index (>= 0)
            var i = p.tagOrIndex();
            if (i >= 0L)
            {
                return p.typList[i];
            } 

            // otherwise, i is the type tag (< 0)
            ref types.Type t = default;

            if (i == namedTag) 
                var pos = p.pos();
                var tsym = p.qualifiedName();

                t = pkgtype(pos, p.imp, tsym);
                p.typList = append(p.typList, t);
                var dup = !t.IsKind(types.TFORW); // type already imported

                // read underlying type
                var t0 = p.typ(); 
                // TODO(mdempsky): Stop clobbering n.Pos in declare.
                var savedlineno = lineno;
                lineno = pos;
                p.importtype(t, t0);
                lineno = savedlineno; 

                // interfaces don't have associated methods
                if (t0.IsInterface())
                {
                    break;
                } 

                // set correct import context (since p.typ() may be called
                // while importing the body of an inlined function)
                var savedContext = dclcontext;
                dclcontext = PEXTERN; 

                // read associated methods
                {
                    var i__prev1 = i;

                    for (i = p.@int(); i > 0L; i--)
                    {
                        var mpos = p.pos();
                        var sym = p.fieldSym(); 

                        // during import unexported method names should be in the type's package
                        if (!exportname(sym.Name) && sym.Pkg != tsym.Pkg)
                        {
                            Fatalf("imported method name %+v in wrong package %s\n", sym, tsym.Pkg.Name);
                        }
                        var recv = p.paramList(); // TODO(gri) do we need a full param list for the receiver?
                        var @params = p.paramList();
                        var result = p.paramList();
                        var nointerface = p.@bool();

                        var mt = functypefield(recv[0L], params, result);
                        var oldm = addmethod(sym, mt, false, nointerface);

                        if (dup)
                        { 
                            // An earlier import already declared this type and its methods.
                            // Discard the duplicate method declaration.
                            var n = asNode(oldm.Type.Nname());
                            p.funcList = append(p.funcList, n);
                            continue;
                        }
                        n = newfuncnamel(mpos, methodname(sym, recv[0L].Type));
                        n.Type = mt;
                        n.SetClass(PFUNC);
                        checkwidth(n.Type);
                        p.funcList = append(p.funcList, n);
                        importlist = append(importlist, n); 

                        // (comment from parser.go)
                        // inl.C's inlnode in on a dotmeth node expects to find the inlineable body as
                        // (dotmeth's type).Nname.Inl, and dotmeth's type has been pulled
                        // out by typecheck's lookdot as this $$.ttype. So by providing
                        // this back link here we avoid special casing there.
                        mt.SetNname(asTypesNode(n));

                        if (Debug['E'] > 0L)
                        {
                            fmt.Printf("import [%q] meth %v \n", p.imp.Path, n);
                        }
                    }


                    i = i__prev1;
                }

                dclcontext = savedContext;
            else if (i == arrayTag) 
                t = p.newtyp(TARRAY);
                var bound = p.int64();
                var elem = p.typ();
                t.Extra = ref new types.Array(Elem:elem,Bound:bound);
            else if (i == sliceTag) 
                t = p.newtyp(TSLICE);
                elem = p.typ();
                t.Extra = new types.Slice(Elem:elem);
            else if (i == dddTag) 
                t = p.newtyp(TDDDFIELD);
                t.Extra = new types.DDDField(T:p.typ());
            else if (i == structTag) 
                t = p.newtyp(TSTRUCT);
                t.SetFields(p.fieldList());
                checkwidth(t);
            else if (i == pointerTag) 
                t = p.newtyp(types.Tptr);
                t.Extra = new types.Ptr(Elem:p.typ());
            else if (i == signatureTag) 
                t = p.newtyp(TFUNC);
                @params = p.paramList();
                result = p.paramList();
                functypefield0(t, null, params, result);
            else if (i == interfaceTag) 
                {
                    var ml = p.methodList();

                    if (len(ml) == 0L)
                    {
                        t = types.Types[TINTER];
                    }
                    else
                    {
                        t = p.newtyp(TINTER);
                        t.SetInterface(ml);
                    }

                }
            else if (i == mapTag) 
                t = p.newtyp(TMAP);
                mt = t.MapType();
                mt.Key = p.typ();
                mt.Val = p.typ();
            else if (i == chanTag) 
                t = p.newtyp(TCHAN);
                var ct = t.ChanType();
                ct.Dir = types.ChanDir(p.@int());
                ct.Elem = p.typ();
            else 
                p.formatErrorf("unexpected type (tag = %d)", i);
                        if (t == null)
            {
                p.formatErrorf("nil type (type tag = %d)", i);
            }
            return t;
        }

        private static ref types.Sym qualifiedName(this ref importer p)
        {
            var name = p.@string();
            var pkg = p.pkg();
            return pkg.Lookup(name);
        }

        private static slice<ref types.Field> fieldList(this ref importer p)
        {
            {
                var n = p.@int();

                if (n > 0L)
                {
                    fields = make_slice<ref types.Field>(n);
                    foreach (var (i) in fields)
                    {
                        fields[i] = p.field();
                    }
                }

            }
            return;
        }

        private static ref types.Field field(this ref importer p)
        {
            var pos = p.pos();
            var (sym, alias) = p.fieldName();
            var typ = p.typ();
            var note = p.@string();

            var f = types.NewField();
            if (sym.Name == "")
            { 
                // anonymous field: typ must be T or *T and T must be a type name
                var s = typ.Sym;
                if (s == null && typ.IsPtr())
                {
                    s = typ.Elem().Sym; // deref
                }
                sym = sym.Pkg.Lookup(s.Name);
                f.Embedded = 1L;
            }
            else if (alias)
            { 
                // anonymous field: we have an explicit name because it's a type alias
                f.Embedded = 1L;
            }
            f.Sym = sym;
            f.Nname = asTypesNode(newnamel(pos, sym));
            f.Type = typ;
            f.Note = note;

            return f;
        }

        private static slice<ref types.Field> methodList(this ref importer p)
        {
            {
                var n__prev1 = n;

                for (var n = p.@int(); n > 0L; n--)
                {
                    var f = types.NewField();
                    f.Nname = asTypesNode(newname(nblank.Sym));
                    asNode(f.Nname).Pos;

                    p.pos();
                    f.Type = p.typ();
                    methods = append(methods, f);
                }


                n = n__prev1;
            }

            {
                var n__prev1 = n;

                for (n = p.@int(); n > 0L; n--)
                {
                    methods = append(methods, p.method());
                }


                n = n__prev1;
            }

            return;
        }

        private static ref types.Field method(this ref importer p)
        {
            var pos = p.pos();
            var sym = p.methodName();
            var @params = p.paramList();
            var result = p.paramList();

            var f = types.NewField();
            f.Sym = sym;
            f.Nname = asTypesNode(newnamel(pos, sym));
            f.Type = functypefield(fakeRecvField(), params, result);
            return f;
        }

        private static (ref types.Sym, bool) fieldName(this ref importer p)
        {
            var name = p.@string();
            if (p.version == 0L && name == "_")
            { 
                // version 0 didn't export a package for _ field names
                // but used the builtin package instead
                return (builtinpkg.Lookup(name), false);
            }
            var pkg = localpkg;
            var alias = false;

            if (name == "")
            {
                goto __switch_break0;
            }
            if (name == "?") 
            {
                // 2) field name matches base type name and is not exported: need package
                name = "";
                pkg = p.pkg();
                goto __switch_break0;
            }
            if (name == "@") 
            {
                // 3) field name doesn't match base type name (alias name): need name and possibly package
                name = p.@string();
                alias = true;
            }
            // default: 
                if (!exportname(name))
                {
                    pkg = p.pkg();
                }

            __switch_break0:;
            return (pkg.Lookup(name), alias);
        }

        private static ref types.Sym methodName(this ref importer p)
        {
            var name = p.@string();
            if (p.version == 0L && name == "_")
            { 
                // version 0 didn't export a package for _ method names
                // but used the builtin package instead
                return builtinpkg.Lookup(name);
            }
            var pkg = localpkg;
            if (!exportname(name))
            {
                pkg = p.pkg();
            }
            return pkg.Lookup(name);
        }

        private static slice<ref types.Field> paramList(this ref importer p)
        {
            var i = p.@int();
            if (i == 0L)
            {
                return null;
            } 
            // negative length indicates unnamed parameters
            var named = true;
            if (i < 0L)
            {
                i = -i;
                named = false;
            } 
            // i > 0
            var fs = make_slice<ref types.Field>(i);
            {
                var i__prev1 = i;

                foreach (var (__i) in fs)
                {
                    i = __i;
                    fs[i] = p.param(named);
                }

                i = i__prev1;
            }

            return fs;
        }

        private static ref types.Field param(this ref importer p, bool named)
        {
            var f = types.NewField();
            f.Type = p.typ();
            if (f.Type.Etype == TDDDFIELD)
            { 
                // TDDDFIELD indicates wrapped ... slice type
                f.Type = types.NewSlice(f.Type.DDDField());
                f.SetIsddd(true);
            }
            if (named)
            {
                var name = p.@string();
                if (name == "")
                {
                    p.formatErrorf("expected named parameter");
                } 
                // TODO(gri) Supply function/method package rather than
                // encoding the package for each parameter repeatedly.
                var pkg = localpkg;
                if (name != "_")
                {
                    pkg = p.pkg();
                }
                f.Sym = pkg.Lookup(name);
                f.Nname = asTypesNode(newname(f.Sym));
            } 

            // TODO(gri) This is compiler-specific (escape info).
            // Move into compiler-specific section eventually?
            f.Note = p.@string();

            return f;
        }

        private static Val value(this ref importer p, ref types.Type typ)
        {
            {
                var tag = p.tagOrIndex();


                if (tag == falseTag) 
                    x.U = false;
                else if (tag == trueTag) 
                    x.U = true;
                else if (tag == int64Tag) 
                    ptr<object> u = @new<Mpint>();
                    u.SetInt64(p.int64());
                    u.Rune = typ == types.Idealrune;
                    x.U = u;
                else if (tag == floatTag) 
                    var f = newMpflt();
                    p.@float(f);
                    if (typ == types.Idealint || typ.IsInteger())
                    { 
                        // uncommon case: large int encoded as float
                        u = @new<Mpint>();
                        u.SetFloat(f);
                        x.U = u;
                        break;
                    }
                    x.U = f;
                else if (tag == complexTag) 
                    u = @new<Mpcplx>();
                    p.@float(ref u.Real);
                    p.@float(ref u.Imag);
                    x.U = u;
                else if (tag == stringTag) 
                    x.U = p.@string();
                else if (tag == unknownTag) 
                    p.formatErrorf("unknown constant (importing package with errors)");
                else if (tag == nilTag) 
                    x.U = @new<NilVal>();
                else 
                    p.formatErrorf("unexpected value tag %d", tag);

            } 

            // verify ideal type
            if (typ.IsUntyped() && untype(x.Ctype()) != typ)
            {
                p.formatErrorf("value %v and type %v don't match", x, typ);
            }
            return;
        }

        private static void @float(this ref importer p, ref Mpflt x)
        {
            var sign = p.@int();
            if (sign == 0L)
            {
                x.SetFloat64(0L);
                return;
            }
            var exp = p.@int();
            ptr<object> mant = @new<big.Int>().SetBytes((slice<byte>)p.@string());

            var m = x.Val.SetInt(mant);
            m.SetMantExp(m, exp - mant.BitLen());
            if (sign < 0L)
            {
                m.Neg(m);
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

        private static slice<ref Node> stmtList(this ref importer p)
        {
            slice<ref Node> list = default;
            while (true)
            {
                var n = p.node();
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

        private static slice<ref Node> exprList(this ref importer p)
        {
            slice<ref Node> list = default;
            while (true)
            {
                var n = p.expr();
                if (n == null)
                {
                    break;
                }
                list = append(list, n);
            }

            return list;
        }

        private static slice<ref Node> elemList(this ref importer p)
        {
            var c = p.@int();
            var list = make_slice<ref Node>(c);
            foreach (var (i) in list)
            {
                var s = p.fieldSym();
                list[i] = nodSym(OSTRUCTKEY, p.expr(), s);
            }
            return list;
        }

        private static ref Node expr(this ref importer p)
        {
            var n = p.node();
            if (n != null && n.Op == OBLOCK)
            {
                Fatalf("unexpected block node: %v", n);
            }
            return n;
        }

        private static ref Node npos(src.XPos pos, ref Node n)
        {
            n.Pos = pos;
            return n;
        }

        // TODO(gri) split into expr and stmt
        private static ref Node node(this ref importer _p) => func(_p, (ref importer p, Defer _, Panic panic, Recover __) =>
        {
            {
                var op = p.op();


                // expressions
                // case OPAREN:
                //     unreachable - unpacked by exporter

                // case ODDDARG:
                //    unimplemented

                if (op == OLITERAL) 
                    var pos = p.pos();
                    var typ = p.typ();
                    var n = npos(pos, nodlit(p.value(typ)));
                    if (!typ.IsUntyped())
                    { 
                        // Type-checking simplifies unsafe.Pointer(uintptr(c))
                        // to unsafe.Pointer(c) which then cannot type-checked
                        // again. Re-introduce explicit uintptr(c) conversion.
                        // (issue 16317).
                        if (typ.IsUnsafePtr())
                        {
                            n = nodl(pos, OCONV, n, null);
                            n.Type = types.Types[TUINTPTR];
                        }
                        n = nodl(pos, OCONV, n, null);
                        n.Type = typ;
                    }
                    return n;
                else if (op == ONAME) 
                    return npos(p.pos(), mkname(p.sym())); 

                    // case OPACK, ONONAME:
                    //     unreachable - should have been resolved by typechecking
                else if (op == OTYPE) 
                    return npos(p.pos(), typenod(p.typ())); 

                    // case OTARRAY, OTMAP, OTCHAN, OTSTRUCT, OTINTER, OTFUNC:
                    //      unreachable - should have been resolved by typechecking

                    // case OCLOSURE:
                    //    unimplemented
                else if (op == OPTRLIT) 
                    pos = p.pos();
                    n = npos(pos, p.expr());
                    if (!p.@bool())
                    {
                        if (n.Op == OCOMPLIT)
                        { 
                            // Special case for &T{...}: turn into (*T){...}.
                            n.Right = nodl(pos, OIND, n.Right, null);
                            n.Right.SetImplicit(true);
                        }
                        else
                        {
                            n = nodl(pos, OADDR, n, null);
                        }
                    }
                    return n;
                else if (op == OSTRUCTLIT) 
                    // TODO(mdempsky): Export position information for OSTRUCTKEY nodes.
                    var savedlineno = lineno;
                    lineno = p.pos();
                    n = nodl(lineno, OCOMPLIT, null, typenod(p.typ()));
                    n.List.Set(p.elemList()); // special handling of field names
                    lineno = savedlineno;
                    return n; 

                    // case OARRAYLIT, OSLICELIT, OMAPLIT:
                    //     unreachable - mapped to case OCOMPLIT below by exporter
                else if (op == OCOMPLIT) 
                    n = nodl(p.pos(), OCOMPLIT, null, typenod(p.typ()));
                    n.List.Set(p.exprList());
                    return n;
                else if (op == OKEY) 
                    pos = p.pos();
                    var (left, right) = p.exprsOrNil();
                    return nodl(pos, OKEY, left, right); 

                    // case OSTRUCTKEY:
                    //    unreachable - handled in case OSTRUCTLIT by elemList

                    // case OCALLPART:
                    //    unimplemented

                    // case OXDOT, ODOT, ODOTPTR, ODOTINTER, ODOTMETH:
                    //     unreachable - mapped to case OXDOT below by exporter
                else if (op == OXDOT) 
                    // see parser.new_dotname
                    return npos(p.pos(), nodSym(OXDOT, p.expr(), p.fieldSym())); 

                    // case ODOTTYPE, ODOTTYPE2:
                    //     unreachable - mapped to case ODOTTYPE below by exporter
                else if (op == ODOTTYPE) 
                    n = nodl(p.pos(), ODOTTYPE, p.expr(), null);
                    n.Type = p.typ();
                    return n; 

                    // case OINDEX, OINDEXMAP, OSLICE, OSLICESTR, OSLICEARR, OSLICE3, OSLICE3ARR:
                    //     unreachable - mapped to cases below by exporter
                else if (op == OINDEX) 
                    return nodl(p.pos(), op, p.expr(), p.expr());
                else if (op == OSLICE || op == OSLICE3) 
                    n = nodl(p.pos(), op, p.expr(), null);
                    var (low, high) = p.exprsOrNil();
                    ref Node max = default;
                    if (n.Op.IsSlice3())
                    {
                        max = p.expr();
                    }
                    n.SetSliceBounds(low, high, max);
                    return n; 

                    // case OCONV, OCONVIFACE, OCONVNOP, OARRAYBYTESTR, OARRAYRUNESTR, OSTRARRAYBYTE, OSTRARRAYRUNE, ORUNESTR:
                    //     unreachable - mapped to OCONV case below by exporter
                else if (op == OCONV) 
                    n = nodl(p.pos(), OCONV, p.expr(), null);
                    n.Type = p.typ();
                    return n;
                else if (op == OCOPY || op == OCOMPLEX || op == OREAL || op == OIMAG || op == OAPPEND || op == OCAP || op == OCLOSE || op == ODELETE || op == OLEN || op == OMAKE || op == ONEW || op == OPANIC || op == ORECOVER || op == OPRINT || op == OPRINTN) 
                    n = npos(p.pos(), builtinCall(op));
                    n.List.Set(p.exprList());
                    if (op == OAPPEND)
                    {
                        n.SetIsddd(p.@bool());
                    }
                    return n; 

                    // case OCALL, OCALLFUNC, OCALLMETH, OCALLINTER, OGETG:
                    //     unreachable - mapped to OCALL case below by exporter
                else if (op == OCALL) 
                    n = nodl(p.pos(), OCALL, p.expr(), null);
                    n.List.Set(p.exprList());
                    n.SetIsddd(p.@bool());
                    return n;
                else if (op == OMAKEMAP || op == OMAKECHAN || op == OMAKESLICE) 
                    n = npos(p.pos(), builtinCall(OMAKE));
                    n.List.Append(typenod(p.typ()));
                    n.List.Append(p.exprList());
                    return n; 

                    // unary expressions
                else if (op == OPLUS || op == OMINUS || op == OADDR || op == OCOM || op == OIND || op == ONOT || op == ORECV) 
                    return nodl(p.pos(), op, p.expr(), null); 

                    // binary expressions
                else if (op == OADD || op == OAND || op == OANDAND || op == OANDNOT || op == ODIV || op == OEQ || op == OGE || op == OGT || op == OLE || op == OLT || op == OLSH || op == OMOD || op == OMUL || op == ONE || op == OOR || op == OOROR || op == ORSH || op == OSEND || op == OSUB || op == OXOR) 
                    return nodl(p.pos(), op, p.expr(), p.expr());
                else if (op == OADDSTR) 
                    pos = p.pos();
                    var list = p.exprList();
                    var x = npos(pos, list[0L]);
                    foreach (var (_, y) in list[1L..])
                    {
                        x = nodl(pos, OADD, x, y);
                    }
                    return x; 

                    // case OCMPSTR, OCMPIFACE:
                    //     unreachable - mapped to std comparison operators by exporter
                else if (op == ODCLCONST) 
                    // TODO(gri) these should not be exported in the first place
                    return nodl(p.pos(), OEMPTY, null, null); 

                    // --------------------------------------------------------------------
                    // statements
                else if (op == ODCL) 
                    if (p.version < 2L)
                    { 
                        // versions 0 and 1 exported a bool here but it
                        // was always false - simply ignore in this case
                        p.@bool();
                    }
                    pos = p.pos();
                    var lhs = dclname(p.sym());
                    typ = typenod(p.typ());
                    return npos(pos, liststmt(variter(new slice<ref Node>(new ref Node[] { lhs }), typ, null))); // TODO(gri) avoid list creation

                    // case ODCLFIELD:
                    //    unimplemented

                    // case OAS, OASWB:
                    //     unreachable - mapped to OAS case below by exporter
                else if (op == OAS) 
                    return nodl(p.pos(), OAS, p.expr(), p.expr());
                else if (op == OASOP) 
                    n = nodl(p.pos(), OASOP, null, null);
                    n.Etype = types.EType(p.@int());
                    n.Left = p.expr();
                    if (!p.@bool())
                    {
                        n.Right = nodintconst(1L);
                        n.SetImplicit(true);
                    }
                    else
                    {
                        n.Right = p.expr();
                    }
                    return n; 

                    // case OAS2DOTTYPE, OAS2FUNC, OAS2MAPR, OAS2RECV:
                    //     unreachable - mapped to OAS2 case below by exporter
                else if (op == OAS2) 
                    n = nodl(p.pos(), OAS2, null, null);
                    n.List.Set(p.exprList());
                    n.Rlist.Set(p.exprList());
                    return n;
                else if (op == ORETURN) 
                    n = nodl(p.pos(), ORETURN, null, null);
                    n.List.Set(p.exprList());
                    return n; 

                    // case ORETJMP:
                    //     unreachable - generated by compiler for trampolin routines (not exported)
                else if (op == OPROC || op == ODEFER) 
                    return nodl(p.pos(), op, p.expr(), null);
                else if (op == OIF) 
                    n = nodl(p.pos(), OIF, null, null);
                    n.Ninit.Set(p.stmtList());
                    n.Left = p.expr();
                    n.Nbody.Set(p.stmtList());
                    n.Rlist.Set(p.stmtList());
                    return n;
                else if (op == OFOR) 
                    n = nodl(p.pos(), OFOR, null, null);
                    n.Ninit.Set(p.stmtList());
                    n.Left, n.Right = p.exprsOrNil();
                    n.Nbody.Set(p.stmtList());
                    return n;
                else if (op == ORANGE) 
                    n = nodl(p.pos(), ORANGE, null, null);
                    n.List.Set(p.stmtList());
                    n.Right = p.expr();
                    n.Nbody.Set(p.stmtList());
                    return n;
                else if (op == OSELECT || op == OSWITCH) 
                    n = nodl(p.pos(), op, null, null);
                    n.Ninit.Set(p.stmtList());
                    n.Left, _ = p.exprsOrNil();
                    n.List.Set(p.stmtList());
                    return n; 

                    // case OCASE, OXCASE:
                    //     unreachable - mapped to OXCASE case below by exporter
                else if (op == OXCASE) 
                    n = nodl(p.pos(), OXCASE, null, null);
                    n.List.Set(p.exprList()); 
                    // TODO(gri) eventually we must declare variables for type switch
                    // statements (type switch statements are not yet exported)
                    n.Nbody.Set(p.stmtList());
                    return n; 

                    // case OFALL:
                    //     unreachable - mapped to OXFALL case below by exporter
                else if (op == OFALL) 
                    n = nodl(p.pos(), OFALL, null, null);
                    return n;
                else if (op == OBREAK || op == OCONTINUE) 
                    pos = p.pos();
                    var (left, _) = p.exprsOrNil();
                    if (left != null)
                    {
                        left = newname(left.Sym);
                    }
                    return nodl(pos, op, left, null); 

                    // case OEMPTY:
                    //     unreachable - not emitted by exporter
                else if (op == OGOTO || op == OLABEL) 
                    return nodl(p.pos(), op, newname(p.expr().Sym), null);
                else if (op == OEND) 
                    return null;
                else 
                    Fatalf("cannot import %v (%d) node\n" + "==> please file an issue and assign to gri@\n", op, int(op));
                    panic("unreachable"); // satisfy compiler

            }
        });

        private static ref Node builtinCall(Op op)
        {
            return nod(OCALL, mkname(builtinpkg.Lookup(goopnames[op])), null);
        }

        private static (ref Node, ref Node) exprsOrNil(this ref importer p)
        {
            var ab = p.@int();
            if (ab & 1L != 0L)
            {
                a = p.expr();
            }
            if (ab & 2L != 0L)
            {
                b = p.expr();
            }
            return;
        }

        private static ref types.Sym fieldSym(this ref importer p)
        {
            var name = p.@string();
            var pkg = localpkg;
            if (!exportname(name))
            {
                pkg = p.pkg();
            }
            return pkg.Lookup(name);
        }

        private static ref types.Sym sym(this ref importer p)
        {
            var name = p.@string();
            var pkg = localpkg;
            if (name != "_")
            {
                pkg = p.pkg();
            }
            var linkname = p.@string();
            var sym = pkg.Lookup(name);
            sym.Linkname = linkname;
            return sym;
        }

        private static bool @bool(this ref importer p)
        {
            return p.@int() != 0L;
        }

        private static Op op(this ref importer p)
        {
            return Op(p.@int());
        }

        // ----------------------------------------------------------------------------
        // Low-level decoders

        private static long tagOrIndex(this ref importer p)
        {
            if (p.debugFormat)
            {
                p.marker('t');
            }
            return int(p.rawInt64());
        }

        private static long @int(this ref importer p)
        {
            var x = p.int64();
            if (int64(int(x)) != x)
            {
                p.formatErrorf("exported integer too large");
            }
            return int(x);
        }

        private static long int64(this ref importer p)
        {
            if (p.debugFormat)
            {
                p.marker('i');
            }
            return p.rawInt64();
        }

        private static @string @string(this ref importer p)
        {
            if (p.debugFormat)
            {
                p.marker('s');
            } 
            // if the string was seen before, i is its index (>= 0)
            // (the empty string is at index 0)
            var i = p.rawInt64();
            if (i >= 0L)
            {
                return p.strList[i];
            } 
            // otherwise, i is the negative string length (< 0)
            {
                var n = int(-i);

                if (n <= cap(p.buf))
                {
                    p.buf = p.buf[..n];
                }
                else
                {
                    p.buf = make_slice<byte>(n);
                }

            }
            {
                var i__prev1 = i;

                foreach (var (__i) in p.buf)
                {
                    i = __i;
                    p.buf[i] = p.rawByte();
                }

                i = i__prev1;
            }

            var s = string(p.buf);
            p.strList = append(p.strList, s);
            return s;
        }

        private static void marker(this ref importer p, byte want)
        {
            {
                var got = p.rawByte();

                if (got != want)
                {
                    p.formatErrorf("incorrect marker: got %c; want %c (pos = %d)", got, want, p.read);
                }

            }

            var pos = p.read;
            {
                var n = int(p.rawInt64());

                if (n != pos)
                {
                    p.formatErrorf("incorrect position: got %d; want %d", n, pos);
                }

            }
        }

        // rawInt64 should only be used by low-level decoders.
        private static long rawInt64(this ref importer p)
        {
            var (i, err) = binary.ReadVarint(p);
            if (err != null)
            {
                p.formatErrorf("read error: %v", err);
            }
            return i;
        }

        // rawStringln should only be used to read the initial version string.
        private static @string rawStringln(this ref importer p, byte b)
        {
            p.buf = p.buf[..0L];
            while (b != '\n')
            {
                p.buf = append(p.buf, b);
                b = p.rawByte();
            }

            return string(p.buf);
        }

        // needed for binary.ReadVarint in rawInt64
        private static (byte, error) ReadByte(this ref importer p)
        {
            return (p.rawByte(), null);
        }

        // rawByte is the bottleneck interface for reading from p.in.
        // It unescapes '|' 'S' to '$' and '|' '|' to '|'.
        // rawByte should only be used by low-level decoders.
        private static byte rawByte(this ref importer p)
        {
            var (c, err) = p.@in.ReadByte();
            p.read++;
            if (err != null)
            {
                p.formatErrorf("read error: %v", err);
            }
            if (c == '|')
            {
                c, err = p.@in.ReadByte();
                p.read++;
                if (err != null)
                {
                    p.formatErrorf("read error: %v", err);
                }
                switch (c)
                {
                    case 'S': 
                        c = '$';
                        break;
                    case '|': 
                        break;
                    default: 
                        p.formatErrorf("unexpected escape sequence in export data");
                        break;
                }
            }
            return c;
        }
    }
}}}}
