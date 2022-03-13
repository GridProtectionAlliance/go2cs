// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package nilfunc defines an Analyzer that checks for useless
// comparisons against nil.

// package nilfunc -- go2cs converted at 2022 March 13 06:41:57 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/nilfunc" ==> using nilfunc = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.nilfunc_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\nilfunc\nilfunc.go
namespace go.cmd.vendor.golang.org.x.tools.go.analysis.passes;

using ast = go.ast_package;
using token = go.token_package;
using types = go.types_package;

using analysis = golang.org.x.tools.go.analysis_package;
using inspect = golang.org.x.tools.go.analysis.passes.inspect_package;
using inspector = golang.org.x.tools.go.ast.inspector_package;
using System;

public static partial class nilfunc_package {

public static readonly @string Doc = "check for useless comparisons between functions and nil\n\nA useless comparison is " +
    "one like f == nil as opposed to f() == nil.";



public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"nilfunc",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,));

private static (object, error) run(ptr<analysis.Pass> _addr_pass) {
    object _p0 = default;
    error _p0 = default!;
    ref analysis.Pass pass = ref _addr_pass.val;

    ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();

    ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.BinaryExpr)(nil) });
    inspect.Preorder(nodeFilter, n => {
        ptr<ast.BinaryExpr> e = n._<ptr<ast.BinaryExpr>>(); 

        // Only want == or != comparisons.
        if (e.Op != token.EQL && e.Op != token.NEQ) {
            return ;
        }
        ast.Expr e2 = default;

        if (pass.TypesInfo.Types[e.X].IsNil()) 
            e2 = e.Y;
        else if (pass.TypesInfo.Types[e.Y].IsNil()) 
            e2 = e.X;
        else 
            return ;
        // Only want identifiers or selector expressions.
        types.Object obj = default;
        switch (e2.type()) {
            case ptr<ast.Ident> v:
                obj = pass.TypesInfo.Uses[v];
                break;
            case ptr<ast.SelectorExpr> v:
                obj = pass.TypesInfo.Uses[v.Sel];
                break;
            default:
            {
                var v = e2.type();
                return ;
                break;
            } 

            // Only want functions.
        } 

        // Only want functions.
        {
            ptr<types.Func> (_, ok) = obj._<ptr<types.Func>>();

            if (!ok) {
                return ;
            }

        }

        pass.ReportRangef(e, "comparison of function %v %v nil is always %v", obj.Name(), e.Op, e.Op == token.NEQ);
    });
    return (null, error.As(null!)!);
}

} // end nilfunc_package
