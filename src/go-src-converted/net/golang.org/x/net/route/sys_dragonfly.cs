// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package route -- go2cs converted at 2022 March 06 22:15:58 UTC
// import "golang.org/x/net/route" ==> using route = go.golang.org.x.net.route_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\net\route\sys_dragonfly.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.golang.org.x.net;

public static partial class route_package {

public static bool parseable(this RIBType typ) {
    return true;
}

// RouteMetrics represents route metrics.
public partial struct RouteMetrics {
    public nint PathMTU; // path maximum transmission unit
}

// SysType implements the SysType method of Sys interface.
private static SysType SysType(this ptr<RouteMetrics> _addr_rmx) {
    ref RouteMetrics rmx = ref _addr_rmx.val;

    return SysMetrics;
}

// Sys implements the Sys method of Message interface.
private static slice<Sys> Sys(this ptr<RouteMessage> _addr_m) {
    ref RouteMessage m = ref _addr_m.val;

    return new slice<Sys>(new Sys[] { &RouteMetrics{PathMTU:int(nativeEndian.Uint64(m.raw[m.extOff+8:m.extOff+16])),} });
}

// InterfaceMetrics represents interface metrics.
public partial struct InterfaceMetrics {
    public nint Type; // interface type
    public nint MTU; // maximum transmission unit
}

// SysType implements the SysType method of Sys interface.
private static SysType SysType(this ptr<InterfaceMetrics> _addr_imx) {
    ref InterfaceMetrics imx = ref _addr_imx.val;

    return SysMetrics;
}

// Sys implements the Sys method of Message interface.
private static slice<Sys> Sys(this ptr<InterfaceMessage> _addr_m) {
    ref InterfaceMessage m = ref _addr_m.val;

    return new slice<Sys>(new Sys[] { &InterfaceMetrics{Type:int(m.raw[m.extOff]),MTU:int(nativeEndian.Uint32(m.raw[m.extOff+8:m.extOff+12])),} });
}

private static (nint, map<nint, ptr<wireFormat>>) probeRoutingStack() {
    nint _p0 = default;
    map<nint, ptr<wireFormat>> _p0 = default;

    System.UIntPtr p = default;
    ptr<wireFormat> rtm = addr(new wireFormat(extOff:40,bodyOff:sizeofRtMsghdrDragonFlyBSD4));
    rtm.parse = rtm.parseRouteMessage;
    ptr<wireFormat> ifm = addr(new wireFormat(extOff:16,bodyOff:sizeofIfMsghdrDragonFlyBSD4));
    ifm.parse = ifm.parseInterfaceMessage;
    ptr<wireFormat> ifam = addr(new wireFormat(extOff:sizeofIfaMsghdrDragonFlyBSD4,bodyOff:sizeofIfaMsghdrDragonFlyBSD4));
    ifam.parse = ifam.parseInterfaceAddrMessage;
    ptr<wireFormat> ifmam = addr(new wireFormat(extOff:sizeofIfmaMsghdrDragonFlyBSD4,bodyOff:sizeofIfmaMsghdrDragonFlyBSD4));
    ifmam.parse = ifmam.parseInterfaceMulticastAddrMessage;
    ptr<wireFormat> ifanm = addr(new wireFormat(extOff:sizeofIfAnnouncemsghdrDragonFlyBSD4,bodyOff:sizeofIfAnnouncemsghdrDragonFlyBSD4));
    ifanm.parse = ifanm.parseInterfaceAnnounceMessage;

    var (rel, _) = syscall.SysctlUint32("kern.osreldate");
    if (rel >= 500705) { 
        // https://github.com/DragonFlyBSD/DragonFlyBSD/commit/43a373152df2d405c9940983e584e6a25e76632d
        // but only the size of struct ifa_msghdr actually changed
        rtmVersion = 7;
        ifam.bodyOff = sizeofIfaMsghdrDragonFlyBSD58;

    }
    return (int(@unsafe.Sizeof(p)), /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<nint, ptr<wireFormat>>{sysRTM_ADD:rtm,sysRTM_DELETE:rtm,sysRTM_CHANGE:rtm,sysRTM_GET:rtm,sysRTM_LOSING:rtm,sysRTM_REDIRECT:rtm,sysRTM_MISS:rtm,sysRTM_LOCK:rtm,sysRTM_RESOLVE:rtm,sysRTM_NEWADDR:ifam,sysRTM_DELADDR:ifam,sysRTM_IFINFO:ifm,sysRTM_NEWMADDR:ifmam,sysRTM_DELMADDR:ifmam,sysRTM_IFANNOUNCE:ifanm,});

}

} // end route_package
