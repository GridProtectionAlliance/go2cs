// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (!cgo && !windows && !plan9) || android || (osusergo && !windows && !plan9)
// +build !cgo,!windows,!plan9 android osusergo,!windows,!plan9

// package user -- go2cs converted at 2022 March 06 22:14:29 UTC
// import "os/user" ==> using user = go.os.user_package
// Original source: C:\Program Files\Go\src\os\user\lookup_stubs.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using os = go.os_package;
using runtime = go.runtime_package;
using strconv = go.strconv_package;

namespace go.os;

public static partial class user_package {

private static void init() {
    groupImplemented = false;
}

private static (ptr<User>, error) current() {
    ptr<User> _p0 = default!;
    error _p0 = default!;

    var uid = currentUID(); 
    // $USER and /etc/passwd may disagree; prefer the latter if we can get it.
    // See issue 27524 for more information.
    var (u, err) = lookupUserId(uid);
    if (err == null) {
        return (_addr_u!, error.As(null!)!);
    }
    var (homeDir, _) = os.UserHomeDir();
    u = addr(new User(Uid:uid,Gid:currentGID(),Username:os.Getenv("USER"),Name:"",HomeDir:homeDir,)); 
    // On Android, return a dummy user instead of failing.
    switch (runtime.GOOS) {
        case "android": 
            if (u.Uid == "") {
                u.Uid = "1";
            }
            if (u.Username == "") {
                u.Username = "android";
            }
            break;
    } 
    // cgo isn't available, but if we found the minimum information
    // without it, use it:
    if (u.Uid != "" && u.Username != "" && u.HomeDir != "") {
        return (_addr_u!, error.As(null!)!);
    }
    @string missing = default;
    if (u.Username == "") {
        missing = "$USER";
    }
    if (u.HomeDir == "") {
        if (missing != "") {
            missing += ", ";
        }
        missing += "$HOME";

    }
    return (_addr_u!, error.As(fmt.Errorf("user: Current requires cgo or %s set in environment", missing))!);

}

private static (slice<@string>, error) listGroups(ptr<User> _addr__p0) {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref User _p0 = ref _addr__p0.val;

    if (runtime.GOOS == "android" || runtime.GOOS == "aix") {
        return (null, error.As(fmt.Errorf("user: GroupIds not implemented on %s", runtime.GOOS))!);
    }
    return (null, error.As(errors.New("user: GroupIds requires cgo"))!);

}

private static @string currentUID() {
    {
        var id = os.Getuid();

        if (id >= 0) {
            return strconv.Itoa(id);
        }
    } 
    // Note: Windows returns -1, but this file isn't used on
    // Windows anyway, so this empty return path shouldn't be
    // used.
    return "";

}

private static @string currentGID() {
    {
        var id = os.Getgid();

        if (id >= 0) {
            return strconv.Itoa(id);
        }
    }

    return "";

}

} // end user_package
