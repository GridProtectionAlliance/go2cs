// Created by cgo -godefs - DO NOT EDIT
// cgo -godefs types_solaris.go

//go:build amd64 && solaris
// +build amd64,solaris

// package syscall -- go2cs converted at 2022 March 06 22:29:51 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\ztypes_solaris_amd64.go


namespace go;

public static partial class syscall_package {

private static readonly nuint sizeofPtr = 0x8;
private static readonly nuint sizeofShort = 0x2;
private static readonly nuint sizeofInt = 0x4;
private static readonly nuint sizeofLong = 0x8;
private static readonly nuint sizeofLongLong = 0x8;
public static readonly nuint PathMax = 0x400;


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

public partial struct Timeval32 {
    public int Sec;
    public int Usec;
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

private partial struct _Pid_t { // : int
}

private partial struct _Gid_t { // : uint
}

public static readonly nuint S_IFMT = 0xf000;
public static readonly nuint S_IFIFO = 0x1000;
public static readonly nuint S_IFCHR = 0x2000;
public static readonly nuint S_IFDIR = 0x4000;
public static readonly nuint S_IFBLK = 0x6000;
public static readonly nuint S_IFREG = 0x8000;
public static readonly nuint S_IFLNK = 0xa000;
public static readonly nuint S_IFSOCK = 0xc000;
public static readonly nuint S_ISUID = 0x800;
public static readonly nuint S_ISGID = 0x400;
public static readonly nuint S_ISVTX = 0x200;
public static readonly nuint S_IRUSR = 0x100;
public static readonly nuint S_IWUSR = 0x80;
public static readonly nuint S_IXUSR = 0x40;
public static readonly nuint S_IRWXG = 0x38;
public static readonly nuint S_IRWXO = 0x7;


public partial struct Stat_t {
    public ulong Dev;
    public ulong Ino;
    public uint Mode;
    public uint Nlink;
    public uint Uid;
    public uint Gid;
    public ulong Rdev;
    public long Size;
    public Timespec Atim;
    public Timespec Mtim;
    public Timespec Ctim;
    public int Blksize;
    public array<byte> Pad_cgo_0;
    public long Blocks;
    public array<sbyte> Fstype;
}

public partial struct Flock_t {
    public short Type;
    public short Whence;
    public array<byte> Pad_cgo_0;
    public long Start;
    public long Len;
    public int Sysid;
    public int Pid;
    public array<long> Pad;
}

public partial struct Dirent {
    public ulong Ino;
    public long Off;
    public ushort Reclen;
    public array<sbyte> Name;
    public array<byte> Pad_cgo_0;
}

public partial struct RawSockaddrInet4 {
    public ushort Family;
    public ushort Port;
    public array<byte> Addr; /* in_addr */
    public array<sbyte> Zero;
}

public partial struct RawSockaddrInet6 {
    public ushort Family;
    public ushort Port;
    public uint Flowinfo;
    public array<byte> Addr; /* in6_addr */
    public uint Scope_id;
    public uint X__sin6_src_id;
}

public partial struct RawSockaddrUnix {
    public ushort Family;
    public array<sbyte> Path;
}

public partial struct RawSockaddrDatalink {
    public ushort Family;
    public ushort Index;
    public byte Type;
    public byte Nlen;
    public byte Alen;
    public byte Slen;
    public array<sbyte> Data;
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
    public ptr<sbyte> Base;
    public ulong Len;
}

public partial struct IPMreq {
    public array<byte> Multiaddr; /* in_addr */
    public array<byte> Interface; /* in_addr */
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
    public int Iovlen;
    public array<byte> Pad_cgo_1;
    public ptr<sbyte> Accrights;
    public int Accrightslen;
    public array<byte> Pad_cgo_2;
}

public partial struct Cmsghdr {
    public uint Len;
    public int Level;
    public int Type;
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
    public array<uint> X__icmp6_filt;
}

public static readonly nuint SizeofSockaddrInet4 = 0x10;
public static readonly nuint SizeofSockaddrInet6 = 0x20;
public static readonly nuint SizeofSockaddrAny = 0xfc;
public static readonly nuint SizeofSockaddrUnix = 0x6e;
public static readonly nuint SizeofSockaddrDatalink = 0xfc;
public static readonly nuint SizeofLinger = 0x8;
public static readonly nuint SizeofIPMreq = 0x8;
public static readonly nuint SizeofIPv6Mreq = 0x14;
public static readonly nuint SizeofMsghdr = 0x30;
public static readonly nuint SizeofCmsghdr = 0xc;
public static readonly nuint SizeofInet6Pktinfo = 0x14;
public static readonly nuint SizeofIPv6MTUInfo = 0x24;
public static readonly nuint SizeofICMPv6Filter = 0x20;


public partial struct FdSet {
    public array<long> Bits;
}

public static readonly nuint SizeofIfMsghdr = 0x54;
public static readonly nuint SizeofIfData = 0x44;
public static readonly nuint SizeofIfaMsghdr = 0x14;
public static readonly nuint SizeofRtMsghdr = 0x4c;
public static readonly nuint SizeofRtMetrics = 0x28;


public partial struct IfMsghdr {
    public ushort Msglen;
    public byte Version;
    public byte Type;
    public int Addrs;
    public int Flags;
    public ushort Index;
    public array<byte> Pad_cgo_0;
    public IfData Data;
}

public partial struct IfData {
    public byte Type;
    public byte Addrlen;
    public byte Hdrlen;
    public array<byte> Pad_cgo_0;
    public uint Mtu;
    public uint Metric;
    public uint Baudrate;
    public uint Ipackets;
    public uint Ierrors;
    public uint Opackets;
    public uint Oerrors;
    public uint Collisions;
    public uint Ibytes;
    public uint Obytes;
    public uint Imcasts;
    public uint Omcasts;
    public uint Iqdrops;
    public uint Noproto;
    public Timeval32 Lastchange;
}

public partial struct IfaMsghdr {
    public ushort Msglen;
    public byte Version;
    public byte Type;
    public int Addrs;
    public int Flags;
    public ushort Index;
    public array<byte> Pad_cgo_0;
    public int Metric;
}

public partial struct RtMsghdr {
    public ushort Msglen;
    public byte Version;
    public byte Type;
    public ushort Index;
    public array<byte> Pad_cgo_0;
    public int Flags;
    public int Addrs;
    public int Pid;
    public int Seq;
    public int Errno;
    public int Use;
    public uint Inits;
    public RtMetrics Rmx;
}

public partial struct RtMetrics {
    public uint Locks;
    public uint Mtu;
    public uint Hopcount;
    public uint Expire;
    public uint Recvpipe;
    public uint Sendpipe;
    public uint Ssthresh;
    public uint Rtt;
    public uint Rttvar;
    public uint Pksent;
}

public static readonly nuint SizeofBpfVersion = 0x4;
public static readonly nuint SizeofBpfStat = 0x80;
public static readonly nuint SizeofBpfProgram = 0x10;
public static readonly nuint SizeofBpfInsn = 0x8;
public static readonly nuint SizeofBpfHdr = 0x14;


public partial struct BpfVersion {
    public ushort Major;
    public ushort Minor;
}

public partial struct BpfStat {
    public ulong Recv;
    public ulong Drop;
    public ulong Capt;
    public array<ulong> Padding;
}

public partial struct BpfProgram {
    public uint Len;
    public array<byte> Pad_cgo_0;
    public ptr<BpfInsn> Insns;
}

public partial struct BpfInsn {
    public ushort Code;
    public byte Jt;
    public byte Jf;
    public uint K;
}

public partial struct BpfTimeval {
    public int Sec;
    public int Usec;
}

public partial struct BpfHdr {
    public BpfTimeval Tstamp;
    public uint Caplen;
    public uint Datalen;
    public ushort Hdrlen;
    public array<byte> Pad_cgo_0;
}

private static readonly nuint _AT_FDCWD = 0xffd19553;


public partial struct Termios {
    public uint Iflag;
    public uint Oflag;
    public uint Cflag;
    public uint Lflag;
    public array<byte> Cc;
    public array<byte> Pad_cgo_0;
}

} // end syscall_package
