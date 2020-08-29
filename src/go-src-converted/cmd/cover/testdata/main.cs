// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test runner for coverage test. This file is not coverage-annotated; test.go is.
// It knows the coverage counter is called
// "thisNameMustBeVeryLongToCauseOverflowOfCounterIncrementStatementOntoNextLineForTest".

// package main -- go2cs converted at 2020 August 29 09:59:31 UTC
// Original source: C:\Go\src\cmd\cover\testdata\main.go
using fmt = go.fmt_package;
using os = go.os_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            testAll();
            verify();
        }

        private partial struct block
        {
            public uint count;
            public uint line;
        }

        private static var counters = make_map<block, bool>();

        // shorthand for the long counter variable.
        private static var coverTest = ref thisNameMustBeVeryLongToCauseOverflowOfCounterIncrementStatementOntoNextLineForTest;

        // check records the location and expected value for a counter.
        private static void check(uint line, uint count)
        {
            block b = new block(count,line,);
            counters[b] = true;
        }

        // checkVal is a version of check that returns its extra argument,
        // so it can be used in conditionals.
        private static long checkVal(uint line, uint count, long val)
        {
            block b = new block(count,line,);
            counters[b] = true;
            return val;
        }

        public static var PASS = true;

        // verify checks the expected counts against the actual. It runs after the test has completed.
        private static void verify()
        {
            foreach (var (b) in counters)
            {
                var (got, index) = count(b.line);
                if (b.count == anything && got != 0L)
                {
                    got = anything;
                }
                if (got != b.count)
                {
                    fmt.Fprintf(os.Stderr, "test_go:%d expected count %d got %d [counter %d]\n", b.line, b.count, got, index);
                    PASS = false;
                }
            }
            verifyPanic();
            if (!PASS)
            {
                fmt.Fprintf(os.Stderr, "FAIL\n");
                os.Exit(2L);
            }
        }

        // verifyPanic is a special check for the known counter that should be
        // after the panic call in testPanic.
        private static void verifyPanic()
        {
            if (coverTest.Count[panicIndex - 1L] != 1L)
            { 
                // Sanity check for test before panic.
                fmt.Fprintf(os.Stderr, "bad before panic");
                PASS = false;
            }
            if (coverTest.Count[panicIndex] != 0L)
            {
                fmt.Fprintf(os.Stderr, "bad at panic: %d should be 0\n", coverTest.Count[panicIndex]);
                PASS = false;
            }
            if (coverTest.Count[panicIndex + 1L] != 1L)
            {
                fmt.Fprintf(os.Stderr, "bad after panic");
                PASS = false;
            }
        }

        // count returns the count and index for the counter at the specified line.
        private static (uint, long) count(uint line)
        { 
            // Linear search is fine. Choose perfect fit over approximate.
            // We can have a closing brace for a range on the same line as a condition for an "else if"
            // and we don't want that brace to steal the count for the condition on the "if".
            // Therefore we test for a perfect (lo==line && hi==line) match, but if we can't
            // find that we take the first imperfect match.
            long index = -1L;
            var indexLo = uint32(1e9F);
            foreach (var (i) in coverTest.Count)
            {
                var lo = coverTest.Pos[3L * i];
                var hi = coverTest.Pos[3L * i + 1L];
                if (lo == line && line == hi)
                {
                    return (coverTest.Count[i], i);
                } 
                // Choose the earliest match (the counters are in unpredictable order).
                if (lo <= line && line <= hi && indexLo > lo)
                {
                    index = i;
                    indexLo = lo;
                }
            }
            if (index == -1L)
            {
                fmt.Fprintln(os.Stderr, "cover_test: no counter for line", line);
                PASS = false;
                return (0L, 0L);
            }
            return (coverTest.Count[index], index);
        }
    }
}
