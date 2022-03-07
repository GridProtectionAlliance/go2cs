// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2022 March 06 22:12:50 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\copy_file_range_linux.go
using unix = go.@internal.syscall.unix_package;
using atomic = go.sync.atomic_package;
using syscall = go.syscall_package;

namespace go.@internal;

public static partial class poll_package {

private static int copyFileRangeSupported = -1; // accessed atomically

private static readonly nint maxCopyFileRangeRound = 1 << 30;



private static (nint, nint) kernelVersion() {
    nint major = default;
    nint minor = default;

    ref syscall.Utsname uname = ref heap(out ptr<syscall.Utsname> _addr_uname);
    {
        var err = syscall.Uname(_addr_uname);

        if (err != null) {
            return ;
        }
    }


    var rl = uname.Release;
    array<nint> values = new array<nint>(2);
    nint vi = 0;
    nint value = 0;
    foreach (var (_, c) in rl) {
        if ('0' <= c && c <= '9') {
            value = (value * 10) + int(c - '0');
        }
        else
 { 
            // Note that we're assuming N.N.N here.  If we see anything else we are likely to
            // mis-parse it.
            values[vi] = value;
            vi++;
            if (vi >= len(values)) {
                break;
            }

            value = 0;

        }
    }    switch (vi) {
        case 0: 
            return (0, 0);
            break;
        case 1: 
            return (values[0], 0);
            break;
        case 2: 
            return (values[0], values[1]);
            break;
    }
    return ;

}

// CopyFileRange copies at most remain bytes of data from src to dst, using
// the copy_file_range system call. dst and src must refer to regular files.
public static (long, bool, error) CopyFileRange(ptr<FD> _addr_dst, ptr<FD> _addr_src, long remain) {
    long written = default;
    bool handled = default;
    error err = default!;
    ref FD dst = ref _addr_dst.val;
    ref FD src = ref _addr_src.val;

    {
        var supported = atomic.LoadInt32(_addr_copyFileRangeSupported);

        if (supported == 0) {
            return (0, false, error.As(null!)!);
        }
        else if (supported == -1) {
            var (major, minor) = kernelVersion();
            if (major > 5 || (major == 5 && minor >= 3)) {
                atomic.StoreInt32(_addr_copyFileRangeSupported, 1);
            }
            else
 { 
                // copy_file_range(2) is broken in various ways on kernels older than 5.3,
                // see issue #42400 and
                // https://man7.org/linux/man-pages/man2/copy_file_range.2.html#VERSIONS
                atomic.StoreInt32(_addr_copyFileRangeSupported, 0);
                return (0, false, error.As(null!)!);

            }

        }

    }

    while (remain > 0) {
        var max = remain;
        if (max > maxCopyFileRangeRound) {
            max = maxCopyFileRangeRound;
        }
        var (n, err) = copyFileRange(_addr_dst, _addr_src, int(max));

        if (err == syscall.ENOSYS) 
            // copy_file_range(2) was introduced in Linux 4.5.
            // Go supports Linux >= 2.6.33, so the system call
            // may not be present.
            //
            // If we see ENOSYS, we have certainly not transferred
            // any data, so we can tell the caller that we
            // couldn't handle the transfer and let them fall
            // back to more generic code.
            //
            // Seeing ENOSYS also means that we will not try to
            // use copy_file_range(2) again.
            atomic.StoreInt32(_addr_copyFileRangeSupported, 0);
            return (0, false, error.As(null!)!);
        else if (err == syscall.EXDEV || err == syscall.EINVAL || err == syscall.EIO || err == syscall.EOPNOTSUPP || err == syscall.EPERM) 
            // Prior to Linux 5.3, it was not possible to
            // copy_file_range across file systems. Similarly to
            // the ENOSYS case above, if we see EXDEV, we have
            // not transferred any data, and we can let the caller
            // fall back to generic code.
            //
            // As for EINVAL, that is what we see if, for example,
            // dst or src refer to a pipe rather than a regular
            // file. This is another case where no data has been
            // transferred, so we consider it unhandled.
            //
            // If src and dst are on CIFS, we can see EIO.
            // See issue #42334.
            //
            // If the file is on NFS, we can see EOPNOTSUPP.
            // See issue #40731.
            //
            // If the process is running inside a Docker container,
            // we might see EPERM instead of ENOSYS. See issue
            // #40893. Since EPERM might also be a legitimate error,
            // don't mark copy_file_range(2) as unsupported.
            return (0, false, error.As(null!)!);
        else if (err == null) 
            if (n == 0) { 
                // If we did not read any bytes at all,
                // then this file may be in a file system
                // where copy_file_range silently fails.
                // https://lore.kernel.org/linux-fsdevel/20210126233840.GG4626@dread.disaster.area/T/#m05753578c7f7882f6e9ffe01f981bc223edef2b0
                if (written == 0) {
                    return (0, false, error.As(null!)!);
                } 
                // Otherwise src is at EOF, which means
                // we are done.
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
private static (long, error) copyFileRange(ptr<FD> _addr_dst, ptr<FD> _addr_src, nint max) => func((defer, _, _) => {
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

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }

    defer(dst.writeUnlock());
    {
        var err__prev1 = err;

        err = src.readLock();

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }

    defer(src.readUnlock());
    nint n = default;
    while (true) {
        n, err = unix.CopyFileRange(src.Sysfd, null, dst.Sysfd, null, max, 0);
        if (err != syscall.EINTR) {
            break;
        }
    }
    return (int64(n), error.As(err)!);

});

} // end poll_package
