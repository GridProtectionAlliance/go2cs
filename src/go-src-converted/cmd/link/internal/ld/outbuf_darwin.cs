// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2022 March 06 23:22:03 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\outbuf_darwin.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.cmd.link.@internal;

public static partial class ld_package {

private static error fallocate(this ptr<OutBuf> _addr_@out, ulong size) {
    ref OutBuf @out = ref _addr_@out.val;

    var (stat, err) = @out.f.Stat();
    if (err != null) {
        return error.As(err)!;
    }
    var cursize = uint64(stat.Sys()._<ptr<syscall.Stat_t>>().Blocks * 512); // allocated size
    if (size <= cursize) {
        return error.As(null!)!;
    }
    ptr<syscall.Fstore_t> store = addr(new syscall.Fstore_t(Flags:syscall.F_ALLOCATEALL,Posmode:syscall.F_PEOFPOSMODE,Offset:0,Length:int64(size-cursize),));

    var (_, _, errno) = syscall.Syscall(syscall.SYS_FCNTL, uintptr(@out.f.Fd()), syscall.F_PREALLOCATE, uintptr(@unsafe.Pointer(store)));
    if (errno != 0) {
        return error.As(errno)!;
    }
    return error.As(null!)!;

}

private static void purgeSignatureCache(this ptr<OutBuf> _addr_@out) {
    ref OutBuf @out = ref _addr_@out.val;
 
    // Apparently, the Darwin kernel may cache the code signature at mmap.
    // When we mmap the output buffer, it doesn't have a code signature
    // (as we haven't generated one). Invalidate the kernel cache now that
    // we have generated the signature. See issue #42684.
    syscall.Syscall(syscall.SYS_MSYNC, uintptr(@unsafe.Pointer(_addr_@out.buf[0])), uintptr(len(@out.buf)), syscall.MS_INVALIDATE); 
    // Best effort. Ignore error.
}

} // end ld_package
