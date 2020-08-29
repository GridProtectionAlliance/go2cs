// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd solaris

// Minimal RFC 6724 address selection.

// package net -- go2cs converted at 2020 August 29 08:25:06 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\addrselect.go
using sort = go.sort_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static void sortByRFC6724(slice<IPAddr> addrs)
        {
            if (len(addrs) < 2L)
            {
                return;
            }
            sortByRFC6724withSrcs(addrs, srcAddrs(addrs));
        }

        private static void sortByRFC6724withSrcs(slice<IPAddr> addrs, slice<IP> srcs) => func((_, panic, __) =>
        {
            if (len(addrs) != len(srcs))
            {
                panic("internal error");
            }
            var addrAttr = make_slice<ipAttr>(len(addrs));
            var srcAttr = make_slice<ipAttr>(len(srcs));
            foreach (var (i, v) in addrs)
            {
                addrAttr[i] = ipAttrOf(v.IP);
                srcAttr[i] = ipAttrOf(srcs[i]);
            }
            sort.Stable(ref new byRFC6724(addrs:addrs,addrAttr:addrAttr,srcs:srcs,srcAttr:srcAttr,));
        });

        // srcsAddrs tries to UDP-connect to each address to see if it has a
        // route. (This doesn't send any packets). The destination port
        // number is irrelevant.
        private static slice<IP> srcAddrs(slice<IPAddr> addrs)
        {
            var srcs = make_slice<IP>(len(addrs));
            UDPAddr dst = new UDPAddr(Port:9);
            foreach (var (i) in addrs)
            {
                dst.IP = addrs[i].IP;
                dst.Zone = addrs[i].Zone;
                var (c, err) = DialUDP("udp", null, ref dst);
                if (err == null)
                {
                    {
                        ref UDPAddr (src, ok) = c.LocalAddr()._<ref UDPAddr>();

                        if (ok)
                        {
                            srcs[i] = src.IP;
                        }

                    }
                    c.Close();
                }
            }
            return srcs;
        }

        private partial struct ipAttr
        {
            public scope Scope;
            public byte Precedence;
            public byte Label;
        }

        private static ipAttr ipAttrOf(IP ip)
        {
            if (ip == null)
            {
                return new ipAttr();
            }
            var match = rfc6724policyTable.Classify(ip);
            return new ipAttr(Scope:classifyScope(ip),Precedence:match.Precedence,Label:match.Label,);
        }

        private partial struct byRFC6724
        {
            public slice<IPAddr> addrs; // addrs to sort
            public slice<ipAttr> addrAttr;
            public slice<IP> srcs; // or nil if unreachable
            public slice<ipAttr> srcAttr;
        }

        private static long Len(this ref byRFC6724 s)
        {
            return len(s.addrs);
        }

        private static void Swap(this ref byRFC6724 s, long i, long j)
        {
            s.addrs[i] = s.addrs[j];
            s.addrs[j] = s.addrs[i];
            s.srcs[i] = s.srcs[j];
            s.srcs[j] = s.srcs[i];
            s.addrAttr[i] = s.addrAttr[j];
            s.addrAttr[j] = s.addrAttr[i];
            s.srcAttr[i] = s.srcAttr[j];
            s.srcAttr[j] = s.srcAttr[i];
        }

        // Less reports whether i is a better destination address for this
        // host than j.
        //
        // The algorithm and variable names comes from RFC 6724 section 6.
        private static bool Less(this ref byRFC6724 s, long i, long j)
        {
            var DA = s.addrs[i].IP;
            var DB = s.addrs[j].IP;
            var SourceDA = s.srcs[i];
            var SourceDB = s.srcs[j];
            var attrDA = ref s.addrAttr[i];
            var attrDB = ref s.addrAttr[j];
            var attrSourceDA = ref s.srcAttr[i];
            var attrSourceDB = ref s.srcAttr[j];

            const var preferDA = true;

            const var preferDB = false; 

            // Rule 1: Avoid unusable destinations.
            // If DB is known to be unreachable or if Source(DB) is undefined, then
            // prefer DA.  Similarly, if DA is known to be unreachable or if
            // Source(DA) is undefined, then prefer DB.
 

            // Rule 1: Avoid unusable destinations.
            // If DB is known to be unreachable or if Source(DB) is undefined, then
            // prefer DA.  Similarly, if DA is known to be unreachable or if
            // Source(DA) is undefined, then prefer DB.
            if (SourceDA == null && SourceDB == null)
            {
                return false; // "equal"
            }
            if (SourceDB == null)
            {
                return preferDA;
            }
            if (SourceDA == null)
            {
                return preferDB;
            } 

            // Rule 2: Prefer matching scope.
            // If Scope(DA) = Scope(Source(DA)) and Scope(DB) <> Scope(Source(DB)),
            // then prefer DA.  Similarly, if Scope(DA) <> Scope(Source(DA)) and
            // Scope(DB) = Scope(Source(DB)), then prefer DB.
            if (attrDA.Scope == attrSourceDA.Scope && attrDB.Scope != attrSourceDB.Scope)
            {
                return preferDA;
            }
            if (attrDA.Scope != attrSourceDA.Scope && attrDB.Scope == attrSourceDB.Scope)
            {
                return preferDB;
            } 

            // Rule 3: Avoid deprecated addresses.
            // If Source(DA) is deprecated and Source(DB) is not, then prefer DB.
            // Similarly, if Source(DA) is not deprecated and Source(DB) is
            // deprecated, then prefer DA.

            // TODO(bradfitz): implement? low priority for now.

            // Rule 4: Prefer home addresses.
            // If Source(DA) is simultaneously a home address and care-of address
            // and Source(DB) is not, then prefer DA.  Similarly, if Source(DB) is
            // simultaneously a home address and care-of address and Source(DA) is
            // not, then prefer DB.

            // TODO(bradfitz): implement? low priority for now.

            // Rule 5: Prefer matching label.
            // If Label(Source(DA)) = Label(DA) and Label(Source(DB)) <> Label(DB),
            // then prefer DA.  Similarly, if Label(Source(DA)) <> Label(DA) and
            // Label(Source(DB)) = Label(DB), then prefer DB.
            if (attrSourceDA.Label == attrDA.Label && attrSourceDB.Label != attrDB.Label)
            {
                return preferDA;
            }
            if (attrSourceDA.Label != attrDA.Label && attrSourceDB.Label == attrDB.Label)
            {
                return preferDB;
            } 

            // Rule 6: Prefer higher precedence.
            // If Precedence(DA) > Precedence(DB), then prefer DA.  Similarly, if
            // Precedence(DA) < Precedence(DB), then prefer DB.
            if (attrDA.Precedence > attrDB.Precedence)
            {
                return preferDA;
            }
            if (attrDA.Precedence < attrDB.Precedence)
            {
                return preferDB;
            } 

            // Rule 7: Prefer native transport.
            // If DA is reached via an encapsulating transition mechanism (e.g.,
            // IPv6 in IPv4) and DB is not, then prefer DB.  Similarly, if DB is
            // reached via encapsulation and DA is not, then prefer DA.

            // TODO(bradfitz): implement? low priority for now.

            // Rule 8: Prefer smaller scope.
            // If Scope(DA) < Scope(DB), then prefer DA.  Similarly, if Scope(DA) >
            // Scope(DB), then prefer DB.
            if (attrDA.Scope < attrDB.Scope)
            {
                return preferDA;
            }
            if (attrDA.Scope > attrDB.Scope)
            {
                return preferDB;
            } 

            // Rule 9: Use longest matching prefix.
            // When DA and DB belong to the same address family (both are IPv6 or
            // both are IPv4 [but see below]): If CommonPrefixLen(Source(DA), DA) >
            // CommonPrefixLen(Source(DB), DB), then prefer DA.  Similarly, if
            // CommonPrefixLen(Source(DA), DA) < CommonPrefixLen(Source(DB), DB),
            // then prefer DB.
            //
            // However, applying this rule to IPv4 addresses causes
            // problems (see issues 13283 and 18518), so limit to IPv6.
            if (DA.To4() == null && DB.To4() == null)
            {
                var commonA = commonPrefixLen(SourceDA, DA);
                var commonB = commonPrefixLen(SourceDB, DB);

                if (commonA > commonB)
                {
                    return preferDA;
                }
                if (commonA < commonB)
                {
                    return preferDB;
                }
            } 

            // Rule 10: Otherwise, leave the order unchanged.
            // If DA preceded DB in the original list, prefer DA.
            // Otherwise, prefer DB.
            return false; // "equal"
        }

        private partial struct policyTableEntry
        {
            public ptr<IPNet> Prefix;
            public byte Precedence;
            public byte Label;
        }

        private partial struct policyTable // : slice<policyTableEntry>
        {
        }

        // RFC 6724 section 2.1.
        private static policyTable rfc6724policyTable = new policyTable({Prefix:mustCIDR("::1/128"),Precedence:50,Label:0,},{Prefix:mustCIDR("::/0"),Precedence:40,Label:1,},{Prefix:mustCIDR("::ffff:0:0/96"),Precedence:35,Label:4,},{Prefix:mustCIDR("2002::/16"),Precedence:30,Label:2,},{Prefix:mustCIDR("2001::/32"),Precedence:5,Label:5,},{Prefix:mustCIDR("fc00::/7"),Precedence:3,Label:13,},{Prefix:mustCIDR("::/96"),Precedence:1,Label:3,},{Prefix:mustCIDR("fec0::/10"),Precedence:1,Label:11,},{Prefix:mustCIDR("3ffe::/16"),Precedence:1,Label:12,},);

        private static void init()
        {
            sort.Sort(sort.Reverse(byMaskLength(rfc6724policyTable)));
        }

        // byMaskLength sorts policyTableEntry by the size of their Prefix.Mask.Size,
        // from smallest mask, to largest.
        private partial struct byMaskLength // : slice<policyTableEntry>
        {
        }

        private static long Len(this byMaskLength s)
        {
            return len(s);
        }
        private static void Swap(this byMaskLength s, long i, long j)
        {
            s[i] = s[j];
            s[j] = s[i];

        }
        private static bool Less(this byMaskLength s, long i, long j)
        {
            var (isize, _) = s[i].Prefix.Mask.Size();
            var (jsize, _) = s[j].Prefix.Mask.Size();
            return isize < jsize;
        }

        // mustCIDR calls ParseCIDR and panics on any error, or if the network
        // is not IPv6.
        private static ref IPNet mustCIDR(@string s) => func((_, panic, __) =>
        {
            var (ip, ipNet, err) = ParseCIDR(s);
            if (err != null)
            {
                panic(err.Error());
            }
            if (len(ip) != IPv6len)
            {
                panic("unexpected IP length");
            }
            return ipNet;
        });

        // Classify returns the policyTableEntry of the entry with the longest
        // matching prefix that contains ip.
        // The table t must be sorted from largest mask size to smallest.
        private static policyTableEntry Classify(this policyTable t, IP ip)
        {
            foreach (var (_, ent) in t)
            {
                if (ent.Prefix.Contains(ip))
                {
                    return ent;
                }
            }
            return new policyTableEntry();
        }

        // RFC 6724 section 3.1.
        private partial struct scope // : byte
        {
        }

        private static readonly scope scopeInterfaceLocal = 0x1UL;
        private static readonly scope scopeLinkLocal = 0x2UL;
        private static readonly scope scopeAdminLocal = 0x4UL;
        private static readonly scope scopeSiteLocal = 0x5UL;
        private static readonly scope scopeOrgLocal = 0x8UL;
        private static readonly scope scopeGlobal = 0xeUL;

        private static scope classifyScope(IP ip)
        {
            if (ip.IsLoopback() || ip.IsLinkLocalUnicast())
            {
                return scopeLinkLocal;
            }
            var ipv6 = len(ip) == IPv6len && ip.To4() == null;
            if (ipv6 && ip.IsMulticast())
            {
                return scope(ip[1L] & 0xfUL);
            } 
            // Site-local addresses are defined in RFC 3513 section 2.5.6
            // (and deprecated in RFC 3879).
            if (ipv6 && ip[0L] == 0xfeUL && ip[1L] & 0xc0UL == 0xc0UL)
            {
                return scopeSiteLocal;
            }
            return scopeGlobal;
        }

        // commonPrefixLen reports the length of the longest prefix (looking
        // at the most significant, or leftmost, bits) that the
        // two addresses have in common, up to the length of a's prefix (i.e.,
        // the portion of the address not including the interface ID).
        //
        // If a or b is an IPv4 address as an IPv6 address, the IPv4 addresses
        // are compared (with max common prefix length of 32).
        // If a and b are different IP versions, 0 is returned.
        //
        // See https://tools.ietf.org/html/rfc6724#section-2.2
        private static long commonPrefixLen(IP a, IP b)
        {
            {
                var a4 = a.To4();

                if (a4 != null)
                {
                    a = a4;
                }

            }
            {
                var b4 = b.To4();

                if (b4 != null)
                {
                    b = b4;
                }

            }
            if (len(a) != len(b))
            {
                return 0L;
            } 
            // If IPv6, only up to the prefix (first 64 bits)
            if (len(a) > 8L)
            {
                a = a[..8L];
                b = b[..8L];
            }
            while (len(a) > 0L)
            {
                if (a[0L] == b[0L])
                {
                    cpl += 8L;
                    a = a[1L..];
                    b = b[1L..];
                    continue;
                }
                long bits = 8L;
                var ab = a[0L];
                var bb = b[0L];
                while (true)
                {
                    ab >>= 1L;
                    bb >>= 1L;
                    bits--;
                    if (ab == bb)
                    {
                        cpl += bits;
                        return;
                    }
                }

            }

            return;
        }
    }
}
