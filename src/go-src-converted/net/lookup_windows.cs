// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:16:22 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\lookup_windows.go
using context = go.context_package;
using windows = go.@internal.syscall.windows_package;
using os = go.os_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using System;
using System.Threading;


namespace go;

public static partial class net_package {

private static readonly var _WSAHOST_NOT_FOUND = syscall.Errno(11001);



private static error winError(@string call, error err) {

    if (err == _WSAHOST_NOT_FOUND) 
        return error.As(errNoSuchHost)!;
        return error.As(os.NewSyscallError(call, err))!;

}

private static (nint, error) getprotobyname(@string name) {
    nint proto = default;
    error err = default!;

    var (p, err) = syscall.GetProtoByName(name);
    if (err != null) {
        return (0, error.As(winError("getprotobyname", err))!);
    }
    return (int(p.Proto), error.As(null!)!);

}

// lookupProtocol looks up IP protocol name and returns correspondent protocol number.
private static (nint, error) lookupProtocol(context.Context ctx, @string name) => func((defer, _, _) => {
    nint _p0 = default;
    error _p0 = default!;
 
    // GetProtoByName return value is stored in thread local storage.
    // Start new os thread before the call to prevent races.
    private partial struct result {
        public nint proto;
        public error err;
    }
    var ch = make_channel<result>(); // unbuffered
    go_(() => () => {
        acquireThread();
        defer(releaseThread());
        runtime.LockOSThread();
        defer(runtime.UnlockOSThread());
        var (proto, err) = getprotobyname(name);
    }());
    if (r.err != null) {
        {
            var proto__prev2 = proto;

            (proto, err) = lookupProtocolMap(name);

            if (err == null) {
                return (proto, error.As(null!)!);
            }

            proto = proto__prev2;

        }


        ptr<DNSError> dnsError = addr(new DNSError(Err:r.err.Error(),Name:name));
        if (r.err == errNoSuchHost) {
            dnsError.IsNotFound = true;
        }
        r.err = dnsError;

    }
    return (r.proto, error.As(r.err)!);
    return (0, error.As(mapErr(ctx.Err()))!);

});

private static (slice<@string>, error) lookupHost(this ptr<Resolver> _addr_r, context.Context ctx, @string name) {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;

    var (ips, err) = r.lookupIP(ctx, "ip", name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var addrs = make_slice<@string>(0, len(ips));
    foreach (var (_, ip) in ips) {
        addrs = append(addrs, ip.String());
    }    return (addrs, error.As(null!)!);

}

private static (slice<IPAddr>, error) lookupIP(this ptr<Resolver> _addr_r, context.Context ctx, @string network, @string name) => func((defer, _, _) => {
    slice<IPAddr> _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;
 
    // TODO(bradfitz,brainman): use ctx more. See TODO below.

    int family = syscall.AF_UNSPEC;
    switch (ipVersion(network)) {
        case '4': 
            family = syscall.AF_INET;
            break;
        case '6': 
            family = syscall.AF_INET6;
            break;
    }

    Func<(slice<IPAddr>, error)> getaddr = () => {
        acquireThread();
        defer(releaseThread());
        ref syscall.AddrinfoW hints = ref heap(new syscall.AddrinfoW(Family:family,Socktype:syscall.SOCK_STREAM,Protocol:syscall.IPPROTO_IP,), out ptr<syscall.AddrinfoW> _addr_hints);
        ptr<syscall.AddrinfoW> result;
        var (name16p, err) = syscall.UTF16PtrFromString(name);
        if (err != null) {
            return (null, error.As(addr(new DNSError(Name:name,Err:err.Error()))!)!);
        }
        var e = syscall.GetAddrInfoW(name16p, null, _addr_hints, _addr_result);
        if (e != null) {
            var err = winError("getaddrinfow", e);
            ptr<DNSError> dnsError = addr(new DNSError(Err:err.Error(),Name:name));
            if (err == errNoSuchHost) {
                dnsError.IsNotFound = true;
            }
            return (null, error.As(dnsError)!);
        }
        defer(syscall.FreeAddrInfoW(result));
        var addrs = make_slice<IPAddr>(0, 5);
        while (result != null) {
            var addr = @unsafe.Pointer(result.Addr);

            if (result.Family == syscall.AF_INET) 
                var a = (syscall.RawSockaddrInet4.val)(addr).Addr;
                addrs = append(addrs, new IPAddr(IP:IPv4(a[0],a[1],a[2],a[3])));
            else if (result.Family == syscall.AF_INET6) 
                a = (syscall.RawSockaddrInet6.val)(addr).Addr;
                var zone = zoneCache.name(int((syscall.RawSockaddrInet6.val)(addr).Scope_id));
                addrs = append(addrs, new IPAddr(IP:IP{a[0],a[1],a[2],a[3],a[4],a[5],a[6],a[7],a[8],a[9],a[10],a[11],a[12],a[13],a[14],a[15]},Zone:zone));
            else 
                return (null, error.As(addr(new DNSError(Err:syscall.EWINDOWS.Error(),Name:name))!)!);
                        result = result.Next;
        }
        return (addrs, error.As(null!)!);

    };

    private partial struct ret {
        public slice<IPAddr> addrs;
        public error err;
    }

    channel<ret> ch = default;
    if (ctx.Err() == null) {
        ch = make_channel<ret>(1);
        go_(() => () => {
            var (addr, err) = getaddr();
            ch.Send(new ret(addrs:addr,err:err));
        }());
    }
    return (r.addrs, error.As(r.err)!);
    return (null, error.As(addr(new DNSError(Name:name,Err:ctx.Err().Error(),IsTimeout:ctx.Err()==context.DeadlineExceeded,))!)!);

});

private static (nint, error) lookupPort(this ptr<Resolver> _addr_r, context.Context ctx, @string network, @string service) => func((defer, _, _) => {
    nint _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;

    if (r.preferGo()) {
        return lookupPortMap(network, service);
    }
    acquireThread();
    defer(releaseThread());
    int stype = default;
    switch (network) {
        case "tcp4": 

        case "tcp6": 
            stype = syscall.SOCK_STREAM;
            break;
        case "udp4": 

        case "udp6": 
            stype = syscall.SOCK_DGRAM;
            break;
    }
    ref syscall.AddrinfoW hints = ref heap(new syscall.AddrinfoW(Family:syscall.AF_UNSPEC,Socktype:stype,Protocol:syscall.IPPROTO_IP,), out ptr<syscall.AddrinfoW> _addr_hints);
    ptr<syscall.AddrinfoW> result;
    var e = syscall.GetAddrInfoW(null, syscall.StringToUTF16Ptr(service), _addr_hints, _addr_result);
    if (e != null) {
        {
            var (port, err) = lookupPortMap(network, service);

            if (err == null) {
                return (port, error.As(null!)!);
            }

        }

        var err = winError("getaddrinfow", e);
        ptr<DNSError> dnsError = addr(new DNSError(Err:err.Error(),Name:network+"/"+service));
        if (err == errNoSuchHost) {
            dnsError.IsNotFound = true;
        }
        return (0, error.As(dnsError)!);

    }
    defer(syscall.FreeAddrInfoW(result));
    if (result == null) {
        return (0, error.As(addr(new DNSError(Err:syscall.EINVAL.Error(),Name:network+"/"+service))!)!);
    }
    var addr = @unsafe.Pointer(result.Addr);

    if (result.Family == syscall.AF_INET) 
        var a = (syscall.RawSockaddrInet4.val)(addr);
        return (int(syscall.Ntohs(a.Port)), error.As(null!)!);
    else if (result.Family == syscall.AF_INET6) 
        a = (syscall.RawSockaddrInet6.val)(addr);
        return (int(syscall.Ntohs(a.Port)), error.As(null!)!);
        return (0, error.As(addr(new DNSError(Err:syscall.EINVAL.Error(),Name:network+"/"+service))!)!);

});

private static (@string, error) lookupCNAME(this ptr<Resolver> _addr__p0, context.Context ctx, @string name) => func((defer, _, _) => {
    @string _p0 = default;
    error _p0 = default!;
    ref Resolver _p0 = ref _addr__p0.val;
 
    // TODO(bradfitz): finish ctx plumbing. Nothing currently depends on this.
    acquireThread();
    defer(releaseThread());
    ptr<syscall.DNSRecord> r;
    var e = syscall.DnsQuery(name, syscall.DNS_TYPE_CNAME, 0, null, _addr_r, null); 
    // windows returns DNS_INFO_NO_RECORDS if there are no CNAME-s
    {
        syscall.Errno (errno, ok) = e._<syscall.Errno>();

        if (ok && errno == syscall.DNS_INFO_NO_RECORDS) { 
            // if there are no aliases, the canonical name is the input name
            return (absDomainName((slice<byte>)name), error.As(null!)!);

        }
    }

    if (e != null) {
        return ("", error.As(addr(new DNSError(Err:winError("dnsquery",e).Error(),Name:name))!)!);
    }
    defer(syscall.DnsRecordListFree(r, 1));

    var resolved = resolveCNAME(_addr_syscall.StringToUTF16Ptr(name), r);
    var cname = windows.UTF16PtrToString(resolved);
    return (absDomainName((slice<byte>)cname), error.As(null!)!);

});

private static (@string, slice<ptr<SRV>>, error) lookupSRV(this ptr<Resolver> _addr__p0, context.Context ctx, @string service, @string proto, @string name) => func((defer, _, _) => {
    @string _p0 = default;
    slice<ptr<SRV>> _p0 = default;
    error _p0 = default!;
    ref Resolver _p0 = ref _addr__p0.val;
 
    // TODO(bradfitz): finish ctx plumbing. Nothing currently depends on this.
    acquireThread();
    defer(releaseThread());
    @string target = default;
    if (service == "" && proto == "") {
        target = name;
    }
    else
 {
        target = "_" + service + "._" + proto + "." + name;
    }
    ptr<syscall.DNSRecord> r;
    var e = syscall.DnsQuery(target, syscall.DNS_TYPE_SRV, 0, null, _addr_r, null);
    if (e != null) {
        return ("", null, error.As(addr(new DNSError(Err:winError("dnsquery",e).Error(),Name:target))!)!);
    }
    defer(syscall.DnsRecordListFree(r, 1));

    var srvs = make_slice<ptr<SRV>>(0, 10);
    foreach (var (_, p) in validRecs(r, syscall.DNS_TYPE_SRV, target)) {
        var v = (syscall.DNSSRVData.val)(@unsafe.Pointer(_addr_p.Data[0]));
        srvs = append(srvs, addr(new SRV(absDomainName([]byte(syscall.UTF16ToString((*[256]uint16)(unsafe.Pointer(v.Target))[:]))),v.Port,v.Priority,v.Weight)));
    }    byPriorityWeight(srvs).sort();
    return (absDomainName((slice<byte>)target), srvs, error.As(null!)!);

});

private static (slice<ptr<MX>>, error) lookupMX(this ptr<Resolver> _addr__p0, context.Context ctx, @string name) => func((defer, _, _) => {
    slice<ptr<MX>> _p0 = default;
    error _p0 = default!;
    ref Resolver _p0 = ref _addr__p0.val;
 
    // TODO(bradfitz): finish ctx plumbing. Nothing currently depends on this.
    acquireThread();
    defer(releaseThread());
    ptr<syscall.DNSRecord> r;
    var e = syscall.DnsQuery(name, syscall.DNS_TYPE_MX, 0, null, _addr_r, null);
    if (e != null) {
        return (null, error.As(addr(new DNSError(Err:winError("dnsquery",e).Error(),Name:name))!)!);
    }
    defer(syscall.DnsRecordListFree(r, 1));

    var mxs = make_slice<ptr<MX>>(0, 10);
    foreach (var (_, p) in validRecs(r, syscall.DNS_TYPE_MX, name)) {
        var v = (syscall.DNSMXData.val)(@unsafe.Pointer(_addr_p.Data[0]));
        mxs = append(mxs, addr(new MX(absDomainName([]byte(windows.UTF16PtrToString(v.NameExchange))),v.Preference)));
    }    byPref(mxs).sort();
    return (mxs, error.As(null!)!);

});

private static (slice<ptr<NS>>, error) lookupNS(this ptr<Resolver> _addr__p0, context.Context ctx, @string name) => func((defer, _, _) => {
    slice<ptr<NS>> _p0 = default;
    error _p0 = default!;
    ref Resolver _p0 = ref _addr__p0.val;
 
    // TODO(bradfitz): finish ctx plumbing. Nothing currently depends on this.
    acquireThread();
    defer(releaseThread());
    ptr<syscall.DNSRecord> r;
    var e = syscall.DnsQuery(name, syscall.DNS_TYPE_NS, 0, null, _addr_r, null);
    if (e != null) {
        return (null, error.As(addr(new DNSError(Err:winError("dnsquery",e).Error(),Name:name))!)!);
    }
    defer(syscall.DnsRecordListFree(r, 1));

    var nss = make_slice<ptr<NS>>(0, 10);
    foreach (var (_, p) in validRecs(r, syscall.DNS_TYPE_NS, name)) {
        var v = (syscall.DNSPTRData.val)(@unsafe.Pointer(_addr_p.Data[0]));
        nss = append(nss, addr(new NS(absDomainName([]byte(syscall.UTF16ToString((*[256]uint16)(unsafe.Pointer(v.Host))[:]))))));
    }    return (nss, error.As(null!)!);

});

private static (slice<@string>, error) lookupTXT(this ptr<Resolver> _addr__p0, context.Context ctx, @string name) => func((defer, _, _) => {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref Resolver _p0 = ref _addr__p0.val;
 
    // TODO(bradfitz): finish ctx plumbing. Nothing currently depends on this.
    acquireThread();
    defer(releaseThread());
    ptr<syscall.DNSRecord> r;
    var e = syscall.DnsQuery(name, syscall.DNS_TYPE_TEXT, 0, null, _addr_r, null);
    if (e != null) {
        return (null, error.As(addr(new DNSError(Err:winError("dnsquery",e).Error(),Name:name))!)!);
    }
    defer(syscall.DnsRecordListFree(r, 1));

    var txts = make_slice<@string>(0, 10);
    foreach (var (_, p) in validRecs(r, syscall.DNS_TYPE_TEXT, name)) {
        var d = (syscall.DNSTXTData.val)(@unsafe.Pointer(_addr_p.Data[0]));
        @string s = "";
        foreach (var (_, v) in new ptr<ptr<array<ptr<ushort>>>>(@unsafe.Pointer(_addr_(d.StringArray[0]))).slice(-1, d.StringCount, d.StringCount)) {
            s += windows.UTF16PtrToString(v);
        }        txts = append(txts, s);
    }    return (txts, error.As(null!)!);

});

private static (slice<@string>, error) lookupAddr(this ptr<Resolver> _addr__p0, context.Context ctx, @string addr) => func((defer, _, _) => {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref Resolver _p0 = ref _addr__p0.val;
 
    // TODO(bradfitz): finish ctx plumbing. Nothing currently depends on this.
    acquireThread();
    defer(releaseThread());
    var (arpa, err) = reverseaddr(addr);
    if (err != null) {
        return (null, error.As(err)!);
    }
    ptr<syscall.DNSRecord> r;
    var e = syscall.DnsQuery(arpa, syscall.DNS_TYPE_PTR, 0, null, _addr_r, null);
    if (e != null) {
        return (null, error.As(addr(new DNSError(Err:winError("dnsquery",e).Error(),Name:addr))!)!);
    }
    defer(syscall.DnsRecordListFree(r, 1));

    var ptrs = make_slice<@string>(0, 10);
    foreach (var (_, p) in validRecs(r, syscall.DNS_TYPE_PTR, arpa)) {
        var v = (syscall.DNSPTRData.val)(@unsafe.Pointer(_addr_p.Data[0]));
        ptrs = append(ptrs, absDomainName((slice<byte>)windows.UTF16PtrToString(v.Host)));
    }    return (ptrs, error.As(null!)!);

});

private static readonly nuint dnsSectionMask = 0x0003;

// returns only results applicable to name and resolves CNAME entries


// returns only results applicable to name and resolves CNAME entries
private static slice<ptr<syscall.DNSRecord>> validRecs(ptr<syscall.DNSRecord> _addr_r, ushort dnstype, @string name) {
    ref syscall.DNSRecord r = ref _addr_r.val;

    var cname = syscall.StringToUTF16Ptr(name);
    if (dnstype != syscall.DNS_TYPE_CNAME) {
        cname = resolveCNAME(_addr_cname, _addr_r);
    }
    var rec = make_slice<ptr<syscall.DNSRecord>>(0, 10);
    {
        var p = r;

        while (p != null) { 
            // in case of a local machine, DNS records are returned with DNSREC_QUESTION flag instead of DNS_ANSWER
            if (p.Dw & dnsSectionMask != syscall.DnsSectionAnswer && p.Dw & dnsSectionMask != syscall.DnsSectionQuestion) {
                continue;
            p = p.Next;
            }

            if (p.Type != dnstype) {
                continue;
            }

            if (!syscall.DnsNameCompare(cname, p.Name)) {
                continue;
            }

            rec = append(rec, p);

        }
    }
    return rec;

}

// returns the last CNAME in chain
private static ptr<ushort> resolveCNAME(ptr<ushort> _addr_name, ptr<syscall.DNSRecord> _addr_r) {
    ref ushort name = ref _addr_name.val;
    ref syscall.DNSRecord r = ref _addr_r.val;
 
    // limit cname resolving to 10 in case of an infinite CNAME loop
Cname:
    for (nint cnameloop = 0; cnameloop < 10; cnameloop++) {
        {
            var p = r;

            while (p != null) {
                if (p.Dw & dnsSectionMask != syscall.DnsSectionAnswer) {
                    continue;
                p = p.Next;
                }

                if (p.Type != syscall.DNS_TYPE_CNAME) {
                    continue;
                }

                if (!syscall.DnsNameCompare(name, p.Name)) {
                    continue;
                }

                name = (syscall.DNSPTRData.val)(@unsafe.Pointer(_addr_r.Data[0])).Host;
                _continueCname = true;
                break;
            }

        }
        break;

    }
    return _addr_name!;

}

// concurrentThreadsLimit returns the number of threads we permit to
// run concurrently doing DNS lookups.
private static nint concurrentThreadsLimit() {
    return 500;
}

} // end net_package
