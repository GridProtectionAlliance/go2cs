// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !windows

// package web -- go2cs converted at 2020 October 08 04:34:39 UTC
// import "cmd/go/internal/web" ==> using web = go.cmd.go.@internal.web_package
// Original source: C:\Go\src\cmd\go\internal\web\url_other.go
using errors = go.errors_package;
using filepath = go.path.filepath_package;
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

            switch (host)
            {
                case "": 

                case "localhost": 
                    break;
                default: 
                    return ("", error.As(errors.New("file URL specifies non-local host"))!);
                    break;
            }
            return (filepath.FromSlash(path), error.As(null!)!);

        }
    }
}}}}
