// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package http -- go2cs converted at 2020 October 08 03:40:13 UTC
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
        public static readonly @string MethodGet = (@string)"GET";
        public static readonly @string MethodHead = (@string)"HEAD";
        public static readonly @string MethodPost = (@string)"POST";
        public static readonly @string MethodPut = (@string)"PUT";
        public static readonly @string MethodPatch = (@string)"PATCH"; // RFC 5789
        public static readonly @string MethodDelete = (@string)"DELETE";
        public static readonly @string MethodConnect = (@string)"CONNECT";
        public static readonly @string MethodOptions = (@string)"OPTIONS";
        public static readonly @string MethodTrace = (@string)"TRACE";

    }
}}
