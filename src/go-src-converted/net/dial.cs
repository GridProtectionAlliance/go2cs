// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:25:16 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\dial.go
using context = go.context_package;
using nettrace = go.@internal.nettrace_package;
using poll = go.@internal.poll_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class net_package
    {
        // A Dialer contains options for connecting to an address.
        //
        // The zero value for each field is equivalent to dialing
        // without that option. Dialing with the zero value of Dialer
        // is therefore equivalent to just calling the Dial function.
        public partial struct Dialer
        {
            public time.Duration Timeout; // Deadline is the absolute point in time after which dials
// will fail. If Timeout is set, it may fail earlier.
// Zero means no deadline, or dependent on the operating system
// as with the Timeout option.
            public time.Time Deadline; // LocalAddr is the local address to use when dialing an
// address. The address must be of a compatible type for the
// network being dialed.
// If nil, a local address is automatically chosen.
            public Addr LocalAddr; // DualStack enables RFC 6555-compliant "Happy Eyeballs"
// dialing when the network is "tcp" and the host in the
// address parameter resolves to both IPv4 and IPv6 addresses.
// This allows a client to tolerate networks where one address
// family is silently broken.
            public bool DualStack; // FallbackDelay specifies the length of time to wait before
// spawning a fallback connection, when DualStack is enabled.
// If zero, a default delay of 300ms is used.
            public time.Duration FallbackDelay; // KeepAlive specifies the keep-alive period for an active
// network connection.
// If zero, keep-alives are not enabled. Network protocols
// that do not support keep-alives ignore this field.
            public time.Duration KeepAlive; // Resolver optionally specifies an alternate resolver to use.
            public ptr<Resolver> Resolver; // Cancel is an optional channel whose closure indicates that
// the dial should be canceled. Not all types of dials support
// cancelation.
//
// Deprecated: Use DialContext instead.
            public channel<object> Cancel;
        }

        private static time.Time minNonzeroTime(time.Time a, time.Time b)
        {
            if (a.IsZero())
            {
                return b;
            }
            if (b.IsZero() || a.Before(b))
            {
                return a;
            }
            return b;
        }

        // deadline returns the earliest of:
        //   - now+Timeout
        //   - d.Deadline
        //   - the context's deadline
        // Or zero, if none of Timeout, Deadline, or context's deadline is set.
        private static time.Time deadline(this ref Dialer d, context.Context ctx, time.Time now)
        {
            if (d.Timeout != 0L)
            { // including negative, for historical reasons
                earliest = now.Add(d.Timeout);
            }
            {
                var (d, ok) = ctx.Deadline();

                if (ok)
                {
                    earliest = minNonzeroTime(earliest, d);
                }

            }
            return minNonzeroTime(earliest, d.Deadline);
        }

        private static ref Resolver resolver(this ref Dialer d)
        {
            if (d.Resolver != null)
            {
                return d.Resolver;
            }
            return DefaultResolver;
        }

        // partialDeadline returns the deadline to use for a single address,
        // when multiple addresses are pending.
        private static (time.Time, error) partialDeadline(time.Time now, time.Time deadline, long addrsRemaining)
        {
            if (deadline.IsZero())
            {
                return (deadline, null);
            }
            var timeRemaining = deadline.Sub(now);
            if (timeRemaining <= 0L)
            {
                return (new time.Time(), poll.ErrTimeout);
            } 
            // Tentatively allocate equal time to each remaining address.
            var timeout = timeRemaining / time.Duration(addrsRemaining); 
            // If the time per address is too short, steal from the end of the list.
            const long saneMinimum = 2L * time.Second;

            if (timeout < saneMinimum)
            {
                if (timeRemaining < saneMinimum)
                {
                    timeout = timeRemaining;
                }
                else
                {
                    timeout = saneMinimum;
                }
            }
            return (now.Add(timeout), null);
        }

        private static time.Duration fallbackDelay(this ref Dialer d)
        {
            if (d.FallbackDelay > 0L)
            {
                return d.FallbackDelay;
            }
            else
            {
                return 300L * time.Millisecond;
            }
        }

        private static (@string, long, error) parseNetwork(context.Context ctx, @string network, bool needsProto)
        {
            var i = last(network, ':');
            if (i < 0L)
            { // no colon
                switch (network)
                {
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
                        if (needsProto)
                        {
                            return ("", 0L, UnknownNetworkError(network));
                        }
                        break;
                    case "unix": 

                    case "unixgram": 

                    case "unixpacket": 
                        break;
                    default: 
                        return ("", 0L, UnknownNetworkError(network));
                        break;
                }
                return (network, 0L, null);
            }
            afnet = network[..i];
            switch (afnet)
            {
                case "ip": 

                case "ip4": 

                case "ip6": 
                    var protostr = network[i + 1L..];
                    var (proto, i, ok) = dtoi(protostr);
                    if (!ok || i != len(protostr))
                    {
                        proto, err = lookupProtocol(ctx, protostr);
                        if (err != null)
                        {
                            return ("", 0L, err);
                        }
                    }
                    return (afnet, proto, null);
                    break;
            }
            return ("", 0L, UnknownNetworkError(network));
        }

        // resolveAddrList resolves addr using hint and returns a list of
        // addresses. The result contains at least one address when error is
        // nil.
        private static (addrList, error) resolveAddrList(this ref Resolver r, context.Context ctx, @string op, @string network, @string addr, Addr hint)
        {
            var (afnet, _, err) = parseNetwork(ctx, network, true);
            if (err != null)
            {
                return (null, err);
            }
            if (op == "dial" && addr == "")
            {
                return (null, errMissingAddress);
            }
            switch (afnet)
            {
                case "unix": 

                case "unixgram": 

                case "unixpacket": 
                    var (addr, err) = ResolveUnixAddr(afnet, addr);
                    if (err != null)
                    {
                        return (null, err);
                    }
                    if (op == "dial" && hint != null && addr.Network() != hint.Network())
                    {
                        return (null, ref new AddrError(Err:"mismatched local address type",Addr:hint.String()));
                    }
                    return (new addrList(addr), null);
                    break;
            }
            var (addrs, err) = r.internetAddrList(ctx, afnet, addr);
            if (err != null || op != "dial" || hint == null)
            {
                return (addrs, err);
            }
            ref TCPAddr tcp = default;            ref UDPAddr udp = default;            ref IPAddr ip = default;            bool wildcard = default;
            switch (hint.type())
            {
                case ref TCPAddr hint:
                    tcp = hint;
                    wildcard = tcp.isWildcard();
                    break;
                case ref UDPAddr hint:
                    udp = hint;
                    wildcard = udp.isWildcard();
                    break;
                case ref IPAddr hint:
                    ip = hint;
                    wildcard = ip.isWildcard();
                    break;
            }
            var naddrs = addrs[..0L];
            {
                var addr__prev1 = addr;

                foreach (var (_, __addr) in addrs)
                {
                    addr = __addr;
                    if (addr.Network() != hint.Network())
                    {
                        return (null, ref new AddrError(Err:"mismatched local address type",Addr:hint.String()));
                    }
                    switch (addr.type())
                    {
                        case ref TCPAddr addr:
                            if (!wildcard && !addr.isWildcard() && !addr.IP.matchAddrFamily(tcp.IP))
                            {
                                continue;
                            }
                            naddrs = append(naddrs, addr);
                            break;
                        case ref UDPAddr addr:
                            if (!wildcard && !addr.isWildcard() && !addr.IP.matchAddrFamily(udp.IP))
                            {
                                continue;
                            }
                            naddrs = append(naddrs, addr);
                            break;
                        case ref IPAddr addr:
                            if (!wildcard && !addr.isWildcard() && !addr.IP.matchAddrFamily(ip.IP))
                            {
                                continue;
                            }
                            naddrs = append(naddrs, addr);
                            break;
                    }
                }

                addr = addr__prev1;
            }

            if (len(naddrs) == 0L)
            {
                return (null, ref new AddrError(Err:errNoSuitableAddress.Error(),Addr:hint.String()));
            }
            return (naddrs, null);
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
        public static (Conn, error) Dial(@string network, @string address)
        {
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
        public static (Conn, error) DialTimeout(@string network, @string address, time.Duration timeout)
        {
            Dialer d = new Dialer(Timeout:timeout);
            return d.Dial(network, address);
        }

        // dialParam contains a Dial's parameters and configuration.
        private partial struct dialParam
        {
            public ref Dialer Dialer => ref Dialer_val;
            public @string network;
            public @string address;
        }

        // Dial connects to the address on the named network.
        //
        // See func Dial for a description of the network and address
        // parameters.
        private static (Conn, error) Dial(this ref Dialer d, @string network, @string address)
        {
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
        private static (Conn, error) DialContext(this ref Dialer _d, context.Context ctx, @string network, @string address) => func(_d, (ref Dialer d, Defer defer, Panic panic, Recover _) =>
        {
            if (ctx == null)
            {
                panic("nil context");
            }
            var deadline = d.deadline(ctx, time.Now());
            if (!deadline.IsZero())
            {
                {
                    var (d, ok) = ctx.Deadline();

                    if (!ok || deadline.Before(d))
                    {
                        var (subCtx, cancel) = context.WithDeadline(ctx, deadline);
                        defer(cancel());
                        ctx = subCtx;
                    }

                }
            }
            {
                var oldCancel = d.Cancel;

                if (oldCancel != null)
                {
                    (subCtx, cancel) = context.WithCancel(ctx);
                    defer(cancel());
                    go_(() => () =>
                    {
                        cancel();
                    }());
                    ctx = subCtx;
                } 

                // Shadow the nettrace (if any) during resolve so Connect events don't fire for DNS lookups.

            } 

            // Shadow the nettrace (if any) during resolve so Connect events don't fire for DNS lookups.
            var resolveCtx = ctx;
            {
                ref nettrace.Trace (trace, _) = ctx.Value(new nettrace.TraceKey())._<ref nettrace.Trace>();

                if (trace != null)
                {
                    var shadow = trace.Value;
                    shadow.ConnectStart = null;
                    shadow.ConnectDone = null;
                    resolveCtx = context.WithValue(resolveCtx, new nettrace.TraceKey(), ref shadow);
                }

            }

            var (addrs, err) = d.resolver().resolveAddrList(resolveCtx, "dial", network, address, d.LocalAddr);
            if (err != null)
            {
                return (null, ref new OpError(Op:"dial",Net:network,Source:nil,Addr:nil,Err:err));
            }
            dialParam dp = ref new dialParam(Dialer:*d,network:network,address:address,);

            addrList primaries = default;            addrList fallbacks = default;

            if (d.DualStack && network == "tcp")
            {
                primaries, fallbacks = addrs.partition(isIPv4);
            }
            else
            {
                primaries = addrs;
            }
            Conn c = default;
            if (len(fallbacks) > 0L)
            {
                c, err = dialParallel(ctx, dp, primaries, fallbacks);
            }
            else
            {
                c, err = dialSerial(ctx, dp, primaries);
            }
            if (err != null)
            {
                return (null, err);
            }
            {
                ref TCPConn (tc, ok) = c._<ref TCPConn>();

                if (ok && d.KeepAlive > 0L)
                {
                    setKeepAlive(tc.fd, true);
                    setKeepAlivePeriod(tc.fd, d.KeepAlive);
                    testHookSetKeepAlive();
                }

            }
            return (c, null);
        });

        // dialParallel races two copies of dialSerial, giving the first a
        // head start. It returns the first established connection and
        // closes the others. Otherwise it returns an error from the first
        // primary address.
        private static (Conn, error) dialParallel(context.Context ctx, ref dialParam _dp, addrList primaries, addrList fallbacks) => func(_dp, (ref dialParam dp, Defer defer, Panic _, Recover __) =>
        {
            if (len(fallbacks) == 0L)
            {
                return dialSerial(ctx, dp, primaries);
            }
            var returned = make_channel<object>();
            defer(close(returned));

            private partial struct dialResult : error
            {
                public ref Conn Conn => ref Conn_val;
                public error error;
                public bool primary;
                public bool done;
            }
            var results = make_channel<dialResult>(); // unbuffered

            Action<context.Context, bool> startRacer = (ctx, primary) =>
            {
                var ras = primaries;
                if (!primary)
                {
                    ras = fallbacks;
                }
                var (c, err) = dialSerial(ctx, dp, ras);
                if (c != null)
                {
                    c.Close();
                }
            }
;

            dialResult primary = default;            dialResult fallback = default; 

            // Start the main racer.
 

            // Start the main racer.
            var (primaryCtx, primaryCancel) = context.WithCancel(ctx);
            defer(primaryCancel());
            go_(() => startRacer(primaryCtx, true)); 

            // Start the timer for the fallback racer.
            var fallbackTimer = time.NewTimer(dp.fallbackDelay());
            defer(fallbackTimer.Stop());

            while (true)
            {
                var (fallbackCtx, fallbackCancel) = context.WithCancel(ctx);
                defer(fallbackCancel());
                go_(() => startRacer(fallbackCtx, false));

                if (res.error == null)
                {
                    return (res.Conn, null);
                }
                if (res.primary)
                {
                    primary = res;
                }
                else
                {
                    fallback = res;
                }
                if (primary.done && fallback.done)
                {
                    return (null, primary.error);
                }
                if (res.primary && fallbackTimer.Stop())
                { 
                    // If we were able to stop the timer, that means it
                    // was running (hadn't yet started the fallback), but
                    // we just got an error on the primary path, so start
                    // the fallback immediately (in 0 nanoseconds).
                    fallbackTimer.Reset(0L);
                }
            }

        });

        // dialSerial connects to a list of addresses in sequence, returning
        // either the first successful connection, or the first error.
        private static (Conn, error) dialSerial(context.Context ctx, ref dialParam _dp, addrList ras) => func(_dp, (ref dialParam dp, Defer defer, Panic _, Recover __) =>
        {
            error firstErr = default; // The error from the first address is most relevant.

            foreach (var (i, ra) in ras)
            {
                return (null, ref new OpError(Op:"dial",Net:dp.network,Source:dp.LocalAddr,Addr:ra,Err:mapErr(ctx.Err())));
                var (deadline, _) = ctx.Deadline();
                var (partialDeadline, err) = partialDeadline(time.Now(), deadline, len(ras) - i);
                if (err != null)
                { 
                    // Ran out of time.
                    if (firstErr == null)
                    {
                        firstErr = error.As(ref new OpError(Op:"dial",Net:dp.network,Source:dp.LocalAddr,Addr:ra,Err:err));
                    }
                    break;
                }
                var dialCtx = ctx;
                if (partialDeadline.Before(deadline))
                {
                    context.CancelFunc cancel = default;
                    dialCtx, cancel = context.WithDeadline(ctx, partialDeadline);
                    defer(cancel());
                }
                var (c, err) = dialSingle(dialCtx, dp, ra);
                if (err == null)
                {
                    return (c, null);
                }
                if (firstErr == null)
                {
                    firstErr = error.As(err);
                }
            }
            if (firstErr == null)
            {
                firstErr = error.As(ref new OpError(Op:"dial",Net:dp.network,Source:nil,Addr:nil,Err:errMissingAddress));
            }
            return (null, firstErr);
        });

        // dialSingle attempts to establish and returns a single connection to
        // the destination address.
        private static (Conn, error) dialSingle(context.Context ctx, ref dialParam _dp, Addr ra) => func(_dp, (ref dialParam dp, Defer defer, Panic _, Recover __) =>
        {
            ref nettrace.Trace (trace, _) = ctx.Value(new nettrace.TraceKey())._<ref nettrace.Trace>();
            if (trace != null)
            {
                var raStr = ra.String();
                if (trace.ConnectStart != null)
                {
                    trace.ConnectStart(dp.network, raStr);
                }
                if (trace.ConnectDone != null)
                {
                    defer(() =>
                    {
                        trace.ConnectDone(dp.network, raStr, err);

                    }());
                }
            }
            var la = dp.LocalAddr;
            switch (ra.type())
            {
                case ref TCPAddr ra:
                    ref TCPAddr (la, _) = la._<ref TCPAddr>();
                    c, err = dialTCP(ctx, dp.network, la, ra);
                    break;
                case ref UDPAddr ra:
                    (la, _) = la._<ref UDPAddr>();
                    c, err = dialUDP(ctx, dp.network, la, ra);
                    break;
                case ref IPAddr ra:
                    (la, _) = la._<ref IPAddr>();
                    c, err = dialIP(ctx, dp.network, la, ra);
                    break;
                case ref UnixAddr ra:
                    (la, _) = la._<ref UnixAddr>();
                    c, err = dialUnix(ctx, dp.network, la, ra);
                    break;
                default:
                {
                    var ra = ra.type();
                    return (null, ref new OpError(Op:"dial",Net:dp.network,Source:la,Addr:ra,Err:&AddrError{Err:"unexpected address type",Addr:dp.address}));
                    break;
                }
            }
            if (err != null)
            {
                return (null, ref new OpError(Op:"dial",Net:dp.network,Source:la,Addr:ra,Err:err)); // c is non-nil interface containing nil pointer
            }
            return (c, null);
        });

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
        public static (Listener, error) Listen(@string network, @string address)
        {
            var (addrs, err) = DefaultResolver.resolveAddrList(context.Background(), "listen", network, address, null);
            if (err != null)
            {
                return (null, ref new OpError(Op:"listen",Net:network,Source:nil,Addr:nil,Err:err));
            }
            Listener l = default;
            switch (addrs.first(isIPv4).type())
            {
                case ref TCPAddr la:
                    l, err = ListenTCP(network, la);
                    break;
                case ref UnixAddr la:
                    l, err = ListenUnix(network, la);
                    break;
                default:
                {
                    var la = addrs.first(isIPv4).type();
                    return (null, ref new OpError(Op:"listen",Net:network,Source:nil,Addr:la,Err:&AddrError{Err:"unexpected address type",Addr:address}));
                    break;
                }
            }
            if (err != null)
            {
                return (null, err); // l is non-nil interface containing nil pointer
            }
            return (l, null);
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
        public static (PacketConn, error) ListenPacket(@string network, @string address)
        {
            var (addrs, err) = DefaultResolver.resolveAddrList(context.Background(), "listen", network, address, null);
            if (err != null)
            {
                return (null, ref new OpError(Op:"listen",Net:network,Source:nil,Addr:nil,Err:err));
            }
            PacketConn l = default;
            switch (addrs.first(isIPv4).type())
            {
                case ref UDPAddr la:
                    l, err = ListenUDP(network, la);
                    break;
                case ref IPAddr la:
                    l, err = ListenIP(network, la);
                    break;
                case ref UnixAddr la:
                    l, err = ListenUnixgram(network, la);
                    break;
                default:
                {
                    var la = addrs.first(isIPv4).type();
                    return (null, ref new OpError(Op:"listen",Net:network,Source:nil,Addr:la,Err:&AddrError{Err:"unexpected address type",Addr:address}));
                    break;
                }
            }
            if (err != null)
            {
                return (null, err); // l is non-nil interface containing nil pointer
            }
            return (l, null);
        }
    }
}
