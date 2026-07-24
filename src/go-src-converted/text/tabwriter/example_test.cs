// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.text;

using fmt = fmt_package;
using os = os_package;
using tabwriter = go.text.tabwriter_package;
using go.text;
using io = io_package;

partial class tabwriter_test_package {

public static void ExampleWriter_Init() {
    var w = @new<tabwriter.Writer>();
    // Format in tab-separated columns with a tab stop of 8.
    w.Init(new os.FileжWriter(os.Stdout), 0, 8, 0, (rune)'\t', 0);
    fmt.Fprintln(new tabwriter.WriterжWriter(w), "a\tb\tc\td\t.");
    fmt.Fprintln(new tabwriter.WriterжWriter(w), "123\t12345\t1234567\t123456789\t.");
    fmt.Fprintln(new tabwriter.WriterжWriter(w));
    w.Flush();
    // Format right-aligned in space-separated columns of minimal width 5
    // and at least one blank of padding (so wider column entries do not
    // touch each other).
    w.Init(new os.FileжWriter(os.Stdout), 5, 0, 1, (rune)' ', tabwriter.AlignRight);
    fmt.Fprintln(new tabwriter.WriterжWriter(w), "a\tb\tc\td\t.");
    fmt.Fprintln(new tabwriter.WriterжWriter(w), "123\t12345\t1234567\t123456789\t.");
    fmt.Fprintln(new tabwriter.WriterжWriter(w));
    w.Flush();
}

// output:
// a	b	c	d		.
// 123	12345	1234567	123456789	.
//
//     a     b       c         d.
//   123 12345 1234567 123456789.
public static void Example_elastic() {
    // Observe how the b's and the d's, despite appearing in the
    // second cell of each line, belong to different columns.
    var w = tabwriter.NewWriter(new os.FileжWriter(os.Stdout), 0, 0, 1, (rune)'.', (nuint)(tabwriter.AlignRight | tabwriter.Debug));
    fmt.Fprintln(new tabwriter.WriterжWriter(w), "a\tb\tc");
    fmt.Fprintln(new tabwriter.WriterжWriter(w), "aa\tbb\tcc");
    fmt.Fprintln(new tabwriter.WriterжWriter(w), "aaa\t");
    // trailing tab
    fmt.Fprintln(new tabwriter.WriterжWriter(w), "aaaa\tdddd\teeee");
    w.Flush();
}

// output:
// ....a|..b|c
// ...aa|.bb|cc
// ..aaa|
// .aaaa|.dddd|eeee
public static void Example_trailingTab() {
    // Observe that the third line has no trailing tab,
    // so its final cell is not part of an aligned column.
    const nint padding = 3;
    var w = tabwriter.NewWriter(new os.FileжWriter(os.Stdout), 0, 0, padding, (rune)'-', (nuint)(tabwriter.AlignRight | tabwriter.Debug));
    fmt.Fprintln(new tabwriter.WriterжWriter(w), "a\tb\taligned\t");
    fmt.Fprintln(new tabwriter.WriterжWriter(w), "aa\tbb\taligned\t");
    fmt.Fprintln(new tabwriter.WriterжWriter(w), "aaa\tbbb\tunaligned");
    // no trailing tab
    fmt.Fprintln(new tabwriter.WriterжWriter(w), "aaaa\tbbbb\taligned\t");
    w.Flush();
}

// output:
// ------a|------b|---aligned|
// -----aa|-----bb|---aligned|
// ----aaa|----bbb|unaligned
// ---aaaa|---bbbb|---aligned|

} // end tabwriter_test_package
