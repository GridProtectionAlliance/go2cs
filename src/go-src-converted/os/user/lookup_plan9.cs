// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package user -- go2cs converted at 2020 August 29 08:31:50 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Go\src\os\user\lookup_plan9.go
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace os
{
    public static partial class user_package
    {
        // Partial os/user support on Plan 9.
        // Supports Current(), but not Lookup()/LookupId().
        // The latter two would require parsing /adm/users.
        private static readonly @string userFile = "/dev/user";

        private static void init()
        {
            groupImplemented = false;
        }

        private static (ref User, error) current()
        {
            var (ubytes, err) = ioutil.ReadFile(userFile);
            if (err != null)
            {
                return (null, fmt.Errorf("user: %s", err));
            }
            var uname = string(ubytes);

            User u = ref new User(Uid:uname,Gid:uname,Username:uname,Name:uname,HomeDir:os.Getenv("home"),);

            return (u, null);
        }

        private static (ref User, error) lookupUser(@string username)
        {
            return (null, syscall.EPLAN9);
        }

        private static (ref User, error) lookupUserId(@string uid)
        {
            return (null, syscall.EPLAN9);
        }

        private static (ref Group, error) lookupGroup(@string groupname)
        {
            return (null, syscall.EPLAN9);
        }

        private static (ref Group, error) lookupGroupId(@string _p0)
        {
            return (null, syscall.EPLAN9);
        }

        private static (slice<@string>, error) listGroups(ref User _p0)
        {
            return (null, syscall.EPLAN9);
        }
    }
}}
