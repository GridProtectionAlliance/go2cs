// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements the encoding of source positions.

// package src -- go2cs converted at 2020 October 08 03:49:35 UTC
// import "cmd/internal/src" ==> using src = go.cmd.@internal.src_package
// Original source: C:\Go\src\cmd\internal\src\pos.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using io = go.io_package;
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
        // positions. If it refers to a //line directive, a relative position is relative
        // to that directive. A position base in turn contains the position at which it
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
        public static Pos MakePos(ptr<PosBase> _addr_@base, ulong line, ulong col)
        {
            ref PosBase @base = ref _addr_@base.val;

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

        public static @string LineNumber(this Pos p)
        {
            if (!p.IsKnown())
            {
                return "?";
            }

            return p.lico.lineNumber();

        }

        public static @string LineNumberHTML(this Pos p)
        {
            if (!p.IsKnown())
            {
                return "?";
            }

            return p.lico.lineNumberHTML();

        }

        // Filename returns the name of the actual file containing this position.
        public static @string Filename(this Pos p)
        {
            return p.@base.Pos().RelFilename();
        }

        // Base returns the position base.
        public static ptr<PosBase> Base(this Pos p)
        {
            return _addr_p.@base!;
        }

        // SetBase sets the position base.
        private static void SetBase(this ptr<Pos> _addr_p, ptr<PosBase> _addr_@base)
        {
            ref Pos p = ref _addr_p.val;
            ref PosBase @base = ref _addr_@base.val;

            p.@base = base;
        }

        // RelFilename returns the filename recorded with the position's base.
        public static @string RelFilename(this Pos p)
        {
            return p.@base.Filename();
        }

        // RelLine returns the line number relative to the position's base.
        public static ulong RelLine(this Pos p)
        {
            var b = p.@base;
            if (b.Line() == 0L)
            { 
                // base line is unknown => relative line is unknown
                return 0L;

            }

            return b.Line() + (p.Line() - b.Pos().Line());

        }

        // RelCol returns the column number relative to the position's base.
        public static ulong RelCol(this Pos p)
        {
            var b = p.@base;
            if (b.Col() == 0L)
            { 
                // base column is unknown => relative column is unknown
                // (the current specification for line directives requires
                // this to apply until the next PosBase/line directive,
                // not just until the new newline)
                return 0L;

            }

            if (p.Line() == b.Pos().Line())
            { 
                // p on same line as p's base => column is relative to p's base
                return b.Col() + (p.Col() - b.Pos().Col());

            }

            return p.Col();

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
        // controlled by the showCol flag and if the column is known (!= 0).
        // For positions relative to line directives, the original position is
        // shown as well, as in "filename:line[origfile:origline:origcolumn] if
        // showOrig is set.
        public static @string Format(this Pos p, bool showCol, bool showOrig)
        {
            ptr<object> buf = @new<bytes.Buffer>();
            p.WriteTo(buf, showCol, showOrig);
            return buf.String();
        }

        // WriteTo a position to w, formatted as Format does.
        public static void WriteTo(this Pos p, io.Writer w, bool showCol, bool showOrig)
        {
            if (!p.IsKnown())
            {
                io.WriteString(w, "<unknown line number>");
                return ;
            }

            {
                var b = p.@base;

                if (b == b.Pos().@base)
                { 
                    // base is file base (incl. nil)
                    format(w, p.Filename(), p.Line(), p.Col(), showCol);
                    return ;

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
            format(w, p.RelFilename(), p.RelLine(), p.RelCol(), showCol);
            if (showOrig)
            {
                io.WriteString(w, "[");
                format(w, p.Filename(), p.Line(), p.Col(), showCol);
                io.WriteString(w, "]");
            }

        }

        // format formats a (filename, line, col) tuple as "filename:line" (showCol
        // is false or col == 0) or "filename:line:column" (showCol is true and col != 0).
        private static void format(io.Writer w, @string filename, ulong line, ulong col, bool showCol)
        {
            io.WriteString(w, filename);
            io.WriteString(w, ":");
            fmt.Fprint(w, line); 
            // col == 0 and col == colMax are interpreted as unknown column values
            if (showCol && 0L < col && col < colMax)
            {
                io.WriteString(w, ":");
                fmt.Fprint(w, col);
            }

        }

        // formatstr wraps format to return a string.
        private static @string formatstr(@string filename, ulong line, ulong col, bool showCol)
        {
            ptr<object> buf = @new<bytes.Buffer>();
            format(buf, filename, line, col, showCol);
            return buf.String();
        }

        // ----------------------------------------------------------------------------
        // PosBase

        // A PosBase encodes a filename and base position.
        // Typically, each file and line directive introduce a PosBase.
        public partial struct PosBase
        {
            public Pos pos; // position at which the relative position is (line, col)
            public @string filename; // file name used to open source file, for error messages
            public @string absFilename; // absolute file name, for PC-Line tables
            public @string symFilename; // cached symbol file name, to avoid repeated string concatenation
            public ulong line; // relative line, column number at pos
            public ulong col; // relative line, column number at pos
            public long inl; // inlining index (see cmd/internal/obj/inl.go)
        }

        // NewFileBase returns a new *PosBase for a file with the given (relative and
        // absolute) filenames.
        public static ptr<PosBase> NewFileBase(@string filename, @string absFilename)
        {
            ptr<PosBase> @base = addr(new PosBase(filename:filename,absFilename:absFilename,symFilename:FileSymPrefix+absFilename,line:1,col:1,inl:-1,));
            @base.pos = MakePos(_addr_base, 1L, 1L);
            return _addr_base!;
        }

        // NewLinePragmaBase returns a new *PosBase for a line directive of the form
        //      //line filename:line:col
        //      /*line filename:line:col*/
        // at position pos.
        public static ptr<PosBase> NewLinePragmaBase(Pos pos, @string filename, @string absFilename, ulong line, ulong col)
        {
            return addr(new PosBase(pos,filename,absFilename,FileSymPrefix+absFilename,line,col,-1));
        }

        // NewInliningBase returns a copy of the old PosBase with the given inlining
        // index. If old == nil, the resulting PosBase has no filename.
        public static ptr<PosBase> NewInliningBase(ptr<PosBase> _addr_old, long inlTreeIndex)
        {
            ref PosBase old = ref _addr_old.val;

            if (old == null)
            {
                ptr<PosBase> @base = addr(new PosBase(line:1,col:1,inl:inlTreeIndex));
                @base.pos = MakePos(_addr_base, 1L, 1L);
                return _addr_base!;
            }

            ref PosBase copy = ref heap(old, out ptr<PosBase> _addr_copy);
            @base = _addr_copy;
            @base.inl = inlTreeIndex;
            if (old == old.pos.@base)
            {
                @base.pos.@base = base;
            }

            return _addr_base!;

        }

        private static Pos noPos = default;

        // Pos returns the position at which base is located.
        // If b == nil, the result is the zero position.
        private static ptr<Pos> Pos(this ptr<PosBase> _addr_b)
        {
            ref PosBase b = ref _addr_b.val;

            if (b != null)
            {
                return _addr__addr_b.pos!;
            }

            return _addr__addr_noPos!;

        }

        // Filename returns the filename recorded with the base.
        // If b == nil, the result is the empty string.
        private static @string Filename(this ptr<PosBase> _addr_b)
        {
            ref PosBase b = ref _addr_b.val;

            if (b != null)
            {
                return b.filename;
            }

            return "";

        }

        // AbsFilename returns the absolute filename recorded with the base.
        // If b == nil, the result is the empty string.
        private static @string AbsFilename(this ptr<PosBase> _addr_b)
        {
            ref PosBase b = ref _addr_b.val;

            if (b != null)
            {
                return b.absFilename;
            }

            return "";

        }

        public static readonly @string FileSymPrefix = (@string)"gofile..";

        // SymFilename returns the absolute filename recorded with the base,
        // prefixed by FileSymPrefix to make it appropriate for use as a linker symbol.
        // If b is nil, SymFilename returns FileSymPrefix + "??".


        // SymFilename returns the absolute filename recorded with the base,
        // prefixed by FileSymPrefix to make it appropriate for use as a linker symbol.
        // If b is nil, SymFilename returns FileSymPrefix + "??".
        private static @string SymFilename(this ptr<PosBase> _addr_b)
        {
            ref PosBase b = ref _addr_b.val;

            if (b != null)
            {
                return b.symFilename;
            }

            return FileSymPrefix + "??";

        }

        // Line returns the line number recorded with the base.
        // If b == nil, the result is 0.
        private static ulong Line(this ptr<PosBase> _addr_b)
        {
            ref PosBase b = ref _addr_b.val;

            if (b != null)
            {
                return b.line;
            }

            return 0L;

        }

        // Col returns the column number recorded with the base.
        // If b == nil, the result is 0.
        private static ulong Col(this ptr<PosBase> _addr_b)
        {
            ref PosBase b = ref _addr_b.val;

            if (b != null)
            {
                return b.col;
            }

            return 0L;

        }

        // InliningIndex returns the index into the global inlining
        // tree recorded with the base. If b == nil or the base has
        // not been inlined, the result is < 0.
        private static long InliningIndex(this ptr<PosBase> _addr_b)
        {
            ref PosBase b = ref _addr_b.val;

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

        // Layout constants: 20 bits for line, 8 bits for column, 2 for isStmt, 2 for pro/epilogue
        // (If this is too tight, we can either make lico 64b wide,
        // or we can introduce a tiered encoding where we remove column
        // information as line numbers grow bigger; similar to what gcc
        // does.)
        // The bitfield order is chosen to make IsStmt be the least significant
        // part of a position; its use is to communicate statement edges through
        // instruction scrambling in code generation, not to impose an order.
        // TODO: Prologue and epilogue are perhaps better handled as pseudo-ops for the assembler,
        // because they have almost no interaction with other uses of the position.
        private static readonly long lineBits = (long)20L;
        private static readonly long lineMax = (long)1L << (int)(lineBits) - 2L;
        private static readonly long bogusLine = (long)1L; // Used to disrupt infinite loops to prevent debugger looping
        private static readonly long isStmtBits = (long)2L;
        private static readonly long isStmtMax = (long)1L << (int)(isStmtBits) - 1L;
        private static readonly long xlogueBits = (long)2L;
        private static readonly long xlogueMax = (long)1L << (int)(xlogueBits) - 1L;
        private static readonly long colBits = (long)32L - lineBits - xlogueBits - isStmtBits;
        private static readonly long colMax = (long)1L << (int)(colBits) - 1L;

        private static readonly long isStmtShift = (long)0L;
        private static readonly var isStmtMask = (var)isStmtMax << (int)(isStmtShift);
        private static readonly var xlogueShift = (var)isStmtBits + isStmtShift;
        private static readonly var xlogueMask = (var)xlogueMax << (int)(xlogueShift);
        private static readonly var colShift = (var)xlogueBits + xlogueShift;
        private static readonly var lineShift = (var)colBits + colShift;

 
        // It is expected that the front end or a phase in SSA will usually generate positions tagged with
        // PosDefaultStmt, but note statement boundaries with PosIsStmt.  Simple statements will have a single
        // boundary; for loops with initialization may have one for their entry and one for their back edge
        // (this depends on exactly how the loop is compiled; the intent is to provide a good experience to a
        // user debugging a program; the goal is that a breakpoint set on the loop line fires both on entry
        // and on iteration).  Proper treatment of non-gofmt input with multiple simple statements on a single
        // line is TBD.
        //
        // Optimizing compilation will move instructions around, and some of these will become known-bad as
        // step targets for debugging purposes (examples: register spills and reloads; code generated into
        // the entry block; invariant code hoisted out of loops) but those instructions will still have interesting
        // positions for profiling purposes. To reflect this these positions will be changed to PosNotStmt.
        //
        // When the optimizer removes an instruction marked PosIsStmt; it should attempt to find a nearby
        // instruction with the same line marked PosDefaultStmt to be the new statement boundary.  I.e., the
        // optimizer should make a best-effort to conserve statement boundary positions, and might be enhanced
        // to note when a statement boundary is not conserved.
        //
        // Code cloning, e.g. loop unrolling or loop unswitching, is an exception to the conservation rule
        // because a user running a debugger would expect to see breakpoints active in the copies of the code.
        //
        // In non-optimizing compilation there is still a role for PosNotStmt because of code generation
        // into the entry block.  PosIsStmt statement positions should be conserved.
        //
        // When code generation occurs any remaining default-marked positions are replaced with not-statement
        // positions.
        //
        public static readonly ulong PosDefaultStmt = (ulong)iota; // Default; position is not a statement boundary, but might be if optimization removes the designated statement boundary
        public static readonly var PosIsStmt = (var)0; // Position is a statement boundary; if optimization removes the corresponding instruction, it should attempt to find a new instruction to be the boundary.
        public static readonly var PosNotStmt = (var)1; // Position should not be a statement boundary, but line should be preserved for profiling and low-level debugging purposes.

        public partial struct PosXlogue // : ulong
        {
        }

        public static readonly PosXlogue PosDefaultLogue = (PosXlogue)iota;
        public static readonly var PosPrologueEnd = (var)0;
        public static readonly var PosEpilogueBegin = (var)1;


        private static lico makeLicoRaw(ulong line, ulong col)
        {
            return lico(line << (int)(lineShift) | col << (int)(colShift));
        }

        // This is a not-position that will not be elided.
        // Depending on the debugger (gdb or delve) it may or may not be displayed.
        private static lico makeBogusLico()
        {
            return makeLicoRaw(bogusLine, 0L).withIsStmt();
        }

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
            // default is not-sure-if-statement
            return makeLicoRaw(line, col);

        }

        private static ulong Line(this lico x)
        {
            return uint(x) >> (int)(lineShift);
        }
        private static bool SameLine(this lico x, lico y)
        {
            return 0L == (x ^ y) & ~lico(1L << (int)(lineShift) - 1L);
        }
        private static ulong Col(this lico x)
        {
            return uint(x) >> (int)(colShift) & colMax;
        }
        private static ulong IsStmt(this lico x)
        {
            if (x == 0L)
            {
                return PosNotStmt;
            }

            return uint(x) >> (int)(isStmtShift) & isStmtMax;

        }
        private static PosXlogue Xlogue(this lico x)
        {
            return PosXlogue(uint(x) >> (int)(xlogueShift) & xlogueMax);
        }

        // withNotStmt returns a lico for the same location, but not a statement
        private static lico withNotStmt(this lico x)
        {
            return x.withStmt(PosNotStmt);
        }

        // withDefaultStmt returns a lico for the same location, with default isStmt
        private static lico withDefaultStmt(this lico x)
        {
            return x.withStmt(PosDefaultStmt);
        }

        // withIsStmt returns a lico for the same location, tagged as definitely a statement
        private static lico withIsStmt(this lico x)
        {
            return x.withStmt(PosIsStmt);
        }

        // withLogue attaches a prologue/epilogue attribute to a lico
        private static lico withXlogue(this lico x, PosXlogue xlogue)
        {
            if (x == 0L)
            {
                if (xlogue == 0L)
                {
                    return x;
                } 
                // Normalize 0 to "not a statement"
                x = lico(PosNotStmt << (int)(isStmtShift));

            }

            return lico(uint(x) & ~uint(xlogueMax << (int)(xlogueShift)) | (uint(xlogue) << (int)(xlogueShift)));

        }

        // withStmt returns a lico for the same location with specified is_stmt attribute
        private static lico withStmt(this lico x, ulong stmt)
        {
            if (x == 0L)
            {
                return lico(0L);
            }

            return lico(uint(x) & ~uint(isStmtMax << (int)(isStmtShift)) | (stmt << (int)(isStmtShift)));

        }

        private static @string lineNumber(this lico x)
        {
            return fmt.Sprintf("%d", x.Line());
        }

        private static @string lineNumberHTML(this lico x)
        {
            if (x.IsStmt() == PosDefaultStmt)
            {
                return fmt.Sprintf("%d", x.Line());
            }

            @string style = "b";
            @string pfx = "+";
            if (x.IsStmt() == PosNotStmt)
            {
                style = "s"; // /strike not supported in HTML5
                pfx = "";

            }

            return fmt.Sprintf("<%s>%s%d</%s>", style, pfx, x.Line(), style);

        }

        private static lico atColumn1(this lico x)
        {
            return makeLico(x.Line(), 1L).withIsStmt();
        }
    }
}}}
