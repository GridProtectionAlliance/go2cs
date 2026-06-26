// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using ast = go.ast_package;
using token = go.token_package;
using strings = strings_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸж<ast.Ident> = Span<ж<ast.Ident>>;

partial class parser_package {

internal const bool debugResolve = false;

[GoType("dyn")] partial interface resolveFile_type {
    tokenꓸPos Pos();
}

// resolveFile walks the given file to resolve identifiers within the file
// scope, updating ast.Ident.Obj fields with declaration information.
//
// If declErr is non-nil, it is used to report declaration errors during
// resolution. tok is used to format position in error messages.
internal static void resolveFile(ж<ast.File> Ꮡfile, ж<tokenꓸFile> Ꮡhandle, token.Pos, string) declErr) {
    ref var file = ref Ꮡfile.val;
    ref var handle = ref Ꮡhandle.val;

    var pkgScope = ast.NewScope(nil);
    var r = Ꮡ(new resolver(
        handle: handle,
        declErr: declErr,
        topScope: pkgScope,
        pkgScope: pkgScope,
        depth: 1
    ));
    foreach (var (_, decl) in file.Decls) {
        ast.Walk(~r, decl);
    }
    r.closeScope();
    assert((~r).topScope == nil, "unbalanced scopes"u8);
    assert((~r).labelScope == nil, "unbalanced label scopes"u8);
    // resolve global identifiers within the same file
    nint i = 0;
    foreach (var (_, ident) in (~r).unresolved) {
        // i <= index for current ident
        assert((~ident).Obj == unresolved, "object already resolved"u8);
        ident.val.Obj = (~r).pkgScope.Lookup((~ident).Name);
        // also removes unresolved sentinel
        if ((~ident).Obj == nil){
            (~r).unresolved[i] = ident;
            i++;
        } else 
        if (debugResolve) {
            tokenꓸPos pos = (~(~ident).Obj).Decl._<resolveFile_type>().Pos();
            r.trace("resolved %s@%v to package object %v"u8, (~ident).Name, ident.Pos(), pos);
        }
    }
    file.Scope = r.val.pkgScope;
    file.Unresolved = (~r).unresolved[0..(int)(i)];
}

internal const nint maxScopeDepth = 1000;

[GoType] partial struct resolver {
    internal ж<go.token_package.ΔFile> handle;
    internal token.Pos, string) declErr;
    // Ordinary identifier scopes
    internal ж<go.ast_package.Scope> pkgScope; // pkgScope.Outer == nil
    internal ж<go.ast_package.Scope> topScope; // top-most scope; may be pkgScope
    internal ast.Ident unresolved; // unresolved identifiers
    internal nint depth;         // scope depth
    // Label scopes
    // (maintained by open/close LabelScope)
    internal ж<go.ast_package.Scope> labelScope;  // label scope for current function
    internal ast.Ident targetStack; // stack of unresolved labels
}

[GoRecv] internal static void trace(this ref resolver r, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    fmt.Println(strings.Repeat(". "u8, r.depth) + r.sprintf(format, args.ꓸꓸꓸ));
}

[GoRecv] internal static @string sprintf(this ref resolver r, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    foreach (var (i, arg) in args) {
        switch (arg.type()) {
        case tokenꓸPos arg: {
            args[i] = r.handle.Position(arg);
            break;
        }}
    }
    return fmt.Sprintf(format, args.ꓸꓸꓸ);
}

[GoRecv] internal static void openScope(this ref resolver r, tokenꓸPos pos) {
    r.depth++;
    if (r.depth > maxScopeDepth) {
        throw panic(new bailout(pos: pos, msg: "exceeded max scope depth during object resolution"u8));
    }
    if (debugResolve) {
        r.trace("opening scope @%v"u8, pos);
    }
    r.topScope = ast.NewScope(r.topScope);
}

[GoRecv] internal static void closeScope(this ref resolver r) {
    r.depth--;
    if (debugResolve) {
        r.trace("closing scope"u8);
    }
    r.topScope = r.topScope.Outer;
}

[GoRecv] internal static void openLabelScope(this ref resolver r) {
    r.labelScope = ast.NewScope(r.labelScope);
    r.targetStack = append(r.targetStack, default!);
}

[GoRecv] internal static void closeLabelScope(this ref resolver r) {
    // resolve labels
    nint n = len(r.targetStack) - 1;
    var scope = r.labelScope;
    foreach (var (_, ident) in r.targetStack[n]) {
        ident.val.Obj = scope.Lookup((~ident).Name);
        if ((~ident).Obj == nil && r.declErr != default!) {
            r.declErr(ident.Pos(), fmt.Sprintf("label %s undefined"u8, (~ident).Name));
        }
    }
    // pop label scope
    r.targetStack = r.targetStack[0..(int)(n)];
    r.labelScope = r.labelScope.Outer;
}

[GoRecv] internal static void declare(this ref resolver r, any decl, any data, ж<ast.Scope> Ꮡscope, ast.ObjKind kind, params ꓸꓸꓸж<ast.Ident> identsʗp) {
    var idents = identsʗp.slice();

    ref var scope = ref Ꮡscope.val;
    foreach (var (_, ident) in idents) {
        if ((~ident).Obj != nil) {
            throw panic(fmt.Sprintf("%v: identifier %s already declared or resolved"u8, ident.Pos(), (~ident).Name));
        }
        var obj = ast.NewObj(kind, (~ident).Name);
        // remember the corresponding declaration for redeclaration
        // errors and global variable resolution/typechecking phase
        obj.val.Decl = decl;
        obj.val.Data = data;
        // Identifiers (for receiver type parameters) are written to the scope, but
        // never set as the resolved object. See go.dev/issue/50956.
        {
            var (_, ok) = decl._<ж<ast.Ident>>(ᐧ); if (!ok) {
                ident.val.Obj = obj;
            }
        }
        if ((~ident).Name != "_"u8) {
            if (debugResolve) {
                r.trace("declaring %s@%v"u8, (~ident).Name, ident.Pos());
            }
            {
                var alt = scope.Insert(obj); if (alt != nil && r.declErr != default!) {
                    @string prevDecl = ""u8;
                    {
                        tokenꓸPos pos = alt.Pos(); if (pos.IsValid()) {
                            prevDecl = r.sprintf("\n\tprevious declaration at %v"u8, pos);
                        }
                    }
                    r.declErr(ident.Pos(), fmt.Sprintf("%s redeclared in this block%s"u8, (~ident).Name, prevDecl));
                }
            }
        }
    }
}

[GoRecv] internal static void shortVarDecl(this ref resolver r, ж<ast.AssignStmt> Ꮡdecl) {
    ref var decl = ref Ꮡdecl.val;

    // Go spec: A short variable declaration may redeclare variables
    // provided they were originally declared in the same block with
    // the same type, and at least one of the non-blank variables is new.
    nint n = 0;
    // number of new variables
    foreach (var (_, x) in decl.Lhs) {
        {
            var (ident, isIdent) = x._<ж<ast.Ident>>(ᐧ); if (isIdent) {
                assert((~ident).Obj == nil, "identifier already declared or resolved"u8);
                var obj = ast.NewObj(ast.Var, (~ident).Name);
                // remember corresponding assignment for other tools
                obj.val.Decl = decl;
                ident.val.Obj = obj;
                if ((~ident).Name != "_"u8) {
                    if (debugResolve) {
                        r.trace("declaring %s@%v"u8, (~ident).Name, ident.Pos());
                    }
                    {
                        var alt = r.topScope.Insert(obj); if (alt != nil){
                            ident.val.Obj = alt;
                        } else {
                            // redeclaration
                            n++;
                        }
                    }
                }
            }
        }
    }
    // new declaration
    if (n == 0 && r.declErr != default!) {
        r.declErr(decl.Lhs[0].Pos(), "no new variables on left side of :=");
    }
}

// The unresolved object is a sentinel to mark identifiers that have been added
// to the list of unresolved identifiers. The sentinel is only used for verifying
// internal consistency.
internal static ж<ast.Object> unresolved = @new<ast.Object>();

// If x is an identifier, resolve attempts to resolve x by looking up
// the object it denotes. If no object is found and collectUnresolved is
// set, x is marked as unresolved and collected in the list of unresolved
// identifiers.
[GoRecv] internal static void resolve(this ref resolver r, ж<ast.Ident> Ꮡident, bool collectUnresolved) {
    ref var ident = ref Ꮡident.val;

    if (ident.Obj != nil) {
        throw panic(r.sprintf("%v: identifier %s already declared or resolved"u8, ident.Pos(), ident.Name));
    }
    // '_' should never refer to existing declarations, because it has special
    // handling in the spec.
    if (ident.Name == "_"u8) {
        return;
    }
    for (var s = r.topScope; s != nil; s = s.val.Outer) {
        {
            var obj = s.Lookup(ident.Name); if (obj != nil) {
                if (debugResolve) {
                    r.trace("resolved %v:%s to %v"u8, ident.Pos(), ident.Name, obj);
                }
                assert((~obj).Name != ""u8, "obj with no name"u8);
                // Identifiers (for receiver type parameters) are written to the scope,
                // but never set as the resolved object. See go.dev/issue/50956.
                {
                    var (_, ok) = (~obj).Decl._<ж<ast.Ident>>(ᐧ); if (!ok) {
                        ident.Obj = obj;
                    }
                }
                return;
            }
        }
    }
    // all local scopes are known, so any unresolved identifier
    // must be found either in the file scope, package scope
    // (perhaps in another file), or universe scope --- collect
    // them so that they can be resolved later
    if (collectUnresolved) {
        ident.Obj = unresolved;
        r.unresolved = append(r.unresolved, Ꮡident);
    }
}

[GoRecv] internal static void walkExprs(this ref resolver r, slice<ast.Expr> list) {
    foreach (var (_, node) in list) {
        ast.Walk(~r, node);
    }
}

[GoRecv] internal static void walkLHS(this ref resolver r, slice<ast.Expr> list) {
    foreach (var (_, expr) in list) {
        var exprΔ1 = ast.Unparen(expr);
        {
            var (_, ok) = expr._<ж<ast.Ident>>(ᐧ); if (!ok && exprΔ1 != default!) {
                ast.Walk(~r, exprΔ1);
            }
        }
    }
}

[GoRecv] internal static void walkStmts(this ref resolver r, slice<ast.Stmt> list) {
    foreach (var (_, stmt) in list) {
        ast.Walk(~r, stmt);
    }
}

[GoRecv("capture")] internal static ast.Visitor Visit(this ref resolver r, ast.Node node) => func((defer, _) => {
    if (debugResolve && node != default!) {
        r.trace("node %T@%v"u8, node, node.Pos());
    }
    switch (node.type()) {
    case ж<ast.Ident> n: {
        r.resolve(n, // Expressions.
 true);
        break;
    }
    case ж<ast.FuncLit> n: {
        r.openScope(n.Pos());
        defer(r.closeScope);
        r.walkFuncType((~n).Type);
        r.walkBody((~n).Body);
        break;
    }
    case ж<ast.SelectorExpr> n: {
        ast.Walk(~r, (~n).X);
        break;
    }
    case ж<ast.StructType> n: {
        r.openScope(n.Pos());
        defer(r.closeScope);
        r.walkFieldList((~n).Fields, // Note: don't try to resolve n.Sel, as we don't support qualified
 // resolution.
 ast.Var);
        break;
    }
    case ж<ast.FuncType> n: {
        r.openScope(n.Pos());
        defer(r.closeScope);
        r.walkFuncType(n);
        break;
    }
    case ж<ast.CompositeLit> n: {
        if ((~n).Type != default!) {
            ast.Walk(~r, (~n).Type);
        }
        foreach (var (_, e) in (~n).Elts) {
            {
                var (kv, _) = e._<ж<ast.KeyValueExpr>>(ᐧ); if (kv != nil){
                    // See go.dev/issue/45160: try to resolve composite lit keys, but don't
                    // collect them as unresolved if resolution failed. This replicates
                    // existing behavior when resolving during parsing.
                    {
                        var (ident, _) = (~kv).Key._<ж<ast.Ident>>(ᐧ); if (ident != nil){
                            r.resolve(ident, false);
                        } else {
                            ast.Walk(~r, (~kv).Key);
                        }
                    }
                    ast.Walk(~r, (~kv).Value);
                } else {
                    ast.Walk(~r, e);
                }
            }
        }
        break;
    }
    case ж<ast.InterfaceType> n: {
        r.openScope(n.Pos());
        defer(r.closeScope);
        r.walkFieldList((~n).Methods, ast.Fun);
        break;
    }
    case ж<ast.LabeledStmt> n: {
        r.declare(n, // Statements
 default!, r.labelScope, ast.Lbl, (~n).Label);
        ast.Walk(~r, (~n).Stmt);
        break;
    }
    case ж<ast.AssignStmt> n: {
        r.walkExprs((~n).Rhs);
        if ((~n).Tok == token.DEFINE){
            r.shortVarDecl(n);
        } else {
            r.walkExprs((~n).Lhs);
        }
        break;
    }
    case ж<ast.BranchStmt> n: {
        if ((~n).Tok != token.FALLTHROUGH && (~n).Label != nil) {
            // add to list of unresolved targets
            nint depth = len(r.targetStack) - 1;
            r.targetStack[depth] = append(r.targetStack[depth], (~n).Label);
        }
        break;
    }
    case ж<ast.BlockStmt> n: {
        r.openScope(n.Pos());
        defer(r.closeScope);
        r.walkStmts((~n).List);
        break;
    }
    case ж<ast.IfStmt> n: {
        r.openScope(n.Pos());
        defer(r.closeScope);
        if ((~n).Init != default!) {
            ast.Walk(~r, (~n).Init);
        }
        ast.Walk(~r, (~n).Cond);
        ast.Walk(~r, ~(~n).Body);
        if ((~n).Else != default!) {
            ast.Walk(~r, (~n).Else);
        }
        break;
    }
    case ж<ast.CaseClause> n: {
        r.walkExprs((~n).List);
        r.openScope(n.Pos());
        defer(r.closeScope);
        r.walkStmts((~n).Body);
        break;
    }
    case ж<ast.SwitchStmt> n: {
        r.openScope(n.Pos());
        defer(r.closeScope);
        if ((~n).Init != default!) {
            ast.Walk(~r, (~n).Init);
        }
        if ((~n).Tag != default!) {
            // The scope below reproduces some unnecessary behavior of the parser,
            // opening an extra scope in case this is a type switch. It's not needed
            // for expression switches.
            // TODO: remove this once we've matched the parser resolution exactly.
            if ((~n).Init != default!) {
                r.openScope((~n).Tag.Pos());
                defer(r.closeScope);
            }
            ast.Walk(~r, (~n).Tag);
        }
        if ((~n).Body != nil) {
            r.walkStmts((~(~n).Body).List);
        }
        break;
    }
    case ж<ast.TypeSwitchStmt> n: {
        if ((~n).Init != default!) {
            r.openScope(n.Pos());
            defer(r.closeScope);
            ast.Walk(~r, (~n).Init);
        }
        r.openScope((~n).Assign.Pos());
        defer(r.closeScope);
        ast.Walk(~r, (~n).Assign);
        if ((~n).Body != nil) {
            // s.Body consists only of case clauses, so does not get its own
            // scope.
            r.walkStmts((~(~n).Body).List);
        }
        break;
    }
    case ж<ast.CommClause> n: {
        r.openScope(n.Pos());
        defer(r.closeScope);
        if ((~n).Comm != default!) {
            ast.Walk(~r, (~n).Comm);
        }
        r.walkStmts((~n).Body);
        break;
    }
    case ж<ast.SelectStmt> n: {
        if ((~n).Body != nil) {
            // as for switch statements, select statement bodies don't get their own
            // scope.
            r.walkStmts((~(~n).Body).List);
        }
        break;
    }
    case ж<ast.ForStmt> n: {
        r.openScope(n.Pos());
        defer(r.closeScope);
        if ((~n).Init != default!) {
            ast.Walk(~r, (~n).Init);
        }
        if ((~n).Cond != default!) {
            ast.Walk(~r, (~n).Cond);
        }
        if ((~n).Post != default!) {
            ast.Walk(~r, (~n).Post);
        }
        ast.Walk(~r, ~(~n).Body);
        break;
    }
    case ж<ast.RangeStmt> n: {
        r.openScope(n.Pos());
        defer(r.closeScope);
        ast.Walk(~r, (~n).X);
        slice<ast.Expr> lhs = default!;
        if ((~n).Key != default!) {
            lhs = append(lhs, (~n).Key);
        }
        if ((~n).Value != default!) {
            lhs = append(lhs, (~n).Value);
        }
        if (len(lhs) > 0) {
            if ((~n).Tok == token.DEFINE){
                // Note: we can't exactly match the behavior of object resolution
                // during the parsing pass here, as it uses the position of the RANGE
                // token for the RHS OpPos. That information is not contained within
                // the AST.
                var @as = Ꮡ(new ast.AssignStmt(
                    Lhs: lhs,
                    Tok: token.DEFINE,
                    TokPos: (~n).TokPos,
                    Rhs: new ast.Expr[]{new ast.UnaryExpr(Op: token.RANGE, X: (~n).X)}.slice()
                ));
                // TODO(rFindley): this walkLHS reproduced the parser resolution, but
                // is it necessary? By comparison, for a normal AssignStmt we don't
                // walk the LHS in case there is an invalid identifier list.
                r.walkLHS(lhs);
                r.shortVarDecl(@as);
            } else {
                r.walkExprs(lhs);
            }
        }
        ast.Walk(~r, ~(~n).Body);
        break;
    }
    case ж<ast.GenDecl> n: {
        var exprᴛ1 = (~n).Tok;
        if (exprᴛ1 == token.CONST || exprᴛ1 == token.VAR) {
            foreach (var (i, spec) in (~n).Specs) {
                // Declarations
                var specΔ1 = spec._<ж<ast.ValueSpec>>();
                ast.ObjKind kind = ast.Con;
                if ((~n).Tok == token.VAR) {
                    kind = ast.Var;
                }
                r.walkExprs((~specΔ1).Values);
                if ((~specΔ1).Type != default!) {
                    ast.Walk(~r, (~specΔ1).Type);
                }
                r.declare(specΔ1, i, r.topScope, kind, (~specΔ1).Names.ꓸꓸꓸ);
            }
        }
        else if (exprᴛ1 == token.TYPE) {
            foreach (var (_, spec) in (~n).Specs) {
                var specΔ1 = spec._<ж<ast.TypeSpec>>();
                // Go spec: The scope of a type identifier declared inside a function begins
                // at the identifier in the TypeSpec and ends at the end of the innermost
                // containing block.
                r.declare(specΔ1, default!, r.topScope, ast.Typ, (~specΔ1).Name);
                if ((~specΔ1).TypeParams != nil) {
                    r.openScope(specΔ1.Pos());
                    defer(r.closeScope);
                    r.walkTParams((~specΔ1).TypeParams);
                }
                ast.Walk(~r, (~specΔ1).Type);
            }
        }

        break;
    }
    case ж<ast.FuncDecl> n: {
        r.openScope(n.Pos());
        defer(r.closeScope);
        r.walkRecv((~n).Recv);
        if ((~(~n).Type).TypeParams != nil) {
            // Open the function scope.
            // Type parameters are walked normally: they can reference each other, and
            // can be referenced by normal parameters.
            r.walkTParams((~(~n).Type).TypeParams);
        }
        r.resolveList((~(~n).Type).Params);
        r.resolveList((~(~n).Type).Results);
        r.declareList((~n).Recv, // TODO(rFindley): need to address receiver type parameters.
 // Resolve and declare parameters in a specific order to get duplicate
 // declaration errors in the correct location.
 ast.Var);
        r.declareList((~(~n).Type).Params, ast.Var);
        r.declareList((~(~n).Type).Results, ast.Var);
        r.walkBody((~n).Body);
        if ((~n).Recv == nil && (~(~n).Name).Name != "init"u8) {
            r.declare(n, default!, r.pkgScope, ast.Fun, (~n).Name);
        }
        break;
    }
    default: {
        var n = node.type();
        return ~r;
    }}
    return default!;
});

[GoRecv] internal static void walkFuncType(this ref resolver r, ж<ast.FuncType> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.val;

    // typ.TypeParams must be walked separately for FuncDecls.
    r.resolveList(typ.Params);
    r.resolveList(typ.Results);
    r.declareList(typ.Params, ast.Var);
    r.declareList(typ.Results, ast.Var);
}

[GoRecv] internal static void resolveList(this ref resolver r, ж<ast.FieldList> Ꮡlist) {
    ref var list = ref Ꮡlist.val;

    if (list == nil) {
        return;
    }
    foreach (var (_, f) in list.List) {
        if ((~f).Type != default!) {
            ast.Walk(~r, (~f).Type);
        }
    }
}

[GoRecv] internal static void declareList(this ref resolver r, ж<ast.FieldList> Ꮡlist, ast.ObjKind kind) {
    ref var list = ref Ꮡlist.val;

    if (list == nil) {
        return;
    }
    foreach (var (_, f) in list.List) {
        r.declare(f, default!, r.topScope, kind, (~f).Names.ꓸꓸꓸ);
    }
}

[GoRecv] internal static void walkRecv(this ref resolver r, ж<ast.FieldList> Ꮡrecv) {
    ref var recv = ref Ꮡrecv.val;

    // If our receiver has receiver type parameters, we must declare them before
    // trying to resolve the rest of the receiver, and avoid re-resolving the
    // type parameter identifiers.
    if (recv == nil || len(recv.List) == 0) {
        return;
    }
    // nothing to do
    var typ = recv.List[0].Type;
    {
        var (ptr, ok) = typ._<ж<ast.StarExpr>>(ᐧ); if (ok) {
            typ = ptr.val.X;
        }
    }
    slice<ast.Expr> declareExprs = default!;          // exprs to declare
    slice<ast.Expr> resolveExprs = default!;          // exprs to resolve
    switch (typ.type()) {
    case ж<ast.IndexExpr> typ: {
        declareExprs = new ast.Expr[]{(~typ).Index}.slice();
        resolveExprs = append(resolveExprs, (~typ).X);
        break;
    }
    case ж<ast.IndexListExpr> typ: {
        declareExprs = typ.val.Indices;
        resolveExprs = append(resolveExprs, (~typ).X);
        break;
    }
    default: {
        var typ = typ.type();
        resolveExprs = append(resolveExprs, typ);
        break;
    }}
    foreach (var (_, expr) in declareExprs) {
        {
            var (id, _) = expr._<ж<ast.Ident>>(ᐧ); if (id != nil){
                r.declare(expr, default!, r.topScope, ast.Typ, id);
            } else {
                // The receiver type parameter expression is invalid, but try to resolve
                // it anyway for consistency.
                resolveExprs = append(resolveExprs, expr);
            }
        }
    }
    foreach (var (_, expr) in resolveExprs) {
        if (expr != default!) {
            ast.Walk(~r, expr);
        }
    }
    // The receiver is invalid, but try to resolve it anyway for consistency.
    foreach (var (_, f) in recv.List[1..]) {
        if ((~f).Type != default!) {
            ast.Walk(~r, (~f).Type);
        }
    }
}

[GoRecv] internal static void walkFieldList(this ref resolver r, ж<ast.FieldList> Ꮡlist, ast.ObjKind kind) {
    ref var list = ref Ꮡlist.val;

    if (list == nil) {
        return;
    }
    r.resolveList(Ꮡlist);
    r.declareList(Ꮡlist, kind);
}

// walkTParams is like walkFieldList, but declares type parameters eagerly so
// that they may be resolved in the constraint expressions held in the field
// Type.
[GoRecv] internal static void walkTParams(this ref resolver r, ж<ast.FieldList> Ꮡlist) {
    ref var list = ref Ꮡlist.val;

    r.declareList(Ꮡlist, ast.Typ);
    r.resolveList(Ꮡlist);
}

[GoRecv] internal static void walkBody(this ref resolver r, ж<ast.BlockStmt> Ꮡbody) => func((defer, _) => {
    ref var body = ref Ꮡbody.val;

    if (body == nil) {
        return;
    }
    r.openLabelScope();
    defer(r.closeLabelScope);
    r.walkStmts(body.List);
});

} // end parser_package
