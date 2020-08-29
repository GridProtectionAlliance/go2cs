// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:27:30 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\mpint.go
using fmt = go.fmt_package;
using big = go.math.big_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // implements integer arithmetic

        // Mpint represents an integer constant.
        public partial struct Mpint
        {
            public big.Int Val;
            public bool Ovf; // set if Val overflowed compiler limit (sticky)
            public bool Rune; // set if syntax indicates default type rune
        }

        private static void SetOverflow(this ref Mpint a)
        {
            a.Val.SetUint64(1L); // avoid spurious div-zero errors
            a.Ovf = true;
        }

        private static bool checkOverflow(this ref Mpint a, long extra)
        { 
            // We don't need to be precise here, any reasonable upper limit would do.
            // For now, use existing limit so we pass all the tests unchanged.
            if (a.Val.BitLen() + extra > Mpprec)
            {
                a.SetOverflow();
            }
            return a.Ovf;
        }

        private static void Set(this ref Mpint a, ref Mpint b)
        {
            a.Val.Set(ref b.Val);
        }

        private static bool SetFloat(this ref Mpint a, ref Mpflt b)
        { 
            // avoid converting huge floating-point numbers to integers
            // (2*Mpprec is large enough to permit all tests to pass)
            if (b.Val.MantExp(null) > 2L * Mpprec)
            {
                a.SetOverflow();
                return false;
            }
            {
                var (_, acc) = b.Val.Int(ref a.Val);

                if (acc == big.Exact)
                {
                    return true;
                }

            }

            const long delta = 16L; // a reasonably small number of bits > 0
 // a reasonably small number of bits > 0
            big.Float t = default;
            t.SetPrec(Mpprec - delta); 

            // try rounding down a little
            t.SetMode(big.ToZero);
            t.Set(ref b.Val);
            {
                (_, acc) = t.Int(ref a.Val);

                if (acc == big.Exact)
                {
                    return true;
                } 

                // try rounding up a little

            } 

            // try rounding up a little
            t.SetMode(big.AwayFromZero);
            t.Set(ref b.Val);
            {
                (_, acc) = t.Int(ref a.Val);

                if (acc == big.Exact)
                {
                    return true;
                }

            }

            a.Ovf = false;
            return false;
        }

        private static void Add(this ref Mpint a, ref Mpint b)
        {
            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint Add");
                }
                a.SetOverflow();
                return;
            }
            a.Val.Add(ref a.Val, ref b.Val);

            if (a.checkOverflow(0L))
            {
                yyerror("constant addition overflow");
            }
        }

        private static void Sub(this ref Mpint a, ref Mpint b)
        {
            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint Sub");
                }
                a.SetOverflow();
                return;
            }
            a.Val.Sub(ref a.Val, ref b.Val);

            if (a.checkOverflow(0L))
            {
                yyerror("constant subtraction overflow");
            }
        }

        private static void Mul(this ref Mpint a, ref Mpint b)
        {
            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint Mul");
                }
                a.SetOverflow();
                return;
            }
            a.Val.Mul(ref a.Val, ref b.Val);

            if (a.checkOverflow(0L))
            {
                yyerror("constant multiplication overflow");
            }
        }

        private static void Quo(this ref Mpint a, ref Mpint b)
        {
            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint Quo");
                }
                a.SetOverflow();
                return;
            }
            a.Val.Quo(ref a.Val, ref b.Val);

            if (a.checkOverflow(0L))
            { 
                // can only happen for div-0 which should be checked elsewhere
                yyerror("constant division overflow");
            }
        }

        private static void Rem(this ref Mpint a, ref Mpint b)
        {
            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint Rem");
                }
                a.SetOverflow();
                return;
            }
            a.Val.Rem(ref a.Val, ref b.Val);

            if (a.checkOverflow(0L))
            { 
                // should never happen
                yyerror("constant modulo overflow");
            }
        }

        private static void Or(this ref Mpint a, ref Mpint b)
        {
            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint Or");
                }
                a.SetOverflow();
                return;
            }
            a.Val.Or(ref a.Val, ref b.Val);
        }

        private static void And(this ref Mpint a, ref Mpint b)
        {
            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint And");
                }
                a.SetOverflow();
                return;
            }
            a.Val.And(ref a.Val, ref b.Val);
        }

        private static void AndNot(this ref Mpint a, ref Mpint b)
        {
            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint AndNot");
                }
                a.SetOverflow();
                return;
            }
            a.Val.AndNot(ref a.Val, ref b.Val);
        }

        private static void Xor(this ref Mpint a, ref Mpint b)
        {
            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint Xor");
                }
                a.SetOverflow();
                return;
            }
            a.Val.Xor(ref a.Val, ref b.Val);
        }

        private static void Lsh(this ref Mpint a, ref Mpint b)
        {
            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint Lsh");
                }
                a.SetOverflow();
                return;
            }
            var s = b.Int64();
            if (s < 0L || s >= Mpprec)
            {
                @string msg = "shift count too large";
                if (s < 0L)
                {
                    msg = "invalid negative shift count";
                }
                yyerror("%s: %d", msg, s);
                a.SetInt64(0L);
                return;
            }
            if (a.checkOverflow(int(s)))
            {
                yyerror("constant shift overflow");
                return;
            }
            a.Val.Lsh(ref a.Val, uint(s));
        }

        private static void Rsh(this ref Mpint a, ref Mpint b)
        {
            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint Rsh");
                }
                a.SetOverflow();
                return;
            }
            var s = b.Int64();
            if (s < 0L)
            {
                yyerror("invalid negative shift count: %d", s);
                if (a.Val.Sign() < 0L)
                {
                    a.SetInt64(-1L);
                }
                else
                {
                    a.SetInt64(0L);
                }
                return;
            }
            a.Val.Rsh(ref a.Val, uint(s));
        }

        private static long Cmp(this ref Mpint a, ref Mpint b)
        {
            return a.Val.Cmp(ref b.Val);
        }

        private static long CmpInt64(this ref Mpint a, long c)
        {
            if (c == 0L)
            {
                return a.Val.Sign(); // common case shortcut
            }
            return a.Val.Cmp(big.NewInt(c));
        }

        private static void Neg(this ref Mpint a)
        {
            a.Val.Neg(ref a.Val);
        }

        private static long Int64(this ref Mpint a)
        {
            if (a.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("constant overflow");
                }
                return 0L;
            }
            return a.Val.Int64();
        }

        private static void SetInt64(this ref Mpint a, long c)
        {
            a.Val.SetInt64(c);
        }

        private static void SetString(this ref Mpint a, @string @as)
        {
            var (_, ok) = a.Val.SetString(as, 0L);
            if (!ok)
            { 
                // required syntax is [+-][0[x]]d*
                // At the moment we lose precise error cause;
                // the old code distinguished between:
                // - malformed hex constant
                // - malformed octal constant
                // - malformed decimal constant
                // TODO(gri) use different conversion function
                yyerror("malformed integer constant: %s", as);
                a.Val.SetUint64(0L);
                return;
            }
            if (a.checkOverflow(0L))
            {
                yyerror("constant too large: %s", as);
            }
        }

        private static @string String(this ref Mpint x)
        {
            return bconv(x, 0L);
        }

        private static @string bconv(ref Mpint xval, FmtFlag flag)
        {
            if (flag & FmtSharp != 0L)
            {
                return fmt.Sprintf("%#x", ref xval.Val);
            }
            return xval.Val.String();
        }
    }
}}}}
