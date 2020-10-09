// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ppc64 -- go2cs converted at 2020 October 09 05:40:14 UTC
// import "cmd/compile/internal/ppc64" ==> using ppc64 = go.cmd.compile.@internal.ppc64_package
// Original source: C:\Go\src\cmd\compile\internal\ppc64\opt.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ppc64_package
    {
        // Many Power ISA arithmetic and logical instructions come in four
        // standard variants. These bits let us map between variants.
        public static readonly long V_CC = (long)1L << (int)(0L); // xCC (affect CR field 0 flags)
        public static readonly long V_V = (long)1L << (int)(1L); // xV (affect SO and OV flags)
    }
}}}}
