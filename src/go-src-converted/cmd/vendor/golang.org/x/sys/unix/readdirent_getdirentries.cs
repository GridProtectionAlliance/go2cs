// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin
// +build darwin

// package unix -- go2cs converted at 2022 March 06 23:26:39 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\readdirent_getdirentries.go
using @unsafe = go.@unsafe_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    // ReadDirent reads directory entries from fd and writes them into buf.
public static (nint, error) ReadDirent(nint fd, slice<byte> buf) {
    nint n = default;
    error err = default!;
 
    // Final argument is (basep *uintptr) and the syscall doesn't take nil.
    // 64 bits should be enough. (32 bits isn't even on 386). Since the
    // actual system call is getdirentries64, 64 is a good guess.
    // TODO(rsc): Can we use a single global basep for all calls?
    var @base = (uintptr.val)(@unsafe.Pointer(@new<uint64>()));
    return Getdirentries(fd, buf, base);

}

} // end unix_package
