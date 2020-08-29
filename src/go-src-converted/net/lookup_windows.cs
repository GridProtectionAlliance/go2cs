// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:27:03 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\lookup_windows.go
using context = go.context_package;
using os = go.os_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class net_package
    {
        private static readonly var _WSAHOST_NOT_FOUND = syscall.Errno(11001L);



        private static error winError(@string call, error err)
        {

            if (err == _WSAHOST_NOT_FOUND) 
                return error.As(errNoSuchHost);
                        return error.As(os.NewSyscallError(call, err));
        }

        private static (long, error) getprotobyname(@string name)
        {
            var (p, err) = syscall.GetProtoByName(name);
            if (err != null)
            {
                return (0L, winError("getprotobyname", err));
            }
            return (int(p.Proto), null);
        }

        // lookupProtocol looks up IP protocol name and returns correspondent protocol number.
        private static (long, error) lookupProtocol(context.Context ctx, @string name) => func((defer, _, __) =>
        { 
            // GetProtoByName return value is stored in thread local storage.
            // Start new os thread before the call to prevent races.
            private partial struct result
            {
                public long proto;
                public error err;
            }
            var ch = make_channel<result>(); // unbuffered
            go_(() => () =>
            {
                acquireThread();
                defer(releaseThread());
                runtime.LockOSThread();
                defer(runtime.UnlockOSThread());
                var (proto, err) = getprotobyname(name);
            }());
            if (r.err != null)
            {
                {
                    var proto__prev2 = proto;

                    (proto, err) = lookupProtocolMap(name);

                    if (err == null)
                    {
                        return (proto, null);
                    }

                    proto = proto__prev2;

                }
                r.err = ref new DNSError(Err:r.err.Error(),Name:name);
            }
            return (r.proto, r.err);
            return (0L, mapErr(ctx.Err()));
        });

        private static (slice<@string>, error) lookupHost(this ref Resolver r, context.Context ctx, @string name)
        {
            var (ips, err) = r.lookupIP(ctx, name);
            if (err != null)
            {
                return (null, err);
            }
            var addrs = make_slice<@string>(0L, len(ips));
            foreach (var (_, ip) in ips)
            {
                addrs = append(addrs, ip.String());
            }
            return (addrs, null);
        }

        private static (slice<IPAddr>, error) lookupIP(this ref Resolver _r, context.Context ctx, @string name) => func(_r, (ref Resolver r, Defer defer, Panic _, Recover __) =>
        { 
            // TODO(bradfitz,brainman): use ctx more. See TODO below.

            private partial struct ret
            {
                public slice<IPAddr> addrs;
                public error err;
            }
            var ch = make_channel<ret>(1L);
            go_(() => () =>
            {
                acquireThread();
                defer(releaseThread());
                syscall.AddrinfoW hints = new syscall.AddrinfoW(Family:syscall.AF_UNSPEC,Socktype:syscall.SOCK_STREAM,Protocol:syscall.IPPROTO_IP,);
                ref syscall.AddrinfoW result = default;
                var e = syscall.GetAddrInfoW(syscall.StringToUTF16Ptr(name), null, ref hints, ref result);
                if (e != null)
                {
                    ch.Send(new ret(err:&DNSError{Err:winError("getaddrinfow",e).Error(),Name:name}));
                }
                defer(syscall.FreeAddrInfoW(result));
                var addrs = make_slice<IPAddr>(0L, 5L);
                while (result != null)
                {
                    var addr = @unsafe.Pointer(result.Addr);

                    if (result.Family == syscall.AF_INET) 
                        var a = (syscall.RawSockaddrInet4.Value)(addr).Addr;
                        addrs = append(addrs, new IPAddr(IP:IPv4(a[0],a[1],a[2],a[3])));
                    else if (result.Family == syscall.AF_INET6) 
                        a = (syscall.RawSockaddrInet6.Value)(addr).Addr;
                        var zone = zoneCache.name(int((syscall.RawSockaddrInet6.Value)(addr).Scope_id));
                        addrs = append(addrs, new IPAddr(IP:IP{a[0],a[1],a[2],a[3],a[4],a[5],a[6],a[7],a[8],a[9],a[10],a[11],a[12],a[13],a[14],a[15]},Zone:zone));
                    else 
                        ch.Send(new ret(err:&DNSError{Err:syscall.EWINDOWS.Error(),Name:name}));
                                        result = result.Next;
                }

                ch.Send(new ret(addrs:addrs));
            }());
            return (r.addrs, r.err);
            return (null, ref new DNSError(Name:name,Err:ctx.Err().Error(),IsTimeout:ctx.Err()==context.DeadlineExceeded,));
        });

        private static (long, error) lookupPort(this ref Resolver _r, context.Context ctx, @string network, @string service) => func(_r, (ref Resolver r, Defer defer, Panic _, Recover __) =>
        {
            if (r.PreferGo)
            {
                return lookupPortMap(network, service);
            } 

            // TODO(bradfitz): finish ctx plumbing. Nothing currently depends on this.
            acquireThread();
            defer(releaseThread());
            int stype = default;
            switch (network)
            {
                case "tcp4": 

                case "tcp6": 
                    stype = syscall.SOCK_STREAM;
                    break;
                case "udp4": 

                case "udp6": 
                    stype = syscall.SOCK_DGRAM;
                    break;
            }
            syscall.AddrinfoW hints = new syscall.AddrinfoW(Family:syscall.AF_UNSPEC,Socktype:stype,Protocol:syscall.IPPROTO_IP,);
            ref syscall.AddrinfoW result = default;
            var e = syscall.GetAddrInfoW(null, syscall.StringToUTF16Ptr(service), ref hints, ref result);
            if (e != null)
            {
                {
                    var (port, err) = lookupPortMap(network, service);

                    if (err == null)
                    {
                        return (port, null);
                    }

                }
                return (0L, ref new DNSError(Err:winError("getaddrinfow",e).Error(),Name:network+"/"+service));
            }
            defer(syscall.FreeAddrInfoW(result));
            if (result == null)
            {
                return (0L, ref new DNSError(Err:syscall.EINVAL.Error(),Name:network+"/"+service));
            }
            var addr = @unsafe.Pointer(result.Addr);

            if (result.Family == syscall.AF_INET) 
                var a = (syscall.RawSockaddrInet4.Value)(addr);
                return (int(syscall.Ntohs(a.Port)), null);
            else if (result.Family == syscall.AF_INET6) 
                a = (syscall.RawSockaddrInet6.Value)(addr);
                return (int(syscall.Ntohs(a.Port)), null);
                        return (0L, ref new DNSError(Err:syscall.EINVAL.Error(),Name:network+"/"+service));
        });

        private static (@string, error) lookupCNAME(this ref Resolver __p0, context.Context ctx, @string name) => func(__p0, (ref Resolver _p0, Defer defer, Panic _, Recover __) =>
        { 
            // TODO(bradfitz): finish ctx plumbing. Nothing currently depends on this.
            acquireThread();
            defer(releaseThread());
            ref syscall.DNSRecord r = default;
            var e = syscall.DnsQuery(name, syscall.DNS_TYPE_CNAME, 0L, null, ref r, null); 
            // windows returns DNS_INFO_NO_RECORDS if there are no CNAME-s
            {
                syscall.Errno (errno, ok) = e._<syscall.Errno>();

                if (ok && errno == syscall.DNS_INFO_NO_RECORDS)
                { 
                    // if there are no aliases, the canonical name is the input name
                    return (absDomainName((slice<byte>)name), null);
                }

            }
            if (e != null)
            {
                return ("", ref new DNSError(Err:winError("dnsquery",e).Error(),Name:name));
            }
            defer(syscall.DnsRecordListFree(r, 1L));

            var resolved = resolveCNAME(syscall.StringToUTF16Ptr(name), r);
            var cname = syscall.UTF16ToString(new ptr<ref array<ushort>>(@unsafe.Pointer(resolved))[..]);
            return (absDomainName((slice<byte>)cname), null);
        });

        private static (@string, slice<ref SRV>, error) lookupSRV(this ref Resolver __p0, context.Context ctx, @string service, @string proto, @string name) => func(__p0, (ref Resolver _p0, Defer defer, Panic _, Recover __) =>
        { 
            // TODO(bradfitz): finish ctx plumbing. Nothing currently depends on this.
            acquireThread();
            defer(releaseThread());
            @string target = default;
            if (service == "" && proto == "")
            {
                target = name;
            }
            else
            {
                target = "_" + service + "._" + proto + "." + name;
            }
            ref syscall.DNSRecord r = default;
            var e = syscall.DnsQuery(target, syscall.DNS_TYPE_SRV, 0L, null, ref r, null);
            if (e != null)
            {
                return ("", null, ref new DNSError(Err:winError("dnsquery",e).Error(),Name:target));
            }
            defer(syscall.DnsRecordListFree(r, 1L));

            var srvs = make_slice<ref SRV>(0L, 10L);
            foreach (var (_, p) in validRecs(r, syscall.DNS_TYPE_SRV, target))
            {
                var v = (syscall.DNSSRVData.Value)(@unsafe.Pointer(ref p.Data[0L]));
                srvs = append(srvs, ref new SRV(absDomainName([]byte(syscall.UTF16ToString((*[256]uint16)(unsafe.Pointer(v.Target))[:]))),v.Port,v.Priority,v.Weight));
            }
            byPriorityWeight(srvs).sort();
            return (absDomainName((slice<byte>)target), srvs, null);
        });

        private static (slice<ref MX>, error) lookupMX(this ref Resolver __p0, context.Context ctx, @string name) => func(__p0, (ref Resolver _p0, Defer defer, Panic _, Recover __) =>
        { 
            // TODO(bradfitz): finish ctx plumbing. Nothing currently depends on this.
            acquireThread();
            defer(releaseThread());
            ref syscall.DNSRecord r = default;
            var e = syscall.DnsQuery(name, syscall.DNS_TYPE_MX, 0L, null, ref r, null);
            if (e != null)
            {
                return (null, ref new DNSError(Err:winError("dnsquery",e).Error(),Name:name));
            }
            defer(syscall.DnsRecordListFree(r, 1L));

            var mxs = make_slice<ref MX>(0L, 10L);
            foreach (var (_, p) in validRecs(r, syscall.DNS_TYPE_MX, name))
            {
                var v = (syscall.DNSMXData.Value)(@unsafe.Pointer(ref p.Data[0L]));
                mxs = append(mxs, ref new MX(absDomainName([]byte(syscall.UTF16ToString((*[256]uint16)(unsafe.Pointer(v.NameExchange))[:]))),v.Preference));
            }
            byPref(mxs).sort();
            return (mxs, null);
        });

        private static (slice<ref NS>, error) lookupNS(this ref Resolver __p0, context.Context ctx, @string name) => func(__p0, (ref Resolver _p0, Defer defer, Panic _, Recover __) =>
        { 
            // TODO(bradfitz): finish ctx plumbing. Nothing currently depends on this.
            acquireThread();
            defer(releaseThread());
            ref syscall.DNSRecord r = default;
            var e = syscall.DnsQuery(name, syscall.DNS_TYPE_NS, 0L, null, ref r, null);
            if (e != null)
            {
                return (null, ref new DNSError(Err:winError("dnsquery",e).Error(),Name:name));
            }
            defer(syscall.DnsRecordListFree(r, 1L));

            var nss = make_slice<ref NS>(0L, 10L);
            foreach (var (_, p) in validRecs(r, syscall.DNS_TYPE_NS, name))
            {
                var v = (syscall.DNSPTRData.Value)(@unsafe.Pointer(ref p.Data[0L]));
                nss = append(nss, ref new NS(absDomainName([]byte(syscall.UTF16ToString((*[256]uint16)(unsafe.Pointer(v.Host))[:])))));
            }
            return (nss, null);
        });

        private static (slice<@string>, error) lookupTXT(this ref Resolver __p0, context.Context ctx, @string name) => func(__p0, (ref Resolver _p0, Defer defer, Panic _, Recover __) =>
        { 
            // TODO(bradfitz): finish ctx plumbing. Nothing currently depends on this.
            acquireThread();
            defer(releaseThread());
            ref syscall.DNSRecord r = default;
            var e = syscall.DnsQuery(name, syscall.DNS_TYPE_TEXT, 0L, null, ref r, null);
            if (e != null)
            {
                return (null, ref new DNSError(Err:winError("dnsquery",e).Error(),Name:name));
            }
            defer(syscall.DnsRecordListFree(r, 1L));

            var txts = make_slice<@string>(0L, 10L);
            foreach (var (_, p) in validRecs(r, syscall.DNS_TYPE_TEXT, name))
            {
                var d = (syscall.DNSTXTData.Value)(@unsafe.Pointer(ref p.Data[0L]));
                @string s = "";
                foreach (var (_, v) in new ptr<ref array<ref ushort>>(@unsafe.Pointer(ref (d.StringArray[0L])))[..d.StringCount])
                {
                    s += syscall.UTF16ToString(new ptr<ref array<ushort>>(@unsafe.Pointer(v))[..]);
                }
                txts = append(txts, s);
            }
            return (txts, null);
        });

        private static (slice<@string>, error) lookupAddr(this ref Resolver __p0, context.Context ctx, @string addr) => func(__p0, (ref Resolver _p0, Defer defer, Panic _, Recover __) =>
        { 
            // TODO(bradfitz): finish ctx plumbing. Nothing currently depends on this.
            acquireThread();
            defer(releaseThread());
            var (arpa, err) = reverseaddr(addr);
            if (err != null)
            {
                return (null, err);
            }
            ref syscall.DNSRecord r = default;
            var e = syscall.DnsQuery(arpa, syscall.DNS_TYPE_PTR, 0L, null, ref r, null);
            if (e != null)
            {
                return (null, ref new DNSError(Err:winError("dnsquery",e).Error(),Name:addr));
            }
            defer(syscall.DnsRecordListFree(r, 1L));

            var ptrs = make_slice<@string>(0L, 10L);
            foreach (var (_, p) in validRecs(r, syscall.DNS_TYPE_PTR, arpa))
            {
                var v = (syscall.DNSPTRData.Value)(@unsafe.Pointer(ref p.Data[0L]));
                ptrs = append(ptrs, absDomainName((slice<byte>)syscall.UTF16ToString(new ptr<ref array<ushort>>(@unsafe.Pointer(v.Host))[..])));
            }
            return (ptrs, null);
        });

        private static readonly ulong dnsSectionMask = 0x0003UL;

        // returns only results applicable to name and resolves CNAME entries


        // returns only results applicable to name and resolves CNAME entries
        private static slice<ref syscall.DNSRecord> validRecs(ref syscall.DNSRecord r, ushort dnstype, @string name)
        {
            var cname = syscall.StringToUTF16Ptr(name);
            if (dnstype != syscall.DNS_TYPE_CNAME)
            {
                cname = resolveCNAME(cname, r);
            }
            var rec = make_slice<ref syscall.DNSRecord>(0L, 10L);
            {
                var p = r;

                while (p != null)
                {
                    if (p.Dw & dnsSectionMask != syscall.DnsSectionAnswer)
                    {
                        continue;
                    p = p.Next;
                    }
                    if (p.Type != dnstype)
                    {
                        continue;
                    }
                    if (!syscall.DnsNameCompare(cname, p.Name))
                    {
                        continue;
                    }
                    rec = append(rec, p);
                }

            }
            return rec;
        }

        // returns the last CNAME in chain
        private static ref ushort resolveCNAME(ref ushort name, ref syscall.DNSRecord r)
        { 
            // limit cname resolving to 10 in case of a infinite CNAME loop
Cname:
            for (long cnameloop = 0L; cnameloop < 10L; cnameloop++)
            {
                {
                    var p = r;

                    while (p != null)
                    {
                        if (p.Dw & dnsSectionMask != syscall.DnsSectionAnswer)
                        {
                            continue;
                        p = p.Next;
                        }
                        if (p.Type != syscall.DNS_TYPE_CNAME)
                        {
                            continue;
                        }
                        if (!syscall.DnsNameCompare(name, p.Name))
                        {
                            continue;
                        }
                        name = (syscall.DNSPTRData.Value)(@unsafe.Pointer(ref r.Data[0L])).Host;
                        _continueCname = true;
                        break;
                    }

                }
                break;
            }
            return name;
        }
    }
}
