// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// Package unix contains an interface to the low-level operating system
// primitives. OS details vary depending on the underlying system, and
// by default, godoc will display OS-specific documentation for the current
// system. If you want godoc to display OS documentation for another
// system, set $GOOS and $GOARCH to the desired system. For example, if
// you want to view documentation for freebsd/arm on linux/amd64, set $GOOS
// to freebsd and $GOARCH to arm.
//
// The primary use of this package is inside other packages that provide a more
// portable interface to the system, such as "os", "time" and "net".  Use
// those packages rather than this one if you can.
//
// For details of the functions and data types in this package consult
// the manuals for the appropriate operating system.
//
// These calls return err == nil to indicate success; otherwise
// err represents an operating system error describing the failure and
// holds a value of type syscall.Errno.
// package unix -- go2cs converted at 2020 October 09 05:56:21 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall.go
// import "golang.org/x/sys/unix"

using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class unix_package
    {
        // ByteSliceFromString returns a NUL-terminated slice of bytes
        // containing the text of s. If s contains a NUL byte at any
        // location, it returns (nil, EINVAL).
        public static (slice<byte>, error) ByteSliceFromString(@string s)
        {
            slice<byte> _p0 = default;
            error _p0 = default!;

            if (strings.IndexByte(s, 0L) != -1L)
            {
                return (null, error.As(EINVAL)!);
            }
            var a = make_slice<byte>(len(s) + 1L);
            copy(a, s);
            return (a, error.As(null!)!);

        }

        // BytePtrFromString returns a pointer to a NUL-terminated array of
        // bytes containing the text of s. If s contains a NUL byte at any
        // location, it returns (nil, EINVAL).
        public static (ptr<byte>, error) BytePtrFromString(@string s)
        {
            ptr<byte> _p0 = default!;
            error _p0 = default!;

            var (a, err) = ByteSliceFromString(s);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (_addr__addr_a[0L]!, error.As(null!)!);

        }

        // Single-word zero for use when we need a valid pointer to 0 bytes.
        private static System.UIntPtr _zero = default;
    }
}}}}}}
