// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types2 -- go2cs converted at 2022 March 06 23:12:42 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\labels.go
using syntax = go.cmd.compile.@internal.syntax_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class types2_package {

    // labels checks correct label use in body.
private static void labels(this ptr<Checker> _addr_check, ptr<syntax.BlockStmt> _addr_body) {
    ref Checker check = ref _addr_check.val;
    ref syntax.BlockStmt body = ref _addr_body.val;
 
    // set of all labels in this body
    var all = NewScope(null, body.Pos(), syntax.EndPos(body), "label");

    var fwdJumps = check.blockBranches(all, null, null, body.List); 

    // If there are any forward jumps left, no label was found for
    // the corresponding goto statements. Either those labels were
    // never defined, or they are inside blocks and not reachable
    // for the respective gotos.
    foreach (var (_, jmp) in fwdJumps) {
        @string msg = default;
        var name = jmp.Label.Value;
        {
            var alt = all.Lookup(name);

            if (alt != null) {
                msg = "goto %s jumps into block";
                alt._<ptr<Label>>().used = true; // avoid another error
            }
            else
 {
                msg = "label %s not declared";
            }
        }

        check.errorf(jmp.Label, msg, name);

    }    foreach (var (_, obj) in all.elems) {
        {
            ptr<Label> lbl = obj._<ptr<Label>>();

            if (!lbl.used) {
                check.softErrorf(lbl.pos, "label %s declared but not used", lbl.name);
            }
        }

    }
}

// A block tracks label declarations in a block and its enclosing blocks.
private partial struct block {
    public ptr<block> parent; // enclosing block
    public ptr<syntax.LabeledStmt> lstmt; // labeled statement to which this block belongs, or nil
    public map<@string, ptr<syntax.LabeledStmt>> labels; // allocated lazily
}

// insert records a new label declaration for the current block.
// The label must not have been declared before in any block.
private static void insert(this ptr<block> _addr_b, ptr<syntax.LabeledStmt> _addr_s) {
    ref block b = ref _addr_b.val;
    ref syntax.LabeledStmt s = ref _addr_s.val;

    var name = s.Label.Value;
    if (debug) {
        assert(b.gotoTarget(name) == null);
    }
    var labels = b.labels;
    if (labels == null) {
        labels = make_map<@string, ptr<syntax.LabeledStmt>>();
        b.labels = labels;
    }
    labels[name] = s;

}

// gotoTarget returns the labeled statement in the current
// or an enclosing block with the given label name, or nil.
private static ptr<syntax.LabeledStmt> gotoTarget(this ptr<block> _addr_b, @string name) {
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
private static ptr<syntax.LabeledStmt> enclosingTarget(this ptr<block> _addr_b, @string name) {
    ref block b = ref _addr_b.val;

    {
        var s = b;

        while (s != null) {
            {
                var t = s.lstmt;

                if (t != null && t.Label.Value == name) {
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
private static slice<ptr<syntax.BranchStmt>> blockBranches(this ptr<Checker> _addr_check, ptr<Scope> _addr_all, ptr<block> _addr_parent, ptr<syntax.LabeledStmt> _addr_lstmt, slice<syntax.Stmt> list) {
    ref Checker check = ref _addr_check.val;
    ref Scope all = ref _addr_all.val;
    ref block parent = ref _addr_parent.val;
    ref syntax.LabeledStmt lstmt = ref _addr_lstmt.val;

    ptr<block> b = addr(new block(parent,lstmt,nil));

    syntax.Pos varDeclPos = default;    slice<ptr<syntax.BranchStmt>> fwdJumps = default;    slice<ptr<syntax.BranchStmt>> badJumps = default; 

    // All forward jumps jumping over a variable declaration are possibly
    // invalid (they may still jump out of the block and be ok).
    // recordVarDecl records them for the given position.
    Action<syntax.Pos> recordVarDecl = pos => {
        varDeclPos = pos;
        badJumps = append(badJumps[..(int)0], fwdJumps); // copy fwdJumps to badJumps
    };

    Func<ptr<syntax.BranchStmt>, bool> jumpsOverVarDecl = jmp => {
        if (varDeclPos.IsKnown()) {
            foreach (var (_, bad) in badJumps) {
                if (jmp == bad) {
                    return true;
                }
            }
        }
        return false;

    };

    Action<syntax.Stmt> stmtBranches = default;
    stmtBranches = s => {
        switch (s.type()) {
            case ptr<syntax.DeclStmt> s:
                {
                    var d__prev1 = d;

                    foreach (var (_, __d) in s.DeclList) {
                        d = __d;
                        {
                            var d__prev1 = d;

                            ptr<syntax.VarDecl> (d, _) = d._<ptr<syntax.VarDecl>>();

                            if (d != null) {
                                recordVarDecl(d.Pos());
                            }

                            d = d__prev1;

                        }

                    }

                    d = d__prev1;
                }
                break;
            case ptr<syntax.LabeledStmt> s:
                {
                    var name__prev1 = name;

                    var name = s.Label.Value;

                    if (name != "_") {
                        var lbl = NewLabel(s.Label.Pos(), check.pkg, name);
                        {
                            var alt = all.Insert(lbl);

                            if (alt != null) {
                                ref error_ err = ref heap(out ptr<error_> _addr_err);
                                err.soft = true;
                                err.errorf(lbl.pos, "label %s already declared", name);
                                err.recordAltDecl(alt);
                                check.report(_addr_err); 
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
                            if (jmp.Label.Value == name) { 
                                // match
                                lbl.used = true;
                                check.recordUse(jmp.Label, lbl);
                                if (jumpsOverVarDecl(jmp)) {
                                    check.softErrorf(jmp.Label, "goto %s jumps over variable declaration at line %d", name, varDeclPos.Line()); 
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
            case ptr<syntax.BranchStmt> s:
                if (s.Label == null) {
                    return ; // checked in 1st pass (check.stmt)
                } 

                // determine and validate target
                name = s.Label.Value;

                if (s.Tok == syntax.Break) 
                    // spec: "If there is a label, it must be that of an enclosing
                    // "for", "switch", or "select" statement, and that is the one
                    // whose execution terminates."
                    var valid = false;
                    {
                        var t__prev1 = t;

                        var t = b.enclosingTarget(name);

                        if (t != null) {
                            switch (t.Stmt.type()) {
                                case ptr<syntax.SwitchStmt> _:
                                    valid = true;
                                    break;
                                case ptr<syntax.SelectStmt> _:
                                    valid = true;
                                    break;
                                case ptr<syntax.ForStmt> _:
                                    valid = true;
                                    break;
                            }

                        }

                        t = t__prev1;

                    }

                    if (!valid) {
                        check.errorf(s.Label, "invalid break label %s", name);
                        return ;
                    }

                else if (s.Tok == syntax.Continue) 
                    // spec: "If there is a label, it must be that of an enclosing
                    // "for" statement, and that is the one whose execution advances."
                    valid = false;
                    {
                        var t__prev1 = t;

                        t = b.enclosingTarget(name);

                        if (t != null) {
                            switch (t.Stmt.type()) {
                                case ptr<syntax.ForStmt> _:
                                    valid = true;
                                    break;
                            }

                        }

                        t = t__prev1;

                    }

                    if (!valid) {
                        check.errorf(s.Label, "invalid continue label %s", name);
                        return ;
                    }

                else if (s.Tok == syntax.Goto) 
                    if (b.gotoTarget(name) == null) { 
                        // label may be declared later - add branch to forward jumps
                        fwdJumps = append(fwdJumps, s);
                        return ;

                    }

                else 
                    check.errorf(s, invalidAST + "branch statement: %s %s", s.Tok, name);
                    return ;
                // record label use
                var obj = all.Lookup(name);
                obj._<ptr<Label>>().used = true;
                check.recordUse(s.Label, obj);
                break;
            case ptr<syntax.AssignStmt> s:
                if (s.Op == syntax.Def) {
                    recordVarDecl(s.Pos());
                }
                break;
            case ptr<syntax.BlockStmt> s:
                fwdJumps = append(fwdJumps, check.blockBranches(all, b, lstmt, s.List));
                break;
            case ptr<syntax.IfStmt> s:
                stmtBranches(s.Then);
                if (s.Else != null) {
                    stmtBranches(s.Else);
                }
                break;
            case ptr<syntax.SwitchStmt> s:
                b = addr(new block(b,lstmt,nil));
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in s.Body) {
                        s = __s;
                        fwdJumps = append(fwdJumps, check.blockBranches(all, b, null, s.Body));
                    }

                    s = s__prev1;
                }
                break;
            case ptr<syntax.SelectStmt> s:
                b = addr(new block(b,lstmt,nil));
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in s.Body) {
                        s = __s;
                        fwdJumps = append(fwdJumps, check.blockBranches(all, b, null, s.Body));
                    }

                    s = s__prev1;
                }
                break;
            case ptr<syntax.ForStmt> s:
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

} // end types2_package
