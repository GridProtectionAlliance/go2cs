// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cookiejar implements an in-memory RFC 6265-compliant http.CookieJar.
// package cookiejar -- go2cs converted at 2020 October 09 04:58:45 UTC
// import "net/http/cookiejar" ==> using cookiejar = go.net.http.cookiejar_package
// Original source: C:\Go\src\net\http\cookiejar\jar.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using net = go.net_package;
using http = go.net.http_package;
using url = go.net.url_package;
using sort = go.sort_package;
using strings = go.strings_package;
using sync = go.sync_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go {
namespace net {
namespace http
{
    public static partial class cookiejar_package
    {
        // PublicSuffixList provides the public suffix of a domain. For example:
        //      - the public suffix of "example.com" is "com",
        //      - the public suffix of "foo1.foo2.foo3.co.uk" is "co.uk", and
        //      - the public suffix of "bar.pvt.k12.ma.us" is "pvt.k12.ma.us".
        //
        // Implementations of PublicSuffixList must be safe for concurrent use by
        // multiple goroutines.
        //
        // An implementation that always returns "" is valid and may be useful for
        // testing but it is not secure: it means that the HTTP server for foo.com can
        // set a cookie for bar.com.
        //
        // A public suffix list implementation is in the package
        // golang.org/x/net/publicsuffix.
        public partial interface PublicSuffixList
        {
            @string PublicSuffix(@string domain); // String returns a description of the source of this public suffix
// list. The description will typically contain something like a time
// stamp or version number.
            @string String();
        }

        // Options are the options for creating a new Jar.
        public partial struct Options
        {
            public PublicSuffixList PublicSuffixList;
        }

        // Jar implements the http.CookieJar interface from the net/http package.
        public partial struct Jar
        {
            public PublicSuffixList psList; // mu locks the remaining fields.
            public sync.Mutex mu; // entries is a set of entries, keyed by their eTLD+1 and subkeyed by
// their name/domain/path.
            public map<@string, map<@string, entry>> entries; // nextSeqNum is the next sequence number assigned to a new cookie
// created SetCookies.
            public ulong nextSeqNum;
        }

        // New returns a new cookie jar. A nil *Options is equivalent to a zero
        // Options.
        public static (ptr<Jar>, error) New(ptr<Options> _addr_o)
        {
            ptr<Jar> _p0 = default!;
            error _p0 = default!;
            ref Options o = ref _addr_o.val;

            ptr<Jar> jar = addr(new Jar(entries:make(map[string]map[string]entry),));
            if (o != null)
            {
                jar.psList = o.PublicSuffixList;
            }

            return (_addr_jar!, error.As(null!)!);

        }

        // entry is the internal representation of a cookie.
        //
        // This struct type is not used outside of this package per se, but the exported
        // fields are those of RFC 6265.
        private partial struct entry
        {
            public @string Name;
            public @string Value;
            public @string Domain;
            public @string Path;
            public @string SameSite;
            public bool Secure;
            public bool HttpOnly;
            public bool Persistent;
            public bool HostOnly;
            public time.Time Expires;
            public time.Time Creation;
            public time.Time LastAccess; // seqNum is a sequence number so that Cookies returns cookies in a
// deterministic order, even for cookies that have equal Path length and
// equal Creation time. This simplifies testing.
            public ulong seqNum;
        }

        // id returns the domain;path;name triple of e as an id.
        private static @string id(this ptr<entry> _addr_e)
        {
            ref entry e = ref _addr_e.val;

            return fmt.Sprintf("%s;%s;%s", e.Domain, e.Path, e.Name);
        }

        // shouldSend determines whether e's cookie qualifies to be included in a
        // request to host/path. It is the caller's responsibility to check if the
        // cookie is expired.
        private static bool shouldSend(this ptr<entry> _addr_e, bool https, @string host, @string path)
        {
            ref entry e = ref _addr_e.val;

            return e.domainMatch(host) && e.pathMatch(path) && (https || !e.Secure);
        }

        // domainMatch implements "domain-match" of RFC 6265 section 5.1.3.
        private static bool domainMatch(this ptr<entry> _addr_e, @string host)
        {
            ref entry e = ref _addr_e.val;

            if (e.Domain == host)
            {
                return true;
            }

            return !e.HostOnly && hasDotSuffix(host, e.Domain);

        }

        // pathMatch implements "path-match" according to RFC 6265 section 5.1.4.
        private static bool pathMatch(this ptr<entry> _addr_e, @string requestPath)
        {
            ref entry e = ref _addr_e.val;

            if (requestPath == e.Path)
            {
                return true;
            }

            if (strings.HasPrefix(requestPath, e.Path))
            {
                if (e.Path[len(e.Path) - 1L] == '/')
                {
                    return true; // The "/any/" matches "/any/path" case.
                }
                else if (requestPath[len(e.Path)] == '/')
                {
                    return true; // The "/any" matches "/any/path" case.
                }

            }

            return false;

        }

        // hasDotSuffix reports whether s ends in "."+suffix.
        private static bool hasDotSuffix(@string s, @string suffix)
        {
            return len(s) > len(suffix) && s[len(s) - len(suffix) - 1L] == '.' && s[len(s) - len(suffix)..] == suffix;
        }

        // Cookies implements the Cookies method of the http.CookieJar interface.
        //
        // It returns an empty slice if the URL's scheme is not HTTP or HTTPS.
        private static slice<ptr<http.Cookie>> Cookies(this ptr<Jar> _addr_j, ptr<url.URL> _addr_u)
        {
            slice<ptr<http.Cookie>> cookies = default;
            ref Jar j = ref _addr_j.val;
            ref url.URL u = ref _addr_u.val;

            return j.cookies(u, time.Now());
        }

        // cookies is like Cookies but takes the current time as a parameter.
        private static slice<ptr<http.Cookie>> cookies(this ptr<Jar> _addr_j, ptr<url.URL> _addr_u, time.Time now) => func((defer, _, __) =>
        {
            slice<ptr<http.Cookie>> cookies = default;
            ref Jar j = ref _addr_j.val;
            ref url.URL u = ref _addr_u.val;

            if (u.Scheme != "http" && u.Scheme != "https")
            {
                return cookies;
            }

            var (host, err) = canonicalHost(u.Host);
            if (err != null)
            {
                return cookies;
            }

            var key = jarKey(host, j.psList);

            j.mu.Lock();
            defer(j.mu.Unlock());

            var submap = j.entries[key];
            if (submap == null)
            {
                return cookies;
            }

            var https = u.Scheme == "https";
            var path = u.Path;
            if (path == "")
            {
                path = "/";
            }

            var modified = false;
            slice<entry> selected = default;
            {
                var e__prev1 = e;

                foreach (var (__id, __e) in submap)
                {
                    id = __id;
                    e = __e;
                    if (e.Persistent && !e.Expires.After(now))
                    {
                        delete(submap, id);
                        modified = true;
                        continue;
                    }

                    if (!e.shouldSend(https, host, path))
                    {
                        continue;
                    }

                    e.LastAccess = now;
                    submap[id] = e;
                    selected = append(selected, e);
                    modified = true;

                }

                e = e__prev1;
            }

            if (modified)
            {
                if (len(submap) == 0L)
                {
                    delete(j.entries, key);
                }
                else
                {
                    j.entries[key] = submap;
                }

            } 

            // sort according to RFC 6265 section 5.4 point 2: by longest
            // path and then by earliest creation time.
            sort.Slice(selected, (i, j) =>
            {
                var s = selected;
                if (len(s[i].Path) != len(s[j].Path))
                {
                    return len(s[i].Path) > len(s[j].Path);
                }

                if (!s[i].Creation.Equal(s[j].Creation))
                {
                    return s[i].Creation.Before(s[j].Creation);
                }

                return s[i].seqNum < s[j].seqNum;

            });
            {
                var e__prev1 = e;

                foreach (var (_, __e) in selected)
                {
                    e = __e;
                    cookies = append(cookies, addr(new http.Cookie(Name:e.Name,Value:e.Value)));
                }

                e = e__prev1;
            }

            return cookies;

        });

        // SetCookies implements the SetCookies method of the http.CookieJar interface.
        //
        // It does nothing if the URL's scheme is not HTTP or HTTPS.
        private static void SetCookies(this ptr<Jar> _addr_j, ptr<url.URL> _addr_u, slice<ptr<http.Cookie>> cookies)
        {
            ref Jar j = ref _addr_j.val;
            ref url.URL u = ref _addr_u.val;

            j.setCookies(u, cookies, time.Now());
        }

        // setCookies is like SetCookies but takes the current time as parameter.
        private static void setCookies(this ptr<Jar> _addr_j, ptr<url.URL> _addr_u, slice<ptr<http.Cookie>> cookies, time.Time now) => func((defer, _, __) =>
        {
            ref Jar j = ref _addr_j.val;
            ref url.URL u = ref _addr_u.val;

            if (len(cookies) == 0L)
            {
                return ;
            }

            if (u.Scheme != "http" && u.Scheme != "https")
            {
                return ;
            }

            var (host, err) = canonicalHost(u.Host);
            if (err != null)
            {
                return ;
            }

            var key = jarKey(host, j.psList);
            var defPath = defaultPath(u.Path);

            j.mu.Lock();
            defer(j.mu.Unlock());

            var submap = j.entries[key];

            var modified = false;
            foreach (var (_, cookie) in cookies)
            {
                var (e, remove, err) = j.newEntry(cookie, now, defPath, host);
                if (err != null)
                {
                    continue;
                }

                var id = e.id();
                if (remove)
                {
                    if (submap != null)
                    {
                        {
                            var (_, ok) = submap[id];

                            if (ok)
                            {
                                delete(submap, id);
                                modified = true;
                            }

                        }

                    }

                    continue;

                }

                if (submap == null)
                {
                    submap = make_map<@string, entry>();
                }

                {
                    var (old, ok) = submap[id];

                    if (ok)
                    {
                        e.Creation = old.Creation;
                        e.seqNum = old.seqNum;
                    }
                    else
                    {
                        e.Creation = now;
                        e.seqNum = j.nextSeqNum;
                        j.nextSeqNum++;
                    }

                }

                e.LastAccess = now;
                submap[id] = e;
                modified = true;

            }
            if (modified)
            {
                if (len(submap) == 0L)
                {
                    delete(j.entries, key);
                }
                else
                {
                    j.entries[key] = submap;
                }

            }

        });

        // canonicalHost strips port from host if present and returns the canonicalized
        // host name.
        private static (@string, error) canonicalHost(@string host)
        {
            @string _p0 = default;
            error _p0 = default!;

            error err = default!;
            host = strings.ToLower(host);
            if (hasPort(host))
            {
                host, _, err = net.SplitHostPort(host);
                if (err != null)
                {
                    return ("", error.As(err)!);
                }

            }

            if (strings.HasSuffix(host, "."))
            { 
                // Strip trailing dot from fully qualified domain names.
                host = host[..len(host) - 1L];

            }

            return toASCII(host);

        }

        // hasPort reports whether host contains a port number. host may be a host
        // name, an IPv4 or an IPv6 address.
        private static bool hasPort(@string host)
        {
            var colons = strings.Count(host, ":");
            if (colons == 0L)
            {
                return false;
            }

            if (colons == 1L)
            {
                return true;
            }

            return host[0L] == '[' && strings.Contains(host, "]:");

        }

        // jarKey returns the key to use for a jar.
        private static @string jarKey(@string host, PublicSuffixList psl)
        {
            if (isIP(host))
            {
                return host;
            }

            long i = default;
            if (psl == null)
            {
                i = strings.LastIndex(host, ".");
                if (i <= 0L)
                {
                    return host;
                }

            }
            else
            {
                var suffix = psl.PublicSuffix(host);
                if (suffix == host)
                {
                    return host;
                }

                i = len(host) - len(suffix);
                if (i <= 0L || host[i - 1L] != '.')
                { 
                    // The provided public suffix list psl is broken.
                    // Storing cookies under host is a safe stopgap.
                    return host;

                } 
                // Only len(suffix) is used to determine the jar key from
                // here on, so it is okay if psl.PublicSuffix("www.buggy.psl")
                // returns "com" as the jar key is generated from host.
            }

            var prevDot = strings.LastIndex(host[..i - 1L], ".");
            return host[prevDot + 1L..];

        }

        // isIP reports whether host is an IP address.
        private static bool isIP(@string host)
        {
            return net.ParseIP(host) != null;
        }

        // defaultPath returns the directory part of an URL's path according to
        // RFC 6265 section 5.1.4.
        private static @string defaultPath(@string path)
        {
            if (len(path) == 0L || path[0L] != '/')
            {
                return "/"; // Path is empty or malformed.
            }

            var i = strings.LastIndex(path, "/"); // Path starts with "/", so i != -1.
            if (i == 0L)
            {
                return "/"; // Path has the form "/abc".
            }

            return path[..i]; // Path is either of form "/abc/xyz" or "/abc/xyz/".
        }

        // newEntry creates an entry from a http.Cookie c. now is the current time and
        // is compared to c.Expires to determine deletion of c. defPath and host are the
        // default-path and the canonical host name of the URL c was received from.
        //
        // remove records whether the jar should delete this cookie, as it has already
        // expired with respect to now. In this case, e may be incomplete, but it will
        // be valid to call e.id (which depends on e's Name, Domain and Path).
        //
        // A malformed c.Domain will result in an error.
        private static (entry, bool, error) newEntry(this ptr<Jar> _addr_j, ptr<http.Cookie> _addr_c, time.Time now, @string defPath, @string host)
        {
            entry e = default;
            bool remove = default;
            error err = default!;
            ref Jar j = ref _addr_j.val;
            ref http.Cookie c = ref _addr_c.val;

            e.Name = c.Name;

            if (c.Path == "" || c.Path[0L] != '/')
            {
                e.Path = defPath;
            }
            else
            {
                e.Path = c.Path;
            }

            e.Domain, e.HostOnly, err = j.domainAndType(host, c.Domain);
            if (err != null)
            {
                return (e, false, error.As(err)!);
            } 

            // MaxAge takes precedence over Expires.
            if (c.MaxAge < 0L)
            {
                return (e, true, error.As(null!)!);
            }
            else if (c.MaxAge > 0L)
            {
                e.Expires = now.Add(time.Duration(c.MaxAge) * time.Second);
                e.Persistent = true;
            }
            else
            {
                if (c.Expires.IsZero())
                {
                    e.Expires = endOfTime;
                    e.Persistent = false;
                }
                else
                {
                    if (!c.Expires.After(now))
                    {
                        return (e, true, error.As(null!)!);
                    }

                    e.Expires = c.Expires;
                    e.Persistent = true;

                }

            }

            e.Value = c.Value;
            e.Secure = c.Secure;
            e.HttpOnly = c.HttpOnly;


            if (c.SameSite == http.SameSiteDefaultMode) 
                e.SameSite = "SameSite";
            else if (c.SameSite == http.SameSiteStrictMode) 
                e.SameSite = "SameSite=Strict";
            else if (c.SameSite == http.SameSiteLaxMode) 
                e.SameSite = "SameSite=Lax";
                        return (e, false, error.As(null!)!);

        }

        private static var errIllegalDomain = errors.New("cookiejar: illegal cookie domain attribute");        private static var errMalformedDomain = errors.New("cookiejar: malformed cookie domain attribute");        private static var errNoHostname = errors.New("cookiejar: no host name available (IP only)");

        // endOfTime is the time when session (non-persistent) cookies expire.
        // This instant is representable in most date/time formats (not just
        // Go's time.Time) and should be far enough in the future.
        private static var endOfTime = time.Date(9999L, 12L, 31L, 23L, 59L, 59L, 0L, time.UTC);

        // domainAndType determines the cookie's domain and hostOnly attribute.
        private static (@string, bool, error) domainAndType(this ptr<Jar> _addr_j, @string host, @string domain)
        {
            @string _p0 = default;
            bool _p0 = default;
            error _p0 = default!;
            ref Jar j = ref _addr_j.val;

            if (domain == "")
            { 
                // No domain attribute in the SetCookie header indicates a
                // host cookie.
                return (host, true, error.As(null!)!);

            }

            if (isIP(host))
            { 
                // According to RFC 6265 domain-matching includes not being
                // an IP address.
                // TODO: This might be relaxed as in common browsers.
                return ("", false, error.As(errNoHostname)!);

            } 

            // From here on: If the cookie is valid, it is a domain cookie (with
            // the one exception of a public suffix below).
            // See RFC 6265 section 5.2.3.
            if (domain[0L] == '.')
            {
                domain = domain[1L..];
            }

            if (len(domain) == 0L || domain[0L] == '.')
            { 
                // Received either "Domain=." or "Domain=..some.thing",
                // both are illegal.
                return ("", false, error.As(errMalformedDomain)!);

            }

            domain = strings.ToLower(domain);

            if (domain[len(domain) - 1L] == '.')
            { 
                // We received stuff like "Domain=www.example.com.".
                // Browsers do handle such stuff (actually differently) but
                // RFC 6265 seems to be clear here (e.g. section 4.1.2.3) in
                // requiring a reject.  4.1.2.3 is not normative, but
                // "Domain Matching" (5.1.3) and "Canonicalized Host Names"
                // (5.1.2) are.
                return ("", false, error.As(errMalformedDomain)!);

            } 

            // See RFC 6265 section 5.3 #5.
            if (j.psList != null)
            {
                {
                    var ps = j.psList.PublicSuffix(domain);

                    if (ps != "" && !hasDotSuffix(domain, ps))
                    {
                        if (host == domain)
                        { 
                            // This is the one exception in which a cookie
                            // with a domain attribute is a host cookie.
                            return (host, true, error.As(null!)!);

                        }

                        return ("", false, error.As(errIllegalDomain)!);

                    }

                }

            } 

            // The domain must domain-match host: www.mycompany.com cannot
            // set cookies for .ourcompetitors.com.
            if (host != domain && !hasDotSuffix(host, domain))
            {
                return ("", false, error.As(errIllegalDomain)!);
            }

            return (domain, false, error.As(null!)!);

        }
    }
}}}
