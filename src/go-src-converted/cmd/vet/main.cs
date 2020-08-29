// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Vet is a simple checker for static errors in Go source code.
// See doc.go for more information.
// package main -- go2cs converted at 2020 August 29 10:09:03 UTC
// Original source: C:\Go\src\cmd\vet\main.go
using bytes = go.bytes_package;
using json = go.encoding.json_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using build = go.go.build_package;
using importer = go.go.importer_package;
using parser = go.go.parser_package;
using printer = go.go.printer_package;
using token = go.go.token_package;
using types = go.go.types_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        // Important! If you add flags here, make sure to update cmd/go/internal/vet/vetflag.go.
        private static var verbose = flag.Bool("v", false, "verbose");        private static var source = flag.Bool("source", false, "import from source instead of compiled object files");        private static var tags = flag.String("tags", "", "space-separated list of build tags to apply when parsing");        private static @string tagList = new slice<@string>(new @string[] {  });        private static vetConfig vcfg = default;        private static bool mustTypecheck = default;

        private static long exitCode = 0L;

        // "-all" flag enables all non-experimental checks
        private static var all = triStateFlag("all", unset, "enable all non-experimental checks");

        // Flags to control which individual checks to perform.
        private static map report = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ref triState>{"asmdecl":triStateFlag("asmdecl",unset,"check assembly against Go declarations"),"buildtags":triStateFlag("buildtags",unset,"check that +build tags are valid"),};

        // experimental records the flags enabling experimental features. These must be
        // requested explicitly; they are not enabled by -all.
        private static map experimental = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};

        // setTrueCount record how many flags are explicitly set to true.
        private static long setTrueCount = default;

        // dirsRun and filesRun indicate whether the vet is applied to directory or
        // file targets. The distinction affects which checks are run.
        private static bool dirsRun = default;        private static bool filesRun = default;

        // includesNonTest indicates whether the vet is applied to non-test targets.
        // Certain checks are relevant only if they touch both test and non-test files.


        // includesNonTest indicates whether the vet is applied to non-test targets.
        // Certain checks are relevant only if they touch both test and non-test files.
        private static bool includesNonTest = default;

        // A triState is a boolean that knows whether it has been set to either true or false.
        // It is used to identify if a flag appears; the standard boolean flag cannot
        // distinguish missing from unset. It also satisfies flag.Value.
        private partial struct triState // : long
        {
        }

        private static readonly triState unset = iota;
        private static readonly var setTrue = 0;
        private static readonly var setFalse = 1;

        private static ref triState triStateFlag(@string name, triState value, @string usage)
        {
            flag.Var(ref value, name, usage);
            return ref value;
        }

        // triState implements flag.Value, flag.Getter, and flag.boolFlag.
        // They work like boolean flags: we can say vet -printf as well as vet -printf=true
        private static void Get(this ref triState ts)
        {
            return ts == setTrue.Value;
        }

        private static bool isTrue(this triState ts)
        {
            return ts == setTrue;
        }

        private static error Set(this ref triState ts, @string value)
        {
            var (b, err) = strconv.ParseBool(value);
            if (err != null)
            {
                return error.As(err);
            }
            if (b)
            {
                ts.Value = setTrue;
                setTrueCount++;
            }
            else
            {
                ts.Value = setFalse;
            }
            return error.As(null);
        }

        private static @string String(this ref triState _ts) => func(_ts, (ref triState ts, Defer _, Panic panic, Recover __) =>
        {

            if (ts.Value == unset) 
                return "true"; // An unset flag will be set by -all, so defaults to true.
            else if (ts.Value == setTrue) 
                return "true";
            else if (ts.Value == setFalse) 
                return "false";
                        panic("not reached");
        });

        private static bool IsBoolFlag(this triState ts)
        {
            return true;
        }

        // vet tells whether to report errors for the named check, a flag name.
        private static bool vet(@string name)
        {
            return report[name].isTrue();
        }

        // setExit sets the value for os.Exit when it is called, later. It
        // remembers the highest value.
        private static void setExit(long err)
        {
            if (err > exitCode)
            {
                exitCode = err;
            }
        }

 
        // Each of these vars has a corresponding case in (*File).Visit.
        private static ref ast.AssignStmt assignStmt = default;        private static ref ast.BinaryExpr binaryExpr = default;        private static ref ast.CallExpr callExpr = default;        private static ref ast.CompositeLit compositeLit = default;        private static ref ast.ExprStmt exprStmt = default;        private static ref ast.ForStmt forStmt = default;        private static ref ast.FuncDecl funcDecl = default;        private static ref ast.FuncLit funcLit = default;        private static ref ast.GenDecl genDecl = default;        private static ref ast.InterfaceType interfaceType = default;        private static ref ast.RangeStmt rangeStmt = default;        private static ref ast.ReturnStmt returnStmt = default;        private static ref ast.StructType structType = default;        private static var checkers = make_map<ast.Node, map<@string, Action<ref File, ast.Node>>>();

        private static void register(@string name, @string usage, Action<ref File, ast.Node> fn, params ast.Node[] types)
        {
            types = types.Clone();

            report[name] = triStateFlag(name, unset, usage);
            foreach (var (_, typ) in types)
            {
                var m = checkers[typ];
                if (m == null)
                {
                    m = make_map<@string, Action<ref File, ast.Node>>();
                    checkers[typ] = m;
                }
                m[name] = fn;
            }
        }

        // Usage is a replacement usage function for the flags package.
        public static void Usage()
        {
            fmt.Fprintf(os.Stderr, "Usage of vet:\n");
            fmt.Fprintf(os.Stderr, "\tvet [flags] directory...\n");
            fmt.Fprintf(os.Stderr, "\tvet [flags] files... # Must be a single package\n");
            fmt.Fprintf(os.Stderr, "By default, -all is set and all non-experimental checks are run.\n");
            fmt.Fprintf(os.Stderr, "For more information run\n");
            fmt.Fprintf(os.Stderr, "\tgo doc cmd/vet\n\n");
            fmt.Fprintf(os.Stderr, "Flags:\n");
            flag.PrintDefaults();
            os.Exit(2L);
        }

        // File is a wrapper for the state of a file used in the parser.
        // The parse tree walkers are all methods of this type.
        public partial struct File
        {
            public ptr<Package> pkg;
            public ptr<token.FileSet> fset;
            public @string name;
            public slice<byte> content;
            public ptr<ast.File> file;
            public bytes.Buffer b; // for use by methods

// Parsed package "foo" when checking package "foo_test"
            public ptr<Package> basePkg; // The keys are the objects that are receivers of a "String()
// string" method. The value reports whether the method has a
// pointer receiver.
// This is used by the recursiveStringer method in print.go.
            public map<ref ast.Object, bool> stringerPtrs; // Registered checkers to run.
            public map<ast.Node, slice<Action<ref File, ast.Node>>> checkers; // Unreachable nodes; can be ignored in shift check.
            public map<ast.Node, bool> dead;
        }

        private static void Main()
        {
            flag.Usage = Usage;
            flag.Parse(); 

            // If any flag is set, we run only those checks requested.
            // If all flag is set true or if no flags are set true, set all the non-experimental ones
            // not explicitly set (in effect, set the "-all" flag).
            if (setTrueCount == 0L || all == setTrue.Value)
            {
                {
                    var name__prev1 = name;

                    foreach (var (__name, __setting) in report)
                    {
                        name = __name;
                        setting = __setting;
                        if (setting == unset && !experimental[name].Value)
                        {
                            setting.Value = setTrue;
                        }
                    }

                    name = name__prev1;
                }

            } 

            // Accept space-separated tags because that matches
            // the go command's other subcommands.
            // Accept commas because go tool vet traditionally has.
            tagList = strings.Fields(strings.Replace(tags.Value, ",", " ", -1L));

            initPrintFlags();
            initUnusedFlags();

            if (flag.NArg() == 0L)
            {
                Usage();
            } 

            // Special case for "go vet" passing an explicit configuration:
            // single argument ending in vet.cfg.
            // Once we have a more general mechanism for obtaining this
            // information from build tools like the go command,
            // vet should be changed to use it. This vet.cfg hack is an
            // experiment to learn about what form that information should take.
            if (flag.NArg() == 1L && strings.HasSuffix(flag.Arg(0L), "vet.cfg"))
            {
                doPackageCfg(flag.Arg(0L));
                os.Exit(exitCode);
            }
            {
                var name__prev1 = name;

                foreach (var (_, __name) in flag.Args())
                {
                    name = __name; 
                    // Is it a directory?
                    var (fi, err) = os.Stat(name);
                    if (err != null)
                    {
                        warnf("error walking tree: %s", err);
                        continue;
                    }
                    if (fi.IsDir())
                    {
                        dirsRun = true;
                    }
                    else
                    {
                        filesRun = true;
                        if (!strings.HasSuffix(name, "_test.go"))
                        {
                            includesNonTest = true;
                        }
                    }
                }

                name = name__prev1;
            }

            if (dirsRun && filesRun)
            {
                Usage();
            }
            if (dirsRun)
            {
                {
                    var name__prev1 = name;

                    foreach (var (_, __name) in flag.Args())
                    {
                        name = __name;
                        walkDir(name);
                    }

                    name = name__prev1;
                }

                os.Exit(exitCode);
            }
            if (doPackage(flag.Args(), null) == null)
            {
                warnf("no files checked");
            }
            os.Exit(exitCode);
        }

        // prefixDirectory places the directory name on the beginning of each name in the list.
        private static void prefixDirectory(@string directory, slice<@string> names)
        {
            if (directory != ".")
            {
                foreach (var (i, name) in names)
                {
                    names[i] = filepath.Join(directory, name);
                }
            }
        }

        // vetConfig is the JSON config struct prepared by the Go command.
        private partial struct vetConfig
        {
            public @string Compiler;
            public @string Dir;
            public @string ImportPath;
            public slice<@string> GoFiles;
            public map<@string, @string> ImportMap;
            public map<@string, @string> PackageFile;
            public bool SucceedOnTypecheckFailure;
            public types.Importer imp;
        }

        private static (ref types.Package, error) Import(this ref vetConfig v, @string path)
        {
            if (v.imp == null)
            {
                v.imp = importer.For(v.Compiler, v.openPackageFile);
            }
            if (path == "unsafe")
            {
                return v.imp.Import("unsafe");
            }
            var p = v.ImportMap[path];
            if (p == "")
            {
                return (null, fmt.Errorf("unknown import path %q", path));
            }
            if (v.PackageFile[p] == "")
            {
                return (null, fmt.Errorf("unknown package file for import %q", path));
            }
            return v.imp.Import(p);
        }

        private static (io.ReadCloser, error) openPackageFile(this ref vetConfig v, @string path)
        {
            var file = v.PackageFile[path];
            if (file == "")
            { 
                // Note that path here has been translated via v.ImportMap,
                // unlike in the error in Import above. We prefer the error in
                // Import, but it's worth diagnosing this one too, just in case.
                return (null, fmt.Errorf("unknown package file for %q", path));
            }
            var (f, err) = os.Open(file);
            if (err != null)
            {
                return (null, err);
            }
            return (f, null);
        }

        // doPackageCfg analyzes a single package described in a config file.
        private static void doPackageCfg(@string cfgFile)
        {
            var (js, err) = ioutil.ReadFile(cfgFile);
            if (err != null)
            {
                errorf("%v", err);
            }
            {
                var err = json.Unmarshal(js, ref vcfg);

                if (err != null)
                {
                    errorf("parsing vet config %s: %v", cfgFile, err);
                }

            }
            stdImporter = ref vcfg;
            inittypes();
            mustTypecheck = true;
            doPackage(vcfg.GoFiles, null);
        }

        // doPackageDir analyzes the single package found in the directory, if there is one,
        // plus a test package, if there is one.
        private static void doPackageDir(@string directory)
        {
            var context = build.Default;
            if (len(context.BuildTags) != 0L)
            {
                warnf("build tags %s previously set", context.BuildTags);
            }
            context.BuildTags = append(tagList, context.BuildTags);

            var (pkg, err) = context.ImportDir(directory, 0L);
            if (err != null)
            { 
                // If it's just that there are no go source files, that's fine.
                {
                    ref build.NoGoError (_, nogo) = err._<ref build.NoGoError>();

                    if (nogo)
                    {
                        return;
                    } 
                    // Non-fatal: we are doing a recursive walk and there may be other directories.

                } 
                // Non-fatal: we are doing a recursive walk and there may be other directories.
                warnf("cannot process directory %s: %s", directory, err);
                return;
            }
            slice<@string> names = default;
            names = append(names, pkg.GoFiles);
            names = append(names, pkg.CgoFiles);
            names = append(names, pkg.TestGoFiles); // These are also in the "foo" package.
            names = append(names, pkg.SFiles);
            prefixDirectory(directory, names);
            var basePkg = doPackage(names, null); 
            // Is there also a "foo_test" package? If so, do that one as well.
            if (len(pkg.XTestGoFiles) > 0L)
            {
                names = pkg.XTestGoFiles;
                prefixDirectory(directory, names);
                doPackage(names, basePkg);
            }
        }

        public partial struct Package
        {
            public @string path;
            public map<ref ast.Ident, types.Object> defs;
            public map<ref ast.Ident, types.Object> uses;
            public map<ref ast.SelectorExpr, ref types.Selection> selectors;
            public map<ast.Expr, types.TypeAndValue> types;
            public map<types.Object, Span> spans;
            public slice<ref File> files;
            public ptr<types.Package> typesPkg;
        }

        // doPackage analyzes the single package constructed from the named files.
        // It returns the parsed Package or nil if none of the files have been checked.
        private static ref Package doPackage(slice<@string> names, ref Package basePkg)
        {
            slice<ref File> files = default;
            slice<ref ast.File> astFiles = default;
            var fs = token.NewFileSet();
            {
                var name__prev1 = name;

                foreach (var (_, __name) in names)
                {
                    name = __name;
                    var (data, err) = ioutil.ReadFile(name);
                    if (err != null)
                    { 
                        // Warn but continue to next package.
                        warnf("%s: %s", name, err);
                        return null;
                    }
                    checkBuildTag(name, data);
                    ref ast.File parsedFile = default;
                    if (strings.HasSuffix(name, ".go"))
                    {
                        parsedFile, err = parser.ParseFile(fs, name, data, 0L);
                        if (err != null)
                        {
                            warnf("%s: %s", name, err);
                            return null;
                        }
                        astFiles = append(astFiles, parsedFile);
                    }
                    files = append(files, ref new File(fset:fs,content:data,name:name,file:parsedFile,dead:make(map[ast.Node]bool),));
                }

                name = name__prev1;
            }

            if (len(astFiles) == 0L)
            {
                return null;
            }
            ptr<Package> pkg = @new<Package>();
            pkg.path = astFiles[0L].Name.Name;
            pkg.files = files; 
            // Type check the package.
            var errs = pkg.check(fs, astFiles);
            if (errs != null)
            {
                if (vcfg.SucceedOnTypecheckFailure)
                {
                    os.Exit(0L);
                }
                if (verbose || mustTypecheck.Value)
                {
                    foreach (var (_, err) in errs)
                    {
                        fmt.Fprintf(os.Stderr, "%v\n", err);
                    }
                    if (mustTypecheck)
                    { 
                        // This message could be silenced, and we could just exit,
                        // but it might be helpful at least at first to make clear that the
                        // above errors are coming from vet and not the compiler
                        // (they often look like compiler errors, such as "declared but not used").
                        errorf("typecheck failures");
                    }
                }
            } 

            // Check.
            var chk = make_map<ast.Node, slice<Action<ref File, ast.Node>>>();
            foreach (var (typ, set) in checkers)
            {
                {
                    var name__prev2 = name;

                    foreach (var (__name, __fn) in set)
                    {
                        name = __name;
                        fn = __fn;
                        if (vet(name))
                        {
                            chk[typ] = append(chk[typ], fn);
                        }
                    }

                    name = name__prev2;
                }

            }
            foreach (var (_, file) in files)
            {
                file.pkg = pkg;
                file.basePkg = basePkg;
                file.checkers = chk;
                if (file.file != null)
                {
                    file.walkFile(file.name, file.file);
                }
            }
            asmCheck(pkg);
            return pkg;
        }

        private static error visit(@string path, os.FileInfo f, error err)
        {
            if (err != null)
            {
                warnf("walk error: %s", err);
                return error.As(err);
            } 
            // One package per directory. Ignore the files themselves.
            if (!f.IsDir())
            {
                return error.As(null);
            }
            doPackageDir(path);
            return error.As(null);
        }

        private static bool hasFileWithSuffix(this ref Package pkg, @string suffix)
        {
            foreach (var (_, f) in pkg.files)
            {
                if (strings.HasSuffix(f.name, suffix))
                {
                    return true;
                }
            }
            return false;
        }

        // walkDir recursively walks the tree looking for Go packages.
        private static void walkDir(@string root)
        {
            filepath.Walk(root, visit);
        }

        // errorf formats the error to standard error, adding program
        // identification and a newline, and exits.
        private static void errorf(@string format, params object[] args)
        {
            args = args.Clone();

            fmt.Fprintf(os.Stderr, "vet: " + format + "\n", args);
            os.Exit(2L);
        }

        // warnf formats the error to standard error, adding program
        // identification and a newline, but does not exit.
        private static void warnf(@string format, params object[] args)
        {
            args = args.Clone();

            fmt.Fprintf(os.Stderr, "vet: " + format + "\n", args);
            setExit(1L);
        }

        // Println is fmt.Println guarded by -v.
        public static void Println(params object[] args)
        {
            args = args.Clone();

            if (!verbose.Value)
            {
                return;
            }
            fmt.Println(args);
        }

        // Printf is fmt.Printf guarded by -v.
        public static void Printf(@string format, params object[] args)
        {
            args = args.Clone();

            if (!verbose.Value)
            {
                return;
            }
            fmt.Printf(format + "\n", args);
        }

        // Bad reports an error and sets the exit code..
        private static void Bad(this ref File f, token.Pos pos, params object[] args)
        {
            f.Warn(pos, args);
            setExit(1L);
        }

        // Badf reports a formatted error and sets the exit code.
        private static void Badf(this ref File f, token.Pos pos, @string format, params object[] args)
        {
            f.Warnf(pos, format, args);
            setExit(1L);
        }

        // loc returns a formatted representation of the position.
        private static @string loc(this ref File f, token.Pos pos)
        {
            if (pos == token.NoPos)
            {
                return "";
            } 
            // Do not print columns. Because the pos often points to the start of an
            // expression instead of the inner part with the actual error, the
            // precision can mislead.
            var posn = f.fset.Position(pos);
            return fmt.Sprintf("%s:%d", posn.Filename, posn.Line);
        }

        // locPrefix returns a formatted representation of the position for use as a line prefix.
        private static @string locPrefix(this ref File f, token.Pos pos)
        {
            if (pos == token.NoPos)
            {
                return "";
            }
            return fmt.Sprintf("%s: ", f.loc(pos));
        }

        // Warn reports an error but does not set the exit code.
        private static void Warn(this ref File f, token.Pos pos, params object[] args)
        {
            fmt.Fprintf(os.Stderr, "%s%s", f.locPrefix(pos), fmt.Sprintln(args));
        }

        // Warnf reports a formatted error but does not set the exit code.
        private static void Warnf(this ref File f, token.Pos pos, @string format, params object[] args)
        {
            fmt.Fprintf(os.Stderr, "%s%s\n", f.locPrefix(pos), fmt.Sprintf(format, args));
        }

        // walkFile walks the file's tree.
        private static void walkFile(this ref File f, @string name, ref ast.File file)
        {
            Println("Checking file", name);
            ast.Walk(f, file);
        }

        // Visit implements the ast.Visitor interface.
        private static ast.Visitor Visit(this ref File f, ast.Node node)
        {
            f.updateDead(node);
            ast.Node key = default;
            switch (node.type())
            {
                case ref ast.AssignStmt _:
                    key = assignStmt;
                    break;
                case ref ast.BinaryExpr _:
                    key = binaryExpr;
                    break;
                case ref ast.CallExpr _:
                    key = callExpr;
                    break;
                case ref ast.CompositeLit _:
                    key = compositeLit;
                    break;
                case ref ast.ExprStmt _:
                    key = exprStmt;
                    break;
                case ref ast.ForStmt _:
                    key = forStmt;
                    break;
                case ref ast.FuncDecl _:
                    key = funcDecl;
                    break;
                case ref ast.FuncLit _:
                    key = funcLit;
                    break;
                case ref ast.GenDecl _:
                    key = genDecl;
                    break;
                case ref ast.InterfaceType _:
                    key = interfaceType;
                    break;
                case ref ast.RangeStmt _:
                    key = rangeStmt;
                    break;
                case ref ast.ReturnStmt _:
                    key = returnStmt;
                    break;
                case ref ast.StructType _:
                    key = structType;
                    break;
            }
            foreach (var (_, fn) in f.checkers[key])
            {
                fn(f, node);
            }
            return f;
        }

        // gofmt returns a string representation of the expression.
        private static @string gofmt(this ref File f, ast.Expr x)
        {
            f.b.Reset();
            printer.Fprint(ref f.b, f.fset, x);
            return f.b.String();
        }
    }
}
