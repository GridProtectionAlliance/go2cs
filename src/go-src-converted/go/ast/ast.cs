// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ast declares the types used to represent syntax trees for Go
// packages.
//
// package ast -- go2cs converted at 2020 August 29 08:46:42 UTC
// import "go/ast" ==> using ast = go.go.ast_package
// Original source: C:\Go\src\go\ast\ast.go
using token = go.go.token_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class ast_package
    {
        // ----------------------------------------------------------------------------
        // Interfaces
        //
        // There are 3 main classes of nodes: Expressions and type nodes,
        // statement nodes, and declaration nodes. The node names usually
        // match the corresponding Go spec production names to which they
        // correspond. The node fields correspond to the individual parts
        // of the respective productions.
        //
        // All nodes contain position information marking the beginning of
        // the corresponding source text segment; it is accessible via the
        // Pos accessor method. Nodes may contain additional position info
        // for language constructs where comments may be found between parts
        // of the construct (typically any larger, parenthesized subpart).
        // That position information is needed to properly position comments
        // when printing the construct.

        // All node types implement the Node interface.
        public partial interface Node
        {
            token.Pos Pos(); // position of first character belonging to the node
            token.Pos End(); // position of first character immediately after the node
        }

        // All expression nodes implement the Expr interface.
        public partial interface Expr : Node
        {
            void exprNode();
        }

        // All statement nodes implement the Stmt interface.
        public partial interface Stmt : Node
        {
            void stmtNode();
        }

        // All declaration nodes implement the Decl interface.
        public partial interface Decl : Node
        {
            void declNode();
        }

        // ----------------------------------------------------------------------------
        // Comments

        // A Comment node represents a single //-style or /*-style comment.
        public partial struct Comment
        {
            public token.Pos Slash; // position of "/" starting the comment
            public @string Text; // comment text (excluding '\n' for //-style comments)
        }

        private static token.Pos Pos(this ref Comment c)
        {
            return c.Slash;
        }
        private static token.Pos End(this ref Comment c)
        {
            return token.Pos(int(c.Slash) + len(c.Text));
        }

        // A CommentGroup represents a sequence of comments
        // with no other tokens and no empty lines between.
        //
        public partial struct CommentGroup
        {
            public slice<ref Comment> List; // len(List) > 0
        }

        private static token.Pos Pos(this ref CommentGroup g)
        {
            return g.List[0L].Pos();
        }
        private static token.Pos End(this ref CommentGroup g)
        {
            return g.List[len(g.List) - 1L].End();
        }

        private static bool isWhitespace(byte ch)
        {
            return ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r';
        }

        private static @string stripTrailingWhitespace(@string s)
        {
            var i = len(s);
            while (i > 0L && isWhitespace(s[i - 1L]))
            {
                i--;
            }

            return s[0L..i];
        }

        // Text returns the text of the comment.
        // Comment markers (//, /*, and */), the first space of a line comment, and
        // leading and trailing empty lines are removed. Multiple empty lines are
        // reduced to one, and trailing space on lines is trimmed. Unless the result
        // is empty, it is newline-terminated.
        //
        private static @string Text(this ref CommentGroup g)
        {
            if (g == null)
            {
                return "";
            }
            var comments = make_slice<@string>(len(g.List));
            {
                var c__prev1 = c;

                foreach (var (__i, __c) in g.List)
                {
                    i = __i;
                    c = __c;
                    comments[i] = c.Text;
                }

                c = c__prev1;
            }

            var lines = make_slice<@string>(0L, 10L); // most comments are less than 10 lines
            {
                var c__prev1 = c;

                foreach (var (_, __c) in comments)
                {
                    c = __c; 
                    // Remove comment markers.
                    // The parser has given us exactly the comment text.
                    switch (c[1L])
                    {
                        case '/': 
                            //-style comment (no newline at the end)
                            c = c[2L..]; 
                            // strip first space - required for Example tests
                            if (len(c) > 0L && c[0L] == ' ')
                            {
                                c = c[1L..];
                            }
                            break;
                        case '*': 
                            /*-style comment */
                            c = c[2L..len(c) - 2L];
                            break;
                    } 

                    // Split on newlines.
                    var cl = strings.Split(c, "\n"); 

                    // Walk lines, stripping trailing white space and adding to list.
                    foreach (var (_, l) in cl)
                    {
                        lines = append(lines, stripTrailingWhitespace(l));
                    }
                } 

                // Remove leading blank lines; convert runs of
                // interior blank lines to a single blank line.

                c = c__prev1;
            }

            long n = 0L;
            foreach (var (_, line) in lines)
            {
                if (line != "" || n > 0L && lines[n - 1L] != "")
                {
                    lines[n] = line;
                    n++;
                }
            }
            lines = lines[0L..n]; 

            // Add final "" entry to get trailing newline from Join.
            if (n > 0L && lines[n - 1L] != "")
            {
                lines = append(lines, "");
            }
            return strings.Join(lines, "\n");
        }

        // ----------------------------------------------------------------------------
        // Expressions and types

        // A Field represents a Field declaration list in a struct type,
        // a method list in an interface type, or a parameter/result declaration
        // in a signature.
        //
        public partial struct Field
        {
            public ptr<CommentGroup> Doc; // associated documentation; or nil
            public slice<ref Ident> Names; // field/method/parameter names; or nil if anonymous field
            public Expr Type; // field/method/parameter type
            public ptr<BasicLit> Tag; // field tag; or nil
            public ptr<CommentGroup> Comment; // line comments; or nil
        }

        private static token.Pos Pos(this ref Field f)
        {
            if (len(f.Names) > 0L)
            {
                return f.Names[0L].Pos();
            }
            return f.Type.Pos();
        }

        private static token.Pos End(this ref Field f)
        {
            if (f.Tag != null)
            {
                return f.Tag.End();
            }
            return f.Type.End();
        }

        // A FieldList represents a list of Fields, enclosed by parentheses or braces.
        public partial struct FieldList
        {
            public token.Pos Opening; // position of opening parenthesis/brace, if any
            public slice<ref Field> List; // field list; or nil
            public token.Pos Closing; // position of closing parenthesis/brace, if any
        }

        private static token.Pos Pos(this ref FieldList f)
        {
            if (f.Opening.IsValid())
            {
                return f.Opening;
            } 
            // the list should not be empty in this case;
            // be conservative and guard against bad ASTs
            if (len(f.List) > 0L)
            {
                return f.List[0L].Pos();
            }
            return token.NoPos;
        }

        private static token.Pos End(this ref FieldList f)
        {
            if (f.Closing.IsValid())
            {
                return f.Closing + 1L;
            } 
            // the list should not be empty in this case;
            // be conservative and guard against bad ASTs
            {
                var n = len(f.List);

                if (n > 0L)
                {
                    return f.List[n - 1L].End();
                }

            }
            return token.NoPos;
        }

        // NumFields returns the number of (named and anonymous fields) in a FieldList.
        private static long NumFields(this ref FieldList f)
        {
            long n = 0L;
            if (f != null)
            {
                foreach (var (_, g) in f.List)
                {
                    var m = len(g.Names);
                    if (m == 0L)
                    {
                        m = 1L; // anonymous field
                    }
                    n += m;
                }
            }
            return n;
        }

        // An expression is represented by a tree consisting of one
        // or more of the following concrete expression nodes.
        //
 
        // A BadExpr node is a placeholder for expressions containing
        // syntax errors for which no correct expression nodes can be
        // created.
        //
        public partial struct BadExpr
        {
            public token.Pos From; // position range of bad expression
            public token.Pos To; // position range of bad expression
        } 

        // An Ident node represents an identifier.
        public partial struct Ident
        {
            public token.Pos NamePos; // identifier position
            public @string Name; // identifier name
            public ptr<Object> Obj; // denoted object; or nil
        } 

        // An Ellipsis node stands for the "..." type in a
        // parameter list or the "..." length in an array type.
        //
        public partial struct Ellipsis
        {
            public token.Pos Ellipsis; // position of "..."
            public Expr Elt; // ellipsis element type (parameter lists only); or nil
        } 

        // A BasicLit node represents a literal of basic type.
        public partial struct BasicLit
        {
            public token.Pos ValuePos; // literal position
            public token.Token Kind; // token.INT, token.FLOAT, token.IMAG, token.CHAR, or token.STRING
            public @string Value; // literal string; e.g. 42, 0x7f, 3.14, 1e-9, 2.4i, 'a', '\x7f', "foo" or `\m\n\o`
        } 

        // A FuncLit node represents a function literal.
        public partial struct FuncLit
        {
            public ptr<FuncType> Type; // function type
            public ptr<BlockStmt> Body; // function body
        } 

        // A CompositeLit node represents a composite literal.
        public partial struct CompositeLit
        {
            public Expr Type; // literal type; or nil
            public token.Pos Lbrace; // position of "{"
            public slice<Expr> Elts; // list of composite elements; or nil
            public token.Pos Rbrace; // position of "}"
        } 

        // A ParenExpr node represents a parenthesized expression.
        public partial struct ParenExpr
        {
            public token.Pos Lparen; // position of "("
            public Expr X; // parenthesized expression
            public token.Pos Rparen; // position of ")"
        } 

        // A SelectorExpr node represents an expression followed by a selector.
        public partial struct SelectorExpr
        {
            public Expr X; // expression
            public ptr<Ident> Sel; // field selector
        } 

        // An IndexExpr node represents an expression followed by an index.
        public partial struct IndexExpr
        {
            public Expr X; // expression
            public token.Pos Lbrack; // position of "["
            public Expr Index; // index expression
            public token.Pos Rbrack; // position of "]"
        } 

        // An SliceExpr node represents an expression followed by slice indices.
        public partial struct SliceExpr
        {
            public Expr X; // expression
            public token.Pos Lbrack; // position of "["
            public Expr Low; // begin of slice range; or nil
            public Expr High; // end of slice range; or nil
            public Expr Max; // maximum capacity of slice; or nil
            public bool Slice3; // true if 3-index slice (2 colons present)
            public token.Pos Rbrack; // position of "]"
        } 

        // A TypeAssertExpr node represents an expression followed by a
        // type assertion.
        //
        public partial struct TypeAssertExpr
        {
            public Expr X; // expression
            public token.Pos Lparen; // position of "("
            public Expr Type; // asserted type; nil means type switch X.(type)
            public token.Pos Rparen; // position of ")"
        } 

        // A CallExpr node represents an expression followed by an argument list.
        public partial struct CallExpr
        {
            public Expr Fun; // function expression
            public token.Pos Lparen; // position of "("
            public slice<Expr> Args; // function arguments; or nil
            public token.Pos Ellipsis; // position of "..." (token.NoPos if there is no "...")
            public token.Pos Rparen; // position of ")"
        } 

        // A StarExpr node represents an expression of the form "*" Expression.
        // Semantically it could be a unary "*" expression, or a pointer type.
        //
        public partial struct StarExpr
        {
            public token.Pos Star; // position of "*"
            public Expr X; // operand
        } 

        // A UnaryExpr node represents a unary expression.
        // Unary "*" expressions are represented via StarExpr nodes.
        //
        public partial struct UnaryExpr
        {
            public token.Pos OpPos; // position of Op
            public token.Token Op; // operator
            public Expr X; // operand
        } 

        // A BinaryExpr node represents a binary expression.
        public partial struct BinaryExpr
        {
            public Expr X; // left operand
            public token.Pos OpPos; // position of Op
            public token.Token Op; // operator
            public Expr Y; // right operand
        } 

        // A KeyValueExpr node represents (key : value) pairs
        // in composite literals.
        //
        public partial struct KeyValueExpr
        {
            public Expr Key;
            public token.Pos Colon; // position of ":"
            public Expr Value;
        }        public partial struct ChanDir // : long
        {
        }

        public static readonly ChanDir SEND = 1L << (int)(iota);
        public static readonly var RECV = 0;

        // A type is represented by a tree consisting of one
        // or more of the following type-specific expression
        // nodes.
        //
 
        // An ArrayType node represents an array or slice type.
        public partial struct ArrayType
        {
            public token.Pos Lbrack; // position of "["
            public Expr Len; // Ellipsis node for [...]T array types, nil for slice types
            public Expr Elt; // element type
        } 

        // A StructType node represents a struct type.
        public partial struct StructType
        {
            public token.Pos Struct; // position of "struct" keyword
            public ptr<FieldList> Fields; // list of field declarations
            public bool Incomplete; // true if (source) fields are missing in the Fields list
        } 

        // Pointer types are represented via StarExpr nodes.

        // A FuncType node represents a function type.
        public partial struct FuncType
        {
            public token.Pos Func; // position of "func" keyword (token.NoPos if there is no "func")
            public ptr<FieldList> Params; // (incoming) parameters; non-nil
            public ptr<FieldList> Results; // (outgoing) results; or nil
        } 

        // An InterfaceType node represents an interface type.
        public partial struct InterfaceType
        {
            public token.Pos Interface; // position of "interface" keyword
            public ptr<FieldList> Methods; // list of methods
            public bool Incomplete; // true if (source) methods are missing in the Methods list
        } 

        // A MapType node represents a map type.
        public partial struct MapType
        {
            public token.Pos Map; // position of "map" keyword
            public Expr Key;
            public Expr Value;
        } 

        // A ChanType node represents a channel type.
        public partial struct ChanType
        {
            public token.Pos Begin; // position of "chan" keyword or "<-" (whichever comes first)
            public token.Pos Arrow; // position of "<-" (token.NoPos if there is no "<-")
            public ChanDir Dir; // channel direction
            public Expr Value; // value type
        }        private static token.Pos Pos(this ref BadExpr x)
        {
            return x.From;
        }
        private static token.Pos Pos(this ref Ident x)
        {
            return x.NamePos;
        }
        private static token.Pos Pos(this ref Ellipsis x)
        {
            return x.Ellipsis;
        }
        private static token.Pos Pos(this ref BasicLit x)
        {
            return x.ValuePos;
        }
        private static token.Pos Pos(this ref FuncLit x)
        {
            return x.Type.Pos();
        }
        private static token.Pos Pos(this ref CompositeLit x)
        {
            if (x.Type != null)
            {
                return x.Type.Pos();
            }
            return x.Lbrace;
        }
        private static token.Pos Pos(this ref ParenExpr x)
        {
            return x.Lparen;
        }
        private static token.Pos Pos(this ref SelectorExpr x)
        {
            return x.X.Pos();
        }
        private static token.Pos Pos(this ref IndexExpr x)
        {
            return x.X.Pos();
        }
        private static token.Pos Pos(this ref SliceExpr x)
        {
            return x.X.Pos();
        }
        private static token.Pos Pos(this ref TypeAssertExpr x)
        {
            return x.X.Pos();
        }
        private static token.Pos Pos(this ref CallExpr x)
        {
            return x.Fun.Pos();
        }
        private static token.Pos Pos(this ref StarExpr x)
        {
            return x.Star;
        }
        private static token.Pos Pos(this ref UnaryExpr x)
        {
            return x.OpPos;
        }
        private static token.Pos Pos(this ref BinaryExpr x)
        {
            return x.X.Pos();
        }
        private static token.Pos Pos(this ref KeyValueExpr x)
        {
            return x.Key.Pos();
        }
        private static token.Pos Pos(this ref ArrayType x)
        {
            return x.Lbrack;
        }
        private static token.Pos Pos(this ref StructType x)
        {
            return x.Struct;
        }
        private static token.Pos Pos(this ref FuncType x)
        {
            if (x.Func.IsValid() || x.Params == null)
            { // see issue 3870
                return x.Func;
            }
            return x.Params.Pos(); // interface method declarations have no "func" keyword
        }
        private static token.Pos Pos(this ref InterfaceType x)
        {
            return x.Interface;
        }
        private static token.Pos Pos(this ref MapType x)
        {
            return x.Map;
        }
        private static token.Pos Pos(this ref ChanType x)
        {
            return x.Begin;
        }

        private static token.Pos End(this ref BadExpr x)
        {
            return x.To;
        }
        private static token.Pos End(this ref Ident x)
        {
            return token.Pos(int(x.NamePos) + len(x.Name));
        }
        private static token.Pos End(this ref Ellipsis x)
        {
            if (x.Elt != null)
            {
                return x.Elt.End();
            }
            return x.Ellipsis + 3L; // len("...")
        }
        private static token.Pos End(this ref BasicLit x)
        {
            return token.Pos(int(x.ValuePos) + len(x.Value));
        }
        private static token.Pos End(this ref FuncLit x)
        {
            return x.Body.End();
        }
        private static token.Pos End(this ref CompositeLit x)
        {
            return x.Rbrace + 1L;
        }
        private static token.Pos End(this ref ParenExpr x)
        {
            return x.Rparen + 1L;
        }
        private static token.Pos End(this ref SelectorExpr x)
        {
            return x.Sel.End();
        }
        private static token.Pos End(this ref IndexExpr x)
        {
            return x.Rbrack + 1L;
        }
        private static token.Pos End(this ref SliceExpr x)
        {
            return x.Rbrack + 1L;
        }
        private static token.Pos End(this ref TypeAssertExpr x)
        {
            return x.Rparen + 1L;
        }
        private static token.Pos End(this ref CallExpr x)
        {
            return x.Rparen + 1L;
        }
        private static token.Pos End(this ref StarExpr x)
        {
            return x.X.End();
        }
        private static token.Pos End(this ref UnaryExpr x)
        {
            return x.X.End();
        }
        private static token.Pos End(this ref BinaryExpr x)
        {
            return x.Y.End();
        }
        private static token.Pos End(this ref KeyValueExpr x)
        {
            return x.Value.End();
        }
        private static token.Pos End(this ref ArrayType x)
        {
            return x.Elt.End();
        }
        private static token.Pos End(this ref StructType x)
        {
            return x.Fields.End();
        }
        private static token.Pos End(this ref FuncType x)
        {
            if (x.Results != null)
            {
                return x.Results.End();
            }
            return x.Params.End();
        }
        private static token.Pos End(this ref InterfaceType x)
        {
            return x.Methods.End();
        }
        private static token.Pos End(this ref MapType x)
        {
            return x.Value.End();
        }
        private static token.Pos End(this ref ChanType x)
        {
            return x.Value.End();
        }

        // exprNode() ensures that only expression/type nodes can be
        // assigned to an Expr.
        //
        private static void exprNode(this ref BadExpr _p0)
        {
        }
        private static void exprNode(this ref Ident _p0)
        {
        }
        private static void exprNode(this ref Ellipsis _p0)
        {
        }
        private static void exprNode(this ref BasicLit _p0)
        {
        }
        private static void exprNode(this ref FuncLit _p0)
        {
        }
        private static void exprNode(this ref CompositeLit _p0)
        {
        }
        private static void exprNode(this ref ParenExpr _p0)
        {
        }
        private static void exprNode(this ref SelectorExpr _p0)
        {
        }
        private static void exprNode(this ref IndexExpr _p0)
        {
        }
        private static void exprNode(this ref SliceExpr _p0)
        {
        }
        private static void exprNode(this ref TypeAssertExpr _p0)
        {
        }
        private static void exprNode(this ref CallExpr _p0)
        {
        }
        private static void exprNode(this ref StarExpr _p0)
        {
        }
        private static void exprNode(this ref UnaryExpr _p0)
        {
        }
        private static void exprNode(this ref BinaryExpr _p0)
        {
        }
        private static void exprNode(this ref KeyValueExpr _p0)
        {
        }

        private static void exprNode(this ref ArrayType _p0)
        {
        }
        private static void exprNode(this ref StructType _p0)
        {
        }
        private static void exprNode(this ref FuncType _p0)
        {
        }
        private static void exprNode(this ref InterfaceType _p0)
        {
        }
        private static void exprNode(this ref MapType _p0)
        {
        }
        private static void exprNode(this ref ChanType _p0)
        {
        }

        // ----------------------------------------------------------------------------
        // Convenience functions for Idents

        // NewIdent creates a new Ident without position.
        // Useful for ASTs generated by code other than the Go parser.
        //
        public static ref Ident NewIdent(@string name)
        {
            return ref new Ident(token.NoPos,name,nil);
        }

        // IsExported reports whether name is an exported Go symbol
        // (that is, whether it begins with an upper-case letter).
        //
        public static bool IsExported(@string name)
        {
            var (ch, _) = utf8.DecodeRuneInString(name);
            return unicode.IsUpper(ch);
        }

        // IsExported reports whether id is an exported Go symbol
        // (that is, whether it begins with an uppercase letter).
        //
        private static bool IsExported(this ref Ident id)
        {
            return IsExported(id.Name);
        }

        private static @string String(this ref Ident id)
        {
            if (id != null)
            {
                return id.Name;
            }
            return "<nil>";
        }

        // ----------------------------------------------------------------------------
        // Statements

        // A statement is represented by a tree consisting of one
        // or more of the following concrete statement nodes.
        //
 
        // A BadStmt node is a placeholder for statements containing
        // syntax errors for which no correct statement nodes can be
        // created.
        //
        public partial struct BadStmt
        {
            public token.Pos From; // position range of bad statement
            public token.Pos To; // position range of bad statement
        } 

        // A DeclStmt node represents a declaration in a statement list.
        public partial struct DeclStmt
        {
            public Decl Decl; // *GenDecl with CONST, TYPE, or VAR token
        } 

        // An EmptyStmt node represents an empty statement.
        // The "position" of the empty statement is the position
        // of the immediately following (explicit or implicit) semicolon.
        //
        public partial struct EmptyStmt
        {
            public token.Pos Semicolon; // position of following ";"
            public bool Implicit; // if set, ";" was omitted in the source
        } 

        // A LabeledStmt node represents a labeled statement.
        public partial struct LabeledStmt
        {
            public ptr<Ident> Label;
            public token.Pos Colon; // position of ":"
            public Stmt Stmt;
        } 

        // An ExprStmt node represents a (stand-alone) expression
        // in a statement list.
        //
        public partial struct ExprStmt
        {
            public Expr X; // expression
        } 

        // A SendStmt node represents a send statement.
        public partial struct SendStmt
        {
            public Expr Chan;
            public token.Pos Arrow; // position of "<-"
            public Expr Value;
        } 

        // An IncDecStmt node represents an increment or decrement statement.
        public partial struct IncDecStmt
        {
            public Expr X;
            public token.Pos TokPos; // position of Tok
            public token.Token Tok; // INC or DEC
        } 

        // An AssignStmt node represents an assignment or
        // a short variable declaration.
        //
        public partial struct AssignStmt
        {
            public slice<Expr> Lhs;
            public token.Pos TokPos; // position of Tok
            public token.Token Tok; // assignment token, DEFINE
            public slice<Expr> Rhs;
        } 

        // A GoStmt node represents a go statement.
        public partial struct GoStmt
        {
            public token.Pos Go; // position of "go" keyword
            public ptr<CallExpr> Call;
        } 

        // A DeferStmt node represents a defer statement.
        public partial struct DeferStmt
        {
            public token.Pos Defer; // position of "defer" keyword
            public ptr<CallExpr> Call;
        } 

        // A ReturnStmt node represents a return statement.
        public partial struct ReturnStmt
        {
            public token.Pos Return; // position of "return" keyword
            public slice<Expr> Results; // result expressions; or nil
        } 

        // A BranchStmt node represents a break, continue, goto,
        // or fallthrough statement.
        //
        public partial struct BranchStmt
        {
            public token.Pos TokPos; // position of Tok
            public token.Token Tok; // keyword token (BREAK, CONTINUE, GOTO, FALLTHROUGH)
            public ptr<Ident> Label; // label name; or nil
        } 

        // A BlockStmt node represents a braced statement list.
        public partial struct BlockStmt
        {
            public token.Pos Lbrace; // position of "{"
            public slice<Stmt> List;
            public token.Pos Rbrace; // position of "}"
        } 

        // An IfStmt node represents an if statement.
        public partial struct IfStmt
        {
            public token.Pos If; // position of "if" keyword
            public Stmt Init; // initialization statement; or nil
            public Expr Cond; // condition
            public ptr<BlockStmt> Body;
            public Stmt Else; // else branch; or nil
        } 

        // A CaseClause represents a case of an expression or type switch statement.
        public partial struct CaseClause
        {
            public token.Pos Case; // position of "case" or "default" keyword
            public slice<Expr> List; // list of expressions or types; nil means default case
            public token.Pos Colon; // position of ":"
            public slice<Stmt> Body; // statement list; or nil
        } 

        // A SwitchStmt node represents an expression switch statement.
        public partial struct SwitchStmt
        {
            public token.Pos Switch; // position of "switch" keyword
            public Stmt Init; // initialization statement; or nil
            public Expr Tag; // tag expression; or nil
            public ptr<BlockStmt> Body; // CaseClauses only
        } 

        // An TypeSwitchStmt node represents a type switch statement.
        public partial struct TypeSwitchStmt
        {
            public token.Pos Switch; // position of "switch" keyword
            public Stmt Init; // initialization statement; or nil
            public Stmt Assign; // x := y.(type) or y.(type)
            public ptr<BlockStmt> Body; // CaseClauses only
        } 

        // A CommClause node represents a case of a select statement.
        public partial struct CommClause
        {
            public token.Pos Case; // position of "case" or "default" keyword
            public Stmt Comm; // send or receive statement; nil means default case
            public token.Pos Colon; // position of ":"
            public slice<Stmt> Body; // statement list; or nil
        } 

        // An SelectStmt node represents a select statement.
        public partial struct SelectStmt
        {
            public token.Pos Select; // position of "select" keyword
            public ptr<BlockStmt> Body; // CommClauses only
        } 

        // A ForStmt represents a for statement.
        public partial struct ForStmt
        {
            public token.Pos For; // position of "for" keyword
            public Stmt Init; // initialization statement; or nil
            public Expr Cond; // condition; or nil
            public Stmt Post; // post iteration statement; or nil
            public ptr<BlockStmt> Body;
        } 

        // A RangeStmt represents a for statement with a range clause.
        public partial struct RangeStmt
        {
            public token.Pos For; // position of "for" keyword
            public Expr Key; // Key, Value may be nil
            public Expr Value; // Key, Value may be nil
            public token.Pos TokPos; // position of Tok; invalid if Key == nil
            public token.Token Tok; // ILLEGAL if Key == nil, ASSIGN, DEFINE
            public Expr X; // value to range over
            public ptr<BlockStmt> Body;
        }        private static token.Pos Pos(this ref BadStmt s)
        {
            return s.From;
        }
        private static token.Pos Pos(this ref DeclStmt s)
        {
            return s.Decl.Pos();
        }
        private static token.Pos Pos(this ref EmptyStmt s)
        {
            return s.Semicolon;
        }
        private static token.Pos Pos(this ref LabeledStmt s)
        {
            return s.Label.Pos();
        }
        private static token.Pos Pos(this ref ExprStmt s)
        {
            return s.X.Pos();
        }
        private static token.Pos Pos(this ref SendStmt s)
        {
            return s.Chan.Pos();
        }
        private static token.Pos Pos(this ref IncDecStmt s)
        {
            return s.X.Pos();
        }
        private static token.Pos Pos(this ref AssignStmt s)
        {
            return s.Lhs[0L].Pos();
        }
        private static token.Pos Pos(this ref GoStmt s)
        {
            return s.Go;
        }
        private static token.Pos Pos(this ref DeferStmt s)
        {
            return s.Defer;
        }
        private static token.Pos Pos(this ref ReturnStmt s)
        {
            return s.Return;
        }
        private static token.Pos Pos(this ref BranchStmt s)
        {
            return s.TokPos;
        }
        private static token.Pos Pos(this ref BlockStmt s)
        {
            return s.Lbrace;
        }
        private static token.Pos Pos(this ref IfStmt s)
        {
            return s.If;
        }
        private static token.Pos Pos(this ref CaseClause s)
        {
            return s.Case;
        }
        private static token.Pos Pos(this ref SwitchStmt s)
        {
            return s.Switch;
        }
        private static token.Pos Pos(this ref TypeSwitchStmt s)
        {
            return s.Switch;
        }
        private static token.Pos Pos(this ref CommClause s)
        {
            return s.Case;
        }
        private static token.Pos Pos(this ref SelectStmt s)
        {
            return s.Select;
        }
        private static token.Pos Pos(this ref ForStmt s)
        {
            return s.For;
        }
        private static token.Pos Pos(this ref RangeStmt s)
        {
            return s.For;
        }

        private static token.Pos End(this ref BadStmt s)
        {
            return s.To;
        }
        private static token.Pos End(this ref DeclStmt s)
        {
            return s.Decl.End();
        }
        private static token.Pos End(this ref EmptyStmt s)
        {
            if (s.Implicit)
            {
                return s.Semicolon;
            }
            return s.Semicolon + 1L; /* len(";") */
        }
        private static token.Pos End(this ref LabeledStmt s)
        {
            return s.Stmt.End();
        }
        private static token.Pos End(this ref ExprStmt s)
        {
            return s.X.End();
        }
        private static token.Pos End(this ref SendStmt s)
        {
            return s.Value.End();
        }
        private static token.Pos End(this ref IncDecStmt s)
        {
            return s.TokPos + 2L; /* len("++") */
        }
        private static token.Pos End(this ref AssignStmt s)
        {
            return s.Rhs[len(s.Rhs) - 1L].End();
        }
        private static token.Pos End(this ref GoStmt s)
        {
            return s.Call.End();
        }
        private static token.Pos End(this ref DeferStmt s)
        {
            return s.Call.End();
        }
        private static token.Pos End(this ref ReturnStmt s)
        {
            {
                var n = len(s.Results);

                if (n > 0L)
                {
                    return s.Results[n - 1L].End();
                }

            }
            return s.Return + 6L; // len("return")
        }
        private static token.Pos End(this ref BranchStmt s)
        {
            if (s.Label != null)
            {
                return s.Label.End();
            }
            return token.Pos(int(s.TokPos) + len(s.Tok.String()));
        }
        private static token.Pos End(this ref BlockStmt s)
        {
            return s.Rbrace + 1L;
        }
        private static token.Pos End(this ref IfStmt s)
        {
            if (s.Else != null)
            {
                return s.Else.End();
            }
            return s.Body.End();
        }
        private static token.Pos End(this ref CaseClause s)
        {
            {
                var n = len(s.Body);

                if (n > 0L)
                {
                    return s.Body[n - 1L].End();
                }

            }
            return s.Colon + 1L;
        }
        private static token.Pos End(this ref SwitchStmt s)
        {
            return s.Body.End();
        }
        private static token.Pos End(this ref TypeSwitchStmt s)
        {
            return s.Body.End();
        }
        private static token.Pos End(this ref CommClause s)
        {
            {
                var n = len(s.Body);

                if (n > 0L)
                {
                    return s.Body[n - 1L].End();
                }

            }
            return s.Colon + 1L;
        }
        private static token.Pos End(this ref SelectStmt s)
        {
            return s.Body.End();
        }
        private static token.Pos End(this ref ForStmt s)
        {
            return s.Body.End();
        }
        private static token.Pos End(this ref RangeStmt s)
        {
            return s.Body.End();
        }

        // stmtNode() ensures that only statement nodes can be
        // assigned to a Stmt.
        //
        private static void stmtNode(this ref BadStmt _p0)
        {
        }
        private static void stmtNode(this ref DeclStmt _p0)
        {
        }
        private static void stmtNode(this ref EmptyStmt _p0)
        {
        }
        private static void stmtNode(this ref LabeledStmt _p0)
        {
        }
        private static void stmtNode(this ref ExprStmt _p0)
        {
        }
        private static void stmtNode(this ref SendStmt _p0)
        {
        }
        private static void stmtNode(this ref IncDecStmt _p0)
        {
        }
        private static void stmtNode(this ref AssignStmt _p0)
        {
        }
        private static void stmtNode(this ref GoStmt _p0)
        {
        }
        private static void stmtNode(this ref DeferStmt _p0)
        {
        }
        private static void stmtNode(this ref ReturnStmt _p0)
        {
        }
        private static void stmtNode(this ref BranchStmt _p0)
        {
        }
        private static void stmtNode(this ref BlockStmt _p0)
        {
        }
        private static void stmtNode(this ref IfStmt _p0)
        {
        }
        private static void stmtNode(this ref CaseClause _p0)
        {
        }
        private static void stmtNode(this ref SwitchStmt _p0)
        {
        }
        private static void stmtNode(this ref TypeSwitchStmt _p0)
        {
        }
        private static void stmtNode(this ref CommClause _p0)
        {
        }
        private static void stmtNode(this ref SelectStmt _p0)
        {
        }
        private static void stmtNode(this ref ForStmt _p0)
        {
        }
        private static void stmtNode(this ref RangeStmt _p0)
        {
        }

        // ----------------------------------------------------------------------------
        // Declarations

        // A Spec node represents a single (non-parenthesized) import,
        // constant, type, or variable declaration.
        //
 
        // The Spec type stands for any of *ImportSpec, *ValueSpec, and *TypeSpec.
        public partial interface Spec : Node
        {
            void specNode();
        } 

        // An ImportSpec node represents a single package import.
        public partial struct ImportSpec
        {
            public ptr<CommentGroup> Doc; // associated documentation; or nil
            public ptr<Ident> Name; // local package name (including "."); or nil
            public ptr<BasicLit> Path; // import path
            public ptr<CommentGroup> Comment; // line comments; or nil
            public token.Pos EndPos; // end of spec (overrides Path.Pos if nonzero)
        } 

        // A ValueSpec node represents a constant or variable declaration
        // (ConstSpec or VarSpec production).
        //
        public partial struct ValueSpec
        {
            public ptr<CommentGroup> Doc; // associated documentation; or nil
            public slice<ref Ident> Names; // value names (len(Names) > 0)
            public Expr Type; // value type; or nil
            public slice<Expr> Values; // initial values; or nil
            public ptr<CommentGroup> Comment; // line comments; or nil
        } 

        // A TypeSpec node represents a type declaration (TypeSpec production).
        public partial struct TypeSpec
        {
            public ptr<CommentGroup> Doc; // associated documentation; or nil
            public ptr<Ident> Name; // type name
            public token.Pos Assign; // position of '=', if any
            public Expr Type; // *Ident, *ParenExpr, *SelectorExpr, *StarExpr, or any of the *XxxTypes
            public ptr<CommentGroup> Comment; // line comments; or nil
        }        private static token.Pos Pos(this ref ImportSpec s)
        {
            if (s.Name != null)
            {
                return s.Name.Pos();
            }
            return s.Path.Pos();
        }
        private static token.Pos Pos(this ref ValueSpec s)
        {
            return s.Names[0L].Pos();
        }
        private static token.Pos Pos(this ref TypeSpec s)
        {
            return s.Name.Pos();
        }

        private static token.Pos End(this ref ImportSpec s)
        {
            if (s.EndPos != 0L)
            {
                return s.EndPos;
            }
            return s.Path.End();
        }

        private static token.Pos End(this ref ValueSpec s)
        {
            {
                var n = len(s.Values);

                if (n > 0L)
                {
                    return s.Values[n - 1L].End();
                }

            }
            if (s.Type != null)
            {
                return s.Type.End();
            }
            return s.Names[len(s.Names) - 1L].End();
        }
        private static token.Pos End(this ref TypeSpec s)
        {
            return s.Type.End();
        }

        // specNode() ensures that only spec nodes can be
        // assigned to a Spec.
        //
        private static void specNode(this ref ImportSpec _p0)
        {
        }
        private static void specNode(this ref ValueSpec _p0)
        {
        }
        private static void specNode(this ref TypeSpec _p0)
        {
        }

        // A declaration is represented by one of the following declaration nodes.
        //
 
        // A BadDecl node is a placeholder for declarations containing
        // syntax errors for which no correct declaration nodes can be
        // created.
        //
        public partial struct BadDecl
        {
            public token.Pos From; // position range of bad declaration
            public token.Pos To; // position range of bad declaration
        } 

        // A GenDecl node (generic declaration node) represents an import,
        // constant, type or variable declaration. A valid Lparen position
        // (Lparen.IsValid()) indicates a parenthesized declaration.
        //
        // Relationship between Tok value and Specs element type:
        //
        //    token.IMPORT  *ImportSpec
        //    token.CONST   *ValueSpec
        //    token.TYPE    *TypeSpec
        //    token.VAR     *ValueSpec
        //
        public partial struct GenDecl
        {
            public ptr<CommentGroup> Doc; // associated documentation; or nil
            public token.Pos TokPos; // position of Tok
            public token.Token Tok; // IMPORT, CONST, TYPE, VAR
            public token.Pos Lparen; // position of '(', if any
            public slice<Spec> Specs;
            public token.Pos Rparen; // position of ')', if any
        } 

        // A FuncDecl node represents a function declaration.
        public partial struct FuncDecl
        {
            public ptr<CommentGroup> Doc; // associated documentation; or nil
            public ptr<FieldList> Recv; // receiver (methods); or nil (functions)
            public ptr<Ident> Name; // function/method name
            public ptr<FuncType> Type; // function signature: parameters, results, and position of "func" keyword
            public ptr<BlockStmt> Body; // function body; or nil for external (non-Go) function
        }        private static token.Pos Pos(this ref BadDecl d)
        {
            return d.From;
        }
        private static token.Pos Pos(this ref GenDecl d)
        {
            return d.TokPos;
        }
        private static token.Pos Pos(this ref FuncDecl d)
        {
            return d.Type.Pos();
        }

        private static token.Pos End(this ref BadDecl d)
        {
            return d.To;
        }
        private static token.Pos End(this ref GenDecl d)
        {
            if (d.Rparen.IsValid())
            {
                return d.Rparen + 1L;
            }
            return d.Specs[0L].End();
        }
        private static token.Pos End(this ref FuncDecl d)
        {
            if (d.Body != null)
            {
                return d.Body.End();
            }
            return d.Type.End();
        }

        // declNode() ensures that only declaration nodes can be
        // assigned to a Decl.
        //
        private static void declNode(this ref BadDecl _p0)
        {
        }
        private static void declNode(this ref GenDecl _p0)
        {
        }
        private static void declNode(this ref FuncDecl _p0)
        {
        }

        // ----------------------------------------------------------------------------
        // Files and packages

        // A File node represents a Go source file.
        //
        // The Comments list contains all comments in the source file in order of
        // appearance, including the comments that are pointed to from other nodes
        // via Doc and Comment fields.
        //
        // For correct printing of source code containing comments (using packages
        // go/format and go/printer), special care must be taken to update comments
        // when a File's syntax tree is modified: For printing, comments are interspersed
        // between tokens based on their position. If syntax tree nodes are
        // removed or moved, relevant comments in their vicinity must also be removed
        // (from the File.Comments list) or moved accordingly (by updating their
        // positions). A CommentMap may be used to facilitate some of these operations.
        //
        // Whether and how a comment is associated with a node depends on the
        // interpretation of the syntax tree by the manipulating program: Except for Doc
        // and Comment comments directly associated with nodes, the remaining comments
        // are "free-floating" (see also issues #18593, #20744).
        //
        public partial struct File
        {
            public ptr<CommentGroup> Doc; // associated documentation; or nil
            public token.Pos Package; // position of "package" keyword
            public ptr<Ident> Name; // package name
            public slice<Decl> Decls; // top-level declarations; or nil
            public ptr<Scope> Scope; // package scope (this file only)
            public slice<ref ImportSpec> Imports; // imports in this file
            public slice<ref Ident> Unresolved; // unresolved identifiers in this file
            public slice<ref CommentGroup> Comments; // list of all comments in the source file
        }

        private static token.Pos Pos(this ref File f)
        {
            return f.Package;
        }
        private static token.Pos End(this ref File f)
        {
            {
                var n = len(f.Decls);

                if (n > 0L)
                {
                    return f.Decls[n - 1L].End();
                }

            }
            return f.Name.End();
        }

        // A Package node represents a set of source files
        // collectively building a Go package.
        //
        public partial struct Package
        {
            public @string Name; // package name
            public ptr<Scope> Scope; // package scope across all files
            public map<@string, ref Object> Imports; // map of package id -> package object
            public map<@string, ref File> Files; // Go source files by filename
        }

        private static token.Pos Pos(this ref Package p)
        {
            return token.NoPos;
        }
        private static token.Pos End(this ref Package p)
        {
            return token.NoPos;
        }
    }
}}
