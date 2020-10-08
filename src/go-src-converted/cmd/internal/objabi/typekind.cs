// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package objabi -- go2cs converted at 2020 October 08 03:50:15 UTC
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
        public static readonly long KindBool = (long)1L + iota;
        public static readonly var KindInt = (var)0;
        public static readonly var KindInt8 = (var)1;
        public static readonly var KindInt16 = (var)2;
        public static readonly var KindInt32 = (var)3;
        public static readonly var KindInt64 = (var)4;
        public static readonly var KindUint = (var)5;
        public static readonly var KindUint8 = (var)6;
        public static readonly var KindUint16 = (var)7;
        public static readonly var KindUint32 = (var)8;
        public static readonly var KindUint64 = (var)9;
        public static readonly var KindUintptr = (var)10;
        public static readonly var KindFloat32 = (var)11;
        public static readonly var KindFloat64 = (var)12;
        public static readonly var KindComplex64 = (var)13;
        public static readonly var KindComplex128 = (var)14;
        public static readonly var KindArray = (var)15;
        public static readonly var KindChan = (var)16;
        public static readonly var KindFunc = (var)17;
        public static readonly var KindInterface = (var)18;
        public static readonly var KindMap = (var)19;
        public static readonly var KindPtr = (var)20;
        public static readonly var KindSlice = (var)21;
        public static readonly var KindString = (var)22;
        public static readonly var KindStruct = (var)23;
        public static readonly KindDirectIface KindUnsafePointer = (KindDirectIface)1L << (int)(5L);
        public static readonly long KindGCProg = (long)1L << (int)(6L);
        public static readonly long KindMask = (long)(1L << (int)(5L)) - 1L;

    }
}}}
