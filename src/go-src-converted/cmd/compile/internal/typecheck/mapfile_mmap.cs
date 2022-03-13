// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin || dragonfly || freebsd || linux || netbsd || openbsd
// +build darwin dragonfly freebsd linux netbsd openbsd

// package typecheck -- go2cs converted at 2022 March 13 05:59:58 UTC
// import "cmd/compile/internal/typecheck" ==> using typecheck = go.cmd.compile.@internal.typecheck_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\typecheck\mapfile_mmap.go
namespace go.cmd.compile.@internal;

using os = os_package;
using reflect = reflect_package;
using syscall = syscall_package;
using @unsafe = @unsafe_package;


// TODO(mdempsky): Is there a higher-level abstraction that still
// works well for iimport?

// mapFile returns length bytes from the file starting at the
// specified offset as a string.

using System;
public static partial class typecheck_package {

private static (@string, error) mapFile(ptr<os.File> _addr_f, long offset, long length) {
    @string _p0 = default;
    error _p0 = default!;
    ref os.File f = ref _addr_f.val;
 
    // POSIX mmap: "The implementation may require that off is a
    // multiple of the page size."
    var x = offset & int64(os.Getpagesize() - 1);
    offset -= x;
    length += x;

    var (buf, err) = syscall.Mmap(int(f.Fd()), offset, int(length), syscall.PROT_READ, syscall.MAP_SHARED);
    keepAlive(f);
    if (err != null) {
        return ("", error.As(err)!);
    }
    buf = buf[(int)x..];
    var pSlice = (reflect.SliceHeader.val)(@unsafe.Pointer(_addr_buf));

    ref @string res = ref heap(out ptr<@string> _addr_res);
    var pString = (reflect.StringHeader.val)(@unsafe.Pointer(_addr_res));

    pString.Data = pSlice.Data;
    pString.Len = pSlice.Len;

    return (res, error.As(null!)!);
}

// keepAlive is a reimplementation of runtime.KeepAlive, which wasn't
// added until Go 1.7, whereas we need to compile with Go 1.4.
private static Action<object> keepAlive = _p0 => {
};

} // end typecheck_package
