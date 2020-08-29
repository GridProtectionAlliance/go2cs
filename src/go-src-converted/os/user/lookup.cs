// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package user -- go2cs converted at 2020 August 29 08:31:50 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Go\src\os\user\lookup.go
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace os
{
    public static partial class user_package
    {
        // Current returns the current user.
        public static (ref User, error) Current()
        {
            cache.Do(() =>
            {
                cache.u, cache.err = current();

            });
            if (cache.err != null)
            {
                return (null, cache.err);
            }
            var u = cache.u.Value; // copy
            return (ref u, null);
        }

        // cache of the current user
        private static var cache = default;

        // Lookup looks up a user by username. If the user cannot be found, the
        // returned error is of type UnknownUserError.
        public static (ref User, error) Lookup(@string username)
        {
            {
                var (u, err) = Current();

                if (err == null && u.Username == username)
                {
                    return (u, err);
                }

            }
            return lookupUser(username);
        }

        // LookupId looks up a user by userid. If the user cannot be found, the
        // returned error is of type UnknownUserIdError.
        public static (ref User, error) LookupId(@string uid)
        {
            {
                var (u, err) = Current();

                if (err == null && u.Uid == uid)
                {
                    return (u, err);
                }

            }
            return lookupUserId(uid);
        }

        // LookupGroup looks up a group by name. If the group cannot be found, the
        // returned error is of type UnknownGroupError.
        public static (ref Group, error) LookupGroup(@string name)
        {
            return lookupGroup(name);
        }

        // LookupGroupId looks up a group by groupid. If the group cannot be found, the
        // returned error is of type UnknownGroupIdError.
        public static (ref Group, error) LookupGroupId(@string gid)
        {
            return lookupGroupId(gid);
        }

        // GroupIds returns the list of group IDs that the user is a member of.
        private static (slice<@string>, error) GroupIds(this ref User u)
        {
            return listGroups(u);
        }
    }
}}
