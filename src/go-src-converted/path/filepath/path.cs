// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package filepath implements utility routines for manipulating filename paths
// in a way compatible with the target operating system-defined file paths.
//
// The filepath package uses either forward slashes or backslashes,
// depending on the operating system. To process paths such as URLs
// that always use forward slashes regardless of the operating
// system, see the path package.
// package filepath -- go2cs converted at 2020 October 09 04:49:46 UTC
// import "path/filepath" ==> using filepath = go.path.filepath_package
// Original source: C:\Go\src\path\filepath\path.go
using errors = go.errors_package;
using os = go.os_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace path
{
    public static partial class filepath_package
    {
        // A lazybuf is a lazily constructed path buffer.
        // It supports append, reading previously appended bytes,
        // and retrieving the final string. It does not allocate a buffer
        // to hold the output until that output diverges from s.
        private partial struct lazybuf
        {
            public @string path;
            public slice<byte> buf;
            public long w;
            public @string volAndPath;
            public long volLen;
        }

        private static byte index(this ptr<lazybuf> _addr_b, long i)
        {
            ref lazybuf b = ref _addr_b.val;

            if (b.buf != null)
            {
                return b.buf[i];
            }

            return b.path[i];

        }

        private static void append(this ptr<lazybuf> _addr_b, byte c)
        {
            ref lazybuf b = ref _addr_b.val;

            if (b.buf == null)
            {
                if (b.w < len(b.path) && b.path[b.w] == c)
                {
                    b.w++;
                    return ;
                }

                b.buf = make_slice<byte>(len(b.path));
                copy(b.buf, b.path[..b.w]);

            }

            b.buf[b.w] = c;
            b.w++;

        }

        private static @string @string(this ptr<lazybuf> _addr_b)
        {
            ref lazybuf b = ref _addr_b.val;

            if (b.buf == null)
            {
                return b.volAndPath[..b.volLen + b.w];
            }

            return b.volAndPath[..b.volLen] + string(b.buf[..b.w]);

        }

        public static readonly var Separator = os.PathSeparator;
        public static readonly var ListSeparator = os.PathListSeparator;


        // Clean returns the shortest path name equivalent to path
        // by purely lexical processing. It applies the following rules
        // iteratively until no further processing can be done:
        //
        //    1. Replace multiple Separator elements with a single one.
        //    2. Eliminate each . path name element (the current directory).
        //    3. Eliminate each inner .. path name element (the parent directory)
        //       along with the non-.. element that precedes it.
        //    4. Eliminate .. elements that begin a rooted path:
        //       that is, replace "/.." by "/" at the beginning of a path,
        //       assuming Separator is '/'.
        //
        // The returned path ends in a slash only if it represents a root directory,
        // such as "/" on Unix or `C:\` on Windows.
        //
        // Finally, any occurrences of slash are replaced by Separator.
        //
        // If the result of this process is an empty string, Clean
        // returns the string ".".
        //
        // See also Rob Pike, ``Lexical File Names in Plan 9 or
        // Getting Dot-Dot Right,''
        // https://9p.io/sys/doc/lexnames.html
        public static @string Clean(@string path)
        {
            var originalPath = path;
            var volLen = volumeNameLen(path);
            path = path[volLen..];
            if (path == "")
            {
                if (volLen > 1L && originalPath[1L] != ':')
                { 
                    // should be UNC
                    return FromSlash(originalPath);

                }

                return originalPath + ".";

            }

            var rooted = os.IsPathSeparator(path[0L]); 

            // Invariants:
            //    reading from path; r is index of next byte to process.
            //    writing to buf; w is index of next byte to write.
            //    dotdot is index in buf where .. must stop, either because
            //        it is the leading slash or it is a leading ../../.. prefix.
            var n = len(path);
            lazybuf @out = new lazybuf(path:path,volAndPath:originalPath,volLen:volLen);
            long r = 0L;
            long dotdot = 0L;
            if (rooted)
            {
                @out.append(Separator);
                r = 1L;
                dotdot = 1L;

            }

            while (r < n)
            {

                if (os.IsPathSeparator(path[r])) 
                    // empty path element
                    r++;
                else if (path[r] == '.' && (r + 1L == n || os.IsPathSeparator(path[r + 1L]))) 
                    // . element
                    r++;
                else if (path[r] == '.' && path[r + 1L] == '.' && (r + 2L == n || os.IsPathSeparator(path[r + 2L]))) 
                    // .. element: remove to last separator
                    r += 2L;

                    if (@out.w > dotdot) 
                        // can backtrack
                        @out.w--;
                        while (@out.w > dotdot && !os.IsPathSeparator(@out.index(@out.w)))
                        {
                            @out.w--;
                        }
                    else if (!rooted) 
                        // cannot backtrack, but not rooted, so append .. element.
                        if (@out.w > 0L)
                        {
                            @out.append(Separator);
                        }

                        @out.append('.');
                        @out.append('.');
                        dotdot = @out.w;
                                    else 
                    // real path element.
                    // add slash if needed
                    if (rooted && @out.w != 1L || !rooted && @out.w != 0L)
                    {
                        @out.append(Separator);
                    } 
                    // copy element
                    while (r < n && !os.IsPathSeparator(path[r]))
                    {
                        @out.append(path[r]);
                        r++;
                    }
                
            } 

            // Turn empty string into "."
 

            // Turn empty string into "."
            if (@out.w == 0L)
            {
                @out.append('.');
            }

            return FromSlash(@out.@string());

        }

        // ToSlash returns the result of replacing each separator character
        // in path with a slash ('/') character. Multiple separators are
        // replaced by multiple slashes.
        public static @string ToSlash(@string path)
        {
            if (Separator == '/')
            {
                return path;
            }

            return strings.ReplaceAll(path, string(Separator), "/");

        }

        // FromSlash returns the result of replacing each slash ('/') character
        // in path with a separator character. Multiple slashes are replaced
        // by multiple separators.
        public static @string FromSlash(@string path)
        {
            if (Separator == '/')
            {
                return path;
            }

            return strings.ReplaceAll(path, "/", string(Separator));

        }

        // SplitList splits a list of paths joined by the OS-specific ListSeparator,
        // usually found in PATH or GOPATH environment variables.
        // Unlike strings.Split, SplitList returns an empty slice when passed an empty
        // string.
        public static slice<@string> SplitList(@string path)
        {
            return splitList(path);
        }

        // Split splits path immediately following the final Separator,
        // separating it into a directory and file name component.
        // If there is no Separator in path, Split returns an empty dir
        // and file set to path.
        // The returned values have the property that path = dir+file.
        public static (@string, @string) Split(@string path)
        {
            @string dir = default;
            @string file = default;

            var vol = VolumeName(path);
            var i = len(path) - 1L;
            while (i >= len(vol) && !os.IsPathSeparator(path[i]))
            {
                i--;
            }

            return (path[..i + 1L], path[i + 1L..]);

        }

        // Join joins any number of path elements into a single path,
        // separating them with an OS specific Separator. Empty elements
        // are ignored. The result is Cleaned. However, if the argument
        // list is empty or all its elements are empty, Join returns
        // an empty string.
        // On Windows, the result will only be a UNC path if the first
        // non-empty element is a UNC path.
        public static @string Join(params @string[] elem)
        {
            elem = elem.Clone();

            return join(elem);
        }

        // Ext returns the file name extension used by path.
        // The extension is the suffix beginning at the final dot
        // in the final element of path; it is empty if there is
        // no dot.
        public static @string Ext(@string path)
        {
            for (var i = len(path) - 1L; i >= 0L && !os.IsPathSeparator(path[i]); i--)
            {
                if (path[i] == '.')
                {
                    return path[i..];
                }

            }

            return "";

        }

        // EvalSymlinks returns the path name after the evaluation of any symbolic
        // links.
        // If path is relative the result will be relative to the current directory,
        // unless one of the components is an absolute symbolic link.
        // EvalSymlinks calls Clean on the result.
        public static (@string, error) EvalSymlinks(@string path)
        {
            @string _p0 = default;
            error _p0 = default!;

            return evalSymlinks(path);
        }

        // Abs returns an absolute representation of path.
        // If the path is not absolute it will be joined with the current
        // working directory to turn it into an absolute path. The absolute
        // path name for a given file is not guaranteed to be unique.
        // Abs calls Clean on the result.
        public static (@string, error) Abs(@string path)
        {
            @string _p0 = default;
            error _p0 = default!;

            return abs(path);
        }

        private static (@string, error) unixAbs(@string path)
        {
            @string _p0 = default;
            error _p0 = default!;

            if (IsAbs(path))
            {
                return (Clean(path), error.As(null!)!);
            }

            var (wd, err) = os.Getwd();
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            return (Join(wd, path), error.As(null!)!);

        }

        // Rel returns a relative path that is lexically equivalent to targpath when
        // joined to basepath with an intervening separator. That is,
        // Join(basepath, Rel(basepath, targpath)) is equivalent to targpath itself.
        // On success, the returned path will always be relative to basepath,
        // even if basepath and targpath share no elements.
        // An error is returned if targpath can't be made relative to basepath or if
        // knowing the current working directory would be necessary to compute it.
        // Rel calls Clean on the result.
        public static (@string, error) Rel(@string basepath, @string targpath)
        {
            @string _p0 = default;
            error _p0 = default!;

            var baseVol = VolumeName(basepath);
            var targVol = VolumeName(targpath);
            var @base = Clean(basepath);
            var targ = Clean(targpath);
            if (sameWord(targ, base))
            {
                return (".", error.As(null!)!);
            }

            base = base[len(baseVol)..];
            targ = targ[len(targVol)..];
            if (base == ".")
            {
                base = "";
            } 
            // Can't use IsAbs - `\a` and `a` are both relative in Windows.
            var baseSlashed = len(base) > 0L && base[0L] == Separator;
            var targSlashed = len(targ) > 0L && targ[0L] == Separator;
            if (baseSlashed != targSlashed || !sameWord(baseVol, targVol))
            {
                return ("", error.As(errors.New("Rel: can't make " + targpath + " relative to " + basepath))!);
            } 
            // Position base[b0:bi] and targ[t0:ti] at the first differing elements.
            var bl = len(base);
            var tl = len(targ);
            long b0 = default;            long bi = default;            long t0 = default;            long ti = default;

            while (true)
            {
                while (bi < bl && base[bi] != Separator)
                {
                    bi++;
                }

                while (ti < tl && targ[ti] != Separator)
                {
                    ti++;
                }

                if (!sameWord(targ[t0..ti], base[b0..bi]))
                {
                    break;
                }

                if (bi < bl)
                {
                    bi++;
                }

                if (ti < tl)
                {
                    ti++;
                }

                b0 = bi;
                t0 = ti;

            }

            if (base[b0..bi] == "..")
            {
                return ("", error.As(errors.New("Rel: can't make " + targpath + " relative to " + basepath))!);
            }

            if (b0 != bl)
            { 
                // Base elements left. Must go up before going down.
                var seps = strings.Count(base[b0..bl], string(Separator));
                long size = 2L + seps * 3L;
                if (tl != t0)
                {
                    size += 1L + tl - t0;
                }

                var buf = make_slice<byte>(size);
                var n = copy(buf, "..");
                for (long i = 0L; i < seps; i++)
                {
                    buf[n] = Separator;
                    copy(buf[n + 1L..], "..");
                    n += 3L;
                }

                if (t0 != tl)
                {
                    buf[n] = Separator;
                    copy(buf[n + 1L..], targ[t0..]);
                }

                return (string(buf), error.As(null!)!);

            }

            return (targ[t0..], error.As(null!)!);

        }

        // SkipDir is used as a return value from WalkFuncs to indicate that
        // the directory named in the call is to be skipped. It is not returned
        // as an error by any function.
        public static var SkipDir = errors.New("skip this directory");

        // WalkFunc is the type of the function called for each file or directory
        // visited by Walk. The path argument contains the argument to Walk as a
        // prefix; that is, if Walk is called with "dir", which is a directory
        // containing the file "a", the walk function will be called with argument
        // "dir/a". The info argument is the os.FileInfo for the named path.
        //
        // If there was a problem walking to the file or directory named by path, the
        // incoming error will describe the problem and the function can decide how
        // to handle that error (and Walk will not descend into that directory). In the
        // case of an error, the info argument will be nil. If an error is returned,
        // processing stops. The sole exception is when the function returns the special
        // value SkipDir. If the function returns SkipDir when invoked on a directory,
        // Walk skips the directory's contents entirely. If the function returns SkipDir
        // when invoked on a non-directory file, Walk skips the remaining files in the
        // containing directory.
        public delegate  error WalkFunc(@string,  os.FileInfo,  error);

        private static var lstat = os.Lstat; // for testing

        // walk recursively descends path, calling walkFn.
        private static error walk(@string path, os.FileInfo info, WalkFunc walkFn)
        {
            if (!info.IsDir())
            {
                return error.As(walkFn(path, info, null))!;
            }

            var (names, err) = readDirNames(path);
            var err1 = walkFn(path, info, err); 
            // If err != nil, walk can't walk into this directory.
            // err1 != nil means walkFn want walk to skip this directory or stop walking.
            // Therefore, if one of err and err1 isn't nil, walk will return.
            if (err != null || err1 != null)
            { 
                // The caller's behavior is controlled by the return value, which is decided
                // by walkFn. walkFn may ignore err and return nil.
                // If walkFn returns SkipDir, it will be handled by the caller.
                // So walk should return whatever walkFn returns.
                return error.As(err1)!;

            }

            foreach (var (_, name) in names)
            {
                var filename = Join(path, name);
                var (fileInfo, err) = lstat(filename);
                if (err != null)
                {
                    {
                        var err = walkFn(filename, fileInfo, err);

                        if (err != null && err != SkipDir)
                        {
                            return error.As(err)!;
                        }

                    }

                }
                else
                {
                    err = walk(filename, fileInfo, walkFn);
                    if (err != null)
                    {
                        if (!fileInfo.IsDir() || err != SkipDir)
                        {
                            return error.As(err)!;
                        }

                    }

                }

            }
            return error.As(null!)!;

        }

        // Walk walks the file tree rooted at root, calling walkFn for each file or
        // directory in the tree, including root. All errors that arise visiting files
        // and directories are filtered by walkFn. The files are walked in lexical
        // order, which makes the output deterministic but means that for very
        // large directories Walk can be inefficient.
        // Walk does not follow symbolic links.
        public static error Walk(@string root, WalkFunc walkFn)
        {
            var (info, err) = os.Lstat(root);
            if (err != null)
            {
                err = walkFn(root, null, err);
            }
            else
            {
                err = walk(root, info, walkFn);
            }

            if (err == SkipDir)
            {
                return error.As(null!)!;
            }

            return error.As(err)!;

        }

        // readDirNames reads the directory named by dirname and returns
        // a sorted list of directory entries.
        private static (slice<@string>, error) readDirNames(@string dirname)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;

            var (f, err) = os.Open(dirname);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            var (names, err) = f.Readdirnames(-1L);
            f.Close();
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            sort.Strings(names);
            return (names, error.As(null!)!);

        }

        // Base returns the last element of path.
        // Trailing path separators are removed before extracting the last element.
        // If the path is empty, Base returns ".".
        // If the path consists entirely of separators, Base returns a single separator.
        public static @string Base(@string path)
        {
            if (path == "")
            {
                return ".";
            } 
            // Strip trailing slashes.
            while (len(path) > 0L && os.IsPathSeparator(path[len(path) - 1L]))
            {
                path = path[0L..len(path) - 1L];
            } 
            // Throw away volume name
 
            // Throw away volume name
            path = path[len(VolumeName(path))..]; 
            // Find the last element
            var i = len(path) - 1L;
            while (i >= 0L && !os.IsPathSeparator(path[i]))
            {
                i--;
            }

            if (i >= 0L)
            {
                path = path[i + 1L..];
            } 
            // If empty now, it had only slashes.
            if (path == "")
            {
                return string(Separator);
            }

            return path;

        }

        // Dir returns all but the last element of path, typically the path's directory.
        // After dropping the final element, Dir calls Clean on the path and trailing
        // slashes are removed.
        // If the path is empty, Dir returns ".".
        // If the path consists entirely of separators, Dir returns a single separator.
        // The returned path does not end in a separator unless it is the root directory.
        public static @string Dir(@string path)
        {
            var vol = VolumeName(path);
            var i = len(path) - 1L;
            while (i >= len(vol) && !os.IsPathSeparator(path[i]))
            {
                i--;
            }

            var dir = Clean(path[len(vol)..i + 1L]);
            if (dir == "." && len(vol) > 2L)
            { 
                // must be UNC
                return vol;

            }

            return vol + dir;

        }

        // VolumeName returns leading volume name.
        // Given "C:\foo\bar" it returns "C:" on Windows.
        // Given "\\host\share\foo" it returns "\\host\share".
        // On other platforms it returns "".
        public static @string VolumeName(@string path)
        {
            return path[..volumeNameLen(path)];
        }
    }
}}
