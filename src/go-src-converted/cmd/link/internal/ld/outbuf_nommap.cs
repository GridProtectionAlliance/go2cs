// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !darwin,!dragonfly,!freebsd,!linux,!openbsd,!windows

// package ld -- go2cs converted at 2020 October 08 04:39:25 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\outbuf_nommap.go

using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        private static error Mmap(this ptr<OutBuf> _addr_@out, ulong filesize)
        {
            ref OutBuf @out = ref _addr_@out.val;
 
            // We need space to put all the symbols before we apply relocations.
            @out.heap = make_slice<byte>(filesize);
            return error.As(null!)!;

        }

        private static void munmap(this ptr<OutBuf> _addr_@out) => func((_, panic, __) =>
        {
            ref OutBuf @out = ref _addr_@out.val;

            panic("unreachable");
        });
    }
}}}}
