// Created by cgo -godefs - DO NOT EDIT
// cgo -godefs defs_freebsd.go

// package route -- go2cs converted at 2020 August 29 10:12:42 UTC
// import "vendor/golang_org/x/net/route" ==> using route = go.vendor.golang_org.x.net.route_package
// Original source: C:\Go\src\vendor\golang_org\x\net\route\zsys_freebsd_386.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang_org {
namespace x {
namespace net
{
    public static partial class route_package
    {
        private static readonly ulong sysAF_UNSPEC = 0x0UL;
        private static readonly ulong sysAF_INET = 0x2UL;
        private static readonly ulong sysAF_ROUTE = 0x11UL;
        private static readonly ulong sysAF_LINK = 0x12UL;
        private static readonly ulong sysAF_INET6 = 0x1cUL;

        private static readonly ulong sysSOCK_RAW = 0x3UL;

        private static readonly ulong sysNET_RT_DUMP = 0x1UL;
        private static readonly ulong sysNET_RT_FLAGS = 0x2UL;
        private static readonly ulong sysNET_RT_IFLIST = 0x3UL;
        private static readonly ulong sysNET_RT_IFMALIST = 0x4UL;
        private static readonly ulong sysNET_RT_IFLISTL = 0x5UL;

        private static readonly ulong sysCTL_MAXNAME = 0x18UL;

        private static readonly ulong sysCTL_UNSPEC = 0x0UL;
        private static readonly ulong sysCTL_KERN = 0x1UL;
        private static readonly ulong sysCTL_VM = 0x2UL;
        private static readonly ulong sysCTL_VFS = 0x3UL;
        private static readonly ulong sysCTL_NET = 0x4UL;
        private static readonly ulong sysCTL_DEBUG = 0x5UL;
        private static readonly ulong sysCTL_HW = 0x6UL;
        private static readonly ulong sysCTL_MACHDEP = 0x7UL;
        private static readonly ulong sysCTL_USER = 0x8UL;
        private static readonly ulong sysCTL_P1003_1B = 0x9UL;

        private static readonly ulong sysRTM_VERSION = 0x5UL;

        private static readonly ulong sysRTM_ADD = 0x1UL;
        private static readonly ulong sysRTM_DELETE = 0x2UL;
        private static readonly ulong sysRTM_CHANGE = 0x3UL;
        private static readonly ulong sysRTM_GET = 0x4UL;
        private static readonly ulong sysRTM_LOSING = 0x5UL;
        private static readonly ulong sysRTM_REDIRECT = 0x6UL;
        private static readonly ulong sysRTM_MISS = 0x7UL;
        private static readonly ulong sysRTM_LOCK = 0x8UL;
        private static readonly ulong sysRTM_RESOLVE = 0xbUL;
        private static readonly ulong sysRTM_NEWADDR = 0xcUL;
        private static readonly ulong sysRTM_DELADDR = 0xdUL;
        private static readonly ulong sysRTM_IFINFO = 0xeUL;
        private static readonly ulong sysRTM_NEWMADDR = 0xfUL;
        private static readonly ulong sysRTM_DELMADDR = 0x10UL;
        private static readonly ulong sysRTM_IFANNOUNCE = 0x11UL;
        private static readonly ulong sysRTM_IEEE80211 = 0x12UL;

        private static readonly ulong sysRTA_DST = 0x1UL;
        private static readonly ulong sysRTA_GATEWAY = 0x2UL;
        private static readonly ulong sysRTA_NETMASK = 0x4UL;
        private static readonly ulong sysRTA_GENMASK = 0x8UL;
        private static readonly ulong sysRTA_IFP = 0x10UL;
        private static readonly ulong sysRTA_IFA = 0x20UL;
        private static readonly ulong sysRTA_AUTHOR = 0x40UL;
        private static readonly ulong sysRTA_BRD = 0x80UL;

        private static readonly ulong sysRTAX_DST = 0x0UL;
        private static readonly ulong sysRTAX_GATEWAY = 0x1UL;
        private static readonly ulong sysRTAX_NETMASK = 0x2UL;
        private static readonly ulong sysRTAX_GENMASK = 0x3UL;
        private static readonly ulong sysRTAX_IFP = 0x4UL;
        private static readonly ulong sysRTAX_IFA = 0x5UL;
        private static readonly ulong sysRTAX_AUTHOR = 0x6UL;
        private static readonly ulong sysRTAX_BRD = 0x7UL;
        private static readonly ulong sysRTAX_MAX = 0x8UL;

        private static readonly ulong sizeofIfMsghdrlFreeBSD10 = 0x68UL;
        private static readonly ulong sizeofIfaMsghdrFreeBSD10 = 0x14UL;
        private static readonly ulong sizeofIfaMsghdrlFreeBSD10 = 0x6cUL;
        private static readonly ulong sizeofIfmaMsghdrFreeBSD10 = 0x10UL;
        private static readonly ulong sizeofIfAnnouncemsghdrFreeBSD10 = 0x18UL;

        private static readonly ulong sizeofRtMsghdrFreeBSD10 = 0x5cUL;
        private static readonly ulong sizeofRtMetricsFreeBSD10 = 0x38UL;

        private static readonly ulong sizeofIfMsghdrFreeBSD7 = 0x60UL;
        private static readonly ulong sizeofIfMsghdrFreeBSD8 = 0x60UL;
        private static readonly ulong sizeofIfMsghdrFreeBSD9 = 0x60UL;
        private static readonly ulong sizeofIfMsghdrFreeBSD10 = 0x64UL;
        private static readonly ulong sizeofIfMsghdrFreeBSD11 = 0xa8UL;

        private static readonly ulong sizeofIfDataFreeBSD7 = 0x50UL;
        private static readonly ulong sizeofIfDataFreeBSD8 = 0x50UL;
        private static readonly ulong sizeofIfDataFreeBSD9 = 0x50UL;
        private static readonly ulong sizeofIfDataFreeBSD10 = 0x54UL;
        private static readonly ulong sizeofIfDataFreeBSD11 = 0x98UL; 

        // MODIFIED BY HAND FOR 386 EMULATION ON AMD64
        // 386 EMULATION USES THE UNDERLYING RAW DATA LAYOUT

        private static readonly ulong sizeofIfMsghdrlFreeBSD10Emu = 0xb0UL;
        private static readonly ulong sizeofIfaMsghdrFreeBSD10Emu = 0x14UL;
        private static readonly ulong sizeofIfaMsghdrlFreeBSD10Emu = 0xb0UL;
        private static readonly ulong sizeofIfmaMsghdrFreeBSD10Emu = 0x10UL;
        private static readonly ulong sizeofIfAnnouncemsghdrFreeBSD10Emu = 0x18UL;

        private static readonly ulong sizeofRtMsghdrFreeBSD10Emu = 0x98UL;
        private static readonly ulong sizeofRtMetricsFreeBSD10Emu = 0x70UL;

        private static readonly ulong sizeofIfMsghdrFreeBSD7Emu = 0xa8UL;
        private static readonly ulong sizeofIfMsghdrFreeBSD8Emu = 0xa8UL;
        private static readonly ulong sizeofIfMsghdrFreeBSD9Emu = 0xa8UL;
        private static readonly ulong sizeofIfMsghdrFreeBSD10Emu = 0xa8UL;
        private static readonly ulong sizeofIfMsghdrFreeBSD11Emu = 0xa8UL;

        private static readonly ulong sizeofIfDataFreeBSD7Emu = 0x98UL;
        private static readonly ulong sizeofIfDataFreeBSD8Emu = 0x98UL;
        private static readonly ulong sizeofIfDataFreeBSD9Emu = 0x98UL;
        private static readonly ulong sizeofIfDataFreeBSD10Emu = 0x98UL;
        private static readonly ulong sizeofIfDataFreeBSD11Emu = 0x98UL;

        private static readonly ulong sizeofSockaddrStorage = 0x80UL;
        private static readonly ulong sizeofSockaddrInet = 0x10UL;
        private static readonly ulong sizeofSockaddrInet6 = 0x1cUL;
    }
}}}}}
