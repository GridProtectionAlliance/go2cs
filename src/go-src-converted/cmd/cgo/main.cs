// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Cgo; see doc.go for an overview.

// TODO(rsc):
//    Emit correct line number annotations.
//    Make gc understand the annotations.

// package main -- go2cs converted at 2022 March 06 22:47:21 UTC
// Original source: C:\Program Files\Go\src\cmd\cgo\main.go
using md5 = go.crypto.md5_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using printer = go.go.printer_package;
using token = go.go.token_package;
using buildcfg = go.@internal.buildcfg_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using exec = go.os.exec_package;
using filepath = go.path.filepath_package;
using reflect = go.reflect_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strings = go.strings_package;

using edit = go.cmd.@internal.edit_package;
using objabi = go.cmd.@internal.objabi_package;
using System;


namespace go;

public static partial class main_package {

    // A Package collects information about the package we're going to write.
public partial struct Package {
    public @string PackageName; // name of package
    public @string PackagePath;
    public long PtrSize;
    public long IntSize;
    public slice<@string> GccOptions;
    public bool GccIsClang;
    public map<@string, slice<@string>> CgoFlags; // #cgo flags (CFLAGS, LDFLAGS)
    public map<@string, bool> Written;
    public map<@string, ptr<Name>> Name; // accumulated Name from Files
    public slice<ptr<ExpFunc>> ExpFunc; // accumulated ExpFunc from Files
    public slice<ast.Decl> Decl;
    public slice<@string> GoFiles; // list of Go files
    public slice<@string> GccFiles; // list of gcc output files
    public @string Preamble; // collected preamble for _cgo_export.h
    public map<@string, bool> typedefs; // type names that appear in the types of the objects we're interested in
    public slice<typedefInfo> typedefList;
}

// A typedefInfo is an element on Package.typedefList: a typedef name
// and the position where it was required.
private partial struct typedefInfo {
    public @string typedef;
    public token.Pos pos;
}

// A File collects information about a single Go input file.
public partial struct File {
    public ptr<ast.File> AST; // parsed AST
    public slice<ptr<ast.CommentGroup>> Comments; // comments from file
    public @string Package; // Package name
    public @string Preamble; // C preamble (doc comment on import "C")
    public slice<ptr<Ref>> Ref; // all references to C.xxx in AST
    public slice<ptr<Call>> Calls; // all calls to C.xxx in AST
    public slice<ptr<ExpFunc>> ExpFunc; // exported functions for this file
    public map<@string, ptr<Name>> Name; // map from Go name to Name
    public map<ptr<Name>, token.Pos> NamePos; // map from Name to position of the first reference
    public ptr<edit.Buffer> Edit;
}

private static nint offset(this ptr<File> _addr_f, token.Pos p) {
    ref File f = ref _addr_f.val;

    return fset.Position(p).Offset;
}

private static slice<@string> nameKeys(map<@string, ptr<Name>> m) {
    slice<@string> ks = default;
    foreach (var (k) in m) {
        ks = append(ks, k);
    }    sort.Strings(ks);
    return ks;
}

// A Call refers to a call of a C.xxx function in the AST.
public partial struct Call {
    public ptr<ast.CallExpr> Call;
    public bool Deferred;
    public bool Done;
}

// A Ref refers to an expression of the form C.xxx in the AST.
public partial struct Ref {
    public ptr<Name> Name;
    public ptr<ast.Expr> Expr;
    public astContext Context;
    public bool Done;
}

private static token.Pos Pos(this ptr<Ref> _addr_r) {
    ref Ref r = ref _addr_r.val;

    return (r.Expr.val).Pos();
}

private static @string nameKinds = new slice<@string>(new @string[] { "iconst", "fconst", "sconst", "type", "var", "fpvar", "func", "macro", "not-type" });

// A Name collects information about C.xxx.
public partial struct Name {
    public @string Go; // name used in Go referring to package C
    public @string Mangle; // name used in generated Go
    public @string C; // name used in C
    public @string Define; // #define expansion
    public @string Kind; // one of the nameKinds
    public ptr<Type> Type; // the type of xxx
    public ptr<FuncType> FuncType;
    public bool AddError;
    public @string Const; // constant definition
}

// IsVar reports whether Kind is either "var" or "fpvar"
private static bool IsVar(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return n.Kind == "var" || n.Kind == "fpvar";
}

// IsConst reports whether Kind is either "iconst", "fconst" or "sconst"
private static bool IsConst(this ptr<Name> _addr_n) {
    ref Name n = ref _addr_n.val;

    return strings.HasSuffix(n.Kind, "const");
}

// An ExpFunc is an exported function, callable from C.
// Such functions are identified in the Go input file
// by doc comments containing the line //export ExpName
public partial struct ExpFunc {
    public ptr<ast.FuncDecl> Func;
    public @string ExpName; // name to use from C
    public @string Doc;
}

// A TypeRepr contains the string representation of a type.
public partial struct TypeRepr {
    public @string Repr;
    public slice<object> FormatArgs;
}

// A Type collects information about a type in both the C and Go worlds.
public partial struct Type {
    public long Size;
    public long Align;
    public ptr<TypeRepr> C;
    public ast.Expr Go;
    public map<@string, long> EnumValues;
    public @string Typedef;
    public bool BadPointer; // this pointer type should be represented as a uintptr (deprecated)
    public bool NotInHeap; // this type should have a go:notinheap annotation
}

// A FuncType collects information about a function type in both the C and Go worlds.
public partial struct FuncType {
    public slice<ptr<Type>> Params;
    public ptr<Type> Result;
    public ptr<ast.FuncType> Go;
}

private static void usage() {
    fmt.Fprint(os.Stderr, "usage: cgo -- [compiler options] file.go ...\n");
    flag.PrintDefaults();
    os.Exit(2);
}

private static map ptrSizeMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, long>{"386":4,"alpha":8,"amd64":8,"arm":4,"arm64":8,"m68k":4,"mips":4,"mipsle":4,"mips64":8,"mips64le":8,"nios2":4,"ppc":4,"ppc64":8,"ppc64le":8,"riscv":4,"riscv64":8,"s390":4,"s390x":8,"sh":4,"shbe":4,"sparc":4,"sparc64":8,};

private static map intSizeMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, long>{"386":4,"alpha":8,"amd64":8,"arm":4,"arm64":8,"m68k":4,"mips":4,"mipsle":4,"mips64":8,"mips64le":8,"nios2":4,"ppc":4,"ppc64":8,"ppc64le":8,"riscv":4,"riscv64":8,"s390":4,"s390x":8,"sh":4,"shbe":4,"sparc":4,"sparc64":8,};

private static @string cPrefix = default;

private static var fset = token.NewFileSet();

private static var dynobj = flag.String("dynimport", "", "if non-empty, print dynamic import data for that file");
private static var dynout = flag.String("dynout", "", "write -dynimport output to this file");
private static var dynpackage = flag.String("dynpackage", "main", "set Go package for -dynimport output");
private static var dynlinker = flag.Bool("dynlinker", false, "record dynamic linker information in -dynimport mode");

// This flag is for bootstrapping a new Go implementation,
// to generate Go types that match the data layout and
// constant values used in the host's C libraries and system calls.
private static var godefs = flag.Bool("godefs", false, "for bootstrap: write Go definitions for C file to standard output");

private static var srcDir = flag.String("srcdir", "", "source directory");
private static var objDir = flag.String("objdir", "", "object directory");
private static var importPath = flag.String("importpath", "", "import path of package being built (for comments in generated files)");
private static var exportHeader = flag.String("exportheader", "", "where to write export header if any exported functions");

private static var gccgo = flag.Bool("gccgo", false, "generate files for use with gccgo");
private static var gccgoprefix = flag.String("gccgoprefix", "", "-fgo-prefix option used with gccgo");
private static var gccgopkgpath = flag.String("gccgopkgpath", "", "-fgo-pkgpath option used with gccgo");
private static Func<@string, @string> gccgoMangler = default;
private static var importRuntimeCgo = flag.Bool("import_runtime_cgo", true, "import runtime/cgo in generated code");
private static var importSyscall = flag.Bool("import_syscall", true, "import syscall in generated code");
private static var trimpath = flag.String("trimpath", "", "applies supplied rewrites or trims prefixes to recorded source file paths");

private static @string goarch = default;private static @string goos = default;private static @string gomips = default;private static @string gomips64 = default;



private static void Main() {
    objabi.AddVersionFlag(); // -V
    flag.Usage = usage;
    flag.Parse();

    if (dynobj != "".val) { 
        // cgo -dynimport is essentially a separate helper command
        // built into the cgo binary. It scans a gcc-produced executable
        // and dumps information about the imported symbols and the
        // imported libraries. The 'go build' rules for cgo prepare an
        // appropriate executable and then use its import information
        // instead of needing to make the linkers duplicate all the
        // specialized knowledge gcc has about where to look for imported
        // symbols and which ones to use.
        dynimport(dynobj.val);
        return ;

    }
    if (godefs.val) { 
        // Generating definitions pulled from header files,
        // to be checked into Go repositories.
        // Line numbers are just noise.
        conf.Mode &= printer.SourcePos;

    }
    var args = flag.Args();
    if (len(args) < 1) {
        usage();
    }
    nint i = default;
    for (i = len(args); i > 0; i--) {
        if (!strings.HasSuffix(args[i - 1], ".go")) {
            break;
        }
    }
    if (i == len(args)) {
        usage();
    }
    var goFiles = args[(int)i..];

    foreach (var (_, arg) in args[..(int)i]) {
        if (arg == "-fsanitize=thread") {
            tsanProlog = yesTsanProlog;
        }
        if (arg == "-fsanitize=memory") {
            msanProlog = yesMsanProlog;
        }
    }    var p = newPackage(args[..(int)i]); 

    // We need a C compiler to be available. Check this.
    var gccName = p.gccBaseCmd()[0];
    var (_, err) = exec.LookPath(gccName);
    if (err != null) {
        fatalf("C compiler %q not found: %v", gccName, err);
        os.Exit(2);
    }
    {
        var ldflags = os.Getenv("CGO_LDFLAGS");

        if (ldflags != "") {
            var (args, err) = splitQuoted(ldflags);
            if (err != null) {
                fatalf("bad CGO_LDFLAGS: %q (%s)", ldflags, err);
            }
            p.addToFlag("LDFLAGS", args);
        }
    } 

    // Need a unique prefix for the global C symbols that
    // we use to coordinate between gcc and ourselves.
    // We already put _cgo_ at the beginning, so the main
    // concern is other cgo wrappers for the same functions.
    // Use the beginning of the md5 of the input to disambiguate.
    var h = md5.New();
    io.WriteString(h, importPath.val);
    var fs = make_slice<ptr<File>>(len(goFiles));
    {
        nint i__prev1 = i;
        var input__prev1 = input;

        foreach (var (__i, __input) in goFiles) {
            i = __i;
            input = __input;
            if (srcDir != "".val) {
                input = filepath.Join(srcDir.val, input);
            } 

            // Create absolute path for file, so that it will be used in error
            // messages and recorded in debug line number information.
            // This matches the rest of the toolchain. See golang.org/issue/5122.
            {
                var (aname, err) = filepath.Abs(input);

                if (err == null) {
                    input = aname;
                }

            }


            var (b, err) = ioutil.ReadFile(input);
            if (err != null) {
                fatalf("%s", err);
            }

            _, err = h.Write(b);

            if (err != null) {
                fatalf("%s", err);
            } 

            // Apply trimpath to the file path. The path won't be read from after this point.
            input, _ = objabi.ApplyRewrites(input, trimpath.val);
            goFiles[i] = input;

            ptr<File> f = @new<File>();
            f.Edit = edit.NewBuffer(b);
            f.ParseGo(input, b);
            f.DiscardCgoDirectives();
            fs[i] = f;

        }
        i = i__prev1;
        input = input__prev1;
    }

    cPrefix = fmt.Sprintf("_%x", h.Sum(null)[(int)0..(int)6]);

    if (objDir == "".val) { 
        // make sure that _obj directory exists, so that we can write
        // all the output files there.
        os.Mkdir("_obj", 0777);
        objDir.val = "_obj";

    }
    objDir.val += string(filepath.Separator);

    {
        nint i__prev1 = i;
        var input__prev1 = input;

        foreach (var (__i, __input) in goFiles) {
            i = __i;
            input = __input;
            f = fs[i];
            p.Translate(f);
            foreach (var (_, cref) in f.Ref) {

                if (cref.Context == ctxCall || cref.Context == ctxCall2) 
                    if (cref.Name.Kind != "type") {
                        break;
                    }
                    var old = cref.Expr.val;
                    cref.Expr.val = cref.Name.Type.Go;
                    f.Edit.Replace(f.offset(old.Pos()), f.offset(old.End()), gofmt(cref.Name.Type.Go));
                
            }
            if (nerrors > 0) {
                os.Exit(2);
            }

            p.PackagePath = f.Package;
            p.Record(f);
            if (godefs.val) {
                os.Stdout.WriteString(p.godefs(f));
            }
            else
 {
                p.writeOutput(f, input);
            }

        }
        i = i__prev1;
        input = input__prev1;
    }

    if (!godefs.val) {
        p.writeDefs();
    }
    if (nerrors > 0) {
        os.Exit(2);
    }
}

// newPackage returns a new Package that will invoke
// gcc with the additional arguments specified in args.
private static ptr<Package> newPackage(slice<@string> args) {
    goarch = runtime.GOARCH;
    {
        var s__prev1 = s;

        var s = os.Getenv("GOARCH");

        if (s != "") {
            goarch = s;
        }
        s = s__prev1;

    }

    goos = runtime.GOOS;
    {
        var s__prev1 = s;

        s = os.Getenv("GOOS");

        if (s != "") {
            goos = s;
        }
        s = s__prev1;

    }

    buildcfg.Check();
    gomips = buildcfg.GOMIPS;
    gomips64 = buildcfg.GOMIPS64;
    var ptrSize = ptrSizeMap[goarch];
    if (ptrSize == 0) {
        fatalf("unknown ptrSize for $GOARCH %q", goarch);
    }
    var intSize = intSizeMap[goarch];
    if (intSize == 0) {
        fatalf("unknown intSize for $GOARCH %q", goarch);
    }
    os.Setenv("LANG", "en_US.UTF-8");
    os.Setenv("LC_ALL", "C");

    ptr<Package> p = addr(new Package(PtrSize:ptrSize,IntSize:intSize,CgoFlags:make(map[string][]string),Written:make(map[string]bool),));
    p.addToFlag("CFLAGS", args);
    return _addr_p!;

}

// Record what needs to be recorded about f.
private static void Record(this ptr<Package> _addr_p, ptr<File> _addr_f) {
    ref Package p = ref _addr_p.val;
    ref File f = ref _addr_f.val;

    if (p.PackageName == "") {
        p.PackageName = f.Package;
    }
    else if (p.PackageName != f.Package) {
        error_(token.NoPos, "inconsistent package names: %s, %s", p.PackageName, f.Package);
    }
    if (p.Name == null) {
        p.Name = f.Name;
    }
    else
 {
        foreach (var (k, v) in f.Name) {
            if (p.Name[k] == null) {
                p.Name[k] = v;
            }
            else if (p.incompleteTypedef(p.Name[k].Type)) {
                p.Name[k] = v;
            }
            else if (p.incompleteTypedef(v.Type)) { 
                // Nothing to do.
            }            {
                var (_, ok) = nameToC[k];


                else if (ok) { 
                    // Names we predefine may appear inconsistent
                    // if some files typedef them and some don't.
                    // Issue 26743.
                }
                else if (!reflect.DeepEqual(p.Name[k], v)) {
                    error_(token.NoPos, "inconsistent definitions for C.%s", fixGo(k));
                }


            }

        }
    }
    if (f.ExpFunc != null) {
        p.ExpFunc = append(p.ExpFunc, f.ExpFunc);
        p.Preamble += "\n" + f.Preamble;
    }
    p.Decl = append(p.Decl, f.AST.Decls);

}

// incompleteTypedef reports whether t appears to be an incomplete
// typedef definition.
private static bool incompleteTypedef(this ptr<Package> _addr_p, ptr<Type> _addr_t) {
    ref Package p = ref _addr_p.val;
    ref Type t = ref _addr_t.val;

    return t == null || (t.Size == 0 && t.Align == -1);
}

} // end main_package
