// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package assign defines an Analyzer that detects useless assignments.
// package assign -- go2cs converted at 2022 March 06 23:34:28 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/assign" ==> using assign = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.assign_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\assign\assign.go
// TODO(adonovan): check also for assignments to struct fields inside
// methods that are on T instead of *T.

using fmt = go.fmt_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using reflect = go.reflect_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using analysisutil = go.golang.org.x.tools.go.analysis.passes.@internal.analysisutil_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using System;


namespace go.cmd.vendor.golang.org.x.tools.go.analysis.passes;

public static partial class assign_package {

public static readonly @string Doc = "check for useless assignments\n\nThis checker reports assignments of the form x = x" +
    " or a[i] = a[i].\nThese are almost always useless, and even when they aren\'t they" +
    " are\nusually a mistake.";



public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"assign",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,));

private static (object, error) run(ptr<analysis.Pass> _addr_pass) {
    object _p0 = default;
    error _p0 = default!;
    ref analysis.Pass pass = ref _addr_pass.val;

    ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();

    ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.AssignStmt)(nil) });
    inspect.Preorder(nodeFilter, n => {
        ptr<ast.AssignStmt> stmt = n._<ptr<ast.AssignStmt>>();
        if (stmt.Tok != token.ASSIGN) {
            return ; // ignore :=
        }
        if (len(stmt.Lhs) != len(stmt.Rhs)) { 
            // If LHS and RHS have different cardinality, they can't be the same.
            return ;

        }
        foreach (var (i, lhs) in stmt.Lhs) {
            var rhs = stmt.Rhs[i];
            if (analysisutil.HasSideEffects(pass.TypesInfo, lhs) || analysisutil.HasSideEffects(pass.TypesInfo, rhs)) {
                continue; // expressions may not be equal
            }

            if (reflect.TypeOf(lhs) != reflect.TypeOf(rhs)) {
                continue; // short-circuit the heavy-weight gofmt check
            }

            var le = analysisutil.Format(pass.Fset, lhs);
            var re = analysisutil.Format(pass.Fset, rhs);
            if (le == re) {
                pass.Report(new analysis.Diagnostic(Pos:stmt.Pos(),Message:fmt.Sprintf("self-assignment of %s to %s",re,le),SuggestedFixes:[]analysis.SuggestedFix{{Message:"Remove",TextEdits:[]analysis.TextEdit{{Pos:stmt.Pos(),End:stmt.End(),NewText:[]byte{}},}},},));
            }

        }
    });

    return (null, error.As(null!)!);

}

} // end assign_package
