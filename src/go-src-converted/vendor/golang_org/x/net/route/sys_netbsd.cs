// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package route -- go2cs converted at 2020 August 29 10:12:42 UTC
// import "vendor/golang_org/x/net/route" ==> using route = go.vendor.golang_org.x.net.route_package
// Original source: C:\Go\src\vendor\golang_org\x\net\route\sys_netbsd.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
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
        private static SysType SysType(this ref RouteMetrics rmx)
        {
            return SysMetrics;
        }

        // Sys implements the Sys method of Message interface.
        private static slice<Sys> Sys(this ref RouteMessage m)
        {
            return new slice<Sys>(new Sys[] { &RouteMetrics{PathMTU:int(nativeEndian.Uint64(m.raw[m.extOff+8:m.extOff+16])),} });
        }

        // RouteMetrics represents route metrics.
        public partial struct InterfaceMetrics
        {
            public long Type; // interface type
            public long MTU; // maximum transmission unit
        }

        // SysType implements the SysType method of Sys interface.
        private static SysType SysType(this ref InterfaceMetrics imx)
        {
            return SysMetrics;
        }

        // Sys implements the Sys method of Message interface.
        private static slice<Sys> Sys(this ref InterfaceMessage m)
        {
            return new slice<Sys>(new Sys[] { &InterfaceMetrics{Type:int(m.raw[m.extOff]),MTU:int(nativeEndian.Uint32(m.raw[m.extOff+8:m.extOff+12])),} });
        }

        private static (long, map<long, ref wireFormat>) probeRoutingStack()
        {
            wireFormat rtm = ref new wireFormat(extOff:40,bodyOff:sizeofRtMsghdrNetBSD7);
            rtm.parse = rtm.parseRouteMessage;
            wireFormat ifm = ref new wireFormat(extOff:16,bodyOff:sizeofIfMsghdrNetBSD7);
            ifm.parse = ifm.parseInterfaceMessage;
            wireFormat ifam = ref new wireFormat(extOff:sizeofIfaMsghdrNetBSD7,bodyOff:sizeofIfaMsghdrNetBSD7);
            ifam.parse = ifam.parseInterfaceAddrMessage;
            wireFormat ifanm = ref new wireFormat(extOff:sizeofIfAnnouncemsghdrNetBSD7,bodyOff:sizeofIfAnnouncemsghdrNetBSD7);
            ifanm.parse = ifanm.parseInterfaceAnnounceMessage; 
            // NetBSD 6 and above kernels require 64-bit aligned access to
            // routing facilities.
            return (8L, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<long, ref wireFormat>{sysRTM_ADD:rtm,sysRTM_DELETE:rtm,sysRTM_CHANGE:rtm,sysRTM_GET:rtm,sysRTM_LOSING:rtm,sysRTM_REDIRECT:rtm,sysRTM_MISS:rtm,sysRTM_LOCK:rtm,sysRTM_RESOLVE:rtm,sysRTM_NEWADDR:ifam,sysRTM_DELADDR:ifam,sysRTM_IFANNOUNCE:ifanm,sysRTM_IFINFO:ifm,});
        }
    }
}}}}}
