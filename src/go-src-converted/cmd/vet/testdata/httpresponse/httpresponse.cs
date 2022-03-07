// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package httpresponse -- go2cs converted at 2022 March 06 23:35:18 UTC
// import "cmd/vet/testdata/httpresponse" ==> using httpresponse = go.cmd.vet.testdata.httpresponse_package
// Original source: C:\Program Files\Go\src\cmd\vet\testdata\httpresponse\httpresponse.go
using log = go.log_package;
using http = go.net.http_package;

namespace go.cmd.vet.testdata;

public static partial class httpresponse_package {

private static void goodHTTPGet() => func((defer, _, _) => {
    var (res, err) = http.Get("http://foo.com");
    if (err != null) {
        log.Fatal(err);
    }
    defer(res.Body.Close());

});

private static void badHTTPGet() => func((defer, _, _) => {
    var (res, err) = http.Get("http://foo.com");
    defer(res.Body.Close()); // ERROR "using res before checking for errors"
    if (err != null) {
        log.Fatal(err);
    }
});

} // end httpresponse_package
