// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package http -- go2cs converted at 2020 October 08 03:38:37 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Go\src\net\http\cookie.go
using log = go.log_package;
using net = go.net_package;
using textproto = go.net.textproto_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;
using System;

namespace go {
namespace net
{
    public static partial class http_package
    {
        // A Cookie represents an HTTP cookie as sent in the Set-Cookie header of an
        // HTTP response or the Cookie header of an HTTP request.
        //
        // See https://tools.ietf.org/html/rfc6265 for details.
        public partial struct Cookie
        {
            public @string Name;
            public @string Value;
            public @string Path; // optional
            public @string Domain; // optional
            public time.Time Expires; // optional
            public @string RawExpires; // for reading cookies only

// MaxAge=0 means no 'Max-Age' attribute specified.
// MaxAge<0 means delete cookie now, equivalently 'Max-Age: 0'
// MaxAge>0 means Max-Age attribute present and given in seconds
            public long MaxAge;
            public bool Secure;
            public bool HttpOnly;
            public SameSite SameSite;
            public @string Raw;
            public slice<@string> Unparsed; // Raw text of unparsed attribute-value pairs
        }

        // SameSite allows a server to define a cookie attribute making it impossible for
        // the browser to send this cookie along with cross-site requests. The main
        // goal is to mitigate the risk of cross-origin information leakage, and provide
        // some protection against cross-site request forgery attacks.
        //
        // See https://tools.ietf.org/html/draft-ietf-httpbis-cookie-same-site-00 for details.
        public partial struct SameSite // : long
        {
        }

        public static readonly SameSite SameSiteDefaultMode = (SameSite)iota + 1L;
        public static readonly var SameSiteLaxMode = (var)0;
        public static readonly var SameSiteStrictMode = (var)1;
        public static readonly var SameSiteNoneMode = (var)2;


        // readSetCookies parses all "Set-Cookie" values from
        // the header h and returns the successfully parsed Cookies.
        private static slice<ptr<Cookie>> readSetCookies(Header h)
        {
            var cookieCount = len(h["Set-Cookie"]);
            if (cookieCount == 0L)
            {
                return new slice<ptr<Cookie>>(new ptr<Cookie>[] {  });
            }

            var cookies = make_slice<ptr<Cookie>>(0L, cookieCount);
            foreach (var (_, line) in h["Set-Cookie"])
            {
                var parts = strings.Split(textproto.TrimString(line), ";");
                if (len(parts) == 1L && parts[0L] == "")
                {
                    continue;
                }

                parts[0L] = textproto.TrimString(parts[0L]);
                var j = strings.Index(parts[0L], "=");
                if (j < 0L)
                {
                    continue;
                }

                var name = parts[0L][..j];
                var value = parts[0L][j + 1L..];
                if (!isCookieNameValid(name))
                {
                    continue;
                }

                var (value, ok) = parseCookieValue(value, true);
                if (!ok)
                {
                    continue;
                }

                ptr<Cookie> c = addr(new Cookie(Name:name,Value:value,Raw:line,));
                for (long i = 1L; i < len(parts); i++)
                {
                    parts[i] = textproto.TrimString(parts[i]);
                    if (len(parts[i]) == 0L)
                    {
                        continue;
                    }

                    var attr = parts[i];
                    @string val = "";
                    {
                        var j__prev1 = j;

                        j = strings.Index(attr, "=");

                        if (j >= 0L)
                        {
                            attr = attr[..j];
                            val = attr[j + 1L..];

                        }

                        j = j__prev1;

                    }

                    var lowerAttr = strings.ToLower(attr);
                    val, ok = parseCookieValue(val, false);
                    if (!ok)
                    {
                        c.Unparsed = append(c.Unparsed, parts[i]);
                        continue;
                    }

                    switch (lowerAttr)
                    {
                        case "samesite": 
                            var lowerVal = strings.ToLower(val);
                            switch (lowerVal)
                            {
                                case "lax": 
                                    c.SameSite = SameSiteLaxMode;
                                    break;
                                case "strict": 
                                    c.SameSite = SameSiteStrictMode;
                                    break;
                                case "none": 
                                    c.SameSite = SameSiteNoneMode;
                                    break;
                                default: 
                                    c.SameSite = SameSiteDefaultMode;
                                    break;
                            }
                            continue;
                            break;
                        case "secure": 
                            c.Secure = true;
                            continue;
                            break;
                        case "httponly": 
                            c.HttpOnly = true;
                            continue;
                            break;
                        case "domain": 
                            c.Domain = val;
                            continue;
                            break;
                        case "max-age": 
                            var (secs, err) = strconv.Atoi(val);
                            if (err != null || secs != 0L && val[0L] == '0')
                            {
                                break;
                            }

                            if (secs <= 0L)
                            {
                                secs = -1L;
                            }

                            c.MaxAge = secs;
                            continue;
                            break;
                        case "expires": 
                            c.RawExpires = val;
                            var (exptime, err) = time.Parse(time.RFC1123, val);
                            if (err != null)
                            {
                                exptime, err = time.Parse("Mon, 02-Jan-2006 15:04:05 MST", val);
                                if (err != null)
                                {
                                    c.Expires = new time.Time();
                                    break;
                                }

                            }

                            c.Expires = exptime.UTC();
                            continue;
                            break;
                        case "path": 
                            c.Path = val;
                            continue;
                            break;
                    }
                    c.Unparsed = append(c.Unparsed, parts[i]);

                }

                cookies = append(cookies, c);

            }
            return cookies;

        }

        // SetCookie adds a Set-Cookie header to the provided ResponseWriter's headers.
        // The provided cookie must have a valid Name. Invalid cookies may be
        // silently dropped.
        public static void SetCookie(ResponseWriter w, ptr<Cookie> _addr_cookie)
        {
            ref Cookie cookie = ref _addr_cookie.val;

            {
                var v = cookie.String();

                if (v != "")
                {
                    w.Header().Add("Set-Cookie", v);
                }

            }

        }

        // String returns the serialization of the cookie for use in a Cookie
        // header (if only Name and Value are set) or a Set-Cookie response
        // header (if other fields are set).
        // If c is nil or c.Name is invalid, the empty string is returned.
        private static @string String(this ptr<Cookie> _addr_c)
        {
            ref Cookie c = ref _addr_c.val;

            if (c == null || !isCookieNameValid(c.Name))
            {
                return "";
            } 
            // extraCookieLength derived from typical length of cookie attributes
            // see RFC 6265 Sec 4.1.
            const long extraCookieLength = (long)110L;

            strings.Builder b = default;
            b.Grow(len(c.Name) + len(c.Value) + len(c.Domain) + len(c.Path) + extraCookieLength);
            b.WriteString(c.Name);
            b.WriteRune('=');
            b.WriteString(sanitizeCookieValue(c.Value));

            if (len(c.Path) > 0L)
            {
                b.WriteString("; Path=");
                b.WriteString(sanitizeCookiePath(c.Path));
            }

            if (len(c.Domain) > 0L)
            {
                if (validCookieDomain(c.Domain))
                { 
                    // A c.Domain containing illegal characters is not
                    // sanitized but simply dropped which turns the cookie
                    // into a host-only cookie. A leading dot is okay
                    // but won't be sent.
                    var d = c.Domain;
                    if (d[0L] == '.')
                    {
                        d = d[1L..];
                    }

                    b.WriteString("; Domain=");
                    b.WriteString(d);

                }
                else
                {
                    log.Printf("net/http: invalid Cookie.Domain %q; dropping domain attribute", c.Domain);
                }

            }

            array<byte> buf = new array<byte>(len(TimeFormat));
            if (validCookieExpires(c.Expires))
            {
                b.WriteString("; Expires=");
                b.Write(c.Expires.UTC().AppendFormat(buf[..0L], TimeFormat));
            }

            if (c.MaxAge > 0L)
            {
                b.WriteString("; Max-Age=");
                b.Write(strconv.AppendInt(buf[..0L], int64(c.MaxAge), 10L));
            }
            else if (c.MaxAge < 0L)
            {
                b.WriteString("; Max-Age=0");
            }

            if (c.HttpOnly)
            {
                b.WriteString("; HttpOnly");
            }

            if (c.Secure)
            {
                b.WriteString("; Secure");
            }


            if (c.SameSite == SameSiteDefaultMode) 
                b.WriteString("; SameSite");
            else if (c.SameSite == SameSiteNoneMode) 
                b.WriteString("; SameSite=None");
            else if (c.SameSite == SameSiteLaxMode) 
                b.WriteString("; SameSite=Lax");
            else if (c.SameSite == SameSiteStrictMode) 
                b.WriteString("; SameSite=Strict");
                        return b.String();

        }

        // readCookies parses all "Cookie" values from the header h and
        // returns the successfully parsed Cookies.
        //
        // if filter isn't empty, only cookies of that name are returned
        private static slice<ptr<Cookie>> readCookies(Header h, @string filter)
        {
            var lines = h["Cookie"];
            if (len(lines) == 0L)
            {
                return new slice<ptr<Cookie>>(new ptr<Cookie>[] {  });
            }

            var cookies = make_slice<ptr<Cookie>>(0L, len(lines) + strings.Count(lines[0L], ";"));
            foreach (var (_, line) in lines)
            {
                line = textproto.TrimString(line);

                @string part = default;
                while (len(line) > 0L)
                { // continue since we have rest
                    {
                        var splitIndex = strings.Index(line, ";");

                        if (splitIndex > 0L)
                        {
                            part = line[..splitIndex];
                            line = line[splitIndex + 1L..];

                        }
                        else
                        {
                            part = line;
                            line = "";

                        }

                    }

                    part = textproto.TrimString(part);
                    if (len(part) == 0L)
                    {
                        continue;
                    }

                    var name = part;
                    @string val = "";
                    {
                        var j = strings.Index(part, "=");

                        if (j >= 0L)
                        {
                            name = name[..j];
                            val = name[j + 1L..];

                        }

                    }

                    if (!isCookieNameValid(name))
                    {
                        continue;
                    }

                    if (filter != "" && filter != name)
                    {
                        continue;
                    }

                    var (val, ok) = parseCookieValue(val, true);
                    if (!ok)
                    {
                        continue;
                    }

                    cookies = append(cookies, addr(new Cookie(Name:name,Value:val)));

                }


            }
            return cookies;

        }

        // validCookieDomain reports whether v is a valid cookie domain-value.
        private static bool validCookieDomain(@string v)
        {
            if (isCookieDomainName(v))
            {
                return true;
            }

            if (net.ParseIP(v) != null && !strings.Contains(v, ":"))
            {
                return true;
            }

            return false;

        }

        // validCookieExpires reports whether v is a valid cookie expires-value.
        private static bool validCookieExpires(time.Time t)
        { 
            // IETF RFC 6265 Section 5.1.1.5, the year must not be less than 1601
            return t.Year() >= 1601L;

        }

        // isCookieDomainName reports whether s is a valid domain name or a valid
        // domain name with a leading dot '.'.  It is almost a direct copy of
        // package net's isDomainName.
        private static bool isCookieDomainName(@string s)
        {
            if (len(s) == 0L)
            {
                return false;
            }

            if (len(s) > 255L)
            {
                return false;
            }

            if (s[0L] == '.')
            { 
                // A cookie a domain attribute may start with a leading dot.
                s = s[1L..];

            }

            var last = byte('.');
            var ok = false; // Ok once we've seen a letter.
            long partlen = 0L;
            for (long i = 0L; i < len(s); i++)
            {
                var c = s[i];

                if ('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z') 
                    // No '_' allowed here (in contrast to package net).
                    ok = true;
                    partlen++;
                else if ('0' <= c && c <= '9') 
                    // fine
                    partlen++;
                else if (c == '-') 
                    // Byte before dash cannot be dot.
                    if (last == '.')
                    {
                        return false;
                    }

                    partlen++;
                else if (c == '.') 
                    // Byte before dot cannot be dot, dash.
                    if (last == '.' || last == '-')
                    {
                        return false;
                    }

                    if (partlen > 63L || partlen == 0L)
                    {
                        return false;
                    }

                    partlen = 0L;
                else 
                    return false;
                                last = c;

            }

            if (last == '-' || partlen > 63L)
            {
                return false;
            }

            return ok;

        }

        private static var cookieNameSanitizer = strings.NewReplacer("\n", "-", "\r", "-");

        private static @string sanitizeCookieName(@string n)
        {
            return cookieNameSanitizer.Replace(n);
        }

        // sanitizeCookieValue produces a suitable cookie-value from v.
        // https://tools.ietf.org/html/rfc6265#section-4.1.1
        // cookie-value      = *cookie-octet / ( DQUOTE *cookie-octet DQUOTE )
        // cookie-octet      = %x21 / %x23-2B / %x2D-3A / %x3C-5B / %x5D-7E
        //           ; US-ASCII characters excluding CTLs,
        //           ; whitespace DQUOTE, comma, semicolon,
        //           ; and backslash
        // We loosen this as spaces and commas are common in cookie values
        // but we produce a quoted cookie-value if and only if v contains
        // commas or spaces.
        // See https://golang.org/issue/7243 for the discussion.
        private static @string sanitizeCookieValue(@string v)
        {
            v = sanitizeOrWarn("Cookie.Value", validCookieValueByte, v);
            if (len(v) == 0L)
            {
                return v;
            }

            if (strings.IndexByte(v, ' ') >= 0L || strings.IndexByte(v, ',') >= 0L)
            {
                return "\"" + v + "\"";
            }

            return v;

        }

        private static bool validCookieValueByte(byte b)
        {
            return 0x20UL <= b && b < 0x7fUL && b != '"' && b != ';' && b != '\\';
        }

        // path-av           = "Path=" path-value
        // path-value        = <any CHAR except CTLs or ";">
        private static @string sanitizeCookiePath(@string v)
        {
            return sanitizeOrWarn("Cookie.Path", validCookiePathByte, v);
        }

        private static bool validCookiePathByte(byte b)
        {
            return 0x20UL <= b && b < 0x7fUL && b != ';';
        }

        private static @string sanitizeOrWarn(@string fieldName, Func<byte, bool> valid, @string v)
        {
            var ok = true;
            {
                long i__prev1 = i;

                for (long i = 0L; i < len(v); i++)
                {
                    if (valid(v[i]))
                    {
                        continue;
                    }

                    log.Printf("net/http: invalid byte %q in %s; dropping invalid bytes", v[i], fieldName);
                    ok = false;
                    break;

                }


                i = i__prev1;
            }
            if (ok)
            {
                return v;
            }

            var buf = make_slice<byte>(0L, len(v));
            {
                long i__prev1 = i;

                for (i = 0L; i < len(v); i++)
                {
                    {
                        var b = v[i];

                        if (valid(b))
                        {
                            buf = append(buf, b);
                        }

                    }

                }


                i = i__prev1;
            }
            return string(buf);

        }

        private static (@string, bool) parseCookieValue(@string raw, bool allowDoubleQuote)
        {
            @string _p0 = default;
            bool _p0 = default;
 
            // Strip the quotes, if present.
            if (allowDoubleQuote && len(raw) > 1L && raw[0L] == '"' && raw[len(raw) - 1L] == '"')
            {
                raw = raw[1L..len(raw) - 1L];
            }

            for (long i = 0L; i < len(raw); i++)
            {
                if (!validCookieValueByte(raw[i]))
                {
                    return ("", false);
                }

            }

            return (raw, true);

        }

        private static bool isCookieNameValid(@string raw)
        {
            if (raw == "")
            {
                return false;
            }

            return strings.IndexFunc(raw, isNotToken) < 0L;

        }
    }
}}
