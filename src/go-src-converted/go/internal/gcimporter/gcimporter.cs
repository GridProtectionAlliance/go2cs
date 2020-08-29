// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package gcimporter implements Import for gc-generated object files.
// package gcimporter -- go2cs converted at 2020 August 29 10:09:12 UTC
// import "go/internal/gcimporter" ==> using gcimporter = go.go.@internal.gcimporter_package
// Original source: C:\Go\src\go\internal\gcimporter\gcimporter.go
// import "go/internal/gcimporter"

using bufio = go.bufio_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using token = go.go.token_package;
using types = go.go.types_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace go {
namespace @internal
{
    public static partial class gcimporter_package
    {
        // debugging/development support
        private static readonly var debug = false;



        private static array<@string> pkgExts = new array<@string>(new @string[] { ".a", ".o" });

        // FindPkg returns the filename and unique package id for an import
        // path based on package information provided by build.Import (using
        // the build.Default build.Context). A relative srcDir is interpreted
        // relative to the current working directory.
        // If no file was found, an empty filename is returned.
        //
        public static (@string, @string) FindPkg(@string path, @string srcDir)
        {
            if (path == "")
            {
                return;
            }
            @string noext = default;

            if (build.IsLocalImport(path)) 
                // "./x" -> "/this/directory/x.ext", "/this/directory/x"
                noext = filepath.Join(srcDir, path);
                id = noext;
            else if (filepath.IsAbs(path)) 
                // for completeness only - go/build.Import
                // does not support absolute imports
                // "/x" -> "/x.ext", "/x"
                noext = path;
                id = path;
            else 
                // "x" -> "$GOPATH/pkg/$GOOS_$GOARCH/x.ext", "x"
                // Don't require the source files to be present.
                {
                    var (abs, err) = filepath.Abs(srcDir);

                    if (err == null)
                    { // see issue 14282
                        srcDir = abs;
                    }

                }
                var (bp, _) = build.Import(path, srcDir, build.FindOnly | build.AllowBinary);
                if (bp.PkgObj == "")
                {
                    id = path; // make sure we have an id to print in error message
                    return;
                }
                noext = strings.TrimSuffix(bp.PkgObj, ".a");
                id = bp.ImportPath;
                        if (false)
            { // for debugging
                if (path != id)
                {
                    fmt.Printf("%s -> %s\n", path, id);
                }
            } 

            // try extensions
            foreach (var (_, ext) in pkgExts)
            {
                filename = noext + ext;
                {
                    var (f, err) = os.Stat(filename);

                    if (err == null && !f.IsDir())
                    {
                        return;
                    }

                }
            }
            filename = ""; // not found
            return;
        }

        // Import imports a gc-generated package given its import path and srcDir, adds
        // the corresponding package object to the packages map, and returns the object.
        // The packages map must contain all packages already imported.
        //
        public static (ref types.Package, error) Import(map<@string, ref types.Package> packages, @string path, @string srcDir, Func<@string, (io.ReadCloser, error)> lookup) => func((defer, _, __) =>
        {
            io.ReadCloser rc = default;
            @string id = default;
            if (lookup != null)
            { 
                // With custom lookup specified, assume that caller has
                // converted path to a canonical import path for use in the map.
                if (path == "unsafe")
                {
                    return (types.Unsafe, null);
                }
                id = path; 

                // No need to re-import if the package was imported completely before.
                pkg = packages[id];

                if (pkg != null && pkg.Complete())
                {
                    return;
                }
                var (f, err) = lookup(path);
                if (err != null)
                {
                    return (null, err);
                }
                rc = f;
            }
            else
            {
                @string filename = default;
                filename, id = FindPkg(path, srcDir);
                if (filename == "")
                {
                    if (path == "unsafe")
                    {
                        return (types.Unsafe, null);
                    }
                    return (null, fmt.Errorf("can't find import: %q", id));
                } 

                // no need to re-import if the package was imported completely before
                pkg = packages[id];

                if (pkg != null && pkg.Complete())
                {
                    return;
                } 

                // open file
                (f, err) = os.Open(filename);
                if (err != null)
                {
                    return (null, err);
                }
                defer(() =>
                {
                    if (err != null)
                    { 
                        // add file name to error
                        err = fmt.Errorf("%s: %v", filename, err);
                    }
                }());
                rc = f;
            }
            defer(() =>
            {
                rc.Close();
            }());

            @string hdr = default;
            var buf = bufio.NewReader(rc);
            hdr, err = FindExportData(buf);

            if (err != null)
            {
                return;
            }
            switch (hdr)
            {
                case "$$\n": 
                    err = fmt.Errorf("import %q: old export format no longer supported (recompile library)", path);
                    break;
                case "$$B\n": 
                    slice<byte> data = default;
                    data, err = ioutil.ReadAll(buf);
                    if (err == null)
                    { 
                        // TODO(gri): allow clients of go/importer to provide a FileSet.
                        // Or, define a new standard go/types/gcexportdata package.
                        var fset = token.NewFileSet();
                        _, pkg, err = BImportData(fset, packages, data, id);
                        return;
                    }
                    break;
                default: 
                    err = fmt.Errorf("unknown export data header: %q", hdr);
                    break;
            }

            return;
        });

        private static types.Type deref(types.Type typ)
        {
            {
                ref types.Pointer (p, _) = typ._<ref types.Pointer>();

                if (p != null)
                {
                    return p.Elem();
                }

            }
            return typ;
        }

        private partial struct byPath // : slice<ref types.Package>
        {
        }

        private static long Len(this byPath a)
        {
            return len(a);
        }
        private static void Swap(this byPath a, long i, long j)
        {
            a[i] = a[j];
            a[j] = a[i];

        }
        private static bool Less(this byPath a, long i, long j)
        {
            return a[i].Path() < a[j].Path();
        }
    }
}}}
