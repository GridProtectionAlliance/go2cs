// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// Read system DNS config from /etc/resolv.conf

// package net -- go2cs converted at 2022 March 13 05:29:44 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\dnsconfig_unix.go
namespace go;

using bytealg = @internal.bytealg_package;
using os = os_package;
using atomic = sync.atomic_package;
using time = time_package;

public static partial class net_package {

private static @string defaultNS = new slice<@string>(new @string[] { "127.0.0.1:53", "[::1]:53" });private static var getHostname = os.Hostname;

private partial struct dnsConfig {
    public slice<@string> servers; // server addresses (in host:port form) to use
    public slice<@string> search; // rooted suffixes to append to local name
    public nint ndots; // number of dots in name to trigger absolute lookup
    public time.Duration timeout; // wait before giving up on a query, including retries
    public nint attempts; // lost packets before giving up on server
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
private static ptr<dnsConfig> dnsReadConfig(@string filename) => func((defer, _, _) => {
    ptr<dnsConfig> conf = addr(new dnsConfig(ndots:1,timeout:5*time.Second,attempts:2,));
    var (file, err) = open(filename);
    if (err != null) {
        conf.servers = defaultNS;
        conf.search = dnsDefaultSearch();
        conf.err = err;
        return _addr_conf!;
    }
    defer(file.close());
    {
        var (fi, err) = file.file.Stat();

        if (err == null) {
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

        while (ok) {
            if (len(line) > 0 && (line[0] == ';' || line[0] == '#')) { 
                // comment.
                continue;
            line, ok = file.readLine();
            }
            var f = getFields(line);
            if (len(f) < 1) {
                continue;
            }
            switch (f[0]) {
                case "nameserver": // add one name server
                    if (len(f) > 1 && len(conf.servers) < 3) { // small, but the standard limit
                        // One more check: make sure server name is
                        // just an IP address. Otherwise we need DNS
                        // to look it up.
                        if (parseIPv4(f[1]) != null) {
                            conf.servers = append(conf.servers, JoinHostPort(f[1], "53"));
                        }                    {
                            var (ip, _) = parseIPv6Zone(f[1]);


                            else if (ip != null) {
                                conf.servers = append(conf.servers, JoinHostPort(f[1], "53"));
                            }

                        }
                    }
                    break;
                case "domain": // set search path to just this domain
                    if (len(f) > 1) {
                        conf.search = new slice<@string>(new @string[] { ensureRooted(f[1]) });
                    }
                    break;
                case "search": // set search path to given servers
                    conf.search = make_slice<@string>(len(f) - 1);
                    for (nint i = 0; i < len(conf.search); i++) {
                        conf.search[i] = ensureRooted(f[i + 1]);
                    }

                    break;
                case "options": // magic options
                    foreach (var (_, s) in f[(int)1..]) {

                        if (hasPrefix(s, "ndots:")) 
                            var (n, _, _) = dtoi(s[(int)6..]);
                            if (n < 0) {
                                n = 0;
                            }
                            else if (n > 15) {
                                n = 15;
                            }
                            conf.ndots = n;
                        else if (hasPrefix(s, "timeout:")) 
                            (n, _, _) = dtoi(s[(int)8..]);
                            if (n < 1) {
                                n = 1;
                            }
                            conf.timeout = time.Duration(n) * time.Second;
                        else if (hasPrefix(s, "attempts:")) 
                            (n, _, _) = dtoi(s[(int)9..]);
                            if (n < 1) {
                                n = 1;
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
                    conf.lookup = f[(int)1..];
                    break;
                default: 
                    conf.unknownOpt = true;
                    break;
            }
        }
    }
    if (len(conf.servers) == 0) {
        conf.servers = defaultNS;
    }
    if (len(conf.search) == 0) {
        conf.search = dnsDefaultSearch();
    }
    return _addr_conf!;
});

// serverOffset returns an offset that can be used to determine
// indices of servers in c.servers when making queries.
// When the rotate option is enabled, this offset increases.
// Otherwise it is always 0.
private static uint serverOffset(this ptr<dnsConfig> _addr_c) {
    ref dnsConfig c = ref _addr_c.val;

    if (c.rotate) {
        return atomic.AddUint32(_addr_c.soffset, 1) - 1; // return 0 to start
    }
    return 0;
}

private static slice<@string> dnsDefaultSearch() {
    var (hn, err) = getHostname();
    if (err != null) { 
        // best effort
        return null;
    }
    {
        var i = bytealg.IndexByteString(hn, '.');

        if (i >= 0 && i < len(hn) - 1) {
            return new slice<@string>(new @string[] { ensureRooted(hn[i+1:]) });
        }
    }
    return null;
}

private static bool hasPrefix(@string s, @string prefix) {
    return len(s) >= len(prefix) && s[..(int)len(prefix)] == prefix;
}

private static @string ensureRooted(@string s) {
    if (len(s) > 0 && s[len(s) - 1] == '.') {
        return s;
    }
    return s + ".";
}

} // end net_package
