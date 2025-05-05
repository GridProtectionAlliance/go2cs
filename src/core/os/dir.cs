// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using DirEntry = go.io.fs_package.DirEntry;

namespace go;

using bytealg = @internal.bytealg_package;
using filepathlite = @internal.filepathlite_package;
using io = io_package;
using fs = io.fs_package;
using slices = slices_package;
using @internal;
using io;

partial class os_package {

[GoType("num:nint")] partial struct readdirMode;

internal static readonly readdirMode readdirName = /* iota */ 0;
internal static readonly readdirMode readdirDirEntry = 1;
internal static readonly readdirMode readdirFileInfo = 2;

// Readdir reads the contents of the directory associated with file and
// returns a slice of up to n [FileInfo] values, as would be returned
// by [Lstat], in directory order. Subsequent calls on the same file will yield
// further FileInfos.
//
// If n > 0, Readdir returns at most n FileInfo structures. In this case, if
// Readdir returns an empty slice, it will return a non-nil error
// explaining why. At the end of a directory, the error is [io.EOF].
//
// If n <= 0, Readdir returns all the FileInfo from the directory in
// a single slice. In this case, if Readdir succeeds (reads all
// the way to the end of the directory), it returns the slice and a
// nil error. If it encounters an error before the end of the
// directory, Readdir returns the FileInfo read until that point
// and a non-nil error.
//
// Most clients are better served by the more efficient ReadDir method.
[GoRecv] public static (slice<FileInfo>, error) Readdir(this ref File f, nint n) {
    if (f == nil) {
        return (default!, ErrInvalid);
    }
    (_, _, infos, err) = f.readdir(n, readdirFileInfo);
    if (infos == default!) {
        // Readdir has historically always returned a non-nil empty slice, never nil,
        // even on error (except misuse with nil receiver above).
        // Keep it that way to avoid breaking overly sensitive callers.
        infos = new FileInfo[]{}.slice();
    }
    return (infos, err);
}

// Readdirnames reads the contents of the directory associated with file
// and returns a slice of up to n names of files in the directory,
// in directory order. Subsequent calls on the same file will yield
// further names.
//
// If n > 0, Readdirnames returns at most n names. In this case, if
// Readdirnames returns an empty slice, it will return a non-nil error
// explaining why. At the end of a directory, the error is [io.EOF].
//
// If n <= 0, Readdirnames returns all the names from the directory in
// a single slice. In this case, if Readdirnames succeeds (reads all
// the way to the end of the directory), it returns the slice and a
// nil error. If it encounters an error before the end of the
// directory, Readdirnames returns the names read until that point and
// a non-nil error.
[GoRecv] public static (slice<@string> names, error err) Readdirnames(this ref File f, nint n) {
    slice<@string> names = default!;
    error err = default!;

    if (f == nil) {
        return (default!, ErrInvalid);
    }
    (names, _, _, err) = f.readdir(n, readdirName);
    if (names == default!) {
        // Readdirnames has historically always returned a non-nil empty slice, never nil,
        // even on error (except misuse with nil receiver above).
        // Keep it that way to avoid breaking overly sensitive callers.
        names = new @string[]{}.slice();
    }
    return (names, err);
}

// ReadDir reads the contents of the directory associated with the file f
// and returns a slice of [DirEntry] values in directory order.
// Subsequent calls on the same file will yield later DirEntry records in the directory.
//
// If n > 0, ReadDir returns at most n DirEntry records.
// In this case, if ReadDir returns an empty slice, it will return an error explaining why.
// At the end of a directory, the error is [io.EOF].
//
// If n <= 0, ReadDir returns all the DirEntry records remaining in the directory.
// When it succeeds, it returns a nil error (not io.EOF).
[GoRecv] public static (slice<DirEntry>, error) ReadDir(this ref File f, nint n) {
    if (f == nil) {
        return (default!, ErrInvalid);
    }
    (_, dirents, _, err) = f.readdir(n, readdirDirEntry);
    if (dirents == default!) {
        // Match Readdir and Readdirnames: don't return nil slices.
        dirents = new DirEntry[]{}.slice();
    }
    return (dirents, err);
}

// testingForceReadDirLstat forces ReadDir to call Lstat, for testing that code path.
// This can be difficult to provoke on some Unix systems otherwise.
internal static bool testingForceReadDirLstat;

// ReadDir reads the named directory,
// returning all its directory entries sorted by filename.
// If an error occurs reading the directory,
// ReadDir returns the entries it was able to read before the error,
// along with the error.
public static (slice<DirEntry>, error) ReadDir(@string name) => func((defer, _) => {
    (f, err) = openDir(name);
    if (err != default!) {
        return (default!, err);
    }
    var fʗ1 = f;
    defer(fʗ1.Close);
    (dirs, err) = f.ReadDir(-1);
    slices.SortFunc(dirs, 
    (DirEntry a, DirEntry b) => bytealg.CompareString(a.Name(), b.Name()));
    return (dirs, err);
});

// CopyFS copies the file system fsys into the directory dir,
// creating dir if necessary.
//
// Files are created with mode 0o666 plus any execute permissions
// from the source, and directories are created with mode 0o777
// (before umask).
//
// CopyFS will not overwrite existing files. If a file name in fsys
// already exists in the destination, CopyFS will return an error
// such that errors.Is(err, fs.ErrExist) will be true.
//
// Symbolic links in fsys are not supported. A *PathError with Err set
// to ErrInvalid is returned when copying from a symbolic link.
//
// Symbolic links in dir are followed.
//
// Copying stops at and returns the first error encountered.
public static error CopyFS(@string dir, fs.FS fsys) => func((defer, _) => {
    return fs.WalkDir(fsys, "."u8, (@string path, fs.DirEntry d, error err) => {
        if (err != default!) {
            return err;
        }
        var (fpath, err) = filepathlite.Localize(path);
        if (err != default!) {
            return err;
        }
        @string newPath = joinPath(dir, fpath);
        if (d.IsDir()) {
            return MkdirAll(newPath, 511);
        }
        // TODO(panjf2000): handle symlinks with the help of fs.ReadLinkFS
        // 		once https://go.dev/issue/49580 is done.
        //		we also need filepathlite.IsLocal from https://go.dev/cl/564295.
        if (!d.Type().IsRegular()) {
            return new PathError{Op: "CopyFS"u8, Path: path, Err: ErrInvalid};
        }
        (r, err) = fsys.Open(path);
        if (err != default!) {
            return err;
        }
        var rʗ1 = r;
        defer(rʗ1.Close);
        (info, err) = r.Stat();
        if (err != default!) {
            return err;
        }
        (w, err) = OpenFile(newPath, (nint)((nint)(O_CREATE | O_EXCL) | O_WRONLY), (fs.FileMode)(438 | (fs.FileMode)(info.Mode() & 511)));
        if (err != default!) {
            return err;
        }
        {
            var (_, errΔ1) = io.Copy(~w, r); if (errΔ1 != default!) {
                w.Close();
                return new PathError{Op: "Copy"u8, Path: newPath, Err: errΔ1};
            }
        }
        return w.Close();
    });
});

} // end os_package
