// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build cgo,!netgo
// +build darwin dragonfly freebsd linux netbsd openbsd solaris

// package net -- go2cs converted at 2020 August 29 08:25:11 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\cgo_unix.go
/*
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h>
#include <unistd.h>
#include <string.h>
*/
using C = go.C_package;/*
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h>
#include <unistd.h>
#include <string.h>
*/


using context = go.context_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System.Threading;

namespace go
{
    public static partial class net_package
    {
        // An addrinfoErrno represents a getaddrinfo, getnameinfo-specific
        // error number. It's a signed number and a zero value is a non-error
        // by convention.
        private partial struct addrinfoErrno // : long
        {
        }

        private static @string Error(this addrinfoErrno eai)
        {
            return C.GoString(C.gai_strerror(C.@int(eai)));
        }
        private static bool Temporary(this addrinfoErrno eai)
        {
            return eai == C.EAI_AGAIN;
        }
        private static bool Timeout(this addrinfoErrno eai)
        {
            return false;
        }

        private partial struct portLookupResult
        {
            public long port;
            public error err;
        }

        private partial struct ipLookupResult
        {
            public slice<IPAddr> addrs;
            public @string cname;
            public error err;
        }

        private partial struct reverseLookupResult
        {
            public slice<@string> names;
            public error err;
        }

        private static (slice<@string>, error, bool) cgoLookupHost(context.Context ctx, @string name)
        {
            var (addrs, err, completed) = cgoLookupIP(ctx, name);
            foreach (var (_, addr) in addrs)
            {
                hosts = append(hosts, addr.String());
            }
            return;
        }

        private static (long, error, bool) cgoLookupPort(context.Context ctx, @string network, @string service)
        {
            C.struct_addrinfo hints = default;
            switch (network)
            {
                case "": 
                    break;
                case "tcp": 

                case "tcp4": 

                case "tcp6": 
                    hints.ai_socktype = C.SOCK_STREAM;
                    hints.ai_protocol = C.IPPROTO_TCP;
                    break;
                case "udp": 

                case "udp4": 

                case "udp6": 
                    hints.ai_socktype = C.SOCK_DGRAM;
                    hints.ai_protocol = C.IPPROTO_UDP;
                    break;
                default: 
                    return (0L, ref new DNSError(Err:"unknown network",Name:network+"/"+service), true);
                    break;
            }
            if (len(network) >= 4L)
            {
                switch (network[3L])
                {
                    case '4': 
                        hints.ai_family = C.AF_INET;
                        break;
                    case '6': 
                        hints.ai_family = C.AF_INET6;
                        break;
                }
            }
            if (ctx.Done() == null)
            {
                var (port, err) = cgoLookupServicePort(ref hints, network, service);
                return (port, err, true);
            }
            var result = make_channel<portLookupResult>(1L);
            go_(() => cgoPortLookup(result, ref hints, network, service));
            return (r.port, r.err, true);
            return (0L, mapErr(ctx.Err()), false);
        }

        private static (long, error) cgoLookupServicePort(ref C.struct_addrinfo _hints, @string network, @string service) => func(_hints, (ref C.struct_addrinfo hints, Defer defer, Panic _, Recover __) =>
        {
            var cservice = make_slice<byte>(len(service) + 1L);
            copy(cservice, service); 
            // Lowercase the C service name.
            foreach (var (i, b) in cservice[..len(service)])
            {
                cservice[i] = lowerASCII(b);
            }
            ref C.struct_addrinfo res = default;
            var (gerrno, err) = C.getaddrinfo(null, (C.@char.Value)(@unsafe.Pointer(ref cservice[0L])), hints, ref res);
            if (gerrno != 0L)
            {

                if (gerrno == C.EAI_SYSTEM) 
                    if (err == null)
                    { // see golang.org/issue/6232
                        err = syscall.EMFILE;
                    }
                else 
                    err = addrinfoErrno(gerrno);
                                return (0L, ref new DNSError(Err:err.Error(),Name:network+"/"+service));
            }
            defer(C.freeaddrinfo(res));

            {
                var r = res;

                while (r != null)
                {

                    if (r.ai_family == C.AF_INET) 
                        var sa = (syscall.RawSockaddrInet4.Value)(@unsafe.Pointer(r.ai_addr));
                        ref array<byte> p = new ptr<ref array<byte>>(@unsafe.Pointer(ref sa.Port));
                        return (int(p[0L]) << (int)(8L) | int(p[1L]), null);
                    else if (r.ai_family == C.AF_INET6) 
                        sa = (syscall.RawSockaddrInet6.Value)(@unsafe.Pointer(r.ai_addr));
                        p = new ptr<ref array<byte>>(@unsafe.Pointer(ref sa.Port));
                        return (int(p[0L]) << (int)(8L) | int(p[1L]), null);
                                        r = r.ai_next;
                }

            }
            return (0L, ref new DNSError(Err:"unknown port",Name:network+"/"+service));
        });

        private static void cgoPortLookup(channel<portLookupResult> result, ref C.struct_addrinfo hints, @string network, @string service)
        {
            var (port, err) = cgoLookupServicePort(hints, network, service);
            result.Send(new portLookupResult(port,err));
        }

        private static (slice<IPAddr>, @string, error) cgoLookupIPCNAME(@string name) => func((defer, _, __) =>
        {
            acquireThread();
            defer(releaseThread());

            C.struct_addrinfo hints = default;
            hints.ai_flags = cgoAddrInfoFlags;
            hints.ai_socktype = C.SOCK_STREAM;

            var h = make_slice<byte>(len(name) + 1L);
            copy(h, name);
            ref C.struct_addrinfo res = default;
            var (gerrno, err) = C.getaddrinfo((C.@char.Value)(@unsafe.Pointer(ref h[0L])), null, ref hints, ref res);
            if (gerrno != 0L)
            {

                if (gerrno == C.EAI_SYSTEM) 
                    if (err == null)
                    { 
                        // err should not be nil, but sometimes getaddrinfo returns
                        // gerrno == C.EAI_SYSTEM with err == nil on Linux.
                        // The report claims that it happens when we have too many
                        // open files, so use syscall.EMFILE (too many open files in system).
                        // Most system calls would return ENFILE (too many open files),
                        // so at the least EMFILE should be easy to recognize if this
                        // comes up again. golang.org/issue/6232.
                        err = syscall.EMFILE;
                    }
                else if (gerrno == C.EAI_NONAME) 
                    err = errNoSuchHost;
                else 
                    err = addrinfoErrno(gerrno);
                                return (null, "", ref new DNSError(Err:err.Error(),Name:name));
            }
            defer(C.freeaddrinfo(res));

            if (res != null)
            {
                cname = C.GoString(res.ai_canonname);
                if (cname == "")
                {
                    cname = name;
                }
                if (len(cname) > 0L && cname[len(cname) - 1L] != '.')
                {
                    cname += ".";
                }
            }
            {
                var r = res;

                while (r != null)
                { 
                    // We only asked for SOCK_STREAM, but check anyhow.
                    if (r.ai_socktype != C.SOCK_STREAM)
                    {
                        continue;
                    r = r.ai_next;
                    }

                    if (r.ai_family == C.AF_INET) 
                        var sa = (syscall.RawSockaddrInet4.Value)(@unsafe.Pointer(r.ai_addr));
                        IPAddr addr = new IPAddr(IP:copyIP(sa.Addr[:]));
                        addrs = append(addrs, addr);
                    else if (r.ai_family == C.AF_INET6) 
                        sa = (syscall.RawSockaddrInet6.Value)(@unsafe.Pointer(r.ai_addr));
                        addr = new IPAddr(IP:copyIP(sa.Addr[:]),Zone:zoneCache.name(int(sa.Scope_id)));
                        addrs = append(addrs, addr);
                                    }

            }
            return (addrs, cname, null);
        });

        private static void cgoIPLookup(channel<ipLookupResult> result, @string name)
        {
            var (addrs, cname, err) = cgoLookupIPCNAME(name);
            result.Send(new ipLookupResult(addrs,cname,err));
        }

        private static (slice<IPAddr>, error, bool) cgoLookupIP(context.Context ctx, @string name)
        {
            if (ctx.Done() == null)
            {
                addrs, _, err = cgoLookupIPCNAME(name);
                return (addrs, err, true);
            }
            var result = make_channel<ipLookupResult>(1L);
            go_(() => cgoIPLookup(result, name));
            return (r.addrs, r.err, true);
            return (null, mapErr(ctx.Err()), false);
        }

        private static (@string, error, bool) cgoLookupCNAME(context.Context ctx, @string name)
        {
            if (ctx.Done() == null)
            {
                _, cname, err = cgoLookupIPCNAME(name);
                return (cname, err, true);
            }
            var result = make_channel<ipLookupResult>(1L);
            go_(() => cgoIPLookup(result, name));
            return (r.cname, r.err, true);
            return ("", mapErr(ctx.Err()), false);
        }

        // These are roughly enough for the following:
        //
        // Source        Encoding            Maximum length of single name entry
        // Unicast DNS        ASCII or            <=253 + a NUL terminator
        //            Unicode in RFC 5892        252 * total number of labels + delimiters + a NUL terminator
        // Multicast DNS    UTF-8 in RFC 5198 or        <=253 + a NUL terminator
        //            the same as unicast DNS ASCII    <=253 + a NUL terminator
        // Local database    various                depends on implementation
        private static readonly long nameinfoLen = 64L;
        private static readonly long maxNameinfoLen = 4096L;

        private static (slice<@string>, error, bool) cgoLookupPTR(context.Context ctx, @string addr)
        {
            @string zone = default;
            var ip = parseIPv4(addr);
            if (ip == null)
            {
                ip, zone = parseIPv6(addr, true);
            }
            if (ip == null)
            {
                return (null, ref new DNSError(Err:"invalid address",Name:addr), true);
            }
            var (sa, salen) = cgoSockaddr(ip, zone);
            if (sa == null)
            {
                return (null, ref new DNSError(Err:"invalid address "+ip.String(),Name:addr), true);
            }
            if (ctx.Done() == null)
            {
                var (names, err) = cgoLookupAddrPTR(addr, sa, salen);
                return (names, err, true);
            }
            var result = make_channel<reverseLookupResult>(1L);
            go_(() => cgoReverseLookup(result, addr, sa, salen));
            return (r.names, r.err, true);
            return (null, mapErr(ctx.Err()), false);
        }

        private static (slice<@string>, error) cgoLookupAddrPTR(@string addr, ref C.struct_sockaddr _sa, C.socklen_t salen) => func(_sa, (ref C.struct_sockaddr sa, Defer defer, Panic _, Recover __) =>
        {
            acquireThread();
            defer(releaseThread());

            long gerrno = default;
            slice<byte> b = default;
            {
                var l = nameinfoLen;

                while (l <= maxNameinfoLen)
                {
                    b = make_slice<byte>(l);
                    gerrno, err = cgoNameinfoPTR(b, sa, salen);
                    if (gerrno == 0L || gerrno != C.EAI_OVERFLOW)
                    {
                        break;
                    l *= 2L;
                    }
                }

            }
            if (gerrno != 0L)
            {

                if (gerrno == C.EAI_SYSTEM) 
                    if (err == null)
                    { // see golang.org/issue/6232
                        err = syscall.EMFILE;
                    }
                else 
                    err = addrinfoErrno(gerrno);
                                return (null, ref new DNSError(Err:err.Error(),Name:addr));
            }
            for (long i = 0L; i < len(b); i++)
            {
                if (b[i] == 0L)
                {
                    b = b[..i];
                    break;
                }
            }

            return (new slice<@string>(new @string[] { absDomainName(b) }), null);
        });

        private static void cgoReverseLookup(channel<reverseLookupResult> result, @string addr, ref C.struct_sockaddr sa, C.socklen_t salen)
        {
            var (names, err) = cgoLookupAddrPTR(addr, sa, salen);
            result.Send(new reverseLookupResult(names,err));
        }

        private static (ref C.struct_sockaddr, C.socklen_t) cgoSockaddr(IP ip, @string zone)
        {
            {
                var ip4 = ip.To4();

                if (ip4 != null)
                {
                    return (cgoSockaddrInet4(ip4), C.socklen_t(syscall.SizeofSockaddrInet4));
                }

            }
            {
                var ip6 = ip.To16();

                if (ip6 != null)
                {
                    return (cgoSockaddrInet6(ip6, zoneCache.index(zone)), C.socklen_t(syscall.SizeofSockaddrInet6));
                }

            }
            return (null, 0L);
        }

        private static IP copyIP(IP x)
        {
            if (len(x) < 16L)
            {
                return x.To16();
            }
            var y = make(IP, len(x));
            copy(y, x);
            return y;
        }
    }
}
