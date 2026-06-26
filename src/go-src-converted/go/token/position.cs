// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using cmp = cmp_package;
using fmt = fmt_package;
using slices = slices_package;
using strconv = strconv_package;
using sync = sync_package;
using atomic = sync.atomic_package;
using sync;

partial class token_package {

// If debug is set, invalid offset and position values cause a panic
// (go.dev/issue/57490).
internal const bool debug = false;

// -----------------------------------------------------------------------------
// Positions

// Position describes an arbitrary source position
// including the file, line, and column location.
// A Position is valid if the line number is > 0.
[GoType] partial struct ΔPosition {
    public @string Filename; // filename, if any
    public nint Offset;   // offset, starting at 0
    public nint Line;   // line number, starting at 1
    public nint Column;   // column number, starting at 1 (byte count)
}

// IsValid reports whether the position is valid.
[GoRecv] public static bool IsValid(this ref ΔPosition pos) {
    return pos.Line > 0;
}

// String returns a string in one of several forms:
//
//	file:line:column    valid position with file name
//	file:line           valid position with file name but no column (column == 0)
//	line:column         valid position without file name
//	line                valid position without file name and no column (column == 0)
//	file                invalid position with file name
//	-                   invalid position without file name
public static @string String(this ΔPosition pos) {
    @string s = pos.Filename;
    if (pos.IsValid()) {
        if (s != ""u8) {
            s += ":"u8;
        }
        s += strconv.Itoa(pos.Line);
        if (pos.Column != 0) {
            s += fmt.Sprintf(":%d"u8, pos.Column);
        }
    }
    if (s == ""u8) {
        s = "-"u8;
    }
    return s;
}

[GoType("num:nint")] partial struct ΔPos;

// The zero value for [Pos] is NoPos; there is no file and line information
// associated with it, and NoPos.IsValid() is false. NoPos is always
// smaller than any other [Pos] value. The corresponding [Position] value
// for NoPos is the zero value for [Position].
public static readonly ΔPos NoPos = 0;

// IsValid reports whether the position is valid.
public static bool IsValid(this ΔPos p) {
    return p != NoPos;
}

// -----------------------------------------------------------------------------
// File

// A File is a handle for a file belonging to a [FileSet].
// A File has a name, size, and line offset table.
[GoType] partial struct ΔFile {
    internal @string name; // file name as provided to AddFile
    internal nint @base;   // Pos value range for this file is [base...base+size]
    internal nint size;   // file size as provided to AddFile
    // lines and infos are protected by mutex
    internal sync_package.Mutex mutex;
    internal slice<nint> lines; // lines contains the offset of the first character for each line (the first entry is always 0)
    internal slice<lineInfo> infos;
}

// Name returns the file name of file f as registered with AddFile.
[GoRecv] public static @string Name(this ref ΔFile f) {
    return f.name;
}

// Base returns the base offset of file f as registered with AddFile.
[GoRecv] public static nint Base(this ref ΔFile f) {
    return f.@base;
}

// Size returns the size of file f as registered with AddFile.
[GoRecv] public static nint Size(this ref ΔFile f) {
    return f.size;
}

// LineCount returns the number of lines in file f.
[GoRecv] public static nint LineCount(this ref ΔFile f) {
    f.mutex.Lock();
    nint n = len(f.lines);
    f.mutex.Unlock();
    return n;
}

// AddLine adds the line offset for a new line.
// The line offset must be larger than the offset for the previous line
// and smaller than the file size; otherwise the line offset is ignored.
[GoRecv] public static void AddLine(this ref ΔFile f, nint offset) {
    f.mutex.Lock();
    {
        nint i = len(f.lines); if ((i == 0 || f.lines[i - 1] < offset) && offset < f.size) {
            f.lines = append(f.lines, offset);
        }
    }
    f.mutex.Unlock();
}

// MergeLine merges a line with the following line. It is akin to replacing
// the newline character at the end of the line with a space (to not change the
// remaining offsets). To obtain the line number, consult e.g. [Position.Line].
// MergeLine will panic if given an invalid line number.
[GoRecv] public static void MergeLine(this ref ΔFile f, nint line) => func((defer, _) => {
    if (line < 1) {
        throw panic(fmt.Sprintf("invalid line number %d (should be >= 1)"u8, line));
    }
    f.mutex.Lock();
    defer(f.mutex.Unlock);
    if (line >= len(f.lines)) {
        throw panic(fmt.Sprintf("invalid line number %d (should be < %d)"u8, line, len(f.lines)));
    }
    // To merge the line numbered <line> with the line numbered <line+1>,
    // we need to remove the entry in lines corresponding to the line
    // numbered <line+1>. The entry in lines corresponding to the line
    // numbered <line+1> is located at index <line>, since indices in lines
    // are 0-based and line numbers are 1-based.
    copy(f.lines[(int)(line)..], f.lines[(int)(line + 1)..]);
    f.lines = f.lines[..(int)(len(f.lines) - 1)];
});

// Lines returns the effective line offset table of the form described by [File.SetLines].
// Callers must not mutate the result.
[GoRecv] public static slice<nint> Lines(this ref ΔFile f) {
    f.mutex.Lock();
    var lines = f.lines;
    f.mutex.Unlock();
    return lines;
}

// SetLines sets the line offsets for a file and reports whether it succeeded.
// The line offsets are the offsets of the first character of each line;
// for instance for the content "ab\nc\n" the line offsets are {0, 3}.
// An empty file has an empty line offset table.
// Each line offset must be larger than the offset for the previous line
// and smaller than the file size; otherwise SetLines fails and returns
// false.
// Callers must not mutate the provided slice after SetLines returns.
[GoRecv] public static bool SetLines(this ref ΔFile f, slice<nint> lines) {
    // verify validity of lines table
    nint size = f.size;
    foreach (var (i, offset) in lines) {
        if (i > 0 && offset <= lines[i - 1] || size <= offset) {
            return false;
        }
    }
    // set lines table
    f.mutex.Lock();
    f.lines = lines;
    f.mutex.Unlock();
    return true;
}

// SetLinesForContent sets the line offsets for the given file content.
// It ignores position-altering //line comments.
[GoRecv] public static void SetLinesForContent(this ref ΔFile f, slice<byte> content) {
    slice<nint> lines = default!;
    nint line = 0;
    foreach (var (offset, b) in content) {
        if (line >= 0) {
            lines = append(lines, line);
        }
        line = -1;
        if (b == (rune)'\n') {
            line = offset + 1;
        }
    }
    // set lines table
    f.mutex.Lock();
    f.lines = lines;
    f.mutex.Unlock();
}

// LineStart returns the [Pos] value of the start of the specified line.
// It ignores any alternative positions set using [File.AddLineColumnInfo].
// LineStart panics if the 1-based line number is invalid.
[GoRecv] public static ΔPos LineStart(this ref ΔFile f, nint line) => func((defer, _) => {
    if (line < 1) {
        throw panic(fmt.Sprintf("invalid line number %d (should be >= 1)"u8, line));
    }
    f.mutex.Lock();
    defer(f.mutex.Unlock);
    if (line > len(f.lines)) {
        throw panic(fmt.Sprintf("invalid line number %d (should be < %d)"u8, line, len(f.lines)));
    }
    return ((ΔPos)(f.@base + f.lines[line - 1]));
});

// A lineInfo object describes alternative file, line, and column
// number information (such as provided via a //line directive)
// for a given file offset.
[GoType] partial struct lineInfo {
    // fields are exported to make them accessible to gob
    public nint Offset;
    public @string Filename;
    public nint Line;
    public nint Column;
}

// AddLineInfo is like [File.AddLineColumnInfo] with a column = 1 argument.
// It is here for backward-compatibility for code prior to Go 1.11.
[GoRecv] public static void AddLineInfo(this ref ΔFile f, nint offset, @string filename, nint line) {
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
[GoRecv] public static void AddLineColumnInfo(this ref ΔFile f, nint offset, @string filename, nint line, nint column) {
    f.mutex.Lock();
    {
        nint i = len(f.infos); if ((i == 0 || f.infos[i - 1].Offset < offset) && offset < f.size) {
            f.infos = append(f.infos, new lineInfo(offset, filename, line, column));
        }
    }
    f.mutex.Unlock();
}

// fixOffset fixes an out-of-bounds offset such that 0 <= offset <= f.size.
[GoRecv] internal static nint fixOffset(this ref ΔFile f, nint offset) {
    switch (ᐧ) {
    case {} when offset is < 0: {
        if (!debug) {
            return 0;
        }
        break;
    }
    case {} when offset is > f.size: {
        if (!debug) {
            return f.size;
        }
        break;
    }
    default: {
        return offset;
    }}

    // only generate this code if needed
    if (debug) {
        throw panic(fmt.Sprintf("offset %d out of bounds [%d, %d] (position %d out of bounds [%d, %d])"u8,
            0, /* for symmetry */
 offset, f.size,
            f.@base + offset, f.@base, f.@base + f.size));
    }
    return 0;
}

// Pos returns the Pos value for the given file offset.
//
// If offset is negative, the result is the file's start
// position; if the offset is too large, the result is
// the file's end position (see also go.dev/issue/57490).
//
// The following invariant, though not true for Pos values
// in general, holds for the result p:
// f.Pos(f.Offset(p)) == p.
[GoRecv] public static ΔPos Pos(this ref ΔFile f, nint offset) {
    return ((ΔPos)(f.@base + f.fixOffset(offset)));
}

// Offset returns the offset for the given file position p.
//
// If p is before the file's start position (or if p is NoPos),
// the result is 0; if p is past the file's end position, the
// the result is the file size (see also go.dev/issue/57490).
//
// The following invariant, though not true for offset values
// in general, holds for the result offset:
// f.Offset(f.Pos(offset)) == offset
[GoRecv] public static nint Offset(this ref ΔFile f, ΔPos p) {
    return f.fixOffset(((nint)p) - f.@base);
}

// Line returns the line number for the given file position p;
// p must be a [Pos] value in that file or [NoPos].
[GoRecv] public static nint Line(this ref ΔFile f, ΔPos p) {
    return f.Position(p).Line;
}

internal static nint searchLineInfos(slice<lineInfo> a, nint x) {
    var (i, found) = slices.BinarySearchFunc(a, x, (lineInfo a, nint x) => cmp.Compare(aΔ1.Offset, xΔ1));
    if (!found) {
        // We want the lineInfo containing x, but if we didn't
        // find x then i is the next one.
        i--;
    }
    return i;
}

// unpack returns the filename and line and column number for a file offset.
// If adjusted is set, unpack will return the filename and line information
// possibly adjusted by //line comments; otherwise those comments are ignored.
[GoRecv] internal static (@string filename, nint line, nint column) unpack(this ref ΔFile f, nint offset, bool adjusted) {
    @string filename = default!;
    nint line = default!;
    nint column = default!;

    f.mutex.Lock();
    filename = f.name;
    {
        nint i = searchInts(f.lines, offset); if (i >= 0) {
            (line, column) = (i + 1, offset - f.lines[i] + 1);
        }
    }
    if (adjusted && len(f.infos) > 0) {
        // few files have extra line infos
        {
            nint i = searchLineInfos(f.infos, offset); if (i >= 0) {
                var alt = Ꮡ(f.infos[i]);
                filename = alt.val.Filename;
                {
                    nint iΔ1 = searchInts(f.lines, (~alt).Offset); if (iΔ1 >= 0) {
                        // i+1 is the line at which the alternative position was recorded
                        nint d = line - (iΔ1 + 1);
                        // line distance from alternative position base
                        line = (~alt).Line + d;
                        if ((~alt).Column == 0){
                            // alternative column is unknown => relative column is unknown
                            // (the current specification for line directives requires
                            // this to apply until the next PosBase/line directive,
                            // not just until the new newline)
                            column = 0;
                        } else 
                        if (d == 0) {
                            // the alternative position base is on the current line
                            // => column is relative to alternative column
                            column = (~alt).Column + (offset - (~alt).Offset);
                        }
                    }
                }
            }
        }
    }
    // TODO(mvdan): move Unlock back under Lock with a defer statement once
    // https://go.dev/issue/38471 is fixed to remove the performance penalty.
    f.mutex.Unlock();
    return (filename, line, column);
}

[GoRecv] internal static ΔPosition /*pos*/ position(this ref ΔFile f, ΔPos p, bool adjusted) {
    ΔPosition pos = default!;

    nint offset = f.fixOffset(((nint)p) - f.@base);
    pos.Offset = offset;
    (pos.Filename, pos.Line, pos.Column) = f.unpack(offset, adjusted);
    return pos;
}

// PositionFor returns the Position value for the given file position p.
// If p is out of bounds, it is adjusted to match the File.Offset behavior.
// If adjusted is set, the position may be adjusted by position-altering
// //line comments; otherwise those comments are ignored.
// p must be a Pos value in f or NoPos.
[GoRecv] public static ΔPosition /*pos*/ PositionFor(this ref ΔFile f, ΔPos p, bool adjusted) {
    ΔPosition pos = default!;

    if (p != NoPos) {
        pos = f.position(p, adjusted);
    }
    return pos;
}

// Position returns the Position value for the given file position p.
// If p is out of bounds, it is adjusted to match the File.Offset behavior.
// Calling f.Position(p) is equivalent to calling f.PositionFor(p, true).
[GoRecv] public static ΔPosition /*pos*/ Position(this ref ΔFile f, ΔPos p) {
    ΔPosition pos = default!;

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
// per file. [FileSet.Base] represents the first byte in the file, and size
// is the corresponding file size. A [Pos] value is a value in such
// an interval. By determining the interval a [Pos] value belongs
// to, the file, its file base, and thus the byte offset (position)
// the [Pos] value is representing can be computed.
//
// When adding a new file, a file base must be provided. That can
// be any integer value that is past the end of any interval of any
// file already in the file set. For convenience, [FileSet.Base] provides
// such a value, which is simply the end of the Pos interval of the most
// recently added file, plus one. Unless there is a need to extend an
// interval later, using the [FileSet.Base] should be used as argument
// for [FileSet.AddFile].
//
// A [File] may be removed from a FileSet when it is no longer needed.
// This may reduce memory usage in a long-running application.
[GoType] partial struct FileSet {
    internal sync_package.RWMutex mutex;         // protects the file set
    internal nint @base;                 // base offset for the next file
    internal slice<ж<ΔFile>> files;    // list of files in the order added to the set
    internal sync.atomic_package.Pointer last; // cache of last file looked up
}

// NewFileSet creates a new file set.
public static ж<FileSet> NewFileSet() {
    return Ꮡ(new FileSet(
        @base: 1
    ));
}

// 0 == NoPos

// Base returns the minimum base offset that must be provided to
// [FileSet.AddFile] when adding the next file.
[GoRecv] public static nint Base(this ref FileSet s) {
    s.mutex.RLock();
    nint b = s.@base;
    s.mutex.RUnlock();
    return b;
}

// AddFile adds a new file with a given filename, base offset, and file size
// to the file set s and returns the file. Multiple files may have the same
// name. The base offset must not be smaller than the [FileSet.Base], and
// size must not be negative. As a special case, if a negative base is provided,
// the current value of the [FileSet.Base] is used instead.
//
// Adding the file will set the file set's [FileSet.Base] value to base + size + 1
// as the minimum base value for the next file. The following relationship
// exists between a [Pos] value p for a given file offset offs:
//
//	int(p) = base + offs
//
// with offs in the range [0, size] and thus p in the range [base, base+size].
// For convenience, [File.Pos] may be used to create file-specific position
// values from a file offset.
[GoRecv] public static ж<ΔFile> AddFile(this ref FileSet s, @string filename, nint @base, nint size) => func((defer, _) => {
    // Allocate f outside the critical section.
    var f = Ꮡ(new ΔFile(name: filename, size: size, lines: new nint[]{0}.slice()));
    s.mutex.Lock();
    defer(s.mutex.Unlock);
    if (@base < 0) {
        @base = s.@base;
    }
    if (@base < s.@base) {
        throw panic(fmt.Sprintf("invalid base %d (should be >= %d)"u8, @base, s.@base));
    }
    f.val.@base = @base;
    if (size < 0) {
        throw panic(fmt.Sprintf("invalid size %d (should be >= 0)"u8, size));
    }
    // base >= s.base && size >= 0
    @base += size + 1;
    // +1 because EOF also has a position
    if (@base < 0) {
        throw panic("token.Pos offset overflow (> 2G of source code in file set)");
    }
    // add the file to the file set
    s.@base = @base;
    s.files = append(s.files, f);
    s.last.Store(f);
    return f;
});

// RemoveFile removes a file from the [FileSet] so that subsequent
// queries for its [Pos] interval yield a negative result.
// This reduces the memory usage of a long-lived [FileSet] that
// encounters an unbounded stream of files.
//
// Removing a file that does not belong to the set has no effect.
[GoRecv] public static void RemoveFile(this ref FileSet s, ж<ΔFile> Ꮡfile) => func((defer, _) => {
    ref var file = ref Ꮡfile.val;

    s.last.CompareAndSwap(Ꮡfile, nil);
    // clear last file cache
    s.mutex.Lock();
    defer(s.mutex.Unlock);
    {
        ref var i = ref heap<nint>(out var Ꮡi);
        i = searchFiles(s.files, file.@base); if (i >= 0 && s.files[i] == Ꮡfile) {
            var last = Ꮡ(s.files[len(s.files) - 1]);
            s.files = append(s.files[..(int)(i)], s.files[(int)(i + 1)..].ꓸꓸꓸ);
            last.val = default!;
        }
    }
});

// don't prolong lifetime when popping last element

// Iterate calls f for the files in the file set in the order they were added
// until f returns false.
[GoRecv] public static void Iterate(this ref FileSet s, Func<ж<ΔFile>, bool> f) {
    for (nint i = 0; ᐧ ; i++) {
        ж<ΔFile> file = default!;
        s.mutex.RLock();
        if (i < len(s.files)) {
            file = s.files[i];
        }
        s.mutex.RUnlock();
        if (file == nil || !f(file)) {
            break;
        }
    }
}

internal static nint searchFiles(slice<ж<ΔFile>> a, nint x) {
    var (i, found) = slices.BinarySearchFunc(a, x, (ж<ΔFile> a, nint x) => cmp.Compare((~aΔ1).@base, xΔ1));
    if (!found) {
        // We want the File containing x, but if we didn't
        // find x then i is the next one.
        i--;
    }
    return i;
}

[GoRecv] internal static ж<ΔFile> file(this ref FileSet s, ΔPos p) => func((defer, _) => {
    // common case: p is in last file.
    {
        var f = s.last.Load(); if (f != nil && (~f).@base <= ((nint)p) && ((nint)p) <= (~f).@base + (~f).size) {
            return f;
        }
    }
    s.mutex.RLock();
    defer(s.mutex.RUnlock);
    // p is not in last file - search all files
    {
        nint i = searchFiles(s.files, ((nint)p)); if (i >= 0) {
            var f = s.files[i];
            // f.base <= int(p) by definition of searchFiles
            if (((nint)p) <= (~f).@base + (~f).size) {
                // Update cache of last file. A race is ok,
                // but an exclusive lock causes heavy contention.
                s.last.Store(f);
                return f;
            }
        }
    }
    return default!;
});

// File returns the file that contains the position p.
// If no such file is found (for instance for p == [NoPos]),
// the result is nil.
[GoRecv] public static ж<ΔFile> /*f*/ File(this ref FileSet s, ΔPos p) {
    ж<ΔFile> f = default!;

    if (p != NoPos) {
        f = s.file(p);
    }
    return f;
}

// PositionFor converts a [Pos] p in the fileset into a [Position] value.
// If adjusted is set, the position may be adjusted by position-altering
// //line comments; otherwise those comments are ignored.
// p must be a [Pos] value in s or [NoPos].
[GoRecv] public static ΔPosition /*pos*/ PositionFor(this ref FileSet s, ΔPos p, bool adjusted) {
    ΔPosition pos = default!;

    if (p != NoPos) {
        {
            var f = s.file(p); if (f != nil) {
                return f.position(p, adjusted);
            }
        }
    }
    return pos;
}

// Position converts a [Pos] p in the fileset into a Position value.
// Calling s.Position(p) is equivalent to calling s.PositionFor(p, true).
[GoRecv] public static ΔPosition /*pos*/ Position(this ref FileSet s, ΔPos p) {
    ΔPosition pos = default!;

    return s.PositionFor(p, true);
}

// -----------------------------------------------------------------------------
// Helper functions
internal static nint searchInts(slice<nint> a, nint x) {
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
    nint j = len(a);
    while (i < j) {
        nint h = ((nint)(((nuint)(i + j)) >> (int)(1)));
        // avoid overflow when computing h
        // i ≤ h < j
        if (a[h] <= x){
            i = h + 1;
        } else {
            j = h;
        }
    }
    return i - 1;
}

} // end token_package
