// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains tests for the bool checker.

// package testdata -- go2cs converted at 2020 August 29 10:09:35 UTC
// import "cmd/vet/testdata" ==> using testdata = go.cmd.vet.testdata_package
// Original source: C:\Go\src\cmd\vet\testdata\bool.go
using io = go.io_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vet
{
    public static partial class testdata_package
    {
        public static void RatherStupidConditions()
        {
            Func<long> f = default;            Func<long> g = default;

            if (f() == 0L || f() == 0L)
            { // OK f might have side effects
            }
            {
                var v = f();
                var w = g();

                if (v == w || v == w)
                { // ERROR "redundant or: v == w || v == w"
                }
            }
            _ = f == null || f == null; // ERROR "redundant or: f == nil || f == nil"

            _ = i == byte(1L) || i == byte(1L); // TODO conversions are treated as if they may have side effects

            channel<long> c = default;
            _ = 0L == c || 0L == c.Receive().Receive(); // OK subsequent receives may yield different values
            {
                var i__prev1 = i;
                var j__prev1 = j;

                var i = c.Receive();
                var j = c.Receive();

                while (i == j || i == j)
                { // ERROR "redundant or: i == j || i == j"
                    i = c.Receive();
                j = c.Receive();
                }

                i = i__prev1;
                j = j__prev1;
            }

            i = default;            j = default;            long k = default;

            _ = i + 1L == 1L || i + 1L == 1L; // ERROR "redundant or: i\+1 == 1 || i\+1 == 1"
            _ = i == 1L || j + 1L == i || i == 1L; // ERROR "redundant or: i == 1 || i == 1"

            _ = i == 1L || i == 1L || f() == 1L; // ERROR "redundant or: i == 1 || i == 1"
            _ = i == 1L || f() == 1L || i == 1L; // OK f may alter i as a side effect
            _ = f() == 1L || i == 1L || i == 1L; // ERROR "redundant or: i == 1 || i == 1"

            // Test partition edge cases
            _ = f() == 1L || i == 1L || i == 1L || j == 1L; // ERROR "redundant or: i == 1 || i == 1"
            _ = f() == 1L || j == 1L || i == 1L || i == 1L; // ERROR "redundant or: i == 1 || i == 1"
            _ = i == 1L || f() == 1L || i == 1L || i == 1L; // ERROR "redundant or: i == 1 || i == 1"
            _ = i == 1L || i == 1L || f() == 1L || i == 1L; // ERROR "redundant or: i == 1 || i == 1"
            _ = i == 1L || i == 1L || j == 1L || f() == 1L; // ERROR "redundant or: i == 1 || i == 1"
            _ = j == 1L || i == 1L || i == 1L || f() == 1L; // ERROR "redundant or: i == 1 || i == 1"
            _ = i == 1L || f() == 1L || f() == 1L || i == 1L;

            _ = i == 1L || (i == 1L || i == 2L); // ERROR "redundant or: i == 1 || i == 1"
            _ = i == 1L || (f() == 1L || i == 1L); // OK f may alter i as a side effect
            _ = i == 1L || (i == 1L || f() == 1L); // ERROR "redundant or: i == 1 || i == 1"
            _ = i == 1L || (i == 2L || (i == 1L || i == 3L)); // ERROR "redundant or: i == 1 || i == 1"

            bool a = default;            bool b = default;

            _ = i == 1L || (a || (i == 1L || b)); // ERROR "redundant or: i == 1 || i == 1"

            // Check that all redundant ors are flagged
            _ = j == 0L || i == 1L || f() == 1L || j == 0L || i == 1L || i == 1L || i == 1L || j == 0L || k == 0L;

            _ = i == 1L * 2L * 3L || i == 1L * 2L * 3L; // ERROR "redundant or: i == 1\*2\*3 || i == 1\*2\*3"

            // These test that redundant, suspect expressions do not trigger multiple errors.
            _ = i != 0L || i != 0L; // ERROR "redundant or: i != 0 || i != 0"
            _ = i == 0L && i == 0L; // ERROR "redundant and: i == 0 && i == 0"

            // and is dual to or; check the basics and
            // let the or tests pull the rest of the weight.
            _ = 0L != c && 0L != c.Receive().Receive(); // OK subsequent receives may yield different values
            _ = f() != 0L && f() != 0L; // OK f might have side effects
            _ = f != null && f != null; // ERROR "redundant and: f != nil && f != nil"
            _ = i != 1L && i != 1L && f() != 1L; // ERROR "redundant and: i != 1 && i != 1"
            _ = i != 1L && f() != 1L && i != 1L; // OK f may alter i as a side effect
            _ = f() != 1L && i != 1L && i != 1L; // ERROR "redundant and: i != 1 && i != 1"
        }

        public static void RoyallySuspectConditions()
        {
            long i = default;            long j = default;



            _ = i == 0L || i == 1L; // OK
            _ = i != 0L || i != 1L; // ERROR "suspect or: i != 0 || i != 1"
            _ = i != 0L || 1L != i; // ERROR "suspect or: i != 0 || 1 != i"
            _ = 0L != i || 1L != i; // ERROR "suspect or: 0 != i || 1 != i"
            _ = 0L != i || i != 1L; // ERROR "suspect or: 0 != i || i != 1"

            _ = (0L != i) || i != 1L; // ERROR "suspect or: 0 != i || i != 1"

            _ = i + 3L != 7L || j + 5L == 0L || i + 3L != 9L; // ERROR "suspect or: i\+3 != 7 || i\+3 != 9"

            _ = i != 0L || j == 0L || i != 1L; // ERROR "suspect or: i != 0 || i != 1"

            _ = i != 0L || i != 1L << (int)(4L); // ERROR "suspect or: i != 0 || i != 1<<4"

            _ = i != 0L || j != 0L;
            _ = 0L != i || 0L != j;

            @string s = default;
            _ = s != "one" || s != "the other"; // ERROR "suspect or: s != .one. || s != .the other."

            _ = "et" != "alii" || "et" != "cetera"; // ERROR "suspect or: .et. != .alii. || .et. != .cetera."
            _ = "me gustas" != "tu" || "le gustas" != "tu"; // OK we could catch this case, but it's not worth the code

            error err = default;
            _ = err != null || err != io.EOF; // TODO catch this case?

            // Sanity check and.
            _ = i != 0L && i != 1L; // OK
            _ = i == 0L && i == 1L; // ERROR "suspect and: i == 0 && i == 1"
            _ = i == 0L && 1L == i; // ERROR "suspect and: i == 0 && 1 == i"
            _ = 0L == i && 1L == i; // ERROR "suspect and: 0 == i && 1 == i"
            _ = 0L == i && i == 1L; // ERROR "suspect and: 0 == i && i == 1"
        }
    }
}}}
