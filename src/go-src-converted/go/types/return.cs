// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements isTerminating.

// package types -- go2cs converted at 2020 August 29 08:47:53 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\return.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // isTerminating reports if s is a terminating statement.
        // If s is labeled, label is the label name; otherwise s
        // is "".
        private static bool isTerminating(this ref Checker check, ast.Stmt s, @string label)
        {
            switch (s.type())
            {
                case ref ast.BadStmt s:
                    break;
                case ref ast.DeclStmt s:
                    break;
                case ref ast.EmptyStmt s:
                    break;
                case ref ast.SendStmt s:
                    break;
                case ref ast.IncDecStmt s:
                    break;
                case ref ast.AssignStmt s:
                    break;
                case ref ast.GoStmt s:
                    break;
                case ref ast.DeferStmt s:
                    break;
                case ref ast.RangeStmt s:
                    break;
                case ref ast.LabeledStmt s:
                    return check.isTerminating(s.Stmt, s.Label.Name);
                    break;
                case ref ast.ExprStmt s:
                    {
                        ref ast.CallExpr (call, _) = unparen(s.X)._<ref ast.CallExpr>();

                        if (call != null)
                        {
                            {
                                ref ast.Ident (id, _) = call.Fun._<ref ast.Ident>();

                                if (id != null)
                                {
                                    {
                                        var (_, obj) = check.scope.LookupParent(id.Name, token.NoPos);

                                        if (obj != null)
                                        {
                                            {
                                                ref Builtin (b, _) = obj._<ref Builtin>();

                                                if (b != null && b.id == _Panic)
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case ref ast.ReturnStmt s:
                    return true;
                    break;
                case ref ast.BranchStmt s:
                    if (s.Tok == token.GOTO || s.Tok == token.FALLTHROUGH)
                    {
                        return true;
                    }
                    break;
                case ref ast.BlockStmt s:
                    return check.isTerminatingList(s.List, "");
                    break;
                case ref ast.IfStmt s:
                    if (s.Else != null && check.isTerminating(s.Body, "") && check.isTerminating(s.Else, ""))
                    {
                        return true;
                    }
                    break;
                case ref ast.SwitchStmt s:
                    return check.isTerminatingSwitch(s.Body, label);
                    break;
                case ref ast.TypeSwitchStmt s:
                    return check.isTerminatingSwitch(s.Body, label);
                    break;
                case ref ast.SelectStmt s:
                    {
                        var s__prev1 = s;

                        foreach (var (_, __s) in s.Body.List)
                        {
                            s = __s;
                            ref ast.CommClause cc = s._<ref ast.CommClause>();
                            if (!check.isTerminatingList(cc.Body, "") || hasBreakList(cc.Body, label, true))
                            {
                                return false;
                            }
                        }
                        s = s__prev1;
                    }

                    return true;
                    break;
                case ref ast.ForStmt s:
                    if (s.Cond == null && !hasBreak(s.Body, label, true))
                    {
                        return true;
                    }
                    break;
                default:
                {
                    var s = s.type();
                    unreachable();
                    break;
                }

            }

            return false;
        }

        private static bool isTerminatingList(this ref Checker check, slice<ast.Stmt> list, @string label)
        { 
            // trailing empty statements are permitted - skip them
            for (var i = len(list) - 1L; i >= 0L; i--)
            {
                {
                    ref ast.EmptyStmt (_, ok) = list[i]._<ref ast.EmptyStmt>();

                    if (!ok)
                    {
                        return check.isTerminating(list[i], label);
                    }

                }
            }

            return false; // all statements are empty
        }

        private static bool isTerminatingSwitch(this ref Checker check, ref ast.BlockStmt body, @string label)
        {
            var hasDefault = false;
            foreach (var (_, s) in body.List)
            {
                ref ast.CaseClause cc = s._<ref ast.CaseClause>();
                if (cc.List == null)
                {
                    hasDefault = true;
                }
                if (!check.isTerminatingList(cc.Body, "") || hasBreakList(cc.Body, label, true))
                {
                    return false;
                }
            }
            return hasDefault;
        }

        // TODO(gri) For nested breakable statements, the current implementation of hasBreak
        //         will traverse the same subtree repeatedly, once for each label. Replace
        //           with a single-pass label/break matching phase.

        // hasBreak reports if s is or contains a break statement
        // referring to the label-ed statement or implicit-ly the
        // closest outer breakable statement.
        private static bool hasBreak(ast.Stmt s, @string label, bool @implicit)
        {
            switch (s.type())
            {
                case ref ast.BadStmt s:
                    break;
                case ref ast.DeclStmt s:
                    break;
                case ref ast.EmptyStmt s:
                    break;
                case ref ast.ExprStmt s:
                    break;
                case ref ast.SendStmt s:
                    break;
                case ref ast.IncDecStmt s:
                    break;
                case ref ast.AssignStmt s:
                    break;
                case ref ast.GoStmt s:
                    break;
                case ref ast.DeferStmt s:
                    break;
                case ref ast.ReturnStmt s:
                    break;
                case ref ast.LabeledStmt s:
                    return hasBreak(s.Stmt, label, implicit);
                    break;
                case ref ast.BranchStmt s:
                    if (s.Tok == token.BREAK)
                    {
                        if (s.Label == null)
                        {
                            return implicit;
                        }
                        if (s.Label.Name == label)
                        {
                            return true;
                        }
                    }
                    break;
                case ref ast.BlockStmt s:
                    return hasBreakList(s.List, label, implicit);
                    break;
                case ref ast.IfStmt s:
                    if (hasBreak(s.Body, label, implicit) || s.Else != null && hasBreak(s.Else, label, implicit))
                    {
                        return true;
                    }
                    break;
                case ref ast.CaseClause s:
                    return hasBreakList(s.Body, label, implicit);
                    break;
                case ref ast.SwitchStmt s:
                    if (label != "" && hasBreak(s.Body, label, false))
                    {
                        return true;
                    }
                    break;
                case ref ast.TypeSwitchStmt s:
                    if (label != "" && hasBreak(s.Body, label, false))
                    {
                        return true;
                    }
                    break;
                case ref ast.CommClause s:
                    return hasBreakList(s.Body, label, implicit);
                    break;
                case ref ast.SelectStmt s:
                    if (label != "" && hasBreak(s.Body, label, false))
                    {
                        return true;
                    }
                    break;
                case ref ast.ForStmt s:
                    if (label != "" && hasBreak(s.Body, label, false))
                    {
                        return true;
                    }
                    break;
                case ref ast.RangeStmt s:
                    if (label != "" && hasBreak(s.Body, label, false))
                    {
                        return true;
                    }
                    break;
                default:
                {
                    var s = s.type();
                    unreachable();
                    break;
                }

            }

            return false;
        }

        private static bool hasBreakList(slice<ast.Stmt> list, @string label, bool @implicit)
        {
            foreach (var (_, s) in list)
            {
                if (hasBreak(s, label, implicit))
                {
                    return true;
                }
            }
            return false;
        }
    }
}}
