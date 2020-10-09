// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package riscv64 -- go2cs converted at 2020 October 09 05:48:58 UTC
// import "cmd/link/internal/riscv64" ==> using riscv64 = go.cmd.link.@internal.riscv64_package
// Original source: C:\Go\src\cmd\link\internal\riscv64\l.go

using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class riscv64_package
    {
        private static readonly long maxAlign = (long)32L; // max data alignment
        private static readonly long minAlign = (long)1L;
        private static readonly long funcAlign = (long)8L;

        private static readonly long dwarfRegLR = (long)1L;
        private static readonly long dwarfRegSP = (long)2L;

    }
}}}}
