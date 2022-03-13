// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cfg -- go2cs converted at 2022 March 13 06:42:39 UTC
// import "cmd/vendor/golang.org/x/tools/go/cfg" ==> using cfg = go.cmd.vendor.golang.org.x.tools.go.cfg_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\cfg\builder.go
namespace go.cmd.vendor.golang.org.x.tools.go;
// This file implements the CFG construction pass.


using fmt = fmt_package;
using ast = go.ast_package;
using token = go.token_package;
using System;

public static partial class cfg_package {

private partial struct builder {
    public ptr<CFG> cfg;
    public Func<ptr<ast.CallExpr>, bool> mayReturn;
    public ptr<Block> current;
    public map<ptr<ast.Object>, ptr<lblock>> lblocks; // labeled blocks
    public ptr<targets> targets; // linked stack of branch targets
}

private static void stmt(this ptr<builder> _addr_b, ast.Stmt _s) => func((_, panic, _) => {
    ref builder b = ref _addr_b.val;
 
    // The label of the current statement.  If non-nil, its _goto
    // target is always set; its _break and _continue are set only
    // within the body of switch/typeswitch/select/for/range.
    // It is effectively an additional default-nil parameter of stmt().
    ptr<lblock> label;
start:
    switch (_s.type()) {
        case ptr<ast.BadStmt> s:
            b.add(s);
            break;
        case ptr<ast.SendStmt> s:
            b.add(s);
            break;
        case ptr<ast.IncDecStmt> s:
            b.add(s);
            break;
        case ptr<ast.GoStmt> s:
            b.add(s);
            break;
        case ptr<ast.DeferStmt> s:
            b.add(s);
            break;
        case ptr<ast.EmptyStmt> s:
            b.add(s);
            break;
        case ptr<ast.AssignStmt> s:
            b.add(s);
            break;
        case ptr<ast.ExprStmt> s:
            b.add(s);
            {
                ptr<ast.CallExpr> (call, ok) = s.X._<ptr<ast.CallExpr>>();

                if (ok && !b.mayReturn(call)) { 
                    // Calls to panic, os.Exit, etc, never return.
                    b.current = b.newBlock("unreachable.call");
                }

            }
            break;
        case ptr<ast.DeclStmt> s:
            ptr<ast.GenDecl> d = s.Decl._<ptr<ast.GenDecl>>();
            if (d.Tok == token.VAR) {
                {
                    var spec__prev1 = spec;

                    foreach (var (_, __spec) in d.Specs) {
                        spec = __spec;
                        {
                            var spec__prev2 = spec;

                            ptr<ast.ValueSpec> (spec, ok) = spec._<ptr<ast.ValueSpec>>();

                            if (ok) {
                                b.add(spec);
                            }

                            spec = spec__prev2;

                        }
                    }

                    spec = spec__prev1;
                }
            }
            break;
        case ptr<ast.LabeledStmt> s:
            label = b.labeledBlock(s.Label);
            b.jump(label._goto);
            b.current = label._goto;
            _s = s.Stmt;
            goto start; // effectively: tailcall stmt(g, s.Stmt, label)
            break;
        case ptr<ast.ReturnStmt> s:
            b.add(s);
            b.current = b.newBlock("unreachable.return");
            break;
        case ptr<ast.BranchStmt> s:
            b.branchStmt(s);
            break;
        case ptr<ast.BlockStmt> s:
            b.stmtList(s.List);
            break;
        case ptr<ast.IfStmt> s:
            if (s.Init != null) {
                b.stmt(s.Init);
            }
            var then = b.newBlock("if.then");
            var done = b.newBlock("if.done");
            var _else = done;
            if (s.Else != null) {
                _else = b.newBlock("if.else");
            }
            b.add(s.Cond);
            b.ifelse(then, _else);
            b.current = then;
            b.stmt(s.Body);
            b.jump(done);

            if (s.Else != null) {
                b.current = _else;
                b.stmt(s.Else);
                b.jump(done);
            }
            b.current = done;
            break;
        case ptr<ast.SwitchStmt> s:
            b.switchStmt(s, label);
            break;
        case ptr<ast.TypeSwitchStmt> s:
            b.typeSwitchStmt(s, label);
            break;
        case ptr<ast.SelectStmt> s:
            b.selectStmt(s, label);
            break;
        case ptr<ast.ForStmt> s:
            b.forStmt(s, label);
            break;
        case ptr<ast.RangeStmt> s:
            b.rangeStmt(s, label);
            break;
        default:
        {
            var s = _s.type();
            panic(fmt.Sprintf("unexpected statement kind: %T", s));
            break;
        }
    }
});

private static void stmtList(this ptr<builder> _addr_b, slice<ast.Stmt> list) {
    ref builder b = ref _addr_b.val;

    foreach (var (_, s) in list) {
        b.stmt(s);
    }
}

private static void branchStmt(this ptr<builder> _addr_b, ptr<ast.BranchStmt> _addr_s) {
    ref builder b = ref _addr_b.val;
    ref ast.BranchStmt s = ref _addr_s.val;

    ptr<Block> block;

    if (s.Tok == token.BREAK) 
        if (s.Label != null) {
            {
                var lb__prev2 = lb;

                var lb = b.labeledBlock(s.Label);

                if (lb != null) {
                    block = lb._break;
                }

                lb = lb__prev2;

            }
        }
        else
 {
            {
                var t__prev1 = t;

                var t = b.targets;

                while (t != null && block == null) {
                    block = t._break;
                    t = t.tail;
                }


                t = t__prev1;
            }
        }
    else if (s.Tok == token.CONTINUE) 
        if (s.Label != null) {
            {
                var lb__prev2 = lb;

                lb = b.labeledBlock(s.Label);

                if (lb != null) {
                    block = lb._continue;
                }

                lb = lb__prev2;

            }
        }
        else
 {
            {
                var t__prev1 = t;

                t = b.targets;

                while (t != null && block == null) {
                    block = t._continue;
                    t = t.tail;
                }


                t = t__prev1;
            }
        }
    else if (s.Tok == token.FALLTHROUGH) 
        {
            var t__prev1 = t;

            t = b.targets;

            while (t != null && block == null) {
                block = t._fallthrough;
                t = t.tail;
            }


            t = t__prev1;
        }
    else if (s.Tok == token.GOTO) 
        if (s.Label != null) {
            block = b.labeledBlock(s.Label)._goto;
        }
        if (block == null) {
        block = b.newBlock("undefined.branch");
    }
    b.jump(block);
    b.current = b.newBlock("unreachable.branch");
}

private static void switchStmt(this ptr<builder> _addr_b, ptr<ast.SwitchStmt> _addr_s, ptr<lblock> _addr_label) {
    ref builder b = ref _addr_b.val;
    ref ast.SwitchStmt s = ref _addr_s.val;
    ref lblock label = ref _addr_label.val;

    if (s.Init != null) {
        b.stmt(s.Init);
    }
    if (s.Tag != null) {
        b.add(s.Tag);
    }
    var done = b.newBlock("switch.done");
    if (label != null) {
        label._break = done;
    }
    ptr<slice<ast.Stmt>> defaultBody;
    ptr<Block> defaultFallthrough;
    ptr<Block> fallthru;    ptr<Block> defaultBlock;

    var ncases = len(s.Body.List);
    foreach (var (i, clause) in s.Body.List) {
        var body = fallthru;
        if (body == null) {
            body = b.newBlock("switch.body"); // first case only
        }
        fallthru = done;
        if (i + 1 < ncases) {
            fallthru = b.newBlock("switch.body");
        }
        ptr<ast.CaseClause> cc = clause._<ptr<ast.CaseClause>>();
        if (cc.List == null) { 
            // Default case.
            defaultBody = _addr_cc.Body;
            defaultFallthrough = addr(fallthru);
            defaultBlock = body;
            continue;
        }
        ptr<Block> nextCond;
        foreach (var (_, cond) in cc.List) {
            nextCond = b.newBlock("switch.next");
            b.add(cond); // one half of the tag==cond condition
            b.ifelse(body, nextCond);
            b.current = nextCond;
        }        b.current = body;
        b.targets = addr(new targets(tail:b.targets,_break:done,_fallthrough:fallthru,));
        b.stmtList(cc.Body);
        b.targets = b.targets.tail;
        b.jump(done);
        b.current = nextCond;
    }    if (defaultBlock != null) {
        b.jump(defaultBlock);
        b.current = defaultBlock;
        b.targets = addr(new targets(tail:b.targets,_break:done,_fallthrough:defaultFallthrough,));
        b.stmtList(defaultBody.val);
        b.targets = b.targets.tail;
    }
    b.jump(done);
    b.current = done;
}

private static void typeSwitchStmt(this ptr<builder> _addr_b, ptr<ast.TypeSwitchStmt> _addr_s, ptr<lblock> _addr_label) {
    ref builder b = ref _addr_b.val;
    ref ast.TypeSwitchStmt s = ref _addr_s.val;
    ref lblock label = ref _addr_label.val;

    if (s.Init != null) {
        b.stmt(s.Init);
    }
    if (s.Assign != null) {
        b.add(s.Assign);
    }
    var done = b.newBlock("typeswitch.done");
    if (label != null) {
        label._break = done;
    }
    ptr<ast.CaseClause> default_;
    foreach (var (_, clause) in s.Body.List) {
        ptr<ast.CaseClause> cc = clause._<ptr<ast.CaseClause>>();
        if (cc.List == null) {
            default_ = addr(cc);
            continue;
        }
        var body = b.newBlock("typeswitch.body");
        ptr<Block> next;
        foreach (var (_, casetype) in cc.List) {
            next = b.newBlock("typeswitch.next"); 
            // casetype is a type, so don't call b.add(casetype).
            // This block logically contains a type assertion,
            // x.(casetype), but it's unclear how to represent x.
            _ = casetype;
            b.ifelse(body, next);
            b.current = next;
        }        b.current = body;
        b.typeCaseBody(cc, done);
        b.current = next;
    }    if (default_ != null) {
        b.typeCaseBody(default_, done);
    }
    else
 {
        b.jump(done);
    }
    b.current = done;
}

private static void typeCaseBody(this ptr<builder> _addr_b, ptr<ast.CaseClause> _addr_cc, ptr<Block> _addr_done) {
    ref builder b = ref _addr_b.val;
    ref ast.CaseClause cc = ref _addr_cc.val;
    ref Block done = ref _addr_done.val;

    b.targets = addr(new targets(tail:b.targets,_break:done,));
    b.stmtList(cc.Body);
    b.targets = b.targets.tail;
    b.jump(done);
}

private static void selectStmt(this ptr<builder> _addr_b, ptr<ast.SelectStmt> _addr_s, ptr<lblock> _addr_label) {
    ref builder b = ref _addr_b.val;
    ref ast.SelectStmt s = ref _addr_s.val;
    ref lblock label = ref _addr_label.val;
 
    // First evaluate channel expressions.
    // TODO(adonovan): fix: evaluate only channel exprs here.
    {
        var clause__prev1 = clause;

        foreach (var (_, __clause) in s.Body.List) {
            clause = __clause;
            {
                ptr<ast.CommClause> comm__prev1 = comm;

                ptr<ast.CommClause> comm = clause._<ptr<ast.CommClause>>().Comm;

                if (comm != null) {
                    b.stmt(comm);
                }

                comm = comm__prev1;

            }
        }
        clause = clause__prev1;
    }

    var done = b.newBlock("select.done");
    if (label != null) {
        label._break = done;
    }
    ptr<slice<ast.Stmt>> defaultBody;
    foreach (var (_, cc) in s.Body.List) {
        ptr<ast.CommClause> clause = cc._<ptr<ast.CommClause>>();
        if (clause.Comm == null) {
            defaultBody = _addr_clause.Body;
            continue;
        }
        var body = b.newBlock("select.body");
        var next = b.newBlock("select.next");
        b.ifelse(body, next);
        b.current = body;
        b.targets = addr(new targets(tail:b.targets,_break:done,));
        switch (clause.Comm.type()) {
            case ptr<ast.ExprStmt> comm:
                break;
            case ptr<ast.AssignStmt> comm:
                b.add(comm.Lhs[0]);
                break;
        }
        b.stmtList(clause.Body);
        b.targets = b.targets.tail;
        b.jump(done);
        b.current = next;
    }    if (defaultBody != null) {
        b.targets = addr(new targets(tail:b.targets,_break:done,));
        b.stmtList(defaultBody.val);
        b.targets = b.targets.tail;
        b.jump(done);
    }
    b.current = done;
}

private static void forStmt(this ptr<builder> _addr_b, ptr<ast.ForStmt> _addr_s, ptr<lblock> _addr_label) {
    ref builder b = ref _addr_b.val;
    ref ast.ForStmt s = ref _addr_s.val;
    ref lblock label = ref _addr_label.val;
 
    //    ...init...
    //      jump loop
    // loop:
    //      if cond goto body else done
    // body:
    //      ...body...
    //      jump post
    // post:                 (target of continue)
    //      ...post...
    //      jump loop
    // done:                                 (target of break)
    if (s.Init != null) {
        b.stmt(s.Init);
    }
    var body = b.newBlock("for.body");
    var done = b.newBlock("for.done"); // target of 'break'
    var loop = body; // target of back-edge
    if (s.Cond != null) {
        loop = b.newBlock("for.loop");
    }
    var cont = loop; // target of 'continue'
    if (s.Post != null) {
        cont = b.newBlock("for.post");
    }
    if (label != null) {
        label._break = done;
        label._continue = cont;
    }
    b.jump(loop);
    b.current = loop;
    if (loop != body) {
        b.add(s.Cond);
        b.ifelse(body, done);
        b.current = body;
    }
    b.targets = addr(new targets(tail:b.targets,_break:done,_continue:cont,));
    b.stmt(s.Body);
    b.targets = b.targets.tail;
    b.jump(cont);

    if (s.Post != null) {
        b.current = cont;
        b.stmt(s.Post);
        b.jump(loop); // back-edge
    }
    b.current = done;
}

private static void rangeStmt(this ptr<builder> _addr_b, ptr<ast.RangeStmt> _addr_s, ptr<lblock> _addr_label) {
    ref builder b = ref _addr_b.val;
    ref ast.RangeStmt s = ref _addr_s.val;
    ref lblock label = ref _addr_label.val;

    b.add(s.X);

    if (s.Key != null) {
        b.add(s.Key);
    }
    if (s.Value != null) {
        b.add(s.Value);
    }
    var loop = b.newBlock("range.loop");
    b.jump(loop);
    b.current = loop;

    var body = b.newBlock("range.body");
    var done = b.newBlock("range.done");
    b.ifelse(body, done);
    b.current = body;

    if (label != null) {
        label._break = done;
        label._continue = loop;
    }
    b.targets = addr(new targets(tail:b.targets,_break:done,_continue:loop,));
    b.stmt(s.Body);
    b.targets = b.targets.tail;
    b.jump(loop); // back-edge
    b.current = done;
}

// -------- helpers --------

// Destinations associated with unlabeled for/switch/select stmts.
// We push/pop one of these as we enter/leave each construct and for
// each BranchStmt we scan for the innermost target of the right type.
//
private partial struct targets {
    public ptr<targets> tail; // rest of stack
    public ptr<Block> _break;
    public ptr<Block> _continue;
    public ptr<Block> _fallthrough;
}

// Destinations associated with a labeled block.
// We populate these as labels are encountered in forward gotos or
// labeled statements.
//
private partial struct lblock {
    public ptr<Block> _goto;
    public ptr<Block> _break;
    public ptr<Block> _continue;
}

// labeledBlock returns the branch target associated with the
// specified label, creating it if needed.
//
private static ptr<lblock> labeledBlock(this ptr<builder> _addr_b, ptr<ast.Ident> _addr_label) {
    ref builder b = ref _addr_b.val;
    ref ast.Ident label = ref _addr_label.val;

    var lb = b.lblocks[label.Obj];
    if (lb == null) {
        lb = addr(new lblock(_goto:b.newBlock(label.Name)));
        if (b.lblocks == null) {
            b.lblocks = make_map<ptr<ast.Object>, ptr<lblock>>();
        }
        b.lblocks[label.Obj] = lb;
    }
    return _addr_lb!;
}

// newBlock appends a new unconnected basic block to b.cfg's block
// slice and returns it.
// It does not automatically become the current block.
// comment is an optional string for more readable debugging output.
private static ptr<Block> newBlock(this ptr<builder> _addr_b, @string comment) {
    ref builder b = ref _addr_b.val;

    var g = b.cfg;
    ptr<Block> block = addr(new Block(Index:int32(len(g.Blocks)),comment:comment,));
    block.Succs = block.succs2[..(int)0];
    g.Blocks = append(g.Blocks, block);
    return _addr_block!;
}

private static void add(this ptr<builder> _addr_b, ast.Node n) {
    ref builder b = ref _addr_b.val;

    b.current.Nodes = append(b.current.Nodes, n);
}

// jump adds an edge from the current block to the target block,
// and sets b.current to nil.
private static void jump(this ptr<builder> _addr_b, ptr<Block> _addr_target) {
    ref builder b = ref _addr_b.val;
    ref Block target = ref _addr_target.val;

    b.current.Succs = append(b.current.Succs, target);
    b.current = null;
}

// ifelse emits edges from the current block to the t and f blocks,
// and sets b.current to nil.
private static void ifelse(this ptr<builder> _addr_b, ptr<Block> _addr_t, ptr<Block> _addr_f) {
    ref builder b = ref _addr_b.val;
    ref Block t = ref _addr_t.val;
    ref Block f = ref _addr_f.val;

    b.current.Succs = append(b.current.Succs, t, f);
    b.current = null;
}

} // end cfg_package
