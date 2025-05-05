// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.os;

using sync = sync_package;

partial class user_package {

internal static readonly @string userFile = "/etc/passwd"u8;
internal static readonly @string groupFile = "/etc/group"u8;

internal static slice<byte> colon = new byte[]{(rune)':'}.slice();

// Current returns the current user.
//
// The first call will cache the current user information.
// Subsequent calls will return the cached value and will not reflect
// changes to the current user.
public static (ж<User>, error) Current() {
    cache.Do(
    var cacheʗ2 = cache;
    () => {
        (cacheʗ2.u, cacheʗ2.err) = current();
    });
    if (cache.err != default!) {
        return (default!, cache.err);
    }
    ref var u = ref heap<User>(out var Ꮡu);
    u = cache.u.val;
    // copy
    return (Ꮡu, default!);
}

// cache of the current user

[GoType("dyn")] partial struct cacheᴛ1 {
    public partial ref sync_package.Once Once { get; }
    internal ж<User> u;
    internal error err;
}
internal static cacheᴛ1 cache;

// Lookup looks up a user by username. If the user cannot be found, the
// returned error is of type [UnknownUserError].
public static (ж<User>, error) Lookup(@string username) {
    {
        (u, err) = Current(); if (err == default! && (~u).Username == username) {
            return (u, err);
        }
    }
    return lookupUser(username);
}

// LookupId looks up a user by userid. If the user cannot be found, the
// returned error is of type [UnknownUserIdError].
public static (ж<User>, error) LookupId(@string uid) {
    {
        (u, err) = Current(); if (err == default! && (~u).Uid == uid) {
            return (u, err);
        }
    }
    return lookupUserId(uid);
}

// LookupGroup looks up a group by name. If the group cannot be found, the
// returned error is of type [UnknownGroupError].
public static (ж<Group>, error) LookupGroup(@string name) {
    return lookupGroup(name);
}

// LookupGroupId looks up a group by groupid. If the group cannot be found, the
// returned error is of type [UnknownGroupIdError].
public static (ж<Group>, error) LookupGroupId(@string gid) {
    return lookupGroupId(gid);
}

// GroupIds returns the list of group IDs that the user is a member of.
[GoRecv] public static (slice<@string>, error) GroupIds(this ref User u) {
    return listGroups(u);
}

} // end user_package
