// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package httpproxy provides support for HTTP proxy determination
// based on environment variables, as provided by net/http's
// ProxyFromEnvironment function.
//
// The API is not subject to the Go 1 compatibility promise and may change at
// any time.
// package httpproxy -- go2cs converted at 2020 October 09 04:58:17 UTC
// import "golang.org/x/net/http/httpproxy" ==> using httpproxy = go.golang.org.x.net.http.httpproxy_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\http\httpproxy\proxy.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using net = go.net_package;
using url = go.net.url_package;
using os = go.os_package;
using strings = go.strings_package;
using utf8 = go.unicode.utf8_package;

using idna = go.golang.org.x.net.idna_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace net {
namespace http
{
    public static partial class httpproxy_package
    {
        // Config holds configuration for HTTP proxy settings. See
        // FromEnvironment for details.
        public partial struct Config
        {
            public @string HTTPProxy; // HTTPSProxy represents the HTTPS_PROXY or https_proxy
// environment variable. It will be used as the proxy URL for
// HTTPS requests unless overridden by NoProxy.
            public @string HTTPSProxy; // NoProxy represents the NO_PROXY or no_proxy environment
// variable. It specifies a string that contains comma-separated values
// specifying hosts that should be excluded from proxying. Each value is
// represented by an IP address prefix (1.2.3.4), an IP address prefix in
// CIDR notation (1.2.3.4/8), a domain name, or a special DNS label (*).
// An IP address prefix and domain name can also include a literal port
// number (1.2.3.4:80).
// A domain name matches that name and all subdomains. A domain name with
// a leading "." matches subdomains only. For example "foo.com" matches
// "foo.com" and "bar.foo.com"; ".y.com" matches "x.y.com" but not "y.com".
// A single asterisk (*) indicates that no proxying should be done.
// A best effort is made to parse the string and errors are
// ignored.
            public @string NoProxy; // CGI holds whether the current process is running
// as a CGI handler (FromEnvironment infers this from the
// presence of a REQUEST_METHOD environment variable).
// When this is set, ProxyForURL will return an error
// when HTTPProxy applies, because a client could be
// setting HTTP_PROXY maliciously. See https://golang.org/s/cgihttpproxy.
            public bool CGI;
        }

        // config holds the parsed configuration for HTTP proxy settings.
        private partial struct config
        {
            public ref Config Config => ref Config_val; // httpsProxy is the parsed URL of the HTTPSProxy if defined.
            public ptr<url.URL> httpsProxy; // httpProxy is the parsed URL of the HTTPProxy if defined.
            public ptr<url.URL> httpProxy; // ipMatchers represent all values in the NoProxy that are IP address
// prefixes or an IP address in CIDR notation.
            public slice<matcher> ipMatchers; // domainMatchers represent all values in the NoProxy that are a domain
// name or hostname & domain name
            public slice<matcher> domainMatchers;
        }

        // FromEnvironment returns a Config instance populated from the
        // environment variables HTTP_PROXY, HTTPS_PROXY and NO_PROXY (or the
        // lowercase versions thereof). HTTPS_PROXY takes precedence over
        // HTTP_PROXY for https requests.
        //
        // The environment values may be either a complete URL or a
        // "host[:port]", in which case the "http" scheme is assumed. An error
        // is returned if the value is a different form.
        public static ptr<Config> FromEnvironment()
        {
            return addr(new Config(HTTPProxy:getEnvAny("HTTP_PROXY","http_proxy"),HTTPSProxy:getEnvAny("HTTPS_PROXY","https_proxy"),NoProxy:getEnvAny("NO_PROXY","no_proxy"),CGI:os.Getenv("REQUEST_METHOD")!="",));
        }

        private static @string getEnvAny(params @string[] names)
        {
            names = names.Clone();

            foreach (var (_, n) in names)
            {
                {
                    var val = os.Getenv(n);

                    if (val != "")
                    {
                        return val;
                    }

                }

            }
            return "";

        }

        // ProxyFunc returns a function that determines the proxy URL to use for
        // a given request URL. Changing the contents of cfg will not affect
        // proxy functions created earlier.
        //
        // A nil URL and nil error are returned if no proxy is defined in the
        // environment, or a proxy should not be used for the given request, as
        // defined by NO_PROXY.
        //
        // As a special case, if req.URL.Host is "localhost" (with or without a
        // port number), then a nil URL and nil error will be returned.
        private static Func<ptr<url.URL>, (ptr<url.URL>, error)> ProxyFunc(this ptr<Config> _addr_cfg)
        {
            ref Config cfg = ref _addr_cfg.val;
 
            // Preprocess the Config settings for more efficient evaluation.
            ptr<config> cfg1 = addr(new config(Config:*cfg,));
            cfg1.init();
            return cfg1.proxyForURL;

        }

        private static (ptr<url.URL>, error) proxyForURL(this ptr<config> _addr_cfg, ptr<url.URL> _addr_reqURL)
        {
            ptr<url.URL> _p0 = default!;
            error _p0 = default!;
            ref config cfg = ref _addr_cfg.val;
            ref url.URL reqURL = ref _addr_reqURL.val;

            ptr<url.URL> proxy;
            if (reqURL.Scheme == "https")
            {
                proxy = cfg.httpsProxy;
            }

            if (proxy == null)
            {
                proxy = cfg.httpProxy;
                if (proxy != null && cfg.CGI)
                {
                    return (_addr_null!, error.As(errors.New("refusing to use HTTP_PROXY value in CGI environment; see golang.org/s/cgihttpproxy"))!);
                }

            }

            if (proxy == null)
            {
                return (_addr_null!, error.As(null!)!);
            }

            if (!cfg.useProxy(canonicalAddr(_addr_reqURL)))
            {
                return (_addr_null!, error.As(null!)!);
            }

            return (_addr_proxy!, error.As(null!)!);

        }

        private static (ptr<url.URL>, error) parseProxy(@string proxy)
        {
            ptr<url.URL> _p0 = default!;
            error _p0 = default!;

            if (proxy == "")
            {
                return (_addr_null!, error.As(null!)!);
            }

            var (proxyURL, err) = url.Parse(proxy);
            if (err != null || (proxyURL.Scheme != "http" && proxyURL.Scheme != "https" && proxyURL.Scheme != "socks5"))
            { 
                // proxy was bogus. Try prepending "http://" to it and
                // see if that parses correctly. If not, we fall
                // through and complain about the original one.
                {
                    var proxyURL__prev2 = proxyURL;

                    (proxyURL, err) = url.Parse("http://" + proxy);

                    if (err == null)
                    {
                        return (_addr_proxyURL!, error.As(null!)!);
                    }

                    proxyURL = proxyURL__prev2;

                }

            }

            if (err != null)
            {
                return (_addr_null!, error.As(fmt.Errorf("invalid proxy address %q: %v", proxy, err))!);
            }

            return (_addr_proxyURL!, error.As(null!)!);

        }

        // useProxy reports whether requests to addr should use a proxy,
        // according to the NO_PROXY or no_proxy environment variable.
        // addr is always a canonicalAddr with a host and port.
        private static bool useProxy(this ptr<config> _addr_cfg, @string addr)
        {
            ref config cfg = ref _addr_cfg.val;

            if (len(addr) == 0L)
            {
                return true;
            }

            var (host, port, err) = net.SplitHostPort(addr);
            if (err != null)
            {
                return false;
            }

            if (host == "localhost")
            {
                return false;
            }

            var ip = net.ParseIP(host);
            if (ip != null)
            {
                if (ip.IsLoopback())
                {
                    return false;
                }

            }

            addr = strings.ToLower(strings.TrimSpace(host));

            if (ip != null)
            {
                {
                    var m__prev1 = m;

                    foreach (var (_, __m) in cfg.ipMatchers)
                    {
                        m = __m;
                        if (m.match(addr, port, ip))
                        {
                            return false;
                        }

                    }

                    m = m__prev1;
                }
            }

            {
                var m__prev1 = m;

                foreach (var (_, __m) in cfg.domainMatchers)
                {
                    m = __m;
                    if (m.match(addr, port, ip))
                    {
                        return false;
                    }

                }

                m = m__prev1;
            }

            return true;

        }

        private static void init(this ptr<config> _addr_c)
        {
            ref config c = ref _addr_c.val;

            {
                var parsed__prev1 = parsed;

                var (parsed, err) = parseProxy(c.HTTPProxy);

                if (err == null)
                {
                    c.httpProxy = parsed;
                }

                parsed = parsed__prev1;

            }

            {
                var parsed__prev1 = parsed;

                (parsed, err) = parseProxy(c.HTTPSProxy);

                if (err == null)
                {
                    c.httpsProxy = parsed;
                }

                parsed = parsed__prev1;

            }


            foreach (var (_, p) in strings.Split(c.NoProxy, ","))
            {
                p = strings.ToLower(strings.TrimSpace(p));
                if (len(p) == 0L)
                {
                    continue;
                }

                if (p == "*")
                {
                    c.ipMatchers = new slice<matcher>(new matcher[] { matcher.As(allMatch{})! });
                    c.domainMatchers = new slice<matcher>(new matcher[] { matcher.As(allMatch{})! });
                    return ;
                } 

                // IPv4/CIDR, IPv6/CIDR
                {
                    var (_, pnet, err) = net.ParseCIDR(p);

                    if (err == null)
                    {
                        c.ipMatchers = append(c.ipMatchers, new cidrMatch(cidr:pnet));
                        continue;
                    } 

                    // IPv4:port, [IPv6]:port

                } 

                // IPv4:port, [IPv6]:port
                var (phost, pport, err) = net.SplitHostPort(p);
                if (err == null)
                {
                    if (len(phost) == 0L)
                    { 
                        // There is no host part, likely the entry is malformed; ignore.
                        continue;

                    }

                    if (phost[0L] == '[' && phost[len(phost) - 1L] == ']')
                    {
                        phost = phost[1L..len(phost) - 1L];
                    }

                }
                else
                {
                    phost = p;
                } 
                // IPv4, IPv6
                {
                    var pip = net.ParseIP(phost);

                    if (pip != null)
                    {
                        c.ipMatchers = append(c.ipMatchers, new ipMatch(ip:pip,port:pport));
                        continue;
                    }

                }


                if (len(phost) == 0L)
                { 
                    // There is no host part, likely the entry is malformed; ignore.
                    continue;

                } 

                // domain.com or domain.com:80
                // foo.com matches bar.foo.com
                // .domain.com or .domain.com:port
                // *.domain.com or *.domain.com:port
                if (strings.HasPrefix(phost, "*."))
                {
                    phost = phost[1L..];
                }

                var matchHost = false;
                if (phost[0L] != '.')
                {
                    matchHost = true;
                    phost = "." + phost;
                }

                c.domainMatchers = append(c.domainMatchers, new domainMatch(host:phost,port:pport,matchHost:matchHost));

            }

        }

        private static map portMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"http":"80","https":"443","socks5":"1080",};

        // canonicalAddr returns url.Host but always with a ":port" suffix
        private static @string canonicalAddr(ptr<url.URL> _addr_url)
        {
            ref url.URL url = ref _addr_url.val;

            var addr = url.Hostname();
            {
                var (v, err) = idnaASCII(addr);

                if (err == null)
                {
                    addr = v;
                }

            }

            var port = url.Port();
            if (port == "")
            {
                port = portMap[url.Scheme];
            }

            return net.JoinHostPort(addr, port);

        }

        // Given a string of the form "host", "host:port", or "[ipv6::address]:port",
        // return true if the string includes a port.
        private static bool hasPort(@string s)
        {
            return strings.LastIndex(s, ":") > strings.LastIndex(s, "]");
        }

        private static (@string, error) idnaASCII(@string v)
        {
            @string _p0 = default;
            error _p0 = default!;
 
            // TODO: Consider removing this check after verifying performance is okay.
            // Right now punycode verification, length checks, context checks, and the
            // permissible character tests are all omitted. It also prevents the ToASCII
            // call from salvaging an invalid IDN, when possible. As a result it may be
            // possible to have two IDNs that appear identical to the user where the
            // ASCII-only version causes an error downstream whereas the non-ASCII
            // version does not.
            // Note that for correct ASCII IDNs ToASCII will only do considerably more
            // work, but it will not cause an allocation.
            if (isASCII(v))
            {
                return (v, error.As(null!)!);
            }

            return idna.Lookup.ToASCII(v);

        }

        private static bool isASCII(@string s)
        {
            for (long i = 0L; i < len(s); i++)
            {
                if (s[i] >= utf8.RuneSelf)
                {
                    return false;
                }

            }

            return true;

        }

        // matcher represents the matching rule for a given value in the NO_PROXY list
        private partial interface matcher
        {
            bool match(@string host, @string port, net.IP ip);
        }

        // allMatch matches on all possible inputs
        private partial struct allMatch
        {
        }

        private static bool match(this allMatch a, @string host, @string port, net.IP ip)
        {
            return true;
        }

        private partial struct cidrMatch
        {
            public ptr<net.IPNet> cidr;
        }

        private static bool match(this cidrMatch m, @string host, @string port, net.IP ip)
        {
            return m.cidr.Contains(ip);
        }

        private partial struct ipMatch
        {
            public net.IP ip;
            public @string port;
        }

        private static bool match(this ipMatch m, @string host, @string port, net.IP ip)
        {
            if (m.ip.Equal(ip))
            {
                return m.port == "" || m.port == port;
            }

            return false;

        }

        private partial struct domainMatch
        {
            public @string host;
            public @string port;
            public bool matchHost;
        }

        private static bool match(this domainMatch m, @string host, @string port, net.IP ip)
        {
            if (strings.HasSuffix(host, m.host) || (m.matchHost && host == m.host[1L..]))
            {
                return m.port == "" || m.port == port;
            }

            return false;

        }
    }
}}}}}
