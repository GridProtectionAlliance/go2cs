// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:25:51 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\dnsclient.go
using rand = go.math.rand_package;
using sort = go.sort_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // reverseaddr returns the in-addr.arpa. or ip6.arpa. hostname of the IP
        // address addr suitable for rDNS (PTR) record lookup or an error if it fails
        // to parse the IP address.
        private static (@string, error) reverseaddr(@string addr)
        {
            var ip = ParseIP(addr);
            if (ip == null)
            {
                return ("", ref new DNSError(Err:"unrecognized address",Name:addr));
            }
            if (ip.To4() != null)
            {
                return (uitoa(uint(ip[15L])) + "." + uitoa(uint(ip[14L])) + "." + uitoa(uint(ip[13L])) + "." + uitoa(uint(ip[12L])) + ".in-addr.arpa.", null);
            }
            var buf = make_slice<byte>(0L, len(ip) * 4L + len("ip6.arpa.")); 
            // Add it, in reverse, to the buffer
            for (var i = len(ip) - 1L; i >= 0L; i--)
            {
                var v = ip[i];
                buf = append(buf, hexDigit[v & 0xFUL]);
                buf = append(buf, '.');
                buf = append(buf, hexDigit[v >> (int)(4L)]);
                buf = append(buf, '.');
            } 
            // Append "ip6.arpa." and return (buf already has the final .)
            buf = append(buf, "ip6.arpa.");
            return (string(buf), null);
        }

        // Find answer for name in dns message.
        // On return, if err == nil, addrs != nil.
        private static (@string, slice<dnsRR>, error) answer(@string name, @string server, ref dnsMsg dns, ushort qtype)
        {
            addrs = make_slice<dnsRR>(0L, len(dns.answer));

            if (dns.rcode == dnsRcodeNameError)
            {
                return ("", null, ref new DNSError(Err:errNoSuchHost.Error(),Name:name,Server:server));
            }
            if (dns.rcode != dnsRcodeSuccess)
            { 
                // None of the error codes make sense
                // for the query we sent. If we didn't get
                // a name error and we didn't get success,
                // the server is behaving incorrectly or
                // having temporary trouble.
                DNSError err = ref new DNSError(Err:"server misbehaving",Name:name,Server:server);
                if (dns.rcode == dnsRcodeServerFailure)
                {
                    err.IsTemporary = true;
                }
                return ("", null, err);
            } 

            // Look for the name.
            // Presotto says it's okay to assume that servers listed in
            // /etc/resolv.conf are recursive resolvers.
            // We asked for recursion, so it should have included
            // all the answers we need in this one packet.
Cname:

            for (long cnameloop = 0L; cnameloop < 10L; cnameloop++)
            {
                addrs = addrs[0L..0L];
                foreach (var (_, rr) in dns.answer)
                {
                    {
                        ref dnsRR_Header (_, justHeader) = rr._<ref dnsRR_Header>();

                        if (justHeader)
                        { 
                            // Corrupt record: we only have a
                            // header. That header might say it's
                            // of type qtype, but we don't
                            // actually have it. Skip.
                            continue;
                        }

                    }
                    var h = rr.Header();
                    if (h.Class == dnsClassINET && equalASCIILabel(h.Name, name))
                    {

                        if (h.Rrtype == qtype) 
                            addrs = append(addrs, rr);
                        else if (h.Rrtype == dnsTypeCNAME) 
                            // redirect to cname
                            name = rr._<ref dnsRR_CNAME>().Cname;
                            _continueCname = true;
                            break;
                                            }
                }
                if (len(addrs) == 0L)
                {
                    return ("", null, ref new DNSError(Err:errNoSuchHost.Error(),Name:name,Server:server));
                }
                return (name, addrs, null);
            }

            return ("", null, ref new DNSError(Err:"too many redirects",Name:name,Server:server));
        }

        private static bool equalASCIILabel(@string x, @string y)
        {
            if (len(x) != len(y))
            {
                return false;
            }
            for (long i = 0L; i < len(x); i++)
            {
                var a = x[i];
                var b = y[i];
                if ('A' <= a && a <= 'Z')
                {
                    a += 0x20UL;
                }
                if ('A' <= b && b <= 'Z')
                {
                    b += 0x20UL;
                }
                if (a != b)
                {
                    return false;
                }
            }

            return true;
        }

        // isDomainName checks if a string is a presentation-format domain name
        // (currently restricted to hostname-compatible "preferred name" LDH labels and
        // SRV-like "underscore labels"; see golang.org/issue/12421).
        private static bool isDomainName(@string s)
        { 
            // See RFC 1035, RFC 3696.
            // Presentation format has dots before every label except the first, and the
            // terminal empty label is optional here because we assume fully-qualified
            // (absolute) input. We must therefore reserve space for the first and last
            // labels' length octets in wire format, where they are necessary and the
            // maximum total length is 255.
            // So our _effective_ maximum is 253, but 254 is not rejected if the last
            // character is a dot.
            var l = len(s);
            if (l == 0L || l > 254L || l == 254L && s[l - 1L] != '.')
            {
                return false;
            }
            var last = byte('.');
            var ok = false; // Ok once we've seen a letter.
            long partlen = 0L;
            for (long i = 0L; i < len(s); i++)
            {
                var c = s[i];

                if ('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z' || c == '_') 
                    ok = true;
                    partlen++;
                else if ('0' <= c && c <= '9') 
                    // fine
                    partlen++;
                else if (c == '-') 
                    // Byte before dash cannot be dot.
                    if (last == '.')
                    {
                        return false;
                    }
                    partlen++;
                else if (c == '.') 
                    // Byte before dot cannot be dot, dash.
                    if (last == '.' || last == '-')
                    {
                        return false;
                    }
                    if (partlen > 63L || partlen == 0L)
                    {
                        return false;
                    }
                    partlen = 0L;
                else 
                    return false;
                                last = c;
            }

            if (last == '-' || partlen > 63L)
            {
                return false;
            }
            return ok;
        }

        // absDomainName returns an absolute domain name which ends with a
        // trailing dot to match pure Go reverse resolver and all other lookup
        // routines.
        // See golang.org/issue/12189.
        // But we don't want to add dots for local names from /etc/hosts.
        // It's hard to tell so we settle on the heuristic that names without dots
        // (like "localhost" or "myhost") do not get trailing dots, but any other
        // names do.
        private static @string absDomainName(slice<byte> b)
        {
            var hasDots = false;
            foreach (var (_, x) in b)
            {
                if (x == '.')
                {
                    hasDots = true;
                    break;
                }
            }
            if (hasDots && b[len(b) - 1L] != '.')
            {
                b = append(b, '.');
            }
            return string(b);
        }

        // An SRV represents a single DNS SRV record.
        public partial struct SRV
        {
            public @string Target;
            public ushort Port;
            public ushort Priority;
            public ushort Weight;
        }

        // byPriorityWeight sorts SRV records by ascending priority and weight.
        private partial struct byPriorityWeight // : slice<ref SRV>
        {
        }

        private static long Len(this byPriorityWeight s)
        {
            return len(s);
        }
        private static bool Less(this byPriorityWeight s, long i, long j)
        {
            return s[i].Priority < s[j].Priority || (s[i].Priority == s[j].Priority && s[i].Weight < s[j].Weight);
        }
        private static void Swap(this byPriorityWeight s, long i, long j)
        {
            s[i] = s[j];
            s[j] = s[i];

        }

        // shuffleByWeight shuffles SRV records by weight using the algorithm
        // described in RFC 2782.
        private static void shuffleByWeight(this byPriorityWeight addrs)
        {
            long sum = 0L;
            foreach (var (_, addr) in addrs)
            {
                sum += int(addr.Weight);
            }
            while (sum > 0L && len(addrs) > 1L)
            {
                long s = 0L;
                var n = rand.Intn(sum);
                foreach (var (i) in addrs)
                {
                    s += int(addrs[i].Weight);
                    if (s > n)
                    {
                        if (i > 0L)
                        {
                            addrs[0L] = addrs[i];
                            addrs[i] = addrs[0L];
                        }
                        break;
                    }
                }
                sum -= int(addrs[0L].Weight);
                addrs = addrs[1L..];
            }

        }

        // sort reorders SRV records as specified in RFC 2782.
        private static void sort(this byPriorityWeight addrs)
        {
            sort.Sort(addrs);
            long i = 0L;
            for (long j = 1L; j < len(addrs); j++)
            {
                if (addrs[i].Priority != addrs[j].Priority)
                {
                    addrs[i..j].shuffleByWeight();
                    i = j;
                }
            }

            addrs[i..].shuffleByWeight();
        }

        // An MX represents a single DNS MX record.
        public partial struct MX
        {
            public @string Host;
            public ushort Pref;
        }

        // byPref implements sort.Interface to sort MX records by preference
        private partial struct byPref // : slice<ref MX>
        {
        }

        private static long Len(this byPref s)
        {
            return len(s);
        }
        private static bool Less(this byPref s, long i, long j)
        {
            return s[i].Pref < s[j].Pref;
        }
        private static void Swap(this byPref s, long i, long j)
        {
            s[i] = s[j];
            s[j] = s[i];

        }

        // sort reorders MX records as specified in RFC 5321.
        private static void sort(this byPref s)
        {
            foreach (var (i) in s)
            {
                var j = rand.Intn(i + 1L);
                s[i] = s[j];
                s[j] = s[i];
            }
            sort.Sort(s);
        }

        // An NS represents a single DNS NS record.
        public partial struct NS
        {
            public @string Host;
        }
    }
}
