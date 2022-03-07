// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ast declares the types used to represent syntax trees for Go
// packages.
//
// package ast -- go2cs converted at 2022 March 06 22:41:10 UTC
// import "go/ast" ==> using ast = go.go.ast_package
// Original source: C:\Program Files\Go\src\go\ast\ast.go
using token = go.go.token_package;
using strings = go.strings_package;

namespace go.go;

public static partial class ast_package {

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
public partial interface Node {
    token.Pos Pos(); // position of first character belonging to the node
    token.Pos End(); // position of first character immediately after the node
}

// All expression nodes implement the Expr interface.
public partial interface Expr {
    void exprNode();
}

// All statement nodes implement the Stmt interface.
public partial interface Stmt {
    void stmtNode();
}

// All declaration nodes implement the Decl interface.
public partial interface Decl {
    void declNode();
}

// ----------------------------------------------------------------------------
// Comments

// A Comment node represents a single //-style or /*-style comment.
//
// The Text field contains the comment text without carriage returns (\r) that
// may have been present in the source. Because a comment's end position is
// computed using len(Text), the position reported by End() does not match the
// true source end position for comments containing carriage returns.
public partial struct Comment {
    public token.Pos Slash; // position of "/" starting the comment
    public @string Text; // comment text (excluding '\n' for //-style comments)
}

private static token.Pos Pos(this ptr<Comment> _addr_c) {
    ref Comment c = ref _addr_c.val;

    return c.Slash;
}
private static token.Pos End(this ptr<Comment> _addr_c) {
    ref Comment c = ref _addr_c.val;

    return token.Pos(int(c.Slash) + len(c.Text));
}

// A CommentGroup represents a sequence of comments
// with no other tokens and no empty lines between.
//
public partial struct CommentGroup {
    public slice<ptr<Comment>> List; // len(List) > 0
}

private static token.Pos Pos(this ptr<CommentGroup> _addr_g) {
    ref CommentGroup g = ref _addr_g.val;

    return g.List[0].Pos();
}
private static token.Pos End(this ptr<CommentGroup> _addr_g) {
    ref CommentGroup g = ref _addr_g.val;

    return g.List[len(g.List) - 1].End();
}

private static bool isWhitespace(byte ch) {
    return ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r';
}

private static @string stripTrailingWhitespace(@string s) {
    var i = len(s);
    while (i > 0 && isWhitespace(s[i - 1])) {
        i--;
    }
    return s[(int)0..(int)i];
}

// Text returns the text of the comment.
// Comment markers (//, /*, and */), the first space of a line comment, and
// leading and trailing empty lines are removed.
// Comment directives like "//line" and "//go:noinline" are also removed.
// Multiple empty lines are reduced to one, and trailing space on lines is trimmed.
// Unless the result is empty, it is newline-terminated.
private static @string Text(this ptr<CommentGroup> _addr_g) {
    ref CommentGroup g = ref _addr_g.val;

    if (g == null) {
        return "";
    }
    var comments = make_slice<@string>(len(g.List));
    {
        var c__prev1 = c;

        foreach (var (__i, __c) in g.List) {
            i = __i;
            c = __c;
            comments[i] = c.Text;
        }
        c = c__prev1;
    }

    var lines = make_slice<@string>(0, 10); // most comments are less than 10 lines
    {
        var c__prev1 = c;

        foreach (var (_, __c) in comments) {
            c = __c; 
            // Remove comment markers.
            // The parser has given us exactly the comment text.
            switch (c[1]) {
                case '/': 
                    //-style comment (no newline at the end)
                    c = c[(int)2..];
                    if (len(c) == 0) { 
                        // empty line
                        break;

                    }

                    if (c[0] == ' ') { 
                        // strip first space - required for Example tests
                        c = c[(int)1..];
                        break;

                    }

                    if (isDirective(c)) { 
                        // Ignore //go:noinline, //line, and so on.
                        continue;

                    }

                    break;
                case '*': 
                    /*-style comment */
                    c = c[(int)2..(int)len(c) - 2];
                    break;
            } 

            // Split on newlines.
            var cl = strings.Split(c, "\n"); 

            // Walk lines, stripping trailing white space and adding to list.
            foreach (var (_, l) in cl) {
                lines = append(lines, stripTrailingWhitespace(l));
            }

        }
        c = c__prev1;
    }

    nint n = 0;
    foreach (var (_, line) in lines) {
        if (line != "" || n > 0 && lines[n - 1] != "") {
            lines[n] = line;
            n++;
        }
    }    lines = lines[(int)0..(int)n]; 

    // Add final "" entry to get trailing newline from Join.
    if (n > 0 && lines[n - 1] != "") {
        lines = append(lines, "");
    }
    return strings.Join(lines, "\n");

}

// isDirective reports whether c is a comment directive.
private static bool isDirective(@string c) { 
    // "//line " is a line directive.
    // (The // has been removed.)
    if (strings.HasPrefix(c, "line ")) {
        return true;
    }
    var colon = strings.Index(c, ":");
    if (colon <= 0 || colon + 1 >= len(c)) {
        return false;
    }
    for (nint i = 0; i <= colon + 1; i++) {
        if (i == colon) {
            continue;
        }
        var b = c[i];
        if (!('a' <= b && b <= 'z' || '0' <= b && b <= '9')) {
            return false;
        }
    }
    return true;

}

// ----------------------------------------------------------------------------
// Expressions and types

// A Field represents a Field declaration list in a struct type,
// a method list in an interface type, or a parameter/result declaration
// in a signature.
// Field.Names is nil for unnamed parameters (parameter lists which only contain types)
// and embedded struct fields. In the latter case, the field name is the type name.
// Field.Names contains a single name "type" for elements of interface type lists.
// Types belonging to the same type list share the same "type" identifier which also
// records the position of that keyword.
//
public partial struct Field {
    public ptr<CommentGroup> Doc; // associated documentation; or nil
    public slice<ptr<Ident>> Names; // field/method/(type) parameter names, or type "type"; or nil
    public Expr Type; // field/method/parameter type, type list type; or nil
    public ptr<BasicLit> Tag; // field tag; or nil
    public ptr<CommentGroup> Comment; // line comments; or nil
}

private static token.Pos Pos(this ptr<Field> _addr_f) {
    ref Field f = ref _addr_f.val;

    if (len(f.Names) > 0) {
        return f.Names[0].Pos();
    }
    if (f.Type != null) {
        return f.Type.Pos();
    }
    return token.NoPos;

}

private static token.Pos End(this ptr<Field> _addr_f) {
    ref Field f = ref _addr_f.val;

    if (f.Tag != null) {
        return f.Tag.End();
    }
    if (f.Type != null) {
        return f.Type.End();
    }
    if (len(f.Names) > 0) {
        return f.Names[len(f.Names) - 1].End();
    }
    return token.NoPos;

}

// A FieldList represents a list of Fields, enclosed by parentheses or braces.
public partial struct FieldList {
    public token.Pos Opening; // position of opening parenthesis/brace, if any
    public slice<ptr<Field>> List; // field list; or nil
    public token.Pos Closing; // position of closing parenthesis/brace, if any
}

private static token.Pos Pos(this ptr<FieldList> _addr_f) {
    ref FieldList f = ref _addr_f.val;

    if (f.Opening.IsValid()) {
        return f.Opening;
    }
    if (len(f.List) > 0) {
        return f.List[0].Pos();
    }
    return token.NoPos;

}

private static token.Pos End(this ptr<FieldList> _addr_f) {
    ref FieldList f = ref _addr_f.val;

    if (f.Closing.IsValid()) {
        return f.Closing + 1;
    }
    {
        var n = len(f.List);

        if (n > 0) {
            return f.List[n - 1].End();
        }
    }

    return token.NoPos;

}

// NumFields returns the number of parameters or struct fields represented by a FieldList.
private static nint NumFields(this ptr<FieldList> _addr_f) {
    ref FieldList f = ref _addr_f.val;

    nint n = 0;
    if (f != null) {
        foreach (var (_, g) in f.List) {
            var m = len(g.Names);
            if (m == 0) {
                m = 1;
            }
            n += m;
        }
    }
    return n;

}

// An expression is represented by a tree consisting of one
// or more of the following concrete expression nodes.
//
 
// A BadExpr node is a placeholder for an expression containing
// syntax errors for which a correct expression node cannot be
// created.
//
public partial struct BadExpr {
    public token.Pos From; // position range of bad expression
    public token.Pos To; // position range of bad expression
} 

// An Ident node represents an identifier.
public partial struct Ident {
    public token.Pos NamePos; // identifier position
    public @string Name; // identifier name
    public ptr<Object> Obj; // denoted object; or nil
} 

// An Ellipsis node stands for the "..." type in a
// parameter list or the "..." length in an array type.
//
public partial struct Ellipsis {
    public token.Pos Ellipsis; // position of "..."
    public Expr Elt; // ellipsis element type (parameter lists only); or nil
} 

// A BasicLit node represents a literal of basic type.
public partial struct BasicLit {
    public token.Pos ValuePos; // literal position
    public token.Token Kind; // token.INT, token.FLOAT, token.IMAG, token.CHAR, or token.STRING
    public @string Value; // literal string; e.g. 42, 0x7f, 3.14, 1e-9, 2.4i, 'a', '\x7f', "foo" or `\m\n\o`
} 

// A FuncLit node represents a function literal.
public partial struct FuncLit {
    public ptr<FuncType> Type; // function type
    public ptr<BlockStmt> Body; // function body
} 

// A CompositeLit node represents a composite literal.
public partial struct CompositeLit {
    public Expr Type; // literal type; or nil
    public token.Pos Lbrace; // position of "{"
    public slice<Expr> Elts; // list of composite elements; or nil
    public token.Pos Rbrace; // position of "}"
    public bool Incomplete; // true if (source) expressions are missing in the Elts list
} 

// A ParenExpr node represents a parenthesized expression.
public partial struct ParenExpr {
    public token.Pos Lparen; // position of "("
    public Expr X; // parenthesized expression
    public token.Pos Rparen; // position of ")"
} 

// A SelectorExpr node represents an expression followed by a selector.
public partial struct SelectorExpr {
    public Expr X; // expression
    public ptr<Ident> Sel; // field selector
} 

// An IndexExpr node represents an expression followed by an index.
public partial struct IndexExpr {
    public Expr X; // expression
    public token.Pos Lbrack; // position of "["
    public Expr Index; // index expression
    public token.Pos Rbrack; // position of "]"
} 

// A SliceExpr node represents an expression followed by slice indices.
public partial struct SliceExpr {
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
public partial struct TypeAssertExpr {
    public Expr X; // expression
    public token.Pos Lparen; // position of "("
    public Expr Type; // asserted type; nil means type switch X.(type)
    public token.Pos Rparen; // position of ")"
} 

// A CallExpr node represents an expression followed by an argument list.
public partial struct CallExpr {
    public Expr Fun; // function expression
    public token.Pos Lparen; // position of "("
    public slice<Expr> Args; // function arguments; or nil
    public token.Pos Ellipsis; // position of "..." (token.NoPos if there is no "...")
    public token.Pos Rparen; // position of ")"
} 

// A StarExpr node represents an expression of the form "*" Expression.
// Semantically it could be a unary "*" expression, or a pointer type.
//
public partial struct StarExpr {
    public token.Pos Star; // position of "*"
    public Expr X; // operand
} 

// A UnaryExpr node represents a unary expression.
// Unary "*" expressions are represented via StarExpr nodes.
//
public partial struct UnaryExpr {
    public token.Pos OpPos; // position of Op
    public token.Token Op; // operator
    public Expr X; // operand
} 

// A BinaryExpr node represents a binary expression.
public partial struct BinaryExpr {
    public Expr X; // left operand
    public token.Pos OpPos; // position of Op
    public token.Token Op; // operator
    public Expr Y; // right operand
} 

// A KeyValueExpr node represents (key : value) pairs
// in composite literals.
//
public partial struct KeyValueExpr {
    public Expr Key;
    public token.Pos Colon; // position of ":"
    public Expr Value;
}
public partial struct ChanDir { // : nint
}

public static readonly ChanDir SEND = 1 << (int)(iota);
public static readonly var RECV = 0;


// A type is represented by a tree consisting of one
// or more of the following type-specific expression
// nodes.
//
 
// An ArrayType node represents an array or slice type.
public partial struct ArrayType {
    public token.Pos Lbrack; // position of "["
    public Expr Len; // Ellipsis node for [...]T array types, nil for slice types
    public Expr Elt; // element type
} 

// A StructType node represents a struct type.
public partial struct StructType {
    public token.Pos Struct; // position of "struct" keyword
    public ptr<FieldList> Fields; // list of field declarations
    public bool Incomplete; // true if (source) fields are missing in the Fields list
} 

// Pointer types are represented via StarExpr nodes.

// An InterfaceType node represents an interface type.
public partial struct InterfaceType {
    public token.Pos Interface; // position of "interface" keyword
    public ptr<FieldList> Methods; // list of embedded interfaces, methods, or types
    public bool Incomplete; // true if (source) methods or types are missing in the Methods list
} 

// A MapType node represents a map type.
public partial struct MapType {
    public token.Pos Map; // position of "map" keyword
    public Expr Key;
    public Expr Value;
} 

// A ChanType node represents a channel type.
public partial struct ChanType {
    public token.Pos Begin; // position of "chan" keyword or "<-" (whichever comes first)
    public token.Pos Arrow; // position of "<-" (token.NoPos if there is no "<-")
    public ChanDir Dir; // channel direction
    public Expr Value; // value type
}
private static token.Pos Pos(this ptr<BadExpr> _addr_x) {
    ref BadExpr x = ref _addr_x.val;

    return x.From;
}
private static token.Pos Pos(this ptr<Ident> _addr_x) {
    ref Ident x = ref _addr_x.val;

    return x.NamePos;
}
private static token.Pos Pos(this ptr<Ellipsis> _addr_x) {
    ref Ellipsis x = ref _addr_x.val;

    return x.Ellipsis;
}
private static token.Pos Pos(this ptr<BasicLit> _addr_x) {
    ref BasicLit x = ref _addr_x.val;

    return x.ValuePos;
}
private static token.Pos Pos(this ptr<FuncLit> _addr_x) {
    ref FuncLit x = ref _addr_x.val;

    return x.Type.Pos();
}
private static token.Pos Pos(this ptr<CompositeLit> _addr_x) {
    ref CompositeLit x = ref _addr_x.val;

    if (x.Type != null) {
        return x.Type.Pos();
    }
    return x.Lbrace;

}
private static token.Pos Pos(this ptr<ParenExpr> _addr_x) {
    ref ParenExpr x = ref _addr_x.val;

    return x.Lparen;
}
private static token.Pos Pos(this ptr<SelectorExpr> _addr_x) {
    ref SelectorExpr x = ref _addr_x.val;

    return x.X.Pos();
}
private static token.Pos Pos(this ptr<IndexExpr> _addr_x) {
    ref IndexExpr x = ref _addr_x.val;

    return x.X.Pos();
}
private static token.Pos Pos(this ptr<SliceExpr> _addr_x) {
    ref SliceExpr x = ref _addr_x.val;

    return x.X.Pos();
}
private static token.Pos Pos(this ptr<TypeAssertExpr> _addr_x) {
    ref TypeAssertExpr x = ref _addr_x.val;

    return x.X.Pos();
}
private static token.Pos Pos(this ptr<CallExpr> _addr_x) {
    ref CallExpr x = ref _addr_x.val;

    return x.Fun.Pos();
}
private static token.Pos Pos(this ptr<StarExpr> _addr_x) {
    ref StarExpr x = ref _addr_x.val;

    return x.Star;
}
private static token.Pos Pos(this ptr<UnaryExpr> _addr_x) {
    ref UnaryExpr x = ref _addr_x.val;

    return x.OpPos;
}
private static token.Pos Pos(this ptr<BinaryExpr> _addr_x) {
    ref BinaryExpr x = ref _addr_x.val;

    return x.X.Pos();
}
private static token.Pos Pos(this ptr<KeyValueExpr> _addr_x) {
    ref KeyValueExpr x = ref _addr_x.val;

    return x.Key.Pos();
}
private static token.Pos Pos(this ptr<ArrayType> _addr_x) {
    ref ArrayType x = ref _addr_x.val;

    return x.Lbrack;
}
private static token.Pos Pos(this ptr<StructType> _addr_x) {
    ref StructType x = ref _addr_x.val;

    return x.Struct;
}
private static token.Pos Pos(this ptr<FuncType> _addr_x) {
    ref FuncType x = ref _addr_x.val;

    if (x.Func.IsValid() || x.Params == null) { // see issue 3870
        return x.Func;

    }
    return x.Params.Pos(); // interface method declarations have no "func" keyword
}
private static token.Pos Pos(this ptr<InterfaceType> _addr_x) {
    ref InterfaceType x = ref _addr_x.val;

    return x.Interface;
}
private static token.Pos Pos(this ptr<MapType> _addr_x) {
    ref MapType x = ref _addr_x.val;

    return x.Map;
}
private static token.Pos Pos(this ptr<ChanType> _addr_x) {
    ref ChanType x = ref _addr_x.val;

    return x.Begin;
}

private static token.Pos End(this ptr<BadExpr> _addr_x) {
    ref BadExpr x = ref _addr_x.val;

    return x.To;
}
private static token.Pos End(this ptr<Ident> _addr_x) {
    ref Ident x = ref _addr_x.val;

    return token.Pos(int(x.NamePos) + len(x.Name));
}
private static token.Pos End(this ptr<Ellipsis> _addr_x) {
    ref Ellipsis x = ref _addr_x.val;

    if (x.Elt != null) {
        return x.Elt.End();
    }
    return x.Ellipsis + 3; // len("...")
}
private static token.Pos End(this ptr<BasicLit> _addr_x) {
    ref BasicLit x = ref _addr_x.val;

    return token.Pos(int(x.ValuePos) + len(x.Value));
}
private static token.Pos End(this ptr<FuncLit> _addr_x) {
    ref FuncLit x = ref _addr_x.val;

    return x.Body.End();
}
private static token.Pos End(this ptr<CompositeLit> _addr_x) {
    ref CompositeLit x = ref _addr_x.val;

    return x.Rbrace + 1;
}
private static token.Pos End(this ptr<ParenExpr> _addr_x) {
    ref ParenExpr x = ref _addr_x.val;

    return x.Rparen + 1;
}
private static token.Pos End(this ptr<SelectorExpr> _addr_x) {
    ref SelectorExpr x = ref _addr_x.val;

    return x.Sel.End();
}
private static token.Pos End(this ptr<IndexExpr> _addr_x) {
    ref IndexExpr x = ref _addr_x.val;

    return x.Rbrack + 1;
}
private static token.Pos End(this ptr<SliceExpr> _addr_x) {
    ref SliceExpr x = ref _addr_x.val;

    return x.Rbrack + 1;
}
private static token.Pos End(this ptr<TypeAssertExpr> _addr_x) {
    ref TypeAssertExpr x = ref _addr_x.val;

    return x.Rparen + 1;
}
private static token.Pos End(this ptr<CallExpr> _addr_x) {
    ref CallExpr x = ref _addr_x.val;

    return x.Rparen + 1;
}
private static token.Pos End(this ptr<StarExpr> _addr_x) {
    ref StarExpr x = ref _addr_x.val;

    return x.X.End();
}
private static token.Pos End(this ptr<UnaryExpr> _addr_x) {
    ref UnaryExpr x = ref _addr_x.val;

    return x.X.End();
}
private static token.Pos End(this ptr<BinaryExpr> _addr_x) {
    ref BinaryExpr x = ref _addr_x.val;

    return x.Y.End();
}
private static token.Pos End(this ptr<KeyValueExpr> _addr_x) {
    ref KeyValueExpr x = ref _addr_x.val;

    return x.Value.End();
}
private static token.Pos End(this ptr<ArrayType> _addr_x) {
    ref ArrayType x = ref _addr_x.val;

    return x.Elt.End();
}
private static token.Pos End(this ptr<StructType> _addr_x) {
    ref StructType x = ref _addr_x.val;

    return x.Fields.End();
}
private static token.Pos End(this ptr<FuncType> _addr_x) {
    ref FuncType x = ref _addr_x.val;

    if (x.Results != null) {
        return x.Results.End();
    }
    return x.Params.End();

}
private static token.Pos End(this ptr<InterfaceType> _addr_x) {
    ref InterfaceType x = ref _addr_x.val;

    return x.Methods.End();
}
private static token.Pos End(this ptr<MapType> _addr_x) {
    ref MapType x = ref _addr_x.val;

    return x.Value.End();
}
private static token.Pos End(this ptr<ChanType> _addr_x) {
    ref ChanType x = ref _addr_x.val;

    return x.Value.End();
}

// exprNode() ensures that only expression/type nodes can be
// assigned to an Expr.
//
private static void exprNode(this ptr<BadExpr> _addr__p0) {
    ref BadExpr _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<Ident> _addr__p0) {
    ref Ident _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<Ellipsis> _addr__p0) {
    ref Ellipsis _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<BasicLit> _addr__p0) {
    ref BasicLit _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<FuncLit> _addr__p0) {
    ref FuncLit _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<CompositeLit> _addr__p0) {
    ref CompositeLit _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<ParenExpr> _addr__p0) {
    ref ParenExpr _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<SelectorExpr> _addr__p0) {
    ref SelectorExpr _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<IndexExpr> _addr__p0) {
    ref IndexExpr _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<SliceExpr> _addr__p0) {
    ref SliceExpr _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<TypeAssertExpr> _addr__p0) {
    ref TypeAssertExpr _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<CallExpr> _addr__p0) {
    ref CallExpr _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<StarExpr> _addr__p0) {
    ref StarExpr _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<UnaryExpr> _addr__p0) {
    ref UnaryExpr _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<BinaryExpr> _addr__p0) {
    ref BinaryExpr _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<KeyValueExpr> _addr__p0) {
    ref KeyValueExpr _p0 = ref _addr__p0.val;

}

private static void exprNode(this ptr<ArrayType> _addr__p0) {
    ref ArrayType _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<StructType> _addr__p0) {
    ref StructType _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<FuncType> _addr__p0) {
    ref FuncType _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<InterfaceType> _addr__p0) {
    ref InterfaceType _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<MapType> _addr__p0) {
    ref MapType _p0 = ref _addr__p0.val;

}
private static void exprNode(this ptr<ChanType> _addr__p0) {
    ref ChanType _p0 = ref _addr__p0.val;

}

// ----------------------------------------------------------------------------
// Convenience functions for Idents

// NewIdent creates a new Ident without position.
// Useful for ASTs generated by code other than the Go parser.
//
public static ptr<Ident> NewIdent(@string name) {
    return addr(new Ident(token.NoPos,name,nil));
}

// IsExported reports whether name starts with an upper-case letter.
//
public static bool IsExported(@string name) {
    return token.IsExported(name);
}

// IsExported reports whether id starts with an upper-case letter.
//
private static bool IsExported(this ptr<Ident> _addr_id) {
    ref Ident id = ref _addr_id.val;

    return token.IsExported(id.Name);
}

private static @string String(this ptr<Ident> _addr_id) {
    ref Ident id = ref _addr_id.val;

    if (id != null) {
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
public partial struct BadStmt {
    public token.Pos From; // position range of bad statement
    public token.Pos To; // position range of bad statement
} 

// A DeclStmt node represents a declaration in a statement list.
public partial struct DeclStmt {
    public Decl Decl; // *GenDecl with CONST, TYPE, or VAR token
} 

// An EmptyStmt node represents an empty statement.
// The "position" of the empty statement is the position
// of the immediately following (explicit or implicit) semicolon.
//
public partial struct EmptyStmt {
    public token.Pos Semicolon; // position of following ";"
    public bool Implicit; // if set, ";" was omitted in the source
} 

// A LabeledStmt node represents a labeled statement.
public partial struct LabeledStmt {
    public ptr<Ident> Label;
    public token.Pos Colon; // position of ":"
    public Stmt Stmt;
} 

// An ExprStmt node represents a (stand-alone) expression
// in a statement list.
//
public partial struct ExprStmt {
    public Expr X; // expression
} 

// A SendStmt node represents a send statement.
public partial struct SendStmt {
    public Expr Chan;
    public token.Pos Arrow; // position of "<-"
    public Expr Value;
} 

// An IncDecStmt node represents an increment or decrement statement.
public partial struct IncDecStmt {
    public Expr X;
    public token.Pos TokPos; // position of Tok
    public token.Token Tok; // INC or DEC
} 

// An AssignStmt node represents an assignment or
// a short variable declaration.
//
public partial struct AssignStmt {
    public slice<Expr> Lhs;
    public token.Pos TokPos; // position of Tok
    public token.Token Tok; // assignment token, DEFINE
    public slice<Expr> Rhs;
} 

// A GoStmt node represents a go statement.
public partial struct GoStmt {
    public token.Pos Go; // position of "go" keyword
    public ptr<CallExpr> Call;
} 

// A DeferStmt node represents a defer statement.
public partial struct DeferStmt {
    public token.Pos Defer; // position of "defer" keyword
    public ptr<CallExpr> Call;
} 

// A ReturnStmt node represents a return statement.
public partial struct ReturnStmt {
    public token.Pos Return; // position of "return" keyword
    public slice<Expr> Results; // result expressions; or nil
} 

// A BranchStmt node represents a break, continue, goto,
// or fallthrough statement.
//
public partial struct BranchStmt {
    public token.Pos TokPos; // position of Tok
    public token.Token Tok; // keyword token (BREAK, CONTINUE, GOTO, FALLTHROUGH)
    public ptr<Ident> Label; // label name; or nil
} 

// A BlockStmt node represents a braced statement list.
public partial struct BlockStmt {
    public token.Pos Lbrace; // position of "{"
    public slice<Stmt> List;
    public token.Pos Rbrace; // position of "}", if any (may be absent due to syntax error)
} 

// An IfStmt node represents an if statement.
public partial struct IfStmt {
    public token.Pos If; // position of "if" keyword
    public Stmt Init; // initialization statement; or nil
    public Expr Cond; // condition
    public ptr<BlockStmt> Body;
    public Stmt Else; // else branch; or nil
} 

// A CaseClause represents a case of an expression or type switch statement.
public partial struct CaseClause {
    public token.Pos Case; // position of "case" or "default" keyword
    public slice<Expr> List; // list of expressions or types; nil means default case
    public token.Pos Colon; // position of ":"
    public slice<Stmt> Body; // statement list; or nil
} 

// A SwitchStmt node represents an expression switch statement.
public partial struct SwitchStmt {
    public token.Pos Switch; // position of "switch" keyword
    public Stmt Init; // initialization statement; or nil
    public Expr Tag; // tag expression; or nil
    public ptr<BlockStmt> Body; // CaseClauses only
} 

// A TypeSwitchStmt node represents a type switch statement.
public partial struct TypeSwitchStmt {
    public token.Pos Switch; // position of "switch" keyword
    public Stmt Init; // initialization statement; or nil
    public Stmt Assign; // x := y.(type) or y.(type)
    public ptr<BlockStmt> Body; // CaseClauses only
} 

// A CommClause node represents a case of a select statement.
public partial struct CommClause {
    public token.Pos Case; // position of "case" or "default" keyword
    public Stmt Comm; // send or receive statement; nil means default case
    public token.Pos Colon; // position of ":"
    public slice<Stmt> Body; // statement list; or nil
} 

// A SelectStmt node represents a select statement.
public partial struct SelectStmt {
    public token.Pos Select; // position of "select" keyword
    public ptr<BlockStmt> Body; // CommClauses only
} 

// A ForStmt represents a for statement.
public partial struct ForStmt {
    public token.Pos For; // position of "for" keyword
    public Stmt Init; // initialization statement; or nil
    public Expr Cond; // condition; or nil
    public Stmt Post; // post iteration statement; or nil
    public ptr<BlockStmt> Body;
} 

// A RangeStmt represents a for statement with a range clause.
public partial struct RangeStmt {
    public token.Pos For; // position of "for" keyword
    public Expr Key; // Key, Value may be nil
    public Expr Value; // Key, Value may be nil
    public token.Pos TokPos; // position of Tok; invalid if Key == nil
    public token.Token Tok; // ILLEGAL if Key == nil, ASSIGN, DEFINE
    public Expr X; // value to range over
    public ptr<BlockStmt> Body;
}
private static token.Pos Pos(this ptr<BadStmt> _addr_s) {
    ref BadStmt s = ref _addr_s.val;

    return s.From;
}
private static token.Pos Pos(this ptr<DeclStmt> _addr_s) {
    ref DeclStmt s = ref _addr_s.val;

    return s.Decl.Pos();
}
private static token.Pos Pos(this ptr<EmptyStmt> _addr_s) {
    ref EmptyStmt s = ref _addr_s.val;

    return s.Semicolon;
}
private static token.Pos Pos(this ptr<LabeledStmt> _addr_s) {
    ref LabeledStmt s = ref _addr_s.val;

    return s.Label.Pos();
}
private static token.Pos Pos(this ptr<ExprStmt> _addr_s) {
    ref ExprStmt s = ref _addr_s.val;

    return s.X.Pos();
}
private static token.Pos Pos(this ptr<SendStmt> _addr_s) {
    ref SendStmt s = ref _addr_s.val;

    return s.Chan.Pos();
}
private static token.Pos Pos(this ptr<IncDecStmt> _addr_s) {
    ref IncDecStmt s = ref _addr_s.val;

    return s.X.Pos();
}
private static token.Pos Pos(this ptr<AssignStmt> _addr_s) {
    ref AssignStmt s = ref _addr_s.val;

    return s.Lhs[0].Pos();
}
private static token.Pos Pos(this ptr<GoStmt> _addr_s) {
    ref GoStmt s = ref _addr_s.val;

    return s.Go;
}
private static token.Pos Pos(this ptr<DeferStmt> _addr_s) {
    ref DeferStmt s = ref _addr_s.val;

    return s.Defer;
}
private static token.Pos Pos(this ptr<ReturnStmt> _addr_s) {
    ref ReturnStmt s = ref _addr_s.val;

    return s.Return;
}
private static token.Pos Pos(this ptr<BranchStmt> _addr_s) {
    ref BranchStmt s = ref _addr_s.val;

    return s.TokPos;
}
private static token.Pos Pos(this ptr<BlockStmt> _addr_s) {
    ref BlockStmt s = ref _addr_s.val;

    return s.Lbrace;
}
private static token.Pos Pos(this ptr<IfStmt> _addr_s) {
    ref IfStmt s = ref _addr_s.val;

    return s.If;
}
private static token.Pos Pos(this ptr<CaseClause> _addr_s) {
    ref CaseClause s = ref _addr_s.val;

    return s.Case;
}
private static token.Pos Pos(this ptr<SwitchStmt> _addr_s) {
    ref SwitchStmt s = ref _addr_s.val;

    return s.Switch;
}
private static token.Pos Pos(this ptr<TypeSwitchStmt> _addr_s) {
    ref TypeSwitchStmt s = ref _addr_s.val;

    return s.Switch;
}
private static token.Pos Pos(this ptr<CommClause> _addr_s) {
    ref CommClause s = ref _addr_s.val;

    return s.Case;
}
private static token.Pos Pos(this ptr<SelectStmt> _addr_s) {
    ref SelectStmt s = ref _addr_s.val;

    return s.Select;
}
private static token.Pos Pos(this ptr<ForStmt> _addr_s) {
    ref ForStmt s = ref _addr_s.val;

    return s.For;
}
private static token.Pos Pos(this ptr<RangeStmt> _addr_s) {
    ref RangeStmt s = ref _addr_s.val;

    return s.For;
}

private static token.Pos End(this ptr<BadStmt> _addr_s) {
    ref BadStmt s = ref _addr_s.val;

    return s.To;
}
private static token.Pos End(this ptr<DeclStmt> _addr_s) {
    ref DeclStmt s = ref _addr_s.val;

    return s.Decl.End();
}
private static token.Pos End(this ptr<EmptyStmt> _addr_s) {
    ref EmptyStmt s = ref _addr_s.val;

    if (s.Implicit) {
        return s.Semicolon;
    }
    return s.Semicolon + 1; /* len(";") */
}
private static token.Pos End(this ptr<LabeledStmt> _addr_s) {
    ref LabeledStmt s = ref _addr_s.val;

    return s.Stmt.End();
}
private static token.Pos End(this ptr<ExprStmt> _addr_s) {
    ref ExprStmt s = ref _addr_s.val;

    return s.X.End();
}
private static token.Pos End(this ptr<SendStmt> _addr_s) {
    ref SendStmt s = ref _addr_s.val;

    return s.Value.End();
}
private static token.Pos End(this ptr<IncDecStmt> _addr_s) {
    ref IncDecStmt s = ref _addr_s.val;

    return s.TokPos + 2; /* len("++") */
}
private static token.Pos End(this ptr<AssignStmt> _addr_s) {
    ref AssignStmt s = ref _addr_s.val;

    return s.Rhs[len(s.Rhs) - 1].End();
}
private static token.Pos End(this ptr<GoStmt> _addr_s) {
    ref GoStmt s = ref _addr_s.val;

    return s.Call.End();
}
private static token.Pos End(this ptr<DeferStmt> _addr_s) {
    ref DeferStmt s = ref _addr_s.val;

    return s.Call.End();
}
private static token.Pos End(this ptr<ReturnStmt> _addr_s) {
    ref ReturnStmt s = ref _addr_s.val;

    {
        var n = len(s.Results);

        if (n > 0) {
            return s.Results[n - 1].End();
        }
    }

    return s.Return + 6; // len("return")
}
private static token.Pos End(this ptr<BranchStmt> _addr_s) {
    ref BranchStmt s = ref _addr_s.val;

    if (s.Label != null) {
        return s.Label.End();
    }
    return token.Pos(int(s.TokPos) + len(s.Tok.String()));

}
private static token.Pos End(this ptr<BlockStmt> _addr_s) {
    ref BlockStmt s = ref _addr_s.val;

    if (s.Rbrace.IsValid()) {
        return s.Rbrace + 1;
    }
    {
        var n = len(s.List);

        if (n > 0) {
            return s.List[n - 1].End();
        }
    }

    return s.Lbrace + 1;

}
private static token.Pos End(this ptr<IfStmt> _addr_s) {
    ref IfStmt s = ref _addr_s.val;

    if (s.Else != null) {
        return s.Else.End();
    }
    return s.Body.End();

}
private static token.Pos End(this ptr<CaseClause> _addr_s) {
    ref CaseClause s = ref _addr_s.val;

    {
        var n = len(s.Body);

        if (n > 0) {
            return s.Body[n - 1].End();
        }
    }

    return s.Colon + 1;

}
private static token.Pos End(this ptr<SwitchStmt> _addr_s) {
    ref SwitchStmt s = ref _addr_s.val;

    return s.Body.End();
}
private static token.Pos End(this ptr<TypeSwitchStmt> _addr_s) {
    ref TypeSwitchStmt s = ref _addr_s.val;

    return s.Body.End();
}
private static token.Pos End(this ptr<CommClause> _addr_s) {
    ref CommClause s = ref _addr_s.val;

    {
        var n = len(s.Body);

        if (n > 0) {
            return s.Body[n - 1].End();
        }
    }

    return s.Colon + 1;

}
private static token.Pos End(this ptr<SelectStmt> _addr_s) {
    ref SelectStmt s = ref _addr_s.val;

    return s.Body.End();
}
private static token.Pos End(this ptr<ForStmt> _addr_s) {
    ref ForStmt s = ref _addr_s.val;

    return s.Body.End();
}
private static token.Pos End(this ptr<RangeStmt> _addr_s) {
    ref RangeStmt s = ref _addr_s.val;

    return s.Body.End();
}

// stmtNode() ensures that only statement nodes can be
// assigned to a Stmt.
//
private static void stmtNode(this ptr<BadStmt> _addr__p0) {
    ref BadStmt _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<DeclStmt> _addr__p0) {
    ref DeclStmt _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<EmptyStmt> _addr__p0) {
    ref EmptyStmt _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<LabeledStmt> _addr__p0) {
    ref LabeledStmt _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<ExprStmt> _addr__p0) {
    ref ExprStmt _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<SendStmt> _addr__p0) {
    ref SendStmt _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<IncDecStmt> _addr__p0) {
    ref IncDecStmt _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<AssignStmt> _addr__p0) {
    ref AssignStmt _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<GoStmt> _addr__p0) {
    ref GoStmt _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<DeferStmt> _addr__p0) {
    ref DeferStmt _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<ReturnStmt> _addr__p0) {
    ref ReturnStmt _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<BranchStmt> _addr__p0) {
    ref BranchStmt _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<BlockStmt> _addr__p0) {
    ref BlockStmt _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<IfStmt> _addr__p0) {
    ref IfStmt _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<CaseClause> _addr__p0) {
    ref CaseClause _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<SwitchStmt> _addr__p0) {
    ref SwitchStmt _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<TypeSwitchStmt> _addr__p0) {
    ref TypeSwitchStmt _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<CommClause> _addr__p0) {
    ref CommClause _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<SelectStmt> _addr__p0) {
    ref SelectStmt _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<ForStmt> _addr__p0) {
    ref ForStmt _p0 = ref _addr__p0.val;

}
private static void stmtNode(this ptr<RangeStmt> _addr__p0) {
    ref RangeStmt _p0 = ref _addr__p0.val;

}

// ----------------------------------------------------------------------------
// Declarations

// A Spec node represents a single (non-parenthesized) import,
// constant, type, or variable declaration.
//
 
// The Spec type stands for any of *ImportSpec, *ValueSpec, and *TypeSpec.
public partial interface Spec {
    void specNode();
} 

// An ImportSpec node represents a single package import.
public partial struct ImportSpec {
    public ptr<CommentGroup> Doc; // associated documentation; or nil
    public ptr<Ident> Name; // local package name (including "."); or nil
    public ptr<BasicLit> Path; // import path
    public ptr<CommentGroup> Comment; // line comments; or nil
    public token.Pos EndPos; // end of spec (overrides Path.Pos if nonzero)
} 

// A ValueSpec node represents a constant or variable declaration
// (ConstSpec or VarSpec production).
//
public partial struct ValueSpec {
    public ptr<CommentGroup> Doc; // associated documentation; or nil
    public slice<ptr<Ident>> Names; // value names (len(Names) > 0)
    public Expr Type; // value type; or nil
    public slice<Expr> Values; // initial values; or nil
    public ptr<CommentGroup> Comment; // line comments; or nil
}
private static token.Pos Pos(this ptr<ImportSpec> _addr_s) {
    ref ImportSpec s = ref _addr_s.val;

    if (s.Name != null) {
        return s.Name.Pos();
    }
    return s.Path.Pos();

}
private static token.Pos Pos(this ptr<ValueSpec> _addr_s) {
    ref ValueSpec s = ref _addr_s.val;

    return s.Names[0].Pos();
}
private static token.Pos Pos(this ptr<TypeSpec> _addr_s) {
    ref TypeSpec s = ref _addr_s.val;

    return s.Name.Pos();
}

private static token.Pos End(this ptr<ImportSpec> _addr_s) {
    ref ImportSpec s = ref _addr_s.val;

    if (s.EndPos != 0) {
        return s.EndPos;
    }
    return s.Path.End();

}

private static token.Pos End(this ptr<ValueSpec> _addr_s) {
    ref ValueSpec s = ref _addr_s.val;

    {
        var n = len(s.Values);

        if (n > 0) {
            return s.Values[n - 1].End();
        }
    }

    if (s.Type != null) {
        return s.Type.End();
    }
    return s.Names[len(s.Names) - 1].End();

}
private static token.Pos End(this ptr<TypeSpec> _addr_s) {
    ref TypeSpec s = ref _addr_s.val;

    return s.Type.End();
}

// specNode() ensures that only spec nodes can be
// assigned to a Spec.
//
private static void specNode(this ptr<ImportSpec> _addr__p0) {
    ref ImportSpec _p0 = ref _addr__p0.val;

}
private static void specNode(this ptr<ValueSpec> _addr__p0) {
    ref ValueSpec _p0 = ref _addr__p0.val;

}
private static void specNode(this ptr<TypeSpec> _addr__p0) {
    ref TypeSpec _p0 = ref _addr__p0.val;

}

// A declaration is represented by one of the following declaration nodes.
//
 
// A BadDecl node is a placeholder for a declaration containing
// syntax errors for which a correct declaration node cannot be
// created.
//
public partial struct BadDecl {
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
public partial struct GenDecl {
    public ptr<CommentGroup> Doc; // associated documentation; or nil
    public token.Pos TokPos; // position of Tok
    public token.Token Tok; // IMPORT, CONST, TYPE, or VAR
    public token.Pos Lparen; // position of '(', if any
    public slice<Spec> Specs;
    public token.Pos Rparen; // position of ')', if any
} 

// A FuncDecl node represents a function declaration.
public partial struct FuncDecl {
    public ptr<CommentGroup> Doc; // associated documentation; or nil
    public ptr<FieldList> Recv; // receiver (methods); or nil (functions)
    public ptr<Ident> Name; // function/method name
    public ptr<FuncType> Type; // function signature: type and value parameters, results, and position of "func" keyword
    public ptr<BlockStmt> Body; // function body; or nil for external (non-Go) function
// TODO(rFindley) consider storing TParams here, rather than FuncType, as
//                they are only valid for declared functions
}
private static token.Pos Pos(this ptr<BadDecl> _addr_d) {
    ref BadDecl d = ref _addr_d.val;

    return d.From;
}
private static token.Pos Pos(this ptr<GenDecl> _addr_d) {
    ref GenDecl d = ref _addr_d.val;

    return d.TokPos;
}
private static token.Pos Pos(this ptr<FuncDecl> _addr_d) {
    ref FuncDecl d = ref _addr_d.val;

    return d.Type.Pos();
}

private static token.Pos End(this ptr<BadDecl> _addr_d) {
    ref BadDecl d = ref _addr_d.val;

    return d.To;
}
private static token.Pos End(this ptr<GenDecl> _addr_d) {
    ref GenDecl d = ref _addr_d.val;

    if (d.Rparen.IsValid()) {
        return d.Rparen + 1;
    }
    return d.Specs[0].End();

}
private static token.Pos End(this ptr<FuncDecl> _addr_d) {
    ref FuncDecl d = ref _addr_d.val;

    if (d.Body != null) {
        return d.Body.End();
    }
    return d.Type.End();

}

// declNode() ensures that only declaration nodes can be
// assigned to a Decl.
//
private static void declNode(this ptr<BadDecl> _addr__p0) {
    ref BadDecl _p0 = ref _addr__p0.val;

}
private static void declNode(this ptr<GenDecl> _addr__p0) {
    ref GenDecl _p0 = ref _addr__p0.val;

}
private static void declNode(this ptr<FuncDecl> _addr__p0) {
    ref FuncDecl _p0 = ref _addr__p0.val;

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
public partial struct File {
    public ptr<CommentGroup> Doc; // associated documentation; or nil
    public token.Pos Package; // position of "package" keyword
    public ptr<Ident> Name; // package name
    public slice<Decl> Decls; // top-level declarations; or nil
    public ptr<Scope> Scope; // package scope (this file only)
    public slice<ptr<ImportSpec>> Imports; // imports in this file
    public slice<ptr<Ident>> Unresolved; // unresolved identifiers in this file
    public slice<ptr<CommentGroup>> Comments; // list of all comments in the source file
}

private static token.Pos Pos(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    return f.Package;
}
private static token.Pos End(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    {
        var n = len(f.Decls);

        if (n > 0) {
            return f.Decls[n - 1].End();
        }
    }

    return f.Name.End();

}

// A Package node represents a set of source files
// collectively building a Go package.
//
public partial struct Package {
    public @string Name; // package name
    public ptr<Scope> Scope; // package scope across all files
    public map<@string, ptr<Object>> Imports; // map of package id -> package object
    public map<@string, ptr<File>> Files; // Go source files by filename
}

private static token.Pos Pos(this ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;

    return token.NoPos;
}
private static token.Pos End(this ptr<Package> _addr_p) {
    ref Package p = ref _addr_p.val;

    return token.NoPos;
}

} // end ast_package
