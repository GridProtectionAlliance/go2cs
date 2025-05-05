// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements typechecking of index/slice expressions.
namespace go.go;

using ast = go.ast_package;
using constant = go.constant_package;
using typeparams = go.@internal.typeparams_package;
using static @internal.types.errors_package;
using go.@internal;

partial class types_package {

// If e is a valid function instantiation, indexExpr returns true.
// In that case x represents the uninstantiated function value and
// it is the caller's responsibility to instantiate the function.
[GoRecv] public static bool /*isFuncInst*/ indexExpr(this ref Checker check, ж<operand> Ꮡx, ж<typeparams.IndexExpr> Ꮡe) {
    bool isFuncInst = default!;

    ref var x = ref Ꮡx.val;
    ref var e = ref Ꮡe.val;
    check.exprOrType(Ꮡx, e.X, true);
    // x may be generic
    var exprᴛ1 = x.mode;
    if (exprᴛ1 == invalid) {
        check.use(e.Indices.ꓸꓸꓸ);
        return false;
    }
    if (exprᴛ1 == typexpr) {
        x.mode = invalid;
        x.typ = check.varType(e.Orig);
        if (isValid(x.typ)) {
            // type instantiation
            // TODO(gri) here we re-evaluate e.X - try to avoid this
            x.mode = typexpr;
        }
        return false;
    }
    if (exprᴛ1 == value) {
        {
            var (sig, _) = under(x.typ)._<ΔSignature.val>(ᐧ); if (sig != nil && sig.TypeParams().Len() > 0) {
                // function instantiation
                return true;
            }
        }
    }

    // x should not be generic at this point, but be safe and check
    check.nonGeneric(nil, Ꮡx);
    if (x.mode == invalid) {
        return false;
    }
    // ordinary index expression
    var valid = false;
    var length = ((int64)(-1));
    // valid if >= 0
    switch (under(x.typ).type()) {
    case Basic.val typ: {
        if (isString(~typ)) {
            valid = true;
            if (x.mode == constant_) {
                length = ((int64)len(constant.StringVal(x.val)));
            }
            // an indexed string always yields a byte value
            // (not a constant) even if the string and the
            // index are constant
            x.mode = value;
            x.typ = universeByte;
        }
        break;
    }
    case Array.val typ: {
        valid = true;
        length = typ.val.len;
        if (x.mode != variable) {
            // use 'byte' name
            x.mode = value;
        }
        x.typ = typ.val.elem;
        break;
    }
    case Pointer.val typ: {
        {
            var (typ, _) = under((~typ).@base)._<Array.val>(ᐧ); if (typ != nil) {
                valid = true;
                length = typ.val.len;
                x.mode = variable;
                x.typ = typ.val.elem;
            }
        }
        break;
    }
    case Slice.val typ: {
        valid = true;
        x.mode = variable;
        x.typ = typ.val.elem;
        break;
    }
    case Map.val typ: {
        var indexΔ1 = check.singleIndex(Ꮡe);
        if (indexΔ1 == default!) {
            x.mode = invalid;
            return false;
        }
        ref var keyΔ1 = ref heap(new operand(), out var ᏑkeyΔ1);
        check.expr(nil, ᏑkeyΔ1, indexΔ1);
        check.assignment(ᏑkeyΔ1, (~typ).key, "map index"u8);
        x.mode = mapindex;
        x.typ = typ.val.elem;
        x.expr = e.Orig;
        return false;
    }
    case Interface.val typ: {
        if (!isTypeParam(x.typ)) {
            // ok to continue even if indexing failed - map element type is known
            break;
        }
        // TODO(gri) report detailed failure cause for better error messages
        ΔType key = default!;             // key != nil: we must have all maps
        ΔType elem = default!;
        var mode = variable;
        if (typ.typeSet().underIs(
        var elemʗ2 = elem;
        var keyʗ2 = key;
        (ΔType u) => {
            var l = ((int64)(-1));
            ΔType kΔ1 = default!;
            ΔType eΔ1 = default!;
            switch (u.type()) {
            case Basic.val t: {
                if (isString(~t)) {
                    eΔ1 = universeByte;
                    mode = value;
                }
                break;
            }
            case Array.val t: {
                l = t.val.len;
                eΔ1 = t.val.elemʗ2;
                if (x.mode != variable) {
                    mode = value;
                }
                break;
            }
            case Pointer.val t: {
                {
                    var (t, _) = under((~t).@base)._<Array.val>(ᐧ); if (t != nil) {
                        l = t.val.len;
                        eΔ1 = t.val.elemʗ2;
                    }
                }
                break;
            }
            case Slice.val t: {
                eΔ1 = t.val.elemʗ2;
                break;
            }
            case Map.val t: {
                 = t.val.keyʗ2;
                eΔ1 = t.val.elemʗ2;
                break;
            }}
            if (eΔ1 == default!) {
                return false;
            }
            if (elemʗ2 == default!) {
                length = l;
                (keyʗ2, elemʗ2) = (kΔ1, eΔ1);
                return true;
            }
            if (!Identical(keyʗ2, kΔ1)) {
                return false;
            }
            if (!Identical(elemʗ2, eΔ1)) {
                return false;
            }
            if (l >= 0 && l < length) {
                length = l;
            }
            return true;
        })) {
            // For maps, the index expression must be assignable to the map key type.
            if (key != default!) {
                var indexΔ2 = check.singleIndex(Ꮡe);
                if (indexΔ2 == default!) {
                    x.mode = invalid;
                    return false;
                }
                ref var k = ref heap(new operand(), out var Ꮡk);
                check.expr(nil, Ꮡk, indexΔ2);
                check.assignment(Ꮡk, key, "map index"u8);
                // ok to continue even if indexing failed - map element type is known
                x.mode = mapindex;
                x.typ = elem;
                x.expr = e.Orig;
                return false;
            }
            // no maps
            valid = true;
            x.mode = mode;
            x.typ = elem;
        }
        break;
    }}
    if (!valid) {
        // types2 uses the position of '[' for the error
        check.errorf(~x, NonIndexableOperand, invalidOp + "cannot index %s", x);
        check.use(e.Indices.ꓸꓸꓸ);
        x.mode = invalid;
        return false;
    }
    var index = check.singleIndex(Ꮡe);
    if (index == default!) {
        x.mode = invalid;
        return false;
    }
    // In pathological (invalid) cases (e.g.: type T1 [][[]T1{}[0][0]]T0)
    // the element type may be accessed before it's set. Make sure we have
    // a valid type.
    if (x.typ == default!) {
        x.typ = Typ[Invalid];
    }
    check.index(index, length);
    return false;
}

[GoRecv] public static void sliceExpr(this ref Checker check, ж<operand> Ꮡx, ж<ast.SliceExpr> Ꮡe) {
    ref var x = ref Ꮡx.val;
    ref var e = ref Ꮡe.val;

    check.expr(nil, Ꮡx, e.X);
    if (x.mode == invalid) {
        check.use(e.Low, e.High, e.Max);
        return;
    }
    var valid = false;
    var length = ((int64)(-1));
    // valid if >= 0
    switch (coreString(x.typ).type()) {
    case default! u: {
        check.errorf(~x, NonSliceableOperand, invalidOp + "cannot slice %s: %s has no core type", x, x.typ);
        x.mode = invalid;
        return;
    }
    case Basic.val u: {
        if (isString(~u)) {
            if (e.Slice3) {
                var at = e.Max;
                if (at == default!) {
                    at = ~e;
                }
                // e.Index[2] should be present but be careful
                check.error(at, InvalidSliceExpr, invalidOp + "3-index slice of string");
                x.mode = invalid;
                return;
            }
            valid = true;
            if (x.mode == constant_) {
                length = ((int64)len(constant.StringVal(x.val)));
            }
            // spec: "For untyped string operands the result
            // is a non-constant value of type string."
            if (isUntyped(x.typ)) {
                x.typ = Typ[ΔString];
            }
        }
        break;
    }
    case Array.val u: {
        valid = true;
        length = u.val.len;
        if (x.mode != variable) {
            check.errorf(~x, NonSliceableOperand, invalidOp + "cannot slice %s (value not addressable)", x);
            x.mode = invalid;
            return;
        }
        x.typ = Ꮡ(new Slice(elem: (~u).elem));
        break;
    }
    case Pointer.val u: {
        {
            var (u, _) = under((~u).@base)._<Array.val>(ᐧ); if (u != nil) {
                valid = true;
                length = u.val.len;
                x.typ = Ꮡ(new Slice(elem: (~u).elem));
            }
        }
        break;
    }
    case Slice.val u: {
        valid = true;
        break;
    }}
    // x.typ doesn't change
    if (!valid) {
        check.errorf(~x, NonSliceableOperand, invalidOp + "cannot slice %s", x);
        x.mode = invalid;
        return;
    }
    x.mode = value;
    // spec: "Only the first index may be omitted; it defaults to 0."
    if (e.Slice3 && (e.High == default! || e.Max == default!)) {
        check.error(inNode(~e, e.Rbrack), InvalidSyntaxTree, "2nd and 3rd index required in 3-index slice"u8);
        x.mode = invalid;
        return;
    }
    // check indices
    array<int64> ind = new(3);
    foreach (var (i, expr) in new ast.Expr[]{e.Low, e.High, e.Max}.slice()) {
        var xΔ1 = ((int64)(-1));
        switch (ᐧ) {
        case {} when expr is != default!: {
            var max = ((int64)(-1));
            if (length >= 0) {
                // The "capacity" is only known statically for strings, arrays,
                // and pointers to arrays, and it is the same as the length for
                // those types.
                max = length + 1;
            }
            {
                var (_, v) = check.index(expr, max); if (v >= 0) {
                    xΔ1 = v;
                }
            }
            break;
        }
        case {} when i is 0: {
            xΔ1 = 0;
            break;
        }
        case {} when length is >= 0: {
            xΔ1 = length;
            break;
        }}

        // default is 0 for the first index
        // default is length (== capacity) otherwise
        ind[i] = xΔ1;
    }
    // constant indices must be in range
    // (check.index already checks that existing indices >= 0)
L:
    foreach (var (i, xΔ2) in ind[..(int)(len(ind) - 1)]) {
        if (xΔ2 > 0) {
            foreach (var (j, y) in ind[(int)(i + 1)..]) {
                if (y >= 0 && y < xΔ2) {
                    // The value y corresponds to the expression e.Index[i+1+j].
                    // Because y >= 0, it must have been set from the expression
                    // when checking indices and thus e.Index[i+1+j] is not nil.
                    var at = new ast.Expr[]{e.Low, e.High, e.Max}.slice()[i + 1 + j];
                    check.errorf(at, SwappedSliceIndices, "invalid slice indices: %d < %d"u8, y, xΔ2);
                    goto break_L;
                }
            }
        }
    }
}

// only report one error, ok to continue

// singleIndex returns the (single) index from the index expression e.
// If the index is missing, or if there are multiple indices, an error
// is reported and the result is nil.
[GoRecv] public static ast.Expr singleIndex(this ref Checker check, ж<typeparams.IndexExpr> Ꮡexpr) {
    ref var expr = ref Ꮡexpr.val;

    if (len(expr.Indices) == 0) {
        check.errorf(expr.Orig, InvalidSyntaxTree, "index expression %v with 0 indices"u8, expr);
        return default!;
    }
    if (len(expr.Indices) > 1) {
        // TODO(rFindley) should this get a distinct error code?
        check.error(expr.Indices[1], InvalidIndex, invalidOp + "more than one index");
    }
    return expr.Indices[0];
}

// index checks an index expression for validity.
// If max >= 0, it is the upper bound for index.
// If the result typ is != Typ[Invalid], index is valid and typ is its (possibly named) integer type.
// If the result val >= 0, index is valid and val is its constant int value.
[GoRecv] internal static (ΔType typ, int64 val) index(this ref Checker check, ast.Expr index, int64 max) {
    ΔType typ = default!;
    int64 val = default!;

    typ = ~Typ[Invalid];
    val = -1;
    ref var x = ref heap(new operand(), out var Ꮡx);
    check.expr(nil, Ꮡx, index);
    if (!check.isValidIndex(Ꮡx, InvalidIndex, "index"u8, false)) {
        return (typ, val);
    }
    if (x.mode != constant_) {
        return (x.typ, -1);
    }
    if (x.val.Kind() == constant.Unknown) {
        return (typ, val);
    }
    var (v, ok) = constant.Int64Val(x.val);
    assert(ok);
    if (max >= 0 && v >= max) {
        check.errorf(~Ꮡx, InvalidIndex, invalidArg + "index %s out of bounds [0:%d]", x.val.String(), max);
        return (typ, val);
    }
    // 0 <= v [ && v < max ]
    return (x.typ, v);
}

[GoRecv] public static bool isValidIndex(this ref Checker check, ж<operand> Ꮡx, errors.Code code, @string what, bool allowNegative) {
    ref var x = ref Ꮡx.val;

    if (x.mode == invalid) {
        return false;
    }
    // spec: "a constant index that is untyped is given type int"
    check.convertUntyped(Ꮡx, ~Typ[Int]);
    if (x.mode == invalid) {
        return false;
    }
    // spec: "the index x must be of integer type or an untyped constant"
    if (!allInteger(x.typ)) {
        check.errorf(~x, code, invalidArg + "%s %s must be integer", what, x);
        return false;
    }
    if (x.mode == constant_) {
        // spec: "a constant index must be non-negative ..."
        if (!allowNegative && constant.Sign(x.val) < 0) {
            check.errorf(~x, code, invalidArg + "%s %s must not be negative", what, x);
            return false;
        }
        // spec: "... and representable by a value of type int"
        if (!representableConst(x.val, check, Typ[Int], Ꮡ(x.val))) {
            check.errorf(~x, code, invalidArg + "%s %s overflows int", what, x);
            return false;
        }
    }
    return true;
}

// indexedElts checks the elements (elts) of an array or slice composite literal
// against the literal's element type (typ), and the element indices against
// the literal length if known (length >= 0). It returns the length of the
// literal (maximum index value + 1).
[GoRecv] internal static int64 indexedElts(this ref Checker check, slice<ast.Expr> elts, ΔType typ, int64 length) {
    var visited = new map<int64, bool>(len(elts));
    int64 index = default!;
    int64 max = default!;
    foreach (var (_, e) in elts) {
        // determine and check index
        var validIndex = false;
        var eval = e;
        {
            var (kv, _) = e._<ж<ast.KeyValueExpr>>(ᐧ); if (kv != nil){
                {
                    var (typΔ1, i) = check.index((~kv).Key, length); if (isValid(typΔ1)) {
                        if (i >= 0){
                            index = i;
                            validIndex = true;
                        } else {
                            check.errorf(e, InvalidLitIndex, "index %s must be integer constant"u8, (~kv).Key);
                        }
                    }
                }
                eval = kv.val.Value;
            } else 
            if (length >= 0 && index >= length){
                check.errorf(e, OversizeArrayLit, "index %d is out of bounds (>= %d)"u8, index, length);
            } else {
                validIndex = true;
            }
        }
        // if we have a valid index, check for duplicate entries
        if (validIndex) {
            if (visited[index]) {
                check.errorf(e, DuplicateLitKey, "duplicate index %d in array or slice literal"u8, index);
            }
            visited[index] = true;
        }
        index++;
        if (index > max) {
            max = index;
        }
        // check element against composite literal element type
        ref var x = ref heap(new operand(), out var Ꮡx);
        check.exprWithHint(Ꮡx, eval, typ);
        check.assignment(Ꮡx, typ, "array or slice literal"u8);
    }
    return max;
}

} // end types_package
