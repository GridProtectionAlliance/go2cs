// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Godoc comment extraction and comment -> HTML formatting.

// package doc -- go2cs converted at 2022 March 13 05:52:27 UTC
// import "go/doc" ==> using doc = go.go.doc_package
// Original source: C:\Program Files\Go\src\go\doc\comment.go
namespace go.go;

using bytes = bytes_package;
using lazyregexp = @internal.lazyregexp_package;
using io = io_package;
using strings = strings_package;
using template = text.template_package; // for HTMLEscape
using unicode = unicode_package;
using utf8 = unicode.utf8_package;
using System;

public static partial class doc_package {

private static readonly @string ldquo = "&ldquo;";
private static readonly @string rdquo = "&rdquo;";
private static readonly @string ulquo = "“";
private static readonly @string urquo = "”";

private static var htmlQuoteReplacer = strings.NewReplacer(ulquo, ldquo, urquo, rdquo);private static var unicodeQuoteReplacer = strings.NewReplacer("``", ulquo, "''", urquo);

// Escape comment text for HTML. If nice is set,
// also turn `` into &ldquo; and '' into &rdquo;.
private static void commentEscape(io.Writer w, @string text, bool nice) {
    if (nice) { 
        // In the first pass, we convert `` and '' into their unicode equivalents.
        // This prevents them from being escaped in HTMLEscape.
        text = convertQuotes(text);
        ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
        template.HTMLEscape(_addr_buf, (slice<byte>)text); 
        // Now we convert the unicode quotes to their HTML escaped entities to maintain old behavior.
        // We need to use a temp buffer to read the string back and do the conversion,
        // otherwise HTMLEscape will escape & to &amp;
        htmlQuoteReplacer.WriteString(w, buf.String());
        return ;
    }
    template.HTMLEscape(w, (slice<byte>)text);
}

private static @string convertQuotes(@string text) {
    return unicodeQuoteReplacer.Replace(text);
}

 
// Regexp for Go identifiers
private static readonly @string identRx = "[\\pL_][\\pL_0-9]*"; 

// Regexp for URLs
// Match parens, and check later for balance - see #5043, #22285
// Match .,:;?! within path, but not at end - see #18139, #16565
// This excludes some rare yet valid urls ending in common punctuation
// in order to allow sentences ending in URLs.

// protocol (required) e.g. http
private static readonly @string protoPart = "(https?|ftp|file|gopher|mailto|nntp)"; 
// host (required) e.g. www.example.com or [::1]:8080
private static readonly @string hostPart = "([a-zA-Z0-9_@\\-.\\[\\]:]+)"; 
// path+query+fragment (optional) e.g. /path/index.html?q=foo#bar
private static readonly @string pathPart = "([.,:;?!]*[a-zA-Z0-9$\'()*+&#=@~_/\\-\\[\\]%])*";

private static readonly var urlRx = protoPart + "://" + hostPart + pathPart;

private static var matchRx = lazyregexp.New("(" + urlRx + ")|(" + identRx + ")");

private static slice<byte> html_a = (slice<byte>)"<a href=\"";private static slice<byte> html_aq = (slice<byte>)"\">";private static slice<byte> html_enda = (slice<byte>)"</a>";private static slice<byte> html_i = (slice<byte>)"<i>";private static slice<byte> html_endi = (slice<byte>)"</i>";private static slice<byte> html_p = (slice<byte>)"<p>\n";private static slice<byte> html_endp = (slice<byte>)"</p>\n";private static slice<byte> html_pre = (slice<byte>)"<pre>";private static slice<byte> html_endpre = (slice<byte>)"</pre>\n";private static slice<byte> html_h = (slice<byte>)"<h3 id=\"";private static slice<byte> html_hq = (slice<byte>)"\">";private static slice<byte> html_endh = (slice<byte>)"</h3>\n";

// Emphasize and escape a line of text for HTML. URLs are converted into links;
// if the URL also appears in the words map, the link is taken from the map (if
// the corresponding map value is the empty string, the URL is not converted
// into a link). Go identifiers that appear in the words map are italicized; if
// the corresponding map value is not the empty string, it is considered a URL
// and the word is converted into a link. If nice is set, the remaining text's
// appearance is improved where it makes sense (e.g., `` is turned into &ldquo;
// and '' into &rdquo;).
private static void emphasize(io.Writer w, @string line, map<@string, @string> words, bool nice) {
    while (true) {
        var m = matchRx.FindStringSubmatchIndex(line);
        if (m == null) {
            break;
        }
        commentEscape(w, line[(int)0..(int)m[0]], nice); 

        // adjust match for URLs
        var match = line[(int)m[0]..(int)m[1]];
        if (strings.Contains(match, "://")) {
            var m0 = m[0];
            var m1 = m[1];
            foreach (var (_, s) in new slice<@string>(new @string[] { "()", "{}", "[]" })) {
                var open = s[..(int)1];
                var close = s[(int)1..]; // E.g., "(" and ")"
                // require opening parentheses before closing parentheses (#22285)
                {
                    var i__prev2 = i;

                    var i = strings.Index(match, close);

                    if (i >= 0 && i < strings.Index(match, open)) {
                        m1 = m0 + i;
                        match = line[(int)m0..(int)m1];
                    } 
                    // require balanced pairs of parentheses (#5043)

                    i = i__prev2;

                } 
                // require balanced pairs of parentheses (#5043)
                {
                    var i__prev3 = i;

                    for (i = 0; strings.Count(match, open) != strings.Count(match, close) && i < 10; i++) {
                        m1 = strings.LastIndexAny(line[..(int)m1], s);
                        match = line[(int)m0..(int)m1];
                    }


                    i = i__prev3;
                }
            }
            if (m1 != m[1]) { 
                // redo matching with shortened line for correct indices
                m = matchRx.FindStringSubmatchIndex(line[..(int)m[0] + len(match)]);
            }
        }
        @string url = "";
        var italics = false;
        if (words != null) {
            url, italics = words[match];
        }
        if (m[2] >= 0) { 
            // match against first parenthesized sub-regexp; must be match against urlRx
            if (!italics) { 
                // no alternative URL in words list, use match instead
                url = match;
            }
            italics = false; // don't italicize URLs
        }
        if (len(url) > 0) {
            w.Write(html_a);
            template.HTMLEscape(w, (slice<byte>)url);
            w.Write(html_aq);
        }
        if (italics) {
            w.Write(html_i);
        }
        commentEscape(w, match, nice);
        if (italics) {
            w.Write(html_endi);
        }
        if (len(url) > 0) {
            w.Write(html_enda);
        }
        line = line[(int)m[1]..];
    }
    commentEscape(w, line, nice);
}

private static nint indentLen(@string s) {
    nint i = 0;
    while (i < len(s) && (s[i] == ' ' || s[i] == '\t')) {
        i++;
    }
    return i;
}

private static bool isBlank(@string s) {
    return len(s) == 0 || (len(s) == 1 && s[0] == '\n');
}

private static @string commonPrefix(@string a, @string b) {
    nint i = 0;
    while (i < len(a) && i < len(b) && a[i] == b[i]) {
        i++;
    }
    return a[(int)0..(int)i];
}

private static void unindent(slice<@string> block) {
    if (len(block) == 0) {
        return ;
    }
    var prefix = block[0][(int)0..(int)indentLen(block[0])];
    {
        var line__prev1 = line;

        foreach (var (_, __line) in block) {
            line = __line;
            if (!isBlank(line)) {
                prefix = commonPrefix(prefix, line[(int)0..(int)indentLen(line)]);
            }
        }
        line = line__prev1;
    }

    var n = len(prefix); 

    // remove
    {
        var line__prev1 = line;

        foreach (var (__i, __line) in block) {
            i = __i;
            line = __line;
            if (!isBlank(line)) {
                block[i] = line[(int)n..];
            }
        }
        line = line__prev1;
    }
}

// heading returns the trimmed line if it passes as a section heading;
// otherwise it returns the empty string.
private static @string heading(@string line) {
    line = strings.TrimSpace(line);
    if (len(line) == 0) {
        return "";
    }
    var (r, _) = utf8.DecodeRuneInString(line);
    if (!unicode.IsLetter(r) || !unicode.IsUpper(r)) {
        return "";
    }
    r, _ = utf8.DecodeLastRuneInString(line);
    if (!unicode.IsLetter(r) && !unicode.IsDigit(r)) {
        return "";
    }
    if (strings.ContainsAny(line, ";:!?+*/=[]{}_^°&§~%#@<\">\\")) {
        return "";
    }
    {
        var b__prev1 = b;

        var b = line;

        while () {
            var i = strings.IndexRune(b, '\'');
            if (i < 0) {
                break;
            }
            if (i + 1 >= len(b) || b[i + 1] != 's' || (i + 2 < len(b) && b[i + 2] != ' ')) {
                return ""; // not followed by "s "
            }
            b = b[(int)i + 2..];
        }

        b = b__prev1;
    } 

    // allow "." when followed by non-space
    {
        var b__prev1 = b;

        b = line;

        while () {
            i = strings.IndexRune(b, '.');
            if (i < 0) {
                break;
            }
            if (i + 1 >= len(b) || b[i + 1] == ' ') {
                return ""; // not followed by non-space
            }
            b = b[(int)i + 1..];
        }

        b = b__prev1;
    }

    return line;
}

private partial struct op { // : nint
}

private static readonly op opPara = iota;
private static readonly var opHead = 0;
private static readonly var opPre = 1;

private partial struct block {
    public op op;
    public slice<@string> lines;
}

private static var nonAlphaNumRx = lazyregexp.New("[^a-zA-Z0-9]");

private static @string anchorID(@string line) { 
    // Add a "hdr-" prefix to avoid conflicting with IDs used for package symbols.
    return "hdr-" + nonAlphaNumRx.ReplaceAllString(line, "_");
}

// ToHTML converts comment text to formatted HTML.
// The comment was prepared by DocReader,
// so it is known not to have leading, trailing blank lines
// nor to have trailing spaces at the end of lines.
// The comment markers have already been removed.
//
// Each span of unindented non-blank lines is converted into
// a single paragraph. There is one exception to the rule: a span that
// consists of a single line, is followed by another paragraph span,
// begins with a capital letter, and contains no punctuation
// other than parentheses and commas is formatted as a heading.
//
// A span of indented lines is converted into a <pre> block,
// with the common indent prefix removed.
//
// URLs in the comment text are converted into links; if the URL also appears
// in the words map, the link is taken from the map (if the corresponding map
// value is the empty string, the URL is not converted into a link).
//
// A pair of (consecutive) backticks (`) is converted to a unicode left quote (“), and a pair of (consecutive)
// single quotes (') is converted to a unicode right quote (”).
//
// Go identifiers that appear in the words map are italicized; if the corresponding
// map value is not the empty string, it is considered a URL and the word is converted
// into a link.
public static void ToHTML(io.Writer w, @string text, map<@string, @string> words) {
    foreach (var (_, b) in blocks(text)) {

        if (b.op == opPara) 
            w.Write(html_p);
            {
                var line__prev2 = line;

                foreach (var (_, __line) in b.lines) {
                    line = __line;
                    emphasize(w, line, words, true);
                }

                line = line__prev2;
            }

            w.Write(html_endp);
        else if (b.op == opHead) 
            w.Write(html_h);
            @string id = "";
            {
                var line__prev2 = line;

                foreach (var (_, __line) in b.lines) {
                    line = __line;
                    if (id == "") {
                        id = anchorID(line);
                        w.Write((slice<byte>)id);
                        w.Write(html_hq);
                    }
                    commentEscape(w, line, true);
                }

                line = line__prev2;
            }

            if (id == "") {
                w.Write(html_hq);
            }
            w.Write(html_endh);
        else if (b.op == opPre) 
            w.Write(html_pre);
            {
                var line__prev2 = line;

                foreach (var (_, __line) in b.lines) {
                    line = __line;
                    emphasize(w, line, null, false);
                }

                line = line__prev2;
            }

            w.Write(html_endpre);
            }
}

private static slice<block> blocks(@string text) {
    slice<block> @out = default;    slice<@string> para = default;    var lastWasBlank = false;    var lastWasHeading = false;

    Action close = () => {
        if (para != null) {
            out = append(out, new block(opPara,para));
            para = null;
        }
    };

    var lines = strings.SplitAfter(text, "\n");
    unindent(lines);
    {
        nint i = 0;

        while (i < len(lines)) {
            var line = lines[i];
            if (isBlank(line)) { 
                // close paragraph
                close();
                i++;
                lastWasBlank = true;
                continue;
            }
            if (indentLen(line) > 0) { 
                // close paragraph
                close(); 

                // count indented or blank lines
                var j = i + 1;
                while (j < len(lines) && (isBlank(lines[j]) || indentLen(lines[j]) > 0)) {
                    j++;
                } 
                // but not trailing blank lines
 
                // but not trailing blank lines
                while (j > i && isBlank(lines[j - 1])) {
                    j--;
                }

                var pre = lines[(int)i..(int)j];
                i = j;

                unindent(pre); 

                // put those lines in a pre block
                out = append(out, new block(opPre,pre));
                lastWasHeading = false;
                continue;
            }
            if (lastWasBlank && !lastWasHeading && i + 2 < len(lines) && isBlank(lines[i + 1]) && !isBlank(lines[i + 2]) && indentLen(lines[i + 2]) == 0) { 
                // current line is non-blank, surrounded by blank lines
                // and the next non-blank line is not indented: this
                // might be a heading.
                {
                    var head = heading(line);

                    if (head != "") {
                        close();
                        out = append(out, new block(opHead,[]string{head}));
                        i += 2;
                        lastWasHeading = true;
                        continue;
                    }

                }
            } 

            // open paragraph
            lastWasBlank = false;
            lastWasHeading = false;
            para = append(para, lines[i]);
            i++;
        }
    }
    close();

    return out;
}

// ToText prepares comment text for presentation in textual output.
// It wraps paragraphs of text to width or fewer Unicode code points
// and then prefixes each line with the indent. In preformatted sections
// (such as program text), it prefixes each non-blank line with preIndent.
//
// A pair of (consecutive) backticks (`) is converted to a unicode left quote (“), and a pair of (consecutive)
// single quotes (') is converted to a unicode right quote (”).
public static void ToText(io.Writer w, @string text, @string indent, @string preIndent, nint width) {
    lineWrapper l = new lineWrapper(out:w,width:width,indent:indent,);
    foreach (var (_, b) in blocks(text)) {

        if (b.op == opPara) 
            // l.write will add leading newline if required
            {
                var line__prev2 = line;

                foreach (var (_, __line) in b.lines) {
                    line = __line;
                    line = convertQuotes(line);
                    l.write(line);
                }

                line = line__prev2;
            }

            l.flush();
        else if (b.op == opHead) 
            w.Write(nl);
            {
                var line__prev2 = line;

                foreach (var (_, __line) in b.lines) {
                    line = __line;
                    line = convertQuotes(line);
                    l.write(line + "\n");
                }

                line = line__prev2;
            }

            l.flush();
        else if (b.op == opPre) 
            w.Write(nl);
            {
                var line__prev2 = line;

                foreach (var (_, __line) in b.lines) {
                    line = __line;
                    if (isBlank(line)) {
                        w.Write((slice<byte>)"\n");
                    }
                    else
 {
                        w.Write((slice<byte>)preIndent);
                        w.Write((slice<byte>)line);
                    }
                }

                line = line__prev2;
            }
            }
}

private partial struct lineWrapper {
    public io.Writer @out;
    public bool printed;
    public nint width;
    public @string indent;
    public nint n;
    public nint pendSpace;
}

private static slice<byte> nl = (slice<byte>)"\n";
private static slice<byte> space = (slice<byte>)" ";
private static slice<byte> prefix = (slice<byte>)"// ";

private static void write(this ptr<lineWrapper> _addr_l, @string text) {
    ref lineWrapper l = ref _addr_l.val;

    if (l.n == 0 && l.printed) {
        l.@out.Write(nl); // blank line before new paragraph
    }
    l.printed = true;

    var needsPrefix = false;
    var isComment = strings.HasPrefix(text, "//");
    foreach (var (_, f) in strings.Fields(text)) {
        var w = utf8.RuneCountInString(f); 
        // wrap if line is too long
        if (l.n > 0 && l.n + l.pendSpace + w > l.width) {
            l.@out.Write(nl);
            l.n = 0;
            l.pendSpace = 0;
            needsPrefix = isComment && !strings.HasPrefix(f, "//");
        }
        if (l.n == 0) {
            l.@out.Write((slice<byte>)l.indent);
        }
        if (needsPrefix) {
            l.@out.Write(prefix);
            needsPrefix = false;
        }
        l.@out.Write(space[..(int)l.pendSpace]);
        l.@out.Write((slice<byte>)f);
        l.n += l.pendSpace + w;
        l.pendSpace = 1;
    }
}

private static void flush(this ptr<lineWrapper> _addr_l) {
    ref lineWrapper l = ref _addr_l.val;

    if (l.n == 0) {
        return ;
    }
    l.@out.Write(nl);
    l.pendSpace = 0;
    l.n = 0;
}

} // end doc_package
