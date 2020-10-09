// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package unix -- go2cs converted at 2020 October 09 05:56:13 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\dirent.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class unix_package
    {
        // readInt returns the size-bytes unsigned integer in native byte order at offset off.
        private static (ulong, bool) readInt(slice<byte> b, System.UIntPtr off, System.UIntPtr size)
        {
            ulong u = default;
            bool ok = default;

            if (len(b) < int(off + size))
            {
                return (0L, false);
            }
            if (isBigEndian)
            {
                return (readIntBE(b[off..], size), true);
            }
            return (readIntLE(b[off..], size), true);

        }

        private static ulong readIntBE(slice<byte> b, System.UIntPtr size) => func((_, panic, __) =>
        {
            switch (size)
            {
                case 1L: 
                    return uint64(b[0L]);
                    break;
                case 2L: 
                    _ = b[1L]; // bounds check hint to compiler; see golang.org/issue/14808
                    return uint64(b[1L]) | uint64(b[0L]) << (int)(8L);
                    break;
                case 4L: 
                    _ = b[3L]; // bounds check hint to compiler; see golang.org/issue/14808
                    return uint64(b[3L]) | uint64(b[2L]) << (int)(8L) | uint64(b[1L]) << (int)(16L) | uint64(b[0L]) << (int)(24L);
                    break;
                case 8L: 
                    _ = b[7L]; // bounds check hint to compiler; see golang.org/issue/14808
                    return uint64(b[7L]) | uint64(b[6L]) << (int)(8L) | uint64(b[5L]) << (int)(16L) | uint64(b[4L]) << (int)(24L) | uint64(b[3L]) << (int)(32L) | uint64(b[2L]) << (int)(40L) | uint64(b[1L]) << (int)(48L) | uint64(b[0L]) << (int)(56L);
                    break;
                default: 
                    panic("syscall: readInt with unsupported size");
                    break;
            }

        });

        private static ulong readIntLE(slice<byte> b, System.UIntPtr size) => func((_, panic, __) =>
        {
            switch (size)
            {
                case 1L: 
                    return uint64(b[0L]);
                    break;
                case 2L: 
                    _ = b[1L]; // bounds check hint to compiler; see golang.org/issue/14808
                    return uint64(b[0L]) | uint64(b[1L]) << (int)(8L);
                    break;
                case 4L: 
                    _ = b[3L]; // bounds check hint to compiler; see golang.org/issue/14808
                    return uint64(b[0L]) | uint64(b[1L]) << (int)(8L) | uint64(b[2L]) << (int)(16L) | uint64(b[3L]) << (int)(24L);
                    break;
                case 8L: 
                    _ = b[7L]; // bounds check hint to compiler; see golang.org/issue/14808
                    return uint64(b[0L]) | uint64(b[1L]) << (int)(8L) | uint64(b[2L]) << (int)(16L) | uint64(b[3L]) << (int)(24L) | uint64(b[4L]) << (int)(32L) | uint64(b[5L]) << (int)(40L) | uint64(b[6L]) << (int)(48L) | uint64(b[7L]) << (int)(56L);
                    break;
                default: 
                    panic("syscall: readInt with unsupported size");
                    break;
            }

        });

        // ParseDirent parses up to max directory entries in buf,
        // appending the names to names. It returns the number of
        // bytes consumed from buf, the number of entries added
        // to names, and the new names slice.
        public static (long, long, slice<@string>) ParseDirent(slice<byte> buf, long max, slice<@string> names)
        {
            long consumed = default;
            long count = default;
            slice<@string> newnames = default;

            var origlen = len(buf);
            count = 0L;
            while (max != 0L && len(buf) > 0L)
            {
                var (reclen, ok) = direntReclen(buf);
                if (!ok || reclen > uint64(len(buf)))
                {
                    return (origlen, count, names);
                }

                var rec = buf[..reclen];
                buf = buf[reclen..];
                var (ino, ok) = direntIno(rec);
                if (!ok)
                {
                    break;
                }

                if (ino == 0L)
                { // File absent in directory.
                    continue;

                }

                const var namoff = uint64(@unsafe.Offsetof(new Dirent().Name));

                var (namlen, ok) = direntNamlen(rec);
                if (!ok || namoff + namlen > uint64(len(rec)))
                {
                    break;
                }

                var name = rec[namoff..namoff + namlen];
                foreach (var (i, c) in name)
                {
                    if (c == 0L)
                    {
                        name = name[..i];
                        break;
                    }

                } 
                // Check for useless names before allocating a string.
                if (string(name) == "." || string(name) == "..")
                {
                    continue;
                }

                max--;
                count++;
                names = append(names, string(name));

            }

            return (origlen - len(buf), count, names);

        }
    }
}}}}}}
