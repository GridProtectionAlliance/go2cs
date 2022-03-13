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

// package filepath -- go2cs converted at 2022 March 13 05:28:16 UTC
// import "path/filepath" ==> using filepath = go.path.filepath_package
// Original source: C:\Program Files\Go\src\path\filepath\path.go
namespace go.path;

using errors = errors_package;
using fs = io.fs_package;
using os = os_package;
using sort = sort_package;
using strings = strings_package;


// A lazybuf is a lazily constructed path buffer.
// It supports append, reading previously appended bytes,
// and retrieving the final string. It does not allocate a buffer
// to hold the output until that output diverges from s.

using System;
public static partial class filepath_package {

private partial struct lazybuf {
    public @string path;
    public slice<byte> buf;
    public nint w;
    public @string volAndPath;
    public nint volLen;
}

private static byte index(this ptr<lazybuf> _addr_b, nint i) {
    ref lazybuf b = ref _addr_b.val;

    if (b.buf != null) {
        return b.buf[i];
    }
    return b.path[i];
}

private static void append(this ptr<lazybuf> _addr_b, byte c) {
    ref lazybuf b = ref _addr_b.val;

    if (b.buf == null) {
        if (b.w < len(b.path) && b.path[b.w] == c) {
            b.w++;
            return ;
        }
        b.buf = make_slice<byte>(len(b.path));
        copy(b.buf, b.path[..(int)b.w]);
    }
    b.buf[b.w] = c;
    b.w++;
}

private static @string @string(this ptr<lazybuf> _addr_b) {
    ref lazybuf b = ref _addr_b.val;

    if (b.buf == null) {
        return b.volAndPath[..(int)b.volLen + b.w];
    }
    return b.volAndPath[..(int)b.volLen] + string(b.buf[..(int)b.w]);
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
public static @string Clean(@string path) {
    var originalPath = path;
    var volLen = volumeNameLen(path);
    path = path[(int)volLen..];
    if (path == "") {
        if (volLen > 1 && originalPath[1] != ':') { 
            // should be UNC
            return FromSlash(originalPath);
        }
        return originalPath + ".";
    }
    var rooted = os.IsPathSeparator(path[0]); 

    // Invariants:
    //    reading from path; r is index of next byte to process.
    //    writing to buf; w is index of next byte to write.
    //    dotdot is index in buf where .. must stop, either because
    //        it is the leading slash or it is a leading ../../.. prefix.
    var n = len(path);
    lazybuf @out = new lazybuf(path:path,volAndPath:originalPath,volLen:volLen);
    nint r = 0;
    nint dotdot = 0;
    if (rooted) {
        @out.append(Separator);
        (r, dotdot) = (1, 1);
    }
    while (r < n) {

        if (os.IsPathSeparator(path[r])) 
            // empty path element
            r++;
        else if (path[r] == '.' && (r + 1 == n || os.IsPathSeparator(path[r + 1]))) 
            // . element
            r++;
        else if (path[r] == '.' && path[r + 1] == '.' && (r + 2 == n || os.IsPathSeparator(path[r + 2]))) 
            // .. element: remove to last separator
            r += 2;

            if (@out.w > dotdot) 
                // can backtrack
                @out.w--;
                while (@out.w > dotdot && !os.IsPathSeparator(@out.index(@out.w))) {
                    @out.w--;
                }
            else if (!rooted) 
                // cannot backtrack, but not rooted, so append .. element.
                if (@out.w > 0) {
                    @out.append(Separator);
                }
                @out.append('.');
                @out.append('.');
                dotdot = @out.w;
                    else 
            // real path element.
            // add slash if needed
            if (rooted && @out.w != 1 || !rooted && @out.w != 0) {
                @out.append(Separator);
            } 
            // copy element
            while (r < n && !os.IsPathSeparator(path[r])) {
                @out.append(path[r]);
                r++;
            }
            } 

    // Turn empty string into "."
    if (@out.w == 0) {
        @out.append('.');
    }
    return FromSlash(@out.@string());
}

// ToSlash returns the result of replacing each separator character
// in path with a slash ('/') character. Multiple separators are
// replaced by multiple slashes.
public static @string ToSlash(@string path) {
    if (Separator == '/') {
        return path;
    }
    return strings.ReplaceAll(path, string(Separator), "/");
}

// FromSlash returns the result of replacing each slash ('/') character
// in path with a separator character. Multiple slashes are replaced
// by multiple separators.
public static @string FromSlash(@string path) {
    if (Separator == '/') {
        return path;
    }
    return strings.ReplaceAll(path, "/", string(Separator));
}

// SplitList splits a list of paths joined by the OS-specific ListSeparator,
// usually found in PATH or GOPATH environment variables.
// Unlike strings.Split, SplitList returns an empty slice when passed an empty
// string.
public static slice<@string> SplitList(@string path) {
    return splitList(path);
}

// Split splits path immediately following the final Separator,
// separating it into a directory and file name component.
// If there is no Separator in path, Split returns an empty dir
// and file set to path.
// The returned values have the property that path = dir+file.
public static (@string, @string) Split(@string path) {
    @string dir = default;
    @string file = default;

    var vol = VolumeName(path);
    var i = len(path) - 1;
    while (i >= len(vol) && !os.IsPathSeparator(path[i])) {
        i--;
    }
    return (path[..(int)i + 1], path[(int)i + 1..]);
}

// Join joins any number of path elements into a single path,
// separating them with an OS specific Separator. Empty elements
// are ignored. The result is Cleaned. However, if the argument
// list is empty or all its elements are empty, Join returns
// an empty string.
// On Windows, the result will only be a UNC path if the first
// non-empty element is a UNC path.
public static @string Join(params @string[] elem) {
    elem = elem.Clone();

    return join(elem);
}

// Ext returns the file name extension used by path.
// The extension is the suffix beginning at the final dot
// in the final element of path; it is empty if there is
// no dot.
public static @string Ext(@string path) {
    for (var i = len(path) - 1; i >= 0 && !os.IsPathSeparator(path[i]); i--) {
        if (path[i] == '.') {
            return path[(int)i..];
        }
    }
    return "";
}

// EvalSymlinks returns the path name after the evaluation of any symbolic
// links.
// If path is relative the result will be relative to the current directory,
// unless one of the components is an absolute symbolic link.
// EvalSymlinks calls Clean on the result.
public static (@string, error) EvalSymlinks(@string path) {
    @string _p0 = default;
    error _p0 = default!;

    return evalSymlinks(path);
}

// Abs returns an absolute representation of path.
// If the path is not absolute it will be joined with the current
// working directory to turn it into an absolute path. The absolute
// path name for a given file is not guaranteed to be unique.
// Abs calls Clean on the result.
public static (@string, error) Abs(@string path) {
    @string _p0 = default;
    error _p0 = default!;

    return abs(path);
}

private static (@string, error) unixAbs(@string path) {
    @string _p0 = default;
    error _p0 = default!;

    if (IsAbs(path)) {
        return (Clean(path), error.As(null!)!);
    }
    var (wd, err) = os.Getwd();
    if (err != null) {
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
public static (@string, error) Rel(@string basepath, @string targpath) {
    @string _p0 = default;
    error _p0 = default!;

    var baseVol = VolumeName(basepath);
    var targVol = VolumeName(targpath);
    var @base = Clean(basepath);
    var targ = Clean(targpath);
    if (sameWord(targ, base)) {
        return (".", error.As(null!)!);
    }
    base = base[(int)len(baseVol)..];
    targ = targ[(int)len(targVol)..];
    if (base == ".") {
        base = "";
    }
    else if (base == "" && volumeNameLen(baseVol) > 2) { 
        // Treat any targetpath matching `\\host\share` basepath as absolute path.
        base = string(Separator);
    }
    var baseSlashed = len(base) > 0 && base[0] == Separator;
    var targSlashed = len(targ) > 0 && targ[0] == Separator;
    if (baseSlashed != targSlashed || !sameWord(baseVol, targVol)) {
        return ("", error.As(errors.New("Rel: can't make " + targpath + " relative to " + basepath))!);
    }
    var bl = len(base);
    var tl = len(targ);
    nint b0 = default;    nint bi = default;    nint t0 = default;    nint ti = default;

    while (true) {
        while (bi < bl && base[bi] != Separator) {
            bi++;
        }
        while (ti < tl && targ[ti] != Separator) {
            ti++;
        }
        if (!sameWord(targ[(int)t0..(int)ti], base[(int)b0..(int)bi])) {
            break;
        }
        if (bi < bl) {
            bi++;
        }
        if (ti < tl) {
            ti++;
        }
        b0 = bi;
        t0 = ti;
    }
    if (base[(int)b0..(int)bi] == "..") {
        return ("", error.As(errors.New("Rel: can't make " + targpath + " relative to " + basepath))!);
    }
    if (b0 != bl) { 
        // Base elements left. Must go up before going down.
        var seps = strings.Count(base[(int)b0..(int)bl], string(Separator));
        nint size = 2 + seps * 3;
        if (tl != t0) {
            size += 1 + tl - t0;
        }
        var buf = make_slice<byte>(size);
        var n = copy(buf, "..");
        for (nint i = 0; i < seps; i++) {
            buf[n] = Separator;
            copy(buf[(int)n + 1..], "..");
            n += 3;
        }
        if (t0 != tl) {
            buf[n] = Separator;
            copy(buf[(int)n + 1..], targ[(int)t0..]);
        }
        return (string(buf), error.As(null!)!);
    }
    return (targ[(int)t0..], error.As(null!)!);
}

// SkipDir is used as a return value from WalkFuncs to indicate that
// the directory named in the call is to be skipped. It is not returned
// as an error by any function.
public static error SkipDir = error.As(fs.SkipDir)!;

// WalkFunc is the type of the function called by Walk to visit each
// file or directory.
//
// The path argument contains the argument to Walk as a prefix.
// That is, if Walk is called with root argument "dir" and finds a file
// named "a" in that directory, the walk function will be called with
// argument "dir/a".
//
// The directory and file are joined with Join, which may clean the
// directory name: if Walk is called with the root argument "x/../dir"
// and finds a file named "a" in that directory, the walk function will
// be called with argument "dir/a", not "x/../dir/a".
//
// The info argument is the fs.FileInfo for the named path.
//
// The error result returned by the function controls how Walk continues.
// If the function returns the special value SkipDir, Walk skips the
// current directory (path if info.IsDir() is true, otherwise path's
// parent directory). Otherwise, if the function returns a non-nil error,
// Walk stops entirely and returns that error.
//
// The err argument reports an error related to path, signaling that Walk
// will not walk into that directory. The function can decide how to
// handle that error; as described earlier, returning the error will
// cause Walk to stop walking the entire tree.
//
// Walk calls the function with a non-nil err argument in two cases.
//
// First, if an os.Lstat on the root directory or any directory or file
// in the tree fails, Walk calls the function with path set to that
// directory or file's path, info set to nil, and err set to the error
// from os.Lstat.
//
// Second, if a directory's Readdirnames method fails, Walk calls the
// function with path set to the directory's path, info, set to an
// fs.FileInfo describing the directory, and err set to the error from
// Readdirnames.
public delegate  error WalkFunc(@string,  fs.FileInfo,  error);

private static var lstat = os.Lstat; // for testing

// walkDir recursively descends path, calling walkDirFn.
private static error walkDir(@string path, fs.DirEntry d, fs.WalkDirFunc walkDirFn) {
    {
        var err__prev1 = err;

        var err = walkDirFn(path, d, null);

        if (err != null || !d.IsDir()) {
            if (err == SkipDir && d.IsDir()) { 
                // Successfully skipped directory.
                err = null;
            }
            return error.As(err)!;
        }
        err = err__prev1;

    }

    var (dirs, err) = readDir(path);
    if (err != null) { 
        // Second call, to report ReadDir error.
        err = walkDirFn(path, d, err);
        if (err != null) {
            return error.As(err)!;
        }
    }
    foreach (var (_, d1) in dirs) {
        var path1 = Join(path, d1.Name());
        {
            var err__prev1 = err;

            err = walkDir(path1, d1, walkDirFn);

            if (err != null) {
                if (err == SkipDir) {
                    break;
                }
                return error.As(err)!;
            }

            err = err__prev1;

        }
    }    return error.As(null!)!;
}

// walk recursively descends path, calling walkFn.
private static error walk(@string path, fs.FileInfo info, WalkFunc walkFn) {
    if (!info.IsDir()) {
        return error.As(walkFn(path, info, null))!;
    }
    var (names, err) = readDirNames(path);
    var err1 = walkFn(path, info, err); 
    // If err != nil, walk can't walk into this directory.
    // err1 != nil means walkFn want walk to skip this directory or stop walking.
    // Therefore, if one of err and err1 isn't nil, walk will return.
    if (err != null || err1 != null) { 
        // The caller's behavior is controlled by the return value, which is decided
        // by walkFn. walkFn may ignore err and return nil.
        // If walkFn returns SkipDir, it will be handled by the caller.
        // So walk should return whatever walkFn returns.
        return error.As(err1)!;
    }
    foreach (var (_, name) in names) {
        var filename = Join(path, name);
        var (fileInfo, err) = lstat(filename);
        if (err != null) {
            {
                var err = walkFn(filename, fileInfo, err);

                if (err != null && err != SkipDir) {
                    return error.As(err)!;
                }

            }
        }
        else
 {
            err = walk(filename, fileInfo, walkFn);
            if (err != null) {
                if (!fileInfo.IsDir() || err != SkipDir) {
                    return error.As(err)!;
                }
            }
        }
    }    return error.As(null!)!;
}

// WalkDir walks the file tree rooted at root, calling fn for each file or
// directory in the tree, including root.
//
// All errors that arise visiting files and directories are filtered by fn:
// see the fs.WalkDirFunc documentation for details.
//
// The files are walked in lexical order, which makes the output deterministic
// but requires WalkDir to read an entire directory into memory before proceeding
// to walk that directory.
//
// WalkDir does not follow symbolic links.
public static error WalkDir(@string root, fs.WalkDirFunc fn) {
    var (info, err) = os.Lstat(root);
    if (err != null) {
        err = fn(root, null, err);
    }
    else
 {
        err = walkDir(root, addr(new statDirEntry(info)), fn);
    }
    if (err == SkipDir) {
        return error.As(null!)!;
    }
    return error.As(err)!;
}

private partial struct statDirEntry {
    public fs.FileInfo info;
}

private static @string Name(this ptr<statDirEntry> _addr_d) {
    ref statDirEntry d = ref _addr_d.val;

    return d.info.Name();
}
private static bool IsDir(this ptr<statDirEntry> _addr_d) {
    ref statDirEntry d = ref _addr_d.val;

    return d.info.IsDir();
}
private static fs.FileMode Type(this ptr<statDirEntry> _addr_d) {
    ref statDirEntry d = ref _addr_d.val;

    return d.info.Mode().Type();
}
private static (fs.FileInfo, error) Info(this ptr<statDirEntry> _addr_d) {
    fs.FileInfo _p0 = default;
    error _p0 = default!;
    ref statDirEntry d = ref _addr_d.val;

    return (d.info, error.As(null!)!);
}

// Walk walks the file tree rooted at root, calling fn for each file or
// directory in the tree, including root.
//
// All errors that arise visiting files and directories are filtered by fn:
// see the WalkFunc documentation for details.
//
// The files are walked in lexical order, which makes the output deterministic
// but requires Walk to read an entire directory into memory before proceeding
// to walk that directory.
//
// Walk does not follow symbolic links.
//
// Walk is less efficient than WalkDir, introduced in Go 1.16,
// which avoids calling os.Lstat on every visited file or directory.
public static error Walk(@string root, WalkFunc fn) {
    var (info, err) = os.Lstat(root);
    if (err != null) {
        err = fn(root, null, err);
    }
    else
 {
        err = walk(root, info, fn);
    }
    if (err == SkipDir) {
        return error.As(null!)!;
    }
    return error.As(err)!;
}

// readDir reads the directory named by dirname and returns
// a sorted list of directory entries.
private static (slice<fs.DirEntry>, error) readDir(@string dirname) {
    slice<fs.DirEntry> _p0 = default;
    error _p0 = default!;

    var (f, err) = os.Open(dirname);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var (dirs, err) = f.ReadDir(-1);
    f.Close();
    if (err != null) {
        return (null, error.As(err)!);
    }
    sort.Slice(dirs, (i, j) => dirs[i].Name() < dirs[j].Name());
    return (dirs, error.As(null!)!);
}

// readDirNames reads the directory named by dirname and returns
// a sorted list of directory entry names.
private static (slice<@string>, error) readDirNames(@string dirname) {
    slice<@string> _p0 = default;
    error _p0 = default!;

    var (f, err) = os.Open(dirname);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var (names, err) = f.Readdirnames(-1);
    f.Close();
    if (err != null) {
        return (null, error.As(err)!);
    }
    sort.Strings(names);
    return (names, error.As(null!)!);
}

// Base returns the last element of path.
// Trailing path separators are removed before extracting the last element.
// If the path is empty, Base returns ".".
// If the path consists entirely of separators, Base returns a single separator.
public static @string Base(@string path) {
    if (path == "") {
        return ".";
    }
    while (len(path) > 0 && os.IsPathSeparator(path[len(path) - 1])) {
        path = path[(int)0..(int)len(path) - 1];
    } 
    // Throw away volume name
    path = path[(int)len(VolumeName(path))..]; 
    // Find the last element
    var i = len(path) - 1;
    while (i >= 0 && !os.IsPathSeparator(path[i])) {
        i--;
    }
    if (i >= 0) {
        path = path[(int)i + 1..];
    }
    if (path == "") {
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
public static @string Dir(@string path) {
    var vol = VolumeName(path);
    var i = len(path) - 1;
    while (i >= len(vol) && !os.IsPathSeparator(path[i])) {
        i--;
    }
    var dir = Clean(path[(int)len(vol)..(int)i + 1]);
    if (dir == "." && len(vol) > 2) { 
        // must be UNC
        return vol;
    }
    return vol + dir;
}

// VolumeName returns leading volume name.
// Given "C:\foo\bar" it returns "C:" on Windows.
// Given "\\host\share\foo" it returns "\\host\share".
// On other platforms it returns "".
public static @string VolumeName(@string path) {
    return path[..(int)volumeNameLen(path)];
}

} // end filepath_package
