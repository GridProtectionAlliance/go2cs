// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package web -- go2cs converted at 2020 October 09 05:46:04 UTC
// import "cmd/go/internal/web" ==> using web = go.cmd.go.@internal.web_package
// Original source: C:\Go\src\cmd\go\internal\web\url_windows.go
using errors = go.errors_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class web_package
    {
        private static (@string, error) convertFileURLPath(@string host, @string path)
        {
            @string _p0 = default;
            error _p0 = default!;

            if (len(path) == 0L || path[0L] != '/')
            {
                return ("", error.As(errNotAbsolute)!);
            }
            path = filepath.FromSlash(path); 

            // We interpret Windows file URLs per the description in
            // https://blogs.msdn.microsoft.com/ie/2006/12/06/file-uris-in-windows/.

            // The host part of a file URL (if any) is the UNC volume name,
            // but RFC 8089 reserves the authority "localhost" for the local machine.
            if (host != "" && host != "localhost")
            { 
                // A common "legacy" format omits the leading slash before a drive letter,
                // encoding the drive letter as the host instead of part of the path.
                // (See https://blogs.msdn.microsoft.com/freeassociations/2005/05/19/the-bizarre-and-unhappy-story-of-file-urls/.)
                // We do not support that format, but we should at least emit a more
                // helpful error message for it.
                if (filepath.VolumeName(host) != "")
                {
                    return ("", error.As(errors.New("file URL encodes volume in host field: too few slashes?"))!);
                }
                return ("\\\\" + host + path, error.As(null!)!);

            }
            {
                var vol = filepath.VolumeName(path[1L..]);

                if (vol == "" || strings.HasPrefix(vol, "\\\\"))
                {
                    return ("", error.As(errors.New("file URL missing drive letter"))!);
                }
            }

            return (path[1L..], error.As(null!)!);

        }
    }
}}}}
