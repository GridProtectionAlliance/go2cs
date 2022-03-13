// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fs defines basic interfaces to a file system.
// A file system can be provided by the host operating system
// but also by other packages.

// package fs -- go2cs converted at 2022 March 13 05:23:49 UTC
// import "io/fs" ==> using fs = go.io.fs_package
// Original source: C:\Program Files\Go\src\io\fs\fs.go
namespace go.io;

using oserror = @internal.oserror_package;
using time = time_package;
using utf8 = unicode.utf8_package;


// An FS provides access to a hierarchical file system.
//
// The FS interface is the minimum implementation required of the file system.
// A file system may implement additional interfaces,
// such as ReadFileFS, to provide additional or optimized functionality.

public static partial class fs_package {

public partial interface FS {
    (File, error) Open(@string name);
}

// ValidPath reports whether the given path name
// is valid for use in a call to Open.
//
// Path names passed to open are UTF-8-encoded,
// unrooted, slash-separated sequences of path elements, like “x/y/z”.
// Path names must not contain an element that is “.” or “..” or the empty string,
// except for the special case that the root directory is named “.”.
// Paths must not start or end with a slash: “/x” and “x/” are invalid.
//
// Note that paths are slash-separated on all systems, even Windows.
// Paths containing other characters such as backslash and colon
// are accepted as valid, but those characters must never be
// interpreted by an FS implementation as path element separators.
public static bool ValidPath(@string name) {
    if (!utf8.ValidString(name)) {
        return false;
    }
    if (name == ".") { 
        // special case
        return true;
    }
    while (true) {
        nint i = 0;
        while (i < len(name) && name[i] != '/') {
            i++;
        }
        var elem = name[..(int)i];
        if (elem == "" || elem == "." || elem == "..") {
            return false;
        }
        if (i == len(name)) {
            return true; // reached clean ending
        }
        name = name[(int)i + 1..];
    }
}

// A File provides access to a single file.
// The File interface is the minimum implementation required of the file.
// Directory files should also implement ReadDirFile.
// A file may implement io.ReaderAt or io.Seeker as optimizations.
public partial interface File {
    error Stat();
    error Read(slice<byte> _p0);
    error Close();
}

// A DirEntry is an entry read from a directory
// (using the ReadDir function or a ReadDirFile's ReadDir method).
public partial interface DirEntry {
    (FileInfo, error) Name(); // IsDir reports whether the entry describes a directory.
    (FileInfo, error) IsDir(); // Type returns the type bits for the entry.
// The type bits are a subset of the usual FileMode bits, those returned by the FileMode.Type method.
    (FileInfo, error) Type(); // Info returns the FileInfo for the file or subdirectory described by the entry.
// The returned FileInfo may be from the time of the original directory read
// or from the time of the call to Info. If the file has been removed or renamed
// since the directory read, Info may return an error satisfying errors.Is(err, ErrNotExist).
// If the entry denotes a symbolic link, Info reports the information about the link itself,
// not the link's target.
    (FileInfo, error) Info();
}

// A ReadDirFile is a directory file whose entries can be read with the ReadDir method.
// Every directory file should implement this interface.
// (It is permissible for any file to implement this interface,
// but if so ReadDir should return an error for non-directories.)
public partial interface ReadDirFile {
    (slice<DirEntry>, error) ReadDir(nint n);
}

// Generic file system errors.
// Errors returned by file systems can be tested against these errors
// using errors.Is.
public static var ErrInvalid = errInvalid();public static var ErrPermission = errPermission();public static var ErrExist = errExist();public static var ErrNotExist = errNotExist();public static var ErrClosed = errClosed();

private static error errInvalid() {
    return error.As(oserror.ErrInvalid)!;
}
private static error errPermission() {
    return error.As(oserror.ErrPermission)!;
}
private static error errExist() {
    return error.As(oserror.ErrExist)!;
}
private static error errNotExist() {
    return error.As(oserror.ErrNotExist)!;
}
private static error errClosed() {
    return error.As(oserror.ErrClosed)!;
}

// A FileInfo describes a file and is returned by Stat.
public partial interface FileInfo {
    void Name(); // base name of the file
    void Size(); // length in bytes for regular files; system-dependent for others
    void Mode(); // file mode bits
    void ModTime(); // modification time
    void IsDir(); // abbreviation for Mode().IsDir()
    void Sys(); // underlying data source (can return nil)
}

// A FileMode represents a file's mode and permission bits.
// The bits have the same definition on all systems, so that
// information about files can be moved from one system
// to another portably. Not all bits apply to all systems.
// The only required bit is ModeDir for directories.
public partial struct FileMode { // : uint
}

// The defined file mode bits are the most significant bits of the FileMode.
// The nine least-significant bits are the standard Unix rwxrwxrwx permissions.
// The values of these bits should be considered part of the public API and
// may be used in wire protocols or disk representations: they must not be
// changed, although new bits might be added.
 
// The single letters are the abbreviations
// used by the String method's formatting.
public static readonly FileMode ModeDir = 1 << (int)((32 - 1 - iota)); // d: is a directory
public static readonly var ModeAppend = 0; // a: append-only
public static readonly var ModeExclusive = 1; // l: exclusive use
public static readonly var ModeTemporary = 2; // T: temporary file; Plan 9 only
public static readonly var ModeSymlink = 3; // L: symbolic link
public static readonly var ModeDevice = 4; // D: device file
public static readonly var ModeNamedPipe = 5; // p: named pipe (FIFO)
public static readonly var ModeSocket = 6; // S: Unix domain socket
public static readonly var ModeSetuid = 7; // u: setuid
public static readonly var ModeSetgid = 8; // g: setgid
public static readonly var ModeCharDevice = 9; // c: Unix character device, when ModeDevice is set
public static readonly var ModeSticky = 10; // t: sticky
public static readonly ModeType ModeIrregular = ModeDir | ModeSymlink | ModeNamedPipe | ModeSocket | ModeDevice | ModeCharDevice | ModeIrregular;

public static readonly FileMode ModePerm = 0777; // Unix permission bits

public static @string String(this FileMode m) {
    const @string str = "dalTLDpSugct?";

    array<byte> buf = new array<byte>(32); // Mode is uint32.
    nint w = 0;
    {
        var i__prev1 = i;
        var c__prev1 = c;

        foreach (var (__i, __c) in str) {
            i = __i;
            c = __c;
            if (m & (1 << (int)(uint(32 - 1 - i))) != 0) {
                buf[w] = byte(c);
                w++;
            }
        }
        i = i__prev1;
        c = c__prev1;
    }

    if (w == 0) {
        buf[w] = '-';
        w++;
    }
    const @string rwx = "rwxrwxrwx";

    {
        var i__prev1 = i;
        var c__prev1 = c;

        foreach (var (__i, __c) in rwx) {
            i = __i;
            c = __c;
            if (m & (1 << (int)(uint(9 - 1 - i))) != 0) {
                buf[w] = byte(c);
            }
            else
 {
                buf[w] = '-';
            }
            w++;
        }
        i = i__prev1;
        c = c__prev1;
    }

    return string(buf[..(int)w]);
}

// IsDir reports whether m describes a directory.
// That is, it tests for the ModeDir bit being set in m.
public static bool IsDir(this FileMode m) {
    return m & ModeDir != 0;
}

// IsRegular reports whether m describes a regular file.
// That is, it tests that no mode type bits are set.
public static bool IsRegular(this FileMode m) {
    return m & ModeType == 0;
}

// Perm returns the Unix permission bits in m (m & ModePerm).
public static FileMode Perm(this FileMode m) {
    return m & ModePerm;
}

// Type returns type bits in m (m & ModeType).
public static FileMode Type(this FileMode m) {
    return m & ModeType;
}

// PathError records an error and the operation and file path that caused it.
public partial struct PathError {
    public @string Op;
    public @string Path;
    public error Err;
}

private static @string Error(this ptr<PathError> _addr_e) {
    ref PathError e = ref _addr_e.val;

    return e.Op + " " + e.Path + ": " + e.Err.Error();
}

private static error Unwrap(this ptr<PathError> _addr_e) {
    ref PathError e = ref _addr_e.val;

    return error.As(e.Err)!;
}

// Timeout reports whether this error represents a timeout.
private static bool Timeout(this ptr<PathError> _addr_e) {
    ref PathError e = ref _addr_e.val;

    return ok && t.Timeout();
}

} // end fs_package
