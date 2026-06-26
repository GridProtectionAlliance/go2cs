// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using errors = errors_package;
using fmt = fmt_package;
using log = log_package;
using net = net_package;
using ascii = net.http.@internal.ascii_package;
using textproto = net.textproto_package;
using strconv = strconv_package;
using strings = strings_package;
using time = time_package;
using net.http.@internal;

partial class http_package {

// A Cookie represents an HTTP cookie as sent in the Set-Cookie header of an
// HTTP response or the Cookie header of an HTTP request.
//
// See https://tools.ietf.org/html/rfc6265 for details.
[GoType] partial struct ΔCookie {
    public @string Name;
    public @string Value;
    public bool Quoted; // indicates whether the Value was originally quoted
    public @string Path;   // optional
    public @string Domain;   // optional
    public time_package.Time Expires; // optional
    public @string RawExpires;   // for reading cookies only
    // MaxAge=0 means no 'Max-Age' attribute specified.
    // MaxAge<0 means delete cookie now, equivalently 'Max-Age: 0'
    // MaxAge>0 means Max-Age attribute present and given in seconds
    public nint MaxAge;
    public bool Secure;
    public bool HttpOnly;
    public SameSite SameSite;
    public bool Partitioned;
    public @string Raw;
    public slice<@string> Unparsed; // Raw text of unparsed attribute-value pairs
}

[GoType("num:nint")] partial struct SameSite;

public static readonly SameSite SameSiteDefaultMode = /* iota + 1 */ 1;
public static readonly SameSite SameSiteLaxMode = 2;
public static readonly SameSite SameSiteStrictMode = 3;
public static readonly SameSite SameSiteNoneMode = 4;

internal static error errBlankCookie = errors.New("http: blank cookie"u8);
internal static error errEqualNotFoundInCookie = errors.New("http: '=' not found in cookie"u8);
internal static error errInvalidCookieName = errors.New("http: invalid cookie name"u8);
internal static error errInvalidCookieValue = errors.New("http: invalid cookie value"u8);

// ParseCookie parses a Cookie header value and returns all the cookies
// which were set in it. Since the same cookie name can appear multiple times
// the returned Values can contain more than one value for a given key.
public static (slice<ж<ΔCookie>>, error) ParseCookie(@string line) {
    var parts = strings.Split(textproto.TrimString(line), ";"u8);
    if (len(parts) == 1 && parts[0] == "") {
        return (default!, errBlankCookie);
    }
    var cookies = new slice<ж<ΔCookie>>(0, len(parts));
    foreach (var (_, s) in parts) {
        s = textproto.TrimString(s);
        var (name, value, found) = strings.Cut(s, "="u8);
        if (!found) {
            return (default!, errEqualNotFoundInCookie);
        }
        if (!isCookieNameValid(name)) {
            return (default!, errInvalidCookieName);
        }
        (value, quoted, found) = parseCookieValue(value, true);
        if (!found) {
            return (default!, errInvalidCookieValue);
        }
        cookies = append(cookies, Ꮡ(new ΔCookie(Name: name, Value: value, Quoted: quoted)));
    }
    return (cookies, default!);
}

// ParseSetCookie parses a Set-Cookie header value and returns a cookie.
// It returns an error on syntax error.
public static (ж<ΔCookie>, error) ParseSetCookie(@string line) {
    var parts = strings.Split(textproto.TrimString(line), ";"u8);
    if (len(parts) == 1 && parts[0] == "") {
        return (default!, errBlankCookie);
    }
    parts[0] = textproto.TrimString(parts[0]);
    var (name, value, ok) = strings.Cut(parts[0], "="u8);
    if (!ok) {
        return (default!, errEqualNotFoundInCookie);
    }
    name = textproto.TrimString(name);
    if (!isCookieNameValid(name)) {
        return (default!, errInvalidCookieName);
    }
    (value, quoted, ok) = parseCookieValue(value, true);
    if (!ok) {
        return (default!, errInvalidCookieValue);
    }
    var c = Ꮡ(new ΔCookie(
        Name: name,
        Value: value,
        Quoted: quoted,
        Raw: line
    ));
    for (nint i = 1; i < len(parts); i++) {
        parts[i] = textproto.TrimString(parts[i]);
        if (len(parts[i]) == 0) {
            continue;
        }
        var (attr, val, _) = strings.Cut(parts[i], "="u8);
        var (lowerAttr, isASCII) = ascii.ToLower(attr);
        if (!isASCII) {
            continue;
        }
        (val, _, ok) = parseCookieValue(val, false);
        if (!ok) {
            c.val.Unparsed = append((~c).Unparsed, parts[i]);
            continue;
        }
        var exprᴛ1 = lowerAttr;
        if (exprᴛ1 == "samesite"u8) {
            var (lowerVal, ascii) = ascii.ToLower(val);
            if (!ascii) {
                c.val.SameSite = SameSiteDefaultMode;
                continue;
            }
            var exprᴛ2 = lowerVal;
            if (exprᴛ2 == "lax"u8) {
                c.val.SameSite = SameSiteLaxMode;
            }
            else if (exprᴛ2 == "strict"u8) {
                c.val.SameSite = SameSiteStrictMode;
            }
            else if (exprᴛ2 == "none"u8) {
                c.val.SameSite = SameSiteNoneMode;
            }
            else { /* default: */
                c.val.SameSite = SameSiteDefaultMode;
            }

            continue;
        }
        else if (exprᴛ1 == "secure"u8) {
            c.val.Secure = true;
            continue;
        }
        else if (exprᴛ1 == "httponly"u8) {
            c.val.HttpOnly = true;
            continue;
        }
        else if (exprᴛ1 == "domain"u8) {
            c.val.Domain = val;
            continue;
        }
        else if (exprᴛ1 == "max-age"u8) {
            var (secs, err) = strconv.Atoi(val);
            if (err != default! || secs != 0 && val[0] == (rune)'0') {
                break;
            }
            if (secs <= 0) {
                secs = -1;
            }
            c.val.MaxAge = secs;
            continue;
        }
        else if (exprᴛ1 == "expires"u8) {
            c.val.RawExpires = val;
            var (exptime, err) = time.Parse(time.RFC1123, val);
            if (err != default!) {
                (exptime, err) = time.Parse("Mon, 02-Jan-2006 15:04:05 MST"u8, val);
                if (err != default!) {
                    c.val.Expires = new time.Time(nil);
                    break;
                }
            }
            c.val.Expires = exptime.UTC();
            continue;
        }
        else if (exprᴛ1 == "path"u8) {
            c.val.Path = val;
            continue;
        }
        else if (exprᴛ1 == "partitioned"u8) {
            c.val.Partitioned = true;
            continue;
        }

        c.val.Unparsed = append((~c).Unparsed, parts[i]);
    }
    return (c, default!);
}

// readSetCookies parses all "Set-Cookie" values from
// the header h and returns the successfully parsed Cookies.
internal static slice<ж<ΔCookie>> readSetCookies(ΔHeader h) {
    nint cookieCount = len(h["Set-Cookie"u8]);
    if (cookieCount == 0) {
        return new ж<ΔCookie>[]{}.slice();
    }
    var cookies = new slice<ж<ΔCookie>>(0, cookieCount);
    foreach (var (_, line) in h["Set-Cookie"u8]) {
        {
            (cookie, err) = ParseSetCookie(line); if (err == default!) {
                cookies = append(cookies, cookie);
            }
        }
    }
    return cookies;
}

// SetCookie adds a Set-Cookie header to the provided [ResponseWriter]'s headers.
// The provided cookie must have a valid Name. Invalid cookies may be
// silently dropped.
public static void SetCookie(ResponseWriter w, ж<ΔCookie> Ꮡcookie) {
    ref var cookie = ref Ꮡcookie.val;

    {
        @string v = cookie.String(); if (v != ""u8) {
            w.Header().Add("Set-Cookie"u8, v);
        }
    }
}

// String returns the serialization of the cookie for use in a [Cookie]
// header (if only Name and Value are set) or a Set-Cookie response
// header (if other fields are set).
// If c is nil or c.Name is invalid, the empty string is returned.
[GoRecv] public static @string String(this ref ΔCookie c) {
    if (c == nil || !isCookieNameValid(c.Name)) {
        return ""u8;
    }
    // extraCookieLength derived from typical length of cookie attributes
    // see RFC 6265 Sec 4.1.
    static readonly UntypedInt extraCookieLength = 110;
    strings.Builder b = default!;
    b.Grow(len(c.Name) + len(c.Value) + len(c.Domain) + len(c.Path) + extraCookieLength);
    b.WriteString(c.Name);
    b.WriteRune((rune)'=');
    b.WriteString(sanitizeCookieValue(c.Value, c.Quoted));
    if (len(c.Path) > 0) {
        b.WriteString("; Path="u8);
        b.WriteString(sanitizeCookiePath(c.Path));
    }
    if (len(c.Domain) > 0) {
        if (validCookieDomain(c.Domain)){
            // A c.Domain containing illegal characters is not
            // sanitized but simply dropped which turns the cookie
            // into a host-only cookie. A leading dot is okay
            // but won't be sent.
            @string d = c.Domain;
            if (d[0] == (rune)'.') {
                d = d[1..];
            }
            b.WriteString("; Domain="u8);
            b.WriteString(d);
        } else {
            log.Printf("net/http: invalid Cookie.Domain %q; dropping domain attribute"u8, c.Domain);
        }
    }
    array<byte> buf = new(29); /* len(TimeFormat) */
    if (validCookieExpires(c.Expires)) {
        b.WriteString("; Expires="u8);
        b.Write(c.Expires.UTC().AppendFormat(buf[..0], TimeFormat));
    }
    if (c.MaxAge > 0){
        b.WriteString("; Max-Age="u8);
        b.Write(strconv.AppendInt(buf[..0], ((int64)c.MaxAge), 10));
    } else 
    if (c.MaxAge < 0) {
        b.WriteString("; Max-Age=0"u8);
    }
    if (c.HttpOnly) {
        b.WriteString("; HttpOnly"u8);
    }
    if (c.Secure) {
        b.WriteString("; Secure"u8);
    }
    var exprᴛ1 = c.SameSite;
    if (exprᴛ1 == SameSiteDefaultMode) {
    }
    else if (exprᴛ1 == SameSiteNoneMode) {
        b.WriteString("; SameSite=None"u8);
    }
    else if (exprᴛ1 == SameSiteLaxMode) {
        b.WriteString("; SameSite=Lax"u8);
    }
    else if (exprᴛ1 == SameSiteStrictMode) {
        b.WriteString("; SameSite=Strict"u8);
    }

    // Skip, default mode is obtained by not emitting the attribute.
    if (c.Partitioned) {
        b.WriteString("; Partitioned"u8);
    }
    return b.String();
}

// Valid reports whether the cookie is valid.
[GoRecv] public static error Valid(this ref ΔCookie c) {
    if (c == nil) {
        return errors.New("http: nil Cookie"u8);
    }
    if (!isCookieNameValid(c.Name)) {
        return errors.New("http: invalid Cookie.Name"u8);
    }
    if (!c.Expires.IsZero() && !validCookieExpires(c.Expires)) {
        return errors.New("http: invalid Cookie.Expires"u8);
    }
    for (nint i = 0; i < len(c.Value); i++) {
        if (!validCookieValueByte(c.Value[i])) {
            return fmt.Errorf("http: invalid byte %q in Cookie.Value"u8, c.Value[i]);
        }
    }
    if (len(c.Path) > 0) {
        for (nint i = 0; i < len(c.Path); i++) {
            if (!validCookiePathByte(c.Path[i])) {
                return fmt.Errorf("http: invalid byte %q in Cookie.Path"u8, c.Path[i]);
            }
        }
    }
    if (len(c.Domain) > 0) {
        if (!validCookieDomain(c.Domain)) {
            return errors.New("http: invalid Cookie.Domain"u8);
        }
    }
    if (c.Partitioned) {
        if (!c.Secure) {
            return errors.New("http: partitioned cookies must be set with Secure"u8);
        }
    }
    return default!;
}

// readCookies parses all "Cookie" values from the header h and
// returns the successfully parsed Cookies.
//
// if filter isn't empty, only cookies of that name are returned.
internal static slice<ж<ΔCookie>> readCookies(ΔHeader h, @string filter) {
    var lines = h["Cookie"u8];
    if (len(lines) == 0) {
        return new ж<ΔCookie>[]{}.slice();
    }
    var cookies = new slice<ж<ΔCookie>>(0, len(lines) + strings.Count(lines[0], ";"u8));
    foreach (var (_, line) in lines) {
        line = textproto.TrimString(line);
        @string part = default!;
        while (len(line) > 0) {
            // continue since we have rest
            (part, line, _) = strings.Cut(line, ";"u8);
            part = textproto.TrimString(part);
            if (part == ""u8) {
                continue;
            }
            var (name, val, _) = strings.Cut(part, "="u8);
            name = textproto.TrimString(name);
            if (!isCookieNameValid(name)) {
                continue;
            }
            if (filter != ""u8 && filter != name) {
                continue;
            }
            var (val, quoted, ok) = parseCookieValue(val, true);
            if (!ok) {
                continue;
            }
            cookies = append(cookies, Ꮡ(new ΔCookie(Name: name, Value: val, Quoted: quoted)));
        }
    }
    return cookies;
}

// validCookieDomain reports whether v is a valid cookie domain-value.
internal static bool validCookieDomain(@string v) {
    if (isCookieDomainName(v)) {
        return true;
    }
    if (net.ParseIP(v) != default! && !strings.Contains(v, ":"u8)) {
        return true;
    }
    return false;
}

// validCookieExpires reports whether v is a valid cookie expires-value.
internal static bool validCookieExpires(time.Time t) {
    // IETF RFC 6265 Section 5.1.1.5, the year must not be less than 1601
    return t.Year() >= 1601;
}

// isCookieDomainName reports whether s is a valid domain name or a valid
// domain name with a leading dot '.'.  It is almost a direct copy of
// package net's isDomainName.
internal static bool isCookieDomainName(@string s) {
    if (len(s) == 0) {
        return false;
    }
    if (len(s) > 255) {
        return false;
    }
    if (s[0] == (rune)'.') {
        // A cookie a domain attribute may start with a leading dot.
        s = s[1..];
    }
    var last = ((byte)(rune)'.');
    var ok = false;
    // Ok once we've seen a letter.
    nint partlen = 0;
    for (nint i = 0; i < len(s); i++) {
        var c = s[i];
        switch (ᐧ) {
        default: {
            return false;
        }
        case {} when (rune)'a' <= c && c <= (rune)'z' || (rune)'A' <= c && c <= (rune)'Z': {
            ok = true;
            partlen++;
            break;
        }
        case {} when (rune)'0' <= c && c <= (rune)'9': {
            partlen++;
            break;
        }
        case {} when c is (rune)'-': {
            if (last == (rune)'.') {
                // No '_' allowed here (in contrast to package net).
                // fine
                // Byte before dash cannot be dot.
                return false;
            }
            partlen++;
            break;
        }
        case {} when c is (rune)'.': {
            if (last == (rune)'.' || last == (rune)'-') {
                // Byte before dot cannot be dot, dash.
                return false;
            }
            if (partlen > 63 || partlen == 0) {
                return false;
            }
            partlen = 0;
            break;
        }}

        last = c;
    }
    if (last == (rune)'-' || partlen > 63) {
        return false;
    }
    return ok;
}

internal static ж<strings.Replacer> cookieNameSanitizer = strings.NewReplacer("\n"u8, "-", "\r", "-");

internal static @string sanitizeCookieName(@string n) {
    return cookieNameSanitizer.Replace(n);
}

// sanitizeCookieValue produces a suitable cookie-value from v.
// It receives a quoted bool indicating whether the value was originally
// quoted.
// https://tools.ietf.org/html/rfc6265#section-4.1.1
//
//	cookie-value      = *cookie-octet / ( DQUOTE *cookie-octet DQUOTE )
//	cookie-octet      = %x21 / %x23-2B / %x2D-3A / %x3C-5B / %x5D-7E
//	          ; US-ASCII characters excluding CTLs,
//	          ; whitespace DQUOTE, comma, semicolon,
//	          ; and backslash
//
// We loosen this as spaces and commas are common in cookie values
// thus we produce a quoted cookie-value if v contains commas or spaces.
// See https://golang.org/issue/7243 for the discussion.
internal static @string sanitizeCookieValue(@string v, bool quoted) {
    v = sanitizeOrWarn("Cookie.Value"u8, validCookieValueByte, v);
    if (len(v) == 0) {
        return v;
    }
    if (strings.ContainsAny(v, " ,"u8) || quoted) {
        return @""""u8 + v + @""""u8;
    }
    return v;
}

internal static bool validCookieValueByte(byte b) {
    return 32 <= b && b < 127 && b != (rune)'"' && b != (rune)';' && b != (rune)'\\';
}

// path-av           = "Path=" path-value
// path-value        = <any CHAR except CTLs or ";">
internal static @string sanitizeCookiePath(@string v) {
    return sanitizeOrWarn("Cookie.Path"u8, validCookiePathByte, v);
}

internal static bool validCookiePathByte(byte b) {
    return 32 <= b && b < 127 && b != (rune)';';
}

internal static @string sanitizeOrWarn(@string fieldName, Func<byte, bool> valid, @string v) {
    var ok = true;
    for (nint i = 0; i < len(v); i++) {
        if (valid(v[i])) {
            continue;
        }
        log.Printf("net/http: invalid byte %q in %s; dropping invalid bytes"u8, v[i], fieldName);
        ok = false;
        break;
    }
    if (ok) {
        return v;
    }
    var buf = new slice<byte>(0, len(v));
    for (nint i = 0; i < len(v); i++) {
        {
            var b = v[i]; if (valid(b)) {
                buf = append(buf, b);
            }
        }
    }
    return ((@string)buf);
}

// parseCookieValue parses a cookie value according to RFC 6265.
// If allowDoubleQuote is true, parseCookieValue will consider that it
// is parsing the cookie-value;
// otherwise, it will consider that it is parsing a cookie-av value
// (cookie attribute-value).
//
// It returns the parsed cookie value, a boolean indicating whether the
// parsing was successful, and a boolean indicating whether the parsed
// value was enclosed in double quotes.
internal static (@string value, bool quoted, bool ok) parseCookieValue(@string raw, bool allowDoubleQuote) {
    @string value = default!;
    bool quoted = default!;
    bool ok = default!;

    // Strip the quotes, if present.
    if (allowDoubleQuote && len(raw) > 1 && raw[0] == (rune)'"' && raw[len(raw) - 1] == (rune)'"') {
        raw = raw[1..(int)(len(raw) - 1)];
        quoted = true;
    }
    for (nint i = 0; i < len(raw); i++) {
        if (!validCookieValueByte(raw[i])) {
            return ("", quoted, false);
        }
    }
    return (raw, quoted, true);
}

internal static bool isCookieNameValid(@string raw) {
    if (raw == ""u8) {
        return false;
    }
    return strings.IndexFunc(raw, isNotToken) < 0;
}

} // end http_package
