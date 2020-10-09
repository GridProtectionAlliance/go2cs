// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package lostcancel defines an Analyzer that checks for failure to
// call a context cancellation function.
// package lostcancel -- go2cs converted at 2020 October 09 06:04:39 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/lostcancel" ==> using lostcancel = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.lostcancel_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\lostcancel\lostcancel.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using types = go.go.types_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using ctrlflow = go.golang.org.x.tools.go.analysis.passes.ctrlflow_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using cfg = go.golang.org.x.tools.go.cfg_package;
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
    public static partial class lostcancel_package
    {
        public static readonly @string Doc = (@string)@"check cancel func returned by context.WithCancel is called

The cancellation function returned by context.WithCancel, WithTimeout,
and WithDeadline must be called or the new context will remain live
until its parent context is cancelled.
(The background context is never cancelled.)";



        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"lostcancel",Doc:Doc,Run:run,Requires:[]*analysis.Analyzer{inspect.Analyzer,ctrlflow.Analyzer,},));

        private static readonly var debug = false;



        private static @string contextPackage = "context";

        // checkLostCancel reports a failure to the call the cancel function
        // returned by context.WithCancel, either because the variable was
        // assigned to the blank identifier, or because there exists a
        // control-flow path from the call to a return statement and that path
        // does not "use" the cancel function.  Any reference to the variable
        // counts as a use, even within a nested function literal.
        // If the variable's scope is larger than the function
        // containing the assignment, we assume that other uses exist.
        //
        // checkLostCancel analyzes a single named or literal function.
        private static (object, error) run(ptr<analysis.Pass> _addr_pass)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;
 
            // Fast path: bypass check if file doesn't use context.WithCancel.
            if (!hasImport(_addr_pass.Pkg, contextPackage))
            {
                return (null, error.As(null!)!);
            } 

            // Call runFunc for each Func{Decl,Lit}.
            ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();
            ast.Node nodeTypes = new slice<ast.Node>(new ast.Node[] { (*ast.FuncLit)(nil), (*ast.FuncDecl)(nil) });
            inspect.Preorder(nodeTypes, n =>
            {
                runFunc(_addr_pass, n);
            });
            return (null, error.As(null!)!);

        }

        private static void runFunc(ptr<analysis.Pass> _addr_pass, ast.Node node)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
 
            // Find scope of function node
            ptr<types.Scope> funcScope;
            switch (node.type())
            {
                case ptr<ast.FuncLit> v:
                    funcScope = pass.TypesInfo.Scopes[v.Type];
                    break;
                case ptr<ast.FuncDecl> v:
                    funcScope = pass.TypesInfo.Scopes[v.Type];
                    break; 

                // Maps each cancel variable to its defining ValueSpec/AssignStmt.
            } 

            // Maps each cancel variable to its defining ValueSpec/AssignStmt.
            var cancelvars = make_map<ptr<types.Var>, ast.Node>(); 

            // TODO(adonovan): opt: refactor to make a single pass
            // over the AST using inspect.WithStack and node types
            // {FuncDecl,FuncLit,CallExpr,SelectorExpr}.

            // Find the set of cancel vars to analyze.
            var stack = make_slice<ast.Node>(0L, 32L);
            ast.Inspect(node, n =>
            {
                switch (n.type())
                {
                    case ptr<ast.FuncLit> _:
                        if (len(stack) > 0L)
                        {
                            return false; // don't stray into nested functions
                        }

                        break;
                    case 
                        stack = stack[..len(stack) - 1L]; // pop
                        return true;
                        break;
                }
                stack = append(stack, n); // push

                // Look for [{AssignStmt,ValueSpec} CallExpr SelectorExpr]:
                //
                //   ctx, cancel    := context.WithCancel(...)
                //   ctx, cancel     = context.WithCancel(...)
                //   var ctx, cancel = context.WithCancel(...)
                //
                if (!isContextWithCancel(_addr_pass.TypesInfo, n) || !isCall(stack[len(stack) - 2L]))
                {
                    return true;
                }

                ptr<ast.Ident> id; // id of cancel var
                var stmt = stack[len(stack) - 3L];
                switch (stmt.type())
                {
                    case ptr<ast.ValueSpec> stmt:
                        if (len(stmt.Names) > 1L)
                        {
                            id = stmt.Names[1L];
                        }

                        break;
                    case ptr<ast.AssignStmt> stmt:
                        if (len(stmt.Lhs) > 1L)
                        {
                            id, _ = stmt.Lhs[1L]._<ptr<ast.Ident>>();
                        }

                        break;
                }
                if (id != null)
                {
                    if (id.Name == "_")
                    {
                        pass.ReportRangef(id, "the cancel function returned by context.%s should be called, not discarded, to avoid a context leak", n._<ptr<ast.SelectorExpr>>().Sel.Name);
                    }                    {
                        var v__prev3 = v;

                        ptr<types.Var> (v, ok) = pass.TypesInfo.Uses[id]._<ptr<types.Var>>();


                        else if (ok)
                        { 
                            // If the cancel variable is defined outside function scope,
                            // do not analyze it.
                            if (funcScope.Contains(v.Pos()))
                            {
                                cancelvars[v] = stmt;
                            }

                        }                        {
                            var v__prev4 = v;

                            (v, ok) = pass.TypesInfo.Defs[id]._<ptr<types.Var>>();


                            else if (ok)
                            {
                                cancelvars[v] = stmt;
                            }

                            v = v__prev4;

                        }


                        v = v__prev3;

                    }

                }

                return true;

            });

            if (len(cancelvars) == 0L)
            {
                return ; // no need to inspect CFG
            } 

            // Obtain the CFG.
            ptr<ctrlflow.CFGs> cfgs = pass.ResultOf[ctrlflow.Analyzer]._<ptr<ctrlflow.CFGs>>();
            ptr<cfg.CFG> g;
            ptr<types.Signature> sig;
            switch (node.type())
            {
                case ptr<ast.FuncDecl> node:
                    sig, _ = pass.TypesInfo.Defs[node.Name].Type()._<ptr<types.Signature>>();
                    if (node.Name.Name == "main" && sig.Recv() == null && pass.Pkg.Name() == "main")
                    { 
                        // Returning from main.main terminates the process,
                        // so there's no need to cancel contexts.
                        return ;

                    }

                    g = cfgs.FuncDecl(node);
                    break;
                case ptr<ast.FuncLit> node:
                    sig, _ = pass.TypesInfo.Types[node.Type].Type._<ptr<types.Signature>>();
                    g = cfgs.FuncLit(node);
                    break;
            }
            if (sig == null)
            {
                return ; // missing type information
            } 

            // Print CFG.
            if (debug)
            {
                fmt.Println(g.Format(pass.Fset));
            } 

            // Examine the CFG for each variable in turn.
            // (It would be more efficient to analyze all cancelvars in a
            // single pass over the AST, but seldom is there more than one.)
            {
                var v__prev1 = v;
                var stmt__prev1 = stmt;

                foreach (var (__v, __stmt) in cancelvars)
                {
                    v = __v;
                    stmt = __stmt;
                    {
                        var ret = lostCancelPath(_addr_pass, g, _addr_v, stmt, sig);

                        if (ret != null)
                        {
                            var lineno = pass.Fset.Position(stmt.Pos()).Line;
                            pass.ReportRangef(stmt, "the %s function is not used on all paths (possible context leak)", v.Name());
                            pass.ReportRangef(ret, "this return statement may be reached without using the %s var defined on line %d", v.Name(), lineno);
                        }

                    }

                }

                v = v__prev1;
                stmt = stmt__prev1;
            }
        }

        private static bool isCall(ast.Node n)
        {
            ptr<ast.CallExpr> (_, ok) = n._<ptr<ast.CallExpr>>();

            return ok;
        }

        private static bool hasImport(ptr<types.Package> _addr_pkg, @string path)
        {
            ref types.Package pkg = ref _addr_pkg.val;

            foreach (var (_, imp) in pkg.Imports())
            {
                if (imp.Path() == path)
                {
                    return true;
                }

            }
            return false;

        }

        // isContextWithCancel reports whether n is one of the qualified identifiers
        // context.With{Cancel,Timeout,Deadline}.
        private static bool isContextWithCancel(ptr<types.Info> _addr_info, ast.Node n)
        {
            ref types.Info info = ref _addr_info.val;

            ptr<ast.SelectorExpr> (sel, ok) = n._<ptr<ast.SelectorExpr>>();
            if (!ok)
            {
                return false;
            }

            switch (sel.Sel.Name)
            {
                case "WithCancel": 

                case "WithTimeout": 

                case "WithDeadline": 
                    break;
                default: 
                    return false;
                    break;
            }
            {
                ptr<ast.Ident> (x, ok) = sel.X._<ptr<ast.Ident>>();

                if (ok)
                {
                    {
                        ptr<types.PkgName> (pkgname, ok) = info.Uses[x]._<ptr<types.PkgName>>();

                        if (ok)
                        {
                            return pkgname.Imported().Path() == contextPackage;
                        } 
                        // Import failed, so we can't check package path.
                        // Just check the local package name (heuristic).

                    } 
                    // Import failed, so we can't check package path.
                    // Just check the local package name (heuristic).
                    return x.Name == "context";

                }

            }

            return false;

        }

        // lostCancelPath finds a path through the CFG, from stmt (which defines
        // the 'cancel' variable v) to a return statement, that doesn't "use" v.
        // If it finds one, it returns the return statement (which may be synthetic).
        // sig is the function's type, if known.
        private static ptr<ast.ReturnStmt> lostCancelPath(ptr<analysis.Pass> _addr_pass, ptr<cfg.CFG> _addr_g, ptr<types.Var> _addr_v, ast.Node stmt, ptr<types.Signature> _addr_sig) => func((_, panic, __) =>
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref cfg.CFG g = ref _addr_g.val;
            ref types.Var v = ref _addr_v.val;
            ref types.Signature sig = ref _addr_sig.val;

            var vIsNamedResult = sig != null && tupleContains(_addr_sig.Results(), _addr_v); 

            // uses reports whether stmts contain a "use" of variable v.
            Func<ptr<analysis.Pass>, ptr<types.Var>, slice<ast.Node>, bool> uses = (pass, v, stmts) =>
            {
                var found = false;
                foreach (var (_, stmt) in stmts)
                {
                    ast.Inspect(stmt, n =>
                    {
                        switch (n.type())
                        {
                            case ptr<ast.Ident> n:
                                if (pass.TypesInfo.Uses[n] == v)
                                {
                                    found = true;
                                }

                                break;
                            case ptr<ast.ReturnStmt> n:
                                if (n.Results == null && vIsNamedResult)
                                {
                                    found = true;
                                }

                                break;
                        }
                        return _addr_!found!;

                    });

                }
                return _addr_found!;

            } 

            // blockUses computes "uses" for each block, caching the result.
; 

            // blockUses computes "uses" for each block, caching the result.
            var memo = make_map<ptr<cfg.Block>, bool>();
            Func<ptr<analysis.Pass>, ptr<types.Var>, ptr<cfg.Block>, bool> blockUses = (pass, v, b) =>
            {
                var (res, ok) = memo[b];
                if (!ok)
                {
                    res = uses(pass, v, b.Nodes);
                    memo[b] = res;
                }

                return _addr_res!;

            } 

            // Find the var's defining block in the CFG,
            // plus the rest of the statements of that block.
; 

            // Find the var's defining block in the CFG,
            // plus the rest of the statements of that block.
            ptr<cfg.Block> defblock;
            slice<ast.Node> rest = default;
outer:
            {
                var b__prev1 = b;

                foreach (var (_, __b) in g.Blocks)
                {
                    b = __b;
                    {
                        var n__prev2 = n;

                        foreach (var (__i, __n) in b.Nodes)
                        {
                            i = __i;
                            n = __n;
                            if (n == stmt)
                            {
                                defblock = b;
                                rest = b.Nodes[i + 1L..];
                                _breakouter = true;
                                break;
                            }

                        }

                        n = n__prev2;
                    }
                }

                b = b__prev1;
            }
            if (defblock == null)
            {
                panic("internal error: can't find defining block for cancel var");
            } 

            // Is v "used" in the remainder of its defining block?
            if (uses(pass, v, rest))
            {
                return _addr_null!;
            } 

            // Does the defining block return without using v?
            {
                var ret__prev1 = ret;

                var ret = defblock.Return();

                if (ret != null)
                {
                    return _addr_ret!;
                } 

                // Search the CFG depth-first for a path, from defblock to a
                // return block, in which v is never "used".

                ret = ret__prev1;

            } 

            // Search the CFG depth-first for a path, from defblock to a
            // return block, in which v is never "used".
            var seen = make_map<ptr<cfg.Block>, bool>();
            Func<slice<ptr<cfg.Block>>, ptr<ast.ReturnStmt>> search = default;
            search = blocks =>
            {
                {
                    var b__prev1 = b;

                    foreach (var (_, __b) in blocks)
                    {
                        b = __b;
                        if (seen[b])
                        {
                            continue;
                        }

                        seen[b] = true; 

                        // Prune the search if the block uses v.
                        if (blockUses(pass, v, b))
                        {
                            continue;
                        } 

                        // Found path to return statement?
                        {
                            var ret__prev1 = ret;

                            ret = b.Return();

                            if (ret != null)
                            {
                                if (debug)
                                {
                                    fmt.Printf("found path to return in block %s\n", b);
                                }

                                return _addr_ret!; // found
                            } 

                            // Recur

                            ret = ret__prev1;

                        } 

                        // Recur
                        {
                            var ret__prev1 = ret;

                            ret = search(b.Succs);

                            if (ret != null)
                            {
                                if (debug)
                                {
                                    fmt.Printf(" from block %s\n", b);
                                }

                                return _addr_ret!;

                            }

                            ret = ret__prev1;

                        }

                    }

                    b = b__prev1;
                }

                return _addr_null!;

            }
;
            return _addr_search(defblock.Succs)!;

        });

        private static bool tupleContains(ptr<types.Tuple> _addr_tuple, ptr<types.Var> _addr_v)
        {
            ref types.Tuple tuple = ref _addr_tuple.val;
            ref types.Var v = ref _addr_v.val;

            for (long i = 0L; i < tuple.Len(); i++)
            {
                if (tuple.At(i) == v)
                {
                    return true;
                }

            }

            return false;

        }
    }
}}}}}}}}}
