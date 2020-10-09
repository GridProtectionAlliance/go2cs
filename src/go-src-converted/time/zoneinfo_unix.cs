// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin,amd64 dragonfly freebsd linux,!android netbsd openbsd solaris

// Parse "zoneinfo" time zone file.
// This is a fairly standard file format used on OS X, Linux, BSD, Sun, and others.
// See tzfile(5), https://en.wikipedia.org/wiki/Zoneinfo,
// and ftp://munnari.oz.au/pub/oldtz/

// package time -- go2cs converted at 2020 October 09 05:06:16 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Go\src\time\zoneinfo_unix.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class time_package
    {
        // Many systems use /usr/share/zoneinfo, Solaris 2 has
        // /usr/share/lib/zoneinfo, IRIX 6 has /usr/lib/locale/TZ.
        private static @string zoneSources = new slice<@string>(new @string[] { "/usr/share/zoneinfo/", "/usr/share/lib/zoneinfo/", "/usr/lib/locale/TZ/", runtime.GOROOT()+"/lib/time/zoneinfo.zip" });

        private static void initLocal()
        { 
            // consult $TZ to find the time zone to use.
            // no $TZ means use the system default /etc/localtime.
            // $TZ="" means use UTC.
            // $TZ="foo" means use /usr/share/zoneinfo/foo.

            var (tz, ok) = syscall.Getenv("TZ");

            if (!ok) 
                var (z, err) = loadLocation("localtime", new slice<@string>(new @string[] { "/etc" }));
                if (err == null)
                {
                    localLoc = z.val;
                    localLoc.name = "Local";
                    return ;
                }

            else if (tz != "" && tz != "UTC") 
                {
                    var z__prev1 = z;

                    (z, err) = loadLocation(tz, zoneSources);

                    if (err == null)
                    {
                        localLoc = z.val;
                        return ;
                    }

                    z = z__prev1;

                }

            // Fall back to UTC.
            localLoc.name = "UTC";

        }
    }
}
