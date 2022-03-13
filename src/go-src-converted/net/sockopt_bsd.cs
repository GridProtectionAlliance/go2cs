// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin || dragonfly || freebsd || netbsd || openbsd
// +build darwin dragonfly freebsd netbsd openbsd

// package net -- go2cs converted at 2022 March 13 05:30:06 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\sockopt_bsd.go
namespace go;

using os = os_package;
using runtime = runtime_package;
using syscall = syscall_package;

public static partial class net_package {

private static error setDefaultSockopts(nint s, nint family, nint sotype, bool ipv6only) {
    if (runtime.GOOS == "dragonfly" && sotype != syscall.SOCK_RAW) { 
        // On DragonFly BSD, we adjust the ephemeral port
        // range because unlike other BSD systems its default
        // port range doesn't conform to IANA recommendation
        // as described in RFC 6056 and is pretty narrow.

        if (family == syscall.AF_INET) 
            syscall.SetsockoptInt(s, syscall.IPPROTO_IP, syscall.IP_PORTRANGE, syscall.IP_PORTRANGE_HIGH);
        else if (family == syscall.AF_INET6) 
            syscall.SetsockoptInt(s, syscall.IPPROTO_IPV6, syscall.IPV6_PORTRANGE, syscall.IPV6_PORTRANGE_HIGH);
            }
    if (family == syscall.AF_INET6 && sotype != syscall.SOCK_RAW && supportsIPv4map()) { 
        // Allow both IP versions even if the OS default
        // is otherwise. Note that some operating systems
        // never admit this option.
        syscall.SetsockoptInt(s, syscall.IPPROTO_IPV6, syscall.IPV6_V6ONLY, boolint(ipv6only));
    }
    if ((sotype == syscall.SOCK_DGRAM || sotype == syscall.SOCK_RAW) && family != syscall.AF_UNIX) { 
        // Allow broadcast.
        return error.As(os.NewSyscallError("setsockopt", syscall.SetsockoptInt(s, syscall.SOL_SOCKET, syscall.SO_BROADCAST, 1)))!;
    }
    return error.As(null!)!;
}

private static error setDefaultListenerSockopts(nint s) { 
    // Allow reuse of recently-used addresses.
    return error.As(os.NewSyscallError("setsockopt", syscall.SetsockoptInt(s, syscall.SOL_SOCKET, syscall.SO_REUSEADDR, 1)))!;
}

private static error setDefaultMulticastSockopts(nint s) { 
    // Allow multicast UDP and raw IP datagram sockets to listen
    // concurrently across multiple listeners.
    {
        var err = syscall.SetsockoptInt(s, syscall.SOL_SOCKET, syscall.SO_REUSEADDR, 1);

        if (err != null) {
            return error.As(os.NewSyscallError("setsockopt", err))!;
        }
    } 
    // Allow reuse of recently-used ports.
    // This option is supported only in descendants of 4.4BSD,
    // to make an effective multicast application that requires
    // quick draw possible.
    return error.As(os.NewSyscallError("setsockopt", syscall.SetsockoptInt(s, syscall.SOL_SOCKET, syscall.SO_REUSEPORT, 1)))!;
}

} // end net_package
