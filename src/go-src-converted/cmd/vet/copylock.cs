// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains the code to check that locks are not passed by value.

// package main -- go2cs converted at 2020 August 29 10:08:51 UTC
// Original source: C:\Go\src\cmd\vet\copylock.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("copylocks", "check that locks are not passed by value", checkCopyLocks, funcDecl, rangeStmt, funcLit, callExpr, assignStmt, genDecl, compositeLit, returnStmt);
        }

        // checkCopyLocks checks whether node might
        // inadvertently copy a lock.
        private static void checkCopyLocks(ref File f, ast.Node node)
        {
            switch (node.type())
            {
                case ref ast.RangeStmt node:
                    checkCopyLocksRange(f, node);
                    break;
                case ref ast.FuncDecl node:
                    checkCopyLocksFunc(f, node.Name.Name, node.Recv, node.Type);
                    break;
                case ref ast.FuncLit node:
                    checkCopyLocksFunc(f, "func", null, node.Type);
                    break;
                case ref ast.CallExpr node:
                    checkCopyLocksCallExpr(f, node);
                    break;
                case ref ast.AssignStmt node:
                    checkCopyLocksAssign(f, node);
                    break;
                case ref ast.GenDecl node:
                    checkCopyLocksGenDecl(f, node);
                    break;
                case ref ast.CompositeLit node:
                    checkCopyLocksCompositeLit(f, node);
                    break;
                case ref ast.ReturnStmt node:
                    checkCopyLocksReturnStmt(f, node);
                    break;
            }
        }

        // checkCopyLocksAssign checks whether an assignment
        // copies a lock.
        private static void checkCopyLocksAssign(ref File f, ref ast.AssignStmt @as)
        {
            foreach (var (i, x) in @as.Rhs)
            {
                {
                    var path = lockPathRhs(f, x);

                    if (path != null)
                    {
                        f.Badf(x.Pos(), "assignment copies lock value to %v: %v", f.gofmt(@as.Lhs[i]), path);
                    }

                }
            }
        }

        // checkCopyLocksGenDecl checks whether lock is copied
        // in variable declaration.
        private static void checkCopyLocksGenDecl(ref File f, ref ast.GenDecl gd)
        {
            if (gd.Tok != token.VAR)
            {
                return;
            }
            foreach (var (_, spec) in gd.Specs)
            {
                ref ast.ValueSpec valueSpec = spec._<ref ast.ValueSpec>();
                foreach (var (i, x) in valueSpec.Values)
                {
                    {
                        var path = lockPathRhs(f, x);

                        if (path != null)
                        {
                            f.Badf(x.Pos(), "variable declaration copies lock value to %v: %v", valueSpec.Names[i].Name, path);
                        }

                    }
                }
            }
        }

        // checkCopyLocksCompositeLit detects lock copy inside a composite literal
        private static void checkCopyLocksCompositeLit(ref File f, ref ast.CompositeLit cl)
        {
            foreach (var (_, x) in cl.Elts)
            {
                {
                    ref ast.KeyValueExpr (node, ok) = x._<ref ast.KeyValueExpr>();

                    if (ok)
                    {
                        x = node.Value;
                    }

                }
                {
                    var path = lockPathRhs(f, x);

                    if (path != null)
                    {
                        f.Badf(x.Pos(), "literal copies lock value from %v: %v", f.gofmt(x), path);
                    }

                }
            }
        }

        // checkCopyLocksReturnStmt detects lock copy in return statement
        private static void checkCopyLocksReturnStmt(ref File f, ref ast.ReturnStmt rs)
        {
            foreach (var (_, x) in rs.Results)
            {
                {
                    var path = lockPathRhs(f, x);

                    if (path != null)
                    {
                        f.Badf(x.Pos(), "return copies lock value: %v", path);
                    }

                }
            }
        }

        // checkCopyLocksCallExpr detects lock copy in the arguments to a function call
        private static void checkCopyLocksCallExpr(ref File f, ref ast.CallExpr ce)
        {
            ref ast.Ident id = default;
            switch (ce.Fun.type())
            {
                case ref ast.Ident fun:
                    id = fun;
                    break;
                case ref ast.SelectorExpr fun:
                    id = fun.Sel;
                    break;
            }
            {
                var fun__prev1 = fun;

                ref types.Builtin (fun, ok) = f.pkg.uses[id]._<ref types.Builtin>();

                if (ok)
                {
                    switch (fun.Name())
                    {
                        case "new": 

                        case "len": 

                        case "cap": 

                        case "Sizeof": 
                            return;
                            break;
                    }
                }

                fun = fun__prev1;

            }
            foreach (var (_, x) in ce.Args)
            {
                {
                    var path = lockPathRhs(f, x);

                    if (path != null)
                    {
                        f.Badf(x.Pos(), "call of %s copies lock value: %v", f.gofmt(ce.Fun), path);
                    }

                }
            }
        }

        // checkCopyLocksFunc checks whether a function might
        // inadvertently copy a lock, by checking whether
        // its receiver, parameters, or return values
        // are locks.
        private static void checkCopyLocksFunc(ref File f, @string name, ref ast.FieldList recv, ref ast.FuncType typ)
        {
            if (recv != null && len(recv.List) > 0L)
            {
                var expr = recv.List[0L].Type;
                {
                    var path__prev2 = path;

                    var path = lockPath(f.pkg.typesPkg, f.pkg.types[expr].Type);

                    if (path != null)
                    {
                        f.Badf(expr.Pos(), "%s passes lock by value: %v", name, path);
                    }

                    path = path__prev2;

                }
            }
            if (typ.Params != null)
            {
                foreach (var (_, field) in typ.Params.List)
                {
                    expr = field.Type;
                    {
                        var path__prev2 = path;

                        path = lockPath(f.pkg.typesPkg, f.pkg.types[expr].Type);

                        if (path != null)
                        {
                            f.Badf(expr.Pos(), "%s passes lock by value: %v", name, path);
                        }

                        path = path__prev2;

                    }
                }
            } 

            // Don't check typ.Results. If T has a Lock field it's OK to write
            //     return T{}
            // because that is returning the zero value. Leave result checking
            // to the return statement.
        }

        // checkCopyLocksRange checks whether a range statement
        // might inadvertently copy a lock by checking whether
        // any of the range variables are locks.
        private static void checkCopyLocksRange(ref File f, ref ast.RangeStmt r)
        {
            checkCopyLocksRangeVar(f, r.Tok, r.Key);
            checkCopyLocksRangeVar(f, r.Tok, r.Value);
        }

        private static void checkCopyLocksRangeVar(ref File f, token.Token rtok, ast.Expr e)
        {
            if (e == null)
            {
                return;
            }
            ref ast.Ident (id, isId) = e._<ref ast.Ident>();
            if (isId && id.Name == "_")
            {
                return;
            }
            types.Type typ = default;
            if (rtok == token.DEFINE)
            {
                if (!isId)
                {
                    return;
                }
                var obj = f.pkg.defs[id];
                if (obj == null)
                {
                    return;
                }
                typ = obj.Type();
            }
            else
            {
                typ = f.pkg.types[e].Type;
            }
            if (typ == null)
            {
                return;
            }
            {
                var path = lockPath(f.pkg.typesPkg, typ);

                if (path != null)
                {
                    f.Badf(e.Pos(), "range var %s copies lock: %v", f.gofmt(e), path);
                }

            }
        }

        private partial struct typePath // : slice<types.Type>
        {
        }

        // String pretty-prints a typePath.
        private static @string String(this typePath path)
        {
            var n = len(path);
            bytes.Buffer buf = default;
            foreach (var (i) in path)
            {
                if (i > 0L)
                {
                    fmt.Fprint(ref buf, " contains ");
                } 
                // The human-readable path is in reverse order, outermost to innermost.
                fmt.Fprint(ref buf, path[n - i - 1L].String());
            }
            return buf.String();
        }

        private static typePath lockPathRhs(ref File f, ast.Expr x)
        {
            {
                ref ast.CompositeLit (_, ok) = x._<ref ast.CompositeLit>();

                if (ok)
                {
                    return null;
                }

            }
            {
                (_, ok) = x._<ref ast.CallExpr>();

                if (ok)
                { 
                    // A call may return a zero value.
                    return null;
                }

            }
            {
                ref ast.StarExpr (star, ok) = x._<ref ast.StarExpr>();

                if (ok)
                {
                    {
                        (_, ok) = star.X._<ref ast.CallExpr>();

                        if (ok)
                        { 
                            // A call may return a pointer to a zero value.
                            return null;
                        }

                    }
                }

            }
            return lockPath(f.pkg.typesPkg, f.pkg.types[x].Type);
        }

        // lockPath returns a typePath describing the location of a lock value
        // contained in typ. If there is no contained lock, it returns nil.
        private static typePath lockPath(ref types.Package tpkg, types.Type typ)
        {
            if (typ == null)
            {
                return null;
            }
            while (true)
            {
                ref types.Array (atyp, ok) = typ.Underlying()._<ref types.Array>();
                if (!ok)
                {
                    break;
                }
                typ = atyp.Elem();
            } 

            // We're only interested in the case in which the underlying
            // type is a struct. (Interfaces and pointers are safe to copy.)
 

            // We're only interested in the case in which the underlying
            // type is a struct. (Interfaces and pointers are safe to copy.)
            ref types.Struct (styp, ok) = typ.Underlying()._<ref types.Struct>();
            if (!ok)
            {
                return null;
            } 

            // We're looking for cases in which a reference to this type
            // can be locked, but a value cannot. This differentiates
            // embedded interfaces from embedded values.
            {
                var plock = types.NewMethodSet(types.NewPointer(typ)).Lookup(tpkg, "Lock");

                if (plock != null)
                {
                    {
                        var @lock = types.NewMethodSet(typ).Lookup(tpkg, "Lock");

                        if (lock == null)
                        {
                            return new slice<types.Type>(new types.Type[] { typ });
                        }

                    }
                }

            }

            var nfields = styp.NumFields();
            for (long i = 0L; i < nfields; i++)
            {
                var ftyp = styp.Field(i).Type();
                var subpath = lockPath(tpkg, ftyp);
                if (subpath != null)
                {
                    return append(subpath, typ);
                }
            }


            return null;
        }
    }
}
