// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package nettest -- go2cs converted at 2020 October 09 06:07:44 UTC
// import "vendor/golang.org/x/net/nettest" ==> using nettest = go.vendor.golang.org.x.net.nettest_package
// Original source: C:\Go\src\vendor\golang.org\x\net\nettest\nettest_windows.go
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace net
{
    public static partial class nettest_package
    {
        private static bool supportsRawSocket()
        { 
            // From http://msdn.microsoft.com/en-us/library/windows/desktop/ms740548.aspx:
            // Note: To use a socket of type SOCK_RAW requires administrative privileges.
            // Users running Winsock applications that use raw sockets must be a member of
            // the Administrators group on the local computer, otherwise raw socket calls
            // will fail with an error code of WSAEACCES. On Windows Vista and later, access
            // for raw sockets is enforced at socket creation. In earlier versions of Windows,
            // access for raw sockets is enforced during other socket operations.
            foreach (var (_, af) in new slice<long>(new long[] { syscall.AF_INET, syscall.AF_INET6 }))
            {
                var (s, err) = syscall.Socket(af, syscall.SOCK_RAW, 0L);
                if (err != null)
                {
                    continue;
                }
                syscall.Closesocket(s);
                return true;

            }            return false;

        }
    }
}}}}}
