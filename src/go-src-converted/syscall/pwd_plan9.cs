// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The working directory in Plan 9 is effectively per P, so different
// goroutines and even the same goroutine as it's rescheduled on
// different Ps can see different working directories.
//
// Instead, track a Go process-wide intent of the current working directory,
// and switch to it at important points.

// package syscall -- go2cs converted at 2020 August 29 08:37:24 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\pwd_plan9.go
using sync = go.sync_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static sync.Mutex wdmu = default;        private static bool wdSet = default;        private static @string wdStr = default;

        public static void Fixwd() => func((defer, _, __) =>
        {
            wdmu.Lock();
            defer(wdmu.Unlock());
            fixwdLocked();
        });

        private static void fixwdLocked()
        {
            if (!wdSet)
            {
                return;
            } 
            // always call chdir when getwd returns an error
            var (wd, _) = getwd();
            if (wd == wdStr)
            {
                return;
            }
            {
                var err = chdir(wdStr);

                if (err != null)
                {
                    return;
                }

            }
        }

        // goroutine-specific getwd
        private static (@string, error) getwd() => func((defer, _, __) =>
        {
            var (fd, err) = open(".", O_RDONLY);
            if (err != null)
            {
                return ("", err);
            }
            defer(Close(fd));
            return Fd2path(fd);
        });

        public static (@string, error) Getwd() => func((defer, _, __) =>
        {
            wdmu.Lock();
            defer(wdmu.Unlock());

            if (wdSet)
            {
                return (wdStr, null);
            }
            wd, err = getwd();
            if (err != null)
            {
                return;
            }
            wdSet = true;
            wdStr = wd;
            return (wd, null);
        });

        public static error Chdir(@string path) => func((defer, _, __) =>
        {
            wdmu.Lock();
            defer(wdmu.Unlock());

            {
                var err = chdir(path);

                if (err != null)
                {
                    return error.As(err);
                }

            }

            var (wd, err) = getwd();
            if (err != null)
            {
                return error.As(err);
            }
            wdSet = true;
            wdStr = wd;
            return error.As(null);
        });
    }
}
