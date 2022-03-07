// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build cgo && !osusergo
// +build cgo,!osusergo

// package user -- go2cs converted at 2022 March 06 22:14:28 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Program Files\Go\src\os\user\getgrouplist_darwin.go
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

namespace go.os;

public static partial class user_package {

private static C.int getGroupList(ptr<C.char> _addr_name, C.gid_t userGID, ptr<C.gid_t> _addr_gids, ptr<C.int> _addr_n) {
    ref C.char name = ref _addr_name.val;
    ref C.gid_t gids = ref _addr_gids.val;
    ref C.int n = ref _addr_n.val;

    return C.mygetgrouplist(name, userGID, gids, n);
}

// groupRetry retries getGroupList with an increasingly large size for n. The
// result is stored in gids.
private static error groupRetry(@string username, slice<byte> name, C.gid_t userGID, ptr<slice<C.gid_t>> _addr_gids, ptr<C.int> _addr_n) {
    ref slice<C.gid_t> gids = ref _addr_gids.val;
    ref C.int n = ref _addr_n.val;

    n = C.@int(256 * 2);
    while (true) {
        gids = make_slice<C.gid_t>(n);
        var rv = getGroupList(_addr_(C.@char.val)(@unsafe.Pointer(_addr_name[0])), userGID, _addr_(gids)[0], _addr_n);
        if (rv >= 0) { 
            // n is set correctly
            break;

        }
        if (n > maxGroups.val) {
            return error.As(fmt.Errorf("user: %q is a member of more than %d groups", username, maxGroups))!;
        }
        n = n * C.@int(2).val;

    }
    return error.As(null!)!;

}

} // end user_package
