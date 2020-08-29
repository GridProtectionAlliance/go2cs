// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build cmd_go_bootstrap

// This code is compiled only into the bootstrap 'go' binary.
// These stubs avoid importing packages with large dependency
// trees, like the use of "net/http" in vcs.go.

// package web -- go2cs converted at 2020 August 29 10:00:48 UTC
// import "cmd/go/internal/web" ==> using web = go.cmd.go.@internal.web_package
// Original source: C:\Go\src\cmd\go\internal\web\bootstrap.go
using errors = go.errors_package;
using io = go.io_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class web_package
    {
        private static var errHTTP = errors.New("no http in bootstrap go command");

        public partial struct HTTPError
        {
            public long StatusCode;
        }

        private static @string Error(this ref HTTPError _e) => func(_e, (ref HTTPError e, Defer _, Panic panic, Recover __) =>
        {
            panic("unreachable");
        });

        public static (slice<byte>, error) Get(@string url)
        {
            return (null, errHTTP);
        }

        public static (@string, io.ReadCloser, error) GetMaybeInsecure(@string importPath, SecurityMode security)
        {
            return ("", null, errHTTP);
        }

        public static @string QueryEscape(@string s) => func((_, panic, __) =>
        {
            panic("unreachable");

        });
        public static bool OpenBrowser(@string url) => func((_, panic, __) =>
        {
            panic("unreachable");

        });
    }
}}}}
