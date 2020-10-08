// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package @base -- go2cs converted at 2020 October 08 04:36:58 UTC
// import "cmd/go/internal/base" ==> using @base = go.cmd.go.@internal.@base_package
// Original source: C:\Go\src\cmd\go\internal\base\path.go
using os = go.os_package;
using filepath = go.path.filepath_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class @base_package
    {
        private static @string getwd()
        {
            var (wd, err) = os.Getwd();
            if (err != null)
            {
                Fatalf("cannot determine current directory: %v", err);
            }
            return wd;

        }

        public static var Cwd = getwd();

        // ShortPath returns an absolute or relative name for path, whatever is shorter.
        public static @string ShortPath(@string path)
        {
            {
                var (rel, err) = filepath.Rel(Cwd, path);

                if (err == null && len(rel) < len(path))
                {
                    return rel;
                }

            }

            return path;

        }

        // RelPaths returns a copy of paths with absolute paths
        // made relative to the current directory if they would be shorter.
        public static slice<@string> RelPaths(slice<@string> paths)
        {
            slice<@string> @out = default; 
            // TODO(rsc): Can this use Cwd from above?
            var (pwd, _) = os.Getwd();
            foreach (var (_, p) in paths)
            {
                var (rel, err) = filepath.Rel(pwd, p);
                if (err == null && len(rel) < len(p))
                {
                    p = rel;
                }

                out = append(out, p);

            }
            return out;

        }

        // IsTestFile reports whether the source file is a set of tests and should therefore
        // be excluded from coverage analysis.
        public static bool IsTestFile(@string file)
        { 
            // We don't cover tests, only the code they test.
            return strings.HasSuffix(file, "_test.go");

        }
    }
}}}}
