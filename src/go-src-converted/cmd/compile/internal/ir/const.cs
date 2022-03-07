// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ir -- go2cs converted at 2022 March 06 22:48:57 UTC
// import "cmd/compile/internal/ir" ==> using ir = go.cmd.compile.@internal.ir_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ir\const.go
using constant = go.go.constant_package;
using math = go.math_package;
using big = go.math.big_package;

using @base = go.cmd.compile.@internal.@base_package;
using types = go.cmd.compile.@internal.types_package;

namespace go.cmd.compile.@internal;

public static partial class ir_package {

public static Node NewBool(bool b) {
    return NewLiteral(constant.MakeBool(b));
}

public static Node NewInt(long v) {
    return NewLiteral(constant.MakeInt64(v));
}

public static Node NewString(@string s) {
    return NewLiteral(constant.MakeString(s));
}

 
// Maximum size in bits for big.Ints before signalling
// overflow and also mantissa precision for big.Floats.
public static readonly nint ConstPrec = 512;


public static ptr<big.Float> BigFloat(constant.Value v) {
    ptr<big.Float> f = @new<big.Float>();
    f.SetPrec(ConstPrec);
    switch (constant.Val(v).type()) {
        case long u:
            f.SetInt64(u);
            break;
        case ptr<big.Int> u:
            f.SetInt(u);
            break;
        case ptr<big.Float> u:
            f.Set(u);
            break;
        case ptr<big.Rat> u:
            f.SetRat(u);
            break;
        default:
        {
            var u = constant.Val(v).type();
            @base.Fatalf("unexpected: %v", u);
            break;
        }
    }
    return _addr_f!;

}

// ConstOverflow reports whether constant value v is too large
// to represent with type t.
public static bool ConstOverflow(constant.Value v, ptr<types.Type> _addr_t) => func((_, panic, _) => {
    ref types.Type t = ref _addr_t.val;


    if (t.IsInteger()) 
        var bits = uint(8 * t.Size());
        if (t.IsUnsigned()) {
            var (x, ok) = constant.Uint64Val(v);
            return !ok || x >> (int)(bits) != 0;
        }
        (x, ok) = constant.Int64Val(v);
        if (x < 0) {
            x = ~x;
        }
        return !ok || x >> (int)((bits - 1)) != 0;
    else if (t.IsFloat()) 
        switch (t.Size()) {
            case 4: 
                var (f, _) = constant.Float32Val(v);
                return math.IsInf(float64(f), 0);
                break;
            case 8: 
                (f, _) = constant.Float64Val(v);
                return math.IsInf(f, 0);
                break;
        }
    else if (t.IsComplex()) 
        var ft = types.FloatForComplex(t);
        return ConstOverflow(constant.Real(v), _addr_ft) || ConstOverflow(constant.Imag(v), _addr_ft);
        @base.Fatalf("ConstOverflow: %v, %v", v, t);
    panic("unreachable");

});

// IsConstNode reports whether n is a Go language constant (as opposed to a
// compile-time constant).
//
// Expressions derived from nil, like string([]byte(nil)), while they
// may be known at compile time, are not Go language constants.
public static bool IsConstNode(Node n) {
    return n.Op() == OLITERAL;
}

public static bool IsSmallIntConst(Node n) {
    if (n.Op() == OLITERAL) {
        var (v, ok) = constant.Int64Val(n.Val());
        return ok && int64(int32(v)) == v;
    }
    return false;

}

} // end ir_package
