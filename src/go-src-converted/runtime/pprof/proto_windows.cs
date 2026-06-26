// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using errors = errors_package;
using windows = @internal.syscall.windows_package;
using os = os_package;
using syscall = syscall_package;
using @internal.syscall;

partial class pprof_package {

// readMapping adds memory mapping information to the profile.
[GoRecv] internal static void readMapping(this ref profileBuilder b) => func((defer, _) => {
    var (snap, err) = createModuleSnapshot();
    if (err != default!) {
        // pprof expects a map entry, so fake one, when we haven't added anything yet.
        b.addMappingEntry(0, 0, 0, ""u8, ""u8, true);
        return;
    }
    defer(() => {
        _ = syscall.CloseHandle(snap);
    });
    ref var module = ref heap(new @internal.syscall.windows_package.ModuleEntry32(), out var Ꮡmodule);
    module.Size = ((uint32)windows.SizeofModuleEntry32);
    err = windows.Module32First(snap, Ꮡmodule);
    if (err != default!) {
        // pprof expects a map entry, so fake one, when we haven't added anything yet.
        b.addMappingEntry(0, 0, 0, ""u8, ""u8, true);
        return;
    }
    while (err == default!) {
        @string exe = syscall.UTF16ToString(module.ExePath[..]);
        b.addMappingEntry(
            ((uint64)module.ModBaseAddr),
            ((uint64)module.ModBaseAddr) + ((uint64)module.ModBaseSize),
            0,
            exe,
            peBuildID(exe),
            false);
        err = windows.Module32Next(snap, Ꮡmodule);
    }
});

internal static (uint64 start, uint64 end, @string exe, @string buildID, error err) readMainModuleMapping() => func((defer, _) => {
    uint64 start = default!;
    uint64 end = default!;
    @string exe = default!;
    @string buildID = default!;
    error err = default!;

    (exe, err) = os.Executable();
    if (err != default!) {
        return (0, 0, "", "", err);
    }
    var (snap, err) = createModuleSnapshot();
    if (err != default!) {
        return (0, 0, "", "", err);
    }
    defer(() => {
        _ = syscall.CloseHandle(snap);
    });
    ref var module = ref heap(new @internal.syscall.windows_package.ModuleEntry32(), out var Ꮡmodule);
    module.Size = ((uint32)windows.SizeofModuleEntry32);
    err = windows.Module32First(snap, Ꮡmodule);
    if (err != default!) {
        return (0, 0, "", "", err);
    }
    return (((uint64)module.ModBaseAddr), ((uint64)module.ModBaseAddr) + ((uint64)module.ModBaseSize), exe, peBuildID(exe), default!);
});

internal static (syscallꓸHandle, error) createModuleSnapshot() {
    while (ᐧ) {
        var (snap, err) = syscall.CreateToolhelp32Snapshot((uint32)(windows.TH32CS_SNAPMODULE | windows.TH32CS_SNAPMODULE32), ((uint32)syscall.Getpid()));
        ref var errno = ref heap(new syscall_package.Errno(), out var Ꮡerrno);
        if (err != default! && errors.As(err, Ꮡerrno) && errno == windows.ERROR_BAD_LENGTH) {
            // When CreateToolhelp32Snapshot(SNAPMODULE|SNAPMODULE32, ...) fails
            // with ERROR_BAD_LENGTH then it should be retried until it succeeds.
            continue;
        }
        return (snap, err);
    }
}

} // end pprof_package
