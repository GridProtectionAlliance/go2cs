// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements the compressed encoding of source
// positions using a lookup table.

// package src -- go2cs converted at 2020 August 29 08:45:51 UTC
// import "cmd/internal/src" ==> using src = go.cmd.@internal.src_package
// Original source: C:\Go\src\cmd\internal\src\xpos.go

using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class src_package
    {
        // XPos is a more compact representation of Pos.
        public partial struct XPos
        {
            public int index;
            public ref lico lico => ref lico_val;
        }

        // NoXPos is a valid unknown position.
        public static XPos NoXPos = default;

        // IsKnown reports whether the position p is known.
        // XPos.IsKnown() matches Pos.IsKnown() for corresponding
        // positions.
        public static bool IsKnown(this XPos p)
        {
            return p.index != 0L || p.Line() != 0L;
        }

        // Before reports whether the position p comes before q in the source.
        // For positions with different bases, ordering is by base index.
        public static bool Before(this XPos p, XPos q)
        {
            var n = p.index;
            var m = q.index;
            return n < m || n == m && p.lico < q.lico;
        }

        // After reports whether the position p comes after q in the source.
        // For positions with different bases, ordering is by base index.
        public static bool After(this XPos p, XPos q)
        {
            var n = p.index;
            var m = q.index;
            return n > m || n == m && p.lico > q.lico;
        }

        // A PosTable tracks Pos -> XPos conversions and vice versa.
        // Its zero value is a ready-to-use PosTable.
        public partial struct PosTable
        {
            public slice<ref PosBase> baseList;
            public map<ref PosBase, long> indexMap;
        }

        // XPos returns the corresponding XPos for the given pos,
        // adding pos to t if necessary.
        private static XPos XPos(this ref PosTable t, Pos pos)
        {
            var m = t.indexMap;
            if (m == null)
            { 
                // Create new list and map and populate with nil
                // base so that NoPos always gets index 0.
                t.baseList = append(t.baseList, null);
                m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ref PosBase, long>{nil:0};
                t.indexMap = m;
            }
            var (i, ok) = m[pos.@base];
            if (!ok)
            {
                i = len(t.baseList);
                t.baseList = append(t.baseList, pos.@base);
                t.indexMap[pos.@base] = i;
            }
            return new XPos(int32(i),pos.lico);
        }

        // Pos returns the corresponding Pos for the given p.
        // If p cannot be translated via t, the function panics.
        private static Pos Pos(this ref PosTable t, XPos p)
        {
            ref PosBase @base = default;
            if (p.index != 0L)
            {
                base = t.baseList[p.index];
            }
            return new Pos(base,p.lico);
        }
    }
}}}
