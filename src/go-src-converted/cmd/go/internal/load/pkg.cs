// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package load loads packages.
// package load -- go2cs converted at 2020 August 29 10:01:01 UTC
// import "cmd/go/internal/load" ==> using load = go.cmd.go.@internal.load_package
// Original source: C:\Go\src\cmd\go\internal\load\pkg.go
using fmt = go.fmt_package;
using build = go.go.build_package;
using token = go.go.token_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using pathpkg = go.path_package;
using filepath = go.path.filepath_package;
using sort = go.sort_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using str = go.cmd.go.@internal.str_package;
using static go.builtin;
using System.ComponentModel;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class load_package
    {
        public static bool IgnoreImports = default; // control whether we ignore imports in packages

        // A Package describes a single package found in a directory.
        public partial struct Package
        {
            public ref PackagePublic PackagePublic => ref PackagePublic_val; // visible in 'go list'
            public PackageInternal Internal; // for use inside go command only
        }

        public partial struct PackagePublic
        {
            [Description("json:\",omitempty\"")]
            public @string Dir; // directory containing package sources
            [Description("json:\",omitempty\"")]
            public @string ImportPath; // import path of package in dir
            [Description("json:\",omitempty\"")]
            public @string ImportComment; // path in import comment on package statement
            [Description("json:\",omitempty\"")]
            public @string Name; // package name
            [Description("json:\",omitempty\"")]
            public @string Doc; // package documentation string
            [Description("json:\",omitempty\"")]
            public @string Target; // installed target for this package (may be executable)
            [Description("json:\",omitempty\"")]
            public @string Shlib; // the shared library that contains this package (only set when -linkshared)
            [Description("json:\",omitempty\"")]
            public bool Goroot; // is this package found in the Go root?
            [Description("json:\",omitempty\"")]
            public bool Standard; // is this package part of the standard Go library?
            [Description("json:\",omitempty\"")]
            public @string Root; // Go root or Go path dir containing this package
            [Description("json:\",omitempty\"")]
            public @string ConflictDir; // Dir is hidden by this other directory
            [Description("json:\",omitempty\"")]
            public bool BinaryOnly; // package cannot be recompiled

// Stale and StaleReason remain here *only* for the list command.
// They are only initialized in preparation for list execution.
// The regular build determines staleness on the fly during action execution.
            [Description("json:\",omitempty\"")]
            public bool Stale; // would 'go install' do anything for this package?
            [Description("json:\",omitempty\"")]
            public @string StaleReason; // why is Stale true?

// Source files
// If you add to this list you MUST add to p.AllFiles (below) too.
// Otherwise file name security lists will not apply to any new additions.
            [Description("json:\",omitempty\"")]
            public slice<@string> GoFiles; // .go source files (excluding CgoFiles, TestGoFiles, XTestGoFiles)
            [Description("json:\",omitempty\"")]
            public slice<@string> CgoFiles; // .go sources files that import "C"
            [Description("json:\",omitempty\"")]
            public slice<@string> IgnoredGoFiles; // .go sources ignored due to build constraints
            [Description("json:\",omitempty\"")]
            public slice<@string> CFiles; // .c source files
            [Description("json:\",omitempty\"")]
            public slice<@string> CXXFiles; // .cc, .cpp and .cxx source files
            [Description("json:\",omitempty\"")]
            public slice<@string> MFiles; // .m source files
            [Description("json:\",omitempty\"")]
            public slice<@string> HFiles; // .h, .hh, .hpp and .hxx source files
            [Description("json:\",omitempty\"")]
            public slice<@string> FFiles; // .f, .F, .for and .f90 Fortran source files
            [Description("json:\",omitempty\"")]
            public slice<@string> SFiles; // .s source files
            [Description("json:\",omitempty\"")]
            public slice<@string> SwigFiles; // .swig files
            [Description("json:\",omitempty\"")]
            public slice<@string> SwigCXXFiles; // .swigcxx files
            [Description("json:\",omitempty\"")]
            public slice<@string> SysoFiles; // .syso system object files added to package

// Cgo directives
            [Description("json:\",omitempty\"")]
            public slice<@string> CgoCFLAGS; // cgo: flags for C compiler
            [Description("json:\",omitempty\"")]
            public slice<@string> CgoCPPFLAGS; // cgo: flags for C preprocessor
            [Description("json:\",omitempty\"")]
            public slice<@string> CgoCXXFLAGS; // cgo: flags for C++ compiler
            [Description("json:\",omitempty\"")]
            public slice<@string> CgoFFLAGS; // cgo: flags for Fortran compiler
            [Description("json:\",omitempty\"")]
            public slice<@string> CgoLDFLAGS; // cgo: flags for linker
            [Description("json:\",omitempty\"")]
            public slice<@string> CgoPkgConfig; // cgo: pkg-config names

// Dependency information
            [Description("json:\",omitempty\"")]
            public slice<@string> Imports; // import paths used by this package
            [Description("json:\",omitempty\"")]
            public slice<@string> Deps; // all (recursively) imported dependencies

// Error information
            [Description("json:\",omitempty\"")]
            public bool Incomplete; // was there an error loading this package or dependencies?
            [Description("json:\",omitempty\"")]
            public ptr<PackageError> Error; // error loading this package (not dependencies)
            [Description("json:\",omitempty\"")]
            public slice<ref PackageError> DepsErrors; // errors loading dependencies

// Test information
// If you add to this list you MUST add to p.AllFiles (below) too.
// Otherwise file name security lists will not apply to any new additions.
            [Description("json:\",omitempty\"")]
            public slice<@string> TestGoFiles; // _test.go files in package
            [Description("json:\",omitempty\"")]
            public slice<@string> TestImports; // imports from TestGoFiles
            [Description("json:\",omitempty\"")]
            public slice<@string> XTestGoFiles; // _test.go files outside package
            [Description("json:\",omitempty\"")]
            public slice<@string> XTestImports; // imports from XTestGoFiles
        }

        // AllFiles returns the names of all the files considered for the package.
        // This is used for sanity and security checks, so we include all files,
        // even IgnoredGoFiles, because some subcommands consider them.
        // The go/build package filtered others out (like foo_wrongGOARCH.s)
        // and that's OK.
        private static slice<@string> AllFiles(this ref Package p)
        {
            return str.StringList(p.GoFiles, p.CgoFiles, p.IgnoredGoFiles, p.CFiles, p.CXXFiles, p.MFiles, p.HFiles, p.FFiles, p.SFiles, p.SwigFiles, p.SwigCXXFiles, p.SysoFiles, p.TestGoFiles, p.XTestGoFiles);
        }

        public partial struct PackageInternal
        {
            public ptr<build.Package> Build;
            public slice<ref Package> Imports; // this package's direct imports
            public slice<@string> RawImports; // this package's original imports as they appear in the text of the program
            public bool ForceLibrary; // this package is a library (even if named "main")
            public bool CmdlineFiles; // package built from files listed on command line
            public bool CmdlinePkg; // package listed on command line
            public bool Local; // imported via local path (./ or ../)
            public @string LocalPrefix; // interpret ./ and ../ imports relative to this prefix
            public @string ExeName; // desired name for temporary executable
            public @string CoverMode; // preprocess Go source files with the coverage tool in this mode
            public map<@string, ref CoverVar> CoverVars; // variables created by coverage analysis
            public bool OmitDebug; // tell linker not to write debug information
            public bool GobinSubdir; // install target would be subdir of GOBIN

            public slice<@string> Asmflags; // -asmflags for this package
            public slice<@string> Gcflags; // -gcflags for this package
            public slice<@string> Ldflags; // -ldflags for this package
            public slice<@string> Gccgoflags; // -gccgoflags for this package
        }

        public partial struct NoGoError
        {
            public ptr<Package> Package;
        }

        private static @string Error(this ref NoGoError e)
        { 
            // Count files beginning with _ and ., which we will pretend don't exist at all.
            long dummy = 0L;
            foreach (var (_, name) in e.Package.IgnoredGoFiles)
            {
                if (strings.HasPrefix(name, "_") || strings.HasPrefix(name, "."))
                {
                    dummy++;
                }
            }
            if (len(e.Package.IgnoredGoFiles) > dummy)
            { 
                // Go files exist, but they were ignored due to build constraints.
                return "build constraints exclude all Go files in " + e.Package.Dir;
            }
            if (len(e.Package.TestGoFiles) + len(e.Package.XTestGoFiles) > 0L)
            { 
                // Test Go files exist, but we're not interested in them.
                // The double-negative is unfortunate but we want e.Package.Dir
                // to appear at the end of error message.
                return "no non-test Go files in " + e.Package.Dir;
            }
            return "no Go files in " + e.Package.Dir;
        }

        // Vendored returns the vendor-resolved version of imports,
        // which should be p.TestImports or p.XTestImports, NOT p.Imports.
        // The imports in p.TestImports and p.XTestImports are not recursively
        // loaded during the initial load of p, so they list the imports found in
        // the source file, but most processing should be over the vendor-resolved
        // import paths. We do this resolution lazily both to avoid file system work
        // and because the eventual real load of the test imports (during 'go test')
        // can produce better error messages if it starts with the original paths.
        // The initial load of p loads all the non-test imports and rewrites
        // the vendored paths, so nothing should ever call p.vendored(p.Imports).
        private static slice<@string> Vendored(this ref Package _p, slice<@string> imports) => func(_p, (ref Package p, Defer _, Panic panic, Recover __) =>
        {
            if (len(imports) > 0L && len(p.Imports) > 0L && ref imports[0L] == ref p.Imports[0L])
            {
                panic("internal error: p.vendored(p.Imports) called");
            }
            var seen = make_map<@string, bool>();
            slice<@string> all = default;
            foreach (var (_, path) in imports)
            {
                path = VendoredImportPath(p, path);
                if (!seen[path])
                {
                    seen[path] = true;
                    all = append(all, path);
                }
            }
            sort.Strings(all);
            return all;
        });

        // CoverVar holds the name of the generated coverage variables targeting the named file.
        public partial struct CoverVar
        {
            public @string File; // local file name
            public @string Var; // name of count struct
        }

        private static void copyBuild(this ref Package p, ref build.Package pp)
        {
            p.Internal.Build = pp;

            if (pp.PkgTargetRoot != "" && cfg.BuildPkgdir != "")
            {
                var old = pp.PkgTargetRoot;
                pp.PkgRoot = cfg.BuildPkgdir;
                pp.PkgTargetRoot = cfg.BuildPkgdir;
                pp.PkgObj = filepath.Join(cfg.BuildPkgdir, strings.TrimPrefix(pp.PkgObj, old));
            }
            p.Dir = pp.Dir;
            p.ImportPath = pp.ImportPath;
            p.ImportComment = pp.ImportComment;
            p.Name = pp.Name;
            p.Doc = pp.Doc;
            p.Root = pp.Root;
            p.ConflictDir = pp.ConflictDir;
            p.BinaryOnly = pp.BinaryOnly; 

            // TODO? Target
            p.Goroot = pp.Goroot;
            p.Standard = p.Goroot && p.ImportPath != "" && isStandardImportPath(p.ImportPath);
            p.GoFiles = pp.GoFiles;
            p.CgoFiles = pp.CgoFiles;
            p.IgnoredGoFiles = pp.IgnoredGoFiles;
            p.CFiles = pp.CFiles;
            p.CXXFiles = pp.CXXFiles;
            p.MFiles = pp.MFiles;
            p.HFiles = pp.HFiles;
            p.FFiles = pp.FFiles;
            p.SFiles = pp.SFiles;
            p.SwigFiles = pp.SwigFiles;
            p.SwigCXXFiles = pp.SwigCXXFiles;
            p.SysoFiles = pp.SysoFiles;
            p.CgoCFLAGS = pp.CgoCFLAGS;
            p.CgoCPPFLAGS = pp.CgoCPPFLAGS;
            p.CgoCXXFLAGS = pp.CgoCXXFLAGS;
            p.CgoFFLAGS = pp.CgoFFLAGS;
            p.CgoLDFLAGS = pp.CgoLDFLAGS;
            p.CgoPkgConfig = pp.CgoPkgConfig; 
            // We modify p.Imports in place, so make copy now.
            p.Imports = make_slice<@string>(len(pp.Imports));
            copy(p.Imports, pp.Imports);
            p.Internal.RawImports = pp.Imports;
            p.TestGoFiles = pp.TestGoFiles;
            p.TestImports = pp.TestImports;
            p.XTestGoFiles = pp.XTestGoFiles;
            p.XTestImports = pp.XTestImports;
            if (IgnoreImports)
            {
                p.Imports = null;
                p.TestImports = null;
                p.XTestImports = null;
            }
        }

        // isStandardImportPath reports whether $GOROOT/src/path should be considered
        // part of the standard distribution. For historical reasons we allow people to add
        // their own code to $GOROOT instead of using $GOPATH, but we assume that
        // code will start with a domain name (dot in the first element).
        private static bool isStandardImportPath(@string path)
        {
            var i = strings.Index(path, "/");
            if (i < 0L)
            {
                i = len(path);
            }
            var elem = path[..i];
            return !strings.Contains(elem, ".");
        }

        // A PackageError describes an error loading information about a package.
        public partial struct PackageError
        {
            public slice<@string> ImportStack; // shortest path from package named on command line to this one
            public @string Pos; // position of error
            public @string Err; // the error itself
            [Description("json:\"-\"")]
            public bool IsImportCycle; // the error is an import cycle
            [Description("json:\"-\"")]
            public bool Hard; // whether the error is soft or hard; soft errors are ignored in some places
        }

        private static @string Error(this ref PackageError p)
        { 
            // Import cycles deserve special treatment.
            if (p.IsImportCycle)
            {
                return fmt.Sprintf("%s\npackage %s\n", p.Err, strings.Join(p.ImportStack, "\n\timports "));
            }
            if (p.Pos != "")
            { 
                // Omit import stack. The full path to the file where the error
                // is the most important thing.
                return p.Pos + ": " + p.Err;
            }
            if (len(p.ImportStack) == 0L)
            {
                return p.Err;
            }
            return "package " + strings.Join(p.ImportStack, "\n\timports ") + ": " + p.Err;
        }

        // An ImportStack is a stack of import paths.
        public partial struct ImportStack // : slice<@string>
        {
        }

        private static void Push(this ref ImportStack s, @string p)
        {
            s.Value = append(s.Value, p);
        }

        private static void Pop(this ref ImportStack s)
        {
            s.Value = (s.Value)[0L..len(s.Value) - 1L];
        }

        private static slice<@string> Copy(this ref ImportStack s)
        {
            return append(new slice<@string>(new @string[] {  }), s.Value);
        }

        // shorterThan reports whether sp is shorter than t.
        // We use this to record the shortest import sequence
        // that leads to a particular package.
        private static bool shorterThan(this ref ImportStack sp, slice<@string> t)
        {
            var s = sp.Value;
            if (len(s) != len(t))
            {
                return len(s) < len(t);
            } 
            // If they are the same length, settle ties using string ordering.
            foreach (var (i) in s)
            {
                if (s[i] != t[i])
                {
                    return s[i] < t[i];
                }
            }
            return false; // they are equal
        }

        // packageCache is a lookup cache for loadPackage,
        // so that if we look up a package multiple times
        // we return the same pointer each time.
        private static map packageCache = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ref Package>{};

        public static void ClearPackageCache()
        {
            foreach (var (name) in packageCache)
            {
                delete(packageCache, name);
            }
        }

        public static void ClearPackageCachePartial(slice<@string> args)
        {
            foreach (var (_, arg) in args)
            {
                var p = packageCache[arg];
                if (p != null)
                {
                    delete(packageCache, p.Dir);
                    delete(packageCache, p.ImportPath);
                }
            }
        }

        // reloadPackage is like loadPackage but makes sure
        // not to use the package cache.
        public static ref Package ReloadPackage(@string arg, ref ImportStack stk)
        {
            var p = packageCache[arg];
            if (p != null)
            {
                delete(packageCache, p.Dir);
                delete(packageCache, p.ImportPath);
            }
            return LoadPackage(arg, stk);
        }

        // dirToImportPath returns the pseudo-import path we use for a package
        // outside the Go path. It begins with _/ and then contains the full path
        // to the directory. If the package lives in c:\home\gopher\my\pkg then
        // the pseudo-import path is _/c_/home/gopher/my/pkg.
        // Using a pseudo-import path like this makes the ./ imports no longer
        // a special case, so that all the code to deal with ordinary imports works
        // automatically.
        private static @string dirToImportPath(@string dir)
        {
            return pathpkg.Join("_", strings.Map(makeImportValid, filepath.ToSlash(dir)));
        }

        private static int makeImportValid(int r)
        { 
            // Should match Go spec, compilers, and ../../go/parser/parser.go:/isValidImport.
            const @string illegalChars = "!\"#$%&\'()*,:;<=>?[\\]^{|}" + "`\uFFFD";

            if (!unicode.IsGraphic(r) || unicode.IsSpace(r) || strings.ContainsRune(illegalChars, r))
            {
                return '_';
            }
            return r;
        }

        // Mode flags for loadImport and download (in get.go).
 
        // UseVendor means that loadImport should do vendor expansion
        // (provided the vendoring experiment is enabled).
        // That is, useVendor means that the import path came from
        // a source file and has not been vendor-expanded yet.
        // Every import path should be loaded initially with useVendor,
        // and then the expanded version (with the /vendor/ in it) gets
        // recorded as the canonical import path. At that point, future loads
        // of that package must not pass useVendor, because
        // disallowVendor will reject direct use of paths containing /vendor/.
        public static readonly long UseVendor = 1L << (int)(iota); 

        // GetTestDeps is for download (part of "go get") and indicates
        // that test dependencies should be fetched too.
        public static readonly var GetTestDeps = 0;

        // LoadImport scans the directory named by path, which must be an import path,
        // but possibly a local import path (an absolute file system path or one beginning
        // with ./ or ../). A local relative path is interpreted relative to srcDir.
        // It returns a *Package describing the package found in that directory.
        public static ref Package LoadImport(@string path, @string srcDir, ref Package _parent, ref ImportStack _stk, slice<token.Position> importPos, long mode) => func(_parent, _stk, (ref Package parent, ref ImportStack stk, Defer defer, Panic _, Recover __) =>
        {
            stk.Push(path);
            defer(stk.Pop()); 

            // Determine canonical identifier for this package.
            // For a local import the identifier is the pseudo-import path
            // we create from the full directory to the package.
            // Otherwise it is the usual import path.
            // For vendored imports, it is the expanded form.
            var importPath = path;
            var origPath = path;
            var isLocal = build.IsLocalImport(path);
            @string debugDeprecatedImportcfgDir = default;
            if (isLocal)
            {
                importPath = dirToImportPath(filepath.Join(srcDir, path));
            }
            else if (DebugDeprecatedImportcfg.enabled)
            {
                {
                    var (d, i) = DebugDeprecatedImportcfg.lookup(parent, path);

                    if (d != "")
                    {
                        debugDeprecatedImportcfgDir = d;
                        importPath = i;
                    }

                }
            }
            else if (mode & UseVendor != 0L)
            { 
                // We do our own vendor resolution, because we want to
                // find out the key to use in packageCache without the
                // overhead of repeated calls to buildContext.Import.
                // The code is also needed in a few other places anyway.
                path = VendoredImportPath(parent, path);
                importPath = path;
            }
            var p = packageCache[importPath];
            if (p != null)
            {
                p = reusePackage(p, stk);
            }
            else
            {
                p = @new<Package>();
                p.Internal.Local = isLocal;
                p.ImportPath = importPath;
                packageCache[importPath] = p; 

                // Load package.
                // Import always returns bp != nil, even if an error occurs,
                // in order to return partial information.
                ref build.Package bp = default;
                error err = default;
                if (debugDeprecatedImportcfgDir != "")
                {
                    bp, err = cfg.BuildContext.ImportDir(debugDeprecatedImportcfgDir, 0L);
                }
                else if (DebugDeprecatedImportcfg.enabled)
                {
                    bp = @new<build.Package>();
                    err = error.As(fmt.Errorf("unknown import path %q: not in import cfg", importPath));
                }
                else
                {
                    var buildMode = build.ImportComment;
                    if (mode & UseVendor == 0L || path != origPath)
                    { 
                        // Not vendoring, or we already found the vendored path.
                        buildMode |= build.IgnoreVendor;
                    }
                    bp, err = cfg.BuildContext.Import(path, srcDir, buildMode);
                }
                bp.ImportPath = importPath;
                if (cfg.GOBIN != "")
                {
                    bp.BinDir = cfg.GOBIN;
                }
                if (debugDeprecatedImportcfgDir == "" && err == null && !isLocal && bp.ImportComment != "" && bp.ImportComment != path && !strings.Contains(path, "/vendor/") && !strings.HasPrefix(path, "vendor/"))
                {
                    err = error.As(fmt.Errorf("code in directory %s expects import %q", bp.Dir, bp.ImportComment));
                }
                p.load(stk, bp, err);
                if (p.Error != null && p.Error.Pos == "")
                {
                    p = setErrorPos(p, importPos);
                }
                if (debugDeprecatedImportcfgDir == "" && origPath != cleanImport(origPath))
                {
                    p.Error = ref new PackageError(ImportStack:stk.Copy(),Err:fmt.Sprintf("non-canonical import path: %q should be %q",origPath,pathpkg.Clean(origPath)),);
                    p.Incomplete = true;
                }
            } 

            // Checked on every import because the rules depend on the code doing the importing.
            {
                var perr__prev1 = perr;

                var perr = disallowInternal(srcDir, p, stk);

                if (perr != p)
                {
                    return setErrorPos(perr, importPos);
                }

                perr = perr__prev1;

            }
            if (mode & UseVendor != 0L)
            {
                {
                    var perr__prev2 = perr;

                    perr = disallowVendor(srcDir, origPath, p, stk);

                    if (perr != p)
                    {
                        return setErrorPos(perr, importPos);
                    }

                    perr = perr__prev2;

                }
            }
            if (p.Name == "main" && parent != null && parent.Dir != p.Dir)
            {
                perr = p.Value;
                perr.Error = ref new PackageError(ImportStack:stk.Copy(),Err:fmt.Sprintf("import %q is a program, not an importable package",path),);
                return setErrorPos(ref perr, importPos);
            }
            if (p.Internal.Local && parent != null && !parent.Internal.Local)
            {
                perr = p.Value;
                perr.Error = ref new PackageError(ImportStack:stk.Copy(),Err:fmt.Sprintf("local import %q in non-local package",path),);
                return setErrorPos(ref perr, importPos);
            }
            return p;
        });

        private static ref Package setErrorPos(ref Package p, slice<token.Position> importPos)
        {
            if (len(importPos) > 0L)
            {
                var pos = importPos[0L];
                pos.Filename = @base.ShortPath(pos.Filename);
                p.Error.Pos = pos.String();
            }
            return p;
        }

        private static @string cleanImport(@string path)
        {
            var orig = path;
            path = pathpkg.Clean(path);
            if (strings.HasPrefix(orig, "./") && path != ".." && !strings.HasPrefix(path, "../"))
            {
                path = "./" + path;
            }
            return path;
        }

        private static map isDirCache = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};

        private static bool isDir(@string path)
        {
            var (result, ok) = isDirCache[path];
            if (ok)
            {
                return result;
            }
            var (fi, err) = os.Stat(path);
            result = err == null && fi.IsDir();
            isDirCache[path] = result;
            return result;
        }

        // VendoredImportPath returns the expansion of path when it appears in parent.
        // If parent is x/y/z, then path might expand to x/y/z/vendor/path, x/y/vendor/path,
        // x/vendor/path, vendor/path, or else stay path if none of those exist.
        // VendoredImportPath returns the expanded path or, if no expansion is found, the original.
        public static @string VendoredImportPath(ref Package parent, @string path)
        {
            if (DebugDeprecatedImportcfg.enabled)
            {
                {
                    var (d, i) = DebugDeprecatedImportcfg.lookup(parent, path);

                    if (d != "")
                    {
                        return i;
                    }

                }
                return path;
            }
            if (parent == null || parent.Root == "")
            {
                return path;
            }
            var dir = filepath.Clean(parent.Dir);
            var root = filepath.Join(parent.Root, "src");
            if (!str.HasFilePathPrefix(dir, root) || parent.ImportPath != "command-line-arguments" && filepath.Join(root, parent.ImportPath) != dir)
            { 
                // Look for symlinks before reporting error.
                dir = expandPath(dir);
                root = expandPath(root);
            }
            if (!str.HasFilePathPrefix(dir, root) || len(dir) <= len(root) || dir[len(root)] != filepath.Separator || parent.ImportPath != "command-line-arguments" && !parent.Internal.Local && filepath.Join(root, parent.ImportPath) != dir)
            {
                @base.Fatalf("unexpected directory layout:\n" + "	import path: %s\n" + "	root: %s\n" + "	dir: %s\n" + "	expand root: %s\n" + "	expand dir: %s\n" + "	separator: %s", parent.ImportPath, filepath.Join(parent.Root, "src"), filepath.Clean(parent.Dir), root, dir, string(filepath.Separator));
            }
            @string vpath = "vendor/" + path;
            for (var i = len(dir); i >= len(root); i--)
            {
                if (i < len(dir) && dir[i] != filepath.Separator)
                {
                    continue;
                } 
                // Note: checking for the vendor directory before checking
                // for the vendor/path directory helps us hit the
                // isDir cache more often. It also helps us prepare a more useful
                // list of places we looked, to report when an import is not found.
                if (!isDir(filepath.Join(dir[..i], "vendor")))
                {
                    continue;
                }
                var targ = filepath.Join(dir[..i], vpath);
                if (isDir(targ) && hasGoFiles(targ))
                {
                    var importPath = parent.ImportPath;
                    if (importPath == "command-line-arguments")
                    { 
                        // If parent.ImportPath is 'command-line-arguments'.
                        // set to relative directory to root (also chopped root directory)
                        importPath = dir[len(root) + 1L..];
                    } 
                    // We started with parent's dir c:\gopath\src\foo\bar\baz\quux\xyzzy.
                    // We know the import path for parent's dir.
                    // We chopped off some number of path elements and
                    // added vendor\path to produce c:\gopath\src\foo\bar\baz\vendor\path.
                    // Now we want to know the import path for that directory.
                    // Construct it by chopping the same number of path elements
                    // (actually the same number of bytes) from parent's import path
                    // and then append /vendor/path.
                    var chopped = len(dir) - i;
                    if (chopped == len(importPath) + 1L)
                    { 
                        // We walked up from c:\gopath\src\foo\bar
                        // and found c:\gopath\src\vendor\path.
                        // We chopped \foo\bar (length 8) but the import path is "foo/bar" (length 7).
                        // Use "vendor/path" without any prefix.
                        return vpath;
                    }
                    return importPath[..len(importPath) - chopped] + "/" + vpath;
                }
            }

            return path;
        }

        // hasGoFiles reports whether dir contains any files with names ending in .go.
        // For a vendor check we must exclude directories that contain no .go files.
        // Otherwise it is not possible to vendor just a/b/c and still import the
        // non-vendored a/b. See golang.org/issue/13832.
        private static bool hasGoFiles(@string dir)
        {
            var (fis, _) = ioutil.ReadDir(dir);
            foreach (var (_, fi) in fis)
            {
                if (!fi.IsDir() && strings.HasSuffix(fi.Name(), ".go"))
                {
                    return true;
                }
            }
            return false;
        }

        // reusePackage reuses package p to satisfy the import at the top
        // of the import stack stk. If this use causes an import loop,
        // reusePackage updates p's error information to record the loop.
        private static ref Package reusePackage(ref Package p, ref ImportStack stk)
        { 
            // We use p.Internal.Imports==nil to detect a package that
            // is in the midst of its own loadPackage call
            // (all the recursion below happens before p.Internal.Imports gets set).
            if (p.Internal.Imports == null)
            {
                if (p.Error == null)
                {
                    p.Error = ref new PackageError(ImportStack:stk.Copy(),Err:"import cycle not allowed",IsImportCycle:true,);
                }
                p.Incomplete = true;
            } 
            // Don't rewrite the import stack in the error if we have an import cycle.
            // If we do, we'll lose the path that describes the cycle.
            if (p.Error != null && !p.Error.IsImportCycle && stk.shorterThan(p.Error.ImportStack))
            {
                p.Error.ImportStack = stk.Copy();
            }
            return p;
        }

        // disallowInternal checks that srcDir is allowed to import p.
        // If the import is allowed, disallowInternal returns the original package p.
        // If not, it returns a new package containing just an appropriate error.
        private static ref Package disallowInternal(@string srcDir, ref Package p, ref ImportStack stk)
        { 
            // golang.org/s/go14internal:
            // An import of a path containing the element “internal”
            // is disallowed if the importing code is outside the tree
            // rooted at the parent of the “internal” directory.

            // There was an error loading the package; stop here.
            if (p.Error != null)
            {
                return p;
            } 

            // The generated 'testmain' package is allowed to access testing/internal/...,
            // as if it were generated into the testing directory tree
            // (it's actually in a temporary directory outside any Go tree).
            // This cleans up a former kludge in passing functionality to the testing package.
            if (strings.HasPrefix(p.ImportPath, "testing/internal") && len(stk.Value) >= 2L && (stk.Value)[len(stk.Value) - 2L] == "testmain")
            {
                return p;
            } 

            // We can't check standard packages with gccgo.
            if (cfg.BuildContext.Compiler == "gccgo" && p.Standard)
            {
                return p;
            } 

            // The stack includes p.ImportPath.
            // If that's the only thing on the stack, we started
            // with a name given on the command line, not an
            // import. Anything listed on the command line is fine.
            if (len(stk.Value) == 1L)
            {
                return p;
            } 

            // Check for "internal" element: three cases depending on begin of string and/or end of string.
            var (i, ok) = findInternal(p.ImportPath);
            if (!ok)
            {
                return p;
            } 

            // Internal is present.
            // Map import path back to directory corresponding to parent of internal.
            if (i > 0L)
            {
                i--; // rewind over slash in ".../internal"
            }
            var parent = p.Dir[..i + len(p.Dir) - len(p.ImportPath)];
            if (str.HasFilePathPrefix(filepath.Clean(srcDir), filepath.Clean(parent)))
            {
                return p;
            } 

            // Look for symlinks before reporting error.
            srcDir = expandPath(srcDir);
            parent = expandPath(parent);
            if (str.HasFilePathPrefix(filepath.Clean(srcDir), filepath.Clean(parent)))
            {
                return p;
            } 

            // Internal is present, and srcDir is outside parent's tree. Not allowed.
            var perr = p.Value;
            perr.Error = ref new PackageError(ImportStack:stk.Copy(),Err:"use of internal package not allowed",);
            perr.Incomplete = true;
            return ref perr;
        }

        // findInternal looks for the final "internal" path element in the given import path.
        // If there isn't one, findInternal returns ok=false.
        // Otherwise, findInternal returns ok=true and the index of the "internal".
        private static (long, bool) findInternal(@string path)
        { 
            // Three cases, depending on internal at start/end of string or not.
            // The order matters: we must return the index of the final element,
            // because the final one produces the most restrictive requirement
            // on the importer.

            if (strings.HasSuffix(path, "/internal")) 
                return (len(path) - len("internal"), true);
            else if (strings.Contains(path, "/internal/")) 
                return (strings.LastIndex(path, "/internal/") + 1L, true);
            else if (path == "internal" || strings.HasPrefix(path, "internal/")) 
                return (0L, true);
                        return (0L, false);
        }

        // disallowVendor checks that srcDir is allowed to import p as path.
        // If the import is allowed, disallowVendor returns the original package p.
        // If not, it returns a new package containing just an appropriate error.
        private static ref Package disallowVendor(@string srcDir, @string path, ref Package p, ref ImportStack stk)
        { 
            // The stack includes p.ImportPath.
            // If that's the only thing on the stack, we started
            // with a name given on the command line, not an
            // import. Anything listed on the command line is fine.
            if (len(stk.Value) == 1L)
            {
                return p;
            }
            {
                var perr__prev1 = perr;

                var perr = disallowVendorVisibility(srcDir, p, stk);

                if (perr != p)
                {
                    return perr;
                } 

                // Paths like x/vendor/y must be imported as y, never as x/vendor/y.

                perr = perr__prev1;

            } 

            // Paths like x/vendor/y must be imported as y, never as x/vendor/y.
            {
                var (i, ok) = FindVendor(path);

                if (ok)
                {
                    perr = p.Value;
                    perr.Error = ref new PackageError(ImportStack:stk.Copy(),Err:"must be imported as "+path[i+len("vendor/"):],);
                    perr.Incomplete = true;
                    return ref perr;
                }

            }

            return p;
        }

        // disallowVendorVisibility checks that srcDir is allowed to import p.
        // The rules are the same as for /internal/ except that a path ending in /vendor
        // is not subject to the rules, only subdirectories of vendor.
        // This allows people to have packages and commands named vendor,
        // for maximal compatibility with existing source trees.
        private static ref Package disallowVendorVisibility(@string srcDir, ref Package p, ref ImportStack stk)
        { 
            // The stack includes p.ImportPath.
            // If that's the only thing on the stack, we started
            // with a name given on the command line, not an
            // import. Anything listed on the command line is fine.
            if (len(stk.Value) == 1L)
            {
                return p;
            } 

            // Check for "vendor" element.
            var (i, ok) = FindVendor(p.ImportPath);
            if (!ok)
            {
                return p;
            } 

            // Vendor is present.
            // Map import path back to directory corresponding to parent of vendor.
            if (i > 0L)
            {
                i--; // rewind over slash in ".../vendor"
            }
            var truncateTo = i + len(p.Dir) - len(p.ImportPath);
            if (truncateTo < 0L || len(p.Dir) < truncateTo)
            {
                return p;
            }
            var parent = p.Dir[..truncateTo];
            if (str.HasFilePathPrefix(filepath.Clean(srcDir), filepath.Clean(parent)))
            {
                return p;
            } 

            // Look for symlinks before reporting error.
            srcDir = expandPath(srcDir);
            parent = expandPath(parent);
            if (str.HasFilePathPrefix(filepath.Clean(srcDir), filepath.Clean(parent)))
            {
                return p;
            } 

            // Vendor is present, and srcDir is outside parent's tree. Not allowed.
            var perr = p.Value;
            perr.Error = ref new PackageError(ImportStack:stk.Copy(),Err:"use of vendored package not allowed",);
            perr.Incomplete = true;
            return ref perr;
        }

        // FindVendor looks for the last non-terminating "vendor" path element in the given import path.
        // If there isn't one, FindVendor returns ok=false.
        // Otherwise, FindVendor returns ok=true and the index of the "vendor".
        //
        // Note that terminating "vendor" elements don't count: "x/vendor" is its own package,
        // not the vendored copy of an import "" (the empty import path).
        // This will allow people to have packages or commands named vendor.
        // This may help reduce breakage, or it may just be confusing. We'll see.
        public static (long, bool) FindVendor(@string path)
        { 
            // Two cases, depending on internal at start of string or not.
            // The order matters: we must return the index of the final element,
            // because the final one is where the effective import path starts.

            if (strings.Contains(path, "/vendor/")) 
                return (strings.LastIndex(path, "/vendor/") + 1L, true);
            else if (strings.HasPrefix(path, "vendor/")) 
                return (0L, true);
                        return (0L, false);
        }

        public partial struct TargetDir // : long
        {
        }

        public static readonly TargetDir ToTool = iota; // to GOROOT/pkg/tool (default for cmd/*)
        public static readonly var ToBin = 0; // to bin dir inside package root (default for non-cmd/*)
        public static readonly var StalePath = 1; // an old import path; fail to build

        // InstallTargetDir reports the target directory for installing the command p.
        public static TargetDir InstallTargetDir(ref Package p)
        {
            if (strings.HasPrefix(p.ImportPath, "code.google.com/p/go.tools/cmd/"))
            {
                return StalePath;
            }
            if (p.Goroot && strings.HasPrefix(p.ImportPath, "cmd/") && p.Name == "main")
            {
                switch (p.ImportPath)
                {
                    case "cmd/go": 

                    case "cmd/gofmt": 
                        return ToBin;
                        break;
                }
                return ToTool;
            }
            return ToBin;
        }

        private static map cgoExclude = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"runtime/cgo":true,};

        private static map cgoSyscallExclude = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"runtime/cgo":true,"runtime/race":true,"runtime/msan":true,};

        private static var foldPath = make_map<@string, @string>();

        // load populates p using information from bp, err, which should
        // be the result of calling build.Context.Import.
        private static void load(this ref Package _p, ref ImportStack _stk, ref build.Package _bp, error err) => func(_p, _stk, _bp, (ref Package p, ref ImportStack stk, ref build.Package bp, Defer _, Panic panic, Recover __) =>
        {
            p.copyBuild(bp); 

            // Decide whether p was listed on the command line.
            // Given that load is called while processing the command line,
            // you might think we could simply pass a flag down into load
            // saying whether we are loading something named on the command
            // line or something to satisfy an import. But the first load of a
            // package named on the command line may be as a dependency
            // of an earlier package named on the command line, not when we
            // get to that package during command line processing.
            // For example "go test fmt reflect" will load reflect as a dependency
            // of fmt before it attempts to load as a command-line argument.
            // Because loads are cached, the later load will be a no-op,
            // so it is important that the first load can fill in CmdlinePkg correctly.
            // Hence the call to an explicit matching check here.
            p.Internal.CmdlinePkg = isCmdlinePkg(p);

            p.Internal.Asmflags = BuildAsmflags.For(p);
            p.Internal.Gcflags = BuildGcflags.For(p);
            p.Internal.Ldflags = BuildLdflags.For(p);
            p.Internal.Gccgoflags = BuildGccgoflags.For(p); 

            // The localPrefix is the path we interpret ./ imports relative to.
            // Synthesized main packages sometimes override this.
            if (p.Internal.Local)
            {
                p.Internal.LocalPrefix = dirToImportPath(p.Dir);
            }
            if (err != null)
            {
                {
                    ref build.NoGoError (_, ok) = err._<ref build.NoGoError>();

                    if (ok)
                    {
                        err = ref new NoGoError(Package:p);
                    }

                }
                p.Incomplete = true;
                err = @base.ExpandScanner(err);
                p.Error = ref new PackageError(ImportStack:stk.Copy(),Err:err.Error(),);
                return;
            }
            var useBindir = p.Name == "main";
            if (!p.Standard)
            {
                switch (cfg.BuildBuildmode)
                {
                    case "c-archive": 

                    case "c-shared": 

                    case "plugin": 
                        useBindir = false;
                        break;
                }
            }
            if (useBindir)
            { 
                // Report an error when the old code.google.com/p/go.tools paths are used.
                if (InstallTargetDir(p) == StalePath)
                {
                    var newPath = strings.Replace(p.ImportPath, "code.google.com/p/go.", "golang.org/x/", 1L);
                    var e = fmt.Sprintf("the %v command has moved; use %v instead.", p.ImportPath, newPath);
                    p.Error = ref new PackageError(Err:e);
                    return;
                }
                var (_, elem) = filepath.Split(p.Dir);
                var full = cfg.BuildContext.GOOS + "_" + cfg.BuildContext.GOARCH + "/" + elem;
                if (cfg.BuildContext.GOOS != @base.ToolGOOS || cfg.BuildContext.GOARCH != @base.ToolGOARCH)
                { 
                    // Install cross-compiled binaries to subdirectories of bin.
                    elem = full;
                }
                if (p.Internal.Build.BinDir != "")
                { 
                    // Install to GOBIN or bin of GOPATH entry.
                    p.Target = filepath.Join(p.Internal.Build.BinDir, elem);
                    if (!p.Goroot && strings.Contains(elem, "/") && cfg.GOBIN != "")
                    { 
                        // Do not create $GOBIN/goos_goarch/elem.
                        p.Target = "";
                        p.Internal.GobinSubdir = true;
                    }
                }
                if (InstallTargetDir(p) == ToTool)
                { 
                    // This is for 'go tool'.
                    // Override all the usual logic and force it into the tool directory.
                    p.Target = filepath.Join(cfg.GOROOTpkg, "tool", full);
                }
                if (p.Target != "" && cfg.BuildContext.GOOS == "windows")
                {
                    p.Target += ".exe";
                }
            }
            else if (p.Internal.Local)
            { 
                // Local import turned into absolute path.
                // No permanent install target.
                p.Target = "";
            }
            else
            {
                p.Target = p.Internal.Build.PkgObj;
                if (cfg.BuildLinkshared)
                {
                    var shlibnamefile = p.Target[..len(p.Target) - 2L] + ".shlibname";
                    var (shlib, err) = ioutil.ReadFile(shlibnamefile);
                    if (err != null && !os.IsNotExist(err))
                    {
                        @base.Fatalf("reading shlibname: %v", err);
                    }
                    if (err == null)
                    {
                        var libname = strings.TrimSpace(string(shlib));
                        if (cfg.BuildContext.Compiler == "gccgo")
                        {
                            p.Shlib = filepath.Join(p.Internal.Build.PkgTargetRoot, "shlibs", libname);
                        }
                        else
                        {
                            p.Shlib = filepath.Join(p.Internal.Build.PkgTargetRoot, libname);
                        }
                    }
                }
            } 

            // Build augmented import list to add implicit dependencies.
            // Be careful not to add imports twice, just to avoid confusion.
            var importPaths = p.Imports;
            Action<@string> addImport = path =>
            {
                foreach (var (_, p) in importPaths)
                {
                    if (path == p)
                    {
                        return;
                    }
                }
                importPaths = append(importPaths, path);
            } 

            // Cgo translation adds imports of "runtime/cgo" and "syscall",
            // except for certain packages, to avoid circular dependencies.
; 

            // Cgo translation adds imports of "runtime/cgo" and "syscall",
            // except for certain packages, to avoid circular dependencies.
            if (p.UsesCgo() && (!p.Standard || !cgoExclude[p.ImportPath]))
            {
                addImport("runtime/cgo");
            }
            if (p.UsesCgo() && (!p.Standard || !cgoSyscallExclude[p.ImportPath]))
            {
                addImport("syscall");
            } 

            // SWIG adds imports of some standard packages.
            if (p.UsesSwig())
            {
                addImport("runtime/cgo");
                addImport("syscall");
                addImport("sync"); 

                // TODO: The .swig and .swigcxx files can use
                // %go_import directives to import other packages.
            } 

            // The linker loads implicit dependencies.
            if (p.Name == "main" && !p.Internal.ForceLibrary)
            {
                {
                    var dep__prev1 = dep;

                    foreach (var (_, __dep) in LinkerDeps(p))
                    {
                        dep = __dep;
                        addImport(dep);
                    }

                    dep = dep__prev1;
                }

            } 

            // Check for case-insensitive collision of input files.
            // To avoid problems on case-insensitive files, we reject any package
            // where two different input files have equal names under a case-insensitive
            // comparison.
            var inputs = p.AllFiles();
            var (f1, f2) = str.FoldDup(inputs);
            if (f1 != "")
            {
                p.Error = ref new PackageError(ImportStack:stk.Copy(),Err:fmt.Sprintf("case-insensitive file name collision: %q and %q",f1,f2),);
                return;
            } 

            // If first letter of input file is ASCII, it must be alphanumeric.
            // This avoids files turning into flags when invoking commands,
            // and other problems we haven't thought of yet.
            // Also, _cgo_ files must be generated by us, not supplied.
            // They are allowed to have //go:cgo_ldflag directives.
            // The directory scan ignores files beginning with _,
            // so we shouldn't see any _cgo_ files anyway, but just be safe.
            foreach (var (_, file) in inputs)
            {
                if (!SafeArg(file) || strings.HasPrefix(file, "_cgo_"))
                {
                    p.Error = ref new PackageError(ImportStack:stk.Copy(),Err:fmt.Sprintf("invalid input file name %q",file),);
                    return;
                }
            }
            {
                var name = pathpkg.Base(p.ImportPath);

                if (!SafeArg(name))
                {
                    p.Error = ref new PackageError(ImportStack:stk.Copy(),Err:fmt.Sprintf("invalid input directory name %q",name),);
                    return;
                }

            }
            if (!SafeArg(p.ImportPath))
            {
                p.Error = ref new PackageError(ImportStack:stk.Copy(),Err:fmt.Sprintf("invalid import path %q",p.ImportPath),);
                return;
            } 

            // Build list of imported packages and full dependency list.
            var imports = make_slice<ref Package>(0L, len(p.Imports));
            {
                var i__prev1 = i;
                var path__prev1 = path;

                foreach (var (__i, __path) in importPaths)
                {
                    i = __i;
                    path = __path;
                    if (path == "C")
                    {
                        continue;
                    }
                    var p1 = LoadImport(path, p.Dir, p, stk, p.Internal.Build.ImportPos[path], UseVendor);
                    if (p.Standard && p.Error == null && !p1.Standard && p1.Error == null)
                    {
                        p.Error = ref new PackageError(ImportStack:stk.Copy(),Err:fmt.Sprintf("non-standard import %q in standard package %q",path,p.ImportPath),);
                        var pos = p.Internal.Build.ImportPos[path];
                        if (len(pos) > 0L)
                        {
                            p.Error.Pos = pos[0L].String();
                        }
                    }
                    path = p1.ImportPath;
                    importPaths[i] = path;
                    if (i < len(p.Imports))
                    {
                        p.Imports[i] = path;
                    }
                    imports = append(imports, p1);
                    if (p1.Incomplete)
                    {
                        p.Incomplete = true;
                    }
                }

                i = i__prev1;
                path = path__prev1;
            }

            p.Internal.Imports = imports;

            var deps = make_map<@string, ref Package>();
            slice<ref Package> q = default;
            q = append(q, imports);
            {
                var i__prev1 = i;

                for (long i = 0L; i < len(q); i++)
                {
                    p1 = q[i];
                    var path = p1.ImportPath; 
                    // The same import path could produce an error or not,
                    // depending on what tries to import it.
                    // Prefer to record entries with errors, so we can report them.
                    var p0 = deps[path];
                    if (p0 == null || p1.Error != null && (p0.Error == null || len(p0.Error.ImportStack) > len(p1.Error.ImportStack)))
                    {
                        deps[path] = p1;
                        foreach (var (_, p2) in p1.Internal.Imports)
                        {
                            if (deps[p2.ImportPath] != p2)
                            {
                                q = append(q, p2);
                            }
                        }
                    }
                }


                i = i__prev1;
            }

            p.Deps = make_slice<@string>(0L, len(deps));
            {
                var dep__prev1 = dep;

                foreach (var (__dep) in deps)
                {
                    dep = __dep;
                    p.Deps = append(p.Deps, dep);
                }

                dep = dep__prev1;
            }

            sort.Strings(p.Deps);
            {
                var dep__prev1 = dep;

                foreach (var (_, __dep) in p.Deps)
                {
                    dep = __dep;
                    p1 = deps[dep];
                    if (p1 == null)
                    {
                        panic("impossible: missing entry in package cache for " + dep + " imported by " + p.ImportPath);
                    }
                    if (p1.Error != null)
                    {
                        p.DepsErrors = append(p.DepsErrors, p1.Error);
                    }
                } 

                // unsafe is a fake package.

                dep = dep__prev1;
            }

            if (p.Standard && (p.ImportPath == "unsafe" || cfg.BuildContext.Compiler == "gccgo"))
            {
                p.Target = "";
            } 

            // If cgo is not enabled, ignore cgo supporting sources
            // just as we ignore go files containing import "C".
            if (!cfg.BuildContext.CgoEnabled)
            {
                p.CFiles = null;
                p.CXXFiles = null;
                p.MFiles = null;
                p.SwigFiles = null;
                p.SwigCXXFiles = null; 
                // Note that SFiles are okay (they go to the Go assembler)
                // and HFiles are okay (they might be used by the SFiles).
                // Also Sysofiles are okay (they might not contain object
                // code; see issue #16050).
            }
            Action<@string> setError = msg =>
            {
                p.Error = ref new PackageError(ImportStack:stk.Copy(),Err:msg,);
            } 

            // The gc toolchain only permits C source files with cgo or SWIG.
; 

            // The gc toolchain only permits C source files with cgo or SWIG.
            if (len(p.CFiles) > 0L && !p.UsesCgo() && !p.UsesSwig() && cfg.BuildContext.Compiler == "gc")
            {
                setError(fmt.Sprintf("C source files not allowed when not using cgo or SWIG: %s", strings.Join(p.CFiles, " ")));
                return;
            } 

            // C++, Objective-C, and Fortran source files are permitted only with cgo or SWIG,
            // regardless of toolchain.
            if (len(p.CXXFiles) > 0L && !p.UsesCgo() && !p.UsesSwig())
            {
                setError(fmt.Sprintf("C++ source files not allowed when not using cgo or SWIG: %s", strings.Join(p.CXXFiles, " ")));
                return;
            }
            if (len(p.MFiles) > 0L && !p.UsesCgo() && !p.UsesSwig())
            {
                setError(fmt.Sprintf("Objective-C source files not allowed when not using cgo or SWIG: %s", strings.Join(p.MFiles, " ")));
                return;
            }
            if (len(p.FFiles) > 0L && !p.UsesCgo() && !p.UsesSwig())
            {
                setError(fmt.Sprintf("Fortran source files not allowed when not using cgo or SWIG: %s", strings.Join(p.FFiles, " ")));
                return;
            } 

            // Check for case-insensitive collisions of import paths.
            var fold = str.ToFold(p.ImportPath);
            {
                var other = foldPath[fold];

                if (other == "")
                {
                    foldPath[fold] = p.ImportPath;
                }
                else if (other != p.ImportPath)
                {
                    setError(fmt.Sprintf("case-insensitive import collision: %q and %q", p.ImportPath, other));
                    return;
                }

            }
        });

        // SafeArg reports whether arg is a "safe" command-line argument,
        // meaning that when it appears in a command-line, it probably
        // doesn't have some special meaning other than its own name.
        // Obviously args beginning with - are not safe (they look like flags).
        // Less obviously, args beginning with @ are not safe (they look like
        // GNU binutils flagfile specifiers, sometimes called "response files").
        // To be conservative, we reject almost any arg beginning with non-alphanumeric ASCII.
        // We accept leading . _ and / as likely in file system paths.
        // There is a copy of this function in cmd/compile/internal/gc/noder.go.
        public static bool SafeArg(@string name)
        {
            if (name == "")
            {
                return false;
            }
            var c = name[0L];
            return '0' <= c && c <= '9' || 'A' <= c && c <= 'Z' || 'a' <= c && c <= 'z' || c == '.' || c == '_' || c == '/' || c >= utf8.RuneSelf;
        }

        // LinkerDeps returns the list of linker-induced dependencies for main package p.
        public static slice<@string> LinkerDeps(ref Package p)
        { 
            // Everything links runtime.
            @string deps = new slice<@string>(new @string[] { "runtime" }); 

            // External linking mode forces an import of runtime/cgo.
            if (externalLinkingForced(p))
            {
                deps = append(deps, "runtime/cgo");
            } 
            // On ARM with GOARM=5, it forces an import of math, for soft floating point.
            if (cfg.Goarch == "arm")
            {
                deps = append(deps, "math");
            } 
            // Using the race detector forces an import of runtime/race.
            if (cfg.BuildRace)
            {
                deps = append(deps, "runtime/race");
            } 
            // Using memory sanitizer forces an import of runtime/msan.
            if (cfg.BuildMSan)
            {
                deps = append(deps, "runtime/msan");
            }
            return deps;
        }

        // externalLinkingForced reports whether external linking is being
        // forced even for programs that do not use cgo.
        private static bool externalLinkingForced(ref Package p)
        { 
            // Some targets must use external linking even inside GOROOT.
            switch (cfg.BuildContext.GOOS)
            {
                case "android": 
                    return true;
                    break;
                case "darwin": 
                    switch (cfg.BuildContext.GOARCH)
                    {
                        case "arm": 

                        case "arm64": 
                            return true;
                            break;
                    }
                    break;
            }

            if (!cfg.BuildContext.CgoEnabled)
            {
                return false;
            } 
            // Currently build modes c-shared, pie (on systems that do not
            // support PIE with internal linking mode (currently all
            // systems: issue #18968)), plugin, and -linkshared force
            // external linking mode, as of course does
            // -ldflags=-linkmode=external. External linking mode forces
            // an import of runtime/cgo.
            var pieCgo = cfg.BuildBuildmode == "pie";
            var linkmodeExternal = false;
            if (p != null)
            {
                var ldflags = BuildLdflags.For(p);
                foreach (var (i, a) in ldflags)
                {
                    if (a == "-linkmode=external")
                    {
                        linkmodeExternal = true;
                    }
                    if (a == "-linkmode" && i + 1L < len(ldflags) && ldflags[i + 1L] == "external")
                    {
                        linkmodeExternal = true;
                    }
                }
            }
            return cfg.BuildBuildmode == "c-shared" || cfg.BuildBuildmode == "plugin" || pieCgo || cfg.BuildLinkshared || linkmodeExternal;
        }

        // mkAbs rewrites list, which must be paths relative to p.Dir,
        // into a sorted list of absolute paths. It edits list in place but for
        // convenience also returns list back to its caller.
        private static slice<@string> mkAbs(this ref Package p, slice<@string> list)
        {
            foreach (var (i, f) in list)
            {
                list[i] = filepath.Join(p.Dir, f);
            }
            sort.Strings(list);
            return list;
        }

        // InternalGoFiles returns the list of Go files being built for the package,
        // using absolute paths.
        private static slice<@string> InternalGoFiles(this ref Package p)
        {
            return p.mkAbs(str.StringList(p.GoFiles, p.CgoFiles, p.TestGoFiles, p.XTestGoFiles));
        }

        // InternalGoFiles returns the list of all Go files possibly relevant for the package,
        // using absolute paths. "Possibly relevant" means that files are not excluded
        // due to build tags, but files with names beginning with . or _ are still excluded.
        private static slice<@string> InternalAllGoFiles(this ref Package p)
        {
            slice<@string> extra = default;
            foreach (var (_, f) in p.IgnoredGoFiles)
            {
                if (f != "" && f[0L] != '.' || f[0L] != '_')
                {
                    extra = append(extra, f);
                }
            }
            return p.mkAbs(str.StringList(extra, p.GoFiles, p.CgoFiles, p.TestGoFiles, p.XTestGoFiles));
        }

        // usesSwig reports whether the package needs to run SWIG.
        private static bool UsesSwig(this ref Package p)
        {
            return len(p.SwigFiles) > 0L || len(p.SwigCXXFiles) > 0L;
        }

        // usesCgo reports whether the package needs to run cgo
        private static bool UsesCgo(this ref Package p)
        {
            return len(p.CgoFiles) > 0L;
        }

        // packageList returns the list of packages in the dag rooted at roots
        // as visited in a depth-first post-order traversal.
        public static slice<ref Package> PackageList(slice<ref Package> roots)
        {
            map seen = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ref Package, bool>{};
            ref Package all = new slice<ref Package>(new ref Package[] {  });
            Action<ref Package> walk = default;
            walk = p =>
            {
                if (seen[p])
                {
                    return;
                }
                seen[p] = true;
                foreach (var (_, p1) in p.Internal.Imports)
                {
                    walk(p1);
                }
                all = append(all, p);
            }
;
            foreach (var (_, root) in roots)
            {
                walk(root);
            }
            return all;
        }

        private static map cmdCache = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ref Package>{};

        public static void ClearCmdCache()
        {
            foreach (var (name) in cmdCache)
            {
                delete(cmdCache, name);
            }
        }

        // loadPackage is like loadImport but is used for command-line arguments,
        // not for paths found in import statements. In addition to ordinary import paths,
        // loadPackage accepts pseudo-paths beginning with cmd/ to denote commands
        // in the Go command directory, as well as paths to those directories.
        public static ref Package LoadPackage(@string arg, ref ImportStack _stk) => func(_stk, (ref ImportStack stk, Defer defer, Panic _, Recover __) =>
        {
            if (build.IsLocalImport(arg))
            {
                var dir = arg;
                if (!filepath.IsAbs(dir))
                {
                    {
                        var (abs, err) = filepath.Abs(dir);

                        if (err == null)
                        { 
                            // interpret relative to current directory
                            dir = abs;
                        }

                    }
                }
                {
                    var (sub, ok) = hasSubdir(cfg.GOROOTsrc, dir);

                    if (ok && strings.HasPrefix(sub, "cmd/") && !strings.Contains(sub[4L..], "/"))
                    {
                        arg = sub;
                    }

                }
            }
            if (strings.HasPrefix(arg, "cmd/") && !strings.Contains(arg[4L..], "/"))
            {
                {
                    var p__prev2 = p;

                    var p = cmdCache[arg];

                    if (p != null)
                    {
                        return p;
                    }

                    p = p__prev2;

                }
                stk.Push(arg);
                defer(stk.Pop());

                var (bp, err) = cfg.BuildContext.ImportDir(filepath.Join(cfg.GOROOTsrc, arg), 0L);
                bp.ImportPath = arg;
                bp.Goroot = true;
                bp.BinDir = cfg.GOROOTbin;
                if (cfg.GOROOTbin != "")
                {
                    bp.BinDir = cfg.GOROOTbin;
                }
                bp.Root = cfg.GOROOT;
                bp.SrcRoot = cfg.GOROOTsrc;
                p = @new<Package>();
                cmdCache[arg] = p;
                p.load(stk, bp, err);
                if (p.Error == null && p.Name != "main")
                {
                    p.Error = ref new PackageError(ImportStack:stk.Copy(),Err:fmt.Sprintf("expected package main but found package %s in %s",p.Name,p.Dir),);
                }
                return p;
            } 

            // Wasn't a command; must be a package.
            // If it is a local import path but names a standard package,
            // we treat it as if the user specified the standard package.
            // This lets you run go test ./ioutil in package io and be
            // referring to io/ioutil rather than a hypothetical import of
            // "./ioutil".
            if (build.IsLocalImport(arg))
            {
                var (bp, _) = cfg.BuildContext.ImportDir(filepath.Join(@base.Cwd, arg), build.FindOnly);
                if (bp.ImportPath != "" && bp.ImportPath != ".")
                {
                    arg = bp.ImportPath;
                }
            }
            return LoadImport(arg, @base.Cwd, null, stk, null, 0L);
        });

        // packages returns the packages named by the
        // command line arguments 'args'. If a named package
        // cannot be loaded at all (for example, if the directory does not exist),
        // then packages prints an error and does not include that
        // package in the results. However, if errors occur trying
        // to load dependencies of a named package, the named
        // package is still returned, with p.Incomplete = true
        // and details in p.DepsErrors.
        public static slice<ref Package> Packages(slice<@string> args)
        {
            slice<ref Package> pkgs = default;
            foreach (var (_, pkg) in PackagesAndErrors(args))
            {
                if (pkg.Error != null)
                {
                    @base.Errorf("can't load package: %s", pkg.Error);
                    continue;
                }
                pkgs = append(pkgs, pkg);
            }
            return pkgs;
        }

        // packagesAndErrors is like 'packages' but returns a
        // *Package for every argument, even the ones that
        // cannot be loaded at all.
        // The packages that fail to load will have p.Error != nil.
        public static slice<ref Package> PackagesAndErrors(slice<@string> args)
        {
            if (len(args) > 0L && strings.HasSuffix(args[0L], ".go"))
            {
                return new slice<ref Package>(new ref Package[] { GoFilesPackage(args) });
            }
            args = ImportPaths(args);
            slice<ref Package> pkgs = default;            ImportStack stk = default;            var seenArg = make_map<@string, bool>();            var seenPkg = make_map<ref Package, bool>();

            foreach (var (_, arg) in args)
            {
                if (seenArg[arg])
                {
                    continue;
                }
                seenArg[arg] = true;
                var pkg = LoadPackage(arg, ref stk);
                if (seenPkg[pkg])
                {
                    continue;
                }
                seenPkg[pkg] = true;
                pkgs = append(pkgs, pkg);
            }
            return pkgs;
        }

        // packagesForBuild is like 'packages' but fails if any of
        // the packages or their dependencies have errors
        // (cannot be built).
        public static slice<ref Package> PackagesForBuild(slice<@string> args)
        {
            var pkgs = PackagesAndErrors(args);
            map printed = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ref PackageError, bool>{};
            {
                var pkg__prev1 = pkg;

                foreach (var (_, __pkg) in pkgs)
                {
                    pkg = __pkg;
                    if (pkg.Error != null)
                    {
                        @base.Errorf("can't load package: %s", pkg.Error);
                    }
                    foreach (var (_, err) in pkg.DepsErrors)
                    { 
                        // Since these are errors in dependencies,
                        // the same error might show up multiple times,
                        // once in each package that depends on it.
                        // Only print each once.
                        if (!printed[err])
                        {
                            printed[err] = true;
                            @base.Errorf("%s", err);
                        }
                    }
                }

                pkg = pkg__prev1;
            }

            @base.ExitIfErrors(); 

            // Check for duplicate loads of the same package.
            // That should be impossible, but if it does happen then
            // we end up trying to build the same package twice,
            // usually in parallel overwriting the same files,
            // which doesn't work very well.
            map seen = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
            map reported = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
            {
                var pkg__prev1 = pkg;

                foreach (var (_, __pkg) in PackageList(pkgs))
                {
                    pkg = __pkg;
                    if (seen[pkg.ImportPath] && !reported[pkg.ImportPath])
                    {
                        reported[pkg.ImportPath] = true;
                        @base.Errorf("internal error: duplicate loads of %s", pkg.ImportPath);
                    }
                    seen[pkg.ImportPath] = true;
                }

                pkg = pkg__prev1;
            }

            @base.ExitIfErrors();

            return pkgs;
        }

        // GoFilesPackage creates a package for building a collection of Go files
        // (typically named on the command line). The target is named p.a for
        // package p or named after the first Go file for package main.
        public static ref Package GoFilesPackage(slice<@string> gofiles)
        { 
            // TODO: Remove this restriction.
            foreach (var (_, f) in gofiles)
            {
                if (!strings.HasSuffix(f, ".go"))
                {
                    @base.Fatalf("named files must be .go files");
                }
            }
            ImportStack stk = default;
            var ctxt = cfg.BuildContext;
            ctxt.UseAllFiles = true; 

            // Synthesize fake "directory" that only shows the named files,
            // to make it look like this is a standard package or
            // command directory. So that local imports resolve
            // consistently, the files must all be in the same directory.
            slice<os.FileInfo> dirent = default;
            @string dir = default;
            foreach (var (_, file) in gofiles)
            {
                var (fi, err) = os.Stat(file);
                if (err != null)
                {
                    @base.Fatalf("%s", err);
                }
                if (fi.IsDir())
                {
                    @base.Fatalf("%s is a directory, should be a Go file", file);
                }
                var (dir1, _) = filepath.Split(file);
                if (dir1 == "")
                {
                    dir1 = "./";
                }
                if (dir == "")
                {
                    dir = dir1;
                }
                else if (dir != dir1)
                {
                    @base.Fatalf("named files must all be in one directory; have %s and %s", dir, dir1);
                }
                dirent = append(dirent, fi);
            }
            ctxt.ReadDir = _p0 => (dirent, null);

            error err = default;
            if (dir == "")
            {
                dir = @base.Cwd;
            }
            dir, err = filepath.Abs(dir);
            if (err != null)
            {
                @base.Fatalf("%s", err);
            }
            var (bp, err) = ctxt.ImportDir(dir, 0L);
            ptr<Package> pkg = @new<Package>();
            pkg.Internal.Local = true;
            pkg.Internal.CmdlineFiles = true;
            stk.Push("main");
            pkg.load(ref stk, bp, err);
            stk.Pop();
            pkg.Internal.LocalPrefix = dirToImportPath(dir);
            pkg.ImportPath = "command-line-arguments";
            pkg.Target = "";

            if (pkg.Name == "main")
            {
                var (_, elem) = filepath.Split(gofiles[0L]);
                var exe = elem[..len(elem) - len(".go")] + cfg.ExeSuffix;
                if (cfg.BuildO == "")
                {
                    cfg.BuildO = exe;
                }
                if (cfg.GOBIN != "")
                {
                    pkg.Target = filepath.Join(cfg.GOBIN, exe);
                }
            }
            return pkg;
        }

        // TestPackagesFor returns package structs ptest, the package p plus
        // its test files, and pxtest, the external tests of package p.
        // pxtest may be nil. If there are no test files, forceTest decides
        // whether this returns a new package struct or just returns p.
        public static (ref Package, ref Package, error) TestPackagesFor(ref Package p, bool forceTest)
        {
            slice<ref Package> imports = default;            slice<ref Package> ximports = default;

            ImportStack stk = default;
            stk.Push(p.ImportPath + " (test)");
            var rawTestImports = str.StringList(p.TestImports);
            {
                var i__prev1 = i;
                var path__prev1 = path;

                foreach (var (__i, __path) in p.TestImports)
                {
                    i = __i;
                    path = __path;
                    var p1 = LoadImport(path, p.Dir, p, ref stk, p.Internal.Build.TestImportPos[path], UseVendor);
                    if (p1.Error != null)
                    {
                        return (null, null, p1.Error);
                    }
                    if (len(p1.DepsErrors) > 0L)
                    {
                        var err = p1.DepsErrors[0L];
                        err.Pos = ""; // show full import stack
                        return (null, null, err);
                    }
                    if (str.Contains(p1.Deps, p.ImportPath) || p1.ImportPath == p.ImportPath)
                    { 
                        // Same error that loadPackage returns (via reusePackage) in pkg.go.
                        // Can't change that code, because that code is only for loading the
                        // non-test copy of a package.
                        err = ref new PackageError(ImportStack:testImportStack(stk[0],p1,p.ImportPath),Err:"import cycle not allowed in test",IsImportCycle:true,);
                        return (null, null, err);
                    }
                    p.TestImports[i] = p1.ImportPath;
                    imports = append(imports, p1);
                }

                i = i__prev1;
                path = path__prev1;
            }

            stk.Pop();
            stk.Push(p.ImportPath + "_test");
            var pxtestNeedsPtest = false;
            var rawXTestImports = str.StringList(p.XTestImports);
            {
                var i__prev1 = i;
                var path__prev1 = path;

                foreach (var (__i, __path) in p.XTestImports)
                {
                    i = __i;
                    path = __path;
                    p1 = LoadImport(path, p.Dir, p, ref stk, p.Internal.Build.XTestImportPos[path], UseVendor);
                    if (p1.Error != null)
                    {
                        return (null, null, p1.Error);
                    }
                    if (len(p1.DepsErrors) > 0L)
                    {
                        err = p1.DepsErrors[0L];
                        err.Pos = ""; // show full import stack
                        return (null, null, err);
                    }
                    if (p1.ImportPath == p.ImportPath)
                    {
                        pxtestNeedsPtest = true;
                    }
                    else
                    {
                        ximports = append(ximports, p1);
                    }
                    p.XTestImports[i] = p1.ImportPath;
                }

                i = i__prev1;
                path = path__prev1;
            }

            stk.Pop(); 

            // Test package.
            if (len(p.TestGoFiles) > 0L || forceTest)
            {
                ptest = @new<Package>();
                ptest.Value = p.Value;
                ptest.GoFiles = null;
                ptest.GoFiles = append(ptest.GoFiles, p.GoFiles);
                ptest.GoFiles = append(ptest.GoFiles, p.TestGoFiles);
                ptest.Target = ""; 
                // Note: The preparation of the vet config requires that common
                // indexes in ptest.Imports, ptest.Internal.Imports, and ptest.Internal.RawImports
                // all line up (but RawImports can be shorter than the others).
                // That is, for 0 ≤ i < len(RawImports),
                // RawImports[i] is the import string in the program text,
                // Imports[i] is the expanded import string (vendoring applied or relative path expanded away),
                // and Internal.Imports[i] is the corresponding *Package.
                // Any implicitly added imports appear in Imports and Internal.Imports
                // but not RawImports (because they were not in the source code).
                // We insert TestImports, imports, and rawTestImports at the start of
                // these lists to preserve the alignment.
                ptest.Imports = str.StringList(p.TestImports, p.Imports);
                ptest.Internal.Imports = append(imports, p.Internal.Imports);
                ptest.Internal.RawImports = str.StringList(rawTestImports, p.Internal.RawImports);
                ptest.Internal.ForceLibrary = true;
                ptest.Internal.Build = @new<build.Package>();
                ptest.Internal.Build.Value = p.Internal.Build.Value;
                map m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<token.Position>>{};
                {
                    var k__prev1 = k;
                    var v__prev1 = v;

                    foreach (var (__k, __v) in p.Internal.Build.ImportPos)
                    {
                        k = __k;
                        v = __v;
                        m[k] = append(m[k], v);
                    }
            else

                    k = k__prev1;
                    v = v__prev1;
                }

                {
                    var k__prev1 = k;
                    var v__prev1 = v;

                    foreach (var (__k, __v) in p.Internal.Build.TestImportPos)
                    {
                        k = __k;
                        v = __v;
                        m[k] = append(m[k], v);
                    }

                    k = k__prev1;
                    v = v__prev1;
                }

                ptest.Internal.Build.ImportPos = m;
            }            {
                ptest = p;
            } 

            // External test package.
            if (len(p.XTestGoFiles) > 0L)
            {
                pxtest = ref new Package(PackagePublic:PackagePublic{Name:p.Name+"_test",ImportPath:p.ImportPath+"_test",Root:p.Root,Dir:p.Dir,GoFiles:p.XTestGoFiles,Imports:p.XTestImports,},Internal:PackageInternal{LocalPrefix:p.Internal.LocalPrefix,Build:&build.Package{ImportPos:p.Internal.Build.XTestImportPos,},Imports:ximports,RawImports:rawXTestImports,Asmflags:p.Internal.Asmflags,Gcflags:p.Internal.Gcflags,Ldflags:p.Internal.Ldflags,Gccgoflags:p.Internal.Gccgoflags,},);
                if (pxtestNeedsPtest)
                {
                    pxtest.Internal.Imports = append(pxtest.Internal.Imports, ptest);
                }
            }
            return (ptest, pxtest, null);
        }

        private static slice<@string> testImportStack(@string top, ref Package p, @string target)
        {
            @string stk = new slice<@string>(new @string[] { top, p.ImportPath });
Search:
            while (p.ImportPath != target)
            {
                foreach (var (_, p1) in p.Internal.Imports)
                {
                    if (p1.ImportPath == target || str.Contains(p1.Deps, target))
                    {
                        stk = append(stk, p1.ImportPath);
                        p = p1;
                        _continueSearch = true;
                        break;
                    }
                } 
                // Can't happen, but in case it does...
                stk = append(stk, "<lost path to cycle>");
                break;
            }
            return stk;
        }
    }
}}}}
