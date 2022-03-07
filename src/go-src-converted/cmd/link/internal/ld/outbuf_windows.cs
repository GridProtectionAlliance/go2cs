// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2022 March 06 23:22:05 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\outbuf_windows.go
using reflect = go.reflect_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.cmd.link.@internal;

public static partial class ld_package {

    // Mmap maps the output file with the given size. It unmaps the old mapping
    // if it is already mapped. It also flushes any in-heap data to the new
    // mapping.
private static error Mmap(this ptr<OutBuf> _addr_@out, ulong filesize) => func((defer, panic, _) => {
    ref OutBuf @out = ref _addr_@out.val;

    var oldlen = len(@out.buf);
    if (oldlen != 0) {
        @out.munmap();
    }
    var err = @out.f.Truncate(int64(filesize));
    if (err != null) {
        Exitf("resize output file failed: %v", err);
    }
    var low = uint32(filesize);
    var high = uint32(filesize >> 32);
    var (fmap, err) = syscall.CreateFileMapping(syscall.Handle(@out.f.Fd()), null, syscall.PAGE_READWRITE, high, low, null);
    if (err != null) {
        return error.As(err)!;
    }
    defer(syscall.CloseHandle(fmap));

    var (ptr, err) = syscall.MapViewOfFile(fmap, syscall.FILE_MAP_READ | syscall.FILE_MAP_WRITE, 0, 0, uintptr(filesize));
    if (err != null) {
        return error.As(err)!;
    }
    var bufHdr = (reflect.SliceHeader.val)(@unsafe.Pointer(_addr_@out.buf));
    bufHdr.Data = ptr;
    bufHdr.Len = int(filesize);
    bufHdr.Cap = int(filesize); 

    // copy heap to new mapping
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
    var err = syscall.FlushViewOfFile(uintptr(@unsafe.Pointer(_addr_@out.buf[0])), 0);
    if (err != null) {
        Exitf("FlushViewOfFile failed: %v", err);
    }
    err = syscall.UnmapViewOfFile(uintptr(@unsafe.Pointer(_addr_@out.buf[0])));
    @out.buf = null;
    if (err != null) {
        Exitf("UnmapViewOfFile failed: %v", err);
    }
}

} // end ld_package
