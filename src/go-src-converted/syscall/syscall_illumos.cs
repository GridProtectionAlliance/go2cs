// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build illumos
// +build illumos

// Illumos system calls not present on Solaris.

// package syscall -- go2cs converted at 2022 March 06 22:26:58 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\syscall_illumos.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class syscall_package {

    //go:cgo_import_dynamic libc_accept4 accept4 "libsocket.so"
    //go:cgo_import_dynamic libc_flock flock "libc.so"

    //go:linkname procAccept4 libc_accept4
    //go:linkname procFlock libc_flock
private static libcFunc procAccept4 = default;private static libcFunc procFlock = default;


public static (nint, Sockaddr, error) Accept4(nint fd, nint flags) => func((_, panic, _) => {
    nint _p0 = default;
    Sockaddr _p0 = default;
    error _p0 = default!;

    ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
    ref _Socklen addrlen = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_addrlen);
    var (nfd, _, errno) = sysvicall6(uintptr(@unsafe.Pointer(_addr_procAccept4)), 4, uintptr(fd), uintptr(@unsafe.Pointer(_addr_rsa)), uintptr(@unsafe.Pointer(_addr_addrlen)), uintptr(flags), 0, 0);
    if (errno != 0) {
        return (0, null, error.As(errno)!);
    }
    if (addrlen > SizeofSockaddrAny) {
        panic("RawSockaddrAny too small");
    }
    var (sa, err) = anyToSockaddr(_addr_rsa);
    if (err != null) {
        Close(int(nfd));
        return (0, null, error.As(err)!);
    }
    return (int(nfd), sa, error.As(null!)!);

});

public static error Flock(nint fd, nint how) {
    var (_, _, errno) = sysvicall6(uintptr(@unsafe.Pointer(_addr_procFlock)), 2, uintptr(fd), uintptr(how), 0, 0, 0, 0);
    if (errno != 0) {
        return error.As(errno)!;
    }
    return error.As(null!)!;

}

} // end syscall_package
