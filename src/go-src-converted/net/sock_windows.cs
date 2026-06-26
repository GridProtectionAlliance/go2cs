// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using windows = @internal.syscall.windows_package;
using os = os_package;
using syscall = syscall_package;
using @internal.syscall;

partial class net_package {

internal static nint maxListenerBacklog() {
    // When the socket backlog is SOMAXCONN, Windows will set the backlog to
    // "a reasonable maximum value".
    // See: https://learn.microsoft.com/en-us/windows/win32/api/winsock2/nf-winsock2-listen
    return syscall.SOMAXCONN;
}

internal static (syscallꓸHandle, error) sysSocket(nint family, nint sotype, nint proto) {
    var (s, err) = wsaSocketFunc(((int32)family), ((int32)sotype), ((int32)proto),
        nil, 0, (uint32)(windows.WSA_FLAG_OVERLAPPED | windows.WSA_FLAG_NO_HANDLE_INHERIT));
    if (err != default!) {
        return (syscall.InvalidHandle, os.NewSyscallError("socket"u8, err));
    }
    return (s, default!);
}

} // end net_package
