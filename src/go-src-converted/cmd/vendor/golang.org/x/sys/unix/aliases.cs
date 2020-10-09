// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris
// +build go1.9

// package unix -- go2cs converted at 2020 October 09 05:56:11 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\aliases.go
using syscall = go.syscall_package;
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
        public partial struct Signal // : syscall.Signal
        {
        }
        public partial struct Errno // : syscall.Errno
        {
        }
        public partial struct SysProcAttr // : syscall.SysProcAttr
        {
        }
    }
}}}}}}
