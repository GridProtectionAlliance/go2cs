// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Indexed package export.
//
// The indexed export data format is an evolution of the previous
// binary export data format. Its chief contribution is introducing an
// index table, which allows efficient random access of individual
// declarations and inline function bodies. In turn, this allows
// avoiding unnecessary work for compilation units that import large
// packages.
//
//
// The top-level data format is structured as:
//
//     Header struct {
//         Tag        byte   // 'i'
//         Version    uvarint
//         StringSize uvarint
//         DataSize   uvarint
//     }
//
//     Strings [StringSize]byte
//     Data    [DataSize]byte
//
//     MainIndex []struct{
//         PkgPath   stringOff
//         PkgName   stringOff
//         PkgHeight uvarint
//
//         Decls []struct{
//             Name   stringOff
//             Offset declOff
//         }
//     }
//
//     Fingerprint [8]byte
//
// uvarint means a uint64 written out using uvarint encoding.
//
// []T means a uvarint followed by that many T objects. In other
// words:
//
//     Len   uvarint
//     Elems [Len]T
//
// stringOff means a uvarint that indicates an offset within the
// Strings section. At that offset is another uvarint, followed by
// that many bytes, which form the string value.
//
// declOff means a uvarint that indicates an offset within the Data
// section where the associated declaration can be found.
//
//
// There are five kinds of declarations, distinguished by their first
// byte:
//
//     type Var struct {
//         Tag  byte // 'V'
//         Pos  Pos
//         Type typeOff
//     }
//
//     type Func struct {
//         Tag       byte // 'F'
//         Pos       Pos
//         Signature Signature
//     }
//
//     type Const struct {
//         Tag   byte // 'C'
//         Pos   Pos
//         Value Value
//     }
//
//     type Type struct {
//         Tag        byte // 'T'
//         Pos        Pos
//         Underlying typeOff
//
//         Methods []struct{  // omitted if Underlying is an interface type
//             Pos       Pos
//             Name      stringOff
//             Recv      Param
//             Signature Signature
//         }
//     }
//
//     type Alias struct {
//         Tag  byte // 'A'
//         Pos  Pos
//         Type typeOff
//     }
//
//
// typeOff means a uvarint that either indicates a predeclared type,
// or an offset into the Data section. If the uvarint is less than
// predeclReserved, then it indicates the index into the predeclared
// types list (see predeclared in bexport.go for order). Otherwise,
// subtracting predeclReserved yields the offset of a type descriptor.
//
// Value means a type and type-specific value. See
// (*exportWriter).value for details.
//
//
// There are nine kinds of type descriptors, distinguished by an itag:
//
//     type DefinedType struct {
//         Tag     itag // definedType
//         Name    stringOff
//         PkgPath stringOff
//     }
//
//     type PointerType struct {
//         Tag  itag // pointerType
//         Elem typeOff
//     }
//
//     type SliceType struct {
//         Tag  itag // sliceType
//         Elem typeOff
//     }
//
//     type ArrayType struct {
//         Tag  itag // arrayType
//         Len  uint64
//         Elem typeOff
//     }
//
//     type ChanType struct {
//         Tag  itag   // chanType
//         Dir  uint64 // 1 RecvOnly; 2 SendOnly; 3 SendRecv
//         Elem typeOff
//     }
//
//     type MapType struct {
//         Tag  itag // mapType
//         Key  typeOff
//         Elem typeOff
//     }
//
//     type FuncType struct {
//         Tag       itag // signatureType
//         PkgPath   stringOff
//         Signature Signature
//     }
//
//     type StructType struct {
//         Tag     itag // structType
//         PkgPath stringOff
//         Fields []struct {
//             Pos      Pos
//             Name     stringOff
//             Type     typeOff
//             Embedded bool
//             Note     stringOff
//         }
//     }
//
//     type InterfaceType struct {
//         Tag     itag // interfaceType
//         PkgPath stringOff
//         Embeddeds []struct {
//             Pos  Pos
//             Type typeOff
//         }
//         Methods []struct {
//             Pos       Pos
//             Name      stringOff
//             Signature Signature
//         }
//     }
//
//
//     type Signature struct {
//         Params   []Param
//         Results  []Param
//         Variadic bool  // omitted if Results is empty
//     }
//
//     type Param struct {
//         Pos  Pos
//         Name stringOff
//         Type typOff
//     }
//
//
// Pos encodes a file:line:column triple, incorporating a simple delta
// encoding scheme within a data object. See exportWriter.pos for
// details.
//
//
// Compiler-specific details.
//
// cmd/compile writes out a second index for inline bodies and also
// appends additional compiler-specific details after declarations.
// Third-party tools are not expected to depend on these details and
// they're expected to change much more rapidly, so they're omitted
// here. See exportWriter's varExt/funcExt/etc methods for details.

// package gc -- go2cs converted at 2020 October 08 04:29:08 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\iexport.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using types = go.cmd.compile.@internal.types_package;
using goobj2 = go.cmd.@internal.goobj2_package;
using src = go.cmd.@internal.src_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using io = go.io_package;
using big = go.math.big_package;
using sort = go.sort_package;
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
        // Current indexed export format version. Increase with each format change.
        // 1: added column details to Pos
        // 0: Go1.11 encoding
        private static readonly long iexportVersion = (long)1L;

        // predeclReserved is the number of type offsets reserved for types
        // implicitly declared in the universe block.


        // predeclReserved is the number of type offsets reserved for types
        // implicitly declared in the universe block.
        private static readonly long predeclReserved = (long)32L;

        // An itag distinguishes the kind of type that was written into the
        // indexed export format.


        // An itag distinguishes the kind of type that was written into the
        // indexed export format.
        private partial struct itag // : ulong
        {
        }

 
        // Types
        private static readonly itag definedType = (itag)iota;
        private static readonly var pointerType = (var)0;
        private static readonly var sliceType = (var)1;
        private static readonly var arrayType = (var)2;
        private static readonly var chanType = (var)3;
        private static readonly var mapType = (var)4;
        private static readonly var signatureType = (var)5;
        private static readonly var structType = (var)6;
        private static readonly var interfaceType = (var)7;


        private static void iexport(ptr<bufio.Writer> _addr_@out)
        {
            ref bufio.Writer @out = ref _addr_@out.val;
 
            // Mark inline bodies that are reachable through exported types.
            // (Phase 0 of bexport.go.)
            { 
                // TODO(mdempsky): Separate from bexport logic.
                ptr<exporter> p = addr(new exporter(marked:make(map[*types.Type]bool)));
                {
                    var n__prev1 = n;

                    foreach (var (_, __n) in exportlist)
                    {
                        n = __n;
                        var sym = n.Sym;
                        p.markType(asNode(sym.Def).Type);
                    }

                    n = n__prev1;
                }
            }
            p = new iexporter(allPkgs:map[*types.Pkg]bool{},stringIndex:map[string]uint64{},declIndex:map[*Node]uint64{},inlineIndex:map[*Node]uint64{},typIndex:map[*types.Type]uint64{},);

            foreach (var (i, pt) in predeclared())
            {
                p.typIndex[pt] = uint64(i);
            }
            if (len(p.typIndex) > predeclReserved)
            {
                Fatalf("too many predeclared types: %d > %d", len(p.typIndex), predeclReserved);
            } 

            // Initialize work queue with exported declarations.
            {
                var n__prev1 = n;

                foreach (var (_, __n) in exportlist)
                {
                    n = __n;
                    p.pushDecl(n);
                } 

                // Loop until no more work. We use a queue because while
                // writing out inline bodies, we may discover additional
                // declarations that are needed.

                n = n__prev1;
            }

            while (!p.declTodo.empty())
            {
                p.doDecl(p.declTodo.popLeft());
            } 

            // Append indices to data0 section.
 

            // Append indices to data0 section.
            var dataLen = uint64(p.data0.Len());
            var w = p.newWriter();
            w.writeIndex(p.declIndex, true);
            w.writeIndex(p.inlineIndex, false);
            w.flush(); 

            // Assemble header.
            ref intWriter hdr = ref heap(out ptr<intWriter> _addr_hdr);
            hdr.WriteByte('i');
            hdr.uint64(iexportVersion);
            hdr.uint64(uint64(p.strings.Len()));
            hdr.uint64(dataLen); 

            // Flush output.
            io.Copy(out, _addr_hdr);
            io.Copy(out, _addr_p.strings);
            io.Copy(out, _addr_p.data0); 

            // Add fingerprint (used by linker object file).
            // Attach this to the end, so tools (e.g. gcimporter) don't care.
            @out.Write(Ctxt.Fingerprint[..]);

        }

        // writeIndex writes out an object index. mainIndex indicates whether
        // we're writing out the main index, which is also read by
        // non-compiler tools and includes a complete package description
        // (i.e., name and height).
        private static void writeIndex(this ptr<exportWriter> _addr_w, map<ptr<Node>, ulong> index, bool mainIndex)
        {
            ref exportWriter w = ref _addr_w.val;
 
            // Build a map from packages to objects from that package.
            map pkgObjs = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<types.Pkg>, slice<ptr<Node>>>{}; 

            // For the main index, make sure to include every package that
            // we reference, even if we're not exporting (or reexporting)
            // any symbols from it.
            if (mainIndex)
            {
                pkgObjs[localpkg] = null;
                {
                    var pkg__prev1 = pkg;

                    foreach (var (__pkg) in w.p.allPkgs)
                    {
                        pkg = __pkg;
                        pkgObjs[pkg] = null;
                    }

                    pkg = pkg__prev1;
                }
            }

            {
                var n__prev1 = n;

                foreach (var (__n) in index)
                {
                    n = __n;
                    pkgObjs[n.Sym.Pkg] = append(pkgObjs[n.Sym.Pkg], n);
                }

                n = n__prev1;
            }

            slice<ptr<types.Pkg>> pkgs = default;
            {
                var pkg__prev1 = pkg;
                var objs__prev1 = objs;

                foreach (var (__pkg, __objs) in pkgObjs)
                {
                    pkg = __pkg;
                    objs = __objs;
                    pkgs = append(pkgs, pkg);

                    sort.Slice(objs, (i, j) =>
                    {
                        return objs[i].Sym.Name < objs[j].Sym.Name;
                    });

                }

                pkg = pkg__prev1;
                objs = objs__prev1;
            }

            sort.Slice(pkgs, (i, j) =>
            {
                return pkgs[i].Path < pkgs[j].Path;
            });

            w.uint64(uint64(len(pkgs)));
            {
                var pkg__prev1 = pkg;

                foreach (var (_, __pkg) in pkgs)
                {
                    pkg = __pkg;
                    w.@string(pkg.Path);
                    if (mainIndex)
                    {
                        w.@string(pkg.Name);
                        w.uint64(uint64(pkg.Height));
                    }

                    var objs = pkgObjs[pkg];
                    w.uint64(uint64(len(objs)));
                    {
                        var n__prev2 = n;

                        foreach (var (_, __n) in objs)
                        {
                            n = __n;
                            w.@string(n.Sym.Name);
                            w.uint64(index[n]);
                        }

                        n = n__prev2;
                    }
                }

                pkg = pkg__prev1;
            }
        }

        private partial struct iexporter
        {
            public map<ptr<types.Pkg>, bool> allPkgs;
            public nodeQueue declTodo;
            public intWriter strings;
            public map<@string, ulong> stringIndex;
            public intWriter data0;
            public map<ptr<Node>, ulong> declIndex;
            public map<ptr<Node>, ulong> inlineIndex;
            public map<ptr<types.Type>, ulong> typIndex;
        }

        // stringOff returns the offset of s within the string section.
        // If not already present, it's added to the end.
        private static ulong stringOff(this ptr<iexporter> _addr_p, @string s)
        {
            ref iexporter p = ref _addr_p.val;

            var (off, ok) = p.stringIndex[s];
            if (!ok)
            {
                off = uint64(p.strings.Len());
                p.stringIndex[s] = off;

                p.strings.uint64(uint64(len(s)));
                p.strings.WriteString(s);
            }

            return off;

        }

        // pushDecl adds n to the declaration work queue, if not already present.
        private static void pushDecl(this ptr<iexporter> _addr_p, ptr<Node> _addr_n)
        {
            ref iexporter p = ref _addr_p.val;
            ref Node n = ref _addr_n.val;

            if (n.Sym == null || asNode(n.Sym.Def) != n && n.Op != OTYPE)
            {
                Fatalf("weird Sym: %v, %v", n, n.Sym);
            } 

            // Don't export predeclared declarations.
            if (n.Sym.Pkg == builtinpkg || n.Sym.Pkg == unsafepkg)
            {
                return ;
            }

            {
                var (_, ok) = p.declIndex[n];

                if (ok)
                {
                    return ;
                }

            }


            p.declIndex[n] = ~uint64(0L); // mark n present in work queue
            p.declTodo.pushRight(n);

        }

        // exportWriter handles writing out individual data section chunks.
        private partial struct exportWriter
        {
            public ptr<iexporter> p;
            public intWriter data;
            public ptr<types.Pkg> currPkg;
            public @string prevFile;
            public long prevLine;
            public long prevColumn;
        }

        private static void doDecl(this ptr<iexporter> _addr_p, ptr<Node> _addr_n)
        {
            ref iexporter p = ref _addr_p.val;
            ref Node n = ref _addr_n.val;

            var w = p.newWriter();
            w.setPkg(n.Sym.Pkg, false);


            if (n.Op == ONAME) 

                if (n.Class() == PEXTERN) 
                    // Variable.
                    w.tag('V');
                    w.pos(n.Pos);
                    w.typ(n.Type);
                    w.varExt(n);
                else if (n.Class() == PFUNC) 
                    if (n.IsMethod())
                    {
                        Fatalf("unexpected method: %v", n);
                    } 

                    // Function.
                    w.tag('F');
                    w.pos(n.Pos);
                    w.signature(n.Type);
                    w.funcExt(n);
                else 
                    Fatalf("unexpected class: %v, %v", n, n.Class());
                            else if (n.Op == OLITERAL) 
                // Constant.
                n = typecheck(n, ctxExpr);
                w.tag('C');
                w.pos(n.Pos);
                w.value(n.Type, n.Val());
            else if (n.Op == OTYPE) 
                if (IsAlias(n.Sym))
                { 
                    // Alias.
                    w.tag('A');
                    w.pos(n.Pos);
                    w.typ(n.Type);
                    break;

                } 

                // Defined type.
                w.tag('T');
                w.pos(n.Pos);

                var underlying = n.Type.Orig;
                if (underlying == types.Errortype.Orig)
                { 
                    // For "type T error", use error as the
                    // underlying type instead of error's own
                    // underlying anonymous interface. This
                    // ensures consistency with how importers may
                    // declare error (e.g., go/types uses nil Pkg
                    // for predeclared objects).
                    underlying = types.Errortype;

                }

                w.typ(underlying);

                var t = n.Type;
                if (t.IsInterface())
                {
                    break;
                }

                var ms = t.Methods();
                w.uint64(uint64(ms.Len()));
                {
                    var m__prev1 = m;

                    foreach (var (_, __m) in ms.Slice())
                    {
                        m = __m;
                        w.pos(m.Pos);
                        w.selector(m.Sym);
                        w.param(m.Type.Recv());
                        w.signature(m.Type);
                    }

                    m = m__prev1;
                }

                {
                    var m__prev1 = m;

                    foreach (var (_, __m) in ms.Slice())
                    {
                        m = __m;
                        w.methExt(m);
                    }

                    m = m__prev1;
                }
            else 
                Fatalf("unexpected node: %v", n);
                        p.declIndex[n] = w.flush();

        }

        private static void tag(this ptr<exportWriter> _addr_w, byte tag)
        {
            ref exportWriter w = ref _addr_w.val;

            w.data.WriteByte(tag);
        }

        private static void doInline(this ptr<iexporter> _addr_p, ptr<Node> _addr_f)
        {
            ref iexporter p = ref _addr_p.val;
            ref Node f = ref _addr_f.val;

            var w = p.newWriter();
            w.setPkg(fnpkg(f), false);

            w.stmtList(asNodes(f.Func.Inl.Body));

            p.inlineIndex[f] = w.flush();
        }

        private static void pos(this ptr<exportWriter> _addr_w, src.XPos pos)
        {
            ref exportWriter w = ref _addr_w.val;

            var p = Ctxt.PosTable.Pos(pos);
            var file = p.Base().AbsFilename();
            var line = int64(p.RelLine());
            var column = int64(p.RelCol()); 

            // Encode position relative to the last position: column
            // delta, then line delta, then file name. We reserve the
            // bottom bit of the column and line deltas to encode whether
            // the remaining fields are present.
            //
            // Note: Because data objects may be read out of order (or not
            // at all), we can only apply delta encoding within a single
            // object. This is handled implicitly by tracking prevFile,
            // prevLine, and prevColumn as fields of exportWriter.

            var deltaColumn = (column - w.prevColumn) << (int)(1L);
            var deltaLine = (line - w.prevLine) << (int)(1L);

            if (file != w.prevFile)
            {
                deltaLine |= 1L;
            }

            if (deltaLine != 0L)
            {
                deltaColumn |= 1L;
            }

            w.int64(deltaColumn);
            if (deltaColumn & 1L != 0L)
            {
                w.int64(deltaLine);
                if (deltaLine & 1L != 0L)
                {
                    w.@string(file);
                }

            }

            w.prevFile = file;
            w.prevLine = line;
            w.prevColumn = column;

        }

        private static void pkg(this ptr<exportWriter> _addr_w, ptr<types.Pkg> _addr_pkg)
        {
            ref exportWriter w = ref _addr_w.val;
            ref types.Pkg pkg = ref _addr_pkg.val;
 
            // Ensure any referenced packages are declared in the main index.
            w.p.allPkgs[pkg] = true;

            w.@string(pkg.Path);

        }

        private static void qualifiedIdent(this ptr<exportWriter> _addr_w, ptr<Node> _addr_n)
        {
            ref exportWriter w = ref _addr_w.val;
            ref Node n = ref _addr_n.val;
 
            // Ensure any referenced declarations are written out too.
            w.p.pushDecl(n);

            var s = n.Sym;
            w.@string(s.Name);
            w.pkg(s.Pkg);

        }

        private static void selector(this ptr<exportWriter> _addr_w, ptr<types.Sym> _addr_s)
        {
            ref exportWriter w = ref _addr_w.val;
            ref types.Sym s = ref _addr_s.val;

            if (w.currPkg == null)
            {
                Fatalf("missing currPkg");
            } 

            // Method selectors are rewritten into method symbols (of the
            // form T.M) during typechecking, but we want to write out
            // just the bare method name.
            var name = s.Name;
            {
                var i = strings.LastIndex(name, ".");

                if (i >= 0L)
                {
                    name = name[i + 1L..];
                }
                else
                {
                    var pkg = w.currPkg;
                    if (types.IsExported(name))
                    {
                        pkg = localpkg;
                    }

                    if (s.Pkg != pkg)
                    {
                        Fatalf("package mismatch in selector: %v in package %q, but want %q", s, s.Pkg.Path, pkg.Path);
                    }

                }

            }


            w.@string(name);

        }

        private static void typ(this ptr<exportWriter> _addr_w, ptr<types.Type> _addr_t)
        {
            ref exportWriter w = ref _addr_w.val;
            ref types.Type t = ref _addr_t.val;

            w.data.uint64(w.p.typOff(t));
        }

        private static ptr<exportWriter> newWriter(this ptr<iexporter> _addr_p)
        {
            ref iexporter p = ref _addr_p.val;

            return addr(new exportWriter(p:p));
        }

        private static ulong flush(this ptr<exportWriter> _addr_w)
        {
            ref exportWriter w = ref _addr_w.val;

            var off = uint64(w.p.data0.Len());
            io.Copy(_addr_w.p.data0, _addr_w.data);
            return off;
        }

        private static ulong typOff(this ptr<iexporter> _addr_p, ptr<types.Type> _addr_t)
        {
            ref iexporter p = ref _addr_p.val;
            ref types.Type t = ref _addr_t.val;

            var (off, ok) = p.typIndex[t];
            if (!ok)
            {
                var w = p.newWriter();
                w.doTyp(t);
                off = predeclReserved + w.flush();
                p.typIndex[t] = off;
            }

            return off;

        }

        private static void startType(this ptr<exportWriter> _addr_w, itag k)
        {
            ref exportWriter w = ref _addr_w.val;

            w.data.uint64(uint64(k));
        }

        private static void doTyp(this ptr<exportWriter> _addr_w, ptr<types.Type> _addr_t)
        {
            ref exportWriter w = ref _addr_w.val;
            ref types.Type t = ref _addr_t.val;

            if (t.Sym != null)
            {
                if (t.Sym.Pkg == builtinpkg || t.Sym.Pkg == unsafepkg)
                {
                    Fatalf("builtin type missing from typIndex: %v", t);
                }

                w.startType(definedType);
                w.qualifiedIdent(typenod(t));
                return ;

            }


            if (t.Etype == TPTR) 
                w.startType(pointerType);
                w.typ(t.Elem());
            else if (t.Etype == TSLICE) 
                w.startType(sliceType);
                w.typ(t.Elem());
            else if (t.Etype == TARRAY) 
                w.startType(arrayType);
                w.uint64(uint64(t.NumElem()));
                w.typ(t.Elem());
            else if (t.Etype == TCHAN) 
                w.startType(chanType);
                w.uint64(uint64(t.ChanDir()));
                w.typ(t.Elem());
            else if (t.Etype == TMAP) 
                w.startType(mapType);
                w.typ(t.Key());
                w.typ(t.Elem());
            else if (t.Etype == TFUNC) 
                w.startType(signatureType);
                w.setPkg(t.Pkg(), true);
                w.signature(t);
            else if (t.Etype == TSTRUCT) 
                w.startType(structType);
                w.setPkg(t.Pkg(), true);

                w.uint64(uint64(t.NumFields()));
                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in t.FieldSlice())
                    {
                        f = __f;
                        w.pos(f.Pos);
                        w.selector(f.Sym);
                        w.typ(f.Type);
                        w.@bool(f.Embedded != 0L);
                        w.@string(f.Note);
                    }

                    f = f__prev1;
                }
            else if (t.Etype == TINTER) 
                slice<ptr<types.Field>> embeddeds = default;                slice<ptr<types.Field>> methods = default;

                foreach (var (_, m) in t.Methods().Slice())
                {
                    if (m.Sym != null)
                    {
                        methods = append(methods, m);
                    }
                    else
                    {
                        embeddeds = append(embeddeds, m);
                    }

                }
                w.startType(interfaceType);
                w.setPkg(t.Pkg(), true);

                w.uint64(uint64(len(embeddeds)));
                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in embeddeds)
                    {
                        f = __f;
                        w.pos(f.Pos);
                        w.typ(f.Type);
                    }

                    f = f__prev1;
                }

                w.uint64(uint64(len(methods)));
                {
                    var f__prev1 = f;

                    foreach (var (_, __f) in methods)
                    {
                        f = __f;
                        w.pos(f.Pos);
                        w.selector(f.Sym);
                        w.signature(f.Type);
                    }

                    f = f__prev1;
                }
            else 
                Fatalf("unexpected type: %v", t);
            
        }

        private static void setPkg(this ptr<exportWriter> _addr_w, ptr<types.Pkg> _addr_pkg, bool write)
        {
            ref exportWriter w = ref _addr_w.val;
            ref types.Pkg pkg = ref _addr_pkg.val;

            if (pkg == null)
            { 
                // TODO(mdempsky): Proactively set Pkg for types and
                // remove this fallback logic.
                pkg = localpkg;

            }

            if (write)
            {
                w.pkg(pkg);
            }

            w.currPkg = pkg;

        }

        private static void signature(this ptr<exportWriter> _addr_w, ptr<types.Type> _addr_t)
        {
            ref exportWriter w = ref _addr_w.val;
            ref types.Type t = ref _addr_t.val;

            w.paramList(t.Params().FieldSlice());
            w.paramList(t.Results().FieldSlice());
            {
                var n = t.Params().NumFields();

                if (n > 0L)
                {
                    w.@bool(t.Params().Field(n - 1L).IsDDD());
                }

            }

        }

        private static void paramList(this ptr<exportWriter> _addr_w, slice<ptr<types.Field>> fs)
        {
            ref exportWriter w = ref _addr_w.val;

            w.uint64(uint64(len(fs)));
            foreach (var (_, f) in fs)
            {
                w.param(f);
            }

        }

        private static void param(this ptr<exportWriter> _addr_w, ptr<types.Field> _addr_f)
        {
            ref exportWriter w = ref _addr_w.val;
            ref types.Field f = ref _addr_f.val;

            w.pos(f.Pos);
            w.localIdent(origSym(f.Sym), 0L);
            w.typ(f.Type);
        }

        private static Ctype constTypeOf(ptr<types.Type> _addr_typ)
        {
            ref types.Type typ = ref _addr_typ.val;


            if (typ == types.Idealint || typ == types.Idealrune) 
                return CTINT;
            else if (typ == types.Idealfloat) 
                return CTFLT;
            else if (typ == types.Idealcomplex) 
                return CTCPLX;
            
            if (typ.Etype == TCHAN || typ.Etype == TFUNC || typ.Etype == TMAP || typ.Etype == TNIL || typ.Etype == TINTER || typ.Etype == TPTR || typ.Etype == TSLICE || typ.Etype == TUNSAFEPTR) 
                return CTNIL;
            else if (typ.Etype == TBOOL) 
                return CTBOOL;
            else if (typ.Etype == TSTRING) 
                return CTSTR;
            else if (typ.Etype == TINT || typ.Etype == TINT8 || typ.Etype == TINT16 || typ.Etype == TINT32 || typ.Etype == TINT64 || typ.Etype == TUINT || typ.Etype == TUINT8 || typ.Etype == TUINT16 || typ.Etype == TUINT32 || typ.Etype == TUINT64 || typ.Etype == TUINTPTR) 
                return CTINT;
            else if (typ.Etype == TFLOAT32 || typ.Etype == TFLOAT64) 
                return CTFLT;
            else if (typ.Etype == TCOMPLEX64 || typ.Etype == TCOMPLEX128) 
                return CTCPLX;
                        Fatalf("unexpected constant type: %v", typ);
            return 0L;

        }

        private static void value(this ptr<exportWriter> _addr_w, ptr<types.Type> _addr_typ, Val v)
        {
            ref exportWriter w = ref _addr_w.val;
            ref types.Type typ = ref _addr_typ.val;

            if (typ.IsUntyped())
            {
                typ = untype(v.Ctype());
            }

            w.typ(typ); 

            // Each type has only one admissible constant representation,
            // so we could type switch directly on v.U here. However,
            // switching on the type increases symmetry with import logic
            // and provides a useful consistency check.


            if (constTypeOf(_addr_typ) == CTNIL) 
                // Only one value; nothing to encode.
                _ = v.U._<ptr<NilVal>>();
            else if (constTypeOf(_addr_typ) == CTBOOL) 
                w.@bool(v.U._<bool>());
            else if (constTypeOf(_addr_typ) == CTSTR) 
                w.@string(v.U._<@string>());
            else if (constTypeOf(_addr_typ) == CTINT) 
                w.mpint(_addr_v.U._<ptr<Mpint>>().Val, typ);
            else if (constTypeOf(_addr_typ) == CTFLT) 
                w.mpfloat(_addr_v.U._<ptr<Mpflt>>().Val, typ);
            else if (constTypeOf(_addr_typ) == CTCPLX) 
                ptr<Mpcplx> x = v.U._<ptr<Mpcplx>>();
                w.mpfloat(_addr_x.Real.Val, typ);
                w.mpfloat(_addr_x.Imag.Val, typ);
            
        }

        private static (bool, ulong) intSize(ptr<types.Type> _addr_typ)
        {
            bool signed = default;
            ulong maxBytes = default;
            ref types.Type typ = ref _addr_typ.val;

            if (typ.IsUntyped())
            {
                return (true, Mpprec / 8L);
            }


            if (typ.Etype == TFLOAT32 || typ.Etype == TCOMPLEX64) 
                return (true, 3L);
            else if (typ.Etype == TFLOAT64 || typ.Etype == TCOMPLEX128) 
                return (true, 7L);
                        signed = typ.IsSigned();
            maxBytes = uint(typ.Size()); 

            // The go/types API doesn't expose sizes to importers, so they
            // don't know how big these types are.

            if (typ.Etype == TINT || typ.Etype == TUINT || typ.Etype == TUINTPTR) 
                maxBytes = 8L;
                        return ;

        }

        // mpint exports a multi-precision integer.
        //
        // For unsigned types, small values are written out as a single
        // byte. Larger values are written out as a length-prefixed big-endian
        // byte string, where the length prefix is encoded as its complement.
        // For example, bytes 0, 1, and 2 directly represent the integer
        // values 0, 1, and 2; while bytes 255, 254, and 253 indicate a 1-,
        // 2-, and 3-byte big-endian string follow.
        //
        // Encoding for signed types use the same general approach as for
        // unsigned types, except small values use zig-zag encoding and the
        // bottom bit of length prefix byte for large values is reserved as a
        // sign bit.
        //
        // The exact boundary between small and large encodings varies
        // according to the maximum number of bytes needed to encode a value
        // of type typ. As a special case, 8-bit types are always encoded as a
        // single byte.
        //
        // TODO(mdempsky): Is this level of complexity really worthwhile?
        private static void mpint(this ptr<exportWriter> _addr_w, ptr<big.Int> _addr_x, ptr<types.Type> _addr_typ)
        {
            ref exportWriter w = ref _addr_w.val;
            ref big.Int x = ref _addr_x.val;
            ref types.Type typ = ref _addr_typ.val;

            var (signed, maxBytes) = intSize(_addr_typ);

            var negative = x.Sign() < 0L;
            if (!signed && negative)
            {
                Fatalf("negative unsigned integer; type %v, value %v", typ, x);
            }

            var b = x.Bytes();
            if (len(b) > 0L && b[0L] == 0L)
            {
                Fatalf("leading zeros");
            }

            if (uint(len(b)) > maxBytes)
            {
                Fatalf("bad mpint length: %d > %d (type %v, value %v)", len(b), maxBytes, typ, x);
            }

            long maxSmall = 256L - maxBytes;
            if (signed)
            {
                maxSmall = 256L - 2L * maxBytes;
            }

            if (maxBytes == 1L)
            {
                maxSmall = 256L;
            } 

            // Check if x can use small value encoding.
            if (len(b) <= 1L)
            {
                ulong ux = default;
                if (len(b) == 1L)
                {
                    ux = uint(b[0L]);
                }

                if (signed)
                {
                    ux <<= 1L;
                    if (negative)
                    {
                        ux--;
                    }

                }

                if (ux < maxSmall)
                {
                    w.data.WriteByte(byte(ux));
                    return ;
                }

            }

            long n = 256L - uint(len(b));
            if (signed)
            {
                n = 256L - 2L * uint(len(b));
                if (negative)
                {
                    n |= 1L;
                }

            }

            if (n < maxSmall || n >= 256L)
            {
                Fatalf("encoding mistake: %d, %v, %v => %d", len(b), signed, negative, n);
            }

            w.data.WriteByte(byte(n));
            w.data.Write(b);

        }

        // mpfloat exports a multi-precision floating point number.
        //
        // The number's value is decomposed into mantissa × 2**exponent, where
        // mantissa is an integer. The value is written out as mantissa (as a
        // multi-precision integer) and then the exponent, except exponent is
        // omitted if mantissa is zero.
        private static void mpfloat(this ptr<exportWriter> _addr_w, ptr<big.Float> _addr_f, ptr<types.Type> _addr_typ)
        {
            ref exportWriter w = ref _addr_w.val;
            ref big.Float f = ref _addr_f.val;
            ref types.Type typ = ref _addr_typ.val;

            if (f.IsInf())
            {
                Fatalf("infinite constant");
            } 

            // Break into f = mant × 2**exp, with 0.5 <= mant < 1.
            ref big.Float mant = ref heap(out ptr<big.Float> _addr_mant);
            var exp = int64(f.MantExp(_addr_mant)); 

            // Scale so that mant is an integer.
            var prec = mant.MinPrec();
            mant.SetMantExp(_addr_mant, int(prec));
            exp -= int64(prec);

            var (manti, acc) = mant.Int(null);
            if (acc != big.Exact)
            {
                Fatalf("mantissa scaling failed for %f (%s)", f, acc);
            }

            w.mpint(manti, typ);
            if (manti.Sign() != 0L)
            {
                w.int64(exp);
            }

        }

        private static bool @bool(this ptr<exportWriter> _addr_w, bool b)
        {
            ref exportWriter w = ref _addr_w.val;

            ulong x = default;
            if (b)
            {
                x = 1L;
            }

            w.uint64(x);
            return b;

        }

        private static void int64(this ptr<exportWriter> _addr_w, long x)
        {
            ref exportWriter w = ref _addr_w.val;

            w.data.int64(x);
        }
        private static void uint64(this ptr<exportWriter> _addr_w, ulong x)
        {
            ref exportWriter w = ref _addr_w.val;

            w.data.uint64(x);
        }
        private static void @string(this ptr<exportWriter> _addr_w, @string s)
        {
            ref exportWriter w = ref _addr_w.val;

            w.uint64(w.p.stringOff(s));
        }

        // Compiler-specific extensions.

        private static void varExt(this ptr<exportWriter> _addr_w, ptr<Node> _addr_n)
        {
            ref exportWriter w = ref _addr_w.val;
            ref Node n = ref _addr_n.val;

            w.linkname(n.Sym);
            w.symIdx(n.Sym);
        }

        private static void funcExt(this ptr<exportWriter> _addr_w, ptr<Node> _addr_n)
        {
            ref exportWriter w = ref _addr_w.val;
            ref Node n = ref _addr_n.val;

            w.linkname(n.Sym);
            w.symIdx(n.Sym); 

            // Escape analysis.
            foreach (var (_, fs) in _addr_types.RecvsParams)
            {
                foreach (var (_, f) in fs(n.Type).FieldSlice())
                {
                    w.@string(f.Note);
                }

            } 

            // Inline body.
            if (n.Func.Inl != null)
            {
                w.uint64(1L + uint64(n.Func.Inl.Cost));
                if (n.Func.ExportInline())
                {
                    w.p.doInline(n);
                } 

                // Endlineno for inlined function.
                if (n.Name.Defn != null)
                {
                    w.pos(n.Name.Defn.Func.Endlineno);
                }
                else
                { 
                    // When the exported node was defined externally,
                    // e.g. io exports atomic.(*Value).Load or bytes exports errors.New.
                    // Keep it as we don't distinguish this case in iimport.go.
                    w.pos(n.Func.Endlineno);

                }

            }
            else
            {
                w.uint64(0L);
            }

        }

        private static void methExt(this ptr<exportWriter> _addr_w, ptr<types.Field> _addr_m)
        {
            ref exportWriter w = ref _addr_w.val;
            ref types.Field m = ref _addr_m.val;

            w.@bool(m.Nointerface());
            w.funcExt(asNode(m.Type.Nname()));
        }

        private static void linkname(this ptr<exportWriter> _addr_w, ptr<types.Sym> _addr_s)
        {
            ref exportWriter w = ref _addr_w.val;
            ref types.Sym s = ref _addr_s.val;

            w.@string(s.Linkname);
        }

        private static void symIdx(this ptr<exportWriter> _addr_w, ptr<types.Sym> _addr_s)
        {
            ref exportWriter w = ref _addr_w.val;
            ref types.Sym s = ref _addr_s.val;

            if (Ctxt.Flag_go115newobj)
            {
                var lsym = s.Linksym();
                if (lsym.PkgIdx > goobj2.PkgIdxSelf || (lsym.PkgIdx == goobj2.PkgIdxInvalid && !lsym.Indexed()) || s.Linkname != "")
                { 
                    // Don't export index for non-package symbols, linkname'd symbols,
                    // and symbols without an index. They can only be referenced by
                    // name.
                    w.int64(-1L);

                }
                else
                { 
                    // For a defined symbol, export its index.
                    // For re-exporting an imported symbol, pass its index through.
                    w.int64(int64(lsym.SymIdx));

                }

            }

        }

        // Inline bodies.

        private static void stmtList(this ptr<exportWriter> _addr_w, Nodes list)
        {
            ref exportWriter w = ref _addr_w.val;

            foreach (var (_, n) in list.Slice())
            {
                w.node(n);
            }
            w.op(OEND);

        }

        private static void node(this ptr<exportWriter> _addr_w, ptr<Node> _addr_n)
        {
            ref exportWriter w = ref _addr_w.val;
            ref Node n = ref _addr_n.val;

            if (opprec[n.Op] < 0L)
            {
                w.stmt(n);
            }
            else
            {
                w.expr(n);
            }

        }

        // Caution: stmt will emit more than one node for statement nodes n that have a non-empty
        // n.Ninit and where n cannot have a natural init section (such as in "if", "for", etc.).
        private static void stmt(this ptr<exportWriter> _addr_w, ptr<Node> _addr_n)
        {
            ref exportWriter w = ref _addr_w.val;
            ref Node n = ref _addr_n.val;

            if (n.Ninit.Len() > 0L && !stmtwithinit(n.Op))
            { 
                // can't use stmtList here since we don't want the final OEND
                foreach (var (_, n) in n.Ninit.Slice())
                {
                    w.stmt(n);
                }

            }

            {
                var op = n.Op;


                if (op == ODCL) 
                    w.op(ODCL);
                    w.pos(n.Left.Pos);
                    w.localName(n.Left);
                    w.typ(n.Left.Type); 

                    // case ODCLFIELD:
                    //    unimplemented - handled by default case
                else if (op == OAS) 
                    // Don't export "v = <N>" initializing statements, hope they're always
                    // preceded by the DCL which will be re-parsed and typecheck to reproduce
                    // the "v = <N>" again.
                    if (n.Right != null)
                    {
                        w.op(OAS);
                        w.pos(n.Pos);
                        w.expr(n.Left);
                        w.expr(n.Right);
                    }

                else if (op == OASOP) 
                    w.op(OASOP);
                    w.pos(n.Pos);
                    w.op(n.SubOp());
                    w.expr(n.Left);
                    if (w.@bool(!n.Implicit()))
                    {
                        w.expr(n.Right);
                    }

                else if (op == OAS2) 
                    w.op(OAS2);
                    w.pos(n.Pos);
                    w.exprList(n.List);
                    w.exprList(n.Rlist);
                else if (op == OAS2DOTTYPE || op == OAS2FUNC || op == OAS2MAPR || op == OAS2RECV) 
                    w.op(OAS2);
                    w.pos(n.Pos);
                    w.exprList(n.List);
                    w.exprList(asNodes(new slice<ptr<Node>>(new ptr<Node>[] { n.Right })));
                else if (op == ORETURN) 
                    w.op(ORETURN);
                    w.pos(n.Pos);
                    w.exprList(n.List); 

                    // case ORETJMP:
                    //     unreachable - generated by compiler for trampolin routines
                else if (op == OGO || op == ODEFER) 
                    w.op(op);
                    w.pos(n.Pos);
                    w.expr(n.Left);
                else if (op == OIF) 
                    w.op(OIF);
                    w.pos(n.Pos);
                    w.stmtList(n.Ninit);
                    w.expr(n.Left);
                    w.stmtList(n.Nbody);
                    w.stmtList(n.Rlist);
                else if (op == OFOR) 
                    w.op(OFOR);
                    w.pos(n.Pos);
                    w.stmtList(n.Ninit);
                    w.exprsOrNil(n.Left, n.Right);
                    w.stmtList(n.Nbody);
                else if (op == ORANGE) 
                    w.op(ORANGE);
                    w.pos(n.Pos);
                    w.stmtList(n.List);
                    w.expr(n.Right);
                    w.stmtList(n.Nbody);
                else if (op == OSELECT || op == OSWITCH) 
                    w.op(op);
                    w.pos(n.Pos);
                    w.stmtList(n.Ninit);
                    w.exprsOrNil(n.Left, null);
                    w.stmtList(n.List);
                else if (op == OCASE) 
                    w.op(OCASE);
                    w.pos(n.Pos);
                    w.stmtList(n.List);
                    w.stmtList(n.Nbody);
                else if (op == OFALL) 
                    w.op(OFALL);
                    w.pos(n.Pos);
                else if (op == OBREAK || op == OCONTINUE) 
                    w.op(op);
                    w.pos(n.Pos);
                    w.exprsOrNil(n.Left, null);
                else if (op == OEMPTY)                 else if (op == OGOTO || op == OLABEL) 
                    w.op(op);
                    w.pos(n.Pos);
                    w.@string(n.Sym.Name);
                else 
                    Fatalf("exporter: CANNOT EXPORT: %v\nPlease notify gri@\n", n.Op);

            }

        }

        private static void exprList(this ptr<exportWriter> _addr_w, Nodes list)
        {
            ref exportWriter w = ref _addr_w.val;

            foreach (var (_, n) in list.Slice())
            {
                w.expr(n);
            }
            w.op(OEND);

        }

        private static void expr(this ptr<exportWriter> _addr_w, ptr<Node> _addr_n)
        {
            ref exportWriter w = ref _addr_w.val;
            ref Node n = ref _addr_n.val;
 
            // from nodefmt (fmt.go)
            //
            // nodefmt reverts nodes back to their original - we don't need to do
            // it because we are not bound to produce valid Go syntax when exporting
            //
            // if (fmtmode != FExp || n.Op != OLITERAL) && n.Orig != nil {
            //     n = n.Orig
            // }

            // from exprfmt (fmt.go)
            while (n.Op == OPAREN || n.Implicit() && (n.Op == ODEREF || n.Op == OADDR || n.Op == ODOT || n.Op == ODOTPTR))
            {
                n = n.Left;
            }


            {
                var op = n.Op;


                // expressions
                // (somewhat closely following the structure of exprfmt in fmt.go)
                if (op == OLITERAL) 
                    if (n.Val().Ctype() == CTNIL && n.Orig != null && n.Orig != n)
                    {
                        w.expr(n.Orig);
                        break;
                    }

                    w.op(OLITERAL);
                    w.pos(n.Pos);
                    w.value(n.Type, n.Val());
                else if (op == ONAME) 
                    // Special case: explicit name of func (*T) method(...) is turned into pkg.(*T).method,
                    // but for export, this should be rendered as (*pkg.T).meth.
                    // These nodes have the special property that they are names with a left OTYPE and a right ONAME.
                    if (n.isMethodExpression())
                    {
                        w.op(OXDOT);
                        w.pos(n.Pos);
                        w.expr(n.Left); // n.Left.Op == OTYPE
                        w.selector(n.Right.Sym);
                        break;

                    } 

                    // Package scope name.
                    if ((n.Class() == PEXTERN || n.Class() == PFUNC) && !n.isBlank())
                    {
                        w.op(ONONAME);
                        w.qualifiedIdent(n);
                        break;
                    } 

                    // Function scope name.
                    w.op(ONAME);
                    w.localName(n); 

                    // case OPACK, ONONAME:
                    //     should have been resolved by typechecking - handled by default case
                else if (op == OTYPE) 
                    w.op(OTYPE);
                    w.typ(n.Type); 

                    // case OTARRAY, OTMAP, OTCHAN, OTSTRUCT, OTINTER, OTFUNC:
                    //     should have been resolved by typechecking - handled by default case

                    // case OCLOSURE:
                    //    unimplemented - handled by default case

                    // case OCOMPLIT:
                    //     should have been resolved by typechecking - handled by default case
                else if (op == OPTRLIT) 
                    w.op(OADDR);
                    w.pos(n.Pos);
                    w.expr(n.Left);
                else if (op == OSTRUCTLIT) 
                    w.op(OSTRUCTLIT);
                    w.pos(n.Pos);
                    w.typ(n.Type);
                    w.elemList(n.List); // special handling of field names
                else if (op == OARRAYLIT || op == OSLICELIT || op == OMAPLIT) 
                    w.op(OCOMPLIT);
                    w.pos(n.Pos);
                    w.typ(n.Type);
                    w.exprList(n.List);
                else if (op == OKEY) 
                    w.op(OKEY);
                    w.pos(n.Pos);
                    w.exprsOrNil(n.Left, n.Right); 

                    // case OSTRUCTKEY:
                    //    unreachable - handled in case OSTRUCTLIT by elemList

                    // case OCALLPART:
                    //    unimplemented - handled by default case
                else if (op == OXDOT || op == ODOT || op == ODOTPTR || op == ODOTINTER || op == ODOTMETH) 
                    w.op(OXDOT);
                    w.pos(n.Pos);
                    w.expr(n.Left);
                    w.selector(n.Sym);
                else if (op == ODOTTYPE || op == ODOTTYPE2) 
                    w.op(ODOTTYPE);
                    w.pos(n.Pos);
                    w.expr(n.Left);
                    w.typ(n.Type);
                else if (op == OINDEX || op == OINDEXMAP) 
                    w.op(OINDEX);
                    w.pos(n.Pos);
                    w.expr(n.Left);
                    w.expr(n.Right);
                else if (op == OSLICE || op == OSLICESTR || op == OSLICEARR) 
                    w.op(OSLICE);
                    w.pos(n.Pos);
                    w.expr(n.Left);
                    var (low, high, _) = n.SliceBounds();
                    w.exprsOrNil(low, high);
                else if (op == OSLICE3 || op == OSLICE3ARR) 
                    w.op(OSLICE3);
                    w.pos(n.Pos);
                    w.expr(n.Left);
                    var (low, high, max) = n.SliceBounds();
                    w.exprsOrNil(low, high);
                    w.expr(max);
                else if (op == OCOPY || op == OCOMPLEX) 
                    // treated like other builtin calls (see e.g., OREAL)
                    w.op(op);
                    w.pos(n.Pos);
                    w.expr(n.Left);
                    w.expr(n.Right);
                    w.op(OEND);
                else if (op == OCONV || op == OCONVIFACE || op == OCONVNOP || op == OBYTES2STR || op == ORUNES2STR || op == OSTR2BYTES || op == OSTR2RUNES || op == ORUNESTR) 
                    w.op(OCONV);
                    w.pos(n.Pos);
                    w.expr(n.Left);
                    w.typ(n.Type);
                else if (op == OREAL || op == OIMAG || op == OAPPEND || op == OCAP || op == OCLOSE || op == ODELETE || op == OLEN || op == OMAKE || op == ONEW || op == OPANIC || op == ORECOVER || op == OPRINT || op == OPRINTN) 
                    w.op(op);
                    w.pos(n.Pos);
                    if (n.Left != null)
                    {
                        w.expr(n.Left);
                        w.op(OEND);
                    }
                    else
                    {
                        w.exprList(n.List); // emits terminating OEND
                    } 
                    // only append() calls may contain '...' arguments
                    if (op == OAPPEND)
                    {
                        w.@bool(n.IsDDD());
                    }
                    else if (n.IsDDD())
                    {
                        Fatalf("exporter: unexpected '...' with %v call", op);
                    }

                else if (op == OCALL || op == OCALLFUNC || op == OCALLMETH || op == OCALLINTER || op == OGETG) 
                    w.op(OCALL);
                    w.pos(n.Pos);
                    w.stmtList(n.Ninit);
                    w.expr(n.Left);
                    w.exprList(n.List);
                    w.@bool(n.IsDDD());
                else if (op == OMAKEMAP || op == OMAKECHAN || op == OMAKESLICE) 
                    w.op(op); // must keep separate from OMAKE for importer
                    w.pos(n.Pos);
                    w.typ(n.Type);

                    if (n.List.Len() != 0L) // pre-typecheck
                        w.exprList(n.List); // emits terminating OEND
                    else if (n.Right != null) 
                        w.expr(n.Left);
                        w.expr(n.Right);
                        w.op(OEND);
                    else if (n.Left != null && (n.Op == OMAKESLICE || !n.Left.Type.IsUntyped())) 
                        w.expr(n.Left);
                        w.op(OEND);
                    else 
                        // empty list
                        w.op(OEND);
                    // unary expressions
                else if (op == OPLUS || op == ONEG || op == OADDR || op == OBITNOT || op == ODEREF || op == ONOT || op == ORECV) 
                    w.op(op);
                    w.pos(n.Pos);
                    w.expr(n.Left); 

                    // binary expressions
                else if (op == OADD || op == OAND || op == OANDAND || op == OANDNOT || op == ODIV || op == OEQ || op == OGE || op == OGT || op == OLE || op == OLT || op == OLSH || op == OMOD || op == OMUL || op == ONE || op == OOR || op == OOROR || op == ORSH || op == OSEND || op == OSUB || op == OXOR) 
                    w.op(op);
                    w.pos(n.Pos);
                    w.expr(n.Left);
                    w.expr(n.Right);
                else if (op == OADDSTR) 
                    w.op(OADDSTR);
                    w.pos(n.Pos);
                    w.exprList(n.List);
                else if (op == ODCLCONST)                 else 
                    Fatalf("cannot export %v (%d) node\n" + "\t==> please file an issue and assign to gri@", n.Op, int(n.Op));

            }

        }

        private static void op(this ptr<exportWriter> _addr_w, Op op)
        {
            ref exportWriter w = ref _addr_w.val;

            w.uint64(uint64(op));
        }

        private static void exprsOrNil(this ptr<exportWriter> _addr_w, ptr<Node> _addr_a, ptr<Node> _addr_b)
        {
            ref exportWriter w = ref _addr_w.val;
            ref Node a = ref _addr_a.val;
            ref Node b = ref _addr_b.val;

            long ab = 0L;
            if (a != null)
            {
                ab |= 1L;
            }

            if (b != null)
            {
                ab |= 2L;
            }

            w.uint64(uint64(ab));
            if (ab & 1L != 0L)
            {
                w.expr(a);
            }

            if (ab & 2L != 0L)
            {
                w.node(b);
            }

        }

        private static void elemList(this ptr<exportWriter> _addr_w, Nodes list)
        {
            ref exportWriter w = ref _addr_w.val;

            w.uint64(uint64(list.Len()));
            foreach (var (_, n) in list.Slice())
            {
                w.selector(n.Sym);
                w.expr(n.Left);
            }

        }

        private static void localName(this ptr<exportWriter> _addr_w, ptr<Node> _addr_n)
        {
            ref exportWriter w = ref _addr_w.val;
            ref Node n = ref _addr_n.val;
 
            // Escape analysis happens after inline bodies are saved, but
            // we're using the same ONAME nodes, so we might still see
            // PAUTOHEAP here.
            //
            // Check for Stackcopy to identify PAUTOHEAP that came from
            // PPARAM/PPARAMOUT, because we only want to include vargen in
            // non-param names.
            int v = default;
            if (n.Class() == PAUTO || (n.Class() == PAUTOHEAP && n.Name.Param.Stackcopy == null))
            {
                v = n.Name.Vargen;
            }

            w.localIdent(n.Sym, v);

        }

        private static void localIdent(this ptr<exportWriter> _addr_w, ptr<types.Sym> _addr_s, int v)
        {
            ref exportWriter w = ref _addr_w.val;
            ref types.Sym s = ref _addr_s.val;
 
            // Anonymous parameters.
            if (s == null)
            {
                w.@string("");
                return ;
            }

            var name = s.Name;
            if (name == "_")
            {
                w.@string("_");
                return ;
            } 

            // TODO(mdempsky): Fix autotmp hack.
            {
                var i = strings.LastIndex(name, ".");

                if (i >= 0L && !strings.HasPrefix(name, ".autotmp_"))
                {
                    Fatalf("unexpected dot in identifier: %v", name);
                }

            }


            if (v > 0L)
            {
                if (strings.Contains(name, "·"))
                {
                    Fatalf("exporter: unexpected · in symbol name");
                }

                name = fmt.Sprintf("%s·%d", name, v);

            }

            if (!types.IsExported(name) && s.Pkg != w.currPkg)
            {
                Fatalf("weird package in name: %v => %v, not %q", s, name, w.currPkg.Path);
            }

            w.@string(name);

        }

        private partial struct intWriter
        {
            public ref bytes.Buffer Buffer => ref Buffer_val;
        }

        private static void int64(this ptr<intWriter> _addr_w, long x)
        {
            ref intWriter w = ref _addr_w.val;

            array<byte> buf = new array<byte>(binary.MaxVarintLen64);
            var n = binary.PutVarint(buf[..], x);
            w.Write(buf[..n]);
        }

        private static void uint64(this ptr<intWriter> _addr_w, ulong x)
        {
            ref intWriter w = ref _addr_w.val;

            array<byte> buf = new array<byte>(binary.MaxVarintLen64);
            var n = binary.PutUvarint(buf[..], x);
            w.Write(buf[..n]);
        }
    }
}}}}
