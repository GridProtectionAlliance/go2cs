// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !darwin,!dragonfly,!freebsd,!linux,!netbsd,!openbsd

// package gc -- go2cs converted at 2020 October 09 05:41:54 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\mapfile_read.go
using io = go.io_package;
using os = go.os_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        private static (@string, error) mapFile(ptr<os.File> _addr_f, long offset, long length)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref os.File f = ref _addr_f.val;

            var buf = make_slice<byte>(length);
            var (_, err) = io.ReadFull(io.NewSectionReader(f, offset, length), buf);
            if (err != null)
            {
                return ("", error.As(err)!);
            }
            return (string(buf), error.As(null!)!);

        }
    }
}}}}
