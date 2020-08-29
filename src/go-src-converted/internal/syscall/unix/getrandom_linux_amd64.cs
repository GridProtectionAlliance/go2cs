// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2020 August 29 08:30:51 UTC
// import "internal/syscall/unix" ==> using unix = go.@internal.syscall.unix_package
// Original source: C:\Go\src\internal\syscall\unix\getrandom_linux_amd64.go

using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class unix_package
    {
        // Linux getrandom system call number.
        // See GetRandom in getrandom_linux.go.
        private static readonly System.UIntPtr randomTrap = 318L;

    }
}}}
