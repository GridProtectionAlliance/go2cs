// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 08 03:33:10 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\interface.go
using errors = go.errors_package;
using sync = go.sync_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // BUG(mikio): On JS, methods and functions related to
        // Interface are not implemented.

        // BUG(mikio): On AIX, DragonFly BSD, NetBSD, OpenBSD, Plan 9 and
        // Solaris, the MulticastAddrs method of Interface is not implemented.
        private static var errInvalidInterface = errors.New("invalid network interface");        private static var errInvalidInterfaceIndex = errors.New("invalid network interface index");        private static var errInvalidInterfaceName = errors.New("invalid network interface name");        private static var errNoSuchInterface = errors.New("no such network interface");        private static var errNoSuchMulticastInterface = errors.New("no such multicast network interface");

        // Interface represents a mapping between network interface name
        // and index. It also represents network interface facility
        // information.
        public partial struct Interface
        {
            public long Index; // positive integer that starts at one, zero is never used
            public long MTU; // maximum transmission unit
            public @string Name; // e.g., "en0", "lo0", "eth0.100"
            public HardwareAddr HardwareAddr; // IEEE MAC-48, EUI-48 and EUI-64 form
            public Flags Flags; // e.g., FlagUp, FlagLoopback, FlagMulticast
        }

        public partial struct Flags // : ulong
        {
        }

        public static readonly Flags FlagUp = (Flags)1L << (int)(iota); // interface is up
        public static readonly var FlagBroadcast = (var)0; // interface supports broadcast access capability
        public static readonly var FlagLoopback = (var)1; // interface is a loopback interface
        public static readonly var FlagPointToPoint = (var)2; // interface belongs to a point-to-point link
        public static readonly var FlagMulticast = (var)3; // interface supports multicast access capability

        private static @string flagNames = new slice<@string>(new @string[] { "up", "broadcast", "loopback", "pointtopoint", "multicast" });

        public static @string String(this Flags f)
        {
            @string s = "";
            foreach (var (i, name) in flagNames)
            {
                if (f & (1L << (int)(uint(i))) != 0L)
                {
                    if (s != "")
                    {
                        s += "|";
                    }

                    s += name;

                }

            }
            if (s == "")
            {
                s = "0";
            }

            return s;

        }

        // Addrs returns a list of unicast interface addresses for a specific
        // interface.
        private static (slice<Addr>, error) Addrs(this ptr<Interface> _addr_ifi)
        {
            slice<Addr> _p0 = default;
            error _p0 = default!;
            ref Interface ifi = ref _addr_ifi.val;

            if (ifi == null)
            {
                return (null, error.As(addr(new OpError(Op:"route",Net:"ip+net",Source:nil,Addr:nil,Err:errInvalidInterface))!)!);
            }

            var (ifat, err) = interfaceAddrTable(ifi);
            if (err != null)
            {
                err = addr(new OpError(Op:"route",Net:"ip+net",Source:nil,Addr:nil,Err:err));
            }

            return (ifat, error.As(err)!);

        }

        // MulticastAddrs returns a list of multicast, joined group addresses
        // for a specific interface.
        private static (slice<Addr>, error) MulticastAddrs(this ptr<Interface> _addr_ifi)
        {
            slice<Addr> _p0 = default;
            error _p0 = default!;
            ref Interface ifi = ref _addr_ifi.val;

            if (ifi == null)
            {
                return (null, error.As(addr(new OpError(Op:"route",Net:"ip+net",Source:nil,Addr:nil,Err:errInvalidInterface))!)!);
            }

            var (ifat, err) = interfaceMulticastAddrTable(ifi);
            if (err != null)
            {
                err = addr(new OpError(Op:"route",Net:"ip+net",Source:nil,Addr:nil,Err:err));
            }

            return (ifat, error.As(err)!);

        }

        // Interfaces returns a list of the system's network interfaces.
        public static (slice<Interface>, error) Interfaces()
        {
            slice<Interface> _p0 = default;
            error _p0 = default!;

            var (ift, err) = interfaceTable(0L);
            if (err != null)
            {
                return (null, error.As(addr(new OpError(Op:"route",Net:"ip+net",Source:nil,Addr:nil,Err:err))!)!);
            }

            if (len(ift) != 0L)
            {
                zoneCache.update(ift, false);
            }

            return (ift, error.As(null!)!);

        }

        // InterfaceAddrs returns a list of the system's unicast interface
        // addresses.
        //
        // The returned list does not identify the associated interface; use
        // Interfaces and Interface.Addrs for more detail.
        public static (slice<Addr>, error) InterfaceAddrs()
        {
            slice<Addr> _p0 = default;
            error _p0 = default!;

            var (ifat, err) = interfaceAddrTable(null);
            if (err != null)
            {
                err = addr(new OpError(Op:"route",Net:"ip+net",Source:nil,Addr:nil,Err:err));
            }

            return (ifat, error.As(err)!);

        }

        // InterfaceByIndex returns the interface specified by index.
        //
        // On Solaris, it returns one of the logical network interfaces
        // sharing the logical data link; for more precision use
        // InterfaceByName.
        public static (ptr<Interface>, error) InterfaceByIndex(long index)
        {
            ptr<Interface> _p0 = default!;
            error _p0 = default!;

            if (index <= 0L)
            {
                return (_addr_null!, error.As(addr(new OpError(Op:"route",Net:"ip+net",Source:nil,Addr:nil,Err:errInvalidInterfaceIndex))!)!);
            }

            var (ift, err) = interfaceTable(index);
            if (err != null)
            {
                return (_addr_null!, error.As(addr(new OpError(Op:"route",Net:"ip+net",Source:nil,Addr:nil,Err:err))!)!);
            }

            var (ifi, err) = interfaceByIndex(ift, index);
            if (err != null)
            {
                err = addr(new OpError(Op:"route",Net:"ip+net",Source:nil,Addr:nil,Err:err));
            }

            return (_addr_ifi!, error.As(err)!);

        }

        private static (ptr<Interface>, error) interfaceByIndex(slice<Interface> ift, long index)
        {
            ptr<Interface> _p0 = default!;
            error _p0 = default!;

            foreach (var (_, ifi) in ift)
            {
                if (index == ifi.Index)
                {
                    return (_addr__addr_ifi!, error.As(null!)!);
                }

            }
            return (_addr_null!, error.As(errNoSuchInterface)!);

        }

        // InterfaceByName returns the interface specified by name.
        public static (ptr<Interface>, error) InterfaceByName(@string name)
        {
            ptr<Interface> _p0 = default!;
            error _p0 = default!;

            if (name == "")
            {
                return (_addr_null!, error.As(addr(new OpError(Op:"route",Net:"ip+net",Source:nil,Addr:nil,Err:errInvalidInterfaceName))!)!);
            }

            var (ift, err) = interfaceTable(0L);
            if (err != null)
            {
                return (_addr_null!, error.As(addr(new OpError(Op:"route",Net:"ip+net",Source:nil,Addr:nil,Err:err))!)!);
            }

            if (len(ift) != 0L)
            {
                zoneCache.update(ift, false);
            }

            foreach (var (_, ifi) in ift)
            {
                if (name == ifi.Name)
                {
                    return (_addr__addr_ifi!, error.As(null!)!);
                }

            }
            return (_addr_null!, error.As(addr(new OpError(Op:"route",Net:"ip+net",Source:nil,Addr:nil,Err:errNoSuchInterface))!)!);

        }

        // An ipv6ZoneCache represents a cache holding partial network
        // interface information. It is used for reducing the cost of IPv6
        // addressing scope zone resolution.
        //
        // Multiple names sharing the index are managed by first-come
        // first-served basis for consistency.
        private partial struct ipv6ZoneCache
        {
            public ref sync.RWMutex RWMutex => ref RWMutex_val; // guard the following
            public time.Time lastFetched; // last time routing information was fetched
            public map<@string, long> toIndex; // interface name to its index
            public map<long, @string> toName; // interface index to its name
        }

        private static ipv6ZoneCache zoneCache = new ipv6ZoneCache(toIndex:make(map[string]int),toName:make(map[int]string),);

        // update refreshes the network interface information if the cache was last
        // updated more than 1 minute ago, or if force is set. It reports whether the
        // cache was updated.
        private static bool update(this ptr<ipv6ZoneCache> _addr_zc, slice<Interface> ift, bool force) => func((defer, _, __) =>
        {
            bool updated = default;
            ref ipv6ZoneCache zc = ref _addr_zc.val;

            zc.Lock();
            defer(zc.Unlock());
            var now = time.Now();
            if (!force && zc.lastFetched.After(now.Add(-60L * time.Second)))
            {
                return false;
            }

            zc.lastFetched = now;
            if (len(ift) == 0L)
            {
                error err = default!;
                ift, err = interfaceTable(0L);

                if (err != null)
                {
                    return false;
                }

            }

            zc.toIndex = make_map<@string, long>(len(ift));
            zc.toName = make_map<long, @string>(len(ift));
            foreach (var (_, ifi) in ift)
            {
                zc.toIndex[ifi.Name] = ifi.Index;
                {
                    var (_, ok) = zc.toName[ifi.Index];

                    if (!ok)
                    {
                        zc.toName[ifi.Index] = ifi.Name;
                    }

                }

            }
            return true;

        });

        private static @string name(this ptr<ipv6ZoneCache> _addr_zc, long index)
        {
            ref ipv6ZoneCache zc = ref _addr_zc.val;

            if (index == 0L)
            {
                return "";
            }

            var updated = zoneCache.update(null, false);
            zoneCache.RLock();
            var (name, ok) = zoneCache.toName[index];
            zoneCache.RUnlock();
            if (!ok && !updated)
            {
                zoneCache.update(null, true);
                zoneCache.RLock();
                name, ok = zoneCache.toName[index];
                zoneCache.RUnlock();
            }

            if (!ok)
            { // last resort
                name = uitoa(uint(index));

            }

            return name;

        }

        private static long index(this ptr<ipv6ZoneCache> _addr_zc, @string name)
        {
            ref ipv6ZoneCache zc = ref _addr_zc.val;

            if (name == "")
            {
                return 0L;
            }

            var updated = zoneCache.update(null, false);
            zoneCache.RLock();
            var (index, ok) = zoneCache.toIndex[name];
            zoneCache.RUnlock();
            if (!ok && !updated)
            {
                zoneCache.update(null, true);
                zoneCache.RLock();
                index, ok = zoneCache.toIndex[name];
                zoneCache.RUnlock();
            }

            if (!ok)
            { // last resort
                index, _, _ = dtoi(name);

            }

            return index;

        }
    }
}
