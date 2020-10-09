// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Constants that were deprecated or moved to enums in the FreeBSD headers. Keep
// them here for backwards compatibility.

// package unix -- go2cs converted at 2020 October 09 05:56:15 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\errors_freebsd_arm64.go

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
        public static readonly ulong DLT_HHDLC = (ulong)0x79UL;
        public static readonly ulong IPV6_MIN_MEMBERSHIPS = (ulong)0x1fUL;
        public static readonly ulong IP_MAX_SOURCE_FILTER = (ulong)0x400UL;
        public static readonly ulong IP_MIN_MEMBERSHIPS = (ulong)0x1fUL;
        public static readonly ulong RT_CACHING_CONTEXT = (ulong)0x1UL;
        public static readonly ulong RT_NORTREF = (ulong)0x2UL;

    }
}}}}}}
