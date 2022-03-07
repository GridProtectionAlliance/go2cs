// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syntax -- go2cs converted at 2022 March 06 23:12:19 UTC
// import "cmd/compile/internal/syntax" ==> using syntax = go.cmd.compile.@internal.syntax_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\syntax\branches.go
using fmt = go.fmt_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class syntax_package {

    // TODO(gri) consider making this part of the parser code

    // checkBranches checks correct use of labels and branch
    // statements (break, continue, goto) in a function body.
    // It catches:
    //    - misplaced breaks and continues
    //    - bad labeled breaks and continues
    //    - invalid, unused, duplicate, and missing labels
    //    - gotos jumping over variable declarations and into blocks
private static void checkBranches(ptr<BlockStmt> _addr_body, ErrorHandler errh) {
    ref BlockStmt body = ref _addr_body.val;

    if (body == null) {
        return ;
    }
    ptr<labelScope> ls = addr(new labelScope(errh:errh));
    var fwdGotos = ls.blockBranches(null, new targets(), null, body.Pos(), body.List); 

    // If there are any forward gotos left, no matching label was
    // found for them. Either those labels were never defined, or
    // they are inside blocks and not reachable from the gotos.
    foreach (var (_, fwd) in fwdGotos) {
        var name = fwd.Label.Value;
        {
            var l__prev1 = l;

            var l = ls.labels[name];

            if (l != null) {
                l.used = true; // avoid "defined and not used" error
                ls.err(fwd.Label.Pos(), "goto %s jumps into block starting at %s", name, l.parent.start);

            }
            else
 {
                ls.err(fwd.Label.Pos(), "label %s not defined", name);
            }
            l = l__prev1;

        }

    }    {
        var l__prev1 = l;

        foreach (var (_, __l) in ls.labels) {
            l = __l;
            if (!l.used) {
                l = l.lstmt.Label;
                ls.err(l.Pos(), "label %s defined and not used", l.Value);
            }
        }
        l = l__prev1;
    }
}

private partial struct labelScope {
    public ErrorHandler errh;
    public map<@string, ptr<label>> labels; // all label declarations inside the function; allocated lazily
}

private partial struct label {
    public ptr<block> parent; // block containing this label declaration
    public ptr<LabeledStmt> lstmt; // statement declaring the label
    public bool used; // whether the label is used or not
}

private partial struct block {
    public ptr<block> parent; // immediately enclosing block, or nil
    public Pos start; // start of block
    public ptr<LabeledStmt> lstmt; // labeled statement associated with this block, or nil
}

private static void err(this ptr<labelScope> _addr_ls, Pos pos, @string format, params object[] args) {
    args = args.Clone();
    ref labelScope ls = ref _addr_ls.val;

    ls.errh(new Error(pos,fmt.Sprintf(format,args...)));
}

// declare declares the label introduced by s in block b and returns
// the new label. If the label was already declared, declare reports
// and error and the existing label is returned instead.
private static ptr<label> declare(this ptr<labelScope> _addr_ls, ptr<block> _addr_b, ptr<LabeledStmt> _addr_s) {
    ref labelScope ls = ref _addr_ls.val;
    ref block b = ref _addr_b.val;
    ref LabeledStmt s = ref _addr_s.val;

    var name = s.Label.Value;
    var labels = ls.labels;
    if (labels == null) {
        labels = make_map<@string, ptr<label>>();
        ls.labels = labels;
    }    {
        var alt = labels[name];


        else if (alt != null) {
            ls.err(s.Label.Pos(), "label %s already defined at %s", name, alt.lstmt.Label.Pos().String());
            return _addr_alt!;
        }
    }

    ptr<label> l = addr(new label(b,s,false));
    labels[name] = l;
    return _addr_l!;

}

// gotoTarget returns the labeled statement matching the given name and
// declared in block b or any of its enclosing blocks. The result is nil
// if the label is not defined, or doesn't match a valid labeled statement.
private static ptr<LabeledStmt> gotoTarget(this ptr<labelScope> _addr_ls, ptr<block> _addr_b, @string name) {
    ref labelScope ls = ref _addr_ls.val;
    ref block b = ref _addr_b.val;

    {
        var l = ls.labels[name];

        if (l != null) {
            l.used = true; // even if it's not a valid target
            while (b != null) {
                if (l.parent == b) {
                    return _addr_l.lstmt!;
                b = b.parent;
                }

            }


        }
    }

    return _addr_null!;

}

private static ptr<LabeledStmt> invalid = @new<LabeledStmt>(); // singleton to signal invalid enclosing target

// enclosingTarget returns the innermost enclosing labeled statement matching
// the given name. The result is nil if the label is not defined, and invalid
// if the label is defined but doesn't label a valid labeled statement.
private static ptr<LabeledStmt> enclosingTarget(this ptr<labelScope> _addr_ls, ptr<block> _addr_b, @string name) {
    ref labelScope ls = ref _addr_ls.val;
    ref block b = ref _addr_b.val;

    {
        var l = ls.labels[name];

        if (l != null) {
            l.used = true; // even if it's not a valid target (see e.g., test/fixedbugs/bug136.go)
            while (b != null) {
                if (l.lstmt == b.lstmt) {
                    return _addr_l.lstmt!;
                b = b.parent;
                }

            }

            return _addr_invalid!;

        }
    }

    return _addr_null!;

}

// targets describes the target statements within which break
// or continue statements are valid.
private partial struct targets {
    public Stmt breaks; // *ForStmt, *SwitchStmt, *SelectStmt, or nil
    public ptr<ForStmt> continues; // or nil
}

// blockBranches processes a block's body starting at start and returns the
// list of unresolved (forward) gotos. parent is the immediately enclosing
// block (or nil), ctxt provides information about the enclosing statements,
// and lstmt is the labeled statement associated with this block, or nil.
private static slice<ptr<BranchStmt>> blockBranches(this ptr<labelScope> _addr_ls, ptr<block> _addr_parent, targets ctxt, ptr<LabeledStmt> _addr_lstmt, Pos start, slice<Stmt> body) => func((_, panic, _) => {
    ref labelScope ls = ref _addr_ls.val;
    ref block parent = ref _addr_parent.val;
    ref LabeledStmt lstmt = ref _addr_lstmt.val;

    ptr<block> b = addr(new block(parent:parent,start:start,lstmt:lstmt));

    Pos varPos = default;
    Expr varName = default;
    slice<ptr<BranchStmt>> fwdGotos = default;    slice<ptr<BranchStmt>> badGotos = default;



    Action<Pos, Expr> recordVarDecl = (pos, name) => {
        varPos = pos;
        varName = name; 
        // Any existing forward goto jumping over the variable
        // declaration is invalid. The goto may still jump out
        // of the block and be ok, but we don't know that yet.
        // Remember all forward gotos as potential bad gotos.
        badGotos = append(badGotos[..(int)0], fwdGotos);

    };

    Func<ptr<BranchStmt>, bool> jumpsOverVarDecl = fwd => {
        if (varPos.IsKnown()) {
            foreach (var (_, bad) in badGotos) {
                if (fwd == bad) {
                    return true;
                }
            }
        }
        return false;

    };

    Action<targets, Pos, slice<Stmt>> innerBlock = (ctxt, start, body) => { 
        // Unresolved forward gotos from the inner block
        // become forward gotos for the current block.
        fwdGotos = append(fwdGotos, ls.blockBranches(b, ctxt, lstmt, start, body));

    };

    foreach (var (_, stmt) in body) {
        lstmt = null;
L:
        switch (stmt.type()) {
            case ptr<DeclStmt> s:
                foreach (var (_, d) in s.DeclList) {
                    {
                        ptr<VarDecl> (v, ok) = d._<ptr<VarDecl>>();

                        if (ok) {
                            recordVarDecl(v.Pos(), v.NameList[0]);
                            break; // the first VarDecl will do
                        }

                    }

                }
                break;
            case ptr<LabeledStmt> s:
                {
                    var name__prev1 = name;

                    var name = s.Label.Value;

                    if (name != "_") {
                        var l = ls.declare(b, s); 
                        // resolve matching forward gotos
                        nint i = 0;
                        foreach (var (_, fwd) in fwdGotos) {
                            if (fwd.Label.Value == name) {
                                fwd.Target = s;
                                l.used = true;
                                if (jumpsOverVarDecl(fwd)) {
                                    ls.err(fwd.Label.Pos(), "goto %s jumps over declaration of %s at %s", name, String(varName), varPos);
                                }
                            }
                            else
 { 
                                // no match - keep forward goto
                                fwdGotos[i] = fwd;
                                i++;

                            }

                        }
                        fwdGotos = fwdGotos[..(int)i];
                        lstmt = s;

                    } 
                    // process labeled statement

                    name = name__prev1;

                } 
                // process labeled statement
                stmt = s.Stmt;
                goto L;
                break;
            case ptr<BranchStmt> s:
                if (s.Label == null) {

                    if (s.Tok == _Break)
                    {
                        {
                            var t__prev2 = t;

                            var t = ctxt.breaks;

                            if (t != null) {
                                s.Target = t;
                            }
                            else
 {
                                ls.err(s.Pos(), "break is not in a loop, switch, or select");
                            }

                            t = t__prev2;

                        }

                        goto __switch_break0;
                    }
                    if (s.Tok == _Continue)
                    {
                        {
                            var t__prev2 = t;

                            t = ctxt.continues;

                            if (t != null) {
                                s.Target = t;
                            }
                            else
 {
                                ls.err(s.Pos(), "continue is not in a loop");
                            }

                            t = t__prev2;

                        }

                        goto __switch_break0;
                    }
                    if (s.Tok == _Fallthrough)
                    {
                        goto __switch_break0;
                    }
                    if (s.Tok == _Goto)
                    {
                    }
                    // default: 
                        panic("invalid BranchStmt");

                    __switch_break0:;
                    break;

                } 

                // labeled branch statement
                name = s.Label.Value;

                if (s.Tok == _Break) 
                {
                    // spec: "If there is a label, it must be that of an enclosing
                    // "for", "switch", or "select" statement, and that is the one
                    // whose execution terminates."
                    {
                        var t__prev1 = t;

                        t = ls.enclosingTarget(b, name);

                        if (t != null) {
                            switch (t.Stmt.type()) {
                                case ptr<SwitchStmt> t:
                                    s.Target = t;
                                    break;
                                case ptr<SelectStmt> t:
                                    s.Target = t;
                                    break;
                                case ptr<ForStmt> t:
                                    s.Target = t;
                                    break;
                                default:
                                {
                                    var t = t.Stmt.type();
                                    ls.err(s.Label.Pos(), "invalid break label %s", name);
                                    break;
                                }
                            }

                        }
                        else
 {
                            ls.err(s.Label.Pos(), "break label not defined: %s", name);
                        }

                        t = t__prev1;

                    }


                    goto __switch_break1;
                }
                if (s.Tok == _Continue) 
                {
                    // spec: "If there is a label, it must be that of an enclosing
                    // "for" statement, and that is the one whose execution advances."
                    {
                        var t__prev1 = t;

                        t = ls.enclosingTarget(b, name);

                        if (t != null) {
                            {
                                var t__prev2 = t;

                                ptr<ForStmt> (t, ok) = t.Stmt._<ptr<ForStmt>>();

                                if (ok) {
                                    s.Target = t;
                                }
                                else
 {
                                    ls.err(s.Label.Pos(), "invalid continue label %s", name);
                                }

                                t = t__prev2;

                            }

                        }
                        else
 {
                            ls.err(s.Label.Pos(), "continue label not defined: %s", name);
                        }

                        t = t__prev1;

                    }


                    goto __switch_break1;
                }
                if (s.Tok == _Goto)
                {
                    {
                        var t__prev1 = t;

                        t = ls.gotoTarget(b, name);

                        if (t != null) {
                            s.Target = t;
                        }
                        else
 { 
                            // label may be declared later - add goto to forward gotos
                            fwdGotos = append(fwdGotos, s);

                        }

                        t = t__prev1;

                    }


                    goto __switch_break1;
                }
                if (s.Tok == _Fallthrough)
                {
                }
                // default: 
                    panic("invalid BranchStmt");

                __switch_break1:;
                break;
            case ptr<AssignStmt> s:
                if (s.Op == Def) {
                    recordVarDecl(s.Pos(), s.Lhs);
                }
                break;
            case ptr<BlockStmt> s:
                innerBlock(ctxt, s.Pos(), s.List);
                break;
            case ptr<IfStmt> s:
                innerBlock(ctxt, s.Then.Pos(), s.Then.List);
                if (s.Else != null) {
                    innerBlock(ctxt, s.Else.Pos(), new slice<Stmt>(new Stmt[] { s.Else }));
                }
                break;
            case ptr<ForStmt> s:
                innerBlock(new targets(s,s), s.Body.Pos(), s.Body.List);
                break;
            case ptr<SwitchStmt> s:
                targets inner = new targets(s,ctxt.continues);
                {
                    var cc__prev2 = cc;

                    foreach (var (_, __cc) in s.Body) {
                        cc = __cc;
                        innerBlock(inner, cc.Pos(), cc.Body);
                    }

                    cc = cc__prev2;
                }
                break;
            case ptr<SelectStmt> s:
                inner = new targets(s,ctxt.continues);
                {
                    var cc__prev2 = cc;

                    foreach (var (_, __cc) in s.Body) {
                        cc = __cc;
                        innerBlock(inner, cc.Pos(), cc.Body);
                    }

                    cc = cc__prev2;
                }
                break;
        }

    }    return fwdGotos;

});

} // end syntax_package
