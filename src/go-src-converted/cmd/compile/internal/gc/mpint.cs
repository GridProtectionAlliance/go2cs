// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:41:56 UTC
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

        private static void SetOverflow(this ptr<Mpint> _addr_a)
        {
            ref Mpint a = ref _addr_a.val;

            a.Val.SetUint64(1L); // avoid spurious div-zero errors
            a.Ovf = true;

        }

        private static bool checkOverflow(this ptr<Mpint> _addr_a, long extra)
        {
            ref Mpint a = ref _addr_a.val;
 
            // We don't need to be precise here, any reasonable upper limit would do.
            // For now, use existing limit so we pass all the tests unchanged.
            if (a.Val.BitLen() + extra > Mpprec)
            {
                a.SetOverflow();
            }

            return a.Ovf;

        }

        private static void Set(this ptr<Mpint> _addr_a, ptr<Mpint> _addr_b)
        {
            ref Mpint a = ref _addr_a.val;
            ref Mpint b = ref _addr_b.val;

            a.Val.Set(_addr_b.Val);
        }

        private static bool SetFloat(this ptr<Mpint> _addr_a, ptr<Mpflt> _addr_b)
        {
            ref Mpint a = ref _addr_a.val;
            ref Mpflt b = ref _addr_b.val;
 
            // avoid converting huge floating-point numbers to integers
            // (2*Mpprec is large enough to permit all tests to pass)
            if (b.Val.MantExp(null) > 2L * Mpprec)
            {
                a.SetOverflow();
                return false;
            }

            {
                var (_, acc) = b.Val.Int(_addr_a.Val);

                if (acc == big.Exact)
                {
                    return true;
                }

            }


            const long delta = (long)16L; // a reasonably small number of bits > 0
 // a reasonably small number of bits > 0
            big.Float t = default;
            t.SetPrec(Mpprec - delta); 

            // try rounding down a little
            t.SetMode(big.ToZero);
            t.Set(_addr_b.Val);
            {
                (_, acc) = t.Int(_addr_a.Val);

                if (acc == big.Exact)
                {
                    return true;
                } 

                // try rounding up a little

            } 

            // try rounding up a little
            t.SetMode(big.AwayFromZero);
            t.Set(_addr_b.Val);
            {
                (_, acc) = t.Int(_addr_a.Val);

                if (acc == big.Exact)
                {
                    return true;
                }

            }


            a.Ovf = false;
            return false;

        }

        private static void Add(this ptr<Mpint> _addr_a, ptr<Mpint> _addr_b)
        {
            ref Mpint a = ref _addr_a.val;
            ref Mpint b = ref _addr_b.val;

            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint Add");
                }

                a.SetOverflow();
                return ;

            }

            a.Val.Add(_addr_a.Val, _addr_b.Val);

            if (a.checkOverflow(0L))
            {
                yyerror("constant addition overflow");
            }

        }

        private static void Sub(this ptr<Mpint> _addr_a, ptr<Mpint> _addr_b)
        {
            ref Mpint a = ref _addr_a.val;
            ref Mpint b = ref _addr_b.val;

            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint Sub");
                }

                a.SetOverflow();
                return ;

            }

            a.Val.Sub(_addr_a.Val, _addr_b.Val);

            if (a.checkOverflow(0L))
            {
                yyerror("constant subtraction overflow");
            }

        }

        private static void Mul(this ptr<Mpint> _addr_a, ptr<Mpint> _addr_b)
        {
            ref Mpint a = ref _addr_a.val;
            ref Mpint b = ref _addr_b.val;

            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint Mul");
                }

                a.SetOverflow();
                return ;

            }

            a.Val.Mul(_addr_a.Val, _addr_b.Val);

            if (a.checkOverflow(0L))
            {
                yyerror("constant multiplication overflow");
            }

        }

        private static void Quo(this ptr<Mpint> _addr_a, ptr<Mpint> _addr_b)
        {
            ref Mpint a = ref _addr_a.val;
            ref Mpint b = ref _addr_b.val;

            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint Quo");
                }

                a.SetOverflow();
                return ;

            }

            a.Val.Quo(_addr_a.Val, _addr_b.Val);

            if (a.checkOverflow(0L))
            { 
                // can only happen for div-0 which should be checked elsewhere
                yyerror("constant division overflow");

            }

        }

        private static void Rem(this ptr<Mpint> _addr_a, ptr<Mpint> _addr_b)
        {
            ref Mpint a = ref _addr_a.val;
            ref Mpint b = ref _addr_b.val;

            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint Rem");
                }

                a.SetOverflow();
                return ;

            }

            a.Val.Rem(_addr_a.Val, _addr_b.Val);

            if (a.checkOverflow(0L))
            { 
                // should never happen
                yyerror("constant modulo overflow");

            }

        }

        private static void Or(this ptr<Mpint> _addr_a, ptr<Mpint> _addr_b)
        {
            ref Mpint a = ref _addr_a.val;
            ref Mpint b = ref _addr_b.val;

            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint Or");
                }

                a.SetOverflow();
                return ;

            }

            a.Val.Or(_addr_a.Val, _addr_b.Val);

        }

        private static void And(this ptr<Mpint> _addr_a, ptr<Mpint> _addr_b)
        {
            ref Mpint a = ref _addr_a.val;
            ref Mpint b = ref _addr_b.val;

            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint And");
                }

                a.SetOverflow();
                return ;

            }

            a.Val.And(_addr_a.Val, _addr_b.Val);

        }

        private static void AndNot(this ptr<Mpint> _addr_a, ptr<Mpint> _addr_b)
        {
            ref Mpint a = ref _addr_a.val;
            ref Mpint b = ref _addr_b.val;

            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint AndNot");
                }

                a.SetOverflow();
                return ;

            }

            a.Val.AndNot(_addr_a.Val, _addr_b.Val);

        }

        private static void Xor(this ptr<Mpint> _addr_a, ptr<Mpint> _addr_b)
        {
            ref Mpint a = ref _addr_a.val;
            ref Mpint b = ref _addr_b.val;

            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint Xor");
                }

                a.SetOverflow();
                return ;

            }

            a.Val.Xor(_addr_a.Val, _addr_b.Val);

        }

        private static void Lsh(this ptr<Mpint> _addr_a, ptr<Mpint> _addr_b)
        {
            ref Mpint a = ref _addr_a.val;
            ref Mpint b = ref _addr_b.val;

            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint Lsh");
                }

                a.SetOverflow();
                return ;

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
                return ;

            }

            if (a.checkOverflow(int(s)))
            {
                yyerror("constant shift overflow");
                return ;
            }

            a.Val.Lsh(_addr_a.Val, uint(s));

        }

        private static void Rsh(this ptr<Mpint> _addr_a, ptr<Mpint> _addr_b)
        {
            ref Mpint a = ref _addr_a.val;
            ref Mpint b = ref _addr_b.val;

            if (a.Ovf || b.Ovf)
            {
                if (nsavederrors + nerrors == 0L)
                {
                    Fatalf("ovf in Mpint Rsh");
                }

                a.SetOverflow();
                return ;

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

                return ;

            }

            a.Val.Rsh(_addr_a.Val, uint(s));

        }

        private static long Cmp(this ptr<Mpint> _addr_a, ptr<Mpint> _addr_b)
        {
            ref Mpint a = ref _addr_a.val;
            ref Mpint b = ref _addr_b.val;

            return a.Val.Cmp(_addr_b.Val);
        }

        private static long CmpInt64(this ptr<Mpint> _addr_a, long c)
        {
            ref Mpint a = ref _addr_a.val;

            if (c == 0L)
            {
                return a.Val.Sign(); // common case shortcut
            }

            return a.Val.Cmp(big.NewInt(c));

        }

        private static void Neg(this ptr<Mpint> _addr_a)
        {
            ref Mpint a = ref _addr_a.val;

            a.Val.Neg(_addr_a.Val);
        }

        private static long Int64(this ptr<Mpint> _addr_a)
        {
            ref Mpint a = ref _addr_a.val;

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

        private static void SetInt64(this ptr<Mpint> _addr_a, long c)
        {
            ref Mpint a = ref _addr_a.val;

            a.Val.SetInt64(c);
        }

        private static void SetString(this ptr<Mpint> _addr_a, @string @as)
        {
            ref Mpint a = ref _addr_a.val;

            var (_, ok) = a.Val.SetString(as, 0L);
            if (!ok)
            { 
                // The lexer checks for correct syntax of the literal
                // and reports detailed errors. Thus SetString should
                // never fail (in theory it might run out of memory,
                // but that wouldn't be reported as an error here).
                Fatalf("malformed integer constant: %s", as);
                return ;

            }

            if (a.checkOverflow(0L))
            {
                yyerror("constant too large: %s", as);
            }

        }

        private static @string GoString(this ptr<Mpint> _addr_a)
        {
            ref Mpint a = ref _addr_a.val;

            return a.Val.String();
        }

        private static @string String(this ptr<Mpint> _addr_a)
        {
            ref Mpint a = ref _addr_a.val;

            return fmt.Sprintf("%#x", _addr_a.Val);
        }
    }
}}}}
