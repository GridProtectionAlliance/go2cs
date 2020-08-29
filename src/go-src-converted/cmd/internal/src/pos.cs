// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements the encoding of source positions.

// package src -- go2cs converted at 2020 August 29 08:45:45 UTC
// import "cmd/internal/src" ==> using src = go.cmd.@internal.src_package
// Original source: C:\Go\src\cmd\internal\src\pos.go
using strconv = go.strconv_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class src_package
    {
        // A Pos encodes a source position consisting of a (line, column) number pair
        // and a position base. A zero Pos is a ready to use "unknown" position (nil
        // position base and zero line number).
        //
        // The (line, column) values refer to a position in a file independent of any
        // position base ("absolute" file position).
        //
        // The position base is used to determine the "relative" position, that is the
        // filename and line number relative to the position base. If the base refers
        // to the current file, there is no difference between absolute and relative
        // positions. If it refers to a //line pragma, a relative position is relative
        // to that pragma. A position base in turn contains the position at which it
        // was introduced in the current file.
        public partial struct Pos
        {
            public ptr<PosBase> @base;
            public ref lico lico => ref lico_val;
        }

        // NoPos is a valid unknown position.
        public static Pos NoPos = default;

        // MakePos creates a new Pos value with the given base, and (file-absolute)
        // line and column.
        public static Pos MakePos(ref PosBase @base, ulong line, ulong col)
        {
            return new Pos(base,makeLico(line,col));
        }

        // IsKnown reports whether the position p is known.
        // A position is known if it either has a non-nil
        // position base, or a non-zero line number.
        public static bool IsKnown(this Pos p)
        {
            return p.@base != null || p.Line() != 0L;
        }

        // Before reports whether the position p comes before q in the source.
        // For positions in different files, ordering is by filename.
        public static bool Before(this Pos p, Pos q)
        {
            var n = p.Filename();
            var m = q.Filename();
            return n < m || n == m && p.lico < q.lico;
        }

        // After reports whether the position p comes after q in the source.
        // For positions in different files, ordering is by filename.
        public static bool After(this Pos p, Pos q)
        {
            var n = p.Filename();
            var m = q.Filename();
            return n > m || n == m && p.lico > q.lico;
        }

        // Filename returns the name of the actual file containing this position.
        public static @string Filename(this Pos p)
        {
            return p.@base.Pos().RelFilename();
        }

        // Base returns the position base.
        public static ref PosBase Base(this Pos p)
        {
            return p.@base;
        }

        // SetBase sets the position base.
        private static void SetBase(this ref Pos p, ref PosBase @base)
        {
            p.@base = base;

        }

        // RelFilename returns the filename recorded with the position's base.
        public static @string RelFilename(this Pos p)
        {
            return p.@base.Filename();
        }

        // RelLine returns the line number relative to the positions's base.
        public static ulong RelLine(this Pos p)
        {
            var b = p.@base;

            return b.Line() + p.Line() - b.Pos().Line();
        }

        // AbsFilename() returns the absolute filename recorded with the position's base.
        public static @string AbsFilename(this Pos p)
        {
            return p.@base.AbsFilename();
        }

        // SymFilename() returns the absolute filename recorded with the position's base,
        // prefixed by FileSymPrefix to make it appropriate for use as a linker symbol.
        public static @string SymFilename(this Pos p)
        {
            return p.@base.SymFilename();
        }

        public static @string String(this Pos p)
        {
            return p.Format(true, true);
        }

        // Format formats a position as "filename:line" or "filename:line:column",
        // controlled by the showCol flag. A position relative to a line directive
        // is always formatted without column information. In that case, if showOrig
        // is set, the original position (again controlled by showCol) is appended
        // in square brackets: "filename:line[origfile:origline:origcolumn]".
        public static @string Format(this Pos p, bool showCol, bool showOrig)
        {
            if (!p.IsKnown())
            {
                return "<unknown line number>";
            }
            {
                var b = p.@base;

                if (b == b.Pos().@base)
                { 
                    // base is file base (incl. nil)
                    return format(p.Filename(), p.Line(), p.Col(), showCol);
                } 

                // base is relative
                // Print the column only for the original position since the
                // relative position's column information may be bogus (it's
                // typically generated code and we can't say much about the
                // original source at that point but for the file:line info
                // that's provided via a line directive).
                // TODO(gri) This may not be true if we have an inlining base.
                // We may want to differentiate at some point.

            } 

            // base is relative
            // Print the column only for the original position since the
            // relative position's column information may be bogus (it's
            // typically generated code and we can't say much about the
            // original source at that point but for the file:line info
            // that's provided via a line directive).
            // TODO(gri) This may not be true if we have an inlining base.
            // We may want to differentiate at some point.
            var s = format(p.RelFilename(), p.RelLine(), 0L, false);
            if (showOrig)
            {
                s += "[" + format(p.Filename(), p.Line(), p.Col(), showCol) + "]";
            }
            return s;
        }

        // format formats a (filename, line, col) tuple as "filename:line" (showCol
        // is false) or "filename:line:column" (showCol is true).
        private static @string format(@string filename, ulong line, ulong col, bool showCol)
        {
            var s = filename + ":" + strconv.FormatUint(uint64(line), 10L); 
            // col == colMax is interpreted as unknown column value
            if (showCol && col < colMax)
            {
                s += ":" + strconv.FormatUint(uint64(col), 10L);
            }
            return s;
        }

        // ----------------------------------------------------------------------------
        // PosBase

        // A PosBase encodes a filename and base line number.
        // Typically, each file and line pragma introduce a PosBase.
        // A nil *PosBase is a ready to use file PosBase for an unnamed
        // file with line numbers starting at 1.
        public partial struct PosBase
        {
            public Pos pos;
            public @string filename; // file name used to open source file, for error messages
            public @string absFilename; // absolute file name, for PC-Line tables
            public @string symFilename; // cached symbol file name, to avoid repeated string concatenation
            public ulong line; // relative line number at pos
            public long inl; // inlining index (see cmd/internal/obj/inl.go)
        }

        // NewFileBase returns a new *PosBase for a file with the given (relative and
        // absolute) filenames.
        public static ref PosBase NewFileBase(@string filename, @string absFilename)
        {
            if (filename != "")
            {
                PosBase @base = ref new PosBase(filename:filename,absFilename:absFilename,symFilename:FileSymPrefix+absFilename,inl:-1,);
                @base.pos = MakePos(base, 0L, 0L);
                return base;
            }
            return null;
        }

        // NewLinePragmaBase returns a new *PosBase for a line pragma of the form
        //      //line filename:line
        // at position pos.
        public static ref PosBase NewLinePragmaBase(Pos pos, @string filename, @string absFilename, ulong line)
        {
            return ref new PosBase(pos,filename,absFilename,FileSymPrefix+absFilename,line-1,-1);
        }

        // NewInliningBase returns a copy of the old PosBase with the given inlining
        // index. If old == nil, the resulting PosBase has no filename.
        public static ref PosBase NewInliningBase(ref PosBase old, long inlTreeIndex)
        {
            if (old == null)
            {
                PosBase @base = ref new PosBase(inl:inlTreeIndex);
                @base.pos = MakePos(base, 0L, 0L);
                return base;
            }
            var copy = old.Value;
            @base = ref copy;
            @base.inl = inlTreeIndex;
            if (old == old.pos.@base)
            {
                @base.pos.@base = base;
            }
            return base;
        }

        private static Pos noPos = default;

        // Pos returns the position at which base is located.
        // If b == nil, the result is the zero position.
        private static ref Pos Pos(this ref PosBase b)
        {
            if (b != null)
            {
                return ref b.pos;
            }
            return ref noPos;
        }

        // Filename returns the filename recorded with the base.
        // If b == nil, the result is the empty string.
        private static @string Filename(this ref PosBase b)
        {
            if (b != null)
            {
                return b.filename;
            }
            return "";
        }

        // AbsFilename returns the absolute filename recorded with the base.
        // If b == nil, the result is the empty string.
        private static @string AbsFilename(this ref PosBase b)
        {
            if (b != null)
            {
                return b.absFilename;
            }
            return "";
        }

        public static readonly @string FileSymPrefix = "gofile..";

        // SymFilename returns the absolute filename recorded with the base,
        // prefixed by FileSymPrefix to make it appropriate for use as a linker symbol.
        // If b is nil, SymFilename returns FileSymPrefix + "??".


        // SymFilename returns the absolute filename recorded with the base,
        // prefixed by FileSymPrefix to make it appropriate for use as a linker symbol.
        // If b is nil, SymFilename returns FileSymPrefix + "??".
        private static @string SymFilename(this ref PosBase b)
        {
            if (b != null)
            {
                return b.symFilename;
            }
            return FileSymPrefix + "??";
        }

        // Line returns the line number recorded with the base.
        // If b == nil, the result is 0.
        private static ulong Line(this ref PosBase b)
        {
            if (b != null)
            {
                return b.line;
            }
            return 0L;
        }

        // InliningIndex returns the index into the global inlining
        // tree recorded with the base. If b == nil or the base has
        // not been inlined, the result is < 0.
        private static long InliningIndex(this ref PosBase b)
        {
            if (b != null)
            {
                return b.inl;
            }
            return -1L;
        }

        // ----------------------------------------------------------------------------
        // lico

        // A lico is a compact encoding of a LIne and COlumn number.
        private partial struct lico // : uint
        {
        }

        // Layout constants: 24 bits for line, 8 bits for column.
        // (If this is too tight, we can either make lico 64b wide,
        // or we can introduce a tiered encoding where we remove column
        // information as line numbers grow bigger; similar to what gcc
        // does.)
        private static readonly long lineBits = 24L;
        private static readonly long lineMax = 1L << (int)(lineBits) - 1L;
        private static readonly long colBits = 32L - lineBits;
        private static readonly long colMax = 1L << (int)(colBits) - 1L;

        private static lico makeLico(ulong line, ulong col)
        {
            if (line > lineMax)
            { 
                // cannot represent line, use max. line so we have some information
                line = lineMax;
            }
            if (col > colMax)
            { 
                // cannot represent column, use max. column so we have some information
                col = colMax;
            }
            return lico(line << (int)(colBits) | col);
        }

        private static ulong Line(this lico x)
        {
            return uint(x) >> (int)(colBits);
        }
        private static ulong Col(this lico x)
        {
            return uint(x) & colMax;
        }
    }
}}}
