// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:10:34 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\id.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        public partial struct ID // : int
        {
        }

        // idAlloc provides an allocator for unique integers.
        private partial struct idAlloc
        {
            public ID last;
        }

        // get allocates an ID and returns it. IDs are always > 0.
        private static ID get(this ptr<idAlloc> _addr_a) => func((_, panic, __) =>
        {
            ref idAlloc a = ref _addr_a.val;

            var x = a.last;
            x++;
            if (x == 1L << (int)(31L) - 1L)
            {
                panic("too many ids for this function");
            }

            a.last = x;
            return x;

        });

        // num returns the maximum ID ever returned + 1.
        private static long num(this ptr<idAlloc> _addr_a)
        {
            ref idAlloc a = ref _addr_a.val;

            return int(a.last + 1L);
        }
    }
}}}}
