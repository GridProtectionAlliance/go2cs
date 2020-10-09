// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd

// package bio -- go2cs converted at 2020 October 09 05:08:51 UTC
// import "cmd/internal/bio" ==> using bio = go.cmd.@internal.bio_package
// Original source: C:\Go\src\cmd\internal\bio\buf_mmap.go
using runtime = go.runtime_package;
using atomic = go.sync.atomic_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class bio_package
    {
        // mmapLimit is the maximum number of mmaped regions to create before
        // falling back to reading into a heap-allocated slice. This exists
        // because some operating systems place a limit on the number of
        // distinct mapped regions per process. As of this writing:
        //
        //  Darwin    unlimited
        //  DragonFly   1000000 (vm.max_proc_mmap)
        //  FreeBSD   unlimited
        //  Linux         65530 (vm.max_map_count) // TODO: query /proc/sys/vm/max_map_count?
        //  NetBSD    unlimited
        //  OpenBSD   unlimited
        private static int mmapLimit = 1L << (int)(31L) - 1L;

        private static void init()
        { 
            // Linux is the only practically concerning OS.
            if (runtime.GOOS == "linux")
            {
                mmapLimit = 30000L;
            }

        }

        private static (slice<byte>, bool) sliceOS(this ptr<Reader> _addr_r, ulong length)
        {
            slice<byte> _p0 = default;
            bool _p0 = default;
            ref Reader r = ref _addr_r.val;
 
            // For small slices, don't bother with the overhead of a
            // mapping, especially since we have no way to unmap it.
            const long threshold = (long)16L << (int)(10L);

            if (length < threshold)
            {
                return (null, false);
            } 

            // Have we reached the mmap limit?
            if (atomic.AddInt32(_addr_mmapLimit, -1L) < 0L)
            {
                atomic.AddInt32(_addr_mmapLimit, 1L);
                return (null, false);
            } 

            // Page-align the offset.
            var off = r.Offset();
            var align = syscall.Getpagesize();
            var aoff = off & ~int64(align - 1L);

            var (data, err) = syscall.Mmap(int(r.f.Fd()), aoff, int(length + uint64(off - aoff)), syscall.PROT_READ, syscall.MAP_SHARED | syscall.MAP_FILE);
            if (err != null)
            {
                return (null, false);
            }

            data = data[off - aoff..];
            r.MustSeek(int64(length), 1L);
            return (data, true);

        }
    }
}}}
