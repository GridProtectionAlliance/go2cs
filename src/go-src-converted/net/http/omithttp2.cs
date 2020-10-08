// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build nethttpomithttp2

// package http -- go2cs converted at 2020 October 08 03:40:13 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Go\src\net\http\omithttp2.go
using errors = go.errors_package;
using sync = go.sync_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go {
namespace net
{
    public static partial class http_package
    {
        private static void init()
        {
            omitBundledHTTP2 = true;
        }

        private static readonly @string noHTTP2 = (@string)"no bundled HTTP/2"; // should never see this

 // should never see this

        private static var http2errRequestCanceled = errors.New("net/http: request canceled");

        private static long http2goAwayTimeout = 1L * time.Second;

        private static readonly @string http2NextProtoTLS = (@string)"h2";



        private partial struct http2Transport
        {
            public uint MaxHeaderListSize;
        }

        private static (ptr<Response>, error) RoundTrip(this ptr<http2Transport> _addr__p0, ptr<Request> _addr__p0) => func((_, panic, __) =>
        {
            ptr<Response> _p0 = default!;
            error _p0 = default!;
            ref http2Transport _p0 = ref _addr__p0.val;
            ref Request _p0 = ref _addr__p0.val;

            panic(noHTTP2);
        });
        private static void CloseIdleConnections(this ptr<http2Transport> _addr__p0)
        {
            ref http2Transport _p0 = ref _addr__p0.val;

        }

        private partial struct http2erringRoundTripper
        {
            public error err;
        }

        private static (ptr<Response>, error) RoundTrip(this http2erringRoundTripper _p0, ptr<Request> _addr__p0) => func((_, panic, __) =>
        {
            ptr<Response> _p0 = default!;
            error _p0 = default!;
            ref Request _p0 = ref _addr__p0.val;

            panic(noHTTP2);
        });

        private partial struct http2noDialH2RoundTripper
        {
        }

        private static (ptr<Response>, error) RoundTrip(this http2noDialH2RoundTripper _p0, ptr<Request> _addr__p0) => func((_, panic, __) =>
        {
            ptr<Response> _p0 = default!;
            error _p0 = default!;
            ref Request _p0 = ref _addr__p0.val;

            panic(noHTTP2);
        });

        private partial struct http2noDialClientConnPool
        {
            public http2clientConnPool http2clientConnPool;
        }

        private partial struct http2clientConnPool
        {
            public ptr<sync.Mutex> mu;
            public map<@string, slice<object>> conns;
        }

        private static (ptr<http2Transport>, error) http2configureTransport(ptr<Transport> _addr__p0) => func((_, panic, __) =>
        {
            ptr<http2Transport> _p0 = default!;
            error _p0 = default!;
            ref Transport _p0 = ref _addr__p0.val;

            panic(noHTTP2);
        });

        private static bool http2isNoCachedConnError(error err)
        {
            return ok;
        }

        private partial struct http2Server
        {
            public Func<http2WriteScheduler> NewWriteScheduler;
        }

        private partial interface http2WriteScheduler
        {
        }

        private static http2WriteScheduler http2NewPriorityWriteScheduler(object _p0) => func((_, panic, __) =>
        {
            panic(noHTTP2);
        });

        private static error http2ConfigureServer(ptr<Server> _addr_s, ptr<http2Server> _addr_conf) => func((_, panic, __) =>
        {
            ref Server s = ref _addr_s.val;
            ref http2Server conf = ref _addr_conf.val;

            panic(noHTTP2);
        });

        private static http2noCachedConnError http2ErrNoCachedConn = new http2noCachedConnError();

        private partial struct http2noCachedConnError
        {
        }

        private static void IsHTTP2NoCachedConnError(this http2noCachedConnError _p0)
        {
        }

        private static @string Error(this http2noCachedConnError _p0)
        {
            return "http2: no cached connection was available";
        }
    }
}}
