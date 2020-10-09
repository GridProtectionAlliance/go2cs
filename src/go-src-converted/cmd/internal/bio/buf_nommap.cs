// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !darwin,!dragonfly,!freebsd,!linux,!netbsd,!openbsd

// package bio -- go2cs converted at 2020 October 09 05:08:51 UTC
// import "cmd/internal/bio" ==> using bio = go.cmd.@internal.bio_package
// Original source: C:\Go\src\cmd\internal\bio\buf_nommap.go

using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class bio_package
    {
        private static (slice<byte>, bool) sliceOS(this ptr<Reader> _addr_r, ulong length)
        {
            slice<byte> _p0 = default;
            bool _p0 = default;
            ref Reader r = ref _addr_r.val;

            return (null, false);
        }
    }
}}}
