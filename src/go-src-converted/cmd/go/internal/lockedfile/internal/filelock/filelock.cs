// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package filelock provides a platform-independent API for advisory file
// locking. Calls to functions in this package on platforms that do not support
// advisory locks will return errors for which IsNotSupported returns true.
// package filelock -- go2cs converted at 2020 October 08 04:34:18 UTC
// import "cmd/go/internal/lockedfile/internal/filelock" ==> using filelock = go.cmd.go.@internal.lockedfile.@internal.filelock_package
// Original source: C:\Go\src\cmd\go\internal\lockedfile\internal\filelock\filelock.go
using errors = go.errors_package;
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
        // A File provides the minimal set of methods required to lock an open file.
        // File implementations must be usable as map keys.
        // The usual implementation is *os.File.
        public partial interface File
        {
            (os.FileInfo, error) Name(); // Fd returns a valid file descriptor.
// (If the File is an *os.File, it must not be closed.)
            (os.FileInfo, error) Fd(); // Stat returns the FileInfo structure describing file.
            (os.FileInfo, error) Stat();
        }

        // Lock places an advisory write lock on the file, blocking until it can be
        // locked.
        //
        // If Lock returns nil, no other process will be able to place a read or write
        // lock on the file until this process exits, closes f, or calls Unlock on it.
        //
        // If f's descriptor is already read- or write-locked, the behavior of Lock is
        // unspecified.
        //
        // Closing the file may or may not release the lock promptly. Callers should
        // ensure that Unlock is always called when Lock succeeds.
        public static error Lock(File f)
        {
            return error.As(lock(f, writeLock))!;
        }

        // RLock places an advisory read lock on the file, blocking until it can be locked.
        //
        // If RLock returns nil, no other process will be able to place a write lock on
        // the file until this process exits, closes f, or calls Unlock on it.
        //
        // If f is already read- or write-locked, the behavior of RLock is unspecified.
        //
        // Closing the file may or may not release the lock promptly. Callers should
        // ensure that Unlock is always called if RLock succeeds.
        public static error RLock(File f)
        {
            return error.As(lock(f, readLock))!;
        }

        // Unlock removes an advisory lock placed on f by this process.
        //
        // The caller must not attempt to unlock a file that is not locked.
        public static error Unlock(File f)
        {
            return error.As(unlock(f))!;
        }

        // String returns the name of the function corresponding to lt
        // (Lock, RLock, or Unlock).
        private static @string String(this lockType lt)
        {

            if (lt == readLock) 
                return "RLock";
            else if (lt == writeLock) 
                return "Lock";
            else 
                return "Unlock";
            
        }

        // IsNotSupported returns a boolean indicating whether the error is known to
        // report that a function is not supported (possibly for a specific input).
        // It is satisfied by ErrNotSupported as well as some syscall errors.
        public static bool IsNotSupported(error err)
        {
            return isNotSupported(underlyingError(err));
        }

        public static var ErrNotSupported = errors.New("operation not supported");

        // underlyingError returns the underlying error for known os error types.
        private static error underlyingError(error err)
        {
            switch (err.type())
            {
                case ptr<os.PathError> err:
                    return error.As(err.Err)!;
                    break;
                case ptr<os.LinkError> err:
                    return error.As(err.Err)!;
                    break;
                case ptr<os.SyscallError> err:
                    return error.As(err.Err)!;
                    break;
            }
            return error.As(err)!;

        }
    }
}}}}}}
