// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin
// +build arm arm64

// package time -- go2cs converted at 2020 August 29 08:42:28 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Go\src\time\zoneinfo_ios.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class time_package
    {
        private static @string zoneSources = new slice<@string>(new @string[] { getZipParent()+"/zoneinfo.zip" });

        private static @string getZipParent()
        {
            var (wd, err) = syscall.Getwd();
            if (err != null)
            {
                return "/XXXNOEXIST";
            } 

            // The working directory at initialization is the root of the
            // app bundle: "/private/.../bundlename.app". That's where we
            // keep zoneinfo.zip.
            return wd;
        }

        private static void initLocal()
        { 
            // TODO(crawshaw): [NSTimeZone localTimeZone]
            localLoc = UTC.Value;
        }
    }
}
