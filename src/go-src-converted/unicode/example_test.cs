// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using Δunicode = unicode_package;

partial class unicode_test_package {

// Functions starting with "Is" can be used to inspect which table of range a
// rune belongs to. Note that runes may fit into more than one range.
public static void Example_is() {
    // constant with mixed type runes
    @string mixed = "\b5Ὂg̀9! ℃ᾭG"u8;
    foreach (var (_, c) in (@string)mixed) {
        fmt.Printf("For %q:\n"u8, c);
        if (Δunicode.IsControl(c)) {
            fmt.Println("\tis control rune");
        }
        if (Δunicode.IsDigit(c)) {
            fmt.Println("\tis digit rune");
        }
        if (Δunicode.IsGraphic(c)) {
            fmt.Println("\tis graphic rune");
        }
        if (Δunicode.IsLetter(c)) {
            fmt.Println("\tis letter rune");
        }
        if (Δunicode.IsLower(c)) {
            fmt.Println("\tis lower case rune");
        }
        if (Δunicode.IsMark(c)) {
            fmt.Println("\tis mark rune");
        }
        if (Δunicode.IsNumber(c)) {
            fmt.Println("\tis number rune");
        }
        if (Δunicode.IsPrint(c)) {
            fmt.Println("\tis printable rune");
        }
        if (!Δunicode.IsPrint(c)) {
            fmt.Println("\tis not printable rune");
        }
        if (Δunicode.IsPunct(c)) {
            fmt.Println("\tis punct rune");
        }
        if (Δunicode.IsSpace(c)) {
            fmt.Println("\tis space rune");
        }
        if (Δunicode.IsSymbol(c)) {
            fmt.Println("\tis symbol rune");
        }
        if (Δunicode.IsTitle(c)) {
            fmt.Println("\tis title case rune");
        }
        if (Δunicode.IsUpper(c)) {
            fmt.Println("\tis upper case rune");
        }
    }
}

// Output:
// For '\b':
// 	is control rune
// 	is not printable rune
// For '5':
// 	is digit rune
// 	is graphic rune
// 	is number rune
// 	is printable rune
// For 'Ὂ':
// 	is graphic rune
// 	is letter rune
// 	is printable rune
// 	is upper case rune
// For 'g':
// 	is graphic rune
// 	is letter rune
// 	is lower case rune
// 	is printable rune
// For '̀':
// 	is graphic rune
// 	is mark rune
// 	is printable rune
// For '9':
// 	is digit rune
// 	is graphic rune
// 	is number rune
// 	is printable rune
// For '!':
// 	is graphic rune
// 	is printable rune
// 	is punct rune
// For ' ':
// 	is graphic rune
// 	is printable rune
// 	is space rune
// For '℃':
// 	is graphic rune
// 	is printable rune
// 	is symbol rune
// For 'ᾭ':
// 	is graphic rune
// 	is letter rune
// 	is printable rune
// 	is title case rune
// For 'G':
// 	is graphic rune
// 	is letter rune
// 	is printable rune
// 	is upper case rune
public static void ExampleSimpleFold() {
    fmt.Printf("%#U\n"u8, Δunicode.SimpleFold((rune)'A'));
    // 'a'
    fmt.Printf("%#U\n"u8, Δunicode.SimpleFold((rune)'a'));
    // 'A'
    fmt.Printf("%#U\n"u8, Δunicode.SimpleFold((rune)'K'));
    // 'k'
    fmt.Printf("%#U\n"u8, Δunicode.SimpleFold((rune)'k'));
    // '\u212A' (Kelvin symbol, K)
    fmt.Printf("%#U\n"u8, Δunicode.SimpleFold((rune)'\u212A'));
    // 'K'
    fmt.Printf("%#U\n"u8, Δunicode.SimpleFold((rune)'1'));
}

// '1'
// Output:
// U+0061 'a'
// U+0041 'A'
// U+006B 'k'
// U+212A 'K'
// U+004B 'K'
// U+0031 '1'
public static void ExampleTo() {
    const rune lcG = /* 'g' */ 103;
    fmt.Printf("%#U\n"u8, Δunicode.To(Δunicode.UpperCase, lcG));
    fmt.Printf("%#U\n"u8, Δunicode.To(Δunicode.LowerCase, lcG));
    fmt.Printf("%#U\n"u8, Δunicode.To(Δunicode.TitleCase, lcG));
    const rune ucG = /* 'G' */ 71;
    fmt.Printf("%#U\n"u8, Δunicode.To(Δunicode.UpperCase, ucG));
    fmt.Printf("%#U\n"u8, Δunicode.To(Δunicode.LowerCase, ucG));
    fmt.Printf("%#U\n"u8, Δunicode.To(Δunicode.TitleCase, ucG));
}

// Output:
// U+0047 'G'
// U+0067 'g'
// U+0047 'G'
// U+0047 'G'
// U+0067 'g'
// U+0047 'G'
public static void ExampleToLower() {
    const rune ucG = /* 'G' */ 71;
    fmt.Printf("%#U\n"u8, Δunicode.ToLower(ucG));
}

// Output:
// U+0067 'g'
public static void ExampleToTitle() {
    const rune ucG = /* 'g' */ 103;
    fmt.Printf("%#U\n"u8, Δunicode.ToTitle(ucG));
}

// Output:
// U+0047 'G'
public static void ExampleToUpper() {
    const rune ucG = /* 'g' */ 103;
    fmt.Printf("%#U\n"u8, Δunicode.ToUpper(ucG));
}

// Output:
// U+0047 'G'
public static void ExampleSpecialCase() {
    var t = Δunicode.TurkishCase;
    const rune lci = /* 'i' */ 105;
    fmt.Printf("%#U\n"u8, t.ToLower(lci));
    fmt.Printf("%#U\n"u8, t.ToTitle(lci));
    fmt.Printf("%#U\n"u8, t.ToUpper(lci));
    const rune uci = /* 'İ' */ 304;
    fmt.Printf("%#U\n"u8, t.ToLower(uci));
    fmt.Printf("%#U\n"u8, t.ToTitle(uci));
    fmt.Printf("%#U\n"u8, t.ToUpper(uci));
}

// Output:
// U+0069 'i'
// U+0130 'İ'
// U+0130 'İ'
// U+0069 'i'
// U+0130 'İ'
// U+0130 'İ'
public static void ExampleIsDigit() {
    fmt.Printf("%t\n"u8, Δunicode.IsDigit((rune)'৩'));
    fmt.Printf("%t\n"u8, Δunicode.IsDigit((rune)'A'));
}

// Output:
// true
// false
public static void ExampleIsNumber() {
    fmt.Printf("%t\n"u8, Δunicode.IsNumber((rune)'Ⅷ'));
    fmt.Printf("%t\n"u8, Δunicode.IsNumber((rune)'A'));
}

// Output:
// true
// false
public static void ExampleIsLetter() {
    fmt.Printf("%t\n"u8, Δunicode.IsLetter((rune)'A'));
    fmt.Printf("%t\n"u8, Δunicode.IsLetter((rune)'7'));
}

// Output:
// true
// false
public static void ExampleIsLower() {
    fmt.Printf("%t\n"u8, Δunicode.IsLower((rune)'a'));
    fmt.Printf("%t\n"u8, Δunicode.IsLower((rune)'A'));
}

// Output:
// true
// false
public static void ExampleIsUpper() {
    fmt.Printf("%t\n"u8, Δunicode.IsUpper((rune)'A'));
    fmt.Printf("%t\n"u8, Δunicode.IsUpper((rune)'a'));
}

// Output:
// true
// false
public static void ExampleIsTitle() {
    fmt.Printf("%t\n"u8, Δunicode.IsTitle((rune)'ǅ'));
    fmt.Printf("%t\n"u8, Δunicode.IsTitle((rune)'a'));
}

// Output:
// true
// false
public static void ExampleIsSpace() {
    fmt.Printf("%t\n"u8, Δunicode.IsSpace((rune)' '));
    fmt.Printf("%t\n"u8, Δunicode.IsSpace((rune)'\n'));
    fmt.Printf("%t\n"u8, Δunicode.IsSpace((rune)'\t'));
    fmt.Printf("%t\n"u8, Δunicode.IsSpace((rune)'a'));
}

// Output:
// true
// true
// true
// false

} // end unicode_test_package
