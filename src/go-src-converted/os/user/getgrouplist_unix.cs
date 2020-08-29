// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd !android,linux netbsd openbsd

// package user -- go2cs converted at 2020 August 29 08:31:49 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Go\src\os\user\getgrouplist_unix.go
/*
#include <unistd.h>
#include <sys/types.h>
#include <grp.h>

static int mygetgrouplist(const char* user, gid_t group, gid_t* groups, int* ngroups) {
    return getgrouplist(user, group, groups, ngroups);
}
*/
using C = go.C_package;/*
#include <unistd.h>
#include <sys/types.h>
#include <grp.h>

static int mygetgrouplist(const char* user, gid_t group, gid_t* groups, int* ngroups) {
    return getgrouplist(user, group, groups, ngroups);
}
*/

using fmt = go.fmt_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace os
{
    public static partial class user_package
    {
        private static C.int getGroupList(ref C.char name, C.gid_t userGID, ref C.gid_t gids, ref C.int n)
        {
            return C.mygetgrouplist(name, userGID, gids, n);
        }

        // groupRetry retries getGroupList with much larger size for n. The result is
        // stored in gids.
        private static error groupRetry(@string username, slice<byte> name, C.gid_t userGID, ref slice<C.gid_t> gids, ref C.int n)
        { 
            // More than initial buffer, but now n contains the correct size.
            if (n > maxGroups.Value)
            {
                return error.As(fmt.Errorf("user: %q is a member of more than %d groups", username, maxGroups));
            }
            gids.Value = make_slice<C.gid_t>(n.Value);
            var rv = getGroupList((C.@char.Value)(@unsafe.Pointer(ref name[0L])), userGID, ref (gids.Value)[0L], n);
            if (rv == -1L)
            {
                return error.As(fmt.Errorf("user: list groups for %s failed", username));
            }
            return error.As(null);
        }
    }
}}
