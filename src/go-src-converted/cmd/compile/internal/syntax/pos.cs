// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syntax -- go2cs converted at 2022 March 13 06:26:57 UTC
// import "cmd/compile/internal/syntax" ==> using syntax = go.cmd.compile.@internal.syntax_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\syntax\pos.go
namespace go.cmd.compile.@internal;

using fmt = fmt_package;

public static partial class syntax_package {

// PosMax is the largest line or column value that can be represented without loss.
// Incoming values (arguments) larger than PosMax will be set to PosMax.
public static readonly nint PosMax = 1 << 30;

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
public partial struct Pos {
    public ptr<PosBase> @base;
    public uint line;
    public uint col;
}

// MakePos returns a new Pos for the given PosBase, line and column.
public static Pos MakePos(ptr<PosBase> _addr_@base, nuint line, nuint col) {
    ref PosBase @base = ref _addr_@base.val;

    return new Pos(base,sat32(line),sat32(col));
}

// TODO(gri) IsKnown makes an assumption about linebase < 1.
//           Maybe we should check for Base() != nil instead.

public static Pos Pos(this Pos pos) {
    return pos;
}
public static bool IsKnown(this Pos pos) {
    return pos.line > 0;
}
public static ptr<PosBase> Base(this Pos pos) {
    return _addr_pos.@base!;
}
public static nuint Line(this Pos pos) {
    return uint(pos.line);
}
public static nuint Col(this Pos pos) {
    return uint(pos.col);
}

public static @string RelFilename(this Pos pos) {
    return pos.@base.Filename();
}

public static nuint RelLine(this Pos pos) {
    var b = pos.@base;
    if (b.Line() == 0) { 
        // base line is unknown => relative line is unknown
        return 0;
    }
    return b.Line() + (pos.Line() - b.Pos().Line());
}

public static nuint RelCol(this Pos pos) {
    var b = pos.@base;
    if (b.Col() == 0) { 
        // base column is unknown => relative column is unknown
        // (the current specification for line directives requires
        // this to apply until the next PosBase/line directive,
        // not just until the new newline)
        return 0;
    }
    if (pos.Line() == b.Pos().Line()) { 
        // pos on same line as pos base => column is relative to pos base
        return b.Col() + (pos.Col() - b.Pos().Col());
    }
    return pos.Col();
}

// Cmp compares the positions p and q and returns a result r as follows:
//
//    r <  0: p is before q
//    r == 0: p and q are the same position (but may not be identical)
//    r >  0: p is after q
//
// If p and q are in different files, p is before q if the filename
// of p sorts lexicographically before the filename of q.
public static nint Cmp(this Pos p, Pos q) {
    var pname = p.RelFilename();
    var qname = q.RelFilename();

    if (pname < qname) 
        return -1;
    else if (pname > qname) 
        return +1;
        var pline = p.Line();
    var qline = q.Line();

    if (pline < qline) 
        return -1;
    else if (pline > qline) 
        return +1;
        var pcol = p.Col();
    var qcol = q.Col();

    if (pcol < qcol) 
        return -1;
    else if (pcol > qcol) 
        return +1;
        return 0;
}

public static @string String(this Pos pos) {
    position_ rel = new position_(pos.RelFilename(),pos.RelLine(),pos.RelCol());
    position_ abs = new position_(pos.Base().Pos().RelFilename(),pos.Line(),pos.Col());
    var s = rel.String();
    if (rel != abs) {
        s += "[" + abs.String() + "]";
    }
    return s;
}

// TODO(gri) cleanup: find better name, avoid conflict with position in error_test.go
private partial struct position_ {
    public @string filename;
    public nuint line;
    public nuint col;
}

private static @string String(this position_ p) {
    if (p.line == 0) {
        if (p.filename == "") {
            return "<unknown position>";
        }
        return p.filename;
    }
    if (p.col == 0) {
        return fmt.Sprintf("%s:%d", p.filename, p.line);
    }
    return fmt.Sprintf("%s:%d:%d", p.filename, p.line, p.col);
}

// A PosBase represents the base for relative position information:
// At position pos, the relative position is filename:line:col.
public partial struct PosBase {
    public Pos pos;
    public @string filename;
    public uint line;
    public uint col;
}

// NewFileBase returns a new PosBase for the given filename.
// A file PosBase's position is relative to itself, with the
// position being filename:1:1.
public static ptr<PosBase> NewFileBase(@string filename) {
    ptr<PosBase> @base = addr(new PosBase(MakePos(nil,linebase,colbase),filename,linebase,colbase));
    @base.pos.@base = base;
    return _addr_base!;
}

// NewLineBase returns a new PosBase for a line directive "line filename:line:col"
// relative to pos, which is the position of the character immediately following
// the comment containing the line directive. For a directive in a line comment,
// that position is the beginning of the next line (i.e., the newline character
// belongs to the line comment).
public static ptr<PosBase> NewLineBase(Pos pos, @string filename, nuint line, nuint col) {
    return addr(new PosBase(pos,filename,sat32(line),sat32(col)));
}

private static bool IsFileBase(this ptr<PosBase> _addr_@base) {
    ref PosBase @base = ref _addr_@base.val;

    if (base == null) {
        return false;
    }
    return @base.pos.@base == base;
}

private static Pos Pos(this ptr<PosBase> _addr_@base) {
    Pos _ = default;
    ref PosBase @base = ref _addr_@base.val;

    if (base == null) {
        return ;
    }
    return @base.pos;
}

private static @string Filename(this ptr<PosBase> _addr_@base) {
    ref PosBase @base = ref _addr_@base.val;

    if (base == null) {
        return "";
    }
    return @base.filename;
}

private static nuint Line(this ptr<PosBase> _addr_@base) {
    ref PosBase @base = ref _addr_@base.val;

    if (base == null) {
        return 0;
    }
    return uint(@base.line);
}

private static nuint Col(this ptr<PosBase> _addr_@base) {
    ref PosBase @base = ref _addr_@base.val;

    if (base == null) {
        return 0;
    }
    return uint(@base.col);
}

private static uint sat32(nuint x) {
    if (x > PosMax) {
        return PosMax;
    }
    return uint32(x);
}

} // end syntax_package
