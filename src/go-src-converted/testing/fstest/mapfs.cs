// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package fstest -- go2cs converted at 2022 March 06 23:19:26 UTC
// import "testing/fstest" ==> using fstest = go.testing.fstest_package
// Original source: C:\Program Files\Go\src\testing\fstest\mapfs.go
using io = go.io_package;
using fs = go.io.fs_package;
using path = go.path_package;
using sort = go.sort_package;
using strings = go.strings_package;
using time = go.time_package;
using System;


namespace go.testing;

public static partial class fstest_package {

    // A MapFS is a simple in-memory file system for use in tests,
    // represented as a map from path names (arguments to Open)
    // to information about the files or directories they represent.
    //
    // The map need not include parent directories for files contained
    // in the map; those will be synthesized if needed.
    // But a directory can still be included by setting the MapFile.Mode's ModeDir bit;
    // this may be necessary for detailed control over the directory's FileInfo
    // or to create an empty directory.
    //
    // File system operations read directly from the map,
    // so that the file system can be changed by editing the map as needed.
    // An implication is that file system operations must not run concurrently
    // with changes to the map, which would be a race.
    // Another implication is that opening or reading a directory requires
    // iterating over the entire map, so a MapFS should typically be used with not more
    // than a few hundred entries or directory reads.
public partial struct MapFS { // : map<@string, ptr<MapFile>>
}

// A MapFile describes a single file in a MapFS.
public partial struct MapFile {
    public slice<byte> Data; // file content
    public fs.FileMode Mode; // FileInfo.Mode
    public time.Time ModTime; // FileInfo.ModTime
}

private static fs.FS _ = MapFS(null);
private static fs.File _ = (openMapFile.val)(null);

// Open opens the named file.
public static (fs.File, error) Open(this MapFS fsys, @string name) {
    fs.File _p0 = default;
    error _p0 = default!;

    if (!fs.ValidPath(name)) {
        return (null, error.As(addr(new fs.PathError(Op:"open",Path:name,Err:fs.ErrNotExist))!)!);
    }
    var file = fsys[name];
    if (file != null && file.Mode & fs.ModeDir == 0) { 
        // Ordinary file
        return (addr(new openMapFile(name,mapFileInfo{path.Base(name),file},0)), error.As(null!)!);

    }
    slice<mapFileInfo> list = default;
    @string elem = default;
    var need = make_map<@string, bool>();
    if (name == ".") {
        elem = ".";
        {
            var fname__prev1 = fname;
            var f__prev1 = f;

            foreach (var (__fname, __f) in fsys) {
                fname = __fname;
                f = __f;
                var i = strings.Index(fname, "/");
                if (i < 0) {
                    list = append(list, new mapFileInfo(fname,f));
                }
                else
 {
                    need[fname[..(int)i]] = true;
                }

            }
    else

            fname = fname__prev1;
            f = f__prev1;
        }
    } {
        elem = name[(int)strings.LastIndex(name, "/") + 1..];
        var prefix = name + "/";
        {
            var fname__prev1 = fname;
            var f__prev1 = f;

            foreach (var (__fname, __f) in fsys) {
                fname = __fname;
                f = __f;
                if (strings.HasPrefix(fname, prefix)) {
                    var felem = fname[(int)len(prefix)..];
                    i = strings.Index(felem, "/");
                    if (i < 0) {
                        list = append(list, new mapFileInfo(felem,f));
                    }
                    else
 {
                        need[fname[(int)len(prefix)..(int)len(prefix) + i]] = true;
                    }

                }

            } 
            // If the directory name is not in the map,
            // and there are no children of the name in the map,
            // then the directory is treated as not existing.

            fname = fname__prev1;
            f = f__prev1;
        }

        if (file == null && list == null && len(need) == 0) {
            return (null, error.As(addr(new fs.PathError(Op:"open",Path:name,Err:fs.ErrNotExist))!)!);
        }
    }
    foreach (var (_, fi) in list) {
        delete(need, fi.name);
    }    foreach (var (name) in need) {
        list = append(list, new mapFileInfo(name,&MapFile{Mode:fs.ModeDir}));
    }    sort.Slice(list, (i, j) => {
        return list[i].name < list[j].name;
    });

    if (file == null) {
        file = addr(new MapFile(Mode:fs.ModeDir));
    }
    return (addr(new mapDir(name,mapFileInfo{elem,file},list,0)), error.As(null!)!);

}

// fsOnly is a wrapper that hides all but the fs.FS methods,
// to avoid an infinite recursion when implementing special
// methods in terms of helpers that would use them.
// (In general, implementing these methods using the package fs helpers
// is redundant and unnecessary, but having the methods may make
// MapFS exercise more code paths when used in tests.)
private partial struct fsOnly : fs.FS {
    public ref fs.FS FS => ref FS_val;
}

public static (slice<byte>, error) ReadFile(this MapFS fsys, @string name) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    return fs.ReadFile(new fsOnly(fsys), name);
}

public static (fs.FileInfo, error) Stat(this MapFS fsys, @string name) {
    fs.FileInfo _p0 = default;
    error _p0 = default!;

    return fs.Stat(new fsOnly(fsys), name);
}

public static (slice<fs.DirEntry>, error) ReadDir(this MapFS fsys, @string name) {
    slice<fs.DirEntry> _p0 = default;
    error _p0 = default!;

    return fs.ReadDir(new fsOnly(fsys), name);
}

public static (slice<@string>, error) Glob(this MapFS fsys, @string pattern) {
    slice<@string> _p0 = default;
    error _p0 = default!;

    return fs.Glob(new fsOnly(fsys), pattern);
}

private partial struct noSub {
    public ref MapFS MapFS => ref MapFS_val;
}

private static void Sub(this noSub _p0) {
} // not the fs.SubFS signature

public static (fs.FS, error) Sub(this MapFS fsys, @string dir) {
    fs.FS _p0 = default;
    error _p0 = default!;

    return fs.Sub(new noSub(fsys), dir);
}

// A mapFileInfo implements fs.FileInfo and fs.DirEntry for a given map file.
private partial struct mapFileInfo {
    public @string name;
    public ptr<MapFile> f;
}

private static @string Name(this ptr<mapFileInfo> _addr_i) {
    ref mapFileInfo i = ref _addr_i.val;

    return i.name;
}
private static long Size(this ptr<mapFileInfo> _addr_i) {
    ref mapFileInfo i = ref _addr_i.val;

    return int64(len(i.f.Data));
}
private static fs.FileMode Mode(this ptr<mapFileInfo> _addr_i) {
    ref mapFileInfo i = ref _addr_i.val;

    return i.f.Mode;
}
private static fs.FileMode Type(this ptr<mapFileInfo> _addr_i) {
    ref mapFileInfo i = ref _addr_i.val;

    return i.f.Mode.Type();
}
private static time.Time ModTime(this ptr<mapFileInfo> _addr_i) {
    ref mapFileInfo i = ref _addr_i.val;

    return i.f.ModTime;
}
private static bool IsDir(this ptr<mapFileInfo> _addr_i) {
    ref mapFileInfo i = ref _addr_i.val;

    return i.f.Mode & fs.ModeDir != 0;
}
private static void Sys(this ptr<mapFileInfo> _addr_i) {
    ref mapFileInfo i = ref _addr_i.val;

    return i.f.Sys;
}
private static (fs.FileInfo, error) Info(this ptr<mapFileInfo> _addr_i) {
    fs.FileInfo _p0 = default;
    error _p0 = default!;
    ref mapFileInfo i = ref _addr_i.val;

    return (i, error.As(null!)!);
}

// An openMapFile is a regular (non-directory) fs.File open for reading.
private partial struct openMapFile {
    public @string path;
    public ref mapFileInfo mapFileInfo => ref mapFileInfo_val;
    public long offset;
}

private static (fs.FileInfo, error) Stat(this ptr<openMapFile> _addr_f) {
    fs.FileInfo _p0 = default;
    error _p0 = default!;
    ref openMapFile f = ref _addr_f.val;

    return (_addr_f.mapFileInfo, error.As(null!)!);
}

private static error Close(this ptr<openMapFile> _addr_f) {
    ref openMapFile f = ref _addr_f.val;

    return error.As(null!)!;
}

private static (nint, error) Read(this ptr<openMapFile> _addr_f, slice<byte> b) {
    nint _p0 = default;
    error _p0 = default!;
    ref openMapFile f = ref _addr_f.val;

    if (f.offset >= int64(len(f.f.Data))) {
        return (0, error.As(io.EOF)!);
    }
    if (f.offset < 0) {
        return (0, error.As(addr(new fs.PathError(Op:"read",Path:f.path,Err:fs.ErrInvalid))!)!);
    }
    var n = copy(b, f.f.Data[(int)f.offset..]);
    f.offset += int64(n);
    return (n, error.As(null!)!);

}

private static (long, error) Seek(this ptr<openMapFile> _addr_f, long offset, nint whence) {
    long _p0 = default;
    error _p0 = default!;
    ref openMapFile f = ref _addr_f.val;

    switch (whence) {
        case 0: 

            break;
        case 1: 
            offset += f.offset;
            break;
        case 2: 
            offset += int64(len(f.f.Data));
            break;
    }
    if (offset < 0 || offset > int64(len(f.f.Data))) {
        return (0, error.As(addr(new fs.PathError(Op:"seek",Path:f.path,Err:fs.ErrInvalid))!)!);
    }
    f.offset = offset;
    return (offset, error.As(null!)!);

}

private static (nint, error) ReadAt(this ptr<openMapFile> _addr_f, slice<byte> b, long offset) {
    nint _p0 = default;
    error _p0 = default!;
    ref openMapFile f = ref _addr_f.val;

    if (offset < 0 || offset > int64(len(f.f.Data))) {
        return (0, error.As(addr(new fs.PathError(Op:"read",Path:f.path,Err:fs.ErrInvalid))!)!);
    }
    var n = copy(b, f.f.Data[(int)offset..]);
    if (n < len(b)) {
        return (n, error.As(io.EOF)!);
    }
    return (n, error.As(null!)!);

}

// A mapDir is a directory fs.File (so also an fs.ReadDirFile) open for reading.
private partial struct mapDir {
    public @string path;
    public ref mapFileInfo mapFileInfo => ref mapFileInfo_val;
    public slice<mapFileInfo> entry;
    public nint offset;
}

private static (fs.FileInfo, error) Stat(this ptr<mapDir> _addr_d) {
    fs.FileInfo _p0 = default;
    error _p0 = default!;
    ref mapDir d = ref _addr_d.val;

    return (_addr_d.mapFileInfo, error.As(null!)!);
}
private static error Close(this ptr<mapDir> _addr_d) {
    ref mapDir d = ref _addr_d.val;

    return error.As(null!)!;
}
private static (nint, error) Read(this ptr<mapDir> _addr_d, slice<byte> b) {
    nint _p0 = default;
    error _p0 = default!;
    ref mapDir d = ref _addr_d.val;

    return (0, error.As(addr(new fs.PathError(Op:"read",Path:d.path,Err:fs.ErrInvalid))!)!);
}

private static (slice<fs.DirEntry>, error) ReadDir(this ptr<mapDir> _addr_d, nint count) {
    slice<fs.DirEntry> _p0 = default;
    error _p0 = default!;
    ref mapDir d = ref _addr_d.val;

    var n = len(d.entry) - d.offset;
    if (n == 0 && count > 0) {
        return (null, error.As(io.EOF)!);
    }
    if (count > 0 && n > count) {
        n = count;
    }
    var list = make_slice<fs.DirEntry>(n);
    foreach (var (i) in list) {
        list[i] = _addr_d.entry[d.offset + i];
    }    d.offset += n;
    return (list, error.As(null!)!);

}

} // end fstest_package
