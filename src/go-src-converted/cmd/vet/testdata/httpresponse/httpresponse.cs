// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package httpresponse -- go2cs converted at 2022 March 13 06:42:52 UTC
// import "cmd/vet/testdata/httpresponse" ==> using httpresponse = go.cmd.vet.testdata.httpresponse_package
// Original source: C:\Program Files\Go\src\cmd\vet\testdata\httpresponse\httpresponse.go
namespace go.cmd.vet.testdata;

using log = log_package;
using http = net.http_package;

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
