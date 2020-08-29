// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package objabi -- go2cs converted at 2020 August 29 08:46:20 UTC
// import "cmd/internal/objabi" ==> using objabi = go.cmd.@internal.objabi_package
// Original source: C:\Go\src\cmd\internal\objabi\line.go
using os = go.os_package;
using filepath = go.path.filepath_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class objabi_package
    {
        // WorkingDir returns the current working directory
        // (or "/???" if the directory cannot be identified),
        // with "/" as separator.
        public static @string WorkingDir()
        {
            @string path = default;
            path, _ = os.Getwd();
            if (path == "")
            {
                path = "/???";
            }
            return filepath.ToSlash(path);
        }

        // AbsFile returns the absolute filename for file in the given directory.
        // It also removes a leading pathPrefix, or else rewrites a leading $GOROOT
        // prefix to the literal "$GOROOT".
        // If the resulting path is the empty string, the result is "??".
        public static @string AbsFile(@string dir, @string file, @string pathPrefix)
        {
            var abs = file;
            if (dir != "" && !filepath.IsAbs(file))
            {
                abs = filepath.Join(dir, file);
            }
            if (pathPrefix != "" && hasPathPrefix(abs, pathPrefix))
            {
                if (abs == pathPrefix)
                {
                    abs = "";
                }
                else
                {
                    abs = abs[len(pathPrefix) + 1L..];
                }
            }
            else if (hasPathPrefix(abs, GOROOT))
            {
                abs = "$GOROOT" + abs[len(GOROOT)..];
            }
            if (abs == "")
            {
                abs = "??";
            }
            return abs;
        }

        // Does s have t as a path prefix?
        // That is, does s == t or does s begin with t followed by a slash?
        // For portability, we allow ASCII case folding, so that hasPathPrefix("a/b/c", "A/B") is true.
        // Similarly, we allow slash folding, so that hasPathPrefix("a/b/c", "a\\b") is true.
        // We do not allow full Unicode case folding, for fear of causing more confusion
        // or harm than good. (For an example of the kinds of things that can go wrong,
        // see http://article.gmane.org/gmane.linux.kernel/1853266.)
        private static bool hasPathPrefix(@string s, @string t)
        {
            if (len(t) > len(s))
            {
                return false;
            }
            long i = default;
            for (i = 0L; i < len(t); i++)
            {
                var cs = int(s[i]);
                var ct = int(t[i]);
                if ('A' <= cs && cs <= 'Z')
                {
                    cs += 'a' - 'A';
                }
                if ('A' <= ct && ct <= 'Z')
                {
                    ct += 'a' - 'A';
                }
                if (cs == '\\')
                {
                    cs = '/';
                }
                if (ct == '\\')
                {
                    ct = '/';
                }
                if (cs != ct)
                {
                    return false;
                }
            }

            return i >= len(s) || s[i] == '/' || s[i] == '\\';
        }
    }
}}}
