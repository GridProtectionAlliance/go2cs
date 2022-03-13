// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !darwin && !dragonfly && !freebsd && !linux && !netbsd && !openbsd
// +build !darwin,!dragonfly,!freebsd,!linux,!netbsd,!openbsd

// package typecheck -- go2cs converted at 2022 March 13 05:59:58 UTC
// import "cmd/compile/internal/typecheck" ==> using typecheck = go.cmd.compile.@internal.typecheck_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\typecheck\mapfile_read.go
namespace go.cmd.compile.@internal;

using io = io_package;
using os = os_package;

public static partial class typecheck_package {

private static (@string, error) mapFile(ptr<os.File> _addr_f, long offset, long length) {
    @string _p0 = default;
    error _p0 = default!;
    ref os.File f = ref _addr_f.val;

    var buf = make_slice<byte>(length);
    var (_, err) = io.ReadFull(io.NewSectionReader(f, offset, length), buf);
    if (err != null) {
        return ("", error.As(err)!);
    }
    return (string(buf), error.As(null!)!);
}

} // end typecheck_package
