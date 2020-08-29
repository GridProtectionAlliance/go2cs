// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:27:28 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\mpfloat.go
using fmt = go.fmt_package;
using math = go.math_package;
using big = go.math.big_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // implements float arithmetic
 
        // Maximum size in bits for Mpints before signalling
        // overflow and also mantissa precision for Mpflts.
        public static readonly long Mpprec = 512L; 
        // Turn on for constant arithmetic debugging output.
        public static readonly var Mpdebug = false;

        // Mpflt represents a floating-point constant.
        public partial struct Mpflt
        {
            public big.Float Val;
        }

        // Mpcplx represents a complex constant.
        public partial struct Mpcplx
        {
            public Mpflt Real;
            public Mpflt Imag;
        }

        private static ref Mpflt newMpflt()
        {
            Mpflt a = default;
            a.Val.SetPrec(Mpprec);
            return ref a;
        }

        private static ref Mpcplx newMpcmplx()
        {
            Mpcplx a = default;
            a.Real = newMpflt().Value;
            a.Imag = newMpflt().Value;
            return ref a;
        }

        private static void SetInt(this ref Mpflt a, ref Mpint b)
        {
            if (b.checkOverflow(0L))
            { 
                // sign doesn't really matter but copy anyway
                a.Val.SetInf(b.Val.Sign() < 0L);
                return;
            }
            a.Val.SetInt(ref b.Val);
        }

        private static void Set(this ref Mpflt a, ref Mpflt b)
        {
            a.Val.Set(ref b.Val);
        }

        private static void Add(this ref Mpflt a, ref Mpflt b)
        {
            if (Mpdebug)
            {
                fmt.Printf("\n%v + %v", a, b);
            }
            a.Val.Add(ref a.Val, ref b.Val);

            if (Mpdebug)
            {
                fmt.Printf(" = %v\n\n", a);
            }
        }

        private static void AddFloat64(this ref Mpflt a, double c)
        {
            Mpflt b = default;

            b.SetFloat64(c);
            a.Add(ref b);
        }

        private static void Sub(this ref Mpflt a, ref Mpflt b)
        {
            if (Mpdebug)
            {
                fmt.Printf("\n%v - %v", a, b);
            }
            a.Val.Sub(ref a.Val, ref b.Val);

            if (Mpdebug)
            {
                fmt.Printf(" = %v\n\n", a);
            }
        }

        private static void Mul(this ref Mpflt a, ref Mpflt b)
        {
            if (Mpdebug)
            {
                fmt.Printf("%v\n * %v\n", a, b);
            }
            a.Val.Mul(ref a.Val, ref b.Val);

            if (Mpdebug)
            {
                fmt.Printf(" = %v\n\n", a);
            }
        }

        private static void MulFloat64(this ref Mpflt a, double c)
        {
            Mpflt b = default;

            b.SetFloat64(c);
            a.Mul(ref b);
        }

        private static void Quo(this ref Mpflt a, ref Mpflt b)
        {
            if (Mpdebug)
            {
                fmt.Printf("%v\n / %v\n", a, b);
            }
            a.Val.Quo(ref a.Val, ref b.Val);

            if (Mpdebug)
            {
                fmt.Printf(" = %v\n\n", a);
            }
        }

        private static long Cmp(this ref Mpflt a, ref Mpflt b)
        {
            return a.Val.Cmp(ref b.Val);
        }

        private static long CmpFloat64(this ref Mpflt a, double c)
        {
            if (c == 0L)
            {
                return a.Val.Sign(); // common case shortcut
            }
            return a.Val.Cmp(big.NewFloat(c));
        }

        private static double Float64(this ref Mpflt a)
        {
            var (x, _) = a.Val.Float64(); 

            // check for overflow
            if (math.IsInf(x, 0L) && nsavederrors + nerrors == 0L)
            {
                Fatalf("ovf in Mpflt Float64");
            }
            return x + 0L; // avoid -0 (should not be needed, but be conservative)
        }

        private static double Float32(this ref Mpflt a)
        {
            var (x32, _) = a.Val.Float32();
            var x = float64(x32); 

            // check for overflow
            if (math.IsInf(x, 0L) && nsavederrors + nerrors == 0L)
            {
                Fatalf("ovf in Mpflt Float32");
            }
            return x + 0L; // avoid -0 (should not be needed, but be conservative)
        }

        private static void SetFloat64(this ref Mpflt a, double c)
        {
            if (Mpdebug)
            {
                fmt.Printf("\nconst %g", c);
            } 

            // convert -0 to 0
            if (c == 0L)
            {
                c = 0L;
            }
            a.Val.SetFloat64(c);

            if (Mpdebug)
            {
                fmt.Printf(" = %v\n", a);
            }
        }

        private static void Neg(this ref Mpflt a)
        { 
            // avoid -0
            if (a.Val.Sign() != 0L)
            {
                a.Val.Neg(ref a.Val);
            }
        }

        private static void SetString(this ref Mpflt a, @string @as)
        {
            while (len(as) > 0L && (as[0L] == ' ' || as[0L] == '\t'))
            {
                as = as[1L..];
            }


            var (f, _, err) = a.Val.Parse(as, 10L);
            if (err != null)
            {
                yyerror("malformed constant: %s (%v)", as, err);
                a.Val.SetFloat64(0L);
                return;
            }
            if (f.IsInf())
            {
                yyerror("constant too large: %s", as);
                a.Val.SetFloat64(0L);
                return;
            } 

            // -0 becomes 0
            if (f.Sign() == 0L && f.Signbit())
            {
                a.Val.SetFloat64(0L);
            }
        }

        private static @string String(this ref Mpflt f)
        {
            return fconv(f, 0L);
        }

        private static @string fconv(ref Mpflt fvp, FmtFlag flag)
        {
            if (flag & FmtSharp == 0L)
            {
                return fvp.Val.Text('b', 0L);
            } 

            // use decimal format for error messages

            // determine sign
            var f = ref fvp.Val;
            @string sign = default;
            if (f.Sign() < 0L)
            {
                sign = "-";
                f = @new<big.Float>().Abs(f);
            }
            else if (flag & FmtSign != 0L)
            {
                sign = "+";
            } 

            // Don't try to convert infinities (will not terminate).
            if (f.IsInf())
            {
                return sign + "Inf";
            } 

            // Use exact fmt formatting if in float64 range (common case):
            // proceed if f doesn't underflow to 0 or overflow to inf.
            {
                var (x, _) = f.Float64();

                if (f.Sign() == 0L == (x == 0L) && !math.IsInf(x, 0L))
                {
                    return fmt.Sprintf("%s%.6g", sign, x);
                } 

                // Out of float64 range. Do approximate manual to decimal
                // conversion to avoid precise but possibly slow Float
                // formatting.
                // f = mant * 2**exp

            } 

            // Out of float64 range. Do approximate manual to decimal
            // conversion to avoid precise but possibly slow Float
            // formatting.
            // f = mant * 2**exp
            big.Float mant = default;
            var exp = f.MantExp(ref mant); // 0.5 <= mant < 1.0

            // approximate float64 mantissa m and decimal exponent d
            // f ~ m * 10**d
            var (m, _) = mant.Float64(); // 0.5 <= m < 1.0
            var d = float64(exp) * (math.Ln2 / math.Ln10); // log_10(2)

            // adjust m for truncated (integer) decimal exponent e
            var e = int64(d);
            m *= math.Pow(10L, d - float64(e)); 

            // ensure 1 <= m < 10

            if (m < 1L - 0.5e-6F) 
                // The %.6g format below rounds m to 5 digits after the
                // decimal point. Make sure that m*10 < 10 even after
                // rounding up: m*10 + 0.5e-5 < 10 => m < 1 - 0.5e6.
                m *= 10L;
                e--;
            else if (m >= 10L) 
                m /= 10L;
                e++;
                        return fmt.Sprintf("%s%.6ge%+d", sign, m, e);
        }
    }
}}}}
