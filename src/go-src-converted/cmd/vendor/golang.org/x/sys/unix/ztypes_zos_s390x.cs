// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build zos && s390x
// +build zos,s390x

// Hand edited based on ztypes_linux_s390x.go
// TODO: auto-generate.

// package unix -- go2cs converted at 2022 March 06 23:30:31 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\ztypes_zos_s390x.go


namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

public static readonly nuint SizeofPtr = 0x8;
public static readonly nuint SizeofShort = 0x2;
public static readonly nuint SizeofInt = 0x4;
public static readonly nuint SizeofLong = 0x8;
public static readonly nuint SizeofLongLong = 0x8;
public static readonly nuint PathMax = 0x1000;


public static readonly nint SizeofSockaddrAny = 128;
public static readonly nint SizeofCmsghdr = 12;
public static readonly nint SizeofIPMreq = 8;
public static readonly nint SizeofIPv6Mreq = 20;
public static readonly nint SizeofICMPv6Filter = 32;
public static readonly nint SizeofIPv6MTUInfo = 32;
public static readonly nint SizeofLinger = 8;
public static readonly nint SizeofSockaddrInet4 = 16;
public static readonly nint SizeofSockaddrInet6 = 28;
public static readonly nuint SizeofTCPInfo = 0x68;


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

private partial struct timeval_zos {
    public long Sec;
    public array<byte> _; // pad
    public int Usec;
}

public partial struct Tms {
    public uint Utime;
    public uint Stime;
    public uint Cutime;
    public uint Cstime;
}

public partial struct Time_t { // : long
}

public partial struct Utimbuf {
    public long Actime;
    public long Modtime;
}

public partial struct Utsname {
    public array<byte> Sysname;
    public array<byte> Nodename;
    public array<byte> Release;
    public array<byte> Version;
    public array<byte> Machine;
    public array<byte> Domainname;
}

public partial struct RawSockaddrInet4 {
    public byte Len;
    public byte Family;
    public ushort Port;
    public array<byte> Addr; /* in_addr */
    public array<byte> Zero;
}

public partial struct RawSockaddrInet6 {
    public byte Len;
    public byte Family;
    public ushort Port;
    public uint Flowinfo;
    public array<byte> Addr; /* in6_addr */
    public uint Scope_id;
}

public partial struct RawSockaddrUnix {
    public byte Len;
    public byte Family;
    public array<sbyte> Path;
}

public partial struct RawSockaddr {
    public byte Len;
    public byte Family;
    public array<byte> Data;
}

public partial struct RawSockaddrAny {
    public RawSockaddr Addr;
    public array<byte> _; // pad
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

public partial struct IPv6Mreq {
    public array<byte> Multiaddr; /* in6_addr */
    public uint Interface;
}

public partial struct Msghdr {
    public ptr<byte> Name;
    public ptr<Iovec> Iov;
    public ptr<byte> Control;
    public int Flags;
    public int Namelen;
    public int Iovlen;
    public int Controllen;
}

public partial struct Cmsghdr {
    public int Len;
    public int Level;
    public int Type;
}

public partial struct Inet4Pktinfo {
    public array<byte> Addr; /* in_addr */
    public uint Ifindex;
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

public partial struct TCPInfo {
    public byte State;
    public byte Ca_state;
    public byte Retransmits;
    public byte Probes;
    public byte Backoff;
    public byte Options;
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

private partial struct _Gid_t { // : uint
}

private partial struct rusage_zos {
    public timeval_zos Utime;
    public timeval_zos Stime;
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

// { int, short, short } in poll.h
public partial struct PollFd {
    public int Fd;
    public short Events;
    public short Revents;
}

public partial struct Stat_t {
    public ulong Dev;
    public ulong Ino;
    public ulong Nlink;
    public uint Mode;
    public uint Uid;
    public uint Gid;
    public int _;
    public ulong Rdev;
    public long Size;
    public Timespec Atim;
    public Timespec Mtim;
    public Timespec Ctim;
    public long Blksize;
    public long Blocks;
    public array<long> _;
}

public partial struct Stat_LE_t {
    public array<byte> _; // eye catcher
    public ushort Length;
    public ushort Version;
    public int Mode;
    public uint Ino;
    public uint Dev;
    public int Nlink;
    public int Uid;
    public int Gid;
    public long Size;
    public array<byte> Atim31;
    public array<byte> Mtim31;
    public array<byte> Ctim31;
    public uint Rdev;
    public uint Auditoraudit;
    public uint Useraudit;
    public int Blksize;
    public array<byte> Creatim31;
    public array<byte> AuditID;
    public array<byte> _; // rsrvd1
    public array<byte> CharsetID;
    public long Blocks;
    public uint Genvalue;
    public array<byte> Reftim31;
    public array<byte> Fid;
    public byte Filefmt;
    public byte Fspflag2;
    public array<byte> _; // rsrvd2
    public int Ctimemsec;
    public array<byte> Seclabel;
    public array<byte> _; // rsrvd3
    public array<byte> _; // rsrvd4
    public Time_t Atim;
    public Time_t Mtim;
    public Time_t Ctim;
    public Time_t Creatim;
    public Time_t Reftim;
    public array<byte> _; // rsrvd5
}

public partial struct Statvfs_t {
    public array<byte> ID;
    public int Len;
    public ulong Bsize;
    public ulong Blocks;
    public ulong Usedspace;
    public ulong Bavail;
    public ulong Flag;
    public long Maxfilesize;
    public array<byte> _;
    public ulong Frsize;
    public ulong Bfree;
    public uint Files;
    public uint Ffree;
    public uint Favail;
    public uint Namemax31;
    public uint Invarsec;
    public array<byte> _;
    public ulong Fsid;
    public ulong Namemax;
}

public partial struct Statfs_t {
    public uint Type;
    public ulong Bsize;
    public ulong Blocks;
    public ulong Bfree;
    public ulong Bavail;
    public uint Files;
    public uint Ffree;
    public ulong Fsid;
    public ulong Namelen;
    public ulong Frsize;
    public ulong Flags;
}

public partial struct Dirent {
    public ushort Reclen;
    public ushort Namlen;
    public uint Ino;
    public System.UIntPtr Extra;
    public array<byte> Name;
}

public partial struct FdSet {
    public array<int> Bits;
}

// This struct is packed on z/OS so it can't be used directly.
public partial struct Flock_t {
    public short Type;
    public short Whence;
    public long Start;
    public long Len;
    public int Pid;
}

public partial struct Termios {
    public uint Cflag;
    public uint Iflag;
    public uint Lflag;
    public uint Oflag;
    public array<byte> Cc;
}

public partial struct Winsize {
    public ushort Row;
    public ushort Col;
    public ushort Xpixel;
    public ushort Ypixel;
}

public partial struct W_Mnth {
    public array<byte> Hid;
    public int Size;
    public int Cur1; //32bit pointer
    public int Cur2; //^
    public uint Devno;
    public array<byte> _;
}

public partial struct W_Mntent {
    public uint Fstype;
    public uint Mode;
    public uint Dev;
    public uint Parentdev;
    public uint Rootino;
    public byte Status;
    public array<byte> Ddname;
    public array<byte> Fstname;
    public array<byte> Fsname;
    public uint Pathlen;
    public array<byte> Mountpoint;
    public array<byte> Jobname;
    public int PID;
    public int Parmoffset;
    public short Parmlen;
    public array<byte> Owner;
    public array<byte> Quiesceowner;
    public array<byte> _;
}

} // end unix_package
