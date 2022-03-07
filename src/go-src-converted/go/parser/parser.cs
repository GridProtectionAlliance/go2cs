// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package parser implements a parser for Go source files. Input may be
// provided in a variety of forms (see the various Parse* functions); the
// output is an abstract syntax tree (AST) representing the Go source. The
// parser is invoked through one of the Parse* functions.
//
// The parser accepts a larger language than is syntactically permitted by
// the Go spec, for simplicity, and for improved robustness in the presence
// of syntax errors. For instance, in method declarations, the receiver is
// treated like an ordinary parameter list and thus may contain multiple
// entries where the spec permits exactly one. Consequently, the corresponding
// field in the AST (ast.FuncDecl.Recv) field is not restricted to one entry.
//
// package parser -- go2cs converted at 2022 March 06 22:42:45 UTC
// import "go/parser" ==> using parser = go.go.parser_package
// Original source: C:\Program Files\Go\src\go\parser\parser.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using typeparams = go.go.@internal.typeparams_package;
using scanner = go.go.scanner_package;
using token = go.go.token_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using System;


namespace go.go;

public static partial class parser_package {

    // The parser structure holds the parser's internal state.
private partial struct parser {
    public ptr<token.File> file;
    public scanner.ErrorList errors;
    public scanner.Scanner scanner; // Tracing/debugging
    public Mode mode; // parsing mode
    public bool trace; // == (mode&Trace != 0)
    public nint indent; // indentation used for tracing output

// Comments
    public slice<ptr<ast.CommentGroup>> comments;
    public ptr<ast.CommentGroup> leadComment; // last lead comment
    public ptr<ast.CommentGroup> lineComment; // last line comment

// Next token
    public token.Pos pos; // token position
    public token.Token tok; // one token look-ahead
    public @string lit; // token literal

// Error recovery
// (used to limit the number of calls to parser.advance
// w/o making scanning progress - avoids potential endless
// loops across multiple parser functions during error recovery)
    public token.Pos syncPos; // last synchronization position
    public nint syncCnt; // number of parser.advance calls without progress

// Non-syntactic parser control
    public nint exprLev; // < 0: in control clause, >= 0: in expression
    public bool inRhs; // if set, the parser is parsing a rhs expression

    public slice<ptr<ast.ImportSpec>> imports; // list of imports
}

private static void init(this ptr<parser> _addr_p, ptr<token.FileSet> _addr_fset, @string filename, slice<byte> src, Mode mode) {
    ref parser p = ref _addr_p.val;
    ref token.FileSet fset = ref _addr_fset.val;

    p.file = fset.AddFile(filename, -1, len(src));
    scanner.Mode m = default;
    if (mode & ParseComments != 0) {
        m = scanner.ScanComments;
    }
    Action<token.Position, @string> eh = (pos, msg) => {
        p.errors.Add(pos, msg);
    };
    p.scanner.Init(p.file, src, eh, m);

    p.mode = mode;
    p.trace = mode & Trace != 0; // for convenience (p.trace is used frequently)
    p.next();

}

private static bool parseTypeParams(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;

    return typeparams.Enabled && p.mode & typeparams.DisallowParsing == 0;
}

// ----------------------------------------------------------------------------
// Parsing support

private static void printTrace(this ptr<parser> _addr_p, params object[] a) {
    a = a.Clone();
    ref parser p = ref _addr_p.val;

    const @string dots = ". . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . ";

    const var n = len(dots);

    var pos = p.file.Position(p.pos);
    fmt.Printf("%5d:%3d: ", pos.Line, pos.Column);
    nint i = 2 * p.indent;
    while (i > n) {
        fmt.Print(dots);
        i -= n;
    } 
    // i <= n
    fmt.Print(dots[(int)0..(int)i]);
    fmt.Println(a);

}

private static ptr<parser> trace(ptr<parser> _addr_p, @string msg) {
    ref parser p = ref _addr_p.val;

    p.printTrace(msg, "(");
    p.indent++;
    return _addr_p!;
}

// Usage pattern: defer un(trace(p, "..."))
private static void un(ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;

    p.indent--;
    p.printTrace(")");
}

// Advance to the next token.
private static void next0(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;
 
    // Because of one-token look-ahead, print the previous token
    // when tracing as it provides a more readable output. The
    // very first token (!p.pos.IsValid()) is not initialized
    // (it is token.ILLEGAL), so don't print it.
    if (p.trace && p.pos.IsValid()) {
        var s = p.tok.String();

        if (p.tok.IsLiteral()) 
            p.printTrace(s, p.lit);
        else if (p.tok.IsOperator() || p.tok.IsKeyword()) 
            p.printTrace("\"" + s + "\"");
        else 
            p.printTrace(s);
        
    }
    p.pos, p.tok, p.lit = p.scanner.Scan();

}

// Consume a comment and return it and the line on which it ends.
private static (ptr<ast.Comment>, nint) consumeComment(this ptr<parser> _addr_p) {
    ptr<ast.Comment> comment = default!;
    nint endline = default;
    ref parser p = ref _addr_p.val;
 
    // /*-style comments may end on a different line than where they start.
    // Scan the comment for '\n' chars and adjust endline accordingly.
    endline = p.file.Line(p.pos);
    if (p.lit[1] == '*') { 
        // don't use range here - no need to decode Unicode code points
        for (nint i = 0; i < len(p.lit); i++) {
            if (p.lit[i] == '\n') {
                endline++;
            }
        }

    }
    comment = addr(new ast.Comment(Slash:p.pos,Text:p.lit));
    p.next0();

    return ;

}

// Consume a group of adjacent comments, add it to the parser's
// comments list, and return it together with the line at which
// the last comment in the group ends. A non-comment token or n
// empty lines terminate a comment group.
//
private static (ptr<ast.CommentGroup>, nint) consumeCommentGroup(this ptr<parser> _addr_p, nint n) {
    ptr<ast.CommentGroup> comments = default!;
    nint endline = default;
    ref parser p = ref _addr_p.val;

    slice<ptr<ast.Comment>> list = default;
    endline = p.file.Line(p.pos);
    while (p.tok == token.COMMENT && p.file.Line(p.pos) <= endline + n) {
        ptr<ast.Comment> comment;
        comment, endline = p.consumeComment();
        list = append(list, comment);
    } 

    // add comment group to the comments list
    comments = addr(new ast.CommentGroup(List:list));
    p.comments = append(p.comments, comments);

    return ;

}

// Advance to the next non-comment token. In the process, collect
// any comment groups encountered, and remember the last lead and
// line comments.
//
// A lead comment is a comment group that starts and ends in a
// line without any other tokens and that is followed by a non-comment
// token on the line immediately after the comment group.
//
// A line comment is a comment group that follows a non-comment
// token on the same line, and that has no tokens after it on the line
// where it ends.
//
// Lead and line comments may be considered documentation that is
// stored in the AST.
//
private static void next(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;

    p.leadComment = null;
    p.lineComment = null;
    var prev = p.pos;
    p.next0();

    if (p.tok == token.COMMENT) {
        ptr<ast.CommentGroup> comment;
        nint endline = default;

        if (p.file.Line(p.pos) == p.file.Line(prev)) { 
            // The comment is on same line as the previous token; it
            // cannot be a lead comment but may be a line comment.
            comment, endline = p.consumeCommentGroup(0);
            if (p.file.Line(p.pos) != endline || p.tok == token.EOF) { 
                // The next token is on a different line, thus
                // the last comment group is a line comment.
                p.lineComment = comment;

            }

        }
        endline = -1;
        while (p.tok == token.COMMENT) {
            comment, endline = p.consumeCommentGroup(1);
        }

        if (endline + 1 == p.file.Line(p.pos)) { 
            // The next token is following on the line immediately after the
            // comment group, thus the last comment group is a lead comment.
            p.leadComment = comment;

        }
    }
}

// A bailout panic is raised to indicate early termination.
private partial struct bailout {
}

private static void error(this ptr<parser> _addr_p, token.Pos pos, @string msg) => func((defer, panic, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "error: " + msg)));
    }
    var epos = p.file.Position(pos); 

    // If AllErrors is not set, discard errors reported on the same line
    // as the last recorded error and stop parsing if there are more than
    // 10 errors.
    if (p.mode & AllErrors == 0) {
        var n = len(p.errors);
        if (n > 0 && p.errors[n - 1].Pos.Line == epos.Line) {
            return ; // discard - likely a spurious error
        }
        if (n > 10) {
            panic(new bailout());
        }
    }
    p.errors.Add(epos, msg);

});

private static void errorExpected(this ptr<parser> _addr_p, token.Pos pos, @string msg) {
    ref parser p = ref _addr_p.val;

    msg = "expected " + msg;
    if (pos == p.pos) { 
        // the error happened at the current position;
        // make the error message more specific

        if (p.tok == token.SEMICOLON && p.lit == "\n") 
            msg += ", found newline";
        else if (p.tok.IsLiteral()) 
            // print 123 rather than 'INT', etc.
            msg += ", found " + p.lit;
        else 
            msg += ", found '" + p.tok.String() + "'";
        
    }
    p.error(pos, msg);

}

private static token.Pos expect(this ptr<parser> _addr_p, token.Token tok) {
    ref parser p = ref _addr_p.val;

    var pos = p.pos;
    if (p.tok != tok) {
        p.errorExpected(pos, "'" + tok.String() + "'");
    }
    p.next(); // make progress
    return pos;

}

// expect2 is like expect, but it returns an invalid position
// if the expected token is not found.
private static token.Pos expect2(this ptr<parser> _addr_p, token.Token tok) {
    token.Pos pos = default;
    ref parser p = ref _addr_p.val;

    if (p.tok == tok) {
        pos = p.pos;
    }
    else
 {
        p.errorExpected(p.pos, "'" + tok.String() + "'");
    }
    p.next(); // make progress
    return ;

}

// expectClosing is like expect but provides a better error message
// for the common case of a missing comma before a newline.
//
private static token.Pos expectClosing(this ptr<parser> _addr_p, token.Token tok, @string context) {
    ref parser p = ref _addr_p.val;

    if (p.tok != tok && p.tok == token.SEMICOLON && p.lit == "\n") {
        p.error(p.pos, "missing ',' before newline in " + context);
        p.next();
    }
    return p.expect(tok);

}

private static void expectSemi(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;
 
    // semicolon is optional before a closing ')' or '}'
    if (p.tok != token.RPAREN && p.tok != token.RBRACE) {

        if (p.tok == token.COMMA) 
        {
            // permit a ',' instead of a ';' but complain
            p.errorExpected(p.pos, "';'");
            fallthrough = true;
        }
        if (fallthrough || p.tok == token.SEMICOLON)
        {
            p.next();
            goto __switch_break0;
        }
        // default: 
            p.errorExpected(p.pos, "';'");
            p.advance(stmtStart);

        __switch_break0:;

    }
}

private static bool atComma(this ptr<parser> _addr_p, @string context, token.Token follow) {
    ref parser p = ref _addr_p.val;

    if (p.tok == token.COMMA) {
        return true;
    }
    if (p.tok != follow) {
        @string msg = "missing ','";
        if (p.tok == token.SEMICOLON && p.lit == "\n") {
            msg += " before newline";
        }
        p.error(p.pos, msg + " in " + context);
        return true; // "insert" comma and continue
    }
    return false;

}

private static void assert(bool cond, @string msg) => func((_, panic, _) => {
    if (!cond) {
        panic("go/parser internal error: " + msg);
    }
});

// advance consumes tokens until the current token p.tok
// is in the 'to' set, or token.EOF. For error recovery.
private static void advance(this ptr<parser> _addr_p, map<token.Token, bool> to) {
    ref parser p = ref _addr_p.val;

    while (p.tok != token.EOF) {
        if (to[p.tok]) { 
            // Return only if parser made some progress since last
            // sync or if it has not reached 10 advance calls without
            // progress. Otherwise consume at least one token to
            // avoid an endless parser loop (it is possible that
            // both parseOperand and parseStmt call advance and
            // correctly do not advance, thus the need for the
            // invocation limit p.syncCnt).
            if (p.pos == p.syncPos && p.syncCnt < 10) {
                p.syncCnt++;
                return ;
        p.next();
            }

            if (p.pos > p.syncPos) {
                p.syncPos = p.pos;
                p.syncCnt = 0;
                return ;
            } 
            // Reaching here indicates a parser bug, likely an
            // incorrect token list in this function, but it only
            // leads to skipping of possibly correct code if a
            // previous error is present, and thus is preferred
            // over a non-terminating parse.
        }
    }

}

private static map stmtStart = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<token.Token, bool>{token.BREAK:true,token.CONST:true,token.CONTINUE:true,token.DEFER:true,token.FALLTHROUGH:true,token.FOR:true,token.GO:true,token.GOTO:true,token.IF:true,token.RETURN:true,token.SELECT:true,token.SWITCH:true,token.TYPE:true,token.VAR:true,};

private static map declStart = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<token.Token, bool>{token.CONST:true,token.TYPE:true,token.VAR:true,};

private static map exprEnd = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<token.Token, bool>{token.COMMA:true,token.COLON:true,token.SEMICOLON:true,token.RPAREN:true,token.RBRACK:true,token.RBRACE:true,};

// safePos returns a valid file position for a given position: If pos
// is valid to begin with, safePos returns pos. If pos is out-of-range,
// safePos returns the EOF position.
//
// This is hack to work around "artificial" end positions in the AST which
// are computed by adding 1 to (presumably valid) token positions. If the
// token positions are invalid due to parse errors, the resulting end position
// may be past the file's EOF position, which would lead to panics if used
// later on.
//
private static token.Pos safePos(this ptr<parser> _addr_p, token.Pos pos) => func((defer, _, _) => {
    token.Pos res = default;
    ref parser p = ref _addr_p.val;

    defer(() => {
        if (recover() != null) {
            res = token.Pos(p.file.Base() + p.file.Size()); // EOF position
        }
    }());
    _ = p.file.Offset(pos); // trigger a panic if position is out-of-range
    return pos;

});

// ----------------------------------------------------------------------------
// Identifiers

private static ptr<ast.Ident> parseIdent(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;

    var pos = p.pos;
    @string name = "_";
    if (p.tok == token.IDENT) {
        name = p.lit;
        p.next();
    }
    else
 {
        p.expect(token.IDENT); // use expect() error handling
    }
    return addr(new ast.Ident(NamePos:pos,Name:name));

}

private static slice<ptr<ast.Ident>> parseIdentList(this ptr<parser> _addr_p) => func((defer, _, _) => {
    slice<ptr<ast.Ident>> list = default;
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "IdentList")));
    }
    list = append(list, p.parseIdent());
    while (p.tok == token.COMMA) {
        p.next();
        list = append(list, p.parseIdent());
    }

    return ;

});

// ----------------------------------------------------------------------------
// Common productions

// If lhs is set, result list elements which are identifiers are not resolved.
private static slice<ast.Expr> parseExprList(this ptr<parser> _addr_p) => func((defer, _, _) => {
    slice<ast.Expr> list = default;
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "ExpressionList")));
    }
    list = append(list, p.checkExpr(p.parseExpr()));
    while (p.tok == token.COMMA) {
        p.next();
        list = append(list, p.checkExpr(p.parseExpr()));
    }

    return ;

});

private static slice<ast.Expr> parseList(this ptr<parser> _addr_p, bool inRhs) {
    ref parser p = ref _addr_p.val;

    var old = p.inRhs;
    p.inRhs = inRhs;
    var list = p.parseExprList();
    p.inRhs = old;
    return list;
}

// ----------------------------------------------------------------------------
// Types

private static ast.Expr parseType(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "Type")));
    }
    var typ = p.tryIdentOrType();

    if (typ == null) {
        var pos = p.pos;
        p.errorExpected(pos, "type");
        p.advance(exprEnd);
        return addr(new ast.BadExpr(From:pos,To:p.pos));
    }
    return typ;

});

private static ast.Expr parseQualifiedIdent(this ptr<parser> _addr_p, ptr<ast.Ident> _addr_ident) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;
    ref ast.Ident ident = ref _addr_ident.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "QualifiedIdent")));
    }
    var typ = p.parseTypeName(ident);
    if (p.tok == token.LBRACK && p.parseTypeParams()) {
        typ = p.parseTypeInstance(typ);
    }
    return typ;

});

// If the result is an identifier, it is not resolved.
private static ast.Expr parseTypeName(this ptr<parser> _addr_p, ptr<ast.Ident> _addr_ident) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;
    ref ast.Ident ident = ref _addr_ident.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "TypeName")));
    }
    if (ident == null) {
        ident = p.parseIdent();
    }
    if (p.tok == token.PERIOD) { 
        // ident is a package name
        p.next();
        var sel = p.parseIdent();
        return addr(new ast.SelectorExpr(X:ident,Sel:sel));

    }
    return ident;

});

private static ast.Expr parseArrayLen(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "ArrayLen")));
    }
    p.exprLev++;
    ast.Expr len = default; 
    // always permit ellipsis for more fault-tolerant parsing
    if (p.tok == token.ELLIPSIS) {
        len = addr(new ast.Ellipsis(Ellipsis:p.pos));
        p.next();
    }
    else if (p.tok != token.RBRACK) {
        len = p.parseRhs();
    }
    p.exprLev--;

    return len;

});

private static (ptr<ast.Ident>, ast.Expr) parseArrayFieldOrTypeInstance(this ptr<parser> _addr_p, ptr<ast.Ident> _addr_x) => func((defer, _, _) => {
    ptr<ast.Ident> _p0 = default!;
    ast.Expr _p0 = default;
    ref parser p = ref _addr_p.val;
    ref ast.Ident x = ref _addr_x.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "ArrayFieldOrTypeInstance")));
    }
    var lbrack = p.expect(token.LBRACK);
    slice<ast.Expr> args = default;
    token.Pos firstComma = default; 
    // TODO(rfindley): consider changing parseRhsOrType so that this function variable
    // is not needed.
    var argparser = p.parseRhsOrType;
    if (!p.parseTypeParams()) {
        argparser = p.parseRhs;
    }
    if (p.tok != token.RBRACK) {
        p.exprLev++;
        args = append(args, argparser());
        while (p.tok == token.COMMA) {
            if (!firstComma.IsValid()) {
                firstComma = p.pos;
            }
            p.next();
            args = append(args, argparser());
        }
        p.exprLev--;
    }
    var rbrack = p.expect(token.RBRACK);

    if (len(args) == 0) { 
        // x []E
        var elt = p.parseType();
        return (_addr_x!, addr(new ast.ArrayType(Lbrack:lbrack,Elt:elt)));

    }
    if (len(args) == 1) {
        elt = p.tryIdentOrType();
        if (elt != null) { 
            // x [P]E
            return (_addr_x!, addr(new ast.ArrayType(Lbrack:lbrack,Len:args[0],Elt:elt)));

        }
        if (!p.parseTypeParams()) {
            p.error(rbrack, "missing element type in array type expression");
            return (_addr_null!, addr(new ast.BadExpr(From:args[0].Pos(),To:args[0].End())));
        }
    }
    if (!p.parseTypeParams()) {
        p.error(firstComma, "expected ']', found ','");
        return (_addr_x!, addr(new ast.BadExpr(From:args[0].Pos(),To:args[len(args)-1].End())));
    }
    return (_addr_null!, addr(new ast.IndexExpr(X:x,Lbrack:lbrack,Index:typeparams.PackExpr(args),Rbrack:rbrack)));

});

private static ptr<ast.Field> parseFieldDecl(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "FieldDecl")));
    }
    var doc = p.leadComment;

    slice<ptr<ast.Ident>> names = default;
    ast.Expr typ = default;
    if (p.tok == token.IDENT) {
        var name = p.parseIdent();
        if (p.tok == token.PERIOD || p.tok == token.STRING || p.tok == token.SEMICOLON || p.tok == token.RBRACE) { 
            // embedded type
            typ = name;
            if (p.tok == token.PERIOD) {
                typ = p.parseQualifiedIdent(name);
            }

        }
        else
 { 
            // name1, name2, ... T
            names = new slice<ptr<ast.Ident>>(new ptr<ast.Ident>[] { name });
            while (p.tok == token.COMMA) {
                p.next();
                names = append(names, p.parseIdent());
            } 
            // Careful dance: We don't know if we have an embedded instantiated
            // type T[P1, P2, ...] or a field T of array type []E or [P]E.
 
            // Careful dance: We don't know if we have an embedded instantiated
            // type T[P1, P2, ...] or a field T of array type []E or [P]E.
            if (len(names) == 1 && p.tok == token.LBRACK) {
                name, typ = p.parseArrayFieldOrTypeInstance(name);
                if (name == null) {
                    names = null;
                }
            }
            else
 { 
                // T P
                typ = p.parseType();

            }

        }
    else
    } { 
        // embedded, possibly generic type
        // (using the enclosing parentheses to distinguish it from a named field declaration)
        // TODO(rFindley) confirm that this doesn't allow parenthesized embedded type
        typ = p.parseType();

    }
    ptr<ast.BasicLit> tag;
    if (p.tok == token.STRING) {
        tag = addr(new ast.BasicLit(ValuePos:p.pos,Kind:p.tok,Value:p.lit));
        p.next();
    }
    p.expectSemi(); // call before accessing p.linecomment

    ptr<ast.Field> field = addr(new ast.Field(Doc:doc,Names:names,Type:typ,Tag:tag,Comment:p.lineComment));
    return _addr_field!;

});

private static ptr<ast.StructType> parseStructType(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "StructType")));
    }
    var pos = p.expect(token.STRUCT);
    var lbrace = p.expect(token.LBRACE);
    slice<ptr<ast.Field>> list = default;
    while (p.tok == token.IDENT || p.tok == token.MUL || p.tok == token.LPAREN) { 
        // a field declaration cannot start with a '(' but we accept
        // it here for more robust parsing and better error messages
        // (parseFieldDecl will check and complain if necessary)
        list = append(list, p.parseFieldDecl());

    }
    var rbrace = p.expect(token.RBRACE);

    return addr(new ast.StructType(Struct:pos,Fields:&ast.FieldList{Opening:lbrace,List:list,Closing:rbrace,},));

});

private static ptr<ast.StarExpr> parsePointerType(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "PointerType")));
    }
    var star = p.expect(token.MUL);
    var @base = p.parseType();

    return addr(new ast.StarExpr(Star:star,X:base));

});

private static ptr<ast.Ellipsis> parseDotsType(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "DotsType")));
    }
    var pos = p.expect(token.ELLIPSIS);
    var elt = p.parseType();

    return addr(new ast.Ellipsis(Ellipsis:pos,Elt:elt));

});

private partial struct field {
    public ptr<ast.Ident> name;
    public ast.Expr typ;
}

private static field parseParamDecl(this ptr<parser> _addr_p, ptr<ast.Ident> _addr_name) => func((defer, _, _) => {
    field f = default;
    ref parser p = ref _addr_p.val;
    ref ast.Ident name = ref _addr_name.val;
 
    // TODO(rFindley) compare with parser.paramDeclOrNil in the syntax package
    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "ParamDeclOrNil")));
    }
    var ptok = p.tok;
    if (name != null) {
        p.tok = token.IDENT; // force token.IDENT case in switch below
    }

    if (p.tok == token.IDENT) 
        if (name != null) {
            f.name = name;
            p.tok = ptok;
        }
        else
 {
            f.name = p.parseIdent();
        }

        if (p.tok == token.IDENT || p.tok == token.MUL || p.tok == token.ARROW || p.tok == token.FUNC || p.tok == token.CHAN || p.tok == token.MAP || p.tok == token.STRUCT || p.tok == token.INTERFACE || p.tok == token.LPAREN) 
            // name type
            f.typ = p.parseType();
        else if (p.tok == token.LBRACK) 
            // name[type1, type2, ...] or name []type or name [len]type
            f.name, f.typ = p.parseArrayFieldOrTypeInstance(f.name);
        else if (p.tok == token.ELLIPSIS) 
            // name ...type
            f.typ = p.parseDotsType();
        else if (p.tok == token.PERIOD) 
            // qualified.typename
            f.typ = p.parseQualifiedIdent(f.name);
            f.name = null;
            else if (p.tok == token.MUL || p.tok == token.ARROW || p.tok == token.FUNC || p.tok == token.LBRACK || p.tok == token.CHAN || p.tok == token.MAP || p.tok == token.STRUCT || p.tok == token.INTERFACE || p.tok == token.LPAREN) 
        // type
        f.typ = p.parseType();
    else if (p.tok == token.ELLIPSIS) 
        // ...type
        // (always accepted)
        f.typ = p.parseDotsType();
    else 
        p.errorExpected(p.pos, ")");
        p.advance(exprEnd);
        return ;

});

private static slice<ptr<ast.Field>> parseParameterList(this ptr<parser> _addr_p, ptr<ast.Ident> _addr_name0, token.Token closing, Func<ptr<ast.Ident>, field> parseParamDecl, bool tparams) => func((defer, _, _) => {
    slice<ptr<ast.Field>> @params = default;
    ref parser p = ref _addr_p.val;
    ref ast.Ident name0 = ref _addr_name0.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "ParameterList")));
    }
    var pos = p.pos;
    if (name0 != null) {
        pos = name0.Pos();
    }
    slice<field> list = default;
    nint named = default; // number of parameters that have an explicit name and type

    while (name0 != null || p.tok != closing && p.tok != token.EOF) {
        var par = parseParamDecl(name0);
        name0 = null; // 1st name was consumed if present
        if (par.name != null || par.typ != null) {
            list = append(list, par);
            if (par.name != null && par.typ != null) {
                named++;
            }
        }
        if (!p.atComma("parameter list", closing)) {
            break;
        }
        p.next();

    }

    if (len(list) == 0) {
        return ; // not uncommon
    }
    if (named == 0) { 
        // all unnamed => found names are type names
        {
            nint i__prev1 = i;

            for (nint i = 0; i < len(list); i++) {
                par = _addr_list[i];
                {
                    var typ__prev2 = typ;

                    var typ = par.name;

                    if (typ != null) {
                        par.typ = typ;
                        par.name = null;
                    }

                    typ = typ__prev2;

                }

            }


            i = i__prev1;
        }
        if (tparams) {
            p.error(pos, "all type parameters must be named");
        }
    }
    else if (named != len(list)) { 
        // some named => all must be named
        var ok = true;
        typ = default;
        {
            nint i__prev1 = i;

            for (i = len(list) - 1; i >= 0; i--) {
                {
                    var par__prev3 = par;

                    par = _addr_list[i];

                    if (par.typ != null) {
                        typ = par.typ;
                        if (par.name == null) {
                            ok = false;
                            var n = ast.NewIdent("_");
                            n.NamePos = typ.Pos(); // correct position
                            par.name = n;

                        }

                    }
                    else if (typ != null) {
                        par.typ = typ;
                    }
                    else
 { 
                        // par.typ == nil && typ == nil => we only have a par.name
                        ok = false;
                        par.typ = addr(new ast.BadExpr(From:par.name.Pos(),To:p.pos));

                    }


                    par = par__prev3;

                }

            }


            i = i__prev1;
        }
        if (!ok) {
            if (tparams) {
                p.error(pos, "all type parameters must be named");
            }
            else
 {
                p.error(pos, "mixed named and unnamed parameters");
            }

        }
    }
    if (named == 0) { 
        // parameter list consists of types only
        {
            var par__prev1 = par;

            foreach (var (_, __par) in list) {
                par = __par;
                assert(par.typ != null, "nil type in unnamed parameter list");
                params = append(params, addr(new ast.Field(Type:par.typ)));
            }

            par = par__prev1;
        }

        return ;

    }
    slice<ptr<ast.Ident>> names = default;
    typ = default;
    Action addParams = () => {
        assert(typ != null, "nil type in named parameter list");
        ptr<ast.Field> field = addr(new ast.Field(Names:names,Type:typ));
        params = append(params, field);
        names = null;
    };
    {
        var par__prev1 = par;

        foreach (var (_, __par) in list) {
            par = __par;
            if (par.typ != typ) {
                if (len(names) > 0) {
                    addParams();
                }
                typ = par.typ;
            }
            names = append(names, par.name);
        }
        par = par__prev1;
    }

    if (len(names) > 0) {
        addParams();
    }
    return ;

});

private static (ptr<ast.FieldList>, ptr<ast.FieldList>) parseParameters(this ptr<parser> _addr_p, bool acceptTParams) => func((defer, _, _) => {
    ptr<ast.FieldList> tparams = default!;
    ptr<ast.FieldList> @params = default!;
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "Parameters")));
    }
    if (p.parseTypeParams() && acceptTParams && p.tok == token.LBRACK) {
        var opening = p.pos;
        p.next(); 
        // [T any](params) syntax
        var list = p.parseParameterList(null, token.RBRACK, p.parseParamDecl, true);
        var rbrack = p.expect(token.RBRACK);
        tparams = addr(new ast.FieldList(Opening:opening,List:list,Closing:rbrack)); 
        // Type parameter lists must not be empty.
        if (tparams.NumFields() == 0) {
            p.error(tparams.Closing, "empty type parameter list");
            tparams = null; // avoid follow-on errors
        }
    }
    opening = p.expect(token.LPAREN);

    slice<ptr<ast.Field>> fields = default;
    if (p.tok != token.RPAREN) {
        fields = p.parseParameterList(null, token.RPAREN, p.parseParamDecl, false);
    }
    var rparen = p.expect(token.RPAREN);
    params = addr(new ast.FieldList(Opening:opening,List:fields,Closing:rparen));

    return ;

});

private static ptr<ast.FieldList> parseResult(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "Result")));
    }
    if (p.tok == token.LPAREN) {
        var (_, results) = p.parseParameters(false);
        return _addr_results!;
    }
    var typ = p.tryIdentOrType();
    if (typ != null) {
        var list = make_slice<ptr<ast.Field>>(1);
        list[0] = addr(new ast.Field(Type:typ));
        return addr(new ast.FieldList(List:list));
    }
    return _addr_null!;

});

private static ptr<ast.FuncType> parseFuncType(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "FuncType")));
    }
    var pos = p.expect(token.FUNC);
    var (tparams, params) = p.parseParameters(true);
    if (tparams != null) {
        p.error(tparams.Pos(), "function type cannot have type parameters");
    }
    var results = p.parseResult();

    return addr(new ast.FuncType(Func:pos,Params:params,Results:results));

});

private static ptr<ast.Field> parseMethodSpec(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "MethodSpec")));
    }
    var doc = p.leadComment;
    slice<ptr<ast.Ident>> idents = default;
    ast.Expr typ = default;
    var x = p.parseTypeName(null);
    {
        ptr<ast.Ident> (ident, _) = x._<ptr<ast.Ident>>();

        if (ident != null) {

            if (p.tok == token.LBRACK && p.parseTypeParams()) 
                // generic method or embedded instantiated type
                var lbrack = p.pos;
                p.next();
                p.exprLev++;
                x = p.parseExpr();
                p.exprLev--;
                {
                    ptr<ast.Ident> (name0, _) = x._<ptr<ast.Ident>>();

                    if (name0 != null && p.tok != token.COMMA && p.tok != token.RBRACK) { 
                        // generic method m[T any]
                        var list = p.parseParameterList(name0, token.RBRACK, p.parseParamDecl, true);
                        var rbrack = p.expect(token.RBRACK);
                        ptr<ast.FieldList> tparams = addr(new ast.FieldList(Opening:lbrack,List:list,Closing:rbrack)); 
                        // TODO(rfindley) refactor to share code with parseFuncType.
                        var (_, params) = p.parseParameters(false);
                        var results = p.parseResult();
                        idents = new slice<ptr<ast.Ident>>(new ptr<ast.Ident>[] { ident });
                        typ = addr(new ast.FuncType(Func:token.NoPos,Params:params,Results:results));
                        typeparams.Set(typ, tparams);

                    }
                    else
 { 
                        // embedded instantiated type
                        // TODO(rfindley) should resolve all identifiers in x.
                        list = new slice<ast.Expr>(new ast.Expr[] { x });
                        if (p.atComma("type argument list", token.RBRACK)) {
                            p.exprLev++;
                            while (p.tok != token.RBRACK && p.tok != token.EOF) {
                                list = append(list, p.parseType());
                                if (!p.atComma("type argument list", token.RBRACK)) {
                                    break;
                                }
                                p.next();
                            }

                            p.exprLev--;
                        }

                        rbrack = p.expectClosing(token.RBRACK, "type argument list");
                        typ = addr(new ast.IndexExpr(X:ident,Lbrack:lbrack,Index:typeparams.PackExpr(list),Rbrack:rbrack));

                    }
        else

                }

            else if (p.tok == token.LPAREN) 
                // ordinary method
                // TODO(rfindley) refactor to share code with parseFuncType.
                (_, params) = p.parseParameters(false);
                results = p.parseResult();
                idents = new slice<ptr<ast.Ident>>(new ptr<ast.Ident>[] { ident });
                typ = addr(new ast.FuncType(Func:token.NoPos,Params:params,Results:results));
            else 
                // embedded type
                typ = x;
            
        } { 
            // embedded, possibly instantiated type
            typ = x;
            if (p.tok == token.LBRACK && p.parseTypeParams()) { 
                // embedded instantiated interface
                typ = p.parseTypeInstance(typ);

            }

        }
    }

    p.expectSemi(); // call before accessing p.linecomment

    ptr<ast.Field> spec = addr(new ast.Field(Doc:doc,Names:idents,Type:typ,Comment:p.lineComment));

    return _addr_spec!;

});

private static ptr<ast.InterfaceType> parseInterfaceType(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "InterfaceType")));
    }
    var pos = p.expect(token.INTERFACE);
    var lbrace = p.expect(token.LBRACE);
    slice<ptr<ast.Field>> list = default;
    while (p.tok == token.IDENT || p.parseTypeParams() && p.tok == token.TYPE) {
        if (p.tok == token.IDENT) {
            list = append(list, p.parseMethodSpec());
        }
        else
 { 
            // all types in a type list share the same field name "type"
            // (since type is a keyword, a Go program cannot have that field name)
            ptr<ast.Ident> name = new slice<ptr<ast.Ident>>(new ptr<ast.Ident>[] { {NamePos:p.pos,Name:"type"} });
            p.next(); 
            // add each type as a field named "type"
            foreach (var (_, typ) in p.parseTypeList()) {
                list = append(list, addr(new ast.Field(Names:name,Type:typ)));
            }
            p.expectSemi();

        }
    } 
    // TODO(rfindley): the error produced here could be improved, since we could
    // accept a identifier, 'type', or a '}' at this point.
    var rbrace = p.expect(token.RBRACE);

    return addr(new ast.InterfaceType(Interface:pos,Methods:&ast.FieldList{Opening:lbrace,List:list,Closing:rbrace,},));

});

private static ptr<ast.MapType> parseMapType(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "MapType")));
    }
    var pos = p.expect(token.MAP);
    p.expect(token.LBRACK);
    var key = p.parseType();
    p.expect(token.RBRACK);
    var value = p.parseType();

    return addr(new ast.MapType(Map:pos,Key:key,Value:value));

});

private static ptr<ast.ChanType> parseChanType(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "ChanType")));
    }
    var pos = p.pos;
    var dir = ast.SEND | ast.RECV;
    token.Pos arrow = default;
    if (p.tok == token.CHAN) {
        p.next();
        if (p.tok == token.ARROW) {
            arrow = p.pos;
            p.next();
            dir = ast.SEND;
        }
    }
    else
 {
        arrow = p.expect(token.ARROW);
        p.expect(token.CHAN);
        dir = ast.RECV;
    }
    var value = p.parseType();

    return addr(new ast.ChanType(Begin:pos,Arrow:arrow,Dir:dir,Value:value));

});

private static ast.Expr parseTypeInstance(this ptr<parser> _addr_p, ast.Expr typ) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    assert(p.parseTypeParams(), "parseTypeInstance while not parsing type params");
    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "TypeInstance")));
    }
    var opening = p.expect(token.LBRACK);

    p.exprLev++;
    slice<ast.Expr> list = default;
    while (p.tok != token.RBRACK && p.tok != token.EOF) {
        list = append(list, p.parseType());
        if (!p.atComma("type argument list", token.RBRACK)) {
            break;
        }
        p.next();

    }
    p.exprLev--;

    var closing = p.expectClosing(token.RBRACK, "type argument list");

    return addr(new ast.IndexExpr(X:typ,Lbrack:opening,Index:typeparams.PackExpr(list),Rbrack:closing));

});

private static ast.Expr tryIdentOrType(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;


    if (p.tok == token.IDENT) 
        var typ = p.parseTypeName(null);
        if (p.tok == token.LBRACK && p.parseTypeParams()) {
            typ = p.parseTypeInstance(typ);
        }
        return typ;
    else if (p.tok == token.LBRACK) 
        var lbrack = p.expect(token.LBRACK);
        var alen = p.parseArrayLen();
        p.expect(token.RBRACK);
        var elt = p.parseType();
        return addr(new ast.ArrayType(Lbrack:lbrack,Len:alen,Elt:elt));
    else if (p.tok == token.STRUCT) 
        return p.parseStructType();
    else if (p.tok == token.MUL) 
        return p.parsePointerType();
    else if (p.tok == token.FUNC) 
        typ = p.parseFuncType();
        return typ;
    else if (p.tok == token.INTERFACE) 
        return p.parseInterfaceType();
    else if (p.tok == token.MAP) 
        return p.parseMapType();
    else if (p.tok == token.CHAN || p.tok == token.ARROW) 
        return p.parseChanType();
    else if (p.tok == token.LPAREN) 
        var lparen = p.pos;
        p.next();
        typ = p.parseType();
        var rparen = p.expect(token.RPAREN);
        return addr(new ast.ParenExpr(Lparen:lparen,X:typ,Rparen:rparen));
    // no type found
    return null;

}

// ----------------------------------------------------------------------------
// Blocks

private static slice<ast.Stmt> parseStmtList(this ptr<parser> _addr_p) => func((defer, _, _) => {
    slice<ast.Stmt> list = default;
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "StatementList")));
    }
    while (p.tok != token.CASE && p.tok != token.DEFAULT && p.tok != token.RBRACE && p.tok != token.EOF) {
        list = append(list, p.parseStmt());
    }

    return ;

});

private static ptr<ast.BlockStmt> parseBody(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "Body")));
    }
    var lbrace = p.expect(token.LBRACE);
    var list = p.parseStmtList();
    var rbrace = p.expect2(token.RBRACE);

    return addr(new ast.BlockStmt(Lbrace:lbrace,List:list,Rbrace:rbrace));

});

private static ptr<ast.BlockStmt> parseBlockStmt(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "BlockStmt")));
    }
    var lbrace = p.expect(token.LBRACE);
    var list = p.parseStmtList();
    var rbrace = p.expect2(token.RBRACE);

    return addr(new ast.BlockStmt(Lbrace:lbrace,List:list,Rbrace:rbrace));

});

// ----------------------------------------------------------------------------
// Expressions

private static ast.Expr parseFuncTypeOrLit(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "FuncTypeOrLit")));
    }
    var typ = p.parseFuncType();
    if (p.tok != token.LBRACE) { 
        // function type only
        return typ;

    }
    p.exprLev++;
    var body = p.parseBody();
    p.exprLev--;

    return addr(new ast.FuncLit(Type:typ,Body:body));

});

// parseOperand may return an expression or a raw type (incl. array
// types of the form [...]T. Callers must verify the result.
//
private static ast.Expr parseOperand(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "Operand")));
    }

    if (p.tok == token.IDENT) 
        var x = p.parseIdent();
        return x;
    else if (p.tok == token.INT || p.tok == token.FLOAT || p.tok == token.IMAG || p.tok == token.CHAR || p.tok == token.STRING) 
        x = addr(new ast.BasicLit(ValuePos:p.pos,Kind:p.tok,Value:p.lit));
        p.next();
        return x;
    else if (p.tok == token.LPAREN) 
        var lparen = p.pos;
        p.next();
        p.exprLev++;
        x = p.parseRhsOrType(); // types may be parenthesized: (some type)
        p.exprLev--;
        var rparen = p.expect(token.RPAREN);
        return addr(new ast.ParenExpr(Lparen:lparen,X:x,Rparen:rparen));
    else if (p.tok == token.FUNC) 
        return p.parseFuncTypeOrLit();
        {
        var typ = p.tryIdentOrType();

        if (typ != null) { // do not consume trailing type parameters
            // could be type for composite literal or conversion
            ptr<ast.Ident> (_, isIdent) = typ._<ptr<ast.Ident>>();
            assert(!isIdent, "type cannot be identifier");
            return typ;

        }
    } 

    // we have an error
    var pos = p.pos;
    p.errorExpected(pos, "operand");
    p.advance(stmtStart);
    return addr(new ast.BadExpr(From:pos,To:p.pos));

});

private static ast.Expr parseSelector(this ptr<parser> _addr_p, ast.Expr x) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "Selector")));
    }
    var sel = p.parseIdent();

    return addr(new ast.SelectorExpr(X:x,Sel:sel));

});

private static ast.Expr parseTypeAssertion(this ptr<parser> _addr_p, ast.Expr x) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "TypeAssertion")));
    }
    var lparen = p.expect(token.LPAREN);
    ast.Expr typ = default;
    if (p.tok == token.TYPE) { 
        // type switch: typ == nil
        p.next();

    }
    else
 {
        typ = p.parseType();
    }
    var rparen = p.expect(token.RPAREN);

    return addr(new ast.TypeAssertExpr(X:x,Type:typ,Lparen:lparen,Rparen:rparen));

});

private static ast.Expr parseIndexOrSliceOrInstance(this ptr<parser> _addr_p, ast.Expr x) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "parseIndexOrSliceOrInstance")));
    }
    var lbrack = p.expect(token.LBRACK);
    if (p.tok == token.RBRACK) { 
        // empty index, slice or index expressions are not permitted;
        // accept them for parsing tolerance, but complain
        p.errorExpected(p.pos, "operand");
        var rbrack = p.pos;
        p.next();
        return addr(new ast.IndexExpr(X:x,Lbrack:lbrack,Index:&ast.BadExpr{From:rbrack,To:rbrack},Rbrack:rbrack,));

    }
    p.exprLev++;

    const nint N = 3; // change the 3 to 2 to disable 3-index slices
 // change the 3 to 2 to disable 3-index slices
    slice<ast.Expr> args = default;
    array<ast.Expr> index = new array<ast.Expr>(N);
    array<token.Pos> colons = new array<token.Pos>(N - 1);
    token.Pos firstComma = default;
    if (p.tok != token.COLON) { 
        // We can't know if we have an index expression or a type instantiation;
        // so even if we see a (named) type we are not going to be in type context.
        index[0] = p.parseRhsOrType();

    }
    nint ncolons = 0;

    if (p.tok == token.COLON) 
        // slice expression
        while (p.tok == token.COLON && ncolons < len(colons)) {
            colons[ncolons] = p.pos;
            ncolons++;
            p.next();
            if (p.tok != token.COLON && p.tok != token.RBRACK && p.tok != token.EOF) {
                index[ncolons] = p.parseRhs();
            }
        }
    else if (p.tok == token.COMMA) 
        firstComma = p.pos; 
        // instance expression
        args = append(args, index[0]);
        while (p.tok == token.COMMA) {
            p.next();
            if (p.tok != token.RBRACK && p.tok != token.EOF) {
                args = append(args, p.parseType());
            }
        }
        p.exprLev--;
    rbrack = p.expect(token.RBRACK);

    if (ncolons > 0) { 
        // slice expression
        var slice3 = false;
        if (ncolons == 2) {
            slice3 = true; 
            // Check presence of 2nd and 3rd index here rather than during type-checking
            // to prevent erroneous programs from passing through gofmt (was issue 7305).
            if (index[1] == null) {
                p.error(colons[0], "2nd index required in 3-index slice");
                index[1] = addr(new ast.BadExpr(From:colons[0]+1,To:colons[1]));
            }

            if (index[2] == null) {
                p.error(colons[1], "3rd index required in 3-index slice");
                index[2] = addr(new ast.BadExpr(From:colons[1]+1,To:rbrack));
            }

        }
        return addr(new ast.SliceExpr(X:x,Lbrack:lbrack,Low:index[0],High:index[1],Max:index[2],Slice3:slice3,Rbrack:rbrack));

    }
    if (len(args) == 0) { 
        // index expression
        return addr(new ast.IndexExpr(X:x,Lbrack:lbrack,Index:index[0],Rbrack:rbrack));

    }
    if (!p.parseTypeParams()) {
        p.error(firstComma, "expected ']' or ':', found ','");
        return addr(new ast.BadExpr(From:args[0].Pos(),To:args[len(args)-1].End()));
    }
    return addr(new ast.IndexExpr(X:x,Lbrack:lbrack,Index:typeparams.PackExpr(args),Rbrack:rbrack));

});

private static ptr<ast.CallExpr> parseCallOrConversion(this ptr<parser> _addr_p, ast.Expr fun) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "CallOrConversion")));
    }
    var lparen = p.expect(token.LPAREN);
    p.exprLev++;
    slice<ast.Expr> list = default;
    token.Pos ellipsis = default;
    while (p.tok != token.RPAREN && p.tok != token.EOF && !ellipsis.IsValid()) {
        list = append(list, p.parseRhsOrType()); // builtins may expect a type: make(some type, ...)
        if (p.tok == token.ELLIPSIS) {
            ellipsis = p.pos;
            p.next();
        }
        if (!p.atComma("argument list", token.RPAREN)) {
            break;
        }
        p.next();

    }
    p.exprLev--;
    var rparen = p.expectClosing(token.RPAREN, "argument list");

    return addr(new ast.CallExpr(Fun:fun,Lparen:lparen,Args:list,Ellipsis:ellipsis,Rparen:rparen));

});

private static ast.Expr parseValue(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "Element")));
    }
    if (p.tok == token.LBRACE) {
        return p.parseLiteralValue(null);
    }
    var x = p.checkExpr(p.parseExpr());

    return x;

});

private static ast.Expr parseElement(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "Element")));
    }
    var x = p.parseValue();
    if (p.tok == token.COLON) {
        var colon = p.pos;
        p.next();
        x = addr(new ast.KeyValueExpr(Key:x,Colon:colon,Value:p.parseValue()));
    }
    return x;

});

private static slice<ast.Expr> parseElementList(this ptr<parser> _addr_p) => func((defer, _, _) => {
    slice<ast.Expr> list = default;
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "ElementList")));
    }
    while (p.tok != token.RBRACE && p.tok != token.EOF) {
        list = append(list, p.parseElement());
        if (!p.atComma("composite literal", token.RBRACE)) {
            break;
        }
        p.next();

    }

    return ;

});

private static ast.Expr parseLiteralValue(this ptr<parser> _addr_p, ast.Expr typ) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "LiteralValue")));
    }
    var lbrace = p.expect(token.LBRACE);
    slice<ast.Expr> elts = default;
    p.exprLev++;
    if (p.tok != token.RBRACE) {
        elts = p.parseElementList();
    }
    p.exprLev--;
    var rbrace = p.expectClosing(token.RBRACE, "composite literal");
    return addr(new ast.CompositeLit(Type:typ,Lbrace:lbrace,Elts:elts,Rbrace:rbrace));

});

// checkExpr checks that x is an expression (and not a type).
private static ast.Expr checkExpr(this ptr<parser> _addr_p, ast.Expr x) => func((_, panic, _) => {
    ref parser p = ref _addr_p.val;

    switch (unparen(x).type()) {
        case ptr<ast.BadExpr> _:
            break;
        case ptr<ast.Ident> _:
            break;
        case ptr<ast.BasicLit> _:
            break;
        case ptr<ast.FuncLit> _:
            break;
        case ptr<ast.CompositeLit> _:
            break;
        case ptr<ast.ParenExpr> _:
            panic("unreachable");
            break;
        case ptr<ast.SelectorExpr> _:
            break;
        case ptr<ast.IndexExpr> _:
            break;
        case ptr<ast.SliceExpr> _:
            break;
        case ptr<ast.TypeAssertExpr> _:
            break;
        case ptr<ast.CallExpr> _:
            break;
        case ptr<ast.StarExpr> _:
            break;
        case ptr<ast.UnaryExpr> _:
            break;
        case ptr<ast.BinaryExpr> _:
            break;
        default:
        {
            p.errorExpected(x.Pos(), "expression");
            x = addr(new ast.BadExpr(From:x.Pos(),To:p.safePos(x.End())));
            break;
        }
    }
    return x;

});

// If x is of the form (T), unparen returns unparen(T), otherwise it returns x.
private static ast.Expr unparen(ast.Expr x) {
    {
        ptr<ast.ParenExpr> (p, isParen) = x._<ptr<ast.ParenExpr>>();

        if (isParen) {
            x = unparen(p.X);
        }
    }

    return x;

}

// checkExprOrType checks that x is an expression or a type
// (and not a raw type such as [...]T).
//
private static ast.Expr checkExprOrType(this ptr<parser> _addr_p, ast.Expr x) => func((_, panic, _) => {
    ref parser p = ref _addr_p.val;

    switch (unparen(x).type()) {
        case ptr<ast.ParenExpr> t:
            panic("unreachable");
            break;
        case ptr<ast.ArrayType> t:
            {
                ptr<ast.Ellipsis> (len, isEllipsis) = t.Len._<ptr<ast.Ellipsis>>();

                if (isEllipsis) {
                    p.error(len.Pos(), "expected array length, found '...'");
                    x = addr(new ast.BadExpr(From:x.Pos(),To:p.safePos(x.End())));
                }

            }

            break; 

        // all other nodes are expressions or types
    } 

    // all other nodes are expressions or types
    return x;

});

private static ast.Expr parsePrimaryExpr(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ast.Expr x = default;
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "PrimaryExpr")));
    }
    x = p.parseOperand();
    while (true) {

        if (p.tok == token.PERIOD) 
            p.next();

            if (p.tok == token.IDENT) 
                x = p.parseSelector(p.checkExprOrType(x));
            else if (p.tok == token.LPAREN) 
                x = p.parseTypeAssertion(p.checkExpr(x));
            else 
                var pos = p.pos;
                p.errorExpected(pos, "selector or type assertion"); 
                // TODO(rFindley) The check for token.RBRACE below is a targeted fix
                //                to error recovery sufficient to make the x/tools tests to
                //                pass with the new parsing logic introduced for type
                //                parameters. Remove this once error recovery has been
                //                more generally reconsidered.
                if (p.tok != token.RBRACE) {
                    p.next(); // make progress
                }

                ptr<ast.Ident> sel = addr(new ast.Ident(NamePos:pos,Name:"_"));
                x = addr(new ast.SelectorExpr(X:x,Sel:sel));
                    else if (p.tok == token.LBRACK) 
            x = p.parseIndexOrSliceOrInstance(p.checkExpr(x));
        else if (p.tok == token.LPAREN) 
            x = p.parseCallOrConversion(p.checkExprOrType(x));
        else if (p.tok == token.LBRACE) 
            // operand may have returned a parenthesized complit
            // type; accept it but complain if we have a complit
            var t = unparen(x); 
            // determine if '{' belongs to a composite literal or a block statement
            switch (t.type()) {
                case ptr<ast.BadExpr> _:
                    if (p.exprLev < 0) {
                        return ;
                    } 
                    // x is possibly a composite literal type
                    break;
                case ptr<ast.Ident> _:
                    if (p.exprLev < 0) {
                        return ;
                    } 
                    // x is possibly a composite literal type
                    break;
                case ptr<ast.SelectorExpr> _:
                    if (p.exprLev < 0) {
                        return ;
                    } 
                    // x is possibly a composite literal type
                    break;
                case ptr<ast.IndexExpr> _:
                    if (p.exprLev < 0) {
                        return ;
                    } 
                    // x is possibly a composite literal type
                    break;
                case ptr<ast.ArrayType> _:
                    break;
                case ptr<ast.StructType> _:
                    break;
                case ptr<ast.MapType> _:
                    break;
                default:
                {
                    return ;
                    break;
                }
            }
            if (t != x) {
                p.error(t.Pos(), "cannot parenthesize type in composite literal"); 
                // already progressed, no need to advance
            }

            x = p.parseLiteralValue(x);
        else 
            return ;
        
    }

});

private static ast.Expr parseUnaryExpr(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "UnaryExpr")));
    }

    if (p.tok == token.ADD || p.tok == token.SUB || p.tok == token.NOT || p.tok == token.XOR || p.tok == token.AND) 
        var pos = p.pos;
        var op = p.tok;
        p.next();
        var x = p.parseUnaryExpr();
        return addr(new ast.UnaryExpr(OpPos:pos,Op:op,X:p.checkExpr(x)));
    else if (p.tok == token.ARROW) 
        // channel type or receive expression
        var arrow = p.pos;
        p.next(); 

        // If the next token is token.CHAN we still don't know if it
        // is a channel type or a receive operation - we only know
        // once we have found the end of the unary expression. There
        // are two cases:
        //
        //   <- type  => (<-type) must be channel type
        //   <- expr  => <-(expr) is a receive from an expression
        //
        // In the first case, the arrow must be re-associated with
        // the channel type parsed already:
        //
        //   <- (chan type)    =>  (<-chan type)
        //   <- (chan<- type)  =>  (<-chan (<-type))

        x = p.parseUnaryExpr(); 

        // determine which case we have
        {
            ptr<ast.ChanType> (typ, ok) = x._<ptr<ast.ChanType>>();

            if (ok) { 
                // (<-type)

                // re-associate position info and <-
                var dir = ast.SEND;
                while (ok && dir == ast.SEND) {
                    if (typ.Dir == ast.RECV) { 
                        // error: (<-type) is (<-(<-chan T))
                        p.errorExpected(typ.Arrow, "'chan'");

                    }

                    (arrow, typ.Begin, typ.Arrow) = (typ.Arrow, arrow, arrow);                    (dir, typ.Dir) = (typ.Dir, ast.RECV);                    typ, ok = typ.Value._<ptr<ast.ChanType>>();

                }

                if (dir == ast.SEND) {
                    p.errorExpected(arrow, "channel type");
                }

                return x;

            } 

            // <-(expr)

        } 

        // <-(expr)
        return addr(new ast.UnaryExpr(OpPos:arrow,Op:token.ARROW,X:p.checkExpr(x)));
    else if (p.tok == token.MUL) 
        // pointer type or unary "*" expression
        pos = p.pos;
        p.next();
        x = p.parseUnaryExpr();
        return addr(new ast.StarExpr(Star:pos,X:p.checkExprOrType(x)));
        return p.parsePrimaryExpr();

});

private static (token.Token, nint) tokPrec(this ptr<parser> _addr_p) {
    token.Token _p0 = default;
    nint _p0 = default;
    ref parser p = ref _addr_p.val;

    var tok = p.tok;
    if (p.inRhs && tok == token.ASSIGN) {
        tok = token.EQL;
    }
    return (tok, tok.Precedence());

}

private static ast.Expr parseBinaryExpr(this ptr<parser> _addr_p, nint prec1) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "BinaryExpr")));
    }
    var x = p.parseUnaryExpr();
    while (true) {
        var (op, oprec) = p.tokPrec();
        if (oprec < prec1) {
            return x;
        }
        var pos = p.expect(op);
        var y = p.parseBinaryExpr(oprec + 1);
        x = addr(new ast.BinaryExpr(X:p.checkExpr(x),OpPos:pos,Op:op,Y:p.checkExpr(y)));

    }

});

// The result may be a type or even a raw type ([...]int). Callers must
// check the result (using checkExpr or checkExprOrType), depending on
// context.
private static ast.Expr parseExpr(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "Expression")));
    }
    return p.parseBinaryExpr(token.LowestPrec + 1);

});

private static ast.Expr parseRhs(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;

    var old = p.inRhs;
    p.inRhs = true;
    var x = p.checkExpr(p.parseExpr());
    p.inRhs = old;
    return x;
}

private static ast.Expr parseRhsOrType(this ptr<parser> _addr_p) {
    ref parser p = ref _addr_p.val;

    var old = p.inRhs;
    p.inRhs = true;
    var x = p.checkExprOrType(p.parseExpr());
    p.inRhs = old;
    return x;
}

// ----------------------------------------------------------------------------
// Statements

// Parsing modes for parseSimpleStmt.
private static readonly var basic = iota;
private static readonly var labelOk = 0;
private static readonly var rangeOk = 1;


// parseSimpleStmt returns true as 2nd result if it parsed the assignment
// of a range clause (with mode == rangeOk). The returned statement is an
// assignment with a right-hand side that is a single unary expression of
// the form "range x". No guarantees are given for the left-hand side.
private static (ast.Stmt, bool) parseSimpleStmt(this ptr<parser> _addr_p, nint mode) => func((defer, _, _) => {
    ast.Stmt _p0 = default;
    bool _p0 = default;
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "SimpleStmt")));
    }
    var x = p.parseList(false);


    if (p.tok == token.DEFINE || p.tok == token.ASSIGN || p.tok == token.ADD_ASSIGN || p.tok == token.SUB_ASSIGN || p.tok == token.MUL_ASSIGN || p.tok == token.QUO_ASSIGN || p.tok == token.REM_ASSIGN || p.tok == token.AND_ASSIGN || p.tok == token.OR_ASSIGN || p.tok == token.XOR_ASSIGN || p.tok == token.SHL_ASSIGN || p.tok == token.SHR_ASSIGN || p.tok == token.AND_NOT_ASSIGN) 
        // assignment statement, possibly part of a range clause
        var pos = p.pos;
        var tok = p.tok;
        p.next();
        slice<ast.Expr> y = default;
        var isRange = false;
        if (mode == rangeOk && p.tok == token.RANGE && (tok == token.DEFINE || tok == token.ASSIGN)) {
            pos = p.pos;
            p.next();
            y = new slice<ast.Expr>(new ast.Expr[] { &ast.UnaryExpr{OpPos:pos,Op:token.RANGE,X:p.parseRhs()} });
            isRange = true;
        }
        else
 {
            y = p.parseList(true);
        }
        ptr<ast.AssignStmt> @as = addr(new ast.AssignStmt(Lhs:x,TokPos:pos,Tok:tok,Rhs:y));
        if (tok == token.DEFINE) {
            p.checkAssignStmt(as);
        }
        return (as, isRange);
        if (len(x) > 1) {
        p.errorExpected(x[0].Pos(), "1 expression"); 
        // continue with first expression
    }

    if (p.tok == token.COLON) 
        // labeled statement
        var colon = p.pos;
        p.next();
        {
            ptr<ast.Ident> (label, isIdent) = x[0]._<ptr<ast.Ident>>();

            if (mode == labelOk && isIdent) { 
                // Go spec: The scope of a label is the body of the function
                // in which it is declared and excludes the body of any nested
                // function.
                ptr<ast.LabeledStmt> stmt = addr(new ast.LabeledStmt(Label:label,Colon:colon,Stmt:p.parseStmt()));
                return (stmt, false);

            } 
            // The label declaration typically starts at x[0].Pos(), but the label
            // declaration may be erroneous due to a token after that position (and
            // before the ':'). If SpuriousErrors is not set, the (only) error
            // reported for the line is the illegal label error instead of the token
            // before the ':' that caused the problem. Thus, use the (latest) colon
            // position for error reporting.

        } 
        // The label declaration typically starts at x[0].Pos(), but the label
        // declaration may be erroneous due to a token after that position (and
        // before the ':'). If SpuriousErrors is not set, the (only) error
        // reported for the line is the illegal label error instead of the token
        // before the ':' that caused the problem. Thus, use the (latest) colon
        // position for error reporting.
        p.error(colon, "illegal label declaration");
        return (addr(new ast.BadStmt(From:x[0].Pos(),To:colon+1)), false);
    else if (p.tok == token.ARROW) 
        // send statement
        var arrow = p.pos;
        p.next();
        y = p.parseRhs();
        return (addr(new ast.SendStmt(Chan:x[0],Arrow:arrow,Value:y)), false);
    else if (p.tok == token.INC || p.tok == token.DEC) 
        // increment or decrement
        ptr<ast.IncDecStmt> s = addr(new ast.IncDecStmt(X:x[0],TokPos:p.pos,Tok:p.tok));
        p.next();
        return (s, false);
    // expression
    return (addr(new ast.ExprStmt(X:x[0])), false);

});

private static void checkAssignStmt(this ptr<parser> _addr_p, ptr<ast.AssignStmt> _addr_@as) {
    ref parser p = ref _addr_p.val;
    ref ast.AssignStmt @as = ref _addr_@as.val;

    foreach (var (_, x) in @as.Lhs) {
        {
            ptr<ast.Ident> (_, isIdent) = x._<ptr<ast.Ident>>();

            if (!isIdent) {
                p.errorExpected(x.Pos(), "identifier on left side of :=");
            }

        }

    }
}

private static ptr<ast.CallExpr> parseCallExpr(this ptr<parser> _addr_p, @string callType) {
    ref parser p = ref _addr_p.val;

    var x = p.parseRhsOrType(); // could be a conversion: (some type)(x)
    {
        ptr<ast.CallExpr> (call, isCall) = x._<ptr<ast.CallExpr>>();

        if (isCall) {
            return _addr_call!;
        }
    }

    {
        ptr<ast.BadExpr> (_, isBad) = x._<ptr<ast.BadExpr>>();

        if (!isBad) { 
            // only report error if it's a new one
            p.error(p.safePos(x.End()), fmt.Sprintf("function must be invoked in %s statement", callType));

        }
    }

    return _addr_null!;

}

private static ast.Stmt parseGoStmt(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "GoStmt")));
    }
    var pos = p.expect(token.GO);
    var call = p.parseCallExpr("go");
    p.expectSemi();
    if (call == null) {
        return addr(new ast.BadStmt(From:pos,To:pos+2)); // len("go")
    }
    return addr(new ast.GoStmt(Go:pos,Call:call));

});

private static ast.Stmt parseDeferStmt(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "DeferStmt")));
    }
    var pos = p.expect(token.DEFER);
    var call = p.parseCallExpr("defer");
    p.expectSemi();
    if (call == null) {
        return addr(new ast.BadStmt(From:pos,To:pos+5)); // len("defer")
    }
    return addr(new ast.DeferStmt(Defer:pos,Call:call));

});

private static ptr<ast.ReturnStmt> parseReturnStmt(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "ReturnStmt")));
    }
    var pos = p.pos;
    p.expect(token.RETURN);
    slice<ast.Expr> x = default;
    if (p.tok != token.SEMICOLON && p.tok != token.RBRACE) {
        x = p.parseList(true);
    }
    p.expectSemi();

    return addr(new ast.ReturnStmt(Return:pos,Results:x));

});

private static ptr<ast.BranchStmt> parseBranchStmt(this ptr<parser> _addr_p, token.Token tok) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "BranchStmt")));
    }
    var pos = p.expect(tok);
    ptr<ast.Ident> label;
    if (tok != token.FALLTHROUGH && p.tok == token.IDENT) {
        label = p.parseIdent();
    }
    p.expectSemi();

    return addr(new ast.BranchStmt(TokPos:pos,Tok:tok,Label:label));

});

private static ast.Expr makeExpr(this ptr<parser> _addr_p, ast.Stmt s, @string want) {
    ref parser p = ref _addr_p.val;

    if (s == null) {
        return null;
    }
    {
        ptr<ast.ExprStmt> (es, isExpr) = s._<ptr<ast.ExprStmt>>();

        if (isExpr) {
            return p.checkExpr(es.X);
        }
    }

    @string found = "simple statement";
    {
        ptr<ast.AssignStmt> (_, isAss) = s._<ptr<ast.AssignStmt>>();

        if (isAss) {
            found = "assignment";
        }
    }

    p.error(s.Pos(), fmt.Sprintf("expected %s, found %s (missing parentheses around composite literal?)", want, found));
    return addr(new ast.BadExpr(From:s.Pos(),To:p.safePos(s.End())));

}

// parseIfHeader is an adjusted version of parser.header
// in cmd/compile/internal/syntax/parser.go, which has
// been tuned for better error handling.
private static (ast.Stmt, ast.Expr) parseIfHeader(this ptr<parser> _addr_p) {
    ast.Stmt init = default;
    ast.Expr cond = default;
    ref parser p = ref _addr_p.val;

    if (p.tok == token.LBRACE) {
        p.error(p.pos, "missing condition in if statement");
        cond = addr(new ast.BadExpr(From:p.pos,To:p.pos));
        return ;
    }
    var prevLev = p.exprLev;
    p.exprLev = -1;

    if (p.tok != token.SEMICOLON) { 
        // accept potential variable declaration but complain
        if (p.tok == token.VAR) {
            p.next();
            p.error(p.pos, "var declaration not allowed in 'IF' initializer");
        }
        init, _ = p.parseSimpleStmt(basic);

    }
    ast.Stmt condStmt = default;
    var semi = default;
    if (p.tok != token.LBRACE) {
        if (p.tok == token.SEMICOLON) {
            semi.pos = p.pos;
            semi.lit = p.lit;
            p.next();
        }
        else
 {
            p.expect(token.SEMICOLON);
        }
        if (p.tok != token.LBRACE) {
            condStmt, _ = p.parseSimpleStmt(basic);
        }
    }
    else
 {
        condStmt = init;
        init = null;
    }
    if (condStmt != null) {
        cond = p.makeExpr(condStmt, "boolean expression");
    }
    else if (semi.pos.IsValid()) {
        if (semi.lit == "\n") {
            p.error(semi.pos, "unexpected newline, expecting { after if clause");
        }
        else
 {
            p.error(semi.pos, "missing condition in if statement");
        }
    }
    if (cond == null) {
        cond = addr(new ast.BadExpr(From:p.pos,To:p.pos));
    }
    p.exprLev = prevLev;
    return ;

}

private static ptr<ast.IfStmt> parseIfStmt(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "IfStmt")));
    }
    var pos = p.expect(token.IF);

    var (init, cond) = p.parseIfHeader();
    var body = p.parseBlockStmt();

    ast.Stmt else_ = default;
    if (p.tok == token.ELSE) {
        p.next();

        if (p.tok == token.IF) 
            else_ = p.parseIfStmt();
        else if (p.tok == token.LBRACE) 
            else_ = p.parseBlockStmt();
            p.expectSemi();
        else 
            p.errorExpected(p.pos, "if statement or block");
            else_ = addr(new ast.BadStmt(From:p.pos,To:p.pos));
        
    }
    else
 {
        p.expectSemi();
    }
    return addr(new ast.IfStmt(If:pos,Init:init,Cond:cond,Body:body,Else:else_));

});

private static slice<ast.Expr> parseTypeList(this ptr<parser> _addr_p) => func((defer, _, _) => {
    slice<ast.Expr> list = default;
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "TypeList")));
    }
    list = append(list, p.parseType());
    while (p.tok == token.COMMA) {
        p.next();
        list = append(list, p.parseType());
    }

    return ;

});

private static ptr<ast.CaseClause> parseCaseClause(this ptr<parser> _addr_p, bool typeSwitch) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "CaseClause")));
    }
    var pos = p.pos;
    slice<ast.Expr> list = default;
    if (p.tok == token.CASE) {
        p.next();
        if (typeSwitch) {
            list = p.parseTypeList();
        }
        else
 {
            list = p.parseList(true);
        }
    }
    else
 {
        p.expect(token.DEFAULT);
    }
    var colon = p.expect(token.COLON);
    var body = p.parseStmtList();

    return addr(new ast.CaseClause(Case:pos,List:list,Colon:colon,Body:body));

});

private static bool isTypeSwitchAssert(ast.Expr x) {
    ptr<ast.TypeAssertExpr> (a, ok) = x._<ptr<ast.TypeAssertExpr>>();
    return ok && a.Type == null;
}

private static bool isTypeSwitchGuard(this ptr<parser> _addr_p, ast.Stmt s) {
    ref parser p = ref _addr_p.val;

    switch (s.type()) {
        case ptr<ast.ExprStmt> t:
            return isTypeSwitchAssert(t.X);
            break;
        case ptr<ast.AssignStmt> t:
            if (len(t.Lhs) == 1 && len(t.Rhs) == 1 && isTypeSwitchAssert(t.Rhs[0])) {

                if (t.Tok == token.ASSIGN) 
                {
                    // permit v = x.(type) but complain
                    p.error(t.TokPos, "expected ':=', found '='");
                    fallthrough = true;
                }
                if (fallthrough || t.Tok == token.DEFINE)
                {
                    return true;
                    goto __switch_break1;
                }

                __switch_break1:;

            }

            break;
    }
    return false;

}

private static ast.Stmt parseSwitchStmt(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "SwitchStmt")));
    }
    var pos = p.expect(token.SWITCH);

    ast.Stmt s1 = default;    ast.Stmt s2 = default;

    if (p.tok != token.LBRACE) {
        var prevLev = p.exprLev;
        p.exprLev = -1;
        if (p.tok != token.SEMICOLON) {
            s2, _ = p.parseSimpleStmt(basic);
        }
        if (p.tok == token.SEMICOLON) {
            p.next();
            s1 = s2;
            s2 = null;
            if (p.tok != token.LBRACE) { 
                // A TypeSwitchGuard may declare a variable in addition
                // to the variable declared in the initial SimpleStmt.
                // Introduce extra scope to avoid redeclaration errors:
                //
                //    switch t := 0; t := x.(T) { ... }
                //
                // (this code is not valid Go because the first t
                // cannot be accessed and thus is never used, the extra
                // scope is needed for the correct error message).
                //
                // If we don't have a type switch, s2 must be an expression.
                // Having the extra nested but empty scope won't affect it.
                s2, _ = p.parseSimpleStmt(basic);

            }

        }
        p.exprLev = prevLev;

    }
    var typeSwitch = p.isTypeSwitchGuard(s2);
    var lbrace = p.expect(token.LBRACE);
    slice<ast.Stmt> list = default;
    while (p.tok == token.CASE || p.tok == token.DEFAULT) {
        list = append(list, p.parseCaseClause(typeSwitch));
    }
    var rbrace = p.expect(token.RBRACE);
    p.expectSemi();
    ptr<ast.BlockStmt> body = addr(new ast.BlockStmt(Lbrace:lbrace,List:list,Rbrace:rbrace));

    if (typeSwitch) {
        return addr(new ast.TypeSwitchStmt(Switch:pos,Init:s1,Assign:s2,Body:body));
    }
    return addr(new ast.SwitchStmt(Switch:pos,Init:s1,Tag:p.makeExpr(s2,"switch expression"),Body:body));

});

private static ptr<ast.CommClause> parseCommClause(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "CommClause")));
    }
    var pos = p.pos;
    ast.Stmt comm = default;
    if (p.tok == token.CASE) {
        p.next();
        var lhs = p.parseList(false);
        if (p.tok == token.ARROW) { 
            // SendStmt
            if (len(lhs) > 1) {
                p.errorExpected(lhs[0].Pos(), "1 expression"); 
                // continue with first expression
            }

            var arrow = p.pos;
            p.next();
            var rhs = p.parseRhs();
            comm = addr(new ast.SendStmt(Chan:lhs[0],Arrow:arrow,Value:rhs));

        }
        else
 { 
            // RecvStmt
            {
                var tok = p.tok;

                if (tok == token.ASSIGN || tok == token.DEFINE) { 
                    // RecvStmt with assignment
                    if (len(lhs) > 2) {
                        p.errorExpected(lhs[0].Pos(), "1 or 2 expressions"); 
                        // continue with first two expressions
                        lhs = lhs[(int)0..(int)2];

                    }

                    pos = p.pos;
                    p.next();
                    rhs = p.parseRhs();
                    ptr<ast.AssignStmt> @as = addr(new ast.AssignStmt(Lhs:lhs,TokPos:pos,Tok:tok,Rhs:[]ast.Expr{rhs}));
                    if (tok == token.DEFINE) {
                        p.checkAssignStmt(as);
                    }

                    comm = as;

                }
                else
 { 
                    // lhs must be single receive operation
                    if (len(lhs) > 1) {
                        p.errorExpected(lhs[0].Pos(), "1 expression"); 
                        // continue with first expression
                    }

                    comm = addr(new ast.ExprStmt(X:lhs[0]));

                }

            }

        }
    }
    else
 {
        p.expect(token.DEFAULT);
    }
    var colon = p.expect(token.COLON);
    var body = p.parseStmtList();

    return addr(new ast.CommClause(Case:pos,Comm:comm,Colon:colon,Body:body));

});

private static ptr<ast.SelectStmt> parseSelectStmt(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "SelectStmt")));
    }
    var pos = p.expect(token.SELECT);
    var lbrace = p.expect(token.LBRACE);
    slice<ast.Stmt> list = default;
    while (p.tok == token.CASE || p.tok == token.DEFAULT) {
        list = append(list, p.parseCommClause());
    }
    var rbrace = p.expect(token.RBRACE);
    p.expectSemi();
    ptr<ast.BlockStmt> body = addr(new ast.BlockStmt(Lbrace:lbrace,List:list,Rbrace:rbrace));

    return addr(new ast.SelectStmt(Select:pos,Body:body));

});

private static ast.Stmt parseForStmt(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "ForStmt")));
    }
    var pos = p.expect(token.FOR);

    ast.Stmt s1 = default;    ast.Stmt s2 = default;    ast.Stmt s3 = default;

    bool isRange = default;
    if (p.tok != token.LBRACE) {
        var prevLev = p.exprLev;
        p.exprLev = -1;
        if (p.tok != token.SEMICOLON) {
            if (p.tok == token.RANGE) { 
                // "for range x" (nil lhs in assignment)
                pos = p.pos;
                p.next();
                ast.Expr y = new slice<ast.Expr>(new ast.Expr[] { &ast.UnaryExpr{OpPos:pos,Op:token.RANGE,X:p.parseRhs()} });
                s2 = addr(new ast.AssignStmt(Rhs:y));
                isRange = true;

            }
            else
 {
                s2, isRange = p.parseSimpleStmt(rangeOk);
            }

        }
        if (!isRange && p.tok == token.SEMICOLON) {
            p.next();
            s1 = s2;
            s2 = null;
            if (p.tok != token.SEMICOLON) {
                s2, _ = p.parseSimpleStmt(basic);
            }
            p.expectSemi();
            if (p.tok != token.LBRACE) {
                s3, _ = p.parseSimpleStmt(basic);
            }
        }
        p.exprLev = prevLev;

    }
    var body = p.parseBlockStmt();
    p.expectSemi();

    if (isRange) {
        ptr<ast.AssignStmt> @as = s2._<ptr<ast.AssignStmt>>(); 
        // check lhs
        ast.Expr key = default;        ast.Expr value = default;

        switch (len(@as.Lhs)) {
            case 0: 

                break;
            case 1: 
                key = @as.Lhs[0];
                break;
            case 2: 
                (key, value) = (@as.Lhs[0], @as.Lhs[1]);
                break;
            default: 
                p.errorExpected(@as.Lhs[len(@as.Lhs) - 1].Pos(), "at most 2 expressions");
                return addr(new ast.BadStmt(From:pos,To:p.safePos(body.End())));
                break;
        } 
        // parseSimpleStmt returned a right-hand side that
        // is a single unary expression of the form "range x"
        ptr<ast.UnaryExpr> x = @as.Rhs[0]._<ptr<ast.UnaryExpr>>().X;
        return addr(new ast.RangeStmt(For:pos,Key:key,Value:value,TokPos:as.TokPos,Tok:as.Tok,X:x,Body:body,));

    }
    return addr(new ast.ForStmt(For:pos,Init:s1,Cond:p.makeExpr(s2,"boolean or range expression"),Post:s3,Body:body,));

});

private static ast.Stmt parseStmt(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ast.Stmt s = default;
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "Statement")));
    }

    if (p.tok == token.CONST || p.tok == token.TYPE || p.tok == token.VAR) 
        s = addr(new ast.DeclStmt(Decl:p.parseDecl(stmtStart)));
    else if (p.tok == token.IDENT || p.tok == token.INT || p.tok == token.FLOAT || p.tok == token.IMAG || p.tok == token.CHAR || p.tok == token.STRING || p.tok == token.FUNC || p.tok == token.LPAREN || p.tok == token.LBRACK || p.tok == token.STRUCT || p.tok == token.MAP || p.tok == token.CHAN || p.tok == token.INTERFACE || p.tok == token.ADD || p.tok == token.SUB || p.tok == token.MUL || p.tok == token.AND || p.tok == token.XOR || p.tok == token.ARROW || p.tok == token.NOT) // unary operators
        s, _ = p.parseSimpleStmt(labelOk); 
        // because of the required look-ahead, labeled statements are
        // parsed by parseSimpleStmt - don't expect a semicolon after
        // them
        {
            ptr<ast.LabeledStmt> (_, isLabeledStmt) = s._<ptr<ast.LabeledStmt>>();

            if (!isLabeledStmt) {
                p.expectSemi();
            }

        }

    else if (p.tok == token.GO) 
        s = p.parseGoStmt();
    else if (p.tok == token.DEFER) 
        s = p.parseDeferStmt();
    else if (p.tok == token.RETURN) 
        s = p.parseReturnStmt();
    else if (p.tok == token.BREAK || p.tok == token.CONTINUE || p.tok == token.GOTO || p.tok == token.FALLTHROUGH) 
        s = p.parseBranchStmt(p.tok);
    else if (p.tok == token.LBRACE) 
        s = p.parseBlockStmt();
        p.expectSemi();
    else if (p.tok == token.IF) 
        s = p.parseIfStmt();
    else if (p.tok == token.SWITCH) 
        s = p.parseSwitchStmt();
    else if (p.tok == token.SELECT) 
        s = p.parseSelectStmt();
    else if (p.tok == token.FOR) 
        s = p.parseForStmt();
    else if (p.tok == token.SEMICOLON) 
        // Is it ever possible to have an implicit semicolon
        // producing an empty statement in a valid program?
        // (handle correctly anyway)
        s = addr(new ast.EmptyStmt(Semicolon:p.pos,Implicit:p.lit=="\n"));
        p.next();
    else if (p.tok == token.RBRACE) 
        // a semicolon may be omitted before a closing "}"
        s = addr(new ast.EmptyStmt(Semicolon:p.pos,Implicit:true));
    else 
        // no statement found
        var pos = p.pos;
        p.errorExpected(pos, "statement");
        p.advance(stmtStart);
        s = addr(new ast.BadStmt(From:pos,To:p.pos));
        return ;

});

// ----------------------------------------------------------------------------
// Declarations

public delegate  ast.Spec parseSpecFunction(ptr<ast.CommentGroup>,  token.Pos,  token.Token,  nint);

private static bool isValidImport(@string lit) {
    const @string illegalChars = "!\"#$%&\'()*,:;<=>?[\\]^{|}" + "`\uFFFD";

    var (s, _) = strconv.Unquote(lit); // go/scanner returns a legal string literal
    foreach (var (_, r) in s) {
        if (!unicode.IsGraphic(r) || unicode.IsSpace(r) || strings.ContainsRune(illegalChars, r)) {
            return false;
        }
    }    return s != "";

}

private static ast.Spec parseImportSpec(this ptr<parser> _addr_p, ptr<ast.CommentGroup> _addr_doc, token.Pos _, token.Token _, nint _) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;
    ref ast.CommentGroup doc = ref _addr_doc.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "ImportSpec")));
    }
    ptr<ast.Ident> ident;

    if (p.tok == token.PERIOD) 
        ident = addr(new ast.Ident(NamePos:p.pos,Name:"."));
        p.next();
    else if (p.tok == token.IDENT) 
        ident = p.parseIdent();
        var pos = p.pos;
    @string path = default;
    if (p.tok == token.STRING) {
        path = p.lit;
        if (!isValidImport(path)) {
            p.error(pos, "invalid import path: " + path);
        }
        p.next();

    }
    else
 {
        p.expect(token.STRING); // use expect() error handling
    }
    p.expectSemi(); // call before accessing p.linecomment

    // collect imports
    ptr<ast.ImportSpec> spec = addr(new ast.ImportSpec(Doc:doc,Name:ident,Path:&ast.BasicLit{ValuePos:pos,Kind:token.STRING,Value:path},Comment:p.lineComment,));
    p.imports = append(p.imports, spec);

    return spec;

});

private static ast.Spec parseValueSpec(this ptr<parser> _addr_p, ptr<ast.CommentGroup> _addr_doc, token.Pos _, token.Token keyword, nint iota) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;
    ref ast.CommentGroup doc = ref _addr_doc.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, keyword.String() + "Spec")));
    }
    var pos = p.pos;
    var idents = p.parseIdentList();
    var typ = p.tryIdentOrType();
    slice<ast.Expr> values = default; 
    // always permit optional initialization for more tolerant parsing
    if (p.tok == token.ASSIGN) {
        p.next();
        values = p.parseList(true);
    }
    p.expectSemi(); // call before accessing p.linecomment


    if (keyword == token.VAR) 
        if (typ == null && values == null) {
            p.error(pos, "missing variable type or initialization");
        }
    else if (keyword == token.CONST) 
        if (values == null && (iota == 0 || typ != null)) {
            p.error(pos, "missing constant value");
        }
        ptr<ast.ValueSpec> spec = addr(new ast.ValueSpec(Doc:doc,Names:idents,Type:typ,Values:values,Comment:p.lineComment,));
    return spec;

});

private static void parseGenericType(this ptr<parser> _addr_p, ptr<ast.TypeSpec> _addr_spec, token.Pos openPos, ptr<ast.Ident> _addr_name0, token.Token closeTok) {
    ref parser p = ref _addr_p.val;
    ref ast.TypeSpec spec = ref _addr_spec.val;
    ref ast.Ident name0 = ref _addr_name0.val;

    var list = p.parseParameterList(name0, closeTok, p.parseParamDecl, true);
    var closePos = p.expect(closeTok);
    typeparams.Set(spec, addr(new ast.FieldList(Opening:openPos,List:list,Closing:closePos))); 
    // Type alias cannot have type parameters. Accept them for robustness but complain.
    if (p.tok == token.ASSIGN) {
        p.error(p.pos, "generic type cannot be alias");
        p.next();
    }
    spec.Type = p.parseType();

}

private static ast.Spec parseTypeSpec(this ptr<parser> _addr_p, ptr<ast.CommentGroup> _addr_doc, token.Pos _, token.Token _, nint _) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;
    ref ast.CommentGroup doc = ref _addr_doc.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "TypeSpec")));
    }
    var ident = p.parseIdent();
    ptr<ast.TypeSpec> spec = addr(new ast.TypeSpec(Doc:doc,Name:ident));


    if (p.tok == token.LBRACK) 
        var lbrack = p.pos;
        p.next();
        if (p.tok == token.IDENT) { 
            // array type or generic type [T any]
            p.exprLev++;
            var x = p.parseExpr();
            p.exprLev--;
            {
                ptr<ast.Ident> (name0, _) = x._<ptr<ast.Ident>>();

                if (p.parseTypeParams() && name0 != null && p.tok != token.RBRACK) { 
                    // generic type [T any];
                    p.parseGenericType(spec, lbrack, name0, token.RBRACK);

                }
                else
 { 
                    // array type
                    // TODO(rfindley) should resolve all identifiers in x.
                    p.expect(token.RBRACK);
                    var elt = p.parseType();
                    spec.Type = addr(new ast.ArrayType(Lbrack:lbrack,Len:x,Elt:elt));

                }

            }

        }
        else
 { 
            // array type
            var alen = p.parseArrayLen();
            p.expect(token.RBRACK);
            elt = p.parseType();
            spec.Type = addr(new ast.ArrayType(Lbrack:lbrack,Len:alen,Elt:elt));

        }
    else 
        // no type parameters
        if (p.tok == token.ASSIGN) { 
            // type alias
            spec.Assign = p.pos;
            p.next();

        }
        spec.Type = p.parseType();
        p.expectSemi(); // call before accessing p.linecomment
    spec.Comment = p.lineComment;

    return spec;

});

private static ptr<ast.GenDecl> parseGenDecl(this ptr<parser> _addr_p, token.Token keyword, parseSpecFunction f) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "GenDecl(" + keyword.String() + ")")));
    }
    var doc = p.leadComment;
    var pos = p.expect(keyword);
    token.Pos lparen = default;    token.Pos rparen = default;

    slice<ast.Spec> list = default;
    if (p.tok == token.LPAREN) {
        lparen = p.pos;
        p.next();
        for (nint iota = 0; p.tok != token.RPAREN && p.tok != token.EOF; iota++) {
            list = append(list, f(p.leadComment, pos, keyword, iota));
        }
    else

        rparen = p.expect(token.RPAREN);
        p.expectSemi();

    } {
        list = append(list, f(null, pos, keyword, 0));
    }
    return addr(new ast.GenDecl(Doc:doc,TokPos:pos,Tok:keyword,Lparen:lparen,Specs:list,Rparen:rparen,));

});

private static ptr<ast.FuncDecl> parseFuncDecl(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "FunctionDecl")));
    }
    var doc = p.leadComment;
    var pos = p.expect(token.FUNC);

    ptr<ast.FieldList> recv;
    if (p.tok == token.LPAREN) {
        _, recv = p.parseParameters(false);
    }
    var ident = p.parseIdent();

    var (tparams, params) = p.parseParameters(true);
    var results = p.parseResult();

    ptr<ast.BlockStmt> body;
    if (p.tok == token.LBRACE) {
        body = p.parseBody();
        p.expectSemi();
    }
    else if (p.tok == token.SEMICOLON) {
        p.next();
        if (p.tok == token.LBRACE) { 
            // opening { of function declaration on next line
            p.error(p.pos, "unexpected semicolon or newline before {");
            body = p.parseBody();
            p.expectSemi();

        }
    }
    else
 {
        p.expectSemi();
    }
    ptr<ast.FuncDecl> decl = addr(new ast.FuncDecl(Doc:doc,Recv:recv,Name:ident,Type:&ast.FuncType{Func:pos,Params:params,Results:results,},Body:body,));
    typeparams.Set(decl.Type, tparams);
    return _addr_decl!;

});

private static ast.Decl parseDecl(this ptr<parser> _addr_p, map<token.Token, bool> sync) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "Declaration")));
    }
    parseSpecFunction f = default;

    if (p.tok == token.CONST || p.tok == token.VAR) 
        f = p.parseValueSpec;
    else if (p.tok == token.TYPE) 
        f = p.parseTypeSpec;
    else if (p.tok == token.FUNC) 
        return p.parseFuncDecl();
    else 
        var pos = p.pos;
        p.errorExpected(pos, "declaration");
        p.advance(sync);
        return addr(new ast.BadDecl(From:pos,To:p.pos));
        return p.parseGenDecl(p.tok, f);

});

// ----------------------------------------------------------------------------
// Source files

private static ptr<ast.File> parseFile(this ptr<parser> _addr_p) => func((defer, _, _) => {
    ref parser p = ref _addr_p.val;

    if (p.trace) {
        defer(un(_addr_trace(_addr_p, "File")));
    }
    if (p.errors.Len() != 0) {
        return _addr_null!;
    }
    var doc = p.leadComment;
    var pos = p.expect(token.PACKAGE); 
    // Go spec: The package clause is not a declaration;
    // the package name does not appear in any scope.
    var ident = p.parseIdent();
    if (ident.Name == "_" && p.mode & DeclarationErrors != 0) {
        p.error(p.pos, "invalid package name _");
    }
    p.expectSemi(); 

    // Don't bother parsing the rest if we had errors parsing the package clause.
    // Likely not a Go source file at all.
    if (p.errors.Len() != 0) {
        return _addr_null!;
    }
    slice<ast.Decl> decls = default;
    if (p.mode & PackageClauseOnly == 0) { 
        // import decls
        while (p.tok == token.IMPORT) {
            decls = append(decls, p.parseGenDecl(token.IMPORT, p.parseImportSpec));
        }

        if (p.mode & ImportsOnly == 0) { 
            // rest of package body
            while (p.tok != token.EOF) {
                decls = append(decls, p.parseDecl(declStart));
            }


        }
    }
    ptr<ast.File> f = addr(new ast.File(Doc:doc,Package:pos,Name:ident,Decls:decls,Imports:p.imports,Comments:p.comments,));
    Action<token.Pos, @string> declErr = default;
    if (p.mode & DeclarationErrors != 0) {
        declErr = p.error;
    }
    if (p.mode & SkipObjectResolution == 0) {
        resolveFile(f, p.file, declErr);
    }
    return _addr_f!;

});

} // end parser_package
