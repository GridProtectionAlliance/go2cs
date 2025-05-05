// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.testing;

using io = io_package;
using fs = io.fs_package;
using path = path_package;
using slices = slices_package;
using strings = strings_package;
using time = time_package;
using io;

partial class fstest_package {
/* visitMapType: map[string]*MapFile */

// A MapFile describes a single file in a [MapFS].
[GoType] partial struct MapFile {
    public slice<byte> Data; // file content
    public io.fs_package.FileMode Mode; // fs.FileInfo.Mode
    public time_package.Time ModTime;   // fs.FileInfo.ModTime
    public any Sys;         // fs.FileInfo.Sys
}

internal static fs.FS _ = ((MapFS)default!);

internal static fs.File _ = (ж<openMapFile>)(default!);

// Open opens the named file.
public static (fs.File, error) Open(this MapFS fsys, @string name) {
    if (!fs.ValidPath(name)) {
        return (default!, new fs.PathError(Op: "open"u8, Path: name, Err: fs.ErrNotExist));
    }
    var file = fsys[name];
    if (file != nil && (fs.FileMode)((~file).Mode & fs.ModeDir) == 0) {
        // Ordinary file
        return (new openMapFile(name, new mapFileInfo(path.Base(name), file), 0), default!);
    }
    // Directory, possibly synthesized.
    // Note that file can be nil here: the map need not contain explicit parent directories for all its files.
    // But file can also be non-nil, in case the user wants to set metadata for the directory explicitly.
    // Either way, we need to construct the list of children of this directory.
    slice<mapFileInfo> list = default!;
    @string elem = default!;
    map<@string, bool> need = new map<@string, bool>();
    if (name == "."u8){
        elem = "."u8;
        foreach (var (fname, f) in fsys) {
            nint i = strings.Index(fname, "/"u8);
            if (i < 0){
                if (fname != "."u8) {
                    list = append(list, new mapFileInfo(fname, f));
                }
            } else {
                need[fname[..(int)(i)]] = true;
            }
        }
    } else {
        elem = name[(int)(strings.LastIndex(name, "/"u8) + 1)..];
        @string prefix = name + "/"u8;
        foreach (var (fname, f) in fsys) {
            if (strings.HasPrefix(fname, prefix)) {
                @string felem = fname[(int)(len(prefix))..];
                nint i = strings.Index(felem, "/"u8);
                if (i < 0){
                    list = append(list, new mapFileInfo(felem, f));
                } else {
                    need[fname[(int)(len(prefix))..(int)(len(prefix) + i)]] = true;
                }
            }
        }
        // If the directory name is not in the map,
        // and there are no children of the name in the map,
        // then the directory is treated as not existing.
        if (file == nil && list == default! && len(need) == 0) {
            return (default!, new fs.PathError(Op: "open"u8, Path: name, Err: fs.ErrNotExist));
        }
    }
    foreach (var (_, fi) in list) {
        delete(need, fi.name);
    }
    foreach (var (nameΔ1, _) in need) {
        list = append(list, new mapFileInfo(nameΔ1, Ꮡ(new MapFile(Mode: (fs.FileMode)(fs.ModeDir | 365)))));
    }
    slices.SortFunc(list, (mapFileInfo a, mapFileInfo b) => strings.Compare(a.name, b.name));
    if (file == nil) {
        file = Ꮡ(new MapFile(Mode: (fs.FileMode)(fs.ModeDir | 365)));
    }
    return (new mapDir(name, new mapFileInfo(elem, file), list, 0), default!);
}

// fsOnly is a wrapper that hides all but the fs.FS methods,
// to avoid an infinite recursion when implementing special
// methods in terms of helpers that would use them.
// (In general, implementing these methods using the package fs helpers
// is redundant and unnecessary, but having the methods may make
// MapFS exercise more code paths when used in tests.)
[GoType] partial struct fsOnly {
    public partial ref io.fs_package.FS FS { get; }
}

public static (slice<byte>, error) ReadFile(this MapFS fsys, @string name) {
    return fs.ReadFile(new fsOnly(fsys), name);
}

public static (fs.FileInfo, error) Stat(this MapFS fsys, @string name) {
    return fs.Stat(new fsOnly(fsys), name);
}

public static (slice<fs.DirEntry>, error) ReadDir(this MapFS fsys, @string name) {
    return fs.ReadDir(new fsOnly(fsys), name);
}

public static (slice<@string>, error) Glob(this MapFS fsys, @string pattern) {
    return fs.Glob(new fsOnly(fsys), pattern);
}

[GoType] partial struct noSub {
    public partial ref MapFS MapFS { get; }
}

internal static void Sub(this noSub _) {
}

// not the fs.SubFS signature
public static (fs.FS, error) Sub(this MapFS fsys, @string dir) {
    return fs.Sub(new noSub(fsys), dir);
}

// A mapFileInfo implements fs.FileInfo and fs.DirEntry for a given map file.
[GoType] partial struct mapFileInfo {
    internal @string name;
    internal ж<MapFile> f;
}

[GoRecv] internal static @string Name(this ref mapFileInfo i) {
    return path.Base(i.name);
}

[GoRecv] internal static int64 Size(this ref mapFileInfo i) {
    return ((int64)len(i.f.Data));
}

[GoRecv] internal static fs.FileMode Mode(this ref mapFileInfo i) {
    return i.f.Mode;
}

[GoRecv] internal static fs.FileMode Type(this ref mapFileInfo i) {
    return i.f.Mode.Type();
}

[GoRecv] internal static time.Time ModTime(this ref mapFileInfo i) {
    return i.f.ModTime;
}

[GoRecv] internal static bool IsDir(this ref mapFileInfo i) {
    return (fs.FileMode)(i.f.Mode & fs.ModeDir) != 0;
}

[GoRecv] internal static any Sys(this ref mapFileInfo i) {
    return i.f.Sys;
}

[GoRecv("capture")] internal static (fs.FileInfo, error) Info(this ref mapFileInfo i) {
    return (~i, default!);
}

[GoRecv] internal static @string String(this ref mapFileInfo i) {
    return fs.FormatFileInfo(~i);
}

// An openMapFile is a regular (non-directory) fs.File open for reading.
[GoType] partial struct openMapFile {
    internal @string path;
    internal partial ref mapFileInfo mapFileInfo { get; }
    internal int64 offset;
}

[GoRecv] internal static (fs.FileInfo, error) Stat(this ref openMapFile f) {
    return (f.mapFileInfo, default!);
}

[GoRecv] internal static error Close(this ref openMapFile f) {
    return default!;
}

[GoRecv] internal static (nint, error) Read(this ref openMapFile f, slice<byte> b) {
    if (f.offset >= ((int64)len(f.f.Data))) {
        return (0, io.EOF);
    }
    if (f.offset < 0) {
        return (0, new fs.PathError(Op: "read"u8, Path: f.path, Err: fs.ErrInvalid));
    }
    nint n = copy(b, f.f.Data[(int)(f.offset)..]);
    f.offset += ((int64)n);
    return (n, default!);
}

[GoRecv] internal static (int64, error) Seek(this ref openMapFile f, int64 offset, nint whence) {
    switch (whence) {
    case 0: {
        break;
    }
    case 1: {
        offset += f.offset;
        break;
    }
    case 2: {
        offset += ((int64)len(f.f.Data));
        break;
    }}

    // offset += 0
    if (offset < 0 || offset > ((int64)len(f.f.Data))) {
        return (0, new fs.PathError(Op: "seek"u8, Path: f.path, Err: fs.ErrInvalid));
    }
    f.offset = offset;
    return (offset, default!);
}

[GoRecv] internal static (nint, error) ReadAt(this ref openMapFile f, slice<byte> b, int64 offset) {
    if (offset < 0 || offset > ((int64)len(f.f.Data))) {
        return (0, new fs.PathError(Op: "read"u8, Path: f.path, Err: fs.ErrInvalid));
    }
    nint n = copy(b, f.f.Data[(int)(offset)..]);
    if (n < len(b)) {
        return (n, io.EOF);
    }
    return (n, default!);
}

// A mapDir is a directory fs.File (so also an fs.ReadDirFile) open for reading.
[GoType] partial struct mapDir {
    internal @string path;
    internal partial ref mapFileInfo mapFileInfo { get; }
    internal slice<mapFileInfo> entry;
    internal nint offset;
}

[GoRecv] internal static (fs.FileInfo, error) Stat(this ref mapDir d) {
    return (d.mapFileInfo, default!);
}

[GoRecv] internal static error Close(this ref mapDir d) {
    return default!;
}

[GoRecv] internal static (nint, error) Read(this ref mapDir d, slice<byte> b) {
    return (0, new fs.PathError(Op: "read"u8, Path: d.path, Err: fs.ErrInvalid));
}

[GoRecv] internal static (slice<fs.DirEntry>, error) ReadDir(this ref mapDir d, nint count) {
    nint n = len(d.entry) - d.offset;
    if (n == 0 && count > 0) {
        return (default!, io.EOF);
    }
    if (count > 0 && n > count) {
        n = count;
    }
    var list = new slice<fs.DirEntry>(n);
    foreach (var (i, _) in list) {
        list[i] = Ꮡ(d.entry[d.offset + i]);
    }
    d.offset += n;
    return (list, default!);
}

} // end fstest_package
