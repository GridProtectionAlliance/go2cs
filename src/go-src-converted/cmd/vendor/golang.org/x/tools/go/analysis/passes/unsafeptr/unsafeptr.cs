// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package unsafeptr defines an Analyzer that checks for invalid
// conversions of uintptr to unsafe.Pointer.

// package unsafeptr -- go2cs converted at 2022 March 13 06:42:09 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/unsafeptr" ==> using unsafeptr = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.unsafeptr_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\unsafeptr\unsafeptr.go
namespace go.cmd.vendor.golang.org.x.tools.go.analysis.passes;

using ast = go.ast_package;
using token = go.token_package;
using types = go.types_package;

using analysis = golang.org.x.tools.go.analysis_package;
using inspect = golang.org.x.tools.go.analysis.passes.inspect_package;
using analysisutil = golang.org.x.tools.go.analysis.passes.@internal.analysisutil_package;
using inspector = golang.org.x.tools.go.ast.inspector_package;
using System;

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

    ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.CallExpr)(nil), (*ast.StarExpr)(nil), (*ast.UnaryExpr)(nil) });
    inspect.Preorder(nodeFilter, n => {
        switch (n.type()) {
            case ptr<ast.CallExpr> x:
                if (len(x.Args) == 1 && hasBasicType(_addr_pass.TypesInfo, x.Fun, types.UnsafePointer) && hasBasicType(_addr_pass.TypesInfo, x.Args[0], types.Uintptr) && !isSafeUintptr(_addr_pass.TypesInfo, x.Args[0])) {
                    pass.ReportRangef(x, "possible misuse of unsafe.Pointer");
                }
                break;
            case ptr<ast.StarExpr> x:
                {
                    var t__prev1 = t;

                    var t = pass.TypesInfo.Types[x].Type;

                    if (isReflectHeader(t)) {
                        pass.ReportRangef(x, "possible misuse of %s", t);
                    }

                    t = t__prev1;

                }
                break;
            case ptr<ast.UnaryExpr> x:
                if (x.Op != token.AND) {
                    return ;
                }
                {
                    var t__prev1 = t;

                    t = pass.TypesInfo.Types[x.X].Type;

                    if (isReflectHeader(t)) {
                        pass.ReportRangef(x, "possible misuse of %s", t);
                    }

                    t = t__prev1;

                }
                break;
        }
    });
    return (null, error.As(null!)!);
}

// isSafeUintptr reports whether x - already known to be a uintptr -
// is safe to convert to unsafe.Pointer.
private static bool isSafeUintptr(ptr<types.Info> _addr_info, ast.Expr x) {
    ref types.Info info = ref _addr_info.val;
 
    // Check unsafe.Pointer safety rules according to
    // https://golang.org/pkg/unsafe/#Pointer.

    switch (analysisutil.Unparen(x).type()) {
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
            if (ok && isReflectHeader(pt.Elem())) {
                return true;
            }
            break;
        case ptr<ast.CallExpr> x:
            if (len(x.Args) != 0) {
                break;
            }
            ptr<ast.SelectorExpr> (sel, ok) = x.Fun._<ptr<ast.SelectorExpr>>();
            if (!ok) {
                break;
            }
            switch (sel.Sel.Name) {
                case "Pointer": 

                case "UnsafeAddr": 
                    ptr<types.Named> (t, ok) = info.Types[sel.X].Type._<ptr<types.Named>>();
                    if (ok && t.Obj().Pkg().Path() == "reflect" && t.Obj().Name() == "Value") {
                        return true;
                    }
                    break;
            }
            break; 

        // "(3) Conversion of a Pointer to a uintptr and back, with arithmetic."
    } 

    // "(3) Conversion of a Pointer to a uintptr and back, with arithmetic."
    return isSafeArith(_addr_info, x);
}

// isSafeArith reports whether x is a pointer arithmetic expression that is safe
// to convert to unsafe.Pointer.
private static bool isSafeArith(ptr<types.Info> _addr_info, ast.Expr x) {
    ref types.Info info = ref _addr_info.val;

    switch (analysisutil.Unparen(x).type()) {
        case ptr<ast.CallExpr> x:
            return len(x.Args) == 1 && hasBasicType(_addr_info, x.Fun, types.Uintptr) && hasBasicType(_addr_info, x.Args[0], types.UnsafePointer);
            break;
        case ptr<ast.BinaryExpr> x:

            if (x.Op == token.ADD || x.Op == token.SUB || x.Op == token.AND_NOT) 
                // TODO(mdempsky): Match compiler
                // semantics. ADD allows a pointer on either
                // side; SUB and AND_NOT don't care about RHS.
                return isSafeArith(_addr_info, x.X) && !isSafeArith(_addr_info, x.Y);
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

// isReflectHeader reports whether t is reflect.SliceHeader or reflect.StringHeader.
private static bool isReflectHeader(types.Type t) {
    {
        ptr<types.Named> (named, ok) = t._<ptr<types.Named>>();

        if (ok) {
            {
                var obj = named.Obj();

                if (obj.Pkg() != null && obj.Pkg().Path() == "reflect") {
                    switch (obj.Name()) {
                        case "SliceHeader": 

                        case "StringHeader": 
                            return true;
                            break;
                    }
                }

            }
        }
    }
    return false;
}

} // end unsafeptr_package
