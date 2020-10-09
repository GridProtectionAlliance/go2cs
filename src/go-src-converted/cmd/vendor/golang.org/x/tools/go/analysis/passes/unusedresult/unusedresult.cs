// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package unusedresult defines an analyzer that checks for unused
// results of calls to certain pure functions.
// package unusedresult -- go2cs converted at 2020 October 09 06:04:50 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/unusedresult" ==> using unusedresult = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.unusedresult_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\unusedresult\unusedresult.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;
using sort = go.sort_package;
using strings = go.strings_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using analysisutil = go.golang.org.x.tools.go.analysis.passes.@internal.analysisutil_package;
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
    public static partial class unusedresult_package
    {
        // TODO(adonovan): make this analysis modular: export a mustUseResult
        // fact for each function that tail-calls one of the functions that we
        // check, and check those functions too.
        public static readonly @string Doc = (@string)@"check for unused results of calls to some functions

Some functions like fmt.Errorf return a result and have no side effects,
so it is always a mistake to discard the result. This analyzer reports
calls to certain functions in which the result of the call is ignored.

The set of functions may be controlled using flags.";



        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"unusedresult",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,));

        // flags
        private static stringSetFlag funcs = default;        private static stringSetFlag stringMethods = default;



        private static void init()
        { 
            // TODO(adonovan): provide a comment syntax to allow users to
            // add their functions to this set using facts.
            funcs.Set("errors.New,fmt.Errorf,fmt.Sprintf,fmt.Sprint,sort.Reverse");
            Analyzer.Flags.Var(_addr_funcs, "funcs", "comma-separated list of functions whose results must be used");

            stringMethods.Set("Error,String");
            Analyzer.Flags.Var(_addr_stringMethods, "stringmethods", "comma-separated list of names of methods of type func() string whose results must be used");

        }

        private static (object, error) run(ptr<analysis.Pass> _addr_pass)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;

            ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();

            ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.ExprStmt)(nil) });
            inspect.Preorder(nodeFilter, n =>
            {
                ptr<ast.CallExpr> (call, ok) = analysisutil.Unparen(n._<ptr<ast.ExprStmt>>().X)._<ptr<ast.CallExpr>>();
                if (!ok)
                {
                    return ; // not a call statement
                }

                var fun = analysisutil.Unparen(call.Fun);

                if (pass.TypesInfo.Types[fun].IsType())
                {
                    return ; // a conversion, not a call
                }

                ptr<ast.SelectorExpr> (selector, ok) = fun._<ptr<ast.SelectorExpr>>();
                if (!ok)
                {
                    return ; // neither a method call nor a qualified ident
                }

                var (sel, ok) = pass.TypesInfo.Selections[selector];
                if (ok && sel.Kind() == types.MethodVal)
                { 
                    // method (e.g. foo.String())
                    ptr<types.Func> obj = sel.Obj()._<ptr<types.Func>>();
                    ptr<types.Signature> sig = sel.Type()._<ptr<types.Signature>>();
                    if (types.Identical(sig, sigNoArgsStringResult))
                    {
                        if (stringMethods[obj.Name()])
                        {
                            pass.Reportf(call.Lparen, "result of (%s).%s call not used", sig.Recv().Type(), obj.Name());
                        }

                    }

                }
                else if (!ok)
                { 
                    // package-qualified function (e.g. fmt.Errorf)
                    obj = pass.TypesInfo.Uses[selector.Sel];
                    {
                        ptr<types.Func> obj__prev3 = obj;

                        ptr<types.Func> (obj, ok) = obj._<ptr<types.Func>>();

                        if (ok)
                        {
                            var qname = obj.Pkg().Path() + "." + obj.Name();
                            if (funcs[qname])
                            {
                                pass.Reportf(call.Lparen, "result of %v call not used", qname);
                            }

                        }

                        obj = obj__prev3;

                    }

                }

            });
            return (null, error.As(null!)!);

        }

        // func() string
        private static var sigNoArgsStringResult = types.NewSignature(null, null, types.NewTuple(types.NewVar(token.NoPos, null, "", types.Typ[types.String])), false);

        private partial struct stringSetFlag // : map<@string, bool>
        {
        }

        private static @string String(this ptr<stringSetFlag> _addr_ss)
        {
            ref stringSetFlag ss = ref _addr_ss.val;

            slice<@string> items = default;
            foreach (var (item) in ss.val)
            {
                items = append(items, item);
            }
            sort.Strings(items);
            return strings.Join(items, ",");

        }

        private static error Set(this ptr<stringSetFlag> _addr_ss, @string s)
        {
            ref stringSetFlag ss = ref _addr_ss.val;

            var m = make_map<@string, bool>(); // clobber previous value
            if (s != "")
            {
                foreach (var (_, name) in strings.Split(s, ","))
                {
                    if (name == "")
                    {
                        continue; // TODO: report error? proceed?
                    }

                    m[name] = true;

                }

            }

            ss.val = m;
            return error.As(null!)!;

        }
    }
}}}}}}}}}
