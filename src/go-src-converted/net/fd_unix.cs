// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package net -- go2cs converted at 2022 March 06 22:15:44 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\fd_unix.go
using context = go.context_package;
using poll = go.@internal.poll_package;
using os = go.os_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using System;
using System.Threading;


namespace go;

public static partial class net_package {

private static readonly @string readSyscallName = "read";
private static readonly @string readFromSyscallName = "recvfrom";
private static readonly @string readMsgSyscallName = "recvmsg";
private static readonly @string writeSyscallName = "write";
private static readonly @string writeToSyscallName = "sendto";
private static readonly @string writeMsgSyscallName = "sendmsg";


private static (ptr<netFD>, error) newFD(nint sysfd, nint family, nint sotype, @string net) {
    ptr<netFD> _p0 = default!;
    error _p0 = default!;

    ptr<netFD> ret = addr(new netFD(pfd:poll.FD{Sysfd:sysfd,IsStream:sotype==syscall.SOCK_STREAM,ZeroReadIsEOF:sotype!=syscall.SOCK_DGRAM&&sotype!=syscall.SOCK_RAW,},family:family,sotype:sotype,net:net,));
    return (_addr_ret!, error.As(null!)!);
}

private static error init(this ptr<netFD> _addr_fd) {
    ref netFD fd = ref _addr_fd.val;

    return error.As(fd.pfd.Init(fd.net, true))!;
}

private static @string name(this ptr<netFD> _addr_fd) {
    ref netFD fd = ref _addr_fd.val;

    @string ls = default;    @string rs = default;

    if (fd.laddr != null) {
        ls = fd.laddr.String();
    }
    if (fd.raddr != null) {
        rs = fd.raddr.String();
    }
    return fd.net + ":" + ls + "->" + rs;

}

private static (syscall.Sockaddr, error) connect(this ptr<netFD> _addr_fd, context.Context ctx, syscall.Sockaddr la, syscall.Sockaddr ra) => func((defer, _, _) => {
    syscall.Sockaddr rsa = default;
    error ret = default!;
    ref netFD fd = ref _addr_fd.val;
 
    // Do not need to call fd.writeLock here,
    // because fd is not yet accessible to user,
    // so no concurrent operations are possible.
    {
        var err__prev1 = err;

        var err = connectFunc(fd.pfd.Sysfd, ra);


        if (err == syscall.EINPROGRESS || err == syscall.EALREADY || err == syscall.EINTR)
        {
            goto __switch_break0;
        }
        if (err == null || err == syscall.EISCONN)
        {
            return (null, error.As(mapErr(ctx.Err()))!);
            {
                var err__prev1 = err;

                err = fd.pfd.Init(fd.net, true);

                if (err != null) {
                    return (null, error.As(err)!);
                }

                err = err__prev1;

            }

            runtime.KeepAlive(fd);
            return (null, error.As(null!)!);
            goto __switch_break0;
        }
        if (err == syscall.EINVAL) 
        {
            // On Solaris and illumos we can see EINVAL if the socket has
            // already been accepted and closed by the server.  Treat this
            // as a successful connection--writes to the socket will see
            // EOF.  For details and a test case in C see
            // https://golang.org/issue/6828.
            if (runtime.GOOS == "solaris" || runtime.GOOS == "illumos") {
                return (null, error.As(null!)!);
            }
        }
        // default: 
            return (null, error.As(os.NewSyscallError("connect", err))!);

        __switch_break0:;

        err = err__prev1;
    }
    {
        var err__prev1 = err;

        err = fd.pfd.Init(fd.net, true);

        if (err != null) {
            return (null, error.As(err)!);
        }
        err = err__prev1;

    }

    {
        var (deadline, hasDeadline) = ctx.Deadline();

        if (hasDeadline) {
            fd.pfd.SetWriteDeadline(deadline);
            defer(fd.pfd.SetWriteDeadline(noDeadline));
        }
    } 

    // Start the "interrupter" goroutine, if this context might be canceled.
    // (The background context cannot)
    //
    // The interrupter goroutine waits for the context to be done and
    // interrupts the dial (by altering the fd's write deadline, which
    // wakes up waitWrite).
    if (ctx != context.Background()) { 
        // Wait for the interrupter goroutine to exit before returning
        // from connect.
        var done = make_channel<object>();
        var interruptRes = make_channel<error>();
        defer(() => {
            close(done);
            {
                var ctxErr = interruptRes.Receive();

                if (ctxErr != null && ret == null) { 
                    // The interrupter goroutine called SetWriteDeadline,
                    // but the connect code below had returned from
                    // waitWrite already and did a successful connect (ret
                    // == nil). Because we've now poisoned the connection
                    // by making it unwritable, don't return a successful
                    // dial. This was issue 16523.
                    ret = mapErr(ctxErr);
                    fd.Close(); // prevent a leak
                }

            }

        }());
        go_(() => () => {
            fd.pfd.SetWriteDeadline(aLongTimeAgo);
            testHookCanceledDial();
            interruptRes.Send(ctx.Err());
            interruptRes.Send(null);
        }());

    }
    while (true) { 
        // Performing multiple connect system calls on a
        // non-blocking socket under Unix variants does not
        // necessarily result in earlier errors being
        // returned. Instead, once runtime-integrated network
        // poller tells us that the socket is ready, get the
        // SO_ERROR socket option to see if the connection
        // succeeded or failed. See issue 7474 for further
        // details.
        {
            var err__prev1 = err;

            err = fd.pfd.WaitWrite();

            if (err != null) {
                return (null, error.As(mapErr(ctx.Err()))!);
                return (null, error.As(err)!);
            }

            err = err__prev1;

        }

        var (nerr, err) = getsockoptIntFunc(fd.pfd.Sysfd, syscall.SOL_SOCKET, syscall.SO_ERROR);
        if (err != null) {
            return (null, error.As(os.NewSyscallError("getsockopt", err))!);
        }
        {
            var err__prev1 = err;

            err = syscall.Errno(nerr);


            if (err == syscall.EINPROGRESS || err == syscall.EALREADY || err == syscall.EINTR)             else if (err == syscall.EISCONN) 
                return (null, error.As(null!)!);
            else if (err == syscall.Errno(0)) 
                // The runtime poller can wake us up spuriously;
                // see issues 14548 and 19289. Check that we are
                // really connected; if not, wait again.
                {
                    var err__prev1 = err;

                    var (rsa, err) = syscall.Getpeername(fd.pfd.Sysfd);

                    if (err == null) {
                        return (rsa, error.As(null!)!);
                    }

                    err = err__prev1;

                }

            else 
                return (null, error.As(os.NewSyscallError("connect", err))!);


            err = err__prev1;
        }
        runtime.KeepAlive(fd);

    }

});

private static (ptr<netFD>, error) accept(this ptr<netFD> _addr_fd) {
    ptr<netFD> netfd = default!;
    error err = default!;
    ref netFD fd = ref _addr_fd.val;

    var (d, rsa, errcall, err) = fd.pfd.Accept();
    if (err != null) {
        if (errcall != "") {
            err = wrapSyscallError(errcall, err);
        }
        return (_addr_null!, error.As(err)!);

    }
    netfd, err = newFD(d, fd.family, fd.sotype, fd.net);

    if (err != null) {
        poll.CloseFunc(d);
        return (_addr_null!, error.As(err)!);
    }
    err = netfd.init();

    if (err != null) {
        netfd.Close();
        return (_addr_null!, error.As(err)!);
    }
    var (lsa, _) = syscall.Getsockname(netfd.pfd.Sysfd);
    netfd.setAddr(netfd.addrFunc()(lsa), netfd.addrFunc()(rsa));
    return (_addr_netfd!, error.As(null!)!);

}

private static (ptr<os.File>, error) dup(this ptr<netFD> _addr_fd) {
    ptr<os.File> f = default!;
    error err = default!;
    ref netFD fd = ref _addr_fd.val;

    var (ns, call, err) = fd.pfd.Dup();
    if (err != null) {
        if (call != "") {
            err = os.NewSyscallError(call, err);
        }
        return (_addr_null!, error.As(err)!);

    }
    return (_addr_os.NewFile(uintptr(ns), fd.name())!, error.As(null!)!);

}

} // end net_package
