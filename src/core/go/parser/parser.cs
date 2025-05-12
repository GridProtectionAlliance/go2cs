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
namespace go.go;

using fmt = fmt_package;
using ast = go.ast_package;
using constraint = go.build.constraint_package;
using typeparams = go.@internal.typeparams_package;
using scanner = go.scanner_package;
using token = go.token_package;
using strings = strings_package;
using go.@internal;
using go.build;
using ꓸꓸꓸany = Span<any>;

partial class parser_package {

// The parser structure holds the parser's internal state.
[GoType] partial struct parser {
    internal ж<go.token_package.ΔFile> file;
    internal go.scanner_package.ErrorList errors;
    internal go.scanner_package.Scanner scanner;
    // Tracing/debugging
    internal Mode mode; // parsing mode
    internal bool trace; // == (mode&Trace != 0)
    internal nint indent; // indentation used for tracing output
    // Comments
    internal ast.CommentGroup comments;
    internal ж<go.ast_package.CommentGroup> leadComment; // last lead comment
    internal ж<go.ast_package.CommentGroup> lineComment; // last line comment
    internal bool top;              // in top of file (before package clause)
    internal @string goVersion;           // minimum Go version found in //go:build comment
    // Next token
    internal go.token_package.ΔPos pos; // token position
    internal go.token_package.Token tok; // one token look-ahead
    internal @string lit;     // token literal
    // Error recovery
    // (used to limit the number of calls to parser.advance
    // w/o making scanning progress - avoids potential endless
    // loops across multiple parser functions during error recovery)
    internal go.token_package.ΔPos syncPos; // last synchronization position
    internal nint syncCnt;      // number of parser.advance calls without progress
    // Non-syntactic parser control
    internal nint exprLev; // < 0: in control clause, >= 0: in expression
    internal bool inRhs; // if set, the parser is parsing a rhs expression
    internal ast.ImportSpec imports; // list of imports
    // nestLev is used to track and limit the recursion depth
    // during parsing.
    internal nint nestLev;
}

[GoRecv] internal static void init(this ref parser p, ж<token.FileSet> Ꮡfset, @string filename, slice<byte> src, Mode mode) {
    ref var fset = ref Ꮡfset.val;

    p.file = fset.AddFile(filename, -1, len(src));
    var eh = (tokenꓸPosition pos, @string msg) => {
        p.errors.Add(pos, msg);
    };
    p.scanner.Init(p.file, src, eh, scanner.ScanComments);
    p.top = true;
    p.mode = mode;
    p.trace = (Mode)(mode & Trace) != 0;
    // for convenience (p.trace is used frequently)
    p.next();
}

// ----------------------------------------------------------------------------
// Parsing support
[GoRecv] internal static void printTrace(this ref parser p, params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();

    @string dots = ". . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . "u8;
    const nint n = /* len(dots) */ 64;
    var pos = p.file.Position(p.pos);
    fmt.Printf("%5d:%3d: "u8, pos.Line, pos.Column);
    nint i = 2 * p.indent;
    while (i > n) {
        fmt.Print(dots);
        i -= n;
    }
    // i <= n
    fmt.Print(dots[0..(int)(i)]);
    fmt.Println(a.ꓸꓸꓸ);
}

internal static ж<parser> trace(ж<parser> Ꮡp, @string msg) {
    ref var p = ref Ꮡp.val;

    p.printTrace(msg, "(");
    p.indent++;
    return Ꮡp;
}

// Usage pattern: defer un(trace(p, "..."))
internal static void un(ж<parser> Ꮡp) {
    ref var p = ref Ꮡp.val;

    p.indent--;
    p.printTrace(")");
}

// maxNestLev is the deepest we're willing to recurse during parsing
internal const nint maxNestLev = 100000;

internal static ж<parser> incNestLev(ж<parser> Ꮡp) {
    ref var p = ref Ꮡp.val;

    p.nestLev++;
    if (p.nestLev > maxNestLev) {
        p.error(p.pos, "exceeded max nesting depth"u8);
        throw panic(new bailout(nil));
    }
    return Ꮡp;
}

// decNestLev is used to track nesting depth during parsing to prevent stack exhaustion.
// It is used along with incNestLev in a similar fashion to how un and trace are used.
internal static void decNestLev(ж<parser> Ꮡp) {
    ref var p = ref Ꮡp.val;

    p.nestLev--;
}

// Advance to the next token.
[GoRecv] internal static void next0(this ref parser p) {
    // Because of one-token look-ahead, print the previous token
    // when tracing as it provides a more readable output. The
    // very first token (!p.pos.IsValid()) is not initialized
    // (it is token.ILLEGAL), so don't print it.
    if (p.trace && p.pos.IsValid()) {
        @string s = p.tok.String();
        switch (ᐧ) {
        case {} when p.tok.IsLiteral(): {
            p.printTrace(s, p.lit);
            break;
        }
        case {} when (p.tok.IsOperator()) || (p.tok.IsKeyword()): {
            p.printTrace("\""u8 + s + "\""u8);
            break;
        }
        default: {
            p.printTrace(s);
            break;
        }}

    }
    while (ᐧ) {
        (p.pos, p.tok, p.lit) = p.scanner.Scan();
        if (p.tok == token.COMMENT){
            if (p.top && strings.HasPrefix(p.lit, "//go:build"u8)) {
                {
                    (x, err) = constraint.Parse(p.lit); if (err == default!) {
                        p.goVersion = constraint.GoVersion(x);
                    }
                }
            }
            if ((Mode)(p.mode & ParseComments) == 0) {
                continue;
            }
        } else {
            // Found a non-comment; top of file is over.
            p.top = false;
        }
        break;
    }
}

// Consume a comment and return it and the line on which it ends.
[GoRecv] internal static (ж<ast.Comment> comment, nint endline) consumeComment(this ref parser p) {
    ж<ast.Comment> comment = default!;
    nint endline = default!;

    // /*-style comments may end on a different line than where they start.
    // Scan the comment for '\n' chars and adjust endline accordingly.
    endline = p.file.Line(p.pos);
    if (p.lit[1] == (rune)'*') {
        // don't use range here - no need to decode Unicode code points
        for (nint i = 0; i < len(p.lit); i++) {
            if (p.lit[i] == (rune)'\n') {
                endline++;
            }
        }
    }
    comment = Ꮡ(new ast.Comment(Slash: p.pos, Text: p.lit));
    p.next0();
    return (comment, endline);
}

// Consume a group of adjacent comments, add it to the parser's
// comments list, and return it together with the line at which
// the last comment in the group ends. A non-comment token or n
// empty lines terminate a comment group.
[GoRecv] internal static (ж<ast.CommentGroup> comments, nint endline) consumeCommentGroup(this ref parser p, nint n) {
    ж<ast.CommentGroup> comments = default!;
    nint endline = default!;

    slice<ast.Comment> list = default!;
    endline = p.file.Line(p.pos);
    while (p.tok == token.COMMENT && p.file.Line(p.pos) <= endline + n) {
        ж<ast.Comment> comment = default!;
        (comment, endline) = p.consumeComment();
        list = append(list, comment);
    }
    // add comment group to the comments list
    comments = Ꮡ(new ast.CommentGroup(List: list));
    p.comments = append(p.comments, comments);
    return (comments, endline);
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
[GoRecv] internal static void next(this ref parser p) {
    p.leadComment = default!;
    p.lineComment = default!;
    tokenꓸPos prev = p.pos;
    p.next0();
    if (p.tok == token.COMMENT) {
        ж<ast.CommentGroup> comment = default!;
        nint endline = default!;
        if (p.file.Line(p.pos) == p.file.Line(prev)) {
            // The comment is on same line as the previous token; it
            // cannot be a lead comment but may be a line comment.
            (comment, endline) = p.consumeCommentGroup(0);
            if (p.file.Line(p.pos) != endline || p.tok == token.SEMICOLON || p.tok == token.EOF) {
                // The next token is on a different line, thus
                // the last comment group is a line comment.
                p.lineComment = comment;
            }
        }
        // consume successor comments, if any
        endline = -1;
        while (p.tok == token.COMMENT) {
            (comment, endline) = p.consumeCommentGroup(1);
        }
        if (endline + 1 == p.file.Line(p.pos)) {
            // The next token is following on the line immediately after the
            // comment group, thus the last comment group is a lead comment.
            p.leadComment = comment;
        }
    }
}

// A bailout panic is raised to indicate early termination. pos and msg are
// only populated when bailing out of object resolution.
[GoType] partial struct bailout {
    internal go.token_package.ΔPos pos;
    internal @string msg;
}

[GoRecv] internal static void error(this ref parser p, tokenꓸPos pos, @string msg) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "error: "u8 + msg), defer);
    }
    var epos = p.file.Position(pos);
    // If AllErrors is not set, discard errors reported on the same line
    // as the last recorded error and stop parsing if there are more than
    // 10 errors.
    if ((Mode)(p.mode & AllErrors) == 0) {
        nint n = len(p.errors);
        if (n > 0 && p.errors[n - 1].Pos.Line == epos.Line) {
            return;
        }
        // discard - likely a spurious error
        if (n > 10) {
            throw panic(new bailout(nil));
        }
    }
    p.errors.Add(epos, msg);
});

[GoRecv] internal static void errorExpected(this ref parser p, tokenꓸPos pos, @string msg) {
    msg = "expected "u8 + msg;
    if (pos == p.pos) {
        // the error happened at the current position;
        // make the error message more specific
        switch (ᐧ) {
        case {} when p.tok == token.SEMICOLON && p.lit == "\n"u8: {
            msg += ", found newline"u8;
            break;
        }
        case {} when p.tok.IsLiteral(): {
            msg += ", found "u8 + p.lit;
            break;
        }
        default: {
            msg += ", found '"u8 + p.tok.String() + "'"u8;
            break;
        }}

    }
    // print 123 rather than 'INT', etc.
    p.error(pos, msg);
}

[GoRecv] internal static tokenꓸPos expect(this ref parser p, token.Token tok) {
    tokenꓸPos pos = p.pos;
    if (p.tok != tok) {
        p.errorExpected(pos, "'"u8 + tok.String() + "'"u8);
    }
    p.next();
    // make progress
    return pos;
}

// expect2 is like expect, but it returns an invalid position
// if the expected token is not found.
[GoRecv] internal static tokenꓸPos /*pos*/ expect2(this ref parser p, token.Token tok) {
    tokenꓸPos pos = default!;

    if (p.tok == tok){
        pos = p.pos;
    } else {
        p.errorExpected(p.pos, "'"u8 + tok.String() + "'"u8);
    }
    p.next();
    // make progress
    return pos;
}

// expectClosing is like expect but provides a better error message
// for the common case of a missing comma before a newline.
[GoRecv] internal static tokenꓸPos expectClosing(this ref parser p, token.Token tok, @string context) {
    if (p.tok != tok && p.tok == token.SEMICOLON && p.lit == "\n"u8) {
        p.error(p.pos, "missing ',' before newline in "u8 + context);
        p.next();
    }
    return p.expect(tok);
}

// expectSemi consumes a semicolon and returns the applicable line comment.
[GoRecv] internal static ж<ast.CommentGroup> /*comment*/ expectSemi(this ref parser p) {
    ж<ast.CommentGroup> comment = default!;

    // semicolon is optional before a closing ')' or '}'
    if (p.tok != token.RPAREN && p.tok != token.RBRACE) {
        var exprᴛ1 = p.tok;
        var matchᴛ1 = false;
        if (exprᴛ1 == token.COMMA) { matchᴛ1 = true;
            p.errorExpected(p.pos, // permit a ',' instead of a ';' but complain
 "';'"u8);
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == token.SEMICOLON)) {
            if (p.lit == ";"u8){
                // explicit semicolon
                p.next();
                comment = p.lineComment;
            } else {
                // use following comments
                // artificial semicolon
                comment = p.lineComment;
                // use preceding comments
                p.next();
            }
            return comment;
        }
        { /* default: */
            p.errorExpected(p.pos, "';'"u8);
            p.advance(stmtStart);
        }

    }
    return default!;
}

[GoRecv] internal static bool atComma(this ref parser p, @string context, token.Token follow) {
    if (p.tok == token.COMMA) {
        return true;
    }
    if (p.tok != follow) {
        @string msg = "missing ','"u8;
        if (p.tok == token.SEMICOLON && p.lit == "\n"u8) {
            msg += " before newline"u8;
        }
        p.error(p.pos, msg + " in "u8 + context);
        return true;
    }
    // "insert" comma and continue
    return false;
}

internal static void assert(bool cond, @string msg) {
    if (!cond) {
        throw panic("go/parser internal error: "u8 + msg);
    }
}

// advance consumes tokens until the current token p.tok
// is in the 'to' set, or token.EOF. For error recovery.
[GoRecv] internal static void advance(this ref parser p, token.Token>bool to) {
    for (; p.tok != token.EOF; 
    p.next();) {
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
                return;
            }
            if (p.pos > p.syncPos) {
                p.syncPos = p.pos;
                p.syncCnt = 0;
                return;
            }
        }
    }
}

// Reaching here indicates a parser bug, likely an
// incorrect token list in this function, but it only
// leads to skipping of possibly correct code if a
// previous error is present, and thus is preferred
// over a non-terminating parse.
internal static token.Token>bool stmtStart = new map<token.Token, bool>{
    [token.BREAK] = true,
    [token.CONST] = true,
    [token.CONTINUE] = true,
    [token.DEFER] = true,
    [token.FALLTHROUGH] = true,
    [token.FOR] = true,
    [token.GO] = true,
    [token.GOTO] = true,
    [token.IF] = true,
    [token.RETURN] = true,
    [token.SELECT] = true,
    [token.SWITCH] = true,
    [token.TYPE] = true,
    [token.VAR] = true
};

internal static token.Token>bool declStart = new map<token.Token, bool>{
    [token.IMPORT] = true,
    [token.CONST] = true,
    [token.TYPE] = true,
    [token.VAR] = true
};

internal static token.Token>bool exprEnd = new map<token.Token, bool>{
    [token.COMMA] = true,
    [token.COLON] = true,
    [token.SEMICOLON] = true,
    [token.RPAREN] = true,
    [token.RBRACK] = true,
    [token.RBRACE] = true
};

// safePos returns a valid file position for a given position: If pos
// is valid to begin with, safePos returns pos. If pos is out-of-range,
// safePos returns the EOF position.
//
// This is hack to work around "artificial" end positions in the AST which
// are computed by adding 1 to (presumably valid) token positions. If the
// token positions are invalid due to parse errors, the resulting end position
// may be past the file's EOF position, which would lead to panics if used
// later on.
[GoRecv] internal static tokenꓸPos /*res*/ safePos(this ref parser p, tokenꓸPos pos) => func((defer, recover) => {
    tokenꓸPos res = default!;

    defer(() => {
        if (recover() != default!) {
            res = ((tokenꓸPos)(p.file.Base() + p.file.Size()));
        }
    });
    // EOF position
    _ = p.file.Offset(pos);
    // trigger a panic if position is out-of-range
    return pos;
});

// ----------------------------------------------------------------------------
// Identifiers
[GoRecv] internal static ж<ast.Ident> parseIdent(this ref parser p) {
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.pos;
    @string name = "_"u8;
    if (p.tok == token.IDENT){
        name = p.lit;
        p.next();
    } else {
        p.expect(token.IDENT);
    }
    // use expect() error handling
    return Ꮡ(new ast.Ident(NamePos: pos, Name: name));
}

[GoRecv] internal static slice<ast.Ident> /*list*/ parseIdentList(this ref parser p) => func((defer, _) => {
    slice<ast.Ident> list = default!;

    if (p.trace) {
        deferǃ(un, trace(p, "IdentList"u8), defer);
    }
    list = append(list, p.parseIdent());
    while (p.tok == token.COMMA) {
        p.next();
        list = append(list, p.parseIdent());
    }
    return list;
});

// ----------------------------------------------------------------------------
// Common productions

// If lhs is set, result list elements which are identifiers are not resolved.
[GoRecv] internal static slice<ast.Expr> /*list*/ parseExprList(this ref parser p) => func((defer, _) => {
    slice<ast.Expr> list = default!;

    if (p.trace) {
        deferǃ(un, trace(p, "ExpressionList"u8), defer);
    }
    list = append(list, p.parseExpr());
    while (p.tok == token.COMMA) {
        p.next();
        list = append(list, p.parseExpr());
    }
    return list;
});

[GoRecv] internal static slice<ast.Expr> parseList(this ref parser p, bool inRhs) {
    var old = p.inRhs;
    p.inRhs = inRhs;
    var list = p.parseExprList();
    p.inRhs = old;
    return list;
}

// ----------------------------------------------------------------------------
// Types
[GoRecv] internal static ast.Expr parseType(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "Type"u8), defer);
    }
    var typ = p.tryIdentOrType();
    if (typ == default!) {
        ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
        pos = p.pos;
        p.errorExpected(pos, "type"u8);
        p.advance(exprEnd);
        return new ast.BadExpr(From: pos, To: p.pos);
    }
    return typ;
});

[GoRecv] internal static ast.Expr parseQualifiedIdent(this ref parser p, ж<ast.Ident> Ꮡident) => func((defer, _) => {
    ref var ident = ref Ꮡident.val;

    if (p.trace) {
        deferǃ(un, trace(p, "QualifiedIdent"u8), defer);
    }
    var typ = p.parseTypeName(Ꮡident);
    if (p.tok == token.LBRACK) {
        typ = p.parseTypeInstance(typ);
    }
    return typ;
});

// If the result is an identifier, it is not resolved.
[GoRecv] internal static ast.Expr parseTypeName(this ref parser p, ж<ast.Ident> Ꮡident) => func((defer, _) => {
    ref var ident = ref Ꮡident.val;

    if (p.trace) {
        deferǃ(un, trace(p, "TypeName"u8), defer);
    }
    if (ident == nil) {
        ident = p.parseIdent();
    }
    if (p.tok == token.PERIOD) {
        // ident is a package name
        p.next();
        var sel = p.parseIdent();
        return new ast.SelectorExpr(X: ident, Sel: sel);
    }
    return ~ident;
});

// "[" has already been consumed, and lbrack is its position.
// If len != nil it is the already consumed array length.
[GoRecv] internal static ж<ast.ArrayType> parseArrayType(this ref parser p, tokenꓸPos lbrack, ast.Expr len) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "ArrayType"u8), defer);
    }
    if (len == default!) {
        p.exprLev++;
        // always permit ellipsis for more fault-tolerant parsing
        if (p.tok == token.ELLIPSIS){
            Ꮡlen = new ast.Ellipsis(Ellipsis: p.pos); len = ref Ꮡlen.val;
            p.next();
        } else 
        if (p.tok != token.RBRACK) {
            len = p.parseRhs();
        }
        p.exprLev--;
    }
    if (p.tok == token.COMMA) {
        // Trailing commas are accepted in type parameter
        // lists but not in array type declarations.
        // Accept for better error handling but complain.
        p.error(p.pos, "unexpected comma; expecting ]"u8);
        p.next();
    }
    p.expect(token.RBRACK);
    var elt = p.parseType();
    return Ꮡ(new ast.ArrayType(Lbrack: lbrack, Len: len, Elt: elt));
});

[GoRecv] internal static (ж<ast.Ident>, ast.Expr) parseArrayFieldOrTypeInstance(this ref parser p, ж<ast.Ident> Ꮡx) => func((defer, _) => {
    ref var x = ref Ꮡx.val;

    if (p.trace) {
        deferǃ(un, trace(p, "ArrayFieldOrTypeInstance"u8), defer);
    }
    ref var lbrack = ref heap<go.token_package.ΔPos>(out var Ꮡlbrack);
    lbrack = p.expect(token.LBRACK);
    tokenꓸPos trailingComma = token.NoPos;
    // if valid, the position of a trailing comma preceding the ']'
    slice<ast.Expr> args = default!;
    if (p.tok != token.RBRACK) {
        p.exprLev++;
        args = append(args, p.parseRhs());
        while (p.tok == token.COMMA) {
            tokenꓸPos comma = p.pos;
            p.next();
            if (p.tok == token.RBRACK) {
                trailingComma = comma;
                break;
            }
            args = append(args, p.parseRhs());
        }
        p.exprLev--;
    }
    tokenꓸPos rbrack = p.expect(token.RBRACK);
    if (len(args) == 0) {
        // x []E
        var elt = p.parseType();
        return (Ꮡx, new ast.ArrayType(Lbrack: lbrack, Elt: elt));
    }
    // x [P]E or x[P]
    if (len(args) == 1) {
        var elt = p.tryIdentOrType();
        if (elt != default!) {
            // x [P]E
            if (trailingComma.IsValid()) {
                // Trailing commas are invalid in array type fields.
                p.error(trailingComma, "unexpected comma; expecting ]"u8);
            }
            return (Ꮡx, new ast.ArrayType(Lbrack: lbrack, Len: args[0], Elt: elt));
        }
    }
    // x[P], x[P1, P2], ...
    return (default!, typeparams.PackIndexExpr(~x, lbrack, args, rbrack));
});

[GoRecv] internal static ж<ast.Field> parseFieldDecl(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "FieldDecl"u8), defer);
    }
    var doc = p.leadComment;
    slice<ast.Ident> names = default!;
    ast.Expr typ = default!;
    var exprᴛ1 = p.tok;
    if (exprᴛ1 == token.IDENT) {
        var name = p.parseIdent();
        if (p.tok == token.PERIOD || p.tok == token.STRING || p.tok == token.SEMICOLON || p.tok == token.RBRACE){
            // embedded type
            typ = ~name;
            if (p.tok == token.PERIOD) {
                typ = p.parseQualifiedIdent(name);
            }
        } else {
            // name1, name2, ... T
            names = new ast.Ident[]{name}.slice();
            while (p.tok == token.COMMA) {
                p.next();
                names = append(names, p.parseIdent());
            }
            // Careful dance: We don't know if we have an embedded instantiated
            // type T[P1, P2, ...] or a field T of array type []E or [P]E.
            if (len(names) == 1 && p.tok == token.LBRACK){
                (name, typ) = p.parseArrayFieldOrTypeInstance(name);
                if (name == nil) {
                    names = default!;
                }
            } else {
                // T P
                typ = p.parseType();
            }
        }
    }
    else if (exprᴛ1 == token.MUL) {
        ref var star = ref heap<go.token_package.ΔPos>(out var Ꮡstar);
        star = p.pos;
        p.next();
        if (p.tok == token.LPAREN){
            // *(T)
            p.error(p.pos, "cannot parenthesize embedded type"u8);
            p.next();
            typ = p.parseQualifiedIdent(nil);
            // expect closing ')' but no need to complain if missing
            if (p.tok == token.RPAREN) {
                p.next();
            }
        } else {
            // *T
            typ = p.parseQualifiedIdent(nil);
        }
        Ꮡtyp = new ast.StarExpr(Star: star, X: typ); typ = ref Ꮡtyp.val;
    }
    else if (exprᴛ1 == token.LPAREN) {
        p.error(p.pos, "cannot parenthesize embedded type"u8);
        p.next();
        if (p.tok == token.MUL){
            // (*T)
            ref var star = ref heap<go.token_package.ΔPos>(out var Ꮡstar);
            star = p.pos;
            p.next();
            Ꮡtyp = new ast.StarExpr(Star: star, X: p.parseQualifiedIdent(nil)); typ = ref Ꮡtyp.val;
        } else {
            // (T)
            typ = p.parseQualifiedIdent(nil);
        }
        if (p.tok == token.RPAREN) {
            // expect closing ')' but no need to complain if missing
            p.next();
        }
    }
    else { /* default: */
        ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
        pos = p.pos;
        p.errorExpected(pos, "field name or embedded type"u8);
        p.advance(exprEnd);
        Ꮡtyp = new ast.BadExpr(From: pos, To: p.pos); typ = ref Ꮡtyp.val;
    }

    ж<ast.BasicLit> tag = default!;
    if (p.tok == token.STRING) {
        tag = Ꮡ(new ast.BasicLit(ValuePos: p.pos, Kind: p.tok, Value: p.lit));
        p.next();
    }
    var comment = p.expectSemi();
    var field = Ꮡ(new ast.Field(Doc: doc, Names: names, Type: typ, Tag: tag, Comment: comment));
    return field;
});

[GoRecv] internal static ж<ast.StructType> parseStructType(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "StructType"u8), defer);
    }
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.expect(token.STRUCT);
    ref var lbrace = ref heap<go.token_package.ΔPos>(out var Ꮡlbrace);
    lbrace = p.expect(token.LBRACE);
    slice<ast.Field> list = default!;
    while (p.tok == token.IDENT || p.tok == token.MUL || p.tok == token.LPAREN) {
        // a field declaration cannot start with a '(' but we accept
        // it here for more robust parsing and better error messages
        // (parseFieldDecl will check and complain if necessary)
        list = append(list, p.parseFieldDecl());
    }
    ref var rbrace = ref heap<go.token_package.ΔPos>(out var Ꮡrbrace);
    rbrace = p.expect(token.RBRACE);
    return Ꮡ(new ast.StructType(
        Struct: pos,
        Fields: Ꮡ(new ast.FieldList(
            Opening: lbrace,
            List: list,
            Closing: rbrace
        ))
    ));
});

[GoRecv] internal static ж<ast.StarExpr> parsePointerType(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "PointerType"u8), defer);
    }
    ref var star = ref heap<go.token_package.ΔPos>(out var Ꮡstar);
    star = p.expect(token.MUL);
    var @base = p.parseType();
    return Ꮡ(new ast.StarExpr(Star: star, X: @base));
});

[GoRecv] internal static ж<ast.Ellipsis> parseDotsType(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "DotsType"u8), defer);
    }
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.expect(token.ELLIPSIS);
    var elt = p.parseType();
    return Ꮡ(new ast.Ellipsis(Ellipsis: pos, Elt: elt));
});

[GoType] partial struct field {
    internal ж<go.ast_package.Ident> name;
    internal go.ast_package.Expr typ;
}

[GoRecv] internal static field /*f*/ parseParamDecl(this ref parser p, ж<ast.Ident> Ꮡname, bool typeSetsOK) => func((defer, _) => {
    field f = default!;

    ref var name = ref Ꮡname.val;
    // TODO(rFindley) refactor to be more similar to paramDeclOrNil in the syntax
    // package
    if (p.trace) {
        deferǃ(un, trace(p, "ParamDeclOrNil"u8), defer);
    }
    token.Token ptok = p.tok;
    if (name != nil){
        p.tok = token.IDENT;
    } else 
    if (typeSetsOK && p.tok == token.TILDE) {
        // force token.IDENT case in switch below
        // "~" ...
        return new field(nil, p.embeddedElem(default!));
    }
    var exprᴛ1 = p.tok;
    if (exprᴛ1 == token.IDENT) {
        if (name != nil){
            // name
            f.name = name;
            p.tok = ptok;
        } else {
            f.name = p.parseIdent();
        }
        var exprᴛ2 = p.tok;
        if (exprᴛ2 == token.IDENT || exprᴛ2 == token.MUL || exprᴛ2 == token.ARROW || exprᴛ2 == token.FUNC || exprᴛ2 == token.CHAN || exprᴛ2 == token.MAP || exprᴛ2 == token.STRUCT || exprᴛ2 == token.INTERFACE || exprᴛ2 == token.LPAREN) {
            f.typ = p.parseType();
        }
        else if (exprᴛ2 == token.LBRACK) {
            (f.name, f.typ) = p.parseArrayFieldOrTypeInstance(f.name);
        }
        else if (exprᴛ2 == token.ELLIPSIS) {
            f.typ = p.parseDotsType();
            return f;
        }
        if (exprᴛ2 == token.PERIOD) {
            f.typ = p.parseQualifiedIdent(f.name);
            f.name = default!;
        }
        else if (exprᴛ2 == token.TILDE) {
            if (typeSetsOK) {
                // name type
                // name "[" type1, ..., typeN "]" or name "[" n "]" type
                // name "..." type
                // don't allow ...type "|" ...
                // name "." ...
                f.typ = p.embeddedElem(default!);
                return f;
            }
        }
        if (exprᴛ2 == token.OR) {
            if (typeSetsOK) {
                // name "|" typeset
                f.typ = p.embeddedElem(~f.name);
                f.name = default!;
                return f;
            }
        }

    }
    if (exprᴛ1 == token.MUL || exprᴛ1 == token.ARROW || exprᴛ1 == token.FUNC || exprᴛ1 == token.LBRACK || exprᴛ1 == token.CHAN || exprᴛ1 == token.MAP || exprᴛ1 == token.STRUCT || exprᴛ1 == token.INTERFACE || exprᴛ1 == token.LPAREN) {
        f.typ = p.parseType();
    }
    else if (exprᴛ1 == token.ELLIPSIS) {
        f.typ = p.parseDotsType();
        return f;
    }
    { /* default: */
        p.errorExpected(p.pos, // type
 // "..." type
 // (always accepted)
 // don't allow ...type "|" ...
 // TODO(rfindley): this is incorrect in the case of type parameter lists
 //                 (should be "']'" in that case)
 "')'"u8);
        p.advance(exprEnd);
    }

    // [name] type "|"
    if (typeSetsOK && p.tok == token.OR && f.typ != default!) {
        f.typ = p.embeddedElem(f.typ);
    }
    return f;
});

[GoRecv] internal static slice<ast.Field> /*params*/ parseParameterList(this ref parser p, ж<ast.Ident> Ꮡname0, ast.Expr typ0, token.Token closing) => func((defer, _) => {
    slice<ast.Field> @params = default!;

    ref var name0 = ref Ꮡname0.val;
    if (p.trace) {
        deferǃ(un, trace(p, "ParameterList"u8), defer);
    }
    // Type parameters are the only parameter list closed by ']'.
    var tparams = closing == token.RBRACK;
    tokenꓸPos pos0 = p.pos;
    if (name0 != nil){
        pos0 = name0.Pos();
    } else 
    if (typ0 != default!) {
        pos0 = typ0.Pos();
    }
    // Note: The code below matches the corresponding code in the syntax
    //       parser closely. Changes must be reflected in either parser.
    //       For the code to match, we use the local []field list that
    //       corresponds to []syntax.Field. At the end, the list must be
    //       converted into an []*ast.Field.
    slice<field> list = default!;
    nint named = default!;   // number of parameters that have an explicit name and type
    nint typed = default!;   // number of parameters that have an explicit type
    while (name0 != nil || p.tok != closing && p.tok != token.EOF) {
        field par = default!;
        if (typ0 != default!){
            if (tparams) {
                typ0 = p.embeddedElem(typ0);
            }
            par = new field(Ꮡname0, typ0);
        } else {
            par = p.parseParamDecl(Ꮡname0, tparams);
        }
        name0 = default!;
        // 1st name was consumed if present
        typ0 = default!;
        // 1st typ was consumed if present
        if (par.name != nil || par.typ != default!) {
            list = append(list, par);
            if (par.name != nil && par.typ != default!) {
                named++;
            }
            if (par.typ != default!) {
                typed++;
            }
        }
        if (!p.atComma("parameter list"u8, closing)) {
            break;
        }
        p.next();
    }
    if (len(list) == 0) {
        return @params;
    }
    // not uncommon
    // distribute parameter types (len(list) > 0)
    if (named == 0){
        // all unnamed => found names are type names
        for (nint i = 0; i < len(list); i++) {
            var par = Ꮡ(list, i);
            {
                var typΔ1 = par.val.name; if (typΔ1 != nil) {
                    par.val.typ = typΔ1;
                    par.val.name = default!;
                }
            }
        }
        if (tparams) {
            // This is the same error handling as below, adjusted for type parameters only.
            // See comment below for details. (go.dev/issue/64534)
            tokenꓸPos errPosΔ1 = default!;
            @string msgΔ1 = default!;
            if (named == typed){
                /* same as typed == 0 */
                 = p.pos;
                // position error at closing ]
                 = "missing type constraint"u8;
            } else {
                 = pos0;
                // position at opening [ or first name
                 = "missing type parameter name"u8;
                if (len(list) == 1) {
                     += " or invalid array length"u8;
                }
            }
            p.error(errPosΔ1, msgΔ1);
        }
    } else 
    if (named != len(list)) {
        // some named or we're in a type parameter list => all must be named
        ref var errPos = ref heap(new go.token_package.ΔPos(), out var ᏑerrPos);              // left-most error position (or invalid)
        ast.Expr typΔ2 = default!;                // current type (from right to left)
        for (nint i = len(list) - 1; i >= 0; i--) {
            {
                var par = Ꮡ(list, i); if ((~par).typ != default!){
                     = par.val.typ;
                    if ((~par).name == nil) {
                        errPos = typΔ2.Pos();
                        var n = ast.NewIdent("_"u8);
                        n.val.NamePos = errPos;
                        // correct position
                        par.val.name = n;
                    }
                } else 
                if (typΔ2 != default!){
                    par.val.typ = typΔ2;
                } else {
                    // par.typ == nil && typ == nil => we only have a par.name
                    errPos = (~par).name.Pos();
                    par.val.typ = Ꮡ(new ast.BadExpr(From: errPos, To: p.pos));
                }
            }
        }
        if (errPos.IsValid()) {
            @string msg = default!;
            if (tparams){
                // Not all parameters are named because named != len(list).
                // If named == typed we must have parameters that have no types,
                // and they must be at the end of the parameter list, otherwise
                // the types would have been filled in by the right-to-left sweep
                // above and we wouldn't have an error. Since we are in a type
                // parameter list, the missing types are constraints.
                if (named == typed){
                    errPos = p.pos;
                    // position error at closing ]
                    msg = "missing type constraint"u8;
                } else {
                    msg = "missing type parameter name"u8;
                    // go.dev/issue/60812
                    if (len(list) == 1) {
                        msg += " or invalid array length"u8;
                    }
                }
            } else {
                msg = "mixed named and unnamed parameters"u8;
            }
            p.error(errPos, msg);
        }
    }
    // Convert list to []*ast.Field.
    // If list contains types only, each type gets its own ast.Field.
    if (named == 0) {
        // parameter list consists of types only
        ref var par = ref heap(new field(), out var Ꮡpar);

        foreach (var (_, par) in list) {
            assert(par.typ != default!, "nil type in unnamed parameter list"u8);
            @params = append(@params, Ꮡ(new ast.Field(Type: par.typ)));
        }
        return @params;
    }
    // If the parameter list consists of named parameters with types,
    // collect all names with the same types into a single ast.Field.
    slice<ast.Ident> names = default!;
    ast.Expr typ = default!;
    var addParams = 
    var namesʗ1 = names;
    var paramsʗ1 = @params;
    var typʗ1 = typ;
    () => {
        assert(typʗ1 != default!, "nil type in named parameter list"u8);
        var field = Ꮡ(new ast.Field(Names: namesʗ1, Type: typʗ1));
        paramsʗ1 = append(paramsʗ1, field);
        namesʗ1 = default!;
    };
    ref var par = ref heap(new field(), out var Ꮡpar);

    foreach (var (_, par) in list) {
        if (!AreEqual(par.typ, typ)) {
            if (len(names) > 0) {
                addParams();
            }
            typ = par.typ;
        }
        names = append(names, par.name);
    }
    if (len(names) > 0) {
        addParams();
    }
    return @params;
});

[GoRecv] internal static (ж<ast.FieldList> tparams, ж<ast.FieldList> @params) parseParameters(this ref parser p, bool acceptTParams) => func((defer, _) => {
    ж<ast.FieldList> tparams = default!;
    ж<ast.FieldList> @params = default!;

    if (p.trace) {
        deferǃ(un, trace(p, "Parameters"u8), defer);
    }
    if (acceptTParams && p.tok == token.LBRACK) {
        ref var openingΔ1 = ref heap<go.token_package.ΔPos>(out var ᏑopeningΔ1);
        openingΔ1 = p.pos;
        p.next();
        // [T any](params) syntax
        var list = p.parseParameterList(nil, default!, token.RBRACK);
        ref var rbrack = ref heap<go.token_package.ΔPos>(out var Ꮡrbrack);
        rbrack = p.expect(token.RBRACK);
        tparams = Ꮡ(new ast.FieldList(Opening: openingΔ1, List: list, Closing: rbrack));
        // Type parameter lists must not be empty.
        if (tparams.NumFields() == 0) {
            p.error((~tparams).Closing, "empty type parameter list"u8);
            tparams = default!;
        }
    }
    // avoid follow-on errors
    ref var opening = ref heap<go.token_package.ΔPos>(out var Ꮡopening);
    opening = p.expect(token.LPAREN);
    slice<ast.Field> fields = default!;
    if (p.tok != token.RPAREN) {
        fields = p.parseParameterList(nil, default!, token.RPAREN);
    }
    ref var rparen = ref heap<go.token_package.ΔPos>(out var Ꮡrparen);
    rparen = p.expect(token.RPAREN);
    @params = Ꮡ(new ast.FieldList(Opening: opening, List: fields, Closing: rparen));
    return (tparams, @params);
});

[GoRecv] internal static ж<ast.FieldList> parseResult(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "Result"u8), defer);
    }
    if (p.tok == token.LPAREN) {
        (_, results) = p.parseParameters(false);
        return results;
    }
    var typ = p.tryIdentOrType();
    if (typ != default!) {
        var list = new slice<ast.Field>(1);
        list[0] = Ꮡ(new ast.Field(Type: typ));
        return Ꮡ(new ast.FieldList(List: list));
    }
    return default!;
});

[GoRecv] internal static ж<ast.FuncType> parseFuncType(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "FuncType"u8), defer);
    }
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.expect(token.FUNC);
    (tparams, @params) = p.parseParameters(true);
    if (tparams != nil) {
        p.error(tparams.Pos(), "function type must have no type parameters"u8);
    }
    var results = p.parseResult();
    return Ꮡ(new ast.FuncType(Func: pos, Params: @params, Results: results));
});

[GoRecv] internal static ж<ast.Field> parseMethodSpec(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "MethodSpec"u8), defer);
    }
    var doc = p.leadComment;
    slice<ast.Ident> idents = default!;
    ast.Expr typ = default!;
    var x = p.parseTypeName(nil);
    {
        var (ident, _) = x._<ж<ast.Ident>>(ᐧ); if (ident != nil){
            switch (ᐧ) {
            case {} when p.tok is token.LBRACK: {
                tokenꓸPos lbrack = p.pos;
                p.next();
                p.exprLev++;
                var xΔ2 = p.parseExpr();
                p.exprLev--;
                {
                    var (name0, _) = x._<ж<ast.Ident>>(ᐧ); if (name0 != nil && p.tok != token.COMMA && p.tok != token.RBRACK){
                        // generic method or embedded instantiated type
                        // generic method m[T any]
                        //
                        // Interface methods do not have type parameters. We parse them for a
                        // better error message and improved error recovery.
                        _ = p.parseParameterList(name0, default!, token.RBRACK);
                        _ = p.expect(token.RBRACK);
                        p.error(lbrack, "interface method must have no type parameters"u8);
                        // TODO(rfindley) refactor to share code with parseFuncType.
                        (_, @params) = p.parseParameters(false);
                        var results = p.parseResult();
                        idents = new ast.Ident[]{ident}.slice();
                        Ꮡtyp = new ast.FuncType(
                            Func: token.NoPos,
                            Params: @params,
                            Results: results
                        ); typ = ref Ꮡtyp.val;
                    } else {
                        // embedded instantiated type
                        // TODO(rfindley) should resolve all identifiers in x.
                        var list = new ast.Expr[]{xΔ2}.slice();
                        if (p.atComma("type argument list"u8, token.RBRACK)) {
                            p.exprLev++;
                            p.next();
                            while (p.tok != token.RBRACK && p.tok != token.EOF) {
                                list = append(list, p.parseType());
                                if (!p.atComma("type argument list"u8, token.RBRACK)) {
                                    break;
                                }
                                p.next();
                            }
                            p.exprLev--;
                        }
                        tokenꓸPos rbrack = p.expectClosing(token.RBRACK, "type argument list"u8);
                        typ = typeparams.PackIndexExpr(~ident, lbrack, list, rbrack);
                    }
                }
                break;
            }
            case {} when p.tok is token.LPAREN: {
                (_, @params) = p.parseParameters(false);
                var results = p.parseResult();
                idents = new ast.Ident[]{ // ordinary method
 // TODO(rfindley) refactor to share code with parseFuncType.
ident}.slice();
                Ꮡtyp = new ast.FuncType(Func: token.NoPos, Params: @params, Results: results); typ = ref Ꮡtyp.val;
                break;
            }
            default: {
                typ = x;
                break;
            }}

        } else {
            // embedded type
            // embedded, possibly instantiated type
            typ = x;
            if (p.tok == token.LBRACK) {
                // embedded instantiated interface
                typ = p.parseTypeInstance(typ);
            }
        }
    }
    // Comment is added at the callsite: the field below may joined with
    // additional type specs using '|'.
    // TODO(rfindley) this should be refactored.
    // TODO(rfindley) add more tests for comment handling.
    return Ꮡ(new ast.Field(Doc: doc, Names: idents, Type: typ));
});

[GoRecv] internal static ast.Expr embeddedElem(this ref parser p, ast.Expr x) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "EmbeddedElem"u8), defer);
    }
    if (x == default!) {
        x = p.embeddedTerm();
    }
    while (p.tok == token.OR) {
        var t = @new<ast.BinaryExpr>();
        t.val.OpPos = p.pos;
        t.val.Op = token.OR;
        p.next();
        t.val.X = x;
        t.val.Y = p.embeddedTerm();
        x = ~t;
    }
    return x;
});

[GoRecv] internal static ast.Expr embeddedTerm(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "EmbeddedTerm"u8), defer);
    }
    if (p.tok == token.TILDE) {
        var tΔ1 = @new<ast.UnaryExpr>();
        .val.OpPos = p.pos;
        .val.Op = token.TILDE;
        p.next();
        .val.X = p.parseType();
        return ~tΔ1;
    }
    var t = p.tryIdentOrType();
    if (t == default!) {
        ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
        pos = p.pos;
        p.errorExpected(pos, "~ term or type"u8);
        p.advance(exprEnd);
        return new ast.BadExpr(From: pos, To: p.pos);
    }
    return t;
});

[GoRecv] internal static ж<ast.InterfaceType> parseInterfaceType(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "InterfaceType"u8), defer);
    }
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.expect(token.INTERFACE);
    ref var lbrace = ref heap<go.token_package.ΔPos>(out var Ꮡlbrace);
    lbrace = p.expect(token.LBRACE);
    slice<ast.Field> list = default!;
parseElements:
    while (ᐧ) {
        switch (ᐧ) {
        case {} when p.tok is token.IDENT: {
            var f = p.parseMethodSpec();
            if ((~f).Names == default!) {
                f.val.Type = p.embeddedElem((~f).Type);
            }
            f.val.Comment = p.expectSemi();
            list = append(list, f);
            break;
        }
        case {} when p.tok is token.TILDE: {
            var typ = p.embeddedElem(default!);
            var comment = p.expectSemi();
            list = append(list, Ꮡ(new ast.Field(Type: typ, Comment: comment)));
            break;
        }
        default: {
            {
                var t = p.tryIdentOrType(); if (t != default!){
                    var typ = p.embeddedElem(t);
                    var comment = p.expectSemi();
                    list = append(list, Ꮡ(new ast.Field(Type: typ, Comment: comment)));
                } else {
                    goto break_parseElements;
                }
            }
            break;
        }}

continue_parseElements:;
    }
break_parseElements:;
    // TODO(rfindley): the error produced here could be improved, since we could
    // accept an identifier, 'type', or a '}' at this point.
    ref var rbrace = ref heap<go.token_package.ΔPos>(out var Ꮡrbrace);
    rbrace = p.expect(token.RBRACE);
    return Ꮡ(new ast.InterfaceType(
        Interface: pos,
        Methods: Ꮡ(new ast.FieldList(
            Opening: lbrace,
            List: list,
            Closing: rbrace
        ))
    ));
});

[GoRecv] internal static ж<ast.MapType> parseMapType(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "MapType"u8), defer);
    }
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.expect(token.MAP);
    p.expect(token.LBRACK);
    var key = p.parseType();
    p.expect(token.RBRACK);
    var value = p.parseType();
    return Ꮡ(new ast.MapType(Map: pos, Key: key, Value: value));
});

[GoRecv] internal static ж<ast.ChanType> parseChanType(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "ChanType"u8), defer);
    }
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.pos;
    ref var dir = ref heap<go.ast_package.ChanDir>(out var Ꮡdir);
    dir = (ast.ChanDir)(ast.SEND | ast.RECV);
    ref var arrow = ref heap(new go.token_package.ΔPos(), out var Ꮡarrow);
    if (p.tok == token.CHAN){
        p.next();
        if (p.tok == token.ARROW) {
            arrow = p.pos;
            p.next();
            dir = ast.SEND;
        }
    } else {
        arrow = p.expect(token.ARROW);
        p.expect(token.CHAN);
        dir = ast.RECV;
    }
    var value = p.parseType();
    return Ꮡ(new ast.ChanType(Begin: pos, Arrow: arrow, Dir: dir, Value: value));
});

[GoRecv] internal static ast.Expr parseTypeInstance(this ref parser p, ast.Expr typ) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "TypeInstance"u8), defer);
    }
    ref var opening = ref heap<go.token_package.ΔPos>(out var Ꮡopening);
    opening = p.expect(token.LBRACK);
    p.exprLev++;
    slice<ast.Expr> list = default!;
    while (p.tok != token.RBRACK && p.tok != token.EOF) {
        list = append(list, p.parseType());
        if (!p.atComma("type argument list"u8, token.RBRACK)) {
            break;
        }
        p.next();
    }
    p.exprLev--;
    ref var closing = ref heap<go.token_package.ΔPos>(out var Ꮡclosing);
    closing = p.expectClosing(token.RBRACK, "type argument list"u8);
    if (len(list) == 0) {
        p.errorExpected(closing, "type argument list"u8);
        return new ast.IndexExpr(
            X: typ,
            Lbrack: opening,
            Index: Ꮡ(new ast.BadExpr(From: opening + 1, To: closing)),
            Rbrack: closing
        );
    }
    return typeparams.PackIndexExpr(typ, opening, list, closing);
});

[GoRecv] internal static ast.Expr tryIdentOrType(this ref parser p) => func((defer, _) => {
    deferǃ(decNestLev, incNestLev(p), defer);
    var exprᴛ1 = p.tok;
    if (exprᴛ1 == token.IDENT) {
        var typ = p.parseTypeName(nil);
        if (p.tok == token.LBRACK) {
            typ = p.parseTypeInstance(typ);
        }
        return typ;
    }
    if (exprᴛ1 == token.LBRACK) {
        tokenꓸPos lbrack = p.expect(token.LBRACK);
        return ~p.parseArrayType(lbrack, default!);
    }
    if (exprᴛ1 == token.STRUCT) {
        return ~p.parseStructType();
    }
    if (exprᴛ1 == token.MUL) {
        return ~p.parsePointerType();
    }
    if (exprᴛ1 == token.FUNC) {
        return ~p.parseFuncType();
    }
    if (exprᴛ1 == token.INTERFACE) {
        return ~p.parseInterfaceType();
    }
    if (exprᴛ1 == token.MAP) {
        return ~p.parseMapType();
    }
    if (exprᴛ1 == token.CHAN || exprᴛ1 == token.ARROW) {
        return ~p.parseChanType();
    }
    if (exprᴛ1 == token.LPAREN) {
        ref var lparen = ref heap<go.token_package.ΔPos>(out var Ꮡlparen);
        lparen = p.pos;
        p.next();
        var typ = p.parseType();
        ref var rparen = ref heap<go.token_package.ΔPos>(out var Ꮡrparen);
        rparen = p.expect(token.RPAREN);
        return new ast.ParenExpr(Lparen: lparen, X: typ, Rparen: rparen);
    }

    // no type found
    return default!;
});

// ----------------------------------------------------------------------------
// Blocks
[GoRecv] internal static slice<ast.Stmt> /*list*/ parseStmtList(this ref parser p) => func((defer, _) => {
    slice<ast.Stmt> list = default!;

    if (p.trace) {
        deferǃ(un, trace(p, "StatementList"u8), defer);
    }
    while (p.tok != token.CASE && p.tok != token.DEFAULT && p.tok != token.RBRACE && p.tok != token.EOF) {
        list = append(list, p.parseStmt());
    }
    return list;
});

[GoRecv] internal static ж<ast.BlockStmt> parseBody(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "Body"u8), defer);
    }
    ref var lbrace = ref heap<go.token_package.ΔPos>(out var Ꮡlbrace);
    lbrace = p.expect(token.LBRACE);
    var list = p.parseStmtList();
    ref var rbrace = ref heap<go.token_package.ΔPos>(out var Ꮡrbrace);
    rbrace = p.expect2(token.RBRACE);
    return Ꮡ(new ast.BlockStmt(Lbrace: lbrace, List: list, Rbrace: rbrace));
});

[GoRecv] internal static ж<ast.BlockStmt> parseBlockStmt(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "BlockStmt"u8), defer);
    }
    ref var lbrace = ref heap<go.token_package.ΔPos>(out var Ꮡlbrace);
    lbrace = p.expect(token.LBRACE);
    var list = p.parseStmtList();
    ref var rbrace = ref heap<go.token_package.ΔPos>(out var Ꮡrbrace);
    rbrace = p.expect2(token.RBRACE);
    return Ꮡ(new ast.BlockStmt(Lbrace: lbrace, List: list, Rbrace: rbrace));
});

// ----------------------------------------------------------------------------
// Expressions
[GoRecv] internal static ast.Expr parseFuncTypeOrLit(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "FuncTypeOrLit"u8), defer);
    }
    var typ = p.parseFuncType();
    if (p.tok != token.LBRACE) {
        // function type only
        return ~typ;
    }
    p.exprLev++;
    var body = p.parseBody();
    p.exprLev--;
    return new ast.FuncLit(Type: typ, Body: body);
});

// parseOperand may return an expression or a raw type (incl. array
// types of the form [...]T). Callers must verify the result.
[GoRecv] internal static ast.Expr parseOperand(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "Operand"u8), defer);
    }
    var exprᴛ1 = p.tok;
    if (exprᴛ1 == token.IDENT) {
        var x = p.parseIdent();
        return ~x;
    }
    if (exprᴛ1 == token.INT || exprᴛ1 == token.FLOAT || exprᴛ1 == token.IMAG || exprᴛ1 == token.CHAR || exprᴛ1 == token.STRING) {
        var x = Ꮡ(new ast.BasicLit(ValuePos: p.pos, Kind: p.tok, Value: p.lit));
        p.next();
        return ~x;
    }
    if (exprᴛ1 == token.LPAREN) {
        ref var lparen = ref heap<go.token_package.ΔPos>(out var Ꮡlparen);
        lparen = p.pos;
        p.next();
        p.exprLev++;
        var x = p.parseRhs();
        p.exprLev--;
        ref var rparen = ref heap<go.token_package.ΔPos>(out var Ꮡrparen);
        rparen = p.expect(token.RPAREN);
        return new ast.ParenExpr( // types may be parenthesized: (some type)
Lparen: lparen, X: x, Rparen: rparen);
    }
    if (exprᴛ1 == token.FUNC) {
        return p.parseFuncTypeOrLit();
    }

    {
        var typ = p.tryIdentOrType(); if (typ != default!) {
            // do not consume trailing type parameters
            // could be type for composite literal or conversion
            var (_, isIdent) = typ._<ж<ast.Ident>>(ᐧ);
            assert(!isIdent, "type cannot be identifier"u8);
            return typ;
        }
    }
    // we have an error
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.pos;
    p.errorExpected(pos, "operand"u8);
    p.advance(stmtStart);
    return new ast.BadExpr(From: pos, To: p.pos);
});

[GoRecv] internal static ast.Expr parseSelector(this ref parser p, ast.Expr x) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "Selector"u8), defer);
    }
    var sel = p.parseIdent();
    return new ast.SelectorExpr(X: x, Sel: sel);
});

[GoRecv] internal static ast.Expr parseTypeAssertion(this ref parser p, ast.Expr x) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "TypeAssertion"u8), defer);
    }
    ref var lparen = ref heap<go.token_package.ΔPos>(out var Ꮡlparen);
    lparen = p.expect(token.LPAREN);
    ast.Expr typ = default!;
    if (p.tok == token.TYPE){
        // type switch: typ == nil
        p.next();
    } else {
        typ = p.parseType();
    }
    ref var rparen = ref heap<go.token_package.ΔPos>(out var Ꮡrparen);
    rparen = p.expect(token.RPAREN);
    return new ast.TypeAssertExpr(X: x, Type: typ, Lparen: lparen, Rparen: rparen);
});

[GoRecv] internal static ast.Expr parseIndexOrSliceOrInstance(this ref parser p, ast.Expr x) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "parseIndexOrSliceOrInstance"u8), defer);
    }
    ref var lbrack = ref heap<go.token_package.ΔPos>(out var Ꮡlbrack);
    lbrack = p.expect(token.LBRACK);
    if (p.tok == token.RBRACK) {
        // empty index, slice or index expressions are not permitted;
        // accept them for parsing tolerance, but complain
        p.errorExpected(p.pos, "operand"u8);
        ref var rbrackΔ1 = ref heap<go.token_package.ΔPos>(out var ᏑrbrackΔ1);
        rbrackΔ1 = p.pos;
        p.next();
        return new ast.IndexExpr(
            X: x,
            Lbrack: lbrack,
            Index: Ꮡ(new ast.BadExpr(From: rbrackΔ1, To: rbrackΔ1)),
            Rbrack: rbrackΔ1
        );
    }
    p.exprLev++;
    static readonly UntypedInt N = 3; // change the 3 to 2 to disable 3-index slices
    slice<ast.Expr> args = default!;
    array<ast.Expr> index = new(3); /* N */
    array<tokenꓸPos> colons = new(2); /* N - 1 */
    if (p.tok != token.COLON) {
        // We can't know if we have an index expression or a type instantiation;
        // so even if we see a (named) type we are not going to be in type context.
        index[0] = p.parseRhs();
    }
    nint ncolons = 0;
    var exprᴛ1 = p.tok;
    if (exprᴛ1 == token.COLON) {
        while (p.tok == token.COLON && ncolons < len(colons)) {
            // slice expression
            colons[ncolons] = p.pos;
            ncolons++;
            p.next();
            if (p.tok != token.COLON && p.tok != token.RBRACK && p.tok != token.EOF) {
                index[ncolons] = p.parseRhs();
            }
        }
    }
    else if (exprᴛ1 == token.COMMA) {
        args = append(args, // instance expression
 index[0]);
        while (p.tok == token.COMMA) {
            p.next();
            if (p.tok != token.RBRACK && p.tok != token.EOF) {
                args = append(args, p.parseType());
            }
        }
    }

    p.exprLev--;
    ref var rbrack = ref heap<go.token_package.ΔPos>(out var Ꮡrbrack);
    rbrack = p.expect(token.RBRACK);
    if (ncolons > 0) {
        // slice expression
        ref var slice3 = ref heap<bool>(out var Ꮡslice3);
        slice3 = false;
        if (ncolons == 2) {
            slice3 = true;
            // Check presence of middle and final index here rather than during type-checking
            // to prevent erroneous programs from passing through gofmt (was go.dev/issue/7305).
            if (index[1] == default!) {
                p.error(colons[0], "middle index required in 3-index slice"u8);
                index[1] = Ꮡ(new ast.BadExpr(From: colons[0] + 1, To: colons[1]));
            }
            if (index[2] == default!) {
                p.error(colons[1], "final index required in 3-index slice"u8);
                index[2] = Ꮡ(new ast.BadExpr(From: colons[1] + 1, To: rbrack));
            }
        }
        return new ast.SliceExpr(X: x, Lbrack: lbrack, Low: index[0], High: index[1], Max: index[2], Slice3: slice3, Rbrack: rbrack);
    }
    if (len(args) == 0) {
        // index expression
        return new ast.IndexExpr(X: x, Lbrack: lbrack, Index: index[0], Rbrack: rbrack);
    }
    // instance expression
    return typeparams.PackIndexExpr(x, lbrack, args, rbrack);
});

[GoRecv] internal static ж<ast.CallExpr> parseCallOrConversion(this ref parser p, ast.Expr fun) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "CallOrConversion"u8), defer);
    }
    ref var lparen = ref heap<go.token_package.ΔPos>(out var Ꮡlparen);
    lparen = p.expect(token.LPAREN);
    p.exprLev++;
    slice<ast.Expr> list = default!;
    ref var ellipsis = ref heap(new go.token_package.ΔPos(), out var Ꮡellipsis);
    while (p.tok != token.RPAREN && p.tok != token.EOF && !ellipsis.IsValid()) {
        list = append(list, p.parseRhs());
        // builtins may expect a type: make(some type, ...)
        if (p.tok == token.ELLIPSIS) {
            ellipsis = p.pos;
            p.next();
        }
        if (!p.atComma("argument list"u8, token.RPAREN)) {
            break;
        }
        p.next();
    }
    p.exprLev--;
    ref var rparen = ref heap<go.token_package.ΔPos>(out var Ꮡrparen);
    rparen = p.expectClosing(token.RPAREN, "argument list"u8);
    return Ꮡ(new ast.CallExpr(Fun: fun, Lparen: lparen, Args: list, Ellipsis: ellipsis, Rparen: rparen));
});

[GoRecv] internal static ast.Expr parseValue(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "Element"u8), defer);
    }
    if (p.tok == token.LBRACE) {
        return p.parseLiteralValue(default!);
    }
    var x = p.parseExpr();
    return x;
});

[GoRecv] internal static ast.Expr parseElement(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "Element"u8), defer);
    }
    var x = p.parseValue();
    if (p.tok == token.COLON) {
        ref var colon = ref heap<go.token_package.ΔPos>(out var Ꮡcolon);
        colon = p.pos;
        p.next();
        Ꮡx = new ast.KeyValueExpr(Key: x, Colon: colon, Value: p.parseValue()); x = ref Ꮡx.val;
    }
    return x;
});

[GoRecv] internal static slice<ast.Expr> /*list*/ parseElementList(this ref parser p) => func((defer, _) => {
    slice<ast.Expr> list = default!;

    if (p.trace) {
        deferǃ(un, trace(p, "ElementList"u8), defer);
    }
    while (p.tok != token.RBRACE && p.tok != token.EOF) {
        list = append(list, p.parseElement());
        if (!p.atComma("composite literal"u8, token.RBRACE)) {
            break;
        }
        p.next();
    }
    return list;
});

[GoRecv] internal static ast.Expr parseLiteralValue(this ref parser p, ast.Expr typ) => func((defer, _) => {
    deferǃ(decNestLev, incNestLev(p), defer);
    if (p.trace) {
        deferǃ(un, trace(p, "LiteralValue"u8), defer);
    }
    ref var lbrace = ref heap<go.token_package.ΔPos>(out var Ꮡlbrace);
    lbrace = p.expect(token.LBRACE);
    slice<ast.Expr> elts = default!;
    p.exprLev++;
    if (p.tok != token.RBRACE) {
        elts = p.parseElementList();
    }
    p.exprLev--;
    ref var rbrace = ref heap<go.token_package.ΔPos>(out var Ꮡrbrace);
    rbrace = p.expectClosing(token.RBRACE, "composite literal"u8);
    return new ast.CompositeLit(Type: typ, Lbrace: lbrace, Elts: elts, Rbrace: rbrace);
});

[GoRecv] internal static ast.Expr parsePrimaryExpr(this ref parser p, ast.Expr x) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "PrimaryExpr"u8), defer);
    }
    if (x == default!) {
        x = p.parseOperand();
    }
    // We track the nesting here rather than at the entry for the function,
    // since it can iteratively produce a nested output, and we want to
    // limit how deep a structure we generate.
    nint n = default!;
    defer(() => {
        p.nestLev -= n;
    });
    for (n = 1; ᐧ ; n++) {
        incNestLev(p);
        var exprᴛ1 = p.tok;
        if (exprᴛ1 == token.PERIOD) {
            p.next();
            var exprᴛ2 = p.tok;
            if (exprᴛ2 == token.IDENT) {
                x = p.parseSelector(x);
            }
            else if (exprᴛ2 == token.LPAREN) {
                x = p.parseTypeAssertion(x);
            }
            else { /* default: */
                ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
                pos = p.pos;
                p.errorExpected(pos, "selector or type assertion"u8);
                if (p.tok != token.RBRACE) {
                    // TODO(rFindley) The check for token.RBRACE below is a targeted fix
                    //                to error recovery sufficient to make the x/tools tests to
                    //                pass with the new parsing logic introduced for type
                    //                parameters. Remove this once error recovery has been
                    //                more generally reconsidered.
                    p.next();
                }
                var sel = Ꮡ(new ast.Ident( // make progress
NamePos: pos, Name: "_"u8));
                Ꮡx = new ast.SelectorExpr(X: x, Sel: sel); x = ref Ꮡx.val;
            }

        }
        else if (exprᴛ1 == token.LBRACK) {
            x = p.parseIndexOrSliceOrInstance(x);
        }
        else if (exprᴛ1 == token.LPAREN) {
            x = ~p.parseCallOrConversion(x);
        }
        else if (exprᴛ1 == token.LBRACE) {
            var t = ast.Unparen(x);
            switch (t.type()) {
            case ж<ast.BadExpr> : {
                if (p.exprLev < 0) {
                    // operand may have returned a parenthesized complit
                    // type; accept it but complain if we have a complit
                    // determine if '{' belongs to a composite literal or a block statement
                    return x;
                }
                break;
            }
            case ж<ast.Ident> : {
                if (p.exprLev < 0) {
                    return x;
                }
                break;
            }
            case ж<ast.SelectorExpr> : {
                if (p.exprLev < 0) {
                    return x;
                }
                break;
            }
            case ж<ast.IndexExpr> : {
                if (p.exprLev < 0) {
                    // x is possibly a composite literal type
                    return x;
                }
                break;
            }
            case ж<ast.IndexListExpr> : {
                if (p.exprLev < 0) {
                    return x;
                }
                break;
            }
            case ж<ast.ArrayType> : {
                break;
            }
            case ж<ast.StructType> : {
                break;
            }
            case ж<ast.MapType> : {
                break;
            }
            default: {

                return x;
            }}

            if (!AreEqual(t, x)) {
                // x is possibly a composite literal type
                // x is a composite literal type
                p.error(t.Pos(), "cannot parenthesize type in composite literal"u8);
            }
            x = p.parseLiteralValue(x);
        }
        else { /* default: */
            return x;
        }

    }
});

// already progressed, no need to advance
[GoRecv] internal static ast.Expr parseUnaryExpr(this ref parser p) => func((defer, _) => {
    deferǃ(decNestLev, incNestLev(p), defer);
    if (p.trace) {
        deferǃ(un, trace(p, "UnaryExpr"u8), defer);
    }
    var exprᴛ1 = p.tok;
    if (exprᴛ1 == token.ADD || exprᴛ1 == token.SUB || exprᴛ1 == token.NOT || exprᴛ1 == token.XOR || exprᴛ1 == token.AND || exprᴛ1 == token.TILDE) {
        ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
        pos = p.pos;
        ref var op = ref heap<go.token_package.Token>(out var Ꮡop);
        op = p.tok;
        p.next();
        var x = p.parseUnaryExpr();
        return new ast.UnaryExpr(OpPos: pos, Op: op, X: x);
    }
    if (exprᴛ1 == token.ARROW) {
        ref var arrow = ref heap<go.token_package.ΔPos>(out var Ꮡarrow);
        arrow = p.pos;
        p.next();
        var x = p.parseUnaryExpr();
        {
            var (typ, ok) = x._<ж<ast.ChanType>>(ᐧ); if (ok) {
                // channel type or receive expression
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
                // determine which case we have
                // (<-type)
                // re-associate position info and <-
                ast.ChanDir dir = ast.SEND;
                while (ok && dir == ast.SEND) {
                    if ((~typ).Dir == ast.RECV) {
                        // error: (<-type) is (<-(<-chan T))
                        p.errorExpected((~typ).Arrow, "'chan'"u8);
                    }
                    (arrow, typ.val.Begin, typ.val.Arrow) = (typ.val.Arrow, arrow, arrow);
                    (dir, typ.val.Dir) = (typ.val.Dir, ast.RECV);
                    (typ, ok) = (~typ).Value._<ж<ast.ChanType>>(ᐧ);
                }
                if (dir == ast.SEND) {
                    p.errorExpected(arrow, "channel type"u8);
                }
                return x;
            }
        }
        return new ast.UnaryExpr( // <-(expr)
OpPos: arrow, Op: token.ARROW, X: x);
    }
    if (exprᴛ1 == token.MUL) {
        ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
        pos = p.pos;
        p.next();
        var x = p.parseUnaryExpr();
        return new ast.StarExpr( // pointer type or unary "*" expression
Star: pos, X: x);
    }

    return p.parsePrimaryExpr(default!);
});

[GoRecv] internal static (token.Token, nint) tokPrec(this ref parser p) {
    token.Token tok = p.tok;
    if (p.inRhs && tok == token.ASSIGN) {
        tok = token.EQL;
    }
    return (tok, tok.Precedence());
}

// parseBinaryExpr parses a (possibly) binary expression.
// If x is non-nil, it is used as the left operand.
//
// TODO(rfindley): parseBinaryExpr has become overloaded. Consider refactoring.
[GoRecv] internal static ast.Expr parseBinaryExpr(this ref parser p, ast.Expr x, nint prec1) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "BinaryExpr"u8), defer);
    }
    if (x == default!) {
        x = p.parseUnaryExpr();
    }
    // We track the nesting here rather than at the entry for the function,
    // since it can iteratively produce a nested output, and we want to
    // limit how deep a structure we generate.
    nint n = default!;
    defer(() => {
        p.nestLev -= n;
    });
    for (n = 1; ᐧ ; n++) {
        incNestLev(p);
        var (op, oprec) = p.tokPrec();
        if (oprec < prec1) {
            return x;
        }
        ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
        pos = p.expect(op);
        var y = p.parseBinaryExpr(default!, oprec + 1);
        Ꮡx = new ast.BinaryExpr(X: x, OpPos: pos, Op: op, Y: y); x = ref Ꮡx.val;
    }
});

// The result may be a type or even a raw type ([...]int).
[GoRecv] internal static ast.Expr parseExpr(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "Expression"u8), defer);
    }
    return p.parseBinaryExpr(default!, token.LowestPrec + 1);
});

[GoRecv] internal static ast.Expr parseRhs(this ref parser p) {
    var old = p.inRhs;
    p.inRhs = true;
    var x = p.parseExpr();
    p.inRhs = old;
    return x;
}

// ----------------------------------------------------------------------------
// Statements

// Parsing modes for parseSimpleStmt.
internal static readonly UntypedInt basic = iota;

internal static readonly UntypedInt labelOk = 1;

internal static readonly UntypedInt rangeOk = 2;

// parseSimpleStmt returns true as 2nd result if it parsed the assignment
// of a range clause (with mode == rangeOk). The returned statement is an
// assignment with a right-hand side that is a single unary expression of
// the form "range x". No guarantees are given for the left-hand side.
[GoRecv] internal static (ast.Stmt, bool) parseSimpleStmt(this ref parser p, nint mode) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "SimpleStmt"u8), defer);
    }
    var x = p.parseList(false);
    var exprᴛ1 = p.tok;
    if (exprᴛ1 == token.DEFINE || exprᴛ1 == token.ASSIGN || exprᴛ1 == token.ADD_ASSIGN || exprᴛ1 == token.SUB_ASSIGN || exprᴛ1 == token.MUL_ASSIGN || exprᴛ1 == token.QUO_ASSIGN || exprᴛ1 == token.REM_ASSIGN || exprᴛ1 == token.AND_ASSIGN || exprᴛ1 == token.OR_ASSIGN || exprᴛ1 == token.XOR_ASSIGN || exprᴛ1 == token.SHL_ASSIGN || exprᴛ1 == token.SHR_ASSIGN || exprᴛ1 == token.AND_NOT_ASSIGN) {
        ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
        pos = p.pos;
        ref var tok = ref heap<go.token_package.Token>(out var Ꮡtok);
        tok = p.tok;
        p.next();
// assignment statement, possibly part of a range clause
        slice<ast.Expr> y = default!;
        var isRange = false;
        if (mode == rangeOk && p.tok == token.RANGE && (tok == token.DEFINE || tok == token.ASSIGN)){
            ref var posΔ1 = ref heap<go.token_package.ΔPos>(out var ᏑposΔ1);
            posΔ1 = p.pos;
            p.next();
            y = new ast.Expr[]{new ast.UnaryExpr(OpPos: posΔ1, Op: token.RANGE, X: p.parseRhs())}.slice();
            isRange = true;
        } else {
            y = p.parseList(true);
        }
        return (new ast.AssignStmt(Lhs: x, TokPos: pos, Tok: tok, Rhs: y), isRange);
    }

    if (len(x) > 1) {
        p.errorExpected(x[0].Pos(), "1 expression"u8);
    }
    // continue with first expression
    var exprᴛ2 = p.tok;
    if (exprᴛ2 == token.COLON) {
        ref var colon = ref heap<go.token_package.ΔPos>(out var Ꮡcolon);
        colon = p.pos;
        p.next();
        {
            var (label, isIdent) = x[0]._<ж<ast.Ident>>(ᐧ); if (mode == labelOk && isIdent) {
                // labeled statement
                // Go spec: The scope of a label is the body of the function
                // in which it is declared and excludes the body of any nested
                // function.
                var stmt = Ꮡ(new ast.LabeledStmt(Label: label, Colon: colon, Stmt: p.parseStmt()));
                return (~stmt, false);
            }
        }
        p.error(colon, // The label declaration typically starts at x[0].Pos(), but the label
 // declaration may be erroneous due to a token after that position (and
 // before the ':'). If SpuriousErrors is not set, the (only) error
 // reported for the line is the illegal label error instead of the token
 // before the ':' that caused the problem. Thus, use the (latest) colon
 // position for error reporting.
 "illegal label declaration"u8);
        return (new ast.BadStmt(From: x[0].Pos(), To: colon + 1), false);
    }
    if (exprᴛ2 == token.ARROW) {
        ref var arrow = ref heap<go.token_package.ΔPos>(out var Ꮡarrow);
        arrow = p.pos;
        p.next();
        var y = p.parseRhs();
        return (new ast.SendStmt( // send statement
Chan: x[0], Arrow: arrow, Value: y), false);
    }
    if (exprᴛ2 == token.INC || exprᴛ2 == token.DEC) {
        var s = Ꮡ(new ast.IncDecStmt( // increment or decrement
X: x[0], TokPos: p.pos, Tok: p.tok));
        p.next();
        return (~s, false);
    }

    // expression
    return (new ast.ExprStmt(X: x[0]), false);
});

[GoRecv] internal static ж<ast.CallExpr> parseCallExpr(this ref parser p, @string callType) {
    var x = p.parseRhs();
    // could be a conversion: (some type)(x)
    {
        var t = ast.Unparen(x); if (!AreEqual(t, x)) {
            p.error(x.Pos(), fmt.Sprintf("expression in %s must not be parenthesized"u8, callType));
            x = t;
        }
    }
    {
        var (call, isCall) = x._<ж<ast.CallExpr>>(ᐧ); if (isCall) {
            return call;
        }
    }
    {
        var (_, isBad) = x._<ж<ast.BadExpr>>(ᐧ); if (!isBad) {
            // only report error if it's a new one
            p.error(p.safePos(x.End()), fmt.Sprintf("expression in %s must be function call"u8, callType));
        }
    }
    return default!;
}

[GoRecv] internal static ast.Stmt parseGoStmt(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "GoStmt"u8), defer);
    }
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.expect(token.GO);
    var call = p.parseCallExpr("go"u8);
    p.expectSemi();
    if (call == nil) {
        return new ast.BadStmt(From: pos, To: pos + 2);
    }
    // len("go")
    return new ast.GoStmt(Go: pos, Call: call);
});

[GoRecv] internal static ast.Stmt parseDeferStmt(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "DeferStmt"u8), defer);
    }
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.expect(token.DEFER);
    var call = p.parseCallExpr("defer"u8);
    p.expectSemi();
    if (call == nil) {
        return new ast.BadStmt(From: pos, To: pos + 5);
    }
    // len("defer")
    return new ast.DeferStmt(Defer: pos, Call: call);
});

[GoRecv] internal static ж<ast.ReturnStmt> parseReturnStmt(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "ReturnStmt"u8), defer);
    }
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.pos;
    p.expect(token.RETURN);
    slice<ast.Expr> x = default!;
    if (p.tok != token.SEMICOLON && p.tok != token.RBRACE) {
        x = p.parseList(true);
    }
    p.expectSemi();
    return Ꮡ(new ast.ReturnStmt(Return: pos, Results: x));
});

[GoRecv] internal static ж<ast.BranchStmt> parseBranchStmt(this ref parser p, token.Token tok) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "BranchStmt"u8), defer);
    }
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.expect(tok);
    ж<ast.Ident> label = default!;
    if (tok != token.FALLTHROUGH && p.tok == token.IDENT) {
        label = p.parseIdent();
    }
    p.expectSemi();
    return Ꮡ(new ast.BranchStmt(TokPos: pos, Tok: tok, Label: label));
});

[GoRecv] internal static ast.Expr makeExpr(this ref parser p, ast.Stmt s, @string want) {
    if (s == default!) {
        return default!;
    }
    {
        var (es, isExpr) = s._<ж<ast.ExprStmt>>(ᐧ); if (isExpr) {
            return (~es).X;
        }
    }
    @string found = "simple statement"u8;
    {
        var (_, isAss) = s._<ж<ast.AssignStmt>>(ᐧ); if (isAss) {
            found = "assignment"u8;
        }
    }
    p.error(s.Pos(), fmt.Sprintf("expected %s, found %s (missing parentheses around composite literal?)"u8, want, found));
    return new ast.BadExpr(From: s.Pos(), To: p.safePos(s.End()));
}

[GoType("dyn")] partial struct parseIfHeader_semi {
    internal go.token_package.ΔPos pos;
    internal @string lit; // ";" or "\n"; valid if pos.IsValid()
}

// parseIfHeader is an adjusted version of parser.header
// in cmd/compile/internal/syntax/parser.go, which has
// been tuned for better error handling.
[GoRecv] internal static (ast.Stmt init, ast.Expr cond) parseIfHeader(this ref parser p) {
    ast.Stmt init = default!;
    ast.Expr cond = default!;

    if (p.tok == token.LBRACE) {
        p.error(p.pos, "missing condition in if statement"u8);
        Ꮡcond = new ast.BadExpr(From: p.pos, To: p.pos); cond = ref Ꮡcond.val;
        return (init, cond);
    }
    // p.tok != token.LBRACE
    nint prevLev = p.exprLev;
    p.exprLev = -1;
    if (p.tok != token.SEMICOLON) {
        // accept potential variable declaration but complain
        if (p.tok == token.VAR) {
            p.next();
            p.error(p.pos, "var declaration not allowed in if initializer"u8);
        }
        (init, _) = p.parseSimpleStmt(basic);
    }
    ast.Stmt condStmt = default!;
    parseIfHeader_semi semi = default!;
    if (p.tok != token.LBRACE){
        if (p.tok == token.SEMICOLON){
            semi.pos = p.pos;
            semi.lit = p.lit;
            p.next();
        } else {
            p.expect(token.SEMICOLON);
        }
        if (p.tok != token.LBRACE) {
            (condStmt, _) = p.parseSimpleStmt(basic);
        }
    } else {
        condStmt = init;
        init = default!;
    }
    if (condStmt != default!){
        cond = p.makeExpr(condStmt, "boolean expression"u8);
    } else 
    if (semi.pos.IsValid()) {
        if (semi.lit == "\n"u8){
            p.error(semi.pos, "unexpected newline, expecting { after if clause"u8);
        } else {
            p.error(semi.pos, "missing condition in if statement"u8);
        }
    }
    // make sure we have a valid AST
    if (cond == default!) {
        Ꮡcond = new ast.BadExpr(From: p.pos, To: p.pos); cond = ref Ꮡcond.val;
    }
    p.exprLev = prevLev;
    return (init, cond);
}

[GoRecv] internal static ж<ast.IfStmt> parseIfStmt(this ref parser p) => func((defer, _) => {
    deferǃ(decNestLev, incNestLev(p), defer);
    if (p.trace) {
        deferǃ(un, trace(p, "IfStmt"u8), defer);
    }
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.expect(token.IF);
    (init, cond) = p.parseIfHeader();
    var body = p.parseBlockStmt();
    ast.Stmt else_ = default!;
    if (p.tok == token.ELSE){
        p.next();
        var exprᴛ1 = p.tok;
        if (exprᴛ1 == token.IF) {
            else_ = ~p.parseIfStmt();
        }
        else if (exprᴛ1 == token.LBRACE) {
            else_ = ~p.parseBlockStmt();
            p.expectSemi();
        }
        else { /* default: */
            p.errorExpected(p.pos, "if statement or block"u8);
            Ꮡelse_ = new ast.BadStmt(From: p.pos, To: p.pos); else_ = ref Ꮡelse_.val;
        }

    } else {
        p.expectSemi();
    }
    return Ꮡ(new ast.IfStmt(If: pos, Init: init, Cond: cond, Body: body, Else: else_));
});

[GoRecv] internal static ж<ast.CaseClause> parseCaseClause(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "CaseClause"u8), defer);
    }
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.pos;
    slice<ast.Expr> list = default!;
    if (p.tok == token.CASE){
        p.next();
        list = p.parseList(true);
    } else {
        p.expect(token.DEFAULT);
    }
    ref var colon = ref heap<go.token_package.ΔPos>(out var Ꮡcolon);
    colon = p.expect(token.COLON);
    var body = p.parseStmtList();
    return Ꮡ(new ast.CaseClause(Case: pos, List: list, Colon: colon, Body: body));
});

internal static bool isTypeSwitchAssert(ast.Expr x) {
    var (a, ok) = x._<ж<ast.TypeAssertExpr>>(ᐧ);
    return ok && (~a).Type == default!;
}

[GoRecv] internal static bool isTypeSwitchGuard(this ref parser p, ast.Stmt s) {
    switch (s.type()) {
    case ж<ast.ExprStmt> t: {
        return isTypeSwitchAssert((~t).X);
    }
    case ж<ast.AssignStmt> t: {
        if (len((~t).Lhs) == 1 && len((~t).Rhs) == 1 && isTypeSwitchAssert((~t).Rhs[0])) {
            // x.(type)
            // v := x.(type)
            var exprᴛ1 = (~t).Tok;
            var matchᴛ1 = false;
            if (exprᴛ1 == token.ASSIGN) {
                p.error((~t).TokPos, // permit v = x.(type) but complain
 "expected ':=', found '='"u8);
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && exprᴛ1 == token.DEFINE)) { matchᴛ1 = true;
                return true;
            }

        }
        break;
    }}
    return false;
}

[GoRecv] internal static ast.Stmt parseSwitchStmt(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "SwitchStmt"u8), defer);
    }
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.expect(token.SWITCH);
    ast.Stmt s1 = default!;
    ast.Stmt s2 = default!;
    if (p.tok != token.LBRACE) {
        nint prevLev = p.exprLev;
        p.exprLev = -1;
        if (p.tok != token.SEMICOLON) {
            (s2, _) = p.parseSimpleStmt(basic);
        }
        if (p.tok == token.SEMICOLON) {
            p.next();
            s1 = s2;
            s2 = default!;
            if (p.tok != token.LBRACE) {
                // A TypeSwitchGuard may declare a variable in addition
                // to the variable declared in the initial SimpleStmt.
                // Introduce extra scope to avoid redeclaration errors:
                //
                //	switch t := 0; t := x.(T) { ... }
                //
                // (this code is not valid Go because the first t
                // cannot be accessed and thus is never used, the extra
                // scope is needed for the correct error message).
                //
                // If we don't have a type switch, s2 must be an expression.
                // Having the extra nested but empty scope won't affect it.
                (s2, _) = p.parseSimpleStmt(basic);
            }
        }
        p.exprLev = prevLev;
    }
    var typeSwitch = p.isTypeSwitchGuard(s2);
    ref var lbrace = ref heap<go.token_package.ΔPos>(out var Ꮡlbrace);
    lbrace = p.expect(token.LBRACE);
    slice<ast.Stmt> list = default!;
    while (p.tok == token.CASE || p.tok == token.DEFAULT) {
        list = append(list, ~p.parseCaseClause());
    }
    ref var rbrace = ref heap<go.token_package.ΔPos>(out var Ꮡrbrace);
    rbrace = p.expect(token.RBRACE);
    p.expectSemi();
    var body = Ꮡ(new ast.BlockStmt(Lbrace: lbrace, List: list, Rbrace: rbrace));
    if (typeSwitch) {
        return new ast.TypeSwitchStmt(Switch: pos, Init: s1, Assign: s2, Body: body);
    }
    return new ast.SwitchStmt(Switch: pos, Init: s1, Tag: p.makeExpr(s2, "switch expression"u8), Body: body);
});

[GoRecv] internal static ж<ast.CommClause> parseCommClause(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "CommClause"u8), defer);
    }
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.pos;
    ast.Stmt comm = default!;
    if (p.tok == token.CASE){
        p.next();
        var lhs = p.parseList(false);
        if (p.tok == token.ARROW){
            // SendStmt
            if (len(lhs) > 1) {
                p.errorExpected(lhs[0].Pos(), "1 expression"u8);
            }
            // continue with first expression
            ref var arrow = ref heap<go.token_package.ΔPos>(out var Ꮡarrow);
            arrow = p.pos;
            p.next();
            var rhs = p.parseRhs();
            Ꮡcomm = new ast.SendStmt(Chan: lhs[0], Arrow: arrow, Value: rhs); comm = ref Ꮡcomm.val;
        } else {
            // RecvStmt
            {
                ref var tok = ref heap<go.token_package.Token>(out var Ꮡtok);
                tok = p.tok; if (tok == token.ASSIGN || tok == token.DEFINE){
                    // RecvStmt with assignment
                    if (len(lhs) > 2) {
                        p.errorExpected(lhs[0].Pos(), "1 or 2 expressions"u8);
                        // continue with first two expressions
                        lhs = lhs[0..2];
                    }
                    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
                    pos = p.pos;
                    p.next();
                    var rhs = p.parseRhs();
                    Ꮡcomm = new ast.AssignStmt(Lhs: lhs, TokPos: pos, Tok: tok, Rhs: new ast.Expr[]{rhs}.slice()); comm = ref Ꮡcomm.val;
                } else {
                    // lhs must be single receive operation
                    if (len(lhs) > 1) {
                        p.errorExpected(lhs[0].Pos(), "1 expression"u8);
                    }
                    // continue with first expression
                    Ꮡcomm = new ast.ExprStmt(X: lhs[0]); comm = ref Ꮡcomm.val;
                }
            }
        }
    } else {
        p.expect(token.DEFAULT);
    }
    ref var colon = ref heap<go.token_package.ΔPos>(out var Ꮡcolon);
    colon = p.expect(token.COLON);
    var body = p.parseStmtList();
    return Ꮡ(new ast.CommClause(Case: pos, Comm: comm, Colon: colon, Body: body));
});

[GoRecv] internal static ж<ast.SelectStmt> parseSelectStmt(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "SelectStmt"u8), defer);
    }
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.expect(token.SELECT);
    ref var lbrace = ref heap<go.token_package.ΔPos>(out var Ꮡlbrace);
    lbrace = p.expect(token.LBRACE);
    slice<ast.Stmt> list = default!;
    while (p.tok == token.CASE || p.tok == token.DEFAULT) {
        list = append(list, ~p.parseCommClause());
    }
    ref var rbrace = ref heap<go.token_package.ΔPos>(out var Ꮡrbrace);
    rbrace = p.expect(token.RBRACE);
    p.expectSemi();
    var body = Ꮡ(new ast.BlockStmt(Lbrace: lbrace, List: list, Rbrace: rbrace));
    return Ꮡ(new ast.SelectStmt(Select: pos, Body: body));
});

[GoRecv] internal static ast.Stmt parseForStmt(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "ForStmt"u8), defer);
    }
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.expect(token.FOR);
    ast.Stmt s1 = default!;
    ast.Stmt s2 = default!;
    ast.Stmt s3 = default!;
    bool isRange = default!;
    if (p.tok != token.LBRACE) {
        nint prevLev = p.exprLev;
        p.exprLev = -1;
        if (p.tok != token.SEMICOLON) {
            if (p.tok == token.RANGE){
                // "for range x" (nil lhs in assignment)
                ref var posΔ1 = ref heap<go.token_package.ΔPos>(out var ᏑposΔ1);
                posΔ1 = p.pos;
                p.next();
                var y = new ast.Expr[]{new ast.UnaryExpr(OpPos: posΔ1, Op: token.RANGE, X: p.parseRhs())}.slice();
                Ꮡs2 = new ast.AssignStmt(Rhs: y); s2 = ref Ꮡs2.val;
                isRange = true;
            } else {
                (s2, isRange) = p.parseSimpleStmt(rangeOk);
            }
        }
        if (!isRange && p.tok == token.SEMICOLON) {
            p.next();
            s1 = s2;
            s2 = default!;
            if (p.tok != token.SEMICOLON) {
                (s2, _) = p.parseSimpleStmt(basic);
            }
            p.expectSemi();
            if (p.tok != token.LBRACE) {
                (s3, _) = p.parseSimpleStmt(basic);
            }
        }
        p.exprLev = prevLev;
    }
    var body = p.parseBlockStmt();
    p.expectSemi();
    if (isRange) {
        var @as = s2._<ж<ast.AssignStmt>>();
        // check lhs
        ast.Expr key = default!;
        ast.Expr value = default!;
        switch (len((~@as).Lhs)) {
        case 0: {
            break;
        }
        case 1: {
            key = (~@as).Lhs[0];
            break;
        }
        case 2: {
            (key, value) = ((~@as).Lhs[0], (~@as).Lhs[1]);
            break;
        }
        default: {
            p.errorExpected((~@as).Lhs[len((~@as).Lhs) - 1].Pos(), // nothing to do
 "at most 2 expressions"u8);
            return new ast.BadStmt(From: pos, To: p.safePos(body.End()));
        }}

        // parseSimpleStmt returned a right-hand side that
        // is a single unary expression of the form "range x"
        var x = (~@as).Rhs[0]._<ж<ast.UnaryExpr>>().X;
        return new ast.RangeStmt(
            For: pos,
            Key: key,
            Value: value,
            TokPos: (~@as).TokPos,
            Tok: (~@as).Tok,
            Range: (~@as).Rhs[0].Pos(),
            X: x,
            Body: body
        );
    }
    // regular for statement
    return new ast.ForStmt(
        For: pos,
        Init: s1,
        Cond: p.makeExpr(s2, "boolean or range expression"u8),
        Post: s3,
        Body: body
    );
});

[GoRecv] internal static ast.Stmt /*s*/ parseStmt(this ref parser p) => func((defer, _) => {
    ast.Stmt s = default!;

    deferǃ(decNestLev, incNestLev(p), defer);
    if (p.trace) {
        deferǃ(un, trace(p, "Statement"u8), defer);
    }
    var exprᴛ1 = p.tok;
    if (exprᴛ1 == token.CONST || exprᴛ1 == token.TYPE || exprᴛ1 == token.VAR) {
        Ꮡs = new ast.DeclStmt(Decl: p.parseDecl(stmtStart)); s = ref Ꮡs.val;
    }
    else if (exprᴛ1 == token.IDENT || exprᴛ1 == token.INT || exprᴛ1 == token.FLOAT || exprᴛ1 == token.IMAG || exprᴛ1 == token.CHAR || exprᴛ1 == token.STRING || exprᴛ1 == token.FUNC || exprᴛ1 == token.LPAREN || exprᴛ1 == token.LBRACK || exprᴛ1 == token.STRUCT || exprᴛ1 == token.MAP || exprᴛ1 == token.CHAN || exprᴛ1 == token.INTERFACE || exprᴛ1 == token.ADD || exprᴛ1 == token.SUB || exprᴛ1 == token.MUL || exprᴛ1 == token.AND || exprᴛ1 == token.XOR || exprᴛ1 == token.ARROW || exprᴛ1 == token.NOT) {
        (s, _) = p.parseSimpleStmt(labelOk);
        {
            var (_, isLabeledStmt) = s._<ж<ast.LabeledStmt>>(ᐧ); if (!isLabeledStmt) {
                // tokens that may start an expression
                // operands
                // composite types
                // unary operators
                // because of the required look-ahead, labeled statements are
                // parsed by parseSimpleStmt - don't expect a semicolon after
                // them
                p.expectSemi();
            }
        }
    }
    else if (exprᴛ1 == token.GO) {
        s = p.parseGoStmt();
    }
    else if (exprᴛ1 == token.DEFER) {
        s = p.parseDeferStmt();
    }
    else if (exprᴛ1 == token.RETURN) {
        s = ~p.parseReturnStmt();
    }
    else if (exprᴛ1 == token.BREAK || exprᴛ1 == token.CONTINUE || exprᴛ1 == token.GOTO || exprᴛ1 == token.FALLTHROUGH) {
        s = ~p.parseBranchStmt(p.tok);
    }
    else if (exprᴛ1 == token.LBRACE) {
        s = ~p.parseBlockStmt();
        p.expectSemi();
    }
    else if (exprᴛ1 == token.IF) {
        s = ~p.parseIfStmt();
    }
    else if (exprᴛ1 == token.SWITCH) {
        s = p.parseSwitchStmt();
    }
    else if (exprᴛ1 == token.SELECT) {
        s = ~p.parseSelectStmt();
    }
    else if (exprᴛ1 == token.FOR) {
        s = p.parseForStmt();
    }
    else if (exprᴛ1 == token.SEMICOLON) {
        Ꮡs = new ast.EmptyStmt( // Is it ever possible to have an implicit semicolon
 // producing an empty statement in a valid program?
 // (handle correctly anyway)
Semicolon: p.pos, Implicit: p.lit == "\n"u8); s = ref Ꮡs.val;
        p.next();
    }
    else if (exprᴛ1 == token.RBRACE) {
        Ꮡs = new ast.EmptyStmt( // a semicolon may be omitted before a closing "}"
Semicolon: p.pos, Implicit: true); s = ref Ꮡs.val;
    }
    else { /* default: */
        ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
        pos = p.pos;
        p.errorExpected(pos, // no statement found
 "statement"u8);
        p.advance(stmtStart);
        Ꮡs = new ast.BadStmt(From: pos, To: p.pos); s = ref Ꮡs.val;
    }

    return s;
});

internal delegate ast.Spec parseSpecFunction(ж<ast.CommentGroup> doc, token.Token keyword, nint iota);

// ----------------------------------------------------------------------------
// Declarations
[GoRecv] internal static ast.Spec parseImportSpec(this ref parser p, ж<ast.CommentGroup> Ꮡdoc, token.Token _, nint _) => func((defer, _) => {
    ref var doc = ref Ꮡdoc.val;

    if (p.trace) {
        deferǃ(un, trace(p, "ImportSpec"u8), defer);
    }
    ж<ast.Ident> ident = default!;
    var exprᴛ1 = p.tok;
    if (exprᴛ1 == token.IDENT) {
        ident = p.parseIdent();
    }
    else if (exprᴛ1 == token.PERIOD) {
        ident = Ꮡ(new ast.Ident(NamePos: p.pos, Name: "."u8));
        p.next();
    }

    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.pos;
    ref var path = ref heap(new @string(), out var Ꮡpath);
    if (p.tok == token.STRING){
        path = p.lit;
        p.next();
    } else 
    if (p.tok.IsLiteral()){
        p.error(pos, "import path must be a string"u8);
        p.next();
    } else {
        p.error(pos, "missing import path"u8);
        p.advance(exprEnd);
    }
    var comment = p.expectSemi();
    // collect imports
    var spec = Ꮡ(new ast.ImportSpec(
        Doc: doc,
        Name: ident,
        Path: Ꮡ(new ast.BasicLit(ValuePos: pos, Kind: token.STRING, Value: path)),
        Comment: comment
    ));
    p.imports = append(p.imports, spec);
    return ~spec;
});

[GoRecv] internal static ast.Spec parseValueSpec(this ref parser p, ж<ast.CommentGroup> Ꮡdoc, token.Token keyword, nint iota) => func((defer, _) => {
    ref var doc = ref Ꮡdoc.val;

    if (p.trace) {
        deferǃ(un, trace(p, keyword.String() + "Spec"u8), defer);
    }
    var idents = p.parseIdentList();
    ast.Expr typ = default!;
    slice<ast.Expr> values = default!;
    var exprᴛ1 = keyword;
    if (exprᴛ1 == token.CONST) {
        if (p.tok != token.EOF && p.tok != token.SEMICOLON && p.tok != token.RPAREN) {
            // always permit optional type and initialization for more tolerant parsing
            typ = p.tryIdentOrType();
            if (p.tok == token.ASSIGN) {
                p.next();
                values = p.parseList(true);
            }
        }
    }
    else if (exprᴛ1 == token.VAR) {
        if (p.tok != token.ASSIGN) {
            typ = p.parseType();
        }
        if (p.tok == token.ASSIGN) {
            p.next();
            values = p.parseList(true);
        }
    }
    else { /* default: */
        throw panic("unreachable");
    }

    var comment = p.expectSemi();
    var spec = Ꮡ(new ast.ValueSpec(
        Doc: doc,
        Names: idents,
        Type: typ,
        Values: values,
        Comment: comment
    ));
    return ~spec;
});

[GoRecv] internal static void parseGenericType(this ref parser p, ж<ast.TypeSpec> Ꮡspec, tokenꓸPos openPos, ж<ast.Ident> Ꮡname0, ast.Expr typ0) => func((defer, _) => {
    ref var spec = ref Ꮡspec.val;
    ref var name0 = ref Ꮡname0.val;

    if (p.trace) {
        deferǃ(un, trace(p, "parseGenericType"u8), defer);
    }
    var list = p.parseParameterList(Ꮡname0, typ0, token.RBRACK);
    ref var closePos = ref heap<go.token_package.ΔPos>(out var ᏑclosePos);
    closePos = p.expect(token.RBRACK);
    spec.TypeParams = Ꮡ(new ast.FieldList(Opening: openPos, List: list, Closing: closePos));
    // Let the type checker decide whether to accept type parameters on aliases:
    // see go.dev/issue/46477.
    if (p.tok == token.ASSIGN) {
        // type alias
        spec.Assign = p.pos;
        p.next();
    }
    spec.Type = p.parseType();
});

[GoRecv] internal static ast.Spec parseTypeSpec(this ref parser p, ж<ast.CommentGroup> Ꮡdoc, token.Token _, nint _) => func((defer, _) => {
    ref var doc = ref Ꮡdoc.val;

    if (p.trace) {
        deferǃ(un, trace(p, "TypeSpec"u8), defer);
    }
    var name = p.parseIdent();
    var spec = Ꮡ(new ast.TypeSpec(Doc: doc, Name: name));
    if (p.tok == token.LBRACK){
        // spec.Name "[" ...
        // array/slice type or type parameter list
        tokenꓸPos lbrack = p.pos;
        p.next();
        if (p.tok == token.IDENT){
            // We may have an array type or a type parameter list.
            // In either case we expect an expression x (which may
            // just be a name, or a more complex expression) which
            // we can analyze further.
            //
            // A type parameter list may have a type bound starting
            // with a "[" as in: P []E. In that case, simply parsing
            // an expression would lead to an error: P[] is invalid.
            // But since index or slice expressions are never constant
            // and thus invalid array length expressions, if the name
            // is followed by "[" it must be the start of an array or
            // slice constraint. Only if we don't see a "[" do we
            // need to parse a full expression. Notably, name <- x
            // is not a concern because name <- x is a statement and
            // not an expression.
            ast.Expr x = p.parseIdent();
            if (p.tok != token.LBRACK) {
                // To parse the expression starting with name, expand
                // the call sequence we would get by passing in name
                // to parser.expr, and pass in name to parsePrimaryExpr.
                p.exprLev++;
                var lhs = p.parsePrimaryExpr(x);
                x = p.parseBinaryExpr(lhs, token.LowestPrec + 1);
                p.exprLev--;
            }
            // Analyze expression x. If we can split x into a type parameter
            // name, possibly followed by a type parameter type, we consider
            // this the start of a type parameter list, with some caveats:
            // a single name followed by "]" tilts the decision towards an
            // array declaration; a type parameter type that could also be
            // an ordinary expression but which is followed by a comma tilts
            // the decision towards a type parameter list.
            {
                (pname, ptype) = extractName(x, p.tok == token.COMMA); if (pname != nil && (ptype != default! || p.tok != token.RBRACK)){
                    // spec.Name "[" pname ...
                    // spec.Name "[" pname ptype ...
                    // spec.Name "[" pname ptype "," ...
                    p.parseGenericType(spec, lbrack, pname, ptype);
                } else {
                    // ptype may be nil
                    // spec.Name "[" pname "]" ...
                    // spec.Name "[" x ...
                    spec.val.Type = p.parseArrayType(lbrack, x);
                }
            }
        } else {
            // array type
            spec.val.Type = p.parseArrayType(lbrack, default!);
        }
    } else {
        // no type parameters
        if (p.tok == token.ASSIGN) {
            // type alias
            spec.val.Assign = p.pos;
            p.next();
        }
        spec.val.Type = p.parseType();
    }
    spec.val.Comment = p.expectSemi();
    return ~spec;
});

// extractName splits the expression x into (name, expr) if syntactically
// x can be written as name expr. The split only happens if expr is a type
// element (per the isTypeElem predicate) or if force is set.
// If x is just a name, the result is (name, nil). If the split succeeds,
// the result is (name, expr). Otherwise the result is (nil, x).
// Examples:
//
//	x           force    name    expr
//	------------------------------------
//	P*[]int     T/F      P       *[]int
//	P*E         T        P       *E
//	P*E         F        nil     P*E
//	P([]int)    T/F      P       []int
//	P(E)        T        P       E
//	P(E)        F        nil     P(E)
//	P*E|F|~G    T/F      P       *E|F|~G
//	P*E|F|G     T        P       *E|F|G
//	P*E|F|G     F        nil     P*E|F|G
internal static (ж<ast.Ident>, ast.Expr) extractName(ast.Expr x, bool force) {
    switch (x.type()) {
    case ж<ast.Ident> x: {
        return (Ꮡx, default!);
    }
    case ж<ast.BinaryExpr> x: {
        var exprᴛ1 = (~x).Op;
        if (exprᴛ1 == token.MUL) {
            {
                var (name, _) = (~x).X._<ж<ast.Ident>>(ᐧ); if (name != nil && (force || isTypeElem((~x).Y))) {
                    // x = name *x.Y
                    return (name, new ast.StarExpr(Star: (~x).OpPos, X: (~x).Y));
                }
            }
        }
        if (exprᴛ1 == token.OR) {
            {
                (name, lhs) = extractName((~x).X, force || isTypeElem((~x).Y)); if (name != nil && lhs != default!) {
                    // x = name lhs|x.Y
                    ref var op = ref heap<go.ast_package.BinaryExpr>(out var Ꮡop);
                    op = x;
                    op.X = lhs;
                    return (name, ~Ꮡop);
                }
            }
        }

        break;
    }
    case ж<ast.CallExpr> x: {
        {
            var (name, _) = (~x).Fun._<ж<ast.Ident>>(ᐧ); if (name != nil) {
                if (len((~x).Args) == 1 && (~x).Ellipsis == token.NoPos && (force || isTypeElem((~x).Args[0]))) {
                    // x = name "(" x.ArgList[0] ")"
                    return (name, (~x).Args[0]);
                }
            }
        }
        break;
    }}
    return (default!, x);
}

// isTypeElem reports whether x is a (possibly parenthesized) type element expression.
// The result is false if x could be a type element OR an ordinary (value) expression.
internal static bool isTypeElem(ast.Expr x) {
    switch (x.type()) {
    case ж<ast.ArrayType> x: {
        return true;
    }
    case ж<ast.StructType> x: {
        return true;
    }
    case ж<ast.FuncType> x: {
        return true;
    }
    case ж<ast.InterfaceType> x: {
        return true;
    }
    case ж<ast.MapType> x: {
        return true;
    }
    case ж<ast.ChanType> x: {
        return true;
    }
    case ж<ast.BinaryExpr> x: {
        return isTypeElem((~x).X) || isTypeElem((~x).Y);
    }
    case ж<ast.UnaryExpr> x: {
        return (~x).Op == token.TILDE;
    }
    case ж<ast.ParenExpr> x: {
        return isTypeElem((~x).X);
    }}
    return false;
}

[GoRecv] internal static ж<ast.GenDecl> parseGenDecl(this ref parser p, token.Token keyword, parseSpecFunction f) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "GenDecl("u8 + keyword.String() + ")"u8), defer);
    }
    var doc = p.leadComment;
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.expect(keyword);
    ref var lparen = ref heap(new go.token_package.ΔPos(), out var Ꮡlparen);
    ref var rparen = ref heap(new go.token_package.ΔPos(), out var Ꮡrparen);
    slice<ast.Spec> list = default!;
    if (p.tok == token.LPAREN){
        lparen = p.pos;
        p.next();
        for (nint iota = 0; p.tok != token.RPAREN && p.tok != token.EOF; iota++) {
            list = append(list, f(p.leadComment, keyword, iota));
        }
        rparen = p.expect(token.RPAREN);
        p.expectSemi();
    } else {
        list = append(list, f(nil, keyword, 0));
    }
    return Ꮡ(new ast.GenDecl(
        Doc: doc,
        TokPos: pos,
        Tok: keyword,
        Lparen: lparen,
        Specs: list,
        Rparen: rparen
    ));
});

[GoRecv] internal static ж<ast.FuncDecl> parseFuncDecl(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "FunctionDecl"u8), defer);
    }
    var doc = p.leadComment;
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.expect(token.FUNC);
    ж<ast.FieldList> recv = default!;
    if (p.tok == token.LPAREN) {
        (_, recv) = p.parseParameters(false);
    }
    var ident = p.parseIdent();
    (tparams, @params) = p.parseParameters(true);
    if (recv != nil && tparams != nil) {
        // Method declarations do not have type parameters. We parse them for a
        // better error message and improved error recovery.
        p.error((~tparams).Opening, "method must have no type parameters"u8);
        tparams = default!;
    }
    var results = p.parseResult();
    ж<ast.BlockStmt> body = default!;
    var exprᴛ1 = p.tok;
    if (exprᴛ1 == token.LBRACE) {
        body = p.parseBody();
        p.expectSemi();
    }
    else if (exprᴛ1 == token.SEMICOLON) {
        p.next();
        if (p.tok == token.LBRACE) {
            // opening { of function declaration on next line
            p.error(p.pos, "unexpected semicolon or newline before {"u8);
            body = p.parseBody();
            p.expectSemi();
        }
    }
    else { /* default: */
        p.expectSemi();
    }

    var decl = Ꮡ(new ast.FuncDecl(
        Doc: doc,
        Recv: recv,
        Name: ident,
        Type: Ꮡ(new ast.FuncType(
            Func: pos,
            TypeParams: tparams,
            Params: @params,
            Results: results
        )),
        Body: body
    ));
    return decl;
});

[GoRecv] internal static ast.Decl parseDecl(this ref parser p, token.Token>bool sync) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "Declaration"u8), defer);
    }
    parseSpecFunction f = default!;
    var exprᴛ1 = p.tok;
    if (exprᴛ1 == token.IMPORT) {
        f = () => p.parseImportSpec();
    }
    else if (exprᴛ1 == token.CONST || exprᴛ1 == token.VAR) {
        f = () => p.parseValueSpec();
    }
    else if (exprᴛ1 == token.TYPE) {
        f = () => p.parseTypeSpec();
    }
    else if (exprᴛ1 == token.FUNC) {
        return ~p.parseFuncDecl();
    }
    { /* default: */
        ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
        pos = p.pos;
        p.errorExpected(pos, "declaration"u8);
        p.advance(sync);
        return new ast.BadDecl(From: pos, To: p.pos);
    }

    return ~p.parseGenDecl(p.tok, f);
});

// ----------------------------------------------------------------------------
// Source files
[GoRecv] internal static ж<ast.File> parseFile(this ref parser p) => func((defer, _) => {
    if (p.trace) {
        deferǃ(un, trace(p, "File"u8), defer);
    }
    // Don't bother parsing the rest if we had errors scanning the first token.
    // Likely not a Go source file at all.
    if (p.errors.Len() != 0) {
        return default!;
    }
    // package clause
    var doc = p.leadComment;
    ref var pos = ref heap<go.token_package.ΔPos>(out var Ꮡpos);
    pos = p.expect(token.PACKAGE);
    // Go spec: The package clause is not a declaration;
    // the package name does not appear in any scope.
    var ident = p.parseIdent();
    if ((~ident).Name == "_"u8 && (Mode)(p.mode & DeclarationErrors) != 0) {
        p.error(p.pos, "invalid package name _"u8);
    }
    p.expectSemi();
    // Don't bother parsing the rest if we had errors parsing the package clause.
    // Likely not a Go source file at all.
    if (p.errors.Len() != 0) {
        return default!;
    }
    slice<ast.Decl> decls = default!;
    if ((Mode)(p.mode & PackageClauseOnly) == 0) {
        // import decls
        while (p.tok == token.IMPORT) {
            decls = append(decls, ~p.parseGenDecl(token.IMPORT, p.parseImportSpec));
        }
        if ((Mode)(p.mode & ImportsOnly) == 0) {
            // rest of package body
            token.Token prev = token.IMPORT;
            while (p.tok != token.EOF) {
                // Continue to accept import declarations for error tolerance, but complain.
                if (p.tok == token.IMPORT && prev != token.IMPORT) {
                    p.error(p.pos, "imports must appear before other declarations"u8);
                }
                prev = p.tok;
                decls = append(decls, p.parseDecl(declStart));
            }
        }
    }
    var f = Ꮡ(new ast.File(
        Doc: doc,
        Package: pos,
        Name: ident,
        Decls: decls,
        FileStart: ((tokenꓸPos)p.file.Base()),
        FileEnd: ((tokenꓸPos)(p.file.Base() + p.file.Size())),
        Imports: p.imports,
        Comments: p.comments,
        GoVersion: p.goVersion
    ));
    token.Pos, string) declErr = default!;
    if ((Mode)(p.mode & DeclarationErrors) != 0) {
        declErr = () => p.error();
    }
    if ((Mode)(p.mode & SkipObjectResolution) == 0) {
        resolveFile(f, p.file, declErr);
    }
    return f;
});

} // end parser_package
