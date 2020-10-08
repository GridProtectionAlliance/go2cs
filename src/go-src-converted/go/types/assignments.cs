// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements initialization and assignment checks.

// package types -- go2cs converted at 2020 October 08 04:02:57 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\assignments.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // assignment reports whether x can be assigned to a variable of type T,
        // if necessary by attempting to convert untyped values to the appropriate
        // type. context describes the context in which the assignment takes place.
        // Use T == nil to indicate assignment to an untyped blank identifier.
        // x.mode is set to invalid if the assignment failed.
        private static void assignment(this ptr<Checker> _addr_check, ptr<operand> _addr_x, Type T, @string context)
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;

            check.singleValue(x);


            if (x.mode == invalid) 
                return ; // error reported before
            else if (x.mode == constant_ || x.mode == variable || x.mode == mapindex || x.mode == value || x.mode == commaok || x.mode == commaerr)             else 
                unreachable();
                        if (isUntyped(x.typ))
            {
                var target = T; 
                // spec: "If an untyped constant is assigned to a variable of interface
                // type or the blank identifier, the constant is first converted to type
                // bool, rune, int, float64, complex128 or string respectively, depending
                // on whether the value is a boolean, rune, integer, floating-point, complex,
                // or string constant."
                if (T == null || IsInterface(T))
                {
                    if (T == null && x.typ == Typ[UntypedNil])
                    {
                        check.errorf(x.pos(), "use of untyped nil in %s", context);
                        x.mode = invalid;
                        return ;
                    }
                    target = Default(x.typ);

                }
                check.convertUntyped(x, target);
                if (x.mode == invalid)
                {
                    return ;
                }
            }
            if (T == null)
            {
                return ;
            }
            {
                ref @string reason = ref heap("", out ptr<@string> _addr_reason);

                if (!x.assignableTo(check, T, _addr_reason))
                {
                    if (reason != "")
                    {
                        check.errorf(x.pos(), "cannot use %s as %s value in %s: %s", x, T, context, reason);
                    }
                    else
                    {
                        check.errorf(x.pos(), "cannot use %s as %s value in %s", x, T, context);
                    }
                    x.mode = invalid;

                }
            }

        }

        private static void initConst(this ptr<Checker> _addr_check, ptr<Const> _addr_lhs, ptr<operand> _addr_x)
        {
            ref Checker check = ref _addr_check.val;
            ref Const lhs = ref _addr_lhs.val;
            ref operand x = ref _addr_x.val;

            if (x.mode == invalid || x.typ == Typ[Invalid] || lhs.typ == Typ[Invalid])
            {
                if (lhs.typ == null)
                {
                    lhs.typ = Typ[Invalid];
                }

                return ;

            } 

            // rhs must be a constant
            if (x.mode != constant_)
            {
                check.errorf(x.pos(), "%s is not constant", x);
                if (lhs.typ == null)
                {
                    lhs.typ = Typ[Invalid];
                }

                return ;

            }

            assert(isConstType(x.typ)); 

            // If the lhs doesn't have a type yet, use the type of x.
            if (lhs.typ == null)
            {
                lhs.typ = x.typ;
            }

            check.assignment(x, lhs.typ, "constant declaration");
            if (x.mode == invalid)
            {
                return ;
            }

            lhs.val = x.val;

        }

        private static Type initVar(this ptr<Checker> _addr_check, ptr<Var> _addr_lhs, ptr<operand> _addr_x, @string context)
        {
            ref Checker check = ref _addr_check.val;
            ref Var lhs = ref _addr_lhs.val;
            ref operand x = ref _addr_x.val;

            if (x.mode == invalid || x.typ == Typ[Invalid] || lhs.typ == Typ[Invalid])
            {
                if (lhs.typ == null)
                {
                    lhs.typ = Typ[Invalid];
                }

                return null;

            } 

            // If the lhs doesn't have a type yet, use the type of x.
            if (lhs.typ == null)
            {
                var typ = x.typ;
                if (isUntyped(typ))
                { 
                    // convert untyped types to default types
                    if (typ == Typ[UntypedNil])
                    {
                        check.errorf(x.pos(), "use of untyped nil in %s", context);
                        lhs.typ = Typ[Invalid];
                        return null;
                    }

                    typ = Default(typ);

                }

                lhs.typ = typ;

            }

            check.assignment(x, lhs.typ, context);
            if (x.mode == invalid)
            {
                return null;
            }

            return x.typ;

        }

        private static Type assignVar(this ptr<Checker> _addr_check, ast.Expr lhs, ptr<operand> _addr_x)
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;

            if (x.mode == invalid || x.typ == Typ[Invalid])
            {
                return null;
            } 

            // Determine if the lhs is a (possibly parenthesized) identifier.
            ptr<ast.Ident> (ident, _) = unparen(lhs)._<ptr<ast.Ident>>(); 

            // Don't evaluate lhs if it is the blank identifier.
            if (ident != null && ident.Name == "_")
            {
                check.recordDef(ident, null);
                check.assignment(x, null, "assignment to _ identifier");
                if (x.mode == invalid)
                {
                    return null;
                }

                return x.typ;

            } 

            // If the lhs is an identifier denoting a variable v, this assignment
            // is not a 'use' of v. Remember current value of v.used and restore
            // after evaluating the lhs via check.expr.
            ptr<Var> v;
            bool v_used = default;
            if (ident != null)
            {
                {
                    var obj = check.lookup(ident.Name);

                    if (obj != null)
                    { 
                        // It's ok to mark non-local variables, but ignore variables
                        // from other packages to avoid potential race conditions with
                        // dot-imported variables.
                        {
                            ptr<Var> (w, _) = obj._<ptr<Var>>();

                            if (w != null && w.pkg == check.pkg)
                            {
                                v = w;
                                v_used = v.used;
                            }

                        }

                    }

                }

            }

            ref operand z = ref heap(out ptr<operand> _addr_z);
            check.expr(_addr_z, lhs);
            if (v != null)
            {
                v.used = v_used; // restore v.used
            }

            if (z.mode == invalid || z.typ == Typ[Invalid])
            {
                return null;
            } 

            // spec: "Each left-hand side operand must be addressable, a map index
            // expression, or the blank identifier. Operands may be parenthesized."

            if (z.mode == invalid) 
                return null;
            else if (z.mode == variable || z.mode == mapindex)             else 
                {
                    ptr<ast.SelectorExpr> (sel, ok) = z.expr._<ptr<ast.SelectorExpr>>();

                    if (ok)
                    {
                        ref operand op = ref heap(out ptr<operand> _addr_op);
                        check.expr(_addr_op, sel.X);
                        if (op.mode == mapindex)
                        {
                            check.errorf(z.pos(), "cannot assign to struct field %s in map", ExprString(z.expr));
                            return null;
                        }

                    }

                }

                check.errorf(z.pos(), "cannot assign to %s", _addr_z);
                return null;
                        check.assignment(x, z.typ, "assignment");
            if (x.mode == invalid)
            {
                return null;
            }

            return x.typ;

        }

        // If returnPos is valid, initVars is called to type-check the assignment of
        // return expressions, and returnPos is the position of the return statement.
        private static void initVars(this ptr<Checker> _addr_check, slice<ptr<Var>> lhs, slice<ast.Expr> rhs, token.Pos returnPos)
        {
            ref Checker check = ref _addr_check.val;

            var l = len(lhs);
            var (get, r, commaOk) = unpack((x, i) =>
            {
                check.multiExpr(x, rhs[i]);
            }, len(rhs), l == 2L && !returnPos.IsValid());
            if (get == null || l != r)
            { 
                // invalidate lhs and use rhs
                foreach (var (_, obj) in lhs)
                {
                    if (obj.typ == null)
                    {
                        obj.typ = Typ[Invalid];
                    }

                }
                if (get == null)
                {
                    return ; // error reported by unpack
                }

                check.useGetter(get, r);
                if (returnPos.IsValid())
                {
                    check.errorf(returnPos, "wrong number of return values (want %d, got %d)", l, r);
                    return ;
                }

                check.errorf(rhs[0L].Pos(), "cannot initialize %d variables with %d values", l, r);
                return ;

            }

            @string context = "assignment";
            if (returnPos.IsValid())
            {
                context = "return statement";
            }

            ref operand x = ref heap(out ptr<operand> _addr_x);
            if (commaOk)
            {
                array<Type> a = new array<Type>(2L);
                {
                    var i__prev1 = i;

                    foreach (var (__i) in a)
                    {
                        i = __i;
                        get(_addr_x, i);
                        a[i] = check.initVar(lhs[i], _addr_x, context);
                    }

                    i = i__prev1;
                }

                check.recordCommaOkTypes(rhs[0L], a);
                return ;

            }

            {
                var i__prev1 = i;

                foreach (var (__i, __lhs) in lhs)
                {
                    i = __i;
                    lhs = __lhs;
                    get(_addr_x, i);
                    check.initVar(lhs, _addr_x, context);
                }

                i = i__prev1;
            }
        }

        private static void assignVars(this ptr<Checker> _addr_check, slice<ast.Expr> lhs, slice<ast.Expr> rhs)
        {
            ref Checker check = ref _addr_check.val;

            var l = len(lhs);
            var (get, r, commaOk) = unpack((x, i) =>
            {
                check.multiExpr(x, rhs[i]);
            }, len(rhs), l == 2L);
            if (get == null)
            {
                check.useLHS(lhs);
                return ; // error reported by unpack
            }

            if (l != r)
            {
                check.useGetter(get, r);
                check.errorf(rhs[0L].Pos(), "cannot assign %d values to %d variables", r, l);
                return ;
            }

            ref operand x = ref heap(out ptr<operand> _addr_x);
            if (commaOk)
            {
                array<Type> a = new array<Type>(2L);
                {
                    var i__prev1 = i;

                    foreach (var (__i) in a)
                    {
                        i = __i;
                        get(_addr_x, i);
                        a[i] = check.assignVar(lhs[i], _addr_x);
                    }

                    i = i__prev1;
                }

                check.recordCommaOkTypes(rhs[0L], a);
                return ;

            }

            {
                var i__prev1 = i;

                foreach (var (__i, __lhs) in lhs)
                {
                    i = __i;
                    lhs = __lhs;
                    get(_addr_x, i);
                    check.assignVar(lhs, _addr_x);
                }

                i = i__prev1;
            }
        }

        private static void shortVarDecl(this ptr<Checker> _addr_check, token.Pos pos, slice<ast.Expr> lhs, slice<ast.Expr> rhs)
        {
            ref Checker check = ref _addr_check.val;

            var top = len(check.delayed);
            var scope = check.scope; 

            // collect lhs variables
            slice<ptr<Var>> newVars = default;
            var lhsVars = make_slice<ptr<Var>>(len(lhs));
            foreach (var (i, lhs) in lhs)
            {
                ptr<Var> obj;
                {
                    ptr<ast.Ident> (ident, _) = lhs._<ptr<ast.Ident>>();

                    if (ident != null)
                    { 
                        // Use the correct obj if the ident is redeclared. The
                        // variable's scope starts after the declaration; so we
                        // must use Scope.Lookup here and call Scope.Insert
                        // (via check.declare) later.
                        var name = ident.Name;
                        {
                            var alt__prev2 = alt;

                            var alt = scope.Lookup(name);

                            if (alt != null)
                            { 
                                // redeclared object must be a variable
                                {
                                    var alt__prev3 = alt;

                                    ptr<Var> (alt, _) = alt._<ptr<Var>>();

                                    if (alt != null)
                                    {
                                        obj = alt;
                                    }
                                    else
                                    {
                                        check.errorf(lhs.Pos(), "cannot assign to %s", lhs);
                                    }

                                    alt = alt__prev3;

                                }

                                check.recordUse(ident, alt);

                            }
                            else
                            { 
                                // declare new variable, possibly a blank (_) variable
                                obj = NewVar(ident.Pos(), check.pkg, name, null);
                                if (name != "_")
                                {
                                    newVars = append(newVars, obj);
                                }

                                check.recordDef(ident, obj);

                            }

                            alt = alt__prev2;

                        }

                    }
                    else
                    {
                        check.useLHS(lhs);
                        check.errorf(lhs.Pos(), "cannot declare %s", lhs);
                    }

                }

                if (obj == null)
                {
                    obj = NewVar(lhs.Pos(), check.pkg, "_", null); // dummy variable
                }

                lhsVars[i] = obj;

            }
            check.initVars(lhsVars, rhs, token.NoPos); 

            // process function literals in rhs expressions before scope changes
            check.processDelayed(top); 

            // declare new variables
            if (len(newVars) > 0L)
            { 
                // spec: "The scope of a constant or variable identifier declared inside
                // a function begins at the end of the ConstSpec or VarSpec (ShortVarDecl
                // for short variable declarations) and ends at the end of the innermost
                // containing block."
                var scopePos = rhs[len(rhs) - 1L].End();
                {
                    ptr<Var> obj__prev1 = obj;

                    foreach (var (_, __obj) in newVars)
                    {
                        obj = __obj;
                        check.declare(scope, null, obj, scopePos); // recordObject already called
                    }
            else

                    obj = obj__prev1;
                }
            }            {
                check.softErrorf(pos, "no new variables on left side of :=");
            }

        }
    }
}}
