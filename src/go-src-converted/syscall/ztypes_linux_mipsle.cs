// Created by cgo -godefs - DO NOT EDIT
// cgo -godefs types_linux.go

// package syscall -- go2cs converted at 2020 August 29 08:42:12 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\ztypes_linux_mipsle.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static readonly ulong sizeofPtr = 0x4UL;
        private static readonly ulong sizeofShort = 0x2UL;
        private static readonly ulong sizeofInt = 0x4UL;
        private static readonly ulong sizeofLong = 0x4UL;
        private static readonly ulong sizeofLongLong = 0x8UL;
        public static readonly ulong PathMax = 0x1000UL;

        private partial struct _C_short // : short
        {
        }
        private partial struct _C_int // : int
        {
        }
        private partial struct _C_long // : int
        {
        }
        private partial struct _C_long_long // : long
        {
        }        public partial struct Timespec
        {
            public int Sec;
            public int Nsec;
        }

        public partial struct Timeval
        {
            public int Sec;
            public int Usec;
        }

        public partial struct Timex
        {
            public uint Modes;
            public int Offset;
            public int Freq;
            public int Maxerror;
            public int Esterror;
            public int Status;
            public int Constant;
            public int Precision;
            public int Tolerance;
            public Timeval Time;
            public int Tick;
            public int Ppsfreq;
            public int Jitter;
            public int Shift;
            public int Stabil;
            public int Jitcnt;
            public int Calcnt;
            public int Errcnt;
            public int Stbcnt;
            public int Tai;
            public array<byte> Pad_cgo_0;
        }

        public partial struct Time_t // : int
        {
        }

        public partial struct Tms
        {
            public int Utime;
            public int Stime;
            public int Cutime;
            public int Cstime;
        }

        public partial struct Utimbuf
        {
            public int Actime;
            public int Modtime;
        }

        public partial struct Rusage
        {
            public Timeval Utime;
            public Timeval Stime;
            public int Maxrss;
            public int Ixrss;
            public int Idrss;
            public int Isrss;
            public int Minflt;
            public int Majflt;
            public int Nswap;
            public int Inblock;
            public int Oublock;
            public int Msgsnd;
            public int Msgrcv;
            public int Nsignals;
            public int Nvcsw;
            public int Nivcsw;
        }

        public partial struct Rlimit
        {
            public ulong Cur;
            public ulong Max;
        }

        private partial struct _Gid_t // : uint
        {
        }

        public partial struct Stat_t
        {
            public uint Dev;
            public array<int> Pad1;
            public ulong Ino;
            public uint Mode;
            public uint Nlink;
            public uint Uid;
            public uint Gid;
            public uint Rdev;
            public array<int> Pad2;
            public long Size;
            public Timespec Atim;
            public Timespec Mtim;
            public Timespec Ctim;
            public int Blksize;
            public int Pad4;
            public long Blocks;
            public array<int> Pad5;
        }

        public partial struct Statfs_t
        {
            public int Type;
            public int Bsize;
            public int Frsize;
            public array<byte> Pad_cgo_0;
            public ulong Blocks;
            public ulong Bfree;
            public ulong Files;
            public ulong Ffree;
            public ulong Bavail;
            public Fsid Fsid;
            public int Namelen;
            public int Flags;
            public array<int> Spare;
            public array<byte> Pad_cgo_1;
        }

        public partial struct Dirent
        {
            public ulong Ino;
            public long Off;
            public ushort Reclen;
            public byte Type;
            public array<sbyte> Name;
            public array<byte> Pad_cgo_0;
        }

        public partial struct Fsid
        {
            public array<int> X__val;
        }

        public partial struct Flock_t
        {
            public short Type;
            public short Whence;
            public array<byte> Pad_cgo_0;
            public long Start;
            public long Len;
            public int Pid;
            public array<byte> Pad_cgo_1;
        }

        public partial struct RawSockaddrInet4
        {
            public ushort Family;
            public ushort Port;
            public array<byte> Addr; /* in_addr */
            public array<byte> Zero;
        }

        public partial struct RawSockaddrInet6
        {
            public ushort Family;
            public ushort Port;
            public uint Flowinfo;
            public array<byte> Addr; /* in6_addr */
            public uint Scope_id;
        }

        public partial struct RawSockaddrUnix
        {
            public ushort Family;
            public array<sbyte> Path;
        }

        public partial struct RawSockaddrLinklayer
        {
            public ushort Family;
            public ushort Protocol;
            public int Ifindex;
            public ushort Hatype;
            public byte Pkttype;
            public byte Halen;
            public array<byte> Addr;
        }

        public partial struct RawSockaddrNetlink
        {
            public ushort Family;
            public ushort Pad;
            public uint Pid;
            public uint Groups;
        }

        public partial struct RawSockaddr
        {
            public ushort Family;
            public array<sbyte> Data;
        }

        public partial struct RawSockaddrAny
        {
            public RawSockaddr Addr;
            public array<sbyte> Pad;
        }

        private partial struct _Socklen // : uint
        {
        }

        public partial struct Linger
        {
            public int Onoff;
            public int Linger;
        }

        public partial struct Iovec
        {
            public ptr<byte> Base;
            public uint Len;
        }

        public partial struct IPMreq
        {
            public array<byte> Multiaddr; /* in_addr */
            public array<byte> Interface; /* in_addr */
        }

        public partial struct IPMreqn
        {
            public array<byte> Multiaddr; /* in_addr */
            public array<byte> Address; /* in_addr */
            public int Ifindex;
        }

        public partial struct IPv6Mreq
        {
            public array<byte> Multiaddr; /* in6_addr */
            public uint Interface;
        }

        public partial struct Msghdr
        {
            public ptr<byte> Name;
            public uint Namelen;
            public ptr<Iovec> Iov;
            public uint Iovlen;
            public ptr<byte> Control;
            public uint Controllen;
            public int Flags;
        }

        public partial struct Cmsghdr
        {
            public uint Len;
            public int Level;
            public int Type;
        }

        public partial struct Inet4Pktinfo
        {
            public int Ifindex;
            public array<byte> Spec_dst; /* in_addr */
            public array<byte> Addr; /* in_addr */
        }

        public partial struct Inet6Pktinfo
        {
            public array<byte> Addr; /* in6_addr */
            public uint Ifindex;
        }

        public partial struct IPv6MTUInfo
        {
            public RawSockaddrInet6 Addr;
            public uint Mtu;
        }

        public partial struct ICMPv6Filter
        {
            public array<uint> Data;
        }

        public partial struct Ucred
        {
            public int Pid;
            public uint Uid;
            public uint Gid;
        }

        public partial struct TCPInfo
        {
            public byte State;
            public byte Ca_state;
            public byte Retransmits;
            public byte Probes;
            public byte Backoff;
            public byte Options;
            public array<byte> Pad_cgo_0;
            public uint Rto;
            public uint Ato;
            public uint Snd_mss;
            public uint Rcv_mss;
            public uint Unacked;
            public uint Sacked;
            public uint Lost;
            public uint Retrans;
            public uint Fackets;
            public uint Last_data_sent;
            public uint Last_ack_sent;
            public uint Last_data_recv;
            public uint Last_ack_recv;
            public uint Pmtu;
            public uint Rcv_ssthresh;
            public uint Rtt;
            public uint Rttvar;
            public uint Snd_ssthresh;
            public uint Snd_cwnd;
            public uint Advmss;
            public uint Reordering;
            public uint Rcv_rtt;
            public uint Rcv_space;
            public uint Total_retrans;
        }

        public static readonly ulong SizeofSockaddrInet4 = 0x10UL;
        public static readonly ulong SizeofSockaddrInet6 = 0x1cUL;
        public static readonly ulong SizeofSockaddrAny = 0x70UL;
        public static readonly ulong SizeofSockaddrUnix = 0x6eUL;
        public static readonly ulong SizeofSockaddrLinklayer = 0x14UL;
        public static readonly ulong SizeofSockaddrNetlink = 0xcUL;
        public static readonly ulong SizeofLinger = 0x8UL;
        public static readonly ulong SizeofIPMreq = 0x8UL;
        public static readonly ulong SizeofIPMreqn = 0xcUL;
        public static readonly ulong SizeofIPv6Mreq = 0x14UL;
        public static readonly ulong SizeofMsghdr = 0x1cUL;
        public static readonly ulong SizeofCmsghdr = 0xcUL;
        public static readonly ulong SizeofInet4Pktinfo = 0xcUL;
        public static readonly ulong SizeofInet6Pktinfo = 0x14UL;
        public static readonly ulong SizeofIPv6MTUInfo = 0x20UL;
        public static readonly ulong SizeofICMPv6Filter = 0x20UL;
        public static readonly ulong SizeofUcred = 0xcUL;
        public static readonly ulong SizeofTCPInfo = 0x68UL;

        public static readonly ulong IFA_UNSPEC = 0x0UL;
        public static readonly ulong IFA_ADDRESS = 0x1UL;
        public static readonly ulong IFA_LOCAL = 0x2UL;
        public static readonly ulong IFA_LABEL = 0x3UL;
        public static readonly ulong IFA_BROADCAST = 0x4UL;
        public static readonly ulong IFA_ANYCAST = 0x5UL;
        public static readonly ulong IFA_CACHEINFO = 0x6UL;
        public static readonly ulong IFA_MULTICAST = 0x7UL;
        public static readonly ulong IFLA_UNSPEC = 0x0UL;
        public static readonly ulong IFLA_ADDRESS = 0x1UL;
        public static readonly ulong IFLA_BROADCAST = 0x2UL;
        public static readonly ulong IFLA_IFNAME = 0x3UL;
        public static readonly ulong IFLA_MTU = 0x4UL;
        public static readonly ulong IFLA_LINK = 0x5UL;
        public static readonly ulong IFLA_QDISC = 0x6UL;
        public static readonly ulong IFLA_STATS = 0x7UL;
        public static readonly ulong IFLA_COST = 0x8UL;
        public static readonly ulong IFLA_PRIORITY = 0x9UL;
        public static readonly ulong IFLA_MASTER = 0xaUL;
        public static readonly ulong IFLA_WIRELESS = 0xbUL;
        public static readonly ulong IFLA_PROTINFO = 0xcUL;
        public static readonly ulong IFLA_TXQLEN = 0xdUL;
        public static readonly ulong IFLA_MAP = 0xeUL;
        public static readonly ulong IFLA_WEIGHT = 0xfUL;
        public static readonly ulong IFLA_OPERSTATE = 0x10UL;
        public static readonly ulong IFLA_LINKMODE = 0x11UL;
        public static readonly ulong IFLA_LINKINFO = 0x12UL;
        public static readonly ulong IFLA_NET_NS_PID = 0x13UL;
        public static readonly ulong IFLA_IFALIAS = 0x14UL;
        public static readonly ulong IFLA_MAX = 0x27UL;
        public static readonly ulong RT_SCOPE_UNIVERSE = 0x0UL;
        public static readonly ulong RT_SCOPE_SITE = 0xc8UL;
        public static readonly ulong RT_SCOPE_LINK = 0xfdUL;
        public static readonly ulong RT_SCOPE_HOST = 0xfeUL;
        public static readonly ulong RT_SCOPE_NOWHERE = 0xffUL;
        public static readonly ulong RT_TABLE_UNSPEC = 0x0UL;
        public static readonly ulong RT_TABLE_COMPAT = 0xfcUL;
        public static readonly ulong RT_TABLE_DEFAULT = 0xfdUL;
        public static readonly ulong RT_TABLE_MAIN = 0xfeUL;
        public static readonly ulong RT_TABLE_LOCAL = 0xffUL;
        public static readonly ulong RT_TABLE_MAX = 0xffffffffUL;
        public static readonly ulong RTA_UNSPEC = 0x0UL;
        public static readonly ulong RTA_DST = 0x1UL;
        public static readonly ulong RTA_SRC = 0x2UL;
        public static readonly ulong RTA_IIF = 0x3UL;
        public static readonly ulong RTA_OIF = 0x4UL;
        public static readonly ulong RTA_GATEWAY = 0x5UL;
        public static readonly ulong RTA_PRIORITY = 0x6UL;
        public static readonly ulong RTA_PREFSRC = 0x7UL;
        public static readonly ulong RTA_METRICS = 0x8UL;
        public static readonly ulong RTA_MULTIPATH = 0x9UL;
        public static readonly ulong RTA_FLOW = 0xbUL;
        public static readonly ulong RTA_CACHEINFO = 0xcUL;
        public static readonly ulong RTA_TABLE = 0xfUL;
        public static readonly ulong RTN_UNSPEC = 0x0UL;
        public static readonly ulong RTN_UNICAST = 0x1UL;
        public static readonly ulong RTN_LOCAL = 0x2UL;
        public static readonly ulong RTN_BROADCAST = 0x3UL;
        public static readonly ulong RTN_ANYCAST = 0x4UL;
        public static readonly ulong RTN_MULTICAST = 0x5UL;
        public static readonly ulong RTN_BLACKHOLE = 0x6UL;
        public static readonly ulong RTN_UNREACHABLE = 0x7UL;
        public static readonly ulong RTN_PROHIBIT = 0x8UL;
        public static readonly ulong RTN_THROW = 0x9UL;
        public static readonly ulong RTN_NAT = 0xaUL;
        public static readonly ulong RTN_XRESOLVE = 0xbUL;
        public static readonly ulong RTNLGRP_NONE = 0x0UL;
        public static readonly ulong RTNLGRP_LINK = 0x1UL;
        public static readonly ulong RTNLGRP_NOTIFY = 0x2UL;
        public static readonly ulong RTNLGRP_NEIGH = 0x3UL;
        public static readonly ulong RTNLGRP_TC = 0x4UL;
        public static readonly ulong RTNLGRP_IPV4_IFADDR = 0x5UL;
        public static readonly ulong RTNLGRP_IPV4_MROUTE = 0x6UL;
        public static readonly ulong RTNLGRP_IPV4_ROUTE = 0x7UL;
        public static readonly ulong RTNLGRP_IPV4_RULE = 0x8UL;
        public static readonly ulong RTNLGRP_IPV6_IFADDR = 0x9UL;
        public static readonly ulong RTNLGRP_IPV6_MROUTE = 0xaUL;
        public static readonly ulong RTNLGRP_IPV6_ROUTE = 0xbUL;
        public static readonly ulong RTNLGRP_IPV6_IFINFO = 0xcUL;
        public static readonly ulong RTNLGRP_IPV6_PREFIX = 0x12UL;
        public static readonly ulong RTNLGRP_IPV6_RULE = 0x13UL;
        public static readonly ulong RTNLGRP_ND_USEROPT = 0x14UL;
        public static readonly ulong SizeofNlMsghdr = 0x10UL;
        public static readonly ulong SizeofNlMsgerr = 0x14UL;
        public static readonly ulong SizeofRtGenmsg = 0x1UL;
        public static readonly ulong SizeofNlAttr = 0x4UL;
        public static readonly ulong SizeofRtAttr = 0x4UL;
        public static readonly ulong SizeofIfInfomsg = 0x10UL;
        public static readonly ulong SizeofIfAddrmsg = 0x8UL;
        public static readonly ulong SizeofRtMsg = 0xcUL;
        public static readonly ulong SizeofRtNexthop = 0x8UL;

        public partial struct NlMsghdr
        {
            public uint Len;
            public ushort Type;
            public ushort Flags;
            public uint Seq;
            public uint Pid;
        }

        public partial struct NlMsgerr
        {
            public int Error;
            public NlMsghdr Msg;
        }

        public partial struct RtGenmsg
        {
            public byte Family;
        }

        public partial struct NlAttr
        {
            public ushort Len;
            public ushort Type;
        }

        public partial struct RtAttr
        {
            public ushort Len;
            public ushort Type;
        }

        public partial struct IfInfomsg
        {
            public byte Family;
            public byte X__ifi_pad;
            public ushort Type;
            public int Index;
            public uint Flags;
            public uint Change;
        }

        public partial struct IfAddrmsg
        {
            public byte Family;
            public byte Prefixlen;
            public byte Flags;
            public byte Scope;
            public uint Index;
        }

        public partial struct RtMsg
        {
            public byte Family;
            public byte Dst_len;
            public byte Src_len;
            public byte Tos;
            public byte Table;
            public byte Protocol;
            public byte Scope;
            public byte Type;
            public uint Flags;
        }

        public partial struct RtNexthop
        {
            public ushort Len;
            public byte Flags;
            public byte Hops;
            public int Ifindex;
        }

        public static readonly ulong SizeofSockFilter = 0x8UL;
        public static readonly ulong SizeofSockFprog = 0x8UL;

        public partial struct SockFilter
        {
            public ushort Code;
            public byte Jt;
            public byte Jf;
            public uint K;
        }

        public partial struct SockFprog
        {
            public ushort Len;
            public array<byte> Pad_cgo_0;
            public ptr<SockFilter> Filter;
        }

        public partial struct InotifyEvent
        {
            public int Wd;
            public uint Mask;
            public uint Cookie;
            public uint Len;
        }

        public static readonly ulong SizeofInotifyEvent = 0x10UL;



        public partial struct PtraceRegs
        {
            public array<uint> Regs;
            public uint U_tsize;
            public uint U_dsize;
            public uint U_ssize;
            public uint Start_code;
            public uint Start_data;
            public uint Start_stack;
            public int Signal;
            public ptr<byte> U_ar0;
            public uint Magic;
            public array<sbyte> U_comm;
        }

        public partial struct FdSet
        {
            public array<int> Bits;
        }

        public partial struct Sysinfo_t
        {
            public int Uptime;
            public array<uint> Loads;
            public uint Totalram;
            public uint Freeram;
            public uint Sharedram;
            public uint Bufferram;
            public uint Totalswap;
            public uint Freeswap;
            public ushort Procs;
            public ushort Pad;
            public uint Totalhigh;
            public uint Freehigh;
            public uint Unit;
            public array<sbyte> X_f;
        }

        public partial struct Utsname
        {
            public array<sbyte> Sysname;
            public array<sbyte> Nodename;
            public array<sbyte> Release;
            public array<sbyte> Version;
            public array<sbyte> Machine;
            public array<sbyte> Domainname;
        }

        public partial struct Ustat_t
        {
            public int Tfree;
            public uint Tinode;
            public array<sbyte> Fname;
            public array<sbyte> Fpack;
        }

        public partial struct EpollEvent
        {
            public uint Events;
            public int PadFd;
            public int Fd;
            public int Pad;
        }

        private static readonly ulong _AT_FDCWD = -0x64UL;
        private static readonly ulong _AT_REMOVEDIR = 0x200UL;
        private static readonly ulong _AT_SYMLINK_NOFOLLOW = 0x100UL;

        public partial struct Termios
        {
            public uint Iflag;
            public uint Oflag;
            public uint Cflag;
            public uint Lflag;
            public byte Line;
            public array<byte> Cc;
            public array<byte> Pad_cgo_0;
        }

        public static readonly ulong IUCLC = 0x200UL;
        public static readonly ulong OLCUC = 0x2UL;
        public static readonly ulong TCGETS = 0x540dUL;
        public static readonly ulong TCSETS = 0x540eUL;
        public static readonly ulong XCASE = 0x4UL;
    }
}
