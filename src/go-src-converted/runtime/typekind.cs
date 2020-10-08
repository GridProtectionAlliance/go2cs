// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:24:20 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\typekind.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long kindBool = (long)1L + iota;
        private static readonly var kindInt = (var)0;
        private static readonly var kindInt8 = (var)1;
        private static readonly var kindInt16 = (var)2;
        private static readonly var kindInt32 = (var)3;
        private static readonly var kindInt64 = (var)4;
        private static readonly var kindUint = (var)5;
        private static readonly var kindUint8 = (var)6;
        private static readonly var kindUint16 = (var)7;
        private static readonly var kindUint32 = (var)8;
        private static readonly var kindUint64 = (var)9;
        private static readonly var kindUintptr = (var)10;
        private static readonly var kindFloat32 = (var)11;
        private static readonly var kindFloat64 = (var)12;
        private static readonly var kindComplex64 = (var)13;
        private static readonly var kindComplex128 = (var)14;
        private static readonly var kindArray = (var)15;
        private static readonly var kindChan = (var)16;
        private static readonly var kindFunc = (var)17;
        private static readonly var kindInterface = (var)18;
        private static readonly var kindMap = (var)19;
        private static readonly var kindPtr = (var)20;
        private static readonly var kindSlice = (var)21;
        private static readonly var kindString = (var)22;
        private static readonly var kindStruct = (var)23;
        private static readonly kindDirectIface kindUnsafePointer = (kindDirectIface)1L << (int)(5L);
        private static readonly long kindGCProg = (long)1L << (int)(6L);
        private static readonly long kindMask = (long)(1L << (int)(5L)) - 1L;


        // isDirectIface reports whether t is stored directly in an interface value.
        private static bool isDirectIface(ptr<_type> _addr_t)
        {
            ref _type t = ref _addr_t.val;

            return t.kind & kindDirectIface != 0L;
        }
    }
}
