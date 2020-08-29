// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package objabi -- go2cs converted at 2020 August 29 08:46:19 UTC
// import "cmd/internal/objabi" ==> using objabi = go.cmd.@internal.objabi_package
// Original source: C:\Go\src\cmd\internal\objabi\funcid.go

using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class objabi_package
    {
        // A FuncID identifies particular functions that need to be treated
        // specially by the runtime.
        // Note that in some situations involving plugins, there may be multiple
        // copies of a particular special runtime function.
        // Note: this list must match the list in runtime/symtab.go.
        public partial struct FuncID // : uint
        {
        }

        public static readonly FuncID FuncID_normal = iota; // not a special function
        public static readonly var FuncID_goexit = 0;
        public static readonly var FuncID_jmpdefer = 1;
        public static readonly var FuncID_mcall = 2;
        public static readonly var FuncID_morestack = 3;
        public static readonly var FuncID_mstart = 4;
        public static readonly var FuncID_rt0_go = 5;
        public static readonly var FuncID_asmcgocall = 6;
        public static readonly var FuncID_sigpanic = 7;
        public static readonly var FuncID_runfinq = 8;
        public static readonly var FuncID_bgsweep = 9;
        public static readonly var FuncID_forcegchelper = 10;
        public static readonly var FuncID_timerproc = 11;
        public static readonly var FuncID_gcBgMarkWorker = 12;
        public static readonly var FuncID_systemstack_switch = 13;
        public static readonly var FuncID_systemstack = 14;
        public static readonly var FuncID_cgocallback_gofunc = 15;
        public static readonly var FuncID_gogo = 16;
        public static readonly var FuncID_externalthreadhandler = 17;
    }
}}}
