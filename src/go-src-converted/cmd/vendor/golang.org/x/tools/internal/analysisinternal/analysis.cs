// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package analysisinternal exposes internal-only fields from go/analysis.
// package analysisinternal -- go2cs converted at 2022 March 06 23:35:12 UTC
// import "cmd/vendor/golang.org/x/tools/internal/analysisinternal" ==> using analysisinternal = go.cmd.vendor.golang.org.x.tools.@internal.analysisinternal_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\internal\analysisinternal\analysis.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;
using strings = go.strings_package;

using astutil = go.golang.org.x.tools.go.ast.astutil_package;
using fuzzy = go.golang.org.x.tools.@internal.lsp.fuzzy_package;
using System;


namespace go.cmd.vendor.golang.org.x.tools.@internal;

public static partial class analysisinternal_package {

public static Func<object, slice<types.Error>> GetTypeErrors = default;public static Action<object, slice<types.Error>> SetTypeErrors = default;

public static token.Pos TypeErrorEndPos(ptr<token.FileSet> _addr_fset, slice<byte> src, token.Pos start) {
    ref token.FileSet fset = ref _addr_fset.val;
 
    // Get the end position for the type error.
    var offset = fset.PositionFor(start, false).Offset;
    var end = start;
    if (offset >= len(src)) {
        return end;
    }
    {
        var width = bytes.IndexAny(src[(int)offset..], " \n,():;[]+-*");

        if (width > 0) {
            end = start + token.Pos(width);
        }
    }

    return end;

}

public static ast.Expr ZeroValue(ptr<token.FileSet> _addr_fset, ptr<ast.File> _addr_f, ptr<types.Package> _addr_pkg, types.Type typ) => func((_, panic, _) => {
    ref token.FileSet fset = ref _addr_fset.val;
    ref ast.File f = ref _addr_f.val;
    ref types.Package pkg = ref _addr_pkg.val;

    var under = typ;
    {
        ptr<types.Named> (n, ok) = typ._<ptr<types.Named>>();

        if (ok) {
            under = n.Underlying();
        }
    }

    switch (under.type()) {
        case ptr<types.Basic> u:

            if (u.Info() & types.IsNumeric != 0) 
                return addr(new ast.BasicLit(Kind:token.INT,Value:"0"));
            else if (u.Info() & types.IsBoolean != 0) 
                return addr(new ast.Ident(Name:"false"));
            else if (u.Info() & types.IsString != 0) 
                return addr(new ast.BasicLit(Kind:token.STRING,Value:`""`));
            else 
                panic("unknown basic type");
                        break;
        case ptr<types.Chan> u:
            return ast.NewIdent("nil");
            break;
        case ptr<types.Interface> u:
            return ast.NewIdent("nil");
            break;
        case ptr<types.Map> u:
            return ast.NewIdent("nil");
            break;
        case ptr<types.Pointer> u:
            return ast.NewIdent("nil");
            break;
        case ptr<types.Signature> u:
            return ast.NewIdent("nil");
            break;
        case ptr<types.Slice> u:
            return ast.NewIdent("nil");
            break;
        case ptr<types.Array> u:
            return ast.NewIdent("nil");
            break;
        case ptr<types.Struct> u:
            var texpr = TypeExpr(_addr_fset, _addr_f, _addr_pkg, typ); // typ because we want the name here.
            if (texpr == null) {
                return null;
            }

            return addr(new ast.CompositeLit(Type:texpr,));
            break;
    }
    return null;

});

// IsZeroValue checks whether the given expression is a 'zero value' (as determined by output of
// analysisinternal.ZeroValue)
public static bool IsZeroValue(ast.Expr expr) {
    switch (expr.type()) {
        case ptr<ast.BasicLit> e:
            return e.Value == "0" || e.Value == "\"\"";
            break;
        case ptr<ast.Ident> e:
            return e.Name == "nil" || e.Name == "false";
            break;
        default:
        {
            var e = expr.type();
            return false;
            break;
        }
    }

}

public static ast.Expr TypeExpr(ptr<token.FileSet> _addr_fset, ptr<ast.File> _addr_f, ptr<types.Package> _addr_pkg, types.Type typ) {
    ref token.FileSet fset = ref _addr_fset.val;
    ref ast.File f = ref _addr_f.val;
    ref types.Package pkg = ref _addr_pkg.val;

    switch (typ.type()) {
        case ptr<types.Basic> t:

            if (t.Kind() == types.UnsafePointer) 
                return addr(new ast.SelectorExpr(X:ast.NewIdent("unsafe"),Sel:ast.NewIdent("Pointer")));
            else 
                return ast.NewIdent(t.Name());
                        break;
        case ptr<types.Pointer> t:
            var x = TypeExpr(_addr_fset, _addr_f, _addr_pkg, t.Elem());
            if (x == null) {
                return null;
            }
            return addr(new ast.UnaryExpr(Op:token.MUL,X:x,));
            break;
        case ptr<types.Array> t:
            var elt = TypeExpr(_addr_fset, _addr_f, _addr_pkg, t.Elem());
            if (elt == null) {
                return null;
            }
            return addr(new ast.ArrayType(Len:&ast.BasicLit{Kind:token.INT,Value:fmt.Sprintf("%d",t.Len()),},Elt:elt,));
            break;
        case ptr<types.Slice> t:
            elt = TypeExpr(_addr_fset, _addr_f, _addr_pkg, t.Elem());
            if (elt == null) {
                return null;
            }
            return addr(new ast.ArrayType(Elt:elt,));
            break;
        case ptr<types.Map> t:
            var key = TypeExpr(_addr_fset, _addr_f, _addr_pkg, t.Key());
            var value = TypeExpr(_addr_fset, _addr_f, _addr_pkg, t.Elem());
            if (key == null || value == null) {
                return null;
            }
            return addr(new ast.MapType(Key:key,Value:value,));
            break;
        case ptr<types.Chan> t:
            var dir = ast.ChanDir(t.Dir());
            if (t.Dir() == types.SendRecv) {
                dir = ast.SEND | ast.RECV;
            }
            value = TypeExpr(_addr_fset, _addr_f, _addr_pkg, t.Elem());
            if (value == null) {
                return null;
            }
            return addr(new ast.ChanType(Dir:dir,Value:value,));
            break;
        case ptr<types.Signature> t:
            slice<ptr<ast.Field>> @params = default;
            {
                nint i__prev1 = i;

                for (nint i = 0; i < t.Params().Len(); i++) {
                    var p = TypeExpr(_addr_fset, _addr_f, _addr_pkg, t.Params().At(i).Type());
                    if (p == null) {
                        return null;
                    }
                    params = append(params, addr(new ast.Field(Type:p,Names:[]*ast.Ident{{Name:t.Params().At(i).Name(),},},)));
                }


                i = i__prev1;
            }
            slice<ptr<ast.Field>> returns = default;
            {
                nint i__prev1 = i;

                for (i = 0; i < t.Results().Len(); i++) {
                    var r = TypeExpr(_addr_fset, _addr_f, _addr_pkg, t.Results().At(i).Type());
                    if (r == null) {
                        return null;
                    }
                    returns = append(returns, addr(new ast.Field(Type:r,)));
                }


                i = i__prev1;
            }
            return addr(new ast.FuncType(Params:&ast.FieldList{List:params,},Results:&ast.FieldList{List:returns,},));
            break;
        case ptr<types.Named> t:
            if (t.Obj().Pkg() == null) {
                return ast.NewIdent(t.Obj().Name());
            }
            if (t.Obj().Pkg() == pkg) {
                return ast.NewIdent(t.Obj().Name());
            }
            var pkgName = t.Obj().Pkg().Name(); 
            // If the file already imports the package under another name, use that.
            foreach (var (_, group) in astutil.Imports(fset, f)) {
                foreach (var (_, cand) in group) {
                    if (strings.Trim(cand.Path.Value, "\"") == t.Obj().Pkg().Path()) {
                        if (cand.Name != null && cand.Name.Name != "") {
                            pkgName = cand.Name.Name;
                        }
                    }
                }
            }
            if (pkgName == ".") {
                return ast.NewIdent(t.Obj().Name());
            }

            return addr(new ast.SelectorExpr(X:ast.NewIdent(pkgName),Sel:ast.NewIdent(t.Obj().Name()),));
            break;
        case ptr<types.Struct> t:
            return ast.NewIdent(t.String());
            break;
        case ptr<types.Interface> t:
            return ast.NewIdent(t.String());
            break;
        default:
        {
            var t = typ.type();
            return null;
            break;
        }
    }

}

public partial struct TypeErrorPass { // : @string
}

public static readonly TypeErrorPass NoNewVars = "nonewvars";
public static readonly TypeErrorPass NoResultValues = "noresultvalues";
public static readonly TypeErrorPass UndeclaredName = "undeclaredname";


// StmtToInsertVarBefore returns the ast.Stmt before which we can safely insert a new variable.
// Some examples:
//
// Basic Example:
// z := 1
// y := z + x
// If x is undeclared, then this function would return `y := z + x`, so that we
// can insert `x := ` on the line before `y := z + x`.
//
// If stmt example:
// if z == 1 {
// } else if z == y {}
// If y is undeclared, then this function would return `if z == 1 {`, because we cannot
// insert a statement between an if and an else if statement. As a result, we need to find
// the top of the if chain to insert `y := ` before.
public static ast.Stmt StmtToInsertVarBefore(slice<ast.Node> path) {
    nint enclosingIndex = -1;
    {
        var i__prev1 = i;

        foreach (var (__i, __p) in path) {
            i = __i;
            p = __p;
            {
                ast.Stmt (_, ok) = p._<ast.Stmt>();

                if (ok) {
                    enclosingIndex = i;
                    break;
                }

            }

        }
        i = i__prev1;
    }

    if (enclosingIndex == -1) {
        return null;
    }
    var enclosingStmt = path[enclosingIndex];
    switch (enclosingStmt.type()) {
        case ptr<ast.IfStmt> _:
            return baseIfStmt(path, enclosingIndex);
            break;
        case ptr<ast.CaseClause> _:
            {
                var i__prev1 = i;

                for (var i = enclosingIndex + 1; i < len(path); i++) {
                    {
                        ptr<ast.SwitchStmt> node__prev1 = node;

                        ptr<ast.SwitchStmt> (node, ok) = path[i]._<ptr<ast.SwitchStmt>>();

                        if (ok) {
                            return node;
                        }                        {
                            ptr<ast.SwitchStmt> node__prev2 = node;

                            (node, ok) = path[i]._<ptr<ast.TypeSwitchStmt>>();


                            else if (ok) {
                                return node;
                            }

                            node = node__prev2;

                        }


                        node = node__prev1;

                    }

                }


                i = i__prev1;
            }
            break;
    }
    if (len(path) <= enclosingIndex + 1) {
        return enclosingStmt._<ast.Stmt>();
    }
    switch (path[enclosingIndex + 1].type()) {
        case ptr<ast.IfStmt> expr:
            return baseIfStmt(path, enclosingIndex + 1);
            break;
        case ptr<ast.ForStmt> expr:
            if (expr.Init == enclosingStmt || expr.Post == enclosingStmt) {
                return expr;
            }
            break;
    }
    return enclosingStmt._<ast.Stmt>();

}

// baseIfStmt walks up the if/else-if chain until we get to
// the top of the current if chain.
private static ast.Stmt baseIfStmt(slice<ast.Node> path, nint index) {
    var stmt = path[index];
    for (var i = index + 1; i < len(path); i++) {
        {
            ptr<ast.IfStmt> (node, ok) = path[i]._<ptr<ast.IfStmt>>();

            if (ok && node.Else == stmt) {
                stmt = node;
                continue;
            }

        }

        break;

    }
    return stmt._<ast.Stmt>();

}

// WalkASTWithParent walks the AST rooted at n. The semantics are
// similar to ast.Inspect except it does not call f(nil).
public static bool WalkASTWithParent(ast.Node n, Func<ast.Node, ast.Node, bool> f) {
    slice<ast.Node> ancestors = default;
    ast.Inspect(n, n => {
        if (n == null) {
            ancestors = ancestors[..(int)len(ancestors) - 1];
            return false;
        }
        ast.Node parent = default;
        if (len(ancestors) > 0) {
            parent = ancestors[len(ancestors) - 1];
        }
        ancestors = append(ancestors, n);
        return f(n, parent);

    });

}

// FindMatchingIdents finds all identifiers in 'node' that match any of the given types.
// 'pos' represents the position at which the identifiers may be inserted. 'pos' must be within
// the scope of each of identifier we select. Otherwise, we will insert a variable at 'pos' that
// is unrecognized.
public static map<types.Type, slice<ptr<ast.Ident>>> FindMatchingIdents(slice<types.Type> typs, ast.Node node, token.Pos pos, ptr<types.Info> _addr_info, ptr<types.Package> _addr_pkg) {
    ref types.Info info = ref _addr_info.val;
    ref types.Package pkg = ref _addr_pkg.val;

    map matches = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<types.Type, slice<ptr<ast.Ident>>>{}; 
    // Initialize matches to contain the variable types we are searching for.
    {
        var typ__prev1 = typ;

        foreach (var (_, __typ) in typs) {
            typ = __typ;
            if (typ == null) {
                continue;
            }
            matches[typ] = new slice<ptr<ast.Ident>>(new ptr<ast.Ident>[] {  });
        }
        typ = typ__prev1;
    }

    ast.Inspect(node, n => {
        if (n == null) {
            return false;
        }
        ptr<ast.AssignStmt> (assignment, ok) = n._<ptr<ast.AssignStmt>>();
        if (ok && pos > assignment.Pos() && pos <= assignment.End()) {
            return false;
        }
        if (n.End() > pos) {
            return n.Pos() <= pos;
        }
        ptr<ast.Ident> (ident, ok) = n._<ptr<ast.Ident>>();
        if (!ok || ident.Name == "_") {
            return true;
        }
        var obj = info.Defs[ident];
        if (obj == null || obj.Type() == null) {
            return true;
        }
        {
            ptr<types.TypeName> (_, ok) = obj._<ptr<types.TypeName>>();

            if (ok) {
                return true;
            } 
            // Prevent duplicates in matches' values.

        } 
        // Prevent duplicates in matches' values.
        _, ok = seen[obj];

        if (ok) {
            return true;
        }
        seen[obj] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{}; 
        // Find the scope for the given position. Then, check whether the object
        // exists within the scope.
        var innerScope = pkg.Scope().Innermost(pos);
        if (innerScope == null) {
            return true;
        }
        var (_, foundObj) = innerScope.LookupParent(ident.Name, pos);
        if (foundObj != obj) {
            return true;
        }
        {
            var (idents, ok) = matches[obj.Type()];

            if (ok) {
                matches[obj.Type()] = append(idents, ast.NewIdent(ident.Name));
            } 
            // If the object type does not exactly match any of the target types, greedily
            // find the first target type that the object type can satisfy.

        } 
        // If the object type does not exactly match any of the target types, greedily
        // find the first target type that the object type can satisfy.
        {
            var typ__prev1 = typ;

            foreach (var (__typ) in matches) {
                typ = __typ;
                if (obj.Type() == typ) {
                    continue;
                }
                if (equivalentTypes(obj.Type(), typ)) {
                    matches[typ] = append(matches[typ], ast.NewIdent(ident.Name));
                }
            }

            typ = typ__prev1;
        }

        return true;

    });
    return matches;

}

private static bool equivalentTypes(types.Type want, types.Type got) {
    if (want == got || types.Identical(want, got)) {
        return true;
    }
    {
        ptr<types.Basic> (rhs, ok) = want._<ptr<types.Basic>>();

        if (ok && rhs.Info() & types.IsUntyped > 0) {
            {
                ptr<types.Basic> (lhs, ok) = got.Underlying()._<ptr<types.Basic>>();

                if (ok) {
                    return rhs.Info() & types.IsConstType == lhs.Info() & types.IsConstType;
                }

            }

        }
    }

    return types.AssignableTo(want, got);

}

// FindBestMatch employs fuzzy matching to evaluate the similarity of each given identifier to the
// given pattern. We return the identifier whose name is most similar to the pattern.
public static ast.Expr FindBestMatch(@string pattern, slice<ptr<ast.Ident>> idents) {
    var fuzz = fuzzy.NewMatcher(pattern);
    ast.Expr bestFuzz = default;
    var highScore = float32(0); // minimum score is 0 (no match)
    foreach (var (_, ident) in idents) { 
        // TODO: Improve scoring algorithm.
        var score = fuzz.Score(ident.Name);
        if (score > highScore) {
            highScore = score;
            bestFuzz = ident;
        }
        else if (score == 0) { 
            // Order matters in the fuzzy matching algorithm. If we find no match
            // when matching the target to the identifier, try matching the identifier
            // to the target.
            var revFuzz = fuzzy.NewMatcher(ident.Name);
            var revScore = revFuzz.Score(pattern);
            if (revScore > highScore) {
                highScore = revScore;
                bestFuzz = ident;
            }

        }
    }    return bestFuzz;

}

} // end analysisinternal_package
