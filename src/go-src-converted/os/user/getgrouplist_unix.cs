// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd !android,linux netbsd openbsd
// +build cgo,!osusergo

// package user -- go2cs converted at 2020 October 09 05:07:34 UTC
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
        private static C.int getGroupList(ptr<C.char> _addr_name, C.gid_t userGID, ptr<C.gid_t> _addr_gids, ptr<C.int> _addr_n)
        {
            ref C.char name = ref _addr_name.val;
            ref C.gid_t gids = ref _addr_gids.val;
            ref C.int n = ref _addr_n.val;

            return C.mygetgrouplist(name, userGID, gids, n);
        }

        // groupRetry retries getGroupList with much larger size for n. The result is
        // stored in gids.
        private static error groupRetry(@string username, slice<byte> name, C.gid_t userGID, ptr<slice<C.gid_t>> _addr_gids, ptr<C.int> _addr_n)
        {
            ref slice<C.gid_t> gids = ref _addr_gids.val;
            ref C.int n = ref _addr_n.val;
 
            // More than initial buffer, but now n contains the correct size.
            if (n > maxGroups.val)
            {
                return error.As(fmt.Errorf("user: %q is a member of more than %d groups", username, maxGroups))!;
            }

            gids = make_slice<C.gid_t>(n);
            var rv = getGroupList(_addr_(C.@char.val)(@unsafe.Pointer(_addr_name[0L])), userGID, _addr_(gids)[0L], _addr_n);
            if (rv == -1L)
            {
                return error.As(fmt.Errorf("user: list groups for %s failed", username))!;
            }

            return error.As(null!)!;

        }
    }
}}
