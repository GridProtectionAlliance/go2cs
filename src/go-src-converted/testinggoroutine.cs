// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package testinggoroutine -- go2cs converted at 2020 October 09 06:04:17 UTC
// import "golang.org/x/tools/go/analysis/passes/testinggoroutine" ==> using testinggoroutine = go.golang.org.x.tools.go.analysis.passes.testinggoroutine_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\testinggoroutine\testinggoroutine.go
using ast = go.go.ast_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using analysisutil = go.golang.org.x.tools.go.analysis.passes.@internal.analysisutil_package;
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
    public static partial class testinggoroutine_package
    {
        public static readonly @string Doc = (@string)@"report calls to (*testing.T).Fatal from goroutines started by a test.

Functions that abruptly terminate a test, such as the Fatal, Fatalf, FailNow, and
Skip{,f,Now} methods of *testing.T, must be called from the test goroutine itself.
This checker detects calls to these functions that occur within a goroutine
started by the test. For example:

func TestFoo(t *testing.T) {
    go func() {
        t.Fatal(""oops"") // error: (*T).Fatal called from non-test goroutine
    }()
}
";



        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"testinggoroutine",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,));

        private static map forbidden = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"FailNow":true,"Fatal":true,"Fatalf":true,"Skip":true,"Skipf":true,"SkipNow":true,};

        private static (object, error) run(ptr<analysis.Pass> _addr_pass)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;

            ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();

            if (!analysisutil.Imports(pass.Pkg, "testing"))
            {
                return (null, error.As(null!)!);
            } 

            // Filter out anything that isn't a function declaration.
            ast.Node onlyFuncs = new slice<ast.Node>(new ast.Node[] { (*ast.FuncDecl)(nil) });

            inspect.Nodes(onlyFuncs, (node, push) =>
            {
                ptr<ast.FuncDecl> (fnDecl, ok) = node._<ptr<ast.FuncDecl>>();
                if (!ok)
                {
                    return false;
                }

                if (!hasBenchmarkOrTestParams(fnDecl))
                {
                    return false;
                } 

                // Now traverse the benchmark/test's body and check that none of the
                // forbidden methods are invoked in the goroutines within the body.
                ast.Inspect(fnDecl, n =>
                {
                    ptr<ast.GoStmt> (goStmt, ok) = n._<ptr<ast.GoStmt>>();
                    if (!ok)
                    {
                        return true;
                    }

                    checkGoStmt(_addr_pass, goStmt); 

                    // No need to further traverse the GoStmt since right
                    // above we manually traversed it in the ast.Inspect(goStmt, ...)
                    return false;

                });

                return false;

            });

            return (null, error.As(null!)!);

        }

        private static bool hasBenchmarkOrTestParams(ptr<ast.FuncDecl> _addr_fnDecl)
        {
            ref ast.FuncDecl fnDecl = ref _addr_fnDecl.val;
 
            // Check that the function's arguments include "*testing.T" or "*testing.B".
            var @params = fnDecl.Type.Params.List;

            foreach (var (_, param) in params)
            {
                {
                    var (_, ok) = typeIsTestingDotTOrB(param.Type);

                    if (ok)
                    {
                        return true;
                    }

                }

            }
            return false;

        }

        private static (@string, bool) typeIsTestingDotTOrB(ast.Expr expr)
        {
            @string _p0 = default;
            bool _p0 = default;

            ptr<ast.StarExpr> (starExpr, ok) = expr._<ptr<ast.StarExpr>>();
            if (!ok)
            {
                return ("", false);
            }

            ptr<ast.SelectorExpr> (selExpr, ok) = starExpr.X._<ptr<ast.SelectorExpr>>();
            if (!ok)
            {
                return ("", false);
            }

            ptr<ast.Ident> varPkg = selExpr.X._<ptr<ast.Ident>>();
            if (varPkg.Name != "testing")
            {
                return ("", false);
            }

            var varTypeName = selExpr.Sel.Name;
            ok = varTypeName == "B" || varTypeName == "T";
            return (varTypeName, ok);

        }

        // checkGoStmt traverses the goroutine and checks for the
        // use of the forbidden *testing.(B, T) methods.
        private static void checkGoStmt(ptr<analysis.Pass> _addr_pass, ptr<ast.GoStmt> _addr_goStmt)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ast.GoStmt goStmt = ref _addr_goStmt.val;
 
            // Otherwise examine the goroutine to check for the forbidden methods.
            ast.Inspect(goStmt, n =>
            {
                ptr<ast.SelectorExpr> (selExpr, ok) = n._<ptr<ast.SelectorExpr>>();
                if (!ok)
                {
                    return true;
                }

                var (_, bad) = forbidden[selExpr.Sel.Name];
                if (!bad)
                {
                    return true;
                } 

                // Now filter out false positives by the import-path/type.
                ptr<ast.Ident> (ident, ok) = selExpr.X._<ptr<ast.Ident>>();
                if (!ok)
                {
                    return true;
                }

                if (ident.Obj == null || ident.Obj.Decl == null)
                {
                    return true;
                }

                ptr<ast.Field> (field, ok) = ident.Obj.Decl._<ptr<ast.Field>>();
                if (!ok)
                {
                    return true;
                }

                {
                    var (typeName, ok) = typeIsTestingDotTOrB(field.Type);

                    if (ok)
                    {
                        pass.ReportRangef(selExpr, "call to (*%s).%s from a non-test goroutine", typeName, selExpr.Sel);
                    }

                }

                return true;

            });

        }
    }
}}}}}}}
