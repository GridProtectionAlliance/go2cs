// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:11:22 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\phielim.go

using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // phielim eliminates redundant phi values from f.
        // A phi is redundant if its arguments are all equal. For
        // purposes of counting, ignore the phi itself. Both of
        // these phis are redundant:
        //   v = phi(x,x,x)
        //   v = phi(x,v,x,v)
        // We repeat this process to also catch situations like:
        //   v = phi(x, phi(x, x), phi(x, v))
        // TODO: Can we also simplify cases like:
        //   v = phi(v, w, x)
        //   w = phi(v, w, x)
        // and would that be useful?
        private static void phielim(ptr<Func> _addr_f)
        {
            ref Func f = ref _addr_f.val;

            while (true)
            {
                var change = false;
                foreach (var (_, b) in f.Blocks)
                {
                    foreach (var (_, v) in b.Values)
                    {
                        copyelimValue(v);
                        change = phielimValue(_addr_v) || change;
                    }
                }                if (!change)
                {
                    break;
                }
            }

        }

        // phielimValue tries to convert the phi v to a copy.
        private static bool phielimValue(ptr<Value> _addr_v)
        {
            ref Value v = ref _addr_v.val;

            if (v.Op != OpPhi)
            {
                return false;
            } 

            // If there are two distinct args of v which
            // are not v itself, then the phi must remain.
            // Otherwise, we can replace it with a copy.
            ptr<Value> w;
            foreach (var (_, x) in v.Args)
            {
                if (x == v)
                {
                    continue;
                }

                if (x == w)
                {
                    continue;
                }

                if (w != null)
                {
                    return false;
                }

                w = x;

            }
            if (w == null)
            { 
                // v references only itself. It must be in
                // a dead code loop. Don't bother modifying it.
                return false;

            }

            v.Op = OpCopy;
            v.SetArgs1(w);
            var f = v.Block.Func;
            if (f.pass.debug > 0L)
            {
                f.Warnl(v.Pos, "eliminated phi");
            }

            return true;

        }
    }
}}}}
