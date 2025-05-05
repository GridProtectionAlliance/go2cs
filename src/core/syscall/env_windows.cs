// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Windows environment variables.
namespace go;

using @unsafe = unsafe_package;

partial class syscall_package {

public static (@string value, bool found) Getenv(@string key) {
    @string value = default!;
    bool found = default!;

    (keyp, err) = UTF16PtrFromString(key);
    if (err != default!) {
        return ("", false);
    }
    var n = ((uint32)100);
    while (ᐧ) {
        var b = new slice<uint16>(n);
        (n, err) = GetEnvironmentVariable(keyp, Ꮡ(b, 0), ((uint32)len(b)));
        if (n == 0 && err == ERROR_ENVVAR_NOT_FOUND) {
            return ("", false);
        }
        if (n <= ((uint32)len(b))) {
            return (UTF16ToString(b[..(int)(n)]), true);
        }
    }
}

public static error Setenv(@string key, @string value) {
    (v, err) = UTF16PtrFromString(value);
    if (err != default!) {
        return err;
    }
    (keyp, err) = UTF16PtrFromString(key);
    if (err != default!) {
        return err;
    }
    var e = SetEnvironmentVariable(keyp, v);
    if (e != default!) {
        return e;
    }
    runtimeSetenv(key, value);
    return default!;
}

public static error Unsetenv(@string key) {
    (keyp, err) = UTF16PtrFromString(key);
    if (err != default!) {
        return err;
    }
    var e = SetEnvironmentVariable(keyp, nil);
    if (e != default!) {
        return e;
    }
    runtimeUnsetenv(key);
    return default!;
}

public static void Clearenv() {
    foreach (var (_, s) in Environ()) {
        // Environment variables can begin with =
        // so start looking for the separator = at j=1.
        // https://devblogs.microsoft.com/oldnewthing/20100506-00/?p=14133
        for (nint j = 1; j < len(s); j++) {
            if (s[j] == (rune)'=') {
                Unsetenv(s[0..(int)(j)]);
                break;
            }
        }
    }
}

public static slice<@string> Environ() => func((defer, _) => {
    (envp, e) = GetEnvironmentStrings();
    if (e != default!) {
        return default!;
    }
    deferǃ(FreeEnvironmentStrings, envp, defer);
    var r = new slice<@string>(0, 50);
    // Empty with room to grow.
    const uintptr size = /* unsafe.Sizeof(*envp) */ 2;
    while (envp.val != 0) {
        // environment block ends with empty string
        // find NUL terminator
        @unsafe.Pointer end = new @unsafe.Pointer(envp);
        while (~(ж<uint16>)(uintptr)(end) != 0) {
            end = (uintptr)@unsafe.Add(end, size);
        }
        var entry = @unsafe.Slice(envp, (((uintptr)end) - ((uintptr)new @unsafe.Pointer(envp))) / size);
        r = append(r, UTF16ToString(entry));
        envp = (ж<uint16>)(uintptr)(@unsafe.Add(end, size));
    }
    return r;
});

} // end syscall_package
