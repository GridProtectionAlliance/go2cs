// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:16:40 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\sock_linux.go
using syscall = go.syscall_package;

namespace go;

public static partial class net_package {

private static (nint, nint) kernelVersion() {
    nint major = default;
    nint minor = default;

    ref syscall.Utsname uname = ref heap(out ptr<syscall.Utsname> _addr_uname);
    {
        var err = syscall.Uname(_addr_uname);

        if (err != null) {
            return ;
        }
    }


    var rl = uname.Release;
    array<nint> values = new array<nint>(2);
    nint vi = 0;
    nint value = 0;
    foreach (var (_, c) in rl) {
        if (c >= '0' && c <= '9') {
            value = (value * 10) + int(c - '0');
        }
        else
 { 
            // Note that we're assuming N.N.N here.  If we see anything else we are likely to
            // mis-parse it.
            values[vi] = value;
            vi++;
            if (vi >= len(values)) {
                break;
            }
            value = 0;

        }
    }    switch (vi) {
        case 0: 
            return (0, 0);
            break;
        case 1: 
            return (values[0], 0);
            break;
        case 2: 
            return (values[0], values[1]);
            break;
    }
    return ;

}

// Linux stores the backlog as:
//
//  - uint16 in kernel version < 4.1,
//  - uint32 in kernel version >= 4.1
//
// Truncate number to avoid wrapping.
//
// See issue 5030 and 41470.
private static nint maxAckBacklog(nint n) {
    var (major, minor) = kernelVersion();
    nint size = 16;
    if (major > 4 || (major == 4 && minor >= 1)) {
        size = 32;
    }
    nuint max = 1 << (int)(size) - 1;
    if (uint(n) > max) {
        n = int(max);
    }
    return n;

}

private static nint maxListenerBacklog() => func((defer, _, _) => {
    var (fd, err) = open("/proc/sys/net/core/somaxconn");
    if (err != null) {
        return syscall.SOMAXCONN;
    }
    defer(fd.close());
    var (l, ok) = fd.readLine();
    if (!ok) {
        return syscall.SOMAXCONN;
    }
    var f = getFields(l);
    var (n, _, ok) = dtoi(f[0]);
    if (n == 0 || !ok) {
        return syscall.SOMAXCONN;
    }
    if (n > 1 << 16 - 1) {
        return maxAckBacklog(n);
    }
    return n;

});

} // end net_package
