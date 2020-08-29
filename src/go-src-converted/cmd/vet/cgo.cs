// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Check for invalid cgo pointer passing.
// This looks for code that uses cgo to call C code passing values
// whose types are almost always invalid according to the cgo pointer
// sharing rules.
// Specifically, it warns about attempts to pass a Go chan, map, func,
// or slice to C, either directly, or via a pointer, array, or struct.

// package main -- go2cs converted at 2020 August 29 10:08:49 UTC
// Original source: C:\Go\src\cmd\vet\cgo.go
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
            register("cgocall", "check for types that may not be passed to cgo calls", checkCgoCall, callExpr);
        }

        private static void checkCgoCall(ref File f, ast.Node node)
        {
            ref ast.CallExpr x = node._<ref ast.CallExpr>(); 

            // We are only looking for calls to functions imported from
            // the "C" package.
            ref ast.SelectorExpr (sel, ok) = x.Fun._<ref ast.SelectorExpr>();
            if (!ok)
            {
                return;
            }
            ref ast.Ident (id, ok) = sel.X._<ref ast.Ident>();
            if (!ok)
            {
                return;
            }
            ref types.PkgName (pkgname, ok) = f.pkg.uses[id]._<ref types.PkgName>();
            if (!ok || pkgname.Imported().Path() != "C")
            {
                return;
            } 

            // A call to C.CBytes passes a pointer but is always safe.
            if (sel.Sel.Name == "CBytes")
            {
                return;
            }
            foreach (var (_, arg) in x.Args)
            {
                if (!typeOKForCgoCall(cgoBaseType(f, arg), make_map<types.Type, bool>()))
                {
                    f.Badf(arg.Pos(), "possibly passing Go type with embedded pointer to C");
                } 

                // Check for passing the address of a bad type.
                {
                    ref ast.CallExpr (conv, ok) = arg._<ref ast.CallExpr>();

                    if (ok && len(conv.Args) == 1L && f.hasBasicType(conv.Fun, types.UnsafePointer))
                    {
                        arg = conv.Args[0L];
                    }

                }
                {
                    ref ast.UnaryExpr (u, ok) = arg._<ref ast.UnaryExpr>();

                    if (ok && u.Op == token.AND)
                    {
                        if (!typeOKForCgoCall(cgoBaseType(f, u.X), make_map<types.Type, bool>()))
                        {
                            f.Badf(arg.Pos(), "possibly passing Go type with embedded pointer to C");
                        }
                    }

                }
            }
        }

        // cgoBaseType tries to look through type conversions involving
        // unsafe.Pointer to find the real type. It converts:
        //   unsafe.Pointer(x) => x
        //   *(*unsafe.Pointer)(unsafe.Pointer(&x)) => x
        private static types.Type cgoBaseType(ref File f, ast.Expr arg)
        {
            switch (arg.type())
            {
                case ref ast.CallExpr arg:
                    if (len(arg.Args) == 1L && f.hasBasicType(arg.Fun, types.UnsafePointer))
                    {
                        return cgoBaseType(f, arg.Args[0L]);
                    }
                    break;
                case ref ast.StarExpr arg:
                    ref ast.CallExpr (call, ok) = arg.X._<ref ast.CallExpr>();
                    if (!ok || len(call.Args) != 1L)
                    {
                        break;
                    } 
                    // Here arg is *f(v).
                    var t = f.pkg.types[call.Fun].Type;
                    if (t == null)
                    {
                        break;
                    }
                    ref types.Pointer (ptr, ok) = t.Underlying()._<ref types.Pointer>();
                    if (!ok)
                    {
                        break;
                    } 
                    // Here arg is *(*p)(v)
                    ref types.Basic (elem, ok) = ptr.Elem().Underlying()._<ref types.Basic>();
                    if (!ok || elem.Kind() != types.UnsafePointer)
                    {
                        break;
                    } 
                    // Here arg is *(*unsafe.Pointer)(v)
                    call, ok = call.Args[0L]._<ref ast.CallExpr>();
                    if (!ok || len(call.Args) != 1L)
                    {
                        break;
                    } 
                    // Here arg is *(*unsafe.Pointer)(f(v))
                    if (!f.hasBasicType(call.Fun, types.UnsafePointer))
                    {
                        break;
                    } 
                    // Here arg is *(*unsafe.Pointer)(unsafe.Pointer(v))
                    ref ast.UnaryExpr (u, ok) = call.Args[0L]._<ref ast.UnaryExpr>();
                    if (!ok || u.Op != token.AND)
                    {
                        break;
                    } 
                    // Here arg is *(*unsafe.Pointer)(unsafe.Pointer(&v))
                    return cgoBaseType(f, u.X);
                    break;

            }

            return f.pkg.types[arg].Type;
        }

        // typeOKForCgoCall reports whether the type of arg is OK to pass to a
        // C function using cgo. This is not true for Go types with embedded
        // pointers. m is used to avoid infinite recursion on recursive types.
        private static bool typeOKForCgoCall(types.Type t, map<types.Type, bool> m)
        {
            if (t == null || m[t])
            {
                return true;
            }
            m[t] = true;
            switch (t.Underlying().type())
            {
                case ref types.Chan t:
                    return false;
                    break;
                case ref types.Map t:
                    return false;
                    break;
                case ref types.Signature t:
                    return false;
                    break;
                case ref types.Slice t:
                    return false;
                    break;
                case ref types.Pointer t:
                    return typeOKForCgoCall(t.Elem(), m);
                    break;
                case ref types.Array t:
                    return typeOKForCgoCall(t.Elem(), m);
                    break;
                case ref types.Struct t:
                    for (long i = 0L; i < t.NumFields(); i++)
                    {
                        if (!typeOKForCgoCall(t.Field(i).Type(), m))
                        {
                            return false;
                        }
                    }
                    break;
            }
            return true;
        }
    }
}
