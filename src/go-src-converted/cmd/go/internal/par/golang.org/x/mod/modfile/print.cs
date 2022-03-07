// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Module file printer.

// package modfile -- go2cs converted at 2022 March 06 23:16:43 UTC
// import "golang.org/x/mod/modfile" ==> using modfile = go.golang.org.x.mod.modfile_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\mod\modfile\print.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using strings = go.strings_package;

namespace go.golang.org.x.mod;

public static partial class modfile_package {

    // Format returns a go.mod file as a byte slice, formatted in standard style.
public static slice<byte> Format(ptr<FileSyntax> _addr_f) {
    ref FileSyntax f = ref _addr_f.val;

    ptr<printer> pr = addr(new printer());
    pr.file(f);
    return pr.Bytes();
}

// A printer collects the state during printing of a file or expression.
private partial struct printer {
    public ref bytes.Buffer Buffer => ref Buffer_val; // output buffer
    public slice<Comment> comment; // pending end-of-line comments
    public nint margin; // left margin (indent), a number of tabs
}

// printf prints to the buffer.
private static void printf(this ptr<printer> _addr_p, @string format, params object[] args) {
    args = args.Clone();
    ref printer p = ref _addr_p.val;

    fmt.Fprintf(p, format, args);
}

// indent returns the position on the current line, in bytes, 0-indexed.
private static nint indent(this ptr<printer> _addr_p) {
    ref printer p = ref _addr_p.val;

    var b = p.Bytes();
    nint n = 0;
    while (n < len(b) && b[len(b) - 1 - n] != '\n') {
        n++;
    }
    return n;
}

// newline ends the current line, flushing end-of-line comments.
private static void newline(this ptr<printer> _addr_p) {
    ref printer p = ref _addr_p.val;

    if (len(p.comment) > 0) {
        p.printf(" ");
        {
            var i__prev1 = i;

            foreach (var (__i, __com) in p.comment) {
                i = __i;
                com = __com;
                if (i > 0) {
                    p.trim();
                    p.printf("\n");
                    {
                        var i__prev2 = i;

                        for (nint i = 0; i < p.margin; i++) {
                            p.printf("\t");
                        }


                        i = i__prev2;
                    }
                }
                p.printf("%s", strings.TrimSpace(com.Token));
            }

            i = i__prev1;
        }

        p.comment = p.comment[..(int)0];
    }
    p.trim();
    p.printf("\n");
    {
        var i__prev1 = i;

        for (i = 0; i < p.margin; i++) {
            p.printf("\t");
        }

        i = i__prev1;
    }
}

// trim removes trailing spaces and tabs from the current line.
private static void trim(this ptr<printer> _addr_p) {
    ref printer p = ref _addr_p.val;
 
    // Remove trailing spaces and tabs from line we're about to end.
    var b = p.Bytes();
    var n = len(b);
    while (n > 0 && (b[n - 1] == '\t' || b[n - 1] == ' ')) {
        n--;
    }
    p.Truncate(n);
}

// file formats the given file into the print buffer.
private static void file(this ptr<printer> _addr_p, ptr<FileSyntax> _addr_f) {
    ref printer p = ref _addr_p.val;
    ref FileSyntax f = ref _addr_f.val;

    {
        var com__prev1 = com;

        foreach (var (_, __com) in f.Before) {
            com = __com;
            p.printf("%s", strings.TrimSpace(com.Token));
            p.newline();
        }
        com = com__prev1;
    }

    foreach (var (i, stmt) in f.Stmt) {
        switch (stmt.type()) {
            case ptr<CommentBlock> x:
                p.expr(x);
                break;
            default:
            {
                var x = stmt.type();
                p.expr(x);
                p.newline();
                break;
            }

        }

        {
            var com__prev2 = com;

            foreach (var (_, __com) in stmt.Comment().After) {
                com = __com;
                p.printf("%s", strings.TrimSpace(com.Token));
                p.newline();
            }

            com = com__prev2;
        }

        if (i + 1 < len(f.Stmt)) {
            p.newline();
        }
    }
}

private static void expr(this ptr<printer> _addr_p, Expr x) => func((_, panic, _) => {
    ref printer p = ref _addr_p.val;
 
    // Emit line-comments preceding this expression.
    {
        var before = x.Comment().Before;

        if (len(before) > 0) { 
            // Want to print a line comment.
            // Line comments must be at the current margin.
            p.trim();
            if (p.indent() > 0) { 
                // There's other text on the line. Start a new line.
                p.printf("\n");
            } 
            // Re-indent to margin.
            for (nint i = 0; i < p.margin; i++) {
                p.printf("\t");
            }

            foreach (var (_, com) in before) {
                p.printf("%s", strings.TrimSpace(com.Token));
                p.newline();
            }
        }
    }

    switch (x.type()) {
        case ptr<CommentBlock> x:
            break;
        case ptr<LParen> x:
            p.printf("(");
            break;
        case ptr<RParen> x:
            p.printf(")");
            break;
        case ptr<Line> x:
            p.tokens(x.Token);
            break;
        case ptr<LineBlock> x:
            p.tokens(x.Token);
            p.printf(" ");
            p.expr(_addr_x.LParen);
            p.margin++;
            foreach (var (_, l) in x.Line) {
                p.newline();
                p.expr(l);
            }
            p.margin--;
            p.newline();
            p.expr(_addr_x.RParen);
            break;
        default:
        {
            var x = x.type();
            panic(fmt.Errorf("printer: unexpected type %T", x));
            break;
        } 

        // Queue end-of-line comments for printing when we
        // reach the end of the line.
    } 

    // Queue end-of-line comments for printing when we
    // reach the end of the line.
    p.comment = append(p.comment, x.Comment().Suffix);
});

private static void tokens(this ptr<printer> _addr_p, slice<@string> tokens) {
    ref printer p = ref _addr_p.val;

    @string sep = "";
    foreach (var (_, t) in tokens) {
        if (t == "," || t == ")" || t == "]" || t == "}") {
            sep = "";
        }
        p.printf("%s%s", sep, t);
        sep = " ";
        if (t == "(" || t == "[" || t == "{") {
            sep = "";
        }
    }
}

} // end modfile_package
