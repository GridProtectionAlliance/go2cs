// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 06:32:36 UTC
// Original source: C:\Program Files\Go\src\cmd\gofmt\simplify.go
namespace go;

using ast = go.ast_package;
using token = go.token_package;
using reflect = reflect_package;

public static partial class main_package {

private partial struct simplifier {
}

private static ast.Visitor Visit(this simplifier s, ast.Node node) {
    switch (node.type()) {
        case ptr<ast.CompositeLit> n:
            var outer = n;
            ast.Expr keyType = default;            ast.Expr eltType = default;

            switch (outer.Type.type()) {
                case ptr<ast.ArrayType> typ:
                    eltType = typ.Elt;
                    break;
                case ptr<ast.MapType> typ:
                    keyType = typ.Key;
                    eltType = typ.Value;
                    break;

            }

            if (eltType != null) {
                reflect.Value ktyp = default;
                if (keyType != null) {
                    ktyp = reflect.ValueOf(keyType);
                }
                var typ = reflect.ValueOf(eltType);
                foreach (var (i, x) in outer.Elts) {
                    var px = _addr_outer.Elts[i]; 
                    // look at value of indexed/named elements
                    {
                        ptr<ast.KeyValueExpr> (t, ok) = x._<ptr<ast.KeyValueExpr>>();

                        if (ok) {
                            if (keyType != null) {
                                s.simplifyLiteral(ktyp, keyType, t.Key, _addr_t.Key);
                            }
                            x = t.Value;
                            px = _addr_t.Value;
                        }

                    }
                    s.simplifyLiteral(typ, eltType, x, px);
                } 
                // node was simplified - stop walk (there are no subnodes to simplify)
                return null;
            }
            break;
        case ptr<ast.SliceExpr> n:
            if (n.Max != null) { 
                // - 3-index slices always require the 2nd and 3rd index
                break;
            }
            {
                ptr<ast.Ident> (s, _) = n.X._<ptr<ast.Ident>>();

                if (s != null && s.Obj != null) { 
                    // the array/slice object is a single, resolved identifier
                    {
                        ptr<ast.CallExpr> (call, _) = n.High._<ptr<ast.CallExpr>>();

                        if (call != null && len(call.Args) == 1 && !call.Ellipsis.IsValid()) { 
                            // the high expression is a function call with a single argument
                            {
                                ptr<ast.Ident> (fun, _) = call.Fun._<ptr<ast.Ident>>();

                                if (fun != null && fun.Name == "len" && fun.Obj == null) { 
                                    // the function called is "len" and it is not locally defined; and
                                    // because we don't have dot imports, it must be the predefined len()
                                    {
                                        ptr<ast.Ident> (arg, _) = call.Args[0]._<ptr<ast.Ident>>();

                                        if (arg != null && arg.Obj == s.Obj) { 
                                            // the len argument is the array/slice object
                                            n.High = null;
                                        }

                                    }
                                }

                            }
                        }

                    }
                } 
                // Note: We could also simplify slice expressions of the form s[0:b] to s[:b]
                //       but we leave them as is since sometimes we want to be very explicit
                //       about the lower bound.
                // An example where the 0 helps:
                //       x, y, z := b[0:2], b[2:4], b[4:6]
                // An example where it does not:
                //       x, y := b[:n], b[n:]

            } 
            // Note: We could also simplify slice expressions of the form s[0:b] to s[:b]
            //       but we leave them as is since sometimes we want to be very explicit
            //       about the lower bound.
            // An example where the 0 helps:
            //       x, y, z := b[0:2], b[2:4], b[4:6]
            // An example where it does not:
            //       x, y := b[:n], b[n:]
            break;
        case ptr<ast.RangeStmt> n:
            if (isBlank(n.Value)) {
                n.Value = null;
            }
            if (isBlank(n.Key) && n.Value == null) {
                n.Key = null;
            }
            break;

    }

    return s;
}

private static void simplifyLiteral(this simplifier s, reflect.Value typ, ast.Expr astType, ast.Expr x, ptr<ast.Expr> _addr_px) {
    ref ast.Expr px = ref _addr_px.val;

    ast.Walk(s, x); // simplify x

    // if the element is a composite literal and its literal type
    // matches the outer literal's element type exactly, the inner
    // literal type may be omitted
    {
        ptr<ast.CompositeLit> inner__prev1 = inner;

        ptr<ast.CompositeLit> (inner, ok) = x._<ptr<ast.CompositeLit>>();

        if (ok) {
            if (match(null, typ, reflect.ValueOf(inner.Type))) {
                inner.Type = null;
            }
        }
        inner = inner__prev1;

    } 
    // if the outer literal's element type is a pointer type *T
    // and the element is & of a composite literal of type T,
    // the inner &T may be omitted.
    {
        ptr<ast.StarExpr> (ptr, ok) = astType._<ptr<ast.StarExpr>>();

        if (ok) {
            {
                ptr<ast.UnaryExpr> (addr, ok) = x._<ptr<ast.UnaryExpr>>();

                if (ok && addr.Op == token.AND) {
                    {
                        ptr<ast.CompositeLit> inner__prev3 = inner;

                        (inner, ok) = addr.X._<ptr<ast.CompositeLit>>();

                        if (ok) {
                            if (match(null, reflect.ValueOf(ptr.X), reflect.ValueOf(inner.Type))) {
                                inner.Type = null; // drop T
                                px = inner; // drop &
                            }
                        }

                        inner = inner__prev3;

                    }
                }

            }
        }
    }
}

private static bool isBlank(ast.Expr x) {
    ptr<ast.Ident> (ident, ok) = x._<ptr<ast.Ident>>();
    return ok && ident.Name == "_";
}

private static void simplify(ptr<ast.File> _addr_f) {
    ref ast.File f = ref _addr_f.val;
 
    // remove empty declarations such as "const ()", etc
    removeEmptyDeclGroups(_addr_f);

    simplifier s = default;
    ast.Walk(s, f);
}

private static void removeEmptyDeclGroups(ptr<ast.File> _addr_f) {
    ref ast.File f = ref _addr_f.val;

    nint i = 0;
    foreach (var (_, d) in f.Decls) {
        {
            ptr<ast.GenDecl> (g, ok) = d._<ptr<ast.GenDecl>>();

            if (!ok || !isEmpty(_addr_f, g)) {
                f.Decls[i] = d;
                i++;
            }

        }
    }    f.Decls = f.Decls[..(int)i];
}

private static bool isEmpty(ptr<ast.File> _addr_f, ptr<ast.GenDecl> _addr_g) {
    ref ast.File f = ref _addr_f.val;
    ref ast.GenDecl g = ref _addr_g.val;

    if (g.Doc != null || g.Specs != null) {
        return false;
    }
    foreach (var (_, c) in f.Comments) { 
        // if there is a comment in the declaration, it is not considered empty
        if (g.Pos() <= c.Pos() && c.End() <= g.End()) {
            return false;
        }
    }    return true;
}

} // end main_package
