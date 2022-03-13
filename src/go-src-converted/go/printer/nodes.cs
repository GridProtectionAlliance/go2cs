// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements printing of AST nodes; specifically
// expressions, statements, declarations, and files. It uses
// the print functionality implemented in printer.go.

// package printer -- go2cs converted at 2022 March 13 05:58:21 UTC
// import "go/printer" ==> using printer = go.go.printer_package
// Original source: C:\Program Files\Go\src\go\printer\nodes.go
namespace go.go;

using bytes = bytes_package;
using ast = go.ast_package;
using typeparams = go.@internal.typeparams_package;
using token = go.token_package;
using math = math_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;


// Formatting issues:
// - better comment formatting for /*-style comments at the end of a line (e.g. a declaration)
//   when the comment spans multiple lines; if such a comment is just two lines, formatting is
//   not idempotent
// - formatting of expression lists
// - should use blank instead of tab to separate one-line function bodies from
//   the function header unless there is a group of consecutive one-liners

// ----------------------------------------------------------------------------
// Common AST nodes.

// Print as many newlines as necessary (but at least min newlines) to get to
// the current line. ws is printed before the first line break. If newSection
// is set, the first line break is printed as formfeed. Returns 0 if no line
// breaks were printed, returns 1 if there was exactly one newline printed,
// and returns a value > 1 if there was a formfeed or more than one newline
// printed.
//
// TODO(gri): linebreak may add too many lines if the next statement at "line"
//            is preceded by comments because the computation of n assumes
//            the current position before the comment and the target position
//            after the comment. Thus, after interspersing such comments, the
//            space taken up by them is not considered to reduce the number of
//            linebreaks. At the moment there is no easy way to know about
//            future (not yet interspersed) comments in this function.
//

using System;
public static partial class printer_package {

private static nint linebreak(this ptr<printer> _addr_p, nint line, nint min, whiteSpace ws, bool newSection) {
    nint nbreaks = default;
    ref printer p = ref _addr_p.val;

    var n = nlimit(line - p.pos.Line);
    if (n < min) {
        n = min;
    }
    if (n > 0) {
        p.print(ws);
        if (newSection) {
            p.print(formfeed);
            n--;
            nbreaks = 2;
        }
        nbreaks += n;
        while (n > 0) {
            p.print(newline);
            n--;
        }
    }
    return ;
}

// setComment sets g as the next comment if g != nil and if node comments
// are enabled - this mode is used when printing source code fragments such
// as exports only. It assumes that there is no pending comment in p.comments
// and at most one pending comment in the p.comment cache.
private static void setComment(this ptr<printer> _addr_p, ptr<ast.CommentGroup> _addr_g) {
    ref printer p = ref _addr_p.val;
    ref ast.CommentGroup g = ref _addr_g.val;

    if (g == null || !p.useNodeComments) {
        return ;
    }
    if (p.comments == null) { 
        // initialize p.comments lazily
        p.comments = make_slice<ptr<ast.CommentGroup>>(1);
    }
    else if (p.cindex < len(p.comments)) { 
        // for some reason there are pending comments; this
        // should never happen - handle gracefully and flush
        // all comments up to g, ignore anything after that
        p.flush(p.posFor(g.List[0].Pos()), token.ILLEGAL);
        p.comments = p.comments[(int)0..(int)1]; 
        // in debug mode, report error
        p.internalError("setComment found pending comments");
    }
    p.comments[0] = g;
    p.cindex = 0; 
    // don't overwrite any pending comment in the p.comment cache
    // (there may be a pending comment when a line comment is
    // immediately followed by a lead comment with no other
    // tokens between)
    if (p.commentOffset == infinity) {
        p.nextComment(); // get comment ready for use
    }
}

private partial struct exprListMode { // : nuint
}

private static readonly exprListMode commaTerm = 1 << (int)(iota); // list is optionally terminated by a comma
private static readonly var noIndent = 0; // no extra indentation in multi-line lists

// If indent is set, a multi-line identifier list is indented after the
// first linebreak encountered.
private static void identList(this ptr<printer> _addr_p, slice<ptr<ast.Ident>> list, bool indent) {
    ref printer p = ref _addr_p.val;
 
    // convert into an expression list so we can re-use exprList formatting
    var xlist = make_slice<ast.Expr>(len(list));
    foreach (var (i, x) in list) {
        xlist[i] = x;
    }    exprListMode mode = default;
    if (!indent) {
        mode = noIndent;
    }
    p.exprList(token.NoPos, xlist, 1, mode, token.NoPos, false);
}

private static readonly @string filteredMsg = "contains filtered or unexported fields";

// Print a list of expressions. If the list spans multiple
// source lines, the original line breaks are respected between
// expressions.
//
// TODO(gri) Consider rewriting this to be independent of []ast.Expr
//           so that we can use the algorithm for any kind of list
//           (e.g., pass list via a channel over which to range).


// Print a list of expressions. If the list spans multiple
// source lines, the original line breaks are respected between
// expressions.
//
// TODO(gri) Consider rewriting this to be independent of []ast.Expr
//           so that we can use the algorithm for any kind of list
//           (e.g., pass list via a channel over which to range).
private static void exprList(this ptr<printer> _addr_p, token.Pos prev0, slice<ast.Expr> list, nint depth, exprListMode mode, token.Pos next0, bool isIncomplete) {
    ref printer p = ref _addr_p.val;

    if (len(list) == 0) {
        if (isIncomplete) {
            var prev = p.posFor(prev0);
            var next = p.posFor(next0);
            if (prev.IsValid() && prev.Line == next.Line) {
                p.print("/* " + filteredMsg + " */");
            }
            else
 {
                p.print(newline);
                p.print(indent, "// " + filteredMsg, unindent, newline);
            }
        }
        return ;
    }
    prev = p.posFor(prev0);
    next = p.posFor(next0);
    var line = p.lineFor(list[0].Pos());
    var endLine = p.lineFor(list[len(list) - 1].End());

    if (prev.IsValid() && prev.Line == line && line == endLine) { 
        // all list entries on a single line
        {
            var i__prev1 = i;
            var x__prev1 = x;

            foreach (var (__i, __x) in list) {
                i = __i;
                x = __x;
                if (i > 0) { 
                    // use position of expression following the comma as
                    // comma position for correct comment placement
                    p.print(x.Pos(), token.COMMA, blank);
                }
                p.expr0(x, depth);
            }

            i = i__prev1;
            x = x__prev1;
        }

        if (isIncomplete) {
            p.print(token.COMMA, blank, "/* " + filteredMsg + " */");
        }
        return ;
    }
    var ws = ignore;
    if (mode & noIndent == 0) {
        ws = indent;
    }
    nint prevBreak = -1; // index of last expression that was followed by a linebreak
    if (prev.IsValid() && prev.Line < line && p.linebreak(line, 0, ws, true) > 0) {
        ws = ignore;
        prevBreak = 0;
    }
    nint size = 0; 

    // We use the ratio between the geometric mean of the previous key sizes and
    // the current size to determine if there should be a break in the alignment.
    // To compute the geometric mean we accumulate the ln(size) values (lnsum)
    // and the number of sizes included (count).
    float lnsum = 0.0F;
    nint count = 0; 

    // print all list elements
    var prevLine = prev.Line;
    {
        var i__prev1 = i;
        var x__prev1 = x;

        foreach (var (__i, __x) in list) {
            i = __i;
            x = __x;
            line = p.lineFor(x.Pos()); 

            // Determine if the next linebreak, if any, needs to use formfeed:
            // in general, use the entire node size to make the decision; for
            // key:value expressions, use the key size.
            // TODO(gri) for a better result, should probably incorporate both
            //           the key and the node size into the decision process
            var useFF = true; 

            // Determine element size: All bets are off if we don't have
            // position information for the previous and next token (likely
            // generated code - simply ignore the size in this case by setting
            // it to 0).
            var prevSize = size;
            const float infinity = 1e6F; // larger than any source line
 // larger than any source line
            size = p.nodeSize(x, infinity);
            ptr<ast.KeyValueExpr> (pair, isPair) = x._<ptr<ast.KeyValueExpr>>();
            if (size <= infinity && prev.IsValid() && next.IsValid()) { 
                // x fits on a single line
                if (isPair) {
                    size = p.nodeSize(pair.Key, infinity); // size <= infinity
                }
            }
            else
 { 
                // size too large or we don't have good layout information
                size = 0;
            } 

            // If the previous line and the current line had single-
            // line-expressions and the key sizes are small or the
            // ratio between the current key and the geometric mean
            // if the previous key sizes does not exceed a threshold,
            // align columns and do not use formfeed.
            if (prevSize > 0 && size > 0) {
                const nint smallSize = 40;

                if (count == 0 || prevSize <= smallSize && size <= smallSize) {
                    useFF = false;
                }
                else
 {
                    const float r = 2.5F; // threshold
 // threshold
                    var geomean = math.Exp(lnsum / float64(count)); // count > 0
                    var ratio = float64(size) / geomean;
                    useFF = r * ratio <= 1 || r <= ratio;
                }
            }
            nint needsLinebreak = 0 < prevLine && prevLine < line;
            if (i > 0) { 
                // Use position of expression following the comma as
                // comma position for correct comment placement, but
                // only if the expression is on the same line.
                if (!needsLinebreak) {
                    p.print(x.Pos());
                }
                p.print(token.COMMA);
                var needsBlank = true;
                if (needsLinebreak) { 
                    // Lines are broken using newlines so comments remain aligned
                    // unless useFF is set or there are multiple expressions on
                    // the same line in which case formfeed is used.
                    var nbreaks = p.linebreak(line, 0, ws, useFF || prevBreak + 1 < i);
                    if (nbreaks > 0) {
                        ws = ignore;
                        prevBreak = i;
                        needsBlank = false; // we got a line break instead
                    } 
                    // If there was a new section or more than one new line
                    // (which means that the tabwriter will implicitly break
                    // the section), reset the geomean variables since we are
                    // starting a new group of elements with the next element.
                    if (nbreaks > 1) {
                        lnsum = 0;
                        count = 0;
                    }
                }
                if (needsBlank) {
                    p.print(blank);
                }
            }
            if (len(list) > 1 && isPair && size > 0 && needsLinebreak) { 
                // We have a key:value expression that fits onto one line
                // and it's not on the same line as the prior expression:
                // Use a column for the key such that consecutive entries
                // can align if possible.
                // (needsLinebreak is set if we started a new line before)
                p.expr(pair.Key);
                p.print(pair.Colon, token.COLON, vtab);
                p.expr(pair.Value);
            }
            else
 {
                p.expr0(x, depth);
            }
            if (size > 0) {
                lnsum += math.Log(float64(size));
                count++;
            }
            prevLine = line;
        }
        i = i__prev1;
        x = x__prev1;
    }

    if (mode & commaTerm != 0 && next.IsValid() && p.pos.Line < next.Line) { 
        // Print a terminating comma if the next token is on a new line.
        p.print(token.COMMA);
        if (isIncomplete) {
            p.print(newline);
            p.print("// " + filteredMsg);
        }
        if (ws == ignore && mode & noIndent == 0) { 
            // unindent if we indented
            p.print(unindent);
        }
        p.print(formfeed); // terminating comma needs a line break to look good
        return ;
    }
    if (isIncomplete) {
        p.print(token.COMMA, newline);
        p.print("// " + filteredMsg, newline);
    }
    if (ws == ignore && mode & noIndent == 0) { 
        // unindent if we indented
        p.print(unindent);
    }
}

private static void parameters(this ptr<printer> _addr_p, ptr<ast.FieldList> _addr_fields, bool isTypeParam) {
    ref printer p = ref _addr_p.val;
    ref ast.FieldList fields = ref _addr_fields.val;

    var openTok = token.LPAREN;
    var closeTok = token.RPAREN;
    if (isTypeParam) {
        (openTok, closeTok) = (token.LBRACK, token.RBRACK);
    }
    p.print(fields.Opening, openTok);
    if (len(fields.List) > 0) {
        var prevLine = p.lineFor(fields.Opening);
        var ws = indent;
        foreach (var (i, par) in fields.List) { 
            // determine par begin and end line (may be different
            // if there are multiple parameter names for this par
            // or the type is on a separate line)
            var parLineBeg = p.lineFor(par.Pos());
            var parLineEnd = p.lineFor(par.End()); 
            // separating "," if needed
            nint needsLinebreak = 0 < prevLine && prevLine < parLineBeg;
            if (i > 0) { 
                // use position of parameter following the comma as
                // comma position for correct comma placement, but
                // only if the next parameter is on the same line
                if (!needsLinebreak) {
                    p.print(par.Pos());
                }
                p.print(token.COMMA);
            } 
            // separator if needed (linebreak or blank)
            if (needsLinebreak && p.linebreak(parLineBeg, 0, ws, true) > 0) { 
                // break line if the opening "(" or previous parameter ended on a different line
                ws = ignore;
            }
            else if (i > 0) {
                p.print(blank);
            } 
            // parameter names
            if (len(par.Names) > 0) { 
                // Very subtle: If we indented before (ws == ignore), identList
                // won't indent again. If we didn't (ws == indent), identList will
                // indent if the identList spans multiple lines, and it will outdent
                // again at the end (and still ws == indent). Thus, a subsequent indent
                // by a linebreak call after a type, or in the next multi-line identList
                // will do the right thing.
                p.identList(par.Names, ws == indent);
                p.print(blank);
            } 
            // parameter type
            p.expr(stripParensAlways(par.Type));
            prevLine = parLineEnd;
        }        {
            var closing = p.lineFor(fields.Closing);

            if (0 < prevLine && prevLine < closing) {
                p.print(token.COMMA);
                p.linebreak(closing, 0, ignore, true);
            } 
            // unindent if we indented

        } 
        // unindent if we indented
        if (ws == ignore) {
            p.print(unindent);
        }
    }
    p.print(fields.Closing, closeTok);
}

private static void signature(this ptr<printer> _addr_p, ptr<ast.FuncType> _addr_sig) {
    ref printer p = ref _addr_p.val;
    ref ast.FuncType sig = ref _addr_sig.val;

    {
        var tparams = typeparams.Get(sig);

        if (tparams != null) {
            p.parameters(tparams, true);
        }
    }
    if (sig.Params != null) {
        p.parameters(sig.Params, false);
    }
    else
 {
        p.print(token.LPAREN, token.RPAREN);
    }
    var res = sig.Results;
    var n = res.NumFields();
    if (n > 0) { 
        // res != nil
        p.print(blank);
        if (n == 1 && res.List[0].Names == null) { 
            // single anonymous res; no ()'s
            p.expr(stripParensAlways(res.List[0].Type));
            return ;
        }
        p.parameters(res, false);
    }
}

private static nint identListSize(slice<ptr<ast.Ident>> list, nint maxSize) {
    nint size = default;

    foreach (var (i, x) in list) {
        if (i > 0) {
            size += len(", ");
        }
        size += utf8.RuneCountInString(x.Name);
        if (size >= maxSize) {
            break;
        }
    }    return ;
}

private static bool isOneLineFieldList(this ptr<printer> _addr_p, slice<ptr<ast.Field>> list) {
    ref printer p = ref _addr_p.val;

    if (len(list) != 1) {
        return false; // allow only one field
    }
    var f = list[0];
    if (f.Tag != null || f.Comment != null) {
        return false; // don't allow tags or comments
    }
    const nint maxSize = 30; // adjust as appropriate, this is an approximate value
 // adjust as appropriate, this is an approximate value
    var namesSize = identListSize(f.Names, maxSize);
    if (namesSize > 0) {
        namesSize = 1; // blank between names and types
    }
    var typeSize = p.nodeSize(f.Type, maxSize);
    return namesSize + typeSize <= maxSize;
}

private static void setLineComment(this ptr<printer> _addr_p, @string text) {
    ref printer p = ref _addr_p.val;

    p.setComment(addr(new ast.CommentGroup(List:[]*ast.Comment{{Slash:token.NoPos,Text:text}})));
}

private static void fieldList(this ptr<printer> _addr_p, ptr<ast.FieldList> _addr_fields, bool isStruct, bool isIncomplete) {
    ref printer p = ref _addr_p.val;
    ref ast.FieldList fields = ref _addr_fields.val;

    var lbrace = fields.Opening;
    var list = fields.List;
    var rbrace = fields.Closing;
    var hasComments = isIncomplete || p.commentBefore(p.posFor(rbrace));
    var srcIsOneLine = lbrace.IsValid() && rbrace.IsValid() && p.lineFor(lbrace) == p.lineFor(rbrace);

    if (!hasComments && srcIsOneLine) { 
        // possibly a one-line struct/interface
        if (len(list) == 0) { 
            // no blank between keyword and {} in this case
            p.print(lbrace, token.LBRACE, rbrace, token.RBRACE);
            return ;
        }
        else if (p.isOneLineFieldList(list)) { 
            // small enough - print on one line
            // (don't use identList and ignore source line breaks)
            p.print(lbrace, token.LBRACE, blank);
            var f = list[0];
            if (isStruct) {
                {
                    var i__prev1 = i;

                    foreach (var (__i, __x) in f.Names) {
                        i = __i;
                        x = __x;
                        if (i > 0) { 
                            // no comments so no need for comma position
                            p.print(token.COMMA, blank);
                        }
                        p.expr(x);
                    }
            else

                    i = i__prev1;
                }

                if (len(f.Names) > 0) {
                    p.print(blank);
                }
                p.expr(f.Type);
            } { // interface
                if (len(f.Names) > 0) { 
                    // type list type or method
                    var name = f.Names[0]; // "type" or method name
                    p.expr(name);
                    if (name.Name == "type") { 
                        // type list type
                        p.print(blank);
                        p.expr(f.Type);
                    }
                    else
 { 
                        // method
                        p.signature(f.Type._<ptr<ast.FuncType>>()); // don't print "func"
                    }
                }
                else
 { 
                    // embedded interface
                    p.expr(f.Type);
                }
            }
            p.print(blank, rbrace, token.RBRACE);
            return ;
        }
    }
    p.print(blank, lbrace, token.LBRACE, indent);
    if (hasComments || len(list) > 0) {
        p.print(formfeed);
    }
    if (isStruct) {
        var sep = vtab;
        if (len(list) == 1) {
            sep = blank;
        }
        ref nint line = ref heap(out ptr<nint> _addr_line);
        {
            var i__prev1 = i;
            var f__prev1 = f;

            foreach (var (__i, __f) in list) {
                i = __i;
                f = __f;
                if (i > 0) {
                    p.linebreak(p.lineFor(f.Pos()), 1, ignore, p.linesFrom(line) > 0);
                }
                nint extraTabs = 0;
                p.setComment(f.Doc);
                p.recordLine(_addr_line);
                if (len(f.Names) > 0) { 
                    // named fields
                    p.identList(f.Names, false);
                    p.print(sep);
                    p.expr(f.Type);
                    extraTabs = 1;
                }
                else
 { 
                    // anonymous field
                    p.expr(f.Type);
                    extraTabs = 2;
                }
                if (f.Tag != null) {
                    if (len(f.Names) > 0 && sep == vtab) {
                        p.print(sep);
                    }
                    p.print(sep);
                    p.expr(f.Tag);
                    extraTabs = 0;
                }
                if (f.Comment != null) {
                    while (extraTabs > 0) {
                        p.print(sep);
                        extraTabs--;
                    }

                    p.setComment(f.Comment);
                }
    else
            }

            i = i__prev1;
            f = f__prev1;
        }

        if (isIncomplete) {
            if (len(list) > 0) {
                p.print(formfeed);
            }
            p.flush(p.posFor(rbrace), token.RBRACE); // make sure we don't lose the last line comment
            p.setLineComment("// " + filteredMsg);
        }
    } { // interface

        line = default;
        ptr<ast.Ident> prev; // previous "type" identifier
        {
            var i__prev1 = i;
            var f__prev1 = f;

            foreach (var (__i, __f) in list) {
                i = __i;
                f = __f;
                name = ; // first name, or nil
                if (len(f.Names) > 0) {
                    name = f.Names[0];
                }
                if (i > 0) { 
                    // don't do a line break (min == 0) if we are printing a list of types
                    // TODO(gri) this doesn't work quite right if the list of types is
                    //           spread across multiple lines
                    nint min = 1;
                    if (prev != null && name == prev) {
                        min = 0;
                    }
                    p.linebreak(p.lineFor(f.Pos()), min, ignore, p.linesFrom(line) > 0);
                }
                p.setComment(f.Doc);
                p.recordLine(_addr_line);
                if (name != null) { 
                    // type list type or method
                    if (name.Name == "type") { 
                        // type list type
                        if (name == prev) { 
                            // type is part of a list of types
                            p.print(token.COMMA, blank);
                        }
                        else
 { 
                            // type starts a new list of types
                            p.print(name, blank);
                        }
                        p.expr(f.Type);
                        prev = name;
                    }
                    else
 { 
                        // method
                        p.expr(name);
                        p.signature(f.Type._<ptr<ast.FuncType>>()); // don't print "func"
                        prev = null;
                    }
                }
                else
 { 
                    // embedded interface
                    p.expr(f.Type);
                    prev = null;
                }
                p.setComment(f.Comment);
            }

            i = i__prev1;
            f = f__prev1;
        }

        if (isIncomplete) {
            if (len(list) > 0) {
                p.print(formfeed);
            }
            p.flush(p.posFor(rbrace), token.RBRACE); // make sure we don't lose the last line comment
            p.setLineComment("// contains filtered or unexported methods");
        }
    }
    p.print(unindent, formfeed, rbrace, token.RBRACE);
}

// ----------------------------------------------------------------------------
// Expressions

private static (bool, bool, nint) walkBinary(ptr<ast.BinaryExpr> _addr_e) {
    bool has4 = default;
    bool has5 = default;
    nint maxProblem = default;
    ref ast.BinaryExpr e = ref _addr_e.val;

    switch (e.Op.Precedence()) {
        case 4: 
            has4 = true;
            break;
        case 5: 
            has5 = true;
            break;
    }

    switch (e.X.type()) {
        case ptr<ast.BinaryExpr> l:
            if (l.Op.Precedence() < e.Op.Precedence()) { 
                // parens will be inserted.
                // pretend this is an *ast.ParenExpr and do nothing.
                break;
            }
            var (h4, h5, mp) = walkBinary(_addr_l);
            has4 = has4 || h4;
            has5 = has5 || h5;
            if (maxProblem < mp) {
                maxProblem = mp;
            }
            break;

    }

    switch (e.Y.type()) {
        case ptr<ast.BinaryExpr> r:
            if (r.Op.Precedence() <= e.Op.Precedence()) { 
                // parens will be inserted.
                // pretend this is an *ast.ParenExpr and do nothing.
                break;
            }
            (h4, h5, mp) = walkBinary(_addr_r);
            has4 = has4 || h4;
            has5 = has5 || h5;
            if (maxProblem < mp) {
                maxProblem = mp;
            }
            break;
        case ptr<ast.StarExpr> r:
            if (e.Op == token.QUO) { // `*/`
                maxProblem = 5;
            }
            break;
        case ptr<ast.UnaryExpr> r:
            switch (e.Op.String() + r.Op.String()) {
                case "/*": 

                case "&&": 

                case "&^": 
                    maxProblem = 5;
                    break;
                case "++": 

                case "--": 
                    if (maxProblem < 4) {
                        maxProblem = 4;
                    }
                    break;
            }
            break;
    }
    return ;
}

private static nint cutoff(ptr<ast.BinaryExpr> _addr_e, nint depth) {
    ref ast.BinaryExpr e = ref _addr_e.val;

    var (has4, has5, maxProblem) = walkBinary(_addr_e);
    if (maxProblem > 0) {
        return maxProblem + 1;
    }
    if (has4 && has5) {
        if (depth == 1) {
            return 5;
        }
        return 4;
    }
    if (depth == 1) {
        return 6;
    }
    return 4;
}

private static nint diffPrec(ast.Expr expr, nint prec) {
    ptr<ast.BinaryExpr> (x, ok) = expr._<ptr<ast.BinaryExpr>>();
    if (!ok || prec != x.Op.Precedence()) {
        return 1;
    }
    return 0;
}

private static nint reduceDepth(nint depth) {
    depth--;
    if (depth < 1) {
        depth = 1;
    }
    return depth;
}

// Format the binary expression: decide the cutoff and then format.
// Let's call depth == 1 Normal mode, and depth > 1 Compact mode.
// (Algorithm suggestion by Russ Cox.)
//
// The precedences are:
//    5             *  /  %  <<  >>  &  &^
//    4             +  -  |  ^
//    3             ==  !=  <  <=  >  >=
//    2             &&
//    1             ||
//
// The only decision is whether there will be spaces around levels 4 and 5.
// There are never spaces at level 6 (unary), and always spaces at levels 3 and below.
//
// To choose the cutoff, look at the whole expression but excluding primary
// expressions (function calls, parenthesized exprs), and apply these rules:
//
//    1) If there is a binary operator with a right side unary operand
//       that would clash without a space, the cutoff must be (in order):
//
//        /*    6
//        &&    6
//        &^    6
//        ++    5
//        --    5
//
//         (Comparison operators always have spaces around them.)
//
//    2) If there is a mix of level 5 and level 4 operators, then the cutoff
//       is 5 (use spaces to distinguish precedence) in Normal mode
//       and 4 (never use spaces) in Compact mode.
//
//    3) If there are no level 4 operators or no level 5 operators, then the
//       cutoff is 6 (always use spaces) in Normal mode
//       and 4 (never use spaces) in Compact mode.
//
private static void binaryExpr(this ptr<printer> _addr_p, ptr<ast.BinaryExpr> _addr_x, nint prec1, nint cutoff, nint depth) {
    ref printer p = ref _addr_p.val;
    ref ast.BinaryExpr x = ref _addr_x.val;

    var prec = x.Op.Precedence();
    if (prec < prec1) { 
        // parenthesis needed
        // Note: The parser inserts an ast.ParenExpr node; thus this case
        //       can only occur if the AST is created in a different way.
        p.print(token.LPAREN);
        p.expr0(x, reduceDepth(depth)); // parentheses undo one level of depth
        p.print(token.RPAREN);
        return ;
    }
    var printBlank = prec < cutoff;

    var ws = indent;
    p.expr1(x.X, prec, depth + diffPrec(x.X, prec));
    if (printBlank) {
        p.print(blank);
    }
    var xline = p.pos.Line; // before the operator (it may be on the next line!)
    var yline = p.lineFor(x.Y.Pos());
    p.print(x.OpPos, x.Op);
    if (xline != yline && xline > 0 && yline > 0) { 
        // at least one line break, but respect an extra empty line
        // in the source
        if (p.linebreak(yline, 1, ws, true) > 0) {
            ws = ignore;
            printBlank = false; // no blank after line break
        }
    }
    if (printBlank) {
        p.print(blank);
    }
    p.expr1(x.Y, prec + 1, depth + 1);
    if (ws == ignore) {
        p.print(unindent);
    }
}

private static bool isBinary(ast.Expr expr) {
    ptr<ast.BinaryExpr> (_, ok) = expr._<ptr<ast.BinaryExpr>>();
    return ok;
}

private static void expr1(this ptr<printer> _addr_p, ast.Expr expr, nint prec1, nint depth) => func((_, panic, _) => {
    ref printer p = ref _addr_p.val;

    p.print(expr.Pos());

    switch (expr.type()) {
        case ptr<ast.BadExpr> x:
            p.print("BadExpr");
            break;
        case ptr<ast.Ident> x:
            p.print(x);
            break;
        case ptr<ast.BinaryExpr> x:
            if (depth < 1) {
                p.internalError("depth < 1:", depth);
                depth = 1;
            }
            p.binaryExpr(x, prec1, cutoff(_addr_x, depth), depth);
            break;
        case ptr<ast.KeyValueExpr> x:
            p.expr(x.Key);
            p.print(x.Colon, token.COLON, blank);
            p.expr(x.Value);
            break;
        case ptr<ast.StarExpr> x:
            const var prec = token.UnaryPrec;

            if (prec < prec1) { 
                // parenthesis needed
                p.print(token.LPAREN);
                p.print(token.MUL);
                p.expr(x.X);
                p.print(token.RPAREN);
            }
            else
 { 
                // no parenthesis needed
                p.print(token.MUL);
                p.expr(x.X);
            }
            break;
        case ptr<ast.UnaryExpr> x:
            const var prec = token.UnaryPrec;

            if (prec < prec1) { 
                // parenthesis needed
                p.print(token.LPAREN);
                p.expr(x);
                p.print(token.RPAREN);
            }
            else
 { 
                // no parenthesis needed
                p.print(x.Op);
                if (x.Op == token.RANGE) { 
                    // TODO(gri) Remove this code if it cannot be reached.
                    p.print(blank);
                }
                p.expr1(x.X, prec, depth);
            }
            break;
        case ptr<ast.BasicLit> x:
            if (p.Config.Mode & normalizeNumbers != 0) {
                x = normalizedNumber(_addr_x);
            }
            p.print(x);
            break;
        case ptr<ast.FuncLit> x:
            p.print(x.Type.Pos(), token.FUNC); 
            // See the comment in funcDecl about how the header size is computed.
            var startCol = p.@out.Column - len("func");
            p.signature(x.Type);
            p.funcBody(p.distanceFrom(x.Type.Pos(), startCol), blank, x.Body);
            break;
        case ptr<ast.ParenExpr> x:
            {
                ptr<ast.ParenExpr> (_, hasParens) = x.X._<ptr<ast.ParenExpr>>();

                if (hasParens) { 
                    // don't print parentheses around an already parenthesized expression
                    // TODO(gri) consider making this more general and incorporate precedence levels
                    p.expr0(x.X, depth);
                }
                else
 {
                    p.print(token.LPAREN);
                    p.expr0(x.X, reduceDepth(depth)); // parentheses undo one level of depth
                    p.print(x.Rparen, token.RPAREN);
                }

            }
            break;
        case ptr<ast.SelectorExpr> x:
            p.selectorExpr(x, depth, false);
            break;
        case ptr<ast.TypeAssertExpr> x:
            p.expr1(x.X, token.HighestPrec, depth);
            p.print(token.PERIOD, x.Lparen, token.LPAREN);
            if (x.Type != null) {
                p.expr(x.Type);
            }
            else
 {
                p.print(token.TYPE);
            }
            p.print(x.Rparen, token.RPAREN);
            break;
        case ptr<ast.IndexExpr> x:
            p.expr1(x.X, token.HighestPrec, 1);
            p.print(x.Lbrack, token.LBRACK); 
            // Note: we're a bit defensive here to handle the case of a ListExpr of
            // length 1.
            {
                var list = typeparams.UnpackExpr(x.Index);

                if (len(list) > 0) {
                    if (len(list) > 1) {
                        p.exprList(x.Lbrack, list, depth + 1, commaTerm, x.Rbrack, false);
                    }
                    else
 {
                        p.expr0(list[0], depth + 1);
                    }
                }
                else
 {
                    p.expr0(x.Index, depth + 1);
                }

            }
            p.print(x.Rbrack, token.RBRACK);
            break;
        case ptr<ast.SliceExpr> x:
            p.expr1(x.X, token.HighestPrec, 1);
            p.print(x.Lbrack, token.LBRACK);
            ast.Expr indices = new slice<ast.Expr>(new ast.Expr[] { x.Low, x.High });
            if (x.Max != null) {
                indices = append(indices, x.Max);
            } 
            // determine if we need extra blanks around ':'
            bool needsBlanks = default;
            if (depth <= 1) {
                nint indexCount = default;
                bool hasBinaries = default;
                {
                    var x__prev1 = x;

                    foreach (var (_, __x) in indices) {
                        x = __x;
                        if (x != null) {
                            indexCount++;
                            if (isBinary(x)) {
                                hasBinaries = true;
                            }
                        }
                    }

                    x = x__prev1;
                }

                if (indexCount > 1 && hasBinaries) {
                    needsBlanks = true;
                }
            }
            {
                var x__prev1 = x;

                foreach (var (__i, __x) in indices) {
                    i = __i;
                    x = __x;
                    if (i > 0) {
                        if (indices[i - 1] != null && needsBlanks) {
                            p.print(blank);
                        }
                        p.print(token.COLON);
                        if (x != null && needsBlanks) {
                            p.print(blank);
                        }
                    }
                    if (x != null) {
                        p.expr0(x, depth + 1);
                    }
                }

                x = x__prev1;
            }

            p.print(x.Rbrack, token.RBRACK);
            break;
        case ptr<ast.CallExpr> x:
            if (len(x.Args) > 1) {
                depth++;
            }
            bool wasIndented = default;
            {
                ptr<ast.FuncType> (_, ok) = x.Fun._<ptr<ast.FuncType>>();

                if (ok) { 
                    // conversions to literal function types require parentheses around the type
                    p.print(token.LPAREN);
                    wasIndented = p.possibleSelectorExpr(x.Fun, token.HighestPrec, depth);
                    p.print(token.RPAREN);
                }
                else
 {
                    wasIndented = p.possibleSelectorExpr(x.Fun, token.HighestPrec, depth);
                }

            }
            p.print(x.Lparen, token.LPAREN);
            if (x.Ellipsis.IsValid()) {
                p.exprList(x.Lparen, x.Args, depth, 0, x.Ellipsis, false);
                p.print(x.Ellipsis, token.ELLIPSIS);
                if (x.Rparen.IsValid() && p.lineFor(x.Ellipsis) < p.lineFor(x.Rparen)) {
                    p.print(token.COMMA, formfeed);
                }
            }
            else
 {
                p.exprList(x.Lparen, x.Args, depth, commaTerm, x.Rparen, false);
            }
            p.print(x.Rparen, token.RPAREN);
            if (wasIndented) {
                p.print(unindent);
            }
            break;
        case ptr<ast.CompositeLit> x:
            if (x.Type != null) {
                p.expr1(x.Type, token.HighestPrec, depth);
            }
            p.level++;
            p.print(x.Lbrace, token.LBRACE);
            p.exprList(x.Lbrace, x.Elts, 1, commaTerm, x.Rbrace, x.Incomplete); 
            // do not insert extra line break following a /*-style comment
            // before the closing '}' as it might break the code if there
            // is no trailing ','
            var mode = noExtraLinebreak; 
            // do not insert extra blank following a /*-style comment
            // before the closing '}' unless the literal is empty
            if (len(x.Elts) > 0) {
                mode |= noExtraBlank;
            } 
            // need the initial indent to print lone comments with
            // the proper level of indentation
            p.print(indent, unindent, mode, x.Rbrace, token.RBRACE, mode);
            p.level--;
            break;
        case ptr<ast.Ellipsis> x:
            p.print(token.ELLIPSIS);
            if (x.Elt != null) {
                p.expr(x.Elt);
            }
            break;
        case ptr<ast.ArrayType> x:
            p.print(token.LBRACK);
            if (x.Len != null) {
                p.expr(x.Len);
            }
            p.print(token.RBRACK);
            p.expr(x.Elt);
            break;
        case ptr<ast.StructType> x:
            p.print(token.STRUCT);
            p.fieldList(x.Fields, true, x.Incomplete);
            break;
        case ptr<ast.FuncType> x:
            p.print(token.FUNC);
            p.signature(x);
            break;
        case ptr<ast.InterfaceType> x:
            p.print(token.INTERFACE);
            p.fieldList(x.Methods, false, x.Incomplete);
            break;
        case ptr<ast.MapType> x:
            p.print(token.MAP, token.LBRACK);
            p.expr(x.Key);
            p.print(token.RBRACK);
            p.expr(x.Value);
            break;
        case ptr<ast.ChanType> x:

            if (x.Dir == ast.SEND | ast.RECV) 
                p.print(token.CHAN);
            else if (x.Dir == ast.RECV) 
                p.print(token.ARROW, token.CHAN); // x.Arrow and x.Pos() are the same
            else if (x.Dir == ast.SEND) 
                p.print(token.CHAN, x.Arrow, token.ARROW);
                        p.print(blank);
            p.expr(x.Value);
            break;
        default:
        {
            var x = expr.type();
            panic("unreachable");
            break;
        }
    }
});

// normalizedNumber rewrites base prefixes and exponents
// of numbers to use lower-case letters (0X123 to 0x123 and 1.2E3 to 1.2e3),
// and removes leading 0's from integer imaginary literals (0765i to 765i).
// It leaves hexadecimal digits alone.
//
// normalizedNumber doesn't modify the ast.BasicLit value lit points to.
// If lit is not a number or a number in canonical format already,
// lit is returned as is. Otherwise a new ast.BasicLit is created.
private static ptr<ast.BasicLit> normalizedNumber(ptr<ast.BasicLit> _addr_lit) {
    ref ast.BasicLit lit = ref _addr_lit.val;

    if (lit.Kind != token.INT && lit.Kind != token.FLOAT && lit.Kind != token.IMAG) {
        return _addr_lit!; // not a number - nothing to do
    }
    if (len(lit.Value) < 2) {
        return _addr_lit!; // only one digit (common case) - nothing to do
    }
    var x = lit.Value;
    switch (x[..(int)2]) {
        case "0X": 
            x = "0x" + x[(int)2..]; 
            // possibly a hexadecimal float
            {
                var i__prev1 = i;

                i = strings.LastIndexByte(x, 'P');

                if (i >= 0) {
                    x = x[..(int)i] + "p" + x[(int)i + 1..];
                }

                i = i__prev1;

            }
            break;
        case "0x": 
            // possibly a hexadecimal float
            i = strings.LastIndexByte(x, 'P');
            if (i == -1) {
                return _addr_lit!; // nothing to do
            }
            x = x[..(int)i] + "p" + x[(int)i + 1..];
            break;
        case "0O": 
            x = "0o" + x[(int)2..];
            break;
        case "0o": 
            return _addr_lit!; // nothing to do
            break;
        case "0B": 
            x = "0b" + x[(int)2..];
            break;
        case "0b": 
            return _addr_lit!; // nothing to do
            break;
        default: 
            // 0-prefix octal, decimal int, or float (possibly with 'i' suffix)
            {
                var i__prev1 = i;

                var i = strings.LastIndexByte(x, 'E');

                if (i >= 0) {
                    x = x[..(int)i] + "e" + x[(int)i + 1..];
                    break;
                } 
                // remove leading 0's from integer (but not floating-point) imaginary literals

                i = i__prev1;

            } 
            // remove leading 0's from integer (but not floating-point) imaginary literals
            if (x[len(x) - 1] == 'i' && strings.IndexByte(x, '.') < 0 && strings.IndexByte(x, 'e') < 0) {
                x = strings.TrimLeft(x, "0_");
                if (x == "i") {
                    x = "0i";
                }
            }
            break;
    }

    return addr(new ast.BasicLit(ValuePos:lit.ValuePos,Kind:lit.Kind,Value:x));
}

private static bool possibleSelectorExpr(this ptr<printer> _addr_p, ast.Expr expr, nint prec1, nint depth) {
    ref printer p = ref _addr_p.val;

    {
        ptr<ast.SelectorExpr> (x, ok) = expr._<ptr<ast.SelectorExpr>>();

        if (ok) {
            return p.selectorExpr(x, depth, true);
        }
    }
    p.expr1(expr, prec1, depth);
    return false;
}

// selectorExpr handles an *ast.SelectorExpr node and reports whether x spans
// multiple lines.
private static bool selectorExpr(this ptr<printer> _addr_p, ptr<ast.SelectorExpr> _addr_x, nint depth, bool isMethod) {
    ref printer p = ref _addr_p.val;
    ref ast.SelectorExpr x = ref _addr_x.val;

    p.expr1(x.X, token.HighestPrec, depth);
    p.print(token.PERIOD);
    {
        var line = p.lineFor(x.Sel.Pos());

        if (p.pos.IsValid() && p.pos.Line < line) {
            p.print(indent, newline, x.Sel.Pos(), x.Sel);
            if (!isMethod) {
                p.print(unindent);
            }
            return true;
        }
    }
    p.print(x.Sel.Pos(), x.Sel);
    return false;
}

private static void expr0(this ptr<printer> _addr_p, ast.Expr x, nint depth) {
    ref printer p = ref _addr_p.val;

    p.expr1(x, token.LowestPrec, depth);
}

private static void expr(this ptr<printer> _addr_p, ast.Expr x) {
    ref printer p = ref _addr_p.val;

    const nint depth = 1;

    p.expr1(x, token.LowestPrec, depth);
}

// ----------------------------------------------------------------------------
// Statements

// Print the statement list indented, but without a newline after the last statement.
// Extra line breaks between statements in the source are respected but at most one
// empty line is printed between statements.
private static void stmtList(this ptr<printer> _addr_p, slice<ast.Stmt> list, nint nindent, bool nextIsRBrace) {
    ref printer p = ref _addr_p.val;

    if (nindent > 0) {
        p.print(indent);
    }
    ref nint line = ref heap(out ptr<nint> _addr_line);
    nint i = 0;
    foreach (var (_, s) in list) { 
        // ignore empty statements (was issue 3466)
        {
            ptr<ast.EmptyStmt> (_, isEmpty) = s._<ptr<ast.EmptyStmt>>();

            if (!isEmpty) { 
                // nindent == 0 only for lists of switch/select case clauses;
                // in those cases each clause is a new section
                if (len(p.output) > 0) { 
                    // only print line break if we are not at the beginning of the output
                    // (i.e., we are not printing only a partial program)
                    p.linebreak(p.lineFor(s.Pos()), 1, ignore, i == 0 || nindent == 0 || p.linesFrom(line) > 0);
                }
                p.recordLine(_addr_line);
                p.stmt(s, nextIsRBrace && i == len(list) - 1); 
                // labeled statements put labels on a separate line, but here
                // we only care about the start line of the actual statement
                // without label - correct line for each label
                {
                    var t = s;

                    while () {
                        ptr<ast.LabeledStmt> (lt, _) = t._<ptr<ast.LabeledStmt>>();
                        if (lt == null) {
                            break;
                        }
                        line++;
                        t = lt.Stmt;
                    }

                }
                i++;
            }

        }
    }    if (nindent > 0) {
        p.print(unindent);
    }
}

// block prints an *ast.BlockStmt; it always spans at least two lines.
private static void block(this ptr<printer> _addr_p, ptr<ast.BlockStmt> _addr_b, nint nindent) {
    ref printer p = ref _addr_p.val;
    ref ast.BlockStmt b = ref _addr_b.val;

    p.print(b.Lbrace, token.LBRACE);
    p.stmtList(b.List, nindent, true);
    p.linebreak(p.lineFor(b.Rbrace), 1, ignore, true);
    p.print(b.Rbrace, token.RBRACE);
}

private static bool isTypeName(ast.Expr x) {
    switch (x.type()) {
        case ptr<ast.Ident> t:
            return true;
            break;
        case ptr<ast.SelectorExpr> t:
            return isTypeName(t.X);
            break;
    }
    return false;
}

private static ast.Expr stripParens(ast.Expr x) {
    {
        ptr<ast.ParenExpr> (px, strip) = x._<ptr<ast.ParenExpr>>();

        if (strip) { 
            // parentheses must not be stripped if there are any
            // unparenthesized composite literals starting with
            // a type name
            ast.Inspect(px.X, node => {
                switch (node.type()) {
                    case ptr<ast.ParenExpr> x:
                        return false;
                        break;
                    case ptr<ast.CompositeLit> x:
                        if (isTypeName(x.Type)) {
                            strip = false; // do not strip parentheses
                        }
                        return false;
                        break; 
                    // in all other cases, keep inspecting
                } 
                // in all other cases, keep inspecting
                return true;
            });
            if (strip) {
                return stripParens(px.X);
            }
        }
    }
    return x;
}

private static ast.Expr stripParensAlways(ast.Expr x) {
    {
        ptr<ast.ParenExpr> (x, ok) = x._<ptr<ast.ParenExpr>>();

        if (ok) {
            return stripParensAlways(x.X);
        }
    }
    return x;
}

private static void controlClause(this ptr<printer> _addr_p, bool isForStmt, ast.Stmt init, ast.Expr expr, ast.Stmt post) {
    ref printer p = ref _addr_p.val;

    p.print(blank);
    var needsBlank = false;
    if (init == null && post == null) { 
        // no semicolons required
        if (expr != null) {
            p.expr(stripParens(expr));
            needsBlank = true;
        }
    }
    else
 { 
        // all semicolons required
        // (they are not separators, print them explicitly)
        if (init != null) {
            p.stmt(init, false);
        }
        p.print(token.SEMICOLON, blank);
        if (expr != null) {
            p.expr(stripParens(expr));
            needsBlank = true;
        }
        if (isForStmt) {
            p.print(token.SEMICOLON, blank);
            needsBlank = false;
            if (post != null) {
                p.stmt(post, false);
                needsBlank = true;
            }
        }
    }
    if (needsBlank) {
        p.print(blank);
    }
}

// indentList reports whether an expression list would look better if it
// were indented wholesale (starting with the very first element, rather
// than starting at the first line break).
//
private static bool indentList(this ptr<printer> _addr_p, slice<ast.Expr> list) {
    ref printer p = ref _addr_p.val;
 
    // Heuristic: indentList reports whether there are more than one multi-
    // line element in the list, or if there is any element that is not
    // starting on the same line as the previous one ends.
    if (len(list) >= 2) {
        var b = p.lineFor(list[0].Pos());
        var e = p.lineFor(list[len(list) - 1].End());
        if (0 < b && b < e) { 
            // list spans multiple lines
            nint n = 0; // multi-line element count
            var line = b;
            foreach (var (_, x) in list) {
                var xb = p.lineFor(x.Pos());
                var xe = p.lineFor(x.End());
                if (line < xb) { 
                    // x is not starting on the same
                    // line as the previous one ended
                    return true;
                }
                if (xb < xe) { 
                    // x is a multi-line element
                    n++;
                }
                line = xe;
            }
            return n > 1;
        }
    }
    return false;
}

private static void stmt(this ptr<printer> _addr_p, ast.Stmt stmt, bool nextIsRBrace) => func((_, panic, _) => {
    ref printer p = ref _addr_p.val;

    p.print(stmt.Pos());

    switch (stmt.type()) {
        case ptr<ast.BadStmt> s:
            p.print("BadStmt");
            break;
        case ptr<ast.DeclStmt> s:
            p.decl(s.Decl);
            break;
        case ptr<ast.EmptyStmt> s:
            break;
        case ptr<ast.LabeledStmt> s:
            p.print(unindent);
            p.expr(s.Label);
            p.print(s.Colon, token.COLON, indent);
            {
                ptr<ast.EmptyStmt> (e, isEmpty) = s.Stmt._<ptr<ast.EmptyStmt>>();

                if (isEmpty) {
                    if (!nextIsRBrace) {
                        p.print(newline, e.Pos(), token.SEMICOLON);
                        break;
                    }
                }
                else
 {
                    p.linebreak(p.lineFor(s.Stmt.Pos()), 1, ignore, true);
                }

            }
            p.stmt(s.Stmt, nextIsRBrace);
            break;
        case ptr<ast.ExprStmt> s:
            const nint depth = 1;

            p.expr0(s.X, depth);
            break;
        case ptr<ast.SendStmt> s:
            const nint depth = 1;

            p.expr0(s.Chan, depth);
            p.print(blank, s.Arrow, token.ARROW, blank);
            p.expr0(s.Value, depth);
            break;
        case ptr<ast.IncDecStmt> s:
            const nint depth = 1;

            p.expr0(s.X, depth + 1);
            p.print(s.TokPos, s.Tok);
            break;
        case ptr<ast.AssignStmt> s:
            nint depth = 1;
            if (len(s.Lhs) > 1 && len(s.Rhs) > 1) {
                depth++;
            }
            p.exprList(s.Pos(), s.Lhs, depth, 0, s.TokPos, false);
            p.print(blank, s.TokPos, s.Tok, blank);
            p.exprList(s.TokPos, s.Rhs, depth, 0, token.NoPos, false);
            break;
        case ptr<ast.GoStmt> s:
            p.print(token.GO, blank);
            p.expr(s.Call);
            break;
        case ptr<ast.DeferStmt> s:
            p.print(token.DEFER, blank);
            p.expr(s.Call);
            break;
        case ptr<ast.ReturnStmt> s:
            p.print(token.RETURN);
            if (s.Results != null) {
                p.print(blank); 
                // Use indentList heuristic to make corner cases look
                // better (issue 1207). A more systematic approach would
                // always indent, but this would cause significant
                // reformatting of the code base and not necessarily
                // lead to more nicely formatted code in general.
                if (p.indentList(s.Results)) {
                    p.print(indent); 
                    // Use NoPos so that a newline never goes before
                    // the results (see issue #32854).
                    p.exprList(token.NoPos, s.Results, 1, noIndent, token.NoPos, false);
                    p.print(unindent);
                }
                else
 {
                    p.exprList(token.NoPos, s.Results, 1, 0, token.NoPos, false);
                }
            }
            break;
        case ptr<ast.BranchStmt> s:
            p.print(s.Tok);
            if (s.Label != null) {
                p.print(blank);
                p.expr(s.Label);
            }
            break;
        case ptr<ast.BlockStmt> s:
            p.block(s, 1);
            break;
        case ptr<ast.IfStmt> s:
            p.print(token.IF);
            p.controlClause(false, s.Init, s.Cond, null);
            p.block(s.Body, 1);
            if (s.Else != null) {
                p.print(blank, token.ELSE, blank);
                switch (s.Else.type()) {
                    case ptr<ast.BlockStmt> _:
                        p.stmt(s.Else, nextIsRBrace);
                        break;
                    case ptr<ast.IfStmt> _:
                        p.stmt(s.Else, nextIsRBrace);
                        break;
                    default:
                    {
                        p.print(token.LBRACE, indent, formfeed);
                        p.stmt(s.Else, true);
                        p.print(unindent, formfeed, token.RBRACE);
                        break;
                    }
                }
            }
            break;
        case ptr<ast.CaseClause> s:
            if (s.List != null) {
                p.print(token.CASE, blank);
                p.exprList(s.Pos(), s.List, 1, 0, s.Colon, false);
            }
            else
 {
                p.print(token.DEFAULT);
            }
            p.print(s.Colon, token.COLON);
            p.stmtList(s.Body, 1, nextIsRBrace);
            break;
        case ptr<ast.SwitchStmt> s:
            p.print(token.SWITCH);
            p.controlClause(false, s.Init, s.Tag, null);
            p.block(s.Body, 0);
            break;
        case ptr<ast.TypeSwitchStmt> s:
            p.print(token.SWITCH);
            if (s.Init != null) {
                p.print(blank);
                p.stmt(s.Init, false);
                p.print(token.SEMICOLON);
            }
            p.print(blank);
            p.stmt(s.Assign, false);
            p.print(blank);
            p.block(s.Body, 0);
            break;
        case ptr<ast.CommClause> s:
            if (s.Comm != null) {
                p.print(token.CASE, blank);
                p.stmt(s.Comm, false);
            }
            else
 {
                p.print(token.DEFAULT);
            }
            p.print(s.Colon, token.COLON);
            p.stmtList(s.Body, 1, nextIsRBrace);
            break;
        case ptr<ast.SelectStmt> s:
            p.print(token.SELECT, blank);
            var body = s.Body;
            if (len(body.List) == 0 && !p.commentBefore(p.posFor(body.Rbrace))) { 
                // print empty select statement w/o comments on one line
                p.print(body.Lbrace, token.LBRACE, body.Rbrace, token.RBRACE);
            }
            else
 {
                p.block(body, 0);
            }
            break;
        case ptr<ast.ForStmt> s:
            p.print(token.FOR);
            p.controlClause(true, s.Init, s.Cond, s.Post);
            p.block(s.Body, 1);
            break;
        case ptr<ast.RangeStmt> s:
            p.print(token.FOR, blank);
            if (s.Key != null) {
                p.expr(s.Key);
                if (s.Value != null) { 
                    // use position of value following the comma as
                    // comma position for correct comment placement
                    p.print(s.Value.Pos(), token.COMMA, blank);
                    p.expr(s.Value);
                }
                p.print(blank, s.TokPos, s.Tok, blank);
            }
            p.print(token.RANGE, blank);
            p.expr(stripParens(s.X));
            p.print(blank);
            p.block(s.Body, 1);
            break;
        default:
        {
            var s = stmt.type();
            panic("unreachable");
            break;
        }
    }
});

// ----------------------------------------------------------------------------
// Declarations

// The keepTypeColumn function determines if the type column of a series of
// consecutive const or var declarations must be kept, or if initialization
// values (V) can be placed in the type column (T) instead. The i'th entry
// in the result slice is true if the type column in spec[i] must be kept.
//
// For example, the declaration:
//
//    const (
//        foobar int = 42 // comment
//        x          = 7  // comment
//        foo
//              bar = 991
//    )
//
// leads to the type/values matrix below. A run of value columns (V) can
// be moved into the type column if there is no type for any of the values
// in that column (we only move entire columns so that they align properly).
//
//    matrix        formatted     result
//                    matrix
//    T  V    ->    T  V     ->   true      there is a T and so the type
//    -  V          -  V          true      column must be kept
//    -  -          -  -          false
//    -  V          V  -          false     V is moved into T column
//
private static slice<bool> keepTypeColumn(slice<ast.Spec> specs) {
    var m = make_slice<bool>(len(specs));

    Action<nint, nint, bool> populate = (i, j, keepType) => {
        if (keepType) {
            while (i < j) {
                m[i] = true;
                i++;
            }
        }
    };

    nint i0 = -1; // if i0 >= 0 we are in a run and i0 is the start of the run
    bool keepType = default;
    foreach (var (i, s) in specs) {
        ptr<ast.ValueSpec> t = s._<ptr<ast.ValueSpec>>();
        if (t.Values != null) {
            if (i0 < 0) { 
                // start of a run of ValueSpecs with non-nil Values
                i0 = i;
                keepType = false;
            }
        }
        else
 {
            if (i0 >= 0) { 
                // end of a run
                populate(i0, i, keepType);
                i0 = -1;
            }
        }
        if (t.Type != null) {
            keepType = true;
        }
    }    if (i0 >= 0) { 
        // end of a run
        populate(i0, len(specs), keepType);
    }
    return m;
}

private static void valueSpec(this ptr<printer> _addr_p, ptr<ast.ValueSpec> _addr_s, bool keepType) {
    ref printer p = ref _addr_p.val;
    ref ast.ValueSpec s = ref _addr_s.val;

    p.setComment(s.Doc);
    p.identList(s.Names, false); // always present
    nint extraTabs = 3;
    if (s.Type != null || keepType) {
        p.print(vtab);
        extraTabs--;
    }
    if (s.Type != null) {
        p.expr(s.Type);
    }
    if (s.Values != null) {
        p.print(vtab, token.ASSIGN, blank);
        p.exprList(token.NoPos, s.Values, 1, 0, token.NoPos, false);
        extraTabs--;
    }
    if (s.Comment != null) {
        while (extraTabs > 0) {
            p.print(vtab);
            extraTabs--;
        }
        p.setComment(s.Comment);
    }
}

private static ptr<ast.BasicLit> sanitizeImportPath(ptr<ast.BasicLit> _addr_lit) {
    ref ast.BasicLit lit = ref _addr_lit.val;
 
    // Note: An unmodified AST generated by go/parser will already
    // contain a backward- or double-quoted path string that does
    // not contain any invalid characters, and most of the work
    // here is not needed. However, a modified or generated AST
    // may possibly contain non-canonical paths. Do the work in
    // all cases since it's not too hard and not speed-critical.

    // if we don't have a proper string, be conservative and return whatever we have
    if (lit.Kind != token.STRING) {
        return _addr_lit!;
    }
    var (s, err) = strconv.Unquote(lit.Value);
    if (err != null) {
        return _addr_lit!;
    }
    if (s == "") {
        return _addr_lit!;
    }
    const @string illegalChars = "!\"#$%&\'()*,:;<=>?[\\]^{|}" + "`\uFFFD";

    foreach (var (_, r) in s) {
        if (!unicode.IsGraphic(r) || unicode.IsSpace(r) || strings.ContainsRune(illegalChars, r)) {
            return _addr_lit!;
        }
    }    s = strconv.Quote(s);
    if (s == lit.Value) {
        return _addr_lit!; // nothing wrong with lit
    }
    return addr(new ast.BasicLit(ValuePos:lit.ValuePos,Kind:token.STRING,Value:s));
}

// The parameter n is the number of specs in the group. If doIndent is set,
// multi-line identifier lists in the spec are indented when the first
// linebreak is encountered.
//
private static void spec(this ptr<printer> _addr_p, ast.Spec spec, nint n, bool doIndent) => func((_, panic, _) => {
    ref printer p = ref _addr_p.val;

    switch (spec.type()) {
        case ptr<ast.ImportSpec> s:
            p.setComment(s.Doc);
            if (s.Name != null) {
                p.expr(s.Name);
                p.print(blank);
            }
            p.expr(sanitizeImportPath(_addr_s.Path));
            p.setComment(s.Comment);
            p.print(s.EndPos);
            break;
        case ptr<ast.ValueSpec> s:
            if (n != 1) {
                p.internalError("expected n = 1; got", n);
            }
            p.setComment(s.Doc);
            p.identList(s.Names, doIndent); // always present
            if (s.Type != null) {
                p.print(blank);
                p.expr(s.Type);
            }
            if (s.Values != null) {
                p.print(blank, token.ASSIGN, blank);
                p.exprList(token.NoPos, s.Values, 1, 0, token.NoPos, false);
            }
            p.setComment(s.Comment);
            break;
        case ptr<ast.TypeSpec> s:
            p.setComment(s.Doc);
            p.expr(s.Name);
            {
                var tparams = typeparams.Get(s);

                if (tparams != null) {
                    p.parameters(tparams, true);
                }

            }
            if (n == 1) {
                p.print(blank);
            }
            else
 {
                p.print(vtab);
            }
            if (s.Assign.IsValid()) {
                p.print(token.ASSIGN, blank);
            }
            p.expr(s.Type);
            p.setComment(s.Comment);
            break;
        default:
        {
            var s = spec.type();
            panic("unreachable");
            break;
        }
    }
});

private static void genDecl(this ptr<printer> _addr_p, ptr<ast.GenDecl> _addr_d) {
    ref printer p = ref _addr_p.val;
    ref ast.GenDecl d = ref _addr_d.val;

    p.setComment(d.Doc);
    p.print(d.Pos(), d.Tok, blank);

    if (d.Lparen.IsValid() || len(d.Specs) > 1) { 
        // group of parenthesized declarations
        p.print(d.Lparen, token.LPAREN);
        {
            var n = len(d.Specs);

            if (n > 0) {
                p.print(indent, formfeed);
                if (n > 1 && (d.Tok == token.CONST || d.Tok == token.VAR)) { 
                    // two or more grouped const/var declarations:
                    // determine if the type column must be kept
                    var keepType = keepTypeColumn(d.Specs);
                    ref nint line = ref heap(out ptr<nint> _addr_line);
                    {
                        var i__prev1 = i;
                        var s__prev1 = s;

                        foreach (var (__i, __s) in d.Specs) {
                            i = __i;
                            s = __s;
                            if (i > 0) {
                                p.linebreak(p.lineFor(s.Pos()), 1, ignore, p.linesFrom(line) > 0);
                            }
                            p.recordLine(_addr_line);
                            p.valueSpec(s._<ptr<ast.ValueSpec>>(), keepType[i]);
                        }
                else

                        i = i__prev1;
                        s = s__prev1;
                    }
                } {
                    line = default;
                    {
                        var i__prev1 = i;
                        var s__prev1 = s;

                        foreach (var (__i, __s) in d.Specs) {
                            i = __i;
                            s = __s;
                            if (i > 0) {
                                p.linebreak(p.lineFor(s.Pos()), 1, ignore, p.linesFrom(line) > 0);
                            }
                            p.recordLine(_addr_line);
                            p.spec(s, n, false);
                        }

                        i = i__prev1;
                        s = s__prev1;
                    }
                }
                p.print(unindent, formfeed);
            }

        }
        p.print(d.Rparen, token.RPAREN);
    }
    else if (len(d.Specs) > 0) { 
        // single declaration
        p.spec(d.Specs[0], 1, true);
    }
}

// nodeSize determines the size of n in chars after formatting.
// The result is <= maxSize if the node fits on one line with at
// most maxSize chars and the formatted output doesn't contain
// any control chars. Otherwise, the result is > maxSize.
//
private static nint nodeSize(this ptr<printer> _addr_p, ast.Node n, nint maxSize) {
    nint size = default;
    ref printer p = ref _addr_p.val;
 
    // nodeSize invokes the printer, which may invoke nodeSize
    // recursively. For deep composite literal nests, this can
    // lead to an exponential algorithm. Remember previous
    // results to prune the recursion (was issue 1628).
    {
        var (size, found) = p.nodeSizes[n];

        if (found) {
            return size;
        }
    }

    size = maxSize + 1; // assume n doesn't fit
    p.nodeSizes[n] = size; 

    // nodeSize computation must be independent of particular
    // style so that we always get the same decision; print
    // in RawFormat
    Config cfg = new Config(Mode:RawFormat);
    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    {
        var err = cfg.fprint(_addr_buf, p.fset, n, p.nodeSizes);

        if (err != null) {
            return ;
        }
    }
    if (buf.Len() <= maxSize) {
        foreach (var (_, ch) in buf.Bytes()) {
            if (ch < ' ') {
                return ;
            }
        }        size = buf.Len(); // n fits
        p.nodeSizes[n] = size;
    }
    return ;
}

// numLines returns the number of lines spanned by node n in the original source.
private static nint numLines(this ptr<printer> _addr_p, ast.Node n) {
    ref printer p = ref _addr_p.val;

    {
        var from = n.Pos();

        if (from.IsValid()) {
            {
                var to = n.End();

                if (to.IsValid()) {
                    return p.lineFor(to) - p.lineFor(from) + 1;
                }

            }
        }
    }
    return infinity;
}

// bodySize is like nodeSize but it is specialized for *ast.BlockStmt's.
private static nint bodySize(this ptr<printer> _addr_p, ptr<ast.BlockStmt> _addr_b, nint maxSize) {
    ref printer p = ref _addr_p.val;
    ref ast.BlockStmt b = ref _addr_b.val;

    var pos1 = b.Pos();
    var pos2 = b.Rbrace;
    if (pos1.IsValid() && pos2.IsValid() && p.lineFor(pos1) != p.lineFor(pos2)) { 
        // opening and closing brace are on different lines - don't make it a one-liner
        return maxSize + 1;
    }
    if (len(b.List) > 5) { 
        // too many statements - don't make it a one-liner
        return maxSize + 1;
    }
    var bodySize = p.commentSizeBefore(p.posFor(pos2));
    foreach (var (i, s) in b.List) {
        if (bodySize > maxSize) {
            break; // no need to continue
        }
        if (i > 0) {
            bodySize += 2; // space for a semicolon and blank
        }
        bodySize += p.nodeSize(s, maxSize);
    }    return bodySize;
}

// funcBody prints a function body following a function header of given headerSize.
// If the header's and block's size are "small enough" and the block is "simple enough",
// the block is printed on the current line, without line breaks, spaced from the header
// by sep. Otherwise the block's opening "{" is printed on the current line, followed by
// lines for the block's statements and its closing "}".
//
private static void funcBody(this ptr<printer> _addr_p, nint headerSize, whiteSpace sep, ptr<ast.BlockStmt> _addr_b) => func((defer, _, _) => {
    ref printer p = ref _addr_p.val;
    ref ast.BlockStmt b = ref _addr_b.val;

    if (b == null) {
        return ;
    }
    defer(level => {
        p.level = level;
    }(p.level));
    p.level = 0;

    const nint maxSize = 100;

    if (headerSize + p.bodySize(b, maxSize) <= maxSize) {
        p.print(sep, b.Lbrace, token.LBRACE);
        if (len(b.List) > 0) {
            p.print(blank);
            foreach (var (i, s) in b.List) {
                if (i > 0) {
                    p.print(token.SEMICOLON, blank);
                }
                p.stmt(s, i == len(b.List) - 1);
            }
            p.print(blank);
        }
        p.print(noExtraLinebreak, b.Rbrace, token.RBRACE, noExtraLinebreak);
        return ;
    }
    if (sep != ignore) {
        p.print(blank); // always use blank
    }
    p.block(b, 1);
});

// distanceFrom returns the column difference between p.out (the current output
// position) and startOutCol. If the start position is on a different line from
// the current position (or either is unknown), the result is infinity.
private static nint distanceFrom(this ptr<printer> _addr_p, token.Pos startPos, nint startOutCol) {
    ref printer p = ref _addr_p.val;

    if (startPos.IsValid() && p.pos.IsValid() && p.posFor(startPos).Line == p.pos.Line) {
        return p.@out.Column - startOutCol;
    }
    return infinity;
}

private static void funcDecl(this ptr<printer> _addr_p, ptr<ast.FuncDecl> _addr_d) {
    ref printer p = ref _addr_p.val;
    ref ast.FuncDecl d = ref _addr_d.val;

    p.setComment(d.Doc);
    p.print(d.Pos(), token.FUNC, blank); 
    // We have to save startCol only after emitting FUNC; otherwise it can be on a
    // different line (all whitespace preceding the FUNC is emitted only when the
    // FUNC is emitted).
    var startCol = p.@out.Column - len("func ");
    if (d.Recv != null) {
        p.parameters(d.Recv, false); // method: print receiver
        p.print(blank);
    }
    p.expr(d.Name);
    p.signature(d.Type);
    p.funcBody(p.distanceFrom(d.Pos(), startCol), vtab, d.Body);
}

private static void decl(this ptr<printer> _addr_p, ast.Decl decl) => func((_, panic, _) => {
    ref printer p = ref _addr_p.val;

    switch (decl.type()) {
        case ptr<ast.BadDecl> d:
            p.print(d.Pos(), "BadDecl");
            break;
        case ptr<ast.GenDecl> d:
            p.genDecl(d);
            break;
        case ptr<ast.FuncDecl> d:
            p.funcDecl(d);
            break;
        default:
        {
            var d = decl.type();
            panic("unreachable");
            break;
        }
    }
});

// ----------------------------------------------------------------------------
// Files

private static token.Token declToken(ast.Decl decl) {
    token.Token tok = default;

    tok = token.ILLEGAL;
    switch (decl.type()) {
        case ptr<ast.GenDecl> d:
            tok = d.Tok;
            break;
        case ptr<ast.FuncDecl> d:
            tok = token.FUNC;
            break;
    }
    return ;
}

private static void declList(this ptr<printer> _addr_p, slice<ast.Decl> list) {
    ref printer p = ref _addr_p.val;

    var tok = token.ILLEGAL;
    foreach (var (_, d) in list) {
        var prev = tok;
        tok = declToken(d); 
        // If the declaration token changed (e.g., from CONST to TYPE)
        // or the next declaration has documentation associated with it,
        // print an empty line between top-level declarations.
        // (because p.linebreak is called with the position of d, which
        // is past any documentation, the minimum requirement is satisfied
        // even w/o the extra getDoc(d) nil-check - leave it in case the
        // linebreak logic improves - there's already a TODO).
        if (len(p.output) > 0) { 
            // only print line break if we are not at the beginning of the output
            // (i.e., we are not printing only a partial program)
            nint min = 1;
            if (prev != tok || getDoc(d) != null) {
                min = 2;
            } 
            // start a new section if the next declaration is a function
            // that spans multiple lines (see also issue #19544)
            p.linebreak(p.lineFor(d.Pos()), min, ignore, tok == token.FUNC && p.numLines(d) > 1);
        }
        p.decl(d);
    }
}

private static void file(this ptr<printer> _addr_p, ptr<ast.File> _addr_src) {
    ref printer p = ref _addr_p.val;
    ref ast.File src = ref _addr_src.val;

    p.setComment(src.Doc);
    p.print(src.Pos(), token.PACKAGE, blank);
    p.expr(src.Name);
    p.declList(src.Decls);
    p.print(newline);
}

} // end printer_package
