// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !plan9
// +build !plan9

// package socktest -- go2cs converted at 2022 March 06 22:25:41 UTC
// import "net/internal/socktest" ==> using socktest = go.net.@internal.socktest_package
// Original source: C:\Program Files\Go\src\net\internal\socktest\switch_posix.go
using fmt = go.fmt_package;
using syscall = go.syscall_package;

namespace go.net.@internal;

public static partial class socktest_package {

private static @string familyString(nint family) {

    if (family == syscall.AF_INET) 
        return "inet4";
    else if (family == syscall.AF_INET6) 
        return "inet6";
    else if (family == syscall.AF_UNIX) 
        return "local";
    else 
        return fmt.Sprintf("%d", family);
    
}

private static @string typeString(nint sotype) {
    @string s = default;

    if (sotype & 0xff == syscall.SOCK_STREAM) 
        s = "stream";
    else if (sotype & 0xff == syscall.SOCK_DGRAM) 
        s = "datagram";
    else if (sotype & 0xff == syscall.SOCK_RAW) 
        s = "raw";
    else if (sotype & 0xff == syscall.SOCK_SEQPACKET) 
        s = "seqpacket";
    else 
        s = fmt.Sprintf("%d", sotype & 0xff);
        {
        var flags = uint(sotype) & ~uint(0xff);

        if (flags != 0) {
            s += fmt.Sprintf("|%#x", flags);
        }
    }

    return s;

}

private static @string protocolString(nint proto) {

    if (proto == 0) 
        return "default";
    else if (proto == syscall.IPPROTO_TCP) 
        return "tcp";
    else if (proto == syscall.IPPROTO_UDP) 
        return "udp";
    else 
        return fmt.Sprintf("%d", proto);
    
}

} // end socktest_package
