// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build plan9

// package filelock -- go2cs converted at 2020 October 09 05:45:48 UTC
// import "cmd/go/internal/lockedfile/internal/filelock" ==> using filelock = go.cmd.go.@internal.lockedfile.@internal.filelock_package
// Original source: C:\Go\src\cmd\go\internal\lockedfile\internal\filelock\filelock_plan9.go
using os = go.os_package;
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
        private partial struct lockType // : sbyte
        {
        }

        private static readonly var readLock = iota + 1L;
        private static readonly var writeLock = 0;


        private static error @lock(File f, lockType lt)
        {
            return error.As(addr(new os.PathError(Op:lt.String(),Path:f.Name(),Err:ErrNotSupported,))!)!;
        }

        private static error unlock(File f)
        {
            return error.As(addr(new os.PathError(Op:"Unlock",Path:f.Name(),Err:ErrNotSupported,))!)!;
        }

        private static bool isNotSupported(error err)
        {
            return err == ErrNotSupported;
        }
    }
}}}}}}
