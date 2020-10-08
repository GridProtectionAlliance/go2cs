// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build freebsd netbsd

// package unix -- go2cs converted at 2020 October 08 04:48:06 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\xattr_bsd.go
using strings = go.strings_package;
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
        // Derive extattr namespace and attribute name
        private static (long, @string, error) xattrnamespace(@string fullattr)
        {
            long ns = default;
            @string attr = default;
            error err = default!;

            var s = strings.IndexByte(fullattr, '.');
            if (s == -1L)
            {
                return (-1L, "", error.As(ENOATTR)!);
            }
            var @namespace = fullattr[0L..s];
            attr = fullattr[s + 1L..];

            switch (namespace)
            {
                case "user": 
                    return (EXTATTR_NAMESPACE_USER, attr, error.As(null!)!);
                    break;
                case "system": 
                    return (EXTATTR_NAMESPACE_SYSTEM, attr, error.As(null!)!);
                    break;
                default: 
                    return (-1L, "", error.As(ENOATTR)!);
                    break;
            }

        }

        private static unsafe.Pointer initxattrdest(slice<byte> dest, long idx)
        {
            unsafe.Pointer d = default;

            if (len(dest) > idx)
            {
                return @unsafe.Pointer(_addr_dest[idx]);
            }
            else
            {
                return @unsafe.Pointer(_zero);
            }

        }

        // FreeBSD and NetBSD implement their own syscalls to handle extended attributes

        public static (long, error) Getxattr(@string file, @string attr, slice<byte> dest)
        {
            long sz = default;
            error err = default!;

            var d = initxattrdest(dest, 0L);
            var destsize = len(dest);

            var (nsid, a, err) = xattrnamespace(attr);
            if (err != null)
            {
                return (-1L, error.As(err)!);
            }

            return ExtattrGetFile(file, nsid, a, uintptr(d), destsize);

        }

        public static (long, error) Fgetxattr(long fd, @string attr, slice<byte> dest)
        {
            long sz = default;
            error err = default!;

            var d = initxattrdest(dest, 0L);
            var destsize = len(dest);

            var (nsid, a, err) = xattrnamespace(attr);
            if (err != null)
            {
                return (-1L, error.As(err)!);
            }

            return ExtattrGetFd(fd, nsid, a, uintptr(d), destsize);

        }

        public static (long, error) Lgetxattr(@string link, @string attr, slice<byte> dest)
        {
            long sz = default;
            error err = default!;

            var d = initxattrdest(dest, 0L);
            var destsize = len(dest);

            var (nsid, a, err) = xattrnamespace(attr);
            if (err != null)
            {
                return (-1L, error.As(err)!);
            }

            return ExtattrGetLink(link, nsid, a, uintptr(d), destsize);

        }

        // flags are unused on FreeBSD

        public static error Fsetxattr(long fd, @string attr, slice<byte> data, long flags)
        {
            error err = default!;

            unsafe.Pointer d = default;
            if (len(data) > 0L)
            {
                d = @unsafe.Pointer(_addr_data[0L]);
            }

            var datasiz = len(data);

            var (nsid, a, err) = xattrnamespace(attr);
            if (err != null)
            {
                return ;
            }

            _, err = ExtattrSetFd(fd, nsid, a, uintptr(d), datasiz);
            return ;

        }

        public static error Setxattr(@string file, @string attr, slice<byte> data, long flags)
        {
            error err = default!;

            unsafe.Pointer d = default;
            if (len(data) > 0L)
            {
                d = @unsafe.Pointer(_addr_data[0L]);
            }

            var datasiz = len(data);

            var (nsid, a, err) = xattrnamespace(attr);
            if (err != null)
            {
                return ;
            }

            _, err = ExtattrSetFile(file, nsid, a, uintptr(d), datasiz);
            return ;

        }

        public static error Lsetxattr(@string link, @string attr, slice<byte> data, long flags)
        {
            error err = default!;

            unsafe.Pointer d = default;
            if (len(data) > 0L)
            {
                d = @unsafe.Pointer(_addr_data[0L]);
            }

            var datasiz = len(data);

            var (nsid, a, err) = xattrnamespace(attr);
            if (err != null)
            {
                return ;
            }

            _, err = ExtattrSetLink(link, nsid, a, uintptr(d), datasiz);
            return ;

        }

        public static error Removexattr(@string file, @string attr)
        {
            error err = default!;

            var (nsid, a, err) = xattrnamespace(attr);
            if (err != null)
            {
                return ;
            }

            err = ExtattrDeleteFile(file, nsid, a);
            return ;

        }

        public static error Fremovexattr(long fd, @string attr)
        {
            error err = default!;

            var (nsid, a, err) = xattrnamespace(attr);
            if (err != null)
            {
                return ;
            }

            err = ExtattrDeleteFd(fd, nsid, a);
            return ;

        }

        public static error Lremovexattr(@string link, @string attr)
        {
            error err = default!;

            var (nsid, a, err) = xattrnamespace(attr);
            if (err != null)
            {
                return ;
            }

            err = ExtattrDeleteLink(link, nsid, a);
            return ;

        }

        public static (long, error) Listxattr(@string file, slice<byte> dest)
        {
            long sz = default;
            error err = default!;

            var d = initxattrdest(dest, 0L);
            var destsiz = len(dest); 

            // FreeBSD won't allow you to list xattrs from multiple namespaces
            long s = 0L;
            foreach (var (_, nsid) in new array<long>(new long[] { EXTATTR_NAMESPACE_USER, EXTATTR_NAMESPACE_SYSTEM }))
            {
                var (stmp, e) = ExtattrListFile(file, nsid, uintptr(d), destsiz);

                /* Errors accessing system attrs are ignored so that
                         * we can implement the Linux-like behavior of omitting errors that
                         * we don't have read permissions on
                         *
                         * Linux will still error if we ask for user attributes on a file that
                         * we don't have read permissions on, so don't ignore those errors
                         */
                if (e != null && e == EPERM && nsid != EXTATTR_NAMESPACE_USER)
                {
                    continue;
                }
                else if (e != null)
                {
                    return (s, error.As(e)!);
                }

                s += stmp;
                destsiz -= s;
                if (destsiz < 0L)
                {
                    destsiz = 0L;
                }

                d = initxattrdest(dest, s);

            }
            return (s, error.As(null!)!);

        }

        public static (long, error) Flistxattr(long fd, slice<byte> dest)
        {
            long sz = default;
            error err = default!;

            var d = initxattrdest(dest, 0L);
            var destsiz = len(dest);

            long s = 0L;
            foreach (var (_, nsid) in new array<long>(new long[] { EXTATTR_NAMESPACE_USER, EXTATTR_NAMESPACE_SYSTEM }))
            {
                var (stmp, e) = ExtattrListFd(fd, nsid, uintptr(d), destsiz);
                if (e != null && e == EPERM && nsid != EXTATTR_NAMESPACE_USER)
                {
                    continue;
                }
                else if (e != null)
                {
                    return (s, error.As(e)!);
                }

                s += stmp;
                destsiz -= s;
                if (destsiz < 0L)
                {
                    destsiz = 0L;
                }

                d = initxattrdest(dest, s);

            }
            return (s, error.As(null!)!);

        }

        public static (long, error) Llistxattr(@string link, slice<byte> dest)
        {
            long sz = default;
            error err = default!;

            var d = initxattrdest(dest, 0L);
            var destsiz = len(dest);

            long s = 0L;
            foreach (var (_, nsid) in new array<long>(new long[] { EXTATTR_NAMESPACE_USER, EXTATTR_NAMESPACE_SYSTEM }))
            {
                var (stmp, e) = ExtattrListLink(link, nsid, uintptr(d), destsiz);
                if (e != null && e == EPERM && nsid != EXTATTR_NAMESPACE_USER)
                {
                    continue;
                }
                else if (e != null)
                {
                    return (s, error.As(e)!);
                }

                s += stmp;
                destsiz -= s;
                if (destsiz < 0L)
                {
                    destsiz = 0L;
                }

                d = initxattrdest(dest, s);

            }
            return (s, error.As(null!)!);

        }
    }
}}}}}}
