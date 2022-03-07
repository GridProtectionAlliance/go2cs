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

// package typecheck -- go2cs converted at 2022 March 06 22:48:30 UTC
// import "cmd/compile/internal/typecheck" ==> using typecheck = go.cmd.compile.@internal.typecheck_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\typecheck\iexport.go
using bufio = go.bufio_package;
using bytes = go.bytes_package;
using md5 = go.crypto.md5_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using constant = go.go.constant_package;
using io = go.io_package;
using big = go.math.big_package;
using sort = go.sort_package;
using strings = go.strings_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using types = go.cmd.compile.@internal.types_package;
using goobj = go.cmd.@internal.goobj_package;
using src = go.cmd.@internal.src_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class typecheck_package {

    // Current indexed export format version. Increase with each format change.
    // 1: added column details to Pos
    // 0: Go1.11 encoding
private static readonly nint iexportVersion = 1;

// predeclReserved is the number of type offsets reserved for types
// implicitly declared in the universe block.


// predeclReserved is the number of type offsets reserved for types
// implicitly declared in the universe block.
private static readonly nint predeclReserved = 32;

// An itag distinguishes the kind of type that was written into the
// indexed export format.


// An itag distinguishes the kind of type that was written into the
// indexed export format.
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


private static readonly var debug = false;
private static readonly nuint magic = 0x6742937dc293105;


public static void WriteExports(ptr<bufio.Writer> _addr_@out) {
    ref bufio.Writer @out = ref _addr_@out.val;

    iexporter p = new iexporter(allPkgs:map[*types.Pkg]bool{},stringIndex:map[string]uint64{},declIndex:map[*types.Sym]uint64{},inlineIndex:map[*types.Sym]uint64{},typIndex:map[*types.Type]uint64{},);

    foreach (var (i, pt) in predeclared()) {
        p.typIndex[pt] = uint64(i);
    }    if (len(p.typIndex) > predeclReserved) {
        @base.Fatalf("too many predeclared types: %d > %d", len(p.typIndex), predeclReserved);
    }
    foreach (var (_, n) in Target.Exports) {
        p.pushDecl(n);
    }    while (!p.declTodo.Empty()) {
        p.doDecl(p.declTodo.PopLeft());
    } 

    // Append indices to data0 section.
    var dataLen = uint64(p.data0.Len());
    var w = p.newWriter();
    w.writeIndex(p.declIndex, true);
    w.writeIndex(p.inlineIndex, false);
    w.flush();

    if (@base.Flag.LowerV.val) {
        fmt.Printf("export: hdr strings %v, data %v, index %v\n", p.strings.Len(), dataLen, p.data0.Len());
    }
    ref intWriter hdr = ref heap(out ptr<intWriter> _addr_hdr);
    hdr.WriteByte('i');
    hdr.uint64(iexportVersion);
    hdr.uint64(uint64(p.strings.Len()));
    hdr.uint64(dataLen); 

    // Flush output.
    var h = md5.New();
    var wr = io.MultiWriter(out, h);
    io.Copy(wr, _addr_hdr);
    io.Copy(wr, _addr_p.strings);
    io.Copy(wr, _addr_p.data0); 

    // Add fingerprint (used by linker object file).
    // Attach this to the end, so tools (e.g. gcimporter) don't care.
    copy(@base.Ctxt.Fingerprint[..], h.Sum(null)[..]);
    @out.Write(@base.Ctxt.Fingerprint[..]);

}

// writeIndex writes out a symbol index. mainIndex indicates whether
// we're writing out the main index, which is also read by
// non-compiler tools and includes a complete package description
// (i.e., name and height).
private static void writeIndex(this ptr<exportWriter> _addr_w, map<ptr<types.Sym>, ulong> index, bool mainIndex) {
    ref exportWriter w = ref _addr_w.val;
 
    // Build a map from packages to symbols from that package.
    map pkgSyms = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<types.Pkg>, slice<ptr<types.Sym>>>{}; 

    // For the main index, make sure to include every package that
    // we reference, even if we're not exporting (or reexporting)
    // any symbols from it.
    if (mainIndex) {
        pkgSyms[types.LocalPkg] = null;
        {
            var pkg__prev1 = pkg;

            foreach (var (__pkg) in w.p.allPkgs) {
                pkg = __pkg;
                pkgSyms[pkg] = null;
            }

            pkg = pkg__prev1;
        }
    }
    {
        var sym__prev1 = sym;

        foreach (var (__sym) in index) {
            sym = __sym;
            pkgSyms[sym.Pkg] = append(pkgSyms[sym.Pkg], sym);
        }
        sym = sym__prev1;
    }

    slice<ptr<types.Pkg>> pkgs = default;
    {
        var pkg__prev1 = pkg;

        foreach (var (__pkg) in pkgSyms) {
            pkg = __pkg;
            pkgs = append(pkgs, pkg);
        }
        pkg = pkg__prev1;
    }

    sort.Slice(pkgs, (i, j) => {
        return pkgs[i].Path < pkgs[j].Path;
    });

    w.uint64(uint64(len(pkgs)));
    {
        var pkg__prev1 = pkg;

        foreach (var (_, __pkg) in pkgs) {
            pkg = __pkg;
            w.@string(pkg.Path);
            if (mainIndex) {
                w.@string(pkg.Name);
                w.uint64(uint64(pkg.Height));
            } 

            // Sort symbols within a package by name.
            var syms = pkgSyms[pkg];
            sort.Slice(syms, (i, j) => {
                return syms[i].Name < syms[j].Name;
            });

            w.uint64(uint64(len(syms)));
            {
                var sym__prev2 = sym;

                foreach (var (_, __sym) in syms) {
                    sym = __sym;
                    w.@string(sym.Name);
                    w.uint64(index[sym]);
                }

                sym = sym__prev2;
            }
        }
        pkg = pkg__prev1;
    }
}

private partial struct iexporter {
    public map<ptr<types.Pkg>, bool> allPkgs;
    public ir.NameQueue declTodo;
    public intWriter strings;
    public map<@string, ulong> stringIndex;
    public intWriter data0;
    public map<ptr<types.Sym>, ulong> declIndex;
    public map<ptr<types.Sym>, ulong> inlineIndex;
    public map<ptr<types.Type>, ulong> typIndex;
}

// stringOff returns the offset of s within the string section.
// If not already present, it's added to the end.
private static ulong stringOff(this ptr<iexporter> _addr_p, @string s) {
    ref iexporter p = ref _addr_p.val;

    var (off, ok) = p.stringIndex[s];
    if (!ok) {
        off = uint64(p.strings.Len());
        p.stringIndex[s] = off;

        if (@base.Flag.LowerV.val) {
            fmt.Printf("export: str %v %.40q\n", off, s);
        }
        p.strings.uint64(uint64(len(s)));
        p.strings.WriteString(s);

    }
    return off;

}

// pushDecl adds n to the declaration work queue, if not already present.
private static void pushDecl(this ptr<iexporter> _addr_p, ptr<ir.Name> _addr_n) {
    ref iexporter p = ref _addr_p.val;
    ref ir.Name n = ref _addr_n.val;

    if (n.Sym() == null || n.Sym().Def != n && n.Op() != ir.OTYPE) {
        @base.Fatalf("weird Sym: %v, %v", n, n.Sym());
    }
    if (n.Sym().Pkg == types.BuiltinPkg || n.Sym().Pkg == ir.Pkgs.Unsafe) {
        return ;
    }
    {
        var (_, ok) = p.declIndex[n.Sym()];

        if (ok) {
            return ;
        }
    }


    p.declIndex[n.Sym()] = ~uint64(0); // mark n present in work queue
    p.declTodo.PushRight(n);

}

// exportWriter handles writing out individual data section chunks.
private partial struct exportWriter {
    public ptr<iexporter> p;
    public intWriter data;
    public ptr<types.Pkg> currPkg;
    public @string prevFile;
    public long prevLine;
    public long prevColumn; // dclIndex maps function-scoped declarations to an int used to refer to
// them later in the function. For local variables/params, the int is
// non-negative and in order of the appearance in the Func's Dcl list. For
// closure variables, the index is negative starting at -2.
    public map<ptr<ir.Name>, nint> dclIndex;
    public nint maxDclIndex;
    public nint maxClosureVarIndex;
}

private static void doDecl(this ptr<iexporter> _addr_p, ptr<ir.Name> _addr_n) {
    ref iexporter p = ref _addr_p.val;
    ref ir.Name n = ref _addr_n.val;

    var w = p.newWriter();
    w.setPkg(n.Sym().Pkg, false);


    if (n.Op() == ir.ONAME) 

        if (n.Class == ir.PEXTERN) 
            // Variable.
            w.tag('V');
            w.pos(n.Pos());
            w.typ(n.Type());
            w.varExt(n);
        else if (n.Class == ir.PFUNC) 
            if (ir.IsMethod(n)) {
                @base.Fatalf("unexpected method: %v", n);
            } 

            // Function.
            w.tag('F');
            w.pos(n.Pos());
            w.signature(n.Type());
            w.funcExt(n);
        else 
            @base.Fatalf("unexpected class: %v, %v", n, n.Class);
            else if (n.Op() == ir.OLITERAL) 
        // TODO(mdempsky): Extend check to all declarations.
        if (n.Typecheck() == 0) {
            @base.FatalfAt(n.Pos(), "missed typecheck: %v", n);
        }
        w.tag('C');
        w.pos(n.Pos());
        w.value(n.Type(), n.Val());
        w.constExt(n);
    else if (n.Op() == ir.OTYPE) 
        if (types.IsDotAlias(n.Sym())) { 
            // Alias.
            w.tag('A');
            w.pos(n.Pos());
            w.typ(n.Type());
            break;

        }
        w.tag('T');
        w.pos(n.Pos());

        var underlying = n.Type().Underlying();
        if (underlying == types.ErrorType.Underlying()) { 
            // For "type T error", use error as the
            // underlying type instead of error's own
            // underlying anonymous interface. This
            // ensures consistency with how importers may
            // declare error (e.g., go/types uses nil Pkg
            // for predeclared objects).
            underlying = types.ErrorType;

        }
        w.typ(underlying);

        var t = n.Type();
        if (t.IsInterface()) {
            w.typeExt(t);
            break;
        }
        var ms = t.Methods();
        w.uint64(uint64(ms.Len()));
        {
            var m__prev1 = m;

            foreach (var (_, __m) in ms.Slice()) {
                m = __m;
                w.pos(m.Pos);
                w.selector(m.Sym);
                w.param(m.Type.Recv());
                w.signature(m.Type);
            }

            m = m__prev1;
        }

        w.typeExt(t);
        {
            var m__prev1 = m;

            foreach (var (_, __m) in ms.Slice()) {
                m = __m;
                w.methExt(m);
            }

            m = m__prev1;
        }
    else 
        @base.Fatalf("unexpected node: %v", n);
        w.finish("dcl", p.declIndex, n.Sym());

}

private static void tag(this ptr<exportWriter> _addr_w, byte tag) {
    ref exportWriter w = ref _addr_w.val;

    w.data.WriteByte(tag);
}

private static void finish(this ptr<exportWriter> _addr_w, @string what, map<ptr<types.Sym>, ulong> index, ptr<types.Sym> _addr_sym) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Sym sym = ref _addr_sym.val;

    var off = w.flush();
    if (@base.Flag.LowerV.val) {
        fmt.Printf("export: %v %v %v\n", what, off, sym);
    }
    index[sym] = off;

}

private static void doInline(this ptr<iexporter> _addr_p, ptr<ir.Name> _addr_f) {
    ref iexporter p = ref _addr_p.val;
    ref ir.Name f = ref _addr_f.val;

    var w = p.newWriter();
    w.setPkg(fnpkg(f), false);

    w.dclIndex = make_map<ptr<ir.Name>, nint>(len(f.Func.Inl.Dcl));
    w.funcBody(f.Func);

    w.finish("inl", p.inlineIndex, f.Sym());
}

private static void pos(this ptr<exportWriter> _addr_w, src.XPos pos) {
    ref exportWriter w = ref _addr_w.val;

    var p = @base.Ctxt.PosTable.Pos(pos);
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

    var deltaColumn = (column - w.prevColumn) << 1;
    var deltaLine = (line - w.prevLine) << 1;

    if (file != w.prevFile) {
        deltaLine |= 1;
    }
    if (deltaLine != 0) {
        deltaColumn |= 1;
    }
    w.int64(deltaColumn);
    if (deltaColumn & 1 != 0) {
        w.int64(deltaLine);
        if (deltaLine & 1 != 0) {
            w.@string(file);
        }
    }
    w.prevFile = file;
    w.prevLine = line;
    w.prevColumn = column;

}

private static void pkg(this ptr<exportWriter> _addr_w, ptr<types.Pkg> _addr_pkg) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Pkg pkg = ref _addr_pkg.val;
 
    // TODO(mdempsky): Add flag to types.Pkg to mark pseudo-packages.
    if (pkg == ir.Pkgs.Go) {
        @base.Fatalf("export of pseudo-package: %q", pkg.Path);
    }
    w.p.allPkgs[pkg] = true;

    w.@string(pkg.Path);

}

private static void qualifiedIdent(this ptr<exportWriter> _addr_w, ptr<ir.Name> _addr_n) {
    ref exportWriter w = ref _addr_w.val;
    ref ir.Name n = ref _addr_n.val;
 
    // Ensure any referenced declarations are written out too.
    w.p.pushDecl(n);

    var s = n.Sym();
    w.@string(s.Name);
    w.pkg(s.Pkg);

}

private static void selector(this ptr<exportWriter> _addr_w, ptr<types.Sym> _addr_s) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Sym s = ref _addr_s.val;

    if (w.currPkg == null) {
        @base.Fatalf("missing currPkg");
    }
    var pkg = w.currPkg;
    if (types.IsExported(s.Name)) {
        pkg = types.LocalPkg;
    }
    if (s.Pkg != pkg) {
        @base.Fatalf("package mismatch in selector: %v in package %q, but want %q", s, s.Pkg.Path, pkg.Path);
    }
    w.@string(s.Name);

}

private static void typ(this ptr<exportWriter> _addr_w, ptr<types.Type> _addr_t) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Type t = ref _addr_t.val;

    w.data.uint64(w.p.typOff(t));
}

// The "exotic" functions in this section encode a wider range of
// items than the standard encoding functions above. These include
// types that do not appear in declarations, only in code, such as
// method types. These methods need to be separate from the standard
// encoding functions because we don't want to modify the encoding
// generated by the standard functions (because that exported
// information is read by tools besides the compiler).

// exoticType exports a type to the writer.
private static void exoticType(this ptr<exportWriter> _addr_w, ptr<types.Type> _addr_t) => func((_, panic, _) => {
    ref exportWriter w = ref _addr_w.val;
    ref types.Type t = ref _addr_t.val;


    if (t == null) 
        // Calls-as-statements have no type.
        w.data.uint64(exoticTypeNil);
    else if (t.IsStruct() && t.StructType().Funarg != types.FunargNone) 
        // These are weird structs for representing tuples of types returned
        // by multi-return functions.
        // They don't fit the standard struct type mold. For instance,
        // they don't have any package info.
        w.data.uint64(exoticTypeTuple);
        w.uint64(uint64(t.StructType().Funarg));
        w.uint64(uint64(t.NumFields()));
        foreach (var (_, f) in t.FieldSlice()) {
            w.pos(f.Pos);
            var s = f.Sym;
            if (s == null) {
                w.uint64(0);
            }
            else if (s.Pkg == null) {
                w.uint64(exoticTypeSymNoPkg);
                w.@string(s.Name);
            }
            else
 {
                w.uint64(exoticTypeSymWithPkg);
                w.pkg(s.Pkg);
                w.@string(s.Name);
            }

            w.typ(f.Type);
            if (f.Embedded != 0 || f.Note != "") {
                panic("extra info in funarg struct field");
            }

        }    else if (t.Kind() == types.TFUNC && t.Recv() != null) 
        w.data.uint64(exoticTypeRecv); 
        // interface method types have a fake receiver type.
        var isFakeRecv = t.Recv().Type == types.FakeRecvType();
        w.@bool(isFakeRecv);
        if (!isFakeRecv) {
            w.exoticParam(t.Recv());
        }
        w.exoticSignature(t);
    else 
        // A regular type.
        w.data.uint64(exoticTypeRegular);
        w.typ(t);
    
});

private static readonly var exoticTypeNil = iota;
private static readonly var exoticTypeTuple = 0;
private static readonly var exoticTypeRecv = 1;
private static readonly var exoticTypeRegular = 2;

private static readonly var exoticTypeSymNil = iota;
private static readonly var exoticTypeSymNoPkg = 0;
private static readonly var exoticTypeSymWithPkg = 1;


// Export a selector, but one whose package may not match
// the package being compiled. This is a separate function
// because the standard selector() serialization format is fixed
// by the go/types reader. This one can only be used during
// inline/generic body exporting.
private static void exoticSelector(this ptr<exportWriter> _addr_w, ptr<types.Sym> _addr_s) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Sym s = ref _addr_s.val;

    var pkg = w.currPkg;
    if (types.IsExported(s.Name)) {
        pkg = types.LocalPkg;
    }
    w.@string(s.Name);
    if (s.Pkg == pkg) {
        w.uint64(0);
    }
    else
 {
        w.uint64(1);
        w.pkg(s.Pkg);
    }
}

private static void exoticSignature(this ptr<exportWriter> _addr_w, ptr<types.Type> _addr_t) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Type t = ref _addr_t.val;

    var hasPkg = t.Pkg() != null;
    w.@bool(hasPkg);
    if (hasPkg) {
        w.pkg(t.Pkg());
    }
    w.exoticParamList(t.Params().FieldSlice());
    w.exoticParamList(t.Results().FieldSlice());

}

private static void exoticParamList(this ptr<exportWriter> _addr_w, slice<ptr<types.Field>> fs) {
    ref exportWriter w = ref _addr_w.val;

    w.uint64(uint64(len(fs)));
    foreach (var (_, f) in fs) {
        w.exoticParam(f);
    }
}
private static void exoticParam(this ptr<exportWriter> _addr_w, ptr<types.Field> _addr_f) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Field f = ref _addr_f.val;

    w.pos(f.Pos);
    w.exoticSym(f.Sym);
    w.uint64(uint64(f.Offset));
    w.exoticType(f.Type);
    w.@bool(f.IsDDD());
}

private static void exoticField(this ptr<exportWriter> _addr_w, ptr<types.Field> _addr_f) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Field f = ref _addr_f.val;

    w.pos(f.Pos);
    w.exoticSym(f.Sym);
    w.uint64(uint64(f.Offset));
    w.exoticType(f.Type);
    w.@string(f.Note);
}

private static void exoticSym(this ptr<exportWriter> _addr_w, ptr<types.Sym> _addr_s) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Sym s = ref _addr_s.val;

    if (s == null) {
        w.@string("");
        return ;
    }
    if (s.Name == "") {
        @base.Fatalf("empty symbol name");
    }
    w.@string(s.Name);
    if (!types.IsExported(s.Name)) {
        w.pkg(s.Pkg);
    }
}

private static ptr<exportWriter> newWriter(this ptr<iexporter> _addr_p) {
    ref iexporter p = ref _addr_p.val;

    return addr(new exportWriter(p:p));
}

private static ulong flush(this ptr<exportWriter> _addr_w) {
    ref exportWriter w = ref _addr_w.val;

    var off = uint64(w.p.data0.Len());
    io.Copy(_addr_w.p.data0, _addr_w.data);
    return off;
}

private static ulong typOff(this ptr<iexporter> _addr_p, ptr<types.Type> _addr_t) {
    ref iexporter p = ref _addr_p.val;
    ref types.Type t = ref _addr_t.val;

    var (off, ok) = p.typIndex[t];
    if (!ok) {
        var w = p.newWriter();
        w.doTyp(t);
        var rawOff = w.flush();
        if (@base.Flag.LowerV.val) {
            fmt.Printf("export: typ %v %v\n", rawOff, t);
        }
        off = predeclReserved + rawOff;
        p.typIndex[t] = off;

    }
    return off;

}

private static void startType(this ptr<exportWriter> _addr_w, itag k) {
    ref exportWriter w = ref _addr_w.val;

    w.data.uint64(uint64(k));
}

private static void doTyp(this ptr<exportWriter> _addr_w, ptr<types.Type> _addr_t) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Type t = ref _addr_t.val;

    if (t.Sym() != null) {
        if (t.Sym().Pkg == types.BuiltinPkg || t.Sym().Pkg == ir.Pkgs.Unsafe) {
            @base.Fatalf("builtin type missing from typIndex: %v", t);
        }
        w.startType(definedType);
        w.qualifiedIdent(t.Obj()._<ptr<ir.Name>>());
        return ;

    }

    if (t.Kind() == types.TPTR) 
        w.startType(pointerType);
        w.typ(t.Elem());
    else if (t.Kind() == types.TSLICE) 
        w.startType(sliceType);
        w.typ(t.Elem());
    else if (t.Kind() == types.TARRAY) 
        w.startType(arrayType);
        w.uint64(uint64(t.NumElem()));
        w.typ(t.Elem());
    else if (t.Kind() == types.TCHAN) 
        w.startType(chanType);
        w.uint64(uint64(t.ChanDir()));
        w.typ(t.Elem());
    else if (t.Kind() == types.TMAP) 
        w.startType(mapType);
        w.typ(t.Key());
        w.typ(t.Elem());
    else if (t.Kind() == types.TFUNC) 
        w.startType(signatureType);
        w.setPkg(t.Pkg(), true);
        w.signature(t);
    else if (t.Kind() == types.TSTRUCT) 
        w.startType(structType);
        w.setPkg(t.Pkg(), true);

        w.uint64(uint64(t.NumFields()));
        {
            var f__prev1 = f;

            foreach (var (_, __f) in t.FieldSlice()) {
                f = __f;
                w.pos(f.Pos);
                w.selector(f.Sym);
                w.typ(f.Type);
                w.@bool(f.Embedded != 0);
                w.@string(f.Note);
            }

            f = f__prev1;
        }
    else if (t.Kind() == types.TINTER) 
        slice<ptr<types.Field>> embeddeds = default;        slice<ptr<types.Field>> methods = default;

        foreach (var (_, m) in t.Methods().Slice()) {
            if (m.Sym != null) {
                methods = append(methods, m);
            }
            else
 {
                embeddeds = append(embeddeds, m);
            }

        }        w.startType(interfaceType);
        w.setPkg(t.Pkg(), true);

        w.uint64(uint64(len(embeddeds)));
        {
            var f__prev1 = f;

            foreach (var (_, __f) in embeddeds) {
                f = __f;
                w.pos(f.Pos);
                w.typ(f.Type);
            }

            f = f__prev1;
        }

        w.uint64(uint64(len(methods)));
        {
            var f__prev1 = f;

            foreach (var (_, __f) in methods) {
                f = __f;
                w.pos(f.Pos);
                w.selector(f.Sym);
                w.signature(f.Type);
            }

            f = f__prev1;
        }
    else 
        @base.Fatalf("unexpected type: %v", t);
    
}

private static void setPkg(this ptr<exportWriter> _addr_w, ptr<types.Pkg> _addr_pkg, bool write) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Pkg pkg = ref _addr_pkg.val;

    if (pkg == types.NoPkg) {
        @base.Fatalf("missing pkg");
    }
    if (write) {
        w.pkg(pkg);
    }
    w.currPkg = pkg;

}

private static void signature(this ptr<exportWriter> _addr_w, ptr<types.Type> _addr_t) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Type t = ref _addr_t.val;

    w.paramList(t.Params().FieldSlice());
    w.paramList(t.Results().FieldSlice());
    {
        var n = t.Params().NumFields();

        if (n > 0) {
            w.@bool(t.Params().Field(n - 1).IsDDD());
        }
    }

}

private static void paramList(this ptr<exportWriter> _addr_w, slice<ptr<types.Field>> fs) {
    ref exportWriter w = ref _addr_w.val;

    w.uint64(uint64(len(fs)));
    foreach (var (_, f) in fs) {
        w.param(f);
    }
}

private static void param(this ptr<exportWriter> _addr_w, ptr<types.Field> _addr_f) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Field f = ref _addr_f.val;

    w.pos(f.Pos);
    w.localIdent(types.OrigSym(f.Sym));
    w.typ(f.Type);
}

private static constant.Kind constTypeOf(ptr<types.Type> _addr_typ) {
    ref types.Type typ = ref _addr_typ.val;


    if (typ == types.UntypedInt || typ == types.UntypedRune) 
        return constant.Int;
    else if (typ == types.UntypedFloat) 
        return constant.Float;
    else if (typ == types.UntypedComplex) 
        return constant.Complex;
    
    if (typ.Kind() == types.TBOOL) 
        return constant.Bool;
    else if (typ.Kind() == types.TSTRING) 
        return constant.String;
    else if (typ.Kind() == types.TINT || typ.Kind() == types.TINT8 || typ.Kind() == types.TINT16 || typ.Kind() == types.TINT32 || typ.Kind() == types.TINT64 || typ.Kind() == types.TUINT || typ.Kind() == types.TUINT8 || typ.Kind() == types.TUINT16 || typ.Kind() == types.TUINT32 || typ.Kind() == types.TUINT64 || typ.Kind() == types.TUINTPTR) 
        return constant.Int;
    else if (typ.Kind() == types.TFLOAT32 || typ.Kind() == types.TFLOAT64) 
        return constant.Float;
    else if (typ.Kind() == types.TCOMPLEX64 || typ.Kind() == types.TCOMPLEX128) 
        return constant.Complex;
        @base.Fatalf("unexpected constant type: %v", typ);
    return 0;

}

private static void value(this ptr<exportWriter> _addr_w, ptr<types.Type> _addr_typ, constant.Value v) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Type typ = ref _addr_typ.val;

    ir.AssertValidTypeForConst(typ, v);
    w.typ(typ); 

    // Each type has only one admissible constant representation,
    // so we could type switch directly on v.U here. However,
    // switching on the type increases symmetry with import logic
    // and provides a useful consistency check.


    if (constTypeOf(_addr_typ) == constant.Bool) 
        w.@bool(constant.BoolVal(v));
    else if (constTypeOf(_addr_typ) == constant.String) 
        w.@string(constant.StringVal(v));
    else if (constTypeOf(_addr_typ) == constant.Int) 
        w.mpint(v, typ);
    else if (constTypeOf(_addr_typ) == constant.Float) 
        w.mpfloat(v, typ);
    else if (constTypeOf(_addr_typ) == constant.Complex) 
        w.mpfloat(constant.Real(v), typ);
        w.mpfloat(constant.Imag(v), typ);
    
}

private static (bool, nuint) intSize(ptr<types.Type> _addr_typ) {
    bool signed = default;
    nuint maxBytes = default;
    ref types.Type typ = ref _addr_typ.val;

    if (typ.IsUntyped()) {
        return (true, ir.ConstPrec / 8);
    }

    if (typ.Kind() == types.TFLOAT32 || typ.Kind() == types.TCOMPLEX64) 
        return (true, 3);
    else if (typ.Kind() == types.TFLOAT64 || typ.Kind() == types.TCOMPLEX128) 
        return (true, 7);
        signed = typ.IsSigned();
    maxBytes = uint(typ.Size()); 

    // The go/types API doesn't expose sizes to importers, so they
    // don't know how big these types are.

    if (typ.Kind() == types.TINT || typ.Kind() == types.TUINT || typ.Kind() == types.TUINTPTR) 
        maxBytes = 8;
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
private static void mpint(this ptr<exportWriter> _addr_w, constant.Value x, ptr<types.Type> _addr_typ) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Type typ = ref _addr_typ.val;

    var (signed, maxBytes) = intSize(_addr_typ);

    var negative = constant.Sign(x) < 0;
    if (!signed && negative) {
        @base.Fatalf("negative unsigned integer; type %v, value %v", typ, x);
    }
    var b = constant.Bytes(x); // little endian
    {
        nint i = 0;
        var j = len(b) - 1;

        while (i < j) {
            (b[i], b[j]) = (b[j], b[i]);            (i, j) = (i + 1, j - 1);
        }
    }

    if (len(b) > 0 && b[0] == 0) {
        @base.Fatalf("leading zeros");
    }
    if (uint(len(b)) > maxBytes) {
        @base.Fatalf("bad mpint length: %d > %d (type %v, value %v)", len(b), maxBytes, typ, x);
    }
    nint maxSmall = 256 - maxBytes;
    if (signed) {
        maxSmall = 256 - 2 * maxBytes;
    }
    if (maxBytes == 1) {
        maxSmall = 256;
    }
    if (len(b) <= 1) {
        nuint ux = default;
        if (len(b) == 1) {
            ux = uint(b[0]);
        }
        if (signed) {
            ux<<=1;
            if (negative) {
                ux--;
            }
        }
        if (ux < maxSmall) {
            w.data.WriteByte(byte(ux));
            return ;
        }
    }
    nint n = 256 - uint(len(b));
    if (signed) {
        n = 256 - 2 * uint(len(b));
        if (negative) {
            n |= 1;
        }
    }
    if (n < maxSmall || n >= 256) {
        @base.Fatalf("encoding mistake: %d, %v, %v => %d", len(b), signed, negative, n);
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
private static void mpfloat(this ptr<exportWriter> _addr_w, constant.Value v, ptr<types.Type> _addr_typ) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Type typ = ref _addr_typ.val;

    var f = ir.BigFloat(v);
    if (f.IsInf()) {
        @base.Fatalf("infinite constant");
    }
    ref big.Float mant = ref heap(out ptr<big.Float> _addr_mant);
    var exp = int64(f.MantExp(_addr_mant)); 

    // Scale so that mant is an integer.
    var prec = mant.MinPrec();
    mant.SetMantExp(_addr_mant, int(prec));
    exp -= int64(prec);

    var (manti, acc) = mant.Int(null);
    if (acc != big.Exact) {
        @base.Fatalf("mantissa scaling failed for %f (%s)", f, acc);
    }
    w.mpint(constant.Make(manti), typ);
    if (manti.Sign() != 0) {
        w.int64(exp);
    }
}

private static void mprat(this ptr<exportWriter> _addr_w, constant.Value v) {
    ref exportWriter w = ref _addr_w.val;

    ptr<big.Rat> (r, ok) = constant.Val(v)._<ptr<big.Rat>>();
    if (!w.@bool(ok)) {
        return ;
    }
    w.@string(r.String());

}

private static bool @bool(this ptr<exportWriter> _addr_w, bool b) {
    ref exportWriter w = ref _addr_w.val;

    ulong x = default;
    if (b) {
        x = 1;
    }
    w.uint64(x);
    return b;

}

private static void int64(this ptr<exportWriter> _addr_w, long x) {
    ref exportWriter w = ref _addr_w.val;

    w.data.int64(x);
}
private static void uint64(this ptr<exportWriter> _addr_w, ulong x) {
    ref exportWriter w = ref _addr_w.val;

    w.data.uint64(x);
}
private static void @string(this ptr<exportWriter> _addr_w, @string s) {
    ref exportWriter w = ref _addr_w.val;

    w.uint64(w.p.stringOff(s));
}

// Compiler-specific extensions.

private static void constExt(this ptr<exportWriter> _addr_w, ptr<ir.Name> _addr_n) {
    ref exportWriter w = ref _addr_w.val;
    ref ir.Name n = ref _addr_n.val;
 
    // Internally, we now represent untyped float and complex
    // constants with infinite-precision rational numbers using
    // go/constant, but the "public" export data format known to
    // gcimporter only supports 512-bit floating point constants.
    // In case rationals turn out to be a bad idea and we want to
    // switch back to fixed-precision constants, for now we
    // continue writing out the 512-bit truncation in the public
    // data section, and write the exact, rational constant in the
    // compiler's extension data. Also, we only need to worry
    // about exporting rationals for declared constants, because
    // constants that appear in an expression will already have
    // been coerced to a concrete, fixed-precision type.
    //
    // Eventually, assuming we stick with using rationals, we
    // should bump iexportVersion to support rationals, and do the
    // whole gcimporter update song-and-dance.
    //
    // TODO(mdempsky): Prepare vocals for that.


    if (n.Type() == types.UntypedFloat) 
        w.mprat(n.Val());
    else if (n.Type() == types.UntypedComplex) 
        var v = n.Val();
        w.mprat(constant.Real(v));
        w.mprat(constant.Imag(v));
    
}

private static void varExt(this ptr<exportWriter> _addr_w, ptr<ir.Name> _addr_n) {
    ref exportWriter w = ref _addr_w.val;
    ref ir.Name n = ref _addr_n.val;

    w.linkname(n.Sym());
    w.symIdx(n.Sym());
}

private static void funcExt(this ptr<exportWriter> _addr_w, ptr<ir.Name> _addr_n) {
    ref exportWriter w = ref _addr_w.val;
    ref ir.Name n = ref _addr_n.val;

    w.linkname(n.Sym());
    w.symIdx(n.Sym()); 

    // Record definition ABI so cross-ABI calls can be direct.
    // This is important for the performance of calling some
    // common functions implemented in assembly (e.g., bytealg).
    w.uint64(uint64(n.Func.ABI));

    w.uint64(uint64(n.Func.Pragma)); 

    // Escape analysis.
    foreach (var (_, fs) in _addr_types.RecvsParams) {
        foreach (var (_, f) in fs(n.Type()).FieldSlice()) {
            w.@string(f.Note);
        }
    }    if (n.Func.Inl != null) {
        w.uint64(1 + uint64(n.Func.Inl.Cost));
        if (n.Func.ExportInline()) {
            w.p.doInline(n);
        }
        w.pos(n.Func.Endlineno);

    }
    else
 {
        w.uint64(0);
    }
}

private static void methExt(this ptr<exportWriter> _addr_w, ptr<types.Field> _addr_m) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Field m = ref _addr_m.val;

    w.@bool(m.Nointerface());
    w.funcExt(m.Nname._<ptr<ir.Name>>());
}

private static void linkname(this ptr<exportWriter> _addr_w, ptr<types.Sym> _addr_s) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Sym s = ref _addr_s.val;

    w.@string(s.Linkname);
}

private static void symIdx(this ptr<exportWriter> _addr_w, ptr<types.Sym> _addr_s) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Sym s = ref _addr_s.val;

    var lsym = s.Linksym();
    if (lsym.PkgIdx > goobj.PkgIdxSelf || (lsym.PkgIdx == goobj.PkgIdxInvalid && !lsym.Indexed()) || s.Linkname != "") { 
        // Don't export index for non-package symbols, linkname'd symbols,
        // and symbols without an index. They can only be referenced by
        // name.
        w.int64(-1);

    }
    else
 { 
        // For a defined symbol, export its index.
        // For re-exporting an imported symbol, pass its index through.
        w.int64(int64(lsym.SymIdx));

    }
}

private static void typeExt(this ptr<exportWriter> _addr_w, ptr<types.Type> _addr_t) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Type t = ref _addr_t.val;
 
    // Export whether this type is marked notinheap.
    w.@bool(t.NotInHeap()); 
    // For type T, export the index of type descriptor symbols of T and *T.
    {
        var (i, ok) = typeSymIdx[t];

        if (ok) {
            w.int64(i[0]);
            w.int64(i[1]);
            return ;
        }
    }

    w.symIdx(types.TypeSym(t));
    w.symIdx(types.TypeSym(t.PtrTo()));

}

// Inline bodies.

private static void writeNames(this ptr<exportWriter> _addr_w, slice<ptr<ir.Name>> dcl) {
    ref exportWriter w = ref _addr_w.val;

    w.int64(int64(len(dcl)));
    foreach (var (i, n) in dcl) {
        w.pos(n.Pos());
        w.localIdent(n.Sym());
        w.typ(n.Type());
        w.dclIndex[n] = w.maxDclIndex + i;
    }    w.maxDclIndex += len(dcl);
}

private static void funcBody(this ptr<exportWriter> _addr_w, ptr<ir.Func> _addr_fn) {
    ref exportWriter w = ref _addr_w.val;
    ref ir.Func fn = ref _addr_fn.val;
 
    //fmt.Printf("Exporting %s\n", fn.Nname.Sym().Name)
    w.writeNames(fn.Inl.Dcl);

    w.stmtList(fn.Inl.Body);

}

private static void stmtList(this ptr<exportWriter> _addr_w, slice<ir.Node> list) {
    ref exportWriter w = ref _addr_w.val;

    foreach (var (_, n) in list) {
        w.node(n);
    }    w.op(ir.OEND);
}

private static void node(this ptr<exportWriter> _addr_w, ir.Node n) {
    ref exportWriter w = ref _addr_w.val;

    if (ir.OpPrec[n.Op()] < 0) {
        w.stmt(n);
    }
    else
 {
        w.expr(n);
    }
}

// Caution: stmt will emit more than one node for statement nodes n that have a non-empty
// n.Ninit and where n cannot have a natural init section (such as in "if", "for", etc.).
private static void stmt(this ptr<exportWriter> _addr_w, ir.Node n) {
    ref exportWriter w = ref _addr_w.val;

    if (len(n.Init()) > 0 && !ir.StmtWithInit(n.Op())) { 
        // can't use stmtList here since we don't want the final OEND
        {
            var n__prev1 = n;

            foreach (var (_, __n) in n.Init()) {
                n = __n;
                w.stmt(n);
            }

            n = n__prev1;
        }
    }

    if (n.Op() == ir.OBLOCK) 
        // No OBLOCK in export data.
        // Inline content into this statement list,
        // like the init list above.
        // (At the moment neither the parser nor the typechecker
        // generate OBLOCK nodes except to denote an empty
        // function body, although that may change.)
        ptr<ir.BlockStmt> n = n._<ptr<ir.BlockStmt>>();
        {
            var n__prev1 = n;

            foreach (var (_, __n) in n.List) {
                n = __n;
                w.stmt(n);
            }

            n = n__prev1;
        }
    else if (n.Op() == ir.ODCL) 
        n = n._<ptr<ir.Decl>>();
        if (ir.IsBlank(n.X)) {
            return ; // blank declarations not useful to importers
        }
        w.op(ir.ODCL);
        w.localName(n.X);
    else if (n.Op() == ir.OAS) 
        // Don't export "v = <N>" initializing statements, hope they're always
        // preceded by the DCL which will be re-parsed and typecheck to reproduce
        // the "v = <N>" again.
        n = n._<ptr<ir.AssignStmt>>();
        if (n.Y != null) {
            w.op(ir.OAS);
            w.pos(n.Pos());
            w.expr(n.X);
            w.expr(n.Y);
        }
    else if (n.Op() == ir.OASOP) 
        n = n._<ptr<ir.AssignOpStmt>>();
        w.op(ir.OASOP);
        w.pos(n.Pos());
        w.op(n.AsOp);
        w.expr(n.X);
        if (w.@bool(!n.IncDec)) {
            w.expr(n.Y);
        }
    else if (n.Op() == ir.OAS2 || n.Op() == ir.OAS2DOTTYPE || n.Op() == ir.OAS2FUNC || n.Op() == ir.OAS2MAPR || n.Op() == ir.OAS2RECV) 
        n = n._<ptr<ir.AssignListStmt>>();
        if (go117ExportTypes) {
            w.op(n.Op());
        }
        else
 {
            w.op(ir.OAS2);
        }
        w.pos(n.Pos());
        w.exprList(n.Lhs);
        w.exprList(n.Rhs);
    else if (n.Op() == ir.ORETURN) 
        n = n._<ptr<ir.ReturnStmt>>();
        w.op(ir.ORETURN);
        w.pos(n.Pos());
        w.exprList(n.Results); 

        // case ORETJMP:
        //     unreachable - generated by compiler for trampoline routines
    else if (n.Op() == ir.OGO || n.Op() == ir.ODEFER) 
        n = n._<ptr<ir.GoDeferStmt>>();
        w.op(n.Op());
        w.pos(n.Pos());
        w.expr(n.Call);
    else if (n.Op() == ir.OIF) 
        n = n._<ptr<ir.IfStmt>>();
        w.op(ir.OIF);
        w.pos(n.Pos());
        w.stmtList(n.Init());
        w.expr(n.Cond);
        w.stmtList(n.Body);
        w.stmtList(n.Else);
    else if (n.Op() == ir.OFOR) 
        n = n._<ptr<ir.ForStmt>>();
        w.op(ir.OFOR);
        w.pos(n.Pos());
        w.stmtList(n.Init());
        w.exprsOrNil(n.Cond, n.Post);
        w.stmtList(n.Body);
    else if (n.Op() == ir.ORANGE) 
        n = n._<ptr<ir.RangeStmt>>();
        w.op(ir.ORANGE);
        w.pos(n.Pos());
        w.exprsOrNil(n.Key, n.Value);
        w.expr(n.X);
        w.stmtList(n.Body);
    else if (n.Op() == ir.OSELECT) 
        n = n._<ptr<ir.SelectStmt>>();
        w.op(n.Op());
        w.pos(n.Pos());
        w.stmtList(n.Init());
        w.commList(n.Cases);
    else if (n.Op() == ir.OSWITCH) 
        n = n._<ptr<ir.SwitchStmt>>();
        w.op(n.Op());
        w.pos(n.Pos());
        w.stmtList(n.Init());
        w.exprsOrNil(n.Tag, null);
        w.caseList(n.Cases, isNamedTypeSwitch(n.Tag)); 

        // case OCASE:
        //    handled by caseList
    else if (n.Op() == ir.OFALL) 
        n = n._<ptr<ir.BranchStmt>>();
        w.op(ir.OFALL);
        w.pos(n.Pos());
    else if (n.Op() == ir.OBREAK || n.Op() == ir.OCONTINUE || n.Op() == ir.OGOTO || n.Op() == ir.OLABEL) 
        w.op(n.Op());
        w.pos(n.Pos());
        @string label = "";
        {
            var sym = n.Sym();

            if (sym != null) {
                label = sym.Name;
            }

        }

        w.@string(label);
    else 
        @base.Fatalf("exporter: CANNOT EXPORT: %v\nPlease notify gri@\n", n.Op());
    
}

private static bool isNamedTypeSwitch(ir.Node x) {
    ptr<ir.TypeSwitchGuard> (guard, ok) = x._<ptr<ir.TypeSwitchGuard>>();
    return ok && guard.Tag != null;
}

private static void caseList(this ptr<exportWriter> _addr_w, slice<ptr<ir.CaseClause>> cases, bool namedTypeSwitch) {
    ref exportWriter w = ref _addr_w.val;

    w.uint64(uint64(len(cases)));
    foreach (var (_, cas) in cases) {
        w.pos(cas.Pos());
        w.stmtList(cas.List);
        if (namedTypeSwitch) {
            w.localName(cas.Var);
        }
        w.stmtList(cas.Body);

    }
}

private static void commList(this ptr<exportWriter> _addr_w, slice<ptr<ir.CommClause>> cases) {
    ref exportWriter w = ref _addr_w.val;

    w.uint64(uint64(len(cases)));
    foreach (var (_, cas) in cases) {
        w.pos(cas.Pos());
        w.node(cas.Comm);
        w.stmtList(cas.Body);
    }
}

private static void exprList(this ptr<exportWriter> _addr_w, ir.Nodes list) {
    ref exportWriter w = ref _addr_w.val;

    foreach (var (_, n) in list) {
        w.expr(n);
    }    w.op(ir.OEND);
}

private static ir.Node simplifyForExport(ir.Node n) {

    if (n.Op() == ir.OPAREN) 
        ptr<ir.ParenExpr> n = n._<ptr<ir.ParenExpr>>();
        return simplifyForExport(n.X);
        return n;

}

private static void expr(this ptr<exportWriter> _addr_w, ir.Node n) {
    ref exportWriter w = ref _addr_w.val;

    n = simplifyForExport(n);

    // expressions
    // (somewhat closely following the structure of exprfmt in fmt.go)
    if (n.Op() == ir.ONIL) 
        ptr<ir.NilExpr> n = n._<ptr<ir.NilExpr>>();
        if (!n.Type().HasNil()) {
            @base.Fatalf("unexpected type for nil: %v", n.Type());
        }
        w.op(ir.ONIL);
        w.pos(n.Pos());
        w.typ(n.Type());
    else if (n.Op() == ir.OLITERAL) 
        w.op(ir.OLITERAL);
        w.pos(n.Pos());
        w.value(n.Type(), n.Val());
    else if (n.Op() == ir.ONAME) 
        // Package scope name.
        n = n._<ptr<ir.Name>>();
        if ((n.Class == ir.PEXTERN || n.Class == ir.PFUNC) && !ir.IsBlank(n)) {
            w.op(ir.ONONAME);
            w.qualifiedIdent(n);
            if (go117ExportTypes) {
                w.typ(n.Type());
            }
            break;
        }
        w.op(ir.ONAME);
        w.localName(n); 

        // case OPACK, ONONAME:
        //     should have been resolved by typechecking - handled by default case
    else if (n.Op() == ir.OTYPE) 
        w.op(ir.OTYPE);
        w.typ(n.Type());
    else if (n.Op() == ir.OTYPESW) 
        n = n._<ptr<ir.TypeSwitchGuard>>();
        w.op(ir.OTYPESW);
        w.pos(n.Pos());
        ptr<types.Sym> s;
        if (n.Tag != null) {
            if (n.Tag.Op() != ir.ONONAME) {
                @base.Fatalf("expected ONONAME, got %v", n.Tag);
            }
            s = n.Tag.Sym();
        }
        w.localIdent(s); // declared pseudo-variable, if any
        w.expr(n.X); 

        // case OTARRAY, OTMAP, OTCHAN, OTSTRUCT, OTINTER, OTFUNC:
        //     should have been resolved by typechecking - handled by default case
    else if (n.Op() == ir.OCLOSURE) 
        n = n._<ptr<ir.ClosureExpr>>();
        w.op(ir.OCLOSURE);
        w.pos(n.Pos());
        w.signature(n.Type()); 

        // Write out id for the Outer of each conditional variable. The
        // conditional variable itself for this closure will be re-created
        // during import.
        w.int64(int64(len(n.Func.ClosureVars)));
        foreach (var (i, cv) in n.Func.ClosureVars) {
            w.pos(cv.Pos());
            w.localName(cv.Outer); 
            // Closure variable (which will be re-created during
            // import) is given via a negative id, starting at -2,
            // which is used to refer to it later in the function
            // during export. -1 represents blanks.
            w.dclIndex[cv] = -(i + 2) - w.maxClosureVarIndex;

        }        w.maxClosureVarIndex += len(n.Func.ClosureVars); 

        // like w.funcBody(n.Func), but not for .Inl
        w.writeNames(n.Func.Dcl);
        w.stmtList(n.Func.Body); 

        // case OCOMPLIT:
        //     should have been resolved by typechecking - handled by default case
    else if (n.Op() == ir.OPTRLIT) 
        n = n._<ptr<ir.AddrExpr>>();
        if (go117ExportTypes) {
            w.op(ir.OPTRLIT);
        }
        else
 {
            w.op(ir.OADDR);
        }
        w.pos(n.Pos());
        w.expr(n.X);
        if (go117ExportTypes) {
            w.typ(n.Type());
        }
    else if (n.Op() == ir.OSTRUCTLIT) 
        n = n._<ptr<ir.CompLitExpr>>();
        w.op(ir.OSTRUCTLIT);
        w.pos(n.Pos());
        w.typ(n.Type());
        w.fieldList(n.List); // special handling of field names
    else if (n.Op() == ir.OARRAYLIT || n.Op() == ir.OSLICELIT || n.Op() == ir.OMAPLIT) 
        n = n._<ptr<ir.CompLitExpr>>();
        if (go117ExportTypes) {
            w.op(n.Op());
        }
        else
 {
            w.op(ir.OCOMPLIT);
        }
        w.pos(n.Pos());
        w.typ(n.Type());
        w.exprList(n.List);
        if (go117ExportTypes && n.Op() == ir.OSLICELIT) {
            w.uint64(uint64(n.Len));
        }
    else if (n.Op() == ir.OKEY) 
        n = n._<ptr<ir.KeyExpr>>();
        w.op(ir.OKEY);
        w.pos(n.Pos());
        w.expr(n.Key);
        w.expr(n.Value); 

        // case OSTRUCTKEY:
        //    unreachable - handled in case OSTRUCTLIT by elemList
    else if (n.Op() == ir.OXDOT || n.Op() == ir.ODOT || n.Op() == ir.ODOTPTR || n.Op() == ir.ODOTINTER || n.Op() == ir.ODOTMETH || n.Op() == ir.OCALLPART || n.Op() == ir.OMETHEXPR) 
        n = n._<ptr<ir.SelectorExpr>>();
        if (go117ExportTypes) {
            if (n.Op() == ir.OXDOT) {
                @base.Fatalf("shouldn't encounter XDOT  in new exporter");
            }
            w.op(n.Op());
        }
        else
 {
            w.op(ir.OXDOT);
        }
        w.pos(n.Pos());
        w.expr(n.X);
        w.exoticSelector(n.Sel);
        if (go117ExportTypes) {
            w.exoticType(n.Type());
            if (n.Op() == ir.ODOT || n.Op() == ir.ODOTPTR || n.Op() == ir.ODOTINTER) {
                w.exoticField(n.Selection);
            } 
            // n.Selection is not required for OMETHEXPR, ODOTMETH, and OCALLPART. It will
            // be reconstructed during import.
        }
    else if (n.Op() == ir.ODOTTYPE || n.Op() == ir.ODOTTYPE2) 
        n = n._<ptr<ir.TypeAssertExpr>>();
        if (go117ExportTypes) {
            w.op(n.Op());
        }
        else
 {
            w.op(ir.ODOTTYPE);
        }
        w.pos(n.Pos());
        w.expr(n.X);
        w.typ(n.Type());
    else if (n.Op() == ir.OINDEX || n.Op() == ir.OINDEXMAP) 
        n = n._<ptr<ir.IndexExpr>>();
        if (go117ExportTypes) {
            w.op(n.Op());
        }
        else
 {
            w.op(ir.OINDEX);
        }
        w.pos(n.Pos());
        w.expr(n.X);
        w.expr(n.Index);
        if (go117ExportTypes) {
            w.typ(n.Type());
            if (n.Op() == ir.OINDEXMAP) {
                w.@bool(n.Assigned);
            }
        }
    else if (n.Op() == ir.OSLICE || n.Op() == ir.OSLICESTR || n.Op() == ir.OSLICEARR) 
        n = n._<ptr<ir.SliceExpr>>();
        if (go117ExportTypes) {
            w.op(n.Op());
        }
        else
 {
            w.op(ir.OSLICE);
        }
        w.pos(n.Pos());
        w.expr(n.X);
        w.exprsOrNil(n.Low, n.High);
        if (go117ExportTypes) {
            w.typ(n.Type());
        }
    else if (n.Op() == ir.OSLICE3 || n.Op() == ir.OSLICE3ARR) 
        n = n._<ptr<ir.SliceExpr>>();
        if (go117ExportTypes) {
            w.op(n.Op());
        }
        else
 {
            w.op(ir.OSLICE3);
        }
        w.pos(n.Pos());
        w.expr(n.X);
        w.exprsOrNil(n.Low, n.High);
        w.expr(n.Max);
        if (go117ExportTypes) {
            w.typ(n.Type());
        }
    else if (n.Op() == ir.OCOPY || n.Op() == ir.OCOMPLEX || n.Op() == ir.OUNSAFEADD || n.Op() == ir.OUNSAFESLICE) 
        // treated like other builtin calls (see e.g., OREAL)
        n = n._<ptr<ir.BinaryExpr>>();
        w.op(n.Op());
        w.pos(n.Pos());
        w.expr(n.X);
        w.expr(n.Y);
        if (go117ExportTypes) {
            w.typ(n.Type());
        }
        else
 {
            w.op(ir.OEND);
        }
    else if (n.Op() == ir.OCONV || n.Op() == ir.OCONVIFACE || n.Op() == ir.OCONVNOP || n.Op() == ir.OBYTES2STR || n.Op() == ir.ORUNES2STR || n.Op() == ir.OSTR2BYTES || n.Op() == ir.OSTR2RUNES || n.Op() == ir.ORUNESTR || n.Op() == ir.OSLICE2ARRPTR) 
        n = n._<ptr<ir.ConvExpr>>();
        if (go117ExportTypes) {
            w.op(n.Op());
        }
        else
 {
            w.op(ir.OCONV);
        }
        w.pos(n.Pos());
        w.typ(n.Type());
        w.expr(n.X);
    else if (n.Op() == ir.OREAL || n.Op() == ir.OIMAG || n.Op() == ir.OCAP || n.Op() == ir.OCLOSE || n.Op() == ir.OLEN || n.Op() == ir.ONEW || n.Op() == ir.OPANIC) 
        n = n._<ptr<ir.UnaryExpr>>();
        w.op(n.Op());
        w.pos(n.Pos());
        w.expr(n.X);
        if (go117ExportTypes) {
            if (n.Op() != ir.OPANIC) {
                w.typ(n.Type());
            }
        }
        else
 {
            w.op(ir.OEND);
        }
    else if (n.Op() == ir.OAPPEND || n.Op() == ir.ODELETE || n.Op() == ir.ORECOVER || n.Op() == ir.OPRINT || n.Op() == ir.OPRINTN) 
        n = n._<ptr<ir.CallExpr>>();
        w.op(n.Op());
        w.pos(n.Pos());
        w.exprList(n.Args); // emits terminating OEND
        // only append() calls may contain '...' arguments
        if (n.Op() == ir.OAPPEND) {
            w.@bool(n.IsDDD);
        }
        else if (n.IsDDD) {
            @base.Fatalf("exporter: unexpected '...' with %v call", n.Op());
        }
        if (go117ExportTypes) {
            if (n.Op() != ir.ODELETE && n.Op() != ir.OPRINT && n.Op() != ir.OPRINTN) {
                w.typ(n.Type());
            }
        }
    else if (n.Op() == ir.OCALL || n.Op() == ir.OCALLFUNC || n.Op() == ir.OCALLMETH || n.Op() == ir.OCALLINTER || n.Op() == ir.OGETG) 
        n = n._<ptr<ir.CallExpr>>();
        if (go117ExportTypes) {
            w.op(n.Op());
        }
        else
 {
            w.op(ir.OCALL);
        }
        w.pos(n.Pos());
        w.stmtList(n.Init());
        w.expr(n.X);
        w.exprList(n.Args);
        w.@bool(n.IsDDD);
        if (go117ExportTypes) {
            w.exoticType(n.Type());
            w.uint64(uint64(n.Use));
        }
    else if (n.Op() == ir.OMAKEMAP || n.Op() == ir.OMAKECHAN || n.Op() == ir.OMAKESLICE) 
        n = n._<ptr<ir.MakeExpr>>();
        w.op(n.Op()); // must keep separate from OMAKE for importer
        w.pos(n.Pos());
        w.typ(n.Type());

        if (n.Cap != null) 
            w.expr(n.Len);
            w.expr(n.Cap);
            w.op(ir.OEND);
        else if (n.Len != null && (n.Op() == ir.OMAKESLICE || !n.Len.Type().IsUntyped())) 
            // Note: the extra conditional exists because make(T) for
            // T a map or chan type, gets an untyped zero added as
            // an argument. Don't serialize that argument here.
            w.expr(n.Len);
            w.op(ir.OEND);
        else if (n.Len != null && go117ExportTypes) 
            w.expr(n.Len);
            w.op(ir.OEND);
        else 
            // empty list
            w.op(ir.OEND);
        // unary expressions
    else if (n.Op() == ir.OPLUS || n.Op() == ir.ONEG || n.Op() == ir.OBITNOT || n.Op() == ir.ONOT || n.Op() == ir.ORECV) 
        n = n._<ptr<ir.UnaryExpr>>();
        w.op(n.Op());
        w.pos(n.Pos());
        w.expr(n.X);
        if (go117ExportTypes) {
            w.typ(n.Type());
        }
    else if (n.Op() == ir.OADDR) 
        n = n._<ptr<ir.AddrExpr>>();
        w.op(n.Op());
        w.pos(n.Pos());
        w.expr(n.X);
        if (go117ExportTypes) {
            w.typ(n.Type());
        }
    else if (n.Op() == ir.ODEREF) 
        n = n._<ptr<ir.StarExpr>>();
        w.op(n.Op());
        w.pos(n.Pos());
        w.expr(n.X);
        if (go117ExportTypes) {
            w.typ(n.Type());
        }
    else if (n.Op() == ir.OSEND) 
        n = n._<ptr<ir.SendStmt>>();
        w.op(n.Op());
        w.pos(n.Pos());
        w.expr(n.Chan);
        w.expr(n.Value); 

        // binary expressions
    else if (n.Op() == ir.OADD || n.Op() == ir.OAND || n.Op() == ir.OANDNOT || n.Op() == ir.ODIV || n.Op() == ir.OEQ || n.Op() == ir.OGE || n.Op() == ir.OGT || n.Op() == ir.OLE || n.Op() == ir.OLT || n.Op() == ir.OLSH || n.Op() == ir.OMOD || n.Op() == ir.OMUL || n.Op() == ir.ONE || n.Op() == ir.OOR || n.Op() == ir.ORSH || n.Op() == ir.OSUB || n.Op() == ir.OXOR) 
        n = n._<ptr<ir.BinaryExpr>>();
        w.op(n.Op());
        w.pos(n.Pos());
        w.expr(n.X);
        w.expr(n.Y);
        if (go117ExportTypes) {
            w.typ(n.Type());
        }
    else if (n.Op() == ir.OANDAND || n.Op() == ir.OOROR) 
        n = n._<ptr<ir.LogicalExpr>>();
        w.op(n.Op());
        w.pos(n.Pos());
        w.expr(n.X);
        w.expr(n.Y);
        if (go117ExportTypes) {
            w.typ(n.Type());
        }
    else if (n.Op() == ir.OADDSTR) 
        n = n._<ptr<ir.AddStringExpr>>();
        w.op(ir.OADDSTR);
        w.pos(n.Pos());
        w.exprList(n.List);
        if (go117ExportTypes) {
            w.typ(n.Type());
        }
    else if (n.Op() == ir.ODCLCONST)     else 
        @base.Fatalf("cannot export %v (%d) node\n" + "\t==> please file an issue and assign to gri@", n.Op(), int(n.Op()));
    
}

private static void op(this ptr<exportWriter> _addr_w, ir.Op op) {
    ref exportWriter w = ref _addr_w.val;

    if (debug) {
        w.uint64(magic);
    }
    w.uint64(uint64(op));

}

private static void exprsOrNil(this ptr<exportWriter> _addr_w, ir.Node a, ir.Node b) {
    ref exportWriter w = ref _addr_w.val;

    nint ab = 0;
    if (a != null) {
        ab |= 1;
    }
    if (b != null) {
        ab |= 2;
    }
    w.uint64(uint64(ab));
    if (ab & 1 != 0) {
        w.expr(a);
    }
    if (ab & 2 != 0) {
        w.node(b);
    }
}

private static void fieldList(this ptr<exportWriter> _addr_w, ir.Nodes list) {
    ref exportWriter w = ref _addr_w.val;

    w.uint64(uint64(len(list)));
    {
        var n__prev1 = n;

        foreach (var (_, __n) in list) {
            n = __n;
            ptr<ir.StructKeyExpr> n = n._<ptr<ir.StructKeyExpr>>();
            w.pos(n.Pos());
            w.selector(n.Field);
            w.expr(n.Value);
            if (go117ExportTypes) {
                w.uint64(uint64(n.Offset));
            }
        }
        n = n__prev1;
    }
}

private static void localName(this ptr<exportWriter> _addr_w, ptr<ir.Name> _addr_n) {
    ref exportWriter w = ref _addr_w.val;
    ref ir.Name n = ref _addr_n.val;

    if (ir.IsBlank(n)) {
        w.int64(-1);
        return ;
    }
    var (i, ok) = w.dclIndex[n];
    if (!ok) {
        @base.FatalfAt(n.Pos(), "missing from dclIndex: %+v", n);
    }
    w.int64(int64(i));

}

private static void localIdent(this ptr<exportWriter> _addr_w, ptr<types.Sym> _addr_s) {
    ref exportWriter w = ref _addr_w.val;
    ref types.Sym s = ref _addr_s.val;

    if (w.currPkg == null) {
        @base.Fatalf("missing currPkg");
    }
    if (s == null) {
        w.@string("");
        return ;
    }
    var name = s.Name;
    if (name == "_") {
        w.@string("_");
        return ;
    }
    {
        var i = strings.LastIndex(name, ".");

        if (i >= 0 && !strings.HasPrefix(name, ".autotmp_")) {
            @base.Fatalf("unexpected dot in identifier: %v", name);
        }
    }


    if (s.Pkg != w.currPkg) {
        @base.Fatalf("weird package in name: %v => %v from %q, not %q", s, name, s.Pkg.Path, w.currPkg.Path);
    }
    w.@string(name);

}

private partial struct intWriter {
    public ref bytes.Buffer Buffer => ref Buffer_val;
}

private static void int64(this ptr<intWriter> _addr_w, long x) {
    ref intWriter w = ref _addr_w.val;

    array<byte> buf = new array<byte>(binary.MaxVarintLen64);
    var n = binary.PutVarint(buf[..], x);
    w.Write(buf[..(int)n]);
}

private static void uint64(this ptr<intWriter> _addr_w, ulong x) {
    ref intWriter w = ref _addr_w.val;

    array<byte> buf = new array<byte>(binary.MaxVarintLen64);
    var n = binary.PutUvarint(buf[..], x);
    w.Write(buf[..(int)n]);
}

// If go117ExportTypes is true, then we write type information when
// exporting function bodies, so those function bodies don't need to
// be re-typechecked on import.
// This flag adds some other info to the serialized stream as well
// which was previously recomputed during typechecking, like
// specializing opcodes (e.g. OXDOT to ODOTPTR) and ancillary
// information (e.g. length field for OSLICELIT).
private static readonly var go117ExportTypes = true;

public static readonly var Go117ExportTypes = go117ExportTypes;


} // end typecheck_package
