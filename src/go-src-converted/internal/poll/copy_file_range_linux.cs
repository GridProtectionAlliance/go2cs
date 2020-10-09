// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2020 October 09 04:50:53 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\copy_file_range_linux.go
using unix = go.@internal.syscall.unix_package;
using atomic = go.sync.atomic_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
    {
        private static int copyFileRangeSupported = 1L; // accessed atomically

        private static readonly long maxCopyFileRangeRound = (long)1L << (int)(30L);

        // CopyFileRange copies at most remain bytes of data from src to dst, using
        // the copy_file_range system call. dst and src must refer to regular files.


        // CopyFileRange copies at most remain bytes of data from src to dst, using
        // the copy_file_range system call. dst and src must refer to regular files.
        public static (long, bool, error) CopyFileRange(ptr<FD> _addr_dst, ptr<FD> _addr_src, long remain)
        {
            long written = default;
            bool handled = default;
            error err = default!;
            ref FD dst = ref _addr_dst.val;
            ref FD src = ref _addr_src.val;

            if (atomic.LoadInt32(_addr_copyFileRangeSupported) == 0L)
            {
                return (0L, false, error.As(null!)!);
            }

            while (remain > 0L)
            {
                var max = remain;
                if (max > maxCopyFileRangeRound)
                {
                    max = maxCopyFileRangeRound;
                }

                var (n, err) = copyFileRange(_addr_dst, _addr_src, int(max));

                if (err == syscall.ENOSYS) 
                    // copy_file_range(2) was introduced in Linux 4.5.
                    // Go supports Linux >= 2.6.33, so the system call
                    // may not be present.
                    //
                    // If we see ENOSYS, we have certainly not transfered
                    // any data, so we can tell the caller that we
                    // couldn't handle the transfer and let them fall
                    // back to more generic code.
                    //
                    // Seeing ENOSYS also means that we will not try to
                    // use copy_file_range(2) again.
                    atomic.StoreInt32(_addr_copyFileRangeSupported, 0L);
                    return (0L, false, error.As(null!)!);
                else if (err == syscall.EXDEV || err == syscall.EINVAL || err == syscall.EOPNOTSUPP || err == syscall.EPERM) 
                    // Prior to Linux 5.3, it was not possible to
                    // copy_file_range across file systems. Similarly to
                    // the ENOSYS case above, if we see EXDEV, we have
                    // not transfered any data, and we can let the caller
                    // fall back to generic code.
                    //
                    // As for EINVAL, that is what we see if, for example,
                    // dst or src refer to a pipe rather than a regular
                    // file. This is another case where no data has been
                    // transfered, so we consider it unhandled.
                    //
                    // If the file is on NFS, we can see EOPNOTSUPP.
                    // See issue #40731.
                    //
                    // If the process is running inside a Docker container,
                    // we might see EPERM instead of ENOSYS. See issue
                    // #40893. Since EPERM might also be a legitimate error,
                    // don't mark copy_file_range(2) as unsupported.
                    return (0L, false, error.As(null!)!);
                else if (err == null) 
                    if (n == 0L)
                    { 
                        // src is at EOF, which means we are done.
                        return (written, true, error.As(null!)!);

                    }

                    remain -= n;
                    written += n;
                else 
                    return (written, true, error.As(err)!);
                
            }

            return (written, true, error.As(null!)!);

        }

        // copyFileRange performs one round of copy_file_range(2).
        private static (long, error) copyFileRange(ptr<FD> _addr_dst, ptr<FD> _addr_src, long max) => func((defer, _, __) =>
        {
            long written = default;
            error err = default!;
            ref FD dst = ref _addr_dst.val;
            ref FD src = ref _addr_src.val;
 
            // The signature of copy_file_range(2) is:
            //
            // ssize_t copy_file_range(int fd_in, loff_t *off_in,
            //                         int fd_out, loff_t *off_out,
            //                         size_t len, unsigned int flags);
            //
            // Note that in the call to unix.CopyFileRange below, we use nil
            // values for off_in and off_out. For the system call, this means
            // "use and update the file offsets". That is why we must acquire
            // locks for both file descriptors (and why this whole machinery is
            // in the internal/poll package to begin with).
            {
                var err__prev1 = err;

                var err = dst.writeLock();

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

                err = err__prev1;

            }

            defer(dst.writeUnlock());
            {
                var err__prev1 = err;

                err = src.readLock();

                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

                err = err__prev1;

            }

            defer(src.readUnlock());
            long n = default;
            while (true)
            {
                n, err = unix.CopyFileRange(src.Sysfd, null, dst.Sysfd, null, max, 0L);
                if (err != syscall.EINTR)
                {
                    break;
                }

            }

            return (int64(n), error.As(err)!);

        });
    }
}}
