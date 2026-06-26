// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using iter = iter_package;

partial class ast_package {

// A Visitor's Visit method is invoked for each node encountered by [Walk].
// If the result visitor w is not nil, [Walk] visits each of the children
// of node with the visitor w, followed by a call of w.Visit(nil).
[GoType] partial interface Visitor {
    Visitor /*w*/ Visit(Node node);
}

internal static void walkList<N>(Visitor v, slice<N> list)
    where N : Node<N>, new()
{
    foreach (var (_, node) in list) {
        Walk(v, node);
    }
}

// TODO(gri): Investigate if providing a closure to Walk leads to
// simpler use (and may help eliminate Inspect in turn).

// Walk traverses an AST in depth-first order: It starts by calling
// v.Visit(node); node must not be nil. If the visitor w returned by
// v.Visit(node) is not nil, Walk is invoked recursively with visitor
// w for each of the non-nil children of node, followed by a call of
// w.Visit(nil).
public static void Walk(Visitor v, Node node) {
    {
        v = v.Visit(node); if (v == default!) {
            return;
        }
    }
    // walk children
    // (the order of the cases matches the order
    // of the corresponding node types in ast.go)
    switch (node.type()) {
    case Comment.val n: {
        break;
    }
    case CommentGroup.val n: {
        walkList(v, // Comments and fields
 // nothing to do
 (~n).List);
        break;
    }
    case Field.val n: {
        if ((~n).Doc != nil) {
            Walk(v, ~(~n).Doc);
        }
        walkList(v, (~n).Names);
        if ((~n).Type != default!) {
            Walk(v, (~n).Type);
        }
        if ((~n).Tag != nil) {
            Walk(v, ~(~n).Tag);
        }
        if ((~n).Comment != nil) {
            Walk(v, ~(~n).Comment);
        }
        break;
    }
    case FieldList.val n: {
        walkList(v, (~n).List);
        break;
    }
    case BadExpr.val n: {
        break;
    }
    case Ident.val n: {
        break;
    }
    case BasicLit.val n: {
        break;
    }
    case Ellipsis.val n: {
        if ((~n).Elt != default!) {
            // Expressions
            // nothing to do
            Walk(v, (~n).Elt);
        }
        break;
    }
    case FuncLit.val n: {
        Walk(v, ~(~n).Type);
        Walk(v, ~(~n).Body);
        break;
    }
    case CompositeLit.val n: {
        if ((~n).Type != default!) {
            Walk(v, (~n).Type);
        }
        walkList(v, (~n).Elts);
        break;
    }
    case ParenExpr.val n: {
        Walk(v, (~n).X);
        break;
    }
    case SelectorExpr.val n: {
        Walk(v, (~n).X);
        Walk(v, ~(~n).Sel);
        break;
    }
    case IndexExpr.val n: {
        Walk(v, (~n).X);
        Walk(v, (~n).Index);
        break;
    }
    case IndexListExpr.val n: {
        Walk(v, (~n).X);
        walkList(v, (~n).Indices);
        break;
    }
    case SliceExpr.val n: {
        Walk(v, (~n).X);
        if ((~n).Low != default!) {
            Walk(v, (~n).Low);
        }
        if ((~n).High != default!) {
            Walk(v, (~n).High);
        }
        if ((~n).Max != default!) {
            Walk(v, (~n).Max);
        }
        break;
    }
    case TypeAssertExpr.val n: {
        Walk(v, (~n).X);
        if ((~n).Type != default!) {
            Walk(v, (~n).Type);
        }
        break;
    }
    case CallExpr.val n: {
        Walk(v, (~n).Fun);
        walkList(v, (~n).Args);
        break;
    }
    case StarExpr.val n: {
        Walk(v, (~n).X);
        break;
    }
    case UnaryExpr.val n: {
        Walk(v, (~n).X);
        break;
    }
    case BinaryExpr.val n: {
        Walk(v, (~n).X);
        Walk(v, (~n).Y);
        break;
    }
    case KeyValueExpr.val n: {
        Walk(v, (~n).Key);
        Walk(v, (~n).Value);
        break;
    }
    case ArrayType.val n: {
        if ((~n).Len != default!) {
            // Types
            Walk(v, (~n).Len);
        }
        Walk(v, (~n).Elt);
        break;
    }
    case StructType.val n: {
        Walk(v, ~(~n).Fields);
        break;
    }
    case FuncType.val n: {
        if ((~n).TypeParams != nil) {
            Walk(v, ~(~n).TypeParams);
        }
        if ((~n).Params != nil) {
            Walk(v, ~(~n).Params);
        }
        if ((~n).Results != nil) {
            Walk(v, ~(~n).Results);
        }
        break;
    }
    case InterfaceType.val n: {
        Walk(v, ~(~n).Methods);
        break;
    }
    case MapType.val n: {
        Walk(v, (~n).Key);
        Walk(v, (~n).Value);
        break;
    }
    case ChanType.val n: {
        Walk(v, (~n).Value);
        break;
    }
    case BadStmt.val n: {
        break;
    }
    case DeclStmt.val n: {
        Walk(v, // Statements
 // nothing to do
 (~n).Decl);
        break;
    }
    case EmptyStmt.val n: {
        break;
    }
    case LabeledStmt.val n: {
        Walk(v, // nothing to do
 ~(~n).Label);
        Walk(v, (~n).Stmt);
        break;
    }
    case ExprStmt.val n: {
        Walk(v, (~n).X);
        break;
    }
    case SendStmt.val n: {
        Walk(v, (~n).Chan);
        Walk(v, (~n).Value);
        break;
    }
    case IncDecStmt.val n: {
        Walk(v, (~n).X);
        break;
    }
    case AssignStmt.val n: {
        walkList(v, (~n).Lhs);
        walkList(v, (~n).Rhs);
        break;
    }
    case GoStmt.val n: {
        Walk(v, ~(~n).Call);
        break;
    }
    case DeferStmt.val n: {
        Walk(v, ~(~n).Call);
        break;
    }
    case ReturnStmt.val n: {
        walkList(v, (~n).Results);
        break;
    }
    case BranchStmt.val n: {
        if ((~n).Label != nil) {
            Walk(v, ~(~n).Label);
        }
        break;
    }
    case BlockStmt.val n: {
        walkList(v, (~n).List);
        break;
    }
    case IfStmt.val n: {
        if ((~n).Init != default!) {
            Walk(v, (~n).Init);
        }
        Walk(v, (~n).Cond);
        Walk(v, ~(~n).Body);
        if ((~n).Else != default!) {
            Walk(v, (~n).Else);
        }
        break;
    }
    case CaseClause.val n: {
        walkList(v, (~n).List);
        walkList(v, (~n).Body);
        break;
    }
    case SwitchStmt.val n: {
        if ((~n).Init != default!) {
            Walk(v, (~n).Init);
        }
        if ((~n).Tag != default!) {
            Walk(v, (~n).Tag);
        }
        Walk(v, ~(~n).Body);
        break;
    }
    case TypeSwitchStmt.val n: {
        if ((~n).Init != default!) {
            Walk(v, (~n).Init);
        }
        Walk(v, (~n).Assign);
        Walk(v, ~(~n).Body);
        break;
    }
    case CommClause.val n: {
        if ((~n).Comm != default!) {
            Walk(v, (~n).Comm);
        }
        walkList(v, (~n).Body);
        break;
    }
    case SelectStmt.val n: {
        Walk(v, ~(~n).Body);
        break;
    }
    case ForStmt.val n: {
        if ((~n).Init != default!) {
            Walk(v, (~n).Init);
        }
        if ((~n).Cond != default!) {
            Walk(v, (~n).Cond);
        }
        if ((~n).Post != default!) {
            Walk(v, (~n).Post);
        }
        Walk(v, ~(~n).Body);
        break;
    }
    case RangeStmt.val n: {
        if ((~n).Key != default!) {
            Walk(v, (~n).Key);
        }
        if ((~n).Value != default!) {
            Walk(v, (~n).Value);
        }
        Walk(v, (~n).X);
        Walk(v, ~(~n).Body);
        break;
    }
    case ImportSpec.val n: {
        if ((~n).Doc != nil) {
            // Declarations
            Walk(v, ~(~n).Doc);
        }
        if ((~n).Name != nil) {
            Walk(v, ~(~n).Name);
        }
        Walk(v, ~(~n).Path);
        if ((~n).Comment != nil) {
            Walk(v, ~(~n).Comment);
        }
        break;
    }
    case ValueSpec.val n: {
        if ((~n).Doc != nil) {
            Walk(v, ~(~n).Doc);
        }
        walkList(v, (~n).Names);
        if ((~n).Type != default!) {
            Walk(v, (~n).Type);
        }
        walkList(v, (~n).Values);
        if ((~n).Comment != nil) {
            Walk(v, ~(~n).Comment);
        }
        break;
    }
    case TypeSpec.val n: {
        if ((~n).Doc != nil) {
            Walk(v, ~(~n).Doc);
        }
        Walk(v, ~(~n).Name);
        if ((~n).TypeParams != nil) {
            Walk(v, ~(~n).TypeParams);
        }
        Walk(v, (~n).Type);
        if ((~n).Comment != nil) {
            Walk(v, ~(~n).Comment);
        }
        break;
    }
    case BadDecl.val n: {
        break;
    }
    case GenDecl.val n: {
        if ((~n).Doc != nil) {
            // nothing to do
            Walk(v, ~(~n).Doc);
        }
        walkList(v, (~n).Specs);
        break;
    }
    case FuncDecl.val n: {
        if ((~n).Doc != nil) {
            Walk(v, ~(~n).Doc);
        }
        if ((~n).Recv != nil) {
            Walk(v, ~(~n).Recv);
        }
        Walk(v, ~(~n).Name);
        Walk(v, ~(~n).Type);
        if ((~n).Body != nil) {
            Walk(v, ~(~n).Body);
        }
        break;
    }
    case File.val n: {
        if ((~n).Doc != nil) {
            // Files and packages
            Walk(v, ~(~n).Doc);
        }
        Walk(v, ~(~n).Name);
        walkList(v, (~n).Decls);
        break;
    }
    case Package.val n: {
        foreach (var (_, f) in (~n).Files) {
            // don't walk n.Comments - they have been
            // visited already through the individual
            // nodes
            Walk(v, ~f);
        }
        break;
    }
    default: {
        var n = node.type();
        throw panic(fmt.Sprintf("ast.Walk: unexpected node type %T"u8, n));
        break;
    }}
    v.Visit(default!);
}

internal delegate bool inspector(Node _);

internal static Visitor Visit(this inspector f, Node node) {
    if (f(node)) {
        return f;
    }
    return default!;
}

// Inspect traverses an AST in depth-first order: It starts by calling
// f(node); node must not be nil. If f returns true, Inspect invokes f
// recursively for each of the non-nil children of node, followed by a
// call of f(nil).
public static void Inspect(Node node, Func<Node, bool> f) {
    Walk(((inspector)f), node);
}

// Preorder returns an iterator over all the nodes of the syntax tree
// beneath (and including) the specified root, in depth-first
// preorder.
//
// For greater control over the traversal of each subtree, use [Inspect].
public static iter.Seq<Node> Preorder(Node root) {
    return (Func<Node, bool> yield) => {
        var ok = true;
        Inspect(root, (Node n) => {
            if (n != default!) {
                // yield must not be called once ok is false.
                ok = ok && yield(n);
            }
            return ok;
        });
    };
}

} // end ast_package
