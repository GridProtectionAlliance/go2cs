// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using FileInfo = go.io.fs_package.FileInfo;
global using FileMode = go.io.fs_package.FileMode;

namespace go;

using fs = io.fs_package;
using syscall = syscall_package;
using io;

partial class os_package {

// Getpagesize returns the underlying system's memory page size.
public static nint Getpagesize() {
    return syscall.Getpagesize();
}

// File represents an open file descriptor.
//
// The methods of File are safe for concurrent use.
[GoType] partial struct File {
    public partial ref ж<file> file { get; } // os specific
}

// The defined file mode bits are the most significant bits of the [FileMode].
// The nine least-significant bits are the standard Unix rwxrwxrwx permissions.
// The values of these bits should be considered part of the public API and
// may be used in wire protocols or disk representations: they must not be
// changed, although new bits might be added.
public static readonly fs.FileMode ModeDir = /* fs.ModeDir */ 2147483648; // d: is a directory

public static readonly fs.FileMode ModeAppend = /* fs.ModeAppend */ 1073741824; // a: append-only

public static readonly fs.FileMode ModeExclusive = /* fs.ModeExclusive */ 536870912; // l: exclusive use

public static readonly fs.FileMode ModeTemporary = /* fs.ModeTemporary */ 268435456; // T: temporary file; Plan 9 only

public static readonly fs.FileMode ModeSymlink = /* fs.ModeSymlink */ 134217728; // L: symbolic link

public static readonly fs.FileMode ModeDevice = /* fs.ModeDevice */ 67108864; // D: device file

public static readonly fs.FileMode ModeNamedPipe = /* fs.ModeNamedPipe */ 33554432; // p: named pipe (FIFO)

public static readonly fs.FileMode ModeSocket = /* fs.ModeSocket */ 16777216; // S: Unix domain socket

public static readonly fs.FileMode ModeSetuid = /* fs.ModeSetuid */ 8388608; // u: setuid

public static readonly fs.FileMode ModeSetgid = /* fs.ModeSetgid */ 4194304; // g: setgid

public static readonly fs.FileMode ModeCharDevice = /* fs.ModeCharDevice */ 2097152; // c: Unix character device, when ModeDevice is set

public static readonly fs.FileMode ModeSticky = /* fs.ModeSticky */ 1048576; // t: sticky

public static readonly fs.FileMode ModeIrregular = /* fs.ModeIrregular */ 524288; // ?: non-regular file; nothing else is known about this file

public static readonly fs.FileMode ModeType = /* fs.ModeType */ 2401763328;

public static readonly fs.FileMode ModePerm = /* fs.ModePerm */ 511; // Unix permission bits, 0o777

[GoRecv] internal static @string Name(this ref fileStat fs) {
    return fs.name;
}

[GoRecv] internal static bool IsDir(this ref fileStat fs) {
    return fs.Mode().IsDir();
}

// SameFile reports whether fi1 and fi2 describe the same file.
// For example, on Unix this means that the device and inode fields
// of the two underlying structures are identical; on other systems
// the decision may be based on the path names.
// SameFile only applies to results returned by this package's [Stat].
// It returns false in other cases.
public static bool SameFile(FileInfo fi1, FileInfo fi2) {
    var (fs1, ok1) = fi1._<fileStat.val>(ᐧ);
    var (fs2, ok2) = fi2._<fileStat.val>(ᐧ);
    if (!ok1 || !ok2) {
        return false;
    }
    return sameFile(fs1, fs2);
}

} // end os_package
