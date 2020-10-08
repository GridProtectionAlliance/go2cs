// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 08 04:28:36 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\const.go
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using big = go.math.big_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // Ctype describes the constant kind of an "ideal" (untyped) constant.
        public partial struct Ctype // : byte
        {
        }

        public static readonly Ctype CTxxx = (Ctype)iota;

        public static readonly var CTINT = (var)0;
        public static readonly var CTRUNE = (var)1;
        public static readonly var CTFLT = (var)2;
        public static readonly var CTCPLX = (var)3;
        public static readonly var CTSTR = (var)4;
        public static readonly var CTBOOL = (var)5;
        public static readonly var CTNIL = (var)6;


        public partial struct Val
        {
        }

        public static Ctype Ctype(this Val v) => func((_, panic, __) =>
        {
            switch (v.U.type())
            {
                case 
                    return 0L;
                    break;
                case ptr<NilVal> x:
                    return CTNIL;
                    break;
                case bool x:
                    return CTBOOL;
                    break;
                case ptr<Mpint> x:
                    if (x.Rune)
                    {
                        return CTRUNE;
                    }

                    return CTINT;
                    break;
                case ptr<Mpflt> x:
                    return CTFLT;
                    break;
                case ptr<Mpcplx> x:
                    return CTCPLX;
                    break;
                case @string x:
                    return CTSTR;
                    break;
                default:
                {
                    var x = v.U.type();
                    Fatalf("unexpected Ctype for %T", v.U);
                    panic("unreachable");
                    break;
                }
            }

        });

        private static bool eqval(Val a, Val b) => func((_, panic, __) =>
        {
            if (a.Ctype() != b.Ctype())
            {
                return false;
            }

            switch (a.U.type())
            {
                case ptr<NilVal> x:
                    return true;
                    break;
                case bool x:
                    bool y = b.U._<bool>();
                    return x == y;
                    break;
                case ptr<Mpint> x:
                    y = b.U._<ptr<Mpint>>();
                    return x.Cmp(y) == 0L;
                    break;
                case ptr<Mpflt> x:
                    y = b.U._<ptr<Mpflt>>();
                    return x.Cmp(y) == 0L;
                    break;
                case ptr<Mpcplx> x:
                    y = b.U._<ptr<Mpcplx>>();
                    return x.Real.Cmp(_addr_y.Real) == 0L && x.Imag.Cmp(_addr_y.Imag) == 0L;
                    break;
                case @string x:
                    y = b.U._<@string>();
                    return x == y;
                    break;
                default:
                {
                    var x = a.U.type();
                    Fatalf("unexpected Ctype for %T", a.U);
                    panic("unreachable");
                    break;
                }
            }

        });

        // Interface returns the constant value stored in v as an interface{}.
        // It returns int64s for ints and runes, float64s for floats,
        // complex128s for complex values, and nil for constant nils.
        public static void Interface(this Val v) => func((_, panic, __) =>
        {
            switch (v.U.type())
            {
                case ptr<NilVal> x:
                    return null;
                    break;
                case bool x:
                    return x;
                    break;
                case @string x:
                    return x;
                    break;
                case ptr<Mpint> x:
                    return x.Int64();
                    break;
                case ptr<Mpflt> x:
                    return x.Float64();
                    break;
                case ptr<Mpcplx> x:
                    return complex(x.Real.Float64(), x.Imag.Float64());
                    break;
                default:
                {
                    var x = v.U.type();
                    Fatalf("unexpected Interface for %T", v.U);
                    panic("unreachable");
                    break;
                }
            }

        });

        public partial struct NilVal
        {
        }

        // Int64 returns n as an int64.
        // n must be an integer or rune constant.
        private static long Int64(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (!Isconst(_addr_n, CTINT))
            {
                Fatalf("Int64(%v)", n);
            }

            return n.Val().U._<ptr<Mpint>>().Int64();

        }

        // CanInt64 reports whether it is safe to call Int64() on n.
        private static bool CanInt64(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (!Isconst(_addr_n, CTINT))
            {
                return false;
            } 

            // if the value inside n cannot be represented as an int64, the
            // return value of Int64 is undefined
            return n.Val().U._<ptr<Mpint>>().CmpInt64(n.Int64()) == 0L;

        }

        // Bool returns n as a bool.
        // n must be a boolean constant.
        private static bool Bool(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (!Isconst(_addr_n, CTBOOL))
            {
                Fatalf("Bool(%v)", n);
            }

            return n.Val().U._<bool>();

        }

        // truncate float literal fv to 32-bit or 64-bit precision
        // according to type; return truncated value.
        private static ptr<Mpflt> truncfltlit(ptr<Mpflt> _addr_oldv, ptr<types.Type> _addr_t)
        {
            ref Mpflt oldv = ref _addr_oldv.val;
            ref types.Type t = ref _addr_t.val;

            if (t == null)
            {
                return _addr_oldv!;
            }

            if (overflow(new Val(oldv), _addr_t))
            { 
                // If there was overflow, simply continuing would set the
                // value to Inf which in turn would lead to spurious follow-on
                // errors. Avoid this by returning the existing value.
                return _addr_oldv!;

            }

            var fv = newMpflt(); 

            // convert large precision literal floating
            // into limited precision (float64 or float32)

            if (t.Etype == types.TFLOAT32) 
                fv.SetFloat64(oldv.Float32());
            else if (t.Etype == types.TFLOAT64) 
                fv.SetFloat64(oldv.Float64());
            else 
                Fatalf("truncfltlit: unexpected Etype %v", t.Etype);
                        return _addr_fv!;

        }

        // truncate Real and Imag parts of Mpcplx to 32-bit or 64-bit
        // precision, according to type; return truncated value. In case of
        // overflow, calls yyerror but does not truncate the input value.
        private static ptr<Mpcplx> trunccmplxlit(ptr<Mpcplx> _addr_oldv, ptr<types.Type> _addr_t)
        {
            ref Mpcplx oldv = ref _addr_oldv.val;
            ref types.Type t = ref _addr_t.val;

            if (t == null)
            {
                return _addr_oldv!;
            }

            if (overflow(new Val(oldv), _addr_t))
            { 
                // If there was overflow, simply continuing would set the
                // value to Inf which in turn would lead to spurious follow-on
                // errors. Avoid this by returning the existing value.
                return _addr_oldv!;

            }

            var cv = newMpcmplx();


            if (t.Etype == types.TCOMPLEX64) 
                cv.Real.SetFloat64(oldv.Real.Float32());
                cv.Imag.SetFloat64(oldv.Imag.Float32());
            else if (t.Etype == types.TCOMPLEX128) 
                cv.Real.SetFloat64(oldv.Real.Float64());
                cv.Imag.SetFloat64(oldv.Imag.Float64());
            else 
                Fatalf("trunccplxlit: unexpected Etype %v", t.Etype);
                        return _addr_cv!;

        }

        // TODO(mdempsky): Replace these with better APIs.
        private static ptr<Node> convlit(ptr<Node> _addr_n, ptr<types.Type> _addr_t)
        {
            ref Node n = ref _addr_n.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_convlit1(_addr_n, _addr_t, false, null)!;
        }
        private static ptr<Node> defaultlit(ptr<Node> _addr_n, ptr<types.Type> _addr_t)
        {
            ref Node n = ref _addr_n.val;
            ref types.Type t = ref _addr_t.val;

            return _addr_convlit1(_addr_n, _addr_t, false, null)!;
        }

        // convlit1 converts an untyped expression n to type t. If n already
        // has a type, convlit1 has no effect.
        //
        // For explicit conversions, t must be non-nil, and integer-to-string
        // conversions are allowed.
        //
        // For implicit conversions (e.g., assignments), t may be nil; if so,
        // n is converted to its default type.
        //
        // If there's an error converting n to t, context is used in the error
        // message.
        private static ptr<Node> convlit1(ptr<Node> _addr_n, ptr<types.Type> _addr_t, bool @explicit, Func<@string> context)
        {
            ref Node n = ref _addr_n.val;
            ref types.Type t = ref _addr_t.val;

            if (explicit && t == null)
            {
                Fatalf("explicit conversion missing type");
            }

            if (t != null && t.IsUntyped())
            {
                Fatalf("bad conversion to untyped: %v", t);
            }

            if (n == null || n.Type == null)
            { 
                // Allow sloppy callers.
                return _addr_n!;

            }

            if (!n.Type.IsUntyped())
            { 
                // Already typed; nothing to do.
                return _addr_n!;

            }

            if (n.Op == OLITERAL)
            { 
                // Can't always set n.Type directly on OLITERAL nodes.
                // See discussion on CL 20813.
                n = n.rawcopy();

            } 

            // Nil is technically not a constant, so handle it specially.
            if (n.Type.Etype == TNIL)
            {
                if (t == null)
                {
                    yyerror("use of untyped nil");
                    n.SetDiag(true);
                    n.Type = null;
                    return _addr_n!;
                }

                if (!t.HasNil())
                { 
                    // Leave for caller to handle.
                    return _addr_n!;

                }

                n.Type = t;
                return _addr_n!;

            }

            if (t == null || !okforconst[t.Etype])
            {
                t = defaultType(idealkind(_addr_n));
            }


            if (n.Op == OLITERAL) 
                var v = convertVal(n.Val(), _addr_t, explicit);
                if (v.U == null)
                {
                    break;
                }

                n.SetVal(v);
                n.Type = t;
                return _addr_n!;
            else if (n.Op == OPLUS || n.Op == ONEG || n.Op == OBITNOT || n.Op == ONOT || n.Op == OREAL || n.Op == OIMAG) 
                var ot = operandType(n.Op, _addr_t);
                if (ot == null)
                {
                    n = defaultlit(_addr_n, _addr_null);
                    break;
                }

                n.Left = convlit(_addr_n.Left, _addr_ot);
                if (n.Left.Type == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                n.Type = t;
                return _addr_n!;
            else if (n.Op == OADD || n.Op == OSUB || n.Op == OMUL || n.Op == ODIV || n.Op == OMOD || n.Op == OOR || n.Op == OXOR || n.Op == OAND || n.Op == OANDNOT || n.Op == OOROR || n.Op == OANDAND || n.Op == OCOMPLEX) 
                ot = operandType(n.Op, _addr_t);
                if (ot == null)
                {
                    n = defaultlit(_addr_n, _addr_null);
                    break;
                }

                n.Left = convlit(_addr_n.Left, _addr_ot);
                n.Right = convlit(_addr_n.Right, _addr_ot);
                if (n.Left.Type == null || n.Right.Type == null)
                {
                    n.Type = null;
                    return _addr_n!;
                }

                if (!types.Identical(n.Left.Type, n.Right.Type))
                {
                    yyerror("invalid operation: %v (mismatched types %v and %v)", n, n.Left.Type, n.Right.Type);
                    n.Type = null;
                    return _addr_n!;
                }

                n.Type = t;
                return _addr_n!;
            else if (n.Op == OEQ || n.Op == ONE || n.Op == OLT || n.Op == OLE || n.Op == OGT || n.Op == OGE) 
                if (!t.IsBoolean())
                {
                    break;
                }

                n.Type = t;
                return _addr_n!;
            else if (n.Op == OLSH || n.Op == ORSH) 
                n.Left = convlit1(_addr_n.Left, _addr_t, explicit, null);
                n.Type = n.Left.Type;
                if (n.Type != null && !n.Type.IsInteger())
                {
                    yyerror("invalid operation: %v (shift of type %v)", n, n.Type);
                    n.Type = null;
                }

                return _addr_n!;
            else 
                Fatalf("unexpected untyped expression: %v", n);
                        if (!n.Diag())
            {
                if (!t.Broke())
                {
                    if (explicit)
                    {
                        yyerror("cannot convert %L to type %v", n, t);
                    }
                    else if (context != null)
                    {
                        yyerror("cannot use %L as type %v in %s", n, t, context());
                    }
                    else
                    {
                        yyerror("cannot use %L as type %v", n, t);
                    }

                }

                n.SetDiag(true);

            }

            n.Type = null;
            return _addr_n!;

        }

        private static ptr<types.Type> operandType(Op op, ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;


            if (op == OCOMPLEX) 
                if (t.IsComplex())
                {
                    return _addr_floatForComplex(t)!;
                }

            else if (op == OREAL || op == OIMAG) 
                if (t.IsFloat())
                {
                    return _addr_complexForFloat(t)!;
                }

            else 
                if (okfor[op][t.Etype])
                {
                    return _addr_t!;
                }

                        return _addr_null!;

        }

        // convertVal converts v into a representation appropriate for t. If
        // no such representation exists, it returns Val{} instead.
        //
        // If explicit is true, then conversions from integer to string are
        // also allowed.
        private static Val convertVal(Val v, ptr<types.Type> _addr_t, bool @explicit)
        {
            ref types.Type t = ref _addr_t.val;

            {
                var ct = v.Ctype();


                if (ct == CTBOOL)
                {
                    if (t.IsBoolean())
                    {
                        return v;
                    }

                    goto __switch_break0;
                }
                if (ct == CTSTR)
                {
                    if (t.IsString())
                    {
                        return v;
                    }

                    goto __switch_break0;
                }
                if (ct == CTINT || ct == CTRUNE)
                {
                    if (explicit && t.IsString())
                    {
                        return tostr(v);
                    }

                    fallthrough = true;
                }
                if (fallthrough || ct == CTFLT || ct == CTCPLX)
                {

                    if (t.IsInteger()) 
                        v = toint(v);
                        overflow(v, _addr_t);
                        return v;
                    else if (t.IsFloat()) 
                        v = toflt(v);
                        v = new Val(truncfltlit(v.U.(*Mpflt),t));
                        return v;
                    else if (t.IsComplex()) 
                        v = tocplx(v);
                        v = new Val(trunccmplxlit(v.U.(*Mpcplx),t));
                        return v;
                                        goto __switch_break0;
                }

                __switch_break0:;
            }

            return new Val();

        }

        private static Val tocplx(Val v)
        {
            switch (v.U.type())
            {
                case ptr<Mpint> u:
                    var c = newMpcmplx();
                    c.Real.SetInt(u);
                    c.Imag.SetFloat64(0.0F);
                    v.U = c;
                    break;
                case ptr<Mpflt> u:
                    c = newMpcmplx();
                    c.Real.Set(u);
                    c.Imag.SetFloat64(0.0F);
                    v.U = c;
                    break;

            }

            return v;

        }

        private static Val toflt(Val v)
        {
            switch (v.U.type())
            {
                case ptr<Mpint> u:
                    var f = newMpflt();
                    f.SetInt(u);
                    v.U = f;
                    break;
                case ptr<Mpcplx> u:
                    f = newMpflt();
                    f.Set(_addr_u.Real);
                    if (u.Imag.CmpFloat64(0L) != 0L)
                    {
                        yyerror("constant %v truncated to real", u.GoString());
                    }

                    v.U = f;
                    break;

            }

            return v;

        }

        private static Val toint(Val v)
        {
            switch (v.U.type())
            {
                case ptr<Mpint> u:
                    if (u.Rune)
                    {
                        ptr<Mpint> i = @new<Mpint>();
                        i.Set(u);
                        v.U = i;
                    }

                    break;
                case ptr<Mpflt> u:
                    i = @new<Mpint>();
                    if (!i.SetFloat(u))
                    {
                        if (i.checkOverflow(0L))
                        {
                            yyerror("integer too large");
                        }
                        else
                        { 
                            // The value of u cannot be represented as an integer;
                            // so we need to print an error message.
                            // Unfortunately some float values cannot be
                            // reasonably formatted for inclusion in an error
                            // message (example: 1 + 1e-100), so first we try to
                            // format the float; if the truncation resulted in
                            // something that looks like an integer we omit the
                            // value from the error message.
                            // (See issue #11371).
                            big.Float t = default;
                            t.Parse(u.GoString(), 10L);
                            if (t.IsInt())
                            {
                                yyerror("constant truncated to integer");
                            }
                            else
                            {
                                yyerror("constant %v truncated to integer", u.GoString());
                            }

                        }

                    }

                    v.U = i;
                    break;
                case ptr<Mpcplx> u:
                    i = @new<Mpint>();
                    if (!i.SetFloat(_addr_u.Real) || u.Imag.CmpFloat64(0L) != 0L)
                    {
                        yyerror("constant %v truncated to integer", u.GoString());
                    }

                    v.U = i;
                    break;

            }

            return v;

        }

        private static bool doesoverflow(Val v, ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;

            switch (v.U.type())
            {
                case ptr<Mpint> u:
                    if (!t.IsInteger())
                    {
                        Fatalf("overflow: %v integer constant", t);
                    }

                    return u.Cmp(minintval[t.Etype]) < 0L || u.Cmp(maxintval[t.Etype]) > 0L;
                    break;
                case ptr<Mpflt> u:
                    if (!t.IsFloat())
                    {
                        Fatalf("overflow: %v floating-point constant", t);
                    }

                    return u.Cmp(minfltval[t.Etype]) <= 0L || u.Cmp(maxfltval[t.Etype]) >= 0L;
                    break;
                case ptr<Mpcplx> u:
                    if (!t.IsComplex())
                    {
                        Fatalf("overflow: %v complex constant", t);
                    }

                    return u.Real.Cmp(minfltval[t.Etype]) <= 0L || u.Real.Cmp(maxfltval[t.Etype]) >= 0L || u.Imag.Cmp(minfltval[t.Etype]) <= 0L || u.Imag.Cmp(maxfltval[t.Etype]) >= 0L;
                    break;

            }

            return false;

        }

        private static bool overflow(Val v, ptr<types.Type> _addr_t)
        {
            ref types.Type t = ref _addr_t.val;
 
            // v has already been converted
            // to appropriate form for t.
            if (t == null || t.Etype == TIDEAL)
            {
                return false;
            } 

            // Only uintptrs may be converted to pointers, which cannot overflow.
            if (t.IsPtr() || t.IsUnsafePtr())
            {
                return false;
            }

            if (doesoverflow(v, _addr_t))
            {
                yyerror("constant %v overflows %v", v, t);
                return true;
            }

            return false;


        }

        private static Val tostr(Val v)
        {
            switch (v.U.type())
            {
                case ptr<Mpint> u:
                    int r = 0xFFFDUL;
                    if (u.Cmp(minintval[TINT32]) >= 0L && u.Cmp(maxintval[TINT32]) <= 0L)
                    {
                        r = rune(u.Int64());
                    }

                    v.U = string(r);
                    break;

            }

            return v;

        }

        private static Ctype consttype(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n == null || n.Op != OLITERAL)
            {
                return CTxxx;
            }

            return n.Val().Ctype();

        }

        public static bool Isconst(ptr<Node> _addr_n, Ctype ct)
        {
            ref Node n = ref _addr_n.val;

            var t = consttype(_addr_n); 

            // If the caller is asking for CTINT, allow CTRUNE too.
            // Makes life easier for back ends.
            return t == ct || (ct == CTINT && t == CTRUNE);

        }

        // evconst rewrites constant expressions into OLITERAL nodes.
        private static void evconst(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            ref var nl = ref heap(n.Left, out ptr<var> _addr_nl);
            var nr = n.Right; 

            // Pick off just the opcodes that can be constant evaluated.
            {
                var op = n.Op;


                if (op == OPLUS || op == ONEG || op == OBITNOT || op == ONOT) 
                    if (nl.Op == OLITERAL)
                    {
                        setconst(_addr_n, unaryOp(op, nl.Val(), _addr_n.Type));
                    }

                else if (op == OADD || op == OSUB || op == OMUL || op == ODIV || op == OMOD || op == OOR || op == OXOR || op == OAND || op == OANDNOT || op == OOROR || op == OANDAND) 
                    if (nl.Op == OLITERAL && nr.Op == OLITERAL)
                    {
                        setconst(_addr_n, binaryOp(nl.Val(), op, nr.Val()));
                    }

                else if (op == OEQ || op == ONE || op == OLT || op == OLE || op == OGT || op == OGE) 
                    if (nl.Op == OLITERAL && nr.Op == OLITERAL)
                    {
                        setboolconst(_addr_n, compareOp(nl.Val(), op, nr.Val()));
                    }

                else if (op == OLSH || op == ORSH) 
                    if (nl.Op == OLITERAL && nr.Op == OLITERAL)
                    {
                        setconst(_addr_n, shiftOp(nl.Val(), op, nr.Val()));
                    }

                else if (op == OCONV || op == ORUNESTR) 
                    if (okforconst[n.Type.Etype] && nl.Op == OLITERAL)
                    {
                        setconst(_addr_n, convertVal(nl.Val(), _addr_n.Type, true));
                    }

                else if (op == OCONVNOP) 
                    if (okforconst[n.Type.Etype] && nl.Op == OLITERAL)
                    { 
                        // set so n.Orig gets OCONV instead of OCONVNOP
                        n.Op = OCONV;
                        setconst(_addr_n, nl.Val());

                    }

                else if (op == OADDSTR) 
                    // Merge adjacent constants in the argument list.
                    var s = n.List.Slice();
                    for (long i1 = 0L; i1 < len(s); i1++)
                    {
                        if (Isconst(_addr_s[i1], CTSTR) && i1 + 1L < len(s) && Isconst(_addr_s[i1 + 1L], CTSTR))
                        { 
                            // merge from i1 up to but not including i2
                            slice<@string> strs = default;
                            var i2 = i1;
                            while (i2 < len(s) && Isconst(_addr_s[i2], CTSTR))
                            {
                                strs = append(strs, strlit(_addr_s[i2]));
                                i2++;
                            }


                            nl = s[i1].val;
                            _addr_nl.Orig = _addr_nl;
                            nl.Orig = ref _addr_nl.Orig.val;
                            nl.SetVal(new Val(strings.Join(strs,"")));
                            _addr_s[i1] = _addr_nl;
                            s[i1] = ref _addr_s[i1].val;
                            s = append(s[..i1 + 1L], s[i2..]);

                        }

                    }


                    if (len(s) == 1L && Isconst(_addr_s[0L], CTSTR))
                    {
                        n.Op = OLITERAL;
                        n.SetVal(s[0L].Val());
                    }
                    else
                    {
                        n.List.Set(s);
                    }

                else if (op == OCAP || op == OLEN) 

                    if (nl.Type.Etype == TSTRING) 
                        if (Isconst(_addr_nl, CTSTR))
                        {
                            setintconst(_addr_n, int64(len(strlit(_addr_nl))));
                        }

                    else if (nl.Type.Etype == TARRAY) 
                        if (!hascallchan(_addr_nl))
                        {
                            setintconst(_addr_n, nl.Type.NumElem());
                        }

                                    else if (op == OALIGNOF || op == OOFFSETOF || op == OSIZEOF) 
                    setintconst(_addr_n, evalunsafe(n));
                else if (op == OREAL || op == OIMAG) 
                    if (nl.Op == OLITERAL)
                    {
                        ptr<Mpflt> re;                        ptr<Mpflt> im;

                        switch (nl.Val().U.type())
                        {
                            case ptr<Mpint> u:
                                re = newMpflt();
                                re.SetInt(u); 
                                // im = 0
                                break;
                            case ptr<Mpflt> u:
                                re = u; 
                                // im = 0
                                break;
                            case ptr<Mpcplx> u:
                                re = _addr_u.Real;
                                im = _addr_u.Imag;
                                break;
                            default:
                            {
                                var u = nl.Val().U.type();
                                Fatalf("impossible");
                                break;
                            }
                        }
                        if (n.Op == OIMAG)
                        {
                            if (im == null)
                            {
                                im = newMpflt();
                            }

                            re = addr(im);

                        }

                        setconst(_addr_n, new Val(re));

                    }

                else if (op == OCOMPLEX) 
                    if (nl.Op == OLITERAL && nr.Op == OLITERAL)
                    { 
                        // make it a complex literal
                        var c = newMpcmplx();
                        c.Real.Set(toflt(nl.Val()).U._<ptr<Mpflt>>());
                        c.Imag.Set(toflt(nr.Val()).U._<ptr<Mpflt>>());
                        setconst(_addr_n, new Val(c));

                    }


            }

        }

        private static (Val, Val) match(Val x, Val y)
        {
            Val _p0 = default;
            Val _p0 = default;


            if (x.Ctype() == CTCPLX || y.Ctype() == CTCPLX) 
                return (tocplx(x), tocplx(y));
            else if (x.Ctype() == CTFLT || y.Ctype() == CTFLT) 
                return (toflt(x), toflt(y));
            // Mixed int/rune are fine.
            return (x, y);

        }

        private static bool compareOp(Val x, Op op, Val y) => func((_, panic, __) =>
        {
            x, y = match(x, y);


            if (x.Ctype() == CTBOOL) 
                bool x = x.U._<bool>();
                bool y = y.U._<bool>();

                if (op == OEQ) 
                    return x == y;
                else if (op == ONE) 
                    return x != y;
                            else if (x.Ctype() == CTINT || x.Ctype() == CTRUNE) 
                x = x.U._<ptr<Mpint>>();
                y = y.U._<ptr<Mpint>>();
                return cmpZero(x.Cmp(y), op);
            else if (x.Ctype() == CTFLT) 
                x = x.U._<ptr<Mpflt>>();
                y = y.U._<ptr<Mpflt>>();
                return cmpZero(x.Cmp(y), op);
            else if (x.Ctype() == CTCPLX) 
                x = x.U._<ptr<Mpcplx>>();
                y = y.U._<ptr<Mpcplx>>();
                var eq = x.Real.Cmp(_addr_y.Real) == 0L && x.Imag.Cmp(_addr_y.Imag) == 0L;

                if (op == OEQ) 
                    return eq;
                else if (op == ONE) 
                    return !eq;
                            else if (x.Ctype() == CTSTR) 
                x = x.U._<@string>();
                y = y.U._<@string>();

                if (op == OEQ) 
                    return x == y;
                else if (op == ONE) 
                    return x != y;
                else if (op == OLT) 
                    return x < y;
                else if (op == OLE) 
                    return x <= y;
                else if (op == OGT) 
                    return x > y;
                else if (op == OGE) 
                    return x >= y;
                                        Fatalf("compareOp: bad comparison: %v %v %v", x, op, y);
            panic("unreachable");

        });

        private static bool cmpZero(long x, Op op) => func((_, panic, __) =>
        {

            if (op == OEQ) 
                return x == 0L;
            else if (op == ONE) 
                return x != 0L;
            else if (op == OLT) 
                return x < 0L;
            else if (op == OLE) 
                return x <= 0L;
            else if (op == OGT) 
                return x > 0L;
            else if (op == OGE) 
                return x >= 0L;
                        Fatalf("cmpZero: want comparison operator, got %v", op);
            panic("unreachable");

        });

        private static Val binaryOp(Val x, Op op, Val y) => func((_, panic, __) =>
        {
            x, y = match(x, y);

Outer:


            if (x.Ctype() == CTBOOL) 
                bool x = x.U._<bool>();
                bool y = y.U._<bool>();

                if (op == OANDAND) 
                    return new Val(U:x&&y);
                else if (op == OOROR) 
                    return new Val(U:x||y);
                            else if (x.Ctype() == CTINT || x.Ctype() == CTRUNE) 
                x = x.U._<ptr<Mpint>>();
                y = y.U._<ptr<Mpint>>();

                ptr<Mpint> u = @new<Mpint>();
                u.Rune = x.Rune || y.Rune;
                u.Set(x);

                if (op == OADD) 
                    u.Add(y);
                else if (op == OSUB) 
                    u.Sub(y);
                else if (op == OMUL) 
                    u.Mul(y);
                else if (op == ODIV) 
                    if (y.CmpInt64(0L) == 0L)
                    {
                        yyerror("division by zero");
                        return new Val();
                    }

                    u.Quo(y);
                else if (op == OMOD) 
                    if (y.CmpInt64(0L) == 0L)
                    {
                        yyerror("division by zero");
                        return new Val();
                    }

                    u.Rem(y);
                else if (op == OOR) 
                    u.Or(y);
                else if (op == OAND) 
                    u.And(y);
                else if (op == OANDNOT) 
                    u.AndNot(y);
                else if (op == OXOR) 
                    u.Xor(y);
                else 
                    _breakOuter = true;
                    break;
                                return new Val(U:u);
            else if (x.Ctype() == CTFLT) 
                x = x.U._<ptr<Mpflt>>();
                y = y.U._<ptr<Mpflt>>();

                u = newMpflt();
                u.Set(x);

                if (op == OADD) 
                    u.Add(y);
                else if (op == OSUB) 
                    u.Sub(y);
                else if (op == OMUL) 
                    u.Mul(y);
                else if (op == ODIV) 
                    if (y.CmpFloat64(0L) == 0L)
                    {
                        yyerror("division by zero");
                        return new Val();
                    }

                    u.Quo(y);
                else if (op == OMOD || op == OOR || op == OAND || op == OANDNOT || op == OXOR) 
                    // TODO(mdempsky): Move to typecheck; see #31060.
                    yyerror("invalid operation: operator %v not defined on untyped float", op);
                    return new Val();
                else 
                    _breakOuter = true;
                    break;
                                return new Val(U:u);
            else if (x.Ctype() == CTCPLX) 
                x = x.U._<ptr<Mpcplx>>();
                y = y.U._<ptr<Mpcplx>>();

                u = newMpcmplx();
                u.Real.Set(_addr_x.Real);
                u.Imag.Set(_addr_x.Imag);

                if (op == OADD) 
                    u.Real.Add(_addr_y.Real);
                    u.Imag.Add(_addr_y.Imag);
                else if (op == OSUB) 
                    u.Real.Sub(_addr_y.Real);
                    u.Imag.Sub(_addr_y.Imag);
                else if (op == OMUL) 
                    u.Mul(y);
                else if (op == ODIV) 
                    if (!u.Div(y))
                    {
                        yyerror("complex division by zero");
                        return new Val();
                    }

                else if (op == OMOD || op == OOR || op == OAND || op == OANDNOT || op == OXOR) 
                    // TODO(mdempsky): Move to typecheck; see #31060.
                    yyerror("invalid operation: operator %v not defined on untyped complex", op);
                    return new Val();
                else 
                    _breakOuter = true;
                    break;
                                return new Val(U:u);
                        Fatalf("binaryOp: bad operation: %v %v %v", x, op, y);
            panic("unreachable");

        });

        private static Val unaryOp(Op op, Val x, ptr<types.Type> _addr_t) => func((_, panic, __) =>
        {
            ref types.Type t = ref _addr_t.val;


            if (op == OPLUS) 

                if (x.Ctype() == CTINT || x.Ctype() == CTRUNE || x.Ctype() == CTFLT || x.Ctype() == CTCPLX) 
                    return x;
                            else if (op == ONEG) 

                if (x.Ctype() == CTINT || x.Ctype() == CTRUNE) 
                    ptr<Mpint> x = x.U._<ptr<Mpint>>();
                    ptr<Mpint> u = @new<Mpint>();
                    u.Rune = x.Rune;
                    u.Set(x);
                    u.Neg();
                    return new Val(U:u);
                else if (x.Ctype() == CTFLT) 
                    x = x.U._<ptr<Mpflt>>();
                    u = newMpflt();
                    u.Set(x);
                    u.Neg();
                    return new Val(U:u);
                else if (x.Ctype() == CTCPLX) 
                    x = x.U._<ptr<Mpcplx>>();
                    u = newMpcmplx();
                    u.Real.Set(_addr_x.Real);
                    u.Imag.Set(_addr_x.Imag);
                    u.Real.Neg();
                    u.Imag.Neg();
                    return new Val(U:u);
                            else if (op == OBITNOT) 

                if (x.Ctype() == CTINT || x.Ctype() == CTRUNE) 
                    x = x.U._<ptr<Mpint>>();

                    u = @new<Mpint>();
                    u.Rune = x.Rune;
                    if (t.IsSigned() || t.IsUntyped())
                    { 
                        // Signed values change sign.
                        u.SetInt64(-1L);

                    }
                    else
                    { 
                        // Unsigned values invert their bits.
                        u.Set(maxintval[t.Etype]);

                    }

                    u.Xor(x);
                    return new Val(U:u);
                else if (x.Ctype() == CTFLT) 
                    // TODO(mdempsky): Move to typecheck; see #31060.
                    yyerror("invalid operation: operator %v not defined on untyped float", op);
                    return new Val();
                else if (x.Ctype() == CTCPLX) 
                    // TODO(mdempsky): Move to typecheck; see #31060.
                    yyerror("invalid operation: operator %v not defined on untyped complex", op);
                    return new Val();
                            else if (op == ONOT) 
                return new Val(U:!x.U.(bool));
                        Fatalf("unaryOp: bad operation: %v %v", op, x);
            panic("unreachable");

        });

        private static Val shiftOp(Val x, Op op, Val y) => func((_, panic, __) =>
        {
            if (x.Ctype() != CTRUNE)
            {
                x = toint(x);
            }

            y = toint(y);

            ptr<Mpint> u = @new<Mpint>();
            u.Set(x.U._<ptr<Mpint>>());
            u.Rune = x.U._<ptr<Mpint>>().Rune;

            if (op == OLSH) 
                u.Lsh(y.U._<ptr<Mpint>>());
            else if (op == ORSH) 
                u.Rsh(y.U._<ptr<Mpint>>());
            else 
                Fatalf("shiftOp: bad operator: %v", op);
                panic("unreachable");
                        return new Val(U:u);

        });

        // setconst rewrites n as an OLITERAL with value v.
        private static void setconst(ptr<Node> _addr_n, Val v)
        {
            ref Node n = ref _addr_n.val;
 
            // If constant folding failed, mark n as broken and give up.
            if (v.U == null)
            {
                n.Type = null;
                return ;
            } 

            // Ensure n.Orig still points to a semantically-equivalent
            // expression after we rewrite n into a constant.
            if (n.Orig == n)
            {
                n.Orig = n.sepcopy();
            }

            n = new Node(Op:OLITERAL,Pos:n.Pos,Orig:n.Orig,Type:n.Type,Xoffset:BADWIDTH,);
            n.SetVal(v);
            if (n.Type.IsUntyped())
            { 
                // TODO(mdempsky): Make typecheck responsible for setting
                // the correct untyped type.
                n.Type = idealType(v.Ctype());

            } 

            // Check range.
            var lno = setlineno(n);
            overflow(v, _addr_n.Type);
            lineno = lno;

            if (!n.Type.IsUntyped())
            {

                // Truncate precision for non-ideal float.
                if (v.Ctype() == CTFLT) 
                    n.SetVal(new Val(truncfltlit(v.U.(*Mpflt),n.Type))); 
                    // Truncate precision for non-ideal complex.
                else if (v.Ctype() == CTCPLX) 
                    n.SetVal(new Val(trunccmplxlit(v.U.(*Mpcplx),n.Type)));
                
            }

        }

        private static void setboolconst(ptr<Node> _addr_n, bool v)
        {
            ref Node n = ref _addr_n.val;

            setconst(_addr_n, new Val(U:v));
        }

        private static void setintconst(ptr<Node> _addr_n, long v)
        {
            ref Node n = ref _addr_n.val;

            ptr<Mpint> u = @new<Mpint>();
            u.SetInt64(v);
            setconst(_addr_n, new Val(u));
        }

        // nodlit returns a new untyped constant with value v.
        private static ptr<Node> nodlit(Val v)
        {
            var n = nod(OLITERAL, null, null);
            n.SetVal(v);
            n.Type = idealType(v.Ctype());
            return _addr_n!;
        }

        private static ptr<types.Type> idealType(Ctype ct)
        {

            if (ct == CTSTR) 
                return _addr_types.Idealstring!;
            else if (ct == CTBOOL) 
                return _addr_types.Idealbool!;
            else if (ct == CTINT) 
                return _addr_types.Idealint!;
            else if (ct == CTRUNE) 
                return _addr_types.Idealrune!;
            else if (ct == CTFLT) 
                return _addr_types.Idealfloat!;
            else if (ct == CTCPLX) 
                return _addr_types.Idealcomplex!;
            else if (ct == CTNIL) 
                return _addr_types.Types[TNIL]!;
                        Fatalf("unexpected Ctype: %v", ct);
            return _addr_null!;

        }

        // idealkind returns a constant kind like consttype
        // but for an arbitrary "ideal" (untyped constant) expression.
        private static Ctype idealkind(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n == null || !n.Type.IsUntyped())
            {
                return CTxxx;
            }


            if (n.Op == OLITERAL) 
                return n.Val().Ctype(); 

                // numeric kinds.
            else if (n.Op == OADD || n.Op == OAND || n.Op == OANDNOT || n.Op == OBITNOT || n.Op == ODIV || n.Op == ONEG || n.Op == OMOD || n.Op == OMUL || n.Op == OSUB || n.Op == OXOR || n.Op == OOR || n.Op == OPLUS) 
                var k1 = idealkind(_addr_n.Left);
                var k2 = idealkind(_addr_n.Right);
                if (k1 > k2)
                {
                    return k1;
                }
                else
                {
                    return k2;
                }

            else if (n.Op == OREAL || n.Op == OIMAG) 
                return CTFLT;
            else if (n.Op == OCOMPLEX) 
                return CTCPLX;
            else if (n.Op == OADDSTR) 
                return CTSTR;
            else if (n.Op == OANDAND || n.Op == OEQ || n.Op == OGE || n.Op == OGT || n.Op == OLE || n.Op == OLT || n.Op == ONE || n.Op == ONOT || n.Op == OOROR) 
                return CTBOOL; 

                // shifts (beware!).
            else if (n.Op == OLSH || n.Op == ORSH) 
                return idealkind(_addr_n.Left);
            else 
                return CTxxx;
            
        }

        // defaultlit on both nodes simultaneously;
        // if they're both ideal going in they better
        // get the same type going out.
        // force means must assign concrete (non-ideal) type.
        // The results of defaultlit2 MUST be assigned back to l and r, e.g.
        //     n.Left, n.Right = defaultlit2(n.Left, n.Right, force)
        private static (ptr<Node>, ptr<Node>) defaultlit2(ptr<Node> _addr_l, ptr<Node> _addr_r, bool force)
        {
            ptr<Node> _p0 = default!;
            ptr<Node> _p0 = default!;
            ref Node l = ref _addr_l.val;
            ref Node r = ref _addr_r.val;

            if (l.Type == null || r.Type == null)
            {
                return (_addr_l!, _addr_r!);
            }

            if (!l.Type.IsUntyped())
            {
                r = convlit(_addr_r, _addr_l.Type);
                return (_addr_l!, _addr_r!);
            }

            if (!r.Type.IsUntyped())
            {
                l = convlit(_addr_l, _addr_r.Type);
                return (_addr_l!, _addr_r!);
            }

            if (!force)
            {
                return (_addr_l!, _addr_r!);
            } 

            // Can't mix bool with non-bool, string with non-string, or nil with anything (untyped).
            if (l.Type.IsBoolean() != r.Type.IsBoolean())
            {
                return (_addr_l!, _addr_r!);
            }

            if (l.Type.IsString() != r.Type.IsString())
            {
                return (_addr_l!, _addr_r!);
            }

            if (l.isNil() || r.isNil())
            {
                return (_addr_l!, _addr_r!);
            }

            var k = idealkind(_addr_l);
            {
                var rk = idealkind(_addr_r);

                if (rk > k)
                {
                    k = rk;
                }

            }

            var t = defaultType(k);
            l = convlit(_addr_l, _addr_t);
            r = convlit(_addr_r, _addr_t);
            return (_addr_l!, _addr_r!);

        }

        private static ptr<types.Type> defaultType(Ctype k)
        {

            if (k == CTBOOL) 
                return _addr_types.Types[TBOOL]!;
            else if (k == CTSTR) 
                return _addr_types.Types[TSTRING]!;
            else if (k == CTINT) 
                return _addr_types.Types[TINT]!;
            else if (k == CTRUNE) 
                return _addr_types.Runetype!;
            else if (k == CTFLT) 
                return _addr_types.Types[TFLOAT64]!;
            else if (k == CTCPLX) 
                return _addr_types.Types[TCOMPLEX128]!;
                        Fatalf("bad idealkind: %v", k);
            return _addr_null!;

        }

        // strlit returns the value of a literal string Node as a string.
        private static @string strlit(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.Val().U._<@string>();
        }

        // TODO(gri) smallintconst is only used in one place - can we used indexconst?
        private static bool smallintconst(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Op == OLITERAL && Isconst(_addr_n, CTINT) && n.Type != null)
            {

                if (simtype[n.Type.Etype] == TINT8 || simtype[n.Type.Etype] == TUINT8 || simtype[n.Type.Etype] == TINT16 || simtype[n.Type.Etype] == TUINT16 || simtype[n.Type.Etype] == TINT32 || simtype[n.Type.Etype] == TUINT32 || simtype[n.Type.Etype] == TBOOL) 
                    return true;
                else if (simtype[n.Type.Etype] == TIDEAL || simtype[n.Type.Etype] == TINT64 || simtype[n.Type.Etype] == TUINT64 || simtype[n.Type.Etype] == TPTR) 
                    ptr<Mpint> (v, ok) = n.Val().U._<ptr<Mpint>>();
                    if (ok && v.Cmp(minintval[TINT32]) >= 0L && v.Cmp(maxintval[TINT32]) <= 0L)
                    {
                        return true;
                    }

                            }

            return false;

        }

        // indexconst checks if Node n contains a constant expression
        // representable as a non-negative int and returns its value.
        // If n is not a constant expression, not representable as an
        // integer, or negative, it returns -1. If n is too large, it
        // returns -2.
        private static long indexconst(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n.Op != OLITERAL)
            {
                return -1L;
            }

            var v = toint(n.Val()); // toint returns argument unchanged if not representable as an *Mpint
            ptr<Mpint> (vi, ok) = v.U._<ptr<Mpint>>();
            if (!ok || vi.CmpInt64(0L) < 0L)
            {
                return -1L;
            }

            if (vi.Cmp(maxintval[TINT]) > 0L)
            {
                return -2L;
            }

            return vi.Int64();

        }

        // isGoConst reports whether n is a Go language constant (as opposed to a
        // compile-time constant).
        //
        // Expressions derived from nil, like string([]byte(nil)), while they
        // may be known at compile time, are not Go language constants.
        private static bool isGoConst(this ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            return n.Op == OLITERAL && n.Val().Ctype() != CTNIL;
        }

        private static bool hascallchan(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            if (n == null)
            {
                return false;
            }


            if (n.Op == OAPPEND || n.Op == OCALL || n.Op == OCALLFUNC || n.Op == OCALLINTER || n.Op == OCALLMETH || n.Op == OCAP || n.Op == OCLOSE || n.Op == OCOMPLEX || n.Op == OCOPY || n.Op == ODELETE || n.Op == OIMAG || n.Op == OLEN || n.Op == OMAKE || n.Op == ONEW || n.Op == OPANIC || n.Op == OPRINT || n.Op == OPRINTN || n.Op == OREAL || n.Op == ORECOVER || n.Op == ORECV) 
                return true;
                        if (hascallchan(_addr_n.Left) || hascallchan(_addr_n.Right))
            {
                return true;
            }

            foreach (var (_, n1) in n.List.Slice())
            {
                if (hascallchan(_addr_n1))
                {
                    return true;
                }

            }
            foreach (var (_, n2) in n.Rlist.Slice())
            {
                if (hascallchan(_addr_n2))
                {
                    return true;
                }

            }
            return false;

        }

        // A constSet represents a set of Go constant expressions.
        private partial struct constSet
        {
            public map<constSetKey, src.XPos> m;
        }

        private partial struct constSetKey
        {
            public ptr<types.Type> typ;
        }

        // add adds constant expression n to s. If a constant expression of
        // equal value and identical type has already been added, then add
        // reports an error about the duplicate value.
        //
        // pos provides position information for where expression n occurred
        // (in case n does not have its own position information). what and
        // where are used in the error message.
        //
        // n must not be an untyped constant.
        private static void add(this ptr<constSet> _addr_s, src.XPos pos, ptr<Node> _addr_n, @string what, @string where)
        {
            ref constSet s = ref _addr_s.val;
            ref Node n = ref _addr_n.val;

            if (n.Op == OCONVIFACE && n.Implicit())
            {
                n = n.Left;
            }

            if (!n.isGoConst())
            {
                return ;
            }

            if (n.Type.IsUntyped())
            {
                Fatalf("%v is untyped", n);
            } 

            // Consts are only duplicates if they have the same value and
            // identical types.
            //
            // In general, we have to use types.Identical to test type
            // identity, because == gives false negatives for anonymous
            // types and the byte/uint8 and rune/int32 builtin type
            // aliases.  However, this is not a problem here, because
            // constant expressions are always untyped or have a named
            // type, and we explicitly handle the builtin type aliases
            // below.
            //
            // This approach may need to be revisited though if we fix
            // #21866 by treating all type aliases like byte/uint8 and
            // rune/int32.
            var typ = n.Type;

            if (typ == types.Bytetype) 
                typ = types.Types[TUINT8];
            else if (typ == types.Runetype) 
                typ = types.Types[TINT32];
                        constSetKey k = new constSetKey(typ,n.Val().Interface());

            if (hasUniquePos(n))
            {
                pos = n.Pos;
            }

            if (s.m == null)
            {
                s.m = make_map<constSetKey, src.XPos>();
            }

            {
                var (prevPos, isDup) = s.m[k];

                if (isDup)
                {
                    yyerrorl(pos, "duplicate %s %s in %s\n\tprevious %s at %v", what, nodeAndVal(_addr_n), where, what, linestr(prevPos));
                }
                else
                {
                    s.m[k] = pos;
                }

            }

        }

        // nodeAndVal reports both an expression and its constant value, if
        // the latter is non-obvious.
        //
        // TODO(mdempsky): This could probably be a fmt.go flag.
        private static @string nodeAndVal(ptr<Node> _addr_n)
        {
            ref Node n = ref _addr_n.val;

            var show = n.String();
            var val = n.Val().Interface();
            {
                var s = fmt.Sprintf("%#v", val);

                if (show != s)
                {
                    show += " (value " + s + ")";
                }

            }

            return show;

        }
    }
}}}}
