// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package route -- go2cs converted at 2020 October 08 05:01:46 UTC
// import "vendor/golang.org/x/net/route" ==> using route = go.vendor.golang.org.x.net.route_package
// Original source: C:\Go\src\vendor\golang.org\x\net\route\sys_freebsd.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        public static bool parseable(this RIBType typ)
        {
            return true;
        }

        // RouteMetrics represents route metrics.
        public partial struct RouteMetrics
        {
            public long PathMTU; // path maximum transmission unit
        }

        // SysType implements the SysType method of Sys interface.
        private static SysType SysType(this ptr<RouteMetrics> _addr_rmx)
        {
            ref RouteMetrics rmx = ref _addr_rmx.val;

            return SysMetrics;
        }

        // Sys implements the Sys method of Message interface.
        private static slice<Sys> Sys(this ptr<RouteMessage> _addr_m)
        {
            ref RouteMessage m = ref _addr_m.val;

            if (kernelAlign == 8L)
            {
                return new slice<Sys>(new Sys[] { &RouteMetrics{PathMTU:int(nativeEndian.Uint64(m.raw[m.extOff+8:m.extOff+16])),} });
            }

            return new slice<Sys>(new Sys[] { &RouteMetrics{PathMTU:int(nativeEndian.Uint32(m.raw[m.extOff+4:m.extOff+8])),} });

        }

        // InterfaceMetrics represents interface metrics.
        public partial struct InterfaceMetrics
        {
            public long Type; // interface type
            public long MTU; // maximum transmission unit
        }

        // SysType implements the SysType method of Sys interface.
        private static SysType SysType(this ptr<InterfaceMetrics> _addr_imx)
        {
            ref InterfaceMetrics imx = ref _addr_imx.val;

            return SysMetrics;
        }

        // Sys implements the Sys method of Message interface.
        private static slice<Sys> Sys(this ptr<InterfaceMessage> _addr_m)
        {
            ref InterfaceMessage m = ref _addr_m.val;

            return new slice<Sys>(new Sys[] { &InterfaceMetrics{Type:int(m.raw[m.extOff]),MTU:int(nativeEndian.Uint32(m.raw[m.extOff+8:m.extOff+12])),} });
        }

        private static bool compatFreeBSD32 = default; // 386 emulation on amd64

        private static (long, map<long, ptr<wireFormat>>) probeRoutingStack()
        {
            long _p0 = default;
            map<long, ptr<wireFormat>> _p0 = default;

            System.UIntPtr p = default;
            var wordSize = int(@unsafe.Sizeof(p));
            var align = wordSize; 
            // In the case of kern.supported_archs="amd64 i386", we need
            // to know the underlying kernel's architecture because the
            // alignment for routing facilities are set at the build time
            // of the kernel.
            var (conf, _) = syscall.Sysctl("kern.conftxt");
            for (long i = 0L;
            long j = 0L; j < len(conf); j++)
            {
                if (conf[j] != '\n')
                {
                    continue;
                }

                var s = conf[i..j];
                i = j + 1L;
                if (len(s) > len("machine") && s[..len("machine")] == "machine")
                {
                    s = s[len("machine")..];
                    for (long k = 0L; k < len(s); k++)
                    {
                        if (s[k] == ' ' || s[k] == '\t')
                        {
                            s = s[1L..];
                        }

                        break;

                    }

                    if (s == "amd64")
                    {
                        align = 8L;
                    }

                    break;

                }

            }

            if (align != wordSize)
            {
                compatFreeBSD32 = true; // 386 emulation on amd64
            }

            ptr<wireFormat> rtm;            ptr<wireFormat> ifm;            ptr<wireFormat> ifam;            ptr<wireFormat> ifmam;            ptr<wireFormat> ifanm;

            if (compatFreeBSD32)
            {
                rtm = addr(new wireFormat(extOff:sizeofRtMsghdrFreeBSD10Emu-sizeofRtMetricsFreeBSD10Emu,bodyOff:sizeofRtMsghdrFreeBSD10Emu));
                ifm = addr(new wireFormat(extOff:16));
                ifam = addr(new wireFormat(extOff:sizeofIfaMsghdrFreeBSD10Emu,bodyOff:sizeofIfaMsghdrFreeBSD10Emu));
                ifmam = addr(new wireFormat(extOff:sizeofIfmaMsghdrFreeBSD10Emu,bodyOff:sizeofIfmaMsghdrFreeBSD10Emu));
                ifanm = addr(new wireFormat(extOff:sizeofIfAnnouncemsghdrFreeBSD10Emu,bodyOff:sizeofIfAnnouncemsghdrFreeBSD10Emu));
            }
            else
            {
                rtm = addr(new wireFormat(extOff:sizeofRtMsghdrFreeBSD10-sizeofRtMetricsFreeBSD10,bodyOff:sizeofRtMsghdrFreeBSD10));
                ifm = addr(new wireFormat(extOff:16));
                ifam = addr(new wireFormat(extOff:sizeofIfaMsghdrFreeBSD10,bodyOff:sizeofIfaMsghdrFreeBSD10));
                ifmam = addr(new wireFormat(extOff:sizeofIfmaMsghdrFreeBSD10,bodyOff:sizeofIfmaMsghdrFreeBSD10));
                ifanm = addr(new wireFormat(extOff:sizeofIfAnnouncemsghdrFreeBSD10,bodyOff:sizeofIfAnnouncemsghdrFreeBSD10));
            }

            var (rel, _) = syscall.SysctlUint32("kern.osreldate");

            if (rel < 800000L) 
                if (compatFreeBSD32)
                {
                    ifm.bodyOff = sizeofIfMsghdrFreeBSD7Emu;
                }
                else
                {
                    ifm.bodyOff = sizeofIfMsghdrFreeBSD7;
                }

            else if (800000L <= rel && rel < 900000L) 
                if (compatFreeBSD32)
                {
                    ifm.bodyOff = sizeofIfMsghdrFreeBSD8Emu;
                }
                else
                {
                    ifm.bodyOff = sizeofIfMsghdrFreeBSD8;
                }

            else if (900000L <= rel && rel < 1000000L) 
                if (compatFreeBSD32)
                {
                    ifm.bodyOff = sizeofIfMsghdrFreeBSD9Emu;
                }
                else
                {
                    ifm.bodyOff = sizeofIfMsghdrFreeBSD9;
                }

            else if (1000000L <= rel && rel < 1100000L) 
                if (compatFreeBSD32)
                {
                    ifm.bodyOff = sizeofIfMsghdrFreeBSD10Emu;
                }
                else
                {
                    ifm.bodyOff = sizeofIfMsghdrFreeBSD10;
                }

            else 
                if (compatFreeBSD32)
                {
                    ifm.bodyOff = sizeofIfMsghdrFreeBSD11Emu;
                }
                else
                {
                    ifm.bodyOff = sizeofIfMsghdrFreeBSD11;
                }

                if (rel >= 1102000L)
                { // see https://github.com/freebsd/freebsd/commit/027c7f4d66ff8d8c4a46c3665a5ee7d6d8462034#diff-ad4e5b7f1449ea3fc87bc97280de145b
                    align = wordSize;

                }

                        rtm.parse = rtm.parseRouteMessage;
            ifm.parse = ifm.parseInterfaceMessage;
            ifam.parse = ifam.parseInterfaceAddrMessage;
            ifmam.parse = ifmam.parseInterfaceMulticastAddrMessage;
            ifanm.parse = ifanm.parseInterfaceAnnounceMessage;
            return (align, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<long, ptr<wireFormat>>{sysRTM_ADD:rtm,sysRTM_DELETE:rtm,sysRTM_CHANGE:rtm,sysRTM_GET:rtm,sysRTM_LOSING:rtm,sysRTM_REDIRECT:rtm,sysRTM_MISS:rtm,sysRTM_LOCK:rtm,sysRTM_RESOLVE:rtm,sysRTM_NEWADDR:ifam,sysRTM_DELADDR:ifam,sysRTM_IFINFO:ifm,sysRTM_NEWMADDR:ifmam,sysRTM_DELMADDR:ifmam,sysRTM_IFANNOUNCE:ifanm,});

        }
    }
}}}}}
