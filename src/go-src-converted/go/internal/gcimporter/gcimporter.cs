// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package gcimporter implements Import for gc-generated object files.
// package gcimporter -- go2cs converted at 2022 March 06 23:32:37 UTC
// import "go/internal/gcimporter" ==> using gcimporter = go.go.@internal.gcimporter_package
// Original source: C:\Program Files\Go\src\go\internal\gcimporter\gcimporter.go
// import "go/internal/gcimporter"

using bufio = go.bufio_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using token = go.go.token_package;
using types = go.go.types_package;
using io = go.io_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using System;


namespace go.go.@internal;

public static partial class gcimporter_package {

    // debugging/development support
private static readonly var debug = false;



private static array<@string> pkgExts = new array<@string>(new @string[] { ".a", ".o" });

// FindPkg returns the filename and unique package id for an import
// path based on package information provided by build.Import (using
// the build.Default build.Context). A relative srcDir is interpreted
// relative to the current working directory.
// If no file was found, an empty filename is returned.
//
public static (@string, @string) FindPkg(@string path, @string srcDir) {
    @string filename = default;
    @string id = default;

    if (path == "") {
        return ;
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

            if (err == null) { // see issue 14282
                srcDir = abs;

            }

        }

        var (bp, _) = build.Import(path, srcDir, build.FindOnly | build.AllowBinary);
        if (bp.PkgObj == "") {
            id = path; // make sure we have an id to print in error message
            return ;

        }
        noext = strings.TrimSuffix(bp.PkgObj, ".a");
        id = bp.ImportPath;
        if (false) { // for debugging
        if (path != id) {
            fmt.Printf("%s -> %s\n", path, id);
        }
    }
    foreach (var (_, ext) in pkgExts) {
        filename = noext + ext;
        {
            var (f, err) = os.Stat(filename);

            if (err == null && !f.IsDir()) {
                return ;
            }

        }

    }    filename = ""; // not found
    return ;

}

// Import imports a gc-generated package given its import path and srcDir, adds
// the corresponding package object to the packages map, and returns the object.
// The packages map must contain all packages already imported.
//
public static (ptr<types.Package>, error) Import(ptr<token.FileSet> _addr_fset, map<@string, ptr<types.Package>> packages, @string path, @string srcDir, Func<@string, (io.ReadCloser, error)> lookup) => func((defer, _, _) => {
    ptr<types.Package> pkg = default!;
    error err = default!;
    ref token.FileSet fset = ref _addr_fset.val;

    io.ReadCloser rc = default;
    @string id = default;
    if (lookup != null) { 
        // With custom lookup specified, assume that caller has
        // converted path to a canonical import path for use in the map.
        if (path == "unsafe") {
            return (_addr_types.Unsafe!, error.As(null!)!);
        }
        id = path; 

        // No need to re-import if the package was imported completely before.
        pkg = packages[id];

        if (pkg != null && pkg.Complete()) {
            return ;
        }
        var (f, err) = lookup(path);
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        rc = f;

    }
    else
 {
        @string filename = default;
        filename, id = FindPkg(path, srcDir);
        if (filename == "") {
            if (path == "unsafe") {
                return (_addr_types.Unsafe!, error.As(null!)!);
            }
            return (_addr_null!, error.As(fmt.Errorf("can't find import: %q", id))!);
        }
        pkg = packages[id];

        if (pkg != null && pkg.Complete()) {
            return ;
        }
        (f, err) = os.Open(filename);
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        defer(() => {
            if (err != null) { 
                // add file name to error
                err = fmt.Errorf("%s: %v", filename, err);

            }

        }());
        rc = f;

    }
    defer(rc.Close());

    @string hdr = default;
    var buf = bufio.NewReader(rc);
    hdr, err = FindExportData(buf);

    if (err != null) {
        return ;
    }
    switch (hdr) {
        case "$$\n": 
            err = fmt.Errorf("import %q: old textual export format no longer supported (recompile library)", path);
            break;
        case "$$B\n": 
                   byte exportFormat = default;
                   exportFormat, err = buf.ReadByte(); 

                   // The indexed export format starts with an 'i'; the older
                   // binary export format starts with a 'c', 'd', or 'v'
                   // (from "version"). Select appropriate importer.
                   if (err == null && exportFormat == 'i') {
                       pkg, err = iImportData(fset, packages, buf, id);
                   }
                   else
            {
                       err = fmt.Errorf("import %q: old binary export format no longer supported (recompile library)", path);
                   }
            break;
        default: 
            err = fmt.Errorf("import %q: unknown export data header: %q", path, hdr);
            break;
    }

    return ;

});

private partial struct byPath { // : slice<ptr<types.Package>>
}

private static nint Len(this byPath a) {
    return len(a);
}
private static void Swap(this byPath a, nint i, nint j) {
    (a[i], a[j]) = (a[j], a[i]);
}
private static bool Less(this byPath a, nint i, nint j) {
    return a[i].Path() < a[j].Path();
}

} // end gcimporter_package
