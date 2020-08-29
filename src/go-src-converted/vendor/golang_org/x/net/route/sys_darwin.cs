// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package route -- go2cs converted at 2020 August 29 10:12:39 UTC
// import "vendor/golang_org/x/net/route" ==> using route = go.vendor.golang_org.x.net.route_package
// Original source: C:\Go\src\vendor\golang_org\x\net\route\sys_darwin.go

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

            if (typ == sysNET_RT_STAT || typ == sysNET_RT_TRASH) 
                return false;
            else 
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
            return new slice<Sys>(new Sys[] { &RouteMetrics{PathMTU:int(nativeEndian.Uint32(m.raw[m.extOff+4:m.extOff+8])),} });
        }

        // InterfaceMetrics represents interface metrics.
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
            wireFormat rtm = ref new wireFormat(extOff:36,bodyOff:sizeofRtMsghdrDarwin15);
            rtm.parse = rtm.parseRouteMessage;
            wireFormat rtm2 = ref new wireFormat(extOff:36,bodyOff:sizeofRtMsghdr2Darwin15);
            rtm2.parse = rtm2.parseRouteMessage;
            wireFormat ifm = ref new wireFormat(extOff:16,bodyOff:sizeofIfMsghdrDarwin15);
            ifm.parse = ifm.parseInterfaceMessage;
            wireFormat ifm2 = ref new wireFormat(extOff:32,bodyOff:sizeofIfMsghdr2Darwin15);
            ifm2.parse = ifm2.parseInterfaceMessage;
            wireFormat ifam = ref new wireFormat(extOff:sizeofIfaMsghdrDarwin15,bodyOff:sizeofIfaMsghdrDarwin15);
            ifam.parse = ifam.parseInterfaceAddrMessage;
            wireFormat ifmam = ref new wireFormat(extOff:sizeofIfmaMsghdrDarwin15,bodyOff:sizeofIfmaMsghdrDarwin15);
            ifmam.parse = ifmam.parseInterfaceMulticastAddrMessage;
            wireFormat ifmam2 = ref new wireFormat(extOff:sizeofIfmaMsghdr2Darwin15,bodyOff:sizeofIfmaMsghdr2Darwin15);
            ifmam2.parse = ifmam2.parseInterfaceMulticastAddrMessage; 
            // Darwin kernels require 32-bit aligned access to routing facilities.
            return (4L, /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<long, ref wireFormat>{sysRTM_ADD:rtm,sysRTM_DELETE:rtm,sysRTM_CHANGE:rtm,sysRTM_GET:rtm,sysRTM_LOSING:rtm,sysRTM_REDIRECT:rtm,sysRTM_MISS:rtm,sysRTM_LOCK:rtm,sysRTM_RESOLVE:rtm,sysRTM_NEWADDR:ifam,sysRTM_DELADDR:ifam,sysRTM_IFINFO:ifm,sysRTM_NEWMADDR:ifmam,sysRTM_DELMADDR:ifmam,sysRTM_IFINFO2:ifm2,sysRTM_NEWMADDR2:ifmam2,sysRTM_GET2:rtm2,});
        }
    }
}}}}}
