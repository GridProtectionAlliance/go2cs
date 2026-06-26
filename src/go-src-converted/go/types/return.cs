// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements isTerminating.
namespace go.go;

using ast = go.ast_package;
using token = go.token_package;

partial class types_package {

// isTerminating reports if s is a terminating statement.
// If s is labeled, label is the label name; otherwise s
// is "".
[GoRecv] internal static bool isTerminating(this ref Checker check, ast.Stmt s, @string label) {
    switch (s.type()) {
    default: {
        var s = s.type();
        throw panic("unreachable");
        break;
    }
    case ж<ast.BadStmt> s: {
        break;
    }
    case ж<ast.DeclStmt> s: {
        break;
    }
    case ж<ast.EmptyStmt> s: {
        break;
    }
    case ж<ast.SendStmt> s: {
        break;
    }
    case ж<ast.IncDecStmt> s: {
        break;
    }
    case ж<ast.AssignStmt> s: {
        break;
    }
    case ж<ast.GoStmt> s: {
        break;
    }
    case ж<ast.DeferStmt> s: {
        break;
    }
    case ж<ast.RangeStmt> s: {
        break;
    }
    case ж<ast.LabeledStmt> s: {
        return check.isTerminating((~s).Stmt, // no chance
 (~(~s).Label).Name);
    }
    case ж<ast.ExprStmt> s: {
        {
            var (call, ok) = ast.Unparen((~s).X)._<ж<ast.CallExpr>>(ᐧ); if (ok && check.isPanic[call]) {
                // calling the predeclared (possibly parenthesized) panic() function is terminating
                return true;
            }
        }
        break;
    }
    case ж<ast.ReturnStmt> s: {
        return true;
    }
    case ж<ast.BranchStmt> s: {
        if ((~s).Tok == token.GOTO || (~s).Tok == token.FALLTHROUGH) {
            return true;
        }
        break;
    }
    case ж<ast.BlockStmt> s: {
        return check.isTerminatingList((~s).List, ""u8);
    }
    case ж<ast.IfStmt> s: {
        if ((~s).Else != default! && check.isTerminating(~(~s).Body, ""u8) && check.isTerminating((~s).Else, ""u8)) {
            return true;
        }
        break;
    }
    case ж<ast.SwitchStmt> s: {
        return check.isTerminatingSwitch((~s).Body, label);
    }
    case ж<ast.TypeSwitchStmt> s: {
        return check.isTerminatingSwitch((~s).Body, label);
    }
    case ж<ast.SelectStmt> s: {
        foreach (var (_, sΔ1) in (~(~s).Body).List) {
            var cc = sΔ1._<ж<ast.CommClause>>();
            if (!check.isTerminatingList((~cc).Body, ""u8) || hasBreakList((~cc).Body, label, true)) {
                return false;
            }
        }
        return true;
    }
    case ж<ast.ForStmt> s: {
        if ((~s).Cond == default! && !hasBreak(~(~s).Body, label, true)) {
            return true;
        }
        break;
    }}
    return false;
}

[GoRecv] internal static bool isTerminatingList(this ref Checker check, slice<ast.Stmt> list, @string label) {
    // trailing empty statements are permitted - skip them
    for (nint i = len(list) - 1; i >= 0; i--) {
        {
            var (_, ok) = list[i]._<ж<ast.EmptyStmt>>(ᐧ); if (!ok) {
                return check.isTerminating(list[i], label);
            }
        }
    }
    return false;
}

// all statements are empty
[GoRecv] public static bool isTerminatingSwitch(this ref Checker check, ж<ast.BlockStmt> Ꮡbody, @string label) {
    ref var body = ref Ꮡbody.val;

    var hasDefault = false;
    foreach (var (_, s) in body.List) {
        var cc = s._<ж<ast.CaseClause>>();
        if ((~cc).List == default!) {
            hasDefault = true;
        }
        if (!check.isTerminatingList((~cc).Body, ""u8) || hasBreakList((~cc).Body, label, true)) {
            return false;
        }
    }
    return hasDefault;
}

// TODO(gri) For nested breakable statements, the current implementation of hasBreak
// will traverse the same subtree repeatedly, once for each label. Replace
// with a single-pass label/break matching phase.

// hasBreak reports if s is or contains a break statement
// referring to the label-ed statement or implicit-ly the
// closest outer breakable statement.
internal static bool hasBreak(ast.Stmt s, @string label, bool @implicit) {
    switch (s.type()) {
    default: {
        var s = s.type();
        throw panic("unreachable");
        break;
    }
    case ж<ast.BadStmt> s: {
        break;
    }
    case ж<ast.DeclStmt> s: {
        break;
    }
    case ж<ast.EmptyStmt> s: {
        break;
    }
    case ж<ast.ExprStmt> s: {
        break;
    }
    case ж<ast.SendStmt> s: {
        break;
    }
    case ж<ast.IncDecStmt> s: {
        break;
    }
    case ж<ast.AssignStmt> s: {
        break;
    }
    case ж<ast.GoStmt> s: {
        break;
    }
    case ж<ast.DeferStmt> s: {
        break;
    }
    case ж<ast.ReturnStmt> s: {
        break;
    }
    case ж<ast.LabeledStmt> s: {
        return hasBreak((~s).Stmt, // no chance
 label, @implicit);
    }
    case ж<ast.BranchStmt> s: {
        if ((~s).Tok == token.BREAK) {
            if ((~s).Label == nil) {
                return @implicit;
            }
            if ((~(~s).Label).Name == label) {
                return true;
            }
        }
        break;
    }
    case ж<ast.BlockStmt> s: {
        return hasBreakList((~s).List, label, @implicit);
    }
    case ж<ast.IfStmt> s: {
        if (hasBreak(~(~s).Body, label, @implicit) || (~s).Else != default! && hasBreak((~s).Else, label, @implicit)) {
            return true;
        }
        break;
    }
    case ж<ast.CaseClause> s: {
        return hasBreakList((~s).Body, label, @implicit);
    }
    case ж<ast.SwitchStmt> s: {
        if (label != ""u8 && hasBreak(~(~s).Body, label, false)) {
            return true;
        }
        break;
    }
    case ж<ast.TypeSwitchStmt> s: {
        if (label != ""u8 && hasBreak(~(~s).Body, label, false)) {
            return true;
        }
        break;
    }
    case ж<ast.CommClause> s: {
        return hasBreakList((~s).Body, label, @implicit);
    }
    case ж<ast.SelectStmt> s: {
        if (label != ""u8 && hasBreak(~(~s).Body, label, false)) {
            return true;
        }
        break;
    }
    case ж<ast.ForStmt> s: {
        if (label != ""u8 && hasBreak(~(~s).Body, label, false)) {
            return true;
        }
        break;
    }
    case ж<ast.RangeStmt> s: {
        if (label != ""u8 && hasBreak(~(~s).Body, label, false)) {
            return true;
        }
        break;
    }}
    return false;
}

internal static bool hasBreakList(slice<ast.Stmt> list, @string label, bool @implicit) {
    foreach (var (_, s) in list) {
        if (hasBreak(s, label, @implicit)) {
            return true;
        }
    }
    return false;
}

} // end types_package
