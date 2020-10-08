// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build cmd_go_bootstrap

// This code is compiled only into the bootstrap 'go' binary.
// These stubs avoid importing packages with large dependency
// trees that potentially require C linking,
// like the use of "net/http" in vcs.go.

// package web -- go2cs converted at 2020 October 08 04:34:36 UTC
// import "cmd/go/internal/web" ==> using web = go.cmd.go.@internal.web_package
// Original source: C:\Go\src\cmd\go\internal\web\bootstrap.go
using errors = go.errors_package;
using urlpkg = go.net.url_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class web_package
    {
        private static (ptr<Response>, error) get(SecurityMode security, ptr<urlpkg.URL> _addr_url)
        {
            ptr<Response> _p0 = default!;
            error _p0 = default!;
            ref urlpkg.URL url = ref _addr_url.val;

            return (_addr_null!, error.As(errors.New("no http in bootstrap go command"))!);
        }

        private static bool openBrowser(@string url)
        {
            return false;
        }
    }
}}}}
