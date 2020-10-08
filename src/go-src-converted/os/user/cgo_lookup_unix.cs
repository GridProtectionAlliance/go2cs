// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd !android,linux netbsd openbsd solaris
// +build cgo,!osusergo

// package user -- go2cs converted at 2020 October 08 03:45:30 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Go\src\os\user\cgo_lookup_unix.go
using fmt = go.fmt_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using C = go.C_package;
using static go.builtin;
using System;

namespace go {
namespace os
{
    public static partial class user_package
    {
        private static (ptr<User>, error) current()
        {
            ptr<User> _p0 = default!;
            error _p0 = default!;

            return _addr_lookupUnixUid(syscall.Getuid())!;
        }

        private static (ptr<User>, error) lookupUser(@string username) => func((defer, _, __) =>
        {
            ptr<User> _p0 = default!;
            error _p0 = default!;

            ref C.struct_passwd pwd = ref heap(out ptr<C.struct_passwd> _addr_pwd);
            ptr<C.struct_passwd> result;
            var nameC = make_slice<byte>(len(username) + 1L);
            copy(nameC, username);

            var buf = alloc(userBuffer);
            defer(buf.free());

            var err = retryWithBuffer(_addr_buf, () =>
            { 
                // mygetpwnam_r is a wrapper around getpwnam_r to avoid
                // passing a size_t to getpwnam_r, because for unknown
                // reasons passing a size_t to getpwnam_r doesn't work on
                // Solaris.
                return _addr_syscall.Errno(C.mygetpwnam_r((C.@char.val)(@unsafe.Pointer(_addr_nameC[0L])), _addr_pwd, (C.@char.val)(buf.ptr), C.size_t(buf.size), _addr_result))!;

            });
            if (err != null)
            {
                return (_addr_null!, error.As(fmt.Errorf("user: lookup username %s: %v", username, err))!);
            }

            if (result == null)
            {
                return (_addr_null!, error.As(UnknownUserError(username))!);
            }

            return (_addr_buildUser(_addr_pwd)!, error.As(err)!);

        });

        private static (ptr<User>, error) lookupUserId(@string uid)
        {
            ptr<User> _p0 = default!;
            error _p0 = default!;

            var (i, e) = strconv.Atoi(uid);
            if (e != null)
            {
                return (_addr_null!, error.As(e)!);
            }

            return _addr_lookupUnixUid(i)!;

        }

        private static (ptr<User>, error) lookupUnixUid(long uid) => func((defer, _, __) =>
        {
            ptr<User> _p0 = default!;
            error _p0 = default!;

            ref C.struct_passwd pwd = ref heap(out ptr<C.struct_passwd> _addr_pwd);
            ptr<C.struct_passwd> result;

            var buf = alloc(userBuffer);
            defer(buf.free());

            var err = retryWithBuffer(_addr_buf, () =>
            { 
                // mygetpwuid_r is a wrapper around getpwuid_r to avoid using uid_t
                // because C.uid_t(uid) for unknown reasons doesn't work on linux.
                return _addr_syscall.Errno(C.mygetpwuid_r(C.@int(uid), _addr_pwd, (C.@char.val)(buf.ptr), C.size_t(buf.size), _addr_result))!;

            });
            if (err != null)
            {
                return (_addr_null!, error.As(fmt.Errorf("user: lookup userid %d: %v", uid, err))!);
            }

            if (result == null)
            {
                return (_addr_null!, error.As(UnknownUserIdError(uid))!);
            }

            return (_addr_buildUser(_addr_pwd)!, error.As(null!)!);

        });

        private static ptr<User> buildUser(ptr<C.struct_passwd> _addr_pwd)
        {
            ref C.struct_passwd pwd = ref _addr_pwd.val;

            ptr<User> u = addr(new User(Uid:strconv.FormatUint(uint64(pwd.pw_uid),10),Gid:strconv.FormatUint(uint64(pwd.pw_gid),10),Username:C.GoString(pwd.pw_name),Name:C.GoString(pwd.pw_gecos),HomeDir:C.GoString(pwd.pw_dir),)); 
            // The pw_gecos field isn't quite standardized. Some docs
            // say: "It is expected to be a comma separated list of
            // personal data where the first item is the full name of the
            // user."
            {
                var i = strings.Index(u.Name, ",");

                if (i >= 0L)
                {
                    u.Name = u.Name[..i];
                }

            }

            return _addr_u!;

        }

        private static (ptr<Group>, error) lookupGroup(@string groupname) => func((defer, _, __) =>
        {
            ptr<Group> _p0 = default!;
            error _p0 = default!;

            ref C.struct_group grp = ref heap(out ptr<C.struct_group> _addr_grp);
            ptr<C.struct_group> result;

            var buf = alloc(groupBuffer);
            defer(buf.free());
            var cname = make_slice<byte>(len(groupname) + 1L);
            copy(cname, groupname);

            var err = retryWithBuffer(_addr_buf, () =>
            {
                return _addr_syscall.Errno(C.mygetgrnam_r((C.@char.val)(@unsafe.Pointer(_addr_cname[0L])), _addr_grp, (C.@char.val)(buf.ptr), C.size_t(buf.size), _addr_result))!;
            });
            if (err != null)
            {
                return (_addr_null!, error.As(fmt.Errorf("user: lookup groupname %s: %v", groupname, err))!);
            }

            if (result == null)
            {
                return (_addr_null!, error.As(UnknownGroupError(groupname))!);
            }

            return (_addr_buildGroup(_addr_grp)!, error.As(null!)!);

        });

        private static (ptr<Group>, error) lookupGroupId(@string gid)
        {
            ptr<Group> _p0 = default!;
            error _p0 = default!;

            var (i, e) = strconv.Atoi(gid);
            if (e != null)
            {
                return (_addr_null!, error.As(e)!);
            }

            return _addr_lookupUnixGid(i)!;

        }

        private static (ptr<Group>, error) lookupUnixGid(long gid) => func((defer, _, __) =>
        {
            ptr<Group> _p0 = default!;
            error _p0 = default!;

            ref C.struct_group grp = ref heap(out ptr<C.struct_group> _addr_grp);
            ptr<C.struct_group> result;

            var buf = alloc(groupBuffer);
            defer(buf.free());

            var err = retryWithBuffer(_addr_buf, () =>
            { 
                // mygetgrgid_r is a wrapper around getgrgid_r to avoid using gid_t
                // because C.gid_t(gid) for unknown reasons doesn't work on linux.
                return _addr_syscall.Errno(C.mygetgrgid_r(C.@int(gid), _addr_grp, (C.@char.val)(buf.ptr), C.size_t(buf.size), _addr_result))!;

            });
            if (err != null)
            {
                return (_addr_null!, error.As(fmt.Errorf("user: lookup groupid %d: %v", gid, err))!);
            }

            if (result == null)
            {
                return (_addr_null!, error.As(UnknownGroupIdError(strconv.Itoa(gid)))!);
            }

            return (_addr_buildGroup(_addr_grp)!, error.As(null!)!);

        });

        private static ptr<Group> buildGroup(ptr<C.struct_group> _addr_grp)
        {
            ref C.struct_group grp = ref _addr_grp.val;

            ptr<Group> g = addr(new Group(Gid:strconv.Itoa(int(grp.gr_gid)),Name:C.GoString(grp.gr_name),));
            return _addr_g!;
        }

        private partial struct bufferKind // : C.int
        {
        }

        private static readonly var userBuffer = (var)bufferKind(C._SC_GETPW_R_SIZE_MAX);
        private static readonly var groupBuffer = (var)bufferKind(C._SC_GETGR_R_SIZE_MAX);


        private static C.size_t initialSize(this bufferKind k)
        {
            var sz = C.sysconf(C.@int(k));
            if (sz == -1L)
            { 
                // DragonFly and FreeBSD do not have _SC_GETPW_R_SIZE_MAX.
                // Additionally, not all Linux systems have it, either. For
                // example, the musl libc returns -1.
                return 1024L;

            }

            if (!isSizeReasonable(int64(sz)))
            { 
                // Truncate.  If this truly isn't enough, retryWithBuffer will error on the first run.
                return maxBufferSize;

            }

            return C.size_t(sz);

        }

        private partial struct memBuffer
        {
            public unsafe.Pointer ptr;
            public C.size_t size;
        }

        private static ptr<memBuffer> alloc(bufferKind kind)
        {
            var sz = kind.initialSize();
            return addr(new memBuffer(ptr:C.malloc(sz),size:sz,));
        }

        private static void resize(this ptr<memBuffer> _addr_mb, C.size_t newSize)
        {
            ref memBuffer mb = ref _addr_mb.val;

            mb.ptr = C.realloc(mb.ptr, newSize);
            mb.size = newSize;
        }

        private static void free(this ptr<memBuffer> _addr_mb)
        {
            ref memBuffer mb = ref _addr_mb.val;

            C.free(mb.ptr);
        }

        // retryWithBuffer repeatedly calls f(), increasing the size of the
        // buffer each time, until f succeeds, fails with a non-ERANGE error,
        // or the buffer exceeds a reasonable limit.
        private static error retryWithBuffer(ptr<memBuffer> _addr_buf, Func<syscall.Errno> f)
        {
            ref memBuffer buf = ref _addr_buf.val;

            while (true)
            {
                var errno = f();
                if (errno == 0L)
                {
                    return error.As(null!)!;
                }
                else if (errno != syscall.ERANGE)
                {
                    return error.As(errno)!;
                }

                var newSize = buf.size * 2L;
                if (!isSizeReasonable(int64(newSize)))
                {
                    return error.As(fmt.Errorf("internal buffer exceeds %d bytes", maxBufferSize))!;
                }

                buf.resize(newSize);

            }


        }

        private static readonly long maxBufferSize = (long)1L << (int)(20L);



        private static bool isSizeReasonable(long sz)
        {
            return sz > 0L && sz <= maxBufferSize;
        }

        // Because we can't use cgo in tests:
        private static C.struct_passwd structPasswdForNegativeTest()
        {
            C.struct_passwd sp = new C.struct_passwd();
            sp.pw_uid = 1L << (int)(32L) - 2L;
            sp.pw_gid = 1L << (int)(32L) - 3L;
            return sp;
        }
    }
}}
