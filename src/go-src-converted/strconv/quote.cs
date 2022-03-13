// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate go run makeisprint.go -output isprint.go

// package strconv -- go2cs converted at 2022 March 13 05:41:26 UTC
// import "strconv" ==> using strconv = go.strconv_package
// Original source: C:\Program Files\Go\src\strconv\quote.go
namespace go;

using utf8 = unicode.utf8_package;

public static partial class strconv_package {

private static readonly @string lowerhex = "0123456789abcdef";
private static readonly @string upperhex = "0123456789ABCDEF";

// contains reports whether the string contains the byte c.
private static bool contains(@string s, byte c) {
    return index(s, c) != -1;
}

private static @string quoteWith(@string s, byte quote, bool ASCIIonly, bool graphicOnly) {
    return string(appendQuotedWith(make_slice<byte>(0, 3 * len(s) / 2), s, quote, ASCIIonly, graphicOnly));
}

private static @string quoteRuneWith(int r, byte quote, bool ASCIIonly, bool graphicOnly) {
    return string(appendQuotedRuneWith(null, r, quote, ASCIIonly, graphicOnly));
}

private static slice<byte> appendQuotedWith(slice<byte> buf, @string s, byte quote, bool ASCIIonly, bool graphicOnly) { 
    // Often called with big strings, so preallocate. If there's quoting,
    // this is conservative but still helps a lot.
    if (cap(buf) - len(buf) < len(s)) {
        var nBuf = make_slice<byte>(len(buf), len(buf) + 1 + len(s) + 1);
        copy(nBuf, buf);
        buf = nBuf;
    }
    buf = append(buf, quote);
    {
        nint width = 0;

        while (len(s) > 0) {
            var r = rune(s[0]);
            width = 1;
            if (r >= utf8.RuneSelf) {
                r, width = utf8.DecodeRuneInString(s);
            s = s[(int)width..];
            }
            if (width == 1 && r == utf8.RuneError) {
                buf = append(buf, "\\x");
                buf = append(buf, lowerhex[s[0] >> 4]);
                buf = append(buf, lowerhex[s[0] & 0xF]);
                continue;
            }
            buf = appendEscapedRune(buf, r, quote, ASCIIonly, graphicOnly);
        }
    }
    buf = append(buf, quote);
    return buf;
}

private static slice<byte> appendQuotedRuneWith(slice<byte> buf, int r, byte quote, bool ASCIIonly, bool graphicOnly) {
    buf = append(buf, quote);
    if (!utf8.ValidRune(r)) {
        r = utf8.RuneError;
    }
    buf = appendEscapedRune(buf, r, quote, ASCIIonly, graphicOnly);
    buf = append(buf, quote);
    return buf;
}

private static slice<byte> appendEscapedRune(slice<byte> buf, int r, byte quote, bool ASCIIonly, bool graphicOnly) {
    array<byte> runeTmp = new array<byte>(utf8.UTFMax);
    if (r == rune(quote) || r == '\\') { // always backslashed
        buf = append(buf, '\\');
        buf = append(buf, byte(r));
        return buf;
    }
    if (ASCIIonly) {
        if (r < utf8.RuneSelf && IsPrint(r)) {
            buf = append(buf, byte(r));
            return buf;
        }
    }
    else if (IsPrint(r) || graphicOnly && isInGraphicList(r)) {
        var n = utf8.EncodeRune(runeTmp[..], r);
        buf = append(buf, runeTmp[..(int)n]);
        return buf;
    }
    switch (r) {
        case '\a': 
            buf = append(buf, "\\a");
            break;
        case '\b': 
            buf = append(buf, "\\b");
            break;
        case '\f': 
            buf = append(buf, "\\f");
            break;
        case '\n': 
            buf = append(buf, "\\n");
            break;
        case '\r': 
            buf = append(buf, "\\r");
            break;
        case '\t': 
            buf = append(buf, "\\t");
            break;
        case '\v': 
            buf = append(buf, "\\v");
            break;
        default: 

            if (r < ' ')
            {
                buf = append(buf, "\\x");
                buf = append(buf, lowerhex[byte(r) >> 4]);
                buf = append(buf, lowerhex[byte(r) & 0xF]);
                goto __switch_break0;
            }
            if (r > utf8.MaxRune)
            {
                r = 0xFFFD;
                fallthrough = true;
            }
            if (fallthrough || r < 0x10000)
            {
                buf = append(buf, "\\u");
                {
                    nint s__prev1 = s;

                    nint s = 12;

                    while (s >= 0) {
                        buf = append(buf, lowerhex[r >> (int)(uint(s)) & 0xF]);
                        s -= 4;
                    }


                    s = s__prev1;
                }
                goto __switch_break0;
            }
            // default: 
                buf = append(buf, "\\U");
                {
                    nint s__prev1 = s;

                    s = 28;

                    while (s >= 0) {
                        buf = append(buf, lowerhex[r >> (int)(uint(s)) & 0xF]);
                        s -= 4;
                    }


                    s = s__prev1;
                }

            __switch_break0:;
            break;
    }
    return buf;
}

// Quote returns a double-quoted Go string literal representing s. The
// returned string uses Go escape sequences (\t, \n, \xFF, \u0100) for
// control characters and non-printable characters as defined by
// IsPrint.
public static @string Quote(@string s) {
    return quoteWith(s, '"', false, false);
}

// AppendQuote appends a double-quoted Go string literal representing s,
// as generated by Quote, to dst and returns the extended buffer.
public static slice<byte> AppendQuote(slice<byte> dst, @string s) {
    return appendQuotedWith(dst, s, '"', false, false);
}

// QuoteToASCII returns a double-quoted Go string literal representing s.
// The returned string uses Go escape sequences (\t, \n, \xFF, \u0100) for
// non-ASCII characters and non-printable characters as defined by IsPrint.
public static @string QuoteToASCII(@string s) {
    return quoteWith(s, '"', true, false);
}

// AppendQuoteToASCII appends a double-quoted Go string literal representing s,
// as generated by QuoteToASCII, to dst and returns the extended buffer.
public static slice<byte> AppendQuoteToASCII(slice<byte> dst, @string s) {
    return appendQuotedWith(dst, s, '"', true, false);
}

// QuoteToGraphic returns a double-quoted Go string literal representing s.
// The returned string leaves Unicode graphic characters, as defined by
// IsGraphic, unchanged and uses Go escape sequences (\t, \n, \xFF, \u0100)
// for non-graphic characters.
public static @string QuoteToGraphic(@string s) {
    return quoteWith(s, '"', false, true);
}

// AppendQuoteToGraphic appends a double-quoted Go string literal representing s,
// as generated by QuoteToGraphic, to dst and returns the extended buffer.
public static slice<byte> AppendQuoteToGraphic(slice<byte> dst, @string s) {
    return appendQuotedWith(dst, s, '"', false, true);
}

// QuoteRune returns a single-quoted Go character literal representing the
// rune. The returned string uses Go escape sequences (\t, \n, \xFF, \u0100)
// for control characters and non-printable characters as defined by IsPrint.
public static @string QuoteRune(int r) {
    return quoteRuneWith(r, '\'', false, false);
}

// AppendQuoteRune appends a single-quoted Go character literal representing the rune,
// as generated by QuoteRune, to dst and returns the extended buffer.
public static slice<byte> AppendQuoteRune(slice<byte> dst, int r) {
    return appendQuotedRuneWith(dst, r, '\'', false, false);
}

// QuoteRuneToASCII returns a single-quoted Go character literal representing
// the rune. The returned string uses Go escape sequences (\t, \n, \xFF,
// \u0100) for non-ASCII characters and non-printable characters as defined
// by IsPrint.
public static @string QuoteRuneToASCII(int r) {
    return quoteRuneWith(r, '\'', true, false);
}

// AppendQuoteRuneToASCII appends a single-quoted Go character literal representing the rune,
// as generated by QuoteRuneToASCII, to dst and returns the extended buffer.
public static slice<byte> AppendQuoteRuneToASCII(slice<byte> dst, int r) {
    return appendQuotedRuneWith(dst, r, '\'', true, false);
}

// QuoteRuneToGraphic returns a single-quoted Go character literal representing
// the rune. If the rune is not a Unicode graphic character,
// as defined by IsGraphic, the returned string will use a Go escape sequence
// (\t, \n, \xFF, \u0100).
public static @string QuoteRuneToGraphic(int r) {
    return quoteRuneWith(r, '\'', false, true);
}

// AppendQuoteRuneToGraphic appends a single-quoted Go character literal representing the rune,
// as generated by QuoteRuneToGraphic, to dst and returns the extended buffer.
public static slice<byte> AppendQuoteRuneToGraphic(slice<byte> dst, int r) {
    return appendQuotedRuneWith(dst, r, '\'', false, true);
}

// CanBackquote reports whether the string s can be represented
// unchanged as a single-line backquoted string without control
// characters other than tab.
public static bool CanBackquote(@string s) {
    while (len(s) > 0) {
        var (r, wid) = utf8.DecodeRuneInString(s);
        s = s[(int)wid..];
        if (wid > 1) {
            if (r == '\ufeff') {
                return false; // BOMs are invisible and should not be quoted.
            }
            continue; // All other multibyte runes are correctly encoded and assumed printable.
        }
        if (r == utf8.RuneError) {
            return false;
        }
        if ((r < ' ' && r != '\t') || r == '`' || r == '\u007F') {
            return false;
        }
    }
    return true;
}

private static (int, bool) unhex(byte b) {
    int v = default;
    bool ok = default;

    var c = rune(b);

    if ('0' <= c && c <= '9') 
        return (c - '0', true);
    else if ('a' <= c && c <= 'f') 
        return (c - 'a' + 10, true);
    else if ('A' <= c && c <= 'F') 
        return (c - 'A' + 10, true);
        return ;
}

// UnquoteChar decodes the first character or byte in the escaped string
// or character literal represented by the string s.
// It returns four values:
//
//    1) value, the decoded Unicode code point or byte value;
//    2) multibyte, a boolean indicating whether the decoded character requires a multibyte UTF-8 representation;
//    3) tail, the remainder of the string after the character; and
//    4) an error that will be nil if the character is syntactically valid.
//
// The second argument, quote, specifies the type of literal being parsed
// and therefore which escaped quote character is permitted.
// If set to a single quote, it permits the sequence \' and disallows unescaped '.
// If set to a double quote, it permits \" and disallows unescaped ".
// If set to zero, it does not permit either escape and allows both quote characters to appear unescaped.
public static (int, bool, @string, error) UnquoteChar(@string s, byte quote) {
    int value = default;
    bool multibyte = default;
    @string tail = default;
    error err = default!;
 
    // easy cases
    if (len(s) == 0) {
        err = ErrSyntax;
        return ;
    }
    {
        var c__prev1 = c;

        var c = s[0];


        if (c == quote && (quote == '\'' || quote == '"')) 
            err = ErrSyntax;
            return ;
        else if (c >= utf8.RuneSelf) 
            var (r, size) = utf8.DecodeRuneInString(s);
            return (r, true, s[(int)size..], error.As(null!)!);
        else if (c != '\\') 
            return (rune(s[0]), false, s[(int)1..], error.As(null!)!);


        c = c__prev1;
    } 

    // hard case: c is backslash
    if (len(s) <= 1) {
        err = ErrSyntax;
        return ;
    }
    c = s[1];
    s = s[(int)2..];

    switch (c) {
        case 'a': 
            value = '\a';
            break;
        case 'b': 
            value = '\b';
            break;
        case 'f': 
            value = '\f';
            break;
        case 'n': 
            value = '\n';
            break;
        case 'r': 
            value = '\r';
            break;
        case 't': 
            value = '\t';
            break;
        case 'v': 
            value = '\v';
            break;
        case 'x': 

        case 'u': 

        case 'U': 
            nint n = 0;
            switch (c) {
                case 'x': 
                    n = 2;
                    break;
                case 'u': 
                    n = 4;
                    break;
                case 'U': 
                    n = 8;
                    break;
            }
            int v = default;
            if (len(s) < n) {
                err = ErrSyntax;
                return ;
            }
            {
                nint j__prev1 = j;

                for (nint j = 0; j < n; j++) {
                    var (x, ok) = unhex(s[j]);
                    if (!ok) {
                        err = ErrSyntax;
                        return ;
                    }
                    v = v << 4 | x;
                }


                j = j__prev1;
            }
            s = s[(int)n..];
            if (c == 'x') { 
                // single-byte string, possibly not UTF-8
                value = v;
                break;
            }
            if (v > utf8.MaxRune) {
                err = ErrSyntax;
                return ;
            }
            value = v;
            multibyte = true;
            break;
        case '0': 

        case '1': 

        case '2': 

        case '3': 

        case '4': 

        case '5': 

        case '6': 

        case '7': 
            v = rune(c) - '0';
            if (len(s) < 2) {
                err = ErrSyntax;
                return ;
            }
            {
                nint j__prev1 = j;

                for (j = 0; j < 2; j++) { // one digit already; two more
                    var x = rune(s[j]) - '0';
                    if (x < 0 || x > 7) {
                        err = ErrSyntax;
                        return ;
                    }
                    v = (v << 3) | x;
                }


                j = j__prev1;
            }
            s = s[(int)2..];
            if (v > 255) {
                err = ErrSyntax;
                return ;
            }
            value = v;
            break;
        case '\\': 
            value = '\\';
            break;
        case '\'': 

        case '"': 
            if (c != quote) {
                err = ErrSyntax;
                return ;
            }
            value = rune(c);
            break;
        default: 
            err = ErrSyntax;
            return ;
            break;
    }
    tail = s;
    return ;
}

// QuotedPrefix returns the quoted string (as understood by Unquote) at the prefix of s.
// If s does not start with a valid quoted string, QuotedPrefix returns an error.
public static (@string, error) QuotedPrefix(@string s) {
    @string _p0 = default;
    error _p0 = default!;

    var (out, _, err) = unquote(s, false);
    return (out, error.As(err)!);
}

// Unquote interprets s as a single-quoted, double-quoted,
// or backquoted Go string literal, returning the string value
// that s quotes.  (If s is single-quoted, it would be a Go
// character literal; Unquote returns the corresponding
// one-character string.)
public static (@string, error) Unquote(@string s) {
    @string _p0 = default;
    error _p0 = default!;

    var (out, rem, err) = unquote(s, true);
    if (len(rem) > 0) {
        return ("", error.As(ErrSyntax)!);
    }
    return (out, error.As(err)!);
}

// unquote parses a quoted string at the start of the input,
// returning the parsed prefix, the remaining suffix, and any parse errors.
// If unescape is true, the parsed prefix is unescaped,
// otherwise the input prefix is provided verbatim.
private static (@string, @string, error) unquote(@string @in, bool unescape) {
    @string @out = default;
    @string rem = default;
    error err = default!;
 
    // Determine the quote form and optimistically find the terminating quote.
    if (len(in) < 2) {
        return ("", in, error.As(ErrSyntax)!);
    }
    var quote = in[0];
    var end = index(in[(int)1..], quote);
    if (end < 0) {
        return ("", in, error.As(ErrSyntax)!);
    }
    end += 2; // position after terminating quote; may be wrong if escape sequences are present

    switch (quote) {
        case '`': 

            if (!unescape) 
                out = in[..(int)end]; // include quotes
            else if (!contains(in[..(int)end], '\r')) 
                out = in[(int)len("`")..(int)end - len("`")]; // exclude quotes
            else 
                // Carriage return characters ('\r') inside raw string literals
                // are discarded from the raw string value.
                var buf = make_slice<byte>(0, end - len("`") - len("\r") - len("`"));
                for (var i = len("`"); i < end - len("`"); i++) {
                    if (in[i] != '\r') {
                        buf = append(buf, in[i]);
                    }
                }

                out = string(buf);
            // NOTE: Prior implementations did not verify that raw strings consist
            // of valid UTF-8 characters and we continue to not verify it as such.
            // The Go specification does not explicitly require valid UTF-8,
            // but only mention that it is implicitly valid for Go source code
            // (which must be valid UTF-8).
            return (out, in[(int)end..], error.As(null!)!);
            break;
        case '"': 
            // Handle quoted strings without any escape sequences.

        case '\'': 
            // Handle quoted strings without any escape sequences.
                   if (!contains(in[..(int)end], '\\') && !contains(in[..(int)end], '\n')) {
                       bool valid = default;
                       switch (quote) {
                           case '"': 
                               valid = utf8.ValidString(in[(int)len("\"")..(int)end - len("\"")]);
                               break;
                           case '\'': 
                               var (r, n) = utf8.DecodeRuneInString(in[(int)len("'")..(int)end - len("'")]);
                               valid = len("'") + n + len("'") == end && (r != utf8.RuneError || n != 1);
                               break;
                       }
                       if (valid) {
                           out = in[..(int)end];
                           if (unescape) {
                               out = out[(int)1..(int)end - 1]; // exclude quotes
                           }
                           return (out, in[(int)end..], error.As(null!)!);
                       }
                   }
                   buf = default;
                   var in0 = in;
                   in = in[(int)1..]; // skip starting quote
                   if (unescape) {
                       buf = make_slice<byte>(0, 3 * end / 2); // try to avoid more allocations
                   }
                   while (len(in) > 0 && in[0] != quote) { 
                       // Process the next character,
                       // rejecting any unescaped newline characters which are invalid.
                       var (r, multibyte, rem, err) = UnquoteChar(in, quote);
                       if (in[0] == '\n' || err != null) {
                           return ("", in0, error.As(ErrSyntax)!);
                       }
                       in = rem; 

                       // Append the character if unescaping the input.
                       if (unescape) {
                           if (r < utf8.RuneSelf || !multibyte) {
                               buf = append(buf, byte(r));
                           }
                           else
            {
                               array<byte> arr = new array<byte>(utf8.UTFMax);
                               var n = utf8.EncodeRune(arr[..], r);
                               buf = append(buf, arr[..(int)n]);
                           }
                       } 

                       // Single quoted strings must be a single character.
                       if (quote == '\'') {
                           break;
                       }
                   } 

                   // Verify that the string ends with a terminating quote.
                   if (!(len(in) > 0 && in[0] == quote)) {
                       return ("", in0, error.As(ErrSyntax)!);
                   }
                   in = in[(int)1..]; // skip terminating quote

                   if (unescape) {
                       return (string(buf), in, error.As(null!)!);
                   }
                   return (in0[..(int)len(in0) - len(in)], in, error.As(null!)!);
            break;
        default: 
            return ("", in, error.As(ErrSyntax)!);
            break;
    }
}

// bsearch16 returns the smallest i such that a[i] >= x.
// If there is no such i, bsearch16 returns len(a).
private static nint bsearch16(slice<ushort> a, ushort x) {
    nint i = 0;
    var j = len(a);
    while (i < j) {
        var h = i + (j - i) >> 1;
        if (a[h] < x) {
            i = h + 1;
        }
        else
 {
            j = h;
        }
    }
    return i;
}

// bsearch32 returns the smallest i such that a[i] >= x.
// If there is no such i, bsearch32 returns len(a).
private static nint bsearch32(slice<uint> a, uint x) {
    nint i = 0;
    var j = len(a);
    while (i < j) {
        var h = i + (j - i) >> 1;
        if (a[h] < x) {
            i = h + 1;
        }
        else
 {
            j = h;
        }
    }
    return i;
}

// TODO: IsPrint is a local implementation of unicode.IsPrint, verified by the tests
// to give the same answer. It allows this package not to depend on unicode,
// and therefore not pull in all the Unicode tables. If the linker were better
// at tossing unused tables, we could get rid of this implementation.
// That would be nice.

// IsPrint reports whether the rune is defined as printable by Go, with
// the same definition as unicode.IsPrint: letters, numbers, punctuation,
// symbols and ASCII space.
public static bool IsPrint(int r) { 
    // Fast check for Latin-1
    if (r <= 0xFF) {
        if (0x20 <= r && r <= 0x7E) { 
            // All the ASCII is printable from space through DEL-1.
            return true;
        }
        if (0xA1 <= r && r <= 0xFF) { 
            // Similarly for ¡ through ÿ...
            return r != 0xAD; // ...except for the bizarre soft hyphen.
        }
        return false;
    }
    if (0 <= r && r < 1 << 16) {
        var rr = uint16(r);
        var isPrint = isPrint16;
        var isNotPrint = isNotPrint16;
        var i = bsearch16(isPrint, rr);
        if (i >= len(isPrint) || rr < isPrint[i & ~1] || isPrint[i | 1] < rr) {
            return false;
        }
        var j = bsearch16(isNotPrint, rr);
        return j >= len(isNotPrint) || isNotPrint[j] != rr;
    }
    rr = uint32(r);
    isPrint = isPrint32;
    isNotPrint = isNotPrint32;
    i = bsearch32(isPrint, rr);
    if (i >= len(isPrint) || rr < isPrint[i & ~1] || isPrint[i | 1] < rr) {
        return false;
    }
    if (r >= 0x20000) {
        return true;
    }
    r -= 0x10000;
    j = bsearch16(isNotPrint, uint16(r));
    return j >= len(isNotPrint) || isNotPrint[j] != uint16(r);
}

// IsGraphic reports whether the rune is defined as a Graphic by Unicode. Such
// characters include letters, marks, numbers, punctuation, symbols, and
// spaces, from categories L, M, N, P, S, and Zs.
public static bool IsGraphic(int r) {
    if (IsPrint(r)) {
        return true;
    }
    return isInGraphicList(r);
}

// isInGraphicList reports whether the rune is in the isGraphic list. This separation
// from IsGraphic allows quoteWith to avoid two calls to IsPrint.
// Should be called only if IsPrint fails.
private static bool isInGraphicList(int r) { 
    // We know r must fit in 16 bits - see makeisprint.go.
    if (r > 0xFFFF) {
        return false;
    }
    var rr = uint16(r);
    var i = bsearch16(isGraphic, rr);
    return i < len(isGraphic) && rr == isGraphic[i];
}

} // end strconv_package
