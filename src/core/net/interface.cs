// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using itoa = @internal.itoa_package;
using sync = sync_package;
using time = time_package;
using _ = unsafe_package;
using @internal;

partial class net_package {

// BUG(mikio): On JS, methods and functions related to
// Interface are not implemented.
// BUG(mikio): On AIX, DragonFly BSD, NetBSD, OpenBSD, Plan 9 and
// Solaris, the MulticastAddrs method of Interface is not implemented.
// errNoSuchInterface should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/sagernet/sing
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname errNoSuchInterface
internal static error errInvalidInterface = errors.New("invalid network interface"u8);
internal static error errInvalidInterfaceIndex = errors.New("invalid network interface index"u8);
internal static error errInvalidInterfaceName = errors.New("invalid network interface name"u8);
internal static error errNoSuchInterface = errors.New("no such network interface"u8);
internal static error errNoSuchMulticastInterface = errors.New("no such multicast network interface"u8);

// Interface represents a mapping between network interface name
// and index. It also represents network interface facility
// information.
[GoType] partial struct Interface {
    public nint Index;         // positive integer that starts at one, zero is never used
    public nint MTU;         // maximum transmission unit
    public @string Name;      // e.g., "en0", "lo0", "eth0.100"
    public HardwareAddr HardwareAddr; // IEEE MAC-48, EUI-48 and EUI-64 form
    public Flags Flags;        // e.g., FlagUp, FlagLoopback, FlagMulticast
}

[GoType("num:nuint")] partial struct Flags;

public static readonly Flags FlagUp = /* 1 << iota */ 1;                 // interface is administratively up
public static readonly Flags FlagBroadcast = 2;          // interface supports broadcast access capability
public static readonly Flags FlagLoopback = 4;           // interface is a loopback interface
public static readonly Flags FlagPointToPoint = 8;       // interface belongs to a point-to-point link
public static readonly Flags FlagMulticast = 16;          // interface supports multicast access capability
public static readonly Flags FlagRunning = 32;            // interface is in running state

internal static slice<@string> flagNames = new @string[]{
    "up",
    "broadcast",
    "loopback",
    "pointtopoint",
    "multicast",
    "running"
}.slice();

public static @string String(this Flags f) {
    @string s = ""u8;
    foreach (var (i, name) in flagNames) {
        if ((Flags)(f & (1 << (int)(((nuint)i)))) != 0) {
            if (s != ""u8) {
                s += "|"u8;
            }
            s += name;
        }
    }
    if (s == ""u8) {
        s = "0"u8;
    }
    return s;
}

// Addrs returns a list of unicast interface addresses for a specific
// interface.
[GoRecv] public static (slice<ΔAddr>, error) Addrs(this ref Interface ifi) {
    if (ifi == nil) {
        return (default!, new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, ΔAddr: default!, Err: errInvalidInterface));
    }
    (ifat, err) = interfaceAddrTable(ifi);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, ΔAddr: default!, Err: err); err = ref Ꮡerr.val;
    }
    return (ifat, err);
}

// MulticastAddrs returns a list of multicast, joined group addresses
// for a specific interface.
[GoRecv] public static (slice<ΔAddr>, error) MulticastAddrs(this ref Interface ifi) {
    if (ifi == nil) {
        return (default!, new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, ΔAddr: default!, Err: errInvalidInterface));
    }
    (ifat, err) = interfaceMulticastAddrTable(ifi);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, ΔAddr: default!, Err: err); err = ref Ꮡerr.val;
    }
    return (ifat, err);
}

// Interfaces returns a list of the system's network interfaces.
public static (slice<Interface>, error) Interfaces() {
    (ift, err) = interfaceTable(0);
    if (err != default!) {
        return (default!, new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, ΔAddr: default!, Err: err));
    }
    if (len(ift) != 0) {
        zoneCache.update(ift, false);
    }
    return (ift, default!);
}

// InterfaceAddrs returns a list of the system's unicast interface
// addresses.
//
// The returned list does not identify the associated interface; use
// Interfaces and [Interface.Addrs] for more detail.
public static (slice<ΔAddr>, error) InterfaceAddrs() {
    (ifat, err) = interfaceAddrTable(nil);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, ΔAddr: default!, Err: err); err = ref Ꮡerr.val;
    }
    return (ifat, err);
}

// InterfaceByIndex returns the interface specified by index.
//
// On Solaris, it returns one of the logical network interfaces
// sharing the logical data link; for more precision use
// [InterfaceByName].
public static (ж<Interface>, error) InterfaceByIndex(nint index) {
    if (index <= 0) {
        return (default!, new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, ΔAddr: default!, Err: errInvalidInterfaceIndex));
    }
    (ift, err) = interfaceTable(index);
    if (err != default!) {
        return (default!, new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, ΔAddr: default!, Err: err));
    }
    (ifi, err) = interfaceByIndex(ift, index);
    if (err != default!) {
        Ꮡerr = new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, ΔAddr: default!, Err: err); err = ref Ꮡerr.val;
    }
    return (ifi, err);
}

internal static (ж<Interface>, error) interfaceByIndex(slice<Interface> ift, nint index) {
    ref var ifi = ref heap(new Interface(), out var Ꮡifi);

    foreach (var (_, ifi) in ift) {
        if (index == ifi.Index) {
            return (Ꮡifi, default!);
        }
    }
    return (default!, errNoSuchInterface);
}

// InterfaceByName returns the interface specified by name.
public static (ж<Interface>, error) InterfaceByName(@string name) {
    if (name == ""u8) {
        return (default!, new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, ΔAddr: default!, Err: errInvalidInterfaceName));
    }
    (ift, err) = interfaceTable(0);
    if (err != default!) {
        return (default!, new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, ΔAddr: default!, Err: err));
    }
    if (len(ift) != 0) {
        zoneCache.update(ift, false);
    }
    ref var ifi = ref heap(new Interface(), out var Ꮡifi);

    foreach (var (_, ifi) in ift) {
        if (name == ifi.Name) {
            return (Ꮡifi, default!);
        }
    }
    return (default!, new OpError(Op: "route"u8, Net: "ip+net"u8, Source: default!, ΔAddr: default!, Err: errNoSuchInterface));
}

// An ipv6ZoneCache represents a cache holding partial network
// interface information. It is used for reducing the cost of IPv6
// addressing scope zone resolution.
//
// Multiple names sharing the index are managed by first-come
// first-served basis for consistency.
[GoType] partial struct ipv6ZoneCache {
    public partial ref sync_package.RWMutex RWMutex { get; }                // guard the following
    internal time_package.Time lastFetched;      // last time routing information was fetched
    internal map<@string, nint> toIndex; // interface name to its index
    internal map<nint, @string> toName; // interface index to its name
}

internal static ipv6ZoneCache zoneCache = new ipv6ZoneCache(
    toIndex: new map<@string, nint>(),
    toName: new map<nint, @string>()
);

// update refreshes the network interface information if the cache was last
// updated more than 1 minute ago, or if force is set. It reports whether the
// cache was updated.
[GoRecv] internal static bool /*updated*/ update(this ref ipv6ZoneCache zc, slice<Interface> ift, bool force) => func((defer, _) => {
    bool updated = default!;

    zc.Lock();
    defer(zc.Unlock);
    var now = time.Now();
    if (!force && zc.lastFetched.After(now.Add(-60 * time.ΔSecond))) {
        return false;
    }
    zc.lastFetched = now;
    if (len(ift) == 0) {
        error err = default!;
        {
            (ift, err) = interfaceTable(0); if (err != default!) {
                return false;
            }
        }
    }
    zc.toIndex = new map<@string, nint>(len(ift));
    zc.toName = new map<nint, @string>(len(ift));
    foreach (var (_, ifi) in ift) {
        zc.toIndex[ifi.Name] = ifi.Index;
        {
            @string _ = zc.toName[ifi.Index];
            var ok = zc.toName[ifi.Index]; if (!ok) {
                zc.toName[ifi.Index] = ifi.Name;
            }
        }
    }
    return true;
});

[GoRecv] internal static @string name(this ref ipv6ZoneCache zc, nint index) {
    if (index == 0) {
        return ""u8;
    }
    var updated = zoneCache.update(default!, false);
    zoneCache.RLock();
    @string name = zoneCache.toName[index];
    var ok = zoneCache.toName[index];
    zoneCache.RUnlock();
    if (!ok && !updated) {
        zoneCache.update(default!, true);
        zoneCache.RLock();
        (name, ok) = zoneCache.toName[index];
        zoneCache.RUnlock();
    }
    if (!ok) {
        // last resort
        name = itoa.Uitoa(((nuint)index));
    }
    return name;
}

[GoRecv] internal static nint index(this ref ipv6ZoneCache zc, @string name) {
    if (name == ""u8) {
        return 0;
    }
    var updated = zoneCache.update(default!, false);
    zoneCache.RLock();
    nint index = zoneCache.toIndex[name];
    var ok = zoneCache.toIndex[name];
    zoneCache.RUnlock();
    if (!ok && !updated) {
        zoneCache.update(default!, true);
        zoneCache.RLock();
        (index, ok) = zoneCache.toIndex[name];
        zoneCache.RUnlock();
    }
    if (!ok) {
        // last resort
        (index, _, _) = dtoi(name);
    }
    return index;
}

} // end net_package
