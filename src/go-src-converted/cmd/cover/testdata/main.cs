// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test runner for coverage test. This file is not coverage-annotated; test.go is.
// It knows the coverage counter is called
// "thisNameMustBeVeryLongToCauseOverflowOfCounterIncrementStatementOntoNextLineForTest".

// package main -- go2cs converted at 2022 March 13 06:28:40 UTC
// Original source: C:\Program Files\Go\src\cmd\cover\testdata\main.go
namespace go;

using fmt = fmt_package;
using os = os_package;

public static partial class main_package {

private static void Main() {
    testAll();
    verify();
}

private partial struct block {
    public uint count;
    public uint line;
}

private static var counters = make_map<block, bool>();

// shorthand for the long counter variable.
private static var coverTest = _addr_thisNameMustBeVeryLongToCauseOverflowOfCounterIncrementStatementOntoNextLineForTest;

// check records the location and expected value for a counter.
private static void check(uint line, uint count) {
    block b = new block(count,line,);
    counters[b] = true;
}

// checkVal is a version of check that returns its extra argument,
// so it can be used in conditionals.
private static nint checkVal(uint line, uint count, nint val) {
    block b = new block(count,line,);
    counters[b] = true;
    return val;
}

public static var PASS = true;

// verify checks the expected counts against the actual. It runs after the test has completed.
private static void verify() {
    foreach (var (b) in counters) {
        var (got, index) = count(b.line);
        if (b.count == anything && got != 0) {
            got = anything;
        }
        if (got != b.count) {
            fmt.Fprintf(os.Stderr, "test_go:%d expected count %d got %d [counter %d]\n", b.line, b.count, got, index);
            PASS = false;
        }
    }    verifyPanic();
    if (!PASS) {
        fmt.Fprintf(os.Stderr, "FAIL\n");
        os.Exit(2);
    }
}

// verifyPanic is a special check for the known counter that should be
// after the panic call in testPanic.
private static void verifyPanic() {
    if (coverTest.Count[panicIndex - 1] != 1) { 
        // Sanity check for test before panic.
        fmt.Fprintf(os.Stderr, "bad before panic");
        PASS = false;
    }
    if (coverTest.Count[panicIndex] != 0) {
        fmt.Fprintf(os.Stderr, "bad at panic: %d should be 0\n", coverTest.Count[panicIndex]);
        PASS = false;
    }
    if (coverTest.Count[panicIndex + 1] != 1) {
        fmt.Fprintf(os.Stderr, "bad after panic");
        PASS = false;
    }
}

// count returns the count and index for the counter at the specified line.
private static (uint, nint) count(uint line) {
    uint _p0 = default;
    nint _p0 = default;
 
    // Linear search is fine. Choose perfect fit over approximate.
    // We can have a closing brace for a range on the same line as a condition for an "else if"
    // and we don't want that brace to steal the count for the condition on the "if".
    // Therefore we test for a perfect (lo==line && hi==line) match, but if we can't
    // find that we take the first imperfect match.
    nint index = -1;
    var indexLo = uint32(1e9F);
    foreach (var (i) in coverTest.Count) {
        var lo = coverTest.Pos[3 * i];
        var hi = coverTest.Pos[3 * i + 1];
        if (lo == line && line == hi) {
            return (coverTest.Count[i], i);
        }
        if (lo <= line && line <= hi && indexLo > lo) {
            index = i;
            indexLo = lo;
        }
    }    if (index == -1) {
        fmt.Fprintln(os.Stderr, "cover_test: no counter for line", line);
        PASS = false;
        return (0, 0);
    }
    return (coverTest.Count[index], index);
}

} // end main_package
