// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:12:38 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\dir.go
using fs = go.io.fs_package;
using sort = go.sort_package;
using System;


namespace go;

public static partial class os_package {

private partial struct readdirMode { // : nint
}

private static readonly readdirMode readdirName = iota;
private static readonly var readdirDirEntry = 0;
private static readonly var readdirFileInfo = 1;


// Readdir reads the contents of the directory associated with file and
// returns a slice of up to n FileInfo values, as would be returned
// by Lstat, in directory order. Subsequent calls on the same file will yield
// further FileInfos.
//
// If n > 0, Readdir returns at most n FileInfo structures. In this case, if
// Readdir returns an empty slice, it will return a non-nil error
// explaining why. At the end of a directory, the error is io.EOF.
//
// If n <= 0, Readdir returns all the FileInfo from the directory in
// a single slice. In this case, if Readdir succeeds (reads all
// the way to the end of the directory), it returns the slice and a
// nil error. If it encounters an error before the end of the
// directory, Readdir returns the FileInfo read until that point
// and a non-nil error.
//
// Most clients are better served by the more efficient ReadDir method.
private static (slice<FileInfo>, error) Readdir(this ptr<File> _addr_f, nint n) {
    slice<FileInfo> _p0 = default;
    error _p0 = default!;
    ref File f = ref _addr_f.val;

    if (f == null) {
        return (null, error.As(ErrInvalid)!);
    }
    var (_, _, infos, err) = f.readdir(n, readdirFileInfo);
    if (infos == null) { 
        // Readdir has historically always returned a non-nil empty slice, never nil,
        // even on error (except misuse with nil receiver above).
        // Keep it that way to avoid breaking overly sensitive callers.
        infos = new slice<FileInfo>(new FileInfo[] {  });

    }
    return (infos, error.As(err)!);

}

// Readdirnames reads the contents of the directory associated with file
// and returns a slice of up to n names of files in the directory,
// in directory order. Subsequent calls on the same file will yield
// further names.
//
// If n > 0, Readdirnames returns at most n names. In this case, if
// Readdirnames returns an empty slice, it will return a non-nil error
// explaining why. At the end of a directory, the error is io.EOF.
//
// If n <= 0, Readdirnames returns all the names from the directory in
// a single slice. In this case, if Readdirnames succeeds (reads all
// the way to the end of the directory), it returns the slice and a
// nil error. If it encounters an error before the end of the
// directory, Readdirnames returns the names read until that point and
// a non-nil error.
private static (slice<@string>, error) Readdirnames(this ptr<File> _addr_f, nint n) {
    slice<@string> names = default;
    error err = default!;
    ref File f = ref _addr_f.val;

    if (f == null) {
        return (null, error.As(ErrInvalid)!);
    }
    names, _, _, err = f.readdir(n, readdirName);
    if (names == null) { 
        // Readdirnames has historically always returned a non-nil empty slice, never nil,
        // even on error (except misuse with nil receiver above).
        // Keep it that way to avoid breaking overly sensitive callers.
        names = new slice<@string>(new @string[] {  });

    }
    return (names, error.As(err)!);

}

// A DirEntry is an entry read from a directory
// (using the ReadDir function or a File's ReadDir method).
public partial struct DirEntry { // : fs.DirEntry
}

// ReadDir reads the contents of the directory associated with the file f
// and returns a slice of DirEntry values in directory order.
// Subsequent calls on the same file will yield later DirEntry records in the directory.
//
// If n > 0, ReadDir returns at most n DirEntry records.
// In this case, if ReadDir returns an empty slice, it will return an error explaining why.
// At the end of a directory, the error is io.EOF.
//
// If n <= 0, ReadDir returns all the DirEntry records remaining in the directory.
// When it succeeds, it returns a nil error (not io.EOF).
private static (slice<DirEntry>, error) ReadDir(this ptr<File> _addr_f, nint n) {
    slice<DirEntry> _p0 = default;
    error _p0 = default!;
    ref File f = ref _addr_f.val;

    if (f == null) {
        return (null, error.As(ErrInvalid)!);
    }
    var (_, dirents, _, err) = f.readdir(n, readdirDirEntry);
    if (dirents == null) { 
        // Match Readdir and Readdirnames: don't return nil slices.
        dirents = new slice<DirEntry>(new DirEntry[] {  });

    }
    return (dirents, error.As(err)!);

}

// testingForceReadDirLstat forces ReadDir to call Lstat, for testing that code path.
// This can be difficult to provoke on some Unix systems otherwise.
private static bool testingForceReadDirLstat = default;

// ReadDir reads the named directory,
// returning all its directory entries sorted by filename.
// If an error occurs reading the directory,
// ReadDir returns the entries it was able to read before the error,
// along with the error.
public static (slice<DirEntry>, error) ReadDir(@string name) => func((defer, _, _) => {
    slice<DirEntry> _p0 = default;
    error _p0 = default!;

    var (f, err) = Open(name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(f.Close());

    var (dirs, err) = f.ReadDir(-1);
    sort.Slice(dirs, (i, j) => dirs[i].Name() < dirs[j].Name());
    return (dirs, error.As(err)!);

});

} // end os_package
