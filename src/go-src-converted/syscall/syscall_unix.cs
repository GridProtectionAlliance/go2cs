// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd solaris

// package syscall -- go2cs converted at 2020 August 29 08:38:22 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_unix.go
using race = go.@internal.race_package;
using runtime = go.runtime_package;
using sync = go.sync_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static unsafe partial class syscall_package
    {
        public static long Stdin = 0L;        public static long Stdout = 1L;        public static long Stderr = 2L;

        private static readonly var darwin64Bit = runtime.GOOS == "darwin" && sizeofPtr == 8L;
        private static readonly var dragonfly64Bit = runtime.GOOS == "dragonfly" && sizeofPtr == 8L;
        private static readonly var netbsd32Bit = runtime.GOOS == "netbsd" && sizeofPtr == 4L;
        private static readonly var solaris64Bit = runtime.GOOS == "solaris" && sizeofPtr == 8L;

        public static (System.UIntPtr, System.UIntPtr, Errno) Syscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
;
        public static (System.UIntPtr, System.UIntPtr, Errno) Syscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;
        public static (System.UIntPtr, System.UIntPtr, Errno) RawSyscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
;
        public static (System.UIntPtr, System.UIntPtr, Errno) RawSyscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;

        // Mmap manager, for use by operating system-specific implementations.

        private partial struct mmapper
        {
            public ref sync.Mutex Mutex => ref Mutex_val;
            public map<ref byte, slice<byte>> active; // active mappings; key is last byte in mapping
            public Func<System.UIntPtr, System.UIntPtr, long, long, long, long, (System.UIntPtr, error)> mmap;
            public Func<System.UIntPtr, System.UIntPtr, error> munmap;
        }

        private static (slice<byte>, error) Mmap(this ref mmapper _m, long fd, long offset, long length, long prot, long flags) => func(_m, (ref mmapper m, Defer defer, Panic _, Recover __) =>
        {>>MARKER:FUNCTION_RawSyscall6_BLOCK_PREFIX<<
            if (length <= 0L)
            {>>MARKER:FUNCTION_RawSyscall_BLOCK_PREFIX<<
                return (null, EINVAL);
            } 

            // Map the requested memory.
            var (addr, errno) = m.mmap(0L, uintptr(length), prot, flags, fd, offset);
            if (errno != null)
            {>>MARKER:FUNCTION_Syscall6_BLOCK_PREFIX<<
                return (null, errno);
            } 

            // Slice memory layout
            struct{addruintptrlenintcapint} sl = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{addruintptrlenintcapint}{addr,length,length}; 

            // Use unsafe to turn sl into a []byte.
            *(*slice<byte>) b = @unsafe.Pointer(ref sl).Value; 

            // Register mapping in m and return it.
            var p = ref b[cap(b) - 1L];
            m.Lock();
            defer(m.Unlock());
            m.active[p] = b;
            return (b, null);
        });

        private static error Munmap(this ref mmapper _m, slice<byte> data) => func(_m, (ref mmapper m, Defer defer, Panic _, Recover __) =>
        {>>MARKER:FUNCTION_Syscall_BLOCK_PREFIX<<
            if (len(data) == 0L || len(data) != cap(data))
            {
                return error.As(EINVAL);
            } 

            // Find the base of the mapping.
            var p = ref data[cap(data) - 1L];
            m.Lock();
            defer(m.Unlock());
            var b = m.active[p];
            if (b == null || ref b[0L] != ref data[0L])
            {
                return error.As(EINVAL);
            } 

            // Unmap the memory and update m.
            {
                var errno = m.munmap(uintptr(@unsafe.Pointer(ref b[0L])), uintptr(len(b)));

                if (errno != null)
                {
                    return error.As(errno);
                }

            }
            delete(m.active, p);
            return error.As(null);
        });

        // An Errno is an unsigned number describing an error condition.
        // It implements the error interface. The zero Errno is by convention
        // a non-error, so code to convert from Errno to error should use:
        //    err = nil
        //    if errno != 0 {
        //        err = errno
        //    }
        public partial struct Errno // : System.UIntPtr
        {
        }

        public static @string Error(this Errno e)
        {
            if (0L <= int(e) && int(e) < len(errors))
            {
                var s = errors[e];
                if (s != "")
                {
                    return s;
                }
            }
            return "errno " + itoa(int(e));
        }

        public static bool Temporary(this Errno e)
        {
            return e == EINTR || e == EMFILE || e == ECONNRESET || e == ECONNABORTED || e.Timeout();
        }

        public static bool Timeout(this Errno e)
        {
            return e == EAGAIN || e == EWOULDBLOCK || e == ETIMEDOUT;
        }

        // Do the interface allocations only once for common
        // Errno values.
        private static error errEAGAIN = error.As(EAGAIN);        private static error errEINVAL = error.As(EINVAL);        private static error errENOENT = error.As(ENOENT);

        // errnoErr returns common boxed Errno values, to prevent
        // allocations at runtime.
        private static error errnoErr(Errno e)
        {

            if (e == 0L) 
                return error.As(null);
            else if (e == EAGAIN) 
                return error.As(errEAGAIN);
            else if (e == EINVAL) 
                return error.As(errEINVAL);
            else if (e == ENOENT) 
                return error.As(errENOENT);
                        return error.As(e);
        }

        // A Signal is a number describing a process signal.
        // It implements the os.Signal interface.
        public partial struct Signal // : long
        {
        }

        public static void Signal(this Signal s)
        {
        }

        public static @string String(this Signal s)
        {
            if (0L <= s && int(s) < len(signals))
            {
                var str = signals[s];
                if (str != "")
                {
                    return str;
                }
            }
            return "signal " + itoa(int(s));
        }

        public static (long, error) Read(long fd, slice<byte> p)
        {
            n, err = read(fd, p);
            if (race.Enabled)
            {
                if (n > 0L)
                {
                    race.WriteRange(@unsafe.Pointer(ref p[0L]), n);
                }
                if (err == null)
                {
                    race.Acquire(@unsafe.Pointer(ref ioSync));
                }
            }
            if (msanenabled && n > 0L)
            {
                msanWrite(@unsafe.Pointer(ref p[0L]), n);
            }
            return;
        }

        public static (long, error) Write(long fd, slice<byte> p)
        {
            if (race.Enabled)
            {
                race.ReleaseMerge(@unsafe.Pointer(ref ioSync));
            }
            n, err = write(fd, p);
            if (race.Enabled && n > 0L)
            {
                race.ReadRange(@unsafe.Pointer(ref p[0L]), n);
            }
            if (msanenabled && n > 0L)
            {
                msanRead(@unsafe.Pointer(ref p[0L]), n);
            }
            return;
        }

        // For testing: clients can set this flag to force
        // creation of IPv6 sockets to return EAFNOSUPPORT.
        public static bool SocketDisableIPv6 = default;

        public partial interface Sockaddr
        {
            (unsafe.Pointer, _Socklen, error) sockaddr(); // lowercase; only we can define Sockaddrs
        }

        public partial struct SockaddrInet4
        {
            public long Port;
            public array<byte> Addr;
            public RawSockaddrInet4 raw;
        }

        public partial struct SockaddrInet6
        {
            public long Port;
            public uint ZoneId;
            public array<byte> Addr;
            public RawSockaddrInet6 raw;
        }

        public partial struct SockaddrUnix
        {
            public @string Name;
            public RawSockaddrUnix raw;
        }

        public static error Bind(long fd, Sockaddr sa)
        {
            var (ptr, n, err) = sa.sockaddr();
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(bind(fd, ptr, n));
        }

        public static error Connect(long fd, Sockaddr sa)
        {
            var (ptr, n, err) = sa.sockaddr();
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(connect(fd, ptr, n));
        }

        public static (Sockaddr, error) Getpeername(long fd)
        {
            RawSockaddrAny rsa = default;
            _Socklen len = SizeofSockaddrAny;
            err = getpeername(fd, ref rsa, ref len);

            if (err != null)
            {
                return;
            }
            return anyToSockaddr(ref rsa);
        }

        public static (long, error) GetsockoptInt(long fd, long level, long opt)
        {
            int n = default;
            var vallen = _Socklen(4L);
            err = getsockopt(fd, level, opt, @unsafe.Pointer(ref n), ref vallen);
            return (int(n), err);
        }

        public static (long, Sockaddr, error) Recvfrom(long fd, slice<byte> p, long flags)
        {
            RawSockaddrAny rsa = default;
            _Socklen len = SizeofSockaddrAny;
            n, err = recvfrom(fd, p, flags, ref rsa, ref len);

            if (err != null)
            {
                return;
            }
            if (rsa.Addr.Family != AF_UNSPEC)
            {
                from, err = anyToSockaddr(ref rsa);
            }
            return;
        }

        public static error Sendto(long fd, slice<byte> p, long flags, Sockaddr to)
        {
            var (ptr, n, err) = to.sockaddr();
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(sendto(fd, p, flags, ptr, n));
        }

        public static error SetsockoptByte(long fd, long level, long opt, byte value)
        {
            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(ref value), 1L));
        }

        public static error SetsockoptInt(long fd, long level, long opt, long value)
        {
            var n = int32(value);
            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(ref n), 4L));
        }

        public static error SetsockoptInet4Addr(long fd, long level, long opt, array<byte> value)
        {
            value = value.Clone();

            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(ref value[0L]), 4L));
        }

        public static error SetsockoptIPMreq(long fd, long level, long opt, ref IPMreq mreq)
        {
            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(mreq), SizeofIPMreq));
        }

        public static error SetsockoptIPv6Mreq(long fd, long level, long opt, ref IPv6Mreq mreq)
        {
            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(mreq), SizeofIPv6Mreq));
        }

        public static error SetsockoptICMPv6Filter(long fd, long level, long opt, ref ICMPv6Filter filter)
        {
            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(filter), SizeofICMPv6Filter));
        }

        public static error SetsockoptLinger(long fd, long level, long opt, ref Linger l)
        {
            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(l), SizeofLinger));
        }

        public static error SetsockoptString(long fd, long level, long opt, @string s)
        {
            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(ref (slice<byte>)s[0L]), uintptr(len(s))));
        }

        public static error SetsockoptTimeval(long fd, long level, long opt, ref Timeval tv)
        {
            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(tv), @unsafe.Sizeof(tv.Value)));
        }

        public static (long, error) Socket(long domain, long typ, long proto)
        {
            if (domain == AF_INET6 && SocketDisableIPv6)
            {
                return (-1L, EAFNOSUPPORT);
            }
            fd, err = socket(domain, typ, proto);
            return;
        }

        public static (array<long>, error) Socketpair(long domain, long typ, long proto)
        {
            array<int> fdx = new array<int>(2L);
            err = socketpair(domain, typ, proto, ref fdx);
            if (err == null)
            {
                fd[0L] = int(fdx[0L]);
                fd[1L] = int(fdx[1L]);
            }
            return;
        }

        public static (long, error) Sendfile(long outfd, long infd, ref long offset, long count)
        {
            if (race.Enabled)
            {
                race.ReleaseMerge(@unsafe.Pointer(ref ioSync));
            }
            return sendfile(outfd, infd, offset, count);
        }

        private static long ioSync = default;
    }
}
