// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package mime -- go2cs converted at 2022 March 13 05:36:24 UTC
// import "mime" ==> using mime = go.mime_package
// Original source: C:\Program Files\Go\src\mime\mediatype.go
namespace go;

using errors = errors_package;
using fmt = fmt_package;
using sort = sort_package;
using strings = strings_package;
using unicode = unicode_package;


// FormatMediaType serializes mediatype t and the parameters
// param as a media type conforming to RFC 2045 and RFC 2616.
// The type and parameter names are written in lower-case.
// When any of the arguments result in a standard violation then
// FormatMediaType returns the empty string.

public static partial class mime_package {

public static @string FormatMediaType(@string t, map<@string, @string> param) {
    strings.Builder b = default;
    {
        var slash = strings.IndexByte(t, '/');

        if (slash == -1) {
            if (!isToken(t)) {
                return "";
            }
            b.WriteString(strings.ToLower(t));
        }
        else
 {
            var major = t[..(int)slash];
            var sub = t[(int)slash + 1..];
            if (!isToken(major) || !isToken(sub)) {
                return "";
            }
            b.WriteString(strings.ToLower(major));
            b.WriteByte('/');
            b.WriteString(strings.ToLower(sub));
        }
    }

    var attrs = make_slice<@string>(0, len(param));
    foreach (var (a) in param) {
        attrs = append(attrs, a);
    }    sort.Strings(attrs);

    foreach (var (_, attribute) in attrs) {
        var value = param[attribute];
        b.WriteByte(';');
        b.WriteByte(' ');
        if (!isToken(attribute)) {
            return "";
        }
        b.WriteString(strings.ToLower(attribute));

        var needEnc = needsEncoding(value);
        if (needEnc) { 
            // RFC 2231 section 4
            b.WriteByte('*');
        }
        b.WriteByte('=');

        if (needEnc) {
            b.WriteString("utf-8''");

            nint offset = 0;
            {
                nint index__prev2 = index;

                for (nint index = 0; index < len(value); index++) {
                    var ch = value[index]; 
                    // {RFC 2231 section 7}
                    // attribute-char := <any (US-ASCII) CHAR except SPACE, CTLs, "*", "'", "%", or tspecials>
                    if (ch <= ' ' || ch >= 0x7F || ch == '*' || ch == '\'' || ch == '%' || isTSpecial(rune(ch))) {
                        b.WriteString(value[(int)offset..(int)index]);
                        offset = index + 1;

                        b.WriteByte('%');
                        b.WriteByte(upperhex[ch >> 4]);
                        b.WriteByte(upperhex[ch & 0x0F]);
                    }
                }

                index = index__prev2;
            }
            b.WriteString(value[(int)offset..]);
            continue;
        }
        if (isToken(value)) {
            b.WriteString(value);
            continue;
        }
        b.WriteByte('"');
        offset = 0;
        {
            nint index__prev2 = index;

            for (index = 0; index < len(value); index++) {
                var character = value[index];
                if (character == '"' || character == '\\') {
                    b.WriteString(value[(int)offset..(int)index]);
                    offset = index;
                    b.WriteByte('\\');
                }
            }

            index = index__prev2;
        }
        b.WriteString(value[(int)offset..]);
        b.WriteByte('"');
    }    return b.String();
}

private static error checkMediaTypeDisposition(@string s) {
    var (typ, rest) = consumeToken(s);
    if (typ == "") {
        return error.As(errors.New("mime: no media type"))!;
    }
    if (rest == "") {
        return error.As(null!)!;
    }
    if (!strings.HasPrefix(rest, "/")) {
        return error.As(errors.New("mime: expected slash after first token"))!;
    }
    var (subtype, rest) = consumeToken(rest[(int)1..]);
    if (subtype == "") {
        return error.As(errors.New("mime: expected token after slash"))!;
    }
    if (rest != "") {
        return error.As(errors.New("mime: unexpected content after media subtype"))!;
    }
    return error.As(null!)!;
}

// ErrInvalidMediaParameter is returned by ParseMediaType if
// the media type value was found but there was an error parsing
// the optional parameters
public static var ErrInvalidMediaParameter = errors.New("mime: invalid media parameter");

// ParseMediaType parses a media type value and any optional
// parameters, per RFC 1521.  Media types are the values in
// Content-Type and Content-Disposition headers (RFC 2183).
// On success, ParseMediaType returns the media type converted
// to lowercase and trimmed of white space and a non-nil map.
// If there is an error parsing the optional parameter,
// the media type will be returned along with the error
// ErrInvalidMediaParameter.
// The returned map, params, maps from the lowercase
// attribute to the attribute value with its case preserved.
public static (@string, map<@string, @string>, error) ParseMediaType(@string v) {
    @string mediatype = default;
    map<@string, @string> @params = default;
    error err = default!;

    var i = strings.Index(v, ";");
    if (i == -1) {
        i = len(v);
    }
    mediatype = strings.TrimSpace(strings.ToLower(v[(int)0..(int)i]));

    err = checkMediaTypeDisposition(mediatype);
    if (err != null) {
        return ("", null, error.As(err)!);
    }
    params = make_map<@string, @string>(); 

    // Map of base parameter name -> parameter name -> value
    // for parameters containing a '*' character.
    // Lazily initialized.
    map<@string, map<@string, @string>> continuation = default;

    v = v[(int)i..];
    while (len(v) > 0) {
        v = strings.TrimLeftFunc(v, unicode.IsSpace);
        if (len(v) == 0) {
            break;
        }
        var (key, value, rest) = consumeMediaParam(v);
        if (key == "") {
            if (strings.TrimSpace(rest) == ";") { 
                // Ignore trailing semicolons.
                // Not an error.
                return ;
            } 
            // Parse error.
            return (mediatype, null, error.As(ErrInvalidMediaParameter)!);
        }
        var pmap = params;
        {
            var idx = strings.Index(key, "*");

            if (idx != -1) {
                var baseName = key[..(int)idx];
                if (continuation == null) {
                    continuation = make_map<@string, map<@string, @string>>();
                }
                bool ok = default;
                pmap, ok = continuation[baseName];

                if (!ok) {
                    continuation[baseName] = make_map<@string, @string>();
                    pmap = continuation[baseName];
                }
            }

        }
        {
            var (_, exists) = pmap[key];

            if (exists) { 
                // Duplicate parameter name is bogus.
                return ("", null, error.As(errors.New("mime: duplicate parameter name"))!);
            }

        }
        pmap[key] = value;
        v = rest;
    } 

    // Stitch together any continuations or things with stars
    // (i.e. RFC 2231 things with stars: "foo*0" or "foo*")
    strings.Builder buf = default;
    {
        var key__prev1 = key;

        foreach (var (__key, __pieceMap) in continuation) {
            key = __key;
            pieceMap = __pieceMap;
            var singlePartKey = key + "*";
            {
                var v__prev1 = v;

                var (v, ok) = pieceMap[singlePartKey];

                if (ok) {
                    {
                        var decv__prev2 = decv;

                        var (decv, ok) = decode2231Enc(v);

                        if (ok) {
                            params[key] = decv;
                        }

                        decv = decv__prev2;

                    }
                    continue;
                }

                v = v__prev1;

            }

            buf.Reset();
            var valid = false;
            for (nint n = 0; ; n++) {
                var simplePart = fmt.Sprintf("%s*%d", key, n);
                {
                    var v__prev1 = v;

                    (v, ok) = pieceMap[simplePart];

                    if (ok) {
                        valid = true;
                        buf.WriteString(v);
                        continue;
                    }

                    v = v__prev1;

                }
                var encodedPart = simplePart + "*";
                (v, ok) = pieceMap[encodedPart];
                if (!ok) {
                    break;
                }
                valid = true;
                if (n == 0) {
                    {
                        var decv__prev2 = decv;

                        (decv, ok) = decode2231Enc(v);

                        if (ok) {
                            buf.WriteString(decv);
                        }

                        decv = decv__prev2;

                    }
                }
                else
 {
                    var (decv, _) = percentHexUnescape(v);
                    buf.WriteString(decv);
                }
            }

            if (valid) {
                params[key] = buf.String();
            }
        }
        key = key__prev1;
    }

    return ;
}

private static (@string, bool) decode2231Enc(@string v) {
    @string _p0 = default;
    bool _p0 = default;

    var sv = strings.SplitN(v, "'", 3);
    if (len(sv) != 3) {
        return ("", false);
    }
    var charset = strings.ToLower(sv[0]);
    if (len(charset) == 0) {
        return ("", false);
    }
    if (charset != "us-ascii" && charset != "utf-8") { 
        // TODO: unsupported encoding
        return ("", false);
    }
    var (encv, err) = percentHexUnescape(sv[2]);
    if (err != null) {
        return ("", false);
    }
    return (encv, true);
}

private static bool isNotTokenChar(int r) {
    return !isTokenChar(r);
}

// consumeToken consumes a token from the beginning of provided
// string, per RFC 2045 section 5.1 (referenced from 2183), and return
// the token consumed and the rest of the string. Returns ("", v) on
// failure to consume at least one character.
private static (@string, @string) consumeToken(@string v) {
    @string token = default;
    @string rest = default;

    var notPos = strings.IndexFunc(v, isNotTokenChar);
    if (notPos == -1) {
        return (v, "");
    }
    if (notPos == 0) {
        return ("", v);
    }
    return (v[(int)0..(int)notPos], v[(int)notPos..]);
}

// consumeValue consumes a "value" per RFC 2045, where a value is
// either a 'token' or a 'quoted-string'.  On success, consumeValue
// returns the value consumed (and de-quoted/escaped, if a
// quoted-string) and the rest of the string. On failure, returns
// ("", v).
private static (@string, @string) consumeValue(@string v) {
    @string value = default;
    @string rest = default;

    if (v == "") {
        return ;
    }
    if (v[0] != '"') {
        return consumeToken(v);
    }
    ptr<strings.Builder> buffer = @new<strings.Builder>();
    for (nint i = 1; i < len(v); i++) {
        var r = v[i];
        if (r == '"') {
            return (buffer.String(), v[(int)i + 1..]);
        }
        if (r == '\\' && i + 1 < len(v) && isTSpecial(rune(v[i + 1]))) {
            buffer.WriteByte(v[i + 1]);
            i++;
            continue;
        }
        if (r == '\r' || r == '\n') {
            return ("", v);
        }
        buffer.WriteByte(v[i]);
    } 
    // Did not find end quote.
    return ("", v);
}

private static (@string, @string, @string) consumeMediaParam(@string v) {
    @string param = default;
    @string value = default;
    @string rest = default;

    rest = strings.TrimLeftFunc(v, unicode.IsSpace);
    if (!strings.HasPrefix(rest, ";")) {
        return ("", "", v);
    }
    rest = rest[(int)1..]; // consume semicolon
    rest = strings.TrimLeftFunc(rest, unicode.IsSpace);
    param, rest = consumeToken(rest);
    param = strings.ToLower(param);
    if (param == "") {
        return ("", "", v);
    }
    rest = strings.TrimLeftFunc(rest, unicode.IsSpace);
    if (!strings.HasPrefix(rest, "=")) {
        return ("", "", v);
    }
    rest = rest[(int)1..]; // consume equals sign
    rest = strings.TrimLeftFunc(rest, unicode.IsSpace);
    var (value, rest2) = consumeValue(rest);
    if (value == "" && rest2 == rest) {
        return ("", "", v);
    }
    rest = rest2;
    return (param, value, rest);
}

private static (@string, error) percentHexUnescape(@string s) {
    @string _p0 = default;
    error _p0 = default!;
 
    // Count %, check that they're well-formed.
    nint percents = 0;
    {
        nint i__prev1 = i;

        nint i = 0;

        while (i < len(s)) {
            if (s[i] != '%') {
                i++;
                continue;
            }
            percents++;
            if (i + 2 >= len(s) || !ishex(s[i + 1]) || !ishex(s[i + 2])) {
                s = s[(int)i..];
                if (len(s) > 3) {
                    s = s[(int)0..(int)3];
                }
                return ("", error.As(fmt.Errorf("mime: bogus characters after %%: %q", s))!);
            }
            i += 3;
        }

        i = i__prev1;
    }
    if (percents == 0) {
        return (s, error.As(null!)!);
    }
    var t = make_slice<byte>(len(s) - 2 * percents);
    nint j = 0;
    {
        nint i__prev1 = i;

        i = 0;

        while (i < len(s)) {
            switch (s[i]) {
                case '%': 
                    t[j] = unhex(s[i + 1]) << 4 | unhex(s[i + 2]);
                    j++;
                    i += 3;
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
    return (string(t), error.As(null!)!);
}

private static bool ishex(byte c) {

    if ('0' <= c && c <= '9') 
        return true;
    else if ('a' <= c && c <= 'f') 
        return true;
    else if ('A' <= c && c <= 'F') 
        return true;
        return false;
}

private static byte unhex(byte c) {

    if ('0' <= c && c <= '9') 
        return c - '0';
    else if ('a' <= c && c <= 'f') 
        return c - 'a' + 10;
    else if ('A' <= c && c <= 'F') 
        return c - 'A' + 10;
        return 0;
}

} // end mime_package
