// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements rat-to-string conversion functions.

// package big -- go2cs converted at 2020 October 08 03:25:52 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Go\src\math\big\ratconv.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace math
{
    public static partial class big_package
    {
        private static bool ratTok(int ch)
        {
            return strings.ContainsRune("+-/0123456789.eE", ch);
        }

        private static Rat ratZero = default;
        private static fmt.Scanner _ = _addr_ratZero; // *Rat must implement fmt.Scanner

        // Scan is a support routine for fmt.Scanner. It accepts the formats
        // 'e', 'E', 'f', 'F', 'g', 'G', and 'v'. All formats are equivalent.
        private static error Scan(this ptr<Rat> _addr_z, fmt.ScanState s, int ch)
        {
            ref Rat z = ref _addr_z.val;

            var (tok, err) = s.Token(true, ratTok);
            if (err != null)
            {
                return error.As(err)!;
            }

            if (!strings.ContainsRune("efgEFGv", ch))
            {
                return error.As(errors.New("Rat.Scan: invalid verb"))!;
            }

            {
                var (_, ok) = z.SetString(string(tok));

                if (!ok)
                {
                    return error.As(errors.New("Rat.Scan: invalid syntax"))!;
                }

            }

            return error.As(null!)!;

        }

        // SetString sets z to the value of s and returns z and a boolean indicating
        // success. s can be given as a (possibly signed) fraction "a/b", or as a
        // floating-point number optionally followed by an exponent.
        // If a fraction is provided, both the dividend and the divisor may be a
        // decimal integer or independently use a prefix of ``0b'', ``0'' or ``0o'',
        // or ``0x'' (or their upper-case variants) to denote a binary, octal, or
        // hexadecimal integer, respectively. The divisor may not be signed.
        // If a floating-point number is provided, it may be in decimal form or
        // use any of the same prefixes as above but for ``0'' to denote a non-decimal
        // mantissa. A leading ``0'' is considered a decimal leading 0; it does not
        // indicate octal representation in this case.
        // An optional base-10 ``e'' or base-2 ``p'' (or their upper-case variants)
        // exponent may be provided as well, except for hexadecimal floats which
        // only accept an (optional) ``p'' exponent (because an ``e'' or ``E'' cannot
        // be distinguished from a mantissa digit).
        // The entire string, not just a prefix, must be valid for success. If the
        // operation failed, the value of z is undefined but the returned value is nil.
        private static (ptr<Rat>, bool) SetString(this ptr<Rat> _addr_z, @string s) => func((_, panic, __) =>
        {
            ptr<Rat> _p0 = default!;
            bool _p0 = default;
            ref Rat z = ref _addr_z.val;

            if (len(s) == 0L)
            {
                return (_addr_null!, false);
            } 
            // len(s) > 0

            // parse fraction a/b, if any
            {
                var sep = strings.Index(s, "/");

                if (sep >= 0L)
                {
                    {
                        var (_, ok) = z.a.SetString(s[..sep], 0L);

                        if (!ok)
                        {
                            return (_addr_null!, false);
                        }

                    }

                    var r = strings.NewReader(s[sep + 1L..]);
                    error err = default!;
                    z.b.abs, _, _, err = z.b.abs.scan(r, 0L, false);

                    if (err != null)
                    {
                        return (_addr_null!, false);
                    } 
                    // entire string must have been consumed
                    _, err = r.ReadByte();

                    if (err != io.EOF)
                    {
                        return (_addr_null!, false);
                    }

                    if (len(z.b.abs) == 0L)
                    {
                        return (_addr_null!, false);
                    }

                    return (_addr_z.norm()!, true);

                } 

                // parse floating-point number

            } 

            // parse floating-point number
            r = strings.NewReader(s); 

            // sign
            var (neg, err) = scanSign(r);
            if (err != null)
            {
                return (_addr_null!, false);
            } 

            // mantissa
            long @base = default;
            long fcount = default; // fractional digit count; valid if <= 0
            z.a.abs, base, fcount, err = z.a.abs.scan(r, 0L, true);
            if (err != null)
            {
                return (_addr_null!, false);
            } 

            // exponent
            long exp = default;
            long ebase = default;
            exp, ebase, err = scanExponent(r, true, true);
            if (err != null)
            {
                return (_addr_null!, false);
            } 

            // there should be no unread characters left
            _, err = r.ReadByte();

            if (err != io.EOF)
            {
                return (_addr_null!, false);
            } 

            // special-case 0 (see also issue #16176)
            if (len(z.a.abs) == 0L)
            {
                return (_addr_z!, true);
            } 
            // len(z.a.abs) > 0

            // The mantissa may have a radix point (fcount <= 0) and there
            // may be a nonzero exponent exp. The radix point amounts to a
            // division by base**(-fcount), which equals a multiplication by
            // base**fcount. An exponent means multiplication by ebase**exp.
            // Multiplications are commutative, so we can apply them in any
            // order. We only have powers of 2 and 10, and we split powers
            // of 10 into the product of the same powers of 2 and 5. This
            // may reduce the size of shift/multiplication factors or
            // divisors required to create the final fraction, depending
            // on the actual floating-point value.

            // determine binary or decimal exponent contribution of radix point
            long exp2 = default;            long exp5 = default;

            if (fcount < 0L)
            { 
                // The mantissa has a radix point ddd.dddd; and
                // -fcount is the number of digits to the right
                // of '.'. Adjust relevant exponent accordingly.
                var d = int64(fcount);

                if (base == 10L)
                {
                    exp5 = d;
                    fallthrough = true; // 10**e == 5**e * 2**e
                }
                if (fallthrough || base == 2L)
                {
                    exp2 = d;
                    goto __switch_break0;
                }
                if (base == 8L)
                {
                    exp2 = d * 3L; // octal digits are 3 bits each
                    goto __switch_break0;
                }
                if (base == 16L)
                {
                    exp2 = d * 4L; // hexadecimal digits are 4 bits each
                    goto __switch_break0;
                }
                // default: 
                    panic("unexpected mantissa base");

                __switch_break0:; 
                // fcount consumed - not needed anymore
            } 

            // take actual exponent into account

            if (ebase == 10L)
            {
                exp5 += exp;
                fallthrough = true; // see fallthrough above
            }
            if (fallthrough || ebase == 2L)
            {
                exp2 += exp;
                goto __switch_break1;
            }
            // default: 
                panic("unexpected exponent base");

            __switch_break1:; 
            // exp consumed - not needed anymore

            // apply exp5 contributions
            // (start with exp5 so the numbers to multiply are smaller)
            if (exp5 != 0L)
            {
                var n = exp5;
                if (n < 0L)
                {
                    n = -n;
                }

                var pow5 = z.b.abs.expNN(natFive, nat(null).setWord(Word(n)), null); // use underlying array of z.b.abs
                if (exp5 > 0L)
                {
                    z.a.abs = z.a.abs.mul(z.a.abs, pow5);
                    z.b.abs = z.b.abs.setWord(1L);
                }
                else
                {
                    z.b.abs = pow5;
                }

            }
            else
            {
                z.b.abs = z.b.abs.setWord(1L);
            } 

            // apply exp2 contributions
            if (exp2 > 0L)
            {
                if (int64(uint(exp2)) != exp2)
                {
                    panic("exponent too large");
                }

                z.a.abs = z.a.abs.shl(z.a.abs, uint(exp2));

            }
            else if (exp2 < 0L)
            {
                if (int64(uint(-exp2)) != -exp2)
                {
                    panic("exponent too large");
                }

                z.b.abs = z.b.abs.shl(z.b.abs, uint(-exp2));

            }

            z.a.neg = neg && len(z.a.abs) > 0L; // 0 has no sign

            return (_addr_z.norm()!, true);

        });

        // scanExponent scans the longest possible prefix of r representing a base 10
        // (``e'', ``E'') or a base 2 (``p'', ``P'') exponent, if any. It returns the
        // exponent, the exponent base (10 or 2), or a read or syntax error, if any.
        //
        // If sepOk is set, an underscore character ``_'' may appear between successive
        // exponent digits; such underscores do not change the value of the exponent.
        // Incorrect placement of underscores is reported as an error if there are no
        // other errors. If sepOk is not set, underscores are not recognized and thus
        // terminate scanning like any other character that is not a valid digit.
        //
        //    exponent = ( "e" | "E" | "p" | "P" ) [ sign ] digits .
        //    sign     = "+" | "-" .
        //    digits   = digit { [ '_' ] digit } .
        //    digit    = "0" ... "9" .
        //
        // A base 2 exponent is only permitted if base2ok is set.
        private static (long, long, error) scanExponent(io.ByteScanner r, bool base2ok, bool sepOk)
        {
            long exp = default;
            long @base = default;
            error err = default!;
 
            // one char look-ahead
            var (ch, err) = r.ReadByte();
            if (err != null)
            {
                if (err == io.EOF)
                {
                    err = null;
                }

                return (0L, 10L, error.As(err)!);

            } 

            // exponent char

            if (ch == 'e' || ch == 'E')
            {
                base = 10L;
                goto __switch_break2;
            }
            if (ch == 'p' || ch == 'P')
            {
                if (base2ok)
                {
                    base = 2L;
                    break; // ok
                }

            }
            // default: 
                r.UnreadByte(); // ch does not belong to exponent anymore
                return (0L, 10L, error.As(null!)!);

            __switch_break2:; 

            // sign
            slice<byte> digits = default;
            ch, err = r.ReadByte();
            if (err == null && (ch == '+' || ch == '-'))
            {
                if (ch == '-')
                {
                    digits = append(digits, '-');
                }

                ch, err = r.ReadByte();

            } 

            // prev encodes the previously seen char: it is one
            // of '_', '0' (a digit), or '.' (anything else). A
            // valid separator '_' may only occur after a digit.
            char prev = '.';
            var invalSep = false; 

            // exponent value
            var hasDigits = false;
            while (err == null)
            {
                if ('0' <= ch && ch <= '9')
                {
                    digits = append(digits, ch);
                    prev = '0';
                    hasDigits = true;
                }
                else if (ch == '_' && sepOk)
                {
                    if (prev != '0')
                    {
                        invalSep = true;
                    }

                    prev = '_';

                }
                else
                {
                    r.UnreadByte(); // ch does not belong to number anymore
                    break;

                }

                ch, err = r.ReadByte();

            }


            if (err == io.EOF)
            {
                err = null;
            }

            if (err == null && !hasDigits)
            {
                err = errNoDigits;
            }

            if (err == null)
            {
                exp, err = strconv.ParseInt(string(digits), 10L, 64L);
            } 
            // other errors take precedence over invalid separators
            if (err == null && (invalSep || prev == '_'))
            {
                err = errInvalSep;
            }

            return ;

        }

        // String returns a string representation of x in the form "a/b" (even if b == 1).
        private static @string String(this ptr<Rat> _addr_x)
        {
            ref Rat x = ref _addr_x.val;

            return string(x.marshal());
        }

        // marshal implements String returning a slice of bytes
        private static slice<byte> marshal(this ptr<Rat> _addr_x)
        {
            ref Rat x = ref _addr_x.val;

            slice<byte> buf = default;
            buf = x.a.Append(buf, 10L);
            buf = append(buf, '/');
            if (len(x.b.abs) != 0L)
            {
                buf = x.b.Append(buf, 10L);
            }
            else
            {
                buf = append(buf, '1');
            }

            return buf;

        }

        // RatString returns a string representation of x in the form "a/b" if b != 1,
        // and in the form "a" if b == 1.
        private static @string RatString(this ptr<Rat> _addr_x)
        {
            ref Rat x = ref _addr_x.val;

            if (x.IsInt())
            {
                return x.a.String();
            }

            return x.String();

        }

        // FloatString returns a string representation of x in decimal form with prec
        // digits of precision after the radix point. The last digit is rounded to
        // nearest, with halves rounded away from zero.
        private static @string FloatString(this ptr<Rat> _addr_x, long prec)
        {
            ref Rat x = ref _addr_x.val;

            slice<byte> buf = default;

            if (x.IsInt())
            {
                buf = x.a.Append(buf, 10L);
                if (prec > 0L)
                {
                    buf = append(buf, '.');
                    {
                        var i__prev1 = i;

                        for (var i = prec; i > 0L; i--)
                        {
                            buf = append(buf, '0');
                        }


                        i = i__prev1;
                    }

                }

                return string(buf);

            } 
            // x.b.abs != 0
            var (q, r) = nat(null).div(nat(null), x.a.abs, x.b.abs);

            var p = natOne;
            if (prec > 0L)
            {
                p = nat(null).expNN(natTen, nat(null).setUint64(uint64(prec)), null);
            }

            r = r.mul(r, p);
            var (r, r2) = r.div(nat(null), r, x.b.abs); 

            // see if we need to round up
            r2 = r2.add(r2, r2);
            if (x.b.abs.cmp(r2) <= 0L)
            {
                r = r.add(r, natOne);
                if (r.cmp(p) >= 0L)
                {
                    q = nat(null).add(q, natOne);
                    r = nat(null).sub(r, p);
                }

            }

            if (x.a.neg)
            {
                buf = append(buf, '-');
            }

            buf = append(buf, q.utoa(10L)); // itoa ignores sign if q == 0

            if (prec > 0L)
            {
                buf = append(buf, '.');
                var rs = r.utoa(10L);
                {
                    var i__prev1 = i;

                    for (i = prec - len(rs); i > 0L; i--)
                    {
                        buf = append(buf, '0');
                    }


                    i = i__prev1;
                }
                buf = append(buf, rs);

            }

            return string(buf);

        }
    }
}}
