// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package nettest -- go2cs converted at 2020 August 29 10:12:11 UTC
// import "vendor/golang_org/x/net/internal/nettest" ==> using nettest = go.vendor.golang_org.x.net.@internal.nettest_package
// Original source: C:\Go\src\vendor\golang_org\x\net\internal\nettest\interface.go
using net = go.net_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net {
namespace @internal
{
    public static partial class nettest_package
    {
        // IsMulticastCapable reports whether ifi is an IP multicast-capable
        // network interface. Network must be "ip", "ip4" or "ip6".
        public static (net.IP, bool) IsMulticastCapable(@string network, ref net.Interface ifi)
        {
            switch (network)
            {
                case "ip": 

                case "ip4": 

                case "ip6": 
                    break;
                default: 
                    return (null, false);
                    break;
            }
            if (ifi == null || ifi.Flags & net.FlagUp == 0L || ifi.Flags & net.FlagMulticast == 0L)
            {
                return (null, false);
            }
            return hasRoutableIP(network, ifi);
        }

        // RoutedInterface returns a network interface that can route IP
        // traffic and satisfies flags. It returns nil when an appropriate
        // network interface is not found. Network must be "ip", "ip4" or
        // "ip6".
        public static ref net.Interface RoutedInterface(@string network, net.Flags flags)
        {
            switch (network)
            {
                case "ip": 

                case "ip4": 

                case "ip6": 
                    break;
                default: 
                    return null;
                    break;
            }
            var (ift, err) = net.Interfaces();
            if (err != null)
            {
                return null;
            }
            foreach (var (_, ifi) in ift)
            {
                if (ifi.Flags & flags != flags)
                {
                    continue;
                }
                {
                    var (_, ok) = hasRoutableIP(network, ref ifi);

                    if (!ok)
                    {
                        continue;
                    }

                }
                return ref ifi;
            }
            return null;
        }

        private static (net.IP, bool) hasRoutableIP(@string network, ref net.Interface ifi)
        {
            var (ifat, err) = ifi.Addrs();
            if (err != null)
            {
                return (null, false);
            }
            {
                var ifa__prev1 = ifa;

                foreach (var (_, __ifa) in ifat)
                {
                    ifa = __ifa;
                    switch (ifa.type())
                    {
                        case ref net.IPAddr ifa:
                            {
                                var ip__prev1 = ip;

                                var ip = routableIP(network, ifa.IP);

                                if (ip != null)
                                {
                                    return (ip, true);
                                }

                                ip = ip__prev1;

                            }
                            break;
                        case ref net.IPNet ifa:
                            {
                                var ip__prev1 = ip;

                                ip = routableIP(network, ifa.IP);

                                if (ip != null)
                                {
                                    return (ip, true);
                                }

                                ip = ip__prev1;

                            }
                            break;
                    }
                }

                ifa = ifa__prev1;
            }

            return (null, false);
        }

        private static net.IP routableIP(@string network, net.IP ip)
        {
            if (!ip.IsLoopback() && !ip.IsLinkLocalUnicast() && !ip.IsGlobalUnicast())
            {
                return null;
            }
            switch (network)
            {
                case "ip4": 
                    {
                        var ip__prev1 = ip;

                        var ip = ip.To4();

                        if (ip != null)
                        {
                            return ip;
                        }

                        ip = ip__prev1;

                    }
                    break;
                case "ip6": 
                    if (ip.IsLoopback())
                    { // addressing scope of the loopback address depends on each implementation
                        return null;
                    }
                    {
                        var ip__prev1 = ip;

                        ip = ip.To16();

                        if (ip != null && ip.To4() == null)
                        {
                            return ip;
                        }

                        ip = ip__prev1;

                    }
                    break;
                default: 
                    {
                        var ip__prev1 = ip;

                        ip = ip.To4();

                        if (ip != null)
                        {
                            return ip;
                        }

                        ip = ip__prev1;

                    }
                    {
                        var ip__prev1 = ip;

                        ip = ip.To16();

                        if (ip != null)
                        {
                            return ip;
                        }

                        ip = ip__prev1;

                    }
                    break;
            }
            return null;
        }
    }
}}}}}}
