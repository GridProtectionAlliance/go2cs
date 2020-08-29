// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package version implements the ``go version'' command.
// package version -- go2cs converted at 2020 August 29 10:00:41 UTC
// import "cmd/go/internal/version" ==> using version = go.cmd.go.@internal.version_package
// Original source: C:\Go\src\cmd\go\internal\version\version.go
using fmt = go.fmt_package;
using runtime = go.runtime_package;

using @base = go.cmd.go.@internal.@base_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class version_package
    {
        public static base.Command CmdVersion = ref new base.Command(Run:runVersion,UsageLine:"version",Short:"print Go version",Long:`Version prints the Go version, as reported by runtime.Version.`,);

        private static void runVersion(ref base.Command cmd, slice<@string> args)
        {
            if (len(args) != 0L)
            {
                cmd.Usage();
            }
            fmt.Printf("go version %s %s/%s\n", runtime.Version(), runtime.GOOS, runtime.GOARCH);
        }
    }
}}}}
