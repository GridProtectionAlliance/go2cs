// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2022 March 06 22:26:26 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\errors_plan9.go


namespace go;

public static partial class syscall_package {

    // Constants
 
// Invented values to support what package os expects.
public static readonly nuint O_CREAT = 0x02000;
public static readonly nuint O_APPEND = 0x00400;
public static readonly nuint O_NOCTTY = 0x00000;
public static readonly nuint O_NONBLOCK = 0x00000;
public static readonly nuint O_SYNC = 0x00000;
public static readonly nuint O_ASYNC = 0x00000;

public static readonly nuint S_IFMT = 0x1f000;
public static readonly nuint S_IFIFO = 0x1000;
public static readonly nuint S_IFCHR = 0x2000;
public static readonly nuint S_IFDIR = 0x4000;
public static readonly nuint S_IFBLK = 0x6000;
public static readonly nuint S_IFREG = 0x8000;
public static readonly nuint S_IFLNK = 0xa000;
public static readonly nuint S_IFSOCK = 0xc000;


// Errors
public static var EINVAL = NewError("bad arg in system call");public static var ENOTDIR = NewError("not a directory");public static var EISDIR = NewError("file is a directory");public static var ENOENT = NewError("file does not exist");public static var EEXIST = NewError("file already exists");public static var EMFILE = NewError("no free file descriptors");public static var EIO = NewError("i/o error");public static var ENAMETOOLONG = NewError("file name too long");public static var EINTR = NewError("interrupted");public static var EPERM = NewError("permission denied");public static var EBUSY = NewError("no free devices");public static var ETIMEDOUT = NewError("connection timed out");public static var EPLAN9 = NewError("not supported by plan 9");public static var EACCES = NewError("access permission denied");public static var EAFNOSUPPORT = NewError("address family not supported by protocol");public static var ESPIPE = NewError("illegal seek");

// Notes
public static readonly var SIGABRT = Note("abort");
public static readonly var SIGALRM = Note("alarm");
public static readonly var SIGHUP = Note("hangup");
public static readonly var SIGINT = Note("interrupt");
public static readonly var SIGKILL = Note("kill");
public static readonly var SIGTERM = Note("interrupt");


} // end syscall_package
