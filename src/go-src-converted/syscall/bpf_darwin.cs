// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Berkeley packet filter for Darwin

// package syscall -- go2cs converted at 2020 October 09 04:45:21 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\bpf_darwin.go
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
            var err = ioctlPtr(fd, BIOCGBLEN, @unsafe.Pointer(_addr_l));
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            return (l, error.As(null!)!);

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (long, error) SetBpfBuflen(long fd, long l)
        {
            long _p0 = default;
            error _p0 = default!;

            var err = ioctlPtr(fd, BIOCSBLEN, @unsafe.Pointer(_addr_l));
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            return (l, error.As(null!)!);

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (long, error) BpfDatalink(long fd)
        {
            long _p0 = default;
            error _p0 = default!;

            ref long t = ref heap(out ptr<long> _addr_t);
            var err = ioctlPtr(fd, BIOCGDLT, @unsafe.Pointer(_addr_t));
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            return (t, error.As(null!)!);

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (long, error) SetBpfDatalink(long fd, long t)
        {
            long _p0 = default;
            error _p0 = default!;

            var err = ioctlPtr(fd, BIOCSDLT, @unsafe.Pointer(_addr_t));
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            return (t, error.As(null!)!);

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error SetBpfPromisc(long fd, long m)
        {
            var err = ioctlPtr(fd, BIOCPROMISC, @unsafe.Pointer(_addr_m));
            if (err != null)
            {
                return error.As(err)!;
            }

            return error.As(null!)!;

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error FlushBpf(long fd)
        {
            var err = ioctlPtr(fd, BIOCFLUSH, null);
            if (err != null)
            {
                return error.As(err)!;
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
            var err = ioctlPtr(fd, BIOCGETIF, @unsafe.Pointer(_addr_iv));
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            return (name, error.As(null!)!);

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error SetBpfInterface(long fd, @string name)
        {
            ref ivalue iv = ref heap(out ptr<ivalue> _addr_iv);
            copy(iv.name[..], (slice<byte>)name);
            var err = ioctlPtr(fd, BIOCSETIF, @unsafe.Pointer(_addr_iv));
            if (err != null)
            {
                return error.As(err)!;
            }

            return error.As(null!)!;

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (ptr<Timeval>, error) BpfTimeout(long fd)
        {
            ptr<Timeval> _p0 = default!;
            error _p0 = default!;

            ref Timeval tv = ref heap(out ptr<Timeval> _addr_tv);
            var err = ioctlPtr(fd, BIOCGRTIMEOUT, @unsafe.Pointer(_addr_tv));
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (_addr__addr_tv!, error.As(null!)!);

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error SetBpfTimeout(long fd, ptr<Timeval> _addr_tv)
        {
            ref Timeval tv = ref _addr_tv.val;

            var err = ioctlPtr(fd, BIOCSRTIMEOUT, @unsafe.Pointer(tv));
            if (err != null)
            {
                return error.As(err)!;
            }

            return error.As(null!)!;

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static (ptr<BpfStat>, error) BpfStats(long fd)
        {
            ptr<BpfStat> _p0 = default!;
            error _p0 = default!;

            ref BpfStat s = ref heap(out ptr<BpfStat> _addr_s);
            var err = ioctlPtr(fd, BIOCGSTATS, @unsafe.Pointer(_addr_s));
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (_addr__addr_s!, error.As(null!)!);

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error SetBpfImmediate(long fd, long m)
        {
            var err = ioctlPtr(fd, BIOCIMMEDIATE, @unsafe.Pointer(_addr_m));
            if (err != null)
            {
                return error.As(err)!;
            }

            return error.As(null!)!;

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error SetBpf(long fd, slice<BpfInsn> i)
        {
            ref BpfProgram p = ref heap(out ptr<BpfProgram> _addr_p);
            p.Len = uint32(len(i));
            p.Insns = (BpfInsn.val)(@unsafe.Pointer(_addr_i[0L]));
            var err = ioctlPtr(fd, BIOCSETF, @unsafe.Pointer(_addr_p));
            if (err != null)
            {
                return error.As(err)!;
            }

            return error.As(null!)!;

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error CheckBpfVersion(long fd)
        {
            ref BpfVersion v = ref heap(out ptr<BpfVersion> _addr_v);
            var err = ioctlPtr(fd, BIOCVERSION, @unsafe.Pointer(_addr_v));
            if (err != null)
            {
                return error.As(err)!;
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
            var err = ioctlPtr(fd, BIOCGHDRCMPLT, @unsafe.Pointer(_addr_f));
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            return (f, error.As(null!)!);

        }

        // Deprecated: Use golang.org/x/net/bpf instead.
        public static error SetBpfHeadercmpl(long fd, long f)
        {
            var err = ioctlPtr(fd, BIOCSHDRCMPLT, @unsafe.Pointer(_addr_f));
            if (err != null)
            {
                return error.As(err)!;
            }

            return error.As(null!)!;

        }
    }
}
