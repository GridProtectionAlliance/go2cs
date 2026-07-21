// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.text;

using fmt = fmt_package;
using strings = strings_package;
using scanner = go.text.scanner_package;
using unicode = unicode_package;
using go.text;
using io = io_package;

partial class scanner_test_package {

public static void Example() {
    @string src = """

// This is scanned code.
if a > 10 {
	someParsable = text
}
"""u8;
    ref var s = ref heap(new scanner.Scanner(), out var Ꮡs);
    Ꮡs.Init(new strings_ReaderжReader(strings.NewReader(src)));
    s.Filename = "example"u8;
    for (var tok = Ꮡs.Scan(); tok != scanner.EOF; tok = Ꮡs.Scan()) {
        fmt.Printf("%s: %s\n"u8, s.Position, Ꮡs.TokenText());
    }
}

// Output:
// example:3:1: if
// example:3:4: a
// example:3:6: >
// example:3:8: 10
// example:3:11: {
// example:4:2: someParsable
// example:4:15: =
// example:4:17: text
// example:5:1: }
public static void Example_isIdentRune() {
    @string src = "%var1 var2%"u8;
    ref var s = ref heap(new scanner.Scanner(), out var Ꮡs);
    Ꮡs.Init(new strings_ReaderжReader(strings.NewReader(src)));
    s.Filename = "default"u8;
    for (var tok = Ꮡs.Scan(); tok != scanner.EOF; tok = Ꮡs.Scan()) {
        fmt.Printf("%s: %s\n"u8, s.Position, Ꮡs.TokenText());
    }
    fmt.Println();
    Ꮡs.Init(new strings_ReaderжReader(strings.NewReader(src)));
    s.Filename = "percent"u8;
    // treat leading '%' as part of an identifier
    s.IsIdentRune = (rune ch, nint i) => ch == (rune)'%' && i == 0 || unicode.IsLetter(ch) || unicode.IsDigit(ch) && i > 0;
    for (var tok = Ꮡs.Scan(); tok != scanner.EOF; tok = Ꮡs.Scan()) {
        fmt.Printf("%s: %s\n"u8, s.Position, Ꮡs.TokenText());
    }
}

// Output:
// default:1:1: %
// default:1:2: var1
// default:1:7: var2
// default:1:11: %
//
// percent:1:1: %var1
// percent:1:7: var2
// percent:1:11: %
public static void Example_mode() {
    @string src = """

    // Comment begins at column 5.

This line should not be included in the output.

/*
This multiline comment
should be extracted in
its entirety.
*/

"""u8;
    ref var s = ref heap(new scanner.Scanner(), out var Ꮡs);
    Ꮡs.Init(new strings_ReaderжReader(strings.NewReader(src)));
    s.Filename = "comments"u8;
    s.Mode ^= scanner.SkipComments;
    // don't skip comments
    for (var tok = Ꮡs.Scan(); tok != scanner.EOF; tok = Ꮡs.Scan()) {
        @string txt = Ꮡs.TokenText();
        if (strings.HasPrefix(txt, "//"u8) || strings.HasPrefix(txt, "/*"u8)) {
            fmt.Printf("%s: %s\n"u8, s.Position, txt);
        }
    }
}

// Output:
// comments:2:5: // Comment begins at column 5.
// comments:6:1: /*
// This multiline comment
// should be extracted in
// its entirety.
// */
public static void Example_whitespace() {
    // tab-separated values
    @string src = """
aa	ab	ac	ad
ba	bb	bc	bd
ca	cb	cc	cd
da	db	dc	dd
"""u8;
    nint col = default!;
    nint row = default!;
    ref var s = ref heap(new scanner.Scanner(), out var Ꮡs);
    array<array<@string>> tsv = new(4, () => new(4));                            // large enough for example above
    Ꮡs.Init(new strings_ReaderжReader(strings.NewReader(src)));
    s.Whitespace ^= (uint64)((1 << (int)((rune)'\t')) | (1 << (int)((rune)'\n')));
    // don't skip tabs and new lines
    for (var tok = Ꮡs.Scan(); tok != scanner.EOF; tok = Ꮡs.Scan()) {
        switch (tok) {
        case (rune)'\n': {
            row++;
            col = 0;
            break;
        }
        case (rune)'\t': {
            col++;
            break;
        }
        default: {
            tsv[row][col] = Ꮡs.TokenText();
            break;
        }}

    }
    fmt.Print(tsv);
}

// Output:
// [[aa ab ac ad] [ba bb bc bd] [ca cb cc cd] [da db dc dd]]

} // end scanner_test_package
