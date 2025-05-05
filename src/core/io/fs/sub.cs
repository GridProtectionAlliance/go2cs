// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using errors = errors_package;
using path = path_package;

partial class fs_package {

// A SubFS is a file system with a Sub method.
[GoType] partial interface SubFS :
    FS
{
    // Sub returns an FS corresponding to the subtree rooted at dir.
    (FS, error) Sub(@string dir);
}

// Sub returns an [FS] corresponding to the subtree rooted at fsys's dir.
//
// If dir is ".", Sub returns fsys unchanged.
// Otherwise, if fs implements [SubFS], Sub returns fsys.Sub(dir).
// Otherwise, Sub returns a new [FS] implementation sub that,
// in effect, implements sub.Open(name) as fsys.Open(path.Join(dir, name)).
// The implementation also translates calls to ReadDir, ReadFile, and Glob appropriately.
//
// Note that Sub(os.DirFS("/"), "prefix") is equivalent to os.DirFS("/prefix")
// and that neither of them guarantees to avoid operating system
// accesses outside "/prefix", because the implementation of [os.DirFS]
// does not check for symbolic links inside "/prefix" that point to
// other directories. That is, [os.DirFS] is not a general substitute for a
// chroot-style security mechanism, and Sub does not change that fact.
public static (FS, error) Sub(FS fsys, @string dir) {
    if (!ValidPath(dir)) {
        return (default!, new PathError(Op: "sub"u8, Path: dir, Err: ErrInvalid));
    }
    if (dir == "."u8) {
        return (fsys, default!);
    }
    {
        var (fsysΔ1, ok) = fsys._<SubFS>(ᐧ); if (ok) {
            return fsysΔ1.Sub(dir);
        }
    }
    return (new subFS(fsys, dir), default!);
}

[GoType] partial struct subFS {
    internal FS fsys;
    internal @string dir;
}

// fullName maps name to the fully-qualified name dir/name.
[GoRecv] internal static (@string, error) fullName(this ref subFS f, @string op, @string name) {
    if (!ValidPath(name)) {
        return ("", new PathError(Op: op, Path: name, Err: ErrInvalid));
    }
    return (path.Join(f.dir, name), default!);
}

// shorten maps name, which should start with f.dir, back to the suffix after f.dir.
[GoRecv] internal static (@string rel, bool ok) shorten(this ref subFS f, @string name) {
    @string rel = default!;
    bool ok = default!;

    if (name == f.dir) {
        return (".", true);
    }
    if (len(name) >= len(f.dir) + 2 && name[len(f.dir)] == (rune)'/' && name[..(int)(len(f.dir))] == f.dir) {
        return (name[(int)(len(f.dir) + 1)..], true);
    }
    return ("", false);
}

// fixErr shortens any reported names in PathErrors by stripping f.dir.
[GoRecv] internal static error fixErr(this ref subFS f, error err) {
    {
        var (e, ok) = err._<PathError.val>(ᐧ); if (ok) {
            {
                var (@short, okΔ1) = f.shorten((~e).Path); if (okΔ1) {
                    e.val.Path = @short;
                }
            }
        }
    }
    return err;
}

[GoRecv] internal static (File, error) Open(this ref subFS f, @string name) {
    var (full, err) = f.fullName("open"u8, name);
    if (err != default!) {
        return (default!, err);
    }
    (file, err) = f.fsys.Open(full);
    return (file, f.fixErr(err));
}

[GoRecv] internal static (slice<DirEntry>, error) ReadDir(this ref subFS f, @string name) {
    var (full, err) = f.fullName("read"u8, name);
    if (err != default!) {
        return (default!, err);
    }
    (dir, err) = ReadDir(f.fsys, full);
    return (dir, f.fixErr(err));
}

[GoRecv] internal static (slice<byte>, error) ReadFile(this ref subFS f, @string name) {
    var (full, err) = f.fullName("read"u8, name);
    if (err != default!) {
        return (default!, err);
    }
    (data, err) = ReadFile(f.fsys, full);
    return (data, f.fixErr(err));
}

[GoRecv] internal static (slice<@string>, error) Glob(this ref subFS f, @string pattern) {
    // Check pattern is well-formed.
    {
        var (_, errΔ1) = path.Match(pattern, ""u8); if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
    }
    if (pattern == "."u8) {
        return (new @string[]{"."}.slice(), default!);
    }
    @string full = f.dir + "/"u8 + pattern;
    (list, err) = Glob(f.fsys, full);
    foreach (var (i, name) in list) {
        var (nameΔ1, ok) = f.shorten(name);
        if (!ok) {
            return (default!, errors.New("invalid result from inner fsys Glob: "u8 + nameΔ1 + " not in "u8 + f.dir));
        }
        // can't use fmt in this package
        list[i] = nameΔ1;
    }
    return (list, f.fixErr(err));
}

[GoRecv("capture")] internal static (FS, error) Sub(this ref subFS f, @string dir) {
    if (dir == "."u8) {
        return (~f, default!);
    }
    (full, err) = f.fullName("sub"u8, dir);
    if (err != default!) {
        return (default!, err);
    }
    return (new subFS(f.fsys, full), default!);
}

} // end fs_package
