// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Windows environment variables.

// package windows -- go2cs converted at 2022 March 06 23:30:33 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\windows\env_windows.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class windows_package {

public static (@string, bool) Getenv(@string key) {
    @string value = default;
    bool found = default;

    return syscall.Getenv(key);
}

public static error Setenv(@string key, @string value) {
    return error.As(syscall.Setenv(key, value))!;
}

public static void Clearenv() {
    syscall.Clearenv();
}

public static slice<@string> Environ() {
    return syscall.Environ();
}

// Returns a default environment associated with the token, rather than the current
// process. If inheritExisting is true, then this environment also inherits the
// environment of the current process.
public static (slice<@string>, error) Environ(this Token token, bool inheritExisting) => func((defer, _, _) => {
    slice<@string> env = default;
    error err = default!;

    ptr<ushort> block;
    err = CreateEnvironmentBlock(_addr_block, token, inheritExisting);
    if (err != null) {
        return (null, error.As(err)!);
    }
    defer(DestroyEnvironmentBlock(block));
    var blockp = uintptr(@unsafe.Pointer(block));
    while (true) {
        var entry = UTF16PtrToString((uint16.val)(@unsafe.Pointer(blockp)));
        if (len(entry) == 0) {
            break;
        }
        env = append(env, entry);
        blockp += 2 * (uintptr(len(entry)) + 1);

    }
    return (env, error.As(null!)!);

});

public static error Unsetenv(@string key) {
    return error.As(syscall.Unsetenv(key))!;
}

} // end windows_package
