// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package nilfunc defines an Analyzer that checks for useless
// comparisons against nil.
// package nilfunc -- go2cs converted at 2020 October 08 04:58:03 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/nilfunc" ==> using nilfunc = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.nilfunc_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\nilfunc\nilfunc.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes
{
    public static partial class nilfunc_package
    {
        public static readonly @string Doc = (@string)"check for useless comparisons between functions and nil\n\nA useless comparison is " +
    "one like f == nil as opposed to f() == nil.";



        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"nilfunc",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,));

        private static (object, error) run(ptr<analysis.Pass> _addr_pass)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;

            ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();

            ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.BinaryExpr)(nil) });
            inspect.Preorder(nodeFilter, n =>
            {
                ptr<ast.BinaryExpr> e = n._<ptr<ast.BinaryExpr>>(); 

                // Only want == or != comparisons.
                if (e.Op != token.EQL && e.Op != token.NEQ)
                {
                    return ;
                } 

                // Only want comparisons with a nil identifier on one side.
                ast.Expr e2 = default;

                if (pass.TypesInfo.Types[e.X].IsNil()) 
                    e2 = e.Y;
                else if (pass.TypesInfo.Types[e.Y].IsNil()) 
                    e2 = e.X;
                else 
                    return ;
                // Only want identifiers or selector expressions.
                types.Object obj = default;
                switch (e2.type())
                {
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

                    if (!ok)
                    {
                        return ;
                    }

                }


                pass.ReportRangef(e, "comparison of function %v %v nil is always %v", obj.Name(), e.Op, e.Op == token.NEQ);

            });
            return (null, error.As(null!)!);

        }
    }
}}}}}}}}}
