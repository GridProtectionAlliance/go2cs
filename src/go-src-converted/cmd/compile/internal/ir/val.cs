// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ir -- go2cs converted at 2022 March 06 22:49:17 UTC
// import "cmd/compile/internal/ir" ==> using ir = go.cmd.compile.@internal.ir_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ir\val.go
using constant = go.go.constant_package;
using math = go.math_package;

using @base = go.cmd.compile.@internal.@base_package;
using types = go.cmd.compile.@internal.types_package;

namespace go.cmd.compile.@internal;

public static partial class ir_package {

public static constant.Kind ConstType(Node n) {
    if (n == null || n.Op() != OLITERAL) {
        return constant.Unknown;
    }
    return n.Val().Kind();

}

// ConstValue returns the constant value stored in n as an interface{}.
// It returns int64s for ints and runes, float64s for floats,
// and complex128s for complex values.
public static void ConstValue(Node n) => func((_, panic, _) => {
    {
        var v = n.Val();


        if (v.Kind() == constant.Bool) 
            return constant.BoolVal(v);
        else if (v.Kind() == constant.String) 
            return constant.StringVal(v);
        else if (v.Kind() == constant.Int) 
            return IntVal(_addr_n.Type(), v);
        else if (v.Kind() == constant.Float) 
            return Float64Val(v);
        else if (v.Kind() == constant.Complex) 
            return complex(Float64Val(constant.Real(v)), Float64Val(constant.Imag(v)));
        else 
            @base.Fatalf("unexpected constant: %v", v);
            panic("unreachable");

    }

});

// IntVal returns v converted to int64.
// Note: if t is uint64, very large values will be converted to negative int64.
public static long IntVal(ptr<types.Type> _addr_t, constant.Value v) => func((_, panic, _) => {
    ref types.Type t = ref _addr_t.val;

    if (t.IsUnsigned()) {
        {
            var x__prev2 = x;

            var (x, ok) = constant.Uint64Val(v);

            if (ok) {
                return int64(x);
            }

            x = x__prev2;

        }

    }
    else
 {
        {
            var x__prev2 = x;

            (x, ok) = constant.Int64Val(v);

            if (ok) {
                return x;
            }

            x = x__prev2;

        }

    }
    @base.Fatalf("%v out of range for %v", v, t);
    panic("unreachable");

});

public static double Float64Val(constant.Value v) => func((_, panic, _) => {
    {
        var (x, _) = constant.Float64Val(v);

        if (!math.IsInf(x, 0)) {
            return x + 0; // avoid -0 (should not be needed, but be conservative)
        }
    }

    @base.Fatalf("bad float64 value: %v", v);
    panic("unreachable");

});

public static void AssertValidTypeForConst(ptr<types.Type> _addr_t, constant.Value v) {
    ref types.Type t = ref _addr_t.val;

    if (!ValidTypeForConst(_addr_t, v)) {
        @base.Fatalf("%v does not represent %v", t, v);
    }
}

public static bool ValidTypeForConst(ptr<types.Type> _addr_t, constant.Value v) => func((_, panic, _) => {
    ref types.Type t = ref _addr_t.val;


    if (v.Kind() == constant.Unknown) 
        return OKForConst[t.Kind()];
    else if (v.Kind() == constant.Bool) 
        return t.IsBoolean();
    else if (v.Kind() == constant.String) 
        return t.IsString();
    else if (v.Kind() == constant.Int) 
        return t.IsInteger();
    else if (v.Kind() == constant.Float) 
        return t.IsFloat();
    else if (v.Kind() == constant.Complex) 
        return t.IsComplex();
        @base.Fatalf("unexpected constant kind: %v", v);
    panic("unreachable");

});

// NewLiteral returns a new untyped constant with value v.
public static Node NewLiteral(constant.Value v) {
    return NewBasicLit(@base.Pos, v);
}

private static ptr<types.Type> idealType(constant.Kind ct) {

    if (ct == constant.String) 
        return _addr_types.UntypedString!;
    else if (ct == constant.Bool) 
        return _addr_types.UntypedBool!;
    else if (ct == constant.Int) 
        return _addr_types.UntypedInt!;
    else if (ct == constant.Float) 
        return _addr_types.UntypedFloat!;
    else if (ct == constant.Complex) 
        return _addr_types.UntypedComplex!;
        @base.Fatalf("unexpected Ctype: %v", ct);
    return _addr_null!;

}

public static array<bool> OKForConst = new array<bool>(types.NTYPE);

// CanInt64 reports whether it is safe to call Int64Val() on n.
public static bool CanInt64(Node n) {
    if (!IsConst(n, constant.Int)) {
        return false;
    }
    var (_, ok) = constant.Int64Val(n.Val());
    return ok;

}

// Int64Val returns n as an int64.
// n must be an integer or rune constant.
public static long Int64Val(Node n) {
    if (!IsConst(n, constant.Int)) {
        @base.Fatalf("Int64Val(%v)", n);
    }
    var (x, ok) = constant.Int64Val(n.Val());
    if (!ok) {
        @base.Fatalf("Int64Val(%v)", n);
    }
    return x;

}

// Uint64Val returns n as an uint64.
// n must be an integer or rune constant.
public static ulong Uint64Val(Node n) {
    if (!IsConst(n, constant.Int)) {
        @base.Fatalf("Uint64Val(%v)", n);
    }
    var (x, ok) = constant.Uint64Val(n.Val());
    if (!ok) {
        @base.Fatalf("Uint64Val(%v)", n);
    }
    return x;

}

// BoolVal returns n as a bool.
// n must be a boolean constant.
public static bool BoolVal(Node n) {
    if (!IsConst(n, constant.Bool)) {
        @base.Fatalf("BoolVal(%v)", n);
    }
    return constant.BoolVal(n.Val());

}

// StringVal returns the value of a literal string Node as a string.
// n must be a string constant.
public static @string StringVal(Node n) {
    if (!IsConst(n, constant.String)) {
        @base.Fatalf("StringVal(%v)", n);
    }
    return constant.StringVal(n.Val());

}

} // end ir_package
