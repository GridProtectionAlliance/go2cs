// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package httpresponse defines an Analyzer that checks for mistakes
// using HTTP responses.
// package httpresponse -- go2cs converted at 2020 October 09 06:04:34 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/httpresponse" ==> using httpresponse = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.httpresponse_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\httpresponse\httpresponse.go
using ast = go.go.ast_package;
using types = go.go.types_package;

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
    public static partial class httpresponse_package
    {
        public static readonly @string Doc = (@string)@"check for mistakes using HTTP responses

A common mistake when using the net/http package is to defer a function
call to close the http.Response Body before checking the error that
determines whether the response is valid:

	resp, err := http.Head(url)
	defer resp.Body.Close()
	if err != nil {
		log.Fatal(err)
	}
	// (defer statement belongs here)

This checker helps uncover latent nil dereference bugs by reporting a
diagnostic for such mistakes.";



        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"httpresponse",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,));

        private static (object, error) run(ptr<analysis.Pass> _addr_pass)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;

            ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>(); 

            // Fast path: if the package doesn't import net/http,
            // skip the traversal.
            if (!analysisutil.Imports(pass.Pkg, "net/http"))
            {
                return (null, error.As(null!)!);
            }

            ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.CallExpr)(nil) });
            inspect.WithStack(nodeFilter, (n, push, stack) =>
            {
                if (!push)
                {
                    return true;
                }

                ptr<ast.CallExpr> call = n._<ptr<ast.CallExpr>>();
                if (!isHTTPFuncOrMethodOnClient(_addr_pass.TypesInfo, call))
                {
                    return true; // the function call is not related to this check.
                } 

                // Find the innermost containing block, and get the list
                // of statements starting with the one containing call.
                var stmts = restOfBlock(stack);
                if (len(stmts) < 2L)
                {
                    return true; // the call to the http function is the last statement of the block.
                }

                ptr<ast.AssignStmt> (asg, ok) = stmts[0L]._<ptr<ast.AssignStmt>>();
                if (!ok)
                {
                    return true; // the first statement is not assignment.
                }

                var resp = rootIdent(asg.Lhs[0L]);
                if (resp == null)
                {
                    return true; // could not find the http.Response in the assignment.
                }

                ptr<ast.DeferStmt> (def, ok) = stmts[1L]._<ptr<ast.DeferStmt>>();
                if (!ok)
                {
                    return true; // the following statement is not a defer.
                }

                var root = rootIdent(def.Call.Fun);
                if (root == null)
                {
                    return true; // could not find the receiver of the defer call.
                }

                if (resp.Obj == root.Obj)
                {
                    pass.ReportRangef(root, "using %s before checking for errors", resp.Name);
                }

                return true;

            });
            return (null, error.As(null!)!);

        }

        // isHTTPFuncOrMethodOnClient checks whether the given call expression is on
        // either a function of the net/http package or a method of http.Client that
        // returns (*http.Response, error).
        private static bool isHTTPFuncOrMethodOnClient(ptr<types.Info> _addr_info, ptr<ast.CallExpr> _addr_expr)
        {
            ref types.Info info = ref _addr_info.val;
            ref ast.CallExpr expr = ref _addr_expr.val;

            ptr<ast.SelectorExpr> (fun, _) = expr.Fun._<ptr<ast.SelectorExpr>>();
            ptr<types.Signature> (sig, _) = info.Types[fun].Type._<ptr<types.Signature>>();
            if (sig == null)
            {
                return false; // the call is not of the form x.f()
            }

            var res = sig.Results();
            if (res.Len() != 2L)
            {
                return false; // the function called does not return two values.
            }

            {
                ptr<types.Pointer> ptr__prev1 = ptr;

                ptr<types.Pointer> (ptr, ok) = res.At(0L).Type()._<ptr<types.Pointer>>();

                if (!ok || !isNamedType(ptr.Elem(), "net/http", "Response"))
                {
                    return false; // the first return type is not *http.Response.
                }

                ptr = ptr__prev1;

            }


            var errorType = types.Universe.Lookup("error").Type();
            if (!types.Identical(res.At(1L).Type(), errorType))
            {
                return false; // the second return type is not error
            }

            var typ = info.Types[fun.X].Type;
            if (typ == null)
            {
                ptr<ast.Ident> (id, ok) = fun.X._<ptr<ast.Ident>>();
                return ok && id.Name == "http"; // function in net/http package.
            }

            if (isNamedType(typ, "net/http", "Client"))
            {
                return true; // method on http.Client.
            }

            (ptr, ok) = typ._<ptr<types.Pointer>>();
            return ok && isNamedType(ptr.Elem(), "net/http", "Client"); // method on *http.Client.
        }

        // restOfBlock, given a traversal stack, finds the innermost containing
        // block and returns the suffix of its statements starting with the
        // current node (the last element of stack).
        private static slice<ast.Stmt> restOfBlock(slice<ast.Node> stack)
        {
            for (var i = len(stack) - 1L; i >= 0L; i--)
            {
                {
                    ptr<ast.BlockStmt> (b, ok) = stack[i]._<ptr<ast.BlockStmt>>();

                    if (ok)
                    {
                        foreach (var (j, v) in b.List)
                        {
                            if (v == stack[i + 1L])
                            {
                                return b.List[j..];
                            }

                        }
                        break;

                    }

                }

            }

            return null;

        }

        // rootIdent finds the root identifier x in a chain of selections x.y.z, or nil if not found.
        private static ptr<ast.Ident> rootIdent(ast.Node n)
        {
            switch (n.type())
            {
                case ptr<ast.SelectorExpr> n:
                    return _addr_rootIdent(n.X)!;
                    break;
                case ptr<ast.Ident> n:
                    return _addr_n!;
                    break;
                default:
                {
                    var n = n.type();
                    return _addr_null!;
                    break;
                }
            }

        }

        // isNamedType reports whether t is the named type path.name.
        private static bool isNamedType(types.Type t, @string path, @string name)
        {
            ptr<types.Named> (n, ok) = t._<ptr<types.Named>>();
            if (!ok)
            {
                return false;
            }

            var obj = n.Obj();
            return obj.Name() == name && obj.Pkg() != null && obj.Pkg().Path() == path;

        }
    }
}}}}}}}}}
