// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:26:51 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\ipsock_plan9.go
using context = go.context_package;
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class net_package
    {
        // Probe probes IPv4, IPv6 and IPv4-mapped IPv6 communication
        // capabilities.
        //
        // Plan 9 uses IPv6 natively, see ip(3).
        private static void probe(this ref ipStackCapabilities p)
        {
            p.ipv4Enabled = probe(netdir + "/iproute", "4i");
            p.ipv6Enabled = probe(netdir + "/iproute", "6i");
            if (p.ipv4Enabled && p.ipv6Enabled)
            {
                p.ipv4MappedIPv6Enabled = true;
            }
        }

        private static bool probe(@string filename, @string query) => func((defer, _, __) =>
        {
            ref file file = default;
            error err = default;
            file, err = open(filename);

            if (err != null)
            {
                return false;
            }
            defer(file.close());

            var r = false;
            {
                var (line, ok) = file.readLine();

                while (ok && !r)
                {
                    var f = getFields(line);
                    if (len(f) < 3L)
                    {
                        continue;
                    line, ok = file.readLine();
                    }
                    for (long i = 0L; i < len(f); i++)
                    {
                        if (query == f[i])
                        {
                            r = true;
                            break;
                        }
                    }

                }

            }
            return r;
        });

        // parsePlan9Addr parses address of the form [ip!]port (e.g. 127.0.0.1!80).
        private static (IP, long, error) parsePlan9Addr(@string s)
        {
            var addr = IPv4zero; // address contains port only
            var i = byteIndex(s, '!');
            if (i >= 0L)
            {
                addr = ParseIP(s[..i]);
                if (addr == null)
                {
                    return (null, 0L, ref new ParseError(Type:"IP address",Text:s));
                }
            }
            var (p, _, ok) = dtoi(s[i + 1L..]);
            if (!ok)
            {
                return (null, 0L, ref new ParseError(Type:"port",Text:s));
            }
            if (p < 0L || p > 0xFFFFUL)
            {
                return (null, 0L, ref new AddrError(Err:"invalid port",Addr:string(p)));
            }
            return (addr, p, null);
        }

        private static (Addr, error) readPlan9Addr(@string proto, @string filename) => func((defer, _, __) =>
        {
            array<byte> buf = new array<byte>(128L);

            var (f, err) = os.Open(filename);
            if (err != null)
            {
                return;
            }
            defer(f.Close());
            var (n, err) = f.Read(buf[..]);
            if (err != null)
            {
                return;
            }
            var (ip, port, err) = parsePlan9Addr(string(buf[..n]));
            if (err != null)
            {
                return;
            }
            switch (proto)
            {
                case "tcp": 
                    addr = ref new TCPAddr(IP:ip,Port:port);
                    break;
                case "udp": 
                    addr = ref new UDPAddr(IP:ip,Port:port);
                    break;
                default: 
                    return (null, UnknownNetworkError(proto));
                    break;
            }
            return (addr, null);
        });

        private static (ref os.File, @string, @string, @string, error) startPlan9(context.Context ctx, @string net, Addr addr)
        {
            IP ip = default;            long port = default;
            switch (addr.type())
            {
                case ref TCPAddr a:
                    proto = "tcp";
                    ip = a.IP;
                    port = a.Port;
                    break;
                case ref UDPAddr a:
                    proto = "udp";
                    ip = a.IP;
                    port = a.Port;
                    break;
                default:
                {
                    var a = addr.type();
                    err = UnknownNetworkError(net);
                    return;
                    break;
                }

            }

            if (port > 65535L)
            {
                err = InvalidAddrError("port should be < 65536");
                return;
            }
            var (clone, dest, err) = queryCS1(ctx, proto, ip, port);
            if (err != null)
            {
                return;
            }
            var (f, err) = os.OpenFile(clone, os.O_RDWR, 0L);
            if (err != null)
            {
                return;
            }
            array<byte> buf = new array<byte>(16L);
            var (n, err) = f.Read(buf[..]);
            if (err != null)
            {
                f.Close();
                return;
            }
            return (f, dest, proto, string(buf[..n]), null);
        }

        private static void fixErr(error err)
        {
            ref OpError (oe, ok) = err._<ref OpError>();
            if (!ok)
            {
                return;
            }
            Func<Addr, bool> nonNilInterface = a =>
            {
                switch (a.type())
                {
                    case ref TCPAddr a:
                        return a == null;
                        break;
                    case ref UDPAddr a:
                        return a == null;
                        break;
                    case ref IPAddr a:
                        return a == null;
                        break;
                    default:
                    {
                        var a = a.type();
                        return false;
                        break;
                    }
                }
            }
;
            if (nonNilInterface(oe.Source))
            {
                oe.Source = null;
            }
            if (nonNilInterface(oe.Addr))
            {
                oe.Addr = null;
            }
            {
                ref os.PathError (pe, ok) = oe.Err._<ref os.PathError>();

                if (ok)
                {
                    _, ok = pe.Err._<syscall.ErrorString>();

                    if (ok)
                    {
                        oe.Err = pe.Err;
                    }
                }

            }
        }

        private static (ref netFD, error) dialPlan9(context.Context ctx, @string net, Addr laddr, Addr raddr) => func((defer, _, __) =>
        {
            defer(() =>
            {
                fixErr(err);

            }());
            private partial struct res
            {
                public ptr<netFD> fd;
                public error err;
            }
            var resc = make_channel<res>();
            go_(() => () =>
            {
                testHookDialChannel();
                var (fd, err) = dialPlan9Blocking(ctx, net, laddr, raddr);
                if (fd != null)
                {
                    fd.Close();
                }
            }());
            return (res.fd, res.err);
            return (null, mapErr(ctx.Err()));
        });

        private static (ref netFD, error) dialPlan9Blocking(context.Context ctx, @string net, Addr laddr, Addr raddr)
        {
            if (isWildcard(raddr))
            {
                raddr = toLocal(raddr, net);
            }
            var (f, dest, proto, name, err) = startPlan9(ctx, net, raddr);
            if (err != null)
            {
                return (null, err);
            }
            _, err = f.WriteString("connect " + dest);
            if (err != null)
            {
                f.Close();
                return (null, err);
            }
            var (data, err) = os.OpenFile(netdir + "/" + proto + "/" + name + "/data", os.O_RDWR, 0L);
            if (err != null)
            {
                f.Close();
                return (null, err);
            }
            laddr, err = readPlan9Addr(proto, netdir + "/" + proto + "/" + name + "/local");
            if (err != null)
            {
                data.Close();
                f.Close();
                return (null, err);
            }
            return newFD(proto, name, null, f, data, laddr, raddr);
        }

        private static (ref netFD, error) listenPlan9(context.Context ctx, @string net, Addr laddr) => func((defer, _, __) =>
        {
            defer(() =>
            {
                fixErr(err);

            }());
            var (f, dest, proto, name, err) = startPlan9(ctx, net, laddr);
            if (err != null)
            {
                return (null, err);
            }
            _, err = f.WriteString("announce " + dest);
            if (err != null)
            {
                f.Close();
                return (null, err);
            }
            laddr, err = readPlan9Addr(proto, netdir + "/" + proto + "/" + name + "/local");
            if (err != null)
            {
                f.Close();
                return (null, err);
            }
            return newFD(proto, name, null, f, null, laddr, null);
        });

        private static (ref netFD, error) netFD(this ref netFD fd)
        {
            return newFD(fd.net, fd.n, fd.listen, fd.ctl, fd.data, fd.laddr, fd.raddr);
        }

        private static (ref netFD, error) acceptPlan9(this ref netFD _fd) => func(_fd, (ref netFD fd, Defer defer, Panic _, Recover __) =>
        {
            defer(() =>
            {
                fixErr(err);

            }());
            {
                var err = fd.pfd.ReadLock();

                if (err != null)
                {
                    return (null, err);
                }

            }
            defer(fd.pfd.ReadUnlock());
            var (listen, err) = os.Open(fd.dir + "/listen");
            if (err != null)
            {
                return (null, err);
            }
            array<byte> buf = new array<byte>(16L);
            var (n, err) = listen.Read(buf[..]);
            if (err != null)
            {
                listen.Close();
                return (null, err);
            }
            var name = string(buf[..n]);
            var (ctl, err) = os.OpenFile(netdir + "/" + fd.net + "/" + name + "/ctl", os.O_RDWR, 0L);
            if (err != null)
            {
                listen.Close();
                return (null, err);
            }
            var (data, err) = os.OpenFile(netdir + "/" + fd.net + "/" + name + "/data", os.O_RDWR, 0L);
            if (err != null)
            {
                listen.Close();
                ctl.Close();
                return (null, err);
            }
            var (raddr, err) = readPlan9Addr(fd.net, netdir + "/" + fd.net + "/" + name + "/remote");
            if (err != null)
            {
                listen.Close();
                ctl.Close();
                data.Close();
                return (null, err);
            }
            return newFD(fd.net, name, listen, ctl, data, fd.laddr, raddr);
        });

        private static bool isWildcard(Addr a)
        {
            bool wildcard = default;
            switch (a.type())
            {
                case ref TCPAddr a:
                    wildcard = a.isWildcard();
                    break;
                case ref UDPAddr a:
                    wildcard = a.isWildcard();
                    break;
                case ref IPAddr a:
                    wildcard = a.isWildcard();
                    break;
            }
            return wildcard;
        }

        private static Addr toLocal(Addr a, @string net)
        {
            switch (a.type())
            {
                case ref TCPAddr a:
                    a.IP = loopbackIP(net);
                    break;
                case ref UDPAddr a:
                    a.IP = loopbackIP(net);
                    break;
                case ref IPAddr a:
                    a.IP = loopbackIP(net);
                    break;
            }
            return a;
        }
    }
}
