// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:12:26 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\typekind.go


namespace go;

public static partial class runtime_package {

private static readonly nint kindBool = 1 + iota;
private static readonly var kindInt = 0;
private static readonly var kindInt8 = 1;
private static readonly var kindInt16 = 2;
private static readonly var kindInt32 = 3;
private static readonly var kindInt64 = 4;
private static readonly var kindUint = 5;
private static readonly var kindUint8 = 6;
private static readonly var kindUint16 = 7;
private static readonly var kindUint32 = 8;
private static readonly var kindUint64 = 9;
private static readonly var kindUintptr = 10;
private static readonly var kindFloat32 = 11;
private static readonly var kindFloat64 = 12;
private static readonly var kindComplex64 = 13;
private static readonly var kindComplex128 = 14;
private static readonly var kindArray = 15;
private static readonly var kindChan = 16;
private static readonly var kindFunc = 17;
private static readonly var kindInterface = 18;
private static readonly var kindMap = 19;
private static readonly var kindPtr = 20;
private static readonly var kindSlice = 21;
private static readonly var kindString = 22;
private static readonly var kindStruct = 23;
private static readonly kindDirectIface kindUnsafePointer = 1 << 5;
private static readonly nint kindGCProg = 1 << 6;
private static readonly nint kindMask = (1 << 5) - 1;


// isDirectIface reports whether t is stored directly in an interface value.
private static bool isDirectIface(ptr<_type> _addr_t) {
    ref _type t = ref _addr_t.val;

    return t.kind & kindDirectIface != 0;
}

} // end runtime_package
