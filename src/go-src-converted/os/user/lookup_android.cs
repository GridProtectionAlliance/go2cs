// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build android

// package user -- go2cs converted at 2020 October 09 05:07:35 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Go\src\os\user\lookup_android.go
using errors = go.errors_package;
using static go.builtin;

namespace go {
namespace os
{
    public static partial class user_package
    {
        private static (ptr<User>, error) lookupUser(@string _p0)
        {
            ptr<User> _p0 = default!;
            error _p0 = default!;

            return (_addr_null!, error.As(errors.New("user: Lookup not implemented on android"))!);
        }

        private static (ptr<User>, error) lookupUserId(@string _p0)
        {
            ptr<User> _p0 = default!;
            error _p0 = default!;

            return (_addr_null!, error.As(errors.New("user: LookupId not implemented on android"))!);
        }

        private static (ptr<Group>, error) lookupGroup(@string _p0)
        {
            ptr<Group> _p0 = default!;
            error _p0 = default!;

            return (_addr_null!, error.As(errors.New("user: LookupGroup not implemented on android"))!);
        }

        private static (ptr<Group>, error) lookupGroupId(@string _p0)
        {
            ptr<Group> _p0 = default!;
            error _p0 = default!;

            return (_addr_null!, error.As(errors.New("user: LookupGroupId not implemented on android"))!);
        }
    }
}}
