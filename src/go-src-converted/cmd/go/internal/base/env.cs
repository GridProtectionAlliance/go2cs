// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package @base -- go2cs converted at 2020 October 09 05:48:04 UTC
// import "cmd/go/internal/base" ==> using @base = go.cmd.go.@internal.@base_package
// Original source: C:\Go\src\cmd\go\internal\base\env.go

using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class @base_package
    {
        // AppendPWD returns the result of appending PWD=dir to the environment base.
        //
        // The resulting environment makes os.Getwd more efficient for a subprocess
        // running in dir.
        public static slice<@string> AppendPWD(slice<@string> @base, @string dir)
        { 
            // Internally we only use absolute paths, so dir is absolute.
            // Even if dir is not absolute, no harm done.
            return append(base, "PWD=" + dir);

        }
    }
}}}}
