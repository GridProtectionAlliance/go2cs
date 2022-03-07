// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !aix && !darwin && !dragonfly && !freebsd && !linux && !netbsd && !openbsd && !windows
// +build !aix,!darwin,!dragonfly,!freebsd,!linux,!netbsd,!openbsd,!windows

// package ld -- go2cs converted at 2022 March 06 23:22:04 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\outbuf_nommap.go


namespace go.cmd.link.@internal;

public static partial class ld_package {

    // Mmap allocates an in-heap output buffer with the given size. It copies
    // any old data (if any) to the new buffer.
private static error Mmap(this ptr<OutBuf> _addr_@out, ulong filesize) => func((_, panic, _) => {
    ref OutBuf @out = ref _addr_@out.val;
 
    // We need space to put all the symbols before we apply relocations.
    var oldheap = @out.heap;
    if (filesize < uint64(len(oldheap))) {
        panic("mmap size too small");
    }
    @out.heap = make_slice<byte>(filesize);
    copy(@out.heap, oldheap);
    return error.As(null!)!;

});

private static void munmap(this ptr<OutBuf> _addr_@out) => func((_, panic, _) => {
    ref OutBuf @out = ref _addr_@out.val;

    panic("unreachable");
});

} // end ld_package
