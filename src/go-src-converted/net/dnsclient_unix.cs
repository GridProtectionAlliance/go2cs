// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd solaris

// DNS client: see RFC 1035.
// Has to be linked into package net for Dial.

// TODO(rsc):
//    Could potentially handle many outstanding lookups faster.
//    Could have a small cache.
//    Random UDP source port (net.Dial should do that for us).
//    Random request IDs.

// package net -- go2cs converted at 2020 August 29 08:25:58 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\dnsclient_unix.go
using context = go.context_package;
using errors = go.errors_package;
using io = go.io_package;
using rand = go.math.rand_package;
using os = go.os_package;
using sync = go.sync_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class net_package
    {
        // A dnsConn represents a DNS transport endpoint.
        private partial interface dnsConn : io.Closer
        {
            (ref dnsMsg, error) SetDeadline(time.Time _p0); // dnsRoundTrip executes a single DNS transaction, returning a
// DNS response message for the provided DNS query message.
            (ref dnsMsg, error) dnsRoundTrip(ref dnsMsg query);
        }

        // dnsPacketConn implements the dnsConn interface for RFC 1035's
        // "UDP usage" transport mechanism. Conn is a packet-oriented connection,
        // such as a *UDPConn.
        private partial struct dnsPacketConn
        {
            public ref Conn Conn => ref Conn_val;
        }

        private static (ref dnsMsg, error) dnsRoundTrip(this ref dnsPacketConn c, ref dnsMsg query)
        {
            var (b, ok) = query.Pack();
            if (!ok)
            {
                return (null, errors.New("cannot marshal DNS message"));
            }
            {
                var (_, err) = c.Write(b);

                if (err != null)
                {
                    return (null, err);
                }

            }

            b = make_slice<byte>(512L); // see RFC 1035
            while (true)
            {
                var (n, err) = c.Read(b);
                if (err != null)
                {
                    return (null, err);
                }
                dnsMsg resp = ref new dnsMsg();
                if (!resp.Unpack(b[..n]) || !resp.IsResponseTo(query))
                { 
                    // Ignore invalid responses as they may be malicious
                    // forgery attempts. Instead continue waiting until
                    // timeout. See golang.org/issue/13281.
                    continue;
                }
                return (resp, null);
            }

        }

        // dnsStreamConn implements the dnsConn interface for RFC 1035's
        // "TCP usage" transport mechanism. Conn is a stream-oriented connection,
        // such as a *TCPConn.
        private partial struct dnsStreamConn
        {
            public ref Conn Conn => ref Conn_val;
        }

        private static (ref dnsMsg, error) dnsRoundTrip(this ref dnsStreamConn c, ref dnsMsg query)
        {
            var (b, ok) = query.Pack();
            if (!ok)
            {
                return (null, errors.New("cannot marshal DNS message"));
            }
            var l = len(b);
            b = append(new slice<byte>(new byte[] { byte(l>>8), byte(l) }), b);
            {
                var (_, err) = c.Write(b);

                if (err != null)
                {
                    return (null, err);
                }

            }

            b = make_slice<byte>(1280L); // 1280 is a reasonable initial size for IP over Ethernet, see RFC 4035
            {
                (_, err) = io.ReadFull(c, b[..2L]);

                if (err != null)
                {
                    return (null, err);
                }

            }
            l = int(b[0L]) << (int)(8L) | int(b[1L]);
            if (l > len(b))
            {
                b = make_slice<byte>(l);
            }
            var (n, err) = io.ReadFull(c, b[..l]);
            if (err != null)
            {
                return (null, err);
            }
            dnsMsg resp = ref new dnsMsg();
            if (!resp.Unpack(b[..n]))
            {
                return (null, errors.New("cannot unmarshal DNS message"));
            }
            if (!resp.IsResponseTo(query))
            {
                return (null, errors.New("invalid DNS response"));
            }
            return (resp, null);
        }

        // exchange sends a query on the connection and hopes for a response.
        private static (ref dnsMsg, error) exchange(this ref Resolver _r, context.Context ctx, @string server, @string name, ushort qtype, time.Duration timeout) => func(_r, (ref Resolver r, Defer defer, Panic _, Recover __) =>
        {
            dnsMsg @out = new dnsMsg(dnsMsgHdr:dnsMsgHdr{recursion_desired:true,},question:[]dnsQuestion{{name,qtype,dnsClassINET},},);
            foreach (var (_, network) in new slice<@string>(new @string[] { "udp", "tcp" }))
            { 
                // TODO(mdempsky): Refactor so defers from UDP-based
                // exchanges happen before TCP-based exchange.

                var (ctx, cancel) = context.WithDeadline(ctx, time.Now().Add(timeout));
                defer(cancel());

                var (c, err) = r.dial(ctx, network, server);
                if (err != null)
                {
                    return (null, err);
                }
                defer(c.Close());
                {
                    var (d, ok) = ctx.Deadline();

                    if (ok && !d.IsZero())
                    {
                        c.SetDeadline(d);
                    }

                }
                @out.id = uint16(rand.Int()) ^ uint16(time.Now().UnixNano());
                var (in, err) = c.dnsRoundTrip(ref out);
                if (err != null)
                {
                    return (null, mapErr(err));
                }
                if (@in.truncated)
                { // see RFC 5966
                    continue;
                }
                return (in, null);
            }
            return (null, errors.New("no answer from DNS server"));
        });

        // Do a lookup for a single name, which must be rooted
        // (otherwise answer will not find the answers).
        private static (@string, slice<dnsRR>, error) tryOneName(this ref Resolver r, context.Context ctx, ref dnsConfig cfg, @string name, ushort qtype)
        {
            error lastErr = default;
            var serverOffset = cfg.serverOffset();
            var sLen = uint32(len(cfg.servers));

            for (long i = 0L; i < cfg.attempts; i++)
            {
                for (var j = uint32(0L); j < sLen; j++)
                {
                    var server = cfg.servers[(serverOffset + j) % sLen];

                    var (msg, err) = r.exchange(ctx, server, name, qtype, cfg.timeout);
                    if (err != null)
                    {
                        lastErr = error.As(ref new DNSError(Err:err.Error(),Name:name,Server:server,));
                        {
                            Error (nerr, ok) = err._<Error>();

                            if (ok && nerr.Timeout())
                            {
                                lastErr._<ref DNSError>().IsTimeout = true;
                            } 
                            // Set IsTemporary for socket-level errors. Note that this flag
                            // may also be used to indicate a SERVFAIL response.

                        } 
                        // Set IsTemporary for socket-level errors. Note that this flag
                        // may also be used to indicate a SERVFAIL response.
                        {
                            ref OpError (_, ok) = err._<ref OpError>();

                            if (ok)
                            {
                                lastErr._<ref DNSError>().IsTemporary = true;
                            }

                        }
                        continue;
                    } 
                    // libresolv continues to the next server when it receives
                    // an invalid referral response. See golang.org/issue/15434.
                    if (msg.rcode == dnsRcodeSuccess && !msg.authoritative && !msg.recursion_available && len(msg.answer) == 0L && len(msg.extra) == 0L)
                    {
                        lastErr = error.As(ref new DNSError(Err:"lame referral",Name:name,Server:server));
                        continue;
                    }
                    var (cname, rrs, err) = answer(name, server, msg, qtype); 
                    // If answer errored for rcodes dnsRcodeSuccess or dnsRcodeNameError,
                    // it means the response in msg was not useful and trying another
                    // server probably won't help. Return now in those cases.
                    // TODO: indicate this in a more obvious way, such as a field on DNSError?
                    if (err == null || msg.rcode == dnsRcodeSuccess || msg.rcode == dnsRcodeNameError)
                    {
                        return (cname, rrs, err);
                    }
                    lastErr = error.As(err);
                }

            }

            return ("", null, lastErr);
        }

        // addrRecordList converts and returns a list of IP addresses from DNS
        // address records (both A and AAAA). Other record types are ignored.
        private static slice<IPAddr> addrRecordList(slice<dnsRR> rrs)
        {
            var addrs = make_slice<IPAddr>(0L, 4L);
            {
                var rr__prev1 = rr;

                foreach (var (_, __rr) in rrs)
                {
                    rr = __rr;
                    switch (rr.type())
                    {
                        case ref dnsRR_A rr:
                            addrs = append(addrs, new IPAddr(IP:IPv4(byte(rr.A>>24),byte(rr.A>>16),byte(rr.A>>8),byte(rr.A))));
                            break;
                        case ref dnsRR_AAAA rr:
                            var ip = make(IP, IPv6len);
                            copy(ip, rr.AAAA[..]);
                            addrs = append(addrs, new IPAddr(IP:ip));
                            break;
                    }
                }

                rr = rr__prev1;
            }

            return addrs;
        }

        // A resolverConfig represents a DNS stub resolver configuration.
        private partial struct resolverConfig
        {
            public sync.Once initOnce; // guards init of resolverConfig

// ch is used as a semaphore that only allows one lookup at a
// time to recheck resolv.conf.
            public channel<object> ch; // guards lastChecked and modTime
            public time.Time lastChecked; // last time resolv.conf was checked

            public sync.RWMutex mu; // protects dnsConfig
            public ptr<dnsConfig> dnsConfig; // parsed resolv.conf structure used in lookups
        }

        private static resolverConfig resolvConf = default;

        // init initializes conf and is only called via conf.initOnce.
        private static void init(this ref resolverConfig conf)
        { 
            // Set dnsConfig and lastChecked so we don't parse
            // resolv.conf twice the first time.
            conf.dnsConfig = systemConf().resolv;
            if (conf.dnsConfig == null)
            {
                conf.dnsConfig = dnsReadConfig("/etc/resolv.conf");
            }
            conf.lastChecked = time.Now(); 

            // Prepare ch so that only one update of resolverConfig may
            // run at once.
            conf.ch = make_channel<object>(1L);
        }

        // tryUpdate tries to update conf with the named resolv.conf file.
        // The name variable only exists for testing. It is otherwise always
        // "/etc/resolv.conf".
        private static void tryUpdate(this ref resolverConfig _conf, @string name) => func(_conf, (ref resolverConfig conf, Defer defer, Panic _, Recover __) =>
        {
            conf.initOnce.Do(conf.init); 

            // Ensure only one update at a time checks resolv.conf.
            if (!conf.tryAcquireSema())
            {
                return;
            }
            defer(conf.releaseSema());

            var now = time.Now();
            if (conf.lastChecked.After(now.Add(-5L * time.Second)))
            {
                return;
            }
            conf.lastChecked = now;

            time.Time mtime = default;
            {
                var (fi, err) = os.Stat(name);

                if (err == null)
                {
                    mtime = fi.ModTime();
                }

            }
            if (mtime.Equal(conf.dnsConfig.mtime))
            {
                return;
            }
            var dnsConf = dnsReadConfig(name);
            conf.mu.Lock();
            conf.dnsConfig = dnsConf;
            conf.mu.Unlock();
        });

        private static bool tryAcquireSema(this ref resolverConfig conf)
        {
            return true;
            return false;
        }

        private static void releaseSema(this ref resolverConfig conf)
        {
            conf.ch.Receive();
        }

        private static (@string, slice<dnsRR>, error) lookup(this ref Resolver r, context.Context ctx, @string name, ushort qtype)
        {
            if (!isDomainName(name))
            { 
                // We used to use "invalid domain name" as the error,
                // but that is a detail of the specific lookup mechanism.
                // Other lookups might allow broader name syntax
                // (for example Multicast DNS allows UTF-8; see RFC 6762).
                // For consistency with libc resolvers, report no such host.
                return ("", null, ref new DNSError(Err:errNoSuchHost.Error(),Name:name));
            }
            resolvConf.tryUpdate("/etc/resolv.conf");
            resolvConf.mu.RLock();
            var conf = resolvConf.dnsConfig;
            resolvConf.mu.RUnlock();
            foreach (var (_, fqdn) in conf.nameList(name))
            {
                cname, rrs, err = r.tryOneName(ctx, conf, fqdn, qtype);
                if (err == null)
                {
                    break;
                }
                {
                    Error (nerr, ok) = err._<Error>();

                    if (ok && nerr.Temporary() && r.StrictErrors)
                    { 
                        // If we hit a temporary error with StrictErrors enabled,
                        // stop immediately instead of trying more names.
                        break;
                    }

                }
            }
            {
                ref DNSError (err, ok) = err._<ref DNSError>();

                if (ok)
                { 
                    // Show original name passed to lookup, not suffixed one.
                    // In general we might have tried many suffixes; showing
                    // just one is misleading. See also golang.org/issue/6324.
                    err.Name = name;
                }

            }
            return;
        }

        // avoidDNS reports whether this is a hostname for which we should not
        // use DNS. Currently this includes only .onion, per RFC 7686. See
        // golang.org/issue/13705. Does not cover .local names (RFC 6762),
        // see golang.org/issue/16739.
        private static bool avoidDNS(@string name)
        {
            if (name == "")
            {
                return true;
            }
            if (name[len(name) - 1L] == '.')
            {
                name = name[..len(name) - 1L];
            }
            return stringsHasSuffixFold(name, ".onion");
        }

        // nameList returns a list of names for sequential DNS queries.
        private static slice<@string> nameList(this ref dnsConfig conf, @string name)
        {
            if (avoidDNS(name))
            {
                return null;
            } 

            // Check name length (see isDomainName).
            var l = len(name);
            var rooted = l > 0L && name[l - 1L] == '.';
            if (l > 254L || l == 254L && rooted)
            {
                return null;
            } 

            // If name is rooted (trailing dot), try only that name.
            if (rooted)
            {
                return new slice<@string>(new @string[] { name });
            }
            var hasNdots = count(name, '.') >= conf.ndots;
            name += ".";
            l++; 

            // Build list of search choices.
            var names = make_slice<@string>(0L, 1L + len(conf.search)); 
            // If name has enough dots, try unsuffixed first.
            if (hasNdots)
            {
                names = append(names, name);
            } 
            // Try suffixes that are not too long (see isDomainName).
            foreach (var (_, suffix) in conf.search)
            {
                if (l + len(suffix) <= 254L)
                {
                    names = append(names, name + suffix);
                }
            } 
            // Try unsuffixed, if not tried first above.
            if (!hasNdots)
            {
                names = append(names, name);
            }
            return names;
        }

        // hostLookupOrder specifies the order of LookupHost lookup strategies.
        // It is basically a simplified representation of nsswitch.conf.
        // "files" means /etc/hosts.
        private partial struct hostLookupOrder // : long
        {
        }

 
        // hostLookupCgo means defer to cgo.
        private static readonly hostLookupOrder hostLookupCgo = iota;
        private static readonly var hostLookupFilesDNS = 0; // files first
        private static readonly var hostLookupDNSFiles = 1; // dns first
        private static readonly var hostLookupFiles = 2; // only files
        private static readonly var hostLookupDNS = 3; // only DNS

        private static map lookupOrderName = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<hostLookupOrder, @string>{hostLookupCgo:"cgo",hostLookupFilesDNS:"files,dns",hostLookupDNSFiles:"dns,files",hostLookupFiles:"files",hostLookupDNS:"dns",};

        private static @string String(this hostLookupOrder o)
        {
            {
                var (s, ok) = lookupOrderName[o];

                if (ok)
                {
                    return s;
                }

            }
            return "hostLookupOrder=" + itoa(int(o)) + "??";
        }

        // goLookupHost is the native Go implementation of LookupHost.
        // Used only if cgoLookupHost refuses to handle the request
        // (that is, only if cgoLookupHost is the stub in cgo_stub.go).
        // Normally we let cgo use the C library resolver instead of
        // depending on our lookup code, so that Go and C get the same
        // answers.
        private static (slice<@string>, error) goLookupHost(this ref Resolver r, context.Context ctx, @string name)
        {
            return r.goLookupHostOrder(ctx, name, hostLookupFilesDNS);
        }

        private static (slice<@string>, error) goLookupHostOrder(this ref Resolver r, context.Context ctx, @string name, hostLookupOrder order)
        {
            if (order == hostLookupFilesDNS || order == hostLookupFiles)
            { 
                // Use entries from /etc/hosts if they match.
                addrs = lookupStaticHost(name);
                if (len(addrs) > 0L || order == hostLookupFiles)
                {
                    return;
                }
            }
            var (ips, _, err) = r.goLookupIPCNAMEOrder(ctx, name, order);
            if (err != null)
            {
                return;
            }
            addrs = make_slice<@string>(0L, len(ips));
            foreach (var (_, ip) in ips)
            {
                addrs = append(addrs, ip.String());
            }
            return;
        }

        // lookup entries from /etc/hosts
        private static slice<IPAddr> goLookupIPFiles(@string name)
        {
            {
                var haddr__prev1 = haddr;

                foreach (var (_, __haddr) in lookupStaticHost(name))
                {
                    haddr = __haddr;
                    var (haddr, zone) = splitHostZone(haddr);
                    {
                        var ip = ParseIP(haddr);

                        if (ip != null)
                        {
                            IPAddr addr = new IPAddr(IP:ip,Zone:zone);
                            addrs = append(addrs, addr);
                        }

                    }
                }

                haddr = haddr__prev1;
            }

            sortByRFC6724(addrs);
            return;
        }

        // goLookupIP is the native Go implementation of LookupIP.
        // The libc versions are in cgo_*.go.
        private static (slice<IPAddr>, error) goLookupIP(this ref Resolver r, context.Context ctx, @string host)
        {
            var order = systemConf().hostLookupOrder(host);
            addrs, _, err = r.goLookupIPCNAMEOrder(ctx, host, order);
            return;
        }

        private static (slice<IPAddr>, @string, error) goLookupIPCNAMEOrder(this ref Resolver _r, context.Context ctx, @string name, hostLookupOrder order) => func(_r, (ref Resolver r, Defer defer, Panic _, Recover __) =>
        {
            if (order == hostLookupFilesDNS || order == hostLookupFiles)
            {
                addrs = goLookupIPFiles(name);
                if (len(addrs) > 0L || order == hostLookupFiles)
                {
                    return (addrs, name, null);
                }
            }
            if (!isDomainName(name))
            { 
                // See comment in func lookup above about use of errNoSuchHost.
                return (null, "", ref new DNSError(Err:errNoSuchHost.Error(),Name:name));
            }
            resolvConf.tryUpdate("/etc/resolv.conf");
            resolvConf.mu.RLock();
            var conf = resolvConf.dnsConfig;
            resolvConf.mu.RUnlock();
            private partial struct racer : error
            {
                public @string cname;
                public slice<dnsRR> rrs;
                public error error;
            }
            var lane = make_channel<racer>(1L);
            array<ushort> qtypes = new array<ushort>(new ushort[] { dnsTypeA, dnsTypeAAAA });
            error lastErr = default;
            foreach (var (_, fqdn) in conf.nameList(name))
            {
                foreach (var (_, qtype) in qtypes)
                {
                    dnsWaitGroup.Add(1L);
                    go_(() => qtype =>
                    {
                        defer(dnsWaitGroup.Done());
                        var (cname, rrs, err) = r.tryOneName(ctx, conf, fqdn, qtype);
                        lane.Send(new racer(cname,rrs,err));
                    }(qtype));
                }
                var hitStrictError = false;
                foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_2<< in qtypes)
                {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_2<<
                    var racer = lane.Receive();
                    if (racer.error != null)
                    {
                        {
                            Error (nerr, ok) = racer.error._<Error>();

                            if (ok && nerr.Temporary() && r.StrictErrors)
                            { 
                                // This error will abort the nameList loop.
                                hitStrictError = true;
                                lastErr = error.As(racer.error);
                            }
                            else if (lastErr == null || fqdn == name + ".")
                            { 
                                // Prefer error for original name.
                                lastErr = error.As(racer.error);
                            }

                        }
                        continue;
                    }
                    addrs = append(addrs, addrRecordList(racer.rrs));
                    if (cname == "")
                    {
                        cname = racer.cname;
                    }
                }
                if (hitStrictError)
                { 
                    // If either family hit an error with StrictErrors enabled,
                    // discard all addresses. This ensures that network flakiness
                    // cannot turn a dualstack hostname IPv4/IPv6-only.
                    addrs = null;
                    break;
                }
                if (len(addrs) > 0L)
                {
                    break;
                }
            }
            {
                error lastErr__prev1 = lastErr;

                ref DNSError (lastErr, ok) = lastErr._<ref DNSError>();

                if (ok)
                { 
                    // Show original name passed to lookup, not suffixed one.
                    // In general we might have tried many suffixes; showing
                    // just one is misleading. See also golang.org/issue/6324.
                    lastErr.Name = name;
                }

                lastErr = lastErr__prev1;

            }
            sortByRFC6724(addrs);
            if (len(addrs) == 0L)
            {
                if (order == hostLookupDNSFiles)
                {
                    addrs = goLookupIPFiles(name);
                }
                if (len(addrs) == 0L && lastErr != null)
                {
                    return (null, "", lastErr);
                }
            }
            return (addrs, cname, null);
        });

        // goLookupCNAME is the native Go (non-cgo) implementation of LookupCNAME.
        private static (@string, error) goLookupCNAME(this ref Resolver r, context.Context ctx, @string host)
        {
            var order = systemConf().hostLookupOrder(host);
            _, cname, err = r.goLookupIPCNAMEOrder(ctx, host, order);
            return;
        }

        // goLookupPTR is the native Go implementation of LookupAddr.
        // Used only if cgoLookupPTR refuses to handle the request (that is,
        // only if cgoLookupPTR is the stub in cgo_stub.go).
        // Normally we let cgo use the C library resolver instead of depending
        // on our lookup code, so that Go and C get the same answers.
        private static (slice<@string>, error) goLookupPTR(this ref Resolver r, context.Context ctx, @string addr)
        {
            var names = lookupStaticAddr(addr);
            if (len(names) > 0L)
            {
                return (names, null);
            }
            var (arpa, err) = reverseaddr(addr);
            if (err != null)
            {
                return (null, err);
            }
            var (_, rrs, err) = r.lookup(ctx, arpa, dnsTypePTR);
            if (err != null)
            {
                return (null, err);
            }
            var ptrs = make_slice<@string>(len(rrs));
            foreach (var (i, rr) in rrs)
            {
                ptrs[i] = rr._<ref dnsRR_PTR>().Ptr;
            }
            return (ptrs, null);
        }
    }
}
