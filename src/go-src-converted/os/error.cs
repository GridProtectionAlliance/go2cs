// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package os -- go2cs converted at 2020 October 08 03:44:20 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\error.go
using oserror = go.@internal.oserror_package;
using poll = go.@internal.poll_package;
using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // Portable analogs of some common system call errors.
        //
        // Errors returned from this package may be tested against these errors
        // with errors.Is.
 
        // ErrInvalid indicates an invalid argument.
        // Methods on File will return this error when the receiver is nil.
        public static var ErrInvalid = errInvalid();        public static var ErrPermission = errPermission();        public static var ErrExist = errExist();        public static var ErrNotExist = errNotExist();        public static var ErrClosed = errClosed();        public static var ErrNoDeadline = errNoDeadline();        public static var ErrDeadlineExceeded = errDeadlineExceeded();

        private static error errInvalid()
        {
            return error.As(oserror.ErrInvalid)!;
        }
        private static error errPermission()
        {
            return error.As(oserror.ErrPermission)!;
        }
        private static error errExist()
        {
            return error.As(oserror.ErrExist)!;
        }
        private static error errNotExist()
        {
            return error.As(oserror.ErrNotExist)!;
        }
        private static error errClosed()
        {
            return error.As(oserror.ErrClosed)!;
        }
        private static error errNoDeadline()
        {
            return error.As(poll.ErrNoDeadline)!;
        }

        // errDeadlineExceeded returns the value for os.ErrDeadlineExceeded.
        // This error comes from the internal/poll package, which is also
        // used by package net. Doing this this way ensures that the net
        // package will return os.ErrDeadlineExceeded for an exceeded deadline,
        // as documented by net.Conn.SetDeadline, without requiring any extra
        // work in the net package and without requiring the internal/poll
        // package to import os (which it can't, because that would be circular).
        private static error errDeadlineExceeded()
        {
            return error.As(poll.ErrDeadlineExceeded)!;
        }

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

        private static @string Error(this ptr<PathError> _addr_e)
        {
            ref PathError e = ref _addr_e.val;

            return e.Op + " " + e.Path + ": " + e.Err.Error();
        }

        private static error Unwrap(this ptr<PathError> _addr_e)
        {
            ref PathError e = ref _addr_e.val;

            return error.As(e.Err)!;
        }

        // Timeout reports whether this error represents a timeout.
        private static bool Timeout(this ptr<PathError> _addr_e)
        {
            ref PathError e = ref _addr_e.val;

            timeout (t, ok) = timeout.As(e.Err._<timeout>())!;
            return ok && t.Timeout();
        }

        // SyscallError records an error from a specific system call.
        public partial struct SyscallError
        {
            public @string Syscall;
            public error Err;
        }

        private static @string Error(this ptr<SyscallError> _addr_e)
        {
            ref SyscallError e = ref _addr_e.val;

            return e.Syscall + ": " + e.Err.Error();
        }

        private static error Unwrap(this ptr<SyscallError> _addr_e)
        {
            ref SyscallError e = ref _addr_e.val;

            return error.As(e.Err)!;
        }

        // Timeout reports whether this error represents a timeout.
        private static bool Timeout(this ptr<SyscallError> _addr_e)
        {
            ref SyscallError e = ref _addr_e.val;

            timeout (t, ok) = timeout.As(e.Err._<timeout>())!;
            return ok && t.Timeout();
        }

        // NewSyscallError returns, as an error, a new SyscallError
        // with the given system call name and error details.
        // As a convenience, if err is nil, NewSyscallError returns nil.
        public static error NewSyscallError(@string syscall, error err)
        {
            if (err == null)
            {
                return error.As(null!)!;
            }

            return error.As(addr(new SyscallError(syscall,err))!)!;

        }

        // IsExist returns a boolean indicating whether the error is known to report
        // that a file or directory already exists. It is satisfied by ErrExist as
        // well as some syscall errors.
        public static bool IsExist(error err)
        {
            return underlyingErrorIs(err, ErrExist);
        }

        // IsNotExist returns a boolean indicating whether the error is known to
        // report that a file or directory does not exist. It is satisfied by
        // ErrNotExist as well as some syscall errors.
        public static bool IsNotExist(error err)
        {
            return underlyingErrorIs(err, ErrNotExist);
        }

        // IsPermission returns a boolean indicating whether the error is known to
        // report that permission is denied. It is satisfied by ErrPermission as well
        // as some syscall errors.
        public static bool IsPermission(error err)
        {
            return underlyingErrorIs(err, ErrPermission);
        }

        // IsTimeout returns a boolean indicating whether the error is known
        // to report that a timeout occurred.
        public static bool IsTimeout(error err)
        {
            timeout (terr, ok) = timeout.As(underlyingError(err)._<timeout>())!;
            return ok && terr.Timeout();
        }

        private static bool underlyingErrorIs(error err, error target)
        { 
            // Note that this function is not errors.Is:
            // underlyingError only unwraps the specific error-wrapping types
            // that it historically did, not all errors implementing Unwrap().
            err = underlyingError(err);
            if (err == target)
            {
                return true;
            } 
            // To preserve prior behavior, only examine syscall errors.
            syscallErrorType (e, ok) = err._<syscallErrorType>();
            return ok && e.Is(target);

        }

        // underlyingError returns the underlying error for known os error types.
        private static error underlyingError(error err)
        {
            switch (err.type())
            {
                case ptr<PathError> err:
                    return error.As(err.Err)!;
                    break;
                case ptr<LinkError> err:
                    return error.As(err.Err)!;
                    break;
                case ptr<SyscallError> err:
                    return error.As(err.Err)!;
                    break;
            }
            return error.As(err)!;

        }
    }
}
