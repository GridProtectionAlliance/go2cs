// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package gcexportdata provides functions for locating, reading, and
// writing export data files containing type information produced by the
// gc compiler.  This package supports go1.7 export data format and all
// later versions.
//
// Although it might seem convenient for this package to live alongside
// go/types in the standard library, this would cause version skew
// problems for developer tools that use it, since they must be able to
// consume the outputs of the gc compiler both before and after a Go
// update such as from Go 1.7 to Go 1.8.  Because this package lives in
// golang.org/x/tools, sites can update their version of this repo some
// time before the Go 1.8 release and rebuild and redeploy their
// developer tools, which will then be able to consume both Go 1.7 and
// Go 1.8 export data files, so they will work before and after the
// Go update. (See discussion at https://golang.org/issue/15651.)
//
// package gcexportdata -- go2cs converted at 2022 March 06 23:31:52 UTC
// import "golang.org/x/tools/go/gcexportdata" ==> using gcexportdata = go.golang.org.x.tools.go.gcexportdata_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\gcexportdata\gcexportdata.go
// import "golang.org/x/tools/go/gcexportdata"

using bufio = go.bufio_package;
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using token = go.go.token_package;
using types = go.go.types_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;

using gcimporter = go.golang.org.x.tools.go.@internal.gcimporter_package;

namespace go.golang.org.x.tools.go;

public static partial class gcexportdata_package {

    // Find returns the name of an object (.o) or archive (.a) file
    // containing type information for the specified import path,
    // using the workspace layout conventions of go/build.
    // If no file was found, an empty filename is returned.
    //
    // A relative srcDir is interpreted relative to the current working directory.
    //
    // Find also returns the package's resolved (canonical) import path,
    // reflecting the effects of srcDir and vendoring on importPath.
public static (@string, @string) Find(@string importPath, @string srcDir) {
    @string filename = default;
    @string path = default;

    return gcimporter.FindPkg(importPath, srcDir);
}

// NewReader returns a reader for the export data section of an object
// (.o) or archive (.a) file read from r.  The new reader may provide
// additional trailing data beyond the end of the export data.
public static (io.Reader, error) NewReader(io.Reader r) {
    io.Reader _p0 = default;
    error _p0 = default!;

    var buf = bufio.NewReader(r);
    var (_, err) = gcimporter.FindExportData(buf); 
    // If we ever switch to a zip-like archive format with the ToC
    // at the end, we can return the correct portion of export data,
    // but for now we must return the entire rest of the file.
    return (buf, error.As(err)!);

}

// Read reads export data from in, decodes it, and returns type
// information for the package.
// The package name is specified by path.
// File position information is added to fset.
//
// Read may inspect and add to the imports map to ensure that references
// within the export data to other packages are consistent.  The caller
// must ensure that imports[path] does not exist, or exists but is
// incomplete (see types.Package.Complete), and Read inserts the
// resulting package into this map entry.
//
// On return, the state of the reader is undefined.
public static (ptr<types.Package>, error) Read(io.Reader @in, ptr<token.FileSet> _addr_fset, map<@string, ptr<types.Package>> imports, @string path) {
    ptr<types.Package> _p0 = default!;
    error _p0 = default!;
    ref token.FileSet fset = ref _addr_fset.val;

    var (data, err) = ioutil.ReadAll(in);
    if (err != null) {
        return (_addr_null!, error.As(fmt.Errorf("reading export data for %q: %v", path, err))!);
    }
    if (bytes.HasPrefix(data, (slice<byte>)"!<arch>")) {
        return (_addr_null!, error.As(fmt.Errorf("can't read export data for %q directly from an archive file (call gcexportdata.NewReader first to extract export data)", path))!);
    }
    if (bytes.HasPrefix(data, (slice<byte>)"package ")) {
        return _addr_gcimporter.ImportData(imports, path, path, bytes.NewReader(data))!;
    }
    if (len(data) > 0 && data[0] == 'i') {
        var (_, pkg, err) = gcimporter.IImportData(fset, imports, data[(int)1..], path);
        return (_addr_pkg!, error.As(err)!);
    }
    (_, pkg, err) = gcimporter.BImportData(fset, imports, data, path);
    return (_addr_pkg!, error.As(err)!);

}

// Write writes encoded type information for the specified package to out.
// The FileSet provides file position information for named objects.
public static error Write(io.Writer @out, ptr<token.FileSet> _addr_fset, ptr<types.Package> _addr_pkg) {
    ref token.FileSet fset = ref _addr_fset.val;
    ref types.Package pkg = ref _addr_pkg.val;

    var (b, err) = gcimporter.IExportData(fset, pkg);
    if (err != null) {
        return error.As(err)!;
    }
    _, err = @out.Write(b);
    return error.As(err)!;

}

} // end gcexportdata_package
