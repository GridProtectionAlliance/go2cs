// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !darwin,!linux

// package ld -- go2cs converted at 2020 October 09 05:50:14 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\outbuf_nofallocate.go
using errors = go.errors_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        private static error fallocate(this ptr<OutBuf> _addr_@out, ulong size)
        {
            ref OutBuf @out = ref _addr_@out.val;

            return error.As(errors.New(fallocateNotSupportedErr))!;
        }
    }
}}}}
