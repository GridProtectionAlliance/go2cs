// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd !android,linux netbsd openbsd solaris
// +build cgo

// package user -- go2cs converted at 2020 August 29 08:31:48 UTC
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
        private static (ref User, error) current()
        {
            return lookupUnixUid(syscall.Getuid());
        }

        private static (ref User, error) lookupUser(@string username) => func((defer, _, __) =>
        {
            C.struct_passwd pwd = default;
            ref C.struct_passwd result = default;
            var nameC = make_slice<byte>(len(username) + 1L);
            copy(nameC, username);

            var buf = alloc(userBuffer);
            defer(buf.free());

            var err = retryWithBuffer(buf, () =>
            { 
                // mygetpwnam_r is a wrapper around getpwnam_r to avoid
                // passing a size_t to getpwnam_r, because for unknown
                // reasons passing a size_t to getpwnam_r doesn't work on
                // Solaris.
                return syscall.Errno(C.mygetpwnam_r((C.@char.Value)(@unsafe.Pointer(ref nameC[0L])), ref pwd, (C.@char.Value)(buf.ptr), C.size_t(buf.size), ref result));
            });
            if (err != null)
            {
                return (null, fmt.Errorf("user: lookup username %s: %v", username, err));
            }
            if (result == null)
            {
                return (null, UnknownUserError(username));
            }
            return (buildUser(ref pwd), err);
        });

        private static (ref User, error) lookupUserId(@string uid)
        {
            var (i, e) = strconv.Atoi(uid);
            if (e != null)
            {
                return (null, e);
            }
            return lookupUnixUid(i);
        }

        private static (ref User, error) lookupUnixUid(long uid) => func((defer, _, __) =>
        {
            C.struct_passwd pwd = default;
            ref C.struct_passwd result = default;

            var buf = alloc(userBuffer);
            defer(buf.free());

            var err = retryWithBuffer(buf, () =>
            { 
                // mygetpwuid_r is a wrapper around getpwuid_r to
                // to avoid using uid_t because C.uid_t(uid) for
                // unknown reasons doesn't work on linux.
                return syscall.Errno(C.mygetpwuid_r(C.@int(uid), ref pwd, (C.@char.Value)(buf.ptr), C.size_t(buf.size), ref result));
            });
            if (err != null)
            {
                return (null, fmt.Errorf("user: lookup userid %d: %v", uid, err));
            }
            if (result == null)
            {
                return (null, UnknownUserIdError(uid));
            }
            return (buildUser(ref pwd), null);
        });

        private static ref User buildUser(ref C.struct_passwd pwd)
        {
            User u = ref new User(Uid:strconv.FormatUint(uint64(pwd.pw_uid),10),Gid:strconv.FormatUint(uint64(pwd.pw_gid),10),Username:C.GoString(pwd.pw_name),Name:C.GoString(pwd.pw_gecos),HomeDir:C.GoString(pwd.pw_dir),); 
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
            return u;
        }

        private static (ref Group, error) currentGroup()
        {
            return lookupUnixGid(syscall.Getgid());
        }

        private static (ref Group, error) lookupGroup(@string groupname) => func((defer, _, __) =>
        {
            C.struct_group grp = default;
            ref C.struct_group result = default;

            var buf = alloc(groupBuffer);
            defer(buf.free());
            var cname = make_slice<byte>(len(groupname) + 1L);
            copy(cname, groupname);

            var err = retryWithBuffer(buf, () =>
            {
                return syscall.Errno(C.mygetgrnam_r((C.@char.Value)(@unsafe.Pointer(ref cname[0L])), ref grp, (C.@char.Value)(buf.ptr), C.size_t(buf.size), ref result));
            });
            if (err != null)
            {
                return (null, fmt.Errorf("user: lookup groupname %s: %v", groupname, err));
            }
            if (result == null)
            {
                return (null, UnknownGroupError(groupname));
            }
            return (buildGroup(ref grp), null);
        });

        private static (ref Group, error) lookupGroupId(@string gid)
        {
            var (i, e) = strconv.Atoi(gid);
            if (e != null)
            {
                return (null, e);
            }
            return lookupUnixGid(i);
        }

        private static (ref Group, error) lookupUnixGid(long gid) => func((defer, _, __) =>
        {
            C.struct_group grp = default;
            ref C.struct_group result = default;

            var buf = alloc(groupBuffer);
            defer(buf.free());

            var err = retryWithBuffer(buf, () =>
            { 
                // mygetgrgid_r is a wrapper around getgrgid_r to
                // to avoid using gid_t because C.gid_t(gid) for
                // unknown reasons doesn't work on linux.
                return syscall.Errno(C.mygetgrgid_r(C.@int(gid), ref grp, (C.@char.Value)(buf.ptr), C.size_t(buf.size), ref result));
            });
            if (err != null)
            {
                return (null, fmt.Errorf("user: lookup groupid %d: %v", gid, err));
            }
            if (result == null)
            {
                return (null, UnknownGroupIdError(strconv.Itoa(gid)));
            }
            return (buildGroup(ref grp), null);
        });

        private static ref Group buildGroup(ref C.struct_group grp)
        {
            Group g = ref new Group(Gid:strconv.Itoa(int(grp.gr_gid)),Name:C.GoString(grp.gr_name),);
            return g;
        }

        private partial struct bufferKind // : C.int
        {
        }

        private static readonly var userBuffer = bufferKind(C._SC_GETPW_R_SIZE_MAX);
        private static readonly var groupBuffer = bufferKind(C._SC_GETGR_R_SIZE_MAX);

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

        private static ref memBuffer alloc(bufferKind kind)
        {
            var sz = kind.initialSize();
            return ref new memBuffer(ptr:C.malloc(sz),size:sz,);
        }

        private static void resize(this ref memBuffer mb, C.size_t newSize)
        {
            mb.ptr = C.realloc(mb.ptr, newSize);
            mb.size = newSize;
        }

        private static void free(this ref memBuffer mb)
        {
            C.free(mb.ptr);
        }

        // retryWithBuffer repeatedly calls f(), increasing the size of the
        // buffer each time, until f succeeds, fails with a non-ERANGE error,
        // or the buffer exceeds a reasonable limit.
        private static error retryWithBuffer(ref memBuffer buf, Func<syscall.Errno> f)
        {
            while (true)
            {
                var errno = f();
                if (errno == 0L)
                {
                    return error.As(null);
                }
                else if (errno != syscall.ERANGE)
                {
                    return error.As(errno);
                }
                var newSize = buf.size * 2L;
                if (!isSizeReasonable(int64(newSize)))
                {
                    return error.As(fmt.Errorf("internal buffer exceeds %d bytes", maxBufferSize));
                }
                buf.resize(newSize);
            }

        }

        private static readonly long maxBufferSize = 1L << (int)(20L);



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
