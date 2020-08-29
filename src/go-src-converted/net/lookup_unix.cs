// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd solaris

// package net -- go2cs converted at 2020 August 29 08:27:00 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\lookup_unix.go
using context = go.context_package;
using sync = go.sync_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static sync.Once onceReadProtocols = default;

        // readProtocols loads contents of /etc/protocols into protocols map
        // for quick access.
        private static void readProtocols() => func((defer, _, __) =>
        {
            var (file, err) = open("/etc/protocols");
            if (err != null)
            {
                return;
            }
            defer(file.close());

            {
                var (line, ok) = file.readLine();

                while (ok)
                { 
                    // tcp    6   TCP    # transmission control protocol
                    {
                        var i = byteIndex(line, '#');

                        if (i >= 0L)
                        {
                            line = line[0L..i];
                    line, ok = file.readLine();
                        }

                    }
                    var f = getFields(line);
                    if (len(f) < 2L)
                    {
                        continue;
                    }
                    {
                        var (proto, _, ok) = dtoi(f[1L]);

                        if (ok)
                        {
                            {
                                var (_, ok) = protocols[f[0L]];

                                if (!ok)
                                {
                                    protocols[f[0L]] = proto;
                                }

                            }
                            foreach (var (_, alias) in f[2L..])
                            {
                                {
                                    (_, ok) = protocols[alias];

                                    if (!ok)
                                    {
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
        private static (long, error) lookupProtocol(context.Context _, @string name)
        {
            onceReadProtocols.Do(readProtocols);
            return lookupProtocolMap(name);
        }

        private static (dnsConn, error) dial(this ref Resolver r, context.Context ctx, @string network, @string server)
        { 
            // Calling Dial here is scary -- we have to be sure not to
            // dial a name that will require a DNS lookup, or Dial will
            // call back here to translate it. The DNS config parser has
            // already checked that all the cfg.servers are IP
            // addresses, which Dial will use without a DNS lookup.
            Conn c = default;
            error err = default;
            if (r.Dial != null)
            {
                c, err = r.Dial(ctx, network, server);
            }
            else
            {
                Dialer d = default;
                c, err = d.DialContext(ctx, network, server);
            }
            if (err != null)
            {
                return (null, mapErr(err));
            }
            {
                PacketConn (_, ok) = c._<PacketConn>();

                if (ok)
                {
                    return (ref new dnsPacketConn(c), null);
                }

            }
            return (ref new dnsStreamConn(c), null);
        }

        private static (slice<@string>, error) lookupHost(this ref Resolver r, context.Context ctx, @string host)
        {
            var order = systemConf().hostLookupOrder(host);
            if (!r.PreferGo && order == hostLookupCgo)
            {
                {
                    var (addrs, err, ok) = cgoLookupHost(ctx, host);

                    if (ok)
                    {
                        return (addrs, err);
                    } 
                    // cgo not available (or netgo); fall back to Go's DNS resolver

                } 
                // cgo not available (or netgo); fall back to Go's DNS resolver
                order = hostLookupFilesDNS;
            }
            return r.goLookupHostOrder(ctx, host, order);
        }

        private static (slice<IPAddr>, error) lookupIP(this ref Resolver r, context.Context ctx, @string host)
        {
            if (r.PreferGo)
            {
                return r.goLookupIP(ctx, host);
            }
            var order = systemConf().hostLookupOrder(host);
            if (order == hostLookupCgo)
            {
                {
                    var (addrs, err, ok) = cgoLookupIP(ctx, host);

                    if (ok)
                    {
                        return (addrs, err);
                    } 
                    // cgo not available (or netgo); fall back to Go's DNS resolver

                } 
                // cgo not available (or netgo); fall back to Go's DNS resolver
                order = hostLookupFilesDNS;
            }
            addrs, _, err = r.goLookupIPCNAMEOrder(ctx, host, order);
            return;
        }

        private static (long, error) lookupPort(this ref Resolver r, context.Context ctx, @string network, @string service)
        {
            if (!r.PreferGo && systemConf().canUseCgo())
            {
                {
                    var port__prev2 = port;

                    var (port, err, ok) = cgoLookupPort(ctx, network, service);

                    if (ok)
                    {
                        if (err != null)
                        { 
                            // Issue 18213: if cgo fails, first check to see whether we
                            // have the answer baked-in to the net package.
                            {
                                var port__prev4 = port;

                                var (port, err) = goLookupPort(network, service);

                                if (err == null)
                                {
                                    return (port, null);
                                }

                                port = port__prev4;

                            }
                        }
                        return (port, err);
                    }

                    port = port__prev2;

                }
            }
            return goLookupPort(network, service);
        }

        private static (@string, error) lookupCNAME(this ref Resolver r, context.Context ctx, @string name)
        {
            if (!r.PreferGo && systemConf().canUseCgo())
            {
                {
                    var (cname, err, ok) = cgoLookupCNAME(ctx, name);

                    if (ok)
                    {
                        return (cname, err);
                    }

                }
            }
            return r.goLookupCNAME(ctx, name);
        }

        private static (@string, slice<ref SRV>, error) lookupSRV(this ref Resolver r, context.Context ctx, @string service, @string proto, @string name)
        {
            @string target = default;
            if (service == "" && proto == "")
            {
                target = name;
            }
            else
            {
                target = "_" + service + "._" + proto + "." + name;
            }
            var (cname, rrs, err) = r.lookup(ctx, target, dnsTypeSRV);
            if (err != null)
            {
                return ("", null, err);
            }
            var srvs = make_slice<ref SRV>(len(rrs));
            {
                var rr__prev1 = rr;

                foreach (var (__i, __rr) in rrs)
                {
                    i = __i;
                    rr = __rr;
                    ref dnsRR_SRV rr = rr._<ref dnsRR_SRV>();
                    srvs[i] = ref new SRV(Target:rr.Target,Port:rr.Port,Priority:rr.Priority,Weight:rr.Weight);
                }

                rr = rr__prev1;
            }

            byPriorityWeight(srvs).sort();
            return (cname, srvs, null);
        }

        private static (slice<ref MX>, error) lookupMX(this ref Resolver r, context.Context ctx, @string name)
        {
            var (_, rrs, err) = r.lookup(ctx, name, dnsTypeMX);
            if (err != null)
            {
                return (null, err);
            }
            var mxs = make_slice<ref MX>(len(rrs));
            {
                var rr__prev1 = rr;

                foreach (var (__i, __rr) in rrs)
                {
                    i = __i;
                    rr = __rr;
                    ref dnsRR_MX rr = rr._<ref dnsRR_MX>();
                    mxs[i] = ref new MX(Host:rr.Mx,Pref:rr.Pref);
                }

                rr = rr__prev1;
            }

            byPref(mxs).sort();
            return (mxs, null);
        }

        private static (slice<ref NS>, error) lookupNS(this ref Resolver r, context.Context ctx, @string name)
        {
            var (_, rrs, err) = r.lookup(ctx, name, dnsTypeNS);
            if (err != null)
            {
                return (null, err);
            }
            var nss = make_slice<ref NS>(len(rrs));
            foreach (var (i, rr) in rrs)
            {
                nss[i] = ref new NS(Host:rr.(*dnsRR_NS).Ns);
            }
            return (nss, null);
        }

        private static (slice<@string>, error) lookupTXT(this ref Resolver r, context.Context ctx, @string name)
        {
            var (_, rrs, err) = r.lookup(ctx, name, dnsTypeTXT);
            if (err != null)
            {
                return (null, err);
            }
            var txts = make_slice<@string>(len(rrs));
            foreach (var (i, rr) in rrs)
            {
                txts[i] = rr._<ref dnsRR_TXT>().Txt;
            }
            return (txts, null);
        }

        private static (slice<@string>, error) lookupAddr(this ref Resolver r, context.Context ctx, @string addr)
        {
            if (!r.PreferGo && systemConf().canUseCgo())
            {
                {
                    var (ptrs, err, ok) = cgoLookupPTR(ctx, addr);

                    if (ok)
                    {
                        return (ptrs, err);
                    }

                }
            }
            return r.goLookupPTR(ctx, addr);
        }
    }
}
