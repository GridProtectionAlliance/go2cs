// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static go.bytes_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δmath = math_package;
using rand = go.math.rand_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using Δunicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using @unsafe = unsafe_package;
using @internal;
using bytes = bytes_package;
using go.math;
using go.unicode;

partial class bytes_test_package {

internal static bool eq(slice<@string> a, slice<@string> b) {
    if (len(a) != len(b)) {
        return false;
    }
    for (nint i = 0; i < len(a); i++) {
        if (a[i] != b[i]) {
            return false;
        }
    }
    return true;
}

internal static slice<@string> sliceOfString(slice<slice<byte>> s) {
    var result = new slice<@string>(len(s));
    foreach (var (i, v) in s) {
        result[i] = ((@string)v);
    }
    return result;
}

// For ease of reading, the test cases use strings that are converted to byte
// slices before invoking the functions.
internal static @string abcd = "abcd"u8;

internal static @string faces = "☺☻☹"u8;

internal static @string commas = "1,2,3,4"u8;

internal static @string dots = "1....2....3....4"u8;

[GoType] partial struct BinOpTest {
    internal @string a;
    internal @string b;
    internal nint i;
}

public static void TestEqual(ж<testing.T> Ꮡt) {
    // Run the tests and check for allocation at the same time.
    var allocs = testing.AllocsPerRun(10, () => {
        foreach (var (_, vᴛ1) in compareTests) {
            ref var tt = ref heap(new compareTestsᴛ1(), out var Ꮡtt);
            tt = vᴛ1;

            var eql = Equal(tt.a, tt.b);
            if (eql != (tt.i == 0)) {
                Ꮡt.Errorf(@"Equal(%q, %q) = %v"u8, tt.a, tt.b, eql);
            }
        }
    });
    if (allocs > 0) {
        Ꮡt.Errorf("Equal allocated %v times"u8, allocs);
    }
}

public static void TestEqualExhaustive(ж<testing.T> Ꮡt) {
    nint size = 128;
    if (testing.Short()) {
        size = 32;
    }
    var a = new slice<byte>(size);
    var b = new slice<byte>(size);
    var b_init = new slice<byte>(size);
    // randomish but deterministic data
    for (nint i = 0; i < size; i++) {
        a[i] = (byte)(17 * i);
        b_init[i] = (byte)(23 * i + 100);
    }
    for (nint len = 0; len <= size; len++) {
        for (nint x = 0; x <= size - len; x++) {
            for (nint y = 0; y <= size - len; y++) {
                copy(b, b_init);
                copy(b[(int)(y)..(int)(y + len)], a[(int)(x)..(int)(x + len)]);
                if (!Equal(a[(int)(x)..(int)(x + len)], b[(int)(y)..(int)(y + len)]) || !Equal(b[(int)(y)..(int)(y + len)], a[(int)(x)..(int)(x + len)])) {
                    Ꮡt.Errorf("Equal(%d, %d, %d) = false"u8, len, x, y);
                }
            }
        }
    }
}

// make sure Equal returns false for minimally different strings. The data
// is all zeros except for a single one in one location.
public static void TestNotEqual(ж<testing.T> Ꮡt) {
    nint size = 128;
    if (testing.Short()) {
        size = 32;
    }
    var a = new slice<byte>(size);
    var b = new slice<byte>(size);
    for (nint len = 0; len <= size; len++) {
        for (nint x = 0; x <= size - len; x++) {
            for (nint y = 0; y <= size - len; y++) {
                for (nint diffpos = x; diffpos < x + len; diffpos++) {
                    a[diffpos] = 1;
                    if (Equal(a[(int)(x)..(int)(x + len)], b[(int)(y)..(int)(y + len)]) || Equal(b[(int)(y)..(int)(y + len)], a[(int)(x)..(int)(x + len)])) {
                        Ꮡt.Errorf("NotEqual(%d, %d, %d, %d) = true"u8, len, x, y, diffpos);
                    }
                    a[diffpos] = 0;
                }
            }
        }
    }
}

// cases with one byte strings - test IndexByte and special case in Index()
// test fallback to Rabin-Karp.
internal static slice<BinOpTest> indexTests = new BinOpTest[]{
    new(""u8, ""u8, 0),
    new(""u8, "a"u8, -1),
    new(""u8, "foo"u8, -1),
    new("fo"u8, "foo"u8, -1),
    new("foo"u8, "baz"u8, -1),
    new("foo"u8, "foo"u8, 0),
    new("oofofoofooo"u8, "f"u8, 2),
    new("oofofoofooo"u8, "foo"u8, 4),
    new("barfoobarfoo"u8, "foo"u8, 3),
    new("foo"u8, ""u8, 0),
    new("foo"u8, "o"u8, 1),
    new("abcABCabc"u8, "A"u8, 3),
    new(""u8, "a"u8, -1),
    new("x"u8, "a"u8, -1),
    new("x"u8, "x"u8, 0),
    new("abc"u8, "a"u8, 0),
    new("abc"u8, "b"u8, 1),
    new("abc"u8, "c"u8, 2),
    new("abc"u8, "x"u8, -1),
    new("barfoobarfooyyyzzzyyyzzzyyyzzzyyyxxxzzzyyy"u8, "x"u8, 33),
    new("fofofofooofoboo"u8, "oo"u8, 7),
    new("fofofofofofoboo"u8, "ob"u8, 11),
    new("fofofofofofoboo"u8, "boo"u8, 12),
    new("fofofofofofoboo"u8, "oboo"u8, 11),
    new("fofofofofoooboo"u8, "fooo"u8, 8),
    new("fofofofofofoboo"u8, "foboo"u8, 10),
    new("fofofofofofoboo"u8, "fofob"u8, 8),
    new("fofofofofofofoffofoobarfoo"u8, "foffof"u8, 12),
    new("fofofofofoofofoffofoobarfoo"u8, "foffof"u8, 13),
    new("fofofofofofofoffofoobarfoo"u8, "foffofo"u8, 12),
    new("fofofofofoofofoffofoobarfoo"u8, "foffofo"u8, 13),
    new("fofofofofoofofoffofoobarfoo"u8, "foffofoo"u8, 13),
    new("fofofofofofofoffofoobarfoo"u8, "foffofoo"u8, 12),
    new("fofofofofoofofoffofoobarfoo"u8, "foffofoob"u8, 13),
    new("fofofofofofofoffofoobarfoo"u8, "foffofoob"u8, 12),
    new("fofofofofoofofoffofoobarfoo"u8, "foffofooba"u8, 13),
    new("fofofofofofofoffofoobarfoo"u8, "foffofooba"u8, 12),
    new("fofofofofoofofoffofoobarfoo"u8, "foffofoobar"u8, 13),
    new("fofofofofofofoffofoobarfoo"u8, "foffofoobar"u8, 12),
    new("fofofofofoofofoffofoobarfoo"u8, "foffofoobarf"u8, 13),
    new("fofofofofofofoffofoobarfoo"u8, "foffofoobarf"u8, 12),
    new("fofofofofoofofoffofoobarfoo"u8, "foffofoobarfo"u8, 13),
    new("fofofofofofofoffofoobarfoo"u8, "foffofoobarfo"u8, 12),
    new("fofofofofoofofoffofoobarfoo"u8, "foffofoobarfoo"u8, 13),
    new("fofofofofofofoffofoobarfoo"u8, "foffofoobarfoo"u8, 12),
    new("fofofofofoofofoffofoobarfoo"u8, "ofoffofoobarfoo"u8, 12),
    new("fofofofofofofoffofoobarfoo"u8, "ofoffofoobarfoo"u8, 11),
    new("fofofofofoofofoffofoobarfoo"u8, "fofoffofoobarfoo"u8, 11),
    new("fofofofofofofoffofoobarfoo"u8, "fofoffofoobarfoo"u8, 10),
    new("fofofofofoofofoffofoobarfoo"u8, "foobars"u8, -1),
    new("foofyfoobarfoobar"u8, "y"u8, 4),
    new("oooooooooooooooooooooo"u8, "r"u8, -1),
    new("oxoxoxoxoxoxoxoxoxoxoxoy"u8, "oy"u8, 22),
    new("oxoxoxoxoxoxoxoxoxoxoxox"u8, "oy"u8, -1),
    new("000000000000000000000000000000000000000000000000000000000000000000000001"u8, "0000000000000000000000000000000000000000000000000000000000000000001"u8, 5)
}.slice();

internal static slice<BinOpTest> lastIndexTests = new BinOpTest[]{
    new(""u8, ""u8, 0),
    new(""u8, "a"u8, -1),
    new(""u8, "foo"u8, -1),
    new("fo"u8, "foo"u8, -1),
    new("foo"u8, "foo"u8, 0),
    new("foo"u8, "f"u8, 0),
    new("oofofoofooo"u8, "f"u8, 7),
    new("oofofoofooo"u8, "foo"u8, 7),
    new("barfoobarfoo"u8, "foo"u8, 9),
    new("foo"u8, ""u8, 3),
    new("foo"u8, "o"u8, 2),
    new("abcABCabc"u8, "A"u8, 3),
    new("abcABCabc"u8, "a"u8, 6)
}.slice();

internal static slice<BinOpTest> indexAnyTests = new BinOpTest[]{
    new(""u8, ""u8, -1),
    new(""u8, "a"u8, -1),
    new(""u8, "abc"u8, -1),
    new("a"u8, ""u8, -1),
    new("a"u8, "a"u8, 0),
    new(((@string)(new byte[]{0x80})), ((@string)(new byte[]{0xff, 0x62})), 0),
    new("aaa"u8, "a"u8, 0),
    new("abc"u8, "xyz"u8, -1),
    new("abc"u8, "xcz"u8, 2),
    new("ab☺c"u8, "x☺yz"u8, 2),
    new("a☺b☻c☹d"u8, "cx"u8, len("a☺b☻")),
    new("a☺b☻c☹d"u8, "uvw☻xyz"u8, len("a☺b")),
    new("aRegExp*"u8, ".(|)*+?^$[]"u8, 7),
    new(dots + dots + dots, " "u8, -1),
    new("012abcba210"u8, ((@string)(new byte[]{0xff, 0x62})), 4),
    new(((@string)(new byte[]{0x30, 0x31, 0x32, 0x80, 0x62, 0x63, 0x62, 0x80, 0x32, 0x31, 0x30})), ((@string)(new byte[]{0xff, 0x62})), 3),
    new(((@string)(new byte[]{0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0xcf, 0x80, 0x61, 0x62, 0x63})), ((@string)(new byte[]{0xcf, 0x62, 0x80})), 10)
}.slice();

internal static slice<BinOpTest> lastIndexAnyTests = new BinOpTest[]{
    new(""u8, ""u8, -1),
    new(""u8, "a"u8, -1),
    new(""u8, "abc"u8, -1),
    new("a"u8, ""u8, -1),
    new("a"u8, "a"u8, 0),
    new(((@string)(new byte[]{0x80})), ((@string)(new byte[]{0xff, 0x62})), 0),
    new("aaa"u8, "a"u8, 2),
    new("abc"u8, "xyz"u8, -1),
    new("abc"u8, "ab"u8, 1),
    new("ab☺c"u8, "x☺yz"u8, 2),
    new("a☺b☻c☹d"u8, "cx"u8, len("a☺b☻")),
    new("a☺b☻c☹d"u8, "uvw☻xyz"u8, len("a☺b")),
    new("a.RegExp*"u8, ".(|)*+?^$[]"u8, 8),
    new(dots + dots + dots, " "u8, -1),
    new("012abcba210"u8, ((@string)(new byte[]{0xff, 0x62})), 6),
    new(((@string)(new byte[]{0x30, 0x31, 0x32, 0x80, 0x62, 0x63, 0x62, 0x80, 0x32, 0x31, 0x30})), ((@string)(new byte[]{0xff, 0x62})), 7),
    new(((@string)(new byte[]{0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0xcf, 0x80, 0x61, 0x62, 0x63})), ((@string)(new byte[]{0xcf, 0x62, 0x80})), 10)
}.slice();

[GoType("dyn")] partial struct runIndexTests_type {
    internal slice<byte> a;
    internal slice<byte> b;
    internal nint i;
}

// Execute f on each test case.  funcName should be the name of f; it's used
// in failure reports.
internal static void runIndexTests(ж<testing.T> Ꮡt, Func<slice<byte>, slice<byte>, nint> f, @string funcName, slice<BinOpTest> testCases) {
    foreach (var (_, test) in testCases) {
        var a = slice<byte>(test.a);
        var b = slice<byte>(test.b);
        nint actual = f(a, b);
        if (actual != test.i) {
            Ꮡt.Errorf("%s(%q,%q) = %v; want %v"u8, funcName, a, b, actual, test.i);
        }
    }
// case for function Index.
// case for function LastIndex.
    slice<runIndexTests_type> allocTests = new runIndexTests_type[]{
        new(slice<byte>("000000000000000000000000000000000000000000000000000000000000000000000001"u8), slice<byte>("0000000000000000000000000000000000000000000000000000000000000000001"u8), 5),
        new(slice<byte>("000000000000000000000000000000000000000000000000000000000000000010000"u8), slice<byte>("00000000000000000000000000000000000000000000000000000000000001"u8), 3)
    }.slice();
    var allocTestsʗ1 = allocTests;
    var allocs = testing.AllocsPerRun(100, () => {
        {
            nint i = Index(allocTestsʗ1[1].a, allocTestsʗ1[1].b); if (i != allocTestsʗ1[1].i) {
                Ꮡt.Errorf("Index([]byte(%q), []byte(%q)) = %v; want %v"u8, allocTestsʗ1[1].a, allocTestsʗ1[1].b, i, allocTestsʗ1[1].i);
            }
        }
        {
            nint i = LastIndex(allocTestsʗ1[0].a, allocTestsʗ1[0].b); if (i != allocTestsʗ1[0].i) {
                Ꮡt.Errorf("LastIndex([]byte(%q), []byte(%q)) = %v; want %v"u8, allocTestsʗ1[0].a, allocTestsʗ1[0].b, i, allocTestsʗ1[0].i);
            }
        }
    });
    if (allocs != 0) {
        Ꮡt.Errorf("expected no allocations, got %f"u8, allocs);
    }
}

internal static void runIndexAnyTests(ж<testing.T> Ꮡt, Func<slice<byte>, @string, nint> f, @string funcName, slice<BinOpTest> testCases) {
    foreach (var (_, test) in testCases) {
        var a = slice<byte>(test.a);
        nint actual = f(a, test.b);
        if (actual != test.i) {
            Ꮡt.Errorf("%s(%q,%q) = %v; want %v"u8, funcName, a, test.b, actual, test.i);
        }
    }
}

public static void TestIndex(ж<testing.T> Ꮡt) {
    runIndexTests(Ꮡt, Index, "Index"u8, indexTests);
}

public static void TestLastIndex(ж<testing.T> Ꮡt) {
    runIndexTests(Ꮡt, LastIndex, "LastIndex"u8, lastIndexTests);
}

public static void TestIndexAny(ж<testing.T> Ꮡt) {
    runIndexAnyTests(Ꮡt, IndexAny, "IndexAny"u8, indexAnyTests);
}

public static void TestLastIndexAny(ж<testing.T> Ꮡt) {
    runIndexAnyTests(Ꮡt, LastIndexAny, "LastIndexAny"u8, lastIndexAnyTests);
}

public static void TestIndexByte(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in indexTests) {
        if (len(tt.b) != 1) {
            continue;
        }
        var a = slice<byte>(tt.a);
        var b = tt.b[0];
        nint pos = IndexByte(a, b);
        if (pos != tt.i) {
            Ꮡt.Errorf(@"IndexByte(%q, '%c') = %v"u8, tt.a, b, pos);
        }
        nint posp = IndexBytePortable(a, b);
        if (posp != tt.i) {
            Ꮡt.Errorf(@"indexBytePortable(%q, '%c') = %v"u8, tt.a, b, posp);
        }
    }
}

public static void TestLastIndexByte(ж<testing.T> Ꮡt) {
    var testCases = new BinOpTest[]{
        new(""u8, "q"u8, -1),
        new("abcdef"u8, "q"u8, -1),
        new("abcdefabcdef"u8, "a"u8, len("abcdef")), // something in the middle

        new("abcdefabcdef"u8, "f"u8, len("abcdefabcde")), // last byte

        new("zabcdefabcdef"u8, "z"u8, 0), // first byte

        new("a☺b☻c☹d"u8, "b"u8, len("a☺"))
    }.slice();
    // non-ascii
    foreach (var (_, test) in testCases) {
        nint actual = LastIndexByte(slice<byte>(test.a), test.b[0]);
        if (actual != test.i) {
            Ꮡt.Errorf("LastIndexByte(%q,%c) = %v; want %v"u8, test.a, test.b[0], actual, test.i);
        }
    }
}

// test a larger buffer with different sizes and alignments
public static void TestIndexByteBig(ж<testing.T> Ꮡt) {
    nint n = 1024;
    if (testing.Short()) {
        n = 128;
    }
    var b = new slice<byte>(n);
    for (nint i = 0; i < n; i++) {
        // different start alignments
        var b1 = b[(int)(i)..];
        for (nint j = 0; j < len(b1); j++) {
            b1[j] = (rune)'x';
            nint pos = IndexByte(b1, (rune)'x');
            if (pos != j) {
                Ꮡt.Errorf("IndexByte(%q, 'x') = %v"u8, b1, pos);
            }
            b1[j] = 0;
            pos = IndexByte(b1, (rune)'x');
            if (pos != -1) {
                Ꮡt.Errorf("IndexByte(%q, 'x') = %v"u8, b1, pos);
            }
        }
        // different end alignments
        b1 = b[..(int)(i)];
        for (nint j = 0; j < len(b1); j++) {
            b1[j] = (rune)'x';
            nint pos = IndexByte(b1, (rune)'x');
            if (pos != j) {
                Ꮡt.Errorf("IndexByte(%q, 'x') = %v"u8, b1, pos);
            }
            b1[j] = 0;
            pos = IndexByte(b1, (rune)'x');
            if (pos != -1) {
                Ꮡt.Errorf("IndexByte(%q, 'x') = %v"u8, b1, pos);
            }
        }
        // different start and end alignments
        b1 = b[(int)(i / 2)..(int)(n - (i + 1) / 2)];
        for (nint j = 0; j < len(b1); j++) {
            b1[j] = (rune)'x';
            nint pos = IndexByte(b1, (rune)'x');
            if (pos != j) {
                Ꮡt.Errorf("IndexByte(%q, 'x') = %v"u8, b1, pos);
            }
            b1[j] = 0;
            pos = IndexByte(b1, (rune)'x');
            if (pos != -1) {
                Ꮡt.Errorf("IndexByte(%q, 'x') = %v"u8, b1, pos);
            }
        }
    }
}

// test a small index across all page offsets
public static void TestIndexByteSmall(ж<testing.T> Ꮡt) {
    var b = new slice<byte>(5015);
    // bigger than a page
    // Make sure we find the correct byte even when straddling a page.
    for (nint i = 0; i <= len(b) - 15; i++) {
        for (nint j = 0; j < 15; j++) {
            b[i + j] = (byte)(100 + j);
        }
        for (nint j = 0; j < 15; j++) {
            nint p = IndexByte(b[(int)(i)..(int)(i + 15)], (byte)(100 + j));
            if (p != j) {
                Ꮡt.Errorf("IndexByte(%q, %d) = %d"u8, b[(int)(i)..(int)(i + 15)], 100 + j, p);
            }
        }
        for (nint j = 0; j < 15; j++) {
            b[i + j] = 0;
        }
    }
    // Make sure matches outside the slice never trigger.
    for (nint i = 0; i <= len(b) - 15; i++) {
        for (nint j = 0; j < 15; j++) {
            b[i + j] = 1;
        }
        for (nint j = 0; j < 15; j++) {
            nint p = IndexByte(b[(int)(i)..(int)(i + 15)], (byte)0);
            if (p != -1) {
                Ꮡt.Errorf("IndexByte(%q, %d) = %d"u8, b[(int)(i)..(int)(i + 15)], 0, p);
            }
        }
        for (nint j = 0; j < 15; j++) {
            b[i + j] = 0;
        }
    }
}

[GoType("dyn")] partial struct TestIndexRune_tests {
    internal @string @in;
    internal rune rune;
    internal nint want;
}

public static void TestIndexRune(ж<testing.T> Ꮡt) {
    var tests = new TestIndexRune_tests[]{
        new(""u8, (rune)'a', -1),
        new(""u8, (rune)'☺', -1),
        new("foo"u8, (rune)'☹', -1),
        new("foo"u8, (rune)'o', 1),
        new("foo☺bar"u8, (rune)'☺', 3),
        new("foo☺☻☹bar"u8, (rune)'☹', 9),
        new("a A x"u8, (rune)'A', 2),
        new("some_text=some_value"u8, (rune)'=', 9),
        new("☺a"u8, (rune)'a', 3),
        new("a☻☺b"u8, (rune)'☺', 4), // RuneError should match any invalid UTF-8 byte sequence.

        new("�"u8, (rune)'�', 0),
        new(((@string)(new byte[]{0xff})), (rune)'�', 0),
        new("☻x�"u8, (rune)'�', len("☻x")),
        new(((@string)(new byte[]{0xe2, 0x98, 0xbb, 0x78, 0xe2, 0x98})), (rune)'�', len("☻x")),
        new(((@string)(new byte[]{0xe2, 0x98, 0xbb, 0x78, 0xe2, 0x98, 0xef, 0xbf, 0xbd})), (rune)'�', len("☻x")),
        new(((@string)(new byte[]{0xe2, 0x98, 0xbb, 0x78, 0xe2, 0x98, 0x78})), (rune)'�', len("☻x")), // Invalid rune values should never match.

        new(((@string)(new byte[]{0x61, 0xe2, 0x98, 0xba, 0x62, 0xe2, 0x98, 0xbb, 0x63, 0xe2, 0x98, 0xb9, 0x64, 0xe2, 0x98, 0xef, 0xbf, 0xbd, 0xff, 0xef, 0xbf, 0xbd, 0xed, 0xa0, 0x80})), -1, -1),
        new(((@string)(new byte[]{0x61, 0xe2, 0x98, 0xba, 0x62, 0xe2, 0x98, 0xbb, 0x63, 0xe2, 0x98, 0xb9, 0x64, 0xe2, 0x98, 0xef, 0xbf, 0xbd, 0xff, 0xef, 0xbf, 0xbd, 0xed, 0xa0, 0x80})), 0xD800, -1), // Surrogate pair

        new(((@string)(new byte[]{0x61, 0xe2, 0x98, 0xba, 0x62, 0xe2, 0x98, 0xbb, 0x63, 0xe2, 0x98, 0xb9, 0x64, 0xe2, 0x98, 0xef, 0xbf, 0xbd, 0xff, 0xef, 0xbf, 0xbd, 0xed, 0xa0, 0x80})), utf8.MaxRune + 1, -1)
    }.slice();
    foreach (var (_, tt) in tests) {
        {
            nint got = IndexRune(slice<byte>(tt.@in), tt.rune); if (got != tt.want) {
                Ꮡt.Errorf("IndexRune(%q, %d) = %v; want %v"u8, tt.@in, tt.rune, got, tt.want);
            }
        }
    }
    var haystack = slice<byte>("test世界"u8);
    var haystackʗ1 = haystack;
    var allocs = testing.AllocsPerRun(1000, () => {
        {
            nint i = IndexRune(haystackʗ1, (rune)'s'); if (i != 2) {
                Ꮡt.Fatalf("'s' at %d; want 2"u8, i);
            }
        }
        {
            nint i = IndexRune(haystackʗ1, (rune)'世'); if (i != 4) {
                Ꮡt.Fatalf("'世' at %d; want 4"u8, i);
            }
        }
    });
    if (allocs != 0) {
        Ꮡt.Errorf("expected no allocations, got %f"u8, allocs);
    }
}

// test count of a single byte across page offsets
public static void TestCountByte(ж<testing.T> Ꮡt) {
    var b = new slice<byte>(5015);
    // bigger than a page
    var windows = new nint[]{1, 2, 3, 4, 15, 16, 17, 31, 32, 33, 63, 64, 65, 128}.slice();
    var bʗ1 = b;
    var testCountWindow = (nint i, nint window) => {
        for (nint j = 0; j < window; j++) {
            bʗ1[i + j] = (byte)100;
            nint p = Count(bʗ1[(int)(i)..(int)(i + window)], new byte[]{100}.slice());
            if (p != j + 1) {
                Ꮡt.Errorf("TestCountByte.Count(%q, 100) = %d"u8, bʗ1[(int)(i)..(int)(i + window)], p);
            }
        }
    };
    nint maxWnd = windows[len(windows) - 1];
    for (nint i = 0; i <= 2 * maxWnd; i++) {
        foreach (var (_, vᴛ1) in windows) {
            var window = vᴛ1;

            if (window > len(b[(int)(i)..])) {
                window = len(b[(int)(i)..]);
            }
            testCountWindow(i, window);
            for (nint j = 0; j < window; j++) {
                b[i + j] = (byte)0;
            }
        }
    }
    for (nint i = 4096 - (maxWnd + 1); i < len(b); i++) {
        foreach (var (_, vᴛ2) in windows) {
            var window = vᴛ2;

            if (window > len(b[(int)(i)..])) {
                window = len(b[(int)(i)..]);
            }
            testCountWindow(i, window);
            for (nint j = 0; j < window; j++) {
                b[i + j] = (byte)0;
            }
        }
    }
}

// Make sure we don't count bytes outside our window
public static void TestCountByteNoMatch(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    var b = new slice<byte>(5015);
    var windows = new nint[]{1, 2, 3, 4, 15, 16, 17, 31, 32, 33, 63, 64, 65, 128}.slice();
    for (nint i = 0; i <= len(b); i++) {
        foreach (var (_, vᴛ1) in windows) {
            var window = vᴛ1;

            if (window > len(b[(int)(i)..])) {
                window = len(b[(int)(i)..]);
            }
            // Fill the window with non-match
            for (nint j = 0; j < window; j++) {
                b[i + j] = (byte)100;
            }
            // Try to find something that doesn't exist
            nint p = Count(b[(int)(i)..(int)(i + window)], new byte[]{0}.slice());
            if (p != 0) {
                Ꮡt.Errorf("TestCountByteNoMatch(%q, 0) = %d"u8, b[(int)(i)..(int)(i + window)], p);
            }
            for (nint j = 0; j < window; j++) {
                b[i + j] = (byte)0;
            }
        }
    }
}

internal static slice<byte> bmbuf;

internal static @string valName(nint x) {
    {
        nint s = (x >> (int)(20)); if ((s << (int)(20)) == x) {
            return fmt.Sprintf("%dM"u8, s);
        }
    }
    {
        nint s = (x >> (int)(10)); if ((s << (int)(10)) == x) {
            return fmt.Sprintf("%dK"u8, s);
        }
    }
    return fmt.Sprint(x);
}

internal static void benchBytes(ж<testing.B> Ꮡb, slice<nint> sizes, Action<ж<testing.B>, nint> f) {
    foreach (var (_, n) in sizes) {
        if (isRaceBuilder && n > (4 << (int)(10))) {
            continue;
        }
        Ꮡb.Run(valName(n), (ж<testing.B> bΔ1) => {
            if (len(bmbuf) < n) {
                bmbuf = new slice<byte>(n);
            }
            bΔ1.SetBytes((int64)n);
            f(bΔ1, n);
        });
    }
}

internal static slice<nint> indexSizes = new nint[]{10, 32, (4 << (int)(10)), (4 << (int)(20)), (64 << (int)(20))}.slice();

internal static bool isRaceBuilder = strings.HasSuffix(testenv.Builder(), "-race"u8);

public static void BenchmarkIndexByte(ж<testing.B> Ꮡb) {
    benchBytes(Ꮡb, indexSizes, bmIndexByte(IndexByte));
}

public static void BenchmarkIndexBytePortable(ж<testing.B> Ꮡb) {
    benchBytes(Ꮡb, indexSizes, bmIndexByte(IndexBytePortable));
}

internal static Action<ж<testing.B>, nint> bmIndexByte(Func<slice<byte>, byte, nint> index) {
    return (ж<testing.B> b, nint n) => {
        var buf = bmbuf[0..(int)(n)];
        buf[n - 1] = (rune)'x';
        for (nint i = 0; i < (~b).N; i++) {
            nint j = index(buf, (rune)'x');
            if (j != n - 1) {
                b.Fatal("bad index", j);
            }
        }
        buf[n - 1] = (rune)'\x00';
    };
}

public static void BenchmarkIndexRune(ж<testing.B> Ꮡb) {
    benchBytes(Ꮡb, indexSizes, bmIndexRune(IndexRune));
}

public static void BenchmarkIndexRuneASCII(ж<testing.B> Ꮡb) {
    benchBytes(Ꮡb, indexSizes, bmIndexRuneASCII(IndexRune));
}

internal static Action<ж<testing.B>, nint> bmIndexRuneASCII(Func<slice<byte>, rune, nint> index) {
    return (ж<testing.B> b, nint n) => {
        var buf = bmbuf[0..(int)(n)];
        buf[n - 1] = (rune)'x';
        for (nint i = 0; i < (~b).N; i++) {
            nint j = index(buf, (rune)'x');
            if (j != n - 1) {
                b.Fatal("bad index", j);
            }
        }
        buf[n - 1] = (rune)'\x00';
    };
}

internal static Action<ж<testing.B>, nint> bmIndexRune(Func<slice<byte>, rune, nint> index) {
    return (ж<testing.B> b, nint n) => {
        var buf = bmbuf[0..(int)(n)];
        utf8.EncodeRune(buf[(int)(n - 3)..], (rune)'世');
        for (nint i = 0; i < (~b).N; i++) {
            nint j = index(buf, (rune)'世');
            if (j != n - 3) {
                b.Fatal("bad index", j);
            }
        }
        buf[n - 3] = (rune)'\x00';
        buf[n - 2] = (rune)'\x00';
        buf[n - 1] = (rune)'\x00';
    };
}

public static void BenchmarkEqual(ж<testing.B> Ꮡb) {
    Ꮡb.Run("0"u8, (ж<testing.B> bΔ1) => {
        ref var buf = ref heap(new array<byte>(4), out var Ꮡbuf);
        var buf1 = buf[0..0];
        var buf2 = buf[1..1];
        for (nint i = 0; i < (~bΔ1).N; i++) {
            var eq = Equal(buf1, buf2);
            if (!eq) {
                bΔ1.Fatal("bad equal");
            }
        }
    });
    var sizes = new nint[]{1, 6, 9, 15, 16, 20, 32, (4 << (int)(10)), (4 << (int)(20)), (64 << (int)(20))}.slice();
    var sizesʗ1 = sizes;
    Ꮡb.Run("same"u8, (ж<testing.B> bΔ2) => {
        benchBytes(bΔ2, sizesʗ1, bmEqual((slice<byte> a, slice<byte> bΔ3) => Equal(a, a)));
    });
    benchBytes(Ꮡb, sizes, bmEqual(Equal));
}

internal static Action<ж<testing.B>, nint> bmEqual(Func<slice<byte>, slice<byte>, bool> equal) {
    return (ж<testing.B> b, nint n) => {
        if (len(bmbuf) < 2 * n) {
            bmbuf = new slice<byte>(2 * n);
        }
        var buf1 = bmbuf[0..(int)(n)];
        var buf2 = bmbuf[(int)(n)..(int)(2 * n)];
        buf1[n - 1] = (rune)'x';
        buf2[n - 1] = (rune)'x';
        for (nint i = 0; i < (~b).N; i++) {
            var eq = equal(buf1, buf2);
            if (!eq) {
                b.Fatal("bad equal");
            }
        }
        buf1[n - 1] = (rune)'\x00';
        buf2[n - 1] = (rune)'\x00';
    };
}

public static void BenchmarkEqualBothUnaligned(ж<testing.B> Ꮡb) {
    var sizes = new nint[]{64, (4 << (int)(10))}.slice();
    if (!isRaceBuilder) {
        sizes = append(sizes, new nint[]{(4 << (int)(20)), (64 << (int)(20))}.slice().ꓸꓸꓸ);
    }
    nint maxSize = 2 * (sizes[len(sizes) - 1] + 8);
    if (len(bmbuf) < maxSize) {
        bmbuf = new slice<byte>(maxSize);
    }
    foreach (var (_, n) in sizes) {
        foreach (var (_, off) in new nint[]{0, 1, 4, 7}.slice()) {
            var buf1 = bmbuf[(int)(off)..(int)(off + n)];
            nint buf2Start = (len(bmbuf) / 2) + off;
            var buf2 = bmbuf[(int)(buf2Start)..(int)(buf2Start + n)];
            buf1[n - 1] = (rune)'x';
            buf2[n - 1] = (rune)'x';
            var buf1ʗ1 = buf1;
            var buf2ʗ1 = buf2;
            Ꮡb.Run(fmt.Sprint(n, off), (ж<testing.B> bΔ1) => {
                bΔ1.SetBytes((int64)n);
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    var eq = Equal(buf1ʗ1, buf2ʗ1);
                    if (!eq) {
                        bΔ1.Fatal("bad equal");
                    }
                }
            });
            buf1[n - 1] = (rune)'\x00';
            buf2[n - 1] = (rune)'\x00';
        }
    }
}

public static void BenchmarkIndex(ж<testing.B> Ꮡb) {
    benchBytes(Ꮡb, indexSizes, (ж<testing.B> bΔ1, nint n) => {
        var buf = bmbuf[0..(int)(n)];
        buf[n - 1] = (rune)'x';
        for (nint i = 0; i < (~bΔ1).N; i++) {
            nint j = Index(buf, buf[(int)(n - 7)..]);
            if (j != n - 7) {
                bΔ1.Fatal("bad index", j);
            }
        }
        buf[n - 1] = (rune)'\x00';
    });
}

public static void BenchmarkIndexEasy(ж<testing.B> Ꮡb) {
    benchBytes(Ꮡb, indexSizes, (ж<testing.B> bΔ1, nint n) => {
        var buf = bmbuf[0..(int)(n)];
        buf[n - 1] = (rune)'x';
        buf[n - 7] = (rune)'x';
        for (nint i = 0; i < (~bΔ1).N; i++) {
            nint j = Index(buf, buf[(int)(n - 7)..]);
            if (j != n - 7) {
                bΔ1.Fatal("bad index", j);
            }
        }
        buf[n - 1] = (rune)'\x00';
        buf[n - 7] = (rune)'\x00';
    });
}

public static void BenchmarkCount(ж<testing.B> Ꮡb) {
    benchBytes(Ꮡb, indexSizes, (ж<testing.B> bΔ1, nint n) => {
        var buf = bmbuf[0..(int)(n)];
        buf[n - 1] = (rune)'x';
        for (nint i = 0; i < (~bΔ1).N; i++) {
            nint j = Count(buf, buf[(int)(n - 7)..]);
            if (j != 1) {
                bΔ1.Fatal("bad count", j);
            }
        }
        buf[n - 1] = (rune)'\x00';
    });
}

public static void BenchmarkCountEasy(ж<testing.B> Ꮡb) {
    benchBytes(Ꮡb, indexSizes, (ж<testing.B> bΔ1, nint n) => {
        var buf = bmbuf[0..(int)(n)];
        buf[n - 1] = (rune)'x';
        buf[n - 7] = (rune)'x';
        for (nint i = 0; i < (~bΔ1).N; i++) {
            nint j = Count(buf, buf[(int)(n - 7)..]);
            if (j != 1) {
                bΔ1.Fatal("bad count", j);
            }
        }
        buf[n - 1] = (rune)'\x00';
        buf[n - 7] = (rune)'\x00';
    });
}

public static void BenchmarkCountSingle(ж<testing.B> Ꮡb) {
    benchBytes(Ꮡb, indexSizes, (ж<testing.B> bΔ1, nint n) => {
        var buf = bmbuf[0..(int)(n)];
        nint step = 8;
        for (nint i = 0; i < len(buf); i += step) {
            buf[i] = 1;
        }
        nint expect = (len(buf) + (step - 1)) / step;
        for (nint i = 0; i < (~bΔ1).N; i++) {
            nint j = Count(buf, new byte[]{1}.slice());
            if (j != expect) {
                bΔ1.Fatal("bad count", j, expect);
            }
        }
        for (nint i = 0; i < len(buf); i++) {
            buf[i] = 0;
        }
    });
}

[GoType] partial struct SplitTest {
    internal @string s;
    internal @string sep;
    internal nint n;
    internal slice<@string> a;
}

internal static slice<SplitTest> splittests = new SplitTest[]{
    new(""u8, ""u8, -1, new @string[]{}.slice()),
    new(abcd, "a"u8, 0, default!),
    new(abcd, ""u8, 2, new @string[]{"a", "bcd"}.slice()),
    new(abcd, "a"u8, -1, new @string[]{"", "bcd"}.slice()),
    new(abcd, "z"u8, -1, new @string[]{"abcd"}.slice()),
    new(abcd, ""u8, -1, new @string[]{"a", "b", "c", "d"}.slice()),
    new(commas, ","u8, -1, new @string[]{"1", "2", "3", "4"}.slice()),
    new(dots, "..."u8, -1, new @string[]{"1", ".2", ".3", ".4"}.slice()),
    new(faces, "☹"u8, -1, new @string[]{"☺☻", ""}.slice()),
    new(faces, "~"u8, -1, new @string[]{faces}.slice()),
    new(faces, ""u8, -1, new @string[]{"☺", "☻", "☹"}.slice()),
    new("1 2 3 4"u8, " "u8, 3, new @string[]{"1", "2", "3 4"}.slice()),
    new("1 2"u8, " "u8, 3, new @string[]{"1", "2"}.slice()),
    new("123"u8, ""u8, 2, new @string[]{"1", "23"}.slice()),
    new("123"u8, ""u8, 17, new @string[]{"1", "2", "3"}.slice()),
    new("bT"u8, "T"u8, (nint)(2305843009213693951L), new @string[]{"b", ""}.slice()),
    new(((@string)(new byte[]{0xff, 0x2d, 0xff})), ""u8, -1, new @string[]{((@string)(new byte[]{0xff})), "-", ((@string)(new byte[]{0xff}))}.slice()),
    new(((@string)(new byte[]{0xff, 0x2d, 0xff})), "-"u8, -1, new @string[]{((@string)(new byte[]{0xff})), ((@string)(new byte[]{0xff}))}.slice())
}.slice();

public static void TestSplit(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    foreach (var (_, tt) in splittests) {
        var a = SplitN(slice<byte>(tt.s), slice<byte>(tt.sep), tt.n);
        // Appending to the results should not change future results.
        slice<byte> x = default!;
        foreach (var (_, v) in a) {
            x = append(v, (byte)((rune)'z'));
        }
        var result = sliceOfString(a);
        if (!eq(result, tt.a)) {
            Ꮡt.Errorf(@"Split(%q, %q, %d) = %v; want %v"u8, tt.s, tt.sep, tt.n, result, tt.a);
            continue;
        }
        if (tt.n == 0 || len(a) == 0) {
            continue;
        }
        {
            @string want = tt.a[len(tt.a) - 1] + "z"; if (((sstring)x) != want) {
                Ꮡt.Errorf("last appended result was %s; want %s"u8, x, want);
            }
        }
        var s = Join(a, slice<byte>(tt.sep));
        if (((sstring)s) != tt.s) {
            Ꮡt.Errorf(@"Join(Split(%q, %q, %d), %q) = %q"u8, tt.s, tt.sep, tt.n, tt.sep, s);
        }
        if (tt.n < 0) {
            var b = Split(slice<byte>(tt.s), slice<byte>(tt.sep));
            if (!reflect.DeepEqual(a, b)) {
                Ꮡt.Errorf("Split disagrees withSplitN(%q, %q, %d) = %v; want %v"u8, tt.s, tt.sep, tt.n, b, a);
            }
        }
        if (len(a) > 0) {
            var (@in, @out) = (a[0], s);
            if (cap(@in) == cap(@out) && Ꮡ(@in[..1], 0) == Ꮡ(@out[..1], 0)) {
                Ꮡt.Errorf("Join(%#v, %q) didn't copy"u8, a, tt.sep);
            }
        }
    }
}

internal static slice<SplitTest> splitaftertests = new SplitTest[]{
    new(abcd, "a"u8, -1, new @string[]{"a", "bcd"}.slice()),
    new(abcd, "z"u8, -1, new @string[]{"abcd"}.slice()),
    new(abcd, ""u8, -1, new @string[]{"a", "b", "c", "d"}.slice()),
    new(commas, ","u8, -1, new @string[]{"1,", "2,", "3,", "4"}.slice()),
    new(dots, "..."u8, -1, new @string[]{"1...", ".2...", ".3...", ".4"}.slice()),
    new(faces, "☹"u8, -1, new @string[]{"☺☻☹", ""}.slice()),
    new(faces, "~"u8, -1, new @string[]{faces}.slice()),
    new(faces, ""u8, -1, new @string[]{"☺", "☻", "☹"}.slice()),
    new("1 2 3 4"u8, " "u8, 3, new @string[]{"1 ", "2 ", "3 4"}.slice()),
    new("1 2 3"u8, " "u8, 3, new @string[]{"1 ", "2 ", "3"}.slice()),
    new("1 2"u8, " "u8, 3, new @string[]{"1 ", "2"}.slice()),
    new("123"u8, ""u8, 2, new @string[]{"1", "23"}.slice()),
    new("123"u8, ""u8, 17, new @string[]{"1", "2", "3"}.slice())
}.slice();

public static void TestSplitAfter(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in splitaftertests) {
        var a = SplitAfterN(slice<byte>(tt.s), slice<byte>(tt.sep), tt.n);
        // Appending to the results should not change future results.
        slice<byte> x = default!;
        foreach (var (_, v) in a) {
            x = append(v, (byte)((rune)'z'));
        }
        var result = sliceOfString(a);
        if (!eq(result, tt.a)) {
            Ꮡt.Errorf(@"Split(%q, %q, %d) = %v; want %v"u8, tt.s, tt.sep, tt.n, result, tt.a);
            continue;
        }
        {
            @string want = tt.a[len(tt.a) - 1] + "z"; if (((sstring)x) != want) {
                Ꮡt.Errorf("last appended result was %s; want %s"u8, x, want);
            }
        }
        var s = Join(a, default!);
        if (((sstring)s) != tt.s) {
            Ꮡt.Errorf(@"Join(Split(%q, %q, %d), %q) = %q"u8, tt.s, tt.sep, tt.n, tt.sep, s);
        }
        if (tt.n < 0) {
            var b = SplitAfter(slice<byte>(tt.s), slice<byte>(tt.sep));
            if (!reflect.DeepEqual(a, b)) {
                Ꮡt.Errorf("SplitAfter disagrees withSplitAfterN(%q, %q, %d) = %v; want %v"u8, tt.s, tt.sep, tt.n, b, a);
            }
        }
    }
}

[GoType] partial struct FieldsTest {
    internal @string s;
    internal slice<@string> a;
}

internal static slice<FieldsTest> fieldstests = new FieldsTest[]{
    new(""u8, new @string[]{}.slice()),
    new(" "u8, new @string[]{}.slice()),
    new(" \t "u8, new @string[]{}.slice()),
    new("  abc  "u8, new @string[]{"abc"}.slice()),
    new("1 2 3 4"u8, new @string[]{"1", "2", "3", "4"}.slice()),
    new("1  2  3  4"u8, new @string[]{"1", "2", "3", "4"}.slice()),
    new("1\t\t2\t\t3\t4"u8, new @string[]{"1", "2", "3", "4"}.slice()),
    new("1\u20002\u20013\u20024"u8, new @string[]{"1", "2", "3", "4"}.slice()),
    new("\u2000\u2001\u2002"u8, new @string[]{}.slice()),
    new("\n™\t™\n"u8, new @string[]{"™", "™"}.slice()),
    new(faces, new @string[]{faces}.slice())
}.slice();

public static void TestFields(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in fieldstests) {
        var b = slice<byte>(tt.s);
        var a = Fields(b);
        // Appending to the results should not change future results.
        slice<byte> x = default!;
        foreach (var (_, v) in a) {
            x = append(v, (byte)((rune)'z'));
        }
        var result = sliceOfString(a);
        if (!eq(result, tt.a)) {
            Ꮡt.Errorf("Fields(%q) = %v; want %v"u8, tt.s, a, tt.a);
            continue;
        }
        if (((sstring)b) != tt.s) {
            Ꮡt.Errorf("slice changed to %s; want %s"u8, ((@string)b), tt.s);
        }
        if (len(tt.a) > 0) {
            {
                @string want = tt.a[len(tt.a) - 1] + "z"; if (((sstring)x) != want) {
                    Ꮡt.Errorf("last appended result was %s; want %s"u8, x, want);
                }
            }
        }
    }
}

public static void TestFieldsFunc(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in fieldstests) {
        var a = FieldsFunc(slice<byte>(tt.s), Δunicode.IsSpace);
        var result = sliceOfString(a);
        if (!eq(result, tt.a)) {
            Ꮡt.Errorf("FieldsFunc(%q, unicode.IsSpace) = %v; want %v"u8, tt.s, a, tt.a);
            continue;
        }
    }
    var pred = (rune c) => c == (rune)'X';
    slice<FieldsTest> fieldsFuncTests = new FieldsTest[]{
        new(""u8, new @string[]{}.slice()),
        new("XX"u8, new @string[]{}.slice()),
        new("XXhiXXX"u8, new @string[]{"hi"}.slice()),
        new("aXXbXXXcX"u8, new @string[]{"a", "b", "c"}.slice())
    }.slice();
    foreach (var (_, tt) in fieldsFuncTests) {
        var b = slice<byte>(tt.s);
        var a = FieldsFunc(b, pred);
        // Appending to the results should not change future results.
        slice<byte> x = default!;
        foreach (var (_, v) in a) {
            x = append(v, (byte)((rune)'z'));
        }
        var result = sliceOfString(a);
        if (!eq(result, tt.a)) {
            Ꮡt.Errorf("FieldsFunc(%q) = %v, want %v"u8, tt.s, a, tt.a);
        }
        if (((sstring)b) != tt.s) {
            Ꮡt.Errorf("slice changed to %s; want %s"u8, b, tt.s);
        }
        if (len(tt.a) > 0) {
            {
                @string want = tt.a[len(tt.a) - 1] + "z"; if (((sstring)x) != want) {
                    Ꮡt.Errorf("last appended result was %s; want %s"u8, x, want);
                }
            }
        }
    }
}

// Test case for any function which accepts and returns a byte slice.
// For ease of creation, we write the input byte slice as a string.
[GoType] partial struct StringTest {
    internal @string @in;
    internal slice<byte> @out;
}

// grows one byte per char
// test utf8.RuneSelf and utf8.MaxRune
internal static slice<StringTest> upperTests = new StringTest[]{
    new(""u8, slice<byte>(""u8)),
    new("ONLYUPPER"u8, slice<byte>("ONLYUPPER"u8)),
    new("abc"u8, slice<byte>("ABC"u8)),
    new("AbC123"u8, slice<byte>("ABC123"u8)),
    new("azAZ09_"u8, slice<byte>("AZAZ09_"u8)),
    new("longStrinGwitHmixofsmaLLandcAps"u8, slice<byte>("LONGSTRINGWITHMIXOFSMALLANDCAPS"u8)),
    new("long\u0250string\u0250with\u0250nonascii\u2C6Fchars"u8, slice<byte>("LONG\u2C6FSTRING\u2C6FWITH\u2C6FNONASCII\u2C6FCHARS"u8)),
    new("\u0250\u0250\u0250\u0250\u0250"u8, slice<byte>("\u2C6F\u2C6F\u2C6F\u2C6F\u2C6F"u8)),
    new("a\u0080\U0010FFFF"u8, slice<byte>("A\u0080\U0010FFFF"u8))
}.slice();

// shrinks one byte per char
// test utf8.RuneSelf and utf8.MaxRune
internal static slice<StringTest> lowerTests = new StringTest[]{
    new(""u8, slice<byte>(""u8)),
    new("abc"u8, slice<byte>("abc"u8)),
    new("AbC123"u8, slice<byte>("abc123"u8)),
    new("azAZ09_"u8, slice<byte>("azaz09_"u8)),
    new("longStrinGwitHmixofsmaLLandcAps"u8, slice<byte>("longstringwithmixofsmallandcaps"u8)),
    new("LONG\u2C6FSTRING\u2C6FWITH\u2C6FNONASCII\u2C6FCHARS"u8, slice<byte>("long\u0250string\u0250with\u0250nonascii\u0250chars"u8)),
    new("\u2C6D\u2C6D\u2C6D\u2C6D\u2C6D"u8, slice<byte>("\u0251\u0251\u0251\u0251\u0251"u8)),
    new("A\u0080\U0010FFFF"u8, slice<byte>("a\u0080\U0010FFFF"u8))
}.slice();

internal static readonly @string space = "\t\v\r\f\n\u0085\u00a0\u2000\u3000"u8;

internal static slice<StringTest> trimSpaceTests = new StringTest[]{
    new(""u8, default!),
    new("  a"u8, slice<byte>("a"u8)),
    new("b  "u8, slice<byte>("b"u8)),
    new("abc"u8, slice<byte>("abc"u8)),
    new(space + "abc" + space, slice<byte>("abc"u8)),
    new(" "u8, default!),
    new("\u3000 "u8, default!),
    new(" \u3000"u8, default!),
    new(" \t\r\n \t\t\r\r\n\n "u8, default!),
    new(" \t\r\n x\t\t\r\r\n\n "u8, slice<byte>("x"u8)),
    new(" \u2000\t\r\n x\t\t\r\r\ny\n \u3000"u8, slice<byte>("x\t\t\r\r\ny"u8)),
    new("1 \t\r\n2"u8, slice<byte>("1 \t\r\n2"u8)),
    new(((@string)(new byte[]{0x20, 0x78, 0x80})), slice<byte>(((@string)(new byte[]{0x78, 0x80})))),
    new(((@string)(new byte[]{0x20, 0x78, 0xc0})), slice<byte>(((@string)(new byte[]{0x78, 0xc0})))),
    new(((@string)(new byte[]{0x78, 0x20, 0xc0, 0xc0, 0x20})), slice<byte>(((@string)(new byte[]{0x78, 0x20, 0xc0, 0xc0})))),
    new(((@string)(new byte[]{0x78, 0x20, 0xc0})), slice<byte>(((@string)(new byte[]{0x78, 0x20, 0xc0})))),
    new(((@string)(new byte[]{0x78, 0x20, 0xc0, 0x20})), slice<byte>(((@string)(new byte[]{0x78, 0x20, 0xc0})))),
    new(((@string)(new byte[]{0x78, 0x20, 0xc0, 0xc0, 0x20})), slice<byte>(((@string)(new byte[]{0x78, 0x20, 0xc0, 0xc0})))),
    new(((@string)(new byte[]{0x78, 0x20, 0xe2, 0x98, 0xba, 0xc0, 0xc0, 0x20})), slice<byte>(((@string)(new byte[]{0x78, 0x20, 0xe2, 0x98, 0xba, 0xc0, 0xc0})))),
    new("x ☺ "u8, slice<byte>("x ☺"u8))
}.slice();

// Execute f on each test case.  funcName should be the name of f; it's used
// in failure reports.
internal static void runStringTests(ж<testing.T> Ꮡt, Func<slice<byte>, slice<byte>> f, @string funcName, slice<StringTest> testCases) {
    foreach (var (_, tc) in testCases) {
        var actual = f(slice<byte>(tc.@in));
        if (actual == default! && tc.@out != default!) {
            Ꮡt.Errorf("%s(%q) = nil; want %q"u8, funcName, tc.@in, tc.@out);
        }
        if (actual != default! && tc.@out == default!) {
            Ꮡt.Errorf("%s(%q) = %q; want nil"u8, funcName, tc.@in, actual);
        }
        if (!Equal(actual, tc.@out)) {
            Ꮡt.Errorf("%s(%q) = %q; want %q"u8, funcName, tc.@in, actual, tc.@out);
        }
    }
}

internal static @string tenRunes(rune r) {
    var runes = new slice<rune>(10);
    foreach (var (i, _) in runes) {
        runes[i] = r;
    }
    return ((@string)runes);
}

// User-defined self-inverse mapping function
internal static rune rot13(rune r) {
    const rune step = 13;
    if (r >= (rune)'a' && r <= (rune)'z') {
        return ((r - (rune)'a' + step) % 26) + (rune)'a';
    }
    if (r >= (rune)'A' && r <= (rune)'Z') {
        return ((r - (rune)'A' + step) % 26) + (rune)'A';
    }
    return r;
}

public static void TestMap(ж<testing.T> Ꮡt) {
    // Run a couple of awful growth/shrinkage tests
    @string a = tenRunes((rune)'a');
    // 1.  Grow. This triggers two reallocations in Map.
    var maxRune = rune (rune r) => Δunicode.MaxRune;
    var m = Map(maxRune, slice<byte>(a));
    @string expect = tenRunes(Δunicode.MaxRune);
    if (((sstring)m) != expect) {
        Ꮡt.Errorf("growing: expected %q got %q"u8, expect, m);
    }
    // 2. Shrink
    var minRune = (rune r) => (rune)'a';
    m = Map(minRune, slice<byte>(tenRunes(Δunicode.MaxRune)));
    expect = a;
    if (((sstring)m) != expect) {
        Ꮡt.Errorf("shrinking: expected %q got %q"u8, expect, m);
    }
    // 3. Rot13
    m = Map(rot13, slice<byte>("a to zed"u8));
    expect = "n gb mrq"u8;
    if (((sstring)m) != expect) {
        Ꮡt.Errorf("rot13: expected %q got %q"u8, expect, m);
    }
    // 4. Rot13^2
    m = Map(rot13, Map(rot13, slice<byte>("a to zed"u8)));
    expect = "a to zed"u8;
    if (((sstring)m) != expect) {
        Ꮡt.Errorf("rot13: expected %q got %q"u8, expect, m);
    }
    // 5. Drop
    var dropNotLatin = (rune r) => {
        if (Δunicode.Is(Δunicode.Latin, r)) {
            return r;
        }
        return -1;
    };
    m = Map(dropNotLatin, slice<byte>("Hello, 세계"u8));
    expect = "Hello"u8;
    if (((sstring)m) != expect) {
        Ꮡt.Errorf("drop: expected %q got %q"u8, expect, m);
    }
    // 6. Invalid rune
    var invalidRune = rune (rune r) => utf8.MaxRune + 1;
    m = Map(invalidRune, slice<byte>("x"u8));
    expect = "\uFFFD"u8;
    if (((sstring)m) != expect) {
        Ꮡt.Errorf("invalidRune: expected %q got %q"u8, expect, m);
    }
}

public static void TestToUpper(ж<testing.T> Ꮡt) {
    runStringTests(Ꮡt, ToUpper, "ToUpper"u8, upperTests);
}

public static void TestToLower(ж<testing.T> Ꮡt) {
    runStringTests(Ꮡt, ToLower, "ToLower"u8, lowerTests);
}

public static void BenchmarkToUpper(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in upperTests) {
        ref var tc = ref heap(new StringTest(), out var Ꮡtc);
        tc = vᴛ1;

        var tin = slice<byte>(tc.@in);
        var tcʗ1 = tc;
        var tinʗ1 = tin;
        Ꮡb.Run(tc.@in, (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var actual = ToUpper(tinʗ1);
                if (!Equal(actual, tcʗ1.@out)) {
                    bΔ1.Errorf("ToUpper(%q) = %q; want %q"u8, tcʗ1.@in, actual, tcʗ1.@out);
                }
            }
        });
    }
}

public static void BenchmarkToLower(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in lowerTests) {
        ref var tc = ref heap(new StringTest(), out var Ꮡtc);
        tc = vᴛ1;

        var tin = slice<byte>(tc.@in);
        var tcʗ1 = tc;
        var tinʗ1 = tin;
        Ꮡb.Run(tc.@in, (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var actual = ToLower(tinʗ1);
                if (!Equal(actual, tcʗ1.@out)) {
                    bΔ1.Errorf("ToLower(%q) = %q; want %q"u8, tcʗ1.@in, actual, tcʗ1.@out);
                }
            }
        });
    }
}


[GoType("dyn")] partial struct toValidUTF8Testsᴛ1 {
    internal @string @in;
    internal @string repl;
    internal @string @out;
}
internal static slice<toValidUTF8Testsᴛ1> toValidUTF8Tests = new toValidUTF8Testsᴛ1[]{
    new(""u8, "\uFFFD"u8, ""u8),
    new("abc"u8, "\uFFFD"u8, "abc"u8),
    new("\uFDDD"u8, "\uFFFD"u8, "\uFDDD"u8),
    new(((@string)(new byte[]{0x61, 0xff, 0x62})), "\uFFFD"u8, "a\uFFFDb"u8),
    new(((@string)(new byte[]{0x61, 0xff, 0x62, 0xef, 0xbf, 0xbd})), "X"u8, "aXb\uFFFD"u8),
    new(((@string)(new byte[]{0x61, 0xe2, 0x98, 0xba, 0xff, 0x62, 0xe2, 0x98, 0xba, 0xc0, 0xaf, 0x63, 0xe2, 0x98, 0xba, 0xff})), ""u8, "a☺b☺c☺"u8),
    new(((@string)(new byte[]{0x61, 0xe2, 0x98, 0xba, 0xff, 0x62, 0xe2, 0x98, 0xba, 0xc0, 0xaf, 0x63, 0xe2, 0x98, 0xba, 0xff})), "日本語"u8, "a☺日本語b☺日本語c☺日本語"u8),
    new(((@string)(new byte[]{0xc0, 0xaf})), "\uFFFD"u8, "\uFFFD"u8),
    new(((@string)(new byte[]{0xe0, 0x80, 0xaf})), "\uFFFD"u8, "\uFFFD"u8),
    new(((@string)(new byte[]{0xed, 0xa0, 0x80})), "abc"u8, "abc"u8),
    new(((@string)(new byte[]{0xed, 0xbf, 0xbf})), "\uFFFD"u8, "\uFFFD"u8),
    new(((@string)(new byte[]{0xf0, 0x80, 0x80, 0xaf})), "☺"u8, "☺"u8),
    new(((@string)(new byte[]{0xf8, 0x80, 0x80, 0x80, 0xaf})), "\uFFFD"u8, "\uFFFD"u8),
    new(((@string)(new byte[]{0xfc, 0x80, 0x80, 0x80, 0x80, 0xaf})), "\uFFFD"u8, "\uFFFD"u8)
}.slice();

public static void TestToValidUTF8(ж<testing.T> Ꮡt) {
    foreach (var (_, tc) in toValidUTF8Tests) {
        var got = ToValidUTF8(slice<byte>(tc.@in), slice<byte>(tc.repl));
        if (!Equal(got, slice<byte>(tc.@out))) {
            Ꮡt.Errorf("ToValidUTF8(%q, %q) = %q; want %q"u8, tc.@in, tc.repl, got, tc.@out);
        }
    }
}

public static void TestTrimSpace(ж<testing.T> Ꮡt) {
    runStringTests(Ꮡt, TrimSpace, "TrimSpace"u8, trimSpaceTests);
}

[GoType] partial struct RepeatTest {
    internal @string @in, @out;
    internal nint count;
}

internal static @string longString = "a"u8 + ((sstring)new slice<byte>((1 << (int)(16)))) + "z"u8;

// Tests for results over the chunkLimit
public static slice<RepeatTest> RepeatTests = new RepeatTest[]{
    new(""u8, ""u8, 0),
    new(""u8, ""u8, 1),
    new(""u8, ""u8, 2),
    new("-"u8, ""u8, 0),
    new("-"u8, "-"u8, 1),
    new("-"u8, "----------"u8, 10),
    new("abc "u8, "abc abc abc "u8, 3),
    new(((@string)(rune)0), ((@string)new slice<byte>((1 << (int)(16)))), (1 << (int)(16))),
    new(longString, longString + longString, 2)
}.slice();

public static void TestRepeat(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in RepeatTests) {
        var tin = slice<byte>(tt.@in);
        var tout = slice<byte>(tt.@out);
        var a = Repeat(tin, tt.count);
        if (!Equal(a, tout)) {
            Ꮡt.Errorf("Repeat(%q, %d) = %q; want %q"u8, tin, tt.count, a, tout);
            continue;
        }
    }
}

internal static error /*err*/ repeat(slice<byte> b, nint count) {
    error err = default!;
    func((defer, recover) => {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    switch (r.type()) {
                    case {} Δv when Δv._<error>(out var v): {
                        err = v;
                        break;
                    }
                    default: {
                        var v = r;
                        err = fmt.Errorf("%s"u8, v);
                        break;
                    }}
                }
            }
        });
        Repeat(b, count);
    });
    return err;
}

[GoType("dyn")] partial struct TestRepeatCatchesOverflow_testCase {
    internal @string s;
    internal nint count;
    internal @string errStr;
}

// See Issue golang.org/issue/16237
public static void TestRepeatCatchesOverflow(ж<testing.T> Ꮡt) {
    var runTestCases = (@string prefix, slice<TestRepeatCatchesOverflow_testCase> tests) => {
        foreach (var (i, vᴛ1) in tests) {
            ref var tt = ref heap(new TestRepeatCatchesOverflow_testCase(), out var Ꮡtt);
            tt = vᴛ1;

            var err = repeat(slice<byte>(tt.s), tt.count);
            if (tt.errStr == ""u8) {
                if (err != default!) {
                    Ꮡt.Errorf("#%d panicked %v"u8, i, err);
                }
                continue;
            }
            if (err == default! || !strings.Contains(err.Error(), tt.errStr)) {
                Ꮡt.Errorf("%s#%d got %q want %q"u8, prefix, i, err, tt.errStr);
            }
        }
    };
    nint maxInt = /* int(^uint(0) >> 1) */ unchecked((nint)9223372036854775807);
    runTestCases(""u8, new slice<TestRepeatCatchesOverflow_testCase>(7){
        [0] = new("--"u8, -2147483647, "negative"u8),
        [1] = new(""u8, maxInt, ""u8),
        [2] = new("-"u8, 10, ""u8),
        [3] = new("gopher"u8, 0, ""u8),
        [4] = new("-"u8, -1, "negative"u8),
        [5] = new("--"u8, -102, "negative"u8),
        [6] = new(((@string)new slice<byte>(255)), (nint)((~(nuint)0) / 255 + 1), "overflow"u8)
    });
    const bool is64Bit = /* 1<<(^uintptr(0)>>63)/2 != 0 */ true;
    if (!is64Bit) {
        return;
    }
    runTestCases("64-bit"u8, new golib.SparseArray<TestRepeatCatchesOverflow_testCase>{
        [0] = new("-"u8, maxInt, "out of range"u8)
    }.slice());
}

internal static bool runesEqual(slice<rune> a, slice<rune> b) {
    if (len(a) != len(b)) {
        return false;
    }
    foreach (var (i, r) in a) {
        if (r != b[i]) {
            return false;
        }
    }
    return true;
}

[GoType] partial struct RunesTest {
    internal @string @in;
    internal slice<rune> @out;
    internal bool lossy;
}

public static slice<RunesTest> RunesTests = new RunesTest[]{
    new(""u8, new rune[]{}.slice(), false),
    new(" "u8, new rune[]{32}.slice(), false),
    new("ABC"u8, new rune[]{65, 66, 67}.slice(), false),
    new("abc"u8, new rune[]{97, 98, 99}.slice(), false),
    new("\u65e5\u672c\u8a9e"u8, new rune[]{26085, 26412, 35486}.slice(), false),
    new(((@string)(new byte[]{0x61, 0x62, 0x80, 0x63})), new rune[]{97, 98, 0xFFFD, 99}.slice(), true),
    new(((@string)(new byte[]{0x61, 0x62, 0xc0, 0x63})), new rune[]{97, 98, 0xFFFD, 99}.slice(), true)
}.slice();

public static void TestRunes(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    foreach (var (_, tt) in RunesTests) {
        var tin = slice<byte>(tt.@in);
        var a = Runes(tin);
        if (!runesEqual(a, tt.@out)) {
            Ꮡt.Errorf("Runes(%q) = %v; want %v"u8, tin, a, tt.@out);
            continue;
        }
        if (!tt.lossy) {
            // can only test reassembly if we didn't lose information
            @string s = ((@string)a);
            if (s != tt.@in) {
                Ꮡt.Errorf("string(Runes(%q)) = %x; want %x"u8, tin, s, tin);
            }
        }
    }
}

[GoType] partial struct TrimTest {
    internal @string f;
    internal @string @in, arg, @out;
}

//empty string tests
internal static slice<TrimTest> trimTests = new TrimTest[]{
    new("Trim"u8, "abba"u8, "a"u8, "bb"u8),
    new("Trim"u8, "abba"u8, "ab"u8, ""u8),
    new("TrimLeft"u8, "abba"u8, "ab"u8, ""u8),
    new("TrimRight"u8, "abba"u8, "ab"u8, ""u8),
    new("TrimLeft"u8, "abba"u8, "a"u8, "bba"u8),
    new("TrimLeft"u8, "abba"u8, "b"u8, "abba"u8),
    new("TrimRight"u8, "abba"u8, "a"u8, "abb"u8),
    new("TrimRight"u8, "abba"u8, "b"u8, "abba"u8),
    new("Trim"u8, "<tag>"u8, "<>"u8, "tag"u8),
    new("Trim"u8, "* listitem"u8, " *"u8, "listitem"u8),
    new("Trim"u8, @"""quote"""u8, @""""u8, "quote"u8),
    new("Trim"u8, "\u2C6F\u2C6F\u0250\u0250\u2C6F\u2C6F"u8, "\u2C6F"u8, "\u0250\u0250"u8),
    new("Trim"u8, ((@string)(new byte[]{0x80, 0x74, 0x65, 0x73, 0x74, 0xff})), ((@string)(new byte[]{0xff})), "test"u8),
    new("Trim"u8, " Ġ "u8, " "u8, "Ġ"u8),
    new("Trim"u8, " Ġİ0"u8, "0 "u8, "Ġİ"u8),
    new("Trim"u8, "abba"u8, ""u8, "abba"u8),
    new("Trim"u8, ""u8, "123"u8, ""u8),
    new("Trim"u8, ""u8, ""u8, ""u8),
    new("TrimLeft"u8, "abba"u8, ""u8, "abba"u8),
    new("TrimLeft"u8, ""u8, "123"u8, ""u8),
    new("TrimLeft"u8, ""u8, ""u8, ""u8),
    new("TrimRight"u8, "abba"u8, ""u8, "abba"u8),
    new("TrimRight"u8, ""u8, "123"u8, ""u8),
    new("TrimRight"u8, ""u8, ""u8, ""u8),
    new("TrimRight"u8, ((@string)(new byte[]{0xe2, 0x98, 0xba, 0xc0})), "☺"u8, ((@string)(new byte[]{0xe2, 0x98, 0xba, 0xc0}))),
    new("TrimPrefix"u8, "aabb"u8, "a"u8, "abb"u8),
    new("TrimPrefix"u8, "aabb"u8, "b"u8, "aabb"u8),
    new("TrimSuffix"u8, "aabb"u8, "a"u8, "aabb"u8),
    new("TrimSuffix"u8, "aabb"u8, "b"u8, "aab"u8)
}.slice();

[GoType] partial struct TrimNilTest {
    internal @string f;
    internal slice<byte> @in;
    internal @string arg;
    internal slice<byte> @out;
}

internal static slice<TrimNilTest> trimNilTests = new TrimNilTest[]{
    new("Trim"u8, default!, ""u8, default!),
    new("Trim"u8, new byte[]{}.slice(), ""u8, default!),
    new("Trim"u8, new byte[]{(rune)'a'}.slice(), "a"u8, default!),
    new("Trim"u8, new byte[]{(rune)'a', (rune)'a'}.slice(), "a"u8, default!),
    new("Trim"u8, new byte[]{(rune)'a'}.slice(), "ab"u8, default!),
    new("Trim"u8, new byte[]{(rune)'a', (rune)'b'}.slice(), "ab"u8, default!),
    new("Trim"u8, slice<byte>("☺"u8), "☺"u8, default!),
    new("TrimLeft"u8, default!, ""u8, default!),
    new("TrimLeft"u8, new byte[]{}.slice(), ""u8, default!),
    new("TrimLeft"u8, new byte[]{(rune)'a'}.slice(), "a"u8, default!),
    new("TrimLeft"u8, new byte[]{(rune)'a', (rune)'a'}.slice(), "a"u8, default!),
    new("TrimLeft"u8, new byte[]{(rune)'a'}.slice(), "ab"u8, default!),
    new("TrimLeft"u8, new byte[]{(rune)'a', (rune)'b'}.slice(), "ab"u8, default!),
    new("TrimLeft"u8, slice<byte>("☺"u8), "☺"u8, default!),
    new("TrimRight"u8, default!, ""u8, default!),
    new("TrimRight"u8, new byte[]{}.slice(), ""u8, new byte[]{}.slice()),
    new("TrimRight"u8, new byte[]{(rune)'a'}.slice(), "a"u8, new byte[]{}.slice()),
    new("TrimRight"u8, new byte[]{(rune)'a', (rune)'a'}.slice(), "a"u8, new byte[]{}.slice()),
    new("TrimRight"u8, new byte[]{(rune)'a'}.slice(), "ab"u8, new byte[]{}.slice()),
    new("TrimRight"u8, new byte[]{(rune)'a', (rune)'b'}.slice(), "ab"u8, new byte[]{}.slice()),
    new("TrimRight"u8, slice<byte>("☺"u8), "☺"u8, new byte[]{}.slice()),
    new("TrimPrefix"u8, default!, ""u8, default!),
    new("TrimPrefix"u8, new byte[]{}.slice(), ""u8, new byte[]{}.slice()),
    new("TrimPrefix"u8, new byte[]{(rune)'a'}.slice(), "a"u8, new byte[]{}.slice()),
    new("TrimPrefix"u8, slice<byte>("☺"u8), "☺"u8, new byte[]{}.slice()),
    new("TrimSuffix"u8, default!, ""u8, default!),
    new("TrimSuffix"u8, new byte[]{}.slice(), ""u8, new byte[]{}.slice()),
    new("TrimSuffix"u8, new byte[]{(rune)'a'}.slice(), "a"u8, new byte[]{}.slice()),
    new("TrimSuffix"u8, slice<byte>("☺"u8), "☺"u8, new byte[]{}.slice())
}.slice();

public static void TestTrim(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    var toFn = (Func<slice<byte>, @string, slice<byte>>, Func<slice<byte>, slice<byte>, slice<byte>>) (@string name) => {
        var exprᴛ1 = name;
        if (exprᴛ1 == "Trim"u8) {
            return (Trim, default!);
        }
        if (exprᴛ1 == "TrimLeft"u8) {
            return (TrimLeft, default!);
        }
        if (exprᴛ1 == "TrimRight"u8) {
            return (TrimRight, default!);
        }
        if (exprᴛ1 == "TrimPrefix"u8) {
            return (default!, TrimPrefix);
        }
        if (exprᴛ1 == "TrimSuffix"u8) {
            return (default!, TrimSuffix);
        }
        { /* default: */
            Ꮡt.Errorf("Undefined trim function %s"u8, name);
            return (default!, default!);
        }

    };
    foreach (var (_, tc) in trimTests) {
        @string name = tc.f;
        var (f, fb) = toFn(name);
        if (f == default! && fb == default!) {
            continue;
        }
        @string actual = default!;
        if (f != default!){
            actual = ((@string)f(slice<byte>(tc.@in), tc.arg));
        } else {
            actual = ((@string)fb(slice<byte>(tc.@in), slice<byte>(tc.arg)));
        }
        if (actual != tc.@out) {
            Ꮡt.Errorf("%s(%q, %q) = %q; want %q"u8, name, tc.@in, tc.arg, actual, tc.@out);
        }
    }
    foreach (var (_, tc) in trimNilTests) {
        @string name = tc.f;
        var (f, fb) = toFn(name);
        if (f == default! && fb == default!) {
            continue;
        }
        slice<byte> actual = default!;
        if (f != default!){
            actual = f(tc.@in, tc.arg);
        } else {
            actual = fb(tc.@in, slice<byte>(tc.arg));
        }
        var report = @string (slice<byte> s) => {
            if (s == default!){
                return "nil"u8;
            } else {
                return fmt.Sprintf("%q"u8, s);
            }
        };
        if (len(actual) != 0){
            Ꮡt.Errorf("%s(%s, %q) returned non-empty value"u8, name, report(tc.@in), tc.arg);
        } else {
            var actualNil = actual == default!;
            var outNil = tc.@out == default!;
            if (actualNil != outNil) {
                Ꮡt.Errorf("%s(%s, %q) got nil %t; want nil %t"u8, name, report(tc.@in), tc.arg, actualNil, outNil);
            }
        }
    }
}

[GoType] partial struct predicate {
    internal Func<rune, bool> f;
    internal @string name;
}

internal static predicate isSpace = new predicate(Δunicode.IsSpace, "IsSpace");

internal static predicate isDigit = new predicate(Δunicode.IsDigit, "IsDigit");

internal static predicate isUpper = new predicate(Δunicode.IsUpper, "IsUpper");

internal static predicate isValidRune = new predicate(
    (rune r) => r != utf8.RuneError,
    "IsValidRune"
);

[GoType] partial struct TrimFuncTest {
    internal predicate f;
    internal @string @in;
    internal slice<byte> trimOut;
    internal slice<byte> leftOut;
    internal slice<byte> rightOut;
}

internal static predicate not(predicate p) {
        var pʗ1 = p;
    return new predicate(
        (rune r) => !pʗ1.f(r),
        "not " + p.name
    );
}

// The nils returned by TrimLeftFunc are odd behavior, but we need
// to preserve backwards compatibility.
internal static slice<TrimFuncTest> trimFuncTests = new TrimFuncTest[]{
    new(isSpace, space + " hello " + space,
        slice<byte>("hello"u8),
        slice<byte>("hello " + space),
        slice<byte>(space + " hello")),
    new(isDigit, "\u0e50\u0e5212hello34\u0e50\u0e51"u8,
        slice<byte>("hello"u8),
        slice<byte>("hello34\u0e50\u0e51"u8),
        slice<byte>("\u0e50\u0e5212hello"u8)),
    new(isUpper, "\u2C6F\u2C6F\u2C6F\u2C6FABCDhelloEF\u2C6F\u2C6FGH\u2C6F\u2C6F"u8,
        slice<byte>("hello"u8),
        slice<byte>("helloEF\u2C6F\u2C6FGH\u2C6F\u2C6F"u8),
        slice<byte>("\u2C6F\u2C6F\u2C6F\u2C6FABCDhello"u8)),
    new(not(isSpace), "hello" + space + "hello",
        slice<byte>(space),
        slice<byte>(space + "hello"),
        slice<byte>("hello" + space)),
    new(not(isDigit), "hello\u0e50\u0e521234\u0e50\u0e51helo"u8,
        slice<byte>("\u0e50\u0e521234\u0e50\u0e51"u8),
        slice<byte>("\u0e50\u0e521234\u0e50\u0e51helo"u8),
        slice<byte>("hello\u0e50\u0e521234\u0e50\u0e51"u8)),
    new(isValidRune, ((@string)(new byte[]{0x61, 0x62, 0xc0, 0x61, 0xc0, 0x63, 0x64})),
        slice<byte>(((@string)(new byte[]{0xc0, 0x61, 0xc0}))),
        slice<byte>(((@string)(new byte[]{0xc0, 0x61, 0xc0, 0x63, 0x64}))),
        slice<byte>(((@string)(new byte[]{0x61, 0x62, 0xc0, 0x61, 0xc0})))),
    new(not(isValidRune), ((@string)(new byte[]{0xc0, 0x61, 0xc0})),
        slice<byte>("a"u8),
        slice<byte>(((@string)(new byte[]{0x61, 0xc0}))),
        slice<byte>(((@string)(new byte[]{0xc0, 0x61})))),
    new(isSpace, ""u8,
        default!,
        default!,
        slice<byte>(""u8)),
    new(isSpace, " "u8,
        default!,
        default!,
        slice<byte>(""u8))
}.slice();

[GoType("dyn")] partial struct TestTrimFunc_trimmers {
    internal @string name;
    internal Func<slice<byte>, Func<rune, bool>, slice<byte>> trim;
    internal slice<byte> @out;
}

public static void TestTrimFunc(ж<testing.T> Ꮡt) {
    foreach (var (_, tc) in trimFuncTests) {
        var trimmers = new TestTrimFunc_trimmers[]{
            new("TrimFunc"u8, TrimFunc, tc.trimOut),
            new("TrimLeftFunc"u8, TrimLeftFunc, tc.leftOut),
            new("TrimRightFunc"u8, TrimRightFunc, tc.rightOut)
        }.slice();
        foreach (var (_, trimmer) in trimmers) {
            var actual = trimmer.trim(slice<byte>(tc.@in), tc.f.f);
            if (actual == default! && trimmer.@out != default!) {
                Ꮡt.Errorf("%s(%q, %q) = nil; want %q"u8, trimmer.name, tc.@in, tc.f.name, trimmer.@out);
            }
            if (actual != default! && trimmer.@out == default!) {
                Ꮡt.Errorf("%s(%q, %q) = %q; want nil"u8, trimmer.name, tc.@in, tc.f.name, actual);
            }
            if (!Equal(actual, trimmer.@out)) {
                Ꮡt.Errorf("%s(%q, %q) = %q; want %q"u8, trimmer.name, tc.@in, tc.f.name, actual, trimmer.@out);
            }
        }
    }
}

[GoType] partial struct IndexFuncTest {
    internal @string @in;
    internal predicate f;
    internal nint first, last;
}

// last rune in space is 3 bytes
// tests of invalid UTF-8
internal static slice<IndexFuncTest> indexFuncTests = new IndexFuncTest[]{
    new(""u8, isValidRune, -1, -1),
    new("abc"u8, isDigit, -1, -1),
    new("0123"u8, isDigit, 0, 3),
    new("a1b"u8, isDigit, 1, 1),
    new(space, isSpace, 0, len(space) - 3),
    new("\u0e50\u0e5212hello34\u0e50\u0e51"u8, isDigit, 0, 18),
    new("\u2C6F\u2C6F\u2C6F\u2C6FABCDhelloEF\u2C6F\u2C6FGH\u2C6F\u2C6F"u8, isUpper, 0, 34),
    new("12\u0e50\u0e52hello34\u0e50\u0e51"u8, not(isDigit), 8, 12),
    new(((@string)(new byte[]{0x80, 0x31})), isDigit, 1, 1),
    new(((@string)(new byte[]{0x80, 0x61, 0x62, 0x63})), isDigit, -1, -1),
    new(((@string)(new byte[]{0xc0, 0x61, 0xc0})), isValidRune, 1, 1),
    new(((@string)(new byte[]{0xc0, 0x61, 0xc0})), not(isValidRune), 0, 2),
    new(((@string)(new byte[]{0xc0, 0xe2, 0x98, 0xba, 0xc0})), not(isValidRune), 0, 4),
    new(((@string)(new byte[]{0xc0, 0xe2, 0x98, 0xba, 0xc0, 0xc0})), not(isValidRune), 0, 5),
    new(((@string)(new byte[]{0x61, 0x62, 0xc0, 0x61, 0xc0, 0x63, 0x64})), not(isValidRune), 2, 4),
    new(((@string)(new byte[]{0x61, 0xe0, 0x80, 0x63, 0x64})), not(isValidRune), 1, 2)
}.slice();

public static void TestIndexFunc(ж<testing.T> Ꮡt) {
    foreach (var (_, tc) in indexFuncTests) {
        nint first = IndexFunc(slice<byte>(tc.@in), tc.f.f);
        if (first != tc.first) {
            Ꮡt.Errorf("IndexFunc(%q, %s) = %d; want %d"u8, tc.@in, tc.f.name, first, tc.first);
        }
        nint last = LastIndexFunc(slice<byte>(tc.@in), tc.f.f);
        if (last != tc.last) {
            Ꮡt.Errorf("LastIndexFunc(%q, %s) = %d; want %d"u8, tc.@in, tc.f.name, last, tc.last);
        }
    }
}

[GoType] partial struct ReplaceTest {
    internal @string @in;
    internal @string old, @new;
    internal nint n;
    internal @string @out;
}

public static slice<ReplaceTest> ReplaceTests = new ReplaceTest[]{
    new("hello"u8, "l"u8, "L"u8, 0, "hello"u8),
    new("hello"u8, "l"u8, "L"u8, -1, "heLLo"u8),
    new("hello"u8, "x"u8, "X"u8, -1, "hello"u8),
    new(""u8, "x"u8, "X"u8, -1, ""u8),
    new("radar"u8, "r"u8, "<r>"u8, -1, "<r>ada<r>"u8),
    new(""u8, ""u8, "<>"u8, -1, "<>"u8),
    new("banana"u8, "a"u8, "<>"u8, -1, "b<>n<>n<>"u8),
    new("banana"u8, "a"u8, "<>"u8, 1, "b<>nana"u8),
    new("banana"u8, "a"u8, "<>"u8, 1000, "b<>n<>n<>"u8),
    new("banana"u8, "an"u8, "<>"u8, -1, "b<><>a"u8),
    new("banana"u8, "ana"u8, "<>"u8, -1, "b<>na"u8),
    new("banana"u8, ""u8, "<>"u8, -1, "<>b<>a<>n<>a<>n<>a<>"u8),
    new("banana"u8, ""u8, "<>"u8, 10, "<>b<>a<>n<>a<>n<>a<>"u8),
    new("banana"u8, ""u8, "<>"u8, 6, "<>b<>a<>n<>a<>n<>a"u8),
    new("banana"u8, ""u8, "<>"u8, 5, "<>b<>a<>n<>a<>na"u8),
    new("banana"u8, ""u8, "<>"u8, 1, "<>banana"u8),
    new("banana"u8, "a"u8, "a"u8, -1, "banana"u8),
    new("banana"u8, "a"u8, "a"u8, 1, "banana"u8),
    new("☺☻☹"u8, ""u8, "<>"u8, -1, "<>☺<>☻<>☹<>"u8)
}.slice();

public static void TestReplace(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    foreach (var (_, tt) in ReplaceTests) {
        var @in = append(slice<byte>(tt.@in), ((@string)"<spare>"u8).ꓸꓸꓸ);
        @in = @in[..(int)(len(tt.@in))];
        var @out = Replace(@in, slice<byte>(tt.old), slice<byte>(tt.@new), tt.n);
        {
            @string s = ((@string)@out); if (s != tt.@out) {
                Ꮡt.Errorf("Replace(%q, %q, %q, %d) = %q, want %q"u8, tt.@in, tt.old, tt.@new, tt.n, s, tt.@out);
            }
        }
        if (cap(@in) == cap(@out) && Ꮡ(@in[..1], 0) == Ꮡ(@out[..1], 0)) {
            Ꮡt.Errorf("Replace(%q, %q, %q, %d) didn't copy"u8, tt.@in, tt.old, tt.@new, tt.n);
        }
        if (tt.n == -1) {
            var outΔ1 = ReplaceAll(@in, slice<byte>(tt.old), slice<byte>(tt.@new));
            {
                @string s = ((@string)outΔ1); if (s != tt.@out) {
                    Ꮡt.Errorf("ReplaceAll(%q, %q, %q) = %q, want %q"u8, tt.@in, tt.old, tt.@new, s, tt.@out);
                }
            }
        }
    }
}

[GoType] partial struct TitleTest {
    internal @string @in, @out;
}

public static slice<TitleTest> TitleTests = new TitleTest[]{
    new(""u8, ""u8),
    new("a"u8, "A"u8),
    new(" aaa aaa aaa "u8, " Aaa Aaa Aaa "u8),
    new(" Aaa Aaa Aaa "u8, " Aaa Aaa Aaa "u8),
    new("123a456"u8, "123a456"u8),
    new("double-blind"u8, "Double-Blind"u8),
    new("ÿøû"u8, "Ÿøû"u8),
    new("with_underscore"u8, "With_underscore"u8),
    new(((@string)(new byte[]{0x75, 0x6e, 0x69, 0x63, 0x6f, 0x64, 0x65, 0x20, 0xe2, 0x80, 0xa8, 0x20, 0x6c, 0x69, 0x6e, 0x65, 0x20, 0x73, 0x65, 0x70, 0x61, 0x72, 0x61, 0x74, 0x6f, 0x72})), ((@string)(new byte[]{0x55, 0x6e, 0x69, 0x63, 0x6f, 0x64, 0x65, 0x20, 0xe2, 0x80, 0xa8, 0x20, 0x4c, 0x69, 0x6e, 0x65, 0x20, 0x53, 0x65, 0x70, 0x61, 0x72, 0x61, 0x74, 0x6f, 0x72})))
}.slice();

public static void TestTitle(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in TitleTests) {
        {
            @string s = ((@string)Title(slice<byte>(tt.@in))); if (s != tt.@out) {
                Ꮡt.Errorf("Title(%q) = %q, want %q"u8, tt.@in, s, tt.@out);
            }
        }
    }
}

public static slice<TitleTest> ToTitleTests = new TitleTest[]{
    new(""u8, ""u8),
    new("a"u8, "A"u8),
    new(" aaa aaa aaa "u8, " AAA AAA AAA "u8),
    new(" Aaa Aaa Aaa "u8, " AAA AAA AAA "u8),
    new("123a456"u8, "123A456"u8),
    new("double-blind"u8, "DOUBLE-BLIND"u8),
    new("ÿøû"u8, "ŸØÛ"u8)
}.slice();

public static void TestToTitle(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in ToTitleTests) {
        {
            @string s = ((@string)ToTitle(slice<byte>(tt.@in))); if (s != tt.@out) {
                Ꮡt.Errorf("ToTitle(%q) = %q, want %q"u8, tt.@in, s, tt.@out);
            }
        }
    }
}


[GoType("dyn")] partial struct EqualFoldTestsᴛ1 {
    internal @string s, t;
    internal bool @out;
}
public static slice<EqualFoldTestsᴛ1> EqualFoldTests = new EqualFoldTestsᴛ1[]{
    new("abc"u8, "abc"u8, true),
    new("ABcd"u8, "ABcd"u8, true),
    new("123abc"u8, "123ABC"u8, true),
    new("αβδ"u8, "ΑΒΔ"u8, true),
    new("abc"u8, "xyz"u8, false),
    new("abc"u8, "XYZ"u8, false),
    new("abcdefghijk"u8, "abcdefghijX"u8, false),
    new("abcdefghijk"u8, "abcdefghij\u212A"u8, true),
    new("abcdefghijK"u8, "abcdefghij\u212A"u8, true),
    new("abcdefghijkz"u8, "abcdefghij\u212Ay"u8, false),
    new("abcdefghijKz"u8, "abcdefghij\u212Ay"u8, false)
}.slice();

public static void TestEqualFold(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    foreach (var (_, tt) in EqualFoldTests) {
        {
            var @out = EqualFold(slice<byte>(tt.s), slice<byte>(tt.t)); if (@out != tt.@out) {
                Ꮡt.Errorf("EqualFold(%#q, %#q) = %v, want %v"u8, tt.s, tt.t, @out, tt.@out);
            }
        }
        {
            var @out = EqualFold(slice<byte>(tt.t), slice<byte>(tt.s)); if (@out != tt.@out) {
                Ꮡt.Errorf("EqualFold(%#q, %#q) = %v, want %v"u8, tt.t, tt.s, @out, tt.@out);
            }
        }
    }
}


[GoType("dyn")] partial struct cutTestsᴛ1 {
    internal @string s, sep;
    internal @string before, after;
    internal bool found;
}
internal static slice<cutTestsᴛ1> cutTests = new cutTestsᴛ1[]{
    new("abc"u8, "b"u8, "a"u8, "c"u8, true),
    new("abc"u8, "a"u8, ""u8, "bc"u8, true),
    new("abc"u8, "c"u8, "ab"u8, ""u8, true),
    new("abc"u8, "abc"u8, ""u8, ""u8, true),
    new("abc"u8, ""u8, ""u8, "abc"u8, true),
    new("abc"u8, "d"u8, "abc"u8, ""u8, false),
    new(""u8, "d"u8, ""u8, ""u8, false),
    new(""u8, ""u8, ""u8, ""u8, true)
}.slice();

public static void TestCut(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in cutTests) {
        {
            var (before, after, found) = Cut(slice<byte>(tt.s), slice<byte>(tt.sep)); if (((sstring)before) != tt.before || ((sstring)after) != tt.after || found != tt.found) {
                Ꮡt.Errorf("Cut(%q, %q) = %q, %q, %v, want %q, %q, %v"u8, tt.s, tt.sep, before, after, found, tt.before, tt.after, tt.found);
            }
        }
    }
}


[GoType("dyn")] partial struct cutPrefixTestsᴛ1 {
    internal @string s, sep;
    internal @string after;
    internal bool found;
}
internal static slice<cutPrefixTestsᴛ1> cutPrefixTests = new cutPrefixTestsᴛ1[]{
    new("abc"u8, "a"u8, "bc"u8, true),
    new("abc"u8, "abc"u8, ""u8, true),
    new("abc"u8, ""u8, "abc"u8, true),
    new("abc"u8, "d"u8, "abc"u8, false),
    new(""u8, "d"u8, ""u8, false),
    new(""u8, ""u8, ""u8, true)
}.slice();

public static void TestCutPrefix(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in cutPrefixTests) {
        {
            var (after, found) = CutPrefix(slice<byte>(tt.s), slice<byte>(tt.sep)); if (((sstring)after) != tt.after || found != tt.found) {
                Ꮡt.Errorf("CutPrefix(%q, %q) = %q, %v, want %q, %v"u8, tt.s, tt.sep, after, found, tt.after, tt.found);
            }
        }
    }
}


[GoType("dyn")] partial struct cutSuffixTestsᴛ1 {
    internal @string s, sep;
    internal @string before;
    internal bool found;
}
internal static slice<cutSuffixTestsᴛ1> cutSuffixTests = new cutSuffixTestsᴛ1[]{
    new("abc"u8, "bc"u8, "a"u8, true),
    new("abc"u8, "abc"u8, ""u8, true),
    new("abc"u8, ""u8, "abc"u8, true),
    new("abc"u8, "d"u8, "abc"u8, false),
    new(""u8, "d"u8, ""u8, false),
    new(""u8, ""u8, ""u8, true)
}.slice();

public static void TestCutSuffix(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in cutSuffixTests) {
        {
            var (before, found) = CutSuffix(slice<byte>(tt.s), slice<byte>(tt.sep)); if (((sstring)before) != tt.before || found != tt.found) {
                Ꮡt.Errorf("CutSuffix(%q, %q) = %q, %v, want %q, %v"u8, tt.s, tt.sep, before, found, tt.before, tt.found);
            }
        }
    }
}

public static void TestBufferGrowNegative(ж<testing.T> Ꮡt) => func((defer, recover) => {
    defer(() => {
        {
            var err = recover(); if (err == default!) {
                Ꮡt.Fatal("Grow(-1) should have panicked");
            }
        }
    });
    bytes.Buffer b = default!;
    b.Grow(-1);
});

public static void TestBufferTruncateNegative(ж<testing.T> Ꮡt) => func((defer, recover) => {
    defer(() => {
        {
            var err = recover(); if (err == default!) {
                Ꮡt.Fatal("Truncate(-1) should have panicked");
            }
        }
    });
    bytes.Buffer b = default!;
    b.Truncate(-1);
});

public static void TestBufferTruncateOutOfRange(ж<testing.T> Ꮡt) => func((defer, recover) => {
    defer(() => {
        {
            var err = recover(); if (err == default!) {
                Ꮡt.Fatal("Truncate(20) should have panicked");
            }
        }
    });
    bytes.Buffer b = default!;
    b.Write(new slice<byte>(10));
    b.Truncate(20);
});


[GoType("dyn")] partial struct containsTestsᴛ1 {
    internal slice<byte> b, subslice;
    internal bool want;
}
internal static slice<containsTestsᴛ1> containsTests = new containsTestsᴛ1[]{
    new(slice<byte>("hello"u8), slice<byte>("hel"u8), true),
    new(slice<byte>("日本語"u8), slice<byte>("日本"u8), true),
    new(slice<byte>("hello"u8), slice<byte>("Hello, world"u8), false),
    new(slice<byte>("東京"u8), slice<byte>("京東"u8), false)
}.slice();

public static void TestContains(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in containsTests) {
        {
            var got = Contains(tt.b, tt.subslice); if (got != tt.want) {
                Ꮡt.Errorf("Contains(%q, %q) = %v, want %v"u8, tt.b, tt.subslice, got, tt.want);
            }
        }
    }
}


[GoType("dyn")] partial struct ContainsAnyTestsᴛ1 {
    internal slice<byte> b;
    internal @string substr;
    internal bool expected;
}
public static slice<ContainsAnyTestsᴛ1> ContainsAnyTests = new ContainsAnyTestsᴛ1[]{
    new(slice<byte>(""u8), ""u8, false),
    new(slice<byte>(""u8), "a"u8, false),
    new(slice<byte>(""u8), "abc"u8, false),
    new(slice<byte>("a"u8), ""u8, false),
    new(slice<byte>("a"u8), "a"u8, true),
    new(slice<byte>("aaa"u8), "a"u8, true),
    new(slice<byte>("abc"u8), "xyz"u8, false),
    new(slice<byte>("abc"u8), "xcz"u8, true),
    new(slice<byte>("a☺b☻c☹d"u8), "uvw☻xyz"u8, true),
    new(slice<byte>("aRegExp*"u8), ".(|)*+?^$[]"u8, true),
    new(slice<byte>(dots + dots + dots), " "u8, false)
}.slice();

public static void TestContainsAny(ж<testing.T> Ꮡt) {
    foreach (var (_, ct) in ContainsAnyTests) {
        if (ContainsAny(ct.b, ct.substr) != ct.expected) {
            Ꮡt.Errorf("ContainsAny(%s, %s) = %v, want %v"u8,
                ct.b, ct.substr, !ct.expected, ct.expected);
        }
    }
}


[GoType("dyn")] partial struct ContainsRuneTestsᴛ1 {
    internal slice<byte> b;
    internal rune r;
    internal bool expected;
}
public static slice<ContainsRuneTestsᴛ1> ContainsRuneTests = new ContainsRuneTestsᴛ1[]{
    new(slice<byte>(""u8), (rune)'a', false),
    new(slice<byte>("a"u8), (rune)'a', true),
    new(slice<byte>("aaa"u8), (rune)'a', true),
    new(slice<byte>("abc"u8), (rune)'y', false),
    new(slice<byte>("abc"u8), (rune)'c', true),
    new(slice<byte>("a☺b☻c☹d"u8), (rune)'x', false),
    new(slice<byte>("a☺b☻c☹d"u8), (rune)'☻', true),
    new(slice<byte>("aRegExp*"u8), (rune)'*', true)
}.slice();

public static void TestContainsRune(ж<testing.T> Ꮡt) {
    foreach (var (_, ct) in ContainsRuneTests) {
        if (ContainsRune(ct.b, ct.r) != ct.expected) {
            Ꮡt.Errorf("ContainsRune(%q, %q) = %v, want %v"u8,
                ct.b, ct.r, !ct.expected, ct.expected);
        }
    }
}

public static void TestContainsFunc(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in ContainsRuneTests) {
        ref var ct = ref heap(new ContainsRuneTestsᴛ1(), out var Ꮡct);
        ct = vᴛ1;

        var ctʗ1 = ct;
        if (ContainsFunc(ct.b, (rune r) => ctʗ1.r == r) != ct.expected) {
            Ꮡt.Errorf("ContainsFunc(%q, func(%q)) = %v, want %v"u8,
                ct.b, ct.r, !ct.expected, ct.expected);
        }
    }
}

// Input is ~10% space, ~10% 2-byte UTF-8, rest ASCII non-space.
internal static Func<slice<byte>> makeFieldsInput = () => {
    var x = new slice<byte>((1 << (int)(20)));
    foreach (var (i, _) in x) {
        var exprᴛ1 = rand.Intn(10);
        var matchᴛ1 = false;
        if (exprᴛ1 is 0) { matchᴛ1 = true;
            x[i] = (rune)' ';
        }
        else if (exprᴛ1 is 1) { matchᴛ1 = true;
            do {
                if (i > 0 && x[i - 1] == (rune)'x') {
                    copy(x[(int)(i - 1)..], "χ"u8);
                    break;
                }
            } while (false);
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1) { /* default: */
            x[i] = (rune)'x';
        }

    }
    return x;
};

// Input is ~10% space, rest ASCII non-space.
internal static Func<slice<byte>> makeFieldsInputASCII = () => {
    var x = new slice<byte>((1 << (int)(20)));
    foreach (var (i, _) in x) {
        if (rand.Intn(10) == 0){
            x[i] = (rune)' ';
        } else {
            x[i] = (rune)'x';
        }
    }
    return x;
};


[GoType("dyn")] partial struct bytesdataᴛ1 {
    internal @string name;
    internal slice<byte> data;
}
internal static slice<bytesdataᴛ1> bytesdata = new bytesdataᴛ1[]{
    new("ASCII"u8, makeFieldsInputASCII()),
    new("Mixed"u8, makeFieldsInput())
}.slice();

public static void BenchmarkFields(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in bytesdata) {
        ref var sd = ref heap(new bytesdataᴛ1(), out var Ꮡsd);
        sd = vᴛ1;

        var sdʗ1 = sd;
        Ꮡb.Run(sd.name, (ж<testing.B> bΔ1) => {
            for (nint jᴛ1 = (1 << (int)(4)); jᴛ1 <= (1 << (int)(20)); jᴛ1 <<= (int)(4)) {
                var j = jᴛ1;
                var sdʗ2 = sdʗ1;
                bΔ1.Run(fmt.Sprintf("%d"u8, j), (ж<testing.B> bΔ2) => {
                    bΔ2.ReportAllocs();
                    bΔ2.SetBytes((int64)j);
                    var data = sdʗ2.data[..(int)(j)];
                    for (nint i = 0; i < (~bΔ2).N; i++) {
                        Fields(data);
                    }
                });
            }
        });
    }
}

public static void BenchmarkFieldsFunc(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in bytesdata) {
        ref var sd = ref heap(new bytesdataᴛ1(), out var Ꮡsd);
        sd = vᴛ1;

        var sdʗ1 = sd;
        Ꮡb.Run(sd.name, (ж<testing.B> bΔ1) => {
            for (nint jᴛ1 = (1 << (int)(4)); jᴛ1 <= (1 << (int)(20)); jᴛ1 <<= (int)(4)) {
                var j = jᴛ1;
                var sdʗ2 = sdʗ1;
                bΔ1.Run(fmt.Sprintf("%d"u8, j), (ж<testing.B> bΔ2) => {
                    bΔ2.ReportAllocs();
                    bΔ2.SetBytes((int64)j);
                    var data = sdʗ2.data[..(int)(j)];
                    for (nint i = 0; i < (~bΔ2).N; i++) {
                        FieldsFunc(data, Δunicode.IsSpace);
                    }
                });
            }
        });
    }
}

[GoType("dyn")] partial struct BenchmarkTrimSpace_tests {
    internal @string name;
    internal slice<byte> input;
}

public static void BenchmarkTrimSpace(ж<testing.B> Ꮡb) {
    var tests = new BenchmarkTrimSpace_tests[]{
        new("NoTrim"u8, slice<byte>("typical"u8)),
        new("ASCII"u8, slice<byte>("  foo bar  "u8)),
        new("SomeNonASCII"u8, slice<byte>("    \u2000\t\r\n x\t\t\r\r\ny\n \u3000    "u8)),
        new("JustNonASCII"u8, slice<byte>("\u2000\u2000\u2000☺☺☺☺\u3000\u3000\u3000"u8))
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new BenchmarkTrimSpace_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡb.Run(test.name, (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                TrimSpace(testʗ1.input);
            }
        });
    }
}

[GoType("dyn")] partial struct BenchmarkToValidUTF8_tests {
    internal @string name;
    internal slice<byte> input;
}

public static void BenchmarkToValidUTF8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    var tests = new BenchmarkToValidUTF8_tests[]{
        new("Valid"u8, slice<byte>("typical"u8)),
        new("InvalidASCII"u8, slice<byte>(((@string)(new byte[]{0x66, 0x6f, 0x6f, 0xff, 0x62, 0x61, 0x72})))),
        new("InvalidNonASCII"u8, slice<byte>(((@string)(new byte[]{0xe6, 0x97, 0xa5, 0xe6, 0x9c, 0xac, 0xe8, 0xaa, 0x9e, 0xff, 0xe6, 0x97, 0xa5, 0xe6, 0x9c, 0xac, 0xe8, 0xaa, 0x9e}))))
    }.slice();
    var replacement = slice<byte>("\uFFFD"u8);
    b.ResetTimer();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new BenchmarkToValidUTF8_tests(), out var Ꮡtest);
        test = vᴛ1;

        var replacementʗ1 = replacement;
        var testʗ1 = test;
        Ꮡb.Run(test.name, (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                ToValidUTF8(testʗ1.input, replacementʗ1);
            }
        });
    }
}

internal static slice<byte> makeBenchInputHard() {
    var tokens = new @string[]{
        "<a>", "<p>", "<b>", "<strong>",
        "</a>", "</p>", "</b>", "</strong>",
        "hello", "world"
    }.array();
    var x = new slice<byte>(0, (1 << (int)(20)));
    while (ᐧ) {
        nint i = rand.Intn(len(tokens));
        if (len(x) + len(tokens[i]) >= (1 << (int)(20))) {
            break;
        }
        x = append(x, tokens[i].ꓸꓸꓸ);
    }
    return x;
}

internal static slice<byte> benchInputHard = makeBenchInputHard();

internal static void benchmarkIndexHard(ж<testing.B> Ꮡb, slice<byte> sep) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Index(benchInputHard, sep);
    }
}

internal static void benchmarkLastIndexHard(ж<testing.B> Ꮡb, slice<byte> sep) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        LastIndex(benchInputHard, sep);
    }
}

internal static void benchmarkCountHard(ж<testing.B> Ꮡb, slice<byte> sep) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Count(benchInputHard, sep);
    }
}

public static void BenchmarkIndexHard1(ж<testing.B> Ꮡb) {
    benchmarkIndexHard(Ꮡb, slice<byte>("<>"u8));
}

public static void BenchmarkIndexHard2(ж<testing.B> Ꮡb) {
    benchmarkIndexHard(Ꮡb, slice<byte>("</pre>"u8));
}

public static void BenchmarkIndexHard3(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    benchmarkIndexHard(Ꮡb, slice<byte>("<b>hello world</b>"u8));
}

public static void BenchmarkIndexHard4(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    benchmarkIndexHard(Ꮡb, slice<byte>("<pre><b>hello</b><strong>world</strong></pre>"u8));
}

public static void BenchmarkLastIndexHard1(ж<testing.B> Ꮡb) {
    benchmarkLastIndexHard(Ꮡb, slice<byte>("<>"u8));
}

public static void BenchmarkLastIndexHard2(ж<testing.B> Ꮡb) {
    benchmarkLastIndexHard(Ꮡb, slice<byte>("</pre>"u8));
}

public static void BenchmarkLastIndexHard3(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    benchmarkLastIndexHard(Ꮡb, slice<byte>("<b>hello world</b>"u8));
}

public static void BenchmarkCountHard1(ж<testing.B> Ꮡb) {
    benchmarkCountHard(Ꮡb, slice<byte>("<>"u8));
}

public static void BenchmarkCountHard2(ж<testing.B> Ꮡb) {
    benchmarkCountHard(Ꮡb, slice<byte>("</pre>"u8));
}

public static void BenchmarkCountHard3(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    benchmarkCountHard(Ꮡb, slice<byte>("<b>hello world</b>"u8));
}

public static void BenchmarkSplitEmptySeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Split(benchInputHard, default!);
    }
}

public static void BenchmarkSplitSingleByteSeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    var sep = slice<byte>("/"u8);
    for (nint i = 0; i < b.N; i++) {
        Split(benchInputHard, sep);
    }
}

public static void BenchmarkSplitMultiByteSeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    var sep = slice<byte>("hello"u8);
    for (nint i = 0; i < b.N; i++) {
        Split(benchInputHard, sep);
    }
}

public static void BenchmarkSplitNSingleByteSeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    var sep = slice<byte>("/"u8);
    for (nint i = 0; i < b.N; i++) {
        SplitN(benchInputHard, sep, 10);
    }
}

public static void BenchmarkSplitNMultiByteSeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    var sep = slice<byte>("hello"u8);
    for (nint i = 0; i < b.N; i++) {
        SplitN(benchInputHard, sep, 10);
    }
}

public static void BenchmarkRepeat(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < b.N; i++) {
        Repeat(slice<byte>("-"u8), 80);
    }
}

public static void BenchmarkRepeatLarge(ж<testing.B> Ꮡb) {
    var s = Repeat(slice<byte>("@"u8), 8 * 1024);
    for (nint j = 8; j <= 30; j++) {
        foreach (var (_, k) in new nint[]{1, 16, 4097}.slice()) {
            var sΔ1 = s[..(int)(k)];
            nint n = ((1 << (int)(j))) / k;
            if (n == 0) {
                continue;
            }
            var sʗ1 = sΔ1;
            Ꮡb.Run(fmt.Sprintf("%d/%d"u8, (1 << (int)(j)), k), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    Repeat(sʗ1, n);
                }
                bΔ1.SetBytes((int64)(n * len(sʗ1)));
            });
        }
    }
}

public static void BenchmarkBytesCompare(ж<testing.B> Ꮡb) {
    for (nint nᴛ1 = 1; nᴛ1 <= 2048; nᴛ1 <<= (int)(1)) {
        var n = nᴛ1;
        Ꮡb.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            slice<byte> x = new slice<byte>(n);
            slice<byte> y = new slice<byte>(n);
            for (nint i = 0; i < n; i++) {
                x[i] = (rune)'a';
            }
            for (nint i = 0; i < n; i++) {
                y[i] = (rune)'a';
            }
            bΔ1.ResetTimer();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                Compare(x, y);
            }
        });
    }
}

public static void BenchmarkIndexAnyASCII(ж<testing.B> Ꮡb) {
    var x = Repeat(new byte[]{(rune)'#'}.slice(), 2048);
    // Never matches set
    @string cs = "0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz"u8;
    for (nint kᴛ1 = 1; kᴛ1 <= 2048; kᴛ1 <<= (int)(4)) {
        var k = kᴛ1;
        for (nint jᴛ1 = 1; jᴛ1 <= 64; jᴛ1 <<= (int)(1)) {
            var j = jᴛ1;
            var xʗ1 = x;
            Ꮡb.Run(fmt.Sprintf("%d:%d"u8, k, j), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    IndexAny(xʗ1[..(int)(k)], cs[..(int)(j)]);
                }
            });
        }
    }
}

public static void BenchmarkIndexAnyUTF8(ж<testing.B> Ꮡb) {
    var x = Repeat(new byte[]{(rune)'#'}.slice(), 2048);
    // Never matches set
    @string cs = "你好世界, hello world. 你好世界, hello world. 你好世界, hello world."u8;
    for (nint kᴛ1 = 1; kᴛ1 <= 2048; kᴛ1 <<= (int)(4)) {
        var k = kᴛ1;
        for (nint jᴛ1 = 1; jᴛ1 <= 64; jᴛ1 <<= (int)(1)) {
            var j = jᴛ1;
            var xʗ1 = x;
            Ꮡb.Run(fmt.Sprintf("%d:%d"u8, k, j), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    IndexAny(xʗ1[..(int)(k)], cs[..(int)(j)]);
                }
            });
        }
    }
}

public static void BenchmarkLastIndexAnyASCII(ж<testing.B> Ꮡb) {
    var x = Repeat(new byte[]{(rune)'#'}.slice(), 2048);
    // Never matches set
    @string cs = "0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz"u8;
    for (nint kᴛ1 = 1; kᴛ1 <= 2048; kᴛ1 <<= (int)(4)) {
        var k = kᴛ1;
        for (nint jᴛ1 = 1; jᴛ1 <= 64; jᴛ1 <<= (int)(1)) {
            var j = jᴛ1;
            var xʗ1 = x;
            Ꮡb.Run(fmt.Sprintf("%d:%d"u8, k, j), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    LastIndexAny(xʗ1[..(int)(k)], cs[..(int)(j)]);
                }
            });
        }
    }
}

public static void BenchmarkLastIndexAnyUTF8(ж<testing.B> Ꮡb) {
    var x = Repeat(new byte[]{(rune)'#'}.slice(), 2048);
    // Never matches set
    @string cs = "你好世界, hello world. 你好世界, hello world. 你好世界, hello world."u8;
    for (nint kᴛ1 = 1; kᴛ1 <= 2048; kᴛ1 <<= (int)(4)) {
        var k = kᴛ1;
        for (nint jᴛ1 = 1; jᴛ1 <= 64; jᴛ1 <<= (int)(1)) {
            var j = jᴛ1;
            var xʗ1 = x;
            Ꮡb.Run(fmt.Sprintf("%d:%d"u8, k, j), (ж<testing.B> bΔ1) => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    LastIndexAny(xʗ1[..(int)(k)], cs[..(int)(j)]);
                }
            });
        }
    }
}

public static void BenchmarkTrimASCII(ж<testing.B> Ꮡb) {
    @string cs = "0123456789abcdef"u8;
    for (nint kᴛ1 = 1; kᴛ1 <= 4096; kᴛ1 <<= (int)(4)) {
        var k = kᴛ1;
        for (nint jᴛ1 = 1; jᴛ1 <= 16; jᴛ1 <<= (int)(1)) {
            var j = jᴛ1;
            Ꮡb.Run(fmt.Sprintf("%d:%d"u8, k, j), (ж<testing.B> bΔ1) => {
                var x = Repeat(slice<byte>(cs[..(int)(j)]), k);
                // Always matches set
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    Trim(x[..(int)(k)], cs[..(int)(j)]);
                }
            });
        }
    }
}

public static void BenchmarkTrimByte(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    var x = slice<byte>("  the quick brown fox   "u8);
    for (nint i = 0; i < b.N; i++) {
        Trim(x, " "u8);
    }
}

public static void BenchmarkIndexPeriodic(ж<testing.B> Ꮡb) {
    var key = new byte[]{1, 1}.slice();
    foreach (var (_, skip) in new nint[]{2, 4, 8, 16, 32, 64}.array()) {
        var keyʗ1 = key;
        Ꮡb.Run(fmt.Sprintf("IndexPeriodic%d"u8, skip), (ж<testing.B> bΔ1) => {
            var buf = new slice<byte>((1 << (int)(16)));
            for (nint i = 0; i < len(buf); i += skip) {
                buf[i] = 1;
            }
            for (nint i = 0; i < (~bΔ1).N; i++) {
                Index(buf, keyʗ1);
            }
        });
    }
}

public static void TestClone(ж<testing.T> Ꮡt) {
    slice<slice<byte>> cloneTests = new slice<byte>[]{
        slice<byte>(default!),
        new byte[]{}.slice(),
        Clone(new byte[]{}.slice()),
        slice<byte>(strings.Repeat("a"u8, 42))[..0],
        slice<byte>(strings.Repeat("a"u8, 42)).slice(-1, 0, 0),
        slice<byte>("short"u8),
        slice<byte>(strings.Repeat("a"u8, 42))
    }.slice();
    foreach (var (_, input) in cloneTests) {
        var clone = Clone(input);
        if (!Equal(clone, input)) {
            Ꮡt.Errorf("Clone(%q) = %q; want %q"u8, input, clone, input);
        }
        if (input == default! && clone != default!) {
            Ꮡt.Errorf("Clone(%#v) return value should be equal to nil slice."u8, input);
        }
        if (input != default! && clone == default!) {
            Ꮡt.Errorf("Clone(%#v) return value should not be equal to nil slice."u8, input);
        }
        if (cap(input) != 0 && @unsafe.SliceData(input) == @unsafe.SliceData(clone)) {
            Ꮡt.Errorf("Clone(%q) return value should not reference inputs backing memory."u8, input);
        }
    }
}

} // end bytes_test_package
