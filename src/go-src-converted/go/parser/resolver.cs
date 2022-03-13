// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package parser -- go2cs converted at 2022 March 13 05:54:01 UTC
// import "go/parser" ==> using parser = go.go.parser_package
// Original source: C:\Program Files\Go\src\go\parser\resolver.go
namespace go.go;

using fmt = fmt_package;
using ast = go.ast_package;
using typeparams = go.@internal.typeparams_package;
using token = go.token_package;
using System;

public static partial class parser_package {

private static readonly var debugResolve = false;

// resolveFile walks the given file to resolve identifiers within the file
// scope, updating ast.Ident.Obj fields with declaration information.
//
// If declErr is non-nil, it is used to report declaration errors during
// resolution. tok is used to format position in error messages.


// resolveFile walks the given file to resolve identifiers within the file
// scope, updating ast.Ident.Obj fields with declaration information.
//
// If declErr is non-nil, it is used to report declaration errors during
// resolution. tok is used to format position in error messages.
private static void resolveFile(ptr<ast.File> _addr_file, ptr<token.File> _addr_handle, Action<token.Pos, @string> declErr) {
    ref ast.File file = ref _addr_file.val;
    ref token.File handle = ref _addr_handle.val;

    var pkgScope = ast.NewScope(null);
    ptr<resolver> r = addr(new resolver(handle:handle,declErr:declErr,topScope:pkgScope,pkgScope:pkgScope,));

    foreach (var (_, decl) in file.Decls) {
        ast.Walk(r, decl);
    }    r.closeScope();
    assert(r.topScope == null, "unbalanced scopes");
    assert(r.labelScope == null, "unbalanced label scopes"); 

    // resolve global identifiers within the same file
    nint i = 0;
    foreach (var (_, ident) in r.unresolved) { 
        // i <= index for current ident
        assert(ident.Obj == unresolved, "object already resolved");
        ident.Obj = r.pkgScope.Lookup(ident.Name); // also removes unresolved sentinel
        if (ident.Obj == null) {
            r.unresolved[i] = ident;
            i++;
        }
        else if (debugResolve) {
            var pos = .Pos();
            r.dump("resolved %s@%v to package object %v", ident.Name, ident.Pos(), pos);
        }
    }    file.Scope = r.pkgScope;
    file.Unresolved = r.unresolved[(int)0..(int)i];
}

private partial struct resolver {
    public ptr<token.File> handle;
    public Action<token.Pos, @string> declErr; // Ordinary identifier scopes
    public ptr<ast.Scope> pkgScope; // pkgScope.Outer == nil
    public ptr<ast.Scope> topScope; // top-most scope; may be pkgScope
    public slice<ptr<ast.Ident>> unresolved; // unresolved identifiers

// Label scopes
// (maintained by open/close LabelScope)
    public ptr<ast.Scope> labelScope; // label scope for current function
    public slice<slice<ptr<ast.Ident>>> targetStack; // stack of unresolved labels
}

private static void dump(this ptr<resolver> _addr_r, @string format, params object[] args) {
    args = args.Clone();
    ref resolver r = ref _addr_r.val;

    fmt.Println(">>> " + r.sprintf(format, args));
}

private static @string sprintf(this ptr<resolver> _addr_r, @string format, params object[] args) {
    args = args.Clone();
    ref resolver r = ref _addr_r.val;

    {
        var arg__prev1 = arg;

        foreach (var (__i, __arg) in args) {
            i = __i;
            arg = __arg;
            switch (arg.type()) {
                case token.Pos arg:
                    args[i] = r.handle.Position(arg);
                    break;
            }
        }
        arg = arg__prev1;
    }

    return fmt.Sprintf(format, args);
}

private static void openScope(this ptr<resolver> _addr_r, token.Pos pos) {
    ref resolver r = ref _addr_r.val;

    if (debugResolve) {
        r.dump("opening scope @%v", pos);
    }
    r.topScope = ast.NewScope(r.topScope);
}

private static void closeScope(this ptr<resolver> _addr_r) {
    ref resolver r = ref _addr_r.val;

    if (debugResolve) {
        r.dump("closing scope");
    }
    r.topScope = r.topScope.Outer;
}

private static void openLabelScope(this ptr<resolver> _addr_r) {
    ref resolver r = ref _addr_r.val;

    r.labelScope = ast.NewScope(r.labelScope);
    r.targetStack = append(r.targetStack, null);
}

private static void closeLabelScope(this ptr<resolver> _addr_r) {
    ref resolver r = ref _addr_r.val;
 
    // resolve labels
    var n = len(r.targetStack) - 1;
    var scope = r.labelScope;
    foreach (var (_, ident) in r.targetStack[n]) {
        ident.Obj = scope.Lookup(ident.Name);
        if (ident.Obj == null && r.declErr != null) {
            r.declErr(ident.Pos(), fmt.Sprintf("label %s undefined", ident.Name));
        }
    }    r.targetStack = r.targetStack[(int)0..(int)n];
    r.labelScope = r.labelScope.Outer;
}

private static void declare(this ptr<resolver> _addr_r, object decl, object data, ptr<ast.Scope> _addr_scope, ast.ObjKind kind, params ptr<ptr<ast.Ident>>[] _addr_idents) {
    idents = idents.Clone();
    ref resolver r = ref _addr_r.val;
    ref ast.Scope scope = ref _addr_scope.val;
    ref ast.Ident idents = ref _addr_idents.val;

    foreach (var (_, ident) in idents) { 
        // "type" is used for type lists in interfaces, and is otherwise an invalid
        // identifier. The 'type' identifier is also artificially duplicated in the
        // type list, so could cause panics below if we were to proceed.
        if (ident.Name == "type") {
            continue;
        }
        assert(ident.Obj == null, "identifier already declared or resolved");
        var obj = ast.NewObj(kind, ident.Name); 
        // remember the corresponding declaration for redeclaration
        // errors and global variable resolution/typechecking phase
        obj.Decl = decl;
        obj.Data = data;
        ident.Obj = obj;
        if (ident.Name != "_") {
            if (debugResolve) {
                r.dump("declaring %s@%v", ident.Name, ident.Pos());
            }
            {
                var alt = scope.Insert(obj);

                if (alt != null && r.declErr != null) {
                    @string prevDecl = "";
                    {
                        var pos = alt.Pos();

                        if (pos.IsValid()) {
                            prevDecl = fmt.Sprintf("\n\tprevious declaration at %s", r.handle.Position(pos));
                        }

                    }
                    r.declErr(ident.Pos(), fmt.Sprintf("%s redeclared in this block%s", ident.Name, prevDecl));
                }

            }
        }
    }
}

private static void shortVarDecl(this ptr<resolver> _addr_r, ptr<ast.AssignStmt> _addr_decl) {
    ref resolver r = ref _addr_r.val;
    ref ast.AssignStmt decl = ref _addr_decl.val;
 
    // Go spec: A short variable declaration may redeclare variables
    // provided they were originally declared in the same block with
    // the same type, and at least one of the non-blank variables is new.
    nint n = 0; // number of new variables
    foreach (var (_, x) in decl.Lhs) {
        {
            ptr<ast.Ident> (ident, isIdent) = x._<ptr<ast.Ident>>();

            if (isIdent) {
                assert(ident.Obj == null, "identifier already declared or resolved");
                var obj = ast.NewObj(ast.Var, ident.Name); 
                // remember corresponding assignment for other tools
                obj.Decl = decl;
                ident.Obj = obj;
                if (ident.Name != "_") {
                    if (debugResolve) {
                        r.dump("declaring %s@%v", ident.Name, ident.Pos());
                    }
                    {
                        var alt = r.topScope.Insert(obj);

                        if (alt != null) {
                            ident.Obj = alt; // redeclaration
                        }
                        else
 {
                            n++; // new declaration
                        }

                    }
                }
            }

        }
    }    if (n == 0 && r.declErr != null) {
        r.declErr(decl.Lhs[0].Pos(), "no new variables on left side of :=");
    }
}

// The unresolved object is a sentinel to mark identifiers that have been added
// to the list of unresolved identifiers. The sentinel is only used for verifying
// internal consistency.
private static ptr<object> unresolved = @new<ast.Object>();

// If x is an identifier, resolve attempts to resolve x by looking up
// the object it denotes. If no object is found and collectUnresolved is
// set, x is marked as unresolved and collected in the list of unresolved
// identifiers.
//
private static void resolve(this ptr<resolver> _addr_r, ptr<ast.Ident> _addr_ident, bool collectUnresolved) => func((_, panic, _) => {
    ref resolver r = ref _addr_r.val;
    ref ast.Ident ident = ref _addr_ident.val;

    if (ident.Obj != null) {
        panic(fmt.Sprintf("%s: identifier %s already declared or resolved", r.handle.Position(ident.Pos()), ident.Name));
    }
    if (ident.Name == "_" || ident.Name == "type") {
        return ;
    }
    {
        var s = r.topScope;

        while (s != null) {
            {
                var obj = s.Lookup(ident.Name);

                if (obj != null) {
                    assert(obj.Name != "", "obj with no name");
                    ident.Obj = obj;
                    return ;
            s = s.Outer;
                }

            }
        }
    } 
    // all local scopes are known, so any unresolved identifier
    // must be found either in the file scope, package scope
    // (perhaps in another file), or universe scope --- collect
    // them so that they can be resolved later
    if (collectUnresolved) {
        ident.Obj = unresolved;
        r.unresolved = append(r.unresolved, ident);
    }
});

private static void walkExprs(this ptr<resolver> _addr_r, slice<ast.Expr> list) {
    ref resolver r = ref _addr_r.val;

    foreach (var (_, node) in list) {
        ast.Walk(r, node);
    }
}

private static void walkLHS(this ptr<resolver> _addr_r, slice<ast.Expr> list) {
    ref resolver r = ref _addr_r.val;

    {
        var expr__prev1 = expr;

        foreach (var (_, __expr) in list) {
            expr = __expr;
            var expr = unparen(expr);
            {
                ptr<ast.Ident> (_, ok) = expr._<ptr<ast.Ident>>();

                if (!ok && expr != null) {
                    ast.Walk(r, expr);
                }

            }
        }
        expr = expr__prev1;
    }
}

private static void walkStmts(this ptr<resolver> _addr_r, slice<ast.Stmt> list) {
    ref resolver r = ref _addr_r.val;

    foreach (var (_, stmt) in list) {
        ast.Walk(r, stmt);
    }
}

private static ast.Visitor Visit(this ptr<resolver> _addr_r, ast.Node node) => func((defer, _, _) => {
    ref resolver r = ref _addr_r.val;

    if (debugResolve && node != null) {
        r.dump("node %T@%v", node, node.Pos());
    }
    switch (node.type()) {
        case ptr<ast.Ident> n:
            r.resolve(n, true);
            break;
        case ptr<ast.FuncLit> n:
            r.openScope(n.Pos());
            defer(r.closeScope());
            r.walkFuncType(n.Type);
            r.walkBody(n.Body);
            break;
        case ptr<ast.SelectorExpr> n:
            ast.Walk(r, n.X); 
            // Note: don't try to resolve n.Sel, as we don't support qualified
            // resolution.
            break;
        case ptr<ast.StructType> n:
            r.openScope(n.Pos());
            defer(r.closeScope());
            r.walkFieldList(n.Fields, ast.Var);
            break;
        case ptr<ast.FuncType> n:
            r.openScope(n.Pos());
            defer(r.closeScope());
            r.walkFuncType(n);
            break;
        case ptr<ast.CompositeLit> n:
            if (n.Type != null) {
                ast.Walk(r, n.Type);
            }
            foreach (var (_, e) in n.Elts) {
                {
                    ptr<ast.KeyValueExpr> (kv, _) = e._<ptr<ast.KeyValueExpr>>();

                    if (kv != null) { 
                        // See issue #45160: try to resolve composite lit keys, but don't
                        // collect them as unresolved if resolution failed. This replicates
                        // existing behavior when resolving during parsing.
                        {
                            ptr<ast.Ident> (ident, _) = kv.Key._<ptr<ast.Ident>>();

                            if (ident != null) {
                                r.resolve(ident, false);
                            }
                            else
 {
                                ast.Walk(r, kv.Key);
                            }

                        }
                        ast.Walk(r, kv.Value);
                    }
                    else
 {
                        ast.Walk(r, e);
                    }

                }
            }
            break;
        case ptr<ast.InterfaceType> n:
            r.openScope(n.Pos());
            defer(r.closeScope());
            r.walkFieldList(n.Methods, ast.Fun); 

            // Statements
            break;
        case ptr<ast.LabeledStmt> n:
            r.declare(n, null, r.labelScope, ast.Lbl, n.Label);
            ast.Walk(r, n.Stmt);
            break;
        case ptr<ast.AssignStmt> n:
            r.walkExprs(n.Rhs);
            if (n.Tok == token.DEFINE) {
                r.shortVarDecl(n);
            }
            else
 {
                r.walkExprs(n.Lhs);
            }
            break;
        case ptr<ast.BranchStmt> n:
            if (n.Tok != token.FALLTHROUGH && n.Label != null) {
                var depth = len(r.targetStack) - 1;
                r.targetStack[depth] = append(r.targetStack[depth], n.Label);
            }
            break;
        case ptr<ast.BlockStmt> n:
            r.openScope(n.Pos());
            defer(r.closeScope());
            r.walkStmts(n.List);
            break;
        case ptr<ast.IfStmt> n:
            r.openScope(n.Pos());
            defer(r.closeScope());
            if (n.Init != null) {
                ast.Walk(r, n.Init);
            }
            ast.Walk(r, n.Cond);
            ast.Walk(r, n.Body);
            if (n.Else != null) {
                ast.Walk(r, n.Else);
            }
            break;
        case ptr<ast.CaseClause> n:
            r.walkExprs(n.List);
            r.openScope(n.Pos());
            defer(r.closeScope());
            r.walkStmts(n.Body);
            break;
        case ptr<ast.SwitchStmt> n:
            r.openScope(n.Pos());
            defer(r.closeScope());
            if (n.Init != null) {
                ast.Walk(r, n.Init);
            }
            if (n.Tag != null) { 
                // The scope below reproduces some unnecessary behavior of the parser,
                // opening an extra scope in case this is a type switch. It's not needed
                // for expression switches.
                // TODO: remove this once we've matched the parser resolution exactly.
                if (n.Init != null) {
                    r.openScope(n.Tag.Pos());
                    defer(r.closeScope());
                }
                ast.Walk(r, n.Tag);
            }
            if (n.Body != null) {
                r.walkStmts(n.Body.List);
            }
            break;
        case ptr<ast.TypeSwitchStmt> n:
            if (n.Init != null) {
                r.openScope(n.Pos());
                defer(r.closeScope());
                ast.Walk(r, n.Init);
            }
            r.openScope(n.Assign.Pos());
            defer(r.closeScope());
            ast.Walk(r, n.Assign); 
            // s.Body consists only of case clauses, so does not get its own
            // scope.
            if (n.Body != null) {
                r.walkStmts(n.Body.List);
            }
            break;
        case ptr<ast.CommClause> n:
            r.openScope(n.Pos());
            defer(r.closeScope());
            if (n.Comm != null) {
                ast.Walk(r, n.Comm);
            }
            r.walkStmts(n.Body);
            break;
        case ptr<ast.SelectStmt> n:
            if (n.Body != null) {
                r.walkStmts(n.Body.List);
            }
            break;
        case ptr<ast.ForStmt> n:
            r.openScope(n.Pos());
            defer(r.closeScope());
            if (n.Init != null) {
                ast.Walk(r, n.Init);
            }
            if (n.Cond != null) {
                ast.Walk(r, n.Cond);
            }
            if (n.Post != null) {
                ast.Walk(r, n.Post);
            }
            ast.Walk(r, n.Body);
            break;
        case ptr<ast.RangeStmt> n:
            r.openScope(n.Pos());
            defer(r.closeScope());
            ast.Walk(r, n.X);
            slice<ast.Expr> lhs = default;
            if (n.Key != null) {
                lhs = append(lhs, n.Key);
            }
            if (n.Value != null) {
                lhs = append(lhs, n.Value);
            }
            if (len(lhs) > 0) {
                if (n.Tok == token.DEFINE) { 
                    // Note: we can't exactly match the behavior of object resolution
                    // during the parsing pass here, as it uses the position of the RANGE
                    // token for the RHS OpPos. That information is not contained within
                    // the AST.
                    ptr<ast.AssignStmt> @as = addr(new ast.AssignStmt(Lhs:lhs,Tok:token.DEFINE,TokPos:n.TokPos,Rhs:[]ast.Expr{&ast.UnaryExpr{Op:token.RANGE,X:n.X}},)); 
                    // TODO(rFindley): this walkLHS reproduced the parser resolution, but
                    // is it necessary? By comparison, for a normal AssignStmt we don't
                    // walk the LHS in case there is an invalid identifier list.
                    r.walkLHS(lhs);
                    r.shortVarDecl(as);
                }
                else
 {
                    r.walkExprs(lhs);
                }
            }
            ast.Walk(r, n.Body); 

            // Declarations
            break;
        case ptr<ast.GenDecl> n:

            if (n.Tok == token.CONST || n.Tok == token.VAR) 
                {
                    var spec__prev1 = spec;

                    foreach (var (__i, __spec) in n.Specs) {
                        i = __i;
                        spec = __spec;
                        ptr<ast.ValueSpec> spec = spec._<ptr<ast.ValueSpec>>();
                        var kind = ast.Con;
                        if (n.Tok == token.VAR) {
                            kind = ast.Var;
                        }
                        r.walkExprs(spec.Values);
                        if (spec.Type != null) {
                            ast.Walk(r, spec.Type);
                        }
                        r.declare(spec, i, r.topScope, kind, spec.Names);
                    }

                    spec = spec__prev1;
                }
            else if (n.Tok == token.TYPE) 
                {
                    var spec__prev1 = spec;

                    foreach (var (_, __spec) in n.Specs) {
                        spec = __spec;
                        spec = spec._<ptr<ast.TypeSpec>>(); 
                        // Go spec: The scope of a type identifier declared inside a function begins
                        // at the identifier in the TypeSpec and ends at the end of the innermost
                        // containing block.
                        r.declare(spec, null, r.topScope, ast.Typ, spec.Name);
                        {
                            var tparams__prev1 = tparams;

                            var tparams = typeparams.Get(spec);

                            if (tparams != null) {
                                r.openScope(spec.Pos());
                                defer(r.closeScope());
                                r.walkTParams(tparams);
                            }

                            tparams = tparams__prev1;

                        }
                        ast.Walk(r, spec.Type);
                    }

                    spec = spec__prev1;
                }
                        break;
        case ptr<ast.FuncDecl> n:
            r.openScope(n.Pos());
            defer(r.closeScope()); 

            // Resolve the receiver first, without declaring.
            r.resolveList(n.Recv); 

            // Type parameters are walked normally: they can reference each other, and
            // can be referenced by normal parameters.
            {
                var tparams__prev1 = tparams;

                tparams = typeparams.Get(n.Type);

                if (tparams != null) {
                    r.walkTParams(tparams); 
                    // TODO(rFindley): need to address receiver type parameters.
                } 

                // Resolve and declare parameters in a specific order to get duplicate
                // declaration errors in the correct location.

                tparams = tparams__prev1;

            } 

            // Resolve and declare parameters in a specific order to get duplicate
            // declaration errors in the correct location.
            r.resolveList(n.Type.Params);
            r.resolveList(n.Type.Results);
            r.declareList(n.Recv, ast.Var);
            r.declareList(n.Type.Params, ast.Var);
            r.declareList(n.Type.Results, ast.Var);

            r.walkBody(n.Body);
            if (n.Recv == null && n.Name.Name != "init") {
                r.declare(n, null, r.pkgScope, ast.Fun, n.Name);
            }
            break;
        default:
        {
            var n = node.type();
            return r;
            break;
        }

    }

    return null;
});

private static void walkFuncType(this ptr<resolver> _addr_r, ptr<ast.FuncType> _addr_typ) {
    ref resolver r = ref _addr_r.val;
    ref ast.FuncType typ = ref _addr_typ.val;
 
    // typ.TParams must be walked separately for FuncDecls.
    r.resolveList(typ.Params);
    r.resolveList(typ.Results);
    r.declareList(typ.Params, ast.Var);
    r.declareList(typ.Results, ast.Var);
}

private static void resolveList(this ptr<resolver> _addr_r, ptr<ast.FieldList> _addr_list) {
    ref resolver r = ref _addr_r.val;
    ref ast.FieldList list = ref _addr_list.val;

    if (list == null) {
        return ;
    }
    foreach (var (_, f) in list.List) {
        if (f.Type != null) {
            ast.Walk(r, f.Type);
        }
    }
}

private static void declareList(this ptr<resolver> _addr_r, ptr<ast.FieldList> _addr_list, ast.ObjKind kind) {
    ref resolver r = ref _addr_r.val;
    ref ast.FieldList list = ref _addr_list.val;

    if (list == null) {
        return ;
    }
    foreach (var (_, f) in list.List) {
        r.declare(f, null, r.topScope, kind, f.Names);
    }
}

private static void walkFieldList(this ptr<resolver> _addr_r, ptr<ast.FieldList> _addr_list, ast.ObjKind kind) {
    ref resolver r = ref _addr_r.val;
    ref ast.FieldList list = ref _addr_list.val;

    if (list == null) {
        return ;
    }
    r.resolveList(list);
    r.declareList(list, kind);
}

// walkTParams is like walkFieldList, but declares type parameters eagerly so
// that they may be resolved in the constraint expressions held in the field
// Type.
private static void walkTParams(this ptr<resolver> _addr_r, ptr<ast.FieldList> _addr_list) {
    ref resolver r = ref _addr_r.val;
    ref ast.FieldList list = ref _addr_list.val;

    if (list == null) {
        return ;
    }
    r.declareList(list, ast.Typ);
    r.resolveList(list);
}

private static void walkBody(this ptr<resolver> _addr_r, ptr<ast.BlockStmt> _addr_body) => func((defer, _, _) => {
    ref resolver r = ref _addr_r.val;
    ref ast.BlockStmt body = ref _addr_body.val;

    if (body == null) {
        return ;
    }
    r.openLabelScope();
    defer(r.closeLabelScope());
    r.walkStmts(body.List);
});

} // end parser_package
