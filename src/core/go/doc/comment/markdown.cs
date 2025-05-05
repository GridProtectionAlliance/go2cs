// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.doc;

using bytes = bytes_package;
using fmt = fmt_package;
using strings = strings_package;

partial class comment_package {

// An mdPrinter holds the state needed for printing a Doc as Markdown.
[GoType] partial struct mdPrinter {
    public partial ref ж<Printer> Printer { get; }
    internal @string headingPrefix;
    internal bytes_package.Buffer raw;
}

// Markdown returns a Markdown formatting of the Doc.
// See the [Printer] documentation for ways to customize the Markdown output.
[GoRecv] public static slice<byte> Markdown(this ref Printer p, ж<Doc> Ꮡd) {
    ref var d = ref Ꮡd.val;

    var mp = Ꮡ(new mdPrinter(
        Printer: p,
        headingPrefix: strings.Repeat("#"u8, p.headingLevel()) + " "u8
    ));
    ref var out = ref heap(new bytes_package.Buffer(), out var Ꮡout);
    foreach (var (i, x) in d.Content) {
        if (i > 0) {
            @out.WriteByte((rune)'\n');
        }
        mp.block(Ꮡ@out, x);
    }
    return @out.Bytes();
}

// block prints the block x to out.
[GoRecv] internal static void block(this ref mdPrinter p, ж<bytes.Buffer> Ꮡout, Block x) {
    ref var @out = ref Ꮡout.val;

    switch (x.type()) {
    default: {
        var x = x.type();
        fmt.Fprintf(~@out, "?%T"u8, x);
        break;
    }
    case Paragraph.val x: {
        p.text(Ꮡout, (~x).Text);
        @out.WriteString("\n"u8);
        break;
    }
    case Heading.val x: {
        @out.WriteString(p.headingPrefix);
        p.text(Ꮡout, (~x).Text);
        {
            @string id = p.headingID(Ꮡx); if (id != ""u8) {
                @out.WriteString(" {#"u8);
                @out.WriteString(id);
                @out.WriteString("}"u8);
            }
        }
        @out.WriteString("\n"u8);
        break;
    }
    case Code.val x: {
        @string md = x.val.Text;
        while (md != ""u8) {
            @string line = default!;
            (line, md, _) = strings.Cut(md, "\n"u8);
            if (line != ""u8) {
                @out.WriteString("\t"u8);
                @out.WriteString(line);
            }
            @out.WriteString("\n"u8);
        }
        break;
    }
    case List.val x: {
        var loose = x.BlankBetween();
        foreach (var (i, item) in (~x).Items) {
            if (i > 0 && loose) {
                @out.WriteString("\n"u8);
            }
            {
                @string n = item.val.Number; if (n != ""u8){
                    @out.WriteString(" "u8);
                    @out.WriteString(n);
                    @out.WriteString(". "u8);
                } else {
                    @out.WriteString("  - "u8);
                }
            }
            // SP SP - SP
            foreach (var (iΔ1, blk) in (~item).Content) {
                @string fourSpace = "    "u8;
                if (iΔ1 > 0) {
                    @out.WriteString("\n" + fourSpace);
                }
                p.text(Ꮡout, blk._<Paragraph.val>().Text);
                @out.WriteString("\n"u8);
            }
        }
        break;
    }}
}

// text prints the text sequence x to out.
[GoRecv] internal static void text(this ref mdPrinter p, ж<bytes.Buffer> Ꮡout, slice<ΔText> x) {
    ref var @out = ref Ꮡout.val;

    p.raw.Reset();
    p.rawText(Ꮡ(p.raw), x);
    var line = bytes.TrimSpace(p.raw.Bytes());
    if (len(line) == 0) {
        return;
    }
    switch (line[0]) {
    case (rune)'+' or (rune)'-' or (rune)'*' or (rune)'#': {
        @out.WriteByte((rune)'\\');
        break;
    }
    case (rune)'0' or (rune)'1' or (rune)'2' or (rune)'3' or (rune)'4' or (rune)'5' or (rune)'6' or (rune)'7' or (rune)'8' or (rune)'9': {
        nint i = 1;
        while (i < len(line) && (rune)'0' <= line[i] && line[i] <= (rune)'9') {
            // Escape what would be the start of an unordered list or heading.
            i++;
        }
        if (i < len(line) && (line[i] == (rune)'.' || line[i] == (rune)')')) {
            // Escape what would be the start of an ordered list.
            @out.Write(line[..(int)(i)]);
            @out.WriteByte((rune)'\\');
            line = line[(int)(i)..];
        }
        break;
    }}

    @out.Write(line);
}

// rawText prints the text sequence x to out,
// without worrying about escaping characters
// that have special meaning at the start of a Markdown line.
[GoRecv] internal static void rawText(this ref mdPrinter p, ж<bytes.Buffer> Ꮡout, slice<ΔText> x) {
    ref var @out = ref Ꮡout.val;

    foreach (var (_, t) in x) {
        switch (t.type()) {
        case Plain t: {
            p.escape(Ꮡout, ((@string)t));
            break;
        }
        case Italic t: {
            @out.WriteString("*"u8);
            p.escape(Ꮡout, ((@string)t));
            @out.WriteString("*"u8);
            break;
        }
        case Link.val t: {
            @out.WriteString("["u8);
            p.rawText(Ꮡout, (~t).Text);
            @out.WriteString("]("u8);
            @out.WriteString((~t).URL);
            @out.WriteString(")"u8);
            break;
        }
        case DocLink.val t: {
            @string url = p.docLinkURL(t);
            if (url != ""u8) {
                @out.WriteString("["u8);
            }
            p.rawText(Ꮡout, (~t).Text);
            if (url != ""u8) {
                @out.WriteString("]("u8);
                url = strings.ReplaceAll(url, "("u8, "%28"u8);
                url = strings.ReplaceAll(url, ")"u8, "%29"u8);
                @out.WriteString(url);
                @out.WriteString(")"u8);
            }
            break;
        }}
    }
}

// escape prints s to out as plain text,
// escaping special characters to avoid being misinterpreted
// as Markdown markup sequences.
[GoRecv] internal static void escape(this ref mdPrinter p, ж<bytes.Buffer> Ꮡout, @string s) {
    ref var @out = ref Ꮡout.val;

    nint start = 0;
    for (nint i = 0; i < len(s); i++) {
        switch (s[i]) {
        case (rune)'\n': {
            @out.WriteString(s[(int)(start)..(int)(i)]);
            @out.WriteByte((rune)' ');
            start = i + 1;
            continue;
            break;
        }
        case (rune)'`' or (rune)'_' or (rune)'*' or (rune)'[' or (rune)'<' or (rune)'\\': {
            @out.WriteString(s[(int)(start)..(int)(i)]);
            @out.WriteByte((rune)'\\');
            @out.WriteByte(s[i]);
            start = i + 1;
            break;
        }}

    }
    // Turn all \n into spaces, for a few reasons:
    //   - Avoid introducing paragraph breaks accidentally.
    //   - Avoid the need to reindent after the newline.
    //   - Avoid problems with Markdown renderers treating
    //     every mid-paragraph newline as a <br>.
    // Not all of these need to be escaped all the time,
    // but is valid and easy to do so.
    // We assume the Markdown is being passed to a
    // Markdown renderer, not edited by a person,
    // so it's fine to have escapes that are not strictly
    // necessary in some cases.
    @out.WriteString(s[(int)(start)..]);
}

} // end comment_package
