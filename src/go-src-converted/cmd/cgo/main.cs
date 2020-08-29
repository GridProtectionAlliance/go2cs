// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Cgo; see gmp.go for an overview.

// TODO(rsc):
//    Emit correct line number annotations.
//    Make gc understand the annotations.

// package main -- go2cs converted at 2020 August 29 08:52:43 UTC
// Original source: C:\Go\src\cmd\cgo\main.go
using md5 = go.crypto.md5_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using printer = go.go.printer_package;
using token = go.go.token_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using reflect = go.reflect_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strings = go.strings_package;

using edit = go.cmd.@internal.edit_package;
using objabi = go.cmd.@internal.objabi_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // A Package collects information about the package we're going to write.
        public partial struct Package
        {
            public @string PackageName; // name of package
            public @string PackagePath;
            public long PtrSize;
            public long IntSize;
            public slice<@string> GccOptions;
            public bool GccIsClang;
            public map<@string, slice<@string>> CgoFlags; // #cgo flags (CFLAGS, LDFLAGS)
            public map<@string, bool> Written;
            public map<@string, ref Name> Name; // accumulated Name from Files
            public slice<ref ExpFunc> ExpFunc; // accumulated ExpFunc from Files
            public slice<ast.Decl> Decl;
            public slice<@string> GoFiles; // list of Go files
            public slice<@string> GccFiles; // list of gcc output files
            public @string Preamble; // collected preamble for _cgo_export.h
        }

        // A File collects information about a single Go input file.
        public partial struct File
        {
            public ptr<ast.File> AST; // parsed AST
            public slice<ref ast.CommentGroup> Comments; // comments from file
            public @string Package; // Package name
            public @string Preamble; // C preamble (doc comment on import "C")
            public slice<ref Ref> Ref; // all references to C.xxx in AST
            public slice<ref Call> Calls; // all calls to C.xxx in AST
            public slice<ref ExpFunc> ExpFunc; // exported functions for this file
            public map<@string, ref Name> Name; // map from Go name to Name
            public map<ref Name, token.Pos> NamePos; // map from Name to position of the first reference
            public ptr<edit.Buffer> Edit;
        }

        private static long offset(this ref File f, token.Pos p)
        {
            return fset.Position(p).Offset;
        }

        private static slice<@string> nameKeys(map<@string, ref Name> m)
        {
            slice<@string> ks = default;
            foreach (var (k) in m)
            {
                ks = append(ks, k);
            }
            sort.Strings(ks);
            return ks;
        }

        // A Call refers to a call of a C.xxx function in the AST.
        public partial struct Call
        {
            public ptr<ast.CallExpr> Call;
            public bool Deferred;
        }

        // A Ref refers to an expression of the form C.xxx in the AST.
        public partial struct Ref
        {
            public ptr<Name> Name;
            public ptr<ast.Expr> Expr;
            public astContext Context;
        }

        private static token.Pos Pos(this ref Ref r)
        {
            return ref r.Expr();
        }

        // A Name collects information about C.xxx.
        public partial struct Name
        {
            public @string Go; // name used in Go referring to package C
            public @string Mangle; // name used in generated Go
            public @string C; // name used in C
            public @string Define; // #define expansion
            public @string Kind; // "iconst", "fconst", "sconst", "type", "var", "fpvar", "func", "macro", "not-type"
            public ptr<Type> Type; // the type of xxx
            public ptr<FuncType> FuncType;
            public bool AddError;
            public @string Const; // constant definition
        }

        // IsVar reports whether Kind is either "var" or "fpvar"
        private static bool IsVar(this ref Name n)
        {
            return n.Kind == "var" || n.Kind == "fpvar";
        }

        // IsConst reports whether Kind is either "iconst", "fconst" or "sconst"
        private static bool IsConst(this ref Name n)
        {
            return strings.HasSuffix(n.Kind, "const");
        }

        // An ExpFunc is an exported function, callable from C.
        // Such functions are identified in the Go input file
        // by doc comments containing the line //export ExpName
        public partial struct ExpFunc
        {
            public ptr<ast.FuncDecl> Func;
            public @string ExpName; // name to use from C
            public @string Doc;
        }

        // A TypeRepr contains the string representation of a type.
        public partial struct TypeRepr
        {
            public @string Repr;
            public slice<object> FormatArgs;
        }

        // A Type collects information about a type in both the C and Go worlds.
        public partial struct Type
        {
            public long Size;
            public long Align;
            public ptr<TypeRepr> C;
            public ast.Expr Go;
            public map<@string, long> EnumValues;
            public @string Typedef;
        }

        // A FuncType collects information about a function type in both the C and Go worlds.
        public partial struct FuncType
        {
            public slice<ref Type> Params;
            public ptr<Type> Result;
            public ptr<ast.FuncType> Go;
        }

        private static void usage()
        {
            fmt.Fprint(os.Stderr, "usage: cgo -- [compiler options] file.go ...\n");
            flag.PrintDefaults();
            os.Exit(2L);
        }

        private static map ptrSizeMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, long>{"386":4,"amd64":8,"arm":4,"arm64":8,"mips":4,"mipsle":4,"mips64":8,"mips64le":8,"ppc64":8,"ppc64le":8,"s390":4,"s390x":8,};

        private static map intSizeMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, long>{"386":4,"amd64":8,"arm":4,"arm64":8,"mips":4,"mipsle":4,"mips64":8,"mips64le":8,"ppc64":8,"ppc64le":8,"s390":4,"s390x":8,};

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
        private static var importRuntimeCgo = flag.Bool("import_runtime_cgo", true, "import runtime/cgo in generated code");
        private static var importSyscall = flag.Bool("import_syscall", true, "import syscall in generated code");
        private static @string goarch = default;        private static @string goos = default;



        private static void Main()
        {
            objabi.AddVersionFlag(); // -V
            flag.Usage = usage;
            flag.Parse();

            if (dynobj != "".Value)
            { 
                // cgo -dynimport is essentially a separate helper command
                // built into the cgo binary. It scans a gcc-produced executable
                // and dumps information about the imported symbols and the
                // imported libraries. The 'go build' rules for cgo prepare an
                // appropriate executable and then use its import information
                // instead of needing to make the linkers duplicate all the
                // specialized knowledge gcc has about where to look for imported
                // symbols and which ones to use.
                dynimport(dynobj.Value);
                return;
            }
            if (godefs.Value)
            { 
                // Generating definitions pulled from header files,
                // to be checked into Go repositories.
                // Line numbers are just noise.
                conf.Mode &= printer.SourcePos;
            }
            var args = flag.Args();
            if (len(args) < 1L)
            {
                usage();
            } 

            // Find first arg that looks like a go file and assume everything before
            // that are options to pass to gcc.
            long i = default;
            for (i = len(args); i > 0L; i--)
            {
                if (!strings.HasSuffix(args[i - 1L], ".go"))
                {
                    break;
                }
            }

            if (i == len(args))
            {
                usage();
            }
            var goFiles = args[i..];

            foreach (var (_, arg) in args[..i])
            {
                if (arg == "-fsanitize=thread")
                {
                    tsanProlog = yesTsanProlog;
                }
            }
            var p = newPackage(args[..i]); 

            // Record CGO_LDFLAGS from the environment for external linking.
            {
                var ldflags = os.Getenv("CGO_LDFLAGS");

                if (ldflags != "")
                {
                    var (args, err) = splitQuoted(ldflags);
                    if (err != null)
                    {
                        fatalf("bad CGO_LDFLAGS: %q (%s)", ldflags, err);
                    }
                    p.addToFlag("LDFLAGS", args);
                } 

                // Need a unique prefix for the global C symbols that
                // we use to coordinate between gcc and ourselves.
                // We already put _cgo_ at the beginning, so the main
                // concern is other cgo wrappers for the same functions.
                // Use the beginning of the md5 of the input to disambiguate.

            } 

            // Need a unique prefix for the global C symbols that
            // we use to coordinate between gcc and ourselves.
            // We already put _cgo_ at the beginning, so the main
            // concern is other cgo wrappers for the same functions.
            // Use the beginning of the md5 of the input to disambiguate.
            var h = md5.New();
            var fs = make_slice<ref File>(len(goFiles));
            {
                long i__prev1 = i;
                var input__prev1 = input;

                foreach (var (__i, __input) in goFiles)
                {
                    i = __i;
                    input = __input;
                    if (srcDir != "".Value)
                    {
                        input = filepath.Join(srcDir.Value, input);
                    }
                    var (b, err) = ioutil.ReadFile(input);
                    if (err != null)
                    {
                        fatalf("%s", err);
                    }
                    _, err = h.Write(b);

                    if (err != null)
                    {
                        fatalf("%s", err);
                    }
                    ptr<File> f = @new<File>();
                    f.Edit = edit.NewBuffer(b);
                    f.ParseGo(input, b);
                    f.DiscardCgoDirectives();
                    fs[i] = f;
                }

                i = i__prev1;
                input = input__prev1;
            }

            cPrefix = fmt.Sprintf("_%x", h.Sum(null)[0L..6L]);

            if (objDir == "".Value)
            { 
                // make sure that _obj directory exists, so that we can write
                // all the output files there.
                os.Mkdir("_obj", 0777L);
                objDir.Value = "_obj";
            }
            objDir.Value += string(filepath.Separator);

            {
                long i__prev1 = i;
                var input__prev1 = input;

                foreach (var (__i, __input) in goFiles)
                {
                    i = __i;
                    input = __input;
                    f = fs[i];
                    p.Translate(f);
                    foreach (var (_, cref) in f.Ref)
                    {

                        if (cref.Context == ctxCall || cref.Context == ctxCall2) 
                            if (cref.Name.Kind != "type")
                            {
                                break;
                            }
                            var old = cref.Expr.Value;
                            cref.Expr.Value = cref.Name.Type.Go;
                            f.Edit.Replace(f.offset(old.Pos()), f.offset(old.End()), gofmt(cref.Name.Type.Go));
                                            }
                    if (nerrors > 0L)
                    {
                        os.Exit(2L);
                    }
                    p.PackagePath = f.Package;
                    p.Record(f);
                    if (godefs.Value)
                    {
                        os.Stdout.WriteString(p.godefs(f, input));
                    }
                    else
                    {
                        p.writeOutput(f, input);
                    }
                }

                i = i__prev1;
                input = input__prev1;
            }

            if (!godefs.Value)
            {
                p.writeDefs();
            }
            if (nerrors > 0L)
            {
                os.Exit(2L);
            }
        }

        // newPackage returns a new Package that will invoke
        // gcc with the additional arguments specified in args.
        private static ref Package newPackage(slice<@string> args)
        {
            goarch = runtime.GOARCH;
            {
                var s__prev1 = s;

                var s = os.Getenv("GOARCH");

                if (s != "")
                {
                    goarch = s;
                }

                s = s__prev1;

            }
            goos = runtime.GOOS;
            {
                var s__prev1 = s;

                s = os.Getenv("GOOS");

                if (s != "")
                {
                    goos = s;
                }

                s = s__prev1;

            }
            var ptrSize = ptrSizeMap[goarch];
            if (ptrSize == 0L)
            {
                fatalf("unknown ptrSize for $GOARCH %q", goarch);
            }
            var intSize = intSizeMap[goarch];
            if (intSize == 0L)
            {
                fatalf("unknown intSize for $GOARCH %q", goarch);
            } 

            // Reset locale variables so gcc emits English errors [sic].
            os.Setenv("LANG", "en_US.UTF-8");
            os.Setenv("LC_ALL", "C");

            Package p = ref new Package(PtrSize:ptrSize,IntSize:intSize,CgoFlags:make(map[string][]string),Written:make(map[string]bool),);
            p.addToFlag("CFLAGS", args);
            return p;
        }

        // Record what needs to be recorded about f.
        private static void Record(this ref Package p, ref File f)
        {
            if (p.PackageName == "")
            {
                p.PackageName = f.Package;
            }
            else if (p.PackageName != f.Package)
            {
                error_(token.NoPos, "inconsistent package names: %s, %s", p.PackageName, f.Package);
            }
            if (p.Name == null)
            {
                p.Name = f.Name;
            }
            else
            {
                foreach (var (k, v) in f.Name)
                {
                    if (p.Name[k] == null)
                    {
                        p.Name[k] = v;
                    }
                    else if (!reflect.DeepEqual(p.Name[k], v))
                    {
                        error_(token.NoPos, "inconsistent definitions for C.%s", fixGo(k));
                    }
                }
            }
            if (f.ExpFunc != null)
            {
                p.ExpFunc = append(p.ExpFunc, f.ExpFunc);
                p.Preamble += "\n" + f.Preamble;
            }
            p.Decl = append(p.Decl, f.AST.Decls);
        }
    }
}
