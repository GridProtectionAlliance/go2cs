// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build js,wasm

// package os -- go2cs converted at 2020 October 09 05:07:23 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\stat_js.go
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static void fillFileStatFromSys(ptr<fileStat> _addr_fs, @string name)
        {
            ref fileStat fs = ref _addr_fs.val;

            fs.name = basename(name);
            fs.size = fs.sys.Size;
            fs.modTime = timespecToTime(fs.sys.Mtime, fs.sys.MtimeNsec);
            fs.mode = FileMode(fs.sys.Mode & 0777L);

            if (fs.sys.Mode & syscall.S_IFMT == syscall.S_IFBLK) 
                fs.mode |= ModeDevice;
            else if (fs.sys.Mode & syscall.S_IFMT == syscall.S_IFCHR) 
                fs.mode |= ModeDevice | ModeCharDevice;
            else if (fs.sys.Mode & syscall.S_IFMT == syscall.S_IFDIR) 
                fs.mode |= ModeDir;
            else if (fs.sys.Mode & syscall.S_IFMT == syscall.S_IFIFO) 
                fs.mode |= ModeNamedPipe;
            else if (fs.sys.Mode & syscall.S_IFMT == syscall.S_IFLNK) 
                fs.mode |= ModeSymlink;
            else if (fs.sys.Mode & syscall.S_IFMT == syscall.S_IFREG)             else if (fs.sys.Mode & syscall.S_IFMT == syscall.S_IFSOCK) 
                fs.mode |= ModeSocket;
                        if (fs.sys.Mode & syscall.S_ISGID != 0L)
            {
                fs.mode |= ModeSetgid;
            }
            if (fs.sys.Mode & syscall.S_ISUID != 0L)
            {
                fs.mode |= ModeSetuid;
            }
            if (fs.sys.Mode & syscall.S_ISVTX != 0L)
            {
                fs.mode |= ModeSticky;
            }
        }

        private static time.Time timespecToTime(long sec, long nsec)
        {
            return time.Unix(sec, nsec);
        }

        // For testing.
        private static time.Time atime(FileInfo fi)
        {
            ptr<syscall.Stat_t> st = fi.Sys()._<ptr<syscall.Stat_t>>();
            return timespecToTime(st.Atime, st.AtimeNsec);
        }
    }
}
