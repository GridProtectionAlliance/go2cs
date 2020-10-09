// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sys -- go2cs converted at 2020 October 09 04:45:29 UTC
// import "runtime/internal/sys" ==> using sys = go.runtime.@internal.sys_package
// Original source: C:\Go\src\runtime\internal\sys\arch.go

using static go.builtin;

namespace go {
namespace runtime {
namespace @internal
{
    public static partial class sys_package
    {
        public partial struct ArchFamilyType // : long
        {
        }

        public static readonly ArchFamilyType AMD64 = (ArchFamilyType)iota;
        public static readonly var ARM = 0;
        public static readonly var ARM64 = 1;
        public static readonly var I386 = 2;
        public static readonly var MIPS = 3;
        public static readonly var MIPS64 = 4;
        public static readonly var PPC64 = 5;
        public static readonly var RISCV64 = 6;
        public static readonly var S390X = 7;
        public static readonly var WASM = 8;

    }
}}}
