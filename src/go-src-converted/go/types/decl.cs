// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using ast = go.ast_package;
using constant = go.constant_package;
using token = go.token_package;
using buildcfg = @internal.buildcfg_package;
using static @internal.types.errors_package;
using @internal;

partial class types_package {

[GoRecv] public static void declare(this ref Checker check, ж<ΔScope> Ꮡscope, ж<ast.Ident> Ꮡid, Object obj, tokenꓸPos pos) {
    ref var scope = ref Ꮡscope.val;
    ref var id = ref Ꮡid.val;

    // spec: "The blank identifier, represented by the underscore
    // character _, may be used in a declaration like any other
    // identifier but the declaration does not introduce a new
    // binding."
    if (obj.Name() != "_"u8) {
        {
            var alt = scope.Insert(obj); if (alt != default!) {
                var err = check.newError(DuplicateDecl);
                err.addf(obj, "%s redeclared in this block"u8, obj.Name());
                err.addAltDecl(alt);
                err.report();
                return;
            }
        }
        obj.setScopePos(pos);
    }
    if (id != nil) {
        check.recordDef(Ꮡid, obj);
    }
}

// pathString returns a string of the form a->b-> ... ->g for a path [a, b, ... g].
internal static @string pathString(slice<Object> path) {
    @string s = default!;
    foreach (var (i, p) in path) {
        if (i > 0) {
            s += "->"u8;
        }
        s += p.Name();
    }
    return s;
}

// objDecl type-checks the declaration of obj in its respective (file) environment.
// For the meaning of def, see Checker.definedType, in typexpr.go.
[GoRecv] public static void objDecl(this ref Checker check, Object obj, ж<TypeName> Ꮡdef) => func((defer, _) => {
    ref var def = ref Ꮡdef.val;

    if (check.conf._Trace && obj.Type() == default!) {
        if (check.indent == 0) {
            fmt.Println();
        }
        // empty line between top-level objects for readability
        check.trace(obj.Pos(), "-- checking %s (%s, objPath = %s)"u8, obj, obj.color(), pathString(check.objPath));
        check.indent++;
        defer(() => {
            check.indent--;
            check.trace(obj.Pos(), "=> %s (%s)"u8, obj, obj.color());
        });
    }
    // Checking the declaration of obj means inferring its type
    // (and possibly its value, for constants).
    // An object's type (and thus the object) may be in one of
    // three states which are expressed by colors:
    //
    // - an object whose type is not yet known is painted white (initial color)
    // - an object whose type is in the process of being inferred is painted grey
    // - an object whose type is fully inferred is painted black
    //
    // During type inference, an object's color changes from white to grey
    // to black (pre-declared objects are painted black from the start).
    // A black object (i.e., its type) can only depend on (refer to) other black
    // ones. White and grey objects may depend on white and black objects.
    // A dependency on a grey object indicates a cycle which may or may not be
    // valid.
    //
    // When objects turn grey, they are pushed on the object path (a stack);
    // they are popped again when they turn black. Thus, if a grey object (a
    // cycle) is encountered, it is on the object path, and all the objects
    // it depends on are the remaining objects on that path. Color encoding
    // is such that the color value of a grey object indicates the index of
    // that object in the object path.
    // During type-checking, white objects may be assigned a type without
    // traversing through objDecl; e.g., when initializing constants and
    // variables. Update the colors of those objects here (rather than
    // everywhere where we set the type) to satisfy the color invariants.
    if (obj.color() == white && obj.Type() != default!) {
        obj.setColor(black);
        return;
    }
    var exprᴛ1 = obj.color();
    if (exprᴛ1 == white) {
        assert(obj.Type() == default!);
        obj.setColor(grey + ((Δcolor)check.push(obj)));
        defer(() => {
            // All color values other than white and black are considered grey.
            // Because black and white are < grey, all values >= grey are grey.
            // Use those values to encode the object's index into the object path.
            check.pop().setColor(black);
        });
    }
    else if (exprᴛ1 == black) {
        assert(obj.Type() != default!);
        return;
    }
    { /* default: */
    }
    else if (exprᴛ1 == grey) {
        switch (obj.type()) {
        case Const.val obj: {
            if (!check.validCycle(~obj) || obj.typ == default!) {
                // Color values other than white or black are considered grey.
                // We have a (possibly invalid) cycle.
                // In the existing code, this is marked by a non-nil type
                // for the object except for constants and variables whose
                // type may be non-nil (known), or nil if it depends on the
                // not-yet known initialization value.
                // In the former case, set the type to Typ[Invalid] because
                // we have an initialization cycle. The cycle error will be
                // reported later, when determining initialization order.
                // TODO(gri) Report cycle here and simplify initialization
                // order code.
                obj.typ = Typ[Invalid];
            }
            break;
        }
        case Var.val obj: {
            if (!check.validCycle(~obj) || obj.typ == default!) {
                obj.typ = Typ[Invalid];
            }
            break;
        }
        case TypeName.val obj: {
            if (!check.validCycle(~obj)) {
                // break cycle
                // (without this, calling underlying()
                // below may lead to an endless loop
                // if we have a cycle for a defined
                // (*Named) type)
                obj.typ = Typ[Invalid];
            }
            break;
        }
        case Func.val obj: {
            if (!check.validCycle(~obj)) {
            }
            break;
        }
        default: {
            var obj = obj.type();
            throw panic("unreachable");
            break;
        }}
        assert(obj.Type() != default!);
        return;
    }

    // Don't set obj.typ to Typ[Invalid] here
    // because plenty of code type-asserts that
    // functions have a *Signature type. Grey
    // functions have their type set to an empty
    // signature which makes it impossible to
    // initialize a variable with the function.
    var d = check.objMap[obj];
    if (d == nil) {
        check.dump("%v: %s should have been declared"u8, obj.Pos(), obj);
        throw panic("unreachable");
    }
    // save/restore current environment and set up object environment
    deferǃ((environment env) => {
        check.environment = env;
    }, check.environment, defer);
    check.environment = new environment(
        scope: (~d).file
    );
    // Const and var declarations must not have initialization
    // cycles. We track them by remembering the current declaration
    // in check.decl. Initialization expressions depending on other
    // consts, vars, or functions, add dependencies to the current
    // check.decl.
    switch (obj.type()) {
    case Const.val obj: {
        check.decl = d;
        check.constDecl(Ꮡobj, // new package-level const decl
 (~d).vtyp, (~d).init, (~d).inherited);
        break;
    }
    case Var.val obj: {
        check.decl = d;
        check.varDecl(Ꮡobj, // new package-level var decl
 (~d).lhs, (~d).vtyp, (~d).init);
        break;
    }
    case TypeName.val obj: {
        check.typeDecl(Ꮡobj, // invalid recursive types are detected via path
 (~d).tdecl, Ꮡdef);
        check.collectMethods(Ꮡobj);
        break;
    }
    case Func.val obj: {
        check.funcDecl(Ꮡobj, // methods can only be added to top-level types
 // functions may be recursive - no need to track dependencies
 d);
        break;
    }
    default: {
        var obj = obj.type();
        throw panic("unreachable");
        break;
    }}
});

// validCycle checks if the cycle starting with obj is valid and
// reports an error if it is not.
[GoRecv] internal static bool /*valid*/ validCycle(this ref Checker check, Object obj) => func((defer, _) => {
    bool valid = default!;

    // The object map contains the package scope objects and the non-interface methods.
    if (debug) {
        var info = check.objMap[obj];
        var inObjMap = info != nil && ((~info).fdecl == nil || (~(~info).fdecl).Recv == nil);
        // exclude methods
        var isPkgObj = obj.Parent() == check.pkg.scope;
        if (isPkgObj != inObjMap) {
            check.dump("%v: inconsistent object map for %s (isPkgObj = %v, inObjMap = %v)"u8, obj.Pos(), obj, isPkgObj, inObjMap);
            throw panic("unreachable");
        }
    }
    // Count cycle objects.
    assert(obj.color() >= grey);
    var start = obj.color() - grey;
    // index of obj in objPath
    var cycle = check.objPath[(int)(start)..];
    var tparCycle = false;
    // if set, the cycle is through a type parameter list
    nint nval = 0;
    // number of (constant or variable) values in the cycle; valid if !generic
    nint ndef = 0;
    // number of type definitions in the cycle; valid if !generic
loop:
    foreach (var (_, objΔ1) in cycle) {
        switch (objΔ1.type()) {
        case Const.val obj: {
            nval++;
            break;
        }
        case Var.val obj: {
            nval++;
            break;
        }
        case TypeName.val obj: {
            if (check.inTParamList && isGeneric(objΔ1.typ)) {
                // If we reach a generic type that is part of a cycle
                // and we are in a type parameter list, we have a cycle
                // through a type parameter list, which is invalid.
                tparCycle = true;
                goto break_loop;
            }
            // Determine if the type name is an alias or not. For
            // package-level objects, use the object map which
            // provides syntactic information (which doesn't rely
            // on the order in which the objects are set up). For
            // local objects, we can rely on the order, so use
            // the object's predicate.
            // TODO(gri) It would be less fragile to always access
            // the syntactic information. We should consider storing
            // this information explicitly in the object.
            bool alias = default!;
            if (check.conf._EnableAlias){
                alias = objΔ1.IsAlias();
            } else {
                {
                    var d = check.objMap[objΔ1]; if (d != nil){
                        alias = (~(~d).tdecl).Assign.IsValid();
                    } else {
                        // package-level object
                        alias = objΔ1.IsAlias();
                    }
                }
            }
            if (!alias) {
                // function local object
                ndef++;
            }
            break;
        }
        case Func.val obj: {
            break;
        }
        default: {
            var obj = objΔ1.type();
            throw panic("unreachable");
            break;
        }}
    }
    // ignored for now
    if (check.conf._Trace) {
        check.trace(obj.Pos(), "## cycle detected: objPath = %s->%s (len = %d)"u8, pathString(cycle), obj.Name(), len(cycle));
        if (tparCycle){
            check.trace(obj.Pos(), "## cycle contains: generic type in a type parameter list"u8);
        } else {
            check.trace(obj.Pos(), "## cycle contains: %d values, %d type definitions"u8, nval, ndef);
        }
        defer(() => {
            if (valid){
                check.trace(obj.Pos(), "=> cycle is valid"u8);
            } else {
                check.trace(obj.Pos(), "=> error: cycle is invalid"u8);
            }
        });
    }
    if (!tparCycle) {
        // A cycle involving only constants and variables is invalid but we
        // ignore them here because they are reported via the initialization
        // cycle check.
        if (nval == len(cycle)) {
            return true;
        }
        // A cycle involving only types (and possibly functions) must have at least
        // one type definition to be permitted: If there is no type definition, we
        // have a sequence of alias type names which will expand ad infinitum.
        if (nval == 0 && ndef > 0) {
            return true;
        }
    }
    check.cycleError(cycle, firstInSrc(cycle));
    return false;
});

// cycleError reports a declaration cycle starting with the object at cycle[start].
[GoRecv] internal static void cycleError(this ref Checker check, slice<Object> cycle, nint start) {
    // name returns the (possibly qualified) object name.
    // This is needed because with generic types, cycles
    // may refer to imported types. See go.dev/issue/50788.
    // TODO(gri) Thus functionality is used elsewhere. Factor it out.
    var name = (Object obj) => packagePrefix(objΔ1.Pkg(), check.qualifier) + objΔ1.Name();
    var obj = cycle[start];
    @string objName = name(obj);
    // If obj is a type alias, mark it as valid (not broken) in order to avoid follow-on errors.
    var (tname, _) = obj._<TypeName.val>(ᐧ);
    if (tname != nil && tname.IsAlias()) {
        // If we use Alias nodes, it is initialized with Typ[Invalid].
        // TODO(gri) Adjust this code if we initialize with nil.
        if (!check.conf._EnableAlias) {
            check.validAlias(tname, ~Typ[Invalid]);
        }
    }
    // report a more concise error for self references
    if (len(cycle) == 1) {
        if (tname != nil){
            check.errorf(obj, InvalidDeclCycle, "invalid recursive type: %s refers to itself"u8, objName);
        } else {
            check.errorf(obj, InvalidDeclCycle, "invalid cycle in declaration: %s refers to itself"u8, objName);
        }
        return;
    }
    var err = check.newError(InvalidDeclCycle);
    if (tname != nil){
        err.addf(obj, "invalid recursive type %s"u8, objName);
    } else {
        err.addf(obj, "invalid cycle in declaration of %s"u8, objName);
    }
    nint i = start;
    foreach ((_, _) in cycle) {
        err.addf(obj, "%s refers to"u8, objName);
        i++;
        if (i >= len(cycle)) {
            i = 0;
        }
        obj = cycle[i];
        objName = name(obj);
    }
    err.addf(obj, "%s"u8, objName);
    err.report();
}

// firstInSrc reports the index of the object with the "smallest"
// source position in path. path must not be empty.
internal static nint firstInSrc(slice<Object> path) {
    nint fst = 0;
    tokenꓸPos pos = path[0].Pos();
    foreach (var (i, t) in path[1..]) {
        if (cmpPos(t.Pos(), pos) < 0) {
            (fst, pos) = (i + 1, t.Pos());
        }
    }
    return fst;
}

[GoType] partial interface decl {
    ast.Node node();
}

[GoType] partial struct importDecl {
    internal ж<go.ast_package.ImportSpec> spec;
}

[GoType] partial struct ΔconstDecl {
    internal ж<go.ast_package.ValueSpec> spec;
    internal nint iota;
    internal go.ast_package.Expr typ;
    internal ast.Expr init;
    internal bool inherited;
}

[GoType] partial struct ΔvarDecl {
    internal ж<go.ast_package.ValueSpec> spec;
}

[GoType] partial struct ΔtypeDecl {
    internal ж<go.ast_package.TypeSpec> spec;
}

[GoType] partial struct ΔfuncDecl {
    internal ж<go.ast_package.FuncDecl> decl;
}

internal static ast.Node node(this importDecl d) {
    return ~d.spec;
}

internal static ast.Node node(this ΔconstDecl d) {
    return ~d.spec;
}

internal static ast.Node node(this ΔvarDecl d) {
    return ~d.spec;
}

internal static ast.Node node(this ΔtypeDecl d) {
    return ~d.spec;
}

internal static ast.Node node(this ΔfuncDecl d) {
    return ~d.decl;
}

[GoRecv] internal static void walkDecls(this ref Checker check, slice<ast.Decl> decls, Action<decl> f) {
    foreach (var (_, d) in decls) {
        check.walkDecl(d, f);
    }
}

[GoRecv] internal static void walkDecl(this ref Checker check, ast.Decl d, Action<decl> f) {
    switch (d.type()) {
    case ж<ast.BadDecl> d: {
        break;
    }
    case ж<ast.GenDecl> d: {
// ignore
        ж<ast.ValueSpec> last = default!;                          // last ValueSpec with type or init exprs seen
        foreach (var (iota, s) in (~d).Specs) {
            switch (s.type()) {
            case ж<ast.ImportSpec> s: {
                f(new importDecl(s));
                break;
            }
            case ж<ast.ValueSpec> s: {
                var exprᴛ1 = (~d).Tok;
                if (exprᴛ1 == token.CONST) {
                    var inherited = true;
                    switch (ᐧ) {
                    case {} when (~s).Type != default! || len((~s).Values) > 0: {
                        last = s;
                        inherited = false;
                        break;
                    }
                    case {} when last == nil: {
                        last = @new<ast.ValueSpec>();
                        inherited = false;
                        break;
                    }}

                    check.arityMatch(s, // determine which initialization expressions to use
 // make sure last exists
 last);
                    f(new ΔconstDecl(spec: s, iota: iota, typ: (~last).Type, init: (~last).Values, inherited: inherited));
                }
                else if (exprᴛ1 == token.VAR) {
                    check.arityMatch(s, nil);
                    f(new ΔvarDecl(s));
                }
                else { /* default: */
                    check.errorf(~s, InvalidSyntaxTree, "invalid token %s"u8, (~d).Tok);
                }

                break;
            }
            case ж<ast.TypeSpec> s: {
                f(new ΔtypeDecl(s));
                break;
            }
            default: {
                var s = s.type();
                check.errorf(s, InvalidSyntaxTree, "unknown ast.Spec node %T"u8, s);
                break;
            }}
        }
        break;
    }
    case ж<ast.FuncDecl> d: {
        f(new ΔfuncDecl(Ꮡd));
        break;
    }
    default: {
        var d = d.type();
        check.errorf(d, InvalidSyntaxTree, "unknown ast.Decl node %T"u8, d);
        break;
    }}
}

[GoRecv] public static void constDecl(this ref Checker check, ж<Const> Ꮡobj, ast.Expr typ, ast.Expr init, bool inherited) => func((defer, _) => {
    ref var obj = ref Ꮡobj.val;

    assert(obj.typ == default!);
    // use the correct value of iota
    deferǃ((constant.Value iota, positioner errpos) => {
        check.iota = iota;
        check.errpos = errpos;
    }, check.iota, check.errpos, defer);
    check.iota = obj.val;
    check.errpos = default!;
    // provide valid constant value under all circumstances
    obj.val = constant.MakeUnknown();
    // determine type, if any
    if (typ != default!) {
        var t = check.typ(typ);
        if (!isConstType(t)) {
            // don't report an error if the type is an invalid C (defined) type
            // (go.dev/issue/22090)
            if (isValid(under(t))) {
                check.errorf(typ, InvalidConstType, "invalid constant type %s"u8, t);
            }
            obj.typ = Typ[Invalid];
            return;
        }
        obj.typ = t;
    }
    // check initialization
    ref var x = ref heap(new operand(), out var Ꮡx);
    if (init != default!) {
        if (inherited) {
            // The initialization expression is inherited from a previous
            // constant declaration, and (error) positions refer to that
            // expression and not the current constant declaration. Use
            // the constant identifier position for any errors during
            // init expression evaluation since that is all we have
            // (see issues go.dev/issue/42991, go.dev/issue/42992).
            check.errpos = ((atPos)obj.pos);
        }
        check.expr(nil, Ꮡx, init);
    }
    check.initConst(Ꮡobj, Ꮡx);
});

[GoRecv] public static void varDecl(this ref Checker check, ж<Var> Ꮡobj, slice<ж<Var>> lhs, ast.Expr typ, ast.Expr init) {
    ref var obj = ref Ꮡobj.val;

    assert(obj.typ == default!);
    // determine type, if any
    if (typ != default!) {
        obj.typ = check.varType(typ);
    }
    // We cannot spread the type to all lhs variables if there
    // are more than one since that would mark them as checked
    // (see Checker.objDecl) and the assignment of init exprs,
    // if any, would not be checked.
    //
    // TODO(gri) If we have no init expr, we should distribute
    // a given type otherwise we need to re-evaluate the type
    // expr for each lhs variable, leading to duplicate work.
    // check initialization
    if (init == default!) {
        if (typ == default!) {
            // error reported before by arityMatch
            obj.typ = Typ[Invalid];
        }
        return;
    }
    if (lhs == default! || len(lhs) == 1) {
        assert(lhs == default! || lhs[0] == Ꮡobj);
        ref var x = ref heap(new operand(), out var Ꮡx);
        check.expr(newTarget(obj.typ, obj.name), Ꮡx, init);
        check.initVar(Ꮡobj, Ꮡx, "variable declaration"u8);
        return;
    }
    if (debug) {
        // obj must be one of lhs
        var found = false;
        foreach (var (_, lhsΔ1) in lhs) {
            if (Ꮡobj == ᏑlhsΔ1) {
                found = true;
                break;
            }
        }
        if (!found) {
            throw panic("inconsistent lhs");
        }
    }
    // We have multiple variables on the lhs and one init expr.
    // Make sure all variables have been given the same type if
    // one was specified, otherwise they assume the type of the
    // init expression values (was go.dev/issue/15755).
    if (typ != default!) {
        foreach (var (_, lhsΔ2) in lhs) {
            lhsΔ2.typ = obj.typ;
        }
    }
    check.initVars(lhs, new ast.Expr[]{init}.slice(), default!);
}

// isImportedConstraint reports whether typ is an imported type constraint.
[GoRecv] internal static bool isImportedConstraint(this ref Checker check, ΔType typ) {
    var named = asNamed(typ);
    if (named == nil || (~named).obj.pkg == check.pkg || (~named).obj.pkg == nil) {
        return false;
    }
    var (u, _) = named.under()._<Interface.val>(ᐧ);
    return u != nil && !u.IsMethodSet();
}

[GoRecv] public static void typeDecl(this ref Checker check, ж<TypeName> Ꮡobj, ж<ast.TypeSpec> Ꮡtdecl, ж<TypeName> Ꮡdef) => func((defer, _) => {
    ref var obj = ref Ꮡobj.val;
    ref var tdecl = ref Ꮡtdecl.val;
    ref var def = ref Ꮡdef.val;

    assert(obj.typ == default!);
    // Only report a version error if we have not reported one already.
    var versionErr = false;
    ΔType rhs = default!;
    check.later(
    var rhsʗ11 = rhs;
    () => {
        {
            var t = asNamed(obj.typ); if (t != nil) {
                check.validType(t);
            }
        }
        _ = !versionErr && check.isImportedConstraint(rhsʗ11) && check.verifyVersionf(tdecl.Type, go1_18, "using type constraint %s"u8, rhsʗ11);
    }).describef(~obj, "validType(%s)"u8, obj.Name());
    // First type parameter, or nil.
    ж<ast.Field> tparam0 = default!;
    if (tdecl.TypeParams.NumFields() > 0) {
        tparam0 = tdecl.TypeParams.List[0];
    }
    // alias declaration
    if (tdecl.Assign.IsValid()) {
        // Report highest version requirement first so that fixing a version issue
        // avoids possibly two -lang changes (first to Go 1.9 and then to Go 1.23).
        if (!versionErr && tparam0 != nil && !check.verifyVersionf(~tparam0, go1_23, "generic type alias"u8)) {
            versionErr = true;
        }
        if (!versionErr && !check.verifyVersionf(((atPos)tdecl.Assign), go1_9, "type alias"u8)) {
            versionErr = true;
        }
        if (check.conf._EnableAlias){
            // TODO(gri) Should be able to use nil instead of Typ[Invalid] to mark
            //           the alias as incomplete. Currently this causes problems
            //           with certain cycles. Investigate.
            //
            // NOTE(adonovan): to avoid the Invalid being prematurely observed
            // by (e.g.) a var whose type is an unfinished cycle,
            // Unalias does not memoize if Invalid. Perhaps we should use a
            // special sentinel distinct from Invalid.
            var alias = check.newAlias(Ꮡobj, ~Typ[Invalid]);
            setDefType(Ꮡdef, ~alias);
            // handle type parameters even if not allowed (Alias type is supported)
            if (tparam0 != nil) {
                if (!versionErr && !buildcfg.Experiment.AliasTypeParams) {
                    check.error(~tdecl, UnsupportedFeature, "generic type alias requires GOEXPERIMENT=aliastypeparams"u8);
                    versionErr = true;
                }
                check.openScope(~tdecl, "type parameters"u8);
                defer(check.closeScope);
                check.collectTypeParams(Ꮡ((~alias).tparams), tdecl.TypeParams);
            }
            rhs = check.definedType(tdecl.Type, Ꮡobj);
            assert(rhs != default!);
            alias.val.fromRHS = rhs;
            Unalias(~alias);
        } else {
            // resolve alias.actual
            // With Go1.23, the default behavior is to use Alias nodes,
            // reflected by check.enableAlias. Signal non-default behavior.
            //
            // TODO(gri) Testing runs tests in both modes. Do we need to exclude
            //           tracking of non-default behavior for tests?
            gotypesalias.IncNonDefault();
            if (!versionErr && tparam0 != nil) {
                check.error(~tdecl, UnsupportedFeature, "generic type alias requires GODEBUG=gotypesalias=1 or unset"u8);
                versionErr = true;
            }
            check.brokenAlias(Ꮡobj);
            rhs = check.typ(tdecl.Type);
            check.validAlias(Ꮡobj, rhs);
        }
        return;
    }
    // type definition or generic type declaration
    if (!versionErr && tparam0 != nil && !check.verifyVersionf(~tparam0, go1_18, "type parameter"u8)) {
        versionErr = true;
    }
    var named = check.newNamed(Ꮡobj, default!, default!);
    setDefType(Ꮡdef, ~named);
    if (tdecl.TypeParams != nil) {
        check.openScope(~tdecl, "type parameters"u8);
        defer(check.closeScope);
        check.collectTypeParams(Ꮡ((~named).tparams), tdecl.TypeParams);
    }
    // determine underlying type of named
    rhs = check.definedType(tdecl.Type, Ꮡobj);
    assert(rhs != default!);
    named.val.fromRHS = rhs;
    // If the underlying type was not set while type-checking the right-hand
    // side, it is invalid and an error should have been reported elsewhere.
    if ((~named).underlying == default!) {
        named.val.underlying = Typ[Invalid];
    }
    // Disallow a lone type parameter as the RHS of a type declaration (go.dev/issue/45639).
    // We don't need this restriction anymore if we make the underlying type of a type
    // parameter its constraint interface: if the RHS is a lone type parameter, we will
    // use its underlying type (like we do for any RHS in a type declaration), and its
    // underlying type is an interface and the type declaration is well defined.
    if (isTypeParam(rhs)) {
        check.error(tdecl.Type, MisplacedTypeParam, "cannot use a type parameter as RHS in type declaration"u8);
        named.val.underlying = Typ[Invalid];
    }
});

[GoRecv] public static void collectTypeParams(this ref Checker check, ж<ж<TypeParamList>> Ꮡdst, ж<ast.FieldList> Ꮡlist) => func((defer, _) => {
    ref var dst = ref Ꮡdst.val;
    ref var list = ref Ꮡlist.val;

    slice<ж<TypeParam>> tparams = default!;
    // Declare type parameters up-front, with empty interface as type bound.
    // The scope of type parameters starts at the beginning of the type parameter
    // list (so we can have mutually recursive parameterized interfaces).
    tokenꓸPos scopePos = list.Pos();
    foreach (var (_, f) in list.List) {
        tparams = check.declareTypeParams(tparams, (~f).Names, scopePos);
    }
    // Set the type parameters before collecting the type constraints because
    // the parameterized type may be used by the constraints (go.dev/issue/47887).
    // Example: type T[P T[P]] interface{}
    dst = bindTParams(tparams);
    // Signal to cycle detection that we are in a type parameter list.
    // We can only be inside one type parameter list at any given time:
    // function closures may appear inside a type parameter list but they
    // cannot be generic, and their bodies are processed in delayed and
    // sequential fashion. Note that with each new declaration, we save
    // the existing environment and restore it when done; thus inTPList is
    // true exactly only when we are in a specific type parameter list.
    assert(!check.inTParamList);
    check.inTParamList = true;
    defer(() => {
        check.inTParamList = false;
    });
    nint index = 0;
    foreach (var (_, f) in list.List) {
        ΔType bound = default!;
        // NOTE: we may be able to assert that f.Type != nil here, but this is not
        // an invariant of the AST, so we are cautious.
        if ((~f).Type != default!){
            bound = check.bound((~f).Type);
            if (isTypeParam(bound)) {
                // We may be able to allow this since it is now well-defined what
                // the underlying type and thus type set of a type parameter is.
                // But we may need some additional form of cycle detection within
                // type parameter lists.
                check.error((~f).Type, MisplacedTypeParam, "cannot use a type parameter as constraint"u8);
                bound = ~Typ[Invalid];
            }
        } else {
            bound = ~Typ[Invalid];
        }
        foreach (var (i, _) in (~f).Names) {
            tparams[index + i].val.bound = bound;
        }
        index += len((~f).Names);
    }
});

[GoRecv] internal static ΔType bound(this ref Checker check, ast.Expr x) {
    // A type set literal of the form ~T and A|B may only appear as constraint;
    // embed it in an implicit interface so that only interface type-checking
    // needs to take care of such type expressions.
    var wrap = false;
    switch (x.type()) {
    case ж<ast.UnaryExpr> op: {
        wrap = (~op).Op == token.TILDE;
        break;
    }
    case ж<ast.BinaryExpr> op: {
        wrap = (~op).Op == token.OR;
        break;
    }}
    if (wrap) {
        Ꮡx = new ast.InterfaceType(Methods: Ꮡ(new ast.FieldList(List: new ast.Field[]{new(ΔType: x)}.slice()))); x = ref Ꮡx.val;
        var t = check.typ(x);
        // mark t as implicit interface if all went well
        {
            var (tΔ1, _) = t._<Interface.val>(ᐧ); if (tΔ1 != nil) {
                tΔ1.val.@implicit = true;
            }
        }
        return t;
    }
    return check.typ(x);
}

[GoRecv] internal static slice<ж<TypeParam>> declareTypeParams(this ref Checker check, slice<ж<TypeParam>> tparams, slice<ast.Ident> names, tokenꓸPos scopePos) {
    // Use Typ[Invalid] for the type constraint to ensure that a type
    // is present even if the actual constraint has not been assigned
    // yet.
    // TODO(gri) Need to systematically review all uses of type parameter
    //           constraints to make sure we don't rely on them if they
    //           are not properly set yet.
    foreach (var (_, name) in names) {
        var tname = NewTypeName(name.Pos(), check.pkg, (~name).Name, default!);
        var tpar = check.newTypeParam(tname, ~Typ[Invalid]);
        // assigns type to tpar as a side-effect
        check.declare(check.scope, name, ~tname, scopePos);
        tparams = append(tparams, tpar);
    }
    if (check.conf._Trace && len(names) > 0) {
        check.trace(names[0].Pos(), "type params = %v"u8, tparams[(int)(len(tparams) - len(names))..]);
    }
    return tparams;
}

[GoRecv] public static void collectMethods(this ref Checker check, ж<TypeName> Ꮡobj) {
    ref var obj = ref Ꮡobj.val;

    // get associated methods
    // (Checker.collectObjects only collects methods with non-blank names;
    // Checker.resolveBaseTypeName ensures that obj is not an alias name
    // if it has attached methods.)
    var methods = check.methods[obj];
    if (methods == default!) {
        return;
    }
    delete(check.methods, Ꮡobj);
    assert(!check.objMap[obj].tdecl.Assign.IsValid());
    // don't use TypeName.IsAlias (requires fully set up object)
    // use an objset to check for name conflicts
    objset mset = default!;
    // spec: "If the base type is a struct type, the non-blank method
    // and field names must be distinct."
    var @base = asNamed(obj.typ);
    // shouldn't fail but be conservative
    if (@base != nil) {
        assert(@base.TypeArgs().Len() == 0);
        // collectMethods should not be called on an instantiated type
        // See go.dev/issue/52529: we must delay the expansion of underlying here, as
        // base may not be fully set-up.
        check.later(
        var baseʗ11 = @base;
        () => {
            check.checkFieldUniqueness(baseʗ11);
        }).describef(~obj, "verifying field uniqueness for %v"u8, @base);
        // Checker.Files may be called multiple times; additional package files
        // may add methods to already type-checked types. Add pre-existing methods
        // so that we can detect redeclarations.
        for (nint i = 0; i < @base.NumMethods(); i++) {
            var m = @base.Method(i);
            assert(m.name != "_"u8);
            assert(mset.insert(~m) == default!);
        }
    }
    // add valid methods
    foreach (var (_, m) in methods) {
        // spec: "For a base type, the non-blank names of methods bound
        // to it must be unique."
        assert(m.name != "_"u8);
        {
            var alt = mset.insert(~m); if (alt != default!) {
                if (alt.Pos().IsValid()){
                    check.errorf(~m, DuplicateMethod, "method %s.%s already declared at %v"u8, obj.Name(), m.name, alt.Pos());
                } else {
                    check.errorf(~m, DuplicateMethod, "method %s.%s already declared"u8, obj.Name(), m.name);
                }
                continue;
            }
        }
        if (@base != nil) {
            @base.AddMethod(m);
        }
    }
}

[GoRecv] public static void checkFieldUniqueness(this ref Checker check, ж<Named> Ꮡbase) {
    ref var @base = ref Ꮡbase.val;

    {
        var (t, _) = @base.under()._<Struct.val>(ᐧ); if (t != nil) {
            objset mset = default!;
            for (nint i = 0; i < @base.NumMethods(); i++) {
                var m = @base.Method(i);
                assert(m.name != "_"u8);
                assert(mset.insert(~m) == default!);
            }
            // Check that any non-blank field names of base are distinct from its
            // method names.
            foreach (var (_, fld) in (~t).fields) {
                if (fld.name != "_"u8) {
                    {
                        var alt = mset.insert(~fld); if (alt != default!) {
                            // Struct fields should already be unique, so we should only
                            // encounter an alternate via collision with a method name.
                            _ = alt._<Func.val>();
                            // For historical consistency, we report the primary error on the
                            // method, and the alt decl on the field.
                            var err = check.newError(DuplicateFieldAndMethod);
                            err.addf(alt, "field and method with the same name %s"u8, fld.name);
                            err.addAltDecl(~fld);
                            err.report();
                        }
                    }
                }
            }
        }
    }
}

[GoRecv] public static void funcDecl(this ref Checker check, ж<Func> Ꮡobj, ж<declInfo> Ꮡdecl) {
    ref var obj = ref Ꮡobj.val;
    ref var decl = ref Ꮡdecl.val;

    assert(obj.typ == default!);
    // func declarations cannot use iota
    assert(check.iota == default!);
    var sig = @new<ΔSignature>();
    obj.typ = sig;
    // guard against cycles
    // Avoid cycle error when referring to method while type-checking the signature.
    // This avoids a nuisance in the best case (non-parameterized receiver type) and
    // since the method is not a type, we get an error. If we have a parameterized
    // receiver type, instantiating the receiver type leads to the instantiation of
    // its methods, and we don't want a cycle error in that case.
    // TODO(gri) review if this is correct and/or whether we still need this?
    var saved = obj.color_;
    obj.color_ = black;
    var fdecl = decl.fdecl;
    check.funcType(sig, (~fdecl).Recv, (~fdecl).Type);
    obj.color_ = saved;
    // Set the scope's extent to the complete "func (...) { ... }"
    // so that Scope.Innermost works correctly.
    (~sig).scope.val.pos = fdecl.Pos();
    (~sig).scope.val.end = fdecl.End();
    if ((~(~fdecl).Type).TypeParams.NumFields() > 0 && (~fdecl).Body == nil) {
        check.softErrorf(~(~fdecl).Name, BadDecl, "generic function is missing function body"u8);
    }
    // function body must be type-checked after global declarations
    // (functions implemented elsewhere have no body)
    if (!check.conf.IgnoreFuncBodies && (~fdecl).Body != nil) {
        check.later(
        var fdeclʗ11 = fdecl;
        var sigʗ11 = sig;
        () => {
            check.funcBody(Ꮡdecl, obj.name, sigʗ11, (~fdeclʗ11).Body, default!);
        }).describef(~obj, "func %s"u8, obj.name);
    }
}

[GoRecv] internal static void declStmt(this ref Checker check, ast.Decl d) {
    var pkg = check.pkg;
    check.walkDecl(d, 
    var pkgʗ1 = pkg;
    (decl d) => {
        switch (d.type()) {
        case ΔconstDecl d: {
            nint top = len(check.delayed);
            var lhs = new slice<ж<Const>>(len((~d.spec).Names));
            foreach (var (i, name) in (~d.spec).Names) {
                // declare all constants
                var obj = NewConst(name.Pos(), pkgʗ1, (~name).Name, default!, constant.MakeInt64(((int64)d.iota)));
                lhs[i] = obj;
                ast.Expr init = default!;
                if (i < len(d.init)) {
                    init = d.init[i];
                }
                check.constDecl(obj, d.typ, init, d.inherited);
            }
            check.processDelayed(top);
            tokenꓸPos scopePos = d.spec.End();
            foreach (var (i, name) in (~d.spec).Names) {
                // process function literals in init expressions before scope changes
                // spec: "The scope of a constant or variable identifier declared
                // inside a function begins at the end of the ConstSpec or VarSpec
                // (ShortVarDecl for short variable declarations) and ends at the
                // end of the innermost containing block."
                check.declare(check.scope, name, ~lhs[i], scopePos);
            }
            break;
        }
        case ΔvarDecl d: {
            top = len(check.delayed);
            var lhs0 = new slice<ж<Var>>(len((~d.spec).Names));
            foreach (var (i, name) in (~d.spec).Names) {
                lhs0[i] = NewVar(name.Pos(), pkgʗ1, (~name).Name, default!);
            }
            foreach (var (i, obj) in lhs0) {
                // initialize all variables
                slice<ж<Var>> lhsΔ1 = default!;
                ast.Expr init = default!;
                var exprᴛ1 = len((~d.spec).Values);
                if (exprᴛ1 == len((~d.spec).Names)) {
                    init = (~d.spec).Values[i];
                }
                else if (exprᴛ1 is 1) {
                    lhsΔ1 = lhs0;
                    init = (~d.spec).Values[0];
                }
                else { /* default: */
                    if (i < len((~d.spec).Values)) {
                        // lhs and rhs match
                        // rhs is expected to be a multi-valued expression
                        init = (~d.spec).Values[i];
                    }
                }

                check.varDecl(obj, lhsΔ1, (~d.spec).Type, init);
                if (len((~d.spec).Values) == 1) {
                    // If we have a single lhs variable we are done either way.
                    // If we have a single rhs expression, it must be a multi-
                    // valued expression, in which case handling the first lhs
                    // variable will cause all lhs variables to have a type
                    // assigned, and we are done as well.
                    if (debug) {
                        foreach (var (_, objΔ1) in lhs0) {
                            assert(objΔ1.typ != default!);
                        }
                    }
                    break;
                }
            }
            check.processDelayed(top);
            scopePos = d.spec.End();
            foreach (var (i, name) in (~d.spec).Names) {
                // process function literals in init expressions before scope changes
                // declare all variables
                // (only at this point are the variable scopes (parents) set)
                // see constant declarations
                // see constant declarations
                check.declare(check.scope, name, ~lhs0[i], scopePos);
            }
            break;
        }
        case ΔtypeDecl d: {
            var obj = NewTypeName((~d.spec).Name.Pos(), pkgʗ1, (~(~d.spec).Name).Name, default!);
            scopePos = (~d.spec).Name.Pos();
            check.declare(check.scope, // spec: "The scope of a type identifier declared inside a function
 // begins at the identifier in the TypeSpec and ends at the end of
 // the innermost containing block."
 (~d.spec).Name, ~obj, scopePos);
            obj.setColor(grey + ((Δcolor)check.push(~obj)));
            check.typeDecl(obj, // mark and unmark type before calling typeDecl; its type is still nil (see Checker.objDecl)
 d.spec, nil);
            check.pop().setColor(black);
            break;
        }
        default: {
            var d = d.type();
            check.errorf(d.node(), InvalidSyntaxTree, "unknown ast.Decl node %T"u8, d.node());
            break;
        }}
    });
}

} // end types_package
