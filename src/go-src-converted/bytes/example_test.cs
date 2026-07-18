// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = go.bytes_package;
using base64 = encoding.base64_package;
using fmt = fmt_package;
using Δio = io_package;
using Δos = os_package;
using slices = slices_package;
using strconv = strconv_package;
using Δunicode = unicode_package;
using encoding;
using go;

partial class bytes_test_package {

public static void ExampleBuffer() {
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);                         // A Buffer needs no initialization.
    b.Write(slice<byte>("Hello "u8));
    fmt.Fprintf(new bytes.BufferжWriter(Ꮡb), "world!"u8);
    b.WriteTo(new os_FileжWriter(Δos.Stdout));
}

// Output: Hello world!
public static void ExampleBuffer_reader() {
    // A Buffer can turn a string or a []byte into an io.Reader.
    var buf = bytes.NewBufferString("R29waGVycyBydWxlIQ=="u8);
    var dec = base64.NewDecoder(base64.StdEncoding, new bytes.BufferжReader(buf));
    Δio.Copy(new os_FileжWriter(Δos.Stdout), dec);
}

// Output: Gophers rule!
public static void ExampleBuffer_Bytes() {
    var buf = new bytes.Buffer(nil);
    buf.Write(new byte[]{(rune)'h', (rune)'e', (rune)'l', (rune)'l', (rune)'o', (rune)' ', (rune)'w', (rune)'o', (rune)'r', (rune)'l', (rune)'d'}.slice());
    Δos.Stdout.Write(buf.Bytes());
}

// Output: hello world
public static void ExampleBuffer_AvailableBuffer() {
    bytes.Buffer buf = default!;
    for (nint i = 0; i < 4; i++) {
        var b = buf.AvailableBuffer();
        b = strconv.AppendInt(b, (int64)i, 10);
        b = append(b, (byte)((rune)' '));
        buf.Write(b);
    }
    Δos.Stdout.Write(buf.Bytes());
}

// Output: 0 1 2 3
public static void ExampleBuffer_Cap() {
    var buf1 = bytes.NewBuffer(new slice<byte>(10));
    var buf2 = bytes.NewBuffer(new slice<byte>(0, 10));
    fmt.Println(buf1.Cap());
    fmt.Println(buf2.Cap());
}

// Output:
// 10
// 10
public static void ExampleBuffer_Grow() {
    bytes.Buffer b = default!;
    b.Grow(64);
    var bb = b.Bytes();
    b.Write(slice<byte>("64 bytes or fewer"u8));
    fmt.Printf("%q"u8, bb[..(int)(b.Len())]);
}

// Output: "64 bytes or fewer"
public static void ExampleBuffer_Len() {
    bytes.Buffer b = default!;
    b.Grow(64);
    b.Write(slice<byte>("abcde"u8));
    fmt.Printf("%d"u8, b.Len());
}

// Output: 5
public static void ExampleBuffer_Next() {
    bytes.Buffer b = default!;
    b.Grow(64);
    b.Write(slice<byte>("abcde"u8));
    fmt.Printf("%s\n"u8, b.Next(2));
    fmt.Printf("%s\n"u8, b.Next(2));
    fmt.Printf("%s"u8, b.Next(2));
}

// Output:
// ab
// cd
// e
public static void ExampleBuffer_Read() {
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    b.Grow(64);
    b.Write(slice<byte>("abcde"u8));
    var rdbuf = new slice<byte>(1);
    var (n, err) = b.Read(rdbuf);
    if (err != default!) {
        throw panic(err);
    }
    fmt.Println(n);
    fmt.Println(Ꮡb.String());
    fmt.Println(((@string)rdbuf));
}

// Output:
// 1
// bcde
// a
public static void ExampleBuffer_ReadByte() {
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    b.Grow(64);
    b.Write(slice<byte>("abcde"u8));
    var (c, err) = b.ReadByte();
    if (err != default!) {
        throw panic(err);
    }
    fmt.Println(c);
    fmt.Println(Ꮡb.String());
}

// Output:
// 97
// bcde
public static void ExampleClone() {
    var b = slice<byte>("abc"u8);
    var clone = bytes.Clone(b);
    fmt.Printf("%s\n"u8, clone);
    clone[0] = (rune)'d';
    fmt.Printf("%s\n"u8, b);
    fmt.Printf("%s\n"u8, clone);
}

// Output:
// abc
// abc
// dbc
public static void ExampleCompare() {
    // Interpret Compare's result by comparing it to zero.
    slice<byte> a = default!;
    slice<byte> b = default!;
    if (bytes.Compare(a, b) < 0) {
    }
    // a less b
    if (bytes.Compare(a, b) <= 0) {
    }
    // a less or equal b
    if (bytes.Compare(a, b) > 0) {
    }
    // a greater b
    if (bytes.Compare(a, b) >= 0) {
    }
    // a greater or equal b
    // Prefer Equal to Compare for equality comparisons.
    if (bytes.Equal(a, b)) {
    }
    // a equal b
    if (!bytes.Equal(a, b)) {
    }
}

// a not equal b
public static void ExampleCompare_search() {
    // Binary search to find a matching byte slice.
    slice<byte> needle = default!;
    slice<slice<byte>> haystack = default!;          // Assume sorted
    var (_, found) = slices.BinarySearchFunc<slice<slice<byte>>, slice<byte>, slice<byte>>(haystack, needle, bytes.Compare);
    if (found) {
    }
}

// Found it!
public static void ExampleContains() {
    fmt.Println(bytes.Contains(slice<byte>("seafood"u8), slice<byte>("foo"u8)));
    fmt.Println(bytes.Contains(slice<byte>("seafood"u8), slice<byte>("bar"u8)));
    fmt.Println(bytes.Contains(slice<byte>("seafood"u8), slice<byte>(""u8)));
    fmt.Println(bytes.Contains(slice<byte>(""u8), slice<byte>(""u8)));
}

// Output:
// true
// false
// true
// true
public static void ExampleContainsAny() {
    fmt.Println(bytes.ContainsAny(slice<byte>("I like seafood."u8), "fÄo!"u8));
    fmt.Println(bytes.ContainsAny(slice<byte>("I like seafood."u8), "去是伟大的."u8));
    fmt.Println(bytes.ContainsAny(slice<byte>("I like seafood."u8), ""u8));
    fmt.Println(bytes.ContainsAny(slice<byte>(""u8), ""u8));
}

// Output:
// true
// true
// false
// false
public static void ExampleContainsRune() {
    fmt.Println(bytes.ContainsRune(slice<byte>("I like seafood."u8), (rune)'f'));
    fmt.Println(bytes.ContainsRune(slice<byte>("I like seafood."u8), (rune)'ö'));
    fmt.Println(bytes.ContainsRune(slice<byte>("去是伟大的!"u8), (rune)'大'));
    fmt.Println(bytes.ContainsRune(slice<byte>("去是伟大的!"u8), (rune)'!'));
    fmt.Println(bytes.ContainsRune(slice<byte>(""u8), (rune)'@'));
}

// Output:
// true
// false
// true
// true
// false
public static void ExampleContainsFunc() {
    var f = (rune r) => r >= (rune)'a' && r <= (rune)'z';
    fmt.Println(bytes.ContainsFunc(slice<byte>("HELLO"u8), f));
    fmt.Println(bytes.ContainsFunc(slice<byte>("World"u8), f));
}

// Output:
// false
// true
public static void ExampleCount() {
    fmt.Println(bytes.Count(slice<byte>("cheese"u8), slice<byte>("e"u8)));
    fmt.Println(bytes.Count(slice<byte>("five"u8), slice<byte>(""u8)));
}

// before & after each rune
// Output:
// 3
// 5
public static void ExampleCut() {
    var show = (@string s, @string sep) => {
        var (before, after, found) = bytes.Cut(slice<byte>(s), slice<byte>(sep));
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
        var (after, found) = bytes.CutPrefix(slice<byte>(s), slice<byte>(sep));
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
        var (before, found) = bytes.CutSuffix(slice<byte>(s), slice<byte>(sep));
        fmt.Printf("CutSuffix(%q, %q) = %q, %v\n"u8, s, sep, before, found);
    };
    show("Gopher"u8, "Go"u8);
    show("Gopher"u8, "er"u8);
}

// Output:
// CutSuffix("Gopher", "Go") = "Gopher", false
// CutSuffix("Gopher", "er") = "Goph", true
public static void ExampleEqual() {
    fmt.Println(bytes.Equal(slice<byte>("Go"u8), slice<byte>("Go"u8)));
    fmt.Println(bytes.Equal(slice<byte>("Go"u8), slice<byte>("C++"u8)));
}

// Output:
// true
// false
public static void ExampleEqualFold() {
    fmt.Println(bytes.EqualFold(slice<byte>("Go"u8), slice<byte>("go"u8)));
}

// Output: true
public static void ExampleFields() {
    fmt.Printf("Fields are: %q"u8, bytes.Fields(slice<byte>("  foo bar  baz   "u8)));
}

// Output: Fields are: ["foo" "bar" "baz"]
public static void ExampleFieldsFunc() {
    var f = (rune c) => !Δunicode.IsLetter(c) && !Δunicode.IsNumber(c);
    fmt.Printf("Fields are: %q"u8, bytes.FieldsFunc(slice<byte>("  foo1;bar2,baz3..."u8), f));
}

// Output: Fields are: ["foo1" "bar2" "baz3"]
public static void ExampleHasPrefix() {
    fmt.Println(bytes.HasPrefix(slice<byte>("Gopher"u8), slice<byte>("Go"u8)));
    fmt.Println(bytes.HasPrefix(slice<byte>("Gopher"u8), slice<byte>("C"u8)));
    fmt.Println(bytes.HasPrefix(slice<byte>("Gopher"u8), slice<byte>(""u8)));
}

// Output:
// true
// false
// true
public static void ExampleHasSuffix() {
    fmt.Println(bytes.HasSuffix(slice<byte>("Amigo"u8), slice<byte>("go"u8)));
    fmt.Println(bytes.HasSuffix(slice<byte>("Amigo"u8), slice<byte>("O"u8)));
    fmt.Println(bytes.HasSuffix(slice<byte>("Amigo"u8), slice<byte>("Ami"u8)));
    fmt.Println(bytes.HasSuffix(slice<byte>("Amigo"u8), slice<byte>(""u8)));
}

// Output:
// true
// false
// false
// true
public static void ExampleIndex() {
    fmt.Println(bytes.Index(slice<byte>("chicken"u8), slice<byte>("ken"u8)));
    fmt.Println(bytes.Index(slice<byte>("chicken"u8), slice<byte>("dmr"u8)));
}

// Output:
// 4
// -1
public static void ExampleIndexByte() {
    fmt.Println(bytes.IndexByte(slice<byte>("chicken"u8), (byte)(rune)'k'));
    fmt.Println(bytes.IndexByte(slice<byte>("chicken"u8), (byte)(rune)'g'));
}

// Output:
// 4
// -1
public static void ExampleIndexFunc() {
    var f = (rune c) => Δunicode.Is(Δunicode.Han, c);
    fmt.Println(bytes.IndexFunc(slice<byte>("Hello, 世界"u8), f));
    fmt.Println(bytes.IndexFunc(slice<byte>("Hello, world"u8), f));
}

// Output:
// 7
// -1
public static void ExampleIndexAny() {
    fmt.Println(bytes.IndexAny(slice<byte>("chicken"u8), "aeiouy"u8));
    fmt.Println(bytes.IndexAny(slice<byte>("crwth"u8), "aeiouy"u8));
}

// Output:
// 2
// -1
public static void ExampleIndexRune() {
    fmt.Println(bytes.IndexRune(slice<byte>("chicken"u8), (rune)'k'));
    fmt.Println(bytes.IndexRune(slice<byte>("chicken"u8), (rune)'d'));
}

// Output:
// 4
// -1
public static void ExampleJoin() {
    var s = new slice<byte>[]{slice<byte>("foo"u8), slice<byte>("bar"u8), slice<byte>("baz"u8)}.slice();
    fmt.Printf("%s"u8, bytes.Join(s, slice<byte>(", "u8)));
}

// Output: foo, bar, baz
public static void ExampleLastIndex() {
    fmt.Println(bytes.Index(slice<byte>("go gopher"u8), slice<byte>("go"u8)));
    fmt.Println(bytes.LastIndex(slice<byte>("go gopher"u8), slice<byte>("go"u8)));
    fmt.Println(bytes.LastIndex(slice<byte>("go gopher"u8), slice<byte>("rodent"u8)));
}

// Output:
// 0
// 3
// -1
public static void ExampleLastIndexAny() {
    fmt.Println(bytes.LastIndexAny(slice<byte>("go gopher"u8), "MüQp"u8));
    fmt.Println(bytes.LastIndexAny(slice<byte>("go 地鼠"u8), "地大"u8));
    fmt.Println(bytes.LastIndexAny(slice<byte>("go gopher"u8), "z,!."u8));
}

// Output:
// 5
// 3
// -1
public static void ExampleLastIndexByte() {
    fmt.Println(bytes.LastIndexByte(slice<byte>("go gopher"u8), (byte)(rune)'g'));
    fmt.Println(bytes.LastIndexByte(slice<byte>("go gopher"u8), (byte)(rune)'r'));
    fmt.Println(bytes.LastIndexByte(slice<byte>("go gopher"u8), (byte)(rune)'z'));
}

// Output:
// 3
// 8
// -1
public static void ExampleLastIndexFunc() {
    fmt.Println(bytes.LastIndexFunc(slice<byte>("go gopher!"u8), Δunicode.IsLetter));
    fmt.Println(bytes.LastIndexFunc(slice<byte>("go gopher!"u8), Δunicode.IsPunct));
    fmt.Println(bytes.LastIndexFunc(slice<byte>("go gopher!"u8), Δunicode.IsNumber));
}

// Output:
// 8
// 9
// -1
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
    fmt.Printf("%s\n"u8, bytes.Map(rot13, slice<byte>("'Twas brillig and the slithy gopher..."u8)));
}

// Output:
// 'Gjnf oevyyvt naq gur fyvgul tbcure...
public static void ExampleReader_Len() {
    fmt.Println(bytes.NewReader(slice<byte>("Hi!"u8)).Len());
    fmt.Println(bytes.NewReader(slice<byte>("こんにちは!"u8)).Len());
}

// Output:
// 3
// 16
public static void ExampleRepeat() {
    fmt.Printf("ba%s"u8, bytes.Repeat(slice<byte>("na"u8), 2));
}

// Output: banana
public static void ExampleReplace() {
    fmt.Printf("%s\n"u8, bytes.Replace(slice<byte>("oink oink oink"u8), slice<byte>("k"u8), slice<byte>("ky"u8), 2));
    fmt.Printf("%s\n"u8, bytes.Replace(slice<byte>("oink oink oink"u8), slice<byte>("oink"u8), slice<byte>("moo"u8), -1));
}

// Output:
// oinky oinky oink
// moo moo moo
public static void ExampleReplaceAll() {
    fmt.Printf("%s\n"u8, bytes.ReplaceAll(slice<byte>("oink oink oink"u8), slice<byte>("oink"u8), slice<byte>("moo"u8)));
}

// Output:
// moo moo moo
public static void ExampleRunes() {
    var rs = bytes.Runes(slice<byte>("go gopher"u8));
    foreach (var (_, r) in rs) {
        fmt.Printf("%#U\n"u8, r);
    }
}

// Output:
// U+0067 'g'
// U+006F 'o'
// U+0020 ' '
// U+0067 'g'
// U+006F 'o'
// U+0070 'p'
// U+0068 'h'
// U+0065 'e'
// U+0072 'r'
public static void ExampleSplit() {
    fmt.Printf("%q\n"u8, bytes.Split(slice<byte>("a,b,c"u8), slice<byte>(","u8)));
    fmt.Printf("%q\n"u8, bytes.Split(slice<byte>("a man a plan a canal panama"u8), slice<byte>("a "u8)));
    fmt.Printf("%q\n"u8, bytes.Split(slice<byte>(" xyz "u8), slice<byte>(""u8)));
    fmt.Printf("%q\n"u8, bytes.Split(slice<byte>(""u8), slice<byte>("Bernardo O'Higgins"u8)));
}

// Output:
// ["a" "b" "c"]
// ["" "man " "plan " "canal panama"]
// [" " "x" "y" "z" " "]
// [""]
public static void ExampleSplitN() {
    fmt.Printf("%q\n"u8, bytes.SplitN(slice<byte>("a,b,c"u8), slice<byte>(","u8), 2));
    var z = bytes.SplitN(slice<byte>("a,b,c"u8), slice<byte>(","u8), 0);
    fmt.Printf("%q (nil = %v)\n"u8, z, z == default!);
}

// Output:
// ["a" "b,c"]
// [] (nil = true)
public static void ExampleSplitAfter() {
    fmt.Printf("%q\n"u8, bytes.SplitAfter(slice<byte>("a,b,c"u8), slice<byte>(","u8)));
}

// Output: ["a," "b," "c"]
public static void ExampleSplitAfterN() {
    fmt.Printf("%q\n"u8, bytes.SplitAfterN(slice<byte>("a,b,c"u8), slice<byte>(","u8), 2));
}

// Output: ["a," "b,c"]
public static void ExampleTitle() {
    fmt.Printf("%s"u8, bytes.Title(slice<byte>("her royal highness"u8)));
}

// Output: Her Royal Highness
public static void ExampleToTitle() {
    fmt.Printf("%s\n"u8, bytes.ToTitle(slice<byte>("loud noises"u8)));
    fmt.Printf("%s\n"u8, bytes.ToTitle(slice<byte>("хлеб"u8)));
}

// Output:
// LOUD NOISES
// ХЛЕБ
public static void ExampleToTitleSpecial() {
    var str = slice<byte>("ahoj vývojári golang"u8);
    var totitle = bytes.ToTitleSpecial(Δunicode.AzeriCase, str);
    fmt.Println("Original : " + ((sstring)str));
    fmt.Println("ToTitle : " + ((sstring)totitle));
}

// Output:
// Original : ahoj vývojári golang
// ToTitle : AHOJ VÝVOJÁRİ GOLANG
public static void ExampleToValidUTF8() {
    fmt.Printf("%s\n"u8, bytes.ToValidUTF8(slice<byte>("abc"u8), slice<byte>("\uFFFD"u8)));
    fmt.Printf("%s\n"u8, bytes.ToValidUTF8(slice<byte>(((@string)(new byte[]{0x61, 0xff, 0x62, 0xc0, 0xaf, 0x63, 0xff}))), slice<byte>(""u8)));
    fmt.Printf("%s\n"u8, bytes.ToValidUTF8(slice<byte>(((@string)(new byte[]{0xed, 0xa0, 0x80}))), slice<byte>("abc"u8)));
}

// Output:
// abc
// abc
// abc
public static void ExampleTrim() {
    fmt.Printf("[%q]"u8, bytes.Trim(slice<byte>(" !!! Achtung! Achtung! !!! "u8), "! "u8));
}

// Output: ["Achtung! Achtung"]
public static void ExampleTrimFunc() {
    fmt.Println(((@string)bytes.TrimFunc(slice<byte>("go-gopher!"u8), Δunicode.IsLetter)));
    fmt.Println(((@string)bytes.TrimFunc(slice<byte>("\"go-gopher!\""u8), Δunicode.IsLetter)));
    fmt.Println(((@string)bytes.TrimFunc(slice<byte>("go-gopher!"u8), Δunicode.IsPunct)));
    fmt.Println(((@string)bytes.TrimFunc(slice<byte>("1234go-gopher!567"u8), Δunicode.IsNumber)));
}

// Output:
// -gopher!
// "go-gopher!"
// go-gopher
// go-gopher!
public static void ExampleTrimLeft() {
    fmt.Print(((@string)bytes.TrimLeft(slice<byte>("453gopher8257"u8), "0123456789"u8)));
}

// Output:
// gopher8257
public static void ExampleTrimLeftFunc() {
    fmt.Println(((@string)bytes.TrimLeftFunc(slice<byte>("go-gopher"u8), Δunicode.IsLetter)));
    fmt.Println(((@string)bytes.TrimLeftFunc(slice<byte>("go-gopher!"u8), Δunicode.IsPunct)));
    fmt.Println(((@string)bytes.TrimLeftFunc(slice<byte>("1234go-gopher!567"u8), Δunicode.IsNumber)));
}

// Output:
// -gopher
// go-gopher!
// go-gopher!567
public static void ExampleTrimPrefix() {
    slice<byte> b = slice<byte>("Goodbye,, world!"u8);
    b = bytes.TrimPrefix(b, slice<byte>("Goodbye,"u8));
    b = bytes.TrimPrefix(b, slice<byte>("See ya,"u8));
    fmt.Printf("Hello%s"u8, b);
}

// Output: Hello, world!
public static void ExampleTrimSpace() {
    fmt.Printf("%s"u8, bytes.TrimSpace(slice<byte>(" \t\n a lone gopher \n\t\r\n"u8)));
}

// Output: a lone gopher
public static void ExampleTrimSuffix() {
    slice<byte> b = slice<byte>("Hello, goodbye, etc!"u8);
    b = bytes.TrimSuffix(b, slice<byte>("goodbye, etc!"u8));
    b = bytes.TrimSuffix(b, slice<byte>("gopher"u8));
    b = append(b, bytes.TrimSuffix(slice<byte>("world!"u8), slice<byte>("x!"u8)).ꓸꓸꓸ);
    Δos.Stdout.Write(b);
}

// Output: Hello, world!
public static void ExampleTrimRight() {
    fmt.Print(((@string)bytes.TrimRight(slice<byte>("453gopher8257"u8), "0123456789"u8)));
}

// Output:
// 453gopher
public static void ExampleTrimRightFunc() {
    fmt.Println(((@string)bytes.TrimRightFunc(slice<byte>("go-gopher"u8), Δunicode.IsLetter)));
    fmt.Println(((@string)bytes.TrimRightFunc(slice<byte>("go-gopher!"u8), Δunicode.IsPunct)));
    fmt.Println(((@string)bytes.TrimRightFunc(slice<byte>("1234go-gopher!567"u8), Δunicode.IsNumber)));
}

// Output:
// go-
// go-gopher
// 1234go-gopher!
public static void ExampleToLower() {
    fmt.Printf("%s"u8, bytes.ToLower(slice<byte>("Gopher"u8)));
}

// Output: gopher
public static void ExampleToLowerSpecial() {
    var str = slice<byte>("AHOJ VÝVOJÁRİ GOLANG"u8);
    var totitle = bytes.ToLowerSpecial(Δunicode.AzeriCase, str);
    fmt.Println("Original : " + ((sstring)str));
    fmt.Println("ToLower : " + ((sstring)totitle));
}

// Output:
// Original : AHOJ VÝVOJÁRİ GOLANG
// ToLower : ahoj vývojári golang
public static void ExampleToUpper() {
    fmt.Printf("%s"u8, bytes.ToUpper(slice<byte>("Gopher"u8)));
}

// Output: GOPHER
public static void ExampleToUpperSpecial() {
    var str = slice<byte>("ahoj vývojári golang"u8);
    var totitle = bytes.ToUpperSpecial(Δunicode.AzeriCase, str);
    fmt.Println("Original : " + ((sstring)str));
    fmt.Println("ToUpper : " + ((sstring)totitle));
}

// Output:
// Original : ahoj vývojári golang
// ToUpper : AHOJ VÝVOJÁRİ GOLANG

} // end bytes_test_package
