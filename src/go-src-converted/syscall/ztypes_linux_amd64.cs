// Created by cgo -godefs - DO NOT EDIT
// cgo -godefs types_linux.go

// +build amd64,linux

// package syscall -- go2cs converted at 2020 October 08 03:30:35 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\ztypes_linux_amd64.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static readonly ulong sizeofPtr = (ulong)0x8UL;
        private static readonly ulong sizeofShort = (ulong)0x2UL;
        private static readonly ulong sizeofInt = (ulong)0x4UL;
        private static readonly ulong sizeofLong = (ulong)0x8UL;
        private static readonly ulong sizeofLongLong = (ulong)0x8UL;
        public static readonly ulong PathMax = (ulong)0x1000UL;


        private partial struct _C_short // : short
        {
        }
        private partial struct _C_int // : int
        {
        }
        private partial struct _C_long // : long
        {
        }
        private partial struct _C_long_long // : long
        {
        }
        public partial struct Timespec
        {
            public long Sec;
            public long Nsec;
        }

        public partial struct Timeval
        {
            public long Sec;
            public long Usec;
        }

        public partial struct Timex
        {
            public uint Modes;
            public array<byte> Pad_cgo_0;
            public long Offset;
            public long Freq;
            public long Maxerror;
            public long Esterror;
            public int Status;
            public array<byte> Pad_cgo_1;
            public long Constant;
            public long Precision;
            public long Tolerance;
            public Timeval Time;
            public long Tick;
            public long Ppsfreq;
            public long Jitter;
            public int Shift;
            public array<byte> Pad_cgo_2;
            public long Stabil;
            public long Jitcnt;
            public long Calcnt;
            public long Errcnt;
            public long Stbcnt;
            public int Tai;
            public array<byte> Pad_cgo_3;
        }

        public partial struct Time_t // : long
        {
        }

        public partial struct Tms
        {
            public long Utime;
            public long Stime;
            public long Cutime;
            public long Cstime;
        }

        public partial struct Utimbuf
        {
            public long Actime;
            public long Modtime;
        }

        public partial struct Rusage
        {
            public Timeval Utime;
            public Timeval Stime;
            public long Maxrss;
            public long Ixrss;
            public long Idrss;
            public long Isrss;
            public long Minflt;
            public long Majflt;
            public long Nswap;
            public long Inblock;
            public long Oublock;
            public long Msgsnd;
            public long Msgrcv;
            public long Nsignals;
            public long Nvcsw;
            public long Nivcsw;
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
            public ulong Dev;
            public ulong Ino;
            public ulong Nlink;
            public uint Mode;
            public uint Uid;
            public uint Gid;
            public int X__pad0;
            public ulong Rdev;
            public long Size;
            public long Blksize;
            public long Blocks;
            public Timespec Atim;
            public Timespec Mtim;
            public Timespec Ctim;
            public array<long> X__unused;
        }

        public partial struct Statfs_t
        {
            public long Type;
            public long Bsize;
            public ulong Blocks;
            public ulong Bfree;
            public ulong Bavail;
            public ulong Files;
            public ulong Ffree;
            public Fsid Fsid;
            public long Namelen;
            public long Frsize;
            public long Flags;
            public array<long> Spare;
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
            public ulong Len;
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
            public array<byte> Pad_cgo_0;
            public ptr<Iovec> Iov;
            public ulong Iovlen;
            public ptr<byte> Control;
            public ulong Controllen;
            public int Flags;
            public array<byte> Pad_cgo_1;
        }

        public partial struct Cmsghdr
        {
            public ulong Len;
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

        public static readonly ulong SizeofSockaddrInet4 = (ulong)0x10UL;
        public static readonly ulong SizeofSockaddrInet6 = (ulong)0x1cUL;
        public static readonly ulong SizeofSockaddrAny = (ulong)0x70UL;
        public static readonly ulong SizeofSockaddrUnix = (ulong)0x6eUL;
        public static readonly ulong SizeofSockaddrLinklayer = (ulong)0x14UL;
        public static readonly ulong SizeofSockaddrNetlink = (ulong)0xcUL;
        public static readonly ulong SizeofLinger = (ulong)0x8UL;
        public static readonly ulong SizeofIPMreq = (ulong)0x8UL;
        public static readonly ulong SizeofIPMreqn = (ulong)0xcUL;
        public static readonly ulong SizeofIPv6Mreq = (ulong)0x14UL;
        public static readonly ulong SizeofMsghdr = (ulong)0x38UL;
        public static readonly ulong SizeofCmsghdr = (ulong)0x10UL;
        public static readonly ulong SizeofInet4Pktinfo = (ulong)0xcUL;
        public static readonly ulong SizeofInet6Pktinfo = (ulong)0x14UL;
        public static readonly ulong SizeofIPv6MTUInfo = (ulong)0x20UL;
        public static readonly ulong SizeofICMPv6Filter = (ulong)0x20UL;
        public static readonly ulong SizeofUcred = (ulong)0xcUL;
        public static readonly ulong SizeofTCPInfo = (ulong)0x68UL;


        public static readonly ulong IFA_UNSPEC = (ulong)0x0UL;
        public static readonly ulong IFA_ADDRESS = (ulong)0x1UL;
        public static readonly ulong IFA_LOCAL = (ulong)0x2UL;
        public static readonly ulong IFA_LABEL = (ulong)0x3UL;
        public static readonly ulong IFA_BROADCAST = (ulong)0x4UL;
        public static readonly ulong IFA_ANYCAST = (ulong)0x5UL;
        public static readonly ulong IFA_CACHEINFO = (ulong)0x6UL;
        public static readonly ulong IFA_MULTICAST = (ulong)0x7UL;
        public static readonly ulong IFLA_UNSPEC = (ulong)0x0UL;
        public static readonly ulong IFLA_ADDRESS = (ulong)0x1UL;
        public static readonly ulong IFLA_BROADCAST = (ulong)0x2UL;
        public static readonly ulong IFLA_IFNAME = (ulong)0x3UL;
        public static readonly ulong IFLA_MTU = (ulong)0x4UL;
        public static readonly ulong IFLA_LINK = (ulong)0x5UL;
        public static readonly ulong IFLA_QDISC = (ulong)0x6UL;
        public static readonly ulong IFLA_STATS = (ulong)0x7UL;
        public static readonly ulong IFLA_COST = (ulong)0x8UL;
        public static readonly ulong IFLA_PRIORITY = (ulong)0x9UL;
        public static readonly ulong IFLA_MASTER = (ulong)0xaUL;
        public static readonly ulong IFLA_WIRELESS = (ulong)0xbUL;
        public static readonly ulong IFLA_PROTINFO = (ulong)0xcUL;
        public static readonly ulong IFLA_TXQLEN = (ulong)0xdUL;
        public static readonly ulong IFLA_MAP = (ulong)0xeUL;
        public static readonly ulong IFLA_WEIGHT = (ulong)0xfUL;
        public static readonly ulong IFLA_OPERSTATE = (ulong)0x10UL;
        public static readonly ulong IFLA_LINKMODE = (ulong)0x11UL;
        public static readonly ulong IFLA_LINKINFO = (ulong)0x12UL;
        public static readonly ulong IFLA_NET_NS_PID = (ulong)0x13UL;
        public static readonly ulong IFLA_IFALIAS = (ulong)0x14UL;
        public static readonly ulong IFLA_MAX = (ulong)0x1dUL;
        public static readonly ulong RT_SCOPE_UNIVERSE = (ulong)0x0UL;
        public static readonly ulong RT_SCOPE_SITE = (ulong)0xc8UL;
        public static readonly ulong RT_SCOPE_LINK = (ulong)0xfdUL;
        public static readonly ulong RT_SCOPE_HOST = (ulong)0xfeUL;
        public static readonly ulong RT_SCOPE_NOWHERE = (ulong)0xffUL;
        public static readonly ulong RT_TABLE_UNSPEC = (ulong)0x0UL;
        public static readonly ulong RT_TABLE_COMPAT = (ulong)0xfcUL;
        public static readonly ulong RT_TABLE_DEFAULT = (ulong)0xfdUL;
        public static readonly ulong RT_TABLE_MAIN = (ulong)0xfeUL;
        public static readonly ulong RT_TABLE_LOCAL = (ulong)0xffUL;
        public static readonly ulong RT_TABLE_MAX = (ulong)0xffffffffUL;
        public static readonly ulong RTA_UNSPEC = (ulong)0x0UL;
        public static readonly ulong RTA_DST = (ulong)0x1UL;
        public static readonly ulong RTA_SRC = (ulong)0x2UL;
        public static readonly ulong RTA_IIF = (ulong)0x3UL;
        public static readonly ulong RTA_OIF = (ulong)0x4UL;
        public static readonly ulong RTA_GATEWAY = (ulong)0x5UL;
        public static readonly ulong RTA_PRIORITY = (ulong)0x6UL;
        public static readonly ulong RTA_PREFSRC = (ulong)0x7UL;
        public static readonly ulong RTA_METRICS = (ulong)0x8UL;
        public static readonly ulong RTA_MULTIPATH = (ulong)0x9UL;
        public static readonly ulong RTA_FLOW = (ulong)0xbUL;
        public static readonly ulong RTA_CACHEINFO = (ulong)0xcUL;
        public static readonly ulong RTA_TABLE = (ulong)0xfUL;
        public static readonly ulong RTN_UNSPEC = (ulong)0x0UL;
        public static readonly ulong RTN_UNICAST = (ulong)0x1UL;
        public static readonly ulong RTN_LOCAL = (ulong)0x2UL;
        public static readonly ulong RTN_BROADCAST = (ulong)0x3UL;
        public static readonly ulong RTN_ANYCAST = (ulong)0x4UL;
        public static readonly ulong RTN_MULTICAST = (ulong)0x5UL;
        public static readonly ulong RTN_BLACKHOLE = (ulong)0x6UL;
        public static readonly ulong RTN_UNREACHABLE = (ulong)0x7UL;
        public static readonly ulong RTN_PROHIBIT = (ulong)0x8UL;
        public static readonly ulong RTN_THROW = (ulong)0x9UL;
        public static readonly ulong RTN_NAT = (ulong)0xaUL;
        public static readonly ulong RTN_XRESOLVE = (ulong)0xbUL;
        public static readonly ulong RTNLGRP_NONE = (ulong)0x0UL;
        public static readonly ulong RTNLGRP_LINK = (ulong)0x1UL;
        public static readonly ulong RTNLGRP_NOTIFY = (ulong)0x2UL;
        public static readonly ulong RTNLGRP_NEIGH = (ulong)0x3UL;
        public static readonly ulong RTNLGRP_TC = (ulong)0x4UL;
        public static readonly ulong RTNLGRP_IPV4_IFADDR = (ulong)0x5UL;
        public static readonly ulong RTNLGRP_IPV4_MROUTE = (ulong)0x6UL;
        public static readonly ulong RTNLGRP_IPV4_ROUTE = (ulong)0x7UL;
        public static readonly ulong RTNLGRP_IPV4_RULE = (ulong)0x8UL;
        public static readonly ulong RTNLGRP_IPV6_IFADDR = (ulong)0x9UL;
        public static readonly ulong RTNLGRP_IPV6_MROUTE = (ulong)0xaUL;
        public static readonly ulong RTNLGRP_IPV6_ROUTE = (ulong)0xbUL;
        public static readonly ulong RTNLGRP_IPV6_IFINFO = (ulong)0xcUL;
        public static readonly ulong RTNLGRP_IPV6_PREFIX = (ulong)0x12UL;
        public static readonly ulong RTNLGRP_IPV6_RULE = (ulong)0x13UL;
        public static readonly ulong RTNLGRP_ND_USEROPT = (ulong)0x14UL;
        public static readonly ulong SizeofNlMsghdr = (ulong)0x10UL;
        public static readonly ulong SizeofNlMsgerr = (ulong)0x14UL;
        public static readonly ulong SizeofRtGenmsg = (ulong)0x1UL;
        public static readonly ulong SizeofNlAttr = (ulong)0x4UL;
        public static readonly ulong SizeofRtAttr = (ulong)0x4UL;
        public static readonly ulong SizeofIfInfomsg = (ulong)0x10UL;
        public static readonly ulong SizeofIfAddrmsg = (ulong)0x8UL;
        public static readonly ulong SizeofRtMsg = (ulong)0xcUL;
        public static readonly ulong SizeofRtNexthop = (ulong)0x8UL;


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

        public static readonly ulong SizeofSockFilter = (ulong)0x8UL;
        public static readonly ulong SizeofSockFprog = (ulong)0x10UL;


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
            public array<byte> Name;
        }

        public static readonly ulong SizeofInotifyEvent = (ulong)0x10UL;



        public partial struct PtraceRegs
        {
            public ulong R15;
            public ulong R14;
            public ulong R13;
            public ulong R12;
            public ulong Rbp;
            public ulong Rbx;
            public ulong R11;
            public ulong R10;
            public ulong R9;
            public ulong R8;
            public ulong Rax;
            public ulong Rcx;
            public ulong Rdx;
            public ulong Rsi;
            public ulong Rdi;
            public ulong Orig_rax;
            public ulong Rip;
            public ulong Cs;
            public ulong Eflags;
            public ulong Rsp;
            public ulong Ss;
            public ulong Fs_base;
            public ulong Gs_base;
            public ulong Ds;
            public ulong Es;
            public ulong Fs;
            public ulong Gs;
        }

        public partial struct FdSet
        {
            public array<long> Bits;
        }

        public partial struct Sysinfo_t
        {
            public long Uptime;
            public array<ulong> Loads;
            public ulong Totalram;
            public ulong Freeram;
            public ulong Sharedram;
            public ulong Bufferram;
            public ulong Totalswap;
            public ulong Freeswap;
            public ushort Procs;
            public ushort Pad;
            public array<byte> Pad_cgo_0;
            public ulong Totalhigh;
            public ulong Freehigh;
            public uint Unit;
            public array<byte> X_f;
            public array<byte> Pad_cgo_1;
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
            public array<byte> Pad_cgo_0;
            public ulong Tinode;
            public array<sbyte> Fname;
            public array<sbyte> Fpack;
            public array<byte> Pad_cgo_1;
        }

        public partial struct EpollEvent
        {
            public uint Events;
            public int Fd;
            public int Pad;
        }

        private static readonly ulong _AT_FDCWD = (ulong)-0x64UL;
        private static readonly ulong _AT_REMOVEDIR = (ulong)0x200UL;
        private static readonly ulong _AT_SYMLINK_NOFOLLOW = (ulong)0x100UL;
        private static readonly ulong _AT_EACCESS = (ulong)0x200UL;


        private partial struct pollFd
        {
            public int Fd;
            public short Events;
            public short Revents;
        }

        public partial struct Termios
        {
            public uint Iflag;
            public uint Oflag;
            public uint Cflag;
            public uint Lflag;
            public byte Line;
            public array<byte> Cc;
            public array<byte> Pad_cgo_0;
            public uint Ispeed;
            public uint Ospeed;
        }

        public static readonly ulong VINTR = (ulong)0x0UL;
        public static readonly ulong VQUIT = (ulong)0x1UL;
        public static readonly ulong VERASE = (ulong)0x2UL;
        public static readonly ulong VKILL = (ulong)0x3UL;
        public static readonly ulong VEOF = (ulong)0x4UL;
        public static readonly ulong VTIME = (ulong)0x5UL;
        public static readonly ulong VMIN = (ulong)0x6UL;
        public static readonly ulong VSWTC = (ulong)0x7UL;
        public static readonly ulong VSTART = (ulong)0x8UL;
        public static readonly ulong VSTOP = (ulong)0x9UL;
        public static readonly ulong VSUSP = (ulong)0xaUL;
        public static readonly ulong VEOL = (ulong)0xbUL;
        public static readonly ulong VREPRINT = (ulong)0xcUL;
        public static readonly ulong VDISCARD = (ulong)0xdUL;
        public static readonly ulong VWERASE = (ulong)0xeUL;
        public static readonly ulong VLNEXT = (ulong)0xfUL;
        public static readonly ulong VEOL2 = (ulong)0x10UL;
        public static readonly ulong IGNBRK = (ulong)0x1UL;
        public static readonly ulong BRKINT = (ulong)0x2UL;
        public static readonly ulong IGNPAR = (ulong)0x4UL;
        public static readonly ulong PARMRK = (ulong)0x8UL;
        public static readonly ulong INPCK = (ulong)0x10UL;
        public static readonly ulong ISTRIP = (ulong)0x20UL;
        public static readonly ulong INLCR = (ulong)0x40UL;
        public static readonly ulong IGNCR = (ulong)0x80UL;
        public static readonly ulong ICRNL = (ulong)0x100UL;
        public static readonly ulong IUCLC = (ulong)0x200UL;
        public static readonly ulong IXON = (ulong)0x400UL;
        public static readonly ulong IXANY = (ulong)0x800UL;
        public static readonly ulong IXOFF = (ulong)0x1000UL;
        public static readonly ulong IMAXBEL = (ulong)0x2000UL;
        public static readonly ulong IUTF8 = (ulong)0x4000UL;
        public static readonly ulong OPOST = (ulong)0x1UL;
        public static readonly ulong OLCUC = (ulong)0x2UL;
        public static readonly ulong ONLCR = (ulong)0x4UL;
        public static readonly ulong OCRNL = (ulong)0x8UL;
        public static readonly ulong ONOCR = (ulong)0x10UL;
        public static readonly ulong ONLRET = (ulong)0x20UL;
        public static readonly ulong OFILL = (ulong)0x40UL;
        public static readonly ulong OFDEL = (ulong)0x80UL;
        public static readonly ulong B0 = (ulong)0x0UL;
        public static readonly ulong B50 = (ulong)0x1UL;
        public static readonly ulong B75 = (ulong)0x2UL;
        public static readonly ulong B110 = (ulong)0x3UL;
        public static readonly ulong B134 = (ulong)0x4UL;
        public static readonly ulong B150 = (ulong)0x5UL;
        public static readonly ulong B200 = (ulong)0x6UL;
        public static readonly ulong B300 = (ulong)0x7UL;
        public static readonly ulong B600 = (ulong)0x8UL;
        public static readonly ulong B1200 = (ulong)0x9UL;
        public static readonly ulong B1800 = (ulong)0xaUL;
        public static readonly ulong B2400 = (ulong)0xbUL;
        public static readonly ulong B4800 = (ulong)0xcUL;
        public static readonly ulong B9600 = (ulong)0xdUL;
        public static readonly ulong B19200 = (ulong)0xeUL;
        public static readonly ulong B38400 = (ulong)0xfUL;
        public static readonly ulong CSIZE = (ulong)0x30UL;
        public static readonly ulong CS5 = (ulong)0x0UL;
        public static readonly ulong CS6 = (ulong)0x10UL;
        public static readonly ulong CS7 = (ulong)0x20UL;
        public static readonly ulong CS8 = (ulong)0x30UL;
        public static readonly ulong CSTOPB = (ulong)0x40UL;
        public static readonly ulong CREAD = (ulong)0x80UL;
        public static readonly ulong PARENB = (ulong)0x100UL;
        public static readonly ulong PARODD = (ulong)0x200UL;
        public static readonly ulong HUPCL = (ulong)0x400UL;
        public static readonly ulong CLOCAL = (ulong)0x800UL;
        public static readonly ulong B57600 = (ulong)0x1001UL;
        public static readonly ulong B115200 = (ulong)0x1002UL;
        public static readonly ulong B230400 = (ulong)0x1003UL;
        public static readonly ulong B460800 = (ulong)0x1004UL;
        public static readonly ulong B500000 = (ulong)0x1005UL;
        public static readonly ulong B576000 = (ulong)0x1006UL;
        public static readonly ulong B921600 = (ulong)0x1007UL;
        public static readonly ulong B1000000 = (ulong)0x1008UL;
        public static readonly ulong B1152000 = (ulong)0x1009UL;
        public static readonly ulong B1500000 = (ulong)0x100aUL;
        public static readonly ulong B2000000 = (ulong)0x100bUL;
        public static readonly ulong B2500000 = (ulong)0x100cUL;
        public static readonly ulong B3000000 = (ulong)0x100dUL;
        public static readonly ulong B3500000 = (ulong)0x100eUL;
        public static readonly ulong B4000000 = (ulong)0x100fUL;
        public static readonly ulong ISIG = (ulong)0x1UL;
        public static readonly ulong ICANON = (ulong)0x2UL;
        public static readonly ulong XCASE = (ulong)0x4UL;
        public static readonly ulong ECHO = (ulong)0x8UL;
        public static readonly ulong ECHOE = (ulong)0x10UL;
        public static readonly ulong ECHOK = (ulong)0x20UL;
        public static readonly ulong ECHONL = (ulong)0x40UL;
        public static readonly ulong NOFLSH = (ulong)0x80UL;
        public static readonly ulong TOSTOP = (ulong)0x100UL;
        public static readonly ulong ECHOCTL = (ulong)0x200UL;
        public static readonly ulong ECHOPRT = (ulong)0x400UL;
        public static readonly ulong ECHOKE = (ulong)0x800UL;
        public static readonly ulong FLUSHO = (ulong)0x1000UL;
        public static readonly ulong PENDIN = (ulong)0x4000UL;
        public static readonly ulong IEXTEN = (ulong)0x8000UL;
        public static readonly ulong TCGETS = (ulong)0x5401UL;
        public static readonly ulong TCSETS = (ulong)0x5402UL;

    }
}
