// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using windows = @internal.syscall.windows_package;
using syscall = syscall_package;
using @internal.syscall;

partial class net_package {

// SetKeepAliveConfig configures keep-alive messages sent by the operating system.
[GoRecv] public static error SetKeepAliveConfig(this ref TCPConn c, KeepAliveConfig config) {
    if (!c.ok()) {
        return syscall.EINVAL;
    }
    {
        var err = setKeepAlive(c.fd, config.Enable); if (err != default!) {
            return new OpError(Op: "set"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err);
        }
    }
    if (windows.SupportTCPKeepAliveIdle() && windows.SupportTCPKeepAliveInterval()){
        {
            var err = setKeepAliveIdle(c.fd, config.Idle); if (err != default!) {
                return new OpError(Op: "set"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err);
            }
        }
        {
            var err = setKeepAliveInterval(c.fd, config.Interval); if (err != default!) {
                return new OpError(Op: "set"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err);
            }
        }
    } else 
    {
        var err = setKeepAliveIdleAndInterval(c.fd, config.Idle, config.Interval); if (err != default!) {
            return new OpError(Op: "set"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err);
        }
    }
    {
        var err = setKeepAliveCount(c.fd, config.Count); if (err != default!) {
            return new OpError(Op: "set"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err);
        }
    }
    return default!;
}

} // end net_package
