// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2022 March 06 22:13:52 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\sys_aix.go
using syscall = go.syscall_package;

namespace go;

public static partial class os_package {

    // gethostname syscall cannot be used because it also returns the domain.
    // Therefore, hostname is retrieve with uname syscall and the Nodename field.
private static (@string, error) hostname() {
    @string name = default;
    error err = default!;

    ref syscall.Utsname u = ref heap(out ptr<syscall.Utsname> _addr_u);
    {
        var errno = syscall.Uname(_addr_u);

        if (errno != null) {
            return ("", error.As(NewSyscallError("uname", errno))!);
        }
    }

    var b = make_slice<byte>(len(u.Nodename));
    nint i = 0;
    while (i < len(u.Nodename)) {
        if (u.Nodename[i] == 0) {
            break;
        i++;
        }
        b[i] = byte(u.Nodename[i]);

    }
    return (string(b[..(int)i]), error.As(null!)!);

}

} // end os_package
