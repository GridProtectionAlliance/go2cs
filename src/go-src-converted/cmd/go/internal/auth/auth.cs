// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package auth provides access to user-provided authentication credentials.
// package auth -- go2cs converted at 2022 March 06 23:17:19 UTC
// import "cmd/go/internal/auth" ==> using auth = go.cmd.go.@internal.auth_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\auth\auth.go
using http = go.net.http_package;

namespace go.cmd.go.@internal;

public static partial class auth_package {

    // AddCredentials fills in the user's credentials for req, if any.
    // The return value reports whether any matching credentials were found.
public static bool AddCredentials(ptr<http.Request> _addr_req) {
    bool added = default;
    ref http.Request req = ref _addr_req.val;

    var host = req.URL.Hostname(); 

    // TODO(golang.org/issue/26232): Support arbitrary user-provided credentials.
    netrcOnce.Do(readNetrc);
    foreach (var (_, l) in netrc) {
        if (l.machine == host) {
            req.SetBasicAuth(l.login, l.password);
            return true;
        }
    }    return false;

}

} // end auth_package
