// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2022 March 13 05:52:56 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Program Files\Go\src\go\types\decl.go
namespace go.go;

using fmt = fmt_package;
using ast = go.ast_package;
using constant = go.constant_package;
using typeparams = go.@internal.typeparams_package;
using token = go.token_package;
using System;

public static partial class types_package {

private static void reportAltDecl(this ptr<Checker> _addr_check, Object obj) {
    ref Checker check = ref _addr_check.val;

    {
        var pos = obj.Pos();

        if (pos.IsValid()) { 
            // We use "other" rather than "previous" here because
            // the first declaration seen may not be textually
            // earlier in the source.
            check.errorf(obj, _DuplicateDecl, "\tother declaration of %s", obj.Name()); // secondary error, \t indented
        }
    }
}

private static void declare(this ptr<Checker> _addr_check, ptr<Scope> _addr_scope, ptr<ast.Ident> _addr_id, Object obj, token.Pos pos) {
    ref Checker check = ref _addr_check.val;
    ref Scope scope = ref _addr_scope.val;
    ref ast.Ident id = ref _addr_id.val;
 
    // spec: "The blank identifier, represented by the underscore
    // character _, may be used in a declaration like any other
    // identifier but the declaration does not introduce a new
    // binding."
    if (obj.Name() != "_") {
        {
            var alt = scope.Insert(obj);

            if (alt != null) {
                check.errorf(obj, _DuplicateDecl, "%s redeclared in this block", obj.Name());
                check.reportAltDecl(alt);
                return ;
            }

        }
        obj.setScopePos(pos);
    }
    if (id != null) {
        check.recordDef(id, obj);
    }
}

// pathString returns a string of the form a->b-> ... ->g for a path [a, b, ... g].
private static @string pathString(slice<Object> path) {
    @string s = default;
    foreach (var (i, p) in path) {
        if (i > 0) {
            s += "->";
        }
        s += p.Name();
    }    return s;
}

// objDecl type-checks the declaration of obj in its respective (file) context.
// For the meaning of def, see Checker.definedType, in typexpr.go.
private static void objDecl(this ptr<Checker> _addr_check, Object obj, ptr<Named> _addr_def) => func((defer, _, _) => {
    ref Checker check = ref _addr_check.val;
    ref Named def = ref _addr_def.val;

    if (trace && obj.Type() == null) {
        if (check.indent == 0) {
            fmt.Println(); // empty line between top-level objects for readability
        }
        check.trace(obj.Pos(), "-- checking %s (%s, objPath = %s)", obj, obj.color(), pathString(check.objPath));
        check.indent++;
        defer(() => {
            check.indent--;
            check.trace(obj.Pos(), "=> %s (%s)", obj, obj.color());
        }());
    }
    if (obj.color() == white && obj.Type() != null) {
        obj.setColor(black);
        return ;
    }

    if (obj.color() == white) 
        assert(obj.Type() == null); 
        // All color values other than white and black are considered grey.
        // Because black and white are < grey, all values >= grey are grey.
        // Use those values to encode the object's index into the object path.
        obj.setColor(grey + color(check.push(obj)));
        defer(() => {
            check.pop().setColor(black);
        }());
    else if (obj.color() == black) 
        assert(obj.Type() != null);
        return ;
    else if (obj.color() == grey) 
        // We have a cycle.
        // In the existing code, this is marked by a non-nil type
        // for the object except for constants and variables whose
        // type may be non-nil (known), or nil if it depends on the
        // not-yet known initialization value.
        // In the former case, set the type to Typ[Invalid] because
        // we have an initialization cycle. The cycle error will be
        // reported later, when determining initialization order.
        // TODO(gri) Report cycle here and simplify initialization
        // order code.
        switch (obj.type()) {
            case ptr<Const> obj:
                if (check.cycle(obj) || obj.typ == null) {
                    obj.typ = Typ[Invalid];
                }
                break;
            case ptr<Var> obj:
                if (check.cycle(obj) || obj.typ == null) {
                    obj.typ = Typ[Invalid];
                }
                break;
            case ptr<TypeName> obj:
                if (check.cycle(obj)) { 
                    // break cycle
                    // (without this, calling underlying()
                    // below may lead to an endless loop
                    // if we have a cycle for a defined
                    // (*Named) type)
                    obj.typ = Typ[Invalid];
                }
                break;
            case ptr<Func> obj:
                if (check.cycle(obj)) { 
                    // Don't set obj.typ to Typ[Invalid] here
                    // because plenty of code type-asserts that
                    // functions have a *Signature type. Grey
                    // functions have their type set to an empty
                    // signature which makes it impossible to
                    // initialize a variable with the function.
                }
                break;
            default:
            {
                var obj = obj.type();
                unreachable();
                break;
            }
        }
        assert(obj.Type() != null);
        return ;
    else 
        // Color values other than white or black are considered grey.
        var d = check.objMap[obj];
    if (d == null) {
        check.dump("%v: %s should have been declared", obj.Pos(), obj);
        unreachable();
    }
    defer(ctxt => {
        check.context = ctxt;
    }(check.context));
    check.context = new context(scope:d.file,); 

    // Const and var declarations must not have initialization
    // cycles. We track them by remembering the current declaration
    // in check.decl. Initialization expressions depending on other
    // consts, vars, or functions, add dependencies to the current
    // check.decl.
    switch (obj.type()) {
        case ptr<Const> obj:
            check.decl = d; // new package-level const decl
            check.constDecl(obj, d.vtyp, d.init, d.inherited);
            break;
        case ptr<Var> obj:
            check.decl = d; // new package-level var decl
            check.varDecl(obj, d.lhs, d.vtyp, d.init);
            break;
        case ptr<TypeName> obj:
            check.typeDecl(obj, d.tdecl, def);
            check.collectMethods(obj); // methods can only be added to top-level types
            break;
        case ptr<Func> obj:
            check.funcDecl(obj, d);
            break;
        default:
        {
            var obj = obj.type();
            unreachable();
            break;
        }
    }
});

// cycle checks if the cycle starting with obj is valid and
// reports an error if it is not.
private static bool cycle(this ptr<Checker> _addr_check, Object obj) => func((defer, _, _) => {
    bool isCycle = default;
    ref Checker check = ref _addr_check.val;
 
    // The object map contains the package scope objects and the non-interface methods.
    if (debug) {
        var info = check.objMap[obj];
        var inObjMap = info != null && (info.fdecl == null || info.fdecl.Recv == null); // exclude methods
        var isPkgObj = obj.Parent() == check.pkg.scope;
        if (isPkgObj != inObjMap) {
            check.dump("%v: inconsistent object map for %s (isPkgObj = %v, inObjMap = %v)", obj.Pos(), obj, isPkgObj, inObjMap);
            unreachable();
        }
    }
    assert(obj.color() >= grey);
    var start = obj.color() - grey; // index of obj in objPath
    var cycle = check.objPath[(int)start..];
    nint nval = 0; // number of (constant or variable) values in the cycle
    nint ndef = 0; // number of type definitions in the cycle
    {
        var obj__prev1 = obj;

        foreach (var (_, __obj) in cycle) {
            obj = __obj;
            switch (obj.type()) {
                case ptr<Const> obj:
                    nval++;
                    break;
                case ptr<Var> obj:
                    nval++;
                    break;
                case ptr<TypeName> obj:
                    bool alias = default;
                    {
                        var d = check.objMap[obj];

                        if (d != null) {
                            alias = d.tdecl.Assign.IsValid(); // package-level object
                        }
                        else
 {
                            alias = obj.IsAlias(); // function local object
                        }

                    }
                    if (!alias) {
                        ndef++;
                    }
                    break;
                case ptr<Func> obj:
                    break;
                default:
                {
                    var obj = obj.type();
                    unreachable();
                    break;
                }
            }
        }
        obj = obj__prev1;
    }

    if (trace) {
        check.trace(obj.Pos(), "## cycle detected: objPath = %s->%s (len = %d)", pathString(cycle), obj.Name(), len(cycle));
        check.trace(obj.Pos(), "## cycle contains: %d values, %d type definitions", nval, ndef);
        defer(() => {
            if (isCycle) {
                check.trace(obj.Pos(), "=> error: cycle is invalid");
            }
        }());
    }
    if (nval == len(cycle)) {
        return false;
    }
    if (nval == 0 && ndef > 0) {
        return false; // cycle is permitted
    }
    check.cycleError(cycle);

    return true;
});

private partial struct typeInfo { // : nuint
}

// validType verifies that the given type does not "expand" infinitely
// producing a cycle in the type graph. Cycles are detected by marking
// defined types.
// (Cycles involving alias types, as in "type A = [10]A" are detected
// earlier, via the objDecl cycle detection mechanism.)
private static typeInfo validType(this ptr<Checker> _addr_check, Type typ, slice<Object> path) => func((_, panic, _) => {
    ref Checker check = ref _addr_check.val;

    const typeInfo unknown = iota;
    const var marked = 0;
    const var valid = 1;
    const var invalid = 2;

    switch (typ.type()) {
        case ptr<Array> t:
            return check.validType(t.elem, path);
            break;
        case ptr<Struct> t:
            foreach (var (_, f) in t.fields) {
                if (check.validType(f.typ, path) == invalid) {
                    return invalid;
                }
            }
            break;
        case ptr<Interface> t:
            foreach (var (_, etyp) in t.embeddeds) {
                if (check.validType(etyp, path) == invalid) {
                    return invalid;
                }
            }
            break;
        case ptr<Named> t:
            if (t.obj.pkg != check.pkg) {
                return valid;
            } 

            // don't report a 2nd error if we already know the type is invalid
            // (e.g., if a cycle was detected earlier, via under).
            if (t.underlying == Typ[Invalid]) {
                t.info = invalid;
                return invalid;
            }

            if (t.info == unknown) 
                t.info = marked;
                t.info = check.validType(t.orig, append(path, t.obj)); // only types of current package added to path
            else if (t.info == marked) 
                // cycle detected
                foreach (var (i, tn) in path) {
                    if (t.obj.pkg != check.pkg) {
                        panic("internal error: type cycle via package-external type");
                    }
                    if (tn == t.obj) {
                        check.cycleError(path[(int)i..]);
                        t.info = invalid;
                        return t.info;
                    }
                }
                panic("internal error: cycle start not found");
                        return t.info;
            break;
        case ptr<instance> t:
            return check.validType(t.expand(), path);
            break;

    }

    return valid;
});

// cycleError reports a declaration cycle starting with
// the object in cycle that is "first" in the source.
private static void cycleError(this ptr<Checker> _addr_check, slice<Object> cycle) {
    ref Checker check = ref _addr_check.val;
 
    // TODO(gri) Should we start with the last (rather than the first) object in the cycle
    //           since that is the earliest point in the source where we start seeing the
    //           cycle? That would be more consistent with other error messages.
    var i = firstInSrc(cycle);
    var obj = cycle[i];
    check.errorf(obj, _InvalidDeclCycle, "illegal cycle in declaration of %s", obj.Name());
    foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_1<< in cycle) {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_1<<
        check.errorf(obj, _InvalidDeclCycle, "\t%s refers to", obj.Name()); // secondary error, \t indented
        i++;
        if (i >= len(cycle)) {
            i = 0;
        }
        obj = cycle[i];
    }    check.errorf(obj, _InvalidDeclCycle, "\t%s", obj.Name());
}

// firstInSrc reports the index of the object with the "smallest"
// source position in path. path must not be empty.
private static nint firstInSrc(slice<Object> path) {
    nint fst = 0;
    var pos = path[0].Pos();
    foreach (var (i, t) in path[(int)1..]) {
        if (t.Pos() < pos) {
            (fst, pos) = (i + 1, t.Pos());
        }
    }    return fst;
}

private partial interface decl {
    ast.Node node();
}

private partial struct importDecl {
    public ptr<ast.ImportSpec> spec;
}
private partial struct constDecl {
    public ptr<ast.ValueSpec> spec;
    public nint iota;
    public ast.Expr typ;
    public slice<ast.Expr> init;
    public bool inherited;
}
private partial struct varDecl {
    public ptr<ast.ValueSpec> spec;
}
private partial struct typeDecl {
    public ptr<ast.TypeSpec> spec;
}
private partial struct funcDecl {
    public ptr<ast.FuncDecl> decl;
}private static ast.Node node(this importDecl d) {
    return d.spec;
}
private static ast.Node node(this constDecl d) {
    return d.spec;
}
private static ast.Node node(this varDecl d) {
    return d.spec;
}
private static ast.Node node(this typeDecl d) {
    return d.spec;
}
private static ast.Node node(this funcDecl d) {
    return d.decl;
}

private static void walkDecls(this ptr<Checker> _addr_check, slice<ast.Decl> decls, Action<decl> f) {
    ref Checker check = ref _addr_check.val;

    foreach (var (_, d) in decls) {
        check.walkDecl(d, f);
    }
}

private static void walkDecl(this ptr<Checker> _addr_check, ast.Decl d, Action<decl> f) {
    ref Checker check = ref _addr_check.val;

    switch (d.type()) {
        case ptr<ast.BadDecl> d:
            break;
        case ptr<ast.GenDecl> d:
            ptr<ast.ValueSpec> last; // last ValueSpec with type or init exprs seen
            {
                var s__prev1 = s;

                foreach (var (__iota, __s) in d.Specs) {
                    iota = __iota;
                    s = __s;
                    switch (s.type()) {
                        case ptr<ast.ImportSpec> s:
                            f(new importDecl(s));
                            break;
                        case ptr<ast.ValueSpec> s:

                            if (d.Tok == token.CONST) 
                                // determine which initialization expressions to use
                                var inherited = true;

                                if (s.Type != null || len(s.Values) > 0) 
                                    last = s;
                                    inherited = false;
                                else if (last == null) 
                                    last = @new<ast.ValueSpec>(); // make sure last exists
                                    inherited = false;
                                                                check.arityMatch(s, last);
                                f(new constDecl(spec:s,iota:iota,typ:last.Type,init:last.Values,inherited:inherited));
                            else if (d.Tok == token.VAR) 
                                check.arityMatch(s, null);
                                f(new varDecl(s));
                            else 
                                check.invalidAST(s, "invalid token %s", d.Tok);
                                                        break;
                        case ptr<ast.TypeSpec> s:
                            f(new typeDecl(s));
                            break;
                        default:
                        {
                            var s = s.type();
                            check.invalidAST(s, "unknown ast.Spec node %T", s);
                            break;
                        }
                    }
                }

                s = s__prev1;
            }
            break;
        case ptr<ast.FuncDecl> d:
            f(new funcDecl(d));
            break;
        default:
        {
            var d = d.type();
            check.invalidAST(d, "unknown ast.Decl node %T", d);
            break;
        }
    }
}

private static void constDecl(this ptr<Checker> _addr_check, ptr<Const> _addr_obj, ast.Expr typ, ast.Expr init, bool inherited) => func((defer, _, _) => {
    ref Checker check = ref _addr_check.val;
    ref Const obj = ref _addr_obj.val;

    assert(obj.typ == null); 

    // use the correct value of iota
    defer((iota, errpos) => {
        check.iota = iota;
        check.errpos = errpos;
    }(check.iota, check.errpos));
    check.iota = obj.val;
    check.errpos = null; 

    // provide valid constant value under all circumstances
    obj.val = constant.MakeUnknown(); 

    // determine type, if any
    if (typ != null) {
        var t = check.typ(typ);
        if (!isConstType(t)) { 
            // don't report an error if the type is an invalid C (defined) type
            // (issue #22090)
            if (under(t) != Typ[Invalid]) {
                check.errorf(typ, _InvalidConstType, "invalid constant type %s", t);
            }
            obj.typ = Typ[Invalid];
            return ;
        }
        obj.typ = t;
    }
    ref operand x = ref heap(out ptr<operand> _addr_x);
    if (init != null) {
        if (inherited) { 
            // The initialization expression is inherited from a previous
            // constant declaration, and (error) positions refer to that
            // expression and not the current constant declaration. Use
            // the constant identifier position for any errors during
            // init expression evaluation since that is all we have
            // (see issues #42991, #42992).
            check.errpos = atPos(obj.pos);
        }
        check.expr(_addr_x, init);
    }
    check.initConst(obj, _addr_x);
});

private static void varDecl(this ptr<Checker> _addr_check, ptr<Var> _addr_obj, slice<ptr<Var>> lhs, ast.Expr typ, ast.Expr init) => func((_, panic, _) => {
    ref Checker check = ref _addr_check.val;
    ref Var obj = ref _addr_obj.val;

    assert(obj.typ == null); 

    // determine type, if any
    if (typ != null) {
        obj.typ = check.varType(typ); 
        // We cannot spread the type to all lhs variables if there
        // are more than one since that would mark them as checked
        // (see Checker.objDecl) and the assignment of init exprs,
        // if any, would not be checked.
        //
        // TODO(gri) If we have no init expr, we should distribute
        // a given type otherwise we need to re-evalate the type
        // expr for each lhs variable, leading to duplicate work.
    }
    if (init == null) {
        if (typ == null) { 
            // error reported before by arityMatch
            obj.typ = Typ[Invalid];
        }
        return ;
    }
    if (lhs == null || len(lhs) == 1) {
        assert(lhs == null || lhs[0] == obj);
        ref operand x = ref heap(out ptr<operand> _addr_x);
        check.expr(_addr_x, init);
        check.initVar(obj, _addr_x, "variable declaration");
        return ;
    }
    if (debug) { 
        // obj must be one of lhs
        var found = false;
        {
            var lhs__prev1 = lhs;

            foreach (var (_, __lhs) in lhs) {
                lhs = __lhs;
                if (obj == lhs) {
                    found = true;
                    break;
                }
            }

            lhs = lhs__prev1;
        }

        if (!found) {
            panic("inconsistent lhs");
        }
    }
    if (typ != null) {
        {
            var lhs__prev1 = lhs;

            foreach (var (_, __lhs) in lhs) {
                lhs = __lhs;
                lhs.typ = obj.typ;
            }

            lhs = lhs__prev1;
        }
    }
    check.initVars(lhs, new slice<ast.Expr>(new ast.Expr[] { init }), token.NoPos);
});

// under returns the expanded underlying type of n0; possibly by following
// forward chains of named types. If an underlying type is found, resolve
// the chain by setting the underlying type for each defined type in the
// chain before returning it. If no underlying type is found or a cycle
// is detected, the result is Typ[Invalid]. If a cycle is detected and
// n0.check != nil, the cycle is reported.
private static Type under(this ptr<Named> _addr_n0) => func((_, panic, _) => {
    ref Named n0 = ref _addr_n0.val;

    var u = n0.underlying;

    if (u == Typ[Invalid]) {
        return u;
    }
    switch (u.type()) {
        case 
            return Typ[Invalid];
            break;
        case ptr<Named> _:
            break;
        case ptr<instance> _:
            break;
        default:
        {
            return u;
            break;
        }

    }

    if (n0.check == null) {
        panic("internal error: Named.check == nil but type is incomplete");
    }
    var check = n0.check; 

    // If we can't expand u at this point, it is invalid.
    var n = asNamed(u);
    if (n == null) {
        n0.underlying = Typ[Invalid];
        return n0.underlying;
    }
    map seen = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<Named>, nint>{n0:0};
    Object path = new slice<Object>(new Object[] { n0.obj });
    while (true) {
        u = n.underlying;
        if (u == null) {
            u = Typ[Invalid];
            break;
        }
        ptr<Named> n1;
        switch (u.type()) {
            case ptr<Named> u1:
                n1 = u1;
                break;
            case ptr<instance> u1:
                n1, _ = u1.expand()._<ptr<Named>>();
                if (n1 == null) {
                    u = Typ[Invalid];
                }
                break;
        }
        if (n1 == null) {
            break; // end of chain
        }
        seen[n] = len(seen);
        path = append(path, n.obj);
        n = n1;

        {
            var (i, ok) = seen[n];

            if (ok) { 
                // cycle
                check.cycleError(path[(int)i..]);
                u = Typ[Invalid];
                break;
            }

        }
    }

    {
        var n__prev1 = n;

        foreach (var (__n) in seen) {
            n = __n; 
            // We should never have to update the underlying type of an imported type;
            // those underlying types should have been resolved during the import.
            // Also, doing so would lead to a race condition (was issue #31749).
            // Do this check always, not just in debug mode (it's cheap).
            if (n.obj.pkg != check.pkg) {
                panic("internal error: imported type with unresolved underlying type");
            }
            n.underlying = u;
        }
        n = n__prev1;
    }

    return u;
});

private static void setUnderlying(this ptr<Named> _addr_n, Type typ) {
    ref Named n = ref _addr_n.val;

    if (n != null) {
        n.underlying = typ;
    }
}

private static void typeDecl(this ptr<Checker> _addr_check, ptr<TypeName> _addr_obj, ptr<ast.TypeSpec> _addr_tdecl, ptr<Named> _addr_def) => func((defer, _, _) => {
    ref Checker check = ref _addr_check.val;
    ref TypeName obj = ref _addr_obj.val;
    ref ast.TypeSpec tdecl = ref _addr_tdecl.val;
    ref Named def = ref _addr_def.val;

    assert(obj.typ == null);

    check.later(() => {
        check.validType(obj.typ, null);
    });

    var alias = tdecl.Assign.IsValid();
    if (alias && typeparams.Get(tdecl) != null) { 
        // The parser will ensure this but we may still get an invalid AST.
        // Complain and continue as regular type definition.
        check.error(atPos(tdecl.Assign), 0, "generic type cannot be alias");
        alias = false;
    }
    if (alias) { 
        // type alias declaration
        if (!check.allowVersion(check.pkg, 1, 9)) {
            check.errorf(atPos(tdecl.Assign), _BadDecl, "type aliases requires go1.9 or later");
        }
        obj.typ = Typ[Invalid];
        obj.typ = check.anyType(tdecl.Type);
    }
    else
 { 
        // defined type declaration

        var named = check.newNamed(obj, null, null);
        def.setUnderlying(named);
        obj.typ = named; // make sure recursive type declarations terminate

        {
            var tparams = typeparams.Get(tdecl);

            if (tparams != null) {
                check.openScope(tdecl, "type parameters");
                defer(check.closeScope());
                named.tparams = check.collectTypeParams(tparams);
            } 

            // determine underlying type of named

        } 

        // determine underlying type of named
        named.orig = check.definedType(tdecl.Type, named); 

        // The underlying type of named may be itself a named type that is
        // incomplete:
        //
        //    type (
        //        A B
        //        B *C
        //        C A
        //    )
        //
        // The type of C is the (named) type of A which is incomplete,
        // and which has as its underlying type the named type B.
        // Determine the (final, unnamed) underlying type by resolving
        // any forward chain.
        // TODO(gri) Investigate if we can just use named.origin here
        //           and rely on lazy computation of the underlying type.
        named.underlying = under(named);
    }
});

private static slice<ptr<TypeName>> collectTypeParams(this ptr<Checker> _addr_check, ptr<ast.FieldList> _addr_list) {
    slice<ptr<TypeName>> tparams = default;
    ref Checker check = ref _addr_check.val;
    ref ast.FieldList list = ref _addr_list.val;
 
    // Type parameter lists should not be empty. The parser will
    // complain but we still may get an incorrect AST: ignore it.
    if (list.NumFields() == 0) {
        return ;
    }
    {
        var f__prev1 = f;

        foreach (var (_, __f) in list.List) {
            f = __f;
            tparams = check.declareTypeParams(tparams, f.Names);
        }
        f = f__prev1;
    }

    Action<nint, Type> setBoundAt = (at, bound) => {
        assert(IsInterface(bound));
        tparams[at].typ._<ptr<_TypeParam>>().bound = bound;
    };

    nint index = 0;
    Type bound = default;
    {
        var f__prev1 = f;

        foreach (var (_, __f) in list.List) {
            f = __f;
            if (f.Type == null) {
                goto next;
            } 

            // The predeclared identifier "any" is visible only as a constraint
            // in a type parameter list. Look for it before general constraint
            // resolution.
            {
                ptr<ast.Ident> (tident, _) = unparen(f.Type)._<ptr<ast.Ident>>();

                if (tident != null && tident.Name == "any" && check.lookup("any") == null) {
                    bound = universeAny;
                }
                else
 {
                    bound = check.typ(f.Type);
                } 

                // type bound must be an interface
                // TODO(gri) We should delay the interface check because
                //           we may not have a complete interface yet:
                //           type C(type T C) interface {}
                //           (issue #39724).

            } 

            // type bound must be an interface
            // TODO(gri) We should delay the interface check because
            //           we may not have a complete interface yet:
            //           type C(type T C) interface {}
            //           (issue #39724).
            {
                ptr<Interface> (_, ok) = under(bound)._<ptr<Interface>>();

                if (ok) { 
                    // Otherwise, set the bound for each type parameter.
                    foreach (var (i) in f.Names) {
                        setBoundAt(index + i, bound);
                    }
                }
                else if (bound != Typ[Invalid]) {
                    check.errorf(f.Type, _Todo, "%s is not an interface", bound);
                }

            }

next:
            index += len(f.Names);
        }
        f = f__prev1;
    }

    return ;
}

private static slice<ptr<TypeName>> declareTypeParams(this ptr<Checker> _addr_check, slice<ptr<TypeName>> tparams, slice<ptr<ast.Ident>> names) {
    ref Checker check = ref _addr_check.val;

    foreach (var (_, name) in names) {
        var tpar = NewTypeName(name.Pos(), check.pkg, name.Name, null);
        check.newTypeParam(tpar, len(tparams), _addr_emptyInterface); // assigns type to tpar as a side-effect
        check.declare(check.scope, name, tpar, check.scope.pos); // TODO(gri) check scope position
        tparams = append(tparams, tpar);
    }    if (trace && len(names) > 0) {
        check.trace(names[0].Pos(), "type params = %v", tparams[(int)len(tparams) - len(names)..]);
    }
    return tparams;
}

private static void collectMethods(this ptr<Checker> _addr_check, ptr<TypeName> _addr_obj) {
    ref Checker check = ref _addr_check.val;
    ref TypeName obj = ref _addr_obj.val;
 
    // get associated methods
    // (Checker.collectObjects only collects methods with non-blank names;
    // Checker.resolveBaseTypeName ensures that obj is not an alias name
    // if it has attached methods.)
    var methods = check.methods[obj];
    if (methods == null) {
        return ;
    }
    delete(check.methods, obj);
    assert(!check.objMap[obj].tdecl.Assign.IsValid()); // don't use TypeName.IsAlias (requires fully set up object)

    // use an objset to check for name conflicts
    objset mset = default; 

    // spec: "If the base type is a struct type, the non-blank method
    // and field names must be distinct."
    var @base = asNamed(obj.typ); // shouldn't fail but be conservative
    if (base != null) {
        {
            ptr<Struct> (t, _) = @base.underlying._<ptr<Struct>>();

            if (t != null) {
                foreach (var (_, fld) in t.fields) {
                    if (fld.name != "_") {
                        assert(mset.insert(fld) == null);
                    }
                }
            } 

            // Checker.Files may be called multiple times; additional package files
            // may add methods to already type-checked types. Add pre-existing methods
            // so that we can detect redeclarations.

        } 

        // Checker.Files may be called multiple times; additional package files
        // may add methods to already type-checked types. Add pre-existing methods
        // so that we can detect redeclarations.
        {
            var m__prev1 = m;

            foreach (var (_, __m) in @base.methods) {
                m = __m;
                assert(m.name != "_");
                assert(mset.insert(m) == null);
            }

            m = m__prev1;
        }
    }
    {
        var m__prev1 = m;

        foreach (var (_, __m) in methods) {
            m = __m; 
            // spec: "For a base type, the non-blank names of methods bound
            // to it must be unique."
            assert(m.name != "_");
            {
                var alt = mset.insert(m);

                if (alt != null) {
                    switch (alt.type()) {
                        case ptr<Var> _:
                            check.errorf(m, _DuplicateFieldAndMethod, "field and method with the same name %s", m.name);
                            break;
                        case ptr<Func> _:
                            check.errorf(m, _DuplicateMethod, "method %s already declared for %s", m.name, obj);
                            break;
                        default:
                        {
                            unreachable();
                            break;
                        }
                    }
                    check.reportAltDecl(alt);
                    continue;
                }

            }

            if (base != null) {
                @base.methods = append(@base.methods, m);
            }
        }
        m = m__prev1;
    }
}

private static void funcDecl(this ptr<Checker> _addr_check, ptr<Func> _addr_obj, ptr<declInfo> _addr_decl) {
    ref Checker check = ref _addr_check.val;
    ref Func obj = ref _addr_obj.val;
    ref declInfo decl = ref _addr_decl.val;

    assert(obj.typ == null); 

    // func declarations cannot use iota
    assert(check.iota == null);

    ptr<object> sig = @new<Signature>();
    obj.typ = sig; // guard against cycles

    // Avoid cycle error when referring to method while type-checking the signature.
    // This avoids a nuisance in the best case (non-parameterized receiver type) and
    // since the method is not a type, we get an error. If we have a parameterized
    // receiver type, instantiating the receiver type leads to the instantiation of
    // its methods, and we don't want a cycle error in that case.
    // TODO(gri) review if this is correct and/or whether we still need this?
    var saved = obj.color_;
    obj.color_ = black;
    var fdecl = decl.fdecl;
    check.funcType(sig, fdecl.Recv, fdecl.Type);
    obj.color_ = saved; 

    // function body must be type-checked after global declarations
    // (functions implemented elsewhere have no body)
    if (!check.conf.IgnoreFuncBodies && fdecl.Body != null) {
        check.later(() => {
            check.funcBody(decl, obj.name, sig, fdecl.Body, null);
        });
    }
}

private static void declStmt(this ptr<Checker> _addr_check, ast.Decl d) {
    ref Checker check = ref _addr_check.val;

    var pkg = check.pkg;

    check.walkDecl(d, d => {
        switch (d.type()) {
            case constDecl d:
                var top = len(check.delayed); 

                // declare all constants
                var lhs = make_slice<ptr<Const>>(len(d.spec.Names));
                {
                    var i__prev1 = i;
                    var name__prev1 = name;

                    foreach (var (__i, __name) in d.spec.Names) {
                        i = __i;
                        name = __name;
                        var obj = NewConst(name.Pos(), pkg, name.Name, null, constant.MakeInt64(int64(d.iota)));
                        lhs[i] = obj;

                        ast.Expr init = default;
                        if (i < len(d.init)) {
                            init = d.init[i];
                        }
                        check.constDecl(obj, d.typ, init, d.inherited);
                    } 

                    // process function literals in init expressions before scope changes

                    i = i__prev1;
                    name = name__prev1;
                }

                check.processDelayed(top); 

                // spec: "The scope of a constant or variable identifier declared
                // inside a function begins at the end of the ConstSpec or VarSpec
                // (ShortVarDecl for short variable declarations) and ends at the
                // end of the innermost containing block."
                var scopePos = d.spec.End();
                {
                    var i__prev1 = i;
                    var name__prev1 = name;

                    foreach (var (__i, __name) in d.spec.Names) {
                        i = __i;
                        name = __name;
                        check.declare(check.scope, name, lhs[i], scopePos);
                    }

                    i = i__prev1;
                    name = name__prev1;
                }
                break;
            case varDecl d:
                top = len(check.delayed);

                var lhs0 = make_slice<ptr<Var>>(len(d.spec.Names));
                {
                    var i__prev1 = i;
                    var name__prev1 = name;

                    foreach (var (__i, __name) in d.spec.Names) {
                        i = __i;
                        name = __name;
                        lhs0[i] = NewVar(name.Pos(), pkg, name.Name, null);
                    } 

                    // initialize all variables

                    i = i__prev1;
                    name = name__prev1;
                }

                {
                    var i__prev1 = i;
                    var obj__prev1 = obj;

                    foreach (var (__i, __obj) in lhs0) {
                        i = __i;
                        obj = __obj;
                        lhs = default;
                        init = default;

                        if (len(d.spec.Values) == len(d.spec.Names)) 
                            // lhs and rhs match
                            init = d.spec.Values[i];
                        else if (len(d.spec.Values) == 1) 
                            // rhs is expected to be a multi-valued expression
                            lhs = lhs0;
                            init = d.spec.Values[0];
                        else 
                            if (i < len(d.spec.Values)) {
                                init = d.spec.Values[i];
                            }
                                                check.varDecl(obj, lhs, d.spec.Type, init);
                        if (len(d.spec.Values) == 1) { 
                            // If we have a single lhs variable we are done either way.
                            // If we have a single rhs expression, it must be a multi-
                            // valued expression, in which case handling the first lhs
                            // variable will cause all lhs variables to have a type
                            // assigned, and we are done as well.
                            if (debug) {
                                {
                                    var obj__prev2 = obj;

                                    foreach (var (_, __obj) in lhs0) {
                                        obj = __obj;
                                        assert(obj.typ != null);
                                    }

                                    obj = obj__prev2;
                                }
                            }
                            break;
                        }
                    } 

                    // process function literals in init expressions before scope changes

                    i = i__prev1;
                    obj = obj__prev1;
                }

                check.processDelayed(top); 

                // declare all variables
                // (only at this point are the variable scopes (parents) set)
                scopePos = d.spec.End(); // see constant declarations
                {
                    var i__prev1 = i;
                    var name__prev1 = name;

                    foreach (var (__i, __name) in d.spec.Names) {
                        i = __i;
                        name = __name; 
                        // see constant declarations
                        check.declare(check.scope, name, lhs0[i], scopePos);
                    }

                    i = i__prev1;
                    name = name__prev1;
                }
                break;
            case typeDecl d:
                obj = NewTypeName(d.spec.Name.Pos(), pkg, d.spec.Name.Name, null); 
                // spec: "The scope of a type identifier declared inside a function
                // begins at the identifier in the TypeSpec and ends at the end of
                // the innermost containing block."
                scopePos = d.spec.Name.Pos();
                check.declare(check.scope, d.spec.Name, obj, scopePos); 
                // mark and unmark type before calling typeDecl; its type is still nil (see Checker.objDecl)
                obj.setColor(grey + color(check.push(obj)));
                check.typeDecl(obj, d.spec, null);
                check.pop().setColor(black);
                break;
            default:
            {
                var d = d.type();
                check.invalidAST(d.node(), "unknown ast.Decl node %T", d.node());
                break;
            }
        }
    });
}

} // end types_package
