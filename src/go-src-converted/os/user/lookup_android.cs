// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build android
// +build android

// package user -- go2cs converted at 2022 March 06 22:14:29 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Program Files\Go\src\os\user\lookup_android.go
using errors = go.errors_package;

namespace go.os;

public static partial class user_package {

private static (ptr<User>, error) lookupUser(@string _p0) {
    ptr<User> _p0 = default!;
    error _p0 = default!;

    return (_addr_null!, error.As(errors.New("user: Lookup not implemented on android"))!);
}

private static (ptr<User>, error) lookupUserId(@string _p0) {
    ptr<User> _p0 = default!;
    error _p0 = default!;

    return (_addr_null!, error.As(errors.New("user: LookupId not implemented on android"))!);
}

private static (ptr<Group>, error) lookupGroup(@string _p0) {
    ptr<Group> _p0 = default!;
    error _p0 = default!;

    return (_addr_null!, error.As(errors.New("user: LookupGroup not implemented on android"))!);
}

private static (ptr<Group>, error) lookupGroupId(@string _p0) {
    ptr<Group> _p0 = default!;
    error _p0 = default!;

    return (_addr_null!, error.As(errors.New("user: LookupGroupId not implemented on android"))!);
}

} // end user_package
