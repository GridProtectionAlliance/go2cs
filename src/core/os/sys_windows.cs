// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using windows = @internal.syscall.windows_package;
using syscall = syscall_package;
using @internal.syscall;

partial class os_package {

internal static (@string name, error err) hostname() {
    @string name = default!;
    error err = default!;

    // Use PhysicalDnsHostname to uniquely identify host in a cluster
    static readonly UntypedInt format = /* windows.ComputerNamePhysicalDnsHostname */ 5;
    ref var n = ref heap<uint32>(out var Ꮡn);
    n = ((uint32)64);
    while (ᐧ) {
        var b = new slice<uint16>(n);
        var errΔ1 = windows.GetComputerNameEx(format, Ꮡ(b, 0), Ꮡn);
        if (errΔ1 == default!) {
            return (syscall.UTF16ToString(b[..(int)(n)]), default!);
        }
        if (errΔ1 != syscall.ERROR_MORE_DATA) {
            return ("", NewSyscallError("ComputerNameEx"u8, errΔ1));
        }
        // If we received an ERROR_MORE_DATA, but n doesn't get larger,
        // something has gone wrong and we may be in an infinite loop
        if (n <= ((uint32)len(b))) {
            return ("", NewSyscallError("ComputerNameEx"u8, errΔ1));
        }
    }
}

} // end os_package
