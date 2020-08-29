// Created by cgo -godefs - DO NOT EDIT
// cgo -godefs defs_openbsd.go

// package route -- go2cs converted at 2020 August 29 10:12:42 UTC
// import "vendor/golang_org/x/net/route" ==> using route = go.vendor.golang_org.x.net.route_package
// Original source: C:\Go\src\vendor\golang_org\x\net\route\zsys_openbsd.go

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
        private static readonly ulong sysAF_INET6 = 0x18UL;

        private static readonly ulong sysSOCK_RAW = 0x3UL;

        private static readonly ulong sysNET_RT_DUMP = 0x1UL;
        private static readonly ulong sysNET_RT_FLAGS = 0x2UL;
        private static readonly ulong sysNET_RT_IFLIST = 0x3UL;
        private static readonly ulong sysNET_RT_STATS = 0x4UL;
        private static readonly ulong sysNET_RT_TABLE = 0x5UL;
        private static readonly ulong sysNET_RT_IFNAMES = 0x6UL;
        private static readonly ulong sysNET_RT_MAXID = 0x7UL;

        private static readonly ulong sysCTL_MAXNAME = 0xcUL;

        private static readonly ulong sysCTL_UNSPEC = 0x0UL;
        private static readonly ulong sysCTL_KERN = 0x1UL;
        private static readonly ulong sysCTL_VM = 0x2UL;
        private static readonly ulong sysCTL_FS = 0x3UL;
        private static readonly ulong sysCTL_NET = 0x4UL;
        private static readonly ulong sysCTL_DEBUG = 0x5UL;
        private static readonly ulong sysCTL_HW = 0x6UL;
        private static readonly ulong sysCTL_MACHDEP = 0x7UL;
        private static readonly ulong sysCTL_DDB = 0x9UL;
        private static readonly ulong sysCTL_VFS = 0xaUL;
        private static readonly ulong sysCTL_MAXID = 0xbUL;

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
        private static readonly ulong sysRTM_IFANNOUNCE = 0xfUL;
        private static readonly ulong sysRTM_DESYNC = 0x10UL;
        private static readonly ulong sysRTM_INVALIDATE = 0x11UL;
        private static readonly ulong sysRTM_BFD = 0x12UL;
        private static readonly ulong sysRTM_PROPOSAL = 0x13UL;

        private static readonly ulong sysRTA_DST = 0x1UL;
        private static readonly ulong sysRTA_GATEWAY = 0x2UL;
        private static readonly ulong sysRTA_NETMASK = 0x4UL;
        private static readonly ulong sysRTA_GENMASK = 0x8UL;
        private static readonly ulong sysRTA_IFP = 0x10UL;
        private static readonly ulong sysRTA_IFA = 0x20UL;
        private static readonly ulong sysRTA_AUTHOR = 0x40UL;
        private static readonly ulong sysRTA_BRD = 0x80UL;
        private static readonly ulong sysRTA_SRC = 0x100UL;
        private static readonly ulong sysRTA_SRCMASK = 0x200UL;
        private static readonly ulong sysRTA_LABEL = 0x400UL;
        private static readonly ulong sysRTA_BFD = 0x800UL;
        private static readonly ulong sysRTA_DNS = 0x1000UL;
        private static readonly ulong sysRTA_STATIC = 0x2000UL;
        private static readonly ulong sysRTA_SEARCH = 0x4000UL;

        private static readonly ulong sysRTAX_DST = 0x0UL;
        private static readonly ulong sysRTAX_GATEWAY = 0x1UL;
        private static readonly ulong sysRTAX_NETMASK = 0x2UL;
        private static readonly ulong sysRTAX_GENMASK = 0x3UL;
        private static readonly ulong sysRTAX_IFP = 0x4UL;
        private static readonly ulong sysRTAX_IFA = 0x5UL;
        private static readonly ulong sysRTAX_AUTHOR = 0x6UL;
        private static readonly ulong sysRTAX_BRD = 0x7UL;
        private static readonly ulong sysRTAX_SRC = 0x8UL;
        private static readonly ulong sysRTAX_SRCMASK = 0x9UL;
        private static readonly ulong sysRTAX_LABEL = 0xaUL;
        private static readonly ulong sysRTAX_BFD = 0xbUL;
        private static readonly ulong sysRTAX_DNS = 0xcUL;
        private static readonly ulong sysRTAX_STATIC = 0xdUL;
        private static readonly ulong sysRTAX_SEARCH = 0xeUL;
        private static readonly ulong sysRTAX_MAX = 0xfUL;

        private static readonly ulong sizeofRtMsghdr = 0x60UL;

        private static readonly ulong sizeofSockaddrStorage = 0x100UL;
        private static readonly ulong sizeofSockaddrInet = 0x10UL;
        private static readonly ulong sizeofSockaddrInet6 = 0x1cUL;
    }
}}}}}
