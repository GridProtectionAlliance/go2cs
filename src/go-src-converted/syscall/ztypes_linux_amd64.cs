// Created by cgo -godefs - DO NOT EDIT
// cgo -godefs types_linux.go

//go:build amd64 && linux
// +build amd64,linux

// package syscall -- go2cs converted at 2022 March 06 22:29:49 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\ztypes_linux_amd64.go


namespace go;

public static partial class syscall_package {

private static readonly nuint sizeofPtr = 0x8;
private static readonly nuint sizeofShort = 0x2;
private static readonly nuint sizeofInt = 0x4;
private static readonly nuint sizeofLong = 0x8;
private static readonly nuint sizeofLongLong = 0x8;
public static readonly nuint PathMax = 0x1000;


private partial struct _C_short { // : short
}
private partial struct _C_int { // : int
}
private partial struct _C_long { // : long
}
private partial struct _C_long_long { // : long
}
public partial struct Timespec {
    public long Sec;
    public long Nsec;
}

public partial struct Timeval {
    public long Sec;
    public long Usec;
}

public partial struct Timex {
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

public partial struct Time_t { // : long
}

public partial struct Tms {
    public long Utime;
    public long Stime;
    public long Cutime;
    public long Cstime;
}

public partial struct Utimbuf {
    public long Actime;
    public long Modtime;
}

public partial struct Rusage {
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

public partial struct Rlimit {
    public ulong Cur;
    public ulong Max;
}

private partial struct _Gid_t { // : uint
}

public partial struct Stat_t {
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

public partial struct Statfs_t {
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

public partial struct Dirent {
    public ulong Ino;
    public long Off;
    public ushort Reclen;
    public byte Type;
    public array<sbyte> Name;
    public array<byte> Pad_cgo_0;
}

public partial struct Fsid {
    public array<int> X__val;
}

public partial struct Flock_t {
    public short Type;
    public short Whence;
    public array<byte> Pad_cgo_0;
    public long Start;
    public long Len;
    public int Pid;
    public array<byte> Pad_cgo_1;
}

public partial struct RawSockaddrInet4 {
    public ushort Family;
    public ushort Port;
    public array<byte> Addr; /* in_addr */
    public array<byte> Zero;
}

public partial struct RawSockaddrInet6 {
    public ushort Family;
    public ushort Port;
    public uint Flowinfo;
    public array<byte> Addr; /* in6_addr */
    public uint Scope_id;
}

public partial struct RawSockaddrUnix {
    public ushort Family;
    public array<sbyte> Path;
}

public partial struct RawSockaddrLinklayer {
    public ushort Family;
    public ushort Protocol;
    public int Ifindex;
    public ushort Hatype;
    public byte Pkttype;
    public byte Halen;
    public array<byte> Addr;
}

public partial struct RawSockaddrNetlink {
    public ushort Family;
    public ushort Pad;
    public uint Pid;
    public uint Groups;
}

public partial struct RawSockaddr {
    public ushort Family;
    public array<sbyte> Data;
}

public partial struct RawSockaddrAny {
    public RawSockaddr Addr;
    public array<sbyte> Pad;
}

private partial struct _Socklen { // : uint
}

public partial struct Linger {
    public int Onoff;
    public int Linger;
}

public partial struct Iovec {
    public ptr<byte> Base;
    public ulong Len;
}

public partial struct IPMreq {
    public array<byte> Multiaddr; /* in_addr */
    public array<byte> Interface; /* in_addr */
}

public partial struct IPMreqn {
    public array<byte> Multiaddr; /* in_addr */
    public array<byte> Address; /* in_addr */
    public int Ifindex;
}

public partial struct IPv6Mreq {
    public array<byte> Multiaddr; /* in6_addr */
    public uint Interface;
}

public partial struct Msghdr {
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

public partial struct Cmsghdr {
    public ulong Len;
    public int Level;
    public int Type;
}

public partial struct Inet4Pktinfo {
    public int Ifindex;
    public array<byte> Spec_dst; /* in_addr */
    public array<byte> Addr; /* in_addr */
}

public partial struct Inet6Pktinfo {
    public array<byte> Addr; /* in6_addr */
    public uint Ifindex;
}

public partial struct IPv6MTUInfo {
    public RawSockaddrInet6 Addr;
    public uint Mtu;
}

public partial struct ICMPv6Filter {
    public array<uint> Data;
}

public partial struct Ucred {
    public int Pid;
    public uint Uid;
    public uint Gid;
}

public partial struct TCPInfo {
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

public static readonly nuint SizeofSockaddrInet4 = 0x10;
public static readonly nuint SizeofSockaddrInet6 = 0x1c;
public static readonly nuint SizeofSockaddrAny = 0x70;
public static readonly nuint SizeofSockaddrUnix = 0x6e;
public static readonly nuint SizeofSockaddrLinklayer = 0x14;
public static readonly nuint SizeofSockaddrNetlink = 0xc;
public static readonly nuint SizeofLinger = 0x8;
public static readonly nuint SizeofIPMreq = 0x8;
public static readonly nuint SizeofIPMreqn = 0xc;
public static readonly nuint SizeofIPv6Mreq = 0x14;
public static readonly nuint SizeofMsghdr = 0x38;
public static readonly nuint SizeofCmsghdr = 0x10;
public static readonly nuint SizeofInet4Pktinfo = 0xc;
public static readonly nuint SizeofInet6Pktinfo = 0x14;
public static readonly nuint SizeofIPv6MTUInfo = 0x20;
public static readonly nuint SizeofICMPv6Filter = 0x20;
public static readonly nuint SizeofUcred = 0xc;
public static readonly nuint SizeofTCPInfo = 0x68;


public static readonly nuint IFA_UNSPEC = 0x0;
public static readonly nuint IFA_ADDRESS = 0x1;
public static readonly nuint IFA_LOCAL = 0x2;
public static readonly nuint IFA_LABEL = 0x3;
public static readonly nuint IFA_BROADCAST = 0x4;
public static readonly nuint IFA_ANYCAST = 0x5;
public static readonly nuint IFA_CACHEINFO = 0x6;
public static readonly nuint IFA_MULTICAST = 0x7;
public static readonly nuint IFLA_UNSPEC = 0x0;
public static readonly nuint IFLA_ADDRESS = 0x1;
public static readonly nuint IFLA_BROADCAST = 0x2;
public static readonly nuint IFLA_IFNAME = 0x3;
public static readonly nuint IFLA_MTU = 0x4;
public static readonly nuint IFLA_LINK = 0x5;
public static readonly nuint IFLA_QDISC = 0x6;
public static readonly nuint IFLA_STATS = 0x7;
public static readonly nuint IFLA_COST = 0x8;
public static readonly nuint IFLA_PRIORITY = 0x9;
public static readonly nuint IFLA_MASTER = 0xa;
public static readonly nuint IFLA_WIRELESS = 0xb;
public static readonly nuint IFLA_PROTINFO = 0xc;
public static readonly nuint IFLA_TXQLEN = 0xd;
public static readonly nuint IFLA_MAP = 0xe;
public static readonly nuint IFLA_WEIGHT = 0xf;
public static readonly nuint IFLA_OPERSTATE = 0x10;
public static readonly nuint IFLA_LINKMODE = 0x11;
public static readonly nuint IFLA_LINKINFO = 0x12;
public static readonly nuint IFLA_NET_NS_PID = 0x13;
public static readonly nuint IFLA_IFALIAS = 0x14;
public static readonly nuint IFLA_MAX = 0x1d;
public static readonly nuint RT_SCOPE_UNIVERSE = 0x0;
public static readonly nuint RT_SCOPE_SITE = 0xc8;
public static readonly nuint RT_SCOPE_LINK = 0xfd;
public static readonly nuint RT_SCOPE_HOST = 0xfe;
public static readonly nuint RT_SCOPE_NOWHERE = 0xff;
public static readonly nuint RT_TABLE_UNSPEC = 0x0;
public static readonly nuint RT_TABLE_COMPAT = 0xfc;
public static readonly nuint RT_TABLE_DEFAULT = 0xfd;
public static readonly nuint RT_TABLE_MAIN = 0xfe;
public static readonly nuint RT_TABLE_LOCAL = 0xff;
public static readonly nuint RT_TABLE_MAX = 0xffffffff;
public static readonly nuint RTA_UNSPEC = 0x0;
public static readonly nuint RTA_DST = 0x1;
public static readonly nuint RTA_SRC = 0x2;
public static readonly nuint RTA_IIF = 0x3;
public static readonly nuint RTA_OIF = 0x4;
public static readonly nuint RTA_GATEWAY = 0x5;
public static readonly nuint RTA_PRIORITY = 0x6;
public static readonly nuint RTA_PREFSRC = 0x7;
public static readonly nuint RTA_METRICS = 0x8;
public static readonly nuint RTA_MULTIPATH = 0x9;
public static readonly nuint RTA_FLOW = 0xb;
public static readonly nuint RTA_CACHEINFO = 0xc;
public static readonly nuint RTA_TABLE = 0xf;
public static readonly nuint RTN_UNSPEC = 0x0;
public static readonly nuint RTN_UNICAST = 0x1;
public static readonly nuint RTN_LOCAL = 0x2;
public static readonly nuint RTN_BROADCAST = 0x3;
public static readonly nuint RTN_ANYCAST = 0x4;
public static readonly nuint RTN_MULTICAST = 0x5;
public static readonly nuint RTN_BLACKHOLE = 0x6;
public static readonly nuint RTN_UNREACHABLE = 0x7;
public static readonly nuint RTN_PROHIBIT = 0x8;
public static readonly nuint RTN_THROW = 0x9;
public static readonly nuint RTN_NAT = 0xa;
public static readonly nuint RTN_XRESOLVE = 0xb;
public static readonly nuint RTNLGRP_NONE = 0x0;
public static readonly nuint RTNLGRP_LINK = 0x1;
public static readonly nuint RTNLGRP_NOTIFY = 0x2;
public static readonly nuint RTNLGRP_NEIGH = 0x3;
public static readonly nuint RTNLGRP_TC = 0x4;
public static readonly nuint RTNLGRP_IPV4_IFADDR = 0x5;
public static readonly nuint RTNLGRP_IPV4_MROUTE = 0x6;
public static readonly nuint RTNLGRP_IPV4_ROUTE = 0x7;
public static readonly nuint RTNLGRP_IPV4_RULE = 0x8;
public static readonly nuint RTNLGRP_IPV6_IFADDR = 0x9;
public static readonly nuint RTNLGRP_IPV6_MROUTE = 0xa;
public static readonly nuint RTNLGRP_IPV6_ROUTE = 0xb;
public static readonly nuint RTNLGRP_IPV6_IFINFO = 0xc;
public static readonly nuint RTNLGRP_IPV6_PREFIX = 0x12;
public static readonly nuint RTNLGRP_IPV6_RULE = 0x13;
public static readonly nuint RTNLGRP_ND_USEROPT = 0x14;
public static readonly nuint SizeofNlMsghdr = 0x10;
public static readonly nuint SizeofNlMsgerr = 0x14;
public static readonly nuint SizeofRtGenmsg = 0x1;
public static readonly nuint SizeofNlAttr = 0x4;
public static readonly nuint SizeofRtAttr = 0x4;
public static readonly nuint SizeofIfInfomsg = 0x10;
public static readonly nuint SizeofIfAddrmsg = 0x8;
public static readonly nuint SizeofRtMsg = 0xc;
public static readonly nuint SizeofRtNexthop = 0x8;


public partial struct NlMsghdr {
    public uint Len;
    public ushort Type;
    public ushort Flags;
    public uint Seq;
    public uint Pid;
}

public partial struct NlMsgerr {
    public int Error;
    public NlMsghdr Msg;
}

public partial struct RtGenmsg {
    public byte Family;
}

public partial struct NlAttr {
    public ushort Len;
    public ushort Type;
}

public partial struct RtAttr {
    public ushort Len;
    public ushort Type;
}

public partial struct IfInfomsg {
    public byte Family;
    public byte X__ifi_pad;
    public ushort Type;
    public int Index;
    public uint Flags;
    public uint Change;
}

public partial struct IfAddrmsg {
    public byte Family;
    public byte Prefixlen;
    public byte Flags;
    public byte Scope;
    public uint Index;
}

public partial struct RtMsg {
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

public partial struct RtNexthop {
    public ushort Len;
    public byte Flags;
    public byte Hops;
    public int Ifindex;
}

public static readonly nuint SizeofSockFilter = 0x8;
public static readonly nuint SizeofSockFprog = 0x10;


public partial struct SockFilter {
    public ushort Code;
    public byte Jt;
    public byte Jf;
    public uint K;
}

public partial struct SockFprog {
    public ushort Len;
    public array<byte> Pad_cgo_0;
    public ptr<SockFilter> Filter;
}

public partial struct InotifyEvent {
    public int Wd;
    public uint Mask;
    public uint Cookie;
    public uint Len;
    public array<byte> Name;
}

public static readonly nuint SizeofInotifyEvent = 0x10;



public partial struct PtraceRegs {
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

public partial struct FdSet {
    public array<long> Bits;
}

public partial struct Sysinfo_t {
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

public partial struct Utsname {
    public array<sbyte> Sysname;
    public array<sbyte> Nodename;
    public array<sbyte> Release;
    public array<sbyte> Version;
    public array<sbyte> Machine;
    public array<sbyte> Domainname;
}

public partial struct Ustat_t {
    public int Tfree;
    public array<byte> Pad_cgo_0;
    public ulong Tinode;
    public array<sbyte> Fname;
    public array<sbyte> Fpack;
    public array<byte> Pad_cgo_1;
}

public partial struct EpollEvent {
    public uint Events;
    public int Fd;
    public int Pad;
}

private static readonly nuint _AT_FDCWD = -0x64;
private static readonly nuint _AT_REMOVEDIR = 0x200;
private static readonly nuint _AT_SYMLINK_NOFOLLOW = 0x100;
private static readonly nuint _AT_EACCESS = 0x200;


private partial struct pollFd {
    public int Fd;
    public short Events;
    public short Revents;
}

public partial struct Termios {
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

public static readonly nuint VINTR = 0x0;
public static readonly nuint VQUIT = 0x1;
public static readonly nuint VERASE = 0x2;
public static readonly nuint VKILL = 0x3;
public static readonly nuint VEOF = 0x4;
public static readonly nuint VTIME = 0x5;
public static readonly nuint VMIN = 0x6;
public static readonly nuint VSWTC = 0x7;
public static readonly nuint VSTART = 0x8;
public static readonly nuint VSTOP = 0x9;
public static readonly nuint VSUSP = 0xa;
public static readonly nuint VEOL = 0xb;
public static readonly nuint VREPRINT = 0xc;
public static readonly nuint VDISCARD = 0xd;
public static readonly nuint VWERASE = 0xe;
public static readonly nuint VLNEXT = 0xf;
public static readonly nuint VEOL2 = 0x10;
public static readonly nuint IGNBRK = 0x1;
public static readonly nuint BRKINT = 0x2;
public static readonly nuint IGNPAR = 0x4;
public static readonly nuint PARMRK = 0x8;
public static readonly nuint INPCK = 0x10;
public static readonly nuint ISTRIP = 0x20;
public static readonly nuint INLCR = 0x40;
public static readonly nuint IGNCR = 0x80;
public static readonly nuint ICRNL = 0x100;
public static readonly nuint IUCLC = 0x200;
public static readonly nuint IXON = 0x400;
public static readonly nuint IXANY = 0x800;
public static readonly nuint IXOFF = 0x1000;
public static readonly nuint IMAXBEL = 0x2000;
public static readonly nuint IUTF8 = 0x4000;
public static readonly nuint OPOST = 0x1;
public static readonly nuint OLCUC = 0x2;
public static readonly nuint ONLCR = 0x4;
public static readonly nuint OCRNL = 0x8;
public static readonly nuint ONOCR = 0x10;
public static readonly nuint ONLRET = 0x20;
public static readonly nuint OFILL = 0x40;
public static readonly nuint OFDEL = 0x80;
public static readonly nuint B0 = 0x0;
public static readonly nuint B50 = 0x1;
public static readonly nuint B75 = 0x2;
public static readonly nuint B110 = 0x3;
public static readonly nuint B134 = 0x4;
public static readonly nuint B150 = 0x5;
public static readonly nuint B200 = 0x6;
public static readonly nuint B300 = 0x7;
public static readonly nuint B600 = 0x8;
public static readonly nuint B1200 = 0x9;
public static readonly nuint B1800 = 0xa;
public static readonly nuint B2400 = 0xb;
public static readonly nuint B4800 = 0xc;
public static readonly nuint B9600 = 0xd;
public static readonly nuint B19200 = 0xe;
public static readonly nuint B38400 = 0xf;
public static readonly nuint CSIZE = 0x30;
public static readonly nuint CS5 = 0x0;
public static readonly nuint CS6 = 0x10;
public static readonly nuint CS7 = 0x20;
public static readonly nuint CS8 = 0x30;
public static readonly nuint CSTOPB = 0x40;
public static readonly nuint CREAD = 0x80;
public static readonly nuint PARENB = 0x100;
public static readonly nuint PARODD = 0x200;
public static readonly nuint HUPCL = 0x400;
public static readonly nuint CLOCAL = 0x800;
public static readonly nuint B57600 = 0x1001;
public static readonly nuint B115200 = 0x1002;
public static readonly nuint B230400 = 0x1003;
public static readonly nuint B460800 = 0x1004;
public static readonly nuint B500000 = 0x1005;
public static readonly nuint B576000 = 0x1006;
public static readonly nuint B921600 = 0x1007;
public static readonly nuint B1000000 = 0x1008;
public static readonly nuint B1152000 = 0x1009;
public static readonly nuint B1500000 = 0x100a;
public static readonly nuint B2000000 = 0x100b;
public static readonly nuint B2500000 = 0x100c;
public static readonly nuint B3000000 = 0x100d;
public static readonly nuint B3500000 = 0x100e;
public static readonly nuint B4000000 = 0x100f;
public static readonly nuint ISIG = 0x1;
public static readonly nuint ICANON = 0x2;
public static readonly nuint XCASE = 0x4;
public static readonly nuint ECHO = 0x8;
public static readonly nuint ECHOE = 0x10;
public static readonly nuint ECHOK = 0x20;
public static readonly nuint ECHONL = 0x40;
public static readonly nuint NOFLSH = 0x80;
public static readonly nuint TOSTOP = 0x100;
public static readonly nuint ECHOCTL = 0x200;
public static readonly nuint ECHOPRT = 0x400;
public static readonly nuint ECHOKE = 0x800;
public static readonly nuint FLUSHO = 0x1000;
public static readonly nuint PENDIN = 0x4000;
public static readonly nuint IEXTEN = 0x8000;
public static readonly nuint TCGETS = 0x5401;
public static readonly nuint TCSETS = 0x5402;


} // end syscall_package
