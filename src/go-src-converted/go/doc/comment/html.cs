// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.doc;

using bytes = bytes_package;
using fmt = fmt_package;
using strconv = strconv_package;
using io = io_package;

partial class comment_package {

// An htmlPrinter holds the state needed for printing a [Doc] as HTML.
[GoType] partial struct htmlPrinter {
    public partial ref ж<Printer> Printer { get; }
    internal bool tight;
}

// HTML returns an HTML formatting of the [Doc].
// See the [Printer] documentation for ways to customize the HTML output.
public static slice<byte> HTML(this ж<Printer> Ꮡp, ж<Doc> Ꮡd) {
    ref var p = ref Ꮡp.Value;
    ref var d = ref Ꮡd.Value;

    var hp = Ꮡ(new htmlPrinter(Printer: Ꮡp));
    ref var @out = ref heap(new bytes.Buffer(), out var Ꮡout);
    foreach (var (_, x) in d.Content) {
        hp.block(Ꮡout, x);
    }
    return @out.Bytes();
}

// block prints the block x to out.
[GoRecv] internal static void block(this ref htmlPrinter p, ж<bytes.Buffer> Ꮡout, Block x) {
    ref var @out = ref Ꮡout.Value;

    switch (x.type()) {
    default: {
        var xΔ1 = x;
        fmt.Fprintf(new bytes_BufferжWriter(Ꮡout), "?%T"u8, xΔ1);
        break;
    }
    case ж<Paragraph> xΔ1: {
        if (!p.tight) {
            @out.WriteString("<p>"u8);
        }
        p.text(Ꮡout, (~xΔ1).Text);
        @out.WriteString("\n"u8);
        break;
    }
    case ж<Heading> xΔ1: {
        @out.WriteString("<h"u8);
        @string h = strconv.Itoa(p.headingLevel());
        @out.WriteString(h);
        {
            @string id = p.headingID(xΔ1); if (id != ""u8) {
                @out.WriteString(@" id="""u8);
                p.escape(Ꮡout, id);
                @out.WriteString(@""""u8);
            }
        }
        @out.WriteString(">"u8);
        p.text(Ꮡout, (~xΔ1).Text);
        @out.WriteString("</h"u8);
        @out.WriteString(h);
        @out.WriteString(">\n"u8);
        break;
    }
    case ж<Code> xΔ1: {
        @out.WriteString("<pre>"u8);
        p.escape(Ꮡout, (~xΔ1).Text);
        @out.WriteString("</pre>\n"u8);
        break;
    }
    case ж<List> xΔ1: {
        @string kind = "ol>\n"u8;
        if ((~(~xΔ1).Items[0]).Number == ""u8) {
            kind = "ul>\n"u8;
        }
        @out.WriteString("<"u8);
        @out.WriteString(kind);
        @string next = "1"u8;
        foreach (var (_, item) in (~xΔ1).Items) {
            @out.WriteString("<li"u8);
            {
                @string n = item.Value.Number; if (n != ""u8) {
                    if (n != next) {
                        @out.WriteString(@" value="""u8);
                        @out.WriteString(n);
                        @out.WriteString(@""""u8);
                        next = n;
                    }
                    next = inc(next);
                }
            }
            @out.WriteString(">"u8);
            p.tight = !xΔ1.BlankBetween();
            foreach (var (_, blk) in (~item).Content) {
                p.block(Ꮡout, blk);
            }
            p.tight = false;
        }
        @out.WriteString("</"u8);
        @out.WriteString(kind);
        break;
    }}
}

// inc increments the decimal string s.
// For example, inc("1199") == "1200".
internal static @string inc(@string s) {
    var b = slice<byte>(s);
    for (nint i = len(b) - 1; i >= 0; i--) {
        if (b[i] < (rune)'9') {
            b[i]++;
            return ((@string)b);
        }
        b[i] = (rune)'0';
    }
    return "1"u8 + ((@string)b);
}

// text prints the text sequence x to out.
[GoRecv] internal static void text(this ref htmlPrinter p, ж<bytes.Buffer> Ꮡout, slice<ΔText> x) {
    ref var @out = ref Ꮡout.Value;

    foreach (var (_, t) in x) {
        switch (t.type()) {
        case Plain tΔ1: {
            p.escape(Ꮡout, ((@string)tΔ1));
            break;
        }
        case Italic tΔ1: {
            @out.WriteString("<i>"u8);
            p.escape(Ꮡout, ((@string)tΔ1));
            @out.WriteString("</i>"u8);
            break;
        }
        case ж<Link> tΔ1: {
            @out.WriteString(@"<a href="""u8);
            p.escape(Ꮡout, (~tΔ1).URL);
            @out.WriteString(@""">"u8);
            p.text(Ꮡout, (~tΔ1).Text);
            @out.WriteString("</a>"u8);
            break;
        }
        case ж<DocLink> tΔ1: {
            @string url = p.docLinkURL(tΔ1);
            if (url != ""u8) {
                @out.WriteString(@"<a href="""u8);
                p.escape(Ꮡout, url);
                @out.WriteString(@""">"u8);
            }
            p.text(Ꮡout, (~tΔ1).Text);
            if (url != ""u8) {
                @out.WriteString("</a>"u8);
            }
            break;
        }}
    }
}

// escape prints s to out as plain text,
// escaping < & " ' and > to avoid being misinterpreted
// in larger HTML constructs.
[GoRecv] internal static void escape(this ref htmlPrinter p, ж<bytes.Buffer> Ꮡout, @string s) {
    ref var @out = ref Ꮡout.Value;

    nint start = 0;
    for (nint i = 0; i < len(s); i++) {
        switch (s[i]) {
        case (rune)'<': {
            @out.WriteString(s[(int)(start)..(int)(i)]);
            @out.WriteString("&lt;"u8);
            start = i + 1;
            break;
        }
        case (rune)'&': {
            @out.WriteString(s[(int)(start)..(int)(i)]);
            @out.WriteString("&amp;"u8);
            start = i + 1;
            break;
        }
        case (rune)'"': {
            @out.WriteString(s[(int)(start)..(int)(i)]);
            @out.WriteString("&quot;"u8);
            start = i + 1;
            break;
        }
        case (rune)'\'': {
            @out.WriteString(s[(int)(start)..(int)(i)]);
            @out.WriteString("&apos;"u8);
            start = i + 1;
            break;
        }
        case (rune)'>': {
            @out.WriteString(s[(int)(start)..(int)(i)]);
            @out.WriteString("&gt;"u8);
            start = i + 1;
            break;
        }}

    }
    @out.WriteString(s[(int)(start)..]);
}

} // end comment_package
