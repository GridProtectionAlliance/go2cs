// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package url parses URLs and implements query escaping.
// package url -- go2cs converted at 2020 August 29 08:28:27 UTC
// import "net/url" ==> using url = go.net.url_package
// Original source: C:\Go\src\net\url\url.go
// See RFC 3986. This package generally follows RFC 3986, except where
// it deviates for compatibility reasons. When sending changes, first
// search old issues for history on decisions. Unit tests should also
// contain references to issue numbers with details.

using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace net
{
    public static partial class url_package
    {
        // Error reports an error and the operation and URL that caused it.
        public partial struct Error
        {
            public @string Op;
            public @string URL;
            public error Err;
        }

        private static @string Error(this ref Error e)
        {
            return e.Op + " " + e.URL + ": " + e.Err.Error();
        }

        private partial interface timeout
        {
            bool Timeout();
        }

        private static bool Timeout(this ref Error e)
        {
            timeout (t, ok) = e.Err._<timeout>();
            return ok && t.Timeout();
        }

        private partial interface temporary
        {
            bool Temporary();
        }

        private static bool Temporary(this ref Error e)
        {
            temporary (t, ok) = e.Err._<temporary>();
            return ok && t.Temporary();
        }

        private static bool ishex(byte c)
        {

            if ('0' <= c && c <= '9') 
                return true;
            else if ('a' <= c && c <= 'f') 
                return true;
            else if ('A' <= c && c <= 'F') 
                return true;
                        return false;
        }

        private static byte unhex(byte c)
        {

            if ('0' <= c && c <= '9') 
                return c - '0';
            else if ('a' <= c && c <= 'f') 
                return c - 'a' + 10L;
            else if ('A' <= c && c <= 'F') 
                return c - 'A' + 10L;
                        return 0L;
        }

        private partial struct encoding // : long
        {
        }

        private static readonly encoding encodePath = 1L + iota;
        private static readonly var encodePathSegment = 0;
        private static readonly var encodeHost = 1;
        private static readonly var encodeZone = 2;
        private static readonly var encodeUserPassword = 3;
        private static readonly var encodeQueryComponent = 4;
        private static readonly var encodeFragment = 5;

        public partial struct EscapeError // : @string
        {
        }

        public static @string Error(this EscapeError e)
        {
            return "invalid URL escape " + strconv.Quote(string(e));
        }

        public partial struct InvalidHostError // : @string
        {
        }

        public static @string Error(this InvalidHostError e)
        {
            return "invalid character " + strconv.Quote(string(e)) + " in host name";
        }

        // Return true if the specified character should be escaped when
        // appearing in a URL string, according to RFC 3986.
        //
        // Please be informed that for now shouldEscape does not check all
        // reserved characters correctly. See golang.org/issue/5684.
        private static bool shouldEscape(byte c, encoding mode)
        { 
            // §2.3 Unreserved characters (alphanum)
            if ('A' <= c && c <= 'Z' || 'a' <= c && c <= 'z' || '0' <= c && c <= '9')
            {
                return false;
            }
            if (mode == encodeHost || mode == encodeZone)
            { 
                // §3.2.2 Host allows
                //    sub-delims = "!" / "$" / "&" / "'" / "(" / ")" / "*" / "+" / "," / ";" / "="
                // as part of reg-name.
                // We add : because we include :port as part of host.
                // We add [ ] because we include [ipv6]:port as part of host.
                // We add < > because they're the only characters left that
                // we could possibly allow, and Parse will reject them if we
                // escape them (because hosts can't use %-encoding for
                // ASCII bytes).
                switch (c)
                {
                    case '!': 

                    case '$': 

                    case '&': 

                    case '\'': 

                    case '(': 

                    case ')': 

                    case '*': 

                    case '+': 

                    case ',': 

                    case ';': 

                    case '=': 

                    case ':': 

                    case '[': 

                    case ']': 

                    case '<': 

                    case '>': 

                    case '"': 
                        return false;
                        break;
                }
            }
            switch (c)
            {
                case '-': // §2.3 Unreserved characters (mark)

                case '_': // §2.3 Unreserved characters (mark)

                case '.': // §2.3 Unreserved characters (mark)

                case '~': // §2.3 Unreserved characters (mark)
                    return false;
                    break;
                case '$': // §2.2 Reserved characters (reserved)
                    // Different sections of the URL allow a few of
                    // the reserved characters to appear unescaped.

                case '&': // §2.2 Reserved characters (reserved)
                    // Different sections of the URL allow a few of
                    // the reserved characters to appear unescaped.

                case '+': // §2.2 Reserved characters (reserved)
                    // Different sections of the URL allow a few of
                    // the reserved characters to appear unescaped.

                case ',': // §2.2 Reserved characters (reserved)
                    // Different sections of the URL allow a few of
                    // the reserved characters to appear unescaped.

                case '/': // §2.2 Reserved characters (reserved)
                    // Different sections of the URL allow a few of
                    // the reserved characters to appear unescaped.

                case ':': // §2.2 Reserved characters (reserved)
                    // Different sections of the URL allow a few of
                    // the reserved characters to appear unescaped.

                case ';': // §2.2 Reserved characters (reserved)
                    // Different sections of the URL allow a few of
                    // the reserved characters to appear unescaped.

                case '=': // §2.2 Reserved characters (reserved)
                    // Different sections of the URL allow a few of
                    // the reserved characters to appear unescaped.

                case '?': // §2.2 Reserved characters (reserved)
                    // Different sections of the URL allow a few of
                    // the reserved characters to appear unescaped.

                case '@': // §2.2 Reserved characters (reserved)
                    // Different sections of the URL allow a few of
                    // the reserved characters to appear unescaped.

                    if (mode == encodePath) // §3.3
                        // The RFC allows : @ & = + $ but saves / ; , for assigning
                        // meaning to individual path segments. This package
                        // only manipulates the path as a whole, so we allow those
                        // last three as well. That leaves only ? to escape.
                        return c == '?';
                    else if (mode == encodePathSegment) // §3.3
                        // The RFC allows : @ & = + $ but saves / ; , for assigning
                        // meaning to individual path segments.
                        return c == '/' || c == ';' || c == ',' || c == '?';
                    else if (mode == encodeUserPassword) // §3.2.1
                        // The RFC allows ';', ':', '&', '=', '+', '$', and ',' in
                        // userinfo, so we must escape only '@', '/', and '?'.
                        // The parsing of userinfo treats ':' as special so we must escape
                        // that too.
                        return c == '@' || c == '/' || c == '?' || c == ':';
                    else if (mode == encodeQueryComponent) // §3.4
                        // The RFC reserves (so we must escape) everything.
                        return true;
                    else if (mode == encodeFragment) // §4.1
                        // The RFC text is silent but the grammar allows
                        // everything, so escape nothing.
                        return false;
                    break;
            } 

            // Everything else must be escaped.
            return true;
        }

        // QueryUnescape does the inverse transformation of QueryEscape,
        // converting each 3-byte encoded substring of the form "%AB" into the
        // hex-decoded byte 0xAB. It also converts '+' into ' ' (space).
        // It returns an error if any % is not followed by two hexadecimal
        // digits.
        public static (@string, error) QueryUnescape(@string s)
        {
            return unescape(s, encodeQueryComponent);
        }

        // PathUnescape does the inverse transformation of PathEscape,
        // converting each 3-byte encoded substring of the form "%AB" into the
        // hex-decoded byte 0xAB. It also converts '+' into ' ' (space).
        // It returns an error if any % is not followed by two hexadecimal
        // digits.
        //
        // PathUnescape is identical to QueryUnescape except that it does not
        // unescape '+' to ' ' (space).
        public static (@string, error) PathUnescape(@string s)
        {
            return unescape(s, encodePathSegment);
        }

        // unescape unescapes a string; the mode specifies
        // which section of the URL string is being unescaped.
        private static (@string, error) unescape(@string s, encoding mode)
        { 
            // Count %, check that they're well-formed.
            long n = 0L;
            var hasPlus = false;
            {
                long i__prev1 = i;

                long i = 0L;

                while (i < len(s))
                {
                    switch (s[i])
                    {
                        case '%': 
                            n++;
                            if (i + 2L >= len(s) || !ishex(s[i + 1L]) || !ishex(s[i + 2L]))
                            {
                                s = s[i..];
                                if (len(s) > 3L)
                                {
                                    s = s[..3L];
                                }
                                return ("", EscapeError(s));
                            } 
                            // Per https://tools.ietf.org/html/rfc3986#page-21
                            // in the host component %-encoding can only be used
                            // for non-ASCII bytes.
                            // But https://tools.ietf.org/html/rfc6874#section-2
                            // introduces %25 being allowed to escape a percent sign
                            // in IPv6 scoped-address literals. Yay.
                            if (mode == encodeHost && unhex(s[i + 1L]) < 8L && s[i..i + 3L] != "%25")
                            {
                                return ("", EscapeError(s[i..i + 3L]));
                            }
                            if (mode == encodeZone)
                            { 
                                // RFC 6874 says basically "anything goes" for zone identifiers
                                // and that even non-ASCII can be redundantly escaped,
                                // but it seems prudent to restrict %-escaped bytes here to those
                                // that are valid host name bytes in their unescaped form.
                                // That is, you can use escaping in the zone identifier but not
                                // to introduce bytes you couldn't just write directly.
                                // But Windows puts spaces here! Yay.
                                var v = unhex(s[i + 1L]) << (int)(4L) | unhex(s[i + 2L]);
                                if (s[i..i + 3L] != "%25" && v != ' ' && shouldEscape(v, encodeHost))
                                {
                                    return ("", EscapeError(s[i..i + 3L]));
                                }
                            }
                            i += 3L;
                            break;
                        case '+': 
                            hasPlus = mode == encodeQueryComponent;
                            i++;
                            break;
                        default: 
                            if ((mode == encodeHost || mode == encodeZone) && s[i] < 0x80UL && shouldEscape(s[i], mode))
                            {
                                return ("", InvalidHostError(s[i..i + 1L]));
                            }
                            i++;
                            break;
                    }
                }


                i = i__prev1;
            }

            if (n == 0L && !hasPlus)
            {
                return (s, null);
            }
            var t = make_slice<byte>(len(s) - 2L * n);
            long j = 0L;
            {
                long i__prev1 = i;

                i = 0L;

                while (i < len(s))
                {
                    switch (s[i])
                    {
                        case '%': 
                            t[j] = unhex(s[i + 1L]) << (int)(4L) | unhex(s[i + 2L]);
                            j++;
                            i += 3L;
                            break;
                        case '+': 
                            if (mode == encodeQueryComponent)
                            {
                                t[j] = ' ';
                            }
                            else
                            {
                                t[j] = '+';
                            }
                            j++;
                            i++;
                            break;
                        default: 
                            t[j] = s[i];
                            j++;
                            i++;
                            break;
                    }
                }


                i = i__prev1;
            }
            return (string(t), null);
        }

        // QueryEscape escapes the string so it can be safely placed
        // inside a URL query.
        public static @string QueryEscape(@string s)
        {
            return escape(s, encodeQueryComponent);
        }

        // PathEscape escapes the string so it can be safely placed
        // inside a URL path segment.
        public static @string PathEscape(@string s)
        {
            return escape(s, encodePathSegment);
        }

        private static @string escape(@string s, encoding mode)
        {
            long spaceCount = 0L;
            long hexCount = 0L;
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(s); i++)
                {
                    var c = s[i];
                    if (shouldEscape(c, mode))
                    {
                        if (c == ' ' && mode == encodeQueryComponent)
                        {
                            spaceCount++;
                        }
                        else
                        {
                            hexCount++;
                        }
                    }
                }


                i = i__prev1;
            }

            if (spaceCount == 0L && hexCount == 0L)
            {
                return s;
            }
            var t = make_slice<byte>(len(s) + 2L * hexCount);
            long j = 0L;
            {
                long i__prev1 = i;

                for (i = 0L; i < len(s); i++)
                {
                    {
                        var c__prev1 = c;

                        c = s[i];


                        if (c == ' ' && mode == encodeQueryComponent) 
                            t[j] = '+';
                            j++;
                        else if (shouldEscape(c, mode)) 
                            t[j] = '%';
                            t[j + 1L] = "0123456789ABCDEF"[c >> (int)(4L)];
                            t[j + 2L] = "0123456789ABCDEF"[c & 15L];
                            j += 3L;
                        else 
                            t[j] = s[i];
                            j++;


                        c = c__prev1;
                    }
                }


                i = i__prev1;
            }
            return string(t);
        }

        // A URL represents a parsed URL (technically, a URI reference).
        //
        // The general form represented is:
        //
        //    [scheme:][//[userinfo@]host][/]path[?query][#fragment]
        //
        // URLs that do not start with a slash after the scheme are interpreted as:
        //
        //    scheme:opaque[?query][#fragment]
        //
        // Note that the Path field is stored in decoded form: /%47%6f%2f becomes /Go/.
        // A consequence is that it is impossible to tell which slashes in the Path were
        // slashes in the raw URL and which were %2f. This distinction is rarely important,
        // but when it is, code must not use Path directly.
        // The Parse function sets both Path and RawPath in the URL it returns,
        // and URL's String method uses RawPath if it is a valid encoding of Path,
        // by calling the EscapedPath method.
        public partial struct URL
        {
            public @string Scheme;
            public @string Opaque; // encoded opaque data
            public ptr<Userinfo> User; // username and password information
            public @string Host; // host or host:port
            public @string Path; // path (relative paths may omit leading slash)
            public @string RawPath; // encoded path hint (see EscapedPath method)
            public bool ForceQuery; // append a query ('?') even if RawQuery is empty
            public @string RawQuery; // encoded query values, without '?'
            public @string Fragment; // fragment for references, without '#'
        }

        // User returns a Userinfo containing the provided username
        // and no password set.
        public static ref Userinfo User(@string username)
        {
            return ref new Userinfo(username,"",false);
        }

        // UserPassword returns a Userinfo containing the provided username
        // and password.
        //
        // This functionality should only be used with legacy web sites.
        // RFC 2396 warns that interpreting Userinfo this way
        // ``is NOT RECOMMENDED, because the passing of authentication
        // information in clear text (such as URI) has proven to be a
        // security risk in almost every case where it has been used.''
        public static ref Userinfo UserPassword(@string username, @string password)
        {
            return ref new Userinfo(username,password,true);
        }

        // The Userinfo type is an immutable encapsulation of username and
        // password details for a URL. An existing Userinfo value is guaranteed
        // to have a username set (potentially empty, as allowed by RFC 2396),
        // and optionally a password.
        public partial struct Userinfo
        {
            public @string username;
            public @string password;
            public bool passwordSet;
        }

        // Username returns the username.
        private static @string Username(this ref Userinfo u)
        {
            if (u == null)
            {
                return "";
            }
            return u.username;
        }

        // Password returns the password in case it is set, and whether it is set.
        private static (@string, bool) Password(this ref Userinfo u)
        {
            if (u == null)
            {
                return ("", false);
            }
            return (u.password, u.passwordSet);
        }

        // String returns the encoded userinfo information in the standard form
        // of "username[:password]".
        private static @string String(this ref Userinfo u)
        {
            if (u == null)
            {
                return "";
            }
            var s = escape(u.username, encodeUserPassword);
            if (u.passwordSet)
            {
                s += ":" + escape(u.password, encodeUserPassword);
            }
            return s;
        }

        // Maybe rawurl is of the form scheme:path.
        // (Scheme must be [a-zA-Z][a-zA-Z0-9+-.]*)
        // If so, return scheme, path; else return "", rawurl.
        private static (@string, @string, error) getscheme(@string rawurl)
        {
            for (long i = 0L; i < len(rawurl); i++)
            {
                var c = rawurl[i];

                if ('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z')                 else if ('0' <= c && c <= '9' || c == '+' || c == '-' || c == '.') 
                    if (i == 0L)
                    {
                        return ("", rawurl, null);
                    }
                else if (c == ':') 
                    if (i == 0L)
                    {
                        return ("", "", errors.New("missing protocol scheme"));
                    }
                    return (rawurl[..i], rawurl[i + 1L..], null);
                else 
                    // we have encountered an invalid character,
                    // so there is no valid scheme
                    return ("", rawurl, null);
                            }

            return ("", rawurl, null);
        }

        // Maybe s is of the form t c u.
        // If so, return t, c u (or t, u if cutc == true).
        // If not, return s, "".
        private static (@string, @string) split(@string s, @string c, bool cutc)
        {
            var i = strings.Index(s, c);
            if (i < 0L)
            {
                return (s, "");
            }
            if (cutc)
            {
                return (s[..i], s[i + len(c)..]);
            }
            return (s[..i], s[i..]);
        }

        // Parse parses rawurl into a URL structure.
        //
        // The rawurl may be relative (a path, without a host) or absolute
        // (starting with a scheme). Trying to parse a hostname and path
        // without a scheme is invalid but may not necessarily return an
        // error, due to parsing ambiguities.
        public static (ref URL, error) Parse(@string rawurl)
        { 
            // Cut off #frag
            var (u, frag) = split(rawurl, "#", true);
            var (url, err) = parse(u, false);
            if (err != null)
            {
                return (null, ref new Error("parse",u,err));
            }
            if (frag == "")
            {
                return (url, null);
            }
            url.Fragment, err = unescape(frag, encodeFragment);

            if (err != null)
            {
                return (null, ref new Error("parse",rawurl,err));
            }
            return (url, null);
        }

        // ParseRequestURI parses rawurl into a URL structure. It assumes that
        // rawurl was received in an HTTP request, so the rawurl is interpreted
        // only as an absolute URI or an absolute path.
        // The string rawurl is assumed not to have a #fragment suffix.
        // (Web browsers strip #fragment before sending the URL to a web server.)
        public static (ref URL, error) ParseRequestURI(@string rawurl)
        {
            var (url, err) = parse(rawurl, true);
            if (err != null)
            {
                return (null, ref new Error("parse",rawurl,err));
            }
            return (url, null);
        }

        // parse parses a URL from a string in one of two contexts. If
        // viaRequest is true, the URL is assumed to have arrived via an HTTP request,
        // in which case only absolute URLs or path-absolute relative URLs are allowed.
        // If viaRequest is false, all forms of relative URLs are allowed.
        private static (ref URL, error) parse(@string rawurl, bool viaRequest)
        {
            @string rest = default;
            error err = default;

            if (rawurl == "" && viaRequest)
            {
                return (null, errors.New("empty url"));
            }
            ptr<URL> url = @new<URL>();

            if (rawurl == "*")
            {
                url.Path = "*";
                return (url, null);
            } 

            // Split off possible leading "http:", "mailto:", etc.
            // Cannot contain escaped characters.
            url.Scheme, rest, err = getscheme(rawurl);

            if (err != null)
            {
                return (null, err);
            }
            url.Scheme = strings.ToLower(url.Scheme);

            if (strings.HasSuffix(rest, "?") && strings.Count(rest, "?") == 1L)
            {
                url.ForceQuery = true;
                rest = rest[..len(rest) - 1L];
            }
            else
            {
                rest, url.RawQuery = split(rest, "?", true);
            }
            if (!strings.HasPrefix(rest, "/"))
            {
                if (url.Scheme != "")
                { 
                    // We consider rootless paths per RFC 3986 as opaque.
                    url.Opaque = rest;
                    return (url, null);
                }
                if (viaRequest)
                {
                    return (null, errors.New("invalid URI for request"));
                } 

                // Avoid confusion with malformed schemes, like cache_object:foo/bar.
                // See golang.org/issue/16822.
                //
                // RFC 3986, §3.3:
                // In addition, a URI reference (Section 4.1) may be a relative-path reference,
                // in which case the first path segment cannot contain a colon (":") character.
                var colon = strings.Index(rest, ":");
                var slash = strings.Index(rest, "/");
                if (colon >= 0L && (slash < 0L || colon < slash))
                { 
                    // First path segment has colon. Not allowed in relative URL.
                    return (null, errors.New("first path segment in URL cannot contain colon"));
                }
            }
            if ((url.Scheme != "" || !viaRequest && !strings.HasPrefix(rest, "///")) && strings.HasPrefix(rest, "//"))
            {
                @string authority = default;
                authority, rest = split(rest[2L..], "/", false);
                url.User, url.Host, err = parseAuthority(authority);
                if (err != null)
                {
                    return (null, err);
                }
            } 
            // Set Path and, optionally, RawPath.
            // RawPath is a hint of the encoding of Path. We don't want to set it if
            // the default escaping of Path is equivalent, to help make sure that people
            // don't rely on it in general.
            {
                error err__prev1 = err;

                err = url.setPath(rest);

                if (err != null)
                {
                    return (null, err);
                }

                err = err__prev1;

            }
            return (url, null);
        }

        private static (ref Userinfo, @string, error) parseAuthority(@string authority)
        {
            var i = strings.LastIndex(authority, "@");
            if (i < 0L)
            {
                host, err = parseHost(authority);
            }
            else
            {
                host, err = parseHost(authority[i + 1L..]);
            }
            if (err != null)
            {
                return (null, "", err);
            }
            if (i < 0L)
            {
                return (null, host, null);
            }
            var userinfo = authority[..i];
            if (!validUserinfo(userinfo))
            {
                return (null, "", errors.New("net/url: invalid userinfo"));
            }
            if (!strings.Contains(userinfo, ":"))
            {
                userinfo, err = unescape(userinfo, encodeUserPassword);

                if (err != null)
                {
                    return (null, "", err);
                }
                user = User(userinfo);
            }
            else
            {
                var (username, password) = split(userinfo, ":", true);
                username, err = unescape(username, encodeUserPassword);

                if (err != null)
                {
                    return (null, "", err);
                }
                password, err = unescape(password, encodeUserPassword);

                if (err != null)
                {
                    return (null, "", err);
                }
                user = UserPassword(username, password);
            }
            return (user, host, null);
        }

        // parseHost parses host as an authority without user
        // information. That is, as host[:port].
        private static (@string, error) parseHost(@string host)
        {
            if (strings.HasPrefix(host, "["))
            { 
                // Parse an IP-Literal in RFC 3986 and RFC 6874.
                // E.g., "[fe80::1]", "[fe80::1%25en0]", "[fe80::1]:80".
                var i = strings.LastIndex(host, "]");
                if (i < 0L)
                {
                    return ("", errors.New("missing ']' in host"));
                }
                var colonPort = host[i + 1L..];
                if (!validOptionalPort(colonPort))
                {
                    return ("", fmt.Errorf("invalid port %q after host", colonPort));
                } 

                // RFC 6874 defines that %25 (%-encoded percent) introduces
                // the zone identifier, and the zone identifier can use basically
                // any %-encoding it likes. That's different from the host, which
                // can only %-encode non-ASCII bytes.
                // We do impose some restrictions on the zone, to avoid stupidity
                // like newlines.
                var zone = strings.Index(host[..i], "%25");
                if (zone >= 0L)
                {
                    var (host1, err) = unescape(host[..zone], encodeHost);
                    if (err != null)
                    {
                        return ("", err);
                    }
                    var (host2, err) = unescape(host[zone..i], encodeZone);
                    if (err != null)
                    {
                        return ("", err);
                    }
                    var (host3, err) = unescape(host[i..], encodeHost);
                    if (err != null)
                    {
                        return ("", err);
                    }
                    return (host1 + host2 + host3, null);
                }
            }
            error err = default;
            host, err = unescape(host, encodeHost);

            if (err != null)
            {
                return ("", err);
            }
            return (host, null);
        }

        // setPath sets the Path and RawPath fields of the URL based on the provided
        // escaped path p. It maintains the invariant that RawPath is only specified
        // when it differs from the default encoding of the path.
        // For example:
        // - setPath("/foo/bar")   will set Path="/foo/bar" and RawPath=""
        // - setPath("/foo%2fbar") will set Path="/foo/bar" and RawPath="/foo%2fbar"
        // setPath will return an error only if the provided path contains an invalid
        // escaping.
        private static error setPath(this ref URL u, @string p)
        {
            var (path, err) = unescape(p, encodePath);
            if (err != null)
            {
                return error.As(err);
            }
            u.Path = path;
            {
                var escp = escape(path, encodePath);

                if (p == escp)
                { 
                    // Default encoding is fine.
                    u.RawPath = "";
                }
                else
                {
                    u.RawPath = p;
                }

            }
            return error.As(null);
        }

        // EscapedPath returns the escaped form of u.Path.
        // In general there are multiple possible escaped forms of any path.
        // EscapedPath returns u.RawPath when it is a valid escaping of u.Path.
        // Otherwise EscapedPath ignores u.RawPath and computes an escaped
        // form on its own.
        // The String and RequestURI methods use EscapedPath to construct
        // their results.
        // In general, code should call EscapedPath instead of
        // reading u.RawPath directly.
        private static @string EscapedPath(this ref URL u)
        {
            if (u.RawPath != "" && validEncodedPath(u.RawPath))
            {
                var (p, err) = unescape(u.RawPath, encodePath);
                if (err == null && p == u.Path)
                {
                    return u.RawPath;
                }
            }
            if (u.Path == "*")
            {
                return "*"; // don't escape (Issue 11202)
            }
            return escape(u.Path, encodePath);
        }

        // validEncodedPath reports whether s is a valid encoded path.
        // It must not contain any bytes that require escaping during path encoding.
        private static bool validEncodedPath(@string s)
        {
            for (long i = 0L; i < len(s); i++)
            { 
                // RFC 3986, Appendix A.
                // pchar = unreserved / pct-encoded / sub-delims / ":" / "@".
                // shouldEscape is not quite compliant with the RFC,
                // so we check the sub-delims ourselves and let
                // shouldEscape handle the others.
                switch (s[i])
                {
                    case '!': 

                    case '$': 

                    case '&': 

                    case '\'': 

                    case '(': 

                    case ')': 

                    case '*': 

                    case '+': 

                    case ',': 

                    case ';': 

                    case '=': 

                    case ':': 

                    case '@': 
                        break;
                    case '[': 

                    case ']': 
                        break;
                    case '%': 
                        break;
                    default: 
                        if (shouldEscape(s[i], encodePath))
                        {
                            return false;
                        }
                        break;
                }
            }

            return true;
        }

        // validOptionalPort reports whether port is either an empty string
        // or matches /^:\d*$/
        private static bool validOptionalPort(@string port)
        {
            if (port == "")
            {
                return true;
            }
            if (port[0L] != ':')
            {
                return false;
            }
            foreach (var (_, b) in port[1L..])
            {
                if (b < '0' || b > '9')
                {
                    return false;
                }
            }
            return true;
        }

        // String reassembles the URL into a valid URL string.
        // The general form of the result is one of:
        //
        //    scheme:opaque?query#fragment
        //    scheme://userinfo@host/path?query#fragment
        //
        // If u.Opaque is non-empty, String uses the first form;
        // otherwise it uses the second form.
        // To obtain the path, String uses u.EscapedPath().
        //
        // In the second form, the following rules apply:
        //    - if u.Scheme is empty, scheme: is omitted.
        //    - if u.User is nil, userinfo@ is omitted.
        //    - if u.Host is empty, host/ is omitted.
        //    - if u.Scheme and u.Host are empty and u.User is nil,
        //       the entire scheme://userinfo@host/ is omitted.
        //    - if u.Host is non-empty and u.Path begins with a /,
        //       the form host/path does not add its own /.
        //    - if u.RawQuery is empty, ?query is omitted.
        //    - if u.Fragment is empty, #fragment is omitted.
        private static @string String(this ref URL u)
        {
            bytes.Buffer buf = default;
            if (u.Scheme != "")
            {
                buf.WriteString(u.Scheme);
                buf.WriteByte(':');
            }
            if (u.Opaque != "")
            {
                buf.WriteString(u.Opaque);
            }
            else
            {
                if (u.Scheme != "" || u.Host != "" || u.User != null)
                {
                    if (u.Host != "" || u.Path != "" || u.User != null)
                    {
                        buf.WriteString("//");
                    }
                    {
                        var ui = u.User;

                        if (ui != null)
                        {
                            buf.WriteString(ui.String());
                            buf.WriteByte('@');
                        }

                    }
                    {
                        var h = u.Host;

                        if (h != "")
                        {
                            buf.WriteString(escape(h, encodeHost));
                        }

                    }
                }
                var path = u.EscapedPath();
                if (path != "" && path[0L] != '/' && u.Host != "")
                {
                    buf.WriteByte('/');
                }
                if (buf.Len() == 0L)
                { 
                    // RFC 3986 §4.2
                    // A path segment that contains a colon character (e.g., "this:that")
                    // cannot be used as the first segment of a relative-path reference, as
                    // it would be mistaken for a scheme name. Such a segment must be
                    // preceded by a dot-segment (e.g., "./this:that") to make a relative-
                    // path reference.
                    {
                        var i = strings.IndexByte(path, ':');

                        if (i > -1L && strings.IndexByte(path[..i], '/') == -1L)
                        {
                            buf.WriteString("./");
                        }

                    }
                }
                buf.WriteString(path);
            }
            if (u.ForceQuery || u.RawQuery != "")
            {
                buf.WriteByte('?');
                buf.WriteString(u.RawQuery);
            }
            if (u.Fragment != "")
            {
                buf.WriteByte('#');
                buf.WriteString(escape(u.Fragment, encodeFragment));
            }
            return buf.String();
        }

        // Values maps a string key to a list of values.
        // It is typically used for query parameters and form values.
        // Unlike in the http.Header map, the keys in a Values map
        // are case-sensitive.
        public partial struct Values // : map<@string, slice<@string>>
        {
        }

        // Get gets the first value associated with the given key.
        // If there are no values associated with the key, Get returns
        // the empty string. To access multiple values, use the map
        // directly.
        public static @string Get(this Values v, @string key)
        {
            if (v == null)
            {
                return "";
            }
            var vs = v[key];
            if (len(vs) == 0L)
            {
                return "";
            }
            return vs[0L];
        }

        // Set sets the key to value. It replaces any existing
        // values.
        public static void Set(this Values v, @string key, @string value)
        {
            v[key] = new slice<@string>(new @string[] { value });
        }

        // Add adds the value to key. It appends to any existing
        // values associated with key.
        public static void Add(this Values v, @string key, @string value)
        {
            v[key] = append(v[key], value);
        }

        // Del deletes the values associated with key.
        public static void Del(this Values v, @string key)
        {
            delete(v, key);
        }

        // ParseQuery parses the URL-encoded query string and returns
        // a map listing the values specified for each key.
        // ParseQuery always returns a non-nil map containing all the
        // valid query parameters found; err describes the first decoding error
        // encountered, if any.
        //
        // Query is expected to be a list of key=value settings separated by
        // ampersands or semicolons. A setting without an equals sign is
        // interpreted as a key set to an empty value.
        public static (Values, error) ParseQuery(@string query)
        {
            var m = make(Values);
            var err = parseQuery(m, query);
            return (m, err);
        }

        private static error parseQuery(Values m, @string query)
        {
            while (query != "")
            {
                var key = query;
                {
                    var i__prev1 = i;

                    var i = strings.IndexAny(key, "&;");

                    if (i >= 0L)
                    {
                        key = key[..i];
                        query = key[i + 1L..];
                    }
                    else
                    {
                        query = "";
                    }

                    i = i__prev1;

                }
                if (key == "")
                {
                    continue;
                }
                @string value = "";
                {
                    var i__prev1 = i;

                    i = strings.Index(key, "=");

                    if (i >= 0L)
                    {
                        key = key[..i];
                        value = key[i + 1L..];
                    }

                    i = i__prev1;

                }
                var (key, err1) = QueryUnescape(key);
                if (err1 != null)
                {
                    if (err == null)
                    {
                        err = err1;
                    }
                    continue;
                }
                value, err1 = QueryUnescape(value);
                if (err1 != null)
                {
                    if (err == null)
                    {
                        err = err1;
                    }
                    continue;
                }
                m[key] = append(m[key], value);
            }

            return error.As(err);
        }

        // Encode encodes the values into ``URL encoded'' form
        // ("bar=baz&foo=quux") sorted by key.
        public static @string Encode(this Values v)
        {
            if (v == null)
            {
                return "";
            }
            bytes.Buffer buf = default;
            var keys = make_slice<@string>(0L, len(v));
            {
                var k__prev1 = k;

                foreach (var (__k) in v)
                {
                    k = __k;
                    keys = append(keys, k);
                }

                k = k__prev1;
            }

            sort.Strings(keys);
            {
                var k__prev1 = k;

                foreach (var (_, __k) in keys)
                {
                    k = __k;
                    var vs = v[k];
                    var prefix = QueryEscape(k) + "=";
                    foreach (var (_, v) in vs)
                    {
                        if (buf.Len() > 0L)
                        {
                            buf.WriteByte('&');
                        }
                        buf.WriteString(prefix);
                        buf.WriteString(QueryEscape(v));
                    }
                }

                k = k__prev1;
            }

            return buf.String();
        }

        // resolvePath applies special path segments from refs and applies
        // them to base, per RFC 3986.
        private static @string resolvePath(@string @base, @string @ref)
        {
            @string full = default;
            if (ref == "")
            {
                full = base;
            }
            else if (ref[0L] != '/')
            {
                var i = strings.LastIndex(base, "/");
                full = base[..i + 1L] + ref;
            }
            else
            {
                full = ref;
            }
            if (full == "")
            {
                return "";
            }
            slice<@string> dst = default;
            var src = strings.Split(full, "/");
            foreach (var (_, elem) in src)
            {
                switch (elem)
                {
                    case ".": 
                        break;
                    case "..": 
                        if (len(dst) > 0L)
                        {
                            dst = dst[..len(dst) - 1L];
                        }
                        break;
                    default: 
                        dst = append(dst, elem);
                        break;
                }
            }
            {
                var last = src[len(src) - 1L];

                if (last == "." || last == "..")
                { 
                    // Add final slash to the joined path.
                    dst = append(dst, "");
                }

            }
            return "/" + strings.TrimPrefix(strings.Join(dst, "/"), "/");
        }

        // IsAbs reports whether the URL is absolute.
        // Absolute means that it has a non-empty scheme.
        private static bool IsAbs(this ref URL u)
        {
            return u.Scheme != "";
        }

        // Parse parses a URL in the context of the receiver. The provided URL
        // may be relative or absolute. Parse returns nil, err on parse
        // failure, otherwise its return value is the same as ResolveReference.
        private static (ref URL, error) Parse(this ref URL u, @string @ref)
        {
            var (refurl, err) = Parse(ref);
            if (err != null)
            {
                return (null, err);
            }
            return (u.ResolveReference(refurl), null);
        }

        // ResolveReference resolves a URI reference to an absolute URI from
        // an absolute base URI, per RFC 3986 Section 5.2.  The URI reference
        // may be relative or absolute. ResolveReference always returns a new
        // URL instance, even if the returned URL is identical to either the
        // base or reference. If ref is an absolute URL, then ResolveReference
        // ignores base and returns a copy of ref.
        private static ref URL ResolveReference(this ref URL u, ref URL @ref)
        {
            var url = ref.Value;
            if (@ref.Scheme == "")
            {
                url.Scheme = u.Scheme;
            }
            if (@ref.Scheme != "" || @ref.Host != "" || @ref.User != null)
            { 
                // The "absoluteURI" or "net_path" cases.
                // We can ignore the error from setPath since we know we provided a
                // validly-escaped path.
                url.setPath(resolvePath(@ref.EscapedPath(), ""));
                return ref url;
            }
            if (@ref.Opaque != "")
            {
                url.User = null;
                url.Host = "";
                url.Path = "";
                return ref url;
            }
            if (@ref.Path == "" && @ref.RawQuery == "")
            {
                url.RawQuery = u.RawQuery;
                if (@ref.Fragment == "")
                {
                    url.Fragment = u.Fragment;
                }
            } 
            // The "abs_path" or "rel_path" cases.
            url.Host = u.Host;
            url.User = u.User;
            url.setPath(resolvePath(u.EscapedPath(), @ref.EscapedPath()));
            return ref url;
        }

        // Query parses RawQuery and returns the corresponding values.
        // It silently discards malformed value pairs.
        // To check errors use ParseQuery.
        private static Values Query(this ref URL u)
        {
            var (v, _) = ParseQuery(u.RawQuery);
            return v;
        }

        // RequestURI returns the encoded path?query or opaque?query
        // string that would be used in an HTTP request for u.
        private static @string RequestURI(this ref URL u)
        {
            var result = u.Opaque;
            if (result == "")
            {
                result = u.EscapedPath();
                if (result == "")
                {
                    result = "/";
                }
            }
            else
            {
                if (strings.HasPrefix(result, "//"))
                {
                    result = u.Scheme + ":" + result;
                }
            }
            if (u.ForceQuery || u.RawQuery != "")
            {
                result += "?" + u.RawQuery;
            }
            return result;
        }

        // Hostname returns u.Host, without any port number.
        //
        // If Host is an IPv6 literal with a port number, Hostname returns the
        // IPv6 literal without the square brackets. IPv6 literals may include
        // a zone identifier.
        private static @string Hostname(this ref URL u)
        {
            return stripPort(u.Host);
        }

        // Port returns the port part of u.Host, without the leading colon.
        // If u.Host doesn't contain a port, Port returns an empty string.
        private static @string Port(this ref URL u)
        {
            return portOnly(u.Host);
        }

        private static @string stripPort(@string hostport)
        {
            var colon = strings.IndexByte(hostport, ':');
            if (colon == -1L)
            {
                return hostport;
            }
            {
                var i = strings.IndexByte(hostport, ']');

                if (i != -1L)
                {
                    return strings.TrimPrefix(hostport[..i], "[");
                }

            }
            return hostport[..colon];
        }

        private static @string portOnly(@string hostport)
        {
            var colon = strings.IndexByte(hostport, ':');
            if (colon == -1L)
            {
                return "";
            }
            {
                var i = strings.Index(hostport, "]:");

                if (i != -1L)
                {
                    return hostport[i + len("]:")..];
                }

            }
            if (strings.Contains(hostport, "]"))
            {
                return "";
            }
            return hostport[colon + len(":")..];
        }

        // Marshaling interface implementations.
        // Would like to implement MarshalText/UnmarshalText but that will change the JSON representation of URLs.

        private static (slice<byte>, error) MarshalBinary(this ref URL u)
        {
            return ((slice<byte>)u.String(), null);
        }

        private static error UnmarshalBinary(this ref URL u, slice<byte> text)
        {
            var (u1, err) = Parse(string(text));
            if (err != null)
            {
                return error.As(err);
            }
            u.Value = u1.Value;
            return error.As(null);
        }

        // validUserinfo reports whether s is a valid userinfo string per RFC 3986
        // Section 3.2.1:
        //     userinfo    = *( unreserved / pct-encoded / sub-delims / ":" )
        //     unreserved  = ALPHA / DIGIT / "-" / "." / "_" / "~"
        //     sub-delims  = "!" / "$" / "&" / "'" / "(" / ")"
        //                   / "*" / "+" / "," / ";" / "="
        //
        // It doesn't validate pct-encoded. The caller does that via func unescape.
        private static bool validUserinfo(@string s)
        {
            foreach (var (_, r) in s)
            {
                if ('A' <= r && r <= 'Z')
                {
                    continue;
                }
                if ('a' <= r && r <= 'z')
                {
                    continue;
                }
                if ('0' <= r && r <= '9')
                {
                    continue;
                }
                switch (r)
                {
                    case '-': 

                    case '.': 

                    case '_': 

                    case ':': 

                    case '~': 

                    case '!': 

                    case '$': 

                    case '&': 

                    case '\'': 

                    case '(': 

                    case ')': 

                    case '*': 

                    case '+': 

                    case ',': 

                    case ';': 

                    case '=': 

                    case '%': 

                    case '@': 
                        continue;
                        break;
                    default: 
                        return false;
                        break;
                }
            }
            return true;
        }
    }
}}
