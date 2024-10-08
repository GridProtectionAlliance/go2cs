// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The errorsas package defines an Analyzer that checks that the second argument to
// errors.As is a pointer to a type implementing error.

// package errorsas -- go2cs converted at 2022 March 13 06:41:51 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/errorsas" ==> using errorsas = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.errorsas_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\errorsas\errorsas.go
namespace go.cmd.vendor.golang.org.x.tools.go.analysis.passes;

using ast = go.ast_package;
using types = go.types_package;

using analysis = golang.org.x.tools.go.analysis_package;
using inspect = golang.org.x.tools.go.analysis.passes.inspect_package;
using inspector = golang.org.x.tools.go.ast.inspector_package;
using typeutil = golang.org.x.tools.go.types.typeutil_package;
using System;

public static partial class errorsas_package {

public static readonly @string Doc = "report passing non-pointer or non-error values to errors.As\n\nThe errorsas analysi" +
    "s reports calls to errors.As where the type\nof the second argument is not a poin" +
    "ter to a type implementing error.";



public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"errorsas",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,));

private static (object, error) run(ptr<analysis.Pass> _addr_pass) {
    object _p0 = default;
    error _p0 = default!;
    ref analysis.Pass pass = ref _addr_pass.val;

    switch (pass.Pkg.Path()) {
        case "errors": 
            // These packages know how to use their own APIs.
            // Sometimes they are testing what happens to incorrect programs.

        case "errors_test": 
            // These packages know how to use their own APIs.
            // Sometimes they are testing what happens to incorrect programs.
            return (null, error.As(null!)!);
            break;
    }

    ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();

    ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.CallExpr)(nil) });
    inspect.Preorder(nodeFilter, n => {
        ptr<ast.CallExpr> call = n._<ptr<ast.CallExpr>>();
        var fn = typeutil.StaticCallee(pass.TypesInfo, call);
        if (fn == null) {
            return ; // not a static call
        }
        if (len(call.Args) < 2) {
            return ; // not enough arguments, e.g. called with return values of another function
        }
        if (fn.FullName() == "errors.As" && !pointerToInterfaceOrError(_addr_pass, call.Args[1])) {
            pass.ReportRangef(call, "second argument to errors.As must be a non-nil pointer to either a type that implements error, or to any interface type");
        }
    });
    return (null, error.As(null!)!);
}

private static ptr<types.Interface> errorType = types.Universe.Lookup("error").Type().Underlying()._<ptr<types.Interface>>();

// pointerToInterfaceOrError reports whether the type of e is a pointer to an interface or a type implementing error,
// or is the empty interface.
private static bool pointerToInterfaceOrError(ptr<analysis.Pass> _addr_pass, ast.Expr e) {
    ref analysis.Pass pass = ref _addr_pass.val;

    var t = pass.TypesInfo.Types[e].Type;
    {
        ptr<types.Interface> (it, ok) = t.Underlying()._<ptr<types.Interface>>();

        if (ok && it.NumMethods() == 0) {
            return true;
        }
    }
    ptr<types.Pointer> (pt, ok) = t.Underlying()._<ptr<types.Pointer>>();
    if (!ok) {
        return false;
    }
    _, ok = pt.Elem().Underlying()._<ptr<types.Interface>>();
    return ok || types.Implements(pt.Elem(), errorType);
}

} // end errorsas_package
