// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package deepequalerrors defines an Analyzer that checks for the use
// of reflect.DeepEqual with error values.
// package deepequalerrors -- go2cs converted at 2020 October 09 06:04:00 UTC
// import "golang.org/x/tools/go/analysis/passes/deepequalerrors" ==> using deepequalerrors = go.golang.org.x.tools.go.analysis.passes.deepequalerrors_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\deepequalerrors\deepequalerrors.go
using ast = go.go.ast_package;
using types = go.go.types_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using typeutil = go.golang.org.x.tools.go.types.typeutil_package;
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
    public static partial class deepequalerrors_package
    {
        public static readonly @string Doc = (@string)"check for calls of reflect.DeepEqual on error values\n\nThe deepequalerrors checker" +
    " looks for calls of the form:\n\n    reflect.DeepEqual(err1, err2)\n\nwhere err1 and" +
    " err2 are errors. Using reflect.DeepEqual to compare\nerrors is discouraged.";



        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"deepequalerrors",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,));

        private static (object, error) run(ptr<analysis.Pass> _addr_pass)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;

            ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();

            ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.CallExpr)(nil) });
            inspect.Preorder(nodeFilter, n =>
            {
                ptr<ast.CallExpr> call = n._<ptr<ast.CallExpr>>();
                ptr<types.Func> (fn, ok) = typeutil.Callee(pass.TypesInfo, call)._<ptr<types.Func>>();
                if (!ok)
                {
                    return ;
                }

                if (fn.FullName() == "reflect.DeepEqual" && hasError(_addr_pass, call.Args[0L]) && hasError(_addr_pass, call.Args[1L]))
                {
                    pass.ReportRangef(call, "avoid using reflect.DeepEqual with errors");
                }

            });
            return (null, error.As(null!)!);

        }

        private static ptr<types.Interface> errorType = types.Universe.Lookup("error").Type().Underlying()._<ptr<types.Interface>>();

        // hasError reports whether the type of e contains the type error.
        // See containsError, below, for the meaning of "contains".
        private static bool hasError(ptr<analysis.Pass> _addr_pass, ast.Expr e)
        {
            ref analysis.Pass pass = ref _addr_pass.val;

            var (tv, ok) = pass.TypesInfo.Types[e];
            if (!ok)
            { // no type info, assume good
                return false;

            }

            return containsError(tv.Type);

        }

        // Report whether any type that typ could store and that could be compared is the
        // error type. This includes typ itself, as well as the types of struct field, slice
        // and array elements, map keys and elements, and pointers. It does not include
        // channel types (incomparable), arg and result types of a Signature (not stored), or
        // methods of a named or interface type (not stored).
        private static bool containsError(types.Type typ)
        { 
            // Track types being processed, to avoid infinite recursion.
            // Using types as keys here is OK because we are checking for the identical pointer, not
            // type identity. See analysis/passes/printf/types.go.
            var inProgress = make_map<types.Type, bool>();

            Func<types.Type, bool> check = default;
            check = t =>
            {
                if (t == errorType)
                {
                    return true;
                }

                if (inProgress[t])
                {
                    return false;
                }

                inProgress[t] = true;
                switch (t.type())
                {
                    case ptr<types.Pointer> t:
                        return check(t.Elem());
                        break;
                    case ptr<types.Slice> t:
                        return check(t.Elem());
                        break;
                    case ptr<types.Array> t:
                        return check(t.Elem());
                        break;
                    case ptr<types.Map> t:
                        return check(t.Key()) || check(t.Elem());
                        break;
                    case ptr<types.Struct> t:
                        for (long i = 0L; i < t.NumFields(); i++)
                        {
                            if (check(t.Field(i).Type()))
                            {
                                return true;
                            }

                        }
                        break;
                    case ptr<types.Named> t:
                        return check(t.Underlying()); 

                        // We list the remaining valid type kinds for completeness.
                        break;
                    case ptr<types.Basic> t:
                        break;
                    case ptr<types.Chan> t:
                        break;
                    case ptr<types.Signature> t:
                        break;
                    case ptr<types.Tuple> t:
                        break;
                    case ptr<types.Interface> t:
                        break;
                }
                return false;

            }
;

            return check(typ);

        }
    }
}}}}}}}
