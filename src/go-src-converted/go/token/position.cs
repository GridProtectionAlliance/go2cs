// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package token -- go2cs converted at 2020 August 29 08:46:53 UTC
// import "go/token" ==> using token = go.go.token_package
// Original source: C:\Go\src\go\token\position.go
using fmt = go.fmt_package;
using sort = go.sort_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class token_package
    {
        // -----------------------------------------------------------------------------
        // Positions

        // Position describes an arbitrary source position
        // including the file, line, and column location.
        // A Position is valid if the line number is > 0.
        //
        public partial struct Position
        {
            public @string Filename; // filename, if any
            public long Offset; // offset, starting at 0
            public long Line; // line number, starting at 1
            public long Column; // column number, starting at 1 (byte count)
        }

        // IsValid reports whether the position is valid.
        private static bool IsValid(this ref Position pos)
        {
            return pos.Line > 0L;
        }

        // String returns a string in one of several forms:
        //
        //    file:line:column    valid position with file name
        //    line:column         valid position without file name
        //    file                invalid position with file name
        //    -                   invalid position without file name
        //
        public static @string String(this Position pos)
        {
            var s = pos.Filename;
            if (pos.IsValid())
            {
                if (s != "")
                {
                    s += ":";
                }
                s += fmt.Sprintf("%d:%d", pos.Line, pos.Column);
            }
            if (s == "")
            {
                s = "-";
            }
            return s;
        }

        // Pos is a compact encoding of a source position within a file set.
        // It can be converted into a Position for a more convenient, but much
        // larger, representation.
        //
        // The Pos value for a given file is a number in the range [base, base+size],
        // where base and size are specified when adding the file to the file set via
        // AddFile.
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
        public partial struct Pos // : long
        {
        }

        // The zero value for Pos is NoPos; there is no file and line information
        // associated with it, and NoPos.IsValid() is false. NoPos is always
        // smaller than any other Pos value. The corresponding Position value
        // for NoPos is the zero value for Position.
        //
        public static readonly Pos NoPos = 0L;

        // IsValid reports whether the position is valid.


        // IsValid reports whether the position is valid.
        public static bool IsValid(this Pos p)
        {
            return p != NoPos;
        }

        // -----------------------------------------------------------------------------
        // File

        // A File is a handle for a file belonging to a FileSet.
        // A File has a name, size, and line offset table.
        //
        public partial struct File
        {
            public ptr<FileSet> set;
            public @string name; // file name as provided to AddFile
            public long @base; // Pos value range for this file is [base...base+size]
            public long size; // file size as provided to AddFile

// lines and infos are protected by mutex
            public sync.Mutex mutex;
            public slice<long> lines; // lines contains the offset of the first character for each line (the first entry is always 0)
            public slice<lineInfo> infos;
        }

        // Name returns the file name of file f as registered with AddFile.
        private static @string Name(this ref File f)
        {
            return f.name;
        }

        // Base returns the base offset of file f as registered with AddFile.
        private static long Base(this ref File f)
        {
            return f.@base;
        }

        // Size returns the size of file f as registered with AddFile.
        private static long Size(this ref File f)
        {
            return f.size;
        }

        // LineCount returns the number of lines in file f.
        private static long LineCount(this ref File f)
        {
            f.mutex.Lock();
            var n = len(f.lines);
            f.mutex.Unlock();
            return n;
        }

        // AddLine adds the line offset for a new line.
        // The line offset must be larger than the offset for the previous line
        // and smaller than the file size; otherwise the line offset is ignored.
        //
        private static void AddLine(this ref File f, long offset)
        {
            f.mutex.Lock();
            {
                var i = len(f.lines);

                if ((i == 0L || f.lines[i - 1L] < offset) && offset < f.size)
                {
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
        private static void MergeLine(this ref File _f, long line) => func(_f, (ref File f, Defer defer, Panic panic, Recover _) =>
        {
            if (line <= 0L)
            {
                panic("illegal line number (line numbering starts at 1)");
            }
            f.mutex.Lock();
            defer(f.mutex.Unlock());
            if (line >= len(f.lines))
            {
                panic("illegal line number");
            } 
            // To merge the line numbered <line> with the line numbered <line+1>,
            // we need to remove the entry in lines corresponding to the line
            // numbered <line+1>. The entry in lines corresponding to the line
            // numbered <line+1> is located at index <line>, since indices in lines
            // are 0-based and line numbers are 1-based.
            copy(f.lines[line..], f.lines[line + 1L..]);
            f.lines = f.lines[..len(f.lines) - 1L];
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
        private static bool SetLines(this ref File f, slice<long> lines)
        { 
            // verify validity of lines table
            var size = f.size;
            foreach (var (i, offset) in lines)
            {
                if (i > 0L && offset <= lines[i - 1L] || size <= offset)
                {
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
        private static void SetLinesForContent(this ref File f, slice<byte> content)
        {
            slice<long> lines = default;
            long line = 0L;
            foreach (var (offset, b) in content)
            {
                if (line >= 0L)
                {
                    lines = append(lines, line);
                }
                line = -1L;
                if (b == '\n')
                {
                    line = offset + 1L;
                }
            } 

            // set lines table
            f.mutex.Lock();
            f.lines = lines;
            f.mutex.Unlock();
        }

        // A lineInfo object describes alternative file and line number
        // information (such as provided via a //line comment in a .go
        // file) for a given file offset.
        private partial struct lineInfo
        {
            public long Offset;
            public @string Filename;
            public long Line;
        }

        // AddLineInfo adds alternative file and line number information for
        // a given file offset. The offset must be larger than the offset for
        // the previously added alternative line info and smaller than the
        // file size; otherwise the information is ignored.
        //
        // AddLineInfo is typically used to register alternative position
        // information for //line filename:line comments in source files.
        //
        private static void AddLineInfo(this ref File f, long offset, @string filename, long line)
        {
            f.mutex.Lock();
            {
                var i = len(f.infos);

                if (i == 0L || f.infos[i - 1L].Offset < offset && offset < f.size)
                {
                    f.infos = append(f.infos, new lineInfo(offset,filename,line));
                }

            }
            f.mutex.Unlock();
        }

        // Pos returns the Pos value for the given file offset;
        // the offset must be <= f.Size().
        // f.Pos(f.Offset(p)) == p.
        //
        private static Pos Pos(this ref File _f, long offset) => func(_f, (ref File f, Defer _, Panic panic, Recover __) =>
        {
            if (offset > f.size)
            {
                panic("illegal file offset");
            }
            return Pos(f.@base + offset);
        });

        // Offset returns the offset for the given file position p;
        // p must be a valid Pos value in that file.
        // f.Offset(f.Pos(offset)) == offset.
        //
        private static long Offset(this ref File _f, Pos p) => func(_f, (ref File f, Defer _, Panic panic, Recover __) =>
        {
            if (int(p) < f.@base || int(p) > f.@base + f.size)
            {
                panic("illegal Pos value");
            }
            return int(p) - f.@base;
        });

        // Line returns the line number for the given file position p;
        // p must be a Pos value in that file or NoPos.
        //
        private static long Line(this ref File f, Pos p)
        {
            return f.Position(p).Line;
        }

        private static long searchLineInfos(slice<lineInfo> a, long x)
        {
            return sort.Search(len(a), i => a[i].Offset > x) - 1L;
        }

        // unpack returns the filename and line and column number for a file offset.
        // If adjusted is set, unpack will return the filename and line information
        // possibly adjusted by //line comments; otherwise those comments are ignored.
        //
        private static (@string, long, long) unpack(this ref File _f, long offset, bool adjusted) => func(_f, (ref File f, Defer defer, Panic _, Recover __) =>
        {
            f.mutex.Lock();
            defer(f.mutex.Unlock());
            filename = f.name;
            {
                var i__prev1 = i;

                var i = searchInts(f.lines, offset);

                if (i >= 0L)
                {
                    line = i + 1L;
                    column = offset - f.lines[i] + 1L;
                }

                i = i__prev1;

            }
            if (adjusted && len(f.infos) > 0L)
            { 
                // almost no files have extra line infos
                {
                    var i__prev2 = i;

                    i = searchLineInfos(f.infos, offset);

                    if (i >= 0L)
                    {
                        var alt = ref f.infos[i];
                        filename = alt.Filename;
                        {
                            var i__prev3 = i;

                            i = searchInts(f.lines, alt.Offset);

                            if (i >= 0L)
                            {
                                line += alt.Line - i - 1L;
                            }

                            i = i__prev3;

                        }
                    }

                    i = i__prev2;

                }
            }
            return;
        });

        private static Position position(this ref File f, Pos p, bool adjusted)
        {
            var offset = int(p) - f.@base;
            pos.Offset = offset;
            pos.Filename, pos.Line, pos.Column = f.unpack(offset, adjusted);
            return;
        }

        // PositionFor returns the Position value for the given file position p.
        // If adjusted is set, the position may be adjusted by position-altering
        // //line comments; otherwise those comments are ignored.
        // p must be a Pos value in f or NoPos.
        //
        private static Position PositionFor(this ref File _f, Pos p, bool adjusted) => func(_f, (ref File f, Defer _, Panic panic, Recover __) =>
        {
            if (p != NoPos)
            {
                if (int(p) < f.@base || int(p) > f.@base + f.size)
                {
                    panic("illegal Pos value");
                }
                pos = f.position(p, adjusted);
            }
            return;
        });

        // Position returns the Position value for the given file position p.
        // Calling f.Position(p) is equivalent to calling f.PositionFor(p, true).
        //
        private static Position Position(this ref File f, Pos p)
        {
            return f.PositionFor(p, true);
        }

        // -----------------------------------------------------------------------------
        // FileSet

        // A FileSet represents a set of source files.
        // Methods of file sets are synchronized; multiple goroutines
        // may invoke them concurrently.
        //
        public partial struct FileSet
        {
            public sync.RWMutex mutex; // protects the file set
            public long @base; // base offset for the next file
            public slice<ref File> files; // list of files in the order added to the set
            public ptr<File> last; // cache of last file looked up
        }

        // NewFileSet creates a new file set.
        public static ref FileSet NewFileSet()
        {
            return ref new FileSet(base:1,);
        }

        // Base returns the minimum base offset that must be provided to
        // AddFile when adding the next file.
        //
        private static long Base(this ref FileSet s)
        {
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
        private static ref File AddFile(this ref FileSet _s, @string filename, long @base, long size) => func(_s, (ref FileSet s, Defer defer, Panic panic, Recover _) =>
        {
            s.mutex.Lock();
            defer(s.mutex.Unlock());
            if (base < 0L)
            {
                base = s.@base;
            }
            if (base < s.@base || size < 0L)
            {
                panic("illegal base or size");
            } 
            // base >= s.base && size >= 0
            File f = ref new File(set:s,name:filename,base:base,size:size,lines:[]int{0});
            base += size + 1L; // +1 because EOF also has a position
            if (base < 0L)
            {
                panic("token.Pos offset overflow (> 2G of source code in file set)");
            } 
            // add the file to the file set
            s.@base = base;
            s.files = append(s.files, f);
            s.last = f;
            return f;
        });

        // Iterate calls f for the files in the file set in the order they were added
        // until f returns false.
        //
        private static bool Iterate(this ref FileSet s, Func<ref File, bool> f)
        {
            for (long i = 0L; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++)
            {
                ref File file = default;
                s.mutex.RLock();
                if (i < len(s.files))
                {
                    file = s.files[i];
                }
                s.mutex.RUnlock();
                if (file == null || !f(file))
                {
                    break;
                }
            }

        }

        private static long searchFiles(slice<ref File> a, long x)
        {
            return sort.Search(len(a), i => a[i].@base > x) - 1L;
        }

        private static ref File file(this ref FileSet s, Pos p)
        {
            s.mutex.RLock(); 
            // common case: p is in last file
            {
                var f__prev1 = f;

                var f = s.last;

                if (f != null && f.@base <= int(p) && int(p) <= f.@base + f.size)
                {
                    s.mutex.RUnlock();
                    return f;
                } 
                // p is not in last file - search all files

                f = f__prev1;

            } 
            // p is not in last file - search all files
            {
                var i = searchFiles(s.files, int(p));

                if (i >= 0L)
                {
                    f = s.files[i]; 
                    // f.base <= int(p) by definition of searchFiles
                    if (int(p) <= f.@base + f.size)
                    {
                        s.mutex.RUnlock();
                        s.mutex.Lock();
                        s.last = f; // race is ok - s.last is only a cache
                        s.mutex.Unlock();
                        return f;
                    }
                }

            }
            s.mutex.RUnlock();
            return null;
        }

        // File returns the file that contains the position p.
        // If no such file is found (for instance for p == NoPos),
        // the result is nil.
        //
        private static ref File File(this ref FileSet s, Pos p)
        {
            if (p != NoPos)
            {
                f = s.file(p);
            }
            return;
        }

        // PositionFor converts a Pos p in the fileset into a Position value.
        // If adjusted is set, the position may be adjusted by position-altering
        // //line comments; otherwise those comments are ignored.
        // p must be a Pos value in s or NoPos.
        //
        private static Position PositionFor(this ref FileSet s, Pos p, bool adjusted)
        {
            if (p != NoPos)
            {
                {
                    var f = s.file(p);

                    if (f != null)
                    {
                        return f.position(p, adjusted);
                    }

                }
            }
            return;
        }

        // Position converts a Pos p in the fileset into a Position value.
        // Calling s.Position(p) is equivalent to calling s.PositionFor(p, true).
        //
        private static Position Position(this ref FileSet s, Pos p)
        {
            return s.PositionFor(p, true);
        }

        // -----------------------------------------------------------------------------
        // Helper functions

        private static long searchInts(slice<long> a, long x)
        { 
            // This function body is a manually inlined version of:
            //
            //   return sort.Search(len(a), func(i int) bool { return a[i] > x }) - 1
            //
            // With better compiler optimizations, this may not be needed in the
            // future, but at the moment this change improves the go/printer
            // benchmark performance by ~30%. This has a direct impact on the
            // speed of gofmt and thus seems worthwhile (2011-04-29).
            // TODO(gri): Remove this when compilers have caught up.
            long i = 0L;
            var j = len(a);
            while (i < j)
            {
                var h = i + (j - i) / 2L; // avoid overflow when computing h
                // i ≤ h < j
                if (a[h] <= x)
                {
                    i = h + 1L;
                }
                else
                {
                    j = h;
                }
            }

            return i - 1L;
        }
    }
}}
