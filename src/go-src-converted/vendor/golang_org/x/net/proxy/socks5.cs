// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package proxy -- go2cs converted at 2020 August 29 10:12:27 UTC
// import "vendor/golang_org/x/net/proxy" ==> using proxy = go.vendor.golang_org.x.net.proxy_package
// Original source: C:\Go\src\vendor\golang_org\x\net\proxy\socks5.go
using errors = go.errors_package;
using io = go.io_package;
using net = go.net_package;
using strconv = go.strconv_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class proxy_package
    {
        // SOCKS5 returns a Dialer that makes SOCKSv5 connections to the given address
        // with an optional username and password. See RFC 1928.
        public static (Dialer, error) SOCKS5(@string network, @string addr, ref Auth auth, Dialer forward)
        {
            socks5 s = ref new socks5(network:network,addr:addr,forward:forward,);
            if (auth != null)
            {
                s.user = auth.User;
                s.password = auth.Password;
            }
            return (s, null);
        }

        private partial struct socks5
        {
            public @string user;
            public @string password;
            public @string network;
            public @string addr;
            public Dialer forward;
        }

        private static readonly long socks5Version = 5L;



        private static readonly long socks5AuthNone = 0L;
        private static readonly long socks5AuthPassword = 2L;

        private static readonly long socks5Connect = 1L;



        private static readonly long socks5IP4 = 1L;
        private static readonly long socks5Domain = 3L;
        private static readonly long socks5IP6 = 4L;

        private static @string socks5Errors = new slice<@string>(new @string[] { "", "general failure", "connection forbidden", "network unreachable", "host unreachable", "connection refused", "TTL expired", "command not supported", "address type not supported" });

        // Dial connects to the address addr on the network net via the SOCKS5 proxy.
        private static (net.Conn, error) Dial(this ref socks5 s, @string network, @string addr)
        {
            switch (network)
            {
                case "tcp": 

                case "tcp6": 

                case "tcp4": 
                    break;
                default: 
                    return (null, errors.New("proxy: no support for SOCKS5 proxy connections of type " + network));
                    break;
            }

            var (conn, err) = s.forward.Dial(s.network, s.addr);
            if (err != null)
            {
                return (null, err);
            }
            {
                var err = s.connect(conn, addr);

                if (err != null)
                {
                    conn.Close();
                    return (null, err);
                }

            }
            return (conn, null);
        }

        // connect takes an existing connection to a socks5 proxy server,
        // and commands the server to extend that connection to target,
        // which must be a canonical address with a host and port.
        private static error connect(this ref socks5 s, net.Conn conn, @string target)
        {
            var (host, portStr, err) = net.SplitHostPort(target);
            if (err != null)
            {
                return error.As(err);
            }
            var (port, err) = strconv.Atoi(portStr);
            if (err != null)
            {
                return error.As(errors.New("proxy: failed to parse port number: " + portStr));
            }
            if (port < 1L || port > 0xffffUL)
            {
                return error.As(errors.New("proxy: port number out of range: " + portStr));
            } 

            // the size here is just an estimate
            var buf = make_slice<byte>(0L, 6L + len(host));

            buf = append(buf, socks5Version);
            if (len(s.user) > 0L && len(s.user) < 256L && len(s.password) < 256L)
            {
                buf = append(buf, 2L, socks5AuthNone, socks5AuthPassword);
            }
            else
            {
                buf = append(buf, 1L, socks5AuthNone);
            }
            {
                var (_, err) = conn.Write(buf);

                if (err != null)
                {
                    return error.As(errors.New("proxy: failed to write greeting to SOCKS5 proxy at " + s.addr + ": " + err.Error()));
                }

            }

            {
                (_, err) = io.ReadFull(conn, buf[..2L]);

                if (err != null)
                {
                    return error.As(errors.New("proxy: failed to read greeting from SOCKS5 proxy at " + s.addr + ": " + err.Error()));
                }

            }
            if (buf[0L] != 5L)
            {
                return error.As(errors.New("proxy: SOCKS5 proxy at " + s.addr + " has unexpected version " + strconv.Itoa(int(buf[0L]))));
            }
            if (buf[1L] == 0xffUL)
            {
                return error.As(errors.New("proxy: SOCKS5 proxy at " + s.addr + " requires authentication"));
            }
            if (buf[1L] == socks5AuthPassword)
            {
                buf = buf[..0L];
                buf = append(buf, 1L);
                buf = append(buf, uint8(len(s.user)));
                buf = append(buf, s.user);
                buf = append(buf, uint8(len(s.password)));
                buf = append(buf, s.password);

                {
                    (_, err) = conn.Write(buf);

                    if (err != null)
                    {
                        return error.As(errors.New("proxy: failed to write authentication request to SOCKS5 proxy at " + s.addr + ": " + err.Error()));
                    }

                }

                {
                    (_, err) = io.ReadFull(conn, buf[..2L]);

                    if (err != null)
                    {
                        return error.As(errors.New("proxy: failed to read authentication reply from SOCKS5 proxy at " + s.addr + ": " + err.Error()));
                    }

                }

                if (buf[1L] != 0L)
                {
                    return error.As(errors.New("proxy: SOCKS5 proxy at " + s.addr + " rejected username/password"));
                }
            }
            buf = buf[..0L];
            buf = append(buf, socks5Version, socks5Connect, 0L);

            {
                var ip = net.ParseIP(host);

                if (ip != null)
                {
                    {
                        var ip4 = ip.To4();

                        if (ip4 != null)
                        {
                            buf = append(buf, socks5IP4);
                            ip = ip4;
                        }
                        else
                        {
                            buf = append(buf, socks5IP6);
                        }

                    }
                    buf = append(buf, ip);
                }
                else
                {
                    if (len(host) > 255L)
                    {
                        return error.As(errors.New("proxy: destination hostname too long: " + host));
                    }
                    buf = append(buf, socks5Domain);
                    buf = append(buf, byte(len(host)));
                    buf = append(buf, host);
                }

            }
            buf = append(buf, byte(port >> (int)(8L)), byte(port));

            {
                (_, err) = conn.Write(buf);

                if (err != null)
                {
                    return error.As(errors.New("proxy: failed to write connect request to SOCKS5 proxy at " + s.addr + ": " + err.Error()));
                }

            }

            {
                (_, err) = io.ReadFull(conn, buf[..4L]);

                if (err != null)
                {
                    return error.As(errors.New("proxy: failed to read connect reply from SOCKS5 proxy at " + s.addr + ": " + err.Error()));
                }

            }

            @string failure = "unknown error";
            if (int(buf[1L]) < len(socks5Errors))
            {
                failure = socks5Errors[buf[1L]];
            }
            if (len(failure) > 0L)
            {
                return error.As(errors.New("proxy: SOCKS5 proxy at " + s.addr + " failed to connect: " + failure));
            }
            long bytesToDiscard = 0L;

            if (buf[3L] == socks5IP4) 
                bytesToDiscard = net.IPv4len;
            else if (buf[3L] == socks5IP6) 
                bytesToDiscard = net.IPv6len;
            else if (buf[3L] == socks5Domain) 
                (_, err) = io.ReadFull(conn, buf[..1L]);
                if (err != null)
                {
                    return error.As(errors.New("proxy: failed to read domain length from SOCKS5 proxy at " + s.addr + ": " + err.Error()));
                }
                bytesToDiscard = int(buf[0L]);
            else 
                return error.As(errors.New("proxy: got unknown address type " + strconv.Itoa(int(buf[3L])) + " from SOCKS5 proxy at " + s.addr));
                        if (cap(buf) < bytesToDiscard)
            {
                buf = make_slice<byte>(bytesToDiscard);
            }
            else
            {
                buf = buf[..bytesToDiscard];
            }
            {
                (_, err) = io.ReadFull(conn, buf);

                if (err != null)
                {
                    return error.As(errors.New("proxy: failed to read address from SOCKS5 proxy at " + s.addr + ": " + err.Error()));
                } 

                // Also need to discard the port number

            } 

            // Also need to discard the port number
            {
                (_, err) = io.ReadFull(conn, buf[..2L]);

                if (err != null)
                {
                    return error.As(errors.New("proxy: failed to read port from SOCKS5 proxy at " + s.addr + ": " + err.Error()));
                }

            }

            return error.As(null);
        }
    }
}}}}}
