// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd
// +build aix darwin dragonfly freebsd linux netbsd openbsd

// package ld -- go2cs converted at 2022 March 06 23:22:04 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\outbuf_mmap.go
using syscall = go.syscall_package;

namespace go.cmd.link.@internal;

public static partial class ld_package {

    // Mmap maps the output file with the given size. It unmaps the old mapping
    // if it is already mapped. It also flushes any in-heap data to the new
    // mapping.
private static error Mmap(this ptr<OutBuf> _addr_@out, ulong filesize) => func((_, panic, _) => {
    error err = default!;
    ref OutBuf @out = ref _addr_@out.val;

    var oldlen = len(@out.buf);
    if (oldlen != 0) {
        @out.munmap();
    }
    while (true) {
        err = @out.fallocate(filesize);

        if (err != syscall.EINTR) {
            break;
        }
    }
    if (err != null) { 
        // Some file systems do not support fallocate. We ignore that error as linking
        // can still take place, but you might SIGBUS when you write to the mmapped
        // area.
        if (err != syscall.ENOTSUP && err != syscall.EPERM && err != errNoFallocate) {
            return error.As(err)!;
        }
    }
    err = @out.f.Truncate(int64(filesize));
    if (err != null) {
        Exitf("resize output file failed: %v", err);
    }
    @out.buf, err = syscall.Mmap(int(@out.f.Fd()), 0, int(filesize), syscall.PROT_READ | syscall.PROT_WRITE, syscall.MAP_SHARED | syscall.MAP_FILE);
    if (err != null) {
        return error.As(err)!;
    }
    if (uint64(oldlen + len(@out.heap)) > filesize) {
        panic("mmap size too small");
    }
    copy(@out.buf[(int)oldlen..], @out.heap);
    @out.heap = @out.heap[..(int)0];
    return error.As(null!)!;

});

private static void munmap(this ptr<OutBuf> _addr_@out) {
    ref OutBuf @out = ref _addr_@out.val;

    if (@out.buf == null) {
        return ;
    }
    syscall.Munmap(@out.buf);
    @out.buf = null;

}

} // end ld_package
