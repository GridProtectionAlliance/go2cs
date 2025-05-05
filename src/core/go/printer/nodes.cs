// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements printing of AST nodes; specifically
// expressions, statements, declarations, and files. It uses
// the print functionality implemented in printer.go.
namespace go.go;

using ast = go.ast_package;
using token = go.token_package;
using math = math_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;
using unicode;

partial class printer_package {

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
// is preceded by comments because the computation of n assumes
// the current position before the comment and the target position
// after the comment. Thus, after interspersing such comments, the
// space taken up by them is not considered to reduce the number of
// linebreaks. At the moment there is no easy way to know about
// future (not yet interspersed) comments in this function.
[GoRecv] internal static nint /*nbreaks*/ linebreak(this ref printer p, nint line, nint min, whiteSpace ws, bool newSection) {
    nint nbreaks = default!;

    nint n = max(nlimit(line - p.pos.Line), min);
    if (n > 0) {
        p.print(ws);
        if (newSection) {
            p.print(formfeed);
            n--;
            nbreaks = 2;
        }
        nbreaks += n;
        for (; n > 0; n--) {
            p.print(newline);
        }
    }
    return nbreaks;
}

// setComment sets g as the next comment if g != nil and if node comments
// are enabled - this mode is used when printing source code fragments such
// as exports only. It assumes that there is no pending comment in p.comments
// and at most one pending comment in the p.comment cache.
[GoRecv] internal static void setComment(this ref printer p, ж<ast.CommentGroup> Ꮡg) {
    ref var g = ref Ꮡg.val;

    if (g == nil || !p.useNodeComments) {
        return;
    }
    if (p.comments == default!){
        // initialize p.comments lazily
        p.comments = new slice<ast.CommentGroup>(1);
    } else 
    if (p.cindex < len(p.comments)) {
        // for some reason there are pending comments; this
        // should never happen - handle gracefully and flush
        // all comments up to g, ignore anything after that
        p.flush(p.posFor(g.List[0].Pos()), token.ILLEGAL);
        p.comments = p.comments[0..1];
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
        p.nextComment();
    }
}

[GoType("num:nuint")] partial struct exprListMode;

// get comment ready for use
internal static readonly exprListMode commaTerm = /* 1 << iota */ 1;     // list is optionally terminated by a comma
internal static readonly exprListMode noIndent = 2;      // no extra indentation in multi-line lists

// If indent is set, a multi-line identifier list is indented after the
// first linebreak encountered.
[GoRecv] internal static void identList(this ref printer p, slice<ast.Ident> list, bool indent) {
    // convert into an expression list so we can re-use exprList formatting
    var xlist = new slice<ast.Expr>(len(list));
    foreach (var (i, x) in list) {
        xlist[i] = x;
    }
    exprListMode mode = default!;
    if (!indent) {
        mode = noIndent;
    }
    p.exprList(token.NoPos, xlist, 1, mode, token.NoPos, false);
}

internal static readonly @string filteredMsg = "contains filtered or unexported fields"u8;

// Print a list of expressions. If the list spans multiple
// source lines, the original line breaks are respected between
// expressions.
//
// TODO(gri) Consider rewriting this to be independent of []ast.Expr
// so that we can use the algorithm for any kind of list
//
//	(e.g., pass list via a channel over which to range).
[GoRecv] internal static void exprList(this ref printer p, tokenꓸPos prev0, slice<ast.Expr> list, nint depth, exprListMode mode, tokenꓸPos next0, bool isIncomplete) {
    if (len(list) == 0) {
        if (isIncomplete) {
            var prevΔ1 = p.posFor(prev0);
            var nextΔ1 = p.posFor(next0);
            if (prevΔ1.IsValid() && prevΔ1.Line == nextΔ1.Line){
                p.print("/* " + filteredMsg + " */");
            } else {
                p.print(newline);
                p.print(indent, "// " + filteredMsg, unindent, newline);
            }
        }
        return;
    }
    var prev = p.posFor(prev0);
    var next = p.posFor(next0);
    nint line = p.lineFor(list[0].Pos());
    nint endLine = p.lineFor(list[len(list) - 1].End());
    if (prev.IsValid() && prev.Line == line && line == endLine) {
        // all list entries on a single line
        foreach (var (i, x) in list) {
            if (i > 0) {
                // use position of expression following the comma as
                // comma position for correct comment placement
                p.setPos(x.Pos());
                p.print(token.COMMA, blank);
            }
            p.expr0(x, depth);
        }
        if (isIncomplete) {
            p.print(token.COMMA, blank, "/* " + filteredMsg + " */");
        }
        return;
    }
    // list entries span multiple lines;
    // use source code positions to guide line breaks
    // Don't add extra indentation if noIndent is set;
    // i.e., pretend that the first line is already indented.
    var ws = ignore;
    if ((exprListMode)(mode & noIndent) == 0) {
        ws = indent;
    }
    // The first linebreak is always a formfeed since this section must not
    // depend on any previous formatting.
    nint prevBreak = -1;
    // index of last expression that was followed by a linebreak
    if (prev.IsValid() && prev.Line < line && p.linebreak(line, 0, ws, true) > 0) {
        ws = ignore;
        prevBreak = 0;
    }
    // initialize expression/key size: a zero value indicates expr/key doesn't fit on a single line
    nint size = 0;
    // We use the ratio between the geometric mean of the previous key sizes and
    // the current size to determine if there should be a break in the alignment.
    // To compute the geometric mean we accumulate the ln(size) values (lnsum)
    // and the number of sizes included (count).
    var lnsum = 0.0F;
    nint count = 0;
    // print all list elements
    nint prevLine = prev.Line;
    foreach (var (i, x) in list) {
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
        nint prevSize = size;
        static readonly UntypedFloat infinity = 1e+06; // larger than any source line
        size = p.nodeSize(x, infinity);
        var (pair, isPair) = x._<ж<ast.KeyValueExpr>>(ᐧ);
        if (size <= infinity && prev.IsValid() && next.IsValid()){
            // x fits on a single line
            if (isPair) {
                size = p.nodeSize((~pair).Key, infinity);
            }
        } else {
            // size <= infinity
            // size too large or we don't have good layout information
            size = 0;
        }
        // If the previous line and the current line had single-
        // line-expressions and the key sizes are small or the
        // ratio between the current key and the geometric mean
        // if the previous key sizes does not exceed a threshold,
        // align columns and do not use formfeed.
        if (prevSize > 0 && size > 0) {
            static readonly UntypedInt smallSize = 40;
            if (count == 0 || prevSize <= smallSize && size <= smallSize){
                useFF = false;
            } else {
                static readonly UntypedFloat r = 2.5;              // threshold
                var geomean = math.Exp(lnsum / ((float64)count));
                // count > 0
                var ratio = ((float64)size) / geomean;
                useFF = r * ratio <= 1 || r <= ratio;
            }
        }
        var needsLinebreak = 0 < prevLine && prevLine < line;
        if (i > 0) {
            // Use position of expression following the comma as
            // comma position for correct comment placement, but
            // only if the expression is on the same line.
            if (!needsLinebreak) {
                p.setPos(x.Pos());
            }
            p.print(token.COMMA);
            var needsBlank = true;
            if (needsLinebreak) {
                // Lines are broken using newlines so comments remain aligned
                // unless useFF is set or there are multiple expressions on
                // the same line in which case formfeed is used.
                nint nbreaks = p.linebreak(line, 0, ws, useFF || prevBreak + 1 < i);
                if (nbreaks > 0) {
                    ws = ignore;
                    prevBreak = i;
                    needsBlank = false;
                }
                // we got a line break instead
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
        if (len(list) > 1 && isPair && size > 0 && needsLinebreak){
            // We have a key:value expression that fits onto one line
            // and it's not on the same line as the prior expression:
            // Use a column for the key such that consecutive entries
            // can align if possible.
            // (needsLinebreak is set if we started a new line before)
            p.expr((~pair).Key);
            p.setPos((~pair).Colon);
            p.print(token.COLON, vtab);
            p.expr((~pair).Value);
        } else {
            p.expr0(x, depth);
        }
        if (size > 0) {
            lnsum += math.Log(((float64)size));
            count++;
        }
        prevLine = line;
    }
    if ((exprListMode)(mode & commaTerm) != 0 && next.IsValid() && p.pos.Line < next.Line) {
        // Print a terminating comma if the next token is on a new line.
        p.print(token.COMMA);
        if (isIncomplete) {
            p.print(newline);
            p.print("// " + filteredMsg);
        }
        if (ws == ignore && (exprListMode)(mode & noIndent) == 0) {
            // unindent if we indented
            p.print(unindent);
        }
        p.print(formfeed);
        // terminating comma needs a line break to look good
        return;
    }
    if (isIncomplete) {
        p.print(token.COMMA, newline);
        p.print("// " + filteredMsg, newline);
    }
    if (ws == ignore && (exprListMode)(mode & noIndent) == 0) {
        // unindent if we indented
        p.print(unindent);
    }
}

[GoType("num:nint")] partial struct paramMode;

internal static readonly paramMode funcParam = /* iota */ 0;
internal static readonly paramMode funcTParam = 1;
internal static readonly paramMode typeTParam = 2;

[GoRecv] internal static void parameters(this ref printer p, ж<ast.FieldList> Ꮡfields, paramMode mode) {
    ref var fields = ref Ꮡfields.val;

    token.Token openTok = token.LPAREN;
    token.Token closeTok = token.RPAREN;
    if (mode != funcParam) {
        (openTok, closeTok) = (token.LBRACK, token.RBRACK);
    }
    p.setPos(fields.Opening);
    p.print(openTok);
    if (len(fields.List) > 0) {
        nint prevLine = p.lineFor(fields.Opening);
        var ws = indent;
        foreach (var (i, par) in fields.List) {
            // determine par begin and end line (may be different
            // if there are multiple parameter names for this par
            // or the type is on a separate line)
            nint parLineBeg = p.lineFor(par.Pos());
            nint parLineEnd = p.lineFor(par.End());
            // separating "," if needed
            var needsLinebreak = 0 < prevLine && prevLine < parLineBeg;
            if (i > 0) {
                // use position of parameter following the comma as
                // comma position for correct comma placement, but
                // only if the next parameter is on the same line
                if (!needsLinebreak) {
                    p.setPos(par.Pos());
                }
                p.print(token.COMMA);
            }
            // separator if needed (linebreak or blank)
            if (needsLinebreak && p.linebreak(parLineBeg, 0, ws, true) > 0){
                // break line if the opening "(" or previous parameter ended on a different line
                ws = ignore;
            } else 
            if (i > 0) {
                p.print(blank);
            }
            // parameter names
            if (len((~par).Names) > 0) {
                // Very subtle: If we indented before (ws == ignore), identList
                // won't indent again. If we didn't (ws == indent), identList will
                // indent if the identList spans multiple lines, and it will outdent
                // again at the end (and still ws == indent). Thus, a subsequent indent
                // by a linebreak call after a type, or in the next multi-line identList
                // will do the right thing.
                p.identList((~par).Names, ws == indent);
                p.print(blank);
            }
            // parameter type
            p.expr(stripParensAlways((~par).Type));
            prevLine = parLineEnd;
        }
        // if the closing ")" is on a separate line from the last parameter,
        // print an additional "," and line break
        {
            nint closing = p.lineFor(fields.Closing); if (0 < prevLine && prevLine < closing){
                p.print(token.COMMA);
                p.linebreak(closing, 0, ignore, true);
            } else 
            if (mode == typeTParam && fields.NumFields() == 1 && combinesWithName(fields.List[0].Type)) {
                // A type parameter list [P T] where the name P and the type expression T syntactically
                // combine to another valid (value) expression requires a trailing comma, as in [P *T,]
                // (or an enclosing interface as in [P interface(*T)]), so that the type parameter list
                // is not parsed as an array length [P*T].
                p.print(token.COMMA);
            }
        }
        // unindent if we indented
        if (ws == ignore) {
            p.print(unindent);
        }
    }
    p.setPos(fields.Closing);
    p.print(closeTok);
}

// combinesWithName reports whether a name followed by the expression x
// syntactically combines to another valid (value) expression. For instance
// using *T for x, "name *T" syntactically appears as the expression x*T.
// On the other hand, using  P|Q or *P|~Q for x, "name P|Q" or name *P|~Q"
// cannot be combined into a valid (value) expression.
internal static bool combinesWithName(ast.Expr x) {
    switch (x.type()) {
    case ж<ast.StarExpr> x: {
        return !isTypeElem((~x).X);
    }
    case ж<ast.BinaryExpr> x: {
        return combinesWithName((~x).X) && !isTypeElem((~x).Y);
    }
    case ж<ast.ParenExpr> x: {
        throw panic("unexpected parenthesized expression");
        break;
    }}
    // name *x.X combines to name*x.X if x.X is not a type element
    // name(x) combines but we are making sure at
    // the call site that x is never parenthesized.
    return false;
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
    case ж<ast.UnaryExpr> x: {
        return (~x).Op == token.TILDE;
    }
    case ж<ast.BinaryExpr> x: {
        return isTypeElem((~x).X) || isTypeElem((~x).Y);
    }
    case ж<ast.ParenExpr> x: {
        return isTypeElem((~x).X);
    }}
    return false;
}

[GoRecv] internal static void signature(this ref printer p, ж<ast.FuncType> Ꮡsig) {
    ref var sig = ref Ꮡsig.val;

    if (sig.TypeParams != nil) {
        p.parameters(sig.TypeParams, funcTParam);
    }
    if (sig.Params != nil){
        p.parameters(sig.Params, funcParam);
    } else {
        p.print(token.LPAREN, token.RPAREN);
    }
    var res = sig.Results;
    nint n = res.NumFields();
    if (n > 0) {
        // res != nil
        p.print(blank);
        if (n == 1 && (~(~res).List[0]).Names == default!) {
            // single anonymous res; no ()'s
            p.expr(stripParensAlways((~(~res).List[0]).Type));
            return;
        }
        p.parameters(res, funcParam);
    }
}

internal static nint /*size*/ identListSize(slice<ast.Ident> list, nint maxSize) {
    nint size = default!;

    foreach (var (i, x) in list) {
        if (i > 0) {
            size += len(", ");
        }
        size += utf8.RuneCountInString((~x).Name);
        if (size >= maxSize) {
            break;
        }
    }
    return size;
}

[GoRecv] internal static bool isOneLineFieldList(this ref printer p, slice<ast.Field> list) {
    if (len(list) != 1) {
        return false;
    }
    // allow only one field
    var f = list[0];
    if ((~f).Tag != nil || (~f).Comment != nil) {
        return false;
    }
    // don't allow tags or comments
    // only name(s) and type
    static readonly UntypedInt maxSize = 30; // adjust as appropriate, this is an approximate value
    nint namesSize = identListSize((~f).Names, maxSize);
    if (namesSize > 0) {
        namesSize = 1;
    }
    // blank between names and types
    nint typeSize = p.nodeSize((~f).Type, maxSize);
    return namesSize + typeSize <= maxSize;
}

[GoRecv] internal static void setLineComment(this ref printer p, @string text) {
    p.setComment(Ꮡ(new ast.CommentGroup(List: new ast.Comment[]{new(Slash: token.NoPos, Text: text)}.slice())));
}

[GoRecv] internal static void fieldList(this ref printer p, ж<ast.FieldList> Ꮡfields, bool isStruct, bool isIncomplete) {
    ref var fields = ref Ꮡfields.val;

    tokenꓸPos lbrace = fields.Opening;
    var list = fields.List;
    tokenꓸPos rbrace = fields.Closing;
    var hasComments = isIncomplete || p.commentBefore(p.posFor(rbrace));
    var srcIsOneLine = lbrace.IsValid() && rbrace.IsValid() && p.lineFor(lbrace) == p.lineFor(rbrace);
    if (!hasComments && srcIsOneLine) {
        // possibly a one-line struct/interface
        if (len(list) == 0){
            // no blank between keyword and {} in this case
            p.setPos(lbrace);
            p.print(token.LBRACE);
            p.setPos(rbrace);
            p.print(token.RBRACE);
            return;
        } else 
        if (p.isOneLineFieldList(list)) {
            // small enough - print on one line
            // (don't use identList and ignore source line breaks)
            p.setPos(lbrace);
            p.print(token.LBRACE, blank);
            var f = list[0];
            if (isStruct){
                foreach (var (i, x) in (~f).Names) {
                    if (i > 0) {
                        // no comments so no need for comma position
                        p.print(token.COMMA, blank);
                    }
                    p.expr(~x);
                }
                if (len((~f).Names) > 0) {
                    p.print(blank);
                }
                p.expr((~f).Type);
            } else {
                // interface
                if (len((~f).Names) > 0){
                    var nameΔ1 = (~f).Names[0];
                    // method name
                    p.expr(~nameΔ1);
                    p.signature((~f).Type._<ж<ast.FuncType>>());
                } else {
                    // don't print "func"
                    // embedded interface
                    p.expr((~f).Type);
                }
            }
            p.print(blank);
            p.setPos(rbrace);
            p.print(token.RBRACE);
            return;
        }
    }
    // hasComments || !srcIsOneLine
    p.print(blank);
    p.setPos(lbrace);
    p.print(token.LBRACE, indent);
    if (hasComments || len(list) > 0) {
        p.print(formfeed);
    }
    if (isStruct){
        var sep = vtab;
        if (len(list) == 1) {
            sep = blank;
        }
        ref var lineΔ1 = ref heap(new nint(), out var ᏑlineΔ1);
        foreach (var (i, f) in list) {
            if (i > 0) {
                p.linebreak(p.lineFor(f.Pos()), 1, ignore, p.linesFrom(lineΔ1) > 0);
            }
            nint extraTabs = 0;
            p.setComment((~f).Doc);
            p.recordLine(ᏑlineΔ1);
            if (len((~f).Names) > 0){
                // named fields
                p.identList((~f).Names, false);
                p.print(sep);
                p.expr((~f).Type);
                extraTabs = 1;
            } else {
                // anonymous field
                p.expr((~f).Type);
                extraTabs = 2;
            }
            if ((~f).Tag != nil) {
                if (len((~f).Names) > 0 && sep == vtab) {
                    p.print(sep);
                }
                p.print(sep);
                p.expr(~(~f).Tag);
                extraTabs = 0;
            }
            if ((~f).Comment != nil) {
                for (; extraTabs > 0; extraTabs--) {
                    p.print(sep);
                }
                p.setComment((~f).Comment);
            }
        }
        if (isIncomplete) {
            if (len(list) > 0) {
                p.print(formfeed);
            }
            p.flush(p.posFor(rbrace), token.RBRACE);
            // make sure we don't lose the last line comment
            p.setLineComment("// " + filteredMsg);
        }
    } else {
        // interface
        ref var line = ref heap(new nint(), out var Ꮡline);
        ж<ast.Ident> prev = default!;                  // previous "type" identifier
        foreach (var (i, f) in list) {
            ж<ast.Ident> name = default!;                  // first name, or nil
            if (len((~f).Names) > 0) {
                name = (~f).Names[0];
            }
            if (i > 0) {
                // don't do a line break (min == 0) if we are printing a list of types
                // TODO(gri) this doesn't work quite right if the list of types is
                //           spread across multiple lines
                nint min = 1;
                if (prev != nil && name == prev) {
                    min = 0;
                }
                p.linebreak(p.lineFor(f.Pos()), min, ignore, p.linesFrom(line) > 0);
            }
            p.setComment((~f).Doc);
            p.recordLine(Ꮡline);
            if (name != nil){
                // method
                p.expr(~name);
                p.signature((~f).Type._<ж<ast.FuncType>>());
                // don't print "func"
                prev = default!;
            } else {
                // embedded interface
                p.expr((~f).Type);
                prev = default!;
            }
            p.setComment((~f).Comment);
        }
        if (isIncomplete) {
            if (len(list) > 0) {
                p.print(formfeed);
            }
            p.flush(p.posFor(rbrace), token.RBRACE);
            // make sure we don't lose the last line comment
            p.setLineComment("// contains filtered or unexported methods"u8);
        }
    }
    p.print(unindent, formfeed);
    p.setPos(rbrace);
    p.print(token.RBRACE);
}

// ----------------------------------------------------------------------------
// Expressions
internal static (bool has4, bool has5, nint maxProblem) walkBinary(ж<ast.BinaryExpr> Ꮡe) {
    bool has4 = default!;
    bool has5 = default!;
    nint maxProblem = default!;

    ref var e = ref Ꮡe.val;
    switch (e.Op.Precedence()) {
    case 4: {
        has4 = true;
        break;
    }
    case 5: {
        has5 = true;
        break;
    }}

    switch (e.X.type()) {
    case ж<ast.BinaryExpr> l: {
        if ((~l).Op.Precedence() < e.Op.Precedence()) {
            // parens will be inserted.
            // pretend this is an *ast.ParenExpr and do nothing.
            break;
        }
        var (h4, h5, mp) = walkBinary(l);
        has4 = has4 || h4;
        has5 = has5 || h5;
        maxProblem = max(maxProblem, mp);
        break;
    }}
    switch (e.Y.type()) {
    case ж<ast.BinaryExpr> r: {
        if ((~r).Op.Precedence() <= e.Op.Precedence()) {
            // parens will be inserted.
            // pretend this is an *ast.ParenExpr and do nothing.
            break;
        }
        var (h4, h5, mp) = walkBinary(r);
        has4 = has4 || h4;
        has5 = has5 || h5;
        maxProblem = max(maxProblem, mp);
        break;
    }
    case ж<ast.StarExpr> r: {
        if (e.Op == token.QUO) {
            // `*/`
            maxProblem = 5;
        }
        break;
    }
    case ж<ast.UnaryExpr> r: {
        var exprᴛ1 = e.Op.String() + (~r).Op.String();
        if (exprᴛ1 == "/*"u8 || exprᴛ1 == "&&"u8 || exprᴛ1 == "&^"u8) {
            maxProblem = 5;
        }
        else if (exprᴛ1 == "++"u8 || exprᴛ1 == "--"u8) {
            maxProblem = max(maxProblem, 4);
        }

        break;
    }}
    return (has4, has5, maxProblem);
}

internal static nint cutoff(ж<ast.BinaryExpr> Ꮡe, nint depth) {
    ref var e = ref Ꮡe.val;

    var (has4, has5, maxProblem) = walkBinary(Ꮡe);
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

internal static nint diffPrec(ast.Expr expr, nint prec) {
    var (x, ok) = expr._<ж<ast.BinaryExpr>>(ᐧ);
    if (!ok || prec != (~x).Op.Precedence()) {
        return 1;
    }
    return 0;
}

internal static nint reduceDepth(nint depth) {
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
//
//	5             *  /  %  <<  >>  &  &^
//	4             +  -  |  ^
//	3             ==  !=  <  <=  >  >=
//	2             &&
//	1             ||
//
// The only decision is whether there will be spaces around levels 4 and 5.
// There are never spaces at level 6 (unary), and always spaces at levels 3 and below.
//
// To choose the cutoff, look at the whole expression but excluding primary
// expressions (function calls, parenthesized exprs), and apply these rules:
//
//  1. If there is a binary operator with a right side unary operand
//     that would clash without a space, the cutoff must be (in order):
//
//     /*	6
//     &&	6
//     &^	6
//     ++	5
//     --	5
//
//     (Comparison operators always have spaces around them.)
//
//  2. If there is a mix of level 5 and level 4 operators, then the cutoff
//     is 5 (use spaces to distinguish precedence) in Normal mode
//     and 4 (never use spaces) in Compact mode.
//
//  3. If there are no level 4 operators or no level 5 operators, then the
//     cutoff is 6 (always use spaces) in Normal mode
//     and 4 (never use spaces) in Compact mode.
[GoRecv] internal static void binaryExpr(this ref printer p, ж<ast.BinaryExpr> Ꮡx, nint prec1, nint cutoff, nint depth) {
    ref var x = ref Ꮡx.val;

    nint prec = x.Op.Precedence();
    if (prec < prec1) {
        // parenthesis needed
        // Note: The parser inserts an ast.ParenExpr node; thus this case
        //       can only occur if the AST is created in a different way.
        p.print(token.LPAREN);
        p.expr0(~x, reduceDepth(depth));
        // parentheses undo one level of depth
        p.print(token.RPAREN);
        return;
    }
    var printBlank = prec < cutoff;
    var ws = indent;
    p.expr1(x.X, prec, depth + diffPrec(x.X, prec));
    if (printBlank) {
        p.print(blank);
    }
    nint xline = p.pos.Line;
    // before the operator (it may be on the next line!)
    nint yline = p.lineFor(x.Y.Pos());
    p.setPos(x.OpPos);
    p.print(x.Op);
    if (xline != yline && xline > 0 && yline > 0) {
        // at least one line break, but respect an extra empty line
        // in the source
        if (p.linebreak(yline, 1, ws, true) > 0) {
            ws = ignore;
            printBlank = false;
        }
    }
    // no blank after line break
    if (printBlank) {
        p.print(blank);
    }
    p.expr1(x.Y, prec + 1, depth + 1);
    if (ws == ignore) {
        p.print(unindent);
    }
}

internal static bool isBinary(ast.Expr expr) {
    var (_, ok) = expr._<ж<ast.BinaryExpr>>(ᐧ);
    return ok;
}

[GoRecv] internal static void expr1(this ref printer p, ast.Expr expr, nint prec1, nint depth) {
    p.setPos(expr.Pos());
    switch (expr.type()) {
    case ж<ast.BadExpr> x: {
        p.print("BadExpr");
        break;
    }
    case ж<ast.Ident> x: {
        p.print(x);
        break;
    }
    case ж<ast.BinaryExpr> x: {
        if (depth < 1) {
            p.internalError("depth < 1:", depth);
            depth = 1;
        }
        p.binaryExpr(x, prec1, cutoff(x, depth), depth);
        break;
    }
    case ж<ast.KeyValueExpr> x: {
        p.expr((~x).Key);
        p.setPos((~x).Colon);
        p.print(token.COLON, blank);
        p.expr((~x).Value);
        break;
    }
    case ж<ast.StarExpr> x: {
        static readonly UntypedInt prec = /* token.UnaryPrec */ 6;
        if (prec < prec1){
            // parenthesis needed
            p.print(token.LPAREN);
            p.print(token.MUL);
            p.expr((~x).X);
            p.print(token.RPAREN);
        } else {
            // no parenthesis needed
            p.print(token.MUL);
            p.expr((~x).X);
        }
        break;
    }
    case ж<ast.UnaryExpr> x: {
        static readonly UntypedInt prec = /* token.UnaryPrec */ 6;
        if (prec < prec1){
            // parenthesis needed
            p.print(token.LPAREN);
            p.expr(~x);
            p.print(token.RPAREN);
        } else {
            // no parenthesis needed
            p.print((~x).Op);
            if ((~x).Op == token.RANGE) {
                // TODO(gri) Remove this code if it cannot be reached.
                p.print(blank);
            }
            p.expr1((~x).X, prec, depth);
        }
        break;
    }
    case ж<ast.BasicLit> x: {
        if ((Mode)(p.Config.Mode & normalizeNumbers) != 0) {
            var x = normalizedNumber(x);
        }
        p.print(x);
        break;
    }
    case ж<ast.FuncLit> x: {
        p.setPos((~x).Type.Pos());
        p.print(token.FUNC);
        nint startCol = p.@out.Column - len("func");
        p.signature((~x).Type);
        p.funcBody(p.distanceFrom((~x).Type.Pos(), // See the comment in funcDecl about how the header size is computed.
 startCol), blank, (~x).Body);
        break;
    }
    case ж<ast.ParenExpr> x: {
        {
            var (_, hasParens) = (~x).X._<ж<ast.ParenExpr>>(ᐧ); if (hasParens){
                // don't print parentheses around an already parenthesized expression
                // TODO(gri) consider making this more general and incorporate precedence levels
                p.expr0((~x).X, depth);
            } else {
                p.print(token.LPAREN);
                p.expr0((~x).X, reduceDepth(depth));
                // parentheses undo one level of depth
                p.setPos((~x).Rparen);
                p.print(token.RPAREN);
            }
        }
        break;
    }
    case ж<ast.SelectorExpr> x: {
        p.selectorExpr(x, depth, false);
        break;
    }
    case ж<ast.TypeAssertExpr> x: {
        p.expr1((~x).X, token.HighestPrec, depth);
        p.print(token.PERIOD);
        p.setPos((~x).Lparen);
        p.print(token.LPAREN);
        if ((~x).Type != default!){
            p.expr((~x).Type);
        } else {
            p.print(token.TYPE);
        }
        p.setPos((~x).Rparen);
        p.print(token.RPAREN);
        break;
    }
    case ж<ast.IndexExpr> x: {
        p.expr1((~x).X, // TODO(gri): should treat[] like parentheses and undo one level of depth
 token.HighestPrec, 1);
        p.setPos((~x).Lbrack);
        p.print(token.LBRACK);
        p.expr0((~x).Index, depth + 1);
        p.setPos((~x).Rbrack);
        p.print(token.RBRACK);
        break;
    }
    case ж<ast.IndexListExpr> x: {
        p.expr1((~x).X, // TODO(gri): as for IndexExpr, should treat [] like parentheses and undo
 // one level of depth
 token.HighestPrec, 1);
        p.setPos((~x).Lbrack);
        p.print(token.LBRACK);
        p.exprList((~x).Lbrack, (~x).Indices, depth + 1, commaTerm, (~x).Rbrack, false);
        p.setPos((~x).Rbrack);
        p.print(token.RBRACK);
        break;
    }
    case ж<ast.SliceExpr> x: {
        p.expr1((~x).X, // TODO(gri): should treat[] like parentheses and undo one level of depth
 token.HighestPrec, 1);
        p.setPos((~x).Lbrack);
        p.print(token.LBRACK);
        var indices = new ast.Expr[]{(~x).Low, (~x).High}.slice();
        if ((~x).Max != default!) {
            indices = append(indices, (~x).Max);
        }
        // determine if we need extra blanks around ':'
        bool needsBlanks = default!;
        if (depth <= 1) {
            nint indexCount = default!;
            bool hasBinaries = default!;
            foreach (var (_, x) in indices) {
                if (x != default!) {
                    indexCount++;
                    if (isBinary(x)) {
                        hasBinaries = true;
                    }
                }
            }
            if (indexCount > 1 && hasBinaries) {
                needsBlanks = true;
            }
        }
        foreach (var (i, x) in indices) {
            if (i > 0) {
                if (indices[i - 1] != default! && needsBlanks) {
                    p.print(blank);
                }
                p.print(token.COLON);
                if (x != default! && needsBlanks) {
                    p.print(blank);
                }
            }
            if (x != default!) {
                p.expr0(x, depth + 1);
            }
        }
        p.setPos((~x).Rbrack);
        p.print(token.RBRACK);
        break;
    }
    case ж<ast.CallExpr> x: {
        if (len((~x).Args) > 1) {
            depth++;
        }
        var paren = false;
        switch ((~x).Fun.type()) {
        case ж<ast.FuncType> t: {
            paren = true;
            break;
        }
        case ж<ast.ChanType> t: {
            paren = (~t).Dir == ast.RECV;
            break;
        }}
        if (paren) {
            // Conversions to literal function types or <-chan
            // types require parentheses around the type.
            p.print(token.LPAREN);
        }
        var wasIndented = p.possibleSelectorExpr((~x).Fun, token.HighestPrec, depth);
        if (paren) {
            p.print(token.RPAREN);
        }
        p.setPos((~x).Lparen);
        p.print(token.LPAREN);
        if ((~x).Ellipsis.IsValid()){
            p.exprList((~x).Lparen, (~x).Args, depth, 0, (~x).Ellipsis, false);
            p.setPos((~x).Ellipsis);
            p.print(token.ELLIPSIS);
            if ((~x).Rparen.IsValid() && p.lineFor((~x).Ellipsis) < p.lineFor((~x).Rparen)) {
                p.print(token.COMMA, formfeed);
            }
        } else {
            p.exprList((~x).Lparen, (~x).Args, depth, commaTerm, (~x).Rparen, false);
        }
        p.setPos((~x).Rparen);
        p.print(token.RPAREN);
        if (wasIndented) {
            p.print(unindent);
        }
        break;
    }
    case ж<ast.CompositeLit> x: {
        if ((~x).Type != default!) {
            // composite literal elements that are composite literals themselves may have the type omitted
            p.expr1((~x).Type, token.HighestPrec, depth);
        }
        p.level++;
        p.setPos((~x).Lbrace);
        p.print(token.LBRACE);
        p.exprList((~x).Lbrace, (~x).Elts, 1, commaTerm, (~x).Rbrace, (~x).Incomplete);
        pmode mode = noExtraLinebreak;
        if (len((~x).Elts) > 0) {
            // do not insert extra line break following a /*-style comment
            // before the closing '}' as it might break the code if there
            // is no trailing ','
            // do not insert extra blank following a /*-style comment
            // before the closing '}' unless the literal is empty
            mode |= (pmode)(noExtraBlank);
        }
        p.print(indent, // need the initial indent to print lone comments with
 // the proper level of indentation
 unindent, mode);
        p.setPos((~x).Rbrace);
        p.print(token.RBRACE, mode);
        p.level--;
        break;
    }
    case ж<ast.Ellipsis> x: {
        p.print(token.ELLIPSIS);
        if ((~x).Elt != default!) {
            p.expr((~x).Elt);
        }
        break;
    }
    case ж<ast.ArrayType> x: {
        p.print(token.LBRACK);
        if ((~x).Len != default!) {
            p.expr((~x).Len);
        }
        p.print(token.RBRACK);
        p.expr((~x).Elt);
        break;
    }
    case ж<ast.StructType> x: {
        p.print(token.STRUCT);
        p.fieldList((~x).Fields, true, (~x).Incomplete);
        break;
    }
    case ж<ast.FuncType> x: {
        p.print(token.FUNC);
        p.signature(x);
        break;
    }
    case ж<ast.InterfaceType> x: {
        p.print(token.INTERFACE);
        p.fieldList((~x).Methods, false, (~x).Incomplete);
        break;
    }
    case ж<ast.MapType> x: {
        p.print(token.MAP, token.LBRACK);
        p.expr((~x).Key);
        p.print(token.RBRACK);
        p.expr((~x).Value);
        break;
    }
    case ж<ast.ChanType> x: {
        var exprᴛ1 = (~x).Dir;
        if (exprᴛ1 == (ast.ChanDir)(ast.SEND | ast.RECV)) {
            p.print(token.CHAN);
        }
        else if (exprᴛ1 == ast.RECV) {
            p.print(token.ARROW, token.CHAN);
        }
        else if (exprᴛ1 == ast.SEND) {
            p.print(token.CHAN);
            p.setPos((~x).Arrow);
            p.print(token.ARROW);
        }

        p.print(blank);
        p.expr((~x).Value);
        break;
    }
    default: {
        var x = expr.type();
        throw panic("unreachable");
        break;
    }}
}

// x.Arrow and x.Pos() are the same

// normalizedNumber rewrites base prefixes and exponents
// of numbers to use lower-case letters (0X123 to 0x123 and 1.2E3 to 1.2e3),
// and removes leading 0's from integer imaginary literals (0765i to 765i).
// It leaves hexadecimal digits alone.
//
// normalizedNumber doesn't modify the ast.BasicLit value lit points to.
// If lit is not a number or a number in canonical format already,
// lit is returned as is. Otherwise a new ast.BasicLit is created.
internal static ж<ast.BasicLit> normalizedNumber(ж<ast.BasicLit> Ꮡlit) {
    ref var lit = ref Ꮡlit.val;

    if (lit.Kind != token.INT && lit.Kind != token.FLOAT && lit.Kind != token.IMAG) {
        return Ꮡlit;
    }
    // not a number - nothing to do
    if (len(lit.Value) < 2) {
        return Ꮡlit;
    }
    // only one digit (common case) - nothing to do
    // len(lit.Value) >= 2
    // We ignore lit.Kind because for lit.Kind == token.IMAG the literal may be an integer
    // or floating-point value, decimal or not. Instead, just consider the literal pattern.
    @string x = lit.Value;
    var exprᴛ1 = x[..2];
    { /* default: */
        {
            nint i = strings.LastIndexByte(x, // 0-prefix octal, decimal int, or float (possibly with 'i' suffix)
 (rune)'E'); if (i >= 0) {
                x = x[..(int)(i)] + "e" + x[(int)(i + 1)..];
                break;
            }
        }
        if (x[len(x) - 1] == (rune)'i' && !strings.ContainsAny(x, // remove leading 0's from integer (but not floating-point) imaginary literals
 ".e"u8)) {
            x = strings.TrimLeft(x, "0_"u8);
            if (x == "i"u8) {
                x = "0i"u8;
            }
        }
    }
    else if (exprᴛ1 == "0X"u8) {
        x = "0x" + x[2..];
        {
            nint i = strings.LastIndexByte(x, // possibly a hexadecimal float
 (rune)'P'); if (i >= 0) {
                x = x[..(int)(i)] + "p" + x[(int)(i + 1)..];
            }
        }
    }
    else if (exprᴛ1 == "0x"u8) {
        nint i = strings.LastIndexByte(x, // possibly a hexadecimal float
 (rune)'P');
        if (i == -1) {
            return Ꮡlit;
        }
        x = x[..(int)(i)] + "p" + x[(int)(i + 1)..];
    }
    else if (exprᴛ1 == "0O"u8) {
        x = "0o" + x[2..];
    }
    else if (exprᴛ1 == "0o"u8) {
        return Ꮡlit;
    }
    if (exprᴛ1 == "0B"u8) {
        x = "0b" + x[2..];
    }
    else if (exprᴛ1 == "0b"u8) {
        return Ꮡlit;
    }

    // nothing to do
    // nothing to do
    // nothing to do
    return Ꮡ(new ast.BasicLit(ValuePos: lit.ValuePos, Kind: lit.Kind, Value: x));
}

[GoRecv] internal static bool possibleSelectorExpr(this ref printer p, ast.Expr expr, nint prec1, nint depth) {
    {
        var (x, ok) = expr._<ж<ast.SelectorExpr>>(ᐧ); if (ok) {
            return p.selectorExpr(x, depth, true);
        }
    }
    p.expr1(expr, prec1, depth);
    return false;
}

// selectorExpr handles an *ast.SelectorExpr node and reports whether x spans
// multiple lines.
[GoRecv] internal static bool selectorExpr(this ref printer p, ж<ast.SelectorExpr> Ꮡx, nint depth, bool isMethod) {
    ref var x = ref Ꮡx.val;

    p.expr1(x.X, token.HighestPrec, depth);
    p.print(token.PERIOD);
    {
        nint line = p.lineFor(x.Sel.Pos()); if (p.pos.IsValid() && p.pos.Line < line) {
            p.print(indent, newline);
            p.setPos(x.Sel.Pos());
            p.print(x.Sel);
            if (!isMethod) {
                p.print(unindent);
            }
            return true;
        }
    }
    p.setPos(x.Sel.Pos());
    p.print(x.Sel);
    return false;
}

[GoRecv] internal static void expr0(this ref printer p, ast.Expr x, nint depth) {
    p.expr1(x, token.LowestPrec, depth);
}

[GoRecv] internal static void expr(this ref printer p, ast.Expr x) {
    static readonly UntypedInt depth = 1;
    p.expr1(x, token.LowestPrec, depth);
}

// ----------------------------------------------------------------------------
// Statements

// Print the statement list indented, but without a newline after the last statement.
// Extra line breaks between statements in the source are respected but at most one
// empty line is printed between statements.
[GoRecv] internal static void stmtList(this ref printer p, slice<ast.Stmt> list, nint nindent, bool nextIsRBrace) {
    if (nindent > 0) {
        p.print(indent);
    }
    ref var line = ref heap(new nint(), out var Ꮡline);
    nint i = 0;
    foreach (var (_, s) in list) {
        // ignore empty statements (was issue 3466)
        {
            var (_, isEmpty) = s._<ж<ast.EmptyStmt>>(ᐧ); if (!isEmpty) {
                // nindent == 0 only for lists of switch/select case clauses;
                // in those cases each clause is a new section
                if (len(p.output) > 0) {
                    // only print line break if we are not at the beginning of the output
                    // (i.e., we are not printing only a partial program)
                    p.linebreak(p.lineFor(s.Pos()), 1, ignore, i == 0 || nindent == 0 || p.linesFrom(line) > 0);
                }
                p.recordLine(Ꮡline);
                p.stmt(s, nextIsRBrace && i == len(list) - 1);
                // labeled statements put labels on a separate line, but here
                // we only care about the start line of the actual statement
                // without label - correct line for each label
                for (var t = s; ᐧ ; ) {
                    var (lt, _) = t._<ж<ast.LabeledStmt>>(ᐧ);
                    if (lt == nil) {
                        break;
                    }
                    line++;
                    t = lt.val.Stmt;
                }
                i++;
            }
        }
    }
    if (nindent > 0) {
        p.print(unindent);
    }
}

// block prints an *ast.BlockStmt; it always spans at least two lines.
[GoRecv] internal static void block(this ref printer p, ж<ast.BlockStmt> Ꮡb, nint nindent) {
    ref var b = ref Ꮡb.val;

    p.setPos(b.Lbrace);
    p.print(token.LBRACE);
    p.stmtList(b.List, nindent, true);
    p.linebreak(p.lineFor(b.Rbrace), 1, ignore, true);
    p.setPos(b.Rbrace);
    p.print(token.RBRACE);
}

internal static bool isTypeName(ast.Expr x) {
    switch (x.type()) {
    case ж<ast.Ident> t: {
        return true;
    }
    case ж<ast.SelectorExpr> t: {
        return isTypeName((~t).X);
    }}
    return false;
}

internal static ast.Expr stripParens(ast.Expr x) {
    {
        var (px, strip) = x._<ж<ast.ParenExpr>>(ᐧ); if (strip) {
            // parentheses must not be stripped if there are any
            // unparenthesized composite literals starting with
            // a type name
            ast.Inspect((~px).X, (ast.Node node) => {
                switch (node.type()) {
                case ж<ast.ParenExpr> x: {
                    return false;
                }
                case ж<ast.CompositeLit> x: {
                    if (isTypeName((~x).Type)) {
                        // parentheses protect enclosed composite literals
                        strip = false;
                    }
                    return false;
                }}
                // do not strip parentheses
                // in all other cases, keep inspecting
                return true;
            });
            if (strip) {
                return stripParens((~px).X);
            }
        }
    }
    return x;
}

internal static ast.Expr stripParensAlways(ast.Expr x) {
    {
        var (xΔ1, ok) = x._<ж<ast.ParenExpr>>(ᐧ); if (ok) {
            return stripParensAlways((~xΔ1).X);
        }
    }
    return x;
}

[GoRecv] internal static void controlClause(this ref printer p, bool isForStmt, ast.Stmt init, ast.Expr expr, ast.Stmt post) {
    p.print(blank);
    var needsBlank = false;
    if (init == default! && post == default!){
        // no semicolons required
        if (expr != default!) {
            p.expr(stripParens(expr));
            needsBlank = true;
        }
    } else {
        // all semicolons required
        // (they are not separators, print them explicitly)
        if (init != default!) {
            p.stmt(init, false);
        }
        p.print(token.SEMICOLON, blank);
        if (expr != default!) {
            p.expr(stripParens(expr));
            needsBlank = true;
        }
        if (isForStmt) {
            p.print(token.SEMICOLON, blank);
            needsBlank = false;
            if (post != default!) {
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
[GoRecv] internal static bool indentList(this ref printer p, slice<ast.Expr> list) {
    // Heuristic: indentList reports whether there are more than one multi-
    // line element in the list, or if there is any element that is not
    // starting on the same line as the previous one ends.
    if (len(list) >= 2) {
        nint b = p.lineFor(list[0].Pos());
        nint e = p.lineFor(list[len(list) - 1].End());
        if (0 < b && b < e) {
            // list spans multiple lines
            nint n = 0;
            // multi-line element count
            nint line = b;
            foreach (var (_, x) in list) {
                nint xb = p.lineFor(x.Pos());
                nint xe = p.lineFor(x.End());
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

[GoRecv] internal static void stmt(this ref printer p, ast.Stmt stmt, bool nextIsRBrace) {
    p.setPos(stmt.Pos());
    switch (stmt.type()) {
    case ж<ast.BadStmt> s: {
        p.print("BadStmt");
        break;
    }
    case ж<ast.DeclStmt> s: {
        p.decl((~s).Decl);
        break;
    }
    case ж<ast.EmptyStmt> s: {
        break;
    }
    case ж<ast.LabeledStmt> s: {
        p.print(unindent);
        p.expr(~(~s).Label);
        p.setPos((~s).Colon);
        p.print(token.COLON, // nothing to do
 // a "correcting" unindent immediately following a line break
 // is applied before the line break if there is no comment
 // between (see writeWhitespace)
 indent);
        {
            var (e, isEmpty) = (~s).Stmt._<ж<ast.EmptyStmt>>(ᐧ); if (isEmpty){
                if (!nextIsRBrace) {
                    p.print(newline);
                    p.setPos(e.Pos());
                    p.print(token.SEMICOLON);
                    break;
                }
            } else {
                p.linebreak(p.lineFor((~s).Stmt.Pos()), 1, ignore, true);
            }
        }
        p.stmt((~s).Stmt, nextIsRBrace);
        break;
    }
    case ж<ast.ExprStmt> s: {
        static readonly UntypedInt depth = 1;
        p.expr0((~s).X, depth);
        break;
    }
    case ж<ast.SendStmt> s: {
        static readonly UntypedInt depth = 1;
        p.expr0((~s).Chan, depth);
        p.print(blank);
        p.setPos((~s).Arrow);
        p.print(token.ARROW, blank);
        p.expr0((~s).Value, depth);
        break;
    }
    case ж<ast.IncDecStmt> s: {
        static readonly UntypedInt depth = 1;
        p.expr0((~s).X, depth + 1);
        p.setPos((~s).TokPos);
        p.print((~s).Tok);
        break;
    }
    case ж<ast.AssignStmt> s: {
        nint depth = 1;
        if (len((~s).Lhs) > 1 && len((~s).Rhs) > 1) {
            depth++;
        }
        p.exprList(s.Pos(), (~s).Lhs, depth, 0, (~s).TokPos, false);
        p.print(blank);
        p.setPos((~s).TokPos);
        p.print((~s).Tok, blank);
        p.exprList((~s).TokPos, (~s).Rhs, depth, 0, token.NoPos, false);
        break;
    }
    case ж<ast.GoStmt> s: {
        p.print(token.GO, blank);
        p.expr(~(~s).Call);
        break;
    }
    case ж<ast.DeferStmt> s: {
        p.print(token.DEFER, blank);
        p.expr(~(~s).Call);
        break;
    }
    case ж<ast.ReturnStmt> s: {
        p.print(token.RETURN);
        if ((~s).Results != default!) {
            p.print(blank);
            // Use indentList heuristic to make corner cases look
            // better (issue 1207). A more systematic approach would
            // always indent, but this would cause significant
            // reformatting of the code base and not necessarily
            // lead to more nicely formatted code in general.
            if (p.indentList((~s).Results)){
                p.print(indent);
                // Use NoPos so that a newline never goes before
                // the results (see issue #32854).
                p.exprList(token.NoPos, (~s).Results, 1, noIndent, token.NoPos, false);
                p.print(unindent);
            } else {
                p.exprList(token.NoPos, (~s).Results, 1, 0, token.NoPos, false);
            }
        }
        break;
    }
    case ж<ast.BranchStmt> s: {
        p.print((~s).Tok);
        if ((~s).Label != nil) {
            p.print(blank);
            p.expr(~(~s).Label);
        }
        break;
    }
    case ж<ast.BlockStmt> s: {
        p.block(s, 1);
        break;
    }
    case ж<ast.IfStmt> s: {
        p.print(token.IF);
        p.controlClause(false, (~s).Init, (~s).Cond, default!);
        p.block((~s).Body, 1);
        if ((~s).Else != default!) {
            p.print(blank, token.ELSE, blank);
            switch ((~s).Else.type()) {
            case ж<ast.BlockStmt> : {
                p.stmt((~s).Else, nextIsRBrace);
                break;
            }
            case ж<ast.IfStmt> : {
                p.stmt((~s).Else, nextIsRBrace);
                break;
            }
            default: {

                p.print(token.LBRACE, // This can only happen with an incorrectly
 // constructed AST. Permit it but print so
 // that it can be parsed without errors.
 indent, formfeed);
                p.stmt((~s).Else, true);
                p.print(unindent, formfeed, token.RBRACE);
                break;
            }}

        }
        break;
    }
    case ж<ast.CaseClause> s: {
        if ((~s).List != default!){
            p.print(token.CASE, blank);
            p.exprList(s.Pos(), (~s).List, 1, 0, (~s).Colon, false);
        } else {
            p.print(token.DEFAULT);
        }
        p.setPos((~s).Colon);
        p.print(token.COLON);
        p.stmtList((~s).Body, 1, nextIsRBrace);
        break;
    }
    case ж<ast.SwitchStmt> s: {
        p.print(token.SWITCH);
        p.controlClause(false, (~s).Init, (~s).Tag, default!);
        p.block((~s).Body, 0);
        break;
    }
    case ж<ast.TypeSwitchStmt> s: {
        p.print(token.SWITCH);
        if ((~s).Init != default!) {
            p.print(blank);
            p.stmt((~s).Init, false);
            p.print(token.SEMICOLON);
        }
        p.print(blank);
        p.stmt((~s).Assign, false);
        p.print(blank);
        p.block((~s).Body, 0);
        break;
    }
    case ж<ast.CommClause> s: {
        if ((~s).Comm != default!){
            p.print(token.CASE, blank);
            p.stmt((~s).Comm, false);
        } else {
            p.print(token.DEFAULT);
        }
        p.setPos((~s).Colon);
        p.print(token.COLON);
        p.stmtList((~s).Body, 1, nextIsRBrace);
        break;
    }
    case ж<ast.SelectStmt> s: {
        p.print(token.SELECT, blank);
        var body = s.val.Body;
        if (len((~body).List) == 0 && !p.commentBefore(p.posFor((~body).Rbrace))){
            // print empty select statement w/o comments on one line
            p.setPos((~body).Lbrace);
            p.print(token.LBRACE);
            p.setPos((~body).Rbrace);
            p.print(token.RBRACE);
        } else {
            p.block(body, 0);
        }
        break;
    }
    case ж<ast.ForStmt> s: {
        p.print(token.FOR);
        p.controlClause(true, (~s).Init, (~s).Cond, (~s).Post);
        p.block((~s).Body, 1);
        break;
    }
    case ж<ast.RangeStmt> s: {
        p.print(token.FOR, blank);
        if ((~s).Key != default!) {
            p.expr((~s).Key);
            if ((~s).Value != default!) {
                // use position of value following the comma as
                // comma position for correct comment placement
                p.setPos((~s).Value.Pos());
                p.print(token.COMMA, blank);
                p.expr((~s).Value);
            }
            p.print(blank);
            p.setPos((~s).TokPos);
            p.print((~s).Tok, blank);
        }
        p.print(token.RANGE, blank);
        p.expr(stripParens((~s).X));
        p.print(blank);
        p.block((~s).Body, 1);
        break;
    }
    default: {
        var s = stmt.type();
        throw panic("unreachable");
        break;
    }}
}

// ----------------------------------------------------------------------------
// Declarations

// The keepTypeColumn function determines if the type column of a series of
// consecutive const or var declarations must be kept, or if initialization
// values (V) can be placed in the type column (T) instead. The i'th entry
// in the result slice is true if the type column in spec[i] must be kept.
//
// For example, the declaration:
//
//		const (
//			foobar int = 42 // comment
//			x          = 7  // comment
//			foo
//	             bar = 991
//		)
//
// leads to the type/values matrix below. A run of value columns (V) can
// be moved into the type column if there is no type for any of the values
// in that column (we only move entire columns so that they align properly).
//
//		matrix        formatted     result
//	                   matrix
//		T  V    ->    T  V     ->   true      there is a T and so the type
//		-  V          -  V          true      column must be kept
//		-  -          -  -          false
//		-  V          V  -          false     V is moved into T column
internal static slice<bool> keepTypeColumn(slice<ast.Spec> specs) {
    var m = new slice<bool>(len(specs));
    var populate = 
    var mʗ1 = m;
    (nint i, nint j, bool keepType) => {
        if (keepTypeΔ1) {
            for (; i < j; i++) {
                mʗ1[i] = true;
            }
        }
    };
    nint i0 = -1;
    // if i0 >= 0 we are in a run and i0 is the start of the run
    bool keepType = default!;
    foreach (var (i, s) in specs) {
        var t = s._<ж<ast.ValueSpec>>();
        if ((~t).Values != default!){
            if (i0 < 0) {
                // start of a run of ValueSpecs with non-nil Values
                i0 = i;
                keepType = false;
            }
        } else {
            if (i0 >= 0) {
                // end of a run
                populate(i0, i, keepType);
                i0 = -1;
            }
        }
        if ((~t).Type != default!) {
            keepType = true;
        }
    }
    if (i0 >= 0) {
        // end of a run
        populate(i0, len(specs), keepType);
    }
    return m;
}

[GoRecv] internal static void valueSpec(this ref printer p, ж<ast.ValueSpec> Ꮡs, bool keepType) {
    ref var s = ref Ꮡs.val;

    p.setComment(s.Doc);
    p.identList(s.Names, false);
    // always present
    nint extraTabs = 3;
    if (s.Type != default! || keepType) {
        p.print(vtab);
        extraTabs--;
    }
    if (s.Type != default!) {
        p.expr(s.Type);
    }
    if (s.Values != default!) {
        p.print(vtab, token.ASSIGN, blank);
        p.exprList(token.NoPos, s.Values, 1, 0, token.NoPos, false);
        extraTabs--;
    }
    if (s.Comment != nil) {
        for (; extraTabs > 0; extraTabs--) {
            p.print(vtab);
        }
        p.setComment(s.Comment);
    }
}

internal static ж<ast.BasicLit> sanitizeImportPath(ж<ast.BasicLit> Ꮡlit) {
    ref var lit = ref Ꮡlit.val;

    // Note: An unmodified AST generated by go/parser will already
    // contain a backward- or double-quoted path string that does
    // not contain any invalid characters, and most of the work
    // here is not needed. However, a modified or generated AST
    // may possibly contain non-canonical paths. Do the work in
    // all cases since it's not too hard and not speed-critical.
    // if we don't have a proper string, be conservative and return whatever we have
    if (lit.Kind != token.STRING) {
        return Ꮡlit;
    }
    (s, err) = strconv.Unquote(lit.Value);
    if (err != default!) {
        return Ꮡlit;
    }
    // if the string is an invalid path, return whatever we have
    //
    // spec: "Implementation restriction: A compiler may restrict
    // ImportPaths to non-empty strings using only characters belonging
    // to Unicode's L, M, N, P, and S general categories (the Graphic
    // characters without spaces) and may also exclude the characters
    // !"#$%&'()*,:;<=>?[\]^`{|} and the Unicode replacement character
    // U+FFFD."
    if (s == ""u8) {
        return Ꮡlit;
    }
    @string illegalChars = "!\"#$%&'()*,:;<=>?[\\]^{|}`�";
    foreach (var (_, r) in s) {
        if (!unicode.IsGraphic(r) || unicode.IsSpace(r) || strings.ContainsRune(illegalChars, r)) {
            return Ꮡlit;
        }
    }
    // otherwise, return the double-quoted path
    s = strconv.Quote(s);
    if (s == lit.Value) {
        return Ꮡlit;
    }
    // nothing wrong with lit
    return Ꮡ(new ast.BasicLit(ValuePos: lit.ValuePos, Kind: token.STRING, Value: s));
}

// The parameter n is the number of specs in the group. If doIndent is set,
// multi-line identifier lists in the spec are indented when the first
// linebreak is encountered.
[GoRecv] internal static void spec(this ref printer p, ast.Spec spec, nint n, bool doIndent) {
    switch (spec.type()) {
    case ж<ast.ImportSpec> s: {
        p.setComment((~s).Doc);
        if ((~s).Name != nil) {
            p.expr(~(~s).Name);
            p.print(blank);
        }
        p.expr(~sanitizeImportPath((~s).Path));
        p.setComment((~s).Comment);
        p.setPos((~s).EndPos);
        break;
    }
    case ж<ast.ValueSpec> s: {
        if (n != 1) {
            p.internalError("expected n = 1; got", n);
        }
        p.setComment((~s).Doc);
        p.identList((~s).Names, doIndent);
        if ((~s).Type != default!) {
            // always present
            p.print(blank);
            p.expr((~s).Type);
        }
        if ((~s).Values != default!) {
            p.print(blank, token.ASSIGN, blank);
            p.exprList(token.NoPos, (~s).Values, 1, 0, token.NoPos, false);
        }
        p.setComment((~s).Comment);
        break;
    }
    case ж<ast.TypeSpec> s: {
        p.setComment((~s).Doc);
        p.expr(~(~s).Name);
        if ((~s).TypeParams != nil) {
            p.parameters((~s).TypeParams, typeTParam);
        }
        if (n == 1){
            p.print(blank);
        } else {
            p.print(vtab);
        }
        if ((~s).Assign.IsValid()) {
            p.print(token.ASSIGN, blank);
        }
        p.expr((~s).Type);
        p.setComment((~s).Comment);
        break;
    }
    default: {
        var s = spec.type();
        throw panic("unreachable");
        break;
    }}
}

[GoRecv] internal static void genDecl(this ref printer p, ж<ast.GenDecl> Ꮡd) {
    ref var d = ref Ꮡd.val;

    p.setComment(d.Doc);
    p.setPos(d.Pos());
    p.print(d.Tok, blank);
    if (d.Lparen.IsValid() || len(d.Specs) != 1){
        // group of parenthesized declarations
        p.setPos(d.Lparen);
        p.print(token.LPAREN);
        {
            nint n = len(d.Specs); if (n > 0) {
                p.print(indent, formfeed);
                if (n > 1 && (d.Tok == token.CONST || d.Tok == token.VAR)){
                    // two or more grouped const/var declarations:
                    // determine if the type column must be kept
                    var keepType = keepTypeColumn(d.Specs);
                    ref var lineΔ1 = ref heap(new nint(), out var ᏑlineΔ1);
                    foreach (var (i, s) in d.Specs) {
                        if (i > 0) {
                            p.linebreak(p.lineFor(s.Pos()), 1, ignore, p.linesFrom(lineΔ1) > 0);
                        }
                        p.recordLine(ᏑlineΔ1);
                        p.valueSpec(s._<ж<ast.ValueSpec>>(), keepType[i]);
                    }
                } else {
                    ref var line = ref heap(new nint(), out var Ꮡline);
                    foreach (var (i, s) in d.Specs) {
                        if (i > 0) {
                            p.linebreak(p.lineFor(s.Pos()), 1, ignore, p.linesFrom(line) > 0);
                        }
                        p.recordLine(Ꮡline);
                        p.spec(s, n, false);
                    }
                }
                p.print(unindent, formfeed);
            }
        }
        p.setPos(d.Rparen);
        p.print(token.RPAREN);
    } else 
    if (len(d.Specs) > 0) {
        // single declaration
        p.spec(d.Specs[0], 1, true);
    }
}

// sizeCounter is an io.Writer which counts the number of bytes written,
// as well as whether a newline character was seen.
[GoType] partial struct sizeCounter {
    internal bool hasNewline;
    internal nint size;
}

[GoRecv] internal static (nint, error) Write(this ref sizeCounter c, slice<byte> p) {
    if (!c.hasNewline) {
        foreach (var (_, b) in p) {
            if (b == (rune)'\n' || b == (rune)'\f') {
                c.hasNewline = true;
                break;
            }
        }
    }
    c.size += len(p);
    return (len(p), default!);
}

// nodeSize determines the size of n in chars after formatting.
// The result is <= maxSize if the node fits on one line with at
// most maxSize chars and the formatted output doesn't contain
// any control chars. Otherwise, the result is > maxSize.
[GoRecv] internal static nint /*size*/ nodeSize(this ref printer p, ast.Node n, nint maxSize) {
    nint size = default!;

    // nodeSize invokes the printer, which may invoke nodeSize
    // recursively. For deep composite literal nests, this can
    // lead to an exponential algorithm. Remember previous
    // results to prune the recursion (was issue 1628).
    {
        nint sizeΔ1 = p.nodeSizes[n];
        var found = p.nodeSizes[n]; if (found) {
            return sizeΔ1;
        }
    }
    size = maxSize + 1;
    // assume n doesn't fit
    p.nodeSizes[n] = size;
    // nodeSize computation must be independent of particular
    // style so that we always get the same decision; print
    // in RawFormat
    var cfg = new Config(Mode: RawFormat);
    ref var counter = ref heap(new sizeCounter(), out var Ꮡcounter);
    {
        var err = cfg.fprint(~Ꮡcounter, p.fset, n, p.nodeSizes); if (err != default!) {
            return size;
        }
    }
    if (counter.size <= maxSize && !counter.hasNewline) {
        // n fits in a single line
        size = counter.size;
        p.nodeSizes[n] = size;
    }
    return size;
}

// numLines returns the number of lines spanned by node n in the original source.
[GoRecv] internal static nint numLines(this ref printer p, ast.Node n) {
    {
        tokenꓸPos from = n.Pos(); if (from.IsValid()) {
            {
                tokenꓸPos to = n.End(); if (to.IsValid()) {
                    return p.lineFor(to) - p.lineFor(from) + 1;
                }
            }
        }
    }
    return infinity;
}

// bodySize is like nodeSize but it is specialized for *ast.BlockStmt's.
[GoRecv] internal static nint bodySize(this ref printer p, ж<ast.BlockStmt> Ꮡb, nint maxSize) {
    ref var b = ref Ꮡb.val;

    tokenꓸPos pos1 = b.Pos();
    tokenꓸPos pos2 = b.Rbrace;
    if (pos1.IsValid() && pos2.IsValid() && p.lineFor(pos1) != p.lineFor(pos2)) {
        // opening and closing brace are on different lines - don't make it a one-liner
        return maxSize + 1;
    }
    if (len(b.List) > 5) {
        // too many statements - don't make it a one-liner
        return maxSize + 1;
    }
    // otherwise, estimate body size
    nint bodySize = p.commentSizeBefore(p.posFor(pos2));
    foreach (var (i, s) in b.List) {
        if (bodySize > maxSize) {
            break;
        }
        // no need to continue
        if (i > 0) {
            bodySize += 2;
        }
        // space for a semicolon and blank
        bodySize += p.nodeSize(s, maxSize);
    }
    return bodySize;
}

// funcBody prints a function body following a function header of given headerSize.
// If the header's and block's size are "small enough" and the block is "simple enough",
// the block is printed on the current line, without line breaks, spaced from the header
// by sep. Otherwise the block's opening "{" is printed on the current line, followed by
// lines for the block's statements and its closing "}".
[GoRecv] internal static void funcBody(this ref printer p, nint headerSize, whiteSpace sep, ж<ast.BlockStmt> Ꮡb) => func((defer, _) => {
    ref var b = ref Ꮡb.val;

    if (b == nil) {
        return;
    }
    // save/restore composite literal nesting level
    deferǃ((nint level) => {
        p.level = level;
    }, p.level, defer);
    p.level = 0;
    static readonly UntypedInt maxSize = 100;
    if (headerSize + p.bodySize(Ꮡb, maxSize) <= maxSize) {
        p.print(sep);
        p.setPos(b.Lbrace);
        p.print(token.LBRACE);
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
        p.print(noExtraLinebreak);
        p.setPos(b.Rbrace);
        p.print(token.RBRACE, noExtraLinebreak);
        return;
    }
    if (sep != ignore) {
        p.print(blank);
    }
    // always use blank
    p.block(Ꮡb, 1);
});

// distanceFrom returns the column difference between p.out (the current output
// position) and startOutCol. If the start position is on a different line from
// the current position (or either is unknown), the result is infinity.
[GoRecv] internal static nint distanceFrom(this ref printer p, tokenꓸPos startPos, nint startOutCol) {
    if (startPos.IsValid() && p.pos.IsValid() && p.posFor(startPos).Line == p.pos.Line) {
        return p.@out.Column - startOutCol;
    }
    return infinity;
}

[GoRecv] internal static void funcDecl(this ref printer p, ж<ast.FuncDecl> Ꮡd) {
    ref var d = ref Ꮡd.val;

    p.setComment(d.Doc);
    p.setPos(d.Pos());
    p.print(token.FUNC, blank);
    // We have to save startCol only after emitting FUNC; otherwise it can be on a
    // different line (all whitespace preceding the FUNC is emitted only when the
    // FUNC is emitted).
    nint startCol = p.@out.Column - len("func ");
    if (d.Recv != nil) {
        p.parameters(d.Recv, funcParam);
        // method: print receiver
        p.print(blank);
    }
    p.expr(~d.Name);
    p.signature(d.Type);
    p.funcBody(p.distanceFrom(d.Pos(), startCol), vtab, d.Body);
}

[GoRecv] internal static void decl(this ref printer p, ast.Decl decl) {
    switch (decl.type()) {
    case ж<ast.BadDecl> d: {
        p.setPos(d.Pos());
        p.print("BadDecl");
        break;
    }
    case ж<ast.GenDecl> d: {
        p.genDecl(d);
        break;
    }
    case ж<ast.FuncDecl> d: {
        p.funcDecl(d);
        break;
    }
    default: {
        var d = decl.type();
        throw panic("unreachable");
        break;
    }}
}

// ----------------------------------------------------------------------------
// Files
internal static token.Token /*tok*/ declToken(ast.Decl decl) {
    token.Token tok = default!;

    tok = token.ILLEGAL;
    switch (decl.type()) {
    case ж<ast.GenDecl> d: {
        tok = d.val.Tok;
        break;
    }
    case ж<ast.FuncDecl> d: {
        tok = token.FUNC;
        break;
    }}
    return tok;
}

[GoRecv] internal static void declList(this ref printer p, slice<ast.Decl> list) {
    token.Token tok = token.ILLEGAL;
    foreach (var (_, d) in list) {
        token.Token prev = tok;
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
            if (prev != tok || getDoc(d) != nil) {
                min = 2;
            }
            // start a new section if the next declaration is a function
            // that spans multiple lines (see also issue #19544)
            p.linebreak(p.lineFor(d.Pos()), min, ignore, tok == token.FUNC && p.numLines(d) > 1);
        }
        p.decl(d);
    }
}

[GoRecv] internal static void file(this ref printer p, ж<ast.File> Ꮡsrc) {
    ref var src = ref Ꮡsrc.val;

    p.setComment(src.Doc);
    p.setPos(src.Pos());
    p.print(token.PACKAGE, blank);
    p.expr(~src.Name);
    p.declList(src.Decls);
    p.print(newline);
}

} // end printer_package
