// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd

// package filelock -- go2cs converted at 2020 October 09 05:45:48 UTC
// import "cmd/go/internal/lockedfile/internal/filelock" ==> using filelock = go.cmd.go.@internal.lockedfile.@internal.filelock_package
// Original source: C:\Go\src\cmd\go\internal\lockedfile\internal\filelock\filelock_unix.go
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal {
namespace lockedfile {
namespace @internal
{
    public static partial class filelock_package
    {
        private partial struct lockType // : short
        {
        }

        private static readonly lockType readLock = (lockType)syscall.LOCK_SH;
        private static readonly lockType writeLock = (lockType)syscall.LOCK_EX;


        private static error @lock(File f, lockType lt)
        {
            error err = default!;

            while (true)
            {
                err = syscall.Flock(int(f.Fd()), int(lt));
                if (err != syscall.EINTR)
                {
                    break;
                }

            }

            if (err != null)
            {
                return error.As(addr(new os.PathError(Op:lt.String(),Path:f.Name(),Err:err,))!)!;
            }

            return error.As(null!)!;

        }

        private static error unlock(File f)
        {
            return error.As(lock(f, syscall.LOCK_UN))!;
        }

        private static bool isNotSupported(error err)
        {
            return err == syscall.ENOSYS || err == syscall.ENOTSUP || err == syscall.EOPNOTSUPP || err == ErrNotSupported;
        }
    }
}}}}}}
