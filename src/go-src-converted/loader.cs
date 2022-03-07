// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package loader -- go2cs converted at 2022 March 06 23:33:49 UTC
// import "golang.org/x/tools/go/loader" ==> using loader = go.golang.org.x.tools.go.loader_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\loader\loader.go
// See doc.go for package documentation and implementation notes.

using errors = go.errors_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using build = go.go.build_package;
using parser = go.go.parser_package;
using token = go.go.token_package;
using types = go.go.types_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;

using astutil = go.golang.org.x.tools.go.ast.astutil_package;
using cgo = go.golang.org.x.tools.go.@internal.cgo_package;
using System;
using System.Threading;


namespace go.golang.org.x.tools.go;

public static partial class loader_package {

private static build.ImportMode ignoreVendor = default;

private static readonly var trace = false; // show timing info for type-checking

// Config specifies the configuration for loading a whole program from
// Go source code.
// The zero value for Config is a ready-to-use default configuration.
 // show timing info for type-checking

// Config specifies the configuration for loading a whole program from
// Go source code.
// The zero value for Config is a ready-to-use default configuration.
public partial struct Config {
    public ptr<token.FileSet> Fset; // ParserMode specifies the mode to be used by the parser when
// loading source packages.
    public parser.Mode ParserMode; // TypeChecker contains options relating to the type checker.
//
// The supplied IgnoreFuncBodies is not used; the effective
// value comes from the TypeCheckFuncBodies func below.
// The supplied Import function is not used either.
    public types.Config TypeChecker; // TypeCheckFuncBodies is a predicate over package paths.
// A package for which the predicate is false will
// have its package-level declarations type checked, but not
// its function bodies; this can be used to quickly load
// dependencies from source.  If nil, all func bodies are type
// checked.
    public Func<@string, bool> TypeCheckFuncBodies; // If Build is non-nil, it is used to locate source packages.
// Otherwise &build.Default is used.
//
// By default, cgo is invoked to preprocess Go files that
// import the fake package "C".  This behaviour can be
// disabled by setting CGO_ENABLED=0 in the environment prior
// to startup, or by setting Build.CgoEnabled=false.
    public ptr<build.Context> Build; // The current directory, used for resolving relative package
// references such as "./go/loader".  If empty, os.Getwd will be
// used instead.
    public @string Cwd; // If DisplayPath is non-nil, it is used to transform each
// file name obtained from Build.Import().  This can be used
// to prevent a virtualized build.Config's file names from
// leaking into the user interface.
    public Func<@string, @string> DisplayPath; // If AllowErrors is true, Load will return a Program even
// if some of the its packages contained I/O, parser or type
// errors; such errors are accessible via PackageInfo.Errors.  If
// false, Load will fail if any package had an error.
    public bool AllowErrors; // CreatePkgs specifies a list of non-importable initial
// packages to create.  The resulting packages will appear in
// the corresponding elements of the Program.Created slice.
    public slice<PkgSpec> CreatePkgs; // ImportPkgs specifies a set of initial packages to load.
// The map keys are package paths.
//
// The map value indicates whether to load tests.  If true, Load
// will add and type-check two lists of files to the package:
// non-test files followed by in-package *_test.go files.  In
// addition, it will append the external test package (if any)
// to Program.Created.
    public map<@string, bool> ImportPkgs; // FindPackage is called during Load to create the build.Package
// for a given import path from a given directory.
// If FindPackage is nil, (*build.Context).Import is used.
// A client may use this hook to adapt to a proprietary build
// system that does not follow the "go build" layout
// conventions, for example.
//
// It must be safe to call concurrently from multiple goroutines.
    public Func<ptr<build.Context>, @string, @string, build.ImportMode, (ptr<build.Package>, error)> FindPackage; // AfterTypeCheck is called immediately after a list of files
// has been type-checked and appended to info.Files.
//
// This optional hook function is the earliest opportunity for
// the client to observe the output of the type checker,
// which may be useful to reduce analysis latency when loading
// a large program.
//
// The function is permitted to modify info.Info, for instance
// to clear data structures that are no longer needed, which can
// dramatically reduce peak memory consumption.
//
// The function may be called twice for the same PackageInfo:
// once for the files of the package and again for the
// in-package test files.
//
// It must be safe to call concurrently from multiple goroutines.
    public Action<ptr<PackageInfo>, slice<ptr<ast.File>>> AfterTypeCheck;
}

// A PkgSpec specifies a non-importable package to be created by Load.
// Files are processed first, but typically only one of Files and
// Filenames is provided.  The path needn't be globally unique.
//
// For vendoring purposes, the package's directory is the one that
// contains the first file.
public partial struct PkgSpec {
    public @string Path; // package path ("" => use package declaration)
    public slice<ptr<ast.File>> Files; // ASTs of already-parsed files
    public slice<@string> Filenames; // names of files to be parsed
}

// A Program is a Go program loaded from source as specified by a Config.
public partial struct Program {
    public ptr<token.FileSet> Fset; // the file set for this program

// Created[i] contains the initial package whose ASTs or
// filenames were supplied by Config.CreatePkgs[i], followed by
// the external test package, if any, of each package in
// Config.ImportPkgs ordered by ImportPath.
//
// NOTE: these files must not import "C".  Cgo preprocessing is
// only performed on imported packages, not ad hoc packages.
//
// TODO(adonovan): we need to copy and adapt the logic of
// goFilesPackage (from $GOROOT/src/cmd/go/build.go) and make
// Config.Import and Config.Create methods return the same kind
// of entity, essentially a build.Package.
// Perhaps we can even reuse that type directly.
    public slice<ptr<PackageInfo>> Created; // Imported contains the initially imported packages,
// as specified by Config.ImportPkgs.
    public map<@string, ptr<PackageInfo>> Imported; // AllPackages contains the PackageInfo of every package
// encountered by Load: all initial packages and all
// dependencies, including incomplete ones.
    public map<ptr<types.Package>, ptr<PackageInfo>> AllPackages; // importMap is the canonical mapping of package paths to
// packages.  It contains all Imported initial packages, but not
// Created ones, and all imported dependencies.
    public map<@string, ptr<types.Package>> importMap;
}

// PackageInfo holds the ASTs and facts derived by the type-checker
// for a single package.
//
// Not mutated once exposed via the API.
//
public partial struct PackageInfo {
    public ptr<types.Package> Pkg;
    public bool Importable; // true if 'import "Pkg.Path()"' would resolve to this
    public bool TransitivelyErrorFree; // true if Pkg and all its dependencies are free of errors
    public slice<ptr<ast.File>> Files; // syntax trees for the package's files
    public slice<error> Errors; // non-nil if the package had errors
    public ref types.Info Info => ref Info_val; // type-checker deductions.
    public @string dir; // package directory

    public ptr<types.Checker> checker; // transient type-checker state
    public Action<error> errorFunc;
}

private static @string String(this ptr<PackageInfo> _addr_info) {
    ref PackageInfo info = ref _addr_info.val;

    return info.Pkg.Path();
}

private static void appendError(this ptr<PackageInfo> _addr_info, error err) {
    ref PackageInfo info = ref _addr_info.val;

    if (info.errorFunc != null) {
        info.errorFunc(err);
    }
    else
 {
        fmt.Fprintln(os.Stderr, err);
    }
    info.Errors = append(info.Errors, err);

}

private static ptr<token.FileSet> fset(this ptr<Config> _addr_conf) {
    ref Config conf = ref _addr_conf.val;

    if (conf.Fset == null) {
        conf.Fset = token.NewFileSet();
    }
    return _addr_conf.Fset!;

}

// ParseFile is a convenience function (intended for testing) that invokes
// the parser using the Config's FileSet, which is initialized if nil.
//
// src specifies the parser input as a string, []byte, or io.Reader, and
// filename is its apparent name.  If src is nil, the contents of
// filename are read from the file system.
//
private static (ptr<ast.File>, error) ParseFile(this ptr<Config> _addr_conf, @string filename, object src) {
    ptr<ast.File> _p0 = default!;
    error _p0 = default!;
    ref Config conf = ref _addr_conf.val;
 
    // TODO(adonovan): use conf.build() etc like parseFiles does.
    return _addr_parser.ParseFile(conf.fset(), filename, src, conf.ParserMode)!;

}

// FromArgsUsage is a partial usage message that applications calling
// FromArgs may wish to include in their -help output.
public static readonly @string FromArgsUsage = @"
<args> is a list of arguments denoting a set of initial packages.
It may take one of two forms:

1. A list of *.go source files.

   All of the specified files are loaded, parsed and type-checked
   as a single package.  All the files must belong to the same directory.

2. A list of import paths, each denoting a package.

   The package's directory is found relative to the $GOROOT and
   $GOPATH using similar logic to 'go build', and the *.go files in
   that directory are loaded, parsed and type-checked as a single
   package.

   In addition, all *_test.go files in the directory are then loaded
   and parsed.  Those files whose package declaration equals that of
   the non-*_test.go files are included in the primary package.  Test
   files whose package declaration ends with ""_test"" are type-checked
   as another package, the 'external' test package, so that a single
   import path may denote two packages.  (Whether this behaviour is
   enabled is tool-specific, and may depend on additional flags.)

A '--' argument terminates the list of packages.
";

// FromArgs interprets args as a set of initial packages to load from
// source and updates the configuration.  It returns the list of
// unconsumed arguments.
//
// It is intended for use in command-line interfaces that require a
// set of initial packages to be specified; see FromArgsUsage message
// for details.
//
// Only superficial errors are reported at this stage; errors dependent
// on I/O are detected during Load.
//


// FromArgs interprets args as a set of initial packages to load from
// source and updates the configuration.  It returns the list of
// unconsumed arguments.
//
// It is intended for use in command-line interfaces that require a
// set of initial packages to be specified; see FromArgsUsage message
// for details.
//
// Only superficial errors are reported at this stage; errors dependent
// on I/O are detected during Load.
//
private static (slice<@string>, error) FromArgs(this ptr<Config> _addr_conf, slice<@string> args, bool xtest) {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref Config conf = ref _addr_conf.val;

    slice<@string> rest = default;
    {
        var arg__prev1 = arg;

        foreach (var (__i, __arg) in args) {
            i = __i;
            arg = __arg;
            if (arg == "--") {
                rest = args[(int)i + 1..];
                args = args[..(int)i];
                break; // consume "--" and return the remaining args
            }

        }
        arg = arg__prev1;
    }

    if (len(args) > 0 && strings.HasSuffix(args[0], ".go")) { 
        // Assume args is a list of a *.go files
        // denoting a single ad hoc package.
        {
            var arg__prev1 = arg;

            foreach (var (_, __arg) in args) {
                arg = __arg;
                if (!strings.HasSuffix(arg, ".go")) {
                    return (null, error.As(fmt.Errorf("named files must be .go files: %s", arg))!);
                }
            }
    else

            arg = arg__prev1;
        }

        conf.CreateFromFilenames("", args);

    } { 
        // Assume args are directories each denoting a
        // package and (perhaps) an external test, iff xtest.
        {
            var arg__prev1 = arg;

            foreach (var (_, __arg) in args) {
                arg = __arg;
                if (xtest) {
                    conf.ImportWithTests(arg);
                }
                else
 {
                    conf.Import(arg);
                }

            }

            arg = arg__prev1;
        }
    }
    return (rest, error.As(null!)!);

}

// CreateFromFilenames is a convenience function that adds
// a conf.CreatePkgs entry to create a package of the specified *.go
// files.
//
private static void CreateFromFilenames(this ptr<Config> _addr_conf, @string path, params @string[] filenames) {
    filenames = filenames.Clone();
    ref Config conf = ref _addr_conf.val;

    conf.CreatePkgs = append(conf.CreatePkgs, new PkgSpec(Path:path,Filenames:filenames));
}

// CreateFromFiles is a convenience function that adds a conf.CreatePkgs
// entry to create package of the specified path and parsed files.
//
private static void CreateFromFiles(this ptr<Config> _addr_conf, @string path, params ptr<ptr<ast.File>>[] _addr_files) {
    files = files.Clone();
    ref Config conf = ref _addr_conf.val;
    ref ast.File files = ref _addr_files.val;

    conf.CreatePkgs = append(conf.CreatePkgs, new PkgSpec(Path:path,Files:files));
}

// ImportWithTests is a convenience function that adds path to
// ImportPkgs, the set of initial source packages located relative to
// $GOPATH.  The package will be augmented by any *_test.go files in
// its directory that contain a "package x" (not "package x_test")
// declaration.
//
// In addition, if any *_test.go files contain a "package x_test"
// declaration, an additional package comprising just those files will
// be added to CreatePkgs.
//
private static void ImportWithTests(this ptr<Config> _addr_conf, @string path) {
    ref Config conf = ref _addr_conf.val;

    conf.addImport(path, true);
}

// Import is a convenience function that adds path to ImportPkgs, the
// set of initial packages that will be imported from source.
//
private static void Import(this ptr<Config> _addr_conf, @string path) {
    ref Config conf = ref _addr_conf.val;

    conf.addImport(path, false);
}

private static void addImport(this ptr<Config> _addr_conf, @string path, bool tests) {
    ref Config conf = ref _addr_conf.val;

    if (path == "C") {
        return ; // ignore; not a real package
    }
    if (conf.ImportPkgs == null) {
        conf.ImportPkgs = make_map<@string, bool>();
    }
    conf.ImportPkgs[path] = conf.ImportPkgs[path] || tests;

}

// PathEnclosingInterval returns the PackageInfo and ast.Node that
// contain source interval [start, end), and all the node's ancestors
// up to the AST root.  It searches all ast.Files of all packages in prog.
// exact is defined as for astutil.PathEnclosingInterval.
//
// The zero value is returned if not found.
//
private static (ptr<PackageInfo>, slice<ast.Node>, bool) PathEnclosingInterval(this ptr<Program> _addr_prog, token.Pos start, token.Pos end) {
    ptr<PackageInfo> pkg = default!;
    slice<ast.Node> path = default;
    bool exact = default;
    ref Program prog = ref _addr_prog.val;

    foreach (var (_, info) in prog.AllPackages) {
        foreach (var (_, f) in info.Files) {
            if (f.Pos() == token.NoPos) { 
                // This can happen if the parser saw
                // too many errors and bailed out.
                // (Use parser.AllErrors to prevent that.)
                continue;

            }

            if (!tokenFileContainsPos(prog.Fset.File(f.Pos()), start)) {
                continue;
            }

            {
                var (path, exact) = astutil.PathEnclosingInterval(f, start, end);

                if (path != null) {
                    return (_addr_info!, path, exact);
                }

            }

        }
    }    return (_addr_null!, null, false);

}

// InitialPackages returns a new slice containing the set of initial
// packages (Created + Imported) in unspecified order.
//
private static slice<ptr<PackageInfo>> InitialPackages(this ptr<Program> _addr_prog) {
    ref Program prog = ref _addr_prog.val;

    var infos = make_slice<ptr<PackageInfo>>(0, len(prog.Created) + len(prog.Imported));
    infos = append(infos, prog.Created);
    foreach (var (_, info) in prog.Imported) {
        infos = append(infos, info);
    }    return infos;
}

// Package returns the ASTs and results of type checking for the
// specified package.
private static ptr<PackageInfo> Package(this ptr<Program> _addr_prog, @string path) {
    ref Program prog = ref _addr_prog.val;

    {
        var info__prev1 = info;

        var (info, ok) = prog.AllPackages[prog.importMap[path]];

        if (ok) {
            return _addr_info!;
        }
        info = info__prev1;

    }

    {
        var info__prev1 = info;

        foreach (var (_, __info) in prog.Created) {
            info = __info;
            if (path == info.Pkg.Path()) {
                return _addr_info!;
            }
        }
        info = info__prev1;
    }

    return _addr_null!;

}

// ---------- Implementation ----------

// importer holds the working state of the algorithm.
private partial struct importer {
    public ptr<Config> conf; // the client configuration
    public time.Time start; // for logging

    public sync.Mutex progMu; // guards prog
    public ptr<Program> prog; // the resulting program

// findpkg is a memoization of FindPackage.
    public sync.Mutex findpkgMu; // guards findpkg
    public map<findpkgKey, ptr<findpkgValue>> findpkg;
    public sync.Mutex importedMu; // guards imported
    public map<@string, ptr<importInfo>> imported; // all imported packages (incl. failures) by import path

// import dependency graph: graph[x][y] => x imports y
//
// Since non-importable packages cannot be cyclic, we ignore
// their imports, thus we only need the subgraph over importable
// packages.  Nodes are identified by their import paths.
    public sync.Mutex graphMu;
    public map<@string, map<@string, bool>> graph;
}

private partial struct findpkgKey {
    public @string importPath;
    public @string fromDir;
    public build.ImportMode mode;
}

private partial struct findpkgValue {
    public channel<object> ready; // closed to broadcast readiness
    public ptr<build.Package> bp;
    public error err;
}

// importInfo tracks the success or failure of a single import.
//
// Upon completion, exactly one of info and err is non-nil:
// info on successful creation of a package, err otherwise.
// A successful package may still contain type errors.
//
private partial struct importInfo {
    public @string path; // import path
    public ptr<PackageInfo> info; // results of typechecking (including errors)
    public channel<object> complete; // closed to broadcast that info is set.
}

// awaitCompletion blocks until ii is complete,
// i.e. the info field is safe to inspect.
private static void awaitCompletion(this ptr<importInfo> _addr_ii) {
    ref importInfo ii = ref _addr_ii.val;

    ii.complete.Receive(); // wait for close
}

// Complete marks ii as complete.
// Its info and err fields will not be subsequently updated.
private static void Complete(this ptr<importInfo> _addr_ii, ptr<PackageInfo> _addr_info) => func((_, panic, _) => {
    ref importInfo ii = ref _addr_ii.val;
    ref PackageInfo info = ref _addr_info.val;

    if (info == null) {
        panic("info == nil");
    }
    ii.info = info;
    close(ii.complete);

});

private partial struct importError {
    public @string path; // import path
    public error err; // reason for failure to create a package
}

// Load creates the initial packages specified by conf.{Create,Import}Pkgs,
// loading their dependencies packages as needed.
//
// On success, Load returns a Program containing a PackageInfo for
// each package.  On failure, it returns an error.
//
// If AllowErrors is true, Load will return a Program even if some
// packages contained I/O, parser or type errors, or if dependencies
// were missing.  (Such errors are accessible via PackageInfo.Errors.  If
// false, Load will fail if any package had an error.
//
// It is an error if no packages were loaded.
//
private static (ptr<Program>, error) Load(this ptr<Config> _addr_conf) => func((_, panic, _) => {
    ptr<Program> _p0 = default!;
    error _p0 = default!;
    ref Config conf = ref _addr_conf.val;
 
    // Create a simple default error handler for parse/type errors.
    if (conf.TypeChecker.Error == null) {
        conf.TypeChecker.Error = e => {
            fmt.Fprintln(os.Stderr, e);
        };

    }
    if (conf.Cwd == "") {
        error err = default!;
        conf.Cwd, err = os.Getwd();
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }
    if (conf.FindPackage == null) {
        conf.FindPackage = (build.Context.val).Import;
    }
    ptr<Program> prog = addr(new Program(Fset:conf.fset(),Imported:make(map[string]*PackageInfo),importMap:make(map[string]*types.Package),AllPackages:make(map[*types.Package]*PackageInfo),));

    importer imp = new importer(conf:conf,prog:prog,findpkg:make(map[findpkgKey]*findpkgValue),imported:make(map[string]*importInfo),start:time.Now(),graph:make(map[string]map[string]bool),); 

    // -- loading proper (concurrent phase) --------------------------------

    slice<@string> errpkgs = default; // packages that contained errors

    // Load the initially imported packages and their dependencies,
    // in parallel.
    // No vendor check on packages imported from the command line.
    var (infos, importErrors) = imp.importAll("", conf.Cwd, conf.ImportPkgs, ignoreVendor);
    foreach (var (_, ie) in importErrors) {
        conf.TypeChecker.Error(ie.err); // failed to create package
        errpkgs = append(errpkgs, ie.path);

    }    {
        var info__prev1 = info;

        foreach (var (_, __info) in infos) {
            info = __info;
            prog.Imported[info.Pkg.Path()] = info;
        }
        info = info__prev1;
    }

    slice<ptr<build.Package>> xtestPkgs = default;
    foreach (var (importPath, augment) in conf.ImportPkgs) {
        if (!augment) {
            continue;
        }
        var (bp, err) = imp.findPackage(importPath, conf.Cwd, ignoreVendor);
        if (err != null) { 
            // Package not found, or can't even parse package declaration.
            // Already reported by previous loop; ignore it.
            continue;

        }
        if (len(bp.XTestGoFiles) > 0) {
            xtestPkgs = append(xtestPkgs, bp);
        }
        var path = bp.ImportPath;
        imp.importedMu.Lock(); // (unnecessary, we're sequential here)
        var (ii, ok) = imp.imported[path]; 
        // Paranoid checks added due to issue #11012.
        if (!ok) { 
            // Unreachable.
            // The previous loop called importAll and thus
            // startLoad for each path in ImportPkgs, which
            // populates imp.imported[path] with a non-zero value.
            panic(fmt.Sprintf("imported[%q] not found", path));

        }
        if (ii == null) { 
            // Unreachable.
            // The ii values in this loop are the same as in
            // the previous loop, which enforced the invariant
            // that at least one of ii.err and ii.info is non-nil.
            panic(fmt.Sprintf("imported[%q] == nil", path));

        }
        if (ii.info == null) { 
            // Unreachable.
            // awaitCompletion has the postcondition
            // ii.info != nil.
            panic(fmt.Sprintf("imported[%q].info = nil", path));

        }
        var info = ii.info;
        imp.importedMu.Unlock(); 

        // Parse the in-package test files.
        var (files, errs) = imp.conf.parsePackageFiles(bp, 't');
        {
            error err__prev2 = err;

            foreach (var (_, __err) in errs) {
                err = __err;
                info.appendError(err);
            } 

            // The test files augmenting package P cannot be imported,
            // but may import packages that import P,
            // so we must disable the cycle check.

            err = err__prev2;
        }

        imp.addFiles(info, files, false);

    }    Action<@string, @string, slice<ptr<ast.File>>, slice<error>> createPkg = (path, dir, files, errs) => {
        info = imp.newPackageInfo(path, dir);
        {
            error err__prev1 = err;

            foreach (var (_, __err) in errs) {
                err = __err;
                info.appendError(err);
            } 

            // Ad hoc packages are non-importable,
            // so no cycle check is needed.
            // addFiles loads dependencies in parallel.

            err = err__prev1;
        }

        imp.addFiles(info, files, false);
        prog.Created = append(prog.Created, info);

    }; 

    // Create packages specified by conf.CreatePkgs.
    foreach (var (_, cp) in conf.CreatePkgs) {
        (files, errs) = parseFiles(conf.fset(), conf.build(), null, conf.Cwd, cp.Filenames, conf.ParserMode);
        files = append(files, cp.Files);

        path = cp.Path;
        if (path == "") {
            if (len(files) > 0) {
                path = files[0].Name.Name;
            }
            else
 {
                path = "(unnamed)";
            }

        }
        var dir = conf.Cwd;
        if (len(files) > 0 && files[0].Pos().IsValid()) {
            dir = filepath.Dir(conf.fset().File(files[0].Pos()).Name());
        }
        createPkg(path, dir, files, errs);

    }    sort.Sort(byImportPath(xtestPkgs));
    {
        var bp__prev1 = bp;

        foreach (var (_, __bp) in xtestPkgs) {
            bp = __bp;
            (files, errs) = imp.conf.parsePackageFiles(bp, 'x');
            createPkg(bp.ImportPath + "_test", bp.Dir, files, errs);
        }
        bp = bp__prev1;
    }

    if (len(prog.Imported) + len(prog.Created) == 0) {
        return (_addr_null!, error.As(errors.New("no initial packages were loaded"))!);
    }
    foreach (var (_, obj) in prog.importMap) {
        info = prog.AllPackages[obj];
        if (info == null) {
            prog.AllPackages[obj] = addr(new PackageInfo(Pkg:obj,Importable:true));
        }
        else
 { 
            // finished
            info.checker = null;
            info.errorFunc = null;

        }
    }    if (!conf.AllowErrors) { 
        // Report errors in indirectly imported packages.
        {
            var info__prev1 = info;

            foreach (var (_, __info) in prog.AllPackages) {
                info = __info;
                if (len(info.Errors) > 0) {
                    errpkgs = append(errpkgs, info.Pkg.Path());
                }
            }

            info = info__prev1;
        }

        if (errpkgs != null) {
            @string more = default;
            if (len(errpkgs) > 3) {
                more = fmt.Sprintf(" and %d more", len(errpkgs) - 3);
                errpkgs = errpkgs[..(int)3];
            }
            return (_addr_null!, error.As(fmt.Errorf("couldn't load packages due to errors: %s%s", strings.Join(errpkgs, ", "), more))!);
        }
    }
    markErrorFreePackages(prog.AllPackages);

    return (_addr_prog!, error.As(null!)!);

});

private partial struct byImportPath { // : slice<ptr<build.Package>>
}

private static nint Len(this byImportPath b) {
    return len(b);
}
private static bool Less(this byImportPath b, nint i, nint j) {
    return b[i].ImportPath < b[j].ImportPath;
}
private static void Swap(this byImportPath b, nint i, nint j) {
    (b[i], b[j]) = (b[j], b[i]);
}

// markErrorFreePackages sets the TransitivelyErrorFree flag on all
// applicable packages.
private static void markErrorFreePackages(map<ptr<types.Package>, ptr<PackageInfo>> allPackages) { 
    // Build the transpose of the import graph.
    var importedBy = make_map<ptr<types.Package>, map<ptr<types.Package>, bool>>();
    foreach (var (P) in allPackages) {
        foreach (var (_, Q) in P.Imports()) {
            var (clients, ok) = importedBy[Q];
            if (!ok) {
                clients = make_map<ptr<types.Package>, bool>();
                importedBy[Q] = clients;
            }
            clients[P] = true;
        }
    }    var reachable = make_map<ptr<types.Package>, bool>();
    Action<ptr<types.Package>> visit = default;
    visit = p => {
        if (!reachable[p]) {
            reachable[p] = true;
            foreach (var (q) in importedBy[p]) {
                visit(q);
            }
        }
    };
    {
        var info__prev1 = info;

        foreach (var (_, __info) in allPackages) {
            info = __info;
            if (len(info.Errors) > 0) {
                visit(info.Pkg);
            }
        }
        info = info__prev1;
    }

    {
        var info__prev1 = info;

        foreach (var (_, __info) in allPackages) {
            info = __info;
            if (!reachable[info.Pkg]) {
                info.TransitivelyErrorFree = true;
            }
        }
        info = info__prev1;
    }
}

// build returns the effective build context.
private static ptr<build.Context> build(this ptr<Config> _addr_conf) {
    ref Config conf = ref _addr_conf.val;

    if (conf.Build != null) {
        return _addr_conf.Build!;
    }
    return _addr__addr_build.Default!;

}

// parsePackageFiles enumerates the files belonging to package path,
// then loads, parses and returns them, plus a list of I/O or parse
// errors that were encountered.
//
// 'which' indicates which files to include:
//    'g': include non-test *.go source files (GoFiles + processed CgoFiles)
//    't': include in-package *_test.go source files (TestGoFiles)
//    'x': include external *_test.go source files. (XTestGoFiles)
//
private static (slice<ptr<ast.File>>, slice<error>) parsePackageFiles(this ptr<Config> _addr_conf, ptr<build.Package> _addr_bp, int which) => func((_, panic, _) => {
    slice<ptr<ast.File>> _p0 = default;
    slice<error> _p0 = default;
    ref Config conf = ref _addr_conf.val;
    ref build.Package bp = ref _addr_bp.val;

    if (bp.ImportPath == "unsafe") {
        return (null, null);
    }
    slice<@string> filenames = default;
    switch (which) {
        case 'g': 
            filenames = bp.GoFiles;
            break;
        case 't': 
            filenames = bp.TestGoFiles;
            break;
        case 'x': 
            filenames = bp.XTestGoFiles;
            break;
        default: 
            panic(which);
            break;
    }

    var (files, errs) = parseFiles(conf.fset(), conf.build(), conf.DisplayPath, bp.Dir, filenames, conf.ParserMode); 

    // Preprocess CgoFiles and parse the outputs (sequentially).
    if (which == 'g' && bp.CgoFiles != null) {
        var (cgofiles, err) = cgo.ProcessFiles(bp, conf.fset(), conf.DisplayPath, conf.ParserMode);
        if (err != null) {
            errs = append(errs, err);
        }
        else
 {
            files = append(files, cgofiles);
        }
    }
    return (files, errs);

});

// doImport imports the package denoted by path.
// It implements the types.Importer signature.
//
// It returns an error if a package could not be created
// (e.g. go/build or parse error), but type errors are reported via
// the types.Config.Error callback (the first of which is also saved
// in the package's PackageInfo).
//
// Idempotent.
//
private static (ptr<types.Package>, error) doImport(this ptr<importer> _addr_imp, ptr<PackageInfo> _addr_from, @string to) => func((_, panic, _) => {
    ptr<types.Package> _p0 = default!;
    error _p0 = default!;
    ref importer imp = ref _addr_imp.val;
    ref PackageInfo from = ref _addr_from.val;

    if (to == "C") { 
        // This should be unreachable, but ad hoc packages are
        // not currently subject to cgo preprocessing.
        // See https://golang.org/issue/11627.
        return (_addr_null!, error.As(fmt.Errorf("the loader doesn\'t cgo-process ad hoc packages like %q; see Go issue 11627", from.Pkg.Path()))!);

    }
    var (bp, err) = imp.findPackage(to, from.dir, 0);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    if (bp.ImportPath == "unsafe") {
        return (_addr_types.Unsafe!, error.As(null!)!);
    }
    var path = bp.ImportPath;
    imp.importedMu.Lock();
    var ii = imp.imported[path];
    imp.importedMu.Unlock();
    if (ii == null) {
        panic("internal error: unexpected import: " + path);
    }
    if (ii.info != null) {
        return (_addr_ii.info.Pkg!, error.As(null!)!);
    }
    var fromPath = from.Pkg.Path();
    {
        var cycle = imp.findPath(path, fromPath);

        if (cycle != null) { 
            // Normalize cycle: start from alphabetically largest node.
            nint pos = -1;
            @string start = "";
            foreach (var (i, s) in cycle) {
                if (pos < 0 || s > start) {
                    (pos, start) = (i, s);
                }

            }
            cycle = append(cycle, cycle[..(int)pos])[(int)pos..]; // rotate cycle to start from largest
            cycle = append(cycle, cycle[0]); // add start node to end to show cycliness
            return (_addr_null!, error.As(fmt.Errorf("import cycle: %s", strings.Join(cycle, " -> ")))!);

        }
    }


    panic("internal error: import of incomplete (yet acyclic) package: " + fromPath);

});

// findPackage locates the package denoted by the importPath in the
// specified directory.
private static (ptr<build.Package>, error) findPackage(this ptr<importer> _addr_imp, @string importPath, @string fromDir, build.ImportMode mode) {
    ptr<build.Package> _p0 = default!;
    error _p0 = default!;
    ref importer imp = ref _addr_imp.val;
 
    // We use a non-blocking duplicate-suppressing cache (gopl.io §9.7)
    // to avoid holding the lock around FindPackage.
    findpkgKey key = new findpkgKey(importPath,fromDir,mode);
    imp.findpkgMu.Lock();
    var (v, ok) = imp.findpkg[key];
    if (ok) { 
        // cache hit
        imp.findpkgMu.Unlock().Send(v.ready); // wait for entry to become ready
    }
    else
 { 
        // Cache miss: this goroutine becomes responsible for
        // populating the map entry and broadcasting its readiness.
        v = addr(new findpkgValue(ready:make(chanstruct{})));
        imp.findpkg[key] = v;
        imp.findpkgMu.Unlock();

        ioLimit.Send(true);
        v.bp, v.err = imp.conf.FindPackage(imp.conf.build(), importPath, fromDir, mode);
        ioLimit.Receive();

        {
            ptr<build.NoGoError> (_, ok) = v.err._<ptr<build.NoGoError>>();

            if (ok) {
                v.err = null; // empty directory is not an error
            }

        }


        close(v.ready); // broadcast ready condition
    }
    return (_addr_v.bp!, error.As(v.err)!);

}

// importAll loads, parses, and type-checks the specified packages in
// parallel and returns their completed importInfos in unspecified order.
//
// fromPath is the package path of the importing package, if it is
// importable, "" otherwise.  It is used for cycle detection.
//
// fromDir is the directory containing the import declaration that
// caused these imports.
//
private static (slice<ptr<PackageInfo>>, slice<importError>) importAll(this ptr<importer> _addr_imp, @string fromPath, @string fromDir, map<@string, bool> imports, build.ImportMode mode) {
    slice<ptr<PackageInfo>> infos = default;
    slice<importError> errors = default;
    ref importer imp = ref _addr_imp.val;
 
    // TODO(adonovan): opt: do the loop in parallel once
    // findPackage is non-blocking.
    slice<ptr<importInfo>> pending = default;
    foreach (var (importPath) in imports) {
        var (bp, err) = imp.findPackage(importPath, fromDir, mode);
        if (err != null) {
            errors = append(errors, new importError(path:importPath,err:err,));
            continue;
        }
        pending = append(pending, imp.startLoad(bp));

    }    if (fromPath != "") { 
        // We're loading a set of imports.
        //
        // We must record graph edges from the importing package
        // to its dependencies, and check for cycles.
        imp.graphMu.Lock();
        var (deps, ok) = imp.graph[fromPath];
        if (!ok) {
            deps = make_map<@string, bool>();
            imp.graph[fromPath] = deps;
        }
        {
            var ii__prev1 = ii;

            foreach (var (_, __ii) in pending) {
                ii = __ii;
                deps[ii.path] = true;
            }

            ii = ii__prev1;
        }

        imp.graphMu.Unlock();

    }
    {
        var ii__prev1 = ii;

        foreach (var (_, __ii) in pending) {
            ii = __ii;
            if (fromPath != "") {
                {
                    var cycle = imp.findPath(ii.path, fromPath);

                    if (cycle != null) { 
                        // Cycle-forming import: we must not await its
                        // completion since it would deadlock.
                        //
                        // We don't record the error in ii since
                        // the error is really associated with the
                        // cycle-forming edge, not the package itself.
                        // (Also it would complicate the
                        // invariants of importPath completion.)
                        if (trace) {
                            fmt.Fprintf(os.Stderr, "import cycle: %q\n", cycle);
                        }

                        continue;

                    }

                }

            }

            ii.awaitCompletion();
            infos = append(infos, ii.info);

        }
        ii = ii__prev1;
    }

    return (infos, errors);

}

// findPath returns an arbitrary path from 'from' to 'to' in the import
// graph, or nil if there was none.
private static slice<@string> findPath(this ptr<importer> _addr_imp, @string from, @string to) => func((defer, _, _) => {
    ref importer imp = ref _addr_imp.val;

    imp.graphMu.Lock();
    defer(imp.graphMu.Unlock());

    var seen = make_map<@string, bool>();
    Func<slice<@string>, @string, slice<@string>> search = default;
    search = (stack, importPath) => {
        if (!seen[importPath]) {
            seen[importPath] = true;
            stack = append(stack, importPath);
            if (importPath == to) {
                return stack;
            }
            foreach (var (x) in imp.graph[importPath]) {
                {
                    var p = search(stack, x);

                    if (p != null) {
                        return p;
                    }

                }

            }

        }
        return null;

    };
    return search(make_slice<@string>(0, 20), from);

});

// startLoad initiates the loading, parsing and type-checking of the
// specified package and its dependencies, if it has not already begun.
//
// It returns an importInfo, not necessarily in a completed state.  The
// caller must call awaitCompletion() before accessing its info field.
//
// startLoad is concurrency-safe and idempotent.
//
private static ptr<importInfo> startLoad(this ptr<importer> _addr_imp, ptr<build.Package> _addr_bp) {
    ref importer imp = ref _addr_imp.val;
    ref build.Package bp = ref _addr_bp.val;

    var path = bp.ImportPath;
    imp.importedMu.Lock();
    var (ii, ok) = imp.imported[path];
    if (!ok) {
        ii = addr(new importInfo(path:path,complete:make(chanstruct{})));
        imp.imported[path] = ii;
        go_(() => () => {
            var info = imp.load(bp);
            ii.Complete(info);
        }());
    }
    imp.importedMu.Unlock();

    return _addr_ii!;

}

// load implements package loading by parsing Go source files
// located by go/build.
private static ptr<PackageInfo> load(this ptr<importer> _addr_imp, ptr<build.Package> _addr_bp) {
    ref importer imp = ref _addr_imp.val;
    ref build.Package bp = ref _addr_bp.val;

    var info = imp.newPackageInfo(bp.ImportPath, bp.Dir);
    info.Importable = true;
    var (files, errs) = imp.conf.parsePackageFiles(bp, 'g');
    foreach (var (_, err) in errs) {
        info.appendError(err);
    }    imp.addFiles(info, files, true);

    imp.progMu.Lock();
    imp.prog.importMap[bp.ImportPath] = info.Pkg;
    imp.progMu.Unlock();

    return _addr_info!;
}

// addFiles adds and type-checks the specified files to info, loading
// their dependencies if needed.  The order of files determines the
// package initialization order.  It may be called multiple times on the
// same package.  Errors are appended to the info.Errors field.
//
// cycleCheck determines whether the imports within files create
// dependency edges that should be checked for potential cycles.
//
private static void addFiles(this ptr<importer> _addr_imp, ptr<PackageInfo> _addr_info, slice<ptr<ast.File>> files, bool cycleCheck) => func((_, panic, _) => {
    ref importer imp = ref _addr_imp.val;
    ref PackageInfo info = ref _addr_info.val;
 
    // Ensure the dependencies are loaded, in parallel.
    @string fromPath = default;
    if (cycleCheck) {
        fromPath = info.Pkg.Path();
    }
    imp.importAll(fromPath, info.dir, scanImports(files), 0);

    if (trace) {
        fmt.Fprintf(os.Stderr, "%s: start %q (%d)\n", time.Since(imp.start), info.Pkg.Path(), len(files));
    }
    if (info.Pkg == types.Unsafe) {
        if (len(files) > 0) {
            panic("\"unsafe\" package contains unexpected files");
        }
    }
    else
 { 
        // Ignore the returned (first) error since we
        // already collect them all in the PackageInfo.
        info.checker.Files(files);
        info.Files = append(info.Files, files);

    }
    if (imp.conf.AfterTypeCheck != null) {
        imp.conf.AfterTypeCheck(info, files);
    }
    if (trace) {
        fmt.Fprintf(os.Stderr, "%s: stop %q\n", time.Since(imp.start), info.Pkg.Path());
    }
});

private static ptr<PackageInfo> newPackageInfo(this ptr<importer> _addr_imp, @string path, @string dir) {
    ref importer imp = ref _addr_imp.val;

    ptr<types.Package> pkg;
    if (path == "unsafe") {
        pkg = types.Unsafe;
    }
    else
 {
        pkg = types.NewPackage(path, "");
    }
    ptr<PackageInfo> info = addr(new PackageInfo(Pkg:pkg,Info:types.Info{Types:make(map[ast.Expr]types.TypeAndValue),Defs:make(map[*ast.Ident]types.Object),Uses:make(map[*ast.Ident]types.Object),Implicits:make(map[ast.Node]types.Object),Scopes:make(map[ast.Node]*types.Scope),Selections:make(map[*ast.SelectorExpr]*types.Selection),},errorFunc:imp.conf.TypeChecker.Error,dir:dir,)); 

    // Copy the types.Config so we can vary it across PackageInfos.
    ref var tc = ref heap(imp.conf.TypeChecker, out ptr<var> _addr_tc);
    tc.IgnoreFuncBodies = false;
    {
        var f = imp.conf.TypeCheckFuncBodies;

        if (f != null) {
            tc.IgnoreFuncBodies = !f(path);
        }
    }

    tc.Importer = new closure(imp,info);
    tc.Error = info.appendError; // appendError wraps the user's Error function

    info.checker = types.NewChecker(_addr_tc, imp.conf.fset(), pkg, _addr_info.Info);
    imp.progMu.Lock();
    imp.prog.AllPackages[pkg] = info;
    imp.progMu.Unlock();
    return _addr_info!;

}

private partial struct closure {
    public ptr<importer> imp;
    public ptr<PackageInfo> info;
}

private static (ptr<types.Package>, error) Import(this closure c, @string to) {
    ptr<types.Package> _p0 = default!;
    error _p0 = default!;

    return _addr_c.imp.doImport(c.info, to)!;
}

} // end loader_package
