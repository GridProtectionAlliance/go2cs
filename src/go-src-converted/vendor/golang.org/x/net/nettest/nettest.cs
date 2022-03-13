// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package nettest provides utilities for network testing.

// package nettest -- go2cs converted at 2022 March 13 06:46:27 UTC
// import "vendor/golang.org/x/net/nettest" ==> using nettest = go.vendor.golang.org.x.net.nettest_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\net\nettest\nettest.go
namespace go.vendor.golang.org.x.net;

using errors = errors_package;
using fmt = fmt_package;
using ioutil = io.ioutil_package;
using net = net_package;
using os = os_package;
using exec = os.exec_package;
using runtime = runtime_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;

public static partial class nettest_package {

private static sync.Once stackOnce = default;private static bool ipv4Enabled = default;private static bool ipv6Enabled = default;private static bool unStrmDgramEnabled = default;private static bool rawSocketSess = default;private static var aLongTimeAgo = time.Unix(233431200, 0);private static time.Time neverTimeout = new time.Time();private static var errNoAvailableInterface = errors.New("no available interface");private static var errNoAvailableAddress = errors.New("no available address");

private static void probeStack() {
    {
        var ln__prev1 = ln;

        var (ln, err) = net.Listen("tcp4", "127.0.0.1:0");

        if (err == null) {
            ln.Close();
            ipv4Enabled = true;
        }
        ln = ln__prev1;

    }
    {
        var ln__prev1 = ln;

        (ln, err) = net.Listen("tcp6", "[::1]:0");

        if (err == null) {
            ln.Close();
            ipv6Enabled = true;
        }
        ln = ln__prev1;

    }
    rawSocketSess = supportsRawSocket();
    switch (runtime.GOOS) {
        case "aix": 
            // Unix network isn't properly working on AIX 7.2 with
            // Technical Level < 2.
            var (out, _) = exec.Command("oslevel", "-s").Output();
            if (len(out) >= len("7200-XX-ZZ-YYMM")) { // AIX 7.2, Tech Level XX, Service Pack ZZ, date YYMM
                var ver = string(out[..(int)4]);
                var (tl, _) = strconv.Atoi(string(out[(int)5..(int)7]));
                unStrmDgramEnabled = ver > "7200" || (ver == "7200" && tl >= 2);
            }
            break;
        default: 
            unStrmDgramEnabled = true;
            break;
    }
}

private static bool unixStrmDgramEnabled() {
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
    var ss = strings.Split(network, ":");
    switch (ss[0]) {
        case "ip+nopriv": 
            // This is an internal network name for testing on the
            // package net of the standard library.
            switch (runtime.GOOS) {
                case "android": 

                case "fuchsia": 

                case "hurd": 

                case "js": 

                case "nacl": 

                case "plan9": 

                case "windows": 
                    return false;
                    break;
                case "darwin": 
                    // iOS doesn't support it.

                case "ios": 
                    // iOS doesn't support it.
                    if (runtime.GOARCH == "arm" || runtime.GOARCH == "arm64") {
                        return false;
                    }
                    break;
            }
            break;
        case "ip": 

        case "ip4": 

        case "ip6": 
            switch (runtime.GOOS) {
                case "fuchsia": 

                case "hurd": 

                case "js": 

                case "nacl": 

                case "plan9": 
                    return false;
                    break;
                default: 
                    if (os.Getuid() != 0) {
                        return false;
                    }
                    break;
            }
            break;
        case "unix": 

        case "unixgram": 
            switch (runtime.GOOS) {
                case "android": 

                case "fuchsia": 

                case "hurd": 

                case "js": 

                case "nacl": 

                case "plan9": 

                case "windows": 
                    return false;
                    break;
                case "aix": 
                    return unixStrmDgramEnabled();
                    break;
                case "darwin": 
                    // iOS does not support unix, unixgram.

                case "ios": 
                    // iOS does not support unix, unixgram.
                    if (runtime.GOARCH == "arm" || runtime.GOARCH == "arm64") {
                        return false;
                    }
                    break;
            }
            break;
        case "unixpacket": 
            switch (runtime.GOOS) {
                case "aix": 

                case "android": 

                case "fuchsia": 

                case "hurd": 

                case "darwin": 

                case "ios": 

                case "js": 

                case "nacl": 

                case "plan9": 

                case "windows": 

                case "zos": 
                    return false;
                    break;
                case "netbsd": 
                    // It passes on amd64 at least. 386 fails
                    // (Issue 22927). arm is unknown.
                    if (runtime.GOARCH == "386") {
                        return false;
                    }
                    break;
            }
            break;
    }
    switch (ss[0]) {
        case "tcp4": 

        case "udp4": 

        case "ip4": 
            return SupportsIPv4();
            break;
        case "tcp6": 

        case "udp6": 

        case "ip6": 
            return SupportsIPv6();
            break;
    }
    return true;
}

// TestableAddress reports whether address of network is testable on
// the current platform configuration.
public static bool TestableAddress(@string network, @string address) {
    {
        var ss = strings.Split(network, ":");

        switch (ss[0]) {
            case "unix": 
                // Abstract unix domain sockets, a Linux-ism.

            case "unixgram": 
                // Abstract unix domain sockets, a Linux-ism.

            case "unixpacket": 
                // Abstract unix domain sockets, a Linux-ism.
                if (address[0] == '@' && runtime.GOOS != "linux") {
                    return false;
                }
                break;
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
    net.Listener _p0 = default;
    error _p0 = default!;

    switch (network) {
        case "tcp": 
            if (SupportsIPv4()) {
                {
                    var (ln, err) = net.Listen("tcp4", "127.0.0.1:0");

                    if (err == null) {
                        return (ln, error.As(null!)!);
                    }

                }
            }
            if (SupportsIPv6()) {
                return net.Listen("tcp6", "[::1]:0");
            }
            break;
        case "tcp4": 
            if (SupportsIPv4()) {
                return net.Listen("tcp4", "127.0.0.1:0");
            }
            break;
        case "tcp6": 
            if (SupportsIPv6()) {
                return net.Listen("tcp6", "[::1]:0");
            }
            break;
        case "unix": 

        case "unixpacket": 
            var (path, err) = LocalPath();
            if (err != null) {
                return (null, error.As(err)!);
            }
            return net.Listen(network, path);
            break;
    }
    return (null, error.As(fmt.Errorf("%s is not supported on %s/%s", network, runtime.GOOS, runtime.GOARCH))!);
}

// NewLocalPacketListener returns a packet listener which listens to a
// loopback IP address or local file system path.
//
// The provided network must be "udp", "udp4", "udp6" or "unixgram".
public static (net.PacketConn, error) NewLocalPacketListener(@string network) {
    net.PacketConn _p0 = default;
    error _p0 = default!;

    switch (network) {
        case "udp": 
            if (SupportsIPv4()) {
                {
                    var (c, err) = net.ListenPacket("udp4", "127.0.0.1:0");

                    if (err == null) {
                        return (c, error.As(null!)!);
                    }

                }
            }
            if (SupportsIPv6()) {
                return net.ListenPacket("udp6", "[::1]:0");
            }
            break;
        case "udp4": 
            if (SupportsIPv4()) {
                return net.ListenPacket("udp4", "127.0.0.1:0");
            }
            break;
        case "udp6": 
            if (SupportsIPv6()) {
                return net.ListenPacket("udp6", "[::1]:0");
            }
            break;
        case "unixgram": 
            var (path, err) = LocalPath();
            if (err != null) {
                return (null, error.As(err)!);
            }
            return net.ListenPacket(network, path);
            break;
    }
    return (null, error.As(fmt.Errorf("%s is not supported on %s/%s", network, runtime.GOOS, runtime.GOARCH))!);
}

// LocalPath returns a local path that can be used for Unix-domain
// protocol testing.
public static (@string, error) LocalPath() {
    @string _p0 = default;
    error _p0 = default!;

    var (f, err) = ioutil.TempFile("", "go-nettest");
    if (err != null) {
        return ("", error.As(err)!);
    }
    var path = f.Name();
    f.Close();
    os.Remove(path);
    return (path, error.As(null!)!);
}

// MulticastSource returns a unicast IP address on ifi when ifi is an
// IP multicast-capable network interface.
//
// The provided network must be "ip", "ip4" or "ip6".
public static (net.IP, error) MulticastSource(@string network, ptr<net.Interface> _addr_ifi) {
    net.IP _p0 = default;
    error _p0 = default!;
    ref net.Interface ifi = ref _addr_ifi.val;

    switch (network) {
        case "ip": 

        case "ip4": 

        case "ip6": 

            break;
        default: 
            return (null, error.As(errNoAvailableAddress)!);
            break;
    }
    if (ifi == null || ifi.Flags & net.FlagUp == 0 || ifi.Flags & net.FlagMulticast == 0) {
        return (null, error.As(errNoAvailableAddress)!);
    }
    var (ip, ok) = hasRoutableIP(network, _addr_ifi);
    if (!ok) {
        return (null, error.As(errNoAvailableAddress)!);
    }
    return (ip, error.As(null!)!);
}

// LoopbackInterface returns an available logical network interface
// for loopback test.
public static (ptr<net.Interface>, error) LoopbackInterface() {
    ptr<net.Interface> _p0 = default!;
    error _p0 = default!;

    var (ift, err) = net.Interfaces();
    if (err != null) {
        return (_addr_null!, error.As(errNoAvailableInterface)!);
    }
    foreach (var (_, ifi) in ift) {
        if (ifi.Flags & net.FlagLoopback != 0 && ifi.Flags & net.FlagUp != 0) {
            return (_addr__addr_ifi!, error.As(null!)!);
        }
    }    return (_addr_null!, error.As(errNoAvailableInterface)!);
}

// RoutedInterface returns a network interface that can route IP
// traffic and satisfies flags.
//
// The provided network must be "ip", "ip4" or "ip6".
public static (ptr<net.Interface>, error) RoutedInterface(@string network, net.Flags flags) {
    ptr<net.Interface> _p0 = default!;
    error _p0 = default!;

    switch (network) {
        case "ip": 

        case "ip4": 

        case "ip6": 

            break;
        default: 
            return (_addr_null!, error.As(errNoAvailableInterface)!);
            break;
    }
    var (ift, err) = net.Interfaces();
    if (err != null) {
        return (_addr_null!, error.As(errNoAvailableInterface)!);
    }
    foreach (var (_, ifi) in ift) {
        if (ifi.Flags & flags != flags) {
            continue;
        }
        {
            var (_, ok) = hasRoutableIP(network, _addr_ifi);

            if (!ok) {
                continue;
            }

        }
        return (_addr__addr_ifi!, error.As(null!)!);
    }    return (_addr_null!, error.As(errNoAvailableInterface)!);
}

private static (net.IP, bool) hasRoutableIP(@string network, ptr<net.Interface> _addr_ifi) {
    net.IP _p0 = default;
    bool _p0 = default;
    ref net.Interface ifi = ref _addr_ifi.val;

    var (ifat, err) = ifi.Addrs();
    if (err != null) {
        return (null, false);
    }
    {
        var ifa__prev1 = ifa;

        foreach (var (_, __ifa) in ifat) {
            ifa = __ifa;
            switch (ifa.type()) {
                case ptr<net.IPAddr> ifa:
                    {
                        var ip__prev1 = ip;

                        var (ip, ok) = routableIP(network, ifa.IP);

                        if (ok) {
                            return (ip, true);
                        }

                        ip = ip__prev1;

                    }
                    break;
                case ptr<net.IPNet> ifa:
                    {
                        var ip__prev1 = ip;

                        (ip, ok) = routableIP(network, ifa.IP);

                        if (ok) {
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

private static (net.IP, bool) routableIP(@string network, net.IP ip) {
    net.IP _p0 = default;
    bool _p0 = default;

    if (!ip.IsLoopback() && !ip.IsLinkLocalUnicast() && !ip.IsGlobalUnicast()) {
        return (null, false);
    }
    switch (network) {
        case "ip4": 
            {
                var ip__prev1 = ip;

                var ip = ip.To4();

                if (ip != null) {
                    return (ip, true);
                }

                ip = ip__prev1;

            }
            break;
        case "ip6": 
            if (ip.IsLoopback()) { // addressing scope of the loopback address depends on each implementation
                return (null, false);
            }
            {
                var ip__prev1 = ip;

                ip = ip.To16();

                if (ip != null && ip.To4() == null) {
                    return (ip, true);
                }

                ip = ip__prev1;

            }
            break;
        default: 
            {
                var ip__prev1 = ip;

                ip = ip.To4();

                if (ip != null) {
                    return (ip, true);
                }

                ip = ip__prev1;

            }
            {
                var ip__prev1 = ip;

                ip = ip.To16();

                if (ip != null) {
                    return (ip, true);
                }

                ip = ip__prev1;

            }
            break;
    }
    return (null, false);
}

} // end nettest_package
