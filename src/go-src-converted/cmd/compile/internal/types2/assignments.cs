// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements initialization and assignment checks.

// package types2 -- go2cs converted at 2022 March 06 23:12:20 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\assignments.go
using syntax = go.cmd.compile.@internal.syntax_package;

namespace go.cmd.compile.@internal;

public static partial class types2_package {

    // assignment reports whether x can be assigned to a variable of type T,
    // if necessary by attempting to convert untyped values to the appropriate
    // type. context describes the context in which the assignment takes place.
    // Use T == nil to indicate assignment to an untyped blank identifier.
    // x.mode is set to invalid if the assignment failed.
private static void assignment(this ptr<Checker> _addr_check, ptr<operand> _addr_x, Type T, @string context) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;

    check.singleValue(x);


    if (x.mode == invalid) 
        return ; // error reported before
    else if (x.mode == constant_ || x.mode == variable || x.mode == mapindex || x.mode == value || x.mode == nilvalue || x.mode == commaok || x.mode == commaerr)     else 
        // we may get here because of other problems (issue #39634, crash 12)
        check.errorf(x, "cannot assign %s to %s in %s", x, T, context);
        return ;
        if (isUntyped(x.typ)) {
        var target = T; 
        // spec: "If an untyped constant is assigned to a variable of interface
        // type or the blank identifier, the constant is first converted to type
        // bool, rune, int, float64, complex128 or string respectively, depending
        // on whether the value is a boolean, rune, integer, floating-point,
        // complex, or string constant."
        if (x.isNil()) {
            if (T == null) {
                check.errorf(x, "use of untyped nil in %s", context);
                x.mode = invalid;
                return ;
            }
        }
        else if (T == null || IsInterface(T)) {
            target = Default(x.typ);
        }
        var (newType, val, code) = check.implicitTypeAndValue(x, target);
        if (code != 0) {
            var msg = check.sprintf("cannot use %s as %s value in %s", x, target, context);

            if (code == _TruncatedFloat) 
                msg += " (truncated)";
            else if (code == _NumericOverflow) 
                msg += " (overflows)";
                        check.error(x, msg);
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
    {
        var sig = asSignature(x.typ);

        if (sig != null && len(sig.tparams) > 0) {
            check.errorf(x, "cannot use generic function %s without instantiation in %s", x, context);
        }
    } 

    // spec: "If a left-hand side is the blank identifier, any typed or
    // non-constant value except for the predeclared identifier nil may
    // be assigned to it."
    if (T == null) {
        return ;
    }
    ref @string reason = ref heap("", out ptr<@string> _addr_reason);
    {
        var (ok, _) = x.assignableTo(check, T, _addr_reason);

        if (!ok) {
            if (check.conf.CompilerErrorMessages) {
                check.errorf(x, "incompatible type: cannot use %s as %s value", x, T);
            }
            else
 {
                if (reason != "") {
                    check.errorf(x, "cannot use %s as %s value in %s: %s", x, T, context, reason);
                }
                else
 {
                    check.errorf(x, "cannot use %s as %s value in %s", x, T, context);
                }
            }
            x.mode = invalid;

        }
    }

}

private static void initConst(this ptr<Checker> _addr_check, ptr<Const> _addr_lhs, ptr<operand> _addr_x) {
    ref Checker check = ref _addr_check.val;
    ref Const lhs = ref _addr_lhs.val;
    ref operand x = ref _addr_x.val;

    if (x.mode == invalid || x.typ == Typ[Invalid] || lhs.typ == Typ[Invalid]) {
        if (lhs.typ == null) {
            lhs.typ = Typ[Invalid];
        }
        return ;

    }
    if (x.mode != constant_) {
        check.errorf(x, "%s is not constant", x);
        if (lhs.typ == null) {
            lhs.typ = Typ[Invalid];
        }
        return ;

    }
    assert(isConstType(x.typ)); 

    // If the lhs doesn't have a type yet, use the type of x.
    if (lhs.typ == null) {
        lhs.typ = x.typ;
    }
    check.assignment(x, lhs.typ, "constant declaration");
    if (x.mode == invalid) {
        return ;
    }
    lhs.val = x.val;

}

private static Type initVar(this ptr<Checker> _addr_check, ptr<Var> _addr_lhs, ptr<operand> _addr_x, @string context) {
    ref Checker check = ref _addr_check.val;
    ref Var lhs = ref _addr_lhs.val;
    ref operand x = ref _addr_x.val;

    if (x.mode == invalid || x.typ == Typ[Invalid] || lhs.typ == Typ[Invalid]) {
        if (lhs.typ == null) {
            lhs.typ = Typ[Invalid];
        }
        lhs.used = true; // avoid follow-on "declared but not used" errors
        return null;

    }
    if (lhs.typ == null) {
        var typ = x.typ;
        if (isUntyped(typ)) { 
            // convert untyped types to default types
            if (typ == Typ[UntypedNil]) {
                check.errorf(x, "use of untyped nil in %s", context);
                lhs.typ = Typ[Invalid];
                return null;
            }

            typ = Default(typ);

        }
        lhs.typ = typ;

    }
    check.assignment(x, lhs.typ, context);
    if (x.mode == invalid) {
        return null;
    }
    return x.typ;

}

private static Type assignVar(this ptr<Checker> _addr_check, syntax.Expr lhs, ptr<operand> _addr_x) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;

    if (x.mode == invalid || x.typ == Typ[Invalid]) {
        check.useLHS(lhs);
        return null;
    }
    ptr<syntax.Name> (ident, _) = unparen(lhs)._<ptr<syntax.Name>>(); 

    // Don't evaluate lhs if it is the blank identifier.
    if (ident != null && ident.Value == "_") {
        check.recordDef(ident, null);
        check.assignment(x, null, "assignment to _ identifier");
        if (x.mode == invalid) {
            return null;
        }
        return x.typ;

    }
    ptr<Var> v;
    bool v_used = default;
    if (ident != null) {
        {
            var obj = check.lookup(ident.Value);

            if (obj != null) { 
                // It's ok to mark non-local variables, but ignore variables
                // from other packages to avoid potential race conditions with
                // dot-imported variables.
                {
                    ptr<Var> (w, _) = obj._<ptr<Var>>();

                    if (w != null && w.pkg == check.pkg) {
                        v = w;
                        v_used = v.used;
                    }

                }

            }

        }

    }
    ref operand z = ref heap(out ptr<operand> _addr_z);
    check.expr(_addr_z, lhs);
    if (v != null) {
        v.used = v_used; // restore v.used
    }
    if (z.mode == invalid || z.typ == Typ[Invalid]) {
        return null;
    }

    if (z.mode == invalid) 
        return null;
    else if (z.mode == variable || z.mode == mapindex)     else if (z.mode == nilvalue) 
        check.error(_addr_z, "cannot assign to nil"); // default would print "untyped nil"
        return null;
    else 
        {
            ptr<syntax.SelectorExpr> (sel, ok) = z.expr._<ptr<syntax.SelectorExpr>>();

            if (ok) {
                ref operand op = ref heap(out ptr<operand> _addr_op);
                check.expr(_addr_op, sel.X);
                if (op.mode == mapindex) {
                    check.errorf(_addr_z, "cannot assign to struct field %s in map", syntax.String(z.expr));
                    return null;
                }
            }

        }

        check.errorf(_addr_z, "cannot assign to %s", _addr_z);
        return null;
        check.assignment(x, z.typ, "assignment");
    if (x.mode == invalid) {
        return null;
    }
    return x.typ;

}

// If returnPos is valid, initVars is called to type-check the assignment of
// return expressions, and returnPos is the position of the return statement.
private static void initVars(this ptr<Checker> _addr_check, slice<ptr<Var>> lhs, slice<syntax.Expr> orig_rhs, syntax.Pos returnPos) {
    ref Checker check = ref _addr_check.val;

    var (rhs, commaOk) = check.exprList(orig_rhs, len(lhs) == 2 && !returnPos.IsKnown());

    if (len(lhs) != len(rhs)) { 
        // invalidate lhs
        foreach (var (_, obj) in lhs) {
            if (obj.typ == null) {
                obj.typ = Typ[Invalid];
            }
        }        foreach (var (_, x) in rhs) {
            if (x.mode == invalid) {
                return ;
            }
        }        if (returnPos.IsKnown()) {
            check.errorf(returnPos, "wrong number of return values (want %d, got %d)", len(lhs), len(rhs));
            return ;
        }
        check.errorf(rhs[0], "cannot initialize %d variables with %d values", len(lhs), len(rhs));
        return ;

    }
    @string context = "assignment";
    if (returnPos.IsKnown()) {
        context = "return statement";
    }
    if (commaOk) {
        array<Type> a = new array<Type>(2);
        {
            var i__prev1 = i;

            foreach (var (__i) in a) {
                i = __i;
                a[i] = check.initVar(lhs[i], rhs[i], context);
            }

            i = i__prev1;
        }

        check.recordCommaOkTypes(orig_rhs[0], a);
        return ;

    }
    {
        var i__prev1 = i;

        foreach (var (__i, __lhs) in lhs) {
            i = __i;
            lhs = __lhs;
            check.initVar(lhs, rhs[i], context);
        }
        i = i__prev1;
    }
}

private static void assignVars(this ptr<Checker> _addr_check, slice<syntax.Expr> lhs, slice<syntax.Expr> orig_rhs) {
    ref Checker check = ref _addr_check.val;

    var (rhs, commaOk) = check.exprList(orig_rhs, len(lhs) == 2);

    if (len(lhs) != len(rhs)) {
        check.useLHS(lhs); 
        // don't report an error if we already reported one
        foreach (var (_, x) in rhs) {
            if (x.mode == invalid) {
                return ;
            }
        }        check.errorf(rhs[0], "cannot assign %d values to %d variables", len(rhs), len(lhs));
        return ;

    }
    if (commaOk) {
        array<Type> a = new array<Type>(2);
        {
            var i__prev1 = i;

            foreach (var (__i) in a) {
                i = __i;
                a[i] = check.assignVar(lhs[i], rhs[i]);
            }

            i = i__prev1;
        }

        check.recordCommaOkTypes(orig_rhs[0], a);
        return ;

    }
    {
        var i__prev1 = i;

        foreach (var (__i, __lhs) in lhs) {
            i = __i;
            lhs = __lhs;
            check.assignVar(lhs, rhs[i]);
        }
        i = i__prev1;
    }
}

// unpack unpacks a *syntax.ListExpr into a list of syntax.Expr.
// Helper introduced for the go/types -> types2 port.
// TODO(gri) Should find a more efficient solution that doesn't
//           require introduction of a new slice for simple
//           expressions.
private static slice<syntax.Expr> unpackExpr(syntax.Expr x) {
    {
        ptr<syntax.ListExpr> (x, _) = x._<ptr<syntax.ListExpr>>();

        if (x != null) {
            return x.ElemList;
        }
    }

    if (x != null) {
        return new slice<syntax.Expr>(new syntax.Expr[] { x });
    }
    return null;

}

private static void shortVarDecl(this ptr<Checker> _addr_check, syntax.Pos pos, slice<syntax.Expr> lhs, slice<syntax.Expr> rhs) {
    ref Checker check = ref _addr_check.val;

    var top = len(check.delayed);
    var scope = check.scope; 

    // collect lhs variables
    var seen = make_map<@string, bool>(len(lhs));
    var lhsVars = make_slice<ptr<Var>>(len(lhs));
    var newVars = make_slice<ptr<Var>>(0, len(lhs));
    var hasErr = false;
    {
        var i__prev1 = i;

        foreach (var (__i, __lhs) in lhs) {
            i = __i;
            lhs = __lhs;
            ptr<syntax.Name> (ident, _) = lhs._<ptr<syntax.Name>>();
            if (ident == null) {
                check.useLHS(lhs);
                check.errorf(lhs, "non-name %s on left side of :=", lhs);
                hasErr = true;
                continue;
            }
            var name = ident.Value;
            if (name != "_") {
                if (seen[name]) {
                    check.errorf(lhs, "%s repeated on left side of :=", lhs);
                    hasErr = true;
                    continue;
                }
                seen[name] = true;
            } 

            // Use the correct obj if the ident is redeclared. The
            // variable's scope starts after the declaration; so we
            // must use Scope.Lookup here and call Scope.Insert
            // (via check.declare) later.
            {
                var alt = scope.Lookup(name);

                if (alt != null) {
                    check.recordUse(ident, alt); 
                    // redeclared object must be a variable
                    {
                        ptr<Var> obj__prev2 = obj;

                        ptr<Var> (obj, _) = alt._<ptr<Var>>();

                        if (obj != null) {
                            lhsVars[i] = obj;
                        }
                        else
 {
                            check.errorf(lhs, "cannot assign to %s", lhs);
                            hasErr = true;
                        }

                        obj = obj__prev2;

                    }

                    continue;

                } 

                // declare new variable

            } 

            // declare new variable
            var obj = NewVar(ident.Pos(), check.pkg, name, null);
            lhsVars[i] = obj;
            if (name != "_") {
                newVars = append(newVars, obj);
            }

            check.recordDef(ident, obj);

        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;
        ptr<Var> obj__prev1 = obj;

        foreach (var (__i, __obj) in lhsVars) {
            i = __i;
            obj = __obj;
            if (obj == null) {
                lhsVars[i] = NewVar(lhs[i].Pos(), check.pkg, "_", null);
            }
        }
        i = i__prev1;
        obj = obj__prev1;
    }

    check.initVars(lhsVars, rhs, nopos); 

    // process function literals in rhs expressions before scope changes
    check.processDelayed(top);

    if (len(newVars) == 0 && !hasErr) {
        check.softErrorf(pos, "no new variables on left side of :=");
        return ;
    }
    var scopePos = syntax.EndPos(rhs[len(rhs) - 1]);
    {
        ptr<Var> obj__prev1 = obj;

        foreach (var (_, __obj) in newVars) {
            obj = __obj;
            check.declare(scope, null, obj, scopePos); // id = nil: recordDef already called
        }
        obj = obj__prev1;
    }
}

} // end types2_package