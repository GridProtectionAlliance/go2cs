// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syntax -- go2cs converted at 2022 March 06 23:13:10 UTC
// import "cmd/compile/internal/syntax" ==> using syntax = go.cmd.compile.@internal.syntax_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\syntax\nodes.go


namespace go.cmd.compile.@internal;

public static partial class syntax_package {

    // ----------------------------------------------------------------------------
    // Nodes
public partial interface Node {
    Pos Pos();
    Pos aNode();
}

private partial struct node {
    public Pos pos;
}

private static Pos Pos(this ptr<node> _addr_n) {
    ref node n = ref _addr_n.val;

    return n.pos;
}
private static void aNode(this ptr<node> _addr__p0) {
    ref node _p0 = ref _addr__p0.val;

}

// ----------------------------------------------------------------------------
// Files

// package PkgName; DeclList[0], DeclList[1], ...
public partial struct File {
    public Pragma Pragma;
    public ptr<Name> PkgName;
    public slice<Decl> DeclList;
    public Pos EOF;
    public ref node node => ref node_val;
}

// ----------------------------------------------------------------------------
// Declarations

public partial interface Decl {
    void aDecl();
} 

//              Path
// LocalPkgName Path
public partial struct ImportDecl {
    public ptr<Group> Group; // nil means not part of a group
    public Pragma Pragma;
    public ptr<Name> LocalPkgName; // including "."; nil means no rename present
    public ptr<BasicLit> Path; // Path.Bad || Path.Kind == StringLit; nil means no path
    public ref decl decl => ref decl_val;
} 

// NameList
// NameList      = Values
// NameList Type = Values
public partial struct ConstDecl {
    public ptr<Group> Group; // nil means not part of a group
    public Pragma Pragma;
    public slice<ptr<Name>> NameList;
    public Expr Type; // nil means no type
    public Expr Values; // nil means no values
    public ref decl decl => ref decl_val;
} 

// Name Type
public partial struct TypeDecl {
    public ptr<Group> Group; // nil means not part of a group
    public Pragma Pragma;
    public ptr<Name> Name;
    public slice<ptr<Field>> TParamList; // nil means no type parameters
    public bool Alias;
    public Expr Type;
    public ref decl decl => ref decl_val;
} 

// NameList Type
// NameList Type = Values
// NameList      = Values
public partial struct VarDecl {
    public ptr<Group> Group; // nil means not part of a group
    public Pragma Pragma;
    public slice<ptr<Name>> NameList;
    public Expr Type; // nil means no type
    public Expr Values; // nil means no values
    public ref decl decl => ref decl_val;
} 

// func          Name Type { Body }
// func          Name Type
// func Receiver Name Type { Body }
// func Receiver Name Type
public partial struct FuncDecl {
    public Pragma Pragma;
    public ptr<Field> Recv; // nil means regular function
    public ptr<Name> Name;
    public slice<ptr<Field>> TParamList; // nil means no type parameters
    public ptr<FuncType> Type;
    public ptr<BlockStmt> Body; // nil means no body (forward declaration)
    public ref decl decl => ref decl_val;
}
private partial struct decl {
    public ref node node => ref node_val;
}

private static void aDecl(this ptr<decl> _addr__p0) {
    ref decl _p0 = ref _addr__p0.val;

}

// All declarations belonging to the same group point to the same Group node.
public partial struct Group {
    public nint _; // not empty so we are guaranteed different Group instances
}

// ----------------------------------------------------------------------------
// Expressions

public static ptr<Name> NewName(Pos pos, @string value) {
    ptr<Name> n = @new<Name>();
    n.pos = pos;
    n.Value = value;
    return _addr_n!;
}

public partial interface Expr {
    void aExpr();
} 

// Placeholder for an expression that failed to parse
// correctly and where we can't provide a better node.
public partial struct BadExpr {
    public ref expr expr => ref expr_val;
} 

// Value
public partial struct Name {
    public @string Value;
    public ref expr expr => ref expr_val;
} 

// Value
public partial struct BasicLit {
    public @string Value;
    public LitKind Kind;
    public bool Bad; // true means the literal Value has syntax errors
    public ref expr expr => ref expr_val;
} 

// Type { ElemList[0], ElemList[1], ... }
public partial struct CompositeLit {
    public Expr Type; // nil means no literal type
    public slice<Expr> ElemList;
    public nint NKeys; // number of elements with keys
    public Pos Rbrace;
    public ref expr expr => ref expr_val;
} 

// Key: Value
public partial struct KeyValueExpr {
    public Expr Key;
    public Expr Value;
    public ref expr expr => ref expr_val;
} 

// func Type { Body }
public partial struct FuncLit {
    public ptr<FuncType> Type;
    public ptr<BlockStmt> Body;
    public ref expr expr => ref expr_val;
} 

// (X)
public partial struct ParenExpr {
    public Expr X;
    public ref expr expr => ref expr_val;
} 

// X.Sel
public partial struct SelectorExpr {
    public Expr X;
    public ptr<Name> Sel;
    public ref expr expr => ref expr_val;
} 

// X[Index]
// X[T1, T2, ...] (with Ti = Index.(*ListExpr).ElemList[i])
public partial struct IndexExpr {
    public Expr X;
    public Expr Index;
    public ref expr expr => ref expr_val;
} 

// X[Index[0] : Index[1] : Index[2]]
public partial struct SliceExpr {
    public Expr X;
    public array<Expr> Index; // Full indicates whether this is a simple or full slice expression.
// In a valid AST, this is equivalent to Index[2] != nil.
// TODO(mdempsky): This is only needed to report the "3-index
// slice of string" error when Index[2] is missing.
    public bool Full;
    public ref expr expr => ref expr_val;
} 

// X.(Type)
public partial struct AssertExpr {
    public Expr X;
    public Expr Type;
    public ref expr expr => ref expr_val;
} 

// X.(type)
// Lhs := X.(type)
public partial struct TypeSwitchGuard {
    public ptr<Name> Lhs; // nil means no Lhs :=
    public Expr X; // X.(type)
    public ref expr expr => ref expr_val;
}

public partial struct Operation {
    public Operator Op;
    public Expr X; // Y == nil means unary expression
    public Expr Y; // Y == nil means unary expression
    public ref expr expr => ref expr_val;
} 

// Fun(ArgList[0], ArgList[1], ...)
public partial struct CallExpr {
    public Expr Fun;
    public slice<Expr> ArgList; // nil means no arguments
    public bool HasDots; // last argument is followed by ...
    public ref expr expr => ref expr_val;
} 

// ElemList[0], ElemList[1], ...
public partial struct ListExpr {
    public slice<Expr> ElemList;
    public ref expr expr => ref expr_val;
} 

// [Len]Elem
public partial struct ArrayType {
    public Expr Len; // nil means Len is ...
    public Expr Elem;
    public ref expr expr => ref expr_val;
} 

// []Elem
public partial struct SliceType {
    public Expr Elem;
    public ref expr expr => ref expr_val;
} 

// ...Elem
public partial struct DotsType {
    public Expr Elem;
    public ref expr expr => ref expr_val;
} 

// struct { FieldList[0] TagList[0]; FieldList[1] TagList[1]; ... }
public partial struct StructType {
    public slice<ptr<Field>> FieldList;
    public slice<ptr<BasicLit>> TagList; // i >= len(TagList) || TagList[i] == nil means no tag for field i
    public ref expr expr => ref expr_val;
} 

// Name Type
//      Type
public partial struct Field {
    public ptr<Name> Name; // nil means anonymous field/parameter (structs/parameters), or embedded interface (interfaces)
    public Expr Type; // field names declared in a list share the same Type (identical pointers)
    public ref node node => ref node_val;
} 

// interface { MethodList[0]; MethodList[1]; ... }
public partial struct InterfaceType {
    public slice<ptr<Field>> MethodList; // a field named "type" means a type constraint
    public ref expr expr => ref expr_val;
}

public partial struct FuncType {
    public slice<ptr<Field>> ParamList;
    public slice<ptr<Field>> ResultList;
    public ref expr expr => ref expr_val;
} 

// map[Key]Value
public partial struct MapType {
    public Expr Key;
    public Expr Value;
    public ref expr expr => ref expr_val;
} 

//   chan Elem
// <-chan Elem
// chan<- Elem
public partial struct ChanType {
    public ChanDir Dir; // 0 means no direction
    public Expr Elem;
    public ref expr expr => ref expr_val;
}
private partial struct expr {
    public ref node node => ref node_val;
}

private static void aExpr(this ptr<expr> _addr__p0) {
    ref expr _p0 = ref _addr__p0.val;

}

public partial struct ChanDir { // : nuint
}

private static readonly ChanDir _ = iota;
public static readonly var SendOnly = 0;
public static readonly var RecvOnly = 1;


// ----------------------------------------------------------------------------
// Statements

public partial interface Stmt {
    void aStmt();
}

public partial interface SimpleStmt {
    void aSimpleStmt();
}

public partial struct EmptyStmt {
    public ref simpleStmt simpleStmt => ref simpleStmt_val;
}

public partial struct LabeledStmt {
    public ptr<Name> Label;
    public Stmt Stmt;
    public ref stmt stmt => ref stmt_val;
}

public partial struct BlockStmt {
    public slice<Stmt> List;
    public Pos Rbrace;
    public ref stmt stmt => ref stmt_val;
}

public partial struct ExprStmt {
    public Expr X;
    public ref simpleStmt simpleStmt => ref simpleStmt_val;
}

public partial struct SendStmt {
    public Expr Chan; // Chan <- Value
    public Expr Value; // Chan <- Value
    public ref simpleStmt simpleStmt => ref simpleStmt_val;
}

public partial struct DeclStmt {
    public slice<Decl> DeclList;
    public ref stmt stmt => ref stmt_val;
}

public partial struct AssignStmt {
    public Operator Op; // 0 means no operation
    public Expr Lhs; // Rhs == nil means Lhs++ (Op == Add) or Lhs-- (Op == Sub)
    public Expr Rhs; // Rhs == nil means Lhs++ (Op == Add) or Lhs-- (Op == Sub)
    public ref simpleStmt simpleStmt => ref simpleStmt_val;
}

public partial struct BranchStmt {
    public token Tok; // Break, Continue, Fallthrough, or Goto
    public ptr<Name> Label; // Target is the continuation of the control flow after executing
// the branch; it is computed by the parser if CheckBranches is set.
// Target is a *LabeledStmt for gotos, and a *SwitchStmt, *SelectStmt,
// or *ForStmt for breaks and continues, depending on the context of
// the branch. Target is not set for fallthroughs.
    public Stmt Target;
    public ref stmt stmt => ref stmt_val;
}

public partial struct CallStmt {
    public token Tok; // Go or Defer
    public ptr<CallExpr> Call;
    public ref stmt stmt => ref stmt_val;
}

public partial struct ReturnStmt {
    public Expr Results; // nil means no explicit return values
    public ref stmt stmt => ref stmt_val;
}

public partial struct IfStmt {
    public SimpleStmt Init;
    public Expr Cond;
    public ptr<BlockStmt> Then;
    public Stmt Else; // either nil, *IfStmt, or *BlockStmt
    public ref stmt stmt => ref stmt_val;
}

public partial struct ForStmt {
    public SimpleStmt Init; // incl. *RangeClause
    public Expr Cond;
    public SimpleStmt Post;
    public ptr<BlockStmt> Body;
    public ref stmt stmt => ref stmt_val;
}

public partial struct SwitchStmt {
    public SimpleStmt Init;
    public Expr Tag; // incl. *TypeSwitchGuard
    public slice<ptr<CaseClause>> Body;
    public Pos Rbrace;
    public ref stmt stmt => ref stmt_val;
}

public partial struct SelectStmt {
    public slice<ptr<CommClause>> Body;
    public Pos Rbrace;
    public ref stmt stmt => ref stmt_val;
}
public partial struct RangeClause {
    public Expr Lhs; // nil means no Lhs = or Lhs :=
    public bool Def; // means :=
    public Expr X; // range X
    public ref simpleStmt simpleStmt => ref simpleStmt_val;
}

public partial struct CaseClause {
    public Expr Cases; // nil means default clause
    public slice<Stmt> Body;
    public Pos Colon;
    public ref node node => ref node_val;
}

public partial struct CommClause {
    public SimpleStmt Comm; // send or receive stmt; nil means default clause
    public slice<Stmt> Body;
    public Pos Colon;
    public ref node node => ref node_val;
}
private partial struct stmt {
    public ref node node => ref node_val;
}

private static void aStmt(this stmt _p0) {
}

private partial struct simpleStmt {
    public ref stmt stmt => ref stmt_val;
}

private static void aSimpleStmt(this simpleStmt _p0) {
}

// ----------------------------------------------------------------------------
// Comments

// TODO(gri) Consider renaming to CommentPos, CommentPlacement, etc.
//           Kind = Above doesn't make much sense.
public partial struct CommentKind { // : nuint
}

public static readonly CommentKind Above = iota;
public static readonly var Below = 0;
public static readonly var Left = 1;
public static readonly var Right = 2;


public partial struct Comment {
    public CommentKind Kind;
    public @string Text;
    public ptr<Comment> Next;
}

} // end syntax_package
