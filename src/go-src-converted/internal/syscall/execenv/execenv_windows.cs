// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build windows

// package execenv -- go2cs converted at 2020 October 09 04:58:34 UTC
// import "internal/syscall/execenv" ==> using execenv = go.@internal.syscall.execenv_package
// Original source: C:\Go\src\internal\syscall\execenv\execenv_windows.go
using windows = go.@internal.syscall.windows_package;
using syscall = go.syscall_package;
using utf16 = go.unicode.utf16_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class execenv_package
    {
        // Default will return the default environment
        // variables based on the process attributes
        // provided.
        //
        // If the process attributes contain a token, then
        // the environment variables will be sourced from
        // the defaults for that user token, otherwise they
        // will be sourced from syscall.Environ().
        public static (slice<@string>, error) Default(ptr<syscall.SysProcAttr> _addr_sys) => func((defer, _, __) =>
        {
            slice<@string> env = default;
            error err = default!;
            ref syscall.SysProcAttr sys = ref _addr_sys.val;

            if (sys == null || sys.Token == 0L)
            {
                return (syscall.Environ(), error.As(null!)!);
            }
            ptr<ushort> block;
            err = windows.CreateEnvironmentBlock(_addr_block, sys.Token, false);
            if (err != null)
            {
                return (null, error.As(err)!);
            }
            defer(windows.DestroyEnvironmentBlock(block));
            var blockp = uintptr(@unsafe.Pointer(block));
            while (true)
            {
                // find NUL terminator
                var end = @unsafe.Pointer(blockp);
                while (new ptr<ptr<ptr<ushort>>>(end) != 0L)
                {
                    end = @unsafe.Pointer(uintptr(end) + 2L);
                }

                var n = (uintptr(end) - uintptr(@unsafe.Pointer(blockp))) / 2L;
                if (n == 0L)
                { 
                    // environment block ends with empty string
                    break;

                }
                ptr<array<ushort>> entry = new ptr<ptr<array<ushort>>>(@unsafe.Pointer(blockp)).slice(-1, n, n);
                env = append(env, string(utf16.Decode(entry)));
                blockp += 2L * (uintptr(len(entry)) + 1L);

            }
            return ;

        });
    }
}}}
