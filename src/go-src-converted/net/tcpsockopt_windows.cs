// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:16:48 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\tcpsockopt_windows.go
using os = go.os_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using time = go.time_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class net_package {

private static error setKeepAlivePeriod(ptr<netFD> _addr_fd, time.Duration d) {
    ref netFD fd = ref _addr_fd.val;
 
    // The kernel expects milliseconds so round to next highest
    // millisecond.
    var msecs = uint32(roundDurationUp(d, time.Millisecond));
    ref syscall.TCPKeepalive ka = ref heap(new syscall.TCPKeepalive(OnOff:1,Time:msecs,Interval:msecs,), out ptr<syscall.TCPKeepalive> _addr_ka);
    ref var ret = ref heap(uint32(0), out ptr<var> _addr_ret);
    var size = uint32(@unsafe.Sizeof(ka));
    var err = fd.pfd.WSAIoctl(syscall.SIO_KEEPALIVE_VALS, (byte.val)(@unsafe.Pointer(_addr_ka)), size, null, 0, _addr_ret, null, 0);
    runtime.KeepAlive(fd);
    return error.As(os.NewSyscallError("wsaioctl", err))!;

}

} // end net_package
