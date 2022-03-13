// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package loopclosure defines an Analyzer that checks for references to
// enclosing loop variables from within nested functions.

// package loopclosure -- go2cs converted at 2022 March 13 06:41:55 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/loopclosure" ==> using loopclosure = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.loopclosure_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\loopclosure\loopclosure.go
namespace go.cmd.vendor.golang.org.x.tools.go.analysis.passes;

using ast = go.ast_package;
using types = go.types_package;

using analysis = golang.org.x.tools.go.analysis_package;
using inspect = golang.org.x.tools.go.analysis.passes.inspect_package;
using inspector = golang.org.x.tools.go.ast.inspector_package;
using typeutil = golang.org.x.tools.go.types.typeutil_package;
using System;

public static partial class loopclosure_package {

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
        ast.Expr fun = default;
        switch (body.List[len(body.List) - 1].type()) {
            case ptr<ast.GoStmt> s:
                fun = s.Call.Fun;
                break;
            case ptr<ast.DeferStmt> s:
                fun = s.Call.Fun;
                break;
            case ptr<ast.ExprStmt> s:
                {
                    ptr<ast.CallExpr> (call, ok) = s.X._<ptr<ast.CallExpr>>();

                    if (ok) {
                        fun = goInvokes(_addr_pass.TypesInfo, call);
                    }

                }
                break;
        }
        ptr<ast.FuncLit> (lit, ok) = fun._<ptr<ast.FuncLit>>();
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

// goInvokes returns a function expression that would be called asynchronously
// (but not awaited) in another goroutine as a consequence of the call.
// For example, given the g.Go call below, it returns the function literal expression.
//
//   import "sync/errgroup"
//   var g errgroup.Group
//   g.Go(func() error { ... })
//
// Currently only "golang.org/x/sync/errgroup.Group()" is considered.
private static ast.Expr goInvokes(ptr<types.Info> _addr_info, ptr<ast.CallExpr> _addr_call) {
    ref types.Info info = ref _addr_info.val;
    ref ast.CallExpr call = ref _addr_call.val;

    var f = typeutil.StaticCallee(info, call); 
    // Note: Currently only supports: golang.org/x/sync/errgroup.Go.
    if (f == null || f.Name() != "Go") {
        return null;
    }
    ptr<types.Signature> recv = f.Type()._<ptr<types.Signature>>().Recv();
    if (recv == null) {
        return null;
    }
    ptr<types.Pointer> (rtype, ok) = recv.Type()._<ptr<types.Pointer>>();
    if (!ok) {
        return null;
    }
    ptr<types.Named> (named, ok) = rtype.Elem()._<ptr<types.Named>>();
    if (!ok) {
        return null;
    }
    if (named.Obj().Name() != "Group") {
        return null;
    }
    var pkg = f.Pkg();
    if (pkg == null) {
        return null;
    }
    if (pkg.Path() != "golang.org/x/sync/errgroup") {
        return null;
    }
    return call.Args[0];
}

} // end loopclosure_package
