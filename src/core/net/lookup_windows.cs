// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using windows = @internal.syscall.windows_package;
using os = os_package;
using runtime = runtime_package;
using syscall = syscall_package;
using time = time_package;
using @unsafe = unsafe_package;
using @internal.syscall;

partial class net_package {

// cgoAvailable set to true to indicate that the cgo resolver
// is available on Windows. Note that on Windows the cgo resolver
// does not actually use cgo.
internal const bool cgoAvailable = true;

internal static readonly syscall.Errno _DNS_ERROR_RCODE_NAME_ERROR = /* syscall.Errno(9003) */ 9003;
internal static readonly syscall.Errno _DNS_INFO_NO_RECORDS = /* syscall.Errno(9501) */ 9501;
internal static readonly syscall.Errno _WSAHOST_NOT_FOUND = /* syscall.Errno(11001) */ 11001;
internal static readonly syscall.Errno _WSATRY_AGAIN = /* syscall.Errno(11002) */ 11002;
internal static readonly syscall.Errno _WSATYPE_NOT_FOUND = /* syscall.Errno(10109) */ 10109;

internal static error winError(@string call, error err) {
    var exprᴛ1 = err;
    if (exprᴛ1 == _WSAHOST_NOT_FOUND || exprᴛ1 == _DNS_ERROR_RCODE_NAME_ERROR || exprᴛ1 == _DNS_INFO_NO_RECORDS) {
        return ~errNoSuchHost;
    }

    return os.NewSyscallError(call, err);
}

internal static (nint proto, error err) getprotobyname(@string name) {
    nint proto = default!;
    error err = default!;

    (p, err) = syscall.GetProtoByName(name);
    if (err != default!) {
        return (0, winError("getprotobyname"u8, err));
    }
    return (((nint)(~p).Proto), default!);
}

// GetProtoByName return value is stored in thread local storage.
// Start new os thread before the call to prevent races.
[GoType("dyn")] partial struct lookupProtocol_result {
    internal nint proto;
    internal error err;
}

// lookupProtocol looks up IP protocol name and returns correspondent protocol number.
internal static (nint, error) lookupProtocol(context.Context ctx, @string name) => func((defer, _) => {
    var ch = new channel<result>(1);
    // unbuffered
    var chʗ1 = ch;
    goǃ(() => {
        {
            var err = acquireThread(ctx); if (err != default!) {
                chʗ1.ᐸꟷ(new result(err: mapErr(err)));
                return;
            }
        }
        defer(releaseThread);
        runtime.LockOSThread();
        defer(runtime.UnlockOSThread);
        var (proto, err) = getprotobyname(name);
        switch (select(ch.ᐸꟷ(new result(proto: proto, err: err), ꓸꓸꓸ), ᐸꟷ(ctx.Done(), ꓸꓸꓸ))) {
        case 0: {
            break;
        }
        case 1 when ctx.Done().ꟷᐳ(out _): {
            break;
        }}
    });
    switch (select(ᐸꟷ(ch, ꓸꓸꓸ), ᐸꟷ(ctx.Done(), ꓸꓸꓸ))) {
    case 0 when ch.ꟷᐳ(out var r): {
        if (r.err != default!) {
            {
                var (proto, err) = lookupProtocolMap(name); if (err == default!) {
                    return (proto, default!);
                }
            }
            r.err = newDNSError(r.err, name, ""u8);
        }
        return (r.proto, r.err);
    }
    case 1 when ctx.Done().ꟷᐳ(out _): {
        return (0, mapErr(ctx.Err()));
    }}
});

[GoRecv] internal static (slice<@string>, error) lookupHost(this ref Resolver r, context.Context ctx, @string name) {
    (ips, err) = r.lookupIP(ctx, "ip"u8, name);
    if (err != default!) {
        return (default!, err);
    }
    var addrs = new slice<@string>(0, len(ips));
    foreach (var (_, ip) in ips) {
        addrs = append(addrs, ip.String());
    }
    return (addrs, default!);
}

[GoType("dyn")] partial struct lookupIP_ret {
    internal slice<IPAddr> addrs;
    internal error err;
}

[GoRecv] internal static (slice<IPAddr>, error) lookupIP(this ref Resolver r, context.Context ctx, @string network, @string name) => func((defer, _) => {
    {
        var (order, conf) = systemConf().hostLookupOrder(r, name); if (order != hostLookupCgo) {
            return r.goLookupIP(ctx, network, name, order, conf);
        }
    }
    // TODO(bradfitz,brainman): use ctx more. See TODO below.
    int32 family = syscall.AF_UNSPEC;
    switch (ipVersion(network)) {
    case (rune)'4': {
        family = syscall.AF_INET;
        break;
    }
    case (rune)'6': {
        family = syscall.AF_INET6;
        break;
    }}

    var getaddr = 
    var zoneCacheʗ1 = zoneCache;
    () => {
        {
            var err = acquireThread(ctx); if (err != default!) {
                return (default!, new DNSError(
                    Name: name,
                    Err: mapErr(err).Error(),
                    IsTimeout: AreEqual(ctx.Err(), context.DeadlineExceeded)
                ));
            }
        }
        defer(releaseThread);
        ref var hints = ref heap<syscall_package.AddrinfoW>(out var Ꮡhints);
        hints = new syscall.AddrinfoW(
            Family: family,
            Socktype: syscall.SOCK_STREAM,
            Protocol: syscall.IPPROTO_IP
        );
        ж<syscall.AddrinfoW> result = default!;
        (name16p, err) = syscall.UTF16PtrFromString(name);
        if (err != default!) {
            return (default!, ~newDNSError(err, name, ""u8));
        }
        var dnsConf = getSystemDNSConfig();
        ref var start = ref heap<time_package.Time>(out var Ꮡstart);
        start = time.Now();
        error e = default!;
        for (nint i = 0; i < (~dnsConf).attempts; i++) {
            e = syscall.GetAddrInfoW(name16p, nil, Ꮡhints, Ꮡ(result));
            if (e == default! || e != _WSATRY_AGAIN || time.Since(start) > (~dnsConf).timeout) {
                break;
            }
        }
        if (e != default!) {
            return (default!, ~newDNSError(winError("getaddrinfow"u8, e), name, ""u8));
        }
        deferǃ(syscall.FreeAddrInfoW, result, defer);
        var addrs = new slice<IPAddr>(0, 5);
        for (; result != nil; result = result.val.Next) {
            @unsafe.Pointer addr = ((@unsafe.Pointer)(~result).Addr);
            switch ((~result).Family) {
            case syscall.AF_INET: {
                ref var a = ref heap<array<byte>>(out var Ꮡa);
                a = ((ж<syscall.RawSockaddrInet4>)(uintptr)(addr)).val.Addr;
                addrs = append(addrs, new IPAddr(IP: copyIP(a[..])));
                break;
            }
            case syscall.AF_INET6: {
                ref var a = ref heap<array<byte>>(out var Ꮡa);
                a = ((ж<syscall.RawSockaddrInet6>)(uintptr)(addr)).val.Addr;
                @string zone = zoneCache.name(((nint)((ж<syscall.RawSockaddrInet6>)(uintptr)(addr)).val.Scope_id));
                addrs = append(addrs, new IPAddr(IP: copyIP(a[..]), Zone: zone));
                break;
            }
            default: {
                return (default!, ~newDNSError(syscall.EWINDOWS, name, ""u8));
            }}

        }
        return (addrs, default!);
    };
    channel<ret> ch = default!;
    if (ctx.Err() == default!) {
        ch = new channel<ret>(1);
        var chʗ1 = ch;
        var getaddrʗ1 = getaddr;
        goǃ(() => {
            (addr, err) = getaddrʗ1();
            chʗ1.ᐸꟷ(new ret(addrs: addr, err: err));
        });
    }
    switch (select(ᐸꟷ(ch, ꓸꓸꓸ), ᐸꟷ(ctx.Done(), ꓸꓸꓸ))) {
    case 0 when ch.ꟷᐳ(out var rΔ1): {
        return (rΔ1.addrs, rΔ1.err);
    }
    case 1 when ctx.Done().ꟷᐳ(out _): {
        return (default!, ~newDNSError(mapErr(ctx.Err()), // TODO(bradfitz,brainman): cancel the ongoing
 // GetAddrInfoW? It would require conditionally using
 // GetAddrInfoEx with lpOverlapped, which requires
 // Windows 8 or newer. I guess we'll need oldLookupIP,
 // newLookupIP, and newerLookUP.
 //
 // For now we just let it finish and write to the
 // buffered channel.
 name, ""u8));
    }}
});

[GoRecv] internal static (nint, error) lookupPort(this ref Resolver r, context.Context ctx, @string network, @string service) => func((defer, _) => {
    if (systemConf().mustUseGoResolver(r)) {
        return lookupPortMap(network, service);
    }
    // TODO(bradfitz): finish ctx plumbing
    {
        var err = acquireThread(ctx); if (err != default!) {
            return (0, new DNSError(
                Name: network + "/"u8 + service,
                Err: mapErr(err).Error(),
                IsTimeout: AreEqual(ctx.Err(), context.DeadlineExceeded)
            ));
        }
    }
    defer(releaseThread);
    ref var hints = ref heap(new syscall_package.AddrinfoW(), out var Ꮡhints);
    var exprᴛ1 = network;
    if (exprᴛ1 == "ip"u8) {
    }
    else if (exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8) {
        hints.Socktype = syscall.SOCK_STREAM;
        hints.Protocol = syscall.IPPROTO_TCP;
    }
    else if (exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8) {
        hints.Socktype = syscall.SOCK_DGRAM;
        hints.Protocol = syscall.IPPROTO_UDP;
    }
    else { /* default: */
        return (0, new DNSError( // no hints
Err: "unknown network"u8, Name: network + "/"u8 + service));
    }

    switch (ipVersion(network)) {
    case (rune)'4': {
        hints.Family = syscall.AF_INET;
        break;
    }
    case (rune)'6': {
        hints.Family = syscall.AF_INET6;
        break;
    }}

    ж<syscall.AddrinfoW> result = default!;
    var e = syscall.GetAddrInfoW(nil, syscall.StringToUTF16Ptr(service), Ꮡhints, Ꮡ(result));
    if (e != default!) {
        {
            var (port, err) = lookupPortMap(network, service); if (err == default!) {
                return (port, default!);
            }
        }
        // The _WSATYPE_NOT_FOUND error is returned by GetAddrInfoW
        // when the service name is unknown. We are also checking
        // for _WSAHOST_NOT_FOUND here to match the cgo (unix) version
        // cgo_unix.go (cgoLookupServicePort).
        if (e == _WSATYPE_NOT_FOUND || e == _WSAHOST_NOT_FOUND) {
            return (0, ~newDNSError(~errUnknownPort, network + "/"u8 + service, ""u8));
        }
        return (0, ~newDNSError(winError("getaddrinfow"u8, e), network + "/"u8 + service, ""u8));
    }
    deferǃ(syscall.FreeAddrInfoW, result, defer);
    if (result == nil) {
        return (0, ~newDNSError(syscall.EINVAL, network + "/"u8 + service, ""u8));
    }
    @unsafe.Pointer addr = ((@unsafe.Pointer)(~result).Addr);
    switch ((~result).Family) {
    case syscall.AF_INET: {
        var a = (ж<syscall.RawSockaddrInet4>)(uintptr)(addr);
        return (((nint)syscall.Ntohs((~a).Port)), default!);
    }
    case syscall.AF_INET6: {
        var a = (ж<syscall.RawSockaddrInet6>)(uintptr)(addr);
        return (((nint)syscall.Ntohs((~a).Port)), default!);
    }}

    return (0, ~newDNSError(syscall.EINVAL, network + "/"u8 + service, ""u8));
});

[GoRecv] internal static (@string, error) lookupCNAME(this ref Resolver r, context.Context ctx, @string name) => func((defer, _) => {
    {
        var (order, conf) = systemConf().hostLookupOrder(r, name); if (order != hostLookupCgo) {
            return r.goLookupCNAME(ctx, name, order, conf);
        }
    }
    // TODO(bradfitz): finish ctx plumbing
    {
        var err = acquireThread(ctx); if (err != default!) {
            return ("", new DNSError(
                Name: name,
                Err: mapErr(err).Error(),
                IsTimeout: AreEqual(ctx.Err(), context.DeadlineExceeded)
            ));
        }
    }
    defer(releaseThread);
    ж<syscall.DNSRecord> rec = default!;
    var e = syscall.DnsQuery(name, syscall.DNS_TYPE_CNAME, 0, nil, Ꮡ(rec), nil);
    // windows returns DNS_INFO_NO_RECORDS if there are no CNAME-s
    {
        var (errno, ok) = e._<syscall.Errno>(ᐧ); if (ok && errno == syscall.DNS_INFO_NO_RECORDS) {
            // if there are no aliases, the canonical name is the input name
            return (absDomainName(name), default!);
        }
    }
    if (e != default!) {
        return ("", ~newDNSError(winError("dnsquery"u8, e), name, ""u8));
    }
    deferǃ(syscall.DnsRecordListFree, rec, 1, defer);
    var resolved = resolveCNAME(syscall.StringToUTF16Ptr(name), rec);
    @string cname = windows.UTF16PtrToString(resolved);
    return (absDomainName(cname), default!);
});

[GoRecv] internal static (@string, slice<ж<SRV>>, error) lookupSRV(this ref Resolver r, context.Context ctx, @string service, @string proto, @string name) => func((defer, _) => {
    if (systemConf().mustUseGoResolver(r)) {
        return r.goLookupSRV(ctx, service, proto, name);
    }
    // TODO(bradfitz): finish ctx plumbing
    {
        var err = acquireThread(ctx); if (err != default!) {
            return ("", default!, new DNSError(
                Name: name,
                Err: mapErr(err).Error(),
                IsTimeout: AreEqual(ctx.Err(), context.DeadlineExceeded)
            ));
        }
    }
    defer(releaseThread);
    @string target = default!;
    if (service == ""u8 && proto == ""u8){
        target = name;
    } else {
        target = "_"u8 + service + "._"u8 + proto + "."u8 + name;
    }
    ж<syscall.DNSRecord> rec = default!;
    var e = syscall.DnsQuery(target, syscall.DNS_TYPE_SRV, 0, nil, Ꮡ(rec), nil);
    if (e != default!) {
        return ("", default!, ~newDNSError(winError("dnsquery"u8, e), name, ""u8));
    }
    deferǃ(syscall.DnsRecordListFree, rec, 1, defer);
    var srvs = new slice<ж<SRV>>(0, 10);
    foreach (var (_, p) in validRecs(rec, syscall.DNS_TYPE_SRV, target)) {
        var v = (ж<syscall.DNSSRVData>)(uintptr)(new @unsafe.Pointer(Ꮡ(~p).Data.at<byte>(0)));
        srvs = append(srvs, Ꮡ(new SRV(absDomainName(syscall.UTF16ToString((ж<array<uint16>>)(uintptr)(new @unsafe.Pointer((~v).Target))[..])), (~v).Port, (~v).Priority, (~v).Weight)));
    }
    ((byPriorityWeight)srvs).sort();
    return (absDomainName(target), srvs, default!);
});

[GoRecv] internal static (slice<ж<MX>>, error) lookupMX(this ref Resolver r, context.Context ctx, @string name) => func((defer, _) => {
    if (systemConf().mustUseGoResolver(r)) {
        return r.goLookupMX(ctx, name);
    }
    // TODO(bradfitz): finish ctx plumbing.
    {
        var err = acquireThread(ctx); if (err != default!) {
            return (default!, new DNSError(
                Name: name,
                Err: mapErr(err).Error(),
                IsTimeout: AreEqual(ctx.Err(), context.DeadlineExceeded)
            ));
        }
    }
    defer(releaseThread);
    ж<syscall.DNSRecord> rec = default!;
    var e = syscall.DnsQuery(name, syscall.DNS_TYPE_MX, 0, nil, Ꮡ(rec), nil);
    if (e != default!) {
        return (default!, ~newDNSError(winError("dnsquery"u8, e), name, ""u8));
    }
    deferǃ(syscall.DnsRecordListFree, rec, 1, defer);
    var mxs = new slice<ж<MX>>(0, 10);
    foreach (var (_, p) in validRecs(rec, syscall.DNS_TYPE_MX, name)) {
        var v = (ж<syscall.DNSMXData>)(uintptr)(new @unsafe.Pointer(Ꮡ(~p).Data.at<byte>(0)));
        mxs = append(mxs, Ꮡ(new MX(absDomainName(windows.UTF16PtrToString((~v).NameExchange)), (~v).Preference)));
    }
    ((byPref)mxs).sort();
    return (mxs, default!);
});

[GoRecv] internal static (slice<ж<NS>>, error) lookupNS(this ref Resolver r, context.Context ctx, @string name) => func((defer, _) => {
    if (systemConf().mustUseGoResolver(r)) {
        return r.goLookupNS(ctx, name);
    }
    // TODO(bradfitz): finish ctx plumbing.
    {
        var err = acquireThread(ctx); if (err != default!) {
            return (default!, new DNSError(
                Name: name,
                Err: mapErr(err).Error(),
                IsTimeout: AreEqual(ctx.Err(), context.DeadlineExceeded)
            ));
        }
    }
    defer(releaseThread);
    ж<syscall.DNSRecord> rec = default!;
    var e = syscall.DnsQuery(name, syscall.DNS_TYPE_NS, 0, nil, Ꮡ(rec), nil);
    if (e != default!) {
        return (default!, ~newDNSError(winError("dnsquery"u8, e), name, ""u8));
    }
    deferǃ(syscall.DnsRecordListFree, rec, 1, defer);
    var nss = new slice<ж<NS>>(0, 10);
    foreach (var (_, p) in validRecs(rec, syscall.DNS_TYPE_NS, name)) {
        var v = (ж<syscall.DNSPTRData>)(uintptr)(new @unsafe.Pointer(Ꮡ(~p).Data.at<byte>(0)));
        nss = append(nss, Ꮡ(new NS(absDomainName(syscall.UTF16ToString((ж<array<uint16>>)(uintptr)(new @unsafe.Pointer((~v).Host))[..])))));
    }
    return (nss, default!);
});

[GoRecv] internal static unsafe (slice<@string>, error) lookupTXT(this ref Resolver r, context.Context ctx, @string name) => func((defer, _) => {
    if (systemConf().mustUseGoResolver(r)) {
        return r.goLookupTXT(ctx, name);
    }
    // TODO(bradfitz): finish ctx plumbing.
    {
        var err = acquireThread(ctx); if (err != default!) {
            return (default!, new DNSError(
                Name: name,
                Err: mapErr(err).Error(),
                IsTimeout: AreEqual(ctx.Err(), context.DeadlineExceeded)
            ));
        }
    }
    defer(releaseThread);
    ж<syscall.DNSRecord> rec = default!;
    var e = syscall.DnsQuery(name, syscall.DNS_TYPE_TEXT, 0, nil, Ꮡ(rec), nil);
    if (e != default!) {
        return (default!, ~newDNSError(winError("dnsquery"u8, e), name, ""u8));
    }
    deferǃ(syscall.DnsRecordListFree, rec, 1, defer);
    var txts = new slice<@string>(0, 10);
    foreach (var (_, p) in validRecs(rec, syscall.DNS_TYPE_TEXT, name)) {
        var d = (ж<syscall.DNSTXTData>)(uintptr)(new @unsafe.Pointer(Ꮡ(~p).Data.at<byte>(0)));
        @string s = ""u8;
        foreach (var (_, v) in new Span<ж<uint16>>((uint16**)(uintptr)(((@unsafe.Pointer)(Ꮡ(((~d).StringArray[0]))))), (~d).StringCount)) {
            s += windows.UTF16PtrToString(v);
        }
        txts = append(txts, s);
    }
    return (txts, default!);
});

[GoRecv] internal static (slice<@string>, error) lookupAddr(this ref Resolver r, context.Context ctx, @string addr) => func((defer, _) => {
    {
        var (order, conf) = systemConf().addrLookupOrder(r, addr); if (order != hostLookupCgo) {
            return r.goLookupPTR(ctx, addr, order, conf);
        }
    }
    // TODO(bradfitz): finish ctx plumbing.
    {
        var errΔ1 = acquireThread(ctx); if (errΔ1 != default!) {
            return (default!, new DNSError(
                Name: addr,
                Err: mapErr(errΔ1).Error(),
                IsTimeout: AreEqual(ctx.Err(), context.DeadlineExceeded)
            ));
        }
    }
    defer(releaseThread);
    var (arpa, err) = reverseaddr(addr);
    if (err != default!) {
        return (default!, err);
    }
    ж<syscall.DNSRecord> rec = default!;
    var e = syscall.DnsQuery(arpa, syscall.DNS_TYPE_PTR, 0, nil, Ꮡ(rec), nil);
    if (e != default!) {
        return (default!, ~newDNSError(winError("dnsquery"u8, e), addr, ""u8));
    }
    deferǃ(syscall.DnsRecordListFree, rec, 1, defer);
    var ptrs = new slice<@string>(0, 10);
    foreach (var (_, p) in validRecs(rec, syscall.DNS_TYPE_PTR, arpa)) {
        var v = (ж<syscall.DNSPTRData>)(uintptr)(new @unsafe.Pointer(Ꮡ(~p).Data.at<byte>(0)));
        ptrs = append(ptrs, absDomainName(windows.UTF16PtrToString((~v).Host)));
    }
    return (ptrs, default!);
});

internal static readonly UntypedInt dnsSectionMask = /* 0x0003 */ 3;

// returns only results applicable to name and resolves CNAME entries.
internal static slice<ж<syscall.DNSRecord>> validRecs(ж<syscall.DNSRecord> Ꮡr, uint16 dnstype, @string name) {
    ref var r = ref Ꮡr.val;

    var cname = syscall.StringToUTF16Ptr(name);
    if (dnstype != syscall.DNS_TYPE_CNAME) {
        cname = resolveCNAME(cname, Ꮡr);
    }
    var rec = new slice<ж<syscall.DNSRecord>>(0, 10);
    for (var p = r; p != nil; p = p.val.Next) {
        // in case of a local machine, DNS records are returned with DNSREC_QUESTION flag instead of DNS_ANSWER
        if ((uint32)((~p).Dw & dnsSectionMask) != syscall.DnsSectionAnswer && (uint32)((~p).Dw & dnsSectionMask) != syscall.DnsSectionQuestion) {
            continue;
        }
        if ((~p).Type != dnstype) {
            continue;
        }
        if (!syscall.DnsNameCompare(cname, (~p).Name)) {
            continue;
        }
        rec = append(rec, p);
    }
    return rec;
}

// returns the last CNAME in chain.
internal static ж<uint16> resolveCNAME(ж<uint16> Ꮡname, ж<syscall.DNSRecord> Ꮡr) {
    ref var name = ref Ꮡname.val;
    ref var r = ref Ꮡr.val;

    // limit cname resolving to 10 in case of an infinite CNAME loop
Cname:
    for (nint cnameloop = 0; cnameloop < 10; cnameloop++) {
        for (var p = r; p != nil; p = p.val.Next) {
            if ((uint32)((~p).Dw & dnsSectionMask) != syscall.DnsSectionAnswer) {
                continue;
            }
            if ((~p).Type != syscall.DNS_TYPE_CNAME) {
                continue;
            }
            if (!syscall.DnsNameCompare(Ꮡname, (~p).Name)) {
                continue;
            }
            name = ((ж<syscall.DNSPTRData>)(uintptr)(new @unsafe.Pointer(Ꮡr.Data.at<byte>(0)))).val.Host;
            goto continue_Cname;
        }
        break;
continue_Cname:;
    }
break_Cname:;
    return Ꮡname;
}

// concurrentThreadsLimit returns the number of threads we permit to
// run concurrently doing DNS lookups.
internal static nint concurrentThreadsLimit() {
    return 500;
}

} // end net_package
