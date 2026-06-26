// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using fmt = fmt_package;
using strings = strings_package;
using ꓸꓸꓸany = Span<any>;

partial class template_package {

// urlFilter returns its input unless it contains an unsafe scheme in which
// case it defangs the entire URL.
//
// Schemes that cause unintended side effects that are irreversible without user
// interaction are considered unsafe. For example, clicking on a "javascript:"
// link can immediately trigger JavaScript code execution.
//
// This filter conservatively assumes that all schemes other than the following
// are unsafe:
//   - http:   Navigates to a new website, and may open a new window or tab.
//     These side effects can be reversed by navigating back to the
//     previous website, or closing the window or tab. No irreversible
//     changes will take place without further user interaction with
//     the new website.
//   - https:  Same as http.
//   - mailto: Opens an email program and starts a new draft. This side effect
//     is not irreversible until the user explicitly clicks send; it
//     can be undone by closing the email program.
//
// To allow URLs containing other schemes to bypass this filter, developers must
// explicitly indicate that such a URL is expected and safe by encapsulating it
// in a template.URL value.
internal static @string urlFilter(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    var (s, t) = stringify(args.ꓸꓸꓸ);
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
internal static bool isSafeURL(@string s) {
    {
        var (protocol, _, ok) = strings.Cut(s, ":"u8); if (ok && !strings.Contains(protocol, "/"u8)) {
            if (!strings.EqualFold(protocol, "http"u8) && !strings.EqualFold(protocol, "https"u8) && !strings.EqualFold(protocol, "mailto"u8)) {
                return false;
            }
        }
    }
    return true;
}

// urlEscaper produces an output that can be embedded in a URL query.
// The output can be embedded in an HTML attribute without further escaping.
internal static @string urlEscaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    return urlProcessor(false, args.ꓸꓸꓸ);
}

// urlNormalizer normalizes URL content so it can be embedded in a quote-delimited
// string or parenthesis delimited url(...).
// The normalizer does not encode all HTML specials. Specifically, it does not
// encode '&' so correct embedding in an HTML attribute requires escaping of
// '&' to '&amp;'.
internal static @string urlNormalizer(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    return urlProcessor(true, args.ꓸꓸꓸ);
}

// urlProcessor normalizes (when norm is true) or escapes its input to produce
// a valid hierarchical or opaque URL part.
internal static @string urlProcessor(bool norm, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    var (s, t) = stringify(args.ꓸꓸꓸ);
    if (t == contentTypeURL) {
        norm = true;
    }
    ref var b = ref heap(new strings_package.Builder(), out var Ꮡb);
    if (processURLOnto(s, norm, Ꮡb)) {
        return b.String();
    }
    return s;
}

// processURLOnto appends a normalized URL corresponding to its input to b
// and reports whether the appended content differs from s.
internal static bool processURLOnto(@string s, bool norm, ж<strings.Builder> Ꮡb) {
    ref var b = ref Ꮡb.val;

    b.Grow(len(s) + 16);
    nint written = 0;
    // The byte loop below assumes that all URLs use UTF-8 as the
    // content-encoding. This is similar to the URI to IRI encoding scheme
    // defined in section 3.1 of  RFC 3987, and behaves the same as the
    // EcmaScript builtin encodeURIComponent.
    // It should not cause any misencoding of URLs in pages with
    // Content-type: text/html;charset=UTF-8.
    for (nint i = 0;nint n = len(s); i < n; i++) {
        var c = s[i];
        switch (c) {
        case (rune)'!' or (rune)'#' or (rune)'$' or (rune)'&' or (rune)'*' or (rune)'+' or (rune)',' or (rune)'/' or (rune)':' or (rune)';' or (rune)'=' or (rune)'?' or (rune)'@' or (rune)'[' or (rune)']': {
            if (norm) {
                // Single quote and parens are sub-delims in RFC 3986, but we
                // escape them so the output can be embedded in single
                // quoted attributes and unquoted CSS url(...) constructs.
                // Single quotes are reserved in URLs, but are only used in
                // the obsolete "mark" rule in an appendix in RFC 3986
                // so can be safely encoded.
                continue;
            }
            break;
        }
        case (rune)'-' or (rune)'.' or (rune)'_' or (rune)'~': {
            continue;
            break;
        }
        case (rune)'%': {
            if (norm && i + 2 < len(s) && isHex(s[i + 1]) && isHex(s[i + 2])) {
                // Unreserved according to RFC 3986 sec 2.3
                // "For consistency, percent-encoded octets in the ranges of
                // ALPHA (%41-%5A and %61-%7A), DIGIT (%30-%39), hyphen (%2D),
                // period (%2E), underscore (%5F), or tilde (%7E) should not be
                // created by URI producers
                // When normalizing do not re-encode valid escapes.
                continue;
            }
            break;
        }
        default: {
            if ((rune)'a' <= c && c <= (rune)'z') {
                // Unreserved according to RFC 3986 sec 2.3
                continue;
            }
            if ((rune)'A' <= c && c <= (rune)'Z') {
                continue;
            }
            if ((rune)'0' <= c && c <= (rune)'9') {
                continue;
            }
            break;
        }}

        b.WriteString(s[(int)(written)..(int)(i)]);
        fmt.Fprintf(~b, "%%%02x"u8, c);
        written = i + 1;
    }
    b.WriteString(s[(int)(written)..]);
    return written != 0;
}

// Filters and normalizes srcset values which are comma separated
// URLs followed by metadata.
internal static @string srcsetFilterAndEscaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    var (s, t) = stringify(args.ꓸꓸꓸ);
    var exprᴛ1 = t;
    if (exprᴛ1 == contentTypeSrcset) {
        return s;
    }
    if (exprᴛ1 == contentTypeURL) {
        // Normalizing gets rid of all HTML whitespace
        // which separate the image URL from its metadata.
        ref var bΔ2 = ref heap(new strings_package.Builder(), out var ᏑbΔ2);
        if (processURLOnto(s, true, ᏑbΔ2)) {
            s = bΔ2.String();
        }
        return strings.ReplaceAll(s, // Additionally, commas separate one source from another.
 ","u8, "%2c"u8);
    }

    ref var b = ref heap(new strings_package.Builder(), out var Ꮡb);
    nint written = 0;
    for (nint i = 0; i < len(s); i++) {
        if (s[i] == (rune)',') {
            filterSrcsetElement(s, written, i, Ꮡb);
            b.WriteString(","u8);
            written = i + 1;
        }
    }
    filterSrcsetElement(s, written, len(s), Ꮡb);
    return b.String();
}

// Derived from https://play.golang.org/p/Dhmj7FORT5
internal static readonly @string htmlSpaceAndASCIIAlnumBytes = "\x00\x36\x00\x00\x01\x00\xff\x03\xfe\xff\xff\x07\xfe\xff\xff\x07"u8;

// isHTMLSpace is true iff c is a whitespace character per
// https://infra.spec.whatwg.org/#ascii-whitespace
internal static bool isHTMLSpace(byte c) {
    return (c <= 32) && 0 != ((byte)(htmlSpaceAndASCIIAlnumBytes[c >> (int)(3)] & (1 << (int)(((nuint)((byte)(c & 7)))))));
}

internal static bool isHTMLSpaceOrASCIIAlnum(byte c) {
    return (c < 128) && 0 != ((byte)(htmlSpaceAndASCIIAlnumBytes[c >> (int)(3)] & (1 << (int)(((nuint)((byte)(c & 7)))))));
}

internal static void filterSrcsetElement(@string s, nint left, nint right, ж<strings.Builder> Ꮡb) {
    ref var b = ref Ꮡb.val;

    nint start = left;
    while (start < right && isHTMLSpace(s[start])) {
        start++;
    }
    nint end = right;
    for (nint i = start; i < right; i++) {
        if (isHTMLSpace(s[i])) {
            end = i;
            break;
        }
    }
    {
        @string url = s[(int)(start)..(int)(end)]; if (isSafeURL(url)) {
            // If image metadata is only spaces or alnums then
            // we don't need to URL normalize it.
            var metadataOk = true;
            for (nint i = end; i < right; i++) {
                if (!isHTMLSpaceOrASCIIAlnum(s[i])) {
                    metadataOk = false;
                    break;
                }
            }
            if (metadataOk) {
                b.WriteString(s[(int)(left)..(int)(start)]);
                processURLOnto(url, true, Ꮡb);
                b.WriteString(s[(int)(end)..(int)(right)]);
                return;
            }
        }
    }
    b.WriteString("#"u8);
    b.WriteString(filterFailsafe);
}

} // end template_package
