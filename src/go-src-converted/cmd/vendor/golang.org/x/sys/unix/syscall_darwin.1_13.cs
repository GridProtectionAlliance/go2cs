// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin && go1.13
// +build darwin,go1.13

// package unix -- go2cs converted at 2022 March 06 23:26:46 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_darwin.1_13.go
using @unsafe = go.@unsafe_package;

using unsafeheader = go.golang.org.x.sys.@internal.unsafeheader_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    //sys    closedir(dir uintptr) (err error)
    //sys    readdir_r(dir uintptr, entry *Dirent, result **Dirent) (res Errno)
private static (System.UIntPtr, error) fdopendir(nint fd) {
    System.UIntPtr dir = default;
    error err = default!;

    var (r0, _, e1) = syscall_syscallPtr(libc_fdopendir_trampoline_addr, uintptr(fd), 0, 0);
    dir = uintptr(r0);
    if (e1 != 0) {
        err = errnoErr(e1);
    }
    return ;

}

private static System.UIntPtr libc_fdopendir_trampoline_addr = default;

//go:cgo_import_dynamic libc_fdopendir fdopendir "/usr/lib/libSystem.B.dylib"

public static (nint, error) Getdirentries(nint fd, slice<byte> buf, ptr<System.UIntPtr> _addr_basep) => func((defer, _, _) => {
    nint n = default;
    error err = default!;
    ref System.UIntPtr basep = ref _addr_basep.val;
 
    // Simulate Getdirentries using fdopendir/readdir_r/closedir.
    // We store the number of entries to skip in the seek
    // offset of fd. See issue #31368.
    // It's not the full required semantics, but should handle the case
    // of calling Getdirentries or ReadDirent repeatedly.
    // It won't handle assigning the results of lseek to *basep, or handle
    // the directory being edited underfoot.
    var (skip, err) = Seek(fd, 0, 1);
    if (err != null) {
        return (0, error.As(err)!);
    }
    var (fd2, err) = Openat(fd, ".", O_RDONLY, 0);
    if (err != null) {
        return (0, error.As(err)!);
    }
    var (d, err) = fdopendir(fd2);
    if (err != null) {
        Close(fd2);
        return (0, error.As(err)!);
    }
    defer(closedir(d));

    long cnt = default;
    while (true) {
        ref Dirent entry = ref heap(out ptr<Dirent> _addr_entry);
        ptr<Dirent> entryp;
        var e = readdir_r(d, _addr_entry, _addr_entryp);
        if (e != 0) {
            return (n, error.As(errnoErr(e))!);
        }
        if (entryp == null) {
            break;
        }
        if (skip > 0) {
            skip--;
            cnt++;
            continue;
        }
        var reclen = int(entry.Reclen);
        if (reclen > len(buf)) { 
            // Not enough room. Return for now.
            // The counter will let us know where we should start up again.
            // Note: this strategy for suspending in the middle and
            // restarting is O(n^2) in the length of the directory. Oh well.
            break;

        }
        ref slice<byte> s = ref heap(out ptr<slice<byte>> _addr_s);
        var hdr = (unsafeheader.Slice.val)(@unsafe.Pointer(_addr_s));
        hdr.Data = @unsafe.Pointer(_addr_entry);
        hdr.Cap = reclen;
        hdr.Len = reclen;
        copy(buf, s);

        buf = buf[(int)reclen..];
        n += reclen;
        cnt++;

    } 
    // Set the seek offset of the input fd to record
    // how many files we've already returned.
    _, err = Seek(fd, cnt, 0);
    if (err != null) {
        return (n, error.As(err)!);
    }
    return (n, error.As(null!)!);

});

} // end unix_package
