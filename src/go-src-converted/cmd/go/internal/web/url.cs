// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package web -- go2cs converted at 2020 October 08 04:34:39 UTC
// import "cmd/go/internal/web" ==> using web = go.cmd.go.@internal.web_package
// Original source: C:\Go\src\cmd\go\internal\web\url.go
using errors = go.errors_package;
using url = go.net.url_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class web_package
    {
        // TODO(golang.org/issue/32456): If accepted, move these functions into the
        // net/url package.
        private static var errNotAbsolute = errors.New("path is not absolute");

        private static (@string, error) urlToFilePath(ptr<url.URL> _addr_u)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref url.URL u = ref _addr_u.val;

            if (u.Scheme != "file")
            {
                return ("", error.As(errors.New("non-file URL"))!);
            }

            Func<@string, (@string, error)> checkAbs = path =>
            {
                if (!filepath.IsAbs(path))
                {
                    return ("", error.As(errNotAbsolute)!);
                }

                return (path, error.As(null!)!);

            }
;

            if (u.Path == "")
            {
                if (u.Host != "" || u.Opaque == "")
                {
                    return ("", error.As(errors.New("file URL missing path"))!);
                }

                return checkAbs(filepath.FromSlash(u.Opaque));

            }

            var (path, err) = convertFileURLPath(u.Host, u.Path);
            if (err != null)
            {
                return (path, error.As(err)!);
            }

            return checkAbs(path);

        }

        private static (ptr<url.URL>, error) urlFromFilePath(@string path)
        {
            ptr<url.URL> _p0 = default!;
            error _p0 = default!;

            if (!filepath.IsAbs(path))
            {
                return (_addr_null!, error.As(errNotAbsolute)!);
            } 

            // If path has a Windows volume name, convert the volume to a host and prefix
            // per https://blogs.msdn.microsoft.com/ie/2006/12/06/file-uris-in-windows/.
            {
                var vol = filepath.VolumeName(path);

                if (vol != "")
                {
                    if (strings.HasPrefix(vol, "\\\\"))
                    {
                        path = filepath.ToSlash(path[2L..]);
                        var i = strings.IndexByte(path, '/');

                        if (i < 0L)
                        { 
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

                // /path/to/file
                // becomes
                // file:///path/to/file

            } 

            // /path/to/file
            // becomes
            // file:///path/to/file
            return (addr(new url.URL(Scheme:"file",Path:filepath.ToSlash(path),)), error.As(null!)!);

        }
    }
}}}}
