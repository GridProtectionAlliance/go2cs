// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 August 29 08:36:50 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\errors_plan9.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // Constants
 
        // Invented values to support what package os expects.
        public static readonly ulong O_CREAT = 0x02000UL;
        public static readonly ulong O_APPEND = 0x00400UL;
        public static readonly ulong O_NOCTTY = 0x00000UL;
        public static readonly ulong O_NONBLOCK = 0x00000UL;
        public static readonly ulong O_SYNC = 0x00000UL;
        public static readonly ulong O_ASYNC = 0x00000UL;

        public static readonly ulong S_IFMT = 0x1f000UL;
        public static readonly ulong S_IFIFO = 0x1000UL;
        public static readonly ulong S_IFCHR = 0x2000UL;
        public static readonly ulong S_IFDIR = 0x4000UL;
        public static readonly ulong S_IFBLK = 0x6000UL;
        public static readonly ulong S_IFREG = 0x8000UL;
        public static readonly ulong S_IFLNK = 0xa000UL;
        public static readonly ulong S_IFSOCK = 0xc000UL;

        // Errors
        public static var EINVAL = NewError("bad arg in system call");        public static var ENOTDIR = NewError("not a directory");        public static var EISDIR = NewError("file is a directory");        public static var ENOENT = NewError("file does not exist");        public static var EEXIST = NewError("file already exists");        public static var EMFILE = NewError("no free file descriptors");        public static var EIO = NewError("i/o error");        public static var ENAMETOOLONG = NewError("file name too long");        public static var EINTR = NewError("interrupted");        public static var EPERM = NewError("permission denied");        public static var EBUSY = NewError("no free devices");        public static var ETIMEDOUT = NewError("connection timed out");        public static var EPLAN9 = NewError("not supported by plan 9");        public static var EACCES = NewError("access permission denied");        public static var EAFNOSUPPORT = NewError("address family not supported by protocol");        public static var ESPIPE = NewError("illegal seek");

        // Notes
        public static readonly var SIGABRT = Note("abort");
        public static readonly var SIGALRM = Note("alarm");
        public static readonly var SIGHUP = Note("hangup");
        public static readonly var SIGINT = Note("interrupt");
        public static readonly var SIGKILL = Note("kill");
        public static readonly var SIGTERM = Note("interrupt");
    }
}
