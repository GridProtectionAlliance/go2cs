// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build cgo,!osusergo

// package user -- go2cs converted at 2020 October 09 05:07:34 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Go\src\os\user\listgroups_aix.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace os
{
    public static partial class user_package
    {
        private static (slice<@string>, error) listGroups(ptr<User> _addr_u)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;
            ref User u = ref _addr_u.val;

            return (null, error.As(fmt.Errorf("user: list groups for %s: not supported on AIX", u.Username))!);
        }
    }
}}
