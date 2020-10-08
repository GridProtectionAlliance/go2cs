// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package exec -- go2cs converted at 2020 October 08 03:41:19 UTC
// import "os/exec" ==> using exec = go.os.exec_package
// Original source: C:\Go\src\os\exec\lp_unix.go
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
        public static var ErrNotFound = errors.New("executable file not found in $PATH");

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
        // directories named by the PATH environment variable.
        // If file contains a slash, it is tried directly and the PATH is not consulted.
        // The result may be an absolute path or a path relative to the current directory.
        public static (@string, error) LookPath(@string file)
        {
            @string _p0 = default;
            error _p0 = default!;
 
            // NOTE(rsc): I wish we could use the Plan 9 behavior here
            // (only bypass the path if file begins with / or ./ or ../)
            // but that would not match all the Unix shells.

            if (strings.Contains(file, "/"))
            {
                var err = findExecutable(file);
                if (err == null)
                {
                    return (file, error.As(null!)!);
                }

                return ("", error.As(addr(new Error(file,err))!)!);

            }

            var path = os.Getenv("PATH");
            foreach (var (_, dir) in filepath.SplitList(path))
            {
                if (dir == "")
                { 
                    // Unix shell semantics: path element "" means "."
                    dir = ".";

                }

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
