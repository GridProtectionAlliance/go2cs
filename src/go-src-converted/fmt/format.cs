// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package fmt -- go2cs converted at 2020 October 08 03:25:58 UTC
// import "fmt" ==> using fmt = go.fmt_package
// Original source: C:\Go\src\fmt\format.go
using strconv = go.strconv_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go
{
    public static partial class fmt_package
    {
        private static readonly @string ldigits = (@string)"0123456789abcdefx";
        private static readonly @string udigits = (@string)"0123456789ABCDEFX";


        private static readonly var signed = (var)true;
        private static readonly var unsigned = (var)false;


        // flags placed in a separate struct for easy clearing.
        private partial struct fmtFlags
        {
            public bool widPresent;
            public bool precPresent;
            public bool minus;
            public bool plus;
            public bool sharp;
            public bool space;
            public bool zero; // For the formats %+v %#v, we set the plusV/sharpV flags
// and clear the plus/sharp flags since %+v and %#v are in effect
// different, flagless formats set at the top level.
            public bool plusV;
            public bool sharpV;
        }

        // A fmt is the raw formatter used by Printf etc.
        // It prints into a buffer that must be set up separately.
        private partial struct fmt
        {
            public ptr<buffer> buf;
            public ref fmtFlags fmtFlags => ref fmtFlags_val;
            public long wid; // width
            public long prec; // precision

// intbuf is large enough to store %b of an int64 with a sign and
// avoids padding at the end of the struct on 32 bit architectures.
            public array<byte> intbuf;
        }

        private static void clearflags(this ptr<fmt> _addr_f)
        {
            ref fmt f = ref _addr_f.val;

            f.fmtFlags = new fmtFlags();
        }

        private static void init(this ptr<fmt> _addr_f, ptr<buffer> _addr_buf)
        {
            ref fmt f = ref _addr_f.val;
            ref buffer buf = ref _addr_buf.val;

            f.buf = buf;
            f.clearflags();
        }

        // writePadding generates n bytes of padding.
        private static void writePadding(this ptr<fmt> _addr_f, long n)
        {
            ref fmt f = ref _addr_f.val;

            if (n <= 0L)
            { // No padding bytes needed.
                return ;

            }

            var buf = f.buf.val;
            var oldLen = len(buf);
            var newLen = oldLen + n; 
            // Make enough room for padding.
            if (newLen > cap(buf))
            {
                buf = make(buffer, cap(buf) * 2L + n);
                copy(buf, f.buf.val);
            } 
            // Decide which byte the padding should be filled with.
            var padByte = byte(' ');
            if (f.zero)
            {
                padByte = byte('0');
            } 
            // Fill padding with padByte.
            var padding = buf[oldLen..newLen];
            foreach (var (i) in padding)
            {
                padding[i] = padByte;
            }
            f.buf.val = buf[..newLen];

        }

        // pad appends b to f.buf, padded on left (!f.minus) or right (f.minus).
        private static void pad(this ptr<fmt> _addr_f, slice<byte> b)
        {
            ref fmt f = ref _addr_f.val;

            if (!f.widPresent || f.wid == 0L)
            {
                f.buf.write(b);
                return ;
            }

            var width = f.wid - utf8.RuneCount(b);
            if (!f.minus)
            { 
                // left padding
                f.writePadding(width);
                f.buf.write(b);

            }
            else
            { 
                // right padding
                f.buf.write(b);
                f.writePadding(width);

            }

        }

        // padString appends s to f.buf, padded on left (!f.minus) or right (f.minus).
        private static void padString(this ptr<fmt> _addr_f, @string s)
        {
            ref fmt f = ref _addr_f.val;

            if (!f.widPresent || f.wid == 0L)
            {
                f.buf.writeString(s);
                return ;
            }

            var width = f.wid - utf8.RuneCountInString(s);
            if (!f.minus)
            { 
                // left padding
                f.writePadding(width);
                f.buf.writeString(s);

            }
            else
            { 
                // right padding
                f.buf.writeString(s);
                f.writePadding(width);

            }

        }

        // fmtBoolean formats a boolean.
        private static void fmtBoolean(this ptr<fmt> _addr_f, bool v)
        {
            ref fmt f = ref _addr_f.val;

            if (v)
            {
                f.padString("true");
            }
            else
            {
                f.padString("false");
            }

        }

        // fmtUnicode formats a uint64 as "U+0078" or with f.sharp set as "U+0078 'x'".
        private static void fmtUnicode(this ptr<fmt> _addr_f, ulong u)
        {
            ref fmt f = ref _addr_f.val;

            var buf = f.intbuf[0L..]; 

            // With default precision set the maximum needed buf length is 18
            // for formatting -1 with %#U ("U+FFFFFFFFFFFFFFFF") which fits
            // into the already allocated intbuf with a capacity of 68 bytes.
            long prec = 4L;
            if (f.precPresent && f.prec > 4L)
            {
                prec = f.prec; 
                // Compute space needed for "U+" , number, " '", character, "'".
                long width = 2L + prec + 2L + utf8.UTFMax + 1L;
                if (width > len(buf))
                {
                    buf = make_slice<byte>(width);
                }

            } 

            // Format into buf, ending at buf[i]. Formatting numbers is easier right-to-left.
            var i = len(buf); 

            // For %#U we want to add a space and a quoted character at the end of the buffer.
            if (f.sharp && u <= utf8.MaxRune && strconv.IsPrint(rune(u)))
            {
                i--;
                buf[i] = '\'';
                i -= utf8.RuneLen(rune(u));
                utf8.EncodeRune(buf[i..], rune(u));
                i--;
                buf[i] = '\'';
                i--;
                buf[i] = ' ';
            } 
            // Format the Unicode code point u as a hexadecimal number.
            while (u >= 16L)
            {
                i--;
                buf[i] = udigits[u & 0xFUL];
                prec--;
                u >>= 4L;
            }

            i--;
            buf[i] = udigits[u];
            prec--; 
            // Add zeros in front of the number until requested precision is reached.
            while (prec > 0L)
            {
                i--;
                buf[i] = '0';
                prec--;
            } 
            // Add a leading "U+".
 
            // Add a leading "U+".
            i--;
            buf[i] = '+';
            i--;
            buf[i] = 'U';

            var oldZero = f.zero;
            f.zero = false;
            f.pad(buf[i..]);
            f.zero = oldZero;

        }

        // fmtInteger formats signed and unsigned integers.
        private static void fmtInteger(this ptr<fmt> _addr_f, ulong u, long @base, bool isSigned, int verb, @string digits) => func((_, panic, __) =>
        {
            ref fmt f = ref _addr_f.val;

            var negative = isSigned && int64(u) < 0L;
            if (negative)
            {
                u = -u;
            }

            var buf = f.intbuf[0L..]; 
            // The already allocated f.intbuf with a capacity of 68 bytes
            // is large enough for integer formatting when no precision or width is set.
            if (f.widPresent || f.precPresent)
            { 
                // Account 3 extra bytes for possible addition of a sign and "0x".
                long width = 3L + f.wid + f.prec; // wid and prec are always positive.
                if (width > len(buf))
                { 
                    // We're going to need a bigger boat.
                    buf = make_slice<byte>(width);

                }

            } 

            // Two ways to ask for extra leading zero digits: %.3d or %03d.
            // If both are specified the f.zero flag is ignored and
            // padding with spaces is used instead.
            long prec = 0L;
            if (f.precPresent)
            {
                prec = f.prec; 
                // Precision of 0 and value of 0 means "print nothing" but padding.
                if (prec == 0L && u == 0L)
                {
                    var oldZero = f.zero;
                    f.zero = false;
                    f.writePadding(f.wid);
                    f.zero = oldZero;
                    return ;
                }

            }
            else if (f.zero && f.widPresent)
            {
                prec = f.wid;
                if (negative || f.plus || f.space)
                {
                    prec--; // leave room for sign
                }

            } 

            // Because printing is easier right-to-left: format u into buf, ending at buf[i].
            // We could make things marginally faster by splitting the 32-bit case out
            // into a separate block but it's not worth the duplication, so u has 64 bits.
            var i = len(buf); 
            // Use constants for the division and modulo for more efficient code.
            // Switch cases ordered by popularity.
            switch (base)
            {
                case 10L: 
                    while (u >= 10L)
                    {
                        i--;
                        var next = u / 10L;
                        buf[i] = byte('0' + u - next * 10L);
                        u = next;
                    }
                    break;
                case 16L: 
                    while (u >= 16L)
                    {
                        i--;
                        buf[i] = digits[u & 0xFUL];
                        u >>= 4L;
                    }
                    break;
                case 8L: 
                    while (u >= 8L)
                    {
                        i--;
                        buf[i] = byte('0' + u & 7L);
                        u >>= 3L;
                    }
                    break;
                case 2L: 
                    while (u >= 2L)
                    {
                        i--;
                        buf[i] = byte('0' + u & 1L);
                        u >>= 1L;
                    }
                    break;
                default: 
                    panic("fmt: unknown base; can't happen");
                    break;
            }
            i--;
            buf[i] = digits[u];
            while (i > 0L && prec > len(buf) - i)
            {
                i--;
                buf[i] = '0';
            } 

            // Various prefixes: 0x, -, etc.
 

            // Various prefixes: 0x, -, etc.
            if (f.sharp)
            {
                switch (base)
                {
                    case 2L: 
                        // Add a leading 0b.
                        i--;
                        buf[i] = 'b';
                        i--;
                        buf[i] = '0';
                        break;
                    case 8L: 
                        if (buf[i] != '0')
                        {
                            i--;
                            buf[i] = '0';
                        }

                        break;
                    case 16L: 
                        // Add a leading 0x or 0X.
                        i--;
                        buf[i] = digits[16L];
                        i--;
                        buf[i] = '0';
                        break;
                }

            }

            if (verb == 'O')
            {
                i--;
                buf[i] = 'o';
                i--;
                buf[i] = '0';
            }

            if (negative)
            {
                i--;
                buf[i] = '-';
            }
            else if (f.plus)
            {
                i--;
                buf[i] = '+';
            }
            else if (f.space)
            {
                i--;
                buf[i] = ' ';
            } 

            // Left padding with zeros has already been handled like precision earlier
            // or the f.zero flag is ignored due to an explicitly set precision.
            oldZero = f.zero;
            f.zero = false;
            f.pad(buf[i..]);
            f.zero = oldZero;

        });

        // truncate truncates the string s to the specified precision, if present.
        private static @string truncateString(this ptr<fmt> _addr_f, @string s)
        {
            ref fmt f = ref _addr_f.val;

            if (f.precPresent)
            {
                var n = f.prec;
                foreach (var (i) in s)
                {
                    n--;
                    if (n < 0L)
                    {
                        return s[..i];
                    }

                }

            }

            return s;

        }

        // truncate truncates the byte slice b as a string of the specified precision, if present.
        private static slice<byte> truncate(this ptr<fmt> _addr_f, slice<byte> b)
        {
            ref fmt f = ref _addr_f.val;

            if (f.precPresent)
            {
                var n = f.prec;
                {
                    long i = 0L;

                    while (i < len(b))
                    {
                        n--;
                        if (n < 0L)
                        {
                            return b[..i];
                        }

                        long wid = 1L;
                        if (b[i] >= utf8.RuneSelf)
                        {
                            _, wid = utf8.DecodeRune(b[i..]);
                        }

                        i += wid;

                    }

                }

            }

            return b;

        }

        // fmtS formats a string.
        private static void fmtS(this ptr<fmt> _addr_f, @string s)
        {
            ref fmt f = ref _addr_f.val;

            s = f.truncateString(s);
            f.padString(s);
        }

        // fmtBs formats the byte slice b as if it was formatted as string with fmtS.
        private static void fmtBs(this ptr<fmt> _addr_f, slice<byte> b)
        {
            ref fmt f = ref _addr_f.val;

            b = f.truncate(b);
            f.pad(b);
        }

        // fmtSbx formats a string or byte slice as a hexadecimal encoding of its bytes.
        private static void fmtSbx(this ptr<fmt> _addr_f, @string s, slice<byte> b, @string digits)
        {
            ref fmt f = ref _addr_f.val;

            var length = len(b);
            if (b == null)
            { 
                // No byte slice present. Assume string s should be encoded.
                length = len(s);

            } 
            // Set length to not process more bytes than the precision demands.
            if (f.precPresent && f.prec < length)
            {
                length = f.prec;
            } 
            // Compute width of the encoding taking into account the f.sharp and f.space flag.
            long width = 2L * length;
            if (width > 0L)
            {
                if (f.space)
                { 
                    // Each element encoded by two hexadecimals will get a leading 0x or 0X.
                    if (f.sharp)
                    {
                        width *= 2L;
                    } 
                    // Elements will be separated by a space.
                    width += length - 1L;

                }
                else if (f.sharp)
                { 
                    // Only a leading 0x or 0X will be added for the whole string.
                    width += 2L;

                }

            }
            else
            { // The byte slice or string that should be encoded is empty.
                if (f.widPresent)
                {
                    f.writePadding(f.wid);
                }

                return ;

            } 
            // Handle padding to the left.
            if (f.widPresent && f.wid > width && !f.minus)
            {
                f.writePadding(f.wid - width);
            } 
            // Write the encoding directly into the output buffer.
            var buf = f.buf.val;
            if (f.sharp)
            { 
                // Add leading 0x or 0X.
                buf = append(buf, '0', digits[16L]);

            }

            byte c = default;
            for (long i = 0L; i < length; i++)
            {
                if (f.space && i > 0L)
                { 
                    // Separate elements with a space.
                    buf = append(buf, ' ');
                    if (f.sharp)
                    { 
                        // Add leading 0x or 0X for each element.
                        buf = append(buf, '0', digits[16L]);

                    }

                }

                if (b != null)
                {
                    c = b[i]; // Take a byte from the input byte slice.
                }
                else
                {
                    c = s[i]; // Take a byte from the input string.
                } 
                // Encode each byte as two hexadecimal digits.
                buf = append(buf, digits[c >> (int)(4L)], digits[c & 0xFUL]);

            }

            f.buf.val = buf; 
            // Handle padding to the right.
            if (f.widPresent && f.wid > width && f.minus)
            {
                f.writePadding(f.wid - width);
            }

        }

        // fmtSx formats a string as a hexadecimal encoding of its bytes.
        private static void fmtSx(this ptr<fmt> _addr_f, @string s, @string digits)
        {
            ref fmt f = ref _addr_f.val;

            f.fmtSbx(s, null, digits);
        }

        // fmtBx formats a byte slice as a hexadecimal encoding of its bytes.
        private static void fmtBx(this ptr<fmt> _addr_f, slice<byte> b, @string digits)
        {
            ref fmt f = ref _addr_f.val;

            f.fmtSbx("", b, digits);
        }

        // fmtQ formats a string as a double-quoted, escaped Go string constant.
        // If f.sharp is set a raw (backquoted) string may be returned instead
        // if the string does not contain any control characters other than tab.
        private static void fmtQ(this ptr<fmt> _addr_f, @string s)
        {
            ref fmt f = ref _addr_f.val;

            s = f.truncateString(s);
            if (f.sharp && strconv.CanBackquote(s))
            {
                f.padString("`" + s + "`");
                return ;
            }

            var buf = f.intbuf[..0L];
            if (f.plus)
            {
                f.pad(strconv.AppendQuoteToASCII(buf, s));
            }
            else
            {
                f.pad(strconv.AppendQuote(buf, s));
            }

        }

        // fmtC formats an integer as a Unicode character.
        // If the character is not valid Unicode, it will print '\ufffd'.
        private static void fmtC(this ptr<fmt> _addr_f, ulong c)
        {
            ref fmt f = ref _addr_f.val;

            var r = rune(c);
            if (c > utf8.MaxRune)
            {
                r = utf8.RuneError;
            }

            var buf = f.intbuf[..0L];
            var w = utf8.EncodeRune(buf[..utf8.UTFMax], r);
            f.pad(buf[..w]);

        }

        // fmtQc formats an integer as a single-quoted, escaped Go character constant.
        // If the character is not valid Unicode, it will print '\ufffd'.
        private static void fmtQc(this ptr<fmt> _addr_f, ulong c)
        {
            ref fmt f = ref _addr_f.val;

            var r = rune(c);
            if (c > utf8.MaxRune)
            {
                r = utf8.RuneError;
            }

            var buf = f.intbuf[..0L];
            if (f.plus)
            {
                f.pad(strconv.AppendQuoteRuneToASCII(buf, r));
            }
            else
            {
                f.pad(strconv.AppendQuoteRune(buf, r));
            }

        }

        // fmtFloat formats a float64. It assumes that verb is a valid format specifier
        // for strconv.AppendFloat and therefore fits into a byte.
        private static void fmtFloat(this ptr<fmt> _addr_f, double v, long size, int verb, long prec)
        {
            ref fmt f = ref _addr_f.val;
 
            // Explicit precision in format specifier overrules default precision.
            if (f.precPresent)
            {
                prec = f.prec;
            } 
            // Format number, reserving space for leading + sign if needed.
            var num = strconv.AppendFloat(f.intbuf[..1L], v, byte(verb), prec, size);
            if (num[1L] == '-' || num[1L] == '+')
            {
                num = num[1L..];
            }
            else
            {
                num[0L] = '+';
            } 
            // f.space means to add a leading space instead of a "+" sign unless
            // the sign is explicitly asked for by f.plus.
            if (f.space && num[0L] == '+' && !f.plus)
            {
                num[0L] = ' ';
            } 
            // Special handling for infinities and NaN,
            // which don't look like a number so shouldn't be padded with zeros.
            if (num[1L] == 'I' || num[1L] == 'N')
            {
                var oldZero = f.zero;
                f.zero = false; 
                // Remove sign before NaN if not asked for.
                if (num[1L] == 'N' && !f.space && !f.plus)
                {
                    num = num[1L..];
                }

                f.pad(num);
                f.zero = oldZero;
                return ;

            } 
            // The sharp flag forces printing a decimal point for non-binary formats
            // and retains trailing zeros, which we may need to restore.
            if (f.sharp && verb != 'b')
            {
                long digits = 0L;
                switch (verb)
                {
                    case 'v': 

                    case 'g': 

                    case 'G': 

                    case 'x': 
                        digits = prec; 
                        // If no precision is set explicitly use a precision of 6.
                        if (digits == -1L)
                        {
                            digits = 6L;
                        }

                        break;
                } 

                // Buffer pre-allocated with enough room for
                // exponent notations of the form "e+123" or "p-1023".
                array<byte> tailBuf = new array<byte>(6L);
                var tail = tailBuf[..0L];

                var hasDecimalPoint = false;
                var sawNonzeroDigit = false; 
                // Starting from i = 1 to skip sign at num[0].
                for (long i = 1L; i < len(num); i++)
                {

                    if (num[i] == '.')
                    {
                        hasDecimalPoint = true;
                        goto __switch_break0;
                    }
                    if (num[i] == 'p' || num[i] == 'P')
                    {
                        tail = append(tail, num[i..]);
                        num = num[..i];
                        goto __switch_break0;
                    }
                    if (num[i] == 'e' || num[i] == 'E')
                    {
                        if (verb != 'x' && verb != 'X')
                        {
                            tail = append(tail, num[i..]);
                            num = num[..i];
                            break;
                        }

                    }
                    // default: 
                        if (num[i] != '0')
                        {
                            sawNonzeroDigit = true;
                        } 
                        // Count significant digits after the first non-zero digit.
                        if (sawNonzeroDigit)
                        {
                            digits--;
                        }


                    __switch_break0:;

                }

                if (!hasDecimalPoint)
                { 
                    // Leading digit 0 should contribute once to digits.
                    if (len(num) == 2L && num[1L] == '0')
                    {
                        digits--;
                    }

                    num = append(num, '.');

                }

                while (digits > 0L)
                {
                    num = append(num, '0');
                    digits--;
                }

                num = append(num, tail);

            } 
            // We want a sign if asked for and if the sign is not positive.
            if (f.plus || num[0L] != '+')
            { 
                // If we're zero padding to the left we want the sign before the leading zeros.
                // Achieve this by writing the sign out and then padding the unsigned number.
                if (f.zero && f.widPresent && f.wid > len(num))
                {
                    f.buf.writeByte(num[0L]);
                    f.writePadding(f.wid - len(num));
                    f.buf.write(num[1L..]);
                    return ;
                }

                f.pad(num);
                return ;

            } 
            // No sign to show and the number is positive; just print the unsigned number.
            f.pad(num[1L..]);

        }
    }
}
