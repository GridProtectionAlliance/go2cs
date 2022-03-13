// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements typechecking of index/slice expressions.

// package types -- go2cs converted at 2022 March 13 05:53:06 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Program Files\Go\src\go\types\index.go
namespace go.go;

using ast = go.ast_package;
using constant = go.constant_package;
using typeparams = go.@internal.typeparams_package;


// If e is a valid function instantiation, indexExpr returns true.
// In that case x represents the uninstantiated function value and
// it is the caller's responsibility to instantiate the function.

using System;
public static partial class types_package {

private static bool indexExpr(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<ast.IndexExpr> _addr_e) => func((_, panic, _) => {
    bool isFuncInst = default;
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;
    ref ast.IndexExpr e = ref _addr_e.val;

    check.exprOrType(x, e.X);


    if (x.mode == invalid) 
        check.use(typeparams.UnpackExpr(e.Index));
        return false;
    else if (x.mode == typexpr) 
        // type instantiation
        x.mode = invalid;
        x.typ = check.varType(e);
        if (x.typ != Typ[Invalid]) {
            x.mode = typexpr;
        }
        return false;
    else if (x.mode == value) 
        {
            var sig = asSignature(x.typ);

            if (sig != null && len(sig.tparams) > 0) { 
                // function instantiation
                return true;
            }
        }
        var valid = false;
    var length = int64(-1); // valid if >= 0
    switch (optype(x.typ).type()) {
        case ptr<Basic> typ:
            if (isString(typ)) {
                valid = true;
                if (x.mode == constant_) {
                    length = int64(len(constant.StringVal(x.val)));
                }
                x.mode = value;
                x.typ = universeByte; // use 'byte' name
            }
            break;
        case ptr<Array> typ:
            valid = true;
            length = typ.len;
            if (x.mode != variable) {
                x.mode = value;
            }
            x.typ = typ.elem;
            break;
        case ptr<Pointer> typ:
            {
                var typ__prev1 = typ;

                var typ = asArray(typ.@base);

                if (typ != null) {
                    valid = true;
                    length = typ.len;
                    x.mode = variable;
                    x.typ = typ.elem;
                }
                typ = typ__prev1;

            }
            break;
        case ptr<Slice> typ:
            valid = true;
            x.mode = variable;
            x.typ = typ.elem;
            break;
        case ptr<Map> typ:
            var index = check.singleIndex(e);
            if (index == null) {
                x.mode = invalid;
                return ;
            }
            ref operand key = ref heap(out ptr<operand> _addr_key);
            check.expr(_addr_key, index);
            check.assignment(_addr_key, typ.key, "map index"); 
            // ok to continue even if indexing failed - map element type is known
            x.mode = mapindex;
            x.typ = typ.elem;
            x.expr = e;
            return ;
            break;
        case ptr<_Sum> typ:
            Type tkey = default;            Type telem = default; // key is for map types only
 // key is for map types only
            nint nmaps = 0; // number of map types in sum type
            if (typ.@is(t => {
                Type e = default;
                switch (under(t).type()) {
                    case ptr<Basic> t:
                        if (isString(t)) {
                            e = universeByte;
                        }
                        break;
                    case ptr<Array> t:
                        e = t.elem;
                        break;
                    case ptr<Pointer> t:
                        {
                            var t__prev2 = t;

                            var t = asArray(t.@base);

                            if (t != null) {
                                e = t.elem;
                            }
                            t = t__prev2;

                        }
                        break;
                    case ptr<Slice> t:
                        e = t.elem;
                        break;
                    case ptr<Map> t:
                        if (tkey != null && !Identical(t.key, tkey)) {
                            return false;
                        }
                        tkey = t.key;
                        e = t.elem;
                        nmaps++;
                        break;
                    case ptr<_TypeParam> t:
                        check.errorf(x, 0, "type of %s contains a type parameter - cannot index (implementation restriction)", x);
                        break;
                    case ptr<instance> t:
                        panic("unimplemented");
                        break;
                }
                if (e == null || telem != null && !Identical(e, telem)) {
                    return false;
                }
                telem = e;
                return true;
            })) { 
                // If there are maps, the index expression must be assignable
                // to the map key type (as for simple map index expressions).
                if (nmaps > 0) {
                    index = check.singleIndex(e);
                    if (index == null) {
                        x.mode = invalid;
                        return ;
                    }
                    key = default;
                    check.expr(_addr_key, index);
                    check.assignment(_addr_key, tkey, "map index"); 
                    // ok to continue even if indexing failed - map element type is known

                    // If there are only maps, we are done.
                    if (nmaps == len(typ.types)) {
                        x.mode = mapindex;
                        x.typ = telem;
                        x.expr = e;
                        return ;
                    }
                    valid = isInteger(tkey); 
                    // avoid 2nd indexing error if indexing failed above
                    if (!valid && key.mode == invalid) {
                        x.mode = invalid;
                        return ;
                    }
                    x.mode = value; // map index expressions are not addressable
                }
                else
 { 
                    // no maps
                    valid = true;
                    x.mode = variable;
                }
                x.typ = telem;
            }
            break;

    }

    if (!valid) {
        check.invalidOp(x, _NonIndexableOperand, "cannot index %s", x);
        x.mode = invalid;
        return ;
    }
    index = check.singleIndex(e);
    if (index == null) {
        x.mode = invalid;
        return ;
    }
    if (x.typ == null) {
        x.typ = Typ[Invalid];
    }
    check.index(index, length);
    return false;
});

private static void sliceExpr(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<ast.SliceExpr> _addr_e) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;
    ref ast.SliceExpr e = ref _addr_e.val;

    check.expr(x, e.X);
    if (x.mode == invalid) {
        check.use(e.Low, e.High, e.Max);
        return ;
    }
    var valid = false;
    var length = int64(-1); // valid if >= 0
    switch (optype(x.typ).type()) {
        case ptr<Basic> typ:
            if (isString(typ)) {
                if (e.Slice3) {
                    check.invalidOp(x, _InvalidSliceExpr, "3-index slice of string");
                    x.mode = invalid;
                    return ;
                }
                valid = true;
                if (x.mode == constant_) {
                    length = int64(len(constant.StringVal(x.val)));
                } 
                // spec: "For untyped string operands the result
                // is a non-constant value of type string."
                if (typ.kind == UntypedString) {
                    x.typ = Typ[String];
                }
            }
            break;
        case ptr<Array> typ:
            valid = true;
            length = typ.len;
            if (x.mode != variable) {
                check.invalidOp(x, _NonSliceableOperand, "cannot slice %s (value not addressable)", x);
                x.mode = invalid;
                return ;
            }
            x.typ = addr(new Slice(elem:typ.elem));
            break;
        case ptr<Pointer> typ:
            {
                var typ__prev1 = typ;

                var typ = asArray(typ.@base);

                if (typ != null) {
                    valid = true;
                    length = typ.len;
                    x.typ = addr(new Slice(elem:typ.elem));
                }

                typ = typ__prev1;

            }
            break;
        case ptr<Slice> typ:
            valid = true; 
            // x.typ doesn't change
            break;
        case ptr<_Sum> typ:
            check.errorf(x, 0, "generic slice expressions not yet implemented");
            x.mode = invalid;
            return ;
            break;
        case ptr<_TypeParam> typ:
            check.errorf(x, 0, "generic slice expressions not yet implemented");
            x.mode = invalid;
            return ;
            break;

    }

    if (!valid) {
        check.invalidOp(x, _NonSliceableOperand, "cannot slice %s", x);
        x.mode = invalid;
        return ;
    }
    x.mode = value; 

    // spec: "Only the first index may be omitted; it defaults to 0."
    if (e.Slice3 && (e.High == null || e.Max == null)) {
        check.invalidAST(inNode(e, e.Rbrack), "2nd and 3rd index required in 3-index slice");
        x.mode = invalid;
        return ;
    }
    array<long> ind = new array<long>(3);
    {
        ast.Expr i__prev1 = i;

        foreach (var (__i, __expr) in new slice<ast.Expr>(new ast.Expr[] { e.Low, e.High, e.Max })) {
            i = __i;
            expr = __expr;
            var x = int64(-1);

            if (expr != null) 
                // The "capacity" is only known statically for strings, arrays,
                // and pointers to arrays, and it is the same as the length for
                // those types.
                var max = int64(-1);
                if (length >= 0) {
                    max = length + 1;
                }
                {
                    var (_, v) = check.index(expr, max);

                    if (v >= 0) {
                        x = v;
                    }

                }
            else if (i == 0) 
                // default is 0 for the first index
                x = 0;
            else if (length >= 0) 
                // default is length (== capacity) otherwise
                x = length;
                        ind[i] = x;
        }
        i = i__prev1;
    }

L:
    {
        ast.Expr i__prev1 = i;
        var x__prev1 = x;

        foreach (var (__i, __x) in ind[..(int)len(ind) - 1]) {
            i = __i;
            x = __x;
            if (x > 0) {
                foreach (var (_, y) in ind[(int)i + 1..]) {
                    if (y >= 0 && x > y) {
                        check.errorf(inNode(e, e.Rbrack), _SwappedSliceIndices, "swapped slice indices: %d > %d", x, y);
                        _breakL = true; // only report one error, ok to continue
                        break;
                    }
                }
            }
        }
        i = i__prev1;
        x = x__prev1;
    }
}

// singleIndex returns the (single) index from the index expression e.
// If the index is missing, or if there are multiple indices, an error
// is reported and the result is nil.
private static ast.Expr singleIndex(this ptr<Checker> _addr_check, ptr<ast.IndexExpr> _addr_e) {
    ref Checker check = ref _addr_check.val;
    ref ast.IndexExpr e = ref _addr_e.val;

    var index = e.Index;
    if (index == null) {
        check.invalidAST(e, "missing index for %s", e);
        return null;
    }
    var indexes = typeparams.UnpackExpr(index);
    if (len(indexes) == 0) {
        check.invalidAST(index, "index expression %v with 0 indices", index);
        return null;
    }
    if (len(indexes) > 1) { 
        // TODO(rFindley) should this get a distinct error code?
        check.invalidOp(indexes[1], _InvalidIndex, "more than one index");
    }
    return indexes[0];
}

// index checks an index expression for validity.
// If max >= 0, it is the upper bound for index.
// If the result typ is != Typ[Invalid], index is valid and typ is its (possibly named) integer type.
// If the result val >= 0, index is valid and val is its constant int value.
private static (Type, long) index(this ptr<Checker> _addr_check, ast.Expr index, long max) {
    Type typ = default;
    long val = default;
    ref Checker check = ref _addr_check.val;

    typ = Typ[Invalid];
    val = -1;

    ref operand x = ref heap(out ptr<operand> _addr_x);
    check.expr(_addr_x, index);
    if (!check.isValidIndex(_addr_x, _InvalidIndex, "index", false)) {
        return ;
    }
    if (x.mode != constant_) {
        return (x.typ, -1);
    }
    if (x.val.Kind() == constant.Unknown) {
        return ;
    }
    var (v, ok) = constant.Int64Val(x.val);
    assert(ok);
    if (max >= 0 && v >= max) {
        check.invalidArg(_addr_x, _InvalidIndex, "index %s is out of bounds", _addr_x);
        return ;
    }
    return (x.typ, v);
}

private static bool isValidIndex(this ptr<Checker> _addr_check, ptr<operand> _addr_x, errorCode code, @string what, bool allowNegative) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;

    if (x.mode == invalid) {
        return false;
    }
    check.convertUntyped(x, Typ[Int]);
    if (x.mode == invalid) {
        return false;
    }
    if (!isInteger(x.typ)) {
        check.invalidArg(x, code, "%s %s must be integer", what, x);
        return false;
    }
    if (x.mode == constant_) { 
        // spec: "a constant index must be non-negative ..."
        if (!allowNegative && constant.Sign(x.val) < 0) {
            check.invalidArg(x, code, "%s %s must not be negative", what, x);
            return false;
        }
        if (!representableConst(x.val, check, Typ[Int], _addr_x.val)) {
            check.invalidArg(x, code, "%s %s overflows int", what, x);
            return false;
        }
    }
    return true;
}

// indexElts checks the elements (elts) of an array or slice composite literal
// against the literal's element type (typ), and the element indices against
// the literal length if known (length >= 0). It returns the length of the
// literal (maximum index value + 1).
//
private static long indexedElts(this ptr<Checker> _addr_check, slice<ast.Expr> elts, Type typ, long length) {
    ref Checker check = ref _addr_check.val;

    var visited = make_map<long, bool>(len(elts));
    long index = default;    long max = default;

    foreach (var (_, e) in elts) { 
        // determine and check index
        var validIndex = false;
        var eval = e;
        {
            ptr<ast.KeyValueExpr> (kv, _) = e._<ptr<ast.KeyValueExpr>>();

            if (kv != null) {
                {
                    var (typ, i) = check.index(kv.Key, length);

                    if (typ != Typ[Invalid]) {
                        if (i >= 0) {
                            index = i;
                            validIndex = true;
                        }
                        else
 {
                            check.errorf(e, _InvalidLitIndex, "index %s must be integer constant", kv.Key);
                        }
                    }

                }
                eval = kv.Value;
            }
            else if (length >= 0 && index >= length) {
                check.errorf(e, _OversizeArrayLit, "index %d is out of bounds (>= %d)", index, length);
            }
            else
 {
                validIndex = true;
            } 

            // if we have a valid index, check for duplicate entries

        } 

        // if we have a valid index, check for duplicate entries
        if (validIndex) {
            if (visited[index]) {
                check.errorf(e, _DuplicateLitKey, "duplicate index %d in array or slice literal", index);
            }
            visited[index] = true;
        }
        index++;
        if (index > max) {
            max = index;
        }
        ref operand x = ref heap(out ptr<operand> _addr_x);
        check.exprWithHint(_addr_x, eval, typ);
        check.assignment(_addr_x, typ, "array or slice literal");
    }    return max;
}

} // end types_package
