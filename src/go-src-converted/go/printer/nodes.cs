// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements printing of AST nodes; specifically
// expressions, statements, declarations, and files. It uses
// the print functionality implemented in printer.go.

// package printer -- go2cs converted at 2020 August 29 08:52:25 UTC
// import "go/printer" ==> using printer = go.go.printer_package
// Original source: C:\Go\src\go\printer\nodes.go
using bytes = go.bytes_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class printer_package
    {
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
        // is set, the first line break is printed as formfeed. Returns true if any
        // line break was printed; returns false otherwise.
        //
        // TODO(gri): linebreak may add too many lines if the next statement at "line"
        //            is preceded by comments because the computation of n assumes
        //            the current position before the comment and the target position
        //            after the comment. Thus, after interspersing such comments, the
        //            space taken up by them is not considered to reduce the number of
        //            linebreaks. At the moment there is no easy way to know about
        //            future (not yet interspersed) comments in this function.
        //
        private static bool linebreak(this ref printer p, long line, long min, whiteSpace ws, bool newSection)
        {
            var n = nlimit(line - p.pos.Line);
            if (n < min)
            {
                n = min;
            }
            if (n > 0L)
            {
                p.print(ws);
                if (newSection)
                {
                    p.print(formfeed);
                    n--;
                }
                while (n > 0L)
                {
                    p.print(newline);
                    n--;
                }
                printedBreak = true;
            }
            return;
        }

        // setComment sets g as the next comment if g != nil and if node comments
        // are enabled - this mode is used when printing source code fragments such
        // as exports only. It assumes that there is no pending comment in p.comments
        // and at most one pending comment in the p.comment cache.
        private static void setComment(this ref printer p, ref ast.CommentGroup g)
        {
            if (g == null || !p.useNodeComments)
            {
                return;
            }
            if (p.comments == null)
            { 
                // initialize p.comments lazily
                p.comments = make_slice<ref ast.CommentGroup>(1L);
            }
            else if (p.cindex < len(p.comments))
            { 
                // for some reason there are pending comments; this
                // should never happen - handle gracefully and flush
                // all comments up to g, ignore anything after that
                p.flush(p.posFor(g.List[0L].Pos()), token.ILLEGAL);
                p.comments = p.comments[0L..1L]; 
                // in debug mode, report error
                p.internalError("setComment found pending comments");
            }
            p.comments[0L] = g;
            p.cindex = 0L; 
            // don't overwrite any pending comment in the p.comment cache
            // (there may be a pending comment when a line comment is
            // immediately followed by a lead comment with no other
            // tokens between)
            if (p.commentOffset == infinity)
            {
                p.nextComment(); // get comment ready for use
            }
        }

        private partial struct exprListMode // : ulong
        {
        }

        private static readonly exprListMode commaTerm = 1L << (int)(iota); // list is optionally terminated by a comma
        private static readonly var noIndent = 0; // no extra indentation in multi-line lists

        // If indent is set, a multi-line identifier list is indented after the
        // first linebreak encountered.
        private static void identList(this ref printer p, slice<ref ast.Ident> list, bool indent)
        { 
            // convert into an expression list so we can re-use exprList formatting
            var xlist = make_slice<ast.Expr>(len(list));
            foreach (var (i, x) in list)
            {
                xlist[i] = x;
            }
            exprListMode mode = default;
            if (!indent)
            {
                mode = noIndent;
            }
            p.exprList(token.NoPos, xlist, 1L, mode, token.NoPos);
        }

        // Print a list of expressions. If the list spans multiple
        // source lines, the original line breaks are respected between
        // expressions.
        //
        // TODO(gri) Consider rewriting this to be independent of []ast.Expr
        //           so that we can use the algorithm for any kind of list
        //           (e.g., pass list via a channel over which to range).
        private static void exprList(this ref printer p, token.Pos prev0, slice<ast.Expr> list, long depth, exprListMode mode, token.Pos next0)
        {
            if (len(list) == 0L)
            {
                return;
            }
            var prev = p.posFor(prev0);
            var next = p.posFor(next0);
            var line = p.lineFor(list[0L].Pos());
            var endLine = p.lineFor(list[len(list) - 1L].End());

            if (prev.IsValid() && prev.Line == line && line == endLine)
            { 
                // all list entries on a single line
                {
                    var i__prev1 = i;
                    var x__prev1 = x;

                    foreach (var (__i, __x) in list)
                    {
                        i = __i;
                        x = __x;
                        if (i > 0L)
                        { 
                            // use position of expression following the comma as
                            // comma position for correct comment placement
                            p.print(x.Pos(), token.COMMA, blank);
                        }
                        p.expr0(x, depth);
                    }

                    i = i__prev1;
                    x = x__prev1;
                }

                return;
            } 

            // list entries span multiple lines;
            // use source code positions to guide line breaks

            // don't add extra indentation if noIndent is set;
            // i.e., pretend that the first line is already indented
            var ws = ignore;
            if (mode & noIndent == 0L)
            {
                ws = indent;
            } 

            // the first linebreak is always a formfeed since this section must not
            // depend on any previous formatting
            long prevBreak = -1L; // index of last expression that was followed by a linebreak
            if (prev.IsValid() && prev.Line < line && p.linebreak(line, 0L, ws, true))
            {
                ws = ignore;
                prevBreak = 0L;
            } 

            // initialize expression/key size: a zero value indicates expr/key doesn't fit on a single line
            long size = 0L; 

            // print all list elements
            var prevLine = prev.Line;
            {
                var i__prev1 = i;
                var x__prev1 = x;

                foreach (var (__i, __x) in list)
                {
                    i = __i;
                    x = __x;
                    line = p.lineFor(x.Pos()); 

                    // determine if the next linebreak, if any, needs to use formfeed:
                    // in general, use the entire node size to make the decision; for
                    // key:value expressions, use the key size
                    // TODO(gri) for a better result, should probably incorporate both
                    //           the key and the node size into the decision process
                    var useFF = true; 

                    // determine element size: all bets are off if we don't have
                    // position information for the previous and next token (likely
                    // generated code - simply ignore the size in this case by setting
                    // it to 0)
                    var prevSize = size;
                    const float infinity = 1e6F; // larger than any source line
 // larger than any source line
                    size = p.nodeSize(x, infinity);
                    ref ast.KeyValueExpr (pair, isPair) = x._<ref ast.KeyValueExpr>();
                    if (size <= infinity && prev.IsValid() && next.IsValid())
                    { 
                        // x fits on a single line
                        if (isPair)
                        {
                            size = p.nodeSize(pair.Key, infinity); // size <= infinity
                        }
                    }
                    else
                    { 
                        // size too large or we don't have good layout information
                        size = 0L;
                    } 

                    // if the previous line and the current line had single-
                    // line-expressions and the key sizes are small or the
                    // the ratio between the key sizes does not exceed a
                    // threshold, align columns and do not use formfeed
                    if (prevSize > 0L && size > 0L)
                    {
                        const long smallSize = 20L;

                        if (prevSize <= smallSize && size <= smallSize)
                        {
                            useFF = false;
                        }
                        else
                        {
                            const long r = 4L; // threshold
 // threshold
                            var ratio = float64(size) / float64(prevSize);
                            useFF = ratio <= 1.0F / r || r <= ratio;
                        }
                    }
                    long needsLinebreak = 0L < prevLine && prevLine < line;
                    if (i > 0L)
                    { 
                        // use position of expression following the comma as
                        // comma position for correct comment placement, but
                        // only if the expression is on the same line
                        if (!needsLinebreak)
                        {
                            p.print(x.Pos());
                        }
                        p.print(token.COMMA);
                        var needsBlank = true;
                        if (needsLinebreak)
                        { 
                            // lines are broken using newlines so comments remain aligned
                            // unless forceFF is set or there are multiple expressions on
                            // the same line in which case formfeed is used
                            if (p.linebreak(line, 0L, ws, useFF || prevBreak + 1L < i))
                            {
                                ws = ignore;
                                prevBreak = i;
                                needsBlank = false; // we got a line break instead
                            }
                        }
                        if (needsBlank)
                        {
                            p.print(blank);
                        }
                    }
                    if (len(list) > 1L && isPair && size > 0L && needsLinebreak)
                    { 
                        // we have a key:value expression that fits onto one line
                        // and it's not on the same line as the prior expression:
                        // use a column for the key such that consecutive entries
                        // can align if possible
                        // (needsLinebreak is set if we started a new line before)
                        p.expr(pair.Key);
                        p.print(pair.Colon, token.COLON, vtab);
                        p.expr(pair.Value);
                    }
                    else
                    {
                        p.expr0(x, depth);
                    }
                    prevLine = line;
                }

                i = i__prev1;
                x = x__prev1;
            }

            if (mode & commaTerm != 0L && next.IsValid() && p.pos.Line < next.Line)
            { 
                // print a terminating comma if the next token is on a new line
                p.print(token.COMMA);
                if (ws == ignore && mode & noIndent == 0L)
                { 
                    // unindent if we indented
                    p.print(unindent);
                }
                p.print(formfeed); // terminating comma needs a line break to look good
                return;
            }
            if (ws == ignore && mode & noIndent == 0L)
            { 
                // unindent if we indented
                p.print(unindent);
            }
        }

        private static void parameters(this ref printer p, ref ast.FieldList fields)
        {
            p.print(fields.Opening, token.LPAREN);
            if (len(fields.List) > 0L)
            {
                var prevLine = p.lineFor(fields.Opening);
                var ws = indent;
                foreach (var (i, par) in fields.List)
                { 
                    // determine par begin and end line (may be different
                    // if there are multiple parameter names for this par
                    // or the type is on a separate line)
                    long parLineBeg = default;
                    if (len(par.Names) > 0L)
                    {
                        parLineBeg = p.lineFor(par.Names[0L].Pos());
                    }
                    else
                    {
                        parLineBeg = p.lineFor(par.Type.Pos());
                    }
                    var parLineEnd = p.lineFor(par.Type.End()); 
                    // separating "," if needed
                    long needsLinebreak = 0L < prevLine && prevLine < parLineBeg;
                    if (i > 0L)
                    { 
                        // use position of parameter following the comma as
                        // comma position for correct comma placement, but
                        // only if the next parameter is on the same line
                        if (!needsLinebreak)
                        {
                            p.print(par.Pos());
                        }
                        p.print(token.COMMA);
                    } 
                    // separator if needed (linebreak or blank)
                    if (needsLinebreak && p.linebreak(parLineBeg, 0L, ws, true))
                    { 
                        // break line if the opening "(" or previous parameter ended on a different line
                        ws = ignore;
                    }
                    else if (i > 0L)
                    {
                        p.print(blank);
                    } 
                    // parameter names
                    if (len(par.Names) > 0L)
                    { 
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
                } 
                // if the closing ")" is on a separate line from the last parameter,
                // print an additional "," and line break
                {
                    var closing = p.lineFor(fields.Closing);

                    if (0L < prevLine && prevLine < closing)
                    {
                        p.print(token.COMMA);
                        p.linebreak(closing, 0L, ignore, true);
                    } 
                    // unindent if we indented

                } 
                // unindent if we indented
                if (ws == ignore)
                {
                    p.print(unindent);
                }
            }
            p.print(fields.Closing, token.RPAREN);
        }

        private static void signature(this ref printer p, ref ast.FieldList @params, ref ast.FieldList result)
        {
            if (params != null)
            {
                p.parameters(params);
            }
            else
            {
                p.print(token.LPAREN, token.RPAREN);
            }
            var n = result.NumFields();
            if (n > 0L)
            { 
                // result != nil
                p.print(blank);
                if (n == 1L && result.List[0L].Names == null)
                { 
                    // single anonymous result; no ()'s
                    p.expr(stripParensAlways(result.List[0L].Type));
                    return;
                }
                p.parameters(result);
            }
        }

        private static long identListSize(slice<ref ast.Ident> list, long maxSize)
        {
            foreach (var (i, x) in list)
            {
                if (i > 0L)
                {
                    size += len(", ");
                }
                size += utf8.RuneCountInString(x.Name);
                if (size >= maxSize)
                {
                    break;
                }
            }
            return;
        }

        private static bool isOneLineFieldList(this ref printer p, slice<ref ast.Field> list)
        {
            if (len(list) != 1L)
            {
                return false; // allow only one field
            }
            var f = list[0L];
            if (f.Tag != null || f.Comment != null)
            {
                return false; // don't allow tags or comments
            } 
            // only name(s) and type
            const long maxSize = 30L; // adjust as appropriate, this is an approximate value
 // adjust as appropriate, this is an approximate value
            var namesSize = identListSize(f.Names, maxSize);
            if (namesSize > 0L)
            {
                namesSize = 1L; // blank between names and types
            }
            var typeSize = p.nodeSize(f.Type, maxSize);
            return namesSize + typeSize <= maxSize;
        }

        private static void setLineComment(this ref printer p, @string text)
        {
            p.setComment(ref new ast.CommentGroup(List:[]*ast.Comment{{Slash:token.NoPos,Text:text}}));
        }

        private static void fieldList(this ref printer p, ref ast.FieldList fields, bool isStruct, bool isIncomplete)
        {
            var lbrace = fields.Opening;
            var list = fields.List;
            var rbrace = fields.Closing;
            var hasComments = isIncomplete || p.commentBefore(p.posFor(rbrace));
            var srcIsOneLine = lbrace.IsValid() && rbrace.IsValid() && p.lineFor(lbrace) == p.lineFor(rbrace);

            if (!hasComments && srcIsOneLine)
            { 
                // possibly a one-line struct/interface
                if (len(list) == 0L)
                { 
                    // no blank between keyword and {} in this case
                    p.print(lbrace, token.LBRACE, rbrace, token.RBRACE);
                    return;
                }
                else if (p.isOneLineFieldList(list))
                { 
                    // small enough - print on one line
                    // (don't use identList and ignore source line breaks)
                    p.print(lbrace, token.LBRACE, blank);
                    var f = list[0L];
                    if (isStruct)
                    {
                        {
                            var i__prev1 = i;

                            foreach (var (__i, __x) in f.Names)
                            {
                                i = __i;
                                x = __x;
                                if (i > 0L)
                                { 
                                    // no comments so no need for comma position
                                    p.print(token.COMMA, blank);
                                }
                                p.expr(x);
                            }
                    else

                            i = i__prev1;
                        }

                        if (len(f.Names) > 0L)
                        {
                            p.print(blank);
                        }
                        p.expr(f.Type);
                    }                    { // interface
                        {
                            ref ast.FuncType ftyp__prev5 = ftyp;

                            ref ast.FuncType (ftyp, isFtyp) = f.Type._<ref ast.FuncType>();

                            if (isFtyp)
                            { 
                                // method
                                p.expr(f.Names[0L]);
                                p.signature(ftyp.Params, ftyp.Results);
                            }
                            else
                            { 
                                // embedded interface
                                p.expr(f.Type);
                            }

                            ftyp = ftyp__prev5;

                        }
                    }
                    p.print(blank, rbrace, token.RBRACE);
                    return;
                }
            } 
            // hasComments || !srcIsOneLine
            p.print(blank, lbrace, token.LBRACE, indent);
            if (hasComments || len(list) > 0L)
            {
                p.print(formfeed);
            }
            if (isStruct)
            {
                var sep = vtab;
                if (len(list) == 1L)
                {
                    sep = blank;
                }
                long line = default;
                {
                    var i__prev1 = i;
                    var f__prev1 = f;

                    foreach (var (__i, __f) in list)
                    {
                        i = __i;
                        f = __f;
                        if (i > 0L)
                        {
                            p.linebreak(p.lineFor(f.Pos()), 1L, ignore, p.linesFrom(line) > 0L);
                        }
                        long extraTabs = 0L;
                        p.setComment(f.Doc);
                        p.recordLine(ref line);
                        if (len(f.Names) > 0L)
                        { 
                            // named fields
                            p.identList(f.Names, false);
                            p.print(sep);
                            p.expr(f.Type);
                            extraTabs = 1L;
                        }
                        else
                        { 
                            // anonymous field
                            p.expr(f.Type);
                            extraTabs = 2L;
                        }
                        if (f.Tag != null)
                        {
                            if (len(f.Names) > 0L && sep == vtab)
                            {
                                p.print(sep);
                            }
                            p.print(sep);
                            p.expr(f.Tag);
                            extraTabs = 0L;
                        }
                        if (f.Comment != null)
                        {
                            while (extraTabs > 0L)
                            {
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

                if (isIncomplete)
                {
                    if (len(list) > 0L)
                    {
                        p.print(formfeed);
                    }
                    p.flush(p.posFor(rbrace), token.RBRACE); // make sure we don't lose the last line comment
                    p.setLineComment("// contains filtered or unexported fields");
                }
            }            { // interface

                line = default;
                {
                    var i__prev1 = i;
                    var f__prev1 = f;

                    foreach (var (__i, __f) in list)
                    {
                        i = __i;
                        f = __f;
                        if (i > 0L)
                        {
                            p.linebreak(p.lineFor(f.Pos()), 1L, ignore, p.linesFrom(line) > 0L);
                        }
                        p.setComment(f.Doc);
                        p.recordLine(ref line);
                        {
                            ref ast.FuncType ftyp__prev2 = ftyp;

                            (ftyp, isFtyp) = f.Type._<ref ast.FuncType>();

                            if (isFtyp)
                            { 
                                // method
                                p.expr(f.Names[0L]);
                                p.signature(ftyp.Params, ftyp.Results);
                            }
                            else
                            { 
                                // embedded interface
                                p.expr(f.Type);
                            }

                            ftyp = ftyp__prev2;

                        }
                        p.setComment(f.Comment);
                    }

                    i = i__prev1;
                    f = f__prev1;
                }

                if (isIncomplete)
                {
                    if (len(list) > 0L)
                    {
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

        private static (bool, bool, long) walkBinary(ref ast.BinaryExpr e)
        {
            switch (e.Op.Precedence())
            {
                case 4L: 
                    has4 = true;
                    break;
                case 5L: 
                    has5 = true;
                    break;
            }

            switch (e.X.type())
            {
                case ref ast.BinaryExpr l:
                    if (l.Op.Precedence() < e.Op.Precedence())
                    { 
                        // parens will be inserted.
                        // pretend this is an *ast.ParenExpr and do nothing.
                        break;
                    }
                    var (h4, h5, mp) = walkBinary(l);
                    has4 = has4 || h4;
                    has5 = has5 || h5;
                    if (maxProblem < mp)
                    {
                        maxProblem = mp;
                    }
                    break;

            }

            switch (e.Y.type())
            {
                case ref ast.BinaryExpr r:
                    if (r.Op.Precedence() <= e.Op.Precedence())
                    { 
                        // parens will be inserted.
                        // pretend this is an *ast.ParenExpr and do nothing.
                        break;
                    }
                    (h4, h5, mp) = walkBinary(r);
                    has4 = has4 || h4;
                    has5 = has5 || h5;
                    if (maxProblem < mp)
                    {
                        maxProblem = mp;
                    }
                    break;
                case ref ast.StarExpr r:
                    if (e.Op == token.QUO)
                    { // `*/`
                        maxProblem = 5L;
                    }
                    break;
                case ref ast.UnaryExpr r:
                    switch (e.Op.String() + r.Op.String())
                    {
                        case "/*": 

                        case "&&": 

                        case "&^": 
                            maxProblem = 5L;
                            break;
                        case "++": 

                        case "--": 
                            if (maxProblem < 4L)
                            {
                                maxProblem = 4L;
                            }
                            break;
                    }
                    break;
            }
            return;
        }

        private static long cutoff(ref ast.BinaryExpr e, long depth)
        {
            var (has4, has5, maxProblem) = walkBinary(e);
            if (maxProblem > 0L)
            {
                return maxProblem + 1L;
            }
            if (has4 && has5)
            {
                if (depth == 1L)
                {
                    return 5L;
                }
                return 4L;
            }
            if (depth == 1L)
            {
                return 6L;
            }
            return 4L;
        }

        private static long diffPrec(ast.Expr expr, long prec)
        {
            ref ast.BinaryExpr (x, ok) = expr._<ref ast.BinaryExpr>();
            if (!ok || prec != x.Op.Precedence())
            {
                return 1L;
            }
            return 0L;
        }

        private static long reduceDepth(long depth)
        {
            depth--;
            if (depth < 1L)
            {
                depth = 1L;
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
        private static void binaryExpr(this ref printer p, ref ast.BinaryExpr x, long prec1, long cutoff, long depth)
        {
            var prec = x.Op.Precedence();
            if (prec < prec1)
            { 
                // parenthesis needed
                // Note: The parser inserts an ast.ParenExpr node; thus this case
                //       can only occur if the AST is created in a different way.
                p.print(token.LPAREN);
                p.expr0(x, reduceDepth(depth)); // parentheses undo one level of depth
                p.print(token.RPAREN);
                return;
            }
            var printBlank = prec < cutoff;

            var ws = indent;
            p.expr1(x.X, prec, depth + diffPrec(x.X, prec));
            if (printBlank)
            {
                p.print(blank);
            }
            var xline = p.pos.Line; // before the operator (it may be on the next line!)
            var yline = p.lineFor(x.Y.Pos());
            p.print(x.OpPos, x.Op);
            if (xline != yline && xline > 0L && yline > 0L)
            { 
                // at least one line break, but respect an extra empty line
                // in the source
                if (p.linebreak(yline, 1L, ws, true))
                {
                    ws = ignore;
                    printBlank = false; // no blank after line break
                }
            }
            if (printBlank)
            {
                p.print(blank);
            }
            p.expr1(x.Y, prec + 1L, depth + 1L);
            if (ws == ignore)
            {
                p.print(unindent);
            }
        }

        private static bool isBinary(ast.Expr expr)
        {
            ref ast.BinaryExpr (_, ok) = expr._<ref ast.BinaryExpr>();
            return ok;
        }

        private static void expr1(this ref printer _p, ast.Expr expr, long prec1, long depth) => func(_p, (ref printer p, Defer _, Panic panic, Recover __) =>
        {
            p.print(expr.Pos());

            switch (expr.type())
            {
                case ref ast.BadExpr x:
                    p.print("BadExpr");
                    break;
                case ref ast.Ident x:
                    p.print(x);
                    break;
                case ref ast.BinaryExpr x:
                    if (depth < 1L)
                    {
                        p.internalError("depth < 1:", depth);
                        depth = 1L;
                    }
                    p.binaryExpr(x, prec1, cutoff(x, depth), depth);
                    break;
                case ref ast.KeyValueExpr x:
                    p.expr(x.Key);
                    p.print(x.Colon, token.COLON, blank);
                    p.expr(x.Value);
                    break;
                case ref ast.StarExpr x:
                    const var prec = token.UnaryPrec;

                    if (prec < prec1)
                    { 
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
                case ref ast.UnaryExpr x:
                    const var prec = token.UnaryPrec;

                    if (prec < prec1)
                    { 
                        // parenthesis needed
                        p.print(token.LPAREN);
                        p.expr(x);
                        p.print(token.RPAREN);
                    }
                    else
                    { 
                        // no parenthesis needed
                        p.print(x.Op);
                        if (x.Op == token.RANGE)
                        { 
                            // TODO(gri) Remove this code if it cannot be reached.
                            p.print(blank);
                        }
                        p.expr1(x.X, prec, depth);
                    }
                    break;
                case ref ast.BasicLit x:
                    p.print(x);
                    break;
                case ref ast.FuncLit x:
                    p.expr(x.Type);
                    p.funcBody(p.distanceFrom(x.Type.Pos()), blank, x.Body);
                    break;
                case ref ast.ParenExpr x:
                    {
                        ref ast.ParenExpr (_, hasParens) = x.X._<ref ast.ParenExpr>();

                        if (hasParens)
                        { 
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
                case ref ast.SelectorExpr x:
                    p.selectorExpr(x, depth, false);
                    break;
                case ref ast.TypeAssertExpr x:
                    p.expr1(x.X, token.HighestPrec, depth);
                    p.print(token.PERIOD, x.Lparen, token.LPAREN);
                    if (x.Type != null)
                    {
                        p.expr(x.Type);
                    }
                    else
                    {
                        p.print(token.TYPE);
                    }
                    p.print(x.Rparen, token.RPAREN);
                    break;
                case ref ast.IndexExpr x:
                    p.expr1(x.X, token.HighestPrec, 1L);
                    p.print(x.Lbrack, token.LBRACK);
                    p.expr0(x.Index, depth + 1L);
                    p.print(x.Rbrack, token.RBRACK);
                    break;
                case ref ast.SliceExpr x:
                    p.expr1(x.X, token.HighestPrec, 1L);
                    p.print(x.Lbrack, token.LBRACK);
                    ast.Expr indices = new slice<ast.Expr>(new ast.Expr[] { x.Low, x.High });
                    if (x.Max != null)
                    {
                        indices = append(indices, x.Max);
                    } 
                    // determine if we need extra blanks around ':'
                    bool needsBlanks = default;
                    if (depth <= 1L)
                    {
                        long indexCount = default;
                        bool hasBinaries = default;
                        {
                            var x__prev1 = x;

                            foreach (var (_, __x) in indices)
                            {
                                x = __x;
                                if (x != null)
                                {
                                    indexCount++;
                                    if (isBinary(x))
                                    {
                                        hasBinaries = true;
                                    }
                                }
                            }

                            x = x__prev1;
                        }

                        if (indexCount > 1L && hasBinaries)
                        {
                            needsBlanks = true;
                        }
                    }
                    {
                        var x__prev1 = x;

                        foreach (var (__i, __x) in indices)
                        {
                            i = __i;
                            x = __x;
                            if (i > 0L)
                            {
                                if (indices[i - 1L] != null && needsBlanks)
                                {
                                    p.print(blank);
                                }
                                p.print(token.COLON);
                                if (x != null && needsBlanks)
                                {
                                    p.print(blank);
                                }
                            }
                            if (x != null)
                            {
                                p.expr0(x, depth + 1L);
                            }
                        }

                        x = x__prev1;
                    }

                    p.print(x.Rbrack, token.RBRACK);
                    break;
                case ref ast.CallExpr x:
                    if (len(x.Args) > 1L)
                    {
                        depth++;
                    }
                    bool wasIndented = default;
                    {
                        ref ast.FuncType (_, ok) = x.Fun._<ref ast.FuncType>();

                        if (ok)
                        { 
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
                    if (x.Ellipsis.IsValid())
                    {
                        p.exprList(x.Lparen, x.Args, depth, 0L, x.Ellipsis);
                        p.print(x.Ellipsis, token.ELLIPSIS);
                        if (x.Rparen.IsValid() && p.lineFor(x.Ellipsis) < p.lineFor(x.Rparen))
                        {
                            p.print(token.COMMA, formfeed);
                        }
                    }
                    else
                    {
                        p.exprList(x.Lparen, x.Args, depth, commaTerm, x.Rparen);
                    }
                    p.print(x.Rparen, token.RPAREN);
                    if (wasIndented)
                    {
                        p.print(unindent);
                    }
                    break;
                case ref ast.CompositeLit x:
                    if (x.Type != null)
                    {
                        p.expr1(x.Type, token.HighestPrec, depth);
                    }
                    p.level++;
                    p.print(x.Lbrace, token.LBRACE);
                    p.exprList(x.Lbrace, x.Elts, 1L, commaTerm, x.Rbrace); 
                    // do not insert extra line break following a /*-style comment
                    // before the closing '}' as it might break the code if there
                    // is no trailing ','
                    var mode = noExtraLinebreak; 
                    // do not insert extra blank following a /*-style comment
                    // before the closing '}' unless the literal is empty
                    if (len(x.Elts) > 0L)
                    {
                        mode |= noExtraBlank;
                    } 
                    // need the initial indent to print lone comments with
                    // the proper level of indentation
                    p.print(indent, unindent, mode, x.Rbrace, token.RBRACE, mode);
                    p.level--;
                    break;
                case ref ast.Ellipsis x:
                    p.print(token.ELLIPSIS);
                    if (x.Elt != null)
                    {
                        p.expr(x.Elt);
                    }
                    break;
                case ref ast.ArrayType x:
                    p.print(token.LBRACK);
                    if (x.Len != null)
                    {
                        p.expr(x.Len);
                    }
                    p.print(token.RBRACK);
                    p.expr(x.Elt);
                    break;
                case ref ast.StructType x:
                    p.print(token.STRUCT);
                    p.fieldList(x.Fields, true, x.Incomplete);
                    break;
                case ref ast.FuncType x:
                    p.print(token.FUNC);
                    p.signature(x.Params, x.Results);
                    break;
                case ref ast.InterfaceType x:
                    p.print(token.INTERFACE);
                    p.fieldList(x.Methods, false, x.Incomplete);
                    break;
                case ref ast.MapType x:
                    p.print(token.MAP, token.LBRACK);
                    p.expr(x.Key);
                    p.print(token.RBRACK);
                    p.expr(x.Value);
                    break;
                case ref ast.ChanType x:

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

        private static bool possibleSelectorExpr(this ref printer p, ast.Expr expr, long prec1, long depth)
        {
            {
                ref ast.SelectorExpr (x, ok) = expr._<ref ast.SelectorExpr>();

                if (ok)
                {
                    return p.selectorExpr(x, depth, true);
                }

            }
            p.expr1(expr, prec1, depth);
            return false;
        }

        // selectorExpr handles an *ast.SelectorExpr node and returns whether x spans
        // multiple lines.
        private static bool selectorExpr(this ref printer p, ref ast.SelectorExpr x, long depth, bool isMethod)
        {
            p.expr1(x.X, token.HighestPrec, depth);
            p.print(token.PERIOD);
            {
                var line = p.lineFor(x.Sel.Pos());

                if (p.pos.IsValid() && p.pos.Line < line)
                {
                    p.print(indent, newline, x.Sel.Pos(), x.Sel);
                    if (!isMethod)
                    {
                        p.print(unindent);
                    }
                    return true;
                }

            }
            p.print(x.Sel.Pos(), x.Sel);
            return false;
        }

        private static void expr0(this ref printer p, ast.Expr x, long depth)
        {
            p.expr1(x, token.LowestPrec, depth);
        }

        private static void expr(this ref printer p, ast.Expr x)
        {
            const long depth = 1L;

            p.expr1(x, token.LowestPrec, depth);
        }

        // ----------------------------------------------------------------------------
        // Statements

        // Print the statement list indented, but without a newline after the last statement.
        // Extra line breaks between statements in the source are respected but at most one
        // empty line is printed between statements.
        private static void stmtList(this ref printer p, slice<ast.Stmt> list, long nindent, bool nextIsRBrace)
        {
            if (nindent > 0L)
            {
                p.print(indent);
            }
            long line = default;
            long i = 0L;
            foreach (var (_, s) in list)
            { 
                // ignore empty statements (was issue 3466)
                {
                    ref ast.EmptyStmt (_, isEmpty) = s._<ref ast.EmptyStmt>();

                    if (!isEmpty)
                    { 
                        // nindent == 0 only for lists of switch/select case clauses;
                        // in those cases each clause is a new section
                        if (len(p.output) > 0L)
                        { 
                            // only print line break if we are not at the beginning of the output
                            // (i.e., we are not printing only a partial program)
                            p.linebreak(p.lineFor(s.Pos()), 1L, ignore, i == 0L || nindent == 0L || p.linesFrom(line) > 0L);
                        }
                        p.recordLine(ref line);
                        p.stmt(s, nextIsRBrace && i == len(list) - 1L); 
                        // labeled statements put labels on a separate line, but here
                        // we only care about the start line of the actual statement
                        // without label - correct line for each label
                        {
                            var t = s;

                            while (>>MARKER:FOREXPRESSION_LEVEL_2<<)
                            {
                                ref ast.LabeledStmt (lt, _) = t._<ref ast.LabeledStmt>();
                                if (lt == null)
                                {
                                    break;
                                }
                                line++;
                                t = lt.Stmt;
                            }

                        }
                        i++;
                    }

                }
            }
            if (nindent > 0L)
            {
                p.print(unindent);
            }
        }

        // block prints an *ast.BlockStmt; it always spans at least two lines.
        private static void block(this ref printer p, ref ast.BlockStmt b, long nindent)
        {
            p.print(b.Lbrace, token.LBRACE);
            p.stmtList(b.List, nindent, true);
            p.linebreak(p.lineFor(b.Rbrace), 1L, ignore, true);
            p.print(b.Rbrace, token.RBRACE);
        }

        private static bool isTypeName(ast.Expr x)
        {
            switch (x.type())
            {
                case ref ast.Ident t:
                    return true;
                    break;
                case ref ast.SelectorExpr t:
                    return isTypeName(t.X);
                    break;
            }
            return false;
        }

        private static ast.Expr stripParens(ast.Expr x)
        {
            {
                ref ast.ParenExpr (px, strip) = x._<ref ast.ParenExpr>();

                if (strip)
                { 
                    // parentheses must not be stripped if there are any
                    // unparenthesized composite literals starting with
                    // a type name
                    ast.Inspect(px.X, node =>
                    {
                        switch (node.type())
                        {
                            case ref ast.ParenExpr x:
                                return false;
                                break;
                            case ref ast.CompositeLit x:
                                if (isTypeName(x.Type))
                                {
                                    strip = false; // do not strip parentheses
                                }
                                return false;
                                break; 
                            // in all other cases, keep inspecting
                        } 
                        // in all other cases, keep inspecting
                        return true;
                    });
                    if (strip)
                    {
                        return stripParens(px.X);
                    }
                }

            }
            return x;
        }

        private static ast.Expr stripParensAlways(ast.Expr x)
        {
            {
                ref ast.ParenExpr (x, ok) = x._<ref ast.ParenExpr>();

                if (ok)
                {
                    return stripParensAlways(x.X);
                }

            }
            return x;
        }

        private static void controlClause(this ref printer p, bool isForStmt, ast.Stmt init, ast.Expr expr, ast.Stmt post)
        {
            p.print(blank);
            var needsBlank = false;
            if (init == null && post == null)
            { 
                // no semicolons required
                if (expr != null)
                {
                    p.expr(stripParens(expr));
                    needsBlank = true;
                }
            }
            else
            { 
                // all semicolons required
                // (they are not separators, print them explicitly)
                if (init != null)
                {
                    p.stmt(init, false);
                }
                p.print(token.SEMICOLON, blank);
                if (expr != null)
                {
                    p.expr(stripParens(expr));
                    needsBlank = true;
                }
                if (isForStmt)
                {
                    p.print(token.SEMICOLON, blank);
                    needsBlank = false;
                    if (post != null)
                    {
                        p.stmt(post, false);
                        needsBlank = true;
                    }
                }
            }
            if (needsBlank)
            {
                p.print(blank);
            }
        }

        // indentList reports whether an expression list would look better if it
        // were indented wholesale (starting with the very first element, rather
        // than starting at the first line break).
        //
        private static bool indentList(this ref printer p, slice<ast.Expr> list)
        { 
            // Heuristic: indentList returns true if there are more than one multi-
            // line element in the list, or if there is any element that is not
            // starting on the same line as the previous one ends.
            if (len(list) >= 2L)
            {
                var b = p.lineFor(list[0L].Pos());
                var e = p.lineFor(list[len(list) - 1L].End());
                if (0L < b && b < e)
                { 
                    // list spans multiple lines
                    long n = 0L; // multi-line element count
                    var line = b;
                    foreach (var (_, x) in list)
                    {
                        var xb = p.lineFor(x.Pos());
                        var xe = p.lineFor(x.End());
                        if (line < xb)
                        { 
                            // x is not starting on the same
                            // line as the previous one ended
                            return true;
                        }
                        if (xb < xe)
                        { 
                            // x is a multi-line element
                            n++;
                        }
                        line = xe;
                    }
                    return n > 1L;
                }
            }
            return false;
        }

        private static void stmt(this ref printer _p, ast.Stmt stmt, bool nextIsRBrace) => func(_p, (ref printer p, Defer _, Panic panic, Recover __) =>
        {
            p.print(stmt.Pos());

            switch (stmt.type())
            {
                case ref ast.BadStmt s:
                    p.print("BadStmt");
                    break;
                case ref ast.DeclStmt s:
                    p.decl(s.Decl);
                    break;
                case ref ast.EmptyStmt s:
                    break;
                case ref ast.LabeledStmt s:
                    p.print(unindent);
                    p.expr(s.Label);
                    p.print(s.Colon, token.COLON, indent);
                    {
                        ref ast.EmptyStmt (e, isEmpty) = s.Stmt._<ref ast.EmptyStmt>();

                        if (isEmpty)
                        {
                            if (!nextIsRBrace)
                            {
                                p.print(newline, e.Pos(), token.SEMICOLON);
                                break;
                            }
                        }
                        else
                        {
                            p.linebreak(p.lineFor(s.Stmt.Pos()), 1L, ignore, true);
                        }

                    }
                    p.stmt(s.Stmt, nextIsRBrace);
                    break;
                case ref ast.ExprStmt s:
                    const long depth = 1L;

                    p.expr0(s.X, depth);
                    break;
                case ref ast.SendStmt s:
                    const long depth = 1L;

                    p.expr0(s.Chan, depth);
                    p.print(blank, s.Arrow, token.ARROW, blank);
                    p.expr0(s.Value, depth);
                    break;
                case ref ast.IncDecStmt s:
                    const long depth = 1L;

                    p.expr0(s.X, depth + 1L);
                    p.print(s.TokPos, s.Tok);
                    break;
                case ref ast.AssignStmt s:
                    long depth = 1L;
                    if (len(s.Lhs) > 1L && len(s.Rhs) > 1L)
                    {
                        depth++;
                    }
                    p.exprList(s.Pos(), s.Lhs, depth, 0L, s.TokPos);
                    p.print(blank, s.TokPos, s.Tok, blank);
                    p.exprList(s.TokPos, s.Rhs, depth, 0L, token.NoPos);
                    break;
                case ref ast.GoStmt s:
                    p.print(token.GO, blank);
                    p.expr(s.Call);
                    break;
                case ref ast.DeferStmt s:
                    p.print(token.DEFER, blank);
                    p.expr(s.Call);
                    break;
                case ref ast.ReturnStmt s:
                    p.print(token.RETURN);
                    if (s.Results != null)
                    {
                        p.print(blank); 
                        // Use indentList heuristic to make corner cases look
                        // better (issue 1207). A more systematic approach would
                        // always indent, but this would cause significant
                        // reformatting of the code base and not necessarily
                        // lead to more nicely formatted code in general.
                        if (p.indentList(s.Results))
                        {
                            p.print(indent);
                            p.exprList(s.Pos(), s.Results, 1L, noIndent, token.NoPos);
                            p.print(unindent);
                        }
                        else
                        {
                            p.exprList(s.Pos(), s.Results, 1L, 0L, token.NoPos);
                        }
                    }
                    break;
                case ref ast.BranchStmt s:
                    p.print(s.Tok);
                    if (s.Label != null)
                    {
                        p.print(blank);
                        p.expr(s.Label);
                    }
                    break;
                case ref ast.BlockStmt s:
                    p.block(s, 1L);
                    break;
                case ref ast.IfStmt s:
                    p.print(token.IF);
                    p.controlClause(false, s.Init, s.Cond, null);
                    p.block(s.Body, 1L);
                    if (s.Else != null)
                    {
                        p.print(blank, token.ELSE, blank);
                        switch (s.Else.type())
                        {
                            case ref ast.BlockStmt _:
                                p.stmt(s.Else, nextIsRBrace);
                                break;
                            case ref ast.IfStmt _:
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
                case ref ast.CaseClause s:
                    if (s.List != null)
                    {
                        p.print(token.CASE, blank);
                        p.exprList(s.Pos(), s.List, 1L, 0L, s.Colon);
                    }
                    else
                    {
                        p.print(token.DEFAULT);
                    }
                    p.print(s.Colon, token.COLON);
                    p.stmtList(s.Body, 1L, nextIsRBrace);
                    break;
                case ref ast.SwitchStmt s:
                    p.print(token.SWITCH);
                    p.controlClause(false, s.Init, s.Tag, null);
                    p.block(s.Body, 0L);
                    break;
                case ref ast.TypeSwitchStmt s:
                    p.print(token.SWITCH);
                    if (s.Init != null)
                    {
                        p.print(blank);
                        p.stmt(s.Init, false);
                        p.print(token.SEMICOLON);
                    }
                    p.print(blank);
                    p.stmt(s.Assign, false);
                    p.print(blank);
                    p.block(s.Body, 0L);
                    break;
                case ref ast.CommClause s:
                    if (s.Comm != null)
                    {
                        p.print(token.CASE, blank);
                        p.stmt(s.Comm, false);
                    }
                    else
                    {
                        p.print(token.DEFAULT);
                    }
                    p.print(s.Colon, token.COLON);
                    p.stmtList(s.Body, 1L, nextIsRBrace);
                    break;
                case ref ast.SelectStmt s:
                    p.print(token.SELECT, blank);
                    var body = s.Body;
                    if (len(body.List) == 0L && !p.commentBefore(p.posFor(body.Rbrace)))
                    { 
                        // print empty select statement w/o comments on one line
                        p.print(body.Lbrace, token.LBRACE, body.Rbrace, token.RBRACE);
                    }
                    else
                    {
                        p.block(body, 0L);
                    }
                    break;
                case ref ast.ForStmt s:
                    p.print(token.FOR);
                    p.controlClause(true, s.Init, s.Cond, s.Post);
                    p.block(s.Body, 1L);
                    break;
                case ref ast.RangeStmt s:
                    p.print(token.FOR, blank);
                    if (s.Key != null)
                    {
                        p.expr(s.Key);
                        if (s.Value != null)
                        { 
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
                    p.block(s.Body, 1L);
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
        private static slice<bool> keepTypeColumn(slice<ast.Spec> specs)
        {
            var m = make_slice<bool>(len(specs));

            Action<long, long, bool> populate = (i, j, keepType) =>
            {
                if (keepType)
                {
                    while (i < j)
                    {
                        m[i] = true;
                        i++;
                    }

                }
            }
;

            long i0 = -1L; // if i0 >= 0 we are in a run and i0 is the start of the run
            bool keepType = default;
            foreach (var (i, s) in specs)
            {
                ref ast.ValueSpec t = s._<ref ast.ValueSpec>();
                if (t.Values != null)
                {
                    if (i0 < 0L)
                    { 
                        // start of a run of ValueSpecs with non-nil Values
                        i0 = i;
                        keepType = false;
                    }
                }
                else
                {
                    if (i0 >= 0L)
                    { 
                        // end of a run
                        populate(i0, i, keepType);
                        i0 = -1L;
                    }
                }
                if (t.Type != null)
                {
                    keepType = true;
                }
            }
            if (i0 >= 0L)
            { 
                // end of a run
                populate(i0, len(specs), keepType);
            }
            return m;
        }

        private static void valueSpec(this ref printer p, ref ast.ValueSpec s, bool keepType)
        {
            p.setComment(s.Doc);
            p.identList(s.Names, false); // always present
            long extraTabs = 3L;
            if (s.Type != null || keepType)
            {
                p.print(vtab);
                extraTabs--;
            }
            if (s.Type != null)
            {
                p.expr(s.Type);
            }
            if (s.Values != null)
            {
                p.print(vtab, token.ASSIGN, blank);
                p.exprList(token.NoPos, s.Values, 1L, 0L, token.NoPos);
                extraTabs--;
            }
            if (s.Comment != null)
            {
                while (extraTabs > 0L)
                {
                    p.print(vtab);
                    extraTabs--;
                }

                p.setComment(s.Comment);
            }
        }

        private static ref ast.BasicLit sanitizeImportPath(ref ast.BasicLit lit)
        { 
            // Note: An unmodified AST generated by go/parser will already
            // contain a backward- or double-quoted path string that does
            // not contain any invalid characters, and most of the work
            // here is not needed. However, a modified or generated AST
            // may possibly contain non-canonical paths. Do the work in
            // all cases since it's not too hard and not speed-critical.

            // if we don't have a proper string, be conservative and return whatever we have
            if (lit.Kind != token.STRING)
            {
                return lit;
            }
            var (s, err) = strconv.Unquote(lit.Value);
            if (err != null)
            {
                return lit;
            } 

            // if the string is an invalid path, return whatever we have
            //
            // spec: "Implementation restriction: A compiler may restrict
            // ImportPaths to non-empty strings using only characters belonging
            // to Unicode's L, M, N, P, and S general categories (the Graphic
            // characters without spaces) and may also exclude the characters
            // !"#$%&'()*,:;<=>?[\]^`{|} and the Unicode replacement character
            // U+FFFD."
            if (s == "")
            {
                return lit;
            }
            const @string illegalChars = "!\"#$%&\'()*,:;<=>?[\\]^{|}" + "`\uFFFD";

            foreach (var (_, r) in s)
            {
                if (!unicode.IsGraphic(r) || unicode.IsSpace(r) || strings.ContainsRune(illegalChars, r))
                {
                    return lit;
                }
            } 

            // otherwise, return the double-quoted path
            s = strconv.Quote(s);
            if (s == lit.Value)
            {
                return lit; // nothing wrong with lit
            }
            return ref new ast.BasicLit(ValuePos:lit.ValuePos,Kind:token.STRING,Value:s);
        }

        // The parameter n is the number of specs in the group. If doIndent is set,
        // multi-line identifier lists in the spec are indented when the first
        // linebreak is encountered.
        //
        private static void spec(this ref printer _p, ast.Spec spec, long n, bool doIndent) => func(_p, (ref printer p, Defer _, Panic panic, Recover __) =>
        {
            switch (spec.type())
            {
                case ref ast.ImportSpec s:
                    p.setComment(s.Doc);
                    if (s.Name != null)
                    {
                        p.expr(s.Name);
                        p.print(blank);
                    }
                    p.expr(sanitizeImportPath(s.Path));
                    p.setComment(s.Comment);
                    p.print(s.EndPos);
                    break;
                case ref ast.ValueSpec s:
                    if (n != 1L)
                    {
                        p.internalError("expected n = 1; got", n);
                    }
                    p.setComment(s.Doc);
                    p.identList(s.Names, doIndent); // always present
                    if (s.Type != null)
                    {
                        p.print(blank);
                        p.expr(s.Type);
                    }
                    if (s.Values != null)
                    {
                        p.print(blank, token.ASSIGN, blank);
                        p.exprList(token.NoPos, s.Values, 1L, 0L, token.NoPos);
                    }
                    p.setComment(s.Comment);
                    break;
                case ref ast.TypeSpec s:
                    p.setComment(s.Doc);
                    p.expr(s.Name);
                    if (n == 1L)
                    {
                        p.print(blank);
                    }
                    else
                    {
                        p.print(vtab);
                    }
                    if (s.Assign.IsValid())
                    {
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

        private static void genDecl(this ref printer p, ref ast.GenDecl d)
        {
            p.setComment(d.Doc);
            p.print(d.Pos(), d.Tok, blank);

            if (d.Lparen.IsValid())
            { 
                // group of parenthesized declarations
                p.print(d.Lparen, token.LPAREN);
                {
                    var n = len(d.Specs);

                    if (n > 0L)
                    {
                        p.print(indent, formfeed);
                        if (n > 1L && (d.Tok == token.CONST || d.Tok == token.VAR))
                        { 
                            // two or more grouped const/var declarations:
                            // determine if the type column must be kept
                            var keepType = keepTypeColumn(d.Specs);
                            long line = default;
                            {
                                var i__prev1 = i;
                                var s__prev1 = s;

                                foreach (var (__i, __s) in d.Specs)
                                {
                                    i = __i;
                                    s = __s;
                                    if (i > 0L)
                                    {
                                        p.linebreak(p.lineFor(s.Pos()), 1L, ignore, p.linesFrom(line) > 0L);
                                    }
                                    p.recordLine(ref line);
                                    p.valueSpec(s._<ref ast.ValueSpec>(), keepType[i]);
                                }
                        else

                                i = i__prev1;
                                s = s__prev1;
                            }

                        }                        {
                            line = default;
                            {
                                var i__prev1 = i;
                                var s__prev1 = s;

                                foreach (var (__i, __s) in d.Specs)
                                {
                                    i = __i;
                                    s = __s;
                                    if (i > 0L)
                                    {
                                        p.linebreak(p.lineFor(s.Pos()), 1L, ignore, p.linesFrom(line) > 0L);
                                    }
                                    p.recordLine(ref line);
                                    p.spec(s, n, false);
                                }

                                i = i__prev1;
                                s = s__prev1;
                            }

                        }
            else
                        p.print(unindent, formfeed);
                    }

                }
                p.print(d.Rparen, token.RPAREN);

            }            { 
                // single declaration
                p.spec(d.Specs[0L], 1L, true);
            }
        }

        // nodeSize determines the size of n in chars after formatting.
        // The result is <= maxSize if the node fits on one line with at
        // most maxSize chars and the formatted output doesn't contain
        // any control chars. Otherwise, the result is > maxSize.
        //
        private static long nodeSize(this ref printer p, ast.Node n, long maxSize)
        { 
            // nodeSize invokes the printer, which may invoke nodeSize
            // recursively. For deep composite literal nests, this can
            // lead to an exponential algorithm. Remember previous
            // results to prune the recursion (was issue 1628).
            {
                var (size, found) = p.nodeSizes[n];

                if (found)
                {
                    return size;
                }

            }

            size = maxSize + 1L; // assume n doesn't fit
            p.nodeSizes[n] = size; 

            // nodeSize computation must be independent of particular
            // style so that we always get the same decision; print
            // in RawFormat
            Config cfg = new Config(Mode:RawFormat);
            bytes.Buffer buf = default;
            {
                var err = cfg.fprint(ref buf, p.fset, n, p.nodeSizes);

                if (err != null)
                {
                    return;
                }

            }
            if (buf.Len() <= maxSize)
            {
                foreach (var (_, ch) in buf.Bytes())
                {
                    if (ch < ' ')
                    {
                        return;
                    }
                }
                size = buf.Len(); // n fits
                p.nodeSizes[n] = size;
            }
            return;
        }

        // numLines returns the number of lines spanned by node n in the original source.
        private static long numLines(this ref printer p, ast.Node n)
        {
            {
                var from = n.Pos();

                if (from.IsValid())
                {
                    {
                        var to = n.End();

                        if (to.IsValid())
                        {
                            return p.lineFor(to) - p.lineFor(from) + 1L;
                        }

                    }
                }

            }
            return infinity;
        }

        // bodySize is like nodeSize but it is specialized for *ast.BlockStmt's.
        private static long bodySize(this ref printer p, ref ast.BlockStmt b, long maxSize)
        {
            var pos1 = b.Pos();
            var pos2 = b.Rbrace;
            if (pos1.IsValid() && pos2.IsValid() && p.lineFor(pos1) != p.lineFor(pos2))
            { 
                // opening and closing brace are on different lines - don't make it a one-liner
                return maxSize + 1L;
            }
            if (len(b.List) > 5L)
            { 
                // too many statements - don't make it a one-liner
                return maxSize + 1L;
            } 
            // otherwise, estimate body size
            var bodySize = p.commentSizeBefore(p.posFor(pos2));
            foreach (var (i, s) in b.List)
            {
                if (bodySize > maxSize)
                {
                    break; // no need to continue
                }
                if (i > 0L)
                {
                    bodySize += 2L; // space for a semicolon and blank
                }
                bodySize += p.nodeSize(s, maxSize);
            }
            return bodySize;
        }

        // funcBody prints a function body following a function header of given headerSize.
        // If the header's and block's size are "small enough" and the block is "simple enough",
        // the block is printed on the current line, without line breaks, spaced from the header
        // by sep. Otherwise the block's opening "{" is printed on the current line, followed by
        // lines for the block's statements and its closing "}".
        //
        private static void funcBody(this ref printer _p, long headerSize, whiteSpace sep, ref ast.BlockStmt _b) => func(_p, _b, (ref printer p, ref ast.BlockStmt b, Defer defer, Panic _, Recover __) =>
        {
            if (b == null)
            {
                return;
            } 

            // save/restore composite literal nesting level
            defer(level =>
            {
                p.level = level;
            }(p.level));
            p.level = 0L;

            const long maxSize = 100L;

            if (headerSize + p.bodySize(b, maxSize) <= maxSize)
            {
                p.print(sep, b.Lbrace, token.LBRACE);
                if (len(b.List) > 0L)
                {
                    p.print(blank);
                    foreach (var (i, s) in b.List)
                    {
                        if (i > 0L)
                        {
                            p.print(token.SEMICOLON, blank);
                        }
                        p.stmt(s, i == len(b.List) - 1L);
                    }
                    p.print(blank);
                }
                p.print(noExtraLinebreak, b.Rbrace, token.RBRACE, noExtraLinebreak);
                return;
            }
            if (sep != ignore)
            {
                p.print(blank); // always use blank
            }
            p.block(b, 1L);
        });

        // distanceFrom returns the column difference between from and p.pos (the current
        // estimated position) if both are on the same line; if they are on different lines
        // (or unknown) the result is infinity.
        private static long distanceFrom(this ref printer p, token.Pos from)
        {
            if (from.IsValid() && p.pos.IsValid())
            {
                {
                    var f = p.posFor(from);

                    if (f.Line == p.pos.Line)
                    {
                        return p.pos.Column - f.Column;
                    }

                }
            }
            return infinity;
        }

        private static void funcDecl(this ref printer p, ref ast.FuncDecl d)
        {
            p.setComment(d.Doc);
            p.print(d.Pos(), token.FUNC, blank);
            if (d.Recv != null)
            {
                p.parameters(d.Recv); // method: print receiver
                p.print(blank);
            }
            p.expr(d.Name);
            p.signature(d.Type.Params, d.Type.Results);
            p.funcBody(p.distanceFrom(d.Pos()), vtab, d.Body);
        }

        private static void decl(this ref printer _p, ast.Decl decl) => func(_p, (ref printer p, Defer _, Panic panic, Recover __) =>
        {
            switch (decl.type())
            {
                case ref ast.BadDecl d:
                    p.print(d.Pos(), "BadDecl");
                    break;
                case ref ast.GenDecl d:
                    p.genDecl(d);
                    break;
                case ref ast.FuncDecl d:
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

        private static token.Token declToken(ast.Decl decl)
        {
            tok = token.ILLEGAL;
            switch (decl.type())
            {
                case ref ast.GenDecl d:
                    tok = d.Tok;
                    break;
                case ref ast.FuncDecl d:
                    tok = token.FUNC;
                    break;
            }
            return;
        }

        private static void declList(this ref printer p, slice<ast.Decl> list)
        {
            var tok = token.ILLEGAL;
            foreach (var (_, d) in list)
            {
                var prev = tok;
                tok = declToken(d); 
                // If the declaration token changed (e.g., from CONST to TYPE)
                // or the next declaration has documentation associated with it,
                // print an empty line between top-level declarations.
                // (because p.linebreak is called with the position of d, which
                // is past any documentation, the minimum requirement is satisfied
                // even w/o the extra getDoc(d) nil-check - leave it in case the
                // linebreak logic improves - there's already a TODO).
                if (len(p.output) > 0L)
                { 
                    // only print line break if we are not at the beginning of the output
                    // (i.e., we are not printing only a partial program)
                    long min = 1L;
                    if (prev != tok || getDoc(d) != null)
                    {
                        min = 2L;
                    } 
                    // start a new section if the next declaration is a function
                    // that spans multiple lines (see also issue #19544)
                    p.linebreak(p.lineFor(d.Pos()), min, ignore, tok == token.FUNC && p.numLines(d) > 1L);
                }
                p.decl(d);
            }
        }

        private static void file(this ref printer p, ref ast.File src)
        {
            p.setComment(src.Doc);
            p.print(src.Pos(), token.PACKAGE, blank);
            p.expr(src.Name);
            p.declList(src.Decls);
            p.print(newline);
        }
    }
}}
