// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris || windows
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris windows

// package net -- go2cs converted at 2022 March 13 05:30:07 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\sock_posix.go
namespace go;

using context = context_package;
using poll = @internal.poll_package;
using os = os_package;
using syscall = syscall_package;


// socket returns a network file descriptor that is ready for
// asynchronous I/O using the network poller.

using System;
public static partial class net_package {

private static (ptr<netFD>, error) socket(context.Context ctx, @string net, nint family, nint sotype, nint proto, bool ipv6only, sockaddr laddr, sockaddr raddr, Func<@string, @string, syscall.RawConn, error> ctrlFn) {
    ptr<netFD> fd = default!;
    error err = default!;

    var (s, err) = sysSocket(family, sotype, proto);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    err = setDefaultSockopts(s, family, sotype, ipv6only);

    if (err != null) {
        poll.CloseFunc(s);
        return (_addr_null!, error.As(err)!);
    }
    fd, err = newFD(s, family, sotype, net);

    if (err != null) {
        poll.CloseFunc(s);
        return (_addr_null!, error.As(err)!);
    }
    if (laddr != null && raddr == null) {

        if (sotype == syscall.SOCK_STREAM || sotype == syscall.SOCK_SEQPACKET) 
            {
                var err__prev2 = err;

                var err = fd.listenStream(laddr, listenerBacklog(), ctrlFn);

                if (err != null) {
                    fd.Close();
                    return (_addr_null!, error.As(err)!);
                }
                err = err__prev2;

            }
            return (_addr_fd!, error.As(null!)!);
        else if (sotype == syscall.SOCK_DGRAM) 
            {
                var err__prev2 = err;

                err = fd.listenDatagram(laddr, ctrlFn);

                if (err != null) {
                    fd.Close();
                    return (_addr_null!, error.As(err)!);
                }
                err = err__prev2;

            }
            return (_addr_fd!, error.As(null!)!);
            }
    {
        var err__prev1 = err;

        err = fd.dial(ctx, laddr, raddr, ctrlFn);

        if (err != null) {
            fd.Close();
            return (_addr_null!, error.As(err)!);
        }
        err = err__prev1;

    }
    return (_addr_fd!, error.As(null!)!);
}

private static @string ctrlNetwork(this ptr<netFD> _addr_fd) {
    ref netFD fd = ref _addr_fd.val;

    switch (fd.net) {
        case "unix": 

        case "unixgram": 

        case "unixpacket": 
            return fd.net;
            break;
    }
    switch (fd.net[len(fd.net) - 1]) {
        case '4': 

        case '6': 
            return fd.net;
            break;
    }
    if (fd.family == syscall.AF_INET) {
        return fd.net + "4";
    }
    return fd.net + "6";
}

private static Func<syscall.Sockaddr, Addr> addrFunc(this ptr<netFD> _addr_fd) {
    ref netFD fd = ref _addr_fd.val;


    if (fd.family == syscall.AF_INET || fd.family == syscall.AF_INET6) 

        if (fd.sotype == syscall.SOCK_STREAM) 
            return sockaddrToTCP;
        else if (fd.sotype == syscall.SOCK_DGRAM) 
            return sockaddrToUDP;
        else if (fd.sotype == syscall.SOCK_RAW) 
            return sockaddrToIP;
            else if (fd.family == syscall.AF_UNIX) 

        if (fd.sotype == syscall.SOCK_STREAM) 
            return sockaddrToUnix;
        else if (fd.sotype == syscall.SOCK_DGRAM) 
            return sockaddrToUnixgram;
        else if (fd.sotype == syscall.SOCK_SEQPACKET) 
            return sockaddrToUnixpacket;
                return _p0 => null;
}

private static error dial(this ptr<netFD> _addr_fd, context.Context ctx, sockaddr laddr, sockaddr raddr, Func<@string, @string, syscall.RawConn, error> ctrlFn) {
    ref netFD fd = ref _addr_fd.val;

    if (ctrlFn != null) {
        var (c, err) = newRawConn(fd);
        if (err != null) {
            return error.As(err)!;
        }
        @string ctrlAddr = default;
        if (raddr != null) {
            ctrlAddr = raddr.String();
        }
        else if (laddr != null) {
            ctrlAddr = laddr.String();
        }
        {
            var err__prev2 = err;

            var err = ctrlFn(fd.ctrlNetwork(), ctrlAddr, c);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
    }
    err = default!;
    syscall.Sockaddr lsa = default;
    if (laddr != null) {
        lsa, err = laddr.sockaddr(fd.family);

        if (err != null) {
            return error.As(err)!;
        }
        else if (lsa != null) {
            err = syscall.Bind(fd.pfd.Sysfd, lsa);

            if (err != null) {
                return error.As(os.NewSyscallError("bind", err))!;
            }
        }
    }
    syscall.Sockaddr rsa = default; // remote address from the user
    syscall.Sockaddr crsa = default; // remote address we actually connected to
    if (raddr != null) {
        rsa, err = raddr.sockaddr(fd.family);

        if (err != null) {
            return error.As(err)!;
        }
        crsa, err = fd.connect(ctx, lsa, rsa);

        if (err != null) {
            return error.As(err)!;
        }
        fd.isConnected = true;
    }
    else
 {
        {
            var err__prev2 = err;

            err = fd.init();

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
    }
    lsa, _ = syscall.Getsockname(fd.pfd.Sysfd);
    if (crsa != null) {
        fd.setAddr(fd.addrFunc()(lsa), fd.addrFunc()(crsa));
    }    rsa, _ = syscall.Getpeername(fd.pfd.Sysfd);


    else if (rsa != null) {
        fd.setAddr(fd.addrFunc()(lsa), fd.addrFunc()(rsa));
    }
    else
 {
        fd.setAddr(fd.addrFunc()(lsa), raddr);
    }
    return error.As(null!)!;
}

private static error listenStream(this ptr<netFD> _addr_fd, sockaddr laddr, nint backlog, Func<@string, @string, syscall.RawConn, error> ctrlFn) {
    ref netFD fd = ref _addr_fd.val;

    error err = default!;
    err = error.As(setDefaultListenerSockopts(fd.pfd.Sysfd))!;

    if (err != null) {
        return error.As(err)!;
    }
    syscall.Sockaddr lsa = default;
    lsa, err = laddr.sockaddr(fd.family);

    if (err != null) {
        return error.As(err)!;
    }
    if (ctrlFn != null) {
        var (c, err) = newRawConn(fd);
        if (err != null) {
            return error.As(err)!;
        }
        {
            error err__prev2 = err;

            err = ctrlFn(fd.ctrlNetwork(), laddr.String(), c);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
    }
    err = error.As(syscall.Bind(fd.pfd.Sysfd, lsa))!;

    if (err != null) {
        return error.As(os.NewSyscallError("bind", err))!;
    }
    err = error.As(listenFunc(fd.pfd.Sysfd, backlog))!;

    if (err != null) {
        return error.As(os.NewSyscallError("listen", err))!;
    }
    err = error.As(fd.init())!;

    if (err != null) {
        return error.As(err)!;
    }
    lsa, _ = syscall.Getsockname(fd.pfd.Sysfd);
    fd.setAddr(fd.addrFunc()(lsa), null);
    return error.As(null!)!;
}

private static error listenDatagram(this ptr<netFD> _addr_fd, sockaddr laddr, Func<@string, @string, syscall.RawConn, error> ctrlFn) {
    ref netFD fd = ref _addr_fd.val;

    switch (laddr.type()) {
        case ptr<UDPAddr> addr:
            if (addr.IP != null && addr.IP.IsMulticast()) {
                {
                    var err__prev2 = err;

                    var err = setDefaultMulticastSockopts(fd.pfd.Sysfd);

                    if (err != null) {
                        return error.As(err)!;
                    }

                    err = err__prev2;

                }
                ref var addr = ref heap(addr.val, out ptr<var> _addr_addr);

                if (fd.family == syscall.AF_INET) 
                    addr.IP = IPv4zero;
                else if (fd.family == syscall.AF_INET6) 
                    addr.IP = IPv6unspecified;
                                _addr_laddr = _addr_addr;
                laddr = ref _addr_laddr.val;
            }
            break;
    }
    err = default!;
    syscall.Sockaddr lsa = default;
    lsa, err = laddr.sockaddr(fd.family);

    if (err != null) {
        return error.As(err)!;
    }
    if (ctrlFn != null) {
        var (c, err) = newRawConn(fd);
        if (err != null) {
            return error.As(err)!;
        }
        {
            var err__prev2 = err;

            err = ctrlFn(fd.ctrlNetwork(), laddr.String(), c);

            if (err != null) {
                return error.As(err)!;
            }

            err = err__prev2;

        }
    }
    err = syscall.Bind(fd.pfd.Sysfd, lsa);

    if (err != null) {
        return error.As(os.NewSyscallError("bind", err))!;
    }
    err = fd.init();

    if (err != null) {
        return error.As(err)!;
    }
    lsa, _ = syscall.Getsockname(fd.pfd.Sysfd);
    fd.setAddr(fd.addrFunc()(lsa), null);
    return error.As(null!)!;
}

} // end net_package
