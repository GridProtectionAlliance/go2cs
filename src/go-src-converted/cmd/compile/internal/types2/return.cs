// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements isTerminating.

// package types2 -- go2cs converted at 2022 March 13 06:26:14 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\return.go
namespace go.cmd.compile.@internal;

using syntax = cmd.compile.@internal.syntax_package;


// isTerminating reports if s is a terminating statement.
// If s is labeled, label is the label name; otherwise s
// is "".

public static partial class types2_package {

private static bool isTerminating(this ptr<Checker> _addr_check, syntax.Stmt s, @string label) {
    ref Checker check = ref _addr_check.val;

    switch (s.type()) {
        case ptr<syntax.DeclStmt> s:
            break;
        case ptr<syntax.EmptyStmt> s:
            break;
        case ptr<syntax.SendStmt> s:
            break;
        case ptr<syntax.AssignStmt> s:
            break;
        case ptr<syntax.CallStmt> s:
            break;
        case ptr<syntax.LabeledStmt> s:
            return check.isTerminating(s.Stmt, s.Label.Value);
            break;
        case ptr<syntax.ExprStmt> s:
            {
                ptr<syntax.CallExpr> (call, ok) = unparen(s.X)._<ptr<syntax.CallExpr>>();

                if (ok && check.isPanic[call]) {
                    return true;
                }
            }
            break;
        case ptr<syntax.ReturnStmt> s:
            return true;
            break;
        case ptr<syntax.BranchStmt> s:
            if (s.Tok == syntax.Goto || s.Tok == syntax.Fallthrough) {
                return true;
            }
            break;
        case ptr<syntax.BlockStmt> s:
            return check.isTerminatingList(s.List, "");
            break;
        case ptr<syntax.IfStmt> s:
            if (s.Else != null && check.isTerminating(s.Then, "") && check.isTerminating(s.Else, "")) {
                return true;
            }
            break;
        case ptr<syntax.SwitchStmt> s:
            return check.isTerminatingSwitch(s.Body, label);
            break;
        case ptr<syntax.SelectStmt> s:
            foreach (var (_, cc) in s.Body) {
                if (!check.isTerminatingList(cc.Body, "") || hasBreakList(cc.Body, label, true)) {
                    return false;
                }
            }            return true;
            break;
        case ptr<syntax.ForStmt> s:
            if (s.Cond == null && !hasBreak(s.Body, label, true)) {
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

private static bool isTerminatingList(this ptr<Checker> _addr_check, slice<syntax.Stmt> list, @string label) {
    ref Checker check = ref _addr_check.val;
 
    // trailing empty statements are permitted - skip them
    for (var i = len(list) - 1; i >= 0; i--) {
        {
            ptr<syntax.EmptyStmt> (_, ok) = list[i]._<ptr<syntax.EmptyStmt>>();

            if (!ok) {
                return check.isTerminating(list[i], label);
            }

        }
    }
    return false; // all statements are empty
}

private static bool isTerminatingSwitch(this ptr<Checker> _addr_check, slice<ptr<syntax.CaseClause>> body, @string label) {
    ref Checker check = ref _addr_check.val;

    var hasDefault = false;
    foreach (var (_, cc) in body) {
        if (cc.Cases == null) {
            hasDefault = true;
        }
        if (!check.isTerminatingList(cc.Body, "") || hasBreakList(cc.Body, label, true)) {
            return false;
        }
    }    return hasDefault;
}

// TODO(gri) For nested breakable statements, the current implementation of hasBreak
//         will traverse the same subtree repeatedly, once for each label. Replace
//           with a single-pass label/break matching phase.

// hasBreak reports if s is or contains a break statement
// referring to the label-ed statement or implicit-ly the
// closest outer breakable statement.
private static bool hasBreak(syntax.Stmt s, @string label, bool @implicit) {
    switch (s.type()) {
        case ptr<syntax.DeclStmt> s:
            break;
        case ptr<syntax.EmptyStmt> s:
            break;
        case ptr<syntax.ExprStmt> s:
            break;
        case ptr<syntax.SendStmt> s:
            break;
        case ptr<syntax.AssignStmt> s:
            break;
        case ptr<syntax.CallStmt> s:
            break;
        case ptr<syntax.ReturnStmt> s:
            break;
        case ptr<syntax.LabeledStmt> s:
            return hasBreak(s.Stmt, label, implicit);
            break;
        case ptr<syntax.BranchStmt> s:
            if (s.Tok == syntax.Break) {
                if (s.Label == null) {
                    return implicit;
                }
                if (s.Label.Value == label) {
                    return true;
                }
            }
            break;
        case ptr<syntax.BlockStmt> s:
            return hasBreakList(s.List, label, implicit);
            break;
        case ptr<syntax.IfStmt> s:
            if (hasBreak(s.Then, label, implicit) || s.Else != null && hasBreak(s.Else, label, implicit)) {
                return true;
            }
            break;
        case ptr<syntax.SwitchStmt> s:
            if (label != "" && hasBreakCaseList(s.Body, label, false)) {
                return true;
            }
            break;
        case ptr<syntax.SelectStmt> s:
            if (label != "" && hasBreakCommList(s.Body, label, false)) {
                return true;
            }
            break;
        case ptr<syntax.ForStmt> s:
            if (label != "" && hasBreak(s.Body, label, false)) {
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

private static bool hasBreakList(slice<syntax.Stmt> list, @string label, bool @implicit) {
    foreach (var (_, s) in list) {
        if (hasBreak(s, label, implicit)) {
            return true;
        }
    }    return false;
}

private static bool hasBreakCaseList(slice<ptr<syntax.CaseClause>> list, @string label, bool @implicit) {
    foreach (var (_, s) in list) {
        if (hasBreakList(s.Body, label, implicit)) {
            return true;
        }
    }    return false;
}

private static bool hasBreakCommList(slice<ptr<syntax.CommClause>> list, @string label, bool @implicit) {
    foreach (var (_, s) in list) {
        if (hasBreakList(s.Body, label, implicit)) {
            return true;
        }
    }    return false;
}

} // end types2_package
