// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package httpguts -- go2cs converted at 2022 March 13 06:45:26 UTC
// import "vendor/golang.org/x/net/http/httpguts" ==> using httpguts = go.vendor.golang.org.x.net.http.httpguts_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\net\http\httpguts\httplex.go
namespace go.vendor.golang.org.x.net.http;

using net = net_package;
using strings = strings_package;
using utf8 = unicode.utf8_package;

using idna = golang.org.x.net.idna_package;

public static partial class httpguts_package {

private static array<bool> isTokenTable = new array<bool>(InitKeyedValues<bool>(127, ('!', true), ('#', true), ('$', true), ('%', true), ('&', true), ('\'', true), ('*', true), ('+', true), ('-', true), ('.', true), ('0', true), ('1', true), ('2', true), ('3', true), ('4', true), ('5', true), ('6', true), ('7', true), ('8', true), ('9', true), ('A', true), ('B', true), ('C', true), ('D', true), ('E', true), ('F', true), ('G', true), ('H', true), ('I', true), ('J', true), ('K', true), ('L', true), ('M', true), ('N', true), ('O', true), ('P', true), ('Q', true), ('R', true), ('S', true), ('T', true), ('U', true), ('W', true), ('V', true), ('X', true), ('Y', true), ('Z', true), ('^', true), ('_', true), ('`', true), ('a', true), ('b', true), ('c', true), ('d', true), ('e', true), ('f', true), ('g', true), ('h', true), ('i', true), ('j', true), ('k', true), ('l', true), ('m', true), ('n', true), ('o', true), ('p', true), ('q', true), ('r', true), ('s', true), ('t', true), ('u', true), ('v', true), ('w', true), ('x', true), ('y', true), ('z', true), ('|', true), ('~', true)));

public static bool IsTokenRune(int r) {
    var i = int(r);
    return i < len(isTokenTable) && isTokenTable[i];
}

private static bool isNotToken(int r) {
    return !IsTokenRune(r);
}

// HeaderValuesContainsToken reports whether any string in values
// contains the provided token, ASCII case-insensitively.
public static bool HeaderValuesContainsToken(slice<@string> values, @string token) {
    foreach (var (_, v) in values) {
        if (headerValueContainsToken(v, token)) {
            return true;
        }
    }    return false;
}

// isOWS reports whether b is an optional whitespace byte, as defined
// by RFC 7230 section 3.2.3.
private static bool isOWS(byte b) {
    return b == ' ' || b == '\t';
}

// trimOWS returns x with all optional whitespace removes from the
// beginning and end.
private static @string trimOWS(@string x) { 
    // TODO: consider using strings.Trim(x, " \t") instead,
    // if and when it's fast enough. See issue 10292.
    // But this ASCII-only code will probably always beat UTF-8
    // aware code.
    while (len(x) > 0 && isOWS(x[0])) {
        x = x[(int)1..];
    }
    while (len(x) > 0 && isOWS(x[len(x) - 1])) {
        x = x[..(int)len(x) - 1];
    }
    return x;
}

// headerValueContainsToken reports whether v (assumed to be a
// 0#element, in the ABNF extension described in RFC 7230 section 7)
// contains token amongst its comma-separated tokens, ASCII
// case-insensitively.
private static bool headerValueContainsToken(@string v, @string token) {
    {
        var comma = strings.IndexByte(v, ',');

        while (comma != -1) {
            if (tokenEqual(trimOWS(v[..(int)comma]), token)) {
                return true;
            comma = strings.IndexByte(v, ',');
            }
            v = v[(int)comma + 1..];
        }
    }
    return tokenEqual(trimOWS(v), token);
}

// lowerASCII returns the ASCII lowercase version of b.
private static byte lowerASCII(byte b) {
    if ('A' <= b && b <= 'Z') {
        return b + ('a' - 'A');
    }
    return b;
}

// tokenEqual reports whether t1 and t2 are equal, ASCII case-insensitively.
private static bool tokenEqual(@string t1, @string t2) {
    if (len(t1) != len(t2)) {
        return false;
    }
    foreach (var (i, b) in t1) {
        if (b >= utf8.RuneSelf) { 
            // No UTF-8 or non-ASCII allowed in tokens.
            return false;
        }
        if (lowerASCII(byte(b)) != lowerASCII(t2[i])) {
            return false;
        }
    }    return true;
}

// isLWS reports whether b is linear white space, according
// to http://www.w3.org/Protocols/rfc2616/rfc2616-sec2.html#sec2.2
//      LWS            = [CRLF] 1*( SP | HT )
private static bool isLWS(byte b) {
    return b == ' ' || b == '\t';
}

// isCTL reports whether b is a control byte, according
// to http://www.w3.org/Protocols/rfc2616/rfc2616-sec2.html#sec2.2
//      CTL            = <any US-ASCII control character
//                       (octets 0 - 31) and DEL (127)>
private static bool isCTL(byte b) {
    const nuint del = 0x7f; // a CTL
 // a CTL
    return b < ' ' || b == del;
}

// ValidHeaderFieldName reports whether v is a valid HTTP/1.x header name.
// HTTP/2 imposes the additional restriction that uppercase ASCII
// letters are not allowed.
//
//  RFC 7230 says:
//   header-field   = field-name ":" OWS field-value OWS
//   field-name     = token
//   token          = 1*tchar
//   tchar = "!" / "#" / "$" / "%" / "&" / "'" / "*" / "+" / "-" / "." /
//           "^" / "_" / "`" / "|" / "~" / DIGIT / ALPHA
public static bool ValidHeaderFieldName(@string v) {
    if (len(v) == 0) {
        return false;
    }
    foreach (var (_, r) in v) {
        if (!IsTokenRune(r)) {
            return false;
        }
    }    return true;
}

// ValidHostHeader reports whether h is a valid host header.
public static bool ValidHostHeader(@string h) { 
    // The latest spec is actually this:
    //
    // http://tools.ietf.org/html/rfc7230#section-5.4
    //     Host = uri-host [ ":" port ]
    //
    // Where uri-host is:
    //     http://tools.ietf.org/html/rfc3986#section-3.2.2
    //
    // But we're going to be much more lenient for now and just
    // search for any byte that's not a valid byte in any of those
    // expressions.
    for (nint i = 0; i < len(h); i++) {
        if (!validHostByte[h[i]]) {
            return false;
        }
    }
    return true;
}

// See the validHostHeader comment.
private static array<bool> validHostByte = new array<bool>(InitKeyedValues<bool>(256, ('0', true), ('1', true), ('2', true), ('3', true), ('4', true), ('5', true), ('6', true), ('7', true), ('8', true), ('9', true), ('a', true), ('b', true), ('c', true), ('d', true), ('e', true), ('f', true), ('g', true), ('h', true), ('i', true), ('j', true), ('k', true), ('l', true), ('m', true), ('n', true), ('o', true), ('p', true), ('q', true), ('r', true), ('s', true), ('t', true), ('u', true), ('v', true), ('w', true), ('x', true), ('y', true), ('z', true), ('A', true), ('B', true), ('C', true), ('D', true), ('E', true), ('F', true), ('G', true), ('H', true), ('I', true), ('J', true), ('K', true), ('L', true), ('M', true), ('N', true), ('O', true), ('P', true), ('Q', true), ('R', true), ('S', true), ('T', true), ('U', true), ('V', true), ('W', true), ('X', true), ('Y', true), ('Z', true), ('!', true), ('$', true), ('%', true), ('&', true), ('(', true), (')', true), ('*', true), ('+', true), (',', true), ('-', true), ('.', true), (':', true), (';', true), ('=', true), ('[', true), ('\'', true), (']', true), ('_', true), ('~', true)));

// ValidHeaderFieldValue reports whether v is a valid "field-value" according to
// http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2 :
//
//        message-header = field-name ":" [ field-value ]
//        field-value    = *( field-content | LWS )
//        field-content  = <the OCTETs making up the field-value
//                         and consisting of either *TEXT or combinations
//                         of token, separators, and quoted-string>
//
// http://www.w3.org/Protocols/rfc2616/rfc2616-sec2.html#sec2.2 :
//
//        TEXT           = <any OCTET except CTLs,
//                          but including LWS>
//        LWS            = [CRLF] 1*( SP | HT )
//        CTL            = <any US-ASCII control character
//                         (octets 0 - 31) and DEL (127)>
//
// RFC 7230 says:
//  field-value    = *( field-content / obs-fold )
//  obj-fold       =  N/A to http2, and deprecated
//  field-content  = field-vchar [ 1*( SP / HTAB ) field-vchar ]
//  field-vchar    = VCHAR / obs-text
//  obs-text       = %x80-FF
//  VCHAR          = "any visible [USASCII] character"
//
// http2 further says: "Similarly, HTTP/2 allows header field values
// that are not valid. While most of the values that can be encoded
// will not alter header field parsing, carriage return (CR, ASCII
// 0xd), line feed (LF, ASCII 0xa), and the zero character (NUL, ASCII
// 0x0) might be exploited by an attacker if they are translated
// verbatim. Any request or response that contains a character not
// permitted in a header field value MUST be treated as malformed
// (Section 8.1.2.6). Valid characters are defined by the
// field-content ABNF rule in Section 3.2 of [RFC7230]."
//
// This function does not (yet?) properly handle the rejection of
// strings that begin or end with SP or HTAB.
public static bool ValidHeaderFieldValue(@string v) {
    for (nint i = 0; i < len(v); i++) {
        var b = v[i];
        if (isCTL(b) && !isLWS(b)) {
            return false;
        }
    }
    return true;
}

private static bool isASCII(@string s) {
    for (nint i = 0; i < len(s); i++) {
        if (s[i] >= utf8.RuneSelf) {
            return false;
        }
    }
    return true;
}

// PunycodeHostPort returns the IDNA Punycode version
// of the provided "host" or "host:port" string.
public static (@string, error) PunycodeHostPort(@string v) {
    @string _p0 = default;
    error _p0 = default!;

    if (isASCII(v)) {
        return (v, error.As(null!)!);
    }
    var (host, port, err) = net.SplitHostPort(v);
    if (err != null) { 
        // The input 'v' argument was just a "host" argument,
        // without a port. This error should not be returned
        // to the caller.
        host = v;
        port = "";
    }
    host, err = idna.ToASCII(host);
    if (err != null) { 
        // Non-UTF-8? Not representable in Punycode, in any
        // case.
        return ("", error.As(err)!);
    }
    if (port == "") {
        return (host, error.As(null!)!);
    }
    return (net.JoinHostPort(host, port), error.As(null!)!);
}

} // end httpguts_package
