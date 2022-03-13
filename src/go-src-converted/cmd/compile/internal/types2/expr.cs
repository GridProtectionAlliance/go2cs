// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements typechecking of expressions.

// package types2 -- go2cs converted at 2022 March 13 06:25:59 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\expr.go
namespace go.cmd.compile.@internal;

using syntax = cmd.compile.@internal.syntax_package;
using fmt = fmt_package;
using constant = go.constant_package;
using token = go.token_package;
using math = math_package;


/*
Basic algorithm:

Expressions are checked recursively, top down. Expression checker functions
are generally of the form:

  func f(x *operand, e *syntax.Expr, ...)

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


using System;public static partial class types2_package {

private partial struct opPredicates { // : map<syntax.Operator, Func<Type, bool>>
}

private static opPredicates unaryOpPredicates = default;

private static void init() { 
    // Setting unaryOpPredicates in init avoids declaration cycles.
    unaryOpPredicates = new opPredicates(syntax.Add:isNumeric,syntax.Sub:isNumeric,syntax.Xor:isInteger,syntax.Not:isBoolean,);
}

private static bool op(this ptr<Checker> _addr_check, opPredicates m, ptr<operand> _addr_x, syntax.Operator op) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;

    {
        var pred = m[op];

        if (pred != null) {
            if (!pred(x.typ)) {
                if (check.conf.CompilerErrorMessages) {
                    check.errorf(x, invalidOp + "operator %s not defined on %s", op, x);
                }
                else
 {
                    check.errorf(x, invalidOp + "operator %s not defined for %s", op, x);
                }
                return false;
            }
        }
        else
 {
            check.errorf(x, invalidAST + "unknown operator %s", op);
            return false;
        }
    }
    return true;
}

// overflow checks that the constant x is representable by its type.
// For untyped constants, it checks that the value doesn't become
// arbitrarily large.
private static void overflow(this ptr<Checker> _addr_check, ptr<operand> _addr_x) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;

    assert(x.mode == constant_); 

    // If the corresponding expression is an operation, use the
    // operator position rather than the start of the expression
    // as error position.
    var pos = syntax.StartPos(x.expr);
    @string what = ""; // operator description, if any
    {
        ptr<syntax.Operation> (op, _) = x.expr._<ptr<syntax.Operation>>();

        if (op != null) {
            pos = op.Pos();
            what = opName(op);
        }
    }

    if (x.val.Kind() == constant.Unknown) { 
        // TODO(gri) We should report exactly what went wrong. At the
        //           moment we don't have the (go/constant) API for that.
        //           See also TODO in go/constant/value.go.
        check.error(pos, "constant result is not representable");
        return ;
    }
    if (isTyped(x.typ)) {
        check.representable(x, asBasic(x.typ));
        return ;
    }
    const nint prec = 512; // 512 is the constant precision
 // 512 is the constant precision
    if (x.val.Kind() == constant.Int && constant.BitLen(x.val) > prec) {
        check.errorf(pos, "constant %s overflow", what);
        x.val = constant.MakeUnknown();
    }
}

// opName returns the name of an operation, or the empty string.
// For now, only operations that might overflow are handled.
// TODO(gri) Expand this to a general mechanism giving names to
//           nodes?
private static @string opName(ptr<syntax.Operation> _addr_e) {
    ref syntax.Operation e = ref _addr_e.val;

    var op = int(e.Op);
    if (e.Y == null) {
        if (op < len(op2str1)) {
            return op2str1[op];
        }
    }
    else
 {
        if (op < len(op2str2)) {
            return op2str2[op];
        }
    }
    return "";
}

private static array<@string> op2str1 = new array<@string>(InitKeyedValues<@string>((syntax.Xor, "bitwise complement")));

// This is only used for operations that may cause overflow.
private static array<@string> op2str2 = new array<@string>(InitKeyedValues<@string>((syntax.Add, "addition"), (syntax.Sub, "subtraction"), (syntax.Xor, "bitwise XOR"), (syntax.Mul, "multiplication"), (syntax.Shl, "shift")));

private static void unary(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<syntax.Operation> _addr_e) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;
    ref syntax.Operation e = ref _addr_e.val;

    check.expr(x, e.X);
    if (x.mode == invalid) {
        return ;
    }

    if (e.Op == syntax.And) 
        // spec: "As an exception to the addressability
        // requirement x may also be a composite literal."
        {
            ptr<syntax.CompositeLit> (_, ok) = unparen(e.X)._<ptr<syntax.CompositeLit>>();

            if (!ok && x.mode != variable) {
                check.errorf(x, invalidOp + "cannot take address of %s", x);
                x.mode = invalid;
                return ;
            }

        }
        x.mode = value;
        x.typ = addr(new Pointer(base:x.typ));
        return ;
    else if (e.Op == syntax.Recv) 
        var typ = asChan(x.typ);
        if (typ == null) {
            check.errorf(x, invalidOp + "cannot receive from non-channel %s", x);
            x.mode = invalid;
            return ;
        }
        if (typ.dir == SendOnly) {
            check.errorf(x, invalidOp + "cannot receive from send-only channel %s", x);
            x.mode = invalid;
            return ;
        }
        x.mode = commaok;
        x.typ = typ.elem;
        check.hasCallOrRecv = true;
        return ;
        if (!check.op(unaryOpPredicates, x, e.Op)) {
        x.mode = invalid;
        return ;
    }
    if (x.mode == constant_) {
        if (x.val.Kind() == constant.Unknown) { 
            // nothing to do (and don't cause an error below in the overflow check)
            return ;
        }
        nuint prec = default;
        if (isUnsigned(x.typ)) {
            prec = uint(check.conf.@sizeof(x.typ) * 8);
        }
        x.val = constant.UnaryOp(op2tok[e.Op], x.val, prec);
        x.expr = e;
        check.overflow(x);
        return ;
    }
    x.mode = value; 
    // x.typ remains unchanged
}

private static bool isShift(syntax.Operator op) {
    return op == syntax.Shl || op == syntax.Shr;
}

private static bool isComparison(syntax.Operator op) { 
    // Note: tokens are not ordered well to make this much easier

    if (op == syntax.Eql || op == syntax.Neq || op == syntax.Lss || op == syntax.Leq || op == syntax.Gtr || op == syntax.Geq) 
        return true;
        return false;
}

private static bool fitsFloat32(constant.Value x) {
    var (f32, _) = constant.Float32Val(x);
    var f = float64(f32);
    return !math.IsInf(f, 0);
}

private static constant.Value roundFloat32(constant.Value x) {
    var (f32, _) = constant.Float32Val(x);
    var f = float64(f32);
    if (!math.IsInf(f, 0)) {
        return constant.MakeFloat64(f);
    }
    return null;
}

private static bool fitsFloat64(constant.Value x) {
    var (f, _) = constant.Float64Val(x);
    return !math.IsInf(f, 0);
}

private static constant.Value roundFloat64(constant.Value x) {
    var (f, _) = constant.Float64Val(x);
    if (!math.IsInf(f, 0)) {
        return constant.MakeFloat64(f);
    }
    return null;
}

// representableConst reports whether x can be represented as
// value of the given basic type and for the configuration
// provided (only needed for int/uint sizes).
//
// If rounded != nil, *rounded is set to the rounded value of x for
// representable floating-point and complex values, and to an Int
// value for integer values; it is left alone otherwise.
// It is ok to provide the addressof the first argument for rounded.
//
// The check parameter may be nil if representableConst is invoked
// (indirectly) through an exported API call (AssignableTo, ConvertibleTo)
// because we don't need the Checker's config for those calls.
private static bool representableConst(constant.Value x, ptr<Checker> _addr_check, ptr<Basic> _addr_typ, ptr<constant.Value> _addr_rounded) {
    ref Checker check = ref _addr_check.val;
    ref Basic typ = ref _addr_typ.val;
    ref constant.Value rounded = ref _addr_rounded.val;

    if (x.Kind() == constant.Unknown) {
        return true; // avoid follow-up errors
    }
    ptr<Config> conf;
    if (check != null) {
        conf = check.conf;
    }

    if (isInteger(typ)) 
        var x = constant.ToInt(x);
        if (x.Kind() != constant.Int) {
            return false;
        }
        if (rounded != null) {
            rounded = x;
        }
        {
            var x__prev1 = x;

            var (x, ok) = constant.Int64Val(x);

            if (ok) {

                if (typ.kind == Int) 
                    var s = uint(conf.@sizeof(typ)) * 8;
                    return int64(-1) << (int)((s - 1)) <= x && x <= int64(1) << (int)((s - 1)) - 1;
                else if (typ.kind == Int8) 
                    const nint s = 8;

                    return -1 << (int)((s - 1)) <= x && x <= 1 << (int)((s - 1)) - 1;
                else if (typ.kind == Int16) 
                    const nint s = 16;

                    return -1 << (int)((s - 1)) <= x && x <= 1 << (int)((s - 1)) - 1;
                else if (typ.kind == Int32) 
                    const nint s = 32;

                    return -1 << (int)((s - 1)) <= x && x <= 1 << (int)((s - 1)) - 1;
                else if (typ.kind == Int64 || typ.kind == UntypedInt) 
                    return true;
                else if (typ.kind == Uint || typ.kind == Uintptr) 
                    {
                        var s__prev2 = s;

                        s = uint(conf.@sizeof(typ)) * 8;

                        if (s < 64) {
                            return 0 <= x && x <= int64(1) << (int)(s) - 1;
                        }

                        s = s__prev2;

                    }
                    return 0 <= x;
                else if (typ.kind == Uint8) 
                    const nint s = 8;

                    return 0 <= x && x <= 1 << (int)(s) - 1;
                else if (typ.kind == Uint16) 
                    const nint s = 16;

                    return 0 <= x && x <= 1 << (int)(s) - 1;
                else if (typ.kind == Uint32) 
                    const nint s = 32;

                    return 0 <= x && x <= 1 << (int)(s) - 1;
                else if (typ.kind == Uint64) 
                    return 0 <= x;
                else 
                    unreachable();
                            } 
            // x does not fit into int64

            x = x__prev1;

        } 
        // x does not fit into int64
        {
            var n = constant.BitLen(x);


            if (typ.kind == Uint || typ.kind == Uintptr) 
                s = uint(conf.@sizeof(typ)) * 8;
                return constant.Sign(x) >= 0 && n <= int(s);
            else if (typ.kind == Uint64) 
                return constant.Sign(x) >= 0 && n <= 64;
            else if (typ.kind == UntypedInt) 
                return true;

        }
    else if (isFloat(typ)) 
        x = constant.ToFloat(x);
        if (x.Kind() != constant.Float) {
            return false;
        }

        if (typ.kind == Float32) 
            if (rounded == null) {
                return fitsFloat32(x);
            }
            var r = roundFloat32(x);
            if (r != null) {
                rounded = r;
                return true;
            }
        else if (typ.kind == Float64) 
            if (rounded == null) {
                return fitsFloat64(x);
            }
            r = roundFloat64(x);
            if (r != null) {
                rounded = r;
                return true;
            }
        else if (typ.kind == UntypedFloat) 
            return true;
        else 
            unreachable();
            else if (isComplex(typ)) 
        x = constant.ToComplex(x);
        if (x.Kind() != constant.Complex) {
            return false;
        }

        if (typ.kind == Complex64) 
            if (rounded == null) {
                return fitsFloat32(constant.Real(x)) && fitsFloat32(constant.Imag(x));
            }
            var re = roundFloat32(constant.Real(x));
            var im = roundFloat32(constant.Imag(x));
            if (re != null && im != null) {
                rounded = constant.BinaryOp(re, token.ADD, constant.MakeImag(im));
                return true;
            }
        else if (typ.kind == Complex128) 
            if (rounded == null) {
                return fitsFloat64(constant.Real(x)) && fitsFloat64(constant.Imag(x));
            }
            re = roundFloat64(constant.Real(x));
            im = roundFloat64(constant.Imag(x));
            if (re != null && im != null) {
                rounded = constant.BinaryOp(re, token.ADD, constant.MakeImag(im));
                return true;
            }
        else if (typ.kind == UntypedComplex) 
            return true;
        else 
            unreachable();
            else if (isString(typ)) 
        return x.Kind() == constant.String;
    else if (isBoolean(typ)) 
        return x.Kind() == constant.Bool;
        return false;
}

// An errorCode is a (constant) value uniquely identifing a specific error.
private partial struct errorCode { // : nint
}

// The following error codes are "borrowed" from go/types which codes for
// all errors. Here we list the few codes currently needed by the various
// conversion checking functions.
// Eventually we will switch to reporting codes for all errors, using a
// an error code table shared between types2 and go/types.
private static readonly var _ = errorCode(iota);
private static readonly var _TruncatedFloat = 0;
private static readonly var _NumericOverflow = 1;
private static readonly var _InvalidConstVal = 2;
private static readonly var _InvalidUntypedConversion = 3; 

// The following error codes are only returned by operand.assignableTo
// and none of its callers use the error. Still, we keep returning the
// error codes to make the transition to reporting error codes all the
// time easier in the future.
private static readonly var _IncompatibleAssign = 4;
private static readonly var _InvalidIfaceAssign = 5;
private static readonly var _InvalidChanAssign = 6;

// representable checks that a constant operand is representable in the given
// basic type.
private static void representable(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<Basic> _addr_typ) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;
    ref Basic typ = ref _addr_typ.val;

    var (v, code) = check.representation(x, typ);
    if (code != 0) {
        check.invalidConversion(code, x, typ);
        x.mode = invalid;
        return ;
    }
    assert(v != null);
    x.val = v;
}

// representation returns the representation of the constant operand x as the
// basic type typ.
//
// If no such representation is possible, it returns a non-zero error code.
private static (constant.Value, errorCode) representation(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<Basic> _addr_typ) {
    constant.Value _p0 = default;
    errorCode _p0 = default;
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;
    ref Basic typ = ref _addr_typ.val;

    assert(x.mode == constant_);
    ref var v = ref heap(x.val, out ptr<var> _addr_v);
    if (!representableConst(x.val, _addr_check, _addr_typ, _addr_v)) {
        if (isNumeric(x.typ) && isNumeric(typ)) { 
            // numeric conversion : error msg
            //
            // integer -> integer : overflows
            // integer -> float   : overflows (actually not possible)
            // float   -> integer : truncated
            // float   -> float   : overflows
            //
            if (!isInteger(x.typ) && isInteger(typ)) {
                return (null, _TruncatedFloat);
            }
            else
 {
                return (null, _NumericOverflow);
            }
        }
        return (null, _InvalidConstVal);
    }
    return (v, 0);
}

private static void invalidConversion(this ptr<Checker> _addr_check, errorCode code, ptr<operand> _addr_x, Type target) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;

    @string msg = "cannot convert %s to %s";

    if (code == _TruncatedFloat) 
        msg = "%s truncated to %s";
    else if (code == _NumericOverflow) 
        msg = "%s overflows %s";
        check.errorf(x, msg, x, target);
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
//
private static void updateExprType(this ptr<Checker> _addr_check, syntax.Expr x, Type typ, bool final) {
    ref Checker check = ref _addr_check.val;

    var (old, found) = check.untyped[x];
    if (!found) {
        return ; // nothing to do
    }
    switch (x.type()) {
        case ptr<syntax.BadExpr> x:
            if (debug) {
                check.dump("%v: found old type(%s): %s (new: %s)", posFor(x), x, old.typ, typ);
                unreachable();
            }
            return ;
            break;
        case ptr<syntax.FuncLit> x:
            if (debug) {
                check.dump("%v: found old type(%s): %s (new: %s)", posFor(x), x, old.typ, typ);
                unreachable();
            }
            return ;
            break;
        case ptr<syntax.CompositeLit> x:
            if (debug) {
                check.dump("%v: found old type(%s): %s (new: %s)", posFor(x), x, old.typ, typ);
                unreachable();
            }
            return ;
            break;
        case ptr<syntax.IndexExpr> x:
            if (debug) {
                check.dump("%v: found old type(%s): %s (new: %s)", posFor(x), x, old.typ, typ);
                unreachable();
            }
            return ;
            break;
        case ptr<syntax.SliceExpr> x:
            if (debug) {
                check.dump("%v: found old type(%s): %s (new: %s)", posFor(x), x, old.typ, typ);
                unreachable();
            }
            return ;
            break;
        case ptr<syntax.AssertExpr> x:
            if (debug) {
                check.dump("%v: found old type(%s): %s (new: %s)", posFor(x), x, old.typ, typ);
                unreachable();
            }
            return ;
            break;
        case ptr<syntax.ListExpr> x:
            if (debug) {
                check.dump("%v: found old type(%s): %s (new: %s)", posFor(x), x, old.typ, typ);
                unreachable();
            }
            return ;
            break;
        case ptr<syntax.KeyValueExpr> x:
            if (debug) {
                check.dump("%v: found old type(%s): %s (new: %s)", posFor(x), x, old.typ, typ);
                unreachable();
            }
            return ;
            break;
        case ptr<syntax.ArrayType> x:
            if (debug) {
                check.dump("%v: found old type(%s): %s (new: %s)", posFor(x), x, old.typ, typ);
                unreachable();
            }
            return ;
            break;
        case ptr<syntax.StructType> x:
            if (debug) {
                check.dump("%v: found old type(%s): %s (new: %s)", posFor(x), x, old.typ, typ);
                unreachable();
            }
            return ;
            break;
        case ptr<syntax.FuncType> x:
            if (debug) {
                check.dump("%v: found old type(%s): %s (new: %s)", posFor(x), x, old.typ, typ);
                unreachable();
            }
            return ;
            break;
        case ptr<syntax.InterfaceType> x:
            if (debug) {
                check.dump("%v: found old type(%s): %s (new: %s)", posFor(x), x, old.typ, typ);
                unreachable();
            }
            return ;
            break;
        case ptr<syntax.MapType> x:
            if (debug) {
                check.dump("%v: found old type(%s): %s (new: %s)", posFor(x), x, old.typ, typ);
                unreachable();
            }
            return ;
            break;
        case ptr<syntax.ChanType> x:
            if (debug) {
                check.dump("%v: found old type(%s): %s (new: %s)", posFor(x), x, old.typ, typ);
                unreachable();
            }
            return ;
            break;
        case ptr<syntax.CallExpr> x:
            break;
        case ptr<syntax.Name> x:
            break;
        case ptr<syntax.BasicLit> x:
            break;
        case ptr<syntax.SelectorExpr> x:
            break;
        case ptr<syntax.ParenExpr> x:
            check.updateExprType(x.X, typ, final); 

            // case *syntax.UnaryExpr:
            //     // If x is a constant, the operands were constants.
            //     // The operands don't need to be updated since they
            //     // never get "materialized" into a typed value. If
            //     // left in the untyped map, they will be processed
            //     // at the end of the type check.
            //     if old.val != nil {
            //         break
            //     }
            //     check.updateExprType(x.X, typ, final)
            break;
        case ptr<syntax.Operation> x:
            if (x.Y == null) { 
                // unary expression
                if (x.Op == syntax.Mul) { 
                    // see commented out code for StarExpr above
                    // TODO(gri) needs cleanup
                    if (debug) {
                        unimplemented();
                    }
                    return ;
                } 
                // If x is a constant, the operands were constants.
                // The operands don't need to be updated since they
                // never get "materialized" into a typed value. If
                // left in the untyped map, they will be processed
                // at the end of the type check.
                if (old.val != null) {
                    break;
                }
                check.updateExprType(x.X, typ, final);
                break;
            } 

            // binary expression
            if (old.val != null) {
                break; // see comment for unary expressions
            }
            if (isComparison(x.Op)) { 
                // The result type is independent of operand types
                // and the operand types must have final types.
            }
            else if (isShift(x.Op)) { 
                // The result type depends only on lhs operand.
                // The rhs type was updated when checking the shift.
                check.updateExprType(x.X, typ, final);
            }
            else
 { 
                // The operand types match the result type.
                check.updateExprType(x.X, typ, final);
                check.updateExprType(x.Y, typ, final);
            }
            break;
        default:
        {
            var x = x.type();
            unreachable();
            break;
        } 

        // If the new type is not final and still untyped, just
        // update the recorded type.
    } 

    // If the new type is not final and still untyped, just
    // update the recorded type.
    if (!final && isUntyped(typ)) {
        old.typ = asBasic(typ);
        check.untyped[x] = old;
        return ;
    }
    delete(check.untyped, x);

    if (old.isLhs) { 
        // If x is the lhs of a shift, its final type must be integer.
        // We already know from the shift check that it is representable
        // as an integer if it is a constant.
        if (!isInteger(typ)) {
            check.errorf(x, invalidOp + "shifted operand %s (type %s) must be integer", x, typ);
            return ;
        }
    }
    if (old.val != null) { 
        // If x is a constant, it must be representable as a value of typ.
        ref operand c = ref heap(new operand(old.mode,x,old.typ,old.val,0), out ptr<operand> _addr_c);
        check.convertUntyped(_addr_c, typ);
        if (c.mode == invalid) {
            return ;
        }
    }
    check.recordTypeAndValue(x, old.mode, typ, old.val);
}

// updateExprVal updates the value of x to val.
private static void updateExprVal(this ptr<Checker> _addr_check, syntax.Expr x, constant.Value val) {
    ref Checker check = ref _addr_check.val;

    {
        var (info, ok) = check.untyped[x];

        if (ok) {
            info.val = val;
            check.untyped[x] = info;
        }
    }
}

// convertUntyped attempts to set the type of an untyped value to the target type.
private static void convertUntyped(this ptr<Checker> _addr_check, ptr<operand> _addr_x, Type target) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;

    var (newType, val, code) = check.implicitTypeAndValue(x, target);
    if (code != 0) {
        check.invalidConversion(code, x, target.Underlying());
        x.mode = invalid;
        return ;
    }
    if (val != null) {
        x.val = val;
        check.updateExprVal(x.expr, val);
    }
    if (newType != x.typ) {
        x.typ = newType;
        check.updateExprType(x.expr, newType, false);
    }
}

// implicitTypeAndValue returns the implicit type of x when used in a context
// where the target type is expected. If no such implicit conversion is
// possible, it returns a nil Type and non-zero error code.
//
// If x is a constant operand, the returned constant.Value will be the
// representation of x in this context.
private static (Type, constant.Value, errorCode) implicitTypeAndValue(this ptr<Checker> _addr_check, ptr<operand> _addr_x, Type target) {
    Type _p0 = default;
    constant.Value _p0 = default;
    errorCode _p0 = default;
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;

    target = expand(target);
    if (x.mode == invalid || isTyped(x.typ) || target == Typ[Invalid]) {
        return (x.typ, null, 0);
    }
    if (isUntyped(target)) { 
        // both x and target are untyped
        ptr<Basic> xkind = x.typ._<ptr<Basic>>().kind;
        ptr<Basic> tkind = target._<ptr<Basic>>().kind;
        if (isNumeric(x.typ) && isNumeric(target)) {
            if (xkind < tkind) {
                return (target, null, 0);
            }
        }
        else if (xkind != tkind) {
            return (null, null, _InvalidUntypedConversion);
        }
        return (x.typ, null, 0);
    }
    if (x.isNil()) {
        assert(isUntyped(x.typ));
        if (hasNil(target)) {
            return (target, null, 0);
        }
        return (null, null, _InvalidUntypedConversion);
    }
    switch (optype(target).type()) {
        case ptr<Basic> t:
            if (x.mode == constant_) {
                var (v, code) = check.representation(x, t);
                if (code != 0) {
                    return (null, null, code);
                }
                return (target, v, code);
            } 
            // Non-constant untyped values may appear as the
            // result of comparisons (untyped bool), intermediate
            // (delayed-checked) rhs operands of shifts, and as
            // the value nil.

            if (x.typ._<ptr<Basic>>().kind == UntypedBool) 
                if (!isBoolean(target)) {
                    return (null, null, _InvalidUntypedConversion);
                }
            else if (x.typ._<ptr<Basic>>().kind == UntypedInt || x.typ._<ptr<Basic>>().kind == UntypedRune || x.typ._<ptr<Basic>>().kind == UntypedFloat || x.typ._<ptr<Basic>>().kind == UntypedComplex) 
                if (!isNumeric(target)) {
                    return (null, null, _InvalidUntypedConversion);
                }
            else if (x.typ._<ptr<Basic>>().kind == UntypedString) 
                // Non-constant untyped string values are not permitted by the spec and
                // should not occur during normal typechecking passes, but this path is
                // reachable via the AssignableTo API.
                if (!isString(target)) {
                    return (null, null, _InvalidUntypedConversion);
                }
            else 
                return (null, null, _InvalidUntypedConversion);
                        break;
        case ptr<Sum> t:
            var ok = t.@is(t => {
                var (target, _, _) = check.implicitTypeAndValue(x, t);
                return target != null;
            });
            if (!ok) {
                return (null, null, _InvalidUntypedConversion);
            }
            break;
        case ptr<Interface> t:
            check.completeInterface(nopos, t);
            if (!t.Empty()) {
                return (null, null, _InvalidUntypedConversion); // cannot assign untyped values to non-empty interfaces
            }
            return (Default(x.typ), null, 0); // default type for nil is nil
            break;
        default:
        {
            var t = optype(target).type();
            return (null, null, _InvalidUntypedConversion);
            break;
        }
    }
    return (target, null, 0);
}

private static void comparison(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<operand> _addr_y, syntax.Operator op) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;
    ref operand y = ref _addr_y.val;
 
    // spec: "In any comparison, the first operand must be assignable
    // to the type of the second operand, or vice versa."
    @string err = "";
    var (xok, _) = x.assignableTo(check, y.typ, null);
    var (yok, _) = y.assignableTo(check, x.typ, null);
    if (xok || yok) {
        var defined = false;

        if (op == syntax.Eql || op == syntax.Neq) 
            // spec: "The equality operators == and != apply to operands that are comparable."
            defined = Comparable(x.typ) && Comparable(y.typ) || x.isNil() && hasNil(y.typ) || y.isNil() && hasNil(x.typ);
        else if (op == syntax.Lss || op == syntax.Leq || op == syntax.Gtr || op == syntax.Geq) 
            // spec: The ordering operators <, <=, >, and >= apply to operands that are ordered."
            defined = isOrdered(x.typ) && isOrdered(y.typ);
        else 
            unreachable();
                if (!defined) {
            var typ = x.typ;
            if (x.isNil()) {
                typ = y.typ;
            }
            if (check.conf.CompilerErrorMessages) {
                err = check.sprintf("operator %s not defined on %s", op, typ);
            }
            else
 {
                err = check.sprintf("operator %s not defined for %s", op, typ);
            }
        }
    }
    else
 {
        err = check.sprintf("mismatched types %s and %s", x.typ, y.typ);
    }
    if (err != "") { 
        // TODO(gri) better error message for cases where one can only compare against nil
        check.errorf(x, invalidOp + "cannot compare %s %s %s (%s)", x.expr, op, y.expr, err);
        x.mode = invalid;
        return ;
    }
    if (x.mode == constant_ && y.mode == constant_) {
        x.val = constant.MakeBool(constant.Compare(x.val, op2tok[op], y.val)); 
        // The operands are never materialized; no need to update
        // their types.
    }
    else
 {
        x.mode = value; 
        // The operands have now their final types, which at run-
        // time will be materialized. Update the expression trees.
        // If the current types are untyped, the materialized type
        // is the respective default type.
        check.updateExprType(x.expr, Default(x.typ), true);
        check.updateExprType(y.expr, Default(y.typ), true);
    }
    x.typ = Typ[UntypedBool];
}

// If e != nil, it must be the shift expression; it may be nil for non-constant shifts.
private static void shift(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<operand> _addr_y, syntax.Expr e, syntax.Operator op) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;
    ref operand y = ref _addr_y.val;
 
    // TODO(gri) This function seems overly complex. Revisit.

    constant.Value xval = default;
    if (x.mode == constant_) {
        xval = constant.ToInt(x.val);
    }
    if (isInteger(x.typ) || isUntyped(x.typ) && xval != null && xval.Kind() == constant.Int) { 
        // The lhs is of integer type or an untyped constant representable
        // as an integer. Nothing to do.
    }
    else
 { 
        // shift has no chance
        check.errorf(x, invalidOp + "shifted operand %s must be integer", x);
        x.mode = invalid;
        return ;
    }
    if (y.mode == constant_) {
        var yval = constant.ToInt(y.val); // consider -1, 1.0, but not -1.1
        if (yval.Kind() == constant.Int && constant.Sign(yval) < 0) {
            check.errorf(y, invalidOp + "negative shift count %s", y);
            x.mode = invalid;
            return ;
        }
    }
    if (isUntyped(y.typ)) {
        check.convertUntyped(y, Typ[Uint]);
        if (y.mode == invalid) {
            x.mode = invalid;
            return ;
        }
    }
    else if (!isInteger(y.typ)) {
        check.errorf(y, invalidOp + "shift count %s must be integer", y);
        x.mode = invalid;
        return ;
    }
    else if (!isUnsigned(y.typ) && !check.allowVersion(check.pkg, 1, 13)) {
        check.errorf(y, invalidOp + "signed shift count %s requires go1.13 or later", y);
        x.mode = invalid;
        return ;
    }
    if (x.mode == constant_) {
        if (y.mode == constant_) { 
            // if either x or y has an unknown value, the result is unknown
            if (x.val.Kind() == constant.Unknown || y.val.Kind() == constant.Unknown) {
                x.val = constant.MakeUnknown(); 
                // ensure the correct type - see comment below
                if (!isInteger(x.typ)) {
                    x.typ = Typ[UntypedInt];
                }
                return ;
            } 
            // rhs must be within reasonable bounds in constant shifts
            const nint shiftBound = 1023 - 1 + 52; // so we can express smallestFloat64 (see issue #44057)
 // so we can express smallestFloat64 (see issue #44057)
            var (s, ok) = constant.Uint64Val(y.val);
            if (!ok || s > shiftBound) {
                check.errorf(y, invalidOp + "invalid shift count %s", y);
                x.mode = invalid;
                return ;
            } 
            // The lhs is representable as an integer but may not be an integer
            // (e.g., 2.0, an untyped float) - this can only happen for untyped
            // non-integer numeric constants. Correct the type so that the shift
            // result is of integer type.
            if (!isInteger(x.typ)) {
                x.typ = Typ[UntypedInt];
            } 
            // x is a constant so xval != nil and it must be of Int kind.
            x.val = constant.Shift(xval, op2tok[op], uint(s));
            x.expr = e;
            check.overflow(x);
            return ;
        }
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
            // Example: var e, f = int(1<<""[f]) // issue 11347
            {
                var (info, found) = check.untyped[x.expr];

                if (found) {
                    info.isLhs = true;
                    check.untyped[x.expr] = info;
                } 
                // keep x's type

            } 
            // keep x's type
            x.mode = value;
            return ;
        }
    }
    if (!isInteger(x.typ)) {
        check.errorf(x, invalidOp + "shifted operand %s must be integer", x);
        x.mode = invalid;
        return ;
    }
    x.mode = value;
}

private static opPredicates binaryOpPredicates = default;

private static void init() { 
    // Setting binaryOpPredicates in init avoids declaration cycles.
    binaryOpPredicates = new opPredicates(syntax.Add:isNumericOrString,syntax.Sub:isNumeric,syntax.Mul:isNumeric,syntax.Div:isNumeric,syntax.Rem:isInteger,syntax.And:isInteger,syntax.Or:isInteger,syntax.Xor:isInteger,syntax.AndNot:isInteger,syntax.AndAnd:isBoolean,syntax.OrOr:isBoolean,);
}

// If e != nil, it must be the binary expression; it may be nil for non-constant expressions
// (when invoked for an assignment operation where the binary expression is implicit).
private static void binary(this ptr<Checker> _addr_check, ptr<operand> _addr_x, syntax.Expr e, syntax.Expr lhs, syntax.Expr rhs, syntax.Operator op) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;

    ref operand y = ref heap(out ptr<operand> _addr_y);

    check.expr(x, lhs);
    check.expr(_addr_y, rhs);

    if (x.mode == invalid) {
        return ;
    }
    if (y.mode == invalid) {
        x.mode = invalid;
        x.expr = y.expr;
        return ;
    }
    if (isShift(op)) {
        check.shift(x, _addr_y, e, op);
        return ;
    }
    check.convertUntyped(x, y.typ);
    if (x.mode == invalid) {
        return ;
    }
    check.convertUntyped(_addr_y, x.typ);
    if (y.mode == invalid) {
        x.mode = invalid;
        return ;
    }
    if (isComparison(op)) {
        check.comparison(x, _addr_y, op);
        return ;
    }
    if (!check.identical(x.typ, y.typ)) { 
        // only report an error if we have valid types
        // (otherwise we had an error reported elsewhere already)
        if (x.typ != Typ[Invalid] && y.typ != Typ[Invalid]) {
            check.errorf(x, invalidOp + "mismatched types %s and %s", x.typ, y.typ);
        }
        x.mode = invalid;
        return ;
    }
    if (!check.op(binaryOpPredicates, x, op)) {
        x.mode = invalid;
        return ;
    }
    if (op == syntax.Div || op == syntax.Rem) { 
        // check for zero divisor
        if ((x.mode == constant_ || isInteger(x.typ)) && y.mode == constant_ && constant.Sign(y.val) == 0) {
            check.error(_addr_y, invalidOp + "division by zero");
            x.mode = invalid;
            return ;
        }
        if (x.mode == constant_ && y.mode == constant_ && isComplex(x.typ)) {
            var re = constant.Real(y.val);
            var im = constant.Imag(y.val);
            var re2 = constant.BinaryOp(re, token.MUL, re);
            var im2 = constant.BinaryOp(im, token.MUL, im);
            if (constant.Sign(re2) == 0 && constant.Sign(im2) == 0) {
                check.error(_addr_y, invalidOp + "division by zero");
                x.mode = invalid;
                return ;
            }
        }
    }
    if (x.mode == constant_ && y.mode == constant_) { 
        // if either x or y has an unknown value, the result is unknown
        if (x.val.Kind() == constant.Unknown || y.val.Kind() == constant.Unknown) {
            x.val = constant.MakeUnknown(); 
            // x.typ is unchanged
            return ;
        }
        var tok = op2tok[op];
        if (op == syntax.Div && isInteger(x.typ)) {
            tok = token.QUO_ASSIGN;
        }
        x.val = constant.BinaryOp(x.val, tok, y.val);
        x.expr = e;
        check.overflow(x);
        return ;
    }
    x.mode = value; 
    // x.typ is unchanged
}

// exprKind describes the kind of an expression; the kind
// determines if an expression is valid in 'statement context'.
private partial struct exprKind { // : nint
}

private static readonly exprKind conversion = iota;
private static readonly var expression = 0;
private static readonly var statement = 1;

// rawExpr typechecks expression e and initializes x with the expression
// value or type. If an error occurred, x.mode is set to invalid.
// If hint != nil, it is the type of a composite literal element.
//
private static exprKind rawExpr(this ptr<Checker> _addr_check, ptr<operand> _addr_x, syntax.Expr e, Type hint) => func((defer, _, _) => {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;

    if (check.conf.Trace) {
        check.trace(e.Pos(), "expr %s", e);
        check.indent++;
        defer(() => {
            check.indent--;
            check.trace(e.Pos(), "=> %s", x);
        }());
    }
    var kind = check.exprInternal(x, e, hint);
    check.record(x);

    return kind;
});

// exprInternal contains the core of type checking of expressions.
// Must only be called by rawExpr.
//
private static exprKind exprInternal(this ptr<Checker> _addr_check, ptr<operand> _addr_x, syntax.Expr e, Type hint) => func((_, panic, _) => {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;
 
    // make sure x has a valid state in case of bailout
    // (was issue 5770)
    x.mode = invalid;
    x.typ = Typ[Invalid];

    switch (e.type()) {
        case 
            unreachable();
            break;
        case ptr<syntax.BadExpr> e:
            goto Error; // error was reported before
            break;
        case ptr<syntax.Name> e:
            check.ident(x, e, null, false);
            break;
        case ptr<syntax.DotsType> e:
            check.error(e, "invalid use of '...'");
            goto Error;
            break;
        case ptr<syntax.BasicLit> e:
            if (e.Bad) {
                goto Error; // error reported during parsing
            }

            if (e.Kind == syntax.IntLit || e.Kind == syntax.FloatLit || e.Kind == syntax.ImagLit) 
                check.langCompat(e); 
                // The max. mantissa precision for untyped numeric values
                // is 512 bits, or 4048 bits for each of the two integer
                // parts of a fraction for floating-point numbers that are
                // represented accurately in the go/constant package.
                // Constant literals that are longer than this many bits
                // are not meaningful; and excessively long constants may
                // consume a lot of space and time for a useless conversion.
                // Cap constant length with a generous upper limit that also
                // allows for separators between all digits.
                const nint limit = 10000;

                if (len(e.Value) > limit) {
                    check.errorf(e, "excessively long constant: %s... (%d chars)", e.Value[..(int)10], len(e.Value));
                    goto Error;
                }
                        x.setConst(e.Kind, e.Value);
            if (x.mode == invalid) { 
                // The parser already establishes syntactic correctness.
                // If we reach here it's because of number under-/overflow.
                // TODO(gri) setConst (and in turn the go/constant package)
                // should return an error describing the issue.
                check.errorf(e, "malformed constant: %s", e.Value);
                goto Error;
            }
            break;
        case ptr<syntax.FuncLit> e:
            {
                ptr<Signature> (sig, ok) = check.typ(e.Type)._<ptr<Signature>>();

                if (ok) {
                    if (!check.conf.IgnoreFuncBodies && e.Body != null) { 
                        // Anonymous functions are considered part of the
                        // init expression/func declaration which contains
                        // them: use existing package-level declaration info.
                        var decl = check.decl; // capture for use in closure below
                        var iota = check.iota; // capture for use in closure below (#22345)
                        // Don't type-check right away because the function may
                        // be part of a type definition to which the function
                        // body refers. Instead, type-check as soon as possible,
                        // but before the enclosing scope contents changes (#22992).
                        check.later(() => {
                            check.funcBody(decl, "<function literal>", sig, e.Body, iota);
                        });
                    }
                else
                    x.mode = value;
                    x.typ = sig;
                } {
                    check.errorf(e, invalidAST + "invalid function literal %v", e);
                    goto Error;
                }

            }
            break;
        case ptr<syntax.CompositeLit> e:
            Type typ = default;            Type @base = default;




            if (e.Type != null) 
                // composite literal type present - use it
                // [...]T array types may only appear with composite literals.
                // Check for them here so we don't have to handle ... in general.
                {
                    ptr<syntax.ArrayType> (atyp, _) = e.Type._<ptr<syntax.ArrayType>>();

                    if (atyp != null && atyp.Len == null) { 
                        // We have an "open" [...]T array type.
                        // Create a new ArrayType with unknown length (-1)
                        // and finish setting it up after analyzing the literal.
                        typ = addr(new Array(len:-1,elem:check.varType(atyp.Elem)));
                        base = typ;
                        break;
                    }

                }
                typ = check.typ(e.Type);
                base = typ;
            else if (hint != null) 
                // no composite literal type present - use hint (element type of enclosing type)
                typ = hint;
                base, _ = deref(under(typ)); // *T implies &T{}
            else 
                // TODO(gri) provide better error messages depending on context
                check.error(e, "missing type in composite literal");
                goto Error;
                        switch (optype(base).type()) {
                case ptr<Struct> utyp:
                    if (len(e.ElemList) == 0) {
                        break;
                    }
                    var fields = utyp.fields;
                    {
                        ptr<syntax.KeyValueExpr> (_, ok) = e.ElemList[0]._<ptr<syntax.KeyValueExpr>>();

                        if (ok) { 
                            // all elements must have keys
                            var visited = make_slice<bool>(len(fields));
                            {
                                var e__prev1 = e;

                                foreach (var (_, __e) in e.ElemList) {
                                    e = __e;
                                    ptr<syntax.KeyValueExpr> (kv, _) = e._<ptr<syntax.KeyValueExpr>>();
                                    if (kv == null) {
                                        check.error(e, "mixture of field:value and value elements in struct literal");
                                        continue;
                                    }
                                    ptr<syntax.Name> (key, _) = kv.Key._<ptr<syntax.Name>>(); 
                                    // do all possible checks early (before exiting due to errors)
                                    // so we don't drop information on the floor
                                    check.expr(x, kv.Value);
                                    if (key == null) {
                                        check.errorf(kv, "invalid field name %s in struct literal", kv.Key);
                                        continue;
                                    }
                                    var i = fieldIndex(utyp.fields, check.pkg, key.Value);
                                    if (i < 0) {
                                        if (check.conf.CompilerErrorMessages) {
                                            check.errorf(kv.Key, "unknown field '%s' in struct literal of type %s", key.Value, base);
                                        }
                                        else
 {
                                            check.errorf(kv.Key, "unknown field %s in struct literal", key.Value);
                                        }
                                        continue;
                                    }
                                    var fld = fields[i];
                                    check.recordUse(key, fld);
                                    var etyp = fld.typ;
                                    check.assignment(x, etyp, "struct literal"); 
                                    // 0 <= i < len(fields)
                                    if (visited[i]) {
                                        check.errorf(kv, "duplicate field name %s in struct literal", key.Value);
                                        continue;
                                    }
                                    visited[i] = true;
                                }
                        else

                                e = e__prev1;
                            }
                        } { 
                            // no element must have a key
                            {
                                var i__prev1 = i;
                                var e__prev1 = e;

                                foreach (var (__i, __e) in e.ElemList) {
                                    i = __i;
                                    e = __e;
                                    {
                                        ptr<syntax.KeyValueExpr> kv__prev2 = kv;

                                        (kv, _) = e._<ptr<syntax.KeyValueExpr>>();

                                        if (kv != null) {
                                            check.error(kv, "mixture of field:value and value elements in struct literal");
                                            continue;
                                        }

                                        kv = kv__prev2;

                                    }
                                    check.expr(x, e);
                                    if (i >= len(fields)) {
                                        check.error(x, "too many values in struct literal");
                                        break; // cannot continue
                                    } 
                                    // i < len(fields)
                                    fld = fields[i];
                                    if (!fld.Exported() && fld.pkg != check.pkg) {
                                        check.errorf(x, "implicit assignment to unexported field %s in %s literal", fld.name, typ);
                                        continue;
                                    }
                                    etyp = fld.typ;
                                    check.assignment(x, etyp, "struct literal");
                                }

                                i = i__prev1;
                                e = e__prev1;
                            }

                            if (len(e.ElemList) < len(fields)) {
                                check.error(e.Rbrace, "too few values in struct literal"); 
                                // ok to continue
                            }
                        }

                    }
                    break;
                case ptr<Array> utyp:
                    if (utyp.elem == null) {
                        check.error(e, "illegal cycle in type declaration");
                        goto Error;
                    }
                    var n = check.indexedElts(e.ElemList, utyp.elem, utyp.len); 
                    // If we have an array of unknown length (usually [...]T arrays, but also
                    // arrays [n]T where n is invalid) set the length now that we know it and
                    // record the type for the array (usually done by check.typ which is not
                    // called for [...]T). We handle [...]T arrays and arrays with invalid
                    // length the same here because it makes sense to "guess" the length for
                    // the latter if we have a composite literal; e.g. for [n]int{1, 2, 3}
                    // where n is invalid for some reason, it seems fair to assume it should
                    // be 3 (see also Checked.arrayLength and issue #27346).
                    if (utyp.len < 0) {
                        utyp.len = n; 
                        // e.Type is missing if we have a composite literal element
                        // that is itself a composite literal with omitted type. In
                        // that case there is nothing to record (there is no type in
                        // the source at that point).
                        if (e.Type != null) {
                            check.recordTypeAndValue(e.Type, typexpr, utyp, null);
                        }
                    }
                    break;
                case ptr<Slice> utyp:
                    if (utyp.elem == null) {
                        check.error(e, "illegal cycle in type declaration");
                        goto Error;
                    }
                    check.indexedElts(e.ElemList, utyp.elem, -1);
                    break;
                case ptr<Map> utyp:
                    if (utyp.key == null || utyp.elem == null) {
                        check.error(e, "illegal cycle in type declaration");
                        goto Error;
                    }
                    visited = make(len(e.ElemList));
                    {
                        var e__prev1 = e;

                        foreach (var (_, __e) in e.ElemList) {
                            e = __e;
                            (kv, _) = e._<ptr<syntax.KeyValueExpr>>();
                            if (kv == null) {
                                check.error(e, "missing key in map literal");
                                continue;
                            }
                            check.exprWithHint(x, kv.Key, utyp.key);
                            check.assignment(x, utyp.key, "map literal");
                            if (x.mode == invalid) {
                                continue;
                            }
                            if (x.mode == constant_) {
                                var duplicate = false; 
                                // if the key is of interface type, the type is also significant when checking for duplicates
                                var xkey = keyVal(x.val);
                                if (asInterface(utyp.key) != null) {
                                    foreach (var (_, vtyp) in visited[xkey]) {
                                        if (check.identical(vtyp, x.typ)) {
                                            duplicate = true;
                                            break;
                                        }
                                    }
                                else
                                    visited[xkey] = append(visited[xkey], x.typ);
                                } {
                                    _, duplicate = visited[xkey];
                                    visited[xkey] = null;
                                }
                                if (duplicate) {
                                    check.errorf(x, "duplicate key %s in map literal", x.val);
                                    continue;
                                }
                            }
                            check.exprWithHint(x, kv.Value, utyp.elem);
                            check.assignment(x, utyp.elem, "map literal");
                        }

                        e = e__prev1;
                    }
                    break;
                default:
                {
                    var utyp = optype(base).type();
                    {
                        var e__prev1 = e;

                        foreach (var (_, __e) in e.ElemList) {
                            e = __e;
                            {
                                ptr<syntax.KeyValueExpr> kv__prev1 = kv;

                                (kv, _) = e._<ptr<syntax.KeyValueExpr>>();

                                if (kv != null) { 
                                    // Ideally, we should also "use" kv.Key but we can't know
                                    // if it's an externally defined struct key or not. Going
                                    // forward anyway can lead to other errors. Give up instead.
                                    e = kv.Value;
                                }

                                kv = kv__prev1;

                            }
                            check.use(e);
                        } 
                        // if utyp is invalid, an error was reported before

                        e = e__prev1;
                    }

                    if (utyp != Typ[Invalid]) {
                        check.errorf(e, "invalid composite literal type %s", typ);
                        goto Error;
                    }
                    break;
                }

            }

            x.mode = value;
            x.typ = typ;
            break;
        case ptr<syntax.ParenExpr> e:
            var kind = check.rawExpr(x, e.X, null);
            x.expr = e;
            return kind;
            break;
        case ptr<syntax.SelectorExpr> e:
            check.selector(x, e);
            break;
        case ptr<syntax.IndexExpr> e:
            if (check.indexExpr(x, e)) {
                check.funcInst(x, e);
            }
            if (x.mode == invalid) {
                goto Error;
            }
            break;
        case ptr<syntax.SliceExpr> e:
            check.sliceExpr(x, e);
            if (x.mode == invalid) {
                goto Error;
            }
            break;
        case ptr<syntax.AssertExpr> e:
            check.expr(x, e.X);
            if (x.mode == invalid) {
                goto Error;
            }
            ptr<Interface> (xtyp, _) = under(x.typ)._<ptr<Interface>>();
            if (xtyp == null) {
                check.errorf(x, "%s is not an interface type", x);
                goto Error;
            }
            check.ordinaryType(x.Pos(), xtyp); 
            // x.(type) expressions are encoded via TypeSwitchGuards
            if (e.Type == null) {
                check.error(e, invalidAST + "invalid use of AssertExpr");
                goto Error;
            }
            var T = check.varType(e.Type);
            if (T == Typ[Invalid]) {
                goto Error;
            }
            check.typeAssertion(posFor(x), x, xtyp, T);
            x.mode = commaok;
            x.typ = T;
            break;
        case ptr<syntax.TypeSwitchGuard> e:
            check.error(e, invalidAST + "use of .(type) outside type switch");
            goto Error;
            break;
        case ptr<syntax.CallExpr> e:
            return check.callExpr(x, e);
            break;
        case ptr<syntax.ListExpr> e:
            check.error(e, "unexpected list of expressions");
            goto Error; 

            // case *syntax.UnaryExpr:
            //     check.expr(x, e.X)
            //     if x.mode == invalid {
            //         goto Error
            //     }
            //     check.unary(x, e, e.Op)
            //     if x.mode == invalid {
            //         goto Error
            //     }
            //     if e.Op == token.ARROW {
            //         x.expr = e
            //         return statement // receive operations may appear in statement context
            //     }

            // case *syntax.BinaryExpr:
            //     check.binary(x, e, e.X, e.Y, e.Op)
            //     if x.mode == invalid {
            //         goto Error
            //     }
            break;
        case ptr<syntax.Operation> e:
            if (e.Y == null) { 
                // unary expression
                if (e.Op == syntax.Mul) { 
                    // pointer indirection
                    check.exprOrType(x, e.X);

                    if (x.mode == invalid) 
                        goto Error;
                    else if (x.mode == typexpr) 
                        x.typ = addr(new Pointer(base:x.typ));
                    else 
                        {
                            Type typ__prev3 = typ;

                            typ = asPointer(x.typ);

                            if (typ != null) {
                                x.mode = variable;
                                x.typ = typ.@base;
                            }
                            else
 {
                                check.errorf(x, invalidOp + "cannot indirect %s", x);
                                goto Error;
                            }

                            typ = typ__prev3;

                        }
                                        break;
                }
                check.unary(x, e);
                if (x.mode == invalid) {
                    goto Error;
                }
                if (e.Op == syntax.Recv) {
                    x.expr = e;
                    return statement; // receive operations may appear in statement context
                }
                break;
            } 

            // binary expression
            check.binary(x, e, e.X, e.Y, e.Op);
            if (x.mode == invalid) {
                goto Error;
            }
            break;
        case ptr<syntax.KeyValueExpr> e:
            check.error(e, invalidAST + "no key:value expected");
            goto Error;
            break;
        case ptr<syntax.ArrayType> e:
            x.mode = typexpr;
            x.typ = check.typ(e); 
            // Note: rawExpr (caller of exprInternal) will call check.recordTypeAndValue
            // even though check.typ has already called it. This is fine as both
            // times the same expression and type are recorded. It is also not a
            // performance issue because we only reach here for composite literal
            // types, which are comparatively rare.
            break;
        case ptr<syntax.SliceType> e:
            x.mode = typexpr;
            x.typ = check.typ(e); 
            // Note: rawExpr (caller of exprInternal) will call check.recordTypeAndValue
            // even though check.typ has already called it. This is fine as both
            // times the same expression and type are recorded. It is also not a
            // performance issue because we only reach here for composite literal
            // types, which are comparatively rare.
            break;
        case ptr<syntax.StructType> e:
            x.mode = typexpr;
            x.typ = check.typ(e); 
            // Note: rawExpr (caller of exprInternal) will call check.recordTypeAndValue
            // even though check.typ has already called it. This is fine as both
            // times the same expression and type are recorded. It is also not a
            // performance issue because we only reach here for composite literal
            // types, which are comparatively rare.
            break;
        case ptr<syntax.FuncType> e:
            x.mode = typexpr;
            x.typ = check.typ(e); 
            // Note: rawExpr (caller of exprInternal) will call check.recordTypeAndValue
            // even though check.typ has already called it. This is fine as both
            // times the same expression and type are recorded. It is also not a
            // performance issue because we only reach here for composite literal
            // types, which are comparatively rare.
            break;
        case ptr<syntax.InterfaceType> e:
            x.mode = typexpr;
            x.typ = check.typ(e); 
            // Note: rawExpr (caller of exprInternal) will call check.recordTypeAndValue
            // even though check.typ has already called it. This is fine as both
            // times the same expression and type are recorded. It is also not a
            // performance issue because we only reach here for composite literal
            // types, which are comparatively rare.
            break;
        case ptr<syntax.MapType> e:
            x.mode = typexpr;
            x.typ = check.typ(e); 
            // Note: rawExpr (caller of exprInternal) will call check.recordTypeAndValue
            // even though check.typ has already called it. This is fine as both
            // times the same expression and type are recorded. It is also not a
            // performance issue because we only reach here for composite literal
            // types, which are comparatively rare.
            break;
        case ptr<syntax.ChanType> e:
            x.mode = typexpr;
            x.typ = check.typ(e); 
            // Note: rawExpr (caller of exprInternal) will call check.recordTypeAndValue
            // even though check.typ has already called it. This is fine as both
            // times the same expression and type are recorded. It is also not a
            // performance issue because we only reach here for composite literal
            // types, which are comparatively rare.
            break;
        default:
        {
            var e = e.type();
            panic(fmt.Sprintf("%s: unknown expression type %T", posFor(e), e));
            break;
        } 

        // everything went well
    } 

    // everything went well
    x.expr = e;
    return expression;

Error:
    x.mode = invalid;
    x.expr = e;
    return statement; // avoid follow-up errors
});

private static void keyVal(constant.Value x) {

    if (x.Kind() == constant.Bool) 
        return constant.BoolVal(x);
    else if (x.Kind() == constant.String) 
        return constant.StringVal(x);
    else if (x.Kind() == constant.Int) 
        {
            var v__prev1 = v;

            var (v, ok) = constant.Int64Val(x);

            if (ok) {
                return v;
            }

            v = v__prev1;

        }
        {
            var v__prev1 = v;

            (v, ok) = constant.Uint64Val(x);

            if (ok) {
                return v;
            }

            v = v__prev1;

        }
    else if (x.Kind() == constant.Float) 
        var (v, _) = constant.Float64Val(x);
        return v;
    else if (x.Kind() == constant.Complex) 
        var (r, _) = constant.Float64Val(constant.Real(x));
        var (i, _) = constant.Float64Val(constant.Imag(x));
        return complex(r, i);
        return x;
}

// typeAssertion checks that x.(T) is legal; xtyp must be the type of x.
private static void typeAssertion(this ptr<Checker> _addr_check, syntax.Pos pos, ptr<operand> _addr_x, ptr<Interface> _addr_xtyp, Type T) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;
    ref Interface xtyp = ref _addr_xtyp.val;

    var (method, wrongType) = check.assertableTo(xtyp, T);
    if (method == null) {
        return ;
    }
    @string msg = default;
    if (wrongType != null) {
        if (check.identical(method.typ, wrongType.typ)) {
            msg = fmt.Sprintf("missing method %s (%s has pointer receiver)", method.name, method.name);
        }
        else
 {
            msg = fmt.Sprintf("wrong type for method %s (have %s, want %s)", method.name, wrongType.typ, method.typ);
        }
    }
    else
 {
        msg = "missing method " + method.name;
    }
    if (check.conf.CompilerErrorMessages) {
        check.errorf(pos, "impossible type assertion: %s (%s)", x, msg);
    }
    else
 {
        check.errorf(pos, "%s cannot have dynamic type %s (%s)", x, T, msg);
    }
}

// expr typechecks expression e and initializes x with the expression value.
// The result must be a single value.
// If an error occurred, x.mode is set to invalid.
//
private static void expr(this ptr<Checker> _addr_check, ptr<operand> _addr_x, syntax.Expr e) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;

    check.rawExpr(x, e, null);
    check.exclude(x, 1 << (int)(novalue) | 1 << (int)(builtin) | 1 << (int)(typexpr));
    check.singleValue(x);
}

// multiExpr is like expr but the result may also be a multi-value.
private static void multiExpr(this ptr<Checker> _addr_check, ptr<operand> _addr_x, syntax.Expr e) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;

    check.rawExpr(x, e, null);
    check.exclude(x, 1 << (int)(novalue) | 1 << (int)(builtin) | 1 << (int)(typexpr));
}

// exprWithHint typechecks expression e and initializes x with the expression value;
// hint is the type of a composite literal element.
// If an error occurred, x.mode is set to invalid.
//
private static void exprWithHint(this ptr<Checker> _addr_check, ptr<operand> _addr_x, syntax.Expr e, Type hint) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;

    assert(hint != null);
    check.rawExpr(x, e, hint);
    check.exclude(x, 1 << (int)(novalue) | 1 << (int)(builtin) | 1 << (int)(typexpr));
    check.singleValue(x);
}

// exprOrType typechecks expression or type e and initializes x with the expression value or type.
// If an error occurred, x.mode is set to invalid.
//
private static void exprOrType(this ptr<Checker> _addr_check, ptr<operand> _addr_x, syntax.Expr e) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;

    check.rawExpr(x, e, null);
    check.exclude(x, 1 << (int)(novalue));
    check.singleValue(x);
}

// exclude reports an error if x.mode is in modeset and sets x.mode to invalid.
// The modeset may contain any of 1<<novalue, 1<<builtin, 1<<typexpr.
private static void exclude(this ptr<Checker> _addr_check, ptr<operand> _addr_x, nuint modeset) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;

    if (modeset & (1 << (int)(x.mode)) != 0) {
        @string msg = default;

        if (x.mode == novalue) 
            if (modeset & (1 << (int)(typexpr)) != 0) {
                msg = "%s used as value";
            }
            else
 {
                msg = "%s used as value or type";
            }
        else if (x.mode == builtin) 
            msg = "%s must be called";
        else if (x.mode == typexpr) 
            msg = "%s is not an expression";
        else 
            unreachable();
                check.errorf(x, msg, x);
        x.mode = invalid;
    }
}

// singleValue reports an error if x describes a tuple and sets x.mode to invalid.
private static void singleValue(this ptr<Checker> _addr_check, ptr<operand> _addr_x) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;

    if (x.mode == value) { 
        // tuple types are never named - no need for underlying type below
        {
            ptr<Tuple> (t, ok) = x.typ._<ptr<Tuple>>();

            if (ok) {
                assert(t.Len() != 1);
                check.errorf(x, "%d-valued %s where single value is expected", t.Len(), x);
                x.mode = invalid;
            }

        }
    }
}

// op2tok translates syntax.Operators into token.Tokens.
private static array<token.Token> op2tok = new array<token.Token>(InitKeyedValues<token.Token>((syntax.Def, token.ILLEGAL), (syntax.Not, token.NOT), (syntax.Recv, token.ILLEGAL), (syntax.OrOr, token.LOR), (syntax.AndAnd, token.LAND), (syntax.Eql, token.EQL), (syntax.Neq, token.NEQ), (syntax.Lss, token.LSS), (syntax.Leq, token.LEQ), (syntax.Gtr, token.GTR), (syntax.Geq, token.GEQ), (syntax.Add, token.ADD), (syntax.Sub, token.SUB), (syntax.Or, token.OR), (syntax.Xor, token.XOR), (syntax.Mul, token.MUL), (syntax.Div, token.QUO), (syntax.Rem, token.REM), (syntax.And, token.AND), (syntax.AndNot, token.AND_NOT), (syntax.Shl, token.SHL), (syntax.Shr, token.SHR)));

} // end types2_package
