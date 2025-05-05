// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using bytealg = @internal.bytealg_package;
using fs = io.fs_package;
using netip = net.netip_package;
using sync = sync_package;
using time = time_package;
using @internal;
using io;
using net;

partial class net_package {

internal static readonly time.Duration cacheMaxAge = /* 5 * time.Second */ 5000000000;

internal static @string parseLiteralIP(@string addr) {
    var (ip, err) = netip.ParseAddr(addr);
    if (err != default!) {
        return ""u8;
    }
    return ip.String();
}

[GoType] partial struct byName {
    internal slice<@string> addrs;
    internal @string canonicalName;
}

// hosts contains known host entries.

[GoType("dyn")] partial struct hostsᴛ1 {
    public partial ref sync_package.Mutex Mutex { get; }
    // Key for the list of literal IP addresses must be a host
    // name. It would be part of DNS labels, a FQDN or an absolute
    // FQDN.
    // For now the key is converted to lower case for convenience.
    internal map<@string, byName> byName;
    // Key for the list of host names must be a literal IP address
    // including IPv6 address with zone identifier.
    // We don't support old-classful IP address notation.
    internal map<@string, slice<@string>> byAddr;
    internal time_package.Time expire;
    internal @string path;
    internal time_package.Time mtime;
    internal int64 size;
}
internal static hostsᴛ1 hosts;

internal static void readHosts() => func((defer, _) => {
    var now = time.Now();
    @string hp = hostsFilePath;
    if (now.Before(hosts.expire) && hosts.path == hp && len(hosts.byName) > 0) {
        return;
    }
    var (mtime, size, err) = stat(hp);
    if (err == default! && hosts.path == hp && hosts.mtime.Equal(mtime) && hosts.size == size) {
        hosts.expire = now.Add(cacheMaxAge);
        return;
    }
    var hs = new map<@string, byName>();
    var @is = new map<@string, slice<@string>>();
    (Δfile, err) = open(hp);
    if (err != default!) {
        if (!errors.Is(err, fs.ErrNotExist) && !errors.Is(err, fs.ErrPermission)) {
            return;
        }
    }
    if (Δfile != nil) {
        var fileʗ1 = Δfile;
        defer(fileʗ1.close);
        for (var (line, ok) = Δfile.readLine(); ok; (line, ok) = Δfile.readLine()) {
            {
                nint i = bytealg.IndexByteString(line, (rune)'#'); if (i >= 0) {
                    // Discard comments.
                    line = line[0..(int)(i)];
                }
            }
            var f = getFields(line);
            if (len(f) < 2) {
                continue;
            }
            @string addr = parseLiteralIP(f[0]);
            if (addr == ""u8) {
                continue;
            }
            @string canonical = default!;
            for (nint i = 1; i < len(f); i++) {
                @string name = absDomainName(f[i]);
                var h = slice<byte>(f[i]);
                lowerASCIIBytes(h);
                @string key = absDomainName(((@string)h));
                if (i == 1) {
                    canonical = key;
                }
                @is[addr] = append(@is[addr], name);
                {
                    var (v, okΔ1) = hs[key]; if (okΔ1) {
                        hs[key] = new byName(
                            addrs: append(v.addrs, addr),
                            canonicalName: v.canonicalName
                        );
                        continue;
                    }
                }
                hs[key] = new byName(
                    addrs: new @string[]{addr}.slice(),
                    canonicalName: canonical
                );
            }
        }
    }
    // Update the data cache.
    hosts.expire = now.Add(cacheMaxAge);
    hosts.path = hp;
    hosts.byName = hs;
    hosts.byAddr = @is;
    hosts.mtime = mtime;
    hosts.size = size;
});

// lookupStaticHost looks up the addresses and the canonical name for the given host from /etc/hosts.
internal static (slice<@string>, @string) lookupStaticHost(@string host) => func((defer, _) => {
    hosts.Lock();
    var hostsʗ1 = hosts;
    defer(hostsʗ1.Unlock);
    readHosts();
    if (len(hosts.byName) != 0) {
        if (hasUpperCase(host)) {
            var lowerHost = slice<byte>(host);
            lowerASCIIBytes(lowerHost);
            host = ((@string)lowerHost);
        }
        {
            var (byName, ok) = hosts.byName[absDomainName(host)]; if (ok) {
                var ipsCp = new slice<@string>(len(byName.addrs));
                copy(ipsCp, byName.addrs);
                return (ipsCp, byName.canonicalName);
            }
        }
    }
    return (default!, "");
});

// lookupStaticAddr looks up the hosts for the given address from /etc/hosts.
internal static slice<@string> lookupStaticAddr(@string addr) => func((defer, _) => {
    hosts.Lock();
    var hostsʗ1 = hosts;
    defer(hostsʗ1.Unlock);
    readHosts();
    addr = parseLiteralIP(addr);
    if (addr == ""u8) {
        return default!;
    }
    if (len(hosts.byAddr) != 0) {
        {
            var hosts = hosts.byAddr[addr];
            var ok = hosts.byAddr[addr]; if (ok) {
                var hostsCp = new slice<@string>(len(hosts));
                copy(hostsCp, hosts);
                return hostsCp;
            }
        }
    }
    return default!;
});

} // end net_package
