// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build cgo,!netgo
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package net -- go2cs converted at 2020 October 09 04:50:27 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\cgo_unix.go
/*
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h>
#include <unistd.h>
#include <string.h>

// If nothing else defined EAI_OVERFLOW, make sure it has a value.
#ifndef EAI_OVERFLOW
#define EAI_OVERFLOW -12
#endif
*/
using C = go.C_package;/*
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h>
#include <unistd.h>
#include <string.h>

// If nothing else defined EAI_OVERFLOW, make sure it has a value.
#ifndef EAI_OVERFLOW
#define EAI_OVERFLOW -12
#endif
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
            slice<@string> hosts = default;
            error err = default!;
            bool completed = default;

            var (addrs, err, completed) = cgoLookupIP(ctx, "ip", name);
            foreach (var (_, addr) in addrs)
            {
                hosts = append(hosts, addr.String());
            }
            return ;

        }

        private static (long, error, bool) cgoLookupPort(context.Context ctx, @string network, @string service)
        {
            long port = default;
            error err = default!;
            bool completed = default;

            ref C.struct_addrinfo hints = ref heap(out ptr<C.struct_addrinfo> _addr_hints);
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
                    return (0L, error.As(addr(new DNSError(Err:"unknown network",Name:network+"/"+service))!)!, true);
                    break;
            }
            switch (ipVersion(network))
            {
                case '4': 
                    hints.ai_family = C.AF_INET;
                    break;
                case '6': 
                    hints.ai_family = C.AF_INET6;
                    break;
            }
            if (ctx.Done() == null)
            {
                var (port, err) = cgoLookupServicePort(_addr_hints, network, service);
                return (port, error.As(err)!, true);
            }

            var result = make_channel<portLookupResult>(1L);
            go_(() => cgoPortLookup(result, _addr_hints, network, service));
            return (r.port, error.As(r.err)!, true);
            return (0L, error.As(mapErr(ctx.Err()))!, false);

        }

        private static (long, error) cgoLookupServicePort(ptr<C.struct_addrinfo> _addr_hints, @string network, @string service) => func((defer, _, __) =>
        {
            long port = default;
            error err = default!;
            ref C.struct_addrinfo hints = ref _addr_hints.val;

            var cservice = make_slice<byte>(len(service) + 1L);
            copy(cservice, service); 
            // Lowercase the C service name.
            foreach (var (i, b) in cservice[..len(service)])
            {
                cservice[i] = lowerASCII(b);
            }
            ptr<C.struct_addrinfo> res;
            var (gerrno, err) = C.getaddrinfo(null, (C.@char.val)(@unsafe.Pointer(_addr_cservice[0L])), hints, _addr_res);
            if (gerrno != 0L)
            {
                var isTemporary = false;

                if (gerrno == C.EAI_SYSTEM) 
                    if (err == null)
                    { // see golang.org/issue/6232
                        err = syscall.EMFILE;

                    }

                else 
                    err = addrinfoErrno(gerrno);
                    isTemporary = addrinfoErrno(gerrno).Temporary();
                                return (0L, error.As(addr(new DNSError(Err:err.Error(),Name:network+"/"+service,IsTemporary:isTemporary))!)!);

            }

            defer(C.freeaddrinfo(res));

            {
                var r = res;

                while (r != null)
                {

                    if (r.ai_family == C.AF_INET) 
                        var sa = (syscall.RawSockaddrInet4.val)(@unsafe.Pointer(r.ai_addr));
                        ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.Port));
                        return (int(p[0L]) << (int)(8L) | int(p[1L]), error.As(null!)!);
                    else if (r.ai_family == C.AF_INET6) 
                        sa = (syscall.RawSockaddrInet6.val)(@unsafe.Pointer(r.ai_addr));
                        p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.Port));
                        return (int(p[0L]) << (int)(8L) | int(p[1L]), error.As(null!)!);
                                        r = r.ai_next;
                }

            }
            return (0L, error.As(addr(new DNSError(Err:"unknown port",Name:network+"/"+service))!)!);

        });

        private static void cgoPortLookup(channel<portLookupResult> result, ptr<C.struct_addrinfo> _addr_hints, @string network, @string service)
        {
            ref C.struct_addrinfo hints = ref _addr_hints.val;

            var (port, err) = cgoLookupServicePort(_addr_hints, network, service);
            result.Send(new portLookupResult(port,err));
        }

        private static (slice<IPAddr>, @string, error) cgoLookupIPCNAME(@string network, @string name) => func((defer, _, __) =>
        {
            slice<IPAddr> addrs = default;
            @string cname = default;
            error err = default!;

            acquireThread();
            defer(releaseThread());

            ref C.struct_addrinfo hints = ref heap(out ptr<C.struct_addrinfo> _addr_hints);
            hints.ai_flags = cgoAddrInfoFlags;
            hints.ai_socktype = C.SOCK_STREAM;
            hints.ai_family = C.AF_UNSPEC;
            switch (ipVersion(network))
            {
                case '4': 
                    hints.ai_family = C.AF_INET;
                    break;
                case '6': 
                    hints.ai_family = C.AF_INET6;
                    break;
            }

            var h = make_slice<byte>(len(name) + 1L);
            copy(h, name);
            ptr<C.struct_addrinfo> res;
            var (gerrno, err) = C.getaddrinfo((C.@char.val)(@unsafe.Pointer(_addr_h[0L])), null, _addr_hints, _addr_res);
            if (gerrno != 0L)
            {
                var isErrorNoSuchHost = false;
                var isTemporary = false;

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
                    isErrorNoSuchHost = true;
                else 
                    err = addrinfoErrno(gerrno);
                    isTemporary = addrinfoErrno(gerrno).Temporary();
                                return (null, "", error.As(addr(new DNSError(Err:err.Error(),Name:name,IsNotFound:isErrorNoSuchHost,IsTemporary:isTemporary))!)!);

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
                        var sa = (syscall.RawSockaddrInet4.val)(@unsafe.Pointer(r.ai_addr));
                        IPAddr addr = new IPAddr(IP:copyIP(sa.Addr[:]));
                        addrs = append(addrs, addr);
                    else if (r.ai_family == C.AF_INET6) 
                        sa = (syscall.RawSockaddrInet6.val)(@unsafe.Pointer(r.ai_addr));
                        addr = new IPAddr(IP:copyIP(sa.Addr[:]),Zone:zoneCache.name(int(sa.Scope_id)));
                        addrs = append(addrs, addr);
                    
                }

            }
            return (addrs, cname, error.As(null!)!);

        });

        private static void cgoIPLookup(channel<ipLookupResult> result, @string network, @string name)
        {
            var (addrs, cname, err) = cgoLookupIPCNAME(network, name);
            result.Send(new ipLookupResult(addrs,cname,err));
        }

        private static (slice<IPAddr>, error, bool) cgoLookupIP(context.Context ctx, @string network, @string name)
        {
            slice<IPAddr> addrs = default;
            error err = default!;
            bool completed = default;

            if (ctx.Done() == null)
            {
                addrs, _, err = cgoLookupIPCNAME(network, name);
                return (addrs, error.As(err)!, true);
            }

            var result = make_channel<ipLookupResult>(1L);
            go_(() => cgoIPLookup(result, network, name));
            return (r.addrs, error.As(r.err)!, true);
            return (null, error.As(mapErr(ctx.Err()))!, false);

        }

        private static (@string, error, bool) cgoLookupCNAME(context.Context ctx, @string name)
        {
            @string cname = default;
            error err = default!;
            bool completed = default;

            if (ctx.Done() == null)
            {
                _, cname, err = cgoLookupIPCNAME("ip", name);
                return (cname, error.As(err)!, true);
            }

            var result = make_channel<ipLookupResult>(1L);
            go_(() => cgoIPLookup(result, "ip", name));
            return (r.cname, error.As(r.err)!, true);
            return ("", error.As(mapErr(ctx.Err()))!, false);

        }

        // These are roughly enough for the following:
        //
        // Source        Encoding            Maximum length of single name entry
        // Unicast DNS        ASCII or            <=253 + a NUL terminator
        //            Unicode in RFC 5892        252 * total number of labels + delimiters + a NUL terminator
        // Multicast DNS    UTF-8 in RFC 5198 or        <=253 + a NUL terminator
        //            the same as unicast DNS ASCII    <=253 + a NUL terminator
        // Local database    various                depends on implementation
        private static readonly long nameinfoLen = (long)64L;
        private static readonly long maxNameinfoLen = (long)4096L;


        private static (slice<@string>, error, bool) cgoLookupPTR(context.Context ctx, @string addr)
        {
            slice<@string> names = default;
            error err = default!;
            bool completed = default;

            @string zone = default;
            var ip = parseIPv4(addr);
            if (ip == null)
            {
                ip, zone = parseIPv6Zone(addr);
            }

            if (ip == null)
            {
                return (null, error.As(addr(new DNSError(Err:"invalid address",Name:addr))!)!, true);
            }

            var (sa, salen) = cgoSockaddr(ip, zone);
            if (sa == null)
            {
                return (null, error.As(addr(new DNSError(Err:"invalid address "+ip.String(),Name:addr))!)!, true);
            }

            if (ctx.Done() == null)
            {
                var (names, err) = cgoLookupAddrPTR(addr, _addr_sa, salen);
                return (names, error.As(err)!, true);
            }

            var result = make_channel<reverseLookupResult>(1L);
            go_(() => cgoReverseLookup(result, addr, _addr_sa, salen));
            return (r.names, error.As(r.err)!, true);
            return (null, error.As(mapErr(ctx.Err()))!, false);

        }

        private static (slice<@string>, error) cgoLookupAddrPTR(@string addr, ptr<C.struct_sockaddr> _addr_sa, C.socklen_t salen) => func((defer, _, __) =>
        {
            slice<@string> names = default;
            error err = default!;
            ref C.struct_sockaddr sa = ref _addr_sa.val;

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
                var isTemporary = false;

                if (gerrno == C.EAI_SYSTEM) 
                    if (err == null)
                    { // see golang.org/issue/6232
                        err = syscall.EMFILE;

                    }

                else 
                    err = addrinfoErrno(gerrno);
                    isTemporary = addrinfoErrno(gerrno).Temporary();
                                return (null, error.As(addr(new DNSError(Err:err.Error(),Name:addr,IsTemporary:isTemporary))!)!);

            }

            for (long i = 0L; i < len(b); i++)
            {
                if (b[i] == 0L)
                {
                    b = b[..i];
                    break;
                }

            }

            return (new slice<@string>(new @string[] { absDomainName(b) }), error.As(null!)!);

        });

        private static void cgoReverseLookup(channel<reverseLookupResult> result, @string addr, ptr<C.struct_sockaddr> _addr_sa, C.socklen_t salen)
        {
            ref C.struct_sockaddr sa = ref _addr_sa.val;

            var (names, err) = cgoLookupAddrPTR(addr, _addr_sa, salen);
            result.Send(new reverseLookupResult(names,err));
        }

        private static (ptr<C.struct_sockaddr>, C.socklen_t) cgoSockaddr(IP ip, @string zone)
        {
            ptr<C.struct_sockaddr> _p0 = default!;
            C.socklen_t _p0 = default;

            {
                var ip4 = ip.To4();

                if (ip4 != null)
                {
                    return (_addr_cgoSockaddrInet4(ip4)!, C.socklen_t(syscall.SizeofSockaddrInet4));
                }

            }

            {
                var ip6 = ip.To16();

                if (ip6 != null)
                {
                    return (_addr_cgoSockaddrInet6(ip6, zoneCache.index(zone))!, C.socklen_t(syscall.SizeofSockaddrInet6));
                }

            }

            return (_addr_null!, 0L);

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
