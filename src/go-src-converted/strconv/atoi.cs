// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package strconv -- go2cs converted at 2020 August 29 08:42:49 UTC
// import "strconv" ==> using strconv = go.strconv_package
// Original source: C:\Go\src\strconv\atoi.go
using errors = go.errors_package;
using static go.builtin;

namespace go
{
    public static partial class strconv_package
    {
        // ErrRange indicates that a value is out of range for the target type.
        public static var ErrRange = errors.New("value out of range");

        // ErrSyntax indicates that a value does not have the right syntax for the target type.
        public static var ErrSyntax = errors.New("invalid syntax");

        // A NumError records a failed conversion.
        public partial struct NumError
        {
            public @string Func; // the failing function (ParseBool, ParseInt, ParseUint, ParseFloat)
            public @string Num; // the input
            public error Err; // the reason the conversion failed (e.g. ErrRange, ErrSyntax, etc.)
        }

        private static @string Error(this ref NumError e)
        {
            return "strconv." + e.Func + ": " + "parsing " + Quote(e.Num) + ": " + e.Err.Error();
        }

        private static ref NumError syntaxError(@string fn, @string str)
        {
            return ref new NumError(fn,str,ErrSyntax);
        }

        private static ref NumError rangeError(@string fn, @string str)
        {
            return ref new NumError(fn,str,ErrRange);
        }

        private static ref NumError baseError(@string fn, @string str, long @base)
        {
            return ref new NumError(fn,str,errors.New("invalid base "+Itoa(base)));
        }

        private static ref NumError bitSizeError(@string fn, @string str, long bitSize)
        {
            return ref new NumError(fn,str,errors.New("invalid bit size "+Itoa(bitSize)));
        }

        private static readonly long intSize = 32L << (int)((~uint(0L) >> (int)(63L)));

        // IntSize is the size in bits of an int or uint value.


        // IntSize is the size in bits of an int or uint value.
        public static readonly var IntSize = intSize;



        private static readonly long maxUint64 = (1L << (int)(64L) - 1L);

        // ParseUint is like ParseInt but for unsigned numbers.


        // ParseUint is like ParseInt but for unsigned numbers.
        public static (ulong, error) ParseUint(@string s, long @base, long bitSize)
        {
            const @string fnParseUint = "ParseUint";



            if (len(s) == 0L)
            {
                return (0L, syntaxError(fnParseUint, s));
            }
            var s0 = s;

            if (2L <= base && base <= 36L)             else if (base == 0L) 
                // Look for octal, hex prefix.

                if (s[0L] == '0' && len(s) > 1L && (s[1L] == 'x' || s[1L] == 'X')) 
                    if (len(s) < 3L)
                    {
                        return (0L, syntaxError(fnParseUint, s0));
                    }
                    base = 16L;
                    s = s[2L..];
                else if (s[0L] == '0') 
                    base = 8L;
                    s = s[1L..];
                else 
                    base = 10L;
                            else 
                return (0L, baseError(fnParseUint, s0, base));
                        if (bitSize == 0L)
            {
                bitSize = int(IntSize);
            }
            else if (bitSize < 0L || bitSize > 64L)
            {
                return (0L, bitSizeError(fnParseUint, s0, bitSize));
            } 

            // Cutoff is the smallest number such that cutoff*base > maxUint64.
            // Use compile-time constants for common cases.
            ulong cutoff = default;
            switch (base)
            {
                case 10L: 
                    cutoff = maxUint64 / 10L + 1L;
                    break;
                case 16L: 
                    cutoff = maxUint64 / 16L + 1L;
                    break;
                default: 
                    cutoff = maxUint64 / uint64(base) + 1L;
                    break;
            }

            var maxVal = uint64(1L) << (int)(uint(bitSize)) - 1L;

            ulong n = default;
            foreach (var (_, c) in (slice<byte>)s)
            {
                byte d = default;

                if ('0' <= c && c <= '9') 
                    d = c - '0';
                else if ('a' <= c && c <= 'z') 
                    d = c - 'a' + 10L;
                else if ('A' <= c && c <= 'Z') 
                    d = c - 'A' + 10L;
                else 
                    return (0L, syntaxError(fnParseUint, s0));
                                if (d >= byte(base))
                {
                    return (0L, syntaxError(fnParseUint, s0));
                }
                if (n >= cutoff)
                { 
                    // n*base overflows
                    return (maxVal, rangeError(fnParseUint, s0));
                }
                n *= uint64(base);

                var n1 = n + uint64(d);
                if (n1 < n || n1 > maxVal)
                { 
                    // n+v overflows
                    return (maxVal, rangeError(fnParseUint, s0));
                }
                n = n1;
            }
            return (n, null);
        }

        // ParseInt interprets a string s in the given base (0, 2 to 36) and
        // bit size (0 to 64) and returns the corresponding value i.
        //
        // If base == 0, the base is implied by the string's prefix:
        // base 16 for "0x", base 8 for "0", and base 10 otherwise.
        // For bases 1, below 0 or above 36 an error is returned.
        //
        // The bitSize argument specifies the integer type
        // that the result must fit into. Bit sizes 0, 8, 16, 32, and 64
        // correspond to int, int8, int16, int32, and int64.
        // For a bitSize below 0 or above 64 an error is returned.
        //
        // The errors that ParseInt returns have concrete type *NumError
        // and include err.Num = s. If s is empty or contains invalid
        // digits, err.Err = ErrSyntax and the returned value is 0;
        // if the value corresponding to s cannot be represented by a
        // signed integer of the given size, err.Err = ErrRange and the
        // returned value is the maximum magnitude integer of the
        // appropriate bitSize and sign.
        public static (long, error) ParseInt(@string s, long @base, long bitSize)
        {
            const @string fnParseInt = "ParseInt"; 

            // Empty string bad.
 

            // Empty string bad.
            if (len(s) == 0L)
            {
                return (0L, syntaxError(fnParseInt, s));
            } 

            // Pick off leading sign.
            var s0 = s;
            var neg = false;
            if (s[0L] == '+')
            {
                s = s[1L..];
            }
            else if (s[0L] == '-')
            {
                neg = true;
                s = s[1L..];
            } 

            // Convert unsigned and check range.
            ulong un = default;
            un, err = ParseUint(s, base, bitSize);
            if (err != null && err._<ref NumError>().Err != ErrRange)
            {
                err._<ref NumError>().Func = fnParseInt;
                err._<ref NumError>().Num = s0;
                return (0L, err);
            }
            if (bitSize == 0L)
            {
                bitSize = int(IntSize);
            }
            var cutoff = uint64(1L << (int)(uint(bitSize - 1L)));
            if (!neg && un >= cutoff)
            {
                return (int64(cutoff - 1L), rangeError(fnParseInt, s0));
            }
            if (neg && un > cutoff)
            {
                return (-int64(cutoff), rangeError(fnParseInt, s0));
            }
            var n = int64(un);
            if (neg)
            {
                n = -n;
            }
            return (n, null);
        }

        // Atoi returns the result of ParseInt(s, 10, 0) converted to type int.
        public static (long, error) Atoi(@string s)
        {
            const @string fnAtoi = "Atoi";



            var sLen = len(s);
            if (intSize == 32L && (0L < sLen && sLen < 10L) || intSize == 64L && (0L < sLen && sLen < 19L))
            { 
                // Fast path for small integers that fit int type.
                var s0 = s;
                if (s[0L] == '-' || s[0L] == '+')
                {
                    s = s[1L..];
                    if (len(s) < 1L)
                    {
                        return (0L, ref new NumError(fnAtoi,s0,ErrSyntax));
                    }
                }
                long n = 0L;
                foreach (var (_, ch) in (slice<byte>)s)
                {
                    ch -= '0';
                    if (ch > 9L)
                    {
                        return (0L, ref new NumError(fnAtoi,s0,ErrSyntax));
                    }
                    n = n * 10L + int(ch);
                }
                if (s0[0L] == '-')
                {
                    n = -n;
                }
                return (n, null);
            } 

            // Slow path for invalid or big integers.
            var (i64, err) = ParseInt(s, 10L, 0L);
            {
                ref NumError (nerr, ok) = err._<ref NumError>();

                if (ok)
                {
                    nerr.Func = fnAtoi;
                }

            }
            return (int(i64), err);
        }
    }
}
