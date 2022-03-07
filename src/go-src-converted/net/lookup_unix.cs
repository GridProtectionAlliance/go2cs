// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package net -- go2cs converted at 2022 March 06 22:16:20 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\lookup_unix.go
using context = go.context_package;
using bytealg = go.@internal.bytealg_package;
using sync = go.sync_package;
using syscall = go.syscall_package;

using dnsmessage = go.golang.org.x.net.dns.dnsmessage_package;

namespace go;

public static partial class net_package {

private static sync.Once onceReadProtocols = default;

// readProtocols loads contents of /etc/protocols into protocols map
// for quick access.
private static void readProtocols() => func((defer, _, _) => {
    var (file, err) = open("/etc/protocols");
    if (err != null) {
        return ;
    }
    defer(file.close());

    {
        var (line, ok) = file.readLine();

        while (ok) { 
            // tcp    6   TCP    # transmission control protocol
            {
                var i = bytealg.IndexByteString(line, '#');

                if (i >= 0) {
                    line = line[(int)0..(int)i];
            line, ok = file.readLine();
                }

            }

            var f = getFields(line);
            if (len(f) < 2) {
                continue;
            }

            {
                var (proto, _, ok) = dtoi(f[1]);

                if (ok) {
                    {
                        var (_, ok) = protocols[f[0]];

                        if (!ok) {
                            protocols[f[0]] = proto;
                        }

                    }

                    foreach (var (_, alias) in f[(int)2..]) {
                        {
                            (_, ok) = protocols[alias];

                            if (!ok) {
                                protocols[alias] = proto;
                            }

                        }

                    }

                }

            }

        }
    }

});

// lookupProtocol looks up IP protocol name in /etc/protocols and
// returns correspondent protocol number.
private static (nint, error) lookupProtocol(context.Context _, @string name) {
    nint _p0 = default;
    error _p0 = default!;

    onceReadProtocols.Do(readProtocols);
    return lookupProtocolMap(name);
}

private static (Conn, error) dial(this ptr<Resolver> _addr_r, context.Context ctx, @string network, @string server) {
    Conn _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;
 
    // Calling Dial here is scary -- we have to be sure not to
    // dial a name that will require a DNS lookup, or Dial will
    // call back here to translate it. The DNS config parser has
    // already checked that all the cfg.servers are IP
    // addresses, which Dial will use without a DNS lookup.
    Conn c = default;
    error err = default!;
    if (r != null && r.Dial != null) {
        c, err = r.Dial(ctx, network, server);
    }
    else
 {
        Dialer d = default;
        c, err = d.DialContext(ctx, network, server);
    }
    if (err != null) {
        return (null, error.As(mapErr(err))!);
    }
    return (c, error.As(null!)!);

}

private static (slice<@string>, error) lookupHost(this ptr<Resolver> _addr_r, context.Context ctx, @string host) {
    slice<@string> addrs = default;
    error err = default!;
    ref Resolver r = ref _addr_r.val;

    var order = systemConf().hostLookupOrder(r, host);
    if (!r.preferGo() && order == hostLookupCgo) {
        {
            var (addrs, err, ok) = cgoLookupHost(ctx, host);

            if (ok) {
                return (addrs, error.As(err)!);
            } 
            // cgo not available (or netgo); fall back to Go's DNS resolver

        } 
        // cgo not available (or netgo); fall back to Go's DNS resolver
        order = hostLookupFilesDNS;

    }
    return r.goLookupHostOrder(ctx, host, order);

}

private static (slice<IPAddr>, error) lookupIP(this ptr<Resolver> _addr_r, context.Context ctx, @string network, @string host) {
    slice<IPAddr> addrs = default;
    error err = default!;
    ref Resolver r = ref _addr_r.val;

    if (r.preferGo()) {
        return r.goLookupIP(ctx, network, host);
    }
    var order = systemConf().hostLookupOrder(r, host);
    if (order == hostLookupCgo) {
        {
            var (addrs, err, ok) = cgoLookupIP(ctx, network, host);

            if (ok) {
                return (addrs, error.As(err)!);
            } 
            // cgo not available (or netgo); fall back to Go's DNS resolver

        } 
        // cgo not available (or netgo); fall back to Go's DNS resolver
        order = hostLookupFilesDNS;

    }
    var (ips, _, err) = r.goLookupIPCNAMEOrder(ctx, network, host, order);
    return (ips, error.As(err)!);

}

private static (nint, error) lookupPort(this ptr<Resolver> _addr_r, context.Context ctx, @string network, @string service) {
    nint _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;

    if (!r.preferGo() && systemConf().canUseCgo()) {
        {
            var port__prev2 = port;

            var (port, err, ok) = cgoLookupPort(ctx, network, service);

            if (ok) {
                if (err != null) { 
                    // Issue 18213: if cgo fails, first check to see whether we
                    // have the answer baked-in to the net package.
                    {
                        var port__prev4 = port;

                        var (port, err) = goLookupPort(network, service);

                        if (err == null) {
                            return (port, error.As(null!)!);
                        }

                        port = port__prev4;

                    }

                }

                return (port, error.As(err)!);

            }

            port = port__prev2;

        }

    }
    return goLookupPort(network, service);

}

private static (@string, error) lookupCNAME(this ptr<Resolver> _addr_r, context.Context ctx, @string name) {
    @string _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;

    if (!r.preferGo() && systemConf().canUseCgo()) {
        {
            var (cname, err, ok) = cgoLookupCNAME(ctx, name);

            if (ok) {
                return (cname, error.As(err)!);
            }

        }

    }
    return r.goLookupCNAME(ctx, name);

}

private static (@string, slice<ptr<SRV>>, error) lookupSRV(this ptr<Resolver> _addr_r, context.Context ctx, @string service, @string proto, @string name) {
    @string _p0 = default;
    slice<ptr<SRV>> _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;

    @string target = default;
    if (service == "" && proto == "") {
        target = name;
    }
    else
 {
        target = "_" + service + "._" + proto + "." + name;
    }
    var (p, server, err) = r.lookup(ctx, target, dnsmessage.TypeSRV);
    if (err != null) {
        return ("", null, error.As(err)!);
    }
    slice<ptr<SRV>> srvs = default;
    dnsmessage.Name cname = default;
    while (true) {
        var (h, err) = p.AnswerHeader();
        if (err == dnsmessage.ErrSectionDone) {
            break;
        }
        if (err != null) {
            return ("", null, error.As(addr(new DNSError(Err:"cannot unmarshal DNS message",Name:name,Server:server,))!)!);
        }
        if (h.Type != dnsmessage.TypeSRV) {
            {
                var err = p.SkipAnswer();

                if (err != null) {
                    return ("", null, error.As(addr(new DNSError(Err:"cannot unmarshal DNS message",Name:name,Server:server,))!)!);
                }

            }

            continue;

        }
        if (cname.Length == 0 && h.Name.Length != 0) {
            cname = h.Name;
        }
        var (srv, err) = p.SRVResource();
        if (err != null) {
            return ("", null, error.As(addr(new DNSError(Err:"cannot unmarshal DNS message",Name:name,Server:server,))!)!);
        }
        srvs = append(srvs, addr(new SRV(Target:srv.Target.String(),Port:srv.Port,Priority:srv.Priority,Weight:srv.Weight)));

    }
    byPriorityWeight(srvs).sort();
    return (cname.String(), srvs, error.As(null!)!);

}

private static (slice<ptr<MX>>, error) lookupMX(this ptr<Resolver> _addr_r, context.Context ctx, @string name) {
    slice<ptr<MX>> _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;

    var (p, server, err) = r.lookup(ctx, name, dnsmessage.TypeMX);
    if (err != null) {
        return (null, error.As(err)!);
    }
    slice<ptr<MX>> mxs = default;
    while (true) {
        var (h, err) = p.AnswerHeader();
        if (err == dnsmessage.ErrSectionDone) {
            break;
        }
        if (err != null) {
            return (null, error.As(addr(new DNSError(Err:"cannot unmarshal DNS message",Name:name,Server:server,))!)!);
        }
        if (h.Type != dnsmessage.TypeMX) {
            {
                var err = p.SkipAnswer();

                if (err != null) {
                    return (null, error.As(addr(new DNSError(Err:"cannot unmarshal DNS message",Name:name,Server:server,))!)!);
                }

            }

            continue;

        }
        var (mx, err) = p.MXResource();
        if (err != null) {
            return (null, error.As(addr(new DNSError(Err:"cannot unmarshal DNS message",Name:name,Server:server,))!)!);
        }
        mxs = append(mxs, addr(new MX(Host:mx.MX.String(),Pref:mx.Pref)));


    }
    byPref(mxs).sort();
    return (mxs, error.As(null!)!);

}

private static (slice<ptr<NS>>, error) lookupNS(this ptr<Resolver> _addr_r, context.Context ctx, @string name) {
    slice<ptr<NS>> _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;

    var (p, server, err) = r.lookup(ctx, name, dnsmessage.TypeNS);
    if (err != null) {
        return (null, error.As(err)!);
    }
    slice<ptr<NS>> nss = default;
    while (true) {
        var (h, err) = p.AnswerHeader();
        if (err == dnsmessage.ErrSectionDone) {
            break;
        }
        if (err != null) {
            return (null, error.As(addr(new DNSError(Err:"cannot unmarshal DNS message",Name:name,Server:server,))!)!);
        }
        if (h.Type != dnsmessage.TypeNS) {
            {
                var err = p.SkipAnswer();

                if (err != null) {
                    return (null, error.As(addr(new DNSError(Err:"cannot unmarshal DNS message",Name:name,Server:server,))!)!);
                }

            }

            continue;

        }
        var (ns, err) = p.NSResource();
        if (err != null) {
            return (null, error.As(addr(new DNSError(Err:"cannot unmarshal DNS message",Name:name,Server:server,))!)!);
        }
        nss = append(nss, addr(new NS(Host:ns.NS.String())));

    }
    return (nss, error.As(null!)!);

}

private static (slice<@string>, error) lookupTXT(this ptr<Resolver> _addr_r, context.Context ctx, @string name) {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;

    var (p, server, err) = r.lookup(ctx, name, dnsmessage.TypeTXT);
    if (err != null) {
        return (null, error.As(err)!);
    }
    slice<@string> txts = default;
    while (true) {
        var (h, err) = p.AnswerHeader();
        if (err == dnsmessage.ErrSectionDone) {
            break;
        }
        if (err != null) {
            return (null, error.As(addr(new DNSError(Err:"cannot unmarshal DNS message",Name:name,Server:server,))!)!);
        }
        if (h.Type != dnsmessage.TypeTXT) {
            {
                var err = p.SkipAnswer();

                if (err != null) {
                    return (null, error.As(addr(new DNSError(Err:"cannot unmarshal DNS message",Name:name,Server:server,))!)!);
                }

            }

            continue;

        }
        var (txt, err) = p.TXTResource();
        if (err != null) {
            return (null, error.As(addr(new DNSError(Err:"cannot unmarshal DNS message",Name:name,Server:server,))!)!);
        }
        nint n = 0;
        {
            var s__prev2 = s;

            foreach (var (_, __s) in txt.TXT) {
                s = __s;
                n += len(s);
            }

            s = s__prev2;
        }

        var txtJoin = make_slice<byte>(0, n);
        {
            var s__prev2 = s;

            foreach (var (_, __s) in txt.TXT) {
                s = __s;
                txtJoin = append(txtJoin, s);
            }

            s = s__prev2;
        }

        if (len(txts) == 0) {
            txts = make_slice<@string>(0, 1);
        }
        txts = append(txts, string(txtJoin));

    }
    return (txts, error.As(null!)!);

}

private static (slice<@string>, error) lookupAddr(this ptr<Resolver> _addr_r, context.Context ctx, @string addr) {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;

    if (!r.preferGo() && systemConf().canUseCgo()) {
        {
            var (ptrs, err, ok) = cgoLookupPTR(ctx, addr);

            if (ok) {
                return (ptrs, error.As(err)!);
            }

        }

    }
    return r.goLookupPTR(ctx, addr);

}

// concurrentThreadsLimit returns the number of threads we permit to
// run concurrently doing DNS lookups via cgo. A DNS lookup may use a
// file descriptor so we limit this to less than the number of
// permitted open files. On some systems, notably Darwin, if
// getaddrinfo is unable to open a file descriptor it simply returns
// EAI_NONAME rather than a useful error. Limiting the number of
// concurrent getaddrinfo calls to less than the permitted number of
// file descriptors makes that error less likely. We don't bother to
// apply the same limit to DNS lookups run directly from Go, because
// there we will return a meaningful "too many open files" error.
private static nint concurrentThreadsLimit() {
    ref syscall.Rlimit rlim = ref heap(out ptr<syscall.Rlimit> _addr_rlim);
    {
        var err = syscall.Getrlimit(syscall.RLIMIT_NOFILE, _addr_rlim);

        if (err != null) {
            return 500;
        }
    }

    var r = int(rlim.Cur);
    if (r > 500) {
        r = 500;
    }
    else if (r > 30) {
        r -= 30;
    }
    return r;

}

} // end net_package
