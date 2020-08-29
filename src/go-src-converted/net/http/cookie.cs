// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package http -- go2cs converted at 2020 August 29 08:32:16 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Go\src\net\http\cookie.go
using bytes = go.bytes_package;
using log = go.log_package;
using net = go.net_package;
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
        // See http://tools.ietf.org/html/rfc6265 for details.
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
            public @string Raw;
            public slice<@string> Unparsed; // Raw text of unparsed attribute-value pairs
        }

        // readSetCookies parses all "Set-Cookie" values from
        // the header h and returns the successfully parsed Cookies.
        private static slice<ref Cookie> readSetCookies(Header h)
        {
            var cookieCount = len(h["Set-Cookie"]);
            if (cookieCount == 0L)
            {
                return new slice<ref Cookie>(new ref Cookie[] {  });
            }
            var cookies = make_slice<ref Cookie>(0L, cookieCount);
            foreach (var (_, line) in h["Set-Cookie"])
            {
                var parts = strings.Split(strings.TrimSpace(line), ";");
                if (len(parts) == 1L && parts[0L] == "")
                {
                    continue;
                }
                parts[0L] = strings.TrimSpace(parts[0L]);
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
                Cookie c = ref new Cookie(Name:name,Value:value,Raw:line,);
                for (long i = 1L; i < len(parts); i++)
                {
                    parts[i] = strings.TrimSpace(parts[i]);
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
        public static void SetCookie(ResponseWriter w, ref Cookie cookie)
        {
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
        private static @string String(this ref Cookie c)
        {
            if (c == null || !isCookieNameValid(c.Name))
            {
                return "";
            }
            bytes.Buffer b = default;
            b.WriteString(sanitizeCookieName(c.Name));
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
            if (validCookieExpires(c.Expires))
            {
                b.WriteString("; Expires=");
                var b2 = b.Bytes();
                b.Reset();
                b.Write(c.Expires.UTC().AppendFormat(b2, TimeFormat));
            }
            if (c.MaxAge > 0L)
            {
                b.WriteString("; Max-Age=");
                b2 = b.Bytes();
                b.Reset();
                b.Write(strconv.AppendInt(b2, int64(c.MaxAge), 10L));
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
            return b.String();
        }

        // readCookies parses all "Cookie" values from the header h and
        // returns the successfully parsed Cookies.
        //
        // if filter isn't empty, only cookies of that name are returned
        private static slice<ref Cookie> readCookies(Header h, @string filter)
        {
            var (lines, ok) = h["Cookie"];
            if (!ok)
            {
                return new slice<ref Cookie>(new ref Cookie[] {  });
            }
            ref Cookie cookies = new slice<ref Cookie>(new ref Cookie[] {  });
            foreach (var (_, line) in lines)
            {
                var parts = strings.Split(strings.TrimSpace(line), ";");
                if (len(parts) == 1L && parts[0L] == "")
                {
                    continue;
                } 
                // Per-line attributes
                for (long i = 0L; i < len(parts); i++)
                {
                    parts[i] = strings.TrimSpace(parts[i]);
                    if (len(parts[i]) == 0L)
                    {
                        continue;
                    }
                    var name = parts[i];
                    @string val = "";
                    {
                        var j = strings.Index(name, "=");

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
                    cookies = append(cookies, ref new Cookie(Name:name,Value:val));
                }

            }
            return cookies;
        }

        // validCookieDomain returns whether v is a valid cookie domain-value.
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

        // validCookieExpires returns whether v is a valid cookie expires-value.
        private static bool validCookieExpires(time.Time t)
        { 
            // IETF RFC 6265 Section 5.1.1.5, the year must not be less than 1601
            return t.Year() >= 1601L;
        }

        // isCookieDomainName returns whether s is a valid domain name or a valid
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

        // http://tools.ietf.org/html/rfc6265#section-4.1.1
        // cookie-value      = *cookie-octet / ( DQUOTE *cookie-octet DQUOTE )
        // cookie-octet      = %x21 / %x23-2B / %x2D-3A / %x3C-5B / %x5D-7E
        //           ; US-ASCII characters excluding CTLs,
        //           ; whitespace DQUOTE, comma, semicolon,
        //           ; and backslash
        // We loosen this as spaces and commas are common in cookie values
        // but we produce a quoted cookie-value in when value starts or ends
        // with a comma or space.
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
