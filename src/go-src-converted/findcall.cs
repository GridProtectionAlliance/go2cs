// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package findcall defines an Analyzer that serves as a trivial
// example and test of the Analysis API. It reports a diagnostic for
// every call to a function or method of the name specified by its
// -name flag. It also exports a fact for each declaration that
// matches the name, plus a package-level fact if the package contained
// one or more such declarations.
// package findcall -- go2cs converted at 2020 October 09 06:04:01 UTC
// import "golang.org/x/tools/go/analysis/passes/findcall" ==> using findcall = go.golang.org.x.tools.go.analysis.passes.findcall_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\findcall\findcall.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using types = go.go.types_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
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
    public static partial class findcall_package
    {
        public static readonly @string Doc = (@string)"find calls to a particular function\n\nThe findcall analysis reports calls to funct" +
    "ions or methods\nof a particular name.";



        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"findcall",Doc:Doc,Run:run,RunDespiteErrors:true,FactTypes:[]analysis.Fact{new(foundFact)},));

        private static @string name = default; // -name flag

        private static void init()
        {
            Analyzer.Flags.StringVar(_addr_name, "name", name, "name of the function to find");
        }

        private static (object, error) run(ptr<analysis.Pass> _addr_pass)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;

            {
                var f__prev1 = f;

                foreach (var (_, __f) in pass.Files)
                {
                    f = __f;
                    ast.Inspect(f, n =>
                    {
                        {
                            ptr<ast.CallExpr> (call, ok) = n._<ptr<ast.CallExpr>>();

                            if (ok)
                            {
                                ptr<ast.Ident> id;
                                switch (call.Fun.type())
                                {
                                    case ptr<ast.Ident> fun:
                                        id = fun;
                                        break;
                                    case ptr<ast.SelectorExpr> fun:
                                        id = fun.Sel;
                                        break;
                                }
                                if (id != null && !pass.TypesInfo.Types[id].IsType() && id.Name == name)
                                {
                                    pass.Report(new analysis.Diagnostic(Pos:call.Lparen,Message:fmt.Sprintf("call of %s(...)",id.Name),SuggestedFixes:[]analysis.SuggestedFix{{Message:fmt.Sprintf("Add '_TEST_'"),TextEdits:[]analysis.TextEdit{{Pos:call.Lparen,End:call.Lparen,NewText:[]byte("_TEST_"),}},}},));
                                }

                            }

                        }

                        return true;

                    });

                } 

                // Export a fact for each matching function.
                //
                // These facts are produced only to test the testing
                // infrastructure in the analysistest package.
                // They are not consumed by the findcall Analyzer
                // itself, as would happen in a more realistic example.

                f = f__prev1;
            }

            {
                var f__prev1 = f;

                foreach (var (_, __f) in pass.Files)
                {
                    f = __f;
                    {
                        var decl__prev2 = decl;

                        foreach (var (_, __decl) in f.Decls)
                        {
                            decl = __decl;
                            {
                                var decl__prev1 = decl;

                                ptr<ast.FuncDecl> (decl, ok) = decl._<ptr<ast.FuncDecl>>();

                                if (ok && decl.Name.Name == name)
                                {
                                    {
                                        ptr<types.Func> (obj, ok) = pass.TypesInfo.Defs[decl.Name]._<ptr<types.Func>>();

                                        if (ok)
                                        {
                                            pass.ExportObjectFact(obj, @new<foundFact>());
                                        }

                                    }

                                }

                                decl = decl__prev1;

                            }

                        }

                        decl = decl__prev2;
                    }
                }

                f = f__prev1;
            }

            if (len(pass.AllObjectFacts()) > 0L)
            {
                pass.ExportPackageFact(@new<foundFact>());
            }

            return (null, error.As(null!)!);

        }

        // foundFact is a fact associated with functions that match -name.
        // We use it to exercise the fact machinery in tests.
        private partial struct foundFact
        {
        }

        private static @string String(this ptr<foundFact> _addr__p0)
        {
            ref foundFact _p0 = ref _addr__p0.val;

            return "found";
        }
        private static void AFact(this ptr<foundFact> _addr__p0)
        {
            ref foundFact _p0 = ref _addr__p0.val;

        }
    }
}}}}}}}
