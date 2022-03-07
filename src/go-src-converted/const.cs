// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2022 March 06 23:33:11 UTC
// import "golang.org/x/tools/go/ssa" ==> using ssa = go.golang.org.x.tools.go.ssa_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\const.go
// This file defines the Const SSA value type.

using fmt = go.fmt_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using types = go.go.types_package;
using strconv = go.strconv_package;

namespace go.golang.org.x.tools.go;

public static partial class ssa_package {

    // NewConst returns a new constant of the specified value and type.
    // val must be valid according to the specification of Const.Value.
    //
public static ptr<Const> NewConst(constant.Value val, types.Type typ) {
    return addr(new Const(typ,val));
}

// intConst returns an 'int' constant that evaluates to i.
// (i is an int64 in case the host is narrower than the target.)
private static ptr<Const> intConst(long i) {
    return _addr_NewConst(constant.MakeInt64(i), tInt)!;
}

// nilConst returns a nil constant of the specified type, which may
// be any reference type, including interfaces.
//
private static ptr<Const> nilConst(types.Type typ) {
    return _addr_NewConst(null, typ)!;
}

// stringConst returns a 'string' constant that evaluates to s.
private static ptr<Const> stringConst(@string s) {
    return _addr_NewConst(constant.MakeString(s), tString)!;
}

// zeroConst returns a new "zero" constant of the specified type,
// which must not be an array or struct type: the zero values of
// aggregates are well-defined but cannot be represented by Const.
//
private static ptr<Const> zeroConst(types.Type t) => func((_, panic, _) => {
    switch (t.type()) {
        case ptr<types.Basic> t:

            if (t.Info() & types.IsBoolean != 0)
            {
                return _addr_NewConst(constant.MakeBool(false), t)!;
                goto __switch_break0;
            }
            if (t.Info() & types.IsNumeric != 0)
            {
                return _addr_NewConst(constant.MakeInt64(0), t)!;
                goto __switch_break0;
            }
            if (t.Info() & types.IsString != 0)
            {
                return _addr_NewConst(constant.MakeString(""), t)!;
                goto __switch_break0;
            }
            if (t.Kind() == types.UnsafePointer)
            {
                fallthrough = true;
            }
            if (fallthrough || t.Kind() == types.UntypedNil)
            {
                return _addr_nilConst(t)!;
                goto __switch_break0;
            }
            // default: 
                panic(fmt.Sprint("zeroConst for unexpected type:", t));

            __switch_break0:;
            break;
        case ptr<types.Pointer> t:
            return _addr_nilConst(t)!;
            break;
        case ptr<types.Slice> t:
            return _addr_nilConst(t)!;
            break;
        case ptr<types.Interface> t:
            return _addr_nilConst(t)!;
            break;
        case ptr<types.Chan> t:
            return _addr_nilConst(t)!;
            break;
        case ptr<types.Map> t:
            return _addr_nilConst(t)!;
            break;
        case ptr<types.Signature> t:
            return _addr_nilConst(t)!;
            break;
        case ptr<types.Named> t:
            return _addr_NewConst(zeroConst(t.Underlying()).Value, t)!;
            break;
        case ptr<types.Array> t:
            panic(fmt.Sprint("zeroConst applied to aggregate:", t));
            break;
        case ptr<types.Struct> t:
            panic(fmt.Sprint("zeroConst applied to aggregate:", t));
            break;
        case ptr<types.Tuple> t:
            panic(fmt.Sprint("zeroConst applied to aggregate:", t));
            break;
    }
    panic(fmt.Sprint("zeroConst: unexpected ", t));

});

private static @string RelString(this ptr<Const> _addr_c, ptr<types.Package> _addr_from) {
    ref Const c = ref _addr_c.val;
    ref types.Package from = ref _addr_from.val;

    @string s = default;
    if (c.Value == null) {
        s = "nil";
    }
    else if (c.Value.Kind() == constant.String) {
        s = constant.StringVal(c.Value);
        const nint max = 20; 
        // TODO(adonovan): don't cut a rune in half.
 
        // TODO(adonovan): don't cut a rune in half.
        if (len(s) > max) {
            s = s[..(int)max - 3] + "..."; // abbreviate
        }
        s = strconv.Quote(s);

    }
    else
 {
        s = c.Value.String();
    }
    return s + ":" + relType(c.Type(), from);

}

private static @string Name(this ptr<Const> _addr_c) {
    ref Const c = ref _addr_c.val;

    return c.RelString(null);
}

private static @string String(this ptr<Const> _addr_c) {
    ref Const c = ref _addr_c.val;

    return c.Name();
}

private static types.Type Type(this ptr<Const> _addr_c) {
    ref Const c = ref _addr_c.val;

    return c.typ;
}

private static ptr<slice<Instruction>> Referrers(this ptr<Const> _addr_c) {
    ref Const c = ref _addr_c.val;

    return _addr_null!;
}

private static ptr<Function> Parent(this ptr<Const> _addr_c) {
    ref Const c = ref _addr_c.val;

    return _addr_null!;
}

private static token.Pos Pos(this ptr<Const> _addr_c) {
    ref Const c = ref _addr_c.val;

    return token.NoPos;
}

// IsNil returns true if this constant represents a typed or untyped nil value.
private static bool IsNil(this ptr<Const> _addr_c) {
    ref Const c = ref _addr_c.val;

    return c.Value == null;
}

// TODO(adonovan): move everything below into golang.org/x/tools/go/ssa/interp.

// Int64 returns the numeric value of this constant truncated to fit
// a signed 64-bit integer.
//
private static long Int64(this ptr<Const> _addr_c) => func((_, panic, _) => {
    ref Const c = ref _addr_c.val;

    {
        var x = constant.ToInt(c.Value);


        if (x.Kind() == constant.Int) 
            {
                var (i, ok) = constant.Int64Val(x);

                if (ok) {
                    return i;
                }

            }

            return 0;
        else if (x.Kind() == constant.Float) 
            var (f, _) = constant.Float64Val(x);
            return int64(f);

    }
    panic(fmt.Sprintf("unexpected constant value: %T", c.Value));

});

// Uint64 returns the numeric value of this constant truncated to fit
// an unsigned 64-bit integer.
//
private static ulong Uint64(this ptr<Const> _addr_c) => func((_, panic, _) => {
    ref Const c = ref _addr_c.val;

    {
        var x = constant.ToInt(c.Value);


        if (x.Kind() == constant.Int) 
            {
                var (u, ok) = constant.Uint64Val(x);

                if (ok) {
                    return u;
                }

            }

            return 0;
        else if (x.Kind() == constant.Float) 
            var (f, _) = constant.Float64Val(x);
            return uint64(f);

    }
    panic(fmt.Sprintf("unexpected constant value: %T", c.Value));

});

// Float64 returns the numeric value of this constant truncated to fit
// a float64.
//
private static double Float64(this ptr<Const> _addr_c) {
    ref Const c = ref _addr_c.val;

    var (f, _) = constant.Float64Val(c.Value);
    return f;
}

// Complex128 returns the complex value of this constant truncated to
// fit a complex128.
//
private static System.Numerics.Complex128 Complex128(this ptr<Const> _addr_c) {
    ref Const c = ref _addr_c.val;

    var (re, _) = constant.Float64Val(constant.Real(c.Value));
    var (im, _) = constant.Float64Val(constant.Imag(c.Value));
    return complex(re, im);
}

} // end ssa_package
