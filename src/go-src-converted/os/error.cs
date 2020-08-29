// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 August 29 08:43:35 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\error.go
using errors = go.errors_package;
using poll = go.@internal.poll_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // Portable analogs of some common system call errors.
        public static var ErrInvalid = errors.New("invalid argument");        public static var ErrPermission = errors.New("permission denied");        public static var ErrExist = errors.New("file already exists");        public static var ErrNotExist = errors.New("file does not exist");        public static var ErrClosed = errors.New("file already closed");        public static var ErrNoDeadline = poll.ErrNoDeadline;

        private partial interface timeout
        {
            bool Timeout();
        }

        // PathError records an error and the operation and file path that caused it.
        public partial struct PathError
        {
            public @string Op;
            public @string Path;
            public error Err;
        }

        private static @string Error(this ref PathError e)
        {
            return e.Op + " " + e.Path + ": " + e.Err.Error();
        }

        // Timeout reports whether this error represents a timeout.
        private static bool Timeout(this ref PathError e)
        {
            timeout (t, ok) = e.Err._<timeout>();
            return ok && t.Timeout();
        }

        // SyscallError records an error from a specific system call.
        public partial struct SyscallError
        {
            public @string Syscall;
            public error Err;
        }

        private static @string Error(this ref SyscallError e)
        {
            return e.Syscall + ": " + e.Err.Error();
        }

        // Timeout reports whether this error represents a timeout.
        private static bool Timeout(this ref SyscallError e)
        {
            timeout (t, ok) = e.Err._<timeout>();
            return ok && t.Timeout();
        }

        // NewSyscallError returns, as an error, a new SyscallError
        // with the given system call name and error details.
        // As a convenience, if err is nil, NewSyscallError returns nil.
        public static error NewSyscallError(@string syscall, error err)
        {
            if (err == null)
            {
                return error.As(null);
            }
            return error.As(ref new SyscallError(syscall,err));
        }

        // IsExist returns a boolean indicating whether the error is known to report
        // that a file or directory already exists. It is satisfied by ErrExist as
        // well as some syscall errors.
        public static bool IsExist(error err)
        {
            return isExist(err);
        }

        // IsNotExist returns a boolean indicating whether the error is known to
        // report that a file or directory does not exist. It is satisfied by
        // ErrNotExist as well as some syscall errors.
        public static bool IsNotExist(error err)
        {
            return isNotExist(err);
        }

        // IsPermission returns a boolean indicating whether the error is known to
        // report that permission is denied. It is satisfied by ErrPermission as well
        // as some syscall errors.
        public static bool IsPermission(error err)
        {
            return isPermission(err);
        }

        // IsTimeout returns a boolean indicating whether the error is known
        // to report that a timeout occurred.
        public static bool IsTimeout(error err)
        {
            timeout (terr, ok) = underlyingError(err)._<timeout>();
            return ok && terr.Timeout();
        }

        // underlyingError returns the underlying error for known os error types.
        private static error underlyingError(error err)
        {
            switch (err.type())
            {
                case ref PathError err:
                    return error.As(err.Err);
                    break;
                case ref LinkError err:
                    return error.As(err.Err);
                    break;
                case ref SyscallError err:
                    return error.As(err.Err);
                    break;
            }
            return error.As(err);
        }
    }
}
