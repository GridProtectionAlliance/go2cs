// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package template -- go2cs converted at 2022 March 13 05:40:13 UTC
// import "html/template" ==> using template = go.html.template_package
// Original source: C:\Program Files\Go\src\html\template\url.go
namespace go.html;

using bytes = bytes_package;
using fmt = fmt_package;
using strings = strings_package;


// urlFilter returns its input unless it contains an unsafe scheme in which
// case it defangs the entire URL.
//
// Schemes that cause unintended side effects that are irreversible without user
// interaction are considered unsafe. For example, clicking on a "javascript:"
// link can immediately trigger JavaScript code execution.
//
// This filter conservatively assumes that all schemes other than the following
// are unsafe:
//    * http:   Navigates to a new website, and may open a new window or tab.
//              These side effects can be reversed by navigating back to the
//              previous website, or closing the window or tab. No irreversible
//              changes will take place without further user interaction with
//              the new website.
//    * https:  Same as http.
//    * mailto: Opens an email program and starts a new draft. This side effect
//              is not irreversible until the user explicitly clicks send; it
//              can be undone by closing the email program.
//
// To allow URLs containing other schemes to bypass this filter, developers must
// explicitly indicate that such a URL is expected and safe by encapsulating it
// in a template.URL value.

public static partial class template_package {

private static @string urlFilter(params object[] args) {
    args = args.Clone();

    var (s, t) = stringify(args);
    if (t == contentTypeURL) {
        return s;
    }
    if (!isSafeURL(s)) {
        return "#" + filterFailsafe;
    }
    return s;
}

// isSafeURL is true if s is a relative URL or if URL has a protocol in
// (http, https, mailto).
private static bool isSafeURL(@string s) {
    {
        var i = strings.IndexRune(s, ':');

        if (i >= 0 && !strings.ContainsRune(s[..(int)i], '/')) {
            var protocol = s[..(int)i];
            if (!strings.EqualFold(protocol, "http") && !strings.EqualFold(protocol, "https") && !strings.EqualFold(protocol, "mailto")) {
                return false;
            }
        }
    }
    return true;
}

// urlEscaper produces an output that can be embedded in a URL query.
// The output can be embedded in an HTML attribute without further escaping.
private static @string urlEscaper(params object[] args) {
    args = args.Clone();

    return urlProcessor(false, args);
}

// urlNormalizer normalizes URL content so it can be embedded in a quote-delimited
// string or parenthesis delimited url(...).
// The normalizer does not encode all HTML specials. Specifically, it does not
// encode '&' so correct embedding in an HTML attribute requires escaping of
// '&' to '&amp;'.
private static @string urlNormalizer(params object[] args) {
    args = args.Clone();

    return urlProcessor(true, args);
}

// urlProcessor normalizes (when norm is true) or escapes its input to produce
// a valid hierarchical or opaque URL part.
private static @string urlProcessor(bool norm, params object[] args) {
    args = args.Clone();

    var (s, t) = stringify(args);
    if (t == contentTypeURL) {
        norm = true;
    }
    ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);
    if (processURLOnto(s, norm, _addr_b)) {
        return b.String();
    }
    return s;
}

// processURLOnto appends a normalized URL corresponding to its input to b
// and reports whether the appended content differs from s.
private static bool processURLOnto(@string s, bool norm, ptr<bytes.Buffer> _addr_b) {
    ref bytes.Buffer b = ref _addr_b.val;

    b.Grow(len(s) + 16);
    nint written = 0; 
    // The byte loop below assumes that all URLs use UTF-8 as the
    // content-encoding. This is similar to the URI to IRI encoding scheme
    // defined in section 3.1 of  RFC 3987, and behaves the same as the
    // EcmaScript builtin encodeURIComponent.
    // It should not cause any misencoding of URLs in pages with
    // Content-type: text/html;charset=UTF-8.
    for (nint i = 0;
    var n = len(s); i < n; i++) {
        var c = s[i];
        switch (c) { 
        // Single quote and parens are sub-delims in RFC 3986, but we
        // escape them so the output can be embedded in single
        // quoted attributes and unquoted CSS url(...) constructs.
        // Single quotes are reserved in URLs, but are only used in
        // the obsolete "mark" rule in an appendix in RFC 3986
        // so can be safely encoded.
            case '!': 

            case '#': 

            case '$': 

            case '&': 

            case '*': 

            case '+': 

            case ',': 

            case '/': 

            case ':': 

            case ';': 

            case '=': 

            case '?': 

            case '@': 

            case '[': 

            case ']': 
                if (norm) {
                    continue;
                } 
                // Unreserved according to RFC 3986 sec 2.3
                // "For consistency, percent-encoded octets in the ranges of
                // ALPHA (%41-%5A and %61-%7A), DIGIT (%30-%39), hyphen (%2D),
                // period (%2E), underscore (%5F), or tilde (%7E) should not be
                // created by URI producers
                break;
            case '-': 

            case '.': 

            case '_': 

            case '~': 
                continue;
                break;
            case '%': 
                // When normalizing do not re-encode valid escapes.
                if (norm && i + 2 < len(s) && isHex(s[i + 1]) && isHex(s[i + 2])) {
                    continue;
                }
                break;
            default: 
                // Unreserved according to RFC 3986 sec 2.3
                if ('a' <= c && c <= 'z') {
                    continue;
                }
                if ('A' <= c && c <= 'Z') {
                    continue;
                }
                if ('0' <= c && c <= '9') {
                    continue;
                }
                break;
        }
        b.WriteString(s[(int)written..(int)i]);
        fmt.Fprintf(b, "%%%02x", c);
        written = i + 1;
    }
    b.WriteString(s[(int)written..]);
    return written != 0;
}

// Filters and normalizes srcset values which are comma separated
// URLs followed by metadata.
private static @string srcsetFilterAndEscaper(params object[] args) {
    args = args.Clone();

    var (s, t) = stringify(args);

    if (t == contentTypeSrcset) 
        return s;
    else if (t == contentTypeURL) 
        // Normalizing gets rid of all HTML whitespace
        // which separate the image URL from its metadata.
        ref bytes.Buffer b = ref heap(out ptr<bytes.Buffer> _addr_b);
        if (processURLOnto(s, true, _addr_b)) {
            s = b.String();
        }
        return strings.ReplaceAll(s, ",", "%2c");
        b = default;
    nint written = 0;
    for (nint i = 0; i < len(s); i++) {
        if (s[i] == ',') {
            filterSrcsetElement(s, written, i, _addr_b);
            b.WriteString(",");
            written = i + 1;
        }
    }
    filterSrcsetElement(s, written, len(s), _addr_b);
    return b.String();
}

// Derived from https://play.golang.org/p/Dhmj7FORT5
private static readonly @string htmlSpaceAndASCIIAlnumBytes = "\x00\x36\x00\x00\x01\x00\xff\x03\xfe\xff\xff\x07\xfe\xff\xff\x07";

// isHTMLSpace is true iff c is a whitespace character per
// https://infra.spec.whatwg.org/#ascii-whitespace


// isHTMLSpace is true iff c is a whitespace character per
// https://infra.spec.whatwg.org/#ascii-whitespace
private static bool isHTMLSpace(byte c) {
    return (c <= 0x20) && 0 != (htmlSpaceAndASCIIAlnumBytes[c >> 3] & (1 << (int)(uint(c & 0x7))));
}

private static bool isHTMLSpaceOrASCIIAlnum(byte c) {
    return (c < 0x80) && 0 != (htmlSpaceAndASCIIAlnumBytes[c >> 3] & (1 << (int)(uint(c & 0x7))));
}

private static void filterSrcsetElement(@string s, nint left, nint right, ptr<bytes.Buffer> _addr_b) {
    ref bytes.Buffer b = ref _addr_b.val;

    var start = left;
    while (start < right && isHTMLSpace(s[start])) {
        start++;
    }
    var end = right;
    {
        var i__prev1 = i;

        for (var i = start; i < right; i++) {
            if (isHTMLSpace(s[i])) {
                end = i;
                break;
            }
        }

        i = i__prev1;
    }
    {
        var url = s[(int)start..(int)end];

        if (isSafeURL(url)) { 
            // If image metadata is only spaces or alnums then
            // we don't need to URL normalize it.
            var metadataOk = true;
            {
                var i__prev1 = i;

                for (i = end; i < right; i++) {
                    if (!isHTMLSpaceOrASCIIAlnum(s[i])) {
                        metadataOk = false;
                        break;
                    }
                }


                i = i__prev1;
            }
            if (metadataOk) {
                b.WriteString(s[(int)left..(int)start]);
                processURLOnto(url, true, _addr_b);
                b.WriteString(s[(int)end..(int)right]);
                return ;
            }
        }
    }
    b.WriteString("#");
    b.WriteString(filterFailsafe);
}

} // end template_package
