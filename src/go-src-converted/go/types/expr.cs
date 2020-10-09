// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements typechecking of expressions.

// package types -- go2cs converted at 2020 October 09 05:19:27 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\expr.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using math = go.math_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class types_package
    {
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
        private partial struct opPredicates // : map<token.Token, Func<Type, bool>>
        {
        }

        private static opPredicates unaryOpPredicates = new opPredicates(token.ADD:isNumeric,token.SUB:isNumeric,token.XOR:isInteger,token.NOT:isBoolean,);

        private static bool op(this ptr<Checker> _addr_check, opPredicates m, ptr<operand> _addr_x, token.Token op)
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;

            {
                var pred = m[op];

                if (pred != null)
                {
                    if (!pred(x.typ))
                    {
                        check.invalidOp(x.pos(), "operator %s not defined for %s", op, x);
                        return false;
                    }

                }
                else
                {
                    check.invalidAST(x.pos(), "unknown operator %s", op);
                    return false;
                }

            }

            return true;

        }

        // The unary expression e may be nil. It's passed in for better error messages only.
        private static void unary(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<ast.UnaryExpr> _addr_e, token.Token op)
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;
            ref ast.UnaryExpr e = ref _addr_e.val;


            if (op == token.AND) 
                // spec: "As an exception to the addressability
                // requirement x may also be a composite literal."
                {
                    ptr<ast.CompositeLit> (_, ok) = unparen(x.expr)._<ptr<ast.CompositeLit>>();

                    if (!ok && x.mode != variable)
                    {
                        check.invalidOp(x.pos(), "cannot take address of %s", x);
                        x.mode = invalid;
                        return ;
                    }

                }

                x.mode = value;
                x.typ = addr(new Pointer(base:x.typ));
                return ;
            else if (op == token.ARROW) 
                ptr<Chan> (typ, ok) = x.typ.Underlying()._<ptr<Chan>>();
                if (!ok)
                {
                    check.invalidOp(x.pos(), "cannot receive from non-channel %s", x);
                    x.mode = invalid;
                    return ;
                }

                if (typ.dir == SendOnly)
                {
                    check.invalidOp(x.pos(), "cannot receive from send-only channel %s", x);
                    x.mode = invalid;
                    return ;
                }

                x.mode = commaok;
                x.typ = typ.elem;
                check.hasCallOrRecv = true;
                return ;
                        if (!check.op(unaryOpPredicates, x, op))
            {
                x.mode = invalid;
                return ;
            }

            if (x.mode == constant_)
            {
                ptr<Basic> typ = x.typ.Underlying()._<ptr<Basic>>();
                ulong prec = default;
                if (isUnsigned(typ))
                {
                    prec = uint(check.conf.@sizeof(typ) * 8L);
                }

                x.val = constant.UnaryOp(op, x.val, prec); 
                // Typed constants must be representable in
                // their type after each constant operation.
                if (isTyped(typ))
                {
                    if (e != null)
                    {
                        x.expr = e; // for better error message
                    }

                    check.representable(x, typ);

                }

                return ;

            }

            x.mode = value; 
            // x.typ remains unchanged
        }

        private static bool isShift(token.Token op)
        {
            return op == token.SHL || op == token.SHR;
        }

        private static bool isComparison(token.Token op)
        { 
            // Note: tokens are not ordered well to make this much easier

            if (op == token.EQL || op == token.NEQ || op == token.LSS || op == token.LEQ || op == token.GTR || op == token.GEQ) 
                return true;
                        return false;

        }

        private static bool fitsFloat32(constant.Value x)
        {
            var (f32, _) = constant.Float32Val(x);
            var f = float64(f32);
            return !math.IsInf(f, 0L);
        }

        private static constant.Value roundFloat32(constant.Value x)
        {
            var (f32, _) = constant.Float32Val(x);
            var f = float64(f32);
            if (!math.IsInf(f, 0L))
            {
                return constant.MakeFloat64(f);
            }

            return null;

        }

        private static bool fitsFloat64(constant.Value x)
        {
            var (f, _) = constant.Float64Val(x);
            return !math.IsInf(f, 0L);
        }

        private static constant.Value roundFloat64(constant.Value x)
        {
            var (f, _) = constant.Float64Val(x);
            if (!math.IsInf(f, 0L))
            {
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
        private static bool representableConst(constant.Value x, ptr<Checker> _addr_check, ptr<Basic> _addr_typ, ptr<constant.Value> _addr_rounded)
        {
            ref Checker check = ref _addr_check.val;
            ref Basic typ = ref _addr_typ.val;
            ref constant.Value rounded = ref _addr_rounded.val;

            if (x.Kind() == constant.Unknown)
            {
                return true; // avoid follow-up errors
            }

            ptr<Config> conf;
            if (check != null)
            {
                conf = check.conf;
            }


            if (isInteger(typ)) 
                var x = constant.ToInt(x);
                if (x.Kind() != constant.Int)
                {
                    return false;
                }

                if (rounded != null)
                {
                    rounded = x;
                }

                {
                    var x__prev1 = x;

                    var (x, ok) = constant.Int64Val(x);

                    if (ok)
                    {

                        if (typ.kind == Int) 
                            var s = uint(conf.@sizeof(typ)) * 8L;
                            return int64(-1L) << (int)((s - 1L)) <= x && x <= int64(1L) << (int)((s - 1L)) - 1L;
                        else if (typ.kind == Int8) 
                            const long s = (long)8L;

                            return -1L << (int)((s - 1L)) <= x && x <= 1L << (int)((s - 1L)) - 1L;
                        else if (typ.kind == Int16) 
                            const long s = (long)16L;

                            return -1L << (int)((s - 1L)) <= x && x <= 1L << (int)((s - 1L)) - 1L;
                        else if (typ.kind == Int32) 
                            const long s = (long)32L;

                            return -1L << (int)((s - 1L)) <= x && x <= 1L << (int)((s - 1L)) - 1L;
                        else if (typ.kind == Int64 || typ.kind == UntypedInt) 
                            return true;
                        else if (typ.kind == Uint || typ.kind == Uintptr) 
                            {
                                var s__prev2 = s;

                                s = uint(conf.@sizeof(typ)) * 8L;

                                if (s < 64L)
                                {
                                    return 0L <= x && x <= int64(1L) << (int)(s) - 1L;
                                }

                                s = s__prev2;

                            }

                            return 0L <= x;
                        else if (typ.kind == Uint8) 
                            const long s = (long)8L;

                            return 0L <= x && x <= 1L << (int)(s) - 1L;
                        else if (typ.kind == Uint16) 
                            const long s = (long)16L;

                            return 0L <= x && x <= 1L << (int)(s) - 1L;
                        else if (typ.kind == Uint32) 
                            const long s = (long)32L;

                            return 0L <= x && x <= 1L << (int)(s) - 1L;
                        else if (typ.kind == Uint64) 
                            return 0L <= x;
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
                        s = uint(conf.@sizeof(typ)) * 8L;
                        return constant.Sign(x) >= 0L && n <= int(s);
                    else if (typ.kind == Uint64) 
                        return constant.Sign(x) >= 0L && n <= 64L;
                    else if (typ.kind == UntypedInt) 
                        return true;

                }
            else if (isFloat(typ)) 
                x = constant.ToFloat(x);
                if (x.Kind() != constant.Float)
                {
                    return false;
                }


                if (typ.kind == Float32) 
                    if (rounded == null)
                    {
                        return fitsFloat32(x);
                    }

                    var r = roundFloat32(x);
                    if (r != null)
                    {
                        rounded = r;
                        return true;
                    }

                else if (typ.kind == Float64) 
                    if (rounded == null)
                    {
                        return fitsFloat64(x);
                    }

                    r = roundFloat64(x);
                    if (r != null)
                    {
                        rounded = r;
                        return true;
                    }

                else if (typ.kind == UntypedFloat) 
                    return true;
                else 
                    unreachable();
                            else if (isComplex(typ)) 
                x = constant.ToComplex(x);
                if (x.Kind() != constant.Complex)
                {
                    return false;
                }


                if (typ.kind == Complex64) 
                    if (rounded == null)
                    {
                        return fitsFloat32(constant.Real(x)) && fitsFloat32(constant.Imag(x));
                    }

                    var re = roundFloat32(constant.Real(x));
                    var im = roundFloat32(constant.Imag(x));
                    if (re != null && im != null)
                    {
                        rounded = constant.BinaryOp(re, token.ADD, constant.MakeImag(im));
                        return true;
                    }

                else if (typ.kind == Complex128) 
                    if (rounded == null)
                    {
                        return fitsFloat64(constant.Real(x)) && fitsFloat64(constant.Imag(x));
                    }

                    re = roundFloat64(constant.Real(x));
                    im = roundFloat64(constant.Imag(x));
                    if (re != null && im != null)
                    {
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

        // representable checks that a constant operand is representable in the given basic type.
        private static void representable(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<Basic> _addr_typ)
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;
            ref Basic typ = ref _addr_typ.val;

            assert(x.mode == constant_);
            if (!representableConst(x.val, _addr_check, _addr_typ, _addr_x.val))
            {
                @string msg = default;
                if (isNumeric(x.typ) && isNumeric(typ))
                { 
                    // numeric conversion : error msg
                    //
                    // integer -> integer : overflows
                    // integer -> float   : overflows (actually not possible)
                    // float   -> integer : truncated
                    // float   -> float   : overflows
                    //
                    if (!isInteger(x.typ) && isInteger(typ))
                    {
                        msg = "%s truncated to %s";
                    }
                    else
                    {
                        msg = "%s overflows %s";
                    }

                }
                else
                {
                    msg = "cannot convert %s to %s";
                }

                check.errorf(x.pos(), msg, x, typ);
                x.mode = invalid;

            }

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
        private static void updateExprType(this ptr<Checker> _addr_check, ast.Expr x, Type typ, bool final)
        {
            ref Checker check = ref _addr_check.val;

            var (old, found) = check.untyped[x];
            if (!found)
            {
                return ; // nothing to do
            } 

            // update operands of x if necessary
            switch (x.type())
            {
                case ptr<ast.BadExpr> x:
                    if (debug)
                    {
                        check.dump("%v: found old type(%s): %s (new: %s)", x.Pos(), x, old.typ, typ);
                        unreachable();
                    }

                    return ;
                    break;
                case ptr<ast.FuncLit> x:
                    if (debug)
                    {
                        check.dump("%v: found old type(%s): %s (new: %s)", x.Pos(), x, old.typ, typ);
                        unreachable();
                    }

                    return ;
                    break;
                case ptr<ast.CompositeLit> x:
                    if (debug)
                    {
                        check.dump("%v: found old type(%s): %s (new: %s)", x.Pos(), x, old.typ, typ);
                        unreachable();
                    }

                    return ;
                    break;
                case ptr<ast.IndexExpr> x:
                    if (debug)
                    {
                        check.dump("%v: found old type(%s): %s (new: %s)", x.Pos(), x, old.typ, typ);
                        unreachable();
                    }

                    return ;
                    break;
                case ptr<ast.SliceExpr> x:
                    if (debug)
                    {
                        check.dump("%v: found old type(%s): %s (new: %s)", x.Pos(), x, old.typ, typ);
                        unreachable();
                    }

                    return ;
                    break;
                case ptr<ast.TypeAssertExpr> x:
                    if (debug)
                    {
                        check.dump("%v: found old type(%s): %s (new: %s)", x.Pos(), x, old.typ, typ);
                        unreachable();
                    }

                    return ;
                    break;
                case ptr<ast.StarExpr> x:
                    if (debug)
                    {
                        check.dump("%v: found old type(%s): %s (new: %s)", x.Pos(), x, old.typ, typ);
                        unreachable();
                    }

                    return ;
                    break;
                case ptr<ast.KeyValueExpr> x:
                    if (debug)
                    {
                        check.dump("%v: found old type(%s): %s (new: %s)", x.Pos(), x, old.typ, typ);
                        unreachable();
                    }

                    return ;
                    break;
                case ptr<ast.ArrayType> x:
                    if (debug)
                    {
                        check.dump("%v: found old type(%s): %s (new: %s)", x.Pos(), x, old.typ, typ);
                        unreachable();
                    }

                    return ;
                    break;
                case ptr<ast.StructType> x:
                    if (debug)
                    {
                        check.dump("%v: found old type(%s): %s (new: %s)", x.Pos(), x, old.typ, typ);
                        unreachable();
                    }

                    return ;
                    break;
                case ptr<ast.FuncType> x:
                    if (debug)
                    {
                        check.dump("%v: found old type(%s): %s (new: %s)", x.Pos(), x, old.typ, typ);
                        unreachable();
                    }

                    return ;
                    break;
                case ptr<ast.InterfaceType> x:
                    if (debug)
                    {
                        check.dump("%v: found old type(%s): %s (new: %s)", x.Pos(), x, old.typ, typ);
                        unreachable();
                    }

                    return ;
                    break;
                case ptr<ast.MapType> x:
                    if (debug)
                    {
                        check.dump("%v: found old type(%s): %s (new: %s)", x.Pos(), x, old.typ, typ);
                        unreachable();
                    }

                    return ;
                    break;
                case ptr<ast.ChanType> x:
                    if (debug)
                    {
                        check.dump("%v: found old type(%s): %s (new: %s)", x.Pos(), x, old.typ, typ);
                        unreachable();
                    }

                    return ;
                    break;
                case ptr<ast.CallExpr> x:
                    break;
                case ptr<ast.Ident> x:
                    break;
                case ptr<ast.BasicLit> x:
                    break;
                case ptr<ast.SelectorExpr> x:
                    break;
                case ptr<ast.ParenExpr> x:
                    check.updateExprType(x.X, typ, final);
                    break;
                case ptr<ast.UnaryExpr> x:
                    if (old.val != null)
                    {
                        break;
                    }

                    check.updateExprType(x.X, typ, final);
                    break;
                case ptr<ast.BinaryExpr> x:
                    if (old.val != null)
                    {
                        break; // see comment for unary expressions
                    }

                    if (isComparison(x.Op))
                    { 
                        // The result type is independent of operand types
                        // and the operand types must have final types.
                    }
                    else if (isShift(x.Op))
                    { 
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
            if (!final && isUntyped(typ))
            {
                old.typ = typ.Underlying()._<ptr<Basic>>();
                check.untyped[x] = old;
                return ;
            } 

            // Otherwise we have the final (typed or untyped type).
            // Remove it from the map of yet untyped expressions.
            delete(check.untyped, x);

            if (old.isLhs)
            { 
                // If x is the lhs of a shift, its final type must be integer.
                // We already know from the shift check that it is representable
                // as an integer if it is a constant.
                if (!isInteger(typ))
                {
                    check.invalidOp(x.Pos(), "shifted operand %s (type %s) must be integer", x, typ);
                    return ;
                } 
                // Even if we have an integer, if the value is a constant we
                // still must check that it is representable as the specific
                // int type requested (was issue #22969). Fall through here.
            }

            if (old.val != null)
            { 
                // If x is a constant, it must be representable as a value of typ.
                ref operand c = ref heap(new operand(old.mode,x,old.typ,old.val,0), out ptr<operand> _addr_c);
                check.convertUntyped(_addr_c, typ);
                if (c.mode == invalid)
                {
                    return ;
                }

            } 

            // Everything's fine, record final type and value for x.
            check.recordTypeAndValue(x, old.mode, typ, old.val);

        }

        // updateExprVal updates the value of x to val.
        private static void updateExprVal(this ptr<Checker> _addr_check, ast.Expr x, constant.Value val)
        {
            ref Checker check = ref _addr_check.val;

            {
                var (info, ok) = check.untyped[x];

                if (ok)
                {
                    info.val = val;
                    check.untyped[x] = info;
                }

            }

        }

        // convertUntyped attempts to set the type of an untyped value to the target type.
        private static void convertUntyped(this ptr<Checker> _addr_check, ptr<operand> _addr_x, Type target)
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;

            if (x.mode == invalid || isTyped(x.typ) || target == Typ[Invalid])
            {
                return ;
            } 

            // TODO(gri) Sloppy code - clean up. This function is central
            //           to assignment and expression checking.
            if (isUntyped(target))
            { 
                // both x and target are untyped
                ptr<Basic> xkind = x.typ._<ptr<Basic>>().kind;
                ptr<Basic> tkind = target._<ptr<Basic>>().kind;
                if (isNumeric(x.typ) && isNumeric(target))
                {
                    if (xkind < tkind)
                    {
                        x.typ = target;
                        check.updateExprType(x.expr, target, false);
                    }

                }
                else if (xkind != tkind)
                {
                    goto Error;
                }

                return ;

            } 

            // typed target
            switch (target.Underlying().type())
            {
                case ptr<Basic> t:
                    if (x.mode == constant_)
                    {
                        check.representable(x, t);
                        if (x.mode == invalid)
                        {
                            return ;
                        } 
                        // expression value may have been rounded - update if needed
                        check.updateExprVal(x.expr, x.val);

                    }
                    else
                    { 
                        // Non-constant untyped values may appear as the
                        // result of comparisons (untyped bool), intermediate
                        // (delayed-checked) rhs operands of shifts, and as
                        // the value nil.

                        if (x.typ._<ptr<Basic>>().kind == UntypedBool) 
                            if (!isBoolean(target))
                            {
                                goto Error;
                            }

                        else if (x.typ._<ptr<Basic>>().kind == UntypedInt || x.typ._<ptr<Basic>>().kind == UntypedRune || x.typ._<ptr<Basic>>().kind == UntypedFloat || x.typ._<ptr<Basic>>().kind == UntypedComplex) 
                            if (!isNumeric(target))
                            {
                                goto Error;
                            }

                        else if (x.typ._<ptr<Basic>>().kind == UntypedString) 
                            // Non-constant untyped string values are not
                            // permitted by the spec and should not occur.
                            unreachable();
                        else if (x.typ._<ptr<Basic>>().kind == UntypedNil) 
                            // Unsafe.Pointer is a basic type that includes nil.
                            if (!hasNil(target))
                            {
                                goto Error;
                            }

                        else 
                            goto Error;
                        
                    }

                    break;
                case ptr<Interface> t:
                    if (x.isNil())
                    {
                        target = Typ[UntypedNil];
                    }
                    else
                    { 
                        // cannot assign untyped values to non-empty interfaces
                        check.completeInterface(t);
                        if (!t.Empty())
                        {
                            goto Error;
                        }

                        target = Default(x.typ);

                    }

                    break;
                case ptr<Pointer> t:
                    if (!x.isNil())
                    {
                        goto Error;
                    } 
                    // keep nil untyped - see comment for interfaces, above
                    target = Typ[UntypedNil];
                    break;
                case ptr<Signature> t:
                    if (!x.isNil())
                    {
                        goto Error;
                    } 
                    // keep nil untyped - see comment for interfaces, above
                    target = Typ[UntypedNil];
                    break;
                case ptr<Slice> t:
                    if (!x.isNil())
                    {
                        goto Error;
                    } 
                    // keep nil untyped - see comment for interfaces, above
                    target = Typ[UntypedNil];
                    break;
                case ptr<Map> t:
                    if (!x.isNil())
                    {
                        goto Error;
                    } 
                    // keep nil untyped - see comment for interfaces, above
                    target = Typ[UntypedNil];
                    break;
                case ptr<Chan> t:
                    if (!x.isNil())
                    {
                        goto Error;
                    } 
                    // keep nil untyped - see comment for interfaces, above
                    target = Typ[UntypedNil];
                    break;
                default:
                {
                    var t = target.Underlying().type();
                    goto Error;
                    break;
                }

            }

            x.typ = target;
            check.updateExprType(x.expr, target, true); // UntypedNils are final
            return ;

Error:
            check.errorf(x.pos(), "cannot convert %s to %s", x, target);
            x.mode = invalid;

        }

        private static void comparison(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<operand> _addr_y, token.Token op)
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;
            ref operand y = ref _addr_y.val;
 
            // spec: "In any comparison, the first operand must be assignable
            // to the type of the second operand, or vice versa."
            @string err = "";
            if (x.assignableTo(check, y.typ, null) || y.assignableTo(check, x.typ, null))
            {
                var defined = false;

                if (op == token.EQL || op == token.NEQ) 
                    // spec: "The equality operators == and != apply to operands that are comparable."
                    defined = Comparable(x.typ) && Comparable(y.typ) || x.isNil() && hasNil(y.typ) || y.isNil() && hasNil(x.typ);
                else if (op == token.LSS || op == token.LEQ || op == token.GTR || op == token.GEQ) 
                    // spec: The ordering operators <, <=, >, and >= apply to operands that are ordered."
                    defined = isOrdered(x.typ) && isOrdered(y.typ);
                else 
                    unreachable();
                                if (!defined)
                {
                    var typ = x.typ;
                    if (x.isNil())
                    {
                        typ = y.typ;
                    }

                    err = check.sprintf("operator %s not defined for %s", op, typ);

                }

            }
            else
            {
                err = check.sprintf("mismatched types %s and %s", x.typ, y.typ);
            }

            if (err != "")
            {
                check.errorf(x.pos(), "cannot compare %s %s %s (%s)", x.expr, op, y.expr, err);
                x.mode = invalid;
                return ;
            }

            if (x.mode == constant_ && y.mode == constant_)
            {
                x.val = constant.MakeBool(constant.Compare(x.val, op, y.val)); 
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

            // spec: "Comparison operators compare two operands and yield
            //        an untyped boolean value."
            x.typ = Typ[UntypedBool];

        }

        private static void shift(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<operand> _addr_y, ptr<ast.BinaryExpr> _addr_e, token.Token op)
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;
            ref operand y = ref _addr_y.val;
            ref ast.BinaryExpr e = ref _addr_e.val;

            var untypedx = isUntyped(x.typ);

            constant.Value xval = default;
            if (x.mode == constant_)
            {
                xval = constant.ToInt(x.val);
            }

            if (isInteger(x.typ) || untypedx && xval != null && xval.Kind() == constant.Int)
            { 
                // The lhs is of integer type or an untyped constant representable
                // as an integer. Nothing to do.
            }
            else
            { 
                // shift has no chance
                check.invalidOp(x.pos(), "shifted operand %s must be integer", x);
                x.mode = invalid;
                return ;

            } 

            // spec: "The right operand in a shift expression must have integer type
            // or be an untyped constant representable by a value of type uint."

            if (isInteger(y.typ))             else if (isUntyped(y.typ)) 
                check.convertUntyped(y, Typ[Uint]);
                if (y.mode == invalid)
                {
                    x.mode = invalid;
                    return ;
                }

            else 
                check.invalidOp(y.pos(), "shift count %s must be integer", y);
                x.mode = invalid;
                return ;
                        constant.Value yval = default;
            if (y.mode == constant_)
            { 
                // rhs must be an integer value
                // (Either it was of an integer type already, or it was
                // untyped and successfully converted to a uint above.)
                yval = constant.ToInt(y.val);
                assert(yval.Kind() == constant.Int);
                if (constant.Sign(yval) < 0L)
                {
                    check.invalidOp(y.pos(), "negative shift count %s", y);
                    x.mode = invalid;
                    return ;
                }

            }

            if (x.mode == constant_)
            {
                if (y.mode == constant_)
                { 
                    // rhs must be within reasonable bounds in constant shifts
                    const long shiftBound = (long)1023L - 1L + 52L; // so we can express smallestFloat64
 // so we can express smallestFloat64
                    var (s, ok) = constant.Uint64Val(yval);
                    if (!ok || s > shiftBound)
                    {
                        check.invalidOp(y.pos(), "invalid shift count %s", y);
                        x.mode = invalid;
                        return ;
                    } 
                    // The lhs is representable as an integer but may not be an integer
                    // (e.g., 2.0, an untyped float) - this can only happen for untyped
                    // non-integer numeric constants. Correct the type so that the shift
                    // result is of integer type.
                    if (!isInteger(x.typ))
                    {
                        x.typ = Typ[UntypedInt];
                    } 
                    // x is a constant so xval != nil and it must be of Int kind.
                    x.val = constant.Shift(xval, op, uint(s)); 
                    // Typed constants must be representable in
                    // their type after each constant operation.
                    if (isTyped(x.typ))
                    {
                        if (e != null)
                        {
                            x.expr = e; // for better error message
                        }

                        check.representable(x, x.typ.Underlying()._<ptr<Basic>>());

                    }

                    return ;

                } 

                // non-constant shift with constant lhs
                if (untypedx)
                { 
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

                        if (found)
                        {
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

            // non-constant shift - lhs must be an integer
            if (!isInteger(x.typ))
            {
                check.invalidOp(x.pos(), "shifted operand %s must be integer", x);
                x.mode = invalid;
                return ;
            }

            x.mode = value;

        }

        private static opPredicates binaryOpPredicates = new opPredicates(token.ADD:func(typType)bool{returnisNumeric(typ)||isString(typ)},token.SUB:isNumeric,token.MUL:isNumeric,token.QUO:isNumeric,token.REM:isInteger,token.AND:isInteger,token.OR:isInteger,token.XOR:isInteger,token.AND_NOT:isInteger,token.LAND:isBoolean,token.LOR:isBoolean,);

        // The binary expression e may be nil. It's passed in for better error messages only.
        private static void binary(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<ast.BinaryExpr> _addr_e, ast.Expr lhs, ast.Expr rhs, token.Token op)
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;
            ref ast.BinaryExpr e = ref _addr_e.val;

            ref operand y = ref heap(out ptr<operand> _addr_y);

            check.expr(x, lhs);
            check.expr(_addr_y, rhs);

            if (x.mode == invalid)
            {
                return ;
            }

            if (y.mode == invalid)
            {
                x.mode = invalid;
                x.expr = y.expr;
                return ;
            }

            if (isShift(op))
            {
                check.shift(x, _addr_y, e, op);
                return ;
            }

            check.convertUntyped(x, y.typ);
            if (x.mode == invalid)
            {
                return ;
            }

            check.convertUntyped(_addr_y, x.typ);
            if (y.mode == invalid)
            {
                x.mode = invalid;
                return ;
            }

            if (isComparison(op))
            {
                check.comparison(x, _addr_y, op);
                return ;
            }

            if (!check.identical(x.typ, y.typ))
            { 
                // only report an error if we have valid types
                // (otherwise we had an error reported elsewhere already)
                if (x.typ != Typ[Invalid] && y.typ != Typ[Invalid])
                {
                    check.invalidOp(x.pos(), "mismatched types %s and %s", x.typ, y.typ);
                }

                x.mode = invalid;
                return ;

            }

            if (!check.op(binaryOpPredicates, x, op))
            {
                x.mode = invalid;
                return ;
            }

            if (op == token.QUO || op == token.REM)
            { 
                // check for zero divisor
                if ((x.mode == constant_ || isInteger(x.typ)) && y.mode == constant_ && constant.Sign(y.val) == 0L)
                {
                    check.invalidOp(y.pos(), "division by zero");
                    x.mode = invalid;
                    return ;
                } 

                // check for divisor underflow in complex division (see issue 20227)
                if (x.mode == constant_ && y.mode == constant_ && isComplex(x.typ))
                {
                    var re = constant.Real(y.val);
                    var im = constant.Imag(y.val);
                    var re2 = constant.BinaryOp(re, token.MUL, re);
                    var im2 = constant.BinaryOp(im, token.MUL, im);
                    if (constant.Sign(re2) == 0L && constant.Sign(im2) == 0L)
                    {
                        check.invalidOp(y.pos(), "division by zero");
                        x.mode = invalid;
                        return ;
                    }

                }

            }

            if (x.mode == constant_ && y.mode == constant_)
            {
                var xval = x.val;
                var yval = y.val;
                ptr<Basic> typ = x.typ.Underlying()._<ptr<Basic>>(); 
                // force integer division of integer operands
                if (op == token.QUO && isInteger(typ))
                {
                    op = token.QUO_ASSIGN;
                }

                x.val = constant.BinaryOp(xval, op, yval); 
                // Typed constants must be representable in
                // their type after each constant operation.
                if (isTyped(typ))
                {
                    if (e != null)
                    {
                        x.expr = e; // for better error message
                    }

                    check.representable(x, typ);

                }

                return ;

            }

            x.mode = value; 
            // x.typ is unchanged
        }

        // index checks an index expression for validity.
        // If max >= 0, it is the upper bound for index.
        // If the result typ is != Typ[Invalid], index is valid and typ is its (possibly named) integer type.
        // If the result val >= 0, index is valid and val is its constant int value.
        private static (Type, long) index(this ptr<Checker> _addr_check, ast.Expr index, long max)
        {
            Type typ = default;
            long val = default;
            ref Checker check = ref _addr_check.val;

            typ = Typ[Invalid];
            val = -1L;

            ref operand x = ref heap(out ptr<operand> _addr_x);
            check.expr(_addr_x, index);
            if (x.mode == invalid)
            {
                return ;
            } 

            // an untyped constant must be representable as Int
            check.convertUntyped(_addr_x, Typ[Int]);
            if (x.mode == invalid)
            {
                return ;
            } 

            // the index must be of integer type
            if (!isInteger(x.typ))
            {
                check.invalidArg(x.pos(), "index %s must be integer", _addr_x);
                return ;
            }

            if (x.mode != constant_)
            {
                return (x.typ, -1L);
            } 

            // a constant index i must be in bounds
            if (constant.Sign(x.val) < 0L)
            {
                check.invalidArg(x.pos(), "index %s must not be negative", _addr_x);
                return ;
            }

            var (v, valid) = constant.Int64Val(constant.ToInt(x.val));
            if (!valid || max >= 0L && v >= max)
            {
                check.errorf(x.pos(), "index %s is out of bounds", _addr_x);
                return ;
            } 

            // 0 <= v [ && v < max ]
            return (Typ[Int], v);

        }

        // indexElts checks the elements (elts) of an array or slice composite literal
        // against the literal's element type (typ), and the element indices against
        // the literal length if known (length >= 0). It returns the length of the
        // literal (maximum index value + 1).
        //
        private static long indexedElts(this ptr<Checker> _addr_check, slice<ast.Expr> elts, Type typ, long length)
        {
            ref Checker check = ref _addr_check.val;

            var visited = make_map<long, bool>(len(elts));
            long index = default;            long max = default;

            foreach (var (_, e) in elts)
            { 
                // determine and check index
                var validIndex = false;
                var eval = e;
                {
                    ptr<ast.KeyValueExpr> (kv, _) = e._<ptr<ast.KeyValueExpr>>();

                    if (kv != null)
                    {
                        {
                            var (typ, i) = check.index(kv.Key, length);

                            if (typ != Typ[Invalid])
                            {
                                if (i >= 0L)
                                {
                                    index = i;
                                    validIndex = true;
                                }
                                else
                                {
                                    check.errorf(e.Pos(), "index %s must be integer constant", kv.Key);
                                }

                            }

                        }

                        eval = kv.Value;

                    }
                    else if (length >= 0L && index >= length)
                    {
                        check.errorf(e.Pos(), "index %d is out of bounds (>= %d)", index, length);
                    }
                    else
                    {
                        validIndex = true;
                    } 

                    // if we have a valid index, check for duplicate entries

                } 

                // if we have a valid index, check for duplicate entries
                if (validIndex)
                {
                    if (visited[index])
                    {
                        check.errorf(e.Pos(), "duplicate index %d in array or slice literal", index);
                    }

                    visited[index] = true;

                }

                index++;
                if (index > max)
                {
                    max = index;
                } 

                // check element against composite literal element type
                ref operand x = ref heap(out ptr<operand> _addr_x);
                check.exprWithHint(_addr_x, eval, typ);
                check.assignment(_addr_x, typ, "array or slice literal");

            }
            return max;

        }

        // exprKind describes the kind of an expression; the kind
        // determines if an expression is valid in 'statement context'.
        private partial struct exprKind // : long
        {
        }

        private static readonly exprKind conversion = (exprKind)iota;
        private static readonly var expression = 0;
        private static readonly var statement = 1;


        // rawExpr typechecks expression e and initializes x with the expression
        // value or type. If an error occurred, x.mode is set to invalid.
        // If hint != nil, it is the type of a composite literal element.
        //
        private static exprKind rawExpr(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ast.Expr e, Type hint) => func((defer, _, __) =>
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;

            if (trace)
            {
                check.trace(e.Pos(), "%s", e);
                check.indent++;
                defer(() =>
                {
                    check.indent--;
                    check.trace(e.Pos(), "=> %s", x);
                }());

            }

            var kind = check.exprInternal(x, e, hint); 

            // convert x into a user-friendly set of values
            // TODO(gri) this code can be simplified
            Type typ = default;
            constant.Value val = default;

            if (x.mode == invalid) 
                typ = Typ[Invalid];
            else if (x.mode == novalue) 
                typ = (Tuple.val)(null);
            else if (x.mode == constant_) 
                typ = x.typ;
                val = x.val;
            else 
                typ = x.typ;
                        assert(x.expr != null && typ != null);

            if (isUntyped(typ))
            { 
                // delay type and value recording until we know the type
                // or until the end of type checking
                check.rememberUntyped(x.expr, false, x.mode, typ._<ptr<Basic>>(), val);

            }
            else
            {
                check.recordTypeAndValue(e, x.mode, typ, val);
            }

            return kind;

        });

        // exprInternal contains the core of type checking of expressions.
        // Must only be called by rawExpr.
        //
        private static exprKind exprInternal(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ast.Expr e, Type hint) => func((_, panic, __) =>
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;
 
            // make sure x has a valid state in case of bailout
            // (was issue 5770)
            x.mode = invalid;
            x.typ = Typ[Invalid];

            switch (e.type())
            {
                case ptr<ast.BadExpr> e:
                    goto Error; // error was reported before
                    break;
                case ptr<ast.Ident> e:
                    check.ident(x, e, null, false);
                    break;
                case ptr<ast.Ellipsis> e:
                    check.error(e.Pos(), "invalid use of '...'");
                    goto Error;
                    break;
                case ptr<ast.BasicLit> e:
                    x.setConst(e.Kind, e.Value);
                    if (x.mode == invalid)
                    {
                        check.invalidAST(e.Pos(), "invalid literal %v", e.Value);
                        goto Error;
                    }

                    break;
                case ptr<ast.FuncLit> e:
                    {
                        ptr<Signature> (sig, ok) = check.typ(e.Type)._<ptr<Signature>>();

                        if (ok)
                        { 
                            // Anonymous functions are considered part of the
                            // init expression/func declaration which contains
                            // them: use existing package-level declaration info.
                            var decl = check.decl; // capture for use in closure below
                            var iota = check.iota; // capture for use in closure below (#22345)
                            // Don't type-check right away because the function may
                            // be part of a type definition to which the function
                            // body refers. Instead, type-check as soon as possible,
                            // but before the enclosing scope contents changes (#22992).
                            check.later(() =>
                            {
                                check.funcBody(decl, "<function literal>", sig, e.Body, iota);
                            }
                        else
);
                            x.mode = value;
                            x.typ = sig;

                        }                        {
                            check.invalidAST(e.Pos(), "invalid function literal %s", e);
                            goto Error;
                        }

                    }


                    break;
                case ptr<ast.CompositeLit> e:
                    Type typ = default;                    Type @base = default;




                    if (e.Type != null) 
                        // composite literal type present - use it
                        // [...]T array types may only appear with composite literals.
                        // Check for them here so we don't have to handle ... in general.
                        {
                            ptr<ast.ArrayType> (atyp, _) = e.Type._<ptr<ast.ArrayType>>();

                            if (atyp != null && atyp.Len != null)
                            {
                                {
                                    ptr<ast.Ellipsis> (ellip, _) = atyp.Len._<ptr<ast.Ellipsis>>();

                                    if (ellip != null && ellip.Elt == null)
                                    { 
                                        // We have an "open" [...]T array type.
                                        // Create a new ArrayType with unknown length (-1)
                                        // and finish setting it up after analyzing the literal.
                                        typ = addr(new Array(len:-1,elem:check.typ(atyp.Elt)));
                                        base = typ;
                                        break;

                                    }

                                }

                            }

                        }

                        typ = check.typ(e.Type);
                        base = typ;
                    else if (hint != null) 
                        // no composite literal type present - use hint (element type of enclosing type)
                        typ = hint;
                        base, _ = deref(typ.Underlying()); // *T implies &T{}
                    else 
                        // TODO(gri) provide better error messages depending on context
                        check.error(e.Pos(), "missing type in composite literal");
                        goto Error;
                                        switch (@base.Underlying().type())
                    {
                        case ptr<Struct> utyp:
                            if (len(e.Elts) == 0L)
                            {
                                break;
                            }

                            var fields = utyp.fields;
                            {
                                ptr<ast.KeyValueExpr> (_, ok) = e.Elts[0L]._<ptr<ast.KeyValueExpr>>();

                                if (ok)
                                { 
                                    // all elements must have keys
                                    var visited = make_slice<bool>(len(fields));
                                    {
                                        var e__prev1 = e;

                                        foreach (var (_, __e) in e.Elts)
                                        {
                                            e = __e;
                                            ptr<ast.KeyValueExpr> (kv, _) = e._<ptr<ast.KeyValueExpr>>();
                                            if (kv == null)
                                            {
                                                check.error(e.Pos(), "mixture of field:value and value elements in struct literal");
                                                continue;
                                            }

                                            ptr<ast.Ident> (key, _) = kv.Key._<ptr<ast.Ident>>(); 
                                            // do all possible checks early (before exiting due to errors)
                                            // so we don't drop information on the floor
                                            check.expr(x, kv.Value);
                                            if (key == null)
                                            {
                                                check.errorf(kv.Pos(), "invalid field name %s in struct literal", kv.Key);
                                                continue;
                                            }

                                            var i = fieldIndex(utyp.fields, check.pkg, key.Name);
                                            if (i < 0L)
                                            {
                                                check.errorf(kv.Pos(), "unknown field %s in struct literal", key.Name);
                                                continue;
                                            }

                                            var fld = fields[i];
                                            check.recordUse(key, fld);
                                            var etyp = fld.typ;
                                            check.assignment(x, etyp, "struct literal"); 
                                            // 0 <= i < len(fields)
                                            if (visited[i])
                                            {
                                                check.errorf(kv.Pos(), "duplicate field name %s in struct literal", key.Name);
                                                continue;
                                            }

                                            visited[i] = true;

                                        }
                                else

                                        e = e__prev1;
                                    }
                                }                                { 
                                    // no element must have a key
                                    {
                                        var i__prev1 = i;
                                        var e__prev1 = e;

                                        foreach (var (__i, __e) in e.Elts)
                                        {
                                            i = __i;
                                            e = __e;
                                            {
                                                ptr<ast.KeyValueExpr> kv__prev2 = kv;

                                                (kv, _) = e._<ptr<ast.KeyValueExpr>>();

                                                if (kv != null)
                                                {
                                                    check.error(kv.Pos(), "mixture of field:value and value elements in struct literal");
                                                    continue;
                                                }

                                                kv = kv__prev2;

                                            }

                                            check.expr(x, e);
                                            if (i >= len(fields))
                                            {
                                                check.error(x.pos(), "too many values in struct literal");
                                                break; // cannot continue
                                            } 
                                            // i < len(fields)
                                            fld = fields[i];
                                            if (!fld.Exported() && fld.pkg != check.pkg)
                                            {
                                                check.errorf(x.pos(), "implicit assignment to unexported field %s in %s literal", fld.name, typ);
                                                continue;
                                            }

                                            etyp = fld.typ;
                                            check.assignment(x, etyp, "struct literal");

                                        }

                                        i = i__prev1;
                                        e = e__prev1;
                                    }

                                    if (len(e.Elts) < len(fields))
                                    {
                                        check.error(e.Rbrace, "too few values in struct literal"); 
                                        // ok to continue
                                    }

                                }

                            }


                            break;
                        case ptr<Array> utyp:
                            if (utyp.elem == null)
                            {
                                check.error(e.Pos(), "illegal cycle in type declaration");
                                goto Error;
                            }

                            var n = check.indexedElts(e.Elts, utyp.elem, utyp.len); 
                            // If we have an array of unknown length (usually [...]T arrays, but also
                            // arrays [n]T where n is invalid) set the length now that we know it and
                            // record the type for the array (usually done by check.typ which is not
                            // called for [...]T). We handle [...]T arrays and arrays with invalid
                            // length the same here because it makes sense to "guess" the length for
                            // the latter if we have a composite literal; e.g. for [n]int{1, 2, 3}
                            // where n is invalid for some reason, it seems fair to assume it should
                            // be 3 (see also Checked.arrayLength and issue #27346).
                            if (utyp.len < 0L)
                            {
                                utyp.len = n; 
                                // e.Type is missing if we have a composite literal element
                                // that is itself a composite literal with omitted type. In
                                // that case there is nothing to record (there is no type in
                                // the source at that point).
                                if (e.Type != null)
                                {
                                    check.recordTypeAndValue(e.Type, typexpr, utyp, null);
                                }

                            }

                            break;
                        case ptr<Slice> utyp:
                            if (utyp.elem == null)
                            {
                                check.error(e.Pos(), "illegal cycle in type declaration");
                                goto Error;
                            }

                            check.indexedElts(e.Elts, utyp.elem, -1L);
                            break;
                        case ptr<Map> utyp:
                            if (utyp.key == null || utyp.elem == null)
                            {
                                check.error(e.Pos(), "illegal cycle in type declaration");
                                goto Error;
                            }

                            visited = make(len(e.Elts));
                            {
                                var e__prev1 = e;

                                foreach (var (_, __e) in e.Elts)
                                {
                                    e = __e;
                                    (kv, _) = e._<ptr<ast.KeyValueExpr>>();
                                    if (kv == null)
                                    {
                                        check.error(e.Pos(), "missing key in map literal");
                                        continue;
                                    }

                                    check.exprWithHint(x, kv.Key, utyp.key);
                                    check.assignment(x, utyp.key, "map literal");
                                    if (x.mode == invalid)
                                    {
                                        continue;
                                    }

                                    if (x.mode == constant_)
                                    {
                                        var duplicate = false; 
                                        // if the key is of interface type, the type is also significant when checking for duplicates
                                        var xkey = keyVal(x.val);
                                        {
                                            (_, ok) = utyp.key.Underlying()._<ptr<Interface>>();

                                            if (ok)
                                            {
                                                foreach (var (_, vtyp) in visited[xkey])
                                                {
                                                    if (check.identical(vtyp, x.typ))
                                                    {
                                                        duplicate = true;
                                                        break;
                                                    }

                                                }
                                            else
                                                visited[xkey] = append(visited[xkey], x.typ);

                                            }                                            {
                                                _, duplicate = visited[xkey];
                                                visited[xkey] = null;
                                            }

                                        }

                                        if (duplicate)
                                        {
                                            check.errorf(x.pos(), "duplicate key %s in map literal", x.val);
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
                            var utyp = @base.Underlying().type();
                            {
                                var e__prev1 = e;

                                foreach (var (_, __e) in e.Elts)
                                {
                                    e = __e;
                                    {
                                        ptr<ast.KeyValueExpr> kv__prev1 = kv;

                                        (kv, _) = e._<ptr<ast.KeyValueExpr>>();

                                        if (kv != null)
                                        { 
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

                            if (utyp != Typ[Invalid])
                            {
                                check.errorf(e.Pos(), "invalid composite literal type %s", typ);
                                goto Error;
                            }

                            break;
                        }

                    }

                    x.mode = value;
                    x.typ = typ;
                    break;
                case ptr<ast.ParenExpr> e:
                    var kind = check.rawExpr(x, e.X, null);
                    x.expr = e;
                    return kind;
                    break;
                case ptr<ast.SelectorExpr> e:
                    check.selector(x, e);
                    break;
                case ptr<ast.IndexExpr> e:
                    check.expr(x, e.X);
                    if (x.mode == invalid)
                    {
                        check.use(e.Index);
                        goto Error;
                    }

                    var valid = false;
                    var length = int64(-1L); // valid if >= 0
                    switch (x.typ.Underlying().type())
                    {
                        case ptr<Basic> typ:
                            if (isString(typ))
                            {
                                valid = true;
                                if (x.mode == constant_)
                                {
                                    length = int64(len(constant.StringVal(x.val)));
                                } 
                                // an indexed string always yields a byte value
                                // (not a constant) even if the string and the
                                // index are constant
                                x.mode = value;
                                x.typ = universeByte; // use 'byte' name
                            }

                            break;
                        case ptr<Array> typ:
                            valid = true;
                            length = typ.len;
                            if (x.mode != variable)
                            {
                                x.mode = value;
                            }

                            x.typ = typ.elem;
                            break;
                        case ptr<Pointer> typ:
                            {
                                Type typ__prev1 = typ;

                                ptr<Array> (typ, _) = typ.@base.Underlying()._<ptr<Array>>();

                                if (typ != null)
                                {
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
                            ref operand key = ref heap(out ptr<operand> _addr_key);
                            check.expr(_addr_key, e.Index);
                            check.assignment(_addr_key, typ.key, "map index");
                            if (x.mode == invalid)
                            {
                                goto Error;
                            }

                            x.mode = mapindex;
                            x.typ = typ.elem;
                            x.expr = e;
                            return expression;
                            break;

                    }

                    if (!valid)
                    {
                        check.invalidOp(x.pos(), "cannot index %s", x);
                        goto Error;
                    }

                    if (e.Index == null)
                    {
                        check.invalidAST(e.Pos(), "missing index for %s", x);
                        goto Error;
                    }

                    check.index(e.Index, length); 
                    // ok to continue
                    break;
                case ptr<ast.SliceExpr> e:
                    check.expr(x, e.X);
                    if (x.mode == invalid)
                    {
                        check.use(e.Low, e.High, e.Max);
                        goto Error;
                    }

                    valid = false;
                    length = int64(-1L); // valid if >= 0
                    switch (x.typ.Underlying().type())
                    {
                        case ptr<Basic> typ:
                            if (isString(typ))
                            {
                                if (e.Slice3)
                                {
                                    check.invalidOp(x.pos(), "3-index slice of string");
                                    goto Error;
                                }

                                valid = true;
                                if (x.mode == constant_)
                                {
                                    length = int64(len(constant.StringVal(x.val)));
                                } 
                                // spec: "For untyped string operands the result
                                // is a non-constant value of type string."
                                if (typ.kind == UntypedString)
                                {
                                    x.typ = Typ[String];
                                }

                            }

                            break;
                        case ptr<Array> typ:
                            valid = true;
                            length = typ.len;
                            if (x.mode != variable)
                            {
                                check.invalidOp(x.pos(), "cannot slice %s (value not addressable)", x);
                                goto Error;
                            }

                            x.typ = addr(new Slice(elem:typ.elem));
                            break;
                        case ptr<Pointer> typ:
                            {
                                Type typ__prev1 = typ;

                                (typ, _) = typ.@base.Underlying()._<ptr<Array>>();

                                if (typ != null)
                                {
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

                    }

                    if (!valid)
                    {
                        check.invalidOp(x.pos(), "cannot slice %s", x);
                        goto Error;
                    }

                    x.mode = value; 

                    // spec: "Only the first index may be omitted; it defaults to 0."
                    if (e.Slice3 && (e.High == null || e.Max == null))
                    {
                        check.error(e.Rbrack, "2nd and 3rd index required in 3-index slice");
                        goto Error;
                    } 

                    // check indices
                    array<long> ind = new array<long>(3L);
                    {
                        var i__prev1 = i;

                        foreach (var (__i, __expr) in new slice<ast.Expr>(new ast.Expr[] { e.Low, e.High, e.Max }))
                        {
                            i = __i;
                            expr = __expr;
                            var x = int64(-1L);

                            if (expr != null) 
                                // The "capacity" is only known statically for strings, arrays,
                                // and pointers to arrays, and it is the same as the length for
                                // those types.
                                var max = int64(-1L);
                                if (length >= 0L)
                                {
                                    max = length + 1L;
                                }

                                {
                                    var (_, v) = check.index(expr, max);

                                    if (v >= 0L)
                                    {
                                        x = v;
                                    }

                                }

                            else if (i == 0L) 
                                // default is 0 for the first index
                                x = 0L;
                            else if (length >= 0L) 
                                // default is length (== capacity) otherwise
                                x = length;
                                                        ind[i] = x;

                        } 

                        // constant indices must be in range
                        // (check.index already checks that existing indices >= 0)

                        i = i__prev1;
                    }

L:

                    {
                        var i__prev1 = i;
                        var x__prev1 = x;

                        foreach (var (__i, __x) in ind[..len(ind) - 1L])
                        {
                            i = __i;
                            x = __x;
                            if (x > 0L)
                            {
                                foreach (var (_, y) in ind[i + 1L..])
                                {
                                    if (y >= 0L && x > y)
                                    {
                                        check.errorf(e.Rbrack, "invalid slice indices: %d > %d", x, y);
                                        _breakL = true; // only report one error, ok to continue
                                        break;
                                    }

                                }

                            }

                        }

                        i = i__prev1;
                        x = x__prev1;
                    }
                    break;
                case ptr<ast.TypeAssertExpr> e:
                    check.expr(x, e.X);
                    if (x.mode == invalid)
                    {
                        goto Error;
                    }

                    ptr<Interface> (xtyp, _) = x.typ.Underlying()._<ptr<Interface>>();
                    if (xtyp == null)
                    {
                        check.invalidOp(x.pos(), "%s is not an interface", x);
                        goto Error;
                    } 
                    // x.(type) expressions are handled explicitly in type switches
                    if (e.Type == null)
                    {
                        check.invalidAST(e.Pos(), "use of .(type) outside type switch");
                        goto Error;
                    }

                    var T = check.typ(e.Type);
                    if (T == Typ[Invalid])
                    {
                        goto Error;
                    }

                    check.typeAssertion(x.pos(), x, xtyp, T);
                    x.mode = commaok;
                    x.typ = T;
                    break;
                case ptr<ast.CallExpr> e:
                    return check.call(x, e);
                    break;
                case ptr<ast.StarExpr> e:
                    check.exprOrType(x, e.X);

                    if (x.mode == invalid) 
                        goto Error;
                    else if (x.mode == typexpr) 
                        x.typ = addr(new Pointer(base:x.typ));
                    else 
                        {
                            Type typ__prev1 = typ;

                            ptr<Pointer> (typ, ok) = x.typ.Underlying()._<ptr<Pointer>>();

                            if (ok)
                            {
                                x.mode = variable;
                                x.typ = typ.@base;
                            }
                            else
                            {
                                check.invalidOp(x.pos(), "cannot indirect %s", x);
                                goto Error;
                            }

                            typ = typ__prev1;

                        }

                                        break;
                case ptr<ast.UnaryExpr> e:
                    check.expr(x, e.X);
                    if (x.mode == invalid)
                    {
                        goto Error;
                    }

                    check.unary(x, e, e.Op);
                    if (x.mode == invalid)
                    {
                        goto Error;
                    }

                    if (e.Op == token.ARROW)
                    {
                        x.expr = e;
                        return statement; // receive operations may appear in statement context
                    }

                    break;
                case ptr<ast.BinaryExpr> e:
                    check.binary(x, e, e.X, e.Y, e.Op);
                    if (x.mode == invalid)
                    {
                        goto Error;
                    }

                    break;
                case ptr<ast.KeyValueExpr> e:
                    check.invalidAST(e.Pos(), "no key:value expected");
                    goto Error;
                    break;
                case ptr<ast.ArrayType> e:
                    x.mode = typexpr;
                    x.typ = check.typ(e); 
                    // Note: rawExpr (caller of exprInternal) will call check.recordTypeAndValue
                    // even though check.typ has already called it. This is fine as both
                    // times the same expression and type are recorded. It is also not a
                    // performance issue because we only reach here for composite literal
                    // types, which are comparatively rare.
                    break;
                case ptr<ast.StructType> e:
                    x.mode = typexpr;
                    x.typ = check.typ(e); 
                    // Note: rawExpr (caller of exprInternal) will call check.recordTypeAndValue
                    // even though check.typ has already called it. This is fine as both
                    // times the same expression and type are recorded. It is also not a
                    // performance issue because we only reach here for composite literal
                    // types, which are comparatively rare.
                    break;
                case ptr<ast.FuncType> e:
                    x.mode = typexpr;
                    x.typ = check.typ(e); 
                    // Note: rawExpr (caller of exprInternal) will call check.recordTypeAndValue
                    // even though check.typ has already called it. This is fine as both
                    // times the same expression and type are recorded. It is also not a
                    // performance issue because we only reach here for composite literal
                    // types, which are comparatively rare.
                    break;
                case ptr<ast.InterfaceType> e:
                    x.mode = typexpr;
                    x.typ = check.typ(e); 
                    // Note: rawExpr (caller of exprInternal) will call check.recordTypeAndValue
                    // even though check.typ has already called it. This is fine as both
                    // times the same expression and type are recorded. It is also not a
                    // performance issue because we only reach here for composite literal
                    // types, which are comparatively rare.
                    break;
                case ptr<ast.MapType> e:
                    x.mode = typexpr;
                    x.typ = check.typ(e); 
                    // Note: rawExpr (caller of exprInternal) will call check.recordTypeAndValue
                    // even though check.typ has already called it. This is fine as both
                    // times the same expression and type are recorded. It is also not a
                    // performance issue because we only reach here for composite literal
                    // types, which are comparatively rare.
                    break;
                case ptr<ast.ChanType> e:
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
                    panic(fmt.Sprintf("%s: unknown expression type %T", check.fset.Position(e.Pos()), e));
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

        private static void keyVal(constant.Value x)
        {

            if (x.Kind() == constant.Bool) 
                return constant.BoolVal(x);
            else if (x.Kind() == constant.String) 
                return constant.StringVal(x);
            else if (x.Kind() == constant.Int) 
                {
                    var v__prev1 = v;

                    var (v, ok) = constant.Int64Val(x);

                    if (ok)
                    {
                        return v;
                    }

                    v = v__prev1;

                }

                {
                    var v__prev1 = v;

                    (v, ok) = constant.Uint64Val(x);

                    if (ok)
                    {
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
        private static void typeAssertion(this ptr<Checker> _addr_check, token.Pos pos, ptr<operand> _addr_x, ptr<Interface> _addr_xtyp, Type T)
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;
            ref Interface xtyp = ref _addr_xtyp.val;

            var (method, wrongType) = check.assertableTo(xtyp, T);
            if (method == null)
            {
                return ;
            }

            @string msg = default;
            if (wrongType != null)
            {
                if (check.identical(method.typ, wrongType.typ))
                {
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

            check.errorf(pos, "%s cannot have dynamic type %s (%s)", x, T, msg);

        }

        private static void singleValue(this ptr<Checker> _addr_check, ptr<operand> _addr_x)
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;

            if (x.mode == value)
            { 
                // tuple types are never named - no need for underlying type below
                {
                    ptr<Tuple> (t, ok) = x.typ._<ptr<Tuple>>();

                    if (ok)
                    {
                        assert(t.Len() != 1L);
                        check.errorf(x.pos(), "%d-valued %s where single value is expected", t.Len(), x);
                        x.mode = invalid;
                    }

                }

            }

        }

        // expr typechecks expression e and initializes x with the expression value.
        // The result must be a single value.
        // If an error occurred, x.mode is set to invalid.
        //
        private static void expr(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ast.Expr e)
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;

            check.multiExpr(x, e);
            check.singleValue(x);
        }

        // multiExpr is like expr but the result may be a multi-value.
        private static void multiExpr(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ast.Expr e)
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;

            check.rawExpr(x, e, null);
            @string msg = default;

            if (x.mode == novalue) 
                msg = "%s used as value";
            else if (x.mode == builtin) 
                msg = "%s must be called";
            else if (x.mode == typexpr) 
                msg = "%s is not an expression";
            else 
                return ;
                        check.errorf(x.pos(), msg, x);
            x.mode = invalid;

        }

        // exprWithHint typechecks expression e and initializes x with the expression value;
        // hint is the type of a composite literal element.
        // If an error occurred, x.mode is set to invalid.
        //
        private static void exprWithHint(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ast.Expr e, Type hint)
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;

            assert(hint != null);
            check.rawExpr(x, e, hint);
            check.singleValue(x);
            @string msg = default;

            if (x.mode == novalue) 
                msg = "%s used as value";
            else if (x.mode == builtin) 
                msg = "%s must be called";
            else if (x.mode == typexpr) 
                msg = "%s is not an expression";
            else 
                return ;
                        check.errorf(x.pos(), msg, x);
            x.mode = invalid;

        }

        // exprOrType typechecks expression or type e and initializes x with the expression value or type.
        // If an error occurred, x.mode is set to invalid.
        //
        private static void exprOrType(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ast.Expr e)
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;

            check.rawExpr(x, e, null);
            check.singleValue(x);
            if (x.mode == novalue)
            {
                check.errorf(x.pos(), "%s used as value or type", x);
                x.mode = invalid;
            }

        }
    }
}}
