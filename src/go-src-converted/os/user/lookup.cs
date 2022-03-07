// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package user -- go2cs converted at 2022 March 06 22:14:28 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Program Files\Go\src\os\user\lookup.go
using sync = go.sync_package;
using System;


namespace go.os;

public static partial class user_package {

    // Current returns the current user.
    //
    // The first call will cache the current user information.
    // Subsequent calls will return the cached value and will not reflect
    // changes to the current user.
public static (ptr<User>, error) Current() {
    ptr<User> _p0 = default!;
    error _p0 = default!;

    cache.Do(() => {
        cache.u, cache.err = current();
    });
    if (cache.err != null) {
        return (_addr_null!, error.As(cache.err)!);
    }
    ref var u = ref heap(cache.u.val, out ptr<var> _addr_u); // copy
    return (_addr__addr_u!, error.As(null!)!);

}

// cache of the current user
private static var cache = default;

// Lookup looks up a user by username. If the user cannot be found, the
// returned error is of type UnknownUserError.
public static (ptr<User>, error) Lookup(@string username) {
    ptr<User> _p0 = default!;
    error _p0 = default!;

    {
        var (u, err) = Current();

        if (err == null && u.Username == username) {
            return (_addr_u!, error.As(err)!);
        }
    }

    return _addr_lookupUser(username)!;

}

// LookupId looks up a user by userid. If the user cannot be found, the
// returned error is of type UnknownUserIdError.
public static (ptr<User>, error) LookupId(@string uid) {
    ptr<User> _p0 = default!;
    error _p0 = default!;

    {
        var (u, err) = Current();

        if (err == null && u.Uid == uid) {
            return (_addr_u!, error.As(err)!);
        }
    }

    return _addr_lookupUserId(uid)!;

}

// LookupGroup looks up a group by name. If the group cannot be found, the
// returned error is of type UnknownGroupError.
public static (ptr<Group>, error) LookupGroup(@string name) {
    ptr<Group> _p0 = default!;
    error _p0 = default!;

    return _addr_lookupGroup(name)!;
}

// LookupGroupId looks up a group by groupid. If the group cannot be found, the
// returned error is of type UnknownGroupIdError.
public static (ptr<Group>, error) LookupGroupId(@string gid) {
    ptr<Group> _p0 = default!;
    error _p0 = default!;

    return _addr_lookupGroupId(gid)!;
}

// GroupIds returns the list of group IDs that the user is a member of.
private static (slice<@string>, error) GroupIds(this ptr<User> _addr_u) {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref User u = ref _addr_u.val;

    return listGroups(u);
}

} // end user_package
