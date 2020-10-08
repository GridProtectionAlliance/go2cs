// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 08 03:33:55 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\lookup_plan9.go
using context = go.context_package;
using errors = go.errors_package;
using bytealg = go.@internal.bytealg_package;
using io = go.io_package;
using os = go.os_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class net_package
    {
        private static (slice<@string>, error) query(context.Context ctx, @string filename, @string query, long bufSize) => func((defer, _, __) =>
        {
            slice<@string> addrs = default;
            error err = default!;

            Func<(slice<@string>, error)> queryAddrs = () =>
            {
                var (file, err) = os.OpenFile(filename, os.O_RDWR, 0L);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }
                defer(file.Close());

                _, err = file.Seek(0L, io.SeekStart);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }
                _, err = file.WriteString(query);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }
                _, err = file.Seek(0L, io.SeekStart);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }
                var buf = make_slice<byte>(bufSize);
                while (true)
                {
                    var (n, _) = file.Read(buf);
                    if (n <= 0L)
                    {
                        break;
                    }
                    addrs = append(addrs, string(buf[..n]));

                }
                return (addrs, error.As(null!)!);

            };

            private partial struct ret
            {
                public slice<@string> addrs;
                public error err;
            }

            var ch = make_channel<ret>(1L);
            go_(() => () =>
            {
                var (addrs, err) = queryAddrs();
                ch.Send(new ret(addrs:addrs,err:err));
            }());

            return (r.addrs, error.As(r.err)!);
            return (null, error.As(addr(new DNSError(Name:query,Err:ctx.Err().Error(),IsTimeout:ctx.Err()==context.DeadlineExceeded,))!)!);

        });

        private static (slice<@string>, error) queryCS(context.Context ctx, @string net, @string host, @string service)
        {
            slice<@string> res = default;
            error err = default!;

            switch (net)
            {
                case "tcp4": 

                case "tcp6": 
                    net = "tcp";
                    break;
                case "udp4": 

                case "udp6": 
                    net = "udp";
                    break;
            }
            if (host == "")
            {
                host = "*";
            }

            return query(ctx, netdir + "/cs", net + "!" + host + "!" + service, 128L);

        }

        private static (@string, @string, error) queryCS1(context.Context ctx, @string net, IP ip, long port)
        {
            @string clone = default;
            @string dest = default;
            error err = default!;

            @string ips = "*";
            if (len(ip) != 0L && !ip.IsUnspecified())
            {
                ips = ip.String();
            }

            var (lines, err) = queryCS(ctx, net, ips, itoa(port));
            if (err != null)
            {
                return ;
            }

            var f = getFields(lines[0L]);
            if (len(f) < 2L)
            {
                return ("", "", error.As(errors.New("bad response from ndb/cs"))!);
            }

            clone = f[0L];
            dest = f[1L];
            return ;

        }

        private static (slice<@string>, error) queryDNS(context.Context ctx, @string addr, @string typ)
        {
            slice<@string> res = default;
            error err = default!;

            return query(ctx, netdir + "/dns", addr + " " + typ, 1024L);
        }

        // toLower returns a lower-case version of in. Restricting us to
        // ASCII is sufficient to handle the IP protocol names and allow
        // us to not depend on the strings and unicode packages.
        private static @string toLower(@string @in)
        {
            {
                var c__prev1 = c;

                foreach (var (_, __c) in in)
                {
                    c = __c;
                    if ('A' <= c && c <= 'Z')
                    { 
                        // Has upper case; need to fix.
                        slice<byte> @out = (slice<byte>)in;
                        for (long i = 0L; i < len(in); i++)
                        {
                            var c = in[i];
                            if ('A' <= c && c <= 'Z')
                            {
                                c += 'a' - 'A';
                            }

                            out[i] = c;

                        }

                        return string(out);

                    }

                }

                c = c__prev1;
            }

            return in;

        }

        // lookupProtocol looks up IP protocol name and returns
        // the corresponding protocol number.
        private static (long, error) lookupProtocol(context.Context ctx, @string name)
        {
            long proto = default;
            error err = default!;

            var (lines, err) = query(ctx, netdir + "/cs", "!protocol=" + toLower(name), 128L);
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            if (len(lines) == 0L)
            {
                return (0L, error.As(UnknownNetworkError(name))!);
            }

            var f = getFields(lines[0L]);
            if (len(f) < 2L)
            {
                return (0L, error.As(UnknownNetworkError(name))!);
            }

            var s = f[1L];
            {
                var (n, _, ok) = dtoi(s[bytealg.IndexByteString(s, '=') + 1L..]);

                if (ok)
                {
                    return (n, error.As(null!)!);
                }

            }

            return (0L, error.As(UnknownNetworkError(name))!);

        }

        private static (slice<@string>, error) lookupHost(this ptr<Resolver> _addr__p0, context.Context ctx, @string host)
        {
            slice<@string> addrs = default;
            error err = default!;
            ref Resolver _p0 = ref _addr__p0.val;
 
            // Use netdir/cs instead of netdir/dns because cs knows about
            // host names in local network (e.g. from /lib/ndb/local)
            var (lines, err) = queryCS(ctx, "net", host, "1");
            if (err != null)
            {
                ptr<DNSError> dnsError = addr(new DNSError(Err:err.Error(),Name:host));
                if (stringsHasSuffix(err.Error(), "dns failure"))
                {
                    dnsError.Err = errNoSuchHost.Error();
                    dnsError.IsNotFound = true;
                }

                return (null, error.As(dnsError)!);

            }

loop:
            foreach (var (_, line) in lines)
            {
                var f = getFields(line);
                if (len(f) < 2L)
                {
                    continue;
                }

                var addr = f[1L];
                {
                    var i = bytealg.IndexByteString(addr, '!');

                    if (i >= 0L)
                    {
                        addr = addr[..i]; // remove port
                    }

                }

                if (ParseIP(addr) == null)
                {
                    continue;
                } 
                // only return unique addresses
                foreach (var (_, a) in addrs)
                {
                    if (a == addr)
                    {
                        _continueloop = true;
                        break;
                    }

                }
                addrs = append(addrs, addr);

            }
            return ;

        }

        private static (slice<IPAddr>, error) lookupIP(this ptr<Resolver> _addr_r, context.Context ctx, @string _, @string host)
        {
            slice<IPAddr> addrs = default;
            error err = default!;
            ref Resolver r = ref _addr_r.val;

            var (lits, err) = r.lookupHost(ctx, host);
            if (err != null)
            {
                return ;
            }

            foreach (var (_, lit) in lits)
            {
                var (host, zone) = splitHostZone(lit);
                {
                    var ip = ParseIP(host);

                    if (ip != null)
                    {
                        IPAddr addr = new IPAddr(IP:ip,Zone:zone);
                        addrs = append(addrs, addr);
                    }

                }

            }
            return ;

        }

        private static (long, error) lookupPort(this ptr<Resolver> _addr__p0, context.Context ctx, @string network, @string service)
        {
            long port = default;
            error err = default!;
            ref Resolver _p0 = ref _addr__p0.val;

            switch (network)
            {
                case "tcp4": 

                case "tcp6": 
                    network = "tcp";
                    break;
                case "udp4": 

                case "udp6": 
                    network = "udp";
                    break;
            }
            var (lines, err) = queryCS(ctx, network, "127.0.0.1", toLower(service));
            if (err != null)
            {
                return ;
            }

            ptr<AddrError> unknownPortError = addr(new AddrError(Err:"unknown port",Addr:network+"/"+service));
            if (len(lines) == 0L)
            {
                return (0L, error.As(unknownPortError)!);
            }

            var f = getFields(lines[0L]);
            if (len(f) < 2L)
            {
                return (0L, error.As(unknownPortError)!);
            }

            var s = f[1L];
            {
                var i = bytealg.IndexByteString(s, '!');

                if (i >= 0L)
                {
                    s = s[i + 1L..]; // remove address
                }

            }

            {
                var (n, _, ok) = dtoi(s);

                if (ok)
                {
                    return (n, error.As(null!)!);
                }

            }

            return (0L, error.As(unknownPortError)!);

        }

        private static (@string, error) lookupCNAME(this ptr<Resolver> _addr__p0, context.Context ctx, @string name)
        {
            @string cname = default;
            error err = default!;
            ref Resolver _p0 = ref _addr__p0.val;

            var (lines, err) = queryDNS(ctx, name, "cname");
            if (err != null)
            {
                if (stringsHasSuffix(err.Error(), "dns failure") || stringsHasSuffix(err.Error(), "resource does not exist; negrcode 0"))
                {
                    cname = name + ".";
                    err = null;
                }

                return ;

            }

            if (len(lines) > 0L)
            {
                {
                    var f = getFields(lines[0L]);

                    if (len(f) >= 3L)
                    {
                        return (f[2L] + ".", error.As(null!)!);
                    }

                }

            }

            return ("", error.As(errors.New("bad response from ndb/dns"))!);

        }

        private static (@string, slice<ptr<SRV>>, error) lookupSRV(this ptr<Resolver> _addr__p0, context.Context ctx, @string service, @string proto, @string name)
        {
            @string cname = default;
            slice<ptr<SRV>> addrs = default;
            error err = default!;
            ref Resolver _p0 = ref _addr__p0.val;

            @string target = default;
            if (service == "" && proto == "")
            {
                target = name;
            }
            else
            {
                target = "_" + service + "._" + proto + "." + name;
            }

            var (lines, err) = queryDNS(ctx, target, "srv");
            if (err != null)
            {
                return ;
            }

            foreach (var (_, line) in lines)
            {
                var f = getFields(line);
                if (len(f) < 6L)
                {
                    continue;
                }

                var (port, _, portOk) = dtoi(f[4L]);
                var (priority, _, priorityOk) = dtoi(f[3L]);
                var (weight, _, weightOk) = dtoi(f[2L]);
                if (!(portOk && priorityOk && weightOk))
                {
                    continue;
                }

                addrs = append(addrs, addr(new SRV(absDomainName([]byte(f[5])),uint16(port),uint16(priority),uint16(weight))));
                cname = absDomainName((slice<byte>)f[0L]);

            }
            byPriorityWeight(addrs).sort();
            return ;

        }

        private static (slice<ptr<MX>>, error) lookupMX(this ptr<Resolver> _addr__p0, context.Context ctx, @string name)
        {
            slice<ptr<MX>> mx = default;
            error err = default!;
            ref Resolver _p0 = ref _addr__p0.val;

            var (lines, err) = queryDNS(ctx, name, "mx");
            if (err != null)
            {
                return ;
            }

            foreach (var (_, line) in lines)
            {
                var f = getFields(line);
                if (len(f) < 4L)
                {
                    continue;
                }

                {
                    var (pref, _, ok) = dtoi(f[2L]);

                    if (ok)
                    {
                        mx = append(mx, addr(new MX(absDomainName([]byte(f[3])),uint16(pref))));
                    }

                }

            }
            byPref(mx).sort();
            return ;

        }

        private static (slice<ptr<NS>>, error) lookupNS(this ptr<Resolver> _addr__p0, context.Context ctx, @string name)
        {
            slice<ptr<NS>> ns = default;
            error err = default!;
            ref Resolver _p0 = ref _addr__p0.val;

            var (lines, err) = queryDNS(ctx, name, "ns");
            if (err != null)
            {
                return ;
            }

            foreach (var (_, line) in lines)
            {
                var f = getFields(line);
                if (len(f) < 3L)
                {
                    continue;
                }

                ns = append(ns, addr(new NS(absDomainName([]byte(f[2])))));

            }
            return ;

        }

        private static (slice<@string>, error) lookupTXT(this ptr<Resolver> _addr__p0, context.Context ctx, @string name)
        {
            slice<@string> txt = default;
            error err = default!;
            ref Resolver _p0 = ref _addr__p0.val;

            var (lines, err) = queryDNS(ctx, name, "txt");
            if (err != null)
            {
                return ;
            }

            foreach (var (_, line) in lines)
            {
                {
                    var i = bytealg.IndexByteString(line, '\t');

                    if (i >= 0L)
                    {
                        txt = append(txt, absDomainName((slice<byte>)line[i + 1L..]));
                    }

                }

            }
            return ;

        }

        private static (slice<@string>, error) lookupAddr(this ptr<Resolver> _addr__p0, context.Context ctx, @string addr)
        {
            slice<@string> name = default;
            error err = default!;
            ref Resolver _p0 = ref _addr__p0.val;

            var (arpa, err) = reverseaddr(addr);
            if (err != null)
            {
                return ;
            }

            var (lines, err) = queryDNS(ctx, arpa, "ptr");
            if (err != null)
            {
                return ;
            }

            foreach (var (_, line) in lines)
            {
                var f = getFields(line);
                if (len(f) < 3L)
                {
                    continue;
                }

                name = append(name, absDomainName((slice<byte>)f[2L]));

            }
            return ;

        }

        // concurrentThreadsLimit returns the number of threads we permit to
        // run concurrently doing DNS lookups.
        private static long concurrentThreadsLimit()
        {
            return 500L;
        }
    }
}
