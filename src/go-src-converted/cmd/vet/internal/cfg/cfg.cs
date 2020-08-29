// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This package constructs a simple control-flow graph (CFG) of the
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
// package cfg -- go2cs converted at 2020 August 29 10:09:00 UTC
// import "cmd/vet/internal/cfg" ==> using cfg = go.cmd.vet.@internal.cfg_package
// Original source: C:\Go\src\cmd\vet\internal\cfg\cfg.go
// Although the vet tool has type information, it is often extremely
// fragmentary, so for simplicity this package does not depend on
// go/types.  Consequently control-flow conditions are ignored even
// when constant, and "mayReturn" information must be provided by the
// client.
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using format = go.go.format_package;
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
        // A CFG represents the control-flow graph of a single function.
        //
        // The entry point is Blocks[0]; there may be multiple return blocks.
        public partial struct CFG
        {
            public slice<ref Block> Blocks; // block[0] is entry; order otherwise undefined
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
            public slice<ref Block> Succs; // successor nodes in the graph

            public @string comment; // for debugging
            public int index; // index within CFG.Blocks
            public bool unreachable; // is block of stmts following return/panic/for{}
            public array<ref Block> succs2; // underlying array for Succs
        }

        // New returns a new control-flow graph for the specified function body,
        // which must be non-nil.
        //
        // The CFG builder calls mayReturn to determine whether a given function
        // call may return.  For example, calls to panic, os.Exit, and log.Fatal
        // do not return, so the builder can remove infeasible graph edges
        // following such calls.  The builder calls mayReturn only for a
        // CallExpr beneath an ExprStmt.
        public static ref CFG New(ref ast.BlockStmt body, Func<ref ast.CallExpr, bool> mayReturn)
        {
            builder b = new builder(mayReturn:mayReturn,cfg:new(CFG),);
            b.current = b.newBlock("entry");
            b.stmt(body); 

            // Does control fall off the end of the function's body?
            // Make implicit return explicit.
            if (b.current != null && !b.current.unreachable)
            {
                b.add(ref new ast.ReturnStmt(Return:body.End()-1,));
            }
            return b.cfg;
        }

        private static @string String(this ref Block b)
        {
            return fmt.Sprintf("block %d (%s)", b.index, b.comment);
        }

        // Return returns the return statement at the end of this block if present, nil otherwise.
        private static ref ast.ReturnStmt Return(this ref Block b)
        {
            if (len(b.Nodes) > 0L)
            {
                ret, _ = b.Nodes[len(b.Nodes) - 1L]._<ref ast.ReturnStmt>();
            }
            return;
        }

        // Format formats the control-flow graph for ease of debugging.
        private static @string Format(this ref CFG g, ref token.FileSet fset)
        {
            bytes.Buffer buf = default;
            foreach (var (_, b) in g.Blocks)
            {
                fmt.Fprintf(ref buf, ".%d: # %s\n", b.index, b.comment);
                foreach (var (_, n) in b.Nodes)
                {
                    fmt.Fprintf(ref buf, "\t%s\n", formatNode(fset, n));
                }
                if (len(b.Succs) > 0L)
                {
                    fmt.Fprintf(ref buf, "\tsuccs:");
                    foreach (var (_, succ) in b.Succs)
                    {
                        fmt.Fprintf(ref buf, " %d", succ.index);
                    }
                    buf.WriteByte('\n');
                }
                buf.WriteByte('\n');
            }
            return buf.String();
        }

        private static @string formatNode(ref token.FileSet fset, ast.Node n)
        {
            bytes.Buffer buf = default;
            format.Node(ref buf, fset, n); 
            // Indent secondary lines by a tab.
            return string(bytes.Replace(buf.Bytes(), (slice<byte>)"\n", (slice<byte>)"\n\t", -1L));
        }
    }
}}}}
