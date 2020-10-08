// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:26:37 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\sparsemap.go
using src = go.cmd.@internal.src_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // from https://research.swtch.com/sparse
        // in turn, from Briggs and Torczon
        private partial struct sparseEntry
        {
            public ID key;
            public int val;
            public src.XPos aux;
        }

        private partial struct sparseMap
        {
            public slice<sparseEntry> dense;
            public slice<int> sparse;
        }

        // newSparseMap returns a sparseMap that can map
        // integers between 0 and n-1 to int32s.
        private static ptr<sparseMap> newSparseMap(long n)
        {
            return addr(new sparseMap(dense:nil,sparse:make([]int32,n)));
        }

        private static long cap(this ptr<sparseMap> _addr_s)
        {
            ref sparseMap s = ref _addr_s.val;

            return len(s.sparse);
        }

        private static long size(this ptr<sparseMap> _addr_s)
        {
            ref sparseMap s = ref _addr_s.val;

            return len(s.dense);
        }

        private static bool contains(this ptr<sparseMap> _addr_s, ID k)
        {
            ref sparseMap s = ref _addr_s.val;

            var i = s.sparse[k];
            return i < int32(len(s.dense)) && s.dense[i].key == k;
        }

        // get returns the value for key k, or -1 if k does
        // not appear in the map.
        private static int get(this ptr<sparseMap> _addr_s, ID k)
        {
            ref sparseMap s = ref _addr_s.val;

            var i = s.sparse[k];
            if (i < int32(len(s.dense)) && s.dense[i].key == k)
            {
                return s.dense[i].val;
            }

            return -1L;

        }

        private static void set(this ptr<sparseMap> _addr_s, ID k, int v, src.XPos a)
        {
            ref sparseMap s = ref _addr_s.val;

            var i = s.sparse[k];
            if (i < int32(len(s.dense)) && s.dense[i].key == k)
            {
                s.dense[i].val = v;
                s.dense[i].aux = a;
                return ;
            }

            s.dense = append(s.dense, new sparseEntry(k,v,a));
            s.sparse[k] = int32(len(s.dense)) - 1L;

        }

        // setBit sets the v'th bit of k's value, where 0 <= v < 32
        private static void setBit(this ptr<sparseMap> _addr_s, ID k, ulong v) => func((_, panic, __) =>
        {
            ref sparseMap s = ref _addr_s.val;

            if (v >= 32L)
            {
                panic("bit index too large.");
            }

            var i = s.sparse[k];
            if (i < int32(len(s.dense)) && s.dense[i].key == k)
            {
                s.dense[i].val |= 1L << (int)(v);
                return ;
            }

            s.dense = append(s.dense, new sparseEntry(k,1<<v,src.NoXPos));
            s.sparse[k] = int32(len(s.dense)) - 1L;

        });

        private static void remove(this ptr<sparseMap> _addr_s, ID k)
        {
            ref sparseMap s = ref _addr_s.val;

            var i = s.sparse[k];
            if (i < int32(len(s.dense)) && s.dense[i].key == k)
            {
                var y = s.dense[len(s.dense) - 1L];
                s.dense[i] = y;
                s.sparse[y.key] = i;
                s.dense = s.dense[..len(s.dense) - 1L];
            }

        }

        private static void clear(this ptr<sparseMap> _addr_s)
        {
            ref sparseMap s = ref _addr_s.val;

            s.dense = s.dense[..0L];
        }

        private static slice<sparseEntry> contents(this ptr<sparseMap> _addr_s)
        {
            ref sparseMap s = ref _addr_s.val;

            return s.dense;
        }
    }
}}}}
