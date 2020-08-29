// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package proxy -- go2cs converted at 2020 August 29 10:12:25 UTC
// import "vendor/golang_org/x/net/proxy" ==> using proxy = go.vendor.golang_org.x.net.proxy_package
// Original source: C:\Go\src\vendor\golang_org\x\net\proxy\per_host.go
using net = go.net_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class proxy_package
    {
        // A PerHost directs connections to a default Dialer unless the hostname
        // requested matches one of a number of exceptions.
        public partial struct PerHost
        {
            public Dialer def;
            public Dialer bypass;
            public slice<ref net.IPNet> bypassNetworks;
            public slice<net.IP> bypassIPs;
            public slice<@string> bypassZones;
            public slice<@string> bypassHosts;
        }

        // NewPerHost returns a PerHost Dialer that directs connections to either
        // defaultDialer or bypass, depending on whether the connection matches one of
        // the configured rules.
        public static ref PerHost NewPerHost(Dialer defaultDialer, Dialer bypass)
        {
            return ref new PerHost(def:defaultDialer,bypass:bypass,);
        }

        // Dial connects to the address addr on the given network through either
        // defaultDialer or bypass.
        private static (net.Conn, error) Dial(this ref PerHost p, @string network, @string addr)
        {
            var (host, _, err) = net.SplitHostPort(addr);
            if (err != null)
            {
                return (null, err);
            }
            return p.dialerForRequest(host).Dial(network, addr);
        }

        private static Dialer dialerForRequest(this ref PerHost p, @string host)
        {
            {
                var ip = net.ParseIP(host);

                if (ip != null)
                {
                    foreach (var (_, net) in p.bypassNetworks)
                    {
                        if (net.Contains(ip))
                        {
                            return p.bypass;
                        }
                    }
                    foreach (var (_, bypassIP) in p.bypassIPs)
                    {
                        if (bypassIP.Equal(ip))
                        {
                            return p.bypass;
                        }
                    }
                    return p.def;
                }

            }

            foreach (var (_, zone) in p.bypassZones)
            {
                if (strings.HasSuffix(host, zone))
                {
                    return p.bypass;
                }
                if (host == zone[1L..])
                { 
                    // For a zone "example.com", we match "example.com"
                    // too.
                    return p.bypass;
                }
            }
            foreach (var (_, bypassHost) in p.bypassHosts)
            {
                if (bypassHost == host)
                {
                    return p.bypass;
                }
            }
            return p.def;
        }

        // AddFromString parses a string that contains comma-separated values
        // specifying hosts that should use the bypass proxy. Each value is either an
        // IP address, a CIDR range, a zone (*.example.com) or a hostname
        // (localhost). A best effort is made to parse the string and errors are
        // ignored.
        private static void AddFromString(this ref PerHost p, @string s)
        {
            var hosts = strings.Split(s, ",");
            foreach (var (_, host) in hosts)
            {
                host = strings.TrimSpace(host);
                if (len(host) == 0L)
                {
                    continue;
                }
                if (strings.Contains(host, "/"))
                { 
                    // We assume that it's a CIDR address like 127.0.0.0/8
                    {
                        var (_, net, err) = net.ParseCIDR(host);

                        if (err == null)
                        {
                            p.AddNetwork(net);
                        }

                    }
                    continue;
                }
                {
                    var ip = net.ParseIP(host);

                    if (ip != null)
                    {
                        p.AddIP(ip);
                        continue;
                    }

                }
                if (strings.HasPrefix(host, "*."))
                {
                    p.AddZone(host[1L..]);
                    continue;
                }
                p.AddHost(host);
            }
        }

        // AddIP specifies an IP address that will use the bypass proxy. Note that
        // this will only take effect if a literal IP address is dialed. A connection
        // to a named host will never match an IP.
        private static void AddIP(this ref PerHost p, net.IP ip)
        {
            p.bypassIPs = append(p.bypassIPs, ip);
        }

        // AddNetwork specifies an IP range that will use the bypass proxy. Note that
        // this will only take effect if a literal IP address is dialed. A connection
        // to a named host will never match.
        private static void AddNetwork(this ref PerHost p, ref net.IPNet net)
        {
            p.bypassNetworks = append(p.bypassNetworks, net);
        }

        // AddZone specifies a DNS suffix that will use the bypass proxy. A zone of
        // "example.com" matches "example.com" and all of its subdomains.
        private static void AddZone(this ref PerHost p, @string zone)
        {
            if (strings.HasSuffix(zone, "."))
            {
                zone = zone[..len(zone) - 1L];
            }
            if (!strings.HasPrefix(zone, "."))
            {
                zone = "." + zone;
            }
            p.bypassZones = append(p.bypassZones, zone);
        }

        // AddHost specifies a hostname that will use the bypass proxy.
        private static void AddHost(this ref PerHost p, @string host)
        {
            if (strings.HasSuffix(host, "."))
            {
                host = host[..len(host) - 1L];
            }
            p.bypassHosts = append(p.bypassHosts, host);
        }
    }
}}}}}
