// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cfg -- go2cs converted at 2020 August 29 10:08:59 UTC
// import "cmd/vet/internal/cfg" ==> using cfg = go.cmd.vet.@internal.cfg_package
// Original source: C:\Go\src\cmd\vet\internal\cfg\builder.go
// This file implements the CFG construction pass.

using fmt = go.fmt_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vet {
namespace @internal
{
    public static partial class cfg_package
    {
        private partial struct builder
        {
            public ptr<CFG> cfg;
            public Func<ref ast.CallExpr, bool> mayReturn;
            public ptr<Block> current;
            public map<ref ast.Object, ref lblock> lblocks; // labeled blocks
            public ptr<targets> targets; // linked stack of branch targets
        }

        private static void stmt(this ref builder _b, ast.Stmt _s) => func(_b, (ref builder b, Defer _, Panic panic, Recover __) =>
        { 
            // The label of the current statement.  If non-nil, its _goto
            // target is always set; its _break and _continue are set only
            // within the body of switch/typeswitch/select/for/range.
            // It is effectively an additional default-nil parameter of stmt().
            ref lblock label = default;
start:
            switch (_s.type())
            {
                case ref ast.BadStmt s:
                    b.add(s);
                    break;
                case ref ast.SendStmt s:
                    b.add(s);
                    break;
                case ref ast.IncDecStmt s:
                    b.add(s);
                    break;
                case ref ast.GoStmt s:
                    b.add(s);
                    break;
                case ref ast.DeferStmt s:
                    b.add(s);
                    break;
                case ref ast.EmptyStmt s:
                    b.add(s);
                    break;
                case ref ast.AssignStmt s:
                    b.add(s);
                    break;
                case ref ast.ExprStmt s:
                    b.add(s);
                    {
                        ref ast.CallExpr (call, ok) = s.X._<ref ast.CallExpr>();

                        if (ok && !b.mayReturn(call))
                        { 
                            // Calls to panic, os.Exit, etc, never return.
                            b.current = b.newUnreachableBlock("unreachable.call");
                        }

                    }
                    break;
                case ref ast.DeclStmt s:
                    ref ast.GenDecl d = s.Decl._<ref ast.GenDecl>();
                    if (d.Tok == token.VAR)
                    {
                        {
                            var spec__prev1 = spec;

                            foreach (var (_, __spec) in d.Specs)
                            {
                                spec = __spec;
                                {
                                    var spec__prev2 = spec;

                                    ref ast.ValueSpec (spec, ok) = spec._<ref ast.ValueSpec>();

                                    if (ok)
                                    {
                                        b.add(spec);
                                    }

                                    spec = spec__prev2;

                                }
                            }

                            spec = spec__prev1;
                        }

                    }
                    break;
                case ref ast.LabeledStmt s:
                    label = b.labeledBlock(s.Label);
                    b.jump(label._goto);
                    b.current = label._goto;
                    _s = s.Stmt;
                    goto start; // effectively: tailcall stmt(g, s.Stmt, label)
                    break;
                case ref ast.ReturnStmt s:
                    b.add(s);
                    b.current = b.newUnreachableBlock("unreachable.return");
                    break;
                case ref ast.BranchStmt s:
                    ref Block block = default;

                    if (s.Tok == token.BREAK) 
                        if (s.Label != null)
                        {
                            {
                                var lb__prev2 = lb;

                                var lb = b.labeledBlock(s.Label);

                                if (lb != null)
                                {
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

                                while (t != null && block == null)
                                {
                                    block = t._break;
                                    t = t.tail;
                                }


                                t = t__prev1;
                            }
                        }
                    else if (s.Tok == token.CONTINUE) 
                        if (s.Label != null)
                        {
                            {
                                var lb__prev2 = lb;

                                lb = b.labeledBlock(s.Label);

                                if (lb != null)
                                {
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

                                while (t != null && block == null)
                                {
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

                            while (t != null)
                            {
                                block = t._fallthrough;
                                t = t.tail;
                            }


                            t = t__prev1;
                        }
                    else if (s.Tok == token.GOTO) 
                        if (s.Label != null)
                        {
                            block = b.labeledBlock(s.Label)._goto;
                        }
                                        if (block == null)
                    {
                        block = b.newBlock("undefined.branch");
                    }
                    b.jump(block);
                    b.current = b.newUnreachableBlock("unreachable.branch");
                    break;
                case ref ast.BlockStmt s:
                    b.stmtList(s.List);
                    break;
                case ref ast.IfStmt s:
                    if (s.Init != null)
                    {
                        b.stmt(s.Init);
                    }
                    var then = b.newBlock("if.then");
                    var done = b.newBlock("if.done");
                    var _else = done;
                    if (s.Else != null)
                    {
                        _else = b.newBlock("if.else");
                    }
                    b.add(s.Cond);
                    b.ifelse(then, _else);
                    b.current = then;
                    b.stmt(s.Body);
                    b.jump(done);

                    if (s.Else != null)
                    {
                        b.current = _else;
                        b.stmt(s.Else);
                        b.jump(done);
                    }
                    b.current = done;
                    break;
                case ref ast.SwitchStmt s:
                    b.switchStmt(s, label);
                    break;
                case ref ast.TypeSwitchStmt s:
                    b.typeSwitchStmt(s, label);
                    break;
                case ref ast.SelectStmt s:
                    b.selectStmt(s, label);
                    break;
                case ref ast.ForStmt s:
                    b.forStmt(s, label);
                    break;
                case ref ast.RangeStmt s:
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

        private static void stmtList(this ref builder b, slice<ast.Stmt> list)
        {
            foreach (var (_, s) in list)
            {
                b.stmt(s);
            }
        }

        private static void switchStmt(this ref builder b, ref ast.SwitchStmt s, ref lblock label)
        {
            if (s.Init != null)
            {
                b.stmt(s.Init);
            }
            if (s.Tag != null)
            {
                b.add(s.Tag);
            }
            var done = b.newBlock("switch.done");
            if (label != null)
            {
                label._break = done;
            } 
            // We pull the default case (if present) down to the end.
            // But each fallthrough label must point to the next
            // body block in source order, so we preallocate a
            // body block (fallthru) for the next case.
            // Unfortunately this makes for a confusing block order.
            ref slice<ast.Stmt> defaultBody = default;
            ref Block defaultFallthrough = default;
            ref Block fallthru = default;            ref Block defaultBlock = default;

            var ncases = len(s.Body.List);
            foreach (var (i, clause) in s.Body.List)
            {
                var body = fallthru;
                if (body == null)
                {
                    body = b.newBlock("switch.body"); // first case only
                } 

                // Preallocate body block for the next case.
                fallthru = done;
                if (i + 1L < ncases)
                {
                    fallthru = b.newBlock("switch.body");
                }
                ref ast.CaseClause cc = clause._<ref ast.CaseClause>();
                if (cc.List == null)
                { 
                    // Default case.
                    defaultBody = ref cc.Body;
                    defaultFallthrough = fallthru;
                    defaultBlock = body;
                    continue;
                }
                ref Block nextCond = default;
                foreach (var (_, cond) in cc.List)
                {
                    nextCond = b.newBlock("switch.next");
                    b.add(cond); // one half of the tag==cond condition
                    b.ifelse(body, nextCond);
                    b.current = nextCond;
                }
                b.current = body;
                b.targets = ref new targets(tail:b.targets,_break:done,_fallthrough:fallthru,);
                b.stmtList(cc.Body);
                b.targets = b.targets.tail;
                b.jump(done);
                b.current = nextCond;
            }
            if (defaultBlock != null)
            {
                b.jump(defaultBlock);
                b.current = defaultBlock;
                b.targets = ref new targets(tail:b.targets,_break:done,_fallthrough:defaultFallthrough,);
                b.stmtList(defaultBody.Value);
                b.targets = b.targets.tail;
            }
            b.jump(done);
            b.current = done;
        }

        private static void typeSwitchStmt(this ref builder b, ref ast.TypeSwitchStmt s, ref lblock label)
        {
            if (s.Init != null)
            {
                b.stmt(s.Init);
            }
            if (s.Assign != null)
            {
                b.add(s.Assign);
            }
            var done = b.newBlock("typeswitch.done");
            if (label != null)
            {
                label._break = done;
            }
            ref ast.CaseClause default_ = default;
            foreach (var (_, clause) in s.Body.List)
            {
                ref ast.CaseClause cc = clause._<ref ast.CaseClause>();
                if (cc.List == null)
                {
                    default_ = cc;
                    continue;
                }
                var body = b.newBlock("typeswitch.body");
                ref Block next = default;
                foreach (var (_, casetype) in cc.List)
                {
                    next = b.newBlock("typeswitch.next"); 
                    // casetype is a type, so don't call b.add(casetype).
                    // This block logically contains a type assertion,
                    // x.(casetype), but it's unclear how to represent x.
                    _ = casetype;
                    b.ifelse(body, next);
                    b.current = next;
                }
                b.current = body;
                b.typeCaseBody(cc, done);
                b.current = next;
            }
            if (default_ != null)
            {
                b.typeCaseBody(default_, done);
            }
            else
            {
                b.jump(done);
            }
            b.current = done;
        }

        private static void typeCaseBody(this ref builder b, ref ast.CaseClause cc, ref Block done)
        {
            b.targets = ref new targets(tail:b.targets,_break:done,);
            b.stmtList(cc.Body);
            b.targets = b.targets.tail;
            b.jump(done);
        }

        private static void selectStmt(this ref builder b, ref ast.SelectStmt s, ref lblock label)
        { 
            // First evaluate channel expressions.
            // TODO(adonovan): fix: evaluate only channel exprs here.
            {
                var clause__prev1 = clause;

                foreach (var (_, __clause) in s.Body.List)
                {
                    clause = __clause;
                    {
                        ref ast.CommClause comm__prev1 = comm;

                        ref ast.CommClause comm = clause._<ref ast.CommClause>().Comm;

                        if (comm != null)
                        {
                            b.stmt(comm);
                        }

                        comm = comm__prev1;

                    }
                }

                clause = clause__prev1;
            }

            var done = b.newBlock("select.done");
            if (label != null)
            {
                label._break = done;
            }
            ref slice<ast.Stmt> defaultBody = default;
            foreach (var (_, cc) in s.Body.List)
            {
                ref ast.CommClause clause = cc._<ref ast.CommClause>();
                if (clause.Comm == null)
                {
                    defaultBody = ref clause.Body;
                    continue;
                }
                var body = b.newBlock("select.body");
                var next = b.newBlock("select.next");
                b.ifelse(body, next);
                b.current = body;
                b.targets = ref new targets(tail:b.targets,_break:done,);
                switch (clause.Comm.type())
                {
                    case ref ast.ExprStmt comm:
                        break;
                    case ref ast.AssignStmt comm:
                        b.add(comm.Lhs[0L]);
                        break;
                }
                b.stmtList(clause.Body);
                b.targets = b.targets.tail;
                b.jump(done);
                b.current = next;
            }
            if (defaultBody != null)
            {
                b.targets = ref new targets(tail:b.targets,_break:done,);
                b.stmtList(defaultBody.Value);
                b.targets = b.targets.tail;
                b.jump(done);
            }
            b.current = done;
        }

        private static void forStmt(this ref builder b, ref ast.ForStmt s, ref lblock label)
        { 
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
            if (s.Init != null)
            {
                b.stmt(s.Init);
            }
            var body = b.newBlock("for.body");
            var done = b.newBlock("for.done"); // target of 'break'
            var loop = body; // target of back-edge
            if (s.Cond != null)
            {
                loop = b.newBlock("for.loop");
            }
            var cont = loop; // target of 'continue'
            if (s.Post != null)
            {
                cont = b.newBlock("for.post");
            }
            if (label != null)
            {
                label._break = done;
                label._continue = cont;
            }
            b.jump(loop);
            b.current = loop;
            if (loop != body)
            {
                b.add(s.Cond);
                b.ifelse(body, done);
                b.current = body;
            }
            b.targets = ref new targets(tail:b.targets,_break:done,_continue:cont,);
            b.stmt(s.Body);
            b.targets = b.targets.tail;
            b.jump(cont);

            if (s.Post != null)
            {
                b.current = cont;
                b.stmt(s.Post);
                b.jump(loop); // back-edge
            }
            b.current = done;
        }

        private static void rangeStmt(this ref builder b, ref ast.RangeStmt s, ref lblock label)
        {
            b.add(s.X);

            if (s.Key != null)
            {
                b.add(s.Key);
            }
            if (s.Value != null)
            {
                b.add(s.Value);
            } 

            //      ...
            // loop:                                   (target of continue)
            //     if ... goto body else done
            // body:
            //      ...
            //     jump loop
            // done:                                   (target of break)
            var loop = b.newBlock("range.loop");
            b.jump(loop);
            b.current = loop;

            var body = b.newBlock("range.body");
            var done = b.newBlock("range.done");
            b.ifelse(body, done);
            b.current = body;

            if (label != null)
            {
                label._break = done;
                label._continue = loop;
            }
            b.targets = ref new targets(tail:b.targets,_break:done,_continue:loop,);
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
        private partial struct targets
        {
            public ptr<targets> tail; // rest of stack
            public ptr<Block> _break;
            public ptr<Block> _continue;
            public ptr<Block> _fallthrough;
        }

        // Destinations associated with a labeled block.
        // We populate these as labels are encountered in forward gotos or
        // labeled statements.
        //
        private partial struct lblock
        {
            public ptr<Block> _goto;
            public ptr<Block> _break;
            public ptr<Block> _continue;
        }

        // labeledBlock returns the branch target associated with the
        // specified label, creating it if needed.
        //
        private static ref lblock labeledBlock(this ref builder b, ref ast.Ident label)
        {
            var lb = b.lblocks[label.Obj];
            if (lb == null)
            {
                lb = ref new lblock(_goto:b.newBlock(label.Name));
                if (b.lblocks == null)
                {
                    b.lblocks = make_map<ref ast.Object, ref lblock>();
                }
                b.lblocks[label.Obj] = lb;
            }
            return lb;
        }

        // newBlock appends a new unconnected basic block to b.cfg's block
        // slice and returns it.
        // It does not automatically become the current block.
        // comment is an optional string for more readable debugging output.
        private static ref Block newBlock(this ref builder b, @string comment)
        {
            var g = b.cfg;
            Block block = ref new Block(index:int32(len(g.Blocks)),comment:comment,);
            block.Succs = block.succs2[..0L];
            g.Blocks = append(g.Blocks, block);
            return block;
        }

        private static ref Block newUnreachableBlock(this ref builder b, @string comment)
        {
            var block = b.newBlock(comment);
            block.unreachable = true;
            return block;
        }

        private static void add(this ref builder b, ast.Node n)
        {
            b.current.Nodes = append(b.current.Nodes, n);
        }

        // jump adds an edge from the current block to the target block,
        // and sets b.current to nil.
        private static void jump(this ref builder b, ref Block target)
        {
            b.current.Succs = append(b.current.Succs, target);
            b.current = null;
        }

        // ifelse emits edges from the current block to the t and f blocks,
        // and sets b.current to nil.
        private static void ifelse(this ref builder b, ref Block t, ref Block f)
        {
            b.current.Succs = append(b.current.Succs, t, f);
            b.current = null;
        }
    }
}}}}
