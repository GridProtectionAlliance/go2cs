// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 10:08:56 UTC
// Original source: C:\Go\src\cmd\vet\lostcancel.go
using cfg = go.cmd.vet.@internal.cfg_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using types = go.go.types_package;
using strconv = go.strconv_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("lostcancel", "check for failure to call cancelation function returned by context.WithCancel", checkLostCancel, funcDecl, funcLit);
        }

        private static readonly var debugLostCancel = false;



        private static @string contextPackage = "context";

        // checkLostCancel reports a failure to the call the cancel function
        // returned by context.WithCancel, either because the variable was
        // assigned to the blank identifier, or because there exists a
        // control-flow path from the call to a return statement and that path
        // does not "use" the cancel function.  Any reference to the variable
        // counts as a use, even within a nested function literal.
        //
        // checkLostCancel analyzes a single named or literal function.
        private static void checkLostCancel(ref File f, ast.Node node)
        { 
            // Fast path: bypass check if file doesn't use context.WithCancel.
            if (!hasImport(f.file, contextPackage))
            {
                return;
            } 

            // Maps each cancel variable to its defining ValueSpec/AssignStmt.
            var cancelvars = make_map<ref types.Var, ast.Node>(); 

            // Find the set of cancel vars to analyze.
            var stack = make_slice<ast.Node>(0L, 32L);
            ast.Inspect(node, n =>
            {
                switch (n.type())
                {
                    case ref ast.FuncLit _:
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
                if (isContextWithCancel(f, n) && isCall(stack[len(stack) - 2L]))
                {
                    ref ast.Ident id = default; // id of cancel var
                    var stmt = stack[len(stack) - 3L];
                    switch (stmt.type())
                    {
                        case ref ast.ValueSpec stmt:
                            if (len(stmt.Names) > 1L)
                            {
                                id = stmt.Names[1L];
                            }
                            break;
                        case ref ast.AssignStmt stmt:
                            if (len(stmt.Lhs) > 1L)
                            {
                                id, _ = stmt.Lhs[1L]._<ref ast.Ident>();
                            }
                            break;
                    }
                    if (id != null)
                    {
                        if (id.Name == "_")
                        {
                            f.Badf(id.Pos(), "the cancel function returned by context.%s should be called, not discarded, to avoid a context leak", n._<ref ast.SelectorExpr>().Sel.Name);
                        }                        {
                            ref types.Var v__prev4 = v;

                            ref types.Var (v, ok) = f.pkg.uses[id]._<ref types.Var>();


                            else if (ok)
                            {
                                cancelvars[v] = stmt;
                            }                            {
                                ref types.Var v__prev5 = v;

                                (v, ok) = f.pkg.defs[id]._<ref types.Var>();


                                else if (ok)
                                {
                                    cancelvars[v] = stmt;
                                }

                                v = v__prev5;

                            }

                            v = v__prev4;

                        }
                    }
                }
                return true;
            });

            if (len(cancelvars) == 0L)
            {
                return; // no need to build CFG
            } 

            // Tell the CFG builder which functions never return.
            types.Info info = ref new types.Info(Uses:f.pkg.uses,Selections:f.pkg.selectors);
            Func<ref ast.CallExpr, bool> mayReturn = call =>
            {
                var name = callName(info, call);
                return !noReturnFuncs[name];
            } 

            // Build the CFG.
; 

            // Build the CFG.
            ref cfg.CFG g = default;
            ref types.Signature sig = default;
            switch (node.type())
            {
                case ref ast.FuncDecl node:
                    var obj = f.pkg.defs[node.Name];
                    if (obj == null)
                    {
                        return; // type error (e.g. duplicate function declaration)
                    }
                    sig, _ = obj.Type()._<ref types.Signature>();
                    g = cfg.New(node.Body, mayReturn);
                    break;
                case ref ast.FuncLit node:
                    sig, _ = f.pkg.types[node.Type].Type._<ref types.Signature>();
                    g = cfg.New(node.Body, mayReturn);
                    break; 

                // Print CFG.
            } 

            // Print CFG.
            if (debugLostCancel)
            {
                fmt.Println(g.Format(f.fset));
            } 

            // Examine the CFG for each variable in turn.
            // (It would be more efficient to analyze all cancelvars in a
            // single pass over the AST, but seldom is there more than one.)
            {
                ref types.Var v__prev1 = v;
                var stmt__prev1 = stmt;

                foreach (var (__v, __stmt) in cancelvars)
                {
                    v = __v;
                    stmt = __stmt;
                    {
                        var ret = lostCancelPath(f, g, v, stmt, sig);

                        if (ret != null)
                        {
                            var lineno = f.fset.Position(stmt.Pos()).Line;
                            f.Badf(stmt.Pos(), "the %s function is not used on all paths (possible context leak)", v.Name());
                            f.Badf(ret.Pos(), "this return statement may be reached without using the %s var defined on line %d", v.Name(), lineno);
                        }

                    }
                }

                v = v__prev1;
                stmt = stmt__prev1;
            }

        }

        private static bool isCall(ast.Node n)
        {
            ref ast.CallExpr (_, ok) = n._<ref ast.CallExpr>();

            return ok;
        }

        private static bool hasImport(ref ast.File f, @string path)
        {
            foreach (var (_, imp) in f.Imports)
            {
                var (v, _) = strconv.Unquote(imp.Path.Value);
                if (v == path)
                {
                    return true;
                }
            }
            return false;
        }

        // isContextWithCancel reports whether n is one of the qualified identifiers
        // context.With{Cancel,Timeout,Deadline}.
        private static bool isContextWithCancel(ref File f, ast.Node n)
        {
            {
                ref ast.SelectorExpr (sel, ok) = n._<ref ast.SelectorExpr>();

                if (ok)
                {
                    switch (sel.Sel.Name)
                    {
                        case "WithCancel": 

                        case "WithTimeout": 

                        case "WithDeadline": 
                            {
                                ref ast.Ident (x, ok) = sel.X._<ref ast.Ident>();

                                if (ok)
                                {
                                    {
                                        ref types.PkgName (pkgname, ok) = f.pkg.uses[x]._<ref types.PkgName>();

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
                            break;
                    }
                }

            }
            return false;
        }

        // lostCancelPath finds a path through the CFG, from stmt (which defines
        // the 'cancel' variable v) to a return statement, that doesn't "use" v.
        // If it finds one, it returns the return statement (which may be synthetic).
        // sig is the function's type, if known.
        private static ref ast.ReturnStmt lostCancelPath(ref File _f, ref cfg.CFG _g, ref types.Var _v, ast.Node stmt, ref types.Signature _sig) => func(_f, _g, _v, _sig, (ref File f, ref cfg.CFG g, ref types.Var v, ref types.Signature sig, Defer _, Panic panic, Recover __) =>
        {
            var vIsNamedResult = sig != null && tupleContains(sig.Results(), v); 

            // uses reports whether stmts contain a "use" of variable v.
            Func<ref File, ref types.Var, slice<ast.Node>, bool> uses = (f, v, stmts) =>
            {
                var found = false;
                foreach (var (_, stmt) in stmts)
                {
                    ast.Inspect(stmt, n =>
                    {
                        switch (n.type())
                        {
                            case ref ast.Ident n:
                                if (f.pkg.uses[n] == v)
                                {
                                    found = true;
                                }
                                break;
                            case ref ast.ReturnStmt n:
                                if (n.Results == null && vIsNamedResult)
                                {
                                    found = true;
                                }
                                break;
                        }
                        return !found;
                    });
                }
                return found;
            } 

            // blockUses computes "uses" for each block, caching the result.
; 

            // blockUses computes "uses" for each block, caching the result.
            var memo = make_map<ref cfg.Block, bool>();
            Func<ref File, ref types.Var, ref cfg.Block, bool> blockUses = (f, v, b) =>
            {
                var (res, ok) = memo[b];
                if (!ok)
                {
                    res = uses(f, v, b.Nodes);
                    memo[b] = res;
                }
                return res;
            } 

            // Find the var's defining block in the CFG,
            // plus the rest of the statements of that block.
; 

            // Find the var's defining block in the CFG,
            // plus the rest of the statements of that block.
            ref cfg.Block defblock = default;
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
            if (uses(f, v, rest))
            {
                return null;
            } 

            // Does the defining block return without using v?
            {
                var ret__prev1 = ret;

                var ret = defblock.Return();

                if (ret != null)
                {
                    return ret;
                } 

                // Search the CFG depth-first for a path, from defblock to a
                // return block, in which v is never "used".

                ret = ret__prev1;

            } 

            // Search the CFG depth-first for a path, from defblock to a
            // return block, in which v is never "used".
            var seen = make_map<ref cfg.Block, bool>();
            Func<slice<ref cfg.Block>, ref ast.ReturnStmt> search = default;
            search = blocks =>
            {
                {
                    var b__prev1 = b;

                    foreach (var (_, __b) in blocks)
                    {
                        b = __b;
                        if (!seen[b])
                        {
                            seen[b] = true; 

                            // Prune the search if the block uses v.
                            if (blockUses(f, v, b))
                            {
                                continue;
                            } 

                            // Found path to return statement?
                            {
                                var ret__prev2 = ret;

                                ret = b.Return();

                                if (ret != null)
                                {
                                    if (debugLostCancel)
                                    {
                                        fmt.Printf("found path to return in block %s\n", b);
                                    }
                                    return ret; // found
                                } 

                                // Recur

                                ret = ret__prev2;

                            } 

                            // Recur
                            {
                                var ret__prev2 = ret;

                                ret = search(b.Succs);

                                if (ret != null)
                                {
                                    if (debugLostCancel)
                                    {
                                        fmt.Printf(" from block %s\n", b);
                                    }
                                    return ret;
                                }

                                ret = ret__prev2;

                            }
                        }
                    }

                    b = b__prev1;
                }

                return null;
            }
;
            return search(defblock.Succs);
        });

        private static bool tupleContains(ref types.Tuple tuple, ref types.Var v)
        {
            for (long i = 0L; i < tuple.Len(); i++)
            {
                if (tuple.At(i) == v)
                {
                    return true;
                }
            }

            return false;
        }

        private static map noReturnFuncs = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, bool>{"(*testing.common).FailNow":true,"(*testing.common).Fatal":true,"(*testing.common).Fatalf":true,"(*testing.common).Skip":true,"(*testing.common).SkipNow":true,"(*testing.common).Skipf":true,"log.Fatal":true,"log.Fatalf":true,"log.Fatalln":true,"os.Exit":true,"panic":true,"runtime.Goexit":true,};

        // callName returns the canonical name of the builtin, method, or
        // function called by call, if known.
        private static @string callName(ref types.Info info, ref ast.CallExpr call)
        {
            switch (call.Fun.type())
            {
                case ref ast.Ident fun:
                    {
                        ref types.Builtin obj__prev1 = obj;

                        ref types.Builtin (obj, ok) = info.Uses[fun]._<ref types.Builtin>();

                        if (ok)
                        {
                            return obj.Name();
                        }

                        obj = obj__prev1;

                    }
                    break;
                case ref ast.SelectorExpr fun:
                    {
                        var (sel, ok) = info.Selections[fun];

                        if (ok && sel.Kind() == types.MethodVal)
                        { 
                            // method call, e.g. "(*testing.common).Fatal"
                            var meth = sel.Obj();
                            return fmt.Sprintf("(%s).%s", meth.Type()._<ref types.Signature>().Recv().Type(), meth.Name());
                        }

                    }
                    {
                        ref types.Builtin obj__prev1 = obj;

                        (obj, ok) = info.Uses[fun.Sel];

                        if (ok)
                        { 
                            // qualified identifier, e.g. "os.Exit"
                            return fmt.Sprintf("%s.%s", obj.Pkg().Path(), obj.Name());
                        }

                        obj = obj__prev1;

                    }
                    break; 

                // function with no name, or defined in missing imported package
            } 

            // function with no name, or defined in missing imported package
            return "";
        }
    }
}
