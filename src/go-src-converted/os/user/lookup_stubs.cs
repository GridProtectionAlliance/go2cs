// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !cgo,!windows,!plan9 android

// package user -- go2cs converted at 2020 August 29 08:31:51 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Go\src\os\user\lookup_stubs.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using os = go.os_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using static go.builtin;

namespace go {
namespace os
{
    public static partial class user_package
    {
        private static void init()
        {
            groupImplemented = false;
        }

        private static (ref User, error) current()
        {
            User u = ref new User(Uid:currentUID(),Gid:currentGID(),Username:os.Getenv("USER"),Name:"",HomeDir:os.Getenv("HOME"),); 
            // On NaCL and Android, return a dummy user instead of failing.
            switch (runtime.GOOS)
            {
                case "nacl": 
                    if (u.Uid == "")
                    {
                        u.Uid = "1";
                    }
                    if (u.Username == "")
                    {
                        u.Username = "nacl";
                    }
                    if (u.HomeDir == "")
                    {
                        u.HomeDir = "/";
                    }
                    break;
                case "android": 
                    if (u.Uid == "")
                    {
                        u.Uid = "1";
                    }
                    if (u.Username == "")
                    {
                        u.Username = "android";
                    }
                    if (u.HomeDir == "")
                    {
                        u.HomeDir = "/sdcard";
                    }
                    break;
            } 
            // cgo isn't available, but if we found the minimum information
            // without it, use it:
            if (u.Uid != "" && u.Username != "" && u.HomeDir != "")
            {
                return (u, null);
            }
            return (u, fmt.Errorf("user: Current not implemented on %s/%s", runtime.GOOS, runtime.GOARCH));
        }

        private static (slice<@string>, error) listGroups(ref User _p0)
        {
            if (runtime.GOOS == "android")
            {
                return (null, errors.New("user: GroupIds not implemented on Android"));
            }
            return (null, errors.New("user: GroupIds requires cgo"));
        }

        private static @string currentUID()
        {
            {
                var id = os.Getuid();

                if (id >= 0L)
                {
                    return strconv.Itoa(id);
                } 
                // Note: Windows returns -1, but this file isn't used on
                // Windows anyway, so this empty return path shouldn't be
                // used.

            } 
            // Note: Windows returns -1, but this file isn't used on
            // Windows anyway, so this empty return path shouldn't be
            // used.
            return "";
        }

        private static @string currentGID()
        {
            {
                var id = os.Getgid();

                if (id >= 0L)
                {
                    return strconv.Itoa(id);
                }

            }
            return "";
        }
    }
}}
