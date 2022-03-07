// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package unsafeptr defines an Analyzer that checks for invalid
// conversions of uintptr to unsafe.Pointer.
// package unsafeptr -- go2cs converted at 2022 March 06 23:34:19 UTC
// import "golang.org/x/tools/go/analysis/passes/unsafeptr" ==> using unsafeptr = go.golang.org.x.tools.go.analysis.passes.unsafeptr_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\unsafeptr\unsafeptr.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using System;


namespace go.golang.org.x.tools.go.analysis.passes;

public static partial class unsafeptr_package {

public static readonly @string Doc = @"check for invalid conversions of uintptr to unsafe.Pointer

The unsafeptr analyzer reports likely incorrect uses of unsafe.Pointer
to convert integers to pointers. A conversion from uintptr to
unsafe.Pointer is invalid if it implies that there is a uintptr-typed
word in memory that holds a pointer value, because that word will be
invisible to stack copying and to the garbage collector.";



public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"unsafeptr",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,));

private static (object, error) run(ptr<analysis.Pass> _addr_pass) {
    object _p0 = default;
    error _p0 = default!;
    ref analysis.Pass pass = ref _addr_pass.val;

    ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();

    ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.CallExpr)(nil) });
    inspect.Preorder(nodeFilter, n => {
        ptr<ast.CallExpr> x = n._<ptr<ast.CallExpr>>();
        if (len(x.Args) != 1) {
            return ;
        }
        if (hasBasicType(_addr_pass.TypesInfo, x.Fun, types.UnsafePointer) && hasBasicType(_addr_pass.TypesInfo, x.Args[0], types.Uintptr) && !isSafeUintptr(_addr_pass.TypesInfo, x.Args[0])) {
            pass.ReportRangef(x, "possible misuse of unsafe.Pointer");
        }
    });
    return (null, error.As(null!)!);

}

// isSafeUintptr reports whether x - already known to be a uintptr -
// is safe to convert to unsafe.Pointer. It is safe if x is itself derived
// directly from an unsafe.Pointer via conversion and pointer arithmetic
// or if x is the result of reflect.Value.Pointer or reflect.Value.UnsafeAddr
// or obtained from the Data field of a *reflect.SliceHeader or *reflect.StringHeader.
private static bool isSafeUintptr(ptr<types.Info> _addr_info, ast.Expr x) {
    ref types.Info info = ref _addr_info.val;

    switch (x.type()) {
        case ptr<ast.ParenExpr> x:
            return isSafeUintptr(_addr_info, x.X);
            break;
        case ptr<ast.SelectorExpr> x:
            if (x.Sel.Name != "Data") {
                break;
            } 
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
            ptr<types.Pointer> (pt, ok) = info.Types[x.X].Type._<ptr<types.Pointer>>();
            if (ok) {
                ptr<types.Named> (t, ok) = pt.Elem()._<ptr<types.Named>>();
                if (ok && t.Obj().Pkg().Path() == "reflect") {
                    switch (t.Obj().Name()) {
                        case "StringHeader": 

                        case "SliceHeader": 
                            return true;
                            break;
                    }

                }

            }

            break;
        case ptr<ast.CallExpr> x:
            switch (len(x.Args)) {
                case 0: 
                    // maybe call to reflect.Value.Pointer or reflect.Value.UnsafeAddr.
                    ptr<ast.SelectorExpr> (sel, ok) = x.Fun._<ptr<ast.SelectorExpr>>();
                    if (!ok) {
                        break;
                    }
                    switch (sel.Sel.Name) {
                        case "Pointer": 

                        case "UnsafeAddr": 
                            (t, ok) = info.Types[sel.X].Type._<ptr<types.Named>>();
                            if (ok && t.Obj().Pkg().Path() == "reflect" && t.Obj().Name() == "Value") {
                                return true;
                            }
                            break;
                    }

                    break;
                case 1: 
                    // maybe conversion of uintptr to unsafe.Pointer
                    return hasBasicType(_addr_info, x.Fun, types.Uintptr) && hasBasicType(_addr_info, x.Args[0], types.UnsafePointer);
                    break;
            }
            break;
        case ptr<ast.BinaryExpr> x:

            if (x.Op == token.ADD || x.Op == token.SUB || x.Op == token.AND_NOT) 
                return isSafeUintptr(_addr_info, x.X) && !isSafeUintptr(_addr_info, x.Y);
                        break;
    }
    return false;

}

// hasBasicType reports whether x's type is a types.Basic with the given kind.
private static bool hasBasicType(ptr<types.Info> _addr_info, ast.Expr x, types.BasicKind kind) {
    ref types.Info info = ref _addr_info.val;

    var t = info.Types[x].Type;
    if (t != null) {
        t = t.Underlying();
    }
    ptr<types.Basic> (b, ok) = t._<ptr<types.Basic>>();
    return ok && b.Kind() == kind;

}

} // end unsafeptr_package
