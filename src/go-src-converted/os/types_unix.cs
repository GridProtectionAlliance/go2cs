// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !windows && !plan9
// +build !windows,!plan9

// package os -- go2cs converted at 2022 March 13 05:28:06 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\types_unix.go
namespace go;

using syscall = syscall_package;
using time = time_package;


// A fileStat is the implementation of FileInfo returned by Stat and Lstat.

public static partial class os_package {

private partial struct fileStat {
    public @string name;
    public long size;
    public FileMode mode;
    public time.Time modTime;
    public syscall.Stat_t sys;
}

private static long Size(this ptr<fileStat> _addr_fs) {
    ref fileStat fs = ref _addr_fs.val;

    return fs.size;
}
private static FileMode Mode(this ptr<fileStat> _addr_fs) {
    ref fileStat fs = ref _addr_fs.val;

    return fs.mode;
}
private static time.Time ModTime(this ptr<fileStat> _addr_fs) {
    ref fileStat fs = ref _addr_fs.val;

    return fs.modTime;
}
private static void Sys(this ptr<fileStat> _addr_fs) {
    ref fileStat fs = ref _addr_fs.val;

    return _addr_fs.sys;
}

private static bool sameFile(ptr<fileStat> _addr_fs1, ptr<fileStat> _addr_fs2) {
    ref fileStat fs1 = ref _addr_fs1.val;
    ref fileStat fs2 = ref _addr_fs2.val;

    return fs1.sys.Dev == fs2.sys.Dev && fs1.sys.Ino == fs2.sys.Ino;
}

} // end os_package
