// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gcexportdata -- go2cs converted at 2022 March 06 23:32:15 UTC
// import "golang.org/x/tools/go/gcexportdata" ==> using gcexportdata = go.golang.org.x.tools.go.gcexportdata_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\gcexportdata\importer.go
using fmt = go.fmt_package;
using token = go.go.token_package;
using types = go.go.types_package;
using os = go.os_package;
using System;


namespace go.golang.org.x.tools.go;

public static partial class gcexportdata_package {

    // NewImporter returns a new instance of the types.Importer interface
    // that reads type information from export data files written by gc.
    // The Importer also satisfies types.ImporterFrom.
    //
    // Export data files are located using "go build" workspace conventions
    // and the build.Default context.
    //
    // Use this importer instead of go/importer.For("gc", ...) to avoid the
    // version-skew problems described in the documentation of this package,
    // or to control the FileSet or access the imports map populated during
    // package loading.
    //
public static types.ImporterFrom NewImporter(ptr<token.FileSet> _addr_fset, map<@string, ptr<types.Package>> imports) {
    ref token.FileSet fset = ref _addr_fset.val;

    return new importer(fset,imports);
}

private partial struct importer {
    public ptr<token.FileSet> fset;
    public map<@string, ptr<types.Package>> imports;
}

private static (ptr<types.Package>, error) Import(this importer imp, @string importPath) {
    ptr<types.Package> _p0 = default!;
    error _p0 = default!;

    return _addr_imp.ImportFrom(importPath, "", 0)!;
}

private static (ptr<types.Package>, error) ImportFrom(this importer imp, @string importPath, @string srcDir, types.ImportMode mode) => func((defer, _, _) => {
    ptr<types.Package> _ = default!;
    error err = default!;

    var (filename, path) = Find(importPath, srcDir);
    if (filename == "") {
        if (importPath == "unsafe") { 
            // Even for unsafe, call Find first in case
            // the package was vendored.
            return (_addr_types.Unsafe!, error.As(null!)!);

        }
        return (_addr_null!, error.As(fmt.Errorf("can't find import: %s", importPath))!);

    }
    {
        var (pkg, ok) = imp.imports[path];

        if (ok && pkg.Complete()) {
            return (_addr_pkg!, error.As(null!)!); // cache hit
        }
    } 

    // open file
    var (f, err) = os.Open(filename);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    defer(() => {
        f.Close();
        if (err != null) { 
            // add file name to error
            err = fmt.Errorf("reading export data: %s: %v", filename, err);

        }
    }());

    var (r, err) = NewReader(f);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    return _addr_Read(r, imp.fset, imp.imports, path)!;

});

} // end gcexportdata_package
