// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build cmd_go_bootstrap
// +build cmd_go_bootstrap

// This code is compiled only into the bootstrap 'go' binary.
// These stubs avoid importing packages with large dependency
// trees that potentially require C linking,
// like the use of "net/http" in vcs.go.

// package web -- go2cs converted at 2022 March 13 06:30:35 UTC
// import "cmd/go/internal/web" ==> using web = go.cmd.go.@internal.web_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\web\bootstrap.go
namespace go.cmd.go.@internal;

using errors = errors_package;
using urlpkg = net.url_package;

public static partial class web_package {

private static (ptr<Response>, error) get(SecurityMode security, ptr<urlpkg.URL> _addr_url) {
    ptr<Response> _p0 = default!;
    error _p0 = default!;
    ref urlpkg.URL url = ref _addr_url.val;

    return (_addr_null!, error.As(errors.New("no http in bootstrap go command"))!);
}

private static bool openBrowser(@string url) {
    return false;
}

} // end web_package
