// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build solaris

// package lif -- go2cs converted at 2020 October 08 05:01:25 UTC
// import "vendor/golang.org/x/net/lif" ==> using lif = go.vendor.golang.org.x.net.lif_package
// Original source: C:\Go\src\vendor\golang.org\x\net\lif\link.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace net
{
    public static partial class lif_package
    {
        // A Link represents logical data link information.
        //
        // It also represents base information for logical network interface.
        // On Solaris, each logical network interface represents network layer
        // adjacency information and the interface has a only single network
        // address or address pair for tunneling. It's usual that multiple
        // logical network interfaces share the same logical data link.
        public partial struct Link
        {
            public @string Name; // name, equivalent to IP interface name
            public long Index; // index, equivalent to IP interface index
            public long Type; // type
            public long Flags; // flags
            public long MTU; // maximum transmission unit, basically link MTU but may differ between IP address families
            public slice<byte> Addr; // address
        }

        private static void fetch(this ptr<Link> _addr_ll, System.UIntPtr s)
        {
            ref Link ll = ref _addr_ll.val;

            ref lifreq lifr = ref heap(out ptr<lifreq> _addr_lifr);
            for (long i = 0L; i < len(ll.Name); i++)
            {
                lifr.Name[i] = int8(ll.Name[i]);
            }

            var ioc = int64(sysSIOCGLIFINDEX);
            {
                var err__prev1 = err;

                var err = ioctl(s, uintptr(ioc), @unsafe.Pointer(_addr_lifr));

                if (err == null)
                {
                    ll.Index = int(nativeEndian.Uint32(lifr.Lifru[..4L]));
                }

                err = err__prev1;

            }

            ioc = int64(sysSIOCGLIFFLAGS);
            {
                var err__prev1 = err;

                err = ioctl(s, uintptr(ioc), @unsafe.Pointer(_addr_lifr));

                if (err == null)
                {
                    ll.Flags = int(nativeEndian.Uint64(lifr.Lifru[..8L]));
                }

                err = err__prev1;

            }

            ioc = int64(sysSIOCGLIFMTU);
            {
                var err__prev1 = err;

                err = ioctl(s, uintptr(ioc), @unsafe.Pointer(_addr_lifr));

                if (err == null)
                {
                    ll.MTU = int(nativeEndian.Uint32(lifr.Lifru[..4L]));
                }

                err = err__prev1;

            }


            if (ll.Type == sysIFT_IPV4 || ll.Type == sysIFT_IPV6 || ll.Type == sysIFT_6TO4)             else 
                ioc = int64(sysSIOCGLIFHWADDR);
                {
                    var err__prev1 = err;

                    err = ioctl(s, uintptr(ioc), @unsafe.Pointer(_addr_lifr));

                    if (err == null)
                    {
                        ll.Addr, _ = parseLinkAddr(lifr.Lifru[4L..]);
                    }

                    err = err__prev1;

                }

                    }

        // Links returns a list of logical data links.
        //
        // The provided af must be an address family and name must be a data
        // link name. The zero value of af or name means a wildcard.
        public static (slice<Link>, error) Links(long af, @string name) => func((defer, _, __) =>
        {
            slice<Link> _p0 = default;
            error _p0 = default!;

            var (eps, err) = newEndpoints(af);
            if (len(eps) == 0L)
            {
                return (null, error.As(err)!);
            }

            defer(() =>
            {
                foreach (var (_, ep) in eps)
                {
                    ep.close();
                }

            }());
            return links(eps, name);

        });

        private static (slice<Link>, error) links(slice<endpoint> eps, @string name)
        {
            slice<Link> _p0 = default;
            error _p0 = default!;

            slice<Link> lls = default;
            ref lifnum lifn = ref heap(new lifnum(Flags:sysLIFC_NOXMIT|sysLIFC_TEMPORARY|sysLIFC_ALLZONES|sysLIFC_UNDER_IPMP), out ptr<lifnum> _addr_lifn);
            ref lifconf lifc = ref heap(new lifconf(Flags:sysLIFC_NOXMIT|sysLIFC_TEMPORARY|sysLIFC_ALLZONES|sysLIFC_UNDER_IPMP), out ptr<lifconf> _addr_lifc);
            foreach (var (_, ep) in eps)
            {
                lifn.Family = uint16(ep.af);
                var ioc = int64(sysSIOCGLIFNUM);
                {
                    var err__prev1 = err;

                    var err = ioctl(ep.s, uintptr(ioc), @unsafe.Pointer(_addr_lifn));

                    if (err != null)
                    {
                        continue;
                    }

                    err = err__prev1;

                }

                if (lifn.Count == 0L)
                {
                    continue;
                }

                var b = make_slice<byte>(lifn.Count * sizeofLifreq);
                lifc.Family = uint16(ep.af);
                lifc.Len = lifn.Count * sizeofLifreq;
                if (len(lifc.Lifcu) == 8L)
                {
                    nativeEndian.PutUint64(lifc.Lifcu[..], uint64(uintptr(@unsafe.Pointer(_addr_b[0L]))));
                }
                else
                {
                    nativeEndian.PutUint32(lifc.Lifcu[..], uint32(uintptr(@unsafe.Pointer(_addr_b[0L]))));
                }

                ioc = int64(sysSIOCGLIFCONF);
                {
                    var err__prev1 = err;

                    err = ioctl(ep.s, uintptr(ioc), @unsafe.Pointer(_addr_lifc));

                    if (err != null)
                    {
                        continue;
                    }

                    err = err__prev1;

                }

                var nb = make_slice<byte>(32L); // see LIFNAMSIZ in net/if.h
                {
                    long i__prev2 = i;

                    for (long i = 0L; i < int(lifn.Count); i++)
                    {
                        var lifr = (lifreq.val)(@unsafe.Pointer(_addr_b[i * sizeofLifreq]));
                        {
                            long i__prev3 = i;

                            for (i = 0L; i < 32L; i++)
                            {
                                if (lifr.Name[i] == 0L)
                                {
                                    nb = nb[..i];
                                    break;
                                }

                                nb[i] = byte(lifr.Name[i]);

                            }


                            i = i__prev3;
                        }
                        var llname = string(nb);
                        nb = nb[..32L];
                        if (isDupLink(lls, llname) || name != "" && name != llname)
                        {
                            continue;
                        }

                        Link ll = new Link(Name:llname,Type:int(lifr.Type));
                        ll.fetch(ep.s);
                        lls = append(lls, ll);

                    }


                    i = i__prev2;
                }

            }
            return (lls, error.As(null!)!);

        }

        private static bool isDupLink(slice<Link> lls, @string name)
        {
            foreach (var (_, ll) in lls)
            {
                if (ll.Name == name)
                {
                    return true;
                }

            }
            return false;

        }
    }
}}}}}
