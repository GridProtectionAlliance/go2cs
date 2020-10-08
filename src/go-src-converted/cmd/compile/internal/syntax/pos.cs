// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syntax -- go2cs converted at 2020 October 08 04:28:24 UTC
// import "cmd/compile/internal/syntax" ==> using syntax = go.cmd.compile.@internal.syntax_package
// Original source: C:\Go\src\cmd\compile\internal\syntax\pos.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class syntax_package
    {
        // PosMax is the largest line or column value that can be represented without loss.
        // Incoming values (arguments) larger than PosMax will be set to PosMax.
        public static readonly long PosMax = (long)1L << (int)(30L);

        // A Pos represents an absolute (line, col) source position
        // with a reference to position base for computing relative
        // (to a file, or line directive) position information.
        // Pos values are intentionally light-weight so that they
        // can be created without too much concern about space use.


        // A Pos represents an absolute (line, col) source position
        // with a reference to position base for computing relative
        // (to a file, or line directive) position information.
        // Pos values are intentionally light-weight so that they
        // can be created without too much concern about space use.
        public partial struct Pos
        {
            public ptr<PosBase> @base;
            public uint line;
            public uint col;
        }

        // MakePos returns a new Pos for the given PosBase, line and column.
        public static Pos MakePos(ptr<PosBase> _addr_@base, ulong line, ulong col)
        {
            ref PosBase @base = ref _addr_@base.val;

            return new Pos(base,sat32(line),sat32(col));
        }

        // TODO(gri) IsKnown makes an assumption about linebase < 1.
        //           Maybe we should check for Base() != nil instead.

        public static bool IsKnown(this Pos pos)
        {
            return pos.line > 0L;
        }
        public static ptr<PosBase> Base(this Pos pos)
        {
            return _addr_pos.@base!;
        }
        public static ulong Line(this Pos pos)
        {
            return uint(pos.line);
        }
        public static ulong Col(this Pos pos)
        {
            return uint(pos.col);
        }

        public static @string RelFilename(this Pos pos)
        {
            return pos.@base.Filename();
        }

        public static ulong RelLine(this Pos pos)
        {
            var b = pos.@base;
            if (b.Line() == 0L)
            { 
                // base line is unknown => relative line is unknown
                return 0L;

            }

            return b.Line() + (pos.Line() - b.Pos().Line());

        }

        public static ulong RelCol(this Pos pos)
        {
            var b = pos.@base;
            if (b.Col() == 0L)
            { 
                // base column is unknown => relative column is unknown
                // (the current specification for line directives requires
                // this to apply until the next PosBase/line directive,
                // not just until the new newline)
                return 0L;

            }

            if (pos.Line() == b.Pos().Line())
            { 
                // pos on same line as pos base => column is relative to pos base
                return b.Col() + (pos.Col() - b.Pos().Col());

            }

            return pos.Col();

        }

        public static @string String(this Pos pos)
        {
            position_ rel = new position_(pos.RelFilename(),pos.RelLine(),pos.RelCol());
            position_ abs = new position_(pos.Base().Pos().RelFilename(),pos.Line(),pos.Col());
            var s = rel.String();
            if (rel != abs)
            {
                s += "[" + abs.String() + "]";
            }

            return s;

        }

        // TODO(gri) cleanup: find better name, avoid conflict with position in error_test.go
        private partial struct position_
        {
            public @string filename;
            public ulong line;
            public ulong col;
        }

        private static @string String(this position_ p)
        {
            if (p.line == 0L)
            {
                if (p.filename == "")
                {
                    return "<unknown position>";
                }

                return p.filename;

            }

            if (p.col == 0L)
            {
                return fmt.Sprintf("%s:%d", p.filename, p.line);
            }

            return fmt.Sprintf("%s:%d:%d", p.filename, p.line, p.col);

        }

        // A PosBase represents the base for relative position information:
        // At position pos, the relative position is filename:line:col.
        public partial struct PosBase
        {
            public Pos pos;
            public @string filename;
            public uint line;
            public uint col;
        }

        // NewFileBase returns a new PosBase for the given filename.
        // A file PosBase's position is relative to itself, with the
        // position being filename:1:1.
        public static ptr<PosBase> NewFileBase(@string filename)
        {
            ptr<PosBase> @base = addr(new PosBase(MakePos(nil,linebase,colbase),filename,linebase,colbase));
            @base.pos.@base = base;
            return _addr_base!;
        }

        // NewLineBase returns a new PosBase for a line directive "line filename:line:col"
        // relative to pos, which is the position of the character immediately following
        // the comment containing the line directive. For a directive in a line comment,
        // that position is the beginning of the next line (i.e., the newline character
        // belongs to the line comment).
        public static ptr<PosBase> NewLineBase(Pos pos, @string filename, ulong line, ulong col)
        {
            return addr(new PosBase(pos,filename,sat32(line),sat32(col)));
        }

        private static bool IsFileBase(this ptr<PosBase> _addr_@base)
        {
            ref PosBase @base = ref _addr_@base.val;

            if (base == null)
            {
                return false;
            }

            return @base.pos.@base == base;

        }

        private static Pos Pos(this ptr<PosBase> _addr_@base)
        {
            Pos _ = default;
            ref PosBase @base = ref _addr_@base.val;

            if (base == null)
            {
                return ;
            }

            return @base.pos;

        }

        private static @string Filename(this ptr<PosBase> _addr_@base)
        {
            ref PosBase @base = ref _addr_@base.val;

            if (base == null)
            {
                return "";
            }

            return @base.filename;

        }

        private static ulong Line(this ptr<PosBase> _addr_@base)
        {
            ref PosBase @base = ref _addr_@base.val;

            if (base == null)
            {
                return 0L;
            }

            return uint(@base.line);

        }

        private static ulong Col(this ptr<PosBase> _addr_@base)
        {
            ref PosBase @base = ref _addr_@base.val;

            if (base == null)
            {
                return 0L;
            }

            return uint(@base.col);

        }

        private static uint sat32(ulong x)
        {
            if (x > PosMax)
            {
                return PosMax;
            }

            return uint32(x);

        }
    }
}}}}
