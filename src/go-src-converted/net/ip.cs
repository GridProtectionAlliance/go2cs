// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// IP address manipulations
//
// IPv4 addresses are 4 bytes; IPv6 addresses are 16 bytes.
// An IPv4 address can be converted to an IPv6 address by
// adding a canonical prefix (10 zeros, 2 0xFFs).
// This library accepts either size of byte slice but always
// returns 16-byte addresses.

// package net -- go2cs converted at 2022 March 13 05:29:51 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\ip.go
namespace go;

using bytealg = @internal.bytealg_package;
using itoa = @internal.itoa_package;


// IP address lengths (bytes).

public static partial class net_package {

public static readonly nint IPv4len = 4;
public static readonly nint IPv6len = 16;

// An IP is a single IP address, a slice of bytes.
// Functions in this package accept either 4-byte (IPv4)
// or 16-byte (IPv6) slices as input.
//
// Note that in this documentation, referring to an
// IP address as an IPv4 address or an IPv6 address
// is a semantic property of the address, not just the
// length of the byte slice: a 16-byte slice can still
// be an IPv4 address.
public partial struct IP { // : slice<byte>
}

// An IPMask is a bitmask that can be used to manipulate
// IP addresses for IP addressing and routing.
//
// See type IPNet and func ParseCIDR for details.
public partial struct IPMask { // : slice<byte>
}

// An IPNet represents an IP network.
public partial struct IPNet {
    public IP IP; // network number
    public IPMask Mask; // network mask
}

// IPv4 returns the IP address (in 16-byte form) of the
// IPv4 address a.b.c.d.
public static IP IPv4(byte a, byte b, byte c, byte d) {
    var p = make(IP, IPv6len);
    copy(p, v4InV6Prefix);
    p[12] = a;
    p[13] = b;
    p[14] = c;
    p[15] = d;
    return p;
}

private static byte v4InV6Prefix = new slice<byte>(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xff, 0xff });

// IPv4Mask returns the IP mask (in 4-byte form) of the
// IPv4 mask a.b.c.d.
public static IPMask IPv4Mask(byte a, byte b, byte c, byte d) {
    var p = make(IPMask, IPv4len);
    p[0] = a;
    p[1] = b;
    p[2] = c;
    p[3] = d;
    return p;
}

// CIDRMask returns an IPMask consisting of 'ones' 1 bits
// followed by 0s up to a total length of 'bits' bits.
// For a mask of this form, CIDRMask is the inverse of IPMask.Size.
public static IPMask CIDRMask(nint ones, nint bits) {
    if (bits != 8 * IPv4len && bits != 8 * IPv6len) {
        return null;
    }
    if (ones < 0 || ones > bits) {
        return null;
    }
    var l = bits / 8;
    var m = make(IPMask, l);
    var n = uint(ones);
    for (nint i = 0; i < l; i++) {
        if (n >= 8) {
            m[i] = 0xff;
            n -= 8;
            continue;
        }
        m[i] = ~byte(0xff >> (int)(n));
        n = 0;
    }
    return m;
}

// Well-known IPv4 addresses
public static var IPv4bcast = IPv4(255, 255, 255, 255);public static var IPv4allsys = IPv4(224, 0, 0, 1);public static var IPv4allrouter = IPv4(224, 0, 0, 2);public static var IPv4zero = IPv4(0, 0, 0, 0);

// Well-known IPv6 addresses
public static IP IPv6zero = new IP(0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0);public static IP IPv6unspecified = new IP(0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0);public static IP IPv6loopback = new IP(0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1);public static IP IPv6interfacelocalallnodes = new IP(0xff,0x01,0,0,0,0,0,0,0,0,0,0,0,0,0,0x01);public static IP IPv6linklocalallnodes = new IP(0xff,0x02,0,0,0,0,0,0,0,0,0,0,0,0,0,0x01);public static IP IPv6linklocalallrouters = new IP(0xff,0x02,0,0,0,0,0,0,0,0,0,0,0,0,0,0x02);

// IsUnspecified reports whether ip is an unspecified address, either
// the IPv4 address "0.0.0.0" or the IPv6 address "::".
public static bool IsUnspecified(this IP ip) {
    return ip.Equal(IPv4zero) || ip.Equal(IPv6unspecified);
}

// IsLoopback reports whether ip is a loopback address.
public static bool IsLoopback(this IP ip) {
    {
        var ip4 = ip.To4();

        if (ip4 != null) {
            return ip4[0] == 127;
        }
    }
    return ip.Equal(IPv6loopback);
}

// IsPrivate reports whether ip is a private address, according to
// RFC 1918 (IPv4 addresses) and RFC 4193 (IPv6 addresses).
public static bool IsPrivate(this IP ip) {
    {
        var ip4 = ip.To4();

        if (ip4 != null) { 
            // Following RFC 1918, Section 3. Private Address Space which says:
            //   The Internet Assigned Numbers Authority (IANA) has reserved the
            //   following three blocks of the IP address space for private internets:
            //     10.0.0.0        -   10.255.255.255  (10/8 prefix)
            //     172.16.0.0      -   172.31.255.255  (172.16/12 prefix)
            //     192.168.0.0     -   192.168.255.255 (192.168/16 prefix)
            return ip4[0] == 10 || (ip4[0] == 172 && ip4[1] & 0xf0 == 16) || (ip4[0] == 192 && ip4[1] == 168);
        }
    } 
    // Following RFC 4193, Section 8. IANA Considerations which says:
    //   The IANA has assigned the FC00::/7 prefix to "Unique Local Unicast".
    return len(ip) == IPv6len && ip[0] & 0xfe == 0xfc;
}

// IsMulticast reports whether ip is a multicast address.
public static bool IsMulticast(this IP ip) {
    {
        var ip4 = ip.To4();

        if (ip4 != null) {
            return ip4[0] & 0xf0 == 0xe0;
        }
    }
    return len(ip) == IPv6len && ip[0] == 0xff;
}

// IsInterfaceLocalMulticast reports whether ip is
// an interface-local multicast address.
public static bool IsInterfaceLocalMulticast(this IP ip) {
    return len(ip) == IPv6len && ip[0] == 0xff && ip[1] & 0x0f == 0x01;
}

// IsLinkLocalMulticast reports whether ip is a link-local
// multicast address.
public static bool IsLinkLocalMulticast(this IP ip) {
    {
        var ip4 = ip.To4();

        if (ip4 != null) {
            return ip4[0] == 224 && ip4[1] == 0 && ip4[2] == 0;
        }
    }
    return len(ip) == IPv6len && ip[0] == 0xff && ip[1] & 0x0f == 0x02;
}

// IsLinkLocalUnicast reports whether ip is a link-local
// unicast address.
public static bool IsLinkLocalUnicast(this IP ip) {
    {
        var ip4 = ip.To4();

        if (ip4 != null) {
            return ip4[0] == 169 && ip4[1] == 254;
        }
    }
    return len(ip) == IPv6len && ip[0] == 0xfe && ip[1] & 0xc0 == 0x80;
}

// IsGlobalUnicast reports whether ip is a global unicast
// address.
//
// The identification of global unicast addresses uses address type
// identification as defined in RFC 1122, RFC 4632 and RFC 4291 with
// the exception of IPv4 directed broadcast addresses.
// It returns true even if ip is in IPv4 private address space or
// local IPv6 unicast address space.
public static bool IsGlobalUnicast(this IP ip) {
    return (len(ip) == IPv4len || len(ip) == IPv6len) && !ip.Equal(IPv4bcast) && !ip.IsUnspecified() && !ip.IsLoopback() && !ip.IsMulticast() && !ip.IsLinkLocalUnicast();
}

// Is p all zeros?
private static bool isZeros(IP p) {
    for (nint i = 0; i < len(p); i++) {
        if (p[i] != 0) {
            return false;
        }
    }
    return true;
}

// To4 converts the IPv4 address ip to a 4-byte representation.
// If ip is not an IPv4 address, To4 returns nil.
public static IP To4(this IP ip) {
    if (len(ip) == IPv4len) {
        return ip;
    }
    if (len(ip) == IPv6len && isZeros(ip[(int)0..(int)10]) && ip[10] == 0xff && ip[11] == 0xff) {
        return ip[(int)12..(int)16];
    }
    return null;
}

// To16 converts the IP address ip to a 16-byte representation.
// If ip is not an IP address (it is the wrong length), To16 returns nil.
public static IP To16(this IP ip) {
    if (len(ip) == IPv4len) {
        return IPv4(ip[0], ip[1], ip[2], ip[3]);
    }
    if (len(ip) == IPv6len) {
        return ip;
    }
    return null;
}

// Default route masks for IPv4.
private static var classAMask = IPv4Mask(0xff, 0, 0, 0);private static var classBMask = IPv4Mask(0xff, 0xff, 0, 0);private static var classCMask = IPv4Mask(0xff, 0xff, 0xff, 0);

// DefaultMask returns the default IP mask for the IP address ip.
// Only IPv4 addresses have default masks; DefaultMask returns
// nil if ip is not a valid IPv4 address.
public static IPMask DefaultMask(this IP ip) {
    ip = ip.To4();

    if (ip == null) {
        return null;
    }

    if (ip[0] < 0x80) 
        return classAMask;
    else if (ip[0] < 0xC0) 
        return classBMask;
    else 
        return classCMask;
    }

private static bool allFF(slice<byte> b) {
    foreach (var (_, c) in b) {
        if (c != 0xff) {
            return false;
        }
    }    return true;
}

// Mask returns the result of masking the IP address ip with mask.
public static IP Mask(this IP ip, IPMask mask) {
    if (len(mask) == IPv6len && len(ip) == IPv4len && allFF(mask[..(int)12])) {
        mask = mask[(int)12..];
    }
    if (len(mask) == IPv4len && len(ip) == IPv6len && bytealg.Equal(ip[..(int)12], v4InV6Prefix)) {
        ip = ip[(int)12..];
    }
    var n = len(ip);
    if (n != len(mask)) {
        return null;
    }
    var @out = make(IP, n);
    for (nint i = 0; i < n; i++) {
        out[i] = ip[i] & mask[i];
    }
    return out;
}

// ubtoa encodes the string form of the integer v to dst[start:] and
// returns the number of bytes written to dst. The caller must ensure
// that dst has sufficient length.
private static nint ubtoa(slice<byte> dst, nint start, byte v) {
    if (v < 10) {
        dst[start] = v + '0';
        return 1;
    }
    else if (v < 100) {
        dst[start + 1] = v % 10 + '0';
        dst[start] = v / 10 + '0';
        return 2;
    }
    dst[start + 2] = v % 10 + '0';
    dst[start + 1] = (v / 10) % 10 + '0';
    dst[start] = v / 100 + '0';
    return 3;
}

// String returns the string form of the IP address ip.
// It returns one of 4 forms:
//   - "<nil>", if ip has length 0
//   - dotted decimal ("192.0.2.1"), if ip is an IPv4 or IP4-mapped IPv6 address
//   - IPv6 ("2001:db8::1"), if ip is a valid IPv6 address
//   - the hexadecimal form of ip, without punctuation, if no other cases apply
public static @string String(this IP ip) {
    var p = ip;

    if (len(ip) == 0) {
        return "<nil>";
    }
    {
        var p4 = p.To4();

        if (len(p4) == IPv4len) {
            const var maxIPv4StringLen = len("255.255.255.255");

            var b = make_slice<byte>(maxIPv4StringLen);

            var n = ubtoa(b, 0, p4[0]);
            b[n] = '.';
            n++;

            n += ubtoa(b, n, p4[1]);
            b[n] = '.';
            n++;

            n += ubtoa(b, n, p4[2]);
            b[n] = '.';
            n++;

            n += ubtoa(b, n, p4[3]);
            return string(b[..(int)n]);
        }
    }
    if (len(p) != IPv6len) {
        return "?" + hexString(ip);
    }
    nint e0 = -1;
    nint e1 = -1;
    {
        nint i__prev1 = i;

        nint i = 0;

        while (i < IPv6len) {
            var j = i;
            while (j < IPv6len && p[j] == 0 && p[j + 1] == 0) {
                j += 2;
            i += 2;
            }

            if (j > i && j - i > e1 - e0) {
                e0 = i;
                e1 = j;
                i = j;
            }
        }

        i = i__prev1;
    } 
    // The symbol "::" MUST NOT be used to shorten just one 16 bit 0 field.
    if (e1 - e0 <= 2) {
        e0 = -1;
        e1 = -1;
    }
    const var maxLen = len("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff");

    b = make_slice<byte>(0, maxLen); 

    // Print with possible :: in place of run of zeros
    {
        nint i__prev1 = i;

        i = 0;

        while (i < IPv6len) {
            if (i == e0) {
                b = append(b, ':', ':');
                i = e1;
                if (i >= IPv6len) {
                    break;
            i += 2;
                }
            }
            else if (i > 0) {
                b = append(b, ':');
            }
            b = appendHex(b, (uint32(p[i]) << 8) | uint32(p[i + 1]));
        }

        i = i__prev1;
    }
    return string(b);
}

private static @string hexString(slice<byte> b) {
    var s = make_slice<byte>(len(b) * 2);
    foreach (var (i, tn) in b) {
        (s[i * 2], s[i * 2 + 1]) = (hexDigit[tn >> 4], hexDigit[tn & 0xf]);
    }    return string(s);
}

// ipEmptyString is like ip.String except that it returns
// an empty string when ip is unset.
private static @string ipEmptyString(IP ip) {
    if (len(ip) == 0) {
        return "";
    }
    return ip.String();
}

// MarshalText implements the encoding.TextMarshaler interface.
// The encoding is the same as returned by String, with one exception:
// When len(ip) is zero, it returns an empty slice.
public static (slice<byte>, error) MarshalText(this IP ip) {
    slice<byte> _p0 = default;
    error _p0 = default!;

    if (len(ip) == 0) {
        return ((slice<byte>)"", error.As(null!)!);
    }
    if (len(ip) != IPv4len && len(ip) != IPv6len) {
        return (null, error.As(addr(new AddrError(Err:"invalid IP address",Addr:hexString(ip)))!)!);
    }
    return ((slice<byte>)ip.String(), error.As(null!)!);
}

// UnmarshalText implements the encoding.TextUnmarshaler interface.
// The IP address is expected in a form accepted by ParseIP.
private static error UnmarshalText(this ptr<IP> _addr_ip, slice<byte> text) {
    ref IP ip = ref _addr_ip.val;

    if (len(text) == 0) {
        ip.val = null;
        return error.As(null!)!;
    }
    var s = string(text);
    var x = ParseIP(s);
    if (x == null) {
        return error.As(addr(new ParseError(Type:"IP address",Text:s))!)!;
    }
    ip.val = x;
    return error.As(null!)!;
}

// Equal reports whether ip and x are the same IP address.
// An IPv4 address and that same address in IPv6 form are
// considered to be equal.
public static bool Equal(this IP ip, IP x) {
    if (len(ip) == len(x)) {
        return bytealg.Equal(ip, x);
    }
    if (len(ip) == IPv4len && len(x) == IPv6len) {
        return bytealg.Equal(x[(int)0..(int)12], v4InV6Prefix) && bytealg.Equal(ip, x[(int)12..]);
    }
    if (len(ip) == IPv6len && len(x) == IPv4len) {
        return bytealg.Equal(ip[(int)0..(int)12], v4InV6Prefix) && bytealg.Equal(ip[(int)12..], x);
    }
    return false;
}

public static bool matchAddrFamily(this IP ip, IP x) {
    return ip.To4() != null && x.To4() != null || ip.To16() != null && ip.To4() == null && x.To16() != null && x.To4() == null;
}

// If mask is a sequence of 1 bits followed by 0 bits,
// return the number of 1 bits.
private static nint simpleMaskLength(IPMask mask) {
    nint n = default;
    foreach (var (i, v) in mask) {
        if (v == 0xff) {
            n += 8;
            continue;
        }
        while (v & 0x80 != 0) {
            n++;
            v<<=1;
        } 
        // rest must be 0 bits
        if (v != 0) {
            return -1;
        }
        i++;

        while (i < len(mask)) {
            if (mask[i] != 0) {
                return -1;
            i++;
            }
        }
        break;
    }    return n;
}

// Size returns the number of leading ones and total bits in the mask.
// If the mask is not in the canonical form--ones followed by zeros--then
// Size returns 0, 0.
public static (nint, nint) Size(this IPMask m) {
    nint ones = default;
    nint bits = default;

    (ones, bits) = (simpleMaskLength(m), len(m) * 8);    if (ones == -1) {
        return (0, 0);
    }
    return ;
}

// String returns the hexadecimal form of m, with no punctuation.
public static @string String(this IPMask m) {
    if (len(m) == 0) {
        return "<nil>";
    }
    return hexString(m);
}

private static (IP, IPMask) networkNumberAndMask(ptr<IPNet> _addr_n) {
    IP ip = default;
    IPMask m = default;
    ref IPNet n = ref _addr_n.val;

    ip = n.IP.To4();

    if (ip == null) {
        ip = n.IP;
        if (len(ip) != IPv6len) {
            return (null, null);
        }
    }
    m = n.Mask;

    if (len(m) == IPv4len) 
        if (len(ip) != IPv4len) {
            return (null, null);
        }
    else if (len(m) == IPv6len) 
        if (len(ip) == IPv4len) {
            m = m[(int)12..];
        }
    else 
        return (null, null);
        return ;
}

// Contains reports whether the network includes ip.
private static bool Contains(this ptr<IPNet> _addr_n, IP ip) {
    ref IPNet n = ref _addr_n.val;

    var (nn, m) = networkNumberAndMask(_addr_n);
    {
        var x = ip.To4();

        if (x != null) {
            ip = x;
        }
    }
    var l = len(ip);
    if (l != len(nn)) {
        return false;
    }
    for (nint i = 0; i < l; i++) {
        if (nn[i] & m[i] != ip[i] & m[i]) {
            return false;
        }
    }
    return true;
}

// Network returns the address's network name, "ip+net".
private static @string Network(this ptr<IPNet> _addr_n) {
    ref IPNet n = ref _addr_n.val;

    return "ip+net";
}

// String returns the CIDR notation of n like "192.0.2.0/24"
// or "2001:db8::/48" as defined in RFC 4632 and RFC 4291.
// If the mask is not in the canonical form, it returns the
// string which consists of an IP address, followed by a slash
// character and a mask expressed as hexadecimal form with no
// punctuation like "198.51.100.0/c000ff00".
private static @string String(this ptr<IPNet> _addr_n) {
    ref IPNet n = ref _addr_n.val;

    var (nn, m) = networkNumberAndMask(_addr_n);
    if (nn == null || m == null) {
        return "<nil>";
    }
    var l = simpleMaskLength(m);
    if (l == -1) {
        return nn.String() + "/" + m.String();
    }
    return nn.String() + "/" + itoa.Uitoa(uint(l));
}

// Parse IPv4 address (d.d.d.d).
private static IP parseIPv4(@string s) {
    array<byte> p = new array<byte>(IPv4len);
    for (nint i = 0; i < IPv4len; i++) {
        if (len(s) == 0) { 
            // Missing octets.
            return null;
        }
        if (i > 0) {
            if (s[0] != '.') {
                return null;
            }
            s = s[(int)1..];
        }
        var (n, c, ok) = dtoi(s);
        if (!ok || n > 0xFF) {
            return null;
        }
        if (c > 1 && s[0] == '0') { 
            // Reject non-zero components with leading zeroes.
            return null;
        }
        s = s[(int)c..];
        p[i] = byte(n);
    }
    if (len(s) != 0) {
        return null;
    }
    return IPv4(p[0], p[1], p[2], p[3]);
}

// parseIPv6Zone parses s as a literal IPv6 address and its associated zone
// identifier which is described in RFC 4007.
private static (IP, @string) parseIPv6Zone(@string s) {
    IP _p0 = default;
    @string _p0 = default;

    var (s, zone) = splitHostZone(s);
    return (parseIPv6(s), zone);
}

// parseIPv6 parses s as a literal IPv6 address described in RFC 4291
// and RFC 5952.
private static IP parseIPv6(@string s) {
    IP ip = default;

    ip = make(IP, IPv6len);
    nint ellipsis = -1; // position of ellipsis in ip

    // Might have leading ellipsis
    if (len(s) >= 2 && s[0] == ':' && s[1] == ':') {
        ellipsis = 0;
        s = s[(int)2..]; 
        // Might be only ellipsis
        if (len(s) == 0) {
            return ip;
        }
    }
    nint i = 0;
    while (i < IPv6len) { 
        // Hex number.
        var (n, c, ok) = xtoi(s);
        if (!ok || n > 0xFFFF) {
            return null;
        }
        if (c < len(s) && s[c] == '.') {
            if (ellipsis < 0 && i != IPv6len - IPv4len) { 
                // Not the right place.
                return null;
            }
            if (i + IPv4len > IPv6len) { 
                // Not enough room.
                return null;
            }
            var ip4 = parseIPv4(s);
            if (ip4 == null) {
                return null;
            }
            ip[i] = ip4[12];
            ip[i + 1] = ip4[13];
            ip[i + 2] = ip4[14];
            ip[i + 3] = ip4[15];
            s = "";
            i += IPv4len;
            break;
        }
        ip[i] = byte(n >> 8);
        ip[i + 1] = byte(n);
        i += 2; 

        // Stop at end of string.
        s = s[(int)c..];
        if (len(s) == 0) {
            break;
        }
        if (s[0] != ':' || len(s) == 1) {
            return null;
        }
        s = s[(int)1..]; 

        // Look for ellipsis.
        if (s[0] == ':') {
            if (ellipsis >= 0) { // already have one
                return null;
            }
            ellipsis = i;
            s = s[(int)1..];
            if (len(s) == 0) { // can be at end
                break;
            }
        }
    } 

    // Must have used entire string.
    if (len(s) != 0) {
        return null;
    }
    if (i < IPv6len) {
        if (ellipsis < 0) {
            return null;
        }
        var n = IPv6len - i;
        {
            var j__prev1 = j;

            for (var j = i - 1; j >= ellipsis; j--) {
                ip[j + n] = ip[j];
            }


            j = j__prev1;
        }
        {
            var j__prev1 = j;

            for (j = ellipsis + n - 1; j >= ellipsis; j--) {
                ip[j] = 0;
            }


            j = j__prev1;
        }
    }
    else if (ellipsis >= 0) { 
        // Ellipsis must represent at least one 0 group.
        return null;
    }
    return ip;
}

// ParseIP parses s as an IP address, returning the result.
// The string s can be in IPv4 dotted decimal ("192.0.2.1"), IPv6
// ("2001:db8::68"), or IPv4-mapped IPv6 ("::ffff:192.0.2.1") form.
// If s is not a valid textual representation of an IP address,
// ParseIP returns nil.
public static IP ParseIP(@string s) {
    for (nint i = 0; i < len(s); i++) {
        switch (s[i]) {
            case '.': 
                return parseIPv4(s);
                break;
            case ':': 
                return parseIPv6(s);
                break;
        }
    }
    return null;
}

// parseIPZone parses s as an IP address, return it and its associated zone
// identifier (IPv6 only).
private static (IP, @string) parseIPZone(@string s) {
    IP _p0 = default;
    @string _p0 = default;

    for (nint i = 0; i < len(s); i++) {
        switch (s[i]) {
            case '.': 
                return (parseIPv4(s), "");
                break;
            case ':': 
                return parseIPv6Zone(s);
                break;
        }
    }
    return (null, "");
}

// ParseCIDR parses s as a CIDR notation IP address and prefix length,
// like "192.0.2.0/24" or "2001:db8::/32", as defined in
// RFC 4632 and RFC 4291.
//
// It returns the IP address and the network implied by the IP and
// prefix length.
// For example, ParseCIDR("192.0.2.1/24") returns the IP address
// 192.0.2.1 and the network 192.0.2.0/24.
public static (IP, ptr<IPNet>, error) ParseCIDR(@string s) {
    IP _p0 = default;
    ptr<IPNet> _p0 = default!;
    error _p0 = default!;

    var i = bytealg.IndexByteString(s, '/');
    if (i < 0) {
        return (null, _addr_null!, error.As(addr(new ParseError(Type:"CIDR address",Text:s))!)!);
    }
    var addr = s[..(int)i];
    var mask = s[(int)i + 1..];
    var iplen = IPv4len;
    var ip = parseIPv4(addr);
    if (ip == null) {
        iplen = IPv6len;
        ip = parseIPv6(addr);
    }
    var (n, i, ok) = dtoi(mask);
    if (ip == null || !ok || i != len(mask) || n < 0 || n > 8 * iplen) {
        return (null, _addr_null!, error.As(addr(new ParseError(Type:"CIDR address",Text:s))!)!);
    }
    var m = CIDRMask(n, 8 * iplen);
    return (ip, addr(new IPNet(IP:ip.Mask(m),Mask:m)), error.As(null!)!);
}

} // end net_package
