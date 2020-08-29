// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package load -- go2cs converted at 2020 August 29 10:00:52 UTC
// import "cmd/go/internal/load" ==> using load = go.cmd.go.@internal.load_package
// Original source: C:\Go\src\cmd\go\internal\load\path.go
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class load_package
    {
        // hasSubdir reports whether dir is a subdirectory of
        // (possibly multiple levels below) root.
        // If so, it sets rel to the path fragment that must be
        // appended to root to reach dir.
        private static (@string, bool) hasSubdir(@string root, @string dir)
        {
            {
                var p__prev1 = p;

                var (p, err) = filepath.EvalSymlinks(root);

                if (err == null)
                {
                    root = p;
                }
                p = p__prev1;

            }
            {
                var p__prev1 = p;

                (p, err) = filepath.EvalSymlinks(dir);

                if (err == null)
                {
                    dir = p;
                }
                p = p__prev1;

            }
            const var sep = string(filepath.Separator);

            root = filepath.Clean(root);
            if (!strings.HasSuffix(root, sep))
            {
                root += sep;
            }
            dir = filepath.Clean(dir);
            if (!strings.HasPrefix(dir, root))
            {
                return ("", false);
            }
            return (filepath.ToSlash(dir[len(root)..]), true);
        }

        // hasPathPrefix reports whether the path s begins with the
        // elements in prefix.
        private static bool hasPathPrefix(@string s, @string prefix)
        {

            if (len(s) == len(prefix)) 
                return s == prefix;
            else if (len(s) > len(prefix)) 
                if (prefix != "" && prefix[len(prefix) - 1L] == '/')
                {
                    return strings.HasPrefix(s, prefix);
                }
                return s[len(prefix)] == '/' && s[..len(prefix)] == prefix;
            else 
                return false;
                    }

        // expandPath returns the symlink-expanded form of path.
        private static @string expandPath(@string p)
        {
            var (x, err) = filepath.EvalSymlinks(p);
            if (err == null)
            {
                return x;
            }
            return p;
        }
    }
}}}}
