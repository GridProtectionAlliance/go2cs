// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements the compressed encoding of source
// positions using a lookup table.

// package src -- go2cs converted at 2020 October 08 03:49:41 UTC
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

        // SameFile reports whether p and q are positions in the same file.
        public static bool SameFile(this XPos p, XPos q)
        {
            return p.index == q.index;
        }

        // SameFileAndLine reports whether p and q are positions on the same line in the same file.
        public static bool SameFileAndLine(this XPos p, XPos q)
        {
            return p.index == q.index && p.lico.SameLine(q.lico);
        }

        // After reports whether the position p comes after q in the source.
        // For positions with different bases, ordering is by base index.
        public static bool After(this XPos p, XPos q)
        {
            var n = p.index;
            var m = q.index;
            return n > m || n == m && p.lico > q.lico;

        }

        // WithNotStmt returns the same location to be marked with DWARF is_stmt=0
        public static XPos WithNotStmt(this XPos p)
        {
            p.lico = p.lico.withNotStmt();
            return p;
        }

        // WithDefaultStmt returns the same location with undetermined is_stmt
        public static XPos WithDefaultStmt(this XPos p)
        {
            p.lico = p.lico.withDefaultStmt();
            return p;
        }

        // WithIsStmt returns the same location to be marked with DWARF is_stmt=1
        public static XPos WithIsStmt(this XPos p)
        {
            p.lico = p.lico.withIsStmt();
            return p;
        }

        // WithBogusLine returns a bogus line that won't match any recorded for the source code.
        // Its use is to disrupt the statements within an infinite loop so that the debugger
        // will not itself loop infinitely waiting for the line number to change.
        // gdb chooses not to display the bogus line; delve shows it with a complaint, but the
        // alternative behavior is to hang.
        public static XPos WithBogusLine(this XPos p) => func((_, panic, __) =>
        {
            if (p.index == 0L)
            { 
                // See #35652
                panic("Assigning a bogus line to XPos with no file will cause mysterious downstream failures.");

            }

            p.lico = makeBogusLico();
            return p;

        });

        // WithXlogue returns the same location but marked with DWARF function prologue/epilogue
        public static XPos WithXlogue(this XPos p, PosXlogue x)
        {
            p.lico = p.lico.withXlogue(x);
            return p;
        }

        // LineNumber returns a string for the line number, "?" if it is not known.
        public static @string LineNumber(this XPos p)
        {
            if (!p.IsKnown())
            {
                return "?";
            }

            return p.lico.lineNumber();

        }

        // FileIndex returns a smallish non-negative integer corresponding to the
        // file for this source position.  Smallish is relative; it can be thousands
        // large, but not millions.
        public static int FileIndex(this XPos p)
        {
            return p.index;
        }

        public static @string LineNumberHTML(this XPos p)
        {
            if (!p.IsKnown())
            {
                return "?";
            }

            return p.lico.lineNumberHTML();

        }

        // AtColumn1 returns the same location but shifted to column 1.
        public static XPos AtColumn1(this XPos p)
        {
            p.lico = p.lico.atColumn1();
            return p;
        }

        // A PosTable tracks Pos -> XPos conversions and vice versa.
        // Its zero value is a ready-to-use PosTable.
        public partial struct PosTable
        {
            public slice<ptr<PosBase>> baseList;
            public map<ptr<PosBase>, long> indexMap;
            public map<@string, long> nameMap; // Maps file symbol name to index for debug information.
        }

        // XPos returns the corresponding XPos for the given pos,
        // adding pos to t if necessary.
        private static XPos XPos(this ptr<PosTable> _addr_t, Pos pos)
        {
            ref PosTable t = ref _addr_t.val;

            var m = t.indexMap;
            if (m == null)
            { 
                // Create new list and map and populate with nil
                // base so that NoPos always gets index 0.
                t.baseList = append(t.baseList, null);
                m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<PosBase>, long>{nil:0};
                t.indexMap = m;
                t.nameMap = make_map<@string, long>();

            }

            var (i, ok) = m[pos.@base];
            if (!ok)
            {
                i = len(t.baseList);
                t.baseList = append(t.baseList, pos.@base);
                t.indexMap[pos.@base] = i;
                {
                    var (_, ok) = t.nameMap[pos.@base.symFilename];

                    if (!ok)
                    {
                        t.nameMap[pos.@base.symFilename] = len(t.nameMap);
                    }

                }

            }

            return new XPos(int32(i),pos.lico);

        }

        // Pos returns the corresponding Pos for the given p.
        // If p cannot be translated via t, the function panics.
        private static Pos Pos(this ptr<PosTable> _addr_t, XPos p)
        {
            ref PosTable t = ref _addr_t.val;

            ptr<PosBase> @base;
            if (p.index != 0L)
            {
                base = t.baseList[p.index];
            }

            return new Pos(base,p.lico);

        }

        // FileIndex returns the index of the given filename(symbol) in the PosTable, or -1 if not found.
        private static long FileIndex(this ptr<PosTable> _addr_t, @string filename)
        {
            ref PosTable t = ref _addr_t.val;

            {
                var (v, ok) = t.nameMap[filename];

                if (ok)
                {
                    return v;
                }

            }

            return -1L;

        }

        // DebugLinesFiles returns the file table for the debug_lines DWARF section.
        private static slice<@string> DebugLinesFileTable(this ptr<PosTable> _addr_t)
        {
            ref PosTable t = ref _addr_t.val;
 
            // Create a LUT of the global package level file indices. This table is what
            // is written in the debug_lines header, the file[N] will be referenced as
            // N+1 in the debug_lines table.
            var fileLUT = make_slice<@string>(len(t.nameMap));
            foreach (var (str, i) in t.nameMap)
            {
                fileLUT[i] = str;
            }
            return fileLUT;

        }
    }
}}}
