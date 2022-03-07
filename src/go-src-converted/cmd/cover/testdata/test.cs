// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This program is processed by the cover command, and then testAll is called.
// The test driver in main.go can then compare the coverage statistics with expectation.

// The word LINE is replaced by the line number in this file. When the file is executed,
// the coverage processing has changed the line numbers, so we can't use runtime.Caller.

// package main -- go2cs converted at 2022 March 06 23:15:12 UTC
// Original source: C:\Program Files\Go\src\cmd\cover\testdata\test.go
using _@unsafe_ = go.@unsafe_package;
using System;
using System.Threading;


namespace go;

public static partial class main_package {
 // for go:linkname

    //go:linkname some_name some_name
private static readonly float anything = 1e9F; // Just some unlikely value that means "we got here, don't care how often"

 // Just some unlikely value that means "we got here, don't care how often"

private static void testAll() {
    testSimple();
    testBlockRun();
    testIf();
    testFor();
    testRange();
    testSwitch();
    testTypeSwitch();
    testSelect1();
    testSelect2();
    testPanic();
    testEmptySwitches();
    testFunctionLiteral();
    testGoto();
}

// The indexes of the counters in testPanic are known to main.go
private static readonly nint panicIndex = 3;

// This test appears first because the index of its counters is known to main.go


// This test appears first because the index of its counters is known to main.go
private static void testPanic() => func((defer, panic, recover) => {
    defer(() => {
        recover();
    }());
    check(LINE, 1);
    panic("should not get next line");
    check(LINE, 0); // this is GoCover.Count[panicIndex]
    // The next counter is in testSimple and it will be non-zero.
    // If the panic above does not trigger a counter, the test will fail
    // because GoCover.Count[panicIndex] will be the one in testSimple.
});

private static void testSimple() {
    check(LINE, 1);
}

private static void testIf() {
    if (true) {
        check(LINE, 1);
    }
    else
 {
        check(LINE, 0);
    }
    if (false) {
        check(LINE, 0);
    }
    else
 {
        check(LINE, 1);
    }
    {
        nint i__prev1 = i;

        for (nint i = 0; i < 3; i++) {
            if (checkVal(LINE, 3, i) <= 2) {
                check(LINE, 3);
            }
            if (checkVal(LINE, 3, i) <= 1) {
                check(LINE, 2);
            }
            if (checkVal(LINE, 3, i) <= 0) {
                check(LINE, 1);
            }
        }

        i = i__prev1;
    }
    {
        nint i__prev1 = i;

        for (i = 0; i < 3; i++) {
            if (checkVal(LINE, 3, i) <= 1) {
                check(LINE, 2);
            }
            else
 {
                check(LINE, 1);
            }

        }

        i = i__prev1;
    }
    {
        nint i__prev1 = i;

        for (i = 0; i < 3; i++) {
            if (checkVal(LINE, 3, i) <= 0) {
                check(LINE, 1);
            }
            else if (checkVal(LINE, 2, i) <= 1) {
                check(LINE, 1);
            }
            else if (checkVal(LINE, 1, i) <= 2) {
                check(LINE, 1);
            }
            else if (checkVal(LINE, 0, i) <= 3) {
                check(LINE, 0);
            }

        }

        i = i__prev1;
    }
    if ((a, b) => a < b(3, 4)) {
        check(LINE, 1);
    }
}

private static void testFor() {
    for (nint i = 0; i < 10; () => {
        i++;

        check(LINE, 10);
    }()) {
        check(LINE, 10);
    }

}

private static void testRange() {
    foreach (var (_, f) in new slice<Action>(new Action[] { func(){check(LINE,1)} })) {
        f();
        check(LINE, 1);
    }
}

private static void testBlockRun() {
    check(LINE, 1);
 {
        check(LINE, 1);
    } {
        check(LINE, 1);
    }    check(LINE, 1);
 {
        check(LINE, 1);
    } {
        check(LINE, 1);
    }    check(LINE, 1);
}

private static void testSwitch() {
    for (nint i = 0; i < 5; () => {
        i++;

        check(LINE, 5);
    }()) {
        goto label2;
label1:
        goto label1;
label2:
        switch (i) {
            case 0: 
                check(LINE, 1);
                break;
            case 1: 
                check(LINE, 1);
                break;
            case 2: 
                check(LINE, 1);
                break;
            default: 
                check(LINE, 2);
                break;
        }

    }

}

private static void testTypeSwitch() {

    foreach (var (_, v) in x) {
            () => {
                check(LINE, 3);
            }();

        switch (v.type()) {
            case nint _:
                check(LINE, 1);
                break;
            case double _:
                check(LINE, 1);
                break;
            case @string _:
                check(LINE, 1);
                break;
            case System.Numerics.Complex128 _:
                check(LINE, 0);
                break;
            default:
            {
                check(LINE, 0);
                break;
            }
        }

    }
}

private static void testSelect1() {
    var c = make_channel<nint>();
    go_(() => () => {
        for (nint i = 0; i < 1000; i++) {
            c.Send(i);
        }
    }());
    while (true) {
        check(LINE, anything);
        check(LINE, anything);
        check(LINE, 1);
        return ;
    }
}

private static void testSelect2() {
    var c1 = make_channel<nint>(1000);
    var c2 = make_channel<nint>(1000);
    for (nint i = 0; i < 1000; i++) {
        c1.Send(i);
        c2.Send(i);
    }
    while (true) {
        check(LINE, 1000);
        check(LINE, 1000);
        check(LINE, 1);
        return ;
    }
}

// Empty control statements created syntax errors. This function
// is here just to be sure that those are handled correctly now.
private static void testEmptySwitches() {
    check(LINE, 1);
    switch (3) {
    }
    check(LINE, 1);
    {
        nint i = ._<nint>();

        switch (i) {
        }
    }
    check(LINE, 1);
    var c = make_channel<nint>();
    go_(() => () => {
        check(LINE, 1);
        c.Send(1);
    }());
    c.Receive();
    check(LINE, 1);

}

private static void testFunctionLiteral() => func((_, panic, _) => {
    Func<Action, error> a = f => {
        f();
        f();
        return null;
    };

    Func<Action, bool> b = f => {
        f();
        f();
        return true;
    };

    check(LINE, 1);
    a(() => {
        check(LINE, 2);
    });

    {
        var err = a(() => {
            check(LINE, 2);
        });

        if (err != null)         }
    }


    switch (b(() => {
        check(LINE, 2);
    })) {
    }

    nint x = 2;

    if (x == () => {
            check(LINE, 1);

            return 1;
        }()) 
        check(LINE, 0);
        panic("2=1");
    else if (x == () => {
            check(LINE, 1);

            return 2;
        }()) 
        check(LINE, 1);
    else if (x == () => {
            check(LINE, 0);

            return 3;
        }()) 
        check(LINE, 0);
        panic("2=3");
    
});

private static void testGoto() {
    for (nint i = 0; i < 2; i++) {
        if (i == 0) {
            goto Label;
        }
        check(LINE, 1);
Label:
        check(LINE, 2);

    } 
    // Now test that we don't inject empty statements
    // between a label and a loop.
loop:
    while (true) {
        check(LINE, 1);
        _breakloop = true;
        break;
    }

}

// This comment didn't appear in generated go code.
private static void haha() { 
    // Needed for cover to add counter increment here.
    _ = 42;

}

// Some someFunction.
//
//go:nosplit
private static void someFunction() {
}

} // end main_package
