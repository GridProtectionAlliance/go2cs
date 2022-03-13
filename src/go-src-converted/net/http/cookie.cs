// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package http -- go2cs converted at 2022 March 13 05:36:28 UTC
// import "net/http" ==> using http = go.net.http_package
// Original source: C:\Program Files\Go\src\net\http\cookie.go
namespace go.net;

using log = log_package;
using net = net_package;
using ascii = net.http.@internal.ascii_package;
using textproto = net.textproto_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;


// A Cookie represents an HTTP cookie as sent in the Set-Cookie header of an
// HTTP response or the Cookie header of an HTTP request.
//
// See https://tools.ietf.org/html/rfc6265 for details.

using System;
public static partial class http_package {

public partial struct Cookie {
    public @string Name;
    public @string Value;
    public @string Path; // optional
    public @string Domain; // optional
    public time.Time Expires; // optional
    public @string RawExpires; // for reading cookies only

// MaxAge=0 means no 'Max-Age' attribute specified.
// MaxAge<0 means delete cookie now, equivalently 'Max-Age: 0'
// MaxAge>0 means Max-Age attribute present and given in seconds
    public nint MaxAge;
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
public partial struct SameSite { // : nint
}

public static readonly SameSite SameSiteDefaultMode = iota + 1;
public static readonly var SameSiteLaxMode = 0;
public static readonly var SameSiteStrictMode = 1;
public static readonly var SameSiteNoneMode = 2;

// readSetCookies parses all "Set-Cookie" values from
// the header h and returns the successfully parsed Cookies.
private static slice<ptr<Cookie>> readSetCookies(Header h) {
    var cookieCount = len(h["Set-Cookie"]);
    if (cookieCount == 0) {
        return new slice<ptr<Cookie>>(new ptr<Cookie>[] {  });
    }
    var cookies = make_slice<ptr<Cookie>>(0, cookieCount);
    foreach (var (_, line) in h["Set-Cookie"]) {
        var parts = strings.Split(textproto.TrimString(line), ";");
        if (len(parts) == 1 && parts[0] == "") {
            continue;
        }
        parts[0] = textproto.TrimString(parts[0]);
        var j = strings.Index(parts[0], "=");
        if (j < 0) {
            continue;
        }
        var name = parts[0][..(int)j];
        var value = parts[0][(int)j + 1..];
        if (!isCookieNameValid(name)) {
            continue;
        }
        var (value, ok) = parseCookieValue(value, true);
        if (!ok) {
            continue;
        }
        ptr<Cookie> c = addr(new Cookie(Name:name,Value:value,Raw:line,));
        for (nint i = 1; i < len(parts); i++) {
            parts[i] = textproto.TrimString(parts[i]);
            if (len(parts[i]) == 0) {
                continue;
            }
            var attr = parts[i];
            @string val = "";
            {
                var j__prev1 = j;

                j = strings.Index(attr, "=");

                if (j >= 0) {
                    (attr, val) = (attr[..(int)j], attr[(int)j + 1..]);
                }

                j = j__prev1;

            }
            var (lowerAttr, isASCII) = ascii.ToLower(attr);
            if (!isASCII) {
                continue;
            }
            val, ok = parseCookieValue(val, false);
            if (!ok) {
                c.Unparsed = append(c.Unparsed, parts[i]);
                continue;
            }
            switch (lowerAttr) {
                case "samesite": 
                    var (lowerVal, ascii) = ascii.ToLower(val);
                    if (!ascii) {
                        c.SameSite = SameSiteDefaultMode;
                        continue;
                    }
                    switch (lowerVal) {
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
                    if (err != null || secs != 0 && val[0] == '0') {
                        break;
                    }
                    if (secs <= 0) {
                        secs = -1;
                    }
                    c.MaxAge = secs;
                    continue;
                    break;
                case "expires": 
                    c.RawExpires = val;
                    var (exptime, err) = time.Parse(time.RFC1123, val);
                    if (err != null) {
                        exptime, err = time.Parse("Mon, 02-Jan-2006 15:04:05 MST", val);
                        if (err != null) {
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
    }    return cookies;
}

// SetCookie adds a Set-Cookie header to the provided ResponseWriter's headers.
// The provided cookie must have a valid Name. Invalid cookies may be
// silently dropped.
public static void SetCookie(ResponseWriter w, ptr<Cookie> _addr_cookie) {
    ref Cookie cookie = ref _addr_cookie.val;

    {
        var v = cookie.String();

        if (v != "") {
            w.Header().Add("Set-Cookie", v);
        }
    }
}

// String returns the serialization of the cookie for use in a Cookie
// header (if only Name and Value are set) or a Set-Cookie response
// header (if other fields are set).
// If c is nil or c.Name is invalid, the empty string is returned.
private static @string String(this ptr<Cookie> _addr_c) {
    ref Cookie c = ref _addr_c.val;

    if (c == null || !isCookieNameValid(c.Name)) {
        return "";
    }
    const nint extraCookieLength = 110;

    strings.Builder b = default;
    b.Grow(len(c.Name) + len(c.Value) + len(c.Domain) + len(c.Path) + extraCookieLength);
    b.WriteString(c.Name);
    b.WriteRune('=');
    b.WriteString(sanitizeCookieValue(c.Value));

    if (len(c.Path) > 0) {
        b.WriteString("; Path=");
        b.WriteString(sanitizeCookiePath(c.Path));
    }
    if (len(c.Domain) > 0) {
        if (validCookieDomain(c.Domain)) { 
            // A c.Domain containing illegal characters is not
            // sanitized but simply dropped which turns the cookie
            // into a host-only cookie. A leading dot is okay
            // but won't be sent.
            var d = c.Domain;
            if (d[0] == '.') {
                d = d[(int)1..];
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
    if (validCookieExpires(c.Expires)) {
        b.WriteString("; Expires=");
        b.Write(c.Expires.UTC().AppendFormat(buf[..(int)0], TimeFormat));
    }
    if (c.MaxAge > 0) {
        b.WriteString("; Max-Age=");
        b.Write(strconv.AppendInt(buf[..(int)0], int64(c.MaxAge), 10));
    }
    else if (c.MaxAge < 0) {
        b.WriteString("; Max-Age=0");
    }
    if (c.HttpOnly) {
        b.WriteString("; HttpOnly");
    }
    if (c.Secure) {
        b.WriteString("; Secure");
    }

    if (c.SameSite == SameSiteDefaultMode)     else if (c.SameSite == SameSiteNoneMode) 
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
private static slice<ptr<Cookie>> readCookies(Header h, @string filter) {
    var lines = h["Cookie"];
    if (len(lines) == 0) {
        return new slice<ptr<Cookie>>(new ptr<Cookie>[] {  });
    }
    var cookies = make_slice<ptr<Cookie>>(0, len(lines) + strings.Count(lines[0], ";"));
    foreach (var (_, line) in lines) {
        line = textproto.TrimString(line);

        @string part = default;
        while (len(line) > 0) { // continue since we have rest
            {
                var splitIndex = strings.Index(line, ";");

                if (splitIndex > 0) {
                    (part, line) = (line[..(int)splitIndex], line[(int)splitIndex + 1..]);
                }
                else
 {
                    (part, line) = (line, "");
                }

            }
            part = textproto.TrimString(part);
            if (len(part) == 0) {
                continue;
            }
            var name = part;
            @string val = "";
            {
                var j = strings.Index(part, "=");

                if (j >= 0) {
                    (name, val) = (name[..(int)j], name[(int)j + 1..]);
                }

            }
            if (!isCookieNameValid(name)) {
                continue;
            }
            if (filter != "" && filter != name) {
                continue;
            }
            var (val, ok) = parseCookieValue(val, true);
            if (!ok) {
                continue;
            }
            cookies = append(cookies, addr(new Cookie(Name:name,Value:val)));
        }
    }    return cookies;
}

// validCookieDomain reports whether v is a valid cookie domain-value.
private static bool validCookieDomain(@string v) {
    if (isCookieDomainName(v)) {
        return true;
    }
    if (net.ParseIP(v) != null && !strings.Contains(v, ":")) {
        return true;
    }
    return false;
}

// validCookieExpires reports whether v is a valid cookie expires-value.
private static bool validCookieExpires(time.Time t) { 
    // IETF RFC 6265 Section 5.1.1.5, the year must not be less than 1601
    return t.Year() >= 1601;
}

// isCookieDomainName reports whether s is a valid domain name or a valid
// domain name with a leading dot '.'.  It is almost a direct copy of
// package net's isDomainName.
private static bool isCookieDomainName(@string s) {
    if (len(s) == 0) {
        return false;
    }
    if (len(s) > 255) {
        return false;
    }
    if (s[0] == '.') { 
        // A cookie a domain attribute may start with a leading dot.
        s = s[(int)1..];
    }
    var last = byte('.');
    var ok = false; // Ok once we've seen a letter.
    nint partlen = 0;
    for (nint i = 0; i < len(s); i++) {
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
            if (last == '.') {
                return false;
            }
            partlen++;
        else if (c == '.') 
            // Byte before dot cannot be dot, dash.
            if (last == '.' || last == '-') {
                return false;
            }
            if (partlen > 63 || partlen == 0) {
                return false;
            }
            partlen = 0;
        else 
            return false;
                last = c;
    }
    if (last == '-' || partlen > 63) {
        return false;
    }
    return ok;
}

private static var cookieNameSanitizer = strings.NewReplacer("\n", "-", "\r", "-");

private static @string sanitizeCookieName(@string n) {
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
private static @string sanitizeCookieValue(@string v) {
    v = sanitizeOrWarn("Cookie.Value", validCookieValueByte, v);
    if (len(v) == 0) {
        return v;
    }
    if (strings.IndexByte(v, ' ') >= 0 || strings.IndexByte(v, ',') >= 0) {
        return "\"" + v + "\"";
    }
    return v;
}

private static bool validCookieValueByte(byte b) {
    return 0x20 <= b && b < 0x7f && b != '"' && b != ';' && b != '\\';
}

// path-av           = "Path=" path-value
// path-value        = <any CHAR except CTLs or ";">
private static @string sanitizeCookiePath(@string v) {
    return sanitizeOrWarn("Cookie.Path", validCookiePathByte, v);
}

private static bool validCookiePathByte(byte b) {
    return 0x20 <= b && b < 0x7f && b != ';';
}

private static @string sanitizeOrWarn(@string fieldName, Func<byte, bool> valid, @string v) {
    var ok = true;
    {
        nint i__prev1 = i;

        for (nint i = 0; i < len(v); i++) {
            if (valid(v[i])) {
                continue;
            }
            log.Printf("net/http: invalid byte %q in %s; dropping invalid bytes", v[i], fieldName);
            ok = false;
            break;
        }

        i = i__prev1;
    }
    if (ok) {
        return v;
    }
    var buf = make_slice<byte>(0, len(v));
    {
        nint i__prev1 = i;

        for (i = 0; i < len(v); i++) {
            {
                var b = v[i];

                if (valid(b)) {
                    buf = append(buf, b);
                }

            }
        }

        i = i__prev1;
    }
    return string(buf);
}

private static (@string, bool) parseCookieValue(@string raw, bool allowDoubleQuote) {
    @string _p0 = default;
    bool _p0 = default;
 
    // Strip the quotes, if present.
    if (allowDoubleQuote && len(raw) > 1 && raw[0] == '"' && raw[len(raw) - 1] == '"') {
        raw = raw[(int)1..(int)len(raw) - 1];
    }
    for (nint i = 0; i < len(raw); i++) {
        if (!validCookieValueByte(raw[i])) {
            return ("", false);
        }
    }
    return (raw, true);
}

private static bool isCookieNameValid(@string raw) {
    if (raw == "") {
        return false;
    }
    return strings.IndexFunc(raw, isNotToken) < 0;
}

} // end http_package
