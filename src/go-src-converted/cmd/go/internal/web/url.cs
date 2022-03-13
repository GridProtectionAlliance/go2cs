// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package web -- go2cs converted at 2022 March 13 06:30:38 UTC
// import "cmd/go/internal/web" ==> using web = go.cmd.go.@internal.web_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\web\url.go
namespace go.cmd.go.@internal;

using errors = errors_package;
using url = net.url_package;
using filepath = path.filepath_package;
using strings = strings_package;


// TODO(golang.org/issue/32456): If accepted, move these functions into the
// net/url package.


using System;public static partial class web_package {

private static var errNotAbsolute = errors.New("path is not absolute");

private static (@string, error) urlToFilePath(ptr<url.URL> _addr_u) {
    @string _p0 = default;
    error _p0 = default!;
    ref url.URL u = ref _addr_u.val;

    if (u.Scheme != "file") {
        return ("", error.As(errors.New("non-file URL"))!);
    }
    Func<@string, (@string, error)> checkAbs = path => {
        if (!filepath.IsAbs(path)) {
            return ("", error.As(errNotAbsolute)!);
        }
        return (path, error.As(null!)!);
    };

    if (u.Path == "") {
        if (u.Host != "" || u.Opaque == "") {
            return ("", error.As(errors.New("file URL missing path"))!);
        }
        return checkAbs(filepath.FromSlash(u.Opaque));
    }
    var (path, err) = convertFileURLPath(u.Host, u.Path);
    if (err != null) {
        return (path, error.As(err)!);
    }
    return checkAbs(path);
}

private static (ptr<url.URL>, error) urlFromFilePath(@string path) {
    ptr<url.URL> _p0 = default!;
    error _p0 = default!;

    if (!filepath.IsAbs(path)) {
        return (_addr_null!, error.As(errNotAbsolute)!);
    }
    {
        var vol = filepath.VolumeName(path);

        if (vol != "") {
            if (strings.HasPrefix(vol, "\\\\")) {
                path = filepath.ToSlash(path[(int)2..]);
                var i = strings.IndexByte(path, '/');

                if (i < 0) { 
                    // A degenerate case.
                    // \\host.example.com (without a share name)
                    // becomes
                    // file://host.example.com/
                    return (addr(new url.URL(Scheme:"file",Host:path,Path:"/",)), error.As(null!)!);
                } 

                // \\host.example.com\Share\path\to\file
                // becomes
                // file://host.example.com/Share/path/to/file
                return (addr(new url.URL(Scheme:"file",Host:path[:i],Path:filepath.ToSlash(path[i:]),)), error.As(null!)!);
            } 

            // C:\path\to\file
            // becomes
            // file:///C:/path/to/file
            return (addr(new url.URL(Scheme:"file",Path:"/"+filepath.ToSlash(path),)), error.As(null!)!);
        }
    } 

    // /path/to/file
    // becomes
    // file:///path/to/file
    return (addr(new url.URL(Scheme:"file",Path:filepath.ToSlash(path),)), error.As(null!)!);
}

} // end web_package
