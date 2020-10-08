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

// package net -- go2cs converted at 2020 October 08 03:33:41 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\ip.go
using bytealg = go.@internal.bytealg_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // IP address lengths (bytes).
        public static readonly long IPv4len = (long)4L;
        public static readonly long IPv6len = (long)16L;


        // An IP is a single IP address, a slice of bytes.
        // Functions in this package accept either 4-byte (IPv4)
        // or 16-byte (IPv6) slices as input.
        //
        // Note that in this documentation, referring to an
        // IP address as an IPv4 address or an IPv6 address
        // is a semantic property of the address, not just the
        // length of the byte slice: a 16-byte slice can still
        // be an IPv4 address.
        public partial struct IP // : slice<byte>
        {
        }

        // An IPMask is a bitmask that can be used to manipulate
        // IP addresses for IP addressing and routing.
        //
        // See type IPNet and func ParseCIDR for details.
        public partial struct IPMask // : slice<byte>
        {
        }

        // An IPNet represents an IP network.
        public partial struct IPNet
        {
            public IP IP; // network number
            public IPMask Mask; // network mask
        }

        // IPv4 returns the IP address (in 16-byte form) of the
        // IPv4 address a.b.c.d.
        public static IP IPv4(byte a, byte b, byte c, byte d)
        {
            var p = make(IP, IPv6len);
            copy(p, v4InV6Prefix);
            p[12L] = a;
            p[13L] = b;
            p[14L] = c;
            p[15L] = d;
            return p;
        }

        private static byte v4InV6Prefix = new slice<byte>(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xff, 0xff });

        // IPv4Mask returns the IP mask (in 4-byte form) of the
        // IPv4 mask a.b.c.d.
        public static IPMask IPv4Mask(byte a, byte b, byte c, byte d)
        {
            var p = make(IPMask, IPv4len);
            p[0L] = a;
            p[1L] = b;
            p[2L] = c;
            p[3L] = d;
            return p;
        }

        // CIDRMask returns an IPMask consisting of 'ones' 1 bits
        // followed by 0s up to a total length of 'bits' bits.
        // For a mask of this form, CIDRMask is the inverse of IPMask.Size.
        public static IPMask CIDRMask(long ones, long bits)
        {
            if (bits != 8L * IPv4len && bits != 8L * IPv6len)
            {
                return null;
            }

            if (ones < 0L || ones > bits)
            {
                return null;
            }

            var l = bits / 8L;
            var m = make(IPMask, l);
            var n = uint(ones);
            for (long i = 0L; i < l; i++)
            {
                if (n >= 8L)
                {
                    m[i] = 0xffUL;
                    n -= 8L;
                    continue;
                }

                m[i] = ~byte(0xffUL >> (int)(n));
                n = 0L;

            }

            return m;

        }

        // Well-known IPv4 addresses
        public static var IPv4bcast = IPv4(255L, 255L, 255L, 255L);        public static var IPv4allsys = IPv4(224L, 0L, 0L, 1L);        public static var IPv4allrouter = IPv4(224L, 0L, 0L, 2L);        public static var IPv4zero = IPv4(0L, 0L, 0L, 0L);

        // Well-known IPv6 addresses
        public static IP IPv6zero = new IP(0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0);        public static IP IPv6unspecified = new IP(0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0);        public static IP IPv6loopback = new IP(0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1);        public static IP IPv6interfacelocalallnodes = new IP(0xff,0x01,0,0,0,0,0,0,0,0,0,0,0,0,0,0x01);        public static IP IPv6linklocalallnodes = new IP(0xff,0x02,0,0,0,0,0,0,0,0,0,0,0,0,0,0x01);        public static IP IPv6linklocalallrouters = new IP(0xff,0x02,0,0,0,0,0,0,0,0,0,0,0,0,0,0x02);

        // IsUnspecified reports whether ip is an unspecified address, either
        // the IPv4 address "0.0.0.0" or the IPv6 address "::".
        public static bool IsUnspecified(this IP ip)
        {
            return ip.Equal(IPv4zero) || ip.Equal(IPv6unspecified);
        }

        // IsLoopback reports whether ip is a loopback address.
        public static bool IsLoopback(this IP ip)
        {
            {
                var ip4 = ip.To4();

                if (ip4 != null)
                {
                    return ip4[0L] == 127L;
                }

            }

            return ip.Equal(IPv6loopback);

        }

        // IsMulticast reports whether ip is a multicast address.
        public static bool IsMulticast(this IP ip)
        {
            {
                var ip4 = ip.To4();

                if (ip4 != null)
                {
                    return ip4[0L] & 0xf0UL == 0xe0UL;
                }

            }

            return len(ip) == IPv6len && ip[0L] == 0xffUL;

        }

        // IsInterfaceLocalMulticast reports whether ip is
        // an interface-local multicast address.
        public static bool IsInterfaceLocalMulticast(this IP ip)
        {
            return len(ip) == IPv6len && ip[0L] == 0xffUL && ip[1L] & 0x0fUL == 0x01UL;
        }

        // IsLinkLocalMulticast reports whether ip is a link-local
        // multicast address.
        public static bool IsLinkLocalMulticast(this IP ip)
        {
            {
                var ip4 = ip.To4();

                if (ip4 != null)
                {
                    return ip4[0L] == 224L && ip4[1L] == 0L && ip4[2L] == 0L;
                }

            }

            return len(ip) == IPv6len && ip[0L] == 0xffUL && ip[1L] & 0x0fUL == 0x02UL;

        }

        // IsLinkLocalUnicast reports whether ip is a link-local
        // unicast address.
        public static bool IsLinkLocalUnicast(this IP ip)
        {
            {
                var ip4 = ip.To4();

                if (ip4 != null)
                {
                    return ip4[0L] == 169L && ip4[1L] == 254L;
                }

            }

            return len(ip) == IPv6len && ip[0L] == 0xfeUL && ip[1L] & 0xc0UL == 0x80UL;

        }

        // IsGlobalUnicast reports whether ip is a global unicast
        // address.
        //
        // The identification of global unicast addresses uses address type
        // identification as defined in RFC 1122, RFC 4632 and RFC 4291 with
        // the exception of IPv4 directed broadcast addresses.
        // It returns true even if ip is in IPv4 private address space or
        // local IPv6 unicast address space.
        public static bool IsGlobalUnicast(this IP ip)
        {
            return (len(ip) == IPv4len || len(ip) == IPv6len) && !ip.Equal(IPv4bcast) && !ip.IsUnspecified() && !ip.IsLoopback() && !ip.IsMulticast() && !ip.IsLinkLocalUnicast();
        }

        // Is p all zeros?
        private static bool isZeros(IP p)
        {
            for (long i = 0L; i < len(p); i++)
            {
                if (p[i] != 0L)
                {
                    return false;
                }

            }

            return true;

        }

        // To4 converts the IPv4 address ip to a 4-byte representation.
        // If ip is not an IPv4 address, To4 returns nil.
        public static IP To4(this IP ip)
        {
            if (len(ip) == IPv4len)
            {
                return ip;
            }

            if (len(ip) == IPv6len && isZeros(ip[0L..10L]) && ip[10L] == 0xffUL && ip[11L] == 0xffUL)
            {
                return ip[12L..16L];
            }

            return null;

        }

        // To16 converts the IP address ip to a 16-byte representation.
        // If ip is not an IP address (it is the wrong length), To16 returns nil.
        public static IP To16(this IP ip)
        {
            if (len(ip) == IPv4len)
            {
                return IPv4(ip[0L], ip[1L], ip[2L], ip[3L]);
            }

            if (len(ip) == IPv6len)
            {
                return ip;
            }

            return null;

        }

        // Default route masks for IPv4.
        private static var classAMask = IPv4Mask(0xffUL, 0L, 0L, 0L);        private static var classBMask = IPv4Mask(0xffUL, 0xffUL, 0L, 0L);        private static var classCMask = IPv4Mask(0xffUL, 0xffUL, 0xffUL, 0L);

        // DefaultMask returns the default IP mask for the IP address ip.
        // Only IPv4 addresses have default masks; DefaultMask returns
        // nil if ip is not a valid IPv4 address.
        public static IPMask DefaultMask(this IP ip)
        {
            ip = ip.To4();

            if (ip == null)
            {
                return null;
            }


            if (ip[0L] < 0x80UL) 
                return classAMask;
            else if (ip[0L] < 0xC0UL) 
                return classBMask;
            else 
                return classCMask;
            
        }

        private static bool allFF(slice<byte> b)
        {
            foreach (var (_, c) in b)
            {
                if (c != 0xffUL)
                {
                    return false;
                }

            }
            return true;

        }

        // Mask returns the result of masking the IP address ip with mask.
        public static IP Mask(this IP ip, IPMask mask)
        {
            if (len(mask) == IPv6len && len(ip) == IPv4len && allFF(mask[..12L]))
            {
                mask = mask[12L..];
            }

            if (len(mask) == IPv4len && len(ip) == IPv6len && bytealg.Equal(ip[..12L], v4InV6Prefix))
            {
                ip = ip[12L..];
            }

            var n = len(ip);
            if (n != len(mask))
            {
                return null;
            }

            var @out = make(IP, n);
            for (long i = 0L; i < n; i++)
            {
                out[i] = ip[i] & mask[i];
            }

            return out;

        }

        // ubtoa encodes the string form of the integer v to dst[start:] and
        // returns the number of bytes written to dst. The caller must ensure
        // that dst has sufficient length.
        private static long ubtoa(slice<byte> dst, long start, byte v)
        {
            if (v < 10L)
            {
                dst[start] = v + '0';
                return 1L;
            }
            else if (v < 100L)
            {
                dst[start + 1L] = v % 10L + '0';
                dst[start] = v / 10L + '0';
                return 2L;
            }

            dst[start + 2L] = v % 10L + '0';
            dst[start + 1L] = (v / 10L) % 10L + '0';
            dst[start] = v / 100L + '0';
            return 3L;

        }

        // String returns the string form of the IP address ip.
        // It returns one of 4 forms:
        //   - "<nil>", if ip has length 0
        //   - dotted decimal ("192.0.2.1"), if ip is an IPv4 or IP4-mapped IPv6 address
        //   - IPv6 ("2001:db8::1"), if ip is a valid IPv6 address
        //   - the hexadecimal form of ip, without punctuation, if no other cases apply
        public static @string String(this IP ip)
        {
            var p = ip;

            if (len(ip) == 0L)
            {
                return "<nil>";
            } 

            // If IPv4, use dotted notation.
            {
                var p4 = p.To4();

                if (len(p4) == IPv4len)
                {
                    const var maxIPv4StringLen = (var)len("255.255.255.255");

                    var b = make_slice<byte>(maxIPv4StringLen);

                    var n = ubtoa(b, 0L, p4[0L]);
                    b[n] = '.';
                    n++;

                    n += ubtoa(b, n, p4[1L]);
                    b[n] = '.';
                    n++;

                    n += ubtoa(b, n, p4[2L]);
                    b[n] = '.';
                    n++;

                    n += ubtoa(b, n, p4[3L]);
                    return string(b[..n]);
                }

            }

            if (len(p) != IPv6len)
            {
                return "?" + hexString(ip);
            } 

            // Find longest run of zeros.
            long e0 = -1L;
            long e1 = -1L;
            {
                long i__prev1 = i;

                long i = 0L;

                while (i < IPv6len)
                {
                    var j = i;
                    while (j < IPv6len && p[j] == 0L && p[j + 1L] == 0L)
                    {
                        j += 2L;
                    i += 2L;
                    }

                    if (j > i && j - i > e1 - e0)
                    {
                        e0 = i;
                        e1 = j;
                        i = j;
                    }

                } 
                // The symbol "::" MUST NOT be used to shorten just one 16 bit 0 field.


                i = i__prev1;
            } 
            // The symbol "::" MUST NOT be used to shorten just one 16 bit 0 field.
            if (e1 - e0 <= 2L)
            {
                e0 = -1L;
                e1 = -1L;
            }

            const var maxLen = (var)len("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff");

            b = make_slice<byte>(0L, maxLen); 

            // Print with possible :: in place of run of zeros
            {
                long i__prev1 = i;

                i = 0L;

                while (i < IPv6len)
                {
                    if (i == e0)
                    {
                        b = append(b, ':', ':');
                        i = e1;
                        if (i >= IPv6len)
                        {
                            break;
                    i += 2L;
                        }

                    }
                    else if (i > 0L)
                    {
                        b = append(b, ':');
                    }

                    b = appendHex(b, (uint32(p[i]) << (int)(8L)) | uint32(p[i + 1L]));

                }


                i = i__prev1;
            }
            return string(b);

        }

        private static @string hexString(slice<byte> b)
        {
            var s = make_slice<byte>(len(b) * 2L);
            foreach (var (i, tn) in b)
            {
                s[i * 2L] = hexDigit[tn >> (int)(4L)];
                s[i * 2L + 1L] = hexDigit[tn & 0xfUL];

            }
            return string(s);

        }

        // ipEmptyString is like ip.String except that it returns
        // an empty string when ip is unset.
        private static @string ipEmptyString(IP ip)
        {
            if (len(ip) == 0L)
            {
                return "";
            }

            return ip.String();

        }

        // MarshalText implements the encoding.TextMarshaler interface.
        // The encoding is the same as returned by String, with one exception:
        // When len(ip) is zero, it returns an empty slice.
        public static (slice<byte>, error) MarshalText(this IP ip)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            if (len(ip) == 0L)
            {
                return ((slice<byte>)"", error.As(null!)!);
            }

            if (len(ip) != IPv4len && len(ip) != IPv6len)
            {
                return (null, error.As(addr(new AddrError(Err:"invalid IP address",Addr:hexString(ip)))!)!);
            }

            return ((slice<byte>)ip.String(), error.As(null!)!);

        }

        // UnmarshalText implements the encoding.TextUnmarshaler interface.
        // The IP address is expected in a form accepted by ParseIP.
        private static error UnmarshalText(this ptr<IP> _addr_ip, slice<byte> text)
        {
            ref IP ip = ref _addr_ip.val;

            if (len(text) == 0L)
            {
                ip.val = null;
                return error.As(null!)!;
            }

            var s = string(text);
            var x = ParseIP(s);
            if (x == null)
            {
                return error.As(addr(new ParseError(Type:"IP address",Text:s))!)!;
            }

            ip.val = x;
            return error.As(null!)!;

        }

        // Equal reports whether ip and x are the same IP address.
        // An IPv4 address and that same address in IPv6 form are
        // considered to be equal.
        public static bool Equal(this IP ip, IP x)
        {
            if (len(ip) == len(x))
            {
                return bytealg.Equal(ip, x);
            }

            if (len(ip) == IPv4len && len(x) == IPv6len)
            {
                return bytealg.Equal(x[0L..12L], v4InV6Prefix) && bytealg.Equal(ip, x[12L..]);
            }

            if (len(ip) == IPv6len && len(x) == IPv4len)
            {
                return bytealg.Equal(ip[0L..12L], v4InV6Prefix) && bytealg.Equal(ip[12L..], x);
            }

            return false;

        }

        public static bool matchAddrFamily(this IP ip, IP x)
        {
            return ip.To4() != null && x.To4() != null || ip.To16() != null && ip.To4() == null && x.To16() != null && x.To4() == null;
        }

        // If mask is a sequence of 1 bits followed by 0 bits,
        // return the number of 1 bits.
        private static long simpleMaskLength(IPMask mask)
        {
            long n = default;
            foreach (var (i, v) in mask)
            {
                if (v == 0xffUL)
                {
                    n += 8L;
                    continue;
                } 
                // found non-ff byte
                // count 1 bits
                while (v & 0x80UL != 0L)
                {
                    n++;
                    v <<= 1L;
                } 
                // rest must be 0 bits
 
                // rest must be 0 bits
                if (v != 0L)
                {
                    return -1L;
                }

                i++;

                while (i < len(mask))
                {
                    if (mask[i] != 0L)
                    {
                        return -1L;
                    i++;
                    }

                }

                break;

            }
            return n;

        }

        // Size returns the number of leading ones and total bits in the mask.
        // If the mask is not in the canonical form--ones followed by zeros--then
        // Size returns 0, 0.
        public static (long, long) Size(this IPMask m)
        {
            long ones = default;
            long bits = default;

            ones = simpleMaskLength(m);
            bits = len(m) * 8L;
            if (ones == -1L)
            {
                return (0L, 0L);
            }

            return ;

        }

        // String returns the hexadecimal form of m, with no punctuation.
        public static @string String(this IPMask m)
        {
            if (len(m) == 0L)
            {
                return "<nil>";
            }

            return hexString(m);

        }

        private static (IP, IPMask) networkNumberAndMask(ptr<IPNet> _addr_n)
        {
            IP ip = default;
            IPMask m = default;
            ref IPNet n = ref _addr_n.val;

            ip = n.IP.To4();

            if (ip == null)
            {
                ip = n.IP;
                if (len(ip) != IPv6len)
                {
                    return (null, null);
                }

            }

            m = n.Mask;

            if (len(m) == IPv4len) 
                if (len(ip) != IPv4len)
                {
                    return (null, null);
                }

            else if (len(m) == IPv6len) 
                if (len(ip) == IPv4len)
                {
                    m = m[12L..];
                }

            else 
                return (null, null);
                        return ;

        }

        // Contains reports whether the network includes ip.
        private static bool Contains(this ptr<IPNet> _addr_n, IP ip)
        {
            ref IPNet n = ref _addr_n.val;

            var (nn, m) = networkNumberAndMask(_addr_n);
            {
                var x = ip.To4();

                if (x != null)
                {
                    ip = x;
                }

            }

            var l = len(ip);
            if (l != len(nn))
            {
                return false;
            }

            for (long i = 0L; i < l; i++)
            {
                if (nn[i] & m[i] != ip[i] & m[i])
                {
                    return false;
                }

            }

            return true;

        }

        // Network returns the address's network name, "ip+net".
        private static @string Network(this ptr<IPNet> _addr_n)
        {
            ref IPNet n = ref _addr_n.val;

            return "ip+net";
        }

        // String returns the CIDR notation of n like "192.0.2.0/24"
        // or "2001:db8::/48" as defined in RFC 4632 and RFC 4291.
        // If the mask is not in the canonical form, it returns the
        // string which consists of an IP address, followed by a slash
        // character and a mask expressed as hexadecimal form with no
        // punctuation like "198.51.100.0/c000ff00".
        private static @string String(this ptr<IPNet> _addr_n)
        {
            ref IPNet n = ref _addr_n.val;

            var (nn, m) = networkNumberAndMask(_addr_n);
            if (nn == null || m == null)
            {
                return "<nil>";
            }

            var l = simpleMaskLength(m);
            if (l == -1L)
            {
                return nn.String() + "/" + m.String();
            }

            return nn.String() + "/" + uitoa(uint(l));

        }

        // Parse IPv4 address (d.d.d.d).
        private static IP parseIPv4(@string s)
        {
            array<byte> p = new array<byte>(IPv4len);
            for (long i = 0L; i < IPv4len; i++)
            {
                if (len(s) == 0L)
                { 
                    // Missing octets.
                    return null;

                }

                if (i > 0L)
                {
                    if (s[0L] != '.')
                    {
                        return null;
                    }

                    s = s[1L..];

                }

                var (n, c, ok) = dtoi(s);
                if (!ok || n > 0xFFUL)
                {
                    return null;
                }

                s = s[c..];
                p[i] = byte(n);

            }

            if (len(s) != 0L)
            {
                return null;
            }

            return IPv4(p[0L], p[1L], p[2L], p[3L]);

        }

        // parseIPv6Zone parses s as a literal IPv6 address and its associated zone
        // identifier which is described in RFC 4007.
        private static (IP, @string) parseIPv6Zone(@string s)
        {
            IP _p0 = default;
            @string _p0 = default;

            var (s, zone) = splitHostZone(s);
            return (parseIPv6(s), zone);
        }

        // parseIPv6 parses s as a literal IPv6 address described in RFC 4291
        // and RFC 5952.
        private static IP parseIPv6(@string s)
        {
            IP ip = default;

            ip = make(IP, IPv6len);
            long ellipsis = -1L; // position of ellipsis in ip

            // Might have leading ellipsis
            if (len(s) >= 2L && s[0L] == ':' && s[1L] == ':')
            {
                ellipsis = 0L;
                s = s[2L..]; 
                // Might be only ellipsis
                if (len(s) == 0L)
                {
                    return ip;
                }

            } 

            // Loop, parsing hex numbers followed by colon.
            long i = 0L;
            while (i < IPv6len)
            { 
                // Hex number.
                var (n, c, ok) = xtoi(s);
                if (!ok || n > 0xFFFFUL)
                {
                    return null;
                } 

                // If followed by dot, might be in trailing IPv4.
                if (c < len(s) && s[c] == '.')
                {
                    if (ellipsis < 0L && i != IPv6len - IPv4len)
                    { 
                        // Not the right place.
                        return null;

                    }

                    if (i + IPv4len > IPv6len)
                    { 
                        // Not enough room.
                        return null;

                    }

                    var ip4 = parseIPv4(s);
                    if (ip4 == null)
                    {
                        return null;
                    }

                    ip[i] = ip4[12L];
                    ip[i + 1L] = ip4[13L];
                    ip[i + 2L] = ip4[14L];
                    ip[i + 3L] = ip4[15L];
                    s = "";
                    i += IPv4len;
                    break;

                } 

                // Save this 16-bit chunk.
                ip[i] = byte(n >> (int)(8L));
                ip[i + 1L] = byte(n);
                i += 2L; 

                // Stop at end of string.
                s = s[c..];
                if (len(s) == 0L)
                {
                    break;
                } 

                // Otherwise must be followed by colon and more.
                if (s[0L] != ':' || len(s) == 1L)
                {
                    return null;
                }

                s = s[1L..]; 

                // Look for ellipsis.
                if (s[0L] == ':')
                {
                    if (ellipsis >= 0L)
                    { // already have one
                        return null;

                    }

                    ellipsis = i;
                    s = s[1L..];
                    if (len(s) == 0L)
                    { // can be at end
                        break;

                    }

                }

            } 

            // Must have used entire string.
 

            // Must have used entire string.
            if (len(s) != 0L)
            {
                return null;
            } 

            // If didn't parse enough, expand ellipsis.
            if (i < IPv6len)
            {
                if (ellipsis < 0L)
                {
                    return null;
                }

                var n = IPv6len - i;
                {
                    var j__prev1 = j;

                    for (var j = i - 1L; j >= ellipsis; j--)
                    {
                        ip[j + n] = ip[j];
                    }


                    j = j__prev1;
                }
                {
                    var j__prev1 = j;

                    for (j = ellipsis + n - 1L; j >= ellipsis; j--)
                    {
                        ip[j] = 0L;
                    }


                    j = j__prev1;
                }

            }
            else if (ellipsis >= 0L)
            { 
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
        public static IP ParseIP(@string s)
        {
            for (long i = 0L; i < len(s); i++)
            {
                switch (s[i])
                {
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
        private static (IP, @string) parseIPZone(@string s)
        {
            IP _p0 = default;
            @string _p0 = default;

            for (long i = 0L; i < len(s); i++)
            {
                switch (s[i])
                {
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
        public static (IP, ptr<IPNet>, error) ParseCIDR(@string s)
        {
            IP _p0 = default;
            ptr<IPNet> _p0 = default!;
            error _p0 = default!;

            var i = bytealg.IndexByteString(s, '/');
            if (i < 0L)
            {
                return (null, _addr_null!, error.As(addr(new ParseError(Type:"CIDR address",Text:s))!)!);
            }

            var addr = s[..i];
            var mask = s[i + 1L..];
            var iplen = IPv4len;
            var ip = parseIPv4(addr);
            if (ip == null)
            {
                iplen = IPv6len;
                ip = parseIPv6(addr);
            }

            var (n, i, ok) = dtoi(mask);
            if (ip == null || !ok || i != len(mask) || n < 0L || n > 8L * iplen)
            {
                return (null, _addr_null!, error.As(addr(new ParseError(Type:"CIDR address",Text:s))!)!);
            }

            var m = CIDRMask(n, 8L * iplen);
            return (ip, addr(new IPNet(IP:ip.Mask(m),Mask:m)), error.As(null!)!);

        }
    }
}
