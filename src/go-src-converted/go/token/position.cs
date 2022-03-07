// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package token -- go2cs converted at 2022 March 06 22:25:54 UTC
// import "go/token" ==> using token = go.go.token_package
// Original source: C:\Program Files\Go\src\go\token\position.go
using fmt = go.fmt_package;
using sort = go.sort_package;
using sync = go.sync_package;
using System;


namespace go.go;

public static partial class token_package {

    // -----------------------------------------------------------------------------
    // Positions

    // Position describes an arbitrary source position
    // including the file, line, and column location.
    // A Position is valid if the line number is > 0.
    //
public partial struct Position {
    public @string Filename; // filename, if any
    public nint Offset; // offset, starting at 0
    public nint Line; // line number, starting at 1
    public nint Column; // column number, starting at 1 (byte count)
}

// IsValid reports whether the position is valid.
private static bool IsValid(this ptr<Position> _addr_pos) {
    ref Position pos = ref _addr_pos.val;

    return pos.Line > 0;
}

// String returns a string in one of several forms:
//
//    file:line:column    valid position with file name
//    file:line           valid position with file name but no column (column == 0)
//    line:column         valid position without file name
//    line                valid position without file name and no column (column == 0)
//    file                invalid position with file name
//    -                   invalid position without file name
//
public static @string String(this Position pos) {
    var s = pos.Filename;
    if (pos.IsValid()) {
        if (s != "") {
            s += ":";
        }
        s += fmt.Sprintf("%d", pos.Line);
        if (pos.Column != 0) {
            s += fmt.Sprintf(":%d", pos.Column);
        }
    }
    if (s == "") {
        s = "-";
    }
    return s;

}

// Pos is a compact encoding of a source position within a file set.
// It can be converted into a Position for a more convenient, but much
// larger, representation.
//
// The Pos value for a given file is a number in the range [base, base+size],
// where base and size are specified when a file is added to the file set.
// The difference between a Pos value and the corresponding file base
// corresponds to the byte offset of that position (represented by the Pos value)
// from the beginning of the file. Thus, the file base offset is the Pos value
// representing the first byte in the file.
//
// To create the Pos value for a specific source offset (measured in bytes),
// first add the respective file to the current file set using FileSet.AddFile
// and then call File.Pos(offset) for that file. Given a Pos value p
// for a specific file set fset, the corresponding Position value is
// obtained by calling fset.Position(p).
//
// Pos values can be compared directly with the usual comparison operators:
// If two Pos values p and q are in the same file, comparing p and q is
// equivalent to comparing the respective source file offsets. If p and q
// are in different files, p < q is true if the file implied by p was added
// to the respective file set before the file implied by q.
//
public partial struct Pos { // : nint
}

// The zero value for Pos is NoPos; there is no file and line information
// associated with it, and NoPos.IsValid() is false. NoPos is always
// smaller than any other Pos value. The corresponding Position value
// for NoPos is the zero value for Position.
//
public static readonly Pos NoPos = 0;

// IsValid reports whether the position is valid.


// IsValid reports whether the position is valid.
public static bool IsValid(this Pos p) {
    return p != NoPos;
}

// -----------------------------------------------------------------------------
// File

// A File is a handle for a file belonging to a FileSet.
// A File has a name, size, and line offset table.
//
public partial struct File {
    public ptr<FileSet> set;
    public @string name; // file name as provided to AddFile
    public nint @base; // Pos value range for this file is [base...base+size]
    public nint size; // file size as provided to AddFile

// lines and infos are protected by mutex
    public sync.Mutex mutex;
    public slice<nint> lines; // lines contains the offset of the first character for each line (the first entry is always 0)
    public slice<lineInfo> infos;
}

// Name returns the file name of file f as registered with AddFile.
private static @string Name(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    return f.name;
}

// Base returns the base offset of file f as registered with AddFile.
private static nint Base(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    return f.@base;
}

// Size returns the size of file f as registered with AddFile.
private static nint Size(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    return f.size;
}

// LineCount returns the number of lines in file f.
private static nint LineCount(this ptr<File> _addr_f) {
    ref File f = ref _addr_f.val;

    f.mutex.Lock();
    var n = len(f.lines);
    f.mutex.Unlock();
    return n;
}

// AddLine adds the line offset for a new line.
// The line offset must be larger than the offset for the previous line
// and smaller than the file size; otherwise the line offset is ignored.
//
private static void AddLine(this ptr<File> _addr_f, nint offset) {
    ref File f = ref _addr_f.val;

    f.mutex.Lock();
    {
        var i = len(f.lines);

        if ((i == 0 || f.lines[i - 1] < offset) && offset < f.size) {
            f.lines = append(f.lines, offset);
        }
    }

    f.mutex.Unlock();

}

// MergeLine merges a line with the following line. It is akin to replacing
// the newline character at the end of the line with a space (to not change the
// remaining offsets). To obtain the line number, consult e.g. Position.Line.
// MergeLine will panic if given an invalid line number.
//
private static void MergeLine(this ptr<File> _addr_f, nint line) => func((defer, panic, _) => {
    ref File f = ref _addr_f.val;

    if (line < 1) {
        panic(fmt.Sprintf("invalid line number %d (should be >= 1)", line));
    }
    f.mutex.Lock();
    defer(f.mutex.Unlock());
    if (line >= len(f.lines)) {
        panic(fmt.Sprintf("invalid line number %d (should be < %d)", line, len(f.lines)));
    }
    copy(f.lines[(int)line..], f.lines[(int)line + 1..]);
    f.lines = f.lines[..(int)len(f.lines) - 1];

});

// SetLines sets the line offsets for a file and reports whether it succeeded.
// The line offsets are the offsets of the first character of each line;
// for instance for the content "ab\nc\n" the line offsets are {0, 3}.
// An empty file has an empty line offset table.
// Each line offset must be larger than the offset for the previous line
// and smaller than the file size; otherwise SetLines fails and returns
// false.
// Callers must not mutate the provided slice after SetLines returns.
//
private static bool SetLines(this ptr<File> _addr_f, slice<nint> lines) {
    ref File f = ref _addr_f.val;
 
    // verify validity of lines table
    var size = f.size;
    foreach (var (i, offset) in lines) {
        if (i > 0 && offset <= lines[i - 1] || size <= offset) {
            return false;
        }
    }    f.mutex.Lock();
    f.lines = lines;
    f.mutex.Unlock();
    return true;

}

// SetLinesForContent sets the line offsets for the given file content.
// It ignores position-altering //line comments.
private static void SetLinesForContent(this ptr<File> _addr_f, slice<byte> content) {
    ref File f = ref _addr_f.val;

    slice<nint> lines = default;
    nint line = 0;
    foreach (var (offset, b) in content) {
        if (line >= 0) {
            lines = append(lines, line);
        }
        line = -1;
        if (b == '\n') {
            line = offset + 1;
        }
    }    f.mutex.Lock();
    f.lines = lines;
    f.mutex.Unlock();

}

// LineStart returns the Pos value of the start of the specified line.
// It ignores any alternative positions set using AddLineColumnInfo.
// LineStart panics if the 1-based line number is invalid.
private static Pos LineStart(this ptr<File> _addr_f, nint line) => func((defer, panic, _) => {
    ref File f = ref _addr_f.val;

    if (line < 1) {
        panic(fmt.Sprintf("invalid line number %d (should be >= 1)", line));
    }
    f.mutex.Lock();
    defer(f.mutex.Unlock());
    if (line > len(f.lines)) {
        panic(fmt.Sprintf("invalid line number %d (should be < %d)", line, len(f.lines)));
    }
    return Pos(f.@base + f.lines[line - 1]);

});

// A lineInfo object describes alternative file, line, and column
// number information (such as provided via a //line directive)
// for a given file offset.
private partial struct lineInfo {
    public nint Offset;
    public @string Filename;
    public nint Line;
    public nint Column;
}

// AddLineInfo is like AddLineColumnInfo with a column = 1 argument.
// It is here for backward-compatibility for code prior to Go 1.11.
//
private static void AddLineInfo(this ptr<File> _addr_f, nint offset, @string filename, nint line) {
    ref File f = ref _addr_f.val;

    f.AddLineColumnInfo(offset, filename, line, 1);
}

// AddLineColumnInfo adds alternative file, line, and column number
// information for a given file offset. The offset must be larger
// than the offset for the previously added alternative line info
// and smaller than the file size; otherwise the information is
// ignored.
//
// AddLineColumnInfo is typically used to register alternative position
// information for line directives such as //line filename:line:column.
//
private static void AddLineColumnInfo(this ptr<File> _addr_f, nint offset, @string filename, nint line, nint column) {
    ref File f = ref _addr_f.val;

    f.mutex.Lock();
    {
        var i = len(f.infos);

        if (i == 0 || f.infos[i - 1].Offset < offset && offset < f.size) {
            f.infos = append(f.infos, new lineInfo(offset,filename,line,column));
        }
    }

    f.mutex.Unlock();

}

// Pos returns the Pos value for the given file offset;
// the offset must be <= f.Size().
// f.Pos(f.Offset(p)) == p.
//
private static Pos Pos(this ptr<File> _addr_f, nint offset) => func((_, panic, _) => {
    ref File f = ref _addr_f.val;

    if (offset > f.size) {
        panic(fmt.Sprintf("invalid file offset %d (should be <= %d)", offset, f.size));
    }
    return Pos(f.@base + offset);

});

// Offset returns the offset for the given file position p;
// p must be a valid Pos value in that file.
// f.Offset(f.Pos(offset)) == offset.
//
private static nint Offset(this ptr<File> _addr_f, Pos p) => func((_, panic, _) => {
    ref File f = ref _addr_f.val;

    if (int(p) < f.@base || int(p) > f.@base + f.size) {
        panic(fmt.Sprintf("invalid Pos value %d (should be in [%d, %d])", p, f.@base, f.@base + f.size));
    }
    return int(p) - f.@base;

});

// Line returns the line number for the given file position p;
// p must be a Pos value in that file or NoPos.
//
private static nint Line(this ptr<File> _addr_f, Pos p) {
    ref File f = ref _addr_f.val;

    return f.Position(p).Line;
}

private static nint searchLineInfos(slice<lineInfo> a, nint x) {
    return sort.Search(len(a), i => a[i].Offset > x) - 1;
}

// unpack returns the filename and line and column number for a file offset.
// If adjusted is set, unpack will return the filename and line information
// possibly adjusted by //line comments; otherwise those comments are ignored.
//
private static (@string, nint, nint) unpack(this ptr<File> _addr_f, nint offset, bool adjusted) => func((defer, _, _) => {
    @string filename = default;
    nint line = default;
    nint column = default;
    ref File f = ref _addr_f.val;

    f.mutex.Lock();
    defer(f.mutex.Unlock());
    filename = f.name;
    {
        var i__prev1 = i;

        var i = searchInts(f.lines, offset);

        if (i >= 0) {
            (line, column) = (i + 1, offset - f.lines[i] + 1);
        }
        i = i__prev1;

    }

    if (adjusted && len(f.infos) > 0) { 
        // few files have extra line infos
        {
            var i__prev2 = i;

            i = searchLineInfos(f.infos, offset);

            if (i >= 0) {
                var alt = _addr_f.infos[i];
                filename = alt.Filename;
                {
                    var i__prev3 = i;

                    i = searchInts(f.lines, alt.Offset);

                    if (i >= 0) { 
                        // i+1 is the line at which the alternative position was recorded
                        var d = line - (i + 1); // line distance from alternative position base
                        line = alt.Line + d;
                        if (alt.Column == 0) { 
                            // alternative column is unknown => relative column is unknown
                            // (the current specification for line directives requires
                            // this to apply until the next PosBase/line directive,
                            // not just until the new newline)
                            column = 0;

                        }
                        else if (d == 0) { 
                            // the alternative position base is on the current line
                            // => column is relative to alternative column
                            column = alt.Column + (offset - alt.Offset);

                        }

                    }

                    i = i__prev3;

                }

            }

            i = i__prev2;

        }

    }
    return ;

});

private static Position position(this ptr<File> _addr_f, Pos p, bool adjusted) {
    Position pos = default;
    ref File f = ref _addr_f.val;

    var offset = int(p) - f.@base;
    pos.Offset = offset;
    pos.Filename, pos.Line, pos.Column = f.unpack(offset, adjusted);
    return ;
}

// PositionFor returns the Position value for the given file position p.
// If adjusted is set, the position may be adjusted by position-altering
// //line comments; otherwise those comments are ignored.
// p must be a Pos value in f or NoPos.
//
private static Position PositionFor(this ptr<File> _addr_f, Pos p, bool adjusted) => func((_, panic, _) => {
    Position pos = default;
    ref File f = ref _addr_f.val;

    if (p != NoPos) {
        if (int(p) < f.@base || int(p) > f.@base + f.size) {
            panic(fmt.Sprintf("invalid Pos value %d (should be in [%d, %d])", p, f.@base, f.@base + f.size));
        }
        pos = f.position(p, adjusted);

    }
    return ;

});

// Position returns the Position value for the given file position p.
// Calling f.Position(p) is equivalent to calling f.PositionFor(p, true).
//
private static Position Position(this ptr<File> _addr_f, Pos p) {
    Position pos = default;
    ref File f = ref _addr_f.val;

    return f.PositionFor(p, true);
}

// -----------------------------------------------------------------------------
// FileSet

// A FileSet represents a set of source files.
// Methods of file sets are synchronized; multiple goroutines
// may invoke them concurrently.
//
// The byte offsets for each file in a file set are mapped into
// distinct (integer) intervals, one interval [base, base+size]
// per file. Base represents the first byte in the file, and size
// is the corresponding file size. A Pos value is a value in such
// an interval. By determining the interval a Pos value belongs
// to, the file, its file base, and thus the byte offset (position)
// the Pos value is representing can be computed.
//
// When adding a new file, a file base must be provided. That can
// be any integer value that is past the end of any interval of any
// file already in the file set. For convenience, FileSet.Base provides
// such a value, which is simply the end of the Pos interval of the most
// recently added file, plus one. Unless there is a need to extend an
// interval later, using the FileSet.Base should be used as argument
// for FileSet.AddFile.
//
public partial struct FileSet {
    public sync.RWMutex mutex; // protects the file set
    public nint @base; // base offset for the next file
    public slice<ptr<File>> files; // list of files in the order added to the set
    public ptr<File> last; // cache of last file looked up
}

// NewFileSet creates a new file set.
public static ptr<FileSet> NewFileSet() {
    return addr(new FileSet(base:1,));
}

// Base returns the minimum base offset that must be provided to
// AddFile when adding the next file.
//
private static nint Base(this ptr<FileSet> _addr_s) {
    ref FileSet s = ref _addr_s.val;

    s.mutex.RLock();
    var b = s.@base;
    s.mutex.RUnlock();
    return b;
}

// AddFile adds a new file with a given filename, base offset, and file size
// to the file set s and returns the file. Multiple files may have the same
// name. The base offset must not be smaller than the FileSet's Base(), and
// size must not be negative. As a special case, if a negative base is provided,
// the current value of the FileSet's Base() is used instead.
//
// Adding the file will set the file set's Base() value to base + size + 1
// as the minimum base value for the next file. The following relationship
// exists between a Pos value p for a given file offset offs:
//
//    int(p) = base + offs
//
// with offs in the range [0, size] and thus p in the range [base, base+size].
// For convenience, File.Pos may be used to create file-specific position
// values from a file offset.
//
private static ptr<File> AddFile(this ptr<FileSet> _addr_s, @string filename, nint @base, nint size) => func((defer, panic, _) => {
    ref FileSet s = ref _addr_s.val;

    s.mutex.Lock();
    defer(s.mutex.Unlock());
    if (base < 0) {
        base = s.@base;
    }
    if (base < s.@base) {
        panic(fmt.Sprintf("invalid base %d (should be >= %d)", base, s.@base));
    }
    if (size < 0) {
        panic(fmt.Sprintf("invalid size %d (should be >= 0)", size));
    }
    ptr<File> f = addr(new File(set:s,name:filename,base:base,size:size,lines:[]int{0}));
    base += size + 1; // +1 because EOF also has a position
    if (base < 0) {
        panic("token.Pos offset overflow (> 2G of source code in file set)");
    }
    s.@base = base;
    s.files = append(s.files, f);
    s.last = f;
    return _addr_f!;

});

// Iterate calls f for the files in the file set in the order they were added
// until f returns false.
//
private static bool Iterate(this ptr<FileSet> _addr_s, Func<ptr<File>, bool> f) {
    ref FileSet s = ref _addr_s.val;

    for (nint i = 0; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++) {
        ptr<File> file;
        s.mutex.RLock();
        if (i < len(s.files)) {
            file = s.files[i];
        }
        s.mutex.RUnlock();
        if (file == null || !f(file)) {
            break;
        }
    }

}

private static nint searchFiles(slice<ptr<File>> a, nint x) {
    return sort.Search(len(a), i => a[i].@base > x) - 1;
}

private static ptr<File> file(this ptr<FileSet> _addr_s, Pos p) {
    ref FileSet s = ref _addr_s.val;

    s.mutex.RLock(); 
    // common case: p is in last file
    {
        var f__prev1 = f;

        var f = s.last;

        if (f != null && f.@base <= int(p) && int(p) <= f.@base + f.size) {
            s.mutex.RUnlock();
            return _addr_f!;
        }
        f = f__prev1;

    } 
    // p is not in last file - search all files
    {
        var i = searchFiles(s.files, int(p));

        if (i >= 0) {
            f = s.files[i]; 
            // f.base <= int(p) by definition of searchFiles
            if (int(p) <= f.@base + f.size) {
                s.mutex.RUnlock();
                s.mutex.Lock();
                s.last = f; // race is ok - s.last is only a cache
                s.mutex.Unlock();
                return _addr_f!;

            }

        }
    }

    s.mutex.RUnlock();
    return _addr_null!;

}

// File returns the file that contains the position p.
// If no such file is found (for instance for p == NoPos),
// the result is nil.
//
private static ptr<File> File(this ptr<FileSet> _addr_s, Pos p) {
    ptr<File> f = default!;
    ref FileSet s = ref _addr_s.val;

    if (p != NoPos) {
        f = s.file(p);
    }
    return ;

}

// PositionFor converts a Pos p in the fileset into a Position value.
// If adjusted is set, the position may be adjusted by position-altering
// //line comments; otherwise those comments are ignored.
// p must be a Pos value in s or NoPos.
//
private static Position PositionFor(this ptr<FileSet> _addr_s, Pos p, bool adjusted) {
    Position pos = default;
    ref FileSet s = ref _addr_s.val;

    if (p != NoPos) {
        {
            var f = s.file(p);

            if (f != null) {
                return f.position(p, adjusted);
            }

        }

    }
    return ;

}

// Position converts a Pos p in the fileset into a Position value.
// Calling s.Position(p) is equivalent to calling s.PositionFor(p, true).
//
private static Position Position(this ptr<FileSet> _addr_s, Pos p) {
    Position pos = default;
    ref FileSet s = ref _addr_s.val;

    return s.PositionFor(p, true);
}

// -----------------------------------------------------------------------------
// Helper functions

private static nint searchInts(slice<nint> a, nint x) { 
    // This function body is a manually inlined version of:
    //
    //   return sort.Search(len(a), func(i int) bool { return a[i] > x }) - 1
    //
    // With better compiler optimizations, this may not be needed in the
    // future, but at the moment this change improves the go/printer
    // benchmark performance by ~30%. This has a direct impact on the
    // speed of gofmt and thus seems worthwhile (2011-04-29).
    // TODO(gri): Remove this when compilers have caught up.
    nint i = 0;
    var j = len(a);
    while (i < j) {
        var h = i + (j - i) >> 1; // avoid overflow when computing h
        // i ≤ h < j
        if (a[h] <= x) {
            i = h + 1;
        }
        else
 {
            j = h;
        }
    }
    return i - 1;

}

} // end token_package
