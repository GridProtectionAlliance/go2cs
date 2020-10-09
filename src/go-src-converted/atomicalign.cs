// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package atomicalign defines an Analyzer that checks for non-64-bit-aligned
// arguments to sync/atomic functions. On non-32-bit platforms, those functions
// panic if their argument variables are not 64-bit aligned. It is therefore
// the caller's responsibility to arrange for 64-bit alignment of such variables.
// See https://golang.org/pkg/sync/atomic/#pkg-note-BUG
// package atomicalign -- go2cs converted at 2020 October 09 06:03:02 UTC
// import "golang.org/x/tools/go/analysis/passes/atomicalign" ==> using atomicalign = go.golang.org.x.tools.go.analysis.passes.atomicalign_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\atomicalign\atomicalign.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;

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
    public static partial class atomicalign_package
    {
        public static readonly @string Doc = (@string)"check for non-64-bits-aligned arguments to sync/atomic functions";



        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"atomicalign",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,));

        private static (object, error) run(ptr<analysis.Pass> _addr_pass)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;

            if (8L * pass.TypesSizes.Sizeof(types.Typ[types.Uintptr]) == 64L)
            {
                return (null, error.As(null!)!); // 64-bit platform
            }

            if (!analysisutil.Imports(pass.Pkg, "sync/atomic"))
            {
                return (null, error.As(null!)!); // doesn't directly import sync/atomic
            }

            ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();
            ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.CallExpr)(nil) });

            inspect.Preorder(nodeFilter, node =>
            {
                ptr<ast.CallExpr> call = node._<ptr<ast.CallExpr>>();
                ptr<ast.SelectorExpr> (sel, ok) = call.Fun._<ptr<ast.SelectorExpr>>();
                if (!ok)
                {
                    return ;
                }

                ptr<ast.Ident> (pkgIdent, ok) = sel.X._<ptr<ast.Ident>>();
                if (!ok)
                {
                    return ;
                }

                ptr<types.PkgName> (pkgName, ok) = pass.TypesInfo.Uses[pkgIdent]._<ptr<types.PkgName>>();
                if (!ok || pkgName.Imported().Path() != "sync/atomic")
                {
                    return ;
                }

                switch (sel.Sel.Name)
                {
                    case "AddInt64": 

                        // For all the listed functions, the expression to check is always the first function argument.

                    case "AddUint64": 

                        // For all the listed functions, the expression to check is always the first function argument.

                    case "LoadInt64": 

                        // For all the listed functions, the expression to check is always the first function argument.

                    case "LoadUint64": 

                        // For all the listed functions, the expression to check is always the first function argument.

                    case "StoreInt64": 

                        // For all the listed functions, the expression to check is always the first function argument.

                    case "StoreUint64": 

                        // For all the listed functions, the expression to check is always the first function argument.

                    case "SwapInt64": 

                        // For all the listed functions, the expression to check is always the first function argument.

                    case "SwapUint64": 

                        // For all the listed functions, the expression to check is always the first function argument.

                    case "CompareAndSwapInt64": 

                        // For all the listed functions, the expression to check is always the first function argument.

                    case "CompareAndSwapUint64": 

                        // For all the listed functions, the expression to check is always the first function argument.
                        check64BitAlignment(_addr_pass, sel.Sel.Name, call.Args[0L]);
                        break;
                }

            });

            return (null, error.As(null!)!);

        }

        private static void check64BitAlignment(ptr<analysis.Pass> _addr_pass, @string funcName, ast.Expr arg)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
 
            // Checks the argument is made of the address operator (&) applied to
            // to a struct field (as opposed to a variable as the first word of
            // uint64 and int64 variables can be relied upon to be 64-bit aligned.
            ptr<ast.UnaryExpr> (unary, ok) = arg._<ptr<ast.UnaryExpr>>();
            if (!ok || unary.Op != token.AND)
            {
                return ;
            } 

            // Retrieve the types.Struct in order to get the offset of the
            // atomically accessed field.
            ptr<ast.SelectorExpr> (sel, ok) = unary.X._<ptr<ast.SelectorExpr>>();
            if (!ok)
            {
                return ;
            }

            ptr<types.Var> (tvar, ok) = pass.TypesInfo.Selections[sel].Obj()._<ptr<types.Var>>();
            if (!ok || !tvar.IsField())
            {
                return ;
            }

            ptr<types.Struct> (stype, ok) = pass.TypesInfo.Types[sel.X].Type.Underlying()._<ptr<types.Struct>>();
            if (!ok)
            {
                return ;
            }

            long offset = default;
            slice<ptr<types.Var>> fields = default;
            for (long i = 0L; i < stype.NumFields(); i++)
            {
                var f = stype.Field(i);
                fields = append(fields, f);
                if (f == tvar)
                { 
                    // We're done, this is the field we were looking for,
                    // no need to fill the fields slice further.
                    offset = pass.TypesSizes.Offsetsof(fields)[i];
                    break;

                }

            }

            if (offset & 7L == 0L)
            {
                return ; // 64-bit aligned
            }

            pass.ReportRangef(arg, "address of non 64-bit aligned field .%s passed to atomic.%s", tvar.Name(), funcName);

        }
    }
}}}}}}}
