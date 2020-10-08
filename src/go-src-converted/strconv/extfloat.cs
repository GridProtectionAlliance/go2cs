// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package strconv -- go2cs converted at 2020 October 08 03:48:54 UTC
// import "strconv" ==> using strconv = go.strconv_package
// Original source: C:\Go\src\strconv\extfloat.go
using bits = go.math.bits_package;
using static go.builtin;

namespace go
{
    public static partial class strconv_package
    {
        // An extFloat represents an extended floating-point number, with more
        // precision than a float64. It does not try to save bits: the
        // number represented by the structure is mant*(2^exp), with a negative
        // sign if neg is true.
        private partial struct extFloat
        {
            public ulong mant;
            public long exp;
            public bool neg;
        }

        // Powers of ten taken from double-conversion library.
        // https://code.google.com/p/double-conversion/
        private static readonly long firstPowerOfTen = (long)-348L;
        private static readonly long stepPowerOfTen = (long)8L;


        private static array<extFloat> smallPowersOfTen = new array<extFloat>(new extFloat[] { {1<<63,-63,false}, {0xa<<60,-60,false}, {0x64<<57,-57,false}, {0x3e8<<54,-54,false}, {0x2710<<50,-50,false}, {0x186a0<<47,-47,false}, {0xf4240<<44,-44,false}, {0x989680<<40,-40,false} });

        private static array<extFloat> powersOfTen = new array<extFloat>(new extFloat[] { {0xfa8fd5a0081c0288,-1220,false}, {0xbaaee17fa23ebf76,-1193,false}, {0x8b16fb203055ac76,-1166,false}, {0xcf42894a5dce35ea,-1140,false}, {0x9a6bb0aa55653b2d,-1113,false}, {0xe61acf033d1a45df,-1087,false}, {0xab70fe17c79ac6ca,-1060,false}, {0xff77b1fcbebcdc4f,-1034,false}, {0xbe5691ef416bd60c,-1007,false}, {0x8dd01fad907ffc3c,-980,false}, {0xd3515c2831559a83,-954,false}, {0x9d71ac8fada6c9b5,-927,false}, {0xea9c227723ee8bcb,-901,false}, {0xaecc49914078536d,-874,false}, {0x823c12795db6ce57,-847,false}, {0xc21094364dfb5637,-821,false}, {0x9096ea6f3848984f,-794,false}, {0xd77485cb25823ac7,-768,false}, {0xa086cfcd97bf97f4,-741,false}, {0xef340a98172aace5,-715,false}, {0xb23867fb2a35b28e,-688,false}, {0x84c8d4dfd2c63f3b,-661,false}, {0xc5dd44271ad3cdba,-635,false}, {0x936b9fcebb25c996,-608,false}, {0xdbac6c247d62a584,-582,false}, {0xa3ab66580d5fdaf6,-555,false}, {0xf3e2f893dec3f126,-529,false}, {0xb5b5ada8aaff80b8,-502,false}, {0x87625f056c7c4a8b,-475,false}, {0xc9bcff6034c13053,-449,false}, {0x964e858c91ba2655,-422,false}, {0xdff9772470297ebd,-396,false}, {0xa6dfbd9fb8e5b88f,-369,false}, {0xf8a95fcf88747d94,-343,false}, {0xb94470938fa89bcf,-316,false}, {0x8a08f0f8bf0f156b,-289,false}, {0xcdb02555653131b6,-263,false}, {0x993fe2c6d07b7fac,-236,false}, {0xe45c10c42a2b3b06,-210,false}, {0xaa242499697392d3,-183,false}, {0xfd87b5f28300ca0e,-157,false}, {0xbce5086492111aeb,-130,false}, {0x8cbccc096f5088cc,-103,false}, {0xd1b71758e219652c,-77,false}, {0x9c40000000000000,-50,false}, {0xe8d4a51000000000,-24,false}, {0xad78ebc5ac620000,3,false}, {0x813f3978f8940984,30,false}, {0xc097ce7bc90715b3,56,false}, {0x8f7e32ce7bea5c70,83,false}, {0xd5d238a4abe98068,109,false}, {0x9f4f2726179a2245,136,false}, {0xed63a231d4c4fb27,162,false}, {0xb0de65388cc8ada8,189,false}, {0x83c7088e1aab65db,216,false}, {0xc45d1df942711d9a,242,false}, {0x924d692ca61be758,269,false}, {0xda01ee641a708dea,295,false}, {0xa26da3999aef774a,322,false}, {0xf209787bb47d6b85,348,false}, {0xb454e4a179dd1877,375,false}, {0x865b86925b9bc5c2,402,false}, {0xc83553c5c8965d3d,428,false}, {0x952ab45cfa97a0b3,455,false}, {0xde469fbd99a05fe3,481,false}, {0xa59bc234db398c25,508,false}, {0xf6c69a72a3989f5c,534,false}, {0xb7dcbf5354e9bece,561,false}, {0x88fcf317f22241e2,588,false}, {0xcc20ce9bd35c78a5,614,false}, {0x98165af37b2153df,641,false}, {0xe2a0b5dc971f303a,667,false}, {0xa8d9d1535ce3b396,694,false}, {0xfb9b7cd9a4a7443c,720,false}, {0xbb764c4ca7a44410,747,false}, {0x8bab8eefb6409c1a,774,false}, {0xd01fef10a657842c,800,false}, {0x9b10a4e5e9913129,827,false}, {0xe7109bfba19c0c9d,853,false}, {0xac2820d9623bf429,880,false}, {0x80444b5e7aa7cf85,907,false}, {0xbf21e44003acdd2d,933,false}, {0x8e679c2f5e44ff8f,960,false}, {0xd433179d9c8cb841,986,false}, {0x9e19db92b4e31ba9,1013,false}, {0xeb96bf6ebadf77d9,1039,false}, {0xaf87023b9bf0ee6b,1066,false} });

        // floatBits returns the bits of the float64 that best approximates
        // the extFloat passed as receiver. Overflow is set to true if
        // the resulting float64 is ±Inf.
        private static (ulong, bool) floatBits(this ptr<extFloat> _addr_f, ptr<floatInfo> _addr_flt)
        {
            ulong bits = default;
            bool overflow = default;
            ref extFloat f = ref _addr_f.val;
            ref floatInfo flt = ref _addr_flt.val;

            f.Normalize();

            var exp = f.exp + 63L; 

            // Exponent too small.
            if (exp < flt.bias + 1L)
            {
                var n = flt.bias + 1L - exp;
                f.mant >>= uint(n);
                exp += n;
            } 

            // Extract 1+flt.mantbits bits from the 64-bit mantissa.
            var mant = f.mant >> (int)((63L - flt.mantbits));
            if (f.mant & (1L << (int)((62L - flt.mantbits))) != 0L)
            { 
                // Round up.
                mant += 1L;

            } 

            // Rounding might have added a bit; shift down.
            if (mant == 2L << (int)(flt.mantbits))
            {
                mant >>= 1L;
                exp++;
            } 

            // Infinities.
            if (exp - flt.bias >= 1L << (int)(flt.expbits) - 1L)
            { 
                // ±Inf
                mant = 0L;
                exp = 1L << (int)(flt.expbits) - 1L + flt.bias;
                overflow = true;

            }
            else if (mant & (1L << (int)(flt.mantbits)) == 0L)
            { 
                // Denormalized?
                exp = flt.bias;

            } 
            // Assemble bits.
            bits = mant & (uint64(1L) << (int)(flt.mantbits) - 1L);
            bits |= uint64((exp - flt.bias) & (1L << (int)(flt.expbits) - 1L)) << (int)(flt.mantbits);
            if (f.neg)
            {
                bits |= 1L << (int)((flt.mantbits + flt.expbits));
            }

            return ;

        }

        // AssignComputeBounds sets f to the floating point value
        // defined by mant, exp and precision given by flt. It returns
        // lower, upper such that any number in the closed interval
        // [lower, upper] is converted back to the same floating point number.
        private static (extFloat, extFloat) AssignComputeBounds(this ptr<extFloat> _addr_f, ulong mant, long exp, bool neg, ptr<floatInfo> _addr_flt)
        {
            extFloat lower = default;
            extFloat upper = default;
            ref extFloat f = ref _addr_f.val;
            ref floatInfo flt = ref _addr_flt.val;

            f.mant = mant;
            f.exp = exp - int(flt.mantbits);
            f.neg = neg;
            if (f.exp <= 0L && mant == (mant >> (int)(uint(-f.exp))) << (int)(uint(-f.exp)))
            { 
                // An exact integer
                f.mant >>= uint(-f.exp);
                f.exp = 0L;
                return (f.val, f.val);

            }

            var expBiased = exp - flt.bias;

            upper = new extFloat(mant:2*f.mant+1,exp:f.exp-1,neg:f.neg);
            if (mant != 1L << (int)(flt.mantbits) || expBiased == 1L)
            {
                lower = new extFloat(mant:2*f.mant-1,exp:f.exp-1,neg:f.neg);
            }
            else
            {
                lower = new extFloat(mant:4*f.mant-1,exp:f.exp-2,neg:f.neg);
            }

            return ;

        }

        // Normalize normalizes f so that the highest bit of the mantissa is
        // set, and returns the number by which the mantissa was left-shifted.
        private static ulong Normalize(this ptr<extFloat> _addr_f)
        {
            ref extFloat f = ref _addr_f.val;
 
            // bits.LeadingZeros64 would return 64
            if (f.mant == 0L)
            {
                return 0L;
            }

            var shift = bits.LeadingZeros64(f.mant);
            f.mant <<= uint(shift);
            f.exp -= shift;
            return uint(shift);

        }

        // Multiply sets f to the product f*g: the result is correctly rounded,
        // but not normalized.
        private static void Multiply(this ptr<extFloat> _addr_f, extFloat g)
        {
            ref extFloat f = ref _addr_f.val;

            var (hi, lo) = bits.Mul64(f.mant, g.mant); 
            // Round up.
            f.mant = hi + (lo >> (int)(63L));
            f.exp = f.exp + g.exp + 64L;

        }

        private static array<ulong> uint64pow10 = new array<ulong>(new ulong[] { 1, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10, 1e11, 1e12, 1e13, 1e14, 1e15, 1e16, 1e17, 1e18, 1e19 });

        // AssignDecimal sets f to an approximate value mantissa*10^exp. It
        // reports whether the value represented by f is guaranteed to be the
        // best approximation of d after being rounded to a float64 or
        // float32 depending on flt.
        private static bool AssignDecimal(this ptr<extFloat> _addr_f, ulong mantissa, long exp10, bool neg, bool trunc, ptr<floatInfo> _addr_flt)
        {
            bool ok = default;
            ref extFloat f = ref _addr_f.val;
            ref floatInfo flt = ref _addr_flt.val;

            const long uint64digits = (long)19L; 

            // Errors (in the "numerical approximation" sense, not the "Go's error
            // type" sense) in this function are measured as multiples of 1/8 of a ULP,
            // so that "1/2 of a ULP" can be represented in integer arithmetic.
            //
            // The C++ double-conversion library also uses this 8x scaling factor:
            // https://github.com/google/double-conversion/blob/f4cb2384/double-conversion/strtod.cc#L291
            // but this Go implementation has a bug, where it forgets to scale other
            // calculations (further below in this function) by the same number. The
            // C++ implementation does not forget:
            // https://github.com/google/double-conversion/blob/f4cb2384/double-conversion/strtod.cc#L366
            //
            // Scaling the "errors" in the "is mant_extra in the range (halfway ±
            // errors)" check, but not scaling the other values, means that we return
            // ok=false (and fall back to a slower atof code path) more often than we
            // could. This affects performance but not correctness.
            //
            // Longer term, we could fix the forgot-to-scale bug (and look carefully
            // for correctness regressions; https://codereview.appspot.com/5494068
            // landed in 2011), or replace this atof algorithm with a faster one (e.g.
            // Ryu). Shorter term, this comment will suffice.
 

            // Errors (in the "numerical approximation" sense, not the "Go's error
            // type" sense) in this function are measured as multiples of 1/8 of a ULP,
            // so that "1/2 of a ULP" can be represented in integer arithmetic.
            //
            // The C++ double-conversion library also uses this 8x scaling factor:
            // https://github.com/google/double-conversion/blob/f4cb2384/double-conversion/strtod.cc#L291
            // but this Go implementation has a bug, where it forgets to scale other
            // calculations (further below in this function) by the same number. The
            // C++ implementation does not forget:
            // https://github.com/google/double-conversion/blob/f4cb2384/double-conversion/strtod.cc#L366
            //
            // Scaling the "errors" in the "is mant_extra in the range (halfway ±
            // errors)" check, but not scaling the other values, means that we return
            // ok=false (and fall back to a slower atof code path) more often than we
            // could. This affects performance but not correctness.
            //
            // Longer term, we could fix the forgot-to-scale bug (and look carefully
            // for correctness regressions; https://codereview.appspot.com/5494068
            // landed in 2011), or replace this atof algorithm with a faster one (e.g.
            // Ryu). Shorter term, this comment will suffice.
            const long errorscale = (long)8L;



            long errors = 0L; // An upper bound for error, computed in ULP/errorscale.
            if (trunc)
            { 
                // the decimal number was truncated.
                errors += errorscale / 2L;

            }

            f.mant = mantissa;
            f.exp = 0L;
            f.neg = neg; 

            // Multiply by powers of ten.
            var i = (exp10 - firstPowerOfTen) / stepPowerOfTen;
            if (exp10 < firstPowerOfTen || i >= len(powersOfTen))
            {
                return false;
            }

            var adjExp = (exp10 - firstPowerOfTen) % stepPowerOfTen; 

            // We multiply by exp%step
            if (adjExp < uint64digits && mantissa < uint64pow10[uint64digits - adjExp])
            { 
                // We can multiply the mantissa exactly.
                f.mant *= uint64pow10[adjExp];
                f.Normalize();

            }
            else
            {
                f.Normalize();
                f.Multiply(smallPowersOfTen[adjExp]);
                errors += errorscale / 2L;
            } 

            // We multiply by 10 to the exp - exp%step.
            f.Multiply(powersOfTen[i]);
            if (errors > 0L)
            {
                errors += 1L;
            }

            errors += errorscale / 2L; 

            // Normalize
            var shift = f.Normalize();
            errors <<= shift; 

            // Now f is a good approximation of the decimal.
            // Check whether the error is too large: that is, if the mantissa
            // is perturbated by the error, the resulting float64 will change.
            // The 64 bits mantissa is 1 + 52 bits for float64 + 11 extra bits.
            //
            // In many cases the approximation will be good enough.
            var denormalExp = flt.bias - 63L;
            ulong extrabits = default;
            if (f.exp <= denormalExp)
            { 
                // f.mant * 2^f.exp is smaller than 2^(flt.bias+1).
                extrabits = 63L - flt.mantbits + 1L + uint(denormalExp - f.exp);

            }
            else
            {
                extrabits = 63L - flt.mantbits;
            }

            var halfway = uint64(1L) << (int)((extrabits - 1L));
            var mant_extra = f.mant & (1L << (int)(extrabits) - 1L); 

            // Do a signed comparison here! If the error estimate could make
            // the mantissa round differently for the conversion to double,
            // then we can't give a definite answer.
            if (int64(halfway) - int64(errors) < int64(mant_extra) && int64(mant_extra) < int64(halfway) + int64(errors))
            {
                return false;
            }

            return true;

        }

        // Frexp10 is an analogue of math.Frexp for decimal powers. It scales
        // f by an approximate power of ten 10^-exp, and returns exp10, so
        // that f*10^exp10 has the same value as the old f, up to an ulp,
        // as well as the index of 10^-exp in the powersOfTen table.
        private static (long, long) frexp10(this ptr<extFloat> _addr_f)
        {
            long exp10 = default;
            long index = default;
            ref extFloat f = ref _addr_f.val;
 
            // The constants expMin and expMax constrain the final value of the
            // binary exponent of f. We want a small integral part in the result
            // because finding digits of an integer requires divisions, whereas
            // digits of the fractional part can be found by repeatedly multiplying
            // by 10.
            const long expMin = (long)-60L;

            const long expMax = (long)-32L; 
            // Find power of ten such that x * 10^n has a binary exponent
            // between expMin and expMax.
 
            // Find power of ten such that x * 10^n has a binary exponent
            // between expMin and expMax.
            var approxExp10 = ((expMin + expMax) / 2L - f.exp) * 28L / 93L; // log(10)/log(2) is close to 93/28.
            var i = (approxExp10 - firstPowerOfTen) / stepPowerOfTen;
Loop: 
            // Apply the desired decimal shift on f. It will have exponent
            // in the desired range. This is multiplication by 10^-exp10.
            while (true)
            {
                var exp = f.exp + powersOfTen[i].exp + 64L;

                if (exp < expMin) 
                    i++;
                else if (exp > expMax) 
                    i--;
                else 
                    _breakLoop = true;
                    break;
                            } 
            // Apply the desired decimal shift on f. It will have exponent
            // in the desired range. This is multiplication by 10^-exp10.
 
            // Apply the desired decimal shift on f. It will have exponent
            // in the desired range. This is multiplication by 10^-exp10.
            f.Multiply(powersOfTen[i]);

            return (-(firstPowerOfTen + i * stepPowerOfTen), i);

        }

        // frexp10Many applies a common shift by a power of ten to a, b, c.
        private static long frexp10Many(ptr<extFloat> _addr_a, ptr<extFloat> _addr_b, ptr<extFloat> _addr_c)
        {
            long exp10 = default;
            ref extFloat a = ref _addr_a.val;
            ref extFloat b = ref _addr_b.val;
            ref extFloat c = ref _addr_c.val;

            var (exp10, i) = c.frexp10();
            a.Multiply(powersOfTen[i]);
            b.Multiply(powersOfTen[i]);
            return ;
        }

        // FixedDecimal stores in d the first n significant digits
        // of the decimal representation of f. It returns false
        // if it cannot be sure of the answer.
        private static bool FixedDecimal(this ptr<extFloat> _addr_f, ptr<decimalSlice> _addr_d, long n) => func((_, panic, __) =>
        {
            ref extFloat f = ref _addr_f.val;
            ref decimalSlice d = ref _addr_d.val;

            if (f.mant == 0L)
            {
                d.nd = 0L;
                d.dp = 0L;
                d.neg = f.neg;
                return true;
            }

            if (n == 0L)
            {
                panic("strconv: internal error: extFloat.FixedDecimal called with n == 0");
            } 
            // Multiply by an appropriate power of ten to have a reasonable
            // number to process.
            f.Normalize();
            var (exp10, _) = f.frexp10();

            var shift = uint(-f.exp);
            var integer = uint32(f.mant >> (int)(shift));
            var fraction = f.mant - (uint64(integer) << (int)(shift));
            var ε = uint64(1L); // ε is the uncertainty we have on the mantissa of f.

            // Write exactly n digits to d.
            var needed = n; // how many digits are left to write.
            long integerDigits = 0L; // the number of decimal digits of integer.
            var pow10 = uint64(1L); // the power of ten by which f was scaled.
            {
                long i__prev1 = i;

                for (long i = 0L;
                var pow = uint64(1L); i < 20L; i++)
                {
                    if (pow > uint64(integer))
                    {
                        integerDigits = i;
                        break;
                    }

                    pow *= 10L;

                }


                i = i__prev1;
            }
            var rest = integer;
            if (integerDigits > needed)
            { 
                // the integral part is already large, trim the last digits.
                pow10 = uint64pow10[integerDigits - needed];
                integer /= uint32(pow10);
                rest -= integer * uint32(pow10);

            }
            else
            {
                rest = 0L;
            } 

            // Write the digits of integer: the digits of rest are omitted.
            array<byte> buf = new array<byte>(32L);
            var pos = len(buf);
            {
                var v = integer;

                while (v > 0L)
                {
                    var v1 = v / 10L;
                    v -= 10L * v1;
                    pos--;
                    buf[pos] = byte(v + '0');
                    v = v1;
                }

            }
            {
                long i__prev1 = i;

                for (i = pos; i < len(buf); i++)
                {
                    d.d[i - pos] = buf[i];
                }


                i = i__prev1;
            }
            var nd = len(buf) - pos;
            d.nd = nd;
            d.dp = integerDigits + exp10;
            needed -= nd;

            if (needed > 0L)
            {
                if (rest != 0L || pow10 != 1L)
                {
                    panic("strconv: internal error, rest != 0 but needed > 0");
                } 
                // Emit digits for the fractional part. Each time, 10*fraction
                // fits in a uint64 without overflow.
                while (needed > 0L)
                {
                    fraction *= 10L;
                    ε *= 10L; // the uncertainty scales as we multiply by ten.
                    if (2L * ε > 1L << (int)(shift))
                    { 
                        // the error is so large it could modify which digit to write, abort.
                        return false;

                    }

                    var digit = fraction >> (int)(shift);
                    d.d[nd] = byte(digit + '0');
                    fraction -= digit << (int)(shift);
                    nd++;
                    needed--;

                }

                d.nd = nd;

            } 

            // We have written a truncation of f (a numerator / 10^d.dp). The remaining part
            // can be interpreted as a small number (< 1) to be added to the last digit of the
            // numerator.
            //
            // If rest > 0, the amount is:
            //    (rest<<shift | fraction) / (pow10 << shift)
            //    fraction being known with a ±ε uncertainty.
            //    The fact that n > 0 guarantees that pow10 << shift does not overflow a uint64.
            //
            // If rest = 0, pow10 == 1 and the amount is
            //    fraction / (1 << shift)
            //    fraction being known with a ±ε uncertainty.
            //
            // We pass this information to the rounding routine for adjustment.
            var ok = adjustLastDigitFixed(_addr_d, uint64(rest) << (int)(shift) | fraction, pow10, shift, ε);
            if (!ok)
            {
                return false;
            } 
            // Trim trailing zeros.
            {
                long i__prev1 = i;

                for (i = d.nd - 1L; i >= 0L; i--)
                {
                    if (d.d[i] != '0')
                    {
                        d.nd = i + 1L;
                        break;
                    }

                }


                i = i__prev1;
            }
            return true;

        });

        // adjustLastDigitFixed assumes d contains the representation of the integral part
        // of some number, whose fractional part is num / (den << shift). The numerator
        // num is only known up to an uncertainty of size ε, assumed to be less than
        // (den << shift)/2.
        //
        // It will increase the last digit by one to account for correct rounding, typically
        // when the fractional part is greater than 1/2, and will return false if ε is such
        // that no correct answer can be given.
        private static bool adjustLastDigitFixed(ptr<decimalSlice> _addr_d, ulong num, ulong den, ulong shift, ulong ε) => func((_, panic, __) =>
        {
            ref decimalSlice d = ref _addr_d.val;

            if (num > den << (int)(shift))
            {
                panic("strconv: num > den<<shift in adjustLastDigitFixed");
            }

            if (2L * ε > den << (int)(shift))
            {
                panic("strconv: ε > (den<<shift)/2");
            }

            if (2L * (num + ε) < den << (int)(shift))
            {
                return true;
            }

            if (2L * (num - ε) > den << (int)(shift))
            { 
                // increment d by 1.
                var i = d.nd - 1L;
                while (i >= 0L)
                {
                    if (d.d[i] == '9')
                    {
                        d.nd--;
                    i--;
                    }
                    else
                    {
                        break;
                    }

                }

                if (i < 0L)
                {
                    d.d[0L] = '1';
                    d.nd = 1L;
                    d.dp++;
                }
                else
                {
                    d.d[i]++;
                }

                return true;

            }

            return false;

        });

        // ShortestDecimal stores in d the shortest decimal representation of f
        // which belongs to the open interval (lower, upper), where f is supposed
        // to lie. It returns false whenever the result is unsure. The implementation
        // uses the Grisu3 algorithm.
        private static bool ShortestDecimal(this ptr<extFloat> _addr_f, ptr<decimalSlice> _addr_d, ptr<extFloat> _addr_lower, ptr<extFloat> _addr_upper)
        {
            ref extFloat f = ref _addr_f.val;
            ref decimalSlice d = ref _addr_d.val;
            ref extFloat lower = ref _addr_lower.val;
            ref extFloat upper = ref _addr_upper.val;

            if (f.mant == 0L)
            {
                d.nd = 0L;
                d.dp = 0L;
                d.neg = f.neg;
                return true;
            }

            if (f.exp == 0L && lower == f && lower == upper.val)
            { 
                // an exact integer.
                array<byte> buf = new array<byte>(24L);
                var n = len(buf) - 1L;
                {
                    var v = f.mant;

                    while (v > 0L)
                    {
                        var v1 = v / 10L;
                        v -= 10L * v1;
                        buf[n] = byte(v + '0');
                        n--;
                        v = v1;
                    }

                }
                var nd = len(buf) - n - 1L;
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < nd; i++)
                    {
                        d.d[i] = buf[n + 1L + i];
                    }


                    i = i__prev1;
                }
                d.nd = nd;
                d.dp = nd;
                while (d.nd > 0L && d.d[d.nd - 1L] == '0')
                {
                    d.nd--;
                }

                if (d.nd == 0L)
                {
                    d.dp = 0L;
                }

                d.neg = f.neg;
                return true;

            }

            upper.Normalize(); 
            // Uniformize exponents.
            if (f.exp > upper.exp)
            {
                f.mant <<= uint(f.exp - upper.exp);
                f.exp = upper.exp;
            }

            if (lower.exp > upper.exp)
            {
                lower.mant <<= uint(lower.exp - upper.exp);
                lower.exp = upper.exp;
            }

            var exp10 = frexp10Many(_addr_lower, _addr_f, _addr_upper); 
            // Take a safety margin due to rounding in frexp10Many, but we lose precision.
            upper.mant++;
            lower.mant--; 

            // The shortest representation of f is either rounded up or down, but
            // in any case, it is a truncation of upper.
            var shift = uint(-upper.exp);
            var integer = uint32(upper.mant >> (int)(shift));
            var fraction = upper.mant - (uint64(integer) << (int)(shift)); 

            // How far we can go down from upper until the result is wrong.
            var allowance = upper.mant - lower.mant; 
            // How far we should go to get a very precise result.
            var targetDiff = upper.mant - f.mant; 

            // Count integral digits: there are at most 10.
            long integerDigits = default;
            {
                long i__prev1 = i;
                var pow__prev1 = pow;

                for (i = 0L;
                var pow = uint64(1L); i < 20L; i++)
                {
                    if (pow > uint64(integer))
                    {
                        integerDigits = i;
                        break;
                    }

                    pow *= 10L;

                }


                i = i__prev1;
                pow = pow__prev1;
            }
            {
                long i__prev1 = i;

                for (i = 0L; i < integerDigits; i++)
                {
                    pow = uint64pow10[integerDigits - i - 1L];
                    var digit = integer / uint32(pow);
                    d.d[i] = byte(digit + '0');
                    integer -= digit * uint32(pow); 
                    // evaluate whether we should stop.
                    {
                        var currentDiff = uint64(integer) << (int)(shift) + fraction;

                        if (currentDiff < allowance)
                        {
                            d.nd = i + 1L;
                            d.dp = integerDigits + exp10;
                            d.neg = f.neg; 
                            // Sometimes allowance is so large the last digit might need to be
                            // decremented to get closer to f.
                            return adjustLastDigit(_addr_d, currentDiff, targetDiff, allowance, pow << (int)(shift), 2L);

                        }

                    }

                }


                i = i__prev1;
            }
            d.nd = integerDigits;
            d.dp = d.nd + exp10;
            d.neg = f.neg; 

            // Compute digits of the fractional part. At each step fraction does not
            // overflow. The choice of minExp implies that fraction is less than 2^60.
            digit = default;
            var multiplier = uint64(1L);
            while (true)
            {
                fraction *= 10L;
                multiplier *= 10L;
                digit = int(fraction >> (int)(shift));
                d.d[d.nd] = byte(digit + '0');
                d.nd++;
                fraction -= uint64(digit) << (int)(shift);
                if (fraction < allowance * multiplier)
                { 
                    // We are in the admissible range. Note that if allowance is about to
                    // overflow, that is, allowance > 2^64/10, the condition is automatically
                    // true due to the limited range of fraction.
                    return adjustLastDigit(_addr_d, fraction, targetDiff * multiplier, allowance * multiplier, 1L << (int)(shift), multiplier * 2L);

                }

            }


        }

        // adjustLastDigit modifies d = x-currentDiff*ε, to get closest to
        // d = x-targetDiff*ε, without becoming smaller than x-maxDiff*ε.
        // It assumes that a decimal digit is worth ulpDecimal*ε, and that
        // all data is known with an error estimate of ulpBinary*ε.
        private static bool adjustLastDigit(ptr<decimalSlice> _addr_d, ulong currentDiff, ulong targetDiff, ulong maxDiff, ulong ulpDecimal, ulong ulpBinary)
        {
            ref decimalSlice d = ref _addr_d.val;

            if (ulpDecimal < 2L * ulpBinary)
            { 
                // Approximation is too wide.
                return false;

            }

            while (currentDiff + ulpDecimal / 2L + ulpBinary < targetDiff)
            {
                d.d[d.nd - 1L]--;
                currentDiff += ulpDecimal;
            }

            if (currentDiff + ulpDecimal <= targetDiff + ulpDecimal / 2L + ulpBinary)
            { 
                // we have two choices, and don't know what to do.
                return false;

            }

            if (currentDiff < ulpBinary || currentDiff > maxDiff - ulpBinary)
            { 
                // we went too far
                return false;

            }

            if (d.nd == 1L && d.d[0L] == '0')
            { 
                // the number has actually reached zero.
                d.nd = 0L;
                d.dp = 0L;

            }

            return true;

        }
    }
}
