// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 08 04:31:37 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\types.go
using types = go.cmd.compile.@internal.types_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // convenience constants
        public static readonly var Txxx = (var)types.Txxx;

        public static readonly var TINT8 = (var)types.TINT8;
        public static readonly var TUINT8 = (var)types.TUINT8;
        public static readonly var TINT16 = (var)types.TINT16;
        public static readonly var TUINT16 = (var)types.TUINT16;
        public static readonly var TINT32 = (var)types.TINT32;
        public static readonly var TUINT32 = (var)types.TUINT32;
        public static readonly var TINT64 = (var)types.TINT64;
        public static readonly var TUINT64 = (var)types.TUINT64;
        public static readonly var TINT = (var)types.TINT;
        public static readonly var TUINT = (var)types.TUINT;
        public static readonly var TUINTPTR = (var)types.TUINTPTR;

        public static readonly var TCOMPLEX64 = (var)types.TCOMPLEX64;
        public static readonly var TCOMPLEX128 = (var)types.TCOMPLEX128;

        public static readonly var TFLOAT32 = (var)types.TFLOAT32;
        public static readonly var TFLOAT64 = (var)types.TFLOAT64;

        public static readonly var TBOOL = (var)types.TBOOL;

        public static readonly var TPTR = (var)types.TPTR;
        public static readonly var TFUNC = (var)types.TFUNC;
        public static readonly var TSLICE = (var)types.TSLICE;
        public static readonly var TARRAY = (var)types.TARRAY;
        public static readonly var TSTRUCT = (var)types.TSTRUCT;
        public static readonly var TCHAN = (var)types.TCHAN;
        public static readonly var TMAP = (var)types.TMAP;
        public static readonly var TINTER = (var)types.TINTER;
        public static readonly var TFORW = (var)types.TFORW;
        public static readonly var TANY = (var)types.TANY;
        public static readonly var TSTRING = (var)types.TSTRING;
        public static readonly var TUNSAFEPTR = (var)types.TUNSAFEPTR; 

        // pseudo-types for literals
        public static readonly var TIDEAL = (var)types.TIDEAL;
        public static readonly var TNIL = (var)types.TNIL;
        public static readonly var TBLANK = (var)types.TBLANK; 

        // pseudo-types for frame layout
        public static readonly var TFUNCARGS = (var)types.TFUNCARGS;
        public static readonly var TCHANARGS = (var)types.TCHANARGS;

        public static readonly var NTYPE = (var)types.NTYPE;

    }
}}}}
