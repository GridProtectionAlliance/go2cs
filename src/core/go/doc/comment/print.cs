// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.doc;

using bytes = bytes_package;
using fmt = fmt_package;
using strings = strings_package;

partial class comment_package {

// A Printer is a doc comment printer.
// The fields in the struct can be filled in before calling
// any of the printing methods
// in order to customize the details of the printing process.
[GoType] partial struct Printer {
    // HeadingLevel is the nesting level used for
    // HTML and Markdown headings.
    // If HeadingLevel is zero, it defaults to level 3,
    // meaning to use <h3> and ###.
    public nint HeadingLevel;
    // HeadingID is a function that computes the heading ID
    // (anchor tag) to use for the heading h when generating
    // HTML and Markdown. If HeadingID returns an empty string,
    // then the heading ID is omitted.
    // If HeadingID is nil, h.DefaultID is used.
    public Func<ж<Heading>, @string> HeadingID;
    // DocLinkURL is a function that computes the URL for the given DocLink.
    // If DocLinkURL is nil, then link.DefaultURL(p.DocLinkBaseURL) is used.
    public Func<ж<DocLink>, @string> DocLinkURL;
    // DocLinkBaseURL is used when DocLinkURL is nil,
    // passed to [DocLink.DefaultURL] to construct a DocLink's URL.
    // See that method's documentation for details.
    public @string DocLinkBaseURL;
    // TextPrefix is a prefix to print at the start of every line
    // when generating text output using the Text method.
    public @string TextPrefix;
    // TextCodePrefix is the prefix to print at the start of each
    // preformatted (code block) line when generating text output,
    // instead of (not in addition to) TextPrefix.
    // If TextCodePrefix is the empty string, it defaults to TextPrefix+"\t".
    public @string TextCodePrefix;
    // TextWidth is the maximum width text line to generate,
    // measured in Unicode code points,
    // excluding TextPrefix and the newline character.
    // If TextWidth is zero, it defaults to 80 minus the number of code points in TextPrefix.
    // If TextWidth is negative, there is no limit.
    public nint TextWidth;
}

[GoRecv] internal static nint headingLevel(this ref Printer p) {
    if (p.HeadingLevel <= 0) {
        return 3;
    }
    return p.HeadingLevel;
}

[GoRecv] public static @string headingID(this ref Printer p, ж<Heading> Ꮡh) {
    ref var h = ref Ꮡh.val;

    if (p.HeadingID == default!) {
        return h.DefaultID();
    }
    return p.HeadingID(h);
}

[GoRecv] public static @string docLinkURL(this ref Printer p, ж<DocLink> Ꮡlink) {
    ref var link = ref Ꮡlink.val;

    if (p.DocLinkURL != default!) {
        return p.DocLinkURL(link);
    }
    return link.DefaultURL(p.DocLinkBaseURL);
}

// DefaultURL constructs and returns the documentation URL for l,
// using baseURL as a prefix for links to other packages.
//
// The possible forms returned by DefaultURL are:
//   - baseURL/ImportPath, for a link to another package
//   - baseURL/ImportPath#Name, for a link to a const, func, type, or var in another package
//   - baseURL/ImportPath#Recv.Name, for a link to a method in another package
//   - #Name, for a link to a const, func, type, or var in this package
//   - #Recv.Name, for a link to a method in this package
//
// If baseURL ends in a trailing slash, then DefaultURL inserts
// a slash between ImportPath and # in the anchored forms.
// For example, here are some baseURL values and URLs they can generate:
//
//	"/pkg/" → "/pkg/math/#Sqrt"
//	"/pkg"  → "/pkg/math#Sqrt"
//	"/"     → "/math/#Sqrt"
//	""      → "/math#Sqrt"
[GoRecv] public static @string DefaultURL(this ref DocLink l, @string baseURL) {
    if (l.ImportPath != ""u8) {
        @string slash = ""u8;
        if (strings.HasSuffix(baseURL, "/"u8)){
            slash = "/"u8;
        } else {
            baseURL += "/"u8;
        }
        switch (ᐧ) {
        case {} when l.Name == ""u8: {
            return baseURL + l.ImportPath + slash;
        }
        case {} when l.Recv != ""u8: {
            return baseURL + l.ImportPath + slash + "#"u8 + l.Recv + "."u8 + l.Name;
        }
        default: {
            return baseURL + l.ImportPath + slash + "#"u8 + l.Name;
        }}

    }
    if (l.Recv != ""u8) {
        return "#"u8 + l.Recv + "."u8 + l.Name;
    }
    return "#"u8 + l.Name;
}

// DefaultID returns the default anchor ID for the heading h.
//
// The default anchor ID is constructed by converting every
// rune that is not alphanumeric ASCII to an underscore
// and then adding the prefix “hdr-”.
// For example, if the heading text is “Go Doc Comments”,
// the default ID is “hdr-Go_Doc_Comments”.
[GoRecv] public static @string DefaultID(this ref Heading h) {
    // Note: The “hdr-” prefix is important to avoid DOM clobbering attacks.
    // See https://pkg.go.dev/github.com/google/safehtml#Identifier.
    ref var out = ref heap(new strings_package.Builder(), out var Ꮡout);
    textPrinter p = default!;
    p.oneLongLine(Ꮡ@out, h.Text);
    @string s = strings.TrimSpace(@out.String());
    if (s == ""u8) {
        return ""u8;
    }
    @out.Reset();
    @out.WriteString("hdr-"u8);
    foreach (var (_, r) in s) {
        if (r < 128 && isIdentASCII(((byte)r))){
            @out.WriteByte(((byte)r));
        } else {
            @out.WriteByte((rune)'_');
        }
    }
    return @out.String();
}

[GoType] partial struct commentPrinter {
    public partial ref ж<Printer> Printer { get; }
}

// Comment returns the standard Go formatting of the [Doc],
// without any comment markers.
[GoRecv] public static slice<byte> Comment(this ref Printer p, ж<Doc> Ꮡd) {
    ref var d = ref Ꮡd.val;

    var cp = Ꮡ(new commentPrinter(Printer: p));
    ref var out = ref heap(new bytes_package.Buffer(), out var Ꮡout);
    foreach (var (iΔ1, x) in d.Content) {
        if (iΔ1 > 0 && blankBefore(x)) {
            @out.WriteString("\n"u8);
        }
        cp.block(Ꮡ@out, x);
    }
    // Print one block containing all the link definitions that were used,
    // and then a second block containing all the unused ones.
    // This makes it easy to clean up the unused ones: gofmt and
    // delete the final block. And it's a nice visual signal without
    // affecting the way the comment formats for users.
    for (nint i = 0; i < 2; i++) {
        var used = i == 0;
        var first = true;
        foreach (var (_, def) in d.Links) {
            if ((~def).Used == used) {
                if (first) {
                    @out.WriteString("\n"u8);
                    first = false;
                }
                @out.WriteString("["u8);
                @out.WriteString((~def).Text);
                @out.WriteString("]: "u8);
                @out.WriteString((~def).URL);
                @out.WriteString("\n"u8);
            }
        }
    }
    return @out.Bytes();
}

// blankBefore reports whether the block x requires a blank line before it.
// All blocks do, except for Lists that return false from x.BlankBefore().
internal static bool blankBefore(Block x) {
    {
        var (xΔ1, ok) = x._<List.val>(ᐧ); if (ok) {
            return xΔ1.BlankBefore();
        }
    }
    return true;
}

// block prints the block x to out.
[GoRecv] internal static void block(this ref commentPrinter p, ж<bytes.Buffer> Ꮡout, Block x) {
    ref var @out = ref Ꮡout.val;

    switch (x.type()) {
    default: {
        var x = x.type();
        fmt.Fprintf(~@out, "?%T"u8, x);
        break;
    }
    case Paragraph.val x: {
        p.text(Ꮡout, ""u8, (~x).Text);
        @out.WriteString("\n"u8);
        break;
    }
    case Heading.val x: {
        @out.WriteString("# "u8);
        p.text(Ꮡout, ""u8, (~x).Text);
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
            @out.WriteString(" "u8);
            if ((~item).Number == ""u8){
                @out.WriteString(" - "u8);
            } else {
                @out.WriteString((~item).Number);
                @out.WriteString(". "u8);
            }
            foreach (var (iΔ1, blk) in (~item).Content) {
                @string fourSpace = "    "u8;
                if (iΔ1 > 0) {
                    @out.WriteString("\n" + fourSpace);
                }
                p.text(Ꮡout, fourSpace, blk._<Paragraph.val>().Text);
                @out.WriteString("\n"u8);
            }
        }
        break;
    }}
}

// text prints the text sequence x to out.
[GoRecv] internal static void text(this ref commentPrinter p, ж<bytes.Buffer> Ꮡout, @string indent, slice<ΔText> x) {
    ref var @out = ref Ꮡout.val;

    foreach (var (_, t) in x) {
        switch (t.type()) {
        case Plain t: {
            p.indent(Ꮡout, indent, ((@string)t));
            break;
        }
        case Italic t: {
            p.indent(Ꮡout, indent, ((@string)t));
            break;
        }
        case Link.val t: {
            if ((~t).Auto){
                p.text(Ꮡout, indent, (~t).Text);
            } else {
                @out.WriteString("["u8);
                p.text(Ꮡout, indent, (~t).Text);
                @out.WriteString("]"u8);
            }
            break;
        }
        case DocLink.val t: {
            @out.WriteString("["u8);
            p.text(Ꮡout, indent, (~t).Text);
            @out.WriteString("]"u8);
            break;
        }}
    }
}

// indent prints s to out, indenting with the indent string
// after each newline in s.
[GoRecv] internal static void indent(this ref commentPrinter p, ж<bytes.Buffer> Ꮡout, @string indent, @string s) {
    ref var @out = ref Ꮡout.val;

    while (s != ""u8) {
        var (line, rest, ok) = strings.Cut(s, "\n"u8);
        @out.WriteString(line);
        if (ok) {
            @out.WriteString("\n"u8);
            @out.WriteString(indent);
        }
        s = rest;
    }
}

} // end comment_package
