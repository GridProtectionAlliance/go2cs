// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// illumos system calls not present on Solaris.

// +build amd64,illumos

// package unix -- go2cs converted at 2020 October 08 04:47:15 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_illumos.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class unix_package
    {
        private static slice<Iovec> bytes2iovec(slice<slice<byte>> bs)
        {
            var iovecs = make_slice<Iovec>(len(bs));
            foreach (var (i, b) in bs)
            {
                iovecs[i].SetLen(len(b));
                if (len(b) > 0L)
                { 
                    // somehow Iovec.Base on illumos is (*int8), not (*byte)
                    iovecs[i].Base = (int8.val)(@unsafe.Pointer(_addr_b[0L]));

                }
                else
                {
                    iovecs[i].Base = (int8.val)(@unsafe.Pointer(_addr__zero));
                }
            }            return iovecs;

        }

        //sys   readv(fd int, iovs []Iovec) (n int, err error)

        public static (long, error) Readv(long fd, slice<slice<byte>> iovs)
        {
            long n = default;
            error err = default!;

            var iovecs = bytes2iovec(iovs);
            n, err = readv(fd, iovecs);
            return (n, error.As(err)!);
        }

        //sys   preadv(fd int, iovs []Iovec, off int64) (n int, err error)

        public static (long, error) Preadv(long fd, slice<slice<byte>> iovs, long off)
        {
            long n = default;
            error err = default!;

            var iovecs = bytes2iovec(iovs);
            n, err = preadv(fd, iovecs, off);
            return (n, error.As(err)!);
        }

        //sys   writev(fd int, iovs []Iovec) (n int, err error)

        public static (long, error) Writev(long fd, slice<slice<byte>> iovs)
        {
            long n = default;
            error err = default!;

            var iovecs = bytes2iovec(iovs);
            n, err = writev(fd, iovecs);
            return (n, error.As(err)!);
        }

        //sys   pwritev(fd int, iovs []Iovec, off int64) (n int, err error)

        public static (long, error) Pwritev(long fd, slice<slice<byte>> iovs, long off)
        {
            long n = default;
            error err = default!;

            var iovecs = bytes2iovec(iovs);
            n, err = pwritev(fd, iovecs, off);
            return (n, error.As(err)!);
        }
    }
}}}}}}
