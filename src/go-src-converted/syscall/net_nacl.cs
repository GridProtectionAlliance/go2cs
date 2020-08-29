// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// A simulated network for use within NaCl.
// The simulation is not particularly tied to NaCl,
// but other systems have real networks.

// All int64 times are UnixNanos.

// package syscall -- go2cs converted at 2020 August 29 08:37:23 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\net_nacl.go
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class syscall_package
    {
        // Interface to timers implemented in package runtime.
        // Must be in sync with ../runtime/time.go:/^type timer
        // Really for use by package time, but we cannot import time here.
        private partial struct runtimeTimer
        {
            public System.UIntPtr tb;
            public long i;
            public long when;
            public long period;
            public Action<object, System.UIntPtr> f; // NOTE: must not be closure
            public System.UIntPtr seq;
        }

        private static void startTimer(ref runtimeTimer _p0)
;
        private static bool stopTimer(ref runtimeTimer _p0)
;

        private partial struct timer
        {
            public bool expired;
            public ptr<queue> q;
            public runtimeTimer r;
        }

        private static void start(this ref timer t, ref queue q, long deadline)
        {>>MARKER:FUNCTION_stopTimer_BLOCK_PREFIX<<
            if (deadline == 0L)
            {>>MARKER:FUNCTION_startTimer_BLOCK_PREFIX<<
                return;
            }
            t.q = q;
            t.r.when = deadline;
            t.r.f = timerExpired;
            t.r.arg = t;
            startTimer(ref t.r);
        }

        private static void stop(this ref timer t)
        {
            if (t.r.f == null)
            {
                return;
            }
            stopTimer(ref t.r);
        }

        private static void reset(this ref timer t, ref queue q, long deadline)
        {
            t.stop();
            if (deadline == 0L)
            {
                return;
            }
            if (t.r.f == null)
            {
                t.q = q;
                t.r.f = timerExpired;
                t.r.arg = t;
            }
            t.r.when = deadline;
            startTimer(ref t.r);
        }

        private static void timerExpired(object i, System.UIntPtr seq) => func((defer, _, __) =>
        {
            ref timer t = i._<ref timer>();
            go_(() => () =>
            {
                t.q.Lock();
                defer(t.q.Unlock());
                t.expired = true;
                t.q.canRead.Broadcast();
                t.q.canWrite.Broadcast();
            }());
        });

        // Network constants and data structures. These match the traditional values.

        public static readonly var AF_UNSPEC = iota;
        public static readonly var AF_UNIX = 0;
        public static readonly var AF_INET = 1;
        public static readonly var AF_INET6 = 2;

        public static readonly var SHUT_RD = iota;
        public static readonly var SHUT_WR = 0;
        public static readonly var SHUT_RDWR = 1;

        public static readonly long SOCK_STREAM = 1L + iota;
        public static readonly var SOCK_DGRAM = 0;
        public static readonly var SOCK_RAW = 1;
        public static readonly var SOCK_SEQPACKET = 2;

        public static readonly long IPPROTO_IP = 0L;
        public static readonly long IPPROTO_IPV4 = 4L;
        public static readonly ulong IPPROTO_IPV6 = 0x29UL;
        public static readonly long IPPROTO_TCP = 6L;
        public static readonly ulong IPPROTO_UDP = 0x11UL;

        // Misc constants expected by package net but not supported.
        private static readonly var _ = iota;
        public static readonly var SOL_SOCKET = 0;
        public static readonly var SO_TYPE = 1;
        public static readonly var NET_RT_IFLIST = 2;
        public static readonly var IFNAMSIZ = 3;
        public static readonly var IFF_UP = 4;
        public static readonly var IFF_BROADCAST = 5;
        public static readonly var IFF_LOOPBACK = 6;
        public static readonly var IFF_POINTOPOINT = 7;
        public static readonly var IFF_MULTICAST = 8;
        public static readonly var IPV6_V6ONLY = 9;
        public static readonly var SOMAXCONN = 10;
        public static readonly var F_DUPFD_CLOEXEC = 11;
        public static readonly var SO_BROADCAST = 12;
        public static readonly var SO_REUSEADDR = 13;
        public static readonly var SO_REUSEPORT = 14;
        public static readonly var SO_RCVBUF = 15;
        public static readonly var SO_SNDBUF = 16;
        public static readonly var SO_KEEPALIVE = 17;
        public static readonly var SO_LINGER = 18;
        public static readonly var SO_ERROR = 19;
        public static readonly var IP_PORTRANGE = 20;
        public static readonly var IP_PORTRANGE_DEFAULT = 21;
        public static readonly var IP_PORTRANGE_LOW = 22;
        public static readonly var IP_PORTRANGE_HIGH = 23;
        public static readonly var IP_MULTICAST_IF = 24;
        public static readonly var IP_MULTICAST_LOOP = 25;
        public static readonly var IP_ADD_MEMBERSHIP = 26;
        public static readonly var IPV6_PORTRANGE = 27;
        public static readonly var IPV6_PORTRANGE_DEFAULT = 28;
        public static readonly var IPV6_PORTRANGE_LOW = 29;
        public static readonly var IPV6_PORTRANGE_HIGH = 30;
        public static readonly var IPV6_MULTICAST_IF = 31;
        public static readonly var IPV6_MULTICAST_LOOP = 32;
        public static readonly var IPV6_JOIN_GROUP = 33;
        public static readonly var TCP_NODELAY = 34;
        public static readonly var TCP_KEEPINTVL = 35;
        public static readonly SYS_FCNTL TCP_KEEPIDLE = 500L; // unsupported

        public static bool SocketDisableIPv6 = default;

        // A Sockaddr is one of the SockaddrXxx structs.
        public partial interface Sockaddr
        {
            void copy(); // key returns the value of the underlying data,
// for comparison as a map key.
            void key();
        }

        public partial struct SockaddrInet4
        {
            public long Port;
            public array<byte> Addr;
        }

        private static Sockaddr copy(this ref SockaddrInet4 sa)
        {
            var sa1 = sa.Value;
            return ref sa1;
        }

        private static void key(this ref SockaddrInet4 sa)
        {
            return sa.Value;
        }

        private static bool isIPv4Localhost(Sockaddr sa)
        {
            ref SockaddrInet4 (sa4, ok) = sa._<ref SockaddrInet4>();
            return ok && sa4.Addr == new array<byte>(new byte[] { 127, 0, 0, 1 });
        }

        public partial struct SockaddrInet6
        {
            public long Port;
            public uint ZoneId;
            public array<byte> Addr;
        }

        private static Sockaddr copy(this ref SockaddrInet6 sa)
        {
            var sa1 = sa.Value;
            return ref sa1;
        }

        private static void key(this ref SockaddrInet6 sa)
        {
            return sa.Value;
        }

        public partial struct SockaddrUnix
        {
            public @string Name;
        }

        private static Sockaddr copy(this ref SockaddrUnix sa)
        {
            var sa1 = sa.Value;
            return ref sa1;
        }

        private static void key(this ref SockaddrUnix sa)
        {
            return sa.Value;
        }

        public partial struct SockaddrDatalink
        {
            public byte Len;
            public byte Family;
            public ushort Index;
            public byte Type;
            public byte Nlen;
            public byte Alen;
            public byte Slen;
            public array<sbyte> Data;
        }

        private static Sockaddr copy(this ref SockaddrDatalink sa)
        {
            var sa1 = sa.Value;
            return ref sa1;
        }

        private static void key(this ref SockaddrDatalink sa)
        {
            return sa.Value;
        }

        // RoutingMessage represents a routing message.
        public partial interface RoutingMessage
        {
            void unimplemented();
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

        public partial struct Linger
        {
            public int Onoff;
            public int Linger;
        }

        public partial struct ICMPv6Filter
        {
            public array<uint> Filt;
        }

        // A queue is the bookkeeping for a synchronized buffered queue.
        // We do not use channels because we need to be able to handle
        // writes after and during close, and because a chan byte would
        // require too many send and receive operations in real use.
        private partial struct queue
        {
            public ref sync.Mutex Mutex => ref Mutex_val;
            public sync.Cond canRead;
            public sync.Cond canWrite;
            public ptr<timer> rtimer; // non-nil if in read
            public ptr<timer> wtimer; // non-nil if in write
            public long r; // total read index
            public long w; // total write index
            public long m; // index mask
            public bool closed;
        }

        private static void init(this ref queue _q, long size) => func(_q, (ref queue q, Defer _, Panic panic, Recover __) =>
        {
            if (size & (size - 1L) != 0L)
            {
                panic("invalid queue size - must be power of two");
            }
            q.canRead.L = ref q.Mutex;
            q.canWrite.L = ref q.Mutex;
            q.m = size - 1L;
        });

        private static bool past(long deadline)
        {
            var (sec, nsec) = now();
            return deadline > 0L && deadline < sec * 1e9F + int64(nsec);
        }

        private static (long, error) waitRead(this ref queue q, long n, long deadline)
        {
            if (past(deadline))
            {
                return (0L, EAGAIN);
            }
            timer t = default;
            t.start(q, deadline);
            q.rtimer = ref t;
            while (q.w - q.r == 0L && !q.closed && !t.expired)
            {
                q.canRead.Wait();
            }

            q.rtimer = null;
            t.stop();
            var m = q.w - q.r;
            if (m == 0L && t.expired)
            {
                return (0L, EAGAIN);
            }
            if (m > n)
            {
                m = n;
                q.canRead.Signal(); // wake up next reader too
            }
            q.canWrite.Signal();
            return (m, null);
        }

        private static (long, error) waitWrite(this ref queue q, long n, long deadline)
        {
            if (past(deadline))
            {
                return (0L, EAGAIN);
            }
            timer t = default;
            t.start(q, deadline);
            q.wtimer = ref t;
            while (q.w - q.r > q.m && !q.closed && !t.expired)
            {
                q.canWrite.Wait();
            }

            q.wtimer = null;
            t.stop();
            var m = q.m + 1L - (q.w - q.r);
            if (m == 0L && t.expired)
            {
                return (0L, EAGAIN);
            }
            if (m == 0L)
            {
                return (0L, EAGAIN);
            }
            if (m > n)
            {
                m = n;
                q.canWrite.Signal(); // wake up next writer too
            }
            q.canRead.Signal();
            return (m, null);
        }

        private static void close(this ref queue _q) => func(_q, (ref queue q, Defer defer, Panic _, Recover __) =>
        {
            q.Lock();
            defer(q.Unlock());
            q.closed = true;
            q.canRead.Broadcast();
            q.canWrite.Broadcast();
        });

        // A byteq is a byte queue.
        private partial struct byteq
        {
            public ref queue queue => ref queue_val;
            public slice<byte> data;
        }

        private static ref byteq newByteq()
        {
            byteq q = ref new byteq(data:make([]byte,4096),);
            q.init(len(q.data));
            return q;
        }

        private static (long, error) read(this ref byteq _q, slice<byte> b, long deadline) => func(_q, (ref byteq q, Defer defer, Panic _, Recover __) =>
        {
            q.Lock();
            defer(q.Unlock());
            var (n, err) = q.waitRead(len(b), deadline);
            if (err != null)
            {
                return (0L, err);
            }
            b = b[..n];
            while (len(b) > 0L)
            {
                var m = copy(b, q.data[q.r & q.m..]);
                q.r += m;
                b = b[m..];
            }

            return (n, null);
        });

        private static (long, error) write(this ref byteq _q, slice<byte> b, long deadline) => func(_q, (ref byteq q, Defer defer, Panic _, Recover __) =>
        {
            q.Lock();
            defer(q.Unlock());
            while (n < len(b))
            {
                var (nn, err) = q.waitWrite(len(b[n..]), deadline);
                if (err != null)
                {
                    return (n, err);
                }
                var bb = b[n..n + nn];
                n += nn;
                while (len(bb) > 0L)
                {
                    var m = copy(q.data[q.w & q.m..], bb);
                    q.w += m;
                    bb = bb[m..];
                }

            }

            return (n, null);
        });

        // A msgq is a queue of messages.
        private partial struct msgq
        {
            public ref queue queue => ref queue_val;
            public slice<object> data;
        }

        private static ref msgq newMsgq()
        {
            msgq q = ref new msgq(data:make([]interface{},32),);
            q.init(len(q.data));
            return q;
        }

        private static (object, error) read(this ref msgq _q, long deadline) => func(_q, (ref msgq q, Defer defer, Panic _, Recover __) =>
        {
            q.Lock();
            defer(q.Unlock());
            var (n, err) = q.waitRead(1L, deadline);
            if (err != null)
            {
                return (null, err);
            }
            if (n == 0L)
            {
                return (null, null);
            }
            var m = q.data[q.r & q.m];
            q.r++;
            return (m, null);
        });

        private static error write(this ref msgq _q, object m, long deadline) => func(_q, (ref msgq q, Defer defer, Panic _, Recover __) =>
        {
            q.Lock();
            defer(q.Unlock());
            var (_, err) = q.waitWrite(1L, deadline);
            if (err != null)
            {
                return error.As(err);
            }
            q.data[q.w & q.m] = m;
            q.w++;
            return error.As(null);
        });

        // An addr is a sequence of bytes uniquely identifying a network address.
        // It is not human-readable.
        private partial struct addr // : @string
        {
        }

        // A conn is one side of a stream-based network connection.
        // That is, a stream-based network connection is a pair of cross-connected conns.
        private partial struct conn
        {
            public ptr<byteq> rd;
            public ptr<byteq> wr;
            public addr local;
            public addr remote;
        }

        // A pktconn is one side of a packet-based network connection.
        // That is, a packet-based network connection is a pair of cross-connected pktconns.
        private partial struct pktconn
        {
            public ptr<msgq> rd;
            public ptr<msgq> wr;
            public addr local;
            public addr remote;
        }

        // A listener accepts incoming stream-based network connections.
        private partial struct listener
        {
            public ptr<msgq> rd;
            public addr local;
        }

        // A netFile is an open network file.
        private partial struct netFile
        {
            public ref defaultFileImpl defaultFileImpl => ref defaultFileImpl_val;
            public ptr<netproto> proto;
            public long sotype;
            public ptr<msgq> listener;
            public ptr<msgq> packet;
            public ptr<byteq> rd;
            public ptr<byteq> wr;
            public long rddeadline;
            public long wrdeadline;
            public Sockaddr addr;
            public Sockaddr raddr;
        }

        // A netAddr is a network address in the global listener map.
        // All the fields must have defined == operations.
        private partial struct netAddr
        {
            public ptr<netproto> proto;
            public long sotype;
        }

        // net records the state of the network.
        // It maps a network address to the listener on that address.
        private static struct{sync.Mutexlistenermap[netAddr]*netFile} net = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{sync.Mutexlistenermap[netAddr]*netFile}{listener:make(map[netAddr]*netFile),};

        // TODO(rsc): Some day, do a better job with port allocation.
        // For playground programs, incrementing is fine.
        private static long nextport = 2L;

        // A netproto contains protocol-specific functionality
        // (one for AF_INET, one for AF_INET6 and so on).
        // It is a struct instead of an interface because the
        // implementation needs no state, and I expect to
        // add some data fields at some point.
        private partial struct netproto
        {
            public Func<ref netFile, Sockaddr, error> bind;
        }

        private static netproto netprotoAF_INET = ref new netproto(bind:func(f*netFile,saSockaddr)error{ifsa==nil{f.addr=&SockaddrInet4{Port:nextport,Addr:[4]byte{127,0,0,1},}nextport++returnnil}addr,ok:=sa.(*SockaddrInet4)if!ok{returnEINVAL}addr=addr.copy().(*SockaddrInet4)ifaddr.Port==0{addr.Port=nextportnextport++}f.addr=addrreturnnil},);

        private static map netprotos = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<long, ref netproto>{AF_INET:netprotoAF_INET,};

        // These functions implement the usual BSD socket operations.

        private static error bind(this ref netFile f, Sockaddr sa)
        {
            if (f.addr != null)
            {
                return error.As(EISCONN);
            }
            {
                var err = f.proto.bind(f, sa);

                if (err != null)
                {
                    return error.As(err);
                }

            }
            if (f.sotype == SOCK_DGRAM)
            {
                var (_, ok) = net.listener[new netAddr(f.proto,f.sotype,f.addr.key())];
                if (ok)
                {
                    f.addr = null;
                    return error.As(EADDRINUSE);
                }
                net.listener[new netAddr(f.proto,f.sotype,f.addr.key())] = f;
                f.packet = newMsgq();
            }
            return error.As(null);
        }

        private static error listen(this ref netFile _f, long backlog) => func(_f, (ref netFile f, Defer defer, Panic _, Recover __) =>
        {
            net.Lock();
            defer(net.Unlock());
            if (f.listener != null)
            {
                return error.As(EINVAL);
            }
            var (old, ok) = net.listener[new netAddr(f.proto,f.sotype,f.addr.key())];
            if (ok && !old.listenerClosed())
            {
                return error.As(EADDRINUSE);
            }
            net.listener[new netAddr(f.proto,f.sotype,f.addr.key())] = f;
            f.listener = newMsgq();
            return error.As(null);
        });

        private static (long, Sockaddr, error) accept(this ref netFile f)
        {
            var (msg, err) = f.listener.read(f.readDeadline());
            if (err != null)
            {
                return (-1L, null, err);
            }
            ref netFile (newf, ok) = msg._<ref netFile>();
            if (!ok)
            { 
                // must be eof
                return (-1L, null, EAGAIN);
            }
            return (newFD(newf), newf.raddr.copy(), null);
        }

        private static error connect(this ref netFile f, Sockaddr sa)
        {
            if (past(f.writeDeadline()))
            {
                return error.As(EAGAIN);
            }
            if (f.addr == null)
            {
                {
                    var err = f.bind(null);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
            }
            net.Lock();
            if (sa == null)
            {
                net.Unlock();
                return error.As(EINVAL);
            }
            sa = sa.copy();
            if (f.raddr != null)
            {
                net.Unlock();
                return error.As(EISCONN);
            }
            if (f.sotype == SOCK_DGRAM)
            {
                net.Unlock();
                f.raddr = sa;
                return error.As(null);
            }
            if (f.listener != null)
            {
                net.Unlock();
                return error.As(EISCONN);
            }
            var (l, ok) = net.listener[new netAddr(f.proto,f.sotype,sa.key())];
            if (!ok)
            { 
                // If we're dialing 127.0.0.1 but found nothing, try
                // 0.0.0.0 also. (Issue 20611)
                if (isIPv4Localhost(sa))
                {
                    sa = ref new SockaddrInet4(Port:sa.(*SockaddrInet4).Port);
                    l, ok = net.listener[new netAddr(f.proto,f.sotype,sa.key())];
                }
            }
            if (!ok || l.listenerClosed())
            {
                net.Unlock();
                return error.As(ECONNREFUSED);
            }
            f.raddr = sa;
            f.rd = newByteq();
            f.wr = newByteq();
            netFile newf = ref new netFile(proto:f.proto,sotype:f.sotype,addr:f.raddr,raddr:f.addr,rd:f.wr,wr:f.rd,);
            net.Unlock();
            l.listener.write(newf, f.writeDeadline());
            return error.As(null);
        }

        private static (long, error) read(this ref netFile f, slice<byte> b)
        {
            if (f.rd == null)
            {
                if (f.raddr != null)
                {
                    var (n, _, err) = f.recvfrom(b, 0L);
                    return (n, err);
                }
                return (0L, ENOTCONN);
            }
            return f.rd.read(b, f.readDeadline());
        }

        private static (long, error) write(this ref netFile f, slice<byte> b)
        {
            if (f.wr == null)
            {
                if (f.raddr != null)
                {
                    var err = f.sendto(b, 0L, f.raddr);
                    long n = default;
                    if (err == null)
                    {
                        n = len(b);
                    }
                    return (n, err);
                }
                return (0L, ENOTCONN);
            }
            return f.wr.write(b, f.writeDeadline());
        }

        private partial struct pktmsg
        {
            public slice<byte> buf;
            public Sockaddr addr;
        }

        private static (long, Sockaddr, error) recvfrom(this ref netFile f, slice<byte> p, long flags)
        {
            if (f.sotype != SOCK_DGRAM)
            {
                return (0L, null, EINVAL);
            }
            if (f.packet == null)
            {
                return (0L, null, ENOTCONN);
            }
            var (msg1, err) = f.packet.read(f.readDeadline());
            if (err != null)
            {
                return (0L, null, err);
            }
            ref pktmsg (msg, ok) = msg1._<ref pktmsg>();
            if (!ok)
            {
                return (0L, null, EAGAIN);
            }
            return (copy(p, msg.buf), msg.addr, null);
        }

        private static error sendto(this ref netFile f, slice<byte> p, long flags, Sockaddr to)
        {
            if (f.sotype != SOCK_DGRAM)
            {
                return error.As(EINVAL);
            }
            if (f.packet == null)
            {
                {
                    var err = f.bind(null);

                    if (err != null)
                    {
                        return error.As(err);
                    }

                }
            }
            net.Lock();
            if (to == null)
            {
                net.Unlock();
                return error.As(EINVAL);
            }
            to = to.copy();
            var (l, ok) = net.listener[new netAddr(f.proto,f.sotype,to.key())];
            if (!ok || l.packet == null)
            {
                net.Unlock();
                return error.As(ECONNREFUSED);
            }
            net.Unlock();
            pktmsg msg = ref new pktmsg(buf:make([]byte,len(p)),addr:f.addr,);
            copy(msg.buf, p);
            l.packet.write(msg, f.writeDeadline());
            return error.As(null);
        }

        private static bool listenerClosed(this ref netFile _f) => func(_f, (ref netFile f, Defer defer, Panic _, Recover __) =>
        {
            f.listener.Lock();
            defer(f.listener.Unlock());
            return f.listener.closed;
        });

        private static error close(this ref netFile f)
        {
            if (f.listener != null)
            {
                f.listener.close();
            }
            if (f.packet != null)
            {
                f.packet.close();
            }
            if (f.rd != null)
            {
                f.rd.close();
            }
            if (f.wr != null)
            {
                f.wr.close();
            }
            return error.As(null);
        }

        private static (ref netFile, error) fdToNetFile(long fd)
        {
            var (f, err) = fdToFile(fd);
            if (err != null)
            {
                return (null, err);
            }
            var impl = f.impl;
            ref netFile (netf, ok) = impl._<ref netFile>();
            if (!ok)
            {
                return (null, EINVAL);
            }
            return (netf, null);
        }

        public static (long, error) Socket(long proto, long sotype, long unused)
        {
            var p = netprotos[proto];
            if (p == null)
            {
                return (-1L, EPROTONOSUPPORT);
            }
            if (sotype != SOCK_STREAM && sotype != SOCK_DGRAM)
            {
                return (-1L, ESOCKTNOSUPPORT);
            }
            netFile f = ref new netFile(proto:p,sotype:sotype,);
            return (newFD(f), null);
        }

        public static error Bind(long fd, Sockaddr sa)
        {
            var (f, err) = fdToNetFile(fd);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(f.bind(sa));
        }

        public static error StopIO(long fd)
        {
            var (f, err) = fdToNetFile(fd);
            if (err != null)
            {
                return error.As(err);
            }
            f.close();
            return error.As(null);
        }

        public static error Listen(long fd, long backlog)
        {
            var (f, err) = fdToNetFile(fd);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(f.listen(backlog));
        }

        public static (long, Sockaddr, error) Accept(long fd)
        {
            var (f, err) = fdToNetFile(fd);
            if (err != null)
            {
                return (0L, null, err);
            }
            return f.accept();
        }

        public static (Sockaddr, error) Getsockname(long fd)
        {
            var (f, err) = fdToNetFile(fd);
            if (err != null)
            {
                return (null, err);
            }
            if (f.addr == null)
            {
                return (null, ENOTCONN);
            }
            return (f.addr.copy(), null);
        }

        public static (Sockaddr, error) Getpeername(long fd)
        {
            var (f, err) = fdToNetFile(fd);
            if (err != null)
            {
                return (null, err);
            }
            if (f.raddr == null)
            {
                return (null, ENOTCONN);
            }
            return (f.raddr.copy(), null);
        }

        public static error Connect(long fd, Sockaddr sa)
        {
            var (f, err) = fdToNetFile(fd);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(f.connect(sa));
        }

        public static (long, Sockaddr, error) Recvfrom(long fd, slice<byte> p, long flags)
        {
            var (f, err) = fdToNetFile(fd);
            if (err != null)
            {
                return (0L, null, err);
            }
            return f.recvfrom(p, flags);
        }

        public static error Sendto(long fd, slice<byte> p, long flags, Sockaddr to)
        {
            var (f, err) = fdToNetFile(fd);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(f.sendto(p, flags, to));
        }

        public static (long, long, long, Sockaddr, error) Recvmsg(long fd, slice<byte> p, slice<byte> oob, long flags)
        {
            var (f, err) = fdToNetFile(fd);
            if (err != null)
            {
                return;
            }
            n, from, err = f.recvfrom(p, flags);
            return;
        }

        public static error Sendmsg(long fd, slice<byte> p, slice<byte> oob, Sockaddr to, long flags)
        {
            var (_, err) = SendmsgN(fd, p, oob, to, flags);
            return error.As(err);
        }

        public static (long, error) SendmsgN(long fd, slice<byte> p, slice<byte> oob, Sockaddr to, long flags)
        {
            var (f, err) = fdToNetFile(fd);
            if (err != null)
            {
                return (0L, err);
            }

            if (f.sotype == SOCK_STREAM) 
                n, err = f.write(p);
            else if (f.sotype == SOCK_DGRAM) 
                n = len(p);
                err = f.sendto(p, flags, to);
                        if (err != null)
            {
                return (0L, err);
            }
            return (n, null);
        }

        public static (long, error) GetsockoptInt(long fd, long level, long opt)
        {
            var (f, err) = fdToNetFile(fd);
            if (err != null)
            {
                return (0L, err);
            }

            if (level == SOL_SOCKET && opt == SO_TYPE) 
                return (f.sotype, null);
                        return (0L, ENOTSUP);
        }

        public static error SetsockoptInt(long fd, long level, long opt, long value)
        {
            return error.As(null);
        }

        public static error SetsockoptByte(long fd, long level, long opt, byte value)
        {
            var (_, err) = fdToNetFile(fd);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(ENOTSUP);
        }

        public static error SetsockoptLinger(long fd, long level, long opt, ref Linger l)
        {
            return error.As(null);
        }

        public static error SetReadDeadline(long fd, long t)
        {
            var (f, err) = fdToNetFile(fd);
            if (err != null)
            {
                return error.As(err);
            }
            atomic.StoreInt64(ref f.rddeadline, t);
            {
                var bq = f.rd;

                if (bq != null)
                {
                    bq.Lock();
                    {
                        var timer = bq.rtimer;

                        if (timer != null)
                        {
                            timer.reset(ref bq.queue, t);
                        }

                    }
                    bq.Unlock();
                }

            }
            return error.As(null);
        }

        private static long readDeadline(this ref netFile f)
        {
            return atomic.LoadInt64(ref f.rddeadline);
        }

        public static error SetWriteDeadline(long fd, long t)
        {
            var (f, err) = fdToNetFile(fd);
            if (err != null)
            {
                return error.As(err);
            }
            atomic.StoreInt64(ref f.wrdeadline, t);
            {
                var bq = f.wr;

                if (bq != null)
                {
                    bq.Lock();
                    {
                        var timer = bq.wtimer;

                        if (timer != null)
                        {
                            timer.reset(ref bq.queue, t);
                        }

                    }
                    bq.Unlock();
                }

            }
            return error.As(null);
        }

        private static long writeDeadline(this ref netFile f)
        {
            return atomic.LoadInt64(ref f.wrdeadline);
        }

        public static error Shutdown(long fd, long how)
        {
            var (f, err) = fdToNetFile(fd);
            if (err != null)
            {
                return error.As(err);
            }

            if (how == SHUT_RD) 
                f.rd.close();
            else if (how == SHUT_WR) 
                f.wr.close();
            else if (how == SHUT_RDWR) 
                f.rd.close();
                f.wr.close();
                        return error.As(null);
        }

        public static error SetsockoptICMPv6Filter(long fd, long level, long opt, ref ICMPv6Filter _filter) => func(_filter, (ref ICMPv6Filter filter, Defer _, Panic panic, Recover __) =>
        {
            panic("SetsockoptICMPv");

        });
        public static error SetsockoptIPMreq(long fd, long level, long opt, ref IPMreq _mreq) => func(_mreq, (ref IPMreq mreq, Defer _, Panic panic, Recover __) =>
        {
            panic("SetsockoptIPMreq");

        });
        public static error SetsockoptIPv6Mreq(long fd, long level, long opt, ref IPv6Mreq _mreq) => func(_mreq, (ref IPv6Mreq mreq, Defer _, Panic panic, Recover __) =>
        {
            panic("SetsockoptIPv");

        });
        public static error SetsockoptInet4Addr(long fd, long level, long opt, array<byte> value) => func((_, panic, __) =>
        {
            value = value.Clone();

            panic("SetsockoptInet");

        });
        public static error SetsockoptString(long fd, long level, long opt, @string s) => func((_, panic, __) =>
        {
            panic("SetsockoptString");

        });
        public static error SetsockoptTimeval(long fd, long level, long opt, ref Timeval _tv) => func(_tv, (ref Timeval tv, Defer _, Panic panic, Recover __) =>
        {
            panic("SetsockoptTimeval");

        });
        public static (array<long>, error) Socketpair(long domain, long typ, long proto) => func((_, panic, __) =>
        {
            panic("Socketpair");

        });

        public static error SetNonblock(long fd, bool nonblocking)
        {
            return error.As(null);
        }
    }
}
