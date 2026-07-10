// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build windows
namespace go.@internal.syscall;

using Δwindows = go.@internal.syscall.windows_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using go.@internal.syscall;

partial class execenv_package {

// Default will return the default environment
// variables based on the process attributes
// provided.
//
// If the process attributes contain a token, then
// the environment variables will be sourced from
// the defaults for that user token, otherwise they
// will be sourced from syscall.Environ().
public static (slice<@string> env, error err) Default(ж<syscall.SysProcAttr> Ꮡsys) {
    slice<@string> env = default!;
    error err = default!;
    func((defer, recover) => {
    ref var sys = ref Ꮡsys.DerefOrNil();

        if (Ꮡsys == nil || sys.Token == 0) {
            (env, err) = (syscall.Environ(), default!); return;
        }
        ref var blockp = ref heap<ж<uint16>>(out var Ꮡblockp);
        err = Δwindows.CreateEnvironmentBlock(Ꮡblockp, sys.Token, false);
        if (err != default!) {
            (env, err) = (default!, err); return;
        }
        deferǃ(Δwindows.DestroyEnvironmentBlock, blockp, defer);
        uintptr size = /* unsafe.Sizeof(*blockp) */ 2;
        while (blockp.Value != 0) {
            // environment block ends with empty string
            // find NUL terminator
            @unsafe.Pointer end = (uintptr)@unsafe.Add(new @unsafe.Pointer(blockp), size);
            while (~(ж<uint16>)(uintptr)(end) != 0) {
                end = (uintptr)@unsafe.Add(end, size);
            }
            var entry = @unsafe.Slice(blockp, ((uintptr)end - (uintptr)new @unsafe.Pointer(blockp)) / 2);
            env = append(env, syscall.UTF16ToString(entry));
            blockp = (ж<uint16>)(uintptr)(@unsafe.Add(end, size));
        }
    });
    return (env, err);
}

} // end execenv_package
