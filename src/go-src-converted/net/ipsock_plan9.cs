// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:16:14 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\ipsock_plan9.go
using context = go.context_package;
using bytealg = go.@internal.bytealg_package;
using itoa = go.@internal.itoa_package;
using fs = go.io.fs_package;
using os = go.os_package;
using syscall = go.syscall_package;
using System;
using System.Threading;


namespace go;

public static partial class net_package {

    // probe probes IPv4, IPv6 and IPv4-mapped IPv6 communication
    // capabilities.
    //
    // Plan 9 uses IPv6 natively, see ip(3).
private static void probe(this ptr<ipStackCapabilities> _addr_p) {
    ref ipStackCapabilities p = ref _addr_p.val;

    p.ipv4Enabled = probe(netdir + "/iproute", "4i");
    p.ipv6Enabled = probe(netdir + "/iproute", "6i");
    if (p.ipv4Enabled && p.ipv6Enabled) {
        p.ipv4MappedIPv6Enabled = true;
    }
}

private static bool probe(@string filename, @string query) => func((defer, _, _) => {
    ptr<file> file;
    error err = default!;
    file, err = open(filename);

    if (err != null) {
        return false;
    }
    defer(file.close());

    var r = false;
    {
        var (line, ok) = file.readLine();

        while (ok && !r) {
            var f = getFields(line);
            if (len(f) < 3) {
                continue;
            line, ok = file.readLine();
            }

            for (nint i = 0; i < len(f); i++) {
                if (query == f[i]) {
                    r = true;
                    break;
                }
            }


        }
    }
    return r;

});

// parsePlan9Addr parses address of the form [ip!]port (e.g. 127.0.0.1!80).
private static (IP, nint, error) parsePlan9Addr(@string s) {
    IP ip = default;
    nint iport = default;
    error err = default!;

    var addr = IPv4zero; // address contains port only
    var i = bytealg.IndexByteString(s, '!');
    if (i >= 0) {
        addr = ParseIP(s[..(int)i]);
        if (addr == null) {
            return (null, 0, error.As(addr(new ParseError(Type:"IP address",Text:s))!)!);
        }
    }
    var (p, plen, ok) = dtoi(s[(int)i + 1..]);
    if (!ok) {
        return (null, 0, error.As(addr(new ParseError(Type:"port",Text:s))!)!);
    }
    if (p < 0 || p > 0xFFFF) {
        return (null, 0, error.As(addr(new AddrError(Err:"invalid port",Addr:s[i+1:i+1+plen]))!)!);
    }
    return (addr, p, error.As(null!)!);

}

private static (Addr, error) readPlan9Addr(@string net, @string filename) => func((defer, _, _) => {
    Addr addr = default;
    error err = default!;

    array<byte> buf = new array<byte>(128);

    var (f, err) = os.Open(filename);
    if (err != null) {
        return ;
    }
    defer(f.Close());
    var (n, err) = f.Read(buf[..]);
    if (err != null) {
        return ;
    }
    var (ip, port, err) = parsePlan9Addr(string(buf[..(int)n]));
    if (err != null) {
        return ;
    }
    switch (net) {
        case "tcp4": 

        case "udp4": 
            if (ip.Equal(IPv6zero)) {
                ip = ip[..(int)IPv4len];
            }
            break;
    }
    switch (net) {
        case "tcp": 

        case "tcp4": 

        case "tcp6": 
            addr = addr(new TCPAddr(IP:ip,Port:port));
            break;
        case "udp": 

        case "udp4": 

        case "udp6": 
            addr = addr(new UDPAddr(IP:ip,Port:port));
            break;
        default: 
            return (null, error.As(UnknownNetworkError(net))!);
            break;
    }
    return (addr, error.As(null!)!);

});

private static (ptr<os.File>, @string, @string, @string, error) startPlan9(context.Context ctx, @string net, Addr addr) {
    ptr<os.File> ctl = default!;
    @string dest = default;
    @string proto = default;
    @string name = default;
    error err = default!;

    IP ip = default;    nint port = default;
    switch (addr.type()) {
        case ptr<TCPAddr> a:
            proto = "tcp";
            ip = a.IP;
            port = a.Port;
            break;
        case ptr<UDPAddr> a:
            proto = "udp";
            ip = a.IP;
            port = a.Port;
            break;
        default:
        {
            var a = addr.type();
            err = UnknownNetworkError(net);
            return ;
            break;
        }

    }

    if (port > 65535) {
        err = InvalidAddrError("port should be < 65536");
        return ;
    }
    var (clone, dest, err) = queryCS1(ctx, proto, ip, port);
    if (err != null) {
        return ;
    }
    var (f, err) = os.OpenFile(clone, os.O_RDWR, 0);
    if (err != null) {
        return ;
    }
    array<byte> buf = new array<byte>(16);
    var (n, err) = f.Read(buf[..]);
    if (err != null) {
        f.Close();
        return ;
    }
    return (_addr_f!, dest, proto, string(buf[..(int)n]), error.As(null!)!);

}

private static void fixErr(error err) {
    ptr<OpError> (oe, ok) = err._<ptr<OpError>>();
    if (!ok) {
        return ;
    }
    Func<Addr, bool> nonNilInterface = a => {
        switch (a.type()) {
            case ptr<TCPAddr> a:
                return a == null;
                break;
            case ptr<UDPAddr> a:
                return a == null;
                break;
            case ptr<IPAddr> a:
                return a == null;
                break;
            default:
            {
                var a = a.type();
                return false;
                break;
            }
        }

    };
    if (nonNilInterface(oe.Source)) {
        oe.Source = null;
    }
    if (nonNilInterface(oe.Addr)) {
        oe.Addr = null;
    }
    {
        ptr<fs.PathError> (pe, ok) = oe.Err._<ptr<fs.PathError>>();

        if (ok) {
            _, ok = pe.Err._<syscall.ErrorString>();

            if (ok) {
                oe.Err = pe.Err;
            }

        }
    }

}

private static (ptr<netFD>, error) dialPlan9(context.Context ctx, @string net, Addr laddr, Addr raddr) => func((defer, _, _) => {
    ptr<netFD> fd = default!;
    error err = default!;

    defer(() => {
        fixErr(err);
    }());
    private partial struct res {
        public ptr<netFD> fd;
        public error err;
    }
    var resc = make_channel<res>();
    go_(() => () => {
        testHookDialChannel();
        var (fd, err) = dialPlan9Blocking(ctx, net, laddr, raddr);
        if (fd != null) {
            fd.Close();
        }
    }());
    return (_addr_res.fd!, error.As(res.err)!);
    return (_addr_null!, error.As(mapErr(ctx.Err()))!);

});

private static (ptr<netFD>, error) dialPlan9Blocking(context.Context ctx, @string net, Addr laddr, Addr raddr) {
    ptr<netFD> fd = default!;
    error err = default!;

    if (isWildcard(raddr)) {
        raddr = toLocal(raddr, net);
    }
    var (f, dest, proto, name, err) = startPlan9(ctx, net, raddr);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    {
        var la = plan9LocalAddr(laddr);

        if (la == "") {
            err = hangupCtlWrite(ctx, proto, _addr_f, "connect " + dest);
        }
        else
 {
            err = hangupCtlWrite(ctx, proto, _addr_f, "connect " + dest + " " + la);
        }
    }

    if (err != null) {
        f.Close();
        return (_addr_null!, error.As(err)!);
    }
    var (data, err) = os.OpenFile(netdir + "/" + proto + "/" + name + "/data", os.O_RDWR, 0);
    if (err != null) {
        f.Close();
        return (_addr_null!, error.As(err)!);
    }
    laddr, err = readPlan9Addr(net, netdir + "/" + proto + "/" + name + "/local");
    if (err != null) {
        data.Close();
        f.Close();
        return (_addr_null!, error.As(err)!);
    }
    return _addr_newFD(proto, name, null, f, data, laddr, raddr)!;

}

private static (ptr<netFD>, error) listenPlan9(context.Context ctx, @string net, Addr laddr) => func((defer, _, _) => {
    ptr<netFD> fd = default!;
    error err = default!;

    defer(() => {
        fixErr(err);
    }());
    var (f, dest, proto, name, err) = startPlan9(ctx, net, laddr);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    _, err = f.WriteString("announce " + dest);
    if (err != null) {
        f.Close();
        return (_addr_null!, error.As(addr(new OpError(Op:"announce",Net:net,Source:laddr,Addr:nil,Err:err))!)!);
    }
    laddr, err = readPlan9Addr(net, netdir + "/" + proto + "/" + name + "/local");
    if (err != null) {
        f.Close();
        return (_addr_null!, error.As(err)!);
    }
    return _addr_newFD(proto, name, null, f, null, laddr, null)!;

});

private static (ptr<netFD>, error) netFD(this ptr<netFD> _addr_fd) {
    ptr<netFD> _p0 = default!;
    error _p0 = default!;
    ref netFD fd = ref _addr_fd.val;

    return _addr_newFD(fd.net, fd.n, fd.listen, fd.ctl, fd.data, fd.laddr, fd.raddr)!;
}

private static (ptr<netFD>, error) acceptPlan9(this ptr<netFD> _addr_fd) => func((defer, _, _) => {
    ptr<netFD> nfd = default!;
    error err = default!;
    ref netFD fd = ref _addr_fd.val;

    defer(() => {
        fixErr(err);
    }());
    {
        var err = fd.pfd.ReadLock();

        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
    }

    defer(fd.pfd.ReadUnlock());
    var (listen, err) = os.Open(fd.dir + "/listen");
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    array<byte> buf = new array<byte>(16);
    var (n, err) = listen.Read(buf[..]);
    if (err != null) {
        listen.Close();
        return (_addr_null!, error.As(err)!);
    }
    var name = string(buf[..(int)n]);
    var (ctl, err) = os.OpenFile(netdir + "/" + fd.net + "/" + name + "/ctl", os.O_RDWR, 0);
    if (err != null) {
        listen.Close();
        return (_addr_null!, error.As(err)!);
    }
    var (data, err) = os.OpenFile(netdir + "/" + fd.net + "/" + name + "/data", os.O_RDWR, 0);
    if (err != null) {
        listen.Close();
        ctl.Close();
        return (_addr_null!, error.As(err)!);
    }
    var (raddr, err) = readPlan9Addr(fd.net, netdir + "/" + fd.net + "/" + name + "/remote");
    if (err != null) {
        listen.Close();
        ctl.Close();
        data.Close();
        return (_addr_null!, error.As(err)!);
    }
    return _addr_newFD(fd.net, name, listen, ctl, data, fd.laddr, raddr)!;

});

private static bool isWildcard(Addr a) {
    bool wildcard = default;
    switch (a.type()) {
        case ptr<TCPAddr> a:
            wildcard = a.isWildcard();
            break;
        case ptr<UDPAddr> a:
            wildcard = a.isWildcard();
            break;
        case ptr<IPAddr> a:
            wildcard = a.isWildcard();
            break;
    }
    return wildcard;

}

private static Addr toLocal(Addr a, @string net) {
    switch (a.type()) {
        case ptr<TCPAddr> a:
            a.IP = loopbackIP(net);
            break;
        case ptr<UDPAddr> a:
            a.IP = loopbackIP(net);
            break;
        case ptr<IPAddr> a:
            a.IP = loopbackIP(net);
            break;
    }
    return a;

}

// plan9LocalAddr returns a Plan 9 local address string.
// See setladdrport at https://9p.io/sources/plan9/sys/src/9/ip/devip.c.
private static @string plan9LocalAddr(Addr addr) {
    IP ip = default;
    nint port = 0;
    switch (addr.type()) {
        case ptr<TCPAddr> a:
            if (a != null) {
                ip = a.IP;
                port = a.Port;
            }
            break;
        case ptr<UDPAddr> a:
            if (a != null) {
                ip = a.IP;
                port = a.Port;
            }
            break;
    }
    if (len(ip) == 0 || ip.IsUnspecified()) {
        if (port == 0) {
            return "";
        }
        return itoa.Itoa(port);

    }
    return ip.String() + "!" + itoa.Itoa(port);

}

private static error hangupCtlWrite(context.Context ctx, @string proto, ptr<os.File> _addr_ctl, @string msg) {
    ref os.File ctl = ref _addr_ctl.val;

    if (proto != "tcp") {
        var (_, err) = ctl.WriteString(msg);
        return error.As(err)!;
    }
    var written = make_channel<object>();
    var errc = make_channel<error>();
    go_(() => () => {
        ctl.WriteString("hangup");
        errc.Send(mapErr(ctx.Err()));
        errc.Send(null);
    }());
    (_, err) = ctl.WriteString(msg);
    close(written);
    {
        var e = errc.Receive();

        if (err == null && e != null) { // we hung up
            return error.As(e)!;

        }
    }

    return error.As(err)!;

}

} // end net_package
