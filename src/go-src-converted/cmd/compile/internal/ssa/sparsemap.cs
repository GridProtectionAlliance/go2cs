// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 09:24:15 UTC
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
        // from http://research.swtch.com/sparse
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
        private static ref sparseMap newSparseMap(long n)
        {
            return ref new sparseMap(dense:nil,sparse:make([]int32,n));
        }

        private static long size(this ref sparseMap s)
        {
            return len(s.dense);
        }

        private static bool contains(this ref sparseMap s, ID k)
        {
            var i = s.sparse[k];
            return i < int32(len(s.dense)) && s.dense[i].key == k;
        }

        // get returns the value for key k, or -1 if k does
        // not appear in the map.
        private static int get(this ref sparseMap s, ID k)
        {
            var i = s.sparse[k];
            if (i < int32(len(s.dense)) && s.dense[i].key == k)
            {
                return s.dense[i].val;
            }
            return -1L;
        }

        private static void set(this ref sparseMap s, ID k, int v, src.XPos a)
        {
            var i = s.sparse[k];
            if (i < int32(len(s.dense)) && s.dense[i].key == k)
            {
                s.dense[i].val = v;
                s.dense[i].aux = a;
                return;
            }
            s.dense = append(s.dense, new sparseEntry(k,v,a));
            s.sparse[k] = int32(len(s.dense)) - 1L;
        }

        // setBit sets the v'th bit of k's value, where 0 <= v < 32
        private static void setBit(this ref sparseMap _s, ID k, ulong v) => func(_s, (ref sparseMap s, Defer _, Panic panic, Recover __) =>
        {
            if (v >= 32L)
            {
                panic("bit index too large.");
            }
            var i = s.sparse[k];
            if (i < int32(len(s.dense)) && s.dense[i].key == k)
            {
                s.dense[i].val |= 1L << (int)(v);
                return;
            }
            s.dense = append(s.dense, new sparseEntry(k,1<<v,src.NoXPos));
            s.sparse[k] = int32(len(s.dense)) - 1L;
        });

        private static void remove(this ref sparseMap s, ID k)
        {
            var i = s.sparse[k];
            if (i < int32(len(s.dense)) && s.dense[i].key == k)
            {
                var y = s.dense[len(s.dense) - 1L];
                s.dense[i] = y;
                s.sparse[y.key] = i;
                s.dense = s.dense[..len(s.dense) - 1L];
            }
        }

        private static void clear(this ref sparseMap s)
        {
            s.dense = s.dense[..0L];
        }

        private static slice<sparseEntry> contents(this ref sparseMap s)
        {
            return s.dense;
        }
    }
}}}}
