// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using strings = go.strings_package;
using Δunicode = unicode_package;
using @unsafe = unsafe_package;
using go;
using io = io_package;

partial class strings_test_package {

public static void ExampleClone() {
    @string s = "abc"u8;
    @string clone = strings.Clone(s);
    fmt.Println(s == clone);
    fmt.Println(@unsafe.StringData(s) == @unsafe.StringData(clone));
}

// Output:
// true
// false
public static void ExampleBuilder() {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    for (nint i = 3; i >= 1; i--) {
        fmt.Fprintf(new strings.BuilderжWriter(Ꮡb), "%d..."u8, i);
    }
    Ꮡb.WriteString("ignition"u8);
    fmt.Println(b.String());
}

// Output: 3...2...1...ignition
public static void ExampleCompare() {
    fmt.Println(strings.Compare("a"u8, "b"u8));
    fmt.Println(strings.Compare("a"u8, "a"u8));
    fmt.Println(strings.Compare("b"u8, "a"u8));
}

// Output:
// -1
// 0
// 1
public static void ExampleContains() {
    fmt.Println(strings.Contains("seafood"u8, "foo"u8));
    fmt.Println(strings.Contains("seafood"u8, "bar"u8));
    fmt.Println(strings.Contains("seafood"u8, ""u8));
    fmt.Println(strings.Contains(""u8, ""u8));
}

// Output:
// true
// false
// true
// true
public static void ExampleContainsAny() {
    fmt.Println(strings.ContainsAny("team"u8, "i"u8));
    fmt.Println(strings.ContainsAny("fail"u8, "ui"u8));
    fmt.Println(strings.ContainsAny("ure"u8, "ui"u8));
    fmt.Println(strings.ContainsAny("failure"u8, "ui"u8));
    fmt.Println(strings.ContainsAny("foo"u8, ""u8));
    fmt.Println(strings.ContainsAny(""u8, ""u8));
}

// Output:
// false
// true
// true
// true
// false
// false
public static void ExampleContainsRune() {
    // Finds whether a string contains a particular Unicode code point.
    // The code point for the lowercase letter "a", for example, is 97.
    fmt.Println(strings.ContainsRune("aardvark"u8, 97));
    fmt.Println(strings.ContainsRune("timeout"u8, 97));
}

// Output:
// true
// false
public static void ExampleContainsFunc() {
    var f = (rune r) => r == (rune)'a' || r == (rune)'e' || r == (rune)'i' || r == (rune)'o' || r == (rune)'u';
    fmt.Println(strings.ContainsFunc("hello"u8, f));
    fmt.Println(strings.ContainsFunc("rhythms"u8, f));
}

// Output:
// true
// false
public static void ExampleCount() {
    fmt.Println(strings.Count("cheese"u8, "e"u8));
    fmt.Println(strings.Count("five"u8, ""u8));
}

// before & after each rune
// Output:
// 3
// 5
public static void ExampleCut() {
    var show = (@string s, @string sep) => {
        var (before, after, found) = strings.Cut(s, sep);
        fmt.Printf("Cut(%q, %q) = %q, %q, %v\n"u8, s, sep, before, after, found);
    };
    show("Gopher"u8, "Go"u8);
    show("Gopher"u8, "ph"u8);
    show("Gopher"u8, "er"u8);
    show("Gopher"u8, "Badger"u8);
}

// Output:
// Cut("Gopher", "Go") = "", "pher", true
// Cut("Gopher", "ph") = "Go", "er", true
// Cut("Gopher", "er") = "Goph", "", true
// Cut("Gopher", "Badger") = "Gopher", "", false
public static void ExampleCutPrefix() {
    var show = (@string s, @string sep) => {
        var (after, found) = strings.CutPrefix(s, sep);
        fmt.Printf("CutPrefix(%q, %q) = %q, %v\n"u8, s, sep, after, found);
    };
    show("Gopher"u8, "Go"u8);
    show("Gopher"u8, "ph"u8);
}

// Output:
// CutPrefix("Gopher", "Go") = "pher", true
// CutPrefix("Gopher", "ph") = "Gopher", false
public static void ExampleCutSuffix() {
    var show = (@string s, @string sep) => {
        var (before, found) = strings.CutSuffix(s, sep);
        fmt.Printf("CutSuffix(%q, %q) = %q, %v\n"u8, s, sep, before, found);
    };
    show("Gopher"u8, "Go"u8);
    show("Gopher"u8, "er"u8);
}

// Output:
// CutSuffix("Gopher", "Go") = "Gopher", false
// CutSuffix("Gopher", "er") = "Goph", true
public static void ExampleEqualFold() {
    fmt.Println(strings.EqualFold("Go"u8, "go"u8));
    fmt.Println(strings.EqualFold("AB"u8, "ab"u8));
    // true because comparison uses simple case-folding
    fmt.Println(strings.EqualFold("ß"u8, "ss"u8));
}

// false because comparison does not use full case-folding
// Output:
// true
// true
// false
public static void ExampleFields() {
    fmt.Printf("Fields are: %q"u8, strings.Fields("  foo bar  baz   "u8));
}

// Output: Fields are: ["foo" "bar" "baz"]
public static void ExampleFieldsFunc() {
    var f = (rune c) => !Δunicode.IsLetter(c) && !Δunicode.IsNumber(c);
    fmt.Printf("Fields are: %q"u8, strings.FieldsFunc("  foo1;bar2,baz3..."u8, f));
}

// Output: Fields are: ["foo1" "bar2" "baz3"]
public static void ExampleHasPrefix() {
    fmt.Println(strings.HasPrefix("Gopher"u8, "Go"u8));
    fmt.Println(strings.HasPrefix("Gopher"u8, "C"u8));
    fmt.Println(strings.HasPrefix("Gopher"u8, ""u8));
}

// Output:
// true
// false
// true
public static void ExampleHasSuffix() {
    fmt.Println(strings.HasSuffix("Amigo"u8, "go"u8));
    fmt.Println(strings.HasSuffix("Amigo"u8, "O"u8));
    fmt.Println(strings.HasSuffix("Amigo"u8, "Ami"u8));
    fmt.Println(strings.HasSuffix("Amigo"u8, ""u8));
}

// Output:
// true
// false
// false
// true
public static void ExampleIndex() {
    fmt.Println(strings.Index("chicken"u8, "ken"u8));
    fmt.Println(strings.Index("chicken"u8, "dmr"u8));
}

// Output:
// 4
// -1
public static void ExampleIndexFunc() {
    var f = (rune c) => Δunicode.Is(Δunicode.Han, c);
    fmt.Println(strings.IndexFunc("Hello, 世界"u8, f));
    fmt.Println(strings.IndexFunc("Hello, world"u8, f));
}

// Output:
// 7
// -1
public static void ExampleIndexAny() {
    fmt.Println(strings.IndexAny("chicken"u8, "aeiouy"u8));
    fmt.Println(strings.IndexAny("crwth"u8, "aeiouy"u8));
}

// Output:
// 2
// -1
public static void ExampleIndexByte() {
    fmt.Println(strings.IndexByte("golang"u8, (rune)'g'));
    fmt.Println(strings.IndexByte("gophers"u8, (rune)'h'));
    fmt.Println(strings.IndexByte("golang"u8, (rune)'x'));
}

// Output:
// 0
// 3
// -1
public static void ExampleIndexRune() {
    fmt.Println(strings.IndexRune("chicken"u8, (rune)'k'));
    fmt.Println(strings.IndexRune("chicken"u8, (rune)'d'));
}

// Output:
// 4
// -1
public static void ExampleLastIndex() {
    fmt.Println(strings.Index("go gopher"u8, "go"u8));
    fmt.Println(strings.LastIndex("go gopher"u8, "go"u8));
    fmt.Println(strings.LastIndex("go gopher"u8, "rodent"u8));
}

// Output:
// 0
// 3
// -1
public static void ExampleLastIndexAny() {
    fmt.Println(strings.LastIndexAny("go gopher"u8, "go"u8));
    fmt.Println(strings.LastIndexAny("go gopher"u8, "rodent"u8));
    fmt.Println(strings.LastIndexAny("go gopher"u8, "fail"u8));
}

// Output:
// 4
// 8
// -1
public static void ExampleLastIndexByte() {
    fmt.Println(strings.LastIndexByte("Hello, world"u8, (rune)'l'));
    fmt.Println(strings.LastIndexByte("Hello, world"u8, (rune)'o'));
    fmt.Println(strings.LastIndexByte("Hello, world"u8, (rune)'x'));
}

// Output:
// 10
// 8
// -1
public static void ExampleLastIndexFunc() {
    fmt.Println(strings.LastIndexFunc("go 123"u8, Δunicode.IsNumber));
    fmt.Println(strings.LastIndexFunc("123 go"u8, Δunicode.IsNumber));
    fmt.Println(strings.LastIndexFunc("go"u8, Δunicode.IsNumber));
}

// Output:
// 5
// 2
// -1
public static void ExampleJoin() {
    var s = new @string[]{"foo", "bar", "baz"}.slice();
    fmt.Println(strings.Join(s, ", "u8));
}

// Output: foo, bar, baz
public static void ExampleRepeat() {
    fmt.Println("ba" + strings.Repeat("na"u8, 2));
}

// Output: banana
public static void ExampleReplace() {
    fmt.Println(strings.Replace("oink oink oink"u8, "k"u8, "ky"u8, 2));
    fmt.Println(strings.Replace("oink oink oink"u8, "oink"u8, "moo"u8, -1));
}

// Output:
// oinky oinky oink
// moo moo moo
public static void ExampleReplaceAll() {
    fmt.Println(strings.ReplaceAll("oink oink oink"u8, "oink"u8, "moo"u8));
}

// Output:
// moo moo moo
public static void ExampleSplit() {
    fmt.Printf("%q\n"u8, strings.Split("a,b,c"u8, ","u8));
    fmt.Printf("%q\n"u8, strings.Split("a man a plan a canal panama"u8, "a "u8));
    fmt.Printf("%q\n"u8, strings.Split(" xyz "u8, ""u8));
    fmt.Printf("%q\n"u8, strings.Split(""u8, "Bernardo O'Higgins"u8));
}

// Output:
// ["a" "b" "c"]
// ["" "man " "plan " "canal panama"]
// [" " "x" "y" "z" " "]
// [""]
public static void ExampleSplitN() {
    fmt.Printf("%q\n"u8, strings.SplitN("a,b,c"u8, ","u8, 2));
    var z = strings.SplitN("a,b,c"u8, ","u8, 0);
    fmt.Printf("%q (nil = %v)\n"u8, z, z == default!);
}

// Output:
// ["a" "b,c"]
// [] (nil = true)
public static void ExampleSplitAfter() {
    fmt.Printf("%q\n"u8, strings.SplitAfter("a,b,c"u8, ","u8));
}

// Output: ["a," "b," "c"]
public static void ExampleSplitAfterN() {
    fmt.Printf("%q\n"u8, strings.SplitAfterN("a,b,c"u8, ","u8, 2));
}

// Output: ["a," "b,c"]
public static void ExampleTitle() {
    // Compare this example to the ToTitle example.
    fmt.Println(strings.Title("her royal highness"u8));
    fmt.Println(strings.Title("loud noises"u8));
    fmt.Println(strings.Title("хлеб"u8));
}

// Output:
// Her Royal Highness
// Loud Noises
// Хлеб
public static void ExampleToTitle() {
    // Compare this example to the Title example.
    fmt.Println(strings.ToTitle("her royal highness"u8));
    fmt.Println(strings.ToTitle("loud noises"u8));
    fmt.Println(strings.ToTitle("хлеб"u8));
}

// Output:
// HER ROYAL HIGHNESS
// LOUD NOISES
// ХЛЕБ
public static void ExampleToTitleSpecial() {
    fmt.Println(strings.ToTitleSpecial(Δunicode.TurkishCase, "dünyanın ilk borsa yapısı Aizonai kabul edilir"u8));
}

// Output:
// DÜNYANIN İLK BORSA YAPISI AİZONAİ KABUL EDİLİR
public static void ExampleMap() {
    var rot13 = (rune r) => {
        switch (ᐧ) {
        case {} when r >= (rune)'A' && r <= (rune)'Z': {
            return (rune)'A' + (r - (rune)'A' + 13) % 26;
        }
        case {} when r >= (rune)'a' && r <= (rune)'z': {
            return (rune)'a' + (r - (rune)'a' + 13) % 26;
        }}

        return r;
    };
    fmt.Println(strings.Map(rot13, "'Twas brillig and the slithy gopher..."u8));
}

// Output: 'Gjnf oevyyvt naq gur fyvgul tbcure...
public static void ExampleNewReplacer() {
    var r = strings.NewReplacer("<"u8, "&lt;", ">", "&gt;");
    fmt.Println(r.Replace("This is <b>HTML</b>!"u8));
}

// Output: This is &lt;b&gt;HTML&lt;/b&gt;!
public static void ExampleToUpper() {
    fmt.Println(strings.ToUpper("Gopher"u8));
}

// Output: GOPHER
public static void ExampleToUpperSpecial() {
    fmt.Println(strings.ToUpperSpecial(Δunicode.TurkishCase, "örnek iş"u8));
}

// Output: ÖRNEK İŞ
public static void ExampleToLower() {
    fmt.Println(strings.ToLower("Gopher"u8));
}

// Output: gopher
public static void ExampleToLowerSpecial() {
    fmt.Println(strings.ToLowerSpecial(Δunicode.TurkishCase, "Önnek İş"u8));
}

// Output: önnek iş
public static void ExampleTrim() {
    fmt.Print(strings.Trim("¡¡¡Hello, Gophers!!!"u8, "!¡"u8));
}

// Output: Hello, Gophers
public static void ExampleTrimSpace() {
    fmt.Println(strings.TrimSpace(" \t\n Hello, Gophers \n\t\r\n"u8));
}

// Output: Hello, Gophers
public static void ExampleTrimPrefix() {
    @string s = "¡¡¡Hello, Gophers!!!"u8;
    s = strings.TrimPrefix(s, "¡¡¡Hello, "u8);
    s = strings.TrimPrefix(s, "¡¡¡Howdy, "u8);
    fmt.Print(s);
}

// Output: Gophers!!!
public static void ExampleTrimSuffix() {
    @string s = "¡¡¡Hello, Gophers!!!"u8;
    s = strings.TrimSuffix(s, ", Gophers!!!"u8);
    s = strings.TrimSuffix(s, ", Marmots!!!"u8);
    fmt.Print(s);
}

// Output: ¡¡¡Hello
public static void ExampleTrimFunc() {
    fmt.Print(strings.TrimFunc("¡¡¡Hello, Gophers!!!"u8, (rune r) => !Δunicode.IsLetter(r) && !Δunicode.IsNumber(r)));
}

// Output: Hello, Gophers
public static void ExampleTrimLeft() {
    fmt.Print(strings.TrimLeft("¡¡¡Hello, Gophers!!!"u8, "!¡"u8));
}

// Output: Hello, Gophers!!!
public static void ExampleTrimLeftFunc() {
    fmt.Print(strings.TrimLeftFunc("¡¡¡Hello, Gophers!!!"u8, (rune r) => !Δunicode.IsLetter(r) && !Δunicode.IsNumber(r)));
}

// Output: Hello, Gophers!!!
public static void ExampleTrimRight() {
    fmt.Print(strings.TrimRight("¡¡¡Hello, Gophers!!!"u8, "!¡"u8));
}

// Output: ¡¡¡Hello, Gophers
public static void ExampleTrimRightFunc() {
    fmt.Print(strings.TrimRightFunc("¡¡¡Hello, Gophers!!!"u8, (rune r) => !Δunicode.IsLetter(r) && !Δunicode.IsNumber(r)));
}

// Output: ¡¡¡Hello, Gophers
public static void ExampleToValidUTF8() {
    fmt.Printf("%s\n"u8, strings.ToValidUTF8("abc"u8, "\uFFFD"u8));
    fmt.Printf("%s\n"u8, strings.ToValidUTF8(((@string)(new byte[]{0x61, 0xff, 0x62, 0xc0, 0xaf, 0x63, 0xff})), ""u8));
    fmt.Printf("%s\n"u8, strings.ToValidUTF8(((@string)(new byte[]{0xed, 0xa0, 0x80})), "abc"u8));
}

// Output:
// abc
// abc
// abc

} // end strings_test_package
