// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements isTerminating.

// package types -- go2cs converted at 2020 October 08 04:03:41 UTC
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
        private static bool isTerminating(this ptr<Checker> _addr_check, ast.Stmt s, @string label)
        {
            ref Checker check = ref _addr_check.val;

            switch (s.type())
            {
                case ptr<ast.BadStmt> s:
                    break;
                case ptr<ast.DeclStmt> s:
                    break;
                case ptr<ast.EmptyStmt> s:
                    break;
                case ptr<ast.SendStmt> s:
                    break;
                case ptr<ast.IncDecStmt> s:
                    break;
                case ptr<ast.AssignStmt> s:
                    break;
                case ptr<ast.GoStmt> s:
                    break;
                case ptr<ast.DeferStmt> s:
                    break;
                case ptr<ast.RangeStmt> s:
                    break;
                case ptr<ast.LabeledStmt> s:
                    return check.isTerminating(s.Stmt, s.Label.Name);
                    break;
                case ptr<ast.ExprStmt> s:
                    {
                        ptr<ast.CallExpr> (call, ok) = unparen(s.X)._<ptr<ast.CallExpr>>();

                        if (ok && check.isPanic[call])
                        {
                            return true;
                        }
                    }


                    break;
                case ptr<ast.ReturnStmt> s:
                    return true;
                    break;
                case ptr<ast.BranchStmt> s:
                    if (s.Tok == token.GOTO || s.Tok == token.FALLTHROUGH)
                    {
                        return true;
                    }
                    break;
                case ptr<ast.BlockStmt> s:
                    return check.isTerminatingList(s.List, "");
                    break;
                case ptr<ast.IfStmt> s:
                    if (s.Else != null && check.isTerminating(s.Body, "") && check.isTerminating(s.Else, ""))
                    {
                        return true;
                    }
                    break;
                case ptr<ast.SwitchStmt> s:
                    return check.isTerminatingSwitch(s.Body, label);
                    break;
                case ptr<ast.TypeSwitchStmt> s:
                    return check.isTerminatingSwitch(s.Body, label);
                    break;
                case ptr<ast.SelectStmt> s:
                    {
                        var s__prev1 = s;

                        foreach (var (_, __s) in s.Body.List)
                        {
                            s = __s;
                            ptr<ast.CommClause> cc = s._<ptr<ast.CommClause>>();
                            if (!check.isTerminatingList(cc.Body, "") || hasBreakList(cc.Body, label, true))
                            {
                                return false;
                            }
                        }
                        s = s__prev1;
                    }

                    return true;
                    break;
                case ptr<ast.ForStmt> s:
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

        private static bool isTerminatingList(this ptr<Checker> _addr_check, slice<ast.Stmt> list, @string label)
        {
            ref Checker check = ref _addr_check.val;
 
            // trailing empty statements are permitted - skip them
            for (var i = len(list) - 1L; i >= 0L; i--)
            {
                {
                    ptr<ast.EmptyStmt> (_, ok) = list[i]._<ptr<ast.EmptyStmt>>();

                    if (!ok)
                    {
                        return check.isTerminating(list[i], label);
                    }

                }

            }

            return false; // all statements are empty
        }

        private static bool isTerminatingSwitch(this ptr<Checker> _addr_check, ptr<ast.BlockStmt> _addr_body, @string label)
        {
            ref Checker check = ref _addr_check.val;
            ref ast.BlockStmt body = ref _addr_body.val;

            var hasDefault = false;
            foreach (var (_, s) in body.List)
            {
                ptr<ast.CaseClause> cc = s._<ptr<ast.CaseClause>>();
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
                case ptr<ast.BadStmt> s:
                    break;
                case ptr<ast.DeclStmt> s:
                    break;
                case ptr<ast.EmptyStmt> s:
                    break;
                case ptr<ast.ExprStmt> s:
                    break;
                case ptr<ast.SendStmt> s:
                    break;
                case ptr<ast.IncDecStmt> s:
                    break;
                case ptr<ast.AssignStmt> s:
                    break;
                case ptr<ast.GoStmt> s:
                    break;
                case ptr<ast.DeferStmt> s:
                    break;
                case ptr<ast.ReturnStmt> s:
                    break;
                case ptr<ast.LabeledStmt> s:
                    return hasBreak(s.Stmt, label, implicit);
                    break;
                case ptr<ast.BranchStmt> s:
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
                case ptr<ast.BlockStmt> s:
                    return hasBreakList(s.List, label, implicit);
                    break;
                case ptr<ast.IfStmt> s:
                    if (hasBreak(s.Body, label, implicit) || s.Else != null && hasBreak(s.Else, label, implicit))
                    {
                        return true;
                    }

                    break;
                case ptr<ast.CaseClause> s:
                    return hasBreakList(s.Body, label, implicit);
                    break;
                case ptr<ast.SwitchStmt> s:
                    if (label != "" && hasBreak(s.Body, label, false))
                    {
                        return true;
                    }

                    break;
                case ptr<ast.TypeSwitchStmt> s:
                    if (label != "" && hasBreak(s.Body, label, false))
                    {
                        return true;
                    }

                    break;
                case ptr<ast.CommClause> s:
                    return hasBreakList(s.Body, label, implicit);
                    break;
                case ptr<ast.SelectStmt> s:
                    if (label != "" && hasBreak(s.Body, label, false))
                    {
                        return true;
                    }

                    break;
                case ptr<ast.ForStmt> s:
                    if (label != "" && hasBreak(s.Body, label, false))
                    {
                        return true;
                    }

                    break;
                case ptr<ast.RangeStmt> s:
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
