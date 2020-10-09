// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package template -- go2cs converted at 2020 October 09 04:59:15 UTC
// import "html/template" ==> using template = go.html.template_package
// Original source: C:\Go\src\html\template\css.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go {
namespace html
{
    public static partial class template_package
    {
        // endsWithCSSKeyword reports whether b ends with an ident that
        // case-insensitively matches the lower-case kw.
        private static bool endsWithCSSKeyword(slice<byte> b, @string kw)
        {
            var i = len(b) - len(kw);
            if (i < 0L)
            { 
                // Too short.
                return false;

            }
            if (i != 0L)
            {
                var (r, _) = utf8.DecodeLastRune(b[..i]);
                if (isCSSNmchar(r))
                { 
                    // Too long.
                    return false;

                }
            }
            return string(bytes.ToLower(b[i..])) == kw;

        }

        // isCSSNmchar reports whether rune is allowed anywhere in a CSS identifier.
        private static bool isCSSNmchar(int r)
        { 
            // Based on the CSS3 nmchar production but ignores multi-rune escape
            // sequences.
            // https://www.w3.org/TR/css3-syntax/#SUBTOK-nmchar
            return 'a' <= r && r <= 'z' || 'A' <= r && r <= 'Z' || '0' <= r && r <= '9' || r == '-' || r == '_' || 0x80UL <= r && r <= 0xd7ffUL || 0xe000UL <= r && r <= 0xfffdUL || 0x10000UL <= r && r <= 0x10ffffUL;

        }

        // decodeCSS decodes CSS3 escapes given a sequence of stringchars.
        // If there is no change, it returns the input, otherwise it returns a slice
        // backed by a new array.
        // https://www.w3.org/TR/css3-syntax/#SUBTOK-stringchar defines stringchar.
        private static slice<byte> decodeCSS(slice<byte> s)
        {
            var i = bytes.IndexByte(s, '\\');
            if (i == -1L)
            {
                return s;
            } 
            // The UTF-8 sequence for a codepoint is never longer than 1 + the
            // number hex digits need to represent that codepoint, so len(s) is an
            // upper bound on the output length.
            var b = make_slice<byte>(0L, len(s));
            while (len(s) != 0L)
            {
                i = bytes.IndexByte(s, '\\');
                if (i == -1L)
                {
                    i = len(s);
                }

                b = append(b, s[..i]);
                s = s[i..];
                if (len(s) < 2L)
                {
                    break;
                } 
                // https://www.w3.org/TR/css3-syntax/#SUBTOK-escape
                // escape ::= unicode | '\' [#x20-#x7E#x80-#xD7FF#xE000-#xFFFD#x10000-#x10FFFF]
                if (isHex(s[1L]))
                { 
                    // https://www.w3.org/TR/css3-syntax/#SUBTOK-unicode
                    //   unicode ::= '\' [0-9a-fA-F]{1,6} wc?
                    long j = 2L;
                    while (j < len(s) && j < 7L && isHex(s[j]))
                    {
                        j++;
                    }
                else

                    var r = hexDecode(s[1L..j]);
                    if (r > unicode.MaxRune)
                    {
                        r = r / 16L;
                        j = j - 1L;

                    }

                    var n = utf8.EncodeRune(b[len(b)..cap(b)], r); 
                    // The optional space at the end allows a hex
                    // sequence to be followed by a literal hex.
                    // string(decodeCSS([]byte(`\A B`))) == "\nB"
                    b = b[..len(b) + n];
                    s = skipCSSSpace(s[j..]);

                }                { 
                    // `\\` decodes to `\` and `\"` to `"`.
                    var (_, n) = utf8.DecodeRune(s[1L..]);
                    b = append(b, s[1L..1L + n]);
                    s = s[1L + n..];

                }

            }

            return b;

        }

        // isHex reports whether the given character is a hex digit.
        private static bool isHex(byte c)
        {
            return '0' <= c && c <= '9' || 'a' <= c && c <= 'f' || 'A' <= c && c <= 'F';
        }

        // hexDecode decodes a short hex digit sequence: "10" -> 16.
        private static int hexDecode(slice<byte> s) => func((_, panic, __) =>
        {
            char n = '\x00';
            foreach (var (_, c) in s)
            {
                n <<= 4L;

                if ('0' <= c && c <= '9') 
                    n |= rune(c - '0');
                else if ('a' <= c && c <= 'f') 
                    n |= rune(c - 'a') + 10L;
                else if ('A' <= c && c <= 'F') 
                    n |= rune(c - 'A') + 10L;
                else 
                    panic(fmt.Sprintf("Bad hex digit in %q", s));
                
            }
            return n;

        });

        // skipCSSSpace returns a suffix of c, skipping over a single space.
        private static slice<byte> skipCSSSpace(slice<byte> c)
        {
            if (len(c) == 0L)
            {
                return c;
            } 
            // wc ::= #x9 | #xA | #xC | #xD | #x20
            switch (c[0L])
            {
                case '\t': 

                case '\n': 

                case '\f': 

                case ' ': 
                    return c[1L..];
                    break;
                case '\r': 
                    // This differs from CSS3's wc production because it contains a
                    // probable spec error whereby wc contains all the single byte
                    // sequences in nl (newline) but not CRLF.
                    if (len(c) >= 2L && c[1L] == '\n')
                    {
                        return c[2L..];
                    }

                    return c[1L..];
                    break;
            }
            return c;

        }

        // isCSSSpace reports whether b is a CSS space char as defined in wc.
        private static bool isCSSSpace(byte b)
        {
            switch (b)
            {
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
        private static @string cssEscaper(params object[] args)
        {
            args = args.Clone();

            var (s, _) = stringify(args);
            strings.Builder b = default;
            var r = rune(0L);
            long w = 0L;
            long written = 0L;
            {
                long i = 0L;

                while (i < len(s))
                { 
                    // See comment in htmlEscaper.
                    r, w = utf8.DecodeRuneInString(s[i..]);
                    @string repl = default;

                    if (int(r) < len(cssReplacementTable) && cssReplacementTable[r] != "") 
                        repl = cssReplacementTable[r];
                    else 
                        continue;
                                        if (written == 0L)
                    {
                        b.Grow(len(s));
                    i += w;
                    }

                    b.WriteString(s[written..i]);
                    b.WriteString(repl);
                    written = i + w;
                    if (repl != "\\\\" && (written == len(s) || isHex(s[written]) || isCSSSpace(s[written])))
                    {
                        b.WriteByte(' ');
                    }

                }

            }
            if (written == 0L)
            {
                return s;
            }

            b.WriteString(s[written..]);
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
        private static @string cssValueFilter(params object[] args)
        {
            args = args.Clone();

            var (s, t) = stringify(args);
            if (t == contentTypeCSS)
            {
                return s;
            }

            var b = decodeCSS((slice<byte>)s);
            var id = make_slice<byte>(0L, 64L); 

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
            foreach (var (i, c) in b)
            {
                switch (c)
                {
                    case 0L: 

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
                        if (i != 0L && b[i - 1L] == '-')
                        {
                            return filterFailsafe;
                        }

                        break;
                    default: 
                        if (c < utf8.RuneSelf && isCSSNmchar(rune(c)))
                        {
                            id = append(id, c);
                        }

                        break;
                }

            }
            id = bytes.ToLower(id);
            if (bytes.Contains(id, expressionBytes) || bytes.Contains(id, mozBindingBytes))
            {
                return filterFailsafe;
            }

            return string(b);

        }
    }
}}
