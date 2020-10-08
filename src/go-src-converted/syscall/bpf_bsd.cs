// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd netbsd openbsd

// Berkeley packet filter for BSD variants

// package syscall -- go2cs converted at 2020 October 08 00:33:54 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\bpf_bsd.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // Deprecated: Use golang.org/x/net/bpf instead.
        public static ptr<BpfInsn> BpfStmt(long code, long k)
        {
            return addr(new BpfInsn(Code:uint16(code),K:uint32(k)));
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static ptr<BpfInsn> BpfJump(long code, long k, long jt, long jf)
        {
            return addr(new BpfInsn(Code:uint16(code),Jt:uint8(jt),Jf:uint8(jf),K:uint32(k)));
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (long, error) BpfBuflen(long fd)
        {
            long _p0 = default;
            error _p0 = default!;

            ref long l = ref heap(out ptr<long> _addr_l);
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCGBLEN, uintptr(@unsafe.Pointer(_addr_l)));
            if (err != 0L)
            {
                return (0L, error.As(Errno(err))!);
            }

            return (l, error.As(null!)!);

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (long, error) SetBpfBuflen(long fd, long l)
        {
            long _p0 = default;
            error _p0 = default!;

            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCSBLEN, uintptr(@unsafe.Pointer(_addr_l)));
            if (err != 0L)
            {
                return (0L, error.As(Errno(err))!);
            }

            return (l, error.As(null!)!);

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (long, error) BpfDatalink(long fd)
        {
            long _p0 = default;
            error _p0 = default!;

            ref long t = ref heap(out ptr<long> _addr_t);
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCGDLT, uintptr(@unsafe.Pointer(_addr_t)));
            if (err != 0L)
            {
                return (0L, error.As(Errno(err))!);
            }

            return (t, error.As(null!)!);

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (long, error) SetBpfDatalink(long fd, long t)
        {
            long _p0 = default;
            error _p0 = default!;

            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCSDLT, uintptr(@unsafe.Pointer(_addr_t)));
            if (err != 0L)
            {
                return (0L, error.As(Errno(err))!);
            }

            return (t, error.As(null!)!);

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error SetBpfPromisc(long fd, long m)
        {
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCPROMISC, uintptr(@unsafe.Pointer(_addr_m)));
            if (err != 0L)
            {
                return error.As(Errno(err))!;
            }

            return error.As(null!)!;

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error FlushBpf(long fd)
        {
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCFLUSH, 0L);
            if (err != 0L)
            {
                return error.As(Errno(err))!;
            }

            return error.As(null!)!;

        }

        private partial struct ivalue
        {
            public array<byte> name;
            public short value;
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (@string, error) BpfInterface(long fd, @string name)
        {
            @string _p0 = default;
            error _p0 = default!;

            ref ivalue iv = ref heap(out ptr<ivalue> _addr_iv);
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCGETIF, uintptr(@unsafe.Pointer(_addr_iv)));
            if (err != 0L)
            {
                return ("", error.As(Errno(err))!);
            }

            return (name, error.As(null!)!);

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error SetBpfInterface(long fd, @string name)
        {
            ref ivalue iv = ref heap(out ptr<ivalue> _addr_iv);
            copy(iv.name[..], (slice<byte>)name);
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCSETIF, uintptr(@unsafe.Pointer(_addr_iv)));
            if (err != 0L)
            {
                return error.As(Errno(err))!;
            }

            return error.As(null!)!;

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (ptr<Timeval>, error) BpfTimeout(long fd)
        {
            ptr<Timeval> _p0 = default!;
            error _p0 = default!;

            ref Timeval tv = ref heap(out ptr<Timeval> _addr_tv);
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCGRTIMEOUT, uintptr(@unsafe.Pointer(_addr_tv)));
            if (err != 0L)
            {
                return (_addr_null!, error.As(Errno(err))!);
            }

            return (_addr__addr_tv!, error.As(null!)!);

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error SetBpfTimeout(long fd, ptr<Timeval> _addr_tv)
        {
            ref Timeval tv = ref _addr_tv.val;

            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCSRTIMEOUT, uintptr(@unsafe.Pointer(tv)));
            if (err != 0L)
            {
                return error.As(Errno(err))!;
            }

            return error.As(null!)!;

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (ptr<BpfStat>, error) BpfStats(long fd)
        {
            ptr<BpfStat> _p0 = default!;
            error _p0 = default!;

            ref BpfStat s = ref heap(out ptr<BpfStat> _addr_s);
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCGSTATS, uintptr(@unsafe.Pointer(_addr_s)));
            if (err != 0L)
            {
                return (_addr_null!, error.As(Errno(err))!);
            }

            return (_addr__addr_s!, error.As(null!)!);

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error SetBpfImmediate(long fd, long m)
        {
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCIMMEDIATE, uintptr(@unsafe.Pointer(_addr_m)));
            if (err != 0L)
            {
                return error.As(Errno(err))!;
            }

            return error.As(null!)!;

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error SetBpf(long fd, slice<BpfInsn> i)
        {
            ref BpfProgram p = ref heap(out ptr<BpfProgram> _addr_p);
            p.Len = uint32(len(i));
            p.Insns = (BpfInsn.val)(@unsafe.Pointer(_addr_i[0L]));
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCSETF, uintptr(@unsafe.Pointer(_addr_p)));
            if (err != 0L)
            {
                return error.As(Errno(err))!;
            }

            return error.As(null!)!;

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error CheckBpfVersion(long fd)
        {
            ref BpfVersion v = ref heap(out ptr<BpfVersion> _addr_v);
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCVERSION, uintptr(@unsafe.Pointer(_addr_v)));
            if (err != 0L)
            {
                return error.As(Errno(err))!;
            }

            if (v.Major != BPF_MAJOR_VERSION || v.Minor != BPF_MINOR_VERSION)
            {
                return error.As(EINVAL)!;
            }

            return error.As(null!)!;

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (long, error) BpfHeadercmpl(long fd)
        {
            long _p0 = default;
            error _p0 = default!;

            ref long f = ref heap(out ptr<long> _addr_f);
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCGHDRCMPLT, uintptr(@unsafe.Pointer(_addr_f)));
            if (err != 0L)
            {
                return (0L, error.As(Errno(err))!);
            }

            return (f, error.As(null!)!);

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error SetBpfHeadercmpl(long fd, long f)
        {
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCSHDRCMPLT, uintptr(@unsafe.Pointer(_addr_f)));
            if (err != 0L)
            {
                return error.As(Errno(err))!;
            }

            return error.As(null!)!;

        }
    }
}
