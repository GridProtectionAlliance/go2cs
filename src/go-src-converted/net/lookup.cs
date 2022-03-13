// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 13 05:29:55 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\lookup.go
namespace go;

using context = context_package;
using nettrace = @internal.nettrace_package;
using singleflight = @internal.singleflight_package;
using sync = sync_package;


// protocols contains minimal mappings between internet protocol
// names and numbers for platforms that don't have a complete list of
// protocol numbers.
//
// See https://www.iana.org/assignments/protocol-numbers
//
// On Unix, this map is augmented by readProtocols via lookupProtocol.

using System;
using System.Threading;
public static partial class net_package {

private static map protocols = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, nint>{"icmp":1,"igmp":2,"tcp":6,"udp":17,"ipv6-icmp":58,};

// services contains minimal mappings between services names and port
// numbers for platforms that don't have a complete list of port numbers.
//
// See https://www.iana.org/assignments/service-names-port-numbers
//
// On Unix, this map is augmented by readServices via goLookupPort.
private static map services = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, map<@string, nint>>{"udp":{"domain":53,},"tcp":{"ftp":21,"ftps":990,"gopher":70,"http":80,"https":443,"imap2":143,"imap3":220,"imaps":993,"pop3":110,"pop3s":995,"smtp":25,"ssh":22,"telnet":23,},};

// dnsWaitGroup can be used by tests to wait for all DNS goroutines to
// complete. This avoids races on the test hooks.
private static sync.WaitGroup dnsWaitGroup = default;

private static readonly var maxProtoLength = len("RSVP-E2E-IGNORE") + 10; // with room to grow

 // with room to grow

private static (nint, error) lookupProtocolMap(@string name) {
    nint _p0 = default;
    error _p0 = default!;

    array<byte> lowerProtocol = new array<byte>(maxProtoLength);
    var n = copy(lowerProtocol[..], name);
    lowerASCIIBytes(lowerProtocol[..(int)n]);
    var (proto, found) = protocols[string(lowerProtocol[..(int)n])];
    if (!found || n != len(name)) {
        return (0, error.As(addr(new AddrError(Err:"unknown IP protocol specified",Addr:name))!)!);
    }
    return (proto, error.As(null!)!);
}

// maxPortBufSize is the longest reasonable name of a service
// (non-numeric port).
// Currently the longest known IANA-unregistered name is
// "mobility-header", so we use that length, plus some slop in case
// something longer is added in the future.
private static readonly var maxPortBufSize = len("mobility-header") + 10;



private static (nint, error) lookupPortMap(@string network, @string service) {
    nint port = default;
    error error = default!;

    switch (network) {
        case "tcp4": 

        case "tcp6": 
            network = "tcp";
            break;
        case "udp4": 

        case "udp6": 
            network = "udp";
            break;
    }

    {
        var (m, ok) = services[network];

        if (ok) {
            array<byte> lowerService = new array<byte>(maxPortBufSize);
            var n = copy(lowerService[..], service);
            lowerASCIIBytes(lowerService[..(int)n]);
            {
                var (port, ok) = m[string(lowerService[..(int)n])];

                if (ok && n == len(service)) {
                    return (port, error.As(null!)!);
                }

            }
        }
    }
    return (0, error.As(addr(new AddrError(Err:"unknown port",Addr:network+"/"+service))!)!);
}

// ipVersion returns the provided network's IP version: '4', '6' or 0
// if network does not end in a '4' or '6' byte.
private static byte ipVersion(@string network) {
    if (network == "") {
        return 0;
    }
    var n = network[len(network) - 1];
    if (n != '4' && n != '6') {
        n = 0;
    }
    return n;
}

// DefaultResolver is the resolver used by the package-level Lookup
// functions and by Dialers without a specified Resolver.
public static ptr<Resolver> DefaultResolver = addr(new Resolver());

// A Resolver looks up names and numbers.
//
// A nil *Resolver is equivalent to a zero Resolver.
public partial struct Resolver {
    public bool PreferGo; // StrictErrors controls the behavior of temporary errors
// (including timeout, socket errors, and SERVFAIL) when using
// Go's built-in resolver. For a query composed of multiple
// sub-queries (such as an A+AAAA address lookup, or walking the
// DNS search list), this option causes such errors to abort the
// whole query instead of returning a partial result. This is
// not enabled by default because it may affect compatibility
// with resolvers that process AAAA queries incorrectly.
    public bool StrictErrors; // Dial optionally specifies an alternate dialer for use by
// Go's built-in DNS resolver to make TCP and UDP connections
// to DNS services. The host in the address parameter will
// always be a literal IP address and not a host name, and the
// port in the address parameter will be a literal port number
// and not a service name.
// If the Conn returned is also a PacketConn, sent and received DNS
// messages must adhere to RFC 1035 section 4.2.1, "UDP usage".
// Otherwise, DNS messages transmitted over Conn must adhere
// to RFC 7766 section 5, "Transport Protocol Selection".
// If nil, the default dialer is used.
    public Func<context.Context, @string, @string, (Conn, error)> Dial; // lookupGroup merges LookupIPAddr calls together for lookups for the same
// host. The lookupGroup key is the LookupIPAddr.host argument.
// The return values are ([]IPAddr, error).
    public singleflight.Group lookupGroup; // TODO(bradfitz): optional interface impl override hook
// TODO(bradfitz): Timeout time.Duration?
}

private static bool preferGo(this ptr<Resolver> _addr_r) {
    ref Resolver r = ref _addr_r.val;

    return r != null && r.PreferGo;
}
private static bool strictErrors(this ptr<Resolver> _addr_r) {
    ref Resolver r = ref _addr_r.val;

    return r != null && r.StrictErrors;
}

private static ptr<singleflight.Group> getLookupGroup(this ptr<Resolver> _addr_r) {
    ref Resolver r = ref _addr_r.val;

    if (r == null) {
        return _addr__addr_DefaultResolver.lookupGroup!;
    }
    return _addr__addr_r.lookupGroup!;
}

// LookupHost looks up the given host using the local resolver.
// It returns a slice of that host's addresses.
//
// LookupHost uses context.Background internally; to specify the context, use
// Resolver.LookupHost.
public static (slice<@string>, error) LookupHost(@string host) {
    slice<@string> addrs = default;
    error err = default!;

    return DefaultResolver.LookupHost(context.Background(), host);
}

// LookupHost looks up the given host using the local resolver.
// It returns a slice of that host's addresses.
private static (slice<@string>, error) LookupHost(this ptr<Resolver> _addr_r, context.Context ctx, @string host) {
    slice<@string> addrs = default;
    error err = default!;
    ref Resolver r = ref _addr_r.val;
 
    // Make sure that no matter what we do later, host=="" is rejected.
    // parseIP, for example, does accept empty strings.
    if (host == "") {
        return (null, error.As(addr(new DNSError(Err:errNoSuchHost.Error(),Name:host,IsNotFound:true))!)!);
    }
    {
        var (ip, _) = parseIPZone(host);

        if (ip != null) {
            return (new slice<@string>(new @string[] { host }), error.As(null!)!);
        }
    }
    return r.lookupHost(ctx, host);
}

// LookupIP looks up host using the local resolver.
// It returns a slice of that host's IPv4 and IPv6 addresses.
public static (slice<IP>, error) LookupIP(@string host) {
    slice<IP> _p0 = default;
    error _p0 = default!;

    var (addrs, err) = DefaultResolver.LookupIPAddr(context.Background(), host);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var ips = make_slice<IP>(len(addrs));
    foreach (var (i, ia) in addrs) {
        ips[i] = ia.IP;
    }    return (ips, error.As(null!)!);
}

// LookupIPAddr looks up host using the local resolver.
// It returns a slice of that host's IPv4 and IPv6 addresses.
private static (slice<IPAddr>, error) LookupIPAddr(this ptr<Resolver> _addr_r, context.Context ctx, @string host) {
    slice<IPAddr> _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;

    return r.lookupIPAddr(ctx, "ip", host);
}

// LookupIP looks up host for the given network using the local resolver.
// It returns a slice of that host's IP addresses of the type specified by
// network.
// network must be one of "ip", "ip4" or "ip6".
private static (slice<IP>, error) LookupIP(this ptr<Resolver> _addr_r, context.Context ctx, @string network, @string host) {
    slice<IP> _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;

    var (afnet, _, err) = parseNetwork(ctx, network, false);
    if (err != null) {
        return (null, error.As(err)!);
    }
    switch (afnet) {
        case "ip": 

        case "ip4": 

        case "ip6": 

            break;
        default: 
            return (null, error.As(UnknownNetworkError(network))!);
            break;
    }
    var (addrs, err) = r.internetAddrList(ctx, afnet, host);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var ips = make_slice<IP>(0, len(addrs));
    foreach (var (_, addr) in addrs) {
        ips = append(ips, addr._<ptr<IPAddr>>().IP);
    }    return (ips, error.As(null!)!);
}

// onlyValuesCtx is a context that uses an underlying context
// for value lookup if the underlying context hasn't yet expired.
private partial struct onlyValuesCtx : context.Context {
    public ref context.Context Context => ref Context_val;
    public context.Context lookupValues;
}

private static context.Context _ = (onlyValuesCtx.val)(null);

// Value performs a lookup if the original context hasn't expired.
private static void Value(this ptr<onlyValuesCtx> _addr_ovc, object key) {
    ref onlyValuesCtx ovc = ref _addr_ovc.val;

    return null;
    return ovc.lookupValues.Value(key);
}

// withUnexpiredValuesPreserved returns a context.Context that only uses lookupCtx
// for its values, otherwise it is never canceled and has no deadline.
// If the lookup context expires, any looked up values will return nil.
// See Issue 28600.
private static context.Context withUnexpiredValuesPreserved(context.Context lookupCtx) {
    return addr(new onlyValuesCtx(Context:context.Background(),lookupValues:lookupCtx));
}

// lookupIPAddr looks up host using the local resolver and particular network.
// It returns a slice of that host's IPv4 and IPv6 addresses.
private static (slice<IPAddr>, error) lookupIPAddr(this ptr<Resolver> _addr_r, context.Context ctx, @string network, @string host) => func((defer, _, _) => {
    slice<IPAddr> _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;
 
    // Make sure that no matter what we do later, host=="" is rejected.
    // parseIP, for example, does accept empty strings.
    if (host == "") {
        return (null, error.As(addr(new DNSError(Err:errNoSuchHost.Error(),Name:host,IsNotFound:true))!)!);
    }
    {
        var (ip, zone) = parseIPZone(host);

        if (ip != null) {
            return (new slice<IPAddr>(new IPAddr[] { {IP:ip,Zone:zone} }), error.As(null!)!);
        }
    }
    ptr<nettrace.Trace> (trace, _) = ctx.Value(new nettrace.TraceKey())._<ptr<nettrace.Trace>>();
    if (trace != null && trace.DNSStart != null) {
        trace.DNSStart(host);
    }
    var resolverFunc = r.lookupIP;
    {
        Func<context.Context, @string, @string, (slice<IPAddr>, error)> (alt, _) = ctx.Value(new nettrace.LookupIPAltResolverKey())._<Func<context.Context, @string, @string, (slice<IPAddr>, error)>>();

        if (alt != null) {
            resolverFunc = alt;
        }
    } 

    // We don't want a cancellation of ctx to affect the
    // lookupGroup operation. Otherwise if our context gets
    // canceled it might cause an error to be returned to a lookup
    // using a completely different context. However we need to preserve
    // only the values in context. See Issue 28600.
    var (lookupGroupCtx, lookupGroupCancel) = context.WithCancel(withUnexpiredValuesPreserved(ctx));

    var lookupKey = network + " " + host;
    dnsWaitGroup.Add(1);
    var (ch, called) = r.getLookupGroup().DoChan(lookupKey, () => {
        defer(dnsWaitGroup.Done());
        return testHookLookupIP(lookupGroupCtx, resolverFunc, network, host);
    });
    if (!called) {
        dnsWaitGroup.Done();
    }
    if (r.getLookupGroup().ForgetUnshared(lookupKey)) {
        lookupGroupCancel();
    }
    else
 {
        go_(() => () => {
            ch.Receive();
            lookupGroupCancel();
        }());
    }
    var err = mapErr(ctx.Err());
    if (trace != null && trace.DNSDone != null) {
        trace.DNSDone(null, false, err);
    }
    return (null, error.As(err)!);
    lookupGroupCancel();
    if (trace != null && trace.DNSDone != null) {
        slice<IPAddr> (addrs, _) = r.Val._<slice<IPAddr>>();
        trace.DNSDone(ipAddrsEface(addrs), r.Shared, r.Err);
    }
    return lookupIPReturn(r.Val, r.Err, r.Shared);
});

// lookupIPReturn turns the return values from singleflight.Do into
// the return values from LookupIP.
private static (slice<IPAddr>, error) lookupIPReturn(object addrsi, error err, bool shared) {
    slice<IPAddr> _p0 = default;
    error _p0 = default!;

    if (err != null) {
        return (null, error.As(err)!);
    }
    slice<IPAddr> addrs = addrsi._<slice<IPAddr>>();
    if (shared) {
        var clone = make_slice<IPAddr>(len(addrs));
        copy(clone, addrs);
        addrs = clone;
    }
    return (addrs, error.As(null!)!);
}

// ipAddrsEface returns an empty interface slice of addrs.
private static slice<object> ipAddrsEface(slice<IPAddr> addrs) {
    var s = make_slice<object>(len(addrs));
    foreach (var (i, v) in addrs) {
        s[i] = v;
    }    return s;
}

// LookupPort looks up the port for the given network and service.
//
// LookupPort uses context.Background internally; to specify the context, use
// Resolver.LookupPort.
public static (nint, error) LookupPort(@string network, @string service) {
    nint port = default;
    error err = default!;

    return DefaultResolver.LookupPort(context.Background(), network, service);
}

// LookupPort looks up the port for the given network and service.
private static (nint, error) LookupPort(this ptr<Resolver> _addr_r, context.Context ctx, @string network, @string service) {
    nint port = default;
    error err = default!;
    ref Resolver r = ref _addr_r.val;

    var (port, needsLookup) = parsePort(service);
    if (needsLookup) {
        switch (network) {
            case "tcp": 

            case "tcp4": 

            case "tcp6": 

            case "udp": 

            case "udp4": 

            case "udp6": 

                break;
            case "": // a hint wildcard for Go 1.0 undocumented behavior
                network = "ip";
                break;
            default: 
                return (0, error.As(addr(new AddrError(Err:"unknown network",Addr:network))!)!);
                break;
        }
        port, err = r.lookupPort(ctx, network, service);
        if (err != null) {
            return (0, error.As(err)!);
        }
    }
    if (0 > port || port > 65535) {
        return (0, error.As(addr(new AddrError(Err:"invalid port",Addr:service))!)!);
    }
    return (port, error.As(null!)!);
}

// LookupCNAME returns the canonical name for the given host.
// Callers that do not care about the canonical name can call
// LookupHost or LookupIP directly; both take care of resolving
// the canonical name as part of the lookup.
//
// A canonical name is the final name after following zero
// or more CNAME records.
// LookupCNAME does not return an error if host does not
// contain DNS "CNAME" records, as long as host resolves to
// address records.
//
// The returned canonical name is validated to be a properly
// formatted presentation-format domain name.
//
// LookupCNAME uses context.Background internally; to specify the context, use
// Resolver.LookupCNAME.
public static (@string, error) LookupCNAME(@string host) {
    @string cname = default;
    error err = default!;

    return DefaultResolver.LookupCNAME(context.Background(), host);
}

// LookupCNAME returns the canonical name for the given host.
// Callers that do not care about the canonical name can call
// LookupHost or LookupIP directly; both take care of resolving
// the canonical name as part of the lookup.
//
// A canonical name is the final name after following zero
// or more CNAME records.
// LookupCNAME does not return an error if host does not
// contain DNS "CNAME" records, as long as host resolves to
// address records.
//
// The returned canonical name is validated to be a properly
// formatted presentation-format domain name.
private static (@string, error) LookupCNAME(this ptr<Resolver> _addr_r, context.Context ctx, @string host) {
    @string _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;

    var (cname, err) = r.lookupCNAME(ctx, host);
    if (err != null) {
        return ("", error.As(err)!);
    }
    if (!isDomainName(cname)) {
        return ("", error.As(addr(new DNSError(Err:errMalformedDNSRecordsDetail,Name:host))!)!);
    }
    return (cname, error.As(null!)!);
}

// LookupSRV tries to resolve an SRV query of the given service,
// protocol, and domain name. The proto is "tcp" or "udp".
// The returned records are sorted by priority and randomized
// by weight within a priority.
//
// LookupSRV constructs the DNS name to look up following RFC 2782.
// That is, it looks up _service._proto.name. To accommodate services
// publishing SRV records under non-standard names, if both service
// and proto are empty strings, LookupSRV looks up name directly.
//
// The returned service names are validated to be properly
// formatted presentation-format domain names. If the response contains
// invalid names, those records are filtered out and an error
// will be returned alongside the the remaining results, if any.
public static (@string, slice<ptr<SRV>>, error) LookupSRV(@string service, @string proto, @string name) {
    @string cname = default;
    slice<ptr<SRV>> addrs = default;
    error err = default!;

    return DefaultResolver.LookupSRV(context.Background(), service, proto, name);
}

// LookupSRV tries to resolve an SRV query of the given service,
// protocol, and domain name. The proto is "tcp" or "udp".
// The returned records are sorted by priority and randomized
// by weight within a priority.
//
// LookupSRV constructs the DNS name to look up following RFC 2782.
// That is, it looks up _service._proto.name. To accommodate services
// publishing SRV records under non-standard names, if both service
// and proto are empty strings, LookupSRV looks up name directly.
//
// The returned service names are validated to be properly
// formatted presentation-format domain names. If the response contains
// invalid names, those records are filtered out and an error
// will be returned alongside the the remaining results, if any.
private static (@string, slice<ptr<SRV>>, error) LookupSRV(this ptr<Resolver> _addr_r, context.Context ctx, @string service, @string proto, @string name) {
    @string _p0 = default;
    slice<ptr<SRV>> _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;

    var (cname, addrs, err) = r.lookupSRV(ctx, service, proto, name);
    if (err != null) {
        return ("", null, error.As(err)!);
    }
    if (cname != "" && !isDomainName(cname)) {
        return ("", null, error.As(addr(new DNSError(Err:"SRV header name is invalid",Name:name))!)!);
    }
    var filteredAddrs = make_slice<ptr<SRV>>(0, len(addrs));
    foreach (var (_, addr) in addrs) {
        if (addr == null) {
            continue;
        }
        if (!isDomainName(addr.Target)) {
            continue;
        }
        filteredAddrs = append(filteredAddrs, addr);
    }    if (len(addrs) != len(filteredAddrs)) {
        return (cname, filteredAddrs, error.As(addr(new DNSError(Err:errMalformedDNSRecordsDetail,Name:name))!)!);
    }
    return (cname, filteredAddrs, error.As(null!)!);
}

// LookupMX returns the DNS MX records for the given domain name sorted by preference.
//
// The returned mail server names are validated to be properly
// formatted presentation-format domain names. If the response contains
// invalid names, those records are filtered out and an error
// will be returned alongside the the remaining results, if any.
//
// LookupMX uses context.Background internally; to specify the context, use
// Resolver.LookupMX.
public static (slice<ptr<MX>>, error) LookupMX(@string name) {
    slice<ptr<MX>> _p0 = default;
    error _p0 = default!;

    return DefaultResolver.LookupMX(context.Background(), name);
}

// LookupMX returns the DNS MX records for the given domain name sorted by preference.
//
// The returned mail server names are validated to be properly
// formatted presentation-format domain names. If the response contains
// invalid names, those records are filtered out and an error
// will be returned alongside the the remaining results, if any.
private static (slice<ptr<MX>>, error) LookupMX(this ptr<Resolver> _addr_r, context.Context ctx, @string name) {
    slice<ptr<MX>> _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;

    var (records, err) = r.lookupMX(ctx, name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var filteredMX = make_slice<ptr<MX>>(0, len(records));
    foreach (var (_, mx) in records) {
        if (mx == null) {
            continue;
        }
        if (mx.Host != "." && !isDomainName(mx.Host)) {
            continue;
        }
        filteredMX = append(filteredMX, mx);
    }    if (len(records) != len(filteredMX)) {
        return (filteredMX, error.As(addr(new DNSError(Err:errMalformedDNSRecordsDetail,Name:name))!)!);
    }
    return (filteredMX, error.As(null!)!);
}

// LookupNS returns the DNS NS records for the given domain name.
//
// The returned name server names are validated to be properly
// formatted presentation-format domain names. If the response contains
// invalid names, those records are filtered out and an error
// will be returned alongside the the remaining results, if any.
//
// LookupNS uses context.Background internally; to specify the context, use
// Resolver.LookupNS.
public static (slice<ptr<NS>>, error) LookupNS(@string name) {
    slice<ptr<NS>> _p0 = default;
    error _p0 = default!;

    return DefaultResolver.LookupNS(context.Background(), name);
}

// LookupNS returns the DNS NS records for the given domain name.
//
// The returned name server names are validated to be properly
// formatted presentation-format domain names. If the response contains
// invalid names, those records are filtered out and an error
// will be returned alongside the the remaining results, if any.
private static (slice<ptr<NS>>, error) LookupNS(this ptr<Resolver> _addr_r, context.Context ctx, @string name) {
    slice<ptr<NS>> _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;

    var (records, err) = r.lookupNS(ctx, name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var filteredNS = make_slice<ptr<NS>>(0, len(records));
    foreach (var (_, ns) in records) {
        if (ns == null) {
            continue;
        }
        if (!isDomainName(ns.Host)) {
            continue;
        }
        filteredNS = append(filteredNS, ns);
    }    if (len(records) != len(filteredNS)) {
        return (filteredNS, error.As(addr(new DNSError(Err:errMalformedDNSRecordsDetail,Name:name))!)!);
    }
    return (filteredNS, error.As(null!)!);
}

// LookupTXT returns the DNS TXT records for the given domain name.
//
// LookupTXT uses context.Background internally; to specify the context, use
// Resolver.LookupTXT.
public static (slice<@string>, error) LookupTXT(@string name) {
    slice<@string> _p0 = default;
    error _p0 = default!;

    return DefaultResolver.lookupTXT(context.Background(), name);
}

// LookupTXT returns the DNS TXT records for the given domain name.
private static (slice<@string>, error) LookupTXT(this ptr<Resolver> _addr_r, context.Context ctx, @string name) {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;

    return r.lookupTXT(ctx, name);
}

// LookupAddr performs a reverse lookup for the given address, returning a list
// of names mapping to that address.
//
// The returned names are validated to be properly formatted presentation-format
// domain names. If the response contains invalid names, those records are filtered
// out and an error will be returned alongside the the remaining results, if any.
//
// When using the host C library resolver, at most one result will be
// returned. To bypass the host resolver, use a custom Resolver.
//
// LookupAddr uses context.Background internally; to specify the context, use
// Resolver.LookupAddr.
public static (slice<@string>, error) LookupAddr(@string addr) {
    slice<@string> names = default;
    error err = default!;

    return DefaultResolver.LookupAddr(context.Background(), addr);
}

// LookupAddr performs a reverse lookup for the given address, returning a list
// of names mapping to that address.
//
// The returned names are validated to be properly formatted presentation-format
// domain names. If the response contains invalid names, those records are filtered
// out and an error will be returned alongside the the remaining results, if any.
private static (slice<@string>, error) LookupAddr(this ptr<Resolver> _addr_r, context.Context ctx, @string addr) {
    slice<@string> _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;

    var (names, err) = r.lookupAddr(ctx, addr);
    if (err != null) {
        return (null, error.As(err)!);
    }
    var filteredNames = make_slice<@string>(0, len(names));
    foreach (var (_, name) in names) {
        if (isDomainName(name)) {
            filteredNames = append(filteredNames, name);
        }
    }    if (len(names) != len(filteredNames)) {
        return (filteredNames, error.As(addr(new DNSError(Err:errMalformedDNSRecordsDetail,Name:addr))!)!);
    }
    return (filteredNames, error.As(null!)!);
}

// errMalformedDNSRecordsDetail is the DNSError detail which is returned when a Resolver.Lookup...
// method recieves DNS records which contain invalid DNS names. This may be returned alongside
// results which have had the malformed records filtered out.
private static @string errMalformedDNSRecordsDetail = "DNS response contained records which contain invalid names";

} // end net_package
