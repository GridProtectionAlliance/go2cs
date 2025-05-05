// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using ast = go.ast_package;
using token = go.token_package;
using static @internal.types.errors_package;

partial class types_package {

// labels checks correct label use in body.
[GoRecv] public static void labels(this ref Checker check, ж<ast.BlockStmt> Ꮡbody) {
    ref var body = ref Ꮡbody.val;

    // set of all labels in this body
    var all = NewScope(nil, body.Pos(), body.End(), "label"u8);
    var fwdJumps = check.blockBranches(all, nil, nil, body.List);
    // If there are any forward jumps left, no label was found for
    // the corresponding goto statements. Either those labels were
    // never defined, or they are inside blocks and not reachable
    // for the respective gotos.
    foreach (var (_, jmp) in fwdJumps) {
        @string msg = default!;
        errors.Code code = default!;
        @string name = (~jmp).Label.val.Name;
        {
            var alt = all.Lookup(name); if (alt != default!){
                msg = "goto %s jumps into block"u8;
                code = JumpIntoBlock;
                alt._<Label.val>().used = true;
            } else {
                // avoid another error
                msg = "label %s not declared"u8;
                code = UndeclaredLabel;
            }
        }
        check.errorf(~(~jmp).Label, code, msg, name);
    }
    // spec: "It is illegal to define a label that is never used."
    foreach (var (name, obj) in (~all).elems) {
        obj = resolve(name, obj);
        {
            var lbl = obj._<Label.val>(); if (!(~lbl).used) {
                check.softErrorf(~lbl, UnusedLabel, "label %s declared and not used"u8, lbl.name);
            }
        }
    }
}

// A block tracks label declarations in a block and its enclosing blocks.
[GoType] partial struct block {
    internal ж<block> parent;                   // enclosing block
    internal ж<go.ast_package.LabeledStmt> lstmt;         // labeled statement to which this block belongs, or nil
    internal ast.LabeledStmt labels; // allocated lazily
}

// insert records a new label declaration for the current block.
// The label must not have been declared before in any block.
[GoRecv] internal static void insert(this ref block b, ж<ast.LabeledStmt> Ꮡs) {
    ref var s = ref Ꮡs.val;

    @string name = s.Label.Name;
    if (debug) {
        assert(b.gotoTarget(name) == nil);
    }
    var labels = b.labels;
    if (labels == default!) {
        labels = new ast.LabeledStmt();
        b.labels = labels;
    }
    labels[name] = s;
}

// gotoTarget returns the labeled statement in the current
// or an enclosing block with the given label name, or nil.
[GoRecv] internal static ж<ast.LabeledStmt> gotoTarget(this ref block b, @string name) {
    for (var s = b; s != nil; s = s.val.parent) {
        {
            var t = (~s).labels[name]; if (t != nil) {
                return t;
            }
        }
    }
    return default!;
}

// enclosingTarget returns the innermost enclosing labeled
// statement with the given label name, or nil.
[GoRecv] internal static ж<ast.LabeledStmt> enclosingTarget(this ref block b, @string name) {
    for (var s = b; s != nil; s = s.val.parent) {
        {
            var t = s.val.lstmt; if (t != nil && (~(~t).Label).Name == name) {
                return t;
            }
        }
    }
    return default!;
}

// blockBranches processes a block's statement list and returns the set of outgoing forward jumps.
// all is the scope of all declared labels, parent the set of labels declared in the immediately
// enclosing block, and lstmt is the labeled statement this block is associated with (or nil).
[GoRecv] public static slice<ast.BranchStmt> blockBranches(this ref Checker check, ж<ΔScope> Ꮡall, ж<block> Ꮡparent, ж<ast.LabeledStmt> Ꮡlstmt, slice<ast.Stmt> list) {
    ref var all = ref Ꮡall.val;
    ref var parent = ref Ꮡparent.val;
    ref var lstmt = ref Ꮡlstmt.val;

    var b = Ꮡ(new block(parent: parent, lstmt: lstmt));
    tokenꓸPos varDeclPos = default!;
    slice<ast.BranchStmt> fwdJumps = default!;
    slice<ast.BranchStmt> badJumps = default!;
    // All forward jumps jumping over a variable declaration are possibly
    // invalid (they may still jump out of the block and be ok).
    // recordVarDecl records them for the given position.
    var recordVarDecl = 
    var badJumpsʗ1 = badJumps;
    var fwdJumpsʗ1 = fwdJumps;
    (tokenꓸPos pos) => {
        varDeclPos = pos;
        badJumpsʗ1 = append(badJumpsʗ1[..0], ᏑfwdJumpsʗ1.ꓸꓸꓸ);
    };
    // copy fwdJumps to badJumps
    var jumpsOverVarDecl = 
    var badJumpsʗ2 = badJumps;
    (ж<ast.BranchStmt> jmp) => {
        if (varDeclPos.IsValid()) {
            foreach (var (_, bad) in badJumpsʗ2) {
                if (jmp == bad) {
                    return true;
                }
            }
        }
        return false;
    };
    var blockBranches = 
    var bʗ1 = b;
    var fwdJumpsʗ2 = fwdJumps;
    (ж<ast.LabeledStmt> lstmt, slice<ast.Stmt> list) => {
        // Unresolved forward jumps inside the nested block
        // become forward jumps in the current block.
        fwdJumpsʗ2 = append(fwdJumpsʗ2, check.blockBranches(Ꮡall, bʗ1, ᏑlstmtΔ1, listΔ1).ꓸꓸꓸ);
    };
    ast.Stmt) stmtBranches = default!;
    stmtBranches = 
    var bʗ2 = b;
    var blockBranchesʗ1 = blockBranches;
    var fwdJumpsʗ3 = fwdJumps;
    var jumpsOverVarDeclʗ1 = jumpsOverVarDecl;
    var recordVarDeclʗ1 = recordVarDecl;
    var stmtBranchesʗ1 = stmtBranches;
    (ast.Stmt s) => {
        switch (s.type()) {
        case ж<ast.DeclStmt> s: {
            {
                var (d, _) = (~s).Decl._<ж<ast.GenDecl>>(ᐧ); if (d != nil && (~d).Tok == token.VAR) {
                    recordVarDeclʗ1(d.Pos());
                }
            }
            break;
        }
        case ж<ast.LabeledStmt> s: {
            {
                @string name = (~s).Label.val.Name; if (name != "_"u8) {
                    // declare non-blank label
                    var lbl = NewLabel((~s).Label.Pos(), check.pkg, name);
                    {
                        var alt = all.Insert(~lbl); if (alt != default!){
                            var err = check.newError(DuplicateLabel);
                            err.val.soft = true;
                            err.addf(~lbl, "label %s already declared"u8, name);
                            err.addAltDecl(alt);
                            err.report();
                        } else {
                            // ok to continue
                            bʗ2.insert(s);
                            check.recordDef((~s).Label, ~lbl);
                        }
                    }
                    // resolve matching forward jumps and remove them from fwdJumps
                    nint i = 0;
                    foreach (var (_, jmp) in fwdJumpsʗ3) {
                        if ((~(~jmp).Label).Name == name){
                            // match
                            lbl.val.used = true;
                            check.recordUse((~jmp).Label, ~lbl);
                            if (jumpsOverVarDeclʗ1(jmp)) {
                                check.softErrorf(
                                    ~(~jmp).Label,
                                    JumpOverDecl,
                                    "goto %s jumps over variable declaration at line %d"u8,
                                    name,
                                    check.fset.Position(varDeclPos).Line);
                            }
                        } else {
                            // ok to continue
                            // no match - record new forward jump
                            fwdJumpsʗ3[i] = jmp;
                            i++;
                        }
                    }
                    fwdJumpsʗ3 = fwdJumpsʗ3[..(int)(i)];
                    lstmt = s;
                }
            }
            stmtBranchesʗ1((~s).Stmt);
            break;
        }
        case ж<ast.BranchStmt> s: {
            if ((~s).Label == nil) {
                return;
            }
            @string name = (~s).Label.val.Name;
            var exprᴛ1 = (~s).Tok;
            if (exprᴛ1 == token.BREAK) {
                var valid = false;
                {
                    var t = bʗ2.enclosingTarget(name); if (t != nil) {
                        // checked in 1st pass (check.stmt)
                        // determine and validate target
                        // spec: "If there is a label, it must be that of an enclosing
                        // "for", "switch", or "select" statement, and that is the one
                        // whose execution terminates."
                        switch ((~t).Stmt.type()) {
                        case ж<ast.SwitchStmt> : {
                            valid = true;
                            break;
                        }
                        case ж<ast.TypeSwitchStmt> : {
                            valid = true;
                            break;
                        }
                        case ж<ast.SelectStmt> : {
                            valid = true;
                            break;
                        }
                        case ж<ast.ForStmt> : {
                            valid = true;
                            break;
                        }
                        case ж<ast.RangeStmt> : {
                            valid = true;
                            break;
                        }}

                    }
                }
                if (!valid) {
                    check.errorf(~(~s).Label, MisplacedLabel, "invalid break label %s"u8, name);
                    return;
                }
            }
            if (exprᴛ1 == token.CONTINUE) {
                var valid = false;
                {
                    var t = bʗ2.enclosingTarget(name); if (t != nil) {
                        // spec: "If there is a label, it must be that of an enclosing
                        // "for" statement, and that is the one whose execution advances."
                        switch ((~t).Stmt.type()) {
                        case ж<ast.ForStmt> : {
                            valid = true;
                            break;
                        }
                        case ж<ast.RangeStmt> : {
                            valid = true;
                            break;
                        }}

                    }
                }
                if (!valid) {
                    check.errorf(~(~s).Label, MisplacedLabel, "invalid continue label %s"u8, name);
                    return;
                }
            }
            if (exprᴛ1 == token.GOTO) {
                if (bʗ2.gotoTarget(name) == nil) {
                    // label may be declared later - add branch to forward jumps
                    fwdJumpsʗ3 = append(fwdJumpsʗ3, s);
                    return;
                }
            }
            { /* default: */
                check.errorf(~s, InvalidSyntaxTree, "branch statement: %s %s"u8, (~s).Tok, name);
                return;
            }

            var obj = all.Lookup(name);
            obj._<Label.val>().used = true;
            check.recordUse((~s).Label, // record label use
 obj);
            break;
        }
        case ж<ast.AssignStmt> s: {
            if ((~s).Tok == token.DEFINE) {
                recordVarDeclʗ1(s.Pos());
            }
            break;
        }
        case ж<ast.BlockStmt> s: {
            blockBranchesʗ1(Ꮡlstmt, (~s).List);
            break;
        }
        case ж<ast.IfStmt> s: {
            stmtBranchesʗ1(~(~s).Body);
            if ((~s).Else != default!) {
                stmtBranchesʗ1((~s).Else);
            }
            break;
        }
        case ж<ast.CaseClause> s: {
            blockBranchesʗ1(nil, (~s).Body);
            break;
        }
        case ж<ast.SwitchStmt> s: {
            stmtBranchesʗ1(~(~s).Body);
            break;
        }
        case ж<ast.TypeSwitchStmt> s: {
            stmtBranchesʗ1(~(~s).Body);
            break;
        }
        case ж<ast.CommClause> s: {
            blockBranchesʗ1(nil, (~s).Body);
            break;
        }
        case ж<ast.SelectStmt> s: {
            stmtBranchesʗ1(~(~s).Body);
            break;
        }
        case ж<ast.ForStmt> s: {
            stmtBranchesʗ1(~(~s).Body);
            break;
        }
        case ж<ast.RangeStmt> s: {
            stmtBranchesʗ1(~(~s).Body);
            break;
        }}
    };
    foreach (var (_, s) in list) {
        stmtBranches(s);
    }
    return fwdJumps;
}

} // end types_package
