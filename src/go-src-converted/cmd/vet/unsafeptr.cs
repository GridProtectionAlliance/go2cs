// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Check for invalid uintptr -> unsafe.Pointer conversions.

// package main -- go2cs converted at 2020 August 29 10:09:32 UTC
// Original source: C:\Go\src\cmd\vet\unsafeptr.go
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
            register("unsafeptr", "check for misuse of unsafe.Pointer", checkUnsafePointer, callExpr);
        }

        private static void checkUnsafePointer(ref File f, ast.Node node)
        {
            ref ast.CallExpr x = node._<ref ast.CallExpr>();
            if (len(x.Args) != 1L)
            {
                return;
            }
            if (f.hasBasicType(x.Fun, types.UnsafePointer) && f.hasBasicType(x.Args[0L], types.Uintptr) && !f.isSafeUintptr(x.Args[0L]))
            {
                f.Badf(x.Pos(), "possible misuse of unsafe.Pointer");
            }
        }

        // isSafeUintptr reports whether x - already known to be a uintptr -
        // is safe to convert to unsafe.Pointer. It is safe if x is itself derived
        // directly from an unsafe.Pointer via conversion and pointer arithmetic
        // or if x is the result of reflect.Value.Pointer or reflect.Value.UnsafeAddr
        // or obtained from the Data field of a *reflect.SliceHeader or *reflect.StringHeader.
        private static bool isSafeUintptr(this ref File f, ast.Expr x)
        {
            switch (x.type())
            {
                case ref ast.ParenExpr x:
                    return f.isSafeUintptr(x.X);
                    break;
                case ref ast.SelectorExpr x:
                    switch (x.Sel.Name)
                    {
                        case "Data": 
                            // reflect.SliceHeader and reflect.StringHeader are okay,
                            // but only if they are pointing at a real slice or string.
                            // It's not okay to do:
                            //    var x SliceHeader
                            //    x.Data = uintptr(unsafe.Pointer(...))
                            //    ... use x ...
                            //    p := unsafe.Pointer(x.Data)
                            // because in the middle the garbage collector doesn't
                            // see x.Data as a pointer and so x.Data may be dangling
                            // by the time we get to the conversion at the end.
                            // For now approximate by saying that *Header is okay
                            // but Header is not.
                            ref types.Pointer (pt, ok) = f.pkg.types[x.X].Type._<ref types.Pointer>();
                            if (ok)
                            {
                                ref types.Named (t, ok) = pt.Elem()._<ref types.Named>();
                                if (ok && t.Obj().Pkg().Path() == "reflect")
                                {
                                    switch (t.Obj().Name())
                                    {
                                        case "StringHeader": 

                                        case "SliceHeader": 
                                            return true;
                                            break;
                                    }
                                }
                            }
                            break;
                    }
                    break;
                case ref ast.CallExpr x:
                    switch (len(x.Args))
                    {
                        case 0L: 
                            // maybe call to reflect.Value.Pointer or reflect.Value.UnsafeAddr.
                            ref ast.SelectorExpr (sel, ok) = x.Fun._<ref ast.SelectorExpr>();
                            if (!ok)
                            {
                                break;
                            }
                            switch (sel.Sel.Name)
                            {
                                case "Pointer": 

                                case "UnsafeAddr": 
                                    (t, ok) = f.pkg.types[sel.X].Type._<ref types.Named>();
                                    if (ok && t.Obj().Pkg().Path() == "reflect" && t.Obj().Name() == "Value")
                                    {
                                        return true;
                                    }
                                    break;
                            }
                            break;
                        case 1L: 
                            // maybe conversion of uintptr to unsafe.Pointer
                            return f.hasBasicType(x.Fun, types.Uintptr) && f.hasBasicType(x.Args[0L], types.UnsafePointer);
                            break;
                    }
                    break;
                case ref ast.BinaryExpr x:

                    if (x.Op == token.ADD || x.Op == token.SUB || x.Op == token.AND_NOT) 
                        return f.isSafeUintptr(x.X) && !f.isSafeUintptr(x.Y);
                                        break;
            }
            return false;
        }
    }
}
