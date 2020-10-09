// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd netbsd openbsd

// package terminal -- go2cs converted at 2020 October 09 05:55:48 UTC
// import "cmd/vendor/golang.org/x/crypto/ssh/terminal" ==> using terminal = go.cmd.vendor.golang.org.x.crypto.ssh.terminal_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\crypto\ssh\terminal\util_bsd.go
using unix = go.golang.org.x.sys.unix_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace crypto {
namespace ssh
{
    public static partial class terminal_package
    {
        private static readonly var ioctlReadTermios = unix.TIOCGETA;

        private static readonly var ioctlWriteTermios = unix.TIOCSETA;

    }
}}}}}}}
