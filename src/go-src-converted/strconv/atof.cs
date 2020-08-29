// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package strconv -- go2cs converted at 2020 August 29 08:42:48 UTC
// import "strconv" ==> using strconv = go.strconv_package
// Original source: C:\Go\src\strconv\atof.go
// decimal to binary floating point conversion.
// Algorithm:
//   1) Store input in multiprecision decimal.
//   2) Multiply/divide decimal by powers of two until in range [0.5, 1)
//   3) Multiply by 2^precision and round to get mantissa.

using math = go.math_package;
using static go.builtin;

namespace go
{
    public static partial class strconv_package
    {
        private static var optimize = true; // can change for testing

        private static bool equalIgnoreCase(@string s1, @string s2)
        {
            if (len(s1) != len(s2))
            {
                return false;
            }
            for (long i = 0L; i < len(s1); i++)
            {
                var c1 = s1[i];
                if ('A' <= c1 && c1 <= 'Z')
                {
                    c1 += 'a' - 'A';
                }
                var c2 = s2[i];
                if ('A' <= c2 && c2 <= 'Z')
                {
                    c2 += 'a' - 'A';
                }
                if (c1 != c2)
                {
                    return false;
                }
            }

            return true;
        }

        private static (double, bool) special(@string s)
        {
            if (len(s) == 0L)
            {
                return;
            }
            switch (s[0L])
            {
                case '+': 
                    if (equalIgnoreCase(s, "+inf") || equalIgnoreCase(s, "+infinity"))
                    {
                        return (math.Inf(1L), true);
                    }
                    break;
                case '-': 
                    if (equalIgnoreCase(s, "-inf") || equalIgnoreCase(s, "-infinity"))
                    {
                        return (math.Inf(-1L), true);
                    }
                    break;
                case 'n': 

                case 'N': 
                    if (equalIgnoreCase(s, "nan"))
                    {
                        return (math.NaN(), true);
                    }
                    break;
                case 'i': 

                case 'I': 
                    if (equalIgnoreCase(s, "inf") || equalIgnoreCase(s, "infinity"))
                    {
                        return (math.Inf(1L), true);
                    }
                    break;
                default: 
                    return;
                    break;
            }
            return;
        }

        private static bool set(this ref decimal b, @string s)
        {
            long i = 0L;
            b.neg = false;
            b.trunc = false; 

            // optional sign
            if (i >= len(s))
            {
                return;
            }

            if (s[i] == '+') 
                i++;
            else if (s[i] == '-') 
                b.neg = true;
                i++;
            // digits
            var sawdot = false;
            var sawdigits = false;
            while (i < len(s))
            {

                if (s[i] == '.') 
                    if (sawdot)
                    {
                        return;
                i++;
                    }
                    sawdot = true;
                    b.dp = b.nd;
                    continue;
                else if ('0' <= s[i] && s[i] <= '9') 
                    sawdigits = true;
                    if (s[i] == '0' && b.nd == 0L)
                    { // ignore leading zeros
                        b.dp--;
                        continue;
                    }
                    if (b.nd < len(b.d))
                    {
                        b.d[b.nd] = s[i];
                        b.nd++;
                    }
                    else if (s[i] != '0')
                    {
                        b.trunc = true;
                    }
                    continue;
                                break;
            }

            if (!sawdigits)
            {
                return;
            }
            if (!sawdot)
            {
                b.dp = b.nd;
            } 

            // optional exponent moves decimal point.
            // if we read a very large, very long number,
            // just be sure to move the decimal point by
            // a lot (say, 100000).  it doesn't matter if it's
            // not the exact number.
            if (i < len(s) && (s[i] == 'e' || s[i] == 'E'))
            {
                i++;
                if (i >= len(s))
                {
                    return;
                }
                long esign = 1L;
                if (s[i] == '+')
                {
                    i++;
                }
                else if (s[i] == '-')
                {
                    i++;
                    esign = -1L;
                }
                if (i >= len(s) || s[i] < '0' || s[i] > '9')
                {
                    return;
                }
                long e = 0L;
                while (i < len(s) && '0' <= s[i] && s[i] <= '9')
                {
                    if (e < 10000L)
                    {
                        e = e * 10L + int(s[i]) - '0';
                    i++;
                    }
                }

                b.dp += e * esign;
            }
            if (i != len(s))
            {
                return;
            }
            ok = true;
            return;
        }

        // readFloat reads a decimal mantissa and exponent from a float
        // string representation. It sets ok to false if the number could
        // not fit return types or is invalid.
        private static (ulong, long, bool, bool, bool) readFloat(@string s)
        {
            const long uint64digits = 19L;

            long i = 0L; 

            // optional sign
            if (i >= len(s))
            {
                return;
            }

            if (s[i] == '+') 
                i++;
            else if (s[i] == '-') 
                neg = true;
                i++;
            // digits
            var sawdot = false;
            var sawdigits = false;
            long nd = 0L;
            long ndMant = 0L;
            long dp = 0L;
            while (i < len(s))
            {
                {
                    var c = s[i];


                    if (true == c == '.') 
                        if (sawdot)
                        {
                            return;
                i++;
                        }
                        sawdot = true;
                        dp = nd;
                        continue;
                    else if (true == '0' <= c && c <= '9') 
                        sawdigits = true;
                        if (c == '0' && nd == 0L)
                        { // ignore leading zeros
                            dp--;
                            continue;
                        }
                        nd++;
                        if (ndMant < uint64digits)
                        {
                            mantissa *= 10L;
                            mantissa += uint64(c - '0');
                            ndMant++;
                        }
                        else if (s[i] != '0')
                        {
                            trunc = true;
                        }
                        continue;

                }
                break;
            }

            if (!sawdigits)
            {
                return;
            }
            if (!sawdot)
            {
                dp = nd;
            } 

            // optional exponent moves decimal point.
            // if we read a very large, very long number,
            // just be sure to move the decimal point by
            // a lot (say, 100000).  it doesn't matter if it's
            // not the exact number.
            if (i < len(s) && (s[i] == 'e' || s[i] == 'E'))
            {
                i++;
                if (i >= len(s))
                {
                    return;
                }
                long esign = 1L;
                if (s[i] == '+')
                {
                    i++;
                }
                else if (s[i] == '-')
                {
                    i++;
                    esign = -1L;
                }
                if (i >= len(s) || s[i] < '0' || s[i] > '9')
                {
                    return;
                }
                long e = 0L;
                while (i < len(s) && '0' <= s[i] && s[i] <= '9')
                {
                    if (e < 10000L)
                    {
                        e = e * 10L + int(s[i]) - '0';
                    i++;
                    }
                }

                dp += e * esign;
            }
            if (i != len(s))
            {
                return;
            }
            if (mantissa != 0L)
            {
                exp = dp - ndMant;
            }
            ok = true;
            return;

        }

        // decimal power of ten to binary power of two.
        private static long powtab = new slice<long>(new long[] { 1, 3, 6, 9, 13, 16, 19, 23, 26 });

        private static (ulong, bool) floatBits(this ref decimal d, ref floatInfo flt)
        {
            long exp = default;
            ulong mant = default; 

            // Zero is always a special case.
            if (d.nd == 0L)
            {
                mant = 0L;
                exp = flt.bias;
                goto @out;
            } 

            // Obvious overflow/underflow.
            // These bounds are for 64-bit floats.
            // Will have to change if we want to support 80-bit floats in the future.
            if (d.dp > 310L)
            {
                goto overflow;
            }
            if (d.dp < -330L)
            { 
                // zero
                mant = 0L;
                exp = flt.bias;
                goto @out;
            } 

            // Scale by powers of two until in range [0.5, 1.0)
            exp = 0L;
            while (d.dp > 0L)
            {
                long n = default;
                if (d.dp >= len(powtab))
                {
                    n = 27L;
                }
                else
                {
                    n = powtab[d.dp];
                }
                d.Shift(-n);
                exp += n;
            }

            while (d.dp < 0L || d.dp == 0L && d.d[0L] < '5')
            {
                n = default;
                if (-d.dp >= len(powtab))
                {
                    n = 27L;
                }
                else
                {
                    n = powtab[-d.dp];
                }
                d.Shift(n);
                exp -= n;
            } 

            // Our range is [0.5,1) but floating point range is [1,2).
 

            // Our range is [0.5,1) but floating point range is [1,2).
            exp--; 

            // Minimum representable exponent is flt.bias+1.
            // If the exponent is smaller, move it up and
            // adjust d accordingly.
            if (exp < flt.bias + 1L)
            {
                n = flt.bias + 1L - exp;
                d.Shift(-n);
                exp += n;
            }
            if (exp - flt.bias >= 1L << (int)(flt.expbits) - 1L)
            {
                goto overflow;
            } 

            // Extract 1+flt.mantbits bits.
            d.Shift(int(1L + flt.mantbits));
            mant = d.RoundedInteger(); 

            // Rounding might have added a bit; shift down.
            if (mant == 2L << (int)(flt.mantbits))
            {
                mant >>= 1L;
                exp++;
                if (exp - flt.bias >= 1L << (int)(flt.expbits) - 1L)
                {
                    goto overflow;
                }
            } 

            // Denormalized?
            if (mant & (1L << (int)(flt.mantbits)) == 0L)
            {
                exp = flt.bias;
            }
            goto @out;

overflow:
            mant = 0L;
            exp = 1L << (int)(flt.expbits) - 1L + flt.bias;
            overflow = true;

@out:
            var bits = mant & (uint64(1L) << (int)(flt.mantbits) - 1L);
            bits |= uint64((exp - flt.bias) & (1L << (int)(flt.expbits) - 1L)) << (int)(flt.mantbits);
            if (d.neg)
            {
                bits |= 1L << (int)(flt.mantbits) << (int)(flt.expbits);
            }
            return (bits, overflow);
        }

        // Exact powers of 10.
        private static double float64pow10 = new slice<double>(new double[] { 1e0, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10, 1e11, 1e12, 1e13, 1e14, 1e15, 1e16, 1e17, 1e18, 1e19, 1e20, 1e21, 1e22 });
        private static float float32pow10 = new slice<float>(new float[] { 1e0, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10 });

        // If possible to convert decimal representation to 64-bit float f exactly,
        // entirely in floating-point math, do so, avoiding the expense of decimalToFloatBits.
        // Three common cases:
        //    value is exact integer
        //    value is exact integer * exact power of ten
        //    value is exact integer / exact power of ten
        // These all produce potentially inexact but correctly rounded answers.
        private static (double, bool) atof64exact(ulong mantissa, long exp, bool neg)
        {
            if (mantissa >> (int)(float64info.mantbits) != 0L)
            {
                return;
            }
            f = float64(mantissa);
            if (neg)
            {
                f = -f;
            }

            if (exp == 0L) 
                // an integer.
                return (f, true); 
                // Exact integers are <= 10^15.
                // Exact powers of ten are <= 10^22.
            else if (exp > 0L && exp <= 15L + 22L) // int * 10^k
                // If exponent is big but number of digits is not,
                // can move a few zeros into the integer part.
                if (exp > 22L)
                {
                    f *= float64pow10[exp - 22L];
                    exp = 22L;
                }
                if (f > 1e15F || f < -1e15F)
                { 
                    // the exponent was really too large.
                    return;
                }
                return (f * float64pow10[exp], true);
            else if (exp < 0L && exp >= -22L) // int / 10^k
                return (f / float64pow10[-exp], true);
                        return;
        }

        // If possible to compute mantissa*10^exp to 32-bit float f exactly,
        // entirely in floating-point math, do so, avoiding the machinery above.
        private static (float, bool) atof32exact(ulong mantissa, long exp, bool neg)
        {
            if (mantissa >> (int)(float32info.mantbits) != 0L)
            {
                return;
            }
            f = float32(mantissa);
            if (neg)
            {
                f = -f;
            }

            if (exp == 0L) 
                return (f, true); 
                // Exact integers are <= 10^7.
                // Exact powers of ten are <= 10^10.
            else if (exp > 0L && exp <= 7L + 10L) // int * 10^k
                // If exponent is big but number of digits is not,
                // can move a few zeros into the integer part.
                if (exp > 10L)
                {
                    f *= float32pow10[exp - 10L];
                    exp = 10L;
                }
                if (f > 1e7F || f < -1e7F)
                { 
                    // the exponent was really too large.
                    return;
                }
                return (f * float32pow10[exp], true);
            else if (exp < 0L && exp >= -10L) // int / 10^k
                return (f / float32pow10[-exp], true);
                        return;
        }

        private static readonly @string fnParseFloat = "ParseFloat";



        private static (float, error) atof32(@string s)
        {
            {
                var (val, ok) = special(s);

                if (ok)
                {
                    return (float32(val), null);
                }

            }

            if (optimize)
            { 
                // Parse mantissa and exponent.
                var (mantissa, exp, neg, trunc, ok) = readFloat(s);
                if (ok)
                { 
                    // Try pure floating-point arithmetic conversion.
                    if (!trunc)
                    {
                        {
                            var (f, ok) = atof32exact(mantissa, exp, neg);

                            if (ok)
                            {
                                return (f, null);
                            }

                        }
                    } 
                    // Try another fast path.
                    ptr<object> ext = @new<extFloat>();
                    {
                        var ok = ext.AssignDecimal(mantissa, exp, neg, trunc, ref float32info);

                        if (ok)
                        {
                            var (b, ovf) = ext.floatBits(ref float32info);
                            f = math.Float32frombits(uint32(b));
                            if (ovf)
                            {
                                err = rangeError(fnParseFloat, s);
                            }
                            return (f, err);
                        }

                    }
                }
            }
            decimal d = default;
            if (!d.set(s))
            {
                return (0L, syntaxError(fnParseFloat, s));
            }
            (b, ovf) = d.floatBits(ref float32info);
            f = math.Float32frombits(uint32(b));
            if (ovf)
            {
                err = rangeError(fnParseFloat, s);
            }
            return (f, err);
        }

        private static (double, error) atof64(@string s)
        {
            {
                var (val, ok) = special(s);

                if (ok)
                {
                    return (val, null);
                }

            }

            if (optimize)
            { 
                // Parse mantissa and exponent.
                var (mantissa, exp, neg, trunc, ok) = readFloat(s);
                if (ok)
                { 
                    // Try pure floating-point arithmetic conversion.
                    if (!trunc)
                    {
                        {
                            var (f, ok) = atof64exact(mantissa, exp, neg);

                            if (ok)
                            {
                                return (f, null);
                            }

                        }
                    } 
                    // Try another fast path.
                    ptr<object> ext = @new<extFloat>();
                    {
                        var ok = ext.AssignDecimal(mantissa, exp, neg, trunc, ref float64info);

                        if (ok)
                        {
                            var (b, ovf) = ext.floatBits(ref float64info);
                            f = math.Float64frombits(b);
                            if (ovf)
                            {
                                err = rangeError(fnParseFloat, s);
                            }
                            return (f, err);
                        }

                    }
                }
            }
            decimal d = default;
            if (!d.set(s))
            {
                return (0L, syntaxError(fnParseFloat, s));
            }
            (b, ovf) = d.floatBits(ref float64info);
            f = math.Float64frombits(b);
            if (ovf)
            {
                err = rangeError(fnParseFloat, s);
            }
            return (f, err);
        }

        // ParseFloat converts the string s to a floating-point number
        // with the precision specified by bitSize: 32 for float32, or 64 for float64.
        // When bitSize=32, the result still has type float64, but it will be
        // convertible to float32 without changing its value.
        //
        // If s is well-formed and near a valid floating point number,
        // ParseFloat returns the nearest floating point number rounded
        // using IEEE754 unbiased rounding.
        //
        // The errors that ParseFloat returns have concrete type *NumError
        // and include err.Num = s.
        //
        // If s is not syntactically well-formed, ParseFloat returns err.Err = ErrSyntax.
        //
        // If s is syntactically well-formed but is more than 1/2 ULP
        // away from the largest floating point number of the given size,
        // ParseFloat returns f = ±Inf, err.Err = ErrRange.
        public static (double, error) ParseFloat(@string s, long bitSize)
        {
            if (bitSize == 32L)
            {
                var (f, err) = atof32(s);
                return (float64(f), err);
            }
            return atof64(s);
        }
    }
}
