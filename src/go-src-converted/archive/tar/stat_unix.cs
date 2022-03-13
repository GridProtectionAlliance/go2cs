// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || linux || darwin || dragonfly || freebsd || openbsd || netbsd || solaris
// +build aix linux darwin dragonfly freebsd openbsd netbsd solaris

// package tar -- go2cs converted at 2022 March 13 05:42:27 UTC
// import "archive/tar" ==> using tar = go.archive.tar_package
// Original source: C:\Program Files\Go\src\archive\tar\stat_unix.go
namespace go.archive;

using fs = io.fs_package;
using user = os.user_package;
using runtime = runtime_package;
using strconv = strconv_package;
using sync = sync_package;
using syscall = syscall_package;

public static partial class tar_package {

private static void init() {
    sysStat = statUnix;
}

// userMap and groupMap caches UID and GID lookups for performance reasons.
// The downside is that renaming uname or gname by the OS never takes effect.
private static sync.Map userMap = default;private static sync.Map groupMap = default; // map[int]string

 // map[int]string

private static error statUnix(fs.FileInfo fi, ptr<Header> _addr_h) {
    ref Header h = ref _addr_h.val;

    ptr<syscall.Stat_t> (sys, ok) = fi.Sys()._<ptr<syscall.Stat_t>>();
    if (!ok) {
        return error.As(null!)!;
    }
    h.Uid = int(sys.Uid);
    h.Gid = int(sys.Gid); 

    // Best effort at populating Uname and Gname.
    // The os/user functions may fail for any number of reasons
    // (not implemented on that platform, cgo not enabled, etc).
    {
        var u__prev1 = u;

        var (u, ok) = userMap.Load(h.Uid);

        if (ok) {
            h.Uname = u._<@string>();
        }        {
            var u__prev2 = u;

            var (u, err) = user.LookupId(strconv.Itoa(h.Uid));


            else if (err == null) {
                h.Uname = u.Username;
                userMap.Store(h.Uid, h.Uname);
            }

            u = u__prev2;

        }

        u = u__prev1;

    }
    {
        var g__prev1 = g;

        var (g, ok) = groupMap.Load(h.Gid);

        if (ok) {
            h.Gname = g._<@string>();
        }        {
            var g__prev2 = g;

            var (g, err) = user.LookupGroupId(strconv.Itoa(h.Gid));


            else if (err == null) {
                h.Gname = g.Name;
                groupMap.Store(h.Gid, h.Gname);
            }

            g = g__prev2;

        }


        g = g__prev1;

    }

    h.AccessTime = statAtime(sys);
    h.ChangeTime = statCtime(sys); 

    // Best effort at populating Devmajor and Devminor.
    if (h.Typeflag == TypeChar || h.Typeflag == TypeBlock) {
        var dev = uint64(sys.Rdev); // May be int32 or uint32
        switch (runtime.GOOS) {
            case "aix": 
                uint major = default;            uint minor = default;

                major = uint32((dev & 0x3fffffff00000000) >> 32);
                minor = uint32((dev & 0x00000000ffffffff) >> 0);
                (h.Devmajor, h.Devminor) = (int64(major), int64(minor));
                break;
            case "linux": 
                // Copied from golang.org/x/sys/unix/dev_linux.go.
                major = uint32((dev & 0x00000000000fff00) >> 8);
                major |= uint32((dev & 0xfffff00000000000) >> 32);
                minor = uint32((dev & 0x00000000000000ff) >> 0);
                minor |= uint32((dev & 0x00000ffffff00000) >> 12);
                (h.Devmajor, h.Devminor) = (int64(major), int64(minor));
                break;
            case "darwin": 
                // Copied from golang.org/x/sys/unix/dev_darwin.go.

            case "ios": 
                // Copied from golang.org/x/sys/unix/dev_darwin.go.
                major = uint32((dev >> 24) & 0xff);
                minor = uint32(dev & 0xffffff);
                (h.Devmajor, h.Devminor) = (int64(major), int64(minor));
                break;
            case "dragonfly": 
                // Copied from golang.org/x/sys/unix/dev_dragonfly.go.
                major = uint32((dev >> 8) & 0xff);
                minor = uint32(dev & 0xffff00ff);
                (h.Devmajor, h.Devminor) = (int64(major), int64(minor));
                break;
            case "freebsd": 
                // Copied from golang.org/x/sys/unix/dev_freebsd.go.
                major = uint32((dev >> 8) & 0xff);
                minor = uint32(dev & 0xffff00ff);
                (h.Devmajor, h.Devminor) = (int64(major), int64(minor));
                break;
            case "netbsd": 
                // Copied from golang.org/x/sys/unix/dev_netbsd.go.
                major = uint32((dev & 0x000fff00) >> 8);
                minor = uint32((dev & 0x000000ff) >> 0);
                minor |= uint32((dev & 0xfff00000) >> 12);
                (h.Devmajor, h.Devminor) = (int64(major), int64(minor));
                break;
            case "openbsd": 
                // Copied from golang.org/x/sys/unix/dev_openbsd.go.
                major = uint32((dev & 0x0000ff00) >> 8);
                minor = uint32((dev & 0x000000ff) >> 0);
                minor |= uint32((dev & 0xffff0000) >> 8);
                (h.Devmajor, h.Devminor) = (int64(major), int64(minor));
                break;
            default: 

                break;
        }
    }
    return error.As(null!)!;
}

} // end tar_package
