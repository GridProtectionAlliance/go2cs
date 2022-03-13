// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package template -- go2cs converted at 2022 March 13 05:38:51 UTC
// import "html/template" ==> using template = go.html.template_package
// Original source: C:\Program Files\Go\src\html\template\css.go
namespace go.html;

using bytes = bytes_package;
using fmt = fmt_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;


// endsWithCSSKeyword reports whether b ends with an ident that
// case-insensitively matches the lower-case kw.

public static partial class template_package {

private static bool endsWithCSSKeyword(slice<byte> b, @string kw) {
    var i = len(b) - len(kw);
    if (i < 0) { 
        // Too short.
        return false;
    }
    if (i != 0) {
        var (r, _) = utf8.DecodeLastRune(b[..(int)i]);
        if (isCSSNmchar(r)) { 
            // Too long.
            return false;
        }
    }
    return string(bytes.ToLower(b[(int)i..])) == kw;
}

// isCSSNmchar reports whether rune is allowed anywhere in a CSS identifier.
private static bool isCSSNmchar(int r) { 
    // Based on the CSS3 nmchar production but ignores multi-rune escape
    // sequences.
    // https://www.w3.org/TR/css3-syntax/#SUBTOK-nmchar
    return 'a' <= r && r <= 'z' || 'A' <= r && r <= 'Z' || '0' <= r && r <= '9' || r == '-' || r == '_' || 0x80 <= r && r <= 0xd7ff || 0xe000 <= r && r <= 0xfffd || 0x10000 <= r && r <= 0x10ffff;
}

// decodeCSS decodes CSS3 escapes given a sequence of stringchars.
// If there is no change, it returns the input, otherwise it returns a slice
// backed by a new array.
// https://www.w3.org/TR/css3-syntax/#SUBTOK-stringchar defines stringchar.
private static slice<byte> decodeCSS(slice<byte> s) {
    var i = bytes.IndexByte(s, '\\');
    if (i == -1) {
        return s;
    }
    var b = make_slice<byte>(0, len(s));
    while (len(s) != 0) {
        i = bytes.IndexByte(s, '\\');
        if (i == -1) {
            i = len(s);
        }
        (b, s) = (append(b, s[..(int)i]), s[(int)i..]);        if (len(s) < 2) {
            break;
        }
        if (isHex(s[1])) { 
            // https://www.w3.org/TR/css3-syntax/#SUBTOK-unicode
            //   unicode ::= '\' [0-9a-fA-F]{1,6} wc?
            nint j = 2;
            while (j < len(s) && j < 7 && isHex(s[j])) {
                j++;
            }
        else

            var r = hexDecode(s[(int)1..(int)j]);
            if (r > unicode.MaxRune) {
                (r, j) = (r / 16, j - 1);
            }
            var n = utf8.EncodeRune(b[(int)len(b)..(int)cap(b)], r); 
            // The optional space at the end allows a hex
            // sequence to be followed by a literal hex.
            // string(decodeCSS([]byte(`\A B`))) == "\nB"
            (b, s) = (b[..(int)len(b) + n], skipCSSSpace(s[(int)j..]));
        } { 
            // `\\` decodes to `\` and `\"` to `"`.
            var (_, n) = utf8.DecodeRune(s[(int)1..]);
            (b, s) = (append(b, s[(int)1..(int)1 + n]), s[(int)1 + n..]);
        }
    }
    return b;
}

// isHex reports whether the given character is a hex digit.
private static bool isHex(byte c) {
    return '0' <= c && c <= '9' || 'a' <= c && c <= 'f' || 'A' <= c && c <= 'F';
}

// hexDecode decodes a short hex digit sequence: "10" -> 16.
private static int hexDecode(slice<byte> s) => func((_, panic, _) => {
    char n = '\x00';
    foreach (var (_, c) in s) {
        n<<=4;

        if ('0' <= c && c <= '9') 
            n |= rune(c - '0');
        else if ('a' <= c && c <= 'f') 
            n |= rune(c - 'a') + 10;
        else if ('A' <= c && c <= 'F') 
            n |= rune(c - 'A') + 10;
        else 
            panic(fmt.Sprintf("Bad hex digit in %q", s));
            }    return n;
});

// skipCSSSpace returns a suffix of c, skipping over a single space.
private static slice<byte> skipCSSSpace(slice<byte> c) {
    if (len(c) == 0) {
        return c;
    }
    switch (c[0]) {
        case '\t': 

        case '\n': 

        case '\f': 

        case ' ': 
            return c[(int)1..];
            break;
        case '\r': 
            // This differs from CSS3's wc production because it contains a
            // probable spec error whereby wc contains all the single byte
            // sequences in nl (newline) but not CRLF.
            if (len(c) >= 2 && c[1] == '\n') {
                return c[(int)2..];
            }
            return c[(int)1..];
            break;
    }
    return c;
}

// isCSSSpace reports whether b is a CSS space char as defined in wc.
private static bool isCSSSpace(byte b) {
    switch (b) {
        case '\t': 

        case '\n': 

        case '\f': 

        case '\r': 

        case ' ': 
            return true;
            break;
    }
    return false;
}

// cssEscaper escapes HTML and CSS special characters using \<hex>+ escapes.
private static @string cssEscaper(params object[] args) {
    args = args.Clone();

    var (s, _) = stringify(args);
    strings.Builder b = default;
    var r = rune(0);
    nint w = 0;
    nint written = 0;
    {
        nint i = 0;

        while (i < len(s)) { 
            // See comment in htmlEscaper.
            r, w = utf8.DecodeRuneInString(s[(int)i..]);
            @string repl = default;

            if (int(r) < len(cssReplacementTable) && cssReplacementTable[r] != "") 
                repl = cssReplacementTable[r];
            else 
                continue;
                        if (written == 0) {
                b.Grow(len(s));
            i += w;
            }
            b.WriteString(s[(int)written..(int)i]);
            b.WriteString(repl);
            written = i + w;
            if (repl != "\\\\" && (written == len(s) || isHex(s[written]) || isCSSSpace(s[written]))) {
                b.WriteByte(' ');
            }
        }
    }
    if (written == 0) {
        return s;
    }
    b.WriteString(s[(int)written..]);
    return b.String();
}

private static @string cssReplacementTable = new slice<@string>(InitKeyedValues<@string>((0, `\0`), ('\t', `\9`), ('\n', `\a`), ('\f', `\c`), ('\r', `\d`), ('"', `\22`), ('&', `\26`), ('\'', `\27`), ('(', `\28`), (')', `\29`), ('+', `\2b`), ('/', `\2f`), (':', `\3a`), (';', `\3b`), ('<', `\3c`), ('>', `\3e`), ('\\', `\\`), ('{', `\7b`), ('}', `\7d`)));

private static slice<byte> expressionBytes = (slice<byte>)"expression";
private static slice<byte> mozBindingBytes = (slice<byte>)"mozbinding";

// cssValueFilter allows innocuous CSS values in the output including CSS
// quantities (10px or 25%), ID or class literals (#foo, .bar), keyword values
// (inherit, blue), and colors (#888).
// It filters out unsafe values, such as those that affect token boundaries,
// and anything that might execute scripts.
private static @string cssValueFilter(params object[] args) {
    args = args.Clone();

    var (s, t) = stringify(args);
    if (t == contentTypeCSS) {
        return s;
    }
    var b = decodeCSS((slice<byte>)s);
    var id = make_slice<byte>(0, 64); 

    // CSS3 error handling is specified as honoring string boundaries per
    // https://www.w3.org/TR/css3-syntax/#error-handling :
    //     Malformed declarations. User agents must handle unexpected
    //     tokens encountered while parsing a declaration by reading until
    //     the end of the declaration, while observing the rules for
    //     matching pairs of (), [], {}, "", and '', and correctly handling
    //     escapes. For example, a malformed declaration may be missing a
    //     property, colon (:) or value.
    // So we need to make sure that values do not have mismatched bracket
    // or quote characters to prevent the browser from restarting parsing
    // inside a string that might embed JavaScript source.
    foreach (var (i, c) in b) {
        switch (c) {
            case 0: 

            case '"': 

            case '\'': 

            case '(': 

            case ')': 

            case '/': 

            case ';': 

            case '@': 

            case '[': 

            case '\\': 

            case ']': 

            case '`': 

            case '{': 

            case '}': 
                return filterFailsafe;
                break;
            case '-': 
                // Disallow <!-- or -->.
                // -- should not appear in valid identifiers.
                if (i != 0 && b[i - 1] == '-') {
                    return filterFailsafe;
                }
                break;
            default: 
                if (c < utf8.RuneSelf && isCSSNmchar(rune(c))) {
                    id = append(id, c);
                }
                break;
        }
    }    id = bytes.ToLower(id);
    if (bytes.Contains(id, expressionBytes) || bytes.Contains(id, mozBindingBytes)) {
        return filterFailsafe;
    }
    return string(b);
}

} // end template_package
