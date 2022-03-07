// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package nettest -- go2cs converted at 2022 March 06 23:38:10 UTC
// import "vendor/golang.org/x/net/nettest" ==> using nettest = go.vendor.golang.org.x.net.nettest_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\net\nettest\nettest_windows.go
using syscall = go.syscall_package;

namespace go.vendor.golang.org.x.net;

public static partial class nettest_package {

private static bool supportsRawSocket() { 
    // From http://msdn.microsoft.com/en-us/library/windows/desktop/ms740548.aspx:
    // Note: To use a socket of type SOCK_RAW requires administrative privileges.
    // Users running Winsock applications that use raw sockets must be a member of
    // the Administrators group on the local computer, otherwise raw socket calls
    // will fail with an error code of WSAEACCES. On Windows Vista and later, access
    // for raw sockets is enforced at socket creation. In earlier versions of Windows,
    // access for raw sockets is enforced during other socket operations.
    foreach (var (_, af) in new slice<nint>(new nint[] { syscall.AF_INET, syscall.AF_INET6 })) {
        var (s, err) = syscall.Socket(af, syscall.SOCK_RAW, 0);
        if (err != null) {
            continue;
        }
        syscall.Closesocket(s);
        return true;

    }    return false;

}

} // end nettest_package
