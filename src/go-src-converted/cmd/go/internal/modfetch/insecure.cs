// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modfetch -- go2cs converted at 2020 October 09 05:47:19 UTC
// import "cmd/go/internal/modfetch" ==> using modfetch = go.cmd.go.@internal.modfetch_package
// Original source: C:\Go\src\cmd\go\internal\modfetch\insecure.go
using cfg = go.cmd.go.@internal.cfg_package;
using get = go.cmd.go.@internal.get_package;
using str = go.cmd.go.@internal.str_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modfetch_package
    {
        // allowInsecure reports whether we are allowed to fetch this path in an insecure manner.
        private static bool allowInsecure(@string path)
        {
            return get.Insecure || str.GlobsMatchPath(cfg.GOINSECURE, path);
        }
    }
}}}}
