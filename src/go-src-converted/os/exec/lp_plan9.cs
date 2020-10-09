// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package exec -- go2cs converted at 2020 October 09 04:58:43 UTC
// import "os/exec" ==> using exec = go.os.exec_package
// Original source: C:\Go\src\os\exec\lp_plan9.go
using errors = go.errors_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace os
{
    public static partial class exec_package
    {
        // ErrNotFound is the error resulting if a path search failed to find an executable file.
        public static var ErrNotFound = errors.New("executable file not found in $path");

        private static error findExecutable(@string file)
        {
            var (d, err) = os.Stat(file);
            if (err != null)
            {
                return error.As(err)!;
            }

            {
                var m = d.Mode();

                if (!m.IsDir() && m & 0111L != 0L)
                {
                    return error.As(null!)!;
                }

            }

            return error.As(os.ErrPermission)!;

        }

        // LookPath searches for an executable named file in the
        // directories named by the path environment variable.
        // If file begins with "/", "#", "./", or "../", it is tried
        // directly and the path is not consulted.
        // The result may be an absolute path or a path relative to the current directory.
        public static (@string, error) LookPath(@string file)
        {
            @string _p0 = default;
            error _p0 = default!;
 
            // skip the path lookup for these prefixes
            @string skip = new slice<@string>(new @string[] { "/", "#", "./", "../" });

            foreach (var (_, p) in skip)
            {
                if (strings.HasPrefix(file, p))
                {
                    var err = findExecutable(file);
                    if (err == null)
                    {
                        return (file, error.As(null!)!);
                    }

                    return ("", error.As(addr(new Error(file,err))!)!);

                }

            }
            var path = os.Getenv("path");
            foreach (var (_, dir) in filepath.SplitList(path))
            {
                path = filepath.Join(dir, file);
                {
                    var err__prev1 = err;

                    err = findExecutable(path);

                    if (err == null)
                    {
                        return (path, error.As(null!)!);
                    }

                    err = err__prev1;

                }

            }
            return ("", error.As(addr(new Error(file,ErrNotFound))!)!);

        }
    }
}}
