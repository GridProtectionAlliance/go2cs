// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package noder -- go2cs converted at 2022 March 13 06:27:42 UTC
// import "cmd/compile/internal/noder" ==> using noder = go.cmd.compile.@internal.noder_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\noder\stmt.go
namespace go.cmd.compile.@internal;

using ir = cmd.compile.@internal.ir_package;
using syntax = cmd.compile.@internal.syntax_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;
using src = cmd.@internal.src_package;

public static partial class noder_package {

private static slice<ir.Node> stmts(this ptr<irgen> _addr_g, slice<syntax.Stmt> stmts) {
    ref irgen g = ref _addr_g.val;

    slice<ir.Node> nodes = default;
    foreach (var (_, stmt) in stmts) {
        switch (g.stmt(stmt).type()) {
            case 
                break;
            case ptr<ir.BlockStmt> s:
                nodes = append(nodes, s.List);
                break;
            default:
            {
                var s = g.stmt(stmt).type();
                nodes = append(nodes, s);
                break;
            }
        }
    }    return nodes;
}

private static ir.Node stmt(this ptr<irgen> _addr_g, syntax.Stmt stmt) => func((_, panic, _) => {
    ref irgen g = ref _addr_g.val;

    switch (stmt.type()) {
        case ptr<syntax.EmptyStmt> stmt:
            return null;
            break;
        case ptr<syntax.LabeledStmt> stmt:
            return g.labeledStmt(stmt);
            break;
        case ptr<syntax.BlockStmt> stmt:
            return ir.NewBlockStmt(g.pos(stmt), g.blockStmt(stmt));
            break;
        case ptr<syntax.ExprStmt> stmt:
            var x = g.expr(stmt.X);
            {
                ptr<ir.CallExpr> (call, ok) = x._<ptr<ir.CallExpr>>();

                if (ok) {
                    call.Use = ir.CallUseStmt;
                }

            }
            return x;
            break;
        case ptr<syntax.SendStmt> stmt:
            var n = ir.NewSendStmt(g.pos(stmt), g.expr(stmt.Chan), g.expr(stmt.Value));
            if (n.Chan.Type().HasTParam() || n.Value.Type().HasTParam()) { 
                // Delay transforming the send if the channel or value
                // have a type param.
                n.SetTypecheck(3);
                return n;
            }
            transformSend(n);
            n.SetTypecheck(1);
            return n;
            break;
        case ptr<syntax.DeclStmt> stmt:
            return ir.NewBlockStmt(g.pos(stmt), g.decls(stmt.DeclList));
            break;
        case ptr<syntax.AssignStmt> stmt:
            if (stmt.Op != 0 && stmt.Op != syntax.Def) {
                var op = g.op(stmt.Op, binOps[..]);
                n = ;
                if (stmt.Rhs == null) {
                    n = IncDec(g.pos(stmt), op, g.expr(stmt.Lhs));
                }
                else
 {
                    n = ir.NewAssignOpStmt(g.pos(stmt), op, g.expr(stmt.Lhs), g.expr(stmt.Rhs));
                }
                if (n.X.Typecheck() == 3) {
                    n.SetTypecheck(3);
                    return n;
                }
                transformAsOp(n);
                n.SetTypecheck(1);
                return n;
            }
            var (names, lhs) = g.assignList(stmt.Lhs, stmt.Op == syntax.Def);
            var rhs = g.exprList(stmt.Rhs); 

            // We must delay transforming the assign statement if any of the
            // lhs or rhs nodes are also delayed, since transformAssign needs
            // to know the types of the left and right sides in various cases.
            var delay = false;
            {
                var e__prev1 = e;

                foreach (var (_, __e) in lhs) {
                    e = __e;
                    if (e.Typecheck() == 3) {
                        delay = true;
                        break;
                    }
                }

                e = e__prev1;
            }

            {
                var e__prev1 = e;

                foreach (var (_, __e) in rhs) {
                    e = __e;
                    if (e.Typecheck() == 3) {
                        delay = true;
                        break;
                    }
                }

                e = e__prev1;
            }

            if (len(lhs) == 1 && len(rhs) == 1) {
                n = ir.NewAssignStmt(g.pos(stmt), lhs[0], rhs[0]);
                n.Def = initDefn(n, names);

                if (delay) {
                    n.SetTypecheck(3);
                    return n;
                }
                ir.Node lhs = new slice<ir.Node>(new ir.Node[] { n.X });
                rhs = new slice<ir.Node>(new ir.Node[] { n.Y });
                transformAssign(n, lhs, rhs);
                (n.X, n.Y) = (lhs[0], rhs[0]);                n.SetTypecheck(1);
                return n;
            }
            n = ir.NewAssignListStmt(g.pos(stmt), ir.OAS2, lhs, rhs);
            n.Def = initDefn(n, names);
            if (delay) {
                n.SetTypecheck(3);
                return n;
            }
            transformAssign(n, n.Lhs, n.Rhs);
            n.SetTypecheck(1);
            return n;
            break;
        case ptr<syntax.BranchStmt> stmt:
            return ir.NewBranchStmt(g.pos(stmt), g.tokOp(int(stmt.Tok), branchOps[..]), g.name(stmt.Label));
            break;
        case ptr<syntax.CallStmt> stmt:
            return ir.NewGoDeferStmt(g.pos(stmt), g.tokOp(int(stmt.Tok), callOps[..]), g.expr(stmt.Call));
            break;
        case ptr<syntax.ReturnStmt> stmt:
            n = ir.NewReturnStmt(g.pos(stmt), g.exprList(stmt.Results));
            {
                var e__prev1 = e;

                foreach (var (_, __e) in n.Results) {
                    e = __e;
                    if (e.Type().HasTParam()) { 
                        // Delay transforming the return statement if any of the
                        // return values have a type param.
                        n.SetTypecheck(3);
                        return n;
                    }
                }

                e = e__prev1;
            }

            transformReturn(n);
            n.SetTypecheck(1);
            return n;
            break;
        case ptr<syntax.IfStmt> stmt:
            return g.ifStmt(stmt);
            break;
        case ptr<syntax.ForStmt> stmt:
            return g.forStmt(stmt);
            break;
        case ptr<syntax.SelectStmt> stmt:
            n = g.selectStmt(stmt);
            transformSelect(n._<ptr<ir.SelectStmt>>());
            n.SetTypecheck(1);
            return n;
            break;
        case ptr<syntax.SwitchStmt> stmt:
            return g.switchStmt(stmt);
            break;
        default:
        {
            var stmt = stmt.type();
            g.unhandled("statement", stmt);
            panic("unreachable");
            break;
        }
    }
});

// TODO(mdempsky): Investigate replacing with switch statements or dense arrays.

private static array<ir.Op> branchOps = new array<ir.Op>(InitKeyedValues<ir.Op>((syntax.Break, ir.OBREAK), (syntax.Continue, ir.OCONTINUE), (syntax.Fallthrough, ir.OFALL), (syntax.Goto, ir.OGOTO)));

private static array<ir.Op> callOps = new array<ir.Op>(InitKeyedValues<ir.Op>((syntax.Defer, ir.ODEFER), (syntax.Go, ir.OGO)));

private static ir.Op tokOp(this ptr<irgen> _addr_g, nint tok, slice<ir.Op> ops) {
    ref irgen g = ref _addr_g.val;
 
    // TODO(mdempsky): Validate.
    return ops[tok];
}

private static ir.Op op(this ptr<irgen> _addr_g, syntax.Operator op, slice<ir.Op> ops) {
    ref irgen g = ref _addr_g.val;
 
    // TODO(mdempsky): Validate.
    return ops[op];
}

private static (slice<ptr<ir.Name>>, slice<ir.Node>) assignList(this ptr<irgen> _addr_g, syntax.Expr expr, bool def) {
    slice<ptr<ir.Name>> _p0 = default;
    slice<ir.Node> _p0 = default;
    ref irgen g = ref _addr_g.val;

    if (!def) {
        return (null, g.exprList(expr));
    }
    slice<syntax.Expr> exprs = default;
    {
        ptr<syntax.ListExpr> (list, ok) = expr._<ptr<syntax.ListExpr>>();

        if (ok) {
            exprs = list.ElemList;
        }
        else
 {
            exprs = new slice<syntax.Expr>(new syntax.Expr[] { expr });
        }
    }

    slice<ptr<ir.Name>> names = default;
    var res = make_slice<ir.Node>(len(exprs));
    {
        var expr__prev1 = expr;

        foreach (var (__i, __expr) in exprs) {
            i = __i;
            expr = __expr;
            ptr<syntax.Name> expr = expr._<ptr<syntax.Name>>();
            if (expr.Value == "_") {
                res[i] = ir.BlankNode;
                continue;
            }
            {
                var (obj, ok) = g.info.Uses[expr];

                if (ok) {
                    res[i] = g.obj(obj);
                    continue;
                }

            }

            var (name, _) = g.def(expr);
            names = append(names, name);
            res[i] = name;
        }
        expr = expr__prev1;
    }

    return (names, res);
}

// initDefn marks the given names as declared by defn and populates
// its Init field with ODCL nodes. It then reports whether any names
// were so declared, which can be used to initialize defn.Def.
private static bool initDefn(ir.InitNode defn, slice<ptr<ir.Name>> names) {
    if (len(names) == 0) {
        return false;
    }
    var init = make_slice<ir.Node>(len(names));
    foreach (var (i, name) in names) {
        name.Defn = defn;
        init[i] = ir.NewDecl(name.Pos(), ir.ODCL, name);
    }    defn.SetInit(init);
    return true;
}

private static slice<ir.Node> blockStmt(this ptr<irgen> _addr_g, ptr<syntax.BlockStmt> _addr_stmt) {
    ref irgen g = ref _addr_g.val;
    ref syntax.BlockStmt stmt = ref _addr_stmt.val;

    return g.stmts(stmt.List);
}

private static ir.Node ifStmt(this ptr<irgen> _addr_g, ptr<syntax.IfStmt> _addr_stmt) {
    ref irgen g = ref _addr_g.val;
    ref syntax.IfStmt stmt = ref _addr_stmt.val;

    var init = g.stmt(stmt.Init);
    var n = ir.NewIfStmt(g.pos(stmt), g.expr(stmt.Cond), g.blockStmt(stmt.Then), null);
    if (stmt.Else != null) {
        var e = g.stmt(stmt.Else);
        if (e.Op() == ir.OBLOCK) {
            e = e._<ptr<ir.BlockStmt>>();
            n.Else = e.List;
        }
        else
 {
            n.Else = new slice<ir.Node>(new ir.Node[] { e });
        }
    }
    return g.init(init, n);
}

// unpackTwo returns the first two nodes in list. If list has fewer
// than 2 nodes, then the missing nodes are replaced with nils.
private static (ir.Node, ir.Node) unpackTwo(slice<ir.Node> list) {
    ir.Node fst = default;
    ir.Node snd = default;

    switch (len(list)) {
        case 0: 
            return (null, null);
            break;
        case 1: 
            return (list[0], null);
            break;
        default: 
            return (list[0], list[1]);
            break;
    }
}

private static ir.Node forStmt(this ptr<irgen> _addr_g, ptr<syntax.ForStmt> _addr_stmt) {
    ref irgen g = ref _addr_g.val;
    ref syntax.ForStmt stmt = ref _addr_stmt.val;

    {
        ptr<syntax.RangeClause> (r, ok) = stmt.Init._<ptr<syntax.RangeClause>>();

        if (ok) {
            var (names, lhs) = g.assignList(r.Lhs, r.Def);
            var (key, value) = unpackTwo(lhs);
            var n = ir.NewRangeStmt(g.pos(r), key, value, g.expr(r.X), g.blockStmt(stmt.Body));
            n.Def = initDefn(n, names);
            return n;
        }
    }

    return ir.NewForStmt(g.pos(stmt), g.stmt(stmt.Init), g.expr(stmt.Cond), g.stmt(stmt.Post), g.blockStmt(stmt.Body));
}

private static ir.Node selectStmt(this ptr<irgen> _addr_g, ptr<syntax.SelectStmt> _addr_stmt) {
    ref irgen g = ref _addr_g.val;
    ref syntax.SelectStmt stmt = ref _addr_stmt.val;

    var body = make_slice<ptr<ir.CommClause>>(len(stmt.Body));
    foreach (var (i, clause) in stmt.Body) {
        body[i] = ir.NewCommStmt(g.pos(clause), g.stmt(clause.Comm), g.stmts(clause.Body));
    }    return ir.NewSelectStmt(g.pos(stmt), body);
}

private static ir.Node switchStmt(this ptr<irgen> _addr_g, ptr<syntax.SwitchStmt> _addr_stmt) {
    ref irgen g = ref _addr_g.val;
    ref syntax.SwitchStmt stmt = ref _addr_stmt.val;

    var pos = g.pos(stmt);
    var init = g.stmt(stmt.Init);

    ir.Node expr = default;
    switch (stmt.Tag.type()) {
        case ptr<syntax.TypeSwitchGuard> tag:
            ptr<ir.Ident> ident;
            if (tag.Lhs != null) {
                ident = ir.NewIdent(g.pos(tag.Lhs), g.name(tag.Lhs));
            }
            expr = ir.NewTypeSwitchGuard(pos, ident, g.expr(tag.X));
            break;
        default:
        {
            var tag = stmt.Tag.type();
            expr = g.expr(tag);
            break;
        }

    }

    var body = make_slice<ptr<ir.CaseClause>>(len(stmt.Body));
    foreach (var (i, clause) in stmt.Body) { 
        // Check for an implicit clause variable before
        // visiting body, because it may contain function
        // literals that reference it, and then it'll be
        // associated to the wrong function.
        //
        // Also, override its position to the clause's colon, so that
        // dwarfgen can find the right scope for it later.
        // TODO(mdempsky): We should probably just store the scope
        // directly in the ir.Name.
        ptr<ir.Name> cv;
        {
            var (obj, ok) = g.info.Implicits[clause];

            if (ok) {
                cv = g.obj(obj);
                cv.SetPos(g.makeXPos(clause.Colon));
            }

        }
        body[i] = ir.NewCaseStmt(g.pos(clause), g.exprList(clause.Cases), g.stmts(clause.Body));
        body[i].Var = cv;
    }    return g.init(init, ir.NewSwitchStmt(pos, expr, body));
}

private static ir.Node labeledStmt(this ptr<irgen> _addr_g, ptr<syntax.LabeledStmt> _addr_label) {
    ref irgen g = ref _addr_g.val;
    ref syntax.LabeledStmt label = ref _addr_label.val;

    var sym = g.name(label.Label);
    var lhs = ir.NewLabelStmt(g.pos(label), sym);
    var ls = g.stmt(label.Stmt); 

    // Attach label directly to control statement too.
    switch (ls.type()) {
        case ptr<ir.ForStmt> ls:
            ls.Label = sym;
            break;
        case ptr<ir.RangeStmt> ls:
            ls.Label = sym;
            break;
        case ptr<ir.SelectStmt> ls:
            ls.Label = sym;
            break;
        case ptr<ir.SwitchStmt> ls:
            ls.Label = sym;
            break;

    }

    ir.Node l = new slice<ir.Node>(new ir.Node[] { lhs });
    if (ls != null) {
        if (ls.Op() == ir.OBLOCK) {
            ls = ls._<ptr<ir.BlockStmt>>();
            l = append(l, ls.List);
        }
        else
 {
            l = append(l, ls);
        }
    }
    return ir.NewBlockStmt(src.NoXPos, l);
}

private static ir.InitNode init(this ptr<irgen> _addr_g, ir.Node init, ir.InitNode stmt) {
    ref irgen g = ref _addr_g.val;

    if (init != null) {
        stmt.SetInit(new slice<ir.Node>(new ir.Node[] { init }));
    }
    return stmt;
}

private static ptr<types.Sym> name(this ptr<irgen> _addr_g, ptr<syntax.Name> _addr_name) {
    ref irgen g = ref _addr_g.val;
    ref syntax.Name name = ref _addr_name.val;

    if (name == null) {
        return _addr_null!;
    }
    return _addr_typecheck.Lookup(name.Value)!;
}

} // end noder_package
