// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 08 03:45:22 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\types_plan9.go
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
        }

        private static long Size(this ptr<fileStat> _addr_fs)
        {
            ref fileStat fs = ref _addr_fs.val;

            return fs.size;
        }
        private static FileMode Mode(this ptr<fileStat> _addr_fs)
        {
            ref fileStat fs = ref _addr_fs.val;

            return fs.mode;
        }
        private static time.Time ModTime(this ptr<fileStat> _addr_fs)
        {
            ref fileStat fs = ref _addr_fs.val;

            return fs.modTime;
        }
        private static void Sys(this ptr<fileStat> _addr_fs)
        {
            ref fileStat fs = ref _addr_fs.val;

            return fs.sys;
        }

        private static bool sameFile(ptr<fileStat> _addr_fs1, ptr<fileStat> _addr_fs2)
        {
            ref fileStat fs1 = ref _addr_fs1.val;
            ref fileStat fs2 = ref _addr_fs2.val;

            ptr<syscall.Dir> a = fs1.sys._<ptr<syscall.Dir>>();
            ptr<syscall.Dir> b = fs2.sys._<ptr<syscall.Dir>>();
            return a.Qid.Path == b.Qid.Path && a.Type == b.Type && a.Dev == b.Dev;
        }

        private static readonly long badFd = (long)-1L;

    }
}
