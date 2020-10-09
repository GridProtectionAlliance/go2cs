// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cgo -- go2cs converted at 2020 October 09 06:03:52 UTC
// import "golang.org/x/tools/go/internal/cgo" ==> using cgo = go.golang.org.x.tools.go.@internal.cgo_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\internal\cgo\cgo_pkgconfig.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using build = go.go.build_package;
using exec = go.os.exec_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace @internal
{
    public static partial class cgo_package
    {
        // pkgConfig runs pkg-config with the specified arguments and returns the flags it prints.
        private static (slice<@string>, error) pkgConfig(@string mode, slice<@string> pkgs)
        {
            slice<@string> flags = default;
            error err = default!;

            var cmd = exec.Command("pkg-config", append(new slice<@string>(new @string[] { mode }), pkgs));
            var (out, err) = cmd.CombinedOutput();
            if (err != null)
            {
                var s = fmt.Sprintf("%s failed: %v", strings.Join(cmd.Args, " "), err);
                if (len(out) > 0L)
                {
                    s = fmt.Sprintf("%s: %s", s, out);
                }
                return (null, error.As(errors.New(s))!);

            }
            if (len(out) > 0L)
            {
                flags = strings.Fields(string(out));
            }
            return ;

        }

        // pkgConfigFlags calls pkg-config if needed and returns the cflags
        // needed to build the package.
        private static (slice<@string>, error) pkgConfigFlags(ptr<build.Package> _addr_p)
        {
            slice<@string> cflags = default;
            error err = default!;
            ref build.Package p = ref _addr_p.val;

            if (len(p.CgoPkgConfig) == 0L)
            {
                return (null, error.As(null!)!);
            }

            return pkgConfig("--cflags", p.CgoPkgConfig);

        }
    }
}}}}}}
