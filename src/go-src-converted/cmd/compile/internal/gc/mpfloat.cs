// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 08 04:29:30 UTC
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
        public static readonly long Mpprec = (long)512L; 
        // Turn on for constant arithmetic debugging output.
        public static readonly var Mpdebug = (var)false;


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

        // Use newMpflt (not new(Mpflt)!) to get the correct default precision.
        private static ptr<Mpflt> newMpflt()
        {
            ref Mpflt a = ref heap(out ptr<Mpflt> _addr_a);
            a.Val.SetPrec(Mpprec);
            return _addr__addr_a!;
        }

        // Use newMpcmplx (not new(Mpcplx)!) to get the correct default precision.
        private static ptr<Mpcplx> newMpcmplx()
        {
            ref Mpcplx a = ref heap(out ptr<Mpcplx> _addr_a);
            a.Real = newMpflt().val;
            a.Imag = newMpflt().val;
            return _addr__addr_a!;
        }

        private static void SetInt(this ptr<Mpflt> _addr_a, ptr<Mpint> _addr_b)
        {
            ref Mpflt a = ref _addr_a.val;
            ref Mpint b = ref _addr_b.val;

            if (b.checkOverflow(0L))
            { 
                // sign doesn't really matter but copy anyway
                a.Val.SetInf(b.Val.Sign() < 0L);
                return ;

            }

            a.Val.SetInt(_addr_b.Val);

        }

        private static void Set(this ptr<Mpflt> _addr_a, ptr<Mpflt> _addr_b)
        {
            ref Mpflt a = ref _addr_a.val;
            ref Mpflt b = ref _addr_b.val;

            a.Val.Set(_addr_b.Val);
        }

        private static void Add(this ptr<Mpflt> _addr_a, ptr<Mpflt> _addr_b)
        {
            ref Mpflt a = ref _addr_a.val;
            ref Mpflt b = ref _addr_b.val;

            if (Mpdebug)
            {
                fmt.Printf("\n%v + %v", a, b);
            }

            a.Val.Add(_addr_a.Val, _addr_b.Val);

            if (Mpdebug)
            {
                fmt.Printf(" = %v\n\n", a);
            }

        }

        private static void AddFloat64(this ptr<Mpflt> _addr_a, double c)
        {
            ref Mpflt a = ref _addr_a.val;

            ref Mpflt b = ref heap(out ptr<Mpflt> _addr_b);

            b.SetFloat64(c);
            a.Add(_addr_b);
        }

        private static void Sub(this ptr<Mpflt> _addr_a, ptr<Mpflt> _addr_b)
        {
            ref Mpflt a = ref _addr_a.val;
            ref Mpflt b = ref _addr_b.val;

            if (Mpdebug)
            {
                fmt.Printf("\n%v - %v", a, b);
            }

            a.Val.Sub(_addr_a.Val, _addr_b.Val);

            if (Mpdebug)
            {
                fmt.Printf(" = %v\n\n", a);
            }

        }

        private static void Mul(this ptr<Mpflt> _addr_a, ptr<Mpflt> _addr_b)
        {
            ref Mpflt a = ref _addr_a.val;
            ref Mpflt b = ref _addr_b.val;

            if (Mpdebug)
            {
                fmt.Printf("%v\n * %v\n", a, b);
            }

            a.Val.Mul(_addr_a.Val, _addr_b.Val);

            if (Mpdebug)
            {
                fmt.Printf(" = %v\n\n", a);
            }

        }

        private static void MulFloat64(this ptr<Mpflt> _addr_a, double c)
        {
            ref Mpflt a = ref _addr_a.val;

            ref Mpflt b = ref heap(out ptr<Mpflt> _addr_b);

            b.SetFloat64(c);
            a.Mul(_addr_b);
        }

        private static void Quo(this ptr<Mpflt> _addr_a, ptr<Mpflt> _addr_b)
        {
            ref Mpflt a = ref _addr_a.val;
            ref Mpflt b = ref _addr_b.val;

            if (Mpdebug)
            {
                fmt.Printf("%v\n / %v\n", a, b);
            }

            a.Val.Quo(_addr_a.Val, _addr_b.Val);

            if (Mpdebug)
            {
                fmt.Printf(" = %v\n\n", a);
            }

        }

        private static long Cmp(this ptr<Mpflt> _addr_a, ptr<Mpflt> _addr_b)
        {
            ref Mpflt a = ref _addr_a.val;
            ref Mpflt b = ref _addr_b.val;

            return a.Val.Cmp(_addr_b.Val);
        }

        private static long CmpFloat64(this ptr<Mpflt> _addr_a, double c)
        {
            ref Mpflt a = ref _addr_a.val;

            if (c == 0L)
            {
                return a.Val.Sign(); // common case shortcut
            }

            return a.Val.Cmp(big.NewFloat(c));

        }

        private static double Float64(this ptr<Mpflt> _addr_a)
        {
            ref Mpflt a = ref _addr_a.val;

            var (x, _) = a.Val.Float64(); 

            // check for overflow
            if (math.IsInf(x, 0L) && nsavederrors + nerrors == 0L)
            {
                Fatalf("ovf in Mpflt Float64");
            }

            return x + 0L; // avoid -0 (should not be needed, but be conservative)
        }

        private static double Float32(this ptr<Mpflt> _addr_a)
        {
            ref Mpflt a = ref _addr_a.val;

            var (x32, _) = a.Val.Float32();
            var x = float64(x32); 

            // check for overflow
            if (math.IsInf(x, 0L) && nsavederrors + nerrors == 0L)
            {
                Fatalf("ovf in Mpflt Float32");
            }

            return x + 0L; // avoid -0 (should not be needed, but be conservative)
        }

        private static void SetFloat64(this ptr<Mpflt> _addr_a, double c)
        {
            ref Mpflt a = ref _addr_a.val;

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

        private static void Neg(this ptr<Mpflt> _addr_a)
        {
            ref Mpflt a = ref _addr_a.val;
 
            // avoid -0
            if (a.Val.Sign() != 0L)
            {
                a.Val.Neg(_addr_a.Val);
            }

        }

        private static void SetString(this ptr<Mpflt> _addr_a, @string @as)
        {
            ref Mpflt a = ref _addr_a.val;

            var (f, _, err) = a.Val.Parse(as, 0L);
            if (err != null)
            {
                yyerror("malformed constant: %s (%v)", as, err);
                a.Val.SetFloat64(0L);
                return ;
            }

            if (f.IsInf())
            {
                yyerror("constant too large: %s", as);
                a.Val.SetFloat64(0L);
                return ;
            } 

            // -0 becomes 0
            if (f.Sign() == 0L && f.Signbit())
            {
                a.Val.SetFloat64(0L);
            }

        }

        private static @string String(this ptr<Mpflt> _addr_f)
        {
            ref Mpflt f = ref _addr_f.val;

            return f.Val.Text('b', 0L);
        }

        private static @string GoString(this ptr<Mpflt> _addr_fvp)
        {
            ref Mpflt fvp = ref _addr_fvp.val;
 
            // determine sign
            @string sign = "";
            var f = _addr_fvp.Val;
            if (f.Sign() < 0L)
            {
                sign = "-";
                f = @new<big.Float>().Abs(f);
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
            ref big.Float mant = ref heap(out ptr<big.Float> _addr_mant);
            var exp = f.MantExp(_addr_mant); // 0.5 <= mant < 1.0

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

        // complex multiply v *= rv
        //    (a, b) * (c, d) = (a*c - b*d, b*c + a*d)
        private static void Mul(this ptr<Mpcplx> _addr_v, ptr<Mpcplx> _addr_rv)
        {
            ref Mpcplx v = ref _addr_v.val;
            ref Mpcplx rv = ref _addr_rv.val;

            ref Mpflt ac = ref heap(out ptr<Mpflt> _addr_ac);            ref Mpflt ad = ref heap(out ptr<Mpflt> _addr_ad);            ref Mpflt bc = ref heap(out ptr<Mpflt> _addr_bc);            ref Mpflt bd = ref heap(out ptr<Mpflt> _addr_bd);



            ac.Set(_addr_v.Real);
            ac.Mul(_addr_rv.Real); // ac

            bd.Set(_addr_v.Imag);
            bd.Mul(_addr_rv.Imag); // bd

            bc.Set(_addr_v.Imag);
            bc.Mul(_addr_rv.Real); // bc

            ad.Set(_addr_v.Real);
            ad.Mul(_addr_rv.Imag); // ad

            v.Real.Set(_addr_ac);
            v.Real.Sub(_addr_bd); // ac-bd

            v.Imag.Set(_addr_bc);
            v.Imag.Add(_addr_ad); // bc+ad
        }

        // complex divide v /= rv
        //    (a, b) / (c, d) = ((a*c + b*d), (b*c - a*d))/(c*c + d*d)
        private static bool Div(this ptr<Mpcplx> _addr_v, ptr<Mpcplx> _addr_rv)
        {
            ref Mpcplx v = ref _addr_v.val;
            ref Mpcplx rv = ref _addr_rv.val;

            if (rv.Real.CmpFloat64(0L) == 0L && rv.Imag.CmpFloat64(0L) == 0L)
            {
                return false;
            }

            ref Mpflt ac = ref heap(out ptr<Mpflt> _addr_ac);            ref Mpflt ad = ref heap(out ptr<Mpflt> _addr_ad);            ref Mpflt bc = ref heap(out ptr<Mpflt> _addr_bc);            ref Mpflt bd = ref heap(out ptr<Mpflt> _addr_bd);            ref Mpflt cc_plus_dd = ref heap(out ptr<Mpflt> _addr_cc_plus_dd);



            cc_plus_dd.Set(_addr_rv.Real);
            cc_plus_dd.Mul(_addr_rv.Real); // cc

            ac.Set(_addr_rv.Imag);
            ac.Mul(_addr_rv.Imag); // dd
            cc_plus_dd.Add(_addr_ac); // cc+dd

            // We already checked that c and d are not both zero, but we can't
            // assume that c²+d² != 0 follows, because for tiny values of c
            // and/or d c²+d² can underflow to zero.  Check that c²+d² is
            // nonzero, return if it's not.
            if (cc_plus_dd.CmpFloat64(0L) == 0L)
            {
                return false;
            }

            ac.Set(_addr_v.Real);
            ac.Mul(_addr_rv.Real); // ac

            bd.Set(_addr_v.Imag);
            bd.Mul(_addr_rv.Imag); // bd

            bc.Set(_addr_v.Imag);
            bc.Mul(_addr_rv.Real); // bc

            ad.Set(_addr_v.Real);
            ad.Mul(_addr_rv.Imag); // ad

            v.Real.Set(_addr_ac);
            v.Real.Add(_addr_bd); // ac+bd
            v.Real.Quo(_addr_cc_plus_dd); // (ac+bd)/(cc+dd)

            v.Imag.Set(_addr_bc);
            v.Imag.Sub(_addr_ad); // bc-ad
            v.Imag.Quo(_addr_cc_plus_dd); // (bc+ad)/(cc+dd)

            return true;

        }

        private static @string String(this ptr<Mpcplx> _addr_v)
        {
            ref Mpcplx v = ref _addr_v.val;

            return fmt.Sprintf("(%s+%si)", v.Real.String(), v.Imag.String());
        }

        private static @string GoString(this ptr<Mpcplx> _addr_v)
        {
            ref Mpcplx v = ref _addr_v.val;

            @string re = default;
            var sre = v.Real.CmpFloat64(0L);
            if (sre != 0L)
            {
                re = v.Real.GoString();
            }

            @string im = default;
            var sim = v.Imag.CmpFloat64(0L);
            if (sim != 0L)
            {
                im = v.Imag.GoString();
            }


            if (sre == 0L && sim == 0L) 
                return "0";
            else if (sre == 0L) 
                return im + "i";
            else if (sim == 0L) 
                return re;
            else if (sim < 0L) 
                return fmt.Sprintf("(%s%si)", re, im);
            else 
                return fmt.Sprintf("(%s+%si)", re, im);
            
        }
    }
}}}}
