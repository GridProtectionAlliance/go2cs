// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build cgo && !osusergo
// +build cgo,!osusergo

// Even though this file requires no C, it is used to provide a
// listGroup stub because all the other illumos calls work.  Otherwise,
// this stub will conflict with the lookup_stubs.go fallback.

// package user -- go2cs converted at 2022 March 06 22:14:28 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Program Files\Go\src\os\user\listgroups_illumos.go
using fmt = go.fmt_package;

namespace go.os;

public static partial class user_package {

private static (slice<@string>, error) listGroups(ptr<User> _addr_u) {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref User u = ref _addr_u.val;

    return (null, error.As(fmt.Errorf("user: list groups for %s: not supported on illumos", u.Username))!);
}

} // end user_package
