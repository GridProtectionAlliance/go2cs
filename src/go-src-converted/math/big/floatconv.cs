// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements string-to-Float conversion functions.

// package big -- go2cs converted at 2020 August 29 08:29:08 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Go\src\math\big\floatconv.go
using fmt = go.fmt_package;
using io = go.io_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace math
{
    public static partial class big_package
    {
        private static Float floatZero = default;

        // SetString sets z to the value of s and returns z and a boolean indicating
        // success. s must be a floating-point number of the same format as accepted
        // by Parse, with base argument 0. The entire string (not just a prefix) must
        // be valid for success. If the operation failed, the value of z is undefined
        // but the returned value is nil.
        private static (ref Float, bool) SetString(this ref Float z, @string s)
        {
            {
                var (f, _, err) = z.Parse(s, 0L);

                if (err == null)
                {
                    return (f, true);
                }

            }
            return (null, false);
        }

        // scan is like Parse but reads the longest possible prefix representing a valid
        // floating point number from an io.ByteScanner rather than a string. It serves
        // as the implementation of Parse. It does not recognize ±Inf and does not expect
        // EOF at the end.
        private static (ref Float, long, error) scan(this ref Float _z, io.ByteScanner r, long @base) => func(_z, (ref Float z, Defer _, Panic panic, Recover __) =>
        {
            var prec = z.prec;
            if (prec == 0L)
            {
                prec = 64L;
            } 

            // A reasonable value in case of an error.
            z.form = zero; 

            // sign
            z.neg, err = scanSign(r);
            if (err != null)
            {
                return;
            } 

            // mantissa
            long fcount = default; // fractional digit count; valid if <= 0
            z.mant, b, fcount, err = z.mant.scan(r, base, true);
            if (err != null)
            {
                return;
            } 

            // exponent
            long exp = default;
            long ebase = default;
            exp, ebase, err = scanExponent(r, true);
            if (err != null)
            {
                return;
            } 

            // special-case 0
            if (len(z.mant) == 0L)
            {
                z.prec = prec;
                z.acc = Exact;
                z.form = zero;
                f = z;
                return;
            } 
            // len(z.mant) > 0

            // The mantissa may have a decimal point (fcount <= 0) and there
            // may be a nonzero exponent exp. The decimal point amounts to a
            // division by b**(-fcount). An exponent means multiplication by
            // ebase**exp. Finally, mantissa normalization (shift left) requires
            // a correcting multiplication by 2**(-shiftcount). Multiplications
            // are commutative, so we can apply them in any order as long as there
            // is no loss of precision. We only have powers of 2 and 10, and
            // we split powers of 10 into the product of the same powers of
            // 2 and 5. This reduces the size of the multiplication factor
            // needed for base-10 exponents.

            // normalize mantissa and determine initial exponent contributions
            var exp2 = int64(len(z.mant)) * _W - fnorm(z.mant);
            var exp5 = int64(0L); 

            // determine binary or decimal exponent contribution of decimal point
            if (fcount < 0L)
            { 
                // The mantissa has a "decimal" point ddd.dddd; and
                // -fcount is the number of digits to the right of '.'.
                // Adjust relevant exponent accordingly.
                var d = int64(fcount);

                if (b == 10L)
                {
                    exp5 = d;
                    fallthrough = true; // 10**e == 5**e * 2**e
                }
                if (fallthrough || b == 2L)
                {
                    exp2 += d;
                    goto __switch_break0;
                }
                if (b == 16L)
                {
                    exp2 += d * 4L; // hexadecimal digits are 4 bits each
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
                fallthrough = true;
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

            // apply 2**exp2
            if (MinExp <= exp2 && exp2 <= MaxExp)
            {
                z.prec = prec;
                z.form = finite;
                z.exp = int32(exp2);
                f = z;
            }
            else
            {
                err = fmt.Errorf("exponent overflow");
                return;
            }
            if (exp5 == 0L)
            { 
                // no decimal exponent contribution
                z.round(0L);
                return;
            } 
            // exp5 != 0

            // apply 5**exp5
            ptr<Float> p = @new<Float>().SetPrec(z.Prec() + 64L); // use more bits for p -- TODO(gri) what is the right number?
            if (exp5 < 0L)
            {
                z.Quo(z, p.pow5(uint64(-exp5)));
            }
            else
            {
                z.Mul(z, p.pow5(uint64(exp5)));
            }
            return;
        });

        // These powers of 5 fit into a uint64.
        //
        //    for p, q := uint64(0), uint64(1); p < q; p, q = q, q*5 {
        //        fmt.Println(q)
        //    }
        //
        private static array<ulong> pow5tab = new array<ulong>(new ulong[] { 1, 5, 25, 125, 625, 3125, 15625, 78125, 390625, 1953125, 9765625, 48828125, 244140625, 1220703125, 6103515625, 30517578125, 152587890625, 762939453125, 3814697265625, 19073486328125, 95367431640625, 476837158203125, 2384185791015625, 11920928955078125, 59604644775390625, 298023223876953125, 1490116119384765625, 7450580596923828125 });

        // pow5 sets z to 5**n and returns z.
        // n must not be negative.
        private static ref Float pow5(this ref Float z, ulong n)
        {
            const var m = uint64(len(pow5tab) - 1L);

            if (n <= m)
            {
                return z.SetUint64(pow5tab[n]);
            } 
            // n > m
            z.SetUint64(pow5tab[m]);
            n -= m; 

            // use more bits for f than for z
            // TODO(gri) what is the right number?
            ptr<Float> f = @new<Float>().SetPrec(z.Prec() + 64L).SetUint64(5L);

            while (n > 0L)
            {
                if (n & 1L != 0L)
                {
                    z.Mul(z, f);
                }
                f.Mul(f, f);
                n >>= 1L;
            }


            return z;
        }

        // Parse parses s which must contain a text representation of a floating-
        // point number with a mantissa in the given conversion base (the exponent
        // is always a decimal number), or a string representing an infinite value.
        //
        // It sets z to the (possibly rounded) value of the corresponding floating-
        // point value, and returns z, the actual base b, and an error err, if any.
        // The entire string (not just a prefix) must be consumed for success.
        // If z's precision is 0, it is changed to 64 before rounding takes effect.
        // The number must be of the form:
        //
        //    number   = [ sign ] [ prefix ] mantissa [ exponent ] | infinity .
        //    sign     = "+" | "-" .
        //    prefix   = "0" ( "x" | "X" | "b" | "B" ) .
        //    mantissa = digits | digits "." [ digits ] | "." digits .
        //    exponent = ( "E" | "e" | "p" ) [ sign ] digits .
        //    digits   = digit { digit } .
        //    digit    = "0" ... "9" | "a" ... "z" | "A" ... "Z" .
        //    infinity = [ sign ] ( "inf" | "Inf" ) .
        //
        // The base argument must be 0, 2, 10, or 16. Providing an invalid base
        // argument will lead to a run-time panic.
        //
        // For base 0, the number prefix determines the actual base: A prefix of
        // "0x" or "0X" selects base 16, and a "0b" or "0B" prefix selects
        // base 2; otherwise, the actual base is 10 and no prefix is accepted.
        // The octal prefix "0" is not supported (a leading "0" is simply
        // considered a "0").
        //
        // A "p" exponent indicates a binary (rather then decimal) exponent;
        // for instance "0x1.fffffffffffffp1023" (using base 0) represents the
        // maximum float64 value. For hexadecimal mantissae, the exponent must
        // be binary, if present (an "e" or "E" exponent indicator cannot be
        // distinguished from a mantissa digit).
        //
        // The returned *Float f is nil and the value of z is valid but not
        // defined if an error is reported.
        //
        private static (ref Float, long, error) Parse(this ref Float z, @string s, long @base)
        { 
            // scan doesn't handle ±Inf
            if (len(s) == 3L && (s == "Inf" || s == "inf"))
            {
                f = z.SetInf(false);
                return;
            }
            if (len(s) == 4L && (s[0L] == '+' || s[0L] == '-') && (s[1L..] == "Inf" || s[1L..] == "inf"))
            {
                f = z.SetInf(s[0L] == '-');
                return;
            }
            var r = strings.NewReader(s);
            f, b, err = z.scan(r, base);

            if (err != null)
            {
                return;
            } 

            // entire string must have been consumed
            {
                var (ch, err2) = r.ReadByte();

                if (err2 == null)
                {
                    err = fmt.Errorf("expected end of string, found %q", ch);
                }
                else if (err2 != io.EOF)
                {
                    err = err2;
                }

            }

            return;
        }

        // ParseFloat is like f.Parse(s, base) with f set to the given precision
        // and rounding mode.
        public static (ref Float, long, error) ParseFloat(@string s, long @base, ulong prec, RoundingMode mode)
        {
            return @new<Float>().SetPrec(prec).SetMode(mode).Parse(s, base);
        }

        private static fmt.Scanner _ = ref floatZero; // *Float must implement fmt.Scanner

        // Scan is a support routine for fmt.Scanner; it sets z to the value of
        // the scanned number. It accepts formats whose verbs are supported by
        // fmt.Scan for floating point values, which are:
        // 'b' (binary), 'e', 'E', 'f', 'F', 'g' and 'G'.
        // Scan doesn't handle ±Inf.
        private static error Scan(this ref Float z, fmt.ScanState s, int ch)
        {
            s.SkipSpace();
            var (_, _, err) = z.scan(new byteReader(s), 0L);
            return error.As(err);
        }
    }
}}
