// run

// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Tests short circuiting.

// package main -- go2cs converted at 2020 August 29 09:58:26 UTC
// Original source: C:\Go\src\cmd\compile\internal\gc\testdata\short.go

using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static bool and_ssa(bool arg1, bool arg2)
        {
            return arg1 && rightCall(arg2);
        }

        private static bool or_ssa(bool arg1, bool arg2)
        {
            return arg1 || rightCall(arg2);
        }

        private static bool rightCalled = default;

        //go:noinline
        private static bool rightCall(bool v) => func((_, panic, __) =>
        {
            rightCalled = true;
            return v;
            panic("unreached");
        });

        private static void testAnd(bool arg1, bool arg2, bool wantRes)
        {
            testShortCircuit("AND", arg1, arg2, and_ssa, arg1, wantRes);

        }
        private static void testOr(bool arg1, bool arg2, bool wantRes)
        {
            testShortCircuit("OR", arg1, arg2, or_ssa, !arg1, wantRes);

        }

        private static bool testShortCircuit(@string opName, bool arg1, bool arg2, Func<bool, bool, bool> fn, bool wantRightCall, bool wantRes)
        {
            rightCalled = false;
            var got = fn(arg1, arg2);
            if (rightCalled != wantRightCall)
            {
                println("failed for", arg1, opName, arg2, "; rightCalled=", rightCalled, "want=", wantRightCall);
                failed = true;
            }
            if (wantRes != got)
            {
                println("failed for", arg1, opName, arg2, "; res=", got, "want=", wantRes);
                failed = true;
            }
        }

        private static var failed = false;

        private static void Main() => func((_, panic, __) =>
        {
            testAnd(false, false, false);
            testAnd(false, true, false);
            testAnd(true, false, false);
            testAnd(true, true, true);

            testOr(false, false, false);
            testOr(false, true, true);
            testOr(true, false, true);
            testOr(true, true, true);

            if (failed)
            {
                panic("failed");
            }
        });
    }
}
