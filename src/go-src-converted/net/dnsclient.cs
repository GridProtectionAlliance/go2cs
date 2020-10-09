// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 09 04:50:31 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\dnsclient.go
using rand = go.math.rand_package;
using sort = go.sort_package;

using dnsmessage = go.golang.org.x.net.dns.dnsmessage_package;
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
            @string arpa = default;
            error err = default!;

            var ip = ParseIP(addr);
            if (ip == null)
            {
                return ("", error.As(addr(new DNSError(Err:"unrecognized address",Name:addr))!)!);
            }
            if (ip.To4() != null)
            {
                return (uitoa(uint(ip[15L])) + "." + uitoa(uint(ip[14L])) + "." + uitoa(uint(ip[13L])) + "." + uitoa(uint(ip[12L])) + ".in-addr.arpa.", error.As(null!)!);
            }
            var buf = make_slice<byte>(0L, len(ip) * 4L + len("ip6.arpa.")); 
            // Add it, in reverse, to the buffer
            for (var i = len(ip) - 1L; i >= 0L; i--)
            {
                var v = ip[i];
                buf = append(buf, hexDigit[v & 0xFUL], '.', hexDigit[v >> (int)(4L)], '.');
            } 
            // Append "ip6.arpa." and return (buf already has the final .)
            buf = append(buf, "ip6.arpa.");
            return (string(buf), error.As(null!)!);

        }

        private static bool equalASCIIName(dnsmessage.Name x, dnsmessage.Name y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }

            for (long i = 0L; i < int(x.Length); i++)
            {
                var a = x.Data[i];
                var b = y.Data[i];
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
            var nonNumeric = false; // true once we've seen a letter or hyphen
            long partlen = 0L;
            for (long i = 0L; i < len(s); i++)
            {
                var c = s[i];

                if ('a' <= c && c <= 'z' || 'A' <= c && c <= 'Z' || c == '_') 
                    nonNumeric = true;
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
                    nonNumeric = true;
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

            return nonNumeric;

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
        private partial struct byPriorityWeight // : slice<ptr<SRV>>
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
        private partial struct byPref // : slice<ptr<MX>>
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
