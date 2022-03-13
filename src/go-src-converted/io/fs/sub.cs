// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package fs -- go2cs converted at 2022 March 13 05:27:47 UTC
// import "io/fs" ==> using fs = go.io.fs_package
// Original source: C:\Program Files\Go\src\io\fs\sub.go
namespace go.io;

using errors = errors_package;
using path = path_package;


// A SubFS is a file system with a Sub method.

public static partial class fs_package {

public partial interface SubFS {
    (FS, error) Sub(@string dir);
}

// Sub returns an FS corresponding to the subtree rooted at fsys's dir.
//
// If dir is ".", Sub returns fsys unchanged.
// Otherwise, if fs implements SubFS, Sub returns fsys.Sub(dir).
// Otherwise, Sub returns a new FS implementation sub that,
// in effect, implements sub.Open(name) as fsys.Open(path.Join(dir, name)).
// The implementation also translates calls to ReadDir, ReadFile, and Glob appropriately.
//
// Note that Sub(os.DirFS("/"), "prefix") is equivalent to os.DirFS("/prefix")
// and that neither of them guarantees to avoid operating system
// accesses outside "/prefix", because the implementation of os.DirFS
// does not check for symbolic links inside "/prefix" that point to
// other directories. That is, os.DirFS is not a general substitute for a
// chroot-style security mechanism, and Sub does not change that fact.
public static (FS, error) Sub(FS fsys, @string dir) {
    FS _p0 = default;
    error _p0 = default!;

    if (!ValidPath(dir)) {
        return (null, error.As(addr(new PathError(Op:"sub",Path:dir,Err:errors.New("invalid name")))!)!);
    }
    if (dir == ".") {
        return (fsys, error.As(null!)!);
    }
    {
        SubFS (fsys, ok) = SubFS.As(fsys._<SubFS>())!;

        if (ok) {
            return fsys.Sub(dir);
        }
    }
    return (addr(new subFS(fsys,dir)), error.As(null!)!);
}

private partial struct subFS {
    public FS fsys;
    public @string dir;
}

// fullName maps name to the fully-qualified name dir/name.
private static (@string, error) fullName(this ptr<subFS> _addr_f, @string op, @string name) {
    @string _p0 = default;
    error _p0 = default!;
    ref subFS f = ref _addr_f.val;

    if (!ValidPath(name)) {
        return ("", error.As(addr(new PathError(Op:op,Path:name,Err:errors.New("invalid name")))!)!);
    }
    return (path.Join(f.dir, name), error.As(null!)!);
}

// shorten maps name, which should start with f.dir, back to the suffix after f.dir.
private static (@string, bool) shorten(this ptr<subFS> _addr_f, @string name) {
    @string rel = default;
    bool ok = default;
    ref subFS f = ref _addr_f.val;

    if (name == f.dir) {
        return (".", true);
    }
    if (len(name) >= len(f.dir) + 2 && name[len(f.dir)] == '/' && name[..(int)len(f.dir)] == f.dir) {
        return (name[(int)len(f.dir) + 1..], true);
    }
    return ("", false);
}

// fixErr shortens any reported names in PathErrors by stripping f.dir.
private static error fixErr(this ptr<subFS> _addr_f, error err) {
    ref subFS f = ref _addr_f.val;

    {
        ptr<PathError> (e, ok) = err._<ptr<PathError>>();

        if (ok) {
            {
                var (short, ok) = f.shorten(e.Path);

                if (ok) {
                    e.Path = short;
                }

            }
        }
    }
    return error.As(err)!;
}

private static (File, error) Open(this ptr<subFS> _addr_f, @string name) {
    File _p0 = default;
    error _p0 = default!;
    ref subFS f = ref _addr_f.val;

    var (full, err) = f.fullName("open", name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var (file, err) = f.fsys.Open(full);
    return (file, error.As(f.fixErr(err))!);
}

private static (slice<DirEntry>, error) ReadDir(this ptr<subFS> _addr_f, @string name) {
    slice<DirEntry> _p0 = default;
    error _p0 = default!;
    ref subFS f = ref _addr_f.val;

    var (full, err) = f.fullName("read", name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var (dir, err) = ReadDir(f.fsys, full);
    return (dir, error.As(f.fixErr(err))!);
}

private static (slice<byte>, error) ReadFile(this ptr<subFS> _addr_f, @string name) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref subFS f = ref _addr_f.val;

    var (full, err) = f.fullName("read", name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var (data, err) = ReadFile(f.fsys, full);
    return (data, error.As(f.fixErr(err))!);
}

private static (slice<@string>, error) Glob(this ptr<subFS> _addr_f, @string pattern) {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref subFS f = ref _addr_f.val;
 
    // Check pattern is well-formed.
    {
        var (_, err) = path.Match(pattern, "");

        if (err != null) {
            return (null, error.As(err)!);
        }
    }
    if (pattern == ".") {
        return (new slice<@string>(new @string[] { "." }), error.As(null!)!);
    }
    var full = f.dir + "/" + pattern;
    var (list, err) = Glob(f.fsys, full);
    {
        var name__prev1 = name;

        foreach (var (__i, __name) in list) {
            i = __i;
            name = __name;
            var (name, ok) = f.shorten(name);
            if (!ok) {
                return (null, error.As(errors.New("invalid result from inner fsys Glob: " + name + " not in " + f.dir))!); // can't use fmt in this package
            }
            list[i] = name;
        }
        name = name__prev1;
    }

    return (list, error.As(f.fixErr(err))!);
}

private static (FS, error) Sub(this ptr<subFS> _addr_f, @string dir) {
    FS _p0 = default;
    error _p0 = default!;
    ref subFS f = ref _addr_f.val;

    if (dir == ".") {
        return (f, error.As(null!)!);
    }
    var (full, err) = f.fullName("sub", dir);
    if (err != null) {
        return (null, error.As(err)!);
    }
    return (addr(new subFS(f.fsys,full)), error.As(null!)!);
}

} // end fs_package
