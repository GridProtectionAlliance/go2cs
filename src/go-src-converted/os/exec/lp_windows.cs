// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package exec -- go2cs converted at 2020 August 29 08:24:40 UTC
// import "os/exec" ==> using exec = go.os.exec_package
// Original source: C:\Go\src\os\exec\lp_windows.go
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
        public static var ErrNotFound = errors.New("executable file not found in %PATH%");

        private static error chkStat(@string file)
        {
            var (d, err) = os.Stat(file);
            if (err != null)
            {
                return error.As(err);
            }
            if (d.IsDir())
            {
                return error.As(os.ErrPermission);
            }
            return error.As(null);
        }

        private static bool hasExt(@string file)
        {
            var i = strings.LastIndex(file, ".");
            if (i < 0L)
            {
                return false;
            }
            return strings.LastIndexAny(file, ":\\/") < i;
        }

        private static (@string, error) findExecutable(@string file, slice<@string> exts)
        {
            if (len(exts) == 0L)
            {
                return (file, chkStat(file));
            }
            if (hasExt(file))
            {
                if (chkStat(file) == null)
                {
                    return (file, null);
                }
            }
            foreach (var (_, e) in exts)
            {
                {
                    var f = file + e;

                    if (chkStat(f) == null)
                    {
                        return (f, null);
                    }

                }
            }
            return ("", os.ErrNotExist);
        }

        // LookPath searches for an executable binary named file
        // in the directories named by the PATH environment variable.
        // If file contains a slash, it is tried directly and the PATH is not consulted.
        // LookPath also uses PATHEXT environment variable to match
        // a suitable candidate.
        // The result may be an absolute path or a path relative to the current directory.
        public static (@string, error) LookPath(@string file)
        {
            slice<@string> exts = default;
            var x = os.Getenv("PATHEXT");
            if (x != "")
            {
                foreach (var (_, e) in strings.Split(strings.ToLower(x), ";"))
                {
                    if (e == "")
                    {
                        continue;
                    }
                    if (e[0L] != '.')
                    {
                        e = "." + e;
                    }
                    exts = append(exts, e);
                }
            else
            }            {
                exts = new slice<@string>(new @string[] { ".com", ".exe", ".bat", ".cmd" });
            }
            if (strings.ContainsAny(file, ":\\/"))
            {
                {
                    var f__prev2 = f;

                    var (f, err) = findExecutable(file, exts);

                    if (err == null)
                    {
                        return (f, null);
                    }
                    else
                    {
                        return ("", ref new Error(file,err));
                    }

                    f = f__prev2;

                }
            }
            {
                var f__prev1 = f;

                (f, err) = findExecutable(filepath.Join(".", file), exts);

                if (err == null)
                {
                    return (f, null);
                }

                f = f__prev1;

            }
            var path = os.Getenv("path");
            foreach (var (_, dir) in filepath.SplitList(path))
            {
                {
                    var f__prev1 = f;

                    (f, err) = findExecutable(filepath.Join(dir, file), exts);

                    if (err == null)
                    {
                        return (f, null);
                    }

                    f = f__prev1;

                }
            }
            return ("", ref new Error(file,ErrNotFound));
        }
    }
}}
