// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build android

// package user -- go2cs converted at 2020 August 29 08:31:50 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Go\src\os\user\lookup_android.go
using errors = go.errors_package;
using static go.builtin;

namespace go {
namespace os
{
    public static partial class user_package
    {
        private static (ref User, error) lookupUser(@string _p0)
        {
            return (null, errors.New("user: Lookup not implemented on android"));
        }

        private static (ref User, error) lookupUserId(@string _p0)
        {
            return (null, errors.New("user: LookupId not implemented on android"));
        }

        private static (ref Group, error) lookupGroup(@string _p0)
        {
            return (null, errors.New("user: LookupGroup not implemented on android"));
        }

        private static (ref Group, error) lookupGroupId(@string _p0)
        {
            return (null, errors.New("user: LookupGroupId not implemented on android"));
        }
    }
}}
