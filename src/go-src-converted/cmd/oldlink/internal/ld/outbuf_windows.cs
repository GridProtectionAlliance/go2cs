// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 October 09 05:52:24 UTC
// import "cmd/oldlink/internal/ld" ==> using ld = go.cmd.oldlink.@internal.ld_package
// Original source: C:\Go\src\cmd\oldlink\internal\ld\outbuf_windows.go
using reflect = go.reflect_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class ld_package
    {
        private static error Mmap(this ptr<OutBuf> _addr_@out, ulong filesize) => func((defer, _, __) =>
        {
            ref OutBuf @out = ref _addr_@out.val;

            var err = @out.f.Truncate(int64(filesize));
            if (err != null)
            {
                Exitf("resize output file failed: %v", err);
            }
            var low = uint32(filesize);
            var high = uint32(filesize >> (int)(32L));
            var (fmap, err) = syscall.CreateFileMapping(syscall.Handle(@out.f.Fd()), null, syscall.PAGE_READONLY, high, low, null);
            if (err != null)
            {
                return error.As(err)!;
            }
            defer(syscall.CloseHandle(fmap));

            var (ptr, err) = syscall.MapViewOfFile(fmap, syscall.FILE_MAP_READ | syscall.FILE_MAP_WRITE, 0L, 0L, uintptr(filesize));
            if (err != null)
            {
                return error.As(err)!;
            }
            (reflect.SliceHeader.val)(@unsafe.Pointer(_addr_@out.buf)).val;

            new reflect.SliceHeader(Data:ptr,Len:int(filesize),Cap:int(filesize));
            return error.As(null!)!;

        });

        private static void Munmap(this ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;

            if (@out.buf == null)
            {
                return ;
            }

            var err = syscall.UnmapViewOfFile(uintptr(@unsafe.Pointer(_addr_@out.buf[0L])));
            if (err != null)
            {
                Exitf("UnmapViewOfFile failed: %v", err);
            }

        }

        private static error Msync(this ptr<OutBuf> _addr_@out)
        {
            ref OutBuf @out = ref _addr_@out.val;

            if (@out.buf == null)
            {
                return error.As(null!)!;
            }

            return error.As(syscall.FlushViewOfFile(uintptr(@unsafe.Pointer(_addr_@out.buf[0L])), 0L))!;

        }
    }
}}}}
