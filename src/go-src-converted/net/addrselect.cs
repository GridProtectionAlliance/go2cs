// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Minimal RFC 6724 address selection.
namespace go;

using netip = net.netip_package;
using sort = sort_package;
using net;

partial class net_package {

internal static void sortByRFC6724(slice<IPAddr> addrs) {
    if (len(addrs) < 2) {
        return;
    }
    sortByRFC6724withSrcs(addrs, srcAddrs(addrs));
}

internal static void sortByRFC6724withSrcs(slice<IPAddr> addrs, slice<netipꓸAddr> srcs) {
    if (len(addrs) != len(srcs)) {
        throw panic("internal error");
    }
    var addrAttr = new slice<ipAttr>(len(addrs));
    var srcAttr = new slice<ipAttr>(len(srcs));
    foreach (var (i, v) in addrs) {
        var (addrAttrIP, _) = netip.AddrFromSlice(v.IP);
        addrAttr[i] = ipAttrOf(addrAttrIP);
        srcAttr[i] = ipAttrOf(srcs[i]);
    }
    sort.Stable(new byRFC6724(
        addrs: addrs,
        addrAttr: addrAttr,
        srcs: srcs,
        srcAttr: srcAttr
    ));
}

// srcAddrs tries to UDP-connect to each address to see if it has a
// route. (This doesn't send any packets). The destination port
// number is irrelevant.
internal static slice<netipꓸAddr> srcAddrs(slice<IPAddr> addrs) {
    var srcs = new slice<netipꓸAddr>(len(addrs));
    ref var dst = ref heap<UDPAddr>(out var Ꮡdst);
    dst = new UDPAddr(Port: 53);
    foreach (var (i, _) in addrs) {
        dst.IP = addrs[i].IP;
        dst.Zone = addrs[i].Zone;
        (c, err) = DialUDP("udp"u8, nil, Ꮡdst);
        if (err == default!) {
            {
                var (src, ok) = c.LocalAddr()._<UDPAddr.val>(ᐧ); if (ok) {
                    (srcs[i], _) = netip.AddrFromSlice((~src).IP);
                }
            }
            c.Close();
        }
    }
    return srcs;
}

[GoType] partial struct ipAttr {
    public scope Scope;
    public uint8 Precedence;
    public uint8 Label;
}

internal static ipAttr ipAttrOf(netipꓸAddr ip) {
    if (!ip.IsValid()) {
        return new ipAttr(nil);
    }
    var match = rfc6724policyTable.Classify(ip);
    return new ipAttr(
        Scope: classifyScope(ip),
        Precedence: match.Precedence,
        Label: match.Label
    );
}

[GoType] partial struct byRFC6724 {
    internal slice<IPAddr> addrs; // addrs to sort
    internal slice<ipAttr> addrAttr;
    internal netipꓸAddr srcs; // or not valid addr if unreachable
    internal slice<ipAttr> srcAttr;
}

[GoRecv] internal static nint Len(this ref byRFC6724 s) {
    return len(s.addrs);
}

[GoRecv] internal static void Swap(this ref byRFC6724 s, nint i, nint j) {
    (s.addrs[i], s.addrs[j]) = (s.addrs[j], s.addrs[i]);
    (s.srcs[i], s.srcs[j]) = (s.srcs[j], s.srcs[i]);
    (s.addrAttr[i], s.addrAttr[j]) = (s.addrAttr[j], s.addrAttr[i]);
    (s.srcAttr[i], s.srcAttr[j]) = (s.srcAttr[j], s.srcAttr[i]);
}

// Less reports whether i is a better destination address for this
// host than j.
//
// The algorithm and variable names comes from RFC 6724 section 6.
[GoRecv] internal static bool Less(this ref byRFC6724 s, nint i, nint j) {
    var DA = s.addrs[i].IP;
    var DB = s.addrs[j].IP;
    var SourceDA = s.srcs[i];
    var SourceDB = s.srcs[j];
    var attrDA = Ꮡ(s.addrAttr[i]);
    var attrDB = Ꮡ(s.addrAttr[j]);
    var attrSourceDA = Ꮡ(s.srcAttr[i]);
    var attrSourceDB = Ꮡ(s.srcAttr[j]);
    const bool preferDA = true;
    const bool preferDB = false;
    // Rule 1: Avoid unusable destinations.
    // If DB is known to be unreachable or if Source(DB) is undefined, then
    // prefer DA.  Similarly, if DA is known to be unreachable or if
    // Source(DA) is undefined, then prefer DB.
    if (!SourceDA.IsValid() && !SourceDB.IsValid()) {
        return false;
    }
    // "equal"
    if (!SourceDB.IsValid()) {
        return preferDA;
    }
    if (!SourceDA.IsValid()) {
        return preferDB;
    }
    // Rule 2: Prefer matching scope.
    // If Scope(DA) = Scope(Source(DA)) and Scope(DB) <> Scope(Source(DB)),
    // then prefer DA.  Similarly, if Scope(DA) <> Scope(Source(DA)) and
    // Scope(DB) = Scope(Source(DB)), then prefer DB.
    if ((~attrDA).Scope == (~attrSourceDA).Scope && (~attrDB).Scope != (~attrSourceDB).Scope) {
        return preferDA;
    }
    if ((~attrDA).Scope != (~attrSourceDA).Scope && (~attrDB).Scope == (~attrSourceDB).Scope) {
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
    if ((~attrSourceDA).Label == (~attrDA).Label && (~attrSourceDB).Label != (~attrDB).Label) {
        return preferDA;
    }
    if ((~attrSourceDA).Label != (~attrDA).Label && (~attrSourceDB).Label == (~attrDB).Label) {
        return preferDB;
    }
    // Rule 6: Prefer higher precedence.
    // If Precedence(DA) > Precedence(DB), then prefer DA.  Similarly, if
    // Precedence(DA) < Precedence(DB), then prefer DB.
    if ((~attrDA).Precedence > (~attrDB).Precedence) {
        return preferDA;
    }
    if ((~attrDA).Precedence < (~attrDB).Precedence) {
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
    if ((~attrDA).Scope < (~attrDB).Scope) {
        return preferDA;
    }
    if ((~attrDA).Scope > (~attrDB).Scope) {
        return preferDB;
    }
    // Rule 9: Use the longest matching prefix.
    // When DA and DB belong to the same address family (both are IPv6 or
    // both are IPv4 [but see below]): If CommonPrefixLen(Source(DA), DA) >
    // CommonPrefixLen(Source(DB), DB), then prefer DA.  Similarly, if
    // CommonPrefixLen(Source(DA), DA) < CommonPrefixLen(Source(DB), DB),
    // then prefer DB.
    //
    // However, applying this rule to IPv4 addresses causes
    // problems (see issues 13283 and 18518), so limit to IPv6.
    if (DA.To4() == default! && DB.To4() == default!) {
        nint commonA = commonPrefixLen(SourceDA, DA);
        nint commonB = commonPrefixLen(SourceDB, DB);
        if (commonA > commonB) {
            return preferDA;
        }
        if (commonA < commonB) {
            return preferDB;
        }
    }
    // Rule 10: Otherwise, leave the order unchanged.
    // If DA preceded DB in the original list, prefer DA.
    // Otherwise, prefer DB.
    return false;
}

// "equal"
[GoType] partial struct policyTableEntry {
    public net.netip_package.ΔPrefix Prefix;
    public uint8 Precedence;
    public uint8 Label;
}

[GoType("[]policyTableEntry")] partial struct policyTable;

// "::1/128"
// "::ffff:0:0/96"
// IPv4-compatible, etc.
// "::/96"
// "2001::/32"
// Teredo
// "2002::/16"
// 6to4
// "3ffe::/16"
// "fec0::/10"
// "fc00::/7"
// "::/0"
// RFC 6724 section 2.1.
// Items are sorted by the size of their Prefix.Mask.Size,
internal static policyTable rfc6724policyTable = new policyTable{
    new(
        Prefix: netip.PrefixFrom(netip.AddrFrom16(new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}.array()), 128),
        Precedence: 50,
        Label: 0
    ),
    new(
        Prefix: netip.PrefixFrom(netip.AddrFrom16(new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255}.array()), 96),
        Precedence: 35,
        Label: 4
    ),
    new(
        Prefix: netip.PrefixFrom(netip.AddrFrom16(new byte[]{}.array()), 96),
        Precedence: 1,
        Label: 3
    ),
    new(
        Prefix: netip.PrefixFrom(netip.AddrFrom16(new byte[]{32, 1}.array()), 32),
        Precedence: 5,
        Label: 5
    ),
    new(
        Prefix: netip.PrefixFrom(netip.AddrFrom16(new byte[]{32, 2}.array()), 16),
        Precedence: 30,
        Label: 2
    ),
    new(
        Prefix: netip.PrefixFrom(netip.AddrFrom16(new byte[]{63, 254}.array()), 16),
        Precedence: 1,
        Label: 12
    ),
    new(
        Prefix: netip.PrefixFrom(netip.AddrFrom16(new byte[]{254, 192}.array()), 10),
        Precedence: 1,
        Label: 11
    ),
    new(
        Prefix: netip.PrefixFrom(netip.AddrFrom16(new byte[]{252}.array()), 7),
        Precedence: 3,
        Label: 13
    ),
    new(
        Prefix: netip.PrefixFrom(netip.AddrFrom16(new byte[]{}.array()), 0),
        Precedence: 40,
        Label: 1
    )
};

// Classify returns the policyTableEntry of the entry with the longest
// matching prefix that contains ip.
// The table t must be sorted from largest mask size to smallest.
internal static policyTableEntry Classify(this policyTable t, netipꓸAddr ip) {
    // Prefix.Contains() will not match an IPv6 prefix for an IPv4 address.
    if (ip.Is4()) {
        ip = netip.AddrFrom16(ip.As16());
    }
    foreach (var (_, ent) in t) {
        if (ent.Prefix.Contains(ip)) {
            return ent;
        }
    }
    return new policyTableEntry(nil);
}

[GoType("num:uint8")] partial struct scope;

internal static readonly scope scopeInterfaceLocal = /* 0x1 */ 1;
internal static readonly scope scopeLinkLocal = /* 0x2 */ 2;
internal static readonly scope scopeAdminLocal = /* 0x4 */ 4;
internal static readonly scope scopeSiteLocal = /* 0x5 */ 5;
internal static readonly scope scopeOrgLocal = /* 0x8 */ 8;
internal static readonly scope scopeGlobal = /* 0xe */ 14;

internal static scope classifyScope(netipꓸAddr ip) {
    if (ip.IsLoopback() || ip.IsLinkLocalUnicast()) {
        return scopeLinkLocal;
    }
    var ipv6 = ip.Is6() && !ip.Is4In6();
    var ipv6AsBytes = ip.As16();
    if (ipv6 && ip.IsMulticast()) {
        return ((scope)((byte)(ipv6AsBytes[1] & 15)));
    }
    // Site-local addresses are defined in RFC 3513 section 2.5.6
    // (and deprecated in RFC 3879).
    if (ipv6 && ipv6AsBytes[0] == 254 && (byte)(ipv6AsBytes[1] & 192) == 192) {
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
internal static nint /*cpl*/ commonPrefixLen(netipꓸAddr a, IP b) {
    nint cpl = default!;

    {
        var b4 = b.To4(); if (b4 != default!) {
            b = b4;
        }
    }
    var aAsSlice = a.AsSlice();
    if (len(aAsSlice) != len(b)) {
        return 0;
    }
    // If IPv6, only up to the prefix (first 64 bits)
    if (len(aAsSlice) > 8) {
        aAsSlice = aAsSlice[..8];
        b = b[..8];
    }
    while (len(aAsSlice) > 0) {
        if (aAsSlice[0] == b[0]) {
            cpl += 8;
            aAsSlice = aAsSlice[1..];
            b = b[1..];
            continue;
        }
        nint bits = 8;
        var (ab, bb) = (aAsSlice[0], b[0]);
        while (ᐧ) {
            ab >>= (UntypedInt)(1);
            bb >>= (UntypedInt)(1);
            bits--;
            if (ab == bb) {
                cpl += bits;
                return cpl;
            }
        }
    }
    return cpl;
}

} // end net_package
