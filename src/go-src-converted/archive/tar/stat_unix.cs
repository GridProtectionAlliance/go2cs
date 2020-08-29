// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux darwin dragonfly freebsd openbsd netbsd solaris

// package tar -- go2cs converted at 2020 August 29 08:45:26 UTC
// import "archive/tar" ==> using tar = go.archive.tar_package
// Original source: C:\Go\src\archive\tar\stat_unix.go
using os = go.os_package;
using user = go.os.user_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace archive
{
    public static partial class tar_package
    {
        private static void init()
        {
            sysStat = statUnix;
        }

        // userMap and groupMap caches UID and GID lookups for performance reasons.
        // The downside is that renaming uname or gname by the OS never takes effect.
        private static sync.Map userMap = default;        private static sync.Map groupMap = default; // map[int]string

 // map[int]string

        private static error statUnix(os.FileInfo fi, ref Header h)
        {
            ref syscall.Stat_t (sys, ok) = fi.Sys()._<ref syscall.Stat_t>();
            if (!ok)
            {
                return error.As(null);
            }
            h.Uid = int(sys.Uid);
            h.Gid = int(sys.Gid); 

            // Best effort at populating Uname and Gname.
            // The os/user functions may fail for any number of reasons
            // (not implemented on that platform, cgo not enabled, etc).
            {
                var u__prev1 = u;

                var (u, ok) = userMap.Load(h.Uid);

                if (ok)
                {
                    h.Uname = u._<@string>();
                }                {
                    var u__prev2 = u;

                    var (u, err) = user.LookupId(strconv.Itoa(h.Uid));


                    else if (err == null)
                    {
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

                if (ok)
                {
                    h.Gname = g._<@string>();
                }                {
                    var g__prev2 = g;

                    var (g, err) = user.LookupGroupId(strconv.Itoa(h.Gid));


                    else if (err == null)
                    {
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
            if (h.Typeflag == TypeChar || h.Typeflag == TypeBlock)
            {
                var dev = uint64(sys.Rdev); // May be int32 or uint32
                switch (runtime.GOOS)
                {
                    case "linux": 
                        // Copied from golang.org/x/sys/unix/dev_linux.go.
                        var major = uint32((dev & 0x00000000000fff00UL) >> (int)(8L));
                        major |= uint32((dev & 0xfffff00000000000UL) >> (int)(32L));
                        var minor = uint32((dev & 0x00000000000000ffUL) >> (int)(0L));
                        minor |= uint32((dev & 0x00000ffffff00000UL) >> (int)(12L));
                        h.Devmajor = int64(major);
                        h.Devminor = int64(minor);
                        break;
                    case "darwin": 
                        // Copied from golang.org/x/sys/unix/dev_darwin.go.
                        major = uint32((dev >> (int)(24L)) & 0xffUL);
                        minor = uint32(dev & 0xffffffUL);
                        h.Devmajor = int64(major);
                        h.Devminor = int64(minor);
                        break;
                    case "dragonfly": 
                        // Copied from golang.org/x/sys/unix/dev_dragonfly.go.
                        major = uint32((dev >> (int)(8L)) & 0xffUL);
                        minor = uint32(dev & 0xffff00ffUL);
                        h.Devmajor = int64(major);
                        h.Devminor = int64(minor);
                        break;
                    case "freebsd": 
                        // Copied from golang.org/x/sys/unix/dev_freebsd.go.
                        major = uint32((dev >> (int)(8L)) & 0xffUL);
                        minor = uint32(dev & 0xffff00ffUL);
                        h.Devmajor = int64(major);
                        h.Devminor = int64(minor);
                        break;
                    case "netbsd": 
                        // Copied from golang.org/x/sys/unix/dev_netbsd.go.
                        major = uint32((dev & 0x000fff00UL) >> (int)(8L));
                        minor = uint32((dev & 0x000000ffUL) >> (int)(0L));
                        minor |= uint32((dev & 0xfff00000UL) >> (int)(12L));
                        h.Devmajor = int64(major);
                        h.Devminor = int64(minor);
                        break;
                    case "openbsd": 
                        // Copied from golang.org/x/sys/unix/dev_openbsd.go.
                        major = uint32((dev & 0x0000ff00UL) >> (int)(8L));
                        minor = uint32((dev & 0x000000ffUL) >> (int)(0L));
                        minor |= uint32((dev & 0xffff0000UL) >> (int)(8L));
                        h.Devmajor = int64(major);
                        h.Devminor = int64(minor);
                        break;
                    default: 
                        break;
                }
            }
            return error.As(null);
        }
    }
}}
