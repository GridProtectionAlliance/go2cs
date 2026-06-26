// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements typechecking of expressions.
namespace go.go;

using fmt = fmt_package;
using ast = go.ast_package;
using constant = go.constant_package;
using typeparams = go.@internal.typeparams_package;
using token = go.token_package;
using static @internal.types.errors_package;
using strings = strings_package;
using go.@internal;
using ꓸꓸꓸany = Span<any>;

partial class types_package {
/* visitMapType: map[token.Token]func(Type) bool */

/*
Basic algorithm:

Expressions are checked recursively, top down. Expression checker functions
are generally of the form:

  func f(x *operand, e *ast.Expr, ...)

where e is the expression to be checked, and x is the result of the check.
The check performed by f may fail in which case x.mode == invalid, and
related error messages will have been issued by f.

If a hint argument is present, it is the composite literal element type
of an outer composite literal; it is used to type-check composite literal
elements that have no explicit type specification in the source
(e.g.: []T{{...}, {...}}, the hint is the type T in this case).

All expressions are checked via rawExpr, which dispatches according
to expression kind. Upon returning, rawExpr is recording the types and
constant values for all expressions that have an untyped type (those types
may change on the way up in the expression tree). Usually these are constants,
but the results of comparisons or non-constant shifts of untyped constants
may also be untyped, but not constant.

Untyped expressions may eventually become fully typed (i.e., not untyped),
typically when the value is assigned to a variable, or is used otherwise.
The updateExprType method is used to record this final type and update
the recorded types: the type-checked expression tree is again traversed down,
and the new type is propagated as needed. Untyped constant expression values
that become fully typed must now be representable by the full type (constant
sub-expression trees are left alone except for their roots). This mechanism
ensures that a client sees the actual (run-time) type an untyped value would
have. It also permits type-checking of lhs shift operands "as if the shift
were not present": when updateExprType visits an untyped lhs shift operand
and assigns it it's final type, that type must be an integer type, and a
constant lhs must be representable as an integer.

When an expression gets its final type, either on the way out from rawExpr,
on the way down in updateExprType, or at the end of the type checker run,
the type (and constant value, if any) is recorded via Info.Types, if present.
*/
internal static opPredicates unaryOpPredicates;

[GoInit] internal static void init() {
    // Setting unaryOpPredicates in init avoids declaration cycles.
    unaryOpPredicates = new opPredicates{
        token.ADD: allNumeric,
        token.SUB: allNumeric,
        token.XOR: allInteger,
        token.NOT: allBoolean
    };
}

[GoRecv] public static bool op(this ref Checker check, opPredicates m, ж<operand> Ꮡx, token.Token op) {
    ref var x = ref Ꮡx.val;

    {
        var pred = m[op]; if (pred != default!){
            if (!pred(x.typ)) {
                check.errorf(~x, UndefinedOp, invalidOp + "operator %s not defined on %s", op, x);
                return false;
            }
        } else {
            check.errorf(~x, InvalidSyntaxTree, "unknown operator %s"u8, op);
            return false;
        }
    }
    return true;
}

// opName returns the name of the operation if x is an operation
// that might overflow; otherwise it returns the empty string.
internal static @string opName(ast.Expr e) {
    switch (e.type()) {
    case ж<ast.BinaryExpr> e: {
        if (((nint)(~e).Op) < len(op2str2)) {
            return op2str2[(~e).Op];
        }
        break;
    }
    case ж<ast.UnaryExpr> e: {
        if (((nint)(~e).Op) < len(op2str1)) {
            return op2str1[(~e).Op];
        }
        break;
    }}
    return ""u8;
}

internal static array<@string> op2str1 = new runtime.SparseArray<@string>{
    [token.XOR] = "bitwise complement"u8
}.array();

// This is only used for operations that may cause overflow.
internal static array<@string> op2str2 = new runtime.SparseArray<@string>{
    [token.ADD] = "addition"u8,
    [token.SUB] = "subtraction"u8,
    [token.XOR] = "bitwise XOR"u8,
    [token.MUL] = "multiplication"u8,
    [token.SHL] = "shift"u8
}.array();

// If typ is a type parameter, underIs returns the result of typ.underIs(f).
// Otherwise, underIs returns the result of f(under(typ)).
internal static bool underIs(ΔType typ, Func<ΔType, bool> f) {
    typ = Unalias(typ);
    {
        var (tpar, _) = typ._<TypeParam.val>(ᐧ); if (tpar != nil) {
            return tpar.underIs(f);
        }
    }
    return f(under(typ));
}

// The unary expression e may be nil. It's passed in for better error messages only.
[GoRecv] public static void unary(this ref Checker check, ж<operand> Ꮡx, ж<ast.UnaryExpr> Ꮡe) {
    ref var x = ref Ꮡx.val;
    ref var e = ref Ꮡe.val;

    check.expr(nil, Ꮡx, e.X);
    if (x.mode == invalid) {
        return;
    }
    token.Token op = e.Op;
    var exprᴛ1 = op;
    if (exprᴛ1 == token.AND) {
        {
            var (_, ok) = ast.Unparen(e.X)._<ж<ast.CompositeLit>>(ᐧ); if (!ok && x.mode != variable) {
                // spec: "As an exception to the addressability
                // requirement x may also be a composite literal."
                check.errorf(~x, UnaddressableOperand, invalidOp + "cannot take address of %s", x);
                x.mode = invalid;
                return;
            }
        }
        x.mode = value;
        x.typ = Ꮡ(new Pointer(@base: x.typ));
        return;
    }
    if (exprᴛ1 == token.ARROW) {
        var u = coreType(x.typ);
        if (u == default!) {
            check.errorf(~x, InvalidReceive, invalidOp + "cannot receive from %s (no core type)", x);
            x.mode = invalid;
            return;
        }
        var (ch, _) = u._<Chan.val>(ᐧ);
        if (ch == nil) {
            check.errorf(~x, InvalidReceive, invalidOp + "cannot receive from non-channel %s", x);
            x.mode = invalid;
            return;
        }
        if ((~ch).dir == SendOnly) {
            check.errorf(~x, InvalidReceive, invalidOp + "cannot receive from send-only channel %s", x);
            x.mode = invalid;
            return;
        }
        x.mode = commaok;
        x.typ = ch.val.elem;
        check.hasCallOrRecv = true;
        return;
    }
    if (exprᴛ1 == token.TILDE) {
        if (!allInteger(x.typ)) {
            // Provide a better error position and message than what check.op below would do.
            check.error(~e, UndefinedOp, "cannot use ~ outside of interface or type constraint"u8);
            x.mode = invalid;
            return;
        }
        check.error(~e, UndefinedOp, "cannot use ~ outside of interface or type constraint (use ^ for bitwise complement)"u8);
        op = token.XOR;
    }

    if (!check.op(unaryOpPredicates, Ꮡx, op)) {
        x.mode = invalid;
        return;
    }
    if (x.mode == constant_) {
        if (x.val.Kind() == constant.Unknown) {
            // nothing to do (and don't cause an error below in the overflow check)
            return;
        }
        nuint prec = default!;
        if (isUnsigned(x.typ)) {
            prec = ((nuint)(check.conf.@sizeof(x.typ) * 8));
        }
        x.val = constant.UnaryOp(op, x.val, prec);
        x.expr = e;
        check.overflow(Ꮡx, x.Pos());
        return;
    }
    x.mode = value;
}

// x.typ remains unchanged
internal static bool isShift(token.Token op) {
    return op == token.SHL || op == token.SHR;
}

internal static bool isComparison(token.Token op) {
    // Note: tokens are not ordered well to make this much easier
    var exprᴛ1 = op;
    if (exprᴛ1 == token.EQL || exprᴛ1 == token.NEQ || exprᴛ1 == token.LSS || exprᴛ1 == token.LEQ || exprᴛ1 == token.GTR || exprᴛ1 == token.GEQ) {
        return true;
    }

    return false;
}

// updateExprType updates the type of x to typ and invokes itself
// recursively for the operands of x, depending on expression kind.
// If typ is still an untyped and not the final type, updateExprType
// only updates the recorded untyped type for x and possibly its
// operands. Otherwise (i.e., typ is not an untyped type anymore,
// or it is the final type for x), the type and value are recorded.
// Also, if x is a constant, it must be representable as a value of typ,
// and if x is the (formerly untyped) lhs operand of a non-constant
// shift, it must be an integer value.
[GoRecv] internal static void updateExprType(this ref Checker check, ast.Expr x, ΔType typ, bool final) {
    check.updateExprType0(default!, x, typ, final);
}

[GoRecv] internal static void updateExprType0(this ref Checker check, ast.Expr parent, ast.Expr x, ΔType typ, bool final) {
    var (old, found) = check.untyped[x];
    if (!found) {
        return;
    }
    // nothing to do
    // update operands of x if necessary
    switch (x.type()) {
    case ж<ast.BadExpr> x: {
        if (debug) {
            // These expression are never untyped - nothing to do.
            // The respective sub-expressions got their final types
            // upon assignment or use.
            check.dump("%v: found old type(%s): %s (new: %s)"u8, x.Pos(), x, old.typ, typ);
            throw panic("unreachable");
        }
        return;
    }
    case ж<ast.FuncLit> x: {
        if (debug) {
            check.dump("%v: found old type(%s): %s (new: %s)"u8, x.Pos(), x, old.typ, typ);
            throw panic("unreachable");
        }
        return;
    }
    case ж<ast.CompositeLit> x: {
        if (debug) {
            check.dump("%v: found old type(%s): %s (new: %s)"u8, x.Pos(), x, old.typ, typ);
            throw panic("unreachable");
        }
        return;
    }
    case ж<ast.IndexExpr> x: {
        if (debug) {
            check.dump("%v: found old type(%s): %s (new: %s)"u8, x.Pos(), x, old.typ, typ);
            throw panic("unreachable");
        }
        return;
    }
    case ж<ast.SliceExpr> x: {
        if (debug) {
            check.dump("%v: found old type(%s): %s (new: %s)"u8, x.Pos(), x, old.typ, typ);
            throw panic("unreachable");
        }
        return;
    }
    case ж<ast.TypeAssertExpr> x: {
        if (debug) {
            check.dump("%v: found old type(%s): %s (new: %s)"u8, x.Pos(), x, old.typ, typ);
            throw panic("unreachable");
        }
        return;
    }
    case ж<ast.StarExpr> x: {
        if (debug) {
            check.dump("%v: found old type(%s): %s (new: %s)"u8, x.Pos(), x, old.typ, typ);
            throw panic("unreachable");
        }
        return;
    }
    case ж<ast.KeyValueExpr> x: {
        if (debug) {
            check.dump("%v: found old type(%s): %s (new: %s)"u8, x.Pos(), x, old.typ, typ);
            throw panic("unreachable");
        }
        return;
    }
    case ж<ast.ArrayType> x: {
        if (debug) {
            check.dump("%v: found old type(%s): %s (new: %s)"u8, x.Pos(), x, old.typ, typ);
            throw panic("unreachable");
        }
        return;
    }
    case ж<ast.StructType> x: {
        if (debug) {
            check.dump("%v: found old type(%s): %s (new: %s)"u8, x.Pos(), x, old.typ, typ);
            throw panic("unreachable");
        }
        return;
    }
    case ж<ast.FuncType> x: {
        if (debug) {
            check.dump("%v: found old type(%s): %s (new: %s)"u8, x.Pos(), x, old.typ, typ);
            throw panic("unreachable");
        }
        return;
    }
    case ж<ast.InterfaceType> x: {
        if (debug) {
            check.dump("%v: found old type(%s): %s (new: %s)"u8, x.Pos(), x, old.typ, typ);
            throw panic("unreachable");
        }
        return;
    }
    case ж<ast.MapType> x: {
        if (debug) {
            check.dump("%v: found old type(%s): %s (new: %s)"u8, x.Pos(), x, old.typ, typ);
            throw panic("unreachable");
        }
        return;
    }
    case ж<ast.ChanType> x: {
        if (debug) {
            check.dump("%v: found old type(%s): %s (new: %s)"u8, x.Pos(), x, old.typ, typ);
            throw panic("unreachable");
        }
        return;
    }
    case ж<ast.CallExpr> x: {
    }
    case ж<ast.Ident> x: {
    }
    case ж<ast.BasicLit> x: {
    }
    case ж<ast.SelectorExpr> x: {
    }
    case ж<ast.ParenExpr> x: {
        check.updateExprType0(~x, // Resulting in an untyped constant (e.g., built-in complex).
 // The respective calls take care of calling updateExprType
 // for the arguments if necessary.
 // An identifier denoting a constant, a constant literal,
 // or a qualified identifier (imported untyped constant).
 // No operands to take care of.
 (~x).X, typ, final);
        break;
    }
    case ж<ast.UnaryExpr> x: {
        if (old.val != default!) {
            // If x is a constant, the operands were constants.
            // The operands don't need to be updated since they
            // never get "materialized" into a typed value. If
            // left in the untyped map, they will be processed
            // at the end of the type check.
            break;
        }
        check.updateExprType0(~x, (~x).X, typ, final);
        break;
    }
    case ж<ast.BinaryExpr> x: {
        if (old.val != default!) {
            break;
        }
        if (isComparison((~x).Op)){
        } else 
        if (isShift((~x).Op)){
            // see comment for unary expressions
            // The result type is independent of operand types
            // and the operand types must have final types.
            // The result type depends only on lhs operand.
            // The rhs type was updated when checking the shift.
            check.updateExprType0(~x, (~x).X, typ, final);
        } else {
            // The operand types match the result type.
            check.updateExprType0(~x, (~x).X, typ, final);
            check.updateExprType0(~x, (~x).Y, typ, final);
        }
        break;
    }
    default: {
        var x = x.type();
        throw panic("unreachable");
        break;
    }}
    // If the new type is not final and still untyped, just
    // update the recorded type.
    if (!final && isUntyped(typ)) {
        old.typ = under(typ)._<Basic.val>();
        check.untyped[x] = old;
        return;
    }
    // Otherwise we have the final (typed or untyped type).
    // Remove it from the map of yet untyped expressions.
    delete(check.untyped, x);
    if (old.isLhs) {
        // If x is the lhs of a shift, its final type must be integer.
        // We already know from the shift check that it is representable
        // as an integer if it is a constant.
        if (!allInteger(typ)) {
            check.errorf(x, InvalidShiftOperand, invalidOp + "shifted operand %s (type %s) must be integer", x, typ);
            return;
        }
    }
    // Even if we have an integer, if the value is a constant we
    // still must check that it is representable as the specific
    // int type requested (was go.dev/issue/22969). Fall through here.
    if (old.val != default!) {
        // If x is a constant, it must be representable as a value of typ.
        ref var c = ref heap<operand>(out var Ꮡc);
        c = new operand(old.mode, x, old.typ, old.val, 0);
        check.convertUntyped(Ꮡc, typ);
        if (c.mode == invalid) {
            return;
        }
    }
    // Everything's fine, record final type and value for x.
    check.recordTypeAndValue(x, old.mode, typ, old.val);
}

// updateExprVal updates the value of x to val.
[GoRecv] internal static void updateExprVal(this ref Checker check, ast.Expr x, constant.Value val) {
    {
        var (info, ok) = check.untyped[x]; if (ok) {
            info.val = val;
            check.untyped[x] = info;
        }
    }
}

// implicitTypeAndValue returns the implicit type of x when used in a context
// where the target type is expected. If no such implicit conversion is
// possible, it returns a nil Type and non-zero error code.
//
// If x is a constant operand, the returned constant.Value will be the
// representation of x in this context.
[GoRecv] public static (ΔType, constant.Value, errors.Code) implicitTypeAndValue(this ref Checker check, ж<operand> Ꮡx, ΔType target) {
    ref var x = ref Ꮡx.val;

    if (x.mode == invalid || isTyped(x.typ) || !isValid(target)) {
        return (x.typ, default!, 0);
    }
    // x is untyped
    if (isUntyped(target)) {
        // both x and target are untyped
        {
            var m = maxType(x.typ, target); if (m != default!) {
                return (m, default!, 0);
            }
        }
        return (default!, default!, InvalidUntypedConversion);
    }
    switch (under(target).type()) {
    case Basic.val u: {
        if (x.mode == constant_) {
            var (v, code) = check.representation(Ꮡx, u);
            if (code != 0) {
                return (default!, default!, code);
            }
            return (target, v, code);
        }
        var exprᴛ1 = x.typ._<Basic.val>().kind;
        if (exprᴛ1 == UntypedBool) {
            if (!isBoolean(target)) {
                // Non-constant untyped values may appear as the
                // result of comparisons (untyped bool), intermediate
                // (delayed-checked) rhs operands of shifts, and as
                // the value nil.
                return (default!, default!, InvalidUntypedConversion);
            }
        }
        if (exprᴛ1 == ΔUntypedInt || exprᴛ1 == UntypedRune || exprᴛ1 == ΔUntypedFloat || exprᴛ1 == ΔUntypedComplex) {
            if (!isNumeric(target)) {
                return (default!, default!, InvalidUntypedConversion);
            }
        }
        if (exprᴛ1 == UntypedString) {
            if (!isString(target)) {
                // Non-constant untyped string values are not permitted by the spec and
                // should not occur during normal typechecking passes, but this path is
                // reachable via the AssignableTo API.
                return (default!, default!, InvalidUntypedConversion);
            }
        }
        if (exprᴛ1 == UntypedNil) {
            if (!hasNil(target)) {
                // Unsafe.Pointer is a basic type that includes nil.
                return (default!, default!, InvalidUntypedConversion);
            }
            return (~Typ[UntypedNil], default!, 0);
        }
        { /* default: */
            return (default!, default!, InvalidUntypedConversion);
        }

        break;
    }
    case Interface.val u: {
        if (isTypeParam(target)) {
            // Preserve the type of nil as UntypedNil: see go.dev/issue/13061.
            if (!u.typeSet().underIs((ΔType u) => {
                if (u == default!) {
                    return false;
                }
                var (t, _, _) = check.implicitTypeAndValue(Ꮡx, u);
                return t != default!;
            })) {
                return (default!, default!, InvalidUntypedConversion);
            }
            // keep nil untyped (was bug go.dev/issue/39755)
            if (x.isNil()) {
                return (~Typ[UntypedNil], default!, 0);
            }
            break;
        }
        if (x.isNil()) {
            // Values must have concrete dynamic types. If the value is nil,
            // keep it untyped (this is important for tools such as go vet which
            // need the dynamic type for argument checking of say, print
            // functions)
            return (~Typ[UntypedNil], default!, 0);
        }
        if (!u.Empty()) {
            // cannot assign untyped values to non-empty interfaces
            return (default!, default!, InvalidUntypedConversion);
        }
        return (Default(x.typ), default!, 0);
    }
    case Pointer.val u: {
        if (!x.isNil()) {
            return (default!, default!, InvalidUntypedConversion);
        }
        return (~Typ[UntypedNil], default!, 0);
    }
    case ΔSignature.val u: {
        if (!x.isNil()) {
            return (default!, default!, InvalidUntypedConversion);
        }
        return (~Typ[UntypedNil], default!, 0);
    }
    case Slice.val u: {
        if (!x.isNil()) {
            return (default!, default!, InvalidUntypedConversion);
        }
        return (~Typ[UntypedNil], default!, 0);
    }
    case Map.val u: {
        if (!x.isNil()) {
            return (default!, default!, InvalidUntypedConversion);
        }
        return (~Typ[UntypedNil], default!, 0);
    }
    case Chan.val u: {
        if (!x.isNil()) {
            return (default!, default!, InvalidUntypedConversion);
        }
        return (~Typ[UntypedNil], default!, 0);
    }
    default: {
        var u = under(target).type();
        return (default!, default!, InvalidUntypedConversion);
    }}
    // Keep nil untyped - see comment for interfaces, above.
    return (target, default!, 0);
}

// If switchCase is true, the operator op is ignored.
[GoRecv] public static void comparison(this ref Checker check, ж<operand> Ꮡx, ж<operand> Ꮡy, token.Token op, bool switchCase) {
    ref var x = ref Ꮡx.val;
    ref var y = ref Ꮡy.val;

    // Avoid spurious errors if any of the operands has an invalid type (go.dev/issue/54405).
    if (!isValid(x.typ) || !isValid(y.typ)) {
        x.mode = invalid;
        return;
    }
    if (switchCase) {
        op = token.EQL;
    }
    var errOp = x;
    // operand for which error is reported, if any
    @string cause = ""u8;
    // specific error cause, if any
    // spec: "In any comparison, the first operand must be assignable
    // to the type of the second operand, or vice versa."
    errors.Code code = MismatchedTypes;
    var (ok, _) = x.assignableTo(check, y.typ, nil);
    if (!ok) {
        (ok, _) = y.assignableTo(check, x.typ, nil);
    }
    if (!ok) {
        // Report the error on the 2nd operand since we only
        // know after seeing the 2nd operand whether we have
        // a type mismatch.
        errOp = y;
        cause = check.sprintf("mismatched types %s and %s"u8, x.typ, y.typ);
        goto ΔError;
    }
    // check if comparison is defined for operands
    code = UndefinedOp;
    var exprᴛ1 = op;
    if (exprᴛ1 == token.EQL || exprᴛ1 == token.NEQ) {
        switch (ᐧ) {
        case {} when x.isNil() || y.isNil(): {
            var typ = x.typ;
            if (x.isNil()) {
                // spec: "The equality operators == and != apply to operands that are comparable."
                // Comparison against nil requires that the other operand type has nil.
                typ = y.typ;
            }
            if (!hasNil(typ)) {
                // This case should only be possible for "nil == nil".
                // Report the error on the 2nd operand since we only
                // know after seeing the 2nd operand whether we have
                // an invalid comparison.
                errOp = y;
                goto ΔError;
            }
            break;
        }
        case {} when !Comparable(x.typ): {
            errOp = x;
            cause = check.incomparableCause(x.typ);
            goto ΔError;
            break;
        }
        case {} when !Comparable(y.typ): {
            errOp = y;
            cause = check.incomparableCause(y.typ);
            goto ΔError;
            break;
        }}

    }
    else if (exprᴛ1 == token.LSS || exprᴛ1 == token.LEQ || exprᴛ1 == token.GTR || exprᴛ1 == token.GEQ) {
        switch (ᐧ) {
        case {} when !allOrdered(x.typ): {
            errOp = x;
            goto ΔError;
            break;
        }
        case {} when !allOrdered(y.typ): {
            errOp = y;
            goto ΔError;
            break;
        }}

    }
    else { /* default: */
        throw panic("unreachable");
    }

    // spec: The ordering operators <, <=, >, and >= apply to operands that are ordered."
    // comparison is ok
    if (x.mode == constant_ && y.mode == constant_){
        x.val = constant.MakeBool(constant.Compare(x.val, op, y.val));
    } else {
        // The operands are never materialized; no need to update
        // their types.
        x.mode = value;
        // The operands have now their final types, which at run-
        // time will be materialized. Update the expression trees.
        // If the current types are untyped, the materialized type
        // is the respective default type.
        check.updateExprType(x.expr, Default(x.typ), true);
        check.updateExprType(y.expr, Default(y.typ), true);
    }
    // spec: "Comparison operators compare two operands and yield
    //        an untyped boolean value."
    x.typ = Typ[UntypedBool];
    return;
ΔError:
    if (cause == ""u8) {
        // We have an offending operand errOp and possibly an error cause.
        if (isTypeParam(x.typ) || isTypeParam(y.typ)){
            // TODO(gri) should report the specific type causing the problem, if any
            if (!isTypeParam(x.typ)) {
                errOp = y;
            }
            cause = check.sprintf("type parameter %s is not comparable with %s"u8, (~errOp).typ, op);
        } else {
            cause = check.sprintf("operator %s not defined on %s"u8, op, check.kindString((~errOp).typ));
        }
    }
    // catch-all
    if (switchCase){
        check.errorf(~x, code, "invalid case %s in switch on %s (%s)"u8, x.expr, y.expr, cause);
    } else {
        // error position always at 1st operand
        check.errorf(~errOp, code, invalidOp + "%s %s %s (%s)", x.expr, op, y.expr, cause);
    }
    x.mode = invalid;
}

// incomparableCause returns a more specific cause why typ is not comparable.
// If there is no more specific cause, the result is "".
[GoRecv] internal static @string incomparableCause(this ref Checker check, ΔType typ) {
    switch (under(typ).type()) {
    case Slice.val : {
        return check.kindString(typ) + " can only be compared to nil"u8;
    }
    case ΔSignature.val : {
        return check.kindString(typ) + " can only be compared to nil"u8;
    }
    case Map.val : {
        return check.kindString(typ) + " can only be compared to nil"u8;
    }}

    // see if we can extract a more specific error
    @string cause = default!;
    comparable(typ, true, default!, (@string format, params ꓸꓸꓸany argsʗp) => {
        cause = check.sprintf(format, args.ꓸꓸꓸ);
    });
    return cause;
}

// kindString returns the type kind as a string.
[GoRecv] internal static @string kindString(this ref Checker check, ΔType typ) {
    switch (under(typ).type()) {
    case Array.val : {
        return "array"u8;
    }
    case Slice.val : {
        return "slice"u8;
    }
    case Struct.val : {
        return "struct"u8;
    }
    case Pointer.val : {
        return "pointer"u8;
    }
    case ΔSignature.val : {
        return "func"u8;
    }
    case Interface.val : {
        if (isTypeParam(typ)) {
            return check.sprintf("type parameter %s"u8, typ);
        }
        return "interface"u8;
    }
    case Map.val : {
        return "map"u8;
    }
    case Chan.val : {
        return "chan"u8;
    }
    default: {

        return check.sprintf("%s"u8, typ);
    }}

}

// catch-all

// If e != nil, it must be the shift expression; it may be nil for non-constant shifts.
[GoRecv] public static void shift(this ref Checker check, ж<operand> Ꮡx, ж<operand> Ꮡy, ast.Expr e, token.Token op) {
    ref var x = ref Ꮡx.val;
    ref var y = ref Ꮡy.val;

    // TODO(gri) This function seems overly complex. Revisit.
    constant.Value xval = default!;
    if (x.mode == constant_) {
        xval = constant.ToInt(x.val);
    }
    if (allInteger(x.typ) || isUntyped(x.typ) && xval != default! && xval.Kind() == constant.Int){
    } else {
        // The lhs is of integer type or an untyped constant representable
        // as an integer. Nothing to do.
        // shift has no chance
        check.errorf(~x, InvalidShiftOperand, invalidOp + "shifted operand %s must be integer", x);
        x.mode = invalid;
        return;
    }
    // spec: "The right operand in a shift expression must have integer type
    // or be an untyped constant representable by a value of type uint."
    // Check that constants are representable by uint, but do not convert them
    // (see also go.dev/issue/47243).
    constant.Value yval = default!;
    if (y.mode == constant_){
        // Provide a good error message for negative shift counts.
        yval = constant.ToInt(y.val);
        // consider -1, 1.0, but not -1.1
        if (yval.Kind() == constant.Int && constant.Sign(yval) < 0) {
            check.errorf(~y, InvalidShiftCount, invalidOp + "negative shift count %s", y);
            x.mode = invalid;
            return;
        }
        if (isUntyped(y.typ)) {
            // Caution: Check for representability here, rather than in the switch
            // below, because isInteger includes untyped integers (was bug go.dev/issue/43697).
            check.representable(Ꮡy, Typ[Uint]);
            if (y.mode == invalid) {
                x.mode = invalid;
                return;
            }
        }
    } else {
        // Check that RHS is otherwise at least of integer type.
        switch (ᐧ) {
        case {} when allInteger(y.typ): {
            if (!allUnsigned(y.typ) && !check.verifyVersionf(~y, go1_13, invalidOp + "signed shift count %s", y)) {
                x.mode = invalid;
                return;
            }
            break;
        }
        case {} when isUntyped(y.typ): {
            check.convertUntyped(Ꮡy, // This is incorrect, but preserves pre-existing behavior.
 // See also go.dev/issue/47410.
 ~Typ[Uint]);
            if (y.mode == invalid) {
                x.mode = invalid;
                return;
            }
            break;
        }
        default: {
            check.errorf(~y, InvalidShiftCount, invalidOp + "shift count %s must be integer", y);
            x.mode = invalid;
            return;
        }}

    }
    if (x.mode == constant_) {
        if (y.mode == constant_) {
            // if either x or y has an unknown value, the result is unknown
            if (x.val.Kind() == constant.Unknown || y.val.Kind() == constant.Unknown) {
                x.val = constant.MakeUnknown();
                // ensure the correct type - see comment below
                if (!isInteger(x.typ)) {
                    x.typ = Typ[ΔUntypedInt];
                }
                return;
            }
            // rhs must be within reasonable bounds in constant shifts
            static readonly UntypedInt shiftBound = /* 1023 - 1 + 52 */ 1074; // so we can express smallestFloat64 (see go.dev/issue/44057)
            var (s, ok) = constant.Uint64Val(yval);
            if (!ok || s > shiftBound) {
                check.errorf(~y, InvalidShiftCount, invalidOp + "invalid shift count %s", y);
                x.mode = invalid;
                return;
            }
            // The lhs is representable as an integer but may not be an integer
            // (e.g., 2.0, an untyped float) - this can only happen for untyped
            // non-integer numeric constants. Correct the type so that the shift
            // result is of integer type.
            if (!isInteger(x.typ)) {
                x.typ = Typ[ΔUntypedInt];
            }
            // x is a constant so xval != nil and it must be of Int kind.
            x.val = constant.Shift(xval, op, ((nuint)s));
            x.expr = e;
            tokenꓸPos opPos = x.Pos();
            {
                var (b, _) = e._<ж<ast.BinaryExpr>>(ᐧ); if (b != nil) {
                    opPos = b.val.OpPos;
                }
            }
            check.overflow(Ꮡx, opPos);
            return;
        }
        // non-constant shift with constant lhs
        if (isUntyped(x.typ)) {
            // spec: "If the left operand of a non-constant shift
            // expression is an untyped constant, the type of the
            // constant is what it would be if the shift expression
            // were replaced by its left operand alone.".
            //
            // Delay operand checking until we know the final type
            // by marking the lhs expression as lhs shift operand.
            //
            // Usually (in correct programs), the lhs expression
            // is in the untyped map. However, it is possible to
            // create incorrect programs where the same expression
            // is evaluated twice (via a declaration cycle) such
            // that the lhs expression type is determined in the
            // first round and thus deleted from the map, and then
            // not found in the second round (double insertion of
            // the same expr node still just leads to one entry for
            // that node, and it can only be deleted once).
            // Be cautious and check for presence of entry.
            // Example: var e, f = int(1<<""[f]) // go.dev/issue/11347
            {
                var (info, found) = check.untyped[x.expr]; if (found) {
                    info.isLhs = true;
                    check.untyped[x.expr] = info;
                }
            }
            // keep x's type
            x.mode = value;
            return;
        }
    }
    // non-constant shift - lhs must be an integer
    if (!allInteger(x.typ)) {
        check.errorf(~x, InvalidShiftOperand, invalidOp + "shifted operand %s must be integer", x);
        x.mode = invalid;
        return;
    }
    x.mode = value;
}

internal static opPredicates binaryOpPredicates;

[GoInit] internal static void initΔ2() {
    // Setting binaryOpPredicates in init avoids declaration cycles.
    binaryOpPredicates = new opPredicates{
        token.ADD: allNumericOrString,
        token.SUB: allNumeric,
        token.MUL: allNumeric,
        token.QUO: allNumeric,
        token.REM: allInteger,
        token.AND: allInteger,
        token.OR: allInteger,
        token.XOR: allInteger,
        token.AND_NOT: allInteger,
        token.LAND: allBoolean,
        token.LOR: allBoolean
    };
}

// If e != nil, it must be the binary expression; it may be nil for non-constant expressions
// (when invoked for an assignment operation where the binary expression is implicit).
[GoRecv] public static void binary(this ref Checker check, ж<operand> Ꮡx, ast.Expr e, ast.Expr lhs, ast.Expr rhs, token.Token op, tokenꓸPos opPos) {
    ref var x = ref Ꮡx.val;

    ref var y = ref heap(new operand(), out var Ꮡy);
    check.expr(nil, Ꮡx, lhs);
    check.expr(nil, Ꮡy, rhs);
    if (x.mode == invalid) {
        return;
    }
    if (y.mode == invalid) {
        x.mode = invalid;
        x.expr = y.expr;
        return;
    }
    if (isShift(op)) {
        check.shift(Ꮡx, Ꮡy, e, op);
        return;
    }
    check.matchTypes(Ꮡx, Ꮡy);
    if (x.mode == invalid) {
        return;
    }
    if (isComparison(op)) {
        check.comparison(Ꮡx, Ꮡy, op, false);
        return;
    }
    if (!Identical(x.typ, y.typ)) {
        // only report an error if we have valid types
        // (otherwise we had an error reported elsewhere already)
        if (isValid(x.typ) && isValid(y.typ)) {
            positioner posn = x;
            if (e != default!) {
                posn = e;
            }
            if (e != default!){
                check.errorf(posn, MismatchedTypes, invalidOp + "%s (mismatched types %s and %s)", e, x.typ, y.typ);
            } else {
                check.errorf(posn, MismatchedTypes, invalidOp + "%s %s= %s (mismatched types %s and %s)", lhs, op, rhs, x.typ, y.typ);
            }
        }
        x.mode = invalid;
        return;
    }
    if (!check.op(binaryOpPredicates, Ꮡx, op)) {
        x.mode = invalid;
        return;
    }
    if (op == token.QUO || op == token.REM) {
        // check for zero divisor
        if ((x.mode == constant_ || allInteger(x.typ)) && y.mode == constant_ && constant.Sign(y.val) == 0) {
            check.error(~Ꮡy, DivByZero, invalidOp + "division by zero");
            x.mode = invalid;
            return;
        }
        // check for divisor underflow in complex division (see go.dev/issue/20227)
        if (x.mode == constant_ && y.mode == constant_ && isComplex(x.typ)) {
            var re = constant.Real(y.val);
            var im = constant.Imag(y.val);
            var re2 = constant.BinaryOp(re, token.MUL, re);
            var im2 = constant.BinaryOp(im, token.MUL, im);
            if (constant.Sign(re2) == 0 && constant.Sign(im2) == 0) {
                check.error(~Ꮡy, DivByZero, invalidOp + "division by zero");
                x.mode = invalid;
                return;
            }
        }
    }
    if (x.mode == constant_ && y.mode == constant_) {
        // if either x or y has an unknown value, the result is unknown
        if (x.val.Kind() == constant.Unknown || y.val.Kind() == constant.Unknown) {
            x.val = constant.MakeUnknown();
            // x.typ is unchanged
            return;
        }
        // force integer division of integer operands
        if (op == token.QUO && isInteger(x.typ)) {
            op = token.QUO_ASSIGN;
        }
        x.val = constant.BinaryOp(x.val, op, y.val);
        x.expr = e;
        check.overflow(Ꮡx, opPos);
        return;
    }
    x.mode = value;
}

// x.typ is unchanged

// matchTypes attempts to convert any untyped types x and y such that they match.
// If an error occurs, x.mode is set to invalid.
[GoRecv] public static void matchTypes(this ref Checker check, ж<operand> Ꮡx, ж<operand> Ꮡy) {
    ref var x = ref Ꮡx.val;
    ref var y = ref Ꮡy.val;

    // mayConvert reports whether the operands x and y may
    // possibly have matching types after converting one
    // untyped operand to the type of the other.
    // If mayConvert returns true, we try to convert the
    // operands to each other's types, and if that fails
    // we report a conversion failure.
    // If mayConvert returns false, we continue without an
    // attempt at conversion, and if the operand types are
    // not compatible, we report a type mismatch error.
    var mayConvert = (ж<operand> x, ж<operand> y) => {
        // If both operands are typed, there's no need for an implicit conversion.
        if (isTyped((~xΔ1).typ) && isTyped((~yΔ1).typ)) {
            return false;
        }
        // An untyped operand may convert to its default type when paired with an empty interface
        // TODO(gri) This should only matter for comparisons (the only binary operation that is
        //           valid with interfaces), but in that case the assignability check should take
        //           care of the conversion. Verify and possibly eliminate this extra test.
        if (isNonTypeParamInterface((~xΔ1).typ) || isNonTypeParamInterface((~yΔ1).typ)) {
            return true;
        }
        // A boolean type can only convert to another boolean type.
        if (allBoolean((~xΔ1).typ) != allBoolean((~yΔ1).typ)) {
            return false;
        }
        // A string type can only convert to another string type.
        if (allString((~xΔ1).typ) != allString((~yΔ1).typ)) {
            return false;
        }
        // Untyped nil can only convert to a type that has a nil.
        if (xΔ1.isNil()) {
            return hasNil((~yΔ1).typ);
        }
        if (yΔ1.isNil()) {
            return hasNil((~xΔ1).typ);
        }
        // An untyped operand cannot convert to a pointer.
        // TODO(gri) generalize to type parameters
        if (isPointer((~xΔ1).typ) || isPointer((~yΔ1).typ)) {
            return false;
        }
        return true;
    };
    if (mayConvert(Ꮡx, Ꮡy)) {
        check.convertUntyped(Ꮡx, y.typ);
        if (x.mode == invalid) {
            return;
        }
        check.convertUntyped(Ꮡy, x.typ);
        if (y.mode == invalid) {
            x.mode = invalid;
            return;
        }
    }
}

[GoType("num:nint")] partial struct exprKind;

internal static readonly exprKind Δconversion = /* iota */ 0;
internal static readonly exprKind expression = 1;
internal static readonly exprKind statement = 2;

// target represent the (signature) type and description of the LHS
// variable of an assignment, or of a function result variable.
[GoType] partial struct target {
    internal ж<ΔSignature> sig;
    internal @string desc;
}

// newTarget creates a new target for the given type and description.
// The result is nil if typ is not a signature.
internal static ж<target> newTarget(ΔType typ, @string desc) {
    if (typ != default!) {
        {
            var (sig, _) = under(typ)._<ΔSignature.val>(ᐧ); if (sig != nil) {
                return Ꮡ(new target(sig, desc));
            }
        }
    }
    return default!;
}

// rawExpr typechecks expression e and initializes x with the expression
// value or type. If an error occurred, x.mode is set to invalid.
// If a non-nil target T is given and e is a generic function,
// T is used to infer the type arguments for e.
// If hint != nil, it is the type of a composite literal element.
// If allowGeneric is set, the operand type may be an uninstantiated
// parameterized type or function value.
[GoRecv] public static exprKind rawExpr(this ref Checker check, ж<target> ᏑT, ж<operand> Ꮡx, ast.Expr e, ΔType hint, bool allowGeneric) => func((defer, _) => {
    ref var T = ref ᏑT.val;
    ref var x = ref Ꮡx.val;

    if (check.conf._Trace) {
        check.trace(e.Pos(), "-- expr %s"u8, e);
        check.indent++;
        defer(() => {
            check.indent--;
            check.trace(e.Pos(), "=> %s"u8, x);
        });
    }
    exprKind kind = check.exprInternal(ᏑT, Ꮡx, e, hint);
    if (!allowGeneric) {
        check.nonGeneric(ᏑT, Ꮡx);
    }
    check.record(Ꮡx);
    return kind;
});

// If x is a generic type, or a generic function whose type arguments cannot be inferred
// from a non-nil target T, nonGeneric reports an error and invalidates x.mode and x.typ.
// Otherwise it leaves x alone.
[GoRecv] public static void nonGeneric(this ref Checker check, ж<target> ᏑT, ж<operand> Ꮡx) {
    ref var T = ref ᏑT.val;
    ref var x = ref Ꮡx.val;

    if (x.mode == invalid || x.mode == novalue) {
        return;
    }
    @string what = default!;
    switch (x.typ.type()) {
    case Alias.val t: {
        if (isGeneric(t)) {
            what = "type"u8;
        }
        break;
    }
    case Named.val t: {
        if (isGeneric(t)) {
            what = "type"u8;
        }
        break;
    }
    case ΔSignature.val t: {
        if ((~t).tparams != nil) {
            if (enableReverseTypeInference && T != nil) {
                check.funcInst(ᏑT, x.Pos(), Ꮡx, nil, true);
                return;
            }
            what = "function"u8;
        }
        break;
    }}
    if (what != ""u8) {
        check.errorf(x.expr, WrongTypeArgCount, "cannot use generic %s %s without instantiation"u8, what, x.expr);
        x.mode = invalid;
        x.typ = Typ[Invalid];
    }
}

// langCompat reports an error if the representation of a numeric
// literal is not compatible with the current language version.
[GoRecv] public static void langCompat(this ref Checker check, ж<ast.BasicLit> Ꮡlit) {
    ref var lit = ref Ꮡlit.val;

    @string s = lit.Value;
    if (len(s) <= 2 || check.allowVersion(~lit, go1_13)) {
        return;
    }
    // len(s) > 2
    if (strings.Contains(s, "_"u8)) {
        check.versionErrorf(~lit, go1_13, "underscore in numeric literal"u8);
        return;
    }
    if (s[0] != (rune)'0') {
        return;
    }
    var radix = s[1];
    if (radix == (rune)'b' || radix == (rune)'B') {
        check.versionErrorf(~lit, go1_13, "binary literal"u8);
        return;
    }
    if (radix == (rune)'o' || radix == (rune)'O') {
        check.versionErrorf(~lit, go1_13, "0o/0O-style octal literal"u8);
        return;
    }
    if (lit.Kind != token.INT && (radix == (rune)'x' || radix == (rune)'X')) {
        check.versionErrorf(~lit, go1_13, "hexadecimal floating-point literal"u8);
    }
}

// exprInternal contains the core of type checking of expressions.
// Must only be called by rawExpr.
// (See rawExpr for an explanation of the parameters.)
[GoRecv] public static exprKind exprInternal(this ref Checker check, ж<target> ᏑT, ж<operand> Ꮡx, ast.Expr e, ΔType hint) {
    ref var T = ref ᏑT.val;
    ref var x = ref Ꮡx.val;

    // make sure x has a valid state in case of bailout
    // (was go.dev/issue/5770)
    x.mode = invalid;
    x.typ = Typ[Invalid];
    switch (e.type()) {
    case ж<ast.BadExpr> e: {
        goto ΔError;
        break;
    }
    case ж<ast.Ident> e: {
        check.ident(Ꮡx, // error was reported before
 Ꮡe, nil, false);
        break;
    }
    case ж<ast.Ellipsis> e: {
        check.error(~e, // ellipses are handled explicitly where they are legal
 // (array composite literals and parameter lists)
 BadDotDotDotSyntax, "invalid use of '...'"u8);
        goto ΔError;
        break;
    }
    case ж<ast.BasicLit> e: {
        var exprᴛ1 = (~e).Kind;
        if (exprᴛ1 == token.INT || exprᴛ1 == token.FLOAT || exprᴛ1 == token.IMAG) {
            check.langCompat(Ꮡe);
            // The max. mantissa precision for untyped numeric values
            // is 512 bits, or 4048 bits for each of the two integer
            // parts of a fraction for floating-point numbers that are
            // represented accurately in the go/constant package.
            // Constant literals that are longer than this many bits
            // are not meaningful; and excessively long constants may
            // consume a lot of space and time for a useless conversion.
            // Cap constant length with a generous upper limit that also
            // allows for separators between all digits.
            static readonly UntypedInt limit = 10000;
            if (len((~e).Value) > limit) {
                check.errorf(~e, InvalidConstVal, "excessively long constant: %s... (%d chars)"u8, (~e).Value[..10], len((~e).Value));
                goto ΔError;
            }
        }

        x.setConst((~e).Kind, (~e).Value);
        if (x.mode == invalid) {
            // The parser already establishes syntactic correctness.
            // If we reach here it's because of number under-/overflow.
            // TODO(gri) setConst (and in turn the go/constant package)
            // should return an error describing the issue.
            check.errorf(~e, InvalidConstVal, "malformed constant: %s"u8, (~e).Value);
            goto ΔError;
        }
        check.overflow(Ꮡx, // Ensure that integer values don't overflow (go.dev/issue/54280).
 e.Pos());
        break;
    }
    case ж<ast.FuncLit> e: {
        {
            var (sig, ok) = check.typ(~(~e).Type)._<ΔSignature.val>(ᐧ); if (ok){
                // Set the Scope's extent to the complete "func (...) {...}"
                // so that Scope.Innermost works correctly.
                (~sig).scope.val.pos = e.Pos();
                (~sig).scope.val.end = e.End();
                if (!check.conf.IgnoreFuncBodies && (~e).Body != nil) {
                    // Anonymous functions are considered part of the
                    // init expression/func declaration which contains
                    // them: use existing package-level declaration info.
                    var decl = check.decl;
                    // capture for use in closure below
                    var iota = check.iota;
                    // capture for use in closure below (go.dev/issue/22345)
                    // Don't type-check right away because the function may
                    // be part of a type definition to which the function
                    // body refers. Instead, type-check as soon as possible,
                    // but before the enclosing scope contents changes (go.dev/issue/22992).
                    check.later(
                    var declʗ11 = decl;
                    var iotaʗ11 = iota;
                    var sigʗ11 = sig;
                    () => {
                        check.funcBody(declʗ11, "<function literal>"u8, sigʗ11, (~e).Body, iotaʗ11);
                    }).describef(~e, "func literal"u8);
                }
                x.mode = value;
                x.typ = sig;
            } else {
                check.errorf(~e, InvalidSyntaxTree, "invalid function literal %v"u8, e);
                goto ΔError;
            }
        }
        break;
    }
    case ж<ast.CompositeLit> e: {
        ΔType typ = default!;
        ΔType baseΔ1 = default!;
        switch (ᐧ) {
        case {} when (~e).Type != default!: {
            {
                var (atyp, _) = (~e).Type._<ж<ast.ArrayType>>(ᐧ); if (atyp != nil && (~atyp).Len != default!) {
                    // composite literal type present - use it
                    // [...]T array types may only appear with composite literals.
                    // Check for them here so we don't have to handle ... in general.
                    {
                        var (ellip, _) = (~atyp).Len._<ж<ast.Ellipsis>>(ᐧ); if (ellip != nil && (~ellip).Elt == default!) {
                            // We have an "open" [...]T array type.
                            // Create a new ArrayType with unknown length (-1)
                            // and finish setting it up after analyzing the literal.
                            Ꮡtyp = new Array(len: -1, elem: check.varType((~atyp).Elt)); typ = ref Ꮡtyp.val;
                             = typ;
                            break;
                        }
                    }
                }
            }
            typ = check.typ((~e).Type);
             = typ;
            break;
        }
        case {} when hint != default!: {
            typ = hint;
            (, _) = deref(coreType(typ));
            if (baseΔ1 == default!) {
                // no composite literal type present - use hint (element type of enclosing type)
                // *T implies &T{}
                check.errorf(~e, InvalidLit, "invalid composite literal element type %s (no core type)"u8, typ);
                goto ΔError;
            }
            break;
        }
        default: {
            check.error(~e, // TODO(gri) provide better error messages depending on context
 UntypedLit, "missing type in composite literal"u8);
            goto ΔError;
            break;
        }}

        switch (coreType(@base).type()) {
        case Struct.val utyp: {
            if ((~utyp).fields == default!) {
                // Prevent crash if the struct referred to is not yet set up.
                // See analogous comment for *Array.
                check.error(~e, InvalidTypeCycle, "invalid recursive type"u8);
                goto ΔError;
            }
            if (len((~e).Elts) == 0) {
                break;
            }
            var fields = utyp.val.fields;
            {
                var (_, ok) = (~e).Elts[0]._<ж<ast.KeyValueExpr>>(ᐧ); if (ok){
                    // Convention for error messages on invalid struct literals:
                    // we mention the struct type only if it clarifies the error
                    // (e.g., a duplicate field error doesn't need the struct type).
                    // all elements must have keys
                    var visited = new slice<bool>(len(fields));
                    foreach (var (_, eΔ1) in (~e).Elts) {
                        var (kv, _) = eΔ1._<ж<ast.KeyValueExpr>>(ᐧ);
                        if (kv == nil) {
                            check.error(eΔ1, MixedStructLit, "mixture of field:value and value elements in struct literal"u8);
                            continue;
                        }
                        var (key, _) = (~kv).Key._<ж<ast.Ident>>(ᐧ);
                        // do all possible checks early (before exiting due to errors)
                        // so we don't drop information on the floor
                        check.expr(nil, Ꮡx, (~kv).Value);
                        if (key == nil) {
                            check.errorf(~kv, InvalidLitField, "invalid field name %s in struct literal"u8, (~kv).Key);
                            continue;
                        }
                        nint i = fieldIndex((~utyp).fields, check.pkg, (~key).Name, false);
                        if (i < 0) {
                            Object alt = default!;
                            {
                                nint j = fieldIndex(fields, check.pkg, (~key).Name, true); if (j >= 0) {
                                    alt = ~fields[j];
                                }
                            }
                            @string msg = check.lookupError(baseΔ1, (~key).Name, alt, true);
                            check.error((~kv).Key, MissingLitField, msg);
                            continue;
                        }
                        var fld = fields[i];
                        check.recordUse(key, ~fld);
                        var etyp = fld.typ;
                        check.assignment(Ꮡx, etyp, "struct literal"u8);
                        // 0 <= i < len(fields)
                        if (visited[i]) {
                            check.errorf(~kv, DuplicateLitField, "duplicate field name %s in struct literal"u8, (~key).Name);
                            continue;
                        }
                        visited[i] = true;
                    }
                } else {
                    // no element must have a key
                    foreach (var (i, eΔ2) in (~e).Elts) {
                        {
                            var (kv, _) = eΔ2._<ж<ast.KeyValueExpr>>(ᐧ); if (kv != nil) {
                                check.error(~kv, MixedStructLit, "mixture of field:value and value elements in struct literal"u8);
                                continue;
                            }
                        }
                        check.expr(nil, Ꮡx, eΔ2);
                        if (i >= len(fields)) {
                            check.errorf(~x, InvalidStructLit, "too many values in struct literal of type %s"u8, baseΔ1);
                            break;
                        }
                        // cannot continue
                        // i < len(fields)
                        var fld = fields[i];
                        if (!fld.Exported() && fld.pkg != check.pkg) {
                            check.errorf(~x,
                                UnexportedLitField,
                                "implicit assignment to unexported field %s in struct literal of type %s"u8, fld.name, baseΔ1);
                            continue;
                        }
                        var etyp = fld.typ;
                        check.assignment(Ꮡx, etyp, "struct literal"u8);
                    }
                    if (len((~e).Elts) < len(fields)) {
                        check.errorf(inNode(~e, (~e).Rbrace), InvalidStructLit, "too few values in struct literal of type %s"u8, baseΔ1);
                    }
                }
            }
            break;
        }
        case Array.val utyp: {
            if ((~utyp).elem == default!) {
                // ok to continue
                // Prevent crash if the array referred to is not yet set up. Was go.dev/issue/18643.
                // This is a stop-gap solution. Should use Checker.objPath to report entire
                // path starting with earliest declaration in the source. TODO(gri) fix this.
                check.error(~e, InvalidTypeCycle, "invalid recursive type"u8);
                goto ΔError;
            }
            var n = check.indexedElts((~e).Elts, (~utyp).elem, (~utyp).len);
            if ((~utyp).len < 0) {
                // If we have an array of unknown length (usually [...]T arrays, but also
                // arrays [n]T where n is invalid) set the length now that we know it and
                // record the type for the array (usually done by check.typ which is not
                // called for [...]T). We handle [...]T arrays and arrays with invalid
                // length the same here because it makes sense to "guess" the length for
                // the latter if we have a composite literal; e.g. for [n]int{1, 2, 3}
                // where n is invalid for some reason, it seems fair to assume it should
                // be 3 (see also Checked.arrayLength and go.dev/issue/27346).
                var utyp.val.len = n;
                // e.Type is missing if we have a composite literal element
                // that is itself a composite literal with omitted type. In
                // that case there is nothing to record (there is no type in
                // the source at that point).
                if ((~e).Type != default!) {
                    check.recordTypeAndValue((~e).Type, typexpr, ~utyp, default!);
                }
            }
            break;
        }
        case Slice.val utyp: {
            if ((~utyp).elem == default!) {
                // Prevent crash if the slice referred to is not yet set up.
                // See analogous comment for *Array.
                check.error(~e, InvalidTypeCycle, "invalid recursive type"u8);
                goto ΔError;
            }
            check.indexedElts((~e).Elts, (~utyp).elem, -1);
            break;
        }
        case Map.val utyp: {
            if ((~utyp).key == default! || (~utyp).elem == default!) {
                // Prevent crash if the map referred to is not yet set up.
                // See analogous comment for *Array.
                check.error(~e, InvalidTypeCycle, "invalid recursive type"u8);
                goto ΔError;
            }
            var keyIsInterface = isNonTypeParamInterface((~utyp).key);
            var visited = new map<any, slice<ΔType>>(len((~e).Elts));
            foreach (var (_, eΔ3) in (~e).Elts) {
                // If the map key type is an interface (but not a type parameter),
                // the type of a constant key must be considered when checking for
                // duplicates.
                var (kv, _) = eΔ3._<ж<ast.KeyValueExpr>>(ᐧ);
                if (kv == nil) {
                    check.error(eΔ3, MissingLitKey, "missing key in map literal"u8);
                    continue;
                }
                check.exprWithHint(Ꮡx, (~kv).Key, (~utyp).key);
                check.assignment(Ꮡx, (~utyp).key, "map literal"u8);
                if (x.mode == invalid) {
                    continue;
                }
                if (x.mode == constant_) {
                    var duplicate = false;
                    var xkey = keyVal(x.val);
                    if (keyIsInterface){
                        foreach (var (_, vtyp) in visited[xkey]) {
                            if (Identical(vtyp, x.typ)) {
                                duplicate = true;
                                break;
                            }
                        }
                        visited[xkey] = append(visited[xkey], x.typ);
                    } else {
                        var _ = visited[xkey];
                        duplicate = visited[xkey];
                        visited[xkey] = default!;
                    }
                    if (duplicate) {
                        check.errorf(~x, DuplicateLitKey, "duplicate key %s in map literal"u8, x.val);
                        continue;
                    }
                }
                check.exprWithHint(Ꮡx, (~kv).Value, (~utyp).elem);
                check.assignment(Ꮡx, (~utyp).elem, "map literal"u8);
            }
            break;
        }
        default: {
            var utyp = coreType(@base).type();
            foreach (var (_, eΔ4) in (~e).Elts) {
                // when "using" all elements unpack KeyValueExpr
                // explicitly because check.use doesn't accept them
                {
                    var (kv, _) = eΔ4._<ж<ast.KeyValueExpr>>(ᐧ); if (kv != nil) {
                        // Ideally, we should also "use" kv.Key but we can't know
                        // if it's an externally defined struct key or not. Going
                        // forward anyway can lead to other errors. Give up instead.
                        eΔ4 = kv.val.Value;
                    }
                }
                check.use(eΔ4);
            }
            if (isValid(utyp)) {
                // if utyp is invalid, an error was reported before
                check.errorf(~e, InvalidLit, "invalid composite literal type %s"u8, typ);
                goto ΔError;
            }
            break;
        }}
        x.mode = value;
        x.typ = typ;
        break;
    }
    case ж<ast.ParenExpr> e: {
        exprKind kind = check.rawExpr(nil, // type inference doesn't go past parentheses (targe type T = nil)
 Ꮡx, (~e).X, default!, false);
        x.expr = e;
        return kind;
    }
    case ж<ast.SelectorExpr> e: {
        check.selector(Ꮡx, Ꮡe, nil, false);
        break;
    }
    case ж<ast.IndexExpr> e: {
        var ix = typeparams.UnpackIndexExpr(e);
        if (check.indexExpr(Ꮡx, ix)) {
            if (!enableReverseTypeInference) {
                T = default!;
            }
            check.funcInst(ᏑT, e.Pos(), Ꮡx, ix, true);
        }
        if (x.mode == invalid) {
            goto ΔError;
        }
        break;
    }
    case ж<ast.IndexListExpr> e: {
        var ix = typeparams.UnpackIndexExpr(e);
        if (check.indexExpr(Ꮡx, ix)) {
            if (!enableReverseTypeInference) {
                T = default!;
            }
            check.funcInst(ᏑT, e.Pos(), Ꮡx, ix, true);
        }
        if (x.mode == invalid) {
            goto ΔError;
        }
        break;
    }
    case ж<ast.SliceExpr> e: {
        check.sliceExpr(Ꮡx, Ꮡe);
        if (x.mode == invalid) {
            goto ΔError;
        }
        break;
    }
    case ж<ast.TypeAssertExpr> e: {
        check.expr(nil, Ꮡx, (~e).X);
        if (x.mode == invalid) {
            goto ΔError;
        }
        if ((~e).Type == default!) {
            // x.(type) expressions are handled explicitly in type switches
            // Don't use InvalidSyntaxTree because this can occur in the AST produced by
            // go/parser.
            check.error(~e, BadTypeKeyword, "use of .(type) outside type switch"u8);
            goto ΔError;
        }
        if (isTypeParam(x.typ)) {
            check.errorf(~x, InvalidAssert, invalidOp + "cannot use type assertion on type parameter value %s", x);
            goto ΔError;
        }
        {
            var (_, ok) = under(x.typ)._<Interface.val>(ᐧ); if (!ok) {
                check.errorf(~x, InvalidAssert, invalidOp + "%s is not an interface", x);
                goto ΔError;
            }
        }
        var TΔ1 = check.varType((~e).Type);
        if (!isValid(TΔ1)) {
            goto ΔError;
        }
        check.typeAssertion(~e, Ꮡx, TΔ1, false);
        x.mode = commaok;
        x.typ = TΔ1;
        break;
    }
    case ж<ast.CallExpr> e: {
        return check.callExpr(Ꮡx, Ꮡe);
    }
    case ж<ast.StarExpr> e: {
        check.exprOrType(Ꮡx, (~e).X, false);
        var exprᴛ2 = x.mode;
        if (exprᴛ2 == invalid) {
            goto ΔError;
        }
        else if (exprᴛ2 == typexpr) {
            check.validVarType((~e).X, x.typ);
            x.typ = Ꮡ(new Pointer(@base: x.typ));
        }
        else { /* default: */
            ΔType baseΔ3 = default!;
            if (!underIs(x.typ, 
            var baseʗ1 = baseΔ3;
            (ΔType u) => {
                var (p, _) = u._<Pointer.val>(ᐧ);
                if (p == nil) {
                    check.errorf(~x, InvalidIndirection, invalidOp + "cannot indirect %s", x);
                    return false;
                }
                if (baseʗ1 != default! && !Identical((~p).baseʗ1, baseʗ1)) {
                    check.errorf(~x, InvalidIndirection, invalidOp + "pointers of %s must have identical base types", x);
                    return false;
                }
                baseʗ1 = p.val.baseʗ1;
                return true;
            })) {
                goto ΔError;
            }
            x.mode = variable;
            x.typ = baseΔ3;
        }

        break;
    }
    case ж<ast.UnaryExpr> e: {
        check.unary(Ꮡx, Ꮡe);
        if (x.mode == invalid) {
            goto ΔError;
        }
        if ((~e).Op == token.ARROW) {
            x.expr = e;
            return statement;
        }
        break;
    }
    case ж<ast.BinaryExpr> e: {
        check.binary(Ꮡx, // receive operations may appear in statement context
 ~e, (~e).X, (~e).Y, (~e).Op, (~e).OpPos);
        if (x.mode == invalid) {
            goto ΔError;
        }
        break;
    }
    case ж<ast.KeyValueExpr> e: {
        check.error(~e, // key:value expressions are handled in composite literals
 InvalidSyntaxTree, "no key:value expected"u8);
        goto ΔError;
        break;
    }
    case ж<ast.ArrayType> e: {
        x.mode = typexpr;
        x.typ = check.typ(e);
        break;
    }
    case ж<ast.StructType> e: {
        x.mode = typexpr;
        x.typ = check.typ(e);
        break;
    }
    case ж<ast.FuncType> e: {
        x.mode = typexpr;
        x.typ = check.typ(e);
        break;
    }
    case ж<ast.InterfaceType> e: {
        x.mode = typexpr;
        x.typ = check.typ(e);
        break;
    }
    case ж<ast.MapType> e: {
        x.mode = typexpr;
        x.typ = check.typ(e);
        break;
    }
    case ж<ast.ChanType> e: {
        x.mode = typexpr;
        x.typ = check.typ(e);
        break;
    }
    default: {
        var e = e.type();
        throw panic(fmt.Sprintf("%s: unknown expression type %T"u8, // Note: rawExpr (caller of exprInternal) will call check.recordTypeAndValue
 // even though check.typ has already called it. This is fine as both
 // times the same expression and type are recorded. It is also not a
 // performance issue because we only reach here for composite literal
 // types, which are comparatively rare.
 check.fset.Position(e.Pos()), e));
        break;
    }}
    // everything went well
    x.expr = e;
    return expression;
ΔError:
    x.mode = invalid;
    x.expr = e;
    return statement;
}

// avoid follow-up errors

// keyVal maps a complex, float, integer, string or boolean constant value
// to the corresponding complex128, float64, int64, uint64, string, or bool
// Go value if possible; otherwise it returns x.
// A complex constant that can be represented as a float (such as 1.2 + 0i)
// is returned as a floating point value; if a floating point value can be
// represented as an integer (such as 1.0) it is returned as an integer value.
// This ensures that constants of different kind but equal value (such as
// 1.0 + 0i, 1.0, 1) result in the same value.
internal static any keyVal(constant.Value x) {
    var exprᴛ1 = x.Kind();
    var matchᴛ1 = false;
    if (exprᴛ1 == constant.Complex) { matchᴛ1 = true;
        var f = constant.ToFloat(x);
        if (f.Kind() != constant.Float) {
            var (r, _) = constant.Float64Val(constant.Real(x));
            var (i, _) = constant.Float64Val(constant.Imag(x));
            return complex(r, i);
        }
        x = f;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == constant.Float)) { matchᴛ1 = true;
        var i = constant.ToInt(x);
        if (i.Kind() != constant.Int) {
            var (v, _) = constant.Float64Val(x);
            return v;
        }
        x = i;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == constant.Int)) { matchᴛ1 = true;
        {
            var (v, ok) = constant.Int64Val(x); if (ok) {
                return v;
            }
        }
        {
            var (v, ok) = constant.Uint64Val(x); if (ok) {
                return v;
            }
        }
    }
    if (exprᴛ1 == constant.ΔString) {
        return constant.StringVal(x);
    }
    if (exprᴛ1 == constant.Bool) { matchᴛ1 = true;
        return constant.BoolVal(x);
    }

    return x;
}

// typeAssertion checks x.(T). The type of x must be an interface.
[GoRecv] public static void typeAssertion(this ref Checker check, ast.Expr e, ж<operand> Ꮡx, ΔType T, bool typeSwitch) {
    ref var x = ref Ꮡx.val;

    ref var cause = ref heap(new @string(), out var Ꮡcause);
    if (check.assertableTo(x.typ, T, Ꮡcause)) {
        return;
    }
    // success
    if (typeSwitch) {
        check.errorf(e, ImpossibleAssert, "impossible type switch case: %s\n\t%s cannot have dynamic type %s %s"u8, e, x, T, cause);
        return;
    }
    check.errorf(e, ImpossibleAssert, "impossible type assertion: %s\n\t%s does not implement %s %s"u8, e, T, x.typ, cause);
}

// expr typechecks expression e and initializes x with the expression value.
// If a non-nil target T is given and e is a generic function or
// a function call, T is used to infer the type arguments for e.
// The result must be a single value.
// If an error occurred, x.mode is set to invalid.
[GoRecv] public static void expr(this ref Checker check, ж<target> ᏑT, ж<operand> Ꮡx, ast.Expr e) {
    ref var T = ref ᏑT.val;
    ref var x = ref Ꮡx.val;

    check.rawExpr(ᏑT, Ꮡx, e, default!, false);
    check.exclude(Ꮡx, (nuint)((UntypedInt)(1 << (int)(novalue) | 1 << (int)(Δbuiltin)) | 1 << (int)(typexpr)));
    check.singleValue(Ꮡx);
}

// genericExpr is like expr but the result may also be generic.
[GoRecv] public static void genericExpr(this ref Checker check, ж<operand> Ꮡx, ast.Expr e) {
    ref var x = ref Ꮡx.val;

    check.rawExpr(nil, Ꮡx, e, default!, true);
    check.exclude(Ꮡx, (nuint)((UntypedInt)(1 << (int)(novalue) | 1 << (int)(Δbuiltin)) | 1 << (int)(typexpr)));
    check.singleValue(Ꮡx);
}

// multiExpr typechecks e and returns its value (or values) in list.
// If allowCommaOk is set and e is a map index, comma-ok, or comma-err
// expression, the result is a two-element list containing the value
// of e, and an untyped bool value or an error value, respectively.
// If an error occurred, list[0] is not valid.
[GoRecv] internal static (slice<ж<operand>> list, bool commaOk) multiExpr(this ref Checker check, ast.Expr e, bool allowCommaOk) {
    slice<ж<operand>> list = default!;
    bool commaOk = default!;

    ref var x = ref heap(new operand(), out var Ꮡx);
    check.rawExpr(nil, Ꮡx, e, default!, false);
    check.exclude(Ꮡx, (nuint)((UntypedInt)(1 << (int)(novalue) | 1 << (int)(Δbuiltin)) | 1 << (int)(typexpr)));
    {
        var (t, ok) = x.typ._<Tuple.val>(ᐧ); if (ok && x.mode != invalid) {
            // multiple values
            list = new slice<ж<operand>>(t.Len());
            foreach (var (i, v) in (~t).vars) {
                list[i] = Ꮡ(new operand(mode: value, expr: e, typ: v.typ));
            }
            return (list, commaOk);
        }
    }
    // exactly one (possibly invalid or comma-ok) value
    list = new ж<operand>[]{Ꮡx}.slice();
    if (allowCommaOk && (x.mode == mapindex || x.mode == commaok || x.mode == commaerr)) {
        var x2 = Ꮡ(new operand(mode: value, expr: e, typ: Typ[UntypedBool]));
        if (x.mode == commaerr) {
            x2.val.typ = universeError;
        }
        list = append(list, x2);
        commaOk = true;
    }
    return (list, commaOk);
}

// exprWithHint typechecks expression e and initializes x with the expression value;
// hint is the type of a composite literal element.
// If an error occurred, x.mode is set to invalid.
[GoRecv] public static void exprWithHint(this ref Checker check, ж<operand> Ꮡx, ast.Expr e, ΔType hint) {
    ref var x = ref Ꮡx.val;

    assert(hint != default!);
    check.rawExpr(nil, Ꮡx, e, hint, false);
    check.exclude(Ꮡx, (nuint)((UntypedInt)(1 << (int)(novalue) | 1 << (int)(Δbuiltin)) | 1 << (int)(typexpr)));
    check.singleValue(Ꮡx);
}

// exprOrType typechecks expression or type e and initializes x with the expression value or type.
// If allowGeneric is set, the operand type may be an uninstantiated parameterized type or function
// value.
// If an error occurred, x.mode is set to invalid.
[GoRecv] public static void exprOrType(this ref Checker check, ж<operand> Ꮡx, ast.Expr e, bool allowGeneric) {
    ref var x = ref Ꮡx.val;

    check.rawExpr(nil, Ꮡx, e, default!, allowGeneric);
    check.exclude(Ꮡx, 1 << (int)(novalue));
    check.singleValue(Ꮡx);
}

// exclude reports an error if x.mode is in modeset and sets x.mode to invalid.
// The modeset may contain any of 1<<novalue, 1<<builtin, 1<<typexpr.
[GoRecv] public static void exclude(this ref Checker check, ж<operand> Ꮡx, nuint modeset) {
    ref var x = ref Ꮡx.val;

    if ((nuint)(modeset & (1 << (int)(x.mode))) != 0) {
        @string msg = default!;
        errors.Code code = default!;
        var exprᴛ1 = x.mode;
        if (exprᴛ1 == novalue) {
            if ((nuint)(modeset & (1 << (int)(typexpr))) != 0){
                msg = "%s used as value"u8;
            } else {
                msg = "%s used as value or type"u8;
            }
            code = TooManyValues;
        }
        else if (exprᴛ1 == Δbuiltin) {
            msg = "%s must be called"u8;
            code = UncalledBuiltin;
        }
        else if (exprᴛ1 == typexpr) {
            msg = "%s is not an expression"u8;
            code = NotAnExpr;
        }
        else { /* default: */
            throw panic("unreachable");
        }

        check.errorf(~x, code, msg, x);
        x.mode = invalid;
    }
}

// singleValue reports an error if x describes a tuple and sets x.mode to invalid.
[GoRecv] public static void singleValue(this ref Checker check, ж<operand> Ꮡx) {
    ref var x = ref Ꮡx.val;

    if (x.mode == value) {
        // tuple types are never named - no need for underlying type below
        {
            var (t, ok) = x.typ._<Tuple.val>(ᐧ); if (ok) {
                assert(t.Len() != 1);
                check.errorf(~x, TooManyValues, "multiple-value %s in single-value context"u8, x);
                x.mode = invalid;
            }
        }
    }
}

} // end types_package
