// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 22:26:03 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprog\map.go
using runtime = go.runtime_package;
using System;
using System.Threading;


namespace go;

public static partial class main_package {

private static void init() {
    register("concurrentMapWrites", concurrentMapWrites);
    register("concurrentMapReadWrite", concurrentMapReadWrite);
    register("concurrentMapIterateWrite", concurrentMapIterateWrite);
}

private static void concurrentMapWrites() {
    map m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<nint, nint>{};
    var c = make_channel<object>();
    go_(() => () => {
        {
            nint i__prev1 = i;

            for (nint i = 0; i < 10000; i++) {
                m[5] = 0;
                runtime.Gosched();
            }


            i = i__prev1;
        }
        c.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});

    }());
    go_(() => () => {
        {
            nint i__prev1 = i;

            for (i = 0; i < 10000; i++) {
                m[6] = 0;
                runtime.Gosched();
            }


            i = i__prev1;
        }
        c.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});

    }());
    c.Receive().Send(c);

}

private static void concurrentMapReadWrite() {
    map m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<nint, nint>{};
    var c = make_channel<object>();
    go_(() => () => {
        {
            nint i__prev1 = i;

            for (nint i = 0; i < 10000; i++) {
                m[5] = 0;
                runtime.Gosched();
            }


            i = i__prev1;
        }
        c.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});

    }());
    go_(() => () => {
        {
            nint i__prev1 = i;

            for (i = 0; i < 10000; i++) {
                _ = m[6];
                runtime.Gosched();
            }


            i = i__prev1;
        }
        c.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});

    }());
    c.Receive().Send(c);

}

private static void concurrentMapIterateWrite() {
    map m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<nint, nint>{};
    var c = make_channel<object>();
    go_(() => () => {
        {
            nint i__prev1 = i;

            for (nint i = 0; i < 10000; i++) {
                m[5] = 0;
                runtime.Gosched();
            }


            i = i__prev1;
        }
        c.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});

    }());
    go_(() => () => {
        {
            nint i__prev1 = i;

            for (i = 0; i < 10000; i++) {
                foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_2<< in m) {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_2<<
                }
                runtime.Gosched();
            }


            i = i__prev1;
        }
        c.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});

    }());
    c.Receive().Send(c);

}

} // end main_package
