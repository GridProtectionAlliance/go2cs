// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package nettest provides utilities for network testing.
namespace go.vendor.golang.org.x.net;

using errors = errors_package;
using fmt = fmt_package;
using net = net_package;
using os = os_package;
using exec = os.exec_package;
using runtime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;
using os;

partial class nettest_package {

internal static sync.Once stackOnce;
internal static bool ipv4Enabled;
internal static bool canListenTCP4OnLoopback;
internal static bool ipv6Enabled;
internal static bool canListenTCP6OnLoopback;
internal static bool unStrmDgramEnabled;
internal static bool rawSocketSess;
internal static time.Time aLongTimeAgo = time.Unix(233431200, 0);
internal static time.Time neverTimeout = new time.Time(nil);
internal static error errNoAvailableInterface = errors.New("no available interface"u8);
internal static error errNoAvailableAddress = errors.New("no available address"u8);

internal static void probeStack() {
    {
        (_, err) = RoutedInterface("ip4"u8, net.FlagUp); if (err == default!) {
            ipv4Enabled = true;
        }
    }
    {
        (ln, err) = net.Listen("tcp4"u8, "127.0.0.1:0"u8); if (err == default!) {
            ln.Close();
            canListenTCP4OnLoopback = true;
        }
    }
    {
        (_, err) = RoutedInterface("ip6"u8, net.FlagUp); if (err == default!) {
            ipv6Enabled = true;
        }
    }
    {
        (ln, err) = net.Listen("tcp6"u8, "[::1]:0"u8); if (err == default!) {
            ln.Close();
            canListenTCP6OnLoopback = true;
        }
    }
    rawSocketSess = supportsRawSocket();
    var exprᴛ1 = runtime.GOOS;
    if (exprᴛ1 == "aix"u8) {
        (@out, _) = exec.Command("oslevel"u8, // Unix network isn't properly working on AIX 7.2 with
 // Technical Level < 2.
 "-s"u8).Output();
        if (len(@out) >= len("7200-XX-ZZ-YYMM")) {
            // AIX 7.2, Tech Level XX, Service Pack ZZ, date YYMM
            @string ver = ((@string)(@out[..4]));
            var (tl, _) = strconv.Atoi(((@string)(@out[5..7])));
            unStrmDgramEnabled = ver > "7200"u8 || (ver == "7200"u8 && tl >= 2);
        }
    }
    else { /* default: */
        unStrmDgramEnabled = true;
    }

}

internal static bool unixStrmDgramEnabled() {
    stackOnce.Do(probeStack);
    return unStrmDgramEnabled;
}

// SupportsIPv4 reports whether the platform supports IPv4 networking
// functionality.
public static bool SupportsIPv4() {
    stackOnce.Do(probeStack);
    return ipv4Enabled;
}

// SupportsIPv6 reports whether the platform supports IPv6 networking
// functionality.
public static bool SupportsIPv6() {
    stackOnce.Do(probeStack);
    return ipv6Enabled;
}

// SupportsRawSocket reports whether the current session is available
// to use raw sockets.
public static bool SupportsRawSocket() {
    stackOnce.Do(probeStack);
    return rawSocketSess;
}

// TestableNetwork reports whether network is testable on the current
// platform configuration.
//
// See func Dial of the standard library for the supported networks.
public static bool TestableNetwork(@string network) {
    var ss = strings.Split(network, ":"u8);
    var exprᴛ1 = ss[0];
    if (exprᴛ1 == "ip+nopriv"u8) {
        var exprᴛ2 = runtime.GOOS;
        if (exprᴛ2 == "android"u8 || exprᴛ2 == "fuchsia"u8 || exprᴛ2 == "hurd"u8 || exprᴛ2 == "ios"u8 || exprᴛ2 == "js"u8 || exprᴛ2 == "nacl"u8 || exprᴛ2 == "plan9"u8 || exprᴛ2 == "wasip1"u8 || exprᴛ2 == "windows"u8) {
            return false;
        }

    }
    if (exprᴛ1 == "ip"u8 || exprᴛ1 == "ip4"u8 || exprᴛ1 == "ip6"u8) {
        var exprᴛ3 = runtime.GOOS;
        if (exprᴛ3 == "fuchsia"u8 || exprᴛ3 == "hurd"u8 || exprᴛ3 == "js"u8 || exprᴛ3 == "nacl"u8 || exprᴛ3 == "plan9"u8 || exprᴛ3 == "wasip1"u8) {
            return false;
        }
        { /* default: */
            if (os.Getuid() != 0) {
                // This is an internal network name for testing on the
                // package net of the standard library.
                return false;
            }
        }

    }
    if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixgram"u8) {
        var exprᴛ4 = runtime.GOOS;
        if (exprᴛ4 == "android"u8 || exprᴛ4 == "fuchsia"u8 || exprᴛ4 == "hurd"u8 || exprᴛ4 == "ios"u8 || exprᴛ4 == "js"u8 || exprᴛ4 == "nacl"u8 || exprᴛ4 == "plan9"u8 || exprᴛ4 == "wasip1"u8 || exprᴛ4 == "windows"u8) {
            return false;
        }
        if (exprᴛ4 == "aix"u8) {
            return unixStrmDgramEnabled();
        }

    }
    if (exprᴛ1 == "unixpacket"u8) {
        var exprᴛ5 = runtime.GOOS;
        if (exprᴛ5 == "aix"u8 || exprᴛ5 == "android"u8 || exprᴛ5 == "fuchsia"u8 || exprᴛ5 == "hurd"u8 || exprᴛ5 == "darwin"u8 || exprᴛ5 == "ios"u8 || exprᴛ5 == "js"u8 || exprᴛ5 == "nacl"u8 || exprᴛ5 == "plan9"u8 || exprᴛ5 == "wasip1"u8 || exprᴛ5 == "windows"u8 || exprᴛ5 == "zos"u8) {
            return false;
        }

    }

    var exprᴛ6 = ss[0];
    if (exprᴛ6 == "tcp4"u8 || exprᴛ6 == "udp4"u8 || exprᴛ6 == "ip4"u8) {
        return SupportsIPv4();
    }
    if (exprᴛ6 == "tcp6"u8 || exprᴛ6 == "udp6"u8 || exprᴛ6 == "ip6"u8) {
        return SupportsIPv6();
    }

    return true;
}

// TestableAddress reports whether address of network is testable on
// the current platform configuration.
public static bool TestableAddress(@string network, @string address) {
    {
        var ss = strings.Split(network, ":"u8);
        var exprᴛ1 = ss[0];
        if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixgram"u8 || exprᴛ1 == "unixpacket"u8) {
            if (address[0] == (rune)'@' && runtime.GOOS != "linux"u8) {
                // Abstract unix domain sockets, a Linux-ism.
                return false;
            }
        }
    }

    return true;
}

// NewLocalListener returns a listener which listens to a loopback IP
// address or local file system path.
//
// The provided network must be "tcp", "tcp4", "tcp6", "unix" or
// "unixpacket".
public static (net.Listener, error) NewLocalListener(@string network) {
    stackOnce.Do(probeStack);
    var exprᴛ1 = network;
    if (exprᴛ1 == "tcp"u8) {
        if (canListenTCP4OnLoopback) {
            {
                (ln, err) = net.Listen("tcp4"u8, "127.0.0.1:0"u8); if (err == default!) {
                    return (ln, default!);
                }
            }
        }
        if (canListenTCP6OnLoopback) {
            return net.Listen("tcp6"u8, "[::1]:0"u8);
        }
    }
    if (exprᴛ1 == "tcp4"u8) {
        if (canListenTCP4OnLoopback) {
            return net.Listen("tcp4"u8, "127.0.0.1:0"u8);
        }
    }
    if (exprᴛ1 == "tcp6"u8) {
        if (canListenTCP6OnLoopback) {
            return net.Listen("tcp6"u8, "[::1]:0"u8);
        }
    }
    if (exprᴛ1 == "unix"u8 || exprᴛ1 == "unixpacket"u8) {
        var (path, err) = LocalPath();
        if (err != default!) {
            return (default!, err);
        }
        return net.Listen(network, path);
    }

    return (default!, fmt.Errorf("%s is not supported on %s/%s"u8, network, runtime.GOOS, runtime.GOARCH));
}

// NewLocalPacketListener returns a packet listener which listens to a
// loopback IP address or local file system path.
//
// The provided network must be "udp", "udp4", "udp6" or "unixgram".
public static (net.PacketConn, error) NewLocalPacketListener(@string network) {
    stackOnce.Do(probeStack);
    var exprᴛ1 = network;
    if (exprᴛ1 == "udp"u8) {
        if (canListenTCP4OnLoopback) {
            {
                (c, err) = net.ListenPacket("udp4"u8, "127.0.0.1:0"u8); if (err == default!) {
                    return (c, default!);
                }
            }
        }
        if (canListenTCP6OnLoopback) {
            return net.ListenPacket("udp6"u8, "[::1]:0"u8);
        }
    }
    if (exprᴛ1 == "udp4"u8) {
        if (canListenTCP4OnLoopback) {
            return net.ListenPacket("udp4"u8, "127.0.0.1:0"u8);
        }
    }
    if (exprᴛ1 == "udp6"u8) {
        if (canListenTCP6OnLoopback) {
            return net.ListenPacket("udp6"u8, "[::1]:0"u8);
        }
    }
    if (exprᴛ1 == "unixgram"u8) {
        var (path, err) = LocalPath();
        if (err != default!) {
            return (default!, err);
        }
        return net.ListenPacket(network, path);
    }

    return (default!, fmt.Errorf("%s is not supported on %s/%s"u8, network, runtime.GOOS, runtime.GOARCH));
}

// LocalPath returns a local path that can be used for Unix-domain
// protocol testing.
public static (@string, error) LocalPath() {
    @string dir = ""u8;
    if (runtime.GOOS == "darwin"u8) {
        dir = "/tmp"u8;
    }
    (f, err) = os.CreateTemp(dir, "go-nettest"u8);
    if (err != default!) {
        return ("", err);
    }
    @string path = f.Name();
    f.Close();
    os.Remove(path);
    return (path, default!);
}

// MulticastSource returns a unicast IP address on ifi when ifi is an
// IP multicast-capable network interface.
//
// The provided network must be "ip", "ip4" or "ip6".
public static (net.IP, error) MulticastSource(@string network, ж<net.Interface> Ꮡifi) {
    ref var ifi = ref Ꮡifi.val;

    var exprᴛ1 = network;
    if (exprᴛ1 == "ip"u8 || exprᴛ1 == "ip4"u8 || exprᴛ1 == "ip6"u8) {
    }
    else { /* default: */
        return (default!, errNoAvailableAddress);
    }

    if (ifi == nil || (net.Flags)(ifi.Flags & net.FlagUp) == 0 || (net.Flags)(ifi.Flags & net.FlagMulticast) == 0) {
        return (default!, errNoAvailableAddress);
    }
    var (ip, ok) = hasRoutableIP(network, Ꮡifi);
    if (!ok) {
        return (default!, errNoAvailableAddress);
    }
    return (ip, default!);
}

// LoopbackInterface returns an available logical network interface
// for loopback test.
public static (ж<net.Interface>, error) LoopbackInterface() {
    (ift, err) = net.Interfaces();
    if (err != default!) {
        return (default!, errNoAvailableInterface);
    }
    ref var ifi = ref heap(new net_package.Interface(), out var Ꮡifi);

    foreach (var (_, ifi) in ift) {
        if ((net.Flags)(ifi.Flags & net.FlagLoopback) != 0 && (net.Flags)(ifi.Flags & net.FlagUp) != 0) {
            return (Ꮡifi, default!);
        }
    }
    return (default!, errNoAvailableInterface);
}

// RoutedInterface returns a network interface that can route IP
// traffic and satisfies flags.
//
// The provided network must be "ip", "ip4" or "ip6".
public static (ж<net.Interface>, error) RoutedInterface(@string network, net.Flags flags) {
    var exprᴛ1 = network;
    if (exprᴛ1 == "ip"u8 || exprᴛ1 == "ip4"u8 || exprᴛ1 == "ip6"u8) {
    }
    else { /* default: */
        return (default!, errNoAvailableInterface);
    }

    (ift, err) = net.Interfaces();
    if (err != default!) {
        return (default!, errNoAvailableInterface);
    }
    ref var ifi = ref heap(new net_package.Interface(), out var Ꮡifi);

    foreach (var (_, ifi) in ift) {
        if ((net.Flags)(ifi.Flags & flags) != flags) {
            continue;
        }
        {
            var (_, ok) = hasRoutableIP(network, Ꮡifi); if (!ok) {
                continue;
            }
        }
        return (Ꮡifi, default!);
    }
    return (default!, errNoAvailableInterface);
}

internal static (net.IP, bool) hasRoutableIP(@string network, ж<net.Interface> Ꮡifi) {
    ref var ifi = ref Ꮡifi.val;

    (ifat, err) = ifi.Addrs();
    if (err != default!) {
        return (default!, false);
    }
    foreach (var (_, ifa) in ifat) {
        switch (ifa.type()) {
        case ж<net.IPAddr> ifa: {
            {
                var (ip, ok) = routableIP(network, (~ifa).IP); if (ok) {
                    return (ip, true);
                }
            }
            break;
        }
        case ж<net.IPNet> ifa: {
            {
                var (ip, ok) = routableIP(network, (~ifa).IP); if (ok) {
                    return (ip, true);
                }
            }
            break;
        }}
    }
    return (default!, false);
}

internal static (net.IP, bool) routableIP(@string network, net.IP ip) {
    if (!ip.IsLoopback() && !ip.IsLinkLocalUnicast() && !ip.IsGlobalUnicast()) {
        return (default!, false);
    }
    var exprᴛ1 = network;
    if (exprᴛ1 == "ip4"u8) {
        {
            var ipΔ5 = ip.To4(); if (ipΔ5 != default!) {
                return (ipΔ5, true);
            }
        }
    }
    if (exprᴛ1 == "ip6"u8) {
        if (ip.IsLoopback()) {
            // addressing scope of the loopback address depends on each implementation
            return (default!, false);
        }
        {
            var ipΔ6 = ip.To16(); if (ipΔ6 != default! && ipΔ6.To4() == default!) {
                return (ipΔ6, true);
            }
        }
    }
    { /* default: */
        {
            var ipΔ7 = ip.To4(); if (ipΔ7 != default!) {
                return (ipΔ7, true);
            }
        }
        {
            var ipΔ8 = ip.To16(); if (ipΔ8 != default!) {
                return (ipΔ8, true);
            }
        }
    }

    return (default!, false);
}

} // end nettest_package
