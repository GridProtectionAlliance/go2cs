// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package user -- go2cs converted at 2020 August 29 08:31:49 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Go\src\os\user\getgrouplist_darwin.go
/*
#include <unistd.h>
#include <sys/types.h>
#include <stdlib.h>

static int mygetgrouplist(const char* user, gid_t group, gid_t* groups, int* ngroups) {
    int* buf = malloc(*ngroups * sizeof(int));
    int rv = getgrouplist(user, (int) group, buf, ngroups);
    int i;
    if (rv == 0) {
        for (i = 0; i < *ngroups; i++) {
            groups[i] = (gid_t) buf[i];
        }
    }
    free(buf);
    return rv;
}
*/
using C = go.C_package;/*
#include <unistd.h>
#include <sys/types.h>
#include <stdlib.h>

static int mygetgrouplist(const char* user, gid_t group, gid_t* groups, int* ngroups) {
    int* buf = malloc(*ngroups * sizeof(int));
    int rv = getgrouplist(user, (int) group, buf, ngroups);
    int i;
    if (rv == 0) {
        for (i = 0; i < *ngroups; i++) {
            groups[i] = (gid_t) buf[i];
        }
    }
    free(buf);
    return rv;
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

        // groupRetry retries getGroupList with an increasingly large size for n. The
        // result is stored in gids.
        private static error groupRetry(@string username, slice<byte> name, C.gid_t userGID, ref slice<C.gid_t> gids, ref C.int n)
        {
            n.Value = C.@int(256L * 2L);
            while (true)
            {
                gids.Value = make_slice<C.gid_t>(n.Value);
                var rv = getGroupList((C.@char.Value)(@unsafe.Pointer(ref name[0L])), userGID, ref (gids.Value)[0L], n);
                if (rv >= 0L)
                { 
                    // n is set correctly
                    break;
                }
                if (n > maxGroups.Value)
                {
                    return error.As(fmt.Errorf("user: %q is a member of more than %d groups", username, maxGroups));
                }
                n.Value = n * C.@int(2L).Value;
            }

            return error.As(null);
        }
    }
}}
