// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || (darwin && !ios) || dragonfly || freebsd || (linux && !android) || netbsd || openbsd || solaris
// +build aix darwin,!ios dragonfly freebsd linux,!android netbsd openbsd solaris

// Parse "zoneinfo" time zone file.
// This is a fairly standard file format used on OS X, Linux, BSD, Sun, and others.
// See tzfile(5), https://en.wikipedia.org/wiki/Zoneinfo,
// and ftp://munnari.oz.au/pub/oldtz/

// package time -- go2cs converted at 2022 March 13 05:41:08 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Program Files\Go\src\time\zoneinfo_unix.go
namespace go;

using runtime = runtime_package;
using syscall = syscall_package;


// Many systems use /usr/share/zoneinfo, Solaris 2 has
// /usr/share/lib/zoneinfo, IRIX 6 has /usr/lib/locale/TZ.

public static partial class time_package {

private static @string zoneSources = new slice<@string>(new @string[] { "/usr/share/zoneinfo/", "/usr/share/lib/zoneinfo/", "/usr/lib/locale/TZ/", runtime.GOROOT()+"/lib/time/zoneinfo.zip" });

private static void initLocal() { 
    // consult $TZ to find the time zone to use.
    // no $TZ means use the system default /etc/localtime.
    // $TZ="" means use UTC.
    // $TZ="foo" or $TZ=":foo" if foo is an absolute path, then the file pointed
    // by foo will be used to initialize timezone; otherwise, file
    // /usr/share/zoneinfo/foo will be used.

    var (tz, ok) = syscall.Getenv("TZ");

    if (!ok) 
        var (z, err) = loadLocation("localtime", new slice<@string>(new @string[] { "/etc" }));
        if (err == null) {
            localLoc = z.val;
            localLoc.name = "Local";
            return ;
        }
    else if (tz != "") 
        if (tz[0] == ':') {
            tz = tz[(int)1..];
        }
        if (tz != "" && tz[0] == '/') {
            {
                var z__prev2 = z;

                (z, err) = loadLocation(tz, new slice<@string>(new @string[] { "" }));

                if (err == null) {
                    localLoc = z.val;
                    if (tz == "/etc/localtime") {
                        localLoc.name = "Local";
                    }
                    else
 {
                        localLoc.name = tz;
                    }
                    return ;
                }

                z = z__prev2;

            }
        }
        else if (tz != "" && tz != "UTC") {
            {
                var z__prev3 = z;

                (z, err) = loadLocation(tz, zoneSources);

                if (err == null) {
                    localLoc = z.val;
                    return ;
                }

                z = z__prev3;

            }
        }
    // Fall back to UTC.
    localLoc.name = "UTC";
}

} // end time_package
