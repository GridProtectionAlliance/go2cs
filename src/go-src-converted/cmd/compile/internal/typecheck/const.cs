// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package typecheck -- go2cs converted at 2022 March 06 22:48:05 UTC
// import "cmd/compile/internal/typecheck" ==> using typecheck = go.cmd.compile.@internal.typecheck_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\typecheck\const.go
using fmt = go.fmt_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using math = go.math_package;
using big = go.math.big_package;
using strings = go.strings_package;
using unicode = go.unicode_package;

using @base = go.cmd.compile.@internal.@base_package;
using ir = go.cmd.compile.@internal.ir_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class typecheck_package {

private static constant.Value roundFloat(constant.Value v, long sz) => func((_, panic, _) => {
    switch (sz) {
        case 4: 
            var (f, _) = constant.Float32Val(v);
            return makeFloat64(float64(f));
            break;
        case 8: 
            (f, _) = constant.Float64Val(v);
            return makeFloat64(f);
            break;
    }
    @base.Fatalf("unexpected size: %v", sz);
    panic("unreachable");

});

// truncate float literal fv to 32-bit or 64-bit precision
// according to type; return truncated value.
private static constant.Value truncfltlit(constant.Value v, ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (t.IsUntyped() || overflow(v, _addr_t)) { 
        // If there was overflow, simply continuing would set the
        // value to Inf which in turn would lead to spurious follow-on
        // errors. Avoid this by returning the existing value.
        return v;

    }
    return roundFloat(v, t.Size());

}

// truncate Real and Imag parts of Mpcplx to 32-bit or 64-bit
// precision, according to type; return truncated value. In case of
// overflow, calls Errorf but does not truncate the input value.
private static constant.Value trunccmplxlit(constant.Value v, ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (t.IsUntyped() || overflow(v, _addr_t)) { 
        // If there was overflow, simply continuing would set the
        // value to Inf which in turn would lead to spurious follow-on
        // errors. Avoid this by returning the existing value.
        return v;

    }
    var fsz = t.Size() / 2;
    return makeComplex(roundFloat(constant.Real(v), fsz), roundFloat(constant.Imag(v), fsz));

}

// TODO(mdempsky): Replace these with better APIs.
private static ir.Node convlit(ir.Node n, ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    return convlit1(n, _addr_t, false, null);
}
public static ir.Node DefaultLit(ir.Node n, ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    return convlit1(n, _addr_t, false, null);
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
private static ir.Node convlit1(ir.Node n, ptr<types.Type> _addr_t, bool @explicit, Func<@string> context) {
    ref types.Type t = ref _addr_t.val;

    if (explicit && t == null) {
        @base.Fatalf("explicit conversion missing type");
    }
    if (t != null && t.IsUntyped()) {
        @base.Fatalf("bad conversion to untyped: %v", t);
    }
    if (n == null || n.Type() == null) { 
        // Allow sloppy callers.
        return n;

    }
    if (!n.Type().IsUntyped()) { 
        // Already typed; nothing to do.
        return n;

    }
    if (n.Type().Kind() == types.TNIL) {
        if (n.Op() != ir.ONIL) {
            @base.Fatalf("unexpected op: %v (%v)", n, n.Op());
        }
        n = ir.Copy(n);
        if (t == null) {
            @base.Errorf("use of untyped nil");
            n.SetDiag(true);
            n.SetType(null);
            return n;
        }
        if (!t.HasNil()) { 
            // Leave for caller to handle.
            return n;

        }
        n.SetType(t);
        return n;

    }
    if (t == null || !ir.OKForConst[t.Kind()]) {
        t = defaultType(_addr_n.Type());
    }

    if (n.Op() == ir.OLITERAL) 
        var v = convertVal(n.Val(), _addr_t, explicit);
        if (v.Kind() == constant.Unknown) {
            n = ir.NewConstExpr(n.Val(), n);
            break;
        }
        n = ir.NewConstExpr(v, n);
        n.SetType(t);
        return n;
    else if (n.Op() == ir.OPLUS || n.Op() == ir.ONEG || n.Op() == ir.OBITNOT || n.Op() == ir.ONOT || n.Op() == ir.OREAL || n.Op() == ir.OIMAG) 
        var ot = operandType(n.Op(), _addr_t);
        if (ot == null) {
            n = DefaultLit(n, _addr_null);
            break;
        }
        ptr<ir.UnaryExpr> n = n._<ptr<ir.UnaryExpr>>();
        n.X = convlit(n.X, _addr_ot);
        if (n.X.Type() == null) {
            n.SetType(null);
            return n;
        }
        n.SetType(t);
        return n;
    else if (n.Op() == ir.OADD || n.Op() == ir.OSUB || n.Op() == ir.OMUL || n.Op() == ir.ODIV || n.Op() == ir.OMOD || n.Op() == ir.OOR || n.Op() == ir.OXOR || n.Op() == ir.OAND || n.Op() == ir.OANDNOT || n.Op() == ir.OOROR || n.Op() == ir.OANDAND || n.Op() == ir.OCOMPLEX) 
        ot = operandType(n.Op(), _addr_t);
        if (ot == null) {
            n = DefaultLit(n, _addr_null);
            break;
        }
        ir.Node l = default;        ir.Node r = default;

        switch (n.type()) {
            case ptr<ir.BinaryExpr> n:
                n.X = convlit(n.X, _addr_ot);
                n.Y = convlit(n.Y, _addr_ot);
                (l, r) = (n.X, n.Y);                break;
            case ptr<ir.LogicalExpr> n:
                n.X = convlit(n.X, _addr_ot);
                n.Y = convlit(n.Y, _addr_ot);
                (l, r) = (n.X, n.Y);                break;

        }

        if (l.Type() == null || r.Type() == null) {
            n.SetType(null);
            return n;
        }
        if (!types.Identical(l.Type(), r.Type())) {
            @base.Errorf("invalid operation: %v (mismatched types %v and %v)", n, l.Type(), r.Type());
            n.SetType(null);
            return n;
        }
        n.SetType(t);
        return n;
    else if (n.Op() == ir.OEQ || n.Op() == ir.ONE || n.Op() == ir.OLT || n.Op() == ir.OLE || n.Op() == ir.OGT || n.Op() == ir.OGE) 
        n = n._<ptr<ir.BinaryExpr>>();
        if (!t.IsBoolean()) {
            break;
        }
        n.SetType(t);
        return n;
    else if (n.Op() == ir.OLSH || n.Op() == ir.ORSH) 
        n = n._<ptr<ir.BinaryExpr>>();
        n.X = convlit1(n.X, _addr_t, explicit, null);
        n.SetType(n.X.Type());
        if (n.Type() != null && !n.Type().IsInteger()) {
            @base.Errorf("invalid operation: %v (shift of type %v)", n, n.Type());
            n.SetType(null);
        }
        return n;
    else 
        @base.Fatalf("unexpected untyped expression: %v", n);
        if (!n.Diag()) {
        if (!t.Broke()) {
            if (explicit) {
                @base.Errorf("cannot convert %L to type %v", n, t);
            }
            else if (context != null) {
                @base.Errorf("cannot use %L as type %v in %s", n, t, context());
            }
            else
 {
                @base.Errorf("cannot use %L as type %v", n, t);
            }

        }
        n.SetDiag(true);

    }
    n.SetType(null);
    return n;

}

private static ptr<types.Type> operandType(ir.Op op, ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;


    if (op == ir.OCOMPLEX) 
        if (t.IsComplex()) {
            return _addr_types.FloatForComplex(t)!;
        }
    else if (op == ir.OREAL || op == ir.OIMAG) 
        if (t.IsFloat()) {
            return _addr_types.ComplexForFloat(t)!;
        }
    else 
        if (okfor[op][t.Kind()]) {
            return _addr_t!;
        }
        return _addr_null!;

}

// convertVal converts v into a representation appropriate for t. If
// no such representation exists, it returns Val{} instead.
//
// If explicit is true, then conversions from integer to string are
// also allowed.
private static constant.Value convertVal(constant.Value v, ptr<types.Type> _addr_t, bool @explicit) {
    ref types.Type t = ref _addr_t.val;

    {
        var ct = v.Kind();


        if (ct == constant.Bool)
        {
            if (t.IsBoolean()) {
                return v;
            }
            goto __switch_break0;
        }
        if (ct == constant.String)
        {
            if (t.IsString()) {
                return v;
            }
            goto __switch_break0;
        }
        if (ct == constant.Int)
        {
            if (explicit && t.IsString()) {
                return tostr(v);
            }
            fallthrough = true;
        }
        if (fallthrough || ct == constant.Float || ct == constant.Complex)
        {

            if (t.IsInteger()) 
                v = toint(v);
                overflow(v, _addr_t);
                return v;
            else if (t.IsFloat()) 
                v = toflt(v);
                v = truncfltlit(v, _addr_t);
                return v;
            else if (t.IsComplex()) 
                v = tocplx(v);
                v = trunccmplxlit(v, _addr_t);
                return v;
                        goto __switch_break0;
        }

        __switch_break0:;
    }

    return constant.MakeUnknown();

}

private static constant.Value tocplx(constant.Value v) {
    return constant.ToComplex(v);
}

private static constant.Value toflt(constant.Value v) {
    if (v.Kind() == constant.Complex) {
        if (constant.Sign(constant.Imag(v)) != 0) {
            @base.Errorf("constant %v truncated to real", v);
        }
        v = constant.Real(v);

    }
    return constant.ToFloat(v);

}

private static constant.Value toint(constant.Value v) {
    if (v.Kind() == constant.Complex) {
        if (constant.Sign(constant.Imag(v)) != 0) {
            @base.Errorf("constant %v truncated to integer", v);
        }
        v = constant.Real(v);

    }
    {
        var v = constant.ToInt(v);

        if (v.Kind() == constant.Int) {
            return v;
        }
    } 

    // The value of v cannot be represented as an integer;
    // so we need to print an error message.
    // Unfortunately some float values cannot be
    // reasonably formatted for inclusion in an error
    // message (example: 1 + 1e-100), so first we try to
    // format the float; if the truncation resulted in
    // something that looks like an integer we omit the
    // value from the error message.
    // (See issue #11371).
    var f = ir.BigFloat(v);
    if (f.MantExp(null) > 2 * ir.ConstPrec) {
        @base.Errorf("integer too large");
    }
    else
 {
        big.Float t = default;
        t.Parse(fmt.Sprint(v), 0);
        if (t.IsInt()) {
            @base.Errorf("constant truncated to integer");
        }
        else
 {
            @base.Errorf("constant %v truncated to integer", v);
        }
    }
    return constant.MakeInt64(1);

}

// overflow reports whether constant value v is too large
// to represent with type t, and emits an error message if so.
private static bool overflow(constant.Value v, ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;
 
    // v has already been converted
    // to appropriate form for t.
    if (t.IsUntyped()) {
        return false;
    }
    if (v.Kind() == constant.Int && constant.BitLen(v) > ir.ConstPrec) {
        @base.Errorf("integer too large");
        return true;
    }
    if (ir.ConstOverflow(v, t)) {
        @base.Errorf("constant %v overflows %v", types.FmtConst(v, false), t);
        return true;
    }
    return false;

}

private static constant.Value tostr(constant.Value v) {
    if (v.Kind() == constant.Int) {
        var r = unicode.ReplacementChar;
        {
            var (x, ok) = constant.Uint64Val(v);

            if (ok && x <= unicode.MaxRune) {
                r = rune(x);
            }

        }

        v = constant.MakeString(string(r));

    }
    return v;

}

private static array<token.Token> tokenForOp = new array<token.Token>(InitKeyedValues<token.Token>((ir.OPLUS, token.ADD), (ir.ONEG, token.SUB), (ir.ONOT, token.NOT), (ir.OBITNOT, token.XOR), (ir.OADD, token.ADD), (ir.OSUB, token.SUB), (ir.OMUL, token.MUL), (ir.ODIV, token.QUO), (ir.OMOD, token.REM), (ir.OOR, token.OR), (ir.OXOR, token.XOR), (ir.OAND, token.AND), (ir.OANDNOT, token.AND_NOT), (ir.OOROR, token.LOR), (ir.OANDAND, token.LAND), (ir.OEQ, token.EQL), (ir.ONE, token.NEQ), (ir.OLT, token.LSS), (ir.OLE, token.LEQ), (ir.OGT, token.GTR), (ir.OGE, token.GEQ), (ir.OLSH, token.SHL), (ir.ORSH, token.SHR)));

// EvalConst returns a constant-evaluated expression equivalent to n.
// If n is not a constant, EvalConst returns n.
// Otherwise, EvalConst returns a new OLITERAL with the same value as n,
// and with .Orig pointing back to n.
public static ir.Node EvalConst(ir.Node n) { 
    // Pick off just the opcodes that can be constant evaluated.

    if (n.Op() == ir.OPLUS || n.Op() == ir.ONEG || n.Op() == ir.OBITNOT || n.Op() == ir.ONOT) 
        ptr<ir.UnaryExpr> n = n._<ptr<ir.UnaryExpr>>();
        var nl = n.X;
        if (nl.Op() == ir.OLITERAL) {
            nuint prec = default;
            if (n.Type().IsUnsigned()) {
                prec = uint(n.Type().Size() * 8);
            }
            return OrigConst(n, constant.UnaryOp(tokenForOp[n.Op()], nl.Val(), prec));
        }
    else if (n.Op() == ir.OADD || n.Op() == ir.OSUB || n.Op() == ir.OMUL || n.Op() == ir.ODIV || n.Op() == ir.OMOD || n.Op() == ir.OOR || n.Op() == ir.OXOR || n.Op() == ir.OAND || n.Op() == ir.OANDNOT) 
        n = n._<ptr<ir.BinaryExpr>>();
        nl = n.X;
        var nr = n.Y;
        if (nl.Op() == ir.OLITERAL && nr.Op() == ir.OLITERAL) {
            var rval = nr.Val(); 

            // check for divisor underflow in complex division (see issue 20227)
            if (n.Op() == ir.ODIV && n.Type().IsComplex() && constant.Sign(square(constant.Real(rval))) == 0 && constant.Sign(square(constant.Imag(rval))) == 0) {
                @base.Errorf("complex division by zero");
                n.SetType(null);
                return n;
            }

            if ((n.Op() == ir.ODIV || n.Op() == ir.OMOD) && constant.Sign(rval) == 0) {
                @base.Errorf("division by zero");
                n.SetType(null);
                return n;
            }

            var tok = tokenForOp[n.Op()];
            if (n.Op() == ir.ODIV && n.Type().IsInteger()) {
                tok = token.QUO_ASSIGN; // integer division
            }

            return OrigConst(n, constant.BinaryOp(nl.Val(), tok, rval));

        }
    else if (n.Op() == ir.OOROR || n.Op() == ir.OANDAND) 
        n = n._<ptr<ir.LogicalExpr>>();
        nl = n.X;
        nr = n.Y;
        if (nl.Op() == ir.OLITERAL && nr.Op() == ir.OLITERAL) {
            return OrigConst(n, constant.BinaryOp(nl.Val(), tokenForOp[n.Op()], nr.Val()));
        }
    else if (n.Op() == ir.OEQ || n.Op() == ir.ONE || n.Op() == ir.OLT || n.Op() == ir.OLE || n.Op() == ir.OGT || n.Op() == ir.OGE) 
        n = n._<ptr<ir.BinaryExpr>>();
        nl = n.X;
        nr = n.Y;
        if (nl.Op() == ir.OLITERAL && nr.Op() == ir.OLITERAL) {
            return OrigBool(n, constant.Compare(nl.Val(), tokenForOp[n.Op()], nr.Val()));
        }
    else if (n.Op() == ir.OLSH || n.Op() == ir.ORSH) 
        n = n._<ptr<ir.BinaryExpr>>();
        nl = n.X;
        nr = n.Y;
        if (nl.Op() == ir.OLITERAL && nr.Op() == ir.OLITERAL) { 
            // shiftBound from go/types; "so we can express smallestFloat64" (see issue #44057)
            const nint shiftBound = 1023 - 1 + 52;

            var (s, ok) = constant.Uint64Val(nr.Val());
            if (!ok || s > shiftBound) {
                @base.Errorf("invalid shift count %v", nr);
                n.SetType(null);
                break;
            }

            return OrigConst(n, constant.Shift(toint(nl.Val()), tokenForOp[n.Op()], uint(s)));

        }
    else if (n.Op() == ir.OCONV || n.Op() == ir.ORUNESTR) 
        n = n._<ptr<ir.ConvExpr>>();
        nl = n.X;
        if (ir.OKForConst[n.Type().Kind()] && nl.Op() == ir.OLITERAL) {
            return OrigConst(n, convertVal(nl.Val(), _addr_n.Type(), true));
        }
    else if (n.Op() == ir.OCONVNOP) 
        n = n._<ptr<ir.ConvExpr>>();
        nl = n.X;
        if (ir.OKForConst[n.Type().Kind()] && nl.Op() == ir.OLITERAL) { 
            // set so n.Orig gets OCONV instead of OCONVNOP
            n.SetOp(ir.OCONV);
            return OrigConst(n, nl.Val());

        }
    else if (n.Op() == ir.OADDSTR) 
        // Merge adjacent constants in the argument list.
        n = n._<ptr<ir.AddStringExpr>>();
        var s = n.List;
        nint need = 0;
        {
            nint i__prev1 = i;

            for (nint i = 0; i < len(s); i++) {
                if (i == 0 || !ir.IsConst(s[i - 1], constant.String) || !ir.IsConst(s[i], constant.String)) { 
                    // Can't merge s[i] into s[i-1]; need a slot in the list.
                    need++;

                }

            }


            i = i__prev1;
        }
        if (need == len(s)) {
            return n;
        }
        if (need == 1) {
            slice<@string> strs = default;
            foreach (var (_, c) in s) {
                strs = append(strs, ir.StringVal(c));
            }
            return OrigConst(n, constant.MakeString(strings.Join(strs, "")));
        }
        var newList = make_slice<ir.Node>(0, need);
        {
            nint i__prev1 = i;

            for (i = 0; i < len(s); i++) {
                if (ir.IsConst(s[i], constant.String) && i + 1 < len(s) && ir.IsConst(s[i + 1], constant.String)) { 
                    // merge from i up to but not including i2
                    strs = default;
                    var i2 = i;
                    while (i2 < len(s) && ir.IsConst(s[i2], constant.String)) {
                        strs = append(strs, ir.StringVal(s[i2]));
                        i2++;
                    }
                else



                    nl = ir.Copy(n)._<ptr<ir.AddStringExpr>>();
                    nl.List = s[(int)i..(int)i2];
                    newList = append(newList, OrigConst(nl, constant.MakeString(strings.Join(strs, ""))));
                    i = i2 - 1;

                } {
                    newList = append(newList, s[i]);
                }

            }


            i = i__prev1;
        }

        ptr<ir.AddStringExpr> nn = ir.Copy(n)._<ptr<ir.AddStringExpr>>();
        nn.List = newList;
        return nn;
    else if (n.Op() == ir.OCAP || n.Op() == ir.OLEN) 
        n = n._<ptr<ir.UnaryExpr>>();
        nl = n.X;

        if (nl.Type().Kind() == types.TSTRING) 
            if (ir.IsConst(nl, constant.String)) {
                return OrigInt(n, int64(len(ir.StringVal(nl))));
            }
        else if (nl.Type().Kind() == types.TARRAY) 
            if (!anyCallOrChan(nl)) {
                return OrigInt(n, nl.Type().NumElem());
            }
            else if (n.Op() == ir.OALIGNOF || n.Op() == ir.OOFFSETOF || n.Op() == ir.OSIZEOF) 
        n = n._<ptr<ir.UnaryExpr>>();
        return OrigInt(n, evalunsafe(n));
    else if (n.Op() == ir.OREAL) 
        n = n._<ptr<ir.UnaryExpr>>();
        nl = n.X;
        if (nl.Op() == ir.OLITERAL) {
            return OrigConst(n, constant.Real(nl.Val()));
        }
    else if (n.Op() == ir.OIMAG) 
        n = n._<ptr<ir.UnaryExpr>>();
        nl = n.X;
        if (nl.Op() == ir.OLITERAL) {
            return OrigConst(n, constant.Imag(nl.Val()));
        }
    else if (n.Op() == ir.OCOMPLEX) 
        n = n._<ptr<ir.BinaryExpr>>();
        nl = n.X;
        nr = n.Y;
        if (nl.Op() == ir.OLITERAL && nr.Op() == ir.OLITERAL) {
            return OrigConst(n, makeComplex(nl.Val(), nr.Val()));
        }
        return n;

}

private static constant.Value makeFloat64(double f) {
    if (math.IsInf(f, 0)) {
        @base.Fatalf("infinity is not a valid constant");
    }
    return constant.MakeFloat64(f);

}

private static constant.Value makeComplex(constant.Value real, constant.Value imag) {
    return constant.BinaryOp(constant.ToFloat(real), token.ADD, constant.MakeImag(constant.ToFloat(imag)));
}

private static constant.Value square(constant.Value x) {
    return constant.BinaryOp(x, token.MUL, x);
}

// For matching historical "constant OP overflow" error messages.
// TODO(mdempsky): Replace with error messages like go/types uses.
private static array<@string> overflowNames = new array<@string>(InitKeyedValues<@string>((ir.OADD, "addition"), (ir.OSUB, "subtraction"), (ir.OMUL, "multiplication"), (ir.OLSH, "shift"), (ir.OXOR, "bitwise XOR"), (ir.OBITNOT, "bitwise complement")));

// OrigConst returns an OLITERAL with orig n and value v.
public static ir.Node OrigConst(ir.Node n, constant.Value v) {
    var lno = ir.SetPos(n);
    v = convertVal(v, _addr_n.Type(), false);
    @base.Pos = lno;


    if (v.Kind() == constant.Int)
    {
        if (constant.BitLen(v) <= ir.ConstPrec) {
            break;
        }
        fallthrough = true;
    }
    if (fallthrough || v.Kind() == constant.Unknown)
    {
        var what = overflowNames[n.Op()];
        if (what == "") {
            @base.Fatalf("unexpected overflow: %v", n.Op());
        }
        @base.ErrorfAt(n.Pos(), "constant %v overflow", what);
        n.SetType(null);
        return n;
        goto __switch_break1;
    }

    __switch_break1:;

    return ir.NewConstExpr(v, n);

}

public static ir.Node OrigBool(ir.Node n, bool v) {
    return OrigConst(n, constant.MakeBool(v));
}

public static ir.Node OrigInt(ir.Node n, long v) {
    return OrigConst(n, constant.MakeInt64(v));
}

// DefaultLit on both nodes simultaneously;
// if they're both ideal going in they better
// get the same type going out.
// force means must assign concrete (non-ideal) type.
// The results of defaultlit2 MUST be assigned back to l and r, e.g.
//     n.Left, n.Right = defaultlit2(n.Left, n.Right, force)
private static (ir.Node, ir.Node) defaultlit2(ir.Node l, ir.Node r, bool force) {
    ir.Node _p0 = default;
    ir.Node _p0 = default;

    if (l.Type() == null || r.Type() == null) {
        return (l, r);
    }
    if (!l.Type().IsInterface() && !r.Type().IsInterface()) { 
        // Can't mix bool with non-bool, string with non-string.
        if (l.Type().IsBoolean() != r.Type().IsBoolean()) {
            return (l, r);
        }
        if (l.Type().IsString() != r.Type().IsString()) {
            return (l, r);
        }
    }
    if (!l.Type().IsUntyped()) {
        r = convlit(r, _addr_l.Type());
        return (l, r);
    }
    if (!r.Type().IsUntyped()) {
        l = convlit(l, _addr_r.Type());
        return (l, r);
    }
    if (!force) {
        return (l, r);
    }
    if (ir.IsNil(l) || ir.IsNil(r)) {
        return (l, r);
    }
    var t = defaultType(_addr_mixUntyped(_addr_l.Type(), _addr_r.Type()));
    l = convlit(l, _addr_t);
    r = convlit(r, _addr_t);
    return (l, r);

}

private static ptr<types.Type> mixUntyped(ptr<types.Type> _addr_t1, ptr<types.Type> _addr_t2) => func((_, panic, _) => {
    ref types.Type t1 = ref _addr_t1.val;
    ref types.Type t2 = ref _addr_t2.val;

    if (t1 == t2) {
        return _addr_t1!;
    }
    Func<ptr<types.Type>, nint> rank = t => {

        if (t == types.UntypedInt) 
            return _addr_0!;
        else if (t == types.UntypedRune) 
            return _addr_1!;
        else if (t == types.UntypedFloat) 
            return _addr_2!;
        else if (t == types.UntypedComplex) 
            return _addr_3!;
                @base.Fatalf("bad type %v", t);
        panic("unreachable");

    };

    if (rank(t2) > rank(t1)) {
        return _addr_t2!;
    }
    return _addr_t1!;

});

private static ptr<types.Type> defaultType(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (!t.IsUntyped() || t.Kind() == types.TNIL) {
        return _addr_t!;
    }

    if (t == types.UntypedBool) 
        return _addr_types.Types[types.TBOOL]!;
    else if (t == types.UntypedString) 
        return _addr_types.Types[types.TSTRING]!;
    else if (t == types.UntypedInt) 
        return _addr_types.Types[types.TINT]!;
    else if (t == types.UntypedRune) 
        return _addr_types.RuneType!;
    else if (t == types.UntypedFloat) 
        return _addr_types.Types[types.TFLOAT64]!;
    else if (t == types.UntypedComplex) 
        return _addr_types.Types[types.TCOMPLEX128]!;
        @base.Fatalf("bad type %v", t);
    return _addr_null!;

}

// IndexConst checks if Node n contains a constant expression
// representable as a non-negative int and returns its value.
// If n is not a constant expression, not representable as an
// integer, or negative, it returns -1. If n is too large, it
// returns -2.
public static long IndexConst(ir.Node n) {
    if (n.Op() != ir.OLITERAL) {
        return -1;
    }
    if (!n.Type().IsInteger() && n.Type().Kind() != types.TIDEAL) {
        return -1;
    }
    var v = toint(n.Val());
    if (v.Kind() != constant.Int || constant.Sign(v) < 0) {
        return -1;
    }
    if (ir.ConstOverflow(v, types.Types[types.TINT])) {
        return -2;
    }
    return ir.IntVal(types.Types[types.TINT], v);

}

// anyCallOrChan reports whether n contains any calls or channel operations.
private static bool anyCallOrChan(ir.Node n) {
    return ir.Any(n, n => {

        if (n.Op() == ir.OAPPEND || n.Op() == ir.OCALL || n.Op() == ir.OCALLFUNC || n.Op() == ir.OCALLINTER || n.Op() == ir.OCALLMETH || n.Op() == ir.OCAP || n.Op() == ir.OCLOSE || n.Op() == ir.OCOMPLEX || n.Op() == ir.OCOPY || n.Op() == ir.ODELETE || n.Op() == ir.OIMAG || n.Op() == ir.OLEN || n.Op() == ir.OMAKE || n.Op() == ir.ONEW || n.Op() == ir.OPANIC || n.Op() == ir.OPRINT || n.Op() == ir.OPRINTN || n.Op() == ir.OREAL || n.Op() == ir.ORECOVER || n.Op() == ir.ORECV || n.Op() == ir.OUNSAFEADD || n.Op() == ir.OUNSAFESLICE) 
            return true;
                return false;

    });

}

// A constSet represents a set of Go constant expressions.
private partial struct constSet {
    public map<constSetKey, src.XPos> m;
}

private partial struct constSetKey {
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
private static void add(this ptr<constSet> _addr_s, src.XPos pos, ir.Node n, @string what, @string where) {
    ref constSet s = ref _addr_s.val;

    {
        var conv__prev1 = conv;

        var conv = n;

        if (conv.Op() == ir.OCONVIFACE) {
            conv = conv._<ptr<ir.ConvExpr>>();
            if (conv.Implicit()) {
                n = conv.X;
            }
        }
        conv = conv__prev1;

    }


    if (!ir.IsConstNode(n) || n.Type() == null) {
        return ;
    }
    if (n.Type().IsUntyped()) {
        @base.Fatalf("%v is untyped", n);
    }
    var typ = n.Type();

    if (typ == types.ByteType) 
        typ = types.Types[types.TUINT8];
    else if (typ == types.RuneType) 
        typ = types.Types[types.TINT32];
        constSetKey k = new constSetKey(typ,ir.ConstValue(n));

    if (ir.HasUniquePos(n)) {
        pos = n.Pos();
    }
    if (s.m == null) {
        s.m = make_map<constSetKey, src.XPos>();
    }
    {
        var (prevPos, isDup) = s.m[k];

        if (isDup) {
            @base.ErrorfAt(pos, "duplicate %s %s in %s\n\tprevious %s at %v", what, nodeAndVal(n), where, what, @base.FmtPos(prevPos));
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
private static @string nodeAndVal(ir.Node n) {
    var show = fmt.Sprint(n);
    var val = ir.ConstValue(n);
    {
        var s = fmt.Sprintf("%#v", val);

        if (show != s) {
            show += " (value " + s + ")";
        }
    }

    return show;

}

// evalunsafe evaluates a package unsafe operation and returns the result.
private static long evalunsafe(ir.Node n) {

    if (n.Op() == ir.OALIGNOF || n.Op() == ir.OSIZEOF) 
        ptr<ir.UnaryExpr> n = n._<ptr<ir.UnaryExpr>>();
        n.X = Expr(n.X);
        n.X = DefaultLit(n.X, _addr_null);
        var tr = n.X.Type();
        if (tr == null) {
            return 0;
        }
        types.CalcSize(tr);
        if (n.Op() == ir.OALIGNOF) {
            return int64(tr.Align);
        }
        return tr.Width;
    else if (n.Op() == ir.OOFFSETOF) 
        // must be a selector.
        n = n._<ptr<ir.UnaryExpr>>();
        if (n.X.Op() != ir.OXDOT) {
            @base.Errorf("invalid expression %v", n);
            return 0;
        }
        ptr<ir.SelectorExpr> sel = n.X._<ptr<ir.SelectorExpr>>(); 

        // Remember base of selector to find it back after dot insertion.
        // Since r->left may be mutated by typechecking, check it explicitly
        // first to track it correctly.
        sel.X = Expr(sel.X);
        var sbase = sel.X;

        var tsel = Expr(sel);
        n.X = tsel;
        if (tsel.Type() == null) {
            return 0;
        }

        if (tsel.Op() == ir.ODOT || tsel.Op() == ir.ODOTPTR) 
            break;
        else if (tsel.Op() == ir.OCALLPART) 
            @base.Errorf("invalid expression %v: argument is a method value", n);
            return 0;
        else 
            @base.Errorf("invalid expression %v", n);
            return 0;
        // Sum offsets for dots until we reach sbase.
        long v = default;
        ir.Node next = default;
        {
            var r__prev1 = r;

            var r = tsel;

            while (r != sbase) {

                if (r.Op() == ir.ODOTPTR) 
                {
                    // For Offsetof(s.f), s may itself be a pointer,
                    // but accessing f must not otherwise involve
                    // indirection via embedded pointer types.
                    r = r._<ptr<ir.SelectorExpr>>();
                    if (r.X != sbase) {
                        @base.Errorf("invalid expression %v: selector implies indirection of embedded %v", n, r.X);
                        return 0;
                r = next;
                    }

                    fallthrough = true;
                }
                if (fallthrough || r.Op() == ir.ODOT)
                {
                    r = r._<ptr<ir.SelectorExpr>>();
                    v += r.Offset();
                    next = r.X;
                    goto __switch_break2;
                }
                // default: 
                    ir.Dump("unsafenmagic", tsel);
                    @base.Fatalf("impossible %v node after dot insertion", r.Op());

                __switch_break2:;

            }


            r = r__prev1;
        }
        return v;
        @base.Fatalf("unexpected op %v", n.Op());
    return 0;

}

} // end typecheck_package
