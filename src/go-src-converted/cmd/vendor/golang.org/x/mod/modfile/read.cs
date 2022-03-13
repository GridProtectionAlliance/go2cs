// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modfile -- go2cs converted at 2022 March 13 06:40:51 UTC
// import "cmd/vendor/golang.org/x/mod/modfile" ==> using modfile = go.cmd.vendor.golang.org.x.mod.modfile_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\mod\modfile\read.go
namespace go.cmd.vendor.golang.org.x.mod;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using os = os_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;


// A Position describes an arbitrary source position in a file, including the
// file, line, column, and byte offset.

using System;
public static partial class modfile_package {

public partial struct Position {
    public nint Line; // line in input (starting at 1)
    public nint LineRune; // rune in line (starting at 1)
    public nint Byte; // byte in input (starting at 0)
}

// add returns the position at the end of s, assuming it starts at p.
public static Position add(this Position p, @string s) {
    p.Byte += len(s);
    {
        var n = strings.Count(s, "\n");

        if (n > 0) {
            p.Line += n;
            s = s[(int)strings.LastIndex(s, "\n") + 1..];
            p.LineRune = 1;
        }
    }
    p.LineRune += utf8.RuneCountInString(s);
    return p;
}

// An Expr represents an input element.
public partial interface Expr {
    ptr<Comments> Span(); // Comment returns the comments attached to the expression.
// This method would normally be named 'Comments' but that
// would interfere with embedding a type of the same name.
    ptr<Comments> Comment();
}

// A Comment represents a single // comment.
public partial struct Comment {
    public Position Start;
    public @string Token; // without trailing newline
    public bool Suffix; // an end of line (not whole line) comment
}

// Comments collects the comments associated with an expression.
public partial struct Comments {
    public slice<Comment> Before; // whole-line comments before this expression
    public slice<Comment> Suffix; // end-of-line comments after this expression

// For top-level expressions only, After lists whole-line
// comments following the expression.
    public slice<Comment> After;
}

// Comment returns the receiver. This isn't useful by itself, but
// a Comments struct is embedded into all the expression
// implementation types, and this gives each of those a Comment
// method to satisfy the Expr interface.
private static ptr<Comments> Comment(this ptr<Comments> _addr_c) {
    ref Comments c = ref _addr_c.val;

    return _addr_c!;
}

// A FileSyntax represents an entire go.mod file.
public partial struct FileSyntax {
    public @string Name; // file path
    public ref Comments Comments => ref Comments_val;
    public slice<Expr> Stmt;
}

private static (Position, Position) Span(this ptr<FileSyntax> _addr_x) {
    Position start = default;
    Position end = default;
    ref FileSyntax x = ref _addr_x.val;

    if (len(x.Stmt) == 0) {
        return ;
    }
    start, _ = x.Stmt[0].Span();
    _, end = x.Stmt[len(x.Stmt) - 1].Span();
    return (start, end);
}

// addLine adds a line containing the given tokens to the file.
//
// If the first token of the hint matches the first token of the
// line, the new line is added at the end of the block containing hint,
// extracting hint into a new block if it is not yet in one.
//
// If the hint is non-nil buts its first token does not match,
// the new line is added after the block containing hint
// (or hint itself, if not in a block).
//
// If no hint is provided, addLine appends the line to the end of
// the last block with a matching first token,
// or to the end of the file if no such block exists.
private static ptr<Line> addLine(this ptr<FileSyntax> _addr_x, Expr hint, params @string[] tokens) {
    tokens = tokens.Clone();
    ref FileSyntax x = ref _addr_x.val;

    if (hint == null) { 
        // If no hint given, add to the last statement of the given type.
Loop:
        {
            var i__prev1 = i;

            for (var i = len(x.Stmt) - 1; i >= 0; i--) {
                var stmt = x.Stmt[i];
                switch (stmt.type()) {
                    case ptr<Line> stmt:
                        if (stmt.Token != null && stmt.Token[0] == tokens[0]) {
                            hint = stmt;
                            _breakLoop = true;
                            break;
                        }
                        break;
                    case ptr<LineBlock> stmt:
                        if (stmt.Token[0] == tokens[0]) {
                            hint = stmt;
                            _breakLoop = true;
                            break;
                        }
                        break;
                }
            }


            i = i__prev1;
        }
    }
    Func<nint, ptr<Line>> newLineAfter = i => {
        ptr<Line> @new = addr(new Line(Token:tokens));
        if (i == len(x.Stmt)) {
            x.Stmt = append(x.Stmt, new);
        }
        else
 {
            x.Stmt = append(x.Stmt, null);
            copy(x.Stmt[(int)i + 2..], x.Stmt[(int)i + 1..]);
            x.Stmt[i + 1] = new;
        }
        return _addr_new!;
    };

    if (hint != null) {
        {
            var i__prev1 = i;
            var stmt__prev1 = stmt;

            foreach (var (__i, __stmt) in x.Stmt) {
                i = __i;
                stmt = __stmt;
                switch (stmt.type()) {
                    case ptr<Line> stmt:
                        if (stmt == hint) {
                            if (stmt.Token == null || stmt.Token[0] != tokens[0]) {
                                return _addr_newLineAfter(i)!;
                            } 

                            // Convert line to line block.
                            stmt.InBlock = true;
                            ptr<LineBlock> block = addr(new LineBlock(Token:stmt.Token[:1],Line:[]*Line{stmt}));
                            stmt.Token = stmt.Token[(int)1..];
                            x.Stmt[i] = block;
                            @new = addr(new Line(Token:tokens[1:],InBlock:true));
                            block.Line = append(block.Line, new);
                            return _addr_new!;
                        }
                        break;
                    case ptr<LineBlock> stmt:
                        if (stmt == hint) {
                            if (stmt.Token[0] != tokens[0]) {
                                return _addr_newLineAfter(i)!;
                            }
                            @new = addr(new Line(Token:tokens[1:],InBlock:true));
                            stmt.Line = append(stmt.Line, new);
                            return _addr_new!;
                        }
                        foreach (var (j, line) in stmt.Line) {
                            if (line == hint) {
                                if (stmt.Token[0] != tokens[0]) {
                                    return _addr_newLineAfter(i)!;
                                } 

                                // Add new line after hint within the block.
                                stmt.Line = append(stmt.Line, null);
                                copy(stmt.Line[(int)j + 2..], stmt.Line[(int)j + 1..]);
                                @new = addr(new Line(Token:tokens[1:],InBlock:true));
                                stmt.Line[j + 1] = new;
                                return _addr_new!;
                            }
                        }
                        break;
                }
            }

            i = i__prev1;
            stmt = stmt__prev1;
        }
    }
    @new = addr(new Line(Token:tokens));
    x.Stmt = append(x.Stmt, new);
    return _addr_new!;
}

private static void updateLine(this ptr<FileSyntax> _addr_x, ptr<Line> _addr_line, params @string[] tokens) {
    tokens = tokens.Clone();
    ref FileSyntax x = ref _addr_x.val;
    ref Line line = ref _addr_line.val;

    if (line.InBlock) {
        tokens = tokens[(int)1..];
    }
    line.Token = tokens;
}

// markRemoved modifies line so that it (and its end-of-line comment, if any)
// will be dropped by (*FileSyntax).Cleanup.
private static void markRemoved(this ptr<Line> _addr_line) {
    ref Line line = ref _addr_line.val;

    line.Token = null;
    line.Comments.Suffix = null;
}

// Cleanup cleans up the file syntax x after any edit operations.
// To avoid quadratic behavior, (*Line).markRemoved marks the line as dead
// by setting line.Token = nil but does not remove it from the slice
// in which it appears. After edits have all been indicated,
// calling Cleanup cleans out the dead lines.
private static void Cleanup(this ptr<FileSyntax> _addr_x) {
    ref FileSyntax x = ref _addr_x.val;

    nint w = 0;
    {
        var stmt__prev1 = stmt;

        foreach (var (_, __stmt) in x.Stmt) {
            stmt = __stmt;
            switch (stmt.type()) {
                case ptr<Line> stmt:
                    if (stmt.Token == null) {
                        continue;
                    }
                    break;
                case ptr<LineBlock> stmt:
                    nint ww = 0;
                    {
                        var line__prev2 = line;

                        foreach (var (_, __line) in stmt.Line) {
                            line = __line;
                            if (line.Token != null) {
                                stmt.Line[ww] = line;
                                ww++;
                            }
                        }

                        line = line__prev2;
                    }

                    if (ww == 0) {
                        continue;
                    }
                    if (ww == 1) { 
                        // Collapse block into single line.
                        ptr<Line> line = addr(new Line(Comments:Comments{Before:commentsAdd(stmt.Before,stmt.Line[0].Before),Suffix:commentsAdd(stmt.Line[0].Suffix,stmt.Suffix),After:commentsAdd(stmt.Line[0].After,stmt.After),},Token:stringsAdd(stmt.Token,stmt.Line[0].Token),));
                        x.Stmt[w] = line;
                        w++;
                        continue;
                    }
                    stmt.Line = stmt.Line[..(int)ww];
                    break;
            }
            x.Stmt[w] = stmt;
            w++;
        }
        stmt = stmt__prev1;
    }

    x.Stmt = x.Stmt[..(int)w];
}

private static slice<Comment> commentsAdd(slice<Comment> x, slice<Comment> y) {
    return append(x.slice(-1, len(x), len(x)), y);
}

private static slice<@string> stringsAdd(slice<@string> x, slice<@string> y) {
    return append(x.slice(-1, len(x), len(x)), y);
}

// A CommentBlock represents a top-level block of comments separate
// from any rule.
public partial struct CommentBlock {
    public ref Comments Comments => ref Comments_val;
    public Position Start;
}

private static (Position, Position) Span(this ptr<CommentBlock> _addr_x) {
    Position start = default;
    Position end = default;
    ref CommentBlock x = ref _addr_x.val;

    return (x.Start, x.Start);
}

// A Line is a single line of tokens.
public partial struct Line {
    public ref Comments Comments => ref Comments_val;
    public Position Start;
    public slice<@string> Token;
    public bool InBlock;
    public Position End;
}

private static (Position, Position) Span(this ptr<Line> _addr_x) {
    Position start = default;
    Position end = default;
    ref Line x = ref _addr_x.val;

    return (x.Start, x.End);
}

// A LineBlock is a factored block of lines, like
//
//    require (
//        "x"
//        "y"
//    )
//
public partial struct LineBlock {
    public ref Comments Comments => ref Comments_val;
    public Position Start;
    public LParen LParen;
    public slice<@string> Token;
    public slice<ptr<Line>> Line;
    public RParen RParen;
}

private static (Position, Position) Span(this ptr<LineBlock> _addr_x) {
    Position start = default;
    Position end = default;
    ref LineBlock x = ref _addr_x.val;

    return (x.Start, x.RParen.Pos.add(")"));
}

// An LParen represents the beginning of a parenthesized line block.
// It is a place to store suffix comments.
public partial struct LParen {
    public ref Comments Comments => ref Comments_val;
    public Position Pos;
}

private static (Position, Position) Span(this ptr<LParen> _addr_x) {
    Position start = default;
    Position end = default;
    ref LParen x = ref _addr_x.val;

    return (x.Pos, x.Pos.add(")"));
}

// An RParen represents the end of a parenthesized line block.
// It is a place to store whole-line (before) comments.
public partial struct RParen {
    public ref Comments Comments => ref Comments_val;
    public Position Pos;
}

private static (Position, Position) Span(this ptr<RParen> _addr_x) {
    Position start = default;
    Position end = default;
    ref RParen x = ref _addr_x.val;

    return (x.Pos, x.Pos.add(")"));
}

// An input represents a single input file being parsed.
private partial struct input {
    public @string filename; // name of input file, for errors
    public slice<byte> complete; // entire input
    public slice<byte> remaining; // remaining input
    public slice<byte> tokenStart; // token being scanned to end of input
    public token token; // next token to be returned by lex, peek
    public Position pos; // current input position
    public slice<Comment> comments; // accumulated comments

// Parser state.
    public ptr<FileSyntax> file; // returned top-level syntax tree
    public ErrorList parseErrors; // errors encountered during parsing

// Comment assignment state.
    public slice<Expr> pre; // all expressions, in preorder traversal
    public slice<Expr> post; // all expressions, in postorder traversal
}

private static ptr<input> newInput(@string filename, slice<byte> data) {
    return addr(new input(filename:filename,complete:data,remaining:data,pos:Position{Line:1,LineRune:1,Byte:0},));
}

// parse parses the input file.
private static (ptr<FileSyntax>, error) parse(@string file, slice<byte> data) => func((defer, _, _) => {
    ptr<FileSyntax> f = default!;
    error err = default!;
 
    // The parser panics for both routine errors like syntax errors
    // and for programmer bugs like array index errors.
    // Turn both into error returns. Catching bug panics is
    // especially important when processing many files.
    var @in = newInput(file, data);
    defer(() => {
        {
            var e = recover();

            if (e != null && e != _addr_@in.parseErrors) {
                @in.parseErrors = append(@in.parseErrors, new Error(Filename:in.filename,Pos:in.pos,Err:fmt.Errorf("internal error: %v",e),));
            }

        }
        if (err == null && len(@in.parseErrors) > 0) {
            err = @in.parseErrors;
        }
    }()); 

    // Prime the lexer by reading in the first token. It will be available
    // in the next peek() or lex() call.
    @in.readToken(); 

    // Invoke the parser.
    @in.parseFile();
    if (len(@in.parseErrors) > 0) {
        return (_addr_null!, error.As(@in.parseErrors)!);
    }
    @in.file.Name = @in.filename; 

    // Assign comments to nearby syntax.
    @in.assignComments();

    return (_addr_@in.file!, error.As(null!)!);
});

// Error is called to report an error.
// Error does not return: it panics.
private static void Error(this ptr<input> _addr_@in, @string s) => func((_, panic, _) => {
    ref input @in = ref _addr_@in.val;

    @in.parseErrors = append(@in.parseErrors, new Error(Filename:in.filename,Pos:in.pos,Err:errors.New(s),));
    panic(_addr_@in.parseErrors);
});

// eof reports whether the input has reached end of file.
private static bool eof(this ptr<input> _addr_@in) {
    ref input @in = ref _addr_@in.val;

    return len(@in.remaining) == 0;
}

// peekRune returns the next rune in the input without consuming it.
private static nint peekRune(this ptr<input> _addr_@in) {
    ref input @in = ref _addr_@in.val;

    if (len(@in.remaining) == 0) {
        return 0;
    }
    var (r, _) = utf8.DecodeRune(@in.remaining);
    return int(r);
}

// peekPrefix reports whether the remaining input begins with the given prefix.
private static bool peekPrefix(this ptr<input> _addr_@in, @string prefix) {
    ref input @in = ref _addr_@in.val;
 
    // This is like bytes.HasPrefix(in.remaining, []byte(prefix))
    // but without the allocation of the []byte copy of prefix.
    for (nint i = 0; i < len(prefix); i++) {
        if (i >= len(@in.remaining) || @in.remaining[i] != prefix[i]) {
            return false;
        }
    }
    return true;
}

// readRune consumes and returns the next rune in the input.
private static nint readRune(this ptr<input> _addr_@in) {
    ref input @in = ref _addr_@in.val;

    if (len(@in.remaining) == 0) {
        @in.Error("internal lexer error: readRune at EOF");
    }
    var (r, size) = utf8.DecodeRune(@in.remaining);
    @in.remaining = @in.remaining[(int)size..];
    if (r == '\n') {
        @in.pos.Line++;
        @in.pos.LineRune = 1;
    }
    else
 {
        @in.pos.LineRune++;
    }
    @in.pos.Byte += size;
    return int(r);
}

private partial struct token {
    public tokenKind kind;
    public Position pos;
    public Position endPos;
    public @string text;
}

private partial struct tokenKind { // : nint
}

private static readonly tokenKind _EOF = -(iota + 1);
private static readonly var _EOLCOMMENT = 0;
private static readonly var _IDENT = 1;
private static readonly var _STRING = 2;
private static readonly var _COMMENT = 3; 

// newlines and punctuation tokens are allowed as ASCII codes.

private static bool isComment(this tokenKind k) {
    return k == _COMMENT || k == _EOLCOMMENT;
}

// isEOL returns whether a token terminates a line.
private static bool isEOL(this tokenKind k) {
    return k == _EOF || k == _EOLCOMMENT || k == '\n';
}

// startToken marks the beginning of the next input token.
// It must be followed by a call to endToken, once the token's text has
// been consumed using readRune.
private static void startToken(this ptr<input> _addr_@in) {
    ref input @in = ref _addr_@in.val;

    @in.tokenStart = @in.remaining;
    @in.token.text = "";
    @in.token.pos = @in.pos;
}

// endToken marks the end of an input token.
// It records the actual token string in tok.text.
// A single trailing newline (LF or CRLF) will be removed from comment tokens.
private static void endToken(this ptr<input> _addr_@in, tokenKind kind) {
    ref input @in = ref _addr_@in.val;

    @in.token.kind = kind;
    var text = string(@in.tokenStart[..(int)len(@in.tokenStart) - len(@in.remaining)]);
    if (kind.isComment()) {
        if (strings.HasSuffix(text, "\r\n")) {
            text = text[..(int)len(text) - 2];
        }
        else
 {
            text = strings.TrimSuffix(text, "\n");
        }
    }
    @in.token.text = text;
    @in.token.endPos = @in.pos;
}

// peek returns the kind of the the next token returned by lex.
private static tokenKind peek(this ptr<input> _addr_@in) {
    ref input @in = ref _addr_@in.val;

    return @in.token.kind;
}

// lex is called from the parser to obtain the next input token.
private static token lex(this ptr<input> _addr_@in) {
    ref input @in = ref _addr_@in.val;

    var tok = @in.token;
    @in.readToken();
    return tok;
}

// readToken lexes the next token from the text and stores it in in.token.
private static void readToken(this ptr<input> _addr_@in) {
    ref input @in = ref _addr_@in.val;
 
    // Skip past spaces, stopping at non-space or EOF.
    while (!@in.eof()) {
        var c = @in.peekRune();
        if (c == ' ' || c == '\t' || c == '\r') {
            @in.readRune();
            continue;
        }
        if (@in.peekPrefix("//")) {
            @in.startToken(); 

            // Is this comment the only thing on its line?
            // Find the last \n before this // and see if it's all
            // spaces from there to here.
            var i = bytes.LastIndex(@in.complete[..(int)@in.pos.Byte], (slice<byte>)"\n");
            var suffix = len(bytes.TrimSpace(@in.complete[(int)i + 1..(int)@in.pos.Byte])) > 0;
            @in.readRune();
            @in.readRune(); 

            // Consume comment.
            while (len(@in.remaining) > 0 && @in.readRune() != '\n') {
            } 

            // If we are at top level (not in a statement), hand the comment to
            // the parser as a _COMMENT token. The grammar is written
            // to handle top-level comments itself.
 

            // If we are at top level (not in a statement), hand the comment to
            // the parser as a _COMMENT token. The grammar is written
            // to handle top-level comments itself.
            if (!suffix) {
                @in.endToken(_COMMENT);
                return ;
            } 

            // Otherwise, save comment for later attachment to syntax tree.
            @in.endToken(_EOLCOMMENT);
            @in.comments = append(@in.comments, new Comment(in.token.pos,in.token.text,suffix));
            return ;
        }
        if (@in.peekPrefix("/*")) {
            @in.Error("mod files must use // comments (not /* */ comments)");
        }
        break;
    } 

    // Found the beginning of the next token.
    @in.startToken(); 

    // End of file.
    if (@in.eof()) {
        @in.endToken(_EOF);
        return ;
    }
    {
        var c__prev1 = c;

        c = @in.peekRune();

        switch (c) {
            case '\n': 

            case '(': 

            case ')': 

            case '[': 

            case ']': 

            case '{': 

            case '}': 

            case ',': 
                @in.readRune();
                @in.endToken(tokenKind(c));
                return ;
                break;
            case '"': // quoted string

            case '`': // quoted string
                var quote = c;
                @in.readRune();
                while (true) {
                    if (@in.eof()) {
                        @in.pos = @in.token.pos;
                        @in.Error("unexpected EOF in string");
                    }
                    if (@in.peekRune() == '\n') {
                        @in.Error("unexpected newline in string");
                    }
                    c = @in.readRune();
                    if (c == quote) {
                        break;
                    }
                    if (c == '\\' && quote != '`') {
                        if (@in.eof()) {
                            @in.pos = @in.token.pos;
                            @in.Error("unexpected EOF in string");
                        }
                        @in.readRune();
                    }
                }

                @in.endToken(_STRING);
                return ;
                break;
        }

        c = c__prev1;
    } 

    // Checked all punctuation. Must be identifier token.
    {
        var c__prev1 = c;

        c = @in.peekRune();

        if (!isIdent(c)) {
            @in.Error(fmt.Sprintf("unexpected input character %#q", c));
        }
        c = c__prev1;

    } 

    // Scan over identifier.
    while (isIdent(@in.peekRune())) {
        if (@in.peekPrefix("//")) {
            break;
        }
        if (@in.peekPrefix("/*")) {
            @in.Error("mod files must use // comments (not /* */ comments)");
        }
        @in.readRune();
    }
    @in.endToken(_IDENT);
}

// isIdent reports whether c is an identifier rune.
// We treat most printable runes as identifier runes, except for a handful of
// ASCII punctuation characters.
private static bool isIdent(nint c) {
    {
        var r = rune(c);

        switch (r) {
            case ' ': 

            case '(': 

            case ')': 

            case '[': 

            case ']': 

            case '{': 

            case '}': 

            case ',': 
                return false;
                break;
            default: 
                return !unicode.IsSpace(r) && unicode.IsPrint(r);
                break;
        }
    }
}

// Comment assignment.
// We build two lists of all subexpressions, preorder and postorder.
// The preorder list is ordered by start location, with outer expressions first.
// The postorder list is ordered by end location, with outer expressions last.
// We use the preorder list to assign each whole-line comment to the syntax
// immediately following it, and we use the postorder list to assign each
// end-of-line comment to the syntax immediately preceding it.

// order walks the expression adding it and its subexpressions to the
// preorder and postorder lists.
private static void order(this ptr<input> _addr_@in, Expr x) => func((_, panic, _) => {
    ref input @in = ref _addr_@in.val;

    if (x != null) {
        @in.pre = append(@in.pre, x);
    }
    switch (x.type()) {
        case 
            break;
        case ptr<LParen> x:
            break;
        case ptr<RParen> x:
            break;
        case ptr<CommentBlock> x:
            break;
        case ptr<Line> x:
            break;
        case ptr<FileSyntax> x:
            foreach (var (_, stmt) in x.Stmt) {
                @in.order(stmt);
            }
            break;
        case ptr<LineBlock> x:
            @in.order(_addr_x.LParen);
            foreach (var (_, l) in x.Line) {
                @in.order(l);
            }
            @in.order(_addr_x.RParen);
            break;
        default:
        {
            var x = x.type();
            panic(fmt.Errorf("order: unexpected type %T", x));
            break;
        }
    }
    if (x != null) {
        @in.post = append(@in.post, x);
    }
});

// assignComments attaches comments to nearby syntax.
private static void assignComments(this ptr<input> _addr_@in) {
    ref input @in = ref _addr_@in.val;

    const var debug = false; 

    // Generate preorder and postorder lists.
 

    // Generate preorder and postorder lists.
    @in.order(@in.file); 

    // Split into whole-line comments and suffix comments.
    slice<Comment> line = default;    slice<Comment> suffix = default;

    foreach (var (_, com) in @in.comments) {
        if (com.Suffix) {
            suffix = append(suffix, com);
        }
        else
 {
            line = append(line, com);
        }
    }    if (debug) {
        {
            var c__prev1 = c;

            foreach (var (_, __c) in line) {
                c = __c;
                fmt.Fprintf(os.Stderr, "LINE %q :%d:%d #%d\n", c.Token, c.Start.Line, c.Start.LineRune, c.Start.Byte);
            }

            c = c__prev1;
        }
    }
    {
        var x__prev1 = x;

        foreach (var (_, __x) in @in.pre) {
            x = __x;
            var (start, _) = x.Span();
            if (debug) {
                fmt.Fprintf(os.Stderr, "pre %T :%d:%d #%d\n", x, start.Line, start.LineRune, start.Byte);
            }
            var xcom = x.Comment();
            while (len(line) > 0 && start.Byte >= line[0].Start.Byte) {
                if (debug) {
                    fmt.Fprintf(os.Stderr, "ASSIGN LINE %q #%d\n", line[0].Token, line[0].Start.Byte);
                }
                xcom.Before = append(xcom.Before, line[0]);
                line = line[(int)1..];
            }
        }
        x = x__prev1;
    }

    @in.file.After = append(@in.file.After, line);

    if (debug) {
        {
            var c__prev1 = c;

            foreach (var (_, __c) in suffix) {
                c = __c;
                fmt.Fprintf(os.Stderr, "SUFFIX %q :%d:%d #%d\n", c.Token, c.Start.Line, c.Start.LineRune, c.Start.Byte);
            }

            c = c__prev1;
        }
    }
    for (var i = len(@in.post) - 1; i >= 0; i--) {
        var x = @in.post[i];

        var (start, end) = x.Span();
        if (debug) {
            fmt.Fprintf(os.Stderr, "post %T :%d:%d #%d :%d:%d #%d\n", x, start.Line, start.LineRune, start.Byte, end.Line, end.LineRune, end.Byte);
        }
        switch (x.type()) {
            case ptr<FileSyntax> _:
                continue;
                break; 

            // Do not assign suffix comments to something that starts
            // on an earlier line, so that in
            //
            //    x ( y
            //        z ) // comment
            //
            // we assign the comment to z and not to x ( ... ).
        } 

        // Do not assign suffix comments to something that starts
        // on an earlier line, so that in
        //
        //    x ( y
        //        z ) // comment
        //
        // we assign the comment to z and not to x ( ... ).
        if (start.Line != end.Line) {
            continue;
        }
        xcom = x.Comment();
        while (len(suffix) > 0 && end.Byte <= suffix[len(suffix) - 1].Start.Byte) {
            if (debug) {
                fmt.Fprintf(os.Stderr, "ASSIGN SUFFIX %q #%d\n", suffix[len(suffix) - 1].Token, suffix[len(suffix) - 1].Start.Byte);
            }
            xcom.Suffix = append(xcom.Suffix, suffix[len(suffix) - 1]);
            suffix = suffix[..(int)len(suffix) - 1];
        }
    } 

    // We assigned suffix comments in reverse.
    // If multiple suffix comments were appended to the same
    // expression node, they are now in reverse. Fix that.
    {
        var x__prev1 = x;

        foreach (var (_, __x) in @in.post) {
            x = __x;
            reverseComments(x.Comment().Suffix);
        }
        x = x__prev1;
    }

    @in.file.Before = append(@in.file.Before, suffix);
}

// reverseComments reverses the []Comment list.
private static void reverseComments(slice<Comment> list) {
    {
        nint i = 0;
        var j = len(list) - 1;

        while (i < j) {
            (list[i], list[j]) = (list[j], list[i]);            (i, j) = (i + 1, j - 1);
        }
    }
}

private static void parseFile(this ptr<input> _addr_@in) {
    ref input @in = ref _addr_@in.val;

    @in.file = @new<FileSyntax>();
    ptr<CommentBlock> cb;
    while (true) {

        if (@in.peek() == '\n') 
            @in.lex();
            if (cb != null) {
                @in.file.Stmt = append(@in.file.Stmt, cb);
                cb = null;
            }
        else if (@in.peek() == _COMMENT) 
            var tok = @in.lex();
            if (cb == null) {
                cb = addr(new CommentBlock(Start:tok.pos));
            }
            var com = cb.Comment();
            com.Before = append(com.Before, new Comment(Start:tok.pos,Token:tok.text));
        else if (@in.peek() == _EOF) 
            if (cb != null) {
                @in.file.Stmt = append(@in.file.Stmt, cb);
            }
            return ;
        else 
            @in.parseStmt();
            if (cb != null) {
                @in.file.Stmt[len(@in.file.Stmt) - 1].Comment().Before = cb.Before;
                cb = null;
            }
            }
}

private static void parseStmt(this ptr<input> _addr_@in) {
    ref input @in = ref _addr_@in.val;

    var tok = @in.lex();
    var start = tok.pos;
    var end = tok.endPos;
    @string tokens = new slice<@string>(new @string[] { tok.text });
    while (true) {
        tok = @in.lex();

        if (tok.kind.isEOL()) 
            @in.file.Stmt = append(@in.file.Stmt, addr(new Line(Start:start,Token:tokens,End:end,)));
            return ;
        else if (tok.kind == '(') 
            {
                var next = @in.peek();

                if (next.isEOL()) { 
                    // Start of block: no more tokens on this line.
                    @in.file.Stmt = append(@in.file.Stmt, @in.parseLineBlock(start, tokens, tok));
                    return ;
                }
                else if (next == ')') {
                    var rparen = @in.lex();
                    if (@in.peek().isEOL()) { 
                        // Empty block.
                        @in.lex();
                        @in.file.Stmt = append(@in.file.Stmt, addr(new LineBlock(Start:start,Token:tokens,LParen:LParen{Pos:tok.pos},RParen:RParen{Pos:rparen.pos},)));
                        return ;
                    } 
                    // '( )' in the middle of the line, not a block.
                    tokens = append(tokens, tok.text, rparen.text);
                }
                else
 { 
                    // '(' in the middle of the line, not a block.
                    tokens = append(tokens, tok.text);
                }

            }
        else 
            tokens = append(tokens, tok.text);
            end = tok.endPos;
            }
}

private static ptr<LineBlock> parseLineBlock(this ptr<input> _addr_@in, Position start, slice<@string> token, token lparen) {
    ref input @in = ref _addr_@in.val;

    ptr<LineBlock> x = addr(new LineBlock(Start:start,Token:token,LParen:LParen{Pos:lparen.pos},));
    slice<Comment> comments = default;
    while (true) {

        if (@in.peek() == _EOLCOMMENT) 
            // Suffix comment, will be attached later by assignComments.
            @in.lex();
        else if (@in.peek() == '\n') 
            // Blank line. Add an empty comment to preserve it.
            @in.lex();
            if (len(comments) == 0 && len(x.Line) > 0 || len(comments) > 0 && comments[len(comments) - 1].Token != "") {
                comments = append(comments, new Comment());
            }
        else if (@in.peek() == _COMMENT) 
            var tok = @in.lex();
            comments = append(comments, new Comment(Start:tok.pos,Token:tok.text));
        else if (@in.peek() == _EOF) 
            @in.Error(fmt.Sprintf("syntax error (unterminated block started at %s:%d:%d)", @in.filename, x.Start.Line, x.Start.LineRune));
        else if (@in.peek() == ')') 
            var rparen = @in.lex();
            x.RParen.Before = comments;
            x.RParen.Pos = rparen.pos;
            if (!@in.peek().isEOL()) {
                @in.Error("syntax error (expected newline after closing paren)");
            }
            @in.lex();
            return _addr_x!;
        else 
            var l = @in.parseLine();
            x.Line = append(x.Line, l);
            l.Comment().Before = comments;
            comments = null;
            }
}

private static ptr<Line> parseLine(this ptr<input> _addr_@in) {
    ref input @in = ref _addr_@in.val;

    var tok = @in.lex();
    if (tok.kind.isEOL()) {
        @in.Error("internal parse error: parseLine at end of line");
    }
    var start = tok.pos;
    var end = tok.endPos;
    @string tokens = new slice<@string>(new @string[] { tok.text });
    while (true) {
        tok = @in.lex();
        if (tok.kind.isEOL()) {
            return addr(new Line(Start:start,Token:tokens,End:end,InBlock:true,));
        }
        tokens = append(tokens, tok.text);
        end = tok.endPos;
    }
}

private static slice<byte> slashSlash = (slice<byte>)"//";private static slice<byte> moduleStr = (slice<byte>)"module";

// ModulePath returns the module path from the gomod file text.
// If it cannot find a module path, it returns an empty string.
// It is tolerant of unrelated problems in the go.mod file.
public static @string ModulePath(slice<byte> mod) {
    while (len(mod) > 0) {
        var line = mod;
        mod = null;
        {
            var i__prev1 = i;

            var i = bytes.IndexByte(line, '\n');

            if (i >= 0) {
                (line, mod) = (line[..(int)i], line[(int)i + 1..]);
            }

            i = i__prev1;

        }
        {
            var i__prev1 = i;

            i = bytes.Index(line, slashSlash);

            if (i >= 0) {
                line = line[..(int)i];
            }

            i = i__prev1;

        }
        line = bytes.TrimSpace(line);
        if (!bytes.HasPrefix(line, moduleStr)) {
            continue;
        }
        line = line[(int)len(moduleStr)..];
        var n = len(line);
        line = bytes.TrimSpace(line);
        if (len(line) == n || len(line) == 0) {
            continue;
        }
        if (line[0] == '"' || line[0] == '`') {
            var (p, err) = strconv.Unquote(string(line));
            if (err != null) {
                return ""; // malformed quoted string or multiline module path
            }
            return p;
        }
        return string(line);
    }
    return ""; // missing module path
}

} // end modfile_package
