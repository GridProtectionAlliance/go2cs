// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package constraint implements parsing and evaluation of build constraint lines.
// See https://golang.org/cmd/go/#hdr-Build_constraints for documentation about build constraints themselves.
//
// This package parses both the original “// +build” syntax and the “//go:build” syntax that will be added in Go 1.17.
// The parser is being included in Go 1.16 to allow tools that need to process Go 1.17 source code
// to still be built against the Go 1.16 release.
// See https://golang.org/design/draft-gobuild for details about the “//go:build” syntax.
// package constraint -- go2cs converted at 2022 March 06 22:41:20 UTC
// import "go/build/constraint" ==> using constraint = go.go.build.constraint_package
// Original source: C:\Program Files\Go\src\go\build\constraint\expr.go
using errors = go.errors_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using System;


namespace go.go.build;

public static partial class constraint_package {

    // An Expr is a build tag constraint expression.
    // The underlying concrete type is *AndExpr, *OrExpr, *NotExpr, or *TagExpr.
public partial interface Expr {
    bool String(); // Eval reports whether the expression evaluates to true.
// It calls ok(tag) as needed to find out whether a given build tag
// is satisfied by the current build configuration.
    bool Eval(Func<@string, bool> ok); // The presence of an isExpr method explicitly marks the type as an Expr.
// Only implementations in this package should be used as Exprs.
    bool isExpr();
}

// A TagExpr is an Expr for the single tag Tag.
public partial struct TagExpr {
    public @string Tag; // for example, “linux” or “cgo”
}

private static void isExpr(this ptr<TagExpr> _addr_x) {
    ref TagExpr x = ref _addr_x.val;

}

private static bool Eval(this ptr<TagExpr> _addr_x, Func<@string, bool> ok) {
    ref TagExpr x = ref _addr_x.val;

    return ok(x.Tag);
}

private static @string String(this ptr<TagExpr> _addr_x) {
    ref TagExpr x = ref _addr_x.val;

    return x.Tag;
}

private static Expr tag(@string tag) {
    return addr(new TagExpr(tag));
}

// A NotExpr represents the expression !X (the negation of X).
public partial struct NotExpr {
    public Expr X;
}

private static void isExpr(this ptr<NotExpr> _addr_x) {
    ref NotExpr x = ref _addr_x.val;

}

private static bool Eval(this ptr<NotExpr> _addr_x, Func<@string, bool> ok) {
    ref NotExpr x = ref _addr_x.val;

    return !x.X.Eval(ok);
}

private static @string String(this ptr<NotExpr> _addr_x) {
    ref NotExpr x = ref _addr_x.val;

    var s = x.X.String();
    switch (x.X.type()) {
        case ptr<AndExpr> _:
            s = "(" + s + ")";
            break;
        case ptr<OrExpr> _:
            s = "(" + s + ")";
            break;
    }
    return "!" + s;

}

private static Expr not(Expr x) {
    return addr(new NotExpr(x));
}

// An AndExpr represents the expression X && Y.
public partial struct AndExpr {
    public Expr X;
    public Expr Y;
}

private static void isExpr(this ptr<AndExpr> _addr_x) {
    ref AndExpr x = ref _addr_x.val;

}

private static bool Eval(this ptr<AndExpr> _addr_x, Func<@string, bool> ok) {
    ref AndExpr x = ref _addr_x.val;
 
    // Note: Eval both, to make sure ok func observes all tags.
    var xok = x.X.Eval(ok);
    var yok = x.Y.Eval(ok);
    return xok && yok;

}

private static @string String(this ptr<AndExpr> _addr_x) {
    ref AndExpr x = ref _addr_x.val;

    return andArg(x.X) + " && " + andArg(x.Y);
}

private static @string andArg(Expr x) {
    var s = x.String();
    {
        ptr<OrExpr> (_, ok) = x._<ptr<OrExpr>>();

        if (ok) {
            s = "(" + s + ")";
        }
    }

    return s;

}

private static Expr and(Expr x, Expr y) {
    return addr(new AndExpr(x,y));
}

// An OrExpr represents the expression X || Y.
public partial struct OrExpr {
    public Expr X;
    public Expr Y;
}

private static void isExpr(this ptr<OrExpr> _addr_x) {
    ref OrExpr x = ref _addr_x.val;

}

private static bool Eval(this ptr<OrExpr> _addr_x, Func<@string, bool> ok) {
    ref OrExpr x = ref _addr_x.val;
 
    // Note: Eval both, to make sure ok func observes all tags.
    var xok = x.X.Eval(ok);
    var yok = x.Y.Eval(ok);
    return xok || yok;

}

private static @string String(this ptr<OrExpr> _addr_x) {
    ref OrExpr x = ref _addr_x.val;

    return orArg(x.X) + " || " + orArg(x.Y);
}

private static @string orArg(Expr x) {
    var s = x.String();
    {
        ptr<AndExpr> (_, ok) = x._<ptr<AndExpr>>();

        if (ok) {
            s = "(" + s + ")";
        }
    }

    return s;

}

private static Expr or(Expr x, Expr y) {
    return addr(new OrExpr(x,y));
}

// A SyntaxError reports a syntax error in a parsed build expression.
public partial struct SyntaxError {
    public nint Offset; // byte offset in input where error was detected
    public @string Err; // description of error
}

private static @string Error(this ptr<SyntaxError> _addr_e) {
    ref SyntaxError e = ref _addr_e.val;

    return e.Err;
}

private static var errNotConstraint = errors.New("not a build constraint");

// Parse parses a single build constraint line of the form “//go:build ...” or “// +build ...”
// and returns the corresponding boolean expression.
public static (Expr, error) Parse(@string line) {
    Expr _p0 = default;
    error _p0 = default!;

    {
        var text__prev1 = text;

        var (text, ok) = splitGoBuild(line);

        if (ok) {
            return parseExpr(text);
        }
        text = text__prev1;

    }

    {
        var text__prev1 = text;

        (text, ok) = splitPlusBuild(line);

        if (ok) {
            return (parsePlusBuildExpr(text), error.As(null!)!);
        }
        text = text__prev1;

    }

    return (null, error.As(errNotConstraint)!);

}

// IsGoBuild reports whether the line of text is a “//go:build” constraint.
// It only checks the prefix of the text, not that the expression itself parses.
public static bool IsGoBuild(@string line) {
    var (_, ok) = splitGoBuild(line);
    return ok;
}

// splitGoBuild splits apart the leading //go:build prefix in line from the build expression itself.
// It returns "", false if the input is not a //go:build line or if the input contains multiple lines.
private static (@string, bool) splitGoBuild(@string line) {
    @string expr = default;
    bool ok = default;
 
    // A single trailing newline is OK; otherwise multiple lines are not.
    if (len(line) > 0 && line[len(line) - 1] == '\n') {
        line = line[..(int)len(line) - 1];
    }
    if (strings.Contains(line, "\n")) {
        return ("", false);
    }
    if (!strings.HasPrefix(line, "//go:build")) {
        return ("", false);
    }
    line = strings.TrimSpace(line);
    line = line[(int)len("//go:build")..]; 

    // If strings.TrimSpace finds more to trim after removing the //go:build prefix,
    // it means that the prefix was followed by a space, making this a //go:build line
    // (as opposed to a //go:buildsomethingelse line).
    // If line is empty, we had "//go:build" by itself, which also counts.
    var trim = strings.TrimSpace(line);
    if (len(line) == len(trim) && line != "") {
        return ("", false);
    }
    return (trim, true);

}

// An exprParser holds state for parsing a build expression.
private partial struct exprParser {
    public @string s; // input string
    public nint i; // next read location in s

    public @string tok; // last token read
    public bool isTag;
    public nint pos; // position (start) of last token
}

// parseExpr parses a boolean build tag expression.
private static (Expr, error) parseExpr(@string text) => func((defer, panic, _) => {
    Expr x = default;
    error err = default!;

    defer(() => {
        {
            var e__prev1 = e;

            var e = recover();

            if (e != null) {
                {
                    var e__prev2 = e;

                    ptr<SyntaxError> (e, ok) = e._<ptr<SyntaxError>>();

                    if (ok) {
                        err = e;
                        return ;
                    }

                    e = e__prev2;

                }

                panic(e); // unreachable unless parser has a bug
            }

            e = e__prev1;

        }

    }());

    ptr<exprParser> p = addr(new exprParser(s:text));
    x = p.or();
    if (p.tok != "") {
        panic(addr(new SyntaxError(Offset:p.pos,Err:"unexpected token "+p.tok)));
    }
    return (x, error.As(null!)!);

});

// or parses a sequence of || expressions.
// On entry, the next input token has not yet been lexed.
// On exit, the next input token has been lexed and is in p.tok.
private static Expr or(this ptr<exprParser> _addr_p) {
    ref exprParser p = ref _addr_p.val;

    var x = p.and();
    while (p.tok == "||") {
        x = or(x, p.and());
    }
    return x;
}

// and parses a sequence of && expressions.
// On entry, the next input token has not yet been lexed.
// On exit, the next input token has been lexed and is in p.tok.
private static Expr and(this ptr<exprParser> _addr_p) {
    ref exprParser p = ref _addr_p.val;

    var x = p.not();
    while (p.tok == "&&") {
        x = and(x, p.not());
    }
    return x;
}

// not parses a ! expression.
// On entry, the next input token has not yet been lexed.
// On exit, the next input token has been lexed and is in p.tok.
private static Expr not(this ptr<exprParser> _addr_p) => func((_, panic, _) => {
    ref exprParser p = ref _addr_p.val;

    p.lex();
    if (p.tok == "!") {
        p.lex();
        if (p.tok == "!") {
            panic(addr(new SyntaxError(Offset:p.pos,Err:"double negation not allowed")));
        }
        return not(p.atom());

    }
    return p.atom();

});

// atom parses a tag or a parenthesized expression.
// On entry, the next input token HAS been lexed.
// On exit, the next input token has been lexed and is in p.tok.
private static Expr atom(this ptr<exprParser> _addr_p) => func((defer, panic, _) => {
    ref exprParser p = ref _addr_p.val;
 
    // first token already in p.tok
    if (p.tok == "(") {
        var pos = p.pos;
        defer(() => {
            {
                var e__prev2 = e;

                var e = recover();

                if (e != null) {
                    {
                        var e__prev3 = e;

                        ptr<SyntaxError> (e, ok) = e._<ptr<SyntaxError>>();

                        if (ok && e.Err == "unexpected end of expression") {
                            e.Err = "missing close paren";
                        }

                        e = e__prev3;

                    }

                    panic(e);

                }

                e = e__prev2;

            }

        }());
        var x = p.or();
        if (p.tok != ")") {
            panic(addr(new SyntaxError(Offset:pos,Err:"missing close paren")));
        }
        p.lex();
        return x;

    }
    if (!p.isTag) {
        if (p.tok == "") {
            panic(addr(new SyntaxError(Offset:p.pos,Err:"unexpected end of expression")));
        }
        panic(addr(new SyntaxError(Offset:p.pos,Err:"unexpected token "+p.tok)));

    }
    var tok = p.tok;
    p.lex();
    return tag(tok);

});

// lex finds and consumes the next token in the input stream.
// On return, p.tok is set to the token text,
// p.isTag reports whether the token was a tag,
// and p.pos records the byte offset of the start of the token in the input stream.
// If lex reaches the end of the input, p.tok is set to the empty string.
// For any other syntax error, lex panics with a SyntaxError.
private static void lex(this ptr<exprParser> _addr_p) => func((_, panic, _) => {
    ref exprParser p = ref _addr_p.val;

    p.isTag = false;
    while (p.i < len(p.s) && (p.s[p.i] == ' ' || p.s[p.i] == '\t')) {
        p.i++;
    }
    if (p.i >= len(p.s)) {
        p.tok = "";
        p.pos = p.i;
        return ;
    }
    switch (p.s[p.i]) {
        case '(': 

        case ')': 

        case '!': 
            p.pos = p.i;
            p.i++;
            p.tok = p.s[(int)p.pos..(int)p.i];
            return ;
            break;
        case '&': 

        case '|': 
            if (p.i + 1 >= len(p.s) || p.s[p.i + 1] != p.s[p.i]) {
                panic(addr(new SyntaxError(Offset:p.i,Err:"invalid syntax at "+string(rune(p.s[p.i])))));
            }
            p.pos = p.i;
            p.i += 2;
            p.tok = p.s[(int)p.pos..(int)p.i];
            return ;

            break;
    }

    var tag = p.s[(int)p.i..];
    {
        var c__prev1 = c;

        foreach (var (__i, __c) in tag) {
            i = __i;
            c = __c;
            if (!unicode.IsLetter(c) && !unicode.IsDigit(c) && c != '_' && c != '.') {
                tag = tag[..(int)i];
                break;
            }
        }
        c = c__prev1;
    }

    if (tag == "") {
        var (c, _) = utf8.DecodeRuneInString(p.s[(int)p.i..]);
        panic(addr(new SyntaxError(Offset:p.i,Err:"invalid syntax at "+string(c))));
    }
    p.pos = p.i;
    p.i += len(tag);
    p.tok = p.s[(int)p.pos..(int)p.i];
    p.isTag = true;
    return ;

});

// IsPlusBuild reports whether the line of text is a “// +build” constraint.
// It only checks the prefix of the text, not that the expression itself parses.
public static bool IsPlusBuild(@string line) {
    var (_, ok) = splitPlusBuild(line);
    return ok;
}

// splitPlusBuild splits apart the leading // +build prefix in line from the build expression itself.
// It returns "", false if the input is not a // +build line or if the input contains multiple lines.
private static (@string, bool) splitPlusBuild(@string line) {
    @string expr = default;
    bool ok = default;
 
    // A single trailing newline is OK; otherwise multiple lines are not.
    if (len(line) > 0 && line[len(line) - 1] == '\n') {
        line = line[..(int)len(line) - 1];
    }
    if (strings.Contains(line, "\n")) {
        return ("", false);
    }
    if (!strings.HasPrefix(line, "//")) {
        return ("", false);
    }
    line = line[(int)len("//")..]; 
    // Note the space is optional; "//+build" is recognized too.
    line = strings.TrimSpace(line);

    if (!strings.HasPrefix(line, "+build")) {
        return ("", false);
    }
    line = line[(int)len("+build")..]; 

    // If strings.TrimSpace finds more to trim after removing the +build prefix,
    // it means that the prefix was followed by a space, making this a +build line
    // (as opposed to a +buildsomethingelse line).
    // If line is empty, we had "// +build" by itself, which also counts.
    var trim = strings.TrimSpace(line);
    if (len(line) == len(trim) && line != "") {
        return ("", false);
    }
    return (trim, true);

}

// parsePlusBuildExpr parses a legacy build tag expression (as used with “// +build”).
private static Expr parsePlusBuildExpr(@string text) {
    Expr x = default!;
    foreach (var (_, clause) in strings.Fields(text)) {
        Expr y = default!;
        foreach (var (_, lit) in strings.Split(clause, ",")) {
            Expr z = default!;
            bool neg = default;
            if (strings.HasPrefix(lit, "!!") || lit == "!") {
                z = Expr.As(tag("ignore"))!;
            }
            else
 {
                if (strings.HasPrefix(lit, "!")) {
                    neg = true;
                    lit = lit[(int)len("!")..];
                }
                if (isValidTag(lit)) {
                    z = Expr.As(tag(lit))!;
                }
                else
 {
                    z = Expr.As(tag("ignore"))!;
                }

                if (neg) {
                    z = Expr.As(not(z))!;
                }

            }

            if (y == null) {
                y = Expr.As(z)!;
            }
            else
 {
                y = Expr.As(and(y, z))!;
            }

        }        if (x == null) {
            x = Expr.As(y)!;
        }
        else
 {
            x = Expr.As(or(x, y))!;
        }
    }    if (x == null) {
        x = Expr.As(tag("ignore"))!;
    }
    return x;

}

// isValidTag reports whether the word is a valid build tag.
// Tags must be letters, digits, underscores or dots.
// Unlike in Go identifiers, all digits are fine (e.g., "386").
private static bool isValidTag(@string word) {
    if (word == "") {
        return false;
    }
    foreach (var (_, c) in word) {
        if (!unicode.IsLetter(c) && !unicode.IsDigit(c) && c != '_' && c != '.') {
            return false;
        }
    }    return true;

}

private static var errComplex = errors.New("expression too complex for // +build lines");

// PlusBuildLines returns a sequence of “// +build” lines that evaluate to the build expression x.
// If the expression is too complex to convert directly to “// +build” lines, PlusBuildLines returns an error.
public static (slice<@string>, error) PlusBuildLines(Expr x) {
    slice<@string> _p0 = default;
    error _p0 = default!;
 
    // Push all NOTs to the expression leaves, so that //go:build !(x && y) can be treated as !x || !y.
    // This rewrite is both efficient and commonly needed, so it's worth doing.
    // Essentially all other possible rewrites are too expensive and too rarely needed.
    x = pushNot(x, false); 

    // Split into AND of ORs of ANDs of literals (tag or NOT tag).
    slice<slice<slice<Expr>>> split = default;
    {
        var or__prev1 = or;

        foreach (var (_, __or) in appendSplitAnd(null, x)) {
            or = __or;
            slice<slice<Expr>> ands = default;
            {
                var and__prev2 = and;

                foreach (var (_, __and) in appendSplitOr(null, or)) {
                    and = __and;
                    slice<Expr> lits = default;
                    {
                        var lit__prev3 = lit;

                        foreach (var (_, __lit) in appendSplitAnd(null, and)) {
                            lit = __lit;
                            switch (lit.type()) {
                                case ptr<TagExpr> _:
                                    lits = append(lits, lit);
                                    break;
                                case ptr<NotExpr> _:
                                    lits = append(lits, lit);
                                    break;
                                default:
                                {
                                    return (null, error.As(errComplex)!);
                                    break;
                                }
                            }

                        }

                        lit = lit__prev3;
                    }

                    ands = append(ands, lits);

                }

                and = and__prev2;
            }

            split = append(split, ands);

        }
        or = or__prev1;
    }

    nint maxOr = 0;
    {
        var or__prev1 = or;

        foreach (var (_, __or) in split) {
            or = __or;
            if (maxOr < len(or)) {
                maxOr = len(or);
            }
        }
        or = or__prev1;
    }

    if (maxOr == 1) {
        lits = default;
        {
            var or__prev1 = or;

            foreach (var (_, __or) in split) {
                or = __or;
                lits = append(lits, or[0]);
            }

            or = or__prev1;
        }

        split = new slice<slice<slice<Expr>>>(new slice<slice<Expr>>[] { {lits} });

    }
    slice<@string> lines = default;
    {
        var or__prev1 = or;

        foreach (var (_, __or) in split) {
            or = __or;
            @string line = "// +build";
            {
                var and__prev2 = and;

                foreach (var (_, __and) in or) {
                    and = __and;
                    @string clause = "";
                    {
                        var lit__prev3 = lit;

                        foreach (var (__i, __lit) in and) {
                            i = __i;
                            lit = __lit;
                            if (i > 0) {
                                clause += ",";
                            }
                            clause += lit.String();
                        }

                        lit = lit__prev3;
                    }

                    line += " " + clause;

                }

                and = and__prev2;
            }

            lines = append(lines, line);

        }
        or = or__prev1;
    }

    return (lines, error.As(null!)!);

}

// pushNot applies DeMorgan's law to push negations down the expression,
// so that only tags are negated in the result.
// (It applies the rewrites !(X && Y) => (!X || !Y) and !(X || Y) => (!X && !Y).)
private static Expr pushNot(Expr x, bool not) {
    switch (x.type()) {
        case ptr<NotExpr> x:
            {
                ptr<TagExpr> (_, ok) = x.X._<ptr<TagExpr>>();

                if (ok && !not) {
                    return x;
                }

            }

            return pushNot(x.X, !not);
            break;
        case ptr<TagExpr> x:
            if (not) {
                return addr(new NotExpr(X:x));
            }
            return x;
            break;
        case ptr<AndExpr> x:
            var x1 = pushNot(x.X, not);
            var y1 = pushNot(x.Y, not);
            if (not) {
                return or(x1, y1);
            }
            if (x1 == x.X && y1 == x.Y) {
                return x;
            }
            return and(x1, y1);
            break;
        case ptr<OrExpr> x:
            x1 = pushNot(x.X, not);
            y1 = pushNot(x.Y, not);
            if (not) {
                return and(x1, y1);
            }
            if (x1 == x.X && y1 == x.Y) {
                return x;
            }
            return or(x1, y1);
            break;
        default:
        {
            var x = x.type();
            return x;
            break;
        }
    }

}

// appendSplitAnd appends x to list while splitting apart any top-level && expressions.
// For example, appendSplitAnd({W}, X && Y && Z) = {W, X, Y, Z}.
private static slice<Expr> appendSplitAnd(slice<Expr> list, Expr x) {
    {
        ptr<AndExpr> (x, ok) = x._<ptr<AndExpr>>();

        if (ok) {
            list = appendSplitAnd(list, x.X);
            list = appendSplitAnd(list, x.Y);
            return list;
        }
    }

    return append(list, x);

}

// appendSplitOr appends x to list while splitting apart any top-level || expressions.
// For example, appendSplitOr({W}, X || Y || Z) = {W, X, Y, Z}.
private static slice<Expr> appendSplitOr(slice<Expr> list, Expr x) {
    {
        ptr<OrExpr> (x, ok) = x._<ptr<OrExpr>>();

        if (ok) {
            list = appendSplitOr(list, x.X);
            list = appendSplitOr(list, x.Y);
            return list;
        }
    }

    return append(list, x);

}

} // end constraint_package
