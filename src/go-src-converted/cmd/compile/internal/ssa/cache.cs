// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 08:53:20 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\cache.go
using sort = go.sort_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // A Cache holds reusable compiler state.
        // It is intended to be re-used for multiple Func compilations.
        public partial struct Cache
        {
            public array<Value> values;
            public array<Block> blocks;
            public array<Location> locs; // Storage for DWARF variable locations. Lazily allocated
// since location lists are off by default.
            public slice<VarLoc> varLocs;
            public long curVarLoc; // Reusable stackAllocState.
// See stackalloc.go's {new,put}StackAllocState.
            public ptr<stackAllocState> stackAllocState;
            public slice<ID> domblockstore; // scratch space for computing dominators
            public slice<ref sparseSet> scrSparse; // scratch sparse sets to be re-used.
        }

        private static void Reset(this ref Cache c)
        {
            var nv = sort.Search(len(c.values), i => c.values[i].ID == 0L);
            var xv = c.values[..nv];
            {
                var i__prev1 = i;

                foreach (var (__i) in xv)
                {
                    i = __i;
                    xv[i] = new Value();
                }

                i = i__prev1;
            }

            var nb = sort.Search(len(c.blocks), i => c.blocks[i].ID == 0L);
            var xb = c.blocks[..nb];
            {
                var i__prev1 = i;

                foreach (var (__i) in xb)
                {
                    i = __i;
                    xb[i] = new Block();
                }

                i = i__prev1;
            }

            var nl = sort.Search(len(c.locs), i => c.locs[i] == null);
            var xl = c.locs[..nl];
            {
                var i__prev1 = i;

                foreach (var (__i) in xl)
                {
                    i = __i;
                    xl[i] = null;
                }

                i = i__prev1;
            }

            var xvl = c.varLocs[..c.curVarLoc];
            {
                var i__prev1 = i;

                foreach (var (__i) in xvl)
                {
                    i = __i;
                    xvl[i] = new VarLoc();
                }

                i = i__prev1;
            }

            c.curVarLoc = 0L;
        }

        private static ref VarLoc NewVarLoc(this ref Cache c)
        {
            if (c.varLocs == null)
            {
                c.varLocs = make_slice<VarLoc>(4000L);
            }
            if (c.curVarLoc == len(c.varLocs))
            {
                return ref new VarLoc();
            }
            var vl = ref c.varLocs[c.curVarLoc];
            c.curVarLoc++;
            return vl;
        }
    }
}}}}
