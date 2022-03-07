// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build ios
// +build ios

// package time -- go2cs converted at 2022 March 06 22:30:16 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Program Files\Go\src\time\zoneinfo_ios.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;

namespace go;

public static partial class time_package {

private static @string zoneSources = new slice<@string>(new @string[] { getZoneRoot()+"/zoneinfo.zip" });

private static @string getZoneRoot() => func((defer, _, _) => { 
    // The working directory at initialization is the root of the
    // app bundle: "/private/.../bundlename.app". That's where we
    // keep zoneinfo.zip for tethered iOS builds.
    // For self-hosted iOS builds, the zoneinfo.zip is in GOROOT.
    @string roots = new slice<@string>(new @string[] { runtime.GOROOT()+"/lib/time" });
    var (wd, err) = syscall.Getwd();
    if (err == null) {
        roots = append(roots, wd);
    }
    foreach (var (_, r) in roots) {
        ref syscall.Stat_t st = ref heap(out ptr<syscall.Stat_t> _addr_st);
        var (fd, err) = syscall.Open(r, syscall.O_RDONLY, 0);
        if (err != null) {
            continue;
        }
        defer(syscall.Close(fd));
        {
            var err = syscall.Fstat(fd, _addr_st);

            if (err == null) {
                return r;
            }

        }

    }    return "/XXXNOEXIST";

});

private static void initLocal() { 
    // TODO(crawshaw): [NSTimeZone localTimeZone]
    localLoc = UTC.val;

}

} // end time_package
