// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements type-checking of identifiers and type expressions.
namespace go.go;

using fmt = fmt_package;
using ast = go.ast_package;
using constant = go.constant_package;
using typeparams = go.@internal.typeparams_package;
using static @internal.types.errors_package;
using strings = strings_package;
using go.@internal;

partial class types_package {

// ident type-checks identifier e and initializes x with the value or type of e.
// If an error occurred, x.mode is set to invalid.
// For the meaning of def, see Checker.definedType, below.
// If wantType is set, the identifier e is expected to denote a type.
[GoRecv] public static void ident(this ref Checker check, ж<operand> Ꮡx, ж<ast.Ident> Ꮡe, ж<TypeName> Ꮡdef, bool wantType) {
    ref var x = ref Ꮡx.val;
    ref var e = ref Ꮡe.val;
    ref var def = ref Ꮡdef.val;

    x.mode = invalid;
    x.expr = e;
    // Note that we cannot use check.lookup here because the returned scope
    // may be different from obj.Parent(). See also Scope.LookupParent doc.
    (scope, obj) = check.scope.LookupParent(e.Name, check.pos);
    var exprᴛ1 = obj;
    if (exprᴛ1 == default!) {
        if (e.Name == "_"u8){
            // Blank identifiers are never declared, but the current identifier may
            // be a placeholder for a receiver type parameter. In this case we can
            // resolve its type and object from Checker.recvTParamMap.
            {
                var tpar = check.recvTParamMap[e]; if (tpar != nil){
                    x.mode = typexpr;
                    x.typ = tpar;
                } else {
                    check.error(~e, InvalidBlank, "cannot use _ as value or type"u8);
                }
            }
        } else {
            check.errorf(~e, UndeclaredName, "undefined: %s"u8, e.Name);
        }
        return;
    }
    if (exprᴛ1 == universeComparable) {
        if (!check.verifyVersionf(~e, go1_18, "predeclared %s"u8, e.Name)) {
            return;
        }
    }

    // avoid follow-on errors
    // Because the representation of any depends on gotypesalias, we don't check
    // pointer identity here.
    if (obj.Name() == "any"u8 && obj.Parent() == Universe) {
        if (!check.verifyVersionf(~e, go1_18, "predeclared %s"u8, e.Name)) {
            return;
        }
    }
    // avoid follow-on errors
    check.recordUse(Ꮡe, obj);
    // If we want a type but don't have one, stop right here and avoid potential problems
    // with missing underlying types. This also gives better error messages in some cases
    // (see go.dev/issue/65344).
    var (_, gotType) = obj._<TypeName.val>(ᐧ);
    if (!gotType && wantType) {
        check.errorf(~e, NotAType, "%s is not a type"u8, obj.Name());
        // avoid "declared but not used" errors
        // (don't use Checker.use - we don't want to evaluate too much)
        {
            var (v, _) = obj._<Var.val>(ᐧ); if (v != nil && v.pkg == check.pkg) {
                /* see Checker.use1 */
                v.val.used = true;
            }
        }
        return;
    }
    // Type-check the object.
    // Only call Checker.objDecl if the object doesn't have a type yet
    // (in which case we must actually determine it) or the object is a
    // TypeName and we also want a type (in which case we might detect
    // a cycle which needs to be reported). Otherwise we can skip the
    // call and avoid a possible cycle error in favor of the more
    // informative "not a type/value" error that this function's caller
    // will issue (see go.dev/issue/25790).
    var typ = obj.Type();
    if (typ == default! || gotType && wantType) {
        check.objDecl(obj, Ꮡdef);
        typ = obj.Type();
    }
    // type must have been assigned by Checker.objDecl
    assert(typ != default!);
    // The object may have been dot-imported.
    // If so, mark the respective package as used.
    // (This code is only needed for dot-imports. Without them,
    // we only have to mark variables, see *Var case below).
    {
        var pkgName = check.dotImportMap[new dotImportKey(scope, obj.Name())]; if (pkgName != nil) {
            pkgName.val.used = true;
        }
    }
    switch (obj.type()) {
    case PkgName.val obj: {
        check.errorf(~e, InvalidPkgUse, "use of package %s not in selector"u8, obj.name);
        return;
    }
    case Const.val obj: {
        check.addDeclDep(~obj);
        if (!isValid(typ)) {
            return;
        }
        if (~obj == ᏑuniverseIota){
            if (check.iota == default!) {
                check.error(~e, InvalidIota, "cannot use iota outside constant declaration"u8);
                return;
            }
            x.val = check.iota;
        } else {
            x.val = obj.val.val;
        }
        assert(x.val != default!);
        x.mode = constant_;
        break;
    }
    case TypeName.val obj: {
        if (!check.conf._EnableAlias && check.isBrokenAlias(obj)) {
            check.errorf(~e, InvalidDeclCycle, "invalid use of type alias %s in recursive type (see go.dev/issue/50729)"u8, obj.name);
            return;
        }
        x.mode = typexpr;
        break;
    }
    case Var.val obj: {
        if (obj.pkg == check.pkg) {
            // It's ok to mark non-local variables, but ignore variables
            // from other packages to avoid potential race conditions with
            // dot-imported variables.
            obj.val.used = true;
        }
        check.addDeclDep(~obj);
        if (!isValid(typ)) {
            return;
        }
        x.mode = variable;
        break;
    }
    case Func.val obj: {
        check.addDeclDep(~obj);
        x.mode = value;
        break;
    }
    case Builtin.val obj: {
        x.id = obj.val.id;
        x.mode = Δbuiltin;
        break;
    }
    case Nil.val obj: {
        x.mode = value;
        break;
    }
    default: {
        var obj = obj.type();
        throw panic("unreachable");
        break;
    }}
    x.typ = typ;
}

// typ type-checks the type expression e and returns its type, or Typ[Invalid].
// The type must not be an (uninstantiated) generic type.
[GoRecv] internal static ΔType typ(this ref Checker check, ast.Expr e) {
    return check.definedType(e, nil);
}

// varType type-checks the type expression e and returns its type, or Typ[Invalid].
// The type must not be an (uninstantiated) generic type and it must not be a
// constraint interface.
[GoRecv] internal static ΔType varType(this ref Checker check, ast.Expr e) {
    var typ = check.definedType(e, nil);
    check.validVarType(e, typ);
    return typ;
}

// validVarType reports an error if typ is a constraint interface.
// The expression e is used for error reporting, if any.
[GoRecv] internal static void validVarType(this ref Checker check, ast.Expr e, ΔType typ) {
    // If we have a type parameter there's nothing to do.
    if (isTypeParam(typ)) {
        return;
    }
    // We don't want to call under() or complete interfaces while we are in
    // the middle of type-checking parameter declarations that might belong
    // to interface methods. Delay this check to the end of type-checking.
    check.later(() => {
        {
            var (t, _) = under(typ)._<Interface.val>(ᐧ); if (t != nil) {
                var tset = computeInterfaceTypeSet(check, e.Pos(), t);
                if (!tset.IsMethodSet()) {
                    if ((~tset).comparable){
                        check.softErrorf(e, MisplacedConstraintIface, "cannot use type %s outside a type constraint: interface is (or embeds) comparable"u8, typ);
                    } else {
                        check.softErrorf(e, MisplacedConstraintIface, "cannot use type %s outside a type constraint: interface contains type constraints"u8, typ);
                    }
                }
            }
        }
    }).describef(e, "check var type %s"u8, typ);
}

// definedType is like typ but also accepts a type name def.
// If def != nil, e is the type specification for the type named def, declared
// in a type declaration, and def.typ.underlying will be set to the type of e
// before any components of e are type-checked.
[GoRecv] public static ΔType definedType(this ref Checker check, ast.Expr e, ж<TypeName> Ꮡdef) {
    ref var def = ref Ꮡdef.val;

    var typ = check.typInternal(e, Ꮡdef);
    assert(isTyped(typ));
    if (isGeneric(typ)) {
        check.errorf(e, WrongTypeArgCount, "cannot use generic type %s without instantiation"u8, typ);
        typ = ~Typ[Invalid];
    }
    check.recordTypeAndValue(e, typexpr, typ, default!);
    return typ;
}

// genericType is like typ but the type must be an (uninstantiated) generic
// type. If cause is non-nil and the type expression was a valid type but not
// generic, cause will be populated with a message describing the error.
[GoRecv] public static ΔType genericType(this ref Checker check, ast.Expr e, ж<@string> Ꮡcause) {
    ref var cause = ref Ꮡcause.val;

    var typ = check.typInternal(e, nil);
    assert(isTyped(typ));
    if (isValid(typ) && !isGeneric(typ)) {
        if (cause != nil) {
            cause = check.sprintf("%s is not a generic type"u8, typ);
        }
        typ = ~Typ[Invalid];
    }
    // TODO(gri) what is the correct call below?
    check.recordTypeAndValue(e, typexpr, typ, default!);
    return typ;
}

// goTypeName returns the Go type name for typ and
// removes any occurrences of "types." from that name.
internal static @string goTypeName(ΔType typ) {
    return strings.ReplaceAll(fmt.Sprintf("%T"u8, typ), "types."u8, ""u8);
}

// typInternal drives type checking of types.
// Must only be called by definedType or genericType.
[GoRecv] public static ΔType /*T*/ typInternal(this ref Checker check, ast.Expr e0, ж<TypeName> Ꮡdef) => func((defer, _) => {
    ΔType T = default!;

    ref var def = ref Ꮡdef.val;
    if (check.conf._Trace) {
        check.trace(e0.Pos(), "-- type %s"u8, e0);
        check.indent++;
        defer(() => {
            check.indent--;
            ΔType under = default!;
            if (T != default!) {
                // Calling under() here may lead to endless instantiations.
                // Test case: type T[P any] *T[P]
                under = safeUnderlying(T);
            }
            if (AreEqual(T, under)){
                check.trace(e0.Pos(), "=> %s // %s"u8, T, goTypeName(T));
            } else {
                check.trace(e0.Pos(), "=> %s (under = %s) // %s"u8, T, under, goTypeName(T));
            }
        });
    }
    switch (e0.type()) {
    case ж<ast.BadExpr> e: {
        break;
    }
    case ж<ast.Ident> e: {
// ignore - error reported before
        ref var xΔ1 = ref heap(new operand(), out var ᏑxΔ1);
        check.ident(ᏑxΔ1, e, Ꮡdef, true);
        var exprᴛ1 = xΔ1.mode;
        if (exprᴛ1 == typexpr) {
            var typΔ2 = xΔ1.typ;
            setDefType(Ꮡdef, typΔ2);
            return typΔ2;
        }
        if (exprᴛ1 == invalid) {
        }
        if (exprᴛ1 == novalue) {
            check.errorf(~ᏑxΔ1, // ignore - error reported before
 NotAType, "%s used as type"u8, ᏑxΔ1);
        }
        else { /* default: */
            check.errorf(~ᏑxΔ1, NotAType, "%s is not a type"u8, ᏑxΔ1);
        }

        break;
    }
    case ж<ast.SelectorExpr> e: {
        ref var x = ref heap(new operand(), out var Ꮡx);
        check.selector(Ꮡx, e, Ꮡdef, true);
        var exprᴛ2 = x.mode;
        if (exprᴛ2 == typexpr) {
            var typΔ4 = x.typ;
            setDefType(Ꮡdef, typΔ4);
            return typΔ4;
        }
        if (exprᴛ2 == invalid) {
        }
        if (exprᴛ2 == novalue) {
            check.errorf(~Ꮡx, // ignore - error reported before
 NotAType, "%s used as type"u8, Ꮡx);
        }
        else { /* default: */
            check.errorf(~Ꮡx, NotAType, "%s is not a type"u8, Ꮡx);
        }

        break;
    }
    case ж<ast.IndexExpr> e: {
        var ix = typeparams.UnpackIndexExpr(e);
        check.verifyVersionf(inNode(e, (~ix).Lbrack), go1_18, "type instantiation"u8);
        return check.instantiatedType(ix, Ꮡdef);
    }
    case ж<ast.IndexListExpr> e: {
        var ix = typeparams.UnpackIndexExpr(e);
        check.verifyVersionf(inNode(e, (~ix).Lbrack), go1_18, "type instantiation"u8);
        return check.instantiatedType(ix, Ꮡdef);
    }
    case ж<ast.ParenExpr> e: {
        return check.definedType((~e).X, // Generic types must be instantiated before they can be used in any form.
 // Consequently, generic types cannot be parenthesized.
 Ꮡdef);
    }
    case ж<ast.ArrayType> e: {
        if ((~e).Len == default!) {
            var typΔ5 = @new<Slice>();
            setDefType(Ꮡdef, ~typΔ5);
            .val.elem = check.varType((~e).Elt);
            return ~typΔ5;
        }
        var typΔ6 = @new<Array>();
        setDefType(Ꮡdef, ~typΔ6);
        {
            var (_, ok) = (~e).Len._<ж<ast.Ellipsis>>(ᐧ); if (ok){
                // Provide a more specific error when encountering a [...] array
                // rather than leaving it to the handling of the ... expression.
                check.error((~e).Len, BadDotDotDotSyntax, "invalid use of [...] array (outside a composite literal)"u8);
                .val.len = -1;
            } else {
                .val.len = check.arrayLength((~e).Len);
            }
        }
        .val.elem = check.varType((~e).Elt);
        if ((~typΔ6).len >= 0) {
            return ~typΔ6;
        }
        break;
    }
    case ж<ast.Ellipsis> e: {
        check.error(~e, // report error if we encountered [...]
 // dots are handled explicitly where they are legal
 // (array composite literals and parameter lists)
 InvalidDotDotDot, "invalid use of '...'"u8);
        check.use((~e).Elt);
        break;
    }
    case ж<ast.StructType> e: {
         = @new<Struct>();
        setDefType(Ꮡdef, ~typ);
        check.structType(typ, e);
        return ~typ;
    }
    case ж<ast.StarExpr> e: {
         = @new<Pointer>();
        .val.@base = Typ[Invalid];
        setDefType(Ꮡdef, // avoid nil base in invalid recursive type declaration
 ~typ);
        .val.@base = check.varType((~e).X);
        return ~typ;
    }
    case ж<ast.FuncType> e: {
         = @new<ΔSignature>();
        setDefType(Ꮡdef, ~typ);
        check.funcType(typ, nil, e);
        return ~typ;
    }
    case ж<ast.InterfaceType> e: {
         = check.newInterface();
        setDefType(Ꮡdef, ~typ);
        check.interfaceType(typ, e, Ꮡdef);
        return ~typ;
    }
    case ж<ast.MapType> e: {
         = @new<Map>();
        setDefType(Ꮡdef, ~typ);
        .val.key = check.varType((~e).Key);
        .val.elem = check.varType((~e).Value);
        check.later(() => {
            if (!Comparable((~typ).key)) {
                @string why = default!;
                if (isTypeParam((~typ).key)) {
                    why = " (missing comparable constraint)"u8;
                }
                check.errorf((~e).Key, IncomparableMapKey, "invalid map key type %s%s"u8, (~typ).key, why);
            }
        }).describef((~e).Key, "check map key %s"u8, (~typ).key);
        return ~typ;
    }
    case ж<ast.ChanType> e: {
         = @new<Chan>();
        setDefType(Ꮡdef, ~typ);
        ChanDir dir = SendRecv;
        var exprᴛ3 = (~e).Dir;
        if (exprᴛ3 == (ast.ChanDir)(ast.SEND | ast.RECV)) {
        }
        else if (exprᴛ3 == ast.SEND) {
            dir = SendOnly;
        }
        else if (exprᴛ3 == ast.RECV) {
            dir = RecvOnly;
        }
        else { /* default: */
            check.errorf(~e, // nothing to do
 InvalidSyntaxTree, "unknown channel direction %d"u8, (~e).Dir);
        }

        .val.dir = dir;
        .val.elem = check.varType((~e).Value);
        return ~typ;
    }
    default: {
        var e = e0.type();
        check.errorf(e0, // ok to continue
 NotAType, "%s is not a type"u8, e0);
        check.use(e0);
        break;
    }}
    var typ = Typ[Invalid];
    setDefType(Ꮡdef, ~typ);
    return ~typ;
});

internal static void setDefType(ж<TypeName> Ꮡdef, ΔType typ) {
    ref var def = ref Ꮡdef.val;

    if (def != nil) {
        switch (def.typ.type()) {
        case Alias.val t: {
            if ((~t).fromRHS != ~Typ[Invalid] && !AreEqual((~t).fromRHS, typ)) {
                // t.fromRHS should always be set, either to an invalid type
                // in the beginning, or to typ in certain cyclic declarations.
                throw panic(sprintf(nil, default!, true, "t.fromRHS = %s, typ = %s\n"u8, (~t).fromRHS, typ));
            }
            var t.val.fromRHS = typ;
            break;
        }
        case Basic.val t: {
            assert(t == Typ[Invalid]);
            break;
        }
        case Named.val t: {
            var t.val.underlying = typ;
            break;
        }
        default: {
            var t = def.typ.type();
            throw panic(fmt.Sprintf("unexpected type %T"u8, t));
            break;
        }}
    }
}

[GoRecv] public static ΔType /*res*/ instantiatedType(this ref Checker check, ж<typeparams.IndexExpr> Ꮡix, ж<TypeName> Ꮡdef) => func((defer, _) => {
    ΔType res = default!;

    ref var ix = ref Ꮡix.val;
    ref var def = ref Ꮡdef.val;
    if (check.conf._Trace) {
        check.trace(ix.Pos(), "-- instantiating type %s with %s"u8, ix.X, ix.Indices);
        check.indent++;
        defer(() => {
            check.indent--;
            // Don't format the underlying here. It will always be nil.
            check.trace(ix.Pos(), "=> %s"u8, res);
        });
    }
    defer(() => {
        setDefType(Ꮡdef, res);
    });
    ref var cause = ref heap(new @string(), out var Ꮡcause);
    var gtyp = check.genericType(ix.X, Ꮡcause);
    if (cause != ""u8) {
        check.errorf(ix.Orig, NotAGenericType, invalidOp + "%s (%s)", ix.Orig, cause);
    }
    if (!isValid(gtyp)) {
        return gtyp;
    }
    // error already reported
    // evaluate arguments
    var targs = check.typeList(ix.Indices);
    if (targs == default!) {
        return ~Typ[Invalid];
    }
    {
        var (origΔ1, _) = gtyp._<Alias.val>(ᐧ); if (origΔ1 != nil) {
            return check.instance(ix.Pos(), ~origΔ1, targs, nil, check.context());
        }
    }
    var orig = asNamed(gtyp);
    if (orig == nil) {
        throw panic(fmt.Sprintf("%v: cannot instantiate %v"u8, ix.Pos(), gtyp));
    }
    // create the instance
    var inst = asNamed(check.instance(ix.Pos(), ~orig, targs, nil, check.context()));
    // orig.tparams may not be set up, so we need to do expansion later.
    check.later(
    var instʗ11 = inst;
    () => {
        check.recordInstance(ix.Orig, instʗ11.TypeArgs().list(), ~instʗ11);
        if (check.validateTArgLen(ix.Pos(), (~instʗ11).obj.name, instʗ11.TypeParams().Len(), instʗ11.TypeArgs().Len())) {
            {
                var (i, err) = check.verify(ix.Pos(), instʗ11.TypeParams().list(), instʗ11.TypeArgs().list(), check.context()); if (err != default!){
                    tokenꓸPos pos = ix.Pos();
                    if (i < len(ix.Indices)) {
                        pos = ix.Indices[i].Pos();
                    }
                    check.softErrorf(((atPos)pos), InvalidTypeArg, "%v"u8, err);
                } else {
                    check.mono.recordInstance(check.pkg, ix.Pos(), instʗ11.TypeParams().list(), instʗ11.TypeArgs().list(), ix.Indices);
                }
            }
        }
        check.validType(instʗ11);
    }).describef(~ix, "resolve instance %s"u8, inst);
    return ~inst;
});

// arrayLength type-checks the array length expression e
// and returns the constant length >= 0, or a value < 0
// to indicate an error (and thus an unknown length).
[GoRecv] internal static int64 arrayLength(this ref Checker check, ast.Expr e) {
    // If e is an identifier, the array declaration might be an
    // attempt at a parameterized type declaration with missing
    // constraint. Provide an error message that mentions array
    // length.
    {
        var (name, _) = e._<ж<ast.Ident>>(ᐧ); if (name != nil) {
            var obj = check.lookup((~name).Name);
            if (obj == default!) {
                check.errorf(~name, InvalidArrayLen, "undefined array length %s or missing type constraint"u8, (~name).Name);
                return -1;
            }
            {
                var (_, ok) = obj._<Const.val>(ᐧ); if (!ok) {
                    check.errorf(~name, InvalidArrayLen, "invalid array length %s"u8, (~name).Name);
                    return -1;
                }
            }
        }
    }
    ref var x = ref heap(new operand(), out var Ꮡx);
    check.expr(nil, Ꮡx, e);
    if (x.mode != constant_) {
        if (x.mode != invalid) {
            check.errorf(~Ꮡx, InvalidArrayLen, "array length %s must be constant"u8, Ꮡx);
        }
        return -1;
    }
    if (isUntyped(x.typ) || isInteger(x.typ)) {
        {
            var val = constant.ToInt(x.val); if (val.Kind() == constant.Int) {
                if (representableConst(val, check, Typ[Int], nil)) {
                    {
                        var (n, ok) = constant.Int64Val(val); if (ok && n >= 0) {
                            return n;
                        }
                    }
                }
            }
        }
    }
    @string msg = default!;
    if (isInteger(x.typ)){
        msg = "invalid array length %s"u8;
    } else {
        msg = "array length %s must be integer"u8;
    }
    check.errorf(~Ꮡx, InvalidArrayLen, msg, Ꮡx);
    return -1;
}

// typeList provides the list of types corresponding to the incoming expression list.
// If an error occurred, the result is nil, but all list elements were type-checked.
[GoRecv] internal static slice<ΔType> typeList(this ref Checker check, slice<ast.Expr> list) {
    var res = new slice<ΔType>(len(list));
    // res != nil even if len(list) == 0
    foreach (var (i, x) in list) {
        var t = check.varType(x);
        if (!isValid(t)) {
            res = default!;
        }
        if (res != default!) {
            res[i] = t;
        }
    }
    return res;
}

} // end types_package
