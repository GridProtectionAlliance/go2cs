// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:generate go run makeisprint.go -output isprint.go

// package strconv -- go2cs converted at 2020 October 09 05:06:35 UTC
// import "strconv" ==> using strconv = go.strconv_package
// Original source: C:\Go\src\strconv\quote.go
using bytealg = go.@internal.bytealg_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go
{
    public static partial class strconv_package
    {
        private static readonly @string lowerhex = (@string)"0123456789abcdef";
        private static readonly @string upperhex = (@string)"0123456789ABCDEF";


        private static @string quoteWith(@string s, byte quote, bool ASCIIonly, bool graphicOnly)
        {
            return string(appendQuotedWith(make_slice<byte>(0L, 3L * len(s) / 2L), s, quote, ASCIIonly, graphicOnly));
        }

        private static @string quoteRuneWith(int r, byte quote, bool ASCIIonly, bool graphicOnly)
        {
            return string(appendQuotedRuneWith(null, r, quote, ASCIIonly, graphicOnly));
        }

        private static slice<byte> appendQuotedWith(slice<byte> buf, @string s, byte quote, bool ASCIIonly, bool graphicOnly)
        { 
            // Often called with big strings, so preallocate. If there's quoting,
            // this is conservative but still helps a lot.
            if (cap(buf) - len(buf) < len(s))
            {
                var nBuf = make_slice<byte>(len(buf), len(buf) + 1L + len(s) + 1L);
                copy(nBuf, buf);
                buf = nBuf;
            }

            buf = append(buf, quote);
            {
                long width = 0L;

                while (len(s) > 0L)
                {
                    var r = rune(s[0L]);
                    width = 1L;
                    if (r >= utf8.RuneSelf)
                    {
                        r, width = utf8.DecodeRuneInString(s);
                    s = s[width..];
                    }

                    if (width == 1L && r == utf8.RuneError)
                    {
                        buf = append(buf, "\\x");
                        buf = append(buf, lowerhex[s[0L] >> (int)(4L)]);
                        buf = append(buf, lowerhex[s[0L] & 0xFUL]);
                        continue;
                    }

                    buf = appendEscapedRune(buf, r, quote, ASCIIonly, graphicOnly);

                }

            }
            buf = append(buf, quote);
            return buf;

        }

        private static slice<byte> appendQuotedRuneWith(slice<byte> buf, int r, byte quote, bool ASCIIonly, bool graphicOnly)
        {
            buf = append(buf, quote);
            if (!utf8.ValidRune(r))
            {
                r = utf8.RuneError;
            }

            buf = appendEscapedRune(buf, r, quote, ASCIIonly, graphicOnly);
            buf = append(buf, quote);
            return buf;

        }

        private static slice<byte> appendEscapedRune(slice<byte> buf, int r, byte quote, bool ASCIIonly, bool graphicOnly)
        {
            array<byte> runeTmp = new array<byte>(utf8.UTFMax);
            if (r == rune(quote) || r == '\\')
            { // always backslashed
                buf = append(buf, '\\');
                buf = append(buf, byte(r));
                return buf;

            }

            if (ASCIIonly)
            {
                if (r < utf8.RuneSelf && IsPrint(r))
                {
                    buf = append(buf, byte(r));
                    return buf;
                }

            }
            else if (IsPrint(r) || graphicOnly && isInGraphicList(r))
            {
                var n = utf8.EncodeRune(runeTmp[..], r);
                buf = append(buf, runeTmp[..n]);
                return buf;
            }

            switch (r)
            {
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
                        buf = append(buf, lowerhex[byte(r) >> (int)(4L)]);
                        buf = append(buf, lowerhex[byte(r) & 0xFUL]);
                        goto __switch_break0;
                    }
                    if (r > utf8.MaxRune)
                    {
                        r = 0xFFFDUL;
                        fallthrough = true;
                    }
                    if (fallthrough || r < 0x10000UL)
                    {
                        buf = append(buf, "\\u");
                        {
                            long s__prev1 = s;

                            long s = 12L;

                            while (s >= 0L)
                            {
                                buf = append(buf, lowerhex[r >> (int)(uint(s)) & 0xFUL]);
                                s -= 4L;
                            }


                            s = s__prev1;
                        }
                        goto __switch_break0;
                    }
                    // default: 
                        buf = append(buf, "\\U");
                        {
                            long s__prev1 = s;

                            s = 28L;

                            while (s >= 0L)
                            {
                                buf = append(buf, lowerhex[r >> (int)(uint(s)) & 0xFUL]);
                                s -= 4L;
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
        public static @string Quote(@string s)
        {
            return quoteWith(s, '"', false, false);
        }

        // AppendQuote appends a double-quoted Go string literal representing s,
        // as generated by Quote, to dst and returns the extended buffer.
        public static slice<byte> AppendQuote(slice<byte> dst, @string s)
        {
            return appendQuotedWith(dst, s, '"', false, false);
        }

        // QuoteToASCII returns a double-quoted Go string literal representing s.
        // The returned string uses Go escape sequences (\t, \n, \xFF, \u0100) for
        // non-ASCII characters and non-printable characters as defined by IsPrint.
        public static @string QuoteToASCII(@string s)
        {
            return quoteWith(s, '"', true, false);
        }

        // AppendQuoteToASCII appends a double-quoted Go string literal representing s,
        // as generated by QuoteToASCII, to dst and returns the extended buffer.
        public static slice<byte> AppendQuoteToASCII(slice<byte> dst, @string s)
        {
            return appendQuotedWith(dst, s, '"', true, false);
        }

        // QuoteToGraphic returns a double-quoted Go string literal representing s.
        // The returned string leaves Unicode graphic characters, as defined by
        // IsGraphic, unchanged and uses Go escape sequences (\t, \n, \xFF, \u0100)
        // for non-graphic characters.
        public static @string QuoteToGraphic(@string s)
        {
            return quoteWith(s, '"', false, true);
        }

        // AppendQuoteToGraphic appends a double-quoted Go string literal representing s,
        // as generated by QuoteToGraphic, to dst and returns the extended buffer.
        public static slice<byte> AppendQuoteToGraphic(slice<byte> dst, @string s)
        {
            return appendQuotedWith(dst, s, '"', false, true);
        }

        // QuoteRune returns a single-quoted Go character literal representing the
        // rune. The returned string uses Go escape sequences (\t, \n, \xFF, \u0100)
        // for control characters and non-printable characters as defined by IsPrint.
        public static @string QuoteRune(int r)
        {
            return quoteRuneWith(r, '\'', false, false);
        }

        // AppendQuoteRune appends a single-quoted Go character literal representing the rune,
        // as generated by QuoteRune, to dst and returns the extended buffer.
        public static slice<byte> AppendQuoteRune(slice<byte> dst, int r)
        {
            return appendQuotedRuneWith(dst, r, '\'', false, false);
        }

        // QuoteRuneToASCII returns a single-quoted Go character literal representing
        // the rune. The returned string uses Go escape sequences (\t, \n, \xFF,
        // \u0100) for non-ASCII characters and non-printable characters as defined
        // by IsPrint.
        public static @string QuoteRuneToASCII(int r)
        {
            return quoteRuneWith(r, '\'', true, false);
        }

        // AppendQuoteRuneToASCII appends a single-quoted Go character literal representing the rune,
        // as generated by QuoteRuneToASCII, to dst and returns the extended buffer.
        public static slice<byte> AppendQuoteRuneToASCII(slice<byte> dst, int r)
        {
            return appendQuotedRuneWith(dst, r, '\'', true, false);
        }

        // QuoteRuneToGraphic returns a single-quoted Go character literal representing
        // the rune. If the rune is not a Unicode graphic character,
        // as defined by IsGraphic, the returned string will use a Go escape sequence
        // (\t, \n, \xFF, \u0100).
        public static @string QuoteRuneToGraphic(int r)
        {
            return quoteRuneWith(r, '\'', false, true);
        }

        // AppendQuoteRuneToGraphic appends a single-quoted Go character literal representing the rune,
        // as generated by QuoteRuneToGraphic, to dst and returns the extended buffer.
        public static slice<byte> AppendQuoteRuneToGraphic(slice<byte> dst, int r)
        {
            return appendQuotedRuneWith(dst, r, '\'', false, true);
        }

        // CanBackquote reports whether the string s can be represented
        // unchanged as a single-line backquoted string without control
        // characters other than tab.
        public static bool CanBackquote(@string s)
        {
            while (len(s) > 0L)
            {
                var (r, wid) = utf8.DecodeRuneInString(s);
                s = s[wid..];
                if (wid > 1L)
                {
                    if (r == '\ufeff')
                    {
                        return false; // BOMs are invisible and should not be quoted.
                    }

                    continue; // All other multibyte runes are correctly encoded and assumed printable.
                }

                if (r == utf8.RuneError)
                {
                    return false;
                }

                if ((r < ' ' && r != '\t') || r == '`' || r == '\u007F')
                {
                    return false;
                }

            }

            return true;

        }

        private static (int, bool) unhex(byte b)
        {
            int v = default;
            bool ok = default;

            var c = rune(b);

            if ('0' <= c && c <= '9') 
                return (c - '0', true);
            else if ('a' <= c && c <= 'f') 
                return (c - 'a' + 10L, true);
            else if ('A' <= c && c <= 'F') 
                return (c - 'A' + 10L, true);
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
        public static (int, bool, @string, error) UnquoteChar(@string s, byte quote)
        {
            int value = default;
            bool multibyte = default;
            @string tail = default;
            error err = default!;
 
            // easy cases
            if (len(s) == 0L)
            {
                err = ErrSyntax;
                return ;
            }

            {
                var c__prev1 = c;

                var c = s[0L];


                if (c == quote && (quote == '\'' || quote == '"')) 
                    err = ErrSyntax;
                    return ;
                else if (c >= utf8.RuneSelf) 
                    var (r, size) = utf8.DecodeRuneInString(s);
                    return (r, true, s[size..], error.As(null!)!);
                else if (c != '\\') 
                    return (rune(s[0L]), false, s[1L..], error.As(null!)!);


                c = c__prev1;
            } 

            // hard case: c is backslash
            if (len(s) <= 1L)
            {
                err = ErrSyntax;
                return ;
            }

            c = s[1L];
            s = s[2L..];

            switch (c)
            {
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
                    long n = 0L;
                    switch (c)
                    {
                        case 'x': 
                            n = 2L;
                            break;
                        case 'u': 
                            n = 4L;
                            break;
                        case 'U': 
                            n = 8L;
                            break;
                    }
                    int v = default;
                    if (len(s) < n)
                    {
                        err = ErrSyntax;
                        return ;
                    }

                    {
                        long j__prev1 = j;

                        for (long j = 0L; j < n; j++)
                        {
                            var (x, ok) = unhex(s[j]);
                            if (!ok)
                            {
                                err = ErrSyntax;
                                return ;
                            }

                            v = v << (int)(4L) | x;

                        }


                        j = j__prev1;
                    }
                    s = s[n..];
                    if (c == 'x')
                    { 
                        // single-byte string, possibly not UTF-8
                        value = v;
                        break;

                    }

                    if (v > utf8.MaxRune)
                    {
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
                    if (len(s) < 2L)
                    {
                        err = ErrSyntax;
                        return ;
                    }

                    {
                        long j__prev1 = j;

                        for (j = 0L; j < 2L; j++)
                        { // one digit already; two more
                            var x = rune(s[j]) - '0';
                            if (x < 0L || x > 7L)
                            {
                                err = ErrSyntax;
                                return ;
                            }

                            v = (v << (int)(3L)) | x;

                        }


                        j = j__prev1;
                    }
                    s = s[2L..];
                    if (v > 255L)
                    {
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
                    if (c != quote)
                    {
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

        // Unquote interprets s as a single-quoted, double-quoted,
        // or backquoted Go string literal, returning the string value
        // that s quotes.  (If s is single-quoted, it would be a Go
        // character literal; Unquote returns the corresponding
        // one-character string.)
        public static (@string, error) Unquote(@string s)
        {
            @string _p0 = default;
            error _p0 = default!;

            var n = len(s);
            if (n < 2L)
            {
                return ("", error.As(ErrSyntax)!);
            }

            var quote = s[0L];
            if (quote != s[n - 1L])
            {
                return ("", error.As(ErrSyntax)!);
            }

            s = s[1L..n - 1L];

            if (quote == '`')
            {
                if (contains(s, '`'))
                {
                    return ("", error.As(ErrSyntax)!);
                }

                if (contains(s, '\r'))
                { 
                    // -1 because we know there is at least one \r to remove.
                    var buf = make_slice<byte>(0L, len(s) - 1L);
                    for (long i = 0L; i < len(s); i++)
                    {
                        if (s[i] != '\r')
                        {
                            buf = append(buf, s[i]);
                        }

                    }

                    return (string(buf), error.As(null!)!);

                }

                return (s, error.As(null!)!);

            }

            if (quote != '"' && quote != '\'')
            {
                return ("", error.As(ErrSyntax)!);
            }

            if (contains(s, '\n'))
            {
                return ("", error.As(ErrSyntax)!);
            } 

            // Is it trivial? Avoid allocation.
            if (!contains(s, '\\') && !contains(s, quote))
            {
                switch (quote)
                {
                    case '"': 
                        if (utf8.ValidString(s))
                        {
                            return (s, error.As(null!)!);
                        }

                        break;
                    case '\'': 
                        var (r, size) = utf8.DecodeRuneInString(s);
                        if (size == len(s) && (r != utf8.RuneError || size != 1L))
                        {
                            return (s, error.As(null!)!);
                        }

                        break;
                }

            }

            array<byte> runeTmp = new array<byte>(utf8.UTFMax);
            buf = make_slice<byte>(0L, 3L * len(s) / 2L); // Try to avoid more allocations.
            while (len(s) > 0L)
            {
                var (c, multibyte, ss, err) = UnquoteChar(s, quote);
                if (err != null)
                {
                    return ("", error.As(err)!);
                }

                s = ss;
                if (c < utf8.RuneSelf || !multibyte)
                {
                    buf = append(buf, byte(c));
                }
                else
                {
                    n = utf8.EncodeRune(runeTmp[..], c);
                    buf = append(buf, runeTmp[..n]);
                }

                if (quote == '\'' && len(s) != 0L)
                { 
                    // single-quoted must be single character
                    return ("", error.As(ErrSyntax)!);

                }

            }

            return (string(buf), error.As(null!)!);

        }

        // contains reports whether the string contains the byte c.
        private static bool contains(@string s, byte c)
        {
            return bytealg.IndexByteString(s, c) != -1L;
        }

        // bsearch16 returns the smallest i such that a[i] >= x.
        // If there is no such i, bsearch16 returns len(a).
        private static long bsearch16(slice<ushort> a, ushort x)
        {
            long i = 0L;
            var j = len(a);
            while (i < j)
            {
                var h = i + (j - i) / 2L;
                if (a[h] < x)
                {
                    i = h + 1L;
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
        private static long bsearch32(slice<uint> a, uint x)
        {
            long i = 0L;
            var j = len(a);
            while (i < j)
            {
                var h = i + (j - i) / 2L;
                if (a[h] < x)
                {
                    i = h + 1L;
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
        public static bool IsPrint(int r)
        { 
            // Fast check for Latin-1
            if (r <= 0xFFUL)
            {
                if (0x20UL <= r && r <= 0x7EUL)
                { 
                    // All the ASCII is printable from space through DEL-1.
                    return true;

                }

                if (0xA1UL <= r && r <= 0xFFUL)
                { 
                    // Similarly for ¡ through ÿ...
                    return r != 0xADUL; // ...except for the bizarre soft hyphen.
                }

                return false;

            } 

            // Same algorithm, either on uint16 or uint32 value.
            // First, find first i such that isPrint[i] >= x.
            // This is the index of either the start or end of a pair that might span x.
            // The start is even (isPrint[i&^1]) and the end is odd (isPrint[i|1]).
            // If we find x in a range, make sure x is not in isNotPrint list.
            if (0L <= r && r < 1L << (int)(16L))
            {
                var rr = uint16(r);
                var isPrint = isPrint16;
                var isNotPrint = isNotPrint16;
                var i = bsearch16(isPrint, rr);
                if (i >= len(isPrint) || rr < isPrint[i & ~1L] || isPrint[i | 1L] < rr)
                {
                    return false;
                }

                var j = bsearch16(isNotPrint, rr);
                return j >= len(isNotPrint) || isNotPrint[j] != rr;

            }

            rr = uint32(r);
            isPrint = isPrint32;
            isNotPrint = isNotPrint32;
            i = bsearch32(isPrint, rr);
            if (i >= len(isPrint) || rr < isPrint[i & ~1L] || isPrint[i | 1L] < rr)
            {
                return false;
            }

            if (r >= 0x20000UL)
            {
                return true;
            }

            r -= 0x10000UL;
            j = bsearch16(isNotPrint, uint16(r));
            return j >= len(isNotPrint) || isNotPrint[j] != uint16(r);

        }

        // IsGraphic reports whether the rune is defined as a Graphic by Unicode. Such
        // characters include letters, marks, numbers, punctuation, symbols, and
        // spaces, from categories L, M, N, P, S, and Zs.
        public static bool IsGraphic(int r)
        {
            if (IsPrint(r))
            {
                return true;
            }

            return isInGraphicList(r);

        }

        // isInGraphicList reports whether the rune is in the isGraphic list. This separation
        // from IsGraphic allows quoteWith to avoid two calls to IsPrint.
        // Should be called only if IsPrint fails.
        private static bool isInGraphicList(int r)
        { 
            // We know r must fit in 16 bits - see makeisprint.go.
            if (r > 0xFFFFUL)
            {
                return false;
            }

            var rr = uint16(r);
            var i = bsearch16(isGraphic, rr);
            return i < len(isGraphic) && rr == isGraphic[i];

        }
    }
}
