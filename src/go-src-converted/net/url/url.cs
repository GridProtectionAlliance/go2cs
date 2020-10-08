// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package url parses URLs and implements query escaping.
// package url -- go2cs converted at 2020 October 08 03:35:13 UTC
// import "net/url" ==> using url = go.net.url_package
// Original source: C:\Go\src\net\url\url.go
// See RFC 3986. This package generally follows RFC 3986, except where
// it deviates for compatibility reasons. When sending changes, first
// search old issues for history on decisions. Unit tests should also
// contain references to issue numbers with details.

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

        private static error Unwrap(this ptr<Error> _addr_e)
        {
            ref Error e = ref _addr_e.val;

            return error.As(e.Err)!;
        }
        private static @string Error(this ptr<Error> _addr_e)
        {
            ref Error e = ref _addr_e.val;

            return fmt.Sprintf("%s %q: %s", e.Op, e.URL, e.Err);
        }

        private static bool Timeout(this ptr<Error> _addr_e)
        {
            ref Error e = ref _addr_e.val;

            return ok && t.Timeout();
        }

        private static bool Temporary(this ptr<Error> _addr_e)
        {
            ref Error e = ref _addr_e.val;

            return ok && t.Temporary();
        }

        private static readonly @string upperhex = (@string)"0123456789ABCDEF";



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

        private static readonly encoding encodePath = (encoding)1L + iota;
        private static readonly var encodePathSegment = (var)0;
        private static readonly var encodeHost = (var)1;
        private static readonly var encodeZone = (var)2;
        private static readonly var encodeUserPassword = (var)3;
        private static readonly var encodeQueryComponent = (var)4;
        private static readonly var encodeFragment = (var)5;


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
            if ('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z' || '0' <= c && c <= '9')
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

            if (mode == encodeFragment)
            { 
                // RFC 3986 §2.2 allows not escaping sub-delims. A subset of sub-delims are
                // included in reserved from RFC 2396 §2.2. The remaining sub-delims do not
                // need to be escaped. To minimize potential breakage, we apply two restrictions:
                // (1) we always escape sub-delims outside of the fragment, and (2) we always
                // escape single quote to avoid breaking callers that had previously assumed that
                // single quotes would be escaped. See issue #19917.
                switch (c)
                {
                    case '!': 

                    case '(': 

                    case ')': 

                    case '*': 
                        return false;
                        break;
                }

            } 

            // Everything else must be escaped.
            return true;

        }

        // QueryUnescape does the inverse transformation of QueryEscape,
        // converting each 3-byte encoded substring of the form "%AB" into the
        // hex-decoded byte 0xAB.
        // It returns an error if any % is not followed by two hexadecimal
        // digits.
        public static (@string, error) QueryUnescape(@string s)
        {
            @string _p0 = default;
            error _p0 = default!;

            return unescape(s, encodeQueryComponent);
        }

        // PathUnescape does the inverse transformation of PathEscape,
        // converting each 3-byte encoded substring of the form "%AB" into the
        // hex-decoded byte 0xAB. It returns an error if any % is not followed
        // by two hexadecimal digits.
        //
        // PathUnescape is identical to QueryUnescape except that it does not
        // unescape '+' to ' ' (space).
        public static (@string, error) PathUnescape(@string s)
        {
            @string _p0 = default;
            error _p0 = default!;

            return unescape(s, encodePathSegment);
        }

        // unescape unescapes a string; the mode specifies
        // which section of the URL string is being unescaped.
        private static (@string, error) unescape(@string s, encoding mode)
        {
            @string _p0 = default;
            error _p0 = default!;
 
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

                                return ("", error.As(EscapeError(s))!);

                            } 
                            // Per https://tools.ietf.org/html/rfc3986#page-21
                            // in the host component %-encoding can only be used
                            // for non-ASCII bytes.
                            // But https://tools.ietf.org/html/rfc6874#section-2
                            // introduces %25 being allowed to escape a percent sign
                            // in IPv6 scoped-address literals. Yay.
                            if (mode == encodeHost && unhex(s[i + 1L]) < 8L && s[i..i + 3L] != "%25")
                            {
                                return ("", error.As(EscapeError(s[i..i + 3L]))!);
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
                                    return ("", error.As(EscapeError(s[i..i + 3L]))!);
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
                                return ("", error.As(InvalidHostError(s[i..i + 1L]))!);
                            }

                            i++;
                            break;
                    }

                }


                i = i__prev1;
            }

            if (n == 0L && !hasPlus)
            {
                return (s, error.As(null!)!);
            }

            strings.Builder t = default;
            t.Grow(len(s) - 2L * n);
            {
                long i__prev1 = i;

                for (i = 0L; i < len(s); i++)
                {
                    switch (s[i])
                    {
                        case '%': 
                            t.WriteByte(unhex(s[i + 1L]) << (int)(4L) | unhex(s[i + 2L]));
                            i += 2L;
                            break;
                        case '+': 
                            if (mode == encodeQueryComponent)
                            {
                                t.WriteByte(' ');
                            }
                            else
                            {
                                t.WriteByte('+');
                            }

                            break;
                        default: 
                            t.WriteByte(s[i]);
                            break;
                    }

                }


                i = i__prev1;
            }
            return (t.String(), error.As(null!)!);

        }

        // QueryEscape escapes the string so it can be safely placed
        // inside a URL query.
        public static @string QueryEscape(@string s)
        {
            return escape(s, encodeQueryComponent);
        }

        // PathEscape escapes the string so it can be safely placed inside a URL path segment,
        // replacing special characters (including /) with %XX sequences as needed.
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

            array<byte> buf = new array<byte>(64L);
            slice<byte> t = default;

            var required = len(s) + 2L * hexCount;
            if (required <= len(buf))
            {
                t = buf[..required];
            }
            else
            {
                t = make_slice<byte>(required);
            }

            if (hexCount == 0L)
            {
                copy(t, s);
                {
                    long i__prev1 = i;

                    for (i = 0L; i < len(s); i++)
                    {
                        if (s[i] == ' ')
                        {
                            t[i] = '+';
                        }

                    }


                    i = i__prev1;
                }
                return string(t);

            }

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
                            t[j + 1L] = upperhex[c >> (int)(4L)];
                            t[j + 2L] = upperhex[c & 15L];
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
        // but when it is, the code should use RawPath, an optional field which only gets
        // set if the default encoding is different from Path.
        //
        // URL's String method uses the EscapedPath method to obtain the path. See the
        // EscapedPath method for more details.
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
            public @string RawFragment; // encoded fragment hint (see EscapedFragment method)
        }

        // User returns a Userinfo containing the provided username
        // and no password set.
        public static ptr<Userinfo> User(@string username)
        {
            return addr(new Userinfo(username,"",false));
        }

        // UserPassword returns a Userinfo containing the provided username
        // and password.
        //
        // This functionality should only be used with legacy web sites.
        // RFC 2396 warns that interpreting Userinfo this way
        // ``is NOT RECOMMENDED, because the passing of authentication
        // information in clear text (such as URI) has proven to be a
        // security risk in almost every case where it has been used.''
        public static ptr<Userinfo> UserPassword(@string username, @string password)
        {
            return addr(new Userinfo(username,password,true));
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
        private static @string Username(this ptr<Userinfo> _addr_u)
        {
            ref Userinfo u = ref _addr_u.val;

            if (u == null)
            {
                return "";
            }

            return u.username;

        }

        // Password returns the password in case it is set, and whether it is set.
        private static (@string, bool) Password(this ptr<Userinfo> _addr_u)
        {
            @string _p0 = default;
            bool _p0 = default;
            ref Userinfo u = ref _addr_u.val;

            if (u == null)
            {
                return ("", false);
            }

            return (u.password, u.passwordSet);

        }

        // String returns the encoded userinfo information in the standard form
        // of "username[:password]".
        private static @string String(this ptr<Userinfo> _addr_u)
        {
            ref Userinfo u = ref _addr_u.val;

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
            @string scheme = default;
            @string path = default;
            error err = default!;

            for (long i = 0L; i < len(rawurl); i++)
            {
                var c = rawurl[i];

                if ('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z')                 else if ('0' <= c && c <= '9' || c == '+' || c == '-' || c == '.') 
                    if (i == 0L)
                    {
                        return ("", rawurl, error.As(null!)!);
                    }

                else if (c == ':') 
                    if (i == 0L)
                    {
                        return ("", "", error.As(errors.New("missing protocol scheme"))!);
                    }

                    return (rawurl[..i], rawurl[i + 1L..], error.As(null!)!);
                else 
                    // we have encountered an invalid character,
                    // so there is no valid scheme
                    return ("", rawurl, error.As(null!)!);
                
            }

            return ("", rawurl, error.As(null!)!);

        }

        // split slices s into two substrings separated by the first occurrence of
        // sep. If cutc is true then sep is excluded from the second substring.
        // If sep does not occur in s then s and the empty string is returned.
        private static (@string, @string) split(@string s, byte sep, bool cutc)
        {
            @string _p0 = default;
            @string _p0 = default;

            var i = strings.IndexByte(s, sep);
            if (i < 0L)
            {
                return (s, "");
            }

            if (cutc)
            {
                return (s[..i], s[i + 1L..]);
            }

            return (s[..i], s[i..]);

        }

        // Parse parses rawurl into a URL structure.
        //
        // The rawurl may be relative (a path, without a host) or absolute
        // (starting with a scheme). Trying to parse a hostname and path
        // without a scheme is invalid but may not necessarily return an
        // error, due to parsing ambiguities.
        public static (ptr<URL>, error) Parse(@string rawurl)
        {
            ptr<URL> _p0 = default!;
            error _p0 = default!;
 
            // Cut off #frag
            var (u, frag) = split(rawurl, '#', true);
            var (url, err) = parse(u, false);
            if (err != null)
            {
                return (_addr_null!, error.As(addr(new Error("parse",u,err))!)!);
            }

            if (frag == "")
            {
                return (_addr_url!, error.As(null!)!);
            }

            err = url.setFragment(frag);

            if (err != null)
            {
                return (_addr_null!, error.As(addr(new Error("parse",rawurl,err))!)!);
            }

            return (_addr_url!, error.As(null!)!);

        }

        // ParseRequestURI parses rawurl into a URL structure. It assumes that
        // rawurl was received in an HTTP request, so the rawurl is interpreted
        // only as an absolute URI or an absolute path.
        // The string rawurl is assumed not to have a #fragment suffix.
        // (Web browsers strip #fragment before sending the URL to a web server.)
        public static (ptr<URL>, error) ParseRequestURI(@string rawurl)
        {
            ptr<URL> _p0 = default!;
            error _p0 = default!;

            var (url, err) = parse(rawurl, true);
            if (err != null)
            {
                return (_addr_null!, error.As(addr(new Error("parse",rawurl,err))!)!);
            }

            return (_addr_url!, error.As(null!)!);

        }

        // parse parses a URL from a string in one of two contexts. If
        // viaRequest is true, the URL is assumed to have arrived via an HTTP request,
        // in which case only absolute URLs or path-absolute relative URLs are allowed.
        // If viaRequest is false, all forms of relative URLs are allowed.
        private static (ptr<URL>, error) parse(@string rawurl, bool viaRequest)
        {
            ptr<URL> _p0 = default!;
            error _p0 = default!;

            @string rest = default;
            error err = default!;

            if (stringContainsCTLByte(rawurl))
            {
                return (_addr_null!, error.As(errors.New("net/url: invalid control character in URL"))!);
            }

            if (rawurl == "" && viaRequest)
            {
                return (_addr_null!, error.As(errors.New("empty url"))!);
            }

            ptr<URL> url = @new<URL>();

            if (rawurl == "*")
            {
                url.Path = "*";
                return (_addr_url!, error.As(null!)!);
            } 

            // Split off possible leading "http:", "mailto:", etc.
            // Cannot contain escaped characters.
            url.Scheme, rest, err = getscheme(rawurl);

            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            url.Scheme = strings.ToLower(url.Scheme);

            if (strings.HasSuffix(rest, "?") && strings.Count(rest, "?") == 1L)
            {
                url.ForceQuery = true;
                rest = rest[..len(rest) - 1L];
            }
            else
            {
                rest, url.RawQuery = split(rest, '?', true);
            }

            if (!strings.HasPrefix(rest, "/"))
            {
                if (url.Scheme != "")
                { 
                    // We consider rootless paths per RFC 3986 as opaque.
                    url.Opaque = rest;
                    return (_addr_url!, error.As(null!)!);

                }

                if (viaRequest)
                {
                    return (_addr_null!, error.As(errors.New("invalid URI for request"))!);
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
                    return (_addr_null!, error.As(errors.New("first path segment in URL cannot contain colon"))!);

                }

            }

            if ((url.Scheme != "" || !viaRequest && !strings.HasPrefix(rest, "///")) && strings.HasPrefix(rest, "//"))
            {
                @string authority = default;
                authority, rest = split(rest[2L..], '/', false);
                url.User, url.Host, err = parseAuthority(authority);
                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
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
                    return (_addr_null!, error.As(err)!);
                }

                err = err__prev1;

            }

            return (_addr_url!, error.As(null!)!);

        }

        private static (ptr<Userinfo>, @string, error) parseAuthority(@string authority)
        {
            ptr<Userinfo> user = default!;
            @string host = default;
            error err = default!;

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
                return (_addr_null!, "", error.As(err)!);
            }

            if (i < 0L)
            {
                return (_addr_null!, host, error.As(null!)!);
            }

            var userinfo = authority[..i];
            if (!validUserinfo(userinfo))
            {
                return (_addr_null!, "", error.As(errors.New("net/url: invalid userinfo"))!);
            }

            if (!strings.Contains(userinfo, ":"))
            {
                userinfo, err = unescape(userinfo, encodeUserPassword);

                if (err != null)
                {
                    return (_addr_null!, "", error.As(err)!);
                }

                user = User(userinfo);

            }
            else
            {
                var (username, password) = split(userinfo, ':', true);
                username, err = unescape(username, encodeUserPassword);

                if (err != null)
                {
                    return (_addr_null!, "", error.As(err)!);
                }

                password, err = unescape(password, encodeUserPassword);

                if (err != null)
                {
                    return (_addr_null!, "", error.As(err)!);
                }

                user = UserPassword(username, password);

            }

            return (_addr_user!, host, error.As(null!)!);

        }

        // parseHost parses host as an authority without user
        // information. That is, as host[:port].
        private static (@string, error) parseHost(@string host)
        {
            @string _p0 = default;
            error _p0 = default!;

            if (strings.HasPrefix(host, "["))
            { 
                // Parse an IP-Literal in RFC 3986 and RFC 6874.
                // E.g., "[fe80::1]", "[fe80::1%25en0]", "[fe80::1]:80".
                var i = strings.LastIndex(host, "]");
                if (i < 0L)
                {
                    return ("", error.As(errors.New("missing ']' in host"))!);
                }

                var colonPort = host[i + 1L..];
                if (!validOptionalPort(colonPort))
                {
                    return ("", error.As(fmt.Errorf("invalid port %q after host", colonPort))!);
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
                        return ("", error.As(err)!);
                    }

                    var (host2, err) = unescape(host[zone..i], encodeZone);
                    if (err != null)
                    {
                        return ("", error.As(err)!);
                    }

                    var (host3, err) = unescape(host[i..], encodeHost);
                    if (err != null)
                    {
                        return ("", error.As(err)!);
                    }

                    return (host1 + host2 + host3, error.As(null!)!);

                }

            }            {
                var i__prev2 = i;

                i = strings.LastIndex(host, ":");


                else if (i != -1L)
                {
                    colonPort = host[i..];
                    if (!validOptionalPort(colonPort))
                    {
                        return ("", error.As(fmt.Errorf("invalid port %q after host", colonPort))!);
                    }

                }

                i = i__prev2;

            }


            error err = default!;
            host, err = unescape(host, encodeHost);

            if (err != null)
            {
                return ("", error.As(err)!);
            }

            return (host, error.As(null!)!);

        }

        // setPath sets the Path and RawPath fields of the URL based on the provided
        // escaped path p. It maintains the invariant that RawPath is only specified
        // when it differs from the default encoding of the path.
        // For example:
        // - setPath("/foo/bar")   will set Path="/foo/bar" and RawPath=""
        // - setPath("/foo%2fbar") will set Path="/foo/bar" and RawPath="/foo%2fbar"
        // setPath will return an error only if the provided path contains an invalid
        // escaping.
        private static error setPath(this ptr<URL> _addr_u, @string p)
        {
            ref URL u = ref _addr_u.val;

            var (path, err) = unescape(p, encodePath);
            if (err != null)
            {
                return error.As(err)!;
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

            return error.As(null!)!;

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
        private static @string EscapedPath(this ptr<URL> _addr_u)
        {
            ref URL u = ref _addr_u.val;

            if (u.RawPath != "" && validEncoded(u.RawPath, encodePath))
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

        // validEncoded reports whether s is a valid encoded path or fragment,
        // according to mode.
        // It must not contain any bytes that require escaping during encoding.
        private static bool validEncoded(@string s, encoding mode)
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
                        if (shouldEscape(s[i], mode))
                        {
                            return false;
                        }

                        break;
                }

            }

            return true;

        }

        // setFragment is like setPath but for Fragment/RawFragment.
        private static error setFragment(this ptr<URL> _addr_u, @string f)
        {
            ref URL u = ref _addr_u.val;

            var (frag, err) = unescape(f, encodeFragment);
            if (err != null)
            {
                return error.As(err)!;
            }

            u.Fragment = frag;
            {
                var escf = escape(frag, encodeFragment);

                if (f == escf)
                { 
                    // Default encoding is fine.
                    u.RawFragment = "";

                }
                else
                {
                    u.RawFragment = f;
                }

            }

            return error.As(null!)!;

        }

        // EscapedFragment returns the escaped form of u.Fragment.
        // In general there are multiple possible escaped forms of any fragment.
        // EscapedFragment returns u.RawFragment when it is a valid escaping of u.Fragment.
        // Otherwise EscapedFragment ignores u.RawFragment and computes an escaped
        // form on its own.
        // The String method uses EscapedFragment to construct its result.
        // In general, code should call EscapedFragment instead of
        // reading u.RawFragment directly.
        private static @string EscapedFragment(this ptr<URL> _addr_u)
        {
            ref URL u = ref _addr_u.val;

            if (u.RawFragment != "" && validEncoded(u.RawFragment, encodeFragment))
            {
                var (f, err) = unescape(u.RawFragment, encodeFragment);
                if (err == null && f == u.Fragment)
                {
                    return u.RawFragment;
                }

            }

            return escape(u.Fragment, encodeFragment);

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
        // Any non-ASCII characters in host are escaped.
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
        private static @string String(this ptr<URL> _addr_u)
        {
            ref URL u = ref _addr_u.val;

            strings.Builder buf = default;
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
                buf.WriteString(u.EscapedFragment());
            }

            return buf.String();

        }

        // Redacted is like String but replaces any password with "xxxxx".
        // Only the password in u.URL is redacted.
        private static @string Redacted(this ptr<URL> _addr_u)
        {
            ref URL u = ref _addr_u.val;

            if (u == null)
            {
                return "";
            }

            var ru = u.val;
            {
                var (_, has) = ru.User.Password();

                if (has)
                {
                    ru.User = UserPassword(ru.User.Username(), "xxxxx");
                }

            }

            return ru.String();

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
            Values _p0 = default;
            error _p0 = default!;

            var m = make(Values);
            var err = parseQuery(m, query);
            return (m, error.As(err)!);
        }

        private static error parseQuery(Values m, @string query)
        {
            error err = default!;

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

            return error.As(err)!;

        }

        // Encode encodes the values into ``URL encoded'' form
        // ("bar=baz&foo=quux") sorted by key.
        public static @string Encode(this Values v)
        {
            if (v == null)
            {
                return "";
            }

            strings.Builder buf = default;
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
                    var keyEscaped = QueryEscape(k);
                    foreach (var (_, v) in vs)
                    {
                        if (buf.Len() > 0L)
                        {
                            buf.WriteByte('&');
                        }

                        buf.WriteString(keyEscaped);
                        buf.WriteByte('=');
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

            var src = strings.Split(full, "/");
            var dst = make_slice<@string>(0L, len(src));
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
        private static bool IsAbs(this ptr<URL> _addr_u)
        {
            ref URL u = ref _addr_u.val;

            return u.Scheme != "";
        }

        // Parse parses a URL in the context of the receiver. The provided URL
        // may be relative or absolute. Parse returns nil, err on parse
        // failure, otherwise its return value is the same as ResolveReference.
        private static (ptr<URL>, error) Parse(this ptr<URL> _addr_u, @string @ref)
        {
            ptr<URL> _p0 = default!;
            error _p0 = default!;
            ref URL u = ref _addr_u.val;

            var (refurl, err) = Parse(ref);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (_addr_u.ResolveReference(refurl)!, error.As(null!)!);

        }

        // ResolveReference resolves a URI reference to an absolute URI from
        // an absolute base URI u, per RFC 3986 Section 5.2. The URI reference
        // may be relative or absolute. ResolveReference always returns a new
        // URL instance, even if the returned URL is identical to either the
        // base or reference. If ref is an absolute URL, then ResolveReference
        // ignores base and returns a copy of ref.
        private static ptr<URL> ResolveReference(this ptr<URL> _addr_u, ptr<URL> _addr_@ref)
        {
            ref URL u = ref _addr_u.val;
            ref URL @ref = ref _addr_@ref.val;

            ref var url = ref heap(ref.val, out ptr<var> _addr_url);
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
                return _addr__addr_url!;

            }

            if (@ref.Opaque != "")
            {
                url.User = null;
                url.Host = "";
                url.Path = "";
                return _addr__addr_url!;
            }

            if (@ref.Path == "" && @ref.RawQuery == "")
            {
                url.RawQuery = u.RawQuery;
                if (@ref.Fragment == "")
                {
                    url.Fragment = u.Fragment;
                    url.RawFragment = u.RawFragment;
                }

            } 
            // The "abs_path" or "rel_path" cases.
            url.Host = u.Host;
            url.User = u.User;
            url.setPath(resolvePath(u.EscapedPath(), @ref.EscapedPath()));
            return _addr__addr_url!;

        }

        // Query parses RawQuery and returns the corresponding values.
        // It silently discards malformed value pairs.
        // To check errors use ParseQuery.
        private static Values Query(this ptr<URL> _addr_u)
        {
            ref URL u = ref _addr_u.val;

            var (v, _) = ParseQuery(u.RawQuery);
            return v;
        }

        // RequestURI returns the encoded path?query or opaque?query
        // string that would be used in an HTTP request for u.
        private static @string RequestURI(this ptr<URL> _addr_u)
        {
            ref URL u = ref _addr_u.val;

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

        // Hostname returns u.Host, stripping any valid port number if present.
        //
        // If the result is enclosed in square brackets, as literal IPv6 addresses are,
        // the square brackets are removed from the result.
        private static @string Hostname(this ptr<URL> _addr_u)
        {
            ref URL u = ref _addr_u.val;

            var (host, _) = splitHostPort(u.Host);
            return host;
        }

        // Port returns the port part of u.Host, without the leading colon.
        //
        // If u.Host doesn't contain a valid numeric port, Port returns an empty string.
        private static @string Port(this ptr<URL> _addr_u)
        {
            ref URL u = ref _addr_u.val;

            var (_, port) = splitHostPort(u.Host);
            return port;
        }

        // splitHostPort separates host and port. If the port is not valid, it returns
        // the entire input as host, and it doesn't check the validity of the host.
        // Unlike net.SplitHostPort, but per RFC 3986, it requires ports to be numeric.
        private static (@string, @string) splitHostPort(@string hostport)
        {
            @string host = default;
            @string port = default;

            host = hostport;

            var colon = strings.LastIndexByte(host, ':');
            if (colon != -1L && validOptionalPort(host[colon..]))
            {
                host = host[..colon];
                port = host[colon + 1L..];

            }

            if (strings.HasPrefix(host, "[") && strings.HasSuffix(host, "]"))
            {
                host = host[1L..len(host) - 1L];
            }

            return ;

        }

        // Marshaling interface implementations.
        // Would like to implement MarshalText/UnmarshalText but that will change the JSON representation of URLs.

        private static (slice<byte>, error) MarshalBinary(this ptr<URL> _addr_u)
        {
            slice<byte> text = default;
            error err = default!;
            ref URL u = ref _addr_u.val;

            return ((slice<byte>)u.String(), error.As(null!)!);
        }

        private static error UnmarshalBinary(this ptr<URL> _addr_u, slice<byte> text)
        {
            ref URL u = ref _addr_u.val;

            var (u1, err) = Parse(string(text));
            if (err != null)
            {
                return error.As(err)!;
            }

            u.val = u1.val;
            return error.As(null!)!;

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

        // stringContainsCTLByte reports whether s contains any ASCII control character.
        private static bool stringContainsCTLByte(@string s)
        {
            for (long i = 0L; i < len(s); i++)
            {
                var b = s[i];
                if (b < ' ' || b == 0x7fUL)
                {
                    return true;
                }

            }

            return false;

        }
    }
}}
