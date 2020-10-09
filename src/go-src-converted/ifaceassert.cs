// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ifaceassert defines an Analyzer that flags
// impossible interface-interface type assertions.
// package ifaceassert -- go2cs converted at 2020 October 09 06:04:02 UTC
// import "golang.org/x/tools/go/analysis/passes/ifaceassert" ==> using ifaceassert = go.golang.org.x.tools.go.analysis.passes.ifaceassert_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\ifaceassert\ifaceassert.go
using ast = go.go.ast_package;
using types = go.go.types_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes
{
    public static partial class ifaceassert_package
    {
        public static readonly @string Doc = (@string)@"detect impossible interface-to-interface type assertions

This checker flags type assertions v.(T) and corresponding type-switch cases
in which the static type V of v is an interface that cannot possibly implement
the target interface T. This occurs when V and T contain methods with the same
name but different signatures. Example:

	var v interface {
		Read()
	}
	_ = v.(io.Reader)

The Read method in v has a different signature than the Read method in
io.Reader, so this assertion cannot succeed.
";



        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"ifaceassert",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,));

        // assertableTo checks whether interface v can be asserted into t. It returns
        // nil on success, or the first conflicting method on failure.
        private static ptr<types.Func> assertableTo(types.Type v, types.Type t)
        { 
            // ensure that v and t are interfaces
            ptr<types.Interface> (V, _) = v.Underlying()._<ptr<types.Interface>>();
            ptr<types.Interface> (T, _) = t.Underlying()._<ptr<types.Interface>>();
            if (V == null || T == null)
            {
                return _addr_null!;
            }

            {
                var (f, wrongType) = types.MissingMethod(V, T, false);

                if (wrongType)
                {
                    return _addr_f!;
                }

            }

            return _addr_null!;

        }

        private static (object, error) run(ptr<analysis.Pass> _addr_pass)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;

            ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();
            ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.TypeAssertExpr)(nil), (*ast.TypeSwitchStmt)(nil) });
            inspect.Preorder(nodeFilter, n =>
            {
                ptr<ast.TypeAssertExpr> assert;                slice<ast.Expr> targets = default;
                switch (n.type())
                {
                    case ptr<ast.TypeAssertExpr> n:
                        if (n.Type == null)
                        {
                            return ;
                        }

                        assert = n;
                        targets = append(targets, n.Type);
                        break;
                    case ptr<ast.TypeSwitchStmt> n:
                        switch (n.Assign.type())
                        {
                            case ptr<ast.ExprStmt> t:
                                assert = t.X._<ptr<ast.TypeAssertExpr>>();
                                break;
                            case ptr<ast.AssignStmt> t:
                                assert = t.Rhs[0L]._<ptr<ast.TypeAssertExpr>>();
                                break; 
                            // gather target types from case clauses
                        } 
                        // gather target types from case clauses
                        foreach (var (_, c) in n.Body.List)
                        {
                            targets = append(targets, c._<ptr<ast.CaseClause>>().List);
                        }
                        break;
                }
                var V = pass.TypesInfo.TypeOf(assert.X);
                foreach (var (_, target) in targets)
                {
                    var T = pass.TypesInfo.TypeOf(target);
                    {
                        var f = assertableTo(V, T);

                        if (f != null)
                        {
                            pass.Reportf(target.Pos(), "impossible type assertion: no type can implement both %v and %v (conflicting types for %v method)", V, T, f.Name());
                        }

                    }

                }

            });
            return (null, error.As(null!)!);

        }
    }
}}}}}}}
