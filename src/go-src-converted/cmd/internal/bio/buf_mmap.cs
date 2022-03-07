// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin || dragonfly || freebsd || linux || netbsd || openbsd
// +build darwin dragonfly freebsd linux netbsd openbsd

// package bio -- go2cs converted at 2022 March 06 22:32:21 UTC
// import "cmd/internal/bio" ==> using bio = go.cmd.@internal.bio_package
// Original source: C:\Program Files\Go\src\cmd\internal\bio\buf_mmap.go
using runtime = go.runtime_package;
using atomic = go.sync.atomic_package;
using syscall = go.syscall_package;

namespace go.cmd.@internal;

public static partial class bio_package {

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
private static int mmapLimit = 1 << 31 - 1;

private static void init() { 
    // Linux is the only practically concerning OS.
    if (runtime.GOOS == "linux") {
        mmapLimit = 30000;
    }
}

private static (slice<byte>, bool) sliceOS(this ptr<Reader> _addr_r, ulong length) {
    slice<byte> _p0 = default;
    bool _p0 = default;
    ref Reader r = ref _addr_r.val;
 
    // For small slices, don't bother with the overhead of a
    // mapping, especially since we have no way to unmap it.
    const nint threshold = 16 << 10;

    if (length < threshold) {
        return (null, false);
    }
    if (atomic.AddInt32(_addr_mmapLimit, -1) < 0) {
        atomic.AddInt32(_addr_mmapLimit, 1);
        return (null, false);
    }
    var off = r.Offset();
    var align = syscall.Getpagesize();
    var aoff = off & ~int64(align - 1);

    var (data, err) = syscall.Mmap(int(r.f.Fd()), aoff, int(length + uint64(off - aoff)), syscall.PROT_READ, syscall.MAP_SHARED | syscall.MAP_FILE);
    if (err != null) {
        return (null, false);
    }
    data = data[(int)off - aoff..];
    r.MustSeek(int64(length), 1);
    return (data, true);

}

} // end bio_package
