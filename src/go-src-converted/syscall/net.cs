// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 August 29 08:37:17 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\net.go

using static go.builtin;
using System;

namespace go
{
    public static partial class syscall_package
    {
        // A RawConn is a raw network connection.
        public partial interface RawConn
        {
            error Control(Action<System.UIntPtr> f); // Read invokes f on the underlying connection's file
// descriptor or handle; f is expected to try to read from the
// file descriptor.
// If f returns true, Read returns. Otherwise Read blocks
// waiting for the connection to be ready for reading and
// tries again repeatedly.
// The file descriptor is guaranteed to remain valid while f
// executes but not after f returns.
            error Read(Func<System.UIntPtr, bool> f); // Write is like Read but for writing.
            error Write(Func<System.UIntPtr, bool> f);
        }

        // Conn is implemented by some types in the net package to provide
        // access to the underlying file descriptor or handle.
        public partial interface Conn
        {
            (RawConn, error) SyscallConn();
        }
    }
}
