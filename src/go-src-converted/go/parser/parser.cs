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
using ast = global::go.go.ast_package;
using constraint = global::go.go.build.constraint_package;
using typeparams = global::go.go.@internal.typeparams_package;
using scanner = global::go.go.scanner_package;
using token = global::go.go.token_package;
using strings = strings_package;
using global::go.go;
using global::go.go.@internal;
using global::go.go.build;
using ꓸꓸꓸany = Span<any>;

partial class parser_package {

// The parser structure holds the parser's internal state.
[GoType] partial struct parser {
    internal ж<tokenꓸFile> @file;
    internal scanner.ErrorList errors;
    internal scanner.Scanner scanner;
    // Tracing/debugging
    internal Mode mode; // parsing mode
    internal bool trace; // == (mode&Trace != 0)
    internal nint indent; // indentation used for tracing output
    // Comments
    internal slice<ж<ast.CommentGroup>> comments;
    internal ж<ast.CommentGroup> leadComment; // last lead comment
    internal ж<ast.CommentGroup> lineComment; // last line comment
    internal bool top;              // in top of file (before package clause)
    internal @string goVersion;           // minimum Go version found in //go:build comment
    // Next token
    internal tokenꓸPos pos;   // token position
    internal token.Token tok; // one token look-ahead
    internal @string lit;     // token literal
    // Error recovery
    // (used to limit the number of calls to parser.advance
    // w/o making scanning progress - avoids potential endless
    // loops across multiple parser functions during error recovery)
    internal tokenꓸPos syncPos; // last synchronization position
    internal nint syncCnt;      // number of parser.advance calls without progress
    // Non-syntactic parser control
    internal nint exprLev; // < 0: in control clause, >= 0: in expression
    internal bool inRhs; // if set, the parser is parsing a rhs expression
    internal slice<ж<ast.ImportSpec>> imports; // list of imports
    // nestLev is used to track and limit the recursion depth
    // during parsing.
    internal nint nestLev;
}

internal static void init(this ж<parser> Ꮡp, ж<token.FileSet> Ꮡfset, @string filename, slice<byte> src, Mode mode) {
    ref var p = ref Ꮡp.Value;

    p.@file = Ꮡfset.AddFile(filename, -1, len(src));
    var eh = (tokenꓸPosition pos, @string msg) => {
        Ꮡp.Value.errors.Add(pos, msg);
    };
    p.scanner.Init(p.@file, src, new Action<tokenꓸPosition, @string>(eh), scanner.ScanComments);
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
    var pos = p.@file.Position(p.pos);
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
    ref var p = ref Ꮡp.Value;

    p.printTrace(msg, "(");
    p.indent++;
    return Ꮡp;
}

// Usage pattern: defer un(trace(p, "..."))
internal static void un(ж<parser> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    p.indent--;
    p.printTrace(")");
}

// maxNestLev is the deepest we're willing to recurse during parsing
internal const nint maxNestLev = 100000;

internal static ж<parser> incNestLev(ж<parser> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    p.nestLev++;
    if (p.nestLev > maxNestLev) {
        Ꮡp.error(p.pos, "exceeded max nesting depth"u8);
        throw panic(new bailout(nil));
    }
    return Ꮡp;
}

// decNestLev is used to track nesting depth during parsing to prevent stack exhaustion.
// It is used along with incNestLev in a similar fashion to how un and trace are used.
internal static void decNestLev(ж<parser> Ꮡp) {
    ref var p = ref Ꮡp.Value;

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
            p.printTrace("\"" + s + "\"");
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
                    var (x, err) = constraint.Parse(p.lit); if (err == default!) {
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
    endline = p.@file.Line(p.pos);
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

    slice<ж<ast.Comment>> list = default!;
    endline = p.@file.Line(p.pos);
    while (p.tok == token.COMMENT && p.@file.Line(p.pos) <= endline + n) {
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
        if (p.@file.Line(p.pos) == p.@file.Line(prev)) {
            // The comment is on same line as the previous token; it
            // cannot be a lead comment but may be a line comment.
            (comment, endline) = p.consumeCommentGroup(0);
            if (p.@file.Line(p.pos) != endline || p.tok == token.SEMICOLON || p.tok == token.EOF) {
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
        if (endline + 1 == p.@file.Line(p.pos)) {
            // The next token is following on the line immediately after the
            // comment group, thus the last comment group is a lead comment.
            p.leadComment = comment;
        }
    }
}

// A bailout panic is raised to indicate early termination. pos and msg are
// only populated when bailing out of object resolution.
[GoType] partial struct bailout {
    internal tokenꓸPos pos;
    internal @string msg;
}

internal static void error(this ж<parser> Ꮡp, tokenꓸPos pos, @string msg) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "error: "u8 + msg), defer);
    }
    var epos = p.@file.Position(pos);
    // If AllErrors is not set, discard errors reported on the same line
    // as the last recorded error and stop parsing if there are more than
    // 10 errors.
    if ((Mode)(p.mode & AllErrors) == 0) {
        nint n = len(p.errors);
        if (n > 0 && (~p.errors[n - 1]).Pos.Line == epos.Line) {
            return;
        }
        // discard - likely a spurious error
        if (n > 10) {
            throw panic(new bailout(nil));
        }
    }
    p.errors.Add(epos, msg);
});

internal static void errorExpected(this ж<parser> Ꮡp, tokenꓸPos pos, @string msg) {
    ref var p = ref Ꮡp.Value;

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
    Ꮡp.error(pos, msg);
}

internal static tokenꓸPos expect(this ж<parser> Ꮡp, token.Token tok) {
    ref var p = ref Ꮡp.Value;

    tokenꓸPos pos = p.pos;
    if (p.tok != tok) {
        Ꮡp.errorExpected(pos, "'"u8 + tok.String() + "'"u8);
    }
    p.next();
    // make progress
    return pos;
}

// expect2 is like expect, but it returns an invalid position
// if the expected token is not found.
internal static tokenꓸPos /*pos*/ expect2(this ж<parser> Ꮡp, token.Token tok) {
    tokenꓸPos pos = default!;

    ref var p = ref Ꮡp.Value;
    if (p.tok == tok){
        pos = p.pos;
    } else {
        Ꮡp.errorExpected(p.pos, "'"u8 + tok.String() + "'"u8);
    }
    p.next();
    // make progress
    return pos;
}

// expectClosing is like expect but provides a better error message
// for the common case of a missing comma before a newline.
internal static tokenꓸPos expectClosing(this ж<parser> Ꮡp, token.Token tok, @string context) {
    ref var p = ref Ꮡp.Value;

    if (p.tok != tok && p.tok == token.SEMICOLON && p.lit == "\n"u8) {
        Ꮡp.error(p.pos, "missing ',' before newline in "u8 + context);
        p.next();
    }
    return Ꮡp.expect(tok);
}

// expectSemi consumes a semicolon and returns the applicable line comment.
internal static ж<ast.CommentGroup> /*comment*/ expectSemi(this ж<parser> Ꮡp) {
    ж<ast.CommentGroup> comment = default!;

    ref var p = ref Ꮡp.Value;
    // semicolon is optional before a closing ')' or '}'
    if (p.tok != token.RPAREN && p.tok != token.RBRACE) {
        var exprᴛ1 = p.tok;
        var matchᴛ1 = false;
        if (exprᴛ1 == token.COMMA) { matchᴛ1 = true;
            Ꮡp.errorExpected(p.pos, // permit a ',' instead of a ';' but complain
 "';'"u8);
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 == token.SEMICOLON) {
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
            Ꮡp.errorExpected(p.pos, "';'"u8);
            p.advance(stmtStart);
        }

    }
    return default!;
}

internal static bool atComma(this ж<parser> Ꮡp, @string context, token.Token follow) {
    ref var p = ref Ꮡp.Value;

    if (p.tok == token.COMMA) {
        return true;
    }
    if (p.tok != follow) {
        @string msg = "missing ','"u8;
        if (p.tok == token.SEMICOLON && p.lit == "\n"u8) {
            msg += " before newline"u8;
        }
        Ꮡp.error(p.pos, msg + " in "u8 + context);
        return true;
    }
    // "insert" comma and continue
    return false;
}

internal static void assert(bool cond, @string msg) {
    if (!cond) {
        throw panic("go/parser internal error: " + msg);
    }
}

// advance consumes tokens until the current token p.tok
// is in the 'to' set, or token.EOF. For error recovery.
[GoRecv] internal static void advance(this ref parser p, map<token.Token, bool> to) {
    for (; p.tok != token.EOF; p.next()) {
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
internal static map<token.Token, bool> stmtStart = new map<token.Token, bool>{
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

internal static map<token.Token, bool> declStart = new map<token.Token, bool>{
    [token.IMPORT] = true,
    [token.CONST] = true,
    [token.TYPE] = true,
    [token.VAR] = true
};

internal static map<token.Token, bool> exprEnd = new map<token.Token, bool>{
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
internal static tokenꓸPos /*res*/ safePos(this ж<parser> Ꮡp, tokenꓸPos pos) {
    tokenꓸPos res = default!;
    func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

        defer(() => {
            if (recover() != default!) {
                res = ((tokenꓸPos)(Ꮡp.Value.@file.Base() + Ꮡp.Value.@file.Size()));
            }
        });
        // EOF position
        _ = p.@file.Offset(pos);
        // trigger a panic if position is out-of-range
        res = pos;
    });
    return res;
}

// ----------------------------------------------------------------------------
// Identifiers
internal static ж<ast.Ident> parseIdent(this ж<parser> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = p.pos;
    ref var name = ref heap<@string>(out var Ꮡname);
    name = "_"u8;
    if (p.tok == token.IDENT){
        name = p.lit;
        p.next();
    } else {
        Ꮡp.expect(token.IDENT);
    }
    // use expect() error handling
    return Ꮡ(new ast.Ident(NamePos: pos, Name: name));
}

internal static slice<ж<ast.Ident>> /*list*/ parseIdentList(this ж<parser> Ꮡp) {
    slice<ж<ast.Ident>> list = default!;
    func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

        if (p.trace) {
            deferǃ(un, trace(Ꮡp, "IdentList"u8), defer);
        }
        list = append(list, Ꮡp.parseIdent());
        while (p.tok == token.COMMA) {
            p.next();
            list = append(list, Ꮡp.parseIdent());
        }
    });
    return list;
}

// ----------------------------------------------------------------------------
// Common productions

// If lhs is set, result list elements which are identifiers are not resolved.
internal static slice<ast.Expr> /*list*/ parseExprList(this ж<parser> Ꮡp) {
    slice<ast.Expr> list = default!;
    func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

        if (p.trace) {
            deferǃ(un, trace(Ꮡp, "ExpressionList"u8), defer);
        }
        list = append(list, Ꮡp.parseExpr());
        while (p.tok == token.COMMA) {
            p.next();
            list = append(list, Ꮡp.parseExpr());
        }
    });
    return list;
}

internal static slice<ast.Expr> parseList(this ж<parser> Ꮡp, bool inRhs) {
    ref var p = ref Ꮡp.Value;

    var old = p.inRhs;
    p.inRhs = inRhs;
    var list = Ꮡp.parseExprList();
    p.inRhs = old;
    return list;
}

// ----------------------------------------------------------------------------
// Types
internal static ast.Expr parseType(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "Type"u8), defer);
    }
    var typ = Ꮡp.tryIdentOrType();
    if (typ == default!) {
        ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
        pos = p.pos;
        Ꮡp.errorExpected(pos, "type"u8);
        p.advance(exprEnd);
        return new ast_BadExprжExpr(Ꮡ(new ast.BadExpr(From: pos, To: p.pos)));
    }
    return typ;
});

internal static ast.Expr parseQualifiedIdent(this ж<parser> Ꮡp, ж<ast.Ident> Ꮡident) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "QualifiedIdent"u8), defer);
    }
    var typ = Ꮡp.parseTypeName(Ꮡident);
    if (p.tok == token.LBRACK) {
        typ = Ꮡp.parseTypeInstance(typ);
    }
    return typ;
});

// If the result is an identifier, it is not resolved.
internal static ast.Expr parseTypeName(this ж<parser> Ꮡp, ж<ast.Ident> Ꮡident) => func<ast.Expr>((defer, recover) => {
    ref var p = ref Ꮡp.Value;
    ref var ident = ref Ꮡident.DerefOrNil();

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "TypeName"u8), defer);
    }
    if (Ꮡident == nil) {
        Ꮡident = Ꮡp.parseIdent(); ident = ref Ꮡident.DerefOrNil();
    }
    if (p.tok == token.PERIOD) {
        // ident is a package name
        p.next();
        var sel = Ꮡp.parseIdent();
        return new ast_SelectorExprжExpr(Ꮡ(new ast.SelectorExpr(X: new ast_IdentжExpr(Ꮡident), Sel: sel)));
    }
    return new ast_IdentжExpr(Ꮡident);
});

// "[" has already been consumed, and lbrack is its position.
// If len != nil it is the already consumed array length.
internal static ж<ast.ArrayType> parseArrayType(this ж<parser> Ꮡp, tokenꓸPos lbrack, ast.Expr len) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "ArrayType"u8), defer);
    }
    if (len == default!) {
        p.exprLev++;
        // always permit ellipsis for more fault-tolerant parsing
        if (p.tok == token.ELLIPSIS){
            len = new ast_EllipsisжExpr(Ꮡ(new ast.Ellipsis(ΔEllipsis: p.pos)));
            p.next();
        } else 
        if (p.tok != token.RBRACK) {
            len = Ꮡp.parseRhs();
        }
        p.exprLev--;
    }
    if (p.tok == token.COMMA) {
        // Trailing commas are accepted in type parameter
        // lists but not in array type declarations.
        // Accept for better error handling but complain.
        Ꮡp.error(p.pos, "unexpected comma; expecting ]"u8);
        p.next();
    }
    Ꮡp.expect(token.RBRACK);
    var elt = Ꮡp.parseType();
    return Ꮡ(new ast.ArrayType(Lbrack: lbrack, Len: len, Elt: elt));
});

internal static (ж<ast.Ident>, ast.Expr) parseArrayFieldOrTypeInstance(this ж<parser> Ꮡp, ж<ast.Ident> Ꮡx) => func<(ж<ast.Ident>, ast.Expr)>((defer, recover) => {
    ref var p = ref Ꮡp.Value;
    ref var x = ref Ꮡx.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "ArrayFieldOrTypeInstance"u8), defer);
    }
    ref var lbrack = ref heap<tokenꓸPos>(out var Ꮡlbrack);
    lbrack = Ꮡp.expect(token.LBRACK);
    tokenꓸPos trailingComma = token.NoPos;
    // if valid, the position of a trailing comma preceding the ']'
    slice<ast.Expr> args = default!;
    if (p.tok != token.RBRACK) {
        p.exprLev++;
        args = append(args, Ꮡp.parseRhs());
        while (p.tok == token.COMMA) {
            tokenꓸPos comma = p.pos;
            p.next();
            if (p.tok == token.RBRACK) {
                trailingComma = comma;
                break;
            }
            args = append(args, Ꮡp.parseRhs());
        }
        p.exprLev--;
    }
    tokenꓸPos rbrack = Ꮡp.expect(token.RBRACK);
    if (len(args) == 0) {
        // x []E
        var elt = Ꮡp.parseType();
        return (Ꮡx, new ast_ArrayTypeжExpr(Ꮡ(new ast.ArrayType(Lbrack: lbrack, Elt: elt))));
    }
    // x [P]E or x[P]
    if (len(args) == 1) {
        var elt = Ꮡp.tryIdentOrType();
        if (elt != default!) {
            // x [P]E
            if (trailingComma.IsValid()) {
                // Trailing commas are invalid in array type fields.
                Ꮡp.error(trailingComma, "unexpected comma; expecting ]"u8);
            }
            return (Ꮡx, new ast_ArrayTypeжExpr(Ꮡ(new ast.ArrayType(Lbrack: lbrack, Len: args[0], Elt: elt))));
        }
    }
    // x[P], x[P1, P2], ...
    return (default!, typeparams.PackIndexExpr(new ast_IdentжExpr(Ꮡx), lbrack, args, rbrack));
});

internal static ж<ast.Field> parseFieldDecl(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "FieldDecl"u8), defer);
    }
    var doc = p.leadComment;
    slice<ж<ast.Ident>> names = default!;
    ast.Expr typ = default!;
    var exprᴛ1 = p.tok;
    if (exprᴛ1 == token.IDENT) {
        var name = Ꮡp.parseIdent();
        if (p.tok == token.PERIOD || p.tok == token.STRING || p.tok == token.SEMICOLON || p.tok == token.RBRACE){
            // embedded type
            typ = new ast_IdentжExpr(name);
            if (p.tok == token.PERIOD) {
                typ = Ꮡp.parseQualifiedIdent(name);
            }
        } else {
            // name1, name2, ... T
            names = new ж<ast.Ident>[]{name}.slice();
            while (p.tok == token.COMMA) {
                p.next();
                names = append(names, Ꮡp.parseIdent());
            }
            // Careful dance: We don't know if we have an embedded instantiated
            // type T[P1, P2, ...] or a field T of array type []E or [P]E.
            if (len(names) == 1 && p.tok == token.LBRACK){
                (name, typ) = Ꮡp.parseArrayFieldOrTypeInstance(name);
                if (name == nil) {
                    names = default!;
                }
            } else {
                // T P
                typ = Ꮡp.parseType();
            }
        }
    }
    else if (exprᴛ1 == token.MUL) {
        ref var star = ref heap<tokenꓸPos>(out var Ꮡstar);
        star = p.pos;
        p.next();
        if (p.tok == token.LPAREN){
            // *(T)
            Ꮡp.error(p.pos, "cannot parenthesize embedded type"u8);
            p.next();
            typ = Ꮡp.parseQualifiedIdent(nil);
            // expect closing ')' but no need to complain if missing
            if (p.tok == token.RPAREN) {
                p.next();
            }
        } else {
            // *T
            typ = Ꮡp.parseQualifiedIdent(nil);
        }
        typ = new ast_StarExprжExpr(Ꮡ(new ast.StarExpr(Star: star, X: typ)));
    }
    else if (exprᴛ1 == token.LPAREN) {
        Ꮡp.error(p.pos, "cannot parenthesize embedded type"u8);
        p.next();
        if (p.tok == token.MUL){
            // (*T)
            ref var star = ref heap<tokenꓸPos>(out var Ꮡstar);
            star = p.pos;
            p.next();
            typ = new ast_StarExprжExpr(Ꮡ(new ast.StarExpr(Star: star, X: Ꮡp.parseQualifiedIdent(nil))));
        } else {
            // (T)
            typ = Ꮡp.parseQualifiedIdent(nil);
        }
        if (p.tok == token.RPAREN) {
            // expect closing ')' but no need to complain if missing
            p.next();
        }
    }
    else { /* default: */
        ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
        pos = p.pos;
        Ꮡp.errorExpected(pos, "field name or embedded type"u8);
        p.advance(exprEnd);
        typ = new ast_BadExprжExpr(Ꮡ(new ast.BadExpr(From: pos, To: p.pos)));
    }

    ж<ast.BasicLit> tag = default!;
    if (p.tok == token.STRING) {
        tag = Ꮡ(new ast.BasicLit(ValuePos: p.pos, Kind: p.tok, Value: p.lit));
        p.next();
    }
    var comment = Ꮡp.expectSemi();
    var field = Ꮡ(new ast.Field(Doc: doc, Names: names, Type: typ, Tag: tag, Comment: comment));
    return field;
});

internal static ж<ast.StructType> parseStructType(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "StructType"u8), defer);
    }
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = Ꮡp.expect(token.STRUCT);
    ref var lbrace = ref heap<tokenꓸPos>(out var Ꮡlbrace);
    lbrace = Ꮡp.expect(token.LBRACE);
    slice<ж<ast.Field>> list = default!;
    while (p.tok == token.IDENT || p.tok == token.MUL || p.tok == token.LPAREN) {
        // a field declaration cannot start with a '(' but we accept
        // it here for more robust parsing and better error messages
        // (parseFieldDecl will check and complain if necessary)
        list = append(list, Ꮡp.parseFieldDecl());
    }
    ref var rbrace = ref heap<tokenꓸPos>(out var Ꮡrbrace);
    rbrace = Ꮡp.expect(token.RBRACE);
    return Ꮡ(new ast.StructType(
        Struct: pos,
        Fields: Ꮡ(new ast.FieldList(
            Opening: lbrace,
            List: list,
            Closing: rbrace
        ))
    ));
});

internal static ж<ast.StarExpr> parsePointerType(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "PointerType"u8), defer);
    }
    ref var star = ref heap<tokenꓸPos>(out var Ꮡstar);
    star = Ꮡp.expect(token.MUL);
    var @base = Ꮡp.parseType();
    return Ꮡ(new ast.StarExpr(Star: star, X: @base));
});

internal static ж<ast.Ellipsis> parseDotsType(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "DotsType"u8), defer);
    }
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = Ꮡp.expect(token.ELLIPSIS);
    var elt = Ꮡp.parseType();
    return Ꮡ(new ast.Ellipsis(ΔEllipsis: pos, Elt: elt));
});

[GoType] partial struct field {
    internal ж<ast.Ident> name;
    internal ast.Expr typ;
}

internal static field /*f*/ parseParamDecl(this ж<parser> Ꮡp, ж<ast.Ident> Ꮡname, bool typeSetsOK) {
    field f = default!;
    func((defer, recover) => {
    ref var p = ref Ꮡp.Value;
    ref var name = ref Ꮡname.DerefOrNil();

        // TODO(rFindley) refactor to be more similar to paramDeclOrNil in the syntax
        // package
        if (p.trace) {
            deferǃ(un, trace(Ꮡp, "ParamDeclOrNil"u8), defer);
        }
        token.Token ptok = p.tok;
        if (Ꮡname != nil){
            p.tok = token.IDENT;
        } else 
        if (typeSetsOK && p.tok == token.TILDE) {
            // force token.IDENT case in switch below
            // "~" ...
            f = new field(nil, Ꮡp.embeddedElem(default!)); return;
        }
        var exprᴛ1 = p.tok;
        if (exprᴛ1 == token.IDENT) {
            if (Ꮡname != nil){
                // name
                f.name = Ꮡname;
                p.tok = ptok;
            } else {
                f.name = Ꮡp.parseIdent();
            }
            var exprᴛ2 = p.tok;
            if (exprᴛ2 == token.IDENT || exprᴛ2 == token.MUL || exprᴛ2 == token.ARROW || exprᴛ2 == token.FUNC || exprᴛ2 == token.CHAN || exprᴛ2 == token.MAP || exprᴛ2 == token.STRUCT || exprᴛ2 == token.INTERFACE || exprᴛ2 == token.LPAREN) {
                f.typ = Ꮡp.parseType();
            }
            else if (exprᴛ2 == token.LBRACK) {
                (f.name, f.typ) = Ꮡp.parseArrayFieldOrTypeInstance(f.name);
            }
            else if (exprᴛ2 == token.ELLIPSIS) {
                f.typ = new ast_EllipsisжExpr(Ꮡp.parseDotsType());
                return;
            }
            if (exprᴛ2 == token.PERIOD) {
                f.typ = Ꮡp.parseQualifiedIdent(f.name);
                f.name = default!;
            }
            else if (exprᴛ2 == token.TILDE) {
                if (typeSetsOK) {
                    // name type
                    // name "[" type1, ..., typeN "]" or name "[" n "]" type
                    // name "..." type
                    // don't allow ...type "|" ...
                    // name "." ...
                    f.typ = Ꮡp.embeddedElem(default!);
                    return;
                }
            }
            if (exprᴛ2 == token.OR) {
                if (typeSetsOK) {
                    // name "|" typeset
                    f.typ = Ꮡp.embeddedElem(new ast_IdentжExpr(f.name));
                    f.name = default!;
                    return;
                }
            }

        }
        if (exprᴛ1 == token.MUL || exprᴛ1 == token.ARROW || exprᴛ1 == token.FUNC || exprᴛ1 == token.LBRACK || exprᴛ1 == token.CHAN || exprᴛ1 == token.MAP || exprᴛ1 == token.STRUCT || exprᴛ1 == token.INTERFACE || exprᴛ1 == token.LPAREN) {
            f.typ = Ꮡp.parseType();
        }
        else if (exprᴛ1 == token.ELLIPSIS) {
            f.typ = new ast_EllipsisжExpr(Ꮡp.parseDotsType());
            return;
        }
        { /* default: */
            Ꮡp.errorExpected(p.pos, // type
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
            f.typ = Ꮡp.embeddedElem(f.typ);
        }
    });
    return f;
}

internal static slice<ж<ast.Field>> /*params*/ parseParameterList(this ж<parser> Ꮡp, ж<ast.Ident> Ꮡname0, ast.Expr typ0, token.Token closing) {
    slice<ж<ast.Field>> @params = default!;
    func((defer, recover) => {
    ref var p = ref Ꮡp.Value;
    ref var name0 = ref Ꮡname0.DerefOrNil();

        if (p.trace) {
            deferǃ(un, trace(Ꮡp, "ParameterList"u8), defer);
        }
        // Type parameters are the only parameter list closed by ']'.
        var tparams = closing == token.RBRACK;
        tokenꓸPos pos0 = p.pos;
        if (Ꮡname0 != nil){
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
        while (Ꮡname0 != nil || p.tok != closing && p.tok != token.EOF) {
            field par = default!;
            if (typ0 != default!){
                if (tparams) {
                    typ0 = Ꮡp.embeddedElem(typ0);
                }
                par = new field(Ꮡname0, typ0);
            } else {
                par = Ꮡp.parseParamDecl(Ꮡname0, tparams);
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
            if (!Ꮡp.atComma("parameter list"u8, closing)) {
                break;
            }
            p.next();
        }
        if (len(list) == 0) {
            return;
        }
        // not uncommon
        // distribute parameter types (len(list) > 0)
        if (named == 0){
            // all unnamed => found names are type names
            for (nint i = 0; i < len(list); i++) {
                var par = Ꮡ(list, i);
                {
                    var typΔ1 = par.Value.name; if (typΔ1 != nil) {
                        par.Value.typ = new ast_IdentжExpr(typΔ1);
                        par.Value.name = default!;
                    }
                }
            }
            if (tparams) {
                // This is the same error handling as below, adjusted for type parameters only.
                // See comment below for details. (go.dev/issue/64534)
                tokenꓸPos errPos = default!;
                @string msg = default!;
                if (named == typed){
                    /* same as typed == 0 */
                    errPos = p.pos;
                    // position error at closing ]
                    msg = "missing type constraint"u8;
                } else {
                    errPos = pos0;
                    // position at opening [ or first name
                    msg = "missing type parameter name"u8;
                    if (len(list) == 1) {
                        msg += " or invalid array length"u8;
                    }
                }
                Ꮡp.error(errPos, msg);
            }
        } else 
        if (named != len(list)) {
            // some named or we're in a type parameter list => all must be named
            ref var errPos = ref heap(new tokenꓸPos(), out var ᏑerrPos);              // left-most error position (or invalid)
            ast.Expr typΔ2 = default!;                // current type (from right to left)
            for (nint i = len(list) - 1; i >= 0; i--) {
                {
                    var par = Ꮡ(list, i); if ((~par).typ != default!){
                        typΔ2 = par.Value.typ;
                        if ((~par).name == nil) {
                            errPos = typΔ2.Pos();
                            var n = ast.NewIdent("_"u8);
                            n.Value.NamePos = errPos;
                            // correct position
                            par.Value.name = n;
                        }
                    } else 
                    if (typΔ2 != default!){
                        par.Value.typ = typΔ2;
                    } else {
                        // par.typ == nil && typ == nil => we only have a par.name
                        errPos = (~par).name.Pos();
                        par.Value.typ = new ast_BadExprжExpr(Ꮡ(new ast.BadExpr(From: errPos, To: p.pos)));
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
                Ꮡp.error(errPos, msg);
            }
        }
        // Convert list to []*ast.Field.
        // If list contains types only, each type gets its own ast.Field.
        if (named == 0) {
            // parameter list consists of types only
            foreach (var (_, par) in list) {
                assert(par.typ != default!, "nil type in unnamed parameter list"u8);
                @params = append(@params, Ꮡ(new ast.Field(Type: par.typ)));
            }
            return;
        }
        // If the parameter list consists of named parameters with types,
        // collect all names with the same types into a single ast.Field.
        ref var names = ref heap<slice<ж<ast.Ident>>>(out var Ꮡnames);
        ref var typ = ref heap<ast.Expr>(out var Ꮡtyp);
        var addParams = () => {
            assert(Ꮡtyp.ValueSlot != default!, "nil type in named parameter list"u8);
            var field = Ꮡ(new ast.Field(Names: Ꮡnames.ValueSlot, Type: Ꮡtyp.ValueSlot));
            @params = append(@params, field);
            Ꮡnames.ValueSlot = default!;
        };
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
    });
    return @params;
}

internal static (ж<ast.FieldList> tparams, ж<ast.FieldList> @params) parseParameters(this ж<parser> Ꮡp, bool acceptTParams) {
    ж<ast.FieldList> tparams = default!;
    ж<ast.FieldList> @params = default!;
    func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

        if (p.trace) {
            deferǃ(un, trace(Ꮡp, "Parameters"u8), defer);
        }
        if (acceptTParams && p.tok == token.LBRACK) {
            ref var openingΔ1 = ref heap<tokenꓸPos>(out var ᏑopeningΔ1);
            openingΔ1 = p.pos;
            p.next();
            // [T any](params) syntax
            var list = Ꮡp.parseParameterList(nil, default!, token.RBRACK);
            ref var rbrack = ref heap<tokenꓸPos>(out var Ꮡrbrack);
            rbrack = Ꮡp.expect(token.RBRACK);
            tparams = Ꮡ(new ast.FieldList(Opening: openingΔ1, List: list, Closing: rbrack));
            // Type parameter lists must not be empty.
            if (tparams.NumFields() == 0) {
                Ꮡp.error((~tparams).Closing, "empty type parameter list"u8);
                tparams = default!;
            }
        }
        // avoid follow-on errors
        ref var opening = ref heap<tokenꓸPos>(out var Ꮡopening);
        opening = Ꮡp.expect(token.LPAREN);
        slice<ж<ast.Field>> fields = default!;
        if (p.tok != token.RPAREN) {
            fields = Ꮡp.parseParameterList(nil, default!, token.RPAREN);
        }
        ref var rparen = ref heap<tokenꓸPos>(out var Ꮡrparen);
        rparen = Ꮡp.expect(token.RPAREN);
        @params = Ꮡ(new ast.FieldList(Opening: opening, List: fields, Closing: rparen));
    });
    return (tparams, @params);
}

internal static ж<ast.FieldList> parseResult(this ж<parser> Ꮡp) => func<ж<ast.FieldList>>((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "Result"u8), defer);
    }
    if (p.tok == token.LPAREN) {
        var (_, results) = Ꮡp.parseParameters(false);
        return results;
    }
    var typ = Ꮡp.tryIdentOrType();
    if (typ != default!) {
        var list = new slice<ж<ast.Field>>(1);
        list[0] = Ꮡ(new ast.Field(Type: typ));
        return Ꮡ(new ast.FieldList(List: list));
    }
    return default!;
});

internal static ж<ast.FuncType> parseFuncType(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "FuncType"u8), defer);
    }
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = Ꮡp.expect(token.FUNC);
    var (tparams, @params) = Ꮡp.parseParameters(true);
    if (tparams != nil) {
        Ꮡp.error(tparams.Pos(), "function type must have no type parameters"u8);
    }
    var results = Ꮡp.parseResult();
    return Ꮡ(new ast.FuncType(Func: pos, Params: @params, Results: results));
});

internal static ж<ast.Field> parseMethodSpec(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "MethodSpec"u8), defer);
    }
    var doc = p.leadComment;
    slice<ж<ast.Ident>> idents = default!;
    ast.Expr typ = default!;
    var x = Ꮡp.parseTypeName(nil);
    {
        var (ident, _) = x._<ж<ast.Ident>>(ᐧ); if (ident != nil){
            switch (ᐧ) {
            case {} when p.tok == token.LBRACK: {
                tokenꓸPos lbrack = p.pos;
                p.next();
                p.exprLev++;
                var xΔ2 = Ꮡp.parseExpr();
                p.exprLev--;
                {
                    var (name0, _) = xΔ2._<ж<ast.Ident>>(ᐧ); if (name0 != nil && p.tok != token.COMMA && p.tok != token.RBRACK){
                        // generic method or embedded instantiated type
                        // generic method m[T any]
                        //
                        // Interface methods do not have type parameters. We parse them for a
                        // better error message and improved error recovery.
                        _ = Ꮡp.parseParameterList(name0, default!, token.RBRACK);
                        _ = Ꮡp.expect(token.RBRACK);
                        Ꮡp.error(lbrack, "interface method must have no type parameters"u8);
                        // TODO(rfindley) refactor to share code with parseFuncType.
                        var (_, @params) = Ꮡp.parseParameters(false);
                        var results = Ꮡp.parseResult();
                        idents = new ж<ast.Ident>[]{ident}.slice();
                        typ = new ast_FuncTypeжExpr(Ꮡ(new ast.FuncType(
                            Func: token.NoPos,
                            Params: @params,
                            Results: results
                        )));
                    } else {
                        // embedded instantiated type
                        // TODO(rfindley) should resolve all identifiers in x.
                        var list = new ast.Expr[]{xΔ2}.slice();
                        if (Ꮡp.atComma("type argument list"u8, token.RBRACK)) {
                            p.exprLev++;
                            p.next();
                            while (p.tok != token.RBRACK && p.tok != token.EOF) {
                                list = append(list, Ꮡp.parseType());
                                if (!Ꮡp.atComma("type argument list"u8, token.RBRACK)) {
                                    break;
                                }
                                p.next();
                            }
                            p.exprLev--;
                        }
                        tokenꓸPos rbrack = Ꮡp.expectClosing(token.RBRACK, "type argument list"u8);
                        typ = typeparams.PackIndexExpr(new ast_IdentжExpr(ident), lbrack, list, rbrack);
                    }
                }
                break;
            }
            case {} when p.tok == token.LPAREN: {
                var (_, @params) = Ꮡp.parseParameters(false);
                var results = Ꮡp.parseResult();
                idents = new ж<ast.Ident>[]{ // ordinary method
 // TODO(rfindley) refactor to share code with parseFuncType.
ident}.slice();
                typ = new ast_FuncTypeжExpr(Ꮡ(new ast.FuncType(Func: token.NoPos, Params: @params, Results: results)));
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
                typ = Ꮡp.parseTypeInstance(typ);
            }
        }
    }
    // Comment is added at the callsite: the field below may joined with
    // additional type specs using '|'.
    // TODO(rfindley) this should be refactored.
    // TODO(rfindley) add more tests for comment handling.
    return Ꮡ(new ast.Field(Doc: doc, Names: idents, Type: typ));
});

internal static ast.Expr embeddedElem(this ж<parser> Ꮡp, ast.Expr x) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "EmbeddedElem"u8), defer);
    }
    if (x == default!) {
        x = Ꮡp.embeddedTerm();
    }
    while (p.tok == token.OR) {
        var t = @new<ast.BinaryExpr>();
        t.Value.OpPos = p.pos;
        t.Value.Op = token.OR;
        p.next();
        t.Value.X = x;
        t.Value.Y = Ꮡp.embeddedTerm();
        x = new ast_BinaryExprжExpr(t);
    }
    return x;
});

internal static ast.Expr embeddedTerm(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "EmbeddedTerm"u8), defer);
    }
    if (p.tok == token.TILDE) {
        var tΔ1 = @new<ast.UnaryExpr>();
        tΔ1.Value.OpPos = p.pos;
        tΔ1.Value.Op = token.TILDE;
        p.next();
        tΔ1.Value.X = Ꮡp.parseType();
        return new ast_UnaryExprжExpr(tΔ1);
    }
    var t = Ꮡp.tryIdentOrType();
    if (t == default!) {
        ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
        pos = p.pos;
        Ꮡp.errorExpected(pos, "~ term or type"u8);
        p.advance(exprEnd);
        return new ast_BadExprжExpr(Ꮡ(new ast.BadExpr(From: pos, To: p.pos)));
    }
    return t;
});

internal static ж<ast.InterfaceType> parseInterfaceType(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "InterfaceType"u8), defer);
    }
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = Ꮡp.expect(token.INTERFACE);
    ref var lbrace = ref heap<tokenꓸPos>(out var Ꮡlbrace);
    lbrace = Ꮡp.expect(token.LBRACE);
    slice<ж<ast.Field>> list = default!;
parseElements:
    while (ᐧ) {
        switch (ᐧ) {
        case {} when p.tok == token.IDENT: {
            var f = Ꮡp.parseMethodSpec();
            if ((~f).Names == default!) {
                f.Value.Type = Ꮡp.embeddedElem((~f).Type);
            }
            f.Value.Comment = Ꮡp.expectSemi();
            list = append(list, f);
            break;
        }
        case {} when p.tok == token.TILDE: {
            var typ = Ꮡp.embeddedElem(default!);
            var comment = Ꮡp.expectSemi();
            list = append(list, Ꮡ(new ast.Field(Type: typ, Comment: comment)));
            break;
        }
        default: {
            {
                var t = Ꮡp.tryIdentOrType(); if (t != default!){
                    var typ = Ꮡp.embeddedElem(t);
                    var comment = Ꮡp.expectSemi();
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
    ref var rbrace = ref heap<tokenꓸPos>(out var Ꮡrbrace);
    rbrace = Ꮡp.expect(token.RBRACE);
    return Ꮡ(new ast.InterfaceType(
        Interface: pos,
        Methods: Ꮡ(new ast.FieldList(
            Opening: lbrace,
            List: list,
            Closing: rbrace
        ))
    ));
});

internal static ж<ast.MapType> parseMapType(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "MapType"u8), defer);
    }
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = Ꮡp.expect(token.MAP);
    Ꮡp.expect(token.LBRACK);
    var key = Ꮡp.parseType();
    Ꮡp.expect(token.RBRACK);
    var value = Ꮡp.parseType();
    return Ꮡ(new ast.MapType(Map: pos, Key: key, Value: value));
});

internal static ж<ast.ChanType> parseChanType(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "ChanType"u8), defer);
    }
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = p.pos;
    ref var dir = ref heap<ast.ChanDir>(out var Ꮡdir);
    dir = (ast.ChanDir)(ast.SEND | ast.RECV);
    ref var arrow = ref heap(new tokenꓸPos(), out var Ꮡarrow);
    if (p.tok == token.CHAN){
        p.next();
        if (p.tok == token.ARROW) {
            arrow = p.pos;
            p.next();
            dir = ast.SEND;
        }
    } else {
        arrow = Ꮡp.expect(token.ARROW);
        Ꮡp.expect(token.CHAN);
        dir = ast.RECV;
    }
    var value = Ꮡp.parseType();
    return Ꮡ(new ast.ChanType(Begin: pos, Arrow: arrow, Dir: dir, Value: value));
});

internal static ast.Expr parseTypeInstance(this ж<parser> Ꮡp, ast.Expr typ) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "TypeInstance"u8), defer);
    }
    ref var opening = ref heap<tokenꓸPos>(out var Ꮡopening);
    opening = Ꮡp.expect(token.LBRACK);
    p.exprLev++;
    slice<ast.Expr> list = default!;
    while (p.tok != token.RBRACK && p.tok != token.EOF) {
        list = append(list, Ꮡp.parseType());
        if (!Ꮡp.atComma("type argument list"u8, token.RBRACK)) {
            break;
        }
        p.next();
    }
    p.exprLev--;
    ref var closing = ref heap<tokenꓸPos>(out var Ꮡclosing);
    closing = Ꮡp.expectClosing(token.RBRACK, "type argument list"u8);
    if (len(list) == 0) {
        Ꮡp.errorExpected(closing, "type argument list"u8);
        return new ast_IndexExprжExpr(Ꮡ(new ast.IndexExpr(
            X: typ,
            Lbrack: opening,
            Index: new ast_BadExprжExpr(Ꮡ(new ast.BadExpr(From: opening + 1, To: closing))),
            Rbrack: closing
        )));
    }
    return typeparams.PackIndexExpr(typ, opening, list, closing);
});

internal static ast.Expr tryIdentOrType(this ж<parser> Ꮡp) => func<ast.Expr>((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    deferǃ(decNestLev, incNestLev(Ꮡp), defer);
    var exprᴛ1 = p.tok;
    if (exprᴛ1 == token.IDENT) {
        var typ = Ꮡp.parseTypeName(nil);
        if (p.tok == token.LBRACK) {
            typ = Ꮡp.parseTypeInstance(typ);
        }
        return typ;
    }
    if (exprᴛ1 == token.LBRACK) {
        tokenꓸPos lbrack = Ꮡp.expect(token.LBRACK);
        return new ast_ArrayTypeжExpr(Ꮡp.parseArrayType(lbrack, default!));
    }
    if (exprᴛ1 == token.STRUCT) {
        return new ast_StructTypeжExpr(Ꮡp.parseStructType());
    }
    if (exprᴛ1 == token.MUL) {
        return new ast_StarExprжExpr(Ꮡp.parsePointerType());
    }
    if (exprᴛ1 == token.FUNC) {
        return new ast_FuncTypeжExpr(Ꮡp.parseFuncType());
    }
    if (exprᴛ1 == token.INTERFACE) {
        return new ast_InterfaceTypeжExpr(Ꮡp.parseInterfaceType());
    }
    if (exprᴛ1 == token.MAP) {
        return new ast_MapTypeжExpr(Ꮡp.parseMapType());
    }
    if (exprᴛ1 == token.CHAN || exprᴛ1 == token.ARROW) {
        return new ast_ChanTypeжExpr(Ꮡp.parseChanType());
    }
    if (exprᴛ1 == token.LPAREN) {
        ref var lparen = ref heap<tokenꓸPos>(out var Ꮡlparen);
        lparen = p.pos;
        p.next();
        var typ = Ꮡp.parseType();
        ref var rparen = ref heap<tokenꓸPos>(out var Ꮡrparen);
        rparen = Ꮡp.expect(token.RPAREN);
        return new ast_ParenExprжExpr(Ꮡ(new ast.ParenExpr(Lparen: lparen, X: typ, Rparen: rparen)));
    }

    // no type found
    return default!;
});

// ----------------------------------------------------------------------------
// Blocks
internal static slice<ast.Stmt> /*list*/ parseStmtList(this ж<parser> Ꮡp) {
    slice<ast.Stmt> list = default!;
    func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

        if (p.trace) {
            deferǃ(un, trace(Ꮡp, "StatementList"u8), defer);
        }
        while (p.tok != token.CASE && p.tok != token.DEFAULT && p.tok != token.RBRACE && p.tok != token.EOF) {
            list = append(list, Ꮡp.parseStmt());
        }
    });
    return list;
}

internal static ж<ast.BlockStmt> parseBody(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "Body"u8), defer);
    }
    ref var lbrace = ref heap<tokenꓸPos>(out var Ꮡlbrace);
    lbrace = Ꮡp.expect(token.LBRACE);
    var list = Ꮡp.parseStmtList();
    ref var rbrace = ref heap<tokenꓸPos>(out var Ꮡrbrace);
    rbrace = Ꮡp.expect2(token.RBRACE);
    return Ꮡ(new ast.BlockStmt(Lbrace: lbrace, List: list, Rbrace: rbrace));
});

internal static ж<ast.BlockStmt> parseBlockStmt(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "BlockStmt"u8), defer);
    }
    ref var lbrace = ref heap<tokenꓸPos>(out var Ꮡlbrace);
    lbrace = Ꮡp.expect(token.LBRACE);
    var list = Ꮡp.parseStmtList();
    ref var rbrace = ref heap<tokenꓸPos>(out var Ꮡrbrace);
    rbrace = Ꮡp.expect2(token.RBRACE);
    return Ꮡ(new ast.BlockStmt(Lbrace: lbrace, List: list, Rbrace: rbrace));
});

// ----------------------------------------------------------------------------
// Expressions
internal static ast.Expr parseFuncTypeOrLit(this ж<parser> Ꮡp) => func<ast.Expr>((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "FuncTypeOrLit"u8), defer);
    }
    var typ = Ꮡp.parseFuncType();
    if (p.tok != token.LBRACE) {
        // function type only
        return new ast_FuncTypeжExpr(typ);
    }
    p.exprLev++;
    var body = Ꮡp.parseBody();
    p.exprLev--;
    return new ast_FuncLitжExpr(Ꮡ(new ast.FuncLit(Type: typ, Body: body)));
});

// parseOperand may return an expression or a raw type (incl. array
// types of the form [...]T). Callers must verify the result.
internal static ast.Expr parseOperand(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "Operand"u8), defer);
    }
    var exprᴛ1 = p.tok;
    if (exprᴛ1 == token.IDENT) {
        var x = Ꮡp.parseIdent();
        return new ast_IdentжExpr(x);
    }
    if (exprᴛ1 == token.INT || exprᴛ1 == token.FLOAT || exprᴛ1 == token.IMAG || exprᴛ1 == token.CHAR || exprᴛ1 == token.STRING) {
        var x = Ꮡ(new ast.BasicLit(ValuePos: p.pos, Kind: p.tok, Value: p.lit));
        p.next();
        return new ast_BasicLitжExpr(x);
    }
    if (exprᴛ1 == token.LPAREN) {
        ref var lparen = ref heap<tokenꓸPos>(out var Ꮡlparen);
        lparen = p.pos;
        p.next();
        p.exprLev++;
        var x = Ꮡp.parseRhs();
        p.exprLev--;
        ref var rparen = ref heap<tokenꓸPos>(out var Ꮡrparen);
        rparen = Ꮡp.expect(token.RPAREN);
        return new ast_ParenExprжExpr(Ꮡ(new ast.ParenExpr( // types may be parenthesized: (some type)
Lparen: lparen, X: x, Rparen: rparen)));
    }
    if (exprᴛ1 == token.FUNC) {
        return Ꮡp.parseFuncTypeOrLit();
    }

    {
        var typ = Ꮡp.tryIdentOrType(); if (typ != default!) {
            // do not consume trailing type parameters
            // could be type for composite literal or conversion
            var (_, isIdent) = typ._<ж<ast.Ident>>(ᐧ);
            assert(!isIdent, "type cannot be identifier"u8);
            return typ;
        }
    }
    // we have an error
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = p.pos;
    Ꮡp.errorExpected(pos, "operand"u8);
    p.advance(stmtStart);
    return new ast_BadExprжExpr(Ꮡ(new ast.BadExpr(From: pos, To: p.pos)));
});

internal static ast.Expr parseSelector(this ж<parser> Ꮡp, ast.Expr x) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "Selector"u8), defer);
    }
    var sel = Ꮡp.parseIdent();
    return new ast_SelectorExprжExpr(Ꮡ(new ast.SelectorExpr(X: x, Sel: sel)));
});

internal static ast.Expr parseTypeAssertion(this ж<parser> Ꮡp, ast.Expr x) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "TypeAssertion"u8), defer);
    }
    ref var lparen = ref heap<tokenꓸPos>(out var Ꮡlparen);
    lparen = Ꮡp.expect(token.LPAREN);
    ast.Expr typ = default!;
    if (p.tok == token.TYPE){
        // type switch: typ == nil
        p.next();
    } else {
        typ = Ꮡp.parseType();
    }
    ref var rparen = ref heap<tokenꓸPos>(out var Ꮡrparen);
    rparen = Ꮡp.expect(token.RPAREN);
    return new ast_TypeAssertExprжExpr(Ꮡ(new ast.TypeAssertExpr(X: x, Type: typ, Lparen: lparen, Rparen: rparen)));
});

internal static ast.Expr parseIndexOrSliceOrInstance(this ж<parser> Ꮡp, ast.Expr x) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "parseIndexOrSliceOrInstance"u8), defer);
    }
    ref var lbrack = ref heap<tokenꓸPos>(out var Ꮡlbrack);
    lbrack = Ꮡp.expect(token.LBRACK);
    if (p.tok == token.RBRACK) {
        // empty index, slice or index expressions are not permitted;
        // accept them for parsing tolerance, but complain
        Ꮡp.errorExpected(p.pos, "operand"u8);
        ref var rbrackΔ1 = ref heap<tokenꓸPos>(out var ᏑrbrackΔ1);
        rbrackΔ1 = p.pos;
        p.next();
        return new ast_IndexExprжExpr(Ꮡ(new ast.IndexExpr(
            X: x,
            Lbrack: lbrack,
            Index: new ast_BadExprжExpr(Ꮡ(new ast.BadExpr(From: rbrackΔ1, To: rbrackΔ1))),
            Rbrack: rbrackΔ1
        )));
    }
    p.exprLev++;
    UntypedInt N = 3; // change the 3 to 2 to disable 3-index slices
    slice<ast.Expr> args = default!;
    array<ast.Expr> index = new(3); /* N */
    array<tokenꓸPos> colons = new(2); /* N - 1 */
    if (p.tok != token.COLON) {
        // We can't know if we have an index expression or a type instantiation;
        // so even if we see a (named) type we are not going to be in type context.
        index[0] = Ꮡp.parseRhs();
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
                index[ncolons] = Ꮡp.parseRhs();
            }
        }
    }
    else if (exprᴛ1 == token.COMMA) {
        args = append(args, // instance expression
 index[0]);
        while (p.tok == token.COMMA) {
            p.next();
            if (p.tok != token.RBRACK && p.tok != token.EOF) {
                args = append(args, Ꮡp.parseType());
            }
        }
    }

    p.exprLev--;
    ref var rbrack = ref heap<tokenꓸPos>(out var Ꮡrbrack);
    rbrack = Ꮡp.expect(token.RBRACK);
    if (ncolons > 0) {
        // slice expression
        ref var slice3 = ref heap<bool>(out var Ꮡslice3);
        slice3 = false;
        if (ncolons == 2) {
            slice3 = true;
            // Check presence of middle and final index here rather than during type-checking
            // to prevent erroneous programs from passing through gofmt (was go.dev/issue/7305).
            if (index[1] == default!) {
                Ꮡp.error(colons[0], "middle index required in 3-index slice"u8);
                index[1] = new ast_BadExprжExpr(Ꮡ(new ast.BadExpr(From: colons[0] + 1, To: colons[1])));
            }
            if (index[2] == default!) {
                Ꮡp.error(colons[1], "final index required in 3-index slice"u8);
                index[2] = new ast_BadExprжExpr(Ꮡ(new ast.BadExpr(From: colons[1] + 1, To: rbrack)));
            }
        }
        return new ast_SliceExprжExpr(Ꮡ(new ast.SliceExpr(X: x, Lbrack: lbrack, Low: index[0], High: index[1], Max: index[2], Slice3: slice3, Rbrack: rbrack)));
    }
    if (len(args) == 0) {
        // index expression
        return new ast_IndexExprжExpr(Ꮡ(new ast.IndexExpr(X: x, Lbrack: lbrack, Index: index[0], Rbrack: rbrack)));
    }
    // instance expression
    return typeparams.PackIndexExpr(x, lbrack, args, rbrack);
});

internal static ж<ast.CallExpr> parseCallOrConversion(this ж<parser> Ꮡp, ast.Expr fun) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "CallOrConversion"u8), defer);
    }
    ref var lparen = ref heap<tokenꓸPos>(out var Ꮡlparen);
    lparen = Ꮡp.expect(token.LPAREN);
    p.exprLev++;
    slice<ast.Expr> list = default!;
    ref var ellipsis = ref heap(new tokenꓸPos(), out var Ꮡellipsis);
    while (p.tok != token.RPAREN && p.tok != token.EOF && !ellipsis.IsValid()) {
        list = append(list, Ꮡp.parseRhs());
        // builtins may expect a type: make(some type, ...)
        if (p.tok == token.ELLIPSIS) {
            ellipsis = p.pos;
            p.next();
        }
        if (!Ꮡp.atComma("argument list"u8, token.RPAREN)) {
            break;
        }
        p.next();
    }
    p.exprLev--;
    ref var rparen = ref heap<tokenꓸPos>(out var Ꮡrparen);
    rparen = Ꮡp.expectClosing(token.RPAREN, "argument list"u8);
    return Ꮡ(new ast.CallExpr(Fun: fun, Lparen: lparen, Args: list, Ellipsis: ellipsis, Rparen: rparen));
});

internal static ast.Expr parseValue(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "Element"u8), defer);
    }
    if (p.tok == token.LBRACE) {
        return Ꮡp.parseLiteralValue(default!);
    }
    var x = Ꮡp.parseExpr();
    return x;
});

internal static ast.Expr parseElement(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "Element"u8), defer);
    }
    var x = Ꮡp.parseValue();
    if (p.tok == token.COLON) {
        ref var colon = ref heap<tokenꓸPos>(out var Ꮡcolon);
        colon = p.pos;
        p.next();
        x = new ast_KeyValueExprжExpr(Ꮡ(new ast.KeyValueExpr(Key: x, Colon: colon, Value: Ꮡp.parseValue())));
    }
    return x;
});

internal static slice<ast.Expr> /*list*/ parseElementList(this ж<parser> Ꮡp) {
    slice<ast.Expr> list = default!;
    func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

        if (p.trace) {
            deferǃ(un, trace(Ꮡp, "ElementList"u8), defer);
        }
        while (p.tok != token.RBRACE && p.tok != token.EOF) {
            list = append(list, Ꮡp.parseElement());
            if (!Ꮡp.atComma("composite literal"u8, token.RBRACE)) {
                break;
            }
            p.next();
        }
    });
    return list;
}

internal static ast.Expr parseLiteralValue(this ж<parser> Ꮡp, ast.Expr typ) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    deferǃ(decNestLev, incNestLev(Ꮡp), defer);
    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "LiteralValue"u8), defer);
    }
    ref var lbrace = ref heap<tokenꓸPos>(out var Ꮡlbrace);
    lbrace = Ꮡp.expect(token.LBRACE);
    slice<ast.Expr> elts = default!;
    p.exprLev++;
    if (p.tok != token.RBRACE) {
        elts = Ꮡp.parseElementList();
    }
    p.exprLev--;
    ref var rbrace = ref heap<tokenꓸPos>(out var Ꮡrbrace);
    rbrace = Ꮡp.expectClosing(token.RBRACE, "composite literal"u8);
    return new ast_CompositeLitжExpr(Ꮡ(new ast.CompositeLit(Type: typ, Lbrace: lbrace, Elts: elts, Rbrace: rbrace)));
});

internal static ast.Expr parsePrimaryExpr(this ж<parser> Ꮡp, ast.Expr x) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "PrimaryExpr"u8), defer);
    }
    if (x == default!) {
        x = Ꮡp.parseOperand();
    }
    // We track the nesting here rather than at the entry for the function,
    // since it can iteratively produce a nested output, and we want to
    // limit how deep a structure we generate.
    nint n = default!;
    defer(() => {
        Ꮡp.Value.nestLev -= n;
    });
    for (n = 1; ᐧ ; n++) {
        incNestLev(Ꮡp);
        var exprᴛ1 = p.tok;
        if (exprᴛ1 == token.PERIOD) {
            p.next();
            var exprᴛ2 = p.tok;
            if (exprᴛ2 == token.IDENT) {
                x = Ꮡp.parseSelector(x);
            }
            else if (exprᴛ2 == token.LPAREN) {
                x = Ꮡp.parseTypeAssertion(x);
            }
            else { /* default: */
                ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
                pos = p.pos;
                Ꮡp.errorExpected(pos, "selector or type assertion"u8);
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
                x = new ast_SelectorExprжExpr(Ꮡ(new ast.SelectorExpr(X: x, Sel: sel)));
            }

        }
        else if (exprᴛ1 == token.LBRACK) {
            x = Ꮡp.parseIndexOrSliceOrInstance(x);
        }
        else if (exprᴛ1 == token.LPAREN) {
            x = new ast_CallExprжExpr(Ꮡp.parseCallOrConversion(x));
        }
        else if (exprᴛ1 == token.LBRACE) {
            var t = ast.Unparen(x);
            switch (t.type()) {
            case ж<ast.BadExpr> _:
            case ж<ast.Ident> _:
            case ж<ast.SelectorExpr> _: {
                if (p.exprLev < 0) {
                    // operand may have returned a parenthesized complit
                    // type; accept it but complain if we have a complit
                    // determine if '{' belongs to a composite literal or a block statement
                    return x;
                }
                break;
            }
            case ж<ast.IndexExpr> _:
            case ж<ast.IndexListExpr> _: {
                if (p.exprLev < 0) {
                    // x is possibly a composite literal type
                    return x;
                }
                break;
            }
            case ж<ast.ArrayType> _:
            case ж<ast.StructType> _:
            case ж<ast.MapType> _: {
                break;
            }
            default: {
                return x;
            }}

            if (!AreEqual(t, x)) {
                // x is possibly a composite literal type
                // x is a composite literal type
                Ꮡp.error(t.Pos(), "cannot parenthesize type in composite literal"u8);
            }
            x = Ꮡp.parseLiteralValue(x);
        }
        else { /* default: */
            return x;
        }

    }
});

// already progressed, no need to advance
internal static ast.Expr parseUnaryExpr(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    deferǃ(decNestLev, incNestLev(Ꮡp), defer);
    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "UnaryExpr"u8), defer);
    }
    var exprᴛ1 = p.tok;
    if (exprᴛ1 == token.ADD || exprᴛ1 == token.SUB || exprᴛ1 == token.NOT || exprᴛ1 == token.XOR || exprᴛ1 == token.AND || exprᴛ1 == token.TILDE) {
        ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
        pos = p.pos;
        ref var op = ref heap<token.Token>(out var Ꮡop);
        op = p.tok;
        p.next();
        var x = Ꮡp.parseUnaryExpr();
        return new ast_UnaryExprжExpr(Ꮡ(new ast.UnaryExpr(OpPos: pos, Op: op, X: x)));
    }
    if (exprᴛ1 == token.ARROW) {
        ref var arrow = ref heap<tokenꓸPos>(out var Ꮡarrow);
        arrow = p.pos;
        p.next();
        var x = Ꮡp.parseUnaryExpr();
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
                        Ꮡp.errorExpected((~typ).Arrow, "'chan'"u8);
                    }
                    arrow = typ.Value.Arrow;
                    typ.Value.Begin = arrow;
                    typ.Value.Arrow = arrow;
                    dir = typ.Value.Dir;
                    typ.Value.Dir = ast.RECV;
                    (typ, ok) = (~typ).Value._<ж<ast.ChanType>>(ᐧ);
                }
                if (dir == ast.SEND) {
                    Ꮡp.errorExpected(arrow, "channel type"u8);
                }
                return x;
            }
        }
        return new ast_UnaryExprжExpr(Ꮡ(new ast.UnaryExpr( // <-(expr)
OpPos: arrow, Op: token.ARROW, X: x)));
    }
    if (exprᴛ1 == token.MUL) {
        ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
        pos = p.pos;
        p.next();
        var x = Ꮡp.parseUnaryExpr();
        return new ast_StarExprжExpr(Ꮡ(new ast.StarExpr( // pointer type or unary "*" expression
Star: pos, X: x)));
    }

    return Ꮡp.parsePrimaryExpr(default!);
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
internal static ast.Expr parseBinaryExpr(this ж<parser> Ꮡp, ast.Expr x, nint prec1) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "BinaryExpr"u8), defer);
    }
    if (x == default!) {
        x = Ꮡp.parseUnaryExpr();
    }
    // We track the nesting here rather than at the entry for the function,
    // since it can iteratively produce a nested output, and we want to
    // limit how deep a structure we generate.
    nint n = default!;
    defer(() => {
        Ꮡp.Value.nestLev -= n;
    });
    for (n = 1; ᐧ ; n++) {
        incNestLev(Ꮡp);
        ref var op = ref heap<token.Token>(out var Ꮡop);
        (op, var oprec) = p.tokPrec();
        if (oprec < prec1) {
            return x;
        }
        ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
        pos = Ꮡp.expect(op);
        var y = Ꮡp.parseBinaryExpr(default!, oprec + 1);
        x = new ast_BinaryExprжExpr(Ꮡ(new ast.BinaryExpr(X: x, OpPos: pos, Op: op, Y: y)));
    }
});

// The result may be a type or even a raw type ([...]int).
internal static ast.Expr parseExpr(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "Expression"u8), defer);
    }
    return Ꮡp.parseBinaryExpr(default!, token.LowestPrec + 1);
});

internal static ast.Expr parseRhs(this ж<parser> Ꮡp) {
    ref var p = ref Ꮡp.Value;

    var old = p.inRhs;
    p.inRhs = true;
    var x = Ꮡp.parseExpr();
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
internal static (ast.Stmt, bool) parseSimpleStmt(this ж<parser> Ꮡp, nint mode) => func<(ast.Stmt, bool)>((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "SimpleStmt"u8), defer);
    }
    var x = Ꮡp.parseList(false);
    var exprᴛ1 = p.tok;
    if (exprᴛ1 == token.DEFINE || exprᴛ1 == token.ASSIGN || exprᴛ1 == token.ADD_ASSIGN || exprᴛ1 == token.SUB_ASSIGN || exprᴛ1 == token.MUL_ASSIGN || exprᴛ1 == token.QUO_ASSIGN || exprᴛ1 == token.REM_ASSIGN || exprᴛ1 == token.AND_ASSIGN || exprᴛ1 == token.OR_ASSIGN || exprᴛ1 == token.XOR_ASSIGN || exprᴛ1 == token.SHL_ASSIGN || exprᴛ1 == token.SHR_ASSIGN || exprᴛ1 == token.AND_NOT_ASSIGN) {
        ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
        pos = p.pos;
        ref var tok = ref heap<token.Token>(out var Ꮡtok);
        tok = p.tok;
        p.next();
// assignment statement, possibly part of a range clause
        slice<ast.Expr> y = default!;
        var isRange = false;
        if (mode == rangeOk && p.tok == token.RANGE && (tok == token.DEFINE || tok == token.ASSIGN)){
            ref var posΔ1 = ref heap<tokenꓸPos>(out var ᏑposΔ1);
            posΔ1 = p.pos;
            p.next();
            y = new ast.Expr[]{new ast_UnaryExprжExpr(Ꮡ(new ast.UnaryExpr(OpPos: posΔ1, Op: token.RANGE, X: Ꮡp.parseRhs())))}.slice();
            isRange = true;
        } else {
            y = Ꮡp.parseList(true);
        }
        return (new ast_AssignStmtжStmt(Ꮡ(new ast.AssignStmt(Lhs: x, TokPos: pos, Tok: tok, Rhs: y))), isRange);
    }

    if (len(x) > 1) {
        Ꮡp.errorExpected(x[0].Pos(), "1 expression"u8);
    }
    // continue with first expression
    var exprᴛ2 = p.tok;
    if (exprᴛ2 == token.COLON) {
        ref var colon = ref heap<tokenꓸPos>(out var Ꮡcolon);
        colon = p.pos;
        p.next();
        {
            var (label, isIdent) = x[0]._<ж<ast.Ident>>(ᐧ); if (mode == labelOk && isIdent) {
                // labeled statement
                // Go spec: The scope of a label is the body of the function
                // in which it is declared and excludes the body of any nested
                // function.
                var stmt = Ꮡ(new ast.LabeledStmt(Label: label, Colon: colon, Stmt: Ꮡp.parseStmt()));
                return (new ast_LabeledStmtжStmt(stmt), false);
            }
        }
        Ꮡp.error(colon, // The label declaration typically starts at x[0].Pos(), but the label
 // declaration may be erroneous due to a token after that position (and
 // before the ':'). If SpuriousErrors is not set, the (only) error
 // reported for the line is the illegal label error instead of the token
 // before the ':' that caused the problem. Thus, use the (latest) colon
 // position for error reporting.
 "illegal label declaration"u8);
        return (new ast_BadStmtжStmt(Ꮡ(new ast.BadStmt(From: x[0].Pos(), To: colon + 1))), false);
    }
    if (exprᴛ2 == token.ARROW) {
        ref var arrow = ref heap<tokenꓸPos>(out var Ꮡarrow);
        arrow = p.pos;
        p.next();
        var y = Ꮡp.parseRhs();
        return (new ast_SendStmtжStmt(Ꮡ(new ast.SendStmt( // send statement
Chan: x[0], Arrow: arrow, Value: y))), false);
    }
    if (exprᴛ2 == token.INC || exprᴛ2 == token.DEC) {
        var s = Ꮡ(new ast.IncDecStmt( // increment or decrement
X: x[0], TokPos: p.pos, Tok: p.tok));
        p.next();
        return (new ast_IncDecStmtжStmt(s), false);
    }

    // expression
    return (new ast_ExprStmtжStmt(Ꮡ(new ast.ExprStmt(X: x[0]))), false);
});

internal static ж<ast.CallExpr> parseCallExpr(this ж<parser> Ꮡp, @string callType) {
    var x = Ꮡp.parseRhs();
    // could be a conversion: (some type)(x)
    {
        var t = ast.Unparen(x); if (!AreEqual(t, x)) {
            Ꮡp.error(x.Pos(), fmt.Sprintf("expression in %s must not be parenthesized"u8, callType));
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
            Ꮡp.error(Ꮡp.safePos(x.End()), fmt.Sprintf("expression in %s must be function call"u8, callType));
        }
    }
    return default!;
}

internal static ast.Stmt parseGoStmt(this ж<parser> Ꮡp) => func<ast.Stmt>((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "GoStmt"u8), defer);
    }
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = Ꮡp.expect(token.GO);
    var call = Ꮡp.parseCallExpr("go"u8);
    Ꮡp.expectSemi();
    if (call == nil) {
        return new ast_BadStmtжStmt(Ꮡ(new ast.BadStmt(From: pos, To: pos + 2)));
    }
    // len("go")
    return new ast_GoStmtжStmt(Ꮡ(new ast.GoStmt(Go: pos, Call: call)));
});

internal static ast.Stmt parseDeferStmt(this ж<parser> Ꮡp) => func<ast.Stmt>((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "DeferStmt"u8), defer);
    }
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = Ꮡp.expect(token.DEFER);
    var call = Ꮡp.parseCallExpr("defer"u8);
    Ꮡp.expectSemi();
    if (call == nil) {
        return new ast_BadStmtжStmt(Ꮡ(new ast.BadStmt(From: pos, To: pos + 5)));
    }
    // len("defer")
    return new ast_DeferStmtжStmt(Ꮡ(new ast.DeferStmt(Defer: pos, Call: call)));
});

internal static ж<ast.ReturnStmt> parseReturnStmt(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "ReturnStmt"u8), defer);
    }
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = p.pos;
    Ꮡp.expect(token.RETURN);
    slice<ast.Expr> x = default!;
    if (p.tok != token.SEMICOLON && p.tok != token.RBRACE) {
        x = Ꮡp.parseList(true);
    }
    Ꮡp.expectSemi();
    return Ꮡ(new ast.ReturnStmt(Return: pos, Results: x));
});

internal static ж<ast.BranchStmt> parseBranchStmt(this ж<parser> Ꮡp, token.Token tok) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "BranchStmt"u8), defer);
    }
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = Ꮡp.expect(tok);
    ж<ast.Ident> label = default!;
    if (tok != token.FALLTHROUGH && p.tok == token.IDENT) {
        label = Ꮡp.parseIdent();
    }
    Ꮡp.expectSemi();
    return Ꮡ(new ast.BranchStmt(TokPos: pos, Tok: tok, Label: label));
});

internal static ast.Expr makeExpr(this ж<parser> Ꮡp, ast.Stmt s, @string want) {
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
    Ꮡp.error(s.Pos(), fmt.Sprintf("expected %s, found %s (missing parentheses around composite literal?)"u8, want, found));
    return new ast_BadExprжExpr(Ꮡ(new ast.BadExpr(From: s.Pos(), To: Ꮡp.safePos(s.End()))));
}

[GoType("dyn")] partial struct parseIfHeader_semi {
    internal tokenꓸPos pos;
    internal @string lit; // ";" or "\n"; valid if pos.IsValid()
}

// parseIfHeader is an adjusted version of parser.header
// in cmd/compile/internal/syntax/parser.go, which has
// been tuned for better error handling.
internal static (ast.Stmt init, ast.Expr cond) parseIfHeader(this ж<parser> Ꮡp) {
    ast.Stmt init = default!;
    ast.Expr cond = default!;

    ref var p = ref Ꮡp.Value;
    if (p.tok == token.LBRACE) {
        Ꮡp.error(p.pos, "missing condition in if statement"u8);
        cond = new ast_BadExprжExpr(Ꮡ(new ast.BadExpr(From: p.pos, To: p.pos)));
        return (init, cond);
    }
    // p.tok != token.LBRACE
    nint prevLev = p.exprLev;
    p.exprLev = -1;
    if (p.tok != token.SEMICOLON) {
        // accept potential variable declaration but complain
        if (p.tok == token.VAR) {
            p.next();
            Ꮡp.error(p.pos, "var declaration not allowed in if initializer"u8);
        }
        (init, _) = Ꮡp.parseSimpleStmt(basic);
    }
    ast.Stmt condStmt = default!;
    parseIfHeader_semi semi = default!;
    if (p.tok != token.LBRACE){
        if (p.tok == token.SEMICOLON){
            semi.pos = p.pos;
            semi.lit = p.lit;
            p.next();
        } else {
            Ꮡp.expect(token.SEMICOLON);
        }
        if (p.tok != token.LBRACE) {
            (condStmt, _) = Ꮡp.parseSimpleStmt(basic);
        }
    } else {
        condStmt = init;
        init = default!;
    }
    if (condStmt != default!){
        cond = Ꮡp.makeExpr(condStmt, "boolean expression"u8);
    } else 
    if (semi.pos.IsValid()) {
        if (semi.lit == "\n"u8){
            Ꮡp.error(semi.pos, "unexpected newline, expecting { after if clause"u8);
        } else {
            Ꮡp.error(semi.pos, "missing condition in if statement"u8);
        }
    }
    // make sure we have a valid AST
    if (cond == default!) {
        cond = new ast_BadExprжExpr(Ꮡ(new ast.BadExpr(From: p.pos, To: p.pos)));
    }
    p.exprLev = prevLev;
    return (init, cond);
}

internal static ж<ast.IfStmt> parseIfStmt(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    deferǃ(decNestLev, incNestLev(Ꮡp), defer);
    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "IfStmt"u8), defer);
    }
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = Ꮡp.expect(token.IF);
    var (init, cond) = Ꮡp.parseIfHeader();
    var body = Ꮡp.parseBlockStmt();
    ast.Stmt else_ = default!;
    if (p.tok == token.ELSE){
        p.next();
        var exprᴛ1 = p.tok;
        if (exprᴛ1 == token.IF) {
            else_ = new ast_IfStmtжStmt(Ꮡp.parseIfStmt());
        }
        else if (exprᴛ1 == token.LBRACE) {
            else_ = new ast_BlockStmtжStmt(Ꮡp.parseBlockStmt());
            Ꮡp.expectSemi();
        }
        else { /* default: */
            Ꮡp.errorExpected(p.pos, "if statement or block"u8);
            else_ = new ast_BadStmtжStmt(Ꮡ(new ast.BadStmt(From: p.pos, To: p.pos)));
        }

    } else {
        Ꮡp.expectSemi();
    }
    return Ꮡ(new ast.IfStmt(If: pos, Init: init, Cond: cond, Body: body, Else: else_));
});

internal static ж<ast.CaseClause> parseCaseClause(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "CaseClause"u8), defer);
    }
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = p.pos;
    slice<ast.Expr> list = default!;
    if (p.tok == token.CASE){
        p.next();
        list = Ꮡp.parseList(true);
    } else {
        Ꮡp.expect(token.DEFAULT);
    }
    ref var colon = ref heap<tokenꓸPos>(out var Ꮡcolon);
    colon = Ꮡp.expect(token.COLON);
    var body = Ꮡp.parseStmtList();
    return Ꮡ(new ast.CaseClause(Case: pos, List: list, Colon: colon, Body: body));
});

internal static bool isTypeSwitchAssert(ast.Expr x) {
    var (a, ok) = x._<ж<ast.TypeAssertExpr>>(ᐧ);
    return ok && (~a).Type == default!;
}

internal static bool isTypeSwitchGuard(this ж<parser> Ꮡp, ast.Stmt s) {
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
                Ꮡp.error((~t).TokPos, // permit v = x.(type) but complain
 "expected ':=', found '='"u8);
                fallthrough = true;
            }
            if (fallthrough || !matchᴛ1 && exprᴛ1 == token.DEFINE) { matchᴛ1 = true;
                return true;
            }

        }
        break;
    }}
    return false;
}

internal static ast.Stmt parseSwitchStmt(this ж<parser> Ꮡp) => func<ast.Stmt>((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "SwitchStmt"u8), defer);
    }
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = Ꮡp.expect(token.SWITCH);
    ast.Stmt s1 = default!;
    ast.Stmt s2 = default!;
    if (p.tok != token.LBRACE) {
        nint prevLev = p.exprLev;
        p.exprLev = -1;
        if (p.tok != token.SEMICOLON) {
            (s2, _) = Ꮡp.parseSimpleStmt(basic);
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
                (s2, _) = Ꮡp.parseSimpleStmt(basic);
            }
        }
        p.exprLev = prevLev;
    }
    var typeSwitch = Ꮡp.isTypeSwitchGuard(s2);
    ref var lbrace = ref heap<tokenꓸPos>(out var Ꮡlbrace);
    lbrace = Ꮡp.expect(token.LBRACE);
    slice<ast.Stmt> list = default!;
    while (p.tok == token.CASE || p.tok == token.DEFAULT) {
        list = append(list, (ast.Stmt)(new ast_CaseClauseжStmt(Ꮡp.parseCaseClause())));
    }
    ref var rbrace = ref heap<tokenꓸPos>(out var Ꮡrbrace);
    rbrace = Ꮡp.expect(token.RBRACE);
    Ꮡp.expectSemi();
    var body = Ꮡ(new ast.BlockStmt(Lbrace: lbrace, List: list, Rbrace: rbrace));
    if (typeSwitch) {
        return new ast_TypeSwitchStmtжStmt(Ꮡ(new ast.TypeSwitchStmt(Switch: pos, Init: s1, Assign: s2, Body: body)));
    }
    return new ast_SwitchStmtжStmt(Ꮡ(new ast.SwitchStmt(Switch: pos, Init: s1, Tag: Ꮡp.makeExpr(s2, "switch expression"u8), Body: body)));
});

internal static ж<ast.CommClause> parseCommClause(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "CommClause"u8), defer);
    }
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = p.pos;
    ast.Stmt comm = default!;
    if (p.tok == token.CASE){
        p.next();
        var lhs = Ꮡp.parseList(false);
        if (p.tok == token.ARROW){
            // SendStmt
            if (len(lhs) > 1) {
                Ꮡp.errorExpected(lhs[0].Pos(), "1 expression"u8);
            }
            // continue with first expression
            ref var arrow = ref heap<tokenꓸPos>(out var Ꮡarrow);
            arrow = p.pos;
            p.next();
            var rhs = Ꮡp.parseRhs();
            comm = new ast_SendStmtжStmt(Ꮡ(new ast.SendStmt(Chan: lhs[0], Arrow: arrow, Value: rhs)));
        } else {
            // RecvStmt
            {
                ref var tok = ref heap<token.Token>(out var Ꮡtok);
                tok = p.tok; if (tok == token.ASSIGN || tok == token.DEFINE){
                    // RecvStmt with assignment
                    if (len(lhs) > 2) {
                        Ꮡp.errorExpected(lhs[0].Pos(), "1 or 2 expressions"u8);
                        // continue with first two expressions
                        lhs = lhs[0..2];
                    }
                    ref var posΔ1 = ref heap<tokenꓸPos>(out var ᏑposΔ1);
                    posΔ1 = p.pos;
                    p.next();
                    var rhs = Ꮡp.parseRhs();
                    comm = new ast_AssignStmtжStmt(Ꮡ(new ast.AssignStmt(Lhs: lhs, TokPos: posΔ1, Tok: tok, Rhs: new ast.Expr[]{rhs}.slice())));
                } else {
                    // lhs must be single receive operation
                    if (len(lhs) > 1) {
                        Ꮡp.errorExpected(lhs[0].Pos(), "1 expression"u8);
                    }
                    // continue with first expression
                    comm = new ast_ExprStmtжStmt(Ꮡ(new ast.ExprStmt(X: lhs[0])));
                }
            }
        }
    } else {
        Ꮡp.expect(token.DEFAULT);
    }
    ref var colon = ref heap<tokenꓸPos>(out var Ꮡcolon);
    colon = Ꮡp.expect(token.COLON);
    var body = Ꮡp.parseStmtList();
    return Ꮡ(new ast.CommClause(Case: pos, Comm: comm, Colon: colon, Body: body));
});

internal static ж<ast.SelectStmt> parseSelectStmt(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "SelectStmt"u8), defer);
    }
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = Ꮡp.expect(token.SELECT);
    ref var lbrace = ref heap<tokenꓸPos>(out var Ꮡlbrace);
    lbrace = Ꮡp.expect(token.LBRACE);
    slice<ast.Stmt> list = default!;
    while (p.tok == token.CASE || p.tok == token.DEFAULT) {
        list = append(list, (ast.Stmt)(new ast_CommClauseжStmt(Ꮡp.parseCommClause())));
    }
    ref var rbrace = ref heap<tokenꓸPos>(out var Ꮡrbrace);
    rbrace = Ꮡp.expect(token.RBRACE);
    Ꮡp.expectSemi();
    var body = Ꮡ(new ast.BlockStmt(Lbrace: lbrace, List: list, Rbrace: rbrace));
    return Ꮡ(new ast.SelectStmt(Select: pos, Body: body));
});

internal static ast.Stmt parseForStmt(this ж<parser> Ꮡp) => func<ast.Stmt>((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "ForStmt"u8), defer);
    }
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = Ꮡp.expect(token.FOR);
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
                ref var posΔ1 = ref heap<tokenꓸPos>(out var ᏑposΔ1);
                posΔ1 = p.pos;
                p.next();
                var y = new ast.Expr[]{new ast_UnaryExprжExpr(Ꮡ(new ast.UnaryExpr(OpPos: posΔ1, Op: token.RANGE, X: Ꮡp.parseRhs())))}.slice();
                s2 = new ast_AssignStmtжStmt(Ꮡ(new ast.AssignStmt(Rhs: y)));
                isRange = true;
            } else {
                (s2, isRange) = Ꮡp.parseSimpleStmt(rangeOk);
            }
        }
        if (!isRange && p.tok == token.SEMICOLON) {
            p.next();
            s1 = s2;
            s2 = default!;
            if (p.tok != token.SEMICOLON) {
                (s2, _) = Ꮡp.parseSimpleStmt(basic);
            }
            Ꮡp.expectSemi();
            if (p.tok != token.LBRACE) {
                (s3, _) = Ꮡp.parseSimpleStmt(basic);
            }
        }
        p.exprLev = prevLev;
    }
    var body = Ꮡp.parseBlockStmt();
    Ꮡp.expectSemi();
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
            Ꮡp.errorExpected((~@as).Lhs[len((~@as).Lhs) - 1].Pos(), // nothing to do
 "at most 2 expressions"u8);
            return new ast_BadStmtжStmt(Ꮡ(new ast.BadStmt(From: pos, To: Ꮡp.safePos(body.End()))));
        }}

        // parseSimpleStmt returned a right-hand side that
        // is a single unary expression of the form "range x"
        var x = (~@as).Rhs[0]._<ж<ast.UnaryExpr>>().Value.X;
        return new ast_RangeStmtжStmt(Ꮡ(new ast.RangeStmt(
            For: pos,
            Key: key,
            Value: value,
            TokPos: (~@as).TokPos,
            Tok: (~@as).Tok,
            Range: (~@as).Rhs[0].Pos(),
            X: x,
            Body: body
        )));
    }
    // regular for statement
    return new ast_ForStmtжStmt(Ꮡ(new ast.ForStmt(
        For: pos,
        Init: s1,
        Cond: Ꮡp.makeExpr(s2, "boolean or range expression"u8),
        Post: s3,
        Body: body
    )));
});

internal static ast.Stmt /*s*/ parseStmt(this ж<parser> Ꮡp) {
    ast.Stmt s = default!;
    func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

        deferǃ(decNestLev, incNestLev(Ꮡp), defer);
        if (p.trace) {
            deferǃ(un, trace(Ꮡp, "Statement"u8), defer);
        }
        var exprᴛ1 = p.tok;
        if (exprᴛ1 == token.CONST || exprᴛ1 == token.TYPE || exprᴛ1 == token.VAR) {
            s = new ast_DeclStmtжStmt(Ꮡ(new ast.DeclStmt(Decl: Ꮡp.parseDecl(stmtStart))));
        }
        else if (exprᴛ1 == token.IDENT || exprᴛ1 == token.INT || exprᴛ1 == token.FLOAT || exprᴛ1 == token.IMAG || exprᴛ1 == token.CHAR || exprᴛ1 == token.STRING || exprᴛ1 == token.FUNC || exprᴛ1 == token.LPAREN || exprᴛ1 == token.LBRACK || exprᴛ1 == token.STRUCT || exprᴛ1 == token.MAP || exprᴛ1 == token.CHAN || exprᴛ1 == token.INTERFACE || exprᴛ1 == token.ADD || exprᴛ1 == token.SUB || exprᴛ1 == token.MUL || exprᴛ1 == token.AND || exprᴛ1 == token.XOR || exprᴛ1 == token.ARROW || exprᴛ1 == token.NOT) {
            (s, _) = Ꮡp.parseSimpleStmt(labelOk);
            {
                var (_, isLabeledStmt) = s._<ж<ast.LabeledStmt>>(ᐧ); if (!isLabeledStmt) {
                    // tokens that may start an expression
                    // operands
                    // composite types
                    // unary operators
                    // because of the required look-ahead, labeled statements are
                    // parsed by parseSimpleStmt - don't expect a semicolon after
                    // them
                    Ꮡp.expectSemi();
                }
            }
        }
        else if (exprᴛ1 == token.GO) {
            s = Ꮡp.parseGoStmt();
        }
        else if (exprᴛ1 == token.DEFER) {
            s = Ꮡp.parseDeferStmt();
        }
        else if (exprᴛ1 == token.RETURN) {
            s = new ast_ReturnStmtжStmt(Ꮡp.parseReturnStmt());
        }
        else if (exprᴛ1 == token.BREAK || exprᴛ1 == token.CONTINUE || exprᴛ1 == token.GOTO || exprᴛ1 == token.FALLTHROUGH) {
            s = new ast_BranchStmtжStmt(Ꮡp.parseBranchStmt(p.tok));
        }
        else if (exprᴛ1 == token.LBRACE) {
            s = new ast_BlockStmtжStmt(Ꮡp.parseBlockStmt());
            Ꮡp.expectSemi();
        }
        else if (exprᴛ1 == token.IF) {
            s = new ast_IfStmtжStmt(Ꮡp.parseIfStmt());
        }
        else if (exprᴛ1 == token.SWITCH) {
            s = Ꮡp.parseSwitchStmt();
        }
        else if (exprᴛ1 == token.SELECT) {
            s = new ast_SelectStmtжStmt(Ꮡp.parseSelectStmt());
        }
        else if (exprᴛ1 == token.FOR) {
            s = Ꮡp.parseForStmt();
        }
        else if (exprᴛ1 == token.SEMICOLON) {
            s = new ast_EmptyStmtжStmt(Ꮡ(new ast.EmptyStmt( // Is it ever possible to have an implicit semicolon
 // producing an empty statement in a valid program?
 // (handle correctly anyway)
Semicolon: p.pos, Implicit: p.lit == "\n"u8)));
            p.next();
        }
        else if (exprᴛ1 == token.RBRACE) {
            s = new ast_EmptyStmtжStmt(Ꮡ(new ast.EmptyStmt( // a semicolon may be omitted before a closing "}"
Semicolon: p.pos, Implicit: true)));
        }
        else { /* default: */
            ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
            pos = p.pos;
            Ꮡp.errorExpected(pos, // no statement found
 "statement"u8);
            p.advance(stmtStart);
            s = new ast_BadStmtжStmt(Ꮡ(new ast.BadStmt(From: pos, To: p.pos)));
        }

    });
    return s;
}

// type parseSpecFunction is a methodless func type — rendered inline as its base delegate

// ----------------------------------------------------------------------------
// Declarations
internal static ast.Spec parseImportSpec(this ж<parser> Ꮡp, ж<ast.CommentGroup> Ꮡdoc, token.Token _Δp2, nint _Δp3) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "ImportSpec"u8), defer);
    }
    ж<ast.Ident> ident = default!;
    var exprᴛ1 = p.tok;
    if (exprᴛ1 == token.IDENT) {
        ident = Ꮡp.parseIdent();
    }
    else if (exprᴛ1 == token.PERIOD) {
        ident = Ꮡ(new ast.Ident(NamePos: p.pos, Name: "."u8));
        p.next();
    }

    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = p.pos;
    ref var path = ref heap(new @string(), out var Ꮡpath);
    if (p.tok == token.STRING){
        path = p.lit;
        p.next();
    } else 
    if (p.tok.IsLiteral()){
        Ꮡp.error(pos, "import path must be a string"u8);
        p.next();
    } else {
        Ꮡp.error(pos, "missing import path"u8);
        p.advance(exprEnd);
    }
    var comment = Ꮡp.expectSemi();
    // collect imports
    var spec = Ꮡ(new ast.ImportSpec(
        Doc: Ꮡdoc,
        Name: ident,
        Path: Ꮡ(new ast.BasicLit(ValuePos: pos, Kind: token.STRING, Value: path)),
        Comment: comment
    ));
    p.imports = append(p.imports, spec);
    return new ast_ImportSpecжSpec(spec);
});

internal static ast.Spec parseValueSpec(this ж<parser> Ꮡp, ж<ast.CommentGroup> Ꮡdoc, token.Token keyword, nint iota) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, keyword.String() + "Spec"u8), defer);
    }
    var idents = Ꮡp.parseIdentList();
    ast.Expr typ = default!;
    slice<ast.Expr> values = default!;
    var exprᴛ1 = keyword;
    if (exprᴛ1 == token.CONST) {
        if (p.tok != token.EOF && p.tok != token.SEMICOLON && p.tok != token.RPAREN) {
            // always permit optional type and initialization for more tolerant parsing
            typ = Ꮡp.tryIdentOrType();
            if (p.tok == token.ASSIGN) {
                p.next();
                values = Ꮡp.parseList(true);
            }
        }
    }
    else if (exprᴛ1 == token.VAR) {
        if (p.tok != token.ASSIGN) {
            typ = Ꮡp.parseType();
        }
        if (p.tok == token.ASSIGN) {
            p.next();
            values = Ꮡp.parseList(true);
        }
    }
    else { /* default: */
        throw panic("unreachable");
    }

    var comment = Ꮡp.expectSemi();
    var spec = Ꮡ(new ast.ValueSpec(
        Doc: Ꮡdoc,
        Names: idents,
        Type: typ,
        Values: values,
        Comment: comment
    ));
    return new ast_ValueSpecжSpec(spec);
});

internal static void parseGenericType(this ж<parser> Ꮡp, ж<ast.TypeSpec> Ꮡspec, tokenꓸPos openPos, ж<ast.Ident> Ꮡname0, ast.Expr typ0) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;
    ref var spec = ref Ꮡspec.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "parseGenericType"u8), defer);
    }
    var list = Ꮡp.parseParameterList(Ꮡname0, typ0, token.RBRACK);
    ref var closePos = ref heap<tokenꓸPos>(out var ᏑclosePos);
    closePos = Ꮡp.expect(token.RBRACK);
    spec.TypeParams = Ꮡ(new ast.FieldList(Opening: openPos, List: list, Closing: closePos));
    // Let the type checker decide whether to accept type parameters on aliases:
    // see go.dev/issue/46477.
    if (p.tok == token.ASSIGN) {
        // type alias
        spec.Assign = p.pos;
        p.next();
    }
    spec.Type = Ꮡp.parseType();
});

internal static ast.Spec parseTypeSpec(this ж<parser> Ꮡp, ж<ast.CommentGroup> Ꮡdoc, token.Token _Δp2, nint _Δp3) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "TypeSpec"u8), defer);
    }
    var name = Ꮡp.parseIdent();
    var spec = Ꮡ(new ast.TypeSpec(Doc: Ꮡdoc, Name: name));
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
            ast.Expr x = new ast_IdentжExpr(Ꮡp.parseIdent());
            if (p.tok != token.LBRACK) {
                // To parse the expression starting with name, expand
                // the call sequence we would get by passing in name
                // to parser.expr, and pass in name to parsePrimaryExpr.
                p.exprLev++;
                var lhs = Ꮡp.parsePrimaryExpr(x);
                x = Ꮡp.parseBinaryExpr(lhs, token.LowestPrec + 1);
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
                var (pname, ptype) = extractName(x, p.tok == token.COMMA); if (pname != nil && (ptype != default! || p.tok != token.RBRACK)){
                    // spec.Name "[" pname ...
                    // spec.Name "[" pname ptype ...
                    // spec.Name "[" pname ptype "," ...
                    Ꮡp.parseGenericType(spec, lbrack, pname, ptype);
                } else {
                    // ptype may be nil
                    // spec.Name "[" pname "]" ...
                    // spec.Name "[" x ...
                    spec.Value.Type = new ast_ArrayTypeжExpr(Ꮡp.parseArrayType(lbrack, x));
                }
            }
        } else {
            // array type
            spec.Value.Type = new ast_ArrayTypeжExpr(Ꮡp.parseArrayType(lbrack, default!));
        }
    } else {
        // no type parameters
        if (p.tok == token.ASSIGN) {
            // type alias
            spec.Value.Assign = p.pos;
            p.next();
        }
        spec.Value.Type = Ꮡp.parseType();
    }
    spec.Value.Comment = Ꮡp.expectSemi();
    return new ast_TypeSpecжSpec(spec);
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
    case ж<ast.Ident> xΔ1: {
        return (xΔ1, default!);
    }
    case ж<ast.BinaryExpr> xΔ1: {
        var exprᴛ1 = (~xΔ1).Op;
        if (exprᴛ1 == token.MUL) {
            {
                var (name, _) = (~xΔ1).X._<ж<ast.Ident>>(ᐧ); if (name != nil && (force || isTypeElem((~xΔ1).Y))) {
                    // x = name *x.Y
                    return (name, new ast_StarExprжExpr(Ꮡ(new ast.StarExpr(Star: (~xΔ1).OpPos, X: (~xΔ1).Y))));
                }
            }
        }
        if (exprᴛ1 == token.OR) {
            {
                var (name, lhs) = extractName((~xΔ1).X, force || isTypeElem((~xΔ1).Y)); if (name != nil && lhs != default!) {
                    // x = name lhs|x.Y
                    ref var op = ref heap<ast.BinaryExpr>(out var Ꮡop);
                    op = xΔ1.Value;
                    op.X = lhs;
                    return (name, new ast_BinaryExprжExpr(Ꮡop));
                }
            }
        }

        break;
    }
    case ж<ast.CallExpr> xΔ1: {
        {
            var (name, _) = (~xΔ1).Fun._<ж<ast.Ident>>(ᐧ); if (name != nil) {
                if (len((~xΔ1).Args) == 1 && (~xΔ1).Ellipsis == token.NoPos && (force || isTypeElem((~xΔ1).Args[0]))) {
                    // x = name "(" x.ArgList[0] ")"
                    return (name, (~xΔ1).Args[0]);
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
    case ж<ast.ArrayType> _:
    case ж<ast.StructType> _:
    case ж<ast.FuncType> _:
    case ж<ast.InterfaceType> _:
    case ж<ast.MapType> _:
    case ж<ast.ChanType> _: {
        var xΔ1 = x;
        return true;
    }
    case ж<ast.BinaryExpr> xΔ1: {
        return isTypeElem((~xΔ1).X) || isTypeElem((~xΔ1).Y);
    }
    case ж<ast.UnaryExpr> xΔ1: {
        return (~xΔ1).Op == token.TILDE;
    }
    case ж<ast.ParenExpr> xΔ1: {
        return isTypeElem((~xΔ1).X);
    }}
    return false;
}

internal static ж<ast.GenDecl> parseGenDecl(this ж<parser> Ꮡp, token.Token keyword, Func<ж<ast.CommentGroup>, token.Token, nint, ast.Spec> f) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "GenDecl("u8 + keyword.String() + ")"u8), defer);
    }
    var doc = p.leadComment;
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = Ꮡp.expect(keyword);
    ref var lparen = ref heap(new tokenꓸPos(), out var Ꮡlparen);
    ref var rparen = ref heap(new tokenꓸPos(), out var Ꮡrparen);
    slice<ast.Spec> list = default!;
    if (p.tok == token.LPAREN){
        lparen = p.pos;
        p.next();
        for (nint iota = 0; p.tok != token.RPAREN && p.tok != token.EOF; iota++) {
            list = append(list, f(p.leadComment, keyword, iota));
        }
        rparen = Ꮡp.expect(token.RPAREN);
        Ꮡp.expectSemi();
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

internal static ж<ast.FuncDecl> parseFuncDecl(this ж<parser> Ꮡp) => func((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "FunctionDecl"u8), defer);
    }
    var doc = p.leadComment;
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = Ꮡp.expect(token.FUNC);
    ж<ast.FieldList> recv = default!;
    if (p.tok == token.LPAREN) {
        (_, recv) = Ꮡp.parseParameters(false);
    }
    var ident = Ꮡp.parseIdent();
    var (tparams, @params) = Ꮡp.parseParameters(true);
    if (recv != nil && tparams != nil) {
        // Method declarations do not have type parameters. We parse them for a
        // better error message and improved error recovery.
        Ꮡp.error((~tparams).Opening, "method must have no type parameters"u8);
        tparams = default!;
    }
    var results = Ꮡp.parseResult();
    ж<ast.BlockStmt> body = default!;
    var exprᴛ1 = p.tok;
    if (exprᴛ1 == token.LBRACE) {
        body = Ꮡp.parseBody();
        Ꮡp.expectSemi();
    }
    else if (exprᴛ1 == token.SEMICOLON) {
        p.next();
        if (p.tok == token.LBRACE) {
            // opening { of function declaration on next line
            Ꮡp.error(p.pos, "unexpected semicolon or newline before {"u8);
            body = Ꮡp.parseBody();
            Ꮡp.expectSemi();
        }
    }
    else { /* default: */
        Ꮡp.expectSemi();
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

internal static ast.Decl parseDecl(this ж<parser> Ꮡp, map<token.Token, bool> sync) => func<ast.Decl>((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "Declaration"u8), defer);
    }
    Func<ж<ast.CommentGroup>, token.Token, nint, ast.Spec> f = default!;
    var exprᴛ1 = p.tok;
    if (exprᴛ1 == token.IMPORT) {
        f = (ж<ast.CommentGroup> p1, token.Token p2, nint p3) => Ꮡp.parseImportSpec(p1, p2, p3);
    }
    else if (exprᴛ1 == token.CONST || exprᴛ1 == token.VAR) {
        f = (ж<ast.CommentGroup> p1, token.Token p2, nint p3) => Ꮡp.parseValueSpec(p1, p2, p3);
    }
    else if (exprᴛ1 == token.TYPE) {
        f = (ж<ast.CommentGroup> p1, token.Token p2, nint p3) => Ꮡp.parseTypeSpec(p1, p2, p3);
    }
    else if (exprᴛ1 == token.FUNC) {
        return new ast_FuncDeclжDecl(Ꮡp.parseFuncDecl());
    }
    { /* default: */
        ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
        pos = p.pos;
        Ꮡp.errorExpected(pos, "declaration"u8);
        p.advance(sync);
        return new ast_BadDeclжDecl(Ꮡ(new ast.BadDecl(From: pos, To: p.pos)));
    }

    return new ast_GenDeclжDecl(Ꮡp.parseGenDecl(p.tok, f));
});

// ----------------------------------------------------------------------------
// Source files
internal static ж<ast.File> parseFile(this ж<parser> Ꮡp) => func<ж<ast.File>>((defer, recover) => {
    ref var p = ref Ꮡp.Value;

    if (p.trace) {
        deferǃ(un, trace(Ꮡp, "File"u8), defer);
    }
    // Don't bother parsing the rest if we had errors scanning the first token.
    // Likely not a Go source file at all.
    if (p.errors.Len() != 0) {
        return default!;
    }
    // package clause
    var doc = p.leadComment;
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = Ꮡp.expect(token.PACKAGE);
    // Go spec: The package clause is not a declaration;
    // the package name does not appear in any scope.
    var ident = Ꮡp.parseIdent();
    if ((~ident).Name == "_"u8 && (Mode)(p.mode & DeclarationErrors) != 0) {
        Ꮡp.error(p.pos, "invalid package name _"u8);
    }
    Ꮡp.expectSemi();
    // Don't bother parsing the rest if we had errors parsing the package clause.
    // Likely not a Go source file at all.
    if (p.errors.Len() != 0) {
        return default!;
    }
    slice<ast.Decl> decls = default!;
    if ((Mode)(p.mode & PackageClauseOnly) == 0) {
        // import decls
        while (p.tok == token.IMPORT) {
            decls = append(decls, (ast.Decl)(new ast_GenDeclжDecl(Ꮡp.parseGenDecl(token.IMPORT, new Func<ж<ast.CommentGroup>, token.Token, nint, ast.Spec>(Ꮡp.parseImportSpec)))));
        }
        if ((Mode)(p.mode & ImportsOnly) == 0) {
            // rest of package body
            token.Token prev = token.IMPORT;
            while (p.tok != token.EOF) {
                // Continue to accept import declarations for error tolerance, but complain.
                if (p.tok == token.IMPORT && prev != token.IMPORT) {
                    Ꮡp.error(p.pos, "imports must appear before other declarations"u8);
                }
                prev = p.tok;
                decls = append(decls, Ꮡp.parseDecl(declStart));
            }
        }
    }
    var f = Ꮡ(new ast.File(
        Doc: doc,
        Package: pos,
        Name: ident,
        Decls: decls,
        FileStart: ((tokenꓸPos)p.@file.Base()),
        FileEnd: ((tokenꓸPos)(p.@file.Base() + p.@file.Size())),
        Imports: p.imports,
        Comments: p.comments,
        GoVersion: p.goVersion
    ));
    Action<tokenꓸPos, @string> declErr = default!;
    if ((Mode)(p.mode & DeclarationErrors) != 0) {
        declErr = (tokenꓸPos p1, @string p2) => Ꮡp.error(p1, p2);
    }
    if ((Mode)(p.mode & SkipObjectResolution) == 0) {
        resolveFile(f, p.@file, declErr);
    }
    return f;
});

} // end parser_package
