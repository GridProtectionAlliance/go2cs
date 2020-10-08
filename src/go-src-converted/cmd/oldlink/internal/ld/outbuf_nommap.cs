// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !darwin,!dragonfly,!freebsd,!linux,!openbsd,!windows

// package ld -- go2cs converted at 2020 October 08 04:41:45 UTC
// import "cmd/oldlink/internal/ld" ==> using ld = go.cmd.oldlink.@internal.ld_package
// Original source: C:\Go\src\cmd\oldlink\internal\ld\outbuf_nommap.go
using errors = go.errors_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class ld_package
    {
        private static var errNotSupported = errors.New("mmap not supported");

        private static error Mmap(this ptr<OutBuf> _addr_@out, ulong filesize)
        {
            ref OutBuf @out = ref _addr_@out.val;

            return error.As(errNotSupported)!;
        }
        private static void Munmap(this ptr<OutBuf> _addr_@out) => func((_, panic, __) =>
        {
            ref OutBuf @out = ref _addr_@out.val;

            panic("unreachable");
        });
        private static error Msync(this ptr<OutBuf> _addr_@out) => func((_, panic, __) =>
        {
            ref OutBuf @out = ref _addr_@out.val;

            panic("unreachable");
        });
    }
}}}}
