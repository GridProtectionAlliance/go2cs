// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package objabi -- go2cs converted at 2020 August 29 08:46:20 UTC
// import "cmd/internal/objabi" ==> using objabi = go.cmd.@internal.objabi_package
// Original source: C:\Go\src\cmd\internal\objabi\typekind.go

using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class objabi_package
    {
        // Must match runtime and reflect.
        // Included by cmd/gc.
        public static readonly long KindBool = 1L + iota;
        public static readonly var KindInt = 0;
        public static readonly var KindInt8 = 1;
        public static readonly var KindInt16 = 2;
        public static readonly var KindInt32 = 3;
        public static readonly var KindInt64 = 4;
        public static readonly var KindUint = 5;
        public static readonly var KindUint8 = 6;
        public static readonly var KindUint16 = 7;
        public static readonly var KindUint32 = 8;
        public static readonly var KindUint64 = 9;
        public static readonly var KindUintptr = 10;
        public static readonly var KindFloat32 = 11;
        public static readonly var KindFloat64 = 12;
        public static readonly var KindComplex64 = 13;
        public static readonly var KindComplex128 = 14;
        public static readonly var KindArray = 15;
        public static readonly var KindChan = 16;
        public static readonly var KindFunc = 17;
        public static readonly var KindInterface = 18;
        public static readonly var KindMap = 19;
        public static readonly var KindPtr = 20;
        public static readonly var KindSlice = 21;
        public static readonly var KindString = 22;
        public static readonly var KindStruct = 23;
        public static readonly KindDirectIface KindUnsafePointer = 1L << (int)(5L);
        public static readonly long KindGCProg = 1L << (int)(6L);
        public static readonly long KindNoPointers = 1L << (int)(7L);
        public static readonly long KindMask = (1L << (int)(5L)) - 1L;
    }
}}}
