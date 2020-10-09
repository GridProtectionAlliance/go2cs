// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// Read system DNS config from /etc/resolv.conf

// package net -- go2cs converted at 2020 October 09 04:50:51 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\dnsconfig_unix.go
using bytealg = go.@internal.bytealg_package;
using os = go.os_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static @string defaultNS = new slice<@string>(new @string[] { "127.0.0.1:53", "[::1]:53" });        private static var getHostname = os.Hostname;

        private partial struct dnsConfig
        {
            public slice<@string> servers; // server addresses (in host:port form) to use
            public slice<@string> search; // rooted suffixes to append to local name
            public long ndots; // number of dots in name to trigger absolute lookup
            public time.Duration timeout; // wait before giving up on a query, including retries
            public long attempts; // lost packets before giving up on server
            public bool rotate; // round robin among servers
            public bool unknownOpt; // anything unknown was encountered
            public slice<@string> lookup; // OpenBSD top-level database "lookup" order
            public error err; // any error that occurs during open of resolv.conf
            public time.Time mtime; // time of resolv.conf modification
            public uint soffset; // used by serverOffset
            public bool singleRequest; // use sequential A and AAAA queries instead of parallel queries
            public bool useTCP; // force usage of TCP for DNS resolutions
        }

        // See resolv.conf(5) on a Linux machine.
        private static ptr<dnsConfig> dnsReadConfig(@string filename) => func((defer, _, __) =>
        {
            ptr<dnsConfig> conf = addr(new dnsConfig(ndots:1,timeout:5*time.Second,attempts:2,));
            var (file, err) = open(filename);
            if (err != null)
            {
                conf.servers = defaultNS;
                conf.search = dnsDefaultSearch();
                conf.err = err;
                return _addr_conf!;
            }

            defer(file.close());
            {
                var (fi, err) = file.file.Stat();

                if (err == null)
                {
                    conf.mtime = fi.ModTime();
                }
                else
                {
                    conf.servers = defaultNS;
                    conf.search = dnsDefaultSearch();
                    conf.err = err;
                    return _addr_conf!;
                }

            }

            {
                var (line, ok) = file.readLine();

                while (ok)
                {
                    if (len(line) > 0L && (line[0L] == ';' || line[0L] == '#'))
                    { 
                        // comment.
                        continue;
                    line, ok = file.readLine();
                    }

                    var f = getFields(line);
                    if (len(f) < 1L)
                    {
                        continue;
                    }

                    switch (f[0L])
                    {
                        case "nameserver": // add one name server
                            if (len(f) > 1L && len(conf.servers) < 3L)
                            { // small, but the standard limit
                                // One more check: make sure server name is
                                // just an IP address. Otherwise we need DNS
                                // to look it up.
                                if (parseIPv4(f[1L]) != null)
                                {
                                    conf.servers = append(conf.servers, JoinHostPort(f[1L], "53"));
                                }                            {
                                    var (ip, _) = parseIPv6Zone(f[1L]);


                                    else if (ip != null)
                                    {
                                        conf.servers = append(conf.servers, JoinHostPort(f[1L], "53"));
                                    }

                                }

                            }

                            break;
                        case "domain": // set search path to just this domain
                            if (len(f) > 1L)
                            {
                                conf.search = new slice<@string>(new @string[] { ensureRooted(f[1]) });
                            }

                            break;
                        case "search": // set search path to given servers
                            conf.search = make_slice<@string>(len(f) - 1L);
                            for (long i = 0L; i < len(conf.search); i++)
                            {
                                conf.search[i] = ensureRooted(f[i + 1L]);
                            }

                            break;
                        case "options": // magic options
                            foreach (var (_, s) in f[1L..])
                            {

                                if (hasPrefix(s, "ndots:")) 
                                    var (n, _, _) = dtoi(s[6L..]);
                                    if (n < 0L)
                                    {
                                        n = 0L;
                                    }
                                    else if (n > 15L)
                                    {
                                        n = 15L;
                                    }

                                    conf.ndots = n;
                                else if (hasPrefix(s, "timeout:")) 
                                    (n, _, _) = dtoi(s[8L..]);
                                    if (n < 1L)
                                    {
                                        n = 1L;
                                    }

                                    conf.timeout = time.Duration(n) * time.Second;
                                else if (hasPrefix(s, "attempts:")) 
                                    (n, _, _) = dtoi(s[9L..]);
                                    if (n < 1L)
                                    {
                                        n = 1L;
                                    }

                                    conf.attempts = n;
                                else if (s == "rotate") 
                                    conf.rotate = true;
                                else if (s == "single-request" || s == "single-request-reopen") 
                                    // Linux option:
                                    // http://man7.org/linux/man-pages/man5/resolv.conf.5.html
                                    // "By default, glibc performs IPv4 and IPv6 lookups in parallel [...]
                                    //  This option disables the behavior and makes glibc
                                    //  perform the IPv6 and IPv4 requests sequentially."
                                    conf.singleRequest = true;
                                else if (s == "use-vc" || s == "usevc" || s == "tcp") 
                                    // Linux (use-vc), FreeBSD (usevc) and OpenBSD (tcp) option:
                                    // http://man7.org/linux/man-pages/man5/resolv.conf.5.html
                                    // "Sets RES_USEVC in _res.options.
                                    //  This option forces the use of TCP for DNS resolutions."
                                    // https://www.freebsd.org/cgi/man.cgi?query=resolv.conf&sektion=5&manpath=freebsd-release-ports
                                    // https://man.openbsd.org/resolv.conf.5
                                    conf.useTCP = true;
                                else 
                                    conf.unknownOpt = true;

                            }
                            break;
                        case "lookup": 
                            // OpenBSD option:
                            // https://www.openbsd.org/cgi-bin/man.cgi/OpenBSD-current/man5/resolv.conf.5
                            // "the legal space-separated values are: bind, file, yp"
                            conf.lookup = f[1L..];
                            break;
                        default: 
                            conf.unknownOpt = true;
                            break;
                    }

                }

            }
            if (len(conf.servers) == 0L)
            {
                conf.servers = defaultNS;
            }

            if (len(conf.search) == 0L)
            {
                conf.search = dnsDefaultSearch();
            }

            return _addr_conf!;

        });

        // serverOffset returns an offset that can be used to determine
        // indices of servers in c.servers when making queries.
        // When the rotate option is enabled, this offset increases.
        // Otherwise it is always 0.
        private static uint serverOffset(this ptr<dnsConfig> _addr_c)
        {
            ref dnsConfig c = ref _addr_c.val;

            if (c.rotate)
            {
                return atomic.AddUint32(_addr_c.soffset, 1L) - 1L; // return 0 to start
            }

            return 0L;

        }

        private static slice<@string> dnsDefaultSearch()
        {
            var (hn, err) = getHostname();
            if (err != null)
            { 
                // best effort
                return null;

            }

            {
                var i = bytealg.IndexByteString(hn, '.');

                if (i >= 0L && i < len(hn) - 1L)
                {
                    return new slice<@string>(new @string[] { ensureRooted(hn[i+1:]) });
                }

            }

            return null;

        }

        private static bool hasPrefix(@string s, @string prefix)
        {
            return len(s) >= len(prefix) && s[..len(prefix)] == prefix;
        }

        private static @string ensureRooted(@string s)
        {
            if (len(s) > 0L && s[len(s) - 1L] == '.')
            {
                return s;
            }

            return s + ".";

        }
    }
}
