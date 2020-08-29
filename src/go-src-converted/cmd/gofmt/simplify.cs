// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 10:02:12 UTC
// Original source: C:\Go\src\cmd\gofmt\simplify.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using reflect = go.reflect_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private partial struct simplifier
        {
        }

        private static ast.Visitor Visit(this simplifier s, ast.Node node)
        {
            switch (node.type())
            {
                case ref ast.CompositeLit n:
                    var outer = n;
                    ast.Expr keyType = default;                    ast.Expr eltType = default;

                    switch (outer.Type.type())
                    {
                        case ref ast.ArrayType typ:
                            eltType = typ.Elt;
                            break;
                        case ref ast.MapType typ:
                            keyType = typ.Key;
                            eltType = typ.Value;
                            break;

                    }

                    if (eltType != null)
                    {
                        reflect.Value ktyp = default;
                        if (keyType != null)
                        {
                            ktyp = reflect.ValueOf(keyType);
                        }
                        var typ = reflect.ValueOf(eltType);
                        foreach (var (i, x) in outer.Elts)
                        {
                            var px = ref outer.Elts[i]; 
                            // look at value of indexed/named elements
                            {
                                ref ast.KeyValueExpr (t, ok) = x._<ref ast.KeyValueExpr>();

                                if (ok)
                                {
                                    if (keyType != null)
                                    {
                                        s.simplifyLiteral(ktyp, keyType, t.Key, ref t.Key);
                                    }
                                    x = t.Value;
                                    px = ref t.Value;
                                }

                            }
                            s.simplifyLiteral(typ, eltType, x, px);
                        } 
                        // node was simplified - stop walk (there are no subnodes to simplify)
                        return null;
                    }
                    break;
                case ref ast.SliceExpr n:
                    if (n.Max != null)
                    { 
                        // - 3-index slices always require the 2nd and 3rd index
                        break;
                    }
                    {
                        ref ast.Ident (s, _) = n.X._<ref ast.Ident>();

                        if (s != null && s.Obj != null)
                        { 
                            // the array/slice object is a single, resolved identifier
                            {
                                ref ast.CallExpr (call, _) = n.High._<ref ast.CallExpr>();

                                if (call != null && len(call.Args) == 1L && !call.Ellipsis.IsValid())
                                { 
                                    // the high expression is a function call with a single argument
                                    {
                                        ref ast.Ident (fun, _) = call.Fun._<ref ast.Ident>();

                                        if (fun != null && fun.Name == "len" && fun.Obj == null)
                                        { 
                                            // the function called is "len" and it is not locally defined; and
                                            // because we don't have dot imports, it must be the predefined len()
                                            {
                                                ref ast.Ident (arg, _) = call.Args[0L]._<ref ast.Ident>();

                                                if (arg != null && arg.Obj == s.Obj)
                                                { 
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
                case ref ast.RangeStmt n:
                    if (isBlank(n.Value))
                    {
                        n.Value = null;
                    }
                    if (isBlank(n.Key) && n.Value == null)
                    {
                        n.Key = null;
                    }
                    break;

            }

            return s;
        }

        private static void simplifyLiteral(this simplifier s, reflect.Value typ, ast.Expr astType, ast.Expr x, ref ast.Expr px)
        {
            ast.Walk(s, x); // simplify x

            // if the element is a composite literal and its literal type
            // matches the outer literal's element type exactly, the inner
            // literal type may be omitted
            {
                ref ast.CompositeLit inner__prev1 = inner;

                ref ast.CompositeLit (inner, ok) = x._<ref ast.CompositeLit>();

                if (ok)
                {
                    if (match(null, typ, reflect.ValueOf(inner.Type)))
                    {
                        inner.Type = null;
                    }
                } 
                // if the outer literal's element type is a pointer type *T
                // and the element is & of a composite literal of type T,
                // the inner &T may be omitted.

                inner = inner__prev1;

            } 
            // if the outer literal's element type is a pointer type *T
            // and the element is & of a composite literal of type T,
            // the inner &T may be omitted.
            {
                ref ast.StarExpr (ptr, ok) = astType._<ref ast.StarExpr>();

                if (ok)
                {
                    {
                        ref ast.UnaryExpr (addr, ok) = x._<ref ast.UnaryExpr>();

                        if (ok && addr.Op == token.AND)
                        {
                            {
                                ref ast.CompositeLit inner__prev3 = inner;

                                (inner, ok) = addr.X._<ref ast.CompositeLit>();

                                if (ok)
                                {
                                    if (match(null, reflect.ValueOf(ptr.X), reflect.ValueOf(inner.Type)))
                                    {
                                        inner.Type = null; // drop T
                                        px.Value = inner; // drop &
                                    }
                                }

                                inner = inner__prev3;

                            }
                        }

                    }
                }

            }
        }

        private static bool isBlank(ast.Expr x)
        {
            ref ast.Ident (ident, ok) = x._<ref ast.Ident>();
            return ok && ident.Name == "_";
        }

        private static void simplify(ref ast.File f)
        { 
            // remove empty declarations such as "const ()", etc
            removeEmptyDeclGroups(f);

            simplifier s = default;
            ast.Walk(s, f);
        }

        private static void removeEmptyDeclGroups(ref ast.File f)
        {
            long i = 0L;
            foreach (var (_, d) in f.Decls)
            {
                {
                    ref ast.GenDecl (g, ok) = d._<ref ast.GenDecl>();

                    if (!ok || !isEmpty(f, g))
                    {
                        f.Decls[i] = d;
                        i++;
                    }

                }
            }
            f.Decls = f.Decls[..i];
        }

        private static bool isEmpty(ref ast.File f, ref ast.GenDecl g)
        {
            if (g.Doc != null || g.Specs != null)
            {
                return false;
            }
            foreach (var (_, c) in f.Comments)
            { 
                // if there is a comment in the declaration, it is not considered empty
                if (g.Pos() <= c.Pos() && c.End() <= g.End())
                {
                    return false;
                }
            }
            return true;
        }
    }
}
