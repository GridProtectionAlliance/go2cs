// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package windows -- go2cs converted at 2020 October 09 06:00:50 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\windows\memory_windows.go

using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class windows_package
    {
        public static readonly ulong MEM_COMMIT = (ulong)0x00001000UL;
        public static readonly ulong MEM_RESERVE = (ulong)0x00002000UL;
        public static readonly ulong MEM_DECOMMIT = (ulong)0x00004000UL;
        public static readonly ulong MEM_RELEASE = (ulong)0x00008000UL;
        public static readonly ulong MEM_RESET = (ulong)0x00080000UL;
        public static readonly ulong MEM_TOP_DOWN = (ulong)0x00100000UL;
        public static readonly ulong MEM_WRITE_WATCH = (ulong)0x00200000UL;
        public static readonly ulong MEM_PHYSICAL = (ulong)0x00400000UL;
        public static readonly ulong MEM_RESET_UNDO = (ulong)0x01000000UL;
        public static readonly ulong MEM_LARGE_PAGES = (ulong)0x20000000UL;

        public static readonly ulong PAGE_NOACCESS = (ulong)0x01UL;
        public static readonly ulong PAGE_READONLY = (ulong)0x02UL;
        public static readonly ulong PAGE_READWRITE = (ulong)0x04UL;
        public static readonly ulong PAGE_WRITECOPY = (ulong)0x08UL;
        public static readonly ulong PAGE_EXECUTE_READ = (ulong)0x20UL;
        public static readonly ulong PAGE_EXECUTE_READWRITE = (ulong)0x40UL;
        public static readonly ulong PAGE_EXECUTE_WRITECOPY = (ulong)0x80UL;

    }
}}}}}}
