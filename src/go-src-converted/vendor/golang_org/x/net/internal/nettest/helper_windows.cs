// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package nettest -- go2cs converted at 2020 August 29 10:12:09 UTC
// import "vendor/golang_org/x/net/internal/nettest" ==> using nettest = go.vendor.golang_org.x.net.@internal.nettest_package
// Original source: C:\Go\src\vendor\golang_org\x\net\internal\nettest\helper_windows.go
using fmt = go.fmt_package;
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
            return 4L * defaultMaxOpenFiles; /* actually it's 16581375 */
        }

        private static (@string, bool) supportsRawIPSocket()
        { 
            // From http://msdn.microsoft.com/en-us/library/windows/desktop/ms740548.aspx:
            // Note: To use a socket of type SOCK_RAW requires administrative privileges.
            // Users running Winsock applications that use raw sockets must be a member of
            // the Administrators group on the local computer, otherwise raw socket calls
            // will fail with an error code of WSAEACCES. On Windows Vista and later, access
            // for raw sockets is enforced at socket creation. In earlier versions of Windows,
            // access for raw sockets is enforced during other socket operations.
            var (s, err) = syscall.Socket(syscall.AF_INET, syscall.SOCK_RAW, 0L);
            if (err == syscall.WSAEACCES)
            {
                return (fmt.Sprintf("no access to raw socket allowed on %s", runtime.GOOS), false);
            }
            if (err != null)
            {
                return (err.Error(), false);
            }
            syscall.Closesocket(s);
            return ("", true);
        }

        private static bool supportsIPv6MulticastDeliveryOnLoopback()
        {
            return true;
        }

        private static bool causesIPv6Crash()
        {
            return false;
        }
    }
}}}}}}
