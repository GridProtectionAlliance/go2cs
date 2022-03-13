// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 13 05:29:38 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\dial.go
namespace go;

using context = context_package;
using nettrace = @internal.nettrace_package;
using syscall = syscall_package;
using time = time_package;


// defaultTCPKeepAlive is a default constant value for TCPKeepAlive times
// See golang.org/issue/31510

using System;
using System.Threading;
public static partial class net_package {

private static readonly nint defaultTCPKeepAlive = 15 * time.Second;

// A Dialer contains options for connecting to an address.
//
// The zero value for each field is equivalent to dialing
// without that option. Dialing with the zero value of Dialer
// is therefore equivalent to just calling the Dial function.
//
// It is safe to call Dialer's methods concurrently.
public partial struct Dialer {
    public time.Duration Timeout; // Deadline is the absolute point in time after which dials
// will fail. If Timeout is set, it may fail earlier.
// Zero means no deadline, or dependent on the operating system
// as with the Timeout option.
    public time.Time Deadline; // LocalAddr is the local address to use when dialing an
// address. The address must be of a compatible type for the
// network being dialed.
// If nil, a local address is automatically chosen.
    public Addr LocalAddr; // DualStack previously enabled RFC 6555 Fast Fallback
// support, also known as "Happy Eyeballs", in which IPv4 is
// tried soon if IPv6 appears to be misconfigured and
// hanging.
//
// Deprecated: Fast Fallback is enabled by default. To
// disable, set FallbackDelay to a negative value.
    public bool DualStack; // FallbackDelay specifies the length of time to wait before
// spawning a RFC 6555 Fast Fallback connection. That is, this
// is the amount of time to wait for IPv6 to succeed before
// assuming that IPv6 is misconfigured and falling back to
// IPv4.
//
// If zero, a default delay of 300ms is used.
// A negative value disables Fast Fallback support.
    public time.Duration FallbackDelay; // KeepAlive specifies the interval between keep-alive
// probes for an active network connection.
// If zero, keep-alive probes are sent with a default value
// (currently 15 seconds), if supported by the protocol and operating
// system. Network protocols or operating systems that do
// not support keep-alives ignore this field.
// If negative, keep-alive probes are disabled.
    public time.Duration KeepAlive; // Resolver optionally specifies an alternate resolver to use.
    public ptr<Resolver> Resolver; // Cancel is an optional channel whose closure indicates that
// the dial should be canceled. Not all types of dials support
// cancellation.
//
// Deprecated: Use DialContext instead.
    public channel<object> Cancel; // If Control is not nil, it is called after creating the network
// connection but before actually dialing.
//
// Network and address parameters passed to Control method are not
// necessarily the ones passed to Dial. For example, passing "tcp" to Dial
// will cause the Control function to be called with "tcp4" or "tcp6".
    public Func<@string, @string, syscall.RawConn, error> Control;
}

private static bool dualStack(this ptr<Dialer> _addr_d) {
    ref Dialer d = ref _addr_d.val;

    return d.FallbackDelay >= 0;
}

private static time.Time minNonzeroTime(time.Time a, time.Time b) {
    if (a.IsZero()) {
        return b;
    }
    if (b.IsZero() || a.Before(b)) {
        return a;
    }
    return b;
}

// deadline returns the earliest of:
//   - now+Timeout
//   - d.Deadline
//   - the context's deadline
// Or zero, if none of Timeout, Deadline, or context's deadline is set.
private static time.Time deadline(this ptr<Dialer> _addr_d, context.Context ctx, time.Time now) {
    time.Time earliest = default;
    ref Dialer d = ref _addr_d.val;

    if (d.Timeout != 0) { // including negative, for historical reasons
        earliest = now.Add(d.Timeout);
    }
    {
        var (d, ok) = ctx.Deadline();

        if (ok) {
            earliest = minNonzeroTime(earliest, d);
        }
    }
    return minNonzeroTime(earliest, d.Deadline);
}

private static ptr<Resolver> resolver(this ptr<Dialer> _addr_d) {
    ref Dialer d = ref _addr_d.val;

    if (d.Resolver != null) {
        return _addr_d.Resolver!;
    }
    return _addr_DefaultResolver!;
}

// partialDeadline returns the deadline to use for a single address,
// when multiple addresses are pending.
private static (time.Time, error) partialDeadline(time.Time now, time.Time deadline, nint addrsRemaining) {
    time.Time _p0 = default;
    error _p0 = default!;

    if (deadline.IsZero()) {
        return (deadline, error.As(null!)!);
    }
    var timeRemaining = deadline.Sub(now);
    if (timeRemaining <= 0) {
        return (new time.Time(), error.As(errTimeout)!);
    }
    var timeout = timeRemaining / time.Duration(addrsRemaining); 
    // If the time per address is too short, steal from the end of the list.
    const nint saneMinimum = 2 * time.Second;

    if (timeout < saneMinimum) {
        if (timeRemaining < saneMinimum) {
            timeout = timeRemaining;
        }
        else
 {
            timeout = saneMinimum;
        }
    }
    return (now.Add(timeout), error.As(null!)!);
}

private static time.Duration fallbackDelay(this ptr<Dialer> _addr_d) {
    ref Dialer d = ref _addr_d.val;

    if (d.FallbackDelay > 0) {
        return d.FallbackDelay;
    }
    else
 {
        return 300 * time.Millisecond;
    }
}

private static (@string, nint, error) parseNetwork(context.Context ctx, @string network, bool needsProto) {
    @string afnet = default;
    nint proto = default;
    error err = default!;

    var i = last(network, ':');
    if (i < 0) { // no colon
        switch (network) {
            case "tcp": 

            case "tcp4": 

            case "tcp6": 

                break;
            case "udp": 

            case "udp4": 

            case "udp6": 

                break;
            case "ip": 

            case "ip4": 

            case "ip6": 
                if (needsProto) {
                    return ("", 0, error.As(UnknownNetworkError(network))!);
                }
                break;
            case "unix": 

            case "unixgram": 

            case "unixpacket": 

                break;
            default: 
                return ("", 0, error.As(UnknownNetworkError(network))!);
                break;
        }
        return (network, 0, error.As(null!)!);
    }
    afnet = network[..(int)i];
    switch (afnet) {
        case "ip": 

        case "ip4": 

        case "ip6": 
            var protostr = network[(int)i + 1..];
            var (proto, i, ok) = dtoi(protostr);
            if (!ok || i != len(protostr)) {
                proto, err = lookupProtocol(ctx, protostr);
                if (err != null) {
                    return ("", 0, error.As(err)!);
                }
            }
            return (afnet, proto, error.As(null!)!);
            break;
    }
    return ("", 0, error.As(UnknownNetworkError(network))!);
}

// resolveAddrList resolves addr using hint and returns a list of
// addresses. The result contains at least one address when error is
// nil.
private static (addrList, error) resolveAddrList(this ptr<Resolver> _addr_r, context.Context ctx, @string op, @string network, @string addr, Addr hint) {
    addrList _p0 = default;
    error _p0 = default!;
    ref Resolver r = ref _addr_r.val;

    var (afnet, _, err) = parseNetwork(ctx, network, true);
    if (err != null) {
        return (null, error.As(err)!);
    }
    if (op == "dial" && addr == "") {
        return (null, error.As(errMissingAddress)!);
    }
    switch (afnet) {
        case "unix": 

        case "unixgram": 

        case "unixpacket": 
            var (addr, err) = ResolveUnixAddr(afnet, addr);
            if (err != null) {
                return (null, error.As(err)!);
            }
            if (op == "dial" && hint != null && addr.Network() != hint.Network()) {
                return (null, error.As(addr(new AddrError(Err:"mismatched local address type",Addr:hint.String()))!)!);
            }
            return (new addrList(addr), error.As(null!)!);
            break;
    }
    var (addrs, err) = r.internetAddrList(ctx, afnet, addr);
    if (err != null || op != "dial" || hint == null) {
        return (addrs, error.As(err)!);
    }
    ptr<TCPAddr> tcp;    ptr<UDPAddr> udp;    ptr<IPAddr> ip;    bool wildcard = default;
    switch (hint.type()) {
        case ptr<TCPAddr> hint:
            tcp = hint;
            wildcard = tcp.isWildcard();
            break;
        case ptr<UDPAddr> hint:
            udp = hint;
            wildcard = udp.isWildcard();
            break;
        case ptr<IPAddr> hint:
            ip = hint;
            wildcard = ip.isWildcard();
            break;
    }
    var naddrs = addrs[..(int)0];
    {
        var addr__prev1 = addr;

        foreach (var (_, __addr) in addrs) {
            addr = __addr;
            if (addr.Network() != hint.Network()) {
                return (null, error.As(addr(new AddrError(Err:"mismatched local address type",Addr:hint.String()))!)!);
            }
            switch (addr.type()) {
                case ptr<TCPAddr> addr:
                    if (!wildcard && !addr.isWildcard() && !addr.IP.matchAddrFamily(tcp.IP)) {
                        continue;
                    }
                    naddrs = append(naddrs, addr);
                    break;
                case ptr<UDPAddr> addr:
                    if (!wildcard && !addr.isWildcard() && !addr.IP.matchAddrFamily(udp.IP)) {
                        continue;
                    }
                    naddrs = append(naddrs, addr);
                    break;
                case ptr<IPAddr> addr:
                    if (!wildcard && !addr.isWildcard() && !addr.IP.matchAddrFamily(ip.IP)) {
                        continue;
                    }
                    naddrs = append(naddrs, addr);
                    break;
            }
        }
        addr = addr__prev1;
    }

    if (len(naddrs) == 0) {
        return (null, error.As(addr(new AddrError(Err:errNoSuitableAddress.Error(),Addr:hint.String()))!)!);
    }
    return (naddrs, error.As(null!)!);
}

// Dial connects to the address on the named network.
//
// Known networks are "tcp", "tcp4" (IPv4-only), "tcp6" (IPv6-only),
// "udp", "udp4" (IPv4-only), "udp6" (IPv6-only), "ip", "ip4"
// (IPv4-only), "ip6" (IPv6-only), "unix", "unixgram" and
// "unixpacket".
//
// For TCP and UDP networks, the address has the form "host:port".
// The host must be a literal IP address, or a host name that can be
// resolved to IP addresses.
// The port must be a literal port number or a service name.
// If the host is a literal IPv6 address it must be enclosed in square
// brackets, as in "[2001:db8::1]:80" or "[fe80::1%zone]:80".
// The zone specifies the scope of the literal IPv6 address as defined
// in RFC 4007.
// The functions JoinHostPort and SplitHostPort manipulate a pair of
// host and port in this form.
// When using TCP, and the host resolves to multiple IP addresses,
// Dial will try each IP address in order until one succeeds.
//
// Examples:
//    Dial("tcp", "golang.org:http")
//    Dial("tcp", "192.0.2.1:http")
//    Dial("tcp", "198.51.100.1:80")
//    Dial("udp", "[2001:db8::1]:domain")
//    Dial("udp", "[fe80::1%lo0]:53")
//    Dial("tcp", ":80")
//
// For IP networks, the network must be "ip", "ip4" or "ip6" followed
// by a colon and a literal protocol number or a protocol name, and
// the address has the form "host". The host must be a literal IP
// address or a literal IPv6 address with zone.
// It depends on each operating system how the operating system
// behaves with a non-well known protocol number such as "0" or "255".
//
// Examples:
//    Dial("ip4:1", "192.0.2.1")
//    Dial("ip6:ipv6-icmp", "2001:db8::1")
//    Dial("ip6:58", "fe80::1%lo0")
//
// For TCP, UDP and IP networks, if the host is empty or a literal
// unspecified IP address, as in ":80", "0.0.0.0:80" or "[::]:80" for
// TCP and UDP, "", "0.0.0.0" or "::" for IP, the local system is
// assumed.
//
// For Unix networks, the address must be a file system path.
public static (Conn, error) Dial(@string network, @string address) {
    Conn _p0 = default;
    error _p0 = default!;

    Dialer d = default;
    return d.Dial(network, address);
}

// DialTimeout acts like Dial but takes a timeout.
//
// The timeout includes name resolution, if required.
// When using TCP, and the host in the address parameter resolves to
// multiple IP addresses, the timeout is spread over each consecutive
// dial, such that each is given an appropriate fraction of the time
// to connect.
//
// See func Dial for a description of the network and address
// parameters.
public static (Conn, error) DialTimeout(@string network, @string address, time.Duration timeout) {
    Conn _p0 = default;
    error _p0 = default!;

    Dialer d = new Dialer(Timeout:timeout);
    return d.Dial(network, address);
}

// sysDialer contains a Dial's parameters and configuration.
private partial struct sysDialer {
    public ref Dialer Dialer => ref Dialer_val;
    public @string network;
    public @string address;
}

// Dial connects to the address on the named network.
//
// See func Dial for a description of the network and address
// parameters.
//
// Dial uses context.Background internally; to specify the context, use
// DialContext.
private static (Conn, error) Dial(this ptr<Dialer> _addr_d, @string network, @string address) {
    Conn _p0 = default;
    error _p0 = default!;
    ref Dialer d = ref _addr_d.val;

    return d.DialContext(context.Background(), network, address);
}

// DialContext connects to the address on the named network using
// the provided context.
//
// The provided Context must be non-nil. If the context expires before
// the connection is complete, an error is returned. Once successfully
// connected, any expiration of the context will not affect the
// connection.
//
// When using TCP, and the host in the address parameter resolves to multiple
// network addresses, any dial timeout (from d.Timeout or ctx) is spread
// over each consecutive dial, such that each is given an appropriate
// fraction of the time to connect.
// For example, if a host has 4 IP addresses and the timeout is 1 minute,
// the connect to each single address will be given 15 seconds to complete
// before trying the next one.
//
// See func Dial for a description of the network and address
// parameters.
private static (Conn, error) DialContext(this ptr<Dialer> _addr_d, context.Context ctx, @string network, @string address) => func((defer, panic, _) => {
    Conn _p0 = default;
    error _p0 = default!;
    ref Dialer d = ref _addr_d.val;

    if (ctx == null) {
        panic("nil context");
    }
    var deadline = d.deadline(ctx, time.Now());
    if (!deadline.IsZero()) {
        {
            var (d, ok) = ctx.Deadline();

            if (!ok || deadline.Before(d)) {
                var (subCtx, cancel) = context.WithDeadline(ctx, deadline);
                defer(cancel());
                ctx = subCtx;
            }

        }
    }
    {
        var oldCancel = d.Cancel;

        if (oldCancel != null) {
            (subCtx, cancel) = context.WithCancel(ctx);
            defer(cancel());
            go_(() => () => {
                cancel();
            }());
            ctx = subCtx;
        }
    } 

    // Shadow the nettrace (if any) during resolve so Connect events don't fire for DNS lookups.
    var resolveCtx = ctx;
    {
        ptr<nettrace.Trace> (trace, _) = ctx.Value(new nettrace.TraceKey())._<ptr<nettrace.Trace>>();

        if (trace != null) {
            ref var shadow = ref heap(trace.val, out ptr<var> _addr_shadow);
            shadow.ConnectStart = null;
            shadow.ConnectDone = null;
            resolveCtx = context.WithValue(resolveCtx, new nettrace.TraceKey(), _addr_shadow);
        }
    }

    var (addrs, err) = d.resolver().resolveAddrList(resolveCtx, "dial", network, address, d.LocalAddr);
    if (err != null) {
        return (null, error.As(addr(new OpError(Op:"dial",Net:network,Source:nil,Addr:nil,Err:err))!)!);
    }
    ptr<sysDialer> sd = addr(new sysDialer(Dialer:*d,network:network,address:address,));

    addrList primaries = default;    addrList fallbacks = default;

    if (d.dualStack() && network == "tcp") {
        primaries, fallbacks = addrs.partition(isIPv4);
    }
    else
 {
        primaries = addrs;
    }
    Conn c = default;
    if (len(fallbacks) > 0) {
        c, err = sd.dialParallel(ctx, primaries, fallbacks);
    }
    else
 {
        c, err = sd.dialSerial(ctx, primaries);
    }
    if (err != null) {
        return (null, error.As(err)!);
    }
    {
        ptr<TCPConn> (tc, ok) = c._<ptr<TCPConn>>();

        if (ok && d.KeepAlive >= 0) {
            setKeepAlive(tc.fd, true);
            var ka = d.KeepAlive;
            if (d.KeepAlive == 0) {
                ka = defaultTCPKeepAlive;
            }
            setKeepAlivePeriod(tc.fd, ka);
            testHookSetKeepAlive(ka);
        }
    }
    return (c, error.As(null!)!);
});

// dialParallel races two copies of dialSerial, giving the first a
// head start. It returns the first established connection and
// closes the others. Otherwise it returns an error from the first
// primary address.
private static (Conn, error) dialParallel(this ptr<sysDialer> _addr_sd, context.Context ctx, addrList primaries, addrList fallbacks) => func((defer, _, _) => {
    Conn _p0 = default;
    error _p0 = default!;
    ref sysDialer sd = ref _addr_sd.val;

    if (len(fallbacks) == 0) {
        return sd.dialSerial(ctx, primaries);
    }
    var returned = make_channel<object>();
    defer(close(returned));

    private partial struct dialResult : Conn, error {
        public Conn Conn;
        public error error;
        public bool primary;
        public bool done;
    }
    var results = make_channel<dialResult>(); // unbuffered

    Action<context.Context, bool> startRacer = (ctx, primary) => {
        var ras = primaries;
        if (!primary) {
            ras = fallbacks;
        }
        var (c, err) = sd.dialSerial(ctx, ras);
        if (c != null) {
            c.Close();
        }
    };

    dialResult primary = default;    dialResult fallback = default; 

    // Start the main racer.
 

    // Start the main racer.
    var (primaryCtx, primaryCancel) = context.WithCancel(ctx);
    defer(primaryCancel());
    go_(() => startRacer(primaryCtx, true)); 

    // Start the timer for the fallback racer.
    var fallbackTimer = time.NewTimer(sd.fallbackDelay());
    defer(fallbackTimer.Stop());

    while (true) {
        var (fallbackCtx, fallbackCancel) = context.WithCancel(ctx);
        defer(fallbackCancel());
        go_(() => startRacer(fallbackCtx, false));

        if (res.error == null) {
            return (res.Conn, error.As(null!)!);
        }
        if (res.primary) {
            primary = res;
        }
        else
 {
            fallback = res;
        }
        if (primary.done && fallback.done) {
            return (null, error.As(primary.error)!);
        }
        if (res.primary && fallbackTimer.Stop()) { 
            // If we were able to stop the timer, that means it
            // was running (hadn't yet started the fallback), but
            // we just got an error on the primary path, so start
            // the fallback immediately (in 0 nanoseconds).
            fallbackTimer.Reset(0);
        }
    }
});

// dialSerial connects to a list of addresses in sequence, returning
// either the first successful connection, or the first error.
private static (Conn, error) dialSerial(this ptr<sysDialer> _addr_sd, context.Context ctx, addrList ras) => func((defer, _, _) => {
    Conn _p0 = default;
    error _p0 = default!;
    ref sysDialer sd = ref _addr_sd.val;

    error firstErr = default!; // The error from the first address is most relevant.

    foreach (var (i, ra) in ras) {
        return (null, error.As(addr(new OpError(Op:"dial",Net:sd.network,Source:sd.LocalAddr,Addr:ra,Err:mapErr(ctx.Err())))!)!);
        var dialCtx = ctx;
        {
            var (deadline, hasDeadline) = ctx.Deadline();

            if (hasDeadline) {
                var (partialDeadline, err) = partialDeadline(time.Now(), deadline, len(ras) - i);
                if (err != null) { 
                    // Ran out of time.
                    if (firstErr == null) {
                        firstErr = error.As(addr(new OpError(Op:"dial",Net:sd.network,Source:sd.LocalAddr,Addr:ra,Err:err)))!;
                    }
                    break;
                }
                if (partialDeadline.Before(deadline)) {
                    context.CancelFunc cancel = default;
                    dialCtx, cancel = context.WithDeadline(ctx, partialDeadline);
                    defer(cancel());
                }
            }

        }

        var (c, err) = sd.dialSingle(dialCtx, ra);
        if (err == null) {
            return (c, error.As(null!)!);
        }
        if (firstErr == null) {
            firstErr = error.As(err)!;
        }
    }    if (firstErr == null) {
        firstErr = error.As(addr(new OpError(Op:"dial",Net:sd.network,Source:nil,Addr:nil,Err:errMissingAddress)))!;
    }
    return (null, error.As(firstErr)!);
});

// dialSingle attempts to establish and returns a single connection to
// the destination address.
private static (Conn, error) dialSingle(this ptr<sysDialer> _addr_sd, context.Context ctx, Addr ra) => func((defer, _, _) => {
    Conn c = default;
    error err = default!;
    ref sysDialer sd = ref _addr_sd.val;

    ptr<nettrace.Trace> (trace, _) = ctx.Value(new nettrace.TraceKey())._<ptr<nettrace.Trace>>();
    if (trace != null) {
        var raStr = ra.String();
        if (trace.ConnectStart != null) {
            trace.ConnectStart(sd.network, raStr);
        }
        if (trace.ConnectDone != null) {
            defer(() => {
                trace.ConnectDone(sd.network, raStr, err);
            }());
        }
    }
    var la = sd.LocalAddr;
    switch (ra.type()) {
        case ptr<TCPAddr> ra:
            ptr<TCPAddr> (la, _) = la._<ptr<TCPAddr>>();
            c, err = sd.dialTCP(ctx, la, ra);
            break;
        case ptr<UDPAddr> ra:
            (la, _) = la._<ptr<UDPAddr>>();
            c, err = sd.dialUDP(ctx, la, ra);
            break;
        case ptr<IPAddr> ra:
            (la, _) = la._<ptr<IPAddr>>();
            c, err = sd.dialIP(ctx, la, ra);
            break;
        case ptr<UnixAddr> ra:
            (la, _) = la._<ptr<UnixAddr>>();
            c, err = sd.dialUnix(ctx, la, ra);
            break;
        default:
        {
            var ra = ra.type();
            return (null, error.As(addr(new OpError(Op:"dial",Net:sd.network,Source:la,Addr:ra,Err:&AddrError{Err:"unexpected address type",Addr:sd.address}))!)!);
            break;
        }
    }
    if (err != null) {
        return (null, error.As(addr(new OpError(Op:"dial",Net:sd.network,Source:la,Addr:ra,Err:err))!)!); // c is non-nil interface containing nil pointer
    }
    return (c, error.As(null!)!);
});

// ListenConfig contains options for listening to an address.
public partial struct ListenConfig {
    public Func<@string, @string, syscall.RawConn, error> Control; // KeepAlive specifies the keep-alive period for network
// connections accepted by this listener.
// If zero, keep-alives are enabled if supported by the protocol
// and operating system. Network protocols or operating systems
// that do not support keep-alives ignore this field.
// If negative, keep-alives are disabled.
    public time.Duration KeepAlive;
}

// Listen announces on the local network address.
//
// See func Listen for a description of the network and address
// parameters.
private static (Listener, error) Listen(this ptr<ListenConfig> _addr_lc, context.Context ctx, @string network, @string address) {
    Listener _p0 = default;
    error _p0 = default!;
    ref ListenConfig lc = ref _addr_lc.val;

    var (addrs, err) = DefaultResolver.resolveAddrList(ctx, "listen", network, address, null);
    if (err != null) {
        return (null, error.As(addr(new OpError(Op:"listen",Net:network,Source:nil,Addr:nil,Err:err))!)!);
    }
    ptr<sysListener> sl = addr(new sysListener(ListenConfig:*lc,network:network,address:address,));
    Listener l = default;
    var la = addrs.first(isIPv4);
    switch (la.type()) {
        case ptr<TCPAddr> la:
            l, err = sl.listenTCP(ctx, la);
            break;
        case ptr<UnixAddr> la:
            l, err = sl.listenUnix(ctx, la);
            break;
        default:
        {
            var la = la.type();
            return (null, error.As(addr(new OpError(Op:"listen",Net:sl.network,Source:nil,Addr:la,Err:&AddrError{Err:"unexpected address type",Addr:address}))!)!);
            break;
        }
    }
    if (err != null) {
        return (null, error.As(addr(new OpError(Op:"listen",Net:sl.network,Source:nil,Addr:la,Err:err))!)!); // l is non-nil interface containing nil pointer
    }
    return (l, error.As(null!)!);
}

// ListenPacket announces on the local network address.
//
// See func ListenPacket for a description of the network and address
// parameters.
private static (PacketConn, error) ListenPacket(this ptr<ListenConfig> _addr_lc, context.Context ctx, @string network, @string address) {
    PacketConn _p0 = default;
    error _p0 = default!;
    ref ListenConfig lc = ref _addr_lc.val;

    var (addrs, err) = DefaultResolver.resolveAddrList(ctx, "listen", network, address, null);
    if (err != null) {
        return (null, error.As(addr(new OpError(Op:"listen",Net:network,Source:nil,Addr:nil,Err:err))!)!);
    }
    ptr<sysListener> sl = addr(new sysListener(ListenConfig:*lc,network:network,address:address,));
    PacketConn c = default;
    var la = addrs.first(isIPv4);
    switch (la.type()) {
        case ptr<UDPAddr> la:
            c, err = sl.listenUDP(ctx, la);
            break;
        case ptr<IPAddr> la:
            c, err = sl.listenIP(ctx, la);
            break;
        case ptr<UnixAddr> la:
            c, err = sl.listenUnixgram(ctx, la);
            break;
        default:
        {
            var la = la.type();
            return (null, error.As(addr(new OpError(Op:"listen",Net:sl.network,Source:nil,Addr:la,Err:&AddrError{Err:"unexpected address type",Addr:address}))!)!);
            break;
        }
    }
    if (err != null) {
        return (null, error.As(addr(new OpError(Op:"listen",Net:sl.network,Source:nil,Addr:la,Err:err))!)!); // c is non-nil interface containing nil pointer
    }
    return (c, error.As(null!)!);
}

// sysListener contains a Listen's parameters and configuration.
private partial struct sysListener {
    public ref ListenConfig ListenConfig => ref ListenConfig_val;
    public @string network;
    public @string address;
}

// Listen announces on the local network address.
//
// The network must be "tcp", "tcp4", "tcp6", "unix" or "unixpacket".
//
// For TCP networks, if the host in the address parameter is empty or
// a literal unspecified IP address, Listen listens on all available
// unicast and anycast IP addresses of the local system.
// To only use IPv4, use network "tcp4".
// The address can use a host name, but this is not recommended,
// because it will create a listener for at most one of the host's IP
// addresses.
// If the port in the address parameter is empty or "0", as in
// "127.0.0.1:" or "[::1]:0", a port number is automatically chosen.
// The Addr method of Listener can be used to discover the chosen
// port.
//
// See func Dial for a description of the network and address
// parameters.
//
// Listen uses context.Background internally; to specify the context, use
// ListenConfig.Listen.
public static (Listener, error) Listen(@string network, @string address) {
    Listener _p0 = default;
    error _p0 = default!;

    ListenConfig lc = default;
    return lc.Listen(context.Background(), network, address);
}

// ListenPacket announces on the local network address.
//
// The network must be "udp", "udp4", "udp6", "unixgram", or an IP
// transport. The IP transports are "ip", "ip4", or "ip6" followed by
// a colon and a literal protocol number or a protocol name, as in
// "ip:1" or "ip:icmp".
//
// For UDP and IP networks, if the host in the address parameter is
// empty or a literal unspecified IP address, ListenPacket listens on
// all available IP addresses of the local system except multicast IP
// addresses.
// To only use IPv4, use network "udp4" or "ip4:proto".
// The address can use a host name, but this is not recommended,
// because it will create a listener for at most one of the host's IP
// addresses.
// If the port in the address parameter is empty or "0", as in
// "127.0.0.1:" or "[::1]:0", a port number is automatically chosen.
// The LocalAddr method of PacketConn can be used to discover the
// chosen port.
//
// See func Dial for a description of the network and address
// parameters.
//
// ListenPacket uses context.Background internally; to specify the context, use
// ListenConfig.ListenPacket.
public static (PacketConn, error) ListenPacket(@string network, @string address) {
    PacketConn _p0 = default;
    error _p0 = default!;

    ListenConfig lc = default;
    return lc.ListenPacket(context.Background(), network, address);
}

} // end net_package
