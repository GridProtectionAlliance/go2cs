// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd solaris

// package nettest -- go2cs converted at 2020 August 29 10:12:07 UTC
// import "vendor/golang_org/x/net/internal/nettest" ==> using nettest = go.vendor.golang_org.x.net.@internal.nettest_package
// Original source: C:\Go\src\vendor\golang_org\x\net\internal\nettest\helper_unix.go
using fmt = go.fmt_package;
using os = go.os_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net {
namespace @internal
{
    public static partial class nettest_package
    {
        private static long maxOpenFiles()
        {
            syscall.Rlimit rlim = default;
            {
                var err = syscall.Getrlimit(syscall.RLIMIT_NOFILE, ref rlim);

                if (err != null)
                {
                    return defaultMaxOpenFiles;
                }
            }
            return int(rlim.Cur);
        }

        private static (@string, bool) supportsRawIPSocket()
        {
            if (os.Getuid() != 0L)
            {
                return (fmt.Sprintf("must be root on %s", runtime.GOOS), false);
            }
            return ("", true);
        }
    }
}}}}}}
