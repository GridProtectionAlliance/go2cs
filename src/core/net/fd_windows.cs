// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using poll = @internal.poll_package;
using windows = @internal.syscall.windows_package;
using os = os_package;
using runtime = runtime_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.syscall;

partial class net_package {

internal static readonly @string readSyscallName = "wsarecv"u8;
internal static readonly @string readFromSyscallName = "wsarecvfrom"u8;
internal static readonly @string readMsgSyscallName = "wsarecvmsg"u8;
internal static readonly @string writeSyscallName = "wsasend"u8;
internal static readonly @string writeToSyscallName = "wsasendto"u8;
internal static readonly @string writeMsgSyscallName = "wsasendmsg"u8;

[GoInit] internal static void init() {
    poll.InitWSA();
}

// canUseConnectEx reports whether we can use the ConnectEx Windows API call
// for the given network type.
internal static bool canUseConnectEx(@string net) {
    var exprᴛ1 = net;
    if (exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8) {
        return true;
    }

    // ConnectEx windows API does not support connectionless sockets.
    return false;
}

internal static (ж<netFD>, error) newFD(syscallꓸHandle sysfd, nint family, nint sotype, @string net) {
    var ret = Ꮡ(new netFD(
        pfd: new poll.FD(
            Sysfd: sysfd,
            IsStream: sotype == syscall.SOCK_STREAM,
            ZeroReadIsEOF: sotype != syscall.SOCK_DGRAM && sotype != syscall.SOCK_RAW
        ),
        family: family,
        sotype: sotype,
        net: net
    ));
    return (ret, default!);
}

[GoRecv] internal static error init(this ref netFD fd) {
    var (errcall, err) = fd.pfd.Init(fd.net, true);
    if (errcall != ""u8) {
        err = wrapSyscallError(errcall, err);
    }
    return err;
}

// Always returns nil for connected peer address result.
[GoRecv] internal static (syscallꓸSockaddr, error) connect(this ref netFD fd, context.Context ctx, syscallꓸSockaddr la, syscallꓸSockaddr ra) => func((defer, _) => {
    // Do not need to call fd.writeLock here,
    // because fd is not yet accessible to user,
    // so no concurrent operations are possible.
    {
        var err = fd.init(); if (err != default!) {
            return (default!, err);
        }
    }
    if (ctx.Done() != default!) {
        // Propagate the Context's deadline and cancellation.
        // If the context is already done, or if it has a nonzero deadline,
        // ensure that that is applied before the call to ConnectEx begins
        // so that we don't return spurious connections.
        deferǃ(fd.pfd.SetWriteDeadline, noDeadline, defer);
        if (ctx.Err() != default!){
            fd.pfd.SetWriteDeadline(aLongTimeAgo);
        } else {
            {
                var (deadline, ok) = ctx.Deadline(); if (ok && !deadline.IsZero()) {
                    fd.pfd.SetWriteDeadline(deadline);
                }
            }
            var done = new channel<struct{}>(1);
            var stop = context.AfterFunc(ctx, 
            var aLongTimeAgoʗ1 = aLongTimeAgo;
            var doneʗ1 = done;
            () => {
                // Force the runtime's poller to immediately give
                // up waiting for writability.
                fd.pfd.SetWriteDeadline(aLongTimeAgoʗ1);
                close(doneʗ1);
            });
            var doneʗ3 = done;
            var stopʗ1 = stop;
            defer(() => {
                if (!stopʗ1()) {
                    // Wait for the call to SetWriteDeadline to complete so that we can
                    // reset the deadline if everything else succeeded.
                    ᐸꟷ(doneʗ3);
                }
            });
        }
    }
    if (!canUseConnectEx(fd.net)) {
        var err = connectFunc(fd.pfd.Sysfd, ra);
        return (default!, os.NewSyscallError("connect"u8, err));
    }
    // ConnectEx windows API requires an unconnected, previously bound socket.
    if (la == default!) {
        switch (ra.type()) {
        case ж<syscall.SockaddrInet4> : {
            Ꮡla = new syscall.SockaddrInet4(nil); la = ref Ꮡla.val;
            break;
        }
        case ж<syscall.SockaddrInet6> : {
            Ꮡla = new syscall.SockaddrInet6(nil); la = ref Ꮡla.val;
            break;
        }
        default: {

            throw panic("unexpected type in connect");
            break;
        }}

        {
            var err = syscall.Bind(fd.pfd.Sysfd, la); if (err != default!) {
                return (default!, os.NewSyscallError("bind"u8, err));
            }
        }
    }
    bool isloopback = default!;
    switch (ra.type()) {
    case ж<syscall.SockaddrInet4> ra: {
        isloopback = (~ra).Addr[0] == 127;
        break;
    }
    case ж<syscall.SockaddrInet6> ra: {
        isloopback = (~ra).Addr == array<byte>(IPv6loopback);
        break;
    }
    default: {
        var ra = ra.type();
        throw panic("unexpected type in connect");
        break;
    }}
    if (isloopback) {
        // This makes ConnectEx() fails faster if the target port on the localhost
        // is not reachable, instead of waiting for 2s.
        ref var params = ref heap<@internal.syscall.windows_package.TCP_INITIAL_RTO_PARAMETERS>(out var Ꮡparams);
        @params = new windows.TCP_INITIAL_RTO_PARAMETERS(
            Rtt: windows.TCP_INITIAL_RTO_UNSPECIFIED_RTT, // use the default or overridden by the Administrator

            MaxSynRetransmissions: 1
        );
        // minimum possible value before Windows 10.0.16299
        if (windows.SupportTCPInitialRTONoSYNRetransmissions()) {
            // In Windows 10.0.16299 TCP_INITIAL_RTO_NO_SYN_RETRANSMISSIONS makes ConnectEx() fails instantly.
            @params.MaxSynRetransmissions = windows.TCP_INITIAL_RTO_NO_SYN_RETRANSMISSIONS;
        }
        ref var out = ref heap(new uint32(), out var Ꮡout);
        // Don't abort the connection if WSAIoctl fails, as it is only an optimization.
        // If it fails reliably, we expect TestDialClosedPortFailFast to detect it.
        _ = fd.pfd.WSAIoctl(windows.SIO_TCP_INITIAL_RTO, (ж<byte>)(uintptr)(new @unsafe.Pointer(Ꮡ@params)), ((uint32)@unsafe.Sizeof(@params)), nil, 0, Ꮡ@out, nil, 0);
    }
    // Call ConnectEx API.
    {
        var err = fd.pfd.ConnectEx(ra); if (err != default!) {
            switch (ᐧ) {
            case ᐧ when ctx.Done().ꟷᐳ(out _): {
                return (default!, mapErr(ctx.Err()));
            }
            default: {
                {
                    var (_, ok) = err._<syscall.Errno>(ᐧ); if (ok) {
                        err = os.NewSyscallError("connectex"u8, err);
                    }
                }
                return (default!, err);
            }}
        }
    }
    // Refresh socket properties.
    return (default!, os.NewSyscallError("setsockopt"u8, syscall.Setsockopt(fd.pfd.Sysfd, syscall.SOL_SOCKET, syscall.SO_UPDATE_CONNECT_CONTEXT, (ж<byte>)(uintptr)(((@unsafe.Pointer)(Ꮡfd.pfd.of(poll.FD.ᏑSysfd)))), ((int32)@unsafe.Sizeof(fd.pfd.Sysfd)))));
});

[GoRecv] internal static (int64, error) writeBuffers(this ref conn c, ж<Buffers> Ꮡv) {
    ref var v = ref Ꮡv.val;

    if (!c.ok()) {
        return (0, syscall.EINVAL);
    }
    var (n, err) = c.fd.writeBuffers(Ꮡv);
    if (err != default!) {
        return (n, new OpError(Op: "wsasend"u8, Net: c.fd.net, Source: c.fd.laddr, ΔAddr: c.fd.raddr, Err: err));
    }
    return (n, default!);
}

[GoRecv] internal static (int64, error) writeBuffers(this ref netFD fd, ж<Buffers> Ꮡbuf) {
    ref var buf = ref Ꮡbuf.val;

    var (n, err) = fd.pfd.Writev((ж<slice<slice<byte>>>)(buf));
    runtime.KeepAlive(fd);
    return (n, wrapSyscallError("wsasend"u8, err));
}

[GoRecv] internal static (ж<netFD>, error) accept(this ref netFD fd) {
    var (s, rawsa, rsan, errcall, err) = fd.pfd.Accept(() => sysSocket(fd.family, fd.sotype, 0));
    if (err != default!) {
        if (errcall != ""u8) {
            err = wrapSyscallError(errcall, err);
        }
        return (default!, err);
    }
    // Associate our new socket with IOCP.
    (netfd, err) = newFD(s, fd.family, fd.sotype, fd.net);
    if (err != default!) {
        poll.CloseFunc(s);
        return (default!, err);
    }
    {
        var errΔ1 = netfd.init(); if (errΔ1 != default!) {
            fd.Close();
            return (default!, errΔ1);
        }
    }
    // Get local and peer addr out of AcceptEx buffer.
    ж<syscall.RawSockaddrAny> lrsa = default!;
    ж<syscall.RawSockaddrAny> rrsa = default!;
    ref var llen = ref heap(new int32(), out var Ꮡllen);
    ref var rlen = ref heap(new int32(), out var Ꮡrlen);
    syscall.GetAcceptExSockaddrs((ж<byte>)(uintptr)(new @unsafe.Pointer(Ꮡ(rawsa, 0))),
        0, rsan, rsan, Ꮡ(lrsa), Ꮡllen, Ꮡ(rrsa), Ꮡrlen);
    (lsa, _) = lrsa.Sockaddr();
    (rsa, _) = rrsa.Sockaddr();
    netfd.setAddr(netfd.addrFunc()(lsa), netfd.addrFunc()(rsa));
    return (netfd, default!);
}

// Unimplemented functions.
[GoRecv] internal static (ж<os.File>, error) dup(this ref netFD fd) {
    // TODO: Implement this, perhaps using internal/poll.DupCloseOnExec.
    return (default!, syscall.EWINDOWS);
}

} // end net_package
