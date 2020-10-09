// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 09 04:51:32 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\hosts.go
using bytealg = go.@internal.bytealg_package;
using sync = go.sync_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static readonly long cacheMaxAge = (long)5L * time.Second;



        private static @string parseLiteralIP(@string addr)
        {
            IP ip = default;
            @string zone = default;
            ip = parseIPv4(addr);
            if (ip == null)
            {
                ip, zone = parseIPv6Zone(addr);
            }

            if (ip == null)
            {
                return "";
            }

            if (zone == "")
            {
                return ip.String();
            }

            return ip.String() + "%" + zone;

        }

        // hosts contains known host entries.
        private static var hosts = default;

        private static void readHosts()
        {
            var now = time.Now();
            var hp = testHookHostsPath;

            if (now.Before(hosts.expire) && hosts.path == hp && len(hosts.byName) > 0L)
            {
                return ;
            }

            var (mtime, size, err) = stat(hp);
            if (err == null && hosts.path == hp && hosts.mtime.Equal(mtime) && hosts.size == size)
            {
                hosts.expire = now.Add(cacheMaxAge);
                return ;
            }

            var hs = make_map<@string, slice<@string>>();
            var @is = make_map<@string, slice<@string>>();
            ptr<file> file;
            file, _ = open(hp);

            if (file == null)
            {
                return ;
            }

            {
                var (line, ok) = file.readLine();

                while (ok)
                {
                    {
                        var i__prev1 = i;

                        var i = bytealg.IndexByteString(line, '#');

                        if (i >= 0L)
                        { 
                            // Discard comments.
                            line = line[0L..i];
                    line, ok = file.readLine();
                        }

                        i = i__prev1;

                    }

                    var f = getFields(line);
                    if (len(f) < 2L)
                    {
                        continue;
                    }

                    var addr = parseLiteralIP(f[0L]);
                    if (addr == "")
                    {
                        continue;
                    }

                    {
                        var i__prev2 = i;

                        for (i = 1L; i < len(f); i++)
                        {
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
                // Update the data cache.

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
        private static slice<@string> lookupStaticHost(@string host) => func((defer, _, __) =>
        {
            hosts.Lock();
            defer(hosts.Unlock());
            readHosts();
            if (len(hosts.byName) != 0L)
            { 
                // TODO(jbd,bradfitz): avoid this alloc if host is already all lowercase?
                // or linear scan the byName map if it's small enough?
                slice<byte> lowerHost = (slice<byte>)host;
                lowerASCIIBytes(lowerHost);
                {
                    var (ips, ok) = hosts.byName[absDomainName(lowerHost)];

                    if (ok)
                    {
                        var ipsCp = make_slice<@string>(len(ips));
                        copy(ipsCp, ips);
                        return ipsCp;
                    }

                }

            }

            return null;

        });

        // lookupStaticAddr looks up the hosts for the given address from /etc/hosts.
        private static slice<@string> lookupStaticAddr(@string addr) => func((defer, _, __) =>
        {
            hosts.Lock();
            defer(hosts.Unlock());
            readHosts();
            addr = parseLiteralIP(addr);
            if (addr == "")
            {
                return null;
            }

            if (len(hosts.byAddr) != 0L)
            {
                {
                    var (hosts, ok) = hosts.byAddr[addr];

                    if (ok)
                    {
                        var hostsCp = make_slice<@string>(len(hosts));
                        copy(hostsCp, hosts);
                        return hostsCp;
                    }

                }

            }

            return null;

        });
    }
}
