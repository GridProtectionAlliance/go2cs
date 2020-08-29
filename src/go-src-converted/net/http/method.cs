// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package http -- go2cs converted at 2020 August 29 08:33:24 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Go\src\net\http\method.go

using static go.builtin;

namespace go {
namespace net
{
    public static partial class http_package
    {
        // Common HTTP methods.
        //
        // Unless otherwise noted, these are defined in RFC 7231 section 4.3.
        public static readonly @string MethodGet = "GET";
        public static readonly @string MethodHead = "HEAD";
        public static readonly @string MethodPost = "POST";
        public static readonly @string MethodPut = "PUT";
        public static readonly @string MethodPatch = "PATCH"; // RFC 5789
        public static readonly @string MethodDelete = "DELETE";
        public static readonly @string MethodConnect = "CONNECT";
        public static readonly @string MethodOptions = "OPTIONS";
        public static readonly @string MethodTrace = "TRACE";
    }
}}
