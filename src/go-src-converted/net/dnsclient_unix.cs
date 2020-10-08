// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// DNS client: see RFC 1035.
// Has to be linked into package net for Dial.

// TODO(rsc):
//    Could potentially handle many outstanding lookups faster.
//    Random UDP source port (net.Dial should do that for us).
//    Random request IDs.

// package net -- go2cs converted at 2020 October 08 03:31:41 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\dnsclient_unix.go
using context = go.context_package;
using errors = go.errors_package;
using io = go.io_package;
using rand = go.math.rand_package;
using os = go.os_package;
using sync = go.sync_package;
using time = go.time_package;

using dnsmessage = go.golang.org.x.net.dns.dnsmessage_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class net_package
    {
 
        // to be used as a useTCP parameter to exchange
        private static readonly var useTCPOnly = (var)true;
        private static readonly var useUDPOrTCP = (var)false;


        private static var errLameReferral = errors.New("lame referral");        private static var errCannotUnmarshalDNSMessage = errors.New("cannot unmarshal DNS message");        private static var errCannotMarshalDNSMessage = errors.New("cannot marshal DNS message");        private static var errServerMisbehaving = errors.New("server misbehaving");        private static var errInvalidDNSResponse = errors.New("invalid DNS response");        private static var errNoAnswerFromDNSServer = errors.New("no answer from DNS server");        private static var errServerTemporarilyMisbehaving = errors.New("server misbehaving");

        private static (ushort, slice<byte>, slice<byte>, error) newRequest(dnsmessage.Question q)
        {
            ushort id = default;
            slice<byte> udpReq = default;
            slice<byte> tcpReq = default;
            error err = default!;

            id = uint16(rand.Int()) ^ uint16(time.Now().UnixNano());
            var b = dnsmessage.NewBuilder(make_slice<byte>(2L, 514L), new dnsmessage.Header(ID:id,RecursionDesired:true));
            b.EnableCompression();
            {
                var err__prev1 = err;

                var err = b.StartQuestions();

                if (err != null)
                {
                    return (0L, null, null, error.As(err)!);
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = b.Question(q);

                if (err != null)
                {
                    return (0L, null, null, error.As(err)!);
                }

                err = err__prev1;

            }

            tcpReq, err = b.Finish();
            udpReq = tcpReq[2L..];
            var l = len(tcpReq) - 2L;
            tcpReq[0L] = byte(l >> (int)(8L));
            tcpReq[1L] = byte(l);
            return (id, udpReq, tcpReq, error.As(err)!);

        }

        private static bool checkResponse(ushort reqID, dnsmessage.Question reqQues, dnsmessage.Header respHdr, dnsmessage.Question respQues)
        {
            if (!respHdr.Response)
            {
                return false;
            }

            if (reqID != respHdr.ID)
            {
                return false;
            }

            if (reqQues.Type != respQues.Type || reqQues.Class != respQues.Class || !equalASCIIName(reqQues.Name, respQues.Name))
            {
                return false;
            }

            return true;

        }

        private static (dnsmessage.Parser, dnsmessage.Header, error) dnsPacketRoundTrip(Conn c, ushort id, dnsmessage.Question query, slice<byte> b)
        {
            dnsmessage.Parser _p0 = default;
            dnsmessage.Header _p0 = default;
            error _p0 = default!;

            {
                var (_, err) = c.Write(b);

                if (err != null)
                {
                    return (new dnsmessage.Parser(), new dnsmessage.Header(), error.As(err)!);
                }

            }


            b = make_slice<byte>(512L); // see RFC 1035
            while (true)
            {
                var (n, err) = c.Read(b);
                if (err != null)
                {
                    return (new dnsmessage.Parser(), new dnsmessage.Header(), error.As(err)!);
                }

                dnsmessage.Parser p = default; 
                // Ignore invalid responses as they may be malicious
                // forgery attempts. Instead continue waiting until
                // timeout. See golang.org/issue/13281.
                var (h, err) = p.Start(b[..n]);
                if (err != null)
                {
                    continue;
                }

                var (q, err) = p.Question();
                if (err != null || !checkResponse(id, query, h, q))
                {
                    continue;
                }

                return (p, h, error.As(null!)!);

            }


        }

        private static (dnsmessage.Parser, dnsmessage.Header, error) dnsStreamRoundTrip(Conn c, ushort id, dnsmessage.Question query, slice<byte> b)
        {
            dnsmessage.Parser _p0 = default;
            dnsmessage.Header _p0 = default;
            error _p0 = default!;

            {
                var (_, err) = c.Write(b);

                if (err != null)
                {
                    return (new dnsmessage.Parser(), new dnsmessage.Header(), error.As(err)!);
                }

            }


            b = make_slice<byte>(1280L); // 1280 is a reasonable initial size for IP over Ethernet, see RFC 4035
            {
                (_, err) = io.ReadFull(c, b[..2L]);

                if (err != null)
                {
                    return (new dnsmessage.Parser(), new dnsmessage.Header(), error.As(err)!);
                }

            }

            var l = int(b[0L]) << (int)(8L) | int(b[1L]);
            if (l > len(b))
            {
                b = make_slice<byte>(l);
            }

            var (n, err) = io.ReadFull(c, b[..l]);
            if (err != null)
            {
                return (new dnsmessage.Parser(), new dnsmessage.Header(), error.As(err)!);
            }

            dnsmessage.Parser p = default;
            var (h, err) = p.Start(b[..n]);
            if (err != null)
            {
                return (new dnsmessage.Parser(), new dnsmessage.Header(), error.As(errCannotUnmarshalDNSMessage)!);
            }

            var (q, err) = p.Question();
            if (err != null)
            {
                return (new dnsmessage.Parser(), new dnsmessage.Header(), error.As(errCannotUnmarshalDNSMessage)!);
            }

            if (!checkResponse(id, query, h, q))
            {
                return (new dnsmessage.Parser(), new dnsmessage.Header(), error.As(errInvalidDNSResponse)!);
            }

            return (p, h, error.As(null!)!);

        }

        // exchange sends a query on the connection and hopes for a response.
        private static (dnsmessage.Parser, dnsmessage.Header, error) exchange(this ptr<Resolver> _addr_r, context.Context ctx, @string server, dnsmessage.Question q, time.Duration timeout, bool useTCP) => func((defer, _, __) =>
        {
            dnsmessage.Parser _p0 = default;
            dnsmessage.Header _p0 = default;
            error _p0 = default!;
            ref Resolver r = ref _addr_r.val;

            q.Class = dnsmessage.ClassINET;
            var (id, udpReq, tcpReq, err) = newRequest(q);
            if (err != null)
            {
                return (new dnsmessage.Parser(), new dnsmessage.Header(), error.As(errCannotMarshalDNSMessage)!);
            }

            slice<@string> networks = default;
            if (useTCP)
            {
                networks = new slice<@string>(new @string[] { "tcp" });
            }
            else
            {
                networks = new slice<@string>(new @string[] { "udp", "tcp" });
            }

            foreach (var (_, network) in networks)
            {
                var (ctx, cancel) = context.WithDeadline(ctx, time.Now().Add(timeout));
                defer(cancel());

                var (c, err) = r.dial(ctx, network, server);
                if (err != null)
                {
                    return (new dnsmessage.Parser(), new dnsmessage.Header(), error.As(err)!);
                }

                {
                    var (d, ok) = ctx.Deadline();

                    if (ok && !d.IsZero())
                    {
                        c.SetDeadline(d);
                    }

                }

                dnsmessage.Parser p = default;
                dnsmessage.Header h = default;
                {
                    PacketConn (_, ok) = c._<PacketConn>();

                    if (ok)
                    {
                        p, h, err = dnsPacketRoundTrip(c, id, q, udpReq);
                    }
                    else
                    {
                        p, h, err = dnsStreamRoundTrip(c, id, q, tcpReq);
                    }

                }

                c.Close();
                if (err != null)
                {
                    return (new dnsmessage.Parser(), new dnsmessage.Header(), error.As(mapErr(err))!);
                }

                {
                    var err = p.SkipQuestion();

                    if (err != dnsmessage.ErrSectionDone)
                    {
                        return (new dnsmessage.Parser(), new dnsmessage.Header(), error.As(errInvalidDNSResponse)!);
                    }

                }

                if (h.Truncated)
                { // see RFC 5966
                    continue;

                }

                return (p, h, error.As(null!)!);

            }
            return (new dnsmessage.Parser(), new dnsmessage.Header(), error.As(errNoAnswerFromDNSServer)!);

        });

        // checkHeader performs basic sanity checks on the header.
        private static error checkHeader(ptr<dnsmessage.Parser> _addr_p, dnsmessage.Header h)
        {
            ref dnsmessage.Parser p = ref _addr_p.val;

            if (h.RCode == dnsmessage.RCodeNameError)
            {
                return error.As(errNoSuchHost)!;
            }

            var (_, err) = p.AnswerHeader();
            if (err != null && err != dnsmessage.ErrSectionDone)
            {
                return error.As(errCannotUnmarshalDNSMessage)!;
            } 

            // libresolv continues to the next server when it receives
            // an invalid referral response. See golang.org/issue/15434.
            if (h.RCode == dnsmessage.RCodeSuccess && !h.Authoritative && !h.RecursionAvailable && err == dnsmessage.ErrSectionDone)
            {
                return error.As(errLameReferral)!;
            }

            if (h.RCode != dnsmessage.RCodeSuccess && h.RCode != dnsmessage.RCodeNameError)
            { 
                // None of the error codes make sense
                // for the query we sent. If we didn't get
                // a name error and we didn't get success,
                // the server is behaving incorrectly or
                // having temporary trouble.
                if (h.RCode == dnsmessage.RCodeServerFailure)
                {
                    return error.As(errServerTemporarilyMisbehaving)!;
                }

                return error.As(errServerMisbehaving)!;

            }

            return error.As(null!)!;

        }

        private static error skipToAnswer(ptr<dnsmessage.Parser> _addr_p, dnsmessage.Type qtype)
        {
            ref dnsmessage.Parser p = ref _addr_p.val;

            while (true)
            {
                var (h, err) = p.AnswerHeader();
                if (err == dnsmessage.ErrSectionDone)
                {
                    return error.As(errNoSuchHost)!;
                }

                if (err != null)
                {
                    return error.As(errCannotUnmarshalDNSMessage)!;
                }

                if (h.Type == qtype)
                {
                    return error.As(null!)!;
                }

                {
                    var err = p.SkipAnswer();

                    if (err != null)
                    {
                        return error.As(errCannotUnmarshalDNSMessage)!;
                    }

                }

            }


        }

        // Do a lookup for a single name, which must be rooted
        // (otherwise answer will not find the answers).
        private static (dnsmessage.Parser, @string, error) tryOneName(this ptr<Resolver> _addr_r, context.Context ctx, ptr<dnsConfig> _addr_cfg, @string name, dnsmessage.Type qtype)
        {
            dnsmessage.Parser _p0 = default;
            @string _p0 = default;
            error _p0 = default!;
            ref Resolver r = ref _addr_r.val;
            ref dnsConfig cfg = ref _addr_cfg.val;

            error lastErr = default!;
            var serverOffset = cfg.serverOffset();
            var sLen = uint32(len(cfg.servers));

            var (n, err) = dnsmessage.NewName(name);
            if (err != null)
            {
                return (new dnsmessage.Parser(), "", error.As(errCannotMarshalDNSMessage)!);
            }

            dnsmessage.Question q = new dnsmessage.Question(Name:n,Type:qtype,Class:dnsmessage.ClassINET,);

            for (long i = 0L; i < cfg.attempts; i++)
            {
                for (var j = uint32(0L); j < sLen; j++)
                {
                    var server = cfg.servers[(serverOffset + j) % sLen];

                    var (p, h, err) = r.exchange(ctx, server, q, cfg.timeout, cfg.useTCP);
                    if (err != null)
                    {
                        ptr<DNSError> dnsErr = addr(new DNSError(Err:err.Error(),Name:name,Server:server,));
                        {
                            Error (nerr, ok) = err._<Error>();

                            if (ok && nerr.Timeout())
                            {
                                dnsErr.IsTimeout = true;
                            } 
                            // Set IsTemporary for socket-level errors. Note that this flag
                            // may also be used to indicate a SERVFAIL response.

                        } 
                        // Set IsTemporary for socket-level errors. Note that this flag
                        // may also be used to indicate a SERVFAIL response.
                        {
                            ptr<OpError> (_, ok) = err._<ptr<OpError>>();

                            if (ok)
                            {
                                dnsErr.IsTemporary = true;
                            }

                        }

                        lastErr = error.As(dnsErr)!;
                        continue;

                    }

                    {
                        var err = checkHeader(_addr_p, h);

                        if (err != null)
                        {
                            dnsErr = addr(new DNSError(Err:err.Error(),Name:name,Server:server,));
                            if (err == errServerTemporarilyMisbehaving)
                            {
                                dnsErr.IsTemporary = true;
                            }

                            if (err == errNoSuchHost)
                            { 
                                // The name does not exist, so trying
                                // another server won't help.

                                dnsErr.IsNotFound = true;
                                return (p, server, error.As(dnsErr)!);

                            }

                            lastErr = error.As(dnsErr)!;
                            continue;

                        }

                    }


                    err = skipToAnswer(_addr_p, qtype);
                    if (err == null)
                    {
                        return (p, server, error.As(null!)!);
                    }

                    lastErr = error.As(addr(new DNSError(Err:err.Error(),Name:name,Server:server,)))!;
                    if (err == errNoSuchHost)
                    { 
                        // The name does not exist, so trying another
                        // server won't help.

                        lastErr._<ptr<DNSError>>().IsNotFound = true;
                        return (p, server, error.As(lastErr)!);

                    }

                }


            }

            return (new dnsmessage.Parser(), "", error.As(lastErr)!);

        }

        // A resolverConfig represents a DNS stub resolver configuration.
        private partial struct resolverConfig
        {
            public sync.Once initOnce; // guards init of resolverConfig

// ch is used as a semaphore that only allows one lookup at a
// time to recheck resolv.conf.
            public channel<object> ch; // guards lastChecked and modTime
            public time.Time lastChecked; // last time resolv.conf was checked

            public sync.RWMutex mu; // protects dnsConfig
            public ptr<dnsConfig> dnsConfig; // parsed resolv.conf structure used in lookups
        }

        private static resolverConfig resolvConf = default;

        // init initializes conf and is only called via conf.initOnce.
        private static void init(this ptr<resolverConfig> _addr_conf)
        {
            ref resolverConfig conf = ref _addr_conf.val;
 
            // Set dnsConfig and lastChecked so we don't parse
            // resolv.conf twice the first time.
            conf.dnsConfig = systemConf().resolv;
            if (conf.dnsConfig == null)
            {
                conf.dnsConfig = dnsReadConfig("/etc/resolv.conf");
            }

            conf.lastChecked = time.Now(); 

            // Prepare ch so that only one update of resolverConfig may
            // run at once.
            conf.ch = make_channel<object>(1L);

        }

        // tryUpdate tries to update conf with the named resolv.conf file.
        // The name variable only exists for testing. It is otherwise always
        // "/etc/resolv.conf".
        private static void tryUpdate(this ptr<resolverConfig> _addr_conf, @string name) => func((defer, _, __) =>
        {
            ref resolverConfig conf = ref _addr_conf.val;

            conf.initOnce.Do(conf.init); 

            // Ensure only one update at a time checks resolv.conf.
            if (!conf.tryAcquireSema())
            {
                return ;
            }

            defer(conf.releaseSema());

            var now = time.Now();
            if (conf.lastChecked.After(now.Add(-5L * time.Second)))
            {
                return ;
            }

            conf.lastChecked = now;

            time.Time mtime = default;
            {
                var (fi, err) = os.Stat(name);

                if (err == null)
                {
                    mtime = fi.ModTime();
                }

            }

            if (mtime.Equal(conf.dnsConfig.mtime))
            {
                return ;
            }

            var dnsConf = dnsReadConfig(name);
            conf.mu.Lock();
            conf.dnsConfig = dnsConf;
            conf.mu.Unlock();

        });

        private static bool tryAcquireSema(this ptr<resolverConfig> _addr_conf)
        {
            ref resolverConfig conf = ref _addr_conf.val;

            return true;
            return false;
        }

        private static void releaseSema(this ptr<resolverConfig> _addr_conf)
        {
            ref resolverConfig conf = ref _addr_conf.val;

            conf.ch.Receive();
        }

        private static (dnsmessage.Parser, @string, error) lookup(this ptr<Resolver> _addr_r, context.Context ctx, @string name, dnsmessage.Type qtype)
        {
            dnsmessage.Parser _p0 = default;
            @string _p0 = default;
            error _p0 = default!;
            ref Resolver r = ref _addr_r.val;

            if (!isDomainName(name))
            { 
                // We used to use "invalid domain name" as the error,
                // but that is a detail of the specific lookup mechanism.
                // Other lookups might allow broader name syntax
                // (for example Multicast DNS allows UTF-8; see RFC 6762).
                // For consistency with libc resolvers, report no such host.
                return (new dnsmessage.Parser(), "", error.As(addr(new DNSError(Err:errNoSuchHost.Error(),Name:name,IsNotFound:true))!)!);

            }

            resolvConf.tryUpdate("/etc/resolv.conf");
            resolvConf.mu.RLock();
            var conf = resolvConf.dnsConfig;
            resolvConf.mu.RUnlock();
            dnsmessage.Parser p = default;            @string server = default;            error err = default!;
            foreach (var (_, fqdn) in conf.nameList(name))
            {
                p, server, err = r.tryOneName(ctx, conf, fqdn, qtype);
                if (err == null)
                {
                    break;
                }

                {
                    Error (nerr, ok) = err._<Error>();

                    if (ok && nerr.Temporary() && r.strictErrors())
                    { 
                        // If we hit a temporary error with StrictErrors enabled,
                        // stop immediately instead of trying more names.
                        break;

                    }

                }

            }
            if (err == null)
            {
                return (p, server, error.As(null!)!);
            }

            {
                error err__prev1 = err;

                ptr<DNSError> (err, ok) = err._<ptr<DNSError>>();

                if (ok)
                { 
                    // Show original name passed to lookup, not suffixed one.
                    // In general we might have tried many suffixes; showing
                    // just one is misleading. See also golang.org/issue/6324.
                    err.Name = name;

                }

                err = err__prev1;

            }

            return (new dnsmessage.Parser(), "", error.As(err)!);

        }

        // avoidDNS reports whether this is a hostname for which we should not
        // use DNS. Currently this includes only .onion, per RFC 7686. See
        // golang.org/issue/13705. Does not cover .local names (RFC 6762),
        // see golang.org/issue/16739.
        private static bool avoidDNS(@string name)
        {
            if (name == "")
            {
                return true;
            }

            if (name[len(name) - 1L] == '.')
            {
                name = name[..len(name) - 1L];
            }

            return stringsHasSuffixFold(name, ".onion");

        }

        // nameList returns a list of names for sequential DNS queries.
        private static slice<@string> nameList(this ptr<dnsConfig> _addr_conf, @string name)
        {
            ref dnsConfig conf = ref _addr_conf.val;

            if (avoidDNS(name))
            {
                return null;
            } 

            // Check name length (see isDomainName).
            var l = len(name);
            var rooted = l > 0L && name[l - 1L] == '.';
            if (l > 254L || l == 254L && rooted)
            {
                return null;
            } 

            // If name is rooted (trailing dot), try only that name.
            if (rooted)
            {
                return new slice<@string>(new @string[] { name });
            }

            var hasNdots = count(name, '.') >= conf.ndots;
            name += ".";
            l++; 

            // Build list of search choices.
            var names = make_slice<@string>(0L, 1L + len(conf.search)); 
            // If name has enough dots, try unsuffixed first.
            if (hasNdots)
            {
                names = append(names, name);
            } 
            // Try suffixes that are not too long (see isDomainName).
            foreach (var (_, suffix) in conf.search)
            {
                if (l + len(suffix) <= 254L)
                {
                    names = append(names, name + suffix);
                }

            } 
            // Try unsuffixed, if not tried first above.
            if (!hasNdots)
            {
                names = append(names, name);
            }

            return names;

        }

        // hostLookupOrder specifies the order of LookupHost lookup strategies.
        // It is basically a simplified representation of nsswitch.conf.
        // "files" means /etc/hosts.
        private partial struct hostLookupOrder // : long
        {
        }

 
        // hostLookupCgo means defer to cgo.
        private static readonly hostLookupOrder hostLookupCgo = (hostLookupOrder)iota;
        private static readonly var hostLookupFilesDNS = (var)0; // files first
        private static readonly var hostLookupDNSFiles = (var)1; // dns first
        private static readonly var hostLookupFiles = (var)2; // only files
        private static readonly var hostLookupDNS = (var)3; // only DNS

        private static map lookupOrderName = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<hostLookupOrder, @string>{hostLookupCgo:"cgo",hostLookupFilesDNS:"files,dns",hostLookupDNSFiles:"dns,files",hostLookupFiles:"files",hostLookupDNS:"dns",};

        private static @string String(this hostLookupOrder o)
        {
            {
                var (s, ok) = lookupOrderName[o];

                if (ok)
                {
                    return s;
                }

            }

            return "hostLookupOrder=" + itoa(int(o)) + "??";

        }

        // goLookupHost is the native Go implementation of LookupHost.
        // Used only if cgoLookupHost refuses to handle the request
        // (that is, only if cgoLookupHost is the stub in cgo_stub.go).
        // Normally we let cgo use the C library resolver instead of
        // depending on our lookup code, so that Go and C get the same
        // answers.
        private static (slice<@string>, error) goLookupHost(this ptr<Resolver> _addr_r, context.Context ctx, @string name)
        {
            slice<@string> addrs = default;
            error err = default!;
            ref Resolver r = ref _addr_r.val;

            return r.goLookupHostOrder(ctx, name, hostLookupFilesDNS);
        }

        private static (slice<@string>, error) goLookupHostOrder(this ptr<Resolver> _addr_r, context.Context ctx, @string name, hostLookupOrder order)
        {
            slice<@string> addrs = default;
            error err = default!;
            ref Resolver r = ref _addr_r.val;

            if (order == hostLookupFilesDNS || order == hostLookupFiles)
            { 
                // Use entries from /etc/hosts if they match.
                addrs = lookupStaticHost(name);
                if (len(addrs) > 0L || order == hostLookupFiles)
                {
                    return ;
                }

            }

            var (ips, _, err) = r.goLookupIPCNAMEOrder(ctx, name, order);
            if (err != null)
            {
                return ;
            }

            addrs = make_slice<@string>(0L, len(ips));
            foreach (var (_, ip) in ips)
            {
                addrs = append(addrs, ip.String());
            }
            return ;

        }

        // lookup entries from /etc/hosts
        private static slice<IPAddr> goLookupIPFiles(@string name)
        {
            slice<IPAddr> addrs = default;

            {
                var haddr__prev1 = haddr;

                foreach (var (_, __haddr) in lookupStaticHost(name))
                {
                    haddr = __haddr;
                    var (haddr, zone) = splitHostZone(haddr);
                    {
                        var ip = ParseIP(haddr);

                        if (ip != null)
                        {
                            IPAddr addr = new IPAddr(IP:ip,Zone:zone);
                            addrs = append(addrs, addr);
                        }

                    }

                }

                haddr = haddr__prev1;
            }

            sortByRFC6724(addrs);
            return ;

        }

        // goLookupIP is the native Go implementation of LookupIP.
        // The libc versions are in cgo_*.go.
        private static (slice<IPAddr>, error) goLookupIP(this ptr<Resolver> _addr_r, context.Context ctx, @string host)
        {
            slice<IPAddr> addrs = default;
            error err = default!;
            ref Resolver r = ref _addr_r.val;

            var order = systemConf().hostLookupOrder(r, host);
            addrs, _, err = r.goLookupIPCNAMEOrder(ctx, host, order);
            return ;
        }

        private static (slice<IPAddr>, dnsmessage.Name, error) goLookupIPCNAMEOrder(this ptr<Resolver> _addr_r, context.Context ctx, @string name, hostLookupOrder order) => func((defer, _, __) =>
        {
            slice<IPAddr> addrs = default;
            dnsmessage.Name cname = default;
            error err = default!;
            ref Resolver r = ref _addr_r.val;

            if (order == hostLookupFilesDNS || order == hostLookupFiles)
            {
                addrs = goLookupIPFiles(name);
                if (len(addrs) > 0L || order == hostLookupFiles)
                {
                    return (addrs, new dnsmessage.Name(), error.As(null!)!);
                }

            }

            if (!isDomainName(name))
            { 
                // See comment in func lookup above about use of errNoSuchHost.
                return (null, new dnsmessage.Name(), error.As(addr(new DNSError(Err:errNoSuchHost.Error(),Name:name,IsNotFound:true))!)!);

            }

            resolvConf.tryUpdate("/etc/resolv.conf");
            resolvConf.mu.RLock();
            var conf = resolvConf.dnsConfig;
            resolvConf.mu.RUnlock();
            private partial struct result : error
            {
                public dnsmessage.Parser p;
                public @string server;
                public error error;
            }
            var lane = make_channel<result>(1L);
            array<dnsmessage.Type> qtypes = new array<dnsmessage.Type>(new dnsmessage.Type[] { dnsmessage.TypeA, dnsmessage.TypeAAAA });
            Action<@string, dnsmessage.Type> queryFn = default;
            Func<@string, dnsmessage.Type, result> responseFn = default;
            if (conf.singleRequest)
            {
                queryFn = (fqdn, qtype) =>
                {
                }
            else
;
                responseFn = (fqdn, qtype) =>
                {
                    dnsWaitGroup.Add(1L);
                    defer(dnsWaitGroup.Done());
                    var (p, server, err) = r.tryOneName(ctx, conf, fqdn, qtype);
                    return new result(p,server,err);
                }
;

            }            {
                queryFn = (fqdn, qtype) =>
                {
                    dnsWaitGroup.Add(1L);
                    go_(() => qtype =>
                    {
                        (p, server, err) = r.tryOneName(ctx, conf, fqdn, qtype);
                        lane.Send(new result(p,server,err));
                        dnsWaitGroup.Done();
                    }(qtype));

                }
;
                responseFn = (fqdn, qtype) =>
                {
                    return lane.Receive();
                }
;

            }

            error lastErr = default!;
            foreach (var (_, fqdn) in conf.nameList(name))
            {
                {
                    var qtype__prev2 = qtype;

                    foreach (var (_, __qtype) in qtypes)
                    {
                        qtype = __qtype;
                        queryFn(fqdn, qtype);
                    }

                    qtype = qtype__prev2;
                }

                var hitStrictError = false;
                {
                    var qtype__prev2 = qtype;

                    foreach (var (_, __qtype) in qtypes)
                    {
                        qtype = __qtype;
                        var result = responseFn(fqdn, qtype);
                        if (result.error != null)
                        {
                            {
                                Error (nerr, ok) = result.error._<Error>();

                                if (ok && nerr.Temporary() && r.strictErrors())
                                { 
                                    // This error will abort the nameList loop.
                                    hitStrictError = true;
                                    lastErr = error.As(result.error)!;

                                }
                                else if (lastErr == null || fqdn == name + ".")
                                { 
                                    // Prefer error for original name.
                                    lastErr = error.As(result.error)!;

                                }


                            }

                            continue;

                        } 

                        // Presotto says it's okay to assume that servers listed in
                        // /etc/resolv.conf are recursive resolvers.
                        //
                        // We asked for recursion, so it should have included all the
                        // answers we need in this one packet.
                        //
                        // Further, RFC 1035 section 4.3.1 says that "the recursive
                        // response to a query will be... The answer to the query,
                        // possibly preface by one or more CNAME RRs that specify
                        // aliases encountered on the way to an answer."
                        //
                        // Therefore, we should be able to assume that we can ignore
                        // CNAMEs and that the A and AAAA records we requested are
                        // for the canonical name.
loop:
                        while (true)
                        {
                            var (h, err) = result.p.AnswerHeader();
                            if (err != null && err != dnsmessage.ErrSectionDone)
                            {
                                lastErr = error.As(addr(new DNSError(Err:"cannot marshal DNS message",Name:name,Server:result.server,)))!;
                            }

                            if (err != null)
                            {
                                break;
                            }


                            if (h.Type == dnsmessage.TypeA) 
                                var (a, err) = result.p.AResource();
                                if (err != null)
                                {
                                    lastErr = error.As(addr(new DNSError(Err:"cannot marshal DNS message",Name:name,Server:result.server,)))!;
                                    _breakloop = true;
                                    break;
                                }

                                addrs = append(addrs, new IPAddr(IP:IP(a.A[:])));
                            else if (h.Type == dnsmessage.TypeAAAA) 
                                var (aaaa, err) = result.p.AAAAResource();
                                if (err != null)
                                {
                                    lastErr = error.As(addr(new DNSError(Err:"cannot marshal DNS message",Name:name,Server:result.server,)))!;
                                    _breakloop = true;
                                    break;
                                }

                                addrs = append(addrs, new IPAddr(IP:IP(aaaa.AAAA[:])));
                            else 
                                {
                                    var err = result.p.SkipAnswer();

                                    if (err != null)
                                    {
                                        lastErr = error.As(addr(new DNSError(Err:"cannot marshal DNS message",Name:name,Server:result.server,)))!;
                                        _breakloop = true;
                                        break;
                                    }

                                }

                                continue;
                                                        if (cname.Length == 0L && h.Name.Length != 0L)
                            {
                                cname = h.Name;
                            }

                        }

                    }

                    qtype = qtype__prev2;
                }

                if (hitStrictError)
                { 
                    // If either family hit an error with StrictErrors enabled,
                    // discard all addresses. This ensures that network flakiness
                    // cannot turn a dualstack hostname IPv4/IPv6-only.
                    addrs = null;
                    break;

                }

                if (len(addrs) > 0L)
                {
                    break;
                }

            }
            {
                error lastErr__prev1 = lastErr;

                ptr<DNSError> (lastErr, ok) = lastErr._<ptr<DNSError>>();

                if (ok)
                { 
                    // Show original name passed to lookup, not suffixed one.
                    // In general we might have tried many suffixes; showing
                    // just one is misleading. See also golang.org/issue/6324.
                    lastErr.Name = name;

                }

                lastErr = lastErr__prev1;

            }

            sortByRFC6724(addrs);
            if (len(addrs) == 0L)
            {
                if (order == hostLookupDNSFiles)
                {
                    addrs = goLookupIPFiles(name);
                }

                if (len(addrs) == 0L && lastErr != null)
                {
                    return (null, new dnsmessage.Name(), error.As(lastErr)!);
                }

            }

            return (addrs, cname, error.As(null!)!);

        });

        // goLookupCNAME is the native Go (non-cgo) implementation of LookupCNAME.
        private static (@string, error) goLookupCNAME(this ptr<Resolver> _addr_r, context.Context ctx, @string host)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref Resolver r = ref _addr_r.val;

            var order = systemConf().hostLookupOrder(r, host);
            var (_, cname, err) = r.goLookupIPCNAMEOrder(ctx, host, order);
            return (cname.String(), error.As(err)!);
        }

        // goLookupPTR is the native Go implementation of LookupAddr.
        // Used only if cgoLookupPTR refuses to handle the request (that is,
        // only if cgoLookupPTR is the stub in cgo_stub.go).
        // Normally we let cgo use the C library resolver instead of depending
        // on our lookup code, so that Go and C get the same answers.
        private static (slice<@string>, error) goLookupPTR(this ptr<Resolver> _addr_r, context.Context ctx, @string addr)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;
            ref Resolver r = ref _addr_r.val;

            var names = lookupStaticAddr(addr);
            if (len(names) > 0L)
            {
                return (names, error.As(null!)!);
            }

            var (arpa, err) = reverseaddr(addr);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            var (p, server, err) = r.lookup(ctx, arpa, dnsmessage.TypePTR);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            slice<@string> ptrs = default;
            while (true)
            {
                var (h, err) = p.AnswerHeader();
                if (err == dnsmessage.ErrSectionDone)
                {
                    break;
                }

                if (err != null)
                {
                    return (null, error.As(addr(new DNSError(Err:"cannot marshal DNS message",Name:addr,Server:server,))!)!);
                }

                if (h.Type != dnsmessage.TypePTR)
                {
                    var err = p.SkipAnswer();
                    if (err != null)
                    {
                        return (null, error.As(addr(new DNSError(Err:"cannot marshal DNS message",Name:addr,Server:server,))!)!);
                    }

                    continue;

                }

                var (ptr, err) = p.PTRResource();
                if (err != null)
                {
                    return (null, error.As(addr(new DNSError(Err:"cannot marshal DNS message",Name:addr,Server:server,))!)!);
                }

                ptrs = append(ptrs, ptr.PTR.String());


            }

            return (ptrs, error.As(null!)!);

        }
    }
}
