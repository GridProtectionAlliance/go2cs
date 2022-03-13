// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package http -- go2cs converted at 2022 March 13 05:37:15 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Program Files\Go\src\net\http\jar.go
namespace go.net;

using url = net.url_package;


// A CookieJar manages storage and use of cookies in HTTP requests.
//
// Implementations of CookieJar must be safe for concurrent use by multiple
// goroutines.
//
// The net/http/cookiejar package provides a CookieJar implementation.

public static partial class http_package {

public partial interface CookieJar {
    slice<ptr<Cookie>> SetCookies(ptr<url.URL> u, slice<ptr<Cookie>> cookies); // Cookies returns the cookies to send in a request for the given URL.
// It is up to the implementation to honor the standard cookie use
// restrictions such as in RFC 6265.
    slice<ptr<Cookie>> Cookies(ptr<url.URL> u);
}

} // end http_package
