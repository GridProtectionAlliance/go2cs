// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Windows environment variables.

// package windows -- go2cs converted at 2020 October 08 04:53:44 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\windows\env_windows.go
using syscall = go.syscall_package;
using utf16 = go.unicode.utf16_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class windows_package
    {
        public static (@string, bool) Getenv(@string key)
        {
            @string value = default;
            bool found = default;

            return syscall.Getenv(key);
        }

        public static error Setenv(@string key, @string value)
        {
            return error.As(syscall.Setenv(key, value))!;
        }

        public static void Clearenv()
        {
            syscall.Clearenv();
        }

        public static slice<@string> Environ()
        {
            return syscall.Environ();
        }

        // Returns a default environment associated with the token, rather than the current
        // process. If inheritExisting is true, then this environment also inherits the
        // environment of the current process.
        public static (slice<@string>, error) Environ(this Token token, bool inheritExisting) => func((defer, _, __) =>
        {
            slice<@string> env = default;
            error err = default!;

            ptr<ushort> block;
            err = CreateEnvironmentBlock(_addr_block, token, inheritExisting);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            defer(DestroyEnvironmentBlock(block));
            var blockp = uintptr(@unsafe.Pointer(block));
            while (true)
            {
                ptr<array<ushort>> entry = new ptr<ptr<array<ushort>>>(@unsafe.Pointer(blockp))[..];
                foreach (var (i, v) in entry)
                {
                    if (v == 0L)
                    {
                        entry = entry[..i];
                        break;
                    }

                }
                if (len(entry) == 0L)
                {
                    break;
                }

                env = append(env, string(utf16.Decode(entry)));
                blockp += 2L * (uintptr(len(entry)) + 1L);

            }

            return (env, error.As(null!)!);

        });

        public static error Unsetenv(@string key)
        {
            return error.As(syscall.Unsetenv(key))!;
        }
    }
}}}}}}
