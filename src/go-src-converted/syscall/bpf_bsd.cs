// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// Berkeley packet filter for BSD variants

// package syscall -- go2cs converted at 2020 August 29 08:16:14 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\bpf_bsd.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // Deprecated: Use golang.org/x/net/bpf instead.
        public static ref BpfInsn BpfStmt(long code, long k)
        {
            return ref new BpfInsn(Code:uint16(code),K:uint32(k));
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static ref BpfInsn BpfJump(long code, long k, long jt, long jf)
        {
            return ref new BpfInsn(Code:uint16(code),Jt:uint8(jt),Jf:uint8(jf),K:uint32(k));
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (long, error) BpfBuflen(long fd)
        {
            long l = default;
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCGBLEN, uintptr(@unsafe.Pointer(ref l)));
            if (err != 0L)
            {
                return (0L, Errno(err));
            }
            return (l, null);
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (long, error) SetBpfBuflen(long fd, long l)
        {
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCSBLEN, uintptr(@unsafe.Pointer(ref l)));
            if (err != 0L)
            {
                return (0L, Errno(err));
            }
            return (l, null);
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (long, error) BpfDatalink(long fd)
        {
            long t = default;
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCGDLT, uintptr(@unsafe.Pointer(ref t)));
            if (err != 0L)
            {
                return (0L, Errno(err));
            }
            return (t, null);
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (long, error) SetBpfDatalink(long fd, long t)
        {
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCSDLT, uintptr(@unsafe.Pointer(ref t)));
            if (err != 0L)
            {
                return (0L, Errno(err));
            }
            return (t, null);
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error SetBpfPromisc(long fd, long m)
        {
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCPROMISC, uintptr(@unsafe.Pointer(ref m)));
            if (err != 0L)
            {
                return error.As(Errno(err));
            }
            return error.As(null);
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error FlushBpf(long fd)
        {
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCFLUSH, 0L);
            if (err != 0L)
            {
                return error.As(Errno(err));
            }
            return error.As(null);
        }

        private partial struct ivalue
        {
            public array<byte> name;
            public short value;
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (@string, error) BpfInterface(long fd, @string name)
        {
            ivalue iv = default;
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCGETIF, uintptr(@unsafe.Pointer(ref iv)));
            if (err != 0L)
            {
                return ("", Errno(err));
            }
            return (name, null);
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error SetBpfInterface(long fd, @string name)
        {
            ivalue iv = default;
            copy(iv.name[..], (slice<byte>)name);
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCSETIF, uintptr(@unsafe.Pointer(ref iv)));
            if (err != 0L)
            {
                return error.As(Errno(err));
            }
            return error.As(null);
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (ref Timeval, error) BpfTimeout(long fd)
        {
            Timeval tv = default;
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCGRTIMEOUT, uintptr(@unsafe.Pointer(ref tv)));
            if (err != 0L)
            {
                return (null, Errno(err));
            }
            return (ref tv, null);
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error SetBpfTimeout(long fd, ref Timeval tv)
        {
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCSRTIMEOUT, uintptr(@unsafe.Pointer(tv)));
            if (err != 0L)
            {
                return error.As(Errno(err));
            }
            return error.As(null);
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (ref BpfStat, error) BpfStats(long fd)
        {
            BpfStat s = default;
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCGSTATS, uintptr(@unsafe.Pointer(ref s)));
            if (err != 0L)
            {
                return (null, Errno(err));
            }
            return (ref s, null);
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error SetBpfImmediate(long fd, long m)
        {
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCIMMEDIATE, uintptr(@unsafe.Pointer(ref m)));
            if (err != 0L)
            {
                return error.As(Errno(err));
            }
            return error.As(null);
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error SetBpf(long fd, slice<BpfInsn> i)
        {
            BpfProgram p = default;
            p.Len = uint32(len(i));
            p.Insns = (BpfInsn.Value)(@unsafe.Pointer(ref i[0L]));
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCSETF, uintptr(@unsafe.Pointer(ref p)));
            if (err != 0L)
            {
                return error.As(Errno(err));
            }
            return error.As(null);
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error CheckBpfVersion(long fd)
        {
            BpfVersion v = default;
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCVERSION, uintptr(@unsafe.Pointer(ref v)));
            if (err != 0L)
            {
                return error.As(Errno(err));
            }
            if (v.Major != BPF_MAJOR_VERSION || v.Minor != BPF_MINOR_VERSION)
            {
                return error.As(EINVAL);
            }
            return error.As(null);
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (long, error) BpfHeadercmpl(long fd)
        {
            long f = default;
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCGHDRCMPLT, uintptr(@unsafe.Pointer(ref f)));
            if (err != 0L)
            {
                return (0L, Errno(err));
            }
            return (f, null);
        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error SetBpfHeadercmpl(long fd, long f)
        {
            var (_, _, err) = Syscall(SYS_IOCTL, uintptr(fd), BIOCSHDRCMPLT, uintptr(@unsafe.Pointer(ref f)));
            if (err != 0L)
            {
                return error.As(Errno(err));
            }
            return error.As(null);
        }
    }
}
