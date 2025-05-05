// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package srcimporter implements importing directly
// from source files rather than installed packages.
namespace go.go.@internal;

// import "go/internal/srcimporter"
using fmt = fmt_package;
using ast = go.ast_package;
using build = go.build_package;
using parser = go.parser_package;
using token = go.token_package;
using types = go.types_package;
using io = io_package;
using os = os_package;
using exec = os.exec_package;
using filepath = path.filepath_package;
using strings = strings_package;
using sync = sync_package;
using _ = unsafe_package; // for go:linkname
using go;
using os;
using path;
using ꓸꓸꓸ@string = Span<@string>;

partial class srcimporter_package {

// An Importer provides the context for importing packages from source code.
[GoType] partial struct Importer {
    internal ж<go.build_package.Context> ctxt;
    internal ж<go.token_package.FileSet> fset;
    internal go.types_package.Sizes sizes;
    internal types.Package packages;
}

// New returns a new Importer for the given context, file set, and map
// of packages. The context is used to resolve import paths to package paths,
// and identifying the files belonging to the package. If the context provides
// non-nil file system functions, they are used instead of the regular package
// os functions. The file set is used to track position information of package
// files; and imported packages are added to the packages map.
public static ж<Importer> New(ж<build.Context> Ꮡctxt, ж<token.FileSet> Ꮡfset, types.Package packages) {
    ref var ctxt = ref Ꮡctxt.val;
    ref var fset = ref Ꮡfset.val;

    return Ꮡ(new Importer(
        ctxt: ctxt,
        fset: fset,
        sizes: types.SizesFor(ctxt.Compiler, ctxt.GOARCH), // uses go/types default if GOARCH not found

        packages: packages
    ));
}

// Importing is a sentinel taking the place in Importer.packages
// for a package that is in the process of being imported.
internal static types.Package importing;

// Import(path) is a shortcut for ImportFrom(path, ".", 0).
[GoRecv] public static (ж<types.Package>, error) Import(this ref Importer p, @string path) {
    return p.ImportFrom(path, "."u8, 0);
}

// use "." rather than "" (see issue #24441)

// ImportFrom imports the package with the given import path resolved from the given srcDir,
// adds the new package to the set of packages maintained by the importer, and returns the
// package. Package path resolution and file system operations are controlled by the context
// maintained with the importer. The import mode must be zero but is otherwise ignored.
// Packages that are not comprised entirely of pure Go files may fail to import because the
// type checker may not be able to determine all exported entities (e.g. due to cgo dependencies).
[GoRecv] public static (ж<types.Package>, error) ImportFrom(this ref Importer p, @string path, @string srcDir, types.ImportMode mode) => func((defer, _) => {
    if (mode != 0) {
        throw panic("non-zero import mode");
    }
    {
        var (abs, errΔ1) = p.absPath(srcDir); if (errΔ1 == default!) {
            // see issue #14282
            srcDir = abs;
        }
    }
    (bp, err) = p.ctxt.Import(path, srcDir, 0);
    if (err != default!) {
        return (default!, err);
    }
    // err may be *build.NoGoError - return as is
    // package unsafe is known to the type checker
    if ((~bp).ImportPath == "unsafe"u8) {
        return (types.Unsafe, default!);
    }
    // no need to re-import if the package was imported completely before
    var pkg = p.packages[(~bp).ImportPath];
    if (pkg != nil) {
        if (pkg == Ꮡ(importing)) {
            return (default!, fmt.Errorf("import cycle through package %q"u8, (~bp).ImportPath));
        }
        if (!pkg.Complete()) {
            // Package exists but is not complete - we cannot handle this
            // at the moment since the source importer replaces the package
            // wholesale rather than augmenting it (see #19337 for details).
            // Return incomplete package with error (see #16088).
            return (pkg, fmt.Errorf("reimported partially imported package %q"u8, (~bp).ImportPath));
        }
        return (pkg, default!);
    }
    p.packages[(~bp).ImportPath] = Ꮡ(importing);
    var bpʗ1 = bp;
    var importingʗ1 = importing;
    defer(() => {
        // clean up in case of error
        // TODO(gri) Eventually we may want to leave a (possibly empty)
        // package in the map in all cases (and use that package to
        // identify cycles). See also issue 16088.
        if (p.packages[(~bpʗ1).ImportPath] == Ꮡ(importingʗ1)) {
            p.packages[(~bpʗ1).ImportPath] = default!;
        }
    });
    slice<@string> filenames = default!;
    filenames = append(filenames, (~bp).GoFiles.ꓸꓸꓸ);
    filenames = append(filenames, (~bp).CgoFiles.ꓸꓸꓸ);
    (files, err) = p.parseFiles((~bp).Dir, filenames);
    if (err != default!) {
        return (default!, err);
    }
    // type-check package files
    error firstHardErr = default!;
    ref var conf = ref heap<go.types_package.Config>(out var Ꮡconf);
    conf = new types.Config(
        IgnoreFuncBodies: true, // continue type-checking after the first error

        Error: 
        var firstHardErrʗ1 = firstHardErr;
        (error err) => {
            if (firstHardErrʗ1 == default! && !errΔ2._<typesꓸError>().Soft) {
                firstHardErrʗ1 = errΔ2;
            }
        },
        Importer: p,
        Sizes: p.sizes
    );
    if (len((~bp).CgoFiles) > 0) {
        if (p.ctxt.OpenFile != default!){
            // cgo, gcc, pkg-config, etc. do not support
            // build.Context's VFS.
            conf.FakeImportC = true;
        } else {
            setUsesCgo(Ꮡconf);
            (file, errΔ3) = p.cgo(bp);
            if (errΔ3 != default!) {
                return (default!, fmt.Errorf("error processing cgo for package %q: %w"u8, (~bp).ImportPath, errΔ3));
            }
            files = append(files, file);
        }
    }
    (pkg, err) = conf.Check((~bp).ImportPath, p.fset, files, nil);
    if (err != default!) {
        // If there was a hard error it is possibly unsafe
        // to use the package as it may not be fully populated.
        // Do not return it (see also #20837, #20855).
        if (firstHardErr != default!) {
            pkg = default!;
            err = firstHardErr;
        }
        // give preference to first hard error over any soft error
        return (pkg, fmt.Errorf("type-checking package %q failed (%v)"u8, (~bp).ImportPath, err));
    }
    if (firstHardErr != default!) {
        // this can only happen if we have a bug in go/types
        throw panic("package is not safe yet no error was returned");
    }
    p.packages[(~bp).ImportPath] = pkg;
    return (pkg, default!);
});

[GoRecv] internal static (slice<ast.File>, error) parseFiles(this ref Importer p, @string dir, slice<@string> filenames) => func((defer, _) => {
    // use build.Context's OpenFile if there is one
    var open = p.ctxt.OpenFile;
    if (open == default!) {
        open = (@string name) => os.Open(name);
    }
    var files = new slice<ast.File>(len(filenames));
    var errors = new slice<error>(len(filenames));
    ref var wg = ref heap(new sync_package.WaitGroup(), out var Ꮡwg);
    wg.Add(len(filenames));
    foreach (var (i, filename) in filenames) {
        var errorsʗ3 = errors;
        var filesʗ3 = files;
        var openʗ3 = open;
        var wgʗ5 = wg;
        goǃ((nint i, @string filepath) => {
            var wgʗ6 = wg;
            defer(wgʗ6.Done);
            (src, err) = open(filepath);
            if (err != default!) {
                errors[i] = err;
                return;
            }
            (files[i], errors[i]) = parser.ParseFile(p.fset, filepath, src, parser.SkipObjectResolution);
            src.Close();
        }, i, p.joinPath(dir, filename));
    }
    wg.Wait();
    // if there are errors, return the first one for deterministic results
    foreach (var (_, err) in errors) {
        if (err != default!) {
            return (default!, err);
        }
    }
    return (files, default!);
});

[GoRecv] public static (ж<ast.File>, error) cgo(this ref Importer p, ж<build.Package> Ꮡbp) => func((defer, _) => {
    ref var bp = ref Ꮡbp.val;

    var (tmpdir, err) = os.MkdirTemp(""u8, "srcimporter"u8);
    if (err != default!) {
        return (default!, err);
    }
    deferǃ(os.RemoveAll, tmpdir, defer);
    @string goCmd = "go"u8;
    if (p.ctxt.GOROOT != ""u8) {
        goCmd = filepath.Join(p.ctxt.GOROOT, "bin", "go");
    }
    var args = new @string[]{goCmd, "tool", "cgo", "-objdir", tmpdir}.slice();
    if (bp.Goroot) {
        var exprᴛ1 = bp.ImportPath;
        if (exprᴛ1 == "runtime/cgo"u8) {
            args = append(args, "-import_runtime_cgo=false"u8, "-import_syscall=false");
        }
        else if (exprᴛ1 == "runtime/race"u8) {
            args = append(args, "-import_syscall=false"u8);
        }

    }
    args = append(args, "--"u8);
    args = append(args, strings.Fields(os.Getenv("CGO_CPPFLAGS"u8)).ꓸꓸꓸ);
    args = append(args, bp.CgoCPPFLAGS.ꓸꓸꓸ);
    if (len(bp.CgoPkgConfig) > 0) {
        var cmdΔ1 = exec.Command("pkg-config"u8, append(new @string[]{"--cflags"}.slice(), bp.CgoPkgConfig.ꓸꓸꓸ).ꓸꓸꓸ);
        (@out, errΔ1) = cmdΔ1.Output();
        if (errΔ1 != default!) {
            return (default!, fmt.Errorf("pkg-config --cflags: %w"u8, errΔ1));
        }
        args = append(args, strings.Fields(((@string)@out)).ꓸꓸꓸ);
    }
    args = append(args, "-I"u8, tmpdir);
    args = append(args, strings.Fields(os.Getenv("CGO_CFLAGS"u8)).ꓸꓸꓸ);
    args = append(args, bp.CgoCFLAGS.ꓸꓸꓸ);
    args = append(args, bp.CgoFiles.ꓸꓸꓸ);
    var cmd = exec.Command(args[0], args[1..].ꓸꓸꓸ);
    cmd.val.Dir = bp.Dir;
    {
        var errΔ2 = cmd.Run(); if (errΔ2 != default!) {
            return (default!, fmt.Errorf("go tool cgo: %w"u8, errΔ2));
        }
    }
    return parser.ParseFile(p.fset, filepath.Join(tmpdir, "_cgo_gotypes.go"), default!, parser.SkipObjectResolution);
});

// context-controlled file system operations
[GoRecv] internal static (@string, error) absPath(this ref Importer p, @string path) {
    // TODO(gri) This should be using p.ctxt.AbsPath which doesn't
    // exist but probably should. See also issue #14282.
    return filepath.Abs(path);
}

[GoRecv] internal static bool isAbsPath(this ref Importer p, @string path) {
    {
        var f = p.ctxt.IsAbsPath; if (f != default!) {
            return f(path);
        }
    }
    return filepath.IsAbs(path);
}

[GoRecv] internal static @string joinPath(this ref Importer p, params ꓸꓸꓸ@string elemʗp) {
    var elem = elemʗp.slice();

    {
        var f = p.ctxt.JoinPath; if (f != default!) {
            return f(elem.ꓸꓸꓸ);
        }
    }
    return filepath.Join(elem.ꓸꓸꓸ);
}

//go:linkname setUsesCgo go/types.srcimporter_setUsesCgo
internal static partial void setUsesCgo(ж<types.Config> conf);

} // end srcimporter_package
