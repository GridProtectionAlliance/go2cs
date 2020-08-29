// Created by cgo -godefs - DO NOT EDIT
// cgo -godefs types_solaris.go

// +build amd64,solaris

// package syscall -- go2cs converted at 2020 August 29 08:42:17 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\ztypes_solaris_amd64.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static readonly ulong sizeofPtr = 0x8UL;
        private static readonly ulong sizeofShort = 0x2UL;
        private static readonly ulong sizeofInt = 0x4UL;
        private static readonly ulong sizeofLong = 0x8UL;
        private static readonly ulong sizeofLongLong = 0x8UL;
        public static readonly ulong PathMax = 0x400UL;

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
        }        public partial struct Timespec
        {
            public long Sec;
            public long Nsec;
        }

        public partial struct Timeval
        {
            public long Sec;
            public long Usec;
        }

        public partial struct Timeval32
        {
            public int Sec;
            public int Usec;
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

        public static readonly ulong S_IFMT = 0xf000UL;
        public static readonly ulong S_IFIFO = 0x1000UL;
        public static readonly ulong S_IFCHR = 0x2000UL;
        public static readonly ulong S_IFDIR = 0x4000UL;
        public static readonly ulong S_IFBLK = 0x6000UL;
        public static readonly ulong S_IFREG = 0x8000UL;
        public static readonly ulong S_IFLNK = 0xa000UL;
        public static readonly ulong S_IFSOCK = 0xc000UL;
        public static readonly ulong S_ISUID = 0x800UL;
        public static readonly ulong S_ISGID = 0x400UL;
        public static readonly ulong S_ISVTX = 0x200UL;
        public static readonly ulong S_IRUSR = 0x100UL;
        public static readonly ulong S_IWUSR = 0x80UL;
        public static readonly ulong S_IXUSR = 0x40UL;

        public partial struct Stat_t
        {
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

        public partial struct Flock_t
        {
            public short Type;
            public short Whence;
            public array<byte> Pad_cgo_0;
            public long Start;
            public long Len;
            public int Sysid;
            public int Pid;
            public array<long> Pad;
        }

        public partial struct Dirent
        {
            public ulong Ino;
            public long Off;
            public ushort Reclen;
            public array<sbyte> Name;
            public array<byte> Pad_cgo_0;
        }

        public partial struct RawSockaddrInet4
        {
            public ushort Family;
            public ushort Port;
            public array<byte> Addr; /* in_addr */
            public array<sbyte> Zero;
        }

        public partial struct RawSockaddrInet6
        {
            public ushort Family;
            public ushort Port;
            public uint Flowinfo;
            public array<byte> Addr; /* in6_addr */
            public uint Scope_id;
            public uint X__sin6_src_id;
        }

        public partial struct RawSockaddrUnix
        {
            public ushort Family;
            public array<sbyte> Path;
        }

        public partial struct RawSockaddrDatalink
        {
            public ushort Family;
            public ushort Index;
            public byte Type;
            public byte Nlen;
            public byte Alen;
            public byte Slen;
            public array<sbyte> Data;
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
            public ptr<sbyte> Base;
            public ulong Len;
        }

        public partial struct IPMreq
        {
            public array<byte> Multiaddr; /* in_addr */
            public array<byte> Interface; /* in_addr */
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
            public int Iovlen;
            public array<byte> Pad_cgo_1;
            public ptr<sbyte> Accrights;
            public int Accrightslen;
            public array<byte> Pad_cgo_2;
        }

        public partial struct Cmsghdr
        {
            public uint Len;
            public int Level;
            public int Type;
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
            public array<uint> X__icmp6_filt;
        }

        public static readonly ulong SizeofSockaddrInet4 = 0x10UL;
        public static readonly ulong SizeofSockaddrInet6 = 0x20UL;
        public static readonly ulong SizeofSockaddrAny = 0xfcUL;
        public static readonly ulong SizeofSockaddrUnix = 0x6eUL;
        public static readonly ulong SizeofSockaddrDatalink = 0xfcUL;
        public static readonly ulong SizeofLinger = 0x8UL;
        public static readonly ulong SizeofIPMreq = 0x8UL;
        public static readonly ulong SizeofIPv6Mreq = 0x14UL;
        public static readonly ulong SizeofMsghdr = 0x30UL;
        public static readonly ulong SizeofCmsghdr = 0xcUL;
        public static readonly ulong SizeofInet6Pktinfo = 0x14UL;
        public static readonly ulong SizeofIPv6MTUInfo = 0x24UL;
        public static readonly ulong SizeofICMPv6Filter = 0x20UL;

        public partial struct FdSet
        {
            public array<long> Bits;
        }

        public static readonly ulong SizeofIfMsghdr = 0x54UL;
        public static readonly ulong SizeofIfData = 0x44UL;
        public static readonly ulong SizeofIfaMsghdr = 0x14UL;
        public static readonly ulong SizeofRtMsghdr = 0x4cUL;
        public static readonly ulong SizeofRtMetrics = 0x28UL;

        public partial struct IfMsghdr
        {
            public ushort Msglen;
            public byte Version;
            public byte Type;
            public int Addrs;
            public int Flags;
            public ushort Index;
            public array<byte> Pad_cgo_0;
            public IfData Data;
        }

        public partial struct IfData
        {
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

        public partial struct IfaMsghdr
        {
            public ushort Msglen;
            public byte Version;
            public byte Type;
            public int Addrs;
            public int Flags;
            public ushort Index;
            public array<byte> Pad_cgo_0;
            public int Metric;
        }

        public partial struct RtMsghdr
        {
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

        public partial struct RtMetrics
        {
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

        public static readonly ulong SizeofBpfVersion = 0x4UL;
        public static readonly ulong SizeofBpfStat = 0x80UL;
        public static readonly ulong SizeofBpfProgram = 0x10UL;
        public static readonly ulong SizeofBpfInsn = 0x8UL;
        public static readonly ulong SizeofBpfHdr = 0x14UL;

        public partial struct BpfVersion
        {
            public ushort Major;
            public ushort Minor;
        }

        public partial struct BpfStat
        {
            public ulong Recv;
            public ulong Drop;
            public ulong Capt;
            public array<ulong> Padding;
        }

        public partial struct BpfProgram
        {
            public uint Len;
            public array<byte> Pad_cgo_0;
            public ptr<BpfInsn> Insns;
        }

        public partial struct BpfInsn
        {
            public ushort Code;
            public byte Jt;
            public byte Jf;
            public uint K;
        }

        public partial struct BpfTimeval
        {
            public int Sec;
            public int Usec;
        }

        public partial struct BpfHdr
        {
            public BpfTimeval Tstamp;
            public uint Caplen;
            public uint Datalen;
            public ushort Hdrlen;
            public array<byte> Pad_cgo_0;
        }

        private static readonly ulong _AT_FDCWD = 0xffd19553UL;

        public partial struct Termios
        {
            public uint Iflag;
            public uint Oflag;
            public uint Cflag;
            public uint Lflag;
            public array<byte> Cc;
            public array<byte> Pad_cgo_0;
        }
    }
}
