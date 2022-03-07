// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package load loads packages.
// package load -- go2cs converted at 2022 March 06 23:17:12 UTC
// import "cmd/go/internal/load" ==> using load = go.cmd.go.@internal.load_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\load\pkg.go
using bytes = go.bytes_package;
using context = go.context_package;
using json = go.encoding.json_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using scanner = go.go.scanner_package;
using token = go.go.token_package;
using goroot = go.@internal.goroot_package;
using fs = go.io.fs_package;
using os = go.os_package;
using path = go.path_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using fsys = go.cmd.go.@internal.fsys_package;
using imports = go.cmd.go.@internal.imports_package;
using modfetch = go.cmd.go.@internal.modfetch_package;
using modinfo = go.cmd.go.@internal.modinfo_package;
using modload = go.cmd.go.@internal.modload_package;
using par = go.cmd.go.@internal.par_package;
using search = go.cmd.go.@internal.search_package;
using str = go.cmd.go.@internal.str_package;
using trace = go.cmd.go.@internal.trace_package;
using sys = go.cmd.@internal.sys_package;

using modfile = go.golang.org.x.mod.modfile_package;
using module = go.golang.org.x.mod.module_package;
using System.ComponentModel;
using System;
using System.Threading;


namespace go.cmd.go.@internal;

public static partial class load_package {

    // A Package describes a single package found in a directory.
public partial struct Package {
    public ref PackagePublic PackagePublic => ref PackagePublic_val; // visible in 'go list'
    public PackageInternal Internal; // for use inside go command only
}

public partial struct PackagePublic {
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
    public @string Root; // Go root, Go path dir, or module root dir containing this package
    [Description("json:\",omitempty\"")]
    public @string ConflictDir; // Dir is hidden by this other directory
    [Description("json:\",omitempty\"")]
    public @string ForTest; // package is only for use in named test
    [Description("json:\",omitempty\"")]
    public @string Export; // file containing export data (set by go list -export)
    [Description("json:\",omitempty\"")]
    public @string BuildID; // build ID of the compiled package (set by go list -export)
    [Description("json:\",omitempty\"")]
    public ptr<modinfo.ModulePublic> Module; // info about package's module, if any
    [Description("json:\",omitempty\"")]
    public slice<@string> Match; // command-line patterns matching this package
    [Description("json:\",omitempty\"")]
    public bool Goroot; // is this package found in the Go root?
    [Description("json:\",omitempty\"")]
    public bool Standard; // is this package part of the standard Go library?
    [Description("json:\",omitempty\"")]
    public bool DepOnly; // package is only as a dependency, not explicitly listed
    [Description("json:\",omitempty\"")]
    public bool BinaryOnly; // package cannot be recompiled
    [Description("json:\",omitempty\"")]
    public bool Incomplete; // was there an error loading this package or dependencies?

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
    public slice<@string> CgoFiles; // .go source files that import "C"
    [Description("json:\",omitempty\"")]
    public slice<@string> CompiledGoFiles; // .go output from running cgo on CgoFiles
    [Description("json:\",omitempty\"")]
    public slice<@string> IgnoredGoFiles; // .go source files ignored due to build constraints
    [Description("json:\",omitempty\"")]
    public slice<@string> InvalidGoFiles; // .go source files with detected problems (parse error, wrong package name, and so on)
    [Description("json:\",omitempty\"")]
    public slice<@string> IgnoredOtherFiles; // non-.go source files ignored due to build constraints
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

// Embedded files
    [Description("json:\",omitempty\"")]
    public slice<@string> EmbedPatterns; // //go:embed patterns
    [Description("json:\",omitempty\"")]
    public slice<@string> EmbedFiles; // files matched by EmbedPatterns

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
    public map<@string, @string> ImportMap; // map from source import to ImportPath (identity entries omitted)
    [Description("json:\",omitempty\"")]
    public slice<@string> Deps; // all (recursively) imported dependencies

// Error information
// Incomplete is above, packed into the other bools
    [Description("json:\",omitempty\"")]
    public ptr<PackageError> Error; // error loading this package (not dependencies)
    [Description("json:\",omitempty\"")]
    public slice<ptr<PackageError>> DepsErrors; // errors loading dependencies

// Test information
// If you add to this list you MUST add to p.AllFiles (below) too.
// Otherwise file name security lists will not apply to any new additions.
    [Description("json:\",omitempty\"")]
    public slice<@string> TestGoFiles; // _test.go files in package
    [Description("json:\",omitempty\"")]
    public slice<@string> TestImports; // imports from TestGoFiles
    [Description("json:\",omitempty\"")]
    public slice<@string> TestEmbedPatterns; // //go:embed patterns
    [Description("json:\",omitempty\"")]
    public slice<@string> TestEmbedFiles; // files matched by TestEmbedPatterns
    [Description("json:\",omitempty\"")]
    public slice<@string> XTestGoFiles; // _test.go files outside package
    [Description("json:\",omitempty\"")]
    public slice<@string> XTestImports; // imports from XTestGoFiles
    [Description("json:\",omitempty\"")]
    public slice<@string> XTestEmbedPatterns; // //go:embed patterns
    [Description("json:\",omitempty\"")]
    public slice<@string> XTestEmbedFiles; // files matched by XTestEmbedPatterns
}

// AllFiles returns the names of all the files considered for the package.
// This is used for sanity and security checks, so we include all files,
// even IgnoredGoFiles, because some subcommands consider them.
// The go/build package filtered others out (like foo_wrongGOARCH.s)
// and that's OK.
private static slice<@string> AllFiles(this ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;

    var files = str.StringList(p.GoFiles, p.CgoFiles, p.IgnoredGoFiles, p.IgnoredOtherFiles, p.CFiles, p.CXXFiles, p.MFiles, p.HFiles, p.FFiles, p.SFiles, p.SwigFiles, p.SwigCXXFiles, p.SysoFiles, p.TestGoFiles, p.XTestGoFiles); 

    // EmbedFiles may overlap with the other files.
    // Dedup, but delay building the map as long as possible.
    // Only files in the current directory (no slash in name)
    // need to be checked against the files variable above.
    map<@string, bool> have = default;
    {
        var file__prev1 = file;

        foreach (var (_, __file) in p.EmbedFiles) {
            file = __file;
            if (!strings.Contains(file, "/")) {
                if (have == null) {
                    have = make_map<@string, bool>();
                    {
                        var file__prev2 = file;

                        foreach (var (_, __file) in files) {
                            file = __file;
                            have[file] = true;
                        }

                        file = file__prev2;
                    }
                }

                if (have[file]) {
                    continue;
                }

            }

            files = append(files, file);

        }
        file = file__prev1;
    }

    return files;

}

// Desc returns the package "description", for use in b.showOutput.
private static @string Desc(this ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;

    if (p.ForTest != "") {
        return p.ImportPath + " [" + p.ForTest + ".test]";
    }
    return p.ImportPath;

}

public partial struct PackageInternal {
    public ptr<build.Package> Build;
    public slice<ptr<Package>> Imports; // this package's direct imports
    public slice<@string> CompiledImports; // additional Imports necessary when using CompiledGoFiles (all from standard library); 1:1 with the end of PackagePublic.Imports
    public slice<@string> RawImports; // this package's original imports as they appear in the text of the program; 1:1 with the end of PackagePublic.Imports
    public bool ForceLibrary; // this package is a library (even if named "main")
    public bool CmdlineFiles; // package built from files listed on command line
    public bool CmdlinePkg; // package listed on command line
    public bool CmdlinePkgLiteral; // package listed as literal on command line (not via wildcard)
    public bool Local; // imported via local path (./ or ../)
    public @string LocalPrefix; // interpret ./ and ../ imports relative to this prefix
    public @string ExeName; // desired name for temporary executable
    public @string CoverMode; // preprocess Go source files with the coverage tool in this mode
    public map<@string, ptr<CoverVar>> CoverVars; // variables created by coverage analysis
    public bool OmitDebug; // tell linker not to write debug information
    public bool GobinSubdir; // install target would be subdir of GOBIN
    public @string BuildInfo; // add this info to package main
    public ptr<slice<byte>> TestmainGo; // content for _testmain.go
    public map<@string, slice<@string>> Embed; // //go:embed comment mapping
    public @string OrigImportPath; // original import path before adding '_test' suffix

    public slice<@string> Asmflags; // -asmflags for this package
    public slice<@string> Gcflags; // -gcflags for this package
    public slice<@string> Ldflags; // -ldflags for this package
    public slice<@string> Gccgoflags; // -gccgoflags for this package
}

// A NoGoError indicates that no Go files for the package were applicable to the
// build for that package.
//
// That may be because there were no files whatsoever, or because all files were
// excluded, or because all non-excluded files were test sources.
public partial struct NoGoError {
    public ptr<Package> Package;
}

private static @string Error(this ptr<NoGoError> _addr_e) {
    ref NoGoError e = ref _addr_e.val;

    if (len(e.Package.IgnoredGoFiles) > 0) { 
        // Go files exist, but they were ignored due to build constraints.
        return "build constraints exclude all Go files in " + e.Package.Dir;

    }
    if (len(e.Package.TestGoFiles) + len(e.Package.XTestGoFiles) > 0) { 
        // Test Go files exist, but we're not interested in them.
        // The double-negative is unfortunate but we want e.Package.Dir
        // to appear at the end of error message.
        return "no non-test Go files in " + e.Package.Dir;

    }
    return "no Go files in " + e.Package.Dir;

}

// setLoadPackageDataError presents an error found when loading package data
// as a *PackageError. It has special cases for some common errors to improve
// messages shown to users and reduce redundancy.
//
// setLoadPackageDataError returns true if it's safe to load information about
// imported packages, for example, if there was a parse error loading imports
// in one file, but other files are okay.
private static void setLoadPackageDataError(this ptr<Package> _addr_p, error err, @string path, ptr<ImportStack> _addr_stk, slice<token.Position> importPos) => func((defer, _, _) => {
    ref Package p = ref _addr_p.val;
    ref ImportStack stk = ref _addr_stk.val;

    ptr<search.MatchError> (matchErr, isMatchErr) = err._<ptr<search.MatchError>>();
    if (isMatchErr && matchErr.Match.Pattern() == path) {
        if (matchErr.Match.IsLiteral()) { 
            // The error has a pattern has a pattern similar to the import path.
            // It may be slightly different (./foo matching example.com/foo),
            // but close enough to seem redundant.
            // Unwrap the error so we don't show the pattern.
            err = matchErr.Err;

        }
    }
    ptr<build.NoGoError> nogoErr;
    if (errors.As(err, _addr_nogoErr)) {
        if (p.Dir == "" && nogoErr.Dir != "") {
            p.Dir = nogoErr.Dir;
        }
        err = addr(new NoGoError(Package:p));

    }
    @string pos = default;
    bool isScanErr = default;
    {
        scanner.ErrorList (scanErr, ok) = err._<scanner.ErrorList>();

        if (ok && len(scanErr) > 0) {
            isScanErr = true; // For stack push/pop below.

            var scanPos = scanErr[0].Pos;
            scanPos.Filename = @base.ShortPath(scanPos.Filename);
            pos = scanPos.String();
            err = errors.New(scanErr[0].Msg);

        }
    } 

    // Report the error on the importing package if the problem is with the import declaration
    // for example, if the package doesn't exist or if the import path is malformed.
    // On the other hand, don't include a position if the problem is with the imported package,
    // for example there are no Go files (NoGoError), or there's a problem in the imported
    // package's source files themselves (scanner errors).
    //
    // TODO(matloob): Perhaps make each of those the errors in the first group
    // (including modload.ImportMissingError, ImportMissingSumError, and the
    // corresponding "cannot find package %q in any of" GOPATH-mode error
    // produced in build.(*Context).Import; modload.AmbiguousImportError,
    // and modload.PackageNotInModuleError; and the malformed module path errors
    // produced in golang.org/x/mod/module.CheckMod) implement an interface
    // to make it easier to check for them? That would save us from having to
    // move the modload errors into this package to avoid a package import cycle,
    // and from having to export an error type for the errors produced in build.
    if (!isMatchErr && (nogoErr != null || isScanErr)) {
        stk.Push(path);
        defer(stk.Pop());
    }
    p.Error = addr(new PackageError(ImportStack:stk.Copy(),Pos:pos,Err:err,));

    if (path != stk.Top()) {
        p.Error.setPos(importPos);
    }
});

// Resolve returns the resolved version of imports,
// which should be p.TestImports or p.XTestImports, NOT p.Imports.
// The imports in p.TestImports and p.XTestImports are not recursively
// loaded during the initial load of p, so they list the imports found in
// the source file, but most processing should be over the vendor-resolved
// import paths. We do this resolution lazily both to avoid file system work
// and because the eventual real load of the test imports (during 'go test')
// can produce better error messages if it starts with the original paths.
// The initial load of p loads all the non-test imports and rewrites
// the vendored paths, so nothing should ever call p.vendored(p.Imports).
private static slice<@string> Resolve(this ptr<Package> _addr_p, slice<@string> imports) => func((_, panic, _) => {
    ref Package p = ref _addr_p.val;

    if (len(imports) > 0 && len(p.Imports) > 0 && _addr_imports[0] == _addr_p.Imports[0]) {
        panic("internal error: p.Resolve(p.Imports) called");
    }
    var seen = make_map<@string, bool>();
    slice<@string> all = default;
    foreach (var (_, path) in imports) {
        path = ResolveImportPath(_addr_p, path);
        if (!seen[path]) {
            seen[path] = true;
            all = append(all, path);
        }
    }    sort.Strings(all);
    return all;

});

// CoverVar holds the name of the generated coverage variables targeting the named file.
public partial struct CoverVar {
    public @string File; // local file name
    public @string Var; // name of count struct
}

private static void copyBuild(this ptr<Package> _addr_p, PackageOpts opts, ptr<build.Package> _addr_pp) {
    ref Package p = ref _addr_p.val;
    ref build.Package pp = ref _addr_pp.val;

    p.Internal.Build = pp;

    if (pp.PkgTargetRoot != "" && cfg.BuildPkgdir != "") {
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
    p.Standard = p.Goroot && p.ImportPath != "" && search.IsStandardImportPath(p.ImportPath);
    p.GoFiles = pp.GoFiles;
    p.CgoFiles = pp.CgoFiles;
    p.IgnoredGoFiles = pp.IgnoredGoFiles;
    p.InvalidGoFiles = pp.InvalidGoFiles;
    p.IgnoredOtherFiles = pp.IgnoredOtherFiles;
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
    if (opts.IgnoreImports) {
        p.Imports = null;
        p.Internal.RawImports = null;
        p.TestImports = null;
        p.XTestImports = null;
    }
    p.EmbedPatterns = pp.EmbedPatterns;
    p.TestEmbedPatterns = pp.TestEmbedPatterns;
    p.XTestEmbedPatterns = pp.XTestEmbedPatterns;
    p.Internal.OrigImportPath = pp.ImportPath;

}

// A PackageError describes an error loading information about a package.
public partial struct PackageError {
    public slice<@string> ImportStack; // shortest path from package named on command line to this one
    public @string Pos; // position of error
    public error Err; // the error itself
    public bool IsImportCycle; // the error is an import cycle
    public bool Hard; // whether the error is soft or hard; soft errors are ignored in some places
    public bool alwaysPrintStack; // whether to always print the ImportStack
}

private static @string Error(this ptr<PackageError> _addr_p) {
    ref PackageError p = ref _addr_p.val;
 
    // TODO(#43696): decide when to print the stack or the position based on
    // the error type and whether the package is in the main module.
    // Document the rationale.
    if (p.Pos != "" && (len(p.ImportStack) == 0 || !p.alwaysPrintStack)) { 
        // Omit import stack. The full path to the file where the error
        // is the most important thing.
        return p.Pos + ": " + p.Err.Error();

    }
    if (len(p.ImportStack) == 0) {
        return p.Err.Error();
    }
    @string optpos = default;
    if (p.Pos != "") {
        optpos = "\n\t" + p.Pos;
    }
    return "package " + strings.Join(p.ImportStack, "\n\timports ") + optpos + ": " + p.Err.Error();

}

private static error Unwrap(this ptr<PackageError> _addr_p) {
    ref PackageError p = ref _addr_p.val;

    return error.As(p.Err)!;
}

// PackageError implements MarshalJSON so that Err is marshaled as a string
// and non-essential fields are omitted.
private static (slice<byte>, error) MarshalJSON(this ptr<PackageError> _addr_p) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref PackageError p = ref _addr_p.val;

    struct{ImportStack[]stringPosstringErrstring} perr = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{ImportStack[]stringPosstringErrstring}{p.ImportStack,p.Pos,p.Err.Error()};
    return json.Marshal(perr);
}

private static void setPos(this ptr<PackageError> _addr_p, slice<token.Position> posList) {
    ref PackageError p = ref _addr_p.val;

    if (len(posList) == 0) {
        return ;
    }
    var pos = posList[0];
    pos.Filename = @base.ShortPath(pos.Filename);
    p.Pos = pos.String();

}

// ImportPathError is a type of error that prevents a package from being loaded
// for a given import path. When such a package is loaded, a *Package is
// returned with Err wrapping an ImportPathError: the error is attached to
// the imported package, not the importing package.
//
// The string returned by ImportPath must appear in the string returned by
// Error. Errors that wrap ImportPathError (such as PackageError) may omit
// the import path.
public partial interface ImportPathError {
    @string ImportPath();
}

private static ImportPathError _ = ImportPathError.As((importError.val)(null))!;private static ImportPathError _ = ImportPathError.As((mainPackageError.val)(null))!;private static ImportPathError _ = ImportPathError.As((modload.ImportMissingError.val)(null))!;private static ImportPathError _ = ImportPathError.As((modload.ImportMissingSumError.val)(null))!;private static ImportPathError _ = ImportPathError.As((modload.DirectImportFromImplicitDependencyError.val)(null))!;

private partial struct importError {
    public @string importPath;
    public error err; // created with fmt.Errorf
}

public static ImportPathError ImportErrorf(@string path, @string format, params object[] args) => func((_, panic, _) => {
    args = args.Clone();

    ptr<importError> err = addr(new importError(importPath:path,err:fmt.Errorf(format,args...)));
    {
        var errStr = err.Error();

        if (!strings.Contains(errStr, path)) {
            panic(fmt.Sprintf("path %q not in error %q", path, errStr));
        }
    }

    return err;

});

private static @string Error(this ptr<importError> _addr_e) {
    ref importError e = ref _addr_e.val;

    return e.err.Error();
}

private static error Unwrap(this ptr<importError> _addr_e) {
    ref importError e = ref _addr_e.val;
 
    // Don't return e.err directly, since we're only wrapping an error if %w
    // was passed to ImportErrorf.
    return error.As(errors.Unwrap(e.err))!;

}

private static @string ImportPath(this ptr<importError> _addr_e) {
    ref importError e = ref _addr_e.val;

    return e.importPath;
}

// An ImportStack is a stack of import paths, possibly with the suffix " (test)" appended.
// The import path of a test package is the import path of the corresponding
// non-test package with the suffix "_test" added.
public partial struct ImportStack { // : slice<@string>
}

private static void Push(this ptr<ImportStack> _addr_s, @string p) {
    ref ImportStack s = ref _addr_s.val;

    s.val = append(s.val, p);
}

private static void Pop(this ptr<ImportStack> _addr_s) {
    ref ImportStack s = ref _addr_s.val;

    s.val = (s.val)[(int)0..(int)len(s.val) - 1];
}

private static slice<@string> Copy(this ptr<ImportStack> _addr_s) {
    ref ImportStack s = ref _addr_s.val;

    return append(new slice<@string>(new @string[] {  }), s.val);
}

private static @string Top(this ptr<ImportStack> _addr_s) {
    ref ImportStack s = ref _addr_s.val;

    if (len(s.val) == 0) {
        return "";
    }
    return (s.val)[len(s.val) - 1];

}

// shorterThan reports whether sp is shorter than t.
// We use this to record the shortest import sequence
// that leads to a particular package.
private static bool shorterThan(this ptr<ImportStack> _addr_sp, slice<@string> t) {
    ref ImportStack sp = ref _addr_sp.val;

    var s = sp.val;
    if (len(s) != len(t)) {
        return len(s) < len(t);
    }
    foreach (var (i) in s) {
        if (s[i] != t[i]) {
            return s[i] < t[i];
        }
    }    return false; // they are equal
}

// packageCache is a lookup cache for LoadImport,
// so that if we look up a package multiple times
// we return the same pointer each time.
private static map packageCache = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ptr<Package>>{};

// ClearPackageCache clears the in-memory package cache and the preload caches.
// It is only for use by GOPATH-based "go get".
// TODO(jayconrod): When GOPATH-based "go get" is removed, delete this function.
public static void ClearPackageCache() {
    foreach (var (name) in packageCache) {
        delete(packageCache, name);
    }    resolvedImportCache.Clear();
    packageDataCache.Clear();
}

// ClearPackageCachePartial clears packages with the given import paths from the
// in-memory package cache and the preload caches. It is only for use by
// GOPATH-based "go get".
// TODO(jayconrod): When GOPATH-based "go get" is removed, delete this function.
public static void ClearPackageCachePartial(slice<@string> args) {
    var shouldDelete = make_map<@string, bool>();
    foreach (var (_, arg) in args) {
        shouldDelete[arg] = true;
        {
            var p = packageCache[arg];

            if (p != null) {
                delete(packageCache, arg);
            }

        }

    }    resolvedImportCache.DeleteIf(key => {
        return shouldDelete[key._<importSpec>().path];
    });
    packageDataCache.DeleteIf(key => {
        return shouldDelete[key._<@string>()];
    });

}

// ReloadPackageNoFlags is like LoadImport but makes sure
// not to use the package cache.
// It is only for use by GOPATH-based "go get".
// TODO(rsc): When GOPATH-based "go get" is removed, delete this function.
public static ptr<Package> ReloadPackageNoFlags(@string arg, ptr<ImportStack> _addr_stk) {
    ref ImportStack stk = ref _addr_stk.val;

    var p = packageCache[arg];
    if (p != null) {
        delete(packageCache, arg);
        resolvedImportCache.DeleteIf(key => {
            return _addr_key._<importSpec>().path == p.ImportPath!;
        });
        packageDataCache.Delete(p.ImportPath);
    }
    return _addr_LoadImport(context.TODO(), new PackageOpts(), arg, @base.Cwd(), _addr_null, _addr_stk, null, 0)!;

}

// dirToImportPath returns the pseudo-import path we use for a package
// outside the Go path. It begins with _/ and then contains the full path
// to the directory. If the package lives in c:\home\gopher\my\pkg then
// the pseudo-import path is _/c_/home/gopher/my/pkg.
// Using a pseudo-import path like this makes the ./ imports no longer
// a special case, so that all the code to deal with ordinary imports works
// automatically.
private static @string dirToImportPath(@string dir) {
    return pathpkg.Join("_", strings.Map(makeImportValid, filepath.ToSlash(dir)));
}

private static int makeImportValid(int r) { 
    // Should match Go spec, compilers, and ../../go/parser/parser.go:/isValidImport.
    const @string illegalChars = "!\"#$%&\'()*,:;<=>?[\\]^{|}" + "`\uFFFD";

    if (!unicode.IsGraphic(r) || unicode.IsSpace(r) || strings.ContainsRune(illegalChars, r)) {
        return '_';
    }
    return r;

}

// Mode flags for loadImport and download (in get.go).
 
// ResolveImport means that loadImport should do import path expansion.
// That is, ResolveImport means that the import path came from
// a source file and has not been expanded yet to account for
// vendoring or possible module adjustment.
// Every import path should be loaded initially with ResolveImport,
// and then the expanded version (for example with the /vendor/ in it)
// gets recorded as the canonical import path. At that point, future loads
// of that package must not pass ResolveImport, because
// disallowVendor will reject direct use of paths containing /vendor/.
public static readonly nint ResolveImport = 1 << (int)(iota); 

// ResolveModule is for download (part of "go get") and indicates
// that the module adjustment should be done, but not vendor adjustment.
public static readonly var ResolveModule = 0; 

// GetTestDeps is for download (part of "go get") and indicates
// that test dependencies should be fetched too.
public static readonly var GetTestDeps = 1;


// LoadImport scans the directory named by path, which must be an import path,
// but possibly a local import path (an absolute file system path or one beginning
// with ./ or ../). A local relative path is interpreted relative to srcDir.
// It returns a *Package describing the package found in that directory.
// LoadImport does not set tool flags and should only be used by
// this package, as part of a bigger load operation, and by GOPATH-based "go get".
// TODO(rsc): When GOPATH-based "go get" is removed, unexport this function.
public static ptr<Package> LoadImport(context.Context ctx, PackageOpts opts, @string path, @string srcDir, ptr<Package> _addr_parent, ptr<ImportStack> _addr_stk, slice<token.Position> importPos, nint mode) {
    ref Package parent = ref _addr_parent.val;
    ref ImportStack stk = ref _addr_stk.val;

    return _addr_loadImport(ctx, opts, _addr_null, path, srcDir, _addr_parent, _addr_stk, importPos, mode)!;
}

private static ptr<Package> loadImport(context.Context ctx, PackageOpts opts, ptr<preload> _addr_pre, @string path, @string srcDir, ptr<Package> _addr_parent, ptr<ImportStack> _addr_stk, slice<token.Position> importPos, nint mode) => func((defer, panic, _) => {
    ref preload pre = ref _addr_pre.val;
    ref Package parent = ref _addr_parent.val;
    ref ImportStack stk = ref _addr_stk.val;

    if (path == "") {
        panic("LoadImport called with empty package path");
    }
    @string parentPath = default;    @string parentRoot = default;

    var parentIsStd = false;
    if (parent != null) {
        parentPath = parent.ImportPath;
        parentRoot = parent.Root;
        parentIsStd = parent.Standard;
    }
    var (bp, loaded, err) = loadPackageData(ctx, path, parentPath, srcDir, parentRoot, parentIsStd, mode);
    if (loaded && pre != null && !opts.IgnoreImports) {
        pre.preloadImports(ctx, opts, bp.Imports, bp);
    }
    if (bp == null) {
        ptr<Package> p = addr(new Package(PackagePublic:PackagePublic{ImportPath:path,Incomplete:true,},));
        {
            ImportPathError (importErr, ok) = ImportPathError.As(err._<ImportPathError>())!;

            if (!ok || importErr.ImportPath() != path) { 
                // Only add path to the error's import stack if it's not already present
                // in the error.
                //
                // TODO(bcmills): setLoadPackageDataError itself has a similar Push / Pop
                // sequence that empirically doesn't trigger for these errors, guarded by
                // a somewhat complex condition. Figure out how to generalize that
                // condition and eliminate the explicit calls here.
                stk.Push(path);
                defer(stk.Pop());

            }

        }

        p.setLoadPackageDataError(err, path, stk, null);
        return _addr_p!;

    }
    var importPath = bp.ImportPath;
    p = packageCache[importPath];
    if (p != null) {
        stk.Push(path);
        p = reusePackage(p, _addr_stk);
        stk.Pop();
    }
    else
 {
        p = @new<Package>();
        p.Internal.Local = build.IsLocalImport(path);
        p.ImportPath = importPath;
        packageCache[importPath] = p; 

        // Load package.
        // loadPackageData may return bp != nil even if an error occurs,
        // in order to return partial information.
        p.load(ctx, opts, path, stk, importPos, bp, err);

        if (!cfg.ModulesEnabled && path != cleanImport(path)) {
            p.Error = addr(new PackageError(ImportStack:stk.Copy(),Err:ImportErrorf(path,"non-canonical import path %q: should be %q",path,pathpkg.Clean(path)),));
            p.Incomplete = true;
            p.Error.setPos(importPos);
        }
    }
    {
        var perr__prev1 = perr;

        ref var perr = ref heap(disallowInternal(ctx, srcDir, _addr_parent, parentPath, p, _addr_stk), out ptr<var> _addr_perr);

        if (perr != p) {
            perr.Error.setPos(importPos);
            return _addr_perr!;
        }
        perr = perr__prev1;

    }

    if (mode & ResolveImport != 0) {
        {
            var perr__prev2 = perr;

            perr = disallowVendor(srcDir, path, parentPath, p, _addr_stk);

            if (perr != p) {
                perr.Error.setPos(importPos);
                return _addr_perr!;
            }

            perr = perr__prev2;

        }

    }
    if (p.Name == "main" && parent != null && parent.Dir != p.Dir) {
        perr = p.val;
        perr.Error = addr(new PackageError(ImportStack:stk.Copy(),Err:ImportErrorf(path,"import %q is a program, not an importable package",path),));
        perr.Error.setPos(importPos);
        return _addr__addr_perr!;
    }
    if (p.Internal.Local && parent != null && !parent.Internal.Local) {
        perr = p.val;
        error err = default!;
        if (path == ".") {
            err = error.As(ImportErrorf(path, "%s: cannot import current directory", path))!;
        }
        else
 {
            err = error.As(ImportErrorf(path, "local import %q in non-local package", path))!;
        }
        perr.Error = addr(new PackageError(ImportStack:stk.Copy(),Err:err,));
        perr.Error.setPos(importPos);
        return _addr__addr_perr!;

    }
    return _addr_p!;

});

// loadPackageData loads information needed to construct a *Package. The result
// is cached, and later calls to loadPackageData for the same package will return
// the same data.
//
// loadPackageData returns a non-nil package even if err is non-nil unless
// the package path is malformed (for example, the path contains "mod/" or "@").
//
// loadPackageData returns a boolean, loaded, which is true if this is the
// first time the package was loaded. Callers may preload imports in this case.
private static (ptr<build.Package>, bool, error) loadPackageData(context.Context ctx, @string path, @string parentPath, @string parentDir, @string parentRoot, bool parentIsStd, nint mode) => func((_, panic, _) => {
    ptr<build.Package> bp = default!;
    bool loaded = default;
    error err = default!;

    if (path == "") {
        panic("loadPackageData called with empty package path");
    }
    if (strings.HasPrefix(path, "mod/")) { 
        // Paths beginning with "mod/" might accidentally
        // look in the module cache directory tree in $GOPATH/pkg/mod/.
        // This prefix is owned by the Go core for possible use in the
        // standard library (since it does not begin with a domain name),
        // so it's OK to disallow entirely.
        return (_addr_null!, false, error.As(fmt.Errorf("disallowed import path %q", path))!);

    }
    if (strings.Contains(path, "@")) {
        return (_addr_null!, false, error.As(errors.New("can only use path@version syntax with 'go get' and 'go install' in module-aware mode"))!);
    }
    importSpec importKey = new importSpec(path:path,parentPath:parentPath,parentDir:parentDir,parentRoot:parentRoot,parentIsStd:parentIsStd,mode:mode,);
    r = resolvedImportCache.Do(importKey, () => {
        resolvedImport r = default;
        if (build.IsLocalImport(path)) {
            r.dir = filepath.Join(parentDir, path);
            r.path = dirToImportPath(r.dir);
        }
        else if (cfg.ModulesEnabled) {
            r.dir, r.path, r.err = modload.Lookup(parentPath, parentIsStd, path);
        }
        else if (mode & ResolveImport != 0) { 
            // We do our own path resolution, because we want to
            // find out the key to use in packageCache without the
            // overhead of repeated calls to buildContext.Import.
            // The code is also needed in a few other places anyway.
            r.path = resolveImportPath(path, parentPath, parentDir, parentRoot, parentIsStd);

        }
        else if (mode & ResolveModule != 0) {
            r.path = moduleImportPath(path, parentPath, parentDir, parentRoot);
        }
        if (r.path == "") {
            r.path = path;
        }
        return _addr_r!;

    })._<resolvedImport>(); 
    // Invariant: r.path is set to the resolved import path. If the path cannot
    // be resolved, r.path is set to path, the source import path.
    // r.path is never empty.

    // Load the package from its directory. If we already found the package's
    // directory when resolving its import path, use that.
    data = packageDataCache.Do(r.path, () => {
        loaded = true;
        packageData data = default;
        if (r.dir != "") {
            build.ImportMode buildMode = default;
            if (!cfg.ModulesEnabled) {
                buildMode = build.ImportComment;
            }
            data.p, data.err = cfg.BuildContext.ImportDir(r.dir, buildMode);
            if (cfg.ModulesEnabled) { 
                // Override data.p.Root, since ImportDir sets it to $GOPATH, if
                // the module is inside $GOPATH/src.
                {
                    var info = modload.PackageModuleInfo(ctx, path);

                    if (info != null) {
                        data.p.Root = info.Dir;
                    }

                }

            }

            if (r.err != null) {
                if (data.err != null) { 
                    // ImportDir gave us one error, and the module loader gave us another.
                    // We arbitrarily choose to keep the error from ImportDir because
                    // that's what our tests already expect, and it seems to provide a bit
                    // more detail in most cases.
                }
                else if (errors.Is(r.err, imports.ErrNoGo)) { 
                    // ImportDir said there were files in the package, but the module
                    // loader said there weren't. Which one is right?
                    // Without this special-case hack, the TestScript/test_vet case fails
                    // on the vetfail/p1 package (added in CL 83955).
                    // Apparently, imports.ShouldBuild biases toward rejecting files
                    // with invalid build constraints, whereas ImportDir biases toward
                    // accepting them.
                    //
                    // TODO(#41410: Figure out how this actually ought to work and fix
                    // this mess.
                }
                else
 {
                    data.err = r.err;
                }

            }

        }
        else if (r.err != null) {
            data.p = @new<build.Package>();
            data.err = r.err;
        }
        else if (cfg.ModulesEnabled && path != "unsafe") {
            data.p = @new<build.Package>();
            data.err = fmt.Errorf("unknown import path %q: internal error: module loader did not resolve import", r.path);
        }
        else
 {
            buildMode = build.ImportComment;
            if (mode & ResolveImport == 0 || r.path != path) { 
                // Not vendoring, or we already found the vendored path.
                buildMode |= build.IgnoreVendor;

            }

            data.p, data.err = cfg.BuildContext.Import(r.path, parentDir, buildMode);

        }
        data.p.ImportPath = r.path; 

        // Set data.p.BinDir in cases where go/build.Context.Import
        // may give us a path we don't want.
        if (!data.p.Goroot) {
            if (cfg.GOBIN != "") {
                data.p.BinDir = cfg.GOBIN;
            }
            else if (cfg.ModulesEnabled) {
                data.p.BinDir = modload.BinDir();
            }

        }
        if (!cfg.ModulesEnabled && data.err == null && data.p.ImportComment != "" && data.p.ImportComment != path && !strings.Contains(path, "/vendor/") && !strings.HasPrefix(path, "vendor/")) {
            data.err = fmt.Errorf("code in directory %s expects import %q", data.p.Dir, data.p.ImportComment);
        }
        return _addr_data!;

    })._<packageData>();

    return (_addr_data.p!, loaded, error.As(data.err)!);

});

// importSpec describes an import declaration in source code. It is used as a
// cache key for resolvedImportCache.
private partial struct importSpec {
    public @string path;
    public @string parentPath;
    public @string parentDir;
    public @string parentRoot;
    public bool parentIsStd;
    public nint mode;
}

// resolvedImport holds a canonical identifier for a package. It may also contain
// a path to the package's directory and an error if one occurred. resolvedImport
// is the value type in resolvedImportCache.
private partial struct resolvedImport {
    public @string path;
    public @string dir;
    public error err;
}

// packageData holds information loaded from a package. It is the value type
// in packageDataCache.
private partial struct packageData {
    public ptr<build.Package> p;
    public error err;
}

// resolvedImportCache maps import strings (importSpec) to canonical package names
// (resolvedImport).
private static par.Cache resolvedImportCache = default;

// packageDataCache maps canonical package names (string) to package metadata
// (packageData).
private static par.Cache packageDataCache = default;

// preloadWorkerCount is the number of concurrent goroutines that can load
// packages. Experimentally, there are diminishing returns with more than
// 4 workers. This was measured on the following machines.
//
// * MacBookPro with a 4-core Intel Core i7 CPU
// * Linux workstation with 6-core Intel Xeon CPU
// * Linux workstation with 24-core Intel Xeon CPU
//
// It is very likely (though not confirmed) that this workload is limited
// by memory bandwidth. We don't have a good way to determine the number of
// workers that would saturate the bus though, so runtime.GOMAXPROCS
// seems like a reasonable default.
private static var preloadWorkerCount = runtime.GOMAXPROCS(0);

// preload holds state for managing concurrent preloading of package data.
//
// A preload should be created with newPreload before loading a large
// package graph. flush must be called when package loading is complete
// to ensure preload goroutines are no longer active. This is necessary
// because of global mutable state that cannot safely be read and written
// concurrently. In particular, packageDataCache may be cleared by "go get"
// in GOPATH mode, and modload.loaded (accessed via modload.Lookup) may be
// modified by modload.LoadPackages.
private partial struct preload {
    public channel<object> cancel;
    public channel<object> sema;
}

// newPreload creates a new preloader. flush must be called later to avoid
// accessing global state while it is being modified.
private static ptr<preload> newPreload() {
    ptr<preload> pre = addr(new preload(cancel:make(chanstruct{}),sema:make(chanstruct{},preloadWorkerCount),));
    return _addr_pre!;
}

// preloadMatches loads data for package paths matched by patterns.
// When preloadMatches returns, some packages may not be loaded yet, but
// loadPackageData and loadImport are always safe to call.
private static void preloadMatches(this ptr<preload> _addr_pre, context.Context ctx, PackageOpts opts, slice<ptr<search.Match>> matches) {
    ref preload pre = ref _addr_pre.val;

    foreach (var (_, m) in matches) {
        foreach (var (_, pkg) in m.Pkgs) {
            return ;
            go_(() => pkg => {
                nint mode = 0; // don't use vendoring or module import resolution
                var (bp, loaded, err) = loadPackageData(ctx, pkg, "", @base.Cwd(), "", false, mode);
                pre.sema.Receive();
                if (bp != null && loaded && err == null && !opts.IgnoreImports) {
                    pre.preloadImports(ctx, opts, bp.Imports, bp);
                }

            }(pkg));

        }
    }
}

// preloadImports queues a list of imports for preloading.
// When preloadImports returns, some packages may not be loaded yet,
// but loadPackageData and loadImport are always safe to call.
private static void preloadImports(this ptr<preload> _addr_pre, context.Context ctx, PackageOpts opts, slice<@string> imports, ptr<build.Package> _addr_parent) {
    ref preload pre = ref _addr_pre.val;
    ref build.Package parent = ref _addr_parent.val;

    var parentIsStd = parent.Goroot && parent.ImportPath != "" && search.IsStandardImportPath(parent.ImportPath);
    foreach (var (_, path) in imports) {
        if (path == "C" || path == "unsafe") {
            continue;
        }
        return ;
        go_(() => path => {
            var (bp, loaded, err) = loadPackageData(ctx, path, parent.ImportPath, parent.Dir, parent.Root, parentIsStd, ResolveImport);
            pre.sema.Receive();
            if (bp != null && loaded && err == null && !opts.IgnoreImports) {
                pre.preloadImports(ctx, opts, bp.Imports, bp);
            }
        }(path));

    }
}

// flush stops pending preload operations. flush blocks until preload calls to
// loadPackageData have completed. The preloader will not make any new calls
// to loadPackageData.
private static void flush(this ptr<preload> _addr_pre) => func((_, panic, _) => {
    ref preload pre = ref _addr_pre.val;
 
    // flush is usually deferred.
    // Don't hang program waiting for workers on panic.
    {
        var v = recover();

        if (v != null) {
            panic(v);
        }
    }


    close(pre.cancel);
    for (nint i = 0; i < preloadWorkerCount; i++) {
        pre.sema.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
    }

});

private static @string cleanImport(@string path) {
    var orig = path;
    path = pathpkg.Clean(path);
    if (strings.HasPrefix(orig, "./") && path != ".." && !strings.HasPrefix(path, "../")) {
        path = "./" + path;
    }
    return path;

}

private static par.Cache isDirCache = default;

private static bool isDir(@string path) {
    return isDirCache.Do(path, () => {
        var (fi, err) = fsys.Stat(path);
        return err == null && fi.IsDir();
    })._<bool>();
}

// ResolveImportPath returns the true meaning of path when it appears in parent.
// There are two different resolutions applied.
// First, there is Go 1.5 vendoring (golang.org/s/go15vendor).
// If vendor expansion doesn't trigger, then the path is also subject to
// Go 1.11 module legacy conversion (golang.org/issue/25069).
public static @string ResolveImportPath(ptr<Package> _addr_parent, @string path) {
    @string found = default;
    ref Package parent = ref _addr_parent.val;

    @string parentPath = default;    @string parentDir = default;    @string parentRoot = default;

    var parentIsStd = false;
    if (parent != null) {
        parentPath = parent.ImportPath;
        parentDir = parent.Dir;
        parentRoot = parent.Root;
        parentIsStd = parent.Standard;
    }
    return resolveImportPath(path, parentPath, parentDir, parentRoot, parentIsStd);

}

private static @string resolveImportPath(@string path, @string parentPath, @string parentDir, @string parentRoot, bool parentIsStd) {
    @string found = default;

    if (cfg.ModulesEnabled) {
        {
            var (_, p, e) = modload.Lookup(parentPath, parentIsStd, path);

            if (e == null) {
                return p;
            }

        }

        return path;

    }
    found = vendoredImportPath(path, parentPath, parentDir, parentRoot);
    if (found != path) {
        return found;
    }
    return moduleImportPath(path, parentPath, parentDir, parentRoot);

}

// dirAndRoot returns the source directory and workspace root
// for the package p, guaranteeing that root is a path prefix of dir.
private static (@string, @string) dirAndRoot(@string path, @string dir, @string root) {
    @string _p0 = default;
    @string _p0 = default;

    var origDir = dir;
    var origRoot = root;
    dir = filepath.Clean(dir);
    root = filepath.Join(root, "src");
    if (!str.HasFilePathPrefix(dir, root) || path != "command-line-arguments" && filepath.Join(root, path) != dir) { 
        // Look for symlinks before reporting error.
        dir = expandPath(dir);
        root = expandPath(root);

    }
    if (!str.HasFilePathPrefix(dir, root) || len(dir) <= len(root) || dir[len(root)] != filepath.Separator || path != "command-line-arguments" && !build.IsLocalImport(path) && filepath.Join(root, path) != dir) {
        @base.Fatalf("unexpected directory layout:\n" + "	import path: %s\n" + "	root: %s\n" + "	dir: %s\n" + "	expand root: %s\n" + "	expand dir: %s\n" + "	separator: %s", path, filepath.Join(origRoot, "src"), filepath.Clean(origDir), origRoot, origDir, string(filepath.Separator));
    }
    return (dir, root);

}

// vendoredImportPath returns the vendor-expansion of path when it appears in parent.
// If parent is x/y/z, then path might expand to x/y/z/vendor/path, x/y/vendor/path,
// x/vendor/path, vendor/path, or else stay path if none of those exist.
// vendoredImportPath returns the expanded path or, if no expansion is found, the original.
private static @string vendoredImportPath(@string path, @string parentPath, @string parentDir, @string parentRoot) {
    @string found = default;

    if (parentRoot == "") {
        return path;
    }
    var (dir, root) = dirAndRoot(parentPath, parentDir, parentRoot);

    @string vpath = "vendor/" + path;
    for (var i = len(dir); i >= len(root); i--) {
        if (i < len(dir) && dir[i] != filepath.Separator) {
            continue;
        }
        if (!isDir(filepath.Join(dir[..(int)i], "vendor"))) {
            continue;
        }
        var targ = filepath.Join(dir[..(int)i], vpath);
        if (isDir(targ) && hasGoFiles(targ)) {
            var importPath = parentPath;
            if (importPath == "command-line-arguments") { 
                // If parent.ImportPath is 'command-line-arguments'.
                // set to relative directory to root (also chopped root directory)
                importPath = dir[(int)len(root) + 1..];

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
            if (chopped == len(importPath) + 1) { 
                // We walked up from c:\gopath\src\foo\bar
                // and found c:\gopath\src\vendor\path.
                // We chopped \foo\bar (length 8) but the import path is "foo/bar" (length 7).
                // Use "vendor/path" without any prefix.
                return vpath;

            }

            return importPath[..(int)len(importPath) - chopped] + "/" + vpath;

        }
    }
    return path;

}

private static slice<byte> modulePrefix = (slice<byte>)"\nmodule ";private static par.Cache goModPathCache = default;

// goModPath returns the module path in the go.mod in dir, if any.
private static @string goModPath(@string dir) {
    @string path = default;

    return goModPathCache.Do(dir, () => {
        var (data, err) = os.ReadFile(filepath.Join(dir, "go.mod"));
        if (err != null) {
            return "";
        }
        nint i = default;
        if (bytes.HasPrefix(data, modulePrefix[(int)1..])) {
            i = 0;
        }
        else
 {
            i = bytes.Index(data, modulePrefix);
            if (i < 0) {
                return "";
            }
            i++;
        }
        var line = data[(int)i..]; 

        // Cut line at \n, drop trailing \r if present.
        {
            var j = bytes.IndexByte(line, '\n');

            if (j >= 0) {
                line = line[..(int)j];
            }

        }

        if (line[len(line) - 1] == '\r') {
            line = line[..(int)len(line) - 1];
        }
        line = line[(int)len("module ")..]; 

        // If quoted, unquote.
        path = strings.TrimSpace(string(line));
        if (path != "" && path[0] == '"') {
            var (s, err) = strconv.Unquote(path);
            if (err != null) {
                return "";
            }
            path = s;
        }
        return path;

    })._<@string>();

}

// findVersionElement returns the slice indices of the final version element /vN in path.
// If there is no such element, it returns -1, -1.
private static (nint, nint) findVersionElement(@string path) {
    nint i = default;
    nint j = default;

    j = len(path);
    for (i = len(path) - 1; i >= 0; i--) {
        if (path[i] == '/') {
            if (isVersionElement(path[(int)i + 1..(int)j])) {
                return (i, j);
            }
            j = i;
        }
    }
    return (-1, -1);

}

// isVersionElement reports whether s is a well-formed path version element:
// v2, v3, v10, etc, but not v0, v05, v1.
private static bool isVersionElement(@string s) {
    if (len(s) < 2 || s[0] != 'v' || s[1] == '0' || s[1] == '1' && len(s) == 2) {
        return false;
    }
    for (nint i = 1; i < len(s); i++) {
        if (s[i] < '0' || '9' < s[i]) {
            return false;
        }
    }
    return true;

}

// moduleImportPath translates import paths found in go modules
// back down to paths that can be resolved in ordinary builds.
//
// Define “new” code as code with a go.mod file in the same directory
// or a parent directory. If an import in new code says x/y/v2/z but
// x/y/v2/z does not exist and x/y/go.mod says “module x/y/v2”,
// then go build will read the import as x/y/z instead.
// See golang.org/issue/25069.
private static @string moduleImportPath(@string path, @string parentPath, @string parentDir, @string parentRoot) {
    @string found = default;

    if (parentRoot == "") {
        return path;
    }
    {
        var i__prev1 = i;

        var (i, _) = findVersionElement(path);

        if (i < 0) {
            return path;
        }
        i = i__prev1;

    }


    var (dir, root) = dirAndRoot(parentPath, parentDir, parentRoot); 

    // Consider dir and parents, up to and including root.
    {
        var i__prev1 = i;

        for (var i = len(dir); i >= len(root); i--) {
            if (i < len(dir) && dir[i] != filepath.Separator) {
                continue;
            }
            if (goModPath(dir[..(int)i]) != "") {
                goto HaveGoMod;
            }
        }

        i = i__prev1;
    } 
    // This code is not in a tree with a go.mod,
    // so apply no changes to the path.
    return path;

HaveGoMod: 

    // Otherwise look for a go.mod supplying a version element.
    // Some version-like elements may appear in paths but not
    // be module versions; we skip over those to look for module
    // versions. For example the module m/v2 might have a
    // package m/v2/api/v1/foo.
    {
        var bp__prev1 = bp;

        var (bp, _) = cfg.BuildContext.Import(path, "", build.IgnoreVendor);

        if (bp.Dir != "") {
            return path;
        }
        bp = bp__prev1;

    } 

    // Otherwise look for a go.mod supplying a version element.
    // Some version-like elements may appear in paths but not
    // be module versions; we skip over those to look for module
    // versions. For example the module m/v2 might have a
    // package m/v2/api/v1/foo.
    var limit = len(path);
    while (limit > 0) {
        var (i, j) = findVersionElement(path[..(int)limit]);
        if (i < 0) {
            return path;
        }
        {
            var bp__prev1 = bp;

            (bp, _) = cfg.BuildContext.Import(path[..(int)i], "", build.IgnoreVendor);

            if (bp.Dir != "") {
                {
                    var mpath = goModPath(bp.Dir);

                    if (mpath != "") { 
                        // Found a valid go.mod file, so we're stopping the search.
                        // If the path is m/v2/p and we found m/go.mod that says
                        // "module m/v2", then we return "m/p".
                        if (mpath == path[..(int)j]) {
                            return path[..(int)i] + path[(int)j..];
                        } 
                        // Otherwise just return the original path.
                        // We didn't find anything worth rewriting,
                        // and the go.mod indicates that we should
                        // not consider parent directories.
                        return path;

                    }

                }

            }

            bp = bp__prev1;

        }

        limit = i;

    }
    return path;

}

// hasGoFiles reports whether dir contains any files with names ending in .go.
// For a vendor check we must exclude directories that contain no .go files.
// Otherwise it is not possible to vendor just a/b/c and still import the
// non-vendored a/b. See golang.org/issue/13832.
private static bool hasGoFiles(@string dir) {
    var (files, _) = os.ReadDir(dir);
    foreach (var (_, f) in files) {
        if (!f.IsDir() && strings.HasSuffix(f.Name(), ".go")) {
            return true;
        }
    }    return false;

}

// reusePackage reuses package p to satisfy the import at the top
// of the import stack stk. If this use causes an import loop,
// reusePackage updates p's error information to record the loop.
private static ptr<Package> reusePackage(ptr<Package> _addr_p, ptr<ImportStack> _addr_stk) {
    ref Package p = ref _addr_p.val;
    ref ImportStack stk = ref _addr_stk.val;
 
    // We use p.Internal.Imports==nil to detect a package that
    // is in the midst of its own loadPackage call
    // (all the recursion below happens before p.Internal.Imports gets set).
    if (p.Internal.Imports == null) {
        if (p.Error == null) {
            p.Error = addr(new PackageError(ImportStack:stk.Copy(),Err:errors.New("import cycle not allowed"),IsImportCycle:true,));
        }
        else if (!p.Error.IsImportCycle) { 
            // If the error is already set, but it does not indicate that
            // we are in an import cycle, set IsImportCycle so that we don't
            // end up stuck in a loop down the road.
            p.Error.IsImportCycle = true;

        }
        p.Incomplete = true;

    }
    if (p.Error != null && !p.Error.IsImportCycle && stk.shorterThan(p.Error.ImportStack)) {
        p.Error.ImportStack = stk.Copy();
    }
    return _addr_p!;

}

// disallowInternal checks that srcDir (containing package importerPath, if non-empty)
// is allowed to import p.
// If the import is allowed, disallowInternal returns the original package p.
// If not, it returns a new package containing just an appropriate error.
private static ptr<Package> disallowInternal(context.Context ctx, @string srcDir, ptr<Package> _addr_importer, @string importerPath, ptr<Package> _addr_p, ptr<ImportStack> _addr_stk) {
    ref Package importer = ref _addr_importer.val;
    ref Package p = ref _addr_p.val;
    ref ImportStack stk = ref _addr_stk.val;
 
    // golang.org/s/go14internal:
    // An import of a path containing the element “internal”
    // is disallowed if the importing code is outside the tree
    // rooted at the parent of the “internal” directory.

    // There was an error loading the package; stop here.
    if (p.Error != null) {
        return _addr_p!;
    }
    if (str.HasPathPrefix(p.ImportPath, "testing/internal") && importerPath == "testmain") {
        return _addr_p!;
    }
    if (cfg.BuildContext.Compiler == "gccgo" && p.Standard) {
        return _addr_p!;
    }
    if (p.Standard && strings.HasPrefix(importerPath, "bootstrap/")) {
        return _addr_p!;
    }
    if (importerPath == "") {
        return _addr_p!;
    }
    var (i, ok) = findInternal(p.ImportPath);
    if (!ok) {
        return _addr_p!;
    }
    if (i > 0) {
        i--; // rewind over slash in ".../internal"
    }
    if (p.Module == null) {
        var parent = p.Dir[..(int)i + len(p.Dir) - len(p.ImportPath)];

        if (str.HasFilePathPrefix(filepath.Clean(srcDir), filepath.Clean(parent))) {
            return _addr_p!;
        }
        srcDir = expandPath(srcDir);
        parent = expandPath(parent);
        if (str.HasFilePathPrefix(filepath.Clean(srcDir), filepath.Clean(parent))) {
            return _addr_p!;
        }
    }
    else
 { 
        // p is in a module, so make it available based on the importer's import path instead
        // of the file path (https://golang.org/issue/23970).
        if (importer.Internal.CmdlineFiles) { 
            // The importer is a list of command-line files.
            // Pretend that the import path is the import path of the
            // directory containing them.
            // If the directory is outside the main module, this will resolve to ".",
            // which is not a prefix of any valid module.
            importerPath = modload.DirImportPath(ctx, importer.Dir);

        }
        var parentOfInternal = p.ImportPath[..(int)i];
        if (str.HasPathPrefix(importerPath, parentOfInternal)) {
            return _addr_p!;
        }
    }
    ref Package perr = ref heap(p, out ptr<Package> _addr_perr);
    perr.Error = addr(new PackageError(alwaysPrintStack:true,ImportStack:stk.Copy(),Err:ImportErrorf(p.ImportPath,"use of internal package "+p.ImportPath+" not allowed"),));
    perr.Incomplete = true;
    return _addr__addr_perr!;

}

// findInternal looks for the final "internal" path element in the given import path.
// If there isn't one, findInternal returns ok=false.
// Otherwise, findInternal returns ok=true and the index of the "internal".
private static (nint, bool) findInternal(@string path) {
    nint index = default;
    bool ok = default;
 
    // Three cases, depending on internal at start/end of string or not.
    // The order matters: we must return the index of the final element,
    // because the final one produces the most restrictive requirement
    // on the importer.

    if (strings.HasSuffix(path, "/internal")) 
        return (len(path) - len("internal"), true);
    else if (strings.Contains(path, "/internal/")) 
        return (strings.LastIndex(path, "/internal/") + 1, true);
    else if (path == "internal" || strings.HasPrefix(path, "internal/")) 
        return (0, true);
        return (0, false);

}

// disallowVendor checks that srcDir is allowed to import p as path.
// If the import is allowed, disallowVendor returns the original package p.
// If not, it returns a new package containing just an appropriate error.
private static ptr<Package> disallowVendor(@string srcDir, @string path, @string importerPath, ptr<Package> _addr_p, ptr<ImportStack> _addr_stk) {
    ref Package p = ref _addr_p.val;
    ref ImportStack stk = ref _addr_stk.val;
 
    // If the importerPath is empty, we started
    // with a name given on the command line, not an
    // import. Anything listed on the command line is fine.
    if (importerPath == "") {
        return _addr_p!;
    }
    {
        var perr__prev1 = perr;

        ref var perr = ref heap(disallowVendorVisibility(srcDir, _addr_p, importerPath, _addr_stk), out ptr<var> _addr_perr);

        if (perr != p) {
            return _addr_perr!;
        }
        perr = perr__prev1;

    } 

    // Paths like x/vendor/y must be imported as y, never as x/vendor/y.
    {
        var (i, ok) = FindVendor(path);

        if (ok) {
            perr = p;
            perr.Error = addr(new PackageError(ImportStack:stk.Copy(),Err:ImportErrorf(path,"%s must be imported as %s",path,path[i+len("vendor/"):]),));
            perr.Incomplete = true;
            return _addr__addr_perr!;
        }
    }


    return _addr_p!;

}

// disallowVendorVisibility checks that srcDir is allowed to import p.
// The rules are the same as for /internal/ except that a path ending in /vendor
// is not subject to the rules, only subdirectories of vendor.
// This allows people to have packages and commands named vendor,
// for maximal compatibility with existing source trees.
private static ptr<Package> disallowVendorVisibility(@string srcDir, ptr<Package> _addr_p, @string importerPath, ptr<ImportStack> _addr_stk) {
    ref Package p = ref _addr_p.val;
    ref ImportStack stk = ref _addr_stk.val;
 
    // The stack does not include p.ImportPath.
    // If there's nothing on the stack, we started
    // with a name given on the command line, not an
    // import. Anything listed on the command line is fine.
    if (importerPath == "") {
        return _addr_p!;
    }
    var (i, ok) = FindVendor(p.ImportPath);
    if (!ok) {
        return _addr_p!;
    }
    if (i > 0) {
        i--; // rewind over slash in ".../vendor"
    }
    var truncateTo = i + len(p.Dir) - len(p.ImportPath);
    if (truncateTo < 0 || len(p.Dir) < truncateTo) {
        return _addr_p!;
    }
    var parent = p.Dir[..(int)truncateTo];
    if (str.HasFilePathPrefix(filepath.Clean(srcDir), filepath.Clean(parent))) {
        return _addr_p!;
    }
    srcDir = expandPath(srcDir);
    parent = expandPath(parent);
    if (str.HasFilePathPrefix(filepath.Clean(srcDir), filepath.Clean(parent))) {
        return _addr_p!;
    }
    ref Package perr = ref heap(p, out ptr<Package> _addr_perr);
    perr.Error = addr(new PackageError(ImportStack:stk.Copy(),Err:errors.New("use of vendored package not allowed"),));
    perr.Incomplete = true;
    return _addr__addr_perr!;

}

// FindVendor looks for the last non-terminating "vendor" path element in the given import path.
// If there isn't one, FindVendor returns ok=false.
// Otherwise, FindVendor returns ok=true and the index of the "vendor".
//
// Note that terminating "vendor" elements don't count: "x/vendor" is its own package,
// not the vendored copy of an import "" (the empty import path).
// This will allow people to have packages or commands named vendor.
// This may help reduce breakage, or it may just be confusing. We'll see.
public static (nint, bool) FindVendor(@string path) {
    nint index = default;
    bool ok = default;
 
    // Two cases, depending on internal at start of string or not.
    // The order matters: we must return the index of the final element,
    // because the final one is where the effective import path starts.

    if (strings.Contains(path, "/vendor/")) 
        return (strings.LastIndex(path, "/vendor/") + 1, true);
    else if (strings.HasPrefix(path, "vendor/")) 
        return (0, true);
        return (0, false);

}

public partial struct TargetDir { // : nint
}

public static readonly TargetDir ToTool = iota; // to GOROOT/pkg/tool (default for cmd/*)
public static readonly var ToBin = 0; // to bin dir inside package root (default for non-cmd/*)
public static readonly var StalePath = 1; // an old import path; fail to build

// InstallTargetDir reports the target directory for installing the command p.
public static TargetDir InstallTargetDir(ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;

    if (strings.HasPrefix(p.ImportPath, "code.google.com/p/go.tools/cmd/")) {
        return StalePath;
    }
    if (p.Goroot && strings.HasPrefix(p.ImportPath, "cmd/") && p.Name == "main") {
        switch (p.ImportPath) {
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

// exeFromImportPath returns an executable name
// for a package using the import path.
//
// The executable name is the last element of the import path.
// In module-aware mode, an additional rule is used on import paths
// consisting of two or more path elements. If the last element is
// a vN path element specifying the major version, then the
// second last element of the import path is used instead.
private static @string exeFromImportPath(this ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;

    var (_, elem) = pathpkg.Split(p.ImportPath);
    if (cfg.ModulesEnabled) { 
        // If this is example.com/mycmd/v2, it's more useful to
        // install it as mycmd than as v2. See golang.org/issue/24667.
        if (elem != p.ImportPath && isVersionElement(elem)) {
            _, elem = pathpkg.Split(pathpkg.Dir(p.ImportPath));
        }
    }
    return elem;

}

// exeFromFiles returns an executable name for a package
// using the first element in GoFiles or CgoFiles collections without the prefix.
//
// Returns empty string in case of empty collection.
private static @string exeFromFiles(this ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;

    @string src = default;
    if (len(p.GoFiles) > 0) {
        src = p.GoFiles[0];
    }
    else if (len(p.CgoFiles) > 0) {
        src = p.CgoFiles[0];
    }
    else
 {
        return "";
    }
    var (_, elem) = filepath.Split(src);
    return elem[..(int)len(elem) - len(".go")];

}

// DefaultExecName returns the default executable name for a package
private static @string DefaultExecName(this ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;

    if (p.Internal.CmdlineFiles) {
        return p.exeFromFiles();
    }
    return p.exeFromImportPath();

}

// load populates p using information from bp, err, which should
// be the result of calling build.Context.Import.
// stk contains the import stack, not including path itself.
private static void load(this ptr<Package> _addr_p, context.Context ctx, PackageOpts opts, @string path, ptr<ImportStack> _addr_stk, slice<token.Position> importPos, ptr<build.Package> _addr_bp, error err) => func((defer, _, _) => {
    ref Package p = ref _addr_p.val;
    ref ImportStack stk = ref _addr_stk.val;
    ref build.Package bp = ref _addr_bp.val;

    p.copyBuild(opts, bp); 

    // The localPrefix is the path we interpret ./ imports relative to.
    // Synthesized main packages sometimes override this.
    if (p.Internal.Local) {
        p.Internal.LocalPrefix = dirToImportPath(p.Dir);
    }
    Action<error> setError = err => {
        if (p.Error == null) {
            p.Error = addr(new PackageError(ImportStack:stk.Copy(),Err:err,)); 

            // Add the importer's position information if the import position exists, and
            // the current package being examined is the importer.
            // If we have not yet accepted package p onto the import stack,
            // then the cause of the error is not within p itself: the error
            // must be either in an explicit command-line argument,
            // or on the importer side (indicated by a non-empty importPos).
            if (path != stk.Top() && len(importPos) > 0) {
                p.Error.setPos(importPos);
            }

        }
    };

    if (err != null) {
        p.Incomplete = true;
        p.setLoadPackageDataError(err, path, stk, importPos);
    }
    var useBindir = p.Name == "main";
    if (!p.Standard) {
        switch (cfg.BuildBuildmode) {
            case "c-archive": 

            case "c-shared": 

            case "plugin": 
                useBindir = false;
                break;
        }

    }
    if (useBindir) { 
        // Report an error when the old code.google.com/p/go.tools paths are used.
        if (InstallTargetDir(_addr_p) == StalePath) { 
            // TODO(matloob): remove this branch, and StalePath itself. code.google.com/p/go is so
            // old, even this code checking for it is stale now!
            var newPath = strings.Replace(p.ImportPath, "code.google.com/p/go.", "golang.org/x/", 1);
            var e = ImportErrorf(p.ImportPath, "the %v command has moved; use %v instead.", p.ImportPath, newPath);
            setError(e);
            return ;

        }
        var elem = p.DefaultExecName();
        var full = cfg.BuildContext.GOOS + "_" + cfg.BuildContext.GOARCH + "/" + elem;
        if (cfg.BuildContext.GOOS != @base.ToolGOOS || cfg.BuildContext.GOARCH != @base.ToolGOARCH) { 
            // Install cross-compiled binaries to subdirectories of bin.
            elem = full;

        }
        if (p.Internal.Build.BinDir == "" && cfg.ModulesEnabled) {
            p.Internal.Build.BinDir = modload.BinDir();
        }
        if (p.Internal.Build.BinDir != "") { 
            // Install to GOBIN or bin of GOPATH entry.
            p.Target = filepath.Join(p.Internal.Build.BinDir, elem);
            if (!p.Goroot && strings.Contains(elem, "/") && cfg.GOBIN != "") { 
                // Do not create $GOBIN/goos_goarch/elem.
                p.Target = "";
                p.Internal.GobinSubdir = true;

            }

        }
        if (InstallTargetDir(_addr_p) == ToTool) { 
            // This is for 'go tool'.
            // Override all the usual logic and force it into the tool directory.
            if (cfg.BuildToolchainName == "gccgo") {
                p.Target = filepath.Join(@base.ToolDir, elem);
            }
            else
 {
                p.Target = filepath.Join(cfg.GOROOTpkg, "tool", full);
            }

        }
        if (p.Target != "" && cfg.BuildContext.GOOS == "windows") {
            p.Target += ".exe";
        }
    }
    else if (p.Internal.Local) { 
        // Local import turned into absolute path.
        // No permanent install target.
        p.Target = "";

    }
    else
 {
        p.Target = p.Internal.Build.PkgObj;
        if (cfg.BuildLinkshared && p.Target != "") { 
            // TODO(bcmills): The reliance on p.Target implies that -linkshared does
            // not work for any package that lacks a Target — such as a non-main
            // package in module mode. We should probably fix that.
            var shlibnamefile = p.Target[..(int)len(p.Target) - 2] + ".shlibname";
            var (shlib, err) = os.ReadFile(shlibnamefile);
            if (err != null && !os.IsNotExist(err)) {
                @base.Fatalf("reading shlibname: %v", err);
            }

            if (err == null) {
                var libname = strings.TrimSpace(string(shlib));
                if (cfg.BuildContext.Compiler == "gccgo") {
                    p.Shlib = filepath.Join(p.Internal.Build.PkgTargetRoot, "shlibs", libname);
                }
                else
 {
                    p.Shlib = filepath.Join(p.Internal.Build.PkgTargetRoot, libname);
                }

            }

        }
    }
    var importPaths = p.Imports;
    Action<@string, bool> addImport = (path, forCompiler) => {
        foreach (var (_, p) in importPaths) {
            if (path == p) {
                return ;
            }
        }        importPaths = append(importPaths, path);
        if (forCompiler) {
            p.Internal.CompiledImports = append(p.Internal.CompiledImports, path);
        }
    };

    if (!opts.IgnoreImports) { 
        // Cgo translation adds imports of "unsafe", "runtime/cgo" and "syscall",
        // except for certain packages, to avoid circular dependencies.
        if (p.UsesCgo()) {
            addImport("unsafe", true);
        }
        if (p.UsesCgo() && (!p.Standard || !cgoExclude[p.ImportPath]) && cfg.BuildContext.Compiler != "gccgo") {
            addImport("runtime/cgo", true);
        }
        if (p.UsesCgo() && (!p.Standard || !cgoSyscallExclude[p.ImportPath])) {
            addImport("syscall", true);
        }
        if (p.UsesSwig()) {
            addImport("unsafe", true);
            if (cfg.BuildContext.Compiler != "gccgo") {
                addImport("runtime/cgo", true);
            }
            addImport("syscall", true);
            addImport("sync", true); 

            // TODO: The .swig and .swigcxx files can use
            // %go_import directives to import other packages.
        }
        if (p.Name == "main" && !p.Internal.ForceLibrary) {
            foreach (var (_, dep) in LinkerDeps(_addr_p)) {
                addImport(dep, false);
            }
        }
    }
    var fold = str.ToFold(p.ImportPath);
    {
        var other = foldPath[fold];

        if (other == "") {
            foldPath[fold] = p.ImportPath;
        }
        else if (other != p.ImportPath) {
            setError(ImportErrorf(p.ImportPath, "case-insensitive import collision: %q and %q", p.ImportPath, other));
            return ;
        }

    }


    if (!SafeArg(p.ImportPath)) {
        setError(ImportErrorf(p.ImportPath, "invalid import path %q", p.ImportPath));
        return ;
    }
    stk.Push(path);
    defer(stk.Pop());

    var pkgPath = p.ImportPath;
    if (p.Internal.CmdlineFiles) {
        pkgPath = "command-line-arguments";
    }
    if (cfg.ModulesEnabled) {
        p.Module = modload.PackageModuleInfo(ctx, pkgPath);
    }
    p.EmbedFiles, p.Internal.Embed, err = resolveEmbed(p.Dir, p.EmbedPatterns);
    if (err != null) {
        p.Incomplete = true;
        setError(err);
        ptr<EmbedError> embedErr = err._<ptr<EmbedError>>();
        p.Error.setPos(p.Internal.Build.EmbedPatternPos[embedErr.Pattern]);
    }
    var inputs = p.AllFiles();
    var (f1, f2) = str.FoldDup(inputs);
    if (f1 != "") {
        setError(fmt.Errorf("case-insensitive file name collision: %q and %q", f1, f2));
        return ;
    }
    foreach (var (_, file) in inputs) {
        if (!SafeArg(file) || strings.HasPrefix(file, "_cgo_")) {
            setError(fmt.Errorf("invalid input file name %q", file));
            return ;
        }
    }    {
        var name = pathpkg.Base(p.ImportPath);

        if (!SafeArg(name)) {
            setError(fmt.Errorf("invalid input directory name %q", name));
            return ;
        }
    } 

    // Build list of imported packages and full dependency list.
    var imports = make_slice<ptr<Package>>(0, len(p.Imports));
    foreach (var (i, path) in importPaths) {
        if (path == "C") {
            continue;
        }
        var p1 = LoadImport(ctx, opts, path, p.Dir, _addr_p, _addr_stk, p.Internal.Build.ImportPos[path], ResolveImport);

        path = p1.ImportPath;
        importPaths[i] = path;
        if (i < len(p.Imports)) {
            p.Imports[i] = path;
        }
        imports = append(imports, p1);
        if (p1.Incomplete) {
            p.Incomplete = true;
        }
    }    p.Internal.Imports = imports;
    p.collectDeps();

    if (cfg.ModulesEnabled && p.Error == null && p.Name == "main" && len(p.DepsErrors) == 0) {
        p.Internal.BuildInfo = modload.PackageBuildInfo(pkgPath, p.Deps);
    }
    if (p.Standard && (p.ImportPath == "unsafe" || cfg.BuildContext.Compiler == "gccgo")) {
        p.Target = "";
    }
    if (!cfg.BuildContext.CgoEnabled) {
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
    if (len(p.CFiles) > 0 && !p.UsesCgo() && !p.UsesSwig() && cfg.BuildContext.Compiler == "gc") {
        setError(fmt.Errorf("C source files not allowed when not using cgo or SWIG: %s", strings.Join(p.CFiles, " ")));
        return ;
    }
    if (len(p.CXXFiles) > 0 && !p.UsesCgo() && !p.UsesSwig()) {
        setError(fmt.Errorf("C++ source files not allowed when not using cgo or SWIG: %s", strings.Join(p.CXXFiles, " ")));
        return ;
    }
    if (len(p.MFiles) > 0 && !p.UsesCgo() && !p.UsesSwig()) {
        setError(fmt.Errorf("Objective-C source files not allowed when not using cgo or SWIG: %s", strings.Join(p.MFiles, " ")));
        return ;
    }
    if (len(p.FFiles) > 0 && !p.UsesCgo() && !p.UsesSwig()) {
        setError(fmt.Errorf("Fortran source files not allowed when not using cgo or SWIG: %s", strings.Join(p.FFiles, " ")));
        return ;
    }
});

// An EmbedError indicates a problem with a go:embed directive.
public partial struct EmbedError {
    public @string Pattern;
    public error Err;
}

private static @string Error(this ptr<EmbedError> _addr_e) {
    ref EmbedError e = ref _addr_e.val;

    return fmt.Sprintf("pattern %s: %v", e.Pattern, e.Err);
}

private static error Unwrap(this ptr<EmbedError> _addr_e) {
    ref EmbedError e = ref _addr_e.val;

    return error.As(e.Err)!;
}

// ResolveEmbed resolves //go:embed patterns and returns only the file list.
// For use by go mod vendor to find embedded files it should copy into the
// vendor directory.
// TODO(#42504): Once go mod vendor uses load.PackagesAndErrors, just
// call (*Package).ResolveEmbed
public static (slice<@string>, error) ResolveEmbed(@string dir, slice<@string> patterns) {
    slice<@string> _p0 = default;
    error _p0 = default!;

    var (files, _, err) = resolveEmbed(dir, patterns);
    return (files, error.As(err)!);
}

// resolveEmbed resolves //go:embed patterns to precise file lists.
// It sets files to the list of unique files matched (for go list),
// and it sets pmap to the more precise mapping from
// patterns to files.
private static (slice<@string>, map<@string, slice<@string>>, error) resolveEmbed(@string pkgdir, slice<@string> patterns) => func((defer, _, _) => {
    slice<@string> files = default;
    map<@string, slice<@string>> pmap = default;
    error err = default!;

    @string pattern = default;
    defer(() => {
        if (err != null) {
            err = addr(new EmbedError(Pattern:pattern,Err:err,));
        }
    }()); 

    // TODO(rsc): All these messages need position information for better error reports.
    pmap = make_map<@string, slice<@string>>();
    var have = make_map<@string, nint>();
    var dirOK = make_map<@string, bool>();
    nint pid = 0; // pattern ID, to allow reuse of have map
    foreach (var (_, __pattern) in patterns) {
        pattern = __pattern;
        pid++; 

        // Check pattern is valid for //go:embed.
        {
            var (_, err) = path.Match(pattern, "");

            if (err != null || !validEmbedPattern(pattern)) {
                return (null, null, error.As(fmt.Errorf("invalid pattern syntax"))!);
            } 

            // Glob to find matches.

        } 

        // Glob to find matches.
        var (match, err) = fsys.Glob(pkgdir + string(filepath.Separator) + filepath.FromSlash(pattern));
        if (err != null) {
            return (null, null, error.As(err)!);
        }
        slice<@string> list = default;
        {
            var file__prev2 = file;

            foreach (var (_, __file) in match) {
                file = __file;
                var rel = filepath.ToSlash(file[(int)len(pkgdir) + 1..]); // file, relative to p.Dir

                @string what = "file";
                var (info, err) = fsys.Lstat(file);
                if (err != null) {
                    return (null, null, error.As(err)!);
                }

                if (info.IsDir()) {
                    what = "directory";
                } 

                // Check that directories along path do not begin a new module
                // (do not contain a go.mod).
                {
                    var dir = file;

                    while (len(dir) > len(pkgdir) + 1 && !dirOK[dir]) {
                        {
                            (_, err) = fsys.Stat(filepath.Join(dir, "go.mod"));

                            if (err == null) {
                                return (null, null, error.As(fmt.Errorf("cannot embed %s %s: in different module", what, rel))!);
                        dir = filepath.Dir(dir);
                            }

                        }

                        if (dir != file) {
                            {
                                var info__prev2 = info;

                                (info, err) = fsys.Lstat(dir);

                                if (err == null && !info.IsDir()) {
                                    return (null, null, error.As(fmt.Errorf("cannot embed %s %s: in non-directory %s", what, rel, dir[(int)len(pkgdir) + 1..]))!);
                                }

                                info = info__prev2;

                            }

                        }

                        dirOK[dir] = true;
                        {
                            var elem = filepath.Base(dir);

                            if (isBadEmbedName(elem)) {
                                if (dir == file) {
                                    return (null, null, error.As(fmt.Errorf("cannot embed %s %s: invalid name %s", what, rel, elem))!);
                                }
                                else
 {
                                    return (null, null, error.As(fmt.Errorf("cannot embed %s %s: in invalid directory %s", what, rel, elem))!);
                                }

                            }

                        }

                    }

                }


                if (info.Mode().IsRegular()) 
                    if (have[rel] != pid) {
                        have[rel] = pid;
                        list = append(list, rel);
                    }
                else if (info.IsDir()) 
                    // Gather all files in the named directory, stopping at module boundaries
                    // and ignoring files that wouldn't be packaged into a module.
                    nint count = 0;
                    var err = fsys.Walk(file, (path, info, err) => {
                        if (err != null) {
                            return err;
                        }
                        rel = filepath.ToSlash(path[(int)len(pkgdir) + 1..]);
                        var name = info.Name();
                        if (path != file && (isBadEmbedName(name) || name[0] == '.' || name[0] == '_')) { 
                            // Ignore bad names, assuming they won't go into modules.
                            // Also avoid hidden files that user may not know about.
                            // See golang.org/issue/42328.
                            if (info.IsDir()) {
                                return fs.SkipDir;
                            }

                            return null;

                        }

                        if (info.IsDir()) {
                            {
                                (_, err) = fsys.Stat(filepath.Join(path, "go.mod"));

                                if (err == null) {
                                    return filepath.SkipDir;
                                }

                            }

                            return null;

                        }

                        if (!info.Mode().IsRegular()) {
                            return null;
                        }

                        count++;
                        if (have[rel] != pid) {
                            have[rel] = pid;
                            list = append(list, rel);
                        }

                        return null;

                    });
                    if (err != null) {
                        return (null, null, error.As(err)!);
                    }

                    if (count == 0) {
                        return (null, null, error.As(fmt.Errorf("cannot embed directory %s: contains no embeddable files", rel))!);
                    }

                else 
                    return (null, null, error.As(fmt.Errorf("cannot embed irregular file %s", rel))!);
                
            }

            file = file__prev2;
        }

        if (len(list) == 0) {
            return (null, null, error.As(fmt.Errorf("no matching files found"))!);
        }
        sort.Strings(list);
        pmap[pattern] = list;

    }
    {
        var file__prev1 = file;

        foreach (var (__file) in have) {
            file = __file;
            files = append(files, file);
        }
        file = file__prev1;
    }

    sort.Strings(files);
    return (files, pmap, error.As(null!)!);

});

private static bool validEmbedPattern(@string pattern) {
    return pattern != "." && fs.ValidPath(pattern);
}

// isBadEmbedName reports whether name is the base name of a file that
// can't or won't be included in modules and therefore shouldn't be treated
// as existing for embedding.
private static bool isBadEmbedName(@string name) {
    {
        var err = module.CheckFilePath(name);

        if (err != null) {
            return true;
        }
    }

    switch (name) { 
    // Empty string should be impossible but make it bad.
        case "": 
            return true; 
            // Version control directories won't be present in module.
            break;
        case ".bzr": 

        case ".hg": 

        case ".git": 

        case ".svn": 
            return true;
            break;
    }
    return false;

}

// collectDeps populates p.Deps and p.DepsErrors by iterating over
// p.Internal.Imports.
//
// TODO(jayconrod): collectDeps iterates over transitive imports for every
// package. We should only need to visit direct imports.
private static void collectDeps(this ptr<Package> _addr_p) => func((_, panic, _) => {
    ref Package p = ref _addr_p.val;

    var deps = make_map<@string, ptr<Package>>();
    slice<ptr<Package>> q = default;
    q = append(q, p.Internal.Imports);
    for (nint i = 0; i < len(q); i++) {
        var p1 = q[i];
        var path = p1.ImportPath; 
        // The same import path could produce an error or not,
        // depending on what tries to import it.
        // Prefer to record entries with errors, so we can report them.
        var p0 = deps[path];
        if (p0 == null || p1.Error != null && (p0.Error == null || len(p0.Error.ImportStack) > len(p1.Error.ImportStack))) {
            deps[path] = p1;
            foreach (var (_, p2) in p1.Internal.Imports) {
                if (deps[p2.ImportPath] != p2) {
                    q = append(q, p2);
                }
            }
        }
    }

    p.Deps = make_slice<@string>(0, len(deps));
    {
        var dep__prev1 = dep;

        foreach (var (__dep) in deps) {
            dep = __dep;
            p.Deps = append(p.Deps, dep);
        }
        dep = dep__prev1;
    }

    sort.Strings(p.Deps);
    {
        var dep__prev1 = dep;

        foreach (var (_, __dep) in p.Deps) {
            dep = __dep;
            p1 = deps[dep];
            if (p1 == null) {
                panic("impossible: missing entry in package cache for " + dep + " imported by " + p.ImportPath);
            }
            if (p1.Error != null) {
                p.DepsErrors = append(p.DepsErrors, p1.Error);
            }
        }
        dep = dep__prev1;
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
public static bool SafeArg(@string name) {
    if (name == "") {
        return false;
    }
    var c = name[0];
    return '0' <= c && c <= '9' || 'A' <= c && c <= 'Z' || 'a' <= c && c <= 'z' || c == '.' || c == '_' || c == '/' || c >= utf8.RuneSelf;

}

// LinkerDeps returns the list of linker-induced dependencies for main package p.
public static slice<@string> LinkerDeps(ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;
 
    // Everything links runtime.
    @string deps = new slice<@string>(new @string[] { "runtime" }); 

    // External linking mode forces an import of runtime/cgo.
    if (externalLinkingForced(_addr_p) && cfg.BuildContext.Compiler != "gccgo") {
        deps = append(deps, "runtime/cgo");
    }
    if (cfg.Goarch == "arm") {
        deps = append(deps, "math");
    }
    if (cfg.BuildRace) {
        deps = append(deps, "runtime/race");
    }
    if (cfg.BuildMSan) {
        deps = append(deps, "runtime/msan");
    }
    return deps;

}

// externalLinkingForced reports whether external linking is being
// forced even for programs that do not use cgo.
private static bool externalLinkingForced(ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;

    if (!cfg.BuildContext.CgoEnabled) {
        return false;
    }
    switch (cfg.BuildContext.GOOS) {
        case "android": 
            if (cfg.BuildContext.GOARCH != "arm64") {
                return true;
            }
            break;
        case "ios": 
            return true;
            break;
    } 

    // Currently build modes c-shared, pie (on systems that do not
    // support PIE with internal linking mode (currently all
    // systems: issue #18968)), plugin, and -linkshared force
    // external linking mode, as of course does
    // -ldflags=-linkmode=external. External linking mode forces
    // an import of runtime/cgo.
    // If there are multiple -linkmode options, the last one wins.
    var pieCgo = cfg.BuildBuildmode == "pie" && !sys.InternalLinkPIESupported(cfg.BuildContext.GOOS, cfg.BuildContext.GOARCH);
    var linkmodeExternal = false;
    if (p != null) {
        var ldflags = BuildLdflags.For(p);
        for (var i = len(ldflags) - 1; i >= 0; i--) {
            var a = ldflags[i];
            if (a == "-linkmode=external" || a == "-linkmode" && i + 1 < len(ldflags) && ldflags[i + 1] == "external") {
                linkmodeExternal = true;
                break;
            }
            else if (a == "-linkmode=internal" || a == "-linkmode" && i + 1 < len(ldflags) && ldflags[i + 1] == "internal") {
                break;
            }

        }

    }
    return cfg.BuildBuildmode == "c-shared" || cfg.BuildBuildmode == "plugin" || pieCgo || cfg.BuildLinkshared || linkmodeExternal;

}

// mkAbs rewrites list, which must be paths relative to p.Dir,
// into a sorted list of absolute paths. It edits list in place but for
// convenience also returns list back to its caller.
private static slice<@string> mkAbs(this ptr<Package> _addr_p, slice<@string> list) {
    ref Package p = ref _addr_p.val;

    foreach (var (i, f) in list) {
        list[i] = filepath.Join(p.Dir, f);
    }    sort.Strings(list);
    return list;
}

// InternalGoFiles returns the list of Go files being built for the package,
// using absolute paths.
private static slice<@string> InternalGoFiles(this ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;

    return p.mkAbs(str.StringList(p.GoFiles, p.CgoFiles, p.TestGoFiles));
}

// InternalXGoFiles returns the list of Go files being built for the XTest package,
// using absolute paths.
private static slice<@string> InternalXGoFiles(this ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;

    return p.mkAbs(p.XTestGoFiles);
}

// InternalGoFiles returns the list of all Go files possibly relevant for the package,
// using absolute paths. "Possibly relevant" means that files are not excluded
// due to build tags, but files with names beginning with . or _ are still excluded.
private static slice<@string> InternalAllGoFiles(this ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;

    return p.mkAbs(str.StringList(p.IgnoredGoFiles, p.GoFiles, p.CgoFiles, p.TestGoFiles, p.XTestGoFiles));
}

// usesSwig reports whether the package needs to run SWIG.
private static bool UsesSwig(this ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;

    return len(p.SwigFiles) > 0 || len(p.SwigCXXFiles) > 0;
}

// usesCgo reports whether the package needs to run cgo
private static bool UsesCgo(this ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;

    return len(p.CgoFiles) > 0;
}

// PackageList returns the list of packages in the dag rooted at roots
// as visited in a depth-first post-order traversal.
public static slice<ptr<Package>> PackageList(slice<ptr<Package>> roots) {
    map seen = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<Package>, bool>{};
    ptr<Package> all = new slice<ptr<Package>>(new ptr<Package>[] {  });
    Action<ptr<Package>> walk = default;
    walk = p => {
        if (seen[p]) {
            return ;
        }
        seen[p] = true;
        foreach (var (_, p1) in p.Internal.Imports) {
            walk(p1);
        }        all = append(all, p);

    };
    foreach (var (_, root) in roots) {
        walk(root);
    }    return all;

}

// TestPackageList returns the list of packages in the dag rooted at roots
// as visited in a depth-first post-order traversal, including the test
// imports of the roots. This ignores errors in test packages.
public static slice<ptr<Package>> TestPackageList(context.Context ctx, PackageOpts opts, slice<ptr<Package>> roots) {
    map seen = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<Package>, bool>{};
    ptr<Package> all = new slice<ptr<Package>>(new ptr<Package>[] {  });
    Action<ptr<Package>> walk = default;
    walk = p => {
        if (seen[p]) {
            return ;
        }
        seen[p] = true;
        {
            var p1__prev1 = p1;

            foreach (var (_, __p1) in p.Internal.Imports) {
                p1 = __p1;
                walk(p1);
            }

            p1 = p1__prev1;
        }

        all = append(all, p);

    };
    Action<ptr<Package>, @string> walkTest = (root, path) => {
        ref ImportStack stk = ref heap(out ptr<ImportStack> _addr_stk);
        var p1 = LoadImport(ctx, opts, path, root.Dir, _addr_root, _addr_stk, root.Internal.Build.TestImportPos[path], ResolveImport);
        if (p1.Error == null) {
            walk(p1);
        }
    };
    foreach (var (_, root) in roots) {
        walk(root);
        {
            var path__prev2 = path;

            foreach (var (_, __path) in root.TestImports) {
                path = __path;
                walkTest(root, path);
            }

            path = path__prev2;
        }

        {
            var path__prev2 = path;

            foreach (var (_, __path) in root.XTestImports) {
                path = __path;
                walkTest(root, path);
            }

            path = path__prev2;
        }
    }    return all;

}

// LoadImportWithFlags loads the package with the given import path and
// sets tool flags on that package. This function is useful loading implicit
// dependencies (like sync/atomic for coverage).
// TODO(jayconrod): delete this function and set flags automatically
// in LoadImport instead.
public static ptr<Package> LoadImportWithFlags(@string path, @string srcDir, ptr<Package> _addr_parent, ptr<ImportStack> _addr_stk, slice<token.Position> importPos, nint mode) {
    ref Package parent = ref _addr_parent.val;
    ref ImportStack stk = ref _addr_stk.val;

    var p = LoadImport(context.TODO(), new PackageOpts(), path, srcDir, _addr_parent, _addr_stk, importPos, mode);
    setToolFlags(_addr_p);
    return _addr_p!;
}

// PackageOpts control the behavior of PackagesAndErrors and other package
// loading functions.
public partial struct PackageOpts {
    public bool IgnoreImports; // ModResolveTests indicates whether calls to the module loader should also
// resolve test dependencies of the requested packages.
//
// If ModResolveTests is true, then the module loader needs to resolve test
// dependencies at the same time as packages; otherwise, the test dependencies
// of those packages could be missing, and resolving those missing dependencies
// could change the selected versions of modules that provide other packages.
    public bool ModResolveTests; // MainOnly is true if the caller only wants to load main packages.
// For a literal argument matching a non-main package, a stub may be returned
// with an error. For a non-literal argument (with "..."), non-main packages
// are not be matched, and their dependencies may not be loaded. A warning
// may be printed for non-literal arguments that match no main packages.
    public bool MainOnly;
}

// PackagesAndErrors returns the packages named by the command line arguments
// 'patterns'. If a named package cannot be loaded, PackagesAndErrors returns
// a *Package with the Error field describing the failure. If errors are found
// loading imported packages, the DepsErrors field is set. The Incomplete field
// may be set as well.
//
// To obtain a flat list of packages, use PackageList.
// To report errors loading packages, use ReportPackageErrors.
public static slice<ptr<Package>> PackagesAndErrors(context.Context ctx, PackageOpts opts, slice<@string> patterns) => func((defer, panic, _) => {
    var (ctx, span) = trace.StartSpan(ctx, "load.PackagesAndErrors");
    defer(span.Done());

    {
        var p__prev1 = p;

        foreach (var (_, __p) in patterns) {
            p = __p; 
            // Listing is only supported with all patterns referring to either:
            // - Files that are part of the same directory.
            // - Explicit package paths or patterns.
            if (strings.HasSuffix(p, ".go")) { 
                // We need to test whether the path is an actual Go file and not a
                // package path or pattern ending in '.go' (see golang.org/issue/34653).
                {
                    var (fi, err) = fsys.Stat(p);

                    if (err == null && !fi.IsDir()) {
                        return new slice<ptr<Package>>(new ptr<Package>[] { GoFilesPackage(ctx,opts,patterns) });
                    }

                }

            }

        }
        p = p__prev1;
    }

    slice<ptr<search.Match>> matches = default;
    modload.Init();

    if (cfg.ModulesEnabled) {
        modload.PackageOpts modOpts = new modload.PackageOpts(ResolveMissingImports:true,LoadTests:opts.ModResolveTests,SilencePackageErrors:true,);
        matches, _ = modload.LoadPackages(ctx, modOpts, patterns);
    }
    else
 {
        matches = search.ImportPaths(patterns);
    }
    slice<ptr<Package>> pkgs = default;    ref ImportStack stk = ref heap(out ptr<ImportStack> _addr_stk);    var seenPkg = make_map<ptr<Package>, bool>();

    var pre = newPreload();
    defer(pre.flush());
    pre.preloadMatches(ctx, opts, matches);

    foreach (var (_, m) in matches) {
        foreach (var (_, pkg) in m.Pkgs) {
            if (pkg == "") {
                panic(fmt.Sprintf("ImportPaths returned empty package for pattern %s", m.Pattern()));
            }
            var p = loadImport(ctx, opts, _addr_pre, pkg, @base.Cwd(), _addr_null, _addr_stk, null, 0);
            p.Match = append(p.Match, m.Pattern());
            p.Internal.CmdlinePkg = true;
            if (m.IsLiteral()) { 
                // Note: do not set = m.IsLiteral unconditionally
                // because maybe we'll see p matching both
                // a literal and also a non-literal pattern.
                p.Internal.CmdlinePkgLiteral = true;

            }

            if (seenPkg[p]) {
                continue;
            }

            seenPkg[p] = true;
            pkgs = append(pkgs, p);

        }        if (len(m.Errs) > 0) { 
            // In addition to any packages that were actually resolved from the
            // pattern, there was some error in resolving the pattern itself.
            // Report it as a synthetic package.
            p = @new<Package>();
            p.ImportPath = m.Pattern(); 
            // Pass an empty ImportStack and nil importPos: the error arose from a pattern, not an import.
            stk = default;
            slice<token.Position> importPos = default;
            p.setLoadPackageDataError(m.Errs[0], m.Pattern(), _addr_stk, importPos);
            p.Incomplete = true;
            p.Match = append(p.Match, m.Pattern());
            p.Internal.CmdlinePkg = true;
            if (m.IsLiteral()) {
                p.Internal.CmdlinePkgLiteral = true;
            }

            pkgs = append(pkgs, p);

        }
    }    if (opts.MainOnly) {
        pkgs = mainPackagesOnly(pkgs, matches);
    }
    setToolFlags(_addr_pkgs);

    return pkgs;

});

// CheckPackageErrors prints errors encountered loading pkgs and their
// dependencies, then exits with a non-zero status if any errors were found.
public static void CheckPackageErrors(slice<ptr<Package>> pkgs) {
    map printed = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<PackageError>, bool>{};
    {
        var pkg__prev1 = pkg;

        foreach (var (_, __pkg) in pkgs) {
            pkg = __pkg;
            if (pkg.Error != null) {
                @base.Errorf("%v", pkg.Error);
                printed[pkg.Error] = true;
            }
            foreach (var (_, err) in pkg.DepsErrors) { 
                // Since these are errors in dependencies,
                // the same error might show up multiple times,
                // once in each package that depends on it.
                // Only print each once.
                if (!printed[err]) {
                    printed[err] = true;
                    @base.Errorf("%v", err);
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

        foreach (var (_, __pkg) in PackageList(pkgs)) {
            pkg = __pkg;
            if (seen[pkg.ImportPath] && !reported[pkg.ImportPath]) {
                reported[pkg.ImportPath] = true;
                @base.Errorf("internal error: duplicate loads of %s", pkg.ImportPath);
            }
            seen[pkg.ImportPath] = true;
        }
        pkg = pkg__prev1;
    }

    @base.ExitIfErrors();

}

// mainPackagesOnly filters out non-main packages matched only by arguments
// containing "..." and returns the remaining main packages.
//
// Packages with missing, invalid, or ambiguous names may be treated as
// possibly-main packages.
//
// mainPackagesOnly sets a non-main package's Error field and returns it if it
// is named by a literal argument.
//
// mainPackagesOnly prints warnings for non-literal arguments that only match
// non-main packages.
private static slice<ptr<Package>> mainPackagesOnly(slice<ptr<Package>> pkgs, slice<ptr<search.Match>> matches) {
    map treatAsMain = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{};
    {
        var m__prev1 = m;

        foreach (var (_, __m) in matches) {
            m = __m;
            if (m.IsLiteral()) {
                {
                    var path__prev2 = path;

                    foreach (var (_, __path) in m.Pkgs) {
                        path = __path;
                        treatAsMain[path] = true;
                    }

                    path = path__prev2;
                }
            }

        }
        m = m__prev1;
    }

    slice<ptr<Package>> mains = default;
    foreach (var (_, pkg) in pkgs) {
        if (pkg.Name == "main") {
            treatAsMain[pkg.ImportPath] = true;
            mains = append(mains, pkg);
            continue;
        }
        if (len(pkg.InvalidGoFiles) > 0) { // TODO(#45999): && pkg.Name == "", but currently go/build sets pkg.Name arbitrarily if it is ambiguous.
            // The package has (or may have) conflicting names, and we can't easily
            // tell whether one of them is "main". So assume that it could be, and
            // report an error for the package.
            treatAsMain[pkg.ImportPath] = true;

        }
        if (treatAsMain[pkg.ImportPath]) {
            if (pkg.Error == null) {
                pkg.Error = addr(new PackageError(Err:&mainPackageError{importPath:pkg.ImportPath}));
            }
            mains = append(mains, pkg);
        }
    }    {
        var m__prev1 = m;

        foreach (var (_, __m) in matches) {
            m = __m;
            if (m.IsLiteral() || len(m.Pkgs) == 0) {
                continue;
            }
            var foundMain = false;
            {
                var path__prev2 = path;

                foreach (var (_, __path) in m.Pkgs) {
                    path = __path;
                    if (treatAsMain[path]) {
                        foundMain = true;
                        break;
                    }
                }

                path = path__prev2;
            }

            if (!foundMain) {
                fmt.Fprintf(os.Stderr, "go: warning: %q matched only non-main packages\n", m.Pattern());
            }

        }
        m = m__prev1;
    }

    return mains;

}

private partial struct mainPackageError {
    public @string importPath;
}

private static @string Error(this ptr<mainPackageError> _addr_e) {
    ref mainPackageError e = ref _addr_e.val;

    return fmt.Sprintf("package %s is not a main package", e.importPath);
}

private static @string ImportPath(this ptr<mainPackageError> _addr_e) {
    ref mainPackageError e = ref _addr_e.val;

    return e.importPath;
}

private static void setToolFlags(params ptr<ptr<Package>>[] _addr_pkgs) {
    pkgs = pkgs.Clone();
    ref Package pkgs = ref _addr_pkgs.val;

    foreach (var (_, p) in PackageList(pkgs)) {
        p.Internal.Asmflags = BuildAsmflags.For(p);
        p.Internal.Gcflags = BuildGcflags.For(p);
        p.Internal.Ldflags = BuildLdflags.For(p);
        p.Internal.Gccgoflags = BuildGccgoflags.For(p);
    }
}

// GoFilesPackage creates a package for building a collection of Go files
// (typically named on the command line). The target is named p.a for
// package p or named after the first Go file for package main.
public static ptr<Package> GoFilesPackage(context.Context ctx, PackageOpts opts, slice<@string> gofiles) {
    modload.Init();

    foreach (var (_, f) in gofiles) {
        if (!strings.HasSuffix(f, ".go")) {
            ptr<Package> pkg = @new<Package>();
            pkg.Internal.Local = true;
            pkg.Internal.CmdlineFiles = true;
            pkg.Name = f;
            pkg.Error = addr(new PackageError(Err:fmt.Errorf("named files must be .go files: %s",pkg.Name),));
            return _addr_pkg!;
        }
    }    ref ImportStack stk = ref heap(out ptr<ImportStack> _addr_stk);
    var ctxt = cfg.BuildContext;
    ctxt.UseAllFiles = true; 

    // Synthesize fake "directory" that only shows the named files,
    // to make it look like this is a standard package or
    // command directory. So that local imports resolve
    // consistently, the files must all be in the same directory.
    slice<fs.FileInfo> dirent = default;
    @string dir = default;
    foreach (var (_, file) in gofiles) {
        var (fi, err) = fsys.Stat(file);
        if (err != null) {
            @base.Fatalf("%s", err);
        }
        if (fi.IsDir()) {
            @base.Fatalf("%s is a directory, should be a Go file", file);
        }
        var (dir1, _) = filepath.Split(file);
        if (dir1 == "") {
            dir1 = "./";
        }
        if (dir == "") {
            dir = dir1;
        }
        else if (dir != dir1) {
            @base.Fatalf("named files must all be in one directory; have %s and %s", dir, dir1);
        }
        dirent = append(dirent, fi);

    }    ctxt.ReadDir = _p0 => (_addr_dirent!, null);

    if (cfg.ModulesEnabled) {
        modload.ImportFromFiles(ctx, gofiles);
    }
    error err = default!;
    if (dir == "") {
        dir = @base.Cwd();
    }
    dir, err = filepath.Abs(dir);
    if (err != null) {
        @base.Fatalf("%s", err);
    }
    var (bp, err) = ctxt.ImportDir(dir, 0);
    pkg = @new<Package>();
    pkg.Internal.Local = true;
    pkg.Internal.CmdlineFiles = true;
    pkg.load(ctx, opts, "command-line-arguments", _addr_stk, null, bp, err);
    pkg.Internal.LocalPrefix = dirToImportPath(dir);
    pkg.ImportPath = "command-line-arguments";
    pkg.Target = "";
    pkg.Match = gofiles;

    if (pkg.Name == "main") {
        var exe = pkg.DefaultExecName() + cfg.ExeSuffix;

        if (cfg.GOBIN != "") {
            pkg.Target = filepath.Join(cfg.GOBIN, exe);
        }
        else if (cfg.ModulesEnabled) {
            pkg.Target = filepath.Join(modload.BinDir(), exe);
        }
    }
    if (opts.MainOnly && pkg.Name != "main" && pkg.Error == null) {
        pkg.Error = addr(new PackageError(Err:&mainPackageError{importPath:pkg.ImportPath}));
    }
    setToolFlags(pkg);

    return _addr_pkg!;

}

// PackagesAndErrorsOutsideModule is like PackagesAndErrors but runs in
// module-aware mode and ignores the go.mod file in the current directory or any
// parent directory, if there is one. This is used in the implementation of 'go
// install pkg@version' and other commands that support similar forms.
//
// modload.ForceUseModules must be true, and modload.RootMode must be NoRoot
// before calling this function.
//
// PackagesAndErrorsOutsideModule imposes several constraints to avoid
// ambiguity. All arguments must have the same version suffix (not just a suffix
// that resolves to the same version). They must refer to packages in the same
// module, which must not be std or cmd. That module is not considered the main
// module, but its go.mod file (if it has one) must not contain directives that
// would cause it to be interpreted differently if it were the main module
// (replace, exclude).
public static (slice<ptr<Package>>, error) PackagesAndErrorsOutsideModule(context.Context ctx, PackageOpts opts, slice<@string> args) => func((_, panic, _) => {
    slice<ptr<Package>> _p0 = default;
    error _p0 = default!;

    if (!modload.ForceUseModules) {
        panic("modload.ForceUseModules must be true");
    }
    if (modload.RootMode != modload.NoRoot) {
        panic("modload.RootMode must be NoRoot");
    }
    @string version = default;
    {
        var arg__prev1 = arg;

        foreach (var (_, __arg) in args) {
            arg = __arg;
            {
                var i__prev1 = i;

                var i = strings.Index(arg, "@");

                if (i >= 0) {
                    version = arg[(int)i + 1..];
                    if (version == "") {
                        return (null, error.As(fmt.Errorf("%s: version must not be empty", arg))!);
                    }
                    break;
                }

                i = i__prev1;

            }

        }
        arg = arg__prev1;
    }

    var patterns = make_slice<@string>(len(args));
    {
        var i__prev1 = i;
        var arg__prev1 = arg;

        foreach (var (__i, __arg) in args) {
            i = __i;
            arg = __arg;
            if (!strings.HasSuffix(arg, "@" + version)) {
                return (null, error.As(fmt.Errorf("%s: all arguments must have the same version (@%s)", arg, version))!);
            }
            var p = arg[..(int)len(arg) - len(version) - 1];

            if (build.IsLocalImport(p)) 
                return (null, error.As(fmt.Errorf("%s: argument must be a package path, not a relative path", arg))!);
            else if (filepath.IsAbs(p)) 
                return (null, error.As(fmt.Errorf("%s: argument must be a package path, not an absolute path", arg))!);
            else if (search.IsMetaPackage(p)) 
                return (null, error.As(fmt.Errorf("%s: argument must be a package path, not a meta-package", arg))!);
            else if (path.Clean(p) != p) 
                return (null, error.As(fmt.Errorf("%s: argument must be a clean package path", arg))!);
            else if (!strings.Contains(p, "...") && search.IsStandardImportPath(p) && goroot.IsStandardPackage(cfg.GOROOT, cfg.BuildContext.Compiler, p)) 
                return (null, error.As(fmt.Errorf("%s: argument must not be a package in the standard library", arg))!);
            else 
                patterns[i] = p;
            
        }
        i = i__prev1;
        arg = arg__prev1;
    }

    var allowed = modload.CheckAllowed;
    if (modload.IsRevisionQuery(version)) { 
        // Don't check for retractions if a specific revision is requested.
        allowed = null;

    }
    Func<@string, @string> noneSelected = path => "none";
    var (qrs, err) = modload.QueryPackages(ctx, patterns[0], version, noneSelected, allowed);
    if (err != null) {
        return (null, error.As(fmt.Errorf("%s: %w", args[0], err))!);
    }
    var rootMod = qrs[0].Mod;
    var (data, err) = modfetch.GoMod(rootMod.Path, rootMod.Version);
    if (err != null) {
        return (null, error.As(fmt.Errorf("%s: %w", args[0], err))!);
    }
    var (f, err) = modfile.Parse("go.mod", data, null);
    if (err != null) {
        return (null, error.As(fmt.Errorf("%s (in %s): %w", args[0], rootMod, err))!);
    }
    @string directiveFmt = "%s (in %s):\n" + "\tThe go.mod file for the module providing named packages contains one or\n" + "\tmore %s directives. It must not contain directives that would cause\n" + "\tit to be interpreted differently than if it were the main module.";
    if (len(f.Replace) > 0) {
        return (null, error.As(fmt.Errorf(directiveFmt, args[0], rootMod, "replace"))!);
    }
    if (len(f.Exclude) > 0) {
        return (null, error.As(fmt.Errorf(directiveFmt, args[0], rootMod, "exclude"))!);
    }
    {
        var (_, err) = modload.EditBuildList(ctx, null, new slice<module.Version>(new module.Version[] { rootMod }));

        if (err != null) {
            return (null, error.As(fmt.Errorf("%s: %w", args[0], err))!);
        }
    } 

    // Load packages for all arguments.
    var pkgs = PackagesAndErrors(ctx, opts, patterns); 

    // Check that named packages are all provided by the same module.
    foreach (var (_, pkg) in pkgs) {
        error pkgErr = default!;
        if (pkg.Module == null) { 
            // Packages in std, cmd, and their vendored dependencies
            // don't have this field set.
            pkgErr = error.As(fmt.Errorf("package %s not provided by module %s", pkg.ImportPath, rootMod))!;

        }
        else if (pkg.Module.Path != rootMod.Path || pkg.Module.Version != rootMod.Version) {
            pkgErr = error.As(fmt.Errorf("package %s provided by module %s@%s\n\tAll packages must be provided by the same module (%s).", pkg.ImportPath, pkg.Module.Path, pkg.Module.Version, rootMod))!;
        }
        if (pkgErr != null && pkg.Error == null) {
            pkg.Error = addr(new PackageError(Err:pkgErr));
        }
    }    var matchers = make_slice<Func<@string, bool>>(len(patterns));
    {
        var i__prev1 = i;
        var p__prev1 = p;

        foreach (var (__i, __p) in patterns) {
            i = __i;
            p = __p;
            if (strings.Contains(p, "...")) {
                matchers[i] = search.MatchPattern(p);
            }
        }
        i = i__prev1;
        p = p__prev1;
    }

    return (pkgs, error.As(null!)!);

});

} // end load_package
