// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:29:47 UTC
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
        public static readonly var Txxx = types.Txxx;

        public static readonly var TINT8 = types.TINT8;
        public static readonly var TUINT8 = types.TUINT8;
        public static readonly var TINT16 = types.TINT16;
        public static readonly var TUINT16 = types.TUINT16;
        public static readonly var TINT32 = types.TINT32;
        public static readonly var TUINT32 = types.TUINT32;
        public static readonly var TINT64 = types.TINT64;
        public static readonly var TUINT64 = types.TUINT64;
        public static readonly var TINT = types.TINT;
        public static readonly var TUINT = types.TUINT;
        public static readonly var TUINTPTR = types.TUINTPTR;

        public static readonly var TCOMPLEX64 = types.TCOMPLEX64;
        public static readonly var TCOMPLEX128 = types.TCOMPLEX128;

        public static readonly var TFLOAT32 = types.TFLOAT32;
        public static readonly var TFLOAT64 = types.TFLOAT64;

        public static readonly var TBOOL = types.TBOOL;

        public static readonly var TPTR32 = types.TPTR32;
        public static readonly var TPTR64 = types.TPTR64;

        public static readonly var TFUNC = types.TFUNC;
        public static readonly var TSLICE = types.TSLICE;
        public static readonly var TARRAY = types.TARRAY;
        public static readonly var TSTRUCT = types.TSTRUCT;
        public static readonly var TCHAN = types.TCHAN;
        public static readonly var TMAP = types.TMAP;
        public static readonly var TINTER = types.TINTER;
        public static readonly var TFORW = types.TFORW;
        public static readonly var TANY = types.TANY;
        public static readonly var TSTRING = types.TSTRING;
        public static readonly var TUNSAFEPTR = types.TUNSAFEPTR; 

        // pseudo-types for literals
        public static readonly var TIDEAL = types.TIDEAL;
        public static readonly var TNIL = types.TNIL;
        public static readonly var TBLANK = types.TBLANK; 

        // pseudo-types for frame layout
        public static readonly var TFUNCARGS = types.TFUNCARGS;
        public static readonly var TCHANARGS = types.TCHANARGS; 

        // pseudo-types for import/export
        public static readonly var TDDDFIELD = types.TDDDFIELD; // wrapper: contained type is a ... field

        public static readonly var NTYPE = types.NTYPE;
    }
}}}}
