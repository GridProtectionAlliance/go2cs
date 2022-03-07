// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package astutil -- go2cs converted at 2022 March 06 23:34:58 UTC
// import "cmd/vendor/golang.org/x/tools/go/ast/astutil" ==> using astutil = go.cmd.vendor.golang.org.x.tools.go.ast.astutil_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\tools\go\ast\astutil\enclosing.go
// This file defines utilities for working with source positions.

using fmt = go.fmt_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using sort = go.sort_package;
using System;


namespace go.cmd.vendor.golang.org.x.tools.go.ast;

public static partial class astutil_package {

    // PathEnclosingInterval returns the node that encloses the source
    // interval [start, end), and all its ancestors up to the AST root.
    //
    // The definition of "enclosing" used by this function considers
    // additional whitespace abutting a node to be enclosed by it.
    // In this example:
    //
    //              z := x + y // add them
    //                   <-A->
    //                  <----B----->
    //
    // the ast.BinaryExpr(+) node is considered to enclose interval B
    // even though its [Pos()..End()) is actually only interval A.
    // This behaviour makes user interfaces more tolerant of imperfect
    // input.
    //
    // This function treats tokens as nodes, though they are not included
    // in the result. e.g. PathEnclosingInterval("+") returns the
    // enclosing ast.BinaryExpr("x + y").
    //
    // If start==end, the 1-char interval following start is used instead.
    //
    // The 'exact' result is true if the interval contains only path[0]
    // and perhaps some adjacent whitespace.  It is false if the interval
    // overlaps multiple children of path[0], or if it contains only
    // interior whitespace of path[0].
    // In this example:
    //
    //              z := x + y // add them
    //                <--C-->     <---E-->
    //                  ^
    //                  D
    //
    // intervals C, D and E are inexact.  C is contained by the
    // z-assignment statement, because it spans three of its children (:=,
    // x, +).  So too is the 1-char interval D, because it contains only
    // interior whitespace of the assignment.  E is considered interior
    // whitespace of the BlockStmt containing the assignment.
    //
    // Precondition: [start, end) both lie within the same file as root.
    // TODO(adonovan): return (nil, false) in this case and remove precond.
    // Requires FileSet; see loader.tokenFileContainsPos.
    //
    // Postcondition: path is never nil; it always contains at least 'root'.
    //
public static (slice<ast.Node>, bool) PathEnclosingInterval(ptr<ast.File> _addr_root, token.Pos start, token.Pos end) {
    slice<ast.Node> path = default;
    bool exact = default;
    ref ast.File root = ref _addr_root.val;
 
    // fmt.Printf("EnclosingInterval %d %d\n", start, end) // debugging

    // Precondition: node.[Pos..End) and adjoining whitespace contain [start, end).
    Func<ast.Node, bool> visit = default;
    visit = node => {
        path = append(path, node);

        var nodePos = node.Pos();
        var nodeEnd = node.End(); 

        // fmt.Printf("visit(%T, %d, %d)\n", node, nodePos, nodeEnd) // debugging

        // Intersect [start, end) with interval of node.
        if (start < nodePos) {
            start = nodePos;
        }
        if (end > nodeEnd) {
            end = nodeEnd;
        }
        var children = childrenOf(node);
        var l = len(children);
        {
            var i__prev1 = i;

            foreach (var (__i, __child) in children) {
                i = __i;
                child = __child; 
                // [childPos, childEnd) is unaugmented interval of child.
                var childPos = child.Pos();
                var childEnd = child.End(); 

                // [augPos, augEnd) is whitespace-augmented interval of child.
                var augPos = childPos;
                var augEnd = childEnd;
                if (i > 0) {
                    augPos = children[i - 1].End(); // start of preceding whitespace
                }
                if (i < l - 1) {
                    var nextChildPos = children[i + 1].Pos(); 
                    // Does [start, end) lie between child and next child?
                    if (start >= augEnd && end <= nextChildPos) {
                        return false; // inexact match
                    }
                    augEnd = nextChildPos; // end of following whitespace
                }
                if (augPos <= start && end <= augEnd) {
                    tokenNode (_, isToken) = child._<tokenNode>();
                    return isToken || visit(child);
                }
                if (start < childEnd && end > augEnd) {
                    break;
                }
            }
            i = i__prev1;
        }

        if (start == nodePos && end == nodeEnd) {
            return true; // exact match
        }
        return false; // inexact: overlaps multiple children
    };

    if (start > end) {
        (start, end) = (end, start);
    }
    if (start < root.End() && end > root.Pos()) {
        if (start == end) {
            end = start + 1; // empty interval => interval of size 1
        }
        exact = visit(root); 

        // Reverse the path:
        {
            var i__prev1 = i;
            var l__prev1 = l;

            for (nint i = 0;
            l = len(path); i < l / 2; i++) {
                (path[i], path[l - 1 - i]) = (path[l - 1 - i], path[i]);
            }
    else


            i = i__prev1;
            l = l__prev1;
        }

    } { 
        // Selection lies within whitespace preceding the
        // first (or following the last) declaration in the file.
        // The result nonetheless always includes the ast.File.
        path = append(path, root);

    }
    return ;

}

// tokenNode is a dummy implementation of ast.Node for a single token.
// They are used transiently by PathEnclosingInterval but never escape
// this package.
//
private partial struct tokenNode {
    public token.Pos pos;
    public token.Pos end;
}

private static token.Pos Pos(this tokenNode n) {
    return n.pos;
}

private static token.Pos End(this tokenNode n) {
    return n.end;
}

private static ast.Node tok(token.Pos pos, nint len) {
    return new tokenNode(pos,pos+token.Pos(len));
}

// childrenOf returns the direct non-nil children of ast.Node n.
// It may include fake ast.Node implementations for bare tokens.
// it is not safe to call (e.g.) ast.Walk on such nodes.
//
private static slice<ast.Node> childrenOf(ast.Node n) {
    slice<ast.Node> children = default; 

    // First add nodes for all true subtrees.
    ast.Inspect(n, node => {
        if (node == n) { // push n
            return true; // recur
        }
        if (node != null) { // push child
            children = append(children, node);

        }
        return false; // no recursion
    }); 

    // Then add fake Nodes for bare tokens.
    switch (n.type()) {
        case ptr<ast.ArrayType> n:
            children = append(children, tok(n.Lbrack, len("[")), tok(n.Elt.End(), len("]")));
            break;
        case ptr<ast.AssignStmt> n:
            children = append(children, tok(n.TokPos, len(n.Tok.String())));
            break;
        case ptr<ast.BasicLit> n:
            children = append(children, tok(n.ValuePos, len(n.Value)));
            break;
        case ptr<ast.BinaryExpr> n:
            children = append(children, tok(n.OpPos, len(n.Op.String())));
            break;
        case ptr<ast.BlockStmt> n:
            children = append(children, tok(n.Lbrace, len("{")), tok(n.Rbrace, len("}")));
            break;
        case ptr<ast.BranchStmt> n:
            children = append(children, tok(n.TokPos, len(n.Tok.String())));
            break;
        case ptr<ast.CallExpr> n:
            children = append(children, tok(n.Lparen, len("(")), tok(n.Rparen, len(")")));
            if (n.Ellipsis != 0) {
                children = append(children, tok(n.Ellipsis, len("...")));
            }
            break;
        case ptr<ast.CaseClause> n:
            if (n.List == null) {
                children = append(children, tok(n.Case, len("default")));
            }
            else
 {
                children = append(children, tok(n.Case, len("case")));
            }

            children = append(children, tok(n.Colon, len(":")));
            break;
        case ptr<ast.ChanType> n:

            if (n.Dir == ast.RECV) 
                children = append(children, tok(n.Begin, len("<-chan")));
            else if (n.Dir == ast.SEND) 
                children = append(children, tok(n.Begin, len("chan<-")));
            else if (n.Dir == ast.RECV | ast.SEND) 
                children = append(children, tok(n.Begin, len("chan")));
                        break;
        case ptr<ast.CommClause> n:
            if (n.Comm == null) {
                children = append(children, tok(n.Case, len("default")));
            }
            else
 {
                children = append(children, tok(n.Case, len("case")));
            }

            children = append(children, tok(n.Colon, len(":")));
            break;
        case ptr<ast.Comment> n:
            break;
        case ptr<ast.CommentGroup> n:
            break;
        case ptr<ast.CompositeLit> n:
            children = append(children, tok(n.Lbrace, len("{")), tok(n.Rbrace, len("{")));
            break;
        case ptr<ast.DeclStmt> n:
            break;
        case ptr<ast.DeferStmt> n:
            children = append(children, tok(n.Defer, len("defer")));
            break;
        case ptr<ast.Ellipsis> n:
            children = append(children, tok(n.Ellipsis, len("...")));
            break;
        case ptr<ast.EmptyStmt> n:
            break;
        case ptr<ast.ExprStmt> n:
            break;
        case ptr<ast.Field> n:
            break;
        case ptr<ast.FieldList> n:
            children = append(children, tok(n.Opening, len("(")), tok(n.Closing, len(")")));
            break;
        case ptr<ast.File> n:
            children = append(children, tok(n.Package, len("package")));
            break;
        case ptr<ast.ForStmt> n:
            children = append(children, tok(n.For, len("for")));
            break;
        case ptr<ast.FuncDecl> n:
            children = null; // discard ast.Walk(FuncDecl) info subtrees
            children = append(children, tok(n.Type.Func, len("func")));
            if (n.Recv != null) {
                children = append(children, n.Recv);
            }

            children = append(children, n.Name);
            if (n.Type.Params != null) {
                children = append(children, n.Type.Params);
            }

            if (n.Type.Results != null) {
                children = append(children, n.Type.Results);
            }

            if (n.Body != null) {
                children = append(children, n.Body);
            }

            break;
        case ptr<ast.FuncLit> n:
            break;
        case ptr<ast.FuncType> n:
            if (n.Func != 0) {
                children = append(children, tok(n.Func, len("func")));
            }
            break;
        case ptr<ast.GenDecl> n:
            children = append(children, tok(n.TokPos, len(n.Tok.String())));
            if (n.Lparen != 0) {
                children = append(children, tok(n.Lparen, len("(")), tok(n.Rparen, len(")")));
            }
            break;
        case ptr<ast.GoStmt> n:
            children = append(children, tok(n.Go, len("go")));
            break;
        case ptr<ast.Ident> n:
            children = append(children, tok(n.NamePos, len(n.Name)));
            break;
        case ptr<ast.IfStmt> n:
            children = append(children, tok(n.If, len("if")));
            break;
        case ptr<ast.ImportSpec> n:
            break;
        case ptr<ast.IncDecStmt> n:
            children = append(children, tok(n.TokPos, len(n.Tok.String())));
            break;
        case ptr<ast.IndexExpr> n:
            children = append(children, tok(n.Lbrack, len("{")), tok(n.Rbrack, len("}")));
            break;
        case ptr<ast.InterfaceType> n:
            children = append(children, tok(n.Interface, len("interface")));
            break;
        case ptr<ast.KeyValueExpr> n:
            children = append(children, tok(n.Colon, len(":")));
            break;
        case ptr<ast.LabeledStmt> n:
            children = append(children, tok(n.Colon, len(":")));
            break;
        case ptr<ast.MapType> n:
            children = append(children, tok(n.Map, len("map")));
            break;
        case ptr<ast.ParenExpr> n:
            children = append(children, tok(n.Lparen, len("(")), tok(n.Rparen, len(")")));
            break;
        case ptr<ast.RangeStmt> n:
            children = append(children, tok(n.For, len("for")), tok(n.TokPos, len(n.Tok.String())));
            break;
        case ptr<ast.ReturnStmt> n:
            children = append(children, tok(n.Return, len("return")));
            break;
        case ptr<ast.SelectStmt> n:
            children = append(children, tok(n.Select, len("select")));
            break;
        case ptr<ast.SelectorExpr> n:
            break;
        case ptr<ast.SendStmt> n:
            children = append(children, tok(n.Arrow, len("<-")));
            break;
        case ptr<ast.SliceExpr> n:
            children = append(children, tok(n.Lbrack, len("[")), tok(n.Rbrack, len("]")));
            break;
        case ptr<ast.StarExpr> n:
            children = append(children, tok(n.Star, len("*")));
            break;
        case ptr<ast.StructType> n:
            children = append(children, tok(n.Struct, len("struct")));
            break;
        case ptr<ast.SwitchStmt> n:
            children = append(children, tok(n.Switch, len("switch")));
            break;
        case ptr<ast.TypeAssertExpr> n:
            children = append(children, tok(n.Lparen - 1, len(".")), tok(n.Lparen, len("(")), tok(n.Rparen, len(")")));
            break;
        case ptr<ast.TypeSpec> n:
            break;
        case ptr<ast.TypeSwitchStmt> n:
            children = append(children, tok(n.Switch, len("switch")));
            break;
        case ptr<ast.UnaryExpr> n:
            children = append(children, tok(n.OpPos, len(n.Op.String())));
            break;
        case ptr<ast.ValueSpec> n:
            break;
        case ptr<ast.BadDecl> n:
            break;
        case ptr<ast.BadExpr> n:
            break;
        case ptr<ast.BadStmt> n:
            break; 

        // TODO(adonovan): opt: merge the logic of ast.Inspect() into
        // the switch above so we can make interleaved callbacks for
        // both Nodes and Tokens in the right order and avoid the need
        // to sort.
    } 

    // TODO(adonovan): opt: merge the logic of ast.Inspect() into
    // the switch above so we can make interleaved callbacks for
    // both Nodes and Tokens in the right order and avoid the need
    // to sort.
    sort.Sort(byPos(children));

    return children;

}

private partial struct byPos { // : slice<ast.Node>
}

private static nint Len(this byPos sl) {
    return len(sl);
}
private static bool Less(this byPos sl, nint i, nint j) {
    return sl[i].Pos() < sl[j].Pos();
}
private static void Swap(this byPos sl, nint i, nint j) {
    (sl[i], sl[j]) = (sl[j], sl[i]);
}

// NodeDescription returns a description of the concrete type of n suitable
// for a user interface.
//
// TODO(adonovan): in some cases (e.g. Field, FieldList, Ident,
// StarExpr) we could be much more specific given the path to the AST
// root.  Perhaps we should do that.
//
public static @string NodeDescription(ast.Node n) => func((_, panic, _) => {
    switch (n.type()) {
        case ptr<ast.ArrayType> n:
            return "array type";
            break;
        case ptr<ast.AssignStmt> n:
            return "assignment";
            break;
        case ptr<ast.BadDecl> n:
            return "bad declaration";
            break;
        case ptr<ast.BadExpr> n:
            return "bad expression";
            break;
        case ptr<ast.BadStmt> n:
            return "bad statement";
            break;
        case ptr<ast.BasicLit> n:
            return "basic literal";
            break;
        case ptr<ast.BinaryExpr> n:
            return fmt.Sprintf("binary %s operation", n.Op);
            break;
        case ptr<ast.BlockStmt> n:
            return "block";
            break;
        case ptr<ast.BranchStmt> n:

            if (n.Tok == token.BREAK) 
                return "break statement";
            else if (n.Tok == token.CONTINUE) 
                return "continue statement";
            else if (n.Tok == token.GOTO) 
                return "goto statement";
            else if (n.Tok == token.FALLTHROUGH) 
                return "fall-through statement";
                        break;
        case ptr<ast.CallExpr> n:
            if (len(n.Args) == 1 && !n.Ellipsis.IsValid()) {
                return "function call (or conversion)";
            }
            return "function call";
            break;
        case ptr<ast.CaseClause> n:
            return "case clause";
            break;
        case ptr<ast.ChanType> n:
            return "channel type";
            break;
        case ptr<ast.CommClause> n:
            return "communication clause";
            break;
        case ptr<ast.Comment> n:
            return "comment";
            break;
        case ptr<ast.CommentGroup> n:
            return "comment group";
            break;
        case ptr<ast.CompositeLit> n:
            return "composite literal";
            break;
        case ptr<ast.DeclStmt> n:
            return NodeDescription(n.Decl) + " statement";
            break;
        case ptr<ast.DeferStmt> n:
            return "defer statement";
            break;
        case ptr<ast.Ellipsis> n:
            return "ellipsis";
            break;
        case ptr<ast.EmptyStmt> n:
            return "empty statement";
            break;
        case ptr<ast.ExprStmt> n:
            return "expression statement";
            break;
        case ptr<ast.Field> n:
            return "field/method/parameter";
            break;
        case ptr<ast.FieldList> n:
            return "field/method/parameter list";
            break;
        case ptr<ast.File> n:
            return "source file";
            break;
        case ptr<ast.ForStmt> n:
            return "for loop";
            break;
        case ptr<ast.FuncDecl> n:
            return "function declaration";
            break;
        case ptr<ast.FuncLit> n:
            return "function literal";
            break;
        case ptr<ast.FuncType> n:
            return "function type";
            break;
        case ptr<ast.GenDecl> n:

            if (n.Tok == token.IMPORT) 
                return "import declaration";
            else if (n.Tok == token.CONST) 
                return "constant declaration";
            else if (n.Tok == token.TYPE) 
                return "type declaration";
            else if (n.Tok == token.VAR) 
                return "variable declaration";
                        break;
        case ptr<ast.GoStmt> n:
            return "go statement";
            break;
        case ptr<ast.Ident> n:
            return "identifier";
            break;
        case ptr<ast.IfStmt> n:
            return "if statement";
            break;
        case ptr<ast.ImportSpec> n:
            return "import specification";
            break;
        case ptr<ast.IncDecStmt> n:
            if (n.Tok == token.INC) {
                return "increment statement";
            }
            return "decrement statement";
            break;
        case ptr<ast.IndexExpr> n:
            return "index expression";
            break;
        case ptr<ast.InterfaceType> n:
            return "interface type";
            break;
        case ptr<ast.KeyValueExpr> n:
            return "key/value association";
            break;
        case ptr<ast.LabeledStmt> n:
            return "statement label";
            break;
        case ptr<ast.MapType> n:
            return "map type";
            break;
        case ptr<ast.Package> n:
            return "package";
            break;
        case ptr<ast.ParenExpr> n:
            return "parenthesized " + NodeDescription(n.X);
            break;
        case ptr<ast.RangeStmt> n:
            return "range loop";
            break;
        case ptr<ast.ReturnStmt> n:
            return "return statement";
            break;
        case ptr<ast.SelectStmt> n:
            return "select statement";
            break;
        case ptr<ast.SelectorExpr> n:
            return "selector";
            break;
        case ptr<ast.SendStmt> n:
            return "channel send";
            break;
        case ptr<ast.SliceExpr> n:
            return "slice expression";
            break;
        case ptr<ast.StarExpr> n:
            return "*-operation"; // load/store expr or pointer type
            break;
        case ptr<ast.StructType> n:
            return "struct type";
            break;
        case ptr<ast.SwitchStmt> n:
            return "switch statement";
            break;
        case ptr<ast.TypeAssertExpr> n:
            return "type assertion";
            break;
        case ptr<ast.TypeSpec> n:
            return "type specification";
            break;
        case ptr<ast.TypeSwitchStmt> n:
            return "type switch";
            break;
        case ptr<ast.UnaryExpr> n:
            return fmt.Sprintf("unary %s operation", n.Op);
            break;
        case ptr<ast.ValueSpec> n:
            return "value specification";
            break;
    }
    panic(fmt.Sprintf("unexpected node type: %T", n));

});

} // end astutil_package
