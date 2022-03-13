// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2022 March 13 05:53:11 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Program Files\Go\src\go\types\labels.go
namespace go.go;

using ast = go.ast_package;
using token = go.token_package;


// labels checks correct label use in body.

using System;
public static partial class types_package {

private static void labels(this ptr<Checker> _addr_check, ptr<ast.BlockStmt> _addr_body) {
    ref Checker check = ref _addr_check.val;
    ref ast.BlockStmt body = ref _addr_body.val;
 
    // set of all labels in this body
    var all = NewScope(null, body.Pos(), body.End(), "label");

    var fwdJumps = check.blockBranches(all, null, null, body.List); 

    // If there are any forward jumps left, no label was found for
    // the corresponding goto statements. Either those labels were
    // never defined, or they are inside blocks and not reachable
    // for the respective gotos.
    foreach (var (_, jmp) in fwdJumps) {
        @string msg = default;
        errorCode code = default;
        var name = jmp.Label.Name;
        {
            var alt = all.Lookup(name);

            if (alt != null) {
                msg = "goto %s jumps into block";
                alt._<ptr<Label>>().used = true; // avoid another error
                code = _JumpIntoBlock;
            }
            else
 {
                msg = "label %s not declared";
                code = _UndeclaredLabel;
            }
        }
        check.errorf(jmp.Label, code, msg, name);
    }    foreach (var (_, obj) in all.elems) {
        {
            ptr<Label> lbl = obj._<ptr<Label>>();

            if (!lbl.used) {
                check.softErrorf(lbl, _UnusedLabel, "label %s declared but not used", lbl.name);
            }
        }
    }
}

// A block tracks label declarations in a block and its enclosing blocks.
private partial struct block {
    public ptr<block> parent; // enclosing block
    public ptr<ast.LabeledStmt> lstmt; // labeled statement to which this block belongs, or nil
    public map<@string, ptr<ast.LabeledStmt>> labels; // allocated lazily
}

// insert records a new label declaration for the current block.
// The label must not have been declared before in any block.
private static void insert(this ptr<block> _addr_b, ptr<ast.LabeledStmt> _addr_s) {
    ref block b = ref _addr_b.val;
    ref ast.LabeledStmt s = ref _addr_s.val;

    var name = s.Label.Name;
    if (debug) {
        assert(b.gotoTarget(name) == null);
    }
    var labels = b.labels;
    if (labels == null) {
        labels = make_map<@string, ptr<ast.LabeledStmt>>();
        b.labels = labels;
    }
    labels[name] = s;
}

// gotoTarget returns the labeled statement in the current
// or an enclosing block with the given label name, or nil.
private static ptr<ast.LabeledStmt> gotoTarget(this ptr<block> _addr_b, @string name) {
    ref block b = ref _addr_b.val;

    {
        var s = b;

        while (s != null) {
            {
                var t = s.labels[name];

                if (t != null) {
                    return _addr_t!;
            s = s.parent;
                }

            }
        }
    }
    return _addr_null!;
}

// enclosingTarget returns the innermost enclosing labeled
// statement with the given label name, or nil.
private static ptr<ast.LabeledStmt> enclosingTarget(this ptr<block> _addr_b, @string name) {
    ref block b = ref _addr_b.val;

    {
        var s = b;

        while (s != null) {
            {
                var t = s.lstmt;

                if (t != null && t.Label.Name == name) {
                    return _addr_t!;
            s = s.parent;
                }

            }
        }
    }
    return _addr_null!;
}

// blockBranches processes a block's statement list and returns the set of outgoing forward jumps.
// all is the scope of all declared labels, parent the set of labels declared in the immediately
// enclosing block, and lstmt is the labeled statement this block is associated with (or nil).
private static slice<ptr<ast.BranchStmt>> blockBranches(this ptr<Checker> _addr_check, ptr<Scope> _addr_all, ptr<block> _addr_parent, ptr<ast.LabeledStmt> _addr_lstmt, slice<ast.Stmt> list) {
    ref Checker check = ref _addr_check.val;
    ref Scope all = ref _addr_all.val;
    ref block parent = ref _addr_parent.val;
    ref ast.LabeledStmt lstmt = ref _addr_lstmt.val;

    ptr<block> b = addr(new block(parent:parent,lstmt:lstmt));

    token.Pos varDeclPos = default;    slice<ptr<ast.BranchStmt>> fwdJumps = default;    slice<ptr<ast.BranchStmt>> badJumps = default; 

    // All forward jumps jumping over a variable declaration are possibly
    // invalid (they may still jump out of the block and be ok).
    // recordVarDecl records them for the given position.
    Action<token.Pos> recordVarDecl = pos => {
        varDeclPos = pos;
        badJumps = append(badJumps[..(int)0], fwdJumps); // copy fwdJumps to badJumps
    };

    Func<ptr<ast.BranchStmt>, bool> jumpsOverVarDecl = jmp => {
        if (varDeclPos.IsValid()) {
            foreach (var (_, bad) in badJumps) {
                if (jmp == bad) {
                    return true;
                }
            }
        }
        return false;
    };

    Action<ptr<ast.LabeledStmt>, slice<ast.Stmt>> blockBranches = (lstmt, list) => { 
        // Unresolved forward jumps inside the nested block
        // become forward jumps in the current block.
        fwdJumps = append(fwdJumps, check.blockBranches(all, b, lstmt, list));
    };

    Action<ast.Stmt> stmtBranches = default;
    stmtBranches = s => {
        switch (s.type()) {
            case ptr<ast.DeclStmt> s:
                {
                    ptr<ast.GenDecl> (d, _) = s.Decl._<ptr<ast.GenDecl>>();

                    if (d != null && d.Tok == token.VAR) {
                        recordVarDecl(d.Pos());
                    }

                }
                break;
            case ptr<ast.LabeledStmt> s:
                {
                    var name__prev1 = name;

                    var name = s.Label.Name;

                    if (name != "_") {
                        var lbl = NewLabel(s.Label.Pos(), check.pkg, name);
                        {
                            var alt = all.Insert(lbl);

                            if (alt != null) {
                                check.softErrorf(lbl, _DuplicateLabel, "label %s already declared", name);
                                check.reportAltDecl(alt); 
                                // ok to continue
                            }
                            else
 {
                                b.insert(s);
                                check.recordDef(s.Label, lbl);
                            } 
                            // resolve matching forward jumps and remove them from fwdJumps

                        } 
                        // resolve matching forward jumps and remove them from fwdJumps
                        nint i = 0;
                        foreach (var (_, jmp) in fwdJumps) {
                            if (jmp.Label.Name == name) { 
                                // match
                                lbl.used = true;
                                check.recordUse(jmp.Label, lbl);
                                if (jumpsOverVarDecl(jmp)) {
                                    check.softErrorf(jmp.Label, _JumpOverDecl, "goto %s jumps over variable declaration at line %d", name, check.fset.Position(varDeclPos).Line); 
                                    // ok to continue
                                }
                            }
                            else
 { 
                                // no match - record new forward jump
                                fwdJumps[i] = jmp;
                                i++;
                            }
                        }
                        fwdJumps = fwdJumps[..(int)i];
                        lstmt = s;
                    }

                    name = name__prev1;

                }
                stmtBranches(s.Stmt);
                break;
            case ptr<ast.BranchStmt> s:
                if (s.Label == null) {
                    return ; // checked in 1st pass (check.stmt)
                } 

                // determine and validate target
                name = s.Label.Name;

                if (s.Tok == token.BREAK) 
                    // spec: "If there is a label, it must be that of an enclosing
                    // "for", "switch", or "select" statement, and that is the one
                    // whose execution terminates."
                    var valid = false;
                    {
                        var t__prev1 = t;

                        var t = b.enclosingTarget(name);

                        if (t != null) {
                            switch (t.Stmt.type()) {
                                case ptr<ast.SwitchStmt> _:
                                    valid = true;
                                    break;
                                case ptr<ast.TypeSwitchStmt> _:
                                    valid = true;
                                    break;
                                case ptr<ast.SelectStmt> _:
                                    valid = true;
                                    break;
                                case ptr<ast.ForStmt> _:
                                    valid = true;
                                    break;
                                case ptr<ast.RangeStmt> _:
                                    valid = true;
                                    break;
                            }
                        }

                        t = t__prev1;

                    }
                    if (!valid) {
                        check.errorf(s.Label, _MisplacedLabel, "invalid break label %s", name);
                        return ;
                    }
                else if (s.Tok == token.CONTINUE) 
                    // spec: "If there is a label, it must be that of an enclosing
                    // "for" statement, and that is the one whose execution advances."
                    valid = false;
                    {
                        var t__prev1 = t;

                        t = b.enclosingTarget(name);

                        if (t != null) {
                            switch (t.Stmt.type()) {
                                case ptr<ast.ForStmt> _:
                                    valid = true;
                                    break;
                                case ptr<ast.RangeStmt> _:
                                    valid = true;
                                    break;
                            }
                        }

                        t = t__prev1;

                    }
                    if (!valid) {
                        check.errorf(s.Label, _MisplacedLabel, "invalid continue label %s", name);
                        return ;
                    }
                else if (s.Tok == token.GOTO) 
                    if (b.gotoTarget(name) == null) { 
                        // label may be declared later - add branch to forward jumps
                        fwdJumps = append(fwdJumps, s);
                        return ;
                    }
                else 
                    check.invalidAST(s, "branch statement: %s %s", s.Tok, name);
                    return ;
                // record label use
                var obj = all.Lookup(name);
                obj._<ptr<Label>>().used = true;
                check.recordUse(s.Label, obj);
                break;
            case ptr<ast.AssignStmt> s:
                if (s.Tok == token.DEFINE) {
                    recordVarDecl(s.Pos());
                }
                break;
            case ptr<ast.BlockStmt> s:
                blockBranches(lstmt, s.List);
                break;
            case ptr<ast.IfStmt> s:
                stmtBranches(s.Body);
                if (s.Else != null) {
                    stmtBranches(s.Else);
                }
                break;
            case ptr<ast.CaseClause> s:
                blockBranches(null, s.Body);
                break;
            case ptr<ast.SwitchStmt> s:
                stmtBranches(s.Body);
                break;
            case ptr<ast.TypeSwitchStmt> s:
                stmtBranches(s.Body);
                break;
            case ptr<ast.CommClause> s:
                blockBranches(null, s.Body);
                break;
            case ptr<ast.SelectStmt> s:
                stmtBranches(s.Body);
                break;
            case ptr<ast.ForStmt> s:
                stmtBranches(s.Body);
                break;
            case ptr<ast.RangeStmt> s:
                stmtBranches(s.Body);
                break;
        }
    };

    {
        var s__prev1 = s;

        foreach (var (_, __s) in list) {
            s = __s;
            stmtBranches(s);
        }
        s = s__prev1;
    }

    return fwdJumps;
}

} // end types_package
