// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package user -- go2cs converted at 2022 March 06 22:14:29 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Program Files\Go\src\os\user\lookup_plan9.go
using fmt = go.fmt_package;
using os = go.os_package;
using syscall = go.syscall_package;

namespace go.os;

public static partial class user_package {

    // Partial os/user support on Plan 9.
    // Supports Current(), but not Lookup()/LookupId().
    // The latter two would require parsing /adm/users.
private static readonly @string userFile = "/dev/user";


private static void init() {
    groupImplemented = false;
}

private static (ptr<User>, error) current() {
    ptr<User> _p0 = default!;
    error _p0 = default!;

    var (ubytes, err) = os.ReadFile(userFile);
    if (err != null) {
        return (_addr_null!, error.As(fmt.Errorf("user: %s", err))!);
    }
    var uname = string(ubytes);

    ptr<User> u = addr(new User(Uid:uname,Gid:uname,Username:uname,Name:uname,HomeDir:os.Getenv("home"),));

    return (_addr_u!, error.As(null!)!);

}

private static (ptr<User>, error) lookupUser(@string username) {
    ptr<User> _p0 = default!;
    error _p0 = default!;

    return (_addr_null!, error.As(syscall.EPLAN9)!);
}

private static (ptr<User>, error) lookupUserId(@string uid) {
    ptr<User> _p0 = default!;
    error _p0 = default!;

    return (_addr_null!, error.As(syscall.EPLAN9)!);
}

private static (ptr<Group>, error) lookupGroup(@string groupname) {
    ptr<Group> _p0 = default!;
    error _p0 = default!;

    return (_addr_null!, error.As(syscall.EPLAN9)!);
}

private static (ptr<Group>, error) lookupGroupId(@string _p0) {
    ptr<Group> _p0 = default!;
    error _p0 = default!;

    return (_addr_null!, error.As(syscall.EPLAN9)!);
}

private static (slice<@string>, error) listGroups(ptr<User> _addr__p0) {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref User _p0 = ref _addr__p0.val;

    return (null, error.As(syscall.EPLAN9)!);
}

} // end user_package
