// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package objabi -- go2cs converted at 2020 October 09 05:08:52 UTC
// import "cmd/internal/objabi" ==> using objabi = go.cmd.@internal.objabi_package
// Original source: C:\Go\src\cmd\internal\objabi\line.go
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
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

        // AbsFile returns the absolute filename for file in the given directory,
        // as rewritten by the rewrites argument.
        // For unrewritten paths, AbsFile rewrites a leading $GOROOT prefix to the literal "$GOROOT".
        // If the resulting path is the empty string, the result is "??".
        //
        // The rewrites argument is a ;-separated list of rewrites.
        // Each rewrite is of the form "prefix" or "prefix=>replace",
        // where prefix must match a leading sequence of path elements
        // and is either removed entirely or replaced by the replacement.
        public static @string AbsFile(@string dir, @string file, @string rewrites)
        {
            var abs = file;
            if (dir != "" && !filepath.IsAbs(file))
            {
                abs = filepath.Join(dir, file);
            }

            long start = 0L;
            for (long i = 0L; i <= len(rewrites); i++)
            {
                if (i == len(rewrites) || rewrites[i] == ';')
                {
                    {
                        var (new, ok) = applyRewrite(abs, rewrites[start..i]);

                        if (ok)
                        {
                            abs = new;
                            goto Rewritten;
                        }

                    }

                    start = i + 1L;

                }

            }

            if (hasPathPrefix(abs, GOROOT))
            {
                abs = "$GOROOT" + abs[len(GOROOT)..];
            }

Rewritten:
            if (abs == "")
            {
                abs = "??";
            }

            return abs;

        }

        // applyRewrite applies the rewrite to the path,
        // returning the rewritten path and a boolean
        // indicating whether the rewrite applied at all.
        private static (@string, bool) applyRewrite(@string path, @string rewrite)
        {
            @string _p0 = default;
            bool _p0 = default;

            var prefix = rewrite;
            @string replace = "";
            {
                var j = strings.LastIndex(rewrite, "=>");

                if (j >= 0L)
                {
                    prefix = rewrite[..j];
                    replace = rewrite[j + len("=>")..];

                }

            }


            if (prefix == "" || !hasPathPrefix(path, prefix))
            {
                return (path, false);
            }

            if (len(path) == len(prefix))
            {
                return (replace, true);
            }

            if (replace == "")
            {
                return (path[len(prefix) + 1L..], true);
            }

            return (replace + path[len(prefix)..], true);

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
