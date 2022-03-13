// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2022 March 13 06:27:55 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\gc\obj.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using objw = cmd.compile.@internal.objw_package;
using reflectdata = cmd.compile.@internal.reflectdata_package;
using staticdata = cmd.compile.@internal.staticdata_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;
using archive = cmd.@internal.archive_package;
using bio = cmd.@internal.bio_package;
using obj = cmd.@internal.obj_package;
using objabi = cmd.@internal.objabi_package;
using json = encoding.json_package;
using fmt = fmt_package;


// These modes say which kind of object file to generate.
// The default use of the toolchain is to set both bits,
// generating a combined compiler+linker object, one that
// serves to describe the package to both the compiler and the linker.
// In fact the compiler and linker read nearly disjoint sections of
// that file, though, so in a distributed build setting it can be more
// efficient to split the output into two files, supplying the compiler
// object only to future compilations and the linker object only to
// future links.
//
// By default a combined object is written, but if -linkobj is specified
// on the command line then the default -o output is a compiler object
// and the -linkobj output is a linker object.

public static partial class gc_package {

private static readonly nint modeCompilerObj = 1 << (int)(iota);
private static readonly var modeLinkerObj = 0;

private static void dumpobj() {
    if (@base.Flag.LinkObj == "") {
        dumpobj1(@base.Flag.LowerO, modeCompilerObj | modeLinkerObj);
        return ;
    }
    dumpobj1(@base.Flag.LowerO, modeCompilerObj);
    dumpobj1(@base.Flag.LinkObj, modeLinkerObj);
}

private static void dumpobj1(@string outfile, nint mode) => func((defer, _, _) => {
    var (bout, err) = bio.Create(outfile);
    if (err != null) {
        @base.FlushErrors();
        fmt.Printf("can't create %s: %v\n", outfile, err);
        @base.ErrorExit();
    }
    defer(bout.Close());
    bout.WriteString("!<arch>\n");

    if (mode & modeCompilerObj != 0) {
        var start = startArchiveEntry(_addr_bout);
        dumpCompilerObj(_addr_bout);
        finishArchiveEntry(_addr_bout, start, "__.PKGDEF");
    }
    if (mode & modeLinkerObj != 0) {
        start = startArchiveEntry(_addr_bout);
        dumpLinkerObj(_addr_bout);
        finishArchiveEntry(_addr_bout, start, "_go_.o");
    }
});

private static void printObjHeader(ptr<bio.Writer> _addr_bout) {
    ref bio.Writer bout = ref _addr_bout.val;

    bout.WriteString(objabi.HeaderString());
    if (@base.Flag.BuildID != "") {
        fmt.Fprintf(bout, "build id %q\n", @base.Flag.BuildID);
    }
    if (types.LocalPkg.Name == "main") {
        fmt.Fprintf(bout, "main\n");
    }
    fmt.Fprintf(bout, "\n"); // header ends with blank line
}

private static long startArchiveEntry(ptr<bio.Writer> _addr_bout) {
    ref bio.Writer bout = ref _addr_bout.val;

    array<byte> arhdr = new array<byte>(archive.HeaderSize);
    bout.Write(arhdr[..]);
    return bout.Offset();
}

private static void finishArchiveEntry(ptr<bio.Writer> _addr_bout, long start, @string name) {
    ref bio.Writer bout = ref _addr_bout.val;

    bout.Flush();
    var size = bout.Offset() - start;
    if (size & 1 != 0) {
        bout.WriteByte(0);
    }
    bout.MustSeek(start - archive.HeaderSize, 0);

    array<byte> arhdr = new array<byte>(archive.HeaderSize);
    archive.FormatHeader(arhdr[..], name, size);
    bout.Write(arhdr[..]);
    bout.Flush();
    bout.MustSeek(start + size + (size & 1), 0);
}

private static void dumpCompilerObj(ptr<bio.Writer> _addr_bout) {
    ref bio.Writer bout = ref _addr_bout.val;

    printObjHeader(_addr_bout);
    dumpexport(bout);
}

private static void dumpdata() {
    var numExterns = len(typecheck.Target.Externs);
    var numDecls = len(typecheck.Target.Decls);

    dumpglobls(typecheck.Target.Externs);
    reflectdata.CollectPTabs();
    var numExports = len(typecheck.Target.Exports);
    addsignats(typecheck.Target.Externs);
    reflectdata.WriteRuntimeTypes();
    reflectdata.WriteTabs();
    var (numPTabs, numITabs) = reflectdata.CountTabs();
    reflectdata.WriteImportStrings();
    reflectdata.WriteBasicTypes();
    dumpembeds(); 

    // Calls to WriteRuntimeTypes can generate functions,
    // like method wrappers and hash and equality routines.
    // Compile any generated functions, process any new resulting types, repeat.
    // This can't loop forever, because there is no way to generate an infinite
    // number of types in a finite amount of code.
    // In the typical case, we loop 0 or 1 times.
    // It was not until issue 24761 that we found any code that required a loop at all.
    while (true) {
        for (var i = numDecls; i < len(typecheck.Target.Decls); i++) {
            {
                ptr<ir.Func> (n, ok) = typecheck.Target.Decls[i]._<ptr<ir.Func>>();

                if (ok) {
                    enqueueFunc(n);
                }

            }
        }
        numDecls = len(typecheck.Target.Decls);
        compileFunctions();
        reflectdata.WriteRuntimeTypes();
        if (numDecls == len(typecheck.Target.Decls)) {
            break;
        }
    } 

    // Dump extra globals.
    dumpglobls(typecheck.Target.Externs[(int)numExterns..]);

    if (reflectdata.ZeroSize > 0) {
        var zero = @base.PkgLinksym("go.map", "zero", obj.ABI0);
        objw.Global(zero, int32(reflectdata.ZeroSize), obj.DUPOK | obj.RODATA);
        zero.Set(obj.AttrStatic, true);
    }
    staticdata.WriteFuncSyms();
    addGCLocals();

    if (numExports != len(typecheck.Target.Exports)) {
        @base.Fatalf("Target.Exports changed after compile functions loop");
    }
    var (newNumPTabs, newNumITabs) = reflectdata.CountTabs();
    if (newNumPTabs != numPTabs) {
        @base.Fatalf("ptabs changed after compile functions loop");
    }
    if (newNumITabs != numITabs) {
        @base.Fatalf("itabs changed after compile functions loop");
    }
}

private static void dumpLinkerObj(ptr<bio.Writer> _addr_bout) {
    ref bio.Writer bout = ref _addr_bout.val;

    printObjHeader(_addr_bout);

    if (len(typecheck.Target.CgoPragmas) != 0) { 
        // write empty export section; must be before cgo section
        fmt.Fprintf(bout, "\n$$\n\n$$\n\n");
        fmt.Fprintf(bout, "\n$$  // cgo\n");
        {
            var err = json.NewEncoder(bout).Encode(typecheck.Target.CgoPragmas);

            if (err != null) {
                @base.Fatalf("serializing pragcgobuf: %v", err);
            }

        }
        fmt.Fprintf(bout, "\n$$\n\n");
    }
    fmt.Fprintf(bout, "\n!\n");

    obj.WriteObjFile(@base.Ctxt, bout);
}

private static void dumpGlobal(ptr<ir.Name> _addr_n) {
    ref ir.Name n = ref _addr_n.val;

    if (n.Type() == null) {
        @base.Fatalf("external %v nil type\n", n);
    }
    if (n.Class == ir.PFUNC) {
        return ;
    }
    if (n.Sym().Pkg != types.LocalPkg) {
        return ;
    }
    types.CalcSize(n.Type());
    ggloblnod(_addr_n);
    @base.Ctxt.DwarfGlobal(@base.Ctxt.Pkgpath, types.TypeSymName(n.Type()), n.Linksym());
}

private static void dumpGlobalConst(ir.Node n) { 
    // only export typed constants
    var t = n.Type();
    if (t == null) {
        return ;
    }
    if (n.Sym().Pkg != types.LocalPkg) {
        return ;
    }
    if (!t.IsInteger()) {
        return ;
    }
    var v = n.Val();
    if (t.IsUntyped()) { 
        // Export untyped integers as int (if they fit).
        t = types.Types[types.TINT];
        if (ir.ConstOverflow(v, t)) {
            return ;
        }
    }
    @base.Ctxt.DwarfIntConst(@base.Ctxt.Pkgpath, n.Sym().Name, types.TypeSymName(t), ir.IntVal(t, v));
}

private static void dumpglobls(slice<ir.Node> externs) { 
    // add globals
    foreach (var (_, n) in externs) {

        if (n.Op() == ir.ONAME) 
            dumpGlobal(n._<ptr<ir.Name>>());
        else if (n.Op() == ir.OLITERAL) 
            dumpGlobalConst(n);
            }
}

// addGCLocals adds gcargs, gclocals, gcregs, and stack object symbols to Ctxt.Data.
//
// This is done during the sequential phase after compilation, since
// global symbols can't be declared during parallel compilation.
private static void addGCLocals() {
    foreach (var (_, s) in @base.Ctxt.Text) {
        var fn = s.Func();
        if (fn == null) {
            continue;
        }
        foreach (var (_, gcsym) in new slice<ptr<obj.LSym>>(new ptr<obj.LSym>[] { fn.GCArgs, fn.GCLocals })) {
            if (gcsym != null && !gcsym.OnList()) {
                objw.Global(gcsym, int32(len(gcsym.P)), obj.RODATA | obj.DUPOK);
            }
        }        {
            var x__prev1 = x;

            var x = fn.StackObjects;

            if (x != null) {
                var attr = int16(obj.RODATA);
                objw.Global(x, int32(len(x.P)), attr);
                x.Set(obj.AttrStatic, true);
            }

            x = x__prev1;

        }
        {
            var x__prev1 = x;

            x = fn.OpenCodedDeferInfo;

            if (x != null) {
                objw.Global(x, int32(len(x.P)), obj.RODATA | obj.DUPOK);
            }

            x = x__prev1;

        }
        {
            var x__prev1 = x;

            x = fn.ArgInfo;

            if (x != null) {
                objw.Global(x, int32(len(x.P)), obj.RODATA | obj.DUPOK);
                x.Set(obj.AttrStatic, true);
                x.Set(obj.AttrContentAddressable, true);
            }

            x = x__prev1;

        }
    }
}

private static void ggloblnod(ptr<ir.Name> _addr_nam) {
    ref ir.Name nam = ref _addr_nam.val;

    var s = nam.Linksym();
    s.Gotype = reflectdata.TypeLinksym(nam.Type());
    nint flags = 0;
    if (nam.Readonly()) {
        flags = obj.RODATA;
    }
    if (nam.Type() != null && !nam.Type().HasPointers()) {
        flags |= obj.NOPTR;
    }
    @base.Ctxt.Globl(s, nam.Type().Width, flags);
    if (nam.LibfuzzerExtraCounter()) {
        s.Type = objabi.SLIBFUZZER_EXTRA_COUNTER;
    }
    if (nam.Sym().Linkname != "") { 
        // Make sure linkname'd symbol is non-package. When a symbol is
        // both imported and linkname'd, s.Pkg may not set to "_" in
        // types.Sym.Linksym because LSym already exists. Set it here.
        s.Pkg = "_";
    }
}

private static void dumpembeds() {
    foreach (var (_, v) in typecheck.Target.Embeds) {
        staticdata.WriteEmbed(v);
    }
}

private static void addsignats(slice<ir.Node> dcls) { 
    // copy types from dcl list to signatset
    foreach (var (_, n) in dcls) {
        if (n.Op() == ir.OTYPE) {
            reflectdata.NeedRuntimeType(n.Type());
        }
    }
}

} // end gc_package
