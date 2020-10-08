// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package load -- go2cs converted at 2020 October 08 04:34:22 UTC
// import "cmd/go/internal/load" ==> using load = go.cmd.go.@internal.load_package
// Original source: C:\Go\src\cmd\go\internal\load\path.go
using filepath = go.path.filepath_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class load_package
    {
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
