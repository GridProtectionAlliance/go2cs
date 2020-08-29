// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package printer implements printing of AST nodes.
// package printer -- go2cs converted at 2020 August 29 08:52:29 UTC
// import "go/printer" ==> using printer = go.go.printer_package
// Original source: C:\Go\src\go\printer\printer.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using io = go.io_package;
using os = go.os_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using tabwriter = go.text.tabwriter_package;
using unicode = go.unicode_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class printer_package
    {
        private static readonly long maxNewlines = 2L; // max. number of newlines between source text
        private static readonly var debug = false; // enable for debugging
        private static readonly long infinity = 1L << (int)(30L);

        private partial struct whiteSpace // : byte
        {
        }

        private static readonly var ignore = whiteSpace(0L);
        private static readonly var blank = whiteSpace(' ');
        private static readonly var vtab = whiteSpace('\v');
        private static readonly var newline = whiteSpace('\n');
        private static readonly var formfeed = whiteSpace('\f');
        private static readonly var indent = whiteSpace('>');
        private static readonly var unindent = whiteSpace('<');

        // A pmode value represents the current printer mode.
        private partial struct pmode // : long
        {
        }

        private static readonly pmode noExtraBlank = 1L << (int)(iota); // disables extra blank after /*-style comment
        private static readonly var noExtraLinebreak = 0; // disables extra line break after /*-style comment

        private partial struct commentInfo
        {
            public long cindex; // current comment index
            public ptr<ast.CommentGroup> comment; // = printer.comments[cindex]; or nil
            public long commentOffset; // = printer.posFor(printer.comments[cindex].List[0].Pos()).Offset; or infinity
            public bool commentNewline; // true if the comment group contains newlines
        }

        private partial struct printer
        {
            public ref Config Config => ref Config_val;
            public ptr<token.FileSet> fset; // Current state
            public slice<byte> output; // raw printer result
            public long indent; // current indentation
            public long level; // level == 0: outside composite literal; level > 0: inside composite literal
            public pmode mode; // current printer mode
            public bool impliedSemi; // if set, a linebreak implies a semicolon
            public token.Token lastTok; // last token printed (token.ILLEGAL if it's whitespace)
            public token.Token prevOpen; // previous non-brace "open" token (, [, or token.ILLEGAL
            public slice<whiteSpace> wsbuf; // delayed white space

// Positions
// The out position differs from the pos position when the result
// formatting differs from the source formatting (in the amount of
// white space). If there's a difference and SourcePos is set in
// ConfigMode, //line directives are used in the output to restore
// original source positions for a reader.
            public token.Position pos; // current position in AST (source) space
            public token.Position @out; // current position in output space
            public token.Position last; // value of pos after calling writeString
            public ptr<long> linePtr; // if set, record out.Line for the next token in *linePtr

// The list of all source comments, in order of appearance.
            public slice<ref ast.CommentGroup> comments; // may be nil
            public bool useNodeComments; // if not set, ignore lead and line comments of nodes

// Information about p.comments[p.cindex]; set up by nextComment.
            public ref commentInfo commentInfo => ref commentInfo_val; // Cache of already computed node sizes.
            public map<ast.Node, long> nodeSizes; // Cache of most recently computed line position.
            public token.Pos cachedPos;
            public long cachedLine; // line corresponding to cachedPos
        }

        private static void init(this ref printer p, ref Config cfg, ref token.FileSet fset, map<ast.Node, long> nodeSizes)
        {
            p.Config = cfg.Value;
            p.fset = fset;
            p.pos = new token.Position(Line:1,Column:1);
            p.@out = new token.Position(Line:1,Column:1);
            p.wsbuf = make_slice<whiteSpace>(0L, 16L); // whitespace sequences are short
            p.nodeSizes = nodeSizes;
            p.cachedPos = -1L;
        }

        private static void internalError(this ref printer _p, params object[] msg) => func(_p, (ref printer p, Defer _, Panic panic, Recover __) =>
        {
            if (debug)
            {
                fmt.Print(p.pos.String() + ": ");
                fmt.Println(msg);
                panic("go/printer");
            }
        });

        // commentsHaveNewline reports whether a list of comments belonging to
        // an *ast.CommentGroup contains newlines. Because the position information
        // may only be partially correct, we also have to read the comment text.
        private static bool commentsHaveNewline(this ref printer p, slice<ref ast.Comment> list)
        { 
            // len(list) > 0
            var line = p.lineFor(list[0L].Pos());
            foreach (var (i, c) in list)
            {
                if (i > 0L && p.lineFor(list[i].Pos()) != line)
                { 
                    // not all comments on the same line
                    return true;
                }
                {
                    var t = c.Text;

                    if (len(t) >= 2L && (t[1L] == '/' || strings.Contains(t, "\n")))
                    {
                        return true;
                    }

                }
            }
            _ = line;
            return false;
        }

        private static void nextComment(this ref printer p)
        {
            while (p.cindex < len(p.comments))
            {
                var c = p.comments[p.cindex];
                p.cindex++;
                {
                    var list = c.List;

                    if (len(list) > 0L)
                    {
                        p.comment = c;
                        p.commentOffset = p.posFor(list[0L].Pos()).Offset;
                        p.commentNewline = p.commentsHaveNewline(list);
                        return;
                    } 
                    // we should not reach here (correct ASTs don't have empty
                    // ast.CommentGroup nodes), but be conservative and try again

                } 
                // we should not reach here (correct ASTs don't have empty
                // ast.CommentGroup nodes), but be conservative and try again
            } 
            // no more comments
 
            // no more comments
            p.commentOffset = infinity;
        }

        // commentBefore reports whether the current comment group occurs
        // before the next position in the source code and printing it does
        // not introduce implicit semicolons.
        //
        private static bool commentBefore(this ref printer p, token.Position next)
        {
            return p.commentOffset < next.Offset && (!p.impliedSemi || !p.commentNewline);
        }

        // commentSizeBefore returns the estimated size of the
        // comments on the same line before the next position.
        //
        private static long commentSizeBefore(this ref printer _p, token.Position next) => func(_p, (ref printer p, Defer defer, Panic _, Recover __) =>
        { 
            // save/restore current p.commentInfo (p.nextComment() modifies it)
            defer(info =>
            {
                p.commentInfo = info;
            }(p.commentInfo));

            long size = 0L;
            while (p.commentBefore(next))
            {
                foreach (var (_, c) in p.comment.List)
                {
                    size += len(c.Text);
                }
                p.nextComment();
            }

            return size;
        });

        // recordLine records the output line number for the next non-whitespace
        // token in *linePtr. It is used to compute an accurate line number for a
        // formatted construct, independent of pending (not yet emitted) whitespace
        // or comments.
        //
        private static void recordLine(this ref printer p, ref long linePtr)
        {
            p.linePtr = linePtr;
        }

        // linesFrom returns the number of output lines between the current
        // output line and the line argument, ignoring any pending (not yet
        // emitted) whitespace or comments. It is used to compute an accurate
        // size (in number of lines) for a formatted construct.
        //
        private static long linesFrom(this ref printer p, long line)
        {
            return p.@out.Line - line;
        }

        private static token.Position posFor(this ref printer p, token.Pos pos)
        { 
            // not used frequently enough to cache entire token.Position
            return p.fset.Position(pos);
        }

        private static long lineFor(this ref printer p, token.Pos pos)
        {
            if (pos != p.cachedPos)
            {
                p.cachedPos = pos;
                p.cachedLine = p.fset.Position(pos).Line;
            }
            return p.cachedLine;
        }

        // writeLineDirective writes a //line directive if necessary.
        private static void writeLineDirective(this ref printer p, token.Position pos)
        {
            if (pos.IsValid() && (p.@out.Line != pos.Line || p.@out.Filename != pos.Filename))
            {
                p.output = append(p.output, tabwriter.Escape); // protect '\n' in //line from tabwriter interpretation
                p.output = append(p.output, fmt.Sprintf("//line %s:%d\n", pos.Filename, pos.Line));
                p.output = append(p.output, tabwriter.Escape); 
                // p.out must match the //line directive
                p.@out.Filename = pos.Filename;
                p.@out.Line = pos.Line;
            }
        }

        // writeIndent writes indentation.
        private static void writeIndent(this ref printer p)
        { 
            // use "hard" htabs - indentation columns
            // must not be discarded by the tabwriter
            var n = p.Config.Indent + p.indent; // include base indentation
            for (long i = 0L; i < n; i++)
            {
                p.output = append(p.output, '\t');
            } 

            // update positions
 

            // update positions
            p.pos.Offset += n;
            p.pos.Column += n;
            p.@out.Column += n;
        }

        // writeByte writes ch n times to p.output and updates p.pos.
        // Only used to write formatting (white space) characters.
        private static void writeByte(this ref printer p, byte ch, long n)
        {
            if (p.@out.Column == 1L)
            { 
                // no need to write line directives before white space
                p.writeIndent();
            }
            for (long i = 0L; i < n; i++)
            {
                p.output = append(p.output, ch);
            } 

            // update positions
 

            // update positions
            p.pos.Offset += n;
            if (ch == '\n' || ch == '\f')
            {
                p.pos.Line += n;
                p.@out.Line += n;
                p.pos.Column = 1L;
                p.@out.Column = 1L;
                return;
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
        private static void writeString(this ref printer p, token.Position pos, @string s, bool isLit)
        {
            if (p.@out.Column == 1L)
            {
                if (p.Config.Mode & SourcePos != 0L)
                {
                    p.writeLineDirective(pos);
                }
                p.writeIndent();
            }
            if (pos.IsValid())
            { 
                // update p.pos (if pos is invalid, continue with existing p.pos)
                // Note: Must do this after handling line beginnings because
                // writeIndent updates p.pos if there's indentation, but p.pos
                // is the position of s.
                p.pos = pos;
            }
            if (isLit)
            { 
                // Protect s such that is passes through the tabwriter
                // unchanged. Note that valid Go programs cannot contain
                // tabwriter.Escape bytes since they do not appear in legal
                // UTF-8 sequences.
                p.output = append(p.output, tabwriter.Escape);
            }
            if (debug)
            {
                p.output = append(p.output, fmt.Sprintf("/*%s*/", pos)); // do not update p.pos!
            }
            p.output = append(p.output, s); 

            // update positions
            long nlines = 0L;
            long li = default; // index of last newline; valid if nlines > 0
            for (long i = 0L; i < len(s); i++)
            { 
                // Go tokens cannot contain '\f' - no need to look for it
                if (s[i] == '\n')
                {
                    nlines++;
                    li = i;
                }
            }

            p.pos.Offset += len(s);
            if (nlines > 0L)
            {
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
            if (isLit)
            {
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
        private static void writeCommentPrefix(this ref printer p, token.Position pos, token.Position next, ref ast.Comment prev, token.Token tok)
        {
            if (len(p.output) == 0L)
            { 
                // the comment is the first item to be printed - don't write any whitespace
                return;
            }
            if (pos.IsValid() && pos.Filename != p.last.Filename)
            { 
                // comment in a different file - separate with newlines
                p.writeByte('\f', maxNewlines);
                return;
            }
            if (pos.Line == p.last.Line && (prev == null || prev.Text[1L] != '/'))
            { 
                // comment on the same line as last item:
                // separate with at least one separator
                var hasSep = false;
                if (prev == null)
                { 
                    // first comment of a comment group
                    long j = 0L;
                    {
                        var i__prev1 = i;
                        var ch__prev1 = ch;

                        foreach (var (__i, __ch) in p.wsbuf)
                        {
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
 
                // make sure there is at least one separator
                if (!hasSep)
                {
                    var sep = byte('\t');
                    if (pos.Line == next.Line)
                    { 
                        // next item is on the same line as the comment
                        // (which must be a /*-style comment): separate
                        // with a blank instead of a tab
                        sep = ' ';
                    }
                    p.writeByte(sep, 1L);
                }
            }            { 
                // comment on a different line:
                // separate with at least one line break
                var droppedLinebreak = false;
                j = 0L;
                {
                    var i__prev1 = i;
                    var ch__prev1 = ch;

                    foreach (var (__i, __ch) in p.wsbuf)
                    {
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
                            if (i + 1L < len(p.wsbuf) && p.wsbuf[i + 1L] == unindent)
                            {
                                continue;
                            } 
                            // if the next token is not a closing }, apply the unindent
                            // if it appears that the comment is aligned with the
                            // token; otherwise assume the unindent is part of a
                            // closing block and stop (this scenario appears with
                            // comments before a case label where the comments
                            // apply to the next case instead of the current one)
                            if (tok != token.RBRACE && pos.Column == next.Column)
                            {
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
                long n = 0L;
                if (pos.IsValid() && p.last.IsValid())
                {
                    n = pos.Line - p.last.Line;
                    if (n < 0L)
                    { // should never happen
                        n = 0L;
                    }
                } 

                // at the package scope level only (p.indent == 0),
                // add an extra newline if we dropped one before:
                // this preserves a blank line before documentation
                // comments at the package scope level (issue 2570)
                if (p.indent == 0L && droppedLinebreak)
                {
                    n++;
                } 

                // make sure there is at least one line break
                // if the previous comment was a line comment
                if (n == 0L && prev != null && prev.Text[1L] == '/')
                {
                    n = 1L;
                }
                if (n > 0L)
                { 
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
        private static bool isBlank(@string s)
        {
            for (long i = 0L; i < len(s); i++)
            {
                if (s[i] > ' ')
                {
                    return false;
                }
            }

            return true;
        }

        // commonPrefix returns the common prefix of a and b.
        private static @string commonPrefix(@string a, @string b)
        {
            long i = 0L;
            while (i < len(a) && i < len(b) && a[i] == b[i] && (a[i] <= ' ' || a[i] == '*'))
            {
                i++;
            }

            return a[0L..i];
        }

        // trimRight returns s with trailing whitespace removed.
        private static @string trimRight(@string s)
        {
            return strings.TrimRightFunc(s, unicode.IsSpace);
        }

        // stripCommonPrefix removes a common prefix from /*-style comment lines (unless no
        // comment line is indented, all but the first line have some form of space prefix).
        // The prefix is computed using heuristics such that is likely that the comment
        // contents are nicely laid out after re-printing each line using the printer's
        // current indentation.
        //
        private static void stripCommonPrefix(slice<@string> lines)
        {
            if (len(lines) <= 1L)
            {
                return; // at most one line - nothing to do
            } 
            // len(lines) > 1

            // The heuristic in this function tries to handle a few
            // common patterns of /*-style comments: Comments where
            // the opening /* and closing */ are aligned and the
            // rest of the comment text is aligned and indented with
            // blanks or tabs, cases with a vertical "line of stars"
            // on the left, and cases where the closing */ is on the
            // same line as the last comment text.

            // Compute maximum common white prefix of all but the first,
            // last, and blank lines, and replace blank lines with empty
            // lines (the first line starts with /* and has no prefix).
            // In cases where only the first and last lines are not blank,
            // such as two-line comments, or comments where all inner lines
            // are blank, consider the last line for the prefix computation
            // since otherwise the prefix would be empty.
            //
            // Note that the first and last line are never empty (they
            // contain the opening /* and closing */ respectively) and
            // thus they can be ignored by the blank line check.
            @string prefix = "";
            var prefixSet = false;
            if (len(lines) > 2L)
            {
                {
                    var i__prev1 = i;
                    var line__prev1 = line;

                    foreach (var (__i, __line) in lines[1L..len(lines) - 1L])
                    {
                        i = __i;
                        line = __line;
                        if (isBlank(line))
                        {
                            lines[1L + i] = ""; // range starts with lines[1]
                        }
                        else
                        {
                            if (!prefixSet)
                            {
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
            // If we don't have a prefix yet, consider the last line.
            if (!prefixSet)
            {
                var line = lines[len(lines) - 1L];
                prefix = commonPrefix(line, line);
            }

            /*
                 * Check for vertical "line of stars" and correct prefix accordingly.
                 */
            var lineOfStars = false;
            {
                var i__prev1 = i;

                var i = strings.Index(prefix, "*");

                if (i >= 0L)
                { 
                    // Line of stars present.
                    if (i > 0L && prefix[i - 1L] == ' ')
                    {
                        i--; // remove trailing blank from prefix so stars remain aligned
                    }
                    prefix = prefix[0L..i];
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
                    var first = lines[0L];
                    if (isBlank(first[2L..]))
                    { 
                        // no comment text on the first line:
                        // reduce prefix by up to 3 blanks or a tab
                        // if present - this keeps comment text indented
                        // relative to the /* and */'s if it was indented
                        // in the first place
                        i = len(prefix);
                        {
                            long n__prev1 = n;

                            for (long n = 0L; n < 3L && i > 0L && prefix[i - 1L] == ' '; n++)
                            {
                                i--;
                            }
                    else


                            n = n__prev1;
                        }
                        if (i == len(prefix) && i > 0L && prefix[i - 1L] == '\t')
                        {
                            i--;
                        }
                        prefix = prefix[0L..i];
                    }                    { 
                        // comment text on the first line
                        var suffix = make_slice<byte>(len(first));
                        n = 2L; // start after opening /*
                        while (n < len(first) && first[n] <= ' ')
                        {
                            suffix[n] = first[n];
                            n++;
                        }

                        if (n > 2L && suffix[2L] == '\t')
                        { 
                            // assume the '\t' compensates for the /*
                            suffix = suffix[2L..n];
                        }
                        else
                        { 
                            // otherwise assume two blanks
                            suffix[0L] = ' ';
                            suffix[1L] = ' ';
                            suffix = suffix[0L..n];
                        } 
                        // Shorten the computed common prefix by the length of
                        // suffix, if it is found as suffix of the prefix.
                        prefix = strings.TrimSuffix(prefix, string(suffix));
                    }
                } 

                // Handle last line: If it only contains a closing */, align it
                // with the opening /*, otherwise align the text with the other
                // lines.

                i = i__prev1;

            } 

            // Handle last line: If it only contains a closing */, align it
            // with the opening /*, otherwise align the text with the other
            // lines.
            var last = lines[len(lines) - 1L];
            @string closing = "*/";
            i = strings.Index(last, closing); // i >= 0 (closing is always present)
            if (isBlank(last[0L..i]))
            { 
                // last line only contains closing */
                if (lineOfStars)
                {
                    closing = " */"; // add blank to align final star
                }
                lines[len(lines) - 1L] = prefix + closing;
            }
            else
            { 
                // last line contains more comment text - assume
                // it is aligned like the other lines and include
                // in prefix computation
                prefix = commonPrefix(prefix, last);
            } 

            // Remove the common prefix from all but the first and empty lines.
            {
                var i__prev1 = i;
                var line__prev1 = line;

                foreach (var (__i, __line) in lines)
                {
                    i = __i;
                    line = __line;
                    if (i > 0L && line != "")
                    {
                        lines[i] = line[len(prefix)..];
                    }
                }

                i = i__prev1;
                line = line__prev1;
            }

        }

        private static void writeComment(this ref printer _p, ref ast.Comment _comment) => func(_p, _comment, (ref printer p, ref ast.Comment comment, Defer defer, Panic _, Recover __) =>
        {
            var text = comment.Text;
            var pos = p.posFor(comment.Pos());

            const @string linePrefix = "//line ";

            if (strings.HasPrefix(text, linePrefix) && (!pos.IsValid() || pos.Column == 1L))
            { 
                // possibly a line directive
                var ldir = strings.TrimSpace(text[len(linePrefix)..]);
                {
                    var i__prev2 = i;

                    var i = strings.LastIndex(ldir, ":");

                    if (i >= 0L)
                    {
                        {
                            var line__prev3 = line;

                            var (line, err) = strconv.Atoi(ldir[i + 1L..]);

                            if (err == null && line > 0L)
                            { 
                                // The line directive we are about to print changed
                                // the Filename and Line number used for subsequent
                                // tokens. We have to update our AST-space position
                                // accordingly and suspend indentation temporarily.
                                var indent = p.indent;
                                p.indent = 0L;
                                defer(() =>
                                {
                                    p.pos.Filename = ldir[..i];
                                    p.pos.Line = line;
                                    p.pos.Column = 1L;
                                    p.indent = indent;
                                }());
                            }

                            line = line__prev3;

                        }
                    }

                    i = i__prev2;

                }
            } 

            // shortcut common case of //-style comments
            if (text[1L] == '/')
            {
                p.writeString(pos, trimRight(text), true);
                return;
            } 

            // for /*-style comments, print line by line and let the
            // write function take care of the proper indentation
            var lines = strings.Split(text, "\n"); 

            // The comment started in the first column but is going
            // to be indented. For an idempotent result, add indentation
            // to all lines such that they look like they were indented
            // before - this will make sure the common prefix computation
            // is the same independent of how many times formatting is
            // applied (was issue 1835).
            if (pos.IsValid() && pos.Column == 1L && p.indent > 0L)
            {
                {
                    var i__prev1 = i;
                    var line__prev1 = line;

                    foreach (var (__i, __line) in lines[1L..])
                    {
                        i = __i;
                        line = __line;
                        lines[1L + i] = "   " + line;
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

                foreach (var (__i, __line) in lines)
                {
                    i = __i;
                    line = __line;
                    if (i > 0L)
                    {
                        p.writeByte('\f', 1L);
                        pos = p.pos;
                    }
                    if (len(line) > 0L)
                    {
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
        private static (bool, bool) writeCommentSuffix(this ref printer p, bool needsLinebreak)
        {
            foreach (var (i, ch) in p.wsbuf)
            {

                if (ch == blank || ch == vtab) 
                    // ignore trailing whitespace
                    p.wsbuf[i] = ignore;
                else if (ch == indent || ch == unindent)                 else if (ch == newline || ch == formfeed) 
                    // if we need a line break, keep exactly one
                    // but remember if we dropped any formfeeds
                    if (needsLinebreak)
                    {
                        needsLinebreak = false;
                        wroteNewline = true;
                    }
                    else
                    {
                        if (ch == formfeed)
                        {
                            droppedFF = true;
                        }
                        p.wsbuf[i] = ignore;
                    }
                            }
            p.writeWhitespace(len(p.wsbuf)); 

            // make sure we have a line break
            if (needsLinebreak)
            {
                p.writeByte('\n', 1L);
                wroteNewline = true;
            }
            return;
        }

        // containsLinebreak reports whether the whitespace buffer contains any line breaks.
        private static bool containsLinebreak(this ref printer p)
        {
            foreach (var (_, ch) in p.wsbuf)
            {
                if (ch == newline || ch == formfeed)
                {
                    return true;
                }
            }
            return false;
        }

        // intersperseComments consumes all comments that appear before the next token
        // tok and prints it together with the buffered whitespace (i.e., the whitespace
        // that needs to be written before the next token). A heuristic is used to mix
        // the comments and whitespace. The intersperseComments result indicates if a
        // newline was written or if a formfeed was dropped from the whitespace buffer.
        //
        private static (bool, bool) intersperseComments(this ref printer p, token.Position next, token.Token tok)
        {
            ref ast.Comment last = default;
            while (p.commentBefore(next))
            {
                foreach (var (_, c) in p.comment.List)
                {
                    p.writeCommentPrefix(p.posFor(c.Pos()), next, last, tok);
                    p.writeComment(c);
                    last = c;
                }
                p.nextComment();
            }


            if (last != null)
            { 
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
                if (p.mode & noExtraBlank == 0L && last.Text[1L] == '*' && p.lineFor(last.Pos()) == next.Line && tok != token.COMMA && (tok != token.RPAREN || p.prevOpen == token.LPAREN) && (tok != token.RBRACK || p.prevOpen == token.LBRACK))
                {
                    if (p.containsLinebreak() && p.mode & noExtraLinebreak == 0L && p.level == 0L)
                    {
                        needsLinebreak = true;
                    }
                    else
                    {
                        p.writeByte(' ', 1L);
                    }
                } 
                // Ensure that there is a line break after a //-style comment,
                // before EOF, and before a closing '}' unless explicitly disabled.
                if (last.Text[1L] == '/' || tok == token.EOF || tok == token.RBRACE && p.mode & noExtraLinebreak == 0L)
                {
                    needsLinebreak = true;
                }
                return p.writeCommentSuffix(needsLinebreak);
            } 

            // no comment was written - we should never reach here since
            // intersperseComments should not be called in that case
            p.internalError("intersperseComments called without pending comments");
            return;
        }

        // whiteWhitespace writes the first n whitespace entries.
        private static void writeWhitespace(this ref printer p, long n)
        { 
            // write entries
            for (long i = 0L; i < n; i++)
            {
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
                        if (p.indent < 0L)
                        {
                            p.internalError("negative indentation:", p.indent);
                            p.indent = 0L;
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
                        if (i + 1L < n && p.wsbuf[i + 1L] == unindent)
                        { 
                            // Use a formfeed to terminate the current section.
                            // Otherwise, a long label name on the next line leading
                            // to a wide column may increase the indentation column
                            // of lines before the label; effectively leading to wrong
                            // indentation.
                            p.wsbuf[i] = unindent;
                            p.wsbuf[i + 1L] = formfeed;
                            i--; // do it again
                            continue;
                        }
                    }
                    // default: 
                        p.writeByte(byte(ch), 1L);

                    __switch_break0:;
                }
            } 

            // shift remaining entries down
 

            // shift remaining entries down
            var l = copy(p.wsbuf, p.wsbuf[n..]);
            p.wsbuf = p.wsbuf[..l];
        }

        // ----------------------------------------------------------------------------
        // Printing interface

        // nlines limits n to maxNewlines.
        private static long nlimit(long n)
        {
            if (n > maxNewlines)
            {
                n = maxNewlines;
            }
            return n;
        }

        private static bool mayCombine(token.Token prev, byte next)
        {

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
                        return;
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
        private static void print(this ref printer _p, params object[] args) => func(_p, (ref printer p, Defer _, Panic panic, Recover __) =>
        {
            foreach (var (_, arg) in args)
            { 
                // information about the current arg
                @string data = default;
                bool isLit = default;
                bool impliedSemi = default; // value for p.impliedSemi after this arg

                // record previous opening token, if any

                if (p.lastTok == token.ILLEGAL)                 else if (p.lastTok == token.LPAREN || p.lastTok == token.LBRACK) 
                    p.prevOpen = p.lastTok;
                else 
                    // other tokens followed any opening token
                    p.prevOpen = token.ILLEGAL;
                                switch (arg.type())
                {
                    case pmode x:
                        p.mode ^= x;
                        continue;
                        break;
                    case whiteSpace x:
                        if (x == ignore)
                        { 
                            // don't add ignore's to the buffer; they
                            // may screw up "correcting" unindents (see
                            // LabeledStmt)
                            continue;
                        }
                        var i = len(p.wsbuf);
                        if (i == cap(p.wsbuf))
                        { 
                            // Whitespace sequences are very short so this should
                            // never happen. Handle gracefully (but possibly with
                            // bad comment placement) if it does happen.
                            p.writeWhitespace(i);
                            i = 0L;
                        }
                        p.wsbuf = p.wsbuf[0L..i + 1L];
                        p.wsbuf[i] = x;
                        if (x == newline || x == formfeed)
                        { 
                            // newlines affect the current state (p.impliedSemi)
                            // and not the state after printing arg (impliedSemi)
                            // because comments can be interspersed before the arg
                            // in this case
                            p.impliedSemi = false;
                        }
                        p.lastTok = token.ILLEGAL;
                        continue;
                        break;
                    case ref ast.Ident x:
                        data = x.Name;
                        impliedSemi = true;
                        p.lastTok = token.IDENT;
                        break;
                    case ref ast.BasicLit x:
                        data = x.Value;
                        isLit = true;
                        impliedSemi = true;
                        p.lastTok = x.Kind;
                        break;
                    case token.Token x:
                        var s = x.String();
                        if (mayCombine(p.lastTok, s[0L]))
                        { 
                            // the previous and the current token must be
                            // separated by a blank otherwise they combine
                            // into a different incorrect token sequence
                            // (except for token.INT followed by a '.' this
                            // should never happen because it is taken care
                            // of via binary expression formatting)
                            if (len(p.wsbuf) != 0L)
                            {
                                p.internalError("whitespace buffer not empty");
                            }
                            p.wsbuf = p.wsbuf[0L..1L];
                            p.wsbuf[0L] = ' ';
                        }
                        data = s; 
                        // some keywords followed by a newline imply a semicolon

                        if (x == token.BREAK || x == token.CONTINUE || x == token.FALLTHROUGH || x == token.RETURN || x == token.INC || x == token.DEC || x == token.RPAREN || x == token.RBRACK || x == token.RBRACE) 
                            impliedSemi = true;
                                                p.lastTok = x;
                        break;
                    case token.Pos x:
                        if (x.IsValid())
                        {
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
                if (!p.impliedSemi)
                {
                    var n = nlimit(next.Line - p.pos.Line); 
                    // don't exceed maxNewlines if we already wrote one
                    if (wroteNewline && n == maxNewlines)
                    {
                        n = maxNewlines - 1L;
                    }
                    if (n > 0L)
                    {
                        var ch = byte('\n');
                        if (droppedFF)
                        {
                            ch = '\f'; // use formfeed since we dropped one before
                        }
                        p.writeByte(ch, n);
                        impliedSemi = false;
                    }
                } 

                // the next token starts now - record its line number if requested
                if (p.linePtr != null)
                {
                    p.linePtr.Value = p.@out.Line;
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
        private static (bool, bool) flush(this ref printer p, token.Position next, token.Token tok)
        {
            if (p.commentBefore(next))
            { 
                // if there are comments before the next item, intersperse them
                wroteNewline, droppedFF = p.intersperseComments(next, tok);
            }
            else
            { 
                // otherwise, write any leftover whitespace
                p.writeWhitespace(len(p.wsbuf));
            }
            return;
        }

        // getNode returns the ast.CommentGroup associated with n, if any.
        private static ref ast.CommentGroup getDoc(ast.Node n)
        {
            switch (n.type())
            {
                case ref ast.Field n:
                    return n.Doc;
                    break;
                case ref ast.ImportSpec n:
                    return n.Doc;
                    break;
                case ref ast.ValueSpec n:
                    return n.Doc;
                    break;
                case ref ast.TypeSpec n:
                    return n.Doc;
                    break;
                case ref ast.GenDecl n:
                    return n.Doc;
                    break;
                case ref ast.FuncDecl n:
                    return n.Doc;
                    break;
                case ref ast.File n:
                    return n.Doc;
                    break;
            }
            return null;
        }

        private static ref ast.CommentGroup getLastComment(ast.Node n)
        {
            switch (n.type())
            {
                case ref ast.Field n:
                    return n.Comment;
                    break;
                case ref ast.ImportSpec n:
                    return n.Comment;
                    break;
                case ref ast.ValueSpec n:
                    return n.Comment;
                    break;
                case ref ast.TypeSpec n:
                    return n.Comment;
                    break;
                case ref ast.GenDecl n:
                    if (len(n.Specs) > 0L)
                    {
                        return getLastComment(n.Specs[len(n.Specs) - 1L]);
                    }
                    break;
                case ref ast.File n:
                    if (len(n.Comments) > 0L)
                    {
                        return n.Comments[len(n.Comments) - 1L];
                    }
                    break;
            }
            return null;
        }

        private static error printNode(this ref printer p, object node)
        { 
            // unpack *CommentedNode, if any
            slice<ref ast.CommentGroup> comments = default;
            {
                ref CommentedNode (cnode, ok) = node._<ref CommentedNode>();

                if (ok)
                {
                    node = cnode.Node;
                    comments = cnode.Comments;
                }

            }

            if (comments != null)
            { 
                // commented node - restrict comment list to relevant range
                ast.Node (n, ok) = node._<ast.Node>();
                if (!ok)
                {
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

                    if (doc != null)
                    {
                        beg = doc.Pos();
                    }

                }
                {
                    var com = getLastComment(n);

                    if (com != null)
                    {
                        {
                            var e = com.End();

                            if (e > end)
                            {
                                end = e;
                            }

                        }
                    } 
                    // token.Pos values are global offsets, we can
                    // compare them directly

                } 
                // token.Pos values are global offsets, we can
                // compare them directly
                long i = 0L;
                while (i < len(comments) && comments[i].End() < beg)
                {
                    i++;
                }

                var j = i;
                while (j < len(comments) && comments[j].Pos() < end)
                {
                    j++;
                }

                if (i < j)
                {
                    p.comments = comments[i..j];
                }
            }            {
                ast.Node n__prev2 = n;

                (n, ok) = node._<ref ast.File>();


                else if (ok)
                { 
                    // use ast.File comments, if any
                    p.comments = n.Comments;
                } 

                // if there are no comments, use node comments

                n = n__prev2;

            } 

            // if there are no comments, use node comments
            p.useNodeComments = p.comments == null; 

            // get comments ready for use
            p.nextComment(); 

            // format node
            switch (node.type())
            {
                case ast.Expr n:
                    p.expr(n);
                    break;
                case ast.Stmt n:
                    {
                        ref ast.LabeledStmt (_, ok) = n._<ref ast.LabeledStmt>();

                        if (ok)
                        {
                            p.indent = 1L;
                        }

                    }
                    p.stmt(n, false);
                    break;
                case ast.Decl n:
                    p.decl(n);
                    break;
                case ast.Spec n:
                    p.spec(n, 1L, false);
                    break;
                case slice<ast.Stmt> n:
                    foreach (var (_, s) in n)
                    {
                        {
                            (_, ok) = s._<ref ast.LabeledStmt>();

                            if (ok)
                            {
                                p.indent = 1L;
                            }

                        }
                    }
                    p.stmtList(n, 0L, false);
                    break;
                case slice<ast.Decl> n:
                    p.declList(n);
                    break;
                case ref ast.File n:
                    p.file(n);
                    break;
                default:
                {
                    var n = node.type();
                    goto unsupported;
                    break;
                }

            }

            return error.As(null);

unsupported:
            return error.As(fmt.Errorf("go/printer: unsupported node type %T", node));
        }

        // ----------------------------------------------------------------------------
        // Trimmer

        // A trimmer is an io.Writer filter for stripping tabwriter.Escape
        // characters, trailing blanks and tabs, and for converting formfeed
        // and vtab characters into newlines and htabs (in case no tabwriter
        // is used). Text bracketed by tabwriter.Escape characters is passed
        // through unchanged.
        //
        private partial struct trimmer
        {
            public io.Writer output;
            public long state;
            public slice<byte> space;
        }

        // trimmer is implemented as a state machine.
        // It can be in one of the following states:
        private static readonly var inSpace = iota; // inside space
        private static readonly var inEscape = 0; // inside text bracketed by tabwriter.Escapes
        private static readonly var inText = 1; // inside text

        private static void resetSpace(this ref trimmer p)
        {
            p.state = inSpace;
            p.space = p.space[0L..0L];
        }

        // Design note: It is tempting to eliminate extra blanks occurring in
        //              whitespace in this function as it could simplify some
        //              of the blanks logic in the node printing functions.
        //              However, this would mess up any formatting done by
        //              the tabwriter.

        private static slice<byte> aNewline = (slice<byte>)"\n";

        private static (long, error) Write(this ref trimmer _p, slice<byte> data) => func(_p, (ref trimmer p, Defer _, Panic panic, Recover __) =>
        { 
            // invariants:
            // p.state == inSpace:
            //    p.space is unwritten
            // p.state == inEscape, inText:
            //    data[m:n] is unwritten
            long m = 0L;
            byte b = default;
            foreach (var (__n, __b) in data)
            {
                n = __n;
                b = __b;
                if (b == '\v')
                {
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
                        m = n + 1L; // +1: skip tabwriter.Escape
                    else 
                        _, err = p.output.Write(p.space);
                        p.state = inText;
                        m = n;
                                    else if (p.state == inEscape) 
                    if (b == tabwriter.Escape)
                    {
                        _, err = p.output.Write(data[m..n]);
                        p.resetSpace();
                    }
                else if (p.state == inText) 

                    if (b == '\t' || b == ' ') 
                        _, err = p.output.Write(data[m..n]);
                        p.resetSpace();
                        p.space = append(p.space, b);
                    else if (b == '\n' || b == '\f') 
                        _, err = p.output.Write(data[m..n]);
                        p.resetSpace();
                        if (err == null)
                        {
                            _, err = p.output.Write(aNewline);
                        }
                    else if (b == tabwriter.Escape) 
                        _, err = p.output.Write(data[m..n]);
                        p.state = inEscape;
                        m = n + 1L; // +1: skip tabwriter.Escape
                                    else 
                    panic("unreachable");
                                if (err != null)
                {
                    return;
                }
            }

            n = len(data);


            if (p.state == inEscape || p.state == inText) 
                _, err = p.output.Write(data[m..n]);
                p.resetSpace();
                        return;
        });

        // ----------------------------------------------------------------------------
        // Public interface

        // A Mode value is a set of flags (or 0). They control printing.
        public partial struct Mode // : ulong
        {
        }

        public static readonly Mode RawFormat = 1L << (int)(iota); // do not use a tabwriter; if set, UseSpaces is ignored
        public static readonly var TabIndent = 0; // use tabs for indentation independent of UseSpaces
        public static readonly var UseSpaces = 1; // use spaces instead of tabs for alignment
        public static readonly var SourcePos = 2; // emit //line directives to preserve original source positions

        // A Config node controls the output of Fprint.
        public partial struct Config
        {
            public Mode Mode; // default: 0
            public long Tabwidth; // default: 8
            public long Indent; // default: 0 (all code is indented at least by this much)
        }

        // fprint implements Fprint and takes a nodesSizes map for setting up the printer state.
        private static error fprint(this ref Config cfg, io.Writer output, ref token.FileSet fset, object node, map<ast.Node, long> nodeSizes)
        { 
            // print node
            printer p = default;
            p.init(cfg, fset, nodeSizes);
            err = p.printNode(node);

            if (err != null)
            {
                return;
            } 
            // print outstanding comments
            p.impliedSemi = false; // EOF acts like a newline
            p.flush(new token.Position(Offset:infinity,Line:infinity), token.EOF); 

            // redirect output through a trimmer to eliminate trailing whitespace
            // (Input to a tabwriter must be untrimmed since trailing tabs provide
            // formatting information. The tabwriter could provide trimming
            // functionality but no tabwriter is used when RawFormat is set.)
            output = ref new trimmer(output:output); 

            // redirect output through a tabwriter if necessary
            if (cfg.Mode & RawFormat == 0L)
            {
                var minwidth = cfg.Tabwidth;

                var padchar = byte('\t');
                if (cfg.Mode & UseSpaces != 0L)
                {
                    padchar = ' ';
                }
                var twmode = tabwriter.DiscardEmptyColumns;
                if (cfg.Mode & TabIndent != 0L)
                {
                    minwidth = 0L;
                    twmode |= tabwriter.TabIndent;
                }
                output = tabwriter.NewWriter(output, minwidth, cfg.Tabwidth, 1L, padchar, twmode);
            } 

            // write printer result via tabwriter/trimmer to output
            _, err = output.Write(p.output);

            if (err != null)
            {
                return;
            } 

            // flush tabwriter, if any
            {
                ref tabwriter.Writer (tw, _) = output._<ref tabwriter.Writer>();

                if (tw != null)
                {
                    err = tw.Flush();
                }

            }

            return;
        }

        // A CommentedNode bundles an AST node and corresponding comments.
        // It may be provided as argument to any of the Fprint functions.
        //
        public partial struct CommentedNode
        {
            public slice<ref ast.CommentGroup> Comments;
        }

        // Fprint "pretty-prints" an AST node to output for a given configuration cfg.
        // Position information is interpreted relative to the file set fset.
        // The node type must be *ast.File, *CommentedNode, []ast.Decl, []ast.Stmt,
        // or assignment-compatible to ast.Expr, ast.Decl, ast.Spec, or ast.Stmt.
        //
        private static error Fprint(this ref Config cfg, io.Writer output, ref token.FileSet fset, object node)
        {
            return error.As(cfg.fprint(output, fset, node, make_map<ast.Node, long>()));
        }

        // Fprint "pretty-prints" an AST node to output.
        // It calls Config.Fprint with default settings.
        // Note that gofmt uses tabs for indentation but spaces for alignment;
        // use format.Node (package go/format) for output that matches gofmt.
        //
        public static error Fprint(io.Writer output, ref token.FileSet fset, object node)
        {
            return error.As((ref new Config(Tabwidth:8)).Fprint(output, fset, node));
        }
    }
}}
