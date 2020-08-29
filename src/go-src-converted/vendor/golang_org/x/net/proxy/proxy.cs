// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package proxy provides support for a variety of protocols to proxy network
// data.
// package proxy -- go2cs converted at 2020 August 29 10:12:26 UTC
// import "vendor/golang_org/x/net/proxy" ==> using proxy = go.vendor.golang_org.x.net.proxy_package
// Original source: C:\Go\src\vendor\golang_org\x\net\proxy\proxy.go
// import "golang.org/x/net/proxy"

using errors = go.errors_package;
using net = go.net_package;
using url = go.net.url_package;
using os = go.os_package;
using static go.builtin;
using System;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class proxy_package
    {
        // A Dialer is a means to establish a connection.
        public partial interface Dialer
        {
            (net.Conn, error) Dial(@string network, @string addr);
        }

        // Auth contains authentication parameters that specific Dialers may require.
        public partial struct Auth
        {
            public @string User;
            public @string Password;
        }

        // FromEnvironment returns the dialer specified by the proxy related variables in
        // the environment.
        public static Dialer FromEnvironment()
        {
            var allProxy = os.Getenv("all_proxy");
            if (len(allProxy) == 0L)
            {
                return Direct;
            }
            var (proxyURL, err) = url.Parse(allProxy);
            if (err != null)
            {
                return Direct;
            }
            var (proxy, err) = FromURL(proxyURL, Direct);
            if (err != null)
            {
                return Direct;
            }
            var noProxy = os.Getenv("no_proxy");
            if (len(noProxy) == 0L)
            {
                return proxy;
            }
            var perHost = NewPerHost(proxy, Direct);
            perHost.AddFromString(noProxy);
            return perHost;
        }

        // proxySchemes is a map from URL schemes to a function that creates a Dialer
        // from a URL with such a scheme.
        private static map<@string, Func<ref url.URL, Dialer, (Dialer, error)>> proxySchemes = default;

        // RegisterDialerType takes a URL scheme and a function to generate Dialers from
        // a URL with that scheme and a forwarding Dialer. Registered schemes are used
        // by FromURL.
        public static (Dialer, error) RegisterDialerType(@string scheme, Func<ref url.URL, Dialer, (Dialer, error)> f)
        {
            if (proxySchemes == null)
            {
                proxySchemes = make_map<@string, Func<ref url.URL, Dialer, (Dialer, error)>>();
            }
            proxySchemes[scheme] = f;
        }

        // FromURL returns a Dialer given a URL specification and an underlying
        // Dialer for it to make network requests.
        public static (Dialer, error) FromURL(ref url.URL u, Dialer forward)
        {
            ref Auth auth = default;
            if (u.User != null)
            {
                auth = @new<Auth>();
                auth.User = u.User.Username();
                {
                    var (p, ok) = u.User.Password();

                    if (ok)
                    {
                        auth.Password = p;
                    }

                }
            }
            switch (u.Scheme)
            {
                case "socks5": 
                    return SOCKS5("tcp", u.Host, auth, forward);
                    break;
            } 

            // If the scheme doesn't match any of the built-in schemes, see if it
            // was registered by another package.
            if (proxySchemes != null)
            {
                {
                    var (f, ok) = proxySchemes[u.Scheme];

                    if (ok)
                    {
                        return f(u, forward);
                    }

                }
            }
            return (null, errors.New("proxy: unknown scheme: " + u.Scheme));
        }
    }
}}}}}
