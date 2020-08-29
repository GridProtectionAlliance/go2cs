// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package user allows user account lookups by name or id.
// package user -- go2cs converted at 2020 August 29 08:31:54 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Go\src\os\user\user.go
using strconv = go.strconv_package;
using static go.builtin;

namespace go {
namespace os
{
    public static partial class user_package
    {
        private static var userImplemented = true;        private static var groupImplemented = true;

        // User represents a user account.
        public partial struct User
        {
            public @string Uid; // Gid is the primary group ID.
// On POSIX systems, this is a decimal number representing the gid.
// On Windows, this is a SID in a string format.
// On Plan 9, this is the contents of /dev/user.
            public @string Gid; // Username is the login name.
            public @string Username; // Name is the user's real or display name.
// It might be blank.
// On POSIX systems, this is the first (or only) entry in the GECOS field
// list.
// On Windows, this is the user's display name.
// On Plan 9, this is the contents of /dev/user.
            public @string Name; // HomeDir is the path to the user's home directory (if they have one).
            public @string HomeDir;
        }

        // Group represents a grouping of users.
        //
        // On POSIX systems Gid contains a decimal number representing the group ID.
        public partial struct Group
        {
            public @string Gid; // group ID
            public @string Name; // group name
        }

        // UnknownUserIdError is returned by LookupId when a user cannot be found.
        public partial struct UnknownUserIdError // : long
        {
        }

        public static @string Error(this UnknownUserIdError e)
        {
            return "user: unknown userid " + strconv.Itoa(int(e));
        }

        // UnknownUserError is returned by Lookup when
        // a user cannot be found.
        public partial struct UnknownUserError // : @string
        {
        }

        public static @string Error(this UnknownUserError e)
        {
            return "user: unknown user " + string(e);
        }

        // UnknownGroupIdError is returned by LookupGroupId when
        // a group cannot be found.
        public partial struct UnknownGroupIdError // : @string
        {
        }

        public static @string Error(this UnknownGroupIdError e)
        {
            return "group: unknown groupid " + string(e);
        }

        // UnknownGroupError is returned by LookupGroup when
        // a group cannot be found.
        public partial struct UnknownGroupError // : @string
        {
        }

        public static @string Error(this UnknownGroupError e)
        {
            return "group: unknown group " + string(e);
        }
    }
}}
