// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd

// package gc -- go2cs converted at 2020 October 08 04:29:27 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\mapfile_mmap.go
using os = go.os_package;
using reflect = go.reflect_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // TODO(mdempsky): Is there a higher-level abstraction that still
        // works well for iimport?

        // mapFile returns length bytes from the file starting at the
        // specified offset as a string.
        private static (@string, error) mapFile(ptr<os.File> _addr_f, long offset, long length)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref os.File f = ref _addr_f.val;
 
            // POSIX mmap: "The implementation may require that off is a
            // multiple of the page size."
            var x = offset & int64(os.Getpagesize() - 1L);
            offset -= x;
            length += x;

            var (buf, err) = syscall.Mmap(int(f.Fd()), offset, int(length), syscall.PROT_READ, syscall.MAP_SHARED);
            keepAlive(f);
            if (err != null)
            {
                return ("", error.As(err)!);
            }
            buf = buf[x..];
            var pSlice = (reflect.SliceHeader.val)(@unsafe.Pointer(_addr_buf));

            ref @string res = ref heap(out ptr<@string> _addr_res);
            var pString = (reflect.StringHeader.val)(@unsafe.Pointer(_addr_res));

            pString.Data = pSlice.Data;
            pString.Len = pSlice.Len;

            return (res, error.As(null!)!);

        }

        // keepAlive is a reimplementation of runtime.KeepAlive, which wasn't
        // added until Go 1.7, whereas we need to compile with Go 1.4.
        private static Action<object> keepAlive = _p0 =>
        {
        };
    }
}}}}
