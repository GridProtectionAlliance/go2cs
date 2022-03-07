// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package printer implements printing of AST nodes.
// package printer -- go2cs converted at 2022 March 06 22:47:09 UTC
// import "go/printer" ==> using printer = go.go.printer_package
// Original source: C:\Program Files\Go\src\go\printer\printer.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using constraint = go.go.build.constraint_package;
using token = go.go.token_package;
using io = go.io_package;
using os = go.os_package;
using strings = go.strings_package;
using tabwriter = go.text.tabwriter_package;
using unicode = go.unicode_package;
using System;


namespace go.go;

public static partial class printer_package {

private static readonly nint maxNewlines = 2; // max. number of newlines between source text
private static readonly var debug = false; // enable for debugging
private static readonly nint infinity = 1 << 30;


private partial struct whiteSpace { // : byte
}

private static readonly var ignore = whiteSpace(0);
private static readonly var blank = whiteSpace(' ');
private static readonly var vtab = whiteSpace('\v');
private static readonly var newline = whiteSpace('\n');
private static readonly var formfeed = whiteSpace('\f');
private static readonly var indent = whiteSpace('>');
private static readonly var unindent = whiteSpace('<');


// A pmode value represents the current printer mode.
private partial struct pmode { // : nint
}

private static readonly pmode noExtraBlank = 1 << (int)(iota); // disables extra blank after /*-style comment
private static readonly var noExtraLinebreak = 0; // disables extra line break after /*-style comment

private partial struct commentInfo {
    public nint cindex; // current comment index
    public ptr<ast.CommentGroup> comment; // = printer.comments[cindex]; or nil
    public nint commentOffset; // = printer.posFor(printer.comments[cindex].List[0].Pos()).Offset; or infinity
    public bool commentNewline; // true if the comment group contains newlines
}

private partial struct printer {
    public ref Config Config => ref Config_val;
    public ptr<token.FileSet> fset; // Current state
    public slice<byte> output; // raw printer result
    public nint indent; // current indentation
    public nint level; // level == 0: outside composite literal; level > 0: inside composite literal
    public pmode mode; // current printer mode
    public bool endAlignment; // if set, terminate alignment immediately
    public bool impliedSemi; // if set, a linebreak implies a semicolon
    public token.Token lastTok; // last token printed (token.ILLEGAL if it's whitespace)
    public token.Token prevOpen; // previous non-brace "open" token (, [, or token.ILLEGAL
    public slice<whiteSpace> wsbuf; // delayed white space
    public slice<nint> goBuild; // start index of all //go:build comments in output
    public slice<nint> plusBuild; // start index of all // +build comments in output

// Positions
// The out position differs from the pos position when the result
// formatting differs from the source formatting (in the amount of
// white space). If there's a difference and SourcePos is set in
// ConfigMode, //line directives are used in the output to restore
// original source positions for a reader.
    public token.Position pos; // current position in AST (source) space
    public token.Position @out; // current position in output space
    public token.Position last; // value of pos after calling writeString
    public ptr<nint> linePtr; // if set, record out.Line for the next token in *linePtr

// The list of all source comments, in order of appearance.
    public slice<ptr<ast.CommentGroup>> comments; // may be nil
    public bool useNodeComments; // if not set, ignore lead and line comments of nodes

// Information about p.comments[p.cindex]; set up by nextComment.
    public ref commentInfo commentInfo => ref commentInfo_val; // Cache of already computed node sizes.
    public map<ast.Node, nint> nodeSizes; // Cache of most recently computed line position.
    public token.Pos cachedPos;
    public nint cachedLine; // line corresponding to cachedPos
}

private static void init(this ptr<printer> _addr_p, ptr<Config> _addr_cfg, ptr<token.FileSet> _addr_fset, map<ast.Node, nint> nodeSizes) {
    ref printer p = ref _addr_p.val;
    ref Config cfg = ref _addr_cfg.val;
    ref token.FileSet fset = ref _addr_fset.val;

    p.Config = cfg;
    p.fset = fset;
    p.pos = new token.Position(Line:1,Column:1);
    p.@out = new token.Position(Line:1,Column:1);
    p.wsbuf = make_slice<whiteSpace>(0, 16); // whitespace sequences are short
    p.nodeSizes = nodeSizes;
    p.cachedPos = -1;

}

private static void internalError(this ptr<printer> _addr_p, params object[] msg) => func((_, panic, _) => {
    msg = msg.Clone();
    ref printer p = ref _addr_p.val;

    if (debug) {
        fmt.Print(p.pos.String() + ": ");
        fmt.Println(msg);
        panic("go/printer");
    }
});

// commentsHaveNewline reports whether a list of comments belonging to
// an *ast.CommentGroup contains newlines. Because the position information
// may only be partially correct, we also have to read the comment text.
private static bool commentsHaveNewline(this ptr<printer> _addr_p, slice<ptr<ast.Comment>> list) {
    ref printer p = ref _addr_p.val;
 
    // len(list) > 0
    var line = p.lineFor(list[0].Pos());
    foreach (var (i, c) in list) {
        if (i > 0 && p.lineFor(list[i].Pos()) != line) { 
            // not all comments on the same line
            return true;

        }
        {
            var t = c.Text;

            if (len(t) >= 2 && (t[1] == '/' || strings.Contains(t, "\n"))) {
                return true;
            }

        }

    }    _ = line;
    return false;

}

private static void nextComment(this ptr<printer> _addr_p) {
    ref printer p = ref _addr_p.val;

    while (p.cindex < len(p.comments)) {
        var c = p.comments[p.cindex];
        p.cindex++;
        {
            var list = c.List;

            if (len(list) > 0) {
                p.comment = c;
                p.commentOffset = p.posFor(list[0].Pos()).Offset;
                p.commentNewline = p.commentsHaveNewline(list);
                return ;
            } 
            // we should not reach here (correct ASTs don't have empty
            // ast.CommentGroup nodes), but be conservative and try again

        } 
        // we should not reach here (correct ASTs don't have empty
        // ast.CommentGroup nodes), but be conservative and try again
    } 
    // no more comments
    p.commentOffset = infinity;

}

// commentBefore reports whether the current comment group occurs
// before the next position in the source code and printing it does
// not introduce implicit semicolons.
//
private static bool commentBefore(this ptr<printer> _addr_p, token.Position next) {
    ref printer p = ref _addr_p.val;

    return p.commentOffset < next.Offset && (!p.impliedSemi || !p.commentNewline);
}

// commentSizeBefore returns the estimated size of the
// comments on the same line before the next position.
//
private static nint commentSizeBefore(this ptr<printer> _addr_p, token.Position next) => func((defer, _, _) => {
    ref printer p = ref _addr_p.val;
 
    // save/restore current p.commentInfo (p.nextComment() modifies it)
    defer(info => {
        p.commentInfo = info;
    }(p.commentInfo));

    nint size = 0;
    while (p.commentBefore(next)) {
        foreach (var (_, c) in p.comment.List) {
            size += len(c.Text);
        }        p.nextComment();
    }
    return size;

});

// recordLine records the output line number for the next non-whitespace
// token in *linePtr. It is used to compute an accurate line number for a
// formatted construct, independent of pending (not yet emitted) whitespace
// or comments.
//
private static void recordLine(this ptr<printer> _addr_p, ptr<nint> _addr_linePtr) {
    ref printer p = ref _addr_p.val;
    ref nint linePtr = ref _addr_linePtr.val;

    p.linePtr = linePtr;
}

// linesFrom returns the number of output lines between the current
// output line and the line argument, ignoring any pending (not yet
// emitted) whitespace or comments. It is used to compute an accurate
// size (in number of lines) for a formatted construct.
//
private static nint linesFrom(this ptr<printer> _addr_p, nint line) {
    ref printer p = ref _addr_p.val;

    return p.@out.Line - line;
}

private static token.Position posFor(this ptr<printer> _addr_p, token.Pos pos) {
    ref printer p = ref _addr_p.val;
 
    // not used frequently enough to cache entire token.Position
    return p.fset.PositionFor(pos, false);

}

private static nint lineFor(this ptr<printer> _addr_p, token.Pos pos) {
    ref printer p = ref _addr_p.val;

    if (pos != p.cachedPos) {
        p.cachedPos = pos;
        p.cachedLine = p.fset.PositionFor(pos, false).Line;
    }
    return p.cachedLine;

}

// writeLineDirective writes a //line directive if necessary.
private static void writeLineDirective(this ptr<printer> _addr_p, token.Position pos) {
    ref printer p = ref _addr_p.val;

    if (pos.IsValid() && (p.@out.Line != pos.Line || p.@out.Filename != pos.Filename)) {
        p.output = append(p.output, tabwriter.Escape); // protect '\n' in //line from tabwriter interpretation
        p.output = append(p.output, fmt.Sprintf("//line %s:%d\n", pos.Filename, pos.Line));
        p.output = append(p.output, tabwriter.Escape); 
        // p.out must match the //line directive
        p.@out.Filename = pos.Filename;
        p.@out.Line = pos.Line;

    }
}

// writeIndent writes indentation.
private static void writeIndent(this ptr<printer> _addr_p) {
    ref printer p = ref _addr_p.val;
 
    // use "hard" htabs - indentation columns
    // must not be discarded by the tabwriter
    var n = p.Config.Indent + p.indent; // include base indentation
    for (nint i = 0; i < n; i++) {
        p.output = append(p.output, '\t');
    } 

    // update positions
    p.pos.Offset += n;
    p.pos.Column += n;
    p.@out.Column += n;

}

// writeByte writes ch n times to p.output and updates p.pos.
// Only used to write formatting (white space) characters.
private static void writeByte(this ptr<printer> _addr_p, byte ch, nint n) {
    ref printer p = ref _addr_p.val;

    if (p.endAlignment) { 
        // Ignore any alignment control character;
        // and at the end of the line, break with
        // a formfeed to indicate termination of
        // existing columns.
        switch (ch) {
            case '\t': 

            case '\v': 
                ch = ' ';
                break;
            case '\n': 

            case '\f': 
                ch = '\f';
                p.endAlignment = false;
                break;
        }

    }
    if (p.@out.Column == 1) { 
        // no need to write line directives before white space
        p.writeIndent();

    }
    for (nint i = 0; i < n; i++) {
        p.output = append(p.output, ch);
    } 

    // update positions
    p.pos.Offset += n;
    if (ch == '\n' || ch == '\f') {
        p.pos.Line += n;
        p.@out.Line += n;
        p.pos.Column = 1;
        p.@out.Column = 1;
        return ;
    }
    p.pos.Column += n;
    p.@out.Column += n;

}

// writeString writes the string s to p.output and updates p.pos, p.out,
// and p.last. If isLit is set, s is escaped w/ tabwriter.Escape characters
// to protect s from being interpreted by the tabwriter.
//
// Note: writeString is only used to write Go tokens, literals, and
// comments, all of which must be written literally. Thus, it is correct
// to always set isLit = true. However, setting it explicitly only when
// needed (i.e., when we don't know that s contains no tabs or line breaks)
// avoids processing extra escape characters and reduces run time of the
// printer benchmark by up to 10%.
//
private static void writeString(this ptr<printer> _addr_p, token.Position pos, @string s, bool isLit) {
    ref printer p = ref _addr_p.val;

    if (p.@out.Column == 1) {
        if (p.Config.Mode & SourcePos != 0) {
            p.writeLineDirective(pos);
        }
        p.writeIndent();

    }
    if (pos.IsValid()) { 
        // update p.pos (if pos is invalid, continue with existing p.pos)
        // Note: Must do this after handling line beginnings because
        // writeIndent updates p.pos if there's indentation, but p.pos
        // is the position of s.
        p.pos = pos;

    }
    if (isLit) { 
        // Protect s such that is passes through the tabwriter
        // unchanged. Note that valid Go programs cannot contain
        // tabwriter.Escape bytes since they do not appear in legal
        // UTF-8 sequences.
        p.output = append(p.output, tabwriter.Escape);

    }
    if (debug) {
        p.output = append(p.output, fmt.Sprintf("/*%s*/", pos)); // do not update p.pos!
    }
    p.output = append(p.output, s); 

    // update positions
    nint nlines = 0;
    nint li = default; // index of last newline; valid if nlines > 0
    for (nint i = 0; i < len(s); i++) { 
        // Raw string literals may contain any character except back quote (`).
        {
            var ch = s[i];

            if (ch == '\n' || ch == '\f') { 
                // account for line break
                nlines++;
                li = i; 
                // A line break inside a literal will break whatever column
                // formatting is in place; ignore any further alignment through
                // the end of the line.
                p.endAlignment = true;

            }

        }

    }
    p.pos.Offset += len(s);
    if (nlines > 0) {
        p.pos.Line += nlines;
        p.@out.Line += nlines;
        var c = len(s) - li;
        p.pos.Column = c;
        p.@out.Column = c;
    }
    else
 {
        p.pos.Column += len(s);
        p.@out.Column += len(s);
    }
    if (isLit) {
        p.output = append(p.output, tabwriter.Escape);
    }
    p.last = p.pos;

}

// writeCommentPrefix writes the whitespace before a comment.
// If there is any pending whitespace, it consumes as much of
// it as is likely to help position the comment nicely.
// pos is the comment position, next the position of the item
// after all pending comments, prev is the previous comment in
// a group of comments (or nil), and tok is the next token.
//
private static void writeCommentPrefix(this ptr<printer> _addr_p, token.Position pos, token.Position next, ptr<ast.Comment> _addr_prev, token.Token tok) {
    ref printer p = ref _addr_p.val;
    ref ast.Comment prev = ref _addr_prev.val;

    if (len(p.output) == 0) { 
        // the comment is the first item to be printed - don't write any whitespace
        return ;

    }
    if (pos.IsValid() && pos.Filename != p.last.Filename) { 
        // comment in a different file - separate with newlines
        p.writeByte('\f', maxNewlines);
        return ;

    }
    if (pos.Line == p.last.Line && (prev == null || prev.Text[1] != '/')) { 
        // comment on the same line as last item:
        // separate with at least one separator
        var hasSep = false;
        if (prev == null) { 
            // first comment of a comment group
            nint j = 0;
            {
                var i__prev1 = i;
                var ch__prev1 = ch;

                foreach (var (__i, __ch) in p.wsbuf) {
                    i = __i;
                    ch = __ch;

                    if (ch == blank) 
                        // ignore any blanks before a comment
                        p.wsbuf[i] = ignore;
                        continue;
                    else if (ch == vtab) 
                        // respect existing tabs - important
                        // for proper formatting of commented structs
                        hasSep = true;
                        continue;
                    else if (ch == indent) 
                        // apply pending indentation
                        continue;
                                        j = i;
                    break;

                }

                i = i__prev1;
                ch = ch__prev1;
            }

            p.writeWhitespace(j);

        }
    else
        if (!hasSep) {
            var sep = byte('\t');
            if (pos.Line == next.Line) { 
                // next item is on the same line as the comment
                // (which must be a /*-style comment): separate
                // with a blank instead of a tab
                sep = ' ';

            }

            p.writeByte(sep, 1);

        }
    } { 
        // comment on a different line:
        // separate with at least one line break
        var droppedLinebreak = false;
        j = 0;
        {
            var i__prev1 = i;
            var ch__prev1 = ch;

            foreach (var (__i, __ch) in p.wsbuf) {
                i = __i;
                ch = __ch;

                if (ch == blank || ch == vtab) 
                    // ignore any horizontal whitespace before line breaks
                    p.wsbuf[i] = ignore;
                    continue;
                else if (ch == indent) 
                    // apply pending indentation
                    continue;
                else if (ch == unindent) 
                    // if this is not the last unindent, apply it
                    // as it is (likely) belonging to the last
                    // construct (e.g., a multi-line expression list)
                    // and is not part of closing a block
                    if (i + 1 < len(p.wsbuf) && p.wsbuf[i + 1] == unindent) {
                        continue;
                    } 
                    // if the next token is not a closing }, apply the unindent
                    // if it appears that the comment is aligned with the
                    // token; otherwise assume the unindent is part of a
                    // closing block and stop (this scenario appears with
                    // comments before a case label where the comments
                    // apply to the next case instead of the current one)
                    if (tok != token.RBRACE && pos.Column == next.Column) {
                        continue;
                    }

                else if (ch == newline || ch == formfeed) 
                    p.wsbuf[i] = ignore;
                    droppedLinebreak = prev == null; // record only if first comment of a group
                                j = i;
                break;

            }

            i = i__prev1;
            ch = ch__prev1;
        }

        p.writeWhitespace(j); 

        // determine number of linebreaks before the comment
        nint n = 0;
        if (pos.IsValid() && p.last.IsValid()) {
            n = pos.Line - p.last.Line;
            if (n < 0) { // should never happen
                n = 0;

            }

        }
        if (p.indent == 0 && droppedLinebreak) {
            n++;
        }
        if (n == 0 && prev != null && prev.Text[1] == '/') {
            n = 1;
        }
        if (n > 0) { 
            // use formfeeds to break columns before a comment;
            // this is analogous to using formfeeds to separate
            // individual lines of /*-style comments
            p.writeByte('\f', nlimit(n));

        }
    }
}

// Returns true if s contains only white space
// (only tabs and blanks can appear in the printer's context).
//
private static bool isBlank(@string s) {
    for (nint i = 0; i < len(s); i++) {
        if (s[i] > ' ') {
            return false;
        }
    }
    return true;

}

// commonPrefix returns the common prefix of a and b.
private static @string commonPrefix(@string a, @string b) {
    nint i = 0;
    while (i < len(a) && i < len(b) && a[i] == b[i] && (a[i] <= ' ' || a[i] == '*')) {
        i++;
    }
    return a[(int)0..(int)i];
}

// trimRight returns s with trailing whitespace removed.
private static @string trimRight(@string s) {
    return strings.TrimRightFunc(s, unicode.IsSpace);
}

// stripCommonPrefix removes a common prefix from /*-style comment lines (unless no
// comment line is indented, all but the first line have some form of space prefix).
// The prefix is computed using heuristics such that is likely that the comment
// contents are nicely laid out after re-printing each line using the printer's
// current indentation.
//
private static void stripCommonPrefix(slice<@string> lines) {
    if (len(lines) <= 1) {
        return ; // at most one line - nothing to do
    }
    @string prefix = "";
    var prefixSet = false;
    if (len(lines) > 2) {
        {
            var i__prev1 = i;
            var line__prev1 = line;

            foreach (var (__i, __line) in lines[(int)1..(int)len(lines) - 1]) {
                i = __i;
                line = __line;
                if (isBlank(line)) {
                    lines[1 + i] = ""; // range starts with lines[1]
                }
                else
 {
                    if (!prefixSet) {
                        prefix = line;
                        prefixSet = true;
                    }
                    prefix = commonPrefix(prefix, line);
                }

            }

            i = i__prev1;
            line = line__prev1;
        }
    }
    if (!prefixSet) {
        var line = lines[len(lines) - 1];
        prefix = commonPrefix(line, line);
    }
    var lineOfStars = false;
    {
        var i__prev1 = i;

        var i = strings.Index(prefix, "*");

        if (i >= 0) { 
            // Line of stars present.
            if (i > 0 && prefix[i - 1] == ' ') {
                i--; // remove trailing blank from prefix so stars remain aligned
            }

            prefix = prefix[(int)0..(int)i];
            lineOfStars = true;

        }
        else
 { 
            // No line of stars present.
            // Determine the white space on the first line after the /*
            // and before the beginning of the comment text, assume two
            // blanks instead of the /* unless the first character after
            // the /* is a tab. If the first comment line is empty but
            // for the opening /*, assume up to 3 blanks or a tab. This
            // whitespace may be found as suffix in the common prefix.
            var first = lines[0];
            if (isBlank(first[(int)2..])) { 
                // no comment text on the first line:
                // reduce prefix by up to 3 blanks or a tab
                // if present - this keeps comment text indented
                // relative to the /* and */'s if it was indented
                // in the first place
                i = len(prefix);
                {
                    nint n__prev1 = n;

                    for (nint n = 0; n < 3 && i > 0 && prefix[i - 1] == ' '; n++) {
                        i--;
                    }
            else


                    n = n__prev1;
                }
                if (i == len(prefix) && i > 0 && prefix[i - 1] == '\t') {
                    i--;
                }

                prefix = prefix[(int)0..(int)i];

            } { 
                // comment text on the first line
                var suffix = make_slice<byte>(len(first));
                n = 2; // start after opening /*
                while (n < len(first) && first[n] <= ' ') {
                    suffix[n] = first[n];
                    n++;
                }

                if (n > 2 && suffix[2] == '\t') { 
                    // assume the '\t' compensates for the /*
                    suffix = suffix[(int)2..(int)n];

                }
                else
 { 
                    // otherwise assume two blanks
                    (suffix[0], suffix[1]) = (' ', ' ');                    suffix = suffix[(int)0..(int)n];

                } 
                // Shorten the computed common prefix by the length of
                // suffix, if it is found as suffix of the prefix.
                prefix = strings.TrimSuffix(prefix, string(suffix));

            }

        }
        i = i__prev1;

    } 

    // Handle last line: If it only contains a closing */, align it
    // with the opening /*, otherwise align the text with the other
    // lines.
    var last = lines[len(lines) - 1];
    @string closing = "*/";
    i = strings.Index(last, closing); // i >= 0 (closing is always present)
    if (isBlank(last[(int)0..(int)i])) { 
        // last line only contains closing */
        if (lineOfStars) {
            closing = " */"; // add blank to align final star
        }
        lines[len(lines) - 1] = prefix + closing;

    }
    else
 { 
        // last line contains more comment text - assume
        // it is aligned like the other lines and include
        // in prefix computation
        prefix = commonPrefix(prefix, last);

    }
    {
        var i__prev1 = i;
        var line__prev1 = line;

        foreach (var (__i, __line) in lines) {
            i = __i;
            line = __line;
            if (i > 0 && line != "") {
                lines[i] = line[(int)len(prefix)..];
            }
        }
        i = i__prev1;
        line = line__prev1;
    }
}

private static void writeComment(this ptr<printer> _addr_p, ptr<ast.Comment> _addr_comment) => func((defer, _, _) => {
    ref printer p = ref _addr_p.val;
    ref ast.Comment comment = ref _addr_comment.val;

    var text = comment.Text;
    var pos = p.posFor(comment.Pos());

    const @string linePrefix = "//line ";

    if (strings.HasPrefix(text, linePrefix) && (!pos.IsValid() || pos.Column == 1)) { 
        // Possibly a //-style line directive.
        // Suspend indentation temporarily to keep line directive valid.
        defer(indent => {
            p.indent = indent;
        }(p.indent));
        p.indent = 0;

    }
    if (text[1] == '/') {
        if (constraint.IsGoBuild(text)) {
            p.goBuild = append(p.goBuild, len(p.output));
        }
        else if (constraint.IsPlusBuild(text)) {
            p.plusBuild = append(p.plusBuild, len(p.output));
        }
        p.writeString(pos, trimRight(text), true);
        return ;

    }
    var lines = strings.Split(text, "\n"); 

    // The comment started in the first column but is going
    // to be indented. For an idempotent result, add indentation
    // to all lines such that they look like they were indented
    // before - this will make sure the common prefix computation
    // is the same independent of how many times formatting is
    // applied (was issue 1835).
    if (pos.IsValid() && pos.Column == 1 && p.indent > 0) {
        {
            var i__prev1 = i;
            var line__prev1 = line;

            foreach (var (__i, __line) in lines[(int)1..]) {
                i = __i;
                line = __line;
                lines[1 + i] = "   " + line;
            }

            i = i__prev1;
            line = line__prev1;
        }
    }
    stripCommonPrefix(lines); 

    // write comment lines, separated by formfeed,
    // without a line break after the last line
    {
        var i__prev1 = i;
        var line__prev1 = line;

        foreach (var (__i, __line) in lines) {
            i = __i;
            line = __line;
            if (i > 0) {
                p.writeByte('\f', 1);
                pos = p.pos;
            }
            if (len(line) > 0) {
                p.writeString(pos, trimRight(line), true);
            }
        }
        i = i__prev1;
        line = line__prev1;
    }
});

// writeCommentSuffix writes a line break after a comment if indicated
// and processes any leftover indentation information. If a line break
// is needed, the kind of break (newline vs formfeed) depends on the
// pending whitespace. The writeCommentSuffix result indicates if a
// newline was written or if a formfeed was dropped from the whitespace
// buffer.
//
private static (bool, bool) writeCommentSuffix(this ptr<printer> _addr_p, bool needsLinebreak) {
    bool wroteNewline = default;
    bool droppedFF = default;
    ref printer p = ref _addr_p.val;

    foreach (var (i, ch) in p.wsbuf) {

        if (ch == blank || ch == vtab) 
            // ignore trailing whitespace
            p.wsbuf[i] = ignore;
        else if (ch == indent || ch == unindent)         else if (ch == newline || ch == formfeed) 
            // if we need a line break, keep exactly one
            // but remember if we dropped any formfeeds
            if (needsLinebreak) {
                needsLinebreak = false;
                wroteNewline = true;
            }
            else
 {
                if (ch == formfeed) {
                    droppedFF = true;
                }
                p.wsbuf[i] = ignore;
            }

            }    p.writeWhitespace(len(p.wsbuf)); 

    // make sure we have a line break
    if (needsLinebreak) {
        p.writeByte('\n', 1);
        wroteNewline = true;
    }
    return ;

}

// containsLinebreak reports whether the whitespace buffer contains any line breaks.
private static bool containsLinebreak(this ptr<printer> _addr_p) {
    ref printer p = ref _addr_p.val;

    foreach (var (_, ch) in p.wsbuf) {
        if (ch == newline || ch == formfeed) {
            return true;
        }
    }    return false;

}

// intersperseComments consumes all comments that appear before the next token
// tok and prints it together with the buffered whitespace (i.e., the whitespace
// that needs to be written before the next token). A heuristic is used to mix
// the comments and whitespace. The intersperseComments result indicates if a
// newline was written or if a formfeed was dropped from the whitespace buffer.
//
private static (bool, bool) intersperseComments(this ptr<printer> _addr_p, token.Position next, token.Token tok) {
    bool wroteNewline = default;
    bool droppedFF = default;
    ref printer p = ref _addr_p.val;

    ptr<ast.Comment> last;
    while (p.commentBefore(next)) {
        foreach (var (_, c) in p.comment.List) {
            p.writeCommentPrefix(p.posFor(c.Pos()), next, last, tok);
            p.writeComment(c);
            last = c;
        }        p.nextComment();
    }

    if (last != null) { 
        // If the last comment is a /*-style comment and the next item
        // follows on the same line but is not a comma, and not a "closing"
        // token immediately following its corresponding "opening" token,
        // add an extra separator unless explicitly disabled. Use a blank
        // as separator unless we have pending linebreaks, they are not
        // disabled, and we are outside a composite literal, in which case
        // we want a linebreak (issue 15137).
        // TODO(gri) This has become overly complicated. We should be able
        // to track whether we're inside an expression or statement and
        // use that information to decide more directly.
        var needsLinebreak = false;
        if (p.mode & noExtraBlank == 0 && last.Text[1] == '*' && p.lineFor(last.Pos()) == next.Line && tok != token.COMMA && (tok != token.RPAREN || p.prevOpen == token.LPAREN) && (tok != token.RBRACK || p.prevOpen == token.LBRACK)) {
            if (p.containsLinebreak() && p.mode & noExtraLinebreak == 0 && p.level == 0) {
                needsLinebreak = true;
            }
            else
 {
                p.writeByte(' ', 1);
            }

        }
        if (last.Text[1] == '/' || tok == token.EOF || tok == token.RBRACE && p.mode & noExtraLinebreak == 0) {
            needsLinebreak = true;
        }
        return p.writeCommentSuffix(needsLinebreak);

    }
    p.internalError("intersperseComments called without pending comments");
    return ;

}

// whiteWhitespace writes the first n whitespace entries.
private static void writeWhitespace(this ptr<printer> _addr_p, nint n) {
    ref printer p = ref _addr_p.val;
 
    // write entries
    for (nint i = 0; i < n; i++) {
        {
            var ch = p.wsbuf[i];


            if (ch == ignore)
            {
                goto __switch_break0;
            }
            if (ch == indent)
            {
                p.indent++;
                goto __switch_break0;
            }
            if (ch == unindent)
            {
                p.indent--;
                if (p.indent < 0) {
                    p.internalError("negative indentation:", p.indent);
                    p.indent = 0;
                }
                goto __switch_break0;
            }
            if (ch == newline || ch == formfeed) 
            {
                // A line break immediately followed by a "correcting"
                // unindent is swapped with the unindent - this permits
                // proper label positioning. If a comment is between
                // the line break and the label, the unindent is not
                // part of the comment whitespace prefix and the comment
                // will be positioned correctly indented.
                if (i + 1 < n && p.wsbuf[i + 1] == unindent) { 
                    // Use a formfeed to terminate the current section.
                    // Otherwise, a long label name on the next line leading
                    // to a wide column may increase the indentation column
                    // of lines before the label; effectively leading to wrong
                    // indentation.
                    (p.wsbuf[i], p.wsbuf[i + 1]) = (unindent, formfeed);                    i--; // do it again
                    continue;

                }

            }
            // default: 
                p.writeByte(byte(ch), 1);

            __switch_break0:;
        }

    } 

    // shift remaining entries down
    var l = copy(p.wsbuf, p.wsbuf[(int)n..]);
    p.wsbuf = p.wsbuf[..(int)l];

}

// ----------------------------------------------------------------------------
// Printing interface

// nlimit limits n to maxNewlines.
private static nint nlimit(nint n) {
    if (n > maxNewlines) {
        n = maxNewlines;
    }
    return n;

}

private static bool mayCombine(token.Token prev, byte next) {
    bool b = default;


    if (prev == token.INT) 
        b = next == '.'; // 1.
    else if (prev == token.ADD) 
        b = next == '+'; // ++
    else if (prev == token.SUB) 
        b = next == '-'; // --
    else if (prev == token.QUO) 
        b = next == '*'; // /*
    else if (prev == token.LSS) 
        b = next == '-' || next == '<'; // <- or <<
    else if (prev == token.AND) 
        b = next == '&' || next == '^'; // && or &^
        return ;

}

// print prints a list of "items" (roughly corresponding to syntactic
// tokens, but also including whitespace and formatting information).
// It is the only print function that should be called directly from
// any of the AST printing functions in nodes.go.
//
// Whitespace is accumulated until a non-whitespace token appears. Any
// comments that need to appear before that token are printed first,
// taking into account the amount and structure of any pending white-
// space for best comment placement. Then, any leftover whitespace is
// printed, followed by the actual token.
//
private static void print(this ptr<printer> _addr_p, params object[] args) => func((_, panic, _) => {
    args = args.Clone();
    ref printer p = ref _addr_p.val;

    foreach (var (_, arg) in args) { 
        // information about the current arg
        @string data = default;
        bool isLit = default;
        bool impliedSemi = default; // value for p.impliedSemi after this arg

        // record previous opening token, if any

        if (p.lastTok == token.ILLEGAL)         else if (p.lastTok == token.LPAREN || p.lastTok == token.LBRACK) 
            p.prevOpen = p.lastTok;
        else 
            // other tokens followed any opening token
            p.prevOpen = token.ILLEGAL;
                switch (arg.type()) {
            case pmode x:
                p.mode ^= x;
                continue;
                break;
            case whiteSpace x:
                if (x == ignore) { 
                    // don't add ignore's to the buffer; they
                    // may screw up "correcting" unindents (see
                    // LabeledStmt)
                    continue;

                }

                var i = len(p.wsbuf);
                if (i == cap(p.wsbuf)) { 
                    // Whitespace sequences are very short so this should
                    // never happen. Handle gracefully (but possibly with
                    // bad comment placement) if it does happen.
                    p.writeWhitespace(i);
                    i = 0;

                }

                p.wsbuf = p.wsbuf[(int)0..(int)i + 1];
                p.wsbuf[i] = x;
                if (x == newline || x == formfeed) { 
                    // newlines affect the current state (p.impliedSemi)
                    // and not the state after printing arg (impliedSemi)
                    // because comments can be interspersed before the arg
                    // in this case
                    p.impliedSemi = false;

                }

                p.lastTok = token.ILLEGAL;
                continue;
                break;
            case ptr<ast.Ident> x:
                data = x.Name;
                impliedSemi = true;
                p.lastTok = token.IDENT;
                break;
            case ptr<ast.BasicLit> x:
                data = x.Value;
                isLit = true;
                impliedSemi = true;
                p.lastTok = x.Kind;
                break;
            case token.Token x:
                var s = x.String();
                if (mayCombine(p.lastTok, s[0])) { 
                    // the previous and the current token must be
                    // separated by a blank otherwise they combine
                    // into a different incorrect token sequence
                    // (except for token.INT followed by a '.' this
                    // should never happen because it is taken care
                    // of via binary expression formatting)
                    if (len(p.wsbuf) != 0) {
                        p.internalError("whitespace buffer not empty");
                    }

                    p.wsbuf = p.wsbuf[(int)0..(int)1];
                    p.wsbuf[0] = ' ';

                }

                data = s; 
                // some keywords followed by a newline imply a semicolon

                if (x == token.BREAK || x == token.CONTINUE || x == token.FALLTHROUGH || x == token.RETURN || x == token.INC || x == token.DEC || x == token.RPAREN || x == token.RBRACK || x == token.RBRACE) 
                    impliedSemi = true;
                                p.lastTok = x;
                break;
            case token.Pos x:
                if (x.IsValid()) {
                    p.pos = p.posFor(x); // accurate position of next item
                }

                continue;
                break;
            case @string x:
                data = x;
                isLit = true;
                impliedSemi = true;
                p.lastTok = token.STRING;
                break;
            default:
            {
                var x = arg.type();
                fmt.Fprintf(os.Stderr, "print: unsupported argument %v (%T)\n", arg, arg);
                panic("go/printer type");
                break;
            } 
            // data != ""

        } 
        // data != ""

        var next = p.pos; // estimated/accurate position of next item
        var (wroteNewline, droppedFF) = p.flush(next, p.lastTok); 

        // intersperse extra newlines if present in the source and
        // if they don't cause extra semicolons (don't do this in
        // flush as it will cause extra newlines at the end of a file)
        if (!p.impliedSemi) {
            var n = nlimit(next.Line - p.pos.Line); 
            // don't exceed maxNewlines if we already wrote one
            if (wroteNewline && n == maxNewlines) {
                n = maxNewlines - 1;
            }

            if (n > 0) {
                var ch = byte('\n');
                if (droppedFF) {
                    ch = '\f'; // use formfeed since we dropped one before
                }

                p.writeByte(ch, n);
                impliedSemi = false;

            }

        }
        if (p.linePtr != null) {
            p.linePtr.val = p.@out.Line;
            p.linePtr = null;
        }
        p.writeString(next, data, isLit);
        p.impliedSemi = impliedSemi;

    }
});

// flush prints any pending comments and whitespace occurring textually
// before the position of the next token tok. The flush result indicates
// if a newline was written or if a formfeed was dropped from the whitespace
// buffer.
//
private static (bool, bool) flush(this ptr<printer> _addr_p, token.Position next, token.Token tok) {
    bool wroteNewline = default;
    bool droppedFF = default;
    ref printer p = ref _addr_p.val;

    if (p.commentBefore(next)) { 
        // if there are comments before the next item, intersperse them
        wroteNewline, droppedFF = p.intersperseComments(next, tok);

    }
    else
 { 
        // otherwise, write any leftover whitespace
        p.writeWhitespace(len(p.wsbuf));

    }
    return ;

}

// getNode returns the ast.CommentGroup associated with n, if any.
private static ptr<ast.CommentGroup> getDoc(ast.Node n) {
    switch (n.type()) {
        case ptr<ast.Field> n:
            return _addr_n.Doc!;
            break;
        case ptr<ast.ImportSpec> n:
            return _addr_n.Doc!;
            break;
        case ptr<ast.ValueSpec> n:
            return _addr_n.Doc!;
            break;
        case ptr<ast.TypeSpec> n:
            return _addr_n.Doc!;
            break;
        case ptr<ast.GenDecl> n:
            return _addr_n.Doc!;
            break;
        case ptr<ast.FuncDecl> n:
            return _addr_n.Doc!;
            break;
        case ptr<ast.File> n:
            return _addr_n.Doc!;
            break;
    }
    return _addr_null!;

}

private static ptr<ast.CommentGroup> getLastComment(ast.Node n) {
    switch (n.type()) {
        case ptr<ast.Field> n:
            return _addr_n.Comment!;
            break;
        case ptr<ast.ImportSpec> n:
            return _addr_n.Comment!;
            break;
        case ptr<ast.ValueSpec> n:
            return _addr_n.Comment!;
            break;
        case ptr<ast.TypeSpec> n:
            return _addr_n.Comment!;
            break;
        case ptr<ast.GenDecl> n:
            if (len(n.Specs) > 0) {
                return _addr_getLastComment(n.Specs[len(n.Specs) - 1])!;
            }
            break;
        case ptr<ast.File> n:
            if (len(n.Comments) > 0) {
                return _addr_n.Comments[len(n.Comments) - 1]!;
            }
            break;
    }
    return _addr_null!;

}

private static error printNode(this ptr<printer> _addr_p, object node) {
    ref printer p = ref _addr_p.val;
 
    // unpack *CommentedNode, if any
    slice<ptr<ast.CommentGroup>> comments = default;
    {
        ptr<CommentedNode> (cnode, ok) = node._<ptr<CommentedNode>>();

        if (ok) {
            node = cnode.Node;
            comments = cnode.Comments;
        }
    }


    if (comments != null) { 
        // commented node - restrict comment list to relevant range
        ast.Node (n, ok) = node._<ast.Node>();
        if (!ok) {
            goto unsupported;
        }
        var beg = n.Pos();
        var end = n.End(); 
        // if the node has associated documentation,
        // include that commentgroup in the range
        // (the comment list is sorted in the order
        // of the comment appearance in the source code)
        {
            var doc = getDoc(n);

            if (doc != null) {
                beg = doc.Pos();
            }

        }

        {
            var com = getLastComment(n);

            if (com != null) {
                {
                    var e = com.End();

                    if (e > end) {
                        end = e;
                    }

                }

            } 
            // token.Pos values are global offsets, we can
            // compare them directly

        } 
        // token.Pos values are global offsets, we can
        // compare them directly
        nint i = 0;
        while (i < len(comments) && comments[i].End() < beg) {
            i++;
        }
        var j = i;
        while (j < len(comments) && comments[j].Pos() < end) {
            j++;
        }
        if (i < j) {
            p.comments = comments[(int)i..(int)j];
        }
    }    {
        ast.Node n__prev2 = n;

        (n, ok) = node._<ptr<ast.File>>();


        else if (ok) { 
            // use ast.File comments, if any
            p.comments = n.Comments;

        }
        n = n__prev2;

    } 

    // if there are no comments, use node comments
    p.useNodeComments = p.comments == null; 

    // get comments ready for use
    p.nextComment();

    p.print(pmode(0)); 

    // format node
    switch (node.type()) {
        case ast.Expr n:
            p.expr(n);
            break;
        case ast.Stmt n:
            {
                ptr<ast.LabeledStmt> (_, ok) = n._<ptr<ast.LabeledStmt>>();

                if (ok) {
                    p.indent = 1;
                }

            }

            p.stmt(n, false);
            break;
        case ast.Decl n:
            p.decl(n);
            break;
        case ast.Spec n:
            p.spec(n, 1, false);
            break;
        case slice<ast.Stmt> n:
            foreach (var (_, s) in n) {
                {
                    (_, ok) = s._<ptr<ast.LabeledStmt>>();

                    if (ok) {
                        p.indent = 1;
                    }

                }

            }
            p.stmtList(n, 0, false);
            break;
        case slice<ast.Decl> n:
            p.declList(n);
            break;
        case ptr<ast.File> n:
            p.file(n);
            break;
        default:
        {
            var n = node.type();
            goto unsupported;
            break;
        }

    }

    return error.As(null!)!;

unsupported:
    return error.As(fmt.Errorf("go/printer: unsupported node type %T", node))!;

}

// ----------------------------------------------------------------------------
// Trimmer

// A trimmer is an io.Writer filter for stripping tabwriter.Escape
// characters, trailing blanks and tabs, and for converting formfeed
// and vtab characters into newlines and htabs (in case no tabwriter
// is used). Text bracketed by tabwriter.Escape characters is passed
// through unchanged.
//
private partial struct trimmer {
    public io.Writer output;
    public nint state;
    public slice<byte> space;
}

// trimmer is implemented as a state machine.
// It can be in one of the following states:
private static readonly var inSpace = iota; // inside space
private static readonly var inEscape = 0; // inside text bracketed by tabwriter.Escapes
private static readonly var inText = 1; // inside text

private static void resetSpace(this ptr<trimmer> _addr_p) {
    ref trimmer p = ref _addr_p.val;

    p.state = inSpace;
    p.space = p.space[(int)0..(int)0];
}

// Design note: It is tempting to eliminate extra blanks occurring in
//              whitespace in this function as it could simplify some
//              of the blanks logic in the node printing functions.
//              However, this would mess up any formatting done by
//              the tabwriter.

private static slice<byte> aNewline = (slice<byte>)"\n";

private static (nint, error) Write(this ptr<trimmer> _addr_p, slice<byte> data) => func((_, panic, _) => {
    nint n = default;
    error err = default!;
    ref trimmer p = ref _addr_p.val;
 
    // invariants:
    // p.state == inSpace:
    //    p.space is unwritten
    // p.state == inEscape, inText:
    //    data[m:n] is unwritten
    nint m = 0;
    byte b = default;
    foreach (var (__n, __b) in data) {
        n = __n;
        b = __b;
        if (b == '\v') {
            b = '\t'; // convert to htab
        }

        if (p.state == inSpace) 

            if (b == '\t' || b == ' ') 
                p.space = append(p.space, b);
            else if (b == '\n' || b == '\f') 
                p.resetSpace(); // discard trailing space
                _, err = p.output.Write(aNewline);
            else if (b == tabwriter.Escape) 
                _, err = p.output.Write(p.space);
                p.state = inEscape;
                m = n + 1; // +1: skip tabwriter.Escape
            else 
                _, err = p.output.Write(p.space);
                p.state = inText;
                m = n;
                    else if (p.state == inEscape) 
            if (b == tabwriter.Escape) {
                _, err = p.output.Write(data[(int)m..(int)n]);
                p.resetSpace();
            }
        else if (p.state == inText) 

            if (b == '\t' || b == ' ') 
                _, err = p.output.Write(data[(int)m..(int)n]);
                p.resetSpace();
                p.space = append(p.space, b);
            else if (b == '\n' || b == '\f') 
                _, err = p.output.Write(data[(int)m..(int)n]);
                p.resetSpace();
                if (err == null) {
                    _, err = p.output.Write(aNewline);
                }
            else if (b == tabwriter.Escape) 
                _, err = p.output.Write(data[(int)m..(int)n]);
                p.state = inEscape;
                m = n + 1; // +1: skip tabwriter.Escape
                    else 
            panic("unreachable");
                if (err != null) {
            return ;
        }
    }
    n = len(data);


    if (p.state == inEscape || p.state == inText) 
        _, err = p.output.Write(data[(int)m..(int)n]);
        p.resetSpace();
        return ;

});

// ----------------------------------------------------------------------------
// Public interface

// A Mode value is a set of flags (or 0). They control printing.
public partial struct Mode { // : nuint
}

public static readonly Mode RawFormat = 1 << (int)(iota); // do not use a tabwriter; if set, UseSpaces is ignored
public static readonly var TabIndent = 0; // use tabs for indentation independent of UseSpaces
public static readonly var UseSpaces = 1; // use spaces instead of tabs for alignment
public static readonly var SourcePos = 2; // emit //line directives to preserve original source positions

// The mode below is not included in printer's public API because
// editing code text is deemed out of scope. Because this mode is
// unexported, it's also possible to modify or remove it based on
// the evolving needs of go/format and cmd/gofmt without breaking
// users. See discussion in CL 240683.
 
// normalizeNumbers means to canonicalize number
// literal prefixes and exponents while printing.
//
// This value is known in and used by go/format and cmd/gofmt.
// It is currently more convenient and performant for those
// packages to apply number normalization during printing,
// rather than by modifying the AST in advance.
private static readonly Mode normalizeNumbers = 1 << 30;


// A Config node controls the output of Fprint.
public partial struct Config {
    public Mode Mode; // default: 0
    public nint Tabwidth; // default: 8
    public nint Indent; // default: 0 (all code is indented at least by this much)
}

// fprint implements Fprint and takes a nodesSizes map for setting up the printer state.
private static error fprint(this ptr<Config> _addr_cfg, io.Writer output, ptr<token.FileSet> _addr_fset, object node, map<ast.Node, nint> nodeSizes) {
    error err = default!;
    ref Config cfg = ref _addr_cfg.val;
    ref token.FileSet fset = ref _addr_fset.val;
 
    // print node
    printer p = default;
    p.init(cfg, fset, nodeSizes);
    err = p.printNode(node);

    if (err != null) {
        return ;
    }
    p.impliedSemi = false; // EOF acts like a newline
    p.flush(new token.Position(Offset:infinity,Line:infinity), token.EOF); 

    // output is buffered in p.output now.
    // fix //go:build and // +build comments if needed.
    p.fixGoBuildLines(); 

    // redirect output through a trimmer to eliminate trailing whitespace
    // (Input to a tabwriter must be untrimmed since trailing tabs provide
    // formatting information. The tabwriter could provide trimming
    // functionality but no tabwriter is used when RawFormat is set.)
    output = addr(new trimmer(output:output)); 

    // redirect output through a tabwriter if necessary
    if (cfg.Mode & RawFormat == 0) {
        var minwidth = cfg.Tabwidth;

        var padchar = byte('\t');
        if (cfg.Mode & UseSpaces != 0) {
            padchar = ' ';
        }
        var twmode = tabwriter.DiscardEmptyColumns;
        if (cfg.Mode & TabIndent != 0) {
            minwidth = 0;
            twmode |= tabwriter.TabIndent;
        }
        output = tabwriter.NewWriter(output, minwidth, cfg.Tabwidth, 1, padchar, twmode);

    }
    _, err = output.Write(p.output);

    if (err != null) {
        return ;
    }
    {
        ptr<tabwriter.Writer> (tw, _) = output._<ptr<tabwriter.Writer>>();

        if (tw != null) {
            err = tw.Flush();
        }
    }


    return ;

}

// A CommentedNode bundles an AST node and corresponding comments.
// It may be provided as argument to any of the Fprint functions.
//
public partial struct CommentedNode {
    public slice<ptr<ast.CommentGroup>> Comments;
}

// Fprint "pretty-prints" an AST node to output for a given configuration cfg.
// Position information is interpreted relative to the file set fset.
// The node type must be *ast.File, *CommentedNode, []ast.Decl, []ast.Stmt,
// or assignment-compatible to ast.Expr, ast.Decl, ast.Spec, or ast.Stmt.
//
private static error Fprint(this ptr<Config> _addr_cfg, io.Writer output, ptr<token.FileSet> _addr_fset, object node) {
    ref Config cfg = ref _addr_cfg.val;
    ref token.FileSet fset = ref _addr_fset.val;

    return error.As(cfg.fprint(output, fset, node, make_map<ast.Node, nint>()))!;
}

// Fprint "pretty-prints" an AST node to output.
// It calls Config.Fprint with default settings.
// Note that gofmt uses tabs for indentation but spaces for alignment;
// use format.Node (package go/format) for output that matches gofmt.
//
public static error Fprint(io.Writer output, ptr<token.FileSet> _addr_fset, object node) {
    ref token.FileSet fset = ref _addr_fset.val;

    return error.As((addr(new Config(Tabwidth:8))).Fprint(output, fset, node)!)!;
}

} // end printer_package
