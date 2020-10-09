// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !js !wasm

// package http -- go2cs converted at 2020 October 09 04:57:51 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Go\src\net\http\roundtrip.go

using static go.builtin;

namespace go {
namespace net
{
    public static partial class http_package
    {
        // RoundTrip implements the RoundTripper interface.
        //
        // For higher-level HTTP client support (such as handling of cookies
        // and redirects), see Get, Post, and the Client type.
        //
        // Like the RoundTripper interface, the error types returned
        // by RoundTrip are unspecified.
        private static (ptr<Response>, error) RoundTrip(this ptr<Transport> _addr_t, ptr<Request> _addr_req)
        {
            ptr<Response> _p0 = default!;
            error _p0 = default!;
            ref Transport t = ref _addr_t.val;
            ref Request req = ref _addr_req.val;

            return _addr_t.roundTrip(req)!;
        }
    }
}}
