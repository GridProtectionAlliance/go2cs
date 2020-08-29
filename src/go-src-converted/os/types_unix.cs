// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !windows
// +build !plan9

// package os -- go2cs converted at 2020 August 29 08:44:38 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\types_unix.go
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // A fileStat is the implementation of FileInfo returned by Stat and Lstat.
        private partial struct fileStat
        {
            public @string name;
            public long size;
            public FileMode mode;
            public time.Time modTime;
            public syscall.Stat_t sys;
        }

        private static long Size(this ref fileStat fs)
        {
            return fs.size;
        }
        private static FileMode Mode(this ref fileStat fs)
        {
            return fs.mode;
        }
        private static time.Time ModTime(this ref fileStat fs)
        {
            return fs.modTime;
        }
        private static void Sys(this ref fileStat fs)
        {
            return ref fs.sys;
        }

        private static bool sameFile(ref fileStat fs1, ref fileStat fs2)
        {
            return fs1.sys.Dev == fs2.sys.Dev && fs1.sys.Ino == fs2.sys.Ino;
        }
    }
}
