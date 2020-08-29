// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package path implements utility routines for manipulating slash-separated
// paths.
//
// The path package should only be used for paths separated by forward
// slashes, such as the paths in URLs. This package does not deal with
// Windows paths with drive letters or backslashes; to manipulate
// operating system paths, use the path/filepath package.
// package path -- go2cs converted at 2020 August 29 08:43:28 UTC
// import "path" ==> using path = go.path_package
// Original source: C:\Go\src\path\path.go
using strings = go.strings_package;
using static go.builtin;

namespace go
{
    public static partial class path_package
    {
        // A lazybuf is a lazily constructed path buffer.
        // It supports append, reading previously appended bytes,
        // and retrieving the final string. It does not allocate a buffer
        // to hold the output until that output diverges from s.
        private partial struct lazybuf
        {
            public @string s;
            public slice<byte> buf;
            public long w;
        }

        private static byte index(this ref lazybuf b, long i)
        {
            if (b.buf != null)
            {
                return b.buf[i];
            }
            return b.s[i];
        }

        private static void append(this ref lazybuf b, byte c)
        {
            if (b.buf == null)
            {
                if (b.w < len(b.s) && b.s[b.w] == c)
                {
                    b.w++;
                    return;
                }
                b.buf = make_slice<byte>(len(b.s));
                copy(b.buf, b.s[..b.w]);
            }
            b.buf[b.w] = c;
            b.w++;
        }

        private static @string @string(this ref lazybuf b)
        {
            if (b.buf == null)
            {
                return b.s[..b.w];
            }
            return string(b.buf[..b.w]);
        }

        // Clean returns the shortest path name equivalent to path
        // by purely lexical processing. It applies the following rules
        // iteratively until no further processing can be done:
        //
        //    1. Replace multiple slashes with a single slash.
        //    2. Eliminate each . path name element (the current directory).
        //    3. Eliminate each inner .. path name element (the parent directory)
        //       along with the non-.. element that precedes it.
        //    4. Eliminate .. elements that begin a rooted path:
        //       that is, replace "/.." by "/" at the beginning of a path.
        //
        // The returned path ends in a slash only if it is the root "/".
        //
        // If the result of this process is an empty string, Clean
        // returns the string ".".
        //
        // See also Rob Pike, ``Lexical File Names in Plan 9 or
        // Getting Dot-Dot Right,''
        // https://9p.io/sys/doc/lexnames.html
        public static @string Clean(@string path)
        {
            if (path == "")
            {
                return ".";
            }
            var rooted = path[0L] == '/';
            var n = len(path); 

            // Invariants:
            //    reading from path; r is index of next byte to process.
            //    writing to buf; w is index of next byte to write.
            //    dotdot is index in buf where .. must stop, either because
            //        it is the leading slash or it is a leading ../../.. prefix.
            lazybuf @out = new lazybuf(s:path);
            long r = 0L;
            long dotdot = 0L;
            if (rooted)
            {
                @out.append('/');
                r = 1L;
                dotdot = 1L;
            }
            while (r < n)
            {

                if (path[r] == '/') 
                    // empty path element
                    r++;
                else if (path[r] == '.' && (r + 1L == n || path[r + 1L] == '/')) 
                    // . element
                    r++;
                else if (path[r] == '.' && path[r + 1L] == '.' && (r + 2L == n || path[r + 2L] == '/')) 
                    // .. element: remove to last /
                    r += 2L;

                    if (@out.w > dotdot) 
                        // can backtrack
                        @out.w--;
                        while (@out.w > dotdot && @out.index(@out.w) != '/')
                        {
                            @out.w--;
                        }
                    else if (!rooted) 
                        // cannot backtrack, but not rooted, so append .. element.
                        if (@out.w > 0L)
                        {
                            @out.append('/');
                        }
                        @out.append('.');
                        @out.append('.');
                        dotdot = @out.w;
                                    else 
                    // real path element.
                    // add slash if needed
                    if (rooted && @out.w != 1L || !rooted && @out.w != 0L)
                    {
                        @out.append('/');
                    } 
                    // copy element
                    while (r < n && path[r] != '/')
                    {
                        @out.append(path[r]);
                        r++;
                    }
                            } 

            // Turn empty string into "."
 

            // Turn empty string into "."
            if (@out.w == 0L)
            {
                return ".";
            }
            return @out.@string();
        }

        // Split splits path immediately following the final slash,
        // separating it into a directory and file name component.
        // If there is no slash in path, Split returns an empty dir and
        // file set to path.
        // The returned values have the property that path = dir+file.
        public static (@string, @string) Split(@string path)
        {
            var i = strings.LastIndex(path, "/");
            return (path[..i + 1L], path[i + 1L..]);
        }

        // Join joins any number of path elements into a single path, adding a
        // separating slash if necessary. The result is Cleaned; in particular,
        // all empty strings are ignored.
        public static @string Join(params @string[] elem)
        {
            elem = elem.Clone();

            foreach (var (i, e) in elem)
            {
                if (e != "")
                {
                    return Clean(strings.Join(elem[i..], "/"));
                }
            }
            return "";
        }

        // Ext returns the file name extension used by path.
        // The extension is the suffix beginning at the final dot
        // in the final slash-separated element of path;
        // it is empty if there is no dot.
        public static @string Ext(@string path)
        {
            for (var i = len(path) - 1L; i >= 0L && path[i] != '/'; i--)
            {
                if (path[i] == '.')
                {
                    return path[i..];
                }
            }

            return "";
        }

        // Base returns the last element of path.
        // Trailing slashes are removed before extracting the last element.
        // If the path is empty, Base returns ".".
        // If the path consists entirely of slashes, Base returns "/".
        public static @string Base(@string path)
        {
            if (path == "")
            {
                return ".";
            } 
            // Strip trailing slashes.
            while (len(path) > 0L && path[len(path) - 1L] == '/')
            {
                path = path[0L..len(path) - 1L];
            } 
            // Find the last element
 
            // Find the last element
            {
                var i = strings.LastIndex(path, "/");

                if (i >= 0L)
                {
                    path = path[i + 1L..];
                } 
                // If empty now, it had only slashes.

            } 
            // If empty now, it had only slashes.
            if (path == "")
            {
                return "/";
            }
            return path;
        }

        // IsAbs reports whether the path is absolute.
        public static bool IsAbs(@string path)
        {
            return len(path) > 0L && path[0L] == '/';
        }

        // Dir returns all but the last element of path, typically the path's directory.
        // After dropping the final element using Split, the path is Cleaned and trailing
        // slashes are removed.
        // If the path is empty, Dir returns ".".
        // If the path consists entirely of slashes followed by non-slash bytes, Dir
        // returns a single slash. In any other case, the returned path does not end in a
        // slash.
        public static @string Dir(@string path)
        {
            var (dir, _) = Split(path);
            return Clean(dir);
        }
    }
}
