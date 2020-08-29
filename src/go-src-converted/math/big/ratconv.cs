// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements rat-to-string conversion functions.

// package big -- go2cs converted at 2020 August 29 08:29:32 UTC
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
        private static fmt.Scanner _ = ref ratZero; // *Rat must implement fmt.Scanner

        // Scan is a support routine for fmt.Scanner. It accepts the formats
        // 'e', 'E', 'f', 'F', 'g', 'G', and 'v'. All formats are equivalent.
        private static error Scan(this ref Rat z, fmt.ScanState s, int ch)
        {
            var (tok, err) = s.Token(true, ratTok);
            if (err != null)
            {
                return error.As(err);
            }
            if (!strings.ContainsRune("efgEFGv", ch))
            {
                return error.As(errors.New("Rat.Scan: invalid verb"));
            }
            {
                var (_, ok) = z.SetString(string(tok));

                if (!ok)
                {
                    return error.As(errors.New("Rat.Scan: invalid syntax"));
                }

            }
            return error.As(null);
        }

        // SetString sets z to the value of s and returns z and a boolean indicating
        // success. s can be given as a fraction "a/b" or as a floating-point number
        // optionally followed by an exponent. The entire string (not just a prefix)
        // must be valid for success. If the operation failed, the value of z is
        // undefined but the returned value is nil.
        private static (ref Rat, bool) SetString(this ref Rat z, @string s)
        {
            if (len(s) == 0L)
            {
                return (null, false);
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
                            return (null, false);
                        }

                    }
                    var r = strings.NewReader(s[sep + 1L..]);
                    error err = default;
                    z.b.abs, _, _, err = z.b.abs.scan(r, 0L, false);

                    if (err != null)
                    {
                        return (null, false);
                    } 
                    // entire string must have been consumed
                    _, err = r.ReadByte();

                    if (err != io.EOF)
                    {
                        return (null, false);
                    }
                    if (len(z.b.abs) == 0L)
                    {
                        return (null, false);
                    }
                    return (z.norm(), true);
                } 

                // parse floating-point number

            } 

            // parse floating-point number
            r = strings.NewReader(s); 

            // sign
            var (neg, err) = scanSign(r);
            if (err != null)
            {
                return (null, false);
            } 

            // mantissa
            long ecorr = default;
            z.a.abs, _, ecorr, err = z.a.abs.scan(r, 10L, true);
            if (err != null)
            {
                return (null, false);
            } 

            // exponent
            long exp = default;
            exp, _, err = scanExponent(r, false);
            if (err != null)
            {
                return (null, false);
            } 

            // there should be no unread characters left
            _, err = r.ReadByte();

            if (err != io.EOF)
            {
                return (null, false);
            } 

            // special-case 0 (see also issue #16176)
            if (len(z.a.abs) == 0L)
            {
                return (z, true);
            } 
            // len(z.a.abs) > 0

            // correct exponent
            if (ecorr < 0L)
            {
                exp += int64(ecorr);
            } 

            // compute exponent power
            var expabs = exp;
            if (expabs < 0L)
            {
                expabs = -expabs;
            }
            var powTen = nat(null).expNN(natTen, nat(null).setWord(Word(expabs)), null); 

            // complete fraction
            if (exp < 0L)
            {
                z.b.abs = powTen;
                z.norm();
            }
            else
            {
                z.a.abs = z.a.abs.mul(z.a.abs, powTen);
                z.b.abs = z.b.abs[..0L];
            }
            z.a.neg = neg && len(z.a.abs) > 0L; // 0 has no sign

            return (z, true);
        }

        // scanExponent scans the longest possible prefix of r representing a decimal
        // ('e', 'E') or binary ('p') exponent, if any. It returns the exponent, the
        // exponent base (10 or 2), or a read or syntax error, if any.
        //
        //    exponent = ( "E" | "e" | "p" ) [ sign ] digits .
        //    sign     = "+" | "-" .
        //    digits   = digit { digit } .
        //    digit    = "0" ... "9" .
        //
        // A binary exponent is only permitted if binExpOk is set.
        private static (long, long, error) scanExponent(io.ByteScanner r, bool binExpOk)
        {
            base = 10L;

            byte ch = default;
            ch, err = r.ReadByte();

            if (err != null)
            {
                if (err == io.EOF)
                {
                    err = null; // no exponent; same as e0
                }
                return;
            }

            if (ch == 'e' || ch == 'E')
            {
                goto __switch_break0;
            }
            if (ch == 'p')
            {
                if (binExpOk)
                {
                    base = 2L;
                    break; // ok
                }
            }
            // default: 
                r.UnreadByte();
                return; // no exponent; same as e0

            __switch_break0:;

            bool neg = default;
            neg, err = scanSign(r);

            if (err != null)
            {
                return;
            }
            slice<byte> digits = default;
            if (neg)
            {
                digits = append(digits, '-');
            } 

            // no need to use nat.scan for exponent digits
            // since we only care about int64 values - the
            // from-scratch scan is easy enough and faster
            for (long i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
            {
                ch, err = r.ReadByte();

                if (err != null)
                {
                    if (err != io.EOF || i == 0L)
                    {
                        return;
                    }
                    err = null;
                    break; // i > 0
                }
                if (ch < '0' || '9' < ch)
                {
                    if (i == 0L)
                    {
                        r.UnreadByte();
                        err = fmt.Errorf("invalid exponent (missing digits)");
                        return;
                    }
                    break; // i > 0
                }
                digits = append(digits, ch);
            } 
            // i > 0 => we have at least one digit
 
            // i > 0 => we have at least one digit

            exp, err = strconv.ParseInt(string(digits), 10L, 64L);
            return;
        }

        // String returns a string representation of x in the form "a/b" (even if b == 1).
        private static @string String(this ref Rat x)
        {
            return string(x.marshal());
        }

        // marshal implements String returning a slice of bytes
        private static slice<byte> marshal(this ref Rat x)
        {
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
        private static @string RatString(this ref Rat x)
        {
            if (x.IsInt())
            {
                return x.a.String();
            }
            return x.String();
        }

        // FloatString returns a string representation of x in decimal form with prec
        // digits of precision after the decimal point. The last digit is rounded to
        // nearest, with halves rounded away from zero.
        private static @string FloatString(this ref Rat x, long prec)
        {
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
