// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package srcimporter implements importing directly
// from source files rather than installed packages.
// package srcimporter -- go2cs converted at 2020 August 29 10:09:10 UTC
// import "go/internal/srcimporter" ==> using srcimporter = go.go.@internal.srcimporter_package
// Original source: C:\Go\src\go\internal\srcimporter\srcimporter.go
// import "go/internal/srcimporter"

using fmt = go.fmt_package;
using ast = go.go.ast_package;
using build = go.go.build_package;
using parser = go.go.parser_package;
using token = go.go.token_package;
using types = go.go.types_package;
using filepath = go.path.filepath_package;
using sync = go.sync_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace go {
namespace @internal
{
    public static partial class srcimporter_package
    {
        // An Importer provides the context for importing packages from source code.
        public partial struct Importer
        {
            public ptr<build.Context> ctxt;
            public ptr<token.FileSet> fset;
            public types.Sizes sizes;
            public map<@string, ref types.Package> packages;
        }

        // NewImporter returns a new Importer for the given context, file set, and map
        // of packages. The context is used to resolve import paths to package paths,
        // and identifying the files belonging to the package. If the context provides
        // non-nil file system functions, they are used instead of the regular package
        // os functions. The file set is used to track position information of package
        // files; and imported packages are added to the packages map.
        public static ref Importer New(ref build.Context ctxt, ref token.FileSet fset, map<@string, ref types.Package> packages)
        {
            return ref new Importer(ctxt:ctxt,fset:fset,sizes:types.SizesFor(ctxt.Compiler,ctxt.GOARCH),packages:packages,);
        }

        // Importing is a sentinel taking the place in Importer.packages
        // for a package that is in the process of being imported.
        private static types.Package importing = default;

        // Import(path) is a shortcut for ImportFrom(path, ".", 0).
        private static (ref types.Package, error) Import(this ref Importer p, @string path)
        {
            return p.ImportFrom(path, ".", 0L); // use "." rather than "" (see issue #24441)
        }

        // ImportFrom imports the package with the given import path resolved from the given srcDir,
        // adds the new package to the set of packages maintained by the importer, and returns the
        // package. Package path resolution and file system operations are controlled by the context
        // maintained with the importer. The import mode must be zero but is otherwise ignored.
        // Packages that are not comprised entirely of pure Go files may fail to import because the
        // type checker may not be able to determine all exported entities (e.g. due to cgo dependencies).
        private static (ref types.Package, error) ImportFrom(this ref Importer _p, @string path, @string srcDir, types.ImportMode mode) => func(_p, (ref Importer p, Defer defer, Panic panic, Recover _) =>
        {
            if (mode != 0L)
            {
                panic("non-zero import mode");
            }
            {
                var (abs, err) = p.absPath(srcDir);

                if (err == null)
                { // see issue #14282
                    srcDir = abs;
                }

            }
            var (bp, err) = p.ctxt.Import(path, srcDir, 0L);
            if (err != null)
            {
                return (null, err); // err may be *build.NoGoError - return as is
            } 

            // package unsafe is known to the type checker
            if (bp.ImportPath == "unsafe")
            {
                return (types.Unsafe, null);
            } 

            // no need to re-import if the package was imported completely before
            var pkg = p.packages[bp.ImportPath];
            if (pkg != null)
            {
                if (pkg == ref importing)
                {
                    return (null, fmt.Errorf("import cycle through package %q", bp.ImportPath));
                }
                if (!pkg.Complete())
                { 
                    // Package exists but is not complete - we cannot handle this
                    // at the moment since the source importer replaces the package
                    // wholesale rather than augmenting it (see #19337 for details).
                    // Return incomplete package with error (see #16088).
                    return (pkg, fmt.Errorf("reimported partially imported package %q", bp.ImportPath));
                }
                return (pkg, null);
            }
            p.packages[bp.ImportPath] = ref importing;
            defer(() =>
            { 
                // clean up in case of error
                // TODO(gri) Eventually we may want to leave a (possibly empty)
                // package in the map in all cases (and use that package to
                // identify cycles). See also issue 16088.
                if (p.packages[bp.ImportPath] == ref importing)
                {
                    p.packages[bp.ImportPath] = null;
                }
            }());

            slice<@string> filenames = default;
            filenames = append(filenames, bp.GoFiles);
            filenames = append(filenames, bp.CgoFiles);

            var (files, err) = p.parseFiles(bp.Dir, filenames);
            if (err != null)
            {
                return (null, err);
            } 

            // type-check package files
            error firstHardErr = default;
            types.Config conf = new types.Config(IgnoreFuncBodies:true,FakeImportC:true,Error:func(errerror){iffirstHardErr==nil&&!err.(types.Error).Soft{firstHardErr=err}},Importer:p,Sizes:p.sizes,);
            pkg, err = conf.Check(bp.ImportPath, p.fset, files, null);
            if (err != null)
            { 
                // If there was a hard error it is possibly unsafe
                // to use the package as it may not be fully populated.
                // Do not return it (see also #20837, #20855).
                if (firstHardErr != null)
                {
                    pkg = null;
                    err = firstHardErr; // give preference to first hard error over any soft error
                }
                return (pkg, fmt.Errorf("type-checking package %q failed (%v)", bp.ImportPath, err));
            }
            if (firstHardErr != null)
            { 
                // this can only happen if we have a bug in go/types
                panic("package is not safe yet no error was returned");
            }
            p.packages[bp.ImportPath] = pkg;
            return (pkg, null);
        });

        private static (slice<ref ast.File>, error) parseFiles(this ref Importer _p, @string dir, slice<@string> filenames) => func(_p, (ref Importer p, Defer defer, Panic _, Recover __) =>
        {
            var open = p.ctxt.OpenFile; // possibly nil

            var files = make_slice<ref ast.File>(len(filenames));
            var errors = make_slice<error>(len(filenames));

            sync.WaitGroup wg = default;
            wg.Add(len(filenames));
            foreach (var (i, filename) in filenames)
            {
                go_(() => (i, filepath) =>
                {
                    defer(wg.Done());
                    if (open != null)
                    {
                        var (src, err) = open(filepath);
                        if (err != null)
                        {
                            errors[i] = fmt.Errorf("opening package file %s failed (%v)", filepath, err);
                            return;
                        }
                        files[i], errors[i] = parser.ParseFile(p.fset, filepath, src, 0L);
                        src.Close(); // ignore Close error - parsing may have succeeded which is all we need
                    }
                    else
                    { 
                        // Special-case when ctxt doesn't provide a custom OpenFile and use the
                        // parser's file reading mechanism directly. This appears to be quite a
                        // bit faster than opening the file and providing an io.ReaderCloser in
                        // both cases.
                        // TODO(gri) investigate performance difference (issue #19281)
                        files[i], errors[i] = parser.ParseFile(p.fset, filepath, null, 0L);
                    }
                }(i, p.joinPath(dir, filename)));
            }
            wg.Wait(); 

            // if there are errors, return the first one for deterministic results
            foreach (var (_, err) in errors)
            {
                if (err != null)
                {
                    return (null, err);
                }
            }
            return (files, null);
        });

        // context-controlled file system operations

        private static (@string, error) absPath(this ref Importer p, @string path)
        { 
            // TODO(gri) This should be using p.ctxt.AbsPath which doesn't
            // exist but probably should. See also issue #14282.
            return filepath.Abs(path);
        }

        private static bool isAbsPath(this ref Importer p, @string path)
        {
            {
                var f = p.ctxt.IsAbsPath;

                if (f != null)
                {
                    return f(path);
                }

            }
            return filepath.IsAbs(path);
        }

        private static @string joinPath(this ref Importer p, params @string[] elem)
        {
            {
                var f = p.ctxt.JoinPath;

                if (f != null)
                {
                    return f(elem);
                }

            }
            return filepath.Join(elem);
        }
    }
}}}
