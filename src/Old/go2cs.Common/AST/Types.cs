//******************************************************************************************************
//  Types.cs - Gbtc
//
//  Copyright © 2022, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/21/2022 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;

namespace go2cs.AST;

// These structures allow deserialization of AST stored in JSON format using "asty" tool:
// https://github.com/asty-org/asty

//type Node struct {
//	NodeType string `json:"NodeType"`
//	RefId    int    `json:"RefId,omitempty"`
//}
public record Node(string NodeType);

public record ExprNode(string NodeType) : Node(NodeType);

public record StmtNode(string NodeType) : Node(NodeType);

public record SpecNode(string NodeType) : Node(NodeType);

public record DeclNode(string NodeType) : Node(NodeType);

//type PositionNode struct {
//	Node
//	Filename string `json:"Filename"`
//	Offset   int    `json:"Offset"`
//	Line     int    `json:"Line"`
//	Column   int    `json:"Column"`
//}
public record PositionNode(string NodeType, string Filename, int Offset, int Line, int Column) : Node(NodeType);

//type CommentNode struct {
//	Node
//	Slash *PositionNode `json:"Slash,omitempty"`
//	Text  string        `json:"Text"`
//}
public record CommentNode(string NodeType, PositionNode Slash, string Text) : Node(NodeType);

//type CommentGroupNode struct {
//	Node
//	List []*CommentNode `json:"List,omitempty"`
//}
public record CommentGroupNode(string NodeType, CommentNode[] List) : Node(NodeType);

//// ---------------------------------------------------------------------------

//type FieldNode struct {
//	Node
//	Doc     *CommentGroupNode `json:"Doc,omitempty"`
//	Names   []*IdentNode      `json:"Names"`
//	Type    IExprNode         `json:"Type"`
//	Tag     *BasicLitNode     `json:"Tag,omitempty"`
//	Comment *CommentGroupNode `json:"Comment,omitempty"`
//}
public record FieldNode(string NodeType, CommentGroupNode Doc, IdentNode[] Names, ExprNode Type, BasicLitNode Tag, CommentGroupNode Comment) : Node(NodeType);

//type FieldListNode struct {
//	Node
//	Opening *PositionNode `json:"Opening,omitempty"`
//	List    []*FieldNode  `json:"List"`
//	Closing *PositionNode `json:"Closing,omitempty"`
//}

public record FieldListNode(string NodeType, PositionNode Opening, FieldNode[] List, PositionNode Closing) : Node(NodeType);

//// ---------------------------------------------------------------------------

//type BadExprNode struct {
//	Node
//	From *PositionNode `json:"From,omitempty"`
//	To   *PositionNode `json:"To,omitempty"`
//}
public record BadExprNode(string NodeType, PositionNode From, PositionNode To) : ExprNode(NodeType);

//type IdentNode struct {
//	Node
//	NamePos *PositionNode `json:"NamePos,omitempty"`
//	Name    string        `json:"Name"`
//	// Obj     *Object   // denoted object; or nil
//}
public record IdentNode(string NodeType, PositionNode NamePos, string Name) : ExprNode(NodeType);

//type EllipsisNode struct {
//	Node
//	Ellipsis *PositionNode `json:"Ellipsis,omitempty"`
//	Elt      IExprNode     `json:"Elt"`
//}
public record EllipsisNode(string NodeType, PositionNode Ellipsis, ExprNode Elt) : ExprNode(NodeType);

//type BasicLitNode struct {
//	Node
//	ValuePos *PositionNode `json:"ValuePos,omitempty"`
//	Kind     string        `json:"Kind"`
//	Value    string        `json:"Value"`
//}
public record BasicLitNode(string NodeType, PositionNode ValuePos, string Kind, string Value) : ExprNode(NodeType);

//type FuncLitNode struct {
//	Node
//	Type *FuncTypeNode  `json:"Type"`
//	Body *BlockStmtNode `json:"Body"`
//}
public record FuncLitNode(string NodeType, FuncTypeNode Type, BlockStmtNode Body) : ExprNode(NodeType);

//type CompositeLitNode struct {
//	Node
//	Type       IExprNode     `json:"Type"`
//	Lbrace     *PositionNode `json:"Lbrace,omitempty"`
//	Elts       []IExprNode   `json:"Elts"`
//	Rbrace     *PositionNode `json:"Rbrace,omitempty"`
//	Incomplete bool          `json:"Incomplete"`
//}
public record CompositeLitNode(string NodeType, ExprNode Type, PositionNode Lbrace, ExprNode[] Elts, PositionNode Rbrace, bool Incomplete) : ExprNode(NodeType);


//type ParenExprNode struct {
//	Node
//	Lparen *PositionNode `json:"Lparen,omitempty"`
//	X      IExprNode     `json:"X"`
//	Rparen *PositionNode `json:"Rparen,omitempty"`
//}
public record ParenExprNode(string NodeType, PositionNode Lparen, ExprNode X, PositionNode Rparen) : ExprNode(NodeType);

//type SelectorExprNode struct {
//	Node
//	X   IExprNode  `json:"X,omitempty"`
//	Sel *IdentNode `json:"Sel,omitempty"`
//}
public record SelectorExprNode(string NodeType, ExprNode X, IdentNode Sel) : ExprNode(NodeType);

//type IndexExprNode struct {
//	Node
//	X      IExprNode     `json:"X"`
//	Lbrack *PositionNode `json:"Lbrack,omitempty"`
//	Index  IExprNode     `json:"Index"`
//	Rbrack *PositionNode `json:"Rbrack,omitempty"`
//}
public record IndexExprNode(string NodeType, ExprNode X, PositionNode Lbrack, ExprNode Index, PositionNode Rbrack) : ExprNode(NodeType);

//type IndexListExprNode struct {
//	Node
//	X       IExprNode     `json:"X"`
//	Lbrack  *PositionNode `json:"Lbrack,omitempty"`
//	Indices []IExprNode   `json:"Indices"`
//	Rbrack  *PositionNode `json:"Rbrack,omitempty"`
//}
public record IndexListExprNode(string NodeType, ExprNode X, PositionNode Lbrack, ExprNode[] Indicies, PositionNode Rbrack) : ExprNode(NodeType);

//type SliceExprNode struct {
//	Node
//	X      IExprNode     `json:"X"`
//	Lbrack *PositionNode `json:"Lbrack,omitempty"`
//	Low    IExprNode     `json:"Low"`
//	High   IExprNode     `json:"High"`
//	Max    IExprNode     `json:"Max"`
//	Slice3 bool          `json:"Slice3"`
//	Rbrack *PositionNode `json:"Rbrack,omitempty"`
//}
public record SliceExprNode(string NodeType, ExprNode X, PositionNode Lbrack, ExprNode Low, ExprNode High, ExprNode Max, bool Slice3, PositionNode Rbrack) : ExprNode(NodeType);

//type TypeAssertExprNode struct {
//	Node
//	X      IExprNode     `json:"X"`
//	Lparen *PositionNode `json:"Lparen,omitempty"`
//	Type   IExprNode     `json:"Type"`
//	Rparen *PositionNode `json:"Rparen,omitempty"`
//}
public record TypeAssertExprNode(string NodeType, ExprNode X, PositionNode Lparen, ExprNode Type, PositionNode Rparen) : ExprNode(NodeType);

//type CallExprNode struct {
//	Node
//	Fun      IExprNode     `json:"Fun"`
//	Lparen   *PositionNode `json:"Lparen,omitempty"`
//	Args     []IExprNode   `json:"Args"`
//	Ellipsis *PositionNode `json:"Ellipsis,omitempty"`
//	Rparen   *PositionNode `json:"Rparen,omitempty"`
//}
public record CallExprNode(string NodeType, ExprNode Fun, PositionNode Lparen, ExprNode[] Args, PositionNode Ellipsis, PositionNode Rparen) : ExprNode(NodeType);

//type StarExprNode struct {
//	Node
//	Star *PositionNode `json:"Star,omitempty"`
//	X    IExprNode     `json:"X"`
//}
public record StarExprNode(string NodeType, PositionNode Star, ExprNode X) : ExprNode(NodeType);

//type UnaryExprNode struct {
//	Node
//	OpPos *PositionNode `json:"OpPos,omitempty"`
//	Op    string        `json:"Op"`
//	X     IExprNode     `json:"X"`
//}
public record UnaryExprNode(string NodeType, PositionNode OpPos, string Op, ExprNode X) : ExprNode(NodeType);

//type BinaryExprNode struct {
//	Node
//	X     IExprNode     `json:"X"`
//	OpPos *PositionNode `json:"OpPos,omitempty"`
//	Op    string        `json:"Op"`
//	Y     IExprNode     `json:"Y"`
//}
public record BinaryExprNode(string NodeType, ExprNode X, PositionNode OpPos, string Op, ExprNode Y) : ExprNode(NodeType);

//type KeyValueExprNode struct {
//	Node
//	Key   IExprNode     `json:"Key"`
//	Colon *PositionNode `json:"Colon,omitempty"`
//	Value IExprNode     `json:"Value"`
//}
public record KeyValueExprNode(string NodeType, ExprNode Key, PositionNode Colon, ExprNode Value) : ExprNode(NodeType);

//// ---------------------------------------------------------------------------

//type ArrayTypeNode struct {
//	Node
//	Lbrack *PositionNode `json:"Lbrack,omitempty"`
//	Len    IExprNode     `json:"Len"`
//	Elt    IExprNode     `json:"Elt"`
//}
public record ArrayTypeNode(string NodeType, PositionNode Lbrack, ExprNode Len, ExprNode Elt) : ExprNode(NodeType);

//type StructTypeNode struct {
//	Node
//	Struct     *PositionNode  `json:"Struct,omitempty"`
//	Fields     *FieldListNode `json:"Fields"`
//	Incomplete bool           `json:"Incomplete"`
//}
public record StructTypeNode(string NodeType, PositionNode Struct, FieldListNode Fields, bool Incomplete) : ExprNode(NodeType);

//type FuncTypeNode struct {
//	Node
//	Func       *PositionNode  `json:"Func,omitempty"`
//	TypeParams *FieldListNode `json:"TypeParams"`
//	Params     *FieldListNode `json:"Params"`
//	Results    *FieldListNode `json:"Results"`
//}
public record FuncTypeNode(string NodeType, PositionNode Func, FieldListNode TypeParams, FieldListNode Params, FieldListNode Results) : ExprNode(NodeType);

//type InterfaceTypeNode struct {
//	Node
//	Interface  *PositionNode  `json:"Interface,omitempty"`
//	Methods    *FieldListNode `json:"Methods"`
//	Incomplete bool           `json:"Incomplete"`
//}
public record InterfaceTypeNode(string NodeType, PositionNode Interface, FieldListNode Methods, bool Incomplete) : ExprNode(NodeType);

//type MapTypeNode struct {
//	Node
//	Map   *PositionNode `json:"Map,omitempty"`
//	Key   IExprNode     `json:"Key"`
//	Value IExprNode     `json:"Value"`
//}
public record MapTypeNode(string NodeType, PositionNode Map, ExprNode Key, ExprNode Value) : ExprNode(NodeType);

//type ChanTypeNode struct {
//	Node
//	Begin *PositionNode `json:"Begin,omitempty"`
//	Arrow *PositionNode `json:"Arrow,omitempty"`
//	Dir   string        `json:"Dir"`
//	Value IExprNode     `json:"Value"`
//}
public record ChanTypeNode(string NodeType, PositionNode Begin, PositionNode Arrow, string Dir, ExprNode Value) : ExprNode(NodeType);

//// ---------------------------------------------------------------------------

//type BadStmtNode struct {
//	Node
//	From *PositionNode `json:"From,omitempty"`
//	To   *PositionNode `json:"To,omitempty"`
//}
public record BadStmtNode(string NodeType, PositionNode From, PositionNode To) : StmtNode(NodeType);

//type DeclStmtNode struct {
//	Node
//	Decl IDeclNode `json:"Decl"`
//}
public record DeclStmtNode(string NodeType, DeclNode Decl) : StmtNode(NodeType);

//type EmptyStmtNode struct {
//	Node
//	Semicolon *PositionNode `json:"Semicolon,omitempty"`
//	Implicit  bool          `json:"Implicit"`
//}
public record EmptyStmtNode(string NodeType, PositionNode Semicolon, bool Implicit) : StmtNode(NodeType);

//type LabeledStmtNode struct {
//	Node
//	Label *IdentNode    `json:"Label"`
//	Colon *PositionNode `json:"Colon,omitempty"`
//	Stmt  IStmtNode     `json:"Stmt"`
//}
public record LabeledStmtNode(string NodeType, IdentNode Label, PositionNode Colon, StmtNode Stmt) : StmtNode(NodeType);

//type ExprStmtNode struct {
//	Node
//	X IExprNode `json:"X,omitempty"`
//}
public record ExprStmtNode(string NodeType, ExprNode X) : StmtNode(NodeType);

//type SendStmtNode struct {
//	Node
//	Chan  IExprNode     `json:"Chan"`
//	Arrow *PositionNode `json:"Arrow,omitempty"`
//	Value IExprNode     `json:"Value"`
//}
public record SendStmtNode(string NodeType, ExprNode Chan, PositionNode Arrow, ExprNode Value) : StmtNode(NodeType);

//type IncDecStmtNode struct {
//	Node
//	X      IExprNode     `json:"X"`
//	TokPos *PositionNode `json:"TokPos,omitempty"`
//	Tok    string        `json:"Tok"`
//}
public record IncDecStmtNode(string NodeType, ExprNode X, PositionNode TokPos, string Tok) : StmtNode(NodeType);

//type AssignStmtNode struct {
//	Node
//	Lhs    []IExprNode   `json:"Lhs"`
//	TokPos *PositionNode `json:"TokPos,omitempty"`
//	Tok    string        `json:"Tok"`
//	Rhs    []IExprNode   `json:"Rhs"`
//}
public record AssignStmtNode(string NodeType, ExprNode[] Lhs, PositionNode TokPos, string Tok, ExprNode[] Rhs) : StmtNode(NodeType);

//type GoStmtNode struct {
//	Node
//	Go   *PositionNode `json:"Go,omitempty"`
//	Call *CallExprNode `json:"Call"`
//}
public record GoStmtNode(string NodeType, PositionNode Go, CallExprNode Call) : StmtNode(NodeType);

//type DeferStmtNode struct {
//	Node
//	Defer *PositionNode `json:"Defer,omitempty"`
//	Call  *CallExprNode `json:"Call"`
//}
public record DeferStmtNode(string NodeType, PositionNode Defer, CallExprNode Call) : StmtNode(NodeType);

//type ReturnStmtNode struct {
//	Node
//	Return  *PositionNode `json:"Return,omitempty"`
//	Results []IExprNode   `json:"Results"`
//}
public record ReturnStmtNode(string NodeType, PositionNode Return, ExprNode[] Results) : StmtNode(NodeType);

//type BranchStmtNode struct {
//	Node
//	TokPos *PositionNode `json:"TokPos,omitempty"`
//	Tok    string        `json:"Tok"`
//	Label  *IdentNode    `json:"Label"`
//}
public record BranchStmtNode(string NodeType, PositionNode TokPos, string Tok, IdentNode Label) : StmtNode(NodeType);

//type BlockStmtNode struct {
//	Node
//	Lbrace *PositionNode `json:"Lbrace,omitempty"`
//	List   []IStmtNode   `json:"List"`
//	Rbrace *PositionNode `json:"Rbrace,omitempty"`
//}
public record BlockStmtNode(string NodeType, PositionNode Lbrace, StmtNode[] List, PositionNode Rbrace) : StmtNode(NodeType);

//type IfStmtNode struct {
//	Node
//	If   *PositionNode  `json:"If,omitempty"`
//	Init IStmtNode      `json:"Init"`
//	Cond IExprNode      `json:"Cond"`
//	Body *BlockStmtNode `json:"Body"`
//	Else IStmtNode      `json:"Else"`
//}
public record IfStmtNode(string NodeType, PositionNode If, StmtNode Init, ExprNode Cond, BlockStmtNode Body, StmtNode Else) : StmtNode(NodeType);

//type CaseClauseNode struct {
//	Node
//	Case  *PositionNode `json:"Case,omitempty"`
//	List  []IExprNode   `json:"List"`
//	Colon *PositionNode `json:"Colon,omitempty"`
//	Body  []IStmtNode   `json:"Body"`
//}
public record CaseClauseNode(string NodeType, PositionNode Case, ExprNode[] List, PositionNode Colon, StmtNode[] Body) : StmtNode(NodeType);

//type SwitchStmtNode struct {
//	Node
//	Switch *PositionNode  `json:"Switch,omitempty"`
//	Init   IStmtNode      `json:"Init"`
//	Tag    IExprNode      `json:"Tag"`
//	Body   *BlockStmtNode `json:"Body"`
//}
public record SwitchStmtNode(string NodeType, PositionNode Switch, StmtNode Init, ExprNode Tag, BlockStmtNode Body) : StmtNode(NodeType);

//type TypeSwitchStmtNode struct {
//	Node
//	Switch *PositionNode  `json:"Switch,omitempty"`
//	Init   IStmtNode      `json:"Init"`
//	Assign IStmtNode      `json:"Assign"`
//	Body   *BlockStmtNode `json:"Body"`
//}
public record TypeSwitchStmtNode(string NodeType, PositionNode Switch, StmtNode Init, StmtNode Assign, BlockStmtNode Body) : StmtNode(NodeType);

//type CommClauseNode struct {
//	Node
//	Case  *PositionNode `json:"Case,omitempty"`
//	Comm  IStmtNode     `json:"Comm"`
//	Colon *PositionNode `json:"Colon,omitempty"`
//	Body  []IStmtNode   `json:"Body"`
//}
public record CommClauseNode(string NodeType, PositionNode Case, StmtNode Comm, PositionNode Colon, StmtNode[] Body) : StmtNode(NodeType);

//type SelectStmtNode struct {
//	Node
//	Select *PositionNode  `json:"Select,omitempty"`
//	Body   *BlockStmtNode `json:"Body"`
//}
public record SelectStmtNode(string NodeType, PositionNode Select, BlockStmtNode Body) : StmtNode(NodeType);

//type ForStmtNode struct {
//	Node
//	For  *PositionNode  `json:"For,omitempty"`
//	Init IStmtNode      `json:"Init"`
//	Cond IExprNode      `json:"Cond"`
//	Post IStmtNode      `json:"Post"`
//	Body *BlockStmtNode `json:"Body"`
//}
public record ForStmtNode(string NodeType, PositionNode For, StmtNode Init, ExprNode Cond, StmtNode Post, BlockStmtNode Body) : StmtNode(NodeType);

//type RangeStmtNode struct {
//	Node
//	For    *PositionNode  `json:"For,omitempty"`
//	Key    IExprNode      `json:"Key"`
//	Value  IExprNode      `json:"Value"`
//	TokPos *PositionNode  `json:"TokPos,omitempty"`
//	Tok    string         `json:"Tok"`
//	X      IExprNode      `json:"X"`
//	Body   *BlockStmtNode `json:"Body"`
//}
public record RangeStmtNode(string NodeType, PositionNode For, ExprNode Key, ExprNode Value, PositionNode TokPos, string Tok, ExprNode X, BlockStmtNode Body) : StmtNode(NodeType);

//// ---------------------------------------------------------------------------

//type ImportSpecNode struct {
//	Node
//	Doc     *CommentGroupNode `json:"Doc,omitempty"`
//	Name    *IdentNode        `json:"Name"`
//	Path    *BasicLitNode     `json:"Path"`
//	Comment *CommentGroupNode `json:"Comment,omitempty"`
//	EndPos  *PositionNode     `json:"EndPos,omitempty"`
//}
public record ImportSpecNode(string NodeType, CommentGroupNode Doc, IdentNode Name, BasicLitNode Path, CommentGroupNode Comment, PositionNode EndPos) : SpecNode(NodeType);

//type ValueSpecNode struct {
//	Node
//	Doc     *CommentGroupNode `json:"Doc,omitempty"`
//	Names   []*IdentNode      `json:"Names"`
//	Type    IExprNode         `json:"Type"`
//	Values  []IExprNode       `json:"Values"`
//	Comment *CommentGroupNode `json:"Comment,omitempty"`
//}
public record ValueSpecNode(string NodeType, CommentGroupNode Doc, IdentNode[] Names, ExprNode Type, ExprNode[] Values, CommentGroupNode Comment) : SpecNode(NodeType);

//type TypeSpecNode struct {
//	Node
//	Doc        *CommentGroupNode `json:"Doc,omitempty"`
//	Name       *IdentNode        `json:"Name"`
//	TypeParams *FieldListNode    `json:"TypeParams"`
//	Assign     *PositionNode     `json:"Assign,omitempty"`
//	Type       IExprNode         `json:"Type"`
//	Comment    *CommentGroupNode `json:"Comment,omitempty"`
//}
public record TypeSpecNode(string NodeType, CommentGroupNode Doc, IdentNode Name, FieldListNode TypeParams, PositionNode Assign, ExprNode Type, CommentGroupNode Comment) : SpecNode(NodeType);

//// ---------------------------------------------------------------------------

//type BadDeclNode struct {
//	Node
//	From *PositionNode `json:"From,omitempty"`
//	To   *PositionNode `json:"To,omitempty"`
//}
public record BadDeclNode(string NodeType, PositionNode From, PositionNode To) : DeclNode(NodeType);

//type GenDeclNode struct {
//	Node
//	Doc    *CommentGroupNode `json:"Doc,omitempty"`
//	TokPos *PositionNode     `json:"TokPos,omitempty"`
//	Tok    string            `json:"Tok"`
//	Lparen *PositionNode     `json:"Lparen,omitempty"`
//	Specs  []ISpecNode       `json:"Specs"`
//	Rparen *PositionNode     `json:"Rparen,omitempty"`
//}
public record GenDeclNode(string NodeType, CommentGroupNode Doc, PositionNode TokPos, string Tok, PositionNode Lparen, SpecNode[] Specs, PositionNode Rparen) : DeclNode(NodeType);

//type FuncDeclNode struct {
//	Node
//	Doc  *CommentGroupNode `json:"Doc,omitempty"`
//	Recv *FieldListNode    `json:"Recv"`
//	Name *IdentNode        `json:"Name"`
//	Type *FuncTypeNode     `json:"Type"`
//	Body *BlockStmtNode    `json:"Body"`
//}
public record FuncDeclNode(string NodeType, CommentGroupNode Doc, FieldListNode Recv, IdentNode Name, FuncTypeNode Type, BlockStmtNode Body) : DeclNode(NodeType);

//// ---------------------------------------------------------------------------

//type FileNode struct {
//	Node
//	Doc        *CommentGroupNode   `json:"Doc,omitempty"`
//	Package    *PositionNode       `json:"Package,omitempty"`
//	Name       *IdentNode          `json:"Name"`
//	Decls      []IDeclNode         `json:"Decls,omitempty"`
//	Imports    []*ImportSpecNode   `json:"Imports,omitempty"`
//	Unresolved []*IdentNode        `json:"Unresolved,omitempty"`
//	Comments   []*CommentGroupNode `json:"Comments,omitempty"`
//	FileSet    *token.FileSet      `json:"FileSet,omitempty"`
//	//	Scope      *Scope
//}
public record FileNode(string NodeType, CommentGroupNode Doc, PositionNode Package, IdentNode Name, DeclNode[] Decls, ImportSpecNode[] Imports, IdentNode[] Unresolved, CommentGroupNode[] Comments) : Node(NodeType);

//type PackageNode struct {
//	Node
//	Name string
//	//	Scope   *Scope
//	//	Imports map[string]*Object
//	Files map[string]*FileNode
//}
public record PackageNode(string NodeType, string Name, Dictionary<string, FileNode> Files) : Node(NodeType);
