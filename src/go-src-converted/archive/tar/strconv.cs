// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package tar -- go2cs converted at 2020 October 09 05:08:06 UTC
// import "archive/tar" ==> using tar = go.archive.tar_package
// Original source: C:\Go\src\archive\tar\strconv.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using time = go.time_package;
using static go.builtin;

namespace go {
namespace archive
{
    public static partial class tar_package
    {
        // hasNUL reports whether the NUL character exists within s.
        private static bool hasNUL(@string s)
        {
            return strings.IndexByte(s, 0L) >= 0L;
        }

        // isASCII reports whether the input is an ASCII C-style string.
        private static bool isASCII(@string s)
        {
            foreach (var (_, c) in s)
            {
                if (c >= 0x80UL || c == 0x00UL)
                {
                    return false;
                }

            }
            return true;

        }

        // toASCII converts the input to an ASCII C-style string.
        // This a best effort conversion, so invalid characters are dropped.
        private static @string toASCII(@string s)
        {
            if (isASCII(s))
            {
                return s;
            }

            var b = make_slice<byte>(0L, len(s));
            foreach (var (_, c) in s)
            {
                if (c < 0x80UL && c != 0x00UL)
                {
                    b = append(b, byte(c));
                }

            }
            return string(b);

        }

        private partial struct parser
        {
            public error err; // Last error seen
        }

        private partial struct formatter
        {
            public error err; // Last error seen
        }

        // parseString parses bytes as a NUL-terminated C-style string.
        // If a NUL byte is not found then the whole slice is returned as a string.
        private static @string parseString(this ptr<parser> _addr__p0, slice<byte> b)
        {
            ref parser _p0 = ref _addr__p0.val;

            {
                var i = bytes.IndexByte(b, 0L);

                if (i >= 0L)
                {
                    return string(b[..i]);
                }

            }

            return string(b);

        }

        // formatString copies s into b, NUL-terminating if possible.
        private static void formatString(this ptr<formatter> _addr_f, slice<byte> b, @string s)
        {
            ref formatter f = ref _addr_f.val;

            if (len(s) > len(b))
            {
                f.err = ErrFieldTooLong;
            }

            copy(b, s);
            if (len(s) < len(b))
            {
                b[len(s)] = 0L;
            } 

            // Some buggy readers treat regular files with a trailing slash
            // in the V7 path field as a directory even though the full path
            // recorded elsewhere (e.g., via PAX record) contains no trailing slash.
            if (len(s) > len(b) && b[len(b) - 1L] == '/')
            {
                var n = len(strings.TrimRight(s[..len(b)], "/"));
                b[n] = 0L; // Replace trailing slash with NUL terminator
            }

        }

        // fitsInBase256 reports whether x can be encoded into n bytes using base-256
        // encoding. Unlike octal encoding, base-256 encoding does not require that the
        // string ends with a NUL character. Thus, all n bytes are available for output.
        //
        // If operating in binary mode, this assumes strict GNU binary mode; which means
        // that the first byte can only be either 0x80 or 0xff. Thus, the first byte is
        // equivalent to the sign bit in two's complement form.
        private static bool fitsInBase256(long n, long x)
        {
            var binBits = uint(n - 1L) * 8L;
            return n >= 9L || (x >= -1L << (int)(binBits) && x < 1L << (int)(binBits));
        }

        // parseNumeric parses the input as being encoded in either base-256 or octal.
        // This function may return negative numbers.
        // If parsing fails or an integer overflow occurs, err will be set.
        private static long parseNumeric(this ptr<parser> _addr_p, slice<byte> b)
        {
            ref parser p = ref _addr_p.val;
 
            // Check for base-256 (binary) format first.
            // If the first bit is set, then all following bits constitute a two's
            // complement encoded number in big-endian byte order.
            if (len(b) > 0L && b[0L] & 0x80UL != 0L)
            { 
                // Handling negative numbers relies on the following identity:
                //    -a-1 == ^a
                //
                // If the number is negative, we use an inversion mask to invert the
                // data bytes and treat the value as an unsigned number.
                byte inv = default; // 0x00 if positive or zero, 0xff if negative
                if (b[0L] & 0x40UL != 0L)
                {
                    inv = 0xffUL;
                }

                ulong x = default;
                foreach (var (i, c) in b)
                {
                    c ^= inv; // Inverts c only if inv is 0xff, otherwise does nothing
                    if (i == 0L)
                    {
                        c &= 0x7fUL; // Ignore signal bit in first byte
                    }

                    if ((x >> (int)(56L)) > 0L)
                    {
                        p.err = ErrHeader; // Integer overflow
                        return 0L;

                    }

                    x = x << (int)(8L) | uint64(c);

                }
                if ((x >> (int)(63L)) > 0L)
                {
                    p.err = ErrHeader; // Integer overflow
                    return 0L;

                }

                if (inv == 0xffUL)
                {
                    return ~int64(x);
                }

                return int64(x);

            } 

            // Normal case is base-8 (octal) format.
            return p.parseOctal(b);

        }

        // formatNumeric encodes x into b using base-8 (octal) encoding if possible.
        // Otherwise it will attempt to use base-256 (binary) encoding.
        private static void formatNumeric(this ptr<formatter> _addr_f, slice<byte> b, long x)
        {
            ref formatter f = ref _addr_f.val;

            if (fitsInOctal(len(b), x))
            {
                f.formatOctal(b, x);
                return ;
            }

            if (fitsInBase256(len(b), x))
            {
                for (var i = len(b) - 1L; i >= 0L; i--)
                {
                    b[i] = byte(x);
                    x >>= 8L;
                }

                b[0L] |= 0x80UL; // Highest bit indicates binary format
                return ;

            }

            f.formatOctal(b, 0L); // Last resort, just write zero
            f.err = ErrFieldTooLong;

        }

        private static long parseOctal(this ptr<parser> _addr_p, slice<byte> b)
        {
            ref parser p = ref _addr_p.val;
 
            // Because unused fields are filled with NULs, we need
            // to skip leading NULs. Fields may also be padded with
            // spaces or NULs.
            // So we remove leading and trailing NULs and spaces to
            // be sure.
            b = bytes.Trim(b, " \x00");

            if (len(b) == 0L)
            {
                return 0L;
            }

            var (x, perr) = strconv.ParseUint(p.parseString(b), 8L, 64L);
            if (perr != null)
            {
                p.err = ErrHeader;
            }

            return int64(x);

        }

        private static void formatOctal(this ptr<formatter> _addr_f, slice<byte> b, long x)
        {
            ref formatter f = ref _addr_f.val;

            if (!fitsInOctal(len(b), x))
            {
                x = 0L; // Last resort, just write zero
                f.err = ErrFieldTooLong;

            }

            var s = strconv.FormatInt(x, 8L); 
            // Add leading zeros, but leave room for a NUL.
            {
                var n = len(b) - len(s) - 1L;

                if (n > 0L)
                {
                    s = strings.Repeat("0", n) + s;
                }

            }

            f.formatString(b, s);

        }

        // fitsInOctal reports whether the integer x fits in a field n-bytes long
        // using octal encoding with the appropriate NUL terminator.
        private static bool fitsInOctal(long n, long x)
        {
            var octBits = uint(n - 1L) * 3L;
            return x >= 0L && (n >= 22L || x < 1L << (int)(octBits));
        }

        // parsePAXTime takes a string of the form %d.%d as described in the PAX
        // specification. Note that this implementation allows for negative timestamps,
        // which is allowed for by the PAX specification, but not always portable.
        private static (time.Time, error) parsePAXTime(@string s)
        {
            time.Time _p0 = default;
            error _p0 = default!;

            const long maxNanoSecondDigits = (long)9L; 

            // Split string into seconds and sub-seconds parts.
 

            // Split string into seconds and sub-seconds parts.
            var ss = s;
            @string sn = "";
            {
                var pos = strings.IndexByte(s, '.');

                if (pos >= 0L)
                {
                    ss = s[..pos];
                    sn = s[pos + 1L..];

                } 

                // Parse the seconds.

            } 

            // Parse the seconds.
            var (secs, err) = strconv.ParseInt(ss, 10L, 64L);
            if (err != null)
            {
                return (new time.Time(), error.As(ErrHeader)!);
            }

            if (len(sn) == 0L)
            {
                return (time.Unix(secs, 0L), error.As(null!)!); // No sub-second values
            } 

            // Parse the nanoseconds.
            if (strings.Trim(sn, "0123456789") != "")
            {
                return (new time.Time(), error.As(ErrHeader)!);
            }

            if (len(sn) < maxNanoSecondDigits)
            {
                sn += strings.Repeat("0", maxNanoSecondDigits - len(sn)); // Right pad
            }
            else
            {
                sn = sn[..maxNanoSecondDigits]; // Right truncate
            }

            var (nsecs, _) = strconv.ParseInt(sn, 10L, 64L); // Must succeed
            if (len(ss) > 0L && ss[0L] == '-')
            {
                return (time.Unix(secs, -1L * nsecs), error.As(null!)!); // Negative correction
            }

            return (time.Unix(secs, nsecs), error.As(null!)!);

        }

        // formatPAXTime converts ts into a time of the form %d.%d as described in the
        // PAX specification. This function is capable of negative timestamps.
        private static @string formatPAXTime(time.Time ts)
        {
            @string s = default;

            var secs = ts.Unix();
            var nsecs = ts.Nanosecond();
            if (nsecs == 0L)
            {
                return strconv.FormatInt(secs, 10L);
            } 

            // If seconds is negative, then perform correction.
            @string sign = "";
            if (secs < 0L)
            {
                sign = "-"; // Remember sign
                secs = -(secs + 1L); // Add a second to secs
                nsecs = -(nsecs - 1e9F); // Take that second away from nsecs
            }

            return strings.TrimRight(fmt.Sprintf("%s%d.%09d", sign, secs, nsecs), "0");

        }

        // parsePAXRecord parses the input PAX record string into a key-value pair.
        // If parsing is successful, it will slice off the currently read record and
        // return the remainder as r.
        private static (@string, @string, @string, error) parsePAXRecord(@string s)
        {
            @string k = default;
            @string v = default;
            @string r = default;
            error err = default!;
 
            // The size field ends at the first space.
            var sp = strings.IndexByte(s, ' ');
            if (sp == -1L)
            {
                return ("", "", s, error.As(ErrHeader)!);
            } 

            // Parse the first token as a decimal integer.
            var (n, perr) = strconv.ParseInt(s[..sp], 10L, 0L); // Intentionally parse as native int
            if (perr != null || n < 5L || int64(len(s)) < n)
            {
                return ("", "", s, error.As(ErrHeader)!);
            } 

            // Extract everything between the space and the final newline.
            var rec = s[sp + 1L..n - 1L];
            var nl = s[n - 1L..n];
            var rem = s[n..];
            if (nl != "\n")
            {
                return ("", "", s, error.As(ErrHeader)!);
            } 

            // The first equals separates the key from the value.
            var eq = strings.IndexByte(rec, '=');
            if (eq == -1L)
            {
                return ("", "", s, error.As(ErrHeader)!);
            }

            k = rec[..eq];
            v = rec[eq + 1L..];

            if (!validPAXRecord(k, v))
            {
                return ("", "", s, error.As(ErrHeader)!);
            }

            return (k, v, rem, error.As(null!)!);

        }

        // formatPAXRecord formats a single PAX record, prefixing it with the
        // appropriate length.
        private static (@string, error) formatPAXRecord(@string k, @string v)
        {
            @string _p0 = default;
            error _p0 = default!;

            if (!validPAXRecord(k, v))
            {
                return ("", error.As(ErrHeader)!);
            }

            const long padding = (long)3L; // Extra padding for ' ', '=', and '\n'
 // Extra padding for ' ', '=', and '\n'
            var size = len(k) + len(v) + padding;
            size += len(strconv.Itoa(size));
            var record = strconv.Itoa(size) + " " + k + "=" + v + "\n"; 

            // Final adjustment if adding size field increased the record size.
            if (len(record) != size)
            {
                size = len(record);
                record = strconv.Itoa(size) + " " + k + "=" + v + "\n";
            }

            return (record, error.As(null!)!);

        }

        // validPAXRecord reports whether the key-value pair is valid where each
        // record is formatted as:
        //    "%d %s=%s\n" % (size, key, value)
        //
        // Keys and values should be UTF-8, but the number of bad writers out there
        // forces us to be a more liberal.
        // Thus, we only reject all keys with NUL, and only reject NULs in values
        // for the PAX version of the USTAR string fields.
        // The key must not contain an '=' character.
        private static bool validPAXRecord(@string k, @string v)
        {
            if (k == "" || strings.IndexByte(k, '=') >= 0L)
            {
                return false;
            }


            if (k == paxPath || k == paxLinkpath || k == paxUname || k == paxGname) 
                return !hasNUL(v);
            else 
                return !hasNUL(k);
            
        }
    }
}}
