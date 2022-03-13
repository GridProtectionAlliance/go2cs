// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package srcimporter implements importing directly
// from source files rather than installed packages.

// package srcimporter -- go2cs converted at 2022 March 13 06:42:16 UTC
// import "go/internal/srcimporter" ==> using srcimporter = go.go.@internal.srcimporter_package
// Original source: C:\Program Files\Go\src\go\internal\srcimporter\srcimporter.go
namespace go.go.@internal;
// import "go/internal/srcimporter"


using fmt = fmt_package;
using ast = go.ast_package;
using build = go.build_package;
using parser = go.parser_package;
using token = go.token_package;
using types = go.types_package;
using exec = @internal.execabs_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using sync = sync_package;
using _@unsafe_ = @unsafe_package; // for go:linkname


// An Importer provides the context for importing packages from source code.

using System;
using System.Threading;
public static partial class srcimporter_package {

public partial struct Importer {
    public ptr<build.Context> ctxt;
    public ptr<token.FileSet> fset;
    public types.Sizes sizes;
    public map<@string, ptr<types.Package>> packages;
}

// New returns a new Importer for the given context, file set, and map
// of packages. The context is used to resolve import paths to package paths,
// and identifying the files belonging to the package. If the context provides
// non-nil file system functions, they are used instead of the regular package
// os functions. The file set is used to track position information of package
// files; and imported packages are added to the packages map.
public static ptr<Importer> New(ptr<build.Context> _addr_ctxt, ptr<token.FileSet> _addr_fset, map<@string, ptr<types.Package>> packages) {
    ref build.Context ctxt = ref _addr_ctxt.val;
    ref token.FileSet fset = ref _addr_fset.val;

    return addr(new Importer(ctxt:ctxt,fset:fset,sizes:types.SizesFor(ctxt.Compiler,ctxt.GOARCH),packages:packages,));
}

// Importing is a sentinel taking the place in Importer.packages
// for a package that is in the process of being imported.
private static types.Package importing = default;

// Import(path) is a shortcut for ImportFrom(path, ".", 0).
private static (ptr<types.Package>, error) Import(this ptr<Importer> _addr_p, @string path) {
    ptr<types.Package> _p0 = default!;
    error _p0 = default!;
    ref Importer p = ref _addr_p.val;

    return _addr_p.ImportFrom(path, ".", 0)!; // use "." rather than "" (see issue #24441)
}

// ImportFrom imports the package with the given import path resolved from the given srcDir,
// adds the new package to the set of packages maintained by the importer, and returns the
// package. Package path resolution and file system operations are controlled by the context
// maintained with the importer. The import mode must be zero but is otherwise ignored.
// Packages that are not comprised entirely of pure Go files may fail to import because the
// type checker may not be able to determine all exported entities (e.g. due to cgo dependencies).
private static (ptr<types.Package>, error) ImportFrom(this ptr<Importer> _addr_p, @string path, @string srcDir, types.ImportMode mode) => func((defer, panic, _) => {
    ptr<types.Package> _p0 = default!;
    error _p0 = default!;
    ref Importer p = ref _addr_p.val;

    if (mode != 0) {
        panic("non-zero import mode");
    }
    {
        var (abs, err) = p.absPath(srcDir);

        if (err == null) { // see issue #14282
            srcDir = abs;
        }
    }
    var (bp, err) = p.ctxt.Import(path, srcDir, 0);
    if (err != null) {
        return (_addr_null!, error.As(err)!); // err may be *build.NoGoError - return as is
    }
    if (bp.ImportPath == "unsafe") {
        return (_addr_types.Unsafe!, error.As(null!)!);
    }
    var pkg = p.packages[bp.ImportPath];
    if (pkg != null) {
        if (pkg == _addr_importing) {
            return (_addr_null!, error.As(fmt.Errorf("import cycle through package %q", bp.ImportPath))!);
        }
        if (!pkg.Complete()) { 
            // Package exists but is not complete - we cannot handle this
            // at the moment since the source importer replaces the package
            // wholesale rather than augmenting it (see #19337 for details).
            // Return incomplete package with error (see #16088).
            return (_addr_pkg!, error.As(fmt.Errorf("reimported partially imported package %q", bp.ImportPath))!);
        }
        return (_addr_pkg!, error.As(null!)!);
    }
    p.packages[bp.ImportPath] = _addr_importing;
    defer(() => { 
        // clean up in case of error
        // TODO(gri) Eventually we may want to leave a (possibly empty)
        // package in the map in all cases (and use that package to
        // identify cycles). See also issue 16088.
        if (p.packages[bp.ImportPath] == _addr_importing) {
            p.packages[bp.ImportPath] = null;
        }
    }());

    slice<@string> filenames = default;
    filenames = append(filenames, bp.GoFiles);
    filenames = append(filenames, bp.CgoFiles);

    var (files, err) = p.parseFiles(bp.Dir, filenames);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    error firstHardErr = default!;
    ref types.Config conf = ref heap(new types.Config(IgnoreFuncBodies:true,Error:func(errerror){iffirstHardErr==nil&&!err.(types.Error).Soft{firstHardErr=err}},Importer:p,Sizes:p.sizes,), out ptr<types.Config> _addr_conf);
    if (len(bp.CgoFiles) > 0) {
        if (p.ctxt.OpenFile != null) { 
            // cgo, gcc, pkg-config, etc. do not support
            // build.Context's VFS.
            conf.FakeImportC = true;
        }
        else
 {
            setUsesCgo(_addr_conf);
            var (file, err) = p.cgo(bp);
            if (err != null) {
                return (_addr_null!, error.As(err)!);
            }
            files = append(files, file);
        }
    }
    pkg, err = conf.Check(bp.ImportPath, p.fset, files, null);
    if (err != null) { 
        // If there was a hard error it is possibly unsafe
        // to use the package as it may not be fully populated.
        // Do not return it (see also #20837, #20855).
        if (firstHardErr != null) {
            pkg = null;
            err = firstHardErr; // give preference to first hard error over any soft error
        }
        return (_addr_pkg!, error.As(fmt.Errorf("type-checking package %q failed (%v)", bp.ImportPath, err))!);
    }
    if (firstHardErr != null) { 
        // this can only happen if we have a bug in go/types
        panic("package is not safe yet no error was returned");
    }
    p.packages[bp.ImportPath] = pkg;
    return (_addr_pkg!, error.As(null!)!);
});

private static (slice<ptr<ast.File>>, error) parseFiles(this ptr<Importer> _addr_p, @string dir, slice<@string> filenames) => func((defer, _, _) => {
    slice<ptr<ast.File>> _p0 = default;
    error _p0 = default!;
    ref Importer p = ref _addr_p.val;
 
    // use build.Context's OpenFile if there is one
    var open = p.ctxt.OpenFile;
    if (open == null) {
        open = name => os.Open(name);
    }
    var files = make_slice<ptr<ast.File>>(len(filenames));
    var errors = make_slice<error>(len(filenames));

    sync.WaitGroup wg = default;
    wg.Add(len(filenames));
    foreach (var (i, filename) in filenames) {
        go_(() => (i, filepath) => {
            defer(wg.Done());
            var (src, err) = open(filepath);
            if (err != null) {
                errors[i] = err; // open provides operation and filename in error
                return ;
            }
            files[i], errors[i] = parser.ParseFile(p.fset, filepath, src, 0);
            src.Close(); // ignore Close error - parsing may have succeeded which is all we need
        }(i, p.joinPath(dir, filename)));
    }    wg.Wait(); 

    // if there are errors, return the first one for deterministic results
    foreach (var (_, err) in errors) {
        if (err != null) {
            return (null, error.As(err)!);
        }
    }    return (files, error.As(null!)!);
});

private static (ptr<ast.File>, error) cgo(this ptr<Importer> _addr_p, ptr<build.Package> _addr_bp) => func((defer, _, _) => {
    ptr<ast.File> _p0 = default!;
    error _p0 = default!;
    ref Importer p = ref _addr_p.val;
    ref build.Package bp = ref _addr_bp.val;

    var (tmpdir, err) = os.MkdirTemp("", "srcimporter");
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    defer(os.RemoveAll(tmpdir));

    @string args = new slice<@string>(new @string[] { "go", "tool", "cgo", "-objdir", tmpdir });
    if (bp.Goroot) {
        switch (bp.ImportPath) {
            case "runtime/cgo": 
                args = append(args, "-import_runtime_cgo=false", "-import_syscall=false");
                break;
            case "runtime/race": 
                args = append(args, "-import_syscall=false");
                break;
        }
    }
    args = append(args, "--");
    args = append(args, strings.Fields(os.Getenv("CGO_CPPFLAGS")));
    args = append(args, bp.CgoCPPFLAGS);
    if (len(bp.CgoPkgConfig) > 0) {
        var cmd = exec.Command("pkg-config", append(new slice<@string>(new @string[] { "--cflags" }), bp.CgoPkgConfig));
        var (out, err) = cmd.CombinedOutput();
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        args = append(args, strings.Fields(string(out)));
    }
    args = append(args, "-I", tmpdir);
    args = append(args, strings.Fields(os.Getenv("CGO_CFLAGS")));
    args = append(args, bp.CgoCFLAGS);
    args = append(args, bp.CgoFiles);

    cmd = exec.Command(args[0], args[(int)1..]);
    cmd.Dir = bp.Dir;
    {
        var err = cmd.Run();

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }

    return _addr_parser.ParseFile(p.fset, filepath.Join(tmpdir, "_cgo_gotypes.go"), null, 0)!;
});

// context-controlled file system operations

private static (@string, error) absPath(this ptr<Importer> _addr_p, @string path) {
    @string _p0 = default;
    error _p0 = default!;
    ref Importer p = ref _addr_p.val;
 
    // TODO(gri) This should be using p.ctxt.AbsPath which doesn't
    // exist but probably should. See also issue #14282.
    return filepath.Abs(path);
}

private static bool isAbsPath(this ptr<Importer> _addr_p, @string path) {
    ref Importer p = ref _addr_p.val;

    {
        var f = p.ctxt.IsAbsPath;

        if (f != null) {
            return f(path);
        }
    }
    return filepath.IsAbs(path);
}

private static @string joinPath(this ptr<Importer> _addr_p, params @string[] elem) {
    elem = elem.Clone();
    ref Importer p = ref _addr_p.val;

    {
        var f = p.ctxt.JoinPath;

        if (f != null) {
            return f(elem);
        }
    }
    return filepath.Join(elem);
}

//go:linkname setUsesCgo go/types.srcimporter_setUsesCgo
private static void setUsesCgo(ptr<types.Config> conf);

} // end srcimporter_package
