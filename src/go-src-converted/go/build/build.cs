// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package build -- go2cs converted at 2020 August 29 08:46:49 UTC
// import "go/build" ==> using build = go.go.build_package
// Original source: C:\Go\src\go\build\build.go
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using doc = go.go.doc_package;
using parser = go.go.parser_package;
using token = go.go.token_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using pathpkg = go.path_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class build_package
    {
        // A Context specifies the supporting context for a build.
        public partial struct Context
        {
            public @string GOARCH; // target architecture
            public @string GOOS; // target operating system
            public @string GOROOT; // Go root
            public @string GOPATH; // Go path
            public bool CgoEnabled; // whether cgo can be used
            public bool UseAllFiles; // use files regardless of +build lines, file names
            public @string Compiler; // compiler to assume when computing target paths

// The build and release tags specify build constraints
// that should be considered satisfied when processing +build lines.
// Clients creating a new context may customize BuildTags, which
// defaults to empty, but it is usually an error to customize ReleaseTags,
// which defaults to the list of Go releases the current release is compatible with.
// In addition to the BuildTags and ReleaseTags, build constraints
// consider the values of GOARCH and GOOS as satisfied tags.
            public slice<@string> BuildTags;
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
            public Func<@string, @string, (@string, bool)> HasSubdir; // ReadDir returns a slice of os.FileInfo, sorted by Name,
// describing the content of the named directory.
// If ReadDir is nil, Import uses ioutil.ReadDir.
            public Func<@string, (slice<os.FileInfo>, error)> ReadDir; // OpenFile opens a file (not a directory) for reading.
// If OpenFile is nil, Import uses os.Open.
            public Func<@string, (io.ReadCloser, error)> OpenFile;
        }

        // joinPath calls ctxt.JoinPath (if not nil) or else filepath.Join.
        private static @string joinPath(this ref Context ctxt, params @string[] elem)
        {
            {
                var f = ctxt.JoinPath;

                if (f != null)
                {
                    return f(elem);
                }

            }
            return filepath.Join(elem);
        }

        // splitPathList calls ctxt.SplitPathList (if not nil) or else filepath.SplitList.
        private static slice<@string> splitPathList(this ref Context ctxt, @string s)
        {
            {
                var f = ctxt.SplitPathList;

                if (f != null)
                {
                    return f(s);
                }

            }
            return filepath.SplitList(s);
        }

        // isAbsPath calls ctxt.IsAbsPath (if not nil) or else filepath.IsAbs.
        private static bool isAbsPath(this ref Context ctxt, @string path)
        {
            {
                var f = ctxt.IsAbsPath;

                if (f != null)
                {
                    return f(path);
                }

            }
            return filepath.IsAbs(path);
        }

        // isDir calls ctxt.IsDir (if not nil) or else uses os.Stat.
        private static bool isDir(this ref Context ctxt, @string path)
        {
            {
                var f = ctxt.IsDir;

                if (f != null)
                {
                    return f(path);
                }

            }
            var (fi, err) = os.Stat(path);
            return err == null && fi.IsDir();
        }

        // hasSubdir calls ctxt.HasSubdir (if not nil) or else uses
        // the local file system to answer the question.
        private static (@string, bool) hasSubdir(this ref Context ctxt, @string root, @string dir)
        {
            {
                var f = ctxt.HasSubdir;

                if (f != null)
                {
                    return f(root, dir);
                } 

                // Try using paths we received.

            } 

            // Try using paths we received.
            rel, ok = hasSubdir(root, dir);

            if (ok)
            {
                return;
            } 

            // Try expanding symlinks and comparing
            // expanded against unexpanded and
            // expanded against expanded.
            var (rootSym, _) = filepath.EvalSymlinks(root);
            var (dirSym, _) = filepath.EvalSymlinks(dir);

            rel, ok = hasSubdir(rootSym, dir);

            if (ok)
            {
                return;
            }
            rel, ok = hasSubdir(root, dirSym);

            if (ok)
            {
                return;
            }
            return hasSubdir(rootSym, dirSym);
        }

        // hasSubdir reports if dir is within root by performing lexical analysis only.
        private static (@string, bool) hasSubdir(@string root, @string dir)
        {
            const var sep = string(filepath.Separator);

            root = filepath.Clean(root);
            if (!strings.HasSuffix(root, sep))
            {
                root += sep;
            }
            dir = filepath.Clean(dir);
            if (!strings.HasPrefix(dir, root))
            {
                return ("", false);
            }
            return (filepath.ToSlash(dir[len(root)..]), true);
        }

        // readDir calls ctxt.ReadDir (if not nil) or else ioutil.ReadDir.
        private static (slice<os.FileInfo>, error) readDir(this ref Context ctxt, @string path)
        {
            {
                var f = ctxt.ReadDir;

                if (f != null)
                {
                    return f(path);
                }

            }
            return ioutil.ReadDir(path);
        }

        // openFile calls ctxt.OpenFile (if not nil) or else os.Open.
        private static (io.ReadCloser, error) openFile(this ref Context ctxt, @string path)
        {
            {
                var fn = ctxt.OpenFile;

                if (fn != null)
                {
                    return fn(path);
                }

            }

            var (f, err) = os.Open(path);
            if (err != null)
            {
                return (null, err); // nil interface
            }
            return (f, null);
        }

        // isFile determines whether path is a file by trying to open it.
        // It reuses openFile instead of adding another function to the
        // list in Context.
        private static bool isFile(this ref Context ctxt, @string path)
        {
            var (f, err) = ctxt.openFile(path);
            if (err != null)
            {
                return false;
            }
            f.Close();
            return true;
        }

        // gopath returns the list of Go path directories.
        private static slice<@string> gopath(this ref Context ctxt)
        {
            slice<@string> all = default;
            foreach (var (_, p) in ctxt.splitPathList(ctxt.GOPATH))
            {
                if (p == "" || p == ctxt.GOROOT)
                { 
                    // Empty paths are uninteresting.
                    // If the path is the GOROOT, ignore it.
                    // People sometimes set GOPATH=$GOROOT.
                    // Do not get confused by this common mistake.
                    continue;
                }
                if (strings.HasPrefix(p, "~"))
                { 
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
            }
            return all;
        }

        // SrcDirs returns a list of package source root directories.
        // It draws from the current Go root and Go path but omits directories
        // that do not exist.
        private static slice<@string> SrcDirs(this ref Context ctxt)
        {
            slice<@string> all = default;
            if (ctxt.GOROOT != "")
            {
                var dir = ctxt.joinPath(ctxt.GOROOT, "src");
                if (ctxt.isDir(dir))
                {
                    all = append(all, dir);
                }
            }
            foreach (var (_, p) in ctxt.gopath())
            {
                dir = ctxt.joinPath(p, "src");
                if (ctxt.isDir(dir))
                {
                    all = append(all, dir);
                }
            }
            return all;
        }

        // Default is the default Context for builds.
        // It uses the GOARCH, GOOS, GOROOT, and GOPATH environment variables
        // if set, or else the compiled code's GOARCH, GOOS, and GOROOT.
        public static Context Default = defaultContext();

        private static @string defaultGOPATH()
        {
            @string env = "HOME";
            if (runtime.GOOS == "windows")
            {
                env = "USERPROFILE";
            }
            else if (runtime.GOOS == "plan9")
            {
                env = "home";
            }
            {
                var home = os.Getenv(env);

                if (home != "")
                {
                    var def = filepath.Join(home, "go");
                    if (filepath.Clean(def) == filepath.Clean(runtime.GOROOT()))
                    { 
                        // Don't set the default GOPATH to GOROOT,
                        // as that will trigger warnings from the go tool.
                        return "";
                    }
                    return def;
                }

            }
            return "";
        }

        private static Context defaultContext()
        {
            Context c = default;

            c.GOARCH = envOr("GOARCH", runtime.GOARCH);
            c.GOOS = envOr("GOOS", runtime.GOOS);
            c.GOROOT = pathpkg.Clean(runtime.GOROOT());
            c.GOPATH = envOr("GOPATH", defaultGOPATH());
            c.Compiler = runtime.Compiler; 

            // Each major Go release in the Go 1.x series should add a tag here.
            // Old tags should not be removed. That is, the go1.x tag is present
            // in all releases >= Go 1.x. Code that requires Go 1.x or later should
            // say "+build go1.x", and code that should only be built before Go 1.x
            // (perhaps it is the stub to use in that case) should say "+build !go1.x".
            // NOTE: If you add to this list, also update the doc comment in doc.go.
            const long version = 10L; // go1.10
 // go1.10
            for (long i = 1L; i <= version; i++)
            {
                c.ReleaseTags = append(c.ReleaseTags, "go1." + strconv.Itoa(i));
            }


            var env = os.Getenv("CGO_ENABLED");
            if (env == "")
            {
                env = defaultCGO_ENABLED;
            }
            switch (env)
            {
                case "1": 
                    c.CgoEnabled = true;
                    break;
                case "0": 
                    c.CgoEnabled = false;
                    break;
                default: 
                    // cgo must be explicitly enabled for cross compilation builds
                    if (runtime.GOARCH == c.GOARCH && runtime.GOOS == c.GOOS)
                    {
                        c.CgoEnabled = cgoEnabled[c.GOOS + "/" + c.GOARCH];
                        break;
                    }
                    c.CgoEnabled = false;
                    break;
            }

            return c;
        }

        private static @string envOr(@string name, @string def)
        {
            var s = os.Getenv(name);
            if (s == "")
            {
                return def;
            }
            return s;
        }

        // An ImportMode controls the behavior of the Import method.
        public partial struct ImportMode // : ulong
        {
        }

 
        // If FindOnly is set, Import stops after locating the directory
        // that should contain the sources for a package. It does not
        // read any files in the directory.
        public static readonly ImportMode FindOnly = 1L << (int)(iota); 

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
        public partial struct Package
        {
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
            public slice<@string> IgnoredGoFiles; // .go source files ignored for this build
            public slice<@string> InvalidGoFiles; // .go source files with detected problems (parse error, wrong package name, and so on)
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

// Dependency information
            public slice<@string> Imports; // import paths from GoFiles, CgoFiles
            public map<@string, slice<token.Position>> ImportPos; // line information for Imports

// Test information
            public slice<@string> TestGoFiles; // _test.go files in package
            public slice<@string> TestImports; // import paths from TestGoFiles
            public map<@string, slice<token.Position>> TestImportPos; // line information for TestImports
            public slice<@string> XTestGoFiles; // _test.go files outside package
            public slice<@string> XTestImports; // import paths from XTestGoFiles
            public map<@string, slice<token.Position>> XTestImportPos; // line information for XTestImports
        }

        // IsCommand reports whether the package is considered a
        // command to be installed (not just a library).
        // Packages named "main" are treated as commands.
        private static bool IsCommand(this ref Package p)
        {
            return p.Name == "main";
        }

        // ImportDir is like Import but processes the Go package found in
        // the named directory.
        private static (ref Package, error) ImportDir(this ref Context ctxt, @string dir, ImportMode mode)
        {
            return ctxt.Import(".", dir, mode);
        }

        // NoGoError is the error used by Import to describe a directory
        // containing no buildable Go source files. (It may still contain
        // test files, files hidden by build tags, and so on.)
        public partial struct NoGoError
        {
            public @string Dir;
        }

        private static @string Error(this ref NoGoError e)
        {
            return "no buildable Go source files in " + e.Dir;
        }

        // MultiplePackageError describes a directory containing
        // multiple buildable Go source files for multiple packages.
        public partial struct MultiplePackageError
        {
            public @string Dir; // directory containing files
            public slice<@string> Packages; // package names found
            public slice<@string> Files; // corresponding files: Files[i] declares package Packages[i]
        }

        private static @string Error(this ref MultiplePackageError e)
        { 
            // Error string limited to two entries for compatibility.
            return fmt.Sprintf("found packages %s (%s) and %s (%s) in %s", e.Packages[0L], e.Files[0L], e.Packages[1L], e.Files[1L], e.Dir);
        }

        private static @string nameExt(@string name)
        {
            var i = strings.LastIndex(name, ".");
            if (i < 0L)
            {
                return "";
            }
            return name[i..];
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
        private static (ref Package, error) Import(this ref Context ctxt, @string path, @string srcDir, ImportMode mode)
        {
            Package p = ref new Package(ImportPath:path,);
            if (path == "")
            {
                return (p, fmt.Errorf("import %q: invalid import path", path));
            }
            @string pkgtargetroot = default;
            @string pkga = default;
            error pkgerr = default;
            @string suffix = "";
            if (ctxt.InstallSuffix != "")
            {
                suffix = "_" + ctxt.InstallSuffix;
            }
            switch (ctxt.Compiler)
            {
                case "gccgo": 
                    pkgtargetroot = "pkg/gccgo_" + ctxt.GOOS + "_" + ctxt.GOARCH + suffix;
                    break;
                case "gc": 
                    pkgtargetroot = "pkg/" + ctxt.GOOS + "_" + ctxt.GOARCH + suffix;
                    break;
                default: 
                    // Save error for end of function.
                    pkgerr = error.As(fmt.Errorf("import %q: unknown compiler %q", path, ctxt.Compiler));
                    break;
            }
            Action setPkga = () =>
            {
                switch (ctxt.Compiler)
                {
                    case "gccgo": 
                        var (dir, elem) = pathpkg.Split(p.ImportPath);
                        pkga = pkgtargetroot + "/" + dir + "lib" + elem + ".a";
                        break;
                    case "gc": 
                        pkga = pkgtargetroot + "/" + p.ImportPath + ".a";
                        break;
                }
            }
;
            setPkga();

            var binaryOnly = false;
            if (IsLocalImport(path))
            {
                pkga = ""; // local imports have no installed path
                if (srcDir == "")
                {
                    return (p, fmt.Errorf("import %q: import relative to unknown directory", path));
                }
                if (!ctxt.isAbsPath(path))
                {
                    p.Dir = ctxt.joinPath(srcDir, path);
                } 
                // p.Dir directory may or may not exist. Gather partial information first, check if it exists later.
                // Determine canonical import path, if any.
                // Exclude results where the import path would include /testdata/.
                Func<@string, bool> inTestdata = sub =>
                {
                    return strings.Contains(sub, "/testdata/") || strings.HasSuffix(sub, "/testdata") || strings.HasPrefix(sub, "testdata/") || sub == "testdata";
                }
            else
;
                if (ctxt.GOROOT != "")
                {
                    var root = ctxt.joinPath(ctxt.GOROOT, "src");
                    {
                        var sub__prev3 = sub;

                        var (sub, ok) = ctxt.hasSubdir(root, p.Dir);

                        if (ok && !inTestdata(sub))
                        {
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

                    foreach (var (__i, __root) in all)
                    {
                        i = __i;
                        root = __root;
                        var rootsrc = ctxt.joinPath(root, "src");
                        {
                            var sub__prev2 = sub;

                            (sub, ok) = ctxt.hasSubdir(rootsrc, p.Dir);

                            if (ok && !inTestdata(sub))
                            { 
                                // We found a potential import path for dir,
                                // but check that using it wouldn't find something
                                // else first.
                                if (ctxt.GOROOT != "")
                                {
                                    {
                                        var dir__prev4 = dir;

                                        var dir = ctxt.joinPath(ctxt.GOROOT, "src", sub);

                                        if (ctxt.isDir(dir))
                                        {
                                            p.ConflictDir = dir;
                                            goto Found;
                                        }

                                        dir = dir__prev4;

                                    }
                                }
                                foreach (var (_, earlyRoot) in all[..i])
                                {
                                    {
                                        var dir__prev3 = dir;

                                        dir = ctxt.joinPath(earlyRoot, "src", sub);

                                        if (ctxt.isDir(dir))
                                        {
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

            }            {
                if (strings.HasPrefix(path, "/"))
                {
                    return (p, fmt.Errorf("import %q: cannot import absolute path", path));
                } 

                // tried records the location of unsuccessful package lookups
                var tried = default;
                var gopath = ctxt.gopath(); 

                // Vendor directories get first chance to satisfy import.
                if (mode & IgnoreVendor == 0L && srcDir != "")
                {
                    Func<@string, bool, bool> searchVendor = (root, isGoroot) =>
                    {
                        (sub, ok) = ctxt.hasSubdir(root, srcDir);
                        if (!ok || !strings.HasPrefix(sub, "src/") || strings.Contains(sub, "/testdata/"))
                        {
                            return false;
                        }
                        while (true)
                        {
                            var vendor = ctxt.joinPath(root, sub, "vendor");
                            if (ctxt.isDir(vendor))
                            {
                                dir = ctxt.joinPath(vendor, path);
                                if (ctxt.isDir(dir) && hasGoFiles(ctxt, dir))
                                {
                                    p.Dir = dir;
                                    p.ImportPath = strings.TrimPrefix(pathpkg.Join(sub, "vendor", path), "src/");
                                    p.Goroot = isGoroot;
                                    p.Root = root;
                                    setPkga(); // p.ImportPath changed
                                    return true;
                                }
                                tried.vendor = append(tried.vendor, dir);
                            }
                            var i = strings.LastIndex(sub, "/");
                            if (i < 0L)
                            {
                                break;
                            }
                            sub = sub[..i];
                        }

                        return false;
                    }
;
                    if (searchVendor(ctxt.GOROOT, true))
                    {
                        goto Found;
                    }
                    {
                        var root__prev1 = root;

                        foreach (var (_, __root) in gopath)
                        {
                            root = __root;
                            if (searchVendor(root, false))
                            {
                                goto Found;
                            }
                        }

                        root = root__prev1;
                    }

                } 

                // Determine directory from import path.
                if (ctxt.GOROOT != "")
                {
                    dir = ctxt.joinPath(ctxt.GOROOT, "src", path);
                    var isDir = ctxt.isDir(dir);
                    binaryOnly = !isDir && mode & AllowBinary != 0L && pkga != "" && ctxt.isFile(ctxt.joinPath(ctxt.GOROOT, pkga));
                    if (isDir || binaryOnly)
                    {
                        p.Dir = dir;
                        p.Goroot = true;
                        p.Root = ctxt.GOROOT;
                        goto Found;
                    }
                    tried.goroot = dir;
                }
                {
                    var root__prev1 = root;

                    foreach (var (_, __root) in gopath)
                    {
                        root = __root;
                        dir = ctxt.joinPath(root, "src", path);
                        isDir = ctxt.isDir(dir);
                        binaryOnly = !isDir && mode & AllowBinary != 0L && pkga != "" && ctxt.isFile(ctxt.joinPath(root, pkga));
                        if (isDir || binaryOnly)
                        {
                            p.Dir = dir;
                            p.Root = root;
                            goto Found;
                        }
                        tried.gopath = append(tried.gopath, dir);
                    } 

                    // package was not found

                    root = root__prev1;
                }

                slice<@string> paths = default;
                @string format = "\t%s (vendor tree)";
                {
                    var dir__prev1 = dir;

                    foreach (var (_, __dir) in tried.vendor)
                    {
                        dir = __dir;
                        paths = append(paths, fmt.Sprintf(format, dir));
                        format = "\t%s";
                    }

                    dir = dir__prev1;
                }

                if (tried.goroot != "")
                {
                    paths = append(paths, fmt.Sprintf("\t%s (from $GOROOT)", tried.goroot));
                }
                else
                {
                    paths = append(paths, "\t($GOROOT not set)");
                }
                format = "\t%s (from $GOPATH)";
                {
                    var dir__prev1 = dir;

                    foreach (var (_, __dir) in tried.gopath)
                    {
                        dir = __dir;
                        paths = append(paths, fmt.Sprintf(format, dir));
                        format = "\t%s";
                    }

                    dir = dir__prev1;
                }

                if (len(tried.gopath) == 0L)
                {
                    paths = append(paths, "\t($GOPATH not set. For more details see: 'go help gopath')");
                }
                return (p, fmt.Errorf("cannot find package %q in any of:\n%s", path, strings.Join(paths, "\n")));
            }
Found: 

            // If it's a local import path, by the time we get here, we still haven't checked
            // that p.Dir directory exists. This is the right time to do that check.
            // We can't do it earlier, because we want to gather partial information for the
            // non-nil *Package returned when an error occurs.
            // We need to do this before we return early on FindOnly flag.
            if (p.Root != "")
            {
                p.SrcRoot = ctxt.joinPath(p.Root, "src");
                p.PkgRoot = ctxt.joinPath(p.Root, "pkg");
                p.BinDir = ctxt.joinPath(p.Root, "bin");
                if (pkga != "")
                {
                    p.PkgTargetRoot = ctxt.joinPath(p.Root, pkgtargetroot);
                    p.PkgObj = ctxt.joinPath(p.Root, pkga);
                }
            } 

            // If it's a local import path, by the time we get here, we still haven't checked
            // that p.Dir directory exists. This is the right time to do that check.
            // We can't do it earlier, because we want to gather partial information for the
            // non-nil *Package returned when an error occurs.
            // We need to do this before we return early on FindOnly flag.
            if (IsLocalImport(path) && !ctxt.isDir(p.Dir))
            { 
                // package was not found
                return (p, fmt.Errorf("cannot find package %q in:\n\t%s", path, p.Dir));
            }
            if (mode & FindOnly != 0L)
            {
                return (p, pkgerr);
            }
            if (binaryOnly && (mode & AllowBinary) != 0L)
            {
                return (p, pkgerr);
            }
            var (dirs, err) = ctxt.readDir(p.Dir);
            if (err != null)
            {
                return (p, err);
            }
            error badGoError = default;
            slice<@string> Sfiles = default; // files with ".S" (capital S)
            @string firstFile = default;            @string firstCommentFile = default;

            var imported = make_map<@string, slice<token.Position>>();
            var testImported = make_map<@string, slice<token.Position>>();
            var xTestImported = make_map<@string, slice<token.Position>>();
            var allTags = make_map<@string, bool>();
            var fset = token.NewFileSet();
            {
                var d__prev1 = d;

                foreach (var (_, __d) in dirs)
                {
                    d = __d;
                    if (d.IsDir())
                    {
                        continue;
                    }
                    var name = d.Name();
                    var ext = nameExt(name);

                    Action<error> badFile = err =>
                    {
                        if (badGoError == null)
                        {
                            badGoError = error.As(err);
                        }
                        p.InvalidGoFiles = append(p.InvalidGoFiles, name);
                    }
;

                    var (match, data, filename, err) = ctxt.matchFile(p.Dir, name, allTags, ref p.BinaryOnly);
                    if (err != null)
                    {
                        badFile(err);
                        continue;
                    }
                    if (!match)
                    {
                        if (ext == ".go")
                        {
                            p.IgnoredGoFiles = append(p.IgnoredGoFiles, name);
                        }
                        continue;
                    } 

                    // Going to save the file. For non-Go files, can stop here.
                    switch (ext)
                    {
                        case ".c": 
                            p.CFiles = append(p.CFiles, name);
                            continue;
                            break;
                        case ".cc": 

                        case ".cpp": 

                        case ".cxx": 
                            p.CXXFiles = append(p.CXXFiles, name);
                            continue;
                            break;
                        case ".m": 
                            p.MFiles = append(p.MFiles, name);
                            continue;
                            break;
                        case ".h": 

                        case ".hh": 

                        case ".hpp": 

                        case ".hxx": 
                            p.HFiles = append(p.HFiles, name);
                            continue;
                            break;
                        case ".f": 

                        case ".F": 

                        case ".for": 

                        case ".f90": 
                            p.FFiles = append(p.FFiles, name);
                            continue;
                            break;
                        case ".s": 
                            p.SFiles = append(p.SFiles, name);
                            continue;
                            break;
                        case ".S": 
                            Sfiles = append(Sfiles, name);
                            continue;
                            break;
                        case ".swig": 
                            p.SwigFiles = append(p.SwigFiles, name);
                            continue;
                            break;
                        case ".swigcxx": 
                            p.SwigCXXFiles = append(p.SwigCXXFiles, name);
                            continue;
                            break;
                        case ".syso": 
                            // binary objects to add to package archive
                            // Likely of the form foo_windows.syso, but
                            // the name was vetted above with goodOSArchFile.
                            p.SysoFiles = append(p.SysoFiles, name);
                            continue;
                            break;
                    }

                    var (pf, err) = parser.ParseFile(fset, filename, data, parser.ImportsOnly | parser.ParseComments);
                    if (err != null)
                    {
                        badFile(err);
                        continue;
                    }
                    var pkg = pf.Name.Name;
                    if (pkg == "documentation")
                    {
                        p.IgnoredGoFiles = append(p.IgnoredGoFiles, name);
                        continue;
                    }
                    var isTest = strings.HasSuffix(name, "_test.go");
                    var isXTest = false;
                    if (isTest && strings.HasSuffix(pkg, "_test"))
                    {
                        isXTest = true;
                        pkg = pkg[..len(pkg) - len("_test")];
                    }
                    if (p.Name == "")
                    {
                        p.Name = pkg;
                        firstFile = name;
                    }
                    else if (pkg != p.Name)
                    {
                        badFile(ref new MultiplePackageError(Dir:p.Dir,Packages:[]string{p.Name,pkg},Files:[]string{firstFile,name},));
                        p.InvalidGoFiles = append(p.InvalidGoFiles, name);
                    }
                    if (pf.Doc != null && p.Doc == "")
                    {
                        p.Doc = doc.Synopsis(pf.Doc.Text());
                    }
                    if (mode & ImportComment != 0L)
                    {
                        var (qcom, line) = findImportComment(data);
                        if (line != 0L)
                        {
                            var (com, err) = strconv.Unquote(qcom);
                            if (err != null)
                            {
                                badFile(fmt.Errorf("%s:%d: cannot parse import comment", filename, line));
                            }
                            else if (p.ImportComment == "")
                            {
                                p.ImportComment = com;
                                firstCommentFile = name;
                            }
                            else if (p.ImportComment != com)
                            {
                                badFile(fmt.Errorf("found import comments %q (%s) and %q (%s) in %s", p.ImportComment, firstCommentFile, com, name, p.Dir));
                            }
                        }
                    } 

                    // Record imports and information about cgo.
                    var isCgo = false;
                    foreach (var (_, decl) in pf.Decls)
                    {
                        ref ast.GenDecl (d, ok) = decl._<ref ast.GenDecl>();
                        if (!ok)
                        {
                            continue;
                        }
                        foreach (var (_, dspec) in d.Specs)
                        {
                            ref ast.ImportSpec (spec, ok) = dspec._<ref ast.ImportSpec>();
                            if (!ok)
                            {
                                continue;
                            }
                            var quoted = spec.Path.Value;
                            var (path, err) = strconv.Unquote(quoted);
                            if (err != null)
                            {
                                log.Panicf("%s: parser returned invalid quoted string: <%s>", filename, quoted);
                            }
                            if (isXTest)
                            {
                                xTestImported[path] = append(xTestImported[path], fset.Position(spec.Pos()));
                            }
                            else if (isTest)
                            {
                                testImported[path] = append(testImported[path], fset.Position(spec.Pos()));
                            }
                            else
                            {
                                imported[path] = append(imported[path], fset.Position(spec.Pos()));
                            }
                            if (path == "C")
                            {
                                if (isTest)
                                {
                                    badFile(fmt.Errorf("use of cgo in test %s not supported", filename));
                                }
                                else
                                {
                                    var cg = spec.Doc;
                                    if (cg == null && len(d.Specs) == 1L)
                                    {
                                        cg = d.Doc;
                                    }
                                    if (cg != null)
                                    {
                                        {
                                            var err = ctxt.saveCgo(filename, p, cg);

                                            if (err != null)
                                            {
                                                badFile(err);
                                            }

                                        }
                                    }
                                    isCgo = true;
                                }
                            }
                        }
                    }
                    if (isCgo)
                    {
                        allTags["cgo"] = true;
                        if (ctxt.CgoEnabled)
                        {
                            p.CgoFiles = append(p.CgoFiles, name);
                        }
                        else
                        {
                            p.IgnoredGoFiles = append(p.IgnoredGoFiles, name);
                        }
                    }
                    else if (isXTest)
                    {
                        p.XTestGoFiles = append(p.XTestGoFiles, name);
                    }
                    else if (isTest)
                    {
                        p.TestGoFiles = append(p.TestGoFiles, name);
                    }
                    else
                    {
                        p.GoFiles = append(p.GoFiles, name);
                    }
                }

                d = d__prev1;
            }

            if (badGoError != null)
            {
                return (p, badGoError);
            }
            if (len(p.GoFiles) + len(p.CgoFiles) + len(p.TestGoFiles) + len(p.XTestGoFiles) == 0L)
            {
                return (p, ref new NoGoError(p.Dir));
            }
            foreach (var (tag) in allTags)
            {
                p.AllTags = append(p.AllTags, tag);
            }
            sort.Strings(p.AllTags);

            p.Imports, p.ImportPos = cleanImports(imported);
            p.TestImports, p.TestImportPos = cleanImports(testImported);
            p.XTestImports, p.XTestImportPos = cleanImports(xTestImported); 

            // add the .S files only if we are using cgo
            // (which means gcc will compile them).
            // The standard assemblers expect .s files.
            if (len(p.CgoFiles) > 0L)
            {
                p.SFiles = append(p.SFiles, Sfiles);
                sort.Strings(p.SFiles);
            }
            return (p, pkgerr);
        }

        // hasGoFiles reports whether dir contains any files with names ending in .go.
        // For a vendor check we must exclude directories that contain no .go files.
        // Otherwise it is not possible to vendor just a/b/c and still import the
        // non-vendored a/b. See golang.org/issue/13832.
        private static bool hasGoFiles(ref Context ctxt, @string dir)
        {
            var (ents, _) = ctxt.readDir(dir);
            foreach (var (_, ent) in ents)
            {
                if (!ent.IsDir() && strings.HasSuffix(ent.Name(), ".go"))
                {
                    return true;
                }
            }
            return false;
        }

        private static (@string, long) findImportComment(slice<byte> data)
        { 
            // expect keyword package
            var (word, data) = parseWord(data);
            if (string(word) != "package")
            {
                return ("", 0L);
            } 

            // expect package name
            _, data = parseWord(data); 

            // now ready for import comment, a // or /* */ comment
            // beginning and ending on the current line.
            while (len(data) > 0L && (data[0L] == ' ' || data[0L] == '\t' || data[0L] == '\r'))
            {
                data = data[1L..];
            }


            slice<byte> comment = default;

            if (bytes.HasPrefix(data, slashSlash)) 
                var i = bytes.Index(data, newline);
                if (i < 0L)
                {
                    i = len(data);
                }
                comment = data[2L..i];
            else if (bytes.HasPrefix(data, slashStar)) 
                data = data[2L..];
                i = bytes.Index(data, starSlash);
                if (i < 0L)
                { 
                    // malformed comment
                    return ("", 0L);
                }
                comment = data[..i];
                if (bytes.Contains(comment, newline))
                {
                    return ("", 0L);
                }
                        comment = bytes.TrimSpace(comment); 

            // split comment into `import`, `"pkg"`
            var (word, arg) = parseWord(comment);
            if (string(word) != "import")
            {
                return ("", 0L);
            }
            line = 1L + bytes.Count(data[..cap(data) - cap(arg)], newline);
            return (strings.TrimSpace(string(arg)), line);
        }

        private static slice<byte> slashSlash = (slice<byte>)"//";        private static slice<byte> slashStar = (slice<byte>)"/*";        private static slice<byte> starSlash = (slice<byte>)"*/";        private static slice<byte> newline = (slice<byte>)"\n";

        // skipSpaceOrComment returns data with any leading spaces or comments removed.
        private static slice<byte> skipSpaceOrComment(slice<byte> data)
        {
            while (len(data) > 0L)
            {
                switch (data[0L])
                {
                    case ' ': 

                    case '\t': 

                    case '\r': 

                    case '\n': 
                        data = data[1L..];
                        continue;
                        break;
                    case '/': 
                        if (bytes.HasPrefix(data, slashSlash))
                        {
                            var i = bytes.Index(data, newline);
                            if (i < 0L)
                            {
                                return null;
                            }
                            data = data[i + 1L..];
                            continue;
                        }
                        if (bytes.HasPrefix(data, slashStar))
                        {
                            data = data[2L..];
                            i = bytes.Index(data, starSlash);
                            if (i < 0L)
                            {
                                return null;
                            }
                            data = data[i + 2L..];
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
        private static (slice<byte>, slice<byte>) parseWord(slice<byte> data)
        {
            data = skipSpaceOrComment(data); 

            // Parse past leading word characters.
            rest = data;
            while (true)
            {
                var (r, size) = utf8.DecodeRune(rest);
                if (unicode.IsLetter(r) || '0' <= r && r <= '9' || r == '_')
                {
                    rest = rest[size..];
                    continue;
                }
                break;
            }


            word = data[..len(data) - len(rest)];
            if (len(word) == 0L)
            {
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
        private static (bool, error) MatchFile(this ref Context ctxt, @string dir, @string name)
        {
            match, _, _, err = ctxt.matchFile(dir, name, null, null);
            return;
        }

        // matchFile determines whether the file with the given name in the given directory
        // should be included in the package being constructed.
        // It returns the data read from the file.
        // If name denotes a Go program, matchFile reads until the end of the
        // imports (and returns that data) even though it only considers text
        // until the first non-comment.
        // If allTags is non-nil, matchFile records any encountered build tag
        // by setting allTags[tag] = true.
        private static (bool, slice<byte>, @string, error) matchFile(this ref Context ctxt, @string dir, @string name, map<@string, bool> allTags, ref bool binaryOnly)
        {
            if (strings.HasPrefix(name, "_") || strings.HasPrefix(name, "."))
            {
                return;
            }
            var i = strings.LastIndex(name, ".");
            if (i < 0L)
            {
                i = len(name);
            }
            var ext = name[i..];

            if (!ctxt.goodOSArchFile(name, allTags) && !ctxt.UseAllFiles)
            {
                return;
            }
            switch (ext)
            {
                case ".go": 

                case ".c": 

                case ".cc": 

                case ".cxx": 

                case ".cpp": 

                case ".m": 

                case ".s": 

                case ".h": 

                case ".hh": 

                case ".hpp": 

                case ".hxx": 

                case ".f": 

                case ".F": 

                case ".f90": 

                case ".S": 

                case ".swig": 

                case ".swigcxx": 
                    break;
                case ".syso": 
                    // binary, no reading
                    match = true;
                    return;
                    break;
                default: 
                    // skip
                    return;
                    break;
            }

            filename = ctxt.joinPath(dir, name);
            var (f, err) = ctxt.openFile(filename);
            if (err != null)
            {
                return;
            }
            if (strings.HasSuffix(filename, ".go"))
            {
                data, err = readImports(f, false, null);
                if (strings.HasSuffix(filename, "_test.go"))
                {
                    binaryOnly = null; // ignore //go:binary-only-package comments in _test.go files
                }
            }
            else
            {
                binaryOnly = null; // ignore //go:binary-only-package comments in non-Go sources
                data, err = readComments(f);
            }
            f.Close();
            if (err != null)
            {
                err = fmt.Errorf("read %s: %v", filename, err);
                return;
            } 

            // Look for +build comments to accept or reject the file.
            bool sawBinaryOnly = default;
            if (!ctxt.shouldBuild(data, allTags, ref sawBinaryOnly) && !ctxt.UseAllFiles)
            {
                return;
            }
            if (binaryOnly != null && sawBinaryOnly)
            {
                binaryOnly.Value = true;
            }
            match = true;
            return;
        }

        private static (slice<@string>, map<@string, slice<token.Position>>) cleanImports(map<@string, slice<token.Position>> m)
        {
            var all = make_slice<@string>(0L, len(m));
            foreach (var (path) in m)
            {
                all = append(all, path);
            }
            sort.Strings(all);
            return (all, m);
        }

        // Import is shorthand for Default.Import.
        public static (ref Package, error) Import(@string path, @string srcDir, ImportMode mode)
        {
            return Default.Import(path, srcDir, mode);
        }

        // ImportDir is shorthand for Default.ImportDir.
        public static (ref Package, error) ImportDir(@string dir, ImportMode mode)
        {
            return Default.ImportDir(dir, mode);
        }

        private static slice<byte> slashslash = (slice<byte>)"//";

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
        // If shouldBuild finds a //go:binary-only-package comment in the file,
        // it sets *binaryOnly to true. Otherwise it does not change *binaryOnly.
        //
        private static bool shouldBuild(this ref Context ctxt, slice<byte> content, map<@string, bool> allTags, ref bool binaryOnly)
        {
            var sawBinaryOnly = false; 

            // Pass 1. Identify leading run of // comments and blank lines,
            // which must be followed by a blank line.
            long end = 0L;
            var p = content;
            while (len(p) > 0L)
            {
                var line = p;
                {
                    var i__prev1 = i;

                    var i = bytes.IndexByte(line, '\n');

                    if (i >= 0L)
                    {
                        line = line[..i];
                        p = p[i + 1L..];
                    }
                    else
                    {
                        p = p[len(p)..];
                    }

                    i = i__prev1;

                }
                line = bytes.TrimSpace(line);
                if (len(line) == 0L)
                { // Blank line
                    end = len(content) - len(p);
                    continue;
                }
                if (!bytes.HasPrefix(line, slashslash))
                { // Not comment line
                    break;
                }
            }

            content = content[..end]; 

            // Pass 2.  Process each line in the run.
            p = content;
            var allok = true;
            while (len(p) > 0L)
            {
                line = p;
                {
                    var i__prev1 = i;

                    i = bytes.IndexByte(line, '\n');

                    if (i >= 0L)
                    {
                        line = line[..i];
                        p = p[i + 1L..];
                    }
                    else
                    {
                        p = p[len(p)..];
                    }

                    i = i__prev1;

                }
                line = bytes.TrimSpace(line);
                if (!bytes.HasPrefix(line, slashslash))
                {
                    continue;
                }
                if (bytes.Equal(line, binaryOnlyComment))
                {
                    sawBinaryOnly = true;
                }
                line = bytes.TrimSpace(line[len(slashslash)..]);
                if (len(line) > 0L && line[0L] == '+')
                { 
                    // Looks like a comment +line.
                    var f = strings.Fields(string(line));
                    if (f[0L] == "+build")
                    {
                        var ok = false;
                        foreach (var (_, tok) in f[1L..])
                        {
                            if (ctxt.match(tok, allTags))
                            {
                                ok = true;
                            }
                        }
                        if (!ok)
                        {
                            allok = false;
                        }
                    }
                }
            }


            if (binaryOnly != null && sawBinaryOnly)
            {
                binaryOnly.Value = true;
            }
            return allok;
        }

        // saveCgo saves the information from the #cgo lines in the import "C" comment.
        // These lines set CFLAGS, CPPFLAGS, CXXFLAGS and LDFLAGS and pkg-config directives
        // that affect the way cgo's C code is built.
        private static error saveCgo(this ref Context ctxt, @string filename, ref Package di, ref ast.CommentGroup cg)
        {
            var text = cg.Text();
            {
                var line__prev1 = line;

                foreach (var (_, __line) in strings.Split(text, "\n"))
                {
                    line = __line;
                    var orig = line; 

                    // Line is
                    //    #cgo [GOOS/GOARCH...] LDFLAGS: stuff
                    //
                    line = strings.TrimSpace(line);
                    if (len(line) < 5L || line[..4L] != "#cgo" || (line[4L] != ' ' && line[4L] != '\t'))
                    {
                        continue;
                    } 

                    // Split at colon.
                    line = strings.TrimSpace(line[4L..]);
                    var i = strings.Index(line, ":");
                    if (i < 0L)
                    {
                        return error.As(fmt.Errorf("%s: invalid #cgo line: %s", filename, orig));
                    }
                    var line = line[..i];
                    var argstr = line[i + 1L..]; 

                    // Parse GOOS/GOARCH stuff.
                    var f = strings.Fields(line);
                    if (len(f) < 1L)
                    {
                        return error.As(fmt.Errorf("%s: invalid #cgo line: %s", filename, orig));
                    }
                    var cond = f[..len(f) - 1L];
                    var verb = f[len(f) - 1L];
                    if (len(cond) > 0L)
                    {
                        var ok = false;
                        foreach (var (_, c) in cond)
                        {
                            if (ctxt.match(c, null))
                            {
                                ok = true;
                                break;
                            }
                        }
                        if (!ok)
                        {
                            continue;
                        }
                    }
                    var (args, err) = splitQuoted(argstr);
                    if (err != null)
                    {
                        return error.As(fmt.Errorf("%s: invalid #cgo line: %s", filename, orig));
                    }
                    ok = default;
                    {
                        var i__prev2 = i;

                        foreach (var (__i, __arg) in args)
                        {
                            i = __i;
                            arg = __arg;
                            arg, ok = expandSrcDir(arg, di.Dir);

                            if (!ok)
                            {
                                return error.As(fmt.Errorf("%s: malformed #cgo argument: %s", filename, arg));
                            }
                            args[i] = arg;
                        }

                        i = i__prev2;
                    }

                    switch (verb)
                    {
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

                    switch (verb)
                    {
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
                            return error.As(fmt.Errorf("%s: invalid #cgo verb: %s", filename, orig));
                            break;
                    }
                }

                line = line__prev1;
            }

            return error.As(null);
        }

        // expandSrcDir expands any occurrence of ${SRCDIR}, making sure
        // the result is safe for the shell.
        private static (@string, bool) expandSrcDir(@string str, @string srcdir)
        { 
            // "\" delimited paths cause safeCgoName to fail
            // so convert native paths with a different delimiter
            // to "/" before starting (eg: on windows).
            srcdir = filepath.ToSlash(srcdir);

            var chunks = strings.Split(str, "${SRCDIR}");
            if (len(chunks) < 2L)
            {
                return (str, safeCgoName(str));
            }
            var ok = true;
            foreach (var (_, chunk) in chunks)
            {
                ok = ok && (chunk == "" || safeCgoName(chunk));
            }
            ok = ok && (srcdir == "" || safeCgoName(srcdir));
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
        private static void makePathsAbsolute(this ref Context ctxt, slice<@string> args, @string srcDir)
        {
            var nextPath = false;
            foreach (var (i, arg) in args)
            {
                if (nextPath)
                {
                    if (!filepath.IsAbs(arg))
                    {
                        args[i] = filepath.Join(srcDir, arg);
                    }
                    nextPath = false;
                }
                else if (strings.HasPrefix(arg, "-I") || strings.HasPrefix(arg, "-L"))
                {
                    if (len(arg) == 2L)
                    {
                        nextPath = true;
                    }
                    else
                    {
                        if (!filepath.IsAbs(arg[2L..]))
                        {
                            args[i] = arg[..2L] + filepath.Join(srcDir, arg[2L..]);
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
        private static readonly @string safeString = "+-.,/0123456789=ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz:$@% ";



        private static bool safeCgoName(@string s)
        {
            if (s == "")
            {
                return false;
            }
            for (long i = 0L; i < len(s); i++)
            {
                {
                    var c = s[i];

                    if (c < utf8.RuneSelf && strings.IndexByte(safeString, c) < 0L)
                    {
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
        private static (slice<@string>, error) splitQuoted(@string s)
        {
            slice<@string> args = default;
            var arg = make_slice<int>(len(s));
            var escaped = false;
            var quoted = false;
            char quote = '\x00';
            long i = 0L;
            foreach (var (_, rune) in s)
            {

                if (escaped) 
                    escaped = false;
                else if (rune == '\\') 
                    escaped = true;
                    continue;
                else if (quote != '\x00') 
                    if (rune == quote)
                    {
                        quote = '\x00';
                        continue;
                    }
                else if (rune == '"' || rune == '\'') 
                    quoted = true;
                    quote = rune;
                    continue;
                else if (unicode.IsSpace(rune)) 
                    if (quoted || i > 0L)
                    {
                        quoted = false;
                        args = append(args, string(arg[..i]));
                        i = 0L;
                    }
                    continue;
                                arg[i] = rune;
                i++;
            }
            if (quoted || i > 0L)
            {
                args = append(args, string(arg[..i]));
            }
            if (quote != 0L)
            {
                err = errors.New("unclosed quote");
            }
            else if (escaped)
            {
                err = errors.New("unfinished escaping");
            }
            return (args, err);
        }

        // match reports whether the name is one of:
        //
        //    $GOOS
        //    $GOARCH
        //    cgo (if cgo is enabled)
        //    !cgo (if cgo is disabled)
        //    ctxt.Compiler
        //    !ctxt.Compiler
        //    tag (if tag is listed in ctxt.BuildTags or ctxt.ReleaseTags)
        //    !tag (if tag is not listed in ctxt.BuildTags or ctxt.ReleaseTags)
        //    a comma-separated list of any of these
        //
        private static bool match(this ref Context ctxt, @string name, map<@string, bool> allTags)
        {
            if (name == "")
            {
                if (allTags != null)
                {
                    allTags[name] = true;
                }
                return false;
            }
            {
                var i = strings.Index(name, ",");

                if (i >= 0L)
                { 
                    // comma-separated list
                    var ok1 = ctxt.match(name[..i], allTags);
                    var ok2 = ctxt.match(name[i + 1L..], allTags);
                    return ok1 && ok2;
                }

            }
            if (strings.HasPrefix(name, "!!"))
            { // bad syntax, reject always
                return false;
            }
            if (strings.HasPrefix(name, "!"))
            { // negation
                return len(name) > 1L && !ctxt.match(name[1L..], allTags);
            }
            if (allTags != null)
            {
                allTags[name] = true;
            } 

            // Tags must be letters, digits, underscores or dots.
            // Unlike in Go identifiers, all digits are fine (e.g., "386").
            foreach (var (_, c) in name)
            {
                if (!unicode.IsLetter(c) && !unicode.IsDigit(c) && c != '_' && c != '.')
                {
                    return false;
                }
            } 

            // special tags
            if (ctxt.CgoEnabled && name == "cgo")
            {
                return true;
            }
            if (name == ctxt.GOOS || name == ctxt.GOARCH || name == ctxt.Compiler)
            {
                return true;
            }
            if (ctxt.GOOS == "android" && name == "linux")
            {
                return true;
            } 

            // other tags
            {
                var tag__prev1 = tag;

                foreach (var (_, __tag) in ctxt.BuildTags)
                {
                    tag = __tag;
                    if (tag == name)
                    {
                        return true;
                    }
                }

                tag = tag__prev1;
            }

            {
                var tag__prev1 = tag;

                foreach (var (_, __tag) in ctxt.ReleaseTags)
                {
                    tag = __tag;
                    if (tag == name)
                    {
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
        // An exception: if GOOS=android, then files with GOOS=linux are also matched.
        private static bool goodOSArchFile(this ref Context ctxt, @string name, map<@string, bool> allTags)
        {
            {
                var dot = strings.Index(name, ".");

                if (dot != -1L)
                {
                    name = name[..dot];
                } 

                // Before Go 1.4, a file called "linux.go" would be equivalent to having a
                // build tag "linux" in that file. For Go 1.4 and beyond, we require this
                // auto-tagging to apply only to files with a non-empty prefix, so
                // "foo_linux.go" is tagged but "linux.go" is not. This allows new operating
                // systems, such as android, to arrive without breaking existing code with
                // innocuous source code in "android.go". The easiest fix: cut everything
                // in the name before the initial _.

            } 

            // Before Go 1.4, a file called "linux.go" would be equivalent to having a
            // build tag "linux" in that file. For Go 1.4 and beyond, we require this
            // auto-tagging to apply only to files with a non-empty prefix, so
            // "foo_linux.go" is tagged but "linux.go" is not. This allows new operating
            // systems, such as android, to arrive without breaking existing code with
            // innocuous source code in "android.go". The easiest fix: cut everything
            // in the name before the initial _.
            var i = strings.Index(name, "_");
            if (i < 0L)
            {
                return true;
            }
            name = name[i..]; // ignore everything before first _

            var l = strings.Split(name, "_");
            {
                var n__prev1 = n;

                var n = len(l);

                if (n > 0L && l[n - 1L] == "test")
                {
                    l = l[..n - 1L];
                }

                n = n__prev1;

            }
            n = len(l);
            if (n >= 2L && knownOS[l[n - 2L]] && knownArch[l[n - 1L]])
            {
                if (allTags != null)
                {
                    allTags[l[n - 2L]] = true;
                    allTags[l[n - 1L]] = true;
                }
                if (l[n - 1L] != ctxt.GOARCH)
                {
                    return false;
                }
                if (ctxt.GOOS == "android" && l[n - 2L] == "linux")
                {
                    return true;
                }
                return l[n - 2L] == ctxt.GOOS;
            }
            if (n >= 1L && knownOS[l[n - 1L]])
            {
                if (allTags != null)
                {
                    allTags[l[n - 1L]] = true;
                }
                if (ctxt.GOOS == "android" && l[n - 1L] == "linux")
                {
                    return true;
                }
                return l[n - 1L] == ctxt.GOOS;
            }
            if (n >= 1L && knownArch[l[n - 1L]])
            {
                if (allTags != null)
                {
                    allTags[l[n - 1L]] = true;
                }
                return l[n - 1L] == ctxt.GOARCH;
            }
            return true;
        }

        private static var knownOS = make_map<@string, bool>();
        private static var knownArch = make_map<@string, bool>();

        private static void init()
        {
            {
                var v__prev1 = v;

                foreach (var (_, __v) in strings.Fields(goosList))
                {
                    v = __v;
                    knownOS[v] = true;
                }

                v = v__prev1;
            }

            {
                var v__prev1 = v;

                foreach (var (_, __v) in strings.Fields(goarchList))
                {
                    v = __v;
                    knownArch[v] = true;
                }

                v = v__prev1;
            }

        }

        // ToolDir is the directory containing build tools.
        public static var ToolDir = filepath.Join(runtime.GOROOT(), "pkg/tool/" + runtime.GOOS + "_" + runtime.GOARCH);

        // IsLocalImport reports whether the import path is
        // a local import path, like ".", "..", "./foo", or "../foo".
        public static bool IsLocalImport(@string path)
        {
            return path == "." || path == ".." || strings.HasPrefix(path, "./") || strings.HasPrefix(path, "../");
        }

        // ArchChar returns "?" and an error.
        // In earlier versions of Go, the returned string was used to derive
        // the compiler and linker tool names, the default object file suffix,
        // and the default linker output name. As of Go 1.5, those strings
        // no longer vary by architecture; they are compile, link, .o, and a.out, respectively.
        public static (@string, error) ArchChar(@string goarch)
        {
            return ("?", errors.New("architecture letter no longer used"));
        }
    }
}}
