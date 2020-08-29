// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build solaris

// package lif -- go2cs converted at 2020 August 29 10:12:14 UTC
// import "vendor/golang_org/x/net/lif" ==> using lif = go.vendor.golang_org.x.net.lif_package
// Original source: C:\Go\src\vendor\golang_org\x\net\lif\address.go
using errors = go.errors_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class lif_package
    {
        // An Addr represents an address associated with packet routing.
        public partial interface Addr
        {
            long Family();
        }

        // An Inet4Addr represents an internet address for IPv4.
        public partial struct Inet4Addr
        {
            public array<byte> IP; // IP address
            public long PrefixLen; // address prefix length
        }

        // Family implements the Family method of Addr interface.
        private static long Family(this ref Inet4Addr a)
        {
            return sysAF_INET;
        }

        // An Inet6Addr represents an internet address for IPv6.
        public partial struct Inet6Addr
        {
            public array<byte> IP; // IP address
            public long PrefixLen; // address prefix length
            public long ZoneID; // zone identifier
        }

        // Family implements the Family method of Addr interface.
        private static long Family(this ref Inet6Addr a)
        {
            return sysAF_INET6;
        }

        // Addrs returns a list of interface addresses.
        //
        // The provided af must be an address family and name must be a data
        // link name. The zero value of af or name means a wildcard.
        public static (slice<Addr>, error) Addrs(long af, @string name) => func((defer, _, __) =>
        {
            var (eps, err) = newEndpoints(af);
            if (len(eps) == 0L)
            {
                return (null, err);
            }
            defer(() =>
            {
                {
                    var ep__prev1 = ep;

                    foreach (var (_, __ep) in eps)
                    {
                        ep = __ep;
                        ep.close();
                    }

                    ep = ep__prev1;
                }

            }());
            var (lls, err) = links(eps, name);
            if (len(lls) == 0L)
            {
                return (null, err);
            }
            slice<Addr> @as = default;
            foreach (var (_, ll) in lls)
            {
                lifreq lifr = default;
                for (long i = 0L; i < len(ll.Name); i++)
                {
                    lifr.Name[i] = int8(ll.Name[i]);
                }

                {
                    var ep__prev2 = ep;

                    foreach (var (_, __ep) in eps)
                    {
                        ep = __ep;
                        var ioc = int64(sysSIOCGLIFADDR);
                        var err = ioctl(ep.s, uintptr(ioc), @unsafe.Pointer(ref lifr));
                        if (err != null)
                        {
                            continue;
                        }
                        var sa = (sockaddrStorage.Value)(@unsafe.Pointer(ref lifr.Lifru[0L]));
                        var l = int(nativeEndian.Uint32(lifr.Lifru1[..4L]));
                        if (l == 0L)
                        {
                            continue;
                        }

                        if (sa.Family == sysAF_INET) 
                            Inet4Addr a = ref new Inet4Addr(PrefixLen:l);
                            copy(a.IP[..], lifr.Lifru[4L..8L]);
                            as = append(as, a);
                        else if (sa.Family == sysAF_INET6) 
                            a = ref new Inet6Addr(PrefixLen:l,ZoneID:int(nativeEndian.Uint32(lifr.Lifru[24:28])));
                            copy(a.IP[..], lifr.Lifru[8L..24L]);
                            as = append(as, a);
                                            }

                    ep = ep__prev2;
                }

            }
            return (as, null);
        });

        private static (slice<byte>, error) parseLinkAddr(slice<byte> b)
        {
            var nlen = int(b[1L]);
            var alen = int(b[2L]);
            var slen = int(b[3L]);
            long l = 4L + nlen + alen + slen;
            if (len(b) < l)
            {
                return (null, errors.New("invalid address"));
            }
            b = b[4L..];
            slice<byte> addr = default;
            if (nlen > 0L)
            {
                b = b[nlen..];
            }
            if (alen > 0L)
            {
                addr = make_slice<byte>(alen);
                copy(addr, b[..alen]);
            }
            return (addr, null);
        }
    }
}}}}}
