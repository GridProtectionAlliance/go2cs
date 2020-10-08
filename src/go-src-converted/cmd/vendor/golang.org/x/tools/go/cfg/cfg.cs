// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cfg constructs a simple control-flow graph (CFG) of the
// statements and expressions within a single function.
//
// Use cfg.New to construct the CFG for a function body.
//
// The blocks of the CFG contain all the function's non-control
// statements.  The CFG does not contain control statements such as If,
// Switch, Select, and Branch, but does contain their subexpressions.
// For example, this source code:
//
//    if x := f(); x != nil {
//        T()
//    } else {
//        F()
//    }
//
// produces this CFG:
//
//    1:  x := f()
//        x != nil
//        succs: 2, 3
//    2:  T()
//        succs: 4
//    3:  F()
//        succs: 4
//    4:
//
// The CFG does contain Return statements; even implicit returns are
// materialized (at the position of the function's closing brace).
//
// The CFG does not record conditions associated with conditional branch
// edges, nor the short-circuit semantics of the && and || operators,
// nor abnormal control flow caused by panic.  If you need this
// information, use golang.org/x/tools/go/ssa instead.
//
// package cfg -- go2cs converted at 2020 October 08 04:58:29 UTC
// import "cmd/vendor/golang.org/x/tools/go/cfg" ==> using cfg = go.cmd.vendor.golang.org.x.tools.go.cfg_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\tools\go\cfg\cfg.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using format = go.go.format_package;
using token = go.go.token_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class cfg_package
    {
        // A CFG represents the control-flow graph of a single function.
        //
        // The entry point is Blocks[0]; there may be multiple return blocks.
        public partial struct CFG
        {
            public slice<ptr<Block>> Blocks; // block[0] is entry; order otherwise undefined
        }

        // A Block represents a basic block: a list of statements and
        // expressions that are always evaluated sequentially.
        //
        // A block may have 0-2 successors: zero for a return block or a block
        // that calls a function such as panic that never returns; one for a
        // normal (jump) block; and two for a conditional (if) block.
        public partial struct Block
        {
            public slice<ast.Node> Nodes; // statements, expressions, and ValueSpecs
            public slice<ptr<Block>> Succs; // successor nodes in the graph
            public int Index; // index within CFG.Blocks
            public bool Live; // block is reachable from entry

            public @string comment; // for debugging
            public array<ptr<Block>> succs2; // underlying array for Succs
        }

        // New returns a new control-flow graph for the specified function body,
        // which must be non-nil.
        //
        // The CFG builder calls mayReturn to determine whether a given function
        // call may return.  For example, calls to panic, os.Exit, and log.Fatal
        // do not return, so the builder can remove infeasible graph edges
        // following such calls.  The builder calls mayReturn only for a
        // CallExpr beneath an ExprStmt.
        public static ptr<CFG> New(ptr<ast.BlockStmt> _addr_body, Func<ptr<ast.CallExpr>, bool> mayReturn)
        {
            ref ast.BlockStmt body = ref _addr_body.val;

            builder b = new builder(mayReturn:mayReturn,cfg:new(CFG),);
            b.current = b.newBlock("entry");
            b.stmt(body); 

            // Compute liveness (reachability from entry point), breadth-first.
            var q = make_slice<ptr<Block>>(0L, len(b.cfg.Blocks));
            q = append(q, b.cfg.Blocks[0L]); // entry point
            while (len(q) > 0L)
            {
                b = q[len(q) - 1L];
                q = q[..len(q) - 1L];

                if (!b.Live)
                {
                    b.Live = true;
                    q = append(q, b.Succs);
                }

            } 

            // Does control fall off the end of the function's body?
            // Make implicit return explicit.
 

            // Does control fall off the end of the function's body?
            // Make implicit return explicit.
            if (b.current != null && b.current.Live)
            {
                b.add(addr(new ast.ReturnStmt(Return:body.End()-1,)));
            }

            return _addr_b.cfg!;

        }

        private static @string String(this ptr<Block> _addr_b)
        {
            ref Block b = ref _addr_b.val;

            return fmt.Sprintf("block %d (%s)", b.Index, b.comment);
        }

        // Return returns the return statement at the end of this block if present, nil otherwise.
        private static ptr<ast.ReturnStmt> Return(this ptr<Block> _addr_b)
        {
            ptr<ast.ReturnStmt> ret = default!;
            ref Block b = ref _addr_b.val;

            if (len(b.Nodes) > 0L)
            {
                ret, _ = b.Nodes[len(b.Nodes) - 1L]._<ptr<ast.ReturnStmt>>();
            }

            return ;

        }

        // Format formats the control-flow graph for ease of debugging.
        private static @string Format(this ptr<CFG> _addr_g, ptr<token.FileSet> _addr_fset)
        {
            ref CFG g = ref _addr_g.val;
            ref token.FileSet fset = ref _addr_fset.val;

            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            foreach (var (_, b) in g.Blocks)
            {
                fmt.Fprintf(_addr_buf, ".%d: # %s\n", b.Index, b.comment);
                foreach (var (_, n) in b.Nodes)
                {
                    fmt.Fprintf(_addr_buf, "\t%s\n", formatNode(_addr_fset, n));
                }
                if (len(b.Succs) > 0L)
                {
                    fmt.Fprintf(_addr_buf, "\tsuccs:");
                    foreach (var (_, succ) in b.Succs)
                    {
                        fmt.Fprintf(_addr_buf, " %d", succ.Index);
                    }
                    buf.WriteByte('\n');

                }

                buf.WriteByte('\n');

            }
            return buf.String();

        }

        private static @string formatNode(ptr<token.FileSet> _addr_fset, ast.Node n)
        {
            ref token.FileSet fset = ref _addr_fset.val;

            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            format.Node(_addr_buf, fset, n); 
            // Indent secondary lines by a tab.
            return string(bytes.Replace(buf.Bytes(), (slice<byte>)"\n", (slice<byte>)"\n\t", -1L));

        }
    }
}}}}}}}
