// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Constants that were deprecated or moved to enums in the FreeBSD headers. Keep
// them here for backwards compatibility.

// package unix -- go2cs converted at 2022 March 06 23:26:33 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\errors_freebsd_arm64.go


namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

public static readonly nuint DLT_HHDLC = 0x79;
public static readonly nuint IPV6_MIN_MEMBERSHIPS = 0x1f;
public static readonly nuint IP_MAX_SOURCE_FILTER = 0x400;
public static readonly nuint IP_MIN_MEMBERSHIPS = 0x1f;
public static readonly nuint RT_CACHING_CONTEXT = 0x1;
public static readonly nuint RT_NORTREF = 0x2;


} // end unix_package
