// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin || freebsd || netbsd
// +build darwin freebsd netbsd

// package tar -- go2cs converted at 2022 March 06 22:31:31 UTC
// import "archive/tar" ==> using tar = go.archive.tar_package
// Original source: C:\Program Files\Go\src\archive\tar\stat_actime2.go
using syscall = go.syscall_package;
using time = go.time_package;

namespace go.archive;

public static partial class tar_package {

private static time.Time statAtime(ptr<syscall.Stat_t> _addr_st) {
    ref syscall.Stat_t st = ref _addr_st.val;

    return time.Unix(st.Atimespec.Unix());
}

private static time.Time statCtime(ptr<syscall.Stat_t> _addr_st) {
    ref syscall.Stat_t st = ref _addr_st.val;

    return time.Unix(st.Ctimespec.Unix());
}

} // end tar_package
