// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package shift -- go2cs converted at 2020 October 09 06:04:44 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/shift" ==> using shift = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.shift_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\shift\dead.go
// Simplified dead code detector.
// Used for skipping shift checks on unreachable arch-specific code.

using ast = go.go.ast_package;
using constant = go.go.constant_package;
using types = go.go.types_package;
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
    public static partial class shift_package
    {
        // updateDead puts unreachable "if" and "case" nodes into dead.
        private static void updateDead(ptr<types.Info> _addr_info, map<ast.Node, bool> dead, ast.Node node)
        {
            ref types.Info info = ref _addr_info.val;

            if (dead[node])
            { 
                // The node is already marked as dead.
                return ;

            }
            Action<ast.Node> setDead = n =>
            {
                ast.Inspect(n, node =>
                {
                    if (node != null)
                    {
                        dead[node] = true;
                    }
                    return true;

                });

            };

            switch (node.type())
            {
                case ptr<ast.IfStmt> stmt:
                    var v = info.Types[stmt.Cond].Value;
                    if (v == null)
                    {
                        return ;
                    }
                    if (!constant.BoolVal(v))
                    {
                        setDead(stmt.Body);
                        return ;
                    }
                    if (stmt.Else != null)
                    {
                        setDead(stmt.Else);
                    }
                    break;
                case ptr<ast.SwitchStmt> stmt:
                    if (stmt.Tag == null)
                    {
BodyLoopBool:
                        {
                            var stmt__prev1 = stmt;

                            foreach (var (_, __stmt) in stmt.Body.List)
                            {
                                stmt = __stmt;
                                ptr<ast.CaseClause> cc = stmt._<ptr<ast.CaseClause>>();
                                if (cc.List == null)
                                { 
                                    // Skip default case.
                                    continue;

                                }
                                {
                                    var expr__prev2 = expr;

                                    foreach (var (_, __expr) in cc.List)
                                    {
                                        expr = __expr;
                                        v = info.Types[expr].Value;
                                        if (v == null || v.Kind() != constant.Bool || constant.BoolVal(v))
                                        {
                                            _continueBodyLoopBool = true;
                                            break;
                                        }
                                    }
                                    expr = expr__prev2;
                                }

                                setDead(cc);

                            }
                            stmt = stmt__prev1;
                        }
                        return ;

                    }
                    v = info.Types[stmt.Tag].Value;
                    if (v == null || v.Kind() != constant.Int)
                    {
                        return ;
                    }
                    var (tagN, ok) = constant.Uint64Val(v);
                    if (!ok)
                    {
                        return ;
                    }
BodyLoopInt:
                    foreach (var (_, x) in stmt.Body.List)
                    {
                        cc = x._<ptr<ast.CaseClause>>();
                        if (cc.List == null)
                        { 
                            // Skip default case.
                            continue;

                        }
                        {
                            var expr__prev2 = expr;

                            foreach (var (_, __expr) in cc.List)
                            {
                                expr = __expr;
                                v = info.Types[expr].Value;
                                if (v == null)
                                {
                                    _continueBodyLoopInt = true;
                                    break;
                                }
                                var (n, ok) = constant.Uint64Val(v);
                                if (!ok || tagN == n)
                                {
                                    _continueBodyLoopInt = true;
                                    break;
                                }
                            }
                            expr = expr__prev2;
                        }

                        setDead(cc);

                    }                    break;
            }

        }
    }
}}}}}}}}}
