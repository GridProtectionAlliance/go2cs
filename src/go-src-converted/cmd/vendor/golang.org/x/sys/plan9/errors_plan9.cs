// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package plan9 -- go2cs converted at 2022 March 06 23:26:24 UTC
// import "cmd/vendor/golang.org/x/sys/plan9" ==> using plan9 = go.cmd.vendor.golang.org.x.sys.plan9_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\plan9\errors_plan9.go
using syscall = go.syscall_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class plan9_package {

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
public static var EINVAL = syscall.NewError("bad arg in system call");public static var ENOTDIR = syscall.NewError("not a directory");public static var EISDIR = syscall.NewError("file is a directory");public static var ENOENT = syscall.NewError("file does not exist");public static var EEXIST = syscall.NewError("file already exists");public static var EMFILE = syscall.NewError("no free file descriptors");public static var EIO = syscall.NewError("i/o error");public static var ENAMETOOLONG = syscall.NewError("file name too long");public static var EINTR = syscall.NewError("interrupted");public static var EPERM = syscall.NewError("permission denied");public static var EBUSY = syscall.NewError("no free devices");public static var ETIMEDOUT = syscall.NewError("connection timed out");public static var EPLAN9 = syscall.NewError("not supported by plan 9");public static var EACCES = syscall.NewError("access permission denied");public static var EAFNOSUPPORT = syscall.NewError("address family not supported by protocol");

} // end plan9_package
