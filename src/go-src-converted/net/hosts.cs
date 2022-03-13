// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 13 05:29:47 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\hosts.go
namespace go;

using bytealg = @internal.bytealg_package;
using sync = sync_package;
using time = time_package;

public static partial class net_package {

private static readonly nint cacheMaxAge = 5 * time.Second;



private static @string parseLiteralIP(@string addr) {
    IP ip = default;
    @string zone = default;
    ip = parseIPv4(addr);
    if (ip == null) {
        ip, zone = parseIPv6Zone(addr);
    }
    if (ip == null) {
        return "";
    }
    if (zone == "") {
        return ip.String();
    }
    return ip.String() + "%" + zone;
}

// hosts contains known host entries.
private static var hosts = default;

private static void readHosts() {
    var now = time.Now();
    var hp = testHookHostsPath;

    if (now.Before(hosts.expire) && hosts.path == hp && len(hosts.byName) > 0) {
        return ;
    }
    var (mtime, size, err) = stat(hp);
    if (err == null && hosts.path == hp && hosts.mtime.Equal(mtime) && hosts.size == size) {
        hosts.expire = now.Add(cacheMaxAge);
        return ;
    }
    var hs = make_map<@string, slice<@string>>();
    var @is = make_map<@string, slice<@string>>();
    ptr<file> file;
    file, _ = open(hp);

    if (file == null) {
        return ;
    }
    {
        var (line, ok) = file.readLine();

        while (ok) {
            {
                var i__prev1 = i;

                var i = bytealg.IndexByteString(line, '#');

                if (i >= 0) { 
                    // Discard comments.
                    line = line[(int)0..(int)i];
            line, ok = file.readLine();
                }

                i = i__prev1;

            }
            var f = getFields(line);
            if (len(f) < 2) {
                continue;
            }
            var addr = parseLiteralIP(f[0]);
            if (addr == "") {
                continue;
            }
            {
                var i__prev2 = i;

                for (i = 1; i < len(f); i++) {
                    var name = absDomainName((slice<byte>)f[i]);
                    slice<byte> h = (slice<byte>)f[i];
                    lowerASCIIBytes(h);
                    var key = absDomainName(h);
                    hs[key] = append(hs[key], addr);
                    is[addr] = append(is[addr], name);
                }


                i = i__prev2;
            }
        }
    } 
    // Update the data cache.
    hosts.expire = now.Add(cacheMaxAge);
    hosts.path = hp;
    hosts.byName = hs;
    hosts.byAddr = is;
    hosts.mtime = mtime;
    hosts.size = size;
    file.close();
}

// lookupStaticHost looks up the addresses for the given host from /etc/hosts.
private static slice<@string> lookupStaticHost(@string host) => func((defer, _, _) => {
    hosts.Lock();
    defer(hosts.Unlock());
    readHosts();
    if (len(hosts.byName) != 0) { 
        // TODO(jbd,bradfitz): avoid this alloc if host is already all lowercase?
        // or linear scan the byName map if it's small enough?
        slice<byte> lowerHost = (slice<byte>)host;
        lowerASCIIBytes(lowerHost);
        {
            var (ips, ok) = hosts.byName[absDomainName(lowerHost)];

            if (ok) {
                var ipsCp = make_slice<@string>(len(ips));
                copy(ipsCp, ips);
                return ipsCp;
            }

        }
    }
    return null;
});

// lookupStaticAddr looks up the hosts for the given address from /etc/hosts.
private static slice<@string> lookupStaticAddr(@string addr) => func((defer, _, _) => {
    hosts.Lock();
    defer(hosts.Unlock());
    readHosts();
    addr = parseLiteralIP(addr);
    if (addr == "") {
        return null;
    }
    if (len(hosts.byAddr) != 0) {
        {
            var (hosts, ok) = hosts.byAddr[addr];

            if (ok) {
                var hostsCp = make_slice<@string>(len(hosts));
                copy(hostsCp, hosts);
                return hostsCp;
            }

        }
    }
    return null;
});

} // end net_package
