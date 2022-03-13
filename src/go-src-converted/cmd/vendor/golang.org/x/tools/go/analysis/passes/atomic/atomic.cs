// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package atomic defines an Analyzer that checks for common mistakes
// using the sync/atomic package.

// package atomic -- go2cs converted at 2022 March 13 06:41:45 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/atomic" ==> using atomic = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.atomic_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\atomic\atomic.go
namespace go.cmd.vendor.golang.org.x.tools.go.analysis.passes;

using ast = go.ast_package;
using token = go.token_package;
using types = go.types_package;

using analysis = golang.org.x.tools.go.analysis_package;
using inspect = golang.org.x.tools.go.analysis.passes.inspect_package;
using analysisutil = golang.org.x.tools.go.analysis.passes.@internal.analysisutil_package;
using inspector = golang.org.x.tools.go.ast.inspector_package;
using System;

public static partial class atomic_package {

public static readonly @string Doc = "check for common mistakes using the sync/atomic package\n\nThe atomic checker looks" +
    " for assignment statements of the form:\n\n\tx = atomic.AddUint64(&x, 1)\n\nwhich are" +
    " not atomic.";



public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"atomic",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},RunDespiteErrors:true,Run:run,));

private static (object, error) run(ptr<analysis.Pass> _addr_pass) {
    object _p0 = default;
    error _p0 = default!;
    ref analysis.Pass pass = ref _addr_pass.val;

    ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();

    ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.AssignStmt)(nil) });
    inspect.Preorder(nodeFilter, node => {
        ptr<ast.AssignStmt> n = node._<ptr<ast.AssignStmt>>();
        if (len(n.Lhs) != len(n.Rhs)) {
            return ;
        }
        if (len(n.Lhs) == 1 && n.Tok == token.DEFINE) {
            return ;
        }
        foreach (var (i, right) in n.Rhs) {
            ptr<ast.CallExpr> (call, ok) = right._<ptr<ast.CallExpr>>();
            if (!ok) {
                continue;
            }
            ptr<ast.SelectorExpr> (sel, ok) = call.Fun._<ptr<ast.SelectorExpr>>();
            if (!ok) {
                continue;
            }
            ptr<ast.Ident> (pkgIdent, _) = sel.X._<ptr<ast.Ident>>();
            ptr<types.PkgName> (pkgName, ok) = pass.TypesInfo.Uses[pkgIdent]._<ptr<types.PkgName>>();
            if (!ok || pkgName.Imported().Path() != "sync/atomic") {
                continue;
            }
            switch (sel.Sel.Name) {
                case "AddInt32": 

                case "AddInt64": 

                case "AddUint32": 

                case "AddUint64": 

                case "AddUintptr": 
                    checkAtomicAddAssignment(_addr_pass, n.Lhs[i], call);
                    break;
            }
        }
    });
    return (null, error.As(null!)!);
}

// checkAtomicAddAssignment walks the atomic.Add* method calls checking
// for assigning the return value to the same variable being used in the
// operation
private static void checkAtomicAddAssignment(ptr<analysis.Pass> _addr_pass, ast.Expr left, ptr<ast.CallExpr> _addr_call) {
    ref analysis.Pass pass = ref _addr_pass.val;
    ref ast.CallExpr call = ref _addr_call.val;

    if (len(call.Args) != 2) {
        return ;
    }
    var arg = call.Args[0];
    var broken = false;

    Func<ast.Expr, @string> gofmt = e => analysisutil.Format(pass.Fset, e);

    {
        ptr<ast.UnaryExpr> (uarg, ok) = arg._<ptr<ast.UnaryExpr>>();

        if (ok && uarg.Op == token.AND) {
            broken = gofmt(left) == gofmt(uarg.X);
        }        {
            ptr<ast.StarExpr> (star, ok) = left._<ptr<ast.StarExpr>>();


            else if (ok) {
                broken = gofmt(star.X) == gofmt(arg);
            }

        }


    }

    if (broken) {
        pass.ReportRangef(left, "direct assignment to atomic value");
    }
}

} // end atomic_package
