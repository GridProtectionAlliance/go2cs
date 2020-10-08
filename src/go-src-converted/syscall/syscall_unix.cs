// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package syscall -- go2cs converted at 2020 October 08 03:27:44 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_unix.go
using oserror = go.@internal.oserror_package;
using race = go.@internal.race_package;
using unsafeheader = go.@internal.unsafeheader_package;
using runtime = go.runtime_package;
using sync = go.sync_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class syscall_package
    {
        public static long Stdin = 0L;        public static long Stdout = 1L;        public static long Stderr = 2L;

        private static readonly var darwin64Bit = (var)runtime.GOOS == "darwin" && sizeofPtr == 8L;
        private static readonly var netbsd32Bit = (var)runtime.GOOS == "netbsd" && sizeofPtr == 4L;


        public static (System.UIntPtr, System.UIntPtr, Errno) Syscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
;
        public static (System.UIntPtr, System.UIntPtr, Errno) Syscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;
        public static (System.UIntPtr, System.UIntPtr, Errno) RawSyscall(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
;
        public static (System.UIntPtr, System.UIntPtr, Errno) RawSyscall6(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;

        // clen returns the index of the first NULL byte in n or len(n) if n contains no NULL byte.
        private static long clen(slice<byte> n)
        {
            for (long i = 0L; i < len(n); i++)
            {>>MARKER:FUNCTION_RawSyscall6_BLOCK_PREFIX<<
                if (n[i] == 0L)
                {>>MARKER:FUNCTION_RawSyscall_BLOCK_PREFIX<<
                    return i;
                }

            }

            return len(n);

        }

        // Mmap manager, for use by operating system-specific implementations.

        private partial struct mmapper
        {
            public ref sync.Mutex Mutex => ref Mutex_val;
            public map<ptr<byte>, slice<byte>> active; // active mappings; key is last byte in mapping
            public Func<System.UIntPtr, System.UIntPtr, long, long, long, long, (System.UIntPtr, error)> mmap;
            public Func<System.UIntPtr, System.UIntPtr, error> munmap;
        }

        private static (slice<byte>, error) Mmap(this ptr<mmapper> _addr_m, long fd, long offset, long length, long prot, long flags) => func((defer, _, __) =>
        {
            slice<byte> data = default;
            error err = default!;
            ref mmapper m = ref _addr_m.val;

            if (length <= 0L)
            {>>MARKER:FUNCTION_Syscall6_BLOCK_PREFIX<<
                return (null, error.As(EINVAL)!);
            } 

            // Map the requested memory.
            var (addr, errno) = m.mmap(0L, uintptr(length), prot, flags, fd, offset);
            if (errno != null)
            {>>MARKER:FUNCTION_Syscall_BLOCK_PREFIX<<
                return (null, error.As(errno)!);
            } 

            // Use unsafe to turn addr into a []byte.
            ref slice<byte> b = ref heap(out ptr<slice<byte>> _addr_b);
            var hdr = (unsafeheader.Slice.val)(@unsafe.Pointer(_addr_b));
            hdr.Data = @unsafe.Pointer(addr);
            hdr.Cap = length;
            hdr.Len = length; 

            // Register mapping in m and return it.
            var p = _addr_b[cap(b) - 1L];
            m.Lock();
            defer(m.Unlock());
            m.active[p] = b;
            return (b, error.As(null!)!);

        });

        private static error Munmap(this ptr<mmapper> _addr_m, slice<byte> data) => func((defer, _, __) =>
        {
            error err = default!;
            ref mmapper m = ref _addr_m.val;

            if (len(data) == 0L || len(data) != cap(data))
            {
                return error.As(EINVAL)!;
            } 

            // Find the base of the mapping.
            var p = _addr_data[cap(data) - 1L];
            m.Lock();
            defer(m.Unlock());
            var b = m.active[p];
            if (b == null || _addr_b[0L] != _addr_data[0L])
            {
                return error.As(EINVAL)!;
            } 

            // Unmap the memory and update m.
            {
                var errno = m.munmap(uintptr(@unsafe.Pointer(_addr_b[0L])), uintptr(len(b)));

                if (errno != null)
                {
                    return error.As(errno)!;
                }

            }

            delete(m.active, p);
            return error.As(null!)!;

        });

        // An Errno is an unsigned number describing an error condition.
        // It implements the error interface. The zero Errno is by convention
        // a non-error, so code to convert from Errno to error should use:
        //    err = nil
        //    if errno != 0 {
        //        err = errno
        //    }
        //
        // Errno values can be tested against error values from the os package
        // using errors.Is. For example:
        //
        //    _, _, err := syscall.Syscall(...)
        //    if errors.Is(err, os.ErrNotExist) ...
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

        public static bool Is(this Errno e, error target)
        {

            if (target == oserror.ErrPermission) 
                return e == EACCES || e == EPERM;
            else if (target == oserror.ErrExist) 
                return e == EEXIST || e == ENOTEMPTY;
            else if (target == oserror.ErrNotExist) 
                return e == ENOENT;
                        return false;

        }

        public static bool Temporary(this Errno e)
        {
            return e == EINTR || e == EMFILE || e == ENFILE || e.Timeout();
        }

        public static bool Timeout(this Errno e)
        {
            return e == EAGAIN || e == EWOULDBLOCK || e == ETIMEDOUT;
        }

        // Do the interface allocations only once for common
        // Errno values.
        private static error errEAGAIN = error.As(EAGAIN)!;        private static error errEINVAL = error.As(EINVAL)!;        private static error errENOENT = error.As(ENOENT)!;

        // errnoErr returns common boxed Errno values, to prevent
        // allocations at runtime.
        private static error errnoErr(Errno e)
        {

            if (e == 0L) 
                return error.As(null!)!;
            else if (e == EAGAIN) 
                return error.As(errEAGAIN)!;
            else if (e == EINVAL) 
                return error.As(errEINVAL)!;
            else if (e == ENOENT) 
                return error.As(errENOENT)!;
                        return error.As(e)!;

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
            long n = default;
            error err = default!;

            n, err = read(fd, p);
            if (race.Enabled)
            {
                if (n > 0L)
                {
                    race.WriteRange(@unsafe.Pointer(_addr_p[0L]), n);
                }

                if (err == null)
                {
                    race.Acquire(@unsafe.Pointer(_addr_ioSync));
                }

            }

            if (msanenabled && n > 0L)
            {
                msanWrite(@unsafe.Pointer(_addr_p[0L]), n);
            }

            return ;

        }

        public static (long, error) Write(long fd, slice<byte> p)
        {
            long n = default;
            error err = default!;

            if (race.Enabled)
            {
                race.ReleaseMerge(@unsafe.Pointer(_addr_ioSync));
            }

            if (faketime && (fd == 1L || fd == 2L))
            {
                n = faketimeWrite(fd, p);
                if (n < 0L)
                {
                    n = 0L;
                    err = errnoErr(Errno(-n));

                }

            }
            else
            {
                n, err = write(fd, p);
            }

            if (race.Enabled && n > 0L)
            {
                race.ReadRange(@unsafe.Pointer(_addr_p[0L]), n);
            }

            if (msanenabled && n > 0L)
            {
                msanRead(@unsafe.Pointer(_addr_p[0L]), n);
            }

            return ;

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
            error err = default!;

            var (ptr, n, err) = sa.sockaddr();
            if (err != null)
            {
                return error.As(err)!;
            }

            return error.As(bind(fd, ptr, n))!;

        }

        public static error Connect(long fd, Sockaddr sa)
        {
            error err = default!;

            var (ptr, n, err) = sa.sockaddr();
            if (err != null)
            {
                return error.As(err)!;
            }

            return error.As(connect(fd, ptr, n))!;

        }

        public static (Sockaddr, error) Getpeername(long fd)
        {
            Sockaddr sa = default;
            error err = default!;

            ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
            ref _Socklen len = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_len);
            err = getpeername(fd, _addr_rsa, _addr_len);

            if (err != null)
            {
                return ;
            }

            return anyToSockaddr(_addr_rsa);

        }

        public static (long, error) GetsockoptInt(long fd, long level, long opt)
        {
            long value = default;
            error err = default!;

            ref int n = ref heap(out ptr<int> _addr_n);
            ref var vallen = ref heap(_Socklen(4L), out ptr<var> _addr_vallen);
            err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_n), _addr_vallen);
            return (int(n), error.As(err)!);
        }

        public static (long, Sockaddr, error) Recvfrom(long fd, slice<byte> p, long flags)
        {
            long n = default;
            Sockaddr from = default;
            error err = default!;

            ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
            ref _Socklen len = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_len);
            n, err = recvfrom(fd, p, flags, _addr_rsa, _addr_len);

            if (err != null)
            {
                return ;
            }

            if (rsa.Addr.Family != AF_UNSPEC)
            {
                from, err = anyToSockaddr(_addr_rsa);
            }

            return ;

        }

        public static error Sendto(long fd, slice<byte> p, long flags, Sockaddr to)
        {
            error err = default!;

            var (ptr, n, err) = to.sockaddr();
            if (err != null)
            {
                return error.As(err)!;
            }

            return error.As(sendto(fd, p, flags, ptr, n))!;

        }

        public static error SetsockoptByte(long fd, long level, long opt, byte value)
        {
            error err = default!;

            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), 1L))!;
        }

        public static error SetsockoptInt(long fd, long level, long opt, long value)
        {
            error err = default!;

            ref var n = ref heap(int32(value), out ptr<var> _addr_n);
            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(_addr_n), 4L))!;
        }

        public static error SetsockoptInet4Addr(long fd, long level, long opt, array<byte> value)
        {
            error err = default!;
            value = value.Clone();

            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(_addr_value[0L]), 4L))!;
        }

        public static error SetsockoptIPMreq(long fd, long level, long opt, ptr<IPMreq> _addr_mreq)
        {
            error err = default!;
            ref IPMreq mreq = ref _addr_mreq.val;

            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(mreq), SizeofIPMreq))!;
        }

        public static error SetsockoptIPv6Mreq(long fd, long level, long opt, ptr<IPv6Mreq> _addr_mreq)
        {
            error err = default!;
            ref IPv6Mreq mreq = ref _addr_mreq.val;

            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(mreq), SizeofIPv6Mreq))!;
        }

        public static error SetsockoptICMPv6Filter(long fd, long level, long opt, ptr<ICMPv6Filter> _addr_filter)
        {
            ref ICMPv6Filter filter = ref _addr_filter.val;

            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(filter), SizeofICMPv6Filter))!;
        }

        public static error SetsockoptLinger(long fd, long level, long opt, ptr<Linger> _addr_l)
        {
            error err = default!;
            ref Linger l = ref _addr_l.val;

            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(l), SizeofLinger))!;
        }

        public static error SetsockoptString(long fd, long level, long opt, @string s)
        {
            error err = default!;

            unsafe.Pointer p = default;
            if (len(s) > 0L)
            {
                p = @unsafe.Pointer(_addr_(slice<byte>)s[0L]);
            }

            return error.As(setsockopt(fd, level, opt, p, uintptr(len(s))))!;

        }

        public static error SetsockoptTimeval(long fd, long level, long opt, ptr<Timeval> _addr_tv)
        {
            error err = default!;
            ref Timeval tv = ref _addr_tv.val;

            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(tv), @unsafe.Sizeof(tv)))!;
        }

        public static (long, error) Socket(long domain, long typ, long proto)
        {
            long fd = default;
            error err = default!;

            if (domain == AF_INET6 && SocketDisableIPv6)
            {
                return (-1L, error.As(EAFNOSUPPORT)!);
            }

            fd, err = socket(domain, typ, proto);
            return ;

        }

        public static (array<long>, error) Socketpair(long domain, long typ, long proto)
        {
            array<long> fd = default;
            error err = default!;

            ref array<int> fdx = ref heap(new array<int>(2L), out ptr<array<int>> _addr_fdx);
            err = socketpair(domain, typ, proto, _addr_fdx);
            if (err == null)
            {
                fd[0L] = int(fdx[0L]);
                fd[1L] = int(fdx[1L]);
            }

            return ;

        }

        public static (long, error) Sendfile(long outfd, long infd, ptr<long> _addr_offset, long count)
        {
            long written = default;
            error err = default!;
            ref long offset = ref _addr_offset.val;

            if (race.Enabled)
            {
                race.ReleaseMerge(@unsafe.Pointer(_addr_ioSync));
            }

            return sendfile(outfd, infd, offset, count);

        }

        private static long ioSync = default;
    }
}
