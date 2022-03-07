// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package loopclosure defines an Analyzer that checks for references to
// enclosing loop variables from within nested functions.
// package loopclosure -- go2cs converted at 2022 March 06 23:34:01 UTC
// import "golang.org/x/tools/go/analysis/passes/loopclosure" ==> using loopclosure = go.golang.org.x.tools.go.analysis.passes.loopclosure_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\loopclosure\loopclosure.go
using ast = go.go.ast_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using System;


namespace go.golang.org.x.tools.go.analysis.passes;

public static partial class loopclosure_package {

    // TODO(adonovan): also report an error for the following structure,
    // which is often used to ensure that deferred calls do not accumulate
    // in a loop:
    //
    //    for i, x := range c {
    //        func() {
    //            ...reference to i or x...
    //        }()
    //    }
public static readonly @string Doc = @"check references to loop variables from within nested functions

This analyzer checks for references to loop variables from within a
function literal inside the loop body. It checks only instances where
the function literal is called in a defer or go statement that is the
last statement in the loop body, as otherwise we would need whole
program analysis.

For example:

	for i, v := range s {
		go func() {
			println(i, v) // not what you might expect
		}()
	}

See: https://golang.org/doc/go_faq.html#closures_and_goroutines";



public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"loopclosure",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,));

private static (object, error) run(ptr<analysis.Pass> _addr_pass) {
    object _p0 = default;
    error _p0 = default!;
    ref analysis.Pass pass = ref _addr_pass.val;

    ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();

    ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.RangeStmt)(nil), (*ast.ForStmt)(nil) });
    inspect.Preorder(nodeFilter, n => { 
        // Find the variables updated by the loop statement.
        slice<ptr<ast.Ident>> vars = default;
        Action<ast.Expr> addVar = expr => {
            {
                ptr<ast.Ident> id__prev1 = id;

                ptr<ast.Ident> (id, ok) = expr._<ptr<ast.Ident>>();

                if (ok) {
                    vars = append(vars, id);
                }

                id = id__prev1;

            }

        };
        ptr<ast.BlockStmt> body;
        switch (n.type()) {
            case ptr<ast.RangeStmt> n:
                body = n.Body;
                addVar(n.Key);
                addVar(n.Value);
                break;
            case ptr<ast.ForStmt> n:
                body = n.Body;
                switch (n.Post.type()) {
                    case ptr<ast.AssignStmt> post:
                        foreach (var (_, lhs) in post.Lhs) {
                            addVar(lhs);
                        }
                        break;
                    case ptr<ast.IncDecStmt> post:
                        addVar(post.X);
                        break;
                }
                break;
        }
        if (vars == null) {
            return ;
        }
        if (len(body.List) == 0) {
            return ;
        }
        ptr<ast.CallExpr> last;
        switch (body.List[len(body.List) - 1].type()) {
            case ptr<ast.GoStmt> s:
                last = s.Call;
                break;
            case ptr<ast.DeferStmt> s:
                last = s.Call;
                break;
            default:
            {
                var s = body.List[len(body.List) - 1].type();
                return ;
                break;
            }
        }
        ptr<ast.FuncLit> (lit, ok) = last.Fun._<ptr<ast.FuncLit>>();
        if (!ok) {
            return ;
        }
        ast.Inspect(lit.Body, n => {
            (id, ok) = n._<ptr<ast.Ident>>();
            if (!ok || id.Obj == null) {
                return true;
            }
            if (pass.TypesInfo.Types[id].Type == null) { 
                // Not referring to a variable (e.g. struct field name)
                return true;

            }

            foreach (var (_, v) in vars) {
                if (v.Obj == id.Obj) {
                    pass.ReportRangef(id, "loop variable %s captured by func literal", id.Name);
                }
            }
            return true;

        });

    });
    return (null, error.As(null!)!);

}

} // end loopclosure_package
