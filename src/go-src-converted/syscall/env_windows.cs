// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Windows environment variables.

// package syscall -- go2cs converted at 2022 March 06 22:26:26 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\env_windows.go
using utf16 = go.unicode.utf16_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class syscall_package {

public static (@string, bool) Getenv(@string key) {
    @string value = default;
    bool found = default;

    var (keyp, err) = UTF16PtrFromString(key);
    if (err != null) {
        return ("", false);
    }
    var n = uint32(100);
    while (true) {
        var b = make_slice<ushort>(n);
        n, err = GetEnvironmentVariable(keyp, _addr_b[0], uint32(len(b)));
        if (n == 0 && err == ERROR_ENVVAR_NOT_FOUND) {
            return ("", false);
        }
        if (n <= uint32(len(b))) {
            return (string(utf16.Decode(b[..(int)n])), true);
        }
    }

}

public static error Setenv(@string key, @string value) {
    var (v, err) = UTF16PtrFromString(value);
    if (err != null) {
        return error.As(err)!;
    }
    var (keyp, err) = UTF16PtrFromString(key);
    if (err != null) {
        return error.As(err)!;
    }
    var e = SetEnvironmentVariable(keyp, v);
    if (e != null) {
        return error.As(e)!;
    }
    return error.As(null!)!;

}

public static error Unsetenv(@string key) {
    var (keyp, err) = UTF16PtrFromString(key);
    if (err != null) {
        return error.As(err)!;
    }
    return error.As(SetEnvironmentVariable(keyp, null))!;

}

public static void Clearenv() {
    foreach (var (_, s) in Environ()) { 
        // Environment variables can begin with =
        // so start looking for the separator = at j=1.
        // https://blogs.msdn.com/b/oldnewthing/archive/2010/05/06/10008132.aspx
        for (nint j = 1; j < len(s); j++) {
            if (s[j] == '=') {
                Unsetenv(s[(int)0..(int)j]);
                break;
            }
        }

    }
}

public static slice<@string> Environ() => func((defer, _, _) => {
    var (s, e) = GetEnvironmentStrings();
    if (e != null) {
        return null;
    }
    defer(FreeEnvironmentStrings(s));
    var r = make_slice<@string>(0, 50); // Empty with room to grow.
    for (nint from = 0;
    nint i = 0;
    ptr<array<ushort>> p = new ptr<ptr<array<ushort>>>(@unsafe.Pointer(s)); true; i++) {
        if (p[i] == 0) { 
            // empty string marks the end
            if (i <= from) {
                break;
            }

            r = append(r, string(utf16.Decode(p[(int)from..(int)i])));
            from = i + 1;

        }
    }
    return r;

});

} // end syscall_package
