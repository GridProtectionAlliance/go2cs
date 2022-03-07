// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:13:55 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\types.go
using fs = go.io.fs_package;
using syscall = go.syscall_package;

namespace go;

public static partial class os_package {

    // Getpagesize returns the underlying system's memory page size.
public static nint Getpagesize() {
    return syscall.Getpagesize();
}

// File represents an open file descriptor.
public partial struct File {
    public ref ptr<file> ptr<file> => ref ptr<file>_ptr; // os specific
}

// A FileInfo describes a file and is returned by Stat and Lstat.
public partial struct FileInfo { // : fs.FileInfo
}

// A FileMode represents a file's mode and permission bits.
// The bits have the same definition on all systems, so that
// information about files can be moved from one system
// to another portably. Not all bits apply to all systems.
// The only required bit is ModeDir for directories.
public partial struct FileMode { // : fs.FileMode
}

// The defined file mode bits are the most significant bits of the FileMode.
// The nine least-significant bits are the standard Unix rwxrwxrwx permissions.
// The values of these bits should be considered part of the public API and
// may be used in wire protocols or disk representations: they must not be
// changed, although new bits might be added.
 
// The single letters are the abbreviations
// used by the String method's formatting.
public static readonly var ModeDir = fs.ModeDir; // d: is a directory
public static readonly var ModeAppend = fs.ModeAppend; // a: append-only
public static readonly var ModeExclusive = fs.ModeExclusive; // l: exclusive use
public static readonly var ModeTemporary = fs.ModeTemporary; // T: temporary file; Plan 9 only
public static readonly var ModeSymlink = fs.ModeSymlink; // L: symbolic link
public static readonly var ModeDevice = fs.ModeDevice; // D: device file
public static readonly var ModeNamedPipe = fs.ModeNamedPipe; // p: named pipe (FIFO)
public static readonly var ModeSocket = fs.ModeSocket; // S: Unix domain socket
public static readonly var ModeSetuid = fs.ModeSetuid; // u: setuid
public static readonly var ModeSetgid = fs.ModeSetgid; // g: setgid
public static readonly var ModeCharDevice = fs.ModeCharDevice; // c: Unix character device, when ModeDevice is set
public static readonly var ModeSticky = fs.ModeSticky; // t: sticky
public static readonly var ModeIrregular = fs.ModeIrregular; // ?: non-regular file; nothing else is known about this file

// Mask for the type bits. For regular files, none will be set.
public static readonly var ModeType = fs.ModeType;

public static readonly var ModePerm = fs.ModePerm; // Unix permission bits, 0o777

private static @string Name(this ptr<fileStat> _addr_fs) {
    ref fileStat fs = ref _addr_fs.val;

    return fs.name;
}
private static bool IsDir(this ptr<fileStat> _addr_fs) {
    ref fileStat fs = ref _addr_fs.val;

    return fs.Mode().IsDir();
}

// SameFile reports whether fi1 and fi2 describe the same file.
// For example, on Unix this means that the device and inode fields
// of the two underlying structures are identical; on other systems
// the decision may be based on the path names.
// SameFile only applies to results returned by this package's Stat.
// It returns false in other cases.
public static bool SameFile(FileInfo fi1, FileInfo fi2) {
    ptr<fileStat> (fs1, ok1) = fi1._<ptr<fileStat>>();
    ptr<fileStat> (fs2, ok2) = fi2._<ptr<fileStat>>();
    if (!ok1 || !ok2) {
        return false;
    }
    return sameFile(fs1, fs2);

}

} // end os_package
