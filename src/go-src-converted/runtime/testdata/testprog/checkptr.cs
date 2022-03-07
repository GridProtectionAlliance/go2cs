// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 22:25:57 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprog\checkptr.go
using runtime = go.runtime_package;
using time = go.time_package;
using @unsafe = go.@unsafe_package;
using System;
using System.Threading;


namespace go;

public static partial class main_package {

private static void init() {
    register("CheckPtrAlignmentNoPtr", CheckPtrAlignmentNoPtr);
    register("CheckPtrAlignmentPtr", CheckPtrAlignmentPtr);
    register("CheckPtrAlignmentNilPtr", CheckPtrAlignmentNilPtr);
    register("CheckPtrArithmetic", CheckPtrArithmetic);
    register("CheckPtrArithmetic2", CheckPtrArithmetic2);
    register("CheckPtrSize", CheckPtrSize);
    register("CheckPtrSmall", CheckPtrSmall);
    register("CheckPtrSliceOK", CheckPtrSliceOK);
    register("CheckPtrSliceFail", CheckPtrSliceFail);
}

public static void CheckPtrAlignmentNoPtr() {
    array<long> x = new array<long>(2);
    var p = @unsafe.Pointer(_addr_x[0]);
    sink2 = (int64.val)(@unsafe.Pointer(uintptr(p) + 1));
}

public static void CheckPtrAlignmentPtr() {
    array<long> x = new array<long>(2);
    var p = @unsafe.Pointer(_addr_x[0]);
    sink2 = (int64.val)(@unsafe.Pointer(uintptr(p) + 1));
}

// CheckPtrAlignmentNilPtr tests that checkptrAlignment doesn't crash
// on nil pointers (#47430).
public static void CheckPtrAlignmentNilPtr() {
    Action<nint> @do = default;
    do = n => { 
        // Inflate the stack so runtime.shrinkstack gets called during GC
        if (n > 0) {
            do(n - 1);
        }
        unsafe.Pointer p = default;
        _ = (int.val)(p);

    };

    go_(() => () => {
        while (true) {
            runtime.GC();
        }
    }());

    go_(() => () => {
        for (nint i = 0; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++) {
            do(i % 1024);
        }
    }());

    time.Sleep(time.Second);

}

public static void CheckPtrArithmetic() {
    ref nint x = ref heap(out ptr<nint> _addr_x);
    var i = uintptr(@unsafe.Pointer(_addr_x));
    sink2 = (int.val)(@unsafe.Pointer(i));
}

public static void CheckPtrArithmetic2() {
    array<long> x = new array<long>(2);
    var p = @unsafe.Pointer(_addr_x[1]);
    System.UIntPtr one = 1;
    sink2 = @unsafe.Pointer(uintptr(p) & ~one);
}

public static void CheckPtrSize() {
    ptr<int64> p = @new<int64>();
    sink2 = p;
    sink2 = new ptr<ptr<array<long>>>(@unsafe.Pointer(p));
}

public static void CheckPtrSmall() {
    sink2 = @unsafe.Pointer(uintptr(1));
}

public static void CheckPtrSliceOK() {
    ptr<var> p = @new<[4]int64>();
    sink2 = @unsafe.Slice(_addr_p[1], 3);
}

public static void CheckPtrSliceFail() {
    ptr<int64> p = @new<int64>();
    sink2 = p;
    sink2 = @unsafe.Slice(p, 100);
}

} // end main_package
