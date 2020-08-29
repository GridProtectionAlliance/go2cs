// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:26:34 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\const.go
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using big = go.math.big_package;
using strings = go.strings_package;
using static go.builtin;

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

        public static readonly Ctype CTxxx = iota;

        public static readonly var CTINT = 0;
        public static readonly var CTRUNE = 1;
        public static readonly var CTFLT = 2;
        public static readonly var CTCPLX = 3;
        public static readonly var CTSTR = 4;
        public static readonly var CTBOOL = 5;
        public static readonly var CTNIL = 6;

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
                case ref NilVal x:
                    return CTNIL;
                    break;
                case bool x:
                    return CTBOOL;
                    break;
                case ref Mpint x:
                    if (x.Rune)
                    {
                        return CTRUNE;
                    }
                    return CTINT;
                    break;
                case ref Mpflt x:
                    return CTFLT;
                    break;
                case ref Mpcplx x:
                    return CTCPLX;
                    break;
                case @string x:
                    return CTSTR;
                    break;
                default:
                {
                    var x = v.U.type();
                    Fatalf("unexpected Ctype for %T", v.U);
                    panic("not reached");
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
                case ref NilVal x:
                    return true;
                    break;
                case bool x:
                    bool y = b.U._<bool>();
                    return x == y;
                    break;
                case ref Mpint x:
                    y = b.U._<ref Mpint>();
                    return x.Cmp(y) == 0L;
                    break;
                case ref Mpflt x:
                    y = b.U._<ref Mpflt>();
                    return x.Cmp(y) == 0L;
                    break;
                case ref Mpcplx x:
                    y = b.U._<ref Mpcplx>();
                    return x.Real.Cmp(ref y.Real) == 0L && x.Imag.Cmp(ref y.Imag) == 0L;
                    break;
                case @string x:
                    y = b.U._<@string>();
                    return x == y;
                    break;
                default:
                {
                    var x = a.U.type();
                    Fatalf("unexpected Ctype for %T", a.U);
                    panic("not reached");
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
                case ref NilVal x:
                    return null;
                    break;
                case bool x:
                    return x;
                    break;
                case @string x:
                    return x;
                    break;
                case ref Mpint x:
                    return x.Int64();
                    break;
                case ref Mpflt x:
                    return x.Float64();
                    break;
                case ref Mpcplx x:
                    return complex(x.Real.Float64(), x.Imag.Float64());
                    break;
                default:
                {
                    var x = v.U.type();
                    Fatalf("unexpected Interface for %T", v.U);
                    panic("not reached");
                    break;
                }
            }
        });

        public partial struct NilVal
        {
        }

        // Int64 returns n as an int64.
        // n must be an integer or rune constant.
        private static long Int64(this ref Node n)
        {
            if (!Isconst(n, CTINT))
            {
                Fatalf("Int64(%v)", n);
            }
            return n.Val().U._<ref Mpint>().Int64();
        }

        // Bool returns n as a bool.
        // n must be a boolean constant.
        private static bool Bool(this ref Node n)
        {
            if (!Isconst(n, CTBOOL))
            {
                Fatalf("Bool(%v)", n);
            }
            return n.Val().U._<bool>();
        }

        // truncate float literal fv to 32-bit or 64-bit precision
        // according to type; return truncated value.
        private static ref Mpflt truncfltlit(ref Mpflt oldv, ref types.Type t)
        {
            if (t == null)
            {
                return oldv;
            }
            if (overflow(new Val(oldv), t))
            { 
                // If there was overflow, simply continuing would set the
                // value to Inf which in turn would lead to spurious follow-on
                // errors. Avoid this by returning the existing value.
                return oldv;
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
                        return fv;
        }

        // truncate Real and Imag parts of Mpcplx to 32-bit or 64-bit
        // precision, according to type; return truncated value. In case of
        // overflow, calls yyerror but does not truncate the input value.
        private static ref Mpcplx trunccmplxlit(ref Mpcplx oldv, ref types.Type t)
        {
            if (t == null)
            {
                return oldv;
            }
            if (overflow(new Val(oldv), t))
            { 
                // If there was overflow, simply continuing would set the
                // value to Inf which in turn would lead to spurious follow-on
                // errors. Avoid this by returning the existing value.
                return oldv;
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
                        return cv;
        }

        // canReuseNode indicates whether it is known to be safe
        // to reuse a Node.
        private partial struct canReuseNode // : bool
        {
        }

        private static readonly canReuseNode noReuse = false; // not necessarily safe to reuse
        private static readonly canReuseNode reuseOK = true; // safe to reuse

        // convert n, if literal, to type t.
        // implicit conversion.
        // The result of convlit MUST be assigned back to n, e.g.
        //     n.Left = convlit(n.Left, t)
        private static ref Node convlit(ref Node n, ref types.Type t)
        {
            return convlit1(n, t, false, noReuse);
        }

        // convlit1 converts n, if literal, to type t.
        // It returns a new node if necessary.
        // The result of convlit1 MUST be assigned back to n, e.g.
        //     n.Left = convlit1(n.Left, t, explicit, reuse)
        private static ref Node convlit1(ref Node n, ref types.Type t, bool @explicit, canReuseNode reuse)
        {
            if (n == null || t == null || n.Type == null || t.IsUntyped() || n.Type == t)
            {
                return n;
            }
            if (!explicit && !n.Type.IsUntyped())
            {
                return n;
            }
            if (n.Op == OLITERAL && !reuse)
            { 
                // Can't always set n.Type directly on OLITERAL nodes.
                // See discussion on CL 20813.
                var nn = n.Value;
                n = ref nn;
                reuse = true;
            }

            if (n.Op == OLITERAL) 
                if (!okforconst[t.Etype] && n.Type.Etype != TNIL)
                {
                    return defaultlitreuse(n, null, reuse);
                }
            else if (n.Op == OLSH || n.Op == ORSH) 
                n.Left = convlit1(n.Left, t, explicit && n.Left.Type.IsUntyped(), noReuse);
                t = n.Left.Type;
                if (t != null && t.Etype == TIDEAL && n.Val().Ctype() != CTINT)
                {
                    n.SetVal(toint(n.Val()));
                }
                if (t != null && !t.IsInteger())
                {
                    yyerror("invalid operation: %v (shift of type %v)", n, t);
                    t = null;
                }
                n.Type = t;
                return n;
            else if (n.Op == OCOMPLEX) 
                if (n.Type.Etype == TIDEAL)
                {

                    if (t.Etype == types.TCOMPLEX128) 
                        n.Type = t;
                        n.Left = convlit(n.Left, types.Types[TFLOAT64]);
                        n.Right = convlit(n.Right, types.Types[TFLOAT64]);
                    else if (t.Etype == TCOMPLEX64) 
                        n.Type = t;
                        n.Left = convlit(n.Left, types.Types[TFLOAT32]);
                        n.Right = convlit(n.Right, types.Types[TFLOAT32]);
                    else 
                        // If trying to convert to non-complex type,
                        // leave as complex128 and let typechecker complain.
                        t = types.Types[TCOMPLEX128];
                                    }
                return n;
            else 
                if (n.Type == types.Idealbool)
                {
                    if (t.IsBoolean())
                    {
                        n.Type = t;
                    }
                    else
                    {
                        n.Type = types.Types[TBOOL];
                    }
                }
                if (n.Type.Etype == TIDEAL)
                {
                    n.Left = convlit(n.Left, t);
                    n.Right = convlit(n.Right, t);
                    n.Type = t;
                }
                return n; 

                // target is invalid type for a constant? leave alone.
            // avoided repeated calculations, errors
            if (eqtype(n.Type, t))
            {
                return n;
            }
            var ct = consttype(n);
            types.EType et = default;
            if (ct == 0L)
            {
                goto bad;
            }
            et = t.Etype;
            if (et == TINTER)
            {
                if (ct == CTNIL && n.Type == types.Types[TNIL])
                {
                    n.Type = t;
                    return n;
                }
                return defaultlitreuse(n, null, reuse);
            }

            if (ct == CTNIL) 

                if (et == TSTRING) 
                    return n;
                else if (et == TARRAY) 
                    goto bad;
                else if (et == TPTR32 || et == TPTR64 || et == TINTER || et == TMAP || et == TCHAN || et == TFUNC || et == TSLICE || et == TUNSAFEPTR) 
                    break; 

                    // A nil literal may be converted to uintptr
                    // if it is an unsafe.Pointer
                else if (et == TUINTPTR) 
                    if (n.Type.Etype == TUNSAFEPTR)
                    {
                        ptr<Mpint> i = @new<Mpint>();
                        i.SetInt64(0L);
                        n.SetVal(new Val(i));
                    }
                    else
                    {
                        goto bad;
                    }
                else 
                    n.Type = null;
                    goto bad; 

                    // let normal conversion code handle it
                            else if (ct == CTSTR || ct == CTBOOL) 
                if (et != n.Type.Etype)
                {
                    goto bad;
                }
            else if (ct == CTINT || ct == CTRUNE || ct == CTFLT || ct == CTCPLX) 
                if (n.Type.Etype == TUNSAFEPTR && t.Etype != TUINTPTR)
                {
                    goto bad;
                }
                ct = n.Val().Ctype();
                if (isInt[et])
                {

                    if (ct == CTCPLX || ct == CTFLT || ct == CTRUNE)
                    {
                        n.SetVal(toint(n.Val()));
                        fallthrough = true;

                    }
                    if (fallthrough || ct == CTINT)
                    {
                        overflow(n.Val(), t);
                        goto __switch_break0;
                    }
                    // default: 
                        goto bad;

                    __switch_break0:;
                }
                else if (isFloat[et])
                {

                    if (ct == CTCPLX || ct == CTINT || ct == CTRUNE)
                    {
                        n.SetVal(toflt(n.Val()));
                        fallthrough = true;

                    }
                    if (fallthrough || ct == CTFLT)
                    {
                        n.SetVal(new Val(truncfltlit(n.Val().U.(*Mpflt),t)));
                        goto __switch_break1;
                    }
                    // default: 
                        goto bad;

                    __switch_break1:;
                }
                else if (isComplex[et])
                {

                    if (ct == CTFLT || ct == CTINT || ct == CTRUNE)
                    {
                        n.SetVal(tocplx(n.Val()));
                        fallthrough = true;

                    }
                    if (fallthrough || ct == CTCPLX)
                    {
                        n.SetVal(new Val(trunccmplxlit(n.Val().U.(*Mpcplx),t)));
                        goto __switch_break2;
                    }
                    // default: 
                        goto bad;

                    __switch_break2:;
                }
                else if (et == types.TSTRING && (ct == CTINT || ct == CTRUNE) && explicit)
                {
                    n.SetVal(tostr(n.Val()));
                }
                else
                {
                    goto bad;
                }
            else 
                goto bad;
                        n.Type = t;
            return n;

bad:

            if (!n.Diag())
            {
                if (!t.Broke())
                {
                    yyerror("cannot convert %L to type %v", n, t);
                }
                n.SetDiag(true);
            }
            if (n.Type.IsUntyped())
            {
                n = defaultlitreuse(n, null, reuse);
            }
            return n;
        }

        private static Val copyval(Val v)
        {
            switch (v.U.type())
            {
                case ref Mpint u:
                    ptr<Mpint> i = @new<Mpint>();
                    i.Set(u);
                    i.Rune = u.Rune;
                    v.U = i;
                    break;
                case ref Mpflt u:
                    var f = newMpflt();
                    f.Set(u);
                    v.U = f;
                    break;
                case ref Mpcplx u:
                    ptr<Mpcplx> c = @new<Mpcplx>();
                    c.Real.Set(ref u.Real);
                    c.Imag.Set(ref u.Imag);
                    v.U = c;
                    break;

            }

            return v;
        }

        private static Val tocplx(Val v)
        {
            switch (v.U.type())
            {
                case ref Mpint u:
                    ptr<Mpcplx> c = @new<Mpcplx>();
                    c.Real.SetInt(u);
                    c.Imag.SetFloat64(0.0F);
                    v.U = c;
                    break;
                case ref Mpflt u:
                    c = @new<Mpcplx>();
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
                case ref Mpint u:
                    var f = newMpflt();
                    f.SetInt(u);
                    v.U = f;
                    break;
                case ref Mpcplx u:
                    f = newMpflt();
                    f.Set(ref u.Real);
                    if (u.Imag.CmpFloat64(0L) != 0L)
                    {
                        yyerror("constant %v%vi truncated to real", fconv(ref u.Real, FmtSharp), fconv(ref u.Imag, FmtSharp | FmtSign));
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
                case ref Mpint u:
                    if (u.Rune)
                    {
                        ptr<Mpint> i = @new<Mpint>();
                        i.Set(u);
                        v.U = i;
                    }
                    break;
                case ref Mpflt u:
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
                            t.Parse(fconv(u, FmtSharp), 10L);
                            if (t.IsInt())
                            {
                                yyerror("constant truncated to integer");
                            }
                            else
                            {
                                yyerror("constant %v truncated to integer", fconv(u, FmtSharp));
                            }
                        }
                    }
                    v.U = i;
                    break;
                case ref Mpcplx u:
                    i = @new<Mpint>();
                    if (!i.SetFloat(ref u.Real) || u.Imag.CmpFloat64(0L) != 0L)
                    {
                        yyerror("constant %v%vi truncated to integer", fconv(ref u.Real, FmtSharp), fconv(ref u.Imag, FmtSharp | FmtSign));
                    }
                    v.U = i;
                    break;

            }

            return v;
        }

        private static bool doesoverflow(Val v, ref types.Type t)
        {
            switch (v.U.type())
            {
                case ref Mpint u:
                    if (!t.IsInteger())
                    {
                        Fatalf("overflow: %v integer constant", t);
                    }
                    return u.Cmp(minintval[t.Etype]) < 0L || u.Cmp(maxintval[t.Etype]) > 0L;
                    break;
                case ref Mpflt u:
                    if (!t.IsFloat())
                    {
                        Fatalf("overflow: %v floating-point constant", t);
                    }
                    return u.Cmp(minfltval[t.Etype]) <= 0L || u.Cmp(maxfltval[t.Etype]) >= 0L;
                    break;
                case ref Mpcplx u:
                    if (!t.IsComplex())
                    {
                        Fatalf("overflow: %v complex constant", t);
                    }
                    return u.Real.Cmp(minfltval[t.Etype]) <= 0L || u.Real.Cmp(maxfltval[t.Etype]) >= 0L || u.Imag.Cmp(minfltval[t.Etype]) <= 0L || u.Imag.Cmp(maxfltval[t.Etype]) >= 0L;
                    break;

            }

            return false;
        }

        private static bool overflow(Val v, ref types.Type t)
        { 
            // v has already been converted
            // to appropriate form for t.
            if (t == null || t.Etype == TIDEAL)
            {
                return false;
            } 

            // Only uintptrs may be converted to unsafe.Pointer, which cannot overflow.
            if (t.Etype == TUNSAFEPTR)
            {
                return false;
            }
            if (doesoverflow(v, t))
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
                case ref Mpint u:
                    long i = 0xFFFDUL;
                    if (u.Cmp(minintval[TUINT32]) >= 0L && u.Cmp(maxintval[TUINT32]) <= 0L)
                    {
                        i = u.Int64();
                    }
                    v.U = string(i);
                    break;
                case ref NilVal u:
                    v.U = "";
                    break;

            }

            return v;
        }

        private static Ctype consttype(ref Node n)
        {
            if (n == null || n.Op != OLITERAL)
            {
                return 0L;
            }
            return n.Val().Ctype();
        }

        public static bool Isconst(ref Node n, Ctype ct)
        {
            var t = consttype(n); 

            // If the caller is asking for CTINT, allow CTRUNE too.
            // Makes life easier for back ends.
            return t == ct || (ct == CTINT && t == CTRUNE);
        }

        private static ref Node saveorig(ref Node n)
        {
            if (n == n.Orig)
            { 
                // duplicate node for n->orig.
                var n1 = nod(OLITERAL, null, null);

                n.Orig = n1;
                n1.Value = n.Value;
            }
            return n.Orig;
        }

        // if n is constant, rewrite as OLITERAL node.
        private static void evconst(ref Node n)
        { 
            // pick off just the opcodes that can be
            // constant evaluated.

            if (n.Op == OADD || n.Op == OAND || n.Op == OANDAND || n.Op == OANDNOT || n.Op == OARRAYBYTESTR || n.Op == OCOM || n.Op == ODIV || n.Op == OEQ || n.Op == OGE || n.Op == OGT || n.Op == OLE || n.Op == OLSH || n.Op == OLT || n.Op == OMINUS || n.Op == OMOD || n.Op == OMUL || n.Op == ONE || n.Op == ONOT || n.Op == OOR || n.Op == OOROR || n.Op == OPLUS || n.Op == ORSH || n.Op == OSUB || n.Op == OXOR) 
                break;
            else if (n.Op == OCONV) 
                if (n.Type == null)
                {
                    return;
                }
                if (!okforconst[n.Type.Etype] && n.Type.Etype != TNIL)
                {
                    return;
                } 

                // merge adjacent constants in the argument list.
            else if (n.Op == OADDSTR) 
                var s = n.List.Slice();
                for (long i1 = 0L; i1 < len(s); i1++)
                {
                    if (Isconst(s[i1], CTSTR) && i1 + 1L < len(s) && Isconst(s[i1 + 1L], CTSTR))
                    { 
                        // merge from i1 up to but not including i2
                        slice<@string> strs = default;
                        var i2 = i1;
                        while (i2 < len(s) && Isconst(s[i2], CTSTR))
                        {
                            strs = append(strs, s[i2].Val().U._<@string>());
                            i2++;
                        }


                        var nl = s[i1].Value;
                        nl.Orig = ref nl;
                        nl.SetVal(new Val(strings.Join(strs,"")));
                        s[i1] = ref nl;
                        s = append(s[..i1 + 1L], s[i2..]);
                    }
                }


                if (len(s) == 1L && Isconst(s[0L], CTSTR))
                {
                    n.Op = OLITERAL;
                    n.SetVal(s[0L].Val());
                }
                else
                {
                    n.List.Set(s);
                }
                return;
            else 
                return;
                        nl = n.Left;
            if (nl == null || nl.Type == null)
            {
                return;
            }
            if (consttype(nl) == 0L)
            {
                return;
            }
            var wl = nl.Type.Etype;
            if (isInt[wl] || isFloat[wl] || isComplex[wl])
            {
                wl = TIDEAL;
            } 

            // avoid constant conversions in switches below
            const var CTINT_ = uint32(CTINT);
            const var CTRUNE_ = uint32(CTRUNE);
            const var CTFLT_ = uint32(CTFLT);
            const var CTCPLX_ = uint32(CTCPLX);
            const var CTSTR_ = uint32(CTSTR);
            const var CTBOOL_ = uint32(CTBOOL);
            const var CTNIL_ = uint32(CTNIL);
            const var OCONV_ = uint32(OCONV) << (int)(16L);
            const var OARRAYBYTESTR_ = uint32(OARRAYBYTESTR) << (int)(16L);
            const var OPLUS_ = uint32(OPLUS) << (int)(16L);
            const var OMINUS_ = uint32(OMINUS) << (int)(16L);
            const var OCOM_ = uint32(OCOM) << (int)(16L);
            const var ONOT_ = uint32(ONOT) << (int)(16L);
            const var OLSH_ = uint32(OLSH) << (int)(16L);
            const var ORSH_ = uint32(ORSH) << (int)(16L);
            const var OADD_ = uint32(OADD) << (int)(16L);
            const var OSUB_ = uint32(OSUB) << (int)(16L);
            const var OMUL_ = uint32(OMUL) << (int)(16L);
            const var ODIV_ = uint32(ODIV) << (int)(16L);
            const var OMOD_ = uint32(OMOD) << (int)(16L);
            const var OOR_ = uint32(OOR) << (int)(16L);
            const var OAND_ = uint32(OAND) << (int)(16L);
            const var OANDNOT_ = uint32(OANDNOT) << (int)(16L);
            const var OXOR_ = uint32(OXOR) << (int)(16L);
            const var OEQ_ = uint32(OEQ) << (int)(16L);
            const var ONE_ = uint32(ONE) << (int)(16L);
            const var OLT_ = uint32(OLT) << (int)(16L);
            const var OLE_ = uint32(OLE) << (int)(16L);
            const var OGE_ = uint32(OGE) << (int)(16L);
            const var OGT_ = uint32(OGT) << (int)(16L);
            const var OOROR_ = uint32(OOROR) << (int)(16L);
            const var OANDAND_ = uint32(OANDAND) << (int)(16L);

            var nr = n.Right;
            Val rv = default;
            src.XPos lno = default;
            types.EType wr = default;
            uint ctype = default;
            Val v = default;
            ref Node norig = default;
            ref Node nn = default;
            if (nr == null)
            { 
                // copy numeric value to avoid modifying
                // nl, in case someone still refers to it (e.g. iota).
                v = nl.Val();

                if (wl == TIDEAL)
                {
                    v = copyval(v);
                } 

                // rune values are int values for the purpose of constant folding.
                ctype = uint32(v.Ctype());
                if (ctype == CTRUNE_)
                {
                    ctype = CTINT_;
                }

                if (uint32(n.Op) << (int)(16L) | ctype == OCONV_ | CTNIL_ || uint32(n.Op) << (int)(16L) | ctype == OARRAYBYTESTR_ | CTNIL_)
                {
                    if (n.Type.IsString())
                    {
                        v = tostr(v);
                        nl.Type = n.Type;
                        break;
                    }
                    fallthrough = true;
                }
                if (fallthrough || uint32(n.Op) << (int)(16L) | ctype == OCONV_ | CTINT_ || uint32(n.Op) << (int)(16L) | ctype == OCONV_ | CTFLT_ || uint32(n.Op) << (int)(16L) | ctype == OCONV_ | CTCPLX_ || uint32(n.Op) << (int)(16L) | ctype == OCONV_ | CTSTR_ || uint32(n.Op) << (int)(16L) | ctype == OCONV_ | CTBOOL_)
                {
                    nl = convlit1(nl, n.Type, true, false);
                    v = nl.Val();
                    goto __switch_break3;
                }
                if (uint32(n.Op) << (int)(16L) | ctype == OPLUS_ | CTINT_)
                {
                    break;
                    goto __switch_break3;
                }
                if (uint32(n.Op) << (int)(16L) | ctype == OMINUS_ | CTINT_)
                {
                    v.U._<ref Mpint>().Neg();
                    goto __switch_break3;
                }
                if (uint32(n.Op) << (int)(16L) | ctype == OCOM_ | CTINT_)
                {
                    types.EType et = Txxx;
                    if (nl.Type != null)
                    {
                        et = nl.Type.Etype;
                    } 

                    // calculate the mask in b
                    // result will be (a ^ mask)
                    Mpint b = default;

                    // signed guys change sign
                    if (et == TUINT8 || et == TUINT16 || et == TUINT32 || et == TUINT64 || et == TUINT || et == TUINTPTR) 
                        b.Set(maxintval[et]);
                    else 
                        b.SetInt64(-1L); 

                        // unsigned guys invert their bits
                                        v.U._<ref Mpint>().Xor(ref b);
                    goto __switch_break3;
                }
                if (uint32(n.Op) << (int)(16L) | ctype == OPLUS_ | CTFLT_)
                {
                    break;
                    goto __switch_break3;
                }
                if (uint32(n.Op) << (int)(16L) | ctype == OMINUS_ | CTFLT_)
                {
                    v.U._<ref Mpflt>().Neg();
                    goto __switch_break3;
                }
                if (uint32(n.Op) << (int)(16L) | ctype == OPLUS_ | CTCPLX_)
                {
                    break;
                    goto __switch_break3;
                }
                if (uint32(n.Op) << (int)(16L) | ctype == OMINUS_ | CTCPLX_)
                {
                    v.U._<ref Mpcplx>().Real.Neg();
                    v.U._<ref Mpcplx>().Imag.Neg();
                    goto __switch_break3;
                }
                if (uint32(n.Op) << (int)(16L) | ctype == ONOT_ | CTBOOL_)
                {
                    if (!v.U._<bool>())
                    {
                        goto settrue;
                    }
                    goto setfalse;
                    goto __switch_break3;
                }
                // default: 
                    if (!n.Diag())
                    {
                        yyerror("illegal constant expression %v %v", n.Op, nl.Type);
                        n.SetDiag(true);
                    }
                    return;

                __switch_break3:;
                goto ret;
            }
            if (nr.Type == null)
            {
                return;
            }
            if (consttype(nr) == 0L)
            {
                return;
            }
            wr = nr.Type.Etype;
            if (isInt[wr] || isFloat[wr] || isComplex[wr])
            {
                wr = TIDEAL;
            } 

            // check for compatible general types (numeric, string, etc)
            if (wl != wr)
            {
                if (wl == TINTER || wr == TINTER)
                {
                    if (n.Op == ONE)
                    {
                        goto settrue;
                    }
                    goto setfalse;
                }
                goto illegal;
            } 

            // check for compatible types.

            // ideal const mixes with anything but otherwise must match.
            if (n.Op == OLSH || n.Op == ORSH) 
                nr = defaultlit(nr, types.Types[TUINT]);

                n.Right = nr;
                if (nr.Type != null && (nr.Type.IsSigned() || !nr.Type.IsInteger()))
                {
                    goto illegal;
                }
                if (nl.Val().Ctype() != CTRUNE)
                {
                    nl.SetVal(toint(nl.Val()));
                }
                nr.SetVal(toint(nr.Val()));
            else 
                if (nl.Type.Etype != TIDEAL)
                {
                    nr = defaultlit(nr, nl.Type);
                    n.Right = nr;
                }
                if (nr.Type.Etype != TIDEAL)
                {
                    nl = defaultlit(nl, nr.Type);
                    n.Left = nl;
                }
                if (nl.Type.Etype != nr.Type.Etype)
                {
                    goto illegal;
                } 

                // right must be unsigned.
                // left can be ideal.
            // copy numeric value to avoid modifying
            // n->left, in case someone still refers to it (e.g. iota).
            v = nl.Val();

            if (wl == TIDEAL)
            {
                v = copyval(v);
            }
            rv = nr.Val(); 

            // convert to common ideal
            if (v.Ctype() == CTCPLX || rv.Ctype() == CTCPLX)
            {
                v = tocplx(v);
                rv = tocplx(rv);
            }
            if (v.Ctype() == CTFLT || rv.Ctype() == CTFLT)
            {
                v = toflt(v);
                rv = toflt(rv);
            } 

            // Rune and int turns into rune.
            if (v.Ctype() == CTRUNE && rv.Ctype() == CTINT)
            {
                ptr<Mpint> i = @new<Mpint>();
                i.Set(rv.U._<ref Mpint>());
                i.Rune = true;
                rv.U = i;
            }
            if (v.Ctype() == CTINT && rv.Ctype() == CTRUNE)
            {
                if (n.Op == OLSH || n.Op == ORSH)
                {
                    i = @new<Mpint>();
                    i.Set(rv.U._<ref Mpint>());
                    rv.U = i;
                }
                else
                {
                    i = @new<Mpint>();
                    i.Set(v.U._<ref Mpint>());
                    i.Rune = true;
                    v.U = i;
                }
            }
            if (v.Ctype() != rv.Ctype())
            { 
                // Use of undefined name as constant?
                if ((v.Ctype() == 0L || rv.Ctype() == 0L) && nerrors > 0L)
                {
                    return;
                }
                Fatalf("constant type mismatch %v(%d) %v(%d)", nl.Type, v.Ctype(), nr.Type, rv.Ctype());
            } 

            // rune values are int values for the purpose of constant folding.
            ctype = uint32(v.Ctype());
            if (ctype == CTRUNE_)
            {
                ctype = CTINT_;
            } 

            // run op

            if (uint32(n.Op) << (int)(16L) | ctype == OADD_ | CTINT_) 
                v.U._<ref Mpint>().Add(rv.U._<ref Mpint>());
            else if (uint32(n.Op) << (int)(16L) | ctype == OSUB_ | CTINT_) 
                v.U._<ref Mpint>().Sub(rv.U._<ref Mpint>());
            else if (uint32(n.Op) << (int)(16L) | ctype == OMUL_ | CTINT_) 
                v.U._<ref Mpint>().Mul(rv.U._<ref Mpint>());
            else if (uint32(n.Op) << (int)(16L) | ctype == ODIV_ | CTINT_) 
                if (rv.U._<ref Mpint>().CmpInt64(0L) == 0L)
                {
                    yyerror("division by zero");
                    v.U._<ref Mpint>().SetOverflow();
                    break;
                }
                v.U._<ref Mpint>().Quo(rv.U._<ref Mpint>());
            else if (uint32(n.Op) << (int)(16L) | ctype == OMOD_ | CTINT_) 
                if (rv.U._<ref Mpint>().CmpInt64(0L) == 0L)
                {
                    yyerror("division by zero");
                    v.U._<ref Mpint>().SetOverflow();
                    break;
                }
                v.U._<ref Mpint>().Rem(rv.U._<ref Mpint>());
            else if (uint32(n.Op) << (int)(16L) | ctype == OLSH_ | CTINT_) 
                v.U._<ref Mpint>().Lsh(rv.U._<ref Mpint>());
            else if (uint32(n.Op) << (int)(16L) | ctype == ORSH_ | CTINT_) 
                v.U._<ref Mpint>().Rsh(rv.U._<ref Mpint>());
            else if (uint32(n.Op) << (int)(16L) | ctype == OOR_ | CTINT_) 
                v.U._<ref Mpint>().Or(rv.U._<ref Mpint>());
            else if (uint32(n.Op) << (int)(16L) | ctype == OAND_ | CTINT_) 
                v.U._<ref Mpint>().And(rv.U._<ref Mpint>());
            else if (uint32(n.Op) << (int)(16L) | ctype == OANDNOT_ | CTINT_) 
                v.U._<ref Mpint>().AndNot(rv.U._<ref Mpint>());
            else if (uint32(n.Op) << (int)(16L) | ctype == OXOR_ | CTINT_) 
                v.U._<ref Mpint>().Xor(rv.U._<ref Mpint>());
            else if (uint32(n.Op) << (int)(16L) | ctype == OADD_ | CTFLT_) 
                v.U._<ref Mpflt>().Add(rv.U._<ref Mpflt>());
            else if (uint32(n.Op) << (int)(16L) | ctype == OSUB_ | CTFLT_) 
                v.U._<ref Mpflt>().Sub(rv.U._<ref Mpflt>());
            else if (uint32(n.Op) << (int)(16L) | ctype == OMUL_ | CTFLT_) 
                v.U._<ref Mpflt>().Mul(rv.U._<ref Mpflt>());
            else if (uint32(n.Op) << (int)(16L) | ctype == ODIV_ | CTFLT_) 
                if (rv.U._<ref Mpflt>().CmpFloat64(0L) == 0L)
                {
                    yyerror("division by zero");
                    v.U._<ref Mpflt>().SetFloat64(1.0F);
                    break;
                }
                v.U._<ref Mpflt>().Quo(rv.U._<ref Mpflt>()); 

                // The default case above would print 'ideal % ideal',
                // which is not quite an ideal error.
            else if (uint32(n.Op) << (int)(16L) | ctype == OMOD_ | CTFLT_) 
                if (!n.Diag())
                {
                    yyerror("illegal constant expression: floating-point %% operation");
                    n.SetDiag(true);
                }
                return;
            else if (uint32(n.Op) << (int)(16L) | ctype == OADD_ | CTCPLX_) 
                v.U._<ref Mpcplx>().Real.Add(ref rv.U._<ref Mpcplx>().Real);
                v.U._<ref Mpcplx>().Imag.Add(ref rv.U._<ref Mpcplx>().Imag);
            else if (uint32(n.Op) << (int)(16L) | ctype == OSUB_ | CTCPLX_) 
                v.U._<ref Mpcplx>().Real.Sub(ref rv.U._<ref Mpcplx>().Real);
                v.U._<ref Mpcplx>().Imag.Sub(ref rv.U._<ref Mpcplx>().Imag);
            else if (uint32(n.Op) << (int)(16L) | ctype == OMUL_ | CTCPLX_) 
                cmplxmpy(v.U._<ref Mpcplx>(), rv.U._<ref Mpcplx>());
            else if (uint32(n.Op) << (int)(16L) | ctype == ODIV_ | CTCPLX_) 
                if (!cmplxdiv(v.U._<ref Mpcplx>(), rv.U._<ref Mpcplx>()))
                {
                    yyerror("complex division by zero");
                    rv.U._<ref Mpcplx>().Real.SetFloat64(1.0F);
                    rv.U._<ref Mpcplx>().Imag.SetFloat64(0.0F);
                    break;
                }
            else if (uint32(n.Op) << (int)(16L) | ctype == OEQ_ | CTNIL_) 
                goto settrue;
            else if (uint32(n.Op) << (int)(16L) | ctype == ONE_ | CTNIL_) 
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == OEQ_ | CTINT_) 
                if (v.U._<ref Mpint>().Cmp(rv.U._<ref Mpint>()) == 0L)
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == ONE_ | CTINT_) 
                if (v.U._<ref Mpint>().Cmp(rv.U._<ref Mpint>()) != 0L)
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == OLT_ | CTINT_) 
                if (v.U._<ref Mpint>().Cmp(rv.U._<ref Mpint>()) < 0L)
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == OLE_ | CTINT_) 
                if (v.U._<ref Mpint>().Cmp(rv.U._<ref Mpint>()) <= 0L)
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == OGE_ | CTINT_) 
                if (v.U._<ref Mpint>().Cmp(rv.U._<ref Mpint>()) >= 0L)
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == OGT_ | CTINT_) 
                if (v.U._<ref Mpint>().Cmp(rv.U._<ref Mpint>()) > 0L)
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == OEQ_ | CTFLT_) 
                if (v.U._<ref Mpflt>().Cmp(rv.U._<ref Mpflt>()) == 0L)
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == ONE_ | CTFLT_) 
                if (v.U._<ref Mpflt>().Cmp(rv.U._<ref Mpflt>()) != 0L)
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == OLT_ | CTFLT_) 
                if (v.U._<ref Mpflt>().Cmp(rv.U._<ref Mpflt>()) < 0L)
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == OLE_ | CTFLT_) 
                if (v.U._<ref Mpflt>().Cmp(rv.U._<ref Mpflt>()) <= 0L)
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == OGE_ | CTFLT_) 
                if (v.U._<ref Mpflt>().Cmp(rv.U._<ref Mpflt>()) >= 0L)
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == OGT_ | CTFLT_) 
                if (v.U._<ref Mpflt>().Cmp(rv.U._<ref Mpflt>()) > 0L)
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == OEQ_ | CTCPLX_) 
                if (v.U._<ref Mpcplx>().Real.Cmp(ref rv.U._<ref Mpcplx>().Real) == 0L && v.U._<ref Mpcplx>().Imag.Cmp(ref rv.U._<ref Mpcplx>().Imag) == 0L)
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == ONE_ | CTCPLX_) 
                if (v.U._<ref Mpcplx>().Real.Cmp(ref rv.U._<ref Mpcplx>().Real) != 0L || v.U._<ref Mpcplx>().Imag.Cmp(ref rv.U._<ref Mpcplx>().Imag) != 0L)
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == OEQ_ | CTSTR_) 
                if (strlit(nl) == strlit(nr))
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == ONE_ | CTSTR_) 
                if (strlit(nl) != strlit(nr))
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == OLT_ | CTSTR_) 
                if (strlit(nl) < strlit(nr))
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == OLE_ | CTSTR_) 
                if (strlit(nl) <= strlit(nr))
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == OGE_ | CTSTR_) 
                if (strlit(nl) >= strlit(nr))
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == OGT_ | CTSTR_) 
                if (strlit(nl) > strlit(nr))
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == OOROR_ | CTBOOL_) 
                if (v.U._<bool>() || rv.U._<bool>())
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == OANDAND_ | CTBOOL_) 
                if (v.U._<bool>() && rv.U._<bool>())
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == OEQ_ | CTBOOL_) 
                if (v.U._<bool>() == rv.U._<bool>())
                {
                    goto settrue;
                }
                goto setfalse;
            else if (uint32(n.Op) << (int)(16L) | ctype == ONE_ | CTBOOL_) 
                if (v.U._<bool>() != rv.U._<bool>())
                {
                    goto settrue;
                }
                goto setfalse;
            else 
                goto illegal;
            ret:
            norig = saveorig(n);
            n.Value = nl.Value; 

            // restore value of n->orig.
            n.Orig = norig;

            n.SetVal(v); 

            // check range.
            lno = setlineno(n);
            overflow(v, n.Type);
            lineno = lno; 

            // truncate precision for non-ideal float.
            if (v.Ctype() == CTFLT && n.Type.Etype != TIDEAL)
            {
                n.SetVal(new Val(truncfltlit(v.U.(*Mpflt),n.Type)));
            }
            return;

settrue:
            nn = nodbool(true);
            nn.Orig = saveorig(n);
            if (!iscmp[n.Op])
            {
                nn.Type = nl.Type;
            }
            n.Value = nn.Value;
            return;

setfalse:
            nn = nodbool(false);
            nn.Orig = saveorig(n);
            if (!iscmp[n.Op])
            {
                nn.Type = nl.Type;
            }
            n.Value = nn.Value;
            return;

illegal:
            if (!n.Diag())
            {
                yyerror("illegal constant expression: %v %v %v", nl.Type, n.Op, nr.Type);
                n.SetDiag(true);
            }
        }

        private static ref Node nodlit(Val v)
        {
            var n = nod(OLITERAL, null, null);
            n.SetVal(v);

            if (v.Ctype() == CTSTR) 
                n.Type = types.Idealstring;
            else if (v.Ctype() == CTBOOL) 
                n.Type = types.Idealbool;
            else if (v.Ctype() == CTINT || v.Ctype() == CTRUNE || v.Ctype() == CTFLT || v.Ctype() == CTCPLX) 
                n.Type = types.Types[TIDEAL];
            else if (v.Ctype() == CTNIL) 
                n.Type = types.Types[TNIL];
            else 
                Fatalf("nodlit ctype %d", v.Ctype());
                        return n;
        }

        private static ref Node nodcplxlit(Val r, Val i)
        {
            r = toflt(r);
            i = toflt(i);

            ptr<Mpcplx> c = @new<Mpcplx>();
            var n = nod(OLITERAL, null, null);
            n.Type = types.Types[TIDEAL];
            n.SetVal(new Val(c));

            if (r.Ctype() != CTFLT || i.Ctype() != CTFLT)
            {
                Fatalf("nodcplxlit ctype %d/%d", r.Ctype(), i.Ctype());
            }
            c.Real.Set(r.U._<ref Mpflt>());
            c.Imag.Set(i.U._<ref Mpflt>());
            return n;
        }

        // idealkind returns a constant kind like consttype
        // but for an arbitrary "ideal" (untyped constant) expression.
        private static Ctype idealkind(ref Node n)
        {
            if (n == null || !n.Type.IsUntyped())
            {
                return CTxxx;
            }

            if (n.Op == OLITERAL) 
                return n.Val().Ctype(); 

                // numeric kinds.
            else if (n.Op == OADD || n.Op == OAND || n.Op == OANDNOT || n.Op == OCOM || n.Op == ODIV || n.Op == OMINUS || n.Op == OMOD || n.Op == OMUL || n.Op == OSUB || n.Op == OXOR || n.Op == OOR || n.Op == OPLUS) 
                var k1 = idealkind(n.Left);

                var k2 = idealkind(n.Right);
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
            else if (n.Op == OANDAND || n.Op == OEQ || n.Op == OGE || n.Op == OGT || n.Op == OLE || n.Op == OLT || n.Op == ONE || n.Op == ONOT || n.Op == OOROR || n.Op == OCMPSTR || n.Op == OCMPIFACE) 
                return CTBOOL; 

                // shifts (beware!).
            else if (n.Op == OLSH || n.Op == ORSH) 
                return idealkind(n.Left);
            else 
                return CTxxx;
                    }

        // The result of defaultlit MUST be assigned back to n, e.g.
        //     n.Left = defaultlit(n.Left, t)
        private static ref Node defaultlit(ref Node n, ref types.Type t)
        {
            return defaultlitreuse(n, t, noReuse);
        }

        // The result of defaultlitreuse MUST be assigned back to n, e.g.
        //     n.Left = defaultlitreuse(n.Left, t, reuse)
        private static ref Node defaultlitreuse(ref Node n, ref types.Type t, canReuseNode reuse)
        {
            if (n == null || !n.Type.IsUntyped())
            {
                return n;
            }
            if (n.Op == OLITERAL && !reuse)
            {
                var nn = n.Value;
                n = ref nn;
                reuse = true;
            }
            var lno = setlineno(n);
            var ctype = idealkind(n);
            ref types.Type t1 = default;

            if (ctype == CTxxx) 
                Fatalf("defaultlit: idealkind is CTxxx: %+v", n);
            else if (ctype == CTBOOL) 
                t1 = types.Types[TBOOL];
                if (t != null && t.IsBoolean())
                {
                    t1 = t;
                }
                n = convlit1(n, t1, false, reuse);
            else if (ctype == CTINT) 
                t1 = types.Types[TINT];
                goto num;
            else if (ctype == CTRUNE) 
                t1 = types.Runetype;
                goto num;
            else if (ctype == CTFLT) 
                t1 = types.Types[TFLOAT64];
                goto num;
            else if (ctype == CTCPLX) 
                t1 = types.Types[TCOMPLEX128];
                goto num;
            else 
                if (t != null)
                {
                    return convlit(n, t);
                }

                if (n.Val().Ctype() == CTNIL) 
                    lineno = lno;
                    if (!n.Diag())
                    {
                        yyerror("use of untyped nil");
                        n.SetDiag(true);
                    }
                    n.Type = null;
                else if (n.Val().Ctype() == CTSTR) 
                    t1 = types.Types[TSTRING];
                    n = convlit1(n, t1, false, reuse);
                else 
                    yyerror("defaultlit: unknown literal: %v", n);
                                        lineno = lno;
            return n;

num:
            var v1 = n.Val();
            if (t != null)
            {
                if (t.IsInteger())
                {
                    t1 = t;
                    v1 = toint(n.Val());
                }
                else if (t.IsFloat())
                {
                    t1 = t;
                    v1 = toflt(n.Val());
                }
                else if (t.IsComplex())
                {
                    t1 = t;
                    v1 = tocplx(n.Val());
                }
                if (n.Val().Ctype() != CTxxx)
                {
                    n.SetVal(v1);
                }
            }
            if (n.Val().Ctype() != CTxxx)
            {
                overflow(n.Val(), t1);
            }
            n = convlit1(n, t1, false, reuse);
            lineno = lno;
            return n;
        }

        // defaultlit on both nodes simultaneously;
        // if they're both ideal going in they better
        // get the same type going out.
        // force means must assign concrete (non-ideal) type.
        // The results of defaultlit2 MUST be assigned back to l and r, e.g.
        //     n.Left, n.Right = defaultlit2(n.Left, n.Right, force)
        private static (ref Node, ref Node) defaultlit2(ref Node l, ref Node r, bool force)
        {
            if (l.Type == null || r.Type == null)
            {
                return (l, r);
            }
            if (!l.Type.IsUntyped())
            {
                r = convlit(r, l.Type);
                return (l, r);
            }
            if (!r.Type.IsUntyped())
            {
                l = convlit(l, r.Type);
                return (l, r);
            }
            if (!force)
            {
                return (l, r);
            }
            if (l.Type.IsBoolean())
            {
                l = convlit(l, types.Types[TBOOL]);
                r = convlit(r, types.Types[TBOOL]);
            }
            var lkind = idealkind(l);
            var rkind = idealkind(r);
            if (lkind == CTCPLX || rkind == CTCPLX)
            {
                l = convlit(l, types.Types[TCOMPLEX128]);
                r = convlit(r, types.Types[TCOMPLEX128]);
                return (l, r);
            }
            if (lkind == CTFLT || rkind == CTFLT)
            {
                l = convlit(l, types.Types[TFLOAT64]);
                r = convlit(r, types.Types[TFLOAT64]);
                return (l, r);
            }
            if (lkind == CTRUNE || rkind == CTRUNE)
            {
                l = convlit(l, types.Runetype);
                r = convlit(r, types.Runetype);
                return (l, r);
            }
            l = convlit(l, types.Types[TINT]);
            r = convlit(r, types.Types[TINT]);

            return (l, r);
        }

        // strlit returns the value of a literal string Node as a string.
        private static @string strlit(ref Node n)
        {
            return n.Val().U._<@string>();
        }

        private static bool smallintconst(ref Node n)
        {
            if (n.Op == OLITERAL && Isconst(n, CTINT) && n.Type != null)
            {

                if (simtype[n.Type.Etype] == TINT8 || simtype[n.Type.Etype] == TUINT8 || simtype[n.Type.Etype] == TINT16 || simtype[n.Type.Etype] == TUINT16 || simtype[n.Type.Etype] == TINT32 || simtype[n.Type.Etype] == TUINT32 || simtype[n.Type.Etype] == TBOOL || simtype[n.Type.Etype] == TPTR32) 
                    return true;
                else if (simtype[n.Type.Etype] == TIDEAL || simtype[n.Type.Etype] == TINT64 || simtype[n.Type.Etype] == TUINT64 || simtype[n.Type.Etype] == TPTR64) 
                    ref Mpint (v, ok) = n.Val().U._<ref Mpint>();
                    if (ok && v.Cmp(minintval[TINT32]) > 0L && v.Cmp(maxintval[TINT32]) < 0L)
                    {
                        return true;
                    }
                            }
            return false;
        }

        // nonnegintconst checks if Node n contains a constant expression
        // representable as a non-negative small integer, and returns its
        // (integer) value if that's the case. Otherwise, it returns -1.
        private static long nonnegintconst(ref Node n)
        {
            if (n.Op != OLITERAL)
            {
                return -1L;
            } 

            // toint will leave n.Val unchanged if it's not castable to an
            // Mpint, so we still have to guard the conversion.
            var v = toint(n.Val());
            ref Mpint (vi, ok) = v.U._<ref Mpint>();
            if (!ok || vi.CmpInt64(0L) < 0L || vi.Cmp(maxintval[TINT32]) > 0L)
            {
                return -1L;
            }
            return vi.Int64();
        }

        // complex multiply v *= rv
        //    (a, b) * (c, d) = (a*c - b*d, b*c + a*d)
        private static void cmplxmpy(ref Mpcplx v, ref Mpcplx rv)
        {
            Mpflt ac = default;
            Mpflt bd = default;
            Mpflt bc = default;
            Mpflt ad = default;

            ac.Set(ref v.Real);
            ac.Mul(ref rv.Real); // ac

            bd.Set(ref v.Imag);

            bd.Mul(ref rv.Imag); // bd

            bc.Set(ref v.Imag);

            bc.Mul(ref rv.Real); // bc

            ad.Set(ref v.Real);

            ad.Mul(ref rv.Imag); // ad

            v.Real.Set(ref ac);

            v.Real.Sub(ref bd); // ac-bd

            v.Imag.Set(ref bc);

            v.Imag.Add(ref ad); // bc+ad
        }

        // complex divide v /= rv
        //    (a, b) / (c, d) = ((a*c + b*d), (b*c - a*d))/(c*c + d*d)
        private static bool cmplxdiv(ref Mpcplx v, ref Mpcplx rv)
        {
            if (rv.Real.CmpFloat64(0L) == 0L && rv.Imag.CmpFloat64(0L) == 0L)
            {
                return false;
            }
            Mpflt ac = default;
            Mpflt bd = default;
            Mpflt bc = default;
            Mpflt ad = default;
            Mpflt cc_plus_dd = default;

            cc_plus_dd.Set(ref rv.Real);

            cc_plus_dd.Mul(ref rv.Real); // cc

            ac.Set(ref rv.Imag);

            ac.Mul(ref rv.Imag); // dd

            cc_plus_dd.Add(ref ac); // cc+dd

            // We already checked that c and d are not both zero, but we can't
            // assume that c²+d² != 0 follows, because for tiny values of c
            // and/or d c²+d² can underflow to zero.  Check that c²+d² is
            // nonzero,return if it's not.
            if (cc_plus_dd.CmpFloat64(0L) == 0L)
            {
                return false;
            }
            ac.Set(ref v.Real);

            ac.Mul(ref rv.Real); // ac

            bd.Set(ref v.Imag);

            bd.Mul(ref rv.Imag); // bd

            bc.Set(ref v.Imag);

            bc.Mul(ref rv.Real); // bc

            ad.Set(ref v.Real);

            ad.Mul(ref rv.Imag); // ad

            v.Real.Set(ref ac);

            v.Real.Add(ref bd); // ac+bd
            v.Real.Quo(ref cc_plus_dd); // (ac+bd)/(cc+dd)

            v.Imag.Set(ref bc);

            v.Imag.Sub(ref ad); // bc-ad
            v.Imag.Quo(ref cc_plus_dd); // (bc+ad)/(cc+dd)

            return true;
        }

        // Is n a Go language constant (as opposed to a compile-time constant)?
        // Expressions derived from nil, like string([]byte(nil)), while they
        // may be known at compile time, are not Go language constants.
        // Only called for expressions known to evaluated to compile-time
        // constants.
        private static bool isgoconst(ref Node n)
        {
            if (n.Orig != null)
            {
                n = n.Orig;
            }

            if (n.Op == OADD || n.Op == OADDSTR || n.Op == OAND || n.Op == OANDAND || n.Op == OANDNOT || n.Op == OCOM || n.Op == ODIV || n.Op == OEQ || n.Op == OGE || n.Op == OGT || n.Op == OLE || n.Op == OLSH || n.Op == OLT || n.Op == OMINUS || n.Op == OMOD || n.Op == OMUL || n.Op == ONE || n.Op == ONOT || n.Op == OOR || n.Op == OOROR || n.Op == OPLUS || n.Op == ORSH || n.Op == OSUB || n.Op == OXOR || n.Op == OIOTA || n.Op == OCOMPLEX || n.Op == OREAL || n.Op == OIMAG) 
                if (isgoconst(n.Left) && (n.Right == null || isgoconst(n.Right)))
                {
                    return true;
                }
            else if (n.Op == OCONV) 
                if (okforconst[n.Type.Etype] && isgoconst(n.Left))
                {
                    return true;
                }
            else if (n.Op == OLEN || n.Op == OCAP) 
                var l = n.Left;
                if (isgoconst(l))
                {
                    return true;
                } 

                // Special case: len/cap is constant when applied to array or
                // pointer to array when the expression does not contain
                // function calls or channel receive operations.
                var t = l.Type;

                if (t != null && t.IsPtr())
                {
                    t = t.Elem();
                }
                if (t != null && t.IsArray() && !hascallchan(l))
                {
                    return true;
                }
            else if (n.Op == OLITERAL) 
                if (n.Val().Ctype() != CTNIL)
                {
                    return true;
                }
            else if (n.Op == ONAME) 
                l = asNode(n.Sym.Def);
                if (l != null && l.Op == OLITERAL && n.Val().Ctype() != CTNIL)
                {
                    return true;
                }
            else if (n.Op == ONONAME) 
                if (asNode(n.Sym.Def) != null && asNode(n.Sym.Def).Op == OIOTA)
                {
                    return true;
                }
            else if (n.Op == OALIGNOF || n.Op == OOFFSETOF || n.Op == OSIZEOF) 
                return true;
            //dump("nonconst", n);
            return false;
        }

        private static bool hascallchan(ref Node n)
        {
            if (n == null)
            {
                return false;
            }

            if (n.Op == OAPPEND || n.Op == OCALL || n.Op == OCALLFUNC || n.Op == OCALLINTER || n.Op == OCALLMETH || n.Op == OCAP || n.Op == OCLOSE || n.Op == OCOMPLEX || n.Op == OCOPY || n.Op == ODELETE || n.Op == OIMAG || n.Op == OLEN || n.Op == OMAKE || n.Op == ONEW || n.Op == OPANIC || n.Op == OPRINT || n.Op == OPRINTN || n.Op == OREAL || n.Op == ORECOVER || n.Op == ORECV) 
                return true;
                        if (hascallchan(n.Left) || hascallchan(n.Right))
            {
                return true;
            }
            foreach (var (_, n1) in n.List.Slice())
            {
                if (hascallchan(n1))
                {
                    return true;
                }
            }
            foreach (var (_, n2) in n.Rlist.Slice())
            {
                if (hascallchan(n2))
                {
                    return true;
                }
            }
            return false;
        }
    }
}}}}
