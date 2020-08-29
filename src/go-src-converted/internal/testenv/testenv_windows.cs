// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package testenv -- go2cs converted at 2020 August 29 10:11:08 UTC
// import "internal/testenv" ==> using testenv = go.@internal.testenv_package
// Original source: C:\Go\src\internal\testenv\testenv_windows.go
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class testenv_package
    {
        private static sync.Once symlinkOnce = default;
        private static error winSymlinkErr = default;

        private static void initWinHasSymlink() => func((defer, panic, _) =>
        {
            var (tmpdir, err) = ioutil.TempDir("", "symtest");
            if (err != null)
            {
                panic("failed to create temp directory: " + err.Error());
            }
            defer(os.RemoveAll(tmpdir));

            err = os.Symlink("target", filepath.Join(tmpdir, "symlink"));
            if (err != null)
            {
                err = err._<ref os.LinkError>().Err;

                if (err == syscall.EWINDOWS || err == syscall.ERROR_PRIVILEGE_NOT_HELD) 
                    winSymlinkErr = err;
                            }
        });

        private static (bool, @string) hasSymlink()
        {
            symlinkOnce.Do(initWinHasSymlink);


            if (winSymlinkErr == null) 
                return (true, "");
            else if (winSymlinkErr == syscall.EWINDOWS) 
                return (false, ": symlinks are not supported on your version of Windows");
            else if (winSymlinkErr == syscall.ERROR_PRIVILEGE_NOT_HELD) 
                return (false, ": you don't have enough privileges to create symlinks");
                        return (false, "");
        }

        public static bool IsWindowsXP() => func((_, panic, __) =>
        {
            var (v, err) = syscall.GetVersion();
            if (err != null)
            {
                panic("GetVersion failed: " + err.Error());
            }
            var major = byte(v);
            return major < 6L;
        });
    }
}}
