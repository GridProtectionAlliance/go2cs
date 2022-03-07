// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:15:45 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\fd_windows.go
using context = go.context_package;
using poll = go.@internal.poll_package;
using os = go.os_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using System;
using System.Threading;


namespace go;

public static partial class net_package {

private static readonly @string readSyscallName = "wsarecv";
private static readonly @string readFromSyscallName = "wsarecvfrom";
private static readonly @string readMsgSyscallName = "wsarecvmsg";
private static readonly @string writeSyscallName = "wsasend";
private static readonly @string writeToSyscallName = "wsasendto";
private static readonly @string writeMsgSyscallName = "wsasendmsg";


// canUseConnectEx reports whether we can use the ConnectEx Windows API call
// for the given network type.
private static bool canUseConnectEx(@string net) {
    switch (net) {
        case "tcp": 

        case "tcp4": 

        case "tcp6": 
            return true;
            break;
    } 
    // ConnectEx windows API does not support connectionless sockets.
    return false;

}

private static (ptr<netFD>, error) newFD(syscall.Handle sysfd, nint family, nint sotype, @string net) {
    ptr<netFD> _p0 = default!;
    error _p0 = default!;

    ptr<netFD> ret = addr(new netFD(pfd:poll.FD{Sysfd:sysfd,IsStream:sotype==syscall.SOCK_STREAM,ZeroReadIsEOF:sotype!=syscall.SOCK_DGRAM&&sotype!=syscall.SOCK_RAW,},family:family,sotype:sotype,net:net,));
    return (_addr_ret!, error.As(null!)!);
}

private static error init(this ptr<netFD> _addr_fd) {
    ref netFD fd = ref _addr_fd.val;

    var (errcall, err) = fd.pfd.Init(fd.net, true);
    if (errcall != "") {
        err = wrapSyscallError(errcall, err);
    }
    return error.As(err)!;

}

// Always returns nil for connected peer address result.
private static (syscall.Sockaddr, error) connect(this ptr<netFD> _addr_fd, context.Context ctx, syscall.Sockaddr la, syscall.Sockaddr ra) => func((defer, panic, _) => {
    syscall.Sockaddr _p0 = default;
    error _p0 = default!;
    ref netFD fd = ref _addr_fd.val;
 
    // Do not need to call fd.writeLock here,
    // because fd is not yet accessible to user,
    // so no concurrent operations are possible.
    {
        var err__prev1 = err;

        var err = fd.init();

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }

    {
        var (deadline, ok) = ctx.Deadline();

        if (ok && !deadline.IsZero()) {
            fd.pfd.SetWriteDeadline(deadline);
            defer(fd.pfd.SetWriteDeadline(noDeadline));
        }
    }

    if (!canUseConnectEx(fd.net)) {
        err = connectFunc(fd.pfd.Sysfd, ra);
        return (null, error.As(os.NewSyscallError("connect", err))!);
    }
    if (la == null) {
        switch (ra.type()) {
            case ptr<syscall.SockaddrInet4> _:
                la = addr(new syscall.SockaddrInet4());
                break;
            case ptr<syscall.SockaddrInet6> _:
                la = addr(new syscall.SockaddrInet6());
                break;
            default:
            {
                panic("unexpected type in connect");
                break;
            }
        }
        {
            var err__prev2 = err;

            err = syscall.Bind(fd.pfd.Sysfd, la);

            if (err != null) {
                return (null, error.As(os.NewSyscallError("bind", err))!);
            }

            err = err__prev2;

        }

    }
    var done = make_channel<bool>(); // must be unbuffered
    defer(() => {
        done.Send(true);
    }());
    go_(() => () => {
        fd.pfd.SetWriteDeadline(aLongTimeAgo).Send(done);
    }()); 

    // Call ConnectEx API.
    {
        var err__prev1 = err;

        err = fd.pfd.ConnectEx(ra);

        if (err != null) {
            return (null, error.As(mapErr(ctx.Err()))!);
            {
                syscall.Errno (_, ok) = err._<syscall.Errno>();

                if (ok) {
                    err = os.NewSyscallError("connectex", err);
                }

            }

            return (null, error.As(err)!);

        }
        err = err__prev1;

    } 
    // Refresh socket properties.
    return (null, error.As(os.NewSyscallError("setsockopt", syscall.Setsockopt(fd.pfd.Sysfd, syscall.SOL_SOCKET, syscall.SO_UPDATE_CONNECT_CONTEXT, (byte.val)(@unsafe.Pointer(_addr_fd.pfd.Sysfd)), int32(@unsafe.Sizeof(fd.pfd.Sysfd)))))!);

});

private static (long, error) writeBuffers(this ptr<conn> _addr_c, ptr<Buffers> _addr_v) {
    long _p0 = default;
    error _p0 = default!;
    ref conn c = ref _addr_c.val;
    ref Buffers v = ref _addr_v.val;

    if (!c.ok()) {
        return (0, error.As(syscall.EINVAL)!);
    }
    var (n, err) = c.fd.writeBuffers(v);
    if (err != null) {
        return (n, error.As(addr(new OpError(Op:"wsasend",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err))!)!);
    }
    return (n, error.As(null!)!);

}

private static (long, error) writeBuffers(this ptr<netFD> _addr_fd, ptr<Buffers> _addr_buf) {
    long _p0 = default;
    error _p0 = default!;
    ref netFD fd = ref _addr_fd.val;
    ref Buffers buf = ref _addr_buf.val;

    var (n, err) = fd.pfd.Writev(new ptr<ptr<slice<slice<byte>>>>(buf));
    runtime.KeepAlive(fd);
    return (n, error.As(wrapSyscallError("wsasend", err))!);
}

private static (ptr<netFD>, error) accept(this ptr<netFD> _addr_fd) {
    ptr<netFD> _p0 = default!;
    error _p0 = default!;
    ref netFD fd = ref _addr_fd.val;

    var (s, rawsa, rsan, errcall, err) = fd.pfd.Accept(() => {
        return _addr_sysSocket(fd.family, fd.sotype, 0)!;
    });

    if (err != null) {
        if (errcall != "") {
            err = wrapSyscallError(errcall, err);
        }
        return (_addr_null!, error.As(err)!);

    }
    var (netfd, err) = newFD(s, fd.family, fd.sotype, fd.net);
    if (err != null) {
        poll.CloseFunc(s);
        return (_addr_null!, error.As(err)!);
    }
    {
        var err = netfd.init();

        if (err != null) {
            fd.Close();
            return (_addr_null!, error.As(err)!);
        }
    } 

    // Get local and peer addr out of AcceptEx buffer.
    ptr<syscall.RawSockaddrAny> lrsa;    ptr<syscall.RawSockaddrAny> rrsa;

    ref int llen = ref heap(out ptr<int> _addr_llen);    ref int rlen = ref heap(out ptr<int> _addr_rlen);

    syscall.GetAcceptExSockaddrs((byte.val)(@unsafe.Pointer(_addr_rawsa[0])), 0, rsan, rsan, _addr_lrsa, _addr_llen, _addr_rrsa, _addr_rlen);
    var (lsa, _) = lrsa.Sockaddr();
    var (rsa, _) = rrsa.Sockaddr();

    netfd.setAddr(netfd.addrFunc()(lsa), netfd.addrFunc()(rsa));
    return (_addr_netfd!, error.As(null!)!);

}

// Unimplemented functions.

private static (ptr<os.File>, error) dup(this ptr<netFD> _addr_fd) {
    ptr<os.File> _p0 = default!;
    error _p0 = default!;
    ref netFD fd = ref _addr_fd.val;
 
    // TODO: Implement this
    return (_addr_null!, error.As(syscall.EWINDOWS)!);

}

} // end net_package
