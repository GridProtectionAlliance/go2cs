// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 08 03:44:58 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\readfrom_linux.go
using poll = go.@internal.poll_package;
using io = go.io_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        private static var pollCopyFileRange = poll.CopyFileRange;

        private static (long, bool, error) readFrom(this ptr<File> _addr_f, io.Reader r)
        {
            long written = default;
            bool handled = default;
            error err = default!;
            ref File f = ref _addr_f.val;
 
            // copy_file_range(2) does not support destinations opened with
            // O_APPEND, so don't even try.
            if (f.appendMode)
            {
                return (0L, false, error.As(null!)!);
            }

            var remain = int64(1L << (int)(62L));

            ptr<io.LimitedReader> (lr, ok) = r._<ptr<io.LimitedReader>>();
            if (ok)
            {
                remain = lr.N;
                r = lr.R;
                if (remain <= 0L)
                {
                    return (0L, true, error.As(null!)!);
                }

            }

            ptr<File> (src, ok) = r._<ptr<File>>();
            if (!ok)
            {
                return (0L, false, error.As(null!)!);
            }

            if (src.checkValid("ReadFrom") != null)
            { 
                // Avoid returning the error as we report handled as false,
                // leave further error handling as the responsibility of the caller.
                return (0L, false, error.As(null!)!);

            }

            written, handled, err = pollCopyFileRange(_addr_f.pfd, _addr_src.pfd, remain);
            if (lr != null)
            {
                lr.N -= written;
            }

            return (written, handled, error.As(NewSyscallError("copy_file_range", err))!);

        }
    }
}
