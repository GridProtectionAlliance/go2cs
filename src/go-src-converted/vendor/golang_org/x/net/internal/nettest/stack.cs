// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package nettest provides utilities for network testing.
// package nettest -- go2cs converted at 2020 August 29 10:12:11 UTC
// import "vendor/golang_org/x/net/internal/nettest" ==> using nettest = go.vendor.golang_org.x.net.@internal.nettest_package
// Original source: C:\Go\src\vendor\golang_org\x\net\internal\nettest\stack.go
// import "golang.org/x/net/internal/nettest"

using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using net = go.net_package;
using os = go.os_package;
using runtime = go.runtime_package;
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
        private static bool supportsIPv4 = default;        private static bool supportsIPv6 = default;

        private static void init()
        {
            {
                var ln__prev1 = ln;

                var (ln, err) = net.Listen("tcp4", "127.0.0.1:0");

                if (err == null)
                {
                    ln.Close();
                    supportsIPv4 = true;
                }

                ln = ln__prev1;

            }
            {
                var ln__prev1 = ln;

                (ln, err) = net.Listen("tcp6", "[::1]:0");

                if (err == null)
                {
                    ln.Close();
                    supportsIPv6 = true;
                }

                ln = ln__prev1;

            }
        }

        // SupportsIPv4 reports whether the platform supports IPv4 networking
        // functionality.
        public static bool SupportsIPv4()
        {
            return supportsIPv4;
        }

        // SupportsIPv6 reports whether the platform supports IPv6 networking
        // functionality.
        public static bool SupportsIPv6()
        {
            return supportsIPv6;
        }

        // SupportsRawIPSocket reports whether the platform supports raw IP
        // sockets.
        public static (@string, bool) SupportsRawIPSocket()
        {
            return supportsRawIPSocket();
        }

        // SupportsIPv6MulticastDeliveryOnLoopback reports whether the
        // platform supports IPv6 multicast packet delivery on software
        // loopback interface.
        public static bool SupportsIPv6MulticastDeliveryOnLoopback()
        {
            return supportsIPv6MulticastDeliveryOnLoopback();
        }

        // ProtocolNotSupported reports whether err is a protocol not
        // supported error.
        public static bool ProtocolNotSupported(error err)
        {
            return protocolNotSupported(err);
        }

        // TestableNetwork reports whether network is testable on the current
        // platform configuration.
        public static bool TestableNetwork(@string network)
        { 
            // This is based on logic from standard library's
            // net/platform_test.go.
            switch (network)
            {
                case "unix": 

                case "unixgram": 
                    switch (runtime.GOOS)
                    {
                        case "android": 

                        case "nacl": 

                        case "plan9": 

                        case "windows": 
                            return false;
                            break;
                    }
                    if (runtime.GOOS == "darwin" && (runtime.GOARCH == "arm" || runtime.GOARCH == "arm64"))
                    {
                        return false;
                    }
                    break;
                case "unixpacket": 
                    switch (runtime.GOOS)
                    {
                        case "android": 

                        case "darwin": 

                        case "freebsd": 

                        case "nacl": 

                        case "plan9": 

                        case "windows": 
                            return false;
                            break;
                        case "netbsd": 
                            // It passes on amd64 at least. 386 fails (Issue 22927). arm is unknown.
                            if (runtime.GOARCH == "386")
                            {
                                return false;
                            }
                            break;
                    }
                    break;
            }
            return true;
        }

        // NewLocalListener returns a listener which listens to a loopback IP
        // address or local file system path.
        // Network must be "tcp", "tcp4", "tcp6", "unix" or "unixpacket".
        public static (net.Listener, error) NewLocalListener(@string network)
        {
            switch (network)
            {
                case "tcp": 
                    if (supportsIPv4)
                    {
                        {
                            var (ln, err) = net.Listen("tcp4", "127.0.0.1:0");

                            if (err == null)
                            {
                                return (ln, null);
                            }

                        }
                    }
                    if (supportsIPv6)
                    {
                        return net.Listen("tcp6", "[::1]:0");
                    }
                    break;
                case "tcp4": 
                    if (supportsIPv4)
                    {
                        return net.Listen("tcp4", "127.0.0.1:0");
                    }
                    break;
                case "tcp6": 
                    if (supportsIPv6)
                    {
                        return net.Listen("tcp6", "[::1]:0");
                    }
                    break;
                case "unix": 

                case "unixpacket": 
                    return net.Listen(network, localPath());
                    break;
            }
            return (null, fmt.Errorf("%s is not supported", network));
        }

        // NewLocalPacketListener returns a packet listener which listens to a
        // loopback IP address or local file system path.
        // Network must be "udp", "udp4", "udp6" or "unixgram".
        public static (net.PacketConn, error) NewLocalPacketListener(@string network)
        {
            switch (network)
            {
                case "udp": 
                    if (supportsIPv4)
                    {
                        {
                            var (c, err) = net.ListenPacket("udp4", "127.0.0.1:0");

                            if (err == null)
                            {
                                return (c, null);
                            }

                        }
                    }
                    if (supportsIPv6)
                    {
                        return net.ListenPacket("udp6", "[::1]:0");
                    }
                    break;
                case "udp4": 
                    if (supportsIPv4)
                    {
                        return net.ListenPacket("udp4", "127.0.0.1:0");
                    }
                    break;
                case "udp6": 
                    if (supportsIPv6)
                    {
                        return net.ListenPacket("udp6", "[::1]:0");
                    }
                    break;
                case "unixgram": 
                    return net.ListenPacket(network, localPath());
                    break;
            }
            return (null, fmt.Errorf("%s is not supported", network));
        }

        private static @string localPath() => func((_, panic, __) =>
        {
            var (f, err) = ioutil.TempFile("", "nettest");
            if (err != null)
            {
                panic(err);
            }
            var path = f.Name();
            f.Close();
            os.Remove(path);
            return path;
        });
    }
}}}}}}
