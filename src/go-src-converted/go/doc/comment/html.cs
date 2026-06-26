// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.doc;

using bytes = bytes_package;
using fmt = fmt_package;
using strconv = strconv_package;

partial class comment_package {

// An htmlPrinter holds the state needed for printing a [Doc] as HTML.
[GoType] partial struct htmlPrinter {
    public partial ref ж<Printer> Printer { get; }
    internal bool tight;
}

// HTML returns an HTML formatting of the [Doc].
// See the [Printer] documentation for ways to customize the HTML output.
[GoRecv] public static slice<byte> HTML(this ref Printer p, ж<Doc> Ꮡd) {
    ref var d = ref Ꮡd.val;

    var hp = Ꮡ(new htmlPrinter(Printer: p));
    ref var out = ref heap(new bytes_package.Buffer(), out var Ꮡout);
    foreach (var (_, x) in d.Content) {
        hp.block(Ꮡ@out, x);
    }
    return @out.Bytes();
}

// block prints the block x to out.
[GoRecv] internal static void block(this ref htmlPrinter p, ж<bytes.Buffer> Ꮡout, Block x) {
    ref var @out = ref Ꮡout.val;

    switch (x.type()) {
    default: {
        var x = x.type();
        fmt.Fprintf(~@out, "?%T"u8, x);
        break;
    }
    case Paragraph.val x: {
        if (!p.tight) {
            @out.WriteString("<p>"u8);
        }
        p.text(Ꮡout, (~x).Text);
        @out.WriteString("\n"u8);
        break;
    }
    case Heading.val x: {
        @out.WriteString("<h"u8);
        @string h = strconv.Itoa(p.headingLevel());
        @out.WriteString(h);
        {
            @string id = p.headingID(Ꮡx); if (id != ""u8) {
                @out.WriteString(@" id="""u8);
                p.escape(Ꮡout, id);
                @out.WriteString(@""""u8);
            }
        }
        @out.WriteString(">"u8);
        p.text(Ꮡout, (~x).Text);
        @out.WriteString("</h"u8);
        @out.WriteString(h);
        @out.WriteString(">\n"u8);
        break;
    }
    case Code.val x: {
        @out.WriteString("<pre>"u8);
        p.escape(Ꮡout, (~x).Text);
        @out.WriteString("</pre>\n"u8);
        break;
    }
    case List.val x: {
        @string kind = "ol>\n"u8;
        if ((~(~x).Items[0]).Number == ""u8) {
            kind = "ul>\n"u8;
        }
        @out.WriteString("<"u8);
        @out.WriteString(kind);
        @string next = "1"u8;
        foreach (var (_, item) in (~x).Items) {
            @out.WriteString("<li"u8);
            {
                @string n = item.val.Number; if (n != ""u8) {
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
            p.tight = !x.BlankBetween();
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
    ref var @out = ref Ꮡout.val;

    foreach (var (_, t) in x) {
        switch (t.type()) {
        case Plain t: {
            p.escape(Ꮡout, ((@string)t));
            break;
        }
        case Italic t: {
            @out.WriteString("<i>"u8);
            p.escape(Ꮡout, ((@string)t));
            @out.WriteString("</i>"u8);
            break;
        }
        case Link.val t: {
            @out.WriteString(@"<a href="""u8);
            p.escape(Ꮡout, (~t).URL);
            @out.WriteString(@""">"u8);
            p.text(Ꮡout, (~t).Text);
            @out.WriteString("</a>"u8);
            break;
        }
        case DocLink.val t: {
            @string url = p.docLinkURL(t);
            if (url != ""u8) {
                @out.WriteString(@"<a href="""u8);
                p.escape(Ꮡout, url);
                @out.WriteString(@""">"u8);
            }
            p.text(Ꮡout, (~t).Text);
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
    ref var @out = ref Ꮡout.val;

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
