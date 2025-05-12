// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using bytealg = @internal.bytealg_package;
using godebug = @internal.godebug_package;
using nettrace = @internal.nettrace_package;
using syscall = syscall_package;
using time = time_package;
using @internal;

partial class net_package {

internal static readonly time.Duration defaultTCPKeepAliveIdle = /* 15 * time.Second */ 15000000000;
internal static readonly time.Duration defaultTCPKeepAliveInterval = /* 15 * time.Second */ 15000000000;
internal static readonly UntypedInt defaultTCPKeepAliveCount = 9;
internal const bool defaultMPTCPEnabled = false;

internal static ж<godebug.Setting> multipathtcp = godebug.New("multipathtcp"u8);

[GoType("num:uint8")] partial struct mptcpStatus;

internal static readonly mptcpStatus mptcpUseDefault = /* iota */ 0;
internal static readonly mptcpStatus mptcpEnabled = 1;
internal static readonly mptcpStatus mptcpDisabled = 2;

[GoRecv] internal static bool get(this ref mptcpStatus m) {
    var exprᴛ1 = m;
    if (exprᴛ1 == mptcpEnabled) {
        return true;
    }
    if (exprᴛ1 == mptcpDisabled) {
        return false;
    }

    // If MPTCP is forced via GODEBUG=multipathtcp=1
    if (multipathtcp.Value() == "1"u8) {
        multipathtcp.IncNonDefault();
        return true;
    }
    return defaultMPTCPEnabled;
}

[GoRecv] internal static void set(this ref mptcpStatus m, bool use) {
    if (use){
        m = mptcpEnabled;
    } else {
        m = mptcpDisabled;
    }
}

// A Dialer contains options for connecting to an address.
//
// The zero value for each field is equivalent to dialing
// without that option. Dialing with the zero value of Dialer
// is therefore equivalent to just calling the [Dial] function.
//
// It is safe to call Dialer's methods concurrently.
[GoType] partial struct Dialer {
    // Timeout is the maximum amount of time a dial will wait for
    // a connect to complete. If Deadline is also set, it may fail
    // earlier.
    //
    // The default is no timeout.
    //
    // When using TCP and dialing a host name with multiple IP
    // addresses, the timeout may be divided between them.
    //
    // With or without a timeout, the operating system may impose
    // its own earlier timeout. For instance, TCP timeouts are
    // often around 3 minutes.
    public time_package.Duration Timeout;
    // Deadline is the absolute point in time after which dials
    // will fail. If Timeout is set, it may fail earlier.
    // Zero means no deadline, or dependent on the operating system
    // as with the Timeout option.
    public time_package.Time Deadline;
    // LocalAddr is the local address to use when dialing an
    // address. The address must be of a compatible type for the
    // network being dialed.
    // If nil, a local address is automatically chosen.
    public ΔAddr LocalAddr;
    // DualStack previously enabled RFC 6555 Fast Fallback
    // support, also known as "Happy Eyeballs", in which IPv4 is
    // tried soon if IPv6 appears to be misconfigured and
    // hanging.
    //
    // Deprecated: Fast Fallback is enabled by default. To
    // disable, set FallbackDelay to a negative value.
    public bool DualStack;
    // FallbackDelay specifies the length of time to wait before
    // spawning a RFC 6555 Fast Fallback connection. That is, this
    // is the amount of time to wait for IPv6 to succeed before
    // assuming that IPv6 is misconfigured and falling back to
    // IPv4.
    //
    // If zero, a default delay of 300ms is used.
    // A negative value disables Fast Fallback support.
    public time_package.Duration FallbackDelay;
    // KeepAlive specifies the interval between keep-alive
    // probes for an active network connection.
    //
    // KeepAlive is ignored if KeepAliveConfig.Enable is true.
    //
    // If zero, keep-alive probes are sent with a default value
    // (currently 15 seconds), if supported by the protocol and operating
    // system. Network protocols or operating systems that do
    // not support keep-alive ignore this field.
    // If negative, keep-alive probes are disabled.
    public time_package.Duration KeepAlive;
    // KeepAliveConfig specifies the keep-alive probe configuration
    // for an active network connection, when supported by the
    // protocol and operating system.
    //
    // If KeepAliveConfig.Enable is true, keep-alive probes are enabled.
    // If KeepAliveConfig.Enable is false and KeepAlive is negative,
    // keep-alive probes are disabled.
    public KeepAliveConfig KeepAliveConfig;
    // Resolver optionally specifies an alternate resolver to use.
    public ж<Resolver> Resolver;
    // Cancel is an optional channel whose closure indicates that
    // the dial should be canceled. Not all types of dials support
    // cancellation.
    //
    // Deprecated: Use DialContext instead.
    public /*<-*/channel<EmptyStruct> Cancel;
    // If Control is not nil, it is called after creating the network
    // connection but before actually dialing.
    //
    // Network and address parameters passed to Control function are not
    // necessarily the ones passed to Dial. For example, passing "tcp" to Dial
    // will cause the Control function to be called with "tcp4" or "tcp6".
    //
    // Control is ignored if ControlContext is not nil.
    public Func<@string, @string, syscall.RawConn, error> Control;
    // If ControlContext is not nil, it is called after creating the network
    // connection but before actually dialing.
    //
    // Network and address parameters passed to ControlContext function are not
    // necessarily the ones passed to Dial. For example, passing "tcp" to Dial
    // will cause the ControlContext function to be called with "tcp4" or "tcp6".
    //
    // If ControlContext is not nil, Control is ignored.
    public Func<context.Context, @string, @string, syscall.RawConn, error> ControlContext;
    // If mptcpStatus is set to a value allowing Multipath TCP (MPTCP) to be
    // used, any call to Dial with "tcp(4|6)" as network will use MPTCP if
    // supported by the operating system.
    internal mptcpStatus mptcpStatus;
}

[GoRecv] internal static bool dualStack(this ref Dialer d) {
    return d.FallbackDelay >= 0;
}

internal static time.Time minNonzeroTime(time.Time a, time.Time b) {
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
//
// Or zero, if none of Timeout, Deadline, or context's deadline is set.
[GoRecv] internal static time.Time /*earliest*/ deadline(this ref Dialer d, context.Context ctx, time.Time now) {
    time.Time earliest = default!;

    if (d.Timeout != 0) {
        // including negative, for historical reasons
        earliest = now.Add(d.Timeout);
    }
    {
        var (dΔ1, ok) = ctx.Deadline(); if (ok) {
            earliest = minNonzeroTime(earliest, dΔ1);
        }
    }
    return minNonzeroTime(earliest, d.Deadline);
}

[GoRecv] internal static ж<Resolver> resolver(this ref Dialer d) {
    if (d.Resolver != nil) {
        return d.Resolver;
    }
    return DefaultResolver;
}

// partialDeadline returns the deadline to use for a single address,
// when multiple addresses are pending.
internal static (time.Time, error) partialDeadline(time.Time now, time.Time deadline, nint addrsRemaining) {
    if (deadline.IsZero()) {
        return (deadline, default!);
    }
    var timeRemaining = deadline.Sub(now);
    if (timeRemaining <= 0) {
        return (new time.Time(nil), errTimeout);
    }
    // Tentatively allocate equal time to each remaining address.
    var timeout = timeRemaining / ((time.Duration)addrsRemaining);
    // If the time per address is too short, steal from the end of the list.
    static readonly time.Duration saneMinimum = /* 2 * time.Second */ 2000000000;
    if (timeout < saneMinimum) {
        if (timeRemaining < saneMinimum){
            timeout = timeRemaining;
        } else {
            timeout = saneMinimum;
        }
    }
    return (now.Add(timeout), default!);
}

[GoRecv] internal static time.Duration fallbackDelay(this ref Dialer d) {
    if (d.FallbackDelay > 0){
        return d.FallbackDelay;
    } else {
        return 300 * time.Millisecond;
    }
}

internal static (@string afnet, nint proto, error err) parseNetwork(context.Context ctx, @string network, bool needsProto) {
    @string afnet = default!;
    nint proto = default!;
    error err = default!;

    nint i = bytealg.LastIndexByteString(network, (rune)':');
    if (i < 0) {
        // no colon
        var exprᴛ1 = network;
        if (exprᴛ1 == "tcp"u8 || exprᴛ1 == "tcp4"u8 || exprᴛ1 == "tcp6"u8) {
        }
        else if (exprᴛ1 == "udp"u8 || exprᴛ1 == "udp4"u8 || exprᴛ1 == "udp6"u8) {
        }
        else if (exprᴛ1 == "ip"u8 || exprᴛ1 == "ip4"u8 || exprᴛ1 == "ip6"u8) {
            if (needsProto) {
                return ("", 0, ((UnknownNetworkError)network));
            }
        }
        if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixgram"u8 || exprᴛ1 == "unixpacket"u8) {
        }
        { /* default: */
            return ("", 0, ((UnknownNetworkError)network));
        }

        return (network, 0, default!);
    }
    afnet = network[..(int)(i)];
    var exprᴛ2 = afnet;
    if (exprᴛ2 == "ip"u8 || exprᴛ2 == "ip4"u8 || exprᴛ2 == "ip6"u8) {
        @string protostr = network[(int)(i + 1)..];
        var (protoΔ2, iΔ2, ok) = dtoi(protostr);
        if (!ok || iΔ2 != len(protostr)) {
            (protoΔ2, err) = lookupProtocol(ctx, protostr);
            if (err != default!) {
                return ("", 0, err);
            }
        }
        return (afnet, protoΔ2, default!);
    }

    return ("", 0, ((UnknownNetworkError)network));
}

// resolveAddrList resolves addr using hint and returns a list of
// addresses. The result contains at least one address when error is
// nil.
[GoRecv] internal static (addrList, error) resolveAddrList(this ref Resolver r, context.Context ctx, @string op, @string network, @string addr, ΔAddr hint) {
    var (afnet, _, err) = parseNetwork(ctx, network, true);
    if (err != default!) {
        return (default!, err);
    }
    if (op == "dial"u8 && addr == ""u8) {
        return (default!, errMissingAddress);
    }
    var exprᴛ1 = afnet;
    if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixgram"u8 || exprᴛ1 == "unixpacket"u8) {
        (addrΔ2, errΔ2) = ResolveUnixAddr(afnet, addr);
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        if (op == "dial"u8 && hint != default! && addrΔ2.Network() != hint.Network()) {
            return (default!, new AddrError(Err: "mismatched local address type"u8, ΔAddr: hint.String()));
        }
        return (new addrList{addrΔ2}, default!);
    }

    (addrs, err) = r.internetAddrList(ctx, afnet, addr);
    if (err != default! || op != "dial"u8 || hint == default!) {
        return (addrs, err);
    }
    ж<TCPAddr> tcp = default!;
    ж<UDPAddr> udp = default!;
    ж<IPAddr> ip = default!;
    bool wildcard = default!;
    switch (hint.type()) {
    case TCPAddr.val hint: {
        tcp = hint;
        wildcard = tcp.isWildcard();
        break;
    }
    case UDPAddr.val hint: {
        udp = hint;
        wildcard = udp.isWildcard();
        break;
    }
    case IPAddr.val hint: {
        ip = hint;
        wildcard = ip.isWildcard();
        break;
    }}
    var naddrs = addrs[..0];
    foreach (var (_, addrΔ3) in addrs) {
        if (addrΔ3.Network() != hint.Network()) {
            return (default!, new AddrError(Err: "mismatched local address type"u8, ΔAddr: hint.String()));
        }
        switch (addrΔ3.type()) {
        case TCPAddr.val addr: {
            if (!wildcard && !addrΔ3.isWildcard() && !(~addrΔ3).IP.matchAddrFamily((~tcp).IP)) {
                continue;
            }
            naddrs = append(naddrs, ~addrΔ3);
            break;
        }
        case UDPAddr.val addr: {
            if (!wildcard && !addrΔ3.isWildcard() && !(~addrΔ3).IP.matchAddrFamily((~udp).IP)) {
                continue;
            }
            naddrs = append(naddrs, ~addrΔ3);
            break;
        }
        case IPAddr.val addr: {
            if (!wildcard && !addrΔ3.isWildcard() && !(~addrΔ3).IP.matchAddrFamily((~ip).IP)) {
                continue;
            }
            naddrs = append(naddrs, ~addrΔ3);
            break;
        }}
    }
    if (len(naddrs) == 0) {
        return (default!, new AddrError(Err: errNoSuitableAddress.Error(), ΔAddr: hint.String()));
    }
    return (naddrs, default!);
}

// MultipathTCP reports whether MPTCP will be used.
//
// This method doesn't check if MPTCP is supported by the operating
// system or not.
[GoRecv] public static bool MultipathTCP(this ref Dialer d) {
    return d.mptcpStatus.get();
}

// SetMultipathTCP directs the [Dial] methods to use, or not use, MPTCP,
// if supported by the operating system. This method overrides the
// system default and the GODEBUG=multipathtcp=... setting if any.
//
// If MPTCP is not available on the host or not supported by the server,
// the Dial methods will fall back to TCP.
[GoRecv] public static void SetMultipathTCP(this ref Dialer d, bool use) {
    d.mptcpStatus.set(use);
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
// The functions [JoinHostPort] and [SplitHostPort] manipulate a pair of
// host and port in this form.
// When using TCP, and the host resolves to multiple IP addresses,
// Dial will try each IP address in order until one succeeds.
//
// Examples:
//
//	Dial("tcp", "golang.org:http")
//	Dial("tcp", "192.0.2.1:http")
//	Dial("tcp", "198.51.100.1:80")
//	Dial("udp", "[2001:db8::1]:domain")
//	Dial("udp", "[fe80::1%lo0]:53")
//	Dial("tcp", ":80")
//
// For IP networks, the network must be "ip", "ip4" or "ip6" followed
// by a colon and a literal protocol number or a protocol name, and
// the address has the form "host". The host must be a literal IP
// address or a literal IPv6 address with zone.
// It depends on each operating system how the operating system
// behaves with a non-well known protocol number such as "0" or "255".
//
// Examples:
//
//	Dial("ip4:1", "192.0.2.1")
//	Dial("ip6:ipv6-icmp", "2001:db8::1")
//	Dial("ip6:58", "fe80::1%lo0")
//
// For TCP, UDP and IP networks, if the host is empty or a literal
// unspecified IP address, as in ":80", "0.0.0.0:80" or "[::]:80" for
// TCP and UDP, "", "0.0.0.0" or "::" for IP, the local system is
// assumed.
//
// For Unix networks, the address must be a file system path.
public static (Conn, error) Dial(@string network, @string address) {
    Dialer d = default!;
    return d.Dial(network, address);
}

// DialTimeout acts like [Dial] but takes a timeout.
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
    var d = new Dialer(Timeout: timeout);
    return d.Dial(network, address);
}

// sysDialer contains a Dial's parameters and configuration.
[GoType] partial struct sysDialer {
    public partial ref Dialer Dialer { get; }
    internal @string network;
    internal @string address;
    internal Func<context.Context, @string, ж<TCPAddr>, ж<net.TCPAddr>, (*net.TCPConn, error)> testHookDialTCP;
}

// Dial connects to the address on the named network.
//
// See func Dial for a description of the network and address
// parameters.
//
// Dial uses [context.Background] internally; to specify the context, use
// [Dialer.DialContext].
[GoRecv] public static (Conn, error) Dial(this ref Dialer d, @string network, @string address) {
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
// See func [Dial] for a description of the network and address
// parameters.
[GoRecv] public static (Conn, error) DialContext(this ref Dialer d, context.Context ctx, @string network, @string address) => func((defer, _) => {
    if (ctx == default!) {
        throw panic("nil context");
    }
    var deadline = d.deadline(ctx, time.Now());
    if (!deadline.IsZero()) {
        testHookStepTime();
        {
            var (dΔ1, ok) = ctx.Deadline(); if (!ok || deadline.Before(dΔ1)) {
                (subCtx, cancel) = context.WithDeadline(ctx, deadline);
                var cancelʗ1 = cancel;
                defer(cancelʗ1);
                ctx = subCtx;
            }
        }
    }
    {
        var oldCancel = d.Cancel; if (oldCancel != default!) {
            (subCtx, cancel) = context.WithCancel(ctx);
            var cancelʗ2 = cancel;
            defer(cancelʗ2);
            var cancelʗ3 = cancel;
            var oldCancelʗ1 = oldCancel;
            var subCtxʗ1 = subCtx;
            goǃ(() => {
                switch (select(ᐸꟷ(oldCancelʗ1, ꓸꓸꓸ), ᐸꟷ(subCtxʗ1.Done(), ꓸꓸꓸ))) {
                case 0 when oldCancelʗ1.ꟷᐳ(out _): {
                    cancelʗ3();
                    break;
                }
                case 1 when subCtxʗ1.Done().ꟷᐳ(out _): {
                    break;
                }}
            });
            ctx = subCtx;
        }
    }
    // Shadow the nettrace (if any) during resolve so Connect events don't fire for DNS lookups.
    var resolveCtx = ctx;
    {
        var (trace, _) = ctx.Value(new nettrace.TraceKey(nil))._<ж<nettrace.Trace>>(ᐧ); if (trace != nil) {
            ref var shadow = ref heap<@internal.nettrace_package.Trace>(out var Ꮡshadow);
            shadow = trace.val;
            shadow.ConnectStart = default!;
            shadow.ConnectDone = default!;
            resolveCtx = context.WithValue(resolveCtx, new nettrace.TraceKey(nil), Ꮡshadow);
        }
    }
    (addrs, err) = d.resolver().resolveAddrList(resolveCtx, "dial"u8, network, address, d.LocalAddr);
    if (err != default!) {
        return (default!, new OpError(Op: "dial"u8, Net: network, Source: default!, ΔAddr: default!, Err: err));
    }
    var sd = Ꮡ(new sysDialer(
        Dialer: d,
        network: network,
        address: address
    ));
    addrList primaries = default!;
    addrList fallbacks = default!;
    if (d.dualStack() && network == "tcp"u8){
        (primaries, fallbacks) = addrs.partition(isIPv4);
    } else {
        primaries = addrs;
    }
    return sd.dialParallel(ctx, primaries, fallbacks);
});

[GoType("dyn")] partial struct dialParallel_dialResult {
    public Conn Conn;
    internal error error;
    internal bool primary;
    internal bool done;
}

// dialParallel races two copies of dialSerial, giving the first a
// head start. It returns the first established connection and
// closes the others. Otherwise it returns an error from the first
// primary address.
[GoRecv] internal static (Conn, error) dialParallel(this ref sysDialer sd, context.Context ctx, addrList primaries, addrList fallbacks) => func((defer, _) => {
    if (len(fallbacks) == 0) {
        return sd.dialSerial(ctx, primaries);
    }
    var returned = new channel<EmptyStruct>(1);
    deferǃ(close, returned, defer);
    var results = new channel<dialResult>(1);
    // unbuffered
    var startRacer = 
    var fallbacksʗ1 = fallbacks;
    var primariesʗ1 = primaries;
    var resultsʗ1 = results;
    var returnedʗ1 = returned;
    (context.Context ctx, bool primary) => {
        var ras = primariesʗ1;
        if (!primaryΔ1) {
            ras = fallbacksʗ1;
        }
        (c, err) = sd.dialSerial(ctxΔ1, ras);
        switch (select(resultsʗ1.ᐸꟷ(new dialResult(Conn: c, error: err, primary: primary, done: true), ꓸꓸꓸ), ᐸꟷ(returnedʗ1, ꓸꓸꓸ))) {
        case 0: {
            break;
        }
        case 1 when returnedʗ1.ꟷᐳ(out _): {
            if (c != default!) {
                c.Close();
            }
            break;
        }}
    };
    dialParallel_dialResult primary = default!;
    dialParallel_dialResult fallback = default!;
    // Start the main racer.
    (primaryCtx, primaryCancel) = context.WithCancel(ctx);
    var primaryCancelʗ1 = primaryCancel;
    defer(primaryCancelʗ1);
    var startRacerʗ1 = startRacer;
    goǃ(startRacerʗ1, primaryCtx, true);
    // Start the timer for the fallback racer.
    var fallbackTimer = time.NewTimer(sd.fallbackDelay());
    var fallbackTimerʗ1 = fallbackTimer;
    defer(fallbackTimerʗ1.Stop);
    while (ᐧ) {
        switch (select(ᐸꟷ((~fallbackTimer).C, ꓸꓸꓸ), ᐸꟷ(results, ꓸꓸꓸ))) {
        case 0 when (~fallbackTimer).C.ꟷᐳ(out _): {
            (fallbackCtx, fallbackCancel) = context.WithCancel(ctx);
            var fallbackCancelʗ1 = fallbackCancel;
            defer(fallbackCancelʗ1);
            var startRacerʗ2 = startRacer;
            goǃ(startRacerʗ2, fallbackCtx, false);
            break;
        }
        case 1 when results.ꟷᐳ(out var res): {
            if (res.error == default!) {
                return (res.Conn, default!);
            }
            if (res.primary){
                primary = res;
            } else {
                fallback = res;
            }
            if (primary.done && fallback.done) {
                return (default!, primary.error);
            }
            if (res.primary && fallbackTimer.Stop()) {
                // If we were able to stop the timer, that means it
                // was running (hadn't yet started the fallback), but
                // we just got an error on the primary path, so start
                // the fallback immediately (in 0 nanoseconds).
                fallbackTimer.Reset(0);
            }
            break;
        }}
    }
});

// dialSerial connects to a list of addresses in sequence, returning
// either the first successful connection, or the first error.
[GoRecv] internal static (Conn, error) dialSerial(this ref sysDialer sd, context.Context ctx, addrList ras) => func((defer, _) => {
    error firstErr = default!;    // The error from the first address is most relevant.
    foreach (var (i, ra) in ras) {
        switch (ᐧ) {
        case ᐧ when ctx.Done().ꟷᐳ(out _): {
            return (default!, new OpError(Op: "dial"u8, Net: sd.network, Source: sd.LocalAddr, ΔAddr: ra, Err: mapErr(ctx.Err())));
        }
        default: {
        }}
        var dialCtx = ctx;
        {
            var (deadline, hasDeadline) = ctx.Deadline(); if (hasDeadline) {
                var (partialDeadline, err) = partialDeadline(time.Now(), deadline, len(ras) - i);
                if (err != default!) {
                    // Ran out of time.
                    if (firstErr == default!) {
                        ᏑfirstErr = new OpError(Op: "dial"u8, Net: sd.network, Source: sd.LocalAddr, ΔAddr: ra, Err: err); firstErr = ref ᏑfirstErr.val;
                    }
                    break;
                }
                if (partialDeadline.Before(deadline)) {
                    context.CancelFunc cancel = default!;
                    (dialCtx, cancel) = context.WithDeadline(ctx, partialDeadline);
                    var cancelʗ1 = cancel;
                    defer(cancelʗ1);
                }
            }
        }
        (c, err) = sd.dialSingle(dialCtx, ra);
        if (err == default!) {
            return (c, default!);
        }
        if (firstErr == default!) {
            firstErr = err;
        }
    }
    if (firstErr == default!) {
        ᏑfirstErr = new OpError(Op: "dial"u8, Net: sd.network, Source: default!, ΔAddr: default!, Err: errMissingAddress); firstErr = ref ᏑfirstErr.val;
    }
    return (default!, firstErr);
});

// dialSingle attempts to establish and returns a single connection to
// the destination address.
[GoRecv] internal static (Conn c, error err) dialSingle(this ref sysDialer sd, context.Context ctx, ΔAddr ra) => func((defer, _) => {
    Conn c = default!;
    error err = default!;

    var (trace, _) = ctx.Value(new nettrace.TraceKey(nil))._<ж<nettrace.Trace>>(ᐧ);
    if (trace != nil) {
        @string raStr = ra.String();
        if ((~trace).ConnectStart != default!) {
            (~trace).ConnectStart(sd.network, raStr);
        }
        if ((~trace).ConnectDone != default!) {
            var traceʗ1 = trace;
            defer(() => {
                (~traceʗ1).ConnectDone(sd.network, raStr, err);
            });
        }
    }
    var la = sd.LocalAddr;
    switch (ra.type()) {
    case TCPAddr.val ra: {
        var (laΔ1, _) = la._<TCPAddr.val>(ᐧ);
        if (sd.MultipathTCP()){
            (c, err) = sd.dialMPTCP(ctx, laΔ1, Ꮡra);
        } else {
            (c, err) = sd.dialTCP(ctx, laΔ1, Ꮡra);
        }
        break;
    }
    case UDPAddr.val ra: {
        var (la, _) = la._<UDPAddr.val>(ᐧ);
        (c, err) = sd.dialUDP(ctx, la, Ꮡra);
        break;
    }
    case IPAddr.val ra: {
        var (la, _) = la._<IPAddr.val>(ᐧ);
        (c, err) = sd.dialIP(ctx, la, Ꮡra);
        break;
    }
    case UnixAddr.val ra: {
        var (la, _) = la._<UnixAddr.val>(ᐧ);
        (c, err) = sd.dialUnix(ctx, la, Ꮡra);
        break;
    }
    default: {
        var ra = ra.type();
        return (default!, new OpError(Op: "dial"u8, Net: sd.network, Source: la, ΔAddr: ra, Err: Ꮡ(new AddrError(Err: "unexpected address type"u8, ΔAddr: sd.address))));
    }}
    if (err != default!) {
        return (default!, new OpError(Op: "dial"u8, Net: sd.network, Source: la, ΔAddr: ra, Err: err));
    }
    // c is non-nil interface containing nil pointer
    return (c, default!);
});

// ListenConfig contains options for listening to an address.
[GoType] partial struct ListenConfig {
    // If Control is not nil, it is called after creating the network
    // connection but before binding it to the operating system.
    //
    // Network and address parameters passed to Control method are not
    // necessarily the ones passed to Listen. For example, passing "tcp" to
    // Listen will cause the Control function to be called with "tcp4" or "tcp6".
    public Func<@string, @string, syscall.RawConn, error> Control;
    // KeepAlive specifies the keep-alive period for network
    // connections accepted by this listener.
    //
    // KeepAlive is ignored if KeepAliveConfig.Enable is true.
    //
    // If zero, keep-alive are enabled if supported by the protocol
    // and operating system. Network protocols or operating systems
    // that do not support keep-alive ignore this field.
    // If negative, keep-alive are disabled.
    public time_package.Duration KeepAlive;
    // KeepAliveConfig specifies the keep-alive probe configuration
    // for an active network connection, when supported by the
    // protocol and operating system.
    //
    // If KeepAliveConfig.Enable is true, keep-alive probes are enabled.
    // If KeepAliveConfig.Enable is false and KeepAlive is negative,
    // keep-alive probes are disabled.
    public KeepAliveConfig KeepAliveConfig;
    // If mptcpStatus is set to a value allowing Multipath TCP (MPTCP) to be
    // used, any call to Listen with "tcp(4|6)" as network will use MPTCP if
    // supported by the operating system.
    internal mptcpStatus mptcpStatus;
}

// MultipathTCP reports whether MPTCP will be used.
//
// This method doesn't check if MPTCP is supported by the operating
// system or not.
[GoRecv] public static bool MultipathTCP(this ref ListenConfig lc) {
    return lc.mptcpStatus.get();
}

// SetMultipathTCP directs the [Listen] method to use, or not use, MPTCP,
// if supported by the operating system. This method overrides the
// system default and the GODEBUG=multipathtcp=... setting if any.
//
// If MPTCP is not available on the host or not supported by the client,
// the Listen method will fall back to TCP.
[GoRecv] public static void SetMultipathTCP(this ref ListenConfig lc, bool use) {
    lc.mptcpStatus.set(use);
}

// Listen announces on the local network address.
//
// See func Listen for a description of the network and address
// parameters.
[GoRecv] public static (Listener, error) Listen(this ref ListenConfig lc, context.Context ctx, @string network, @string address) {
    (addrs, err) = DefaultResolver.resolveAddrList(ctx, "listen"u8, network, address, default!);
    if (err != default!) {
        return (default!, new OpError(Op: "listen"u8, Net: network, Source: default!, ΔAddr: default!, Err: err));
    }
    var sl = Ꮡ(new sysListener(
        ListenConfig: lc,
        network: network,
        address: address
    ));
    Listener l = default!;
    var la = addrs.first(isIPv4);
    switch (la.type()) {
    case TCPAddr.val la: {
        if (sl.MultipathTCP()){
            (l, err) = sl.listenMPTCP(ctx, la);
        } else {
            (l, err) = sl.listenTCP(ctx, la);
        }
        break;
    }
    case UnixAddr.val la: {
        (l, err) = sl.listenUnix(ctx, la);
        break;
    }
    default: {
        var la = la.type();
        return (default!, new OpError(Op: "listen"u8, Net: (~sl).network, Source: default!, ΔAddr: la, Err: Ꮡ(new AddrError(Err: "unexpected address type"u8, ΔAddr: address))));
    }}
    if (err != default!) {
        return (default!, new OpError(Op: "listen"u8, Net: (~sl).network, Source: default!, ΔAddr: la, Err: err));
    }
    // l is non-nil interface containing nil pointer
    return (l, default!);
}

// ListenPacket announces on the local network address.
//
// See func ListenPacket for a description of the network and address
// parameters.
[GoRecv] public static (PacketConn, error) ListenPacket(this ref ListenConfig lc, context.Context ctx, @string network, @string address) {
    (addrs, err) = DefaultResolver.resolveAddrList(ctx, "listen"u8, network, address, default!);
    if (err != default!) {
        return (default!, new OpError(Op: "listen"u8, Net: network, Source: default!, ΔAddr: default!, Err: err));
    }
    var sl = Ꮡ(new sysListener(
        ListenConfig: lc,
        network: network,
        address: address
    ));
    PacketConn c = default!;
    var la = addrs.first(isIPv4);
    switch (la.type()) {
    case UDPAddr.val la: {
        (c, err) = sl.listenUDP(ctx, la);
        break;
    }
    case IPAddr.val la: {
        (c, err) = sl.listenIP(ctx, la);
        break;
    }
    case UnixAddr.val la: {
        (c, err) = sl.listenUnixgram(ctx, la);
        break;
    }
    default: {
        var la = la.type();
        return (default!, new OpError(Op: "listen"u8, Net: (~sl).network, Source: default!, ΔAddr: la, Err: Ꮡ(new AddrError(Err: "unexpected address type"u8, ΔAddr: address))));
    }}
    if (err != default!) {
        return (default!, new OpError(Op: "listen"u8, Net: (~sl).network, Source: default!, ΔAddr: la, Err: err));
    }
    // c is non-nil interface containing nil pointer
    return (c, default!);
}

// sysListener contains a Listen's parameters and configuration.
[GoType] partial struct sysListener {
    public partial ref ListenConfig ListenConfig { get; }
    internal @string network;
    internal @string address;
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
// The [Addr] method of [Listener] can be used to discover the chosen
// port.
//
// See func [Dial] for a description of the network and address
// parameters.
//
// Listen uses context.Background internally; to specify the context, use
// [ListenConfig.Listen].
public static (Listener, error) Listen(@string network, @string address) {
    ListenConfig lc = default!;
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
// The LocalAddr method of [PacketConn] can be used to discover the
// chosen port.
//
// See func [Dial] for a description of the network and address
// parameters.
//
// ListenPacket uses context.Background internally; to specify the context, use
// [ListenConfig.ListenPacket].
public static (PacketConn, error) ListenPacket(@string network, @string address) {
    ListenConfig lc = default!;
    return lc.ListenPacket(context.Background(), network, address);
}

} // end net_package
