// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package build -- go2cs converted at 2022 March 06 22:41:16 UTC
// import "go/build" ==> using build = go.go.build_package
// Original source: C:\Program Files\Go\src\go\build\build.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using constraint = go.go.build.constraint_package;
using doc = go.go.doc_package;
using token = go.go.token_package;
using buildcfg = go.@internal.buildcfg_package;
using exec = go.@internal.execabs_package;
using goroot = go.@internal.goroot_package;
using goversion = go.@internal.goversion_package;
using io = go.io_package;
using fs = go.io.fs_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using pathpkg = go.path_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using System;


namespace go.go;

public static partial class build_package {

    // A Context specifies the supporting context for a build.
public partial struct Context {
    public @string GOARCH; // target architecture
    public @string GOOS; // target operating system
    public @string GOROOT; // Go root
    public @string GOPATH; // Go path

// Dir is the caller's working directory, or the empty string to use
// the current directory of the running process. In module mode, this is used
// to locate the main module.
//
// If Dir is non-empty, directories passed to Import and ImportDir must
// be absolute.
    public @string Dir;
    public bool CgoEnabled; // whether cgo files are included
    public bool UseAllFiles; // use files regardless of +build lines, file names
    public @string Compiler; // compiler to assume when computing target paths

// The build, tool, and release tags specify build constraints
// that should be considered satisfied when processing +build lines.
// Clients creating a new context may customize BuildTags, which
// defaults to empty, but it is usually an error to customize ToolTags or ReleaseTags.
// ToolTags defaults to build tags appropriate to the current Go toolchain configuration.
// ReleaseTags defaults to the list of Go releases the current release is compatible with.
// BuildTags is not set for the Default build Context.
// In addition to the BuildTags, ToolTags, and ReleaseTags, build constraints
// consider the values of GOARCH and GOOS as satisfied tags.
// The last element in ReleaseTags is assumed to be the current release.
    public slice<@string> BuildTags;
    public slice<@string> ToolTags;
    public slice<@string> ReleaseTags; // The install suffix specifies a suffix to use in the name of the installation
// directory. By default it is empty, but custom builds that need to keep
// their outputs separate can set InstallSuffix to do so. For example, when
// using the race detector, the go command uses InstallSuffix = "race", so
// that on a Linux/386 system, packages are written to a directory named
// "linux_386_race" instead of the usual "linux_386".
    public @string InstallSuffix; // By default, Import uses the operating system's file system calls
// to read directories and files. To read from other sources,
// callers can set the following functions. They all have default
// behaviors that use the local file system, so clients need only set
// the functions whose behaviors they wish to change.

// JoinPath joins the sequence of path fragments into a single path.
// If JoinPath is nil, Import uses filepath.Join.
    public Func<@string[], @string> JoinPath; // SplitPathList splits the path list into a slice of individual paths.
// If SplitPathList is nil, Import uses filepath.SplitList.
    public Func<@string, slice<@string>> SplitPathList; // IsAbsPath reports whether path is an absolute path.
// If IsAbsPath is nil, Import uses filepath.IsAbs.
    public Func<@string, bool> IsAbsPath; // IsDir reports whether the path names a directory.
// If IsDir is nil, Import calls os.Stat and uses the result's IsDir method.
    public Func<@string, bool> IsDir; // HasSubdir reports whether dir is lexically a subdirectory of
// root, perhaps multiple levels below. It does not try to check
// whether dir exists.
// If so, HasSubdir sets rel to a slash-separated path that
// can be joined to root to produce a path equivalent to dir.
// If HasSubdir is nil, Import uses an implementation built on
// filepath.EvalSymlinks.
    public Func<@string, @string, (@string, bool)> HasSubdir; // ReadDir returns a slice of fs.FileInfo, sorted by Name,
// describing the content of the named directory.
// If ReadDir is nil, Import uses ioutil.ReadDir.
    public Func<@string, (slice<fs.FileInfo>, error)> ReadDir; // OpenFile opens a file (not a directory) for reading.
// If OpenFile is nil, Import uses os.Open.
    public Func<@string, (io.ReadCloser, error)> OpenFile;
}

// joinPath calls ctxt.JoinPath (if not nil) or else filepath.Join.
private static @string joinPath(this ptr<Context> _addr_ctxt, params @string[] elem) {
    elem = elem.Clone();
    ref Context ctxt = ref _addr_ctxt.val;

    {
        var f = ctxt.JoinPath;

        if (f != null) {
            return f(elem);
        }
    }

    return filepath.Join(elem);

}

// splitPathList calls ctxt.SplitPathList (if not nil) or else filepath.SplitList.
private static slice<@string> splitPathList(this ptr<Context> _addr_ctxt, @string s) {
    ref Context ctxt = ref _addr_ctxt.val;

    {
        var f = ctxt.SplitPathList;

        if (f != null) {
            return f(s);
        }
    }

    return filepath.SplitList(s);

}

// isAbsPath calls ctxt.IsAbsPath (if not nil) or else filepath.IsAbs.
private static bool isAbsPath(this ptr<Context> _addr_ctxt, @string path) {
    ref Context ctxt = ref _addr_ctxt.val;

    {
        var f = ctxt.IsAbsPath;

        if (f != null) {
            return f(path);
        }
    }

    return filepath.IsAbs(path);

}

// isDir calls ctxt.IsDir (if not nil) or else uses os.Stat.
private static bool isDir(this ptr<Context> _addr_ctxt, @string path) {
    ref Context ctxt = ref _addr_ctxt.val;

    {
        var f = ctxt.IsDir;

        if (f != null) {
            return f(path);
        }
    }

    var (fi, err) = os.Stat(path);
    return err == null && fi.IsDir();

}

// hasSubdir calls ctxt.HasSubdir (if not nil) or else uses
// the local file system to answer the question.
private static (@string, bool) hasSubdir(this ptr<Context> _addr_ctxt, @string root, @string dir) {
    @string rel = default;
    bool ok = default;
    ref Context ctxt = ref _addr_ctxt.val;

    {
        var f = ctxt.HasSubdir;

        if (f != null) {
            return f(root, dir);
        }
    } 

    // Try using paths we received.
    rel, ok = hasSubdir(root, dir);

    if (ok) {
        return ;
    }
    var (rootSym, _) = filepath.EvalSymlinks(root);
    var (dirSym, _) = filepath.EvalSymlinks(dir);

    rel, ok = hasSubdir(rootSym, dir);

    if (ok) {
        return ;
    }
    rel, ok = hasSubdir(root, dirSym);

    if (ok) {
        return ;
    }
    return hasSubdir(rootSym, dirSym);

}

// hasSubdir reports if dir is within root by performing lexical analysis only.
private static (@string, bool) hasSubdir(@string root, @string dir) {
    @string rel = default;
    bool ok = default;

    const var sep = string(filepath.Separator);

    root = filepath.Clean(root);
    if (!strings.HasSuffix(root, sep)) {
        root += sep;
    }
    dir = filepath.Clean(dir);
    if (!strings.HasPrefix(dir, root)) {
        return ("", false);
    }
    return (filepath.ToSlash(dir[(int)len(root)..]), true);

}

// readDir calls ctxt.ReadDir (if not nil) or else ioutil.ReadDir.
private static (slice<fs.FileInfo>, error) readDir(this ptr<Context> _addr_ctxt, @string path) {
    slice<fs.FileInfo> _p0 = default;
    error _p0 = default!;
    ref Context ctxt = ref _addr_ctxt.val;

    {
        var f = ctxt.ReadDir;

        if (f != null) {
            return f(path);
        }
    } 
    // TODO: use os.ReadDir
    return ioutil.ReadDir(path);

}

// openFile calls ctxt.OpenFile (if not nil) or else os.Open.
private static (io.ReadCloser, error) openFile(this ptr<Context> _addr_ctxt, @string path) {
    io.ReadCloser _p0 = default;
    error _p0 = default!;
    ref Context ctxt = ref _addr_ctxt.val;

    {
        var fn = ctxt.OpenFile;

        if (fn != null) {
            return fn(path);
        }
    }


    var (f, err) = os.Open(path);
    if (err != null) {
        return (null, error.As(err)!); // nil interface
    }
    return (f, error.As(null!)!);

}

// isFile determines whether path is a file by trying to open it.
// It reuses openFile instead of adding another function to the
// list in Context.
private static bool isFile(this ptr<Context> _addr_ctxt, @string path) {
    ref Context ctxt = ref _addr_ctxt.val;

    var (f, err) = ctxt.openFile(path);
    if (err != null) {
        return false;
    }
    f.Close();
    return true;

}

// gopath returns the list of Go path directories.
private static slice<@string> gopath(this ptr<Context> _addr_ctxt) {
    ref Context ctxt = ref _addr_ctxt.val;

    slice<@string> all = default;
    foreach (var (_, p) in ctxt.splitPathList(ctxt.GOPATH)) {
        if (p == "" || p == ctxt.GOROOT) { 
            // Empty paths are uninteresting.
            // If the path is the GOROOT, ignore it.
            // People sometimes set GOPATH=$GOROOT.
            // Do not get confused by this common mistake.
            continue;

        }
        if (strings.HasPrefix(p, "~")) { 
            // Path segments starting with ~ on Unix are almost always
            // users who have incorrectly quoted ~ while setting GOPATH,
            // preventing it from expanding to $HOME.
            // The situation is made more confusing by the fact that
            // bash allows quoted ~ in $PATH (most shells do not).
            // Do not get confused by this, and do not try to use the path.
            // It does not exist, and printing errors about it confuses
            // those users even more, because they think "sure ~ exists!".
            // The go command diagnoses this situation and prints a
            // useful error.
            // On Windows, ~ is used in short names, such as c:\progra~1
            // for c:\program files.
            continue;

        }
        all = append(all, p);

    }    return all;

}

// SrcDirs returns a list of package source root directories.
// It draws from the current Go root and Go path but omits directories
// that do not exist.
private static slice<@string> SrcDirs(this ptr<Context> _addr_ctxt) {
    ref Context ctxt = ref _addr_ctxt.val;

    slice<@string> all = default;
    if (ctxt.GOROOT != "" && ctxt.Compiler != "gccgo") {
        var dir = ctxt.joinPath(ctxt.GOROOT, "src");
        if (ctxt.isDir(dir)) {
            all = append(all, dir);
        }
    }
    foreach (var (_, p) in ctxt.gopath()) {
        dir = ctxt.joinPath(p, "src");
        if (ctxt.isDir(dir)) {
            all = append(all, dir);
        }
    }    return all;

}

// Default is the default Context for builds.
// It uses the GOARCH, GOOS, GOROOT, and GOPATH environment variables
// if set, or else the compiled code's GOARCH, GOOS, and GOROOT.
public static Context Default = defaultContext();

private static @string defaultGOPATH() {
    @string env = "HOME";
    if (runtime.GOOS == "windows") {
        env = "USERPROFILE";
    }
    else if (runtime.GOOS == "plan9") {
        env = "home";
    }
    {
        var home = os.Getenv(env);

        if (home != "") {
            var def = filepath.Join(home, "go");
            if (filepath.Clean(def) == filepath.Clean(runtime.GOROOT())) { 
                // Don't set the default GOPATH to GOROOT,
                // as that will trigger warnings from the go tool.
                return "";

            }

            return def;

        }
    }

    return "";

}

private static slice<@string> defaultToolTags = default;private static slice<@string> defaultReleaseTags = default;



private static Context defaultContext() {
    Context c = default;

    c.GOARCH = buildcfg.GOARCH;
    c.GOOS = buildcfg.GOOS;
    c.GOROOT = pathpkg.Clean(runtime.GOROOT());
    c.GOPATH = envOr("GOPATH", defaultGOPATH());
    c.Compiler = runtime.Compiler; 

    // For each experiment that has been enabled in the toolchain, define a
    // build tag with the same name but prefixed by "goexperiment." which can be
    // used for compiling alternative files for the experiment. This allows
    // changes for the experiment, like extra struct fields in the runtime,
    // without affecting the base non-experiment code at all.
    foreach (var (_, exp) in buildcfg.EnabledExperiments()) {
        c.ToolTags = append(c.ToolTags, "goexperiment." + exp);
    }    defaultToolTags = append(new slice<@string>(new @string[] {  }), c.ToolTags); // our own private copy

    // Each major Go release in the Go 1.x series adds a new
    // "go1.x" release tag. That is, the go1.x tag is present in
    // all releases >= Go 1.x. Code that requires Go 1.x or later
    // should say "+build go1.x", and code that should only be
    // built before Go 1.x (perhaps it is the stub to use in that
    // case) should say "+build !go1.x".
    // The last element in ReleaseTags is the current release.
    for (nint i = 1; i <= goversion.Version; i++) {
        c.ReleaseTags = append(c.ReleaseTags, "go1." + strconv.Itoa(i));
    }

    defaultReleaseTags = append(new slice<@string>(new @string[] {  }), c.ReleaseTags); // our own private copy

    var env = os.Getenv("CGO_ENABLED");
    if (env == "") {
        env = defaultCGO_ENABLED;
    }
    switch (env) {
        case "1": 
            c.CgoEnabled = true;
            break;
        case "0": 
            c.CgoEnabled = false;
            break;
        default: 
            // cgo must be explicitly enabled for cross compilation builds
            if (runtime.GOARCH == c.GOARCH && runtime.GOOS == c.GOOS) {
                c.CgoEnabled = cgoEnabled[c.GOOS + "/" + c.GOARCH];
                break;
            }
            c.CgoEnabled = false;

            break;
    }

    return c;

}

private static @string envOr(@string name, @string def) {
    var s = os.Getenv(name);
    if (s == "") {
        return def;
    }
    return s;

}

// An ImportMode controls the behavior of the Import method.
public partial struct ImportMode { // : nuint
}

 
// If FindOnly is set, Import stops after locating the directory
// that should contain the sources for a package. It does not
// read any files in the directory.
public static readonly ImportMode FindOnly = 1 << (int)(iota); 

// If AllowBinary is set, Import can be satisfied by a compiled
// package object without corresponding sources.
//
// Deprecated:
// The supported way to create a compiled-only package is to
// write source code containing a //go:binary-only-package comment at
// the top of the file. Such a package will be recognized
// regardless of this flag setting (because it has source code)
// and will have BinaryOnly set to true in the returned Package.
public static readonly var AllowBinary = 0; 

// If ImportComment is set, parse import comments on package statements.
// Import returns an error if it finds a comment it cannot understand
// or finds conflicting comments in multiple source files.
// See golang.org/s/go14customimport for more information.
public static readonly var ImportComment = 1; 

// By default, Import searches vendor directories
// that apply in the given source directory before searching
// the GOROOT and GOPATH roots.
// If an Import finds and returns a package using a vendor
// directory, the resulting ImportPath is the complete path
// to the package, including the path elements leading up
// to and including "vendor".
// For example, if Import("y", "x/subdir", 0) finds
// "x/vendor/y", the returned package's ImportPath is "x/vendor/y",
// not plain "y".
// See golang.org/s/go15vendor for more information.
//
// Setting IgnoreVendor ignores vendor directories.
//
// In contrast to the package's ImportPath,
// the returned package's Imports, TestImports, and XTestImports
// are always the exact import paths from the source files:
// Import makes no attempt to resolve or check those paths.
public static readonly var IgnoreVendor = 2;


// A Package describes the Go package found in a directory.
public partial struct Package {
    public @string Dir; // directory containing package sources
    public @string Name; // package name
    public @string ImportComment; // path in import comment on package statement
    public @string Doc; // documentation synopsis
    public @string ImportPath; // import path of package ("" if unknown)
    public @string Root; // root of Go tree where this package lives
    public @string SrcRoot; // package source root directory ("" if unknown)
    public @string PkgRoot; // package install root directory ("" if unknown)
    public @string PkgTargetRoot; // architecture dependent install root directory ("" if unknown)
    public @string BinDir; // command install directory ("" if unknown)
    public bool Goroot; // package found in Go root
    public @string PkgObj; // installed .a file
    public slice<@string> AllTags; // tags that can influence file selection in this directory
    public @string ConflictDir; // this directory shadows Dir in $GOPATH
    public bool BinaryOnly; // cannot be rebuilt from source (has //go:binary-only-package comment)

// Source files
    public slice<@string> GoFiles; // .go source files (excluding CgoFiles, TestGoFiles, XTestGoFiles)
    public slice<@string> CgoFiles; // .go source files that import "C"
    public slice<@string> IgnoredGoFiles; // .go source files ignored for this build (including ignored _test.go files)
    public slice<@string> InvalidGoFiles; // .go source files with detected problems (parse error, wrong package name, and so on)
    public slice<@string> IgnoredOtherFiles; // non-.go source files ignored for this build
    public slice<@string> CFiles; // .c source files
    public slice<@string> CXXFiles; // .cc, .cpp and .cxx source files
    public slice<@string> MFiles; // .m (Objective-C) source files
    public slice<@string> HFiles; // .h, .hh, .hpp and .hxx source files
    public slice<@string> FFiles; // .f, .F, .for and .f90 Fortran source files
    public slice<@string> SFiles; // .s source files
    public slice<@string> SwigFiles; // .swig files
    public slice<@string> SwigCXXFiles; // .swigcxx files
    public slice<@string> SysoFiles; // .syso system object files to add to archive

// Cgo directives
    public slice<@string> CgoCFLAGS; // Cgo CFLAGS directives
    public slice<@string> CgoCPPFLAGS; // Cgo CPPFLAGS directives
    public slice<@string> CgoCXXFLAGS; // Cgo CXXFLAGS directives
    public slice<@string> CgoFFLAGS; // Cgo FFLAGS directives
    public slice<@string> CgoLDFLAGS; // Cgo LDFLAGS directives
    public slice<@string> CgoPkgConfig; // Cgo pkg-config directives

// Test information
    public slice<@string> TestGoFiles; // _test.go files in package
    public slice<@string> XTestGoFiles; // _test.go files outside package

// Dependency information
    public slice<@string> Imports; // import paths from GoFiles, CgoFiles
    public map<@string, slice<token.Position>> ImportPos; // line information for Imports
    public slice<@string> TestImports; // import paths from TestGoFiles
    public map<@string, slice<token.Position>> TestImportPos; // line information for TestImports
    public slice<@string> XTestImports; // import paths from XTestGoFiles
    public map<@string, slice<token.Position>> XTestImportPos; // line information for XTestImports

// //go:embed patterns found in Go source files
// For example, if a source file says
//    //go:embed a* b.c
// then the list will contain those two strings as separate entries.
// (See package embed for more details about //go:embed.)
    public slice<@string> EmbedPatterns; // patterns from GoFiles, CgoFiles
    public map<@string, slice<token.Position>> EmbedPatternPos; // line information for EmbedPatterns
    public slice<@string> TestEmbedPatterns; // patterns from TestGoFiles
    public map<@string, slice<token.Position>> TestEmbedPatternPos; // line information for TestEmbedPatterns
    public slice<@string> XTestEmbedPatterns; // patterns from XTestGoFiles
    public map<@string, slice<token.Position>> XTestEmbedPatternPos; // line information for XTestEmbedPatternPos
}

// IsCommand reports whether the package is considered a
// command to be installed (not just a library).
// Packages named "main" are treated as commands.
private static bool IsCommand(this ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;

    return p.Name == "main";
}

// ImportDir is like Import but processes the Go package found in
// the named directory.
private static (ptr<Package>, error) ImportDir(this ptr<Context> _addr_ctxt, @string dir, ImportMode mode) {
    ptr<Package> _p0 = default!;
    error _p0 = default!;
    ref Context ctxt = ref _addr_ctxt.val;

    return _addr_ctxt.Import(".", dir, mode)!;
}

// NoGoError is the error used by Import to describe a directory
// containing no buildable Go source files. (It may still contain
// test files, files hidden by build tags, and so on.)
public partial struct NoGoError {
    public @string Dir;
}

private static @string Error(this ptr<NoGoError> _addr_e) {
    ref NoGoError e = ref _addr_e.val;

    return "no buildable Go source files in " + e.Dir;
}

// MultiplePackageError describes a directory containing
// multiple buildable Go source files for multiple packages.
public partial struct MultiplePackageError {
    public @string Dir; // directory containing files
    public slice<@string> Packages; // package names found
    public slice<@string> Files; // corresponding files: Files[i] declares package Packages[i]
}

private static @string Error(this ptr<MultiplePackageError> _addr_e) {
    ref MultiplePackageError e = ref _addr_e.val;
 
    // Error string limited to two entries for compatibility.
    return fmt.Sprintf("found packages %s (%s) and %s (%s) in %s", e.Packages[0], e.Files[0], e.Packages[1], e.Files[1], e.Dir);

}

private static @string nameExt(@string name) {
    var i = strings.LastIndex(name, ".");
    if (i < 0) {
        return "";
    }
    return name[(int)i..];

}

// Import returns details about the Go package named by the import path,
// interpreting local import paths relative to the srcDir directory.
// If the path is a local import path naming a package that can be imported
// using a standard import path, the returned package will set p.ImportPath
// to that path.
//
// In the directory containing the package, .go, .c, .h, and .s files are
// considered part of the package except for:
//
//    - .go files in package documentation
//    - files starting with _ or . (likely editor temporary files)
//    - files with build constraints not satisfied by the context
//
// If an error occurs, Import returns a non-nil error and a non-nil
// *Package containing partial information.
//
private static (ptr<Package>, error) Import(this ptr<Context> _addr_ctxt, @string path, @string srcDir, ImportMode mode) {
    ptr<Package> _p0 = default!;
    error _p0 = default!;
    ref Context ctxt = ref _addr_ctxt.val;

    ptr<Package> p = addr(new Package(ImportPath:path,));
    if (path == "") {
        return (_addr_p!, error.As(fmt.Errorf("import %q: invalid import path", path))!);
    }
    @string pkgtargetroot = default;
    @string pkga = default;
    error pkgerr = default!;
    @string suffix = "";
    if (ctxt.InstallSuffix != "") {
        suffix = "_" + ctxt.InstallSuffix;
    }
    switch (ctxt.Compiler) {
        case "gccgo": 
            pkgtargetroot = "pkg/gccgo_" + ctxt.GOOS + "_" + ctxt.GOARCH + suffix;
            break;
        case "gc": 
            pkgtargetroot = "pkg/" + ctxt.GOOS + "_" + ctxt.GOARCH + suffix;
            break;
        default: 
            // Save error for end of function.
            pkgerr = error.As(fmt.Errorf("import %q: unknown compiler %q", path, ctxt.Compiler))!;
            break;
    }
    Action setPkga = () => {
        switch (ctxt.Compiler) {
            case "gccgo": 
                var (dir, elem) = pathpkg.Split(p.ImportPath);
                pkga = pkgtargetroot + "/" + dir + "lib" + elem + ".a";
                break;
            case "gc": 
                pkga = pkgtargetroot + "/" + p.ImportPath + ".a";
                break;
        }

    };
    setPkga();

    var binaryOnly = false;
    if (IsLocalImport(path)) {
        pkga = ""; // local imports have no installed path
        if (srcDir == "") {
            return (_addr_p!, error.As(fmt.Errorf("import %q: import relative to unknown directory", path))!);
        }
        if (!ctxt.isAbsPath(path)) {
            p.Dir = ctxt.joinPath(srcDir, path);
        }
        Func<@string, bool> inTestdata = sub => {
            return _addr_strings.Contains(sub, "/testdata/") || strings.HasSuffix(sub, "/testdata") || strings.HasPrefix(sub, "testdata/") || sub == "testdata"!;
        }
    else
;
        if (ctxt.GOROOT != "") {
            var root = ctxt.joinPath(ctxt.GOROOT, "src");
            {
                var sub__prev3 = sub;

                var (sub, ok) = ctxt.hasSubdir(root, p.Dir);

                if (ok && !inTestdata(sub)) {
                    p.Goroot = true;
                    p.ImportPath = sub;
                    p.Root = ctxt.GOROOT;
                    setPkga(); // p.ImportPath changed
                    goto Found;

                }

                sub = sub__prev3;

            }

        }
        var all = ctxt.gopath();
        {
            var i__prev1 = i;
            var root__prev1 = root;

            foreach (var (__i, __root) in all) {
                i = __i;
                root = __root;
                var rootsrc = ctxt.joinPath(root, "src");
                {
                    var sub__prev2 = sub;

                    (sub, ok) = ctxt.hasSubdir(rootsrc, p.Dir);

                    if (ok && !inTestdata(sub)) { 
                        // We found a potential import path for dir,
                        // but check that using it wouldn't find something
                        // else first.
                        if (ctxt.GOROOT != "" && ctxt.Compiler != "gccgo") {
                            {
                                var dir__prev4 = dir;

                                var dir = ctxt.joinPath(ctxt.GOROOT, "src", sub);

                                if (ctxt.isDir(dir)) {
                                    p.ConflictDir = dir;
                                    goto Found;
                                }

                                dir = dir__prev4;

                            }

                        }

                        foreach (var (_, earlyRoot) in all[..(int)i]) {
                            {
                                var dir__prev3 = dir;

                                dir = ctxt.joinPath(earlyRoot, "src", sub);

                                if (ctxt.isDir(dir)) {
                                    p.ConflictDir = dir;
                                    goto Found;
                                }

                                dir = dir__prev3;

                            }

                        } 

                        // sub would not name some other directory instead of this one.
                        // Record it.
                        p.ImportPath = sub;
                        p.Root = root;
                        setPkga(); // p.ImportPath changed
                        goto Found;

                    }

                    sub = sub__prev2;

                }

            } 
            // It's okay that we didn't find a root containing dir.
            // Keep going with the information we have.

            i = i__prev1;
            root = root__prev1;
        }
    } {
        if (strings.HasPrefix(path, "/")) {
            return (_addr_p!, error.As(fmt.Errorf("import %q: cannot import absolute path", path))!);
        }
        {
            var err__prev2 = err;

            var err = ctxt.importGo(p, path, srcDir, mode);

            if (err == null) {
                goto Found;
            }
            else if (err != errNoModules) {
                return (_addr_p!, error.As(err)!);
            }


            err = err__prev2;

        }


        var gopath = ctxt.gopath(); // needed twice below; avoid computing many times

        // tried records the location of unsuccessful package lookups
        var tried = default; 

        // Vendor directories get first chance to satisfy import.
        if (mode & IgnoreVendor == 0 && srcDir != "") {
            Func<@string, bool, bool> searchVendor = (root, isGoroot) => {
                (sub, ok) = ctxt.hasSubdir(root, srcDir);
                if (!ok || !strings.HasPrefix(sub, "src/") || strings.Contains(sub, "/testdata/")) {
                    return _addr_false!;
                }
                while (true) {
                    var vendor = ctxt.joinPath(root, sub, "vendor");
                    if (ctxt.isDir(vendor)) {
                        dir = ctxt.joinPath(vendor, path);
                        if (ctxt.isDir(dir) && hasGoFiles(_addr_ctxt, dir)) {
                            p.Dir = dir;
                            p.ImportPath = strings.TrimPrefix(pathpkg.Join(sub, "vendor", path), "src/");
                            p.Goroot = isGoroot;
                            p.Root = root;
                            setPkga(); // p.ImportPath changed
                            return _addr_true!;

                        }

                        tried.vendor = append(tried.vendor, dir);

                    }

                    var i = strings.LastIndex(sub, "/");
                    if (i < 0) {
                        break;
                    }

                    sub = sub[..(int)i];

                }

                return _addr_false!;

            }
;
            if (ctxt.Compiler != "gccgo" && searchVendor(ctxt.GOROOT, true)) {
                goto Found;
            }

            {
                var root__prev1 = root;

                foreach (var (_, __root) in gopath) {
                    root = __root;
                    if (searchVendor(root, false)) {
                        goto Found;
                    }
                }

                root = root__prev1;
            }
        }
        if (ctxt.GOROOT != "") { 
            // If the package path starts with "vendor/", only search GOROOT before
            // GOPATH if the importer is also within GOROOT. That way, if the user has
            // vendored in a package that is subsequently included in the standard
            // distribution, they'll continue to pick up their own vendored copy.
            var gorootFirst = srcDir == "" || !strings.HasPrefix(path, "vendor/");
            if (!gorootFirst) {
                _, gorootFirst = ctxt.hasSubdir(ctxt.GOROOT, srcDir);
            }

            if (gorootFirst) {
                dir = ctxt.joinPath(ctxt.GOROOT, "src", path);
                if (ctxt.Compiler != "gccgo") {
                    var isDir = ctxt.isDir(dir);
                    binaryOnly = !isDir && mode & AllowBinary != 0 && pkga != "" && ctxt.isFile(ctxt.joinPath(ctxt.GOROOT, pkga));
                    if (isDir || binaryOnly) {
                        p.Dir = dir;
                        p.Goroot = true;
                        p.Root = ctxt.GOROOT;
                        goto Found;
                    }
                }
                tried.goroot = dir;
            }

        }
        if (ctxt.Compiler == "gccgo" && goroot.IsStandardPackage(ctxt.GOROOT, ctxt.Compiler, path)) {
            p.Dir = ctxt.joinPath(ctxt.GOROOT, "src", path);
            p.Goroot = true;
            p.Root = ctxt.GOROOT;
            goto Found;
        }
        {
            var root__prev1 = root;

            foreach (var (_, __root) in gopath) {
                root = __root;
                dir = ctxt.joinPath(root, "src", path);
                isDir = ctxt.isDir(dir);
                binaryOnly = !isDir && mode & AllowBinary != 0 && pkga != "" && ctxt.isFile(ctxt.joinPath(root, pkga));
                if (isDir || binaryOnly) {
                    p.Dir = dir;
                    p.Root = root;
                    goto Found;
                }
                tried.gopath = append(tried.gopath, dir);
            } 

            // If we tried GOPATH first due to a "vendor/" prefix, fall back to GOPATH.
            // That way, the user can still get useful results from 'go list' for
            // standard-vendored paths passed on the command line.

            root = root__prev1;
        }

        if (ctxt.GOROOT != "" && tried.goroot == "") {
            dir = ctxt.joinPath(ctxt.GOROOT, "src", path);
            if (ctxt.Compiler != "gccgo") {
                isDir = ctxt.isDir(dir);
                binaryOnly = !isDir && mode & AllowBinary != 0 && pkga != "" && ctxt.isFile(ctxt.joinPath(ctxt.GOROOT, pkga));
                if (isDir || binaryOnly) {
                    p.Dir = dir;
                    p.Goroot = true;
                    p.Root = ctxt.GOROOT;
                    goto Found;
                }
            }
            tried.goroot = dir;
        }
        slice<@string> paths = default;
        @string format = "\t%s (vendor tree)";
        {
            var dir__prev1 = dir;

            foreach (var (_, __dir) in tried.vendor) {
                dir = __dir;
                paths = append(paths, fmt.Sprintf(format, dir));
                format = "\t%s";
            }

            dir = dir__prev1;
        }

        if (tried.goroot != "") {
            paths = append(paths, fmt.Sprintf("\t%s (from $GOROOT)", tried.goroot));
        }
        else
 {
            paths = append(paths, "\t($GOROOT not set)");
        }
        format = "\t%s (from $GOPATH)";
        {
            var dir__prev1 = dir;

            foreach (var (_, __dir) in tried.gopath) {
                dir = __dir;
                paths = append(paths, fmt.Sprintf(format, dir));
                format = "\t%s";
            }

            dir = dir__prev1;
        }

        if (len(tried.gopath) == 0) {
            paths = append(paths, "\t($GOPATH not set. For more details see: 'go help gopath')");
        }
        return (_addr_p!, error.As(fmt.Errorf("cannot find package %q in any of:\n%s", path, strings.Join(paths, "\n")))!);

    }
Found: 

    // If it's a local import path, by the time we get here, we still haven't checked
    // that p.Dir directory exists. This is the right time to do that check.
    // We can't do it earlier, because we want to gather partial information for the
    // non-nil *Package returned when an error occurs.
    // We need to do this before we return early on FindOnly flag.
    if (p.Root != "") {
        p.SrcRoot = ctxt.joinPath(p.Root, "src");
        p.PkgRoot = ctxt.joinPath(p.Root, "pkg");
        p.BinDir = ctxt.joinPath(p.Root, "bin");
        if (pkga != "") {
            p.PkgTargetRoot = ctxt.joinPath(p.Root, pkgtargetroot);
            p.PkgObj = ctxt.joinPath(p.Root, pkga);
        }
    }
    if (IsLocalImport(path) && !ctxt.isDir(p.Dir)) {
        if (ctxt.Compiler == "gccgo" && p.Goroot) { 
            // gccgo has no sources for GOROOT packages.
            return (_addr_p!, error.As(null!)!);

        }
        return (_addr_p!, error.As(fmt.Errorf("cannot find package %q in:\n\t%s", path, p.Dir))!);

    }
    if (mode & FindOnly != 0) {
        return (_addr_p!, error.As(pkgerr)!);
    }
    if (binaryOnly && (mode & AllowBinary) != 0) {
        return (_addr_p!, error.As(pkgerr)!);
    }
    if (ctxt.Compiler == "gccgo" && p.Goroot) { 
        // gccgo has no sources for GOROOT packages.
        return (_addr_p!, error.As(null!)!);

    }
    var (dirs, err) = ctxt.readDir(p.Dir);
    if (err != null) {
        return (_addr_p!, error.As(err)!);
    }
    error badGoError = default!;
    var badFiles = make_map<@string, bool>();
    Action<@string, error> badFile = (name, err) => {
        if (badGoError == null) {
            badGoError = error.As(err)!;
        }
        if (!badFiles[name]) {
            p.InvalidGoFiles = append(p.InvalidGoFiles, name);
            badFiles[name] = true;
        }
    };

    slice<@string> Sfiles = default; // files with ".S"(capital S)/.sx(capital s equivalent for case insensitive filesystems)
    @string firstFile = default;    @string firstCommentFile = default;

    var embedPos = make_map<@string, slice<token.Position>>();
    var testEmbedPos = make_map<@string, slice<token.Position>>();
    var xTestEmbedPos = make_map<@string, slice<token.Position>>();
    var importPos = make_map<@string, slice<token.Position>>();
    var testImportPos = make_map<@string, slice<token.Position>>();
    var xTestImportPos = make_map<@string, slice<token.Position>>();
    var allTags = make_map<@string, bool>();
    var fset = token.NewFileSet();
    foreach (var (_, d) in dirs) {
        if (d.IsDir()) {
            continue;
        }
        if (d.Mode() & fs.ModeSymlink != 0) {
            if (ctxt.isDir(ctxt.joinPath(p.Dir, d.Name()))) { 
                // Symlinks to directories are not source files.
                continue;

            }

        }
        var name = d.Name();
        var ext = nameExt(name);

        var (info, err) = ctxt.matchFile(p.Dir, name, allTags, _addr_p.BinaryOnly, fset);
        if (err != null) {
            badFile(name, err);
            continue;
        }
        if (info == null) {
            if (strings.HasPrefix(name, "_") || strings.HasPrefix(name, ".")) { 
                // not due to build constraints - don't report
            }
            else if (ext == ".go") {
                p.IgnoredGoFiles = append(p.IgnoredGoFiles, name);
            }
            else if (fileListForExt(p, ext) != null) {
                p.IgnoredOtherFiles = append(p.IgnoredOtherFiles, name);
            }

            continue;

        }
        var data = info.header;
        var filename = info.name; 

        // Going to save the file. For non-Go files, can stop here.
        switch (ext) {
            case ".go": 

                break;
            case ".S": 
                // special case for cgo, handled at end

            case ".sx": 
                // special case for cgo, handled at end
                Sfiles = append(Sfiles, name);
                continue;
                break;
            default: 
                {
                    var list = fileListForExt(p, ext);

                    if (list != null) {
                        list.val = append(list.val, name);
                    }

                }

                continue;

                break;
        }

        if (info.parseErr != null) {
            badFile(name, info.parseErr); 
            // Fall through: we might still have a partial AST in info.parsed,
            // and we want to list files with parse errors anyway.
        }
        @string pkg = default;
        if (info.parsed != null) {
            pkg = info.parsed.Name.Name;
            if (pkg == "documentation") {
                p.IgnoredGoFiles = append(p.IgnoredGoFiles, name);
                continue;
            }
        }
        var isTest = strings.HasSuffix(name, "_test.go");
        var isXTest = false;
        if (isTest && strings.HasSuffix(pkg, "_test")) {
            isXTest = true;
            pkg = pkg[..(int)len(pkg) - len("_test")];
        }
        if (p.Name == "") {
            p.Name = pkg;
            firstFile = name;
        }
        else if (pkg != p.Name) { 
            // TODO(#45999): The choice of p.Name is arbitrary based on file iteration
            // order. Instead of resolving p.Name arbitrarily, we should clear out the
            // existing name and mark the existing files as also invalid.
            badFile(name, addr(new MultiplePackageError(Dir:p.Dir,Packages:[]string{p.Name,pkg},Files:[]string{firstFile,name},)));

        }
        if (info.parsed != null && info.parsed.Doc != null && p.Doc == "" && !isTest && !isXTest) {
            p.Doc = doc.Synopsis(info.parsed.Doc.Text());
        }
        if (mode & ImportComment != 0) {
            var (qcom, line) = findImportComment(data);
            if (line != 0) {
                var (com, err) = strconv.Unquote(qcom);
                if (err != null) {
                    badFile(name, fmt.Errorf("%s:%d: cannot parse import comment", filename, line));
                }
                else if (p.ImportComment == "") {
                    p.ImportComment = com;
                    firstCommentFile = name;
                }
                else if (p.ImportComment != com) {
                    badFile(name, fmt.Errorf("found import comments %q (%s) and %q (%s) in %s", p.ImportComment, firstCommentFile, com, name, p.Dir));
                }

            }

        }
        var isCgo = false;
        {
            var imp__prev2 = imp;

            foreach (var (_, __imp) in info.imports) {
                imp = __imp;
                if (imp.path == "C") {
                    if (isTest) {
                        badFile(name, fmt.Errorf("use of cgo in test %s not supported", filename));
                        continue;
                    }
                    isCgo = true;
                    if (imp.doc != null) {
                        {
                            var err__prev3 = err;

                            err = ctxt.saveCgo(filename, p, imp.doc);

                            if (err != null) {
                                badFile(name, err);
                            }

                            err = err__prev3;

                        }

                    }

                }

            }

            imp = imp__prev2;
        }

        ptr<slice<@string>> fileList;
        map<@string, slice<token.Position>> importMap = default;        map<@string, slice<token.Position>> embedMap = default;


        if (isCgo) 
            allTags["cgo"] = true;
            if (ctxt.CgoEnabled) {
                fileList = _addr_p.CgoFiles;
                importMap = importPos;
                embedMap = embedPos;
            }
            else
 { 
                // Ignore imports and embeds from cgo files if cgo is disabled.
                fileList = _addr_p.IgnoredGoFiles;

            }

        else if (isXTest) 
            fileList = _addr_p.XTestGoFiles;
            importMap = xTestImportPos;
            embedMap = xTestEmbedPos;
        else if (isTest) 
            fileList = _addr_p.TestGoFiles;
            importMap = testImportPos;
            embedMap = testEmbedPos;
        else 
            fileList = _addr_p.GoFiles;
            importMap = importPos;
            embedMap = embedPos;
                fileList.val = append(fileList.val, name);
        if (importMap != null) {
            {
                var imp__prev2 = imp;

                foreach (var (_, __imp) in info.imports) {
                    imp = __imp;
                    importMap[imp.path] = append(importMap[imp.path], fset.Position(imp.pos));
                }

                imp = imp__prev2;
            }
        }
        if (embedMap != null) {
            foreach (var (_, emb) in info.embeds) {
                embedMap[emb.pattern] = append(embedMap[emb.pattern], emb.pos);
            }
        }
    }    foreach (var (tag) in allTags) {
        p.AllTags = append(p.AllTags, tag);
    }    sort.Strings(p.AllTags);

    p.EmbedPatterns, p.EmbedPatternPos = cleanDecls(embedPos);
    p.TestEmbedPatterns, p.TestEmbedPatternPos = cleanDecls(testEmbedPos);
    p.XTestEmbedPatterns, p.XTestEmbedPatternPos = cleanDecls(xTestEmbedPos);

    p.Imports, p.ImportPos = cleanDecls(importPos);
    p.TestImports, p.TestImportPos = cleanDecls(testImportPos);
    p.XTestImports, p.XTestImportPos = cleanDecls(xTestImportPos); 

    // add the .S/.sx files only if we are using cgo
    // (which means gcc will compile them).
    // The standard assemblers expect .s files.
    if (len(p.CgoFiles) > 0) {
        p.SFiles = append(p.SFiles, Sfiles);
        sort.Strings(p.SFiles);
    }
    else
 {
        p.IgnoredOtherFiles = append(p.IgnoredOtherFiles, Sfiles);
        sort.Strings(p.IgnoredOtherFiles);
    }
    if (badGoError != null) {
        return (_addr_p!, error.As(badGoError)!);
    }
    if (len(p.GoFiles) + len(p.CgoFiles) + len(p.TestGoFiles) + len(p.XTestGoFiles) == 0) {
        return (_addr_p!, error.As(addr(new NoGoError(p.Dir))!)!);
    }
    return (_addr_p!, error.As(pkgerr)!);

}

private static ptr<slice<@string>> fileListForExt(ptr<Package> _addr_p, @string ext) {
    ref Package p = ref _addr_p.val;

    switch (ext) {
        case ".c": 
            return _addr__addr_p.CFiles!;
            break;
        case ".cc": 

        case ".cpp": 

        case ".cxx": 
            return _addr__addr_p.CXXFiles!;
            break;
        case ".m": 
            return _addr__addr_p.MFiles!;
            break;
        case ".h": 

        case ".hh": 

        case ".hpp": 

        case ".hxx": 
            return _addr__addr_p.HFiles!;
            break;
        case ".f": 

        case ".F": 

        case ".for": 

        case ".f90": 
            return _addr__addr_p.FFiles!;
            break;
        case ".s": 

        case ".S": 

        case ".sx": 
            return _addr__addr_p.SFiles!;
            break;
        case ".swig": 
            return _addr__addr_p.SwigFiles!;
            break;
        case ".swigcxx": 
            return _addr__addr_p.SwigCXXFiles!;
            break;
        case ".syso": 
            return _addr__addr_p.SysoFiles!;
            break;
    }
    return _addr_null!;

}

private static slice<@string> uniq(slice<@string> list) {
    if (list == null) {
        return null;
    }
    var @out = make_slice<@string>(len(list));
    copy(out, list);
    sort.Strings(out);
    var uniq = out[..(int)0];
    foreach (var (_, x) in out) {
        if (len(uniq) == 0 || uniq[len(uniq) - 1] != x) {
            uniq = append(uniq, x);
        }
    }    return uniq;

}

private static var errNoModules = errors.New("not using modules");

// importGo checks whether it can use the go command to find the directory for path.
// If using the go command is not appropriate, importGo returns errNoModules.
// Otherwise, importGo tries using the go command and reports whether that succeeded.
// Using the go command lets build.Import and build.Context.Import find code
// in Go modules. In the long term we want tools to use go/packages (currently golang.org/x/tools/go/packages),
// which will also use the go command.
// Invoking the go command here is not very efficient in that it computes information
// about the requested package and all dependencies and then only reports about the requested package.
// Then we reinvoke it for every dependency. But this is still better than not working at all.
// See golang.org/issue/26504.
private static error importGo(this ptr<Context> _addr_ctxt, ptr<Package> _addr_p, @string path, @string srcDir, ImportMode mode) {
    ref Context ctxt = ref _addr_ctxt.val;
    ref Package p = ref _addr_p.val;
 
    // To invoke the go command,
    // we must not being doing special things like AllowBinary or IgnoreVendor,
    // and all the file system callbacks must be nil (we're meant to use the local file system).
    if (mode & AllowBinary != 0 || mode & IgnoreVendor != 0 || ctxt.JoinPath != null || ctxt.SplitPathList != null || ctxt.IsAbsPath != null || ctxt.IsDir != null || ctxt.HasSubdir != null || ctxt.ReadDir != null || ctxt.OpenFile != null || !equal(ctxt.ToolTags, defaultToolTags) || !equal(ctxt.ReleaseTags, defaultReleaseTags)) {
        return error.As(errNoModules)!;
    }
    var go111Module = os.Getenv("GO111MODULE");
    switch (go111Module) {
        case "off": 
            return error.As(errNoModules)!;
            break;
        default: 

            break;
    }

    if (srcDir != "") {
        @string absSrcDir = default;
        if (filepath.IsAbs(srcDir)) {
            absSrcDir = srcDir;
        }
        else if (ctxt.Dir != "") {
            return error.As(fmt.Errorf("go/build: Dir is non-empty, so relative srcDir is not allowed: %v", srcDir))!;
        }
        else
 { 
            // Find the absolute source directory. hasSubdir does not handle
            // relative paths (and can't because the callbacks don't support this).
            error err = default!;
            absSrcDir, err = filepath.Abs(srcDir);
            if (err != null) {
                return error.As(errNoModules)!;
            }

        }
        {
            var (_, ok) = ctxt.hasSubdir(filepath.Join(ctxt.GOROOT, "src"), absSrcDir);

            if (ok) {
                return error.As(errNoModules)!;
            }

        }

    }
    if (ctxt.GOROOT != "") {
        var dir = ctxt.joinPath(ctxt.GOROOT, "src", path);
        if (ctxt.isDir(dir)) {
            return error.As(errNoModules)!;
        }
    }
    if (go111Module == "auto") {
        @string parent = default;        err = default!;
        if (ctxt.Dir == "") {
            parent, err = os.Getwd();
            if (err != null) { 
                // A nonexistent working directory can't be in a module.
                return error.As(errNoModules)!;

            }

        }
        else
 {
            parent, err = filepath.Abs(ctxt.Dir);
            if (err != null) { 
                // If the caller passed a bogus Dir explicitly, that's materially
                // different from not having modules enabled.
                return error.As(err)!;

            }

        }
        while (true) {
            {
                var f__prev2 = f;
                error err__prev2 = err;

                var (f, err) = ctxt.openFile(ctxt.joinPath(parent, "go.mod"));

                if (err == null) {
                    var buf = make_slice<byte>(100);
                    var (_, err) = f.Read(buf);
                    f.Close();
                    if (err == null || err == io.EOF) { 
                        // go.mod exists and is readable (is a file, not a directory).
                        break;

                    }

                }

                f = f__prev2;
                err = err__prev2;

            }

            var d = filepath.Dir(parent);
            if (len(d) >= len(parent)) {
                return error.As(errNoModules)!; // reached top of file system, no go.mod
            }

            parent = d;

        }

    }
    var cmd = exec.Command("go", "list", "-e", "-compiler=" + ctxt.Compiler, "-tags=" + strings.Join(ctxt.BuildTags, ","), "-installsuffix=" + ctxt.InstallSuffix, "-f={{.Dir}}\n{{.ImportPath}}\n{{.Root}}\n{{.Goroot}}\n{{if .Error}}{{.Error}}{{end}}\n", "--", path);

    if (ctxt.Dir != "") {
        cmd.Dir = ctxt.Dir;
    }
    ref strings.Builder stdout = ref heap(out ptr<strings.Builder> _addr_stdout);    ref strings.Builder stderr = ref heap(out ptr<strings.Builder> _addr_stderr);

    _addr_cmd.Stdout = _addr_stdout;
    cmd.Stdout = ref _addr_cmd.Stdout.val;
    _addr_cmd.Stderr = _addr_stderr;
    cmd.Stderr = ref _addr_cmd.Stderr.val;

    @string cgo = "0";
    if (ctxt.CgoEnabled) {
        cgo = "1";
    }
    cmd.Env = append(os.Environ(), "GOOS=" + ctxt.GOOS, "GOARCH=" + ctxt.GOARCH, "GOROOT=" + ctxt.GOROOT, "GOPATH=" + ctxt.GOPATH, "CGO_ENABLED=" + cgo);

    {
        error err__prev1 = err;

        err = cmd.Run();

        if (err != null) {
            return error.As(fmt.Errorf("go/build: go list %s: %v\n%s\n", path, err, stderr.String()))!;
        }
        err = err__prev1;

    }


    var f = strings.SplitN(stdout.String(), "\n", 5);
    if (len(f) != 5) {
        return error.As(fmt.Errorf("go/build: importGo %s: unexpected output:\n%s\n", path, stdout.String()))!;
    }
    dir = f[0];
    var errStr = strings.TrimSpace(f[4]);
    if (errStr != "" && dir == "") { 
        // If 'go list' could not locate the package (dir is empty),
        // return the same error that 'go list' reported.
        return error.As(errors.New(errStr))!;

    }
    p.Dir = dir;
    p.ImportPath = f[1];
    p.Root = f[2];
    p.Goroot = f[3] == "true";
    return error.As(null!)!;

}

private static bool equal(slice<@string> x, slice<@string> y) {
    if (len(x) != len(y)) {
        return false;
    }
    foreach (var (i, xi) in x) {
        if (xi != y[i]) {
            return false;
        }
    }    return true;

}

// hasGoFiles reports whether dir contains any files with names ending in .go.
// For a vendor check we must exclude directories that contain no .go files.
// Otherwise it is not possible to vendor just a/b/c and still import the
// non-vendored a/b. See golang.org/issue/13832.
private static bool hasGoFiles(ptr<Context> _addr_ctxt, @string dir) {
    ref Context ctxt = ref _addr_ctxt.val;

    var (ents, _) = ctxt.readDir(dir);
    foreach (var (_, ent) in ents) {
        if (!ent.IsDir() && strings.HasSuffix(ent.Name(), ".go")) {
            return true;
        }
    }    return false;

}

private static (@string, nint) findImportComment(slice<byte> data) {
    @string s = default;
    nint line = default;
 
    // expect keyword package
    var (word, data) = parseWord(data);
    if (string(word) != "package") {
        return ("", 0);
    }
    _, data = parseWord(data); 

    // now ready for import comment, a // or /* */ comment
    // beginning and ending on the current line.
    while (len(data) > 0 && (data[0] == ' ' || data[0] == '\t' || data[0] == '\r')) {
        data = data[(int)1..];
    }

    slice<byte> comment = default;

    if (bytes.HasPrefix(data, slashSlash)) 
        var i = bytes.Index(data, newline);
        if (i < 0) {
            i = len(data);
        }
        comment = data[(int)2..(int)i];
    else if (bytes.HasPrefix(data, slashStar)) 
        data = data[(int)2..];
        i = bytes.Index(data, starSlash);
        if (i < 0) { 
            // malformed comment
            return ("", 0);

        }
        comment = data[..(int)i];
        if (bytes.Contains(comment, newline)) {
            return ("", 0);
        }
        comment = bytes.TrimSpace(comment); 

    // split comment into `import`, `"pkg"`
    var (word, arg) = parseWord(comment);
    if (string(word) != "import") {
        return ("", 0);
    }
    line = 1 + bytes.Count(data[..(int)cap(data) - cap(arg)], newline);
    return (strings.TrimSpace(string(arg)), line);

}

private static slice<byte> slashSlash = (slice<byte>)"//";private static slice<byte> slashStar = (slice<byte>)"/*";private static slice<byte> starSlash = (slice<byte>)"*/";private static slice<byte> newline = (slice<byte>)"\n";

// skipSpaceOrComment returns data with any leading spaces or comments removed.
private static slice<byte> skipSpaceOrComment(slice<byte> data) {
    while (len(data) > 0) {
        switch (data[0]) {
            case ' ': 

            case '\t': 

            case '\r': 

            case '\n': 
                data = data[(int)1..];
                continue;
                break;
            case '/': 
                if (bytes.HasPrefix(data, slashSlash)) {
                    var i = bytes.Index(data, newline);
                    if (i < 0) {
                        return null;
                    }
                    data = data[(int)i + 1..];
                    continue;
                }
                if (bytes.HasPrefix(data, slashStar)) {
                    data = data[(int)2..];
                    i = bytes.Index(data, starSlash);
                    if (i < 0) {
                        return null;
                    }
                    data = data[(int)i + 2..];
                    continue;
                }
                break;
        }
        break;

    }
    return data;

}

// parseWord skips any leading spaces or comments in data
// and then parses the beginning of data as an identifier or keyword,
// returning that word and what remains after the word.
private static (slice<byte>, slice<byte>) parseWord(slice<byte> data) {
    slice<byte> word = default;
    slice<byte> rest = default;

    data = skipSpaceOrComment(data); 

    // Parse past leading word characters.
    rest = data;
    while (true) {
        var (r, size) = utf8.DecodeRune(rest);
        if (unicode.IsLetter(r) || '0' <= r && r <= '9' || r == '_') {
            rest = rest[(int)size..];
            continue;
        }
        break;

    }

    word = data[..(int)len(data) - len(rest)];
    if (len(word) == 0) {
        return (null, null);
    }
    return (word, rest);

}

// MatchFile reports whether the file with the given name in the given directory
// matches the context and would be included in a Package created by ImportDir
// of that directory.
//
// MatchFile considers the name of the file and may use ctxt.OpenFile to
// read some or all of the file's content.
private static (bool, error) MatchFile(this ptr<Context> _addr_ctxt, @string dir, @string name) {
    bool match = default;
    error err = default!;
    ref Context ctxt = ref _addr_ctxt.val;

    var (info, err) = ctxt.matchFile(dir, name, null, null, null);
    return (info != null, error.As(err)!);
}

private static Package dummyPkg = default;

// fileInfo records information learned about a file included in a build.
private partial struct fileInfo {
    public @string name; // full name including dir
    public slice<byte> header;
    public ptr<token.FileSet> fset;
    public ptr<ast.File> parsed;
    public error parseErr;
    public slice<fileImport> imports;
    public slice<fileEmbed> embeds;
    public error embedErr;
}

private partial struct fileImport {
    public @string path;
    public token.Pos pos;
    public ptr<ast.CommentGroup> doc;
}

private partial struct fileEmbed {
    public @string pattern;
    public token.Position pos;
}

// matchFile determines whether the file with the given name in the given directory
// should be included in the package being constructed.
// If the file should be included, matchFile returns a non-nil *fileInfo (and a nil error).
// Non-nil errors are reserved for unexpected problems.
//
// If name denotes a Go program, matchFile reads until the end of the
// imports and returns that section of the file in the fileInfo's header field,
// even though it only considers text until the first non-comment
// for +build lines.
//
// If allTags is non-nil, matchFile records any encountered build tag
// by setting allTags[tag] = true.
private static (ptr<fileInfo>, error) matchFile(this ptr<Context> _addr_ctxt, @string dir, @string name, map<@string, bool> allTags, ptr<bool> _addr_binaryOnly, ptr<token.FileSet> _addr_fset) {
    ptr<fileInfo> _p0 = default!;
    error _p0 = default!;
    ref Context ctxt = ref _addr_ctxt.val;
    ref bool binaryOnly = ref _addr_binaryOnly.val;
    ref token.FileSet fset = ref _addr_fset.val;

    if (strings.HasPrefix(name, "_") || strings.HasPrefix(name, ".")) {
        return (_addr_null!, error.As(null!)!);
    }
    var i = strings.LastIndex(name, ".");
    if (i < 0) {
        i = len(name);
    }
    var ext = name[(int)i..];

    if (!ctxt.goodOSArchFile(name, allTags) && !ctxt.UseAllFiles) {
        return (_addr_null!, error.As(null!)!);
    }
    if (ext != ".go" && fileListForExt(_addr_dummyPkg, ext) == null) { 
        // skip
        return (_addr_null!, error.As(null!)!);

    }
    ptr<fileInfo> info = addr(new fileInfo(name:ctxt.joinPath(dir,name),fset:fset));
    if (ext == ".syso") { 
        // binary, no reading
        return (_addr_info!, error.As(null!)!);

    }
    var (f, err) = ctxt.openFile(info.name);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    if (strings.HasSuffix(name, ".go")) {
        err = readGoInfo(f, info);
        if (strings.HasSuffix(name, "_test.go")) {
            binaryOnly = null; // ignore //go:binary-only-package comments in _test.go files
        }
    }
    else
 {
        binaryOnly = null; // ignore //go:binary-only-package comments in non-Go sources
        info.header, err = readComments(f);

    }
    f.Close();
    if (err != null) {
        return (_addr_null!, error.As(fmt.Errorf("read %s: %v", info.name, err))!);
    }
    var (ok, sawBinaryOnly, err) = ctxt.shouldBuild(info.header, allTags);
    if (err != null) {
        return (_addr_null!, error.As(fmt.Errorf("%s: %v", name, err))!);
    }
    if (!ok && !ctxt.UseAllFiles) {
        return (_addr_null!, error.As(null!)!);
    }
    if (binaryOnly != null && sawBinaryOnly) {
        binaryOnly = true;
    }
    return (_addr_info!, error.As(null!)!);

}

private static (slice<@string>, map<@string, slice<token.Position>>) cleanDecls(map<@string, slice<token.Position>> m) {
    slice<@string> _p0 = default;
    map<@string, slice<token.Position>> _p0 = default;

    var all = make_slice<@string>(0, len(m));
    foreach (var (path) in m) {
        all = append(all, path);
    }    sort.Strings(all);
    return (all, m);
}

// Import is shorthand for Default.Import.
public static (ptr<Package>, error) Import(@string path, @string srcDir, ImportMode mode) {
    ptr<Package> _p0 = default!;
    error _p0 = default!;

    return _addr_Default.Import(path, srcDir, mode)!;
}

// ImportDir is shorthand for Default.ImportDir.
public static (ptr<Package>, error) ImportDir(@string dir, ImportMode mode) {
    ptr<Package> _p0 = default!;
    error _p0 = default!;

    return _addr_Default.ImportDir(dir, mode)!;
}

private static slice<byte> bSlashSlash = (slice<byte>)slashSlash;private static slice<byte> bStarSlash = (slice<byte>)starSlash;private static slice<byte> bSlashStar = (slice<byte>)slashStar;private static slice<byte> bPlusBuild = (slice<byte>)"+build";private static slice<byte> goBuildComment = (slice<byte>)"//go:build";private static var errGoBuildWithoutBuild = errors.New("//go:build comment without // +build comment");private static var errMultipleGoBuild = errors.New("multiple //go:build comments");

private static bool isGoBuildComment(slice<byte> line) {
    if (!bytes.HasPrefix(line, goBuildComment)) {
        return false;
    }
    line = bytes.TrimSpace(line);
    var rest = line[(int)len(goBuildComment)..];
    return len(rest) == 0 || len(bytes.TrimSpace(rest)) < len(rest);

}

// Special comment denoting a binary-only package.
// See https://golang.org/design/2775-binary-only-packages
// for more about the design of binary-only packages.
private static slice<byte> binaryOnlyComment = (slice<byte>)"//go:binary-only-package";

// shouldBuild reports whether it is okay to use this file,
// The rule is that in the file's leading run of // comments
// and blank lines, which must be followed by a blank line
// (to avoid including a Go package clause doc comment),
// lines beginning with '// +build' are taken as build directives.
//
// The file is accepted only if each such line lists something
// matching the file. For example:
//
//    // +build windows linux
//
// marks the file as applicable only on Windows and Linux.
//
// For each build tag it consults, shouldBuild sets allTags[tag] = true.
//
// shouldBuild reports whether the file should be built
// and whether a //go:binary-only-package comment was found.
private static (bool, bool, error) shouldBuild(this ptr<Context> _addr_ctxt, slice<byte> content, map<@string, bool> allTags) {
    bool shouldBuild = default;
    bool binaryOnly = default;
    error err = default!;
    ref Context ctxt = ref _addr_ctxt.val;
 
    // Identify leading run of // comments and blank lines,
    // which must be followed by a blank line.
    // Also identify any //go:build comments.
    var (content, goBuild, sawBinaryOnly, err) = parseFileHeader(content);
    if (err != null) {
        return (false, false, error.As(err)!);
    }

    if (goBuild != null) 
        var (x, err) = constraint.Parse(string(goBuild));
        if (err != null) {
            return (false, false, error.As(fmt.Errorf("parsing //go:build line: %v", err))!);
        }
        shouldBuild = ctxt.eval(x, allTags);
    else 
        shouldBuild = true;
        var p = content;
        while (len(p) > 0) {
            var line = p;
            {
                var i = bytes.IndexByte(line, '\n');

                if (i >= 0) {
                    (line, p) = (line[..(int)i], p[(int)i + 1..]);
                }
                else
 {
                    p = p[(int)len(p)..];
                }

            }

            line = bytes.TrimSpace(line);
            if (!bytes.HasPrefix(line, bSlashSlash) || !bytes.Contains(line, bPlusBuild)) {
                continue;
            }

            var text = string(line);
            if (!constraint.IsPlusBuild(text)) {
                continue;
            }

            {
                var x__prev1 = x;

                (x, err) = constraint.Parse(text);

                if (err == null) {
                    if (!ctxt.eval(x, allTags)) {
                        shouldBuild = false;
                    }
                }

                x = x__prev1;

            }

        }
        return (shouldBuild, sawBinaryOnly, error.As(null!)!);

}

private static (slice<byte>, slice<byte>, bool, error) parseFileHeader(slice<byte> content) {
    slice<byte> trimmed = default;
    slice<byte> goBuild = default;
    bool sawBinaryOnly = default;
    error err = default!;

    nint end = 0;
    var p = content;
    var ended = false; // found non-blank, non-// line, so stopped accepting // +build lines
    var inSlashStar = false; // in /* */ comment

Lines:

    while (len(p) > 0) {
        var line = p;
        {
            var i__prev1 = i;

            var i = bytes.IndexByte(line, '\n');

            if (i >= 0) {
                (line, p) = (line[..(int)i], p[(int)i + 1..]);
            }
            else
 {
                p = p[(int)len(p)..];
            }

            i = i__prev1;

        }

        line = bytes.TrimSpace(line);
        if (len(line) == 0 && !ended) { // Blank line
            // Remember position of most recent blank line.
            // When we find the first non-blank, non-// line,
            // this "end" position marks the latest file position
            // where a // +build line can appear.
            // (It must appear _before_ a blank line before the non-blank, non-// line.
            // Yes, that's confusing, which is part of why we moved to //go:build lines.)
            // Note that ended==false here means that inSlashStar==false,
            // since seeing a /* would have set ended==true.
            end = len(content) - len(p);
            _continueLines = true;
            break;
        }
        if (!bytes.HasPrefix(line, slashSlash)) { // Not comment line
            ended = true;

        }
        if (!inSlashStar && isGoBuildComment(line)) {
            if (goBuild != null) {
                return (null, null, false, error.As(errMultipleGoBuild)!);
            }
            goBuild = line;
        }
        if (!inSlashStar && bytes.Equal(line, binaryOnlyComment)) {
            sawBinaryOnly = true;
        }
Comments:
        while (len(line) > 0) {
            if (inSlashStar) {
                {
                    var i__prev2 = i;

                    i = bytes.Index(line, starSlash);

                    if (i >= 0) {
                        inSlashStar = false;
                        line = bytes.TrimSpace(line[(int)i + len(starSlash)..]);
                        _continueComments = true;
                        break;
                    }

                    i = i__prev2;

                }

                _continueLines = true;
                break;
            }

            if (bytes.HasPrefix(line, bSlashSlash)) {
                _continueLines = true;
                break;
            }

            if (bytes.HasPrefix(line, bSlashStar)) {
                inSlashStar = true;
                line = bytes.TrimSpace(line[(int)len(bSlashStar)..]);
                _continueComments = true;
                break;
            } 
            // Found non-comment text.
            _breakLines = true;
            break;
        }

    }
    return (content[..(int)end], goBuild, sawBinaryOnly, error.As(null!)!);

}

// saveCgo saves the information from the #cgo lines in the import "C" comment.
// These lines set CFLAGS, CPPFLAGS, CXXFLAGS and LDFLAGS and pkg-config directives
// that affect the way cgo's C code is built.
private static error saveCgo(this ptr<Context> _addr_ctxt, @string filename, ptr<Package> _addr_di, ptr<ast.CommentGroup> _addr_cg) {
    ref Context ctxt = ref _addr_ctxt.val;
    ref Package di = ref _addr_di.val;
    ref ast.CommentGroup cg = ref _addr_cg.val;

    var text = cg.Text();
    {
        var line__prev1 = line;

        foreach (var (_, __line) in strings.Split(text, "\n")) {
            line = __line;
            var orig = line; 

            // Line is
            //    #cgo [GOOS/GOARCH...] LDFLAGS: stuff
            //
            line = strings.TrimSpace(line);
            if (len(line) < 5 || line[..(int)4] != "#cgo" || (line[4] != ' ' && line[4] != '\t')) {
                continue;
            } 

            // Split at colon.
            line = strings.TrimSpace(line[(int)4..]);
            var i = strings.Index(line, ":");
            if (i < 0) {
                return error.As(fmt.Errorf("%s: invalid #cgo line: %s", filename, orig))!;
            }

            var line = line[..(int)i];
            var argstr = line[(int)i + 1..]; 

            // Parse GOOS/GOARCH stuff.
            var f = strings.Fields(line);
            if (len(f) < 1) {
                return error.As(fmt.Errorf("%s: invalid #cgo line: %s", filename, orig))!;
            }

            var cond = f[..(int)len(f) - 1];
            var verb = f[len(f) - 1];
            if (len(cond) > 0) {
                var ok = false;
                foreach (var (_, c) in cond) {
                    if (ctxt.matchAuto(c, null)) {
                        ok = true;
                        break;
                    }
                }
                if (!ok) {
                    continue;
                }
            }

            var (args, err) = splitQuoted(argstr);
            if (err != null) {
                return error.As(fmt.Errorf("%s: invalid #cgo line: %s", filename, orig))!;
            }

            ok = default;
            {
                var i__prev2 = i;

                foreach (var (__i, __arg) in args) {
                    i = __i;
                    arg = __arg;
                    arg, ok = expandSrcDir(arg, di.Dir);

                    if (!ok) {
                        return error.As(fmt.Errorf("%s: malformed #cgo argument: %s", filename, arg))!;
                    }

                    args[i] = arg;

                }

                i = i__prev2;
            }

            switch (verb) {
                case "CFLAGS": 
                    // Change relative paths to absolute.

                case "CPPFLAGS": 
                    // Change relative paths to absolute.

                case "CXXFLAGS": 
                    // Change relative paths to absolute.

                case "FFLAGS": 
                    // Change relative paths to absolute.

                case "LDFLAGS": 
                    // Change relative paths to absolute.
                    ctxt.makePathsAbsolute(args, di.Dir);
                    break;
            }

            switch (verb) {
                case "CFLAGS": 
                    di.CgoCFLAGS = append(di.CgoCFLAGS, args);
                    break;
                case "CPPFLAGS": 
                    di.CgoCPPFLAGS = append(di.CgoCPPFLAGS, args);
                    break;
                case "CXXFLAGS": 
                    di.CgoCXXFLAGS = append(di.CgoCXXFLAGS, args);
                    break;
                case "FFLAGS": 
                    di.CgoFFLAGS = append(di.CgoFFLAGS, args);
                    break;
                case "LDFLAGS": 
                    di.CgoLDFLAGS = append(di.CgoLDFLAGS, args);
                    break;
                case "pkg-config": 
                    di.CgoPkgConfig = append(di.CgoPkgConfig, args);
                    break;
                default: 
                    return error.As(fmt.Errorf("%s: invalid #cgo verb: %s", filename, orig))!;
                    break;
            }

        }
        line = line__prev1;
    }

    return error.As(null!)!;

}

// expandSrcDir expands any occurrence of ${SRCDIR}, making sure
// the result is safe for the shell.
private static (@string, bool) expandSrcDir(@string str, @string srcdir) {
    @string _p0 = default;
    bool _p0 = default;
 
    // "\" delimited paths cause safeCgoName to fail
    // so convert native paths with a different delimiter
    // to "/" before starting (eg: on windows).
    srcdir = filepath.ToSlash(srcdir);

    var chunks = strings.Split(str, "${SRCDIR}");
    if (len(chunks) < 2) {
        return (str, safeCgoName(str));
    }
    var ok = true;
    foreach (var (_, chunk) in chunks) {
        ok = ok && (chunk == "" || safeCgoName(chunk));
    }    ok = ok && (srcdir == "" || safeCgoName(srcdir));
    var res = strings.Join(chunks, srcdir);
    return (res, ok && res != "");

}

// makePathsAbsolute looks for compiler options that take paths and
// makes them absolute. We do this because through the 1.8 release we
// ran the compiler in the package directory, so any relative -I or -L
// options would be relative to that directory. In 1.9 we changed to
// running the compiler in the build directory, to get consistent
// build results (issue #19964). To keep builds working, we change any
// relative -I or -L options to be absolute.
//
// Using filepath.IsAbs and filepath.Join here means the results will be
// different on different systems, but that's OK: -I and -L options are
// inherently system-dependent.
private static void makePathsAbsolute(this ptr<Context> _addr_ctxt, slice<@string> args, @string srcDir) {
    ref Context ctxt = ref _addr_ctxt.val;

    var nextPath = false;
    foreach (var (i, arg) in args) {
        if (nextPath) {
            if (!filepath.IsAbs(arg)) {
                args[i] = filepath.Join(srcDir, arg);
            }
            nextPath = false;
        }
        else if (strings.HasPrefix(arg, "-I") || strings.HasPrefix(arg, "-L")) {
            if (len(arg) == 2) {
                nextPath = true;
            }
            else
 {
                if (!filepath.IsAbs(arg[(int)2..])) {
                    args[i] = arg[..(int)2] + filepath.Join(srcDir, arg[(int)2..]);
                }
            }

        }
    }
}

// NOTE: $ is not safe for the shell, but it is allowed here because of linker options like -Wl,$ORIGIN.
// We never pass these arguments to a shell (just to programs we construct argv for), so this should be okay.
// See golang.org/issue/6038.
// The @ is for OS X. See golang.org/issue/13720.
// The % is for Jenkins. See golang.org/issue/16959.
// The ! is because module paths may use them. See golang.org/issue/26716.
// The ~ and ^ are for sr.ht. See golang.org/issue/32260.
private static readonly @string safeString = "+-.,/0123456789=ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz:$@%! ~^";



private static bool safeCgoName(@string s) {
    if (s == "") {
        return false;
    }
    for (nint i = 0; i < len(s); i++) {
        {
            var c = s[i];

            if (c < utf8.RuneSelf && strings.IndexByte(safeString, c) < 0) {
                return false;
            }

        }

    }
    return true;

}

// splitQuoted splits the string s around each instance of one or more consecutive
// white space characters while taking into account quotes and escaping, and
// returns an array of substrings of s or an empty list if s contains only white space.
// Single quotes and double quotes are recognized to prevent splitting within the
// quoted region, and are removed from the resulting substrings. If a quote in s
// isn't closed err will be set and r will have the unclosed argument as the
// last element. The backslash is used for escaping.
//
// For example, the following string:
//
//     a b:"c d" 'e''f'  "g\""
//
// Would be parsed as:
//
//     []string{"a", "b:c d", "ef", `g"`}
//
private static (slice<@string>, error) splitQuoted(@string s) {
    slice<@string> r = default;
    error err = default!;

    slice<@string> args = default;
    var arg = make_slice<int>(len(s));
    var escaped = false;
    var quoted = false;
    char quote = '\x00';
    nint i = 0;
    foreach (var (_, rune) in s) {

        if (escaped) 
            escaped = false;
        else if (rune == '\\') 
            escaped = true;
            continue;
        else if (quote != '\x00') 
            if (rune == quote) {
                quote = '\x00';
                continue;
            }
        else if (rune == '"' || rune == '\'') 
            quoted = true;
            quote = rune;
            continue;
        else if (unicode.IsSpace(rune)) 
            if (quoted || i > 0) {
                quoted = false;
                args = append(args, string(arg[..(int)i]));
                i = 0;
            }
            continue;
                arg[i] = rune;
        i++;

    }    if (quoted || i > 0) {
        args = append(args, string(arg[..(int)i]));
    }
    if (quote != 0) {
        err = errors.New("unclosed quote");
    }
    else if (escaped) {
        err = errors.New("unfinished escaping");
    }
    return (args, error.As(err)!);

}

// matchAuto interprets text as either a +build or //go:build expression (whichever works),
// reporting whether the expression matches the build context.
//
// matchAuto is only used for testing of tag evaluation
// and in #cgo lines, which accept either syntax.
private static bool matchAuto(this ptr<Context> _addr_ctxt, @string text, map<@string, bool> allTags) {
    ref Context ctxt = ref _addr_ctxt.val;

    if (strings.ContainsAny(text, "&|()")) {
        text = "//go:build " + text;
    }
    else
 {
        text = "// +build " + text;
    }
    var (x, err) = constraint.Parse(text);
    if (err != null) {
        return false;
    }
    return ctxt.eval(x, allTags);

}

private static bool eval(this ptr<Context> _addr_ctxt, constraint.Expr x, map<@string, bool> allTags) {
    ref Context ctxt = ref _addr_ctxt.val;

    return x.Eval(tag => ctxt.matchTag(tag, allTags));
}

// matchTag reports whether the name is one of:
//
//    cgo (if cgo is enabled)
//    $GOOS
//    $GOARCH
//    ctxt.Compiler
//    linux (if GOOS = android)
//    solaris (if GOOS = illumos)
//    tag (if tag is listed in ctxt.BuildTags or ctxt.ReleaseTags)
//
// It records all consulted tags in allTags.
private static bool matchTag(this ptr<Context> _addr_ctxt, @string name, map<@string, bool> allTags) {
    ref Context ctxt = ref _addr_ctxt.val;

    if (allTags != null) {
        allTags[name] = true;
    }
    if (ctxt.CgoEnabled && name == "cgo") {
        return true;
    }
    if (name == ctxt.GOOS || name == ctxt.GOARCH || name == ctxt.Compiler) {
        return true;
    }
    if (ctxt.GOOS == "android" && name == "linux") {
        return true;
    }
    if (ctxt.GOOS == "illumos" && name == "solaris") {
        return true;
    }
    if (ctxt.GOOS == "ios" && name == "darwin") {
        return true;
    }
    {
        var tag__prev1 = tag;

        foreach (var (_, __tag) in ctxt.BuildTags) {
            tag = __tag;
            if (tag == name) {
                return true;
            }
        }
        tag = tag__prev1;
    }

    {
        var tag__prev1 = tag;

        foreach (var (_, __tag) in ctxt.ToolTags) {
            tag = __tag;
            if (tag == name) {
                return true;
            }
        }
        tag = tag__prev1;
    }

    {
        var tag__prev1 = tag;

        foreach (var (_, __tag) in ctxt.ReleaseTags) {
            tag = __tag;
            if (tag == name) {
                return true;
            }
        }
        tag = tag__prev1;
    }

    return false;

}

// goodOSArchFile returns false if the name contains a $GOOS or $GOARCH
// suffix which does not match the current system.
// The recognized name formats are:
//
//     name_$(GOOS).*
//     name_$(GOARCH).*
//     name_$(GOOS)_$(GOARCH).*
//     name_$(GOOS)_test.*
//     name_$(GOARCH)_test.*
//     name_$(GOOS)_$(GOARCH)_test.*
//
// Exceptions:
// if GOOS=android, then files with GOOS=linux are also matched.
// if GOOS=illumos, then files with GOOS=solaris are also matched.
// if GOOS=ios, then files with GOOS=darwin are also matched.
private static bool goodOSArchFile(this ptr<Context> _addr_ctxt, @string name, map<@string, bool> allTags) {
    ref Context ctxt = ref _addr_ctxt.val;

    {
        var dot = strings.Index(name, ".");

        if (dot != -1) {
            name = name[..(int)dot];
        }
    } 

    // Before Go 1.4, a file called "linux.go" would be equivalent to having a
    // build tag "linux" in that file. For Go 1.4 and beyond, we require this
    // auto-tagging to apply only to files with a non-empty prefix, so
    // "foo_linux.go" is tagged but "linux.go" is not. This allows new operating
    // systems, such as android, to arrive without breaking existing code with
    // innocuous source code in "android.go". The easiest fix: cut everything
    // in the name before the initial _.
    var i = strings.Index(name, "_");
    if (i < 0) {
        return true;
    }
    name = name[(int)i..]; // ignore everything before first _

    var l = strings.Split(name, "_");
    {
        var n__prev1 = n;

        var n = len(l);

        if (n > 0 && l[n - 1] == "test") {
            l = l[..(int)n - 1];
        }
        n = n__prev1;

    }

    n = len(l);
    if (n >= 2 && knownOS[l[n - 2]] && knownArch[l[n - 1]]) {
        return ctxt.matchTag(l[n - 1], allTags) && ctxt.matchTag(l[n - 2], allTags);
    }
    if (n >= 1 && (knownOS[l[n - 1]] || knownArch[l[n - 1]])) {
        return ctxt.matchTag(l[n - 1], allTags);
    }
    return true;

}

private static var knownOS = make_map<@string, bool>();
private static var knownArch = make_map<@string, bool>();

private static void init() {
    {
        var v__prev1 = v;

        foreach (var (_, __v) in strings.Fields(goosList)) {
            v = __v;
            knownOS[v] = true;
        }
        v = v__prev1;
    }

    {
        var v__prev1 = v;

        foreach (var (_, __v) in strings.Fields(goarchList)) {
            v = __v;
            knownArch[v] = true;
        }
        v = v__prev1;
    }
}

// ToolDir is the directory containing build tools.
public static var ToolDir = getToolDir();

// IsLocalImport reports whether the import path is
// a local import path, like ".", "..", "./foo", or "../foo".
public static bool IsLocalImport(@string path) {
    return path == "." || path == ".." || strings.HasPrefix(path, "./") || strings.HasPrefix(path, "../");
}

// ArchChar returns "?" and an error.
// In earlier versions of Go, the returned string was used to derive
// the compiler and linker tool names, the default object file suffix,
// and the default linker output name. As of Go 1.5, those strings
// no longer vary by architecture; they are compile, link, .o, and a.out, respectively.
public static (@string, error) ArchChar(@string goarch) {
    @string _p0 = default;
    error _p0 = default!;

    return ("?", error.As(errors.New("architecture letter no longer used"))!);
}

} // end build_package
