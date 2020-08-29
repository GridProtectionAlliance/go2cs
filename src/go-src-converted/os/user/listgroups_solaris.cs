// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build cgo

// Even though this file requires no C, it is used to provide a
// listGroup stub because all the other Solaris calls work.  Otherwise,
// this stub will conflict with the lookup_stubs.go fallback.

// package user -- go2cs converted at 2020 August 29 08:31:49 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Go\src\os\user\listgroups_solaris.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace os
{
    public static partial class user_package
    {
        private static (slice<@string>, error) listGroups(ref User u)
        {
            return (null, fmt.Errorf("user: list groups for %s: not supported on Solaris", u.Username));
        }
    }
}}
